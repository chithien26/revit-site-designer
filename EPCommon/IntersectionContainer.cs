using System.Collections.Generic;

namespace EPCommon;

public class IntersectionContainer
{
	private List<ProfileContainer> m_ProfileList;

	private bool m_bStartingIntersection;

	private ProfileUsage m_ProfileUsage;

	private Eps_Leg m_RightOffsetLeg;

	private Eps_Leg m_LeftOffsetLeg;

	public ProfileUsage ProfileUsage
	{
		get
		{
			return m_ProfileUsage;
		}
		set
		{
			m_ProfileUsage = value;
		}
	}

	public bool StartingIntersection
	{
		get
		{
			return m_bStartingIntersection;
		}
		set
		{
			m_bStartingIntersection = value;
		}
	}

	public List<ProfileContainer> Profiles
	{
		get
		{
			return m_ProfileList;
		}
		set
		{
			m_ProfileList = value;
		}
	}

	public Eps_Leg RightOffsetLeg
	{
		get
		{
			return m_RightOffsetLeg;
		}
		set
		{
			m_RightOffsetLeg = value;
		}
	}

	public Eps_Leg LeftOffsetLeg
	{
		get
		{
			return m_LeftOffsetLeg;
		}
		set
		{
			m_LeftOffsetLeg = value;
		}
	}

	public IntersectionContainer()
	{
		m_ProfileUsage = ProfileUsage.EntireProfile;
		m_bStartingIntersection = true;
		m_ProfileList = new List<ProfileContainer>();
		m_RightOffsetLeg = null;
		m_LeftOffsetLeg = null;
	}

	public bool IsEqual(IntersectionContainer another)
	{
		if (ProfileUsage == another.ProfileUsage && StartingIntersection == another.StartingIntersection && IsProfileEqual(Profiles, another.Profiles) && LeftOffsetLeg.IsEqual(another.LeftOffsetLeg))
		{
			return RightOffsetLeg.IsEqual(another.RightOffsetLeg);
		}
		return false;
	}

	private bool IsProfileEqual(List<ProfileContainer> first, List<ProfileContainer> second)
	{
		if (first.Count != second.Count)
		{
			return false;
		}
		for (int i = 0; i < first.Count; i++)
		{
			if (!first[i].IsEqual(second[i]))
			{
				return false;
			}
		}
		return true;
	}
}
