using System.Collections.Generic;

namespace EPCommon;

public class IntersectionStationContainer
{
	private string m_strPrimaryPlacementId;

	private string m_strSecondaryPlacementId;

	private List<IntersectionStation> m_IntersectionStations;

	public List<IntersectionStation> IntersectionStations
	{
		get
		{
			return m_IntersectionStations;
		}
		set
		{
			m_IntersectionStations = value;
		}
	}

	public string PrimaryPlacementId
	{
		get
		{
			return m_strPrimaryPlacementId;
		}
		set
		{
			m_strPrimaryPlacementId = value;
		}
	}

	public string SecondaryPlacementId
	{
		get
		{
			return m_strSecondaryPlacementId;
		}
		set
		{
			m_strSecondaryPlacementId = value;
		}
	}

	public IntersectionStationContainer()
	{
		m_strPrimaryPlacementId = string.Empty;
		m_strSecondaryPlacementId = string.Empty;
		m_IntersectionStations = new List<IntersectionStation>();
	}

	public IntersectionStationContainer(double _dStation)
	{
		m_strPrimaryPlacementId = string.Empty;
		m_strSecondaryPlacementId = string.Empty;
		m_IntersectionStations = new List<IntersectionStation>();
		m_IntersectionStations.Add(new IntersectionStation(_dStation));
	}
}
