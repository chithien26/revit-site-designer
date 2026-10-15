namespace EPCommon;

public class Eps_Triangle
{
	private TopoPoint[] m_Verticies;

	public TopoPoint[] Verticies
	{
		get
		{
			return m_Verticies;
		}
		set
		{
			m_Verticies = value;
		}
	}

	public Eps_Triangle()
	{
		m_Verticies = new TopoPoint[3];
	}

	public bool IsPointInside(Eps_Point _FindPoint)
	{
		Eps_Line eps_Line = new Eps_Line();
		bool result = true;
		for (int i = 0; i < 3; i++)
		{
			if (i < 2)
			{
				eps_Line.StartPnt = m_Verticies[i];
				eps_Line.EndPnt = m_Verticies[i + 1];
			}
			else
			{
				eps_Line.StartPnt = m_Verticies[i];
				eps_Line.EndPnt = m_Verticies[0];
			}
			if (RevitGeometry.GeoE_CalcSignedArea(eps_Line, _FindPoint) > 0.0)
			{
				result = false;
				break;
			}
		}
		return result;
	}
}
