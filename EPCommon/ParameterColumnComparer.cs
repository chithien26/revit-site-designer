using System.Collections.Generic;

namespace EPCommon;

internal class ParameterColumnComparer : IComparer<ParameterColumn>
{
	private bool m_bSortByOrder = true;

	int IComparer<ParameterColumn>.Compare(ParameterColumn first, ParameterColumn second)
	{
		int result = 0;
		if (m_bSortByOrder)
		{
			if (first != null)
			{
				result = ((second == null) ? (-1) : first.Order.CompareTo(second.Order));
			}
			else if (second != null)
			{
				result = 1;
			}
		}
		return result;
	}
}
