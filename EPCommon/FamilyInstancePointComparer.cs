using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilyInstancePointComparer : IComparer<FamilyInstance>
{
	private XYZ m_QueryPoint;

	public FamilyInstancePointComparer(XYZ _QueryPoint)
	{
		m_QueryPoint = _QueryPoint;
	}

	int IComparer<FamilyInstance>.Compare(FamilyInstance first, FamilyInstance second)
	{
		int result = 0;
		if (first != null && first.Location != null)
		{
			if (first.Location is LocationPoint locationPoint && second != null && second.Location != null)
			{
				if (second.Location is LocationPoint locationPoint2)
				{
					result = Math.Sqrt(Math.Pow(m_QueryPoint.X - locationPoint.Point.X, 2.0) + Math.Pow(m_QueryPoint.Y - locationPoint.Point.Y, 2.0)).CompareTo(Math.Sqrt(Math.Pow(m_QueryPoint.X - locationPoint2.Point.X, 2.0) + Math.Pow(m_QueryPoint.Y - locationPoint2.Point.Y, 2.0)));
				}
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
		return result;
	}
}
