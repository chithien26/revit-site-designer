using System;

namespace EPCommon;

public class ProjectSettingContainer
{
	private string m_strAppName;

	private DateTime m_CurrentDateTime;

	public string AppName
	{
		get
		{
			return m_strAppName;
		}
		set
		{
			m_strAppName = value;
		}
	}

	public DateTime CurrentDateTime
	{
		get
		{
			return m_CurrentDateTime;
		}
		set
		{
			m_CurrentDateTime = value;
		}
	}

	public ProjectSettingContainer(string _strAppName, DateTime _CurrentDateTime)
	{
		m_strAppName = _strAppName;
		m_CurrentDateTime = _CurrentDateTime;
	}
}
