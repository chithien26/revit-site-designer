using System.Collections;

namespace EPCommon;

public class XmlElementInfo
{
	private string m_strName;

	private bool m_bComment;

	private ArrayList m_Attributes;

	private ArrayList m_Children;

	public string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			m_strName = value;
		}
	}

	public bool IsComment
	{
		get
		{
			return m_bComment;
		}
		set
		{
			m_bComment = value;
		}
	}

	public ArrayList Attributes => m_Attributes;

	public ArrayList Children => m_Children;

	public int ElementCount
	{
		get
		{
			if (m_Children != null)
			{
				return m_Children.Count;
			}
			return 0;
		}
	}

	public XmlElementInfo(string _strName)
	{
		m_bComment = false;
		m_Attributes = null;
		m_Children = null;
		m_strName = _strName;
	}

	public XmlElementInfo()
	{
		m_bComment = false;
		m_Attributes = null;
		m_Children = null;
	}

	public void AddAttribute(XmlAttributeInfo _Attribute)
	{
		if (m_Attributes == null)
		{
			m_Attributes = new ArrayList();
		}
		m_Attributes.Add(_Attribute);
	}

	public void AddElement(XmlElementInfo _Element)
	{
		if (m_Children == null)
		{
			m_Children = new ArrayList();
		}
		m_Children.Add(_Element);
	}

	public XmlElementInfo GetElement(int _nElement)
	{
		if (m_Children != null)
		{
			return (XmlElementInfo)m_Children[_nElement];
		}
		return null;
	}

	public string GetAttribute(string _strAttribute)
	{
		return GetAttributeInfo(_strAttribute)?.Value;
	}

	private XmlAttributeInfo GetAttributeInfo(string _strAttribute)
	{
		if (m_Attributes != null)
		{
			for (int i = 0; i < m_Attributes.Count; i++)
			{
				if (string.Compare(((XmlAttributeInfo)m_Attributes[i]).Name, _strAttribute, ignoreCase: true) == 0)
				{
					return (XmlAttributeInfo)m_Attributes[i];
				}
			}
		}
		return null;
	}
}
