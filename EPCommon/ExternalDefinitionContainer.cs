using Autodesk.Revit.DB;

namespace EPCommon;

public class ExternalDefinitionContainer
{
	private string m_strName;

	private ExternalDefinition m_ExternalDefinition;

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

	public ExternalDefinition ExternalDefinition
	{
		get
		{
			return m_ExternalDefinition;
		}
		set
		{
			m_ExternalDefinition = value;
			if (m_ExternalDefinition != null)
			{
				m_strName = m_ExternalDefinition.Name;
			}
		}
	}

	public ExternalDefinitionContainer(string _strName)
	{
		m_strName = _strName;
	}

	public ExternalDefinitionContainer(ExternalDefinition _ExternalDefinition)
	{
		m_ExternalDefinition = _ExternalDefinition;
		if (m_ExternalDefinition != null)
		{
			m_strName = m_ExternalDefinition.Name;
		}
	}
}
