using System.Collections.Generic;

namespace EPCommon;

internal class FamilyParameterWrapperComparer : IComparer<FamilyParameterWrapper>
{
	private bool m_bSortByDefinitionName = true;

	int IComparer<FamilyParameterWrapper>.Compare(FamilyParameterWrapper first, FamilyParameterWrapper second)
	{
		int result = 0;
		if (m_bSortByDefinitionName)
		{
			if (first != null)
			{
				if (first.FamilyParameter != null && first.FamilyParameter.Definition != null)
				{
					if (second == null)
					{
						result = -1;
					}
					else
					{
						result = ((second.FamilyParameter == null || second.FamilyParameter.Definition == null) ? string.Compare(first.FamilyParameter.Definition.Name, second.ParameterName) : string.Compare(first.FamilyParameter.Definition.Name, second.FamilyParameter.Definition.Name));
					}
				}
				else
				{
					result = ((second.FamilyParameter == null || second.FamilyParameter.Definition == null) ? string.Compare(first.ParameterName, second.ParameterName) : string.Compare(first.ParameterName, second.FamilyParameter.Definition.Name));
				}
			}
			else if (second != null && ((second.FamilyParameter != null && second.FamilyParameter.Definition != null) || second.ParameterName != string.Empty))
			{
				result = 1;
			}
		}
		return result;
	}
}
