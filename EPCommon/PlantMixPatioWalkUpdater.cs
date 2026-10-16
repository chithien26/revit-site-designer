using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.Exceptions;
using SITES.Utilities;

namespace EPCommon;

public class PlantMixPatioWalkUpdater : IUpdater
{
	private static AddInId m_ApplicationId = new AddInId(new Guid("da9d271e-f7a0-4291-8837-02947862c85e"));

	private static UpdaterId m_UpdaterId = new UpdaterId(m_ApplicationId, new Guid("456f44ff-00b1-46dc-ad2a-9eb52be3c270"));

	private List<KeyValuePair<string, Element>> m_RegionElementList;

	private int m_nDocumentsOpenCount;

	public PlantMixPatioWalkUpdater()
	{
		m_RegionElementList = new List<KeyValuePair<string, Element>>();
	}

	public void Execute(UpdaterData _Data)
	{
		Document document = _Data.GetDocument();
		bool flag = false;
		ElementId elementId = null;
		for (int num = m_RegionElementList.Count - 1; num >= 0; num--)
		{
			try
			{
				elementId = m_RegionElementList[num].Value.Id;
			}
			catch (Autodesk.Revit.Exceptions.InvalidOperationException)
			{
				m_RegionElementList.RemoveAt(num);
			}
		}
		Parameter parameter = null;
		Parameter parameter2 = null;
		FilteredElementCollector filteredElementCollector = null;
		List<Element> list = null;
		foreach (ElementId addedElementId in _Data.GetAddedElementIds())
		{
			if (list == null)
			{
				list = new List<Element>();
				filteredElementCollector = new FilteredElementCollector(document);
				IList<Element> list2 = filteredElementCollector.OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).ToElements();
				foreach (Element item in list2)
				{
					if (item != null && string.Compare(item.Name, EPShared.LocalRM.GetString("SubRegionElement"), ignoreCase: true) == 0 && !m_RegionElementList.Contains(new KeyValuePair<string, Element>(item.UniqueId, item)))
					{
						list.Add(item);
					}
				}
				filteredElementCollector = new FilteredElementCollector(document);
				list2 = filteredElementCollector.WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_DetailComponents)).ToElements();
				foreach (Element item2 in list2)
				{
					if (item2 != null && string.Compare(item2.Name, EPShared.LocalRM.GetString("DetailFilledRegionElementName"), ignoreCase: true) == 0 && !m_RegionElementList.Contains(new KeyValuePair<string, Element>(item2.UniqueId, item2)))
					{
						list.Add(item2);
					}
				}
			}
			Element element = document.GetElement(addedElementId);
			if (element is FamilyInstance { Category: not null } familyInstance && familyInstance.Category.Id.Value64() == BuiltInCategory.OST_GenericModel.GetHashCode())
			{
				parameter = familyInstance.Symbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter);
				if (parameter == null)
				{
					continue;
				}
				if (string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.PlantMix).ToString("000"), ignoreCase: true) == 0 || string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.PatioWalk).ToString("000"), ignoreCase: true) == 0)
				{
					parameter2 = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
					if (parameter2 != null)
					{
						for (int i = 0; i < list.Count; i++)
						{
							string uniqueId = list[i].UniqueId;
							if (string.Compare(parameter2.AsString(), uniqueId, ignoreCase: true) == 0)
							{
								m_RegionElementList.Add(new KeyValuePair<string, Element>(list[i].UniqueId, list[i]));
								break;
							}
						}
					}
				}
			}
			flag = true;
		}
		if (flag)
		{
			return;
		}
		List<string> list3 = null;
		foreach (ElementId deletedElementId in _Data.GetDeletedElementIds())
		{
			for (int j = 0; j < m_RegionElementList.Count; j++)
			{
				if (m_RegionElementList[j].Value.Id.Value64() == deletedElementId.Value64())
				{
					if (list3 == null)
					{
						list3 = new List<string>();
					}
					list3.Add(m_RegionElementList[j].Key);
					m_RegionElementList.RemoveAt(j);
					break;
				}
			}
		}
		if (list3 == null || list3.Count == 0)
		{
			return;
		}
		List<string> list4 = new List<string>();
		filteredElementCollector = new FilteredElementCollector(document);
		IList<Element> list5 = filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
		foreach (Element item3 in list5)
		{
			if (!(item3 is FamilyInstance { Category: not null } familyInstance2) || familyInstance2.Category.Id.Value64() != BuiltInCategory.OST_GenericModel.GetHashCode())
			{
				continue;
			}
			parameter = familyInstance2.Symbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter);
			if (parameter == null || (string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.PlantMix).ToString("000"), ignoreCase: true) != 0 && string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.PatioWalk).ToString("000"), ignoreCase: true) != 0))
			{
				continue;
			}
			parameter2 = familyInstance2.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
			if (parameter2 == null)
			{
				continue;
			}
			for (int k = 0; k < list3.Count; k++)
			{
				string strB = list3[k];
				if (string.Compare(parameter2.AsString(), strB, ignoreCase: true) == 0)
				{
					document.Delete(familyInstance2.Id);
					break;
				}
			}
		}
	}

	public void AddElements(Document _CurrentDocument)
	{
		m_nDocumentsOpenCount++;
		List<string> list = new List<string>();
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_CurrentDocument);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_GenericModel);
		Element element = filteredElementCollector.FirstElement();
		if (element == null)
		{
			return;
		}
		Parameter LandcaddIDParameter = null;
		FamilyInstance FindFamilyInstance = null;
		List<Element> list2 = null;
		try
		{
			IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (LandcaddIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter)) != null && (string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.PlantMix).ToString("000"), ignoreCase: true) == 0 || string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.PatioWalk).ToString("000"), ignoreCase: true) == 0) && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
			list2 = source.ToList();
		}
		catch (InternalException)
		{
			return;
		}
		catch (Autodesk.Revit.Exceptions.ApplicationException)
		{
			return;
		}
		LandcaddIDParameter = null;
		Parameter parameter = null;
		bool flag = false;
		filteredElementCollector = new FilteredElementCollector(_CurrentDocument);
		IList<Element> list3 = filteredElementCollector.OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).ToElements();
		filteredElementCollector = new FilteredElementCollector(_CurrentDocument);
		IList<Element> list4 = filteredElementCollector.WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_DetailComponents)).ToElements();
		foreach (Element item in list2)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_GenericModel.GetHashCode())
			{
				continue;
			}
			LandcaddIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter);
			if (LandcaddIDParameter == null || (string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.PlantMix).ToString("000"), ignoreCase: true) != 0 && string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.PatioWalk).ToString("000"), ignoreCase: true) != 0))
			{
				continue;
			}
			parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
			if (parameter == null)
			{
				continue;
			}
			for (int num = 0; num < list3.Count; num++)
			{
				Element element2 = list3[num];
				if (element2 != null && string.Compare(element2.Name, EPShared.LocalRM.GetString("SubRegionElement"), ignoreCase: true) == 0 && string.Compare(parameter.AsString(), element2.UniqueId, ignoreCase: true) == 0)
				{
					m_RegionElementList.Add(new KeyValuePair<string, Element>(element2.UniqueId, element2));
					list3.RemoveAt(num);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int num2 = 0; num2 < list4.Count; num2++)
				{
					Element element3 = list4[num2];
					if (element3 != null && string.Compare(element3.Name, EPShared.LocalRM.GetString("DetailFilledRegionElementName"), ignoreCase: true) == 0 && string.Compare(parameter.AsString(), element3.UniqueId, ignoreCase: true) == 0)
					{
						m_RegionElementList.Add(new KeyValuePair<string, Element>(element3.UniqueId, element3));
						list4.RemoveAt(num2);
						break;
					}
				}
			}
			else
			{
				flag = false;
			}
		}
	}

	public void RemoveElements(Document _CurrentDocument)
	{
		m_nDocumentsOpenCount--;
		ElementId elementId = null;
		for (int num = m_RegionElementList.Count - 1; num >= 0; num--)
		{
			try
			{
				elementId = m_RegionElementList[num].Value.Id;
				if (RevitUtility.CompareFileNames(m_RegionElementList[num].Value.Document.PathName, _CurrentDocument.PathName) == 0 && RevitUtility.CompareFileNames(m_RegionElementList[num].Value.Document.Title, _CurrentDocument.Title) == 0)
				{
					m_RegionElementList.RemoveAt(num);
				}
			}
			catch (Autodesk.Revit.Exceptions.InvalidOperationException)
			{
				m_RegionElementList.RemoveAt(num);
			}
		}
	}

	public string GetAdditionalInformation()
	{
		return "This will delete all Plant Mix and Patio/Walk Generic Models that no longer have corresponding Region/Sub-region elements.";
	}

	public ChangePriority GetChangePriority()
	{
		return ChangePriority.DetailComponents;
	}

	public UpdaterId GetUpdaterId()
	{
		return m_UpdaterId;
	}

	public string GetUpdaterName()
	{
		return "Plant Mix Patio/Walk Updater";
	}

	public int DocumentsOpenCount()
	{
		return m_nDocumentsOpenCount;
	}
}
