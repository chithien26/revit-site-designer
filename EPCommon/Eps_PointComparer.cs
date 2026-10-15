using System.Collections.Generic;

namespace EPCommon;

public class Eps_PointComparer : IComparer<Eps_Point>
{
	private bool m_bSortByElevation = true;

	private bool m_bDecendingValues = true;

	int IComparer<Eps_Point>.Compare(Eps_Point first, Eps_Point second)
	{
		int num = 0;
		if (m_bSortByElevation)
		{
			if (first != null)
			{
				num = ((second == null) ? (-1) : first.z.CompareTo(second.z));
			}
			else if (second != null)
			{
				num = 1;
			}
		}
		if (m_bDecendingValues)
		{
			num = -num;
		}
		return num;
	}
}
