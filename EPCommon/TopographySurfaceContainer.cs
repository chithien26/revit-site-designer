using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace EPCommon;

public class TopographySurfaceContainer
{
	private TopographySurface m_TopographySurface;

	private CeilingAndFloor m_BuildingPad;

	private FamilyInstance m_FamilyInstance;

	private Eps_Point m_MinLeftPoint;

	private Eps_Point m_MaxRightPoint;

	private Application m_CurApplication;

	private bool m_bIsMinMaxPointsFound;

	private bool m_bBuildingPadPresentInDiffentPhase;

	private bool m_bIsBoundaryLegFound;

	private List<Eps_Triangle> m_Triangles;

	private List<Eps_Point> m_Vertices;

	private List<TopoPoint> m_TopoPoints;

	private List<TopographySurfaceContainer> m_SubregionBuildingPadList;

	private List<TopographySurfaceContainer> m_BuildingPadPresentInDifferentPhaseList;

	private TopographySurfaceType m_TopographySurfaceType;

	private Eps_Leg m_BuildingPadLeg;

	private Eps_Leg m_BoundaryLeg;

	private List<Eps_Point> m_BuildingPadPoints;

	public TopographySurface TopographySurface => m_TopographySurface;

	public CeilingAndFloor BuildingPad => m_BuildingPad;

	public List<Eps_Point> Vertices => m_Vertices;

	public List<TopographySurfaceContainer> SubregionBuildingPadList => m_SubregionBuildingPadList;

	public List<Eps_Triangle> Triangles => m_Triangles;

	public List<TopoPoint> TopoPoints => m_TopoPoints;

	public TopographySurfaceType TopographySurfaceType => m_TopographySurfaceType;

	public Eps_Leg BuildingPadLeg => m_BuildingPadLeg;

	public List<Eps_Point> BuildingPadPoints
	{
		get
		{
			if (m_BuildingPadPoints == null && m_BuildingPadLeg != null)
			{
				m_BuildingPadPoints = new List<Eps_Point>();
				Eps_Leg eps_Leg = null;
				for (eps_Leg = m_BuildingPadLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
				{
					m_BuildingPadPoints.Add(eps_Leg.LegStartPnt);
					if (eps_Leg.Next == null)
					{
						m_BuildingPadPoints.Add(eps_Leg.LegEndPnt);
					}
				}
			}
			return m_BuildingPadPoints;
		}
	}

	public Eps_Point MinRightPoint => new Eps_Point(m_MaxRightPoint.x, m_MinLeftPoint.y, 0.0);

	public Eps_Point MaxLeftPoint => new Eps_Point(m_MinLeftPoint.x, m_MaxRightPoint.y, 0.0);

	public Eps_Point MaxRightPoint => m_MaxRightPoint;

	public Eps_Point MinLeftPoint => m_MinLeftPoint;

	public Eps_Leg BoundaryLeg
	{
		get
		{
			if (m_BoundaryLeg == null)
			{
				BuildBoundaryLeg();
			}
			return m_BoundaryLeg;
		}
	}

	public bool BuildingPadPresentInDiffentPhase
	{
		get
		{
			return m_bBuildingPadPresentInDiffentPhase;
		}
		set
		{
			m_bBuildingPadPresentInDiffentPhase = value;
		}
	}

	public List<TopographySurfaceContainer> BuildingPadPresentInDifferentPhaseList => m_BuildingPadPresentInDifferentPhaseList;

	public TopographySurfaceContainer(Application _CurApplication, TopographySurface _TopographySurface)
	{
		m_TopographySurfaceType = TopographySurfaceType.MainSurface;
		SetupTopographySurfaceContainer(_CurApplication, _TopographySurface, _bFindMinMaxPoints: false);
	}

	public TopographySurfaceContainer(Application _CurApplication, TopographySurface _TopographySurface, bool _bFindMinMaxPoints)
	{
		m_TopographySurfaceType = TopographySurfaceType.MainSurface;
		SetupTopographySurfaceContainer(_CurApplication, _TopographySurface, _bFindMinMaxPoints);
	}

	public TopographySurfaceContainer(Application _CurApplication, TopographySurface _TopographySurface, bool _bFindMinMaxPoints, TopographySurfaceType _TopographySurfaceType)
	{
		m_TopographySurfaceType = _TopographySurfaceType;
		SetupTopographySurfaceContainer(_CurApplication, _TopographySurface, _bFindMinMaxPoints);
	}

	private void SetupTopographySurfaceContainer(Application _CurApplication, TopographySurface _TopographySurface, bool _bFindMinMaxPoints)
	{
		m_TopographySurface = _TopographySurface;
		m_BuildingPad = null;
		m_CurApplication = _CurApplication;
		m_SubregionBuildingPadList = new List<TopographySurfaceContainer>();
		m_BuildingPadPresentInDifferentPhaseList = new List<TopographySurfaceContainer>();
		m_BuildingPadPoints = null;
		if (_bFindMinMaxPoints)
		{
			FindMinMaxPointsOfTopographySurface();
		}
	}

	public TopographySurfaceContainer(Application _CurApplication, FamilyInstance _FamilyInstance, bool _bFindMinMaxPoints)
	{
		m_FamilyInstance = _FamilyInstance;
		m_TopographySurface = null;
		m_CurApplication = _CurApplication;
		m_SubregionBuildingPadList = new List<TopographySurfaceContainer>();
		m_BuildingPadPresentInDifferentPhaseList = new List<TopographySurfaceContainer>();
		m_BuildingPadPoints = null;
		if (_bFindMinMaxPoints)
		{
			FindMinMaxPointsOfTopographySurface();
		}
	}

	public TopographySurfaceContainer(Application _CurApplication, CeilingAndFloor _BuildingPad, bool _bFindMinMaxPoints)
	{
		m_TopographySurfaceType = TopographySurfaceType.BuildingPad;
		m_BuildingPad = _BuildingPad;
		m_TopographySurface = null;
		m_CurApplication = _CurApplication;
		m_Vertices = new List<Eps_Point>();
		m_TopoPoints = new List<TopoPoint>();
		m_SubregionBuildingPadList = new List<TopographySurfaceContainer>();
		m_BuildingPadPresentInDifferentPhaseList = new List<TopographySurfaceContainer>();
		if (_bFindMinMaxPoints)
		{
			FindMinMaxPointsOfTopographySurface();
		}
	}

	public bool Refresh()
	{
		try
		{
			m_BuildingPad = null;
			m_SubregionBuildingPadList = new List<TopographySurfaceContainer>();
			m_BuildingPadPresentInDifferentPhaseList = new List<TopographySurfaceContainer>();
			m_BuildingPadPoints = null;
			FindMinMaxPointsOfTopographySurface();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static TopographySurfaceType FindTopographySurfaceType(Application _CurApplication, TopographySurface _TopographySurface)
	{
		Options options = _CurApplication.Create.NewGeometryOptions();
		options.ComputeReferences = true;
		if (_TopographySurface != null)
		{
			if (_TopographySurface.IsSiteSubRegion)
			{
				return TopographySurfaceType.Subregion;
			}
			if (_TopographySurface.Location == null)
			{
				return TopographySurfaceType.BuildingPad;
			}
		}
		return TopographySurfaceType.MainSurface;
	}

	public void FindMinMaxPointsOfTopographySurface()
	{
		FindMinMaxPointsOfTopographySurface(null);
	}

	public void FindMinMaxPointsOfTopographySurface(List<Eps_Point> _BuildingPadPoint)
	{
		m_Triangles = new List<Eps_Triangle>();
		m_Vertices = new List<Eps_Point>();
		m_TopoPoints = new List<TopoPoint>();
		m_BuildingPadLeg = null;
		m_BoundaryLeg = null;
		Eps_Point pMinPnt = new Eps_Point();
		Eps_Point pMaxPnt = new Eps_Point();
		Options options = m_CurApplication.Create.NewGeometryOptions();
		options.ComputeReferences = true;
		if (m_TopographySurface != null)
		{
			List<Eps_Point> list = new List<Eps_Point>();
			List<Eps_Point> list2 = new List<Eps_Point>();
			List<TopoPoint> list3 = new List<TopoPoint>();
			GeometryElement geometryElement = ((Element)m_TopographySurface).get_Geometry(options);
			List<GeometryObject> list4 = new List<GeometryObject>();
			if (geometryElement != null)
			{
				list4 = ElementSetConverter.ToGeometryObjectList(geometryElement.GetEnumerator());
			}
			pMinPnt.x = double.MaxValue;
			pMaxPnt.x = double.MinValue;
			pMinPnt.y = double.MaxValue;
			pMaxPnt.y = double.MinValue;
			pMinPnt.z = double.MaxValue;
			pMaxPnt.z = double.MinValue;
			Eps_Leg eps_Leg = null;
			Eps_Point eps_Point = null;
			Eps_Point Pnt = null;
			MeshTriangle meshTriangle = null;
			Eps_Triangle eps_Triangle = null;
			TopoPoint topoPoint = null;
			bool flag = true;
			bool flag2 = true;
			double dX = double.MinValue;
			double _dStation = 0.0;
			double _dOffset = 0.0;
			for (int num = list4.Count - 1; num >= 0; num--)
			{
				object obj = list4[num];
				Mesh mesh = obj as Mesh;
				if (!(mesh == null))
				{
					flag = true;
					for (int i = 0; i < mesh.NumTriangles; i++)
					{
						meshTriangle = mesh.get_Triangle(i);
						eps_Triangle = new Eps_Triangle();
						for (int j = 0; j < 3; j++)
						{
							eps_Point = new Eps_Point(meshTriangle.get_Vertex(j));
							if (m_TopographySurfaceType == TopographySurfaceType.BuildingPad)
							{
								if (m_BuildingPadLeg == null)
								{
									if (i == 0 && j == 0)
									{
										dX = eps_Point.z;
									}
									else if (!RevitDefines.EQ(dX, eps_Point.z))
									{
										flag = false;
									}
								}
								else if (eps_Leg != null && RevitDefines.EQ(dX, eps_Point.z))
								{
									if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg, ref _dStation, ref _dOffset) == 1700 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(eps_Leg, _dStation, 0.0, ref Pnt) == 0)
									{
										eps_Point = new Eps_Point(Pnt);
									}
									else
									{
										flag2 = false;
									}
								}
							}
							topoPoint = new TopoPoint(eps_Point);
							topoPoint.Number = meshTriangle.get_Index(j) + 1;
							eps_Triangle.Verticies[j] = topoPoint;
							list3.Add(eps_Triangle.Verticies[j]);
							if (flag2)
							{
								list.Add(eps_Triangle.Verticies[j]);
							}
							else
							{
								flag2 = true;
							}
							RevitGeometry.CalcMinMax(eps_Triangle.Verticies[j], ref pMinPnt, ref pMaxPnt);
						}
						if (eps_Triangle.Verticies.Length == 3 && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Triangle.Verticies[0], eps_Triangle.Verticies[1]) && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Triangle.Verticies[1], eps_Triangle.Verticies[2]) && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Triangle.Verticies[0], eps_Triangle.Verticies[2]))
						{
							m_Triangles.Add(eps_Triangle);
						}
					}
					if (m_TopographySurfaceType == TopographySurfaceType.BuildingPad && m_BuildingPadLeg == null && flag)
					{
						list.Clear();
						IList<XYZ> vertices = mesh.Vertices;
						List<Eps_Point> list5 = _BuildingPadPoint;
						if (list5 == null)
						{
							list5 = new List<Eps_Point>();
							foreach (XYZ item in vertices)
							{
								list5.Add(new Eps_Point(item));
							}
						}
						BuildBuildingPadLeg(list5);
						if (m_BuildingPadLeg != null)
						{
							Eps_Leg ppOffsetLegList = new Eps_Leg();
							RevitGeometry.OffsetLegList(m_BuildingPadLeg, 0.01, 1, ref ppOffsetLegList, TrimClosedObject: true);
							Eps_Leg ppOffsetLegList2 = new Eps_Leg();
							RevitGeometry.OffsetLegList(m_BuildingPadLeg, 0.01, 2, ref ppOffsetLegList2, TrimClosedObject: true);
							eps_Leg = ((!(RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2) < RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList))) ? ppOffsetLegList : ppOffsetLegList2);
							Eps_Leg eps_Leg2 = null;
							for (eps_Leg2 = eps_Leg; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
							{
								if (eps_Leg2.Line != null)
								{
									list2.Add(eps_Leg2.Line.StartPnt);
									if (eps_Leg2.Next == null && !eps_Leg.IsLegClosed())
									{
										list2.Add(eps_Leg2.Line.EndPnt);
									}
								}
							}
						}
					}
					list2.AddRange(list);
					list.Clear();
				}
			}
			IEnumerable<Eps_Point> source = list2;
			m_Vertices = source.Distinct(new PointEqualityComparer()).ToList();
			IEnumerable<TopoPoint> source2 = list3;
			m_TopoPoints = source2.Distinct(new TopoPointEqualityComparer()).ToList();
		}
		else if (m_FamilyInstance != null)
		{
			List<Eps_Point> list6 = new List<Eps_Point>();
			List<TopoPoint> list7 = new List<TopoPoint>();
			GeometryElement geometryElement2 = ((Element)m_FamilyInstance).get_Geometry(options);
			List<GeometryObject> list8 = new List<GeometryObject>();
			if (geometryElement2 != null)
			{
				list8 = ElementSetConverter.ToGeometryObjectList(geometryElement2.GetEnumerator());
			}
			pMinPnt.x = double.MaxValue;
			pMaxPnt.x = double.MinValue;
			pMinPnt.y = double.MaxValue;
			pMaxPnt.y = double.MinValue;
			pMinPnt.z = double.MaxValue;
			pMaxPnt.z = double.MinValue;
			Face face = null;
			Mesh mesh2 = null;
			MeshTriangle meshTriangle2 = null;
			Eps_Triangle eps_Triangle2 = null;
			TopoPoint topoPoint2 = null;
			foreach (GeometryObject item2 in list8)
			{
				Solid solid = item2 as Solid;
				if (solid == null)
				{
					continue;
				}
				for (int k = 0; k < solid.Faces.Size; k++)
				{
					face = solid.Faces.get_Item(k);
					if (face == null)
					{
						continue;
					}
					mesh2 = face.Triangulate();
					if (mesh2 == null)
					{
						continue;
					}
					for (int l = 0; l < mesh2.NumTriangles; l++)
					{
						meshTriangle2 = mesh2.get_Triangle(l);
						eps_Triangle2 = new Eps_Triangle();
						for (int m = 0; m < 3; m++)
						{
							topoPoint2 = new TopoPoint(meshTriangle2.get_Vertex(m));
							topoPoint2.Number = meshTriangle2.get_Index(m) + 1;
							eps_Triangle2.Verticies[m] = topoPoint2;
							list6.Add(eps_Triangle2.Verticies[m]);
							list7.Add(eps_Triangle2.Verticies[m]);
							RevitGeometry.CalcMinMax(eps_Triangle2.Verticies[m], ref pMinPnt, ref pMaxPnt);
						}
						if (eps_Triangle2.Verticies.Length == 3 && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Triangle2.Verticies[0], eps_Triangle2.Verticies[1]) && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Triangle2.Verticies[1], eps_Triangle2.Verticies[2]) && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Triangle2.Verticies[0], eps_Triangle2.Verticies[2]))
						{
							m_Triangles.Add(eps_Triangle2);
						}
					}
				}
			}
			IEnumerable<Eps_Point> source3 = list6;
			m_Vertices = source3.Distinct(new PointEqualityComparer()).ToList();
			IEnumerable<TopoPoint> source4 = list7;
			m_TopoPoints = source4.Distinct(new TopoPointEqualityComparer()).ToList();
		}
		else if (m_BuildingPad != null)
		{
			List<Eps_Point> list9 = new List<Eps_Point>();
			List<TopoPoint> list10 = new List<TopoPoint>();
			GeometryElement geometryElement3 = ((Element)m_BuildingPad).get_Geometry(options);
			List<GeometryObject> list11 = new List<GeometryObject>();
			if (geometryElement3 != null)
			{
				list11 = ElementSetConverter.ToGeometryObjectList(geometryElement3.GetEnumerator());
			}
			pMinPnt.x = double.MaxValue;
			pMaxPnt.x = double.MinValue;
			pMinPnt.y = double.MaxValue;
			pMaxPnt.y = double.MinValue;
			pMinPnt.z = double.MaxValue;
			pMaxPnt.z = double.MinValue;
			Face face2 = null;
			Mesh mesh3 = null;
			MeshTriangle meshTriangle3 = null;
			Eps_Triangle eps_Triangle3 = null;
			TopoPoint topoPoint3 = null;
			foreach (GeometryObject item3 in list11)
			{
				Solid solid2 = item3 as Solid;
				if (solid2 == null)
				{
					continue;
				}
				for (int n = 0; n < solid2.Faces.Size; n++)
				{
					face2 = solid2.Faces.get_Item(n);
					if (face2 == null)
					{
						continue;
					}
					mesh3 = face2.Triangulate();
					if (mesh3 == null)
					{
						continue;
					}
					for (int num2 = 0; num2 < mesh3.NumTriangles; num2++)
					{
						meshTriangle3 = mesh3.get_Triangle(num2);
						eps_Triangle3 = new Eps_Triangle();
						for (int num3 = 0; num3 < 3; num3++)
						{
							topoPoint3 = new TopoPoint(meshTriangle3.get_Vertex(num3));
							topoPoint3.Number = meshTriangle3.get_Index(num3) + 1;
							eps_Triangle3.Verticies[num3] = topoPoint3;
							list9.Add(eps_Triangle3.Verticies[num3]);
							list10.Add(eps_Triangle3.Verticies[num3]);
							RevitGeometry.CalcMinMax(eps_Triangle3.Verticies[num3], ref pMinPnt, ref pMaxPnt);
						}
						if (eps_Triangle3.Verticies.Length == 3 && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Triangle3.Verticies[0], eps_Triangle3.Verticies[1]) && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Triangle3.Verticies[1], eps_Triangle3.Verticies[2]) && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Triangle3.Verticies[0], eps_Triangle3.Verticies[2]))
						{
							m_Triangles.Add(eps_Triangle3);
						}
					}
				}
			}
			IEnumerable<Eps_Point> source5 = list9;
			m_Vertices = source5.Distinct(new PointEqualityComparer()).ToList();
			IEnumerable<TopoPoint> source6 = list10;
			m_TopoPoints = source6.Distinct(new TopoPointEqualityComparer()).ToList();
		}
		m_MinLeftPoint = pMinPnt;
		m_MaxRightPoint = pMaxPnt;
		m_bIsMinMaxPointsFound = true;
	}

	public bool IsPointWithinSurfaceBounds(Eps_Point _FindPoint)
	{
		if (!m_bIsMinMaxPointsFound)
		{
			FindMinMaxPointsOfTopographySurface();
		}
		if (RevitDefines.LT(_FindPoint.x, m_MinLeftPoint.x, 1E-05) || RevitDefines.GT(_FindPoint.x, m_MaxRightPoint.x, 1E-05) || RevitDefines.LT(_FindPoint.y, m_MinLeftPoint.y, 1E-05) || RevitDefines.GT(_FindPoint.y, m_MaxRightPoint.y, 1E-05))
		{
			return false;
		}
		if (m_BoundaryLeg != null && m_BoundaryLeg.IsLegClosed() && RevitGeometry.eplGeoE_IsPointInsideLegList(m_BoundaryLeg, _FindPoint, 1E-05, m_MinLeftPoint, m_MaxRightPoint) == 6)
		{
			return false;
		}
		return true;
	}

	public Element GetSurfaceContainingPoint(Eps_Point _FindPoint)
	{
		double _dElevation = 0.0;
		return GetSurfaceContainingPoint(_FindPoint, ref _dElevation);
	}

	public TopographySurface GetSurfaceContainingPoint(Eps_Point _FindPoint, ref double _dElevation)
	{
		return GetSurfaceContainingPoint(_FindPoint, null, ref _dElevation);
	}

	public TopographySurface GetSurfaceContainingPoint(Eps_Point _FindPoint, Phase _CurPhase, ref double _dElevation)
	{
		if (_CurPhase == null || (m_TopographySurface != null && m_TopographySurface.CreatedPhaseId != null && m_TopographySurface.CreatedPhaseId == _CurPhase.Id) || (m_BuildingPad != null && m_BuildingPad.CreatedPhaseId != null && m_BuildingPad.CreatedPhaseId == _CurPhase.Id))
		{
			if (!IsPointWithinSurfaceBounds(_FindPoint))
			{
				return null;
			}
			for (int i = 0; i < m_Triangles.Count; i++)
			{
				if (m_Triangles[i].IsPointInside(_FindPoint))
				{
					_dElevation = GetElevationFromPlane(m_Triangles[i].Verticies[0], m_Triangles[i].Verticies[1], m_Triangles[i].Verticies[2], _FindPoint);
					return m_TopographySurface;
				}
			}
			if (m_SubregionBuildingPadList != null && m_SubregionBuildingPadList.Count > 0)
			{
				for (int num = m_SubregionBuildingPadList.Count - 1; num >= 0; num--)
				{
					if (_CurPhase == null || (m_SubregionBuildingPadList[num].TopographySurface != null && m_SubregionBuildingPadList[num].TopographySurface.CreatedPhaseId != null && m_SubregionBuildingPadList[num].TopographySurface.CreatedPhaseId == _CurPhase.Id) || (m_SubregionBuildingPadList[num].BuildingPad != null && m_SubregionBuildingPadList[num].BuildingPad.CreatedPhaseId != null && m_SubregionBuildingPadList[num].BuildingPad.CreatedPhaseId == _CurPhase.Id))
					{
						for (int j = 0; j < m_SubregionBuildingPadList[num].Triangles.Count; j++)
						{
							if (m_SubregionBuildingPadList[num].Triangles[j].IsPointInside(_FindPoint))
							{
								_dElevation = GetElevationFromPlane(m_SubregionBuildingPadList[num].Triangles[j].Verticies[0], m_SubregionBuildingPadList[num].Triangles[j].Verticies[1], m_SubregionBuildingPadList[num].Triangles[j].Verticies[2], _FindPoint);
								return m_TopographySurface;
							}
						}
					}
				}
			}
		}
		return null;
	}

	private double GetElevationFromPlane(Eps_Point _FirstPoint, Eps_Point _SecondPoint, Eps_Point _ThirdPoint, Eps_Point _FindPoint)
	{
		double num = (_SecondPoint.y - _FirstPoint.y) * (_ThirdPoint.z - _FirstPoint.z) - (_ThirdPoint.y - _FirstPoint.y) * (_SecondPoint.z - _FirstPoint.z);
		double num2 = 0.0 - ((_SecondPoint.x - _FirstPoint.x) * (_ThirdPoint.z - _FirstPoint.z) - (_ThirdPoint.x - _FirstPoint.x) * (_SecondPoint.z - _FirstPoint.z));
		double num3 = (_SecondPoint.x - _FirstPoint.x) * (_ThirdPoint.y - _FirstPoint.y) - (_ThirdPoint.x - _FirstPoint.x) * (_SecondPoint.y - _FirstPoint.y);
		double num4 = (0.0 - num) * _FirstPoint.x - num2 * _FirstPoint.y - num3 * _FirstPoint.z;
		return (0.0 - (num * _FindPoint.x + num2 * _FindPoint.y + num4)) / num3;
	}

	public TopographySurfaceContainer GetSurfaceContainerContainingPoint(Eps_Point _FindPoint)
	{
		return GetSurfaceContainerContainingPoint(_FindPoint, null, null);
	}

	public TopographySurfaceContainer GetSurfaceContainerContainingPoint(Eps_Point _FindPoint, Phase _PhaseCreated, Phase _PhaseDemolished)
	{
		if ((_PhaseCreated == null || (m_TopographySurface != null && m_TopographySurface.CreatedPhaseId != null && m_TopographySurface.CreatedPhaseId == _PhaseCreated.Id) || (m_BuildingPad != null && m_BuildingPad.CreatedPhaseId != null && m_BuildingPad.CreatedPhaseId == _PhaseCreated.Id)) && (_PhaseDemolished == null || (m_TopographySurface != null && m_TopographySurface.DemolishedPhaseId != null && m_TopographySurface.DemolishedPhaseId == _PhaseDemolished.Id) || (m_BuildingPad != null && m_BuildingPad.DemolishedPhaseId != null && m_BuildingPad.DemolishedPhaseId == _PhaseDemolished.Id)))
		{
			if (!IsPointWithinSurfaceBounds(_FindPoint))
			{
				return null;
			}
			for (int i = 0; i < m_Triangles.Count; i++)
			{
				if (m_Triangles[i].IsPointInside(_FindPoint))
				{
					return this;
				}
			}
			if (m_TopographySurfaceType == TopographySurfaceType.BuildingPad && m_BuildingPadLeg != null)
			{
				List<Eps_Point> paPntList = new List<Eps_Point>();
				paPntList.Add(_FindPoint);
				RevitGeometry.GetAllPointsInBoundary(ref paPntList, m_BuildingPadLeg, null);
				if (paPntList.Count > 0)
				{
					return this;
				}
			}
			if (m_SubregionBuildingPadList != null && m_SubregionBuildingPadList.Count > 0)
			{
				for (int num = m_SubregionBuildingPadList.Count - 1; num >= 0; num--)
				{
					if ((_PhaseCreated == null || (m_SubregionBuildingPadList[num].TopographySurface != null && m_SubregionBuildingPadList[num].TopographySurface.CreatedPhaseId != null && m_SubregionBuildingPadList[num].TopographySurface.CreatedPhaseId == _PhaseCreated.Id) || (m_SubregionBuildingPadList[num].BuildingPad != null && m_SubregionBuildingPadList[num].BuildingPad.CreatedPhaseId != null && m_SubregionBuildingPadList[num].BuildingPad.CreatedPhaseId == _PhaseCreated.Id)) && (_PhaseDemolished == null || (m_SubregionBuildingPadList[num].TopographySurface != null && m_SubregionBuildingPadList[num].TopographySurface.DemolishedPhaseId != null && m_SubregionBuildingPadList[num].TopographySurface.DemolishedPhaseId == _PhaseDemolished.Id) || (m_SubregionBuildingPadList[num].BuildingPad != null && m_SubregionBuildingPadList[num].BuildingPad.DemolishedPhaseId != null && m_SubregionBuildingPadList[num].BuildingPad.DemolishedPhaseId == _PhaseDemolished.Id)))
					{
						for (int j = 0; j < m_SubregionBuildingPadList[num].Triangles.Count; j++)
						{
							if (m_SubregionBuildingPadList[num].Triangles[j].IsPointInside(_FindPoint))
							{
								return m_SubregionBuildingPadList[num];
							}
						}
					}
				}
			}
		}
		return null;
	}

	public bool IsTopographySurfaceTypePresent(TopographySurfaceType _TopographySurfaceType)
	{
		if (m_TopographySurfaceType == _TopographySurfaceType)
		{
			return true;
		}
		if (m_SubregionBuildingPadList != null && m_SubregionBuildingPadList.Count > 0)
		{
			for (int i = 0; i < m_SubregionBuildingPadList.Count; i++)
			{
				if (m_SubregionBuildingPadList[i].TopographySurfaceType == _TopographySurfaceType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<Eps_Point> GetAllVertices()
	{
		List<Eps_Point> list = new List<Eps_Point>();
		List<Eps_Point> paPntList = new List<Eps_Point>();
		List<Eps_Point> list2 = new List<Eps_Point>();
		List<int> list3 = new List<int>();
		paPntList.AddRange(m_Vertices);
		for (int num = m_SubregionBuildingPadList.Count - 1; num >= 0; num--)
		{
			if (list3.Contains(num))
			{
				m_SubregionBuildingPadList[num].FindMinMaxPointsOfTopographySurface(m_SubregionBuildingPadList[num].BuildingPadPoints);
			}
			if (m_SubregionBuildingPadList[num].BuildingPadLeg != null)
			{
				RevitGeometry.RemoveAllPointsInBoundary(ref paPntList, m_SubregionBuildingPadList[num].BuildingPadLeg);
				for (int num2 = num - 1; num2 >= 0; num2--)
				{
					if (m_SubregionBuildingPadList[num2].BuildingPadPoints != null && m_SubregionBuildingPadList[num2].BuildingPadPoints.Count != 0)
					{
						for (int i = 0; i < m_SubregionBuildingPadList[num].BuildingPadPoints.Count; i++)
						{
							for (int num3 = m_SubregionBuildingPadList[num2].BuildingPadPoints.Count - 1; num3 >= 0; num3--)
							{
								if (EPShared.eplGeoE_ArePntsDuplicates2D(m_SubregionBuildingPadList[num].BuildingPadPoints[i], m_SubregionBuildingPadList[num2].BuildingPadPoints[num3]))
								{
									m_SubregionBuildingPadList[num2].BuildingPadPoints.RemoveAt(num3);
									if (!list3.Contains(num2))
									{
										list3.Add(num2);
									}
								}
							}
						}
					}
				}
			}
			list2.AddRange(m_SubregionBuildingPadList[num].Vertices);
		}
		paPntList.AddRange(list2);
		IEnumerable<Eps_Point> source = paPntList;
		return source.Distinct(new PointEqualityComparer()).ToList();
	}

	private void BuildBuildingPadLeg(List<Eps_Point> _BuildingPadPoints)
	{
		Eps_Leg eps_Leg = null;
		Eps_Point eps_Point = null;
		Eps_Point eps_Point2 = null;
		m_BuildingPadLeg = null;
		for (int i = 0; i < _BuildingPadPoints.Count; i++)
		{
			eps_Point2 = new Eps_Point(_BuildingPadPoints[i]);
			if (eps_Point != null)
			{
				eps_Leg = new Eps_Leg();
				eps_Leg.Line = new Eps_Line();
				eps_Leg.Line.StartPnt = eps_Point;
				eps_Leg.Line.EndPnt = eps_Point2;
				if (m_BuildingPadLeg == null)
				{
					m_BuildingPadLeg = Eps_Leg.SingleLeg(eps_Leg);
				}
				else
				{
					m_BuildingPadLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
				}
			}
			eps_Point = eps_Point2;
		}
		if (m_BuildingPadLeg != null && !m_BuildingPadLeg.IsLegClosed())
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = new Eps_Line();
			eps_Leg.Line.StartPnt = m_BuildingPadLeg.LegEndPnt;
			eps_Leg.Line.EndPnt = m_BuildingPadLeg.LegStartPnt;
			m_BuildingPadLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
		}
	}

	public void BuildBoundaryLeg()
	{
		if (m_bIsBoundaryLegFound)
		{
			return;
		}
		m_bIsBoundaryLegFound = true;
		m_BoundaryLeg = null;
		if (m_Triangles == null || m_Triangles.Count <= 0)
		{
			return;
		}
		bool flag = false;
		int num = 0;
		Eps_Line eps_Line = null;
		List<Eps_Line> list = new List<Eps_Line>();
		for (int i = 0; i < m_Triangles.Count; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				eps_Line = new Eps_Line();
				if (j < 2)
				{
					eps_Line.StartPnt = m_Triangles[i].Verticies[j];
					eps_Line.EndPnt = m_Triangles[i].Verticies[j + 1];
				}
				else
				{
					eps_Line.StartPnt = m_Triangles[i].Verticies[j];
					eps_Line.EndPnt = m_Triangles[i].Verticies[0];
				}
				list.Add(eps_Line);
			}
		}
		if (list != null && list.Count > 0)
		{
			Eps_LineComparer comparer = new Eps_LineComparer();
			list.Sort(comparer);
			if (list.Count > 1)
			{
				int num2 = -1;
				for (num = 0; num < list.Count - 1; num++)
				{
					num2 = num + 1;
					while (list[num].IsEqual(list[num2]))
					{
						flag = true;
						list.RemoveAt(num2);
						if (num2 > list.Count - 1)
						{
							break;
						}
					}
					if (flag)
					{
						list.RemoveAt(num);
						flag = false;
						num--;
					}
				}
			}
		}
		Eps_Leg _CurrentLeg = null;
		List<Eps_Leg> list2 = new List<Eps_Leg>();
		if (list != null && list.Count > 0)
		{
			int num3 = list.Count;
			while (num3 > 0)
			{
				for (int num4 = list.Count - 1; num4 >= 0; num4--)
				{
					if (Eps_Leg.InsertLineLegSegment(ref _CurrentLeg, list[num4]))
					{
						list.RemoveAt(num4);
					}
				}
				if (num3 == list.Count)
				{
					if (_CurrentLeg != null)
					{
						list2.Add(_CurrentLeg);
						_CurrentLeg = null;
					}
					else
					{
						num3 = 0;
					}
				}
				else
				{
					num3 = list.Count;
				}
			}
		}
		if (_CurrentLeg != null)
		{
			list2.Add(_CurrentLeg);
		}
		if (list2.Count > 1)
		{
			Eps_Leg eps_Leg = null;
			int nLineCount = 0;
			int nLineCount2 = 0;
			int nArcCount = 0;
			int nArcCount2 = 0;
			Eps_Line[] aLines = null;
			Eps_Line[] aLines2 = null;
			Eps_Arc[] aArcs = null;
			Eps_Arc[] aArcs2 = null;
			int[] aLegOrder = null;
			int[] aLegOrder2 = null;
			eps_Leg = list2[0];
			eps_Leg.ConvertLegToLineArcArrays(ref aLines, ref nLineCount, ref aArcs, ref nArcCount, ref aLegOrder, _bExtendLeg: false);
			for (int k = 1; k < list2.Count; k++)
			{
				list2[k].ConvertLegToLineArcArrays(ref aLines2, ref nLineCount2, ref aArcs2, ref nArcCount2, ref aLegOrder2, _bExtendLeg: false);
				if (EPShared.eplGeoE_IsLegListInsideLegList(aLines, nLineCount, aArcs, nArcCount, aLegOrder, aLines2, nLineCount2, aArcs2, nArcCount2, aLegOrder2, 1E-05))
				{
					eps_Leg = list2[k];
					eps_Leg.ConvertLegToLineArcArrays(ref aLines, ref nLineCount, ref aArcs, ref nArcCount, ref aLegOrder, _bExtendLeg: false);
				}
			}
			m_BoundaryLeg = eps_Leg;
		}
		else
		{
			m_BoundaryLeg = _CurrentLeg;
		}
	}
}
