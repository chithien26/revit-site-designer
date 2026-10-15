namespace EPCommon;

public class IntersectionStation
{
	private bool m_bIsLeft;

	private bool m_bIsPrimaryLeft;

	private bool m_bStartingIntersection;

	private double m_dStation;

	private double m_dPrimaryStation;

	private double m_dRadius;

	private double m_dTransitioningDistance;

	private Eps_Leg m_IntersectionLeg;

	private Eps_Point m_InvalidIntersectionPnt;

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

	public bool IsPrimaryLeft
	{
		get
		{
			return m_bIsPrimaryLeft;
		}
		set
		{
			m_bIsPrimaryLeft = value;
		}
	}

	public double PrimaryStation
	{
		get
		{
			return m_dPrimaryStation;
		}
		set
		{
			m_dPrimaryStation = value;
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

	public double Radius
	{
		get
		{
			return m_dRadius;
		}
		set
		{
			m_dRadius = value;
		}
	}

	public double TransitioningDistance
	{
		get
		{
			return m_dTransitioningDistance;
		}
		set
		{
			m_dTransitioningDistance = value;
		}
	}

	public Eps_Leg IntersectionLeg
	{
		get
		{
			return m_IntersectionLeg;
		}
		set
		{
			m_IntersectionLeg = value;
		}
	}

	public bool StartingIntersection
	{
		get
		{
			return m_bStartingIntersection;
		}
		set
		{
			m_bStartingIntersection = value;
		}
	}

	public Eps_Point InvalidIntersectionPnt
	{
		get
		{
			return m_InvalidIntersectionPnt;
		}
		set
		{
			m_InvalidIntersectionPnt = value;
		}
	}

	public IntersectionStation()
	{
		m_bIsLeft = true;
		m_bIsPrimaryLeft = true;
		m_bStartingIntersection = true;
		m_dStation = 0.0;
		m_dPrimaryStation = 0.0;
		m_dRadius = 0.0;
		m_dTransitioningDistance = 0.0;
		m_IntersectionLeg = null;
		m_InvalidIntersectionPnt = null;
	}

	public IntersectionStation(double _dStation)
	{
		m_bIsLeft = true;
		m_bIsPrimaryLeft = true;
		m_bStartingIntersection = true;
		m_dStation = _dStation;
		m_dPrimaryStation = 0.0;
		m_dRadius = 0.0;
		m_dTransitioningDistance = 0.0;
		m_IntersectionLeg = null;
		m_InvalidIntersectionPnt = null;
	}
}
