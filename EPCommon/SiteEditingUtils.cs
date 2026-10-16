using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace EPCommon;

internal class SiteEditingUtils
{
	public static XYZ MoveXYZToElevation(XYZ input, double elevation)
	{
		return input + XYZ.BasisZ * (elevation - input.Z);
	}

	public static double GetAverageElevation(IList<XYZ> existingPoints)
	{
		if (existingPoints.Count == 0)
		{
			return 0.0;
		}
		return existingPoints.Average((XYZ xyz) => xyz.Z);
	}

	public static XYZ GetCenterOf(Element element)
	{
		BoundingBoxXYZ boundingBoxXYZ = element.get_BoundingBox((View)null);
		return (boundingBoxXYZ.Min + boundingBoxXYZ.Max) / 2.0;
	}

	public static IList<XYZ> GetPointsFromSubregionRough(TopographySurface subregion)
	{
		BoundingBoxXYZ boundingBoxXYZ = ((Element)subregion).get_BoundingBox((View)null);
		TopographySurface topographySurfaceHost = GetTopographySurfaceHost(subregion);
		Outline boundingBox = new Outline(boundingBoxXYZ.Min, boundingBoxXYZ.Max);
		return topographySurfaceHost.FindPoints(boundingBox);
	}

	public static IList<XYZ> GetPointsFromSubregionExact(TopographySurface subregion)
	{
		return subregion.GetPoints();
	}

	public static TopographySurface GetTopographySurfaceHost(TopographySurface subregion)
	{
		return TerrainApi.AsSurface(subregion.Document.GetElement(TerrainApi.HostId(subregion)));
	}

	public static IList<XYZ> GetNonBoundaryPoints(TopographySurface toposurface)
	{
		return toposurface.GetInteriorPoints();
	}
}
