using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace EPCommon;

public class ConvertToToposurfaceForm : EPForm
{
	private Document m_Document;

	private IContainer components;

	private Button m_ConvertButton;

	private Button m_CancelButton;

	private Label m_ToposurfaceNameLabel;

	private TextBox m_ToposurfaceNameTextBox;

	private Button m_PickButton;

	private ComboBox m_PhaseComboBox;

	private Label m_PhaseLabel;

	private CheckBox m_HideOriginalCheckBox;

	public string ToposurfaceName => m_ToposurfaceNameTextBox.Text;

	public Phase SelectedPhase
	{
		get
		{
			if (m_PhaseComboBox.SelectedIndex >= 0)
			{
				return m_PhaseComboBox.SelectedItem as Phase;
			}
			return null;
		}
	}

	public bool HideOriginalTopoSurface => m_HideOriginalCheckBox.Checked;

	public ConvertToToposurfaceForm(Document _CurDocument)
	{
		InitializeComponent();
		ContextualHelpID = "HID_SDtopoConversion";
		Text = EPShared.LocalRM.GetString("ConvertObjectToToposurfaceTitle");
		m_ToposurfaceNameLabel.Text = EPShared.LocalRM.GetString("AMP_ToposurfaceNameDialog");
		m_PhaseLabel.Text = EPShared.LocalRM.GetString("AMP_PhaseCreatedFamilyParameterSurfaceInfoDialog");
		m_PickButton.Text = EPShared.LocalRM.GetString("AMP_PickDialog");
		m_ConvertButton.Text = EPShared.LocalRM.GetString("AMP_ConvertDialog");
		m_CancelButton.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		m_HideOriginalCheckBox.Checked = false;
		m_Document = _CurDocument;
		LoadPhases();
		GetDialogPosition();
	}

	private void LoadPhases()
	{
		List<Phase> list = new List<Phase>();
		foreach (Phase phase in m_Document.Phases)
		{
			list.Add(phase);
		}
		m_PhaseComboBox.DataSource = list;
		m_PhaseComboBox.DisplayMember = "Name";
		if (m_PhaseComboBox.Items.Count > 0)
		{
			m_PhaseComboBox.SelectedIndex = 0;
		}
	}

	private void OnConvertButtonClick(object sender, EventArgs e)
	{
		if (ValidateFields())
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	private void OnCancelButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void OnPickButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnConvertToToposurfaceFormClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private bool ValidateFields()
	{
		string value = m_ToposurfaceNameTextBox.Text;
		if (string.IsNullOrEmpty(value))
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("Specify_Message") + EPShared.LocalRM.GetString("ToposurfaceString") + EPShared.LocalRM.GetString("NameString") + " ", EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			SiteDesigner.UI.WpfFocus.Focus(m_ToposurfaceNameTextBox);
			return false;
		}
		return true;
	}

	public void EnableConvertButton()
	{
		m_ConvertButton.Enabled = true;
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
		this.m_ConvertButton = new System.Windows.Forms.Button();
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_ToposurfaceNameLabel = new System.Windows.Forms.Label();
		this.m_ToposurfaceNameTextBox = new System.Windows.Forms.TextBox();
		this.m_PickButton = new System.Windows.Forms.Button();
		this.m_PhaseComboBox = new System.Windows.Forms.ComboBox();
		this.m_PhaseLabel = new System.Windows.Forms.Label();
		this.m_HideOriginalCheckBox = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.m_ConvertButton.Enabled = false;
		this.m_ConvertButton.Location = new System.Drawing.Point(196, 65);
		this.m_ConvertButton.Name = "m_ConvertButton";
		this.m_ConvertButton.Size = new System.Drawing.Size(75, 23);
		this.m_ConvertButton.TabIndex = 5;
		this.m_ConvertButton.Text = "&Convert";
		this.m_ConvertButton.UseVisualStyleBackColor = true;
		this.m_ConvertButton.Click += new System.EventHandler(this.OnConvertButtonClick);
		this.m_CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_CancelButton.Location = new System.Drawing.Point(273, 65);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 6;
		this.m_CancelButton.Text = "&Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButtonClick);
		this.m_ToposurfaceNameLabel.AutoSize = true;
		this.m_ToposurfaceNameLabel.Location = new System.Drawing.Point(12, 15);
		this.m_ToposurfaceNameLabel.Name = "m_ToposurfaceNameLabel";
		this.m_ToposurfaceNameLabel.Size = new System.Drawing.Size(101, 13);
		this.m_ToposurfaceNameLabel.TabIndex = 0;
		this.m_ToposurfaceNameLabel.Text = "Toposurface Name:";
		this.m_ToposurfaceNameTextBox.Location = new System.Drawing.Point(151, 12);
		this.m_ToposurfaceNameTextBox.Name = "m_ToposurfaceNameTextBox";
		this.m_ToposurfaceNameTextBox.Size = new System.Drawing.Size(197, 20);
		this.m_ToposurfaceNameTextBox.TabIndex = 1;
		this.m_PickButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.m_PickButton.Location = new System.Drawing.Point(119, 65);
		this.m_PickButton.Name = "m_PickButton";
		this.m_PickButton.Size = new System.Drawing.Size(75, 23);
		this.m_PickButton.TabIndex = 4;
		this.m_PickButton.Text = "&Pick";
		this.m_PickButton.UseVisualStyleBackColor = true;
		this.m_PickButton.Click += new System.EventHandler(this.OnPickButtonClick);
		this.m_PhaseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_PhaseComboBox.FormattingEnabled = true;
		this.m_PhaseComboBox.Location = new System.Drawing.Point(151, 38);
		this.m_PhaseComboBox.Name = "m_PhaseComboBox";
		this.m_PhaseComboBox.Size = new System.Drawing.Size(197, 21);
		this.m_PhaseComboBox.TabIndex = 3;
		this.m_PhaseLabel.AutoSize = true;
		this.m_PhaseLabel.Location = new System.Drawing.Point(12, 41);
		this.m_PhaseLabel.Name = "m_PhaseLabel";
		this.m_PhaseLabel.Size = new System.Drawing.Size(37, 13);
		this.m_PhaseLabel.TabIndex = 2;
		this.m_PhaseLabel.Text = "Phase";
		this.m_HideOriginalCheckBox.Location = new System.Drawing.Point(15, 57);
		this.m_HideOriginalCheckBox.Name = "m_HideOriginalCheckBox";
		this.m_HideOriginalCheckBox.Size = new System.Drawing.Size(98, 37);
		this.m_HideOriginalCheckBox.TabIndex = 7;
		this.m_HideOriginalCheckBox.Text = "Hide Original Toposurface";
		this.m_HideOriginalCheckBox.UseVisualStyleBackColor = true;
		base.AcceptButton = this.m_ConvertButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_CancelButton;
		base.ClientSize = new System.Drawing.Size(364, 100);
		base.Controls.Add(this.m_HideOriginalCheckBox);
		base.Controls.Add(this.m_PhaseComboBox);
		base.Controls.Add(this.m_PhaseLabel);
		base.Controls.Add(this.m_PickButton);
		base.Controls.Add(this.m_ToposurfaceNameLabel);
		base.Controls.Add(this.m_ToposurfaceNameTextBox);
		base.Controls.Add(this.m_ConvertButton);
		base.Controls.Add(this.m_CancelButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "ConvertToToposurfaceForm";
		this.Text = "ConvertToToposurfaceForm";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnConvertToToposurfaceFormClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
