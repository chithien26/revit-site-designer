using System.Collections.Generic;

namespace EPCommon;

internal class DoubleComparer : IComparer<double>
{
	int IComparer<double>.Compare(double first, double second)
	{
		return first.CompareTo(second);
	}
}
