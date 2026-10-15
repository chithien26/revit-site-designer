namespace EPCommon;

public class ParameterColumn
{
	private PlantField m_PlantField;

	private int m_nOrder;

	public PlantField PlantField
	{
		get
		{
			return m_PlantField;
		}
		set
		{
			m_PlantField = value;
		}
	}

	public int Order
	{
		get
		{
			return m_nOrder;
		}
		set
		{
			m_nOrder = value;
		}
	}

	public ParameterColumn(PlantField _PlantField, int _nOrder)
	{
		m_PlantField = _PlantField;
		m_nOrder = _nOrder;
	}
}
