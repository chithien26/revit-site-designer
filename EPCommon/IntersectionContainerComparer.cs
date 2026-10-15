using System.Collections.Generic;

namespace EPCommon;

public class IntersectionContainerComparer : IComparer<IntersectionContainer>
{
	private bool m_bDecendingValues;

	private bool m_bCompareFirstStations = true;

	public IntersectionContainerComparer(bool _bCompareFirstStations)
	{
		m_bCompareFirstStations = _bCompareFirstStations;
	}

	int IComparer<IntersectionContainer>.Compare(IntersectionContainer first, IntersectionContainer second)
	{
		int num = 0;
		if (first != null && first.Profiles != null && first.Profiles.Count > 0)
		{
			if (second == null || second.Profiles == null || second.Profiles.Count <= 0)
			{
				num = -1;
			}
			else
			{
				num = ((!m_bCompareFirstStations) ? first.Profiles[first.Profiles.Count - 1].Station.CompareTo(second.Profiles[second.Profiles.Count - 1].Station) : first.Profiles[0].Station.CompareTo(second.Profiles[0].Station));
			}
		}
		else if (second != null && second.Profiles != null && second.Profiles.Count > 0)
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
