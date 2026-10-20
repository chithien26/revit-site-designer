using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace EPCommon;

public class PhasePickerForm : EPForm
{
	private class PhaseInformation
	{
		private string m_PhaseName;

		private long m_PhaseID;

		public string PhaseName => m_PhaseName;

		public long PhaseID => m_PhaseID;

		public PhaseInformation(string _strPhaseName, long _nPhaseValue)
		{
			m_PhaseName = _strPhaseName;
			m_PhaseID = _nPhaseValue;
		}
	}

	private Document m_Document;

	private List<Phase> m_PhaseCreatedList = new List<Phase>();

	private IContainer components;

	private Button m_OkButton;

	private TextBox m_ToposurfaceNameTextBox;

	private ComboBox m_PhaseDemolishedCmbBox;

	private Label m_PhaseDemolishedLbl;

	private ComboBox m_PhaseCreatedCmbBox;

	private Label m_PhaseCreatedLbl;

	private Label m_ToposurfaceNameLabel;

	private CheckBox m_HideOriginalCheckBox;

	public long CreatedPhaseId
	{
		get
		{
			int num = -1;
			Phase phase = (Phase)m_PhaseCreatedCmbBox.SelectedItem;
			return phase.Id.Value64();
		}
	}

	public long DemolishedPhaseId => (long)m_PhaseDemolishedCmbBox.SelectedValue;

	public string ToposurfaceName => m_ToposurfaceNameTextBox.Text;

	public bool HideOriginalTopoSurface => m_HideOriginalCheckBox.Checked;

	public PhasePickerForm(Document _CurrentDocument)
	{
		ContextualHelpID = "HID_SDselectSurfacePhase";
		InitializeComponent();
		Text = EPShared.LocalRM.GetString("SelectSurfaceNameAndPhaseDialog");
		m_Document = _CurrentDocument;
		InitializeDlg();
		GetDialogPosition();
	}

	private void InitializeDlg()
	{
		int selectedIndex = 0;
		int num = 0;
		long num2 = -1;
		foreach (Phase phase in m_Document.Phases)
		{
			num2 = phase.Id.Value64();
			if ((m_Document.DisplayUnitSystem == DisplayUnit.IMPERIAL && num2 == 118390) || (m_Document.DisplayUnitSystem == DisplayUnit.METRIC && num2 == 86961) || string.Compare(phase.Name, EPShared.LocalRM.GetString("NewConstructionString"), ignoreCase: true) == 0)
			{
				selectedIndex = num;
			}
			m_PhaseCreatedList.Add(phase);
			num++;
		}
		m_PhaseDemolishedLbl.Hide();
		m_PhaseDemolishedCmbBox.Hide();
		Text = EPShared.LocalRM.GetString("SelectSurfaceNameAndPhaseDialog");
		m_ToposurfaceNameLabel.Text = EPShared.LocalRM.GetString("AMP_ToposurfaceNameDialog");
		m_OkButton.Text = EPShared.LocalRM.GetString("AMP_ApplyDialog");
		m_PhaseCreatedLbl.Text = EPShared.LocalRM.GetString("AMP_PhaseCreatedFamilyParameterSurfaceInfoDialog");
		m_PhaseDemolishedLbl.Text = EPShared.LocalRM.GetString("AMP_PhaseDemolishedFamilyParameterSurfaceInfoDialog");
		m_ToposurfaceNameTextBox.Text = EPShared.LocalRM.GetString("ProposedToposurfaceString");
		m_HideOriginalCheckBox.Checked = TerrainApi.UsesToposolids;
		m_PhaseCreatedCmbBox.DataSource = m_PhaseCreatedList;
		m_PhaseCreatedCmbBox.DisplayMember = EPShared.LocalRM.GetString("NameString");
		if (m_PhaseCreatedCmbBox.Items.Count > 0)
		{
			m_PhaseCreatedCmbBox.SelectedIndex = selectedIndex;
		}
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private void OkButtonClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.OK;
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
		this.m_OkButton = new System.Windows.Forms.Button();
		this.m_ToposurfaceNameTextBox = new System.Windows.Forms.TextBox();
		this.m_PhaseDemolishedCmbBox = new System.Windows.Forms.ComboBox();
		this.m_PhaseDemolishedLbl = new System.Windows.Forms.Label();
		this.m_PhaseCreatedCmbBox = new System.Windows.Forms.ComboBox();
		this.m_PhaseCreatedLbl = new System.Windows.Forms.Label();
		this.m_ToposurfaceNameLabel = new System.Windows.Forms.Label();
		this.m_HideOriginalCheckBox = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.m_OkButton.Location = new System.Drawing.Point(307, 87);
		this.m_OkButton.Name = "m_OkButton";
		this.m_OkButton.Size = new System.Drawing.Size(75, 23);
		this.m_OkButton.TabIndex = 6;
		this.m_OkButton.Text = "Ok";
		this.m_OkButton.UseVisualStyleBackColor = true;
		this.m_OkButton.Click += new System.EventHandler(this.OkButtonClicked);
		this.m_ToposurfaceNameTextBox.Location = new System.Drawing.Point(146, 12);
		this.m_ToposurfaceNameTextBox.Name = "m_ToposurfaceNameTextBox";
		this.m_ToposurfaceNameTextBox.Size = new System.Drawing.Size(236, 20);
		this.m_ToposurfaceNameTextBox.TabIndex = 1;
		this.m_PhaseDemolishedCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_PhaseDemolishedCmbBox.FormattingEnabled = true;
		this.m_PhaseDemolishedCmbBox.Location = new System.Drawing.Point(146, 54);
		this.m_PhaseDemolishedCmbBox.Name = "m_PhaseDemolishedCmbBox";
		this.m_PhaseDemolishedCmbBox.Size = new System.Drawing.Size(236, 21);
		this.m_PhaseDemolishedCmbBox.TabIndex = 5;
		this.m_PhaseDemolishedLbl.AutoSize = true;
		this.m_PhaseDemolishedLbl.Location = new System.Drawing.Point(12, 57);
		this.m_PhaseDemolishedLbl.Name = "m_PhaseDemolishedLbl";
		this.m_PhaseDemolishedLbl.Size = new System.Drawing.Size(95, 13);
		this.m_PhaseDemolishedLbl.TabIndex = 4;
		this.m_PhaseDemolishedLbl.Text = "Phase Demolished";
		this.m_PhaseCreatedCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_PhaseCreatedCmbBox.FormattingEnabled = true;
		this.m_PhaseCreatedCmbBox.Location = new System.Drawing.Point(146, 38);
		this.m_PhaseCreatedCmbBox.Name = "m_PhaseCreatedCmbBox";
		this.m_PhaseCreatedCmbBox.Size = new System.Drawing.Size(236, 21);
		this.m_PhaseCreatedCmbBox.TabIndex = 3;
		this.m_PhaseCreatedLbl.AutoSize = true;
		this.m_PhaseCreatedLbl.Location = new System.Drawing.Point(12, 41);
		this.m_PhaseCreatedLbl.Name = "m_PhaseCreatedLbl";
		this.m_PhaseCreatedLbl.Size = new System.Drawing.Size(77, 13);
		this.m_PhaseCreatedLbl.TabIndex = 2;
		this.m_PhaseCreatedLbl.Text = "Phase Created";
		this.m_ToposurfaceNameLabel.AutoSize = true;
		this.m_ToposurfaceNameLabel.Location = new System.Drawing.Point(12, 15);
		this.m_ToposurfaceNameLabel.Name = "m_ToposurfaceNameLabel";
		this.m_ToposurfaceNameLabel.Size = new System.Drawing.Size(101, 13);
		this.m_ToposurfaceNameLabel.TabIndex = 0;
		this.m_ToposurfaceNameLabel.Text = "Toposurface Name:";
		this.m_HideOriginalCheckBox.Location = new System.Drawing.Point(15, 73);
		this.m_HideOriginalCheckBox.Name = "m_HideOriginalCheckBox";
		this.m_HideOriginalCheckBox.Size = new System.Drawing.Size(98, 37);
		this.m_HideOriginalCheckBox.TabIndex = 8;
		this.m_HideOriginalCheckBox.Text = "Hide Original Toposurface";
		this.m_HideOriginalCheckBox.UseVisualStyleBackColor = true;
		base.AcceptButton = this.m_OkButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(394, 118);
		base.Controls.Add(this.m_HideOriginalCheckBox);
		base.Controls.Add(this.m_ToposurfaceNameLabel);
		base.Controls.Add(this.m_PhaseDemolishedCmbBox);
		base.Controls.Add(this.m_PhaseDemolishedLbl);
		base.Controls.Add(this.m_PhaseCreatedCmbBox);
		base.Controls.Add(this.m_PhaseCreatedLbl);
		base.Controls.Add(this.m_ToposurfaceNameTextBox);
		base.Controls.Add(this.m_OkButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Name = "PhasePickerForm";
		this.Text = "PhasePickerForm";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
