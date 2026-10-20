using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class FailurePreprocessor : IFailuresPreprocessor
{
	private List<FailureDefinitionId> m_FailureDefinitionIdList;

	public List<FailureDefinitionId> FailureDefinitionIdList
	{
		get
		{
			return m_FailureDefinitionIdList;
		}
		set
		{
			m_FailureDefinitionIdList = value;
		}
	}

	public FailurePreprocessor(FailureDefinitionId _FailureDefinitionId)
	{
		m_FailureDefinitionIdList = new List<FailureDefinitionId>();
		m_FailureDefinitionIdList.Add(_FailureDefinitionId);
	}

	public FailurePreprocessor(List<FailureDefinitionId> _FailureDefinitionIdList)
	{
		m_FailureDefinitionIdList = _FailureDefinitionIdList;
	}

	public FailureProcessingResult PreprocessFailures(FailuresAccessor FailuresAccessor)
	{
		IList<FailureMessageAccessor> list = new List<FailureMessageAccessor>();
		list = FailuresAccessor.GetFailureMessages();
		foreach (FailureMessageAccessor item in list)
		{
			FailureDefinitionId failureDefinitionId = item.GetFailureDefinitionId();
			for (int i = 0; i < m_FailureDefinitionIdList.Count; i++)
			{
				if (m_FailureDefinitionIdList[i] == failureDefinitionId)
				{
					FailuresAccessor.DeleteWarning(item);
					break;
				}
			}
		}
		TerrainFailures.DeleteExpectedWarnings(FailuresAccessor);
		return FailureProcessingResult.Continue;
	}
}
