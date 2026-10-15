using System;
using System.Runtime.InteropServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace EPCommon;

[StructLayout(LayoutKind.Sequential)]
public class Eps_Arc : BaseArc
{
	private bool m_bIsReverse;

	public bool IsReverse
	{
		get
		{
			return m_bIsReverse;
		}
		set
		{
			m_bIsReverse = value;
		}
	}

	public Eps_Arc()
	{
	}

	public Eps_Arc(Arc _Arc)
		: base(_Arc)
	{
	}

	public Eps_Arc(Eps_Arc _Arc)
		: base(_Arc)
	{
	}

	public Eps_Arc(BaseArc _Arc)
		: base(_Arc)
	{
	}

	public Eps_Arc Reverse()
	{
		Eps_Arc eps_Arc = new Eps_Arc();
		eps_Arc.StartPnt = EndPnt;
		eps_Arc.EndPnt = StartPnt;
		eps_Arc.RadiusPnt = RadiusPnt;
		eps_Arc.Radius = Radius;
		eps_Arc.Delta = Delta;
		eps_Arc.Direction = Direction;
		if (Direction == -1)
		{
			eps_Arc.Direction = 1;
		}
		else
		{
			eps_Arc.Direction = -1;
		}
		eps_Arc.IsReverse = !m_bIsReverse;
		return eps_Arc;
	}

	public Arc RevitArc(UIDocument _CurDocument)
	{
		return RevitArc(_CurDocument.Document);
	}

	public Arc RevitArc(Document _CurDocument)
	{
		if (!EPShared.eplGeoE_ArePntsDuplicates2D(StartPnt, EndPnt))
		{
			double num = Radius * Delta;
			Eps_Point _StaOffPnt = new Eps_Point();
			EPShared.eplGeoE_FindStaOffPointAlongArc(this, num / 2.0, 0.0, ref _StaOffPnt);
			return Arc.Create(StartPnt.XYZ, EndPnt.XYZ, _StaOffPnt.XYZ);
		}
		double startAngle = 0.0;
		double endAngle = Math.PI * 2.0;
		XYZ xAxis = new XYZ(1.0, 0.0, 0.0);
		XYZ yAxis = new XYZ(0.0, 1.0, 0.0);
		return Arc.Create(RadiusPnt.XYZ, Radius, startAngle, endAngle, xAxis, yAxis);
	}
}
