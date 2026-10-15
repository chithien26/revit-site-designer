using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace EPCommon;

public class MeshTriangleList
{
	private List<MeshTriangleContainer> m_TriangleList;

	private Eps_Leg m_Leg;

	public List<MeshTriangleContainer> TriangleList => m_TriangleList;

	public Eps_Leg Leg => m_Leg;

	public MeshTriangleList(GeometryObject _GeoElement)
	{
		m_Leg = null;
		m_TriangleList = new List<MeshTriangleContainer>();
		AddGeoElement(_GeoElement);
	}

	private void AddGeoElement(GeometryObject _Object)
	{
		GeometryElement geometryElement = _Object as GeometryElement;
		if (!(geometryElement == null))
		{
			GeometryObject[] objects = geometryElement.ToArray();
			AddGeometryObjects(objects);
		}
	}

	private void AddGeometryObjects(GeometryObject[] _Objects)
	{
		foreach (GeometryObject geometryObject in _Objects)
		{
			string text = geometryObject.GetType().Name;
			if (text.StartsWith("Geometry"))
			{
				text = text.Remove(0, 8);
			}
			if (string.Compare(text, "Mesh", ignoreCase: true) == 0)
			{
				AddMesh(geometryObject);
			}
		}
	}

	public VerticeContainer FindVerticeInVerticeList(uint _VerticeNumber, List<VerticeContainer> _VerticeList)
	{
		for (int i = 0; i < _VerticeList.Count; i++)
		{
			if (_VerticeNumber == _VerticeList[i].VerticeNumber)
			{
				return _VerticeList[i];
			}
		}
		return null;
	}

	private void AddMesh(GeometryObject _Object)
	{
		Mesh mesh = _Object as Mesh;
		if (null == mesh)
		{
			return;
		}
		List<MeshTriangleContainer> list = new List<MeshTriangleContainer>();
		uint nLineStartNumber = 0u;
		uint num = 0u;
		List<VerticeContainer> list2 = new List<VerticeContainer>();
		for (int i = 0; i < mesh.NumTriangles; i++)
		{
			MeshTriangle meshTriangle = mesh.get_Triangle(i);
			try
			{
				MeshTriangleContainer meshTriangleContainer = new MeshTriangleContainer();
				MeshTriangleLine item;
				for (int j = 0; j < 3; j++)
				{
					num = meshTriangle.get_Index(j);
					if (FindVerticeInVerticeList(num, list2) == null)
					{
						list2.Add(new VerticeContainer(num, new Eps_Point(meshTriangle.get_Vertex(j))));
					}
					if (j > 0)
					{
						item = new MeshTriangleLine(i, nLineStartNumber, num);
						meshTriangleContainer.TriangleLines.Add(item);
					}
					nLineStartNumber = num;
				}
				num = meshTriangleContainer.TriangleLines[0].LineStartNumber;
				item = new MeshTriangleLine(i, nLineStartNumber, num);
				meshTriangleContainer.TriangleLines.Add(item);
				list.Add(meshTriangleContainer);
			}
			catch
			{
			}
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		bool flag = false;
		bool flag2 = false;
		for (num4 = 0; num4 < list.Count; num4++)
		{
			for (num2 = num4 + 1; num2 < list.Count; num2++)
			{
				for (num5 = 0; num5 < list[num4].TriangleLines.Count; num5++)
				{
					MeshTriangleLine meshTriangleLine = list[num4].TriangleLines[num5];
					for (num3 = 0; num3 < list[num2].TriangleLines.Count; num3++)
					{
						MeshTriangleLine compareLine = list[num2].TriangleLines[num3];
						if (!meshTriangleLine.IsEqual(compareLine))
						{
							continue;
						}
						list[num4].TriangleLines.RemoveAt(num5);
						num5--;
						list[num2].TriangleLines.RemoveAt(num3);
						num3--;
						if (list[num4].TriangleLines.Count == 0 || list[num2].TriangleLines.Count == 0)
						{
							if (list[num4].TriangleLines.Count == 0)
							{
								flag2 = true;
								list.RemoveAt(num4);
								num4--;
								num2--;
							}
							if (list[num2].TriangleLines.Count == 0)
							{
								flag = true;
								list.RemoveAt(num2);
								num2--;
							}
							break;
						}
					}
					if (flag2 || flag)
					{
						if (flag)
						{
							flag = false;
						}
						break;
					}
				}
				if (flag2)
				{
					flag2 = false;
					break;
				}
			}
		}
		List<MeshTriangleLine> list3 = new List<MeshTriangleLine>();
		for (num4 = 0; num4 < list.Count; num4++)
		{
			for (int k = 0; k < list[num4].TriangleLines.Count; k++)
			{
				list3.Add(list[num4].TriangleLines[k]);
			}
		}
		MeshTriangleLineComparer comparer = new MeshTriangleLineComparer();
		list3.Sort(comparer);
		Eps_Leg eps_Leg = null;
		VerticeContainer verticeContainer = null;
		for (int l = 0; l < list3.Count; l++)
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = new Eps_Line();
			verticeContainer = FindVerticeInVerticeList(list3[l].LineStartNumber, list2);
			if (verticeContainer != null)
			{
				eps_Leg.Line.StartPnt = verticeContainer.VerticePoint;
			}
			verticeContainer = FindVerticeInVerticeList(list3[l].LineEndNumber, list2);
			if (verticeContainer != null)
			{
				eps_Leg.Line.EndPnt = verticeContainer.VerticePoint;
			}
			if (m_Leg == null)
			{
				m_Leg = eps_Leg;
			}
			else
			{
				m_Leg.Last.Next = eps_Leg;
			}
		}
	}

	public double Area()
	{
		List<Eps_Point> list = new List<Eps_Point>();
		if (m_Leg != null)
		{
			Eps_Leg eps_Leg = null;
			for (eps_Leg = m_Leg; eps_Leg != null; eps_Leg = eps_Leg.Next)
			{
				if (eps_Leg.Line != null)
				{
					list.Add(eps_Leg.Line.StartPnt);
				}
			}
		}
		return RevitGeometry.eplGeoE_CalcPolygonArea(list);
	}

	public double OffsetArea(double _dOffsetDistance)
	{
		Eps_Leg eps_Leg = OffsetLeg(_dOffsetDistance);
		List<Eps_Point> list = new List<Eps_Point>();
		if (eps_Leg != null)
		{
			Eps_Leg eps_Leg2 = null;
			for (eps_Leg2 = eps_Leg; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
			{
				if (eps_Leg2.Line != null)
				{
					list.Add(eps_Leg2.Line.StartPnt);
				}
			}
		}
		return RevitGeometry.eplGeoE_CalcPolygonArea(list);
	}

	public Eps_Leg OffsetLeg(double _dOffsetDistance)
	{
		Eps_Leg ppOffsetLegList = new Eps_Leg();
		int Direction = 0;
		if (m_Leg != null)
		{
			RevitGeometry.eplGeoE_CalcLegDirection(m_Leg, ref Direction, 1E-05);
			RevitGeometry.OffsetLegList(OffsetDir: (Direction != 1) ? 1 : 2, pLegListToOffset: m_Leg, OffsetDist: _dOffsetDistance, ppOffsetLegList: ref ppOffsetLegList, TrimClosedObject: true);
			return ppOffsetLegList;
		}
		return null;
	}
}
