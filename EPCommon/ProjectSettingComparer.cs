using System.Collections.Generic;

namespace EPCommon;

internal class ProjectSettingComparer : IComparer<ProjectSettingContainer>
{
	private bool m_bSortByDateTime = true;

	private bool m_bOrderByDescending;

	public ProjectSettingComparer(bool _bOrderByDescending)
	{
		m_bOrderByDescending = _bOrderByDescending;
	}

	int IComparer<ProjectSettingContainer>.Compare(ProjectSettingContainer FirstProjectSetting, ProjectSettingContainer SecondProjectSetting)
	{
		int num = 0;
		if (m_bSortByDateTime)
		{
			if (FirstProjectSetting != null)
			{
				num = ((SecondProjectSetting == null) ? (-1) : FirstProjectSetting.CurrentDateTime.CompareTo(SecondProjectSetting.CurrentDateTime));
			}
			else if (SecondProjectSetting != null)
			{
				num = 1;
			}
		}
		if (m_bOrderByDescending)
		{
			num = -num;
		}
		return num;
	}
}
