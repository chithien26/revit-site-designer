using System.Collections.Generic;
using Autodesk.Revit.DB;
using SITES.Utilities;

namespace EPCommon;

public class GenericModelFamilyInstanceWrapper
{
	private string m_strChainID;

	private List<GenericModelFamilyInstanceContainer> m_FamilyContainerList;

	private List<GenericModelFamilyInstanceContainer> m_IslandFamilyInstanceList;

	private List<FamilyInstanceAssignmentContainer> m_FamilyAssignmentList;

	public string ChainID => m_strChainID;

	public List<GenericModelFamilyInstanceContainer> FamilyContainerList => m_FamilyContainerList;

	public List<GenericModelFamilyInstanceContainer> IslandFamilyInstanceList
	{
		get
		{
			return m_IslandFamilyInstanceList;
		}
		set
		{
			m_IslandFamilyInstanceList = value;
		}
	}

	public List<FamilyInstanceAssignmentContainer> FamilyAssignmentList
	{
		get
		{
			return m_FamilyAssignmentList;
		}
		set
		{
			m_FamilyAssignmentList = value;
		}
	}

	public GenericModelFamilyInstanceWrapper(string _strChainID, List<GenericModelFamilyInstanceContainer> _PlantContainerList)
	{
		m_strChainID = _strChainID;
		m_FamilyContainerList = _PlantContainerList;
		m_IslandFamilyInstanceList = null;
		m_FamilyAssignmentList = null;
	}

	public int IndexOfFamilyInstanceUniqueID(Document _CurrentDocument, string _strUniqueID)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			FamilyInstance familyInstance = null;
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				familyInstance = m_FamilyContainerList[i].GetFamilyInstance(_CurrentDocument);
				if (familyInstance != null && string.Compare(familyInstance.UniqueId, _strUniqueID, ignoreCase: true) == 0)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public int IndexOfContainerID(Document _CurrentDocument, ElementId _ContainerID)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			FamilyInstance familyInstance = null;
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				familyInstance = m_FamilyContainerList[i].GetFamilyInstance(_CurrentDocument);
				if (familyInstance != null && m_FamilyContainerList[i].ID == _ContainerID)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public int IndexOfIslandContainerID(Document _CurrentDocument, ElementId _ContainerID)
	{
		if (m_IslandFamilyInstanceList != null && m_IslandFamilyInstanceList.Count > 0)
		{
			FamilyInstance familyInstance = null;
			for (int i = 0; i < m_IslandFamilyInstanceList.Count; i++)
			{
				familyInstance = m_IslandFamilyInstanceList[i].GetFamilyInstance(_CurrentDocument);
				if (familyInstance != null && m_IslandFamilyInstanceList[i].ID == _ContainerID)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public int IndexOfFamilyInstanceID(Document _CurrentDocument, ElementId _ElementId)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				if (m_FamilyContainerList[i].FamilyInstanceID.Value64() == _ElementId.Value64())
				{
					return i;
				}
			}
		}
		return -1;
	}

	public FamilyInstance GetFirstExistingFamilyInstance(Document _CurrentDocument)
	{
		ElementId findElementId = RevitIds.Create(-1);
		return GetFirstExistingFamilyInstance(_CurrentDocument, findElementId);
	}

	public FamilyInstance GetFirstExistingFamilyInstance(Document _CurrentDocument, ElementId _FindElementId)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			FamilyInstance familyInstance = null;
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				if (_FindElementId.Value64() == -1 || _FindElementId.Value64() == m_FamilyContainerList[i].ID.Value64())
				{
					familyInstance = m_FamilyContainerList[i].GetFamilyInstance(_CurrentDocument);
					if (familyInstance != null)
					{
						return familyInstance;
					}
				}
			}
		}
		return null;
	}

	public Element GetFirstExistingElement(Document _CurrentDocument)
	{
		ElementId findElementId = RevitIds.Create(-1);
		return GetFirstExistingElement(_CurrentDocument, findElementId);
	}

	public Element GetFirstExistingElement(Document _CurrentDocument, ElementId _FindElementId)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			Element element = null;
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				if (_FindElementId.Value64() == -1 || _FindElementId.Value64() == m_FamilyContainerList[i].ID.Value64())
				{
					element = _CurrentDocument.GetElement(m_FamilyContainerList[i].ID);
					if (element != null)
					{
						return element;
					}
				}
			}
		}
		return null;
	}

	public int IndexOfFamilyAssignmentInstanceID(Document _CurrentDocument, ElementId _ElementId)
	{
		if (m_FamilyAssignmentList != null && m_FamilyAssignmentList.Count > 0)
		{
			for (int i = 0; i < m_FamilyAssignmentList.Count; i++)
			{
				if (m_FamilyAssignmentList[i].FamilyInstanceID.Value64() == _ElementId.Value64())
				{
					return i;
				}
			}
		}
		return -1;
	}

	public List<KeyValuePair<string, FamilyInstance>> GetFamilyAssignmentInstanceList(Document _CurrentDocument)
	{
		return GetFamilyAssignmentInstanceList(_CurrentDocument, null);
	}

	public List<KeyValuePair<string, FamilyInstance>> GetFamilyAssignmentInstanceList(Document _CurrentDocument, FamilyInstance _PlacementFamilyInstance)
	{
		List<KeyValuePair<string, FamilyInstance>> list = null;
		if (m_FamilyAssignmentList != null && m_FamilyAssignmentList.Count > 0)
		{
			FamilyInstance familyInstance = null;
			Parameter parameter = null;
			for (int i = 0; i < m_FamilyAssignmentList.Count; i++)
			{
				familyInstance = m_FamilyAssignmentList[i].GetFamilyInstance(_CurrentDocument);
				if (familyInstance == null)
				{
					continue;
				}
				parameter = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
				if (parameter != null && (_PlacementFamilyInstance == null || string.Compare(parameter.AsString(), _PlacementFamilyInstance.UniqueId, ignoreCase: true) == 0))
				{
					if (list == null)
					{
						list = new List<KeyValuePair<string, FamilyInstance>>();
					}
					list.Add(new KeyValuePair<string, FamilyInstance>(parameter.AsString(), familyInstance));
				}
			}
		}
		return list;
	}

	public bool GetFamilyAssignmentDeletedList(Document _CurrentDocument, ref List<ElementId> _DeletedElementsList, ref int _nFamilyDeletedCount)
	{
		bool flag = false;
		bool flag2 = true;
		_nFamilyDeletedCount = 0;
		if (m_FamilyAssignmentList != null && m_FamilyAssignmentList.Count > 0)
		{
			FamilyInstance familyInstance = null;
			for (int i = 0; i < m_FamilyAssignmentList.Count; i++)
			{
				for (int j = 0; j < _DeletedElementsList.Count; j++)
				{
					if (_DeletedElementsList[j] == m_FamilyAssignmentList[i].ID)
					{
						_nFamilyDeletedCount++;
						_DeletedElementsList.RemoveAt(j);
						flag = true;
						break;
					}
				}
				if (!flag && flag2)
				{
					familyInstance = m_FamilyAssignmentList[i].GetFamilyInstance(_CurrentDocument);
					if (familyInstance != null)
					{
						flag2 = false;
					}
				}
				else
				{
					flag = false;
				}
			}
		}
		return flag2;
	}
}
