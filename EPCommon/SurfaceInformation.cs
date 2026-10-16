using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace EPCommon;

public class SurfaceInformation : EPForm
{
	private LandXMLSurface m_SelectedSurface;

	private Document m_CurrentDocument;

	private List<TopographySurface> m_ExistingTopoSurfaces = new List<TopographySurface>();

	private List<Phase> m_PhaseList = new List<Phase>();

	private EPTransactionContainer m_TransactionConatiner;

	private const string m_strNummericSeperator = " - ";

	private string m_strOverwriteSurfaceID = string.Empty;

	private IContainer components;

	private GroupBox m_SurfaceInfoGroupBox;

	private Button m_ButtonCancel;

	private Button m_ButtonOk;

	private System.Windows.Forms.Panel panel1;

	private RadioButton m_NewSurfaceRadio;

	private ComboBox m_ExistingSurfaceCombo;

	private Label m_ExistingSurfaceLbl;

	private RadioButton m_OverwriteSurfaceRadio;

	private ComboBox m_PhaseComboBox;

	private Label m_PhaseLbl;

	private Label m_NameLbl;

	private TextBox m_NameTextBox;

	private Label m_NumberPntValueLbl;

	private Label m_NumberPntLbl;

	public string SurfaceName => m_SelectedSurface.SurfaceName;

	public Phase Phase => m_PhaseComboBox.SelectedItem as Phase;

	public string OverwriteSurfaceID => m_strOverwriteSurfaceID;

	public SurfaceInformation(Document _CurrentDocument, EPTransactionContainer _TransactionConatiner)
	{
		m_CurrentDocument = _CurrentDocument;
		m_TransactionConatiner = _TransactionConatiner;
		InitializeComponent();
		ContextualHelpID = "HID_SDimportLandXMLinfo";
		LoadExistingSurface();
		SetupStrings();
		GetDialogPosition();
	}

	private void LoadExistingSurface()
	{
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		IList<Element> list = filteredElementCollector.OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).ToElements();
		foreach (Element item3 in list)
		{
			TopographySurface item = TerrainApi.AsSurface(item3);
			m_ExistingTopoSurfaces.Add(item);
		}
		foreach (Phase phase in m_CurrentDocument.Phases)
		{
			m_PhaseList.Add(phase);
		}
	}

	private void SetupStrings()
	{
		m_ButtonCancel.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		m_ButtonOk.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		string empty = string.Empty;
		empty = (Text = EPShared.LocalRM.GetString("ToposurfaceString") + " " + EPShared.LocalRM.GetString("InformationString"));
		m_SurfaceInfoGroupBox.Text = empty;
		m_NewSurfaceRadio.Text = EPShared.LocalRM.GetString("Add_AMP_NewSurfaceSurfaceInfoDialog");
		m_NameLbl.Text = EPShared.LocalRM.GetString("Toposurface_AMP_NameSurfaceInfoDialog");
		m_PhaseLbl.Text = EPShared.LocalRM.GetString("AMP_PhaseCreatedFamilyParameterSurfaceInfoDialog");
		m_OverwriteSurfaceRadio.Text = EPShared.LocalRM.GetString("AMP_OverwriteExistingSurfaceSurfaceInfoDialog");
		m_ExistingSurfaceLbl.Text = EPShared.LocalRM.GetString("AMP_ToposurfaceToOverwriteSurfaceInfoDialog");
		m_NumberPntLbl.Text = EPShared.LocalRM.GetString("NumberOfPntsString");
		m_PhaseComboBox.DataSource = m_PhaseList;
		m_PhaseComboBox.DisplayMember = EPShared.LocalRM.GetString("NameString");
		if (m_PhaseComboBox.Items.Count > 0)
		{
			m_PhaseComboBox.SelectedIndex = 0;
		}
		int num = 1;
		Parameter parameter = null;
		foreach (TopographySurface existingTopoSurface in m_ExistingTopoSurfaces)
		{
			parameter = TerrainApi.NameParameter(((Element)existingTopoSurface));
			if (parameter != null && parameter.AsString() != null && parameter.AsString() != string.Empty)
			{
				m_ExistingSurfaceCombo.Items.Add(parameter.AsString());
				continue;
			}
			m_ExistingSurfaceCombo.Items.Add("<" + EPShared.LocalRM.GetString("UnnamedSurfaceDialog") + " " + Convert.ToString(num) + ">");
			num++;
		}
		if (m_ExistingSurfaceCombo.Items.Count > 0)
		{
			m_ExistingSurfaceCombo.SelectedIndex = 0;
		}
		m_NewSurfaceRadio.Checked = true;
	}

	private void NewSurfaceRadioCheckedChanged(object sender, EventArgs e)
	{
		if (m_NewSurfaceRadio.Checked)
		{
			m_NameTextBox.Enabled = true;
			m_PhaseComboBox.Enabled = true;
			m_ExistingSurfaceCombo.Enabled = false;
		}
		else if (m_OverwriteSurfaceRadio.Checked)
		{
			if (m_ExistingTopoSurfaces.Count <= 0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoExistingSurfacesString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_NewSurfaceRadio.Checked = true;
			}
			else
			{
				m_NameTextBox.Enabled = false;
				m_PhaseComboBox.Enabled = false;
				m_ExistingSurfaceCombo.Enabled = true;
			}
		}
	}

	private void OnOkButtonClicked(object sender, EventArgs e)
	{
		Parameter parameter = null;
		if (m_NewSurfaceRadio.Checked)
		{
			bool flag = false;
			TopographySurface topographySurface = null;
			if (m_NameTextBox.Text == string.Empty)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidSurfaceNameString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			Phase selectedPhase = m_PhaseComboBox.SelectedItem as Phase;
			int nBeginningIndex = 0;
			topographySurface = FindTopographySurfaceStartingNumberName(m_NameTextBox.Text, selectedPhase, ref nBeginningIndex);
			if (topographySurface != null)
			{
				List<string> list = new List<string>();
				list.Add(EPShared.LocalRM.GetString("OverwriteToposurfaceOption"));
				list.Add(EPShared.LocalRM.GetString("CreateNewToposurfaceOption"));
				WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), m_CurrentDocument.PathName, "ImportLandXML", EPShared.LocalRM.GetString("ToposurfaceSameNameAndPhaseMessage"), list, 0, _bDisplayOKCancel: true, _bDisplayDoNotShow: false);
				warningOptionsForm.ShowDialog();
				if (warningOptionsForm.DialogResult != DialogResult.OK)
				{
					return;
				}
				if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("OverwriteToposurfaceOption"), ignoreCase: true) == 0)
				{
					flag = true;
				}
				else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("CreateNewToposurfaceOption"), ignoreCase: true) == 0)
				{
					m_SelectedSurface.SurfaceName = GetNameWithoutSeperator(m_NameTextBox.Text) + " - " + Convert.ToString(nBeginningIndex);
				}
			}
			else
			{
				m_SelectedSurface.SurfaceName = m_NameTextBox.Text;
			}
			m_TransactionConatiner.BeginTransaction();
			if (topographySurface != null && flag)
			{
				m_strOverwriteSurfaceID = topographySurface.UniqueId;
				m_CurrentDocument.Delete(topographySurface.Id);
			}
			m_TransactionConatiner.EndTransaction();
		}
		else if (m_OverwriteSurfaceRadio.Checked)
		{
			m_TransactionConatiner.BeginTransaction();
			Element element = null;
			int num = 1;
			foreach (TopographySurface existingTopoSurface in m_ExistingTopoSurfaces)
			{
				parameter = TerrainApi.NameParameter(((Element)existingTopoSurface));
				if (parameter != null && parameter.AsString() != null && parameter.AsString() != string.Empty)
				{
					if (string.Compare(parameter.AsString(), m_ExistingSurfaceCombo.SelectedItem.ToString(), ignoreCase: true) == 0)
					{
						element = existingTopoSurface;
						break;
					}
					continue;
				}
				if (string.Compare("<" + EPShared.LocalRM.GetString("UnnamedSurfaceDialog") + " " + Convert.ToString(num) + ">", m_ExistingSurfaceCombo.SelectedItem.ToString(), ignoreCase: true) == 0)
				{
					element = existingTopoSurface;
					break;
				}
				num++;
			}
			if (element != null)
			{
				m_strOverwriteSurfaceID = element.UniqueId;
				m_CurrentDocument.Delete(element.Id);
			}
			m_TransactionConatiner.EndTransaction();
		}
		DialogResult = DialogResult.OK;
	}

	public string GetNameWithoutSeperator(string _strValue)
	{
		int num = _strValue.LastIndexOf(" - ");
		if (num >= 0 && num < _strValue.Length)
		{
			string text = _strValue.Substring(num);
			if (text.StartsWith(" - ") && text.Length > " - ".Length)
			{
				bool flag = true;
				string text2 = text.Substring(" - ".Length);
				for (int i = 0; i < text2.Length; i++)
				{
					if (!char.IsDigit(text2[i]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					_strValue = _strValue.Substring(0, num);
				}
			}
		}
		return _strValue;
	}

	public TopographySurface FindTopographySurfaceStartingNumberName(string _strFindName, Phase SelectedPhase, ref int nBeginningIndex)
	{
		TopographySurface topographySurface = null;
		Parameter parameter = null;
		TopographySurface result = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		List<Element> list = filteredElementCollector.OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).ToList();
		nBeginningIndex = 0;
		int num = -1;
		bool flag = true;
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		string nameWithoutSeperator = GetNameWithoutSeperator(_strFindName);
		int num2 = 1;
		foreach (Element item in list)
		{
			if (!(TerrainApi.AsSurface(item) is {} topographySurface2))
			{
				continue;
			}
			Parameter parameter2 = ((Element)topographySurface2).get_Parameter(BuiltInParameter.PHASE_CREATED);
			if (parameter2 == null || !(parameter2.AsElementId() == SelectedPhase.Id))
			{
				continue;
			}
			parameter = TerrainApi.NameParameter(((Element)topographySurface2));
			empty3 = string.Empty;
			if (parameter != null && parameter.AsString() != null && parameter.AsString() != string.Empty)
			{
				empty3 = parameter.AsString();
			}
			else
			{
				empty3 = "<" + EPShared.LocalRM.GetString("UnnamedSurfaceDialog") + " " + Convert.ToString(num2) + ">";
				num2++;
			}
			if (!(empty3 != string.Empty) || !empty3.ToUpper().StartsWith(nameWithoutSeperator.ToUpper()))
			{
				continue;
			}
			int num3 = empty3.LastIndexOf(" - ");
			if (num3 >= 0 && num3 < empty3.Length)
			{
				empty = empty3.Substring(num3);
				if (!empty.StartsWith(" - ") || empty.Length <= " - ".Length)
				{
					continue;
				}
				empty2 = empty.Substring(" - ".Length);
				for (int i = 0; i < empty2.Length; i++)
				{
					if (!char.IsDigit(empty2[i]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					num = Convert.ToInt32(empty2);
					if (num > nBeginningIndex)
					{
						nBeginningIndex = num;
					}
				}
				else
				{
					flag = true;
				}
			}
			else if (string.Compare(empty3, _strFindName, ignoreCase: true) == 0)
			{
				result = topographySurface2;
			}
		}
		nBeginningIndex++;
		return result;
	}

	private void OnCancelButtonClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
	}

	private void OnSurfaceInformationClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private void UpdateSurfaceDisplayInformation()
	{
		m_NumberPntValueLbl.Text = m_SelectedSurface.SurfacePointCount.ToString();
		m_NameTextBox.Text = m_SelectedSurface.SurfaceName;
	}

	public void SetCurrentSurface(LandXMLSurface _SelectedSurface)
	{
		m_SelectedSurface = _SelectedSurface;
		UpdateSurfaceDisplayInformation();
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
		this.m_SurfaceInfoGroupBox = new System.Windows.Forms.GroupBox();
		this.m_NumberPntValueLbl = new System.Windows.Forms.Label();
		this.m_NumberPntLbl = new System.Windows.Forms.Label();
		this.m_ButtonCancel = new System.Windows.Forms.Button();
		this.m_ButtonOk = new System.Windows.Forms.Button();
		this.panel1 = new System.Windows.Forms.Panel();
		this.m_ExistingSurfaceCombo = new System.Windows.Forms.ComboBox();
		this.m_ExistingSurfaceLbl = new System.Windows.Forms.Label();
		this.m_OverwriteSurfaceRadio = new System.Windows.Forms.RadioButton();
		this.m_PhaseComboBox = new System.Windows.Forms.ComboBox();
		this.m_PhaseLbl = new System.Windows.Forms.Label();
		this.m_NameLbl = new System.Windows.Forms.Label();
		this.m_NameTextBox = new System.Windows.Forms.TextBox();
		this.m_NewSurfaceRadio = new System.Windows.Forms.RadioButton();
		this.m_SurfaceInfoGroupBox.SuspendLayout();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.m_SurfaceInfoGroupBox.Controls.Add(this.m_NumberPntValueLbl);
		this.m_SurfaceInfoGroupBox.Controls.Add(this.m_NumberPntLbl);
		this.m_SurfaceInfoGroupBox.Location = new System.Drawing.Point(13, 8);
		this.m_SurfaceInfoGroupBox.Name = "m_SurfaceInfoGroupBox";
		this.m_SurfaceInfoGroupBox.Size = new System.Drawing.Size(455, 49);
		this.m_SurfaceInfoGroupBox.TabIndex = 0;
		this.m_SurfaceInfoGroupBox.TabStop = false;
		this.m_SurfaceInfoGroupBox.Text = "groupBox1";
		this.m_NumberPntValueLbl.AutoSize = true;
		this.m_NumberPntValueLbl.Location = new System.Drawing.Point(120, 20);
		this.m_NumberPntValueLbl.Name = "m_NumberPntValueLbl";
		this.m_NumberPntValueLbl.Size = new System.Drawing.Size(35, 13);
		this.m_NumberPntValueLbl.TabIndex = 1;
		this.m_NumberPntValueLbl.Text = "label3";
		this.m_NumberPntLbl.AutoSize = true;
		this.m_NumberPntLbl.Location = new System.Drawing.Point(6, 20);
		this.m_NumberPntLbl.Name = "m_NumberPntLbl";
		this.m_NumberPntLbl.Size = new System.Drawing.Size(88, 13);
		this.m_NumberPntLbl.TabIndex = 0;
		this.m_NumberPntLbl.Text = "Number of Points";
		this.m_ButtonCancel.Location = new System.Drawing.Point(395, 224);
		this.m_ButtonCancel.Name = "m_ButtonCancel";
		this.m_ButtonCancel.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonCancel.TabIndex = 2;
		this.m_ButtonCancel.Text = "Cancel";
		this.m_ButtonCancel.UseVisualStyleBackColor = true;
		this.m_ButtonCancel.Click += new System.EventHandler(this.OnCancelButtonClicked);
		this.m_ButtonOk.Location = new System.Drawing.Point(314, 224);
		this.m_ButtonOk.Name = "m_ButtonOk";
		this.m_ButtonOk.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonOk.TabIndex = 1;
		this.m_ButtonOk.Text = "Ok";
		this.m_ButtonOk.UseVisualStyleBackColor = true;
		this.m_ButtonOk.Click += new System.EventHandler(this.OnOkButtonClicked);
		this.panel1.Controls.Add(this.m_ExistingSurfaceCombo);
		this.panel1.Controls.Add(this.m_ExistingSurfaceLbl);
		this.panel1.Controls.Add(this.m_OverwriteSurfaceRadio);
		this.panel1.Controls.Add(this.m_PhaseComboBox);
		this.panel1.Controls.Add(this.m_PhaseLbl);
		this.panel1.Controls.Add(this.m_NameLbl);
		this.panel1.Controls.Add(this.m_NameTextBox);
		this.panel1.Controls.Add(this.m_NewSurfaceRadio);
		this.panel1.Location = new System.Drawing.Point(14, 63);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(454, 155);
		this.panel1.TabIndex = 1;
		this.m_ExistingSurfaceCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_ExistingSurfaceCombo.FormattingEnabled = true;
		this.m_ExistingSurfaceCombo.Location = new System.Drawing.Point(190, 120);
		this.m_ExistingSurfaceCombo.Name = "m_ExistingSurfaceCombo";
		this.m_ExistingSurfaceCombo.Size = new System.Drawing.Size(129, 21);
		this.m_ExistingSurfaceCombo.TabIndex = 7;
		this.m_ExistingSurfaceLbl.AutoSize = true;
		this.m_ExistingSurfaceLbl.Location = new System.Drawing.Point(20, 123);
		this.m_ExistingSurfaceLbl.Name = "m_ExistingSurfaceLbl";
		this.m_ExistingSurfaceLbl.Size = new System.Drawing.Size(160, 13);
		this.m_ExistingSurfaceLbl.TabIndex = 6;
		this.m_ExistingSurfaceLbl.Text = "Select Toposurface to Overwrite";
		this.m_OverwriteSurfaceRadio.AutoSize = true;
		this.m_OverwriteSurfaceRadio.Location = new System.Drawing.Point(4, 97);
		this.m_OverwriteSurfaceRadio.Name = "m_OverwriteSurfaceRadio";
		this.m_OverwriteSurfaceRadio.Size = new System.Drawing.Size(149, 17);
		this.m_OverwriteSurfaceRadio.TabIndex = 5;
		this.m_OverwriteSurfaceRadio.TabStop = true;
		this.m_OverwriteSurfaceRadio.Text = "Overwrite Existing Surface";
		this.m_OverwriteSurfaceRadio.UseVisualStyleBackColor = true;
		this.m_PhaseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_PhaseComboBox.FormattingEnabled = true;
		this.m_PhaseComboBox.Location = new System.Drawing.Point(190, 53);
		this.m_PhaseComboBox.Name = "m_PhaseComboBox";
		this.m_PhaseComboBox.Size = new System.Drawing.Size(129, 21);
		this.m_PhaseComboBox.TabIndex = 4;
		this.m_PhaseLbl.AutoSize = true;
		this.m_PhaseLbl.Location = new System.Drawing.Point(20, 56);
		this.m_PhaseLbl.Name = "m_PhaseLbl";
		this.m_PhaseLbl.Size = new System.Drawing.Size(37, 13);
		this.m_PhaseLbl.TabIndex = 3;
		this.m_PhaseLbl.Text = "Phase";
		this.m_NameLbl.AutoSize = true;
		this.m_NameLbl.Location = new System.Drawing.Point(20, 29);
		this.m_NameLbl.Name = "m_NameLbl";
		this.m_NameLbl.Size = new System.Drawing.Size(35, 13);
		this.m_NameLbl.TabIndex = 1;
		this.m_NameLbl.Text = "Name";
		this.m_NameTextBox.Location = new System.Drawing.Point(190, 26);
		this.m_NameTextBox.Name = "m_NameTextBox";
		this.m_NameTextBox.Size = new System.Drawing.Size(261, 20);
		this.m_NameTextBox.TabIndex = 2;
		this.m_NewSurfaceRadio.AutoSize = true;
		this.m_NewSurfaceRadio.Location = new System.Drawing.Point(4, 4);
		this.m_NewSurfaceRadio.Name = "m_NewSurfaceRadio";
		this.m_NewSurfaceRadio.Size = new System.Drawing.Size(132, 17);
		this.m_NewSurfaceRadio.TabIndex = 0;
		this.m_NewSurfaceRadio.TabStop = true;
		this.m_NewSurfaceRadio.Text = "Add as a New Surface";
		this.m_NewSurfaceRadio.UseVisualStyleBackColor = true;
		this.m_NewSurfaceRadio.CheckedChanged += new System.EventHandler(this.NewSurfaceRadioCheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(484, 257);
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.m_ButtonOk);
		base.Controls.Add(this.m_ButtonCancel);
		base.Controls.Add(this.m_SurfaceInfoGroupBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.MinimumSize = new System.Drawing.Size(490, 289);
		base.Name = "SurfaceInformation";
		this.Text = "SurfaceInformation";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnSurfaceInformationClosing);
		this.m_SurfaceInfoGroupBox.ResumeLayout(false);
		this.m_SurfaceInfoGroupBox.PerformLayout();
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		base.ResumeLayout(false);
		base.AcceptButton = this.m_ButtonOk;
		base.CancelButton = this.m_ButtonCancel;
	}
}
