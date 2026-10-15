namespace EPCommon;

public class ExportReportLine
{
	private string m_strLevelName;

	private string m_strParameter;

	private string m_strValue;

	private bool m_bBlankLine;

	public string LevelName
	{
		get
		{
			return m_strLevelName;
		}
		set
		{
			m_strLevelName = value;
		}
	}

	public string Parameter
	{
		get
		{
			return m_strParameter;
		}
		set
		{
			m_strParameter = value;
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

	public bool BlankLine
	{
		get
		{
			return m_bBlankLine;
		}
		set
		{
			m_bBlankLine = value;
		}
	}

	public ExportReportLine(string _strLevelName, string _strParameter, string _strValue)
	{
		m_strLevelName = _strLevelName;
		m_strParameter = _strParameter;
		m_strValue = _strValue;
		m_bBlankLine = false;
	}

	public ExportReportLine()
	{
		m_bBlankLine = true;
	}
}
