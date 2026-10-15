using System.Collections.Generic;

namespace EPCommon;

public class TopoPointEqualityComparer : IEqualityComparer<TopoPoint>
{
	bool IEqualityComparer<TopoPoint>.Equals(TopoPoint x, TopoPoint y)
	{
		if (x == null || y == null)
		{
			return false;
		}
		return EPShared.eplGeoE_ArePntsDuplicates2D(x, y);
	}

	int IEqualityComparer<TopoPoint>.GetHashCode(TopoPoint obj)
	{
		double x = obj.x;
		double y = obj.y;
		double z = obj.z;
		uint number = obj.Number;
		return x.GetHashCode() + y.GetHashCode() + z.GetHashCode() + number.GetHashCode();
	}
}
