using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

namespace EPCommon;

public class AddBreaklineForm : EPForm
{
	private Units m_Units;

	private const UnitType m_LengthUnitType = UnitType.UT_Length;

	private const UnitType m_SlopeUnitType = UnitType.UT_Slope;

	private const UnitType m_AngularUnitType = UnitType.UT_Angle;

	private FormatOptions m_SlopeFormatOptions;

	private FormatOptions m_LengthFormatOptions;

	private DisplayUnit m_DisplayUnitSystem;

	private UnitSymbolType m_LengthUnitSymbolType;

	private UnitSymbolType m_SlopeUnitSymbolType;

	private Document m_Document;

	private DecimalSymbol m_DecimalSymbolType;

	private CurrentElevationFocus m_CurrentElevationFocus;

	private double m_dAdjustConstElevation;

	private IContainer components;

	private Label m_BeginningValueLabel;

	private TextBox m_BeginningElevationTextBox;

	private Button m_OKButton;

	private Button m_CancelButton;

	private Label m_EndingValueLabel;

	private TextBox m_EndingElevationTextBox;

	private RadioButton m_RelativeElevationRadioButton;

	private RadioButton m_ConstantElevationsRadioButton;

	private RadioButton m_GradeRadioButton;

	private TextBox m_GradeTextBox;

	private RadioButton m_ChainRadioButton;

	private RadioButton m_DisplayAndUseExistingHostLineRadioButton;

	private System.Windows.Forms.Panel m_HostLineSelectionPanel;

	private RadioButton m_DrawHostLineRadioButton;

	private System.Windows.Forms.Panel m_UsePanel;

	private RadioButton m_SegmentRadioButton;

	private Label m_RadiusLabel;

	private TextBox m_RadiusTextBox;

	private Button m_PickInRevitButton;

	private Label m_LinearIntervalLabel;

	private TextBox m_LinearIntervalTextBox;

	public double LinearInterval
	{
		get
		{
			double _dReturnedDouble = 0.0;
			if (m_LinearIntervalTextBox.Text != string.Empty)
			{
				RevitGeometry.Units_ConvertStringToDouble(m_LinearIntervalTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
			}
			return _dReturnedDouble;
		}
	}

	public double BeginningElevation
	{
		get
		{
			double _dReturnedDouble = 0.0;
			if (m_BeginningElevationTextBox.Text != string.Empty)
			{
				RevitGeometry.Units_ConvertStringToDouble(m_BeginningElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (!m_RelativeElevationRadioButton.Checked)
				{
					_dReturnedDouble -= m_dAdjustConstElevation;
				}
			}
			return _dReturnedDouble;
		}
	}

	public double EndingElevation
	{
		get
		{
			double _dReturnedDouble = 0.0;
			if (m_EndingElevationTextBox.Text != string.Empty)
			{
				RevitGeometry.Units_ConvertStringToDouble(m_EndingElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				_dReturnedDouble -= m_dAdjustConstElevation;
			}
			return _dReturnedDouble;
		}
	}

	public double Grade
	{
		get
		{
			double _dReturnedDouble = 0.0;
			if (m_GradeTextBox.Text != string.Empty)
			{
				RevitGeometry.Units_ConvertStringToDouble(m_GradeTextBox.Text, m_Units, UnitType.UT_Slope, ref _dReturnedDouble);
			}
			return _dReturnedDouble;
		}
	}

	public bool RelativeElevationChecked => m_RelativeElevationRadioButton.Checked;

	public bool ConstantElevationsChecked => m_ConstantElevationsRadioButton.Checked;

	public bool GradeChecked => m_GradeRadioButton.Checked;

	public bool ExistingHostLineChecked => m_DisplayAndUseExistingHostLineRadioButton.Checked;

	public double Radius
	{
		get
		{
			if (m_DisplayAndUseExistingHostLineRadioButton.Checked)
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

	public bool SegmentChecked => m_SegmentRadioButton.Checked;

	public bool ChainChecked => m_ChainRadioButton.Checked;

	public AddBreaklineForm(Document _Document)
	{
		InitializeComponent();
		m_Document = _Document;
		Text = EPShared.LocalRM.GetString("AddBreaklineDialog");
		m_LinearIntervalLabel.Text = EPShared.LocalRM.GetString("Linear_AMP_PointIntervalDialog");
		m_RelativeElevationRadioButton.Text = EPShared.LocalRM.GetString("Re_AMP_lativeElevationsDialog");
		m_ConstantElevationsRadioButton.Text = EPShared.LocalRM.GetString("Constant_AMP_ElevationsDialog");
		m_GradeRadioButton.Text = EPShared.LocalRM.GetString("AMP_GradeDialog");
		m_BeginningValueLabel.Text = EPShared.LocalRM.GetString("AMP_BeginningElevationDialog");
		m_SegmentRadioButton.Text = EPShared.LocalRM.GetString("AMP_SegmentDialog");
		m_ChainRadioButton.Text = EPShared.LocalRM.GetString("AMP_ChainDialog");
		m_DisplayAndUseExistingHostLineRadioButton.Text = EPShared.LocalRM.GetString("DisplayAnd_AMP_UseExistingHostLineDialog");
		m_DrawHostLineRadioButton.Text = EPShared.LocalRM.GetString("AMP_DrawHostLineDialog");
		m_RadiusLabel.Text = EPShared.LocalRM.GetString("AMP_RadiusDialog");
		m_OKButton.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		m_CancelButton.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		EPToolTip.SetToolTip(m_PickInRevitButton, EPShared.LocalRM.GetString("PickPointElevationToolTip"));
		m_DisplayUnitSystem = m_Document.DisplayUnitSystem;
		m_LengthFormatOptions = m_Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		m_Units = m_Document.GetUnits();
		try
		{
			m_LengthUnitSymbolType = m_LengthFormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_LengthUnitSymbolType = UnitSymbolType.UST_NONE;
		}
		m_SlopeFormatOptions = m_Document.GetUnits().GetFormatOptions(UnitType.UT_Slope);
		try
		{
			m_SlopeUnitSymbolType = m_SlopeFormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_SlopeUnitSymbolType = UnitSymbolType.UST_NONE;
		}
		m_DecimalSymbolType = m_Document.GetUnits().DecimalSymbol;
		m_dAdjustConstElevation = 0.0;
		InitializeControls();
	}

	private void InitializeControls()
	{
		int _nReturnValue = 0;
		double _dReturnValue = 0.0;
		string _strToDisplay = string.Empty;
		GetDialogPosition();
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("AddBreaklineLinearInterval", ref _dReturnValue))
		{
			_dReturnValue = 5.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_Units, ref _strToDisplay);
		m_LinearIntervalTextBox.Text = _strToDisplay;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("AddBreaklineBeginningElevation", ref _dReturnValue))
		{
			_dReturnValue = 0.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_Units, ref _strToDisplay);
		m_BeginningElevationTextBox.Text = _strToDisplay;
		m_RelativeElevationRadioButton.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("AddBreaklineElevationOption", ref _nReturnValue))
		{
			switch (_nReturnValue)
			{
			case 0:
				m_RelativeElevationRadioButton.Checked = true;
				break;
			case 1:
				m_ConstantElevationsRadioButton.Checked = true;
				break;
			default:
				m_GradeRadioButton.Checked = true;
				break;
			}
		}
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("AddBreaklineEndingElevation", ref _dReturnValue))
		{
			_dReturnValue = 0.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_Units, ref _strToDisplay);
		m_EndingElevationTextBox.Text = _strToDisplay;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("AddBreaklineGrade", ref _dReturnValue))
		{
			_dReturnValue = 0.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_SlopeFormatOptions.LegacyDisplayUnits(), m_SlopeFormatOptions.Accuracy, m_SlopeUnitSymbolType, m_Units, ref _strToDisplay);
		m_GradeTextBox.Text = _strToDisplay;
		m_DisplayAndUseExistingHostLineRadioButton.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("AddBreaklineHostLineSelectionOption", ref _nReturnValue))
		{
			if (_nReturnValue == 0)
			{
				m_DisplayAndUseExistingHostLineRadioButton.Checked = true;
			}
			else
			{
				m_DrawHostLineRadioButton.Checked = true;
			}
		}
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("AddBreaklineRadius", ref _dReturnValue))
		{
			_dReturnValue = 0.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_Units, ref _strToDisplay);
		m_RadiusTextBox.Text = _strToDisplay;
		m_ChainRadioButton.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("AddBreaklineSegmentChainOption", ref _nReturnValue))
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
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_Document.PathName, "UseSharedElevation", ref _nReturnValue) && _nReturnValue == 1)
		{
			ProjectLocation activeProjectLocation = m_Document.ActiveProjectLocation;
			XYZ point = new XYZ();
			ProjectPosition projectPosition = activeProjectLocation.GetProjectPosition(point);
			m_dAdjustConstElevation = projectPosition.Elevation;
		}
	}

	private void SaveSettings()
	{
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_LinearIntervalTextBox.Text, m_Document.GetUnits(), UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("AddBreaklineLinearInterval", _dReturnedDouble);
		}
		if (RevitGeometry.Units_ConvertStringToDouble(m_BeginningElevationTextBox.Text, m_Document.GetUnits(), UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("AddBreaklineBeginningElevation", _dReturnedDouble);
		}
		if (RevitGeometry.Units_ConvertStringToDouble(m_GradeTextBox.Text, m_Document.GetUnits(), UnitType.UT_Slope, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("AddBreaklineGrade", _dReturnedDouble);
		}
		if (RevitGeometry.Units_ConvertStringToDouble(m_EndingElevationTextBox.Text, m_Document.GetUnits(), UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("AddBreaklineEndingElevation", _dReturnedDouble);
		}
		int num = 0;
		num = ((!m_RelativeElevationRadioButton.Checked) ? (m_ConstantElevationsRadioButton.Checked ? 1 : 2) : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting("AddBreaklineElevationOption", num);
		num = ((!m_DisplayAndUseExistingHostLineRadioButton.Checked) ? 1 : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting("AddBreaklineHostLineSelectionOption", num);
		if (RevitGeometry.Units_ConvertStringToDouble(m_RadiusTextBox.Text, m_Document.GetUnits(), UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("AddBreaklineRadius", _dReturnedDouble);
		}
		num = ((!m_SegmentRadioButton.Checked) ? 1 : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting("AddBreaklineSegmentChainOption", num);
	}

	private void OnOKButtonClick(object sender, EventArgs e)
	{
		SaveSettings();
		DialogResult = DialogResult.OK;
		Close();
	}

	private void OnCancelClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void OnElevationGradeOptionChecked(object sender, EventArgs e)
	{
		m_EndingValueLabel.Enabled = !m_RelativeElevationRadioButton.Checked;
		m_EndingElevationTextBox.Enabled = m_ConstantElevationsRadioButton.Checked;
		m_EndingElevationTextBox.Visible = !m_GradeRadioButton.Checked;
		m_GradeTextBox.Visible = m_GradeRadioButton.Checked;
		m_GradeTextBox.Enabled = m_GradeRadioButton.Checked;
		if (m_GradeRadioButton.Checked)
		{
			m_EndingValueLabel.Text = EPShared.LocalRM.GetString("AMP_GradeDialog") + ":";
		}
		else
		{
			m_EndingValueLabel.Text = EPShared.LocalRM.GetString("EndingEle_AMP_vationDialog");
		}
	}

	private void OnAddBreaklineFormClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private void FormatLinearIntervalValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_LinearIntervalTextBox.Text, m_Document.GetUnits(), UnitType.UT_Length, ref _dReturnedDouble))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_Units, ref _strToDisplay);
			m_LinearIntervalTextBox.Text = _strToDisplay;
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_LinearIntervalTextBox);
		}
	}

	private void OnLinearIntervalKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatLinearIntervalValue();
		}
	}

	private void OnLinearIntervalTextBoxLeave(object sender, EventArgs e)
	{
		FormatLinearIntervalValue();
	}

	private void FormatBeginningElevationValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_BeginningElevationTextBox.Text, m_Document.GetUnits(), UnitType.UT_Length, ref _dReturnedDouble))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_Units, ref _strToDisplay);
			m_BeginningElevationTextBox.Text = _strToDisplay;
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_BeginningElevationTextBox);
		}
	}

	private void FormatEndingElevationValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_EndingElevationTextBox.Text, m_Document.GetUnits(), UnitType.UT_Length, ref _dReturnedDouble))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_Units, ref _strToDisplay);
			m_EndingElevationTextBox.Text = _strToDisplay;
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_EndingElevationTextBox);
		}
	}

	private void FormatGradeValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_GradeTextBox.Text, m_Document.GetUnits(), UnitType.UT_Slope, ref _dReturnedDouble))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_SlopeFormatOptions.LegacyDisplayUnits(), m_SlopeFormatOptions.Accuracy, m_SlopeUnitSymbolType, m_Units, ref _strToDisplay);
			m_GradeTextBox.Text = _strToDisplay;
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_GradeTextBox);
		}
	}

	private void OnBeginningElevationTextBoxLeave(object sender, EventArgs e)
	{
		FormatBeginningElevationValue();
		m_CurrentElevationFocus = CurrentElevationFocus.BeginningElevationRange;
	}

	private void OnBeginningElevationTextBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatBeginningElevationValue();
		}
	}

	private void OnEndingElevationTextBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatEndingElevationValue();
		}
	}

	private void OnEndingElevationTextBoxLeave(object sender, EventArgs e)
	{
		FormatEndingElevationValue();
		m_CurrentElevationFocus = CurrentElevationFocus.EndingElevationRange;
	}

	private void OnGradeTextBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatGradeValue();
		}
	}

	private void OnGradeTextBoxLeave(object sender, EventArgs e)
	{
		FormatGradeValue();
	}

	private void OnHostLineSelectionOptionChecked(object sender, EventArgs e)
	{
		m_RadiusLabel.Enabled = m_DrawHostLineRadioButton.Checked;
		m_RadiusTextBox.Enabled = m_DrawHostLineRadioButton.Checked;
	}

	private void OnPickInRevitButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Retry;
		Close();
	}

	public void SetFocusedElevation(double _dElevation)
	{
		string _strToDisplay = string.Empty;
		_dElevation += m_dAdjustConstElevation;
		RevitGeometry.Units_ConvertDoubleToString(_dElevation, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_Units, ref _strToDisplay);
		switch (m_CurrentElevationFocus)
		{
		case CurrentElevationFocus.BeginningElevationRange:
			m_BeginningElevationTextBox.Text = _strToDisplay;
			m_PickInRevitButton.Enabled = true;
			break;
		case CurrentElevationFocus.EndingElevationRange:
			m_EndingElevationTextBox.Text = _strToDisplay;
			m_PickInRevitButton.Enabled = true;
			break;
		}
	}

	private void OnAddBreaklineFormShown(object sender, EventArgs e)
	{
		switch (m_CurrentElevationFocus)
		{
		case CurrentElevationFocus.None:
			m_PickInRevitButton.Enabled = false;
			break;
		case CurrentElevationFocus.BeginningElevationRange:
			SiteDesigner.UI.WpfFocus.Focus(m_BeginningElevationTextBox);
			m_PickInRevitButton.Enabled = true;
			break;
		case CurrentElevationFocus.EndingElevationRange:
			SiteDesigner.UI.WpfFocus.Focus(m_EndingElevationTextBox);
			m_PickInRevitButton.Enabled = true;
			break;
		case CurrentElevationFocus.ConstantElevation:
			break;
		}
	}

	private void OnElevationTextBoxEnter(object sender, EventArgs e)
	{
		m_PickInRevitButton.Enabled = !m_RelativeElevationRadioButton.Checked;
	}

	private void OnElevationTextBoxLostFocus(object sender, EventArgs e)
	{
		if (!SiteDesigner.UI.WpfFocus.HasFocus(m_PickInRevitButton) && !SiteDesigner.UI.WpfFocus.HasFocus(m_BeginningElevationTextBox) && !SiteDesigner.UI.WpfFocus.HasFocus(m_EndingElevationTextBox))
		{
			m_PickInRevitButton.Enabled = false;
		}
	}

	public double GetGrade(double _dLegListLength)
	{
		double _dReturnedDouble = 0.0;
		if (m_ConstantElevationsRadioButton.Checked)
		{
			if (m_EndingElevationTextBox.Text != string.Empty)
			{
				RevitGeometry.Units_ConvertStringToDouble(m_EndingElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				double num = _dReturnedDouble - m_dAdjustConstElevation - BeginningElevation;
				_dReturnedDouble = num / _dLegListLength;
			}
		}
		else if (m_GradeRadioButton.Checked && m_GradeTextBox.Text != string.Empty)
		{
			RevitGeometry.Units_ConvertStringToDouble(m_GradeTextBox.Text, m_Units, UnitType.UT_Slope, ref _dReturnedDouble);
		}
		return _dReturnedDouble;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EPCommon.AddBreaklineForm));
		this.m_BeginningValueLabel = new System.Windows.Forms.Label();
		this.m_BeginningElevationTextBox = new System.Windows.Forms.TextBox();
		this.m_OKButton = new System.Windows.Forms.Button();
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_EndingValueLabel = new System.Windows.Forms.Label();
		this.m_EndingElevationTextBox = new System.Windows.Forms.TextBox();
		this.m_RelativeElevationRadioButton = new System.Windows.Forms.RadioButton();
		this.m_ConstantElevationsRadioButton = new System.Windows.Forms.RadioButton();
		this.m_GradeRadioButton = new System.Windows.Forms.RadioButton();
		this.m_GradeTextBox = new System.Windows.Forms.TextBox();
		this.m_ChainRadioButton = new System.Windows.Forms.RadioButton();
		this.m_DisplayAndUseExistingHostLineRadioButton = new System.Windows.Forms.RadioButton();
		this.m_HostLineSelectionPanel = new System.Windows.Forms.Panel();
		this.m_DrawHostLineRadioButton = new System.Windows.Forms.RadioButton();
		this.m_UsePanel = new System.Windows.Forms.Panel();
		this.m_SegmentRadioButton = new System.Windows.Forms.RadioButton();
		this.m_RadiusLabel = new System.Windows.Forms.Label();
		this.m_RadiusTextBox = new System.Windows.Forms.TextBox();
		this.m_PickInRevitButton = new System.Windows.Forms.Button();
		this.m_LinearIntervalLabel = new System.Windows.Forms.Label();
		this.m_LinearIntervalTextBox = new System.Windows.Forms.TextBox();
		this.m_HostLineSelectionPanel.SuspendLayout();
		this.m_UsePanel.SuspendLayout();
		base.SuspendLayout();
		this.m_BeginningValueLabel.AutoSize = true;
		this.m_BeginningValueLabel.Location = new System.Drawing.Point(10, 110);
		this.m_BeginningValueLabel.Name = "m_BeginningValueLabel";
		this.m_BeginningValueLabel.Size = new System.Drawing.Size(104, 13);
		this.m_BeginningValueLabel.TabIndex = 6;
		this.m_BeginningValueLabel.Text = "Beginning Elevation:";
		this.m_BeginningElevationTextBox.Location = new System.Drawing.Point(171, 107);
		this.m_BeginningElevationTextBox.Name = "m_BeginningElevationTextBox";
		this.m_BeginningElevationTextBox.Size = new System.Drawing.Size(176, 20);
		this.m_BeginningElevationTextBox.TabIndex = 7;
		this.m_BeginningElevationTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnBeginningElevationTextBoxKeyDown);
		this.m_BeginningElevationTextBox.Leave += new System.EventHandler(this.OnBeginningElevationTextBoxLeave);
		this.m_BeginningElevationTextBox.Enter += new System.EventHandler(this.OnElevationTextBoxEnter);
		this.m_BeginningElevationTextBox.LostFocus += new System.EventHandler(this.OnElevationTextBoxLostFocus);
		this.m_OKButton.Location = new System.Drawing.Point(191, 255);
		this.m_OKButton.Name = "m_OKButton";
		this.m_OKButton.Size = new System.Drawing.Size(75, 23);
		this.m_OKButton.TabIndex = 15;
		this.m_OKButton.Text = "&OK";
		this.m_OKButton.UseVisualStyleBackColor = true;
		this.m_OKButton.Click += new System.EventHandler(this.OnOKButtonClick);
		this.m_CancelButton.Location = new System.Drawing.Point(272, 255);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 16;
		this.m_CancelButton.Text = "&Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelClick);
		this.m_EndingValueLabel.AutoSize = true;
		this.m_EndingValueLabel.Enabled = false;
		this.m_EndingValueLabel.Location = new System.Drawing.Point(10, 136);
		this.m_EndingValueLabel.Name = "m_EndingValueLabel";
		this.m_EndingValueLabel.Size = new System.Drawing.Size(90, 13);
		this.m_EndingValueLabel.TabIndex = 8;
		this.m_EndingValueLabel.Text = "Ending Elevation:";
		this.m_EndingElevationTextBox.Location = new System.Drawing.Point(171, 133);
		this.m_EndingElevationTextBox.Name = "m_EndingElevationTextBox";
		this.m_EndingElevationTextBox.Size = new System.Drawing.Size(176, 20);
		this.m_EndingElevationTextBox.TabIndex = 7;
		this.m_EndingElevationTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnEndingElevationTextBoxKeyDown);
		this.m_EndingElevationTextBox.Leave += new System.EventHandler(this.OnEndingElevationTextBoxLeave);
		this.m_EndingElevationTextBox.Enter += new System.EventHandler(this.OnElevationTextBoxEnter);
		this.m_EndingElevationTextBox.LostFocus += new System.EventHandler(this.OnElevationTextBoxLostFocus);
		this.m_RelativeElevationRadioButton.AutoSize = true;
		this.m_RelativeElevationRadioButton.Checked = true;
		this.m_RelativeElevationRadioButton.Location = new System.Drawing.Point(13, 38);
		this.m_RelativeElevationRadioButton.Name = "m_RelativeElevationRadioButton";
		this.m_RelativeElevationRadioButton.Size = new System.Drawing.Size(111, 17);
		this.m_RelativeElevationRadioButton.TabIndex = 3;
		this.m_RelativeElevationRadioButton.TabStop = true;
		this.m_RelativeElevationRadioButton.Text = "Relative Elevation";
		this.m_RelativeElevationRadioButton.UseVisualStyleBackColor = true;
		this.m_RelativeElevationRadioButton.CheckedChanged += new System.EventHandler(this.OnElevationGradeOptionChecked);
		this.m_ConstantElevationsRadioButton.AutoSize = true;
		this.m_ConstantElevationsRadioButton.Location = new System.Drawing.Point(13, 61);
		this.m_ConstantElevationsRadioButton.Name = "m_ConstantElevationsRadioButton";
		this.m_ConstantElevationsRadioButton.Size = new System.Drawing.Size(119, 17);
		this.m_ConstantElevationsRadioButton.TabIndex = 4;
		this.m_ConstantElevationsRadioButton.Text = "Constant Elevations";
		this.m_ConstantElevationsRadioButton.UseVisualStyleBackColor = true;
		this.m_ConstantElevationsRadioButton.CheckedChanged += new System.EventHandler(this.OnElevationGradeOptionChecked);
		this.m_GradeRadioButton.AutoSize = true;
		this.m_GradeRadioButton.Location = new System.Drawing.Point(13, 84);
		this.m_GradeRadioButton.Name = "m_GradeRadioButton";
		this.m_GradeRadioButton.Size = new System.Drawing.Size(54, 17);
		this.m_GradeRadioButton.TabIndex = 5;
		this.m_GradeRadioButton.Text = "Grade";
		this.m_GradeRadioButton.UseVisualStyleBackColor = true;
		this.m_GradeRadioButton.CheckedChanged += new System.EventHandler(this.OnElevationGradeOptionChecked);
		this.m_GradeTextBox.Enabled = false;
		this.m_GradeTextBox.Location = new System.Drawing.Point(171, 133);
		this.m_GradeTextBox.Name = "m_GradeTextBox";
		this.m_GradeTextBox.Size = new System.Drawing.Size(176, 20);
		this.m_GradeTextBox.TabIndex = 10;
		this.m_GradeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnGradeTextBoxKeyDown);
		this.m_GradeTextBox.Leave += new System.EventHandler(this.OnGradeTextBoxLeave);
		this.m_ChainRadioButton.AutoSize = true;
		this.m_ChainRadioButton.Checked = true;
		this.m_ChainRadioButton.Location = new System.Drawing.Point(158, 0);
		this.m_ChainRadioButton.Name = "m_ChainRadioButton";
		this.m_ChainRadioButton.Size = new System.Drawing.Size(52, 17);
		this.m_ChainRadioButton.TabIndex = 1;
		this.m_ChainRadioButton.TabStop = true;
		this.m_ChainRadioButton.Text = "&Chain";
		this.m_ChainRadioButton.UseVisualStyleBackColor = true;
		this.m_DisplayAndUseExistingHostLineRadioButton.AutoSize = true;
		this.m_DisplayAndUseExistingHostLineRadioButton.Location = new System.Drawing.Point(0, 0);
		this.m_DisplayAndUseExistingHostLineRadioButton.Name = "m_DisplayAndUseExistingHostLineRadioButton";
		this.m_DisplayAndUseExistingHostLineRadioButton.Size = new System.Drawing.Size(204, 17);
		this.m_DisplayAndUseExistingHostLineRadioButton.TabIndex = 0;
		this.m_DisplayAndUseExistingHostLineRadioButton.Text = "Display and &Use an Existing Host Line";
		this.m_DisplayAndUseExistingHostLineRadioButton.UseVisualStyleBackColor = true;
		this.m_DisplayAndUseExistingHostLineRadioButton.CheckedChanged += new System.EventHandler(this.OnHostLineSelectionOptionChecked);
		this.m_HostLineSelectionPanel.Controls.Add(this.m_DisplayAndUseExistingHostLineRadioButton);
		this.m_HostLineSelectionPanel.Controls.Add(this.m_DrawHostLineRadioButton);
		this.m_HostLineSelectionPanel.Location = new System.Drawing.Point(12, 182);
		this.m_HostLineSelectionPanel.Name = "m_HostLineSelectionPanel";
		this.m_HostLineSelectionPanel.Size = new System.Drawing.Size(334, 41);
		this.m_HostLineSelectionPanel.TabIndex = 12;
		this.m_DrawHostLineRadioButton.AutoSize = true;
		this.m_DrawHostLineRadioButton.Location = new System.Drawing.Point(0, 23);
		this.m_DrawHostLineRadioButton.Name = "m_DrawHostLineRadioButton";
		this.m_DrawHostLineRadioButton.Size = new System.Drawing.Size(228, 17);
		this.m_DrawHostLineRadioButton.TabIndex = 1;
		this.m_DrawHostLineRadioButton.Text = "&Draw Host Line (Click Escape to Complete)";
		this.m_DrawHostLineRadioButton.UseVisualStyleBackColor = true;
		this.m_DrawHostLineRadioButton.CheckedChanged += new System.EventHandler(this.OnHostLineSelectionOptionChecked);
		this.m_UsePanel.Controls.Add(this.m_SegmentRadioButton);
		this.m_UsePanel.Controls.Add(this.m_ChainRadioButton);
		this.m_UsePanel.Location = new System.Drawing.Point(13, 159);
		this.m_UsePanel.Name = "m_UsePanel";
		this.m_UsePanel.Size = new System.Drawing.Size(333, 18);
		this.m_UsePanel.TabIndex = 11;
		this.m_SegmentRadioButton.AutoSize = true;
		this.m_SegmentRadioButton.Location = new System.Drawing.Point(-1, 0);
		this.m_SegmentRadioButton.Name = "m_SegmentRadioButton";
		this.m_SegmentRadioButton.Size = new System.Drawing.Size(67, 17);
		this.m_SegmentRadioButton.TabIndex = 0;
		this.m_SegmentRadioButton.Text = "&Segment";
		this.m_SegmentRadioButton.UseVisualStyleBackColor = true;
		this.m_RadiusLabel.AutoSize = true;
		this.m_RadiusLabel.Enabled = false;
		this.m_RadiusLabel.Location = new System.Drawing.Point(12, 232);
		this.m_RadiusLabel.Name = "m_RadiusLabel";
		this.m_RadiusLabel.Size = new System.Drawing.Size(43, 13);
		this.m_RadiusLabel.TabIndex = 13;
		this.m_RadiusLabel.Text = "Radius:";
		this.m_RadiusTextBox.Enabled = false;
		this.m_RadiusTextBox.Location = new System.Drawing.Point(171, 229);
		this.m_RadiusTextBox.Name = "m_RadiusTextBox";
		this.m_RadiusTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_RadiusTextBox.TabIndex = 14;
		this.m_PickInRevitButton.Enabled = false;
		this.m_PickInRevitButton.Image = (System.Drawing.Image)EPCommon.SafeResources.GetObject(resources, "m_PickInRevitButton.Image");
		this.m_PickInRevitButton.Location = new System.Drawing.Point(323, 12);
		this.m_PickInRevitButton.Name = "m_PickInRevitButton";
		this.m_PickInRevitButton.Size = new System.Drawing.Size(24, 20);
		this.m_PickInRevitButton.TabIndex = 2;
		this.m_PickInRevitButton.TabStop = false;
		this.m_PickInRevitButton.UseVisualStyleBackColor = true;
		this.m_PickInRevitButton.Click += new System.EventHandler(this.OnPickInRevitButtonClick);
		this.m_LinearIntervalLabel.AutoSize = true;
		this.m_LinearIntervalLabel.Location = new System.Drawing.Point(12, 15);
		this.m_LinearIntervalLabel.Name = "m_LinearIntervalLabel";
		this.m_LinearIntervalLabel.Size = new System.Drawing.Size(104, 13);
		this.m_LinearIntervalLabel.TabIndex = 0;
		this.m_LinearIntervalLabel.Text = "Linear Point Interval:";
		this.m_LinearIntervalTextBox.Location = new System.Drawing.Point(171, 12);
		this.m_LinearIntervalTextBox.Name = "m_LinearIntervalTextBox";
		this.m_LinearIntervalTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_LinearIntervalTextBox.TabIndex = 1;
		this.m_LinearIntervalTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnLinearIntervalKeyDown);
		this.m_LinearIntervalTextBox.Leave += new System.EventHandler(this.OnLinearIntervalTextBoxLeave);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(359, 290);
		base.Controls.Add(this.m_LinearIntervalLabel);
		base.Controls.Add(this.m_LinearIntervalTextBox);
		base.Controls.Add(this.m_PickInRevitButton);
		base.Controls.Add(this.m_HostLineSelectionPanel);
		base.Controls.Add(this.m_UsePanel);
		base.Controls.Add(this.m_RadiusLabel);
		base.Controls.Add(this.m_RadiusTextBox);
		base.Controls.Add(this.m_GradeTextBox);
		base.Controls.Add(this.m_GradeRadioButton);
		base.Controls.Add(this.m_ConstantElevationsRadioButton);
		base.Controls.Add(this.m_RelativeElevationRadioButton);
		base.Controls.Add(this.m_EndingValueLabel);
		base.Controls.Add(this.m_EndingElevationTextBox);
		base.Controls.Add(this.m_BeginningValueLabel);
		base.Controls.Add(this.m_BeginningElevationTextBox);
		base.Controls.Add(this.m_OKButton);
		base.Controls.Add(this.m_CancelButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "AddBreaklineForm";
		this.Text = "AddBreaklineForm";
		base.Shown += new System.EventHandler(this.OnAddBreaklineFormShown);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnAddBreaklineFormClosing);
		this.m_HostLineSelectionPanel.ResumeLayout(false);
		this.m_HostLineSelectionPanel.PerformLayout();
		this.m_UsePanel.ResumeLayout(false);
		this.m_UsePanel.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_OKButton;
		base.CancelButton = this.m_CancelButton;
	}
}
