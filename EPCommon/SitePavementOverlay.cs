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
		depth = Math.Max(depth, MinDepthFt);
		SolidOptions options = SolidOptionsOrNull(materialId);
		Solid solid = TryExtrudeLoop(CurveLoopFromLeg(boundary), depth, options);
		if (solid == null)
			solid = TryExtrudeLoop(PolygonFromLeg(boundary), depth, options);
		if (solid == null)
		{
			RuntimeDiagnostics.Write("Parking-lot pad extrusion failed.");
			return null;
		}
		return Place(document, solid, category, name);
	}

	public static List<Solid> SweepRibbon(Eps_Leg pathLeg, double width, double depth, ElementId materialId)
	{
		List<Solid> solids = new List<Solid>();
		if (pathLeg == null || width <= MinCurveFt) return solids;
		depth = Math.Max(depth, MinDepthFt);
		SolidOptions options = SolidOptionsOrNull(materialId);
		CurveLoop path = CurveLoopFromLeg(pathLeg);
		if (path != null)
		{
			Solid swept = TrySweep(path, width, depth, options);
			if (swept != null)
			{
				solids.Add(swept);
				return solids;
			}
		}
		if (LooksClosed(pathLeg))
		{
			Solid ring = TryClosedRing(pathLeg, width * 0.5, depth, options);
			if (ring != null)
			{
				solids.Add(ring);
				return solids;
			}
			Solid pair = TryOffsetPair(pathLeg, width * 0.5, depth, options);
			if (pair != null)
			{
				solids.Add(pair);
				return solids;
			}
		}
		else
		{
			Solid strip = CreateOffsetStrip(pathLeg, width * 0.5, depth, materialId);
			if (strip != null)
			{
				solids.Add(strip);
				return solids;
			}
		}
		solids.AddRange(SweepEachSegment(path ?? CurveLoopFromLeg(pathLeg), width, depth, options));
		if (solids.Count == 0)
		{
			Eps_Leg flattened = pathLeg.FlattenArcsNarrowerThan(width * 0.5 + 2.0) ?? pathLeg;
			solids.AddRange(SweepEachSegment(CurveLoopFromLeg(flattened), width, depth, options));
		}
		if (solids.Count == 0)
			solids.AddRange(TessellatedRibbon(pathLeg, width, depth, options));
		if (solids.Count > 0)
			RuntimeDiagnostics.Write("Street per-segment ribbon solids=" + solids.Count + " width=" + width + " ft.");
		return solids;
	}

	public static Eps_Leg ExtractBetweenStations(Eps_Leg source, double startStation, double endStation)
	{
		if (source == null || endStation <= startStation + MinCurveFt) return null;
		Eps_Leg head = null;
		double cursor = 0.0;
		for (Eps_Leg current = source; current != null; current = current.Next)
		{
			double length = RevitGeometry.eplGeoE_CalcLegLength(current);
			double segStart = cursor;
			double segEnd = cursor + length;
			cursor = segEnd;
			if (segEnd <= startStation + MinCurveFt || segStart >= endStation - MinCurveFt) continue;
			double from = Math.Max(startStation, segStart) - segStart;
			double to = Math.Min(endStation, segEnd) - segStart;
			Eps_Leg piece = SliceLeg(current, from, to);
			if (piece == null) continue;
			if (head == null) head = piece;
			else head.Last.Next = piece;
		}
		return head;
	}

	public static Solid CreateIntersectionPad(IntersectionContainer container, double depth, ElementId materialId)
	{
		if (container == null) return null;
		depth = Math.Max(depth, MinDepthFt);
		SolidOptions options = SolidOptionsOrNull(materialId);
		if (container.LeftOffsetLeg != null && container.RightOffsetLeg != null)
		{
			Solid sampled = TrySampledPair(container.LeftOffsetLeg, container.RightOffsetLeg, depth, options);
			if (sampled != null) return sampled;
		}
		List<XYZ> left = new List<XYZ>();
		List<XYZ> right = new List<XYZ>();
		if (container.Profiles != null)
		{
			foreach (ProfileContainer profile in container.Profiles)
			{
				if (profile?.Points == null || profile.Points.Count < 2) continue;
				Eps_Point first = profile.Points[0];
				Eps_Point last = profile.Points[profile.Points.Count - 1];
				if (first == null || last == null) continue;
				XYZ a = new XYZ(first.x, first.y, first.z);
				XYZ b = new XYZ(last.x, last.y, last.z);
				if (left.Count == 0 || left[left.Count - 1].DistanceTo(a) >= MinCurveFt) left.Add(a);
				if (right.Count == 0 || right[right.Count - 1].DistanceTo(b) >= MinCurveFt) right.Add(b);
			}
		}
		if (left.Count < 2 || right.Count < 2) return null;
		List<XYZ> ring = new List<XYZ>();
		ring.AddRange(left);
		for (int i = right.Count - 1; i >= 0; i--)
			ring.Add(right[i]);
		return TryExtrudeLoop(LoopFromPoints(ring, true), depth, options);
	}

	public static int PlaceNetworkJunctions(Document document, List<Eps_Leg> legs, double halfWidth, double depth, ElementId materialId)
	{
		if (document == null || legs == null || legs.Count < 2 || halfWidth <= MinCurveFt) return 0;
		depth = Math.Max(depth, MinDepthFt);
		SolidOptions options = SolidOptionsOrNull(materialId);
		int placed = 0;
		for (int i = 0; i < legs.Count; i++)
		{
			for (int j = i + 1; j < legs.Count; j++)
			{
				if (!TryMeetPoint(legs[i], legs[j], out XYZ point)) continue;
				Solid pad = CreateJunctionPadSolid(point, halfWidth, depth, options);
				if (pad == null) continue;
				if (Place(document, pad, BuiltInCategory.OST_Roads, "Street Junction") != null)
					placed++;
			}
		}
		if (placed > 0)
			RuntimeDiagnostics.Write("Street network junction pads=" + placed + " halfWidth=" + halfWidth + " ft.");
		return placed;
	}

	private static bool TryMeetPoint(Eps_Leg first, Eps_Leg second, out XYZ point)
	{
		point = null;
		if (first == null || second == null) return false;
		List<Eps_Point> hits = new List<Eps_Point>();
		int count = 0;
		if (RevitGeometry.eplGeoE_IntersectTwoLegLists(first, second, ref hits, ref count, 0.05) == 0 && count > 0 && hits[0] != null)
		{
			point = hits[0].XYZ;
			return true;
		}
		if (EndOnLeg(first.LegStartPnt, second, out point) || EndOnLeg(first.LegEndPnt, second, out point))
			return true;
		if (EndOnLeg(second.LegStartPnt, first, out point) || EndOnLeg(second.LegEndPnt, first, out point))
			return true;
		return false;
	}

	private static bool EndOnLeg(Eps_Point end, Eps_Leg target, out XYZ point)
	{
		point = null;
		if (end == null || target == null) return false;
		double station = 0.0;
		double offset = 0.0;
		if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(end, target, ref station, ref offset) != 1700)
			return false;
		if (Math.Abs(offset) > 1.0) return false;
		Eps_Point snapped = new Eps_Point();
		if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(target, station, 0.0, ref snapped) != 0)
			return false;
		point = snapped.XYZ;
		return true;
	}

	private static Solid CreateJunctionPadSolid(XYZ center, double halfWidth, double depth, SolidOptions options)
	{
		double reach = Math.Max(halfWidth * 1.15, 2.0);
		List<XYZ> ring = new List<XYZ>();
		int steps = 16;
		double z = center.Z;
		for (int i = 0; i < steps; i++)
		{
			double angle = Math.PI * 2.0 * i / steps;
			ring.Add(new XYZ(center.X + reach * Math.Cos(angle), center.Y + reach * Math.Sin(angle), z));
		}
		return TryExtrudeLoop(LoopFromPoints(ring, true), depth, options);
	}

	public static Solid CreateOffsetStrip(Eps_Leg center, double halfWidth, double thickness, ElementId materialId)
	{
		if (center == null || halfWidth <= MinCurveFt) return null;
		thickness = Math.Max(thickness, MinDepthFt);
		SolidOptions options = SolidOptionsOrNull(materialId);
		// Closed / nearly-closed centerlines must stay a picture-frame ring.
		// CreateViaThicken of a closed loop is a single outer loop → filled plate.
		if (LooksClosed(center))
		{
			Solid ring = TryClosedRing(center, halfWidth, thickness, options);
			if (ring != null) return ring;
			Solid pair = TryOffsetPair(center, halfWidth, thickness, options);
			if (pair != null) return pair;
			List<Solid> tessellated = TessellatedRibbon(center, halfWidth * 2.0, thickness, options);
			if (tessellated.Count > 0) return tessellated[0];
			RuntimeDiagnostics.Write("Closed street ring failed; halfWidth=" + halfWidth + " ft.");
			return null;
		}
		Solid thickened = TryThicken(center, halfWidth * 2.0, thickness, options);
		if (thickened != null) return thickened;
		Solid sampled = TrySampledStrip(center, halfWidth, thickness, options);
		if (sampled != null) return sampled;
		Eps_Leg left = new Eps_Leg();
		Eps_Leg right = new Eps_Leg();
		if (RevitGeometry.OffsetLegList(center, halfWidth, 1, ref left, TrimClosedObject: true) != 0) return null;
		if (RevitGeometry.OffsetLegList(center, halfWidth, 2, ref right, TrimClosedObject: true) != 0) return null;
		return CreateStrip(left, right, thickness, materialId);
	}

	public static Eps_Leg FilletLineCorners(Eps_Leg source, double preferredRadius)
	{
		if (source == null || preferredRadius < 0.5) return source;
		List<Eps_Leg> segs = new List<Eps_Leg>();
		for (Eps_Leg current = source; current != null; current = current.Next)
			segs.Add(Eps_Leg.SingleLeg(current));
		if (segs.Count < 2) return source;
		bool closed = LooksClosed(source);
		List<Eps_Leg> result = new List<Eps_Leg>();
		int count = segs.Count;
		int last = closed ? count : count - 1;
		for (int i = 0; i < last; i++)
		{
			Eps_Leg first = segs[i];
			Eps_Leg second = segs[(i + 1) % count];
			if (first.Line == null || second.Line == null)
			{
				result.Add(first);
				continue;
			}
			double len1 = 0.0;
			double len2 = 0.0;
			EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(first.Line.StartPnt, first.Line.EndPnt, ref len1);
			EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(second.Line.StartPnt, second.Line.EndPnt, ref len2);
			double radius = Math.Min(preferredRadius, Math.Min(len1, len2) * 0.4);
			Eps_Arc arc = new Eps_Arc();
			if (radius < 0.5 || RevitGeometry.SolveArcFromTangentLines(first.Line, second.Line, radius, ref arc, _bVerifyTangentLength: true) != 0)
			{
				result.Add(first);
				continue;
			}
			first.Line.EndPnt = new Eps_Point(arc.StartPnt);
			second.Line.StartPnt = new Eps_Point(arc.EndPnt);
			result.Add(first);
			Eps_Leg corner = new Eps_Leg();
			corner.Arc = arc;
			result.Add(corner);
		}
		if (!closed)
			result.Add(segs[segs.Count - 1]);
		if (result.Count == 0) return source;
		for (int i = 0; i < result.Count - 1; i++)
			result[i].Next = result[i + 1];
		result[result.Count - 1].Next = null;
		return result[0];
	}

	public static Solid CreateOffsetBand(Eps_Leg center, double innerOffset, double outerOffset, int offsetDir, double thickness, ElementId materialId)
	{
		if (center == null || outerOffset <= innerOffset + MinCurveFt) return null;
		thickness = Math.Max(thickness, MinDepthFt);
		SolidOptions options = SolidOptionsOrNull(materialId);
		Eps_Leg inner = new Eps_Leg();
		Eps_Leg outer = new Eps_Leg();
		if (RevitGeometry.OffsetLegList(center, innerOffset, offsetDir, ref inner, TrimClosedObject: true) != 0) return null;
		if (RevitGeometry.OffsetLegList(center, outerOffset, offsetDir, ref outer, TrimClosedObject: true) != 0) return null;
		Solid sampled = TrySampledPair(inner, outer, thickness, options);
		if (sampled != null) return sampled;
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

	public static bool PathLooksClosed(Eps_Leg center) => LooksClosed(center);

	private static bool LooksClosed(Eps_Leg center)
	{
		if (center == null) return false;
		if (center.IsLegClosed()) return true;
		if (center.LegStartPnt == null || center.LegEndPnt == null) return false;
		double gap = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(center.LegStartPnt, center.LegEndPnt, ref gap);
		return gap < 1.0;
	}

	private static Solid TryOffsetPair(Eps_Leg center, double halfWidth, double thickness, SolidOptions options)
	{
		if (center == null || halfWidth <= MinCurveFt) return null;
		double tryHalf = halfWidth;
		for (int attempt = 0; attempt < 3; attempt++)
		{
			Eps_Leg left = new Eps_Leg();
			Eps_Leg right = new Eps_Leg();
			if (RevitGeometry.OffsetLegList(center, tryHalf, 1, ref left, TrimClosedObject: true) == 0
				&& RevitGeometry.OffsetLegList(center, tryHalf, 2, ref right, TrimClosedObject: true) == 0)
			{
				Solid sampled = TrySampledPair(left, right, thickness, options);
				if (sampled != null)
				{
					RuntimeDiagnostics.Write("Street offset-pair ring halfWidth=" + tryHalf + " ft.");
					return sampled;
				}
				Solid strip = CreateStrip(left, right, thickness, ElementId.InvalidElementId);
				if (strip != null)
				{
					RuntimeDiagnostics.Write("Street offset-pair strip halfWidth=" + tryHalf + " ft.");
					return strip;
				}
			}
			tryHalf *= 0.6;
			if (tryHalf <= MinCurveFt) break;
		}
		return null;
	}

	private static Solid TryClosedRing(Eps_Leg center, double halfWidth, double thickness, SolidOptions options)
	{
		List<XYZ> spine = SampleXy(center);
		if (spine.Count < 3) return null;
		if (spine[0].DistanceTo(spine[spine.Count - 1]) >= MinCurveFt)
			spine.Add(spine[0]);
		double minX = spine[0].X, maxX = spine[0].X, minY = spine[0].Y, maxY = spine[0].Y;
		for (int i = 1; i < spine.Count; i++)
		{
			minX = Math.Min(minX, spine[i].X);
			maxX = Math.Max(maxX, spine[i].X);
			minY = Math.Min(minY, spine[i].Y);
			maxY = Math.Max(maxY, spine[i].Y);
		}
		double inradius = 0.35 * Math.Min(maxX - minX, maxY - minY);
		double innerHalf = halfWidth;
		if (inradius > 0.5) innerHalf = Math.Min(halfWidth, inradius);
		List<XYZ> left = OffsetClosedPolyline(spine, halfWidth);
		List<XYZ> right = OffsetClosedPolyline(spine, -innerHalf);
		if (left == null || right == null || left.Count < 3 || right.Count < 3) return null;
		CurveLoop loopA = LoopFromPoints(left, true);
		CurveLoop loopB = LoopFromPoints(right, true);
		if (loopA == null || loopB == null) return null;
		double lenA = PolylineLength(left);
		double lenB = PolylineLength(right);
		CurveLoop outer = lenA >= lenB ? loopA : loopB;
		CurveLoop inner = lenA >= lenB ? loopB : loopA;
		Solid solid = TryExtrude(new List<CurveLoop> { outer, inner }, thickness, options);
		if (solid != null)
		{
			RuntimeDiagnostics.Write("Street closed ring outer=" + Math.Max(lenA, lenB) + " inner=" + Math.Min(lenA, lenB) + " half=" + halfWidth + " innerHalf=" + innerHalf + " ft.");
			return solid;
		}
		try { inner.Flip(); } catch (Exception) { }
		solid = TryExtrude(new List<CurveLoop> { outer, inner }, thickness, options);
		if (solid != null) return solid;
		try { outer.Flip(); } catch (Exception) { }
		return TryExtrude(new List<CurveLoop> { outer, inner }, thickness, options);
	}

	private static List<XYZ> OffsetClosedPolyline(List<XYZ> closed, double distance)
	{
		if (closed == null || closed.Count < 4 || Math.Abs(distance) < MinCurveFt) return null;
		int n = closed.Count - 1;
		if (closed[0].DistanceTo(closed[n]) >= MinCurveFt) n = closed.Count;
		List<XYZ> result = new List<XYZ>();
		for (int i = 0; i < n; i++)
		{
			XYZ prev = closed[(i - 1 + n) % n];
			XYZ cur = closed[i];
			XYZ next = closed[(i + 1) % n];
			XYZ d0 = HorzDir(prev, cur);
			XYZ d1 = HorzDir(cur, next);
			if (d0.IsZeroLength()) d0 = d1;
			if (d1.IsZeroLength()) d1 = d0;
			if (d0.IsZeroLength() || d1.IsZeroLength()) continue;
			XYZ n0 = new XYZ(-d0.Y, d0.X, 0.0);
			XYZ n1 = new XYZ(-d1.Y, d1.X, 0.0);
			XYZ miter = n0.Add(n1);
			if (miter.GetLength() < 1e-8) miter = n0;
			else miter = miter.Normalize();
			double denom = Math.Abs(miter.DotProduct(n0));
			if (denom < 0.25) denom = 0.25;
			XYZ offset = miter.Multiply(distance / denom);
			result.Add(new XYZ(cur.X + offset.X, cur.Y + offset.Y, cur.Z));
		}
		return result.Count >= 3 ? result : null;
	}

	private static XYZ HorzDir(XYZ from, XYZ to)
	{
		XYZ v = new XYZ(to.X - from.X, to.Y - from.Y, 0.0);
		double length = Math.Sqrt(v.X * v.X + v.Y * v.Y);
		if (length < MinCurveFt) return XYZ.Zero;
		return new XYZ(v.X / length, v.Y / length, 0.0);
	}

	private static double PolylineLength(List<XYZ> points)
	{
		if (points == null || points.Count < 2) return 0.0;
		double length = 0.0;
		for (int i = 1; i < points.Count; i++)
			length += points[i - 1].DistanceTo(points[i]);
		length += points[points.Count - 1].DistanceTo(points[0]);
		return length;
	}

	private static Solid TryThicken(Eps_Leg center, double width, double thickness, SolidOptions options)
	{
		try
		{
			CurveLoop path = FlattenToMaxZ(CurveLoopFromLeg(center));
			if (path == null) return null;
			CurveLoop thick = CurveLoop.CreateViaThicken(path, width, XYZ.BasisZ);
			return TryExtrudeLoop(thick, thickness, options);
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("CreateViaThicken failed: " + ex.Message);
			return null;
		}
	}

	private static Solid TrySampledStrip(Eps_Leg center, double halfWidth, double thickness, SolidOptions options)
	{
		Eps_Leg left = new Eps_Leg();
		Eps_Leg right = new Eps_Leg();
		if (RevitGeometry.OffsetLegList(center, halfWidth, 1, ref left, TrimClosedObject: true) != 0) return null;
		if (RevitGeometry.OffsetLegList(center, halfWidth, 2, ref right, TrimClosedObject: true) != 0) return null;
		return TrySampledPair(left, right, thickness, options);
	}

	private static Solid TrySampledPair(Eps_Leg first, Eps_Leg second, double thickness, SolidOptions options)
	{
		if (first == null || second == null) return null;
		if (first.IsLegClosed() && second.IsLegClosed())
		{
			CurveLoop loopA = PolygonFromLeg(first);
			CurveLoop loopB = PolygonFromLeg(second);
			if (loopA == null || loopB == null) return null;
			double lenA = RevitGeometry.eplGeoE_CalcLegListLength(first);
			double lenB = RevitGeometry.eplGeoE_CalcLegListLength(second);
			CurveLoop outer = lenA >= lenB ? loopA : loopB;
			CurveLoop inner = lenA >= lenB ? loopB : loopA;
			Solid solid = TryExtrude(new List<CurveLoop> { outer, inner }, thickness, options);
			if (solid != null) return solid;
			try { inner.Flip(); } catch (Exception) { }
			return TryExtrude(new List<CurveLoop> { outer, inner }, thickness, options);
		}
		List<XYZ> firstPts = SampleXy(first);
		List<XYZ> secondPts = SampleXy(second);
		if (firstPts.Count < 2 || secondPts.Count < 2) return null;
		List<XYZ> ring = new List<XYZ>();
		ring.AddRange(firstPts);
		for (int i = secondPts.Count - 1; i >= 0; i--)
			ring.Add(secondPts[i]);
		return TryExtrudeLoop(LoopFromPoints(ring, true), thickness, options);
	}

	private static Eps_Leg SliceLeg(Eps_Leg source, double from, double to)
	{
		if (source == null || to <= from + MinCurveFt) return null;
		Eps_Point start = new Eps_Point();
		Eps_Point end = new Eps_Point();
		if (source.Line != null)
		{
			EPShared.eplGeoE_FindStaOffPointAlongLine(source.Line, from, 0.0, ref start);
			EPShared.eplGeoE_FindStaOffPointAlongLine(source.Line, to, 0.0, ref end);
			if (start == null || end == null || start.XYZ.DistanceTo(end.XYZ) < MinCurveFt) return null;
			Eps_Leg piece = new Eps_Leg();
			piece.Line = new Eps_Line();
			piece.Line.StartPnt = start;
			piece.Line.EndPnt = end;
			return piece;
		}
		if (source.Arc != null)
		{
			EPShared.eplGeoE_FindStaOffPointAlongArc(source.Arc, from, 0.0, ref start);
			EPShared.eplGeoE_FindStaOffPointAlongArc(source.Arc, to, 0.0, ref end);
			if (start == null || end == null || start.XYZ.DistanceTo(end.XYZ) < MinCurveFt) return null;
			Eps_Leg piece = new Eps_Leg();
			piece.Arc = new Eps_Arc(source.Arc);
			piece.Arc.StartPnt = start;
			piece.Arc.EndPnt = end;
			if (RevitDefines.GT(source.Arc.Radius, 0.0))
				piece.Arc.Delta = (to - from) / source.Arc.Radius;
			return piece;
		}
		return null;
	}

	private static CurveLoop PolygonFromLeg(Eps_Leg leg)
	{
		return LoopFromPoints(SampleXy(leg), true);
	}

	private static List<XYZ> SampleXy(Eps_Leg leg)
	{
		List<XYZ> points = new List<XYZ>();
		if (leg == null) return points;
		double z = leg.LegStartPnt != null ? leg.LegStartPnt.z : 0.0;
		List<Eps_Point> samples = leg.GetPointsAtIntervals(2.0, 1.0);
		if (samples == null) return points;
		foreach (Eps_Point sample in samples)
		{
			if (sample == null) continue;
			XYZ xy = new XYZ(sample.x, sample.y, z);
			if (points.Count == 0 || points[points.Count - 1].DistanceTo(xy) >= MinCurveFt)
				points.Add(xy);
		}
		return points;
	}

	private static CurveLoop LoopFromPoints(List<XYZ> points, bool close)
	{
		if (points == null || points.Count < 3) return null;
		try
		{
			CurveLoop loop = new CurveLoop();
			int last = points.Count;
			for (int i = 0; i < last - 1; i++)
			{
				if (points[i].DistanceTo(points[i + 1]) < MinCurveFt) continue;
				loop.Append(Line.CreateBound(points[i], points[i + 1]));
			}
			if (close && points[points.Count - 1].DistanceTo(points[0]) >= MinCurveFt)
				loop.Append(Line.CreateBound(points[points.Count - 1], points[0]));
			return loop;
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("Polygon loop failed: " + ex.Message);
			return null;
		}
	}

	private static Solid TryExtrudeLoop(CurveLoop loop, double depth, SolidOptions options)
	{
		if (loop == null) return null;
		CurveLoop flat = FlattenToMaxZ(loop) ?? loop;
		Solid solid = TryExtrude(new List<CurveLoop> { flat }, depth, options);
		if (solid != null) return solid;
		try { flat.Flip(); } catch (Exception) { }
		return TryExtrude(new List<CurveLoop> { flat }, depth, options);
	}

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
		if (path == null) return parts;
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
			if (part != null)
			{
				parts.Add(part);
				continue;
			}
			parts.AddRange(QuadStripAlongCurve(curve, width, depth, options));
		}
		return parts;
	}

	private static List<Solid> TessellatedRibbon(Eps_Leg pathLeg, double width, double depth, SolidOptions options)
	{
		List<Solid> parts = new List<Solid>();
		CurveLoop path = CurveLoopFromLeg(pathLeg);
		if (path == null) return parts;
		foreach (Curve curve in path)
			parts.AddRange(QuadStripAlongCurve(curve, width, depth, options));
		if (parts.Count > 0)
			RuntimeDiagnostics.Write("Street tessellated ribbon solids=" + parts.Count + " width=" + width + " ft.");
		return parts;
	}

	private static List<Solid> QuadStripAlongCurve(Curve curve, double width, double depth, SolidOptions options)
	{
		List<Solid> parts = new List<Solid>();
		if (curve == null || width <= MinCurveFt) return parts;
		IList<XYZ> points;
		try
		{
			points = curve.Tessellate();
		}
		catch (Exception)
		{
			return parts;
		}
		if (points == null || points.Count < 2) return parts;
		double half = width * 0.5;
		for (int i = 0; i < points.Count - 1; i++)
		{
			XYZ a = points[i];
			XYZ b = points[i + 1];
			XYZ dir = HorzDir(a, b);
			if (dir.IsZeroLength()) continue;
			XYZ n = new XYZ(-dir.Y, dir.X, 0.0);
			XYZ aL = new XYZ(a.X + n.X * half, a.Y + n.Y * half, a.Z);
			XYZ aR = new XYZ(a.X - n.X * half, a.Y - n.Y * half, a.Z);
			XYZ bL = new XYZ(b.X + n.X * half, b.Y + n.Y * half, b.Z);
			XYZ bR = new XYZ(b.X - n.X * half, b.Y - n.Y * half, b.Z);
			CurveLoop loop = LoopFromPoints(new List<XYZ> { aL, bL, bR, aR }, true);
			Solid solid = TryExtrudeLoop(loop, depth, options);
			if (solid != null) parts.Add(solid);
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
