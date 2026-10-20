using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SITES.Utilities;

namespace EPCommon;

public class SiteComponentCreation
{
	private Document m_Document;

	private LocateSiteComponentForm m_DisplayForm;

	private LocateSiteCurbComponentForm m_CurbDisplayForm;

	private LocateSiteParkingLotComponentForm m_ParkingLotDisplayForm;

	private LocateSiteFeatureLineComponentForm m_FeatureLineDisplayForm;

	private LocateSitePointWipeoutComponentForm m_PointWipeoutDisplayForm;

	private TopographySurfaceContainer m_BaseSurface;

	private TopographySurfaceContainer m_ProposedSurface;

	private List<FamilyContainer> m_FamilyList;

	private EPTransactionContainer m_TransactionContainer;

	private FamilySymbol m_PlacementSymbol;

	private FamilySymbol m_IntersectionPlacementSymbol;

	private FamilyInstance m_BasePlacementFamilyInstance;

	private FamilyInstance m_ProposedPlacementFamilyInstance;

	private LandcaddID m_CurrentLandcaddId = LandcaddID.SoftTerrain;

	private int m_nMaxExistingTopoDrawOrder;

	private bool m_bContinueWithoutLoading;

	private const string m_strNummericSeperator = " - ";

	public Document Document => m_Document;

	public ReadOnlyCollection<FamilyContainer> FamilyList => new ReadOnlyCollection<FamilyContainer>(m_FamilyList);

	public EPTransactionContainer TransactionContainer
	{
		get
		{
			return m_TransactionContainer;
		}
		set
		{
			m_TransactionContainer = value;
		}
	}

	public string NUMMERIC_SEPERATOR => " - ";

	public SiteComponentCreation(Document _CurDocument, TopographySurfaceContainer _BaseSurface, FamilyInstance _BasePlacementFamilyInstance, TopographySurfaceContainer _ProposedSurface, FamilyInstance _ProposedPlacementFamilyInstance, LandcaddID _CurrentLandcaddID)
	{
		m_Document = _CurDocument;
		m_CurrentLandcaddId = _CurrentLandcaddID;
		m_TransactionContainer = new EPTransactionContainer(m_Document);
		m_BaseSurface = _BaseSurface;
		m_BasePlacementFamilyInstance = _BasePlacementFamilyInstance;
		m_ProposedSurface = _ProposedSurface;
		m_ProposedPlacementFamilyInstance = _ProposedPlacementFamilyInstance;
		m_FamilyList = new List<FamilyContainer>();
		m_PlacementSymbol = null;
		m_IntersectionPlacementSymbol = null;
		m_nMaxExistingTopoDrawOrder = -1;
		InitializeListData();
	}

	public void PopulateFamilyAndSymbolLists()
	{
		PopulateFamilyAndSymbolLists(_bCheckForFamily: false);
	}

	private void PopulateFamilyAndSymbolLists(bool _bCheckForFamily)
	{
		int num = 0;
		m_FamilyList.Clear();
		Parameter CurSiteworksID = null;
		FamilySymbol familySymbol = null;
		FilteredElementCollector filteredElementCollector = null;
		List<Element> list;
		if (m_CurrentLandcaddId != LandcaddID.FeatureLine && m_CurrentLandcaddId != LandcaddID.PointWipeout)
		{
			filteredElementCollector = new FilteredElementCollector(m_Document);
			filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Site);
			IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (CurSiteworksID = CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(m_CurrentLandcaddId).ToString("000"), ignoreCase: true) == 0);
			list = source.ToList();
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
		}
		bool flag2 = true;
		bool flag3 = true;
		filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Site);
		CurSiteworksID = null;
		IEnumerable<Element> source2 = filteredElementCollector.Where((Element CurrentElement) => (CurSiteworksID = CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && (string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0 || string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(LandcaddID.IntersectionPlacement).ToString("000"), ignoreCase: true) == 0));
		list = source2.ToList();
		flag2 = false;
		if (m_CurrentLandcaddId == LandcaddID.Sidewalk || m_CurrentLandcaddId == LandcaddID.Street)
		{
			flag3 = false;
		}
		familySymbol = null;
		foreach (Element item2 in list)
		{
			if (!(item2 is FamilySymbol { Category: not null } familySymbol3))
			{
				continue;
			}
			CurSiteworksID = familySymbol3.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (CurSiteworksID == null)
			{
				continue;
			}
			if (string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0)
			{
				m_PlacementSymbol = familySymbol3;
				if (m_PlacementSymbol != null && !m_PlacementSymbol.IsActive)
				{
					m_TransactionContainer.Start("ActivateFamilySymbol");
					m_PlacementSymbol.Activate();
					m_TransactionContainer.Commit();
				}
				flag2 = true;
				if (flag3)
				{
					break;
				}
			}
			else if (string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(LandcaddID.IntersectionPlacement).ToString("000"), ignoreCase: true) == 0)
			{
				m_IntersectionPlacementSymbol = familySymbol3;
				if (m_PlacementSymbol != null && !m_PlacementSymbol.IsActive)
				{
					m_TransactionContainer.Start("ActivateFamilySymbol");
					m_PlacementSymbol.Activate();
					m_TransactionContainer.Commit();
				}
				flag3 = true;
				if (flag2)
				{
					break;
				}
			}
		}
		string _strFullFamilyFilename = string.Empty;
		Family _Family = null;
		if (m_CurrentLandcaddId != LandcaddID.FeatureLine && m_CurrentLandcaddId != LandcaddID.PointWipeout)
		{
			if (_bCheckForFamily && m_FamilyList.Count == 0)
			{
				string strSearchFilename = string.Empty;
				string text = string.Empty;
				switch (m_CurrentLandcaddId)
				{
				case LandcaddID.SoftTerrain:
					strSearchFilename = "Sample Soft Terrain.rfa";
					text = EPShared.LocalRM.GetString("LoadSoftTerrainFamilyTransaction");
					break;
				case LandcaddID.Sidewalk:
					strSearchFilename = "Sample Sidewalk.rfa";
					text = EPShared.LocalRM.GetString("LoadSidewalkFamilyTransaction");
					break;
				case LandcaddID.RetainingWall:
					strSearchFilename = "Sample Retaining Wall.rfa";
					text = EPShared.LocalRM.GetString("LoadRetainingWallFamilyTransaction");
					break;
				case LandcaddID.Street:
					strSearchFilename = "Sample Street.rfa";
					text = EPShared.LocalRM.GetString("LoadStreetFamilyTransaction");
					break;
				case LandcaddID.Curb:
					strSearchFilename = "Sample Curb.rfa";
					text = EPShared.LocalRM.GetString("LoadCurbFamilyTransaction");
					break;
				case LandcaddID.ParkingLot:
					strSearchFilename = "Sample Parking Lot.rfa";
					text = EPShared.LocalRM.GetString("LoadParkingLotFamilyTransaction");
					break;
				}
				m_bContinueWithoutLoading = false;
				if (RevitUtility.GetFullFamilyFilenameIfExist(strSearchFilename, ref _strFullFamilyFilename))
				{
					int num2 = _strFullFamilyFilename.LastIndexOf("\\");
					int num3 = _strFullFamilyFilename.LastIndexOf(".");
					string strNewFamilyName = _strFullFamilyFilename.Substring(num2 + 1, num3 - num2 - 1);
					if (CheckForDuplicateSymbols(strNewFamilyName))
					{
						RevitMessageBox.Show(EPShared.LocalRM.GetString("DuplicateFamilySymbolMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
						return;
					}
					m_TransactionContainer.Start(text + Path.GetFileName(_strFullFamilyFilename));
					RevitUtility.LoadFamily(m_Document, _strFullFamilyFilename, out _Family, _bUseFailureHandling: true, ref m_TransactionContainer);
					m_TransactionContainer.Commit();
					if (_Family != null && _Family.GetFamilySymbolIds() != null)
					{
						foreach (ElementId familySymbolId in _Family.GetFamilySymbolIds())
						{
							if (!(m_Document.GetElement(familySymbolId) is FamilySymbol familySymbol4))
							{
								continue;
							}
							Parameter parameter = null;
							parameter = familySymbol4.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
							if (parameter == null || string.Compare(parameter.AsString(), Convert.ToInt32(m_CurrentLandcaddId).ToString("000"), ignoreCase: true) != 0)
							{
								continue;
							}
							bool flag4 = false;
							for (num = 0; num < m_FamilyList.Count; num++)
							{
								if (string.Compare(m_FamilyList[num].FamilyName, familySymbol4.Family.Name, ignoreCase: true) == 0)
								{
									m_FamilyList[num].FamilySymbolList.Add(familySymbol4);
									flag4 = true;
									break;
								}
							}
							if (!flag4)
							{
								FamilyContainer familyContainer2 = new FamilyContainer(familySymbol4.Family.Name);
								familyContainer2.FamilySymbolList.Add(familySymbol4);
								m_FamilyList.Add(familyContainer2);
							}
						}
						if (m_FamilyList.Count <= 0)
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("NoFamiliesGenericMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
						}
					}
				}
			}
		}
		else
		{
			m_bContinueWithoutLoading = true;
		}
		if (!flag2 && RevitUtility.GetFullFamilyFilenameIfExist("Site Component Placement.rfa", ref _strFullFamilyFilename))
		{
			m_TransactionContainer.Start(EPShared.LocalRM.GetString("LocatePlacementFamilyTransaction") + Path.GetFileName(_strFullFamilyFilename));
			RevitUtility.LoadFamily(m_Document, _strFullFamilyFilename, out _Family, _bUseFailureHandling: true, ref m_TransactionContainer);
			m_TransactionContainer.Commit();
			if (_Family != null && _Family.GetFamilySymbolIds() != null)
			{
				using IEnumerator<ElementId> enumerator4 = _Family.GetFamilySymbolIds().GetEnumerator();
				if (enumerator4.MoveNext())
				{
					ElementId current4 = enumerator4.Current;
					if (m_Document.GetElement(current4) is FamilySymbol placementSymbol)
					{
						m_PlacementSymbol = placementSymbol;
					}
				}
			}
		}
		if (!flag3 && RevitUtility.GetFullFamilyFilenameIfExist("Intersection Placement.rfa", ref _strFullFamilyFilename))
		{
			m_TransactionContainer.Start(EPShared.LocalRM.GetString("LocatePlacementFamilyTransaction") + Path.GetFileName(_strFullFamilyFilename));
			RevitUtility.LoadFamily(m_Document, _strFullFamilyFilename, out _Family, _bUseFailureHandling: true, ref m_TransactionContainer);
			m_TransactionContainer.Commit();
			if (_Family != null && _Family.GetFamilySymbolIds() != null)
			{
				using IEnumerator<ElementId> enumerator5 = _Family.GetFamilySymbolIds().GetEnumerator();
				if (enumerator5.MoveNext())
				{
					ElementId current5 = enumerator5.Current;
					if (m_Document.GetElement(current5) is FamilySymbol intersectionPlacementSymbol)
					{
						m_IntersectionPlacementSymbol = intersectionPlacementSymbol;
					}
				}
			}
		}
		FamilySymbolComparer comparer = new FamilySymbolComparer();
		for (num = 0; num < m_FamilyList.Count; num++)
		{
			m_FamilyList[num].FamilySymbolList.Sort(comparer);
		}
	}

	private void InitializeListData()
	{
		PopulateFamilyAndSymbolLists(_bCheckForFamily: true);
	}

	public void Run()
	{
		try
		{
			DisplayUI();
		}
		finally
		{
			m_TransactionContainer?.Dispose();
			m_TransactionContainer = null;
		}
	}

	private bool DisplayUI()
	{
		bool flag = false;
		if (!m_bContinueWithoutLoading && m_FamilyList.Count <= 0)
		{
			string text = EPShared.LocalRM.GetString("NoString") + " ";
			string text2 = string.Empty;
			switch (m_CurrentLandcaddId)
			{
			case LandcaddID.Street:
				text2 = EPShared.LocalRM.GetString("StreetFamilyManagerRibbonButton");
				break;
			case LandcaddID.Curb:
				text2 = EPShared.LocalRM.GetString("CurbFamilyManagerRibbonButton");
				break;
			case LandcaddID.Sidewalk:
				text2 = EPShared.LocalRM.GetString("LocateSidewalkRibbonButton");
				break;
			case LandcaddID.RetainingWall:
				text2 = EPShared.LocalRM.GetString("LocateRetainingWallRibbonButton");
				break;
			case LandcaddID.Steps:
				text2 = EPShared.LocalRM.GetString("");
				break;
			case LandcaddID.ParkingLot:
				text2 = EPShared.LocalRM.GetString("ParkingLotFamilyManagerRibbonButton");
				break;
			case LandcaddID.SoftTerrain:
				text2 = EPShared.LocalRM.GetString("ModifySoftTerrainRibbonButton");
				break;
			}
			text = text + text2 + EPShared.LocalRM.GetString("NoFamiliesSitesMessage") + text2 + EPShared.LocalRM.GetString("NoFamiliesGenericCreateMessage");
			RevitMessageBox.Show(text, EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
		}
		DialogResult _DialogResult = DialogResult.OK;
		if (m_CurrentLandcaddId == LandcaddID.Curb)
		{
			using (m_CurbDisplayForm = new LocateSiteCurbComponentForm(this))
			{
				while (_DialogResult != DialogResult.Cancel)
				{
					flag = ShowDialog(ref _DialogResult);
					if (_DialogResult != DialogResult.Retry)
					{
						if (flag)
						{
							m_TransactionContainer.Commit();
							m_TransactionContainer.AssimilateTransactionGroup();
							m_CurbDisplayForm.ResetSiteComponentName();
						}
						else
						{
							m_TransactionContainer.Rollback();
							m_TransactionContainer.RollbackTransactionGroup();
						}
					}
				}
			}
		}
		else if (m_CurrentLandcaddId == LandcaddID.ParkingLot)
		{
			using (m_ParkingLotDisplayForm = new LocateSiteParkingLotComponentForm(this))
			{
				while (_DialogResult != DialogResult.Cancel)
				{
					flag = ShowDialog(ref _DialogResult);
					if (_DialogResult != DialogResult.Retry)
					{
						if (flag)
						{
							m_TransactionContainer.Commit();
							m_TransactionContainer.AssimilateTransactionGroup();
							m_ParkingLotDisplayForm.ResetSiteComponentName();
						}
						else
						{
							m_TransactionContainer.Rollback();
							m_TransactionContainer.RollbackTransactionGroup();
						}
					}
				}
			}
		}
		else if (m_CurrentLandcaddId == LandcaddID.FeatureLine)
		{
			using (m_FeatureLineDisplayForm = new LocateSiteFeatureLineComponentForm(this))
			{
				while (_DialogResult != DialogResult.Cancel)
				{
					flag = ShowDialog(ref _DialogResult);
					if (_DialogResult != DialogResult.Retry)
					{
						if (flag)
						{
							m_TransactionContainer.Commit();
							m_TransactionContainer.AssimilateTransactionGroup();
						}
						else
						{
							m_TransactionContainer.Rollback();
							m_TransactionContainer.RollbackTransactionGroup();
						}
					}
				}
			}
		}
		else if (m_CurrentLandcaddId == LandcaddID.PointWipeout)
		{
			using (m_PointWipeoutDisplayForm = new LocateSitePointWipeoutComponentForm(this))
			{
				while (_DialogResult != DialogResult.Cancel)
				{
					flag = ShowDialog(ref _DialogResult);
					if (_DialogResult != DialogResult.Retry)
					{
						if (flag)
						{
							m_TransactionContainer.Commit();
							m_TransactionContainer.AssimilateTransactionGroup();
						}
						else
						{
							m_TransactionContainer.Rollback();
							m_TransactionContainer.RollbackTransactionGroup();
						}
					}
				}
			}
		}
		else
		{
			using (m_DisplayForm = new LocateSiteComponentForm(this, m_CurrentLandcaddId))
			{
				while (_DialogResult != DialogResult.Cancel)
				{
					flag = ShowDialog(ref _DialogResult);
					if (_DialogResult != DialogResult.Retry)
					{
						if (flag)
						{
							m_TransactionContainer.Commit();
							m_TransactionContainer.AssimilateTransactionGroup();
							m_DisplayForm.ResetSiteComponentName();
						}
						else
						{
							m_TransactionContainer.Rollback();
							m_TransactionContainer.RollbackTransactionGroup();
						}
					}
				}
			}
		}
		return flag;
	}

	private bool ShowDialog(ref DialogResult _DialogResult)
	{
		int _nReturnValue = 0;
		if ((m_CurrentLandcaddId == LandcaddID.SoftTerrain || m_CurrentLandcaddId == LandcaddID.ParkingLot || m_CurrentLandcaddId == LandcaddID.FeatureLine || m_CurrentLandcaddId == LandcaddID.PointWipeout) && ApplicationUtility.Settings.eplSetE_GetSetting("MassCreationOnlyMode", ref _nReturnValue) && _nReturnValue != 0)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("MassCreationOnlyModeOnMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
			_DialogResult = DialogResult.Cancel;
			return false;
		}
		double _dReturnValue = 0.0;
		ApplicationUtility.Settings.eplSetE_GetSetting("TopographySurfaceArcPointInterval", ref _dReturnValue);
		if (RevitDefines.LE(_dReturnValue, 0.0))
		{
			_dReturnValue = 1.0;
			ApplicationUtility.Settings.eplSetE_SetSetting("TopographySurfaceArcPointInterval", _dReturnValue);
		}
		if (m_CurrentLandcaddId == LandcaddID.Curb)
		{
			m_CurbDisplayForm.ShowDialog();
			_DialogResult = m_CurbDisplayForm.DialogResult;
		}
		else if (m_CurrentLandcaddId == LandcaddID.ParkingLot)
		{
			m_ParkingLotDisplayForm.ShowDialog();
			_DialogResult = m_ParkingLotDisplayForm.DialogResult;
		}
		else if (m_CurrentLandcaddId == LandcaddID.FeatureLine)
		{
			m_FeatureLineDisplayForm.ShowDialog();
			_DialogResult = m_FeatureLineDisplayForm.DialogResult;
		}
		else if (m_CurrentLandcaddId == LandcaddID.PointWipeout)
		{
			m_PointWipeoutDisplayForm.ShowDialog();
			_DialogResult = m_PointWipeoutDisplayForm.DialogResult;
		}
		else
		{
			m_DisplayForm.ShowDialog();
			_DialogResult = m_DisplayForm.DialogResult;
		}
		if (_DialogResult == DialogResult.Cancel)
		{
			return false;
		}
		if (_DialogResult == DialogResult.Retry)
		{
			Eps_Point _Point = new Eps_Point();
			RevitUtility.GetPointFromProject(m_Document, EPShared.LocalRM.GetString("PickPointPrompt"), ref _Point, _bDisplayError: false);
			double _dElevation = 0.0;
			if (m_ProposedSurface.GetSurfaceContainingPoint(_Point, ref _dElevation) != null)
			{
				if (m_CurrentLandcaddId == LandcaddID.FeatureLine)
				{
					m_FeatureLineDisplayForm.SetFocusedElevation(_dElevation);
				}
				else if (m_CurrentLandcaddId != LandcaddID.Curb && m_CurrentLandcaddId != LandcaddID.ParkingLot && m_CurrentLandcaddId != LandcaddID.PointWipeout)
				{
					m_DisplayForm.SetFocusedElevation(_dElevation);
				}
			}
			else
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("PointNotInProposedBoundaryMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		else if (_DialogResult == DialogResult.OK)
		{
			_nReturnValue = 0;
			if ((!ApplicationUtility.Settings.eplSetE_GetSetting("SiteComponentVerifyPlacementParametersExist", ref _nReturnValue) || _nReturnValue == 0) && m_PlacementSymbol != null)
			{
				List<FamilySymbolParameterContainer> list = new List<FamilySymbolParameterContainer>();
				FamilySymbolParameterContainer familySymbolParameterContainer = null;
				if (m_CurrentLandcaddId == LandcaddID.FeatureLine)
				{
					familySymbolParameterContainer = new FamilySymbolParameterContainer(RevitParameters.LinearPointIntervalParameter, BuiltInParameterGroup.PG_GEOMETRY, ParameterType.Length, _bIsInstance: true);
					list.Add(familySymbolParameterContainer);
				}
				familySymbolParameterContainer = new FamilySymbolParameterContainer(RevitParameters.BeginningElevationFamilyParameter, BuiltInParameterGroup.PG_GEOMETRY, ParameterType.Length, _bIsInstance: true);
				list.Add(familySymbolParameterContainer);
				familySymbolParameterContainer = new FamilySymbolParameterContainer(RevitParameters.EndingElevationFamilyParameter, BuiltInParameterGroup.PG_GEOMETRY, ParameterType.Length, _bIsInstance: true);
				list.Add(familySymbolParameterContainer);
				familySymbolParameterContainer = new FamilySymbolParameterContainer(RevitParameters.UsePlacementElevationsFamilyParameter, BuiltInParameterGroup.INVALID, ParameterType.YesNo, _bIsInstance: true);
				list.Add(familySymbolParameterContainer);
				RevitUtility.AddParametersToFamilySymbol(m_Document, m_TransactionContainer, list, ref m_PlacementSymbol);
				ApplicationUtility.Settings.eplSetE_SetSetting("SiteComponentVerifyPlacementParametersExist", 1);
			}
			Parameter parameter = null;
			string text = string.Empty;
			switch (m_CurrentLandcaddId)
			{
			case LandcaddID.SoftTerrain:
				text = EPShared.LocalRM.GetString("LocateSoftTerrainTransaction");
				break;
			case LandcaddID.Sidewalk:
				text = EPShared.LocalRM.GetString("LocateSidewalkTransaction");
				break;
			case LandcaddID.RetainingWall:
				text = EPShared.LocalRM.GetString("LocateRetainingWallTransaction");
				break;
			case LandcaddID.Street:
				text = EPShared.LocalRM.GetString("LocateStreetTransaction");
				break;
			case LandcaddID.Curb:
				text = EPShared.LocalRM.GetString("LocateCurbTransaction");
				break;
			case LandcaddID.ParkingLot:
				text = EPShared.LocalRM.GetString("LocateParkingLotTransaction");
				break;
			case LandcaddID.FeatureLine:
				text = EPShared.LocalRM.GetString("LocateFeatureLineTransaction");
				break;
			case LandcaddID.PointWipeout:
				text = EPShared.LocalRM.GetString("LocatePointWipeoutTransaction");
				break;
			}
			m_TransactionContainer.StartTransactionGroup(text);
			m_TransactionContainer.Start(text);
			List<FailureDefinitionId> list2 = new List<FailureDefinitionId>();
			list2.Add(BuiltInFailures.InaccurateFailures.InaccurateLine);
			list2.Add(BuiltInFailures.MassFailures.TooManyGeometries);
			m_TransactionContainer.FailureHandlingOptions(list2);
			FamilySymbol type = null;
			string empty = string.Empty;
			if (m_CurrentLandcaddId == LandcaddID.Curb)
			{
				type = m_CurbDisplayForm.Type;
				empty = m_CurbDisplayForm.SitePlacementName;
			}
			else if (m_CurrentLandcaddId == LandcaddID.ParkingLot)
			{
				type = m_ParkingLotDisplayForm.Type;
				empty = m_ParkingLotDisplayForm.SitePlacementName;
			}
			else if (m_CurrentLandcaddId == LandcaddID.FeatureLine)
			{
				empty = EPShared.LocalRM.GetString("FeatureLineString");
			}
			else if (m_CurrentLandcaddId == LandcaddID.PointWipeout)
			{
				empty = EPShared.LocalRM.GetString("PointWipeoutString");
			}
			else
			{
				type = m_DisplayForm.Type;
				empty = m_DisplayForm.SitePlacementName;
			}
			SitePlacementSettings sitePlacementSettings = null;
			sitePlacementSettings = new SitePlacementSettings(m_CurrentLandcaddId, m_Document.ActiveView.GenLevel, type, m_PlacementSymbol, empty, m_nMaxExistingTopoDrawOrder, m_TransactionContainer);
			sitePlacementSettings.IntersectionPlacementSymbol = m_IntersectionPlacementSymbol;
			if (m_CurrentLandcaddId == LandcaddID.Street && m_DisplayForm != null)
			{
				sitePlacementSettings.ComponentWidth = m_DisplayForm.Width;
			}
			ElementSet elementSet = new ElementSet();
			ElementSet elementSet2 = new ElementSet();
			List<Element> list3 = new List<Element>();
			List<Element> list4 = new List<Element>();
			ElementSet elementSet3 = null;
			string empty2 = string.Empty;
			double num = 0.0;
			double num2 = 0.0;
			double num3 = 0.0;
			double num4 = 0.0;
			double num5 = 0.0;
			bool flag = false;
			switch (m_CurrentLandcaddId)
			{
			case LandcaddID.SoftTerrain:
				parameter = m_DisplayForm.Type.LookupParameter(RevitParameters.SoftTerrainLeftOffsetWidthParameter);
				if (parameter != null)
				{
					num = parameter.AsDouble();
				}
				parameter = m_DisplayForm.Type.LookupParameter(RevitParameters.SoftTerrainRightOffsetWidthParameter);
				if (parameter != null)
				{
					num2 = parameter.AsDouble();
				}
				break;
			case LandcaddID.Sidewalk:
				parameter = m_DisplayForm.Type.LookupParameter(RevitParameters.SideWalkWidthParameter);
				if (parameter != null)
				{
					double num8 = parameter.AsDouble();
					num = num8 / 2.0;
					num2 = num8 / 2.0;
				}
				break;
			case LandcaddID.RetainingWall:
				parameter = m_DisplayForm.Type.LookupParameter(RevitParameters.RetainingWallWidthParameter);
				if (parameter != null)
				{
					double num7 = parameter.AsDouble();
					num = num7 / 2.0;
					num2 = num7 / 2.0;
				}
				parameter = m_DisplayForm.Type.LookupParameter(RevitParameters.RetainingWallSlopeProjectionParameter);
				if (parameter != null)
				{
					flag = parameter.AsInteger() == 1;
				}
				parameter = m_DisplayForm.Type.LookupParameter(RevitParameters.RetainingWallHeightParameter);
				if (parameter != null)
				{
					num4 = parameter.AsDouble();
				}
				parameter = m_DisplayForm.Type.LookupParameter(RevitParameters.LinearIncrementParameter);
				if (parameter != null)
				{
					num5 = parameter.AsDouble();
				}
				if (RevitDefines.LT(num5, 0.0833333))
				{
					num5 = 0.0833333;
				}
				break;
			case LandcaddID.Curb:
			{
				double num9 = 0.0;
				double num10 = 0.0;
				parameter = m_CurbDisplayForm.Type.LookupParameter(RevitParameters.StreetCurbWidthParameter);
				if (parameter != null)
				{
					num9 = parameter.AsDouble();
				}
				parameter = m_CurbDisplayForm.Type.LookupParameter(RevitParameters.StreetGutterWidthParameter);
				if (parameter != null)
				{
					num10 = parameter.AsDouble();
				}
				num = num9;
				num2 = num10;
				flag = false;
				parameter = m_CurbDisplayForm.Type.LookupParameter(RevitParameters.LinearIncrementParameter);
				if (parameter != null)
				{
					num5 = parameter.AsDouble();
				}
				if (RevitDefines.LT(num5, 0.0833333))
				{
					num5 = 0.0833333;
				}
				break;
			}
			case LandcaddID.Street:
			{
				double num6 = 0.0;
				parameter = m_DisplayForm.Type.LookupParameter(RevitParameters.StreetLaneWidthParameter);
				if (parameter != null)
				{
					num6 = parameter.AsDouble();
				}
				parameter = m_DisplayForm.Type.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
				if (parameter != null)
				{
					num = num6 * parameter.AsDouble() / 2.0;
					num2 = num6 * parameter.AsDouble() / 2.0;
				}
				if (m_DisplayForm.Width > 0.0)
				{
					num = m_DisplayForm.Width / 2.0;
					num2 = num;
				}
				break;
			}
			}
			num3 = num + num2;
			if (RevitDefines.EQ(num3, 0.0))
			{
				num3 = 10.0;
			}
			bool flag2 = false;
			if (m_CurrentLandcaddId == LandcaddID.Curb)
			{
				flag2 = m_CurbDisplayForm.ExistingHostLineChecked;
			}
			else if (m_CurrentLandcaddId == LandcaddID.ParkingLot)
			{
				flag2 = m_ParkingLotDisplayForm.ExistingHostLineChecked;
			}
			else if (m_CurrentLandcaddId == LandcaddID.FeatureLine)
			{
				flag2 = m_FeatureLineDisplayForm.ExistingHostLineChecked;
			}
			else
			{
				flag2 = ((m_CurrentLandcaddId != LandcaddID.PointWipeout) ? m_DisplayForm.ExistingHostLineChecked : m_PointWipeoutDisplayForm.ExistingHostLineChecked);
			}
			List<Autodesk.Revit.DB.View> list5 = RevitUtility.FindAllViews(m_Document);
			foreach (Autodesk.Revit.DB.View item in list5)
			{
				list4.Add(item);
			}
			Category siteComponentLineSubCategory = RevitUtility.GetSiteComponentLineSubCategory(m_Document, m_CurrentLandcaddId);
			if (!flag2)
			{
				empty2 = EPShared.LocalRM.GetString("PickStartPointPrompt");
				List<Element> list6 = new List<Element>();
				List<Element> list7 = new List<Element>();
				Eps_Leg eps_Leg = null;
				Eps_Leg eps_Leg2 = null;
				Eps_Leg eps_Leg3 = null;
				Eps_Leg eps_Leg4 = null;
				Eps_Point _Point2 = null;
				Eps_Point eps_Point = null;
				Eps_Line eps_Line = null;
				bool flag3 = true;
				while (flag3)
				{
					flag3 = RevitUtility.GetPointFromProject(m_Document, empty2, ref _Point2, _bDisplayError: false);
					if (!flag3)
					{
						continue;
					}
					if (eps_Point != null)
					{
						if (!eps_Point.IsEqual(_Point2))
						{
							eps_Leg4 = new Eps_Leg();
							if (eps_Leg3 == null)
							{
								eps_Leg4.Line = new Eps_Line();
								eps_Leg4.Line.StartPnt = eps_Point;
								eps_Leg4.Line.EndPnt = _Point2;
								eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg4);
								eps_Leg3 = Eps_Leg.SingleLeg(eps_Leg2);
							}
							else
							{
								eps_Line = new Eps_Line();
								eps_Line.StartPnt = eps_Point;
								eps_Line.EndPnt = _Point2;
								double num11 = 0.0;
								if (m_CurrentLandcaddId == LandcaddID.Curb)
								{
									num11 = m_CurbDisplayForm.Radius;
								}
								else if (m_CurrentLandcaddId == LandcaddID.ParkingLot)
								{
									num11 = m_ParkingLotDisplayForm.Radius;
								}
								else if (m_CurrentLandcaddId == LandcaddID.FeatureLine)
								{
									num11 = m_FeatureLineDisplayForm.Radius;
								}
								else
								{
									num11 = ((m_CurrentLandcaddId != LandcaddID.PointWipeout) ? m_DisplayForm.Radius : m_PointWipeoutDisplayForm.Radius);
								}
								Eps_Arc _pArc = new Eps_Arc();
								if (RevitGeometry.SolveArcFromTangentLines(eps_Leg3.Last.Line, eps_Line, num11, ref _pArc, _bVerifyTangentLength: true) == 0)
								{
									eps_Leg4.Line = new Eps_Line();
									eps_Leg4.Line.StartPnt = new Eps_Point(eps_Leg3.Last.Line.StartPnt);
									eps_Leg4.Line.EndPnt = new Eps_Point(_pArc.StartPnt);
									eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg4);
									eps_Leg4 = new Eps_Leg();
									eps_Leg4.Arc = new Eps_Arc(_pArc);
									eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
									eps_Leg4 = new Eps_Leg();
									eps_Leg4.Line = new Eps_Line();
									eps_Leg4.Line.StartPnt = new Eps_Point(_pArc.EndPnt);
									eps_Leg4.Line.EndPnt = _Point2;
									eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
									eps_Leg = Eps_Leg.CopyLeg(eps_Leg3);
									eps_Leg3 = null;
									while (eps_Leg.Next != null)
									{
										if (eps_Leg3 == null)
										{
											eps_Leg3 = Eps_Leg.SingleLeg(eps_Leg);
										}
										else
										{
											eps_Leg3.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
										}
										eps_Leg = eps_Leg.Next;
									}
									if (eps_Leg3 == null)
									{
										eps_Leg3 = Eps_Leg.CopyLeg(eps_Leg2);
									}
									else
									{
										eps_Leg3.Last.Next = Eps_Leg.CopyLeg(eps_Leg2);
									}
								}
								else
								{
									eps_Leg4.Line = eps_Line;
									eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg4);
									eps_Leg3.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
								}
								if (EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg3.LegStartPnt, eps_Leg3.LegEndPnt))
								{
									flag3 = false;
								}
							}
							m_TransactionContainer.BeginTransaction();
							if (eps_Leg2.LegCount > 1)
							{
								if (list6.Count > 0)
								{
									m_Document.Delete(list6[list6.Count - 1].Id);
									list6.RemoveAt(list6.Count - 1);
								}
								if (list7.Count > 0)
								{
									m_Document.Delete(list7[list7.Count - 1].Id);
									list7.RemoveAt(list7.Count - 1);
								}
							}
							ModelCurveArray modelCurveArray = RevitUtility.NewModelCurveArray(m_Document, eps_Leg2);
							foreach (ModelCurve item2 in modelCurveArray)
							{
								if (siteComponentLineSubCategory != null)
								{
									item2.LineStyle = siteComponentLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
								}
								list6.Add(item2);
								elementSet.Insert(item2);
							}
							if (elementSet.Size > 0)
							{
								RevitUtility.HideElementsInAllViews(m_Document, elementSet);
								elementSet.Clear();
							}
							modelCurveArray = RevitUtility.NewModelCurveArray(m_Document, eps_Leg2);
							foreach (ModelCurve item3 in modelCurveArray)
							{
								if (siteComponentLineSubCategory != null)
								{
									item3.LineStyle = siteComponentLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
								}
								list7.Add(item3);
							}
							m_Document.Regenerate();
							m_TransactionContainer.EndTransaction();
							if (m_CurrentLandcaddId == LandcaddID.Curb)
							{
								if (m_CurbDisplayForm.SegmentChecked)
								{
									flag3 = false;
								}
							}
							else if (m_CurrentLandcaddId == LandcaddID.FeatureLine)
							{
								if (m_FeatureLineDisplayForm.SegmentChecked)
								{
									flag3 = false;
								}
							}
							else if (m_CurrentLandcaddId != LandcaddID.ParkingLot && m_CurrentLandcaddId != LandcaddID.PointWipeout && m_DisplayForm.SegmentChecked)
							{
								flag3 = false;
							}
						}
					}
					else
					{
						empty2 = EPShared.LocalRM.GetString("PickEndPointPrompt");
					}
					eps_Point = _Point2;
				}
				for (int i = 0; i < list7.Count; i++)
				{
					elementSet2.Insert(list7[i]);
				}
				bool result = true;
				if (eps_Leg3 != null)
				{
					if (eps_Leg3.IsLegClosed())
					{
						Eps_Arc _pArc2 = new Eps_Arc();
						double num12 = 0.0;
						if (RevitGeometry.SolveArcFromTangentLines(_dRadius: (m_CurrentLandcaddId == LandcaddID.Curb) ? m_CurbDisplayForm.Radius : ((m_CurrentLandcaddId == LandcaddID.ParkingLot) ? m_ParkingLotDisplayForm.Radius : ((m_CurrentLandcaddId == LandcaddID.FeatureLine) ? m_FeatureLineDisplayForm.Radius : ((m_CurrentLandcaddId != LandcaddID.PointWipeout) ? m_DisplayForm.Radius : m_PointWipeoutDisplayForm.Radius))), _pIn: eps_Leg3.Last.Line, _pOut: eps_Leg3.Line, _pArc: ref _pArc2, _bVerifyTangentLength: true) == 0)
						{
							eps_Leg4.Line = new Eps_Line();
							eps_Leg4.Line.StartPnt = new Eps_Point(eps_Leg3.Last.Line.StartPnt);
							eps_Leg4.Line.EndPnt = new Eps_Point(_pArc2.StartPnt);
							eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg4);
							eps_Leg4 = new Eps_Leg();
							eps_Leg4.Arc = new Eps_Arc(_pArc2);
							eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
							eps_Leg = Eps_Leg.CopyLeg(eps_Leg3);
							eps_Leg3 = null;
							while (eps_Leg.Next != null)
							{
								if (eps_Leg3 == null)
								{
									eps_Leg4 = new Eps_Leg();
									eps_Leg4.Line = new Eps_Line();
									eps_Leg4.Line.StartPnt = new Eps_Point(_pArc2.EndPnt);
									eps_Leg4.Line.EndPnt = new Eps_Point(eps_Leg.Line.EndPnt);
									eps_Leg3 = Eps_Leg.SingleLeg(eps_Leg4);
								}
								else
								{
									eps_Leg3.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
								}
								eps_Leg = eps_Leg.Next;
							}
							if (eps_Leg3 == null)
							{
								eps_Leg3 = Eps_Leg.CopyLeg(eps_Leg2);
							}
							else
							{
								eps_Leg3.Last.Next = Eps_Leg.CopyLeg(eps_Leg2);
							}
							foreach (ModelCurve item4 in list6)
							{
								m_Document.Delete(item4.Id);
							}
							list6.Clear();
							ModelCurveArray modelCurveArray2 = RevitUtility.NewModelCurveArray(m_Document, eps_Leg3, m_Document.ActiveView, _bUse2011TempLines: false);
							foreach (ModelCurve item5 in modelCurveArray2)
							{
								if (siteComponentLineSubCategory != null)
								{
									item5.LineStyle = siteComponentLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
								}
								list6.Add(item5);
								elementSet.Insert(item5);
							}
							if (elementSet.Size > 0)
							{
								RevitUtility.HideElementsInAllViews(m_Document, elementSet);
								elementSet.Clear();
							}
						}
						if (m_CurrentLandcaddId == LandcaddID.SoftTerrain)
						{
							DialogResult dialogResult = RevitMessageBox.Show(EPShared.LocalRM.GetString("ChainPickedClosedProjectInnerSlopesMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
							if (dialogResult == DialogResult.Yes)
							{
								sitePlacementSettings.ProjectInnerSlopes = 0;
							}
							else
							{
								sitePlacementSettings.ProjectInnerSlopes = 1;
							}
						}
					}
					else
					{
						if (m_CurrentLandcaddId == LandcaddID.ParkingLot)
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("ParkingLotBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							return false;
						}
						if (m_CurrentLandcaddId == LandcaddID.PointWipeout)
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("PointWipeoutBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							return false;
						}
					}
					if (m_CurrentLandcaddId == LandcaddID.RetainingWall)
					{
						if (!m_DisplayForm.RelativeElevationChecked)
						{
							double _dMinElevation = 0.0;
							double _dMaxElevation = 0.0;
							if (eps_Leg3.FindMinAndMaxElevationOfLeg(m_ProposedSurface, num5, 1.0, ref _dMinElevation, ref _dMaxElevation))
							{
								if (RevitDefines.LT(_dMinElevation, m_DisplayForm.Elevation - num4))
								{
									RevitMessageBox.Show(EPShared.LocalRM.GetString("WallCantBePlacedWithDaylightingMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
									return false;
								}
								if (RevitDefines.GT(_dMaxElevation, m_DisplayForm.Elevation))
								{
									RevitMessageBox.Show(EPShared.LocalRM.GetString("WallCantBePlacedWithGreaterElevationMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
									return false;
								}
							}
						}
						else if (m_DisplayForm.RelativeElevationChecked)
						{
							double num13 = 0.0;
							parameter = m_DisplayForm.Type.LookupParameter(RevitParameters.RetainingWallHeightParameter);
							if (parameter != null)
							{
								num13 = parameter.AsDouble();
							}
							if (m_DisplayForm.Elevation - num13 > 0.0)
							{
								RevitMessageBox.Show(EPShared.LocalRM.GetString("WallCantBePlacedWithDaylightingMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
								return false;
							}
						}
						if (flag)
						{
							bool flag4 = true;
							bool _UseRightSide = true;
							if (!RevitUtility.PickSideLeg(m_Document, eps_Leg3, ref _UseRightSide, EPShared.LocalRM.GetString("PickSideForGradeToPlaceMessage")))
							{
								return false;
							}
							sitePlacementSettings.UseRightSide = _UseRightSide;
						}
					}
					else if (m_CurrentLandcaddId == LandcaddID.Curb)
					{
						bool _UseRightSide2 = false;
						if (eps_Leg3.IsLegClosed())
						{
							Eps_Leg ppOffsetLegList = new Eps_Leg();
							RevitGeometry.OffsetLegList(eps_Leg3, num + num2, 1, ref ppOffsetLegList, TrimClosedObject: true);
							Eps_Leg ppOffsetLegList2 = new Eps_Leg();
							RevitGeometry.OffsetLegList(eps_Leg3, num + num2, 2, ref ppOffsetLegList2, TrimClosedObject: true);
							if (RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2) < RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList))
							{
								eps_Leg3 = eps_Leg3.Reverse();
							}
						}
						else
						{
							bool flag5 = true;
							if (!RevitUtility.PickSideLeg(m_Document, eps_Leg3, ref _UseRightSide2, EPShared.LocalRM.GetString("PickSideForFaceOfCurbMessage")))
							{
								return false;
							}
						}
						sitePlacementSettings.UseRightSide = _UseRightSide2;
					}
					if (m_CurrentLandcaddId == LandcaddID.Curb)
					{
						SetBaseToposurface setBaseToposurface = new SetBaseToposurface(Document, _bPickIndividualSurface: true);
						if (!setBaseToposurface.Run())
						{
							return false;
						}
						elementSet3 = setBaseToposurface.InstanceSet;
						bool flag6 = false;
						bool flag7 = false;
						TopographySurfaceContainer _ProposedSurface = null;
						FamilyInstance _TopoPlacementFamilyInstance = null;
						foreach (Element item6 in elementSet3)
						{
							if (TerrainApi.AsSurface(item6) is {} topographySurface)
							{
								_ProposedSurface = new TopographySurfaceContainer(m_Document.Application, topographySurface, _bFindMinMaxPoints: true);
							}
						}
						if (_ProposedSurface == null || string.Compare(_ProposedSurface.TopographySurface.UniqueId, m_ProposedSurface.TopographySurface.UniqueId) == 0)
						{
							_ProposedSurface = m_ProposedSurface;
							_TopoPlacementFamilyInstance = m_ProposedPlacementFamilyInstance;
							flag6 = true;
						}
						else if (m_BaseSurface != null && string.Compare(_ProposedSurface.TopographySurface.UniqueId, m_BaseSurface.TopographySurface.UniqueId) == 0)
						{
							_ProposedSurface = m_BaseSurface;
							_TopoPlacementFamilyInstance = m_BasePlacementFamilyInstance;
							flag7 = true;
						}
						sitePlacementSettings.IsRelativeElevation = true;
						sitePlacementSettings.Elevation = 0.0;
						if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(m_Document, eps_Leg3, sitePlacementSettings, _dReturnValue, list6, ref _ProposedSurface, ref _TopoPlacementFamilyInstance, ref m_nMaxExistingTopoDrawOrder, list4))
						{
							return false;
						}
						if (flag6)
						{
							m_ProposedSurface = _ProposedSurface;
						}
						else if (m_BaseSurface != null && flag7)
						{
							m_BaseSurface = _ProposedSurface;
						}
					}
					else if (m_CurrentLandcaddId == LandcaddID.ParkingLot || m_CurrentLandcaddId == LandcaddID.PointWipeout)
					{
						if (m_CurrentLandcaddId == LandcaddID.PointWipeout && m_PointWipeoutDisplayForm.ShowAffectedPointsChecked)
						{
							List<Eps_Point> paPntList = m_ProposedSurface.GetAllVertices();
							RevitGeometry.GetAllPointsInBoundary(ref paPntList, eps_Leg3, null);
							if (paPntList.Count > 0)
							{
								Eps_Line eps_Line2 = null;
								ModelCurve modelCurve5 = null;
								m_TransactionContainer.BeginTransaction();
								for (int j = 0; j < paPntList.Count; j++)
								{
									eps_Line2 = new Eps_Line();
									eps_Line2.StartPnt = new Eps_Point(paPntList[j].x + 5.0, paPntList[j].y, 0.0);
									eps_Line2.EndPnt = new Eps_Point(paPntList[j].x - 5.0, paPntList[j].y, 0.0);
									modelCurve5 = RevitUtility.NewModelCurve(m_Document, eps_Line2);
									if (siteComponentLineSubCategory != null)
									{
										modelCurve5.LineStyle = siteComponentLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
									}
									elementSet2.Insert(modelCurve5);
									eps_Line2.StartPnt = new Eps_Point(paPntList[j].x, paPntList[j].y + 5.0, 0.0);
									eps_Line2.EndPnt = new Eps_Point(paPntList[j].x, paPntList[j].y - 5.0, 0.0);
									modelCurve5 = RevitUtility.NewModelCurve(m_Document, eps_Line2);
									if (siteComponentLineSubCategory != null)
									{
										modelCurve5.LineStyle = siteComponentLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
									}
									elementSet2.Insert(modelCurve5);
								}
								m_Document.Regenerate();
								m_TransactionContainer.EndTransaction();
								if (RevitMessageBox.Show(EPShared.LocalRM.GetString("PointsInBoundaryWillBeRemovedMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
								{
									return false;
								}
							}
						}
						sitePlacementSettings.IsRelativeElevation = true;
						sitePlacementSettings.Elevation = 0.0;
						if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(m_Document, eps_Leg3, sitePlacementSettings, _dReturnValue, list6, ref m_ProposedSurface, ref m_ProposedPlacementFamilyInstance, ref m_nMaxExistingTopoDrawOrder, list4))
						{
							return false;
						}
					}
					else if (m_CurrentLandcaddId == LandcaddID.FeatureLine)
					{
						sitePlacementSettings.IsRelativeElevation = m_FeatureLineDisplayForm.RelativeElevationChecked;
						sitePlacementSettings.Elevation = m_FeatureLineDisplayForm.Elevation;
						sitePlacementSettings.Grade = m_FeatureLineDisplayForm.GetGrade(RevitGeometry.eplGeoE_CalcLegListLength(eps_Leg3));
						sitePlacementSettings.LinearPointInterval = m_FeatureLineDisplayForm.LinearInterval;
						if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(m_Document, eps_Leg3, sitePlacementSettings, _dReturnValue, list6, ref m_ProposedSurface, ref m_ProposedPlacementFamilyInstance, ref m_nMaxExistingTopoDrawOrder, list4))
						{
							return false;
						}
					}
					else
					{
						sitePlacementSettings.IsRelativeElevation = m_DisplayForm.RelativeElevationChecked;
						sitePlacementSettings.Elevation = m_DisplayForm.Elevation;
						if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(m_Document, eps_Leg3, sitePlacementSettings, _dReturnValue, list6, ref m_ProposedSurface, ref m_ProposedPlacementFamilyInstance, ref m_nMaxExistingTopoDrawOrder, list4))
						{
							return false;
						}
					}
				}
				if (elementSet2.Size > 0)
				{
					m_TransactionContainer.BeginTransaction();
					m_Document.Delete(ElementSetConverter.ToIDList(elementSet2));
					if ((m_CurrentLandcaddId == LandcaddID.Curb && m_CurbDisplayForm != null && m_CurbDisplayForm.CreateCopyChecked) || (m_CurrentLandcaddId == LandcaddID.ParkingLot && m_ParkingLotDisplayForm != null && m_ParkingLotDisplayForm.CreateCopyChecked) || (m_CurrentLandcaddId == LandcaddID.FeatureLine && m_FeatureLineDisplayForm != null && m_FeatureLineDisplayForm.CreateCopyChecked) || (m_CurrentLandcaddId == LandcaddID.PointWipeout && m_PointWipeoutDisplayForm != null && m_PointWipeoutDisplayForm.CreateCopyChecked) || (m_DisplayForm != null && m_DisplayForm.CreateCopyChecked))
					{
						RevitUtility.NewModelCurveArray(m_Document, eps_Leg3, m_Document.ActiveView, _bUse2011TempLines: false);
					}
					m_TransactionContainer.EndTransaction();
				}
				return result;
			}
			return PickObjectAndCreateSiteComponent(sitePlacementSettings, siteComponentLineSubCategory, num3, num5, _dReturnValue, num4, flag);
		}
		return true;
	}

	private bool PickObjectAndCreateSiteComponent(SitePlacementSettings _PlacementSettings, Category _MassLineSubCategory, double _dOffsetWidth, double _dLinearInterval, double _dArcPointInterval, double _dHeight, bool _bSlopeProjection)
	{
		ElementSet elementSet = new ElementSet();
		ElementSet elementSet2 = new ElementSet();
		List<Element> list = new List<Element>();
		List<Element> list2 = new List<Element>();
		ElementSet _Elements = null;
		int _nReturnValue = 0;
		bool flag = true;
		string strPrompt = EPShared.LocalRM.GetString("PickObjectMessage");
		if (!RevitUtility.GetElementsFromProject(m_Document, strPrompt, ref _Elements))
		{
			return false;
		}
		foreach (Element item5 in _Elements)
		{
			if (item5.Location is LocationCurve { Curve: var curve })
			{
				if (curve != null)
				{
					Eps_Leg _EvalLeg = null;
					if (m_CurrentLandcaddId == LandcaddID.Curb)
					{
						if (m_CurbDisplayForm.ChainChecked)
						{
							_EvalLeg = RevitGeometry.GetLegFromElementList(null, item5);
						}
					}
					else if (m_CurrentLandcaddId == LandcaddID.ParkingLot || m_CurrentLandcaddId == LandcaddID.PointWipeout)
					{
						_EvalLeg = RevitGeometry.GetLegFromElementList(null, item5);
					}
					else if (m_CurrentLandcaddId == LandcaddID.FeatureLine)
					{
						if (m_FeatureLineDisplayForm.ChainChecked)
						{
							_EvalLeg = RevitGeometry.GetLegFromElementList(null, item5);
						}
					}
					else if (m_DisplayForm.ChainChecked)
					{
						_EvalLeg = RevitGeometry.GetLegFromElementList(null, item5);
					}
					if (_EvalLeg != null)
					{
						if (m_CurrentLandcaddId != LandcaddID.ParkingLot && ((m_CurrentLandcaddId == LandcaddID.FeatureLine && !m_FeatureLineDisplayForm.RelativeElevationChecked && !RevitDefines.EQ(m_FeatureLineDisplayForm.GetGrade(RevitGeometry.eplGeoE_CalcLegListLength(_EvalLeg)), 0.0)) || (ApplicationUtility.Settings.eplSetE_GetSetting("SiteComponentPromptForStartingSide", ref _nReturnValue) && _nReturnValue != 0)) && !RevitUtility.EvaluateLeg(m_Document, m_TransactionContainer, 2.0 * _dOffsetWidth, ref _EvalLeg))
						{
							return false;
						}
						if (_EvalLeg.IsLegClosed())
						{
							if (m_CurrentLandcaddId == LandcaddID.SoftTerrain)
							{
								DialogResult dialogResult = RevitMessageBox.Show(EPShared.LocalRM.GetString("ChainPickedClosedProjectInnerSlopesMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
								if (dialogResult == DialogResult.Yes)
								{
									_PlacementSettings.ProjectInnerSlopes = 0;
								}
								else
								{
									_PlacementSettings.ProjectInnerSlopes = 1;
								}
							}
						}
						else
						{
							if (m_CurrentLandcaddId == LandcaddID.ParkingLot)
							{
								RevitMessageBox.Show(EPShared.LocalRM.GetString("ParkingLotBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
								return false;
							}
							if (m_CurrentLandcaddId == LandcaddID.PointWipeout)
							{
								RevitMessageBox.Show(EPShared.LocalRM.GetString("PointWipeoutBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
								return false;
							}
						}
						elementSet2 = _EvalLeg.ElementSet;
						ModelCurveArray modelCurveArray = RevitUtility.NewModelCurveArray(m_Document, _EvalLeg, m_Document.ActiveView, _bUse2011TempLines: false);
						foreach (ModelCurve item6 in modelCurveArray)
						{
							if (_MassLineSubCategory != null)
							{
								item6.LineStyle = _MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
							}
							list.Add(item6);
							elementSet.Insert(item6);
						}
					}
					else if (curve is Arc)
					{
						Arc arc = (Arc)curve;
						if (!arc.IsBound)
						{
							if (m_CurrentLandcaddId == LandcaddID.SoftTerrain)
							{
								DialogResult dialogResult2 = RevitMessageBox.Show(EPShared.LocalRM.GetString("ArcPickedClosedProjectInnerSlopesMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
								if (dialogResult2 == DialogResult.Yes)
								{
									_PlacementSettings.ProjectInnerSlopes = 0;
								}
								else
								{
									_PlacementSettings.ProjectInnerSlopes = 1;
								}
							}
						}
						else
						{
							if (m_CurrentLandcaddId == LandcaddID.ParkingLot)
							{
								RevitMessageBox.Show(EPShared.LocalRM.GetString("ParkingLotBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
								return false;
							}
							if (m_CurrentLandcaddId == LandcaddID.PointWipeout)
							{
								RevitMessageBox.Show(EPShared.LocalRM.GetString("PointWipeoutBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
								return false;
							}
						}
						Eps_Arc _EvalArc = new Eps_Arc(arc);
						double dLegListLength = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_EvalArc.Radius, _EvalArc.Delta);
						if (((m_CurrentLandcaddId == LandcaddID.FeatureLine && !m_FeatureLineDisplayForm.RelativeElevationChecked && !RevitDefines.EQ(m_FeatureLineDisplayForm.GetGrade(dLegListLength), 0.0)) || (ApplicationUtility.Settings.eplSetE_GetSetting("SiteComponentPromptForStartingSide", ref _nReturnValue) && _nReturnValue != 0)) && !RevitUtility.EvaluateArc(m_Document, m_TransactionContainer, 2.0 * _dOffsetWidth, ref _EvalArc))
						{
							return false;
						}
						elementSet2.Insert(item5);
						ModelCurve modelCurve2 = RevitUtility.NewModelCurve(m_Document, _EvalArc);
						if (_MassLineSubCategory != null)
						{
							modelCurve2.LineStyle = _MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
						}
						elementSet.Insert(modelCurve2);
						list.Add(modelCurve2);
						_EvalLeg = new Eps_Leg();
						_EvalLeg.Arc = new Eps_Arc(_EvalArc);
					}
					else
					{
						if (!(curve is Line))
						{
							flag = false;
							RevitMessageBox.Show(EPShared.LocalRM.GetString("ObjectSelectedNotSupportedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
							return PickObjectAndCreateSiteComponent(_PlacementSettings, _MassLineSubCategory, _dOffsetWidth, _dLinearInterval, _dArcPointInterval, _dHeight, _bSlopeProjection);
						}
						if (m_CurrentLandcaddId == LandcaddID.ParkingLot)
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("ParkingLotBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							return false;
						}
						if (m_CurrentLandcaddId == LandcaddID.PointWipeout)
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("PointWipeoutBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							return false;
						}
						Line line = (Line)curve;
						Eps_Line _EvalLine = new Eps_Line(line);
						double _dDistance = 0.0;
						EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_EvalLine.EndPnt, _EvalLine.StartPnt, ref _dDistance);
						if (((m_CurrentLandcaddId == LandcaddID.FeatureLine && !m_FeatureLineDisplayForm.RelativeElevationChecked && !RevitDefines.EQ(m_FeatureLineDisplayForm.GetGrade(_dDistance), 0.0)) || (ApplicationUtility.Settings.eplSetE_GetSetting("SiteComponentPromptForStartingSide", ref _nReturnValue) && _nReturnValue != 0)) && !RevitUtility.EvaluateLine(m_Document, m_TransactionContainer, 2.0 * _dOffsetWidth, ref _EvalLine))
						{
							return false;
						}
						elementSet2.Insert(item5);
						ModelCurve modelCurve3 = RevitUtility.NewModelCurve(m_Document, _EvalLine);
						if (_MassLineSubCategory != null)
						{
							modelCurve3.LineStyle = _MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
						}
						elementSet.Insert(modelCurve3);
						list.Add(modelCurve3);
						_EvalLeg = new Eps_Leg();
						_EvalLeg.Line = new Eps_Line(_EvalLine);
					}
					if (elementSet.Size > 0)
					{
						RevitUtility.HideElementsInAllViews(m_Document, elementSet);
					}
					if (m_CurrentLandcaddId != LandcaddID.SoftTerrain && m_CurrentLandcaddId != LandcaddID.ParkingLot && m_CurrentLandcaddId != LandcaddID.FeatureLine && m_CurrentLandcaddId != LandcaddID.PointWipeout)
					{
						Eps_Leg eps_Leg = null;
						for (eps_Leg = _EvalLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
						{
							if (eps_Leg.Arc != null && RevitDefines.LT(eps_Leg.Arc.Radius, _dOffsetWidth))
							{
								RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidRadiusString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
								return false;
							}
						}
					}
					if (m_CurrentLandcaddId == LandcaddID.RetainingWall)
					{
						if (!m_DisplayForm.RelativeElevationChecked)
						{
							double _dMinElevation = 0.0;
							double _dMaxElevation = 0.0;
							if (_EvalLeg.FindMinAndMaxElevationOfLeg(m_ProposedSurface, _dLinearInterval, 1.0, ref _dMinElevation, ref _dMaxElevation))
							{
								if (RevitDefines.LT(_dMinElevation, m_DisplayForm.Elevation - _dHeight))
								{
									RevitMessageBox.Show(EPShared.LocalRM.GetString("WallCantBePlacedWithDaylightingMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
									return false;
								}
								if (RevitDefines.GT(_dMaxElevation, m_DisplayForm.Elevation))
								{
									RevitMessageBox.Show(EPShared.LocalRM.GetString("WallCantBePlacedWithGreaterElevationMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
									return false;
								}
							}
						}
						if (_bSlopeProjection)
						{
							bool flag2 = true;
							bool _UseRightSide = true;
							if (!RevitUtility.PickSideLeg(m_Document, _EvalLeg, ref _UseRightSide, EPShared.LocalRM.GetString("PickSideForGradeToPlaceMessage")))
							{
								return false;
							}
							_PlacementSettings.UseRightSide = _UseRightSide;
						}
						_PlacementSettings.IsRelativeElevation = m_DisplayForm.RelativeElevationChecked;
						_PlacementSettings.Elevation = m_DisplayForm.Elevation;
					}
					else if (m_CurrentLandcaddId == LandcaddID.Curb)
					{
						bool _UseRightSide2 = false;
						if (_EvalLeg.IsLegClosed())
						{
							Eps_Leg ppOffsetLegList = new Eps_Leg();
							RevitGeometry.OffsetLegList(_EvalLeg, _dOffsetWidth, 1, ref ppOffsetLegList, TrimClosedObject: true);
							Eps_Leg ppOffsetLegList2 = new Eps_Leg();
							RevitGeometry.OffsetLegList(_EvalLeg, _dOffsetWidth, 2, ref ppOffsetLegList2, TrimClosedObject: true);
							if (RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2) < RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList))
							{
								_EvalLeg = _EvalLeg.Reverse();
							}
						}
						else
						{
							bool flag3 = true;
							if (!RevitUtility.PickSideLeg(m_Document, _EvalLeg, ref _UseRightSide2, EPShared.LocalRM.GetString("PickSideForFaceOfCurbMessage")))
							{
								return false;
							}
						}
						_PlacementSettings.UseRightSide = _UseRightSide2;
					}
					if (m_CurrentLandcaddId == LandcaddID.Curb)
					{
						SetBaseToposurface setBaseToposurface = new SetBaseToposurface(Document, _bPickIndividualSurface: true);
						if (!setBaseToposurface.Run())
						{
							return false;
						}
						_Elements = setBaseToposurface.InstanceSet;
						bool flag4 = false;
						bool flag5 = false;
						TopographySurfaceContainer _ProposedSurface = null;
						FamilyInstance _TopoPlacementFamilyInstance = null;
						foreach (Element item7 in _Elements)
						{
							if (TerrainApi.AsSurface(item7) is {} topographySurface)
							{
								_ProposedSurface = new TopographySurfaceContainer(m_Document.Application, topographySurface, _bFindMinMaxPoints: true);
							}
						}
						if (_ProposedSurface == null || string.Compare(_ProposedSurface.TopographySurface.UniqueId, m_ProposedSurface.TopographySurface.UniqueId) == 0)
						{
							_ProposedSurface = m_ProposedSurface;
							_TopoPlacementFamilyInstance = m_ProposedPlacementFamilyInstance;
							flag4 = true;
						}
						else if (m_BaseSurface != null && string.Compare(_ProposedSurface.TopographySurface.UniqueId, m_BaseSurface.TopographySurface.UniqueId) == 0)
						{
							_ProposedSurface = m_BaseSurface;
							_TopoPlacementFamilyInstance = m_BasePlacementFamilyInstance;
							flag5 = true;
						}
						_PlacementSettings.IsRelativeElevation = true;
						_PlacementSettings.Elevation = 0.0;
						if (m_CurbDisplayForm.CreateCopyChecked && elementSet2.Size > 0)
						{
							elementSet2.Clear();
						}
						else
						{
							foreach (Element item8 in elementSet2)
							{
								list2.Add(item8);
							}
						}
						if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(m_Document, _EvalLeg, _PlacementSettings, _dArcPointInterval, list, ref _ProposedSurface, ref _TopoPlacementFamilyInstance, ref m_nMaxExistingTopoDrawOrder, list2))
						{
							return false;
						}
						if (flag4)
						{
							m_ProposedSurface = _ProposedSurface;
						}
						else if (m_BaseSurface != null && flag5)
						{
							m_BaseSurface = _ProposedSurface;
						}
						if (elementSet2.Size > 0)
						{
							m_TransactionContainer.BeginTransaction();
							m_Document.Delete(ElementSetConverter.ToIDList(elementSet2));
							m_TransactionContainer.EndTransaction();
						}
					}
					else if (m_CurrentLandcaddId == LandcaddID.ParkingLot || m_CurrentLandcaddId == LandcaddID.PointWipeout)
					{
						if (m_CurrentLandcaddId == LandcaddID.PointWipeout && m_PointWipeoutDisplayForm.ShowAffectedPointsChecked)
						{
							List<Eps_Point> paPntList = m_ProposedSurface.GetAllVertices();
							RevitGeometry.GetAllPointsInBoundary(ref paPntList, _EvalLeg, null);
							if (paPntList.Count > 0)
							{
								ElementSet elementSet3 = new ElementSet();
								Eps_Line eps_Line = null;
								ModelCurve modelCurve4 = null;
								m_TransactionContainer.BeginTransaction();
								for (int i = 0; i < paPntList.Count; i++)
								{
									eps_Line = new Eps_Line();
									eps_Line.StartPnt = new Eps_Point(paPntList[i].x + 5.0, paPntList[i].y, 0.0);
									eps_Line.EndPnt = new Eps_Point(paPntList[i].x - 5.0, paPntList[i].y, 0.0);
									modelCurve4 = RevitUtility.NewModelCurve(m_Document, eps_Line);
									if (_MassLineSubCategory != null)
									{
										modelCurve4.LineStyle = _MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
									}
									elementSet3.Insert(modelCurve4);
									eps_Line.StartPnt = new Eps_Point(paPntList[i].x, paPntList[i].y + 5.0, 0.0);
									eps_Line.EndPnt = new Eps_Point(paPntList[i].x, paPntList[i].y - 5.0, 0.0);
									modelCurve4 = RevitUtility.NewModelCurve(m_Document, eps_Line);
									if (_MassLineSubCategory != null)
									{
										modelCurve4.LineStyle = _MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
									}
									elementSet3.Insert(modelCurve4);
								}
								m_Document.Regenerate();
								m_TransactionContainer.EndTransaction();
								if (RevitMessageBox.Show(EPShared.LocalRM.GetString("PointsInBoundaryWillBeRemovedMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
								{
									return false;
								}
								if (elementSet3.Size > 0)
								{
									m_TransactionContainer.BeginTransaction();
									m_Document.Delete(ElementSetConverter.ToIDList(elementSet3));
									m_TransactionContainer.EndTransaction();
								}
							}
						}
						_PlacementSettings.IsRelativeElevation = true;
						_PlacementSettings.Elevation = 0.0;
						if (((m_CurrentLandcaddId == LandcaddID.ParkingLot && m_ParkingLotDisplayForm.CreateCopyChecked) || (m_CurrentLandcaddId == LandcaddID.PointWipeout && m_PointWipeoutDisplayForm.CreateCopyChecked)) && elementSet2.Size > 0)
						{
							elementSet2.Clear();
						}
						else
						{
							foreach (Element item9 in elementSet2)
							{
								list2.Add(item9);
							}
						}
						if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(m_Document, _EvalLeg, _PlacementSettings, _dArcPointInterval, list, ref m_ProposedSurface, ref m_ProposedPlacementFamilyInstance, ref m_nMaxExistingTopoDrawOrder, list2))
						{
							return false;
						}
						if (elementSet2.Size > 0)
						{
							m_TransactionContainer.BeginTransaction();
							m_Document.Delete(ElementSetConverter.ToIDList(elementSet2));
							m_TransactionContainer.EndTransaction();
						}
					}
					else if (m_CurrentLandcaddId == LandcaddID.FeatureLine)
					{
						_PlacementSettings.IsRelativeElevation = m_FeatureLineDisplayForm.RelativeElevationChecked;
						_PlacementSettings.Elevation = m_FeatureLineDisplayForm.Elevation;
						_PlacementSettings.Grade = m_FeatureLineDisplayForm.GetGrade(RevitGeometry.eplGeoE_CalcLegListLength(_EvalLeg));
						_PlacementSettings.LinearPointInterval = m_FeatureLineDisplayForm.LinearInterval;
						if (m_FeatureLineDisplayForm.CreateCopyChecked && elementSet2.Size > 0)
						{
							elementSet2.Clear();
						}
						else
						{
							foreach (Element item10 in elementSet2)
							{
								list2.Add(item10);
							}
						}
						if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(m_Document, _EvalLeg, _PlacementSettings, _dArcPointInterval, list, ref m_ProposedSurface, ref m_ProposedPlacementFamilyInstance, ref m_nMaxExistingTopoDrawOrder, list2))
						{
							return false;
						}
						if (elementSet2.Size > 0)
						{
							m_TransactionContainer.BeginTransaction();
							m_Document.Delete(ElementSetConverter.ToIDList(elementSet2));
							m_TransactionContainer.EndTransaction();
						}
					}
					else
					{
						_PlacementSettings.IsRelativeElevation = m_DisplayForm.RelativeElevationChecked;
						_PlacementSettings.Elevation = m_DisplayForm.Elevation;
						if (m_DisplayForm.CreateCopyChecked && elementSet2.Size > 0)
						{
							elementSet2.Clear();
						}
						else
						{
							foreach (Element item11 in elementSet2)
							{
								list2.Add(item11);
							}
						}
						if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(m_Document, _EvalLeg, _PlacementSettings, _dArcPointInterval, list, ref m_ProposedSurface, ref m_ProposedPlacementFamilyInstance, ref m_nMaxExistingTopoDrawOrder, list2))
						{
							return false;
						}
						if (elementSet2.Size > 0)
						{
							m_TransactionContainer.BeginTransaction();
							m_Document.Delete(ElementSetConverter.ToIDList(elementSet2));
							m_TransactionContainer.EndTransaction();
						}
					}
				}
				else
				{
					flag = false;
				}
			}
			else
			{
				flag = false;
			}
			if (!flag)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("ObjectSelectedNotSupportedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return PickObjectAndCreateSiteComponent(_PlacementSettings, _MassLineSubCategory, _dOffsetWidth, _dLinearInterval, _dArcPointInterval, _dHeight, _bSlopeProjection);
			}
		}
		return true;
	}

	public bool FindFamilyName(string _strFindName)
	{
		FamilySymbol familySymbol = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		List<Element> list = filteredElementCollector.OfClass(typeof(FamilySymbol)).ToList();
		foreach (Element item in list)
		{
			if (item is FamilySymbol { Family: not null, Category: not null } familySymbol2 && string.Compare(familySymbol2.Family.Name, _strFindName, ignoreCase: true) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool FindMassingFamilyStartingNumberName(string _strFindName, ref int nBeginningIndex)
	{
		FamilySymbol familySymbol = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		List<Element> list = filteredElementCollector.OfClass(typeof(FamilySymbol)).ToList();
		bool result = false;
		nBeginningIndex = 0;
		int num = -1;
		bool flag = true;
		string empty = string.Empty;
		string empty2 = string.Empty;
		foreach (Element item in list)
		{
			if (!(item is FamilySymbol { Family: not null, Category: not null } familySymbol2) || !familySymbol2.Family.Name.ToUpper().StartsWith(_strFindName.ToUpper()))
			{
				continue;
			}
			int num2 = familySymbol2.Family.Name.LastIndexOf(" - ");
			if (num2 >= 0 && num2 < familySymbol2.Family.Name.Length)
			{
				empty = familySymbol2.Family.Name.Substring(num2);
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
					result = true;
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
			else if (string.Compare(familySymbol2.Family.Name, _strFindName, ignoreCase: true) == 0)
			{
				result = true;
			}
		}
		DirectShape directShape = null;
		filteredElementCollector = new FilteredElementCollector(m_Document);
		IList<Element> list2 = filteredElementCollector.OfClass(typeof(DirectShape)).ToElements();
		foreach (Element item2 in list2)
		{
			if (!(item2 is DirectShape { Category: not null } directShape2) || !directShape2.Name.ToUpper().StartsWith(_strFindName.ToUpper()))
			{
				continue;
			}
			int num3 = directShape2.Name.LastIndexOf(" - ");
			if (num3 >= 0 && num3 < directShape2.Name.Length)
			{
				empty = directShape2.Name.Substring(num3);
				if (!empty.StartsWith(" - ") || empty.Length <= " - ".Length)
				{
					continue;
				}
				empty2 = empty.Substring(" - ".Length);
				for (int j = 0; j < empty2.Length; j++)
				{
					if (!char.IsDigit(empty2[j]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					result = true;
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
			else if (string.Compare(directShape2.Name, _strFindName, ignoreCase: true) == 0)
			{
				result = true;
			}
		}
		nBeginningIndex++;
		return result;
	}

	public bool FindPlacementName(string _strFindName)
	{
		FamilyInstance familyInstance = null;
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		Parameter LocateTypeParameter = null;
		Parameter parameter = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0 && (LocateTypeParameter = FindFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter)) != null && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		string empty = string.Empty;
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance2) || familyInstance2.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
			{
				continue;
			}
			SiteworksIDParameter = familyInstance2.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (SiteworksIDParameter == null)
			{
				continue;
			}
			LocateTypeParameter = familyInstance2.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (LocateTypeParameter != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0)
			{
				parameter = familyInstance2.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
				empty = string.Empty;
				if (parameter != null)
				{
					empty = parameter.AsString();
				}
				if (empty != string.Empty && string.Compare(empty, _strFindName, ignoreCase: true) == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool FindPlacementStartingNumberName(string _strFindName, ref int nBeginningIndex)
	{
		FamilyInstance familyInstance = null;
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		Parameter LocateTypeParameter = null;
		Parameter parameter = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0 && (LocateTypeParameter = FindFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter)) != null && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		bool result = false;
		nBeginningIndex = 0;
		int num = -1;
		bool flag = true;
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance2) || familyInstance2.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
			{
				continue;
			}
			SiteworksIDParameter = familyInstance2.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (SiteworksIDParameter == null)
			{
				continue;
			}
			LocateTypeParameter = familyInstance2.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (LocateTypeParameter == null || string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) != 0)
			{
				continue;
			}
			parameter = familyInstance2.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
			empty3 = string.Empty;
			if (parameter != null)
			{
				empty3 = parameter.AsString();
			}
			if (!(empty3 != string.Empty) || !empty3.ToUpper().StartsWith(_strFindName.ToUpper()))
			{
				continue;
			}
			int num2 = empty3.LastIndexOf(" - ");
			if (num2 >= 0 && num2 < empty3.Length)
			{
				empty = empty3.Substring(num2);
				if (!empty.StartsWith(" - ") || empty.Length <= " - ".Length)
				{
					continue;
				}
				empty2 = empty.Substring(" - ".Length);
				for (int num3 = 0; num3 < empty2.Length; num3++)
				{
					if (!char.IsDigit(empty2[num3]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					result = true;
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
				result = true;
			}
		}
		nBeginningIndex++;
		return result;
	}

	public string GetSitePlacementNameWithoutSeperator(string _strValue)
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

	private bool CheckForDuplicateSymbols(string _strNewFamilyName)
	{
		FamilySymbol familySymbol = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(FamilySymbol));
		List<Element> list = filteredElementCollector.ToList();
		familySymbol = null;
		foreach (Element item in list)
		{
			if (item is FamilySymbol { Category: not null } familySymbol2 && string.Compare(familySymbol2.Family.Name, _strNewFamilyName, ignoreCase: true) == 0)
			{
				return true;
			}
		}
		return false;
	}
}
