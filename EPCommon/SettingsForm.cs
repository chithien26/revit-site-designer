using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

namespace EPCommon;

public class SettingsForm : EPForm
{
	private Units m_Units;

	private const UnitType m_LengthUnitType = UnitType.UT_Length;

	private const UnitType m_SlopeUnitType = UnitType.UT_Slope;

	private const UnitType m_AngularUnitType = UnitType.UT_Angle;

	private Document m_CurrentDocument;

	private RevitProduct m_CurrentProduct = RevitProduct.SITES;

	private QuantumListView m_GlobalListView;

	private List<string> m_ParameterList = new List<string>();

	private List<string> m_ParametersStringList = new List<string>();

	private List<string> m_ParametersValueList = new List<string>();

	private List<string> m_TabList = new List<string>();

	private QuantumListView m_ParameterListView;

	private DecimalSymbol m_DecimalSymbolType;

	private FormatOptions m_FormatOptions;

	private UnitSymbolType m_UnitSymbolType;

	private DisplayUnit m_DisplayUnitSystem;

	private List<string> m_Fields = new List<string>();

	private List<string> m_Values = new List<string>();

	private List<Material> m_AllMaterialList = new List<Material>();

	private IContainer components;

	private Button m_ButtonOk;

	private Button m_ButtonCancel;

	private TabControl m_SettingsTabControl;

	private System.Windows.Forms.Panel m_Panel;

	public SettingsForm(Document _CurrentDocument, RevitProduct _nProductType)
	{
		m_CurrentDocument = _CurrentDocument;
		m_CurrentProduct = _nProductType;
		m_DecimalSymbolType = m_CurrentDocument.GetUnits().DecimalSymbol;
		m_DisplayUnitSystem = m_CurrentDocument.DisplayUnitSystem;
		m_FormatOptions = m_CurrentDocument.GetUnits().GetFormatOptions(UnitType.UT_Length);
		ContextualHelpID = "HID_SDsettings";
		m_Units = _CurrentDocument.GetUnits();
		try
		{
			m_UnitSymbolType = m_FormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_UnitSymbolType = UnitSymbolType.UST_NONE;
		}
		InitializeComponent();
		InitializeData();
		FormClosing += OnSettingsFormClosing;
		ResizeBegin += FormResizeBegin;
	}

	private void InitializeData()
	{
		string empty = string.Empty;
		string _strTemplateFileName = string.Empty;
		if (m_CurrentProduct == RevitProduct.SITES)
		{
			m_ParametersStringList.Add("MassTemplateFileName");
			empty = GetStringSetting("MassTemplateFileName");
			if (empty == string.Empty)
			{
				RevitUtility.FindMassTemplateFileName(m_CurrentDocument, LandcaddID.SoftTerrain, ref _strTemplateFileName, _bIssueWarning: false);
				empty = _strTemplateFileName;
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			_strTemplateFileName = string.Empty;
			m_ParametersStringList.Add("CurbMassTemplateFileName");
			empty = GetStringSetting("CurbMassTemplateFileName");
			if (empty == string.Empty)
			{
				RevitUtility.FindMassTemplateFileName(m_CurrentDocument, LandcaddID.Curb, ref _strTemplateFileName, _bIssueWarning: false);
				empty = _strTemplateFileName;
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			_strTemplateFileName = string.Empty;
			m_ParametersStringList.Add("RetainingWallMassTemplateFileName");
			empty = GetStringSetting("RetainingWallMassTemplateFileName");
			if (empty == string.Empty)
			{
				RevitUtility.FindMassTemplateFileName(m_CurrentDocument, LandcaddID.RetainingWall, ref _strTemplateFileName, _bIssueWarning: false);
				empty = _strTemplateFileName;
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			_strTemplateFileName = string.Empty;
			m_ParametersStringList.Add("SidewalkMassTemplateFileName");
			empty = GetStringSetting("SidewalkMassTemplateFileName");
			if (empty == string.Empty)
			{
				RevitUtility.FindMassTemplateFileName(m_CurrentDocument, LandcaddID.Sidewalk, ref _strTemplateFileName, _bIssueWarning: false);
				empty = _strTemplateFileName;
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			_strTemplateFileName = string.Empty;
			m_ParametersStringList.Add("StreetMassTemplateFileName");
			empty = GetStringSetting("StreetMassTemplateFileName");
			if (empty == string.Empty)
			{
				RevitUtility.FindMassTemplateFileName(m_CurrentDocument, LandcaddID.Street, ref _strTemplateFileName, _bIssueWarning: false);
				empty = _strTemplateFileName;
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			_strTemplateFileName = string.Empty;
			m_ParametersStringList.Add("MassCreationOnlyMode");
			empty = GetIntSetting("MassCreationOnlyMode");
			if (empty == string.Empty)
			{
				empty = "0";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("CurbMassSubCategoryName");
			empty = GetStringSetting("CurbMassSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWCurbLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("RetainingWallMassSubCategoryName");
			empty = GetStringSetting("RetainingWallMassSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWRetainingWallLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("SidewalkMassSubCategoryName");
			empty = GetStringSetting("SidewalkMassSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWSidewalkLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("StreetMassSubCategoryName");
			empty = GetStringSetting("StreetMassSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWStreetLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("CurbLineSubCategoryName");
			empty = GetStringSetting("CurbLineSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWCurbLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("CurbLineSubCategoryColor");
			empty = GetStringSetting("CurbLineSubCategoryColor");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("RGBString") + " " + ApplicationUtility.GetString("DefaultLineColor");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("CurbLineSubCategoryLineWeight");
			empty = GetIntSetting("CurbLineSubCategoryLineWeight");
			if (empty == string.Empty)
			{
				empty = "1";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("FeatureLineLineSubCategoryName");
			empty = GetStringSetting("FeatureLineLineSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWFeatureLineLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("FeatureLineLineSubCategoryColor");
			empty = GetStringSetting("FeatureLineLineSubCategoryColor");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("RGBString") + " " + ApplicationUtility.GetString("DefaultLineColor");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("FeatureLineLineSubCategoryLineWeight");
			empty = GetIntSetting("FeatureLineLineSubCategoryLineWeight");
			if (empty == string.Empty)
			{
				empty = "1";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("ParkingLotLineSubCategoryName");
			empty = GetStringSetting("ParkingLotLineSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWParkingLotLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("ParkingLotLineSubCategoryColor");
			empty = GetStringSetting("ParkingLotLineSubCategoryColor");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("RGBString") + " " + ApplicationUtility.GetString("DefaultLineColor");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("ParkingLotLineSubCategoryLineWeight");
			empty = GetIntSetting("ParkingLotLineSubCategoryLineWeight");
			if (empty == string.Empty)
			{
				empty = "1";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("PointWipeoutLineSubCategoryName");
			empty = GetStringSetting("PointWipeoutLineSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWPointWipeoutLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("PointWipeoutLineSubCategoryColor");
			empty = GetStringSetting("PointWipeoutLineSubCategoryColor");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("RGBString") + " " + ApplicationUtility.GetString("DefaultLineColor");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("PointWipeoutLineSubCategoryLineWeight");
			empty = GetIntSetting("PointWipeoutLineSubCategoryLineWeight");
			if (empty == string.Empty)
			{
				empty = "1";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("RetainingWallLineSubCategoryName");
			empty = GetStringSetting("RetainingWallLineSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWRetainingWallLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("RetainingWallLineSubCategoryColor");
			empty = GetStringSetting("RetainingWallLineSubCategoryColor");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("RGBString") + " " + ApplicationUtility.GetString("DefaultLineColor");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("RetainingWallLineSubCategoryLineWeight");
			empty = GetIntSetting("RetainingWallLineSubCategoryLineWeight");
			if (empty == string.Empty)
			{
				empty = "1";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("SidewalkLineSubCategoryName");
			empty = GetStringSetting("SidewalkLineSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWSidewalkLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("SidewalkLineSubCategoryColor");
			empty = GetStringSetting("SidewalkLineSubCategoryColor");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("RGBString") + " " + ApplicationUtility.GetString("DefaultLineColor");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("SidewalkLineSubCategoryLineWeight");
			empty = GetIntSetting("SidewalkLineSubCategoryLineWeight");
			if (empty == string.Empty)
			{
				empty = "1";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("SoftTerrainLineSubCategoryName");
			empty = GetStringSetting("SoftTerrainLineSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWSoftTerrainLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("SoftTerrainLineSubCategoryColor");
			empty = GetStringSetting("SoftTerrainLineSubCategoryColor");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("RGBString") + " " + ApplicationUtility.GetString("DefaultLineColor");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("SoftTerrainLineSubCategoryLineWeight");
			empty = GetIntSetting("SoftTerrainLineSubCategoryLineWeight");
			if (empty == string.Empty)
			{
				empty = "1";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("StreetLineSubCategoryName");
			empty = GetStringSetting("StreetLineSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWStreetLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("StreetLineSubCategoryColor");
			empty = GetStringSetting("StreetLineSubCategoryColor");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("RGBString") + " " + ApplicationUtility.GetString("DefaultLineColor");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("StreetLineSubCategoryLineWeight");
			empty = GetIntSetting("StreetLineSubCategoryLineWeight");
			if (empty == string.Empty)
			{
				empty = "1";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("IntersectionLineSubCategoryName");
			empty = GetStringSetting("IntersectionLineSubCategoryName");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("SWIntersectionLineNameString");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("IntersectionLineSubCategoryColor");
			empty = GetStringSetting("IntersectionLineSubCategoryColor");
			if (empty == string.Empty)
			{
				empty = EPShared.LocalRM.GetString("RGBString") + " " + ApplicationUtility.GetString("RedLineColor");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("IntersectionLineSubCategoryLineWeight");
			empty = GetIntSetting("IntersectionLineSubCategoryLineWeight");
			if (empty == string.Empty)
			{
				empty = "3";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("TopographySurfaceArcPointInterval");
			empty = GetDoubleSetting("TopographySurfaceArcPointInterval");
			if (empty == string.Empty)
			{
				empty = "1.0";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("ProposedToposurfaceMaterialParameter");
			empty = GetProjectIntSetting("ProposedToposurfaceMaterialParameter");
			if (empty == string.Empty)
			{
				empty = "0";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("ParkingLotIslandToposurfaceMaterialParameter");
			empty = GetProjectIntSetting("ParkingLotIslandToposurfaceMaterialParameter");
			if (empty == string.Empty)
			{
				empty = "0";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("SiteComponentPromptForStartingSide");
			empty = GetIntSetting("SiteComponentPromptForStartingSide");
			if (empty == string.Empty)
			{
				empty = "0";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("UseSharedElevation");
			empty = GetProjectIntSetting("UseSharedElevation");
			if (empty == string.Empty)
			{
				empty = "0";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("StreetIntersectionDefaultRadius");
			empty = GetDoubleSetting("StreetIntersectionDefaultRadius");
			if (empty == string.Empty)
			{
				empty = "5.0";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("SidewalkIntersectionDefaultRadius");
			empty = GetDoubleSetting("SidewalkIntersectionDefaultRadius");
			if (empty == string.Empty)
			{
				empty = "5.0";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("StreetIntersectionDefaultTransitioningDistance");
			empty = GetDoubleSetting("StreetIntersectionDefaultTransitioningDistance");
			if (empty == string.Empty)
			{
				empty = "10.0";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("SidewalkIntersectionDefaultTransitioningDistance");
			empty = GetDoubleSetting("SidewalkIntersectionDefaultTransitioningDistance");
			if (empty == string.Empty)
			{
				empty = "10.0";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
		}
		else if (m_CurrentProduct == RevitProduct.LANDCADD)
		{
			m_ParametersStringList.Add("PlantingTemplateFileName");
			empty = GetStringSetting("PlantingTemplateFileName");
			if (empty == string.Empty)
			{
				ManagePlants.FindTemplateFileName(m_CurrentDocument, ref _strTemplateFileName, _bIssueWarning: false);
				empty = _strTemplateFileName;
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			_strTemplateFileName = string.Empty;
			m_ParametersStringList.Add("GenericTemplateFileName");
			empty = GetStringSetting("GenericTemplateFileName");
			if (empty == string.Empty)
			{
				ManagePlants.FindMixTemplateFileName(m_CurrentDocument, ref _strTemplateFileName, _bIssueWarning: false);
				empty = _strTemplateFileName;
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			_strTemplateFileName = string.Empty;
			m_ParametersStringList.Add("LANDCADDUpdaterCall");
			empty = GetIntSetting("LANDCADDUpdaterCall");
			if (empty == string.Empty)
			{
				empty = "1";
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
			m_ParametersStringList.Add("DisplaySharedParamaterWarningString");
			empty = GetIntSetting("MissingSharedParametersDoNotShowAgainOption");
			if (empty == string.Empty)
			{
				empty = "1";
			}
			else
			{
				empty = ((Convert.ToInt32(empty) != 1) ? "1" : "0");
			}
			m_ParametersValueList.Add(empty);
			empty = string.Empty;
		}
		m_ParametersStringList.Add("CheckForUpdateNotification");
		empty = GetIntSetting("CheckForUpdateNotificationDoNotShowAgainOption");
		if (empty == string.Empty)
		{
			empty = "1";
		}
		else
		{
			empty = ((Convert.ToInt32(empty) != 1) ? "1" : "0");
		}
		m_ParametersValueList.Add(empty);
		empty = string.Empty;
		m_ParametersStringList.Add("LibraryPath");
		empty = GetStringSetting("LibraryPath");
		if (empty == string.Empty)
		{
			ApplicationUtility.RegistryPaths.GetLibraryPath(string.Empty, ref empty);
		}
		m_ParametersValueList.Add(empty);
		empty = string.Empty;
		m_ParametersStringList.Add("SamplePath");
		empty = GetStringSetting("SamplePath");
		if (empty == string.Empty)
		{
			ApplicationUtility.RegistryPaths.GetSamplePath(string.Empty, ref empty);
		}
		m_ParametersValueList.Add(empty);
		empty = string.Empty;
		string text = string.Empty;
		switch (m_CurrentProduct)
		{
		case RevitProduct.LANDCADD:
			text = EPShared.LocalRM.GetString("LandcaddRibbonTab") + " " + EPShared.LocalRM.GetString("SettingRibbonButton");
			break;
		case RevitProduct.SITES:
			text = EPShared.LocalRM.GetString("LandcaddSitesRibbonTab") + " " + EPShared.LocalRM.GetString("SettingRibbonButton");
			break;
		}
		Text = text;
		m_ButtonOk.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		m_ButtonCancel.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		GetDialogPosition();
		SetupTabControl();
	}

	private string GetStringSetting(string _strSettingName)
	{
		string _strReturnValue = string.Empty;
		ApplicationUtility.Settings.eplSetE_GetSetting(_strSettingName, ref _strReturnValue);
		return _strReturnValue;
	}

	private string GetIntSetting(string _strSettingName)
	{
		int _nReturnValue = 0;
		if (ApplicationUtility.Settings.eplSetE_GetSetting(_strSettingName, ref _nReturnValue))
		{
			return Convert.ToString(_nReturnValue);
		}
		return string.Empty;
	}

	private string GetProjectIntSetting(string _strSettingName)
	{
		int _nReturnValue = 0;
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_CurrentDocument.PathName, _strSettingName, ref _nReturnValue))
		{
			return Convert.ToString(_nReturnValue);
		}
		return string.Empty;
	}

	private string GetDoubleSetting(string _strSettingName)
	{
		double _dReturnValue = 0.0;
		if (ApplicationUtility.Settings.eplSetE_GetSetting(_strSettingName, ref _dReturnValue))
		{
			return Convert.ToString(_dReturnValue);
		}
		return string.Empty;
	}

	private void SetupTabControl()
	{
		SetupParameterListTabList();
		m_GlobalListView = new QuantumListView();
		m_SettingsTabControl.SelectedIndex = 0;
		m_SettingsTabControl.TabIndex = 0;
		SetupSettingsDlg();
		SetupGlobalListViewInfoForAdd();
		m_SettingsTabControl.TabPages.Clear();
		int num = 0;
		foreach (string tab in m_TabList)
		{
			m_SettingsTabControl.TabPages.Add(tab);
			m_SettingsTabControl.TabPages[num++].AutoScroll = true;
		}
		SetupSettingsViewInfo(m_SettingsTabControl.TabPages[0].Text);
	}

	private void SetupSettingsDlg()
	{
		int num = Width;
		int num2 = Height;
		if (Environment.OSVersion.Version.Major == 5)
		{
			num2 -= 95;
			num -= 30;
		}
		else if (Environment.OSVersion.Version.Major == 6)
		{
			if (Environment.OSVersion.Version.Minor == 0)
			{
				num2 -= 95;
				num -= 35;
			}
			else
			{
				num2 -= 110;
				num -= 35;
			}
		}
		m_ParameterListView = new QuantumListView(num, num2);
		m_ParameterListView.View = System.Windows.Forms.View.Details;
		m_ParameterListView.Columns.Add(EPShared.LocalRM.GetString("Parameter"));
		m_ParameterListView.Columns.Add(EPShared.LocalRM.GetString("Value"));
		m_ParameterListView.Columns[0].Width = num / 2;
		m_ParameterListView.Columns[1].Width = num / 2;
		m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("NoString"));
		m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("YesString"));
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		IList<Element> list = filteredElementCollector.OfClass(typeof(Material)).OfCategory(BuiltInCategory.OST_Materials).ToElements();
		foreach (Element item in list)
		{
			if (item != null && item is Material material)
			{
				m_ParameterListView.MaterialCmbBox.Items.Add(material.Name);
				m_AllMaterialList.Add(material);
			}
		}
		m_SettingsTabControl.Location = new System.Drawing.Point(3, 3);
	}

	private void SetupGlobalListViewInfoForAdd()
	{
		string empty = string.Empty;
		ListViewItem listViewItem = new ListViewItem();
		List<string> list = new List<string>();
		if (m_CurrentProduct == RevitProduct.LANDCADD)
		{
			list.Add(EPShared.LocalRM.GetString("PlantingTemplateFileNameString"));
			list.Add(EPShared.LocalRM.GetString("GenericTemplateFileNameString"));
			list.Add(EPShared.LocalRM.GetString("DisplaySharedParamaterWarningString"));
			list.Add(EPShared.LocalRM.GetString("LANDCADDUpdaterCall"));
		}
		else if (m_CurrentProduct == RevitProduct.SITES)
		{
			list.Add(EPShared.LocalRM.GetString("MassTemplateFileNameString"));
			list.Add(EPShared.LocalRM.GetString("CurbMassTemplateFileNameString"));
			list.Add(EPShared.LocalRM.GetString("RetainingWallMassTemplateFileNameString"));
			list.Add(EPShared.LocalRM.GetString("SidewalkMassTemplateFileNameString"));
			list.Add(EPShared.LocalRM.GetString("StreetMassTemplateFileNameString"));
			list.Add(EPShared.LocalRM.GetString("MassCreationOnlyMode"));
			list.Add(EPShared.LocalRM.GetString("CurbMassSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("RetainingWallMassSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("SidewalkMassSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("StreetMassSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("CurbLineSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("CurbLineSubCategoryColor"));
			list.Add(EPShared.LocalRM.GetString("CurbLineSubCategoryLineWeight"));
			list.Add(EPShared.LocalRM.GetString("FeatureLineLineSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("FeatureLineLineSubCategoryColor"));
			list.Add(EPShared.LocalRM.GetString("FeatureLineLineSubCategoryLineWeight"));
			list.Add(EPShared.LocalRM.GetString("ParkingLotLineSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("ParkingLotLineSubCategoryColor"));
			list.Add(EPShared.LocalRM.GetString("ParkingLotLineSubCategoryLineWeight"));
			list.Add(EPShared.LocalRM.GetString("PointWipeoutLineSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("PointWipeoutLineSubCategoryColor"));
			list.Add(EPShared.LocalRM.GetString("PointWipeoutLineSubCategoryLineWeight"));
			list.Add(EPShared.LocalRM.GetString("RetainingWallLineSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("RetainingWallLineSubCategoryColor"));
			list.Add(EPShared.LocalRM.GetString("RetainingWallLineSubCategoryLineWeight"));
			list.Add(EPShared.LocalRM.GetString("SidewalkLineSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("SidewalkLineSubCategoryColor"));
			list.Add(EPShared.LocalRM.GetString("SidewalkLineSubCategoryLineWeight"));
			list.Add(EPShared.LocalRM.GetString("SoftTerrainLineSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("SoftTerrainLineSubCategoryColor"));
			list.Add(EPShared.LocalRM.GetString("SoftTerrainLineSubCategoryLineWeight"));
			list.Add(EPShared.LocalRM.GetString("StreetLineSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("StreetLineSubCategoryColor"));
			list.Add(EPShared.LocalRM.GetString("StreetLineSubCategoryLineWeight"));
			list.Add(EPShared.LocalRM.GetString("IntersectionLineSubCategoryName"));
			list.Add(EPShared.LocalRM.GetString("IntersectionLineSubCategoryColor"));
			list.Add(EPShared.LocalRM.GetString("IntersectionLineSubCategoryLineWeight"));
			list.Add(EPShared.LocalRM.GetString("TopographySurfaceArcPointInterval"));
			list.Add(EPShared.LocalRM.GetString("ProposedToposurfaceMaterialParameter"));
			list.Add(EPShared.LocalRM.GetString("ParkingLotIslandToposurfaceMaterialParameter"));
			list.Add(EPShared.LocalRM.GetString("SiteComponentPromptForStartingSide"));
			list.Add(EPShared.LocalRM.GetString("UseSharedElevation"));
			list.Add(EPShared.LocalRM.GetString("StreetIntersectionDefaultRadius"));
			list.Add(EPShared.LocalRM.GetString("SidewalkIntersectionDefaultRadius"));
			list.Add(EPShared.LocalRM.GetString("StreetIntersectionDefaultTransitioningDistance"));
			list.Add(EPShared.LocalRM.GetString("SidewalkIntersectionDefaultTransitioningDistance"));
		}
		list.Add(EPShared.LocalRM.GetString("CheckForUpdateNotification"));
		list.Add(EPShared.LocalRM.GetString("LibraryPath"));
		list.Add(EPShared.LocalRM.GetString("SamplePath"));
		for (int i = 0; i < list.Count; i++)
		{
			string key = list[i];
			empty = m_ParametersValueList[i];
			if (m_CurrentProduct == RevitProduct.SITES)
			{
				if (string.Compare(list[i], EPShared.LocalRM.GetString("TopographySurfaceArcPointInterval"), ignoreCase: true) == 0)
				{
					double dConvertedDouble = 12.0;
					if (empty != string.Empty)
					{
						dConvertedDouble = ValueConversion.ToDouble(empty);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(list[i], EPShared.LocalRM.GetString("StreetIntersectionDefaultRadius"), ignoreCase: true) == 0 || string.Compare(list[i], EPShared.LocalRM.GetString("SidewalkIntersectionDefaultRadius"), ignoreCase: true) == 0 || string.Compare(list[i], EPShared.LocalRM.GetString("StreetIntersectionDefaultTransitioningDistance"), ignoreCase: true) == 0 || string.Compare(list[i], EPShared.LocalRM.GetString("SidewalkIntersectionDefaultTransitioningDistance"), ignoreCase: true) == 0)
				{
					double dConvertedDouble2 = 0.0;
					if (empty != string.Empty)
					{
						dConvertedDouble2 = ValueConversion.ToDouble(empty);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(list[i], EPShared.LocalRM.GetString("MassCreationOnlyMode"), ignoreCase: true) == 0 || string.Compare(list[i], EPShared.LocalRM.GetString("SiteComponentPromptForStartingSide"), ignoreCase: true) == 0 || string.Compare(list[i], EPShared.LocalRM.GetString("UseSharedElevation"), ignoreCase: true) == 0)
				{
					empty = ((string.Compare(empty, "1", ignoreCase: true) != 0) ? EPShared.LocalRM.GetString("NoString") : EPShared.LocalRM.GetString("YesString"));
				}
				else if (string.Compare(list[i], EPShared.LocalRM.GetString("ProposedToposurfaceMaterialParameter"), ignoreCase: true) == 0 || string.Compare(list[i], EPShared.LocalRM.GetString("ParkingLotIslandToposurfaceMaterialParameter"), ignoreCase: true) == 0)
				{
					long nMaterialToFind = Convert.ToInt64(empty);
					IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == nMaterialToFind);
					List<Material> list2 = source.ToList();
					empty = ((list2.Count < 1) ? "Default" : list2.ElementAt(0).Name);
				}
			}
			else if (m_CurrentProduct == RevitProduct.LANDCADD)
			{
				if (string.Compare(list[i], EPShared.LocalRM.GetString("LANDCADDUpdaterCall"), ignoreCase: true) == 0)
				{
					empty = ((string.Compare(empty, "1", ignoreCase: true) != 0) ? EPShared.LocalRM.GetString("NoString") : EPShared.LocalRM.GetString("YesString"));
				}
				else if (string.Compare(list[i], EPShared.LocalRM.GetString("DisplaySharedParamaterWarningString"), ignoreCase: true) == 0)
				{
					empty = ((string.Compare(empty, "1", ignoreCase: true) != 0) ? EPShared.LocalRM.GetString("YesString") : EPShared.LocalRM.GetString("NoString"));
				}
			}
			if (string.Compare(list[i], EPShared.LocalRM.GetString("CheckForUpdateNotification"), ignoreCase: true) == 0)
			{
				empty = ((string.Compare(empty, "1", ignoreCase: true) != 0) ? EPShared.LocalRM.GetString("NoString") : EPShared.LocalRM.GetString("YesString"));
			}
			listViewItem = m_GlobalListView.Items.Add(key, key, string.Empty);
			listViewItem.SubItems.Add(empty);
			listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = empty;
		}
	}

	private void SetupParameterListTabList()
	{
		m_TabList.Clear();
		if (m_CurrentProduct == RevitProduct.LANDCADD)
		{
			m_TabList.Add(EPShared.LocalRM.GetString("TemplatesString"));
			m_TabList.Add(EPShared.LocalRM.GetString("GeneralString"));
		}
		else if (m_CurrentProduct == RevitProduct.SITES)
		{
			m_TabList.Add(EPShared.LocalRM.GetString("GeneralString"));
			if (ApplicationUtility.UseMass)
			{
				m_TabList.Add(EPShared.LocalRM.GetString("MassTemplatesString"));
				m_TabList.Add(EPShared.LocalRM.GetString("MassCreationString"));
			}
			m_TabList.Add(EPShared.LocalRM.GetString("SiteComponentLineCreationString"));
		}
	}

	private void SetupSettingsViewInfo(string _strCurrentTabText)
	{
		UpdateGlobalListView();
		if (m_SettingsTabControl.SelectedTab.Controls.Count > 0)
		{
			m_SettingsTabControl.SelectedTab.Controls.RemoveAt(0);
		}
		m_ParameterList.Clear();
		if (m_CurrentProduct == RevitProduct.LANDCADD)
		{
			if (string.Compare(_strCurrentTabText, EPShared.LocalRM.GetString("TemplatesString"), ignoreCase: true) == 0)
			{
				m_ParameterList.Add(EPShared.LocalRM.GetString("PlantingTemplateFileNameString"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("GenericTemplateFileNameString"));
			}
			else if (string.Compare(_strCurrentTabText, EPShared.LocalRM.GetString("GeneralString"), ignoreCase: true) == 0)
			{
				m_ParameterList.Add(EPShared.LocalRM.GetString("DisplaySharedParamaterWarningString"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("LANDCADDUpdaterCall"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("CheckForUpdateNotification"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("LibraryPath"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SamplePath"));
			}
		}
		else if (m_CurrentProduct == RevitProduct.SITES)
		{
			if (string.Compare(_strCurrentTabText, EPShared.LocalRM.GetString("MassTemplatesString"), ignoreCase: true) == 0)
			{
				ContextualHelpID = "HID_SDsettingsMassTemplates";
				m_ParameterList.Add(EPShared.LocalRM.GetString("MassTemplateFileNameString"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("CurbMassTemplateFileNameString"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("RetainingWallMassTemplateFileNameString"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SidewalkMassTemplateFileNameString"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("StreetMassTemplateFileNameString"));
			}
			else if (string.Compare(_strCurrentTabText, EPShared.LocalRM.GetString("MassCreationString"), ignoreCase: true) == 0)
			{
				ContextualHelpID = "HID_SDsettingsMassCreation";
				m_ParameterList.Add(EPShared.LocalRM.GetString("MassCreationOnlyMode"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("CurbMassSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("RetainingWallMassSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SidewalkMassSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("StreetMassSubCategoryName"));
			}
			else if (string.Compare(_strCurrentTabText, EPShared.LocalRM.GetString("SiteComponentLineCreationString"), ignoreCase: true) == 0)
			{
				ContextualHelpID = "HID_SDsettingsLineCreation";
				m_ParameterList.Add(EPShared.LocalRM.GetString("CurbLineSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("CurbLineSubCategoryColor"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("CurbLineSubCategoryLineWeight"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("FeatureLineLineSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("FeatureLineLineSubCategoryColor"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("FeatureLineLineSubCategoryLineWeight"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("ParkingLotLineSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("ParkingLotLineSubCategoryColor"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("ParkingLotLineSubCategoryLineWeight"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("PointWipeoutLineSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("PointWipeoutLineSubCategoryColor"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("PointWipeoutLineSubCategoryLineWeight"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("RetainingWallLineSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("RetainingWallLineSubCategoryColor"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("RetainingWallLineSubCategoryLineWeight"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SoftTerrainLineSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SoftTerrainLineSubCategoryColor"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SoftTerrainLineSubCategoryLineWeight"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SidewalkLineSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SidewalkLineSubCategoryColor"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SidewalkLineSubCategoryLineWeight"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("StreetLineSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("StreetLineSubCategoryColor"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("StreetLineSubCategoryLineWeight"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("IntersectionLineSubCategoryName"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("IntersectionLineSubCategoryColor"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("IntersectionLineSubCategoryLineWeight"));
			}
			else if (string.Compare(_strCurrentTabText, EPShared.LocalRM.GetString("GeneralString"), ignoreCase: true) == 0)
			{
				ContextualHelpID = "HID_SDsettingsGeneral";
				m_ParameterList.Add(EPShared.LocalRM.GetString("TopographySurfaceArcPointInterval"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("ProposedToposurfaceMaterialParameter"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("ParkingLotIslandToposurfaceMaterialParameter"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SiteComponentPromptForStartingSide"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("UseSharedElevation"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("StreetIntersectionDefaultRadius"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SidewalkIntersectionDefaultRadius"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("StreetIntersectionDefaultTransitioningDistance"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SidewalkIntersectionDefaultTransitioningDistance"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("CheckForUpdateNotification"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("LibraryPath"));
				m_ParameterList.Add(EPShared.LocalRM.GetString("SamplePath"));
			}
		}
		m_ParameterListView.Items.Clear();
		m_ParameterListView.RefreshListView();
		m_ParameterListView.OnListViewItemChanged += ParameterListViewOnListViewItemChanged;
		string empty = string.Empty;
		ListViewItem listViewItem = new ListViewItem();
		for (int i = 0; i < m_ParameterList.Count; i++)
		{
			string text = m_ParameterList[i];
			empty = GetListViewValue(text);
			if (m_CurrentProduct == RevitProduct.SITES)
			{
				if (string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("TopographySurfaceArcPointInterval"), ignoreCase: true) == 0)
				{
					double _dReturnedDouble = 1.0;
					if (empty != string.Empty)
					{
						RevitGeometry.Units_ConvertStringToDouble(empty, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
					}
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("StreetIntersectionDefaultRadius"), ignoreCase: true) == 0 || string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("SidewalkIntersectionDefaultRadius"), ignoreCase: true) == 0 || string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("StreetIntersectionDefaultTransitioningDistance"), ignoreCase: true) == 0 || string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("SidewalkIntersectionDefaultTransitioningDistance"), ignoreCase: true) == 0)
				{
					double _dReturnedDouble2 = 0.0;
					if (empty != string.Empty)
					{
						RevitGeometry.Units_ConvertStringToDouble(empty, m_Units, UnitType.UT_Length, ref _dReturnedDouble2);
					}
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("MassCreationOnlyMode"), ignoreCase: true) == 0 || string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("SiteComponentPromptForStartingSide"), ignoreCase: true) == 0 || string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("UseSharedElevation"), ignoreCase: true) == 0)
				{
					if (string.Compare(empty, "1", ignoreCase: true) == 0)
					{
						empty = EPShared.LocalRM.GetString("YesString");
					}
					else if (string.Compare(empty, "0", ignoreCase: true) == 0)
					{
						empty = EPShared.LocalRM.GetString("NoString");
					}
					else if (empty == string.Empty)
					{
						empty = EPShared.LocalRM.GetString("NoString");
					}
				}
				else if (string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("ProposedToposurfaceMaterialParameter"), ignoreCase: true) == 0 || string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("ParkingLotIslandToposurfaceMaterialParameter"), ignoreCase: true) == 0)
				{
					if (empty == string.Empty)
					{
						empty = "Default";
					}
					string strStringToFind = empty;
					IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strStringToFind, ignoreCase: true) == 0);
					List<Material> list = source.ToList();
					empty = ((list.Count < 1) ? string.Empty : list.ElementAt(0).Name);
				}
			}
			else if (m_CurrentProduct == RevitProduct.LANDCADD)
			{
				if (string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("DisplaySharedParamaterWarningString"), ignoreCase: true) == 0)
				{
					if (string.Compare(empty, "1", ignoreCase: true) == 0)
					{
						empty = EPShared.LocalRM.GetString("YesString");
					}
					else if (string.Compare(empty, "0", ignoreCase: true) == 0)
					{
						empty = EPShared.LocalRM.GetString("NoString");
					}
					else if (empty == string.Empty)
					{
						empty = EPShared.LocalRM.GetString("YesString");
					}
				}
				if (string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("LANDCADDUpdaterCall"), ignoreCase: true) == 0)
				{
					if (string.Compare(empty, "1", ignoreCase: true) == 0)
					{
						empty = EPShared.LocalRM.GetString("YesString");
					}
					else if (string.Compare(empty, "0", ignoreCase: true) == 0)
					{
						empty = EPShared.LocalRM.GetString("NoString");
					}
					else if (empty == string.Empty)
					{
						empty = EPShared.LocalRM.GetString("YesString");
					}
				}
			}
			if (string.Compare(m_ParameterList[i], EPShared.LocalRM.GetString("CheckForUpdateNotification"), ignoreCase: true) == 0)
			{
				if (string.Compare(empty, "1", ignoreCase: true) == 0 || empty == string.Empty)
				{
					empty = EPShared.LocalRM.GetString("YesString");
				}
				else if (string.Compare(empty, "0", ignoreCase: true) == 0)
				{
					empty = EPShared.LocalRM.GetString("NoString");
				}
			}
			listViewItem = m_ParameterListView.Items.Add(text, text, string.Empty);
			listViewItem.SubItems.Add(empty);
			listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = empty;
		}
		m_SettingsTabControl.SelectedTab.Controls.AddRange(new System.Windows.Forms.Control[1] { m_ParameterListView });
	}

	private string GetListViewValue(string _strParameterName)
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		for (int i = 0; i < m_GlobalListView.Items.Count; i++)
		{
			empty = m_GlobalListView.Items[i].SubItems[0].Text;
			empty2 = m_GlobalListView.Items[i].SubItems[1].Text;
			if (string.Compare(empty, _strParameterName, ignoreCase: true) == 0)
			{
				return empty2;
			}
		}
		return string.Empty;
	}

	private void UpdateGlobalListView()
	{
		if (m_ParameterListView == null)
		{
			return;
		}
		string empty = string.Empty;
		string empty2 = string.Empty;
		for (int i = 0; i < m_ParameterListView.Items.Count; i++)
		{
			empty = m_ParameterListView.Items[i].SubItems[0].Text;
			empty2 = m_ParameterListView.Items[i].SubItems[1].Text;
			for (int j = 0; j < m_GlobalListView.Items.Count; j++)
			{
				if (string.Compare(m_GlobalListView.Items[j].SubItems[0].Text, empty, ignoreCase: false) == 0)
				{
					m_GlobalListView.Items[j].SubItems[1].Text = empty2;
					break;
				}
			}
		}
	}

	private void UpdateCurrentTabListView()
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		for (int i = 0; i < m_GlobalListView.Items.Count; i++)
		{
			empty = m_GlobalListView.Items[i].SubItems[0].Text;
			empty2 = m_GlobalListView.Items[i].SubItems[1].Text;
			for (int j = 0; j < m_ParameterListView.Items.Count; j++)
			{
				if (string.Compare(m_ParameterListView.Items[j].SubItems[0].Text, empty, ignoreCase: false) == 0)
				{
					m_ParameterListView.Items[j].SubItems[1].Text = empty2;
					break;
				}
			}
		}
	}

	private void StreetTabControlTabIndexChanged(object sender, EventArgs e)
	{
		SetupSettingsViewInfo(m_SettingsTabControl.SelectedTab.Text);
		UpdateCurrentTabListView();
	}

	private void OnSettingsFormClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private void OnButtonCancelClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
	}

	private void OnButtonOkClicked(object sender, EventArgs e)
	{
		UpdateGlobalListView();
		string empty = string.Empty;
		string empty2 = string.Empty;
		for (int i = 0; i < m_GlobalListView.Items.Count; i++)
		{
			empty = m_GlobalListView.Items[i].SubItems[0].Text;
			empty2 = m_GlobalListView.Items[i].SubItems[1].Text;
			if (m_CurrentProduct == RevitProduct.SITES)
			{
				if (string.Compare(EPShared.LocalRM.GetString("CurbMassSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("CurbMassSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("MassTemplateFileNameString"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("MassTemplateFileName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("CurbMassTemplateFileNameString"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("CurbMassTemplateFileName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("RetainingWallMassTemplateFileNameString"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("RetainingWallMassTemplateFileName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("SidewalkMassTemplateFileNameString"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("SidewalkMassTemplateFileName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("StreetMassTemplateFileNameString"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("StreetMassTemplateFileName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("RetainingWallMassSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("RetainingWallMassSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("SidewalkMassSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("SidewalkMassSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("StreetMassSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("StreetMassSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("CurbLineSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("CurbLineSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("CurbLineSubCategoryColor"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("CurbLineSubCategoryColor");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("CurbLineSubCategoryLineWeight"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("CurbLineSubCategoryLineWeight");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("FeatureLineLineSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("FeatureLineLineSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("FeatureLineLineSubCategoryColor"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("FeatureLineLineSubCategoryColor");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("FeatureLineLineSubCategoryLineWeight"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("FeatureLineLineSubCategoryLineWeight");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("ParkingLotLineSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("ParkingLotLineSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("ParkingLotLineSubCategoryColor"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("ParkingLotLineSubCategoryColor");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("ParkingLotLineSubCategoryLineWeight"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("ParkingLotLineSubCategoryLineWeight");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("PointWipeoutLineSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("PointWipeoutLineSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("PointWipeoutLineSubCategoryColor"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("PointWipeoutLineSubCategoryColor");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("PointWipeoutLineSubCategoryLineWeight"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("PointWipeoutLineSubCategoryLineWeight");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("RetainingWallLineSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("RetainingWallLineSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("RetainingWallLineSubCategoryColor"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("RetainingWallLineSubCategoryColor");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("RetainingWallLineSubCategoryLineWeight"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("RetainingWallLineSubCategoryLineWeight");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("SidewalkLineSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("SidewalkLineSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("SidewalkLineSubCategoryColor"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("SidewalkLineSubCategoryColor");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("SidewalkLineSubCategoryLineWeight"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("SidewalkLineSubCategoryLineWeight");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("SoftTerrainLineSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("SoftTerrainLineSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("SoftTerrainLineSubCategoryColor"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("SoftTerrainLineSubCategoryColor");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("SoftTerrainLineSubCategoryLineWeight"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("SoftTerrainLineSubCategoryLineWeight");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("StreetLineSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("StreetLineSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("StreetLineSubCategoryColor"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("StreetLineSubCategoryColor");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("StreetLineSubCategoryLineWeight"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("StreetLineSubCategoryLineWeight");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("IntersectionLineSubCategoryName"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("IntersectionLineSubCategoryName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("IntersectionLineSubCategoryColor"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("IntersectionLineSubCategoryColor");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("IntersectionLineSubCategoryLineWeight"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("IntersectionLineSubCategoryLineWeight");
					m_Values.Add(empty2);
				}
				if (m_CurrentProduct == RevitProduct.SITES)
				{
					if (string.Compare(empty, EPShared.LocalRM.GetString("TopographySurfaceArcPointInterval"), ignoreCase: true) == 0)
					{
						double _dReturnedDouble = 0.0;
						if (empty2 != null && empty2 != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), empty2, ignoreCase: true) == 0)
						{
							_dReturnedDouble = 0.0;
						}
						else if (empty2 != null && empty2 != string.Empty)
						{
							RevitGeometry.Units_ConvertStringToDouble(empty2, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
						}
						empty2 = ValueConversion.ToString(_dReturnedDouble);
						m_Fields.Add("TopographySurfaceArcPointInterval");
						m_Values.Add(empty2);
					}
					else if (string.Compare(empty, EPShared.LocalRM.GetString("StreetIntersectionDefaultRadius"), ignoreCase: true) == 0 || string.Compare(empty, EPShared.LocalRM.GetString("SidewalkIntersectionDefaultRadius"), ignoreCase: true) == 0)
					{
						double _dReturnedDouble2 = 0.0;
						if (empty2 != null && empty2 != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), empty2, ignoreCase: true) == 0)
						{
							_dReturnedDouble2 = 0.0;
						}
						else if (empty2 != null && empty2 != string.Empty)
						{
							RevitGeometry.Units_ConvertStringToDouble(empty2, m_Units, UnitType.UT_Length, ref _dReturnedDouble2);
						}
						empty2 = ValueConversion.ToString(_dReturnedDouble2);
						if (string.Compare(empty, EPShared.LocalRM.GetString("StreetIntersectionDefaultRadius"), ignoreCase: true) == 0)
						{
							m_Fields.Add("StreetIntersectionDefaultRadius");
						}
						else if (string.Compare(empty, EPShared.LocalRM.GetString("SidewalkIntersectionDefaultRadius"), ignoreCase: true) == 0)
						{
							m_Fields.Add("SidewalkIntersectionDefaultRadius");
						}
						m_Values.Add(empty2);
					}
					else if (string.Compare(empty, EPShared.LocalRM.GetString("StreetIntersectionDefaultTransitioningDistance"), ignoreCase: true) == 0 || string.Compare(empty, EPShared.LocalRM.GetString("SidewalkIntersectionDefaultTransitioningDistance"), ignoreCase: true) == 0)
					{
						double _dReturnedDouble3 = 0.0;
						if (empty2 != null && empty2 != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), empty2, ignoreCase: true) == 0)
						{
							_dReturnedDouble3 = 0.0;
						}
						else if (empty2 != null && empty2 != string.Empty)
						{
							RevitGeometry.Units_ConvertStringToDouble(empty2, m_Units, UnitType.UT_Length, ref _dReturnedDouble3);
						}
						empty2 = ValueConversion.ToString(_dReturnedDouble3);
						if (string.Compare(empty, EPShared.LocalRM.GetString("StreetIntersectionDefaultTransitioningDistance"), ignoreCase: true) == 0)
						{
							m_Fields.Add("StreetIntersectionDefaultTransitioningDistance");
						}
						else if (string.Compare(empty, EPShared.LocalRM.GetString("SidewalkIntersectionDefaultTransitioningDistance"), ignoreCase: true) == 0)
						{
							m_Fields.Add("SidewalkIntersectionDefaultTransitioningDistance");
						}
						m_Values.Add(empty2);
					}
					else if (string.Compare(empty, EPShared.LocalRM.GetString("MassCreationOnlyMode"), ignoreCase: true) == 0 || string.Compare(empty, EPShared.LocalRM.GetString("SiteComponentPromptForStartingSide"), ignoreCase: true) == 0 || string.Compare(empty, EPShared.LocalRM.GetString("UseSharedElevation"), ignoreCase: true) == 0)
					{
						if (string.Compare(empty2, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) != 0)
						{
							empty2 = ((string.Compare(empty2, EPShared.LocalRM.GetString("NoString"), ignoreCase: true) != 0) ? "0" : "0");
						}
						else
						{
							empty2 = "1";
						}
						if (string.Compare(empty, EPShared.LocalRM.GetString("MassCreationOnlyMode"), ignoreCase: true) == 0)
						{
							m_Fields.Add("MassCreationOnlyMode");
						}
						else if (string.Compare(empty, EPShared.LocalRM.GetString("SiteComponentPromptForStartingSide"), ignoreCase: true) == 0)
						{
							m_Fields.Add("SiteComponentPromptForStartingSide");
						}
						else if (string.Compare(empty, EPShared.LocalRM.GetString("UseSharedElevation"), ignoreCase: true) == 0)
						{
							m_Fields.Add("UseSharedElevation");
						}
						m_Values.Add(empty2);
					}
					else if (string.Compare(empty, EPShared.LocalRM.GetString("ProposedToposurfaceMaterialParameter"), ignoreCase: true) == 0 || string.Compare(empty, EPShared.LocalRM.GetString("ParkingLotIslandToposurfaceMaterialParameter"), ignoreCase: true) == 0)
					{
						string strStringToFind = empty2;
						if (strStringToFind == string.Empty)
						{
							empty2 = "00000";
						}
						else
						{
							IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strStringToFind, ignoreCase: false) == 0);
							List<Material> list = source.ToList();
							empty2 = ((list.Count < 1) ? "00000" : list.ElementAt(0).Id.Value64().ToString());
						}
						if (string.Compare(empty, EPShared.LocalRM.GetString("ProposedToposurfaceMaterialParameter"), ignoreCase: true) == 0)
						{
							m_Fields.Add("ProposedToposurfaceMaterialParameter");
						}
						else if (string.Compare(empty, EPShared.LocalRM.GetString("ParkingLotIslandToposurfaceMaterialParameter"), ignoreCase: true) == 0)
						{
							m_Fields.Add("ParkingLotIslandToposurfaceMaterialParameter");
						}
						m_Values.Add(empty2);
					}
				}
			}
			else if (m_CurrentProduct == RevitProduct.LANDCADD)
			{
				if (string.Compare(EPShared.LocalRM.GetString("PlantingTemplateFileNameString"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("PlantingTemplateFileName");
					m_Values.Add(empty2);
				}
				else if (string.Compare(EPShared.LocalRM.GetString("GenericTemplateFileNameString"), empty, ignoreCase: true) == 0)
				{
					m_Fields.Add("GenericTemplateFileName");
					m_Values.Add(empty2);
				}
				if (string.Compare(EPShared.LocalRM.GetString("DisplaySharedParamaterWarningString"), empty, ignoreCase: true) == 0)
				{
					if (string.Compare(empty2, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) != 0)
					{
						empty2 = ((string.Compare(empty2, EPShared.LocalRM.GetString("NoString"), ignoreCase: true) != 0) ? "0" : "1");
					}
					else
					{
						empty2 = "0";
					}
					m_Fields.Add("MissingSharedParametersDoNotShowAgainOption");
					m_Values.Add(empty2);
				}
				if (string.Compare(EPShared.LocalRM.GetString("LANDCADDUpdaterCall"), empty, ignoreCase: true) == 0)
				{
					if (string.Compare(empty2, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) != 0)
					{
						empty2 = ((string.Compare(empty2, EPShared.LocalRM.GetString("NoString"), ignoreCase: true) != 0) ? "1" : "0");
					}
					else
					{
						empty2 = "1";
					}
					m_Fields.Add("LANDCADDUpdaterCall");
					m_Values.Add(empty2);
				}
			}
			if (string.Compare(EPShared.LocalRM.GetString("CheckForUpdateNotification"), empty, ignoreCase: true) == 0)
			{
				if (string.Compare(empty2, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) != 0)
				{
					empty2 = ((string.Compare(empty2, EPShared.LocalRM.GetString("NoString"), ignoreCase: true) != 0) ? "1" : "0");
				}
				else
				{
					empty2 = "1";
				}
				m_Fields.Add("CheckForUpdateNotificationDoNotShowAgainOption");
				m_Values.Add(empty2);
			}
			else if (string.Compare(EPShared.LocalRM.GetString("LibraryPath"), empty, ignoreCase: true) == 0)
			{
				m_Fields.Add("LibraryPath");
				m_Values.Add(empty2);
			}
			else if (string.Compare(EPShared.LocalRM.GetString("SamplePath"), empty, ignoreCase: true) == 0)
			{
				m_Fields.Add("SamplePath");
				m_Values.Add(empty2);
			}
		}
		for (int num = 0; num < m_Fields.Count; num++)
		{
			if (m_CurrentProduct == RevitProduct.SITES)
			{
				if (string.Compare(m_Fields[num], "TopographySurfaceArcPointInterval", ignoreCase: true) == 0)
				{
					if (m_Values[num] != string.Empty)
					{
						SetDoubleSetting(m_Fields[num], Convert.ToDouble(m_Values[num]));
						continue;
					}
				}
				else if (string.Compare(m_Fields[num], "StreetIntersectionDefaultRadius", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "SidewalkIntersectionDefaultRadius", ignoreCase: true) == 0)
				{
					if (m_Values[num] != string.Empty)
					{
						SetDoubleSetting(m_Fields[num], Convert.ToDouble(m_Values[num]));
						continue;
					}
				}
				else if (string.Compare(m_Fields[num], "StreetIntersectionDefaultTransitioningDistance", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "SidewalkIntersectionDefaultTransitioningDistance", ignoreCase: true) == 0)
				{
					if (m_Values[num] != string.Empty)
					{
						SetDoubleSetting(m_Fields[num], Convert.ToDouble(m_Values[num]));
						continue;
					}
				}
				else if ((string.Compare(m_Fields[num], "MassCreationOnlyMode", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "SoftTerrainLineSubCategoryLineWeight", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "SidewalkLineSubCategoryLineWeight", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "RetainingWallLineSubCategoryLineWeight", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "StreetLineSubCategoryLineWeight", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "IntersectionLineSubCategoryLineWeight", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "CurbLineSubCategoryLineWeight", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "ParkingLotLineSubCategoryLineWeight", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "FeatureLineLineSubCategoryLineWeight", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "PointWipeoutLineSubCategoryLineWeight", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "ProposedToposurfaceMaterialParameter", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "ParkingLotIslandToposurfaceMaterialParameter", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "SiteComponentPromptForStartingSide", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "UseSharedElevation", ignoreCase: true) == 0) && m_Values[num] != string.Empty)
				{
					if (string.Compare(m_Fields[num], "ProposedToposurfaceMaterialParameter", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "ParkingLotIslandToposurfaceMaterialParameter", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "UseSharedElevation", ignoreCase: true) == 0)
					{
						SetProjectIntSetting(m_Fields[num], Convert.ToInt32(m_Values[num]));
					}
					else
					{
						SetIntSetting(m_Fields[num], Convert.ToInt32(m_Values[num]));
					}
					continue;
				}
			}
			else if (m_CurrentProduct == RevitProduct.LANDCADD && (string.Compare(m_Fields[num], "LANDCADDUpdaterCall", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "MissingSharedParametersDoNotShowAgainOption", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "CheckSharedParametersFile2010", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "CheckSharedParametersFile2011", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "CheckSharedParametersFile2012", ignoreCase: true) == 0 || string.Compare(m_Fields[num], "CheckSharedParametersFile2013", ignoreCase: true) == 0) && m_Values[num] != string.Empty)
			{
				SetIntSetting(m_Fields[num], Convert.ToInt32(m_Values[num]));
				continue;
			}
			if (string.Compare(m_Fields[num], "CheckForUpdateNotificationDoNotShowAgainOption", ignoreCase: true) == 0)
			{
				if (m_Values[num] != string.Empty)
				{
					if (Convert.ToInt32(m_Values[num]) == 1)
					{
						SetIntSetting(m_Fields[num], 0);
					}
					else
					{
						SetIntSetting(m_Fields[num], 1);
					}
				}
			}
			else if (m_Values[num] != string.Empty)
			{
				SetStringSetting(m_Fields[num], m_Values[num]);
			}
		}
		DialogResult = DialogResult.OK;
	}

	private void SetStringSetting(string _strSettingName, string _strSettingValue)
	{
		ApplicationUtility.Settings.eplSetE_SetSetting(_strSettingName, _strSettingValue);
	}

	private void SetIntSetting(string _strSettingName, int _nSettingValue)
	{
		ApplicationUtility.Settings.eplSetE_SetSetting(_strSettingName, _nSettingValue);
	}

	private void SetProjectIntSetting(string _strSettingName, int _nSettingValue)
	{
		ApplicationUtility.Settings.eplSetE_SetSetting(m_CurrentDocument.PathName, _strSettingName, _nSettingValue);
	}

	private void SetDoubleSetting(string _strSettingName, double _dValue)
	{
		ApplicationUtility.Settings.eplSetE_SetSetting(_strSettingName, _dValue);
	}

	private void FormResizeBegin(object sender, EventArgs e)
	{
		AnchorListView();
	}

	private void AnchorListView()
	{
		if (m_ParameterListView != null)
		{
			m_ParameterListView.SetAnchor();
		}
	}

	private void ParameterListViewOnListViewItemChanged(int _nChangedIndex)
	{
		string strB = m_ParameterListView.Items[_nChangedIndex].SubItems[0].Text;
		string _strToDisplay = m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text;
		if (m_CurrentProduct == RevitProduct.SITES)
		{
			if (string.Compare(EPShared.LocalRM.GetString("TopographySurfaceArcPointInterval"), strB, ignoreCase: true) == 0)
			{
				try
				{
					double _dReturnedDouble = 0.0;
					RevitGeometry.Units_ConvertStringToDouble(_strToDisplay, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
					if (_dReturnedDouble < 1.0)
					{
						string _strToDisplay2 = string.Empty;
						RevitGeometry.Units_ConvertDoubleToString(1.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay2);
						RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetLaneWidthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay2, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay2;
						m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
						m_ParameterListView.SetFocus(_nChangedIndex);
						return;
					}
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
				}
				catch (Exception)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
			}
			else if (string.Compare(EPShared.LocalRM.GetString("StreetIntersectionDefaultRadius"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("SidewalkIntersectionDefaultRadius"), strB, ignoreCase: true) == 0)
			{
				try
				{
					double _dReturnedDouble2 = 0.0;
					RevitGeometry.Units_ConvertStringToDouble(_strToDisplay, m_Units, UnitType.UT_Length, ref _dReturnedDouble2);
					if (_dReturnedDouble2 < 0.0)
					{
						string _strToDisplay3 = string.Empty;
						RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay3);
						string text = string.Empty;
						if (string.Compare(EPShared.LocalRM.GetString("StreetIntersectionDefaultRadius"), strB, ignoreCase: true) == 0)
						{
							text = EPShared.LocalRM.GetString("StreetIntersectionDefaultRadius");
						}
						else if (string.Compare(EPShared.LocalRM.GetString("SidewalkIntersectionDefaultRadius"), strB, ignoreCase: true) == 0)
						{
							text = EPShared.LocalRM.GetString("StreetIntersectionDefaultRadius");
						}
						RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + text + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay3, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay3;
						m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
						m_ParameterListView.SetFocus(_nChangedIndex);
						return;
					}
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
				}
				catch (Exception)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
			}
			else if (string.Compare(EPShared.LocalRM.GetString("StreetIntersectionDefaultTransitioningDistance"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("SidewalkIntersectionDefaultTransitioningDistance"), strB, ignoreCase: true) == 0)
			{
				try
				{
					double _dReturnedDouble3 = 0.0;
					RevitGeometry.Units_ConvertStringToDouble(_strToDisplay, m_Units, UnitType.UT_Length, ref _dReturnedDouble3);
					if (_dReturnedDouble3 < 0.0)
					{
						string _strToDisplay4 = string.Empty;
						RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay4);
						string text2 = string.Empty;
						if (string.Compare(EPShared.LocalRM.GetString("StreetIntersectionDefaultTransitioningDistance"), strB, ignoreCase: true) == 0)
						{
							text2 = EPShared.LocalRM.GetString("StreetIntersectionDefaultTransitioningDistance");
						}
						else if (string.Compare(EPShared.LocalRM.GetString("SidewalkIntersectionDefaultTransitioningDistance"), strB, ignoreCase: true) == 0)
						{
							text2 = EPShared.LocalRM.GetString("SidewalkIntersectionDefaultTransitioningDistance");
						}
						RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + text2 + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay4, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay4;
						m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
						m_ParameterListView.SetFocus(_nChangedIndex);
						return;
					}
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
				}
				catch (Exception)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
			}
			else if (string.Compare(EPShared.LocalRM.GetString("MassTemplateFileNameString"), strB, ignoreCase: true) == 0)
			{
				if (!File.Exists(_strToDisplay))
				{
					MessageBox.Show(EPShared.LocalRM.GetString("InvalidInputString"));
					m_ParameterListView.SetFilePickerFocus(_nChangedIndex);
				}
			}
			else if (string.Compare(EPShared.LocalRM.GetString("CurbMassTemplateFileNameString"), strB, ignoreCase: true) == 0)
			{
				if (!File.Exists(_strToDisplay))
				{
					MessageBox.Show(EPShared.LocalRM.GetString("InvalidInputString"));
					m_ParameterListView.SetFilePickerFocus(_nChangedIndex);
				}
			}
			else if (string.Compare(EPShared.LocalRM.GetString("RetainingWallMassTemplateFileNameString"), strB, ignoreCase: true) == 0)
			{
				if (!File.Exists(_strToDisplay))
				{
					MessageBox.Show(EPShared.LocalRM.GetString("InvalidInputString"));
					m_ParameterListView.SetFilePickerFocus(_nChangedIndex);
				}
			}
			else if (string.Compare(EPShared.LocalRM.GetString("SidewalkMassTemplateFileNameString"), strB, ignoreCase: true) == 0)
			{
				if (!File.Exists(_strToDisplay))
				{
					MessageBox.Show(EPShared.LocalRM.GetString("InvalidInputString"));
					m_ParameterListView.SetFilePickerFocus(_nChangedIndex);
				}
			}
			else if (string.Compare(EPShared.LocalRM.GetString("StreetMassTemplateFileNameString"), strB, ignoreCase: true) == 0)
			{
				if (!File.Exists(_strToDisplay))
				{
					MessageBox.Show(EPShared.LocalRM.GetString("InvalidInputString"));
					m_ParameterListView.SetFilePickerFocus(_nChangedIndex);
				}
			}
			else if (string.Compare(EPShared.LocalRM.GetString("SoftTerrainLineSubCategoryLineWeight"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("SidewalkLineSubCategoryLineWeight"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("RetainingWallLineSubCategoryLineWeight"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("StreetLineSubCategoryLineWeight"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("IntersectionLineSubCategoryLineWeight"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("CurbLineSubCategoryLineWeight"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("ParkingLotLineSubCategoryLineWeight"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("FeatureLineLineSubCategoryLineWeight"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("PointWipeoutLineSubCategoryLineWeight"), strB, ignoreCase: true) == 0)
			{
				int num = 0;
				bool flag = true;
				for (int i = 0; i < _strToDisplay.Length; i++)
				{
					if (!char.IsDigit(_strToDisplay[i]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					num = Convert.ToInt32(_strToDisplay);
				}
				if (num <= 0 || num > 16)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("LineSubCategoryLineWeight") + ". " + EPShared.LocalRM.GetString("ValidLineWeightValuesMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = "1";
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
			}
		}
		else if (m_CurrentProduct == RevitProduct.LANDCADD)
		{
			if (string.Compare(EPShared.LocalRM.GetString("PlantingTemplateFileNameString"), strB, ignoreCase: true) == 0)
			{
				if (!File.Exists(_strToDisplay))
				{
					MessageBox.Show(EPShared.LocalRM.GetString("InvalidInputString"));
					m_ParameterListView.SetFilePickerFocus(_nChangedIndex);
				}
			}
			else if (string.Compare(EPShared.LocalRM.GetString("GenericTemplateFileNameString"), strB, ignoreCase: true) == 0 && !File.Exists(_strToDisplay))
			{
				MessageBox.Show(EPShared.LocalRM.GetString("InvalidInputString"));
				m_ParameterListView.SetFilePickerFocus(_nChangedIndex);
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("LibraryPath"), strB, ignoreCase: true) == 0)
		{
			if (!Directory.Exists(_strToDisplay))
			{
				MessageBox.Show(EPShared.LocalRM.GetString("InvalidInputString"));
				m_ParameterListView.SetDirectoryPickerFocus(_nChangedIndex);
			}
		}
		else if (string.Compare(EPShared.LocalRM.GetString("SamplePath"), strB, ignoreCase: true) == 0 && !Directory.Exists(_strToDisplay))
		{
			MessageBox.Show(EPShared.LocalRM.GetString("InvalidInputString"));
			m_ParameterListView.SetDirectoryPickerFocus(_nChangedIndex);
		}
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
		this.m_ButtonOk = new System.Windows.Forms.Button();
		this.m_ButtonCancel = new System.Windows.Forms.Button();
		this.m_SettingsTabControl = new System.Windows.Forms.TabControl();
		this.m_Panel = new System.Windows.Forms.Panel();
		this.m_Panel.SuspendLayout();
		base.SuspendLayout();
		this.m_ButtonOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_ButtonOk.Location = new System.Drawing.Point(304, 549);
		this.m_ButtonOk.Name = "m_ButtonOk";
		this.m_ButtonOk.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonOk.TabIndex = 0;
		this.m_ButtonOk.Text = "Ok";
		this.m_ButtonOk.UseVisualStyleBackColor = true;
		this.m_ButtonOk.Click += new System.EventHandler(this.OnButtonOkClicked);
		this.m_ButtonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_ButtonCancel.Location = new System.Drawing.Point(385, 549);
		this.m_ButtonCancel.Name = "m_ButtonCancel";
		this.m_ButtonCancel.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonCancel.TabIndex = 1;
		this.m_ButtonCancel.Text = "Cancel";
		this.m_ButtonCancel.UseVisualStyleBackColor = true;
		this.m_ButtonCancel.Click += new System.EventHandler(this.OnButtonCancelClicked);
		this.m_SettingsTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_SettingsTabControl.Location = new System.Drawing.Point(12, 12);
		this.m_SettingsTabControl.Name = "m_SettingsTabControl";
		this.m_SettingsTabControl.SelectedIndex = 0;
		this.m_SettingsTabControl.Size = new System.Drawing.Size(455, 531);
		this.m_SettingsTabControl.TabIndex = 2;
		this.m_SettingsTabControl.SelectedIndexChanged += new System.EventHandler(this.StreetTabControlTabIndexChanged);
		this.m_Panel.AutoSize = true;
		this.m_Panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.m_Panel.Controls.Add(this.m_SettingsTabControl);
		this.m_Panel.Controls.Add(this.m_ButtonCancel);
		this.m_Panel.Controls.Add(this.m_ButtonOk);
		this.m_Panel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_Panel.Location = new System.Drawing.Point(0, 0);
		this.m_Panel.Name = "m_Panel";
		this.m_Panel.Size = new System.Drawing.Size(467, 591);
		this.m_Panel.TabIndex = 3;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(467, 591);
		base.Controls.Add(this.m_Panel);
		this.MinimumSize = new System.Drawing.Size(475, 629);
		base.Name = "SettingsForm";
		this.Text = "SettingsForm";
		this.m_Panel.ResumeLayout(false);
		this.m_Panel.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_ButtonOk;
		base.CancelButton = this.m_ButtonCancel;
	}
}
