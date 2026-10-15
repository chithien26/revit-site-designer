namespace EPCommon;

public class SearchPlantParameter
{
	private string m_strParameter;

	private PlantFieldsComparisionType m_ComparisionType;

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

	public PlantFieldsComparisionType ComparisionType
	{
		get
		{
			return m_ComparisionType;
		}
		set
		{
			m_ComparisionType = value;
		}
	}

	public SearchPlantParameter(PlantFieldsComparisionType _ComparisionType, string _strParameter)
	{
		m_strParameter = _strParameter;
		m_ComparisionType = _ComparisionType;
	}
}
