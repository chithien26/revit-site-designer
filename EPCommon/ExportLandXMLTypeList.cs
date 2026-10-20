using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SITES.Utilities;

namespace EPCommon;

public class ExportLandXMLTypeList : EPForm
{
	private LandXMLElementType m_CurrentType;

	private List<LandXMLAlignment> m_AlignmentList = new List<LandXMLAlignment>();

	private List<LandXMLAlignment> m_SelectedAlignmentList;

	private List<TopographySurfaceContainer> m_SurfaceList;

	private List<TopographySurfaceContainer> m_SelectedSurfaceList;

	private string m_strCurrentType = string.Empty;

	private Document m_CurrentDocument;

	private int m_nElementCount;

	private List<Autodesk.Revit.DB.View> m_ViewList = new List<Autodesk.Revit.DB.View>();

	private List<FamilyInstanceComponentWrapper> m_SiteComponentList;

	private IContainer components;

	private ListView m_LandXMLTypeListView;

	private Button m_ButtonOk;

	private Button m_ButtonCancel;

	private Label m_SelectedTypeLabel;

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

	public ExportLandXMLTypeList(LandXMLElementType _CurrentType, Document _CurrentDocument, ref List<TopographySurfaceContainer> _SurfaceList)
	{
		m_CurrentType = _CurrentType;
		m_CurrentDocument = _CurrentDocument;
		m_SiteComponentList = new List<FamilyInstanceComponentWrapper>();
		m_SurfaceList = new List<TopographySurfaceContainer>();
		m_SelectedSurfaceList = _SurfaceList;
		InitializeComponent();
		ContextualHelpID = "HID_SDimportLandXMLinfo";
		SetupStrings();
		if (m_CurrentType == LandXMLElementType.Surface)
		{
			try
			{
				LoadSurfaces();
				m_nElementCount = m_SurfaceList.Count;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("ErrorParsingXMLFileString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
		}
		else
		{
			try
			{
				m_nElementCount = m_AlignmentList.Count;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("ErrorParsingXMLFileString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
		}
		GetDialogPosition();
		UpdateListView();
	}

	public ExportLandXMLTypeList(LandXMLElementType _CurrentType, Document _CurrentDocument, ref List<LandXMLAlignment> _AlignmentList)
	{
		m_CurrentType = _CurrentType;
		m_CurrentDocument = _CurrentDocument;
		m_AlignmentList = new List<LandXMLAlignment>();
		m_SelectedAlignmentList = _AlignmentList;
		m_SiteComponentList = new List<FamilyInstanceComponentWrapper>();
		InitializeComponent();
		SetupStrings();
		if (m_CurrentType == LandXMLElementType.Surface)
		{
			try
			{
				m_nElementCount = m_SurfaceList.Count;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("ErrorParsingXMLFileString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
		}
		else
		{
			try
			{
				LoadAlignments();
				m_nElementCount = m_AlignmentList.Count;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("ErrorParsingXMLFileString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
		}
		GetDialogPosition();
		UpdateListView();
	}

	private void LoadSurfaces()
	{
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		IList<Element> list = filteredElementCollector.OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).ToElements();
		foreach (Element item in list)
		{
			TopographySurface topographySurface = TerrainApi.AsSurface(item);
			if (topographySurface == null || topographySurface.IsSiteSubRegion)
			{
				continue;
			}
			m_SurfaceList.Add(new TopographySurfaceContainer(m_CurrentDocument.Application, topographySurface, _bFindMinMaxPoints: true));
		}
	}

	private void LoadAlignments()
	{
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		Parameter LocateTypeParameter = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0 && (LocateTypeParameter = FindFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter)) != null && string.Compare(LocateTypeParameter.AsString(), Convert.ToInt32(LandcaddID.Street).ToString("000"), ignoreCase: true) == 0 && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		filteredElementCollector = new FilteredElementCollector(m_CurrentDocument);
		IList<Element> list2 = filteredElementCollector.OfClass(typeof(CurveElement)).OfCategory(BuiltInCategory.OST_Lines).ToElements();
		FamilyInstanceComponentContainer familyInstanceComponentContainer = null;
		Parameter parameter = null;
		Parameter parameter2 = null;
		bool flag = false;
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
			{
				continue;
			}
			SiteworksIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (SiteworksIDParameter == null)
			{
				continue;
			}
			LocateTypeParameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (LocateTypeParameter == null || string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) != 0)
			{
				continue;
			}
			parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
			if (parameter == null)
			{
				continue;
			}
			parameter2 = familyInstance.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
			if (parameter2 == null || !(parameter2.AsString() != string.Empty))
			{
				continue;
			}
			for (int num = 0; num < list2.Count; num++)
			{
				Element element = list2[num];
				if (element == null || element.Category == null || element.Category.Id.Value64() != BuiltInCategory.OST_Lines.GetHashCode() || string.Compare(parameter.AsString(), element.UniqueId, ignoreCase: true) != 0)
				{
					continue;
				}
				for (int num2 = 0; num2 < m_SiteComponentList.Count; num2++)
				{
					if (string.Compare(parameter2.AsString(), m_SiteComponentList[num2].ChainID, ignoreCase: true) == 0)
					{
						flag = true;
						familyInstanceComponentContainer = new FamilyInstanceComponentContainer(element, familyInstance);
						m_SiteComponentList[num2].FamilyContainerList.Add(familyInstanceComponentContainer);
						break;
					}
				}
				if (!flag)
				{
					List<FamilyInstanceComponentContainer> list3 = new List<FamilyInstanceComponentContainer>();
					familyInstanceComponentContainer = new FamilyInstanceComponentContainer(element, familyInstance);
					list3.Add(familyInstanceComponentContainer);
					m_SiteComponentList.Add(new FamilyInstanceComponentWrapper(parameter2.AsString(), list3));
				}
				else
				{
					flag = false;
				}
				list2.RemoveAt(num);
				break;
			}
			if (list2.Count == 0)
			{
				break;
			}
		}
		FamilyInstance familyInstance2 = null;
		Element element2 = null;
		string text = "Alignment ";
		int num3 = 1;
		for (int num4 = 0; num4 < m_SiteComponentList.Count; num4++)
		{
			LandXMLAlignment landXMLAlignment = new LandXMLAlignment();
			familyInstance2 = m_SiteComponentList[num4].GetFirstExistingFamilyInstance();
			element2 = m_SiteComponentList[num4].GetFirstExistingElement();
			if (familyInstance2 != null)
			{
				parameter2 = familyInstance2.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
				if (parameter2 != null)
				{
					landXMLAlignment.Name = parameter2.AsString();
				}
			}
			if (landXMLAlignment.Name == string.Empty)
			{
				landXMLAlignment.Name = text + num3;
				num3++;
			}
			if (element2 != null)
			{
				Eps_Leg eps_Leg = RevitGeometry.GetLegFromElementList(null, element2);
				if (eps_Leg == null)
				{
					LocationCurve locationCurve = element2.Location as LocationCurve;
					Curve curve = null;
					if (locationCurve != null)
					{
						curve = locationCurve.Curve;
						if (curve != null)
						{
							if (curve is Arc)
							{
								Arc arc = (Arc)curve;
								Eps_Arc eps_Arc = new Eps_Arc(arc);
								landXMLAlignment.StartPoint = eps_Arc.StartPnt;
								landXMLAlignment.Length = ValueConversion.ToString(EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Arc.Radius, eps_Arc.Delta));
								landXMLAlignment.Curves.Add(eps_Arc);
							}
							else if (curve is Line)
							{
								Eps_Line eps_Line = new Eps_Line();
								eps_Line.StartPnt = new Eps_Point(curve.GetEndPoint(0));
								eps_Line.EndPnt = new Eps_Point(curve.GetEndPoint(1));
								landXMLAlignment.StartPoint = eps_Line.StartPnt;
								double _dDistance = 0.0;
								if (EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Line.StartPnt, eps_Line.EndPnt, ref _dDistance) == 0)
								{
									landXMLAlignment.Length = ValueConversion.ToString(_dDistance);
								}
								landXMLAlignment.Lines.Add(eps_Line);
							}
							else if (curve is Ellipse || curve is NurbSpline || curve is HermiteSpline)
							{
								IList<XYZ> list4 = curve.Tessellate();
								int count = list4.Count;
								Eps_Leg eps_Leg2 = null;
								if (count > 1)
								{
									Eps_Point startPnt = new Eps_Point();
									Eps_Point eps_Point = new Eps_Point();
									Eps_Leg eps_Leg3 = null;
									for (int num5 = 0; num5 < count; num5 += 2)
									{
										eps_Point = new Eps_Point(list4[num5]);
										if (num5 > 0)
										{
											eps_Leg3 = new Eps_Leg();
											eps_Leg3.Line = new Eps_Line();
											eps_Leg3.Line.StartPnt = startPnt;
											eps_Leg3.Line.EndPnt = eps_Point;
											if (eps_Leg2 == null)
											{
												eps_Leg2 = eps_Leg3;
												eps_Leg2.Element = element2;
											}
											else
											{
												eps_Leg2.Last.Next = eps_Leg3;
												eps_Leg2.Last.Element = element2;
											}
										}
										startPnt = eps_Point;
									}
									if (num4 == count)
									{
										eps_Leg3 = new Eps_Leg();
										eps_Leg3.Line = new Eps_Line();
										eps_Leg3.Line.StartPnt = startPnt;
										eps_Leg3.Line.EndPnt = new Eps_Point(list4[count - 1]);
										if (eps_Leg2 == null)
										{
											eps_Leg2 = eps_Leg3;
											eps_Leg2.Element = element2;
										}
										else
										{
											eps_Leg2.Last.Next = eps_Leg3;
											eps_Leg2.Last.Element = element2;
										}
									}
								}
								if (eps_Leg2 != null)
								{
									landXMLAlignment.StartPoint = eps_Leg2.LegStartPnt;
									landXMLAlignment.Length = ValueConversion.ToString(RevitGeometry.eplGeoE_CalcLegListLength(eps_Leg));
									while (eps_Leg2.Next != null)
									{
										if (eps_Leg2.Line != null)
										{
											landXMLAlignment.Lines.Add(eps_Leg2.Line);
										}
										else if (eps_Leg2.Arc != null)
										{
											landXMLAlignment.Curves.Add(eps_Leg2.Arc);
										}
										eps_Leg2 = eps_Leg2.Next;
									}
								}
							}
						}
					}
				}
				else
				{
					landXMLAlignment.StartPoint = eps_Leg.LegStartPnt;
					landXMLAlignment.Length = ValueConversion.ToString(RevitGeometry.eplGeoE_CalcLegListLength(eps_Leg));
					while (eps_Leg != null)
					{
						if (eps_Leg.Line != null)
						{
							landXMLAlignment.Lines.Add(eps_Leg.Line);
						}
						else if (eps_Leg.Arc != null)
						{
							landXMLAlignment.Curves.Add(eps_Leg.Arc);
						}
						eps_Leg = eps_Leg.Next;
					}
				}
			}
			m_AlignmentList.Add(landXMLAlignment);
		}
	}

	private void SetupStrings()
	{
		m_ButtonCancel.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		m_ButtonOk.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		switch (m_CurrentType)
		{
		case LandXMLElementType.Surface:
			Text = EPShared.LocalRM.GetString("ToposurfacesToExportDialog");
			m_strCurrentType = EPShared.LocalRM.GetString("ToposurfacesString");
			m_SelectedTypeLabel.Text = EPShared.LocalRM.GetString("SelectExportTopoSurfaceLabelString");
			break;
		case LandXMLElementType.Allignment:
			Text = EPShared.LocalRM.GetString("AlignmentToExportDialog");
			m_strCurrentType = EPShared.LocalRM.GetString("AllignmentString");
			m_SelectedTypeLabel.Text = EPShared.LocalRM.GetString("SelectExportTopoAlignmentLabelString");
			break;
		}
		m_LandXMLTypeListView.View = System.Windows.Forms.View.Details;
		m_LandXMLTypeListView.MultiSelect = true;
		List<string> list = new List<string>();
		m_LandXMLTypeListView.Columns.Add(EPShared.LocalRM.GetString("NameSubRegionParameter"));
		m_LandXMLTypeListView.Columns[0].Width = m_LandXMLTypeListView.Width - 4;
	}

	private void UpdateListView()
	{
		m_LandXMLTypeListView.Items.Clear();
		if (m_CurrentType == LandXMLElementType.Surface)
		{
			foreach (TopographySurfaceContainer surface in m_SurfaceList)
			{
				ListViewItem listViewItem = new ListViewItem();
				string text = string.Empty;
				Parameter parameter = TerrainApi.NameParameter(((Element)surface.TopographySurface));
				if (parameter != null && parameter.AsString() != null && parameter.AsString() != string.Empty)
				{
					text = parameter.AsString();
				}
				if (text != string.Empty)
				{
					listViewItem = m_LandXMLTypeListView.Items.Add(text);
					listViewItem.Tag = text;
				}
				else
				{
					listViewItem = m_LandXMLTypeListView.Items.Add(EPShared.LocalRM.GetString("NotAvailableString"));
					listViewItem.Tag = EPShared.LocalRM.GetString("NotAvailableString");
				}
			}
			return;
		}
		foreach (LandXMLAlignment alignment in m_AlignmentList)
		{
			ListViewItem listViewItem2 = new ListViewItem();
			listViewItem2 = m_LandXMLTypeListView.Items.Add(alignment.Name);
			listViewItem2.Tag = alignment.Name;
		}
	}

	private void OnButtonOkClicked(object sender, EventArgs e)
	{
		DialogResult dialogResult = DialogResult.Cancel;
		if (m_CurrentType == LandXMLElementType.Surface)
		{
			if (m_LandXMLTypeListView.SelectedIndices.Count <= 0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoSurfacesSelectedToExportString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Asterisk);
				return;
			}
			for (int i = 0; i < m_LandXMLTypeListView.SelectedIndices.Count; i++)
			{
				m_SelectedSurfaceList.Add(m_SurfaceList[m_LandXMLTypeListView.SelectedIndices[i]]);
			}
			dialogResult = DialogResult.OK;
		}
		else
		{
			if (m_LandXMLTypeListView.SelectedIndices.Count <= 0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoAlignmentSelectedToExportString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Asterisk);
				return;
			}
			for (int j = 0; j < m_LandXMLTypeListView.SelectedIndices.Count; j++)
			{
				m_SelectedAlignmentList.Add(m_AlignmentList[m_LandXMLTypeListView.SelectedIndices[j]]);
			}
			dialogResult = DialogResult.OK;
		}
		DialogResult = dialogResult;
	}

	private void OnButtonCancelClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
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
		this.m_LandXMLTypeListView = new System.Windows.Forms.ListView();
		this.m_ButtonOk = new System.Windows.Forms.Button();
		this.m_ButtonCancel = new System.Windows.Forms.Button();
		this.m_SelectedTypeLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_LandXMLTypeListView.FullRowSelect = true;
		this.m_LandXMLTypeListView.Location = new System.Drawing.Point(12, 34);
		this.m_LandXMLTypeListView.Name = "m_LandXMLTypeListView";
		this.m_LandXMLTypeListView.Size = new System.Drawing.Size(334, 186);
		this.m_LandXMLTypeListView.TabIndex = 0;
		this.m_LandXMLTypeListView.UseCompatibleStateImageBehavior = false;
		this.m_ButtonOk.Location = new System.Drawing.Point(190, 226);
		this.m_ButtonOk.Name = "m_ButtonOk";
		this.m_ButtonOk.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonOk.TabIndex = 1;
		this.m_ButtonOk.Text = "Ok";
		this.m_ButtonOk.UseVisualStyleBackColor = true;
		this.m_ButtonOk.Click += new System.EventHandler(this.OnButtonOkClicked);
		this.m_ButtonCancel.Location = new System.Drawing.Point(271, 226);
		this.m_ButtonCancel.Name = "m_ButtonCancel";
		this.m_ButtonCancel.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonCancel.TabIndex = 2;
		this.m_ButtonCancel.Text = "Cancel";
		this.m_ButtonCancel.UseVisualStyleBackColor = true;
		this.m_ButtonCancel.Click += new System.EventHandler(this.OnButtonCancelClicked);
		this.m_SelectedTypeLabel.AutoSize = true;
		this.m_SelectedTypeLabel.Location = new System.Drawing.Point(12, 9);
		this.m_SelectedTypeLabel.Name = "m_SelectedTypeLabel";
		this.m_SelectedTypeLabel.Size = new System.Drawing.Size(35, 13);
		this.m_SelectedTypeLabel.TabIndex = 3;
		this.m_SelectedTypeLabel.Text = "label1";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(358, 261);
		base.Controls.Add(this.m_SelectedTypeLabel);
		base.Controls.Add(this.m_ButtonCancel);
		base.Controls.Add(this.m_ButtonOk);
		base.Controls.Add(this.m_LandXMLTypeListView);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.MinimumSize = new System.Drawing.Size(364, 287);
		base.Name = "ExportLandXMLTypeList";
		this.Text = "LandXMLTypeList";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_ButtonOk;
		base.CancelButton = this.m_ButtonCancel;
	}
}
