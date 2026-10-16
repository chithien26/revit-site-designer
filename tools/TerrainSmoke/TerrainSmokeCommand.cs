using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EPCommon;
#if !REVIT2022 && !REVIT2023
using TerrainSurface = EPCommon.ToposolidSurface;
using EditScope = EPCommon.TerrainEditScope;
#else
using TerrainSurface = Autodesk.Revit.DB.Architecture.TopographySurface;
using EditScope = Autodesk.Revit.DB.Architecture.TopographyEditScope;
#endif

namespace TerrainSmoke;

[Transaction(TransactionMode.Manual)]
public class TerrainSmokeCommand : IExternalCommand
{
    private const string AssociationParameter="UniqueID";
    private sealed class TestFailures : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor accessor)
        {
            if(accessor.GetFailureMessages().Any(x=>x.GetSeverity()==FailureSeverity.Error)) return FailureProcessingResult.ProceedWithRollBack;
            accessor.DeleteAllWarnings(); return FailureProcessingResult.Continue;
        }
    }
    private static void Start(Transaction transaction)
    {
        transaction.Start();
        transaction.SetFailureHandlingOptions(transaction.GetFailureHandlingOptions().SetClearAfterRollback(true).SetFailuresPreprocessor(new TestFailures()));
    }
    private readonly List<string> checks = new List<string>();
    private string report;
    private void Check(bool condition,string name)
    {
        if(!condition)throw new InvalidOperationException("FAIL: "+name);
        checks.Add("PASS: "+name); File.WriteAllLines(report,checks);
    }
    public Result Execute(ExternalCommandData commandData,ref string message,ElementSet elements)
    {
        // A native API failure can affect the whole Revit process even when the
        // test document is separate. Never run integration tests beside user work.
        if(commandData.Application.Application.Documents.Size!=0)
        {
            message="Run terrain validation in a separate Revit session with no documents open.";
            return Result.Cancelled;
        }
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SiteDesigner","TerrainSmoke",commandData.Application.Application.VersionNumber);
        Directory.CreateDirectory(folder); report=Path.Combine(folder,"result.txt");
        Document doc=null;
        EPTransactionContainer production=null;
        bool succeeded=false;
        Exception failure=null;
        try
        {
            checks.Add("Engine: "+typeof(TerrainApi).Assembly.Location+"; build="+typeof(TerrainApi).Module.ModuleVersionId);
            ApplicationUtility.Initialize(commandData.Application);
            doc=commandData.Application.Application.NewProjectDocument(UnitSystem.Metric);
            TerrainSurface surface;
            using(var transaction=new Transaction(doc,"Test native terrain creation"))
            {
                Start(transaction);
                Level.Create(doc,100);
                surface=TerrainApi.Create(doc,new List<XYZ>{new XYZ(0,0,112),new XYZ(100,0,112),new XYZ(100,100,114),new XYZ(0,100,112),new XYZ(50,50,113),new XYZ(30,30,112.5)});
                TerrainApi.NameParameter(surface)?.Set("Terrain smoke surface");
                transaction.Commit();
            }
            Check(((Element)surface).GetType()==TerrainApi.ElementType,"Version-specific native terrain element type");
            Check(new FilteredElementCollector(doc).OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).GetElementCount()==1,"Terrain collector discovers the native element");
            Check(surface.GetPoints().Count==6,"Creation preserves the six terrain points");
            Check(surface.GetPoints().Any(x=>x.IsAlmostEqualTo(new XYZ(50,50,113),1e-6)),"Creation preserves world Z above a nonzero level");
            Check(TerrainApi.NameParameter(surface)?.AsString()=="Terrain smoke surface","Terrain name persists");
            var familyTransactions=new EPTransactionContainer(doc);
            using(familyTransactions)
            {
                familyTransactions.Start("Test original terrain placement family");
                var path=Path.Combine(ApplicationUtility.RegistryPaths.ProgramPath,"Data","Library","Toposurface Placement.rfa");
                Check(doc.LoadFamily(path,new FamilyLoadOptions(true),out var family),"Original terrain placement family loads in the current Revit version");
                var symbol=(FamilySymbol)doc.GetElement(family.GetFamilySymbolIds().First());
                var level=new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().First();
                var marker=RevitUtility.NewFamilyInstance(doc,XYZ.Zero,symbol,surface,level,Autodesk.Revit.DB.Structure.StructuralType.NonStructural,ref familyTransactions);
                if(marker==null)throw new InvalidOperationException("Revit did not create the original terrain placement family instance.");
                var association=marker.LookupParameter(AssociationParameter);
                if(association==null)throw new InvalidOperationException("The placement family has no association parameter '"+AssociationParameter+"'. Available: "+string.Join(", ",marker.Parameters.Cast<Parameter>().Select(x=>x.Definition.Name)));
                association.Set(surface.UniqueId);
                doc.Regenerate();
                var associated=TopographySurfaceManagement.FindTopoSurfaceInstance(doc,association.AsString());
                if(associated?.Id!=surface.Id)throw new InvalidOperationException("Placement association lookup failed. Stored='"+association.AsString()+"'; expected='"+surface.UniqueId+"'; nativeByUniqueId="+doc.GetElement(association.AsString())?.GetType().FullName+"; terrainCount="+new FilteredElementCollector(doc).OfClass(TerrainApi.ElementType).GetElementCount());
                Check(true,"Placement family associates with the native terrain element");
                familyTransactions.Commit();
            }
            var container=new TopographySurfaceContainer(doc.Application,surface,true);
            Check(container.Triangles.Count>0 && container.MinLeftPoint.z>=112-1e-6 && container.MaxRightPoint.z<=114+1e-6,"Geometry container uses the top surface, excluding solid bottom and sides");
            var id=surface.Id;
            var boundary=surface.GetBoundaryPoints().ToList();
            using(var scope=new EditScope(doc,"Test point batch"))
            {
                scope.Start(surface.Id);
                using(var transaction=new Transaction(doc,"Add move delete points"))
                {
                    Start(transaction);
                    surface.AddPoints(new List<XYZ>{new XYZ(25,60,114.25)});
                    surface.MovePoint(new XYZ(50,50,113),new XYZ(60,50,115.25));
                    surface.DeletePoints(new List<XYZ>{new XYZ(30,30,112.5)});
                    transaction.Commit();
                }
                scope.Commit(new TopographyEditFailuresPreprocessor());
            }
            Check(surface.Id==id,"Point editing preserves terrain element ID");
            Check(surface.GetPoints().Count==6 && surface.GetPoints().Any(x=>x.IsAlmostEqualTo(new XYZ(60,50,115.25),1e-6)),"Batched add/move/delete points and world elevation");
            Check(boundary.All(point=>surface.GetBoundaryPoints().Any(x=>x.IsAlmostEqualTo(point,1e-6))),"Point editing preserves boundary elevations");
            using(var scope=new EditScope(doc,"Cancel point edit"))
            {
                scope.Start(surface.Id);
                using(var transaction=new Transaction(doc,"Discard point"))
                { Start(transaction);surface.AddPoints(new List<XYZ>{new XYZ(20,20,150)});transaction.Commit(); }
                // Disposing an uncommitted scope must discard the whole edit.
            }
            Check(!surface.GetPoints().Any(x=>x.IsAlmostEqualTo(new XYZ(20,20,150),1e-6)),"Uncommitted terrain scope rolls back");
#if !REVIT2022 && !REVIT2023
            production=new EPTransactionContainer(doc);
            production.StartTopographyEditScope("Production point batch",surface.Id);
            production.Start("Production grading points");
            surface.AddPoints(new List<XYZ>{new XYZ(40,80,114.5)});
            Check(production.Commit(),"Production transaction commits the Toposolid point batch");
            var failures=new TopographyEditFailuresPreprocessor();
            production.CommitTopographyEditScope(ref failures);
            Check(surface.GetPoints().Any(x=>x.IsAlmostEqualTo(new XYZ(40,80,114.5),1e-6)),"Production terrain scope persists grading points");
            TerrainSurface profiled;
            ElementId subdivisionMaterial;
            using(var transaction=new Transaction(doc,"Create terrain with opening"))
            {
                Start(transaction);
                var profiles=new List<CurveLoop>{Rectangle(200,0,300,100),Rectangle(240,40,260,60)};
                var points=new List<XYZ>{new XYZ(200,0,112),new XYZ(300,0,112),new XYZ(300,100,114),new XYZ(200,100,112),new XYZ(220,20,113)};
                profiled=new ToposolidSurface(Toposolid.Create(doc,profiles,points,surface.Element.GetTypeId(),surface.Element.LevelId));
                doc.Regenerate();
                var child=profiled.Element.CreateSubDivision(doc,new List<CurveLoop>{Rectangle(205,5,215,15)});
                child.get_Parameter(TerrainApi.SubdivisionHeightParameter).Set(1.75);
                subdivisionMaterial=Material.Create(doc,"Subdivision smoke material");
                TerrainApi.SetMaterialId(new ToposolidSurface(child),subdivisionMaterial);
                doc.Regenerate();
                Check(TerrainApi.HostId(new ToposolidSurface(child))==profiled.Id,"Native subdivision retains its host relationship");
                transaction.Commit();
            }
            TerrainSurface copy;
            using(var transaction=new Transaction(doc,"Duplicate terrain with profile"))
            {
                Start(transaction);
                copy=TerrainApi.Create(doc,profiled.GetPoints(),profiled);
                var material=doc.GetElement(Material.Create(doc,"Terrain smoke material")) as Material;
                TerrainApi.SetMaterialId(copy,material.Id);
                Check(TerrainApi.GetMaterialId(copy)==material.Id && TerrainApi.GetMaterialId(profiled)!=material.Id,"Material edits isolate the terrain type from other instances");
                transaction.Commit();
            }
            Check(copy.GetProfiles().Count==2,"Proposed terrain creation preserves profile openings");
            Check(copy.Element.GetSubDivisionIds().Count==1,"Proposed terrain creation preserves subdivisions");
            var childCopy=new ToposolidSurface((Toposolid)doc.GetElement(copy.Element.GetSubDivisionIds().Single()));
            Check(TerrainApi.GetMaterialId(childCopy)==subdivisionMaterial && Math.Abs(childCopy.Element.get_Parameter(TerrainApi.SubdivisionHeightParameter).AsDouble()-1.75)<1e-6,"Proposed terrain preserves subdivision height and material");
            var containers=TopographySurfaceManagement.FindAllMainSurfaces(doc,true);
            Check(containers.Any(x=>x.TopographySurface?.Id==copy.Id),"Main-surface discovery includes native Toposolids");
            var host=containers.Single(x=>x.TopographySurface?.Id==copy.Id);
            var subdivisionList=new List<TopographySurfaceContainer>{new TopographySurfaceContainer(doc.Application,childCopy,true)};
            var hosts=new List<TopographySurfaceContainer>{host}; bool issueWarning=false;
            Check(!TopographySurfaceManagement.AddSubRegionBuildingPadsToMainSurfaceList(doc,subdivisionList,ref hosts,true,ref issueWarning)
                && host.SubregionBuildingPadList.Count==1,"Native subdivision discovery does not request legacy host replacement");
            Check(host.GetAllVertices().Count==copy.GetPoints().Count,"Host grading excludes subdivision thickness from terrain points");
            using(var scope=new EditScope(doc,"Profiled shape edit"))
            {
                scope.Start(copy.Id);
                using(var transaction=new Transaction(doc,"Point outside profile opening"))
                {Start(transaction);copy.AddPoints(new List<XYZ>{new XYZ(225,25,116)});transaction.Commit();}
                scope.Commit(new TopographyEditFailuresPreprocessor());
            }
            Check(copy.GetProfiles().Count==2 && copy.GetPoints().Any(x=>x.IsAlmostEqualTo(new XYZ(225,25,116),1e-6)),"Point editing preserves openings and requested elevation");
            bool rejected=false;
            try
            {
                using(var scope=new EditScope(doc,"Reject point in opening"))
                {
                    scope.Start(copy.Id);
                    using(var transaction=new Transaction(doc,"Invalid grading point"))
                    {Start(transaction);copy.AddPoints(new List<XYZ>{new XYZ(250,50,120)});transaction.Commit();}
                    scope.Commit(new TopographyEditFailuresPreprocessor());
                }
            }
            catch(InvalidOperationException){rejected=true;}
            Check(rejected && !copy.GetPoints().Any(x=>x.IsAlmostEqualTo(new XYZ(250,50,120),1e-6)),"Point in profile opening is rejected and rolled back");
            rejected=false;
            try
            {
                production.StartTopographyEditScope("Reject production point in opening",copy.Id);
                production.Start("Invalid production grading");
                copy.AddPoints(new List<XYZ>{new XYZ(250,50,120)});
                production.Commit();
            }
            catch(InvalidOperationException){rejected=true;}
            finally {production.DisposeTopographyEditScope();}
            Check(rejected && !doc.IsModifiable && !copy.GetPoints().Any(x=>x.IsAlmostEqualTo(new XYZ(250,50,120),1e-6)),"Production grading failure rolls back transaction and terrain scope");
            TerrainSurface creased;
            using(var transaction=new Transaction(doc,"Create terrain breakline"))
            {
                Start(transaction);
                creased=TerrainApi.Create(doc,new List<XYZ>{new XYZ(400,0,112),new XYZ(500,0,112),new XYZ(500,100,112),new XYZ(400,100,112)});
                var editor=creased.Element.GetSlabShapeEditor(); editor.Enable(); doc.Regenerate();
                var vertices=editor.SlabShapeVertices.Cast<SlabShapeVertex>().ToList();
                var a=vertices.Single(x=>x.Position.IsAlmostEqualTo(new XYZ(400,0,112),1e-6));
                var b=vertices.Single(x=>x.Position.IsAlmostEqualTo(new XYZ(500,100,112),1e-6));
#if REVIT2024
                editor.DrawSplitLine(a,b);
#else
                editor.AddSplitLine(a,b);
#endif
                transaction.Commit();
            }
            using(var scope=new EditScope(doc,"Preserve terrain breakline"))
            {
                scope.Start(creased.Id);
                using(var transaction=new Transaction(doc,"Grade near breakline"))
                { Start(transaction);creased.AddPoints(new List<XYZ>{new XYZ(425,75,114)});transaction.Commit(); }
                scope.Commit(new TestFailures());
            }
            Check(creased.Element.GetSlabShapeEditor().SlabShapeCreases.Cast<SlabShapeCrease>().Any(x=>x.CreaseType==SlabShapeCreaseType.UserDrawn),"Point edits preserve user breaklines");
            production.Start("Test production proposed terrain creation");
            TerrainSurface proposed=null; FamilyInstance placement=null;
            TopographySurfaceManagement.CreateProposedSurface(doc,production,null,creased.GetPoints().Select(x=>new Eps_Point(x)).ToList(),
                "Proposed terrain smoke",null,null,null,ref proposed,ref placement,creased);
            Check(proposed.Element.GetSlabShapeEditor().SlabShapeCreases.Cast<SlabShapeCrease>().Any(x=>x.CreaseType==SlabShapeCreaseType.UserDrawn)
                && TerrainApi.NameParameter(proposed).AsString()=="Proposed terrain smoke","Production proposed terrain preserves breaklines and terrain name");
            production.Rollback();
#endif
            succeeded=true;
            return Result.Succeeded;
        }
        catch(Exception exception)
        {
            failure=exception;
            File.WriteAllText(report,string.Join(Environment.NewLine,checks)+Environment.NewLine+exception);
            message=exception.Message; return Result.Failed;
        }
        finally
        {
            production?.Dispose();
            if(doc!=null && doc.IsValidObject)
            {
                Check(!doc.IsModifiable,"Terrain test leaves no open transaction");
                Check(doc.Close(false),"Terrain test document closes without open groups");
            }
            if(succeeded)
            {
                Check(commandData.Application.Application.Documents.Size==0,"Dedicated test session has no remaining documents");
                checks.Add("SUCCESS: "+checks.Count(x=>x.StartsWith("PASS:"))+" terrain checks"); File.WriteAllLines(report,checks);
            }
            else if(failure!=null) File.AppendAllText(report,Environment.NewLine+failure);
        }
    }
#if !REVIT2022 && !REVIT2023
    private static CurveLoop Rectangle(double x1,double y1,double x2,double y2)
    {
        var points=new[]{new XYZ(x1,y1,0),new XYZ(x2,y1,0),new XYZ(x2,y2,0),new XYZ(x1,y2,0)};
        var loop=new CurveLoop();
        for(int i=0;i<4;i++)loop.Append(Line.CreateBound(points[i],points[(i+1)%4]));
        return loop;
    }
#endif
}
