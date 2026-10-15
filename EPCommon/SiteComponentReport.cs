using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.Exceptions;
using SITES.Utilities;

namespace EPCommon;

public class SiteComponentReport : EPForm
{
	public enum RegionEdgingWarning
	{
		RegionEdgingNotFound,
		IssueWarning,
		WarningDisplayed
	}

	private Units m_Units;

	private Autodesk.Revit.ApplicationServices.Application m_CurApplication;

	private Document m_CurrentDocument;

	private List<FamilyRegionContainer> m_FamilyRegionList;

	private FormatOptions m_LengthFormatOptions;

	private FormatOptions m_AreaFormatOptions;

	private FormatOptions m_VolumeFormatOptions;

	private UnitSymbolType m_LengthUnitSymbolType;

	private UnitSymbolType m_AreaUnitSymbolType;

	private UnitSymbolType m_VolumeUnitSymbolType;

	private bool m_bExportToFile;

	private List<ExportReportLine> m_ReportLineList;

	private string m_strSettingsPrefix;

	private List<FamilyInstanceComponentWrapper> m_AllSiteComponentList;

	private List<FamilyInstanceComponentWrapper> m_SideWalkComponentList;

	private List<FamilyInstanceComponentWrapper> m_StreetComponentList;

	private List<FamilyInstanceComponentWrapper> m_CurbComponentList;

	private List<FamilyInstanceComponentWrapper> m_RetainingWallComponentList;

	private List<FamilyInstanceComponentWrapper> m_ParkingLotComponentList;

	private TopographySurfaceContainer m_ProposedSurface;

	private DecimalSymbol m_DecimalSymbolType;

	private IContainer components;

	private GroupBox m_SiteReportGroupBox;

	private Button m_CloseButton;

	private Button m_ExportButton;

	private TreeView m_ReportTreeView;

	private ListView m_ReportListView;

	private GroupBox m_ExportOptionsGroupBox;

	private RadioButton m_TabExportRadioButton;

	private RadioButton m_CommaExportRadioButton;

	private TextBox m_ExportFileTextBox;

	private Button m_ExportFileButton;

	private Label m_ExportFileLabel;

	public SiteComponentReport(Document _CurDocument, ref bool _bDlgStatus)
	{
		_bDlgStatus = true;
		InitializeComponent();
		EPToolTip.SetToolTip(m_ExportFileButton, EPShared.LocalRM.GetString("LoadFileToolTip"));
		ContextualHelpID = "HID_SDreports";
		m_CurApplication = _CurDocument.Application;
		m_CurrentDocument = _CurDocument;
		m_LengthFormatOptions = m_CurrentDocument.GetUnits().GetFormatOptions(UnitType.UT_Length);
		m_AreaFormatOptions = m_CurrentDocument.GetUnits().GetFormatOptions(UnitType.UT_Area);
		m_VolumeFormatOptions = m_CurrentDocument.GetUnits().GetFormatOptions(UnitType.UT_Volume);
		m_Units = m_CurrentDocument.GetUnits();
		try
		{
			m_LengthUnitSymbolType = m_LengthFormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_LengthUnitSymbolType = UnitSymbolType.UST_NONE;
		}
		try
		{
			m_AreaUnitSymbolType = m_AreaFormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_AreaUnitSymbolType = UnitSymbolType.UST_NONE;
		}
		try
		{
			m_VolumeUnitSymbolType = m_VolumeFormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_VolumeUnitSymbolType = UnitSymbolType.UST_NONE;
		}
		m_DecimalSymbolType = m_CurrentDocument.GetUnits().DecimalSymbol;
		m_FamilyRegionList = new List<FamilyRegionContainer>();
		m_AllSiteComponentList = new List<FamilyInstanceComponentWrapper>();
		Text = EPShared.LocalRM.GetString("SitesReportDialog");
		m_SiteReportGroupBox.Text = Text;
		m_ExportOptionsGroupBox.Text = EPShared.LocalRM.GetString("ExportOptionsDialog");
		m_CommaExportRadioButton.Text = EPShared.LocalRM.GetString("AMP_CSVFileDialog");
		m_TabExportRadioButton.Text = EPShared.LocalRM.GetString("AMP_TabFileDialog");
		m_ExportFileLabel.Text = EPShared.LocalRM.GetString("AMP_FileDialog");
		m_ExportButton.Text = EPShared.LocalRM.GetString("AMP_ExportDialog");
		m_CloseButton.Text = EPShared.LocalRM.GetString("AMP_CloseDialog");
		m_strSettingsPrefix = Text.Replace(" ", "");
		InitializeControls();
		m_ReportListView.Columns.Add(EPShared.LocalRM.GetString("Parameter"), m_ReportListView.Size.Width / 2 - 2, HorizontalAlignment.Left);
		m_ReportListView.Columns.Add(EPShared.LocalRM.GetString("Value"), m_ReportListView.Size.Width / 2 - 2, HorizontalAlignment.Left);
		m_ProposedSurface = null;
		FamilyInstance _TopoPlacementFamilyInstance = null;
		TopographySurfaceManagement.FindBaseLandcaddTopographySurface(m_CurrentDocument, _bIsProposedSurface: true, ref m_ProposedSurface, ref _TopoPlacementFamilyInstance);
		PopulateReportTree();
		if (m_AllSiteComponentList.Count <= 0)
		{
			string strText = EPShared.LocalRM.GetString("NoSiteComponentsInProjectMessage");
			RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
			_bDlgStatus = false;
		}
	}

	private void PopulateReportTree()
	{
		m_AllSiteComponentList = RevitUtility.FindAllSiteComponents(m_CurrentDocument);
		Parameter parameter = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		IList<Element> list = filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Site).ToElements();
		filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		IList<Element> list2 = filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Mass).ToElements();
		filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		IList<Element> list3 = filteredElementCollector.OfClass(typeof(DirectShape)).ToElements();
		filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		IList<Element> list4 = filteredElementCollector.OfClass(typeof(TopographySurface)).OfCategory(BuiltInCategory.OST_Topography).ToElements();
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		string empty4 = string.Empty;
		string empty5 = string.Empty;
		string empty6 = string.Empty;
		string empty7 = string.Empty;
		Parameter parameter2 = null;
		Parameter parameter3 = null;
		bool flag = false;
		for (int i = 0; i < m_AllSiteComponentList.Count; i++)
		{
			FamilyInstance firstExistingFamilyInstance = m_AllSiteComponentList[i].GetFirstExistingFamilyInstance();
			if (firstExistingFamilyInstance == null)
			{
				continue;
			}
			empty = string.Empty;
			empty2 = string.Empty;
			empty3 = string.Empty;
			empty4 = string.Empty;
			empty5 = string.Empty;
			empty6 = string.Empty;
			empty7 = string.Empty;
			parameter3 = firstExistingFamilyInstance.LookupParameter(RevitParameters.SymbolIDGenericFamilyParameter);
			if (parameter3 != null)
			{
				empty = parameter3.AsString();
			}
			parameter = firstExistingFamilyInstance.LookupParameter(RevitParameters.FamilySymbolNameFamilyParameter);
			if (parameter != null)
			{
				empty2 = parameter.AsString();
			}
			parameter = firstExistingFamilyInstance.LookupParameter(RevitParameters.FamilyNameFamilyParameter);
			if (parameter != null)
			{
				empty3 = parameter.AsString();
			}
			parameter = firstExistingFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (parameter != null)
			{
				empty4 = parameter.AsString();
			}
			if (empty4 != string.Empty)
			{
				foreach (Element item in list)
				{
					if (!(item is FamilySymbol { Family: not null } familySymbol))
					{
						continue;
					}
					parameter2 = familySymbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
					if (parameter2 != null && string.Compare(parameter2.AsString(), empty4, ignoreCase: true) == 0)
					{
						if (string.Compare(empty, familySymbol.UniqueId, ignoreCase: true) == 0)
						{
							flag = true;
							m_AllSiteComponentList[i].ProposedFamilySymbol = familySymbol;
							break;
						}
						if (string.Compare(empty2, familySymbol.Name, ignoreCase: true) == 0 && string.Compare(empty3, familySymbol.Family.Name, ignoreCase: true) == 0)
						{
							m_AllSiteComponentList[i].ProposedFamilySymbol = familySymbol;
						}
					}
				}
			}
			if (parameter3 != null && !flag && m_AllSiteComponentList[i].ProposedFamilySymbol != null)
			{
				parameter3.Set(m_AllSiteComponentList[i].ProposedFamilySymbol.UniqueId);
			}
			parameter = firstExistingFamilyInstance.LookupParameter(RevitParameters.PrimaryFormIDFamilyParameter);
			if (parameter != null)
			{
				empty5 = parameter.AsString();
			}
			if (empty5 != string.Empty)
			{
				foreach (Element item2 in list2)
				{
					if (item2 is FamilyInstance { Symbol: not null } familyInstance && string.Compare(empty5, familyInstance.UniqueId, ignoreCase: true) == 0)
					{
						m_AllSiteComponentList[i].PrimarySiteComponentMass = familyInstance;
						break;
					}
				}
				foreach (Element item3 in list3)
				{
					if (item3 is DirectShape directShape && string.Compare(empty5, directShape.UniqueId, ignoreCase: true) == 0)
					{
						m_AllSiteComponentList[i].PrimarySiteComponentDirectShape = directShape;
						break;
					}
				}
			}
			parameter = firstExistingFamilyInstance.LookupParameter(RevitParameters.AdditionalFormIDFamilyParameter);
			if (parameter != null)
			{
				empty6 = parameter.AsString();
			}
			if (empty6 != string.Empty)
			{
				foreach (Element item4 in list2)
				{
					if (item4 is FamilyInstance { Symbol: not null } familyInstance2 && string.Compare(empty6, familyInstance2.UniqueId, ignoreCase: true) == 0)
					{
						m_AllSiteComponentList[i].AdditionalSiteComponentMass = familyInstance2;
						break;
					}
				}
				foreach (Element item5 in list3)
				{
					if (item5 is DirectShape directShape2 && string.Compare(empty6, directShape2.UniqueId, ignoreCase: true) == 0)
					{
						m_AllSiteComponentList[i].AdditionalSiteComponentDirectShape = directShape2;
						break;
					}
				}
			}
			parameter = firstExistingFamilyInstance.LookupParameter(RevitParameters.ParkingLotSurfaceIDFamilyParameter);
			if (parameter != null)
			{
				empty7 = parameter.AsString();
			}
			if (!(empty7 != string.Empty))
			{
				continue;
			}
			foreach (Element item6 in list4)
			{
				if (item6 is TopographySurface topographySurface && string.Compare(empty7, topographySurface.UniqueId, ignoreCase: true) == 0)
				{
					m_AllSiteComponentList[i].ParkingLotTopographySurface = new TopographySurfaceContainer(m_CurApplication, topographySurface, _bFindMinMaxPoints: true);
					break;
				}
			}
		}
		m_RetainingWallComponentList = new List<FamilyInstanceComponentWrapper>();
		for (int j = 0; j < m_AllSiteComponentList.Count; j++)
		{
			if (m_AllSiteComponentList[j].ProposedFamilySymbol != null)
			{
				Parameter parameter4 = m_AllSiteComponentList[j].ProposedFamilySymbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
				if (parameter4 != null && string.Compare(parameter4.AsString(), Convert.ToInt32(LandcaddID.RetainingWall).ToString("000"), ignoreCase: true) == 0)
				{
					m_RetainingWallComponentList.Add(m_AllSiteComponentList[j]);
				}
			}
		}
		if (m_RetainingWallComponentList.Count > 0)
		{
			TreeNode treeNode = null;
			TreeNode treeNode2 = null;
			FamilyInstance familyInstance3 = null;
			treeNode = new TreeNode();
			treeNode.Text = EPShared.LocalRM.GetString("RetainingWallString");
			for (int k = 0; k < m_RetainingWallComponentList.Count; k++)
			{
				treeNode2 = new TreeNode();
				if (m_RetainingWallComponentList[k].ProposedFamilySymbol == null)
				{
					continue;
				}
				familyInstance3 = m_RetainingWallComponentList[k].GetFirstExistingFamilyInstance();
				if (familyInstance3 != null)
				{
					parameter = familyInstance3.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
					if (parameter != null)
					{
						treeNode2.Text = parameter.AsString();
						treeNode.Nodes.Add(treeNode2);
					}
				}
			}
			m_ReportTreeView.Nodes.Add(treeNode);
		}
		m_StreetComponentList = new List<FamilyInstanceComponentWrapper>();
		for (int l = 0; l < m_AllSiteComponentList.Count; l++)
		{
			if (m_AllSiteComponentList[l].ProposedFamilySymbol != null)
			{
				Parameter parameter5 = m_AllSiteComponentList[l].ProposedFamilySymbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
				if (parameter5 != null && string.Compare(parameter5.AsString(), Convert.ToInt32(LandcaddID.Street).ToString("000"), ignoreCase: true) == 0)
				{
					m_StreetComponentList.Add(m_AllSiteComponentList[l]);
				}
			}
		}
		if (m_StreetComponentList.Count > 0)
		{
			TreeNode treeNode3 = null;
			TreeNode treeNode4 = null;
			FamilyInstance familyInstance4 = null;
			treeNode3 = new TreeNode();
			treeNode3.Text = EPShared.LocalRM.GetString("StreetString");
			m_ReportTreeView.Nodes.Add(treeNode3);
			for (int m = 0; m < m_StreetComponentList.Count; m++)
			{
				treeNode4 = new TreeNode();
				if (m_StreetComponentList[m].ProposedFamilySymbol == null)
				{
					continue;
				}
				familyInstance4 = m_StreetComponentList[m].GetFirstExistingFamilyInstance();
				if (familyInstance4 != null)
				{
					parameter = familyInstance4.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
					if (parameter != null)
					{
						treeNode4.Text = parameter.AsString();
						treeNode3.Nodes.Add(treeNode4);
					}
				}
			}
		}
		m_CurbComponentList = new List<FamilyInstanceComponentWrapper>();
		for (int n = 0; n < m_AllSiteComponentList.Count; n++)
		{
			if (m_AllSiteComponentList[n].ProposedFamilySymbol != null)
			{
				Parameter parameter6 = m_AllSiteComponentList[n].ProposedFamilySymbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
				if (parameter6 != null && string.Compare(parameter6.AsString(), Convert.ToInt32(LandcaddID.Curb).ToString("000"), ignoreCase: true) == 0)
				{
					m_CurbComponentList.Add(m_AllSiteComponentList[n]);
				}
			}
		}
		if (m_CurbComponentList.Count > 0)
		{
			TreeNode treeNode5 = null;
			TreeNode treeNode6 = null;
			FamilyInstance familyInstance5 = null;
			treeNode5 = new TreeNode();
			treeNode5.Text = EPShared.LocalRM.GetString("CurbString");
			m_ReportTreeView.Nodes.Add(treeNode5);
			for (int num = 0; num < m_CurbComponentList.Count; num++)
			{
				treeNode6 = new TreeNode();
				if (m_CurbComponentList[num].ProposedFamilySymbol == null)
				{
					continue;
				}
				familyInstance5 = m_CurbComponentList[num].GetFirstExistingFamilyInstance();
				if (familyInstance5 != null)
				{
					parameter = familyInstance5.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
					if (parameter != null)
					{
						treeNode6.Text = parameter.AsString();
						treeNode5.Nodes.Add(treeNode6);
					}
				}
			}
		}
		m_SideWalkComponentList = new List<FamilyInstanceComponentWrapper>();
		for (int num2 = 0; num2 < m_AllSiteComponentList.Count; num2++)
		{
			if (m_AllSiteComponentList[num2].ProposedFamilySymbol != null)
			{
				Parameter parameter7 = m_AllSiteComponentList[num2].ProposedFamilySymbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
				if (parameter7 != null && string.Compare(parameter7.AsString(), Convert.ToInt32(LandcaddID.Sidewalk).ToString("000"), ignoreCase: true) == 0)
				{
					m_SideWalkComponentList.Add(m_AllSiteComponentList[num2]);
				}
			}
		}
		if (m_SideWalkComponentList.Count > 0)
		{
			TreeNode treeNode7 = null;
			TreeNode treeNode8 = null;
			FamilyInstance familyInstance6 = null;
			treeNode7 = new TreeNode();
			treeNode7.Text = EPShared.LocalRM.GetString("SidewalkString");
			m_ReportTreeView.Nodes.Add(treeNode7);
			for (int num3 = 0; num3 < m_SideWalkComponentList.Count; num3++)
			{
				treeNode8 = new TreeNode();
				if (m_SideWalkComponentList[num3].ProposedFamilySymbol == null)
				{
					continue;
				}
				familyInstance6 = m_SideWalkComponentList[num3].GetFirstExistingFamilyInstance();
				if (familyInstance6 != null)
				{
					parameter = familyInstance6.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
					if (parameter != null)
					{
						treeNode8.Text = parameter.AsString();
						treeNode7.Nodes.Add(treeNode8);
					}
				}
			}
		}
		m_ParkingLotComponentList = new List<FamilyInstanceComponentWrapper>();
		for (int num4 = 0; num4 < m_AllSiteComponentList.Count; num4++)
		{
			if (m_AllSiteComponentList[num4].ProposedFamilySymbol != null)
			{
				Parameter parameter8 = m_AllSiteComponentList[num4].ProposedFamilySymbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
				if (parameter8 != null && string.Compare(parameter8.AsString(), Convert.ToInt32(LandcaddID.ParkingLot).ToString("000"), ignoreCase: true) == 0)
				{
					m_ParkingLotComponentList.Add(m_AllSiteComponentList[num4]);
				}
			}
		}
		if (m_ParkingLotComponentList.Count <= 0)
		{
			return;
		}
		TreeNode treeNode9 = null;
		TreeNode treeNode10 = null;
		FamilyInstance familyInstance7 = null;
		treeNode9 = new TreeNode();
		treeNode9.Text = EPShared.LocalRM.GetString("ParkingLotString");
		m_ReportTreeView.Nodes.Add(treeNode9);
		for (int num5 = 0; num5 < m_ParkingLotComponentList.Count; num5++)
		{
			treeNode10 = new TreeNode();
			if (m_ParkingLotComponentList[num5].ProposedFamilySymbol == null)
			{
				continue;
			}
			familyInstance7 = m_ParkingLotComponentList[num5].GetFirstExistingFamilyInstance();
			if (familyInstance7 != null)
			{
				parameter = familyInstance7.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
				if (parameter != null)
				{
					treeNode10.Text = parameter.AsString();
					treeNode9.Nodes.Add(treeNode10);
				}
			}
		}
	}

	private void OnExportButtonClicked(object sender, EventArgs e)
	{
		if (ValidateFields())
		{
			string empty = string.Empty;
			if (m_ReportLineList == null)
			{
				m_ReportLineList = new List<ExportReportLine>();
			}
			m_bExportToFile = true;
			string empty2 = string.Empty;
			empty2 = ((!m_CommaExportRadioButton.Checked) ? "\t" : ",");
			PopulateReportLineList();
			if (m_ReportLineList.Count > 0)
			{
				ImportExport.ExportToFile(empty2, m_ExportFileTextBox.Text, m_ReportLineList);
			}
			m_ReportLineList.Clear();
			m_bExportToFile = false;
		}
	}

	private void PopulateReportLineList()
	{
		Parameter parameter = null;
		FamilyInstance familyInstance = null;
		string text = string.Empty;
		int num = 0;
		if (m_CurbComponentList.Count > 0)
		{
			PopulateTotalAreaInList(m_CurbComponentList, null, LandcaddID.Curb);
			AddBlankLine();
			num = -1;
			foreach (FamilyInstanceComponentWrapper curbComponent in m_CurbComponentList)
			{
				familyInstance = curbComponent.GetFirstExistingFamilyInstance();
				if (familyInstance != null)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
					if (parameter != null)
					{
						text = parameter.AsString();
					}
				}
				num = FindRootIndex(m_CurbComponentList, text);
				if (num != -1)
				{
					PopulateTotalAreaInList(m_CurbComponentList[num], text, LandcaddID.Curb);
					AddBlankLine();
				}
			}
			AddBlankLine();
		}
		if (m_ParkingLotComponentList.Count > 0)
		{
			PopulateTotalAreaInList(m_ParkingLotComponentList, null, LandcaddID.ParkingLot);
			AddBlankLine();
			num = -1;
			foreach (FamilyInstanceComponentWrapper parkingLotComponent in m_ParkingLotComponentList)
			{
				familyInstance = parkingLotComponent.GetFirstExistingFamilyInstance();
				if (familyInstance != null)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
					if (parameter != null)
					{
						text = parameter.AsString();
					}
				}
				num = FindRootIndex(m_ParkingLotComponentList, text);
				if (num != -1)
				{
					PopulateTotalAreaInList(m_ParkingLotComponentList[num], text, LandcaddID.ParkingLot);
					AddBlankLine();
				}
			}
			AddBlankLine();
		}
		if (m_RetainingWallComponentList.Count > 0)
		{
			PopulateTotalAreaInList(m_RetainingWallComponentList, null, LandcaddID.RetainingWall);
			AddBlankLine();
			num = -1;
			foreach (FamilyInstanceComponentWrapper retainingWallComponent in m_RetainingWallComponentList)
			{
				familyInstance = retainingWallComponent.GetFirstExistingFamilyInstance();
				if (familyInstance != null)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
					if (parameter != null)
					{
						text = parameter.AsString();
					}
				}
				num = FindRootIndex(m_RetainingWallComponentList, text);
				if (num != -1)
				{
					PopulateTotalAreaInList(m_RetainingWallComponentList[num], text, LandcaddID.RetainingWall);
					AddBlankLine();
				}
			}
			AddBlankLine();
		}
		if (m_SideWalkComponentList.Count > 0)
		{
			PopulateTotalAreaInList(m_SideWalkComponentList, null, LandcaddID.Sidewalk);
			AddBlankLine();
			num = -1;
			foreach (FamilyInstanceComponentWrapper sideWalkComponent in m_SideWalkComponentList)
			{
				familyInstance = sideWalkComponent.GetFirstExistingFamilyInstance();
				if (familyInstance != null)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
					if (parameter != null)
					{
						text = parameter.AsString();
					}
				}
				num = FindRootIndex(m_SideWalkComponentList, text);
				if (num != -1)
				{
					PopulateTotalAreaInList(m_SideWalkComponentList[num], text, LandcaddID.Sidewalk);
					AddBlankLine();
				}
			}
			AddBlankLine();
		}
		if (m_StreetComponentList.Count <= 0)
		{
			return;
		}
		PopulateTotalAreaInList(m_StreetComponentList, null, LandcaddID.Street);
		AddBlankLine();
		num = -1;
		foreach (FamilyInstanceComponentWrapper streetComponent in m_StreetComponentList)
		{
			familyInstance = streetComponent.GetFirstExistingFamilyInstance();
			if (familyInstance != null)
			{
				parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
				if (parameter != null)
				{
					text = parameter.AsString();
				}
			}
			num = FindRootIndex(m_StreetComponentList, text);
			if (num != -1)
			{
				PopulateTotalAreaInList(m_StreetComponentList[num], text, LandcaddID.Street);
				AddBlankLine();
			}
		}
		AddBlankLine();
	}

	private void OnCloseButtonClicked(object sender, EventArgs e)
	{
		SaveSettings();
		DialogResult = DialogResult.OK;
	}

	private int FindRootIndex(List<FamilyInstanceComponentWrapper> _ComponentList, string _strFamilyName)
	{
		string empty = string.Empty;
		FamilyInstance familyInstance = null;
		Parameter parameter = null;
		for (int i = 0; i < _ComponentList.Count; i++)
		{
			familyInstance = _ComponentList[i].GetFirstExistingFamilyInstance();
			if (familyInstance != null)
			{
				parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
				if (parameter != null && string.Compare(parameter.AsString(), _strFamilyName, ignoreCase: true) == 0)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private void ReportTreeViewAfterSelect(object sender, TreeViewEventArgs e)
	{
		int num = -1;
		m_ReportListView.Items.Clear();
		LandcaddID id = LandcaddID.Steps;
		List<FamilyInstanceComponentWrapper> list = null;
		string empty = string.Empty;
		string text = string.Empty;
		if (m_ReportTreeView.SelectedNode.Parent == null)
		{
			empty = m_ReportTreeView.SelectedNode.Text;
			text = null;
		}
		else if (m_ReportTreeView.SelectedNode.Parent.Parent == null)
		{
			empty = m_ReportTreeView.SelectedNode.Parent.Text;
			text = m_ReportTreeView.SelectedNode.Text;
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetString"), empty, ignoreCase: true) == 0)
		{
			id = LandcaddID.Street;
			list = m_StreetComponentList;
		}
		else if (string.Compare(EPShared.LocalRM.GetString("CurbString"), empty, ignoreCase: true) == 0)
		{
			id = LandcaddID.Curb;
			list = m_CurbComponentList;
		}
		else if (string.Compare(EPShared.LocalRM.GetString("SidewalkString"), empty, ignoreCase: true) == 0)
		{
			id = LandcaddID.Sidewalk;
			list = m_SideWalkComponentList;
		}
		else if (string.Compare(EPShared.LocalRM.GetString("RetainingWallString"), empty, ignoreCase: true) == 0)
		{
			id = LandcaddID.RetainingWall;
			list = m_RetainingWallComponentList;
		}
		else if (string.Compare(EPShared.LocalRM.GetString("ParkingLotString"), empty, ignoreCase: true) == 0)
		{
			id = LandcaddID.ParkingLot;
			list = m_ParkingLotComponentList;
		}
		if (m_ReportTreeView.SelectedNode.Parent == null)
		{
			PopulateTotalAreaInList(list, text, id);
		}
		else if (m_ReportTreeView.SelectedNode.Parent.Parent == null)
		{
			num = FindRootIndex(list, text);
			if (num != -1)
			{
				PopulateTotalAreaInList(list[num], text, id);
			}
		}
	}

	private void PopulateTotalAreaInList(FamilyInstanceComponentWrapper _FamilyInstanceWrapperList, LandcaddID _Id)
	{
		List<FamilyInstanceComponentWrapper> list = new List<FamilyInstanceComponentWrapper>();
		list.Add(_FamilyInstanceWrapperList);
		PopulateTotalAreaInList(list, string.Empty, _Id);
	}

	private void PopulateTotalAreaInList(List<FamilyInstanceComponentWrapper> _FamilyInstanceWrapperList, LandcaddID _Id)
	{
		PopulateTotalAreaInList(_FamilyInstanceWrapperList, string.Empty, _Id);
	}

	private void PopulateTotalAreaInList(FamilyInstanceComponentWrapper _FamilyInstanceWrapperList, string _strTitle, LandcaddID _Id)
	{
		List<FamilyInstanceComponentWrapper> list = new List<FamilyInstanceComponentWrapper>();
		list.Add(_FamilyInstanceWrapperList);
		PopulateTotalAreaInList(list, _strTitle, _Id);
	}

	private void PopulateTotalAreaInList(List<FamilyInstanceComponentWrapper> _FamilyInstanceWrapperList, string _strTitle, LandcaddID _Id)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		string _strToDisplay = string.Empty;
		string empty = string.Empty;
		double _dBase = 0.0;
		double _dSubBase = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double _dParkinglotDepth = 0.0;
		double _dRightOffsetWidth = 0.0;
		double _dLeftOffsetWidth = 0.0;
		double _dAdditionalCurbWidth = 0.0;
		double LegListArea = 0.0;
		double LegListArea2 = 0.0;
		double LegListArea3 = 0.0;
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Leg eps_Leg4 = null;
		Eps_Leg eps_Leg5 = null;
		Eps_Leg eps_Leg6 = null;
		bool flag = false;
		Parameter parameter = null;
		FamilyInstance familyInstance = null;
		string strA = string.Empty;
		if (_strTitle != null)
		{
			for (int i = 0; i < _FamilyInstanceWrapperList.Count; i++)
			{
				familyInstance = _FamilyInstanceWrapperList[i].GetFirstExistingFamilyInstance();
				if (_FamilyInstanceWrapperList[i].ProposedFamilySymbol == null || familyInstance == null)
				{
					continue;
				}
				GetBaseSubBaseParameter(_FamilyInstanceWrapperList[i].ProposedFamilySymbol, _Id, ref _dBase, ref _dSubBase, ref _dParkinglotDepth);
				parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
				if (parameter != null)
				{
					strA = parameter.AsString();
				}
				if (string.Compare(strA, _strTitle, ignoreCase: true) != 0)
				{
					continue;
				}
				eps_Leg = null;
				eps_Leg2 = null;
				eps_Leg3 = null;
				_FamilyInstanceWrapperList[i].CreateOriginalLeg(m_ProposedSurface);
				if (_FamilyInstanceWrapperList[i].OriginalLeg != null && _FamilyInstanceWrapperList[i].OriginalLeg.LegCount > 0)
				{
					GetWidthParameters(_FamilyInstanceWrapperList[i].ProposedFamilySymbol, _Id, ref _dLeftOffsetWidth, ref _dRightOffsetWidth, ref _dAdditionalCurbWidth);
					if (!RevitDefines.EQ(_dRightOffsetWidth + _dLeftOffsetWidth, 0.0))
					{
						eps_Leg5 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[i].OriginalLeg, _dRightOffsetWidth, 1, ref eps_Leg5, TrimClosedObject: true);
						eps_Leg6 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[i].OriginalLeg, _dLeftOffsetWidth, 2, ref eps_Leg6, TrimClosedObject: true);
						eps_Leg = Eps_Leg.CopyLeg(eps_Leg5.Reverse());
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = eps_Leg5.LegStartPnt;
						eps_Leg4.Line.EndPnt = eps_Leg6.LegStartPnt;
						eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
						eps_Leg.Last.Next = Eps_Leg.CopyLeg(eps_Leg6);
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = eps_Leg6.LegEndPnt;
						eps_Leg4.Line.EndPnt = eps_Leg5.LegEndPnt;
						eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
					}
					if (_FamilyInstanceWrapperList[i].PrimarySiteComponentMass != null && eps_Leg != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg, ref LegListArea) == 0)
					{
						num += LegListArea;
						num2 += LegListArea * _dBase;
						num3 += LegListArea * _dSubBase;
						parameter = ((Element)_FamilyInstanceWrapperList[i].PrimarySiteComponentMass).get_Parameter(BuiltInParameter.MASS_GROSS_VOLUME);
						if (parameter != null)
						{
							num4 += parameter.AsDouble();
						}
					}
					if (_FamilyInstanceWrapperList[i].PrimarySiteComponentDirectShape != null && eps_Leg != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg, ref LegListArea) == 0)
					{
						num += LegListArea;
						num2 += LegListArea * _dBase;
						num3 += LegListArea * _dSubBase;
						num4 += RevitGeometry.GetDirectShapeVolume(_FamilyInstanceWrapperList[i].PrimarySiteComponentDirectShape);
					}
					if (!RevitDefines.EQ(_dRightOffsetWidth + _dLeftOffsetWidth, 0.0) && RevitDefines.GT(_dAdditionalCurbWidth, 0.0))
					{
						eps_Leg5 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[i].OriginalLeg, _dRightOffsetWidth, 1, ref eps_Leg5, TrimClosedObject: true);
						Eps_Leg ppOffsetLegList = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[i].OriginalLeg, _dRightOffsetWidth + _dAdditionalCurbWidth, 1, ref ppOffsetLegList, TrimClosedObject: true);
						eps_Leg6 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[i].OriginalLeg, _dLeftOffsetWidth, 2, ref eps_Leg6, TrimClosedObject: true);
						Eps_Leg ppOffsetLegList2 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[i].OriginalLeg, _dLeftOffsetWidth + _dAdditionalCurbWidth, 2, ref ppOffsetLegList2, TrimClosedObject: true);
						eps_Leg2 = Eps_Leg.CopyLeg(eps_Leg5.Reverse());
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = eps_Leg5.LegStartPnt;
						eps_Leg4.Line.EndPnt = ppOffsetLegList.LegStartPnt;
						eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
						eps_Leg2.Last.Next = Eps_Leg.CopyLeg(ppOffsetLegList);
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = ppOffsetLegList.LegEndPnt;
						eps_Leg4.Line.EndPnt = eps_Leg5.LegEndPnt;
						eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
						eps_Leg3 = Eps_Leg.CopyLeg(eps_Leg6.Reverse());
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = eps_Leg6.LegStartPnt;
						eps_Leg4.Line.EndPnt = ppOffsetLegList2.LegStartPnt;
						eps_Leg3.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
						eps_Leg3.Last.Next = Eps_Leg.CopyLeg(ppOffsetLegList2);
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = ppOffsetLegList2.LegEndPnt;
						eps_Leg4.Line.EndPnt = eps_Leg6.LegEndPnt;
						eps_Leg3.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
					}
					if (_FamilyInstanceWrapperList[i].AdditionalSiteComponentMass != null && eps_Leg2 != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg2, ref LegListArea3) == 0 && eps_Leg3 != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg3, ref LegListArea2) == 0)
					{
						flag = true;
						if (_Id != LandcaddID.Street)
						{
							num += LegListArea3 + LegListArea2;
							num2 += (LegListArea3 + LegListArea2) * _dBase;
							num3 += (LegListArea3 + LegListArea2) * _dSubBase;
							parameter = ((Element)_FamilyInstanceWrapperList[i].AdditionalSiteComponentMass).get_Parameter(BuiltInParameter.MASS_GROSS_VOLUME);
							if (parameter != null)
							{
								num4 += parameter.AsDouble();
							}
						}
						else
						{
							num5 += LegListArea3 + LegListArea2;
							num7 += (LegListArea3 + LegListArea2) * _dBase;
							num8 += (LegListArea3 + LegListArea2) * _dSubBase;
							parameter = ((Element)_FamilyInstanceWrapperList[i].AdditionalSiteComponentMass).get_Parameter(BuiltInParameter.MASS_GROSS_VOLUME);
							if (parameter != null)
							{
								num6 += parameter.AsDouble();
							}
						}
					}
					if (_FamilyInstanceWrapperList[i].AdditionalSiteComponentDirectShape != null && eps_Leg2 != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg2, ref LegListArea3) == 0 && eps_Leg3 != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg3, ref LegListArea2) == 0)
					{
						flag = true;
						if (_Id != LandcaddID.Street)
						{
							num += LegListArea3 + LegListArea2;
							num2 += (LegListArea3 + LegListArea2) * _dBase;
							num3 += (LegListArea3 + LegListArea2) * _dSubBase;
							num4 += RevitGeometry.GetDirectShapeVolume(_FamilyInstanceWrapperList[i].AdditionalSiteComponentDirectShape);
						}
						else
						{
							num5 += LegListArea3 + LegListArea2;
							num7 += (LegListArea3 + LegListArea2) * _dBase;
							num8 += (LegListArea3 + LegListArea2) * _dSubBase;
							num6 += RevitGeometry.GetDirectShapeVolume(_FamilyInstanceWrapperList[i].AdditionalSiteComponentDirectShape);
						}
					}
				}
				if (_FamilyInstanceWrapperList[i].ParkingLotTopographySurface != null)
				{
					parameter = ((Element)_FamilyInstanceWrapperList[i].ParkingLotTopographySurface.TopographySurface).get_Parameter(BuiltInParameter.SURFACE_AREA);
					if (parameter != null)
					{
						num += parameter.AsDouble();
						num2 += parameter.AsDouble() * _dBase;
						num3 += parameter.AsDouble() * _dSubBase;
						num4 += parameter.AsDouble() * _dParkinglotDepth;
					}
				}
				break;
			}
		}
		else
		{
			for (int j = 0; j < _FamilyInstanceWrapperList.Count; j++)
			{
				if (_FamilyInstanceWrapperList[j].ProposedFamilySymbol == null)
				{
					continue;
				}
				GetBaseSubBaseParameter(_FamilyInstanceWrapperList[j].ProposedFamilySymbol, _Id, ref _dBase, ref _dSubBase, ref _dParkinglotDepth);
				eps_Leg = null;
				eps_Leg2 = null;
				eps_Leg3 = null;
				_FamilyInstanceWrapperList[j].CreateOriginalLeg(m_ProposedSurface);
				if (_FamilyInstanceWrapperList[j].OriginalLeg != null && _FamilyInstanceWrapperList[j].OriginalLeg.LegCount > 0)
				{
					GetWidthParameters(_FamilyInstanceWrapperList[j].ProposedFamilySymbol, _Id, ref _dLeftOffsetWidth, ref _dRightOffsetWidth, ref _dAdditionalCurbWidth);
					if (!RevitDefines.EQ(_dRightOffsetWidth + _dLeftOffsetWidth, 0.0))
					{
						eps_Leg5 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[j].OriginalLeg, _dRightOffsetWidth, 1, ref eps_Leg5, TrimClosedObject: true);
						eps_Leg6 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[j].OriginalLeg, _dLeftOffsetWidth, 2, ref eps_Leg6, TrimClosedObject: true);
						eps_Leg = Eps_Leg.CopyLeg(eps_Leg5.Reverse());
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = eps_Leg5.LegStartPnt;
						eps_Leg4.Line.EndPnt = eps_Leg6.LegStartPnt;
						eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
						eps_Leg.Last.Next = Eps_Leg.CopyLeg(eps_Leg6);
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = eps_Leg6.LegEndPnt;
						eps_Leg4.Line.EndPnt = eps_Leg5.LegEndPnt;
						eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
					}
					if (_FamilyInstanceWrapperList[j].PrimarySiteComponentMass != null && eps_Leg != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg, ref LegListArea) == 0)
					{
						num += LegListArea;
						num2 += LegListArea * _dBase;
						num3 += LegListArea * _dSubBase;
						parameter = ((Element)_FamilyInstanceWrapperList[j].PrimarySiteComponentMass).get_Parameter(BuiltInParameter.MASS_GROSS_VOLUME);
						if (parameter != null)
						{
							num4 += parameter.AsDouble();
						}
					}
					if (_FamilyInstanceWrapperList[j].PrimarySiteComponentDirectShape != null && eps_Leg != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg, ref LegListArea) == 0)
					{
						num += LegListArea;
						num2 += LegListArea * _dBase;
						num3 += LegListArea * _dSubBase;
						num4 += RevitGeometry.GetDirectShapeVolume(_FamilyInstanceWrapperList[j].PrimarySiteComponentDirectShape);
					}
					if (!RevitDefines.EQ(_dRightOffsetWidth + _dLeftOffsetWidth, 0.0) && RevitDefines.GT(_dAdditionalCurbWidth, 0.0))
					{
						eps_Leg5 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[j].OriginalLeg, _dRightOffsetWidth, 1, ref eps_Leg5, TrimClosedObject: true);
						Eps_Leg ppOffsetLegList3 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[j].OriginalLeg, _dRightOffsetWidth + _dAdditionalCurbWidth, 1, ref ppOffsetLegList3, TrimClosedObject: true);
						eps_Leg6 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[j].OriginalLeg, _dLeftOffsetWidth, 2, ref eps_Leg6, TrimClosedObject: true);
						Eps_Leg ppOffsetLegList4 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_FamilyInstanceWrapperList[j].OriginalLeg, _dLeftOffsetWidth + _dAdditionalCurbWidth, 2, ref ppOffsetLegList4, TrimClosedObject: true);
						eps_Leg2 = Eps_Leg.CopyLeg(eps_Leg5.Reverse());
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = eps_Leg5.LegStartPnt;
						eps_Leg4.Line.EndPnt = ppOffsetLegList3.LegStartPnt;
						eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
						eps_Leg2.Last.Next = Eps_Leg.CopyLeg(ppOffsetLegList3);
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = ppOffsetLegList3.LegEndPnt;
						eps_Leg4.Line.EndPnt = eps_Leg5.LegEndPnt;
						eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
						eps_Leg3 = Eps_Leg.CopyLeg(eps_Leg6.Reverse());
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = eps_Leg6.LegStartPnt;
						eps_Leg4.Line.EndPnt = ppOffsetLegList4.LegStartPnt;
						eps_Leg3.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
						eps_Leg3.Last.Next = Eps_Leg.CopyLeg(ppOffsetLegList4);
						eps_Leg4 = new Eps_Leg();
						eps_Leg4.Line = new Eps_Line();
						eps_Leg4.Line.StartPnt = ppOffsetLegList4.LegEndPnt;
						eps_Leg4.Line.EndPnt = eps_Leg6.LegEndPnt;
						eps_Leg3.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
					}
					if (_FamilyInstanceWrapperList[j].AdditionalSiteComponentMass != null && eps_Leg2 != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg2, ref LegListArea3) == 0 && eps_Leg3 != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg3, ref LegListArea2) == 0)
					{
						flag = true;
						if (_Id != LandcaddID.Street)
						{
							num += LegListArea3 + LegListArea2;
							num2 += (LegListArea3 + LegListArea2) * _dBase;
							num3 += (LegListArea3 + LegListArea2) * _dSubBase;
							parameter = ((Element)_FamilyInstanceWrapperList[j].AdditionalSiteComponentMass).get_Parameter(BuiltInParameter.MASS_GROSS_VOLUME);
							if (parameter != null)
							{
								num4 += parameter.AsDouble();
							}
						}
						else
						{
							num5 += LegListArea3 + LegListArea2;
							num7 += (LegListArea3 + LegListArea2) * _dBase;
							num8 += (LegListArea3 + LegListArea2) * _dSubBase;
							parameter = ((Element)_FamilyInstanceWrapperList[j].AdditionalSiteComponentMass).get_Parameter(BuiltInParameter.MASS_GROSS_VOLUME);
							if (parameter != null)
							{
								num6 += parameter.AsDouble();
							}
						}
					}
					if (_FamilyInstanceWrapperList[j].AdditionalSiteComponentDirectShape != null && eps_Leg2 != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg2, ref LegListArea3) == 0 && eps_Leg3 != null && RevitGeometry.eplGeoE_CalcAreaOfLegList(eps_Leg3, ref LegListArea2) == 0)
					{
						flag = true;
						if (_Id != LandcaddID.Street)
						{
							num += LegListArea3 + LegListArea2;
							num2 += (LegListArea3 + LegListArea2) * _dBase;
							num3 += (LegListArea3 + LegListArea2) * _dSubBase;
							num4 += RevitGeometry.GetDirectShapeVolume(_FamilyInstanceWrapperList[j].AdditionalSiteComponentDirectShape);
						}
						else
						{
							num5 += LegListArea3 + LegListArea2;
							num7 += (LegListArea3 + LegListArea2) * _dBase;
							num8 += (LegListArea3 + LegListArea2) * _dSubBase;
							num6 += RevitGeometry.GetDirectShapeVolume(_FamilyInstanceWrapperList[j].AdditionalSiteComponentDirectShape);
						}
					}
				}
				if (_FamilyInstanceWrapperList[j].ParkingLotTopographySurface != null)
				{
					parameter = ((Element)_FamilyInstanceWrapperList[j].ParkingLotTopographySurface.TopographySurface).get_Parameter(BuiltInParameter.SURFACE_AREA);
					if (parameter != null)
					{
						num += parameter.AsDouble();
						num2 += parameter.AsDouble() * _dBase;
						num3 += parameter.AsDouble() * _dSubBase;
						num4 += parameter.AsDouble() * _dParkinglotDepth;
					}
				}
			}
		}
		string empty2 = string.Empty;
		empty2 = ((_strTitle != null) ? _strTitle : GetCurrentTypeString(_Id));
		empty = EPShared.LocalRM.GetString("MassGrossSurfaceAreaParameter");
		if (RevitDefines.GT(num, 0.0))
		{
			RevitGeometry.Units_ConvertDoubleToString(num, m_AreaFormatOptions.LegacyDisplayUnits(), m_AreaFormatOptions.Accuracy, m_AreaUnitSymbolType, m_Units, ref _strToDisplay);
		}
		else
		{
			_strToDisplay = EPShared.LocalRM.GetString("NotAvailableString");
		}
		PopulateList(empty2, empty, _strToDisplay);
		empty = EPShared.LocalRM.GetString("MassGrossVolumeParameter");
		if (RevitDefines.GT(num4, 0.0))
		{
			RevitGeometry.Units_ConvertDoubleToString(num4, m_VolumeFormatOptions.LegacyDisplayUnits(), m_VolumeFormatOptions.Accuracy, m_VolumeUnitSymbolType, m_Units, ref _strToDisplay);
		}
		else
		{
			_strToDisplay = EPShared.LocalRM.GetString("NotAvailableString");
		}
		PopulateList(empty, _strToDisplay);
		empty = EPShared.LocalRM.GetString("MassBaseVolume");
		if (RevitDefines.GT(num2, 0.0))
		{
			RevitGeometry.Units_ConvertDoubleToString(num2, m_VolumeFormatOptions.LegacyDisplayUnits(), m_VolumeFormatOptions.Accuracy, m_VolumeUnitSymbolType, m_Units, ref _strToDisplay);
		}
		else
		{
			_strToDisplay = EPShared.LocalRM.GetString("NotAvailableString");
		}
		PopulateList(empty, _strToDisplay);
		empty = EPShared.LocalRM.GetString("MassSubBaseVolume");
		if (RevitDefines.GT(num3, 0.0))
		{
			RevitGeometry.Units_ConvertDoubleToString(num3, m_VolumeFormatOptions.LegacyDisplayUnits(), m_VolumeFormatOptions.Accuracy, m_VolumeUnitSymbolType, m_Units, ref _strToDisplay);
		}
		else
		{
			_strToDisplay = EPShared.LocalRM.GetString("NotAvailableString");
		}
		PopulateList(empty, _strToDisplay);
		if (_Id == LandcaddID.Street && flag)
		{
			PopulateList("", "", "");
			empty = GetCurrentTypeString(LandcaddID.Curb) + " " + EPShared.LocalRM.GetString("MassGrossSurfaceAreaParameter");
			if (RevitDefines.GT(num5, 0.0))
			{
				RevitGeometry.Units_ConvertDoubleToString(num5, m_AreaFormatOptions.LegacyDisplayUnits(), m_AreaFormatOptions.Accuracy, m_AreaUnitSymbolType, m_Units, ref _strToDisplay);
			}
			else
			{
				_strToDisplay = EPShared.LocalRM.GetString("NotAvailableString");
			}
			PopulateList(empty, _strToDisplay);
			empty = GetCurrentTypeString(LandcaddID.Curb) + " " + EPShared.LocalRM.GetString("MassGrossVolumeParameter");
			if (RevitDefines.GT(num6, 0.0))
			{
				RevitGeometry.Units_ConvertDoubleToString(num6, m_AreaFormatOptions.LegacyDisplayUnits(), m_AreaFormatOptions.Accuracy, m_AreaUnitSymbolType, m_Units, ref _strToDisplay);
			}
			else
			{
				_strToDisplay = EPShared.LocalRM.GetString("NotAvailableString");
			}
			PopulateList(empty, _strToDisplay);
			empty = GetCurrentTypeString(LandcaddID.Curb) + " " + EPShared.LocalRM.GetString("MassBaseVolume");
			if (RevitDefines.GT(num7, 0.0))
			{
				RevitGeometry.Units_ConvertDoubleToString(num7, m_AreaFormatOptions.LegacyDisplayUnits(), m_AreaFormatOptions.Accuracy, m_AreaUnitSymbolType, m_Units, ref _strToDisplay);
			}
			else
			{
				_strToDisplay = EPShared.LocalRM.GetString("NotAvailableString");
			}
			PopulateList(empty, _strToDisplay);
			empty = GetCurrentTypeString(LandcaddID.Curb) + " " + EPShared.LocalRM.GetString("MassSubBaseVolume");
			if (RevitDefines.GT(num8, 0.0))
			{
				RevitGeometry.Units_ConvertDoubleToString(num8, m_AreaFormatOptions.LegacyDisplayUnits(), m_AreaFormatOptions.Accuracy, m_AreaUnitSymbolType, m_Units, ref _strToDisplay);
			}
			else
			{
				_strToDisplay = EPShared.LocalRM.GetString("NotAvailableString");
			}
			PopulateList(empty, _strToDisplay);
		}
	}

	private string GetCurrentTypeString(LandcaddID _Id)
	{
		string result = string.Empty;
		switch (_Id)
		{
		case LandcaddID.Curb:
			result = EPShared.LocalRM.GetString("CurbString");
			break;
		case LandcaddID.ParkingLot:
			result = EPShared.LocalRM.GetString("ParkingLotString");
			break;
		case LandcaddID.RetainingWall:
			result = EPShared.LocalRM.GetString("RetainingWallString");
			break;
		case LandcaddID.Sidewalk:
			result = EPShared.LocalRM.GetString("SidewalkString");
			break;
		case LandcaddID.Street:
			result = EPShared.LocalRM.GetString("StreetString");
			break;
		}
		return result;
	}

	private bool GetBaseSubBaseParameter(FamilyInstance _CurrentInstance, LandcaddID _CurrentID, ref double _dBase, ref double _dSubBase, ref double _dParkinglotDepth)
	{
		_dBase = 0.0;
		_dSubBase = 0.0;
		_dParkinglotDepth = 0.0;
		if (_CurrentInstance == null)
		{
			return false;
		}
		string name = string.Empty;
		string name2 = string.Empty;
		Parameter parameter = null;
		switch (_CurrentID)
		{
		case LandcaddID.Street:
			name = RevitParameters.StreetBaseSurfaceParameter;
			name2 = RevitParameters.StreetSubbaseSurfaceParameter;
			break;
		case LandcaddID.Sidewalk:
			name = RevitParameters.SideWalkBaseSurfaceParameter;
			name2 = RevitParameters.SideWalkSubbaseSurfaceParameter;
			break;
		case LandcaddID.Curb:
			name = RevitParameters.CurbBaseSurfaceParameter;
			name2 = RevitParameters.CurbSubbaseSurfaceParameter;
			break;
		case LandcaddID.ParkingLot:
			name = RevitParameters.ParkingLotParkingBaseDepthParameter;
			name2 = RevitParameters.ParkingLotParkingSubBaseDepthParameter;
			parameter = _CurrentInstance.LookupParameter(RevitParameters.ParkingLotPavementDepthParameter);
			if (parameter != null)
			{
				_dParkinglotDepth = parameter.AsDouble();
			}
			break;
		case LandcaddID.RetainingWall:
			name = RevitParameters.RetainingWallBaseSurfaceParameter;
			name2 = RevitParameters.RetainingWallSubbaseSurfaceParameter;
			break;
		}
		parameter = _CurrentInstance.LookupParameter(name);
		if (parameter != null)
		{
			_dBase = parameter.AsDouble();
		}
		parameter = _CurrentInstance.LookupParameter(name2);
		if (parameter != null)
		{
			_dSubBase = parameter.AsDouble();
		}
		return true;
	}

	private bool GetBaseSubBaseParameter(FamilySymbol _CurrentSymbol, LandcaddID _CurrentID, ref double _dBase, ref double _dSubBase, ref double _dParkinglotDepth)
	{
		_dBase = 0.0;
		_dSubBase = 0.0;
		_dParkinglotDepth = 0.0;
		if (_CurrentSymbol == null)
		{
			return false;
		}
		string name = string.Empty;
		string name2 = string.Empty;
		Parameter parameter = null;
		switch (_CurrentID)
		{
		case LandcaddID.Street:
			name = RevitParameters.StreetBaseSurfaceParameter;
			name2 = RevitParameters.StreetSubbaseSurfaceParameter;
			break;
		case LandcaddID.Sidewalk:
			name = RevitParameters.SideWalkBaseSurfaceParameter;
			name2 = RevitParameters.SideWalkSubbaseSurfaceParameter;
			break;
		case LandcaddID.Curb:
			name = RevitParameters.CurbBaseSurfaceParameter;
			name2 = RevitParameters.CurbSubbaseSurfaceParameter;
			break;
		case LandcaddID.ParkingLot:
			name = RevitParameters.ParkingLotParkingBaseDepthParameter;
			name2 = RevitParameters.ParkingLotParkingSubBaseDepthParameter;
			parameter = _CurrentSymbol.LookupParameter(RevitParameters.ParkingLotPavementDepthParameter);
			if (parameter != null)
			{
				_dParkinglotDepth = parameter.AsDouble();
			}
			break;
		case LandcaddID.RetainingWall:
			name = RevitParameters.RetainingWallBaseSurfaceParameter;
			name2 = RevitParameters.RetainingWallSubbaseSurfaceParameter;
			break;
		}
		parameter = _CurrentSymbol.LookupParameter(name);
		if (parameter != null)
		{
			_dBase = parameter.AsDouble();
		}
		parameter = _CurrentSymbol.LookupParameter(name2);
		if (parameter != null)
		{
			_dSubBase = parameter.AsDouble();
		}
		return true;
	}

	private bool GetWidthParameters(FamilySymbol _CurrentSymbol, LandcaddID _CurrentID, ref double _dLeftOffsetWidth, ref double _dRightOffsetWidth, ref double _dAdditionalCurbWidth)
	{
		_dLeftOffsetWidth = 0.0;
		_dRightOffsetWidth = 0.0;
		_dAdditionalCurbWidth = 0.0;
		if (_CurrentSymbol == null)
		{
			return false;
		}
		string empty = string.Empty;
		string empty2 = string.Empty;
		Parameter parameter = null;
		switch (_CurrentID)
		{
		case LandcaddID.SoftTerrain:
			parameter = _CurrentSymbol.LookupParameter(RevitParameters.SoftTerrainLeftOffsetWidthParameter);
			if (parameter != null)
			{
				_dLeftOffsetWidth = parameter.AsDouble();
			}
			parameter = _CurrentSymbol.LookupParameter(RevitParameters.SoftTerrainRightOffsetWidthParameter);
			if (parameter != null)
			{
				_dRightOffsetWidth = parameter.AsDouble();
			}
			break;
		case LandcaddID.Sidewalk:
			parameter = _CurrentSymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
			if (parameter != null)
			{
				double num4 = parameter.AsDouble();
				_dLeftOffsetWidth = num4 / 2.0;
				_dRightOffsetWidth = num4 / 2.0;
			}
			break;
		case LandcaddID.Curb:
		{
			double num5 = 0.0;
			double num6 = 0.0;
			parameter = _CurrentSymbol.LookupParameter(RevitParameters.StreetGutterWidthParameter);
			if (parameter != null)
			{
				num6 = parameter.AsDouble();
			}
			parameter = _CurrentSymbol.LookupParameter(RevitParameters.StreetCurbWidthParameter);
			if (parameter != null)
			{
				num5 = parameter.AsDouble();
			}
			parameter = _CurrentSymbol.LookupParameter(RevitParameters.UseRightSideGenericFamilyParameter);
			if (parameter != null && parameter.AsInteger() == 1)
			{
				_dRightOffsetWidth = num6 + num5;
				_dLeftOffsetWidth = 0.0;
			}
			else
			{
				_dLeftOffsetWidth = num6 + num5;
				_dRightOffsetWidth = 0.0;
			}
			break;
		}
		case LandcaddID.RetainingWall:
			parameter = _CurrentSymbol.LookupParameter(RevitParameters.RetainingWallWidthParameter);
			if (parameter != null)
			{
				double num3 = parameter.AsDouble();
				_dLeftOffsetWidth = num3 / 2.0;
				_dRightOffsetWidth = num3 / 2.0;
			}
			break;
		case LandcaddID.Street:
		{
			double num = 0.0;
			parameter = _CurrentSymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
			if (parameter != null)
			{
				num = parameter.AsDouble();
			}
			parameter = _CurrentSymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
			if (parameter != null)
			{
				_dLeftOffsetWidth = num * (double)parameter.AsInteger() / 2.0;
				_dRightOffsetWidth = num * (double)parameter.AsInteger() / 2.0;
			}
			parameter = _CurrentSymbol.LookupParameter(RevitParameters.StreetCurbDisplayParameter);
			if (parameter != null && parameter.AsInteger() != 0)
			{
				double num2 = 0.0;
				parameter = _CurrentSymbol.LookupParameter(RevitParameters.StreetGutterWidthParameter);
				if (parameter != null)
				{
					_dAdditionalCurbWidth = (_dAdditionalCurbWidth = parameter.AsDouble());
				}
				parameter = _CurrentSymbol.LookupParameter(RevitParameters.StreetCurbWidthParameter);
				if (parameter != null)
				{
					_dAdditionalCurbWidth += parameter.AsDouble();
					_dAdditionalCurbWidth += parameter.AsDouble();
				}
			}
			break;
		}
		}
		return true;
	}

	private void OnExportFileButtonClick(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		if (m_CommaExportRadioButton.Checked)
		{
			saveFileDialog.Filter = ApplicationUtility.GetString("CSVAndTxtFilePickerFilter");
		}
		else
		{
			saveFileDialog.Filter = ApplicationUtility.GetString("TxtFilePickerFilter");
		}
		saveFileDialog.FilterIndex = 1;
		saveFileDialog.RestoreDirectory = true;
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			m_ExportFileTextBox.Text = saveFileDialog.FileName;
			SiteDesigner.UI.WpfFocus.Focus(m_ExportFileTextBox);
		}
	}

	private void AddBlankLine()
	{
		if (m_bExportToFile)
		{
			m_ReportLineList.Add(new ExportReportLine());
		}
	}

	private void PopulateList(string _strParameter, string _strValue)
	{
		PopulateList(string.Empty, _strParameter, _strValue);
	}

	private void PopulateList(string _strLevel, string _strParameter, string _strValue)
	{
		if (!m_bExportToFile)
		{
			ListViewItem listViewItem = m_ReportListView.Items.Add(_strParameter, _strParameter, string.Empty);
			listViewItem.Tag = _strParameter;
			listViewItem.SubItems.Add(_strValue);
		}
		else
		{
			m_ReportLineList.Add(new ExportReportLine(_strLevel, _strParameter, _strValue));
		}
	}

	private bool ValidateFields()
	{
		string value = m_ExportFileTextBox.Text;
		if (string.IsNullOrEmpty(value))
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("SpecifyFolderFileNameMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			SiteDesigner.UI.WpfFocus.Focus(m_ExportFileTextBox);
			return false;
		}
		string empty = string.Empty;
		empty = ((!m_CommaExportRadioButton.Checked) ? ".txt" : ".csv");
		if (m_ExportFileTextBox.Text.IndexOf(empty) < 0)
		{
			if (m_ExportFileTextBox.Text.IndexOf("\\") <= 0 && m_ExportFileTextBox.Text.IndexOf("/") <= 0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidDirectoryMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return false;
			}
			int length = -1;
			if (m_ExportFileTextBox.Text.IndexOf("\\") > 0)
			{
				length = m_ExportFileTextBox.Text.LastIndexOf("\\");
			}
			else if (m_ExportFileTextBox.Text.IndexOf("/") > 0)
			{
				length = m_ExportFileTextBox.Text.LastIndexOf("/");
			}
			if (!Directory.Exists(m_ExportFileTextBox.Text.Substring(0, length)))
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidDirectoryMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return false;
			}
			m_ExportFileTextBox.Text += empty;
		}
		else
		{
			if (m_ExportFileTextBox.Text.IndexOf("\\") <= 0 && m_ExportFileTextBox.Text.IndexOf("/") <= 0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidDirectoryMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return false;
			}
			int length2 = -1;
			if (m_ExportFileTextBox.Text.IndexOf("\\") > 0)
			{
				length2 = m_ExportFileTextBox.Text.LastIndexOf("\\");
			}
			else if (m_ExportFileTextBox.Text.IndexOf("/") > 0)
			{
				length2 = m_ExportFileTextBox.Text.LastIndexOf("/");
			}
			if (!Directory.Exists(m_ExportFileTextBox.Text.Substring(0, length2)))
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidDirectoryMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return false;
			}
		}
		return true;
	}

	private void InitializeControls()
	{
		int _nReturnValue = 0;
		string _strReturnValue = string.Empty;
		GetDialogPosition();
		m_CommaExportRadioButton.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_strSettingsPrefix + "ExportOption", ref _nReturnValue))
		{
			if (_nReturnValue == 0)
			{
				m_CommaExportRadioButton.Checked = true;
			}
			else
			{
				m_TabExportRadioButton.Checked = true;
			}
		}
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_strSettingsPrefix + "ExportFileName", ref _strReturnValue))
		{
			m_ExportFileTextBox.Text = _strReturnValue;
		}
	}

	private void SaveSettings()
	{
		int num = 0;
		num = ((!m_CommaExportRadioButton.Checked) ? 1 : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting(m_strSettingsPrefix + "ExportOption", num);
		if (m_ExportFileTextBox.Text != string.Empty)
		{
			ApplicationUtility.Settings.eplSetE_SetSetting(m_strSettingsPrefix + "ExportFileName", m_ExportFileTextBox.Text);
		}
	}

	private void OnSiteComponentReportClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.m_SiteReportGroupBox = new System.Windows.Forms.GroupBox();
		this.m_ReportListView = new System.Windows.Forms.ListView();
		this.m_ReportTreeView = new System.Windows.Forms.TreeView();
		this.m_CloseButton = new System.Windows.Forms.Button();
		this.m_ExportButton = new System.Windows.Forms.Button();
		this.m_ExportOptionsGroupBox = new System.Windows.Forms.GroupBox();
		this.m_TabExportRadioButton = new System.Windows.Forms.RadioButton();
		this.m_CommaExportRadioButton = new System.Windows.Forms.RadioButton();
		this.m_ExportFileTextBox = new System.Windows.Forms.TextBox();
		this.m_ExportFileButton = new System.Windows.Forms.Button();
		this.m_ExportFileLabel = new System.Windows.Forms.Label();
		this.m_SiteReportGroupBox.SuspendLayout();
		this.m_ExportOptionsGroupBox.SuspendLayout();
		base.SuspendLayout();
		this.m_SiteReportGroupBox.Controls.Add(this.m_ReportListView);
		this.m_SiteReportGroupBox.Controls.Add(this.m_ReportTreeView);
		this.m_SiteReportGroupBox.Location = new System.Drawing.Point(12, 5);
		this.m_SiteReportGroupBox.Name = "m_SiteReportGroupBox";
		this.m_SiteReportGroupBox.Size = new System.Drawing.Size(532, 308);
		this.m_SiteReportGroupBox.TabIndex = 0;
		this.m_SiteReportGroupBox.TabStop = false;
		this.m_SiteReportGroupBox.Text = "Site Component Report";
		this.m_ReportListView.Location = new System.Drawing.Point(214, 16);
		this.m_ReportListView.MultiSelect = false;
		this.m_ReportListView.Name = "m_ReportListView";
		this.m_ReportListView.Size = new System.Drawing.Size(312, 285);
		this.m_ReportListView.TabIndex = 1;
		this.m_ReportListView.UseCompatibleStateImageBehavior = false;
		this.m_ReportListView.View = System.Windows.Forms.View.Details;
		this.m_ReportTreeView.Location = new System.Drawing.Point(6, 16);
		this.m_ReportTreeView.Name = "m_ReportTreeView";
		this.m_ReportTreeView.Size = new System.Drawing.Size(202, 285);
		this.m_ReportTreeView.TabIndex = 0;
		this.m_ReportTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ReportTreeViewAfterSelect);
		this.m_CloseButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_CloseButton.Location = new System.Drawing.Point(470, 405);
		this.m_CloseButton.Name = "m_CloseButton";
		this.m_CloseButton.Size = new System.Drawing.Size(76, 23);
		this.m_CloseButton.TabIndex = 3;
		this.m_CloseButton.Text = "&Close";
		this.m_CloseButton.UseVisualStyleBackColor = true;
		this.m_CloseButton.Click += new System.EventHandler(this.OnCloseButtonClicked);
		this.m_ExportButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_ExportButton.Location = new System.Drawing.Point(388, 405);
		this.m_ExportButton.Name = "m_ExportButton";
		this.m_ExportButton.Size = new System.Drawing.Size(76, 23);
		this.m_ExportButton.TabIndex = 2;
		this.m_ExportButton.Text = "&Export";
		this.m_ExportButton.UseVisualStyleBackColor = true;
		this.m_ExportButton.Click += new System.EventHandler(this.OnExportButtonClicked);
		this.m_ExportOptionsGroupBox.Controls.Add(this.m_TabExportRadioButton);
		this.m_ExportOptionsGroupBox.Controls.Add(this.m_CommaExportRadioButton);
		this.m_ExportOptionsGroupBox.Controls.Add(this.m_ExportFileTextBox);
		this.m_ExportOptionsGroupBox.Controls.Add(this.m_ExportFileButton);
		this.m_ExportOptionsGroupBox.Controls.Add(this.m_ExportFileLabel);
		this.m_ExportOptionsGroupBox.Location = new System.Drawing.Point(12, 320);
		this.m_ExportOptionsGroupBox.Name = "m_ExportOptionsGroupBox";
		this.m_ExportOptionsGroupBox.Size = new System.Drawing.Size(532, 77);
		this.m_ExportOptionsGroupBox.TabIndex = 1;
		this.m_ExportOptionsGroupBox.TabStop = false;
		this.m_ExportOptionsGroupBox.Text = "Export Options";
		this.m_TabExportRadioButton.AutoSize = true;
		this.m_TabExportRadioButton.Location = new System.Drawing.Point(181, 20);
		this.m_TabExportRadioButton.Name = "m_TabExportRadioButton";
		this.m_TabExportRadioButton.Size = new System.Drawing.Size(139, 17);
		this.m_TabExportRadioButton.TabIndex = 4;
		this.m_TabExportRadioButton.TabStop = true;
		this.m_TabExportRadioButton.Text = "&Tab Delimited (Text File)";
		this.m_TabExportRadioButton.UseVisualStyleBackColor = true;
		this.m_CommaExportRadioButton.AutoSize = true;
		this.m_CommaExportRadioButton.Location = new System.Drawing.Point(6, 20);
		this.m_CommaExportRadioButton.Name = "m_CommaExportRadioButton";
		this.m_CommaExportRadioButton.Size = new System.Drawing.Size(155, 17);
		this.m_CommaExportRadioButton.TabIndex = 3;
		this.m_CommaExportRadioButton.TabStop = true;
		this.m_CommaExportRadioButton.Text = "&Comma Delimited (Text File)";
		this.m_CommaExportRadioButton.UseVisualStyleBackColor = true;
		this.m_ExportFileTextBox.Location = new System.Drawing.Point(58, 43);
		this.m_ExportFileTextBox.Name = "m_ExportFileTextBox";
		this.m_ExportFileTextBox.Size = new System.Drawing.Size(438, 20);
		this.m_ExportFileTextBox.TabIndex = 6;
		this.m_ExportFileButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.m_ExportFileButton.Location = new System.Drawing.Point(502, 43);
		this.m_ExportFileButton.Name = "m_ExportFileButton";
		this.m_ExportFileButton.Size = new System.Drawing.Size(24, 20);
		this.m_ExportFileButton.TabIndex = 7;
		this.m_ExportFileButton.TabStop = false;
		this.m_ExportFileButton.Text = "...";
		this.m_ExportFileButton.UseVisualStyleBackColor = true;
		this.m_ExportFileButton.Click += new System.EventHandler(this.OnExportFileButtonClick);
		this.m_ExportFileLabel.AutoSize = true;
		this.m_ExportFileLabel.Location = new System.Drawing.Point(6, 46);
		this.m_ExportFileLabel.Name = "m_ExportFileLabel";
		this.m_ExportFileLabel.Size = new System.Drawing.Size(26, 13);
		this.m_ExportFileLabel.TabIndex = 5;
		this.m_ExportFileLabel.Text = "Fi&le:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(558, 435);
		base.Controls.Add(this.m_ExportOptionsGroupBox);
		base.Controls.Add(this.m_ExportButton);
		base.Controls.Add(this.m_CloseButton);
		base.Controls.Add(this.m_SiteReportGroupBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.MinimumSize = new System.Drawing.Size(564, 467);
		base.Name = "SiteComponentReport";
		this.Text = "Site Component Report";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnSiteComponentReportClosing);
		this.m_SiteReportGroupBox.ResumeLayout(false);
		this.m_ExportOptionsGroupBox.ResumeLayout(false);
		this.m_ExportOptionsGroupBox.PerformLayout();
		base.ResumeLayout(false);
		base.AcceptButton = this.m_ExportButton;
		base.CancelButton = this.m_CloseButton;
	}
}
