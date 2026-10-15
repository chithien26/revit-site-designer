using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SITES.Utilities;

namespace EPCommon;

public class ModifySiteComponent
{
	private Document m_Document;

	private EPTransactionContainer m_TransactionContainer;

	private TopographySurfaceContainer m_BaseSurface;

	private TopographySurfaceContainer m_ProposedSurface;

	private List<FamilyInstanceComponentWrapper> m_SiteCategoryComponentList;

	private List<FamilyInstanceComponentWrapper> m_AllSiteComponentList;

	private List<FamilyInstanceComponentWrapper> m_ParkingLotCurbSiteCompList;

	private LandcaddID m_CurrentFamilyType;

	private FamilyInstanceComponentWrapper m_FamilyInstanceComponentWrapper;

	private ElementSet m_LineElementSet;

	private ElementSet m_SiteCategoryLineElementSet;

	private ModifySiteComponentForm m_DisplayForm;

	private ModifySiteParkingLotComponentForm m_ParkingLotDisplayForm;

	private ModifySitePointWipeoutComponentForm m_PointWipeoutDisplayForm;

	private bool m_ModifyStatus;

	private FamilyInstance m_TopoPlacementFamilyInstance;

	private ElementSet m_AnnotationElevationTextSet;

	private ElementSet m_AnnotationGradeTextSet;

	private double m_dMarkerHeight;

	private bool m_bHasElevationAnnotation = true;

	private bool m_bHasGradeAnnotation = true;

	private bool m_bDeleteSiteComponent;

	private bool m_bRetainingWallUseRightLeg = true;

	private bool m_bResetLineVisibilityCategory;

	private bool m_bFindIntersectionStations;

	private bool m_bDoesMassLineSubCategoryExist;

	private double m_dArcPointInterval;

	private Category m_MassLineSubCategory;

	private string m_strParentTopoSurfaceID = string.Empty;

	public ModifySiteComponent(Document _CurDocument, LandcaddID _CurrType)
	{
		m_Document = _CurDocument;
		m_CurrentFamilyType = _CurrType;
		m_TransactionContainer = new EPTransactionContainer(m_Document);
		m_BaseSurface = null;
		m_ProposedSurface = null;
		m_TopoPlacementFamilyInstance = null;
		m_SiteCategoryComponentList = new List<FamilyInstanceComponentWrapper>();
		m_AllSiteComponentList = new List<FamilyInstanceComponentWrapper>();
		m_ParkingLotCurbSiteCompList = null;
		m_LineElementSet = new ElementSet();
		m_SiteCategoryLineElementSet = new ElementSet();
		m_DisplayForm = null;
		m_ParkingLotDisplayForm = null;
		m_PointWipeoutDisplayForm = null;
		m_ModifyStatus = false;
		m_AnnotationElevationTextSet = null;
		m_AnnotationGradeTextSet = null;
		m_dMarkerHeight = 0.0;
		m_dArcPointInterval = 0.0;
		m_MassLineSubCategory = null;
		InitializeData();
	}

	public void Run()
	{
		int _nReturnValue = 0;
		if ((m_CurrentFamilyType == LandcaddID.SoftTerrain || m_CurrentFamilyType == LandcaddID.ParkingLot || m_CurrentFamilyType == LandcaddID.FeatureLine || m_CurrentFamilyType == LandcaddID.PointWipeout) && ApplicationUtility.Settings.eplSetE_GetSetting("MassCreationOnlyMode", ref _nReturnValue) && _nReturnValue != 0)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("MassCreationOnlyModeOnMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
			return;
		}
		m_TransactionContainer.StartTransactionGroup(EPShared.LocalRM.GetString("ModifySiteComponentTransaction"));
		m_TransactionContainer.Start(EPShared.LocalRM.GetString("ModifySiteComponentTransaction"));
		List<FailureDefinitionId> list = new List<FailureDefinitionId>();
		list.Add(BuiltInFailures.InaccurateFailures.InaccurateLine);
		list.Add(BuiltInFailures.MassFailures.TooManyGeometries);
		m_TransactionContainer.FailureHandlingOptions(list);
		if (m_BaseSurface == null || m_ProposedSurface == null)
		{
			if (RevitUtility.RemoveAllSiteComponents(m_Document, m_TransactionContainer))
			{
				m_TransactionContainer.Commit();
				m_TransactionContainer.AssimilateTransactionGroup();
			}
			else
			{
				m_TransactionContainer.Rollback();
				m_TransactionContainer.RollbackTransactionGroup();
			}
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NoToposurfaceCanBeModifiedMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
			return;
		}
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("TopographySurfaceArcPointInterval", ref m_dArcPointInterval))
		{
			m_dArcPointInterval = 1.0;
			ApplicationUtility.Settings.eplSetE_SetSetting("TopographySurfaceArcPointInterval", m_dArcPointInterval);
		}
		if (PromptPick())
		{
			bool flag = DisplayUI();
			if (m_LineElementSet != null && m_LineElementSet.Size > 0)
			{
				try
				{
					m_TransactionContainer.BeginTransaction();
					m_Document.ActiveView.UnhideElements(ElementSetConverter.ToIDList(m_LineElementSet));
					m_TransactionContainer.EndTransaction();
				}
				catch
				{
				}
			}
			if (m_bResetLineVisibilityCategory)
			{
				Category category = m_Document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
				if (category != null && category.get_Visible(m_Document.ActiveView))
				{
					category.set_Visible(m_Document.ActiveView, false);
				}
			}
			if (flag)
			{
				if (m_AnnotationElevationTextSet != null)
				{
					ICollection<ElementId> collection = m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationElevationTextSet));
					m_AnnotationElevationTextSet = null;
				}
				if (m_AnnotationGradeTextSet != null)
				{
					m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationGradeTextSet));
					m_AnnotationGradeTextSet = null;
				}
				if (m_bHasElevationAnnotation || m_bHasGradeAnnotation)
				{
					RevitUtility.AnnotateGradeAndElevationOnLegList(m_Document, m_TransactionContainer, m_FamilyInstanceComponentWrapper, m_bHasElevationAnnotation, m_bHasGradeAnnotation, _bUseOriginalLeg: false, null, ref m_AnnotationElevationTextSet, ref m_AnnotationGradeTextSet, _bCheckProjectSharedCoordinatesSetting: true);
				}
				m_TransactionContainer.Commit();
				m_TransactionContainer.AssimilateTransactionGroup();
			}
			else
			{
				m_TransactionContainer.Rollback();
				m_TransactionContainer.RollbackTransactionGroup();
			}
		}
		else if (m_bDeleteSiteComponent)
		{
			if (m_AnnotationElevationTextSet != null)
			{
				m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationElevationTextSet));
				m_AnnotationElevationTextSet = null;
			}
			if (m_AnnotationGradeTextSet != null)
			{
				m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationGradeTextSet));
				m_AnnotationGradeTextSet = null;
			}
			m_TransactionContainer.Commit();
			m_TransactionContainer.AssimilateTransactionGroup();
		}
		else
		{
			m_TransactionContainer.Rollback();
			m_TransactionContainer.RollbackTransactionGroup();
		}
	}

	private bool DisplayUI()
	{
		bool result = false;
		DialogResult _DialogResult = DialogResult.Retry;
		if (m_CurrentFamilyType == LandcaddID.ParkingLot)
		{
			using (m_ParkingLotDisplayForm = new ModifySiteParkingLotComponentForm())
			{
				while (_DialogResult == DialogResult.Retry)
				{
					result = ShowDialog(ref _DialogResult);
				}
			}
		}
		else if (m_CurrentFamilyType == LandcaddID.PointWipeout)
		{
			using (m_PointWipeoutDisplayForm = new ModifySitePointWipeoutComponentForm())
			{
				while (_DialogResult == DialogResult.Retry)
				{
					result = ShowDialog(ref _DialogResult);
				}
			}
		}
		else
		{
			using (m_DisplayForm = new ModifySiteComponentForm(m_CurrentFamilyType))
			{
				while (_DialogResult == DialogResult.Retry)
				{
					result = ShowDialog(ref _DialogResult);
				}
			}
		}
		return result;
	}

	private bool ShowDialog(ref DialogResult _DialogResult)
	{
		bool flag = false;
		if (m_CurrentFamilyType == LandcaddID.ParkingLot)
		{
			m_ParkingLotDisplayForm.ShowDialog();
			_DialogResult = m_ParkingLotDisplayForm.DialogResult;
		}
		else if (m_CurrentFamilyType == LandcaddID.PointWipeout)
		{
			m_PointWipeoutDisplayForm.ShowDialog();
			_DialogResult = m_PointWipeoutDisplayForm.DialogResult;
		}
		else
		{
			m_DisplayForm.ShowDialog();
			_DialogResult = m_DisplayForm.DialogResult;
		}
		if (_DialogResult == DialogResult.OK)
		{
			if (m_ModifyStatus)
			{
				flag = ApplySiteAlignmentElementUpdate();
			}
		}
		else if (_DialogResult == DialogResult.Cancel)
		{
			flag = false;
		}
		else if (_DialogResult == DialogResult.Retry)
		{
			Parameter parameter = null;
			if (m_FamilyInstanceComponentWrapper.ProposedFamilySymbol != null)
			{
				switch (m_CurrentFamilyType)
				{
				case LandcaddID.SoftTerrain:
					parameter = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.SoftTerrainLeftOffsetWidthParameter);
					if (parameter != null)
					{
						m_dMarkerHeight = parameter.AsDouble();
					}
					parameter = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.SoftTerrainRightOffsetWidthParameter);
					if (parameter != null)
					{
						m_dMarkerHeight += parameter.AsDouble();
					}
					m_dMarkerHeight *= 2.0;
					break;
				case LandcaddID.Sidewalk:
					parameter = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
					if (parameter != null)
					{
						m_dMarkerHeight = parameter.AsDouble();
					}
					m_dMarkerHeight *= 2.0;
					break;
				case LandcaddID.RetainingWall:
					parameter = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.RetainingWallHeightParameter);
					if (parameter != null)
					{
						m_dMarkerHeight = parameter.AsDouble();
					}
					m_dMarkerHeight *= 2.0;
					break;
				case LandcaddID.Street:
				{
					double num2 = 0.0;
					parameter = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
					if (parameter != null)
					{
						num2 = parameter.AsDouble();
					}
					parameter = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
					if (parameter != null)
					{
						m_dMarkerHeight = num2 * (double)parameter.AsInteger();
					}
					m_dMarkerHeight *= 2.0;
					break;
				}
				case LandcaddID.Curb:
				{
					double num = 0.0;
					parameter = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetCurbWidthParameter);
					if (parameter != null)
					{
						num = parameter.AsDouble();
					}
					parameter = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetGutterWidthParameter);
					if (parameter != null)
					{
						m_dMarkerHeight = num * (double)parameter.AsInteger();
					}
					m_dMarkerHeight *= 2.0;
					break;
				}
				}
			}
			if (RevitDefines.EQ(m_dMarkerHeight, 0.0))
			{
				m_dMarkerHeight = 10.0;
			}
			SiteAlignmentAction siteAlignmentAction = SiteAlignmentAction.AddVertex;
			switch ((m_CurrentFamilyType == LandcaddID.ParkingLot) ? m_ParkingLotDisplayForm.PerformAction : ((m_CurrentFamilyType != LandcaddID.PointWipeout) ? m_DisplayForm.PerformAction : m_PointWipeoutDisplayForm.PerformAction))
			{
			case SiteAlignmentAction.Move:
				flag = MoveSiteAlignmentElement();
				if (flag)
				{
					m_bFindIntersectionStations = true;
					TopographySurfaceManagement.RemoveAllIntersectionStationsForCommonSiteComponents(ref m_AllSiteComponentList);
					m_ModifyStatus = true;
				}
				break;
			case SiteAlignmentAction.Rotate:
				flag = RotateSiteAlignmentElement();
				if (flag)
				{
					m_bFindIntersectionStations = true;
					TopographySurfaceManagement.RemoveAllIntersectionStationsForCommonSiteComponents(ref m_AllSiteComponentList);
					m_ModifyStatus = true;
				}
				break;
			case SiteAlignmentAction.ChangeElevationGrade:
				flag = ChangeGradeAndElevationSiteAlignmentElement();
				if (flag)
				{
					m_ModifyStatus = true;
				}
				break;
			case SiteAlignmentAction.AddVertex:
				flag = AddVertexSiteAlignmentElement();
				if (flag)
				{
					m_ModifyStatus = true;
				}
				break;
			case SiteAlignmentAction.DeleteVertex:
				flag = DeleteVertexSiteAlignmentElement();
				if (flag)
				{
					m_ModifyStatus = true;
				}
				break;
			case SiteAlignmentAction.Delete:
				flag = DeleteSiteAlignmentElement();
				if (flag)
				{
					m_ModifyStatus = true;
					ApplySiteAlignmentElementUpdate();
					_DialogResult = DialogResult.OK;
				}
				break;
			case SiteAlignmentAction.ChangeType:
				flag = ChangeTypeSiteAlignmentElement();
				if (flag)
				{
					m_bFindIntersectionStations = true;
					m_ModifyStatus = true;
				}
				break;
			case SiteAlignmentAction.AddBreakline:
				flag = AddBreaklinesToSiteAlignmentElement();
				if (m_LineElementSet != null && m_LineElementSet.Size > 0)
				{
					m_TransactionContainer.BeginTransaction();
					m_Document.ActiveView.HideElements(ElementSetConverter.ToIDList(m_LineElementSet));
					m_TransactionContainer.EndTransaction();
				}
				m_Document.Regenerate();
				if (flag)
				{
					m_ModifyStatus = true;
				}
				break;
			case SiteAlignmentAction.DeleteBreakline:
				flag = DeleteBreaklinesFromSiteAlignmentElement();
				if (flag)
				{
					m_ModifyStatus = true;
				}
				break;
			case SiteAlignmentAction.ChangeViewScale:
				flag = ChangeViewScale();
				if (flag)
				{
					m_ModifyStatus = true;
				}
				break;
			}
		}
		return flag;
	}

	private bool PromptPick()
	{
		Category category = m_Document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
		if (category != null && !category.get_Visible(m_Document.ActiveView))
		{
			m_TransactionContainer.BeginTransaction();
			category.set_Visible(m_Document.ActiveView, true);
			m_TransactionContainer.EndTransaction();
			m_bResetLineVisibilityCategory = true;
		}
		if (m_LineElementSet != null && m_LineElementSet.Size > 0)
		{
			m_TransactionContainer.BeginTransaction();
			m_Document.ActiveView.HideElements(ElementSetConverter.ToIDList(m_LineElementSet));
			m_TransactionContainer.EndTransaction();
		}
		if ((m_SiteCategoryComponentList != null && m_SiteCategoryComponentList.Count > 0) || (m_SiteCategoryLineElementSet != null && m_SiteCategoryLineElementSet.Size > 0))
		{
			DisplaySiteAlignmentLines();
			m_Document.Regenerate();
			if (!PickSiteAlignmentElement())
			{
				return false;
			}
			m_Document.Regenerate();
			if (m_FamilyInstanceComponentWrapper == null || m_FamilyInstanceComponentWrapper.ProposedElementSet == null)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("PickSiteComponentLineMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
				return false;
			}
			return true;
		}
		string text = EPShared.LocalRM.GetString("ThereAreNoString_");
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
			text += EPShared.LocalRM.GetString("");
			break;
		case LandcaddID.ParkingLot:
			text += EPShared.LocalRM.GetString("ParkingLotFamilyManagerRibbonButton");
			break;
		case LandcaddID.SoftTerrain:
			text += EPShared.LocalRM.GetString("ModifySoftTerrainRibbonButton");
			break;
		case LandcaddID.FeatureLine:
			text += EPShared.LocalRM.GetString("FeatureLineRibbonButton");
			break;
		case LandcaddID.PointWipeout:
			text += EPShared.LocalRM.GetString("PointWipeoutRibbonButton");
			break;
		}
		text += EPShared.LocalRM.GetString("_FeaturesInThisModelString");
		RevitMessageBox.Show(text, EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
		return false;
	}

	private void InitializeData()
	{
		FamilyInstance _BasePlacementFamilyInstance = null;
		TopographySurfaceManagement.FindBaseLandcaddTopographySurface(m_Document, ref m_BaseSurface, ref _BasePlacementFamilyInstance, ref m_ProposedSurface, ref m_TopoPlacementFamilyInstance);
		if (m_BaseSurface == null || m_ProposedSurface == null)
		{
			return;
		}
		EPTransactionContainer ePTransactionContainer = null;
		ePTransactionContainer = new EPTransactionContainer(m_Document);
		ePTransactionContainer.Start(EPShared.LocalRM.GetString("CreateLineSubCategoriesTransaction"));
		GetMassAndLineElements(_bOnlyFindSelSiteComponent: true);
		if (!m_bDoesMassLineSubCategoryExist)
		{
			ePTransactionContainer.Commit();
		}
		else
		{
			ePTransactionContainer.Rollback();
		}
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(CurveElement)).OfCategory(BuiltInCategory.OST_Lines);
		List<Element> list = filteredElementCollector.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Element element = list[i];
			if (element != null && element.Category != null && element.Category.Id.Value64() == BuiltInCategory.OST_Lines.GetHashCode() && !element.IsHidden(m_Document.ActiveView))
			{
				m_LineElementSet.Insert(element);
			}
		}
	}

	private void PopulateSiteComponentLists(List<KeyValuePair<string, Category>> MassLineSubCategoryList, List<Element> LineElementList)
	{
		List<Element> list = new List<Element>();
		FamilyInstance FindFamilyInstance = null;
		List<KeyValuePair<string, FamilyInstanceComponentContainer>> list2 = new List<KeyValuePair<string, FamilyInstanceComponentContainer>>();
		Parameter SiteworksIDParameter = null;
		Parameter LocateTypeParameter = null;
		Parameter parameter = null;
		Parameter parameter2 = null;
		Parameter parameter3 = null;
		Parameter parameter4 = null;
		bool flag = false;
		ModelCurve modelCurve = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0 && (LocateTypeParameter = FindFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter)) != null && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = list.Count;
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarObtainingSiteComponentsMessage");
		foreach (Element item in list)
		{
			if (item is FamilyInstance { Category: not null } familyInstance && familyInstance.Category.Id.Value64() == BuiltInCategory.OST_Site.GetHashCode())
			{
				parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
				if (parameter == null)
				{
					continue;
				}
				LocateTypeParameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
				if (LocateTypeParameter == null)
				{
					continue;
				}
				parameter2 = familyInstance.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
				if (parameter2 != null && parameter2.AsString() != string.Empty)
				{
					for (int num = 0; num < LineElementList.Count; num++)
					{
						Element element = LineElementList[num];
						if (element == null || element.Category == null || element.Category.Id.Value64() != BuiltInCategory.OST_Lines.GetHashCode() || string.Compare(parameter.AsString(), element.UniqueId, ignoreCase: true) != 0)
						{
							continue;
						}
						if (MassLineSubCategoryList != null && !m_bDoesMassLineSubCategoryExist)
						{
							for (int num2 = 0; num2 < MassLineSubCategoryList.Count; num2++)
							{
								if (string.Compare(MassLineSubCategoryList[num2].Key, LocateTypeParameter.AsString(), ignoreCase: true) == 0)
								{
									if (MassLineSubCategoryList[num2].Value != null && element is ModelCurve modelCurve2)
									{
										modelCurve2.LineStyle = MassLineSubCategoryList[num2].Value.GetGraphicsStyle(GraphicsStyleType.Projection);
									}
									break;
								}
							}
						}
						parameter4 = familyInstance.LookupParameter(RevitParameters.IsBreaklineFamilyParameter);
						if (parameter4 != null && parameter4.AsInteger() == 1)
						{
							list2.Add(new KeyValuePair<string, FamilyInstanceComponentContainer>(parameter2.AsString(), new FamilyInstanceComponentContainer(element, familyInstance)));
						}
						else
						{
							for (int num3 = 0; num3 < m_AllSiteComponentList.Count; num3++)
							{
								if (string.Compare(parameter2.AsString(), m_AllSiteComponentList[num3].ChainID, ignoreCase: true) != 0)
								{
									continue;
								}
								flag = true;
								if (string.Compare(LocateTypeParameter.AsString(), Convert.ToInt32(m_CurrentFamilyType).ToString("000"), ignoreCase: true) == 0)
								{
									for (int num4 = 0; num4 < m_SiteCategoryComponentList.Count; num4++)
									{
										if (string.Compare(parameter2.AsString(), m_SiteCategoryComponentList[num4].ChainID, ignoreCase: true) == 0)
										{
											m_SiteCategoryComponentList[num4].FamilyContainerList.Add(new FamilyInstanceComponentContainer(element, familyInstance));
										}
									}
								}
								m_AllSiteComponentList[num3].FamilyContainerList.Add(new FamilyInstanceComponentContainer(element, familyInstance));
								break;
							}
							if (!flag)
							{
								List<FamilyInstanceComponentContainer> list3 = null;
								int nDrawOrder = 0;
								parameter3 = familyInstance.LookupParameter(RevitParameters.TopoDrawOrderFamilyParameter);
								if (parameter3 != null)
								{
									nDrawOrder = parameter3.AsInteger();
								}
								if (string.Compare(LocateTypeParameter.AsString(), Convert.ToInt32(m_CurrentFamilyType).ToString("000"), ignoreCase: true) == 0)
								{
									list3 = new List<FamilyInstanceComponentContainer>();
									list3.Add(new FamilyInstanceComponentContainer(element, familyInstance));
									m_SiteCategoryComponentList.Add(new FamilyInstanceComponentWrapper(parameter2.AsString(), list3, nDrawOrder));
								}
								list3 = new List<FamilyInstanceComponentContainer>();
								list3.Add(new FamilyInstanceComponentContainer(element, familyInstance));
								m_AllSiteComponentList.Add(new FamilyInstanceComponentWrapper(parameter2.AsString(), list3, nDrawOrder));
							}
							else
							{
								flag = false;
							}
						}
						LineElementList.RemoveAt(num);
						break;
					}
					if (LineElementList.Count == 0)
					{
						break;
					}
				}
			}
			progressBarForm.UpdateProgressBar(1);
		}
		for (int num5 = 0; num5 < list2.Count; num5++)
		{
			for (int num6 = 0; num6 < m_SiteCategoryComponentList.Count; num6++)
			{
				if (string.Compare(list2[num5].Key, m_SiteCategoryComponentList[num6].ChainID, ignoreCase: true) == 0)
				{
					m_SiteCategoryComponentList[num6].ParkingLotBreaklineElementList.Add(list2[num5].Value.Element);
					m_SiteCategoryComponentList[num6].ParkingLotBreaklineFamilyInstanceList.Add(list2[num5].Value.FamilyInstance);
					break;
				}
			}
			for (int num7 = 0; num7 < m_AllSiteComponentList.Count; num7++)
			{
				if (string.Compare(list2[num5].Key, m_AllSiteComponentList[num7].ChainID, ignoreCase: true) == 0)
				{
					m_AllSiteComponentList[num7].ParkingLotBreaklineElementList.Add(list2[num5].Value.Element);
					m_AllSiteComponentList[num7].ParkingLotBreaklineFamilyInstanceList.Add(list2[num5].Value.FamilyInstance);
					break;
				}
			}
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
	}

	private void GetMassAndLineElements(bool _bOnlyFindSelSiteComponent)
	{
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(CurveElement)).OfCategory(BuiltInCategory.OST_Lines);
		filteredElementCollector.WhereElementIsViewIndependent();
		List<KeyValuePair<string, Category>> MassLineSubCategoryList = new List<KeyValuePair<string, Category>>();
		if (_bOnlyFindSelSiteComponent && m_CurrentFamilyType != LandcaddID.ParkingLot)
		{
			Category siteComponentLineSubCategory = RevitUtility.GetSiteComponentLineSubCategory(m_Document, m_CurrentFamilyType, ref m_bDoesMassLineSubCategoryExist);
			if (siteComponentLineSubCategory != null)
			{
				MassLineSubCategoryList.Add(new KeyValuePair<string, Category>(Convert.ToInt32(m_CurrentFamilyType).ToString("000"), siteComponentLineSubCategory));
			}
		}
		else if (m_CurrentFamilyType == LandcaddID.ParkingLot)
		{
			MassLineSubCategoryList = RevitUtility.GetAllMassLineSubCategories(m_Document, ref m_bDoesMassLineSubCategoryExist);
		}
		else
		{
			MassLineSubCategoryList = RevitUtility.GetAllMassLineSubCategories(m_Document, ref m_bDoesMassLineSubCategoryExist, Convert.ToInt32(m_CurrentFamilyType).ToString("000"));
		}
		List<Element> list = null;
		if (MassLineSubCategoryList != null && MassLineSubCategoryList.Count > 0)
		{
			for (int nIndex = 0; nIndex < MassLineSubCategoryList.Count; nIndex++)
			{
				if (MassLineSubCategoryList[nIndex].Value == null)
				{
					continue;
				}
				if (m_bDoesMassLineSubCategoryExist)
				{
					Parameter LineStyleParameter = null;
					CurveElement FindCurveElement = null;
					IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindCurveElement = CurrentElement as CurveElement) != null && (LineStyleParameter = ((Element)FindCurveElement).get_Parameter(BuiltInParameter.BUILDING_CURVE_GSTYLE)) != null && LineStyleParameter.AsElementId() == MassLineSubCategoryList[nIndex].Value.GetGraphicsStyle(GraphicsStyleType.Projection).Id);
					if (nIndex == 0)
					{
						list = source.ToList();
					}
					else
					{
						list.AddRange(source.ToList());
					}
				}
				if (string.Compare(MassLineSubCategoryList[nIndex].Key, Convert.ToInt32(m_CurrentFamilyType).ToString("000"), ignoreCase: true) == 0)
				{
					m_MassLineSubCategory = MassLineSubCategoryList[nIndex].Value;
					if (!m_bDoesMassLineSubCategoryExist)
					{
						break;
					}
				}
			}
		}
		if (!m_bDoesMassLineSubCategoryExist)
		{
			list = filteredElementCollector.ToList();
			PopulateSiteComponentLists(MassLineSubCategoryList, list);
		}
		else if (m_CurrentFamilyType == LandcaddID.ParkingLot || !_bOnlyFindSelSiteComponent)
		{
			foreach (Element item in m_SiteCategoryLineElementSet)
			{
				list.Add(item);
			}
			PopulateSiteComponentLists(MassLineSubCategoryList, list);
		}
		else
		{
			for (int num = 0; num < list.Count; num++)
			{
				m_SiteCategoryLineElementSet.Insert(list[num]);
			}
		}
	}

	private List<Element> FindSiteFamilyList(string _strID, bool _bIsChainID)
	{
		Parameter IDParameter = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		if (!_bIsChainID)
		{
			IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (IDParameter = CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter)) != null && string.Compare(IDParameter.AsString(), _strID, ignoreCase: true) == 0);
			return source.ToList();
		}
		IEnumerable<Element> source2 = filteredElementCollector.Where((Element CurrentElement) => (IDParameter = CurrentElement.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter)) != null && string.Compare(IDParameter.AsString(), _strID, ignoreCase: true) == 0);
		return source2.ToList();
	}

	private void FindFamilyInstanceComponentWrapper(Element _CurElement)
	{
		Parameter parameter = null;
		Parameter parameter2 = null;
		Parameter parameter3 = null;
		Parameter parameter4 = null;
		Parameter parameter5 = null;
		List<Element> list = FindSiteFamilyList(_CurElement.UniqueId, _bIsChainID: false);
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
			{
				continue;
			}
			parameter = familyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (parameter == null || string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) != 0)
			{
				continue;
			}
			parameter2 = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (parameter2 == null)
			{
				continue;
			}
			parameter5 = familyInstance.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
			if (parameter5 == null || !(parameter5.AsString() != string.Empty))
			{
				continue;
			}
			int nDrawOrder = 0;
			parameter4 = familyInstance.LookupParameter(RevitParameters.TopoDrawOrderFamilyParameter);
			if (parameter4 != null)
			{
				nDrawOrder = parameter4.AsInteger();
			}
			List<FamilyInstanceComponentContainer> list2 = new List<FamilyInstanceComponentContainer>();
			List<Element> list3 = FindSiteFamilyList(parameter5.AsString(), _bIsChainID: true);
			foreach (Element item2 in list3)
			{
				if (!(item2 is FamilyInstance { Category: not null } familyInstance2) || familyInstance2.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
				{
					continue;
				}
				parameter3 = familyInstance2.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
				if (parameter3 == null || string.Compare(parameter3.AsString(), _CurElement.UniqueId, ignoreCase: true) == 0)
				{
					continue;
				}
				parameter = familyInstance2.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
				if (parameter == null || string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) != 0)
				{
					continue;
				}
				parameter2 = familyInstance2.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
				if (parameter2 == null)
				{
					continue;
				}
				foreach (Element item3 in m_SiteCategoryLineElementSet)
				{
					if (string.Compare(parameter3.AsString(), item3.UniqueId, ignoreCase: true) == 0)
					{
						list2.Add(new FamilyInstanceComponentContainer(item3, familyInstance2));
						break;
					}
				}
			}
			list2.Add(new FamilyInstanceComponentContainer(_CurElement, familyInstance));
			m_FamilyInstanceComponentWrapper = new FamilyInstanceComponentWrapper(parameter5.AsString(), list2, nDrawOrder);
			m_FamilyInstanceComponentWrapper.CreateProposedLeg(m_ProposedSurface);
		}
	}

	private void HideSiteAlignmentLines()
	{
		if (m_SiteCategoryComponentList != null && m_SiteCategoryComponentList.Count > 0)
		{
			ElementSet elementSet = new ElementSet();
			for (int i = 0; i < m_SiteCategoryComponentList.Count; i++)
			{
				foreach (Element item in m_SiteCategoryComponentList[i].GetOriginalElementSet())
				{
					elementSet.Insert(item);
				}
			}
			if (elementSet.Size > 0)
			{
				m_TransactionContainer.BeginTransaction();
				m_Document.ActiveView.HideElements(ElementSetConverter.ToIDList(elementSet));
				m_TransactionContainer.EndTransaction();
			}
		}
		else if (m_SiteCategoryLineElementSet != null && m_SiteCategoryLineElementSet.Size > 0)
		{
			m_TransactionContainer.BeginTransaction();
			m_Document.ActiveView.HideElements(ElementSetConverter.ToIDList(m_SiteCategoryLineElementSet));
			m_TransactionContainer.EndTransaction();
		}
	}

	private void DisplaySiteAlignmentLines()
	{
		if (m_SiteCategoryComponentList != null && m_SiteCategoryComponentList.Count > 0)
		{
			ElementSet elementSet = new ElementSet();
			ElementSet elementSet2 = null;
			for (int i = 0; i < m_SiteCategoryComponentList.Count; i++)
			{
				TopographySurfaceContainer topographySurfaceContainer = null;
				if (m_CurrentFamilyType == LandcaddID.ParkingLot)
				{
					elementSet2 = m_SiteCategoryComponentList[i].GetFamilyInstanceElementSet();
					if (elementSet2 != null && elementSet2.Size > 0)
					{
						string strFormUniqueId = string.Empty;
						foreach (Element item2 in elementSet2)
						{
							Parameter parameter = item2.LookupParameter(RevitParameters.ParkingLotSurfaceIDFamilyParameter);
							if (parameter != null && parameter.AsString() != string.Empty)
							{
								strFormUniqueId = parameter.AsString();
								break;
							}
						}
						TopographySurface topographySurface = TopographySurfaceManagement.FindTopoSurfaceInstance(m_Document, strFormUniqueId);
						if (topographySurface != null)
						{
							topographySurfaceContainer = new TopographySurfaceContainer(m_Document.Application, topographySurface, _bFindMinMaxPoints: true);
						}
					}
					m_SiteCategoryComponentList[i].ParkingLotTopographySurface = topographySurfaceContainer;
				}
				if (topographySurfaceContainer == null)
				{
					topographySurfaceContainer = m_ProposedSurface;
				}
				m_SiteCategoryComponentList[i].CreateProposedLeg(topographySurfaceContainer);
				foreach (Element item3 in m_SiteCategoryComponentList[i].GetOriginalElementSet())
				{
					elementSet.Insert(item3);
				}
			}
			if (elementSet.Size > 0)
			{
				m_TransactionContainer.BeginTransaction();
				m_Document.ActiveView.UnhideElements(ElementSetConverter.ToIDList(elementSet));
				m_TransactionContainer.EndTransaction();
			}
		}
		else if (m_SiteCategoryLineElementSet != null && m_SiteCategoryLineElementSet.Size > 0)
		{
			m_TransactionContainer.BeginTransaction();
			m_Document.ActiveView.UnhideElements(ElementSetConverter.ToIDList(m_SiteCategoryLineElementSet));
			m_TransactionContainer.EndTransaction();
		}
	}

	private bool PickSiteAlignmentElement()
	{
		bool flag = false;
		ElementSet _Elements = null;
		string strPrompt = EPShared.LocalRM.GetString("PickObjectMessage");
		if (!RevitUtility.GetElementsFromProject(m_Document, strPrompt, ref _Elements))
		{
			HideSiteAlignmentLines();
			return false;
		}
		Parameter parameter = null;
		List<string> list = new List<string>();
		foreach (Element item in _Elements)
		{
			if (m_SiteCategoryComponentList != null && m_SiteCategoryComponentList.Count > 0)
			{
				for (int i = 0; i < m_SiteCategoryComponentList.Count; i++)
				{
					if (m_SiteCategoryComponentList[i].IndexOfOriginalElementID(item.Id) >= 0)
					{
						m_FamilyInstanceComponentWrapper = m_SiteCategoryComponentList[i];
						break;
					}
				}
			}
			else
			{
				if (m_SiteCategoryLineElementSet == null || m_SiteCategoryLineElementSet.Size <= 0)
				{
					continue;
				}
				foreach (Element item2 in m_SiteCategoryLineElementSet)
				{
					if (item2.Id == item.Id)
					{
						FindFamilyInstanceComponentWrapper(item);
						break;
					}
				}
			}
		}
		if (m_FamilyInstanceComponentWrapper != null)
		{
			bool _bFamilySymbolPresent = false;
			bool flag2 = false;
			m_FamilyInstanceComponentWrapper.ProposedFamilySymbol = SetInitialProposedFamilySymbol(m_FamilyInstanceComponentWrapper, ref _bFamilySymbolPresent);
			if (m_FamilyInstanceComponentWrapper.ProposedFamilySymbol == null && m_CurrentFamilyType != LandcaddID.FeatureLine && m_CurrentFamilyType != LandcaddID.PointWipeout)
			{
				if (!_bFamilySymbolPresent)
				{
					string empty = string.Empty;
					empty = EPShared.LocalRM.GetString("NoString") + " ";
					empty = empty + GetCurrentTypeString(m_CurrentFamilyType) + " ";
					empty += EPShared.LocalRM.GetString("_FamilyTypeNotFoundDeleteMessage");
					DialogResult dialogResult = RevitMessageBox.Show(empty, EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
					if (dialogResult == DialogResult.Yes && DeleteSiteAlignmentElement())
					{
						m_ModifyStatus = true;
						ApplySiteAlignmentElementUpdate();
						m_bDeleteSiteComponent = true;
					}
					return false;
				}
				List<string> list2 = new List<string>();
				list2.Add(EPShared.LocalRM.GetString("SetFamilyTypeForSiteComponentOption"));
				list2.Add(EPShared.LocalRM.GetString("DeleteSiteComponentOption"));
				string text = EPShared.LocalRM.GetString("The_String");
				text += GetCurrentTypeString(m_CurrentFamilyType);
				text += EPShared.LocalRM.GetString("_FamilyTypeNotFoundMessage");
				WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), m_Document.PathName, "ModifySiteComponent", text, list2, 0, _bDisplayOKCancel: true, _bDisplayDoNotShow: false);
				warningOptionsForm.ShowDialog();
				if (warningOptionsForm.DialogResult == DialogResult.OK)
				{
					if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("SetFamilyTypeForSiteComponentOption"), ignoreCase: true) == 0)
					{
						if (!ChangeTypeSiteAlignmentElement())
						{
							return false;
						}
						m_ModifyStatus = true;
					}
					else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("DeleteSiteComponentOption"), ignoreCase: true) == 0 && DeleteSiteAlignmentElement())
					{
						m_ModifyStatus = true;
						ApplySiteAlignmentElementUpdate();
						m_bDeleteSiteComponent = true;
						return false;
					}
				}
			}
			flag = true;
			HideSiteAlignmentLines();
			if (m_FamilyInstanceComponentWrapper.ProposedLeg != null)
			{
				m_FamilyInstanceComponentWrapper.CreateNewProposedElementSet(m_Document, m_TransactionContainer);
			}
			ElementSet familyInstanceElementSet = m_FamilyInstanceComponentWrapper.GetFamilyInstanceElementSet();
			if (familyInstanceElementSet != null && familyInstanceElementSet.Size > 0)
			{
				List<IntersectionStationContainer> _LeftIntersectionStations = null;
				List<IntersectionStationContainer> _RightIntersectionStations = null;
				List<FamilyInstance> _SiteFamilyInstanceList = null;
				foreach (Element item3 in familyInstanceElementSet)
				{
					if (m_CurrentFamilyType == LandcaddID.Street || m_CurrentFamilyType == LandcaddID.Sidewalk)
					{
						TopographySurfaceManagement.GetIntersections(m_Document, item3.UniqueId, ref _SiteFamilyInstanceList, ref _LeftIntersectionStations, ref _RightIntersectionStations);
					}
					if (_SiteFamilyInstanceList != null && _SiteFamilyInstanceList.Count > 0)
					{
						m_FamilyInstanceComponentWrapper.AddIntersectionFamilyInstances(_SiteFamilyInstanceList);
						m_FamilyInstanceComponentWrapper.IntersectionFamilySymbol = _SiteFamilyInstanceList[0].Symbol;
					}
					if (_LeftIntersectionStations != null && _LeftIntersectionStations.Count > 0)
					{
						m_FamilyInstanceComponentWrapper.LeftIntersectionStations.AddRange(_LeftIntersectionStations);
					}
					if (_RightIntersectionStations != null && _RightIntersectionStations.Count > 0)
					{
						m_FamilyInstanceComponentWrapper.RightIntersectionStations.AddRange(_RightIntersectionStations);
					}
					parameter = item3.LookupParameter(RevitParameters.SiteComponentHasElevationAnnotationParameter);
					if (parameter != null)
					{
						m_bHasElevationAnnotation = parameter.AsInteger() == 1;
					}
					parameter = item3.LookupParameter(RevitParameters.SiteComponentHasGradeAnnotationParameter);
					if (parameter != null)
					{
						m_bHasGradeAnnotation = parameter.AsInteger() == 1;
					}
					parameter = item3.LookupParameter(RevitParameters.SiteComponentAnnotationGradeIDParameter);
					if (parameter != null && parameter.AsString() != string.Empty)
					{
						string text2 = parameter.AsString();
						if (text2 != string.Empty)
						{
							list.Add(text2);
						}
					}
					parameter = item3.LookupParameter(RevitParameters.SiteComponentAnnotationBeginElevationIDParameter);
					if (parameter != null)
					{
						string text3 = parameter.AsString();
						if (text3 != string.Empty)
						{
							list.Add(text3);
						}
					}
					parameter = item3.LookupParameter(RevitParameters.SiteComponentAnnotationEndElevationIDParameter);
					if (parameter != null)
					{
						string text4 = parameter.AsString();
						if (text4 != string.Empty)
						{
							list.Add(text4);
						}
					}
				}
				if (list.Count > 0)
				{
					RemoveAnnotationText(list);
				}
			}
			RevitUtility.AnnotateGradeAndElevationOnLegList(m_Document, m_TransactionContainer, m_FamilyInstanceComponentWrapper, _bUseOriginalLeg: false, ref m_AnnotationElevationTextSet, ref m_AnnotationGradeTextSet);
			if (m_FamilyInstanceComponentWrapper.ParkingLotTopographySurface != null && m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList != null && m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList.Count > 0)
			{
				m_FamilyInstanceComponentWrapper.CreateParkingLotBreaklineLegs(m_FamilyInstanceComponentWrapper.ParkingLotTopographySurface);
				m_Document.ActiveView.UnhideElements(ElementSetConverter.ToIDList(m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementSet));
			}
		}
		if (!flag)
		{
			string strText = EPShared.LocalRM.GetString("NotSiteComponentMessage_") + GetCurrentTypeString(m_CurrentFamilyType) + EPShared.LocalRM.GetString("_HostLineString");
			RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return PickSiteAlignmentElement();
		}
		if (m_FamilyInstanceComponentWrapper != null && m_FamilyInstanceComponentWrapper.IntersectionFamilySymbol != null)
		{
			for (int j = 0; j < m_AllSiteComponentList.Count; j++)
			{
				m_AllSiteComponentList[j].IntersectionFamilySymbol = m_FamilyInstanceComponentWrapper.IntersectionFamilySymbol;
			}
		}
		if (m_FamilyInstanceComponentWrapper.ParkingLotTopographySurface != null)
		{
			for (int num = m_AllSiteComponentList.Count - 1; num >= 0; num--)
			{
				ElementSet familyInstanceElementSet2 = m_AllSiteComponentList[num].GetFamilyInstanceElementSet();
				if (familyInstanceElementSet2 != null && familyInstanceElementSet2.Size > 0)
				{
					{
						IEnumerator enumerator4 = familyInstanceElementSet2.GetEnumerator();
						try
						{
							if (enumerator4.MoveNext())
							{
								Element element4 = (Element)enumerator4.Current;
								parameter = element4.LookupParameter(RevitParameters.ParentSurfaceIDFamilyParameter);
								if (parameter != null && parameter.AsString() != string.Empty && string.Compare(m_strParentTopoSurfaceID = parameter.AsString(), m_FamilyInstanceComponentWrapper.ParkingLotTopographySurface.TopographySurface.UniqueId, ignoreCase: true) == 0)
								{
									if (m_ParkingLotCurbSiteCompList == null)
									{
										m_ParkingLotCurbSiteCompList = new List<FamilyInstanceComponentWrapper>();
									}
									m_ParkingLotCurbSiteCompList.Insert(0, m_AllSiteComponentList[num]);
									m_AllSiteComponentList.RemoveAt(num);
									m_ParkingLotCurbSiteCompList[0].CreateProposedLeg(m_FamilyInstanceComponentWrapper.ParkingLotTopographySurface);
									if (m_ParkingLotCurbSiteCompList[0].ProposedLeg != null)
									{
										m_ParkingLotCurbSiteCompList[0].CreateNewProposedElementSet(m_Document, m_TransactionContainer);
									}
									bool _bFamilySymbolPresent2 = false;
									m_ParkingLotCurbSiteCompList[0].ProposedFamilySymbol = SetInitialProposedFamilySymbol(m_ParkingLotCurbSiteCompList[0], ref _bFamilySymbolPresent2);
								}
							}
						}
						finally
						{
							IDisposable disposable = enumerator4 as IDisposable;
							if (disposable != null)
							{
								disposable.Dispose();
							}
						}
					}
				}
			}
		}
		return true;
	}

	private string GetCurrentTypeString(LandcaddID _Id)
	{
		string result = string.Empty;
		switch (_Id)
		{
		case LandcaddID.SoftTerrain:
			result = EPShared.LocalRM.GetString("SoftTerrainString");
			break;
		case LandcaddID.Curb:
			result = EPShared.LocalRM.GetString("CurbString");
			break;
		case LandcaddID.ParkingLot:
			result = EPShared.LocalRM.GetString("ParkingLotString");
			break;
		case LandcaddID.RetainingWall:
			result = EPShared.LocalRM.GetString("RetainingWallString");
			break;
		case LandcaddID.Sidewalk:
			result = EPShared.LocalRM.GetString("SidewalkString");
			break;
		case LandcaddID.Street:
			result = EPShared.LocalRM.GetString("StreetString");
			break;
		case LandcaddID.FeatureLine:
			result = EPShared.LocalRM.GetString("FeatureLineString");
			break;
		case LandcaddID.PointWipeout:
			result = EPShared.LocalRM.GetString("PointWipeoutString");
			break;
		}
		return result;
	}

	private void RemoveAnnotationText(List<string> _AnnotationListID)
	{
		List<Element> list = null;
		ElementSet elementSet = new ElementSet();
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(TextNote)).OfCategory(BuiltInCategory.OST_TextNotes);
		FilteredElementCollector source = filteredElementCollector;
		list = source.ToList();
		foreach (Element item in list)
		{
			foreach (string item2 in _AnnotationListID)
			{
				if (string.Compare(item.UniqueId, item2, ignoreCase: false) == 0)
				{
					elementSet.Insert(item);
					break;
				}
			}
		}
		m_TransactionContainer.BeginTransaction();
		if (!elementSet.IsEmpty)
		{
			m_Document.Delete(ElementSetConverter.ToIDList(elementSet));
		}
		m_TransactionContainer.EndTransaction();
	}

	private bool MoveSiteAlignmentElement()
	{
		if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
		{
			return false;
		}
		string strPrompt = EPShared.LocalRM.GetString("PickStartPointPrompt");
		bool result = false;
		ElementSet elementSet = new ElementSet();
		Eps_Point _Point = null;
		Eps_Point eps_Point = null;
		Eps_Line eps_Line = null;
		Eps_Leg eps_Leg = null;
		ModelCurve modelCurve = null;
		bool flag = true;
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		while (flag)
		{
			flag = RevitUtility.GetPointFromProject(m_Document, strPrompt, ref _Point, _bDisplayError: false);
			if (flag)
			{
				if (eps_Point != null)
				{
					EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Point, _Point, ref _dDistance);
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point, _Point, ref _dDirection);
					result = true;
					m_TransactionContainer.BeginTransaction();
					m_Document.Delete(ElementSetConverter.ToIDList(m_FamilyInstanceComponentWrapper.ProposedElementSet));
					if (elementSet.Size > 0)
					{
						m_Document.Delete(ElementSetConverter.ToIDList(elementSet));
						elementSet.Clear();
					}
					m_FamilyInstanceComponentWrapper.ProposedLeg = m_FamilyInstanceComponentWrapper.ProposedLeg.MoveLeg(_dDirection, _dDistance);
					m_FamilyInstanceComponentWrapper.CreateNewProposedElementSet(m_Document);
					if (m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList != null && m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList.Count > 0)
					{
						m_Document.Delete(ElementSetConverter.ToIDList(m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementSet));
						m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList.Clear();
						for (int i = 0; i < m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList.Count; i++)
						{
							eps_Leg = m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList[i];
							eps_Leg = eps_Leg.MoveLeg(_dDirection, _dDistance);
							m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList[i] = eps_Leg;
							ModelCurveArray modelCurveArray = RevitUtility.NewModelCurveArray(m_Document, eps_Leg);
							foreach (ModelCurve item in modelCurveArray)
							{
								if (m_MassLineSubCategory != null)
								{
									item.LineStyle = m_MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
								}
								m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList.Add(item);
							}
						}
					}
					if (m_ParkingLotCurbSiteCompList != null)
					{
						for (int j = 0; j < m_ParkingLotCurbSiteCompList.Count; j++)
						{
							m_Document.Delete(ElementSetConverter.ToIDList(m_ParkingLotCurbSiteCompList[j].ProposedElementSet));
							m_ParkingLotCurbSiteCompList[j].ProposedLeg = m_ParkingLotCurbSiteCompList[j].ProposedLeg.MoveLeg(_dDirection, _dDistance);
							m_ParkingLotCurbSiteCompList[j].CreateNewProposedElementSet(m_Document);
						}
					}
					m_TransactionContainer.EndTransaction();
					if (m_AnnotationElevationTextSet != null)
					{
						m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationElevationTextSet));
						m_AnnotationElevationTextSet = null;
					}
					if (m_AnnotationGradeTextSet != null)
					{
						m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationGradeTextSet));
						m_AnnotationGradeTextSet = null;
					}
					RevitUtility.AnnotateGradeAndElevationOnLegList(m_Document, m_TransactionContainer, m_FamilyInstanceComponentWrapper, _bUseOriginalLeg: false, ref m_AnnotationElevationTextSet, ref m_AnnotationGradeTextSet);
					strPrompt = EPShared.LocalRM.GetString("PickStartPointPrompt");
					eps_Point = null;
				}
				else
				{
					m_TransactionContainer.BeginTransaction();
					eps_Line = new Eps_Line();
					eps_Line.StartPnt = new Eps_Point(_Point.x + m_dMarkerHeight / 2.0, _Point.y, 0.0);
					eps_Line.EndPnt = new Eps_Point(_Point.x - m_dMarkerHeight / 2.0, _Point.y, 0.0);
					modelCurve = RevitUtility.NewModelCurve(m_Document, eps_Line);
					elementSet.Insert(modelCurve);
					eps_Line.StartPnt = new Eps_Point(_Point.x, _Point.y + m_dMarkerHeight / 2.0, 0.0);
					eps_Line.EndPnt = new Eps_Point(_Point.x, _Point.y - m_dMarkerHeight / 2.0, 0.0);
					modelCurve = RevitUtility.NewModelCurve(m_Document, eps_Line);
					elementSet.Insert(modelCurve);
					m_TransactionContainer.EndTransaction();
					strPrompt = EPShared.LocalRM.GetString("PickEndPointPrompt");
					eps_Point = _Point;
				}
			}
			m_Document.Regenerate();
		}
		if (elementSet.Size > 0)
		{
			m_TransactionContainer.BeginTransaction();
			m_Document.Delete(ElementSetConverter.ToIDList(elementSet));
			m_TransactionContainer.EndTransaction();
		}
		return result;
	}

	private bool RotateSiteAlignmentElement()
	{
		if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
		{
			return false;
		}
		string strPrompt = EPShared.LocalRM.GetString("PickRotationPointPrompt");
		bool result = false;
		ElementSet elementSet = new ElementSet();
		Eps_Point _Point = null;
		Eps_Point eps_Point = null;
		Eps_Point eps_Point2 = null;
		ModelCurve modelCurve = null;
		double _dDirection = 0.0;
		double _dDirection2 = 0.0;
		Eps_Line eps_Line = null;
		Eps_Leg eps_Leg = null;
		bool flag = true;
		while (flag)
		{
			flag = RevitUtility.GetPointFromProject(m_Document, strPrompt, ref _Point, _bDisplayError: false);
			if (flag)
			{
				if (eps_Point2 != null)
				{
					result = true;
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point, eps_Point2, ref _dDirection);
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point, _Point, ref _dDirection2);
					m_TransactionContainer.BeginTransaction();
					m_Document.Delete(ElementSetConverter.ToIDList(m_FamilyInstanceComponentWrapper.ProposedElementSet));
					if (elementSet.Size > 0)
					{
						m_Document.Delete(ElementSetConverter.ToIDList(elementSet));
						elementSet.Clear();
					}
					m_FamilyInstanceComponentWrapper.ProposedLeg = m_FamilyInstanceComponentWrapper.ProposedLeg.RotateLeg(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection2 - _dDirection));
					m_FamilyInstanceComponentWrapper.CreateNewProposedElementSet(m_Document);
					if (m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList != null && m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList.Count > 0)
					{
						m_Document.Delete(ElementSetConverter.ToIDList(m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementSet));
						m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList.Clear();
						for (int i = 0; i < m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList.Count; i++)
						{
							eps_Leg = m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList[i];
							eps_Leg = eps_Leg.RotateLeg(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection2 - _dDirection));
							m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList[i] = eps_Leg;
							ModelCurveArray modelCurveArray = RevitUtility.NewModelCurveArray(m_Document, eps_Leg);
							foreach (ModelCurve item in modelCurveArray)
							{
								if (m_MassLineSubCategory != null)
								{
									item.LineStyle = m_MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
								}
								m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList.Add(item);
							}
						}
					}
					if (m_ParkingLotCurbSiteCompList != null)
					{
						for (int j = 0; j < m_ParkingLotCurbSiteCompList.Count; j++)
						{
							m_Document.Delete(ElementSetConverter.ToIDList(m_ParkingLotCurbSiteCompList[j].ProposedElementSet));
							m_ParkingLotCurbSiteCompList[j].ProposedLeg = m_ParkingLotCurbSiteCompList[j].ProposedLeg.RotateLeg(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection2 - _dDirection));
							m_ParkingLotCurbSiteCompList[j].CreateNewProposedElementSet(m_Document);
						}
					}
					m_TransactionContainer.EndTransaction();
					if (m_AnnotationElevationTextSet != null)
					{
						m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationElevationTextSet));
						m_AnnotationElevationTextSet = null;
					}
					if (m_AnnotationGradeTextSet != null)
					{
						m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationGradeTextSet));
						m_AnnotationGradeTextSet = null;
					}
					RevitUtility.AnnotateGradeAndElevationOnLegList(m_Document, m_TransactionContainer, m_FamilyInstanceComponentWrapper, _bUseOriginalLeg: false, ref m_AnnotationElevationTextSet, ref m_AnnotationGradeTextSet);
					strPrompt = EPShared.LocalRM.GetString("PickRotationPointPrompt");
					eps_Point = null;
					eps_Point2 = null;
				}
				else if (eps_Point != null)
				{
					m_TransactionContainer.BeginTransaction();
					eps_Line = new Eps_Line();
					eps_Line.StartPnt = new Eps_Point(eps_Point);
					eps_Line.EndPnt = new Eps_Point(_Point);
					modelCurve = RevitUtility.NewModelCurve(m_Document, eps_Line);
					elementSet.Insert(modelCurve);
					m_TransactionContainer.EndTransaction();
					strPrompt = EPShared.LocalRM.GetString("PickSecondPointForRotationPrompt");
					eps_Point2 = _Point;
				}
				else
				{
					m_TransactionContainer.BeginTransaction();
					eps_Line = new Eps_Line();
					eps_Line.StartPnt = new Eps_Point(_Point.x + m_dMarkerHeight / 2.0, _Point.y, 0.0);
					eps_Line.EndPnt = new Eps_Point(_Point.x - m_dMarkerHeight / 2.0, _Point.y, 0.0);
					modelCurve = RevitUtility.NewModelCurve(m_Document, eps_Line);
					elementSet.Insert(modelCurve);
					eps_Line.StartPnt = new Eps_Point(_Point.x, _Point.y + m_dMarkerHeight / 2.0, 0.0);
					eps_Line.EndPnt = new Eps_Point(_Point.x, _Point.y - m_dMarkerHeight / 2.0, 0.0);
					modelCurve = RevitUtility.NewModelCurve(m_Document, eps_Line);
					elementSet.Insert(modelCurve);
					m_TransactionContainer.EndTransaction();
					strPrompt = EPShared.LocalRM.GetString("PickFirstPointForRotationPrompt");
					eps_Point = _Point;
				}
			}
			m_Document.Regenerate();
		}
		if (elementSet.Size > 0)
		{
			m_TransactionContainer.BeginTransaction();
			m_Document.Delete(ElementSetConverter.ToIDList(elementSet));
			m_TransactionContainer.EndTransaction();
		}
		return result;
	}

	private bool ChangeGradeAndElevationSiteAlignmentElement()
	{
		if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
		{
			return false;
		}
		int _nReturnValue = 0;
		bool flag = false;
		if (ApplicationUtility.Settings.eplSetE_GetSetting(m_Document.PathName, "UseSharedElevation", ref _nReturnValue))
		{
			flag = Convert.ToBoolean(_nReturnValue);
		}
		double num = 0.0;
		if (flag)
		{
			ProjectLocation activeProjectLocation = m_Document.ActiveProjectLocation;
			XYZ point = new XYZ();
			ProjectPosition projectPosition = activeProjectLocation.GetProjectPosition(point);
			num = projectPosition.Elevation;
		}
		List<Eps_Point> _GradeAndElevationPnts = new List<Eps_Point>();
		TopographySurfaceManagement.FindAllGradeAndElevationPointsInIntersection(m_Document, m_ProposedSurface, m_TransactionContainer, m_bFindIntersectionStations, m_FamilyInstanceComponentWrapper, m_AllSiteComponentList, ref _GradeAndElevationPnts);
		string strPrompt = EPShared.LocalRM.GetString("PickElevationGradePointPrompt");
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		bool result = false;
		Eps_Point _PickedPoint = null;
		bool flag2 = true;
		bool flag3 = false;
		double num2 = 0.0;
		bool flag4 = false;
		bool _bPickFirstLeg = false;
		while (flag2)
		{
			flag4 = false;
			flag3 = false;
			eps_Leg2 = null;
			eps_Leg3 = null;
			flag2 = RevitUtility.BreakApartLeg(m_Document, m_TransactionContainer, m_dMarkerHeight, m_FamilyInstanceComponentWrapper.ProposedLeg, ref eps_Leg2, ref eps_Leg3, ref _bPickFirstLeg, ref flag3, ref _PickedPoint, strPrompt, _GradeAndElevationPnts);
			m_Document.Regenerate();
			if (!flag2)
			{
				continue;
			}
			if (!flag3)
			{
				ChangeValueForm changeValueForm = new ChangeValueForm(m_Document, EPShared.LocalRM.GetString("ChangeElevationDialog"), EPShared.LocalRM.GetString("CurrentElevationDialog"), EPShared.LocalRM.GetString("AMP_NewElevationDialog"), _PickedPoint.z + num, UnitType.UT_Length, "HID_SDchangeElevation");
				changeValueForm.ShowDialog();
				if (changeValueForm.DialogResult == DialogResult.OK)
				{
					num2 = changeValueForm.NewUnitFieldValue - num;
					m_FamilyInstanceComponentWrapper.ProposedLeg = null;
					result = true;
					if (eps_Leg2 != null)
					{
						flag4 = EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg2.LegStartPnt, eps_Leg2.LegEndPnt);
						if (eps_Leg2.Last.Line != null)
						{
							eps_Leg2.Last.Line.EndPnt.z = num2;
						}
						else if (eps_Leg2.Last.Arc != null)
						{
							eps_Leg2.Last.Arc.EndPnt.z = num2;
							eps_Leg2.Last.Arc.RadiusPnt.z = (eps_Leg2.Last.Arc.StartPnt.z + eps_Leg2.Last.Arc.EndPnt.z) / 2.0;
						}
						if (eps_Leg3 == null && flag4)
						{
							if (eps_Leg2.Line != null)
							{
								eps_Leg2.Line.StartPnt.z = num2;
							}
							else if (eps_Leg2.Arc != null)
							{
								eps_Leg2.Arc.StartPnt.z = num2;
								eps_Leg2.Arc.RadiusPnt.z = (eps_Leg2.Arc.StartPnt.z + eps_Leg2.Arc.EndPnt.z) / 2.0;
							}
						}
						m_FamilyInstanceComponentWrapper.ProposedLeg = Eps_Leg.CopyLeg(eps_Leg2);
					}
					if (eps_Leg3 != null)
					{
						flag4 = EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg3.LegStartPnt, eps_Leg3.LegEndPnt);
						if (eps_Leg3.Line != null)
						{
							eps_Leg3.Line.StartPnt.z = num2;
						}
						else if (eps_Leg3.Arc != null)
						{
							eps_Leg3.Arc.StartPnt.z = num2;
							eps_Leg3.Arc.RadiusPnt.z = (eps_Leg3.Arc.StartPnt.z + eps_Leg3.Arc.EndPnt.z) / 2.0;
						}
						if (eps_Leg2 == null && flag4)
						{
							if (eps_Leg3.Last.Line != null)
							{
								eps_Leg3.Last.Line.EndPnt.z = num2;
							}
							else if (eps_Leg3.Last.Arc != null)
							{
								eps_Leg3.Last.Arc.EndPnt.z = num2;
								eps_Leg3.Last.Arc.RadiusPnt.z = (eps_Leg3.Last.Arc.StartPnt.z + eps_Leg3.Last.Arc.EndPnt.z) / 2.0;
							}
						}
						for (eps_Leg = eps_Leg3; eps_Leg != null; eps_Leg = eps_Leg.Next)
						{
							if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
							{
								m_FamilyInstanceComponentWrapper.ProposedLeg = Eps_Leg.SingleLeg(eps_Leg);
							}
							else
							{
								m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
							}
						}
					}
				}
			}
			else
			{
				double _dDistance = 0.0;
				double num3 = 0.0;
				double num4 = 0.0;
				if (_bPickFirstLeg)
				{
					if (eps_Leg2.Last.Line != null)
					{
						EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Leg2.Last.Line.StartPnt, eps_Leg3.Line.EndPnt, ref _dDistance);
						num3 = eps_Leg2.Last.Line.StartPnt.z - eps_Leg3.Line.EndPnt.z;
					}
					else if (eps_Leg2.Last.Arc != null)
					{
						_dDistance = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg2.Last.Arc.Radius, 2.0 * eps_Leg2.Last.Arc.Delta);
						num3 = eps_Leg2.Last.Arc.StartPnt.z - eps_Leg3.Arc.EndPnt.z;
					}
					num4 = num3 / _dDistance;
				}
				else
				{
					if (eps_Leg3.Line != null)
					{
						EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Leg2.Last.Line.StartPnt, eps_Leg3.Line.EndPnt, ref _dDistance);
						num3 = eps_Leg3.Line.EndPnt.z - eps_Leg2.Last.Line.StartPnt.z;
					}
					else if (eps_Leg3.Arc != null)
					{
						_dDistance = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg3.Arc.Radius, 2.0 * eps_Leg3.Arc.Delta);
						num3 = eps_Leg3.Arc.EndPnt.z - eps_Leg2.Last.Arc.StartPnt.z;
					}
					num4 = num3 / _dDistance;
				}
				ChangeValueForm changeValueForm2 = new ChangeValueForm(m_Document, EPShared.LocalRM.GetString("ChangeGradeDialog"), EPShared.LocalRM.GetString("CurrentGradeDialog"), EPShared.LocalRM.GetString("AMP_NewGradeDialog"), num4, UnitType.UT_Slope, "HID_SDchangeGrade");
				changeValueForm2.ShowDialog();
				if (changeValueForm2.DialogResult == DialogResult.OK)
				{
					num2 = changeValueForm2.NewUnitFieldValue;
					m_FamilyInstanceComponentWrapper.ProposedLeg = null;
					result = true;
					num3 = num2 * _dDistance;
					Eps_Point eps_Point = new Eps_Point();
					Eps_Point eps_Point2 = new Eps_Point();
					if (eps_Leg2 != null)
					{
						eps_Leg = eps_Leg2;
						while (eps_Leg.Next != null)
						{
							if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
							{
								m_FamilyInstanceComponentWrapper.ProposedLeg = Eps_Leg.SingleLeg(eps_Leg);
							}
							else
							{
								m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
							}
							eps_Leg = eps_Leg.Next;
						}
						if (eps_Leg.Line != null)
						{
							if (_bPickFirstLeg)
							{
								eps_Point.x = eps_Leg.Line.StartPnt.x;
								eps_Point.y = eps_Leg.Line.StartPnt.y;
								eps_Point.z = eps_Leg3.Line.EndPnt.z + num3;
							}
							else
							{
								eps_Point = new Eps_Point(eps_Leg.Line.StartPnt);
							}
						}
						else if (eps_Leg.Arc != null)
						{
							if (_bPickFirstLeg)
							{
								eps_Point.x = eps_Leg.Arc.StartPnt.x;
								eps_Point.y = eps_Leg.Arc.StartPnt.y;
								eps_Point.z = eps_Leg3.Arc.EndPnt.z + num3;
							}
							else
							{
								eps_Point = new Eps_Point(eps_Leg.Arc.StartPnt);
							}
						}
						if (_bPickFirstLeg && m_FamilyInstanceComponentWrapper.ProposedLeg != null && m_FamilyInstanceComponentWrapper.ProposedLeg.Last != null)
						{
							if (m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Line != null)
							{
								m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Line.EndPnt = eps_Point;
							}
							else if (m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Arc != null)
							{
								m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Arc.EndPnt = eps_Point;
								m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Arc.RadiusPnt.z = (m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Arc.StartPnt.z + m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Arc.EndPnt.z) / 2.0;
							}
						}
					}
					if (eps_Leg3 != null)
					{
						if (eps_Leg3.Line != null)
						{
							if (!_bPickFirstLeg)
							{
								eps_Point2.x = eps_Leg3.Line.EndPnt.x;
								eps_Point2.y = eps_Leg3.Line.EndPnt.y;
								eps_Point2.z = num3 + eps_Leg.Line.StartPnt.z;
							}
							else
							{
								eps_Point2 = new Eps_Point(eps_Leg3.Line.EndPnt);
							}
							eps_Leg = new Eps_Leg();
							eps_Leg.Line = new Eps_Line();
							eps_Leg.Line.StartPnt = eps_Point;
							eps_Leg.Line.EndPnt = eps_Point2;
						}
						else if (eps_Leg3.Arc != null)
						{
							if (!_bPickFirstLeg)
							{
								eps_Point2.x = eps_Leg3.Arc.EndPnt.x;
								eps_Point2.y = eps_Leg3.Arc.EndPnt.y;
								eps_Point2.z = num3 + eps_Leg.Arc.StartPnt.z;
							}
							else
							{
								eps_Point2 = new Eps_Point(eps_Leg3.Arc.EndPnt);
							}
							eps_Leg = new Eps_Leg();
							eps_Leg.Arc = new Eps_Arc(eps_Leg3.Arc);
							eps_Leg.Arc.Delta = eps_Leg.Arc.Delta * 2.0;
							eps_Leg.Arc.StartPnt = eps_Point;
							eps_Leg.Arc.EndPnt = eps_Point2;
							eps_Leg.Arc.RadiusPnt.z = (eps_Leg.Arc.StartPnt.z + eps_Leg.Arc.EndPnt.z) / 2.0;
						}
						if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
						{
							m_FamilyInstanceComponentWrapper.ProposedLeg = Eps_Leg.SingleLeg(eps_Leg);
						}
						else
						{
							m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
						}
						if (!_bPickFirstLeg && eps_Leg3.Next != null)
						{
							if (eps_Leg3.Next.Line != null)
							{
								eps_Leg3.Next.Line.StartPnt = eps_Point2;
							}
							else if (eps_Leg3.Next.Arc != null)
							{
								eps_Leg3.Next.Arc.StartPnt = eps_Point2;
								eps_Leg3.Next.Arc.RadiusPnt.z = (eps_Leg3.Next.Arc.StartPnt.z + eps_Leg3.Next.Arc.EndPnt.z) / 2.0;
							}
						}
						for (eps_Leg = eps_Leg3.Next; eps_Leg != null; eps_Leg = eps_Leg.Next)
						{
							if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
							{
								m_FamilyInstanceComponentWrapper.ProposedLeg = Eps_Leg.SingleLeg(eps_Leg);
							}
							else
							{
								m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
							}
						}
					}
					if (m_FamilyInstanceComponentWrapper.ProposedLeg != null && EPShared.eplGeoE_ArePntsDuplicates2D(m_FamilyInstanceComponentWrapper.ProposedLeg.LegStartPnt, m_FamilyInstanceComponentWrapper.ProposedLeg.LegEndPnt) && !EPShared.eplGeoE_ArePntsDuplicates3D(m_FamilyInstanceComponentWrapper.ProposedLeg.LegStartPnt, m_FamilyInstanceComponentWrapper.ProposedLeg.LegEndPnt))
					{
						if (_bPickFirstLeg)
						{
							if (m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Line != null)
							{
								m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Line.EndPnt.z = m_FamilyInstanceComponentWrapper.ProposedLeg.LegStartPnt.z;
							}
							else if (m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Arc != null)
							{
								m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Arc.EndPnt.z = m_FamilyInstanceComponentWrapper.ProposedLeg.LegStartPnt.z;
								m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Arc.RadiusPnt.z = (m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Arc.StartPnt.z + m_FamilyInstanceComponentWrapper.ProposedLeg.Last.Arc.EndPnt.z) / 2.0;
							}
						}
						else if (m_FamilyInstanceComponentWrapper.ProposedLeg.Line != null)
						{
							m_FamilyInstanceComponentWrapper.ProposedLeg.Line.StartPnt.z = m_FamilyInstanceComponentWrapper.ProposedLeg.LegEndPnt.z;
						}
						else if (m_FamilyInstanceComponentWrapper.ProposedLeg.Arc != null)
						{
							m_FamilyInstanceComponentWrapper.ProposedLeg.Arc.StartPnt.z = m_FamilyInstanceComponentWrapper.ProposedLeg.LegEndPnt.z;
							m_FamilyInstanceComponentWrapper.ProposedLeg.Arc.RadiusPnt.z = (m_FamilyInstanceComponentWrapper.ProposedLeg.Arc.StartPnt.z + m_FamilyInstanceComponentWrapper.ProposedLeg.Arc.EndPnt.z) / 2.0;
						}
					}
				}
			}
			if (m_AnnotationElevationTextSet != null)
			{
				ICollection<ElementId> collection = m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationElevationTextSet));
				m_AnnotationElevationTextSet = null;
			}
			if (m_AnnotationGradeTextSet != null)
			{
				m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationGradeTextSet));
				m_AnnotationGradeTextSet = null;
			}
			RevitUtility.AnnotateGradeAndElevationOnLegList(m_Document, m_TransactionContainer, m_FamilyInstanceComponentWrapper, _bUseOriginalLeg: false, ref m_AnnotationElevationTextSet, ref m_AnnotationGradeTextSet);
		}
		return result;
	}

	private bool AddVertexSiteAlignmentElement()
	{
		if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
		{
			return false;
		}
		string strPrompt = EPShared.LocalRM.GetString("PickAddVertexPointPrompt");
		bool result = false;
		Eps_Point _Point = null;
		Eps_Leg _NewLegList = null;
		bool flag = true;
		while (flag)
		{
			flag = RevitUtility.GetPointFromProject(m_Document, strPrompt, ref _Point, _bDisplayError: false);
			if (flag)
			{
				if (RevitGeometry.AddVertexToLegList(m_FamilyInstanceComponentWrapper.ProposedLeg, _Point, ref _NewLegList) == 1)
				{
					continue;
				}
				result = true;
				m_TransactionContainer.BeginTransaction();
				m_Document.Delete(ElementSetConverter.ToIDList(m_FamilyInstanceComponentWrapper.ProposedElementSet));
				m_FamilyInstanceComponentWrapper.ProposedLeg = Eps_Leg.CopyLeg(_NewLegList);
				m_FamilyInstanceComponentWrapper.CreateNewProposedElementSet(m_Document);
				if (m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList != null && m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList.Count > 0)
				{
					ElementSet elementSet = new ElementSet();
					List<Eps_Point> list = new List<Eps_Point>();
					int num = 0;
					for (int num2 = m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList.Count - 1; num2 >= 0; num2--)
					{
						Eps_Leg eps_Leg = m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList[num2];
						list = eps_Leg.GetPointsAtIntervals(1.0, 1.0);
						num = list.Count;
						RevitGeometry.GetAllPointsInBoundary(ref list, m_FamilyInstanceComponentWrapper.ProposedLeg, null);
						if (list.Count < num)
						{
							ElementSet elementSet2 = m_FamilyInstanceComponentWrapper.RemoveBreaklineAndElementsFromList(num2);
							if (elementSet2 != null)
							{
								foreach (Element item in elementSet2)
								{
									elementSet.Insert(item);
								}
							}
						}
						list.Clear();
					}
					if (elementSet.Size > 0)
					{
						RevitMessageBox.Show(EPShared.LocalRM.GetString("BreaklinesNotInParkingLotBoundaryMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
						m_Document.Delete(ElementSetConverter.ToIDList(elementSet));
					}
				}
				m_TransactionContainer.EndTransaction();
				if (m_AnnotationElevationTextSet != null)
				{
					m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationElevationTextSet));
					m_AnnotationElevationTextSet = null;
				}
				if (m_AnnotationGradeTextSet != null)
				{
					m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationGradeTextSet));
					m_AnnotationGradeTextSet = null;
				}
				RevitUtility.AnnotateGradeAndElevationOnLegList(m_Document, m_TransactionContainer, m_FamilyInstanceComponentWrapper, _bUseOriginalLeg: false, ref m_AnnotationElevationTextSet, ref m_AnnotationGradeTextSet);
				_NewLegList = null;
			}
			m_Document.Regenerate();
		}
		return result;
	}

	private bool DeleteVertexSiteAlignmentElement()
	{
		if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
		{
			return false;
		}
		string strPrompt = EPShared.LocalRM.GetString("PickDeleteVertexPointPrompt");
		bool result = false;
		bool flag = false;
		Eps_Leg _ReturnLeg = null;
		bool flag2 = true;
		while (flag2)
		{
			flag2 = RevitUtility.RemoveLegFromLegList(m_Document, m_TransactionContainer, m_dMarkerHeight, m_FamilyInstanceComponentWrapper.ProposedLeg, ref _ReturnLeg, _bDisplayError: false, strPrompt);
			if (flag2)
			{
				if (_ReturnLeg != null && _ReturnLeg.LegCount > 0)
				{
					result = true;
					m_TransactionContainer.BeginTransaction();
					m_Document.Delete(ElementSetConverter.ToIDList(m_FamilyInstanceComponentWrapper.ProposedElementSet));
					m_FamilyInstanceComponentWrapper.ProposedLeg = Eps_Leg.CopyLeg(_ReturnLeg);
					m_FamilyInstanceComponentWrapper.CreateNewProposedElementSet(m_Document);
					if (m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList != null && m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList.Count > 0)
					{
						ElementSet elementSet = new ElementSet();
						List<Eps_Point> list = new List<Eps_Point>();
						int num = 0;
						for (int num2 = m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList.Count - 1; num2 >= 0; num2--)
						{
							Eps_Leg eps_Leg = m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList[num2];
							list = eps_Leg.GetPointsAtIntervals(1.0, 1.0);
							num = list.Count;
							RevitGeometry.GetAllPointsInBoundary(ref list, m_FamilyInstanceComponentWrapper.ProposedLeg, null);
							if (list.Count < num)
							{
								ElementSet elementSet2 = m_FamilyInstanceComponentWrapper.RemoveBreaklineAndElementsFromList(num2);
								if (elementSet2 != null)
								{
									foreach (Element item in elementSet2)
									{
										elementSet.Insert(item);
									}
								}
							}
							list.Clear();
						}
						if (elementSet.Size > 0)
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("BreaklinesNotInParkingLotBoundaryMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
							m_Document.Delete(ElementSetConverter.ToIDList(elementSet));
						}
					}
					m_TransactionContainer.EndTransaction();
					if (m_AnnotationElevationTextSet != null)
					{
						m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationElevationTextSet));
						m_AnnotationElevationTextSet = null;
					}
					if (m_AnnotationGradeTextSet != null)
					{
						m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationGradeTextSet));
						m_AnnotationGradeTextSet = null;
					}
					RevitUtility.AnnotateGradeAndElevationOnLegList(m_Document, m_TransactionContainer, m_FamilyInstanceComponentWrapper, _bUseOriginalLeg: false, ref m_AnnotationElevationTextSet, ref m_AnnotationGradeTextSet);
					_ReturnLeg = null;
				}
				else if (DeleteSiteAlignmentElement())
				{
					result = true;
					flag2 = false;
				}
				m_Document.Regenerate();
			}
			m_Document.Regenerate();
		}
		return result;
	}

	private bool DeleteSiteAlignmentElement()
	{
		if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
		{
			return false;
		}
		bool result = false;
		DialogResult dialogResult = RevitMessageBox.Show(EPShared.LocalRM.GetString("DeleteSiteComponentLineMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
		if (dialogResult == DialogResult.Yes)
		{
			m_TransactionContainer.BeginTransaction();
			m_Document.Delete(ElementSetConverter.ToIDList(m_FamilyInstanceComponentWrapper.ProposedElementSet));
			m_TransactionContainer.EndTransaction();
			m_FamilyInstanceComponentWrapper.ProposedLeg = null;
			m_FamilyInstanceComponentWrapper.ProposedElementSet = null;
			if (m_AnnotationElevationTextSet != null)
			{
				m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationElevationTextSet));
				m_AnnotationElevationTextSet = null;
			}
			if (m_AnnotationGradeTextSet != null)
			{
				m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationGradeTextSet));
				m_AnnotationGradeTextSet = null;
			}
			result = true;
		}
		return result;
	}

	private bool ChangeTypeSiteAlignmentElement()
	{
		if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
		{
			return false;
		}
		FamilyInstance firstExistingFamilyInstance = m_FamilyInstanceComponentWrapper.GetFirstExistingFamilyInstance();
		if (firstExistingFamilyInstance == null)
		{
			return false;
		}
		bool result = false;
		Parameter parameter = null;
		if (m_CurrentFamilyType != LandcaddID.FeatureLine && m_CurrentFamilyType != LandcaddID.PointWipeout)
		{
			FamilySymbol proposedFamilySymbol = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol;
			string strCurrentTypeValue = string.Empty;
			string strCurrentFamilyValue = string.Empty;
			if (proposedFamilySymbol != null)
			{
				strCurrentTypeValue = proposedFamilySymbol.Name;
				if (proposedFamilySymbol.Family != null)
				{
					strCurrentFamilyValue = proposedFamilySymbol.Family.Name;
				}
			}
			ChangeFamilyTypeForm changeFamilyTypeForm = new ChangeFamilyTypeForm(m_Document, m_TransactionContainer, strCurrentFamilyValue, strCurrentTypeValue, m_CurrentFamilyType);
			changeFamilyTypeForm.ShowDialog();
			if (changeFamilyTypeForm.DialogResult == DialogResult.OK && changeFamilyTypeForm.NewTypeValue != null)
			{
				bool flag = false;
				if (m_CurrentFamilyType == LandcaddID.RetainingWall)
				{
					parameter = changeFamilyTypeForm.NewTypeValue.LookupParameter(RevitParameters.RetainingWallSlopeProjectionParameter);
					if (parameter != null)
					{
						flag = parameter.AsInteger() == 1;
					}
					if (flag)
					{
						bool flag2 = true;
						if (!RevitUtility.PickSideLeg(m_Document, m_FamilyInstanceComponentWrapper.ProposedLeg, ref m_bRetainingWallUseRightLeg, EPShared.LocalRM.GetString("PickSideForGradeToPlaceMessage")))
						{
							return false;
						}
					}
				}
				m_FamilyInstanceComponentWrapper.ProposedFamilySymbol = changeFamilyTypeForm.NewTypeValue;
				result = true;
			}
		}
		else
		{
			parameter = firstExistingFamilyInstance.LookupParameter(RevitParameters.LinearPointIntervalParameter);
			if (parameter != null)
			{
				ChangeValueForm changeValueForm = new ChangeValueForm(m_Document, EPShared.LocalRM.GetString("ChangeLinearIntervalDialog"), EPShared.LocalRM.GetString("CurrentLinearDialog"), EPShared.LocalRM.GetString("AMP_NewLinearDialog"), parameter.AsDouble(), UnitType.UT_Length, "HID_SDaaa");
				changeValueForm.ShowDialog();
				if (changeValueForm.DialogResult == DialogResult.OK)
				{
					ElementSet familyInstanceElementSet = m_FamilyInstanceComponentWrapper.GetFamilyInstanceElementSet();
					if (familyInstanceElementSet != null && familyInstanceElementSet.Size > 0)
					{
						foreach (Element item in familyInstanceElementSet)
						{
							item.LookupParameter(RevitParameters.LinearPointIntervalParameter)?.Set(changeValueForm.NewUnitFieldValue);
						}
						result = true;
					}
				}
			}
		}
		m_Document.Regenerate();
		return result;
	}

	private bool AddBreaklinesToSiteAlignmentElement()
	{
		if (m_FamilyInstanceComponentWrapper.ProposedLeg == null)
		{
			return false;
		}
		Eps_Leg _EvalLeg = null;
		List<Eps_Point> list = new List<Eps_Point>();
		string empty = string.Empty;
		bool result = false;
		DialogResult dialogResult = DialogResult.Retry;
		AddBreaklineForm addBreaklineForm = new AddBreaklineForm(m_Document);
		while (dialogResult == DialogResult.Retry)
		{
			addBreaklineForm.ShowDialog();
			dialogResult = addBreaklineForm.DialogResult;
			switch (dialogResult)
			{
			case DialogResult.Retry:
			{
				string strText = EPShared.LocalRM.GetString("PointNotInParkingLotBoundaryMessage");
				TopographySurfaceContainer topographySurfaceContainer = m_FamilyInstanceComponentWrapper.ParkingLotTopographySurface;
				if (topographySurfaceContainer == null)
				{
					strText = EPShared.LocalRM.GetString("PointNotInProposedBoundaryMessage");
					topographySurfaceContainer = m_ProposedSurface;
				}
				Eps_Point _Point2 = new Eps_Point();
				RevitUtility.GetPointFromProject(m_Document, EPShared.LocalRM.GetString("PickPointPrompt"), ref _Point2, _bDisplayError: false);
				double _dElevation = 0.0;
				if (topographySurfaceContainer.GetSurfaceContainingPoint(_Point2, ref _dElevation) != null)
				{
					addBreaklineForm.SetFocusedElevation(_dElevation);
				}
				else
				{
					RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				break;
			}
			case DialogResult.OK:
			{
				int num = 0;
				List<Element> list2 = new List<Element>();
				if (!addBreaklineForm.ExistingHostLineChecked)
				{
					empty = EPShared.LocalRM.GetString("PickStartPointPrompt");
					Eps_Leg eps_Leg = null;
					Eps_Leg eps_Leg2 = null;
					Eps_Leg eps_Leg3 = null;
					Eps_Point _Point = null;
					Eps_Point eps_Point = null;
					Eps_Line eps_Line = null;
					bool flag = true;
					while (flag)
					{
						flag = RevitUtility.GetPointFromProject(m_Document, empty, ref _Point, _bDisplayError: false);
						if (!flag)
						{
							continue;
						}
						if (eps_Point != null)
						{
							eps_Leg3 = new Eps_Leg();
							if (_EvalLeg == null)
							{
								eps_Leg3.Line = new Eps_Line();
								eps_Leg3.Line.StartPnt = eps_Point;
								eps_Leg3.Line.EndPnt = _Point;
								eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg3);
								_EvalLeg = Eps_Leg.SingleLeg(eps_Leg2);
							}
							else
							{
								eps_Line = new Eps_Line();
								eps_Line.StartPnt = eps_Point;
								eps_Line.EndPnt = _Point;
								double radius = addBreaklineForm.Radius;
								Eps_Arc _pArc = new Eps_Arc();
								if (RevitGeometry.SolveArcFromTangentLines(_EvalLeg.Last.Line, eps_Line, radius, ref _pArc, _bVerifyTangentLength: true) == 0)
								{
									eps_Leg3.Line = new Eps_Line();
									eps_Leg3.Line.StartPnt = new Eps_Point(_EvalLeg.Last.Line.StartPnt);
									eps_Leg3.Line.EndPnt = new Eps_Point(_pArc.StartPnt);
									eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg3);
									eps_Leg3 = new Eps_Leg();
									eps_Leg3.Arc = new Eps_Arc(_pArc);
									eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg3);
									eps_Leg3 = new Eps_Leg();
									eps_Leg3.Line = new Eps_Line();
									eps_Leg3.Line.StartPnt = new Eps_Point(_pArc.EndPnt);
									eps_Leg3.Line.EndPnt = _Point;
									eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg3);
									eps_Leg = Eps_Leg.CopyLeg(_EvalLeg);
									_EvalLeg = null;
									while (eps_Leg.Next != null)
									{
										if (_EvalLeg == null)
										{
											_EvalLeg = Eps_Leg.SingleLeg(eps_Leg);
										}
										else
										{
											_EvalLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
										}
										eps_Leg = eps_Leg.Next;
									}
									if (_EvalLeg == null)
									{
										_EvalLeg = Eps_Leg.CopyLeg(eps_Leg2);
									}
									else
									{
										_EvalLeg.Last.Next = Eps_Leg.CopyLeg(eps_Leg2);
									}
								}
								else
								{
									eps_Leg3.Line = eps_Line;
									eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg3);
									_EvalLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
								}
								if (EPShared.eplGeoE_ArePntsDuplicates2D(_EvalLeg.LegStartPnt, _EvalLeg.LegEndPnt))
								{
									flag = false;
								}
							}
							m_TransactionContainer.BeginTransaction();
							if (eps_Leg2.LegCount > 1 && list2.Count > 0)
							{
								m_Document.Delete(list2[list2.Count - 1].Id);
								list2.RemoveAt(list2.Count - 1);
							}
							ModelCurveArray modelCurveArray = RevitUtility.NewModelCurveArray(m_Document, eps_Leg2);
							foreach (ModelCurve item2 in modelCurveArray)
							{
								if (m_MassLineSubCategory != null)
								{
									item2.LineStyle = m_MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
								}
								list2.Add(item2);
							}
							m_Document.Regenerate();
							m_TransactionContainer.EndTransaction();
							if (addBreaklineForm.SegmentChecked)
							{
								flag = false;
							}
						}
						else
						{
							empty = EPShared.LocalRM.GetString("PickEndPointPrompt");
						}
						eps_Point = _Point;
					}
					if (_EvalLeg != null && _EvalLeg.IsLegClosed())
					{
						Eps_Arc _pArc2 = new Eps_Arc();
						double radius2 = addBreaklineForm.Radius;
						if (RevitGeometry.SolveArcFromTangentLines(_EvalLeg.Last.Line, _EvalLeg.Line, radius2, ref _pArc2, _bVerifyTangentLength: true) == 0)
						{
							eps_Leg3.Line = new Eps_Line();
							eps_Leg3.Line.StartPnt = new Eps_Point(_EvalLeg.Last.Line.StartPnt);
							eps_Leg3.Line.EndPnt = new Eps_Point(_pArc2.StartPnt);
							eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg3);
							eps_Leg3 = new Eps_Leg();
							eps_Leg3.Arc = new Eps_Arc(_pArc2);
							eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg3);
							eps_Leg = Eps_Leg.CopyLeg(_EvalLeg);
							_EvalLeg = null;
							while (eps_Leg.Next != null)
							{
								if (_EvalLeg == null)
								{
									eps_Leg3 = new Eps_Leg();
									eps_Leg3.Line = new Eps_Line();
									eps_Leg3.Line.StartPnt = new Eps_Point(_pArc2.EndPnt);
									eps_Leg3.Line.EndPnt = new Eps_Point(eps_Leg.Line.EndPnt);
									_EvalLeg = Eps_Leg.SingleLeg(eps_Leg3);
								}
								else
								{
									_EvalLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
								}
								eps_Leg = eps_Leg.Next;
							}
							if (_EvalLeg == null)
							{
								_EvalLeg = Eps_Leg.CopyLeg(eps_Leg2);
							}
							else
							{
								_EvalLeg.Last.Next = Eps_Leg.CopyLeg(eps_Leg2);
							}
							foreach (ModelCurve item3 in list2)
							{
								m_Document.Delete(item3.Id);
							}
							list2.Clear();
							ModelCurveArray modelCurveArray2 = RevitUtility.NewModelCurveArray(m_Document, _EvalLeg, m_Document.ActiveView, _bUse2011TempLines: false);
							foreach (ModelCurve item4 in modelCurveArray2)
							{
								if (m_MassLineSubCategory != null)
								{
									item4.LineStyle = m_MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
								}
								list2.Add(item4);
							}
						}
					}
					list = _EvalLeg.GetPointsAtIntervals(1.0, 1.0);
					num = list.Count;
					RevitGeometry.GetAllPointsInBoundary(ref list, m_FamilyInstanceComponentWrapper.ProposedLeg, null);
					if (list.Count < num)
					{
						RevitMessageBox.Show(EPShared.LocalRM.GetString("PointsNotInParkingLotBoundaryMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
						foreach (ModelCurve item5 in list2)
						{
							m_Document.Delete(item5.Id);
						}
						return false;
					}
				}
				else
				{
					if (m_LineElementSet != null && m_LineElementSet.Size > 0)
					{
						m_TransactionContainer.BeginTransaction();
						m_Document.ActiveView.UnhideElements(ElementSetConverter.ToIDList(m_LineElementSet));
						m_TransactionContainer.EndTransaction();
					}
					m_Document.Regenerate();
					ElementSet elementSet = new ElementSet();
					ElementSet _Elements = null;
					empty = EPShared.LocalRM.GetString("PickObjectMessage");
					if (!RevitUtility.GetElementsFromProject(m_Document, empty, ref _Elements))
					{
						return false;
					}
					foreach (Element item6 in _Elements)
					{
						if (!(item6.Location is LocationCurve { Curve: var curve }))
						{
							continue;
						}
						if (curve != null)
						{
							if (addBreaklineForm.ChainChecked)
							{
								_EvalLeg = RevitGeometry.GetLegFromElementList(null, item6);
							}
							if (_EvalLeg != null)
							{
								list = _EvalLeg.GetPointsAtIntervals(1.0, 1.0);
								num = list.Count;
								RevitGeometry.GetAllPointsInBoundary(ref list, m_FamilyInstanceComponentWrapper.ProposedLeg, null);
								if (list.Count < num)
								{
									RevitMessageBox.Show(EPShared.LocalRM.GetString("PointsNotInParkingLotBoundaryMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
									return false;
								}
								if (!addBreaklineForm.RelativeElevationChecked && !RevitDefines.EQ(addBreaklineForm.GetGrade(RevitGeometry.eplGeoE_CalcLegListLength(_EvalLeg)), 0.0) && !RevitUtility.EvaluateLeg(m_Document, m_TransactionContainer, 20.0, ref _EvalLeg))
								{
									return false;
								}
								elementSet = _EvalLeg.ElementSet;
								ModelCurveArray modelCurveArray3 = RevitUtility.NewModelCurveArray(m_Document, _EvalLeg, m_Document.ActiveView, _bUse2011TempLines: false);
								foreach (ModelCurve item7 in modelCurveArray3)
								{
									if (m_MassLineSubCategory != null)
									{
										item7.LineStyle = m_MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
									}
									list2.Add(item7);
								}
							}
							else if (curve is Arc)
							{
								Arc arc = (Arc)curve;
								Eps_Arc _EvalArc = new Eps_Arc(arc);
								double dLegListLength = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_EvalArc.Radius, _EvalArc.Delta);
								if (!addBreaklineForm.RelativeElevationChecked && !RevitDefines.EQ(addBreaklineForm.GetGrade(dLegListLength), 0.0) && !RevitUtility.EvaluateArc(m_Document, m_TransactionContainer, 20.0, ref _EvalArc))
								{
									return false;
								}
								_EvalLeg = new Eps_Leg();
								_EvalLeg.Arc = new Eps_Arc(_EvalArc);
								list = _EvalLeg.GetPointsAtIntervals(1.0, 1.0);
								num = list.Count;
								RevitGeometry.GetAllPointsInBoundary(ref list, m_FamilyInstanceComponentWrapper.ProposedLeg, null);
								if (list.Count < num)
								{
									RevitMessageBox.Show(EPShared.LocalRM.GetString("PointsNotInParkingLotBoundaryMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
									return false;
								}
								elementSet.Insert(item6);
								ModelCurve modelCurve6 = RevitUtility.NewModelCurve(m_Document, _EvalArc);
								if (m_MassLineSubCategory != null)
								{
									modelCurve6.LineStyle = m_MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
								}
								list2.Add(modelCurve6);
							}
							else
							{
								if (!(curve is Line))
								{
									RevitMessageBox.Show(EPShared.LocalRM.GetString("ObjectSelectedNotSupportedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
									return false;
								}
								Line line = (Line)curve;
								Eps_Line _EvalLine = new Eps_Line(line);
								double _dDistance = 0.0;
								EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_EvalLine.EndPnt, _EvalLine.StartPnt, ref _dDistance);
								if (!addBreaklineForm.RelativeElevationChecked && !RevitDefines.EQ(addBreaklineForm.GetGrade(_dDistance), 0.0) && !RevitUtility.EvaluateLine(m_Document, m_TransactionContainer, 20.0, ref _EvalLine))
								{
									return false;
								}
								_EvalLeg = new Eps_Leg();
								_EvalLeg.Line = new Eps_Line(_EvalLine);
								list = _EvalLeg.GetPointsAtIntervals(1.0, 1.0);
								num = list.Count;
								RevitGeometry.GetAllPointsInBoundary(ref list, m_FamilyInstanceComponentWrapper.ProposedLeg, null);
								if (list.Count < num)
								{
									RevitMessageBox.Show(EPShared.LocalRM.GetString("PointsNotInParkingLotBoundaryMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
									return false;
								}
								elementSet.Insert(item6);
								ModelCurve modelCurve7 = RevitUtility.NewModelCurve(m_Document, _EvalLine);
								if (m_MassLineSubCategory != null)
								{
									modelCurve7.LineStyle = m_MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
								}
								list2.Add(modelCurve7);
							}
							list = _EvalLeg.GetPointsAtIntervals(1.0, 1.0);
							num = list.Count;
							RevitGeometry.GetAllPointsInBoundary(ref list, m_FamilyInstanceComponentWrapper.ProposedLeg, null);
							if (list.Count < num)
							{
								RevitMessageBox.Show(EPShared.LocalRM.GetString("PointsNotInParkingLotBoundaryMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
								return false;
							}
							if (elementSet.Size <= 0)
							{
								continue;
							}
							foreach (Element item8 in elementSet)
							{
								if (m_LineElementSet != null && m_LineElementSet.Size > 0 && m_LineElementSet.Contains(item8))
								{
									m_LineElementSet.Erase(item8);
								}
							}
							m_TransactionContainer.BeginTransaction();
							m_Document.Delete(ElementSetConverter.ToIDList(elementSet));
							m_TransactionContainer.EndTransaction();
							continue;
						}
						RevitMessageBox.Show(EPShared.LocalRM.GetString("ObjectSelectedNotSupportedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						return false;
					}
				}
				if (_EvalLeg == null)
				{
					break;
				}
				_EvalLeg = ((!addBreaklineForm.RelativeElevationChecked) ? _EvalLeg.ApplyElevationsToLeg(addBreaklineForm.BeginningElevation, addBreaklineForm.GetGrade(RevitGeometry.eplGeoE_CalcLegListLength(_EvalLeg))) : _EvalLeg.ApplyElevationsToLeg(m_FamilyInstanceComponentWrapper.ParkingLotTopographySurface, _bUseToposurfaceElevation: true, _bUseConstantElevation: false, addBreaklineForm.BeginningElevation));
				m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList.Add(_EvalLeg);
				if (list2.Count > 0)
				{
					m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList.AddRange(list2);
					for (int i = 0; i < list2.Count; i++)
					{
						m_FamilyInstanceComponentWrapper.ParkingLotBreaklineLinearPointIntervals.Add(addBreaklineForm.LinearInterval);
					}
				}
				result = true;
				break;
			}
			}
		}
		return result;
	}

	private bool DeleteBreaklinesFromSiteAlignmentElement()
	{
		if (m_FamilyInstanceComponentWrapper.ProposedLeg == null || m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementSet == null || m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementSet.Size == 0)
		{
			return false;
		}
		bool result = false;
		ElementSet _Elements = null;
		string strPrompt = EPShared.LocalRM.GetString("PickObjectMessage");
		if (!RevitUtility.GetElementsFromProject(m_Document, strPrompt, ref _Elements))
		{
			return false;
		}
		foreach (Element item in _Elements)
		{
			foreach (Element parkingLotBreaklineElement in m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList)
			{
				if (string.Compare(item.UniqueId, parkingLotBreaklineElement.UniqueId, ignoreCase: true) != 0)
				{
					continue;
				}
				int num = m_FamilyInstanceComponentWrapper.FindBreaklineIndexFromElementId(parkingLotBreaklineElement);
				if (num >= 0)
				{
					result = true;
					ElementSet elementSet = m_FamilyInstanceComponentWrapper.RemoveBreaklineAndElementsFromList(num);
					if (elementSet != null && elementSet.Size > 0)
					{
						m_TransactionContainer.BeginTransaction();
						m_Document.Delete(ElementSetConverter.ToIDList(elementSet));
						m_TransactionContainer.EndTransaction();
					}
				}
				break;
			}
		}
		m_Document.Regenerate();
		return result;
	}

	private bool ChangeViewScale()
	{
		bool result = false;
		ChangeValueForm changeValueForm = new ChangeValueForm(m_Document, EPShared.LocalRM.GetString("ChangeViewScaleDialog"), EPShared.LocalRM.GetString("CurrentViewScaleDialog"), EPShared.LocalRM.GetString("AMP_NewViewScaleDialog"), m_Document.ActiveView.Scale, _bIsRatioValue: true, "HID_SDchangeViewScale");
		changeValueForm.ShowDialog();
		if (changeValueForm.DialogResult == DialogResult.OK && m_Document.ActiveView.Scale != changeValueForm.NewIntegerFieldValue)
		{
			result = true;
			m_Document.ActiveView.Scale = changeValueForm.NewIntegerFieldValue;
			if (m_AnnotationElevationTextSet != null)
			{
				m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationElevationTextSet));
				m_AnnotationElevationTextSet = null;
			}
			if (m_AnnotationGradeTextSet != null)
			{
				m_Document.Delete(ElementSetConverter.ToIDList(m_AnnotationGradeTextSet));
				m_AnnotationGradeTextSet = null;
			}
			RevitUtility.AnnotateGradeAndElevationOnLegList(m_Document, m_TransactionContainer, m_FamilyInstanceComponentWrapper, _bUseOriginalLeg: false, ref m_AnnotationElevationTextSet, ref m_AnnotationGradeTextSet);
			m_Document.Regenerate();
		}
		return result;
	}

	private bool ApplySiteAlignmentElementUpdate()
	{
		bool flag = false;
		bool result = false;
		bool flag2 = true;
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		string text4 = string.Empty;
		string text5 = string.Empty;
		Parameter parameter = null;
		Parameter parameter2 = null;
		FamilyInstance familyInstance = null;
		ElementSet elementSet = null;
		Eps_Leg eps_Leg = null;
		TopographySurfaceContainer _ParkingLotSurface = null;
		List<Eps_Point> _ParkingLotTopoPointList = null;
		List<IntersectionStationContainer> list = null;
		List<IntersectionStationContainer> list2 = null;
		List<IntersectionStationContainer> list3 = null;
		List<IntersectionContainer> list4 = null;
		if (m_FamilyInstanceComponentWrapper != null)
		{
			List<Element> list5 = new List<Element>();
			List<FamilyInstanceComponentContainer> familyContainerList = m_FamilyInstanceComponentWrapper.FamilyContainerList;
			for (int i = 0; i < familyContainerList.Count; i++)
			{
				list5.Insert(0, familyContainerList[i].Element);
				list5.Add(familyContainerList[i].FamilyInstance);
				if (text2 == string.Empty)
				{
					parameter = familyContainerList[i].FamilyInstance.LookupParameter(RevitParameters.PrimaryFormIDFamilyParameter);
					if (parameter != null && parameter.AsString() != string.Empty)
					{
						text2 = parameter.AsString();
					}
				}
				if (text3 == string.Empty)
				{
					parameter = familyContainerList[i].FamilyInstance.LookupParameter(RevitParameters.AdditionalFormIDFamilyParameter);
					if (parameter != null && parameter.AsString() != string.Empty)
					{
						text3 = parameter.AsString();
					}
				}
				if (text4 == string.Empty)
				{
					parameter = familyContainerList[i].FamilyInstance.LookupParameter(RevitParameters.IslandSurfaceIdFamilyParameter);
					if (parameter != null && parameter.AsString() != string.Empty)
					{
						text4 = parameter.AsString();
					}
				}
				if (text5 == string.Empty)
				{
					parameter = familyContainerList[i].FamilyInstance.LookupParameter(RevitParameters.ParkingLotSurfaceIDFamilyParameter);
					if (parameter != null && parameter.AsString() != string.Empty)
					{
						text5 = parameter.AsString();
					}
				}
				if (i == familyContainerList.Count - 1)
				{
					list5.Add(RevitUtility.FindFamilyTypeSymbol(m_Document, familyContainerList[i].FamilyInstance, m_CurrentFamilyType));
				}
			}
			if (m_FamilyInstanceComponentWrapper.IntersectionFamilySymbol != null)
			{
				list5.Add(m_FamilyInstanceComponentWrapper.IntersectionFamilySymbol);
			}
			if (text2 != string.Empty)
			{
				DirectShape directShape = RevitUtility.FindDirectShape(m_Document, text2);
				if (directShape != null)
				{
					list5.Add(directShape);
				}
				else
				{
					FamilyInstance familyInstance2 = RevitUtility.FindMassFamilyInstance(m_Document, text2);
					if (familyInstance2 != null && familyInstance2.Symbol != null && familyInstance2.Symbol.Family != null)
					{
						list5.Add(familyInstance2.Symbol.Family);
					}
				}
			}
			if (text3 != string.Empty)
			{
				DirectShape directShape2 = RevitUtility.FindDirectShape(m_Document, text3);
				if (directShape2 != null)
				{
					list5.Add(directShape2);
				}
				else
				{
					FamilyInstance familyInstance3 = RevitUtility.FindMassFamilyInstance(m_Document, text3);
					if (familyInstance3 != null && familyInstance3.Symbol != null && familyInstance3.Symbol.Family != null)
					{
						list5.Add(familyInstance3.Symbol.Family);
					}
				}
			}
			if (text4 != string.Empty)
			{
				TopographySurface topographySurface = TopographySurfaceManagement.FindTopoSurfaceInstance(m_Document, text4);
				if (topographySurface != null)
				{
					list5.Add(topographySurface);
				}
			}
			if (text5 != string.Empty)
			{
				TopographySurface topographySurface2 = TopographySurfaceManagement.FindTopoSurfaceInstance(m_Document, text5);
				if (topographySurface2 != null)
				{
					list5.Add(topographySurface2);
				}
			}
			for (int j = 0; j < m_FamilyInstanceComponentWrapper.ParkingLotBreaklineFamilyInstanceList.Count; j++)
			{
				list5.Add(m_FamilyInstanceComponentWrapper.ParkingLotBreaklineFamilyInstanceList[j]);
			}
			for (int k = 0; k < m_FamilyInstanceComponentWrapper.IntersectionFamilyInstanceList.Count; k++)
			{
				list5.Add(m_FamilyInstanceComponentWrapper.IntersectionFamilyInstanceList[k]);
			}
			if (m_ProposedSurface.TopographySurface != null)
			{
				list5.Insert(0, m_ProposedSurface.TopographySurface);
			}
			if (list5 != null && list5.Count > 0 && !TopographySurfaceManagement.VerifyUserCanManipulateElements(m_Document, list5))
			{
				return false;
			}
		}
		if (m_bDoesMassLineSubCategoryExist && m_CurrentFamilyType != LandcaddID.ParkingLot)
		{
			GetMassAndLineElements(_bOnlyFindSelSiteComponent: false);
		}
		if (m_FamilyInstanceComponentWrapper != null && m_AllSiteComponentList != null)
		{
			if (m_CurrentFamilyType == LandcaddID.PointWipeout && m_PointWipeoutDisplayForm.ShowAffectedPointsChecked && m_FamilyInstanceComponentWrapper.ProposedLeg != null)
			{
				List<Eps_Point> paPntList = m_ProposedSurface.GetAllVertices();
				RevitGeometry.GetAllPointsInBoundary(ref paPntList, m_FamilyInstanceComponentWrapper.ProposedLeg, null);
				if (paPntList.Count > 0)
				{
					ElementSet elementSet2 = new ElementSet();
					Eps_Line eps_Line = null;
					ModelCurve modelCurve = null;
					m_TransactionContainer.BeginTransaction();
					for (int l = 0; l < paPntList.Count; l++)
					{
						eps_Line = new Eps_Line();
						eps_Line.StartPnt = new Eps_Point(paPntList[l].x + 5.0, paPntList[l].y, 0.0);
						eps_Line.EndPnt = new Eps_Point(paPntList[l].x - 5.0, paPntList[l].y, 0.0);
						modelCurve = RevitUtility.NewModelCurve(m_Document, eps_Line);
						if (m_MassLineSubCategory != null)
						{
							modelCurve.LineStyle = m_MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
						}
						elementSet2.Insert(modelCurve);
						eps_Line.StartPnt = new Eps_Point(paPntList[l].x, paPntList[l].y + 5.0, 0.0);
						eps_Line.EndPnt = new Eps_Point(paPntList[l].x, paPntList[l].y - 5.0, 0.0);
						modelCurve = RevitUtility.NewModelCurve(m_Document, eps_Line);
						if (m_MassLineSubCategory != null)
						{
							modelCurve.LineStyle = m_MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
						}
						elementSet2.Insert(modelCurve);
					}
					m_Document.Regenerate();
					m_TransactionContainer.EndTransaction();
					if (RevitMessageBox.Show(EPShared.LocalRM.GetString("PointsInBoundaryWillBeRemovedMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
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
			SitePlacementSettings sitePlacementSettings = new SitePlacementSettings();
			FamilyInstanceComponentWrapperComparer comparer = new FamilyInstanceComponentWrapperComparer();
			if (m_AllSiteComponentList != null && m_AllSiteComponentList.Count > 0)
			{
				m_AllSiteComponentList.Sort(comparer);
			}
			List<Eps_Point> _TopoPointList = new List<Eps_Point>();
			List<Eps_Point> _CrownPointList = new List<Eps_Point>();
			TopographySurfaceManagement.SetupAllIntersectionStationsForSelectedSiteComponent(m_Document, m_ProposedSurface, m_TransactionContainer, m_bFindIntersectionStations, ref m_FamilyInstanceComponentWrapper, ref m_AllSiteComponentList);
			TopographySurfaceManagement.SetupAllIntersectionStationsForAllSiteComponents(m_Document, m_ProposedSurface, m_TransactionContainer, m_FamilyInstanceComponentWrapper.ChainID, ref m_AllSiteComponentList);
			TopographySurfaceContainer topographySurfaceContainer = TopographySurfaceManagement.FindParentSurfaceAndRemovePointsForSelectedCurbComponent(m_Document, m_ProposedSurface, m_FamilyInstanceComponentWrapper, ref _TopoPointList);
			TopographySurfaceContainer topographySurfaceContainer2;
			if (topographySurfaceContainer != null && topographySurfaceContainer.TopographySurface != null && string.Compare(m_ProposedSurface.TopographySurface.UniqueId, topographySurfaceContainer.TopographySurface.UniqueId, ignoreCase: true) != 0)
			{
				topographySurfaceContainer2 = topographySurfaceContainer;
				flag2 = false;
			}
			else
			{
				topographySurfaceContainer2 = m_ProposedSurface;
				_TopoPointList = m_BaseSurface.GetAllVertices();
			}
			ProgressBarForm progressBarForm = new ProgressBarForm();
			progressBarForm.ShowForm();
			progressBarForm.Refresh();
			progressBarForm.MinValue = 0;
			progressBarForm.MaxValue = m_AllSiteComponentList.Count;
			progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarGradingToposurfaceMessage");
			for (int m = 0; m < m_AllSiteComponentList.Count; m++)
			{
				elementSet = m_AllSiteComponentList[m].GetFamilyInstanceElementSet();
				if (elementSet == null || elementSet.Size == 0)
				{
					continue;
				}
				familyInstance = m_AllSiteComponentList[m].GetFirstExistingFamilyInstance();
				if (familyInstance != null && familyInstance.Symbol != null)
				{
					sitePlacementSettings.PlacementSymbol = familyInstance.Symbol;
					parameter = familyInstance.LookupParameter(RevitParameters.ProjectInnerSlopesFamilyParameter);
					if (parameter != null)
					{
						sitePlacementSettings.ProjectInnerSlopes = parameter.AsInteger();
					}
					parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
					if (parameter != null && parameter.AsString() != string.Empty)
					{
						sitePlacementSettings.Name = parameter.AsString();
					}
					parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
					if (parameter != null && parameter.AsString() != string.Empty)
					{
						foreach (int value6 in Enum.GetValues(typeof(LandcaddID)))
						{
							if (string.Compare(parameter.AsString(), value6.ToString("000"), ignoreCase: true) == 0)
							{
								sitePlacementSettings.LocateType = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value6);
								break;
							}
						}
					}
					parameter = familyInstance.LookupParameter(RevitParameters.ElevationFamilyParameter);
					if (parameter != null)
					{
						sitePlacementSettings.Elevation = parameter.AsDouble();
					}
					parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentIsRelativeElevationParameter);
					if (parameter != null)
					{
						sitePlacementSettings.IsRelativeElevation = false;
						int num = parameter.AsInteger();
						if (num == 1)
						{
							sitePlacementSettings.IsRelativeElevation = true;
						}
					}
					sitePlacementSettings.Type = RevitUtility.FindFamilyTypeSymbol(m_Document, familyInstance, sitePlacementSettings.LocateType);
					parameter = familyInstance.LookupParameter(RevitParameters.UseRightSideGenericFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						if (parameter.AsInteger() == 1)
						{
							sitePlacementSettings.UseRightSide = true;
						}
						else
						{
							sitePlacementSettings.UseRightSide = false;
						}
					}
					parameter = familyInstance.LookupParameter(RevitParameters.LinearPointIntervalParameter);
					if (parameter != null)
					{
						sitePlacementSettings.LinearPointInterval = parameter.AsDouble();
					}
					if (m_CurrentFamilyType == LandcaddID.RetainingWall)
					{
						sitePlacementSettings.UseRightSide = m_bRetainingWallUseRightLeg;
					}
					foreach (Element item4 in elementSet)
					{
						parameter = item4.LookupParameter(RevitParameters.PrimaryFormIDFamilyParameter);
						if (parameter != null && parameter.AsString() != string.Empty)
						{
							sitePlacementSettings.LoftFormID = parameter.AsString();
						}
						parameter = item4.LookupParameter(RevitParameters.AdditionalFormIDFamilyParameter);
						if (parameter != null && parameter.AsString() != string.Empty)
						{
							sitePlacementSettings.AdditionalLoftFormID = parameter.AsString();
						}
						parameter = item4.LookupParameter(RevitParameters.IslandSurfaceIdFamilyParameter);
						if (parameter != null && parameter.AsString() != string.Empty)
						{
							sitePlacementSettings.IslandSurfaceID = parameter.AsString();
						}
						parameter = item4.LookupParameter(RevitParameters.ParkingLotSurfaceIDFamilyParameter);
						if (parameter != null && parameter.AsString() != string.Empty)
						{
							sitePlacementSettings.ParkingLotSurfaceID = parameter.AsString();
						}
						parameter = item4.LookupParameter(RevitParameters.ParentSurfaceIDFamilyParameter);
						if (parameter != null && parameter.AsString() != string.Empty)
						{
							sitePlacementSettings.ParentTopoSurfaceID = parameter.AsString();
						}
					}
					parameter = familyInstance.LookupParameter(RevitParameters.TopoDrawOrderFamilyParameter);
					if (parameter != null)
					{
						sitePlacementSettings.TopoDrawOrder = parameter.AsInteger();
					}
				}
				sitePlacementSettings.Level = m_Document.ActiveView.GenLevel;
				sitePlacementSettings.TransactionContainer = m_TransactionContainer;
				List<Element> list6 = new List<Element>();
				bool flag3 = false;
				if (string.Compare(m_AllSiteComponentList[m].ChainID, m_FamilyInstanceComponentWrapper.ChainID, ignoreCase: true) == 0)
				{
					m_AllSiteComponentList[m].ParkingLotBreaklineList = m_FamilyInstanceComponentWrapper.ParkingLotBreaklineList;
					m_AllSiteComponentList[m].ParkingLotBreaklineElementList = m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementList;
					m_AllSiteComponentList[m].ParkingLotBreaklineLinearPointIntervals = m_FamilyInstanceComponentWrapper.ParkingLotBreaklineLinearPointIntervals;
					result = true;
					m_TransactionContainer.BeginTransaction();
					m_Document.Delete(ElementSetConverter.ToIDList(m_FamilyInstanceComponentWrapper.GetOriginalElementSet()));
					m_Document.Delete(ElementSetConverter.ToIDList(m_FamilyInstanceComponentWrapper.GetFamilyInstanceElementSet()));
					m_FamilyInstanceComponentWrapper.FamilyContainerList.Clear();
					if (m_FamilyInstanceComponentWrapper.ProposedLeg != null && m_FamilyInstanceComponentWrapper.ProposedElementSet != null)
					{
						RevitUtility.HideElementsInAllViews(m_Document, m_FamilyInstanceComponentWrapper.ProposedElementSet);
					}
					else
					{
						m_AllSiteComponentList.RemoveAt(m);
						m--;
						flag = true;
					}
					if (sitePlacementSettings.LoftFormID != string.Empty)
					{
						DirectShape directShape3 = RevitUtility.FindDirectShape(m_Document, sitePlacementSettings.LoftFormID);
						if (directShape3 != null)
						{
							m_Document.Delete(directShape3.Id);
						}
						else
						{
							FamilyInstance familyInstance4 = RevitUtility.FindMassFamilyInstance(m_Document, sitePlacementSettings.LoftFormID);
							if (familyInstance4 != null && familyInstance4.Symbol != null && familyInstance4.Symbol.Family != null)
							{
								m_Document.Delete(familyInstance4.Symbol.Family.Id);
							}
						}
					}
					if (sitePlacementSettings.AdditionalLoftFormID != string.Empty)
					{
						DirectShape directShape4 = RevitUtility.FindDirectShape(m_Document, sitePlacementSettings.AdditionalLoftFormID);
						if (directShape4 != null)
						{
							m_Document.Delete(directShape4.Id);
						}
						else
						{
							FamilyInstance familyInstance5 = RevitUtility.FindMassFamilyInstance(m_Document, sitePlacementSettings.AdditionalLoftFormID);
							if (familyInstance5 != null && familyInstance5.Symbol != null && familyInstance5.Symbol.Family != null)
							{
								m_Document.Delete(familyInstance5.Symbol.Family.Id);
							}
						}
					}
					if (sitePlacementSettings.IslandSurfaceID != string.Empty)
					{
						TopographySurface topographySurface3 = TopographySurfaceManagement.FindTopoSurfaceInstance(m_Document, sitePlacementSettings.IslandSurfaceID);
						if (topographySurface3 != null)
						{
							m_Document.Delete(topographySurface3.Id);
						}
					}
					if (sitePlacementSettings.ParkingLotSurfaceID != string.Empty)
					{
						TopographySurface topographySurface4 = TopographySurfaceManagement.FindTopoSurfaceInstance(m_Document, sitePlacementSettings.ParkingLotSurfaceID);
						if (topographySurface4 != null)
						{
							m_Document.Delete(topographySurface4.Id);
						}
					}
					if (sitePlacementSettings.ParentTopoSurfaceID != string.Empty)
					{
						text = sitePlacementSettings.ParentTopoSurfaceID;
					}
					if (m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementSet != null && m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementSet.Size > 0)
					{
						RevitUtility.HideElementsInAllViews(m_Document, m_FamilyInstanceComponentWrapper.ParkingLotBreaklineElementSet);
					}
					for (int n = 0; n < m_FamilyInstanceComponentWrapper.ParkingLotBreaklineFamilyInstanceList.Count; n++)
					{
						m_Document.Delete(m_FamilyInstanceComponentWrapper.ParkingLotBreaklineFamilyInstanceList[n].Id);
					}
					for (int num2 = 0; num2 < m_FamilyInstanceComponentWrapper.IntersectionFamilyInstanceList.Count; num2++)
					{
						m_Document.Delete(m_FamilyInstanceComponentWrapper.IntersectionFamilyInstanceList[num2].Id);
					}
					m_TransactionContainer.EndTransaction();
					if (m_FamilyInstanceComponentWrapper.ProposedLeg != null && m_FamilyInstanceComponentWrapper.ProposedElementSet != null)
					{
						eps_Leg = Eps_Leg.CopyLeg(m_FamilyInstanceComponentWrapper.ProposedLeg);
						foreach (Element item5 in m_FamilyInstanceComponentWrapper.ProposedElementSet)
						{
							list6.Add(item5);
						}
						if (m_FamilyInstanceComponentWrapper.ProposedFamilySymbol != null)
						{
							sitePlacementSettings.Type = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol;
						}
						flag3 = true;
					}
					sitePlacementSettings.IntersectionPlacementSymbol = m_FamilyInstanceComponentWrapper.IntersectionFamilySymbol;
					list = m_FamilyInstanceComponentWrapper.LeftIntersectionStations;
					list2 = m_FamilyInstanceComponentWrapper.CenterIntersectionStations;
					list3 = m_FamilyInstanceComponentWrapper.RightIntersectionStations;
					list4 = m_FamilyInstanceComponentWrapper.IntersectionContainerList;
				}
				else
				{
					if (sitePlacementSettings.LocateType == LandcaddID.Curb && sitePlacementSettings.ParentTopoSurfaceID != string.Empty && string.Compare(sitePlacementSettings.ParentTopoSurfaceID, topographySurfaceContainer2.TopographySurface.UniqueId, ignoreCase: true) != 0)
					{
						sitePlacementSettings.LoftFormID = string.Empty;
						sitePlacementSettings.IslandSurfaceID = string.Empty;
						sitePlacementSettings.ParkingLotSurfaceID = string.Empty;
						continue;
					}
					if (m_AllSiteComponentList[m].OriginalLeg == null && m_AllSiteComponentList[m].ProposedLeg == null)
					{
						m_AllSiteComponentList[m].CreateOriginalLeg(m_ProposedSurface);
					}
					if (m_AllSiteComponentList[m].OriginalLeg != null || m_AllSiteComponentList[m].ProposedLeg != null)
					{
						if (flag)
						{
							m_AllSiteComponentList[m].DrawOrder--;
							foreach (Element item6 in m_AllSiteComponentList[m].GetFamilyInstanceElementSet())
							{
								item6.LookupParameter(RevitParameters.TopoDrawOrderFamilyParameter)?.Set(m_AllSiteComponentList[m].DrawOrder);
							}
						}
						if (m_AllSiteComponentList[m].ProposedFamilySymbol == null)
						{
							m_AllSiteComponentList[m].ProposedFamilySymbol = sitePlacementSettings.Type;
						}
						if (m_AllSiteComponentList[m].ProposedLeg != null)
						{
							eps_Leg = Eps_Leg.CopyLeg(m_AllSiteComponentList[m].ProposedLeg);
						}
						else if (m_AllSiteComponentList[m].OriginalLeg != null)
						{
							eps_Leg = Eps_Leg.CopyLeg(m_AllSiteComponentList[m].OriginalLeg);
						}
					}
					sitePlacementSettings.IntersectionPlacementSymbol = m_AllSiteComponentList[m].IntersectionFamilySymbol;
					list = m_AllSiteComponentList[m].LeftIntersectionStations;
					list2 = m_AllSiteComponentList[m].CenterIntersectionStations;
					list3 = m_AllSiteComponentList[m].RightIntersectionStations;
					list4 = m_AllSiteComponentList[m].IntersectionContainerList;
					flag3 = m_AllSiteComponentList[m].RedrawForm;
					if (flag3)
					{
						foreach (Element item7 in m_AllSiteComponentList[m].GetOriginalElementSet())
						{
							list6.Add(item7);
						}
						if (m_AllSiteComponentList[m].ProposedFamilySymbol != null)
						{
							sitePlacementSettings.Type = m_AllSiteComponentList[m].ProposedFamilySymbol;
						}
						m_TransactionContainer.BeginTransaction();
						foreach (Element item8 in m_AllSiteComponentList[m].GetFamilyInstanceElementSet())
						{
							parameter = item8.LookupParameter(RevitParameters.PrimaryFormIDFamilyParameter);
							if (parameter != null && parameter.AsString() != string.Empty)
							{
								DirectShape directShape5 = RevitUtility.FindDirectShape(m_Document, parameter.AsString());
								if (directShape5 != null)
								{
									m_Document.Delete(directShape5.Id);
								}
								else
								{
									FamilyInstance familyInstance6 = RevitUtility.FindMassFamilyInstance(m_Document, parameter.AsString());
									if (familyInstance6 != null && familyInstance6.Symbol != null && familyInstance6.Symbol.Family != null)
									{
										m_Document.Delete(familyInstance6.Symbol.Family.Id);
									}
								}
							}
							parameter = item8.LookupParameter(RevitParameters.AdditionalFormIDFamilyParameter);
							if (parameter == null || !(parameter.AsString() != string.Empty))
							{
								continue;
							}
							DirectShape directShape6 = RevitUtility.FindDirectShape(m_Document, parameter.AsString());
							if (directShape6 != null)
							{
								m_Document.Delete(directShape6.Id);
								continue;
							}
							FamilyInstance familyInstance7 = RevitUtility.FindMassFamilyInstance(m_Document, parameter.AsString());
							if (familyInstance7 != null && familyInstance7.Symbol != null && familyInstance7.Symbol.Family != null)
							{
								m_Document.Delete(familyInstance7.Symbol.Family.Id);
							}
						}
						m_Document.Delete(ElementSetConverter.ToIDList(m_AllSiteComponentList[m].GetFamilyInstanceElementSet()));
						m_TransactionContainer.EndTransaction();
						if (list != null && list.Count > 0 && list3 != null && list3.Count > 0)
						{
							familyInstance = m_FamilyInstanceComponentWrapper.GetFirstExistingFamilyInstance();
							if (familyInstance != null)
							{
								for (int num3 = 0; num3 < list.Count; num3++)
								{
									list[num3].PrimaryPlacementId = familyInstance.UniqueId;
								}
								for (int num4 = 0; num4 < list3.Count; num4++)
								{
									list3[num4].PrimaryPlacementId = familyInstance.UniqueId;
								}
							}
						}
					}
				}
				if (m_CurrentFamilyType == LandcaddID.Curb)
				{
					if (eps_Leg != null && eps_Leg.IsLegClosed())
					{
						double num5 = 0.0;
						double num6 = 0.0;
						double num7 = 0.0;
						double num8 = 0.0;
						parameter = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetCurbWidthParameter);
						if (parameter != null)
						{
							num8 = parameter.AsDouble();
						}
						parameter = m_FamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetGutterWidthParameter);
						if (parameter != null)
						{
							num7 = parameter.AsDouble();
						}
						num5 = num8;
						num6 = num7;
						Eps_Leg ppOffsetLegList = new Eps_Leg();
						RevitGeometry.OffsetLegList(eps_Leg, num5 + num6, 1, ref ppOffsetLegList, TrimClosedObject: true);
						Eps_Leg ppOffsetLegList2 = new Eps_Leg();
						RevitGeometry.OffsetLegList(eps_Leg, num5 + num6, 2, ref ppOffsetLegList2, TrimClosedObject: true);
						if (RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2) < RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList))
						{
							eps_Leg = eps_Leg.Reverse();
						}
					}
					if (!flag2 && !flag3)
					{
						sitePlacementSettings.LoftFormID = string.Empty;
						sitePlacementSettings.IslandSurfaceID = string.Empty;
						sitePlacementSettings.ParkingLotSurfaceID = string.Empty;
						continue;
					}
				}
				if (eps_Leg != null && eps_Leg.LegCount > 0)
				{
					if (!flag3)
					{
						if (!TopographySurfaceManagement.RebuildProposedTopographySurface(m_Document, sitePlacementSettings, m_dArcPointInterval, eps_Leg, topographySurfaceContainer2, list6, flag3, m_AllSiteComponentList[m].ParkingLotBreaklineList, m_AllSiteComponentList[m].ParkingLotBreaklineElementList, m_AllSiteComponentList[m].ParkingLotBreaklineLinearPointIntervals, list, list2, list3, list4, ref _TopoPointList, ref _CrownPointList))
						{
							return false;
						}
					}
					else
					{
						progressBarForm.Close();
						List<FamilyInstance> _FamilyInstanceList = null;
						if (!TopographySurfaceManagement.RebuildProposedTopographySurface(m_Document, sitePlacementSettings, m_dArcPointInterval, eps_Leg, topographySurfaceContainer2, list6, flag3, m_AllSiteComponentList[m].ParkingLotBreaklineList, m_AllSiteComponentList[m].ParkingLotBreaklineElementList, m_AllSiteComponentList[m].ParkingLotBreaklineLinearPointIntervals, list, list2, list3, list4, ref _TopoPointList, ref _CrownPointList, ref _ParkingLotSurface, ref _ParkingLotTopoPointList, ref _FamilyInstanceList))
						{
							return false;
						}
						progressBarForm = new ProgressBarForm();
						progressBarForm.ShowForm();
						progressBarForm.Refresh();
						progressBarForm.MinValue = 0;
						progressBarForm.MaxValue = m_AllSiteComponentList.Count;
						progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarGradingToposurfaceMessage");
						progressBarForm.UpdateProgressBar(m);
						if (string.Compare(m_AllSiteComponentList[m].ChainID, m_FamilyInstanceComponentWrapper.ChainID, ignoreCase: true) == 0)
						{
							m_FamilyInstanceComponentWrapper.FamilyContainerList.Clear();
							m_AllSiteComponentList[m].FamilyContainerList.Clear();
							if (_FamilyInstanceList != null && _FamilyInstanceList.Count > 0 && _FamilyInstanceList.Count == list6.Count)
							{
								int num9 = 0;
								foreach (Element item9 in list6)
								{
									m_FamilyInstanceComponentWrapper.FamilyContainerList.Add(new FamilyInstanceComponentContainer(item9, _FamilyInstanceList[num9]));
									m_AllSiteComponentList[m].FamilyContainerList.Add(new FamilyInstanceComponentContainer(item9, _FamilyInstanceList[num9]));
									num9++;
								}
							}
						}
						else if (m_FamilyInstanceComponentWrapper.LeftIntersectionStations != null && m_FamilyInstanceComponentWrapper.LeftIntersectionStations.Count > 0 && m_FamilyInstanceComponentWrapper.RightIntersectionStations != null && m_FamilyInstanceComponentWrapper.RightIntersectionStations.Count > 0 && _FamilyInstanceList != null && _FamilyInstanceList.Count > 0)
						{
							for (int num10 = 0; num10 < m_FamilyInstanceComponentWrapper.LeftIntersectionStations.Count; num10++)
							{
								m_FamilyInstanceComponentWrapper.LeftIntersectionStations[num10].PrimaryPlacementId = _FamilyInstanceList[0].UniqueId;
							}
							for (int num11 = 0; num11 < m_FamilyInstanceComponentWrapper.RightIntersectionStations.Count; num11++)
							{
								m_FamilyInstanceComponentWrapper.RightIntersectionStations[num11].PrimaryPlacementId = _FamilyInstanceList[0].UniqueId;
							}
						}
					}
					eps_Leg = null;
				}
				sitePlacementSettings.LoftFormID = string.Empty;
				sitePlacementSettings.IslandSurfaceID = string.Empty;
				sitePlacementSettings.ParkingLotSurfaceID = string.Empty;
				progressBarForm.UpdateProgressBar(1);
			}
			progressBarForm.SetMaxValue();
			Thread.Sleep(300);
			progressBarForm.Close();
			if (_CrownPointList != null && _CrownPointList.Count > 0)
			{
				_TopoPointList.AddRange(_CrownPointList);
			}
			progressBarForm = new ProgressBarForm();
			progressBarForm.ShowForm();
			progressBarForm.Refresh();
			progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarUpdatingModelMessage");
			progressBarForm.MinValue = 0;
			progressBarForm.MaxValue = 12;
			List<XYZ> PointXYZList = new List<XYZ>();
			List<XYZ> list7 = new List<XYZ>();
			double _dOffset = 0.0;
			double _dStation = 0.0;
			bool flag4 = false;
			bool flag5 = false;
			for (int num12 = 0; num12 < _TopoPointList.Count; num12++)
			{
				if (_TopoPointList[num12] != null)
				{
					for (int num13 = 0; num13 < topographySurfaceContainer2.SubregionBuildingPadList.Count; num13++)
					{
						if (topographySurfaceContainer2.SubregionBuildingPadList[num13] == null)
						{
							continue;
						}
						if (topographySurfaceContainer2.SubregionBuildingPadList[num13].BoundaryLeg != null && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_TopoPointList[num12], topographySurfaceContainer2.SubregionBuildingPadList[num13].BoundaryLeg, ref _dStation, ref _dOffset) == 1700 && Math.Abs(_dOffset) <= 0.001)
						{
							flag5 = true;
							break;
						}
						if (topographySurfaceContainer2.SubregionBuildingPadList[num13].GetSurfaceContainerContainingPoint(_TopoPointList[num12]) != null)
						{
							if (topographySurfaceContainer2.SubregionBuildingPadList[num13].TopographySurfaceType == TopographySurfaceType.Subregion)
							{
								flag4 = true;
							}
							else
							{
								flag5 = true;
							}
							break;
						}
					}
					if (flag4)
					{
						list7.Add(_TopoPointList[num12].XYZ);
					}
					else if (!flag5)
					{
						PointXYZList.Add(_TopoPointList[num12].XYZ);
					}
				}
				flag5 = false;
				flag4 = false;
			}
			progressBarForm.UpdateProgressBar(1);
			if (PointXYZList.Count > 0)
			{
				List<XYZ> list8 = new List<XYZ>();
				list8.AddRange(topographySurfaceContainer2.TopographySurface.GetBoundaryPoints().ToList());
				TopographySurfaceManagement.RemoveDuplicateXY(ref PointXYZList);
				TopographySurfaceManagement.RemoveOriginalTopoPoints(list8, ref PointXYZList);
				string name = m_TransactionContainer.GetName();
				m_TransactionContainer.Commit();
				IList<XYZ> list9 = null;
				list9 = topographySurfaceContainer2.TopographySurface.GetInteriorPoints();
				m_TransactionContainer.StartTopographyEditScope(name, topographySurfaceContainer2.TopographySurface.Id);
				SDTimer sDTimer = new SDTimer("Point_Update_Modify", m_Document.Application);
				sDTimer.Start();
				SDTimer sDTimer2 = new SDTimer("Traverse_Point_Modify", m_Document.Application);
				sDTimer2.Start();
				HashSet<XYZ> hashSet = new HashSet<XYZ>(list9, new XYZEqualityComparer());
				for (int num14 = PointXYZList.Count - 1; num14 >= 0; num14--)
				{
					if (hashSet.Contains(PointXYZList[num14]))
					{
						hashSet.Remove(PointXYZList[num14]);
						PointXYZList.RemoveAt(num14);
					}
				}
				sDTimer2.Stop();
				m_TransactionContainer.Start("Update Topo Points");
				if (hashSet.Count <= PointXYZList.Count)
				{
					int num15 = 0;
					foreach (XYZ item10 in hashSet)
					{
						topographySurfaceContainer2.TopographySurface.MovePoint(item10, PointXYZList[num15]);
						num15++;
					}
					List<XYZ> list10 = new List<XYZ>();
					for (; num15 < PointXYZList.Count; num15++)
					{
						list10.Add(PointXYZList[num15]);
					}
					if (list10.Count > 0)
					{
						topographySurfaceContainer2.TopographySurface.AddPoints(list10);
					}
				}
				else
				{
					int num16 = 0;
					int count = PointXYZList.Count;
					List<XYZ> list11 = new List<XYZ>();
					foreach (XYZ item11 in hashSet)
					{
						if (num16 < count)
						{
							topographySurfaceContainer2.TopographySurface.MovePoint(item11, PointXYZList[num16]);
						}
						else
						{
							list11.Add(item11);
						}
						num16++;
					}
					if (list11.Count > 0)
					{
						topographySurfaceContainer2.TopographySurface.DeletePoints(list11);
					}
				}
				m_TransactionContainer.Commit();
				sDTimer.Stop();
				TopographyEditFailuresPreprocessor failuresPreprocessor = new TopographyEditFailuresPreprocessor();
				m_TransactionContainer.CommitTopographyEditScope(ref failuresPreprocessor);
				m_TransactionContainer.Start(name);
				topographySurfaceContainer2.Refresh();
				m_Document.Regenerate();
				progressBarForm.UpdateProgressBar(7);
				if (topographySurfaceContainer2.SubregionBuildingPadList != null && topographySurfaceContainer2.SubregionBuildingPadList.Count > 0)
				{
					List<TopographySurfaceContainer> surfaceList = TopographySurfaceManagement.FindAllMainSurfaces(m_Document, _bFindMinMaxPoints: true);
					Eps_Point eps_Point = TopographySurfaceManagement.FindToposurfaceAdjustmentPoint(m_ProposedSurface, surfaceList);
					ElementTransformUtils.MoveElement(m_Document, m_ProposedSurface.TopographySurface.Id, eps_Point.XY);
					List<TopographySurfaceContainer> list12 = new List<TopographySurfaceContainer>();
					List<ElementId> list13 = new List<ElementId>();
					for (int num17 = 0; num17 < topographySurfaceContainer2.SubregionBuildingPadList.Count; num17++)
					{
						if (topographySurfaceContainer2.SubregionBuildingPadList[num17] == null)
						{
							continue;
						}
						if (topographySurfaceContainer2.SubregionBuildingPadList[num17].TopographySurface != null)
						{
							list13.Add(topographySurfaceContainer2.SubregionBuildingPadList[num17].TopographySurface.Id);
							if (topographySurfaceContainer2.SubregionBuildingPadList[num17].TopographySurfaceType == TopographySurfaceType.Subregion)
							{
								list12.Add(topographySurfaceContainer2.SubregionBuildingPadList[num17]);
							}
						}
						else if (topographySurfaceContainer2.SubregionBuildingPadList[num17].BuildingPad != null)
						{
							list13.Add(topographySurfaceContainer2.SubregionBuildingPadList[num17].BuildingPad.Id);
						}
					}
					List<KeyValuePair<FamilyInstance, TopographySurfaceContainer>> updateGenericModelList = TopographySurfaceManagement.FindPlantMixGenericModelList(m_Document, list12);
					List<ElementId> newElementIds = null;
					try
					{
						newElementIds = LinearArray.ArrayElementsWithoutAssociation(m_Document, m_Document.ActiveView, list13.ToArray(), 2, eps_Point.XY, ArrayAnchorMember.Second).ToList();
					}
					catch (Exception ex)
					{
						if (ex is AccessViolationException || ex is InvalidOperationException)
						{
							progressBarForm.SetMaxValue();
							Thread.Sleep(300);
							progressBarForm.Close();
							RevitMessageBox.Show(EPShared.LocalRM.GetString("SubregionBuildingPadBoundaryErrorMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
							return false;
						}
					}
					ElementTransformUtils.MoveElement(m_Document, m_ProposedSurface.TopographySurface.Id, new XYZ(0.0 - eps_Point.x, 0.0 - eps_Point.y, 0.0));
					if (list7.Count > 0)
					{
						m_ProposedSurface.TopographySurface.AddPoints(list7);
					}
					List<TopographySurfaceContainer> list14 = null;
					list14 = TopographySurfaceManagement.FindNewTopographySurfaceContainers(m_Document, newElementIds);
					TopographySurfaceManagement.UpdatePlantMixGenericModelList(updateGenericModelList, list14);
					if (list14 != null && list14.Count > 0)
					{
						m_ProposedSurface.SubregionBuildingPadList.AddRange(list14);
					}
				}
				progressBarForm.UpdateProgressBar(3);
				topographySurfaceContainer2 = m_ProposedSurface;
				if (flag2 && m_TopoPlacementFamilyInstance != null)
				{
					parameter = m_TopoPlacementFamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(m_ProposedSurface.TopographySurface.UniqueId);
					}
				}
				if (m_CurrentFamilyType == LandcaddID.Curb && text != string.Empty)
				{
					LandcaddID landcaddID = LandcaddID.Curb;
					for (int num18 = 0; num18 < m_AllSiteComponentList.Count; num18++)
					{
						elementSet = m_AllSiteComponentList[num18].GetFamilyInstanceElementSet();
						if (elementSet == null || elementSet.Size == 0)
						{
							continue;
						}
						familyInstance = m_AllSiteComponentList[num18].GetFirstExistingFamilyInstance();
						if (familyInstance == null)
						{
							continue;
						}
						parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
						if (parameter == null || !(parameter.AsString() != string.Empty))
						{
							continue;
						}
						foreach (int value7 in Enum.GetValues(typeof(LandcaddID)))
						{
							if (string.Compare(parameter.AsString(), value7.ToString("000"), ignoreCase: true) == 0)
							{
								landcaddID = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value7);
								break;
							}
						}
						if (landcaddID != LandcaddID.Curb)
						{
							continue;
						}
						foreach (Element item12 in elementSet)
						{
							parameter = item12.LookupParameter(RevitParameters.ParentSurfaceIDFamilyParameter);
							if (parameter != null && !parameter.IsReadOnly && string.Compare(text, parameter.AsString(), ignoreCase: true) == 0)
							{
								parameter.Set(m_ProposedSurface.TopographySurface.UniqueId);
							}
						}
					}
				}
			}
			progressBarForm.SetMaxValue();
			Thread.Sleep(300);
			progressBarForm.Close();
			if (m_ParkingLotCurbSiteCompList != null)
			{
				for (int num19 = 0; num19 < m_ParkingLotCurbSiteCompList.Count; num19++)
				{
					elementSet = m_ParkingLotCurbSiteCompList[num19].GetFamilyInstanceElementSet();
					if (elementSet == null || elementSet.Size == 0)
					{
						continue;
					}
					familyInstance = m_ParkingLotCurbSiteCompList[num19].GetFirstExistingFamilyInstance();
					if (familyInstance.Symbol != null)
					{
						sitePlacementSettings.PlacementSymbol = familyInstance.Symbol;
						parameter = familyInstance.LookupParameter(RevitParameters.ProjectInnerSlopesFamilyParameter);
						if (parameter != null)
						{
							sitePlacementSettings.ProjectInnerSlopes = parameter.AsInteger();
						}
						parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
						if (parameter != null && parameter.AsString() != string.Empty)
						{
							sitePlacementSettings.Name = parameter.AsString();
						}
						parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
						if (parameter != null && parameter.AsString() != string.Empty)
						{
							foreach (int value8 in Enum.GetValues(typeof(LandcaddID)))
							{
								if (string.Compare(parameter.AsString(), value8.ToString("000"), ignoreCase: true) == 0)
								{
									sitePlacementSettings.LocateType = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value8);
									break;
								}
							}
						}
						parameter = familyInstance.LookupParameter(RevitParameters.ElevationFamilyParameter);
						if (parameter != null)
						{
							sitePlacementSettings.Elevation = parameter.AsDouble();
						}
						parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentIsRelativeElevationParameter);
						if (parameter != null)
						{
							sitePlacementSettings.IsRelativeElevation = false;
							int num20 = parameter.AsInteger();
							if (num20 == 1)
							{
								sitePlacementSettings.IsRelativeElevation = true;
							}
						}
						sitePlacementSettings.Type = RevitUtility.FindFamilyTypeSymbol(m_Document, familyInstance, sitePlacementSettings.LocateType);
						parameter = familyInstance.LookupParameter(RevitParameters.UseRightSideGenericFamilyParameter);
						if (parameter != null)
						{
							if (parameter.AsInteger() == 1)
							{
								sitePlacementSettings.UseRightSide = true;
							}
							else
							{
								sitePlacementSettings.UseRightSide = false;
							}
						}
						foreach (Element item13 in elementSet)
						{
							parameter = item13.LookupParameter(RevitParameters.PrimaryFormIDFamilyParameter);
							if (parameter != null && parameter.AsString() != string.Empty)
							{
								sitePlacementSettings.LoftFormID = parameter.AsString();
							}
							parameter = item13.LookupParameter(RevitParameters.AdditionalFormIDFamilyParameter);
							if (parameter != null && parameter.AsString() != string.Empty)
							{
								sitePlacementSettings.AdditionalLoftFormID = parameter.AsString();
							}
							parameter = item13.LookupParameter(RevitParameters.IslandSurfaceIdFamilyParameter);
							if (parameter != null && parameter.AsString() != string.Empty)
							{
								sitePlacementSettings.IslandSurfaceID = parameter.AsString();
							}
						}
						parameter = familyInstance.LookupParameter(RevitParameters.TopoDrawOrderFamilyParameter);
						if (parameter != null)
						{
							sitePlacementSettings.TopoDrawOrder = parameter.AsInteger();
						}
					}
					sitePlacementSettings.Level = m_Document.ActiveView.GenLevel;
					sitePlacementSettings.TransactionContainer = m_TransactionContainer;
					List<Element> list15 = new List<Element>();
					bool bDrawForm = false;
					m_TransactionContainer.BeginTransaction();
					m_Document.Delete(ElementSetConverter.ToIDList(m_ParkingLotCurbSiteCompList[num19].GetOriginalElementSet()));
					m_Document.Delete(ElementSetConverter.ToIDList(m_ParkingLotCurbSiteCompList[num19].GetFamilyInstanceElementSet()));
					if (m_ParkingLotCurbSiteCompList[num19].ProposedLeg != null && m_ParkingLotCurbSiteCompList[num19].ProposedElementSet != null)
					{
						RevitUtility.HideElementsInAllViews(m_Document, m_FamilyInstanceComponentWrapper.ProposedElementSet);
					}
					else
					{
						m_ParkingLotCurbSiteCompList.RemoveAt(num19);
						num19--;
						flag = true;
					}
					if (sitePlacementSettings.LoftFormID != string.Empty)
					{
						DirectShape directShape7 = RevitUtility.FindDirectShape(m_Document, sitePlacementSettings.LoftFormID);
						if (directShape7 != null)
						{
							m_Document.Delete(directShape7.Id);
						}
						else
						{
							FamilyInstance familyInstance8 = RevitUtility.FindMassFamilyInstance(m_Document, sitePlacementSettings.LoftFormID);
							if (familyInstance8 != null && familyInstance8.Symbol != null && familyInstance8.Symbol.Family != null)
							{
								m_Document.Delete(familyInstance8.Symbol.Family.Id);
							}
						}
					}
					if (sitePlacementSettings.AdditionalLoftFormID != string.Empty)
					{
						DirectShape directShape8 = RevitUtility.FindDirectShape(m_Document, sitePlacementSettings.AdditionalLoftFormID);
						if (directShape8 != null)
						{
							m_Document.Delete(directShape8.Id);
						}
						else
						{
							FamilyInstance familyInstance9 = RevitUtility.FindMassFamilyInstance(m_Document, sitePlacementSettings.AdditionalLoftFormID);
							if (familyInstance9 != null && familyInstance9.Symbol != null && familyInstance9.Symbol.Family != null)
							{
								m_Document.Delete(familyInstance9.Symbol.Family.Id);
							}
						}
					}
					if (sitePlacementSettings.IslandSurfaceID != string.Empty)
					{
						TopographySurface topographySurface5 = TopographySurfaceManagement.FindTopoSurfaceInstance(m_Document, sitePlacementSettings.IslandSurfaceID);
						if (topographySurface5 != null)
						{
							m_Document.Delete(topographySurface5.Id);
						}
					}
					m_TransactionContainer.EndTransaction();
					if (m_ParkingLotCurbSiteCompList[num19].ProposedLeg != null && m_ParkingLotCurbSiteCompList[num19].ProposedElementSet != null)
					{
						eps_Leg = Eps_Leg.CopyLeg(m_ParkingLotCurbSiteCompList[num19].ProposedLeg);
						foreach (Element item14 in m_ParkingLotCurbSiteCompList[num19].ProposedElementSet)
						{
							list15.Add(item14);
						}
						if (m_ParkingLotCurbSiteCompList[num19].ProposedFamilySymbol != null)
						{
							sitePlacementSettings.Type = m_ParkingLotCurbSiteCompList[num19].ProposedFamilySymbol;
						}
						bDrawForm = true;
					}
					if (eps_Leg != null && eps_Leg.IsLegClosed())
					{
						double num21 = 0.0;
						double num22 = 0.0;
						double num23 = 0.0;
						double num24 = 0.0;
						parameter = m_ParkingLotCurbSiteCompList[num19].ProposedFamilySymbol.LookupParameter(RevitParameters.StreetCurbWidthParameter);
						if (parameter != null)
						{
							num24 = parameter.AsDouble();
						}
						parameter = m_ParkingLotCurbSiteCompList[num19].ProposedFamilySymbol.LookupParameter(RevitParameters.StreetGutterWidthParameter);
						if (parameter != null)
						{
							num23 = parameter.AsDouble();
						}
						num21 = num24;
						num22 = num23;
						Eps_Leg ppOffsetLegList3 = new Eps_Leg();
						RevitGeometry.OffsetLegList(eps_Leg, num21 + num22, 1, ref ppOffsetLegList3, TrimClosedObject: true);
						Eps_Leg ppOffsetLegList4 = new Eps_Leg();
						RevitGeometry.OffsetLegList(eps_Leg, num21 + num22, 2, ref ppOffsetLegList4, TrimClosedObject: true);
						if (RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList4) < RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList3))
						{
							eps_Leg = eps_Leg.Reverse();
						}
					}
					if (eps_Leg != null && eps_Leg.LegCount > 0)
					{
						if (!TopographySurfaceManagement.RebuildProposedTopographySurface(m_Document, sitePlacementSettings, m_dArcPointInterval, eps_Leg, _ParkingLotSurface, list15, bDrawForm, null, null, null, null, null, null, null, ref _ParkingLotTopoPointList))
						{
							return false;
						}
						eps_Leg = null;
					}
					sitePlacementSettings.LoftFormID = string.Empty;
					sitePlacementSettings.IslandSurfaceID = string.Empty;
					sitePlacementSettings.ParkingLotSurfaceID = string.Empty;
				}
				progressBarForm = new ProgressBarForm();
				progressBarForm.ShowForm();
				progressBarForm.Refresh();
				progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarUpdatingModelMessage");
				progressBarForm.MinValue = 0;
				progressBarForm.MaxValue = 12;
				PointXYZList.Clear();
				for (int num25 = 0; num25 < _ParkingLotTopoPointList.Count; num25++)
				{
					PointXYZList.Add(_ParkingLotTopoPointList[num25].XYZ);
				}
				progressBarForm.UpdateProgressBar(1);
				if (PointXYZList.Count > 0)
				{
					TopographySurfaceManagement.RemoveDuplicateXY(ref PointXYZList);
					TopographySurface topographySurface6 = TopographySurface.Create(m_Document, PointXYZList);
					m_Document.Regenerate();
					progressBarForm.UpdateProgressBar(7);
					parameter2 = ((Element)topographySurface6).get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
					if (parameter2 != null)
					{
						ElementId elementId = null;
						long _nReturnValue = -1;
						if (m_CurrentFamilyType == LandcaddID.Curb && ApplicationUtility.Settings.eplSetE_GetSetting(m_Document.PathName, "ParkingLotIslandToposurfaceMaterialParameter", ref _nReturnValue))
						{
							elementId = RevitIds.Create(_nReturnValue);
							bool flag6 = false;
							FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
							IList<Element> list16 = filteredElementCollector.OfClass(typeof(Material)).OfCategory(BuiltInCategory.OST_Materials).ToElements();
							foreach (Element item15 in list16)
							{
								if (item15.Id == elementId)
								{
									flag6 = true;
									break;
								}
							}
							if (!flag6)
							{
								_nReturnValue = -1;
							}
						}
						if (_nReturnValue == -1)
						{
							parameter = ((Element)_ParkingLotSurface.TopographySurface).get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
							if (parameter != null)
							{
								elementId = parameter.AsElementId();
								_nReturnValue = elementId.Value64();
							}
						}
						if (_nReturnValue != -1)
						{
							parameter2.Set(elementId);
						}
					}
					parameter = ((Element)_ParkingLotSurface.TopographySurface).get_Parameter(BuiltInParameter.PHASE_CREATED);
					if (parameter != null && parameter.AsElementId() != null && parameter.AsElementId() != ElementId.InvalidElementId)
					{
						parameter2 = ((Element)topographySurface6).get_Parameter(BuiltInParameter.PHASE_CREATED);
						if (parameter2 != null && !parameter2.IsReadOnly)
						{
							ElementId value4 = parameter.AsElementId();
							parameter2.Set(value4);
						}
					}
					parameter = ((Element)_ParkingLotSurface.TopographySurface).get_Parameter(BuiltInParameter.PHASE_DEMOLISHED);
					if (parameter != null && parameter.AsElementId() != null && parameter.AsElementId() != ElementId.InvalidElementId)
					{
						parameter2 = ((Element)topographySurface6).get_Parameter(BuiltInParameter.PHASE_DEMOLISHED);
						if (parameter2 != null && !parameter2.IsReadOnly)
						{
							ElementId value5 = parameter.AsElementId();
							parameter2.Set(value5);
						}
					}
					parameter = ((Element)_ParkingLotSurface.TopographySurface).get_Parameter(BuiltInParameter.ROOM_NAME);
					if (parameter != null && parameter.AsString() != null && parameter.AsString() != string.Empty)
					{
						parameter2 = ((Element)topographySurface6).get_Parameter(BuiltInParameter.ROOM_NAME);
						if (parameter2 != null && !parameter2.IsReadOnly)
						{
							parameter2.Set(parameter.AsString());
						}
					}
					elementSet = m_FamilyInstanceComponentWrapper.GetFamilyInstanceElementSet();
					if (elementSet != null && elementSet.Size > 0)
					{
						foreach (Element item16 in elementSet)
						{
							parameter = item16.LookupParameter(RevitParameters.ParkingLotSurfaceIDFamilyParameter);
							if (parameter != null && !parameter.IsReadOnly && string.Compare(parameter.AsString(), _ParkingLotSurface.TopographySurface.UniqueId, ignoreCase: true) == 0)
							{
								parameter.Set(topographySurface6.UniqueId);
							}
						}
					}
					m_Document.Delete(_ParkingLotSurface.TopographySurface.Id);
					progressBarForm.UpdateProgressBar(3);
				}
				progressBarForm.SetMaxValue();
				Thread.Sleep(300);
				progressBarForm.Close();
			}
		}
		return result;
	}

	private FamilySymbol SetInitialProposedFamilySymbol(FamilyInstanceComponentWrapper _FamilyInstanceComponentWrapper, ref bool _bFamilySymbolPresent)
	{
		FamilyInstance firstExistingFamilyInstance = _FamilyInstanceComponentWrapper.GetFirstExistingFamilyInstance();
		if (firstExistingFamilyInstance == null)
		{
			return null;
		}
		LandcaddID locateType = LandcaddID.SoftTerrain;
		Parameter parameter = firstExistingFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
		if (parameter != null && parameter.AsString() != string.Empty)
		{
			foreach (int value in Enum.GetValues(typeof(LandcaddID)))
			{
				if (string.Compare(parameter.AsString(), value.ToString("000"), ignoreCase: true) == 0)
				{
					locateType = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value);
					break;
				}
			}
		}
		FamilySymbol result = null;
		if (_FamilyInstanceComponentWrapper.ProposedFamilySymbol == null)
		{
			result = RevitUtility.FindFamilyTypeSymbol(m_Document, firstExistingFamilyInstance, locateType, ref _bFamilySymbolPresent);
		}
		return result;
	}
}
