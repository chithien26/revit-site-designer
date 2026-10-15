using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

internal class FamilyParameterComparer : IComparer<FamilyParameter>
{
	private bool m_bSortByDefinitionName = true;

	int IComparer<FamilyParameter>.Compare(FamilyParameter first, FamilyParameter second)
	{
		int result = 0;
		if (m_bSortByDefinitionName)
		{
			if (first != null && first.Definition != null)
			{
				result = ((second == null || second.Definition == null) ? (-1) : string.Compare(first.Definition.Name, second.Definition.Name));
			}
			else if (second != null && second.Definition != null)
			{
				result = 1;
			}
		}
		return result;
	}
}
