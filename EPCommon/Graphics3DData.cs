using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class Graphics3DData
{
	public enum ViewDirections
	{
		Top,
		Front,
		Left,
		Right,
		Bottom,
		Back,
		IsoMetric
	}

	private const double m_dRotateAngle = Math.PI / 90.0;

	private readonly List<List<Vector>> m_IniCurves = new List<List<Vector>>();

	private readonly List<List<Vector>> m_Curves = new List<List<Vector>>();

	private readonly int m_NumPoints;

	private BoundingBoxXYZ m_BoundingBox = new BoundingBoxXYZ();

	private UCS m_CurrentUCS = new UCS();

	private static readonly UCS[] SpecialUCSs;

	private static readonly UCS RotateXUCS;

	private static readonly UCS RotateAntiXUCS;

	private static readonly UCS RotateYUCS;

	private static readonly UCS RotateAntiYUCS;

	private static readonly UCS RotateZUCS;

	private static readonly UCS RotateAntiZUCS;

	public UCS CurrentUCS
	{
		get
		{
			return m_CurrentUCS;
		}
		set
		{
			m_CurrentUCS = value;
			UpdateDisplayData(m_CurrentUCS);
			if (UpdateViewEvent != null)
			{
				UpdateViewEvent();
			}
		}
	}

	public int NumPoints => m_NumPoints;

	public List<List<Vector>> ConstCurves => m_Curves;

	public BoundingBoxXYZ BoundingBox => m_BoundingBox;

	public event UpdateViewDelegate UpdateViewEvent;

	static Graphics3DData()
	{
		SpecialUCSs = new UCS[7];
		double num = Math.PI / 90.0;
		double num2 = -Math.PI / 90.0;
		double num3 = Math.Sin(num);
		double num4 = Math.Cos(num);
		double num5 = Math.Sin(num2);
		double num6 = Math.Cos(num2);
		Vector origin = default;
		RotateXUCS = new UCS(origin, new Vector(1.0, 0.0, 0.0), new Vector(0.0, num4, num3));
		RotateAntiXUCS = new UCS(origin, new Vector(1.0, 0.0, 0.0), new Vector(0.0, num6, num5));
		RotateYUCS = new UCS(origin, new Vector(num4, 0.0, 0.0 - num3), new Vector(0.0, 1.0, 0.0));
		RotateAntiYUCS = new UCS(origin, new Vector(num6, 0.0, 0.0 - num5), new Vector(0.0, 1.0, 0.0));
		RotateZUCS = new UCS(origin, new Vector(num4, num3, 0.0), new Vector(0.0 - num3, num4, 0.0));
		RotateAntiZUCS = new UCS(origin, new Vector(num6, num5, 0.0), new Vector(0.0 - num5, num6, 0.0));
		SpecialUCSs[6] = new UCS(origin, new Vector(-0.408248290463863, 0.408248290463863, 0.816496580927726), new Vector(0.707106781186548, 0.707106781186548, 0.0));
		SpecialUCSs[0] = new UCS(origin, new Vector(1.0, 0.0, 0.0), new Vector(0.0, 1.0, 0.0));
		SpecialUCSs[1] = new UCS(origin, new Vector(-1.0, 0.0, 0.0), new Vector(0.0, 0.0, 1.0));
		SpecialUCSs[2] = new UCS(origin, new Vector(0.0, -1.0, 0.0), new Vector(0.0, 0.0, 1.0));
		SpecialUCSs[3] = new UCS(origin, new Vector(0.0, 1.0, 0.0), new Vector(0.0, 0.0, 1.0));
		SpecialUCSs[4] = new UCS(origin, new Vector(-1.0, 0.0, 0.0), new Vector(0.0, 1.0, 0.0));
		SpecialUCSs[5] = new UCS(origin, new Vector(1.0, 0.0, 0.0), new Vector(0.0, 0.0, 1.0));
	}

	public Graphics3DData(List<IList<XYZ>> _Curves, BoundingBoxXYZ _BoundingBox)
	{
		if (_BoundingBox == null)
		{
			return;
		}
		UCS[] specialUCSs = SpecialUCSs;
		foreach (UCS uCS in specialUCSs)
		{
			uCS.Origin = FindBBoxCenter(_BoundingBox);
		}
		foreach (IList<XYZ> _Curf in _Curves)
		{
			List<Vector> list = new List<Vector>();
			m_IniCurves.Add(list);
			foreach (XYZ item in _Curf)
			{
				list.Add(new Vector(item.X, item.Y, item.Z));
				m_NumPoints++;
			}
		}
		SetViewDirection(ViewDirections.IsoMetric);
		InitializeBBox(_BoundingBox);
	}

	public void SetViewDirection(ViewDirections viewDirection)
	{
		m_CurrentUCS = SpecialUCSs[(int)viewDirection];
		UpdateDisplayData(m_CurrentUCS);
	}

	public void RotateZ(bool _bDirection)
	{
		if (_bDirection)
		{
			m_CurrentUCS += RotateZUCS;
			UpdateDisplayData(m_CurrentUCS);
		}
		else
		{
			m_CurrentUCS += RotateAntiZUCS;
			UpdateDisplayData(m_CurrentUCS);
		}
	}

	public void RotateY(bool _bDirection)
	{
		if (_bDirection)
		{
			m_CurrentUCS += RotateYUCS;
			UpdateDisplayData(m_CurrentUCS);
		}
		else
		{
			m_CurrentUCS += RotateAntiYUCS;
			UpdateDisplayData(m_CurrentUCS);
		}
	}

	public void RotateX(bool _bDirection)
	{
		if (_bDirection)
		{
			m_CurrentUCS += RotateXUCS;
			UpdateDisplayData(m_CurrentUCS);
		}
		else
		{
			m_CurrentUCS += RotateAntiXUCS;
			UpdateDisplayData(m_CurrentUCS);
		}
	}

	private void UpdateDisplayData(UCS _LC)
	{
		m_Curves.Clear();
		foreach (List<Vector> iniCurf in m_IniCurves)
		{
			List<Vector> list = new List<Vector>();
			m_Curves.Add(list);
			for (int i = 0; i < iniCurf.Count; i++)
			{
				list.Add(_LC.GC2LC(iniCurf[i]));
			}
		}
		if (UpdateViewEvent != null)
		{
			UpdateViewEvent();
		}
	}

	private void InitializeBBox(BoundingBoxXYZ _BoundingBox)
	{
		XYZ lhs = MathUtil.SubXYZ(_BoundingBox.Max, _BoundingBox.Min);
		m_BoundingBox.Max = MathUtil.DivideXYZ(lhs, 2.0);
		m_BoundingBox.Min = MathUtil.DivideXYZ(lhs, -2.0);
	}

	private Vector FindBBoxCenter(BoundingBoxXYZ _BoundingBox)
	{
		XYZ pnt = MathUtil.DivideXYZ(MathUtil.AddXYZ(_BoundingBox.Max, _BoundingBox.Min), 2.0);
		return MathUtil.XYZ2Vector(pnt);
	}
}
