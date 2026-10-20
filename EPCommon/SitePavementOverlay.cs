using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SITES.Utilities;

namespace EPCommon;

/// <summary>
/// Thin pavement solids for Toposolid. 2020 painted the TIN itself;
/// TerrainApi.Create on 2025 makes a second thick Toposolid (the rectangular box).
/// </summary>
internal static class SitePavementOverlay
{
	private const double MinCurveFt = 0.0026;
	private const double MinDepthFt = 0.04;

	public static DirectShape CreateExtrudedPad(Document document, Eps_Leg boundary, double depth, ElementId materialId, string name, BuiltInCategory category)
	{
		if (document == null || boundary == null) return null;
		CurveLoop loop = CurveLoopFromLeg(boundary);
		if (loop == null) return null;
		loop = FlattenToMaxZ(loop);
		if (loop == null) return null;
		depth = Math.Max(depth, MinDepthFt);
		SolidOptions options = SolidOptionsOrNull(materialId);
		Solid solid = TryExtrude(new List<CurveLoop> { loop }, depth, options);
		if (solid == null)
		{
			try
			{
				loop.Flip();
				solid = TryExtrude(new List<CurveLoop> { loop }, depth, options);
			}
			catch (Exception ex)
			{
				RuntimeDiagnostics.Write("Parking-lot pad reverse failed: " + ex.Message);
			}
		}
		if (solid == null) return null;
		return Place(document, solid, category, name);
	}

	public static List<Solid> SweepRibbon(Eps_Leg pathLeg, double width, double depth, ElementId materialId)
	{
		List<Solid> solids = new List<Solid>();
		if (pathLeg == null || width <= MinCurveFt) return solids;
		depth = Math.Max(depth, MinDepthFt);
		Solid strip = CreateOffsetStrip(pathLeg, width * 0.5, depth, materialId);
		if (strip != null)
		{
			solids.Add(strip);
			return solids;
		}
		CurveLoop path = CurveLoopFromLeg(pathLeg);
		if (path == null) return solids;
		SolidOptions options = SolidOptionsOrNull(materialId);
		Solid solid = TrySweep(path, width, depth, options);
		if (solid != null) solids.Add(solid);
		return solids;
	}

	public static Solid CreateOffsetStrip(Eps_Leg center, double halfWidth, double thickness, ElementId materialId)
	{
		if (center == null || halfWidth <= MinCurveFt) return null;
		Eps_Leg left = new Eps_Leg();
		Eps_Leg right = new Eps_Leg();
		if (RevitGeometry.OffsetLegList(center, halfWidth, 1, ref left, TrimClosedObject: true) != 0) return null;
		if (RevitGeometry.OffsetLegList(center, halfWidth, 2, ref right, TrimClosedObject: true) != 0) return null;
		return CreateStrip(left, right, thickness, materialId);
	}

	public static Solid CreateOffsetBand(Eps_Leg center, double innerOffset, double outerOffset, int offsetDir, double thickness, ElementId materialId)
	{
		if (center == null || outerOffset <= innerOffset + MinCurveFt) return null;
		Eps_Leg inner = new Eps_Leg();
		Eps_Leg outer = new Eps_Leg();
		if (RevitGeometry.OffsetLegList(center, innerOffset, offsetDir, ref inner, TrimClosedObject: true) != 0) return null;
		if (RevitGeometry.OffsetLegList(center, outerOffset, offsetDir, ref outer, TrimClosedObject: true) != 0) return null;
		return CreateStrip(inner, outer, thickness, materialId);
	}

	public static Solid CreateStrip(Eps_Leg first, Eps_Leg second, double thickness, ElementId materialId)
	{
		if (first == null || second == null) return null;
		thickness = Math.Max(thickness, MinDepthFt);
		SolidOptions options = SolidOptionsOrNull(materialId);
		bool closed = first.IsLegClosed() && second.IsLegClosed();
		if (closed)
		{
			CurveLoop loopA = CurveLoopFromLeg(first);
			CurveLoop loopB = CurveLoopFromLeg(second);
			if (loopA == null || loopB == null) return null;
			double z = Math.Max(MaxZ(loopA), MaxZ(loopB));
			loopA = FlattenToZ(loopA, z);
			loopB = FlattenToZ(loopB, z);
			if (loopA == null || loopB == null) return null;
			double lenA = RevitGeometry.eplGeoE_CalcLegListLength(first);
			double lenB = RevitGeometry.eplGeoE_CalcLegListLength(second);
			CurveLoop outer = lenA >= lenB ? loopA : loopB;
			CurveLoop inner = lenA >= lenB ? loopB : loopA;
			Solid solid = TryExtrude(new List<CurveLoop> { outer, inner }, thickness, options);
			if (solid != null) return solid;
			try { inner.Flip(); } catch (Exception) { }
			solid = TryExtrude(new List<CurveLoop> { outer, inner }, thickness, options);
			if (solid != null) return solid;
			try { outer.Flip(); } catch (Exception) { }
			return TryExtrude(new List<CurveLoop> { outer, inner }, thickness, options);
		}

		CurveLoop strip = OpenStripLoop(first, second);
		if (strip == null) return null;
		strip = FlattenToMaxZ(strip);
		Solid extruded = TryExtrude(new List<CurveLoop> { strip }, thickness, options);
		if (extruded != null) return extruded;
		try { strip.Flip(); } catch (Exception) { }
		return TryExtrude(new List<CurveLoop> { strip }, thickness, options);
	}

	public static Solid TrySweepProfile(CurveLoop path, CurveLoop profile, ElementId materialId)
	{
		if (path == null || profile == null) return null;
		CurveLoop aligned = AlignProfileToPath(path, profile) ?? profile;
		SolidOptions options = SolidOptionsOrNull(materialId);
		try
		{
			Curve first = FirstCurve(path);
			if (first == null) return null;
			double param = first.GetEndParameter(0);
			return options == null
				? GeometryCreationUtilities.CreateSweptGeometry(path, 0, param, new List<CurveLoop> { aligned })
				: GeometryCreationUtilities.CreateSweptGeometry(path, 0, param, new List<CurveLoop> { aligned }, options);
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("CreateSweptGeometry (type profile) failed: " + ex.Message);
			return null;
		}
	}

	public static CurveLoop CurveLoopFromSingleLeg(Eps_Leg leg)
	{
		if (leg == null) return null;
		Curve curve = CurveFromLeg(leg);
		if (curve == null) return null;
		CurveLoop loop = new CurveLoop();
		try
		{
			loop.Append(curve);
		}
		catch (Exception)
		{
			return null;
		}
		return loop;
	}

	public static DirectShape Place(Document document, Solid solid, BuiltInCategory category, string name)
	{
		if (document == null || solid == null) return null;
		DirectShape shape = DirectShape.CreateElement(document, RevitIds.Create(category));
		try
		{
			if (document.Application.ActiveAddInId != null)
				shape.ApplicationId = document.Application.ActiveAddInId.GetGUID().ToString();
		}
		catch (Exception)
		{
		}
		shape.SetShape(new GeometryObject[] { solid });
		if (!string.IsNullOrEmpty(name))
			shape.Name = name;
		return shape;
	}

	public static CurveLoop CurveLoopFromLeg(Eps_Leg leg)
	{
		if (leg == null) return null;
		CurveLoop loop = new CurveLoop();
		int count = 0;
		for (Eps_Leg current = leg; current != null; current = current.Next)
		{
			Curve curve = CurveFromLeg(current);
			if (curve == null) continue;
			try
			{
				loop.Append(curve);
				count++;
			}
			catch (Exception ex)
			{
				RuntimeDiagnostics.Write("Skipped host segment while building pavement path: " + ex.Message);
			}
		}
		if (count == 0) return null;
		return loop;
	}

	internal static void DeleteByUniqueId(Document document, string uniqueId)
	{
		if (document == null || string.IsNullOrEmpty(uniqueId)) return;
		Element element = document.GetElement(uniqueId);
		if (element == null)
			element = RevitUtility.FindDirectShape(document, uniqueId);
		if (element == null)
		{
			TopographySurface surface = TopographySurfaceManagement.FindTopoSurfaceInstance(document, uniqueId);
			if (surface != null)
			{
				try { document.Delete(surface.Id); }
				catch (Exception ex) { RuntimeDiagnostics.Write("Could not delete pavement overlay " + uniqueId + ": " + ex.Message); }
				return;
			}
		}
		if (element == null) return;
		try
		{
			document.Delete(element.Id);
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("Could not delete pavement overlay " + uniqueId + ": " + ex.Message);
		}
	}

	private static Curve CurveFromLeg(Eps_Leg leg)
	{
		if (leg == null) return null;
		if (leg.Line != null)
		{
			XYZ start = leg.Line.StartPnt.XYZ;
			XYZ end = leg.Line.EndPnt.XYZ;
			if (start.DistanceTo(end) < MinCurveFt) return null;
			return Line.CreateBound(start, end);
		}
		if (leg.Arc != null)
			return ArcFromEps(leg.Arc);
		return null;
	}

	private static Arc ArcFromEps(Eps_Arc arc)
	{
		if (arc == null) return null;
		if (!EPShared.eplGeoE_ArePntsDuplicates2D(arc.StartPnt, arc.EndPnt))
		{
			double half = arc.Radius * arc.Delta / 2.0;
			Eps_Point mid = new Eps_Point();
			EPShared.eplGeoE_FindStaOffPointAlongArc(arc, half, 0.0, ref mid);
			if (arc.StartPnt.XYZ.DistanceTo(arc.EndPnt.XYZ) < MinCurveFt) return null;
			return Arc.Create(arc.StartPnt.XYZ, arc.EndPnt.XYZ, mid.XYZ);
		}
		return Arc.Create(arc.RadiusPnt.XYZ, Math.Max(arc.Radius, MinCurveFt), 0.0, Math.PI * 2.0, XYZ.BasisX, XYZ.BasisY);
	}

	private static CurveLoop FlattenToMaxZ(CurveLoop source)
	{
		double z = double.MinValue;
		List<Curve> curves = new List<Curve>();
		foreach (Curve curve in source)
		{
			curves.Add(curve);
			z = Math.Max(z, curve.GetEndPoint(0).Z);
			z = Math.Max(z, curve.GetEndPoint(1).Z);
		}
		if (curves.Count == 0 || z == double.MinValue) return null;
		CurveLoop flat = new CurveLoop();
		foreach (Curve curve in curves)
		{
			XYZ a = AtZ(curve.GetEndPoint(0), z);
			XYZ b = AtZ(curve.GetEndPoint(1), z);
			if (a.DistanceTo(b) < MinCurveFt) continue;
			if (curve is Arc)
			{
				XYZ mid = AtZ(curve.Evaluate(0.5, true), z);
				flat.Append(Arc.Create(a, b, mid));
			}
			else
			{
				flat.Append(Line.CreateBound(a, b));
			}
		}
		return flat;
	}

	private static XYZ AtZ(XYZ point, double z) => new XYZ(point.X, point.Y, z);

	private static CurveLoop OpenStripLoop(Eps_Leg left, Eps_Leg right)
	{
		CurveLoop leftLoop = CurveLoopFromLeg(left);
		CurveLoop rightLoop = CurveLoopFromLeg(right);
		if (leftLoop == null || rightLoop == null) return null;
		Eps_Leg reversed = right.Reverse();
		CurveLoop rightBack = CurveLoopFromLeg(reversed);
		if (rightBack == null) return null;
		try
		{
			CurveLoop strip = new CurveLoop();
			AppendCloned(strip, leftLoop);
			XYZ leftEnd = left.LegEndPnt.XYZ;
			XYZ rightEnd = right.LegEndPnt.XYZ;
			if (leftEnd.DistanceTo(rightEnd) >= MinCurveFt)
				strip.Append(Line.CreateBound(leftEnd, rightEnd));
			AppendCloned(strip, rightBack);
			XYZ rightStart = right.LegStartPnt.XYZ;
			XYZ leftStart = left.LegStartPnt.XYZ;
			if (rightStart.DistanceTo(leftStart) >= MinCurveFt)
				strip.Append(Line.CreateBound(rightStart, leftStart));
			return strip;
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("Open street strip failed: " + ex.Message);
			return null;
		}
	}

	private static void AppendCloned(CurveLoop target, CurveLoop source)
	{
		foreach (Curve curve in source)
			target.Append(curve.Clone());
	}

	private static double MaxZ(CurveLoop loop)
	{
		double z = double.MinValue;
		foreach (Curve curve in loop)
		{
			z = Math.Max(z, curve.GetEndPoint(0).Z);
			z = Math.Max(z, curve.GetEndPoint(1).Z);
		}
		return z;
	}

	private static CurveLoop FlattenToZ(CurveLoop source, double z)
	{
		if (source == null) return null;
		CurveLoop flat = new CurveLoop();
		foreach (Curve curve in source)
		{
			XYZ a = AtZ(curve.GetEndPoint(0), z);
			XYZ b = AtZ(curve.GetEndPoint(1), z);
			if (a.DistanceTo(b) < MinCurveFt) continue;
			if (curve is Arc)
			{
				XYZ mid = AtZ(curve.Evaluate(0.5, true), z);
				flat.Append(Arc.Create(a, b, mid));
			}
			else
			{
				flat.Append(Line.CreateBound(a, b));
			}
		}
		return flat;
	}

	private static Solid TryExtrude(IList<CurveLoop> loops, double depth, SolidOptions options)
	{
		try
		{
			return options == null
				? GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ.Negate(), depth)
				: GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ.Negate(), depth, options);
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("CreateExtrusionGeometry failed: " + ex.Message);
			return null;
		}
	}

	private static Solid TrySweep(CurveLoop path, double width, double depth, SolidOptions options)
	{
		CurveLoop profile = ProfileAtPathStart(path, width, depth);
		if (profile == null) return null;
		try
		{
			Curve first = FirstCurve(path);
			double param = first.GetEndParameter(0);
			return options == null
				? GeometryCreationUtilities.CreateSweptGeometry(path, 0, param, new List<CurveLoop> { profile })
				: GeometryCreationUtilities.CreateSweptGeometry(path, 0, param, new List<CurveLoop> { profile }, options);
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("CreateSweptGeometry failed: " + ex.Message);
			return null;
		}
	}

	private static List<Solid> SweepEachSegment(CurveLoop path, double width, double depth, SolidOptions options)
	{
		List<Solid> parts = new List<Solid>();
		foreach (Curve curve in path)
		{
			if (curve == null || curve.Length < MinCurveFt) continue;
			CurveLoop segment = new CurveLoop();
			try
			{
				segment.Append(curve.Clone());
			}
			catch (Exception)
			{
				continue;
			}
			Solid part = TrySweep(segment, width, depth, options);
			if (part != null) parts.Add(part);
		}
		return parts;
	}

	private static CurveLoop AlignProfileToPath(CurveLoop path, CurveLoop profile)
	{
		Curve first = FirstCurve(path);
		if (first == null || profile == null) return profile;
		XYZ origin = first.GetEndPoint(0);
		XYZ tangent = first.ComputeDerivatives(first.GetEndParameter(0), false).BasisX;
		if (tangent.IsZeroLength()) return profile;
		tangent = tangent.Normalize();
		List<XYZ> points = new List<XYZ>();
		foreach (Curve curve in profile)
		{
			XYZ point = curve.GetEndPoint(0);
			XYZ projected = point - tangent.Multiply(tangent.DotProduct(point - origin));
			if (points.Count == 0 || points[points.Count - 1].DistanceTo(projected) > MinCurveFt)
				points.Add(projected);
		}
		if (points.Count < 3) return profile;
		if (points[0].DistanceTo(points[points.Count - 1]) < MinCurveFt)
			points.RemoveAt(points.Count - 1);
		if (points.Count < 3) return profile;
		try
		{
			CurveLoop aligned = new CurveLoop();
			for (int i = 0; i < points.Count; i++)
			{
				XYZ a = points[i];
				XYZ b = points[(i + 1) % points.Count];
				if (a.DistanceTo(b) < MinCurveFt) continue;
				aligned.Append(Line.CreateBound(a, b));
			}
			return aligned;
		}
		catch (Exception)
		{
			return profile;
		}
	}

	private static CurveLoop ProfileAtPathStart(CurveLoop path, double width, double depth)
	{
		Curve first = FirstCurve(path);
		if (first == null) return null;
		XYZ origin = first.GetEndPoint(0);
		XYZ tangent = first.ComputeDerivatives(first.GetEndParameter(0), false).BasisX;
		if (tangent.IsZeroLength()) return null;
		tangent = tangent.Normalize();
		XYZ right = tangent.CrossProduct(XYZ.BasisZ);
		if (right.IsZeroLength()) right = XYZ.BasisY;
		right = right.Normalize();
		XYZ up = right.CrossProduct(tangent);
		if (up.IsZeroLength()) up = XYZ.BasisZ;
		up = up.Normalize();
		if (up.Z < 0.0) up = up.Negate();
		double half = width * 0.5;
		XYZ p0 = origin + right * (-half);
		XYZ p1 = origin + right * half;
		XYZ p2 = origin + right * half - up * depth;
		XYZ p3 = origin + right * (-half) - up * depth;
		CurveLoop profile = new CurveLoop();
		profile.Append(Line.CreateBound(p0, p1));
		profile.Append(Line.CreateBound(p1, p2));
		profile.Append(Line.CreateBound(p2, p3));
		profile.Append(Line.CreateBound(p3, p0));
		return profile;
	}

	private static Curve FirstCurve(CurveLoop path)
	{
		foreach (Curve curve in path)
			return curve;
		return null;
	}

	private static SolidOptions SolidOptionsOrNull(ElementId materialId)
	{
		if (materialId == null || materialId == ElementId.InvalidElementId || materialId.Value64() <= 0)
			return null;
		return new SolidOptions(materialId, ElementId.InvalidElementId);
	}
}
