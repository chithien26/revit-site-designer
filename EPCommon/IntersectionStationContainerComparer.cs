using System.Collections.Generic;

namespace EPCommon;

public class IntersectionStationContainerComparer : IComparer<IntersectionStationContainer>
{
	private bool m_bDecendingValues;

	private bool m_bCompareFirstStations = true;

	public IntersectionStationContainerComparer(bool _bCompareFirstStations)
	{
		m_bCompareFirstStations = _bCompareFirstStations;
	}

	int IComparer<IntersectionStationContainer>.Compare(IntersectionStationContainer first, IntersectionStationContainer second)
	{
		int num = 0;
		if (first != null && first.IntersectionStations != null && first.IntersectionStations.Count > 0)
		{
			if (second == null || second.IntersectionStations == null || second.IntersectionStations.Count <= 0)
			{
				num = -1;
			}
			else
			{
				num = ((!m_bCompareFirstStations) ? first.IntersectionStations[first.IntersectionStations.Count - 1].Station.CompareTo(second.IntersectionStations[second.IntersectionStations.Count - 1].Station) : first.IntersectionStations[0].Station.CompareTo(second.IntersectionStations[0].Station));
			}
		}
		else if (second != null && second.IntersectionStations != null && second.IntersectionStations.Count > 0)
		{
			num = 1;
		}
		if (m_bDecendingValues)
		{
			num = -num;
		}
		return num;
	}
}
