using System.Runtime.InteropServices;
using Autodesk.Revit.DB;

namespace EPCommon;

[StructLayout(LayoutKind.Sequential)]
public class TopoPoint : Eps_Point
{
	private uint m_Number;

	public uint Number
	{
		get
		{
			return m_Number;
		}
		set
		{
			m_Number = value;
		}
	}

	public TopoPoint()
	{
	}

	public TopoPoint(Eps_Point _Point)
		: base(_Point)
	{
	}

	public TopoPoint(TopoPoint _Point)
		: base(_Point)
	{
		m_Number = _Point.Number;
	}

	public TopoPoint(XYZ _XYZPoint)
		: base(_XYZPoint)
	{
	}

	public TopoPoint(double _dX, double _dY, double _dZ)
		: base(_dX, _dY, _dZ)
	{
	}
}
