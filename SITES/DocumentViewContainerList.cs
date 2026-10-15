using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

namespace SITES;

public class DocumentViewContainerList
{
	private List<DocumentViewContainer> m_DocumentViewContainerList;

	public List<DocumentViewContainer> DocumentViewList => m_DocumentViewContainerList;

	public DocumentViewContainerList()
	{
		m_DocumentViewContainerList = new List<DocumentViewContainer>();
	}

	public int FindIndexDocument(Document _FindDocument)
	{
		if (m_DocumentViewContainerList != null && m_DocumentViewContainerList.Count > 0)
		{
			for (int i = 0; i < m_DocumentViewContainerList.Count; i++)
			{
				if (m_DocumentViewContainerList[i].Document.Equals(_FindDocument))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public bool ContainsView(int _ContainerIndex, View _FindView)
	{
		if (m_DocumentViewContainerList != null && _ContainerIndex >= 0 && _ContainerIndex < m_DocumentViewContainerList.Count)
		{
			for (int num = m_DocumentViewContainerList[_ContainerIndex].ViewList.Count - 1; num >= 0; num--)
			{
				try
				{
					if (string.Compare(m_DocumentViewContainerList[_ContainerIndex].ViewList[num].Name, _FindView.Name, ignoreCase: true) == 0)
					{
						return true;
					}
				}
				catch (InvalidObjectException)
				{
					m_DocumentViewContainerList[_ContainerIndex].ViewList.RemoveAt(num);
				}
			}
		}
		return false;
	}
}
