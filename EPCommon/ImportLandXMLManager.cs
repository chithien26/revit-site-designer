using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using SITES.Utilities;

namespace EPCommon;

public class ImportLandXMLManager : EPForm
{
	private Document m_CurrentDocument;

	private List<TopographySurface> m_ListOfTopoSurfaces = new List<TopographySurface>();

	private double m_dDistance;

	private double m_dHzDirection;

	private double m_dVzDirection;

	private double m_dSharedCoordAngle;

	private Eps_Point m_pBasePoint = new Eps_Point();

	private IContainer components;

	private GroupBox m_GroupBox;

	private Button m_CancelButton;

	private Button m_OKButton;

	private Button m_FilePickerButton;

	private TextBox m_FileNameTxtBox;

	private Label m_FileNameLbl;

	private CheckBox m_ImportSurfaceCheckbox;

	private CheckBox m_ImportAlignmentCheckBox;

	private Label m_ImportOptionsLbl;

	private Label m_ImportOptionLbl;

	private ComboBox m_ImportOptionCmbBox;

	public Eps_Point ManualCenterBasePoint
	{
		get
		{
			return m_pBasePoint;
		}
		set
		{
			m_pBasePoint = value;
		}
	}

	public ImportLandXMLManager(Document _CurrentDocument)
	{
		InitializeComponent();
		ContextualHelpID = "HID_SDimportLandXML";
		m_CancelButton.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		m_OKButton.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		m_FileNameLbl.Text = EPShared.LocalRM.GetString("AMP_FileNameImportXMLDialog");
		Text = EPShared.LocalRM.GetString("ImportLandTitleString");
		m_GroupBox.Text = EPShared.LocalRM.GetString("ImportLandXMLString");
		m_ImportSurfaceCheckbox.Text = EPShared.LocalRM.GetString("Import_AMP_ToposurfacesInXMLImportXMLDialog");
		m_ImportAlignmentCheckBox.Text = EPShared.LocalRM.GetString("Import_AMP_AligmentsInXMLImportXMLDialog");
		m_ImportOptionLbl.Text = EPShared.LocalRM.GetString("AMP_PositioningDialog");
		m_ImportOptionCmbBox.Items.Add(EPShared.LocalRM.GetString("PositioningAutoCenterToCenter"));
		m_ImportOptionCmbBox.Items.Add(EPShared.LocalRM.GetString("PositioningAutoOriginToOrigin"));
		m_ImportOptionCmbBox.Items.Add(EPShared.LocalRM.GetString("PositioningManualCenter"));
		m_ImportOptionCmbBox.Items.Add(EPShared.LocalRM.GetString("PositioningUsePreviousValues"));
		m_ImportOptionCmbBox.Items.Add(EPShared.LocalRM.GetString("PositioningBySurveyPoints"));
		m_ImportOptionCmbBox.SelectedIndex = 0;
		m_CurrentDocument = _CurrentDocument;
		InitializeControls();
	}

	private void InitializeControls()
	{
		int _nReturnValue = 0;
		string _strReturnValue = string.Empty;
		GetDialogPosition();
		if (ApplicationUtility.Settings.eplSetE_GetSetting("ImportLandXMLPositioning", ref _strReturnValue))
		{
			string empty = string.Empty;
			for (int i = 0; i < m_ImportOptionCmbBox.Items.Count; i++)
			{
				if (m_ImportOptionCmbBox.Items[i] is string strA && string.Compare(strA, _strReturnValue, ignoreCase: true) == 0)
				{
					m_ImportOptionCmbBox.SelectedIndex = i;
					break;
				}
			}
		}
		m_ImportSurfaceCheckbox.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("ImportLandXMLSurface", ref _nReturnValue))
		{
			m_ImportSurfaceCheckbox.Checked = Convert.ToBoolean(_nReturnValue);
		}
		m_ImportAlignmentCheckBox.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("ImportLandXMLAlignment", ref _nReturnValue))
		{
			m_ImportAlignmentCheckBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
	}

	private void SaveSettings()
	{
		ApplicationUtility.Settings.eplSetE_SetSetting("ImportLandXMLPositioning", Convert.ToString(m_ImportOptionCmbBox.SelectedItem));
		ApplicationUtility.Settings.eplSetE_SetSetting("ImportLandXMLSurface", Convert.ToInt32(m_ImportSurfaceCheckbox.Checked));
		ApplicationUtility.Settings.eplSetE_SetSetting("ImportLandXMLAlignment", Convert.ToInt32(m_ImportAlignmentCheckBox.Checked));
	}

	private void m_FilePickerButton_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = ApplicationUtility.GetString("XmlFilePickerFilter");
		openFileDialog.FilterIndex = 1;
		openFileDialog.RestoreDirectory = true;
		openFileDialog.Multiselect = false;
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			m_FileNameTxtBox.Text = openFileDialog.FileName;
		}
	}

	private void OKButtonClicked(object sender, EventArgs e)
	{
		if (!VerifyFileNameAndPath())
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidFileNameLocationMessageString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return;
		}
		SaveSettings();
		switch (GetCurrentLandXMLImportOptions())
		{
		case LandXMLImportOptions.ManualCenter:
			DialogResult = DialogResult.Retry;
			return;
		case LandXMLImportOptions.UsePreviousTranslation:
		{
			FamilyInstance siteComponentPlacementFamilyInstance = ImportLandXMLClass.GetSiteComponentPlacementFamilyInstance(m_CurrentDocument, "", _bCheckForSurface: false);
			if (siteComponentPlacementFamilyInstance == null)
			{
				string strText = EPShared.LocalRM.GetString("NoPreviousPositionAvailableMessage");
				RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				DialogResult = DialogResult.Cancel;
				return;
			}
			break;
		}
		}
		ImportSurfaceAlignments();
	}

	public DialogResult ImportSurfaceAlignments()
	{
		if (!m_ImportAlignmentCheckBox.Checked && !m_ImportSurfaceCheckbox.Checked)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NoImportOptionsSelectedString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return DialogResult.Retry;
		}
		LandXMLParser landXMLParser = new LandXMLParser(m_FileNameTxtBox.Text);
		DialogResult dialogResult = DialogResult.Cancel;
		DialogResult dialogResult2 = DialogResult.Cancel;
		List<LandXMLSurface> list = new List<LandXMLSurface>();
		List<LandXMLAlignment> list2 = new List<LandXMLAlignment>();
		Autodesk.Revit.DB.View currentView = null;
		EPTransactionContainer ePTransactionContainer = new EPTransactionContainer(m_CurrentDocument);
		ePTransactionContainer.Start(EPShared.LocalRM.GetString("ImportingToposurfaceString"));
		if (m_ImportSurfaceCheckbox.Checked)
		{
			using LandXMLTypeList landXMLTypeList = new LandXMLTypeList(LandXMLElementType.Surface, m_CurrentDocument, ePTransactionContainer, GetCurrentLandXMLImportOptions(), m_pBasePoint, landXMLParser);
			if (!landXMLTypeList.InvalidXMLFile)
			{
				if (landXMLTypeList.ElementCount <= 0)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NoSurfacesInLandXMLString"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
				}
				else
				{
					landXMLTypeList.ShowDialog();
					dialogResult = landXMLTypeList.DialogResult;
					if (dialogResult == DialogResult.OK)
					{
						list = landXMLTypeList.SelectedSurfaceList;
					}
				}
			}
			else
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidXMLFileName"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
		}
		if (m_ImportAlignmentCheckBox.Checked)
		{
			using LandXMLTypeList landXMLTypeList2 = new LandXMLTypeList(LandXMLElementType.Allignment, m_CurrentDocument, ePTransactionContainer, GetCurrentLandXMLImportOptions(), m_pBasePoint, landXMLParser);
			if (!landXMLTypeList2.InvalidXMLFile)
			{
				if (landXMLTypeList2.ElementCount <= 0 && !landXMLTypeList2.HasSpiral)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NoAlignmentInLandXMLString"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
				}
				else if (landXMLTypeList2.ElementCount > 0)
				{
					landXMLTypeList2.ShowDialog();
					dialogResult2 = landXMLTypeList2.DialogResult;
					if (dialogResult2 == DialogResult.OK)
					{
						list2 = landXMLTypeList2.SelectedAlignmentList;
						currentView = landXMLTypeList2.CurrentView;
					}
				}
			}
			else
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidXMLFileName"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
		}
		if (dialogResult == DialogResult.OK || dialogResult2 == DialogResult.OK)
		{
			if (dialogResult == DialogResult.OK && list.Count > 0)
			{
				FindMinMaxForSelectedSurfaces(landXMLParser, list);
				for (int i = 0; i < list.Count; i++)
				{
					CreateAndReassignProposedOrBaseSurface(ePTransactionContainer, list[i]);
				}
			}
			if (dialogResult2 == DialogResult.OK)
			{
				if (landXMLParser != null)
				{
					ProgressBarForm ProgressFrm = null;
					for (int j = 0; j < list2.Count; j++)
					{
						LandXMLAlignment _CurrentAlignment = list2[j];
						landXMLParser.ParseLandXMLAlignmentSegments(ref _CurrentAlignment, ref ProgressFrm, list2.Count);
						list2[j] = _CurrentAlignment;
					}
					if (ProgressFrm != null)
					{
						ProgressFrm.SetMaxValue();
						Thread.Sleep(300);
						ProgressFrm.Close();
					}
				}
				if (list.Count <= 0)
				{
					FindMinMaxForSelectedAlignments(list2);
				}
				foreach (LandXMLAlignment item in list2)
				{
					CreateAlignment(ePTransactionContainer, item, currentView);
				}
			}
			RevitMessageBox.Show(EPShared.LocalRM.GetString("ImportLandXMLComponentsCompleteString"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
			ePTransactionContainer.Commit();
			DialogResult = DialogResult.OK;
			LandXMLImportOptions currentLandXMLImportOptions = GetCurrentLandXMLImportOptions();
			if (currentLandXMLImportOptions == LandXMLImportOptions.UseSharedCoordinates)
			{
				ApplicationUtility.Settings.eplSetE_SetSetting(m_CurrentDocument.PathName, "UseSharedElevation", 1);
			}
		}
		if (ePTransactionContainer.HasStarted())
		{
			ePTransactionContainer.Rollback();
		}
		if (dialogResult == DialogResult.OK || dialogResult2 == DialogResult.OK)
		{
			return DialogResult.OK;
		}
		return DialogResult.Cancel;
	}

	private bool CheckSurfaceExists(string _strSurfaceName)
	{
		return false;
	}

	private void OnFocusLost(object sender, EventArgs e)
	{
		if (!VerifyFileNameAndPath())
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidXMLFileName"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
	}

	private bool VerifyFileNameAndPath()
	{
		if (string.IsNullOrEmpty(m_FileNameTxtBox.Text))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(m_FileNameTxtBox.Text))
		{
			FileInfo fileInfo = new FileInfo(m_FileNameTxtBox.Text);
			if (File.Exists(m_FileNameTxtBox.Text) && string.Compare(fileInfo.Extension, ".xml", ignoreCase: true) != 0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("Source") + " " + EPShared.LocalRM.GetString("NoXMLExtension"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return false;
			}
		}
		return true;
	}

	private void OnCancelButtonClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
	}

	private void OnImportLandXMLClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private LandXMLImportOptions GetCurrentLandXMLImportOptions()
	{
		LandXMLImportOptions result = LandXMLImportOptions.AutoCenterToCenter;
		if (string.Compare(m_ImportOptionCmbBox.Items[m_ImportOptionCmbBox.SelectedIndex].ToString(), EPShared.LocalRM.GetString("PositioningAutoCenterToCenter"), ignoreCase: true) == 0)
		{
			result = LandXMLImportOptions.AutoCenterToCenter;
		}
		else if (string.Compare(m_ImportOptionCmbBox.Items[m_ImportOptionCmbBox.SelectedIndex].ToString(), EPShared.LocalRM.GetString("PositioningAutoOriginToOrigin"), ignoreCase: true) == 0)
		{
			result = LandXMLImportOptions.AutoOriginToOrigin;
		}
		else if (string.Compare(m_ImportOptionCmbBox.Items[m_ImportOptionCmbBox.SelectedIndex].ToString(), EPShared.LocalRM.GetString("PositioningManualCenter"), ignoreCase: true) == 0)
		{
			result = LandXMLImportOptions.ManualCenter;
		}
		else if (string.Compare(m_ImportOptionCmbBox.Items[m_ImportOptionCmbBox.SelectedIndex].ToString(), EPShared.LocalRM.GetString("PositioningUsePreviousValues"), ignoreCase: true) == 0)
		{
			result = LandXMLImportOptions.UsePreviousTranslation;
		}
		else if (string.Compare(m_ImportOptionCmbBox.Items[m_ImportOptionCmbBox.SelectedIndex].ToString(), EPShared.LocalRM.GetString("PositioningBySurveyPoints"), ignoreCase: true) == 0)
		{
			result = LandXMLImportOptions.UseSharedCoordinates;
		}
		return result;
	}

	private void FindMinMaxForSelectedSurfaces(LandXMLParser _XMLParser, List<LandXMLSurface> _SelectedSurfacesList)
	{
		LandXMLImportOptions currentLandXMLImportOptions = GetCurrentLandXMLImportOptions();
		if (_XMLParser != null)
		{
			int num = 0;
			for (int i = 0; i < _SelectedSurfacesList.Count; i++)
			{
				num += _SelectedSurfacesList[i].SurfacePointCount;
			}
			ProgressBarForm ProgressFrm = null;
			for (int j = 0; j < _SelectedSurfacesList.Count; j++)
			{
				LandXMLSurface _CurrentSurface = _SelectedSurfacesList[j];
				_XMLParser.ParseLandXMLSurfacePoints(ref _CurrentSurface, ref ProgressFrm, num);
				_SelectedSurfacesList[j] = _CurrentSurface;
			}
			if (ProgressFrm != null)
			{
				ProgressFrm.SetMaxValue();
				Thread.Sleep(300);
				ProgressFrm.Close();
			}
		}
		if (currentLandXMLImportOptions == LandXMLImportOptions.UsePreviousTranslation)
		{
			double _dDistance = 0.0;
			double _dHzDirection = 0.0;
			double _dVzDirection = 0.0;
			FamilyInstance siteComponentPlacementFamilyInstance = ImportLandXMLClass.GetSiteComponentPlacementFamilyInstance(m_CurrentDocument, "", _bCheckForSurface: false);
			if (siteComponentPlacementFamilyInstance != null)
			{
				ImportLandXMLClass.FindPositioningParameters(siteComponentPlacementFamilyInstance, ref _dDistance, ref _dHzDirection, ref _dVzDirection);
				m_dHzDirection = _dHzDirection;
				m_dVzDirection = _dVzDirection;
				m_dDistance = _dDistance;
			}
			return;
		}
		Eps_Point eps_Point = new Eps_Point();
		if (currentLandXMLImportOptions == LandXMLImportOptions.ManualCenter)
		{
			eps_Point = m_pBasePoint;
		}
		else
		{
			ProjectLocation activeProjectLocation = m_CurrentDocument.ActiveProjectLocation;
			XYZ point = new XYZ();
			ProjectPosition projectPosition = activeProjectLocation.GetProjectPosition(point);
			eps_Point.x = projectPosition.EastWest;
			eps_Point.y = projectPosition.NorthSouth;
			eps_Point.z = projectPosition.Elevation;
			m_dSharedCoordAngle = projectPosition.Angle;
		}
		if (currentLandXMLImportOptions == LandXMLImportOptions.UseSharedCoordinates)
		{
			EPShared.eplGeoE_CalcXYZToPolar(eps_Point, new Eps_Point(), ref m_dHzDirection, ref m_dVzDirection, ref m_dDistance);
			return;
		}
		Eps_Point pMinPnt = new Eps_Point();
		Eps_Point pMaxPnt = new Eps_Point();
		pMinPnt.x = double.MaxValue;
		pMaxPnt.x = double.MinValue;
		pMinPnt.y = double.MaxValue;
		pMaxPnt.y = double.MinValue;
		pMinPnt.z = double.MaxValue;
		pMaxPnt.z = double.MinValue;
		foreach (LandXMLSurface _SelectedSurfaces in _SelectedSurfacesList)
		{
			foreach (Eps_Point surfacePoint in _SelectedSurfaces.SurfacePoints)
			{
				RevitGeometry.CalcMinMax(surfacePoint, ref pMinPnt, ref pMaxPnt);
			}
		}
		Eps_Point eps_Point2 = new Eps_Point();
		eps_Point2.x = (pMaxPnt.x + pMinPnt.x) / 2.0;
		eps_Point2.y = (pMaxPnt.y + pMinPnt.y) / 2.0;
		eps_Point2.z = (pMaxPnt.z + pMinPnt.z) / 2.0;
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point2, eps_Point, ref m_dHzDirection);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Point2, eps_Point, ref m_dDistance);
	}

	private void FindMinMaxForSelectedAlignments(List<LandXMLAlignment> _SelectedAlignmentList)
	{
		LandXMLImportOptions currentLandXMLImportOptions = GetCurrentLandXMLImportOptions();
		if (currentLandXMLImportOptions == LandXMLImportOptions.UsePreviousTranslation)
		{
			double _dDistance = 0.0;
			double _dHzDirection = 0.0;
			double _dVzDirection = 0.0;
			FamilyInstance siteComponentPlacementFamilyInstance = ImportLandXMLClass.GetSiteComponentPlacementFamilyInstance(m_CurrentDocument, "", _bCheckForSurface: false);
			if (siteComponentPlacementFamilyInstance != null)
			{
				ImportLandXMLClass.FindPositioningParameters(siteComponentPlacementFamilyInstance, ref _dDistance, ref _dHzDirection, ref _dVzDirection);
				m_dHzDirection = _dHzDirection;
				m_dVzDirection = _dVzDirection;
				m_dDistance = _dDistance;
			}
			return;
		}
		Eps_Point eps_Point = new Eps_Point();
		if (currentLandXMLImportOptions == LandXMLImportOptions.ManualCenter)
		{
			eps_Point = m_pBasePoint;
		}
		else
		{
			ProjectLocation activeProjectLocation = m_CurrentDocument.ActiveProjectLocation;
			XYZ point = new XYZ();
			ProjectPosition projectPosition = activeProjectLocation.GetProjectPosition(point);
			eps_Point.x = projectPosition.EastWest;
			eps_Point.y = projectPosition.NorthSouth;
			eps_Point.z = projectPosition.Elevation;
			m_dSharedCoordAngle = projectPosition.Angle;
		}
		if (currentLandXMLImportOptions == LandXMLImportOptions.UseSharedCoordinates)
		{
			EPShared.eplGeoE_CalcXYZToPolar(eps_Point, new Eps_Point(), ref m_dHzDirection, ref m_dVzDirection, ref m_dDistance);
			return;
		}
		Eps_Point pMinPnt = new Eps_Point();
		Eps_Point pMaxPnt = new Eps_Point();
		pMinPnt.x = double.MaxValue;
		pMaxPnt.x = double.MinValue;
		pMinPnt.y = double.MaxValue;
		pMaxPnt.y = double.MinValue;
		pMinPnt.z = double.MaxValue;
		pMaxPnt.z = double.MinValue;
		foreach (LandXMLAlignment _SelectedAlignment in _SelectedAlignmentList)
		{
			Eps_Leg eps_Leg = Eps_Leg.CopyLeg(CreateLegFromAlignment(LandXMLAlignment.CopyAlignment(_SelectedAlignment)));
			Eps_Leg eps_Leg2 = null;
			for (eps_Leg2 = eps_Leg; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
			{
				if (eps_Leg2.Line != null)
				{
					RevitGeometry.CalcMinMax(eps_Leg2.Line.StartPnt, ref pMinPnt, ref pMaxPnt);
				}
				else if (eps_Leg2.Arc != null)
				{
					RevitGeometry.CalcMinMax(eps_Leg2.Arc.StartPnt, ref pMinPnt, ref pMaxPnt);
				}
				if (eps_Leg2.Next == null)
				{
					if (eps_Leg2.Line != null)
					{
						RevitGeometry.CalcMinMax(eps_Leg2.Line.EndPnt, ref pMinPnt, ref pMaxPnt);
					}
					else if (eps_Leg2.Arc != null)
					{
						RevitGeometry.CalcMinMax(eps_Leg2.Arc.EndPnt, ref pMinPnt, ref pMaxPnt);
					}
				}
			}
		}
		Eps_Point eps_Point2 = new Eps_Point();
		eps_Point2.x = (pMaxPnt.x + pMinPnt.x) / 2.0;
		eps_Point2.y = (pMaxPnt.y + pMinPnt.y) / 2.0;
		eps_Point2.z = (pMaxPnt.z + pMinPnt.z) / 2.0;
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point2, eps_Point, ref m_dHzDirection);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Point2, eps_Point, ref m_dDistance);
	}

	private TopographySurface AddSurfaceToDocument(EPTransactionContainer _TransactionContainer, LandXMLSurface _CurSurface)
	{
		TopographySurface topographySurface = null;
		List<XYZ> _RevitPntList = new List<XYZ>();
		ConvertLandXMLPntsToRevit(_CurSurface.SurfacePoints, ref _RevitPntList);
		if (_RevitPntList.Count > 0)
		{
			TopographySurfaceManagement.RemoveDuplicateXY(ref _RevitPntList);
			topographySurface = TopographySurface.Create(m_CurrentDocument, _RevitPntList);
			m_CurrentDocument.Regenerate();
		}
		if (topographySurface != null)
		{
			((Element)topographySurface).get_Parameter(BuiltInParameter.ROOM_NAME)?.Set(_CurSurface.SurfaceName);
			Parameter parameter = ((Element)topographySurface).get_Parameter(BuiltInParameter.PHASE_CREATED);
			if (parameter != null && _CurSurface.Phase != null)
			{
				ElementId id = _CurSurface.Phase.Id;
				parameter.Set(id);
			}
			LoadLandXMLSurfaceFamily(_TransactionContainer, topographySurface);
		}
		return topographySurface;
	}

	private void LoadLandXMLSurfaceFamily(EPTransactionContainer _TransactionContainer, TopographySurface _Surface)
	{
		LandXMLImportOptions currentLandXMLImportOptions = GetCurrentLandXMLImportOptions();
		if (currentLandXMLImportOptions == LandXMLImportOptions.UseSharedCoordinates)
		{
			return;
		}
		string _strFullFamilyFilename = string.Empty;
		FamilySymbol familySymbol = null;
		if (!RevitUtility.GetFullFamilyFilenameIfExist("LandXMLSurface Placement.rfa", ref _strFullFamilyFilename))
		{
			return;
		}
		Family _Family = null;
		_TransactionContainer.BeginTransaction();
		RevitUtility.LoadFamily(m_CurrentDocument, _strFullFamilyFilename, out _Family, _bUseFailureHandling: true, ref _TransactionContainer);
		_TransactionContainer.EndTransaction();
		if (_Family != null && _Family.GetFamilySymbolIds() != null)
		{
			using IEnumerator<ElementId> enumerator = _Family.GetFamilySymbolIds().GetEnumerator();
			if (enumerator.MoveNext())
			{
				ElementId current = enumerator.Current;
				FamilySymbol familySymbol2 = _Family.Document.GetElement(current) as FamilySymbol;
				familySymbol = familySymbol2;
			}
		}
		if (familySymbol != null)
		{
			if (!familySymbol.IsActive)
			{
				familySymbol.Activate();
			}
			FamilyInstance familyInstance = RevitUtility.NewFamilyInstance(m_CurrentDocument, new XYZ(0.0, 0.0, 0.0), familySymbol, _Surface, m_CurrentDocument.ActiveView.GenLevel, StructuralType.NonStructural, ref _TransactionContainer);
			Parameter parameter = null;
			parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
			if (parameter != null && !parameter.IsReadOnly)
			{
				parameter.Set(_Surface.UniqueId);
			}
			parameter = familyInstance.LookupParameter(RevitParameters.PositioningMethodLandXMLSurfacePlacementFamilyInstanceParameter);
			if (parameter != null && !parameter.IsReadOnly)
			{
				parameter.Set(Convert.ToInt32(currentLandXMLImportOptions));
			}
			parameter = familyInstance.LookupParameter(RevitParameters.PositionDistanceLandXMLSurfacePlacementFamilyInstanceParameter);
			if (parameter != null && !parameter.IsReadOnly)
			{
				parameter.Set(Convert.ToDouble(m_dDistance));
			}
			parameter = familyInstance.LookupParameter(RevitParameters.PositionHzDirectionLandXMLSurfacePlacementFamilyInstanceParameter);
			if (parameter != null && !parameter.IsReadOnly)
			{
				parameter.Set(Convert.ToDouble(m_dHzDirection));
			}
			parameter = familyInstance.LookupParameter(RevitParameters.PositionVzDirectionLandXMLSurfacePlacementFamilyInstanceParameter);
			if (parameter != null && !parameter.IsReadOnly)
			{
				parameter.Set(Convert.ToDouble(m_dVzDirection));
			}
		}
	}

	private void ConvertLandXMLPntsToRevit(List<Eps_Point> _AllPnts, ref List<XYZ> _RevitPntList)
	{
		_RevitPntList.Clear();
		foreach (Eps_Point _AllPnt in _AllPnts)
		{
			XYZ curRevitPnt = new XYZ(_AllPnt.x, _AllPnt.y, _AllPnt.z);
			_RevitPntList.Add(ApplyTranslation(curRevitPnt));
		}
	}

	private XYZ ApplyTranslation(XYZ CurRevitPnt)
	{
		Eps_Point eps_Point = ApplyTranslation(new Eps_Point(CurRevitPnt));
		return eps_Point.XYZ;
	}

	private Eps_Point ApplyTranslation(Eps_Point CurRevitPnt)
	{
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		switch (GetCurrentLandXMLImportOptions())
		{
		case LandXMLImportOptions.AutoCenterToCenter:
		case LandXMLImportOptions.ManualCenter:
		case LandXMLImportOptions.UsePreviousTranslation:
		case LandXMLImportOptions.UseSharedCoordinates:
		{
			Eps_Point _NewPnt = new Eps_Point();
			EPShared.eplGeoE_CalcPolarToXYZ(CurRevitPnt, m_dHzDirection, m_dVzDirection, m_dDistance, ref _NewPnt);
			EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(new Eps_Point(), _NewPnt, ref _dDirection);
			EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(new Eps_Point(), _NewPnt, ref _dDistance);
			EPShared.eplGeoE_CalcPolarToXY(new Eps_Point(0.0, 0.0, _NewPnt.z), EPShared.eplGeoE_NormalizeRadians(_dDirection - m_dSharedCoordAngle), _dDistance, ref _NewPnt);
			return _NewPnt;
		}
		default:
			return CurRevitPnt;
		}
	}

	private void CreateAndReassignProposedOrBaseSurface(EPTransactionContainer _TransactionContainer, LandXMLSurface _CurSurface)
	{
		Parameter parameter = null;
		_TransactionContainer.BeginTransaction();
		TopographySurface topographySurface = AddSurfaceToDocument(_TransactionContainer, _CurSurface);
		if (topographySurface != null && _CurSurface.OverwriteSurfaceID != string.Empty)
		{
			TopographySurfaceContainer _ProposedTopographySurfaceContainer = null;
			TopographySurfaceContainer _BaseTopographySurfaceContainer = null;
			FamilyInstance _ProposedPlacementFamilyInstance = null;
			FamilyInstance _BasePlacementFamilyInstance = null;
			TopographySurfaceManagement.FindBaseLandcaddTopographySurface(m_CurrentDocument, ref _BaseTopographySurfaceContainer, ref _BasePlacementFamilyInstance, ref _ProposedTopographySurfaceContainer, ref _ProposedPlacementFamilyInstance, _TransactionContainer);
			if (_ProposedTopographySurfaceContainer != null && _ProposedTopographySurfaceContainer.TopographySurface != null && _ProposedPlacementFamilyInstance != null && string.Compare(_ProposedTopographySurfaceContainer.TopographySurface.UniqueId, _CurSurface.OverwriteSurfaceID, ignoreCase: true) == 0)
			{
				_ProposedPlacementFamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter)?.Set(topographySurface.UniqueId);
			}
			if (_BaseTopographySurfaceContainer != null && _BaseTopographySurfaceContainer.TopographySurface != null && _BasePlacementFamilyInstance != null && string.Compare(_BaseTopographySurfaceContainer.TopographySurface.UniqueId, _CurSurface.OverwriteSurfaceID, ignoreCase: true) == 0)
			{
				_BasePlacementFamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter)?.Set(topographySurface.UniqueId);
			}
		}
		_TransactionContainer.EndTransaction();
	}

	private void MoveAlignment(Eps_Leg _CurrentLeg)
	{
		Eps_Leg eps_Leg = null;
		for (eps_Leg = _CurrentLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				eps_Leg.Line.StartPnt = ApplyTranslation(eps_Leg.Line.StartPnt);
				eps_Leg.Line.EndPnt = ApplyTranslation(eps_Leg.Line.EndPnt);
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg.Arc.StartPnt = ApplyTranslation(eps_Leg.Arc.StartPnt);
				eps_Leg.Arc.EndPnt = ApplyTranslation(eps_Leg.Arc.EndPnt);
				eps_Leg.Arc.RadiusPnt = ApplyTranslation(eps_Leg.Arc.RadiusPnt);
			}
		}
	}

	private void CreateAlignment(EPTransactionContainer _TransactionContainer, LandXMLAlignment _CurrentAlignment, Autodesk.Revit.DB.View _CurrentView)
	{
		Eps_Leg eps_Leg = Eps_Leg.CopyLeg(CreateLegFromAlignment(LandXMLAlignment.CopyAlignment(_CurrentAlignment)));
		if (eps_Leg != null)
		{
			LandXMLImportOptions currentLandXMLImportOptions = GetCurrentLandXMLImportOptions();
			if (currentLandXMLImportOptions == LandXMLImportOptions.AutoCenterToCenter || (uint)(currentLandXMLImportOptions - 2) <= 2u)
			{
				MoveAlignment(eps_Leg);
			}
			_TransactionContainer.FailureHandlingOptions(BuiltInFailures.InaccurateFailures.InaccurateLine);
			if (_CurrentView != null)
			{
				_TransactionContainer.BeginTransaction();
				RevitUtility.NewModelCurveArray(m_CurrentDocument, eps_Leg, _CurrentView);
				_TransactionContainer.EndTransaction();
			}
		}
		DialogResult = DialogResult.OK;
	}

	private Eps_Leg CreateLegFromAlignment(LandXMLAlignment _CurrentAlignment)
	{
		Eps_Leg eps_Leg = null;
		List<Eps_Line> list = new List<Eps_Line>(_CurrentAlignment.Lines);
		List<Eps_Arc> list2 = new List<Eps_Arc>(_CurrentAlignment.Curves);
		eps_Leg = FindStartingSegment(_CurrentAlignment);
		if (eps_Leg == null)
		{
			eps_Leg = new Eps_Leg();
			if (_CurrentAlignment.Lines.Count > 0)
			{
				eps_Leg.Line = new Eps_Line(_CurrentAlignment.Lines[0]);
				list.RemoveAt(0);
			}
			else if (_CurrentAlignment.Curves.Count > 0)
			{
				eps_Leg.Arc = new Eps_Arc(_CurrentAlignment.Curves[0]);
				list2.RemoveAt(0);
			}
		}
		Eps_Leg eps_Leg2 = null;
		int num = -1;
		int num2 = -1;
		double num3 = double.MaxValue;
		double num4 = double.MaxValue;
		bool flag = false;
		while (list.Count > 0 || list2.Count > 0)
		{
			for (int num5 = list.Count - 1; num5 >= 0; num5--)
			{
				Eps_Line currentLine = list[num5];
				if (Eps_Leg.InsertLineLegSegment(ref eps_Leg, currentLine))
				{
					list.RemoveAt(num5);
					flag = true;
				}
			}
			for (int num6 = list2.Count - 1; num6 >= 0; num6--)
			{
				Eps_Arc currentArc = list2[num6];
				if (Eps_Leg.InsertArcLegSegment(ref eps_Leg, currentArc))
				{
					list2.RemoveAt(num6);
					flag = true;
				}
			}
			if (!flag)
			{
				for (int num7 = list.Count - 1; num7 >= 0; num7--)
				{
					eps_Leg2 = new Eps_Leg();
					eps_Leg2.Line = list[num7];
					num3 = Eps_Leg.FindMinimumDistanceBetweenLegs(eps_Leg, eps_Leg2);
					if (num3 < num4)
					{
						num4 = num3;
						num = num7;
					}
				}
				for (int num8 = list2.Count - 1; num8 >= 0; num8--)
				{
					eps_Leg2 = new Eps_Leg();
					eps_Leg2.Arc = list2[num8];
					num3 = Eps_Leg.FindMinimumDistanceBetweenLegs(eps_Leg, eps_Leg2);
					if (num3 < num4)
					{
						num4 = num3;
						num2 = num8;
						num = -1;
					}
				}
				if (num >= 0)
				{
					eps_Leg2 = new Eps_Leg();
					eps_Leg2.Line = list[num];
					Eps_Leg.CombineLegListsAtShortestGaps(ref eps_Leg, eps_Leg2);
					list.RemoveAt(num);
					num = -1;
				}
				else if (num2 >= 0)
				{
					eps_Leg2 = new Eps_Leg();
					eps_Leg2.Arc = list2[num2];
					Eps_Leg.CombineLegListsAtShortestGaps(ref eps_Leg, eps_Leg2);
					list2.RemoveAt(num2);
					num2 = -1;
				}
				num3 = double.MaxValue;
				num4 = double.MaxValue;
			}
			else
			{
				flag = false;
			}
		}
		return eps_Leg;
	}

	private Eps_Leg FindStartingSegment(LandXMLAlignment _CurrentAlignment)
	{
		Eps_Line eps_Line = null;
		Eps_Leg eps_Leg = null;
		int num = -1;
		foreach (Eps_Line line in _CurrentAlignment.Lines)
		{
			num++;
			if (line.StartPnt.IsEqual(_CurrentAlignment.StartPoint))
			{
				eps_Line = line;
				break;
			}
			if (line.EndPnt.IsEqual(_CurrentAlignment.StartPoint))
			{
				eps_Line = line.Reverse();
				break;
			}
		}
		if (eps_Line != null)
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = new Eps_Line(eps_Line);
			_CurrentAlignment.Lines.RemoveAt(num);
		}
		else
		{
			num = -1;
			Eps_Arc eps_Arc = null;
			foreach (Eps_Arc curf in _CurrentAlignment.Curves)
			{
				num++;
				if (curf.StartPnt.IsEqual(_CurrentAlignment.StartPoint))
				{
					eps_Arc = curf;
					break;
				}
				if (curf.EndPnt.IsEqual(_CurrentAlignment.StartPoint))
				{
					eps_Arc = curf.Reverse();
					break;
				}
			}
			if (eps_Arc != null)
			{
				eps_Leg = new Eps_Leg();
				eps_Leg.Arc = new Eps_Arc(eps_Arc);
				_CurrentAlignment.Curves.RemoveAt(num);
			}
		}
		return eps_Leg;
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
		this.m_GroupBox = new System.Windows.Forms.GroupBox();
		this.m_FilePickerButton = new System.Windows.Forms.Button();
		this.m_FileNameTxtBox = new System.Windows.Forms.TextBox();
		this.m_FileNameLbl = new System.Windows.Forms.Label();
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_OKButton = new System.Windows.Forms.Button();
		this.m_ImportSurfaceCheckbox = new System.Windows.Forms.CheckBox();
		this.m_ImportAlignmentCheckBox = new System.Windows.Forms.CheckBox();
		this.m_ImportOptionsLbl = new System.Windows.Forms.Label();
		this.m_ImportOptionLbl = new System.Windows.Forms.Label();
		this.m_ImportOptionCmbBox = new System.Windows.Forms.ComboBox();
		this.m_GroupBox.SuspendLayout();
		base.SuspendLayout();
		this.m_GroupBox.Controls.Add(this.m_FilePickerButton);
		this.m_GroupBox.Controls.Add(this.m_FileNameTxtBox);
		this.m_GroupBox.Controls.Add(this.m_FileNameLbl);
		this.m_GroupBox.Location = new System.Drawing.Point(12, 12);
		this.m_GroupBox.Name = "m_GroupBox";
		this.m_GroupBox.Size = new System.Drawing.Size(435, 51);
		this.m_GroupBox.TabIndex = 0;
		this.m_GroupBox.TabStop = false;
		this.m_GroupBox.Text = "groupBox1";
		this.m_FilePickerButton.Location = new System.Drawing.Point(405, 19);
		this.m_FilePickerButton.Name = "m_FilePickerButton";
		this.m_FilePickerButton.Size = new System.Drawing.Size(24, 20);
		this.m_FilePickerButton.TabIndex = 2;
		this.m_FilePickerButton.Text = "...";
		this.m_FilePickerButton.UseVisualStyleBackColor = true;
		this.m_FilePickerButton.Click += new System.EventHandler(this.m_FilePickerButton_Click);
		this.m_FileNameTxtBox.Location = new System.Drawing.Point(88, 19);
		this.m_FileNameTxtBox.Name = "m_FileNameTxtBox";
		this.m_FileNameTxtBox.Size = new System.Drawing.Size(311, 20);
		this.m_FileNameTxtBox.TabIndex = 1;
		this.m_FileNameLbl.AutoSize = true;
		this.m_FileNameLbl.Location = new System.Drawing.Point(6, 23);
		this.m_FileNameLbl.Name = "m_FileNameLbl";
		this.m_FileNameLbl.Size = new System.Drawing.Size(35, 13);
		this.m_FileNameLbl.TabIndex = 0;
		this.m_FileNameLbl.Text = "label1";
		this.m_CancelButton.Location = new System.Drawing.Point(372, 154);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 4;
		this.m_CancelButton.Text = "Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButtonClicked);
		this.m_OKButton.Location = new System.Drawing.Point(291, 154);
		this.m_OKButton.Name = "m_OKButton";
		this.m_OKButton.Size = new System.Drawing.Size(75, 23);
		this.m_OKButton.TabIndex = 3;
		this.m_OKButton.Text = "OK";
		this.m_OKButton.UseVisualStyleBackColor = true;
		this.m_OKButton.Click += new System.EventHandler(this.OKButtonClicked);
		this.m_ImportSurfaceCheckbox.AutoSize = true;
		this.m_ImportSurfaceCheckbox.Location = new System.Drawing.Point(12, 69);
		this.m_ImportSurfaceCheckbox.Name = "m_ImportSurfaceCheckbox";
		this.m_ImportSurfaceCheckbox.Size = new System.Drawing.Size(100, 17);
		this.m_ImportSurfaceCheckbox.TabIndex = 1;
		this.m_ImportSurfaceCheckbox.Text = "Import Surfaces";
		this.m_ImportSurfaceCheckbox.UseVisualStyleBackColor = true;
		this.m_ImportAlignmentCheckBox.AutoSize = true;
		this.m_ImportAlignmentCheckBox.Location = new System.Drawing.Point(12, 92);
		this.m_ImportAlignmentCheckBox.Name = "m_ImportAlignmentCheckBox";
		this.m_ImportAlignmentCheckBox.Size = new System.Drawing.Size(109, 17);
		this.m_ImportAlignmentCheckBox.TabIndex = 2;
		this.m_ImportAlignmentCheckBox.Text = "Import Alignments";
		this.m_ImportAlignmentCheckBox.UseVisualStyleBackColor = true;
		this.m_ImportOptionsLbl.AutoSize = true;
		this.m_ImportOptionsLbl.Location = new System.Drawing.Point(12, 116);
		this.m_ImportOptionsLbl.Name = "m_ImportOptionsLbl";
		this.m_ImportOptionsLbl.Size = new System.Drawing.Size(0, 13);
		this.m_ImportOptionsLbl.TabIndex = 5;
		this.m_ImportOptionLbl.AutoSize = true;
		this.m_ImportOptionLbl.Location = new System.Drawing.Point(11, 119);
		this.m_ImportOptionLbl.Name = "m_ImportOptionLbl";
		this.m_ImportOptionLbl.Size = new System.Drawing.Size(58, 13);
		this.m_ImportOptionLbl.TabIndex = 6;
		this.m_ImportOptionLbl.Text = "Positioning";
		this.m_ImportOptionCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_ImportOptionCmbBox.FormattingEnabled = true;
		this.m_ImportOptionCmbBox.Location = new System.Drawing.Point(100, 116);
		this.m_ImportOptionCmbBox.Name = "m_ImportOptionCmbBox";
		this.m_ImportOptionCmbBox.Size = new System.Drawing.Size(347, 21);
		this.m_ImportOptionCmbBox.TabIndex = 7;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(459, 183);
		base.Controls.Add(this.m_ImportOptionLbl);
		base.Controls.Add(this.m_ImportOptionCmbBox);
		base.Controls.Add(this.m_ImportOptionsLbl);
		base.Controls.Add(this.m_ImportAlignmentCheckBox);
		base.Controls.Add(this.m_ImportSurfaceCheckbox);
		base.Controls.Add(this.m_OKButton);
		base.Controls.Add(this.m_CancelButton);
		base.Controls.Add(this.m_GroupBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.MinimumSize = new System.Drawing.Size(465, 182);
		base.Name = "ImportLandXMLManager";
		this.Text = "Import LandXML";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnImportLandXMLClosing);
		this.m_GroupBox.ResumeLayout(false);
		this.m_GroupBox.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_OKButton;
		base.CancelButton = this.m_CancelButton;
	}
}
