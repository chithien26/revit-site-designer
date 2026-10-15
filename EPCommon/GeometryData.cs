using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace EPCommon;

public class GeometryData
{
	private BoundingBoxXYZ m_BoundingBox;

	private List<IList<XYZ>> m_3DCurveList = new List<IList<XYZ>>();

	public Graphics3DData Data3D => new Graphics3DData(new List<IList<XYZ>>(m_3DCurveList), m_BoundingBox);

	public GeometryData(Application _CurApplication, Element _Element, ViewDetailLevel _Detail, View _CurrentView)
	{
		Options options = _CurApplication.Create.NewGeometryOptions();
		options.DetailLevel = _Detail;
		options.ComputeReferences = false;
		GeometryElement geometryElement = _Element.get_Geometry(options);
		Transform transform = Transform.CreateTranslation(new XYZ(0.0, 0.0, 0.0));
		AddGeoElement(geometryElement, transform);
		m_BoundingBox = _Element.get_BoundingBox(_CurrentView);
	}

	public GeometryData(Application _CurApplication, Element _Element, View _CurrentView)
	{
		Options options = _CurApplication.Create.NewGeometryOptions();
		options.View = _CurrentView;
		options.ComputeReferences = false;
		GeometryElement geometryElement = _Element.get_Geometry(options);
		Transform transform = Transform.CreateTranslation(new XYZ(0.0, 0.0, 0.0));
		AddGeoElement(geometryElement, transform);
		m_BoundingBox = _Element.get_BoundingBox(_CurrentView);
	}

	private void AddGeoElement(GeometryObject _Object, Transform _Transform)
	{
		GeometryElement geometryElement = _Object as GeometryElement;
		if (!(geometryElement == null))
		{
			List<GeometryObject> objects = new List<GeometryObject>();
			if (geometryElement != null)
			{
				objects = ElementSetConverter.ToGeometryObjectList(geometryElement.GetEnumerator());
			}
			AddGeometryObjects(objects, _Transform);
		}
	}

	private void AddGeometryObjects(List<GeometryObject> _Objects, Transform _Transform)
	{
		foreach (GeometryObject _Object in _Objects)
		{
			string text = _Object.GetType().Name;
			if (text.StartsWith("Geometry"))
			{
				text = text.Remove(0, 8);
			}
			switch (text)
			{
			case "Solid":
				AddSolid(_Object, _Transform);
				break;
			case "Face":
				AddFace(_Object, _Transform);
				break;
			case "Mesh":
				AddMesh(_Object, _Transform);
				break;
			case "Curve":
			case "Line":
			case "Arc":
				AddCurve(_Object, _Transform);
				break;
			case "Profile":
				AddProfile(_Object, _Transform);
				break;
			case "Element":
				AddGeoElement(_Object, _Transform);
				break;
			case "Instance":
				AddInstance(_Object, _Transform);
				break;
			case "Edge":
				AddEdge(_Object, _Transform);
				break;
			}
		}
	}

	private void AddSolid(GeometryObject _Object, Transform _Transform)
	{
		Solid solid = _Object as Solid;
		if (null == solid)
		{
			return;
		}
		FaceArray faces = solid.Faces;
		if (faces.Size == 0)
		{
			return;
		}
		foreach (Face item in faces)
		{
			AddFace(item, _Transform);
		}
	}

	private void AddFace(GeometryObject _Object, Transform _Transform)
	{
		Face face = _Object as Face;
		if (!(null == face))
		{
			Mesh mesh = face.Triangulate();
			if (!(null == mesh))
			{
				AddMesh(mesh, _Transform);
			}
		}
	}

	private void AddProfile(GeometryObject _Object, Transform _Transform)
	{
		Profile profile = _Object as Profile;
		if (null == profile)
		{
			return;
		}
		foreach (Curve curf in profile.Curves)
		{
			AddCurve(curf, _Transform);
		}
	}

	private void AddMesh(GeometryObject _Object, Transform _Transform)
	{
		Mesh mesh = _Object as Mesh;
		if (null == mesh)
		{
			return;
		}
		for (int i = 0; i < mesh.NumTriangles; i++)
		{
			MeshTriangle meshTriangle = mesh.get_Triangle(i);
			IList<XYZ> list = new List<XYZ>();
			try
			{
				for (int j = 0; j < 3; j++)
				{
					XYZ point = meshTriangle.get_Vertex(j);
					XYZ basis = MathUtil.GetBasis(point, _Transform);
					list.Add(basis);
				}
				XYZ item = list[0];
				list.Add(item);
				m_3DCurveList.Add(list);
			}
			catch
			{
			}
		}
	}

	private void AddCurve(GeometryObject _Object, Transform _Transform)
	{
		Curve curve = _Object as Curve;
		if (null == curve || !curve.IsBound)
		{
			return;
		}
		IList<XYZ> list = curve.Tessellate();
		IList<XYZ> list2 = new List<XYZ>();
		foreach (XYZ item in list)
		{
			XYZ basis = MathUtil.GetBasis(item, _Transform);
			list2.Add(basis);
		}
		m_3DCurveList.Add(list2);
	}

	private void AddInstance(GeometryObject _Object, Transform _Transform)
	{
		GeometryInstance geometryInstance = _Object as GeometryInstance;
		if (null == geometryInstance)
		{
			return;
		}
		Transform transform = AddTransform(_Transform, geometryInstance.Transform);
		GeometryElement symbolGeometry = geometryInstance.SymbolGeometry;
		if (!(null == symbolGeometry))
		{
			List<GeometryObject> objects = new List<GeometryObject>();
			if (symbolGeometry != null)
			{
				objects = ElementSetConverter.ToGeometryObjectList(symbolGeometry.GetEnumerator());
			}
			AddGeometryObjects(objects, transform);
		}
	}

	private void AddEdge(GeometryObject _Object, Transform _Transform)
	{
		Edge edge = _Object as Edge;
		if (null == edge)
		{
			return;
		}
		IList<XYZ> list = edge.Tessellate();
		IList<XYZ> list2 = new List<XYZ>();
		foreach (XYZ item in list)
		{
			XYZ basis = MathUtil.GetBasis(item, _Transform);
			list2.Add(basis);
		}
		m_3DCurveList.Add(list2);
	}

	private Transform AddTransform(Transform _FirstTransform, Transform _SecondTransform)
	{
		int num = 0;
		XYZ vector = new XYZ(0.0, 0.0, 0.0);
		Transform transform = Transform.CreateTranslation(vector);
		transform.Origin = MathUtil.AddXYZ(_FirstTransform.Origin, _SecondTransform.Origin);
		XYZ[] array = new XYZ[3];
		XYZ[] array2 = new XYZ[3];
		for (num = 0; num < 3; num++)
		{
			array[num] = _FirstTransform.get_Basis(num);
			array2[num] = _SecondTransform.get_Basis(num);
		}
		XYZ[] array3 = MathUtil.MultiCross(array, array2);
		for (num = 0; num < 3; num++)
		{
			transform.set_Basis(num, array3[num]);
		}
		return transform;
	}
}
