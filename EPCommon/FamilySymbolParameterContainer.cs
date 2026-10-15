using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilySymbolParameterContainer
{
	private string m_strParameterName;

	private ExternalDefinition m_FamilyDefinition;

	private BuiltInParameterGroup m_BuiltInParameterGroup;

	private ParameterType m_ParameterType;

	private Category m_FamilyCategory;

	private bool m_bIsInstance;

	public string ParameterName
	{
		get
		{
			return m_strParameterName;
		}
		set
		{
			m_strParameterName = value;
		}
	}

	public BuiltInParameterGroup BuiltInParameterGroup
	{
		get
		{
			return m_BuiltInParameterGroup;
		}
		set
		{
			m_BuiltInParameterGroup = value;
		}
	}

	public ParameterType ParameterType
	{
		get
		{
			return m_ParameterType;
		}
		set
		{
			m_ParameterType = value;
		}
	}

	public bool IsInstance
	{
		get
		{
			return m_bIsInstance;
		}
		set
		{
			m_bIsInstance = value;
		}
	}

	public ExternalDefinition FamilyDefinition
	{
		get
		{
			return m_FamilyDefinition;
		}
		set
		{
			m_FamilyDefinition = value;
		}
	}

	public Category FamilyCategory
	{
		get
		{
			return m_FamilyCategory;
		}
		set
		{
			m_FamilyCategory = value;
		}
	}

	public FamilySymbolParameterContainer(string _strParameterName, BuiltInParameterGroup _BuiltInParameterGroup, ParameterType _ParameterType, bool _bIsInstance)
	{
		m_strParameterName = _strParameterName;
		m_BuiltInParameterGroup = _BuiltInParameterGroup;
		m_ParameterType = _ParameterType;
		m_bIsInstance = _bIsInstance;
		m_FamilyDefinition = null;
		m_FamilyCategory = null;
	}

	public FamilySymbolParameterContainer(string _strParameterName, BuiltInParameterGroup _BuiltInParameterGroup, Category _FamilyCategory, bool _bIsInstance)
	{
		m_strParameterName = _strParameterName;
		m_BuiltInParameterGroup = _BuiltInParameterGroup;
		m_ParameterType = ParameterType.Invalid;
		m_bIsInstance = _bIsInstance;
		m_FamilyDefinition = null;
		m_FamilyCategory = _FamilyCategory;
	}

	public FamilySymbolParameterContainer(ExternalDefinition _FamilyDefinition, BuiltInParameterGroup _BuiltInParameterGroup, bool _bIsInstance)
	{
		m_strParameterName = string.Empty;
		m_BuiltInParameterGroup = _BuiltInParameterGroup;
		m_ParameterType = ParameterType.Invalid;
		m_bIsInstance = _bIsInstance;
		m_FamilyDefinition = _FamilyDefinition;
		m_FamilyCategory = null;
	}
}
