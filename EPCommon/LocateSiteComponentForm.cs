using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using SITES.Utilities;

namespace EPCommon;

public class LocateSiteComponentForm : EPForm
{
	private Units m_Units;

	private const UnitType m_LengthUnitType = UnitType.UT_Length;

	private const UnitType m_SlopeUnitType = UnitType.UT_Slope;

	private const UnitType m_AngularUnitType = UnitType.UT_Angle;

	private SiteComponentCreation m_dataBuffer;

	private FamilySymbol m_CurType;

	private FormatOptions m_FormatOptions;

	private DisplayUnit m_DisplayUnitSystem;

	private UnitSymbolType m_UnitSymbolType;

	private LandcaddID m_CurrentLandcaddID = LandcaddID.SoftTerrain;

	private string m_strCurrentDlgType = string.Empty;

	private DecimalSymbol m_DecimalSymbolType;

	private CurrentElevationFocus m_CurrentElevationFocus;

	private double m_dAdjustConstElevation;

	private IContainer components;

	private Button m_LoadFamilyButton;

	private ComboBox m_TypeNameComboBox;

	private Label m_TypeNameLabel;

	private ComboBox m_FamilyComboBox;

	private Label m_FamilyLabel;

	private Label m_ConstantElevationLabel;

	private TextBox m_ConstantElevationTextBox;

	private System.Windows.Forms.Panel m_MoveablePanel;

	private CheckBox m_CreateCopyCheckBox;

	private Button m_CloseButton;

	private Button m_InsertButton;

	private Label m_RadiusLabel;

	private TextBox m_RadiusTextBox;

	private Label m_NameLabel;

	private TextBox m_NameTextBox;

	private RadioButton m_RelativeElevationRadioButton;

	private Label m_RelativeElevationLabel;

	private TextBox m_RelativeElevationTextBox;

	private RadioButton m_ConstantElevationRadioButton;

	private System.Windows.Forms.Panel m_UsePanel;

	private RadioButton m_SegmentRadioButton;

	private RadioButton m_ChainRadioButton;

	private System.Windows.Forms.Panel m_HostLineSelectionPanel;

	private RadioButton m_UseExistingHostLineRadioButton;

	private RadioButton m_DrawHostLineRadioButton;

	private Button m_PickInRevitButton;

	public bool ExistingHostLineChecked => m_UseExistingHostLineRadioButton.Checked;

	public bool CreateCopyChecked => m_CreateCopyCheckBox.Checked;

	public double Radius
	{
		get
		{
			if (m_UseExistingHostLineRadioButton.Checked)
			{
				return 0.0;
			}
			double _dReturnedDouble = 0.0;
			if (m_RadiusTextBox.Text != string.Empty)
			{
				RevitGeometry.Units_ConvertStringToDouble(m_RadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: true);
			}
			return _dReturnedDouble;
		}
	}

	public FamilySymbol Type => m_CurType;

	public double Elevation
	{
		get
		{
			double _dReturnedDouble = 0.0;
			if (m_RelativeElevationRadioButton.Checked)
			{
				if (m_RelativeElevationTextBox.Text != string.Empty)
				{
					RevitGeometry.Units_ConvertStringToDouble(m_RelativeElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: true);
				}
			}
			else if (m_ConstantElevationTextBox.Text != string.Empty)
			{
				RevitGeometry.Units_ConvertStringToDouble(m_ConstantElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: true);
				_dReturnedDouble -= m_dAdjustConstElevation;
			}
			return _dReturnedDouble;
		}
	}

	public bool SegmentChecked => m_SegmentRadioButton.Checked;

	public bool ChainChecked => m_ChainRadioButton.Checked;

	public bool RelativeElevationChecked => m_RelativeElevationRadioButton.Checked;

	public string SitePlacementName => m_NameTextBox.Text;

	public LocateSiteComponentForm(SiteComponentCreation dataBuffer, LandcaddID _CurrentLandcaddID)
	{
		InitializeComponent();
		m_CurrentLandcaddID = _CurrentLandcaddID;
		SetStrings();
		EPToolTip.SetToolTip(m_LoadFamilyButton, EPShared.LocalRM.GetString("LoadFamilyToolTip"));
		EPToolTip.SetToolTip(m_PickInRevitButton, EPShared.LocalRM.GetString("PickPointElevationToolTip"));
		m_dataBuffer = dataBuffer;
		m_FormatOptions = m_dataBuffer.Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		m_DisplayUnitSystem = m_dataBuffer.Document.DisplayUnitSystem;
		m_Units = m_dataBuffer.Document.GetUnits();
		try
		{
			m_UnitSymbolType = m_FormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_UnitSymbolType = UnitSymbolType.UST_NONE;
		}
		m_DecimalSymbolType = m_dataBuffer.Document.GetUnits().DecimalSymbol;
		m_dAdjustConstElevation = 0.0;
		InitializeControls();
	}

	private void SetStrings()
	{
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		string text4 = string.Empty;
		string text5 = string.Empty;
		string text6 = string.Empty;
		switch (m_CurrentLandcaddID)
		{
		case LandcaddID.SoftTerrain:
			text = EPShared.LocalRM.GetString("LocateSoftTerrainDialog");
			text2 = EPShared.LocalRM.GetString("SoftTerrainFamilyDialog");
			text3 = EPShared.LocalRM.GetString("SoftTerrainTypeDialog");
			text4 = EPShared.LocalRM.GetString("SoftTerrainNameDialog");
			text5 = EPShared.LocalRM.GetString("RelativeElevationFromToposurfaceDialog");
			text6 = EPShared.LocalRM.GetString("ConstantElevationDialog");
			ContextualHelpID = "HID_SDsoftTerrainCreate";
			break;
		case LandcaddID.Sidewalk:
			text = EPShared.LocalRM.GetString("LocateSidewalkDialog");
			text2 = EPShared.LocalRM.GetString("SideWalkFamilyDialog");
			text3 = EPShared.LocalRM.GetString("SideWalkTypeDialog");
			text4 = EPShared.LocalRM.GetString("SideWalkNameDialog");
			text5 = EPShared.LocalRM.GetString("RelativeElevationFromToposurfaceDialog");
			text6 = EPShared.LocalRM.GetString("ConstantElevationDialog");
			ContextualHelpID = "HID_SDsidewalkCreate";
			break;
		case LandcaddID.RetainingWall:
			text = EPShared.LocalRM.GetString("LocateRetainingWallDialog");
			text2 = EPShared.LocalRM.GetString("RetainingWallFamilyDialog");
			text3 = EPShared.LocalRM.GetString("RetainingWallTypeDialog");
			text4 = EPShared.LocalRM.GetString("RetainingWallNameDialog");
			text5 = EPShared.LocalRM.GetString("RetainingwallRelativeElevationFromToposurfaceDialog");
			text6 = EPShared.LocalRM.GetString("RetainingwallConstantElevationDialog");
			ContextualHelpID = "HID_SDretainingWallCreate";
			break;
		case LandcaddID.Street:
			text = EPShared.LocalRM.GetString("LocateStreetDialog");
			text2 = EPShared.LocalRM.GetString("StreetFamilyDialog");
			text3 = EPShared.LocalRM.GetString("StreetTypeDialog");
			text4 = EPShared.LocalRM.GetString("StreetNameDialog");
			text5 = EPShared.LocalRM.GetString("RelativeElevationFromToposurfaceDialog");
			text6 = EPShared.LocalRM.GetString("ConstantElevationDialog");
			ContextualHelpID = "HID_SDstreetCreate";
			break;
		case LandcaddID.Curb:
			text = EPShared.LocalRM.GetString("LocateCurbDialog");
			text2 = EPShared.LocalRM.GetString("CurbFamilyDialog");
			text3 = EPShared.LocalRM.GetString("CurbTypeDialog");
			text4 = EPShared.LocalRM.GetString("CurbNameDialog");
			text5 = EPShared.LocalRM.GetString("RelativeElevationFromToposurfaceDialog");
			text6 = EPShared.LocalRM.GetString("ConstantElevationDialog");
			ContextualHelpID = "HID_SDcurbCreate";
			break;
		}
		Text = text;
		m_FamilyLabel.Text = text2;
		m_TypeNameLabel.Text = text3;
		m_NameLabel.Text = text4;
		m_RelativeElevationRadioButton.Text = text5;
		m_ConstantElevationRadioButton.Text = text6;
		m_RelativeElevationLabel.Text = EPShared.LocalRM.GetString("AMP_ElevationDialog");
		m_ConstantElevationLabel.Text = EPShared.LocalRM.GetString("Ele_AMP_vationDialog");
		m_SegmentRadioButton.Text = EPShared.LocalRM.GetString("AMP_SegmentDialog");
		m_ChainRadioButton.Text = EPShared.LocalRM.GetString("AMP_ChainDialog");
		m_CreateCopyCheckBox.Text = EPShared.LocalRM.GetString("Create_AMP_ACopyDialog");
		m_UseExistingHostLineRadioButton.Text = EPShared.LocalRM.GetString("AMP_UseExistingHostLineDialog");
		m_DrawHostLineRadioButton.Text = EPShared.LocalRM.GetString("AMP_DrawHostLineDialog");
		m_RadiusLabel.Text = EPShared.LocalRM.GetString("AMP_RadiusDialog");
		m_InsertButton.Text = EPShared.LocalRM.GetString("AMP_InsertDialog");
		m_CloseButton.Text = EPShared.LocalRM.GetString("AMP_CloseDialog");
	}

	private void InitializeControls()
	{
		int _nReturnValue = 0;
		double _dReturnValue = 0.0;
		string _strToDisplay = string.Empty;
		GetDialogPosition();
		switch (m_CurrentLandcaddID)
		{
		case LandcaddID.SoftTerrain:
			m_strCurrentDlgType = "LocateSoftTerrain";
			break;
		case LandcaddID.Sidewalk:
			m_strCurrentDlgType = "LocateSidewalk";
			break;
		case LandcaddID.RetainingWall:
			m_strCurrentDlgType = "LocateRetainingWall";
			break;
		case LandcaddID.Street:
			m_strCurrentDlgType = "LocateStreet";
			break;
		}
		if (!ApplicationUtility.Settings.eplSetE_GetSetting(m_strCurrentDlgType + "RelativeElevation", ref _dReturnValue))
		{
			_dReturnValue = 15.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		m_RelativeElevationTextBox.Text = _strToDisplay;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting(m_strCurrentDlgType + "ConstantElevation", ref _dReturnValue))
		{
			_dReturnValue = 15.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		m_ConstantElevationTextBox.Text = _strToDisplay;
		m_UseExistingHostLineRadioButton.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_strCurrentDlgType + "HostLineSelectionOption", ref _nReturnValue))
		{
			if (_nReturnValue == 0)
			{
				m_UseExistingHostLineRadioButton.Checked = true;
			}
			else
			{
				m_DrawHostLineRadioButton.Checked = true;
			}
		}
		if (!ApplicationUtility.Settings.eplSetE_GetSetting(m_strCurrentDlgType + "Radius", ref _dReturnValue))
		{
			_dReturnValue = 0.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		m_RadiusTextBox.Text = _strToDisplay;
		m_CreateCopyCheckBox.Checked = false;
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_strCurrentDlgType + "CreateVisibleCopy", ref _nReturnValue))
		{
			m_CreateCopyCheckBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
		m_FamilyComboBox.DataSource = m_dataBuffer.FamilyList;
		m_FamilyComboBox.DisplayMember = "FamilyName";
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_dataBuffer.Document.PathName, m_strCurrentDlgType + "FamilyName", ref _strToDisplay))
		{
			FamilyContainer familyContainer = null;
			for (int i = 0; i < m_FamilyComboBox.Items.Count; i++)
			{
				if (m_FamilyComboBox.Items[i] is FamilyContainer familyContainer2 && string.Compare(familyContainer2.FamilyName, _strToDisplay, ignoreCase: true) == 0)
				{
					m_FamilyComboBox.SelectedIndex = i;
					break;
				}
			}
		}
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_dataBuffer.Document.PathName, m_strCurrentDlgType + "TypeName", ref _strToDisplay))
		{
			FamilySymbol familySymbol = null;
			for (int j = 0; j < m_TypeNameComboBox.Items.Count; j++)
			{
				if (m_TypeNameComboBox.Items[j] is FamilySymbol familySymbol2 && string.Compare(familySymbol2.Name, _strToDisplay, ignoreCase: true) == 0)
				{
					m_TypeNameComboBox.SelectedIndex = j;
					break;
				}
			}
		}
		string _strReturnValue = string.Empty;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting(m_dataBuffer.Document.PathName, m_strCurrentDlgType + "Name", ref _strReturnValue) && m_FamilyComboBox.SelectedItem is FamilyContainer familyContainer3 && familyContainer3.FamilyName != string.Empty)
		{
			_strReturnValue = familyContainer3.FamilyName;
		}
		bool flag = false;
		if ((m_CurrentLandcaddID != LandcaddID.SoftTerrain) ? m_dataBuffer.FindMassingFamilyStartingNumberName(_strReturnValue, ref _nReturnValue) : m_dataBuffer.FindPlacementStartingNumberName(_strReturnValue, ref _nReturnValue))
		{
			m_NameTextBox.Text = _strReturnValue + m_dataBuffer.NUMMERIC_SEPERATOR + Convert.ToString(_nReturnValue);
		}
		else
		{
			m_NameTextBox.Text = _strReturnValue;
		}
		m_ChainRadioButton.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_strCurrentDlgType + "SegmentChainOption", ref _nReturnValue))
		{
			if (_nReturnValue == 0)
			{
				m_SegmentRadioButton.Checked = true;
			}
			else
			{
				m_ChainRadioButton.Checked = true;
			}
		}
		m_RelativeElevationRadioButton.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_strCurrentDlgType + "ElevationOption", ref _nReturnValue))
		{
			if (_nReturnValue == 0)
			{
				m_RelativeElevationRadioButton.Checked = true;
			}
			else
			{
				m_ConstantElevationRadioButton.Checked = true;
			}
		}
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_dataBuffer.Document.PathName, "UseSharedElevation", ref _nReturnValue) && _nReturnValue == 1)
		{
			ProjectLocation activeProjectLocation = m_dataBuffer.Document.ActiveProjectLocation;
			XYZ point = new XYZ();
			ProjectPosition projectPosition = activeProjectLocation.GetProjectPosition(point);
			m_dAdjustConstElevation = projectPosition.Elevation;
		}
	}

	private void SaveSettings()
	{
		m_CurType = m_TypeNameComboBox.SelectedItem as FamilySymbol;
		int num = 0;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_RelativeElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting(m_strCurrentDlgType + "RelativeElevation", _dReturnedDouble);
		}
		if (RevitGeometry.Units_ConvertStringToDouble(m_ConstantElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting(m_strCurrentDlgType + "ConstantElevation", _dReturnedDouble);
		}
		num = ((!m_UseExistingHostLineRadioButton.Checked) ? 1 : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting(m_strCurrentDlgType + "HostLineSelectionOption", num);
		if (RevitGeometry.Units_ConvertStringToDouble(m_RadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting(m_strCurrentDlgType + "Radius", _dReturnedDouble);
		}
		ApplicationUtility.Settings.eplSetE_SetSetting(m_strCurrentDlgType + "CreateVisibleCopy", Convert.ToInt32(m_CreateCopyCheckBox.Checked));
		if (m_FamilyComboBox.SelectedItem is FamilyContainer familyContainer)
		{
			ApplicationUtility.Settings.eplSetE_SetSetting(m_dataBuffer.Document.PathName, m_strCurrentDlgType + "FamilyName", familyContainer.FamilyName);
		}
		ApplicationUtility.Settings.eplSetE_SetSetting(m_dataBuffer.Document.PathName, m_strCurrentDlgType + "TypeName", m_CurType.Name);
		ApplicationUtility.Settings.eplSetE_SetSetting(m_dataBuffer.Document.PathName, m_strCurrentDlgType + "Name", m_dataBuffer.GetSitePlacementNameWithoutSeperator(m_NameTextBox.Text));
		num = ((!m_SegmentRadioButton.Checked) ? 1 : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting(m_strCurrentDlgType + "SegmentChainOption", num);
		num = ((!m_RelativeElevationRadioButton.Checked) ? 1 : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting(m_strCurrentDlgType + "ElevationOption", num);
	}

	private void OnFamilyComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_FamilyComboBox.SelectedItem is FamilyContainer familyContainer)
		{
			m_TypeNameComboBox.DataSource = familyContainer.FamilySymbolList;
			m_TypeNameComboBox.DisplayMember = EPShared.LocalRM.GetString("NameString");
		}
	}

	private void OnLoadFamilyButtonClick(object sender, EventArgs e)
	{
		int count = m_FamilyComboBox.Items.Count;
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		string _strLibraryPath = string.Empty;
		FamilyContainer familyContainer = null;
		Family _Family = null;
		string title = string.Empty;
		switch (m_CurrentLandcaddID)
		{
		case LandcaddID.SoftTerrain:
			title = EPShared.LocalRM.GetString("PleaseSelectSoftTerrainFamilyFileMessage");
			break;
		case LandcaddID.Sidewalk:
			title = EPShared.LocalRM.GetString("PleaseSelectSidewalkFamilyFileMessage");
			break;
		case LandcaddID.RetainingWall:
			title = EPShared.LocalRM.GetString("PleaseSelectRetainingWallFamilyFileMessage");
			break;
		case LandcaddID.Street:
			title = EPShared.LocalRM.GetString("PleaseSelectStreetFamilyFileMessage");
			break;
		}
		openFileDialog.Title = title;
		openFileDialog.RestoreDirectory = true;
		openFileDialog.Filter = ApplicationUtility.GetString("FamilyFilePickerFilter");
		if (ApplicationUtility.RegistryPaths.GetLibraryPath(string.Empty, ref _strLibraryPath) == 0)
		{
			openFileDialog.InitialDirectory = _strLibraryPath;
		}
		bool flag = false;
		DialogResult dialogResult = DialogResult.OK;
		while (dialogResult != DialogResult.Cancel)
		{
			dialogResult = openFileDialog.ShowDialog();
			if (dialogResult == DialogResult.Cancel)
			{
				continue;
			}
			try
			{
				string text = string.Empty;
				switch (m_CurrentLandcaddID)
				{
				case LandcaddID.SoftTerrain:
					text = EPShared.LocalRM.GetString("LoadSoftTerrainFamilyTransaction");
					break;
				case LandcaddID.Sidewalk:
					text = EPShared.LocalRM.GetString("LoadSidewalkFamilyTransaction");
					break;
				case LandcaddID.RetainingWall:
					text = EPShared.LocalRM.GetString("LoadRetainingWallFamilyTransaction");
					break;
				case LandcaddID.Street:
					text = EPShared.LocalRM.GetString("LoadStreetFamilyTransaction");
					break;
				}
				m_dataBuffer.TransactionContainer.Start(text + openFileDialog.SafeFileName);
				EPTransactionContainer _TransactionContainer = m_dataBuffer.TransactionContainer;
				flag = RevitUtility.LoadFamily(m_dataBuffer.Document, openFileDialog.FileName, out _Family, _bUseFailureHandling: true, ref _TransactionContainer);
				m_dataBuffer.TransactionContainer = _TransactionContainer;
				string text2 = string.Empty;
				if (_Family != null)
				{
					m_dataBuffer.TransactionContainer.Commit();
					text2 = _Family.Name;
				}
				else
				{
					m_dataBuffer.TransactionContainer.Rollback();
					Document document = null;
					bool flag2 = false;
					int num = 0;
					foreach (FamilyContainer family in m_dataBuffer.FamilyList)
					{
						document = m_dataBuffer.Document.EditFamily(family.FamilySymbolList[0].Family);
						if (document != null && string.Compare(document.PathName, openFileDialog.FileName, ignoreCase: true) == 0)
						{
							text2 = family.FamilyName;
							flag2 = true;
							break;
						}
						num++;
					}
					if (flag2)
					{
						m_FamilyComboBox.SelectedIndex = num;
					}
				}
				bool flag3 = false;
				int nBeginningIndex = 0;
				if ((m_CurrentLandcaddID != LandcaddID.SoftTerrain) ? m_dataBuffer.FindMassingFamilyStartingNumberName(text2, ref nBeginningIndex) : m_dataBuffer.FindPlacementStartingNumberName(text2, ref nBeginningIndex))
				{
					m_NameTextBox.Text = text2 + m_dataBuffer.NUMMERIC_SEPERATOR + Convert.ToString(nBeginningIndex);
				}
				else
				{
					m_NameTextBox.Text = text2;
				}
			}
			catch (SEHException)
			{
				m_dataBuffer.TransactionContainer.Rollback();
				flag = false;
				string strText = EPShared.LocalRM.GetString("NoSelectionMadeMessage");
				RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				continue;
			}
			break;
		}
		if (!flag)
		{
			if (count == 0)
			{
				string strText2 = EPShared.LocalRM.GetString("NoSelectionMadeMessage");
				RevitMessageBox.Show(strText2, EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
			}
			return;
		}
		m_dataBuffer.PopulateFamilyAndSymbolLists();
		m_FamilyComboBox.DataSource = m_dataBuffer.FamilyList;
		m_FamilyComboBox.DisplayMember = EPShared.LocalRM.GetString("FamilyNameString");
		if (_Family != null)
		{
			for (int i = 0; i < m_FamilyComboBox.Items.Count; i++)
			{
				if (m_FamilyComboBox.Items[i] is FamilyContainer familyContainer2 && string.Compare(familyContainer2.FamilyName, _Family.Name, ignoreCase: true) == 0)
				{
					m_FamilyComboBox.SelectedIndex = i;
					break;
				}
			}
		}
		if (m_FamilyComboBox.Items.Count == count)
		{
			string strText3 = EPShared.LocalRM.GetString("NoSelectionMadeMessage");
			RevitMessageBox.Show(strText3, EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
		}
	}

	private void OnOKButtonClick(object sender, EventArgs e)
	{
		if (ValidateFields())
		{
			SaveSettings();
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	private void OnCloseButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void OnHostLineSelectionOptionChecked(object sender, EventArgs e)
	{
		m_RadiusLabel.Enabled = m_DrawHostLineRadioButton.Checked;
		m_RadiusTextBox.Enabled = m_DrawHostLineRadioButton.Checked;
	}

	private void OnElevationSelectionOptionChecked(object sender, EventArgs e)
	{
		m_ConstantElevationLabel.Enabled = m_ConstantElevationRadioButton.Checked;
		m_ConstantElevationTextBox.Enabled = m_ConstantElevationRadioButton.Checked;
		m_RelativeElevationLabel.Enabled = !m_ConstantElevationRadioButton.Checked;
		m_RelativeElevationTextBox.Enabled = !m_ConstantElevationRadioButton.Checked;
		m_PickInRevitButton.Enabled = m_ConstantElevationRadioButton.Checked;
	}

	private bool ValidateFields()
	{
		if (m_TypeNameComboBox.SelectedItem == null)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("IncorrectFamilySymbolMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return false;
		}
		string text = string.Empty;
		switch (m_CurrentLandcaddID)
		{
		case LandcaddID.SoftTerrain:
			text = EPShared.LocalRM.GetString("SoftTerrainString");
			break;
		case LandcaddID.Sidewalk:
			text = EPShared.LocalRM.GetString("SidewalkString");
			break;
		case LandcaddID.RetainingWall:
			text = EPShared.LocalRM.GetString("RetainingWallString");
			break;
		case LandcaddID.Street:
			text = EPShared.LocalRM.GetString("StreetString");
			break;
		}
		if (string.IsNullOrEmpty(m_NameTextBox.Text))
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("Specify_Message") + text + EPShared.LocalRM.GetString("NameString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			SiteDesigner.UI.WpfFocus.Focus(m_NameTextBox);
			return false;
		}
		if (m_NameTextBox.Text.StartsWith(" ") || m_NameTextBox.Text.EndsWith(" "))
		{
			RevitMessageBox.Show(text + " " + EPShared.LocalRM.GetString("NameString") + EPShared.LocalRM.GetString("_LeadingAndTrailingSpacesMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			SiteDesigner.UI.WpfFocus.Focus(m_NameTextBox);
			return false;
		}
		bool flag = false;
		if ((m_CurrentLandcaddID != LandcaddID.SoftTerrain) ? m_dataBuffer.FindFamilyName(m_NameTextBox.Text) : m_dataBuffer.FindPlacementName(m_NameTextBox.Text))
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("MassingFamilyNameAlreadyUsedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			SiteDesigner.UI.WpfFocus.Focus(m_NameTextBox);
			return false;
		}
		if (m_ChainRadioButton.Checked && m_DrawHostLineRadioButton.Checked)
		{
			m_CurType = m_TypeNameComboBox.SelectedItem as FamilySymbol;
			if (m_CurType == null)
			{
				return false;
			}
			if (m_CurrentLandcaddID != LandcaddID.SoftTerrain)
			{
				Parameter parameter = null;
				double dY = 0.0;
				switch (m_CurrentLandcaddID)
				{
				case LandcaddID.Sidewalk:
					parameter = m_CurType.LookupParameter(RevitParameters.SideWalkWidthParameter);
					if (parameter != null)
					{
						double num5 = parameter.AsDouble();
						dY = num5;
					}
					break;
				case LandcaddID.RetainingWall:
					parameter = m_CurType.LookupParameter(RevitParameters.RetainingWallWidthParameter);
					if (parameter != null)
					{
						double num2 = parameter.AsDouble();
						dY = num2;
					}
					break;
				case LandcaddID.Curb:
				{
					double num3 = 0.0;
					double num4 = 0.0;
					parameter = m_CurType.LookupParameter(RevitParameters.StreetCurbWidthParameter);
					if (parameter != null)
					{
						num3 = parameter.AsDouble();
					}
					parameter = m_CurType.LookupParameter(RevitParameters.StreetGutterWidthParameter);
					if (parameter != null)
					{
						num4 = parameter.AsDouble();
					}
					dY = num3 + num4;
					break;
				}
				case LandcaddID.Street:
				{
					double num = 0.0;
					parameter = m_CurType.LookupParameter(RevitParameters.StreetLaneWidthParameter);
					if (parameter != null)
					{
						num = parameter.AsDouble();
					}
					parameter = m_CurType.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
					if (parameter != null)
					{
						dY = num * (double)parameter.AsInteger();
					}
					break;
				}
				}
				double _dReturnedDouble = 0.0;
				if (m_RadiusTextBox.Text != string.Empty)
				{
					RevitGeometry.Units_ConvertStringToDouble(m_RadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: true);
				}
				if (RevitDefines.LT(_dReturnedDouble, dY) && !RevitDefines.EQ(_dReturnedDouble, 0.0)
					&& !(m_CurrentLandcaddID == LandcaddID.Street && TerrainApi.UsesToposolids))
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidRadiusString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return false;
				}
			}
		}
		return true;
	}

	private void FormatRelativeElevationValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_RelativeElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: true))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
			m_RelativeElevationTextBox.Text = _strToDisplay;
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_RelativeElevationTextBox);
		}
	}

	private void OnRelativeElevationKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatRelativeElevationValue();
		}
	}

	private void OnRelativeElevationTextBoxLeave(object sender, EventArgs e)
	{
		FormatRelativeElevationValue();
	}

	private void FormatConstantElevationValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_ConstantElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: true))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
			m_ConstantElevationTextBox.Text = _strToDisplay;
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_ConstantElevationTextBox);
		}
	}

	private void OnConstantElevationKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatConstantElevationValue();
		}
	}

	private void OnConstantElevationTextBoxLeave(object sender, EventArgs e)
	{
		FormatConstantElevationValue();
		m_CurrentElevationFocus = CurrentElevationFocus.ConstantElevation;
	}

	private void FormatRadiusValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_RadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: true))
		{
			if (_dReturnedDouble < 0.0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidRadiusValueString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_RadiusTextBox.Text = string.Empty;
			}
			else
			{
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
				m_RadiusTextBox.Text = _strToDisplay;
			}
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_RadiusTextBox);
		}
	}

	private void OnRadiusKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatRadiusValue();
		}
	}

	private void OnRadiusTextBoxLeave(object sender, EventArgs e)
	{
		FormatRadiusValue();
	}

	private void OnLocateSiteComponentClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	public void ResetSiteComponentName()
	{
		string sitePlacementNameWithoutSeperator = m_dataBuffer.GetSitePlacementNameWithoutSeperator(m_NameTextBox.Text);
		int nBeginningIndex = 0;
		bool flag = false;
		if ((m_CurrentLandcaddID != LandcaddID.SoftTerrain) ? m_dataBuffer.FindMassingFamilyStartingNumberName(sitePlacementNameWithoutSeperator, ref nBeginningIndex) : m_dataBuffer.FindPlacementStartingNumberName(sitePlacementNameWithoutSeperator, ref nBeginningIndex))
		{
			m_NameTextBox.Text = sitePlacementNameWithoutSeperator + m_dataBuffer.NUMMERIC_SEPERATOR + Convert.ToString(nBeginningIndex);
		}
		else
		{
			m_NameTextBox.Text = sitePlacementNameWithoutSeperator;
		}
	}

	private void OnPickInRevitButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnElevationTextBoxEnter(object sender, EventArgs e)
	{
		m_PickInRevitButton.Enabled = true;
	}

	private void OnElevationTextBoxLostFocus(object sender, EventArgs e)
	{
		m_PickInRevitButton.Enabled = m_ConstantElevationRadioButton.Checked;
	}

	public void SetFocusedElevation(double _dElevation)
	{
		string _strToDisplay = string.Empty;
		_dElevation += m_dAdjustConstElevation;
		RevitGeometry.Units_ConvertDoubleToString(_dElevation, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		if (m_CurrentElevationFocus == CurrentElevationFocus.ConstantElevation)
		{
			m_ConstantElevationTextBox.Text = _strToDisplay;
			m_PickInRevitButton.Enabled = true;
		}
	}

	private void OnLocateSiteComponentFormShown(object sender, EventArgs e)
	{
		switch (m_CurrentElevationFocus)
		{
		case CurrentElevationFocus.None:
			m_PickInRevitButton.Enabled = false;
			break;
		case CurrentElevationFocus.ConstantElevation:
			SiteDesigner.UI.WpfFocus.Focus(m_ConstantElevationTextBox);
			m_PickInRevitButton.Enabled = true;
			break;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EPCommon.LocateSiteComponentForm));
		this.m_LoadFamilyButton = new System.Windows.Forms.Button();
		this.m_TypeNameComboBox = new System.Windows.Forms.ComboBox();
		this.m_TypeNameLabel = new System.Windows.Forms.Label();
		this.m_FamilyComboBox = new System.Windows.Forms.ComboBox();
		this.m_FamilyLabel = new System.Windows.Forms.Label();
		this.m_ConstantElevationLabel = new System.Windows.Forms.Label();
		this.m_ConstantElevationTextBox = new System.Windows.Forms.TextBox();
		this.m_MoveablePanel = new System.Windows.Forms.Panel();
		this.m_CloseButton = new System.Windows.Forms.Button();
		this.m_InsertButton = new System.Windows.Forms.Button();
		this.m_CreateCopyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_RadiusLabel = new System.Windows.Forms.Label();
		this.m_RadiusTextBox = new System.Windows.Forms.TextBox();
		this.m_NameLabel = new System.Windows.Forms.Label();
		this.m_NameTextBox = new System.Windows.Forms.TextBox();
		this.m_RelativeElevationRadioButton = new System.Windows.Forms.RadioButton();
		this.m_RelativeElevationLabel = new System.Windows.Forms.Label();
		this.m_RelativeElevationTextBox = new System.Windows.Forms.TextBox();
		this.m_ConstantElevationRadioButton = new System.Windows.Forms.RadioButton();
		this.m_UsePanel = new System.Windows.Forms.Panel();
		this.m_SegmentRadioButton = new System.Windows.Forms.RadioButton();
		this.m_ChainRadioButton = new System.Windows.Forms.RadioButton();
		this.m_HostLineSelectionPanel = new System.Windows.Forms.Panel();
		this.m_UseExistingHostLineRadioButton = new System.Windows.Forms.RadioButton();
		this.m_DrawHostLineRadioButton = new System.Windows.Forms.RadioButton();
		this.m_PickInRevitButton = new System.Windows.Forms.Button();
		this.m_MoveablePanel.SuspendLayout();
		this.m_UsePanel.SuspendLayout();
		this.m_HostLineSelectionPanel.SuspendLayout();
		base.SuspendLayout();
		this.m_LoadFamilyButton.Location = new System.Drawing.Point(434, 12);
		this.m_LoadFamilyButton.Name = "m_LoadFamilyButton";
		this.m_LoadFamilyButton.Size = new System.Drawing.Size(24, 20);
		this.m_LoadFamilyButton.TabIndex = 2;
		this.m_LoadFamilyButton.TabStop = false;
		this.m_LoadFamilyButton.Text = "...";
		this.m_LoadFamilyButton.UseVisualStyleBackColor = true;
		this.m_LoadFamilyButton.Click += new System.EventHandler(this.OnLoadFamilyButtonClick);
		this.m_TypeNameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_TypeNameComboBox.FormattingEnabled = true;
		this.m_TypeNameComboBox.Location = new System.Drawing.Point(189, 40);
		this.m_TypeNameComboBox.Name = "m_TypeNameComboBox";
		this.m_TypeNameComboBox.Size = new System.Drawing.Size(239, 21);
		this.m_TypeNameComboBox.TabIndex = 5;
		this.m_TypeNameLabel.AutoSize = true;
		this.m_TypeNameLabel.Location = new System.Drawing.Point(12, 42);
		this.m_TypeNameLabel.Name = "m_TypeNameLabel";
		this.m_TypeNameLabel.Size = new System.Drawing.Size(92, 13);
		this.m_TypeNameLabel.TabIndex = 4;
		this.m_TypeNameLabel.Text = "Soft &Terrain Type:";
		this.m_FamilyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_FamilyComboBox.FormattingEnabled = true;
		this.m_FamilyComboBox.Location = new System.Drawing.Point(189, 13);
		this.m_FamilyComboBox.Name = "m_FamilyComboBox";
		this.m_FamilyComboBox.Size = new System.Drawing.Size(239, 21);
		this.m_FamilyComboBox.TabIndex = 1;
		this.m_FamilyComboBox.SelectedIndexChanged += new System.EventHandler(this.OnFamilyComboBoxSelectedIndexChanged);
		this.m_FamilyLabel.AutoSize = true;
		this.m_FamilyLabel.Location = new System.Drawing.Point(12, 17);
		this.m_FamilyLabel.Name = "m_FamilyLabel";
		this.m_FamilyLabel.Size = new System.Drawing.Size(97, 13);
		this.m_FamilyLabel.TabIndex = 0;
		this.m_FamilyLabel.Text = "&Soft Terrain Family:";
		this.m_ConstantElevationLabel.AutoSize = true;
		this.m_ConstantElevationLabel.Enabled = false;
		this.m_ConstantElevationLabel.Location = new System.Drawing.Point(30, 167);
		this.m_ConstantElevationLabel.Name = "m_ConstantElevationLabel";
		this.m_ConstantElevationLabel.Size = new System.Drawing.Size(54, 13);
		this.m_ConstantElevationLabel.TabIndex = 12;
		this.m_ConstantElevationLabel.Text = "Elevation:";
		this.m_ConstantElevationTextBox.Enabled = false;
		this.m_ConstantElevationTextBox.Location = new System.Drawing.Point(189, 164);
		this.m_ConstantElevationTextBox.Name = "m_ConstantElevationTextBox";
		this.m_ConstantElevationTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_ConstantElevationTextBox.TabIndex = 13;
		this.m_ConstantElevationTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnConstantElevationKeyDown);
		this.m_ConstantElevationTextBox.Leave += new System.EventHandler(this.OnConstantElevationTextBoxLeave);
		this.m_ConstantElevationTextBox.Enter += new System.EventHandler(this.OnElevationTextBoxEnter);
		this.m_ConstantElevationTextBox.LostFocus += new System.EventHandler(this.OnElevationTextBoxLostFocus);
		this.m_MoveablePanel.Controls.Add(this.m_CloseButton);
		this.m_MoveablePanel.Controls.Add(this.m_InsertButton);
		this.m_MoveablePanel.Location = new System.Drawing.Point(12, 308);
		this.m_MoveablePanel.Name = "m_MoveablePanel";
		this.m_MoveablePanel.Size = new System.Drawing.Size(476, 23);
		this.m_MoveablePanel.TabIndex = 19;
		this.m_CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_CloseButton.Location = new System.Drawing.Point(400, 0);
		this.m_CloseButton.Name = "m_CloseButton";
		this.m_CloseButton.Size = new System.Drawing.Size(76, 23);
		this.m_CloseButton.TabIndex = 1;
		this.m_CloseButton.Text = "&Cancel";
		this.m_CloseButton.UseVisualStyleBackColor = true;
		this.m_CloseButton.Click += new System.EventHandler(this.OnCloseButtonClick);
		this.m_InsertButton.Location = new System.Drawing.Point(319, 0);
		this.m_InsertButton.Name = "m_InsertButton";
		this.m_InsertButton.Size = new System.Drawing.Size(75, 23);
		this.m_InsertButton.TabIndex = 0;
		this.m_InsertButton.Text = "&Insert";
		this.m_InsertButton.UseVisualStyleBackColor = true;
		this.m_InsertButton.Click += new System.EventHandler(this.OnOKButtonClick);
		this.m_CreateCopyCheckBox.AutoSize = true;
		this.m_CreateCopyCheckBox.Location = new System.Drawing.Point(12, 213);
		this.m_CreateCopyCheckBox.Name = "m_CreateCopyCheckBox";
		this.m_CreateCopyCheckBox.Size = new System.Drawing.Size(94, 17);
		this.m_CreateCopyCheckBox.TabIndex = 15;
		this.m_CreateCopyCheckBox.Text = "Create A Copy";
		this.m_CreateCopyCheckBox.UseVisualStyleBackColor = true;
		this.m_RadiusLabel.AutoSize = true;
		this.m_RadiusLabel.Enabled = false;
		this.m_RadiusLabel.Location = new System.Drawing.Point(30, 285);
		this.m_RadiusLabel.Name = "m_RadiusLabel";
		this.m_RadiusLabel.Size = new System.Drawing.Size(43, 13);
		this.m_RadiusLabel.TabIndex = 17;
		this.m_RadiusLabel.Text = "Radius:";
		this.m_RadiusTextBox.Enabled = false;
		this.m_RadiusTextBox.Location = new System.Drawing.Point(189, 282);
		this.m_RadiusTextBox.Name = "m_RadiusTextBox";
		this.m_RadiusTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_RadiusTextBox.TabIndex = 18;
		this.m_RadiusTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnRadiusKeyDown);
		this.m_RadiusTextBox.Leave += new System.EventHandler(this.OnRadiusTextBoxLeave);
		this.m_NameLabel.AutoSize = true;
		this.m_NameLabel.Location = new System.Drawing.Point(12, 69);
		this.m_NameLabel.Name = "m_NameLabel";
		this.m_NameLabel.Size = new System.Drawing.Size(96, 13);
		this.m_NameLabel.TabIndex = 6;
		this.m_NameLabel.Text = "Soft Terrain Name:";
		this.m_NameTextBox.Location = new System.Drawing.Point(189, 67);
		this.m_NameTextBox.Name = "m_NameTextBox";
		this.m_NameTextBox.Size = new System.Drawing.Size(239, 20);
		this.m_NameTextBox.TabIndex = 7;
		this.m_RelativeElevationRadioButton.AutoSize = true;
		this.m_RelativeElevationRadioButton.Location = new System.Drawing.Point(12, 92);
		this.m_RelativeElevationRadioButton.Name = "m_RelativeElevationRadioButton";
		this.m_RelativeElevationRadioButton.Size = new System.Drawing.Size(197, 17);
		this.m_RelativeElevationRadioButton.TabIndex = 8;
		this.m_RelativeElevationRadioButton.Text = "&Relative Elevation from Toposurface";
		this.m_RelativeElevationRadioButton.UseVisualStyleBackColor = true;
		this.m_RelativeElevationRadioButton.CheckedChanged += new System.EventHandler(this.OnElevationSelectionOptionChecked);
		this.m_RelativeElevationLabel.AutoSize = true;
		this.m_RelativeElevationLabel.Enabled = false;
		this.m_RelativeElevationLabel.Location = new System.Drawing.Point(30, 118);
		this.m_RelativeElevationLabel.Name = "m_RelativeElevationLabel";
		this.m_RelativeElevationLabel.Size = new System.Drawing.Size(54, 13);
		this.m_RelativeElevationLabel.TabIndex = 9;
		this.m_RelativeElevationLabel.Text = "Elevation:";
		this.m_RelativeElevationTextBox.Enabled = false;
		this.m_RelativeElevationTextBox.Location = new System.Drawing.Point(189, 115);
		this.m_RelativeElevationTextBox.Name = "m_RelativeElevationTextBox";
		this.m_RelativeElevationTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_RelativeElevationTextBox.TabIndex = 10;
		this.m_RelativeElevationTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnRelativeElevationKeyDown);
		this.m_RelativeElevationTextBox.Leave += new System.EventHandler(this.OnRelativeElevationTextBoxLeave);
		this.m_ConstantElevationRadioButton.AutoSize = true;
		this.m_ConstantElevationRadioButton.Location = new System.Drawing.Point(12, 141);
		this.m_ConstantElevationRadioButton.Name = "m_ConstantElevationRadioButton";
		this.m_ConstantElevationRadioButton.Size = new System.Drawing.Size(114, 17);
		this.m_ConstantElevationRadioButton.TabIndex = 11;
		this.m_ConstantElevationRadioButton.Text = "&Constant Elevation";
		this.m_ConstantElevationRadioButton.UseVisualStyleBackColor = true;
		this.m_ConstantElevationRadioButton.CheckedChanged += new System.EventHandler(this.OnElevationSelectionOptionChecked);
		this.m_UsePanel.Controls.Add(this.m_SegmentRadioButton);
		this.m_UsePanel.Controls.Add(this.m_ChainRadioButton);
		this.m_UsePanel.Location = new System.Drawing.Point(12, 190);
		this.m_UsePanel.Name = "m_UsePanel";
		this.m_UsePanel.Size = new System.Drawing.Size(476, 18);
		this.m_UsePanel.TabIndex = 14;
		this.m_SegmentRadioButton.AutoSize = true;
		this.m_SegmentRadioButton.Location = new System.Drawing.Point(0, 0);
		this.m_SegmentRadioButton.Name = "m_SegmentRadioButton";
		this.m_SegmentRadioButton.Size = new System.Drawing.Size(67, 17);
		this.m_SegmentRadioButton.TabIndex = 0;
		this.m_SegmentRadioButton.Text = "&Segment";
		this.m_SegmentRadioButton.UseVisualStyleBackColor = true;
		this.m_ChainRadioButton.AutoSize = true;
		this.m_ChainRadioButton.Checked = true;
		this.m_ChainRadioButton.Location = new System.Drawing.Point(207, 0);
		this.m_ChainRadioButton.Name = "m_ChainRadioButton";
		this.m_ChainRadioButton.Size = new System.Drawing.Size(52, 17);
		this.m_ChainRadioButton.TabIndex = 1;
		this.m_ChainRadioButton.TabStop = true;
		this.m_ChainRadioButton.Text = "&Chain";
		this.m_ChainRadioButton.UseVisualStyleBackColor = true;
		this.m_HostLineSelectionPanel.Controls.Add(this.m_UseExistingHostLineRadioButton);
		this.m_HostLineSelectionPanel.Controls.Add(this.m_DrawHostLineRadioButton);
		this.m_HostLineSelectionPanel.Location = new System.Drawing.Point(12, 236);
		this.m_HostLineSelectionPanel.Name = "m_HostLineSelectionPanel";
		this.m_HostLineSelectionPanel.Size = new System.Drawing.Size(476, 41);
		this.m_HostLineSelectionPanel.TabIndex = 16;
		this.m_UseExistingHostLineRadioButton.AutoSize = true;
		this.m_UseExistingHostLineRadioButton.Location = new System.Drawing.Point(0, 0);
		this.m_UseExistingHostLineRadioButton.Name = "m_UseExistingHostLineRadioButton";
		this.m_UseExistingHostLineRadioButton.Size = new System.Drawing.Size(146, 17);
		this.m_UseExistingHostLineRadioButton.TabIndex = 0;
		this.m_UseExistingHostLineRadioButton.Text = "&Use an Existing Host Line";
		this.m_UseExistingHostLineRadioButton.UseVisualStyleBackColor = true;
		this.m_UseExistingHostLineRadioButton.CheckedChanged += new System.EventHandler(this.OnHostLineSelectionOptionChecked);
		this.m_DrawHostLineRadioButton.AutoSize = true;
		this.m_DrawHostLineRadioButton.Location = new System.Drawing.Point(0, 23);
		this.m_DrawHostLineRadioButton.Name = "m_DrawHostLineRadioButton";
		this.m_DrawHostLineRadioButton.Size = new System.Drawing.Size(228, 17);
		this.m_DrawHostLineRadioButton.TabIndex = 1;
		this.m_DrawHostLineRadioButton.Text = "&Draw Host Line (Click Escape to Complete)";
		this.m_DrawHostLineRadioButton.UseVisualStyleBackColor = true;
		this.m_DrawHostLineRadioButton.CheckedChanged += new System.EventHandler(this.OnHostLineSelectionOptionChecked);
		this.m_PickInRevitButton.Enabled = false;
		this.m_PickInRevitButton.Image = (System.Drawing.Image)EPCommon.SafeResources.GetObject(resources, "m_PickInRevitButton.Image");
		this.m_PickInRevitButton.Location = new System.Drawing.Point(464, 12);
		this.m_PickInRevitButton.Name = "m_PickInRevitButton";
		this.m_PickInRevitButton.Size = new System.Drawing.Size(24, 20);
		this.m_PickInRevitButton.TabIndex = 3;
		this.m_PickInRevitButton.TabStop = false;
		this.m_PickInRevitButton.UseVisualStyleBackColor = true;
		this.m_PickInRevitButton.Click += new System.EventHandler(this.OnPickInRevitButtonClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(500, 343);
		base.Controls.Add(this.m_PickInRevitButton);
		base.Controls.Add(this.m_HostLineSelectionPanel);
		base.Controls.Add(this.m_UsePanel);
		base.Controls.Add(this.m_ConstantElevationRadioButton);
		base.Controls.Add(this.m_RelativeElevationLabel);
		base.Controls.Add(this.m_RelativeElevationTextBox);
		base.Controls.Add(this.m_RelativeElevationRadioButton);
		base.Controls.Add(this.m_NameLabel);
		base.Controls.Add(this.m_NameTextBox);
		base.Controls.Add(this.m_RadiusLabel);
		base.Controls.Add(this.m_RadiusTextBox);
		base.Controls.Add(this.m_CreateCopyCheckBox);
		base.Controls.Add(this.m_MoveablePanel);
		base.Controls.Add(this.m_ConstantElevationLabel);
		base.Controls.Add(this.m_ConstantElevationTextBox);
		base.Controls.Add(this.m_LoadFamilyButton);
		base.Controls.Add(this.m_TypeNameComboBox);
		base.Controls.Add(this.m_TypeNameLabel);
		base.Controls.Add(this.m_FamilyComboBox);
		base.Controls.Add(this.m_FamilyLabel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "LocateSiteComponentForm";
		this.Text = "Locate Soft Terrain";
		base.Shown += new System.EventHandler(this.OnLocateSiteComponentFormShown);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnLocateSiteComponentClosing);
		this.m_MoveablePanel.ResumeLayout(false);
		this.m_UsePanel.ResumeLayout(false);
		this.m_UsePanel.PerformLayout();
		this.m_HostLineSelectionPanel.ResumeLayout(false);
		this.m_HostLineSelectionPanel.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_InsertButton;
		base.CancelButton = this.m_CloseButton;
	}
}
