using System.Collections.Generic;

namespace EPCommon;

public class MeshTriangleLineComparer : IComparer<MeshTriangleLine>
{
	private bool m_bSortByStartNumber = true;

	int IComparer<MeshTriangleLine>.Compare(MeshTriangleLine first, MeshTriangleLine second)
	{
		int result = 0;
		if (m_bSortByStartNumber)
		{
			if (first != null)
			{
				result = ((second == null) ? (-1) : first.LineStartNumber.CompareTo(second.LineStartNumber));
			}
			else if (second != null)
			{
				result = 1;
			}
		}
		return result;
	}
}
