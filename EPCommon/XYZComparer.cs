using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class XYZComparer : IComparer<XYZ>
{
	private bool m_bSortByElevation = true;

	private bool m_bDecendingValues = true;

	int IComparer<XYZ>.Compare(XYZ first, XYZ second)
	{
		int num = 0;
		if (m_bSortByElevation)
		{
			if (first != null)
			{
				num = ((second == null) ? (-1) : first.Z.CompareTo(second.Z));
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
