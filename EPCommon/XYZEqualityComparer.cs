using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class XYZEqualityComparer : IEqualityComparer<XYZ>
{
	public bool Equals(XYZ x, XYZ y)
	{
		if (y == null && x == null)
		{
			return true;
		}
		if ((x == null) | (y == null))
		{
			return false;
		}
		return x.IsAlmostEqualTo(y);
	}

	public int GetHashCode(XYZ obj)
	{
		return Math.Round((decimal)obj.X, 9).GetHashCode() + Math.Round((decimal)obj.Y, 9).GetHashCode() + Math.Round((decimal)obj.Y, 9).GetHashCode();
	}
}
