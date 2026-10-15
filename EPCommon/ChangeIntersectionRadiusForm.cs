using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

namespace EPCommon;

public class ChangeIntersectionRadiusForm : EPForm
{
	private Units m_Units;

	private const UnitType m_LengthUnitType = UnitType.UT_Length;

	private const UnitType m_SlopeUnitType = UnitType.UT_Slope;

	private const UnitType m_AngularUnitType = UnitType.UT_Angle;

	private List<IntersectionStationContainer> m_LeftIntersectionStations;

	private List<IntersectionStationContainer> m_RightIntersectionStations;

	private List<Eps_Point> m_IntersectionPointList = new List<Eps_Point>();

	private Document m_Document;

	private EPTransactionContainer m_TransactionContainer;

	private ElementSet m_DeleteElementSet;

	private FormatOptions m_FormatOptions;

	private DisplayUnit m_DisplayUnitSystem;

	private UnitSymbolType m_UnitSymbolType;

	private DecimalSymbol m_DecimalSymbolType;

	private bool m_bRightSide = true;

	private int m_nIntersectionStationContainerIndex = -1;

	private int m_nIntersectionStationIndex = -1;

	private Eps_Leg m_LeftExistingOffsetLeg;

	private Eps_Leg m_RightExistingOffsetLeg;

	private Eps_Leg m_LeftOffsetLeg;

	private Eps_Leg m_RightOffsetLeg;

	private double m_dExistingAdjustSubgradeElev;

	private bool m_bIsFirstIntersectionFourWay;

	private Eps_Point m_InvalidLegSegmentPoint;

	private double m_dMinimumRadius;

	private LandcaddID m_CurLocateType;

	private IContainer components;

	private GroupBox m_RadiusGroupBox;

	private Button m_ApplyButton;

	private Button m_EditRadiusButton;

	private Label m_NewRadiusLbl;

	private Label m_CurrentRadiusLbl;

	private Button m_FinishButton;

	private Button m_CancelButton;

	private Label m_CurrentRadiusValueLbl;

	private TextBox m_NewRadiusTextBox;

	public List<IntersectionStationContainer> LeftIntersectionStations
	{
		get
		{
			return m_LeftIntersectionStations;
		}
		set
		{
			m_LeftIntersectionStations = value;
		}
	}

	public List<IntersectionStationContainer> RightIntersectionStations
	{
		get
		{
			return m_RightIntersectionStations;
		}
		set
		{
			m_RightIntersectionStations = value;
		}
	}

	public List<Eps_Point> MidPointList => m_IntersectionPointList;

	public bool RightSide
	{
		get
		{
			return m_bRightSide;
		}
		set
		{
			m_bRightSide = value;
		}
	}

	public int IntersectStationContainerIndex
	{
		get
		{
			return m_nIntersectionStationContainerIndex;
		}
		set
		{
			m_nIntersectionStationContainerIndex = value;
		}
	}

	public int IntersectStationIndex
	{
		get
		{
			return m_nIntersectionStationIndex;
		}
		set
		{
			m_nIntersectionStationIndex = value;
		}
	}

	public ChangeIntersectionRadiusForm(Document _Document, EPTransactionContainer _TransactionContainer, LandcaddID _LocateType, Eps_Leg _LeftExistingOffsetLeg, Eps_Leg _RightExistingOffsetLeg, Eps_Leg _LeftOffsetLeg, Eps_Leg _RightOffsetLeg, double _dExistingAdjustSubgradeElev, bool _bIsFirstIntersectionFourWay, double _dMinimumRadius)
	{
		InitializeComponent();
		m_Document = _Document;
		m_TransactionContainer = _TransactionContainer;
		m_CurLocateType = _LocateType;
		ContextualHelpID = "HID_SDstreetRadius";
		SetupString();
		GetDialogPosition();
		m_LeftExistingOffsetLeg = _LeftExistingOffsetLeg;
		m_RightExistingOffsetLeg = _RightExistingOffsetLeg;
		m_LeftOffsetLeg = _LeftOffsetLeg;
		m_RightOffsetLeg = _RightOffsetLeg;
		m_dExistingAdjustSubgradeElev = _dExistingAdjustSubgradeElev;
		m_bIsFirstIntersectionFourWay = _bIsFirstIntersectionFourWay;
		m_dMinimumRadius = _dMinimumRadius;
		m_Units = m_Document.GetUnits();
	}

	public void ShowDialog(ref DialogResult _DialogResult)
	{
		_DialogResult = ShowDialog();
	}

	public void Draw()
	{
		m_IntersectionPointList.Clear();
		if (m_DeleteElementSet != null)
		{
			m_Document.Delete(ElementSetConverter.ToIDList(m_DeleteElementSet));
			m_DeleteElementSet = null;
		}
		DrawAnnotate(m_LeftIntersectionStations);
		DrawAnnotate(m_RightIntersectionStations);
	}

	private void DrawAnnotate(List<IntersectionStationContainer> _IntersectionStations)
	{
		foreach (IntersectionStationContainer _IntersectionStation in _IntersectionStations)
		{
			foreach (IntersectionStation intersectionStation in _IntersectionStation.IntersectionStations)
			{
				if (intersectionStation.InvalidIntersectionPnt == null)
				{
					Eps_Point _StaOffPnt = new Eps_Point();
					ModelCurve modelCurve = null;
					bool _bDoesMassLineSubCategoryExist = false;
					Category category = null;
					double num = 0.0;
					if (intersectionStation.IntersectionLeg == null)
					{
						continue;
					}
					if (intersectionStation.IntersectionLeg.Arc != null)
					{
						num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(intersectionStation.IntersectionLeg.Arc.Radius, intersectionStation.IntersectionLeg.Arc.Delta);
						EPShared.eplGeoE_FindStaOffPointAlongArc(intersectionStation.IntersectionLeg.Arc, num / 2.0, 0.0, ref _StaOffPnt);
						m_IntersectionPointList.Add(_StaOffPnt);
						m_TransactionContainer.BeginTransaction();
						modelCurve = RevitUtility.NewModelCurve(m_Document, intersectionStation.IntersectionLeg.Arc);
						category = RevitUtility.GetSiteComponentLineSubCategory(m_Document, LandcaddID.IntersectionPlacement, ref _bDoesMassLineSubCategoryExist);
						if (category != null)
						{
							modelCurve.LineStyle = category.GetGraphicsStyle(GraphicsStyleType.Projection);
						}
						if (m_DeleteElementSet == null)
						{
							m_DeleteElementSet = new ElementSet();
						}
						m_DeleteElementSet.Insert(modelCurve);
						m_TransactionContainer.EndTransaction();
					}
					else if (intersectionStation.IntersectionLeg.Line != null)
					{
						EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(intersectionStation.IntersectionLeg.Line.EndPnt, intersectionStation.IntersectionLeg.Line.StartPnt, ref _StaOffPnt);
						m_IntersectionPointList.Add(_StaOffPnt);
					}
				}
				else
				{
					m_IntersectionPointList.Add(intersectionStation.InvalidIntersectionPnt);
				}
			}
		}
		m_Document.Regenerate();
	}

	private void SetupString()
	{
		m_FormatOptions = m_Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		m_DisplayUnitSystem = m_Document.DisplayUnitSystem;
		try
		{
			m_UnitSymbolType = m_FormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_UnitSymbolType = UnitSymbolType.UST_NONE;
		}
		m_DecimalSymbolType = m_Document.GetUnits().DecimalSymbol;
		Text = EPShared.LocalRM.GetString("ChangeRadiusDialogTitle");
		m_RadiusGroupBox.Text = EPShared.LocalRM.GetString("RadiusSettingsGroupTitle");
		m_CurrentRadiusLbl.Text = EPShared.LocalRM.GetString("AMP_CurrentRadiusDialog");
		m_CurrentRadiusValueLbl.Text = EPShared.LocalRM.GetString("NotAvailableString");
		m_NewRadiusLbl.Text = EPShared.LocalRM.GetString("AMP_NewRadiusDialog");
		m_ApplyButton.Text = EPShared.LocalRM.GetString("AMP_ApplyDialog");
		m_FinishButton.Text = EPShared.LocalRM.GetString("AMP_FinishDialog");
		m_CancelButton.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		m_EditRadiusButton.Text = EPShared.LocalRM.GetString("AMP_Select");
		m_ApplyButton.Enabled = false;
		m_NewRadiusTextBox.Enabled = false;
	}

	private void OnNewRadiusKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			FormatNewRadiusValue();
		}
	}

	private void OnNewRadiusTextBoxLeave(object sender, EventArgs e)
	{
		FormatNewRadiusValue();
	}

	private void FormatNewRadiusValue()
	{
		string _strToDisplay = string.Empty;
		double _dReturnedDouble = 0.0;
		if (RevitGeometry.Units_ConvertStringToDouble(m_NewRadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble))
		{
			double num = _dReturnedDouble;
			if (num < m_dMinimumRadius)
			{
				if (m_dMinimumRadius > 0.0)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidRadiusForStreetMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				}
				else
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidRadiusMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				}
				SiteDesigner.UI.WpfFocus.SelectAll(m_NewRadiusTextBox);
				SiteDesigner.UI.WpfFocus.Focus(m_NewRadiusTextBox);
			}
			else
			{
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
				m_NewRadiusTextBox.Text = _strToDisplay;
			}
		}
		else
		{
			SiteDesigner.UI.WpfFocus.Focus(m_NewRadiusTextBox);
		}
	}

	private void OnCancelButtonClicked(object sender, EventArgs e)
	{
		if (m_DeleteElementSet != null && m_DeleteElementSet.Size > 0)
		{
			m_Document.Delete(ElementSetConverter.ToIDList(m_DeleteElementSet));
			m_DeleteElementSet = null;
		}
		DialogResult = DialogResult.Cancel;
	}

	private void OnFinishButtonClicked(object sender, EventArgs e)
	{
		bool intersectionProfiles = TopographySurfaceManagement.GetIntersectionProfiles(m_CurLocateType, m_LeftExistingOffsetLeg, m_RightExistingOffsetLeg, m_LeftOffsetLeg, m_dExistingAdjustSubgradeElev, m_bIsFirstIntersectionFourWay, m_RightIntersectionStations, ref m_LeftIntersectionStations);
		bool intersectionProfiles2 = TopographySurfaceManagement.GetIntersectionProfiles(m_CurLocateType, m_LeftExistingOffsetLeg, m_RightExistingOffsetLeg, m_RightOffsetLeg, m_dExistingAdjustSubgradeElev, m_bIsFirstIntersectionFourWay, intersectionProfiles, m_LeftIntersectionStations, ref m_RightIntersectionStations);
		if (m_DeleteElementSet != null && m_DeleteElementSet.Size > 0)
		{
			m_Document.Delete(ElementSetConverter.ToIDList(m_DeleteElementSet));
			m_DeleteElementSet = null;
		}
		m_IntersectionPointList.Clear();
		foreach (IntersectionStationContainer rightIntersectionStation in m_RightIntersectionStations)
		{
			foreach (IntersectionStation intersectionStation in rightIntersectionStation.IntersectionStations)
			{
				if (intersectionStation.InvalidIntersectionPnt != null)
				{
					m_IntersectionPointList.Add(intersectionStation.InvalidIntersectionPnt);
				}
			}
		}
		foreach (IntersectionStationContainer leftIntersectionStation in m_LeftIntersectionStations)
		{
			foreach (IntersectionStation intersectionStation2 in leftIntersectionStation.IntersectionStations)
			{
				if (intersectionStation2.InvalidIntersectionPnt != null)
				{
					m_IntersectionPointList.Add(intersectionStation2.InvalidIntersectionPnt);
				}
			}
		}
		if (m_IntersectionPointList.Count > 0)
		{
			DialogResult = DialogResult.Retry;
		}
		else
		{
			DialogResult = DialogResult.OK;
		}
	}

	private void OnApplyButtonClicked(object sender, EventArgs e)
	{
		bool flag = true;
		double _dReturnedDouble = 0.0;
		if (m_NewRadiusTextBox.Text != string.Empty && RevitGeometry.Units_ConvertStringToDouble(m_NewRadiusTextBox.Text, m_Units, UnitType.UT_Length, ref _dReturnedDouble) && _dReturnedDouble >= m_dMinimumRadius)
		{
			flag = false;
		}
		if (flag)
		{
			if (m_dMinimumRadius > 0.0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidRadiusForStreetMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
			else
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidRadiusMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
			return;
		}
		try
		{
			if (m_InvalidLegSegmentPoint != null && m_IntersectionPointList.Remove(m_InvalidLegSegmentPoint))
			{
				m_InvalidLegSegmentPoint = null;
			}
			if (m_bRightSide)
			{
				if (m_nIntersectionStationContainerIndex >= 0 && m_nIntersectionStationIndex >= 0 && m_nIntersectionStationContainerIndex < m_RightIntersectionStations.Count && m_nIntersectionStationIndex < m_RightIntersectionStations[m_nIntersectionStationContainerIndex].IntersectionStations.Count)
				{
					m_RightIntersectionStations[m_nIntersectionStationContainerIndex].IntersectionStations[m_nIntersectionStationIndex].Radius = _dReturnedDouble;
				}
			}
			else if (m_nIntersectionStationContainerIndex >= 0 && m_nIntersectionStationIndex >= 0 && m_nIntersectionStationContainerIndex < m_LeftIntersectionStations.Count && m_nIntersectionStationIndex < m_LeftIntersectionStations[m_nIntersectionStationContainerIndex].IntersectionStations.Count)
			{
				m_LeftIntersectionStations[m_nIntersectionStationContainerIndex].IntersectionStations[m_nIntersectionStationIndex].Radius = _dReturnedDouble;
			}
			if (m_bRightSide)
			{
				TopographySurfaceManagement.GetIntersectionProfiles(m_CurLocateType, m_LeftExistingOffsetLeg, m_RightExistingOffsetLeg, m_RightOffsetLeg, m_dExistingAdjustSubgradeElev, m_bIsFirstIntersectionFourWay, m_nIntersectionStationContainerIndex, m_nIntersectionStationIndex, m_LeftIntersectionStations, ref m_RightIntersectionStations);
				m_InvalidLegSegmentPoint = m_RightIntersectionStations[m_nIntersectionStationContainerIndex].IntersectionStations[m_nIntersectionStationIndex].InvalidIntersectionPnt;
			}
			else
			{
				TopographySurfaceManagement.GetIntersectionProfiles(m_CurLocateType, m_LeftExistingOffsetLeg, m_RightExistingOffsetLeg, m_LeftOffsetLeg, m_dExistingAdjustSubgradeElev, m_bIsFirstIntersectionFourWay, m_nIntersectionStationContainerIndex, m_nIntersectionStationIndex, m_RightIntersectionStations, ref m_LeftIntersectionStations);
				m_InvalidLegSegmentPoint = m_LeftIntersectionStations[m_nIntersectionStationContainerIndex].IntersectionStations[m_nIntersectionStationIndex].InvalidIntersectionPnt;
			}
			DialogResult = DialogResult.Ignore;
		}
		catch (Exception)
		{
		}
	}

	private string CalculateDisplayValue(string _strValue)
	{
		double num = 0.0;
		string _strToDisplay = EPShared.LocalRM.GetString("NotAvailableString");
		if (_strValue != string.Empty)
		{
			num = RevitGeometry.ConvertLinearValue(ValueConversion.ToDouble(_strValue), DisplayUnitType.DUT_DECIMAL_FEET, m_FormatOptions.LegacyDisplayUnits());
		}
		if (num >= 0.0)
		{
			RevitGeometry.Units_ConvertDoubleToString(num, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
		}
		return _strToDisplay;
	}

	private void OnEditArcClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Retry;
		m_ApplyButton.Enabled = true;
		m_NewRadiusTextBox.Enabled = true;
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private void OnFormLoad(object sender, EventArgs e)
	{
		OverrideKeyUp();
	}

	public void SetNewRadius(double _dNewRadius)
	{
		m_NewRadiusTextBox.Text = CalculateDisplayValue(_dNewRadius.ToString());
		m_CurrentRadiusValueLbl.Text = m_NewRadiusTextBox.Text;
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
		this.m_RadiusGroupBox = new System.Windows.Forms.GroupBox();
		this.m_NewRadiusTextBox = new System.Windows.Forms.TextBox();
		this.m_CurrentRadiusValueLbl = new System.Windows.Forms.Label();
		this.m_ApplyButton = new System.Windows.Forms.Button();
		this.m_EditRadiusButton = new System.Windows.Forms.Button();
		this.m_NewRadiusLbl = new System.Windows.Forms.Label();
		this.m_CurrentRadiusLbl = new System.Windows.Forms.Label();
		this.m_FinishButton = new System.Windows.Forms.Button();
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_RadiusGroupBox.SuspendLayout();
		base.SuspendLayout();
		this.m_RadiusGroupBox.Controls.Add(this.m_FinishButton);
		this.m_RadiusGroupBox.Controls.Add(this.m_CancelButton);
		this.m_RadiusGroupBox.Controls.Add(this.m_NewRadiusTextBox);
		this.m_RadiusGroupBox.Controls.Add(this.m_CurrentRadiusValueLbl);
		this.m_RadiusGroupBox.Controls.Add(this.m_ApplyButton);
		this.m_RadiusGroupBox.Controls.Add(this.m_EditRadiusButton);
		this.m_RadiusGroupBox.Controls.Add(this.m_NewRadiusLbl);
		this.m_RadiusGroupBox.Controls.Add(this.m_CurrentRadiusLbl);
		this.m_RadiusGroupBox.Location = new System.Drawing.Point(12, 12);
		this.m_RadiusGroupBox.Name = "m_RadiusGroupBox";
		this.m_RadiusGroupBox.Size = new System.Drawing.Size(334, 101);
		this.m_RadiusGroupBox.TabIndex = 0;
		this.m_RadiusGroupBox.TabStop = false;
		this.m_RadiusGroupBox.Text = "Radius Settings";
		this.m_NewRadiusTextBox.Location = new System.Drawing.Point(113, 45);
		this.m_NewRadiusTextBox.Name = "m_NewRadiusTextBox";
		this.m_NewRadiusTextBox.Size = new System.Drawing.Size(137, 20);
		this.m_NewRadiusTextBox.TabIndex = 3;
		this.m_NewRadiusTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnNewRadiusKeyDown);
		this.m_NewRadiusTextBox.Leave += new System.EventHandler(this.OnNewRadiusTextBoxLeave);
		this.m_CurrentRadiusValueLbl.AutoSize = true;
		this.m_CurrentRadiusValueLbl.Location = new System.Drawing.Point(110, 19);
		this.m_CurrentRadiusValueLbl.Name = "m_CurrentRadiusValueLbl";
		this.m_CurrentRadiusValueLbl.Size = new System.Drawing.Size(27, 13);
		this.m_CurrentRadiusValueLbl.TabIndex = 1;
		this.m_CurrentRadiusValueLbl.Text = "N/A";
		this.m_ApplyButton.Location = new System.Drawing.Point(253, 43);
		this.m_ApplyButton.Name = "m_ApplyButton";
		this.m_ApplyButton.Size = new System.Drawing.Size(75, 23);
		this.m_ApplyButton.TabIndex = 5;
		this.m_ApplyButton.Text = "Apply";
		this.m_ApplyButton.UseVisualStyleBackColor = true;
		this.m_ApplyButton.Click += new System.EventHandler(this.OnApplyButtonClicked);
		this.m_EditRadiusButton.Location = new System.Drawing.Point(253, 14);
		this.m_EditRadiusButton.Name = "m_EditRadiusButton";
		this.m_EditRadiusButton.Size = new System.Drawing.Size(75, 23);
		this.m_EditRadiusButton.TabIndex = 4;
		this.m_EditRadiusButton.UseVisualStyleBackColor = true;
		this.m_EditRadiusButton.Click += new System.EventHandler(this.OnEditArcClicked);
		this.m_NewRadiusLbl.AutoSize = true;
		this.m_NewRadiusLbl.Location = new System.Drawing.Point(6, 48);
		this.m_NewRadiusLbl.Name = "m_NewRadiusLbl";
		this.m_NewRadiusLbl.Size = new System.Drawing.Size(65, 13);
		this.m_NewRadiusLbl.TabIndex = 2;
		this.m_NewRadiusLbl.Text = "New Radius";
		this.m_CurrentRadiusLbl.AutoSize = true;
		this.m_CurrentRadiusLbl.Location = new System.Drawing.Point(6, 19);
		this.m_CurrentRadiusLbl.Name = "m_CurrentRadiusLbl";
		this.m_CurrentRadiusLbl.Size = new System.Drawing.Size(77, 13);
		this.m_CurrentRadiusLbl.TabIndex = 0;
		this.m_CurrentRadiusLbl.Text = "Current Radius";
		this.m_FinishButton.Location = new System.Drawing.Point(172, 72);
		this.m_FinishButton.Name = "m_FinishButton";
		this.m_FinishButton.Size = new System.Drawing.Size(75, 23);
		this.m_FinishButton.TabIndex = 6;
		this.m_FinishButton.Text = "Done";
		this.m_FinishButton.UseVisualStyleBackColor = true;
		this.m_FinishButton.Click += new System.EventHandler(this.OnFinishButtonClicked);
		this.m_CancelButton.Location = new System.Drawing.Point(253, 72);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 7;
		this.m_CancelButton.Text = "Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButtonClicked);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(358, 125);
		base.Controls.Add(this.m_RadiusGroupBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "ChangeIntersectionRadiusForm";
		this.Text = "ChangeIntersectionRadiusForm";
		base.Load += new System.EventHandler(this.OnFormLoad);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
		this.m_RadiusGroupBox.ResumeLayout(false);
		this.m_RadiusGroupBox.PerformLayout();
		base.ResumeLayout(false);
		base.AcceptButton = this.m_ApplyButton;
		base.CancelButton = this.m_CancelButton;
	}
}
