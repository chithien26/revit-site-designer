using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SITES.Utilities;

namespace EPCommon;

public class ConvertToToposurface
{
	private Autodesk.Revit.ApplicationServices.Application m_CurApplication;

	private Document m_Document;

	private ConvertToToposurfaceForm m_DisplayForm;

	private EPTransactionContainer m_TransactionContainer;

	private Element m_PickedElement;

	private List<Eps_Point> m_TopoPointList;

	private const string m_strNummericSeperator = " - ";

	private ProgressBarForm m_ProgressFrm;

	private bool m_HideOriginalTopoSurface;

	public ConvertToToposurface(Document _CurDocument)
	{
		m_CurApplication = _CurDocument.Application;
		m_Document = _CurDocument;
		m_TransactionContainer = new EPTransactionContainer(m_Document);
		m_PickedElement = null;
		m_TopoPointList = new List<Eps_Point>();
		m_ProgressFrm = null;
	}

	public void Run()
	{
		DisplayUI();
	}

	private bool DisplayUI()
	{
		bool result = false;
		DialogResult _DialogResult = DialogResult.Retry;
		using (m_DisplayForm = new ConvertToToposurfaceForm(m_Document))
		{
			while (_DialogResult == DialogResult.Retry)
			{
				result = ShowDialog(ref _DialogResult);
			}
			return result;
		}
	}

	private bool ShowDialog(ref DialogResult _DialogResult)
	{
		bool flag = false;
		_DialogResult = m_DisplayForm.ShowDialog();
		if (_DialogResult == DialogResult.OK)
		{
			if (m_PickedElement != null)
			{
				m_TransactionContainer.Start(EPShared.LocalRM.GetString("ConvertObjectToToposurfaceTransaction"));
				m_HideOriginalTopoSurface = m_DisplayForm.HideOriginalTopoSurface;
				flag = VerifyAndCreateToposurface();
				if (flag)
				{
					m_TransactionContainer.Commit();
				}
				else
				{
					m_TransactionContainer.Rollback();
				}
			}
		}
		else if (_DialogResult == DialogResult.Cancel)
		{
			flag = false;
		}
		else if (_DialogResult == DialogResult.Retry)
		{
			ElementSet _InstanceSet = new ElementSet();
			if (!PickOject(ref _InstanceSet))
			{
				return false;
			}
			m_DisplayForm.EnableConvertButton();
		}
		return flag;
	}

	private bool VerifyAndCreateToposurface()
	{
		Parameter parameter = null;
		bool flag = false;
		TopographySurface topographySurface = null;
		Phase phase = null;
		string strSurfaceName = string.Empty;
		if (m_DisplayForm.ToposurfaceName == string.Empty)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidSurfaceNameString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return false;
		}
		phase = m_DisplayForm.SelectedPhase;
		int nBeginningIndex = 0;
		topographySurface = FindTopographySurfaceStartingNumberName(m_DisplayForm.ToposurfaceName, phase, ref nBeginningIndex);
		if (topographySurface != null)
		{
			List<string> list = new List<string>();
			list.Add(EPShared.LocalRM.GetString("OverwriteToposurfaceOption"));
			list.Add(EPShared.LocalRM.GetString("CreateNewToposurfaceOption"));
			WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), m_Document.PathName, "ConvertToToposurface", EPShared.LocalRM.GetString("ToposurfaceSameNameAndPhaseMessage"), list, 0, _bDisplayOKCancel: true, _bDisplayDoNotShow: false);
			warningOptionsForm.ShowDialog();
			if (warningOptionsForm.DialogResult != DialogResult.OK)
			{
				return false;
			}
			if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("OverwriteToposurfaceOption"), ignoreCase: true) == 0)
			{
				flag = true;
			}
			else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("CreateNewToposurfaceOption"), ignoreCase: true) == 0)
			{
				strSurfaceName = GetNameWithoutSeperator(m_DisplayForm.ToposurfaceName) + " - " + Convert.ToString(nBeginningIndex);
			}
		}
		else
		{
			strSurfaceName = m_DisplayForm.ToposurfaceName;
		}
		m_ProgressFrm = new ProgressBarForm();
		m_ProgressFrm.ShowForm();
		m_ProgressFrm.Refresh();
		m_ProgressFrm.MinValue = 0;
		m_ProgressFrm.MaxValue = Convert.ToInt32(3);
		m_ProgressFrm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingNewToposurfaceMessage");
		m_TransactionContainer.BeginTransaction();
		TopographySurface topographySurface2 = ConvertObjectToToposurface(strSurfaceName);
		m_ProgressFrm.UpdateProgressBar(1);
		if (topographySurface != null)
		{
			if (topographySurface2 != null)
			{
				TopographySurfaceContainer _ProposedTopographySurfaceContainer = null;
				TopographySurfaceContainer _BaseTopographySurfaceContainer = null;
				FamilyInstance _ProposedPlacementFamilyInstance = null;
				FamilyInstance _BasePlacementFamilyInstance = null;
				TopographySurfaceManagement.FindBaseLandcaddTopographySurface(m_Document, ref _BaseTopographySurfaceContainer, ref _BasePlacementFamilyInstance, ref _ProposedTopographySurfaceContainer, ref _ProposedPlacementFamilyInstance, m_TransactionContainer);
				if (_ProposedTopographySurfaceContainer != null && _ProposedTopographySurfaceContainer.TopographySurface != null && _ProposedPlacementFamilyInstance != null && string.Compare(_ProposedTopographySurfaceContainer.TopographySurface.UniqueId, topographySurface.UniqueId, ignoreCase: true) == 0)
				{
					((Element)_ProposedPlacementFamilyInstance).get_Parameter(new Guid(RevitParameters.UniqueIDGenericModelFamilyParameter))?.Set(topographySurface2.UniqueId);
				}
				if (_BaseTopographySurfaceContainer != null && _BaseTopographySurfaceContainer.TopographySurface != null && _BasePlacementFamilyInstance != null && string.Compare(_BaseTopographySurfaceContainer.TopographySurface.UniqueId, topographySurface.UniqueId, ignoreCase: true) == 0)
				{
					((Element)_BasePlacementFamilyInstance).get_Parameter(new Guid(RevitParameters.UniqueIDGenericModelFamilyParameter))?.Set(topographySurface2.UniqueId);
				}
			}
			if (flag)
			{
				m_Document.Delete(topographySurface.Id);
			}
		}
		m_TransactionContainer.EndTransaction();
		m_ProgressFrm.SetMaxValue();
		Thread.Sleep(300);
		m_ProgressFrm.Close();
		return true;
	}

	private TopographySurface FindTopographySurfaceStartingNumberName(string _strFindName, Phase SelectedPhase, ref int nBeginningIndex)
	{
		TopographySurface topographySurface = null;
		Parameter parameter = null;
		TopographySurface result = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		List<Element> list = filteredElementCollector.OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).ToList();
		nBeginningIndex = 0;
		int num = -1;
		bool flag = true;
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		string nameWithoutSeperator = GetNameWithoutSeperator(_strFindName);
		int num2 = 1;
		foreach (Element item in list)
		{
			if (!(TerrainApi.AsSurface(item) is {} topographySurface2))
			{
				continue;
			}
			Parameter parameter2 = ((Element)topographySurface2).get_Parameter(BuiltInParameter.PHASE_CREATED);
			if (parameter2 == null || !(parameter2.AsElementId() == SelectedPhase.Id))
			{
				continue;
			}
			parameter = TerrainApi.NameParameter(((Element)topographySurface2));
			empty3 = string.Empty;
			if (parameter != null && parameter.AsString() != null && parameter.AsString() != string.Empty)
			{
				empty3 = parameter.AsString();
			}
			else
			{
				empty3 = "<" + EPShared.LocalRM.GetString("UnnamedSurfaceDialog") + " " + Convert.ToString(num2) + ">";
				num2++;
			}
			if (!(empty3 != string.Empty) || !empty3.ToUpper().StartsWith(nameWithoutSeperator.ToUpper()))
			{
				continue;
			}
			int num3 = empty3.LastIndexOf(" - ");
			if (num3 >= 0 && num3 < empty3.Length)
			{
				empty = empty3.Substring(num3);
				if (!empty.StartsWith(" - ") || empty.Length <= " - ".Length)
				{
					continue;
				}
				empty2 = empty.Substring(" - ".Length);
				for (int i = 0; i < empty2.Length; i++)
				{
					if (!char.IsDigit(empty2[i]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					num = Convert.ToInt32(empty2);
					if (num > nBeginningIndex)
					{
						nBeginningIndex = num;
					}
				}
				else
				{
					flag = true;
				}
			}
			else if (string.Compare(empty3, _strFindName, ignoreCase: true) == 0)
			{
				result = topographySurface2;
			}
		}
		nBeginningIndex++;
		return result;
	}

	private string GetNameWithoutSeperator(string _strValue)
	{
		int num = _strValue.LastIndexOf(" - ");
		if (num >= 0 && num < _strValue.Length)
		{
			string text = _strValue.Substring(num);
			if (text.StartsWith(" - ") && text.Length > " - ".Length)
			{
				bool flag = true;
				string text2 = text.Substring(" - ".Length);
				for (int i = 0; i < text2.Length; i++)
				{
					if (!char.IsDigit(text2[i]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					_strValue = _strValue.Substring(0, num);
				}
			}
		}
		return _strValue;
	}

	private TopographySurface ConvertObjectToToposurface(string _strSurfaceName)
	{
		TopographySurface topographySurface = null;
		if (m_PickedElement != null)
		{
			if (m_PickedElement is Group obj && obj.GetMemberIds().Count != 0)
			{
				IList<ElementId> memberIds = obj.GetMemberIds();
				foreach (ElementId item in memberIds)
				{
					Element element = m_Document.GetElement(item);
					GetPointsFromElement(element);
				}
			}
			else
			{
				GetPointsFromElement(m_PickedElement);
			}
			m_ProgressFrm.UpdateProgressBar(1);
			if (m_TopoPointList.Count > 0)
			{
				List<Eps_Point> list = new List<Eps_Point>();
				Eps_PointComparer comparer = new Eps_PointComparer();
				m_TopoPointList.Sort(comparer);
				IEnumerable<Eps_Point> topoPointList = m_TopoPointList;
				list = topoPointList.Distinct(new PointEqualityComparer()).ToList();
				List<XYZ> PointXYZList = new List<XYZ>();
				for (int i = 0; i < list.Count; i++)
				{
					PointXYZList.Add(list[i].XYZ);
				}
				if (PointXYZList.Count > 0)
				{
					TopographySurfaceManagement.RemoveDuplicateXY(ref PointXYZList);
					topographySurface = TerrainApi.Create(m_Document, PointXYZList);
					m_Document.Regenerate();
				}
				if (topographySurface != null)
				{
					TerrainApi.NameParameter(((Element)topographySurface))?.Set(_strSurfaceName);
					Parameter parameter = ((Element)topographySurface).get_Parameter(BuiltInParameter.PHASE_CREATED);
					if (parameter != null && m_DisplayForm.SelectedPhase != null)
					{
						Phase selectedPhase = m_DisplayForm.SelectedPhase;
						ElementId id = selectedPhase.Id;
						parameter.Set(id);
					}
				}
			}
			if (m_HideOriginalTopoSurface)
			{
				ElementSet elementSet = new ElementSet();
				elementSet.Insert(m_PickedElement);
				RevitUtility.HideElementsInAllViews(m_Document, elementSet);
			}
		}
		return topographySurface;
	}

	private bool PickOject(ref ElementSet _InstanceSet)
	{
		ElementSet _Elements = null;
		if (!RevitUtility.GetElementsFromProject(m_Document, EPShared.LocalRM.GetString("PickObjectMessage"), ref _Elements) || _Elements == null || _Elements.Size == 0)
		{
			return false;
		}
		foreach (Element item in _Elements)
		{
			m_PickedElement = item;
		}
		return true;
	}

	private void GetPointsFromElement(Element _CurrentElement)
	{
		Options options = m_CurApplication.Create.NewGeometryOptions();
		options.ComputeReferences = true;
		GeometryElement geometryElement = _CurrentElement.get_Geometry(options);
		if (geometryElement != null && geometryElement.Count() > 0)
		{
			List<GeometryObject> objects = ElementSetConverter.ToGeometryObjectList(geometryElement.GetEnumerator());
			AddGeometryObjects(objects);
		}
	}

	private void AddGeometryObjects(List<GeometryObject> _Objects)
	{
		if (_Objects == null)
		{
			return;
		}
		foreach (GeometryObject _Object in _Objects)
		{
			if (_Object is Solid)
			{
				AddSolid(_Object);
				continue;
			}
			if (_Object is Face)
			{
				AddFace(_Object);
				continue;
			}
			if (_Object is Mesh)
			{
				AddMesh(_Object);
				continue;
			}
			if (_Object is Edge)
			{
				AddEdge(_Object);
				continue;
			}
			string text = _Object.GetType().Name;
			if (text.StartsWith("Geometry"))
			{
				text = text.Remove(0, 8);
			}
			if (!(text == "Element"))
			{
				if (text == "Instance")
				{
					AddInstance(_Object);
				}
			}
			else
			{
				AddGeoElement(_Object);
			}
		}
	}

	private void AddInstance(GeometryObject _Object)
	{
		GeometryInstance geometryInstance = _Object as GeometryInstance;
		if (geometryInstance == null)
		{
			return;
		}
		GeometryElement symbolGeometry = geometryInstance.SymbolGeometry;
		if (!(symbolGeometry == null))
		{
			List<GeometryObject> objects = new List<GeometryObject>();
			if (symbolGeometry != null)
			{
				objects = ElementSetConverter.ToGeometryObjectList(symbolGeometry.GetEnumerator());
			}
			AddGeometryObjects(objects);
		}
	}

	private void AddGeoElement(GeometryObject _Object)
	{
		GeometryElement geometryElement = _Object as GeometryElement;
		if (!(geometryElement == null))
		{
			List<GeometryObject> objects = ElementSetConverter.ToGeometryObjectList(geometryElement.GetEnumerator());
			AddGeometryObjects(objects);
		}
	}

	private void AddSolid(GeometryObject _Object)
	{
		Solid solid = _Object as Solid;
		if (solid == null)
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
			AddFace(item);
		}
	}

	private void AddFace(GeometryObject _Object)
	{
		Face face = _Object as Face;
		if (!(face == null))
		{
			Mesh mesh = face.Triangulate();
			if (!(mesh == null))
			{
				AddMesh(mesh);
			}
		}
	}

	private void AddProfile(GeometryObject _Object)
	{
		Profile profile = _Object as Profile;
		if (profile == null)
		{
			return;
		}
		foreach (Curve curf in profile.Curves)
		{
			AddCurve(curf);
		}
	}

	private void AddMesh(GeometryObject _Object)
	{
		Mesh mesh = _Object as Mesh;
		if (mesh == null)
		{
			return;
		}
		List<Eps_Point> list = new List<Eps_Point>();
		for (int i = 0; i < mesh.NumTriangles; i++)
		{
			MeshTriangle meshTriangle = mesh.get_Triangle(i);
			try
			{
				for (int j = 0; j < 3; j++)
				{
					list.Add(new Eps_Point(meshTriangle.get_Vertex(j)));
				}
			}
			catch
			{
			}
		}
		if (list.Count > 0)
		{
			m_TopoPointList.AddRange(list);
		}
	}

	private void AddCurve(GeometryObject _Object)
	{
		Curve curve = _Object as Curve;
		if (curve == null)
		{
			return;
		}
		List<Eps_Point> list = new List<Eps_Point>();
		if (curve.IsBound)
		{
			IList<XYZ> list2 = curve.Tessellate();
			for (int i = 0; i < list2.Count; i++)
			{
				list.Add(new Eps_Point(list2[i]));
			}
		}
		if (list.Count > 0)
		{
			m_TopoPointList.AddRange(list);
		}
	}

	private void AddEdge(GeometryObject _Object)
	{
		Edge edge = _Object as Edge;
		if (!(edge == null))
		{
			List<Eps_Point> list = new List<Eps_Point>();
			IList<XYZ> list2 = edge.Tessellate();
			for (int i = 0; i < list2.Count; i++)
			{
				list.Add(new Eps_Point(list2[i]));
			}
			if (list.Count > 0)
			{
				m_TopoPointList.AddRange(list);
			}
		}
	}
}
