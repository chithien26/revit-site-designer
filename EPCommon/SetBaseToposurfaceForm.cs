using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace EPCommon;

public class SetBaseToposurfaceForm : EPForm
{
	private SetBaseToposurface m_dataBuffer;

	private List<TopographySurfaceContainer> m_SelectedSurfaceList;

	private IContainer components;

	private Button m_CancelButton;

	private Button m_OKButton;

	private GroupBox m_ToposurfaceGroupBox;

	private ListView m_ToposurfaceListView;

	private TextBox m_MessageTextBox;

	private Button m_PickButton;

	public List<TopographySurfaceContainer> SelectedSurfaceList => m_SelectedSurfaceList;

	public SetBaseToposurfaceForm(SetBaseToposurface _dataBuffer)
	{
		InitializeComponent();
		if (!_dataBuffer.PickIndividualSurface)
		{
			Text = EPShared.LocalRM.GetString("SetBaseToposurfaceDialog");
			m_MessageTextBox.Text = EPShared.LocalRM.GetString("SetBaseToposurfaceInformationDialog");
		}
		else
		{
			Text = EPShared.LocalRM.GetString("ToposurfaceAssociationDialog");
			m_MessageTextBox.Text = EPShared.LocalRM.GetString("PickToposurfaceInformationDialog");
		}
		m_ToposurfaceListView.MultiSelect = !_dataBuffer.PickIndividualSurface;
		m_ToposurfaceGroupBox.Text = EPShared.LocalRM.GetString("ToposurfaceDialog");
		m_PickButton.Text = EPShared.LocalRM.GetString("AMP_PickDialog");
		m_OKButton.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		m_CancelButton.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		ContextualHelpID = "HID_SDsetBaseTopo";
		m_dataBuffer = _dataBuffer;
		m_SelectedSurfaceList = null;
		InitializeControls();
	}

	private void InitializeControls()
	{
		GetDialogPosition();
		SetupListView();
	}

	private void SetupListView()
	{
		m_ToposurfaceListView.View = System.Windows.Forms.View.Details;
		m_ToposurfaceListView.Columns.Add(EPShared.LocalRM.GetString("SurfaceNameDialog"));
		m_ToposurfaceListView.Columns.Add(EPShared.LocalRM.GetString("PhaseCreatedDialog"));
		int num = (m_ToposurfaceListView.Width - 5) / m_ToposurfaceListView.Columns.Count;
		for (int i = 0; i < m_ToposurfaceListView.Columns.Count; i++)
		{
			m_ToposurfaceListView.Columns[i].Width = num;
		}
		PopulateListView();
		SelectListItems();
		if (m_ToposurfaceListView.Items.Count == 1)
		{
			m_ToposurfaceListView.SelectedIndices.Add(0);
		}
	}

	public void SelectListItems()
	{
		m_ToposurfaceListView.SelectedIndices.Clear();
		foreach (Element item in m_dataBuffer.InstanceSet)
		{
			if (TerrainApi.AsSurface(item) is { Category: not null } topographySurface && topographySurface.Category.Id.Value64() == TerrainApi.Category.GetHashCode())
			{
				int num = m_ToposurfaceListView.Items.IndexOfKey(topographySurface.UniqueId);
				if (num >= 0)
				{
					m_ToposurfaceListView.SelectedIndices.Add(num);
				}
			}
		}
	}

	private void PopulateListView()
	{
		ListViewItem listViewItem = new ListViewItem();
		string text = string.Empty;
		Parameter parameter = null;
		int num = 1;
		m_ToposurfaceListView.Items.Clear();
		for (int i = 0; i < m_dataBuffer.SurfaceList.Count; i++)
		{
			parameter = TerrainApi.NameParameter(((Element)m_dataBuffer.SurfaceList[i].TopographySurface));
			if (parameter != null)
			{
				text = parameter.AsString();
			}
			if (text == null || text == string.Empty)
			{
				text = "<" + EPShared.LocalRM.GetString("UnnamedSurfaceDialog") + " " + Convert.ToString(num) + ">";
				num++;
			}
			listViewItem = m_ToposurfaceListView.Items.Insert(i, m_dataBuffer.SurfaceList[i].TopographySurface.UniqueId, text, string.Empty);
			listViewItem.Tag = m_dataBuffer.SurfaceList[i].TopographySurface.UniqueId;
			parameter = ((Element)m_dataBuffer.SurfaceList[i].TopographySurface).get_Parameter(BuiltInParameter.PHASE_CREATED);
			if (parameter != null)
			{
				foreach (Phase phase in m_dataBuffer.Document.Phases)
				{
					if (phase.Id == parameter.AsElementId())
					{
						text = phase.Name;
						break;
					}
				}
			}
			if (text == null || text == string.Empty)
			{
				text = EPShared.LocalRM.GetString("NotAvailableString");
			}
			listViewItem.SubItems.Add(text);
		}
	}

	private void OnPickButtonClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnCancelButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void OnOKButtonClick(object sender, EventArgs e)
	{
		if (SetSelectedTopoList())
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	private void OnFormLoad(object sender, EventArgs e)
	{
		OverrideKeyUp();
	}

	private bool SetSelectedTopoList()
	{
		m_SelectedSurfaceList = new List<TopographySurfaceContainer>();
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		if ((num = m_ToposurfaceListView.SelectedItems.Count) >= 1)
		{
			for (num2 = 0; num2 < num; num2++)
			{
				for (num3 = 0; num3 < m_dataBuffer.SurfaceList.Count; num3++)
				{
					if (string.Compare((string)m_ToposurfaceListView.SelectedItems[num2].Tag, m_dataBuffer.SurfaceList[num3].TopographySurface.UniqueId, ignoreCase: true) == 0)
					{
						m_dataBuffer.SurfaceList[num3].FindMinMaxPointsOfTopographySurface();
						m_SelectedSurfaceList.Add(m_dataBuffer.SurfaceList[num3]);
						break;
					}
				}
			}
			return true;
		}
		RevitMessageBox.Show(EPShared.LocalRM.GetString("NoSurfacesSelectedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		return false;
	}

	private void OnSetBaseToposurfaceClosing(object sender, FormClosingEventArgs e)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EPCommon.SetBaseToposurfaceForm));
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_OKButton = new System.Windows.Forms.Button();
		this.m_ToposurfaceGroupBox = new System.Windows.Forms.GroupBox();
		this.m_ToposurfaceListView = new System.Windows.Forms.ListView();
		this.m_MessageTextBox = new System.Windows.Forms.TextBox();
		this.m_PickButton = new System.Windows.Forms.Button();
		this.m_ToposurfaceGroupBox.SuspendLayout();
		base.SuspendLayout();
		this.m_CancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_CancelButton.Location = new System.Drawing.Point(313, 312);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 4;
		this.m_CancelButton.Text = "Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButtonClick);
		this.m_OKButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_OKButton.Location = new System.Drawing.Point(230, 312);
		this.m_OKButton.Name = "m_OKButton";
		this.m_OKButton.Size = new System.Drawing.Size(77, 23);
		this.m_OKButton.TabIndex = 3;
		this.m_OKButton.Text = "&OK";
		this.m_OKButton.UseVisualStyleBackColor = true;
		this.m_OKButton.Click += new System.EventHandler(this.OnOKButtonClick);
		this.m_ToposurfaceGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_ToposurfaceGroupBox.Controls.Add(this.m_ToposurfaceListView);
		this.m_ToposurfaceGroupBox.Location = new System.Drawing.Point(12, 78);
		this.m_ToposurfaceGroupBox.Name = "m_ToposurfaceGroupBox";
		this.m_ToposurfaceGroupBox.Size = new System.Drawing.Size(376, 228);
		this.m_ToposurfaceGroupBox.TabIndex = 1;
		this.m_ToposurfaceGroupBox.TabStop = false;
		this.m_ToposurfaceGroupBox.Text = "Toposurface";
		this.m_ToposurfaceListView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_ToposurfaceListView.FullRowSelect = true;
		this.m_ToposurfaceListView.HideSelection = false;
		this.m_ToposurfaceListView.Location = new System.Drawing.Point(6, 19);
		this.m_ToposurfaceListView.Name = "m_ToposurfaceListView";
		this.m_ToposurfaceListView.Size = new System.Drawing.Size(364, 203);
		this.m_ToposurfaceListView.TabIndex = 0;
		this.m_ToposurfaceListView.UseCompatibleStateImageBehavior = false;
		this.m_ToposurfaceListView.DoubleClick += new System.EventHandler(this.OnOKButtonClick);
		this.m_MessageTextBox.BackColor = System.Drawing.SystemColors.Control;
		this.m_MessageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_MessageTextBox.Location = new System.Drawing.Point(12, 12);
		this.m_MessageTextBox.Multiline = true;
		this.m_MessageTextBox.Name = "m_MessageTextBox";
		this.m_MessageTextBox.Size = new System.Drawing.Size(376, 60);
		this.m_MessageTextBox.TabIndex = 0;
		this.m_MessageTextBox.TabStop = false;
		this.m_MessageTextBox.Text = resources.GetString("m_MessageTextBox.Text");
		this.m_PickButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_PickButton.Location = new System.Drawing.Point(149, 312);
		this.m_PickButton.Name = "m_PickButton";
		this.m_PickButton.Size = new System.Drawing.Size(75, 23);
		this.m_PickButton.TabIndex = 2;
		this.m_PickButton.Text = "&Pick";
		this.m_PickButton.UseVisualStyleBackColor = true;
		this.m_PickButton.Click += new System.EventHandler(this.OnPickButtonClicked);
		base.AcceptButton = this.m_OKButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_CancelButton;
		base.ClientSize = new System.Drawing.Size(400, 347);
		base.Controls.Add(this.m_PickButton);
		base.Controls.Add(this.m_MessageTextBox);
		base.Controls.Add(this.m_ToposurfaceGroupBox);
		base.Controls.Add(this.m_CancelButton);
		base.Controls.Add(this.m_OKButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Name = "SetBaseToposurfaceForm";
		this.Text = "Set Base Toposurface";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnSetBaseToposurfaceClosing);
		base.Load += new System.EventHandler(this.OnFormLoad);
		this.m_ToposurfaceGroupBox.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
