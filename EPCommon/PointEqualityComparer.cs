using System.Collections.Generic;

namespace EPCommon;

public class PointEqualityComparer : IEqualityComparer<Eps_Point>
{
	bool IEqualityComparer<Eps_Point>.Equals(Eps_Point x, Eps_Point y)
	{
		if (x == null || y == null)
		{
			return false;
		}
		return EPShared.eplGeoE_ArePntsDuplicates2D(x, y);
	}

	int IEqualityComparer<Eps_Point>.GetHashCode(Eps_Point obj)
	{
		double x = obj.x;
		double y = obj.y;
		double z = obj.z;
		return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
	}
}
