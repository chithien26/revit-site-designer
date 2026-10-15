namespace EPCommon;

public class OffsetLegWrapper
{
	private bool m_bIsLeft;

	private double m_dEndingStation;

	private double m_dStation;

	private double m_dAdditionalOffsetWidth;

	private Eps_Leg m_OffsetLeg;

	private Eps_Leg m_OriginalLeg;

	public bool IsLeft
	{
		get
		{
			return m_bIsLeft;
		}
		set
		{
			m_bIsLeft = value;
		}
	}

	public double EndingStation
	{
		get
		{
			return m_dEndingStation;
		}
		set
		{
			m_dEndingStation = value;
		}
	}

	public double Station
	{
		get
		{
			return m_dStation;
		}
		set
		{
			m_dStation = value;
		}
	}

	public double AdditionalOffsetWidth
	{
		get
		{
			return m_dAdditionalOffsetWidth;
		}
		set
		{
			m_dAdditionalOffsetWidth = value;
		}
	}

	public Eps_Leg OffsetLeg
	{
		get
		{
			return m_OffsetLeg;
		}
		set
		{
			m_OffsetLeg = value;
		}
	}

	public Eps_Leg OriginalLeg
	{
		get
		{
			return m_OriginalLeg;
		}
		set
		{
			m_OriginalLeg = value;
		}
	}

	public OffsetLegWrapper()
	{
		m_bIsLeft = false;
		m_dStation = 0.0;
		m_dEndingStation = 0.0;
		m_dAdditionalOffsetWidth = 0.0;
		m_OffsetLeg = null;
		m_OriginalLeg = null;
	}
}
