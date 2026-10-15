using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace EPCommon;

public class CopyFamilyForm : EPForm
{
	private FamilyBinder m_CurrentFamilyBinder;

	private Document m_CurrentDocument;

	private LandcaddID m_CurrentFamilyType = LandcaddID.Street;

	private string m_strNewPath = string.Empty;

	private IContainer components;

	private GroupBox m_AddFamilyGroupBox;

	private Button m_FileBtn;

	private Label m_PathLbl;

	private TextBox m_FileTxt;

	private Button m_CancelButton;

	private Button m_OkButton;

	private Label m_CurrentFamilyTextLbl;

	private Label m_CurrentFamilyNameLbl;

	public string NewPath => m_strNewPath;

	public CopyFamilyForm(FamilyBinder _CurrentFamily, Document _CurDocument, LandcaddID _CurrentFamilyType)
	{
		InitializeComponent();
		m_CurrentFamilyBinder = _CurrentFamily;
		m_CurrentDocument = _CurDocument;
		m_CurrentFamilyType = _CurrentFamilyType;
		m_OkButton.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		m_CancelButton.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		string text = string.Empty;
		switch (m_CurrentFamilyType)
		{
		case LandcaddID.PlantMix:
			text = ApplicationUtility.GetString("");
			break;
		case LandcaddID.PatioWalk:
			text = ApplicationUtility.GetString("");
			break;
		case LandcaddID.Street:
			text = EPShared.LocalRM.GetString("StreetTypeManagerString");
			ContextualHelpID = "HID_SDfamilyCopyStreet";
			break;
		case LandcaddID.Curb:
			text = EPShared.LocalRM.GetString("CurbTypeManagerString");
			ContextualHelpID = "HID_SDfamilyCopyCurb";
			break;
		case LandcaddID.Sidewalk:
			text = EPShared.LocalRM.GetString("SidewalkTypeManagerString");
			ContextualHelpID = "HID_SDfamilyCopySidewalk";
			break;
		case LandcaddID.RetainingWall:
			text = EPShared.LocalRM.GetString("RetainingWallManagerString");
			ContextualHelpID = "HID_SDfamilyCopyRetainingWall";
			break;
		case LandcaddID.Steps:
			text = EPShared.LocalRM.GetString("StepsManagerString");
			break;
		case LandcaddID.ParkingLot:
			text = EPShared.LocalRM.GetString("ParkingLotManagerString");
			ContextualHelpID = "HID_SDfamilyCopyParkinglot";
			break;
		case LandcaddID.SoftTerrain:
			text = EPShared.LocalRM.GetString("SoftTerrainManagerString");
			ContextualHelpID = "HID_SDfamilyCopySoftTerrain";
			break;
		}
		Text = EPShared.LocalRM.GetString("Copy_") + text + " " + EPShared.LocalRM.GetString("FamilyString");
		m_AddFamilyGroupBox.Text = EPShared.LocalRM.GetString("Copy_") + text + EPShared.LocalRM.GetString("_Family_");
		m_PathLbl.Text = EPShared.LocalRM.GetString("Family_AMP_Name_CopyFamilyDialog");
		m_CurrentFamilyNameLbl.Text = EPShared.LocalRM.GetString("CurrentFamilyNameString");
		m_CurrentFamilyTextLbl.Text = _CurrentFamily.FamilyName;
		m_FileTxt.Text = GetPathString(_CurrentFamily.FamilyPath);
		GetDialogPosition();
	}

	private string GetPathString(string _strPath)
	{
		string text = _strPath;
		if (text != string.Empty)
		{
			string fileName = Path.GetFileName(_strPath);
			string directoryName = Path.GetDirectoryName(_strPath);
			text = directoryName + "\\Copy of " + fileName;
		}
		return text;
	}

	private void OkButtonClicked(object sender, EventArgs e)
	{
		if (VerifyFileNameAndPath())
		{
			if (m_FileTxt.Text.LastIndexOf("\\") < 0)
			{
				string _strFullFamilyFilename = string.Empty;
				RevitUtility.GetFullFamilyFilenameIfExist(string.Empty, ref _strFullFamilyFilename);
				string text = m_FileTxt.Text;
				m_FileTxt.Text = _strFullFamilyFilename + text;
			}
			string text2 = m_FileTxt.Text.Substring(m_FileTxt.Text.LastIndexOf("\\") + 1);
			text2 = text2.Substring(0, text2.LastIndexOf('.'));
			m_strNewPath = m_FileTxt.Text;
		}
		DialogResult = DialogResult.OK;
	}

	private bool VerifyFileNameAndPath()
	{
		if (string.IsNullOrEmpty(m_FileTxt.Text))
		{
			return false;
		}
		if (string.IsNullOrEmpty(m_FileTxt.Text))
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("Source") + " " + EPShared.LocalRM.GetString("NoRFAExtension"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return false;
		}
		FileInfo fileInfo = new FileInfo(m_FileTxt.Text);
		if (fileInfo.Extension == string.Empty)
		{
			m_FileTxt.Text += ".rfa";
		}
		return true;
	}

	private void OnCancelButtonClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
	}

	private void OnFileBtnClicked(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.FilterIndex = 1;
		saveFileDialog.RestoreDirectory = true;
		saveFileDialog.Filter = ApplicationUtility.GetString("FamilyFilePickerFilter");
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			m_FileTxt.Text = saveFileDialog.FileName;
			string text = m_FileTxt.Text.Substring(m_FileTxt.Text.LastIndexOf("\\") + 1);
			text = text.Substring(0, text.LastIndexOf('.'));
		}
	}

	private void OnFocusLost(object sender, EventArgs e)
	{
		if (VerifyFileNameAndPath())
		{
			m_strNewPath = m_FileTxt.Text;
		}
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
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
		this.m_AddFamilyGroupBox = new System.Windows.Forms.GroupBox();
		this.m_CurrentFamilyTextLbl = new System.Windows.Forms.Label();
		this.m_CurrentFamilyNameLbl = new System.Windows.Forms.Label();
		this.m_FileBtn = new System.Windows.Forms.Button();
		this.m_PathLbl = new System.Windows.Forms.Label();
		this.m_FileTxt = new System.Windows.Forms.TextBox();
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_OkButton = new System.Windows.Forms.Button();
		this.m_AddFamilyGroupBox.SuspendLayout();
		base.SuspendLayout();
		this.m_AddFamilyGroupBox.Controls.Add(this.m_CurrentFamilyTextLbl);
		this.m_AddFamilyGroupBox.Controls.Add(this.m_CurrentFamilyNameLbl);
		this.m_AddFamilyGroupBox.Controls.Add(this.m_FileBtn);
		this.m_AddFamilyGroupBox.Controls.Add(this.m_PathLbl);
		this.m_AddFamilyGroupBox.Controls.Add(this.m_FileTxt);
		this.m_AddFamilyGroupBox.Location = new System.Drawing.Point(12, 12);
		this.m_AddFamilyGroupBox.Name = "m_AddFamilyGroupBox";
		this.m_AddFamilyGroupBox.Size = new System.Drawing.Size(476, 71);
		this.m_AddFamilyGroupBox.TabIndex = 0;
		this.m_AddFamilyGroupBox.TabStop = false;
		this.m_AddFamilyGroupBox.Text = "Copy Family";
		this.m_CurrentFamilyTextLbl.AutoSize = true;
		this.m_CurrentFamilyTextLbl.Location = new System.Drawing.Point(137, 25);
		this.m_CurrentFamilyTextLbl.Name = "m_CurrentFamilyTextLbl";
		this.m_CurrentFamilyTextLbl.Size = new System.Drawing.Size(35, 13);
		this.m_CurrentFamilyTextLbl.TabIndex = 1;
		this.m_CurrentFamilyTextLbl.Text = "label2";
		this.m_CurrentFamilyNameLbl.AutoSize = true;
		this.m_CurrentFamilyNameLbl.Location = new System.Drawing.Point(6, 25);
		this.m_CurrentFamilyNameLbl.Name = "m_CurrentFamilyNameLbl";
		this.m_CurrentFamilyNameLbl.Size = new System.Drawing.Size(104, 13);
		this.m_CurrentFamilyNameLbl.TabIndex = 0;
		this.m_CurrentFamilyNameLbl.Text = "Current Family Name";
		this.m_FileBtn.Location = new System.Drawing.Point(446, 45);
		this.m_FileBtn.Name = "m_FileBtn";
		this.m_FileBtn.Size = new System.Drawing.Size(24, 20);
		this.m_FileBtn.TabIndex = 4;
		this.m_FileBtn.Text = "...";
		this.m_FileBtn.UseVisualStyleBackColor = true;
		this.m_FileBtn.Click += new System.EventHandler(this.OnFileBtnClicked);
		this.m_PathLbl.AutoSize = true;
		this.m_PathLbl.Location = new System.Drawing.Point(6, 48);
		this.m_PathLbl.Name = "m_PathLbl";
		this.m_PathLbl.Size = new System.Drawing.Size(92, 13);
		this.m_PathLbl.TabIndex = 2;
		this.m_PathLbl.Text = "New Family Name";
		this.m_FileTxt.Location = new System.Drawing.Point(140, 45);
		this.m_FileTxt.Name = "m_FileTxt";
		this.m_FileTxt.Size = new System.Drawing.Size(300, 20);
		this.m_FileTxt.TabIndex = 3;
		this.m_FileTxt.Leave += new System.EventHandler(this.OnFocusLost);
		this.m_CancelButton.Location = new System.Drawing.Point(413, 89);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 2;
		this.m_CancelButton.Text = "Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButtonClicked);
		this.m_OkButton.Location = new System.Drawing.Point(332, 89);
		this.m_OkButton.Name = "m_OkButton";
		this.m_OkButton.Size = new System.Drawing.Size(75, 23);
		this.m_OkButton.TabIndex = 1;
		this.m_OkButton.Text = "Ok";
		this.m_OkButton.UseVisualStyleBackColor = true;
		this.m_OkButton.Click += new System.EventHandler(this.OkButtonClicked);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(502, 120);
		base.Controls.Add(this.m_AddFamilyGroupBox);
		base.Controls.Add(this.m_OkButton);
		base.Controls.Add(this.m_CancelButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "CopyFamilyForm";
		this.Text = "CopyFamilyForm";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
		this.m_AddFamilyGroupBox.ResumeLayout(false);
		this.m_AddFamilyGroupBox.PerformLayout();
		base.ResumeLayout(false);
		base.AcceptButton = this.m_OkButton;
		base.CancelButton = this.m_CancelButton;
	}
}
