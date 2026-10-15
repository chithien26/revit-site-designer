namespace EPCommon;

public class XmlAttributeInfo
{
	private string m_strName;

	private string m_strValue;

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

	public string Value
	{
		get
		{
			return m_strValue;
		}
		set
		{
			m_strValue = value;
		}
	}

	public XmlAttributeInfo(string _strName, string _strValue)
	{
		m_strName = _strName;
		m_strValue = _strValue;
	}

	public XmlAttributeInfo()
	{
	}
}
