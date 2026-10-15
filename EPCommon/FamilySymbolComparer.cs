using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

internal class FamilySymbolComparer : IComparer<FamilySymbol>
{
	int IComparer<FamilySymbol>.Compare(FamilySymbol first, FamilySymbol second)
	{
		int result = 0;
		if (first != null && first.Name != null)
		{
			result = ((second == null || second.Name == null) ? (-1) : string.Compare(first.Name, second.Name));
		}
		else if (second != null && second.Name != null)
		{
			result = 1;
		}
		return result;
	}
}
