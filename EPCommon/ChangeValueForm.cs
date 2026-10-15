using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

namespace EPCommon;

public class ChangeValueForm : EPForm
{
	private Units m_Units;

	private FormatOptions m_FormatOptions;

	private DisplayUnit m_DisplayUnitSystem;

	private UnitSymbolType m_UnitSymbolType;

	private UnitType m_UnitType;

	private DecimalSymbol m_DecimalSymbolType;

	private bool m_bIsUnitValue;

	private bool m_bIsRatioValue;

	private IContainer components;

	private Button m_OKButton;

	private Button m_CancelButton;

	private Label m_NewValueLabel;

	private TextBox m_NewFieldTextBox;

	private Label m_CurrentValueLabel;

	private Label m_CurrentFieldLabel;

	private Label m_CurrentValueRatioLabel;

	private Label m_NewValueRatioLabel;

	public double NewUnitFieldValue
	{
		get
		{
			double _dReturnedDouble = 0.0;
			if (m_NewFieldTextBox.Text != string.Empty)
			{
				switch (m_UnitType)
				{
				case UnitType.UT_Length:
					RevitGeometry.Units_ConvertStringToDouble(m_NewFieldTextBox.Text, m_Units, m_UnitType, ref _dReturnedDouble);
					break;
				case UnitType.UT_Angle:
					_dReturnedDouble = RevitGeometry.eplGeoE_ConvertAngleToRadians(m_NewFieldTextBox.Text);
					break;
				case UnitType.UT_Slope:
					RevitGeometry.Units_ConvertStringToDouble(m_NewFieldTextBox.Text, m_Units, m_UnitType, ref _dReturnedDouble);
					break;
				}
			}
			return _dReturnedDouble;
		}
	}

	public int NewIntegerFieldValue
	{
		get
		{
			int result = 0;
			if (m_NewFieldTextBox.Text != string.Empty)
			{
				result = Convert.ToInt32(m_NewFieldTextBox.Text);
			}
			return result;
		}
	}

	public ChangeValueForm(Document _Document, string _strFormTitle, string _strCurrentValueLabel, string _strNewValueLabel, double _dCurrentValue, UnitType _UnitType, string _contextID)
	{
		InitializeComponent();
		Text = _strFormTitle;
		m_CurrentValueLabel.Text = _strCurrentValueLabel;
		m_NewValueLabel.Text = _strNewValueLabel;
		m_UnitType = _UnitType;
		m_FormatOptions = _Document.GetUnits().GetFormatOptions(m_UnitType);
		m_DisplayUnitSystem = _Document.DisplayUnitSystem;
		try
		{
			m_UnitSymbolType = m_FormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_UnitSymbolType = UnitSymbolType.UST_NONE;
		}
		m_DecimalSymbolType = _Document.GetUnits().DecimalSymbol;
		ContextualHelpID = _contextID;
		m_bIsUnitValue = true;
		m_bIsRatioValue = false;
		m_Units = _Document.GetUnits();
		InitializeControls(_dCurrentValue);
	}

	public ChangeValueForm(Document _Document, string _strFormTitle, string _strCurrentValueLabel, string _strNewValueLabel, int _nCurrentValue, bool _bIsRatioValue, string _contextID)
	{
		InitializeComponent();
		Text = _strFormTitle;
		m_CurrentValueLabel.Text = _strCurrentValueLabel;
		m_NewValueLabel.Text = _strNewValueLabel;
		ContextualHelpID = _contextID;
		m_bIsUnitValue = false;
		m_bIsRatioValue = _bIsRatioValue;
		m_Units = _Document.GetUnits();
		InitializeControls(_nCurrentValue);
	}

	private void InitializeControls(double _dCurrentValue)
	{
		string _strToDisplay = string.Empty;
		GetDialogPosition();
		RevitGeometry.Units_ConvertDoubleToString(_dCurrentValue, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
		m_CurrentFieldLabel.Text = _strToDisplay;
		m_NewFieldTextBox.Text = _strToDisplay;
		m_CurrentValueRatioLabel.Visible = m_bIsRatioValue;
		m_NewValueRatioLabel.Visible = m_bIsRatioValue;
		if (!m_bIsRatioValue)
		{
			System.Drawing.Point location = m_CurrentFieldLabel.Location;
			location.X -= m_CurrentValueRatioLabel.Width;
			m_CurrentFieldLabel.Location = location;
			location = m_NewFieldTextBox.Location;
			location.X -= m_CurrentValueRatioLabel.Width;
			m_NewFieldTextBox.Location = location;
			location = m_OKButton.Location;
			location.X -= m_CurrentValueRatioLabel.Width;
			m_OKButton.Location = location;
			location = m_CancelButton.Location;
			location.X -= m_CurrentValueRatioLabel.Width;
			m_CancelButton.Location = location;
			Size size = Size;
			size.Width -= m_CurrentValueRatioLabel.Width;
			Size = size;
		}
	}

	private void InitializeControls(int _nCurrentValue)
	{
		GetDialogPosition();
		m_CurrentFieldLabel.Text = Convert.ToString(_nCurrentValue);
		m_NewFieldTextBox.Text = Convert.ToString(_nCurrentValue);
	}

	private void OnOKButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.OK;
		Close();
	}

	private void OnCancelButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void OnNewFieldTextBoxLeave(object sender, EventArgs e)
	{
		FormatNewFieldValue();
	}

	private void OnNewFieldTextBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatNewFieldValue();
		}
	}

	private void FormatNewFieldValue()
	{
		if (!m_bIsUnitValue)
		{
			return;
		}
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		bool flag = false;
		switch (m_UnitType)
		{
		case UnitType.UT_Length:
			flag = RevitGeometry.Units_ConvertStringToDouble(m_NewFieldTextBox.Text, m_Units, m_UnitType, ref _dReturnedDouble);
			if (flag)
			{
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
			}
			break;
		case UnitType.UT_Angle:
			_dReturnedDouble = RevitGeometry.eplGeoE_ConvertAngleToRadians(m_NewFieldTextBox.Text);
			if (RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_FormatOptions.LegacyDisplayUnits(), _dReturnedDouble, m_FormatOptions.Accuracy, m_UnitSymbolType, m_DecimalSymbolType, ref _strToDisplay) == 0)
			{
				flag = true;
			}
			break;
		case UnitType.UT_Slope:
			flag = RevitGeometry.Units_ConvertStringToDouble(m_NewFieldTextBox.Text, m_Units, m_UnitType, ref _dReturnedDouble);
			if (flag)
			{
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
			}
			break;
		}
		if (flag)
		{
			m_NewFieldTextBox.Text = _strToDisplay;
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_NewFieldTextBox);
		}
	}

	private void OnChangeValueClosing(object sender, FormClosingEventArgs e)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EPCommon.ChangeValueForm));
		this.m_OKButton = new System.Windows.Forms.Button();
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_NewValueLabel = new System.Windows.Forms.Label();
		this.m_NewFieldTextBox = new System.Windows.Forms.TextBox();
		this.m_CurrentValueLabel = new System.Windows.Forms.Label();
		this.m_CurrentFieldLabel = new System.Windows.Forms.Label();
		this.m_CurrentValueRatioLabel = new System.Windows.Forms.Label();
		this.m_NewValueRatioLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_OKButton.Location = new System.Drawing.Point(200, 64);
		this.m_OKButton.Name = "m_OKButton";
		this.m_OKButton.Size = new System.Drawing.Size(75, 23);
		this.m_OKButton.TabIndex = 6;
		this.m_OKButton.Text = "&OK";
		this.m_OKButton.UseVisualStyleBackColor = true;
		this.m_OKButton.Click += new System.EventHandler(this.OnOKButtonClick);
		this.m_CancelButton.Location = new System.Drawing.Point(281, 64);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 7;
		this.m_CancelButton.Text = "&Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButtonClick);
		this.m_NewValueLabel.AutoSize = true;
		this.m_NewValueLabel.Location = new System.Drawing.Point(12, 41);
		this.m_NewValueLabel.Name = "m_NewValueLabel";
		this.m_NewValueLabel.Size = new System.Drawing.Size(62, 13);
		this.m_NewValueLabel.TabIndex = 3;
		this.m_NewValueLabel.Text = "New Value:";
		this.m_NewFieldTextBox.Location = new System.Drawing.Point(136, 38);
		this.m_NewFieldTextBox.Name = "m_NewFieldTextBox";
		this.m_NewFieldTextBox.Size = new System.Drawing.Size(220, 20);
		this.m_NewFieldTextBox.TabIndex = 5;
		this.m_NewFieldTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnNewFieldTextBoxKeyDown);
		this.m_NewFieldTextBox.Leave += new System.EventHandler(this.OnNewFieldTextBoxLeave);
		this.m_CurrentValueLabel.AutoSize = true;
		this.m_CurrentValueLabel.Location = new System.Drawing.Point(12, 15);
		this.m_CurrentValueLabel.Name = "m_CurrentValueLabel";
		this.m_CurrentValueLabel.Size = new System.Drawing.Size(74, 13);
		this.m_CurrentValueLabel.TabIndex = 0;
		this.m_CurrentValueLabel.Text = "Current Value:";
		this.m_CurrentFieldLabel.AutoSize = true;
		this.m_CurrentFieldLabel.Location = new System.Drawing.Point(133, 15);
		this.m_CurrentFieldLabel.Name = "m_CurrentFieldLabel";
		this.m_CurrentFieldLabel.Size = new System.Drawing.Size(34, 13);
		this.m_CurrentFieldLabel.TabIndex = 2;
		this.m_CurrentFieldLabel.Text = "Value";
		this.m_CurrentValueRatioLabel.AutoSize = true;
		this.m_CurrentValueRatioLabel.Location = new System.Drawing.Point(117, 15);
		this.m_CurrentValueRatioLabel.Name = "m_CurrentValueRatioLabel";
		this.m_CurrentValueRatioLabel.Size = new System.Drawing.Size(19, 13);
		this.m_CurrentValueRatioLabel.TabIndex = 1;
		this.m_CurrentValueRatioLabel.Text = "1 :";
		this.m_NewValueRatioLabel.AutoSize = true;
		this.m_NewValueRatioLabel.Location = new System.Drawing.Point(117, 41);
		this.m_NewValueRatioLabel.Name = "m_NewValueRatioLabel";
		this.m_NewValueRatioLabel.Size = new System.Drawing.Size(19, 13);
		this.m_NewValueRatioLabel.TabIndex = 4;
		this.m_NewValueRatioLabel.Text = "1 :";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(368, 99);
		base.Controls.Add(this.m_NewValueRatioLabel);
		base.Controls.Add(this.m_CurrentValueRatioLabel);
		base.Controls.Add(this.m_CurrentFieldLabel);
		base.Controls.Add(this.m_CurrentValueLabel);
		base.Controls.Add(this.m_NewValueLabel);
		base.Controls.Add(this.m_NewFieldTextBox);
		base.Controls.Add(this.m_OKButton);
		base.Controls.Add(this.m_CancelButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Icon = (System.Drawing.Icon)EPCommon.SafeResources.GetObject(resources, "$this.Icon");
		base.Name = "ChangeValueForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Change Value";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnChangeValueClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_OKButton;
		base.CancelButton = this.m_CancelButton;
	}
}
