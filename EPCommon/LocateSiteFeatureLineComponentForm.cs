using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

namespace EPCommon;

public class LocateSiteFeatureLineComponentForm : EPForm
{
	private Units m_Units;

	private const UnitType m_LengthUnitType = UnitType.UT_Length;

	private const UnitType m_SlopeUnitType = UnitType.UT_Slope;

	private const UnitType m_AngularUnitType = UnitType.UT_Angle;

	private SiteComponentCreation m_dataBuffer;

	private DisplayUnit m_DisplayUnitSystem;

	private FormatOptions m_LengthFormatOptions;

	private FormatOptions m_SlopeFormatOptions;

	private UnitSymbolType m_LengthUnitSymbolType;

	private UnitSymbolType m_SlopeUnitSymbolType;

	private DecimalSymbol m_DecimalSymbolType;

	private CurrentElevationFocus m_CurrentElevationFocus;

	private double m_dAdjustConstElevation;

	private IContainer components;

	private Label m_BeginningElevationRangeLabel;

	private TextBox m_BeginningElevationRangeTextBox;

	private System.Windows.Forms.Panel m_MoveablePanel;

	private CheckBox m_CreateCopyCheckBox;

	private Button m_CloseButton;

	private Button m_InsertButton;

	private Label m_RadiusLabel;

	private TextBox m_RadiusTextBox;

	private RadioButton m_RelativeElevationRadioButton;

	private Label m_RelativeElevationLabel;

	private TextBox m_RelativeElevationTextBox;

	private RadioButton m_ElevationRangeRadioButton;

	private System.Windows.Forms.Panel m_UsePanel;

	private RadioButton m_SegmentRadioButton;

	private RadioButton m_ChainRadioButton;

	private System.Windows.Forms.Panel m_HostLineSelectionPanel;

	private RadioButton m_UseExistingHostLineRadioButton;

	private RadioButton m_DrawHostLineRadioButton;

	private Label m_EndingElevationRangeLabel;

	private TextBox m_EndingElevationRangeTextBox;

	private Label m_GradeLabel;

	private TextBox m_GradeTextBox;

	private RadioButton m_GradeRadioButton;

	private Label m_GradeBeginningElevationLabel;

	private TextBox m_GradeBeginningElevationTextBox;

	private Label m_LinearIntervalLabel;

	private TextBox m_LinearIntervalTextBox;

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
				RevitGeometry.Units_ConvertStringToDouble(m_RadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
			}
			return _dReturnedDouble;
		}
	}

	public double Elevation
	{
		get
		{
			double _dReturnedDouble = 0.0;
			if (m_RelativeElevationRadioButton.Checked)
			{
				if (m_RelativeElevationTextBox.Text != string.Empty)
				{
					RevitGeometry.Units_ConvertStringToDouble(m_RelativeElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				}
			}
			else if (m_ElevationRangeRadioButton.Checked)
			{
				if (m_BeginningElevationRangeTextBox.Text != string.Empty)
				{
					RevitGeometry.Units_ConvertStringToDouble(m_BeginningElevationRangeTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
					_dReturnedDouble -= m_dAdjustConstElevation;
				}
			}
			else if (m_GradeBeginningElevationTextBox.Text != string.Empty)
			{
				RevitGeometry.Units_ConvertStringToDouble(m_GradeBeginningElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				_dReturnedDouble -= m_dAdjustConstElevation;
			}
			return _dReturnedDouble;
		}
	}

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

	public bool SegmentChecked => m_SegmentRadioButton.Checked;

	public bool ChainChecked => m_ChainRadioButton.Checked;

	public bool RelativeElevationChecked => m_RelativeElevationRadioButton.Checked;

	public LocateSiteFeatureLineComponentForm(SiteComponentCreation dataBuffer)
	{
		InitializeComponent();
		SetStrings();
		EPToolTip.SetToolTip(m_PickInRevitButton, EPShared.LocalRM.GetString("PickPointElevationToolTip"));
		ContextualHelpID = "HID_SDfeatureLineCreate";
		m_dataBuffer = dataBuffer;
		m_LengthFormatOptions = m_dataBuffer.Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		m_DisplayUnitSystem = m_dataBuffer.Document.DisplayUnitSystem;
		m_Units = m_dataBuffer.Document.GetUnits();
		try
		{
			m_LengthUnitSymbolType = m_LengthFormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_LengthUnitSymbolType = UnitSymbolType.UST_NONE;
		}
		m_SlopeFormatOptions = m_dataBuffer.Document.GetUnits().GetFormatOptions(UnitType.UT_Slope);
		try
		{
			m_SlopeUnitSymbolType = m_SlopeFormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_SlopeUnitSymbolType = UnitSymbolType.UST_NONE;
		}
		m_DecimalSymbolType = m_dataBuffer.Document.GetUnits().DecimalSymbol;
		m_dAdjustConstElevation = 0.0;
		InitializeControls();
	}

	private void SetStrings()
	{
		Text = EPShared.LocalRM.GetString("LocateFeatureLineDialog");
		m_LinearIntervalLabel.Text = EPShared.LocalRM.GetString("Linear_AMP_PointIntervalDialog");
		m_RelativeElevationRadioButton.Text = EPShared.LocalRM.GetString("Re_AMP_lativeElevationFromToposurfaceDialog");
		m_RelativeElevationLabel.Text = EPShared.LocalRM.GetString("AMP_ElevationDialog");
		m_ElevationRangeRadioButton.Text = EPShared.LocalRM.GetString("Elevatio_AMP_nRangeDialog");
		m_BeginningElevationRangeLabel.Text = EPShared.LocalRM.GetString("AMP_BeginningElevationDialog");
		m_EndingElevationRangeLabel.Text = EPShared.LocalRM.GetString("EndingEle_AMP_vationDialog");
		m_GradeBeginningElevationLabel.Text = EPShared.LocalRM.GetString("AMP_BeginningElevationDialog");
		m_GradeLabel.Text = EPShared.LocalRM.GetString("AMP_GradeDialog");
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
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("LocateFeatureLineLinearInterval", ref _dReturnValue))
		{
			_dReturnValue = 5.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		m_LinearIntervalTextBox.Text = _strToDisplay;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("LocateFeatureLineRelativeElevation", ref _dReturnValue))
		{
			_dReturnValue = 15.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		m_RelativeElevationTextBox.Text = _strToDisplay;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("LocateFeatureLineBeginningElevationRange", ref _dReturnValue))
		{
			_dReturnValue = 15.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		m_BeginningElevationRangeTextBox.Text = _strToDisplay;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("LocateFeatureLineEndingElevationRange", ref _dReturnValue))
		{
			_dReturnValue = 15.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		m_EndingElevationRangeTextBox.Text = _strToDisplay;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("LocateFeatureLineGradeBeginningElevation", ref _dReturnValue))
		{
			_dReturnValue = 15.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		m_GradeBeginningElevationTextBox.Text = _strToDisplay;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("LocateFeatureLineGrade", ref _dReturnValue))
		{
			_dReturnValue = 0.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_SlopeFormatOptions.LegacyDisplayUnits(), m_SlopeFormatOptions.Accuracy, m_SlopeUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		m_GradeTextBox.Text = _strToDisplay;
		m_UseExistingHostLineRadioButton.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("LocateFeatureLineHostLineSelectionOption", ref _nReturnValue))
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
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("LocateFeatureLineRadius", ref _dReturnValue))
		{
			_dReturnValue = 0.0;
		}
		RevitGeometry.Units_ConvertDoubleToString(_dReturnValue, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		m_RadiusTextBox.Text = _strToDisplay;
		m_CreateCopyCheckBox.Checked = false;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("LocateFeatureLineCreateVisibleCopy", ref _nReturnValue))
		{
			m_CreateCopyCheckBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
		m_ChainRadioButton.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("LocateFeatureLineSegmentChainOption", ref _nReturnValue))
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
		if (ApplicationUtility.Settings.eplSetE_GetSetting("LocateFeatureLineElevationOption", ref _nReturnValue))
		{
			switch (_nReturnValue)
			{
			case 0:
				m_RelativeElevationRadioButton.Checked = true;
				break;
			case 1:
				m_ElevationRangeRadioButton.Checked = true;
				break;
			default:
				m_GradeRadioButton.Checked = true;
				break;
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
		int num = 0;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_LinearIntervalTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("LocateFeatureLineLinearInterval", _dReturnedDouble);
		}
		if (RevitGeometry.Units_ConvertStringToDouble(m_RelativeElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("LocateFeatureLineRelativeElevation", _dReturnedDouble);
		}
		if (RevitGeometry.Units_ConvertStringToDouble(m_BeginningElevationRangeTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("LocateFeatureLineBeginningElevationRange", _dReturnedDouble);
		}
		if (RevitGeometry.Units_ConvertStringToDouble(m_EndingElevationRangeTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("LocateFeatureLineEndingElevationRange", _dReturnedDouble);
		}
		if (RevitGeometry.Units_ConvertStringToDouble(m_GradeBeginningElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("LocateFeatureLineGradeBeginningElevation", _dReturnedDouble);
		}
		if (RevitGeometry.Units_ConvertStringToDouble(m_GradeTextBox.Text, m_Units, UnitType.UT_Slope, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("LocateFeatureLineGrade", _dReturnedDouble);
		}
		num = ((!m_UseExistingHostLineRadioButton.Checked) ? 1 : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting("LocateFeatureLineHostLineSelectionOption", num);
		if (RevitGeometry.Units_ConvertStringToDouble(m_RadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble, _bDisplayInvalidInputMessage: false))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("LocateFeatureLineRadius", _dReturnedDouble);
		}
		ApplicationUtility.Settings.eplSetE_SetSetting("LocateFeatureLineCreateVisibleCopy", Convert.ToInt32(m_CreateCopyCheckBox.Checked));
		num = ((!m_SegmentRadioButton.Checked) ? 1 : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting("LocateFeatureLineSegmentChainOption", num);
		num = ((!m_RelativeElevationRadioButton.Checked) ? (m_ElevationRangeRadioButton.Checked ? 1 : 2) : 0);
		ApplicationUtility.Settings.eplSetE_SetSetting("LocateFeatureLineElevationOption", num);
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
		m_RelativeElevationLabel.Enabled = m_RelativeElevationRadioButton.Checked;
		m_RelativeElevationTextBox.Enabled = m_RelativeElevationRadioButton.Checked;
		m_BeginningElevationRangeLabel.Enabled = m_ElevationRangeRadioButton.Checked;
		m_BeginningElevationRangeTextBox.Enabled = m_ElevationRangeRadioButton.Checked;
		m_EndingElevationRangeLabel.Enabled = m_ElevationRangeRadioButton.Checked;
		m_EndingElevationRangeTextBox.Enabled = m_ElevationRangeRadioButton.Checked;
		m_GradeBeginningElevationLabel.Enabled = m_GradeRadioButton.Checked;
		m_GradeBeginningElevationTextBox.Enabled = m_GradeRadioButton.Checked;
		m_GradeLabel.Enabled = m_GradeRadioButton.Checked;
		m_GradeTextBox.Enabled = m_GradeRadioButton.Checked;
	}

	private bool ValidateFields()
	{
		return true;
	}

	private void FormatRelativeElevationValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_RelativeElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
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

	private void FormatBeginningElevationRangeValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_BeginningElevationRangeTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
			m_BeginningElevationRangeTextBox.Text = _strToDisplay;
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_BeginningElevationRangeTextBox);
		}
	}

	private void OnBeginningElevationRangeKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatBeginningElevationRangeValue();
		}
	}

	private void OnBeginningElevationRangeTextBoxLeave(object sender, EventArgs e)
	{
		FormatBeginningElevationRangeValue();
		m_CurrentElevationFocus = CurrentElevationFocus.BeginningElevationRange;
	}

	private void FormatEndingElevationRangeValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_EndingElevationRangeTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
			m_EndingElevationRangeTextBox.Text = _strToDisplay;
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_EndingElevationRangeTextBox);
		}
	}

	private void OnEndingElevationRangeKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatEndingElevationRangeValue();
		}
	}

	private void OnEndingElevationRangeTextBoxLeave(object sender, EventArgs e)
	{
		FormatEndingElevationRangeValue();
		m_CurrentElevationFocus = CurrentElevationFocus.EndingElevationRange;
	}

	private void FormatGradeBeginningElevationValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_GradeBeginningElevationTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
			m_GradeBeginningElevationTextBox.Text = _strToDisplay;
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_GradeBeginningElevationTextBox);
		}
	}

	private void OnGradeBeginningElevationKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatGradeBeginningElevationValue();
		}
	}

	private void OnGradeBeginningElevationTextBoxLeave(object sender, EventArgs e)
	{
		FormatGradeBeginningElevationValue();
		m_CurrentElevationFocus = CurrentElevationFocus.GradeBeginningElevation;
	}

	private void FormatGradeValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_GradeTextBox.Text, m_Units, UnitType.UT_Slope, ref _dReturnedDouble))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_SlopeFormatOptions.LegacyDisplayUnits(), m_SlopeFormatOptions.Accuracy, m_SlopeUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
			m_GradeTextBox.Text = _strToDisplay;
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_GradeTextBox);
		}
	}

	private void OnGradeKeyDown(object sender, KeyEventArgs e)
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
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
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

	private void FormatLinearIntervalValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_LinearIntervalTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble))
		{
			RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
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

	private void OnLocateSiteComponentClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	public double GetGrade(double _dLegListLength)
	{
		double _dReturnedDouble = 0.0;
		if (m_ElevationRangeRadioButton.Checked)
		{
			if (m_EndingElevationRangeTextBox.Text != string.Empty)
			{
				RevitGeometry.Units_ConvertStringToDouble(m_EndingElevationRangeTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				double num = _dReturnedDouble - m_dAdjustConstElevation - Elevation;
				_dReturnedDouble = num / _dLegListLength;
			}
		}
		else if (m_GradeRadioButton.Checked && m_GradeTextBox.Text != string.Empty)
		{
			RevitGeometry.Units_ConvertStringToDouble(m_GradeTextBox.Text, m_Units, UnitType.UT_Slope, ref _dReturnedDouble);
		}
		return _dReturnedDouble;
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
		if (!SiteDesigner.UI.WpfFocus.HasFocus(m_PickInRevitButton) && !SiteDesigner.UI.WpfFocus.HasFocus(m_BeginningElevationRangeTextBox) && !SiteDesigner.UI.WpfFocus.HasFocus(m_EndingElevationRangeTextBox) && !SiteDesigner.UI.WpfFocus.HasFocus(m_GradeBeginningElevationTextBox))
		{
			m_PickInRevitButton.Enabled = false;
		}
	}

	public void SetFocusedElevation(double _dElevation)
	{
		string _strToDisplay = string.Empty;
		_dElevation += m_dAdjustConstElevation;
		RevitGeometry.Units_ConvertDoubleToString(_dElevation, m_LengthFormatOptions.LegacyDisplayUnits(), m_LengthFormatOptions.Accuracy, m_LengthUnitSymbolType, m_dataBuffer.Document.GetUnits(), ref _strToDisplay);
		switch (m_CurrentElevationFocus)
		{
		case CurrentElevationFocus.BeginningElevationRange:
			m_BeginningElevationRangeTextBox.Text = _strToDisplay;
			m_PickInRevitButton.Enabled = true;
			break;
		case CurrentElevationFocus.EndingElevationRange:
			m_EndingElevationRangeTextBox.Text = _strToDisplay;
			m_PickInRevitButton.Enabled = true;
			break;
		case CurrentElevationFocus.GradeBeginningElevation:
			m_GradeBeginningElevationTextBox.Text = _strToDisplay;
			m_PickInRevitButton.Enabled = true;
			break;
		}
	}

	private void OnLocateSiteFeatureLineComponentFormShown(object sender, EventArgs e)
	{
		switch (m_CurrentElevationFocus)
		{
		case CurrentElevationFocus.None:
			m_PickInRevitButton.Enabled = false;
			break;
		case CurrentElevationFocus.BeginningElevationRange:
			SiteDesigner.UI.WpfFocus.Focus(m_BeginningElevationRangeTextBox);
			m_PickInRevitButton.Enabled = true;
			break;
		case CurrentElevationFocus.EndingElevationRange:
			SiteDesigner.UI.WpfFocus.Focus(m_EndingElevationRangeTextBox);
			m_PickInRevitButton.Enabled = true;
			break;
		case CurrentElevationFocus.GradeBeginningElevation:
			SiteDesigner.UI.WpfFocus.Focus(m_GradeBeginningElevationTextBox);
			m_PickInRevitButton.Enabled = true;
			break;
		case CurrentElevationFocus.ConstantElevation:
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EPCommon.LocateSiteFeatureLineComponentForm));
		this.m_BeginningElevationRangeLabel = new System.Windows.Forms.Label();
		this.m_BeginningElevationRangeTextBox = new System.Windows.Forms.TextBox();
		this.m_MoveablePanel = new System.Windows.Forms.Panel();
		this.m_CloseButton = new System.Windows.Forms.Button();
		this.m_InsertButton = new System.Windows.Forms.Button();
		this.m_CreateCopyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_RadiusLabel = new System.Windows.Forms.Label();
		this.m_RadiusTextBox = new System.Windows.Forms.TextBox();
		this.m_RelativeElevationRadioButton = new System.Windows.Forms.RadioButton();
		this.m_RelativeElevationLabel = new System.Windows.Forms.Label();
		this.m_RelativeElevationTextBox = new System.Windows.Forms.TextBox();
		this.m_ElevationRangeRadioButton = new System.Windows.Forms.RadioButton();
		this.m_UsePanel = new System.Windows.Forms.Panel();
		this.m_SegmentRadioButton = new System.Windows.Forms.RadioButton();
		this.m_ChainRadioButton = new System.Windows.Forms.RadioButton();
		this.m_HostLineSelectionPanel = new System.Windows.Forms.Panel();
		this.m_UseExistingHostLineRadioButton = new System.Windows.Forms.RadioButton();
		this.m_DrawHostLineRadioButton = new System.Windows.Forms.RadioButton();
		this.m_EndingElevationRangeLabel = new System.Windows.Forms.Label();
		this.m_EndingElevationRangeTextBox = new System.Windows.Forms.TextBox();
		this.m_GradeLabel = new System.Windows.Forms.Label();
		this.m_GradeTextBox = new System.Windows.Forms.TextBox();
		this.m_GradeRadioButton = new System.Windows.Forms.RadioButton();
		this.m_GradeBeginningElevationLabel = new System.Windows.Forms.Label();
		this.m_GradeBeginningElevationTextBox = new System.Windows.Forms.TextBox();
		this.m_LinearIntervalLabel = new System.Windows.Forms.Label();
		this.m_LinearIntervalTextBox = new System.Windows.Forms.TextBox();
		this.m_PickInRevitButton = new System.Windows.Forms.Button();
		this.m_MoveablePanel.SuspendLayout();
		this.m_UsePanel.SuspendLayout();
		this.m_HostLineSelectionPanel.SuspendLayout();
		base.SuspendLayout();
		this.m_BeginningElevationRangeLabel.AutoSize = true;
		this.m_BeginningElevationRangeLabel.Enabled = false;
		this.m_BeginningElevationRangeLabel.Location = new System.Drawing.Point(30, 113);
		this.m_BeginningElevationRangeLabel.Name = "m_BeginningElevationRangeLabel";
		this.m_BeginningElevationRangeLabel.Size = new System.Drawing.Size(104, 13);
		this.m_BeginningElevationRangeLabel.TabIndex = 7;
		this.m_BeginningElevationRangeLabel.Text = "Beginning Elevation:";
		this.m_BeginningElevationRangeTextBox.Enabled = false;
		this.m_BeginningElevationRangeTextBox.Location = new System.Drawing.Point(156, 110);
		this.m_BeginningElevationRangeTextBox.Name = "m_BeginningElevationRangeTextBox";
		this.m_BeginningElevationRangeTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_BeginningElevationRangeTextBox.TabIndex = 8;
		this.m_BeginningElevationRangeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnBeginningElevationRangeKeyDown);
		this.m_BeginningElevationRangeTextBox.Leave += new System.EventHandler(this.OnBeginningElevationRangeTextBoxLeave);
		this.m_BeginningElevationRangeTextBox.Enter += new System.EventHandler(this.OnElevationTextBoxEnter);
		this.m_BeginningElevationRangeTextBox.LostFocus += new System.EventHandler(this.OnElevationTextBoxLostFocus);
		this.m_MoveablePanel.Controls.Add(this.m_CloseButton);
		this.m_MoveablePanel.Controls.Add(this.m_InsertButton);
		this.m_MoveablePanel.Location = new System.Drawing.Point(12, 304);
		this.m_MoveablePanel.Name = "m_MoveablePanel";
		this.m_MoveablePanel.Size = new System.Drawing.Size(476, 23);
		this.m_MoveablePanel.TabIndex = 21;
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
		this.m_CreateCopyCheckBox.Location = new System.Drawing.Point(12, 209);
		this.m_CreateCopyCheckBox.Name = "m_CreateCopyCheckBox";
		this.m_CreateCopyCheckBox.Size = new System.Drawing.Size(94, 17);
		this.m_CreateCopyCheckBox.TabIndex = 17;
		this.m_CreateCopyCheckBox.Text = "Create A Copy";
		this.m_CreateCopyCheckBox.UseVisualStyleBackColor = true;
		this.m_RadiusLabel.AutoSize = true;
		this.m_RadiusLabel.Enabled = false;
		this.m_RadiusLabel.Location = new System.Drawing.Point(30, 281);
		this.m_RadiusLabel.Name = "m_RadiusLabel";
		this.m_RadiusLabel.Size = new System.Drawing.Size(43, 13);
		this.m_RadiusLabel.TabIndex = 19;
		this.m_RadiusLabel.Text = "Radius:";
		this.m_RadiusTextBox.Enabled = false;
		this.m_RadiusTextBox.Location = new System.Drawing.Point(219, 278);
		this.m_RadiusTextBox.Name = "m_RadiusTextBox";
		this.m_RadiusTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_RadiusTextBox.TabIndex = 20;
		this.m_RadiusTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnRadiusKeyDown);
		this.m_RadiusTextBox.Leave += new System.EventHandler(this.OnRadiusTextBoxLeave);
		this.m_RelativeElevationRadioButton.AutoSize = true;
		this.m_RelativeElevationRadioButton.Location = new System.Drawing.Point(12, 38);
		this.m_RelativeElevationRadioButton.Name = "m_RelativeElevationRadioButton";
		this.m_RelativeElevationRadioButton.Size = new System.Drawing.Size(197, 17);
		this.m_RelativeElevationRadioButton.TabIndex = 3;
		this.m_RelativeElevationRadioButton.Text = "&Relative Elevation from Toposurface";
		this.m_RelativeElevationRadioButton.UseVisualStyleBackColor = true;
		this.m_RelativeElevationRadioButton.CheckedChanged += new System.EventHandler(this.OnElevationSelectionOptionChecked);
		this.m_RelativeElevationLabel.AutoSize = true;
		this.m_RelativeElevationLabel.Enabled = false;
		this.m_RelativeElevationLabel.Location = new System.Drawing.Point(30, 64);
		this.m_RelativeElevationLabel.Name = "m_RelativeElevationLabel";
		this.m_RelativeElevationLabel.Size = new System.Drawing.Size(54, 13);
		this.m_RelativeElevationLabel.TabIndex = 4;
		this.m_RelativeElevationLabel.Text = "Elevation:";
		this.m_RelativeElevationTextBox.Enabled = false;
		this.m_RelativeElevationTextBox.Location = new System.Drawing.Point(156, 61);
		this.m_RelativeElevationTextBox.Name = "m_RelativeElevationTextBox";
		this.m_RelativeElevationTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_RelativeElevationTextBox.TabIndex = 5;
		this.m_RelativeElevationTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnRelativeElevationKeyDown);
		this.m_RelativeElevationTextBox.Leave += new System.EventHandler(this.OnRelativeElevationTextBoxLeave);
		this.m_ElevationRangeRadioButton.AutoSize = true;
		this.m_ElevationRangeRadioButton.Location = new System.Drawing.Point(12, 87);
		this.m_ElevationRangeRadioButton.Name = "m_ElevationRangeRadioButton";
		this.m_ElevationRangeRadioButton.Size = new System.Drawing.Size(104, 17);
		this.m_ElevationRangeRadioButton.TabIndex = 6;
		this.m_ElevationRangeRadioButton.Text = "&Elevation Range";
		this.m_ElevationRangeRadioButton.UseVisualStyleBackColor = true;
		this.m_ElevationRangeRadioButton.CheckedChanged += new System.EventHandler(this.OnElevationSelectionOptionChecked);
		this.m_UsePanel.Controls.Add(this.m_SegmentRadioButton);
		this.m_UsePanel.Controls.Add(this.m_ChainRadioButton);
		this.m_UsePanel.Location = new System.Drawing.Point(12, 186);
		this.m_UsePanel.Name = "m_UsePanel";
		this.m_UsePanel.Size = new System.Drawing.Size(476, 18);
		this.m_UsePanel.TabIndex = 16;
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
		this.m_HostLineSelectionPanel.Location = new System.Drawing.Point(12, 232);
		this.m_HostLineSelectionPanel.Name = "m_HostLineSelectionPanel";
		this.m_HostLineSelectionPanel.Size = new System.Drawing.Size(476, 41);
		this.m_HostLineSelectionPanel.TabIndex = 18;
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
		this.m_EndingElevationRangeLabel.AutoSize = true;
		this.m_EndingElevationRangeLabel.Enabled = false;
		this.m_EndingElevationRangeLabel.Location = new System.Drawing.Point(262, 113);
		this.m_EndingElevationRangeLabel.Name = "m_EndingElevationRangeLabel";
		this.m_EndingElevationRangeLabel.Size = new System.Drawing.Size(90, 13);
		this.m_EndingElevationRangeLabel.TabIndex = 9;
		this.m_EndingElevationRangeLabel.Text = "Ending Elevation:";
		this.m_EndingElevationRangeTextBox.Enabled = false;
		this.m_EndingElevationRangeTextBox.Location = new System.Drawing.Point(388, 110);
		this.m_EndingElevationRangeTextBox.Name = "m_EndingElevationRangeTextBox";
		this.m_EndingElevationRangeTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_EndingElevationRangeTextBox.TabIndex = 10;
		this.m_EndingElevationRangeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnEndingElevationRangeKeyDown);
		this.m_EndingElevationRangeTextBox.Leave += new System.EventHandler(this.OnEndingElevationRangeTextBoxLeave);
		this.m_EndingElevationRangeTextBox.Enter += new System.EventHandler(this.OnElevationTextBoxEnter);
		this.m_EndingElevationRangeTextBox.LostFocus += new System.EventHandler(this.OnElevationTextBoxLostFocus);
		this.m_GradeLabel.AutoSize = true;
		this.m_GradeLabel.Enabled = false;
		this.m_GradeLabel.Location = new System.Drawing.Point(262, 163);
		this.m_GradeLabel.Name = "m_GradeLabel";
		this.m_GradeLabel.Size = new System.Drawing.Size(39, 13);
		this.m_GradeLabel.TabIndex = 14;
		this.m_GradeLabel.Text = "Grade:";
		this.m_GradeTextBox.Enabled = false;
		this.m_GradeTextBox.Location = new System.Drawing.Point(388, 160);
		this.m_GradeTextBox.Name = "m_GradeTextBox";
		this.m_GradeTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_GradeTextBox.TabIndex = 15;
		this.m_GradeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnGradeKeyDown);
		this.m_GradeTextBox.Leave += new System.EventHandler(this.OnGradeTextBoxLeave);
		this.m_GradeRadioButton.AutoSize = true;
		this.m_GradeRadioButton.Location = new System.Drawing.Point(12, 137);
		this.m_GradeRadioButton.Name = "m_GradeRadioButton";
		this.m_GradeRadioButton.Size = new System.Drawing.Size(54, 17);
		this.m_GradeRadioButton.TabIndex = 11;
		this.m_GradeRadioButton.Text = "&Grade";
		this.m_GradeRadioButton.UseVisualStyleBackColor = true;
		this.m_GradeRadioButton.CheckedChanged += new System.EventHandler(this.OnElevationSelectionOptionChecked);
		this.m_GradeBeginningElevationLabel.AutoSize = true;
		this.m_GradeBeginningElevationLabel.Enabled = false;
		this.m_GradeBeginningElevationLabel.Location = new System.Drawing.Point(30, 163);
		this.m_GradeBeginningElevationLabel.Name = "m_GradeBeginningElevationLabel";
		this.m_GradeBeginningElevationLabel.Size = new System.Drawing.Size(104, 13);
		this.m_GradeBeginningElevationLabel.TabIndex = 12;
		this.m_GradeBeginningElevationLabel.Text = "Beginning Elevation:";
		this.m_GradeBeginningElevationTextBox.Enabled = false;
		this.m_GradeBeginningElevationTextBox.Location = new System.Drawing.Point(156, 160);
		this.m_GradeBeginningElevationTextBox.Name = "m_GradeBeginningElevationTextBox";
		this.m_GradeBeginningElevationTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_GradeBeginningElevationTextBox.TabIndex = 13;
		this.m_GradeBeginningElevationTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnGradeBeginningElevationKeyDown);
		this.m_GradeBeginningElevationTextBox.Leave += new System.EventHandler(this.OnGradeBeginningElevationTextBoxLeave);
		this.m_GradeBeginningElevationTextBox.Enter += new System.EventHandler(this.OnElevationTextBoxEnter);
		this.m_GradeBeginningElevationTextBox.LostFocus += new System.EventHandler(this.OnElevationTextBoxLostFocus);
		this.m_LinearIntervalLabel.AutoSize = true;
		this.m_LinearIntervalLabel.Location = new System.Drawing.Point(9, 15);
		this.m_LinearIntervalLabel.Name = "m_LinearIntervalLabel";
		this.m_LinearIntervalLabel.Size = new System.Drawing.Size(104, 13);
		this.m_LinearIntervalLabel.TabIndex = 0;
		this.m_LinearIntervalLabel.Text = "Linear Point Interval:";
		this.m_LinearIntervalTextBox.Location = new System.Drawing.Point(156, 12);
		this.m_LinearIntervalTextBox.Name = "m_LinearIntervalTextBox";
		this.m_LinearIntervalTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_LinearIntervalTextBox.TabIndex = 1;
		this.m_LinearIntervalTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnLinearIntervalKeyDown);
		this.m_LinearIntervalTextBox.Leave += new System.EventHandler(this.OnLinearIntervalTextBoxLeave);
		this.m_PickInRevitButton.Enabled = false;
		this.m_PickInRevitButton.Image = (System.Drawing.Image)EPCommon.SafeResources.GetObject(resources, "m_PickInRevitButton.Image");
		this.m_PickInRevitButton.Location = new System.Drawing.Point(464, 12);
		this.m_PickInRevitButton.Name = "m_PickInRevitButton";
		this.m_PickInRevitButton.Size = new System.Drawing.Size(24, 20);
		this.m_PickInRevitButton.TabIndex = 2;
		this.m_PickInRevitButton.TabStop = false;
		this.m_PickInRevitButton.UseVisualStyleBackColor = true;
		this.m_PickInRevitButton.Click += new System.EventHandler(this.OnPickInRevitButtonClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(500, 335);
		base.Controls.Add(this.m_PickInRevitButton);
		base.Controls.Add(this.m_LinearIntervalLabel);
		base.Controls.Add(this.m_LinearIntervalTextBox);
		base.Controls.Add(this.m_GradeLabel);
		base.Controls.Add(this.m_GradeTextBox);
		base.Controls.Add(this.m_GradeRadioButton);
		base.Controls.Add(this.m_GradeBeginningElevationLabel);
		base.Controls.Add(this.m_GradeBeginningElevationTextBox);
		base.Controls.Add(this.m_EndingElevationRangeLabel);
		base.Controls.Add(this.m_EndingElevationRangeTextBox);
		base.Controls.Add(this.m_HostLineSelectionPanel);
		base.Controls.Add(this.m_UsePanel);
		base.Controls.Add(this.m_ElevationRangeRadioButton);
		base.Controls.Add(this.m_RelativeElevationLabel);
		base.Controls.Add(this.m_RelativeElevationTextBox);
		base.Controls.Add(this.m_RelativeElevationRadioButton);
		base.Controls.Add(this.m_RadiusLabel);
		base.Controls.Add(this.m_RadiusTextBox);
		base.Controls.Add(this.m_CreateCopyCheckBox);
		base.Controls.Add(this.m_MoveablePanel);
		base.Controls.Add(this.m_BeginningElevationRangeLabel);
		base.Controls.Add(this.m_BeginningElevationRangeTextBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "LocateSiteFeatureLineComponentForm";
		this.Text = "Locate Feature Line";
		base.Shown += new System.EventHandler(this.OnLocateSiteFeatureLineComponentFormShown);
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
