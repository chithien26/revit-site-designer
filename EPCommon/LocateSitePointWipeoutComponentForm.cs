using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

namespace EPCommon;

public class LocateSitePointWipeoutComponentForm : EPForm
{
	private Units m_Units;

	private const UnitType m_LengthUnitType = UnitType.UT_Length;

	private const UnitType m_SlopeUnitType = UnitType.UT_Slope;

	private const UnitType m_AngularUnitType = UnitType.UT_Angle;

	private SiteComponentCreation m_dataBuffer;

	private FormatOptions m_FormatOptions;

	private DisplayUnit m_DisplayUnitSystem;

	private UnitSymbolType m_UnitSymbolType;

	private DecimalSymbol m_DecimalSymbolType;

	private IContainer components;

	private CheckBox m_CreateCopyCheckBox;

	private System.Windows.Forms.Panel m_HostLineSelectionPanel;

	private RadioButton m_UseExistingHostLineRadioButton;

	private RadioButton m_DrawHostLineRadioButton;

	private Label m_RadiusLabel;

	private TextBox m_RadiusTextBox;

	private System.Windows.Forms.Panel m_MoveablePanel;

	private Button m_CloseButton;

	private Button m_InsertButton;

	private CheckBox m_ShowAffectedPointsCheckBox;

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

	public bool ShowAffectedPointsChecked => m_ShowAffectedPointsCheckBox.Checked;

	public LocateSitePointWipeoutComponentForm(SiteComponentCreation dataBuffer)
	{
		InitializeComponent();
		SetStrings();
		ContextualHelpID = "HID_SDpointWipeoutCreate";
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
		Text = EPShared.LocalRM.GetString("LocatePointWipeoutDialog");
		m_CreateCopyCheckBox.Text = EPShared.LocalRM.GetString("Create_AMP_ACopyDialog");
		m_UseExistingHostLineRadioButton.Text = EPShared.LocalRM.GetString("AMP_UseExistingHostLineDialog");
		m_DrawHostLineRadioButton.Text = EPShared.LocalRM.GetString("AMP_DrawHostLineDialog");
		m_RadiusLabel.Text = EPShared.LocalRM.GetString("AMP_RadiusDialog");
		m_ShowAffectedPointsCheckBox.Text = EPShared.LocalRM.GetString("S_AMP_howAffectedPointsDialog");
		m_InsertButton.Text = EPShared.LocalRM.GetString("AMP_InsertDialog");
		m_CloseButton.Text = EPShared.LocalRM.GetString("AMP_CloseDialog");
	}

	private void OnHostLineSelectionOptionChecked(object sender, EventArgs e)
	{
		m_RadiusLabel.Enabled = m_DrawHostLineRadioButton.Checked;
		m_RadiusTextBox.Enabled = m_DrawHostLineRadioButton.Checked;
	}

	private void InitializeControls()
	{
		int _nReturnValue = 0;
		double _dReturnValue = 0.0;
		string _strToDisplay = string.Empty;
		GetDialogPosition();
		m_UseExistingHostLineRadioButton.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("LocatePointWipeoutHostLineSelectionOption", ref _nReturnValue))
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
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("LocatePointWipeoutRadius", ref _dReturnValue))
		{
			_dReturnValue = 0.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
		m_RadiusTextBox.Text = _strToDisplay;
		m_CreateCopyCheckBox.Checked = false;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("LocatePointWipeoutCreateVisibleCopy", ref _nReturnValue))
		{
			m_CreateCopyCheckBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
		m_ShowAffectedPointsCheckBox.Checked = false;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("LocatePointWipeoutShowAffectedPoints", ref _nReturnValue))
		{
			m_ShowAffectedPointsCheckBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
	}

	private void SaveSettings()
	{
		int num = 0;
		double _dReturnedDouble = 0.0;
		num = ((!m_UseExistingHostLineRadioButton.Checked) ? 1 : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting("LocatePointWipeoutHostLineSelectionOption", num);
		if (RevitGeometry.Units_ConvertStringToDouble(m_RadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("LocatePointWipeoutRadius", _dReturnedDouble);
		}
		ApplicationUtility.Settings.eplSetE_SetSetting("LocatePointWipeoutCreateVisibleCopy", Convert.ToInt32(m_CreateCopyCheckBox.Checked));
		ApplicationUtility.Settings.eplSetE_SetSetting("LocatePointWipeoutShowAffectedPoints", Convert.ToInt32(m_ShowAffectedPointsCheckBox.Checked));
	}

	private void OnOKButtonClick(object sender, EventArgs e)
	{
		SaveSettings();
		DialogResult = DialogResult.OK;
		Close();
	}

	private void OnCloseButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
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

	private void OnLocateSitePointWipeoutComponentClosing(object sender, FormClosingEventArgs e)
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
		this.m_CreateCopyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_HostLineSelectionPanel = new System.Windows.Forms.Panel();
		this.m_UseExistingHostLineRadioButton = new System.Windows.Forms.RadioButton();
		this.m_DrawHostLineRadioButton = new System.Windows.Forms.RadioButton();
		this.m_RadiusLabel = new System.Windows.Forms.Label();
		this.m_RadiusTextBox = new System.Windows.Forms.TextBox();
		this.m_MoveablePanel = new System.Windows.Forms.Panel();
		this.m_CloseButton = new System.Windows.Forms.Button();
		this.m_InsertButton = new System.Windows.Forms.Button();
		this.m_ShowAffectedPointsCheckBox = new System.Windows.Forms.CheckBox();
		this.m_HostLineSelectionPanel.SuspendLayout();
		this.m_MoveablePanel.SuspendLayout();
		base.SuspendLayout();
		this.m_CreateCopyCheckBox.AutoSize = true;
		this.m_CreateCopyCheckBox.Location = new System.Drawing.Point(12, 12);
		this.m_CreateCopyCheckBox.Name = "m_CreateCopyCheckBox";
		this.m_CreateCopyCheckBox.Size = new System.Drawing.Size(94, 17);
		this.m_CreateCopyCheckBox.TabIndex = 0;
		this.m_CreateCopyCheckBox.Text = "Create A Copy";
		this.m_CreateCopyCheckBox.UseVisualStyleBackColor = true;
		this.m_HostLineSelectionPanel.Controls.Add(this.m_UseExistingHostLineRadioButton);
		this.m_HostLineSelectionPanel.Controls.Add(this.m_DrawHostLineRadioButton);
		this.m_HostLineSelectionPanel.Location = new System.Drawing.Point(12, 35);
		this.m_HostLineSelectionPanel.Name = "m_HostLineSelectionPanel";
		this.m_HostLineSelectionPanel.Size = new System.Drawing.Size(304, 41);
		this.m_HostLineSelectionPanel.TabIndex = 1;
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
		this.m_RadiusLabel.AutoSize = true;
		this.m_RadiusLabel.Enabled = false;
		this.m_RadiusLabel.Location = new System.Drawing.Point(27, 84);
		this.m_RadiusLabel.Name = "m_RadiusLabel";
		this.m_RadiusLabel.Size = new System.Drawing.Size(43, 13);
		this.m_RadiusLabel.TabIndex = 2;
		this.m_RadiusLabel.Text = "Radius:";
		this.m_RadiusTextBox.Enabled = false;
		this.m_RadiusTextBox.Location = new System.Drawing.Point(216, 81);
		this.m_RadiusTextBox.Name = "m_RadiusTextBox";
		this.m_RadiusTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_RadiusTextBox.TabIndex = 3;
		this.m_RadiusTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnRadiusKeyDown);
		this.m_RadiusTextBox.Leave += new System.EventHandler(this.OnRadiusTextBoxLeave);
		this.m_MoveablePanel.Controls.Add(this.m_CloseButton);
		this.m_MoveablePanel.Controls.Add(this.m_InsertButton);
		this.m_MoveablePanel.Location = new System.Drawing.Point(9, 130);
		this.m_MoveablePanel.Name = "m_MoveablePanel";
		this.m_MoveablePanel.Size = new System.Drawing.Size(307, 23);
		this.m_MoveablePanel.TabIndex = 5;
		this.m_CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_CloseButton.Location = new System.Drawing.Point(231, 0);
		this.m_CloseButton.Name = "m_CloseButton";
		this.m_CloseButton.Size = new System.Drawing.Size(76, 23);
		this.m_CloseButton.TabIndex = 1;
		this.m_CloseButton.Text = "&Cancel";
		this.m_CloseButton.UseVisualStyleBackColor = true;
		this.m_CloseButton.Click += new System.EventHandler(this.OnCloseButtonClick);
		this.m_InsertButton.Location = new System.Drawing.Point(150, 0);
		this.m_InsertButton.Name = "m_InsertButton";
		this.m_InsertButton.Size = new System.Drawing.Size(75, 23);
		this.m_InsertButton.TabIndex = 0;
		this.m_InsertButton.Text = "&Insert";
		this.m_InsertButton.UseVisualStyleBackColor = true;
		this.m_InsertButton.Click += new System.EventHandler(this.OnOKButtonClick);
		this.m_ShowAffectedPointsCheckBox.AutoSize = true;
		this.m_ShowAffectedPointsCheckBox.Location = new System.Drawing.Point(12, 107);
		this.m_ShowAffectedPointsCheckBox.Name = "m_ShowAffectedPointsCheckBox";
		this.m_ShowAffectedPointsCheckBox.Size = new System.Drawing.Size(134, 17);
		this.m_ShowAffectedPointsCheckBox.TabIndex = 4;
		this.m_ShowAffectedPointsCheckBox.Text = "Show Affected Point(s)";
		this.m_ShowAffectedPointsCheckBox.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(328, 165);
		base.Controls.Add(this.m_ShowAffectedPointsCheckBox);
		base.Controls.Add(this.m_CreateCopyCheckBox);
		base.Controls.Add(this.m_HostLineSelectionPanel);
		base.Controls.Add(this.m_RadiusLabel);
		base.Controls.Add(this.m_RadiusTextBox);
		base.Controls.Add(this.m_MoveablePanel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "LocateSitePointWipeoutComponentForm";
		this.Text = "Locate Point Wipeout";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnLocateSitePointWipeoutComponentClosing);
		this.m_HostLineSelectionPanel.ResumeLayout(false);
		this.m_HostLineSelectionPanel.PerformLayout();
		this.m_MoveablePanel.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_InsertButton;
		base.CancelButton = this.m_CloseButton;
	}
}
