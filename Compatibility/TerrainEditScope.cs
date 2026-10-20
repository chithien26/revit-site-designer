#if !REVIT2022 && !REVIT2023
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace EPCommon;

// Toposolids use ordinary transactions, with one shape rebuild per committed edit batch.
public sealed class TerrainEditScope : IDisposable
{
    private static readonly List<TerrainPointEdit> edits = new List<TerrainPointEdit>();
    private readonly Document document;
    private readonly string name;
    private TransactionGroup group;
    private TerrainPointEdit edit;
    public bool IsActive => edit != null;
    public bool IsPermitted => !document.IsReadOnly && !document.IsModifiable;
    public TerrainEditScope(Document document,string name){this.document=document;this.name=name;}
    public ElementId Start(ElementId id)
    {
        if(IsActive || !IsPermitted)throw new InvalidOperationException("Finish the current transaction before starting a terrain edit scope.");
        var surface=TerrainApi.AsSurface(document.GetElement(id));
        if(surface==null || surface.IsSiteSubRegion)throw new ArgumentException("Select a host Toposolid to edit.",nameof(id));
        var pending=new TerrainPointEdit(surface);
        var pendingGroup=new TransactionGroup(document,name);
        try
        {
            if(pendingGroup.Start()!=TransactionStatus.Started)throw new InvalidOperationException("Revit could not start the terrain transaction group.");
            group=pendingGroup; edit=pending; edits.Add(edit);
        }
        catch {pendingGroup.Dispose();throw;}
        return id;
    }
    internal static TerrainPointEdit Find(ToposolidSurface surface) => edits.FirstOrDefault(x=>x.Surface.Document.Equals(surface.Document) && x.Surface.Id==surface.Id);
    public static void Flush(Document document)
    {
        foreach(var item in edits.Where(x=>x.Surface.Document.Equals(document) && x.Dirty).ToArray())item.Apply();
    }
    public void Commit(IFailuresPreprocessor failuresPreprocessor)
    {
        if(!IsActive)return;
        if(edit.Dirty)
        {
            if(document.IsModifiable)throw new InvalidOperationException("Commit the point-edit transaction before completing the terrain scope.");
            using(var transaction=new Transaction(document,name))
            {
                transaction.Start();
                var preprocessor = failuresPreprocessor ?? new TopographyEditFailuresPreprocessor();
                transaction.SetFailureHandlingOptions(transaction.GetFailureHandlingOptions().SetFailuresPreprocessor(preprocessor).SetClearAfterRollback(true));
                edit.Apply();
                if(transaction.Commit()!=TransactionStatus.Committed)throw new InvalidOperationException("Revit rolled back the Toposolid point changes.");
            }
        }
        if(group.Assimilate()!=TransactionStatus.Committed)throw new InvalidOperationException("Revit could not commit the terrain transaction group.");
        edits.Remove(edit); edit=null; group.Dispose(); group=null;
    }
    public void Dispose()
    {
        if(group!=null)
        {
            if(group.HasStarted())group.RollBack();
            group.Dispose();group=null;
        }
        if(edit!=null){edits.Remove(edit);edit=null;}
    }
}
#endif
