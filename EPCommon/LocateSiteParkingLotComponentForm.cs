using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

namespace EPCommon;

public class LocateSiteParkingLotComponentForm : EPForm
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

	private DecimalSymbol m_DecimalSymbolType;

	private IContainer components;

	private Button m_LoadFamilyButton;

	private ComboBox m_TypeNameComboBox;

	private Label m_TypeNameLabel;

	private ComboBox m_FamilyComboBox;

	private Label m_FamilyLabel;

	private System.Windows.Forms.Panel m_MoveablePanel;

	private Button m_CloseButton;

	private Button m_InsertButton;

	private Label m_RadiusLabel;

	private TextBox m_RadiusTextBox;

	private Label m_NameLabel;

	private TextBox m_NameTextBox;

	private System.Windows.Forms.Panel m_HostLineSelectionPanel;

	private RadioButton m_UseExistingHostLineRadioButton;

	private RadioButton m_DrawHostLineRadioButton;

	private CheckBox m_CreateCopyCheckBox;

	public bool ExistingHostLineChecked => m_UseExistingHostLineRadioButton.Checked;

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
				RevitGeometry.Units_ConvertStringToDouble(m_RadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
			}
			return _dReturnedDouble;
		}
	}

	public bool CreateCopyChecked => m_CreateCopyCheckBox.Checked;

	public FamilySymbol Type => m_CurType;

	public string SitePlacementName => m_NameTextBox.Text;

	public LocateSiteParkingLotComponentForm(SiteComponentCreation dataBuffer)
	{
		InitializeComponent();
		SetStrings();
		EPToolTip.SetToolTip(m_LoadFamilyButton, EPShared.LocalRM.GetString("LoadFamilyToolTip"));
		ContextualHelpID = "HID_SDparkinglotCreate";
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
		InitializeControls();
	}

	private void SetStrings()
	{
		Text = EPShared.LocalRM.GetString("LocateParkingLotDialog");
		m_FamilyLabel.Text = EPShared.LocalRM.GetString("AMP_ParkingLotFamilyDialog");
		m_TypeNameLabel.Text = EPShared.LocalRM.GetString("Par_AMP_kingLotTypeDialog");
		m_NameLabel.Text = EPShared.LocalRM.GetString("Parki_AMP_ngLotNameDialog");
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
		m_UseExistingHostLineRadioButton.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("LocateParkingLotHostLineSelectionOption", ref _nReturnValue))
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
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("LocateParkingLotRadius", ref _dReturnValue))
		{
			_dReturnValue = 0.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
		m_RadiusTextBox.Text = _strToDisplay;
		m_CreateCopyCheckBox.Checked = false;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("LocateParkingLotCreateVisibleCopy", ref _nReturnValue))
		{
			m_CreateCopyCheckBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
		m_FamilyComboBox.DataSource = m_dataBuffer.FamilyList;
		m_FamilyComboBox.DisplayMember = ApplicationUtility.GetString("FamilyNameString");
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_dataBuffer.Document.PathName, "LocateParkingLotFamilyName", ref _strToDisplay))
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
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_dataBuffer.Document.PathName, "LocateParkingLotTypeName", ref _strToDisplay))
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
		if (!ApplicationUtility.Settings.eplSetE_GetSetting(m_dataBuffer.Document.PathName, "LocateParkingLotName", ref _strReturnValue) && m_FamilyComboBox.SelectedItem is FamilyContainer familyContainer3 && familyContainer3.FamilyName != string.Empty)
		{
			_strReturnValue = familyContainer3.FamilyName;
		}
		if (m_dataBuffer.FindPlacementStartingNumberName(_strReturnValue, ref _nReturnValue))
		{
			m_NameTextBox.Text = _strReturnValue + m_dataBuffer.NUMMERIC_SEPERATOR + Convert.ToString(_nReturnValue);
		}
		else
		{
			m_NameTextBox.Text = _strReturnValue;
		}
	}

	private void SaveSettings()
	{
		m_CurType = m_TypeNameComboBox.SelectedItem as FamilySymbol;
		int num = 0;
		double _dReturnedDouble = 0.0;
		num = ((!m_UseExistingHostLineRadioButton.Checked) ? 1 : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting("LocateParkingLotHostLineSelectionOption", num);
		if (RevitGeometry.Units_ConvertStringToDouble(m_RadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("LocateParkingLotRadius", _dReturnedDouble);
		}
		if (m_FamilyComboBox.SelectedItem is FamilyContainer familyContainer)
		{
			ApplicationUtility.Settings.eplSetE_SetSetting(m_dataBuffer.Document.PathName, "LocateParkingLotFamilyName", familyContainer.FamilyName);
		}
		ApplicationUtility.Settings.eplSetE_SetSetting(m_dataBuffer.Document.PathName, "LocateParkingLotTypeName", m_CurType.Name);
		ApplicationUtility.Settings.eplSetE_SetSetting("LocateParkingLotCreateVisibleCopy", Convert.ToInt32(m_CreateCopyCheckBox.Checked));
		ApplicationUtility.Settings.eplSetE_SetSetting(m_dataBuffer.Document.PathName, "LocateParkingLotName", m_dataBuffer.GetSitePlacementNameWithoutSeperator(m_NameTextBox.Text));
	}

	private void OnFamilyComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_FamilyComboBox.SelectedItem is FamilyContainer familyContainer)
		{
			m_TypeNameComboBox.DataSource = familyContainer.FamilySymbolList;
			m_TypeNameComboBox.DisplayMember = ApplicationUtility.GetString("NameString");
		}
	}

	private void OnLoadFamilyButtonClick(object sender, EventArgs e)
	{
		int count = m_FamilyComboBox.Items.Count;
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		string _strLibraryPath = string.Empty;
		FamilyContainer familyContainer = null;
		Family _Family = null;
		openFileDialog.Title = EPShared.LocalRM.GetString("PleaseSelectParkingLotFamilyFileMessage");
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
				string text = EPShared.LocalRM.GetString("LoadParkingLotFamilyTransaction");
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
				if (m_dataBuffer.FindPlacementStartingNumberName(text2, ref nBeginningIndex))
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
		m_FamilyComboBox.DisplayMember = ApplicationUtility.GetString("FamilyNameString");
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

	private bool ValidateFields()
	{
		if (m_TypeNameComboBox.SelectedItem == null)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("IncorrectFamilySymbolMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return false;
		}
		if (string.IsNullOrEmpty(m_NameTextBox.Text))
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("Specify_Message") + EPShared.LocalRM.GetString("ParkingLotString") + EPShared.LocalRM.GetString("NameString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			SiteDesigner.UI.WpfFocus.Focus(m_NameTextBox);
			return false;
		}
		if (m_NameTextBox.Text.StartsWith(" ") || m_NameTextBox.Text.EndsWith(" "))
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("ParkingLotString") + " " + EPShared.LocalRM.GetString("NameString") + EPShared.LocalRM.GetString("_LeadingAndTrailingSpacesMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			SiteDesigner.UI.WpfFocus.Focus(m_NameTextBox);
			return false;
		}
		if (m_dataBuffer.FindPlacementName(m_NameTextBox.Text))
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("MassingFamilyNameAlreadyUsedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			SiteDesigner.UI.WpfFocus.Focus(m_NameTextBox);
			return false;
		}
		return true;
	}

	private void FormatRadiusValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_RadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble))
		{
			if (_dReturnedDouble < 0.0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidRadiusValueString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_RadiusTextBox.Text = string.Empty;
			}
			else
			{
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
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
		if (m_dataBuffer.FindPlacementStartingNumberName(sitePlacementNameWithoutSeperator, ref nBeginningIndex))
		{
			m_NameTextBox.Text = sitePlacementNameWithoutSeperator + m_dataBuffer.NUMMERIC_SEPERATOR + Convert.ToString(nBeginningIndex);
		}
		else
		{
			m_NameTextBox.Text = sitePlacementNameWithoutSeperator;
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
		this.m_LoadFamilyButton = new System.Windows.Forms.Button();
		this.m_TypeNameComboBox = new System.Windows.Forms.ComboBox();
		this.m_TypeNameLabel = new System.Windows.Forms.Label();
		this.m_FamilyComboBox = new System.Windows.Forms.ComboBox();
		this.m_FamilyLabel = new System.Windows.Forms.Label();
		this.m_MoveablePanel = new System.Windows.Forms.Panel();
		this.m_CloseButton = new System.Windows.Forms.Button();
		this.m_InsertButton = new System.Windows.Forms.Button();
		this.m_RadiusLabel = new System.Windows.Forms.Label();
		this.m_RadiusTextBox = new System.Windows.Forms.TextBox();
		this.m_NameLabel = new System.Windows.Forms.Label();
		this.m_NameTextBox = new System.Windows.Forms.TextBox();
		this.m_HostLineSelectionPanel = new System.Windows.Forms.Panel();
		this.m_UseExistingHostLineRadioButton = new System.Windows.Forms.RadioButton();
		this.m_DrawHostLineRadioButton = new System.Windows.Forms.RadioButton();
		this.m_CreateCopyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_MoveablePanel.SuspendLayout();
		this.m_HostLineSelectionPanel.SuspendLayout();
		base.SuspendLayout();
		this.m_LoadFamilyButton.Location = new System.Drawing.Point(464, 11);
		this.m_LoadFamilyButton.Name = "m_LoadFamilyButton";
		this.m_LoadFamilyButton.Size = new System.Drawing.Size(24, 20);
		this.m_LoadFamilyButton.TabIndex = 2;
		this.m_LoadFamilyButton.TabStop = false;
		this.m_LoadFamilyButton.Text = "...";
		this.m_LoadFamilyButton.UseVisualStyleBackColor = true;
		this.m_LoadFamilyButton.Click += new System.EventHandler(this.OnLoadFamilyButtonClick);
		this.m_TypeNameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_TypeNameComboBox.FormattingEnabled = true;
		this.m_TypeNameComboBox.Location = new System.Drawing.Point(219, 39);
		this.m_TypeNameComboBox.Name = "m_TypeNameComboBox";
		this.m_TypeNameComboBox.Size = new System.Drawing.Size(239, 21);
		this.m_TypeNameComboBox.TabIndex = 4;
		this.m_TypeNameLabel.AutoSize = true;
		this.m_TypeNameLabel.Location = new System.Drawing.Point(12, 42);
		this.m_TypeNameLabel.Name = "m_TypeNameLabel";
		this.m_TypeNameLabel.Size = new System.Drawing.Size(91, 13);
		this.m_TypeNameLabel.TabIndex = 3;
		this.m_TypeNameLabel.Text = "Parking Lot Type:";
		this.m_FamilyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_FamilyComboBox.FormattingEnabled = true;
		this.m_FamilyComboBox.Location = new System.Drawing.Point(219, 12);
		this.m_FamilyComboBox.Name = "m_FamilyComboBox";
		this.m_FamilyComboBox.Size = new System.Drawing.Size(239, 21);
		this.m_FamilyComboBox.TabIndex = 1;
		this.m_FamilyComboBox.SelectedIndexChanged += new System.EventHandler(this.OnFamilyComboBoxSelectedIndexChanged);
		this.m_FamilyLabel.AutoSize = true;
		this.m_FamilyLabel.Location = new System.Drawing.Point(12, 17);
		this.m_FamilyLabel.Name = "m_FamilyLabel";
		this.m_FamilyLabel.Size = new System.Drawing.Size(96, 13);
		this.m_FamilyLabel.TabIndex = 0;
		this.m_FamilyLabel.Text = "&Parking Lot Family:";
		this.m_MoveablePanel.Controls.Add(this.m_CloseButton);
		this.m_MoveablePanel.Controls.Add(this.m_InsertButton);
		this.m_MoveablePanel.Location = new System.Drawing.Point(12, 187);
		this.m_MoveablePanel.Name = "m_MoveablePanel";
		this.m_MoveablePanel.Size = new System.Drawing.Size(476, 23);
		this.m_MoveablePanel.TabIndex = 11;
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
		this.m_RadiusLabel.AutoSize = true;
		this.m_RadiusLabel.Enabled = false;
		this.m_RadiusLabel.Location = new System.Drawing.Point(30, 164);
		this.m_RadiusLabel.Name = "m_RadiusLabel";
		this.m_RadiusLabel.Size = new System.Drawing.Size(43, 13);
		this.m_RadiusLabel.TabIndex = 9;
		this.m_RadiusLabel.Text = "Radius:";
		this.m_RadiusTextBox.Enabled = false;
		this.m_RadiusTextBox.Location = new System.Drawing.Point(219, 161);
		this.m_RadiusTextBox.Name = "m_RadiusTextBox";
		this.m_RadiusTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_RadiusTextBox.TabIndex = 10;
		this.m_RadiusTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnRadiusKeyDown);
		this.m_RadiusTextBox.Leave += new System.EventHandler(this.OnRadiusTextBoxLeave);
		this.m_NameLabel.AutoSize = true;
		this.m_NameLabel.Location = new System.Drawing.Point(12, 69);
		this.m_NameLabel.Name = "m_NameLabel";
		this.m_NameLabel.Size = new System.Drawing.Size(95, 13);
		this.m_NameLabel.TabIndex = 5;
		this.m_NameLabel.Text = "Parking Lot &Name:";
		this.m_NameTextBox.Location = new System.Drawing.Point(219, 66);
		this.m_NameTextBox.Name = "m_NameTextBox";
		this.m_NameTextBox.Size = new System.Drawing.Size(239, 20);
		this.m_NameTextBox.TabIndex = 6;
		this.m_HostLineSelectionPanel.Controls.Add(this.m_UseExistingHostLineRadioButton);
		this.m_HostLineSelectionPanel.Controls.Add(this.m_DrawHostLineRadioButton);
		this.m_HostLineSelectionPanel.Location = new System.Drawing.Point(15, 115);
		this.m_HostLineSelectionPanel.Name = "m_HostLineSelectionPanel";
		this.m_HostLineSelectionPanel.Size = new System.Drawing.Size(476, 41);
		this.m_HostLineSelectionPanel.TabIndex = 8;
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
		this.m_CreateCopyCheckBox.AutoSize = true;
		this.m_CreateCopyCheckBox.Location = new System.Drawing.Point(15, 92);
		this.m_CreateCopyCheckBox.Name = "m_CreateCopyCheckBox";
		this.m_CreateCopyCheckBox.Size = new System.Drawing.Size(94, 17);
		this.m_CreateCopyCheckBox.TabIndex = 7;
		this.m_CreateCopyCheckBox.Text = "Create A Copy";
		this.m_CreateCopyCheckBox.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(493, 222);
		base.Controls.Add(this.m_CreateCopyCheckBox);
		base.Controls.Add(this.m_HostLineSelectionPanel);
		base.Controls.Add(this.m_NameLabel);
		base.Controls.Add(this.m_NameTextBox);
		base.Controls.Add(this.m_RadiusLabel);
		base.Controls.Add(this.m_RadiusTextBox);
		base.Controls.Add(this.m_MoveablePanel);
		base.Controls.Add(this.m_LoadFamilyButton);
		base.Controls.Add(this.m_TypeNameComboBox);
		base.Controls.Add(this.m_TypeNameLabel);
		base.Controls.Add(this.m_FamilyComboBox);
		base.Controls.Add(this.m_FamilyLabel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "LocateSiteParkingLotComponentForm";
		this.Text = "Locate Parking Lot";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnLocateSiteComponentClosing);
		this.m_MoveablePanel.ResumeLayout(false);
		this.m_HostLineSelectionPanel.ResumeLayout(false);
		this.m_HostLineSelectionPanel.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_InsertButton;
		base.CancelButton = this.m_CloseButton;
	}
}
