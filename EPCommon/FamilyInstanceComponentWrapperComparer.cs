using System.Collections.Generic;

namespace EPCommon;

public class FamilyInstanceComponentWrapperComparer : IComparer<FamilyInstanceComponentWrapper>
{
	int IComparer<FamilyInstanceComponentWrapper>.Compare(FamilyInstanceComponentWrapper first, FamilyInstanceComponentWrapper second)
	{
		int result = 0;
		if (first != null)
		{
			result = ((second == null) ? (-1) : first.DrawOrder.CompareTo(second.DrawOrder));
		}
		else if (second != null)
		{
			result = 1;
		}
		return result;
	}
}
