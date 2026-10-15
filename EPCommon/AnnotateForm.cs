using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace EPCommon;

public class AnnotateForm : EPForm
{
	private Annotation m_dataBuffer;

	private IContainer components;

	private Button m_CancelButton;

	private Button m_OKButton;

	private CheckBox m_AnnotateElevCheckBox;

	private CheckBox m_AnnotateGradeCheckBox;

	private ComboBox m_TextTypeComboBox;

	private Label m_TextTypeLabel;

	private CheckBox m_BySharedCoordinatesCheckBox;

	public bool AnnotateElevations => m_AnnotateElevCheckBox.Checked;

	public bool AnnotateGrades => m_AnnotateGradeCheckBox.Checked;

	public TextNoteType TextNoteType => m_TextTypeComboBox.SelectedItem as TextNoteType;

	public AnnotateForm(Annotation _dataBuffer)
	{
		InitializeComponent();
		ContextualHelpID = "HID_SDannotate";
		m_dataBuffer = _dataBuffer;
		SetupString();
		InitializeControls();
	}

	private void SetupString()
	{
		Text = EPShared.LocalRM.GetString("AnnotateTitleDialog");
		m_OKButton.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		m_CancelButton.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		m_AnnotateElevCheckBox.Text = EPShared.LocalRM.GetString("AMP_ElevationsDialog");
		m_AnnotateGradeCheckBox.Text = EPShared.LocalRM.GetString("AMP_GradesDialog");
		m_TextTypeLabel.Text = EPShared.LocalRM.GetString("AMP_TextTypeDialog");
		m_BySharedCoordinatesCheckBox.Text = EPShared.LocalRM.GetString("B_AMP_ySurveyPointsDialog");
	}

	private void InitializeControls()
	{
		int _nReturnValue = 0;
		string _strReturnValue = string.Empty;
		GetDialogPosition();
		m_TextTypeComboBox.DataSource = m_dataBuffer.TextTypeList;
		m_TextTypeComboBox.DisplayMember = "Name";
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_dataBuffer.Document.PathName, "AnnotationTextTypeName", ref _strReturnValue))
		{
			TextNoteType textNoteType = null;
			for (int i = 0; i < m_TextTypeComboBox.Items.Count; i++)
			{
				if (m_TextTypeComboBox.Items[i] is TextNoteType textNoteType2 && string.Compare(textNoteType2.Name, _strReturnValue, ignoreCase: true) == 0)
				{
					m_TextTypeComboBox.SelectedIndex = i;
					break;
				}
			}
		}
		m_AnnotateElevCheckBox.Checked = false;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("AnnotateElevations", ref _nReturnValue))
		{
			m_AnnotateElevCheckBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
		m_AnnotateGradeCheckBox.Checked = false;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("AnnotateGrades", ref _nReturnValue))
		{
			m_AnnotateGradeCheckBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
		m_BySharedCoordinatesCheckBox.Checked = false;
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_dataBuffer.Document.PathName, "AnnotateUsingSharedCoordinates", ref _nReturnValue))
		{
			m_BySharedCoordinatesCheckBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
		else if (ApplicationUtility.Settings.eplSetE_GetSetting(m_dataBuffer.Document.PathName, "UseSharedElevation", ref _nReturnValue))
		{
			m_BySharedCoordinatesCheckBox.Checked = Convert.ToBoolean(_nReturnValue);
			ApplicationUtility.Settings.eplSetE_SetSetting(m_dataBuffer.Document.PathName, "AnnotateUsingSharedCoordinates", Convert.ToInt32(m_BySharedCoordinatesCheckBox.Checked));
		}
	}

	private void SaveSettings()
	{
		if (m_TextTypeComboBox.SelectedItem is TextNoteType textNoteType)
		{
			ApplicationUtility.Settings.eplSetE_SetSetting(m_dataBuffer.Document.PathName, "AnnotationTextTypeName", textNoteType.Name);
		}
		ApplicationUtility.Settings.eplSetE_SetSetting("AnnotateElevations", Convert.ToInt32(m_AnnotateElevCheckBox.Checked));
		ApplicationUtility.Settings.eplSetE_SetSetting("AnnotateGrades", Convert.ToInt32(m_AnnotateGradeCheckBox.Checked));
		ApplicationUtility.Settings.eplSetE_SetSetting(m_dataBuffer.Document.PathName, "AnnotateUsingSharedCoordinates", Convert.ToInt32(m_BySharedCoordinatesCheckBox.Checked));
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private bool CheckBoxStatus()
	{
		if (!m_AnnotateElevCheckBox.Checked && !m_AnnotateGradeCheckBox.Checked)
		{
			return false;
		}
		return true;
	}

	private void OnOkButtonClicked(object sender, EventArgs e)
	{
		if (!CheckBoxStatus())
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NoOptionsSelectedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return;
		}
		SaveSettings();
		DialogResult = DialogResult.OK;
	}

	private void OnCancelButtonClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
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
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_OKButton = new System.Windows.Forms.Button();
		this.m_AnnotateElevCheckBox = new System.Windows.Forms.CheckBox();
		this.m_AnnotateGradeCheckBox = new System.Windows.Forms.CheckBox();
		this.m_TextTypeComboBox = new System.Windows.Forms.ComboBox();
		this.m_TextTypeLabel = new System.Windows.Forms.Label();
		this.m_BySharedCoordinatesCheckBox = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.m_CancelButton.Location = new System.Drawing.Point(207, 108);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 6;
		this.m_CancelButton.Text = "Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButtonClicked);
		this.m_OKButton.Location = new System.Drawing.Point(126, 108);
		this.m_OKButton.Name = "m_OKButton";
		this.m_OKButton.Size = new System.Drawing.Size(75, 23);
		this.m_OKButton.TabIndex = 5;
		this.m_OKButton.Text = "Ok";
		this.m_OKButton.UseVisualStyleBackColor = true;
		this.m_OKButton.Click += new System.EventHandler(this.OnOkButtonClicked);
		this.m_AnnotateElevCheckBox.AutoSize = true;
		this.m_AnnotateElevCheckBox.Location = new System.Drawing.Point(12, 62);
		this.m_AnnotateElevCheckBox.Name = "m_AnnotateElevCheckBox";
		this.m_AnnotateElevCheckBox.Size = new System.Drawing.Size(121, 17);
		this.m_AnnotateElevCheckBox.TabIndex = 3;
		this.m_AnnotateElevCheckBox.Text = "Annotate Elevations";
		this.m_AnnotateElevCheckBox.UseVisualStyleBackColor = true;
		this.m_AnnotateGradeCheckBox.AutoSize = true;
		this.m_AnnotateGradeCheckBox.Location = new System.Drawing.Point(12, 39);
		this.m_AnnotateGradeCheckBox.Name = "m_AnnotateGradeCheckBox";
		this.m_AnnotateGradeCheckBox.Size = new System.Drawing.Size(101, 17);
		this.m_AnnotateGradeCheckBox.TabIndex = 2;
		this.m_AnnotateGradeCheckBox.Text = "Annotate Grade";
		this.m_AnnotateGradeCheckBox.UseVisualStyleBackColor = true;
		this.m_TextTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_TextTypeComboBox.FormattingEnabled = true;
		this.m_TextTypeComboBox.Location = new System.Drawing.Point(130, 12);
		this.m_TextTypeComboBox.Name = "m_TextTypeComboBox";
		this.m_TextTypeComboBox.Size = new System.Drawing.Size(152, 21);
		this.m_TextTypeComboBox.TabIndex = 1;
		this.m_TextTypeLabel.AutoSize = true;
		this.m_TextTypeLabel.Location = new System.Drawing.Point(12, 16);
		this.m_TextTypeLabel.Name = "m_TextTypeLabel";
		this.m_TextTypeLabel.Size = new System.Drawing.Size(58, 13);
		this.m_TextTypeLabel.TabIndex = 0;
		this.m_TextTypeLabel.Text = "&Text Type:";
		this.m_BySharedCoordinatesCheckBox.AutoSize = true;
		this.m_BySharedCoordinatesCheckBox.Location = new System.Drawing.Point(12, 85);
		this.m_BySharedCoordinatesCheckBox.Name = "m_UseSharedCoordinatesCheckBox";
		this.m_BySharedCoordinatesCheckBox.Size = new System.Drawing.Size(141, 17);
		this.m_BySharedCoordinatesCheckBox.TabIndex = 4;
		this.m_BySharedCoordinatesCheckBox.Text = "Use Shared Coordinates";
		this.m_BySharedCoordinatesCheckBox.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(294, 143);
		base.Controls.Add(this.m_BySharedCoordinatesCheckBox);
		base.Controls.Add(this.m_TextTypeComboBox);
		base.Controls.Add(this.m_TextTypeLabel);
		base.Controls.Add(this.m_AnnotateElevCheckBox);
		base.Controls.Add(this.m_AnnotateGradeCheckBox);
		base.Controls.Add(this.m_OKButton);
		base.Controls.Add(this.m_CancelButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Name = "AnnotateForm";
		this.Text = "Annotate Site Components";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_OKButton;
		base.CancelButton = this.m_CancelButton;
	}
}
