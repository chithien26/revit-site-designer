using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace EPCommon;

public class LandXMLTypeList : EPForm
{
	private LandXMLElementType m_CurrentType;

	private List<LandXMLSurface> m_SurfaceList = new List<LandXMLSurface>();

	private List<LandXMLSurface> m_SelectedSurfaceList = new List<LandXMLSurface>();

	private List<LandXMLAlignment> m_AlignmentList = new List<LandXMLAlignment>();

	private List<LandXMLAlignment> m_SelectedAlignmentList = new List<LandXMLAlignment>();

	private string m_strCurrentType = string.Empty;

	private Document m_CurrentDocument;

	private int m_nElementCount;

	private List<Autodesk.Revit.DB.View> m_ViewList = new List<Autodesk.Revit.DB.View>();

	private EPTransactionContainer m_TransactionContainer;

	private bool m_bInvalidXMLFile;

	private bool m_bHasSpiral;

	private Eps_Point m_ManualCenterBasePoint = new Eps_Point();

	private LandXMLImportOptions m_CurrentLandXMLOption;

	private LandXMLParser m_XMLParser;

	private IContainer components;

	private ListView m_LandXMLTypeListView;

	private Button m_ButtonOk;

	private Button m_ButtonCancel;

	private ComboBox m_ViewCombo;

	private Label m_LevelLbl;

	private Label m_SelectSurfaceLabel;

	public int ElementCount
	{
		get
		{
			return m_nElementCount;
		}
		set
		{
			m_nElementCount = value;
		}
	}

	public bool InvalidXMLFile => m_bInvalidXMLFile;

	public bool HasSpiral => m_bHasSpiral;

	public List<LandXMLSurface> SelectedSurfaceList => m_SelectedSurfaceList;

	public List<LandXMLAlignment> SelectedAlignmentList => m_SelectedAlignmentList;

	public Autodesk.Revit.DB.View CurrentView => m_ViewCombo.SelectedItem as Autodesk.Revit.DB.View;

	public LandXMLTypeList(LandXMLElementType _CurrentType, Document _CurrentDocument, EPTransactionContainer _TransactionContainer, LandXMLImportOptions _CurrentLandXMLOptions, Eps_Point _ManualCenterBasePoint, LandXMLParser _XMLParser)
	{
		m_CurrentLandXMLOption = _CurrentLandXMLOptions;
		m_ManualCenterBasePoint = _ManualCenterBasePoint;
		m_CurrentType = _CurrentType;
		m_CurrentDocument = _CurrentDocument;
		m_TransactionContainer = _TransactionContainer;
		InitializeComponent();
		ContextualHelpID = "HID_SDimportLandXMLinfo";
		SetupStrings();
		m_XMLParser = _XMLParser;
		if (m_CurrentType == LandXMLElementType.Surface)
		{
			try
			{
				m_bInvalidXMLFile = false;
				m_XMLParser.ParseLandXMLSurfaces();
				m_SurfaceList = m_XMLParser.Surfaces;
				m_nElementCount = m_SurfaceList.Count;
			}
			catch (Exception)
			{
				m_bInvalidXMLFile = true;
			}
		}
		else
		{
			try
			{
				m_bInvalidXMLFile = false;
				m_XMLParser.ParseLandXMLAlignments();
				m_AlignmentList = m_XMLParser.Allignments;
				m_nElementCount = m_AlignmentList.Count;
				m_bHasSpiral = m_XMLParser.HasSpiral;
			}
			catch (Exception)
			{
				m_bInvalidXMLFile = true;
			}
		}
		GetDialogPosition();
		UpdateListView();
	}

	private void SetupStrings()
	{
		m_ButtonCancel.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		m_ButtonOk.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		switch (m_CurrentType)
		{
		case LandXMLElementType.Surface:
			Text = EPShared.LocalRM.GetString("ToposurfaceSelectionDialog");
			m_strCurrentType = EPShared.LocalRM.GetString("ToposurfacesString");
			m_SelectSurfaceLabel.Text = EPShared.LocalRM.GetString("SelectTopoSurfaceLabelString");
			break;
		case LandXMLElementType.Allignment:
			Text = EPShared.LocalRM.GetString("AlignmentSelectionDialog");
			m_strCurrentType = EPShared.LocalRM.GetString("AllignmentString");
			m_SelectSurfaceLabel.Text = EPShared.LocalRM.GetString("SelectTopoAlignmentLabelString");
			break;
		}
		m_LandXMLTypeListView.View = System.Windows.Forms.View.Details;
		m_LandXMLTypeListView.MultiSelect = true;
		List<string> list = new List<string>();
		if (m_CurrentType == LandXMLElementType.Surface)
		{
			list.Add(EPShared.LocalRM.GetString("NameSubRegionParameter"));
			list.Add(EPShared.LocalRM.GetString("DescriptionPlantFamilyParameter"));
			for (int i = 0; i < list.Count; i++)
			{
				m_LandXMLTypeListView.Columns.Add(list[i], (m_LandXMLTypeListView.Width - 6) / 2, HorizontalAlignment.Center);
			}
			m_LandXMLTypeListView.Columns[1].TextAlign = HorizontalAlignment.Left;
			m_LevelLbl.Hide();
			m_ViewCombo.Hide();
			return;
		}
		m_LandXMLTypeListView.Columns.Add(EPShared.LocalRM.GetString("NameSubRegionParameter"));
		m_LandXMLTypeListView.Columns[0].Width = m_LandXMLTypeListView.Width - 4;
		m_LevelLbl.Text = EPShared.LocalRM.GetString("RevitLevelString");
		m_LevelLbl.Show();
		m_ViewCombo.Show();
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		IList<Element> list2 = filteredElementCollector.OfClass(typeof(Autodesk.Revit.DB.View)).ToElements();
		foreach (Autodesk.Revit.DB.View item in list2)
		{
			if (item.GenLevel == null || m_ViewList.Contains(item) || (item.ViewType != ViewType.FloorPlan && item.ViewType != ViewType.EngineeringPlan && item.ViewType != ViewType.AreaPlan) || string.Compare(item.Name, EPShared.LocalRM.GetString("SiteString"), ignoreCase: true) == 0)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < m_ViewList.Count; j++)
			{
				if (string.Compare(m_ViewList[j].GenLevel.UniqueId, item.GenLevel.UniqueId, ignoreCase: true) == 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				m_ViewList.Add(item);
			}
		}
		m_ViewCombo.DataSource = m_ViewList;
		m_ViewCombo.DisplayMember = EPShared.LocalRM.GetString("NameString");
		if (m_ViewList.Count > 0)
		{
			m_ViewCombo.SelectedIndex = 0;
		}
	}

	private void UpdateListView()
	{
		m_LandXMLTypeListView.Items.Clear();
		if (m_CurrentType == LandXMLElementType.Surface)
		{
			foreach (LandXMLSurface surface in m_SurfaceList)
			{
				ListViewItem listViewItem = new ListViewItem();
				listViewItem = m_LandXMLTypeListView.Items.Add(surface.SurfaceName);
				listViewItem.Tag = surface.SurfaceName;
				if (surface.SurfaceDescription == string.Empty)
				{
					listViewItem.SubItems.Add(EPShared.LocalRM.GetString("NotAvailableString"));
				}
				else
				{
					listViewItem.SubItems.Add(surface.SurfaceDescription);
				}
			}
			if (m_LandXMLTypeListView.Items.Count > 0)
			{
				m_LandXMLTypeListView.Items[0].Selected = true;
			}
			return;
		}
		foreach (LandXMLAlignment alignment in m_AlignmentList)
		{
			ListViewItem listViewItem2 = new ListViewItem();
			listViewItem2 = m_LandXMLTypeListView.Items.Add(alignment.Name);
			listViewItem2.Tag = alignment.Name;
		}
		if (m_LandXMLTypeListView.Items.Count > 0)
		{
			m_LandXMLTypeListView.Items[0].Selected = true;
		}
	}

	private bool FindSelectedIndices(ref List<LandXMLSurface> _SelectedSurfaces)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		if ((num = m_LandXMLTypeListView.SelectedItems.Count) >= 1)
		{
			for (num2 = 0; num2 < num; num2++)
			{
				for (num3 = 0; num3 < m_SurfaceList.Count; num3++)
				{
					if (string.Compare((string)m_LandXMLTypeListView.SelectedItems[num2].Tag, m_SurfaceList[num3].SurfaceName, ignoreCase: true) == 0)
					{
						_SelectedSurfaces.Add(m_SurfaceList[num3]);
						break;
					}
				}
			}
			return true;
		}
		string strText = EPShared.LocalRM.GetString("SurfaceNotSelectedString");
		RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		return false;
	}

	private bool FindSelectedIndices(ref List<LandXMLAlignment> _SelectedAlignment)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		if ((num = m_LandXMLTypeListView.SelectedItems.Count) >= 1)
		{
			for (num2 = 0; num2 < num; num2++)
			{
				for (num3 = 0; num3 < m_AlignmentList.Count; num3++)
				{
					if (string.Compare((string)m_LandXMLTypeListView.SelectedItems[num2].Tag, m_AlignmentList[num3].Name, ignoreCase: true) == 0)
					{
						_SelectedAlignment.Add(m_AlignmentList[num3]);
						break;
					}
				}
			}
			return true;
		}
		string strText = EPShared.LocalRM.GetString("AlignmentNotSelectedString");
		RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		return false;
	}

	private void OnButtonOkClicked(object sender, EventArgs e)
	{
		DialogResult dialogResult = DialogResult.Cancel;
		if (m_CurrentType == LandXMLElementType.Surface)
		{
			if (!FindSelectedIndices(ref m_SelectedSurfaceList))
			{
				return;
			}
			using SurfaceInformation surfaceInformation = new SurfaceInformation(m_CurrentDocument, m_TransactionContainer);
			for (int i = 0; i < m_SelectedSurfaceList.Count; i++)
			{
				surfaceInformation.SetCurrentSurface(m_SelectedSurfaceList[i]);
				if (surfaceInformation.ShowDialog() == DialogResult.OK)
				{
					m_SelectedSurfaceList[i].SurfaceName = surfaceInformation.SurfaceName;
					m_SelectedSurfaceList[i].Phase = surfaceInformation.Phase;
					m_SelectedSurfaceList[i].OverwriteSurfaceID = surfaceInformation.OverwriteSurfaceID;
					dialogResult = DialogResult.OK;
				}
				else
				{
					m_SelectedSurfaceList.RemoveAt(i);
					i--;
				}
			}
		}
		else
		{
			if (!FindSelectedIndices(ref m_SelectedAlignmentList))
			{
				return;
			}
			dialogResult = DialogResult.OK;
		}
		DialogResult = dialogResult;
	}

	private void OnButtonCancelClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
	}

	private void OnLandXMLTypeListClosing(object sender, FormClosingEventArgs e)
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
		this.m_LandXMLTypeListView = new System.Windows.Forms.ListView();
		this.m_ButtonOk = new System.Windows.Forms.Button();
		this.m_ButtonCancel = new System.Windows.Forms.Button();
		this.m_ViewCombo = new System.Windows.Forms.ComboBox();
		this.m_LevelLbl = new System.Windows.Forms.Label();
		this.m_SelectSurfaceLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_LandXMLTypeListView.FullRowSelect = true;
		this.m_LandXMLTypeListView.Location = new System.Drawing.Point(14, 35);
		this.m_LandXMLTypeListView.Name = "m_LandXMLTypeListView";
		this.m_LandXMLTypeListView.Size = new System.Drawing.Size(332, 186);
		this.m_LandXMLTypeListView.TabIndex = 1;
		this.m_LandXMLTypeListView.UseCompatibleStateImageBehavior = false;
		this.m_ButtonOk.Location = new System.Drawing.Point(190, 254);
		this.m_ButtonOk.Name = "m_ButtonOk";
		this.m_ButtonOk.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonOk.TabIndex = 4;
		this.m_ButtonOk.Text = "Ok";
		this.m_ButtonOk.UseVisualStyleBackColor = true;
		this.m_ButtonOk.Click += new System.EventHandler(this.OnButtonOkClicked);
		this.m_ButtonCancel.Location = new System.Drawing.Point(271, 254);
		this.m_ButtonCancel.Name = "m_ButtonCancel";
		this.m_ButtonCancel.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonCancel.TabIndex = 5;
		this.m_ButtonCancel.Text = "Cancel";
		this.m_ButtonCancel.UseVisualStyleBackColor = true;
		this.m_ButtonCancel.Click += new System.EventHandler(this.OnButtonCancelClicked);
		this.m_ViewCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_ViewCombo.FormattingEnabled = true;
		this.m_ViewCombo.Location = new System.Drawing.Point(101, 230);
		this.m_ViewCombo.Name = "m_ViewCombo";
		this.m_ViewCombo.Size = new System.Drawing.Size(245, 21);
		this.m_ViewCombo.TabIndex = 3;
		this.m_LevelLbl.AutoSize = true;
		this.m_LevelLbl.Location = new System.Drawing.Point(12, 233);
		this.m_LevelLbl.Name = "m_LevelLbl";
		this.m_LevelLbl.Size = new System.Drawing.Size(33, 13);
		this.m_LevelLbl.TabIndex = 2;
		this.m_LevelLbl.Text = "Level";
		this.m_SelectSurfaceLabel.AutoSize = true;
		this.m_SelectSurfaceLabel.Location = new System.Drawing.Point(12, 9);
		this.m_SelectSurfaceLabel.Name = "m_SelectSurfaceLabel";
		this.m_SelectSurfaceLabel.Size = new System.Drawing.Size(315, 13);
		this.m_SelectSurfaceLabel.TabIndex = 0;
		this.m_SelectSurfaceLabel.Text = "Click on the toposurface(s) that you wish to import into your model";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(358, 289);
		base.Controls.Add(this.m_SelectSurfaceLabel);
		base.Controls.Add(this.m_LevelLbl);
		base.Controls.Add(this.m_ViewCombo);
		base.Controls.Add(this.m_ButtonCancel);
		base.Controls.Add(this.m_ButtonOk);
		base.Controls.Add(this.m_LandXMLTypeListView);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.MinimumSize = new System.Drawing.Size(364, 310);
		base.Name = "LandXMLTypeList";
		this.Text = "LandXMLTypeList";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnLandXMLTypeListClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_ButtonOk;
		base.CancelButton = this.m_ButtonCancel;
	}
}
