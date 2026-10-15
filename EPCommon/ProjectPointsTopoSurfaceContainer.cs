using System.Collections.Generic;

namespace EPCommon;

public class ProjectPointsTopoSurfaceContainer
{
	private TopographySurfaceContainer m_TopographySurfaceContainer;

	private List<Eps_Point> m_ProjectPoints;

	public TopographySurfaceContainer TopographySurfaceContainer => m_TopographySurfaceContainer;

	public List<Eps_Point> ProjectPoints
	{
		get
		{
			return m_ProjectPoints;
		}
		set
		{
			m_ProjectPoints = value;
		}
	}

	public ProjectPointsTopoSurfaceContainer(TopographySurfaceContainer _TopographySurfaceContainer)
	{
		m_TopographySurfaceContainer = _TopographySurfaceContainer;
		m_ProjectPoints = new List<Eps_Point>();
	}

	public ProjectPointsTopoSurfaceContainer(TopographySurfaceContainer _TopographySurfaceContainer, Eps_Point _CurPoint)
	{
		m_TopographySurfaceContainer = _TopographySurfaceContainer;
		m_ProjectPoints = new List<Eps_Point>();
		m_ProjectPoints.Add(_CurPoint);
	}
}
