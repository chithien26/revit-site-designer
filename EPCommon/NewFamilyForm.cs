using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using SITES.Utilities;

namespace EPCommon;

public class NewFamilyForm : EPForm
{
	private FamilyBinder m_NewFamilyBinder;

	private Document m_CurrentDocument;

	private LandcaddID m_CurrentFamilyType = LandcaddID.Street;

	private string m_strOriginalName = string.Empty;

	private IContainer components;

	private Button m_CancelButton;

	private Button m_OkButton;

	private Button m_FileBtn;

	private Label m_PathLbl;

	private TextBox m_FileTxt;

	public string OriginalFileName => m_strOriginalName;

	public NewFamilyForm(ref FamilyBinder _NewFamily, Document _CurDocument, LandcaddID _CurrentFamilyType)
	{
		InitializeComponent();
		m_NewFamilyBinder = _NewFamily;
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
			ContextualHelpID = "HID_SDfamilyNewStreet";
			break;
		case LandcaddID.Curb:
			text = EPShared.LocalRM.GetString("CurbTypeManagerString");
			ContextualHelpID = "HID_SDfamilyNewCurb";
			break;
		case LandcaddID.Sidewalk:
			text = EPShared.LocalRM.GetString("SidewalkTypeManagerString");
			ContextualHelpID = "HID_SDfamilyNewSidewalk";
			break;
		case LandcaddID.RetainingWall:
			text = EPShared.LocalRM.GetString("RetainingWallManagerString");
			ContextualHelpID = "HID_SDfamilyNewRetainingWall";
			break;
		case LandcaddID.Steps:
			text = EPShared.LocalRM.GetString("StepsManagerString");
			break;
		case LandcaddID.ParkingLot:
			text = EPShared.LocalRM.GetString("ParkingLotManagerString");
			ContextualHelpID = "HID_SDfamilyNewParkinglot";
			break;
		case LandcaddID.SoftTerrain:
			text = EPShared.LocalRM.GetString("SoftTerrainManagerString");
			ContextualHelpID = "HID_SDfamilyNewSoftTerrain";
			break;
		}
		Text = EPShared.LocalRM.GetString("New_") + text + " " + EPShared.LocalRM.GetString("FamilyString");
		m_PathLbl.Text = EPShared.LocalRM.GetString("Family_AMP_Name_NewFamilyDialog");
		GetDialogPosition();
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
			if (m_NewFamilyBinder == null)
			{
				m_NewFamilyBinder = new FamilyBinder();
			}
			string text2 = m_FileTxt.Text.Substring(m_FileTxt.Text.LastIndexOf("\\") + 1);
			text2 = text2.Substring(0, text2.LastIndexOf('.'));
			m_NewFamilyBinder.FamilyName = text2;
			m_NewFamilyBinder.FamilyPath = m_FileTxt.Text;
			m_NewFamilyBinder.IsModified = true;
			DialogResult = DialogResult.OK;
		}
	}

	private bool VerifyFileNameAndPath()
	{
		if (string.IsNullOrEmpty(m_FileTxt.Text))
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidFamilyNameMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
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
		if (File.Exists(m_FileTxt.Text))
		{
			Document document = m_CurrentDocument.Application.OpenDocumentFile(m_FileTxt.Text);
			if (document != null)
			{
				bool flag = false;
				foreach (FamilyType type in document.FamilyManager.Types)
				{
					FamilyParameter familyParameter = document.FamilyManager.get_Parameter(RevitParameters.SiteworksIDFamilyParameter);
					if (familyParameter == null || !type.HasValue(familyParameter) || string.Compare(type.AsString(familyParameter), Convert.ToInt32(m_CurrentFamilyType).ToString("000"), ignoreCase: true) != 0)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					string text = string.Empty;
					switch (m_CurrentFamilyType)
					{
					case LandcaddID.Street:
						text += EPShared.LocalRM.GetString("StreetFamilyManagerRibbonButton");
						break;
					case LandcaddID.Curb:
						text += EPShared.LocalRM.GetString("CurbFamilyManagerRibbonButton");
						break;
					case LandcaddID.Sidewalk:
						text += EPShared.LocalRM.GetString("LocateSidewalkRibbonButton");
						break;
					case LandcaddID.RetainingWall:
						text += EPShared.LocalRM.GetString("LocateRetainingWallRibbonButton");
						break;
					case LandcaddID.Steps:
						text += ApplicationUtility.GetString("");
						break;
					case LandcaddID.ParkingLot:
						text += EPShared.LocalRM.GetString("ParkingLotFamilyManagerRibbonButton");
						break;
					case LandcaddID.SoftTerrain:
						text += EPShared.LocalRM.GetString("ModifySoftTerrainRibbonButton");
						break;
					}
					RevitMessageBox.Show(EPShared.LocalRM.GetString("InValidNewFamily_Message") + text + " " + EPShared.LocalRM.GetString("FamilyString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return false;
				}
			}
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
			if (m_NewFamilyBinder == null)
			{
				m_NewFamilyBinder = new FamilyBinder();
			}
			m_NewFamilyBinder.FamilyPath = m_FileTxt.Text;
			string text = m_FileTxt.Text.Substring(m_FileTxt.Text.LastIndexOf("\\") + 1);
			text = text.Substring(0, text.LastIndexOf('.'));
			m_NewFamilyBinder.IsModified = true;
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
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_OkButton = new System.Windows.Forms.Button();
		this.m_FileBtn = new System.Windows.Forms.Button();
		this.m_PathLbl = new System.Windows.Forms.Label();
		this.m_FileTxt = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.m_CancelButton.Location = new System.Drawing.Point(414, 38);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 4;
		this.m_CancelButton.Text = "Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButtonClicked);
		this.m_OkButton.Location = new System.Drawing.Point(333, 38);
		this.m_OkButton.Name = "m_OkButton";
		this.m_OkButton.Size = new System.Drawing.Size(75, 23);
		this.m_OkButton.TabIndex = 3;
		this.m_OkButton.Text = "Ok";
		this.m_OkButton.UseVisualStyleBackColor = true;
		this.m_OkButton.Click += new System.EventHandler(this.OkButtonClicked);
		this.m_FileBtn.Location = new System.Drawing.Point(466, 12);
		this.m_FileBtn.Name = "m_FileBtn";
		this.m_FileBtn.Size = new System.Drawing.Size(24, 20);
		this.m_FileBtn.TabIndex = 2;
		this.m_FileBtn.Text = "...";
		this.m_FileBtn.UseVisualStyleBackColor = true;
		this.m_FileBtn.Click += new System.EventHandler(this.OnFileBtnClicked);
		this.m_PathLbl.AutoSize = true;
		this.m_PathLbl.Location = new System.Drawing.Point(12, 15);
		this.m_PathLbl.Name = "m_PathLbl";
		this.m_PathLbl.Size = new System.Drawing.Size(67, 13);
		this.m_PathLbl.TabIndex = 0;
		this.m_PathLbl.Text = "Family Name";
		this.m_FileTxt.Location = new System.Drawing.Point(122, 12);
		this.m_FileTxt.Name = "m_FileTxt";
		this.m_FileTxt.Size = new System.Drawing.Size(338, 20);
		this.m_FileTxt.TabIndex = 1;
		base.AcceptButton = this.m_OkButton;
		base.CancelButton = this.m_CancelButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(502, 66);
		base.Controls.Add(this.m_FileBtn);
		base.Controls.Add(this.m_PathLbl);
		base.Controls.Add(this.m_FileTxt);
		base.Controls.Add(this.m_OkButton);
		base.Controls.Add(this.m_CancelButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Name = "NewFamilyForm";
		this.Text = "NewFamilyForm";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
