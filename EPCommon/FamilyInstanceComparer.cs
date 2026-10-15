using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

internal class FamilyInstanceComparer : IComparer<FamilyInstance>
{
	private XYZ m_ProximityPoint;

	private bool m_bSortByProximity;

	public FamilyInstanceComparer(XYZ _ProximityPoint)
	{
		m_ProximityPoint = _ProximityPoint;
		m_bSortByProximity = true;
	}

	int IComparer<FamilyInstance>.Compare(FamilyInstance first, FamilyInstance second)
	{
		int result = 0;
		if (m_bSortByProximity)
		{
			if (first != null && first.Location != null)
			{
				if (second != null && second.Location != null)
				{
					LocationPoint locationPoint = first.Location as LocationPoint;
					LocationPoint locationPoint2 = second.Location as LocationPoint;
					result = Math.Sqrt(Math.Pow(m_ProximityPoint.X - locationPoint.Point.X, 2.0) + Math.Pow(m_ProximityPoint.Y - locationPoint.Point.Y, 2.0)).CompareTo(Math.Sqrt(Math.Pow(m_ProximityPoint.X - locationPoint2.Point.X, 2.0) + Math.Pow(m_ProximityPoint.Y - locationPoint2.Point.Y, 2.0)));
				}
				else
				{
					result = -1;
				}
			}
			else if (second != null && second.Location != null)
			{
				result = 1;
			}
		}
		return result;
	}
}
