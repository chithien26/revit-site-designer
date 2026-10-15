using System.Runtime.InteropServices;
using Autodesk.Revit.DB;

namespace EPCommon;

[StructLayout(LayoutKind.Sequential)]
public class Eps_Point
{
	private double m_dX;

	private double m_dY;

	private double m_dZ;

	public double x
	{
		get
		{
			return m_dX;
		}
		set
		{
			m_dX = value;
		}
	}

	public double y
	{
		get
		{
			return m_dY;
		}
		set
		{
			m_dY = value;
		}
	}

	public double z
	{
		get
		{
			return m_dZ;
		}
		set
		{
			m_dZ = value;
		}
	}

	public XYZ XYZ => new XYZ(m_dX, m_dY, m_dZ);

	public XYZ XY => new XYZ(m_dX, m_dY, 0.0);

	public Eps_Point()
	{
		m_dX = 0.0;
		m_dY = 0.0;
		m_dZ = 0.0;
	}

	public Eps_Point(XYZ _XYZPoint)
	{
		m_dX = _XYZPoint.X;
		m_dY = _XYZPoint.Y;
		m_dZ = _XYZPoint.Z;
	}

	public Eps_Point(double _dX, double _dY, double _dZ)
	{
		m_dX = _dX;
		m_dY = _dY;
		m_dZ = _dZ;
	}

	public Eps_Point(Eps_Point _Point)
	{
		m_dX = _Point.x;
		m_dY = _Point.y;
		m_dZ = _Point.z;
	}

	public bool IsEqual(Eps_Point _ComparePoint)
	{
		return EPShared.eplGeoE_ArePntsDuplicates2D(this, _ComparePoint);
	}
}
