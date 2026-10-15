using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using SITES.Utilities;

namespace EPCommon;

public class ChangeFamilyTypeForm : EPForm
{
	private Document m_Document;

	private List<FamilyContainer> m_FamilyList;

	private LandcaddID m_CurrentLandcaddId = LandcaddID.SoftTerrain;

	private EPTransactionContainer m_TransactionContainer;

	private IContainer components;

	private Button m_OKButton;

	private Button m_CancelButton;

	private Label m_CurrentTypeLabel;

	private Label m_CurrentFamilyLabel;

	private Label m_CurrentFamilyValueLabel;

	private Button m_LoadFamilyButton;

	private ComboBox m_NewTypeComboBox;

	private Label m_NewTypeLabel;

	private ComboBox m_NewFamilyComboBox;

	private Label m_NewFamilyLabel;

	private Label m_CurrentTypeValueLabel;

	public FamilySymbol NewTypeValue
	{
		get
		{
			if (m_NewTypeComboBox.SelectedItem != null)
			{
				return m_NewTypeComboBox.SelectedItem as FamilySymbol;
			}
			return null;
		}
	}

	public ChangeFamilyTypeForm(Document _Document, EPTransactionContainer _TransactionContainer, string _strCurrentFamilyValue, string _strCurrentTypeValue, LandcaddID _CurrentLandcaddID)
	{
		InitializeComponent();
		Text = EPShared.LocalRM.GetString("ChangeFamilyTypeDialog");
		m_CurrentFamilyLabel.Text = EPShared.LocalRM.GetString("CurrentFamilyDialog");
		m_CurrentTypeLabel.Text = EPShared.LocalRM.GetString("CurrentTypeDialog");
		m_NewFamilyLabel.Text = EPShared.LocalRM.GetString("AMP_NewFamilyDialog");
		m_NewTypeLabel.Text = EPShared.LocalRM.GetString("New_AMP_TypeDialog");
		m_CurrentFamilyValueLabel.Text = _strCurrentFamilyValue;
		m_CurrentTypeValueLabel.Text = _strCurrentTypeValue;
		if (_strCurrentFamilyValue == string.Empty)
		{
			m_CurrentFamilyLabel.Enabled = false;
		}
		if (_strCurrentTypeValue == string.Empty)
		{
			m_CurrentTypeLabel.Enabled = false;
		}
		ContextualHelpID = "HID_SDchangeFamilyType";
		m_FamilyList = new List<FamilyContainer>();
		m_Document = _Document;
		m_TransactionContainer = _TransactionContainer;
		m_CurrentLandcaddId = _CurrentLandcaddID;
		InitializeControls();
	}

	private void InitializeControls()
	{
		GetDialogPosition();
		PopulateFamilyList();
		m_NewFamilyComboBox.DataSource = m_FamilyList;
		m_NewFamilyComboBox.DisplayMember = EPShared.LocalRM.GetString("FamilyNameString");
		FamilyContainer familyContainer = null;
		for (int i = 0; i < m_NewFamilyComboBox.Items.Count; i++)
		{
			if (m_NewFamilyComboBox.Items[i] is FamilyContainer familyContainer2 && string.Compare(familyContainer2.FamilyName, m_CurrentFamilyValueLabel.Text, ignoreCase: true) == 0)
			{
				m_NewFamilyComboBox.SelectedIndex = i;
				break;
			}
		}
		FamilySymbol familySymbol = null;
		for (int j = 0; j < m_NewTypeComboBox.Items.Count; j++)
		{
			if (m_NewTypeComboBox.Items[j] is FamilySymbol familySymbol2 && string.Compare(familySymbol2.Name, m_CurrentTypeValueLabel.Text, ignoreCase: true) == 0)
			{
				m_NewTypeComboBox.SelectedIndex = j;
				break;
			}
		}
	}

	private void PopulateFamilyList()
	{
		int num = 0;
		m_FamilyList.Clear();
		Parameter CurSiteworksID = null;
		FamilySymbol familySymbol = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (CurSiteworksID = CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(m_CurrentLandcaddId).ToString("000"), ignoreCase: true) == 0);
		List<Element> list = source.ToList();
		familySymbol = null;
		foreach (Element item in list)
		{
			if (!(item is FamilySymbol { Category: not null } familySymbol2))
			{
				continue;
			}
			CurSiteworksID = familySymbol2.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (CurSiteworksID == null || string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(m_CurrentLandcaddId).ToString("000"), ignoreCase: true) != 0)
			{
				continue;
			}
			bool flag = false;
			for (num = 0; num < m_FamilyList.Count; num++)
			{
				if (string.Compare(m_FamilyList[num].FamilyName, familySymbol2.Family.Name, ignoreCase: true) == 0)
				{
					m_FamilyList[num].FamilySymbolList.Add(familySymbol2);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				FamilyContainer familyContainer = new FamilyContainer(familySymbol2.Family.Name);
				familyContainer.FamilySymbolList.Add(familySymbol2);
				m_FamilyList.Add(familyContainer);
			}
		}
		FamilySymbolComparer comparer = new FamilySymbolComparer();
		for (num = 0; num < m_FamilyList.Count; num++)
		{
			m_FamilyList[num].FamilySymbolList.Sort(comparer);
		}
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

	private void OnChangeValueClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private void OnLoadFamilyButtonClick(object sender, EventArgs e)
	{
		int count = m_NewFamilyComboBox.Items.Count;
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		string _strLibraryPath = string.Empty;
		FamilyContainer familyContainer = null;
		Family _Family = null;
		openFileDialog.Title = EPShared.LocalRM.GetString("PleaseSelectSoftTerrainFamilyFileMessage");
		openFileDialog.RestoreDirectory = true;
		openFileDialog.Filter = ApplicationUtility.GetString("FamilyFilePickerFilter");
		if (ApplicationUtility.RegistryPaths.GetLibraryPath(string.Empty, ref _strLibraryPath) == 0)
		{
			openFileDialog.InitialDirectory = _strLibraryPath;
		}
		bool flag = false;
		DialogResult dialogResult = DialogResult.OK;
		while (dialogResult != DialogResult.Cancel)
		{
			dialogResult = openFileDialog.ShowDialog();
			if (dialogResult != DialogResult.Cancel)
			{
				try
				{
					m_TransactionContainer.Start(EPShared.LocalRM.GetString("LoadSoftTerrainFamilyTransaction") + openFileDialog.SafeFileName);
					flag = RevitUtility.LoadFamily(m_Document, openFileDialog.FileName, out _Family, _bUseFailureHandling: true, ref m_TransactionContainer);
					m_TransactionContainer.Commit();
				}
				catch (SEHException)
				{
					m_TransactionContainer.Rollback();
					flag = false;
					RevitMessageBox.Show(EPShared.LocalRM.GetString("SoftTerrainFamilyCantBeLoadedMessage") + Convert.ToInt32(m_CurrentLandcaddId).ToString("000") + EPShared.LocalRM.GetString("TypeParameterFamilyFileMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					continue;
				}
				break;
			}
		}
		if (!flag)
		{
			if (count == 0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("SoftTerrainFamilyNotLoadedWithIDMessage") + Convert.ToInt32(m_CurrentLandcaddId).ToString("000") + EPShared.LocalRM.GetString("TypeParameterFamilyNotLoadedMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
			}
			return;
		}
		PopulateFamilyList();
		m_NewFamilyComboBox.DataSource = m_FamilyList;
		m_NewFamilyComboBox.DisplayMember = EPShared.LocalRM.GetString("FamilyNameString");
		if (_Family != null)
		{
			for (int i = 0; i < m_NewFamilyComboBox.Items.Count; i++)
			{
				if (m_NewFamilyComboBox.Items[i] is FamilyContainer familyContainer2 && string.Compare(familyContainer2.FamilyName, _Family.Name, ignoreCase: true) == 0)
				{
					m_NewFamilyComboBox.SelectedIndex = i;
					break;
				}
			}
		}
		if (m_NewFamilyComboBox.Items.Count == count)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("FamilyNotSoftTerrainMessage") + Convert.ToInt32(m_CurrentLandcaddId).ToString("000") + EPShared.LocalRM.GetString("TypeParameterFamilyMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
		}
	}

	private void OnFamilyComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_NewFamilyComboBox.SelectedItem is FamilyContainer familyContainer)
		{
			m_NewTypeComboBox.DataSource = familyContainer.FamilySymbolList;
			m_NewTypeComboBox.DisplayMember = EPShared.LocalRM.GetString("NameString");
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EPCommon.ChangeFamilyTypeForm));
		this.m_OKButton = new System.Windows.Forms.Button();
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_CurrentTypeLabel = new System.Windows.Forms.Label();
		this.m_CurrentFamilyLabel = new System.Windows.Forms.Label();
		this.m_CurrentFamilyValueLabel = new System.Windows.Forms.Label();
		this.m_LoadFamilyButton = new System.Windows.Forms.Button();
		this.m_NewTypeComboBox = new System.Windows.Forms.ComboBox();
		this.m_NewTypeLabel = new System.Windows.Forms.Label();
		this.m_NewFamilyComboBox = new System.Windows.Forms.ComboBox();
		this.m_NewFamilyLabel = new System.Windows.Forms.Label();
		this.m_CurrentTypeValueLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_OKButton.Location = new System.Drawing.Point(222, 118);
		this.m_OKButton.Name = "m_OKButton";
		this.m_OKButton.Size = new System.Drawing.Size(75, 23);
		this.m_OKButton.TabIndex = 9;
		this.m_OKButton.Text = "&OK";
		this.m_OKButton.UseVisualStyleBackColor = true;
		this.m_OKButton.Click += new System.EventHandler(this.OnOKButtonClick);
		this.m_CancelButton.Location = new System.Drawing.Point(303, 118);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 10;
		this.m_CancelButton.Text = "&Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButtonClick);
		this.m_CurrentTypeLabel.AutoSize = true;
		this.m_CurrentTypeLabel.Location = new System.Drawing.Point(12, 41);
		this.m_CurrentTypeLabel.Name = "m_CurrentTypeLabel";
		this.m_CurrentTypeLabel.Size = new System.Drawing.Size(71, 13);
		this.m_CurrentTypeLabel.TabIndex = 2;
		this.m_CurrentTypeLabel.Text = "Current Type:";
		this.m_CurrentFamilyLabel.AutoSize = true;
		this.m_CurrentFamilyLabel.Location = new System.Drawing.Point(12, 15);
		this.m_CurrentFamilyLabel.Name = "m_CurrentFamilyLabel";
		this.m_CurrentFamilyLabel.Size = new System.Drawing.Size(76, 13);
		this.m_CurrentFamilyLabel.TabIndex = 0;
		this.m_CurrentFamilyLabel.Text = "Current Family:";
		this.m_CurrentFamilyValueLabel.AutoSize = true;
		this.m_CurrentFamilyValueLabel.Location = new System.Drawing.Point(130, 15);
		this.m_CurrentFamilyValueLabel.Name = "m_CurrentFamilyValueLabel";
		this.m_CurrentFamilyValueLabel.Size = new System.Drawing.Size(34, 13);
		this.m_CurrentFamilyValueLabel.TabIndex = 1;
		this.m_CurrentFamilyValueLabel.Text = "Value";
		this.m_LoadFamilyButton.Location = new System.Drawing.Point(354, 64);
		this.m_LoadFamilyButton.Name = "m_LoadFamilyButton";
		this.m_LoadFamilyButton.Size = new System.Drawing.Size(24, 20);
		this.m_LoadFamilyButton.TabIndex = 6;
		this.m_LoadFamilyButton.TabStop = false;
		this.m_LoadFamilyButton.Text = "...";
		this.m_LoadFamilyButton.UseVisualStyleBackColor = true;
		this.m_LoadFamilyButton.Click += new System.EventHandler(this.OnLoadFamilyButtonClick);
		this.m_NewTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_NewTypeComboBox.FormattingEnabled = true;
		this.m_NewTypeComboBox.Location = new System.Drawing.Point(133, 91);
		this.m_NewTypeComboBox.Name = "m_NewTypeComboBox";
		this.m_NewTypeComboBox.Size = new System.Drawing.Size(215, 21);
		this.m_NewTypeComboBox.TabIndex = 8;
		this.m_NewTypeLabel.AutoSize = true;
		this.m_NewTypeLabel.Location = new System.Drawing.Point(12, 94);
		this.m_NewTypeLabel.Name = "m_NewTypeLabel";
		this.m_NewTypeLabel.Size = new System.Drawing.Size(59, 13);
		this.m_NewTypeLabel.TabIndex = 7;
		this.m_NewTypeLabel.Text = "New Type:";
		this.m_NewFamilyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_NewFamilyComboBox.FormattingEnabled = true;
		this.m_NewFamilyComboBox.Location = new System.Drawing.Point(133, 64);
		this.m_NewFamilyComboBox.Name = "m_NewFamilyComboBox";
		this.m_NewFamilyComboBox.Size = new System.Drawing.Size(215, 21);
		this.m_NewFamilyComboBox.TabIndex = 5;
		this.m_NewFamilyComboBox.SelectedIndexChanged += new System.EventHandler(this.OnFamilyComboBoxSelectedIndexChanged);
		this.m_NewFamilyLabel.AutoSize = true;
		this.m_NewFamilyLabel.Location = new System.Drawing.Point(12, 67);
		this.m_NewFamilyLabel.Name = "m_NewFamilyLabel";
		this.m_NewFamilyLabel.Size = new System.Drawing.Size(64, 13);
		this.m_NewFamilyLabel.TabIndex = 4;
		this.m_NewFamilyLabel.Text = "&New Family:";
		this.m_CurrentTypeValueLabel.AutoSize = true;
		this.m_CurrentTypeValueLabel.Location = new System.Drawing.Point(130, 41);
		this.m_CurrentTypeValueLabel.Name = "m_CurrentTypeValueLabel";
		this.m_CurrentTypeValueLabel.Size = new System.Drawing.Size(34, 13);
		this.m_CurrentTypeValueLabel.TabIndex = 3;
		this.m_CurrentTypeValueLabel.Text = "Value";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(390, 153);
		base.Controls.Add(this.m_CurrentTypeValueLabel);
		base.Controls.Add(this.m_LoadFamilyButton);
		base.Controls.Add(this.m_NewTypeComboBox);
		base.Controls.Add(this.m_NewTypeLabel);
		base.Controls.Add(this.m_NewFamilyComboBox);
		base.Controls.Add(this.m_NewFamilyLabel);
		base.Controls.Add(this.m_CurrentFamilyValueLabel);
		base.Controls.Add(this.m_CurrentFamilyLabel);
		base.Controls.Add(this.m_CurrentTypeLabel);
		base.Controls.Add(this.m_OKButton);
		base.Controls.Add(this.m_CancelButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Icon = (System.Drawing.Icon)EPCommon.SafeResources.GetObject(resources, "$this.Icon");
		base.Name = "ChangeFamilyTypeForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Change Family Type";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnChangeValueClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_OKButton;
		base.CancelButton = this.m_CancelButton;
	}
}
