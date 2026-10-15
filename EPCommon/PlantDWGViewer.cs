using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

namespace EPCommon;

public class PlantDWGViewer
{
	private Element m_selected;

	private Autodesk.Revit.DB.View m_currentView;

	private ViewDetailLevel m_detailLevel = ViewDetailLevel.Fine;

	private Sketch3D m_currentSketch3D;

	private bool m_isSelectView = true;

	private Document m_CurDocument;

	private EPTransactionContainer m_TransactionContainer;

	public ViewDetailLevel DetailLevel
	{
		get
		{
			return m_detailLevel;
		}
		set
		{
			m_detailLevel = value;
			m_isSelectView = false;
			UpdateSketch3D();
		}
	}

	public Sketch3D CurrentSketch3D => m_currentSketch3D;

	public Document CurDocument => m_CurDocument;

	public EPTransactionContainer TransactionContainer => m_TransactionContainer;

	public PlantDWGViewer(Document _CurDocument, EPTransactionContainer _TransactionContainer)
	{
		m_CurDocument = _CurDocument;
		m_TransactionContainer = _TransactionContainer;
	}

	private void UpdateSketch3D()
	{
		GeometryData geometryData;
		if (m_isSelectView)
		{
			geometryData = new GeometryData(m_CurDocument.Application, m_selected, m_currentView);
			m_detailLevel = ViewDetailLevel.Undefined;
		}
		else
		{
			geometryData = new GeometryData(m_CurDocument.Application, m_selected, m_detailLevel, m_currentView);
		}
		Graphics3DData data3D = geometryData.Data3D;
		Graphics3DData data3D2 = m_currentSketch3D.Data3D;
		data3D.CurrentUCS = data3D2.CurrentUCS;
		m_currentSketch3D.Data3D = data3D;
	}

	public bool SetupPreview(string _strDrawingName)
	{
		m_TransactionContainer.Start(EPShared.LocalRM.GetString("ObtainDrawingImageTransaction"));
		DeleteSelected();
		Document curDocument = m_CurDocument;
		DWGImportOptions dWGImportOptions = new DWGImportOptions();
		dWGImportOptions.ColorMode = ImportColorMode.Preserved;
		dWGImportOptions.CustomScale = 0.0;
		dWGImportOptions.OrientToView = true;
		dWGImportOptions.Placement = ImportPlacement.Origin;
		dWGImportOptions.ThisViewOnly = false;
		dWGImportOptions.Unit = ImportUnit.Foot;
		dWGImportOptions.VisibleLayersOnly = false;
		ElementId elementId = null;
		curDocument.Import(_strDrawingName, dWGImportOptions, curDocument.ActiveView, out elementId);
		m_selected = curDocument.GetElement(elementId);
		ElementSet elementSet = new ElementSet();
		elementSet.Insert(m_selected);
		try
		{
			curDocument.ActiveView.HideElements(ElementSetConverter.ToIDList(elementSet));
			m_TransactionContainer.Commit();
		}
		catch (ArgumentException)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("CannotImport_String") + Path.GetFileName(_strDrawingName) + EPShared.LocalRM.GetString("_InSettingString"), EPShared.LocalRM.GetString("ImportString"), MessageBoxIcon.Exclamation);
			DeleteSelected();
			m_TransactionContainer.Commit();
			m_currentSketch3D = null;
			return false;
		}
		m_currentView = curDocument.ActiveView;
		GeometryData geometryData = new GeometryData(m_CurDocument.Application, m_selected, m_currentView);
		m_currentSketch3D = new Sketch3D(geometryData.Data3D);
		return true;
	}

	public void DeleteSelected()
	{
		if (m_selected != null)
		{
			m_TransactionContainer.BeginTransaction();
			m_CurDocument.Delete(m_selected.Id);
			m_TransactionContainer.EndTransaction();
		}
	}
}
