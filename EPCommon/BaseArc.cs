using System.Collections.Generic;
using System.Runtime.InteropServices;
using Autodesk.Revit.DB;

namespace EPCommon;

[StructLayout(LayoutKind.Sequential)]
public class BaseArc
{
	private Eps_Point m_StartPnt;

	private Eps_Point m_EndPnt;

	private Eps_Point m_RadiusPnt;

	private double m_dDelta;

	private double m_dRadius;

	private int m_nDirection;

	public Eps_Point StartPnt
	{
		get
		{
			return m_StartPnt;
		}
		set
		{
			m_StartPnt = value;
		}
	}

	public Eps_Point RadiusPnt
	{
		get
		{
			return m_RadiusPnt;
		}
		set
		{
			m_RadiusPnt = value;
		}
	}

	public Eps_Point EndPnt
	{
		get
		{
			return m_EndPnt;
		}
		set
		{
			m_EndPnt = value;
		}
	}

	public int Direction
	{
		get
		{
			return m_nDirection;
		}
		set
		{
			m_nDirection = value;
		}
	}

	public double Delta
	{
		get
		{
			return m_dDelta;
		}
		set
		{
			m_dDelta = value;
		}
	}

	public double Radius
	{
		get
		{
			return m_dRadius;
		}
		set
		{
			m_dRadius = value;
		}
	}

	public BaseArc()
	{
		m_StartPnt = new Eps_Point();
		m_EndPnt = new Eps_Point();
		m_RadiusPnt = new Eps_Point();
		m_nDirection = -1;
		m_dDelta = 0.0;
		m_dRadius = 0.0;
	}

	public BaseArc(Arc _Arc)
	{
		if (_Arc.IsBound)
		{
			m_StartPnt = new Eps_Point(_Arc.GetEndPoint(0));
			m_EndPnt = new Eps_Point(_Arc.GetEndPoint(1));
		}
		else
		{
			IList<XYZ> list = _Arc.Tessellate();
			if (list.Count > 0)
			{
				m_StartPnt = new Eps_Point(list[0]);
				m_EndPnt = new Eps_Point(list[list.Count - 1]);
			}
		}
		m_RadiusPnt = new Eps_Point(_Arc.Center);
		if (_Arc.Normal.Z > 0.0)
		{
			m_nDirection = 1;
		}
		else
		{
			m_nDirection = -1;
		}
		m_dDelta = _Arc.Length / _Arc.Radius;
		m_dRadius = _Arc.Radius;
	}

	public BaseArc(BaseArc _Arc)
	{
		m_dDelta = _Arc.Delta;
		m_EndPnt = new Eps_Point(_Arc.EndPnt);
		m_dRadius = _Arc.Radius;
		m_nDirection = _Arc.Direction;
		m_RadiusPnt = new Eps_Point(_Arc.RadiusPnt);
		m_StartPnt = new Eps_Point(_Arc.StartPnt);
	}

	public bool IsEqual(BaseArc _CompareArc)
	{
		return IsEqual(_CompareArc, _bCompareDirection: false);
	}

	public bool IsEqual(BaseArc _CompareArc, bool _bCompareDirection)
	{
		bool result = false;
		if (((EPShared.eplGeoE_ArePntsDuplicates2D(m_StartPnt, _CompareArc.StartPnt) && EPShared.eplGeoE_ArePntsDuplicates2D(m_EndPnt, _CompareArc.EndPnt) && EPShared.eplGeoE_ArePntsDuplicates2D(m_RadiusPnt, _CompareArc.RadiusPnt) && m_nDirection == _CompareArc.Direction) || (EPShared.eplGeoE_ArePntsDuplicates2D(m_EndPnt, _CompareArc.StartPnt) && EPShared.eplGeoE_ArePntsDuplicates2D(m_StartPnt, _CompareArc.EndPnt) && EPShared.eplGeoE_ArePntsDuplicates2D(m_RadiusPnt, _CompareArc.RadiusPnt))) && (!_bCompareDirection || m_nDirection == _CompareArc.Direction))
		{
			result = true;
		}
		return result;
	}
}
