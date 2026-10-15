using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SITES.Utilities;

namespace EPCommon;

public class Annotation
{
	private Document m_Document;

	private EPTransactionContainer m_TransactionContainer;

	private List<FamilyInstanceComponentWrapper> m_AllSiteComponentList;

	private FamilyInstance m_TopoPlacementFamilyInstance;

	private ElementSet m_LineElementSet;

	private AnnotateForm m_DisplayForm;

	private TopographySurfaceContainer m_BaseSurface;

	private TopographySurfaceContainer m_ProposedSurface;

	public Document Document => m_Document;

	public ReadOnlyCollection<TextNoteType> TextTypeList
	{
		get
		{
			List<TextNoteType> list = new List<TextNoteType>();
			FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
			ICollection<Element> collection = filteredElementCollector.OfClass(typeof(TextNoteType)).ToList();
			foreach (TextNoteType item in collection)
			{
				list.Add(item);
			}
			return new ReadOnlyCollection<TextNoteType>(list);
		}
	}

	public Annotation(Document _CurDocument)
	{
		m_Document = _CurDocument;
		m_TransactionContainer = new EPTransactionContainer(m_Document);
		m_AllSiteComponentList = new List<FamilyInstanceComponentWrapper>();
		m_LineElementSet = new ElementSet();
		m_TopoPlacementFamilyInstance = null;
		m_DisplayForm = null;
		m_BaseSurface = null;
		m_ProposedSurface = null;
		InitializeData();
	}

	private void InitializeData()
	{
		FamilyInstance _BasePlacementFamilyInstance = null;
		TopographySurfaceManagement.FindBaseLandcaddTopographySurface(m_Document, ref m_BaseSurface, ref _BasePlacementFamilyInstance, ref m_ProposedSurface, ref m_TopoPlacementFamilyInstance);
		if (m_BaseSurface == null || m_ProposedSurface == null)
		{
			return;
		}
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter LandcaddIDParameter = null;
		Parameter LocateTypeParameter = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (LandcaddIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0 && (LocateTypeParameter = FindFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter)) != null && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		filteredElementCollector = new FilteredElementCollector(m_Document);
		IList<Element> list2 = filteredElementCollector.OfClass(typeof(CurveElement)).OfCategory(BuiltInCategory.OST_Lines).ToElements();
		Parameter parameter = null;
		Parameter parameter2 = null;
		Parameter parameter3 = null;
		bool flag = false;
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
			{
				continue;
			}
			LandcaddIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (LandcaddIDParameter == null)
			{
				continue;
			}
			LocateTypeParameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (LocateTypeParameter == null || string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) != 0)
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
				for (int num2 = 0; num2 < m_AllSiteComponentList.Count; num2++)
				{
					if (string.Compare(parameter2.AsString(), m_AllSiteComponentList[num2].ChainID, ignoreCase: true) == 0)
					{
						flag = true;
						m_AllSiteComponentList[num2].FamilyContainerList.Add(new FamilyInstanceComponentContainer(element, familyInstance));
						break;
					}
				}
				if (!flag)
				{
					List<FamilyInstanceComponentContainer> list3 = new List<FamilyInstanceComponentContainer>();
					list3.Add(new FamilyInstanceComponentContainer(element, familyInstance));
					int nDrawOrder = 0;
					parameter3 = familyInstance.LookupParameter(RevitParameters.TopoDrawOrderFamilyParameter);
					if (parameter3 != null)
					{
						nDrawOrder = parameter3.AsInteger();
					}
					list3 = new List<FamilyInstanceComponentContainer>();
					list3.Add(new FamilyInstanceComponentContainer(element, familyInstance));
					m_AllSiteComponentList.Add(new FamilyInstanceComponentWrapper(parameter2.AsString(), list3, nDrawOrder));
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
		for (int num3 = 0; num3 < list2.Count; num3++)
		{
			Element element2 = list2[num3];
			if (element2 != null && element2.Category != null && element2.Category.Id.Value64() == BuiltInCategory.OST_Lines.GetHashCode() && !element2.IsHidden(m_Document.ActiveView))
			{
				m_LineElementSet.Insert(element2);
			}
		}
	}

	public void Run()
	{
		m_TransactionContainer.Start(EPShared.LocalRM.GetString("AnnotateSiteComponentTransaction"));
		if (m_BaseSurface == null || m_ProposedSurface == null)
		{
			if (RevitUtility.RemoveAllSiteComponents(m_Document, m_TransactionContainer))
			{
				m_TransactionContainer.Commit();
			}
			else
			{
				m_TransactionContainer.Rollback();
			}
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NoToposurfaceCanBeModifiedMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
		}
		else if (DisplayUI())
		{
			if (PromptPick())
			{
				if (m_LineElementSet != null && m_LineElementSet.Size > 0)
				{
					m_Document.ActiveView.UnhideElements(ElementSetConverter.ToIDList(m_LineElementSet));
				}
				HideSiteAlignmentLines();
				m_TransactionContainer.Commit();
			}
			else
			{
				m_TransactionContainer.Rollback();
			}
		}
		else
		{
			m_TransactionContainer.Rollback();
		}
	}

	private bool DisplayUI()
	{
		bool result = false;
		using (m_DisplayForm = new AnnotateForm(this))
		{
			if (m_DisplayForm.ShowDialog() == DialogResult.OK)
			{
				result = true;
			}
		}
		return result;
	}

	private bool PromptPick()
	{
		if (m_LineElementSet != null && m_LineElementSet.Size > 0)
		{
			m_TransactionContainer.BeginTransaction();
			m_Document.ActiveView.HideElements(ElementSetConverter.ToIDList(m_LineElementSet));
			m_TransactionContainer.EndTransaction();
		}
		if (m_AllSiteComponentList != null && m_AllSiteComponentList.Count > 0)
		{
			DisplaySiteAlignmentLines();
			m_Document.Regenerate();
			if (!PickSiteAlignmentElement())
			{
				return false;
			}
			m_Document.Regenerate();
			return true;
		}
		RevitMessageBox.Show(EPShared.LocalRM.GetString("RunLocateSiteComponentsMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
		return false;
	}

	private void DisplaySiteAlignmentLines()
	{
		if (m_AllSiteComponentList != null)
		{
			for (int i = 0; i < m_AllSiteComponentList.Count; i++)
			{
				m_TransactionContainer.BeginTransaction();
				m_Document.ActiveView.UnhideElements(ElementSetConverter.ToIDList(m_AllSiteComponentList[i].GetOriginalElementSet()));
				m_TransactionContainer.EndTransaction();
			}
		}
	}

	private void HideSiteAlignmentLines()
	{
		if (m_AllSiteComponentList != null)
		{
			for (int i = 0; i < m_AllSiteComponentList.Count; i++)
			{
				m_TransactionContainer.BeginTransaction();
				m_Document.ActiveView.HideElements(ElementSetConverter.ToIDList(m_AllSiteComponentList[i].GetOriginalElementSet()));
				m_TransactionContainer.EndTransaction();
			}
		}
	}

	private bool PickSiteAlignmentElement()
	{
		ElementSet elementSet = null;
		bool flag = false;
		ElementSet _Elements = null;
		string strPrompt = EPShared.LocalRM.GetString("PickObjectMessage");
		if (!RevitUtility.GetElementsFromProject(m_Document, strPrompt, ref _Elements))
		{
			RemoveSiteAlignmentLines(string.Empty);
			return false;
		}
		foreach (Element item in _Elements)
		{
			for (int i = 0; i < m_AllSiteComponentList.Count; i++)
			{
				if (m_AllSiteComponentList[i].IndexOfOriginalElementID(item.Id) < 0)
				{
					continue;
				}
				flag = true;
				TopographySurfaceContainer topographySurfaceContainer = null;
				LandcaddID landcaddID = LandcaddID.SoftTerrain;
				elementSet = m_AllSiteComponentList[i].GetFamilyInstanceElementSet();
				if (elementSet != null && elementSet.Size > 0)
				{
					string strFormUniqueId = string.Empty;
					foreach (Element item2 in elementSet)
					{
						Parameter parameter = item2.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
						if (parameter != null)
						{
							foreach (int value in Enum.GetValues(typeof(LandcaddID)))
							{
								if (string.Compare(parameter.AsString(), value.ToString("000"), ignoreCase: true) == 0)
								{
									landcaddID = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value);
									break;
								}
							}
						}
						if (landcaddID != LandcaddID.ParkingLot)
						{
							break;
						}
						parameter = item2.LookupParameter(RevitParameters.ParkingLotSurfaceIDFamilyParameter);
						if (parameter != null)
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
				m_AllSiteComponentList[i].ParkingLotTopographySurface = topographySurfaceContainer;
				if (topographySurfaceContainer == null)
				{
					topographySurfaceContainer = m_ProposedSurface;
				}
				m_AllSiteComponentList[i].CreateProposedLeg(topographySurfaceContainer);
				if (m_AllSiteComponentList[i].ProposedLeg != null)
				{
					ElementSet _ElevationText = new ElementSet();
					ElementSet _GradeText = new ElementSet();
					RevitUtility.AnnotateGradeAndElevationOnLegList(m_Document, m_TransactionContainer, m_AllSiteComponentList[i], m_DisplayForm.AnnotateElevations, m_DisplayForm.AnnotateGrades, _bUseOriginalLeg: false, m_DisplayForm.TextNoteType, ref _ElevationText, ref _GradeText, _bCheckProjectSharedCoordinatesSetting: true);
				}
				break;
			}
			if (!flag)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NotSiteComponentHostLineMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				RemoveSiteAlignmentLines(string.Empty);
				return false;
			}
		}
		return true;
	}

	private void RemoveSiteAlignmentLines(string _strIgnoreSiteID)
	{
		if (m_AllSiteComponentList == null)
		{
			return;
		}
		for (int i = 0; i < m_AllSiteComponentList.Count; i++)
		{
			if (string.Compare(m_AllSiteComponentList[i].ChainID, _strIgnoreSiteID, ignoreCase: true) != 0 && m_AllSiteComponentList[i].ProposedLeg != null && m_AllSiteComponentList[i].ProposedElementSet != null)
			{
				m_TransactionContainer.BeginTransaction();
				m_Document.Delete(ElementSetConverter.ToIDList(m_AllSiteComponentList[i].ProposedElementSet));
				m_TransactionContainer.EndTransaction();
				m_AllSiteComponentList[i].ProposedLeg = null;
				m_AllSiteComponentList[i].ProposedElementSet = null;
			}
		}
	}
}
