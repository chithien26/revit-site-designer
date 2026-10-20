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
    public static TopographySurface Create(Document document, IList<XYZ> points, TopographySurface template = null, bool copyHostSketch = true)
    {
#if REVIT2022 || REVIT2023
        return TopographySurface.Create(document, points);
#else
        return ToposolidSurface.Create(document, points, template, copyHostSketch);
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
    public static void SetUniqueName(Element element, string name)
    {
        var parameter = NameParameter(element);
        if (parameter == null || parameter.IsReadOnly) return;
        if (string.IsNullOrEmpty(name)) name = "Terrain";
#if !REVIT2022 && !REVIT2023
        if (element is Toposolid)
        {
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var other in new FilteredElementCollector(element.Document).OfClass(typeof(Toposolid)))
            {
                if (other.Id == element.Id) continue;
                var existing = NameParameter(other)?.AsString();
                if (!string.IsNullOrEmpty(existing)) used.Add(existing);
            }
            string candidate = name;
            int suffix = 2;
            while (used.Contains(candidate))
            {
                candidate = name + " " + suffix;
                suffix++;
            }
            name = candidate;
        }
#endif
        parameter.Set(name);
    }
    public static bool IsTerrain(Element element) => AsSurface(element) != null;
    public static bool IsProposedTerrain(Element element)
    {
        var name = NameParameter(element)?.AsString();
        if (string.IsNullOrEmpty(name)) return false;
        string key = "Proposed Toposurface";
        try
        {
            var localized = EPShared.LocalRM?.GetString("ProposedToposurfaceString");
            if (!string.IsNullOrEmpty(localized)) key = localized;
        }
        catch
        {
        }
        return name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0;
    }
    public static bool MatchesLinkedId(string stored, Element element)
    {
        if (string.IsNullOrEmpty(stored) || element == null) return false;
        if (string.Equals(stored, element.UniqueId, StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(stored, element.Id.Value64().ToString(), StringComparison.OrdinalIgnoreCase)) return true;
        var surface = AsSurface(element);
        return surface != null && string.Equals(stored, surface.UniqueId, StringComparison.OrdinalIgnoreCase);
    }

    public static bool BoundingBoxesOverlapXY(BoundingBoxXYZ a, BoundingBoxXYZ b)
    {
        if (a == null || b == null) return false;
        return a.Min.X <= b.Max.X && a.Max.X >= b.Min.X && a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y;
    }

    public static bool ContainsXY(TopographySurface surface, XYZ point)
    {
        if (surface == null || point == null) return false;
#if REVIT2022 || REVIT2023
        return true;
#else
        const double tolerance = 0.05;
        try
        {
            foreach (Reference reference in HostObjectUtils.GetTopFaces(surface.Element))
            {
                if (surface.Element.GetGeometryObjectFromReference(reference) is not Face face) continue;
                BoundingBoxUV box = face.GetBoundingBox();
                XYZ mid = face.Evaluate((box.Min + box.Max) / 2);
                IntersectionResult projection = face.Project(new XYZ(point.X, point.Y, mid.Z));
                if (projection == null) continue;
                XYZ at = face.Evaluate(projection.UVPoint);
                if (Math.Abs(at.X - point.X) <= tolerance && Math.Abs(at.Y - point.Y) <= tolerance && face.IsInside(projection.UVPoint))
                    return true;
            }
        }
        catch (Autodesk.Revit.Exceptions.ArgumentException)
        {
        }
        try
        {
            IList<CurveLoop> loops = surface.GetProfiles();
            if (loops == null || loops.Count == 0) return false;
            if (!LoopContainsXY(loops[0], point, tolerance, includeBoundary: true)) return false;
            for (int i = 1; i < loops.Count; i++)
            {
                if (LoopContainsXY(loops[i], point, tolerance, includeBoundary: false)) return false;
            }
            return true;
        }
        catch
        {
            return false;
        }
#endif
    }

    public static bool TryGetTopZ(TopographySurface surface, XYZ point, out double z)
    {
        z = 0;
        if (surface == null || point == null) return false;
#if REVIT2022 || REVIT2023
        return false;
#else
        const double tolerance = 0.05;
        double best = double.MinValue;
        bool hit = false;
        try
        {
            foreach (Reference reference in HostObjectUtils.GetTopFaces(surface.Element))
            {
                if (surface.Element.GetGeometryObjectFromReference(reference) is not Face face) continue;
                BoundingBoxUV box = face.GetBoundingBox();
                XYZ mid = face.Evaluate((box.Min + box.Max) / 2);
                IntersectionResult projection = face.Project(new XYZ(point.X, point.Y, mid.Z));
                if (projection == null) continue;
                XYZ at = face.Evaluate(projection.UVPoint);
                if (Math.Abs(at.X - point.X) > tolerance || Math.Abs(at.Y - point.Y) > tolerance || !face.IsInside(projection.UVPoint)) continue;
                if (at.Z >= best)
                {
                    best = at.Z;
                    hit = true;
                }
            }
        }
        catch (Autodesk.Revit.Exceptions.ArgumentException)
        {
        }
        if (!hit) return false;
        z = best;
        return true;
#endif
    }

    private static bool LoopContainsXY(CurveLoop loop, XYZ point, double tolerance, bool includeBoundary)
    {
        var vertices = new List<XYZ>();
        foreach (Curve curve in loop)
        {
            vertices.Add(curve.GetEndPoint(0));
            if (!(curve is Line))
            {
                IList<XYZ> tessellated = curve.Tessellate();
                for (int i = 1; i < tessellated.Count - 1; i++) vertices.Add(tessellated[i]);
            }
        }
        if (vertices.Count < 3) return false;
        for (int i = 0; i < vertices.Count; i++)
        {
            XYZ a = vertices[i];
            XYZ b = vertices[(i + 1) % vertices.Count];
            if (Math.Abs((b.X - a.X) * (point.Y - a.Y) - (b.Y - a.Y) * (point.X - a.X)) <= tolerance
                && point.X >= Math.Min(a.X, b.X) - tolerance && point.X <= Math.Max(a.X, b.X) + tolerance
                && point.Y >= Math.Min(a.Y, b.Y) - tolerance && point.Y <= Math.Max(a.Y, b.Y) + tolerance)
                return includeBoundary;
        }
        bool inside = false;
        for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
        {
            XYZ a = vertices[i];
            XYZ b = vertices[j];
            if ((a.Y > point.Y) != (b.Y > point.Y)
                && point.X < (b.X - a.X) * (point.Y - a.Y) / ((b.Y - a.Y) == 0 ? 1 : (b.Y - a.Y)) + a.X)
                inside = !inside;
        }
        return inside;
    }

    public static bool LooksLikeProposedTerrain(Element element)
    {
        if (element == null) return false;
        if (IsProposedTerrain(element)) return true;
        string typeName = element.Document.GetElement(element.GetTypeId())?.Name ?? string.Empty;
        return typeName.IndexOf("(Site Designer)", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static TopographySurface FindProposedForBase(Document document, TopographySurface baseSurface)
    {
        if (document == null || baseSurface == null) return null;
        BoundingBoxXYZ baseBox = baseSurface.get_BoundingBox(null);
        TopographySurface named = null;
        TopographySurface typed = null;
        foreach (Element element in new FilteredElementCollector(document).OfClass(ElementType).OfCategory(Category))
        {
            if (element == null || element.Id == baseSurface.Id) continue;
            TopographySurface surface = AsSurface(element);
            if (surface == null || surface.IsSiteSubRegion) continue;
            if (!BoundingBoxesOverlapXY(baseBox, surface.get_BoundingBox(null))) continue;
            if (IsProposedTerrain(element))
            {
                named = surface;
                break;
            }
            if (LooksLikeProposedTerrain(element)) typed ??= surface;
        }
        return named ?? typed;
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
