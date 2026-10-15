using System;
using System.Collections.Generic;
using System.Resources;

namespace EPCommon;

public class EPShared
{
	private const double angleRatio = Math.PI / 180.0;

	private static ResourceManager m_LocRM;

	public static ResourceManager LocalRM
	{
		get
		{
			if (m_LocRM == null)
			{
				m_LocRM = new ResourceManager("SITES.WinFormStrings", typeof(SafeResources).Assembly);
			}
			return m_LocRM;
		}
	}

	public static bool eplGeoE_ArePntsDuplicates2D(Eps_Point _FirstCheckPnt, Eps_Point _SecondCheckPnt)
	{
		return eplGeoE_ArePntsDuplicates2D(_FirstCheckPnt, _SecondCheckPnt, 1E-05);
	}

	public static bool eplGeoE_ArePntsDuplicates2D(Eps_Point _FirstCheckPnt, Eps_Point _SecondCheckPnt, double _dTolerance)
	{
		double num = _dTolerance;
		bool result = false;
		double num2 = _FirstCheckPnt.x - _SecondCheckPnt.x;
		double num3 = _FirstCheckPnt.y - _SecondCheckPnt.y;
		if (RevitDefines.EQ(_dTolerance, 1E-09, 0.0))
		{
			double num4 = _dTolerance;
			double num5 = _dTolerance;
			num4 = eplCalE_CalcPointTolerance(_FirstCheckPnt, _dTolerance, _Is3D: false);
			num5 = eplCalE_CalcPointTolerance(_SecondCheckPnt, _dTolerance, _Is3D: false);
			if (RevitDefines.GT(num4, num, 0.0))
			{
				num = num4;
			}
			if (RevitDefines.GT(num5, num, 0.0))
			{
				num = num5;
			}
		}
		if (Math.Sqrt(num2 * num2 + num3 * num3) <= num)
		{
			result = true;
		}
		return result;
	}

	public static int eplGeoE_CalcHorzAngByThreePnts(Eps_Point _StartPnt, Eps_Point _Vertex, Eps_Point _EndPnt, int _nAngleType, ref double _dAngle, double _dPointTol)
	{
		if (eplGeoE_ArePntsDuplicates2D(_Vertex, _StartPnt, _dPointTol) || eplGeoE_ArePntsDuplicates2D(_Vertex, _EndPnt, _dPointTol))
		{
			return 6;
		}
		double num = eplGeoE_CalcPlaneDirBtwnTwoPnts(_Vertex, _StartPnt);
		double num2 = eplGeoE_CalcPlaneDirBtwnTwoPnts(_Vertex, _EndPnt);
		double num3 = eplGeoE_CalcPlaneDirBtwnTwoPnts(_StartPnt, _Vertex);
		switch (_nAngleType)
		{
		case 11:
			_dAngle = Math.Abs(num - num2);
			if (_dAngle > Math.PI)
			{
				_dAngle = Math.PI * 2.0 - _dAngle;
			}
			break;
		case 13:
			_dAngle = eplGeoE_NormalizeRadians(num2 - num);
			break;
		case 12:
			_dAngle = eplGeoE_NormalizeRadians(num - num2);
			break;
		case 15:
			_dAngle = eplGeoE_NormalizeRadians(num2 - num3);
			break;
		case 14:
			_dAngle = eplGeoE_NormalizeRadians(num3 - num2);
			break;
		default:
			return 6;
		}
		return 0;
	}

	public static double eplGeoE_NormalizeRadians(double _dAngle)
	{
		return _dAngle - eplCalE_floor(_dAngle / (Math.PI * 2.0), 1E-10) * (Math.PI * 2.0);
	}

	public static double eplCalE_floor(double Operand, double Tolerance)
	{
		return Math.Floor(Operand + Tolerance);
	}

	public static int eplGeoE_CalcHorzDirBtwnTwoPnts(Eps_Point _FirstPnt, Eps_Point _SecondPnt, ref double _dDirection)
	{
		return eplGeoE_CalcHorzDirBtwnTwoPnts(_FirstPnt, _SecondPnt, ref _dDirection, 1E-05);
	}

	public static int eplGeoE_CalcHorzDirBtwnTwoPnts(Eps_Point _FirstPnt, Eps_Point _SecondPnt, ref double _dDirection, double _dMinDist)
	{
		if (eplGeoE_ArePntsDuplicates2D(_FirstPnt, _SecondPnt, _dMinDist))
		{
			return 6;
		}
		_dDirection = eplGeoE_CalcPlaneDirBtwnTwoPnts(_FirstPnt, _SecondPnt);
		return 0;
	}

	public static double eplGeoE_CalcPlaneDirBtwnTwoPnts(Eps_Point _FirstPnt, Eps_Point _SecondPnt)
	{
		double num = _SecondPnt.x - _FirstPnt.x;
		double num2 = _SecondPnt.y - _FirstPnt.y;
		double dAngle = ((num != 0.0 || num2 != 0.0) ? Math.Atan2(num2, num) : 0.0);
		return eplGeoE_NormalizeRadians(dAngle);
	}

	public static int eplGeoE_CalcHorzDistBtwnTwoPnts(Eps_Point _FirstPnt, Eps_Point _SecondPnt, ref double _dDistance)
	{
		return eplGeoE_CalcHorzDistBtwnTwoPnts(_FirstPnt, _SecondPnt, ref _dDistance, 1E-05);
	}

	public static int eplGeoE_CalcHorzDistBtwnTwoPnts(Eps_Point _FirstPnt, Eps_Point _SecondPnt, ref double _dDistance, double _dMinDist)
	{
		double num = eplGeoE_CalcPlaneDistBtwnTwoPnts(_FirstPnt.x, _FirstPnt.y, _SecondPnt.x, _SecondPnt.y);
		if (num <= _dMinDist)
		{
			_dDistance = 0.0;
			return 6;
		}
		_dDistance = num;
		return 0;
	}

	public static void eplGeoE_CalcPolarToXY(Eps_Point _StartPnt, double _dPolarDirection, double _dPolarDistance, ref Eps_Point _NewPnt)
	{
		double dX = _StartPnt.XYZ.X + _dPolarDistance * Math.Cos(_dPolarDirection);
		double dY = _StartPnt.XYZ.Y + _dPolarDistance * Math.Sin(_dPolarDirection);
		_NewPnt = new Eps_Point(dX, dY, _StartPnt.XYZ.Z);
	}

	public static void eplGeoE_FindStaOffPointAlongArc(Eps_Arc _Arc, double _dStation, double _dOffset, ref Eps_Point _StaOffPnt)
	{
		_StaOffPnt = new Eps_Point();
		double num = 0.0;
		double num2 = 0.0;
		double _dDirection = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		num = eplGeoE_SolveArcLenByRadiusAndDelta(_Arc.Radius, _Arc.Delta);
		num2 = ((!(num <= 1E-05)) ? (_Arc.Delta * (_dStation / num)) : 0.0);
		eplGeoE_CalcHorzDirBtwnTwoPnts(_Arc.RadiusPnt, _Arc.StartPnt, ref _dDirection);
		eplGeoE_CalcPolarToXY(_dPolarDirection: (_Arc.Direction != -1) ? (_dDirection + num2) : (_dDirection - num2), _dPolarDistance: (_Arc.Direction != -1) ? (_Arc.Radius + _dOffset) : (_Arc.Radius - _dOffset), _StartPnt: _Arc.RadiusPnt, _NewPnt: ref _StaOffPnt);
	}

	public static void eplGeoE_FindStaOffPointAlongLine(Eps_Line _Line, double _dStation, double _dOffset, ref Eps_Point _StaOffPnt)
	{
		_StaOffPnt = new Eps_Point();
		double _dDirection = 0.0;
		Eps_Point _NewPnt = new Eps_Point();
		eplGeoE_CalcHorzDirBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDirection);
		eplGeoE_CalcPolarToXY(_Line.StartPnt, _dDirection, _dStation, ref _NewPnt);
		eplGeoE_CalcPolarToXY(_NewPnt, _dDirection - Math.PI / 2.0, _dOffset, ref _StaOffPnt);
	}

	public static void eplGeoE_CalcTangentDirOfArc(Eps_Point _ArcPnt, Eps_Point _RadiusPnt, int _nArcDirection, ref double _dTangentDir)
	{
		eplGeoE_CalcHorzDirBtwnTwoPnts(_RadiusPnt, _ArcPnt, ref _dTangentDir);
		if (_nArcDirection == 1)
		{
			_dTangentDir += Math.PI / 2.0;
		}
		else
		{
			_dTangentDir -= Math.PI / 2.0;
		}
		_dTangentDir = eplGeoE_NormalizeRadians(_dTangentDir);
	}

	public static int eplGeoE_CalcStationOffsetAlongLine(Eps_Point _Point, Eps_Line _Line, ref double _dStation, ref double _dOffset, double _dStaTolerance)
	{
		double _Direction = 0.0;
		double _Direction2 = 0.0;
		double num = 0.0;
		double _Distance = 0.0;
		double _Distance2 = 0.0;
		eplGeoE_InversePoints(_Line.StartPnt, _Line.EndPnt, ref _Direction, ref _Distance);
		eplGeoE_InversePoints(_Line.StartPnt, _Point, ref _Direction2, ref _Distance2);
		num = _Direction - _Direction2;
		_dStation = _Distance2 * Math.Cos(num);
		_dOffset = _Distance2 * Math.Sin(num);
		if (RevitDefines.EQ(_dStation, _Distance))
		{
			return 1700;
		}
		if (_dStation < 0.0)
		{
			if (_dStation + _dStaTolerance < 0.0)
			{
				return 1701;
			}
			_dStation = 0.0;
		}
		else if (_dStation > _Distance)
		{
			if (_dStation - _dStaTolerance > _Distance)
			{
				return 1701;
			}
			_dStation = _Distance;
		}
		return 1700;
	}

	public static int eplGeoE_InversePoints(Eps_Point _StartPnt, Eps_Point _EndPnt, ref double _Direction, ref double _Distance)
	{
		int num = 0;
		_Direction = 0.0;
		_Distance = 0.0;
		num = eplGeoE_CalcHorzDistBtwnTwoPnts(_StartPnt, _EndPnt, ref _Distance);
		if (num != 0)
		{
			return num;
		}
		return eplGeoE_CalcHorzDirBtwnTwoPnts(_StartPnt, _EndPnt, ref _Direction);
	}

	public static int eplGeoE_CalcStationOffsetAlongArc(Eps_Point _Point, Eps_Arc _Arc, ref double _dStation, ref double _dOffset)
	{
		return eplGeoE_CalcStationOffsetAlongArc(_Point, _Arc, ref _dStation, ref _dOffset, 1E-05);
	}

	public static int eplGeoE_CalcStationOffsetAlongArc(Eps_Point _Point, Eps_Arc _Arc, ref double _dStation, ref double _dOffset, double _dStaTolerance)
	{
		double _dDirection = 0.0;
		double _dDirection2 = 0.0;
		double _dDirection3 = 0.0;
		double _dDistance = 0.0;
		double _SweepAngle = 0.0;
		Eps_Point _NewPnt = new Eps_Point();
		if (eplGeoE_ArePntsDuplicates2D(_Arc.RadiusPnt, _Point))
		{
			return 1701;
		}
		if (eplGeoE_ArePntsDuplicates2D(_Arc.StartPnt, _Point))
		{
			_dStation = 0.0;
			_dOffset = 0.0;
			return 1700;
		}
		eplGeoE_CalcHorzDirBtwnTwoPnts(_Arc.RadiusPnt, _Arc.StartPnt, ref _dDirection);
		eplGeoE_CalcHorzDirBtwnTwoPnts(_Arc.RadiusPnt, _Arc.EndPnt, ref _dDirection2);
		eplGeoE_CalcHorzDirBtwnTwoPnts(_Arc.RadiusPnt, _Point, ref _dDirection3);
		eplGeoE_CalcPolarToXY(_Arc.RadiusPnt, _dDirection3, _Arc.Radius, ref _NewPnt);
		eplGeoE_CalcHorzDistBtwnTwoPnts(_Point, _NewPnt, ref _dOffset);
		eplGeoE_CalcHorzDistBtwnTwoPnts(_Arc.RadiusPnt, _Point, ref _dDistance);
		if (_Arc.Direction == -1 && _dDistance > _Arc.Radius)
		{
			_dOffset = 0.0 - _dOffset;
		}
		else if (_Arc.Direction == 1 && _dDistance < _Arc.Radius)
		{
			_dOffset = 0.0 - _dOffset;
		}
		_dDirection2 -= _dDirection;
		_dDirection3 -= _dDirection;
		_dDirection = 0.0;
		_dDirection2 = eplGeoE_NormalizeRadians(_dDirection2);
		_dDirection3 = eplGeoE_NormalizeRadians(_dDirection3);
		_dStation = _dDirection3 * _Arc.Radius;
		if (_Arc.Direction == -1)
		{
			if (_dDirection3 < _dDirection2)
			{
				eplGeoE_CalcSweepAngle(-1, _dDirection2, _dDirection3, ref _SweepAngle);
				_dStation = _SweepAngle * _Arc.Radius;
				if (_dStation - _dStaTolerance > 0.0)
				{
					eplGeoE_CalcSweepAngle(-1, _dDirection3, _dDirection, ref _SweepAngle);
					_dStation = _SweepAngle * _Arc.Radius;
					if (_dStation - _dStaTolerance > 0.0)
					{
						return 1701;
					}
					_dStation = Math.PI * 2.0 * _Arc.Radius;
				}
				else
				{
					_dStation = (Math.PI * 2.0 - _Arc.Delta) * _Arc.Radius;
				}
			}
		}
		else if (_dDirection3 > _dDirection2)
		{
			eplGeoE_CalcSweepAngle(1, _dDirection2, _dDirection3, ref _SweepAngle);
			_dStation = _SweepAngle * _Arc.Radius;
			if (_dStation - _dStaTolerance > 0.0)
			{
				eplGeoE_CalcSweepAngle(1, _dDirection3, _dDirection, ref _SweepAngle);
				_dStation = _SweepAngle * _Arc.Radius;
				if (_dStation - _dStaTolerance > 0.0)
				{
					return 1701;
				}
				_dStation = 0.0;
			}
			else
			{
				_dStation = _Arc.Delta * _Arc.Radius;
			}
		}
		if (_Arc.Direction == -1)
		{
			_dStation = Math.PI * 2.0 * _Arc.Radius - _dStation;
		}
		return 1700;
	}

	public static void eplGeoE_CalcSweepAngle(int _ArcDir, double _StartDir, double _EndDir, ref double _SweepAngle)
	{
		if (_ArcDir == -1)
		{
			_SweepAngle = eplGeoE_NormalizeRadians(_StartDir - _EndDir);
		}
		else
		{
			_SweepAngle = eplGeoE_NormalizeRadians(_EndDir - _StartDir);
		}
	}

	public static double eplGeoE_SolveArcLenByRadiusAndDelta(double _Radius, double _DeltaAngle)
	{
		double num = 0.0;
		return _Radius * _DeltaAngle;
	}

	public static int eplGeoE_ConstructOffsetArc(Eps_Arc _OrigArc, double _dOffsetDist, int _nOffsetDir, ref Eps_Arc _OffsetArc)
	{
		double _dDirection = 0.0;
		double _dDirection2 = 0.0;
		if ((_nOffsetDir == 1 && _OrigArc.Direction == -1) || (_nOffsetDir == 2 && _OrigArc.Direction == 1))
		{
			eplGeoE_CalcHorzDirBtwnTwoPnts(_OrigArc.StartPnt, _OrigArc.RadiusPnt, ref _dDirection);
			eplGeoE_CalcHorzDirBtwnTwoPnts(_OrigArc.EndPnt, _OrigArc.RadiusPnt, ref _dDirection2);
			_OffsetArc.Radius = _OrigArc.Radius - _dOffsetDist;
			if (Math.Abs(_OffsetArc.Radius - 0.0) <= 1E-05)
			{
				return 1;
			}
		}
		else
		{
			if ((_nOffsetDir != 1 || _OrigArc.Direction != 1) && (_nOffsetDir != 2 || _OrigArc.Direction != -1))
			{
				return 1;
			}
			eplGeoE_CalcHorzDirBtwnTwoPnts(_OrigArc.RadiusPnt, _OrigArc.StartPnt, ref _dDirection);
			eplGeoE_CalcHorzDirBtwnTwoPnts(_OrigArc.RadiusPnt, _OrigArc.EndPnt, ref _dDirection2);
			_OffsetArc.Radius = _OrigArc.Radius + _dOffsetDist;
		}
		Eps_Point _NewPnt = new Eps_Point();
		Eps_Point _NewPnt2 = new Eps_Point();
		eplGeoE_CalcPolarToXY(_OrigArc.StartPnt, _dDirection, _dOffsetDist, ref _NewPnt);
		eplGeoE_CalcPolarToXY(_OrigArc.EndPnt, _dDirection2, _dOffsetDist, ref _NewPnt2);
		_OffsetArc.StartPnt = _NewPnt;
		_OffsetArc.EndPnt = _NewPnt2;
		_OffsetArc.RadiusPnt = _OrigArc.RadiusPnt;
		_OffsetArc.Delta = _OrigArc.Delta;
		_OffsetArc.Direction = _OrigArc.Direction;
		return 0;
	}

	public static int eplGeoE_ConstructOffsetLine(Eps_Line _OrigLine, double _dOffsetDist, int _nOffsetDir, ref Eps_Line _OffsetLine)
	{
		_OffsetLine = new Eps_Line();
		double _dDirection = 0.0;
		double num = 0.0;
		eplGeoE_CalcHorzDirBtwnTwoPnts(_OrigLine.StartPnt, _OrigLine.EndPnt, ref _dDirection);
		switch (_nOffsetDir)
		{
		case 1:
			num = eplGeoE_NormalizeRadians(_dDirection - Math.PI / 2.0);
			break;
		case 2:
			num = eplGeoE_NormalizeRadians(_dDirection + Math.PI / 2.0);
			break;
		default:
			return 1;
		}
		Eps_Point _NewPnt = new Eps_Point();
		Eps_Point _NewPnt2 = new Eps_Point();
		eplGeoE_CalcPolarToXY(_OrigLine.StartPnt, num, _dOffsetDist, ref _NewPnt);
		eplGeoE_CalcPolarToXY(_OrigLine.EndPnt, num, _dOffsetDist, ref _NewPnt2);
		_OffsetLine.StartPnt = _NewPnt;
		_OffsetLine.EndPnt = _NewPnt2;
		return 0;
	}

	public static int eplGeoE_CalcMidPointBetweenTwoPoints(Eps_Point _FirstPoint, Eps_Point _SecondPoint, ref Eps_Point _MidPoint)
	{
		_MidPoint = new Eps_Point();
		double _dDistance = 0.0;
		double _dDirection = 0.0;
		eplGeoE_CalcHorzDistBtwnTwoPnts(_FirstPoint, _SecondPoint, ref _dDistance);
		if (_dDistance == 0.0)
		{
			return 6;
		}
		if (eplGeoE_CalcHorzDirBtwnTwoPnts(_FirstPoint, _SecondPoint, ref _dDirection) != 0)
		{
			return 6;
		}
		eplGeoE_CalcPolarToXY(_FirstPoint, _dDirection, _dDistance / 2.0, ref _MidPoint);
		return 0;
	}

	public static int eplGeoE_IntersectTwoDirs2D(Eps_Point _FirstPnt, double _dFirstDir, Eps_Point _SecondPnt, double _dSecondDir, ref Eps_Point _InterPnt, double _dTolerance)
	{
		_InterPnt = new Eps_Point();
		double _dDirection = 0.0;
		double _dDirection2 = 0.0;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = _dTolerance;
		double num8 = _dTolerance;
		double num9 = 1E-10;
		num7 = eplCalE_CalcPointTolerance(_FirstPnt, _dTolerance, _Is3D: false);
		num8 = eplCalE_CalcPointTolerance(_SecondPnt, _dTolerance, _Is3D: false);
		if (RevitDefines.GT(num7, num9, 0.0))
		{
			num9 = num7;
		}
		if (RevitDefines.GT(num8, num9, 0.0))
		{
			num9 = num8;
		}
		_InterPnt.z = _FirstPnt.z;
		_dFirstDir = eplGeoE_NormalizeRadians(_dFirstDir);
		_dSecondDir = eplGeoE_NormalizeRadians(_dSecondDir);
		if (eplGeoE_AreLinesParallel(_dFirstDir, _dSecondDir, num9))
		{
			return 1103;
		}
		if (eplGeoE_ArePntsDuplicates2D(_FirstPnt, _SecondPnt, _dTolerance))
		{
			_InterPnt.x = _FirstPnt.x;
			_InterPnt.y = _FirstPnt.y;
			return 1104;
		}
		if (RevitDefines.EQ(_dFirstDir, Math.PI / 2.0, 1E-10) || RevitDefines.EQ(_dFirstDir, 4.71238898038469, 1E-10))
		{
			_InterPnt.x = _FirstPnt.x;
			num6 = Math.Tan(_dSecondDir);
			num4 = _SecondPnt.y - num6 * _SecondPnt.x;
			_InterPnt.y = num4 + num6 * _InterPnt.x;
		}
		else if (RevitDefines.EQ(_dSecondDir, Math.PI / 2.0, 1E-10) || RevitDefines.EQ(_dSecondDir, 4.71238898038469, 1E-10))
		{
			_InterPnt.x = _SecondPnt.x;
			num5 = Math.Tan(_dFirstDir);
			num3 = _FirstPnt.y - num5 * _FirstPnt.x;
			_InterPnt.y = num5 * _InterPnt.x + num3;
		}
		else
		{
			num5 = Math.Tan(_dFirstDir);
			num6 = Math.Tan(_dSecondDir);
			num3 = _FirstPnt.y - num5 * _FirstPnt.x;
			num4 = _SecondPnt.y - num6 * _SecondPnt.x;
			_InterPnt.x = (0.0 - (num3 - num4)) / (num5 - num6);
			_InterPnt.y = num5 * _InterPnt.x + num3;
		}
		if (eplGeoE_CalcHorzDirBtwnTwoPnts(_InterPnt, _FirstPnt, ref _dDirection) == 1105)
		{
			_dDirection = 0.0;
		}
		if (eplGeoE_CalcHorzDirBtwnTwoPnts(_InterPnt, _SecondPnt, ref _dDirection2) == 1105)
		{
			_dDirection2 = 0.0;
		}
		num = Math.Abs(_dDirection - _dFirstDir);
		num2 = Math.Abs(_dDirection2 - _dSecondDir);
		if (num <= 1E-10 && num2 <= 1E-10)
		{
			return 1106;
		}
		if (num <= 1E-10 && num2 > 1E-10)
		{
			return 1107;
		}
		if (num > 1E-10 && num2 <= 1E-10)
		{
			return 1112;
		}
		return 1104;
	}

	public static double eplCalE_CalcPointTolerance(Eps_Point _Point, double _Tolerance, bool _Is3D)
	{
		double num = _Tolerance;
		double num2 = _Tolerance;
		double num3 = _Tolerance;
		double num4 = _Tolerance;
		if (RevitDefines.EQ(_Tolerance, 1E-05, 0.0))
		{
			if (Math.Abs(_Point.x) >= 100000.0)
			{
				num2 = eplCalE_CalcTolerance(_Point.x);
			}
			if (Math.Abs(_Point.y) >= 100000.0)
			{
				num3 = eplCalE_CalcTolerance(_Point.y);
			}
			if (_Is3D && Math.Abs(_Point.z) >= 100000.0)
			{
				num4 = eplCalE_CalcTolerance(_Point.z);
			}
			if (RevitDefines.GT(num2, num, 0.0))
			{
				num = num2;
			}
			if (RevitDefines.GT(num3, num, 0.0))
			{
				num = num3;
			}
			if (_Is3D && RevitDefines.GT(num4, num, 0.0))
			{
				num = num4;
			}
		}
		return num;
	}

	public static double eplCalE_CalcTolerance(double _Value)
	{
		double num = 1.0;
		long num2 = 0L;
		num2 = GetBase10(_Value);
		return Math.Pow(10.0, 0.0 - (15.0 - (double)num2));
	}

	public static int GetBase10(double _Value)
	{
		double num = 0.0;
		int result = 0;
		if (RevitDefines.EQ(_Value, 0.0, 0.0))
		{
			return 1;
		}
		num = Math.Log10(Math.Abs(_Value));
		if (RevitDefines.GE(num, 0.0, 0.0))
		{
			result = (int)num + 1;
		}
		else if (RevitDefines.LT(num, 0.0, 0.0))
		{
			result = (int)num;
		}
		return result;
	}

	public static bool eplGeoE_AreLinesParallel(Eps_Line _FirstLine, Eps_Line _SecondLine, double _dTolerance)
	{
		double _dDirection = 0.0;
		double _dDirection2 = 0.0;
		eplGeoE_CalcHorzDirBtwnTwoPnts(_FirstLine.StartPnt, _FirstLine.EndPnt, ref _dDirection);
		eplGeoE_CalcHorzDirBtwnTwoPnts(_SecondLine.StartPnt, _SecondLine.EndPnt, ref _dDirection2);
		return eplGeoE_AreLinesParallel(_dDirection, _dDirection2, _dTolerance);
	}

	public static bool eplGeoE_AreLinesParallel(double _dFirstLineDir, double _dSecondLineDir, double _dTolerance)
	{
		if (Math.Abs(_dFirstLineDir - _dSecondLineDir) <= _dTolerance)
		{
			return true;
		}
		if (Math.Abs(eplGeoE_NormalizeRadians(_dFirstLineDir + Math.PI) - _dSecondLineDir) <= _dTolerance)
		{
			return true;
		}
		return false;
	}

	public static bool eplGeoE_ArePntsDuplicates3D(Eps_Point _FirstCheckPnt, Eps_Point _SecondCheckPnt)
	{
		return eplGeoE_ArePntsDuplicates3D(_FirstCheckPnt, _SecondCheckPnt, 1E-05);
	}

	public static bool eplGeoE_ArePntsDuplicates3D(Eps_Point _FirstCheckPnt, Eps_Point _SecondCheckPnt, double _dTolerance)
	{
		bool result = false;
		double num = _FirstCheckPnt.x - _SecondCheckPnt.x;
		double num2 = _FirstCheckPnt.y - _SecondCheckPnt.y;
		double num3 = _FirstCheckPnt.z - _SecondCheckPnt.z;
		if (num * num + num2 * num2 + num3 * num3 <= _dTolerance * _dTolerance)
		{
			result = true;
		}
		return result;
	}

	public static void eplGeoE_CalcArcStructFromPCAndPTAndRP(Eps_Point _PntOfArc, Eps_Point _RadiusPoint, Eps_Point _PntOfTangency, int _nDirection, ref Eps_Arc _ArcStruct)
	{
		_ArcStruct = new Eps_Arc();
		double _dDirection = 0.0;
		double _dDirection2 = 0.0;
		double _dDistance = 0.0;
		double _dDistance2 = 0.0;
		_ArcStruct.StartPnt = _PntOfArc;
		_ArcStruct.RadiusPnt = _RadiusPoint;
		_ArcStruct.EndPnt = _PntOfTangency;
		eplGeoE_CalcHorzDistBtwnTwoPnts(_ArcStruct.RadiusPnt, _ArcStruct.StartPnt, ref _dDistance);
		eplGeoE_CalcHorzDistBtwnTwoPnts(_ArcStruct.RadiusPnt, _ArcStruct.EndPnt, ref _dDistance2);
		if (Math.Abs(_dDistance - _dDistance2) <= 1E-05)
		{
			_ArcStruct.Radius = _dDistance;
		}
		else
		{
			eplGeoE_CalcHorzDirBtwnTwoPnts(_ArcStruct.RadiusPnt, _ArcStruct.EndPnt, ref _dDirection2);
			Eps_Point _NewPnt = new Eps_Point();
			eplGeoE_CalcPolarToXY(_ArcStruct.RadiusPnt, _dDirection2, _dDistance, ref _NewPnt);
			_ArcStruct.EndPnt = _NewPnt;
			_ArcStruct.Radius = _dDistance;
		}
		_ArcStruct.Direction = _nDirection;
		eplGeoE_CalcHorzDirBtwnTwoPnts(_ArcStruct.RadiusPnt, _ArcStruct.StartPnt, ref _dDirection);
		eplGeoE_CalcHorzDirBtwnTwoPnts(_ArcStruct.RadiusPnt, _ArcStruct.EndPnt, ref _dDirection2);
		double _SweepAngle = 0.0;
		eplGeoE_CalcSweepAngle(_ArcStruct.Direction, _dDirection, _dDirection2, ref _SweepAngle);
		_ArcStruct.Delta = _SweepAngle;
	}

	public static void eplGeoE_FindMinMaxOfArc(Eps_Arc _Arc, ref Eps_Point _MinPnt, ref Eps_Point _MaxPnt)
	{
		_MinPnt = new Eps_Point();
		_MaxPnt = new Eps_Point();
		int num = 0;
		Eps_Point _NewPnt = new Eps_Point();
		_MinPnt.x = double.MaxValue;
		_MaxPnt.x = double.MinValue;
		_MinPnt.y = double.MaxValue;
		_MaxPnt.y = double.MinValue;
		_MinPnt.z = double.MaxValue;
		_MaxPnt.z = double.MinValue;
		CalcMinMax(_Arc.StartPnt, ref _MinPnt, ref _MaxPnt);
		CalcMinMax(_Arc.EndPnt, ref _MinPnt, ref _MaxPnt);
		for (num = 0; num < 4; num++)
		{
			eplGeoE_CalcPolarToXY(_Arc.RadiusPnt, eplGeoE_NormalizeRadians(0.0 + (double)num * (Math.PI / 2.0)), _Arc.Radius, ref _NewPnt);
			if (eplGeoE_IsPointOnArc2D(_NewPnt, _Arc, 1E-05))
			{
				CalcMinMax(_NewPnt, ref _MinPnt, ref _MaxPnt);
			}
		}
	}

	public static void CalcMinMax(Eps_Point pPoint, ref Eps_Point pMinPnt, ref Eps_Point pMaxPnt)
	{
		pMinPnt.x = Math.Min(pMinPnt.x, pPoint.x);
		pMaxPnt.x = Math.Max(pMaxPnt.x, pPoint.x);
		pMinPnt.y = Math.Min(pMinPnt.y, pPoint.y);
		pMaxPnt.y = Math.Max(pMaxPnt.y, pPoint.y);
		pMinPnt.z = Math.Min(pMinPnt.z, pPoint.z);
		pMaxPnt.z = Math.Max(pMaxPnt.z, pPoint.z);
	}

	public static bool eplGeoE_IsPointOnArc2D(Eps_Point _Point, Eps_Arc _Arc, double _dPointTolerance)
	{
		double _dDistance = 0.0;
		double _dDirection = 0.0;
		double _dDirection2 = 0.0;
		double _SweepAngle = 0.0;
		bool result = false;
		eplGeoE_CalcHorzDistBtwnTwoPnts(_Arc.RadiusPnt, _Point, ref _dDistance, _dPointTolerance);
		if (RevitDefines.LT(_dDistance, _Arc.Radius, _dPointTolerance) || RevitDefines.GT(_dDistance, _Arc.Radius, _dPointTolerance))
		{
			result = false;
		}
		else if (eplGeoE_ArePntsDuplicates2D(_Arc.StartPnt, _Arc.EndPnt, _dPointTolerance))
		{
			result = true;
		}
		else
		{
			if (_Arc.Direction == -1)
			{
				eplGeoE_CalcHorzDirBtwnTwoPnts(_Arc.RadiusPnt, _Point, ref _dDirection);
				eplGeoE_CalcHorzDirBtwnTwoPnts(_Arc.RadiusPnt, _Arc.EndPnt, ref _dDirection2);
			}
			else
			{
				eplGeoE_CalcHorzDirBtwnTwoPnts(_Arc.RadiusPnt, _Arc.StartPnt, ref _dDirection);
				eplGeoE_CalcHorzDirBtwnTwoPnts(_Arc.RadiusPnt, _Point, ref _dDirection2);
			}
			eplGeoE_CalcSweepAngle(_Arc.Direction, _dDirection, _dDirection2, ref _SweepAngle);
			if (RevitDefines.LE(_SweepAngle, _Arc.Delta, 1E-10))
			{
				result = true;
			}
		}
		return result;
	}

	public static bool eplGeoE_IsPointOnLine2D(Eps_Point _Point, Eps_Line _Line, double _dPointTolerance)
	{
		Eps_Point _ProjPnt = new Eps_Point();
		if (RevitDefines.LT(_Point.x, Math.Min(_Line.StartPnt.x, _Line.EndPnt.x), _dPointTolerance) || RevitDefines.GT(_Point.x, Math.Max(_Line.StartPnt.x, _Line.EndPnt.x), _dPointTolerance) || RevitDefines.LT(_Point.y, Math.Min(_Line.StartPnt.y, _Line.EndPnt.y), _dPointTolerance) || RevitDefines.GT(_Point.y, Math.Max(_Line.StartPnt.y, _Line.EndPnt.y), _dPointTolerance))
		{
			return false;
		}
		if (eplGeoE_ProjectPointOnLine2D(_Point, _Line, ref _ProjPnt, ref _dPointTolerance) == 1700 && eplGeoE_ArePntsDuplicates2D(_Point, _ProjPnt, _dPointTolerance))
		{
			return true;
		}
		return false;
	}

	public static int eplGeoE_ProjectPointOnLine2D(Eps_Point _PointToProj, Eps_Line _Line, ref Eps_Point _ProjPnt, ref double _dPntTolerance)
	{
		double _Direction = 0.0;
		double _Direction2 = 0.0;
		double num = 0.0;
		double _Distance = 0.0;
		double _Distance2 = 0.0;
		double num2 = 0.0;
		eplGeoE_InversePoints(_Line.StartPnt, _Line.EndPnt, ref _Direction, ref _Distance);
		if (eplGeoE_InversePoints(_Line.StartPnt, _PointToProj, ref _Direction2, ref _Distance2) == 1105)
		{
			_ProjPnt = _PointToProj;
			return 1700;
		}
		num = _Direction - _Direction2;
		num2 = _Distance2 * Math.Cos(num);
		eplGeoE_CalcPolarToXY(_Line.StartPnt, _Direction, num2, ref _ProjPnt);
		if (num2 < 0.0)
		{
			if (!eplGeoE_ArePntsDuplicates2D(_Line.StartPnt, _ProjPnt, _dPntTolerance))
			{
				return 1701;
			}
			_ProjPnt = _Line.StartPnt;
		}
		else if (num2 > _Distance)
		{
			if (!eplGeoE_ArePntsDuplicates2D(_Line.EndPnt, _ProjPnt, _dPntTolerance))
			{
				return 1701;
			}
			_ProjPnt = _Line.EndPnt;
		}
		return 1700;
	}

	public static bool eplGeoE_DoBoundingBoxesOverlap(Eps_Line _LineOne, Eps_Line _LineTwo, double _dPointTolerance)
	{
		if (RevitDefines.LT(Math.Max(_LineOne.StartPnt.x, _LineOne.EndPnt.x), Math.Min(_LineTwo.StartPnt.x, _LineTwo.EndPnt.x), _dPointTolerance))
		{
			return false;
		}
		if (RevitDefines.GT(Math.Min(_LineOne.StartPnt.x, _LineOne.EndPnt.x), Math.Max(_LineTwo.StartPnt.x, _LineTwo.EndPnt.x), _dPointTolerance))
		{
			return false;
		}
		if (RevitDefines.LT(Math.Max(_LineOne.StartPnt.y, _LineOne.EndPnt.y), Math.Min(_LineTwo.StartPnt.y, _LineTwo.EndPnt.y), _dPointTolerance))
		{
			return false;
		}
		if (RevitDefines.GT(Math.Min(_LineOne.StartPnt.y, _LineOne.EndPnt.y), Math.Max(_LineTwo.StartPnt.y, _LineTwo.EndPnt.y), _dPointTolerance))
		{
			return false;
		}
		return true;
	}

	public static double eplGeoE_CalcPlaneDistBtwnTwoPnts(double _dFirstPntRightLeft, double _dFirstPntUpDown, double _dSecondPntRightLeft, double _dSecondPntUpDown)
	{
		double num = 0.0;
		double x = _dSecondPntRightLeft - _dFirstPntRightLeft;
		double x2 = _dSecondPntUpDown - _dFirstPntUpDown;
		return Math.Sqrt(Math.Pow(x, 2.0) + Math.Pow(x2, 2.0));
	}

	public static void eplGeoE_MakeVector(Eps_Point _InitialPnt, Eps_Point _FinalPnt, ref Eps_Point _Vector)
	{
		_Vector = new Eps_Point();
		_Vector.x = _FinalPnt.x - _InitialPnt.x;
		_Vector.y = _FinalPnt.y - _InitialPnt.y;
		_Vector.z = _FinalPnt.z - _InitialPnt.z;
	}

	public static int eplGeoE_CheckIfLineInsideLine(Eps_Line _LineOne, Eps_Line _LineTwo, double _dPointTolerance)
	{
		Eps_Point _ProjPnt = new Eps_Point();
		if (eplGeoE_ProjectPointOnLine2D(_LineOne.StartPnt, _LineTwo, ref _ProjPnt, ref _dPointTolerance) == 1701)
		{
			return 1701;
		}
		if (eplGeoE_ProjectPointOnLine2D(_LineOne.EndPnt, _LineTwo, ref _ProjPnt, ref _dPointTolerance) == 1701)
		{
			return 1701;
		}
		return 1700;
	}

	public static int eplGeoE_SolveDeltaByChordLenAndRadius(double _dChordLength, double _dRadius, ref double _dDeltaAngle)
	{
		_dDeltaAngle = 0.0;
		if (_dChordLength > 2.0 * _dRadius)
		{
			return 1210;
		}
		_dDeltaAngle = 2.0 * Math.Asin(_dChordLength / (2.0 * _dRadius));
		return 0;
	}

	public static double eplGeoE_SolveTanLenByRadiusAndDelta(double _dRadius, double _dDeltaAngle)
	{
		double num = 0.0;
		if (Math.Abs(_dDeltaAngle - Math.PI) < 1E-10)
		{
			return 0.0;
		}
		return _dRadius * Math.Tan(_dDeltaAngle / 2.0);
	}

	public static void eplGeoE_CalcRadiusPointFromPCAndPIAndPT(Eps_Point _PntOfCurve, Eps_Point _PntOfInter, Eps_Point _PntOfTangency, ref Eps_Point _RadiusPoint)
	{
		double _dDirection = 0.0;
		double num = 0.0;
		double num2 = 0.0;
		eplGeoE_CalcHorzDirBtwnTwoPnts(_PntOfCurve, _PntOfInter, ref _dDirection);
		num = _dDirection - Math.PI / 2.0;
		num = eplGeoE_NormalizeRadians(num);
		eplGeoE_CalcHorzDirBtwnTwoPnts(_PntOfTangency, _PntOfInter, ref _dDirection);
		num2 = _dDirection + Math.PI / 2.0;
		num2 = eplGeoE_NormalizeRadians(num2);
		eplGeoE_IntersectTwoDirs2D(_PntOfCurve, num, _PntOfTangency, num2, ref _RadiusPoint, 0.0);
	}

	public static void eplGeoE_RotatePoint(ref Eps_Point _Point, Eps_Point _ReferencePoint, double _dRotation)
	{
		if (!_dRotation.Equals(0.0))
		{
			double _pPolarDirection = 0.0;
			double _pPolarDistance = 0.0;
			eplGeoE_CalcXYToPolar(_ReferencePoint, _Point, ref _pPolarDirection, ref _pPolarDistance);
			eplGeoE_CalcPolarToXY(_ReferencePoint, _pPolarDirection + _dRotation, _pPolarDistance, ref _Point);
		}
	}

	public static void eplGeoE_CalcXYToPolar(Eps_Point _pStartPnt, Eps_Point _pSecondPnt, ref double _pPolarDirection, ref double _pPolarDistance)
	{
		eplGeoE_CalcHorzDistBtwnTwoPnts(_pStartPnt, _pSecondPnt, ref _pPolarDistance);
		eplGeoE_CalcHorzDirBtwnTwoPnts(_pStartPnt, _pSecondPnt, ref _pPolarDirection);
	}

	public static void eplGeoE_CalcPolarToXYZ(Eps_Point _StartPnt, double _dHorzPolarDirection, double _dVertPolarDirection, double _dPolarSlopeDistance, ref Eps_Point _NewPnt)
	{
		double num = 0.0;
		_NewPnt = new Eps_Point();
		num = Math.Cos(_dVertPolarDirection) * _dPolarSlopeDistance;
		_NewPnt.x = _StartPnt.x + Math.Cos(_dHorzPolarDirection) * num;
		_NewPnt.y = _StartPnt.y + Math.Sin(_dHorzPolarDirection) * num;
		_NewPnt.z = _StartPnt.z + Math.Sin(_dVertPolarDirection) * _dPolarSlopeDistance;
	}

	public static void eplGeoE_CalcXYZToPolar(Eps_Point _StartPnt, Eps_Point _EndPnt, ref double _dHorzPolarDirection, ref double _dVertPolarDirection, ref double _dPolarSlopeDistance)
	{
		_dHorzPolarDirection = 0.0;
		_dVertPolarDirection = 0.0;
		_dPolarSlopeDistance = 0.0;
		eplGeoE_CalcSlopeDistBtwnTwoPnts(_StartPnt, _EndPnt, ref _dPolarSlopeDistance, 1E-05);
		eplGeoE_CalcHorzDirBtwnTwoPnts(_StartPnt, _EndPnt, ref _dHorzPolarDirection);
		eplGeoE_CalcVertDirBtwnTwoPnts(_StartPnt, _EndPnt, ref _dVertPolarDirection, 0.0);
	}

	public static int eplGeoE_CalcSlopeDistBtwnTwoPnts(Eps_Point _FirstPnt, Eps_Point _SecondPnt, ref double _dDistance, double MinDist)
	{
		double num = 0.0;
		num = _FirstPnt.XYZ.DistanceTo(_SecondPnt.XYZ);
		if (num <= MinDist)
		{
			_dDistance = 0.0;
			return 1113;
		}
		_dDistance = num;
		return 0;
	}

	public static int eplGeoE_CalcVertDirBtwnTwoPnts(Eps_Point _FirstPnt, Eps_Point _SecondPnt, ref double _dDirection, double _dMinDist)
	{
		double num = 0.0;
		double num2 = 0.0;
		if (eplGeoE_ArePntsDuplicates3D(_FirstPnt, _SecondPnt, _dMinDist))
		{
			return 1113;
		}
		num = _SecondPnt.z - _FirstPnt.z;
		num2 = eplGeoE_CalcPlaneDistBtwnTwoPnts(_FirstPnt.x, _FirstPnt.y, _SecondPnt.x, _SecondPnt.y);
		if (num != 0.0 || num2 != 0.0)
		{
			_dDirection = Math.Atan2(num, num2);
			return 0;
		}
		return 1113;
	}

	public static int eplGeoE_CalcTangentInDirOfArc(Eps_Arc _Arc, ref double _dTangentInDir)
	{
		Eps_Point _PIPoint = new Eps_Point();
		int num = 0;
		Eps_Arc eps_Arc = new Eps_Arc();
		switch (eplGeoE_SolvePIByArcStruct(_Arc, ref _PIPoint))
		{
		case 0:
			eplGeoE_CalcHorzDirBtwnTwoPnts(_Arc.StartPnt, _PIPoint, ref _dTangentInDir);
			break;
		case 1211:
			return 1211;
		case 1212:
			eps_Arc = _Arc;
			if (_Arc.Direction == -1)
			{
				eps_Arc.Direction = 1;
			}
			else
			{
				eps_Arc.Direction = -1;
			}
			eps_Arc.Delta = Math.PI * 2.0 - _Arc.Delta;
			eplGeoE_SolvePIByArcStruct(eps_Arc, ref _PIPoint);
			eplGeoE_CalcHorzDirBtwnTwoPnts(_Arc.StartPnt, _PIPoint, ref _dTangentInDir);
			break;
		default:
			return 1;
		}
		return 0;
	}

	public static int eplGeoE_SolvePIByArcStruct(Eps_Arc _ArcStruct, ref Eps_Point _PIPoint)
	{
		double _dDirection = 0.0;
		double num = 0.0;
		double num2 = 0.0;
		if (Math.Abs(_ArcStruct.Delta - Math.PI) < 1E-10)
		{
			return 1211;
		}
		if (_ArcStruct.Delta > Math.PI)
		{
			return 1212;
		}
		eplGeoE_CalcHorzDirBtwnTwoPnts(_ArcStruct.RadiusPnt, _ArcStruct.StartPnt, ref _dDirection);
		num2 = ((_ArcStruct.Direction != -1) ? (_dDirection + _ArcStruct.Delta / 2.0) : (_dDirection - _ArcStruct.Delta / 2.0));
		num = eplGeoE_SolveExternByRadiusAndDelta(_ArcStruct.Radius, _ArcStruct.Delta);
		eplGeoE_CalcPolarToXY(_ArcStruct.RadiusPnt, num2, _ArcStruct.Radius + num, ref _PIPoint);
		return 0;
	}

	public static double eplGeoE_SolveExternByRadiusAndDelta(double _dRadius, double _dDeltaAngle)
	{
		double num = 0.0;
		if (Math.Abs(_dDeltaAngle - Math.PI) < 1E-10)
		{
			return 0.0;
		}
		return Math.Abs(_dRadius * (1.0 / Math.Cos(_dDeltaAngle / 2.0) - 1.0));
	}

	public static int eplGeoE_CalcTangentOutDirOfArc(Eps_Arc _Arc, ref double _dTangentOutDir)
	{
		Eps_Point _PIPoint = new Eps_Point();
		int num = 0;
		Eps_Arc eps_Arc = new Eps_Arc();
		switch (eplGeoE_SolvePIByArcStruct(_Arc, ref _PIPoint))
		{
		case 0:
			eplGeoE_CalcHorzDirBtwnTwoPnts(_PIPoint, _Arc.EndPnt, ref _dTangentOutDir);
			break;
		case 1211:
			return 1211;
		case 1212:
			eps_Arc = _Arc;
			if (_Arc.Direction == -1)
			{
				eps_Arc.Direction = 1;
			}
			else
			{
				eps_Arc.Direction = -1;
			}
			eps_Arc.Delta = Math.PI * 2.0 - _Arc.Delta;
			eplGeoE_SolvePIByArcStruct(eps_Arc, ref _PIPoint);
			eplGeoE_CalcHorzDirBtwnTwoPnts(_PIPoint, eps_Arc.EndPnt, ref _dTangentOutDir);
			break;
		default:
			return 1;
		}
		return 0;
	}

	public static void eplGeoE_CalcArcChordArea(Eps_Arc _Arc, ref double _dChordArea)
	{
		double num = 0.0;
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		Eps_Point _NewPnt = new Eps_Point();
		double _dDistance2 = 0.0;
		double num2 = 0.0;
		num = _Arc.Delta / 2.0 * Math.Pow(_Arc.Radius, 2.0);
		eplGeoE_CalcHorzDirBtwnTwoPnts(_Arc.StartPnt, _Arc.EndPnt, ref _dDirection);
		eplGeoE_CalcHorzDistBtwnTwoPnts(_Arc.StartPnt, _Arc.EndPnt, ref _dDistance);
		eplGeoE_CalcPolarToXY(_Arc.StartPnt, _dDirection, _dDistance / 2.0, ref _NewPnt);
		eplGeoE_CalcHorzDistBtwnTwoPnts(_NewPnt, _Arc.RadiusPnt, ref _dDistance2);
		num2 = _dDistance * _dDistance2 / 2.0;
		if (_Arc.Delta > Math.PI)
		{
			_dChordArea = num + num2;
		}
		else
		{
			_dChordArea = num - num2;
		}
		if (_Arc.Direction == -1)
		{
			_dChordArea = 0.0 - _dChordArea;
		}
	}

	public static double eplCalE_fmod(double _dFirstOperand, double _dSecondOperand)
	{
		return eplCalE_fmod(_dFirstOperand, _dSecondOperand, 1E-05);
	}

	public static double eplCalE_fmod(double _dFirstOperand, double _dSecondOperand, double _dTolerance)
	{
		double num = 0.0;
		if (RevitDefines.EQ(_dSecondOperand, 0.0, _dTolerance))
		{
			num = 0.0;
		}
		else
		{
			num = _dFirstOperand % _dSecondOperand;
			if (Math.Abs(Math.Abs(num) - Math.Abs(_dSecondOperand)) < _dTolerance || RevitDefines.EQ(num, 0.0, _dTolerance))
			{
				num = 0.0;
			}
		}
		return num;
	}

	public static double eplCalE_RoundValue(double _dValue, int _nPrecision)
	{
		return Math.Round(_dValue, _nPrecision);
	}

	public static double eplCalE_CalcComponentKOfCrossProduct(Eps_Point _FirstPoint, Eps_Point _SecondPoint)
	{
		return _FirstPoint.x * _SecondPoint.y - _SecondPoint.x * _FirstPoint.y;
	}

	public static bool eplGeoE_IsLegListInsideLegList(Eps_Line[] _aTestLines, int _nTestLineCount, Eps_Arc[] _aTestArcs, int _nTestArcCount, int[] _aTestLegOrder, Eps_Line[] _aLines, int _nLineCount, Eps_Arc[] _aArcs, int _nArcCount, int[] _aLegOrder, double _dTolerance)
	{
		Eps_Leg _LegList = null;
		Eps_Leg _LegList2 = null;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < _nTestLineCount + _nTestArcCount; i++)
		{
			if (_aTestLegOrder[i] == 0)
			{
				eplDsmE_AddLineToLegList(_aTestLines[num], ref _LegList);
				num++;
			}
			else if (_aTestLegOrder[i] == 1)
			{
				eplDsmE_AddArcToLegList(_aTestArcs[num2], ref _LegList);
				num2++;
			}
		}
		num = 0;
		num2 = 0;
		for (int j = 0; j < _nLineCount + _nArcCount; j++)
		{
			if (_aLegOrder[j] == 0)
			{
				eplDsmE_AddLineToLegList(_aLines[num], ref _LegList2);
				num++;
			}
			else if (_aLegOrder[j] == 1)
			{
				eplDsmE_AddArcToLegList(_aArcs[num2], ref _LegList2);
				num2++;
			}
		}
		return eplGeoE_IsLegListInsideLegList(_LegList, _LegList2, _dTolerance);
	}

	public static bool eplGeoE_IsLegListInsideLegList(Eps_Leg _pTestLegList, Eps_Leg _pLegList, double _dTolerance)
	{
		bool result = true;
		Eps_Leg eps_Leg = new Eps_Leg();
		Eps_Point _MidPnt = new Eps_Point();
		double _dDirection = 0.0;
		if (_pTestLegList == null || _pLegList == null)
		{
			return false;
		}
		if (!eplGeoE_DoLegListsIntersect(_pTestLegList, _pLegList, _dTolerance))
		{
			for (eps_Leg = _pTestLegList; eps_Leg != null; eps_Leg = eps_Leg.Next)
			{
				if (eps_Leg.Line != null)
				{
					if (eplGeoE_ArePntsDuplicates2D(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, _dTolerance))
					{
						_MidPnt = eps_Leg.Line.StartPnt;
					}
					else
					{
						eplGeoE_CalcMidPointBetweenTwoPoints(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _MidPnt);
					}
				}
				else
				{
					if (eps_Leg.Arc == null)
					{
						continue;
					}
					if (eplGeoE_ArePntsDuplicates2D(eps_Leg.Arc.StartPnt, eps_Leg.Arc.EndPnt, _dTolerance))
					{
						if (eplGeoE_ArePntsDuplicates2D(eps_Leg.Arc.StartPnt, eps_Leg.Arc.RadiusPnt, _dTolerance))
						{
							_MidPnt = eps_Leg.Arc.StartPnt;
						}
						else
						{
							eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Arc.StartPnt, eps_Leg.Arc.RadiusPnt, ref _dDirection);
							eplGeoE_CalcPolarToXY(eps_Leg.Arc.RadiusPnt, _dDirection, eps_Leg.Arc.Radius, ref _MidPnt);
						}
					}
					else
					{
						eplGeoE_SolveMidPntByArcStruct(eps_Leg.Arc, ref _MidPnt);
					}
				}
				if (eplGeoE_IsPointInsideLegList(_pLegList, _MidPnt, _dTolerance, null, null) == 6)
				{
					result = false;
					break;
				}
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	public static bool eplGeoE_DoLegListsIntersect(Eps_Leg _pLeg1, Eps_Leg _pLeg2, double _dTolerance)
	{
		Eps_Point[] _IntPoints = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		int _nNumberOfIntersections = 0;
		return DoLegsIntersect(_pLeg1, _pLeg2, ref _IntPoints, ref _nNumberOfIntersections, _dTolerance);
	}

	public static bool DoLegsIntersect(Eps_Leg _Leg1, Eps_Leg _Leg2, ref Eps_Point[] _IntPoints, ref int _nNumberOfIntersections, double _dTolerance)
	{
		_IntPoints = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		Eps_Leg eps_Leg = new Eps_Leg();
		Eps_Leg eps_Leg2 = new Eps_Leg();
		for (eps_Leg = _Leg1; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			for (eps_Leg2 = _Leg2; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
			{
				if (eps_Leg.Line != null)
				{
					if (eps_Leg2.Line != null)
					{
						if (DoLinesIntersect(eps_Leg.Line, eps_Leg2.Line, ref _IntPoints, ref _nNumberOfIntersections, _dTolerance))
						{
							return true;
						}
					}
					else if (eps_Leg2.Arc != null && DoesLineArcIntersect(eps_Leg.Line, eps_Leg2.Arc, ref _IntPoints, ref _nNumberOfIntersections, _dTolerance))
					{
						return true;
					}
				}
				else if (eps_Leg.Arc != null)
				{
					if (eps_Leg2.Line != null)
					{
						if (DoesLineArcIntersect(eps_Leg2.Line, eps_Leg.Arc, ref _IntPoints, ref _nNumberOfIntersections, _dTolerance))
						{
							return true;
						}
					}
					else if (eps_Leg2.Arc != null && DoArcsIntersect(eps_Leg.Arc, eps_Leg2.Arc, ref _IntPoints, ref _nNumberOfIntersections, _dTolerance))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool DoLinesIntersect(Eps_Line _Line1, Eps_Line _Line2, ref Eps_Point[] _IntPoints, ref int _nNumberOfIntersections, double _dTolerance)
	{
		int num = 0;
		num = eplGeoE_IntersectTwoLines(_Line1, _Line2, ref _IntPoints, ref _nNumberOfIntersections, _dTolerance);
		if (num == 1606)
		{
			return true;
		}
		return false;
	}

	public static bool DoesLineArcIntersect(Eps_Line _Line, Eps_Arc _Arc, ref Eps_Point[] _IntPoints, ref int _nNumberOfIntersections, double _dTolerance)
	{
		int num = 0;
		_nNumberOfIntersections = 0;
		switch (eplGeoE_IntersectLineArc(_Line, _Arc, ref _IntPoints, _dTolerance))
		{
		case 1606:
			if (eplGeoE_ArePntsDuplicates2D(_Line.StartPnt, _IntPoints[0], _dTolerance) || eplGeoE_ArePntsDuplicates2D(_Line.EndPnt, _IntPoints[0], _dTolerance))
			{
				return false;
			}
			_nNumberOfIntersections = 1;
			return true;
		case 1607:
			_nNumberOfIntersections = 2;
			if (eplGeoE_ArePntsDuplicates2D(_Line.StartPnt, _IntPoints[0], _dTolerance) || eplGeoE_ArePntsDuplicates2D(_Line.EndPnt, _IntPoints[0], _dTolerance))
			{
				_IntPoints[0] = _IntPoints[1];
				_nNumberOfIntersections--;
			}
			if (eplGeoE_ArePntsDuplicates2D(_Line.StartPnt, _IntPoints[1], _dTolerance) || eplGeoE_ArePntsDuplicates2D(_Line.EndPnt, _IntPoints[1], _dTolerance))
			{
				_IntPoints[1] = _IntPoints[0];
				_nNumberOfIntersections--;
			}
			if (_nNumberOfIntersections > 0)
			{
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	public static bool DoArcsIntersect(Eps_Arc _Arc1, Eps_Arc _Arc2, ref Eps_Point[] _IntPoints, ref int _nNumberOfIntersections, double _dTolerance)
	{
		int num = 0;
		_nNumberOfIntersections = 0;
		switch (eplGeoE_IntersectTwoArcs2D(_Arc1, _Arc2, ref _IntPoints, _dTolerance))
		{
		case 1606:
			_nNumberOfIntersections = 1;
			return true;
		case 1607:
		case 1608:
			_nNumberOfIntersections = 2;
			return true;
		default:
			return false;
		}
	}

	public static int eplGeoE_IntersectTwoArcs2D(Eps_Arc _ArcOne, Eps_Arc _ArcTwo, ref Eps_Point[] _InterPnt, double _dPointTolerance)
	{
		Eps_Point[] _InterPnt2 = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		_InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		int num = 0;
		int result = 1600;
		int num2 = 1600;
		int num3 = 0;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		Eps_Line[] array = new Eps_Line[2]
		{
			new Eps_Line(),
			new Eps_Line()
		};
		Eps_Line obj = array[0];
		Eps_Point startPnt = (array[0].EndPnt = _ArcOne.RadiusPnt);
		obj.StartPnt = startPnt;
		array[0].StartPnt.x -= _ArcOne.Radius;
		array[0].EndPnt.x += _ArcOne.Radius;
		array[0].StartPnt.y -= _ArcOne.Radius;
		array[0].EndPnt.y += _ArcOne.Radius;
		Eps_Line obj2 = array[1];
		startPnt = (array[1].EndPnt = _ArcTwo.RadiusPnt);
		obj2.StartPnt = startPnt;
		array[1].StartPnt.x -= _ArcTwo.Radius;
		array[1].EndPnt.x += _ArcTwo.Radius;
		array[1].StartPnt.y -= _ArcTwo.Radius;
		array[1].EndPnt.y += _ArcTwo.Radius;
		if (!eplGeoE_DoBoundingBoxesOverlap(array[0], array[1], _dPointTolerance))
		{
			return result;
		}
		_InterPnt[0].z = _ArcOne.StartPnt.z;
		_InterPnt[1].z = _ArcOne.StartPnt.z;
		num2 = eplGeoE_FindCommonEndPntsOnArcs2D(_ArcOne, _ArcTwo, ref _InterPnt, _dPointTolerance);
		switch (num2)
		{
		case 1604:
		case 1613:
			return num2;
		case 1209:
			num2 = 1600;
			break;
		}
		num = ChkIfArcsOverlapOrWithIn2D(_ArcOne, _ArcTwo, ref _InterPnt, _dPointTolerance);
		if (num != 1609)
		{
			return num;
		}
		flag5 = eplGeoE_IsPointOnArc2D(_ArcOne.StartPnt, _ArcTwo, _dPointTolerance);
		flag6 = eplGeoE_IsPointOnArc2D(_ArcOne.EndPnt, _ArcTwo, _dPointTolerance);
		flag7 = eplGeoE_IsPointOnArc2D(_ArcTwo.StartPnt, _ArcOne, _dPointTolerance);
		flag8 = eplGeoE_IsPointOnArc2D(_ArcTwo.EndPnt, _ArcOne, _dPointTolerance);
		if (num2 == 1612)
		{
			flag = eplGeoE_ArePntsDuplicates2D(_ArcOne.StartPnt, _ArcTwo.StartPnt, _dPointTolerance);
			flag2 = eplGeoE_ArePntsDuplicates2D(_ArcOne.EndPnt, _ArcTwo.EndPnt, _dPointTolerance);
			flag3 = eplGeoE_ArePntsDuplicates2D(_ArcOne.StartPnt, _ArcTwo.EndPnt, _dPointTolerance);
			flag4 = eplGeoE_ArePntsDuplicates2D(_ArcOne.EndPnt, _ArcTwo.StartPnt, _dPointTolerance);
			if (flag)
			{
				_InterPnt[0] = _ArcOne.StartPnt;
				if (flag6)
				{
					_InterPnt[1] = _ArcOne.EndPnt;
					return 1607;
				}
				if (flag8)
				{
					_InterPnt[1] = _ArcTwo.EndPnt;
					return 1607;
				}
			}
			if (flag2)
			{
				_InterPnt[0] = _ArcOne.EndPnt;
				if (flag5)
				{
					_InterPnt[1] = _ArcOne.StartPnt;
					return 1607;
				}
				if (flag7)
				{
					_InterPnt[1] = _ArcTwo.StartPnt;
					return 1607;
				}
			}
			if (flag3)
			{
				_InterPnt[0] = _ArcOne.StartPnt;
				if (flag6)
				{
					_InterPnt[1] = _ArcOne.EndPnt;
					return 1607;
				}
				if (flag7)
				{
					_InterPnt[1] = _ArcTwo.StartPnt;
					return 1607;
				}
			}
			if (flag4)
			{
				_InterPnt[0] = _ArcOne.EndPnt;
				if (flag5)
				{
					_InterPnt[1] = _ArcOne.StartPnt;
					return 1607;
				}
				if (flag8)
				{
					_InterPnt[1] = _ArcTwo.EndPnt;
					return 1607;
				}
			}
		}
		if (num2 != 1612)
		{
			if (flag5 & flag6)
			{
				_InterPnt[0] = _ArcOne.StartPnt;
				_InterPnt[1] = _ArcOne.EndPnt;
				return 1607;
			}
			if (flag7 & flag8)
			{
				_InterPnt[0] = _ArcTwo.StartPnt;
				_InterPnt[1] = _ArcTwo.EndPnt;
				return 1607;
			}
			if (flag5 & flag7)
			{
				_InterPnt[0] = _ArcOne.StartPnt;
				_InterPnt[1] = _ArcTwo.StartPnt;
				return 1607;
			}
			if (flag5 & flag8)
			{
				_InterPnt[0] = _ArcOne.StartPnt;
				_InterPnt[1] = _ArcTwo.EndPnt;
				return 1607;
			}
			if (flag6 & flag7)
			{
				_InterPnt[0] = _ArcOne.EndPnt;
				_InterPnt[1] = _ArcTwo.StartPnt;
				return 1607;
			}
			if (flag6 & flag8)
			{
				_InterPnt[0] = _ArcOne.EndPnt;
				_InterPnt[1] = _ArcTwo.EndPnt;
				return 1607;
			}
			if (flag5)
			{
				_InterPnt[0] = _ArcOne.StartPnt;
				num2 = 1606;
			}
			else if (flag6)
			{
				_InterPnt[0] = _ArcOne.EndPnt;
				num2 = 1606;
			}
			else if (flag7)
			{
				_InterPnt[0] = _ArcTwo.StartPnt;
				num2 = 1606;
			}
			else if (flag8)
			{
				_InterPnt[0] = _ArcTwo.EndPnt;
				num2 = 1606;
			}
		}
		result = num2;
		switch (eplGeoE_IntersectTwoDists2D(_ArcOne.RadiusPnt, _ArcOne.Radius, _ArcTwo.RadiusPnt, _ArcTwo.Radius, ref _InterPnt2, 0.0))
		{
		case 1606:
			switch (num2)
			{
			case 1606:
				return 1606;
			case 1612:
				return 1612;
			case 1600:
				if (eplGeoE_IsPointOnArc2D(_InterPnt2[0], _ArcOne, _dPointTolerance) && eplGeoE_IsPointOnArc2D(_InterPnt2[0], _ArcTwo, _dPointTolerance))
				{
					_InterPnt[0] = _InterPnt2[0];
					return 1606;
				}
				break;
			}
			break;
		case 1607:
			if (num2 == 1600)
			{
				if (eplGeoE_IsPointOnArc2D(_InterPnt2[0], _ArcOne, _dPointTolerance) && eplGeoE_IsPointOnArc2D(_InterPnt2[0], _ArcTwo, _dPointTolerance))
				{
					_InterPnt[0] = _InterPnt2[0];
					result = ((num2 != 1612) ? 1606 : 1612);
				}
				if (eplGeoE_IsPointOnArc2D(_InterPnt2[1], _ArcOne, _dPointTolerance) && eplGeoE_IsPointOnArc2D(_InterPnt2[1], _ArcTwo, _dPointTolerance))
				{
					if (result != 1606)
					{
						_InterPnt[0] = _InterPnt2[1];
						result = ((num2 != 1612) ? 1606 : 1612);
					}
					else
					{
						_InterPnt[1] = _InterPnt2[1];
						result = 1607;
					}
				}
				return result;
			}
			if (!eplGeoE_ArePntsDuplicates2D(_InterPnt[0], _InterPnt2[0], _dPointTolerance) && eplGeoE_IsPointOnArc2D(_InterPnt2[0], _ArcOne, _dPointTolerance) && eplGeoE_IsPointOnArc2D(_InterPnt2[0], _ArcTwo, _dPointTolerance))
			{
				_InterPnt[1] = _InterPnt2[0];
				return 1607;
			}
			if (!eplGeoE_ArePntsDuplicates2D(_InterPnt[0], _InterPnt2[1], _dPointTolerance))
			{
				if (eplGeoE_IsPointOnArc2D(_InterPnt2[1], _ArcOne, _dPointTolerance) && eplGeoE_IsPointOnArc2D(_InterPnt2[1], _ArcTwo, _dPointTolerance))
				{
					_InterPnt[1] = _InterPnt2[1];
					return 1607;
				}
				break;
			}
			return result;
		default:
			result = 1600;
			break;
		}
		return result;
	}

	public static int ChkIfArcsOverlapOrWithIn2D(Eps_Arc _ArcOne, Eps_Arc _ArcTwo, ref Eps_Point[] _InterPnt, double _dPointTolerance)
	{
		int num = 1600;
		_InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		_InterPnt[0].z = _ArcOne.StartPnt.z;
		_InterPnt[1].z = _ArcOne.StartPnt.z;
		bool flag = ((_ArcOne.Direction == _ArcTwo.Direction) ? true : false);
		bool flag2 = eplGeoE_ArePntsDuplicates2D(_ArcOne.StartPnt, _ArcTwo.StartPnt, _dPointTolerance);
		bool flag3 = eplGeoE_ArePntsDuplicates2D(_ArcOne.EndPnt, _ArcTwo.EndPnt, _dPointTolerance);
		bool flag4 = eplGeoE_ArePntsDuplicates2D(_ArcOne.StartPnt, _ArcTwo.EndPnt, _dPointTolerance);
		bool flag5 = eplGeoE_ArePntsDuplicates2D(_ArcOne.EndPnt, _ArcTwo.StartPnt, _dPointTolerance);
		if (flag2 || flag4 || flag5 || flag3)
		{
			num = 1612;
		}
		bool flag6 = eplGeoE_IsPointOnArc2D(_ArcOne.StartPnt, _ArcTwo, _dPointTolerance);
		bool flag7 = eplGeoE_IsPointOnArc2D(_ArcOne.EndPnt, _ArcTwo, _dPointTolerance);
		bool flag8 = eplGeoE_IsPointOnArc2D(_ArcTwo.StartPnt, _ArcOne, _dPointTolerance);
		bool flag9 = eplGeoE_IsPointOnArc2D(_ArcTwo.EndPnt, _ArcOne, _dPointTolerance);
		if (eplGeoE_ArePntsDuplicates2D(_ArcOne.RadiusPnt, _ArcTwo.RadiusPnt, _dPointTolerance) && Math.Abs(_ArcOne.Radius - _ArcTwo.Radius) <= 1E-05)
		{
			if (_ArcOne.Delta + _ArcTwo.Delta > Math.PI * 2.0 && flag6 && flag7 && flag8 && flag9)
			{
				if (_ArcOne.Delta > _ArcTwo.Delta)
				{
					_InterPnt[0] = _ArcOne.StartPnt;
					_InterPnt[1] = _ArcOne.EndPnt;
				}
				else
				{
					_InterPnt[0] = _ArcTwo.StartPnt;
					_InterPnt[1] = _ArcTwo.EndPnt;
				}
				if (num == 1612)
				{
					return 1619;
				}
				return 1620;
			}
			if (num == 1612)
			{
				if (flag2)
				{
					if (flag7)
					{
						_InterPnt[1] = _ArcOne.EndPnt;
						return 1618;
					}
					if (flag9)
					{
						_InterPnt[1] = _ArcTwo.EndPnt;
						return 1618;
					}
					return 1612;
				}
				if (flag3)
				{
					if (flag6)
					{
						_InterPnt[1] = _ArcOne.StartPnt;
						return 1618;
					}
					if (flag8)
					{
						_InterPnt[1] = _ArcTwo.StartPnt;
						return 1618;
					}
					return 1612;
				}
				if (flag4)
				{
					if (flag7)
					{
						_InterPnt[1] = _ArcOne.EndPnt;
						return 1618;
					}
					if (flag8)
					{
						_InterPnt[1] = _ArcTwo.StartPnt;
						return 1618;
					}
					return 1612;
				}
				if (flag5)
				{
					if (flag6)
					{
						_InterPnt[1] = _ArcOne.StartPnt;
						return 1618;
					}
					if (flag9)
					{
						_InterPnt[1] = _ArcTwo.EndPnt;
						return 1618;
					}
					return 1612;
				}
			}
			if (flag6 && flag7)
			{
				_InterPnt[0] = _ArcOne.StartPnt;
				_InterPnt[1] = _ArcOne.EndPnt;
				return 1610;
			}
			if (flag8 && flag9)
			{
				_InterPnt[0] = _ArcTwo.StartPnt;
				_InterPnt[1] = _ArcTwo.EndPnt;
				return 1611;
			}
			if (flag6 && flag9)
			{
				_InterPnt[0] = _ArcOne.StartPnt;
				_InterPnt[1] = _ArcTwo.EndPnt;
				return 1608;
			}
			if (flag6 && flag8)
			{
				_InterPnt[0] = _ArcOne.StartPnt;
				_InterPnt[1] = _ArcTwo.StartPnt;
				return 1608;
			}
			if (flag7 && flag8)
			{
				_InterPnt[0] = _ArcOne.EndPnt;
				_InterPnt[1] = _ArcTwo.StartPnt;
				return 1608;
			}
			if (flag7 && flag9)
			{
				_InterPnt[0] = _ArcOne.EndPnt;
				_InterPnt[1] = _ArcTwo.EndPnt;
				return 1608;
			}
		}
		return 1609;
	}

	public static int eplGeoE_FindCommonEndPntsOnArcs2D(Eps_Arc _ArcOne, Eps_Arc _ArcTwo, ref Eps_Point[] _InterPnt, double _dPointTolerance)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		_InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		_InterPnt[0].z = _ArcOne.StartPnt.z;
		_InterPnt[1].z = _ArcOne.StartPnt.z;
		flag5 = ((_ArcOne.Direction == _ArcTwo.Direction) ? true : false);
		flag = eplGeoE_ArePntsDuplicates2D(_ArcOne.StartPnt, _ArcTwo.StartPnt, _dPointTolerance);
		flag2 = eplGeoE_ArePntsDuplicates2D(_ArcOne.EndPnt, _ArcTwo.EndPnt, _dPointTolerance);
		flag3 = eplGeoE_ArePntsDuplicates2D(_ArcOne.StartPnt, _ArcTwo.EndPnt, _dPointTolerance);
		flag4 = eplGeoE_ArePntsDuplicates2D(_ArcOne.EndPnt, _ArcTwo.StartPnt, _dPointTolerance);
		if (flag && flag2)
		{
			if (eplGeoE_ArePntsDuplicates2D(_ArcOne.RadiusPnt, _ArcTwo.RadiusPnt, _dPointTolerance))
			{
				if (flag5)
				{
					return 1604;
				}
				_InterPnt[0] = _ArcOne.StartPnt;
				_InterPnt[1] = _ArcOne.EndPnt;
				return 1613;
			}
			_InterPnt[0] = _ArcOne.StartPnt;
			_InterPnt[1] = _ArcOne.EndPnt;
			return 1613;
		}
		if (flag3 && flag4)
		{
			if (eplGeoE_ArePntsDuplicates2D(_ArcOne.RadiusPnt, _ArcTwo.RadiusPnt, _dPointTolerance))
			{
				if (!flag5)
				{
					return 1604;
				}
				_InterPnt[0] = _ArcOne.StartPnt;
				_InterPnt[1] = _ArcOne.EndPnt;
				return 1613;
			}
			_InterPnt[0] = _ArcOne.StartPnt;
			_InterPnt[1] = _ArcOne.EndPnt;
			return 1613;
		}
		if (flag || flag3)
		{
			_InterPnt[0] = _ArcOne.StartPnt;
			return 1612;
		}
		if (flag4 || flag2)
		{
			_InterPnt[0] = _ArcOne.EndPnt;
			return 1612;
		}
		return 1209;
	}

	public static int eplGeoE_IntersectTwoDists2D(Eps_Point _FirstRadiusPnt, double _FirstRadiusLen, Eps_Point _SecondRadiusPnt, double _SecondRadiusLen, ref Eps_Point[] _InterPnt, double _dTolerance)
	{
		double num = 0.0;
		double num2 = 0.0;
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		_InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		num = Math.Abs(_FirstRadiusLen);
		num2 = Math.Abs(_SecondRadiusLen);
		_InterPnt[0].z = _FirstRadiusPnt.z;
		_InterPnt[1].z = _FirstRadiusPnt.z;
		if (num < 1E-05 || num2 < 1E-05)
		{
			return 1602;
		}
		num4 = Math.Abs(num - num2);
		eplGeoE_CalcHorzDistBtwnTwoPnts(_FirstRadiusPnt, _SecondRadiusPnt, ref _dDistance);
		if (_dDistance < _dTolerance && num4 < 1E-05)
		{
			return 1604;
		}
		if (_dDistance < _dTolerance)
		{
			return 1605;
		}
		if (_dDistance + num + _dTolerance < num2 || _dDistance + num2 + _dTolerance < num)
		{
			return 1601;
		}
		if (num + num2 + 1E-05 < _dDistance)
		{
			return 1602;
		}
		num3 = (_dDistance * _dDistance + num * num - num2 * num2) / (2.0 * _dDistance * num);
		if (num3 >= 1.0)
		{
			num3 = 0.0;
		}
		else if (num3 <= -1.0)
		{
			num3 = Math.PI;
		}
		else
		{
			num3 = Math.Acos(num3);
			num3 = eplGeoE_NormalizeRadians(num3);
		}
		eplGeoE_CalcHorzDirBtwnTwoPnts(_FirstRadiusPnt, _SecondRadiusPnt, ref _dDirection);
		Eps_Point _NewPnt = new Eps_Point();
		eplGeoE_CalcPolarToXY(_FirstRadiusPnt, _dDirection + num3, _FirstRadiusLen, ref _NewPnt);
		_InterPnt[0] = _NewPnt;
		if (num3 > 1E-10 && Math.Abs(num3 - Math.PI) > 1E-10)
		{
			eplGeoE_CalcPolarToXY(_FirstRadiusPnt, _dDirection - num3, _FirstRadiusLen, ref _NewPnt);
			_InterPnt[1] = _NewPnt;
			return 1607;
		}
		return 1606;
	}

	public static int eplGeoE_IsPointInsideLegList(Eps_Leg _pLeg, Eps_Point _Point, double _dTolerance, Eps_Point _pMinPnt, Eps_Point _pMaxPnt)
	{
		int num = 0;
		Eps_Leg eps_Leg = new Eps_Leg();
		Eps_Line eps_Line = new Eps_Line();
		Eps_Point _pMinPnt2 = new Eps_Point();
		Eps_Point _pMaxPnt2 = new Eps_Point();
		double _dDistance = 0.0;
		double _dDistance2 = 0.0;
		int _pNumOfInter = 0;
		int num2 = 0;
		List<double> list = new List<double>();
		Eps_Point _pLegStartPnt = new Eps_Point();
		Eps_Point _pLegEndPnt = new Eps_Point();
		double _dDirection = 0.0;
		int num3 = 0;
		int num4 = 0;
		double num5 = 0.0;
		bool flag = false;
		if (_pMinPnt == null || _pMaxPnt == null)
		{
			eplGeoE_FindMinMaxOfLegList(_pLeg, ref _pMinPnt2, ref _pMaxPnt2);
		}
		else
		{
			_pMinPnt2 = _pMinPnt;
			_pMaxPnt2 = _pMaxPnt;
		}
		Eps_Point[] _InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		if (RevitDefines.LT(_Point.x, _pMinPnt2.x, _dTolerance) || RevitDefines.GT(_Point.x, _pMaxPnt2.x, _dTolerance) || RevitDefines.LT(_Point.y, _pMinPnt2.y, _dTolerance) || RevitDefines.GT(_Point.y, _pMaxPnt2.y, _dTolerance))
		{
			return 6;
		}
		eplGeoE_CalcHorzDistBtwnTwoPnts(_pMinPnt2, _pMaxPnt2, ref _dDistance);
		for (eps_Leg = _pLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			eplGeoE_GetLegStartAndEndPoints(eps_Leg, ref _pLegStartPnt, ref _pLegEndPnt);
			if (RevitDefines.EQ(_pLegStartPnt.y, _Point.y) || RevitDefines.EQ(_pLegEndPnt.y, _Point.y))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			for (eps_Leg = _pLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
			{
				eplGeoE_GetLegStartAndEndPoints(eps_Leg, ref _pLegStartPnt, ref _pLegEndPnt);
				if (eps_Leg == _pLeg)
				{
					eplGeoE_CalcHorzDirBtwnTwoPnts(_Point, _pLegStartPnt, ref _dDirection);
					if (!list.Contains(_dDirection))
					{
						list.Add(_dDirection);
					}
				}
				eplGeoE_CalcHorzDirBtwnTwoPnts(_Point, _pLegEndPnt, ref _dDirection);
				if (!list.Contains(_dDirection))
				{
					list.Add(_dDirection);
				}
			}
			list.Sort();
			for (num3 = 0; num3 < list.Count - 1; num3++)
			{
				_dDirection = list[num3 + 1] - list[num3];
				if (num3 == 0 || _dDirection > num5)
				{
					num5 = _dDirection;
					num4 = num3;
				}
			}
			_dDirection = (list[num4 + 1] - list[num4]) / 2.0;
		}
		else
		{
			_dDirection = 0.0;
		}
		eps_Line.StartPnt = _Point;
		Eps_Point _NewPnt = new Eps_Point();
		eplGeoE_CalcPolarToXY(eps_Line.StartPnt, _dDirection, _dDistance * 2.0, ref _NewPnt);
		eps_Line.EndPnt = _NewPnt;
		for (eps_Leg = _pLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				if (eplGeoE_IsPointOnLine2D(_Point, eps_Leg.Line, _dTolerance))
				{
					return 1903;
				}
				switch (eplGeoE_IntersectTwoLines(eps_Line, eps_Leg.Line, ref _InterPnt, ref _pNumOfInter, _dTolerance))
				{
				case 1606:
				case 1612:
					if (!eplGeoE_ArePntsDuplicates2D(_InterPnt[0], eps_Leg.Line.StartPnt) && !eplGeoE_ArePntsDuplicates2D(_InterPnt[0], eps_Leg.Line.EndPnt))
					{
						num2++;
					}
					break;
				case 1608:
				case 1610:
				case 1611:
					num2 += 2;
					break;
				}
			}
			else if (eps_Leg.Arc != null)
			{
				if (eplGeoE_IsPointOnArc2D(_Point, eps_Leg.Arc, _dTolerance))
				{
					return 1903;
				}
				switch (eplGeoE_IntersectLineArc(eps_Line, eps_Leg.Arc, ref _InterPnt, 0.0))
				{
				case 1606:
					eplGeoE_CalcHorzDistBtwnTwoPnts(_Point, eps_Leg.Arc.RadiusPnt, ref _dDistance2);
					if (RevitDefines.LT(_dDistance2, eps_Leg.Arc.Radius))
					{
						num2++;
					}
					break;
				case 1607:
					num2 += 2;
					break;
				}
			}
		}
		if (num2 % 2 == 1)
		{
			return 0;
		}
		return 6;
	}

	public static int eplGeoE_IntersectTwoLines(Eps_Line _pLineOne, Eps_Line _pLineTwo, ref Eps_Point[] _aInterPnt, ref int _pNumOfInter, double _dPointTolerance)
	{
		double num = 0.0;
		double num2 = 0.0;
		int result = 1600;
		_aInterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		_pNumOfInter = 0;
		if (!eplGeoE_DoBoundingBoxesOverlap(_pLineOne, _pLineTwo, _dPointTolerance))
		{
			return result;
		}
		num = eplGeoE_CalcPlaneDistBtwnTwoPnts(_pLineOne.StartPnt.x, _pLineOne.StartPnt.y, _pLineOne.EndPnt.x, _pLineOne.EndPnt.y);
		num2 = eplGeoE_CalcPlaneDistBtwnTwoPnts(_pLineTwo.StartPnt.x, _pLineTwo.StartPnt.y, _pLineTwo.EndPnt.x, _pLineTwo.EndPnt.y);
		if (num <= _dPointTolerance)
		{
			if (num2 <= _dPointTolerance)
			{
				result = 1604;
			}
			else if (eplGeoE_IsPointOnLine2D(_pLineOne.StartPnt, _pLineTwo, _dPointTolerance))
			{
				_aInterPnt[0] = (_aInterPnt[1] = _pLineOne.StartPnt);
				result = 1610;
			}
			return result;
		}
		if (num2 <= _dPointTolerance)
		{
			if (eplGeoE_IsPointOnLine2D(_pLineTwo.StartPnt, _pLineOne, _dPointTolerance))
			{
				_aInterPnt[0] = (_aInterPnt[1] = _pLineTwo.StartPnt);
				result = 1611;
			}
			return result;
		}
		result = eplGeoE_InterBoundedLineLine(_pLineOne, _pLineTwo, ref _aInterPnt, _dPointTolerance);
		switch (result)
		{
		case 1111:
			result = eplGeoE_InterColinearBoundedLineLine(_pLineOne, _pLineTwo, ref _aInterPnt, ref _pNumOfInter, _dPointTolerance);
			break;
		case 1606:
		{
			_pNumOfInter = 1;
			int num3 = CheckLinesForCommonEndPnts(_pLineOne, _pLineTwo, _dPointTolerance);
			if ((uint)(num3 - 1614) <= 3u)
			{
				result = 1612;
			}
			break;
		}
		}
		return result;
	}

	public static int eplGeoE_InterColinearBoundedLineLine(Eps_Line _pLineOne, Eps_Line _pLineTwo, ref Eps_Point[] _aInterPnt, ref int _pNumOfInter, double _dPointTolerance)
	{
		int num = 0;
		int num2 = 0;
		num2 = 1600;
		_pNumOfInter = 0;
		_aInterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		switch (CheckLinesForCommonEndPnts(_pLineOne, _pLineTwo, _dPointTolerance))
		{
		case 1108:
		case 1109:
			return 1604;
		case 1615:
		case 1617:
			_aInterPnt[_pNumOfInter++] = _pLineOne.EndPnt;
			num2 = 1612;
			break;
		case 1614:
		case 1616:
			_aInterPnt[_pNumOfInter++] = _pLineOne.StartPnt;
			num2 = 1612;
			break;
		}
		num = eplGeoE_CheckIfLineInsideLine(_pLineOne, _pLineTwo, _dPointTolerance);
		if (num == 1700)
		{
			_aInterPnt[0] = _pLineOne.StartPnt;
			_aInterPnt[1] = _pLineOne.EndPnt;
			_pNumOfInter = 2;
			num2 = 1610;
		}
		else if (eplGeoE_CheckIfLineInsideLine(_pLineTwo, _pLineOne, _dPointTolerance) == 1700)
		{
			_aInterPnt[0] = _pLineTwo.StartPnt;
			_aInterPnt[1] = _pLineTwo.EndPnt;
			_pNumOfInter = 2;
			num2 = 1611;
		}
		if (num2 == 1600)
		{
			if (eplGeoE_IsPointOnLine2D(_pLineOne.StartPnt, _pLineTwo, _dPointTolerance))
			{
				_aInterPnt[_pNumOfInter++] = _pLineOne.StartPnt;
				if (eplGeoE_IsPointOnLine2D(_pLineTwo.StartPnt, _pLineOne, _dPointTolerance))
				{
					_aInterPnt[_pNumOfInter++] = _pLineTwo.StartPnt;
				}
				else
				{
					_aInterPnt[_pNumOfInter++] = _pLineTwo.EndPnt;
				}
				num2 = 1608;
			}
			else if (eplGeoE_IsPointOnLine2D(_pLineOne.EndPnt, _pLineTwo, _dPointTolerance))
			{
				_aInterPnt[_pNumOfInter++] = _pLineOne.EndPnt;
				if (eplGeoE_IsPointOnLine2D(_pLineTwo.StartPnt, _pLineOne, _dPointTolerance))
				{
					_aInterPnt[_pNumOfInter++] = _pLineTwo.StartPnt;
				}
				else
				{
					_aInterPnt[_pNumOfInter++] = _pLineTwo.EndPnt;
				}
				num2 = 1608;
			}
		}
		return num2;
	}

	public static int CheckLinesForCommonEndPnts(Eps_Line _LineOne, Eps_Line _LineTwo, double _dPointTolerance)
	{
		int result = 1110;
		bool flag = eplGeoE_ArePntsDuplicates2D(_LineOne.StartPnt, _LineTwo.StartPnt, _dPointTolerance);
		bool flag2 = eplGeoE_ArePntsDuplicates2D(_LineOne.EndPnt, _LineTwo.EndPnt, _dPointTolerance);
		bool flag3 = eplGeoE_ArePntsDuplicates2D(_LineOne.StartPnt, _LineTwo.EndPnt, _dPointTolerance);
		bool flag4 = eplGeoE_ArePntsDuplicates2D(_LineOne.EndPnt, _LineTwo.StartPnt, _dPointTolerance);
		if (flag && flag2)
		{
			result = 1108;
		}
		else if (flag3 && flag4)
		{
			result = 1109;
		}
		else if (flag)
		{
			result = 1614;
		}
		else if (flag2)
		{
			result = 1615;
		}
		else if (flag3)
		{
			result = 1616;
		}
		else if (flag4)
		{
			result = 1617;
		}
		return result;
	}

	public static int eplGeoE_InterBoundedLineLine(Eps_Line _pLineOne, Eps_Line _pLineTwo, ref Eps_Point[] _aInterPnt, double _dPointTolerance)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		double num10 = 0.0;
		_aInterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		int result = 1600;
		num = (_pLineTwo.StartPnt.x - _pLineOne.StartPnt.x) * (_pLineOne.EndPnt.y - _pLineOne.StartPnt.y) - (_pLineOne.EndPnt.x - _pLineOne.StartPnt.x) * (_pLineTwo.StartPnt.y - _pLineOne.StartPnt.y);
		num2 = (_pLineTwo.EndPnt.x - _pLineOne.StartPnt.x) * (_pLineOne.EndPnt.y - _pLineOne.StartPnt.y) - (_pLineOne.EndPnt.x - _pLineOne.StartPnt.x) * (_pLineTwo.EndPnt.y - _pLineOne.StartPnt.y);
		num3 = (_pLineOne.StartPnt.x - _pLineTwo.StartPnt.x) * (_pLineTwo.EndPnt.y - _pLineTwo.StartPnt.y) - (_pLineTwo.EndPnt.x - _pLineTwo.StartPnt.x) * (_pLineOne.StartPnt.y - _pLineTwo.StartPnt.y);
		num4 = (_pLineOne.EndPnt.x - _pLineTwo.StartPnt.x) * (_pLineTwo.EndPnt.y - _pLineTwo.StartPnt.y) - (_pLineTwo.EndPnt.x - _pLineTwo.StartPnt.x) * (_pLineOne.EndPnt.y - _pLineTwo.StartPnt.y);
		num9 = eplGeoE_CalcPlaneDistBtwnTwoPnts(_pLineOne.StartPnt.x, _pLineOne.StartPnt.y, _pLineOne.EndPnt.x, _pLineOne.EndPnt.y);
		num10 = eplGeoE_CalcPlaneDistBtwnTwoPnts(_pLineTwo.StartPnt.x, _pLineTwo.StartPnt.y, _pLineTwo.EndPnt.x, _pLineTwo.EndPnt.y);
		num5 = Math.Abs(num / num9);
		num6 = Math.Abs(num2 / num9);
		num7 = Math.Abs(num3 / num10);
		num8 = Math.Abs(num4 / num10);
		if (num5 <= _dPointTolerance)
		{
			if (num6 <= _dPointTolerance)
			{
				result = 1111;
			}
			else if (IsPntInBoundingBoxOfLine(_pLineTwo.StartPnt, _pLineOne, _dPointTolerance))
			{
				_aInterPnt[0] = _pLineTwo.StartPnt;
				result = 1606;
			}
			return result;
		}
		if (num6 <= _dPointTolerance)
		{
			if (IsPntInBoundingBoxOfLine(_pLineTwo.EndPnt, _pLineOne, _dPointTolerance))
			{
				_aInterPnt[0] = _pLineTwo.EndPnt;
				result = 1606;
			}
			return result;
		}
		if (num7 <= _dPointTolerance)
		{
			if (IsPntInBoundingBoxOfLine(_pLineOne.StartPnt, _pLineTwo, _dPointTolerance))
			{
				_aInterPnt[0] = _pLineOne.StartPnt;
				result = 1606;
			}
			return result;
		}
		if (num8 <= _dPointTolerance)
		{
			if (IsPntInBoundingBoxOfLine(_pLineOne.EndPnt, _pLineTwo, _dPointTolerance))
			{
				_aInterPnt[0] = _pLineOne.EndPnt;
				result = 1606;
			}
			return result;
		}
		if (((num < 0.0 && num2 > 0.0) || (num > 0.0 && num2 < 0.0)) && ((num3 < 0.0 && num4 > 0.0) || (num3 > 0.0 && num4 < 0.0)))
		{
			Eps_Point _aInterPnt2 = new Eps_Point();
			CalculateInterPnt(_pLineOne, _pLineTwo, ref _aInterPnt2);
			_aInterPnt[0] = _aInterPnt2;
			result = 1606;
		}
		return result;
	}

	public static void CalculateInterPnt(Eps_Line _pLineOne, Eps_Line _pLineTwo, ref Eps_Point _aInterPnt)
	{
		double num = 0.0;
		Eps_Point _Vector = new Eps_Point();
		Eps_Point _Vector2 = new Eps_Point();
		Eps_Point _Vector3 = new Eps_Point();
		double num2 = 0.0;
		double num3 = 0.0;
		eplGeoE_MakeVector(_pLineOne.StartPnt, _pLineOne.EndPnt, ref _Vector);
		eplGeoE_MakeVector(_pLineTwo.StartPnt, _pLineTwo.EndPnt, ref _Vector2);
		eplGeoE_MakeVector(_pLineOne.StartPnt, _pLineTwo.EndPnt, ref _Vector3);
		num2 = eplCalE_CalcComponentKOfCrossProduct(_Vector, _Vector2);
		num3 = eplCalE_CalcComponentKOfCrossProduct(_Vector3, _Vector2);
		if (RevitDefines.EQ(num2, 0.0, 1E-10))
		{
			num2 = 1E-10;
		}
		num = Math.Abs(num3 / num2);
		_aInterPnt.x = _pLineOne.StartPnt.x + num * _Vector.x;
		_aInterPnt.y = _pLineOne.StartPnt.y + num * _Vector.y;
		_aInterPnt.z = 0.0;
	}

	public static bool IsPntInBoundingBoxOfLine(Eps_Point _pPoint, Eps_Line _pLine, double _dTolerance)
	{
		if (RevitDefines.LT(Math.Max(_pLine.StartPnt.x, _pLine.EndPnt.x), _pPoint.x, _dTolerance))
		{
			return false;
		}
		if (RevitDefines.GT(Math.Min(_pLine.StartPnt.x, _pLine.EndPnt.x), _pPoint.x, _dTolerance))
		{
			return false;
		}
		if (RevitDefines.LT(Math.Max(_pLine.StartPnt.y, _pLine.EndPnt.y), _pPoint.y, _dTolerance))
		{
			return false;
		}
		if (RevitDefines.GT(Math.Min(_pLine.StartPnt.y, _pLine.EndPnt.y), _pPoint.y, _dTolerance))
		{
			return false;
		}
		return true;
	}

	public static int eplGeoE_IntersectLineArc(Eps_Line _Line, Eps_Arc _Arc, ref Eps_Point[] _InterPnt, double _dPointTolerance)
	{
		double _dDirection = 0.0;
		double num = 0.0;
		int result = 1600;
		int num2 = 1600;
		int num3 = 0;
		Eps_Line eps_Line = new Eps_Line();
		Eps_Point startPnt = (eps_Line.EndPnt = _Arc.RadiusPnt);
		eps_Line.StartPnt = startPnt;
		eps_Line.StartPnt.x -= _Arc.Radius;
		eps_Line.EndPnt.x += _Arc.Radius;
		eps_Line.StartPnt.y -= _Arc.Radius;
		eps_Line.EndPnt.y += _Arc.Radius;
		if (!eplGeoE_DoBoundingBoxesOverlap(_Line, eps_Line, _dPointTolerance))
		{
			return result;
		}
		Eps_Point[] _InterPnt2 = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		_InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		_InterPnt[0].z = _Line.StartPnt.z;
		_InterPnt[1].z = _Line.StartPnt.z;
		bool flag = eplGeoE_ArePntsDuplicates2D(_Line.StartPnt, _Arc.StartPnt, _dPointTolerance);
		bool flag2 = eplGeoE_ArePntsDuplicates2D(_Line.EndPnt, _Arc.EndPnt, _dPointTolerance);
		bool flag3 = eplGeoE_ArePntsDuplicates2D(_Line.StartPnt, _Arc.EndPnt, _dPointTolerance);
		bool flag4 = eplGeoE_ArePntsDuplicates2D(_Line.EndPnt, _Arc.StartPnt, _dPointTolerance);
		if ((flag && flag2) || (flag3 && flag4))
		{
			_InterPnt[0] = _Line.StartPnt;
			_InterPnt[1] = _Line.EndPnt;
			return 1613;
		}
		bool flag5 = eplGeoE_IsPointOnArc2D(_Line.StartPnt, _Arc, _dPointTolerance);
		bool flag6 = eplGeoE_IsPointOnArc2D(_Line.EndPnt, _Arc, _dPointTolerance);
		bool flag7 = eplGeoE_IsPointOnLine2D(_Arc.StartPnt, _Line, _dPointTolerance);
		bool flag8 = eplGeoE_IsPointOnLine2D(_Arc.EndPnt, _Line, _dPointTolerance);
		if (flag || flag3)
		{
			_InterPnt[0] = _Line.StartPnt;
			num2 = 1612;
			if (flag6)
			{
				_InterPnt[1] = _Line.EndPnt;
				return 1607;
			}
			if (flag)
			{
				if (flag8)
				{
					_InterPnt[1] = _Arc.EndPnt;
					return 1607;
				}
			}
			else if (flag7)
			{
				_InterPnt[1] = _Arc.StartPnt;
				return 1607;
			}
		}
		if (num2 == 1600 && (flag2 || flag4))
		{
			_InterPnt[0] = _Line.EndPnt;
			num2 = 1612;
			if (flag5)
			{
				_InterPnt[1] = _Line.StartPnt;
				return 1607;
			}
			if (flag2)
			{
				if (flag7)
				{
					_InterPnt[1] = _Arc.StartPnt;
					return 1607;
				}
			}
			else if (flag8)
			{
				_InterPnt[1] = _Arc.EndPnt;
				return 1607;
			}
		}
		if (num2 == 1600)
		{
			if (flag6)
			{
				_InterPnt[0] = _Line.EndPnt;
				num2 = 1606;
				if (flag7)
				{
					_InterPnt[1] = _Arc.StartPnt;
					return 1607;
				}
				if (flag8)
				{
					_InterPnt[1] = _Arc.EndPnt;
					return 1607;
				}
			}
			if (flag5)
			{
				if (num2 == 1606)
				{
					_InterPnt[1] = _Line.StartPnt;
					return 1607;
				}
				_InterPnt[0] = _Line.StartPnt;
				num2 = 1606;
				if (flag7)
				{
					_InterPnt[1] = _Arc.StartPnt;
					return 1607;
				}
				if (flag8)
				{
					_InterPnt[1] = _Arc.EndPnt;
					return 1607;
				}
			}
		}
		if (num2 == 1600)
		{
			if (flag7 && flag8)
			{
				_InterPnt[0] = _Arc.StartPnt;
				_InterPnt[1] = _Arc.EndPnt;
				return 1607;
			}
			if (flag7)
			{
				_InterPnt[0] = _Arc.StartPnt;
				num2 = 1606;
			}
			else if (flag8)
			{
				_InterPnt[0] = _Arc.EndPnt;
				num2 = 1606;
			}
		}
		result = num2;
		eplGeoE_CalcHorzDirBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDirection);
		num = _Arc.Radius;
		switch (eplGeoE_IntersectDirDist2D(_Line.StartPnt, _dDirection, _Arc.RadiusPnt, num, ref _InterPnt2, 0.0))
		{
		case 1606:
			switch (num2)
			{
			case 1606:
				return 1606;
			case 1612:
				return 1612;
			case 1600:
				if (eplGeoE_IsPointOnLine2D(_InterPnt2[0], _Line, _dPointTolerance) && eplGeoE_IsPointOnArc2D(_InterPnt2[0], _Arc, _dPointTolerance))
				{
					_InterPnt[0] = _InterPnt2[0];
					return 1606;
				}
				break;
			}
			break;
		case 1607:
			if (num2 == 1600)
			{
				if (eplGeoE_IsPointOnLine2D(_InterPnt2[0], _Line, _dPointTolerance) && eplGeoE_IsPointOnArc2D(_InterPnt2[0], _Arc, _dPointTolerance))
				{
					_InterPnt[0] = _InterPnt2[0];
					result = 1606;
				}
				if (eplGeoE_IsPointOnLine2D(_InterPnt2[1], _Line, _dPointTolerance) && eplGeoE_IsPointOnArc2D(_InterPnt2[1], _Arc, _dPointTolerance))
				{
					if (result != 1606)
					{
						_InterPnt[0] = _InterPnt2[1];
						result = ((num2 != 1612) ? 1606 : 1612);
					}
					else
					{
						_InterPnt[1] = _InterPnt2[1];
						result = 1607;
					}
				}
				return result;
			}
			if (!eplGeoE_ArePntsDuplicates2D(_InterPnt[0], _InterPnt2[0], _dPointTolerance) && eplGeoE_IsPointOnLine2D(_InterPnt2[0], _Line, _dPointTolerance) && eplGeoE_IsPointOnArc2D(_InterPnt2[0], _Arc, _dPointTolerance))
			{
				_InterPnt[1] = _InterPnt2[0];
				return 1607;
			}
			if (!eplGeoE_ArePntsDuplicates2D(_InterPnt[0], _InterPnt2[1], _dPointTolerance))
			{
				if (eplGeoE_IsPointOnLine2D(_InterPnt2[1], _Line, _dPointTolerance) && eplGeoE_IsPointOnArc2D(_InterPnt2[1], _Arc, _dPointTolerance))
				{
					_InterPnt[1] = _InterPnt2[1];
					return 1607;
				}
				break;
			}
			return result;
		}
		return result;
	}

	public static int eplGeoE_IntersectDirDist2D(Eps_Point _FirstPnt, double _dDirOfFirstPnt, Eps_Point _RadiusPnt, double _dLenOfRadius, ref Eps_Point[] _InterPnt, double _dTolerance)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		double num10 = 0.0;
		double _dDistance = 0.0;
		double num11 = 0.0;
		double _dDirection = 0.0;
		double num12 = 0.0;
		double num13 = 0.0;
		double num14 = 0.0;
		int num15 = 0;
		int num16 = 0;
		int num17 = 0;
		_dTolerance = 0.001;
		Eps_Point eps_Point = new Eps_Point();
		_InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		_InterPnt[0].z = _FirstPnt.z;
		_InterPnt[1].z = _FirstPnt.z;
		num4 = Math.Cos(_dDirOfFirstPnt);
		num5 = Math.Sin(_dDirOfFirstPnt);
		eplGeoE_CalcHorzDistBtwnTwoPnts(_RadiusPnt, _FirstPnt, ref _dDistance);
		if (_dDistance < _dTolerance)
		{
			Eps_Point _NewPnt = new Eps_Point();
			eplGeoE_CalcPolarToXY(_RadiusPnt, _dDirOfFirstPnt, _dLenOfRadius, ref _NewPnt);
			_InterPnt[0] = _NewPnt;
			return 1606;
		}
		eplGeoE_CalcHorzDirBtwnTwoPnts(_FirstPnt, _RadiusPnt, ref _dDirection);
		num = Math.Abs(_dDirOfFirstPnt - _dDirection);
		if (_dLenOfRadius - _dDistance > 1E-05)
		{
			num16 = 1;
		}
		if (num16 == 0)
		{
			num11 = ((_RadiusPnt.y - _FirstPnt.y) * Math.Cos(_dDirOfFirstPnt) - (_RadiusPnt.x - _FirstPnt.x) * Math.Sin(_dDirOfFirstPnt)) / _dLenOfRadius;
			if (Math.Abs(num11) - 1.0 > 1E-05)
			{
				return 1600;
			}
		}
		num3 = Math.Sin(num);
		num8 = _dDistance * num3 / _dLenOfRadius;
		if (num8 < -1.0 || num8 > 1.0)
		{
			if (num8 < -1.0000000001 || num8 > 1.0000000001)
			{
				return 1600;
			}
			num8 = eplCalE_RoundValue(num8, 0);
		}
		num6 = Math.Asin(num8);
		num9 = Math.PI - (num + num6);
		num2 = Math.PI - num;
		num7 = Math.PI - num6;
		num10 = Math.PI - (num + num7);
		num12 = _dDistance * _dDistance + _dLenOfRadius * _dLenOfRadius - 2.0 * _dDistance * _dLenOfRadius * Math.Cos(num9);
		num12 = Math.Sqrt(Math.Abs(num12));
		num13 = _dDistance * _dDistance + _dLenOfRadius * _dLenOfRadius - 2.0 * _dDistance * _dLenOfRadius * Math.Cos(num10);
		num13 = Math.Sqrt(Math.Abs(num13));
		if (num16 == 0)
		{
			num16 = ((Math.Abs(num6 - Math.PI / 2.0) <= 1E-10) ? 1 : 2);
		}
		eps_Point.x = _FirstPnt.x + num12 * num4;
		eps_Point.y = _FirstPnt.y + num12 * num5;
		num14 = (eps_Point.x - _RadiusPnt.x) * (eps_Point.x - _RadiusPnt.x) + (eps_Point.y - _RadiusPnt.y) * (eps_Point.y - _RadiusPnt.y) - _dLenOfRadius * _dLenOfRadius;
		if (num14 <= _dTolerance)
		{
			num17 = 1;
			_InterPnt[num15].x = eps_Point.x;
			_InterPnt[num15].y = eps_Point.y;
			num15 = 1;
		}
		if (num16 == 1)
		{
			if (num15 == 1)
			{
				return 1606;
			}
			return 1600;
		}
		eps_Point.x = _FirstPnt.x + num13 * num4;
		eps_Point.y = _FirstPnt.y + num13 * num5;
		num14 = (eps_Point.x - _RadiusPnt.x) * (eps_Point.x - _RadiusPnt.x) + (eps_Point.y - _RadiusPnt.y) * (eps_Point.y - _RadiusPnt.y) - _dLenOfRadius * _dLenOfRadius;
		if (num14 <= _dTolerance)
		{
			num17++;
			_InterPnt[num15].x = eps_Point.x;
			_InterPnt[num15].y = eps_Point.y;
		}
		return num17 switch
		{
			0 => 1600, 
			1 => 1606, 
			2 => 1607, 
			_ => 1600, 
		};
	}

	public static void eplGeoE_FindMinMaxOfLegList(Eps_Leg _pLeg, ref Eps_Point _pMinPnt, ref Eps_Point _pMaxPnt)
	{
		Eps_Leg eps_Leg = new Eps_Leg();
		Eps_Point _MinPnt = new Eps_Point();
		Eps_Point _MaxPnt = new Eps_Point();
		int num = 0;
		_pMinPnt.x = double.MaxValue;
		_pMaxPnt.x = double.MinValue;
		_pMinPnt.y = double.MaxValue;
		_pMaxPnt.y = double.MinValue;
		_pMinPnt.z = double.MaxValue;
		_pMaxPnt.z = double.MinValue;
		for (eps_Leg = _pLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				CalcMinMax(eps_Leg.Line.StartPnt, ref _pMinPnt, ref _pMaxPnt);
				CalcMinMax(eps_Leg.Line.EndPnt, ref _pMinPnt, ref _pMaxPnt);
			}
			else if (eps_Leg.Arc != null)
			{
				eplGeoE_FindMinMaxOfArc(eps_Leg.Arc, ref _MinPnt, ref _MaxPnt);
				CalcMinMax(_MinPnt, ref _pMinPnt, ref _pMaxPnt);
				CalcMinMax(_MaxPnt, ref _pMinPnt, ref _pMaxPnt);
			}
			else if (eps_Leg.HermiteSpline != null)
			{
				for (num = 0; num < _pLeg.HermiteSpline.ControlPoints.Count; num++)
				{
					Eps_Point pPoint = new Eps_Point(eps_Leg.HermiteSpline.ControlPoints[num]);
					CalcMinMax(pPoint, ref _pMinPnt, ref _pMaxPnt);
				}
			}
			else if (eps_Leg.NurbSpline != null)
			{
				for (num = 0; num < _pLeg.NurbSpline.CtrlPoints.Count; num++)
				{
					Eps_Point pPoint2 = new Eps_Point(eps_Leg.NurbSpline.CtrlPoints[num]);
					CalcMinMax(pPoint2, ref _pMinPnt, ref _pMaxPnt);
				}
			}
		}
	}

	public static void eplGeoE_GetLegStartAndEndPoints(Eps_Leg _pLeg, ref Eps_Point _pLegStartPnt, ref Eps_Point _pLegEndPnt)
	{
		_pLegStartPnt = new Eps_Point();
		_pLegEndPnt = new Eps_Point();
		if (_pLeg != null)
		{
			if (_pLeg.Line != null)
			{
				_pLegStartPnt = _pLeg.Line.StartPnt;
				_pLegEndPnt = _pLeg.Line.EndPnt;
			}
			else if (_pLeg.Arc != null)
			{
				_pLegStartPnt = _pLeg.Arc.StartPnt;
				_pLegEndPnt = _pLeg.Arc.EndPnt;
			}
			else if (_pLeg.HermiteSpline != null)
			{
				_pLegStartPnt = new Eps_Point(_pLeg.HermiteSpline.GetEndPoint(0));
				_pLegEndPnt = new Eps_Point(_pLeg.HermiteSpline.GetEndPoint(1));
			}
			else if (_pLeg.NurbSpline != null)
			{
				_pLegStartPnt = new Eps_Point(_pLeg.NurbSpline.GetEndPoint(0));
				_pLegEndPnt = new Eps_Point(_pLeg.NurbSpline.GetEndPoint(1));
			}
		}
	}

	public static void eplGeoE_SolveMidPntByArcStruct(Eps_Arc _ArcStruct, ref Eps_Point _MidPnt)
	{
		double _dDirection = 0.0;
		eplGeoE_CalcHorzDirBtwnTwoPnts(_ArcStruct.StartPnt, _ArcStruct.EndPnt, ref _dDirection);
		if (_ArcStruct.Direction == -1)
		{
			eplGeoE_CalcPolarToXY(_ArcStruct.RadiusPnt, _dDirection + Math.PI / 2.0, _ArcStruct.Radius, ref _MidPnt);
		}
		else
		{
			eplGeoE_CalcPolarToXY(_ArcStruct.RadiusPnt, _dDirection - Math.PI / 2.0, _ArcStruct.Radius, ref _MidPnt);
		}
	}

	public static int eplDsmE_AddArcToLegList(Eps_Arc _ArcToAdd, ref Eps_Leg _LegList)
	{
		return eplDsmE_AddArcToLegList(_ArcToAdd, ref _LegList, _bIs3D: false, 0.0, 0.0);
	}

	public static int eplDsmE_AddArcToLegList(Eps_Arc _ArcToAdd, ref Eps_Leg _LegList, bool _bIs3D, double _dStartWidth, double _dEndWidth)
	{
		Eps_Leg eps_Leg = new Eps_Leg();
		eps_Leg.Arc = _ArcToAdd;
		eps_Leg.Is3D = _bIs3D;
		return eplDsmE_AddLegToList(eps_Leg, ref _LegList, _dStartWidth, _dEndWidth);
	}

	public static int eplDsmE_AddLineToLegList(Eps_Line _LineToAdd, ref Eps_Leg _LegList)
	{
		return eplDsmE_AddLineToLegList(_LineToAdd, ref _LegList, _bIs3D: false, 0.0, 0.0);
	}

	public static int eplDsmE_AddLineToLegList(Eps_Line _LineToAdd, ref Eps_Leg _LegList, bool _bIs3D, double _dStartWidth, double _dEndWidth)
	{
		Eps_Leg eps_Leg = new Eps_Leg();
		eps_Leg.Line = _LineToAdd;
		eps_Leg.Is3D = _bIs3D;
		return eplDsmE_AddLegToList(eps_Leg, ref _LegList, _dStartWidth, _dEndWidth);
	}

	public static int eplDsmE_AddLegToList(Eps_Leg _LegToAdd, ref Eps_Leg _LegList, double _dStartWidth, double _dEndWidth)
	{
		Eps_Leg eps_Leg = new Eps_Leg();
		Eps_Point eps_Point = new Eps_Point();
		eps_Leg = _LegToAdd;
		eps_Leg.Next = null;
		eplDsmE_AppendLegListToList(eps_Leg, ref _LegList);
		return 0;
	}

	public static void eplDsmE_AppendLegListToList(Eps_Leg _ListToAdd, ref Eps_Leg _LegList)
	{
		Eps_Leg eps_Leg = _LegList;
		if (eps_Leg != null)
		{
			while (eps_Leg.Next != null)
			{
				eps_Leg = eps_Leg.Next;
			}
			eps_Leg.Next = _ListToAdd;
		}
		else
		{
			_LegList = _ListToAdd;
		}
	}

	public static int eplFilP_GetSupportPath(string _strFileName, ref string _strSupportPath)
	{
		_strSupportPath = string.Empty;
		return 6;
	}

	public static bool eplLicM_GetLicense()
	{
		return true;
	}

	public static bool eplLicM_ReturnLicense()
	{
		return true;
	}

	public static void epl_UnloadHelper(bool _bIs64BitInstall)
	{
	}

	public static bool eplLicM_LicenseManagement()
	{
		return true;
	}

	public static bool eplLicM_IsNodeLicense()
	{
		return true;
	}

	public static bool eplLicM_BorrowNetworkLicense()
	{
		return false;
	}

	public static bool eplLicM_IsBorrowedNetworkLicense()
	{
		return false;
	}

	public static bool eplLicM_ReturnBorrowedLicense()
	{
		return false;
	}
}

