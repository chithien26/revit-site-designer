using System.Collections.Generic;

namespace EPCommon;

public class ProfileContainer
{
	private List<Eps_Point> m_PointList;

	private double m_Station;

	public double Station
	{
		get
		{
			return m_Station;
		}
		set
		{
			m_Station = value;
		}
	}

	public List<Eps_Point> Points
	{
		get
		{
			return m_PointList;
		}
		set
		{
			m_PointList = value;
		}
	}

	public ProfileContainer()
	{
		m_Station = 0.0;
		m_PointList = new List<Eps_Point>();
	}

	public bool IsEqual(ProfileContainer another)
	{
		if (!RevitDefines.EQ(Station, another.Station))
		{
			return false;
		}
		if ((Points == null && another.Points != null) || (Points != null && another.Points == null))
		{
			return false;
		}
		if (Points == null && another.Points == null)
		{
			return true;
		}
		if (Points.Count != another.Points.Count)
		{
			return false;
		}
		for (int i = 0; i < Points.Count; i++)
		{
			if (!Points[i].IsEqual(another.Points[i]))
			{
				return false;
			}
		}
		return true;
	}
}
