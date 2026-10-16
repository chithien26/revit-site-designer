using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace EPCommon;

public static class TerrainApi
{
#if REVIT2022 || REVIT2023
    public static bool UsesToposolids => false;
    public static Type ElementType => typeof(TopographySurface);
    public static BuiltInCategory Category => BuiltInCategory.OST_Topography;
    public static TopographySurface AsSurface(object element) => element as TopographySurface;
#else
    public static bool UsesToposolids => true;
    public static Type ElementType => typeof(Toposolid);
    public static BuiltInCategory Category => BuiltInCategory.OST_Toposolid;
#if REVIT2024
    public static BuiltInParameter SubdivisionHeightParameter => BuiltInParameter.TOPOSOLID_SUBDIVIDE_HEIGNT;
#else
    public static BuiltInParameter SubdivisionHeightParameter => BuiltInParameter.TOPOSOLID_SUBDIVIDE_HEIGHT;
#endif
    public static TopographySurface AsSurface(object element) => element as ToposolidSurface ?? (element is Toposolid solid ? new ToposolidSurface(solid) : null);
#endif
    public static TopographySurface Create(Document document, IList<XYZ> points, TopographySurface template = null)
    {
#if REVIT2022 || REVIT2023
        return TopographySurface.Create(document, points);
#else
        return ToposolidSurface.Create(document, points, template);
#endif
    }
    public static IEnumerable<Mesh> GetSurfaceMeshes(TopographySurface surface)
    {
        if (surface == null) return Enumerable.Empty<Mesh>();
#if REVIT2022 || REVIT2023
        var geometry = surface.get_Geometry(new Options());
        return geometry == null ? Enumerable.Empty<Mesh>() : geometry.OfType<Mesh>();
#else
        var meshes = new List<Mesh>();
        try
        {
            foreach (var reference in HostObjectUtils.GetTopFaces(surface.Element))
                if (surface.Element.GetGeometryObjectFromReference(reference) is Face face) meshes.Add(face.Triangulate());
        }
        catch (Autodesk.Revit.Exceptions.ArgumentException) { }
        if (meshes.Count == 0)
        {
            var geometry = surface.Element.get_Geometry(new Options());
            if (geometry != null)
                foreach (var solid in geometry.OfType<Solid>())
                    foreach (Face face in solid.Faces)
                    {
                        var bounds = face.GetBoundingBox();
                        if (face.ComputeNormal((bounds.Min + bounds.Max) / 2).Z > 1e-8) meshes.Add(face.Triangulate());
                    }
        }
        return meshes;
#endif
    }
    public static Parameter NameParameter(Element element)
    {
#if !REVIT2022 && !REVIT2023
        if (element is Toposolid) return element.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
#endif
        return element?.get_Parameter(BuiltInParameter.ROOM_NAME);
    }
    public static ElementId GetMaterialId(TopographySurface surface)
    {
        if (surface == null) return ElementId.InvalidElementId;
#if REVIT2022 || REVIT2023
        return surface.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM)?.AsElementId() ?? ElementId.InvalidElementId;
#else
        if (surface.IsSiteSubRegion)
            return surface.Element.get_Parameter(BuiltInParameter.TOPOSOLID_SUBDIVIDE_MATERIAL)?.AsElementId() ?? ElementId.InvalidElementId;
        var type = surface.Document.GetElement(surface.Element.GetTypeId()) as ToposolidType;
        var structure = type?.GetCompoundStructure();
        return structure != null && structure.LayerCount > 0 ? structure.GetMaterialId(0) : ElementId.InvalidElementId;
#endif
    }
    public static void SetMaterialId(TopographySurface surface, ElementId materialId)
    {
        if (surface == null || materialId == null || materialId == ElementId.InvalidElementId) return;
#if REVIT2022 || REVIT2023
        var parameter = surface.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
        if (parameter != null && !parameter.IsReadOnly) parameter.Set(materialId);
#else
        if (surface.IsSiteSubRegion)
        {
            var material = surface.Element.get_Parameter(BuiltInParameter.TOPOSOLID_SUBDIVIDE_MATERIAL);
            if (material != null && !material.IsReadOnly) material.Set(materialId);
            return;
        }
        if (GetMaterialId(surface) == materialId) return;
        var type = (ToposolidType)surface.Document.GetElement(surface.Element.GetTypeId());
        var structure = type.GetCompoundStructure();
        if (structure == null || structure.LayerCount == 0) throw new InvalidOperationException("The selected Toposolid type has no material layers.");
        // Material belongs to the type: isolate this change from other terrain instances.
        var copy = (ToposolidType)type.Duplicate(type.Name.Substring(0, Math.Min(150, type.Name.Length)) + " Site Designer " + Guid.NewGuid().ToString("N").Substring(0,8));
        structure.SetMaterialId(0, materialId);
        copy.SetCompoundStructure(structure);
        surface.Element.ChangeTypeId(copy.Id);
#endif
    }
    public static void CopyMaterial(TopographySurface source, TopographySurface target) => SetMaterialId(target, GetMaterialId(source));
    public static ElementId HostId(TopographySurface surface)
    {
#if REVIT2022 || REVIT2023
        return surface.IsSiteSubRegion ? surface.AsSiteSubRegion().HostId : ElementId.InvalidElementId;
#else
        return surface.Element.HostTopoId;
#endif
    }
    public static void FlushEdits(Document document)
    {
#if !REVIT2022 && !REVIT2023
        TerrainEditScope.Flush(document);
#endif
    }
}
