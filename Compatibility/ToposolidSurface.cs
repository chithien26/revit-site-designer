#if !REVIT2022 && !REVIT2023
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace EPCommon;

// Version-specific terrain operations; this always wraps a real native Toposolid.
public sealed class ToposolidSurface
{
    internal const double Tolerance = 1e-6;
    public Toposolid Element { get; private set; }
    public Document Document => Element.Document;
    public ElementId Id => Element.Id;
    public string UniqueId => Element.UniqueId;
    public Category Category => Element.Category;
    public Location Location => Element.Location;
    public bool IsValidObject => Element.IsValidObject;
    public bool IsSiteSubRegion => Element.HostTopoId != ElementId.InvalidElementId;
    public ElementId CreatedPhaseId { get => Element.CreatedPhaseId; set => Element.CreatedPhaseId = value; }
    public ElementId DemolishedPhaseId { get => Element.DemolishedPhaseId; set => Element.DemolishedPhaseId = value; }
    public ToposolidSurface(Toposolid element) => Element = element ?? throw new ArgumentNullException(nameof(element));
    public static implicit operator Element(ToposolidSurface surface) => surface?.Element;
    public Parameter LookupParameter(string name) => Element.LookupParameter(name);
    public Parameter get_Parameter(BuiltInParameter parameter) => Element.get_Parameter(parameter);
    public BoundingBoxXYZ get_BoundingBox(View view) => Element.get_BoundingBox(view);
    public IList<ElementId> GetHostedSubRegionIds() => Element.GetSubDivisionIds();

    public static ToposolidSurface Create(Document document, IList<XYZ> points, ToposolidSurface template = null, bool copyHostSketch = true)
    {
        var unique = UniquePoints(points);
        if (unique.Count < 3) throw new ArgumentException("Terrain needs at least three distinct XY points.", nameof(points));
        var typeId = template?.Element.GetTypeId() ?? new FilteredElementCollector(document).OfClass(typeof(ToposolidType)).FirstElementId();
        if (typeId == ElementId.InvalidElementId) throw new InvalidOperationException("Load a Toposolid type into this project before creating terrain.");
        var level = template == null ? new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().OrderBy(x => Math.Abs(x.Elevation - unique.Min(p => p.Z))).FirstOrDefault()
            : document.GetElement(template.Element.LevelId) as Level;
        if (level == null) level = Level.Create(document, 0);
        Toposolid solid;
        var profiles = copyHostSketch ? template?.GetProfiles() : null;
        if (profiles != null && profiles.Count > 0) solid = Toposolid.Create(document, profiles, unique, typeId, level.Id);
        else solid = Toposolid.Create(document, unique, typeId, level.Id);
        document.Regenerate();
        if (template != null && copyHostSketch)
        {
            var result = new ToposolidSurface(solid);
            RestoreBreaklines(result, template.Element.GetSlabShapeEditor().SlabShapeCreases.Cast<SlabShapeCrease>()
                .Where(crease => crease.CreaseType == SlabShapeCreaseType.UserDrawn)
                .Select(crease => new[] { crease.Curve.GetEndPoint(0), crease.Curve.GetEndPoint(1) }).ToList());
            foreach (var childId in template.Element.GetSubDivisionIds())
            {
                var child = new ToposolidSurface((Toposolid)document.GetElement(childId));
                var copy = solid.CreateSubDivision(document, child.GetProfiles());
                // Subdivisions inherit their host type and cannot use ChangeTypeId.
                var material = child.Element.get_Parameter(BuiltInParameter.TOPOSOLID_SUBDIVIDE_MATERIAL);
                var targetMaterial = copy.get_Parameter(BuiltInParameter.TOPOSOLID_SUBDIVIDE_MATERIAL);
                if (material != null && targetMaterial != null && !targetMaterial.IsReadOnly)
                    targetMaterial.Set(material.AsElementId());
                var height = child.Element.get_Parameter(TerrainApi.SubdivisionHeightParameter);
                var targetHeight = copy.get_Parameter(TerrainApi.SubdivisionHeightParameter);
                if (height != null && targetHeight != null && !targetHeight.IsReadOnly)
                    targetHeight.Set(height.AsDouble());
            }
        }
        return new ToposolidSurface(solid);
    }
    internal static void RestoreBreaklines(ToposolidSurface surface, IList<XYZ[]> creases, bool required = true)
    {
        if (creases.Count == 0) return;
        var editor = surface.Element.GetSlabShapeEditor();
        if (!editor.IsEnabled) editor.Enable();
        surface.Document.Regenerate();
        foreach (var crease in creases)
        {
            var shape = surface.Vertices();
            var start = shape.FirstOrDefault(x => SameXY(x.Position, crease[0]));
            var end = shape.FirstOrDefault(x => SameXY(x.Position, crease[1]));
            if (start == null || end == null)
            {
                if (required) throw new InvalidOperationException("Terrain recreation would remove a breakline endpoint. Edit the breakline first.");
                continue;
            }
            try
            {
#if REVIT2024
                editor.DrawSplitLine(start, end);
#else
                editor.AddSplitLine(start, end);
#endif
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                if (required) throw;
            }
            surface.Document.Regenerate();
        }
    }
    public IList<CurveLoop> GetProfiles()
    {
        var sketch = Document.GetElement(Element.SketchId) as Sketch;
        if (sketch == null) throw new InvalidOperationException("The Toposolid has no editable sketch boundary.");
        var loops = new List<CurveLoop>();
        foreach (CurveArray profile in sketch.Profile)
        {
            var loop = new CurveLoop();
            foreach (Curve curve in profile) loop.Append(curve.Clone());
            loops.Add(loop);
        }
        return loops;
    }
    internal List<SlabShapeVertex> Vertices()
    {
        if (IsSiteSubRegion) return new List<SlabShapeVertex>();
        var editor = Element.GetSlabShapeEditor();
        return editor == null ? new List<SlabShapeVertex>() : editor.SlabShapeVertices.Cast<SlabShapeVertex>().ToList();
    }
    internal List<XYZ> ReadPoints()
    {
        var vertices = Vertices();
        return UniquePoints(vertices.Count > 0 ? vertices.Select(x => x.Position) : TerrainApi.GetSurfaceMeshes(this).SelectMany(x => x.Vertices));
    }
    public IList<XYZ> GetPoints() => TerrainEditScope.Find(this)?.Points ?? ReadPoints();
    public IList<XYZ> GetBoundaryPoints()
    {
        var session = TerrainEditScope.Find(this);
        if (session != null) return session.BoundaryPoints;
        var vertices = Vertices();
        if (vertices.Count > 0) return UniquePoints(vertices.Where(x => x.VertexType != SlabShapeVertexType.Interior).Select(x => x.Position));
        return MeshBoundary();
    }
    private List<XYZ> MeshBoundary()
    {
        var edges = new Dictionary<string, (int Count, XYZ A, XYZ B)>();
        foreach (var mesh in TerrainApi.GetSurfaceMeshes(this))
            for (int i=0; i<mesh.NumTriangles; i++)
            {
                var triangle = mesh.get_Triangle(i);
                for (int j=0; j<3; j++)
                {
                    var a=triangle.get_Vertex(j); var b=triangle.get_Vertex((j+1)%3);
                    string ka=Key(a), kb=Key(b), key=string.CompareOrdinal(ka,kb)<0 ? ka+":"+kb : kb+":"+ka;
                    edges[key] = edges.TryGetValue(key,out var edge) ? (edge.Count+1,a,b) : (1,a,b);
                }
            }
        return UniquePoints(edges.Values.Where(x=>x.Count==1).SelectMany(x=>new[]{x.A,x.B}));
    }
    public IList<XYZ> GetInteriorPoints()
    {
        var boundary = new HashSet<string>(GetBoundaryPoints().Select(Key));
        return GetPoints().Where(x => !boundary.Contains(Key(x))).ToList();
    }
    public IList<XYZ> FindPoints(Outline bounds) => GetPoints().Where(x => bounds.Contains(x, Tolerance)).ToList();
    public bool IsBoundaryPoint(XYZ point) => GetBoundaryPoints().Any(x=>SameXY(x,point));
    public bool ContainsPoint(XYZ point) => GetPoints().Any(x=>x.IsAlmostEqualTo(point,Tolerance));
    public static bool ArePointsDistinct(IList<XYZ> points) => points.Count == points.Select(Key).Distinct().Count();
    internal static string Key(XYZ point) => Math.Round(point.X/Tolerance).ToString(System.Globalization.CultureInfo.InvariantCulture)+","+Math.Round(point.Y/Tolerance).ToString(System.Globalization.CultureInfo.InvariantCulture);
    internal static bool SameXY(XYZ a, XYZ b) => Math.Abs(a.X-b.X)<Tolerance && Math.Abs(a.Y-b.Y)<Tolerance;
    internal static List<XYZ> UniquePoints(IEnumerable<XYZ> points) => points.GroupBy(Key).Select(x=>x.Last()).ToList();
    public void AddPoints(IList<XYZ> points) => Edit(session => session.Add(points));
    public void DeletePoints(IList<XYZ> points) => Edit(session => session.Delete(points));
    public void MovePoint(XYZ point, XYZ target) => Edit(session => session.Move(point,target));
    private void Edit(Action<TerrainPointEdit> action)
    {
        if (!Document.IsModifiable) throw new InvalidOperationException("Terrain editing requires an open Revit transaction.");
        if (IsSiteSubRegion) throw new InvalidOperationException("Edit the host Toposolid's points rather than a subdivision's points.");
        var session = TerrainEditScope.Find(this);
        if (session != null) { action(session); return; }
        session = new TerrainPointEdit(this);
        action(session);
        session.Apply();
    }

    internal void EnsureThicknessFor(IList<XYZ> points, double planeZ)
    {
        EnsureThicknessFor(points, null, planeZ);
    }

    internal void EnsureThicknessFor(IList<XYZ> points, IList<XYZ> original, double planeZ)
    {
        if (points == null || points.Count == 0) return;
        double minZ = points.Min(point => point.Z);
        double maxZ = points.Max(point => point.Z);
        if (original != null)
        {
            foreach (XYZ point in original)
            {
                if (point == null) continue;
                if (point.Z > maxZ) maxZ = point.Z;
                if (point.Z < minZ) minZ = point.Z;
            }
        }
        double drop = Math.Max(planeZ, maxZ) - minZ;
        if (drop <= 0.05) return;
        // Only as thick as this cut, plus 100 mm. Never rewrite the project's original type.
        double needed = drop + 0.32808399;
        RuntimeDiagnostics.Write("Toposolid grading Z plane=" + planeZ + " drop=" + drop + " needed=" + needed + " ft.");
        EnsureIsolatedThickness(needed);
    }

    internal void EnsureIsolatedThickness(double needed)
    {
        var type = Document.GetElement(Element.GetTypeId()) as ToposolidType;
        if (type == null) return;
        type = IsolateProposedType(type);
        double current = ThicknessOf(type);
        if (current + 1e-6 >= needed) return;
        if (!SetThickness(type, needed))
        {
            RuntimeDiagnostics.Write("Could not increase Toposolid type thickness from " + current + " ft to " + needed + " ft.");
            return;
        }
        RuntimeDiagnostics.Write("Increased isolated type '" + type.Name + "' thickness from " + current + " ft to " + needed + " ft.");
    }

    private ToposolidType IsolateProposedType(ToposolidType type)
    {
        if (type.Name.IndexOf("(Site Designer)", StringComparison.OrdinalIgnoreCase) >= 0) return type;
        string name = type.Name + " (Site Designer)";
        var existing = new FilteredElementCollector(Document).OfClass(typeof(ToposolidType)).Cast<ToposolidType>()
            .FirstOrDefault(candidate => string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase));
        type = existing ?? (ToposolidType)type.Duplicate(name);
        if (Element.GetTypeId() != type.Id)
        {
            string previousId = Element.UniqueId;
            ElementId newId = Element.ChangeTypeId(type.Id);
            if (newId != Element.Id && Document.GetElement(newId) is Toposolid replacement)
            {
                Element = replacement;
            }
            Document.Regenerate();
            RelinkPlacement(Document, previousId, Element.UniqueId);
        }
        return type;
    }

    private static void RelinkPlacement(Document document, string previousId, string newId)
    {
        if (string.IsNullOrEmpty(previousId) || string.Equals(previousId, newId, StringComparison.Ordinal)) return;
        foreach (FamilyInstance instance in new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site).Cast<FamilyInstance>())
        {
            Parameter parameter = instance.LookupParameter("UniqueID");
            if (parameter != null && !parameter.IsReadOnly && string.Equals(parameter.AsString(), previousId, StringComparison.OrdinalIgnoreCase))
            {
                parameter.Set(newId);
            }
        }
    }

    private static double ThicknessOf(ToposolidType type)
    {
        var structure = type.GetCompoundStructure();
        if (structure != null && structure.LayerCount > 0) return structure.GetWidth();
        foreach (BuiltInParameter id in new[] { BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM, BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM })
        {
            var parameter = type.get_Parameter(id);
            if (parameter != null && parameter.StorageType == StorageType.Double) return parameter.AsDouble();
        }
        return 0;
    }

    private static bool SetThickness(ToposolidType type, double needed)
    {
        var structure = type.GetCompoundStructure();
        if (structure != null && structure.LayerCount > 0)
        {
            double extra = needed - structure.GetWidth();
            if (extra <= 0) return true;
            var layers = new List<int>();
            int variable = structure.VariableLayerIndex;
            if (variable >= 0) layers.Add(variable);
            for (int i = structure.LayerCount - 1; i >= 0; i--)
                if (i != variable) layers.Add(i);
            foreach (int i in layers)
            {
                try
                {
                    structure.SetLayerWidth(i, structure.GetLayerWidth(i) + extra);
                    if (variable < 0)
                    {
                        try { structure.VariableLayerIndex = i; }
                        catch (Autodesk.Revit.Exceptions.ArgumentException) { }
                    }
                    type.SetCompoundStructure(structure);
                    return true;
                }
                catch (Autodesk.Revit.Exceptions.ArgumentException)
                {
                }
            }
        }
        foreach (BuiltInParameter id in new[] { BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM, BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM })
        {
            var parameter = type.get_Parameter(id);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Double)
            {
                parameter.Set(needed);
                return true;
            }
        }
        return false;
    }
}

internal sealed class TerrainPointEdit
{
    internal ToposolidSurface Surface { get; }
    private readonly List<XYZ> original;
    private readonly List<XYZ> current;
    private readonly HashSet<int> moved = new HashSet<int>();
    private readonly HashSet<string> boundary;
    internal bool Dirty { get; private set; }
    internal List<XYZ> Points => current.Where(x=>x!=null).ToList();
    internal List<XYZ> BoundaryPoints => Points.Where(x=>boundary.Contains(ToposolidSurface.Key(x))).ToList();
    internal TerrainPointEdit(ToposolidSurface surface)
    {
        Surface=surface;
        original=surface.ReadPoints();
        current=new List<XYZ>(original);
        boundary=new HashSet<string>(surface.GetBoundaryPoints().Select(ToposolidSurface.Key));
    }
    private int Index(XYZ point)
    {
        for(int i=0;i<original.Count;i++)
            if(!moved.Contains(i) && current[i]!=null && original[i].IsAlmostEqualTo(point,ToposolidSurface.Tolerance))return i;
        return current.FindIndex(x=>x!=null && x.IsAlmostEqualTo(point,ToposolidSurface.Tolerance));
    }
    internal void Add(IList<XYZ> points)
    {
        foreach(var point in points)
        {
            int index=current.FindIndex(x=>x!=null && ToposolidSurface.SameXY(x,point));
            if(index>=0)current[index]=point; else current.Add(point);
        }
        Dirty=true;
    }
    internal void Delete(IList<XYZ> points)
    {
        foreach(var point in points)
        {
            if(boundary.Contains(ToposolidSurface.Key(point)))throw new InvalidOperationException("Toposolid boundary points must be changed by editing its sketch.");
            int index=Index(point);
            if(index>=0){current[index]=null; moved.Add(index);}
        }
        Dirty=true;
    }
    internal void Move(XYZ point,XYZ target)
    {
        if(boundary.Contains(ToposolidSurface.Key(point)) && !ToposolidSurface.SameXY(point,target))throw new InvalidOperationException("Moving a Toposolid boundary in XY requires editing its sketch.");
        int index=Index(point);
        if(index<0)throw new ArgumentException("The terrain point to move was not found.");
        current[index]=target; moved.Add(index); Dirty=true;
    }
    private double SketchPlaneZ(List<SlabShapeVertex> vertices)
    {
        // After ResetSlabShape this is the sketch plane in SlabShapeVertex.Position coordinates.
        return vertices[0].Position.Z;
    }
    internal void Apply()
    {
        if(!Dirty)return;
        var points=Points;
        if(!ToposolidSurface.ArePointsDistinct(points))throw new InvalidOperationException("Terrain editing produced duplicate XY points.");
        var editor=Surface.Element.GetSlabShapeEditor();
        if(!editor.IsEnabled)editor.Enable();
        var creases=editor.SlabShapeCreases.Cast<SlabShapeCrease>().Where(x=>x.CreaseType==SlabShapeCreaseType.UserDrawn)
            .Select(x=>new[]{x.Curve.GetEndPoint(0),x.Curve.GetEndPoint(1)}).ToList();
        foreach(var crease in creases)
            if(crease.Any(endpoint=>!points.Any(x=>ToposolidSurface.SameXY(x,endpoint))))
                throw new InvalidOperationException("A point edit would remove the endpoint of a Toposolid breakline. Edit that breakline first.");
        bool removed = current.Any(point => point == null);
        bool movedXY = false;
        for (int i = 0; i < original.Count && i < current.Count; i++)
        {
            if (current[i] == null || original[i] == null) continue;
            if (!ToposolidSurface.SameXY(original[i], current[i])) { movedXY = true; break; }
        }
        // Resetting rebuilds the whole mesh and folds existing holes into giant planes.
        // Only rebuild when a point was deleted or moved in XY.
        if (removed || movedXY)
        {
            editor.ResetSlabShape();
            if(!editor.IsEnabled)editor.Enable();
            Surface.Document.Regenerate();
        }
        var vertices=Surface.Vertices();
        if(vertices.Count==0)throw new InvalidOperationException("The Toposolid did not expose its boundary shape vertices.");
        double baseZ=SketchPlaneZ(vertices);
        Surface.EnsureThicknessFor(points, original, baseZ);
        Surface.Document.Regenerate();
        editor=Surface.Element.GetSlabShapeEditor();
        if(!editor.IsEnabled)editor.Enable();
        vertices=Surface.Vertices();
        if(vertices.Count==0)throw new InvalidOperationException("The Toposolid did not expose its boundary shape vertices.");
        baseZ=SketchPlaneZ(vertices);
        var faces=HostObjectUtils.GetTopFaces(Surface.Element)
            .Select(reference=>Surface.Element.GetGeometryObjectFromReference(reference)).OfType<Face>().ToList();
        var keys=new HashSet<string>(vertices.Select(x=>ToposolidSurface.Key(x.Position)));
        int skipped = 0;
        foreach(var point in points)
        {
            if(keys.Add(ToposolidSurface.Key(point)))
            {
                var flatPoint=new XYZ(point.X,point.Y,baseZ);
                if(!faces.Any(face=>{var projection=face.Project(flatPoint);return projection!=null && face.IsInside(projection.UVPoint);}))
                {
                    skipped++;
                    continue;
                }
#if REVIT2024
                var added=editor.DrawPoint(flatPoint);
#else
                var added=editor.AddPoint(flatPoint);
#endif
                if(added==null) { skipped++; continue; }
            }
        }
        if (skipped > 0) RuntimeDiagnostics.Write("Toposolid grading skipped " + skipped + " point(s) outside the sketch or inside an opening.");
        Surface.Document.Regenerate();
        var desired=points.ToDictionary(ToposolidSurface.Key);
        foreach(var vertex in Surface.Vertices())
            if(desired.TryGetValue(ToposolidSurface.Key(vertex.Position),out var target))editor.ModifySubElement(vertex,target.Z-baseZ);
        Surface.Document.Regenerate();
        ToposolidSurface.RestoreBreaklines(Surface, creases);
        Surface.Document.Regenerate();
        Dirty=false;
    }
}
#endif
