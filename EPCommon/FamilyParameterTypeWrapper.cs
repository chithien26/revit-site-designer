using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilyParameterTypeWrapper
{
	private string m_parameterDisplayName;

	private string m_parameterName;

	private ParameterType m_parameterType;

	private BuiltInParameter m_builtInParameter;

	public string ParameterDisplayName
	{
		get
		{
			return m_parameterDisplayName;
		}
		set
		{
			m_parameterDisplayName = value;
		}
	}

	public string ParameterName
	{
		get
		{
			return m_parameterName;
		}
		set
		{
			m_parameterName = value;
		}
	}

	public ParameterType ParameterType
	{
		get
		{
			return m_parameterType;
		}
		set
		{
			m_parameterType = value;
		}
	}

	public BuiltInParameter BuiltInParameter
	{
		get
		{
			return m_builtInParameter;
		}
		set
		{
			m_builtInParameter = value;
		}
	}

	public FamilyParameterTypeWrapper(string parameterDisplayName, string parameterName, ParameterType parameterType)
	{
		m_parameterDisplayName = parameterDisplayName;
		m_parameterName = parameterName;
		m_parameterType = parameterType;
		m_builtInParameter = BuiltInParameter.INVALID;
	}

	public FamilyParameterTypeWrapper(string parameterDisplayName, string parameterName, ParameterType parameterType, BuiltInParameter builtInParameter)
	{
		m_parameterDisplayName = parameterDisplayName;
		m_parameterName = parameterName;
		m_parameterType = parameterType;
		m_builtInParameter = builtInParameter;
	}
}
