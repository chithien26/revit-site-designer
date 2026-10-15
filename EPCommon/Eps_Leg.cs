using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace EPCommon;

[StructLayout(LayoutKind.Sequential)]
public class Eps_Leg
{
	private bool m_bIs3D;

	private int m_nDrawingType;

	private Eps_Arc m_Arc;

	private Eps_Line m_Line;

	private NurbSpline m_NurbSpline;

	private HermiteSpline m_HermiteSpline;

	private int m_nSplineNum;

	private Eps_Leg m_Next;

	private Element m_Element;

	private bool m_bIsReverse;

	public bool Is3D
	{
		get
		{
			return m_bIs3D;
		}
		set
		{
			m_bIs3D = value;
		}
	}

	public int DrawingType
	{
		get
		{
			return m_nDrawingType;
		}
		set
		{
			m_nDrawingType = value;
		}
	}

	public Eps_Arc Arc
	{
		get
		{
			return m_Arc;
		}
		set
		{
			m_Arc = value;
		}
	}

	public Eps_Line Line
	{
		get
		{
			return m_Line;
		}
		set
		{
			m_Line = value;
		}
	}

	public NurbSpline NurbSpline
	{
		get
		{
			return m_NurbSpline;
		}
		set
		{
			m_NurbSpline = value;
		}
	}

	public HermiteSpline HermiteSpline
	{
		get
		{
			return m_HermiteSpline;
		}
		set
		{
			m_HermiteSpline = value;
		}
	}

	public int SplineNum
	{
		get
		{
			return m_nSplineNum;
		}
		set
		{
			m_nSplineNum = value;
		}
	}

	public Eps_Leg Next
	{
		get
		{
			return m_Next;
		}
		set
		{
			m_Next = value;
		}
	}

	public Element Element
	{
		get
		{
			return m_Element;
		}
		set
		{
			m_Element = value;
		}
	}

	public ElementSet ElementSet
	{
		get
		{
			ElementSet elementSet = null;
			for (Eps_Leg eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.m_Next)
			{
				if (elementSet == null)
				{
					elementSet = new ElementSet();
				}
				if (eps_Leg.Element != null)
				{
					elementSet.Insert(eps_Leg.Element);
				}
			}
			return elementSet;
		}
	}

	public List<Element> ElementList
	{
		get
		{
			List<Element> list = null;
			for (Eps_Leg eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.m_Next)
			{
				if (list == null)
				{
					list = new List<Element>();
				}
				if (eps_Leg.Element != null)
				{
					list.Add(eps_Leg.Element);
				}
			}
			return list;
		}
	}

	public Eps_Leg Last
	{
		get
		{
			Eps_Leg eps_Leg = this;
			while (eps_Leg.m_Next != null)
			{
				eps_Leg = eps_Leg.m_Next;
			}
			return eps_Leg;
		}
	}

	public int LegCount
	{
		get
		{
			int num = 0;
			for (Eps_Leg eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.m_Next)
			{
				if (eps_Leg.Line != null || eps_Leg.Arc != null)
				{
					num++;
				}
			}
			return num;
		}
	}

	public Eps_Point LegStartPnt
	{
		get
		{
			Eps_Point result = null;
			if (m_Arc != null)
			{
				result = m_Arc.StartPnt;
			}
			else if (m_Line != null)
			{
				result = m_Line.StartPnt;
			}
			return result;
		}
	}

	public Eps_Point LegEndPnt
	{
		get
		{
			Eps_Point result = null;
			Eps_Leg last = Last;
			if (last.Arc != null)
			{
				result = last.Arc.EndPnt;
			}
			else if (last.Line != null)
			{
				result = last.Line.EndPnt;
			}
			return result;
		}
	}

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

	public Eps_Leg()
	{
		m_bIs3D = false;
		m_nDrawingType = 0;
		m_nSplineNum = -1;
	}

	public bool IsEqual(Eps_Leg another)
	{
		if (Is3D != another.Is3D)
		{
			return false;
		}
		if (DrawingType != another.DrawingType)
		{
			return false;
		}
		if (SplineNum != another.SplineNum)
		{
			return false;
		}
		if (Element == another.Element)
		{
			if (Element != null && Element.Id != another.Element.Id)
			{
				return false;
			}
		}
		else
		{
			if (Element == null || another.Element == null)
			{
				return false;
			}
			if (Element.Id != another.Element.Id)
			{
				return false;
			}
		}
		if ((Arc == null && another.Arc != null) || (Arc != null && another.Arc == null))
		{
			return false;
		}
		if (Arc != null && another.Arc != null && !Arc.IsEqual(another.Arc))
		{
			return false;
		}
		if ((Line == null && another.Line != null) || (Line != null && another.Line == null))
		{
			return false;
		}
		if (Line != null && another.Line != null && !Line.IsEqual(another.Line))
		{
			return false;
		}
		Eps_Leg next = Next;
		Eps_Leg next2 = another.Next;
		if ((next == null && next2 != null) || (next != null && next2 == null))
		{
			return false;
		}
		if (next == null && next2 == null)
		{
			return true;
		}
		while (next != null)
		{
			if (!next.IsEqual(next2))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsLegClosed()
	{
		if (LegCount > 0 && RevitDefines.GT(RevitGeometry.eplGeoE_CalcLegListLength(this), 0.0) && EPShared.eplGeoE_ArePntsDuplicates2D(LegStartPnt, LegEndPnt))
		{
			return true;
		}
		return false;
	}

	public bool IsArcInLeg(Eps_Arc _CompareArc)
	{
		for (Eps_Leg eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.m_Next)
		{
			if (eps_Leg.Arc != null && eps_Leg.Arc.IsEqual(_CompareArc))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsLineInLeg(Eps_Line _CompareLine)
	{
		for (Eps_Leg eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.m_Next)
		{
			if (eps_Leg.Line != null && eps_Leg.Line.IsEqual(_CompareLine))
			{
				return true;
			}
		}
		return false;
	}

	public static Eps_Leg CopyLeg(Eps_Leg _CompleteLeg)
	{
		Eps_Leg eps_Leg = SingleLeg(_CompleteLeg);
		Eps_Leg eps_Leg2 = _CompleteLeg;
		while (eps_Leg2.Next != null)
		{
			eps_Leg.Last.Next = SingleLeg(eps_Leg2.Next);
			eps_Leg2 = eps_Leg2.Next;
		}
		return eps_Leg;
	}

	public static Eps_Leg SingleLeg(Eps_Leg _CompleteLeg)
	{
		Eps_Leg eps_Leg = new Eps_Leg();
		eps_Leg.Is3D = _CompleteLeg.Is3D;
		eps_Leg.DrawingType = _CompleteLeg.DrawingType;
		eps_Leg.SplineNum = _CompleteLeg.SplineNum;
		eps_Leg.Element = _CompleteLeg.Element;
		eps_Leg.HermiteSpline = _CompleteLeg.HermiteSpline;
		eps_Leg.NurbSpline = _CompleteLeg.NurbSpline;
		eps_Leg.Arc = _CompleteLeg.Arc;
		eps_Leg.Line = _CompleteLeg.Line;
		eps_Leg.Next = null;
		return eps_Leg;
	}

	public Eps_Leg Reverse()
	{
		List<Eps_Leg> list = new List<Eps_Leg>();
		for (Eps_Leg eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			list.Add(SingleLeg(eps_Leg));
		}
		if (list.Count > 0)
		{
			Eps_Leg eps_Leg2 = new Eps_Leg();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				Eps_Leg eps_Leg = list[num];
				if (eps_Leg.Arc != null)
				{
					eps_Leg.Arc = eps_Leg.Arc.Reverse();
				}
				else if (eps_Leg.Line != null)
				{
					eps_Leg.Line = eps_Leg.Line.Reverse();
				}
				if (num == list.Count - 1)
				{
					eps_Leg2 = eps_Leg;
				}
				else
				{
					eps_Leg2.Last.Next = eps_Leg;
				}
			}
			eps_Leg2.IsReverse = !m_bIsReverse;
			return eps_Leg2;
		}
		return null;
	}

	public CurveArray CurveArray(UIDocument _CurDocument)
	{
		return CurveArray(_CurDocument.Document);
	}

	public CurveArray CurveArray(Document _CurDocument)
	{
		CurveArray curveArray = new CurveArray();
		for (Eps_Leg eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				if (!EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, 0.0025))
				{
					curveArray.Append(eps_Leg.Line.RevitLine(_CurDocument));
				}
			}
			else if (eps_Leg.Arc != null && eps_Leg.Arc.Radius > 0.0)
			{
				curveArray.Append(eps_Leg.Arc.RevitArc(_CurDocument));
			}
		}
		return curveArray;
	}

	public void ConvertLegToLineArcArrays(ref Eps_Line[] aLines, ref int nLineCount, ref Eps_Arc[] aArcs, ref int nArcCount, ref int[] aLegOrder, bool _bExtendLeg)
	{
		ConvertLegToLineArcArrays(ref aLines, ref nLineCount, ref aArcs, ref nArcCount, ref aLegOrder, null, _bUseToposurfaceElevation: false, _bUseConstantElevation: false, 0.0, null, _bExtendLeg, null);
	}

	public void ConvertLegToLineArcArrays(ref Eps_Line[] aLines, ref int nLineCount, ref Eps_Arc[] aArcs, ref int nArcCount, ref int[] aLegOrder, TopographySurfaceContainer _SurfaceContainer, bool _bUseToposurfaceElevation, bool _bUseConstantElevation, double _dElevation, bool _bExtendLeg)
	{
		List<TopographySurfaceContainer> list = new List<TopographySurfaceContainer>();
		if (_SurfaceContainer != null)
		{
			list.Add(_SurfaceContainer);
		}
		ConvertLegToLineArcArrays(ref aLines, ref nLineCount, ref aArcs, ref nArcCount, ref aLegOrder, list, _bUseToposurfaceElevation, _bUseConstantElevation, _dElevation, null, _bExtendLeg, null);
	}

	public void ConvertLegToLineArcArrays(ref Eps_Line[] aLines, ref int nLineCount, ref Eps_Arc[] aArcs, ref int nArcCount, ref int[] aLegOrder, TopographySurfaceContainer _SurfaceContainer, bool _bUseToposurfaceElevation, bool _bUseConstantElevation, double _dElevation, bool _bExtendLeg, List<Eps_Point> _ApplySurfaceElevationPoints)
	{
		List<TopographySurfaceContainer> list = new List<TopographySurfaceContainer>();
		if (_SurfaceContainer != null)
		{
			list.Add(_SurfaceContainer);
		}
		ConvertLegToLineArcArrays(ref aLines, ref nLineCount, ref aArcs, ref nArcCount, ref aLegOrder, list, _bUseToposurfaceElevation, _bUseConstantElevation, _dElevation, null, _bExtendLeg, _ApplySurfaceElevationPoints);
	}

	public void ConvertLegToLineArcArrays(ref Eps_Line[] aLines, ref int nLineCount, ref Eps_Arc[] aArcs, ref int nArcCount, ref int[] aLegOrder, List<TopographySurfaceContainer> _SurfaceList, bool _bUseToposurfaceElevation, bool _bUseConstantElevation, double _dElevation, Phase _CurPhase, bool _bExtendLeg, List<Eps_Point> _ApplySurfaceElevationPoints)
	{
		List<Eps_Line> list = new List<Eps_Line>();
		List<Eps_Arc> list2 = new List<Eps_Arc>();
		List<int> list3 = new List<int>();
		double _dDirection = 0.0;
		double num = 0.0;
		Eps_Leg eps_Leg = this;
		Eps_Point eps_Point = null;
		bool flag = true;
		double _dElevation2 = 0.0;
		double _dElevation3 = 0.0;
		int num2 = 0;
		bool flag2 = false;
		bool flag3 = false;
		while (eps_Leg != null)
		{
			if (eps_Leg.Line != null)
			{
				Eps_Line eps_Line = new Eps_Line(eps_Leg.Line);
				if (_ApplySurfaceElevationPoints != null)
				{
					for (num2 = 0; num2 < _ApplySurfaceElevationPoints.Count; num2++)
					{
						if (eps_Line.StartPnt.IsEqual(_ApplySurfaceElevationPoints[num2]))
						{
							flag2 = true;
							eps_Line.StartPnt.z = _ApplySurfaceElevationPoints[num2].z;
							if (flag3)
							{
								break;
							}
						}
						else if (eps_Line.EndPnt.IsEqual(_ApplySurfaceElevationPoints[num2]))
						{
							flag3 = true;
							eps_Line.EndPnt.z = _ApplySurfaceElevationPoints[num2].z;
							if (flag2)
							{
								break;
							}
						}
					}
				}
				if (_bExtendLeg && flag && !IsLegClosed())
				{
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Line.EndPnt, eps_Line.StartPnt, ref _dDirection);
					eps_Point = new Eps_Point();
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, _dDirection, 0.01, ref eps_Point);
					eps_Line.StartPnt.x = eps_Point.x;
					eps_Line.StartPnt.y = eps_Point.y;
				}
				if (!flag2)
				{
					if (_bUseToposurfaceElevation)
					{
						if (_SurfaceList != null && _SurfaceList.Count > 0)
						{
							RevitUtility.GetSurfaceContainingPoint(eps_Line.StartPnt, _SurfaceList, _CurPhase, ref _dElevation2);
						}
						else
						{
							_dElevation2 = eps_Line.StartPnt.z;
						}
						eps_Line.StartPnt.z = _dElevation2 + _dElevation;
					}
					else if (_bUseConstantElevation)
					{
						eps_Line.StartPnt.z = _dElevation;
					}
				}
				else
				{
					flag2 = false;
				}
				if (!flag3)
				{
					if (_bUseToposurfaceElevation)
					{
						if (_SurfaceList != null && _SurfaceList.Count > 0)
						{
							RevitUtility.GetSurfaceContainingPoint(eps_Line.EndPnt, _SurfaceList, _CurPhase, ref _dElevation3);
						}
						else
						{
							_dElevation3 = eps_Line.EndPnt.z;
						}
						eps_Line.EndPnt.z = _dElevation3 + _dElevation;
					}
					else if (_bUseConstantElevation)
					{
						eps_Line.EndPnt.z = _dElevation;
					}
				}
				else
				{
					flag3 = false;
				}
				if (_bExtendLeg && eps_Leg.Next == null && !IsLegClosed())
				{
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Line.StartPnt, eps_Line.EndPnt, ref _dDirection);
					eps_Point = new Eps_Point();
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, _dDirection, 0.01, ref eps_Point);
					eps_Line.EndPnt.x = eps_Point.x;
					eps_Line.EndPnt.y = eps_Point.y;
				}
				list.Add(eps_Line);
				list3.Add(0);
			}
			else if (eps_Leg.Arc != null)
			{
				Eps_Arc eps_Arc = new Eps_Arc(eps_Leg.Arc);
				if (_ApplySurfaceElevationPoints != null)
				{
					for (num2 = 0; num2 < _ApplySurfaceElevationPoints.Count; num2++)
					{
						if (eps_Arc.StartPnt.IsEqual(_ApplySurfaceElevationPoints[num2]))
						{
							flag2 = true;
							eps_Arc.StartPnt.z = _ApplySurfaceElevationPoints[num2].z;
							if (flag3)
							{
								break;
							}
						}
						else if (eps_Arc.EndPnt.IsEqual(_ApplySurfaceElevationPoints[num2]))
						{
							flag3 = true;
							eps_Arc.EndPnt.z = _ApplySurfaceElevationPoints[num2].z;
							if (flag2)
							{
								break;
							}
						}
					}
				}
				if (_bExtendLeg && flag && !IsLegClosed())
				{
					num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Arc.Radius, eps_Arc.Delta);
					num += 0.01;
					eps_Arc.Delta = num / eps_Arc.Radius;
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Arc.RadiusPnt, eps_Arc.EndPnt, ref _dDirection);
					_dDirection = ((eps_Arc.Direction != -1) ? (_dDirection - eps_Arc.Delta) : (_dDirection + eps_Arc.Delta));
					eps_Point = new Eps_Point();
					EPShared.eplGeoE_CalcPolarToXY(eps_Arc.RadiusPnt, _dDirection, eps_Arc.Radius, ref eps_Point);
					eps_Arc.StartPnt.x = eps_Point.x;
					eps_Arc.StartPnt.y = eps_Point.y;
				}
				if (!flag2)
				{
					if (_bUseToposurfaceElevation)
					{
						if (_SurfaceList != null && _SurfaceList.Count > 0)
						{
							RevitUtility.GetSurfaceContainingPoint(eps_Arc.StartPnt, _SurfaceList, _CurPhase, ref _dElevation2);
						}
						else
						{
							_dElevation2 = eps_Arc.StartPnt.z;
						}
						eps_Arc.StartPnt.z = _dElevation2 + _dElevation;
					}
					else if (_bUseConstantElevation)
					{
						eps_Arc.StartPnt.z = _dElevation;
					}
				}
				else
				{
					flag2 = false;
				}
				if (!flag3)
				{
					if (_bUseToposurfaceElevation)
					{
						if (_SurfaceList != null && _SurfaceList.Count > 0)
						{
							RevitUtility.GetSurfaceContainingPoint(eps_Arc.EndPnt, _SurfaceList, _CurPhase, ref _dElevation3);
						}
						else
						{
							_dElevation3 = eps_Arc.EndPnt.z;
						}
						eps_Arc.EndPnt.z = _dElevation3 + _dElevation;
					}
					else if (_bUseConstantElevation)
					{
						eps_Arc.EndPnt.z = _dElevation;
					}
				}
				else
				{
					flag3 = false;
				}
				eps_Arc.RadiusPnt.z = (eps_Arc.StartPnt.z + eps_Arc.EndPnt.z) / 2.0;
				if (_bExtendLeg && eps_Leg.Next == null && !IsLegClosed())
				{
					num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Arc.Radius, eps_Arc.Delta);
					num += 0.01;
					eps_Arc.Delta = num / eps_Arc.Radius;
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Arc.RadiusPnt, eps_Arc.StartPnt, ref _dDirection);
					_dDirection = ((eps_Arc.Direction != -1) ? (_dDirection + eps_Arc.Delta) : (_dDirection - eps_Arc.Delta));
					eps_Point = new Eps_Point();
					EPShared.eplGeoE_CalcPolarToXY(eps_Arc.RadiusPnt, _dDirection, eps_Arc.Radius, ref eps_Point);
					eps_Arc.EndPnt.x = eps_Point.x;
					eps_Arc.EndPnt.y = eps_Point.y;
				}
				list2.Add(eps_Arc);
				list3.Add(1);
			}
			eps_Leg = eps_Leg.Next;
			flag = false;
		}
		aLines = list.ToArray();
		aArcs = list2.ToArray();
		aLegOrder = list3.ToArray();
		nLineCount = list.Count;
		nArcCount = list2.Count;
	}

	public static Eps_Leg ConvertLineArcArraysToLeg(Eps_Line[] aLines, int nLineCount, Eps_Arc[] aArcs, int nArcCount, int[] aLegOrder)
	{
		int num = 0;
		int num2 = 0;
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		for (int i = 0; i < nLineCount + nArcCount; i++)
		{
			eps_Leg2 = new Eps_Leg();
			if (aLegOrder[i] == 0)
			{
				eps_Leg2.Line = new Eps_Line(aLines[num]);
				eps_Leg2.Element = null;
				if (eps_Leg == null)
				{
					eps_Leg = SingleLeg(eps_Leg2);
				}
				else
				{
					eps_Leg.Last.Next = SingleLeg(eps_Leg2);
				}
				num++;
			}
			else if (aLegOrder[i] == 1)
			{
				eps_Leg2.Arc = new Eps_Arc(aArcs[num2]);
				eps_Leg2.Element = null;
				if (eps_Leg == null)
				{
					eps_Leg = SingleLeg(eps_Leg2);
				}
				else
				{
					eps_Leg.Last.Next = SingleLeg(eps_Leg2);
				}
				num2++;
			}
		}
		return eps_Leg;
	}

	public static double FindMinimumDistanceBetweenLegs(Eps_Leg _FirstLegList, Eps_Leg _SecondLegList)
	{
		double _dDistance = double.MaxValue;
		double _dDistance2 = double.MaxValue;
		double _dDistance3 = double.MaxValue;
		double _dDistance4 = double.MaxValue;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_FirstLegList.LegStartPnt, _SecondLegList.LegStartPnt, ref _dDistance);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_FirstLegList.LegStartPnt, _SecondLegList.LegEndPnt, ref _dDistance2);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_FirstLegList.LegEndPnt, _SecondLegList.LegStartPnt, ref _dDistance3);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_FirstLegList.LegEndPnt, _SecondLegList.LegEndPnt, ref _dDistance4);
		return Math.Min(_dDistance, Math.Min(_dDistance2, Math.Min(_dDistance3, _dDistance4)));
	}

	public static void CombineLegListsAtShortestGaps(ref Eps_Leg _CombinedLegList, Eps_Leg _AddLegList)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		double _dDistance = double.MaxValue;
		double _dDistance2 = double.MaxValue;
		double _dDistance3 = double.MaxValue;
		double _dDistance4 = double.MaxValue;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_CombinedLegList.LegStartPnt, _AddLegList.LegStartPnt, ref _dDistance);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_CombinedLegList.LegStartPnt, _AddLegList.LegEndPnt, ref _dDistance2);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_CombinedLegList.LegEndPnt, _AddLegList.LegStartPnt, ref _dDistance3);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_CombinedLegList.LegEndPnt, _AddLegList.LegEndPnt, ref _dDistance4);
		double num = Math.Min(_dDistance, Math.Min(_dDistance2, Math.Min(_dDistance3, _dDistance4)));
		if (num == _dDistance || num == _dDistance2)
		{
			eps_Leg = ((num != _dDistance) ? CopyLeg(_AddLegList) : CopyLeg(_AddLegList.Reverse()));
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg.LegEndPnt, _CombinedLegList.LegStartPnt))
			{
				eps_Leg2 = new Eps_Leg();
				eps_Leg2.Line = new Eps_Line();
				eps_Leg2.Line.StartPnt = eps_Leg.LegEndPnt;
				eps_Leg2.Line.EndPnt = _CombinedLegList.LegStartPnt;
				eps_Leg.Last.Next = eps_Leg2;
			}
			eps_Leg.Last.Next = _CombinedLegList;
			_CombinedLegList = eps_Leg;
		}
		else if (num == _dDistance3 || num == _dDistance4)
		{
			eps_Leg = ((num != _dDistance3) ? CopyLeg(_AddLegList.Reverse()) : CopyLeg(_AddLegList));
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CombinedLegList.LegEndPnt, eps_Leg.LegStartPnt))
			{
				eps_Leg2 = new Eps_Leg();
				eps_Leg2.Line = new Eps_Line();
				eps_Leg2.Line.StartPnt = _CombinedLegList.LegEndPnt;
				eps_Leg2.Line.EndPnt = eps_Leg.LegStartPnt;
				_CombinedLegList.Last.Next = SingleLeg(eps_Leg2);
			}
			_CombinedLegList.Last.Next = eps_Leg;
		}
	}

	public static bool InsertLineLegSegment(ref Eps_Leg _CurrentLeg, Eps_Line _CurrentLine)
	{
		return InsertLineLegSegment(ref _CurrentLeg, _CurrentLine, _bAppendToEndOnly: false);
	}

	public static bool InsertLineLegSegment(ref Eps_Leg _CurrentLeg, Eps_Line _CurrentLine, bool _bAppendToEndOnly)
	{
		Eps_Leg eps_Leg = null;
		bool result = false;
		if (_CurrentLeg == null)
		{
			_CurrentLeg = new Eps_Leg();
			_CurrentLeg.Line = _CurrentLine;
			result = true;
		}
		else if ((_CurrentLeg.Line != null && _CurrentLeg.Line.IsEqual(_CurrentLine)) || (_CurrentLeg.Last.Line != null && _CurrentLeg.Last.Line.IsEqual(_CurrentLine)))
		{
			result = true;
		}
		else if (_CurrentLeg.LegEndPnt.IsEqual(_CurrentLine.StartPnt))
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = _CurrentLine;
			_CurrentLeg.Last.Next = eps_Leg;
			result = true;
		}
		else if (_CurrentLeg.LegEndPnt.IsEqual(_CurrentLine.EndPnt))
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = _CurrentLine.Reverse();
			_CurrentLeg.Last.Next = eps_Leg;
			result = true;
		}
		else if (_CurrentLeg.LegStartPnt.IsEqual(_CurrentLine.StartPnt) && !_bAppendToEndOnly)
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = _CurrentLine.Reverse();
			eps_Leg.Last.Next = _CurrentLeg;
			_CurrentLeg = eps_Leg;
			result = true;
		}
		else if (_CurrentLeg.LegStartPnt.IsEqual(_CurrentLine.EndPnt) && !_bAppendToEndOnly)
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = _CurrentLine;
			eps_Leg.Last.Next = _CurrentLeg;
			_CurrentLeg = eps_Leg;
			result = true;
		}
		return result;
	}

	public static bool InsertArcLegSegment(ref Eps_Leg _CurrentLeg, Eps_Arc _CurrentArc)
	{
		return InsertArcLegSegment(ref _CurrentLeg, _CurrentArc, _bAppendToEndOnly: false);
	}

	public static bool InsertArcLegSegment(ref Eps_Leg _CurrentLeg, Eps_Arc _CurrentArc, bool _bAppendToEndOnly)
	{
		Eps_Leg eps_Leg = null;
		bool result = false;
		if (_CurrentLeg == null)
		{
			_CurrentLeg = new Eps_Leg();
			_CurrentLeg.Arc = _CurrentArc;
			result = true;
		}
		else if ((_CurrentLeg.Arc != null && _CurrentLeg.Arc.IsEqual(_CurrentArc)) || (_CurrentLeg.Last.Arc != null && _CurrentLeg.Last.Arc.IsEqual(_CurrentArc)))
		{
			result = true;
		}
		else if (_CurrentLeg.LegEndPnt.IsEqual(_CurrentArc.StartPnt))
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Arc = _CurrentArc;
			_CurrentLeg.Last.Next = eps_Leg;
			result = true;
		}
		else if (_CurrentLeg.LegEndPnt.IsEqual(_CurrentArc.EndPnt))
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Arc = _CurrentArc.Reverse();
			_CurrentLeg.Last.Next = eps_Leg;
			result = true;
		}
		else if (_CurrentLeg.LegStartPnt.IsEqual(_CurrentArc.StartPnt) && !_bAppendToEndOnly)
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Arc = _CurrentArc.Reverse();
			eps_Leg.Last.Next = _CurrentLeg;
			_CurrentLeg = eps_Leg;
			result = true;
		}
		else if (_CurrentLeg.LegStartPnt.IsEqual(_CurrentArc.EndPnt) && !_bAppendToEndOnly)
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Arc = _CurrentArc;
			eps_Leg.Last.Next = _CurrentLeg;
			_CurrentLeg = eps_Leg;
			result = true;
		}
		return result;
	}

	public static bool InsertLeg(ref Eps_Leg _CurrentLeg, Eps_Leg _AddLeg)
	{
		Eps_Leg eps_Leg = null;
		bool result = false;
		if (_CurrentLeg == null)
		{
			_CurrentLeg = CopyLeg(_AddLeg);
			result = true;
		}
		else if (_CurrentLeg.LegStartPnt.IsEqual(_AddLeg.LegStartPnt))
		{
			eps_Leg = CopyLeg(_AddLeg.Reverse());
			eps_Leg.Last.Next = _CurrentLeg;
			_CurrentLeg = eps_Leg;
			result = true;
		}
		else if (_CurrentLeg.LegStartPnt.IsEqual(_AddLeg.LegEndPnt))
		{
			eps_Leg = CopyLeg(_AddLeg);
			eps_Leg.Last.Next = _CurrentLeg;
			_CurrentLeg = eps_Leg;
			result = true;
		}
		else if (_CurrentLeg.LegEndPnt.IsEqual(_AddLeg.LegStartPnt))
		{
			eps_Leg = CopyLeg(_AddLeg);
			_CurrentLeg.Last.Next = eps_Leg;
			result = true;
		}
		else if (_CurrentLeg.LegEndPnt.IsEqual(_AddLeg.LegEndPnt))
		{
			eps_Leg = CopyLeg(_AddLeg.Reverse());
			_CurrentLeg.Last.Next = eps_Leg;
			result = true;
		}
		return result;
	}

	public Eps_Leg ApplyElevationsToLeg()
	{
		return ApplyElevationsToLeg(null, _bUseToposurfaceElevation: false, _bUseConstantElevation: false, 0.0, null, null);
	}

	public Eps_Leg ApplyElevationsToLeg(TopographySurfaceContainer _SurfaceContainer, bool _bUseToposurfaceElevation, bool _bUseConstantElevation, double _dConstElevation)
	{
		List<TopographySurfaceContainer> list = new List<TopographySurfaceContainer>();
		list.Add(_SurfaceContainer);
		return ApplyElevationsToLeg(list, _bUseToposurfaceElevation, _bUseConstantElevation, _dConstElevation, null, null);
	}

	public Eps_Leg ApplyElevationsToLeg(List<TopographySurfaceContainer> _SurfaceList, bool _bUseToposurfaceElevation, bool _bUseConstantElevation, double _dConstElevation, Phase _CurPhase)
	{
		return ApplyElevationsToLeg(_SurfaceList, _bUseToposurfaceElevation, _bUseConstantElevation, _dConstElevation, _CurPhase, null);
	}

	public Eps_Leg ApplyElevationsToLeg(TopographySurfaceContainer _SurfaceContainer, bool _bUseToposurfaceElevation, bool _bUseConstantElevation, double _dConstElevation, List<Eps_Point> _ApplySurfaceElevationPoints)
	{
		List<TopographySurfaceContainer> list = new List<TopographySurfaceContainer>();
		if (_SurfaceContainer != null)
		{
			list.Add(_SurfaceContainer);
		}
		return ApplyElevationsToLeg(list, _bUseToposurfaceElevation, _bUseConstantElevation, _dConstElevation, null, _ApplySurfaceElevationPoints);
	}

	public Eps_Leg ApplyElevationsToLeg(List<TopographySurfaceContainer> _SurfaceList, bool _bUseToposurfaceElevation, bool _bUseConstantElevation, double _dConstElevation, Phase _CurPhase, List<Eps_Point> _ApplySurfaceElevationPoints)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		double _dElevation = 0.0;
		double _dElevation2 = 0.0;
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			eps_Leg3 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line(eps_Leg.Line);
				if (_ApplySurfaceElevationPoints != null)
				{
					for (num = 0; num < _ApplySurfaceElevationPoints.Count; num++)
					{
						if (eps_Leg3.Line.StartPnt.IsEqual(_ApplySurfaceElevationPoints[num]))
						{
							flag = true;
							eps_Leg3.Line.StartPnt.z = _ApplySurfaceElevationPoints[num].z;
							if (flag2)
							{
								break;
							}
						}
						else if (eps_Leg3.Line.EndPnt.IsEqual(_ApplySurfaceElevationPoints[num]))
						{
							flag2 = true;
							eps_Leg3.Line.EndPnt.z = _ApplySurfaceElevationPoints[num].z;
							if (flag)
							{
								break;
							}
						}
					}
				}
				if (!flag)
				{
					if (_bUseToposurfaceElevation)
					{
						if (_SurfaceList != null && _SurfaceList.Count > 0)
						{
							RevitUtility.GetSurfaceContainingPoint(eps_Leg3.Line.StartPnt, _SurfaceList, _CurPhase, ref _dElevation);
						}
						else
						{
							_dElevation = eps_Leg3.Line.StartPnt.z;
						}
						eps_Leg3.Line.StartPnt.z = _dElevation + _dConstElevation;
					}
					else if (_bUseConstantElevation)
					{
						eps_Leg3.Line.StartPnt.z = _dConstElevation;
					}
				}
				else
				{
					flag = false;
				}
				if (!flag2)
				{
					if (_bUseToposurfaceElevation)
					{
						if (_SurfaceList != null && _SurfaceList.Count > 0)
						{
							RevitUtility.GetSurfaceContainingPoint(eps_Leg3.Line.EndPnt, _SurfaceList, _CurPhase, ref _dElevation2);
						}
						else
						{
							_dElevation2 = eps_Leg3.Line.EndPnt.z;
						}
						eps_Leg3.Line.EndPnt.z = _dElevation2 + _dConstElevation;
					}
					else if (_bUseConstantElevation)
					{
						eps_Leg3.Line.EndPnt.z = _dConstElevation;
					}
				}
				else
				{
					flag2 = false;
				}
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				if (_ApplySurfaceElevationPoints != null)
				{
					for (num = 0; num < _ApplySurfaceElevationPoints.Count; num++)
					{
						if (eps_Leg3.Arc.StartPnt.IsEqual(_ApplySurfaceElevationPoints[num]))
						{
							flag = true;
							eps_Leg3.Arc.StartPnt.z = _ApplySurfaceElevationPoints[num].z;
							if (flag2)
							{
								break;
							}
						}
						else if (eps_Leg3.Arc.EndPnt.IsEqual(_ApplySurfaceElevationPoints[num]))
						{
							flag2 = true;
							eps_Leg3.Arc.EndPnt.z = _ApplySurfaceElevationPoints[num].z;
							if (flag)
							{
								break;
							}
						}
					}
				}
				if (!flag)
				{
					if (_bUseToposurfaceElevation)
					{
						if (_SurfaceList != null && _SurfaceList.Count > 0)
						{
							RevitUtility.GetSurfaceContainingPoint(eps_Leg3.Arc.StartPnt, _SurfaceList, _CurPhase, ref _dElevation);
						}
						else
						{
							_dElevation = eps_Leg3.Arc.StartPnt.z;
						}
						eps_Leg3.Arc.StartPnt.z = _dElevation + _dConstElevation;
					}
					else if (_bUseConstantElevation)
					{
						eps_Leg3.Arc.StartPnt.z = _dConstElevation;
					}
				}
				else
				{
					flag = false;
				}
				if (!flag2)
				{
					if (_bUseToposurfaceElevation)
					{
						if (_SurfaceList != null && _SurfaceList.Count > 0)
						{
							RevitUtility.GetSurfaceContainingPoint(eps_Leg3.Arc.EndPnt, _SurfaceList, _CurPhase, ref _dElevation2);
						}
						else
						{
							_dElevation2 = eps_Leg3.Arc.EndPnt.z;
						}
						eps_Leg3.Arc.EndPnt.z = _dElevation2 + _dConstElevation;
					}
					else if (_bUseConstantElevation)
					{
						eps_Leg3.Arc.EndPnt.z = _dConstElevation;
					}
				}
				else
				{
					flag2 = false;
				}
				eps_Leg3.Arc.RadiusPnt.z = (eps_Leg3.Arc.StartPnt.z + eps_Leg3.Arc.EndPnt.z) / 2.0;
			}
			if (eps_Leg2 == null)
			{
				eps_Leg2 = SingleLeg(eps_Leg3);
			}
			else
			{
				eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
			}
		}
		return eps_Leg2;
	}

	public Eps_Leg ApplyElevationsToLeg(double _dStartingElevation, double _dGrade)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		double num = 0.0;
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			eps_Leg3 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line(eps_Leg.Line);
				if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg3.Line.StartPnt, this, ref _dStation, ref _dOffset) == 1700)
				{
					num = _dGrade * _dStation;
					eps_Leg3.Line.StartPnt.z = _dStartingElevation + num;
				}
				if (IsLegClosed() && eps_Leg3.Next == null)
				{
					num = _dGrade * RevitGeometry.eplGeoE_CalcLegListLength(this);
					eps_Leg3.Line.EndPnt.z = _dStartingElevation + num;
				}
				else if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg3.Line.EndPnt, this, ref _dStation, ref _dOffset) == 1700)
				{
					num = _dGrade * _dStation;
					eps_Leg3.Line.EndPnt.z = _dStartingElevation + num;
				}
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg3.Arc.StartPnt, this, ref _dStation, ref _dOffset) == 1700)
				{
					num = _dGrade * _dStation;
					eps_Leg3.Arc.StartPnt.z = _dStartingElevation + num;
				}
				if (IsLegClosed() && eps_Leg3.Next == null)
				{
					num = _dGrade * RevitGeometry.eplGeoE_CalcLegListLength(this);
					eps_Leg3.Arc.EndPnt.z = _dStartingElevation + num;
				}
				else if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg3.Arc.EndPnt, this, ref _dStation, ref _dOffset) == 1700)
				{
					num = _dGrade * _dStation;
					eps_Leg3.Arc.EndPnt.z = _dStartingElevation + num;
				}
				eps_Leg3.Arc.RadiusPnt.z = (eps_Leg3.Arc.StartPnt.z + eps_Leg3.Arc.EndPnt.z) / 2.0;
			}
			if (eps_Leg2 == null)
			{
				eps_Leg2 = SingleLeg(eps_Leg3);
			}
			else
			{
				eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
			}
		}
		return eps_Leg2;
	}

	public Eps_Leg TopographySurfaceLeg(List<TopographySurfaceContainer> _SurfaceList, Phase _CurPhase)
	{
		if (LegCount == 0)
		{
			return null;
		}
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Leg eps_Leg4 = null;
		Eps_Leg eps_Leg5 = null;
		double _dElevation = 0.0;
		double z = LegStartPnt.z;
		int num = -1;
		eps_Leg4 = SingleLeg(Last);
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			eps_Leg3 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line(eps_Leg.Line);
				if (RevitUtility.GetSurfaceContainingPoint(eps_Leg3.Line.StartPnt, _SurfaceList, _CurPhase, ref _dElevation) != null)
				{
					eps_Leg3.Line.StartPnt.z = _dElevation;
					z = _dElevation;
				}
				else
				{
					if (num == -1)
					{
						num = FindInnerOffsetLegSide();
					}
					eps_Leg5 = new Eps_Leg();
					eps_Leg5 = SingleLeg(eps_Leg4);
					eps_Leg5.Last.Next = SingleLeg(eps_Leg3);
					if (FindSurfaceElevationAtPoint(num, eps_Leg5, _SurfaceList, _CurPhase, ref _dElevation))
					{
						eps_Leg3.Line.StartPnt.z = _dElevation;
						z = _dElevation;
					}
					else
					{
						eps_Leg3.Line.StartPnt.z = z;
					}
				}
				if (RevitUtility.GetSurfaceContainingPoint(eps_Leg3.Line.EndPnt, _SurfaceList, _CurPhase, ref _dElevation) != null)
				{
					eps_Leg3.Line.EndPnt.z = _dElevation;
					z = _dElevation;
				}
				else
				{
					if (num == -1)
					{
						num = FindInnerOffsetLegSide();
					}
					eps_Leg5 = new Eps_Leg();
					eps_Leg5 = SingleLeg(eps_Leg3);
					if (eps_Leg.Next != null)
					{
						eps_Leg5.Last.Next = SingleLeg(eps_Leg.Next);
					}
					else
					{
						eps_Leg5.Last.Next = SingleLeg(this);
					}
					if (FindSurfaceElevationAtPoint(num, eps_Leg5, _SurfaceList, _CurPhase, ref _dElevation))
					{
						eps_Leg3.Line.EndPnt.z = _dElevation;
						z = _dElevation;
					}
					else
					{
						eps_Leg3.Line.EndPnt.z = z;
					}
				}
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				if (RevitUtility.GetSurfaceContainingPoint(eps_Leg3.Arc.StartPnt, _SurfaceList, _CurPhase, ref _dElevation) != null)
				{
					eps_Leg3.Arc.StartPnt.z = _dElevation;
					z = _dElevation;
				}
				else
				{
					if (num == -1)
					{
						num = FindInnerOffsetLegSide();
					}
					eps_Leg5 = new Eps_Leg();
					eps_Leg5 = SingleLeg(eps_Leg4);
					eps_Leg5.Last.Next = SingleLeg(eps_Leg3);
					if (FindSurfaceElevationAtPoint(num, eps_Leg5, _SurfaceList, _CurPhase, ref _dElevation))
					{
						eps_Leg3.Arc.StartPnt.z = _dElevation;
						z = _dElevation;
					}
					else
					{
						eps_Leg3.Arc.StartPnt.z = z;
					}
				}
				if (RevitUtility.GetSurfaceContainingPoint(eps_Leg3.Arc.EndPnt, _SurfaceList, _CurPhase, ref _dElevation) != null)
				{
					eps_Leg3.Arc.EndPnt.z = _dElevation;
					z = _dElevation;
				}
				else
				{
					if (num == -1)
					{
						num = FindInnerOffsetLegSide();
					}
					eps_Leg5 = new Eps_Leg();
					eps_Leg5 = SingleLeg(eps_Leg3);
					if (eps_Leg.Next != null)
					{
						eps_Leg5.Last.Next = SingleLeg(eps_Leg.Next);
					}
					else
					{
						eps_Leg5.Last.Next = SingleLeg(this);
					}
					if (FindSurfaceElevationAtPoint(num, eps_Leg5, _SurfaceList, _CurPhase, ref _dElevation))
					{
						eps_Leg3.Arc.EndPnt.z = _dElevation;
						z = _dElevation;
					}
					else
					{
						eps_Leg3.Arc.EndPnt.z = z;
					}
				}
				eps_Leg3.Arc.RadiusPnt.z = (eps_Leg3.Arc.StartPnt.z + eps_Leg3.Arc.EndPnt.z) / 2.0;
			}
			if (eps_Leg2 == null)
			{
				eps_Leg2 = SingleLeg(eps_Leg3);
			}
			else
			{
				eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
			}
			eps_Leg4 = SingleLeg(eps_Leg3);
		}
		return eps_Leg2;
	}

	private bool FindSurfaceElevationAtPoint(int nOffset, Eps_Leg LegSection, List<TopographySurfaceContainer> _SurfaceList, Phase _CurPhase, ref double dPointElevation)
	{
		Eps_Point findPoint = null;
		Eps_Leg eps_Leg = null;
		for (int i = 4; i <= 10; i++)
		{
			eps_Leg = new Eps_Leg();
			RevitGeometry.OffsetLegList(LegSection, (double)i * 0.01, nOffset, ref eps_Leg, TrimClosedObject: true);
			if (eps_Leg.Line != null)
			{
				findPoint = eps_Leg.Line.EndPnt;
			}
			else if (eps_Leg.Arc != null)
			{
				findPoint = eps_Leg.Arc.EndPnt;
			}
			if (RevitUtility.GetSurfaceContainingPoint(findPoint, _SurfaceList, _CurPhase, ref dPointElevation) != null)
			{
				return true;
			}
			LegSection = CopyLeg(eps_Leg);
		}
		return false;
	}

	private int FindInnerOffsetLegSide()
	{
		Eps_Leg ppOffsetLegList = new Eps_Leg();
		RevitGeometry.OffsetLegList(this, 0.01, 1, ref ppOffsetLegList, TrimClosedObject: true);
		Eps_Leg ppOffsetLegList2 = new Eps_Leg();
		RevitGeometry.OffsetLegList(this, 0.01, 2, ref ppOffsetLegList2, TrimClosedObject: true);
		if (RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2) < RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList))
		{
			return 2;
		}
		return 1;
	}

	public bool FindMinAndMaxElevationOfLeg(TopographySurfaceContainer _SurfaceContainer, double _dLineInterval, double _dArcInterval, ref double _dMinElevation, ref double _dMaxElevation)
	{
		List<TopographySurfaceContainer> list = new List<TopographySurfaceContainer>();
		list.Add(_SurfaceContainer);
		return FindMinAndMaxElevationOfLeg(list, null, _dLineInterval, _dArcInterval, ref _dMinElevation, ref _dMaxElevation);
	}

	public bool FindMinAndMaxElevationOfLeg(List<TopographySurfaceContainer> _SurfaceList, Phase _CurPhase, double _dLineInterval, double _dArcInterval, ref double _dMinElevation, ref double _dMaxElevation)
	{
		Eps_Point Pnt = new Eps_Point();
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double _dElevation = 0.0;
		double num4 = 0.0;
		bool result = false;
		Eps_Leg eps_Leg = null;
		_dMaxElevation = double.MinValue;
		_dMinElevation = double.MaxValue;
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			num3 = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg);
			if (eps_Leg.Line != null)
			{
				num4 = _dLineInterval;
			}
			else if (eps_Leg.Arc != null)
			{
				num4 = _dArcInterval;
			}
			result = true;
			num2 = num;
			for (; num < num3; num += num4)
			{
				RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(this, num, 0.0, ref Pnt);
				RevitUtility.GetSurfaceContainingPoint(Pnt, _SurfaceList, _CurPhase, ref _dElevation);
				Pnt.z = _dElevation;
				if (RevitDefines.GT(Pnt.z, _dMaxElevation))
				{
					_dMaxElevation = Pnt.z;
				}
				if (RevitDefines.LT(Pnt.z, _dMinElevation))
				{
					_dMinElevation = Pnt.z;
				}
			}
			if (RevitDefines.GT(num3, 0.0))
			{
				num = num2 + num3;
				RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(this, num, 0.0, ref Pnt);
				RevitUtility.GetSurfaceContainingPoint(Pnt, _SurfaceList, _CurPhase, ref _dElevation);
				Pnt.z = _dElevation;
				if (RevitDefines.GT(Pnt.z, _dMaxElevation))
				{
					_dMaxElevation = Pnt.z;
				}
				if (RevitDefines.LT(Pnt.z, _dMinElevation))
				{
					_dMinElevation = Pnt.z;
				}
			}
		}
		return result;
	}

	public Eps_Leg MoveLeg(double _dPolarDirection, double _dPolarDistance)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Point _NewPnt = new Eps_Point();
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			eps_Leg3 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line(eps_Leg.Line);
				EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(_dPolarDirection), _dPolarDistance, ref _NewPnt);
				eps_Leg3.Line.StartPnt = new Eps_Point(_NewPnt);
				EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(_dPolarDirection), _dPolarDistance, ref _NewPnt);
				eps_Leg3.Line.EndPnt = new Eps_Point(_NewPnt);
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Arc.StartPnt, EPShared.eplGeoE_NormalizeRadians(_dPolarDirection), _dPolarDistance, ref _NewPnt);
				eps_Leg3.Arc.StartPnt = new Eps_Point(_NewPnt);
				EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Arc.RadiusPnt, EPShared.eplGeoE_NormalizeRadians(_dPolarDirection), _dPolarDistance, ref _NewPnt);
				eps_Leg3.Arc.RadiusPnt = new Eps_Point(_NewPnt);
				EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Arc.EndPnt, EPShared.eplGeoE_NormalizeRadians(_dPolarDirection), _dPolarDistance, ref _NewPnt);
				eps_Leg3.Arc.EndPnt = new Eps_Point(_NewPnt);
			}
			if (eps_Leg2 == null)
			{
				eps_Leg2 = SingleLeg(eps_Leg3);
			}
			else
			{
				eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
			}
		}
		return eps_Leg2;
	}

	public Eps_Leg MoveLeg(double _dPolarDirection, double _dPolarDistance, double _dElevation)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Point _NewPnt = new Eps_Point();
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			eps_Leg3 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line(eps_Leg.Line);
				EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(_dPolarDirection), _dPolarDistance, ref _NewPnt);
				_NewPnt.z += _dElevation;
				eps_Leg3.Line.StartPnt = new Eps_Point(_NewPnt);
				EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(_dPolarDirection), _dPolarDistance, ref _NewPnt);
				_NewPnt.z += _dElevation;
				eps_Leg3.Line.EndPnt = new Eps_Point(_NewPnt);
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Arc.StartPnt, EPShared.eplGeoE_NormalizeRadians(_dPolarDirection), _dPolarDistance, ref _NewPnt);
				_NewPnt.z += _dElevation;
				eps_Leg3.Arc.StartPnt = new Eps_Point(_NewPnt);
				EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Arc.RadiusPnt, EPShared.eplGeoE_NormalizeRadians(_dPolarDirection), _dPolarDistance, ref _NewPnt);
				_NewPnt.z += _dElevation;
				eps_Leg3.Arc.RadiusPnt = new Eps_Point(_NewPnt);
				EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Arc.EndPnt, EPShared.eplGeoE_NormalizeRadians(_dPolarDirection), _dPolarDistance, ref _NewPnt);
				_NewPnt.z += _dElevation;
				eps_Leg3.Arc.EndPnt = new Eps_Point(_NewPnt);
			}
			if (eps_Leg2 == null)
			{
				eps_Leg2 = SingleLeg(eps_Leg3);
			}
			else
			{
				eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
			}
		}
		return eps_Leg2;
	}

	public Eps_Leg RotateLeg(Eps_Point _BasePoint, double _dAngle)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Point eps_Point = new Eps_Point();
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			eps_Leg3 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line(eps_Leg.Line);
				eps_Point = new Eps_Point(eps_Leg3.Line.StartPnt);
				EPShared.eplGeoE_RotatePoint(ref eps_Point, _BasePoint, EPShared.eplGeoE_NormalizeRadians(_dAngle));
				eps_Leg3.Line.StartPnt.x = eps_Point.x;
				eps_Leg3.Line.StartPnt.y = eps_Point.y;
				eps_Point = new Eps_Point(eps_Leg3.Line.EndPnt);
				EPShared.eplGeoE_RotatePoint(ref eps_Point, _BasePoint, EPShared.eplGeoE_NormalizeRadians(_dAngle));
				eps_Leg3.Line.EndPnt.x = eps_Point.x;
				eps_Leg3.Line.EndPnt.y = eps_Point.y;
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				eps_Point = new Eps_Point(eps_Leg3.Arc.StartPnt);
				EPShared.eplGeoE_RotatePoint(ref eps_Point, _BasePoint, EPShared.eplGeoE_NormalizeRadians(_dAngle));
				eps_Leg3.Arc.StartPnt.x = eps_Point.x;
				eps_Leg3.Arc.StartPnt.y = eps_Point.y;
				eps_Point = new Eps_Point(eps_Leg3.Arc.RadiusPnt);
				EPShared.eplGeoE_RotatePoint(ref eps_Point, _BasePoint, EPShared.eplGeoE_NormalizeRadians(_dAngle));
				eps_Leg3.Arc.RadiusPnt.x = eps_Point.x;
				eps_Leg3.Arc.RadiusPnt.y = eps_Point.y;
				eps_Point = new Eps_Point(eps_Leg3.Arc.EndPnt);
				EPShared.eplGeoE_RotatePoint(ref eps_Point, _BasePoint, EPShared.eplGeoE_NormalizeRadians(_dAngle));
				eps_Leg3.Arc.EndPnt.x = eps_Point.x;
				eps_Leg3.Arc.EndPnt.y = eps_Point.y;
			}
			if (eps_Leg2 == null)
			{
				eps_Leg2 = SingleLeg(eps_Leg3);
			}
			else
			{
				eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
			}
		}
		return eps_Leg2;
	}

	public Eps_Leg SplitArcsInLeg()
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		double num = 0.0;
		Eps_Point _StaOffPnt = new Eps_Point();
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			eps_Leg3 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line(eps_Leg.Line);
			}
			else if (eps_Leg.Arc != null)
			{
				if (RevitDefines.GE(eps_Leg.Arc.Delta, Math.PI))
				{
					num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg.Arc.Radius, eps_Leg.Arc.Delta);
					if (RevitDefines.EQ(eps_Leg.Arc.Delta, Math.PI * 2.0))
					{
						EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num / 3.0, 0.0, ref _StaOffPnt);
						Eps_Point _StaOffPnt2 = new Eps_Point();
						EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num / 3.0 * 2.0, 0.0, ref _StaOffPnt2);
						eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
						eps_Leg3.Arc.Delta = eps_Leg.Arc.Delta / 3.0;
						eps_Leg3.Arc.StartPnt = eps_Leg.Arc.StartPnt;
						eps_Leg3.Arc.EndPnt = _StaOffPnt;
						if (eps_Leg2 == null)
						{
							eps_Leg2 = SingleLeg(eps_Leg3);
						}
						else
						{
							eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
						}
						eps_Leg3 = new Eps_Leg();
						eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
						eps_Leg3.Arc.Delta = eps_Leg.Arc.Delta / 3.0;
						eps_Leg3.Arc.StartPnt = _StaOffPnt;
						eps_Leg3.Arc.EndPnt = _StaOffPnt2;
						eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
						eps_Leg3 = new Eps_Leg();
						eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
						eps_Leg3.Arc.Delta = eps_Leg.Arc.Delta / 3.0;
						eps_Leg3.Arc.StartPnt = _StaOffPnt2;
						eps_Leg3.Arc.EndPnt = eps_Leg.Arc.EndPnt;
					}
					else
					{
						EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num / 2.0, 0.0, ref _StaOffPnt);
						eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
						eps_Leg3.Arc.Delta = eps_Leg.Arc.Delta / 2.0;
						eps_Leg3.Arc.StartPnt = eps_Leg.Arc.StartPnt;
						eps_Leg3.Arc.EndPnt = _StaOffPnt;
						if (eps_Leg2 == null)
						{
							eps_Leg2 = SingleLeg(eps_Leg3);
						}
						else
						{
							eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
						}
						eps_Leg3 = new Eps_Leg();
						eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
						eps_Leg3.Arc.Delta = eps_Leg.Arc.Delta / 2.0;
						eps_Leg3.Arc.StartPnt = _StaOffPnt;
						eps_Leg3.Arc.EndPnt = eps_Leg.Arc.EndPnt;
					}
				}
				else
				{
					eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				}
			}
			if (eps_Leg2 == null)
			{
				eps_Leg2 = SingleLeg(eps_Leg3);
			}
			else
			{
				eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
			}
		}
		return eps_Leg2;
	}

	public List<Eps_Point> GetPointsAtIntervals(double dLinearInterval, double dRadialInterval)
	{
		List<Eps_Point> list = new List<Eps_Point>();
		Eps_Leg eps_Leg = null;
		double num = 0.0;
		double num2 = 0.0;
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			num = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg);
			if (eps_Leg.Arc != null)
			{
				Eps_Point eps_Point = null;
				double num3;
				for (num3 = num2 + num; num2 <= num3; num2 += dRadialInterval)
				{
					eps_Point = new Eps_Point();
					RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(this, num2, 0.0, ref eps_Point);
					if (list.Count == 0 || !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Point, list[list.Count - 1]))
					{
						list.Add(eps_Point);
					}
				}
				eps_Point = new Eps_Point();
				double num4 = num2 - num3;
				num2 -= num4;
				RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(this, num2, 0.0, ref eps_Point);
				if (list.Count == 0 || !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Point, list[list.Count - 1]))
				{
					list.Add(eps_Point);
				}
			}
			else
			{
				Eps_Point Pnt = null;
				double num5;
				for (num5 = num2 + num; num2 <= num5; num2 += dLinearInterval)
				{
					Pnt = new Eps_Point();
					RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(this, num2, 0.0, ref Pnt);
					if (list.Count == 0 || !EPShared.eplGeoE_ArePntsDuplicates2D(Pnt, list[list.Count - 1]))
					{
						list.Add(Pnt);
					}
				}
				double num6 = num2 - num5;
				num2 -= num6;
				RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(this, num2, 0.0, ref Pnt);
				if (list.Count == 0 || !EPShared.eplGeoE_ArePntsDuplicates2D(Pnt, list[list.Count - 1]))
				{
					list.Add(Pnt);
				}
			}
			if (eps_Leg.Next == null)
			{
				Eps_Point Pnt2 = new Eps_Point();
				RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(this, RevitGeometry.eplGeoE_CalcLegListLength(this), 0.0, ref Pnt2);
				if (list.Count == 0 || (!EPShared.eplGeoE_ArePntsDuplicates2D(Pnt2, list[list.Count - 1]) && (!IsLegClosed() || !EPShared.eplGeoE_ArePntsDuplicates2D(Pnt2, list[0]))))
				{
					list.Add(Pnt2);
				}
			}
		}
		return list;
	}

	public Eps_Leg SplitLegAtStations(List<double> _Stations)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		int i = 0;
		Eps_Point _StaOffPnt = new Eps_Point();
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			num = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg);
			eps_Leg3 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line();
				eps_Leg3.Line.StartPnt = eps_Leg.Line.StartPnt;
				for (; i < _Stations.Count && _Stations[i] < num3 + num; i++)
				{
					EPShared.eplGeoE_FindStaOffPointAlongLine(eps_Leg.Line, _Stations[i] - num3, 0.0, ref _StaOffPnt);
					if (!eps_Leg3.Line.StartPnt.IsEqual(_StaOffPnt))
					{
						eps_Leg3.Line.EndPnt = _StaOffPnt;
						if (eps_Leg2 == null)
						{
							eps_Leg2 = SingleLeg(eps_Leg3);
						}
						else
						{
							eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
						}
						eps_Leg3 = new Eps_Leg();
						eps_Leg3.Line = new Eps_Line();
						eps_Leg3.Line.StartPnt = _StaOffPnt;
					}
				}
				if (!eps_Leg3.Line.StartPnt.IsEqual(eps_Leg.Line.EndPnt))
				{
					eps_Leg3.Line.EndPnt = eps_Leg.Line.EndPnt;
				}
				else
				{
					eps_Leg3 = null;
				}
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				eps_Leg3.Arc.StartPnt = eps_Leg.Arc.StartPnt;
				num2 = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg3);
				for (; i < _Stations.Count && _Stations[i] < num3 + num; i++)
				{
					EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, _Stations[i] - num3, 0.0, ref _StaOffPnt);
					if (!eps_Leg3.Arc.StartPnt.IsEqual(_StaOffPnt))
					{
						eps_Leg3.Arc.Delta = (_Stations[i] - num3) / eps_Leg3.Arc.Radius;
						eps_Leg3.Arc.EndPnt = _StaOffPnt;
						if (eps_Leg2 == null)
						{
							eps_Leg2 = SingleLeg(eps_Leg3);
						}
						else
						{
							eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
						}
						eps_Leg3 = new Eps_Leg();
						eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
						eps_Leg3.Arc.StartPnt = _StaOffPnt;
						eps_Leg3.Arc.Delta = (num - (_Stations[i] - num3)) / eps_Leg3.Arc.Radius;
					}
					num2 = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg3);
				}
				if (!eps_Leg3.Arc.StartPnt.IsEqual(eps_Leg.Arc.EndPnt) || RevitDefines.GT(num2, 0.0))
				{
					eps_Leg3.Arc.EndPnt = eps_Leg.Arc.EndPnt;
				}
				else
				{
					eps_Leg3 = null;
				}
			}
			num3 += num;
			if (eps_Leg3 != null)
			{
				if (eps_Leg2 == null)
				{
					eps_Leg2 = SingleLeg(eps_Leg3);
				}
				else
				{
					eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
				}
			}
		}
		return eps_Leg2;
	}

	public Eps_Leg InsertLegAtStations(double _dStation, Eps_Leg _AddLeg, bool _bAppendOnEnd)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		double num = 0.0;
		double num2 = 0.0;
		bool flag = false;
		if (!_bAppendOnEnd)
		{
			if (eps_Leg2 == null)
			{
				eps_Leg2 = CopyLeg(_AddLeg);
			}
			else
			{
				eps_Leg2.Last.Next = CopyLeg(_AddLeg);
			}
		}
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			num = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg);
			eps_Leg3 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line();
				if (_dStation < num2 + num)
				{
					flag = true;
					if (!_bAppendOnEnd)
					{
						eps_Leg3.Line.StartPnt = _AddLeg.LegEndPnt;
						eps_Leg3.Line.EndPnt = eps_Leg.Line.EndPnt;
					}
					else
					{
						eps_Leg3.Line.StartPnt = eps_Leg.Line.StartPnt;
						eps_Leg3.Line.EndPnt = _AddLeg.LegStartPnt;
					}
					if (eps_Leg3.Line.StartPnt.IsEqual(eps_Leg3.Line.EndPnt))
					{
						eps_Leg3 = null;
					}
				}
				else if (!eps_Leg.Line.StartPnt.IsEqual(eps_Leg.Line.EndPnt))
				{
					eps_Leg3.Line.StartPnt = eps_Leg.Line.StartPnt;
					eps_Leg3.Line.EndPnt = eps_Leg.Line.EndPnt;
				}
				else
				{
					eps_Leg3 = null;
				}
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				if (_dStation < num2 + num)
				{
					flag = true;
					eps_Leg3.Arc.Delta = (_dStation - num2) / eps_Leg3.Arc.Radius;
					if (!_bAppendOnEnd)
					{
						eps_Leg3.Arc.StartPnt = _AddLeg.LegEndPnt;
						eps_Leg3.Arc.EndPnt = eps_Leg.Arc.EndPnt;
					}
					else
					{
						eps_Leg3.Arc.StartPnt = eps_Leg.Arc.StartPnt;
						eps_Leg3.Arc.EndPnt = _AddLeg.LegStartPnt;
					}
					eps_Leg3.Arc.RadiusPnt.z = (eps_Leg3.Arc.StartPnt.z + eps_Leg3.Arc.EndPnt.z) / 2.0;
					if (eps_Leg3.Arc.StartPnt.IsEqual(eps_Leg3.Arc.EndPnt))
					{
						eps_Leg3 = null;
					}
				}
				else if (!eps_Leg.Arc.StartPnt.IsEqual(eps_Leg.Arc.EndPnt))
				{
					eps_Leg3.Arc.StartPnt = eps_Leg.Arc.StartPnt;
					eps_Leg3.Arc.EndPnt = eps_Leg.Arc.EndPnt;
				}
				else
				{
					eps_Leg3 = null;
				}
			}
			num2 += num;
			if (_bAppendOnEnd || flag)
			{
				if (eps_Leg3 != null)
				{
					if (eps_Leg2 == null)
					{
						eps_Leg2 = SingleLeg(eps_Leg3);
					}
					else
					{
						eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
					}
				}
				if (_bAppendOnEnd && flag)
				{
					break;
				}
			}
		}
		if (_bAppendOnEnd)
		{
			if (eps_Leg2 == null)
			{
				eps_Leg2 = CopyLeg(_AddLeg);
			}
			else
			{
				eps_Leg2.Last.Next = CopyLeg(_AddLeg);
			}
		}
		return eps_Leg2;
	}

	public Eps_Leg ExtendLegList(bool _bStart, double _dDistance)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Point eps_Point = null;
		double _dDirection = 0.0;
		double num = _dDistance;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		bool flag = true;
		for (eps_Leg = this; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			eps_Leg3 = new Eps_Leg();
			num2 = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg);
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line(eps_Leg.Line);
				num3 = eps_Leg.Line.EndPnt.z - eps_Leg.Line.StartPnt.z;
				num4 = num3 / num2;
				num3 = num4 * (0.0 - _dDistance);
				if (_bStart && flag && !IsLegClosed())
				{
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg3.Line.EndPnt, eps_Leg3.Line.StartPnt, ref _dDirection);
					if (num < 0.0)
					{
						_dDirection = EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI);
						num = Math.Abs(num);
					}
					eps_Point = new Eps_Point();
					EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Line.StartPnt, _dDirection, num, ref eps_Point);
					eps_Leg3.Line.StartPnt.x = eps_Point.x;
					eps_Leg3.Line.StartPnt.y = eps_Point.y;
					eps_Leg3.Line.StartPnt.z += num3;
				}
				else if (!_bStart && eps_Leg.Next == null && !IsLegClosed())
				{
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg3.Line.StartPnt, eps_Leg3.Line.EndPnt, ref _dDirection);
					if (num < 0.0)
					{
						_dDirection = EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI);
						num = Math.Abs(num);
					}
					eps_Point = new Eps_Point();
					EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Line.EndPnt, _dDirection, num, ref eps_Point);
					eps_Leg3.Line.EndPnt.x = eps_Point.x;
					eps_Leg3.Line.EndPnt.y = eps_Point.y;
					eps_Leg3.Line.EndPnt.z += num3;
				}
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				num3 = eps_Leg.Arc.EndPnt.z - eps_Leg.Arc.StartPnt.z;
				num4 = num3 / num2;
				num3 = num4 * (0.0 - _dDistance);
				if (_bStart && flag && !IsLegClosed())
				{
					num2 += num;
					eps_Leg3.Arc.Delta = num2 / eps_Leg3.Arc.Radius;
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg3.Arc.RadiusPnt, eps_Leg3.Arc.EndPnt, ref _dDirection);
					_dDirection = ((eps_Leg3.Arc.Direction != -1) ? (_dDirection - eps_Leg3.Arc.Delta) : (_dDirection + eps_Leg3.Arc.Delta));
					eps_Point = new Eps_Point();
					EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Arc.RadiusPnt, _dDirection, eps_Leg3.Arc.Radius, ref eps_Point);
					eps_Leg3.Arc.StartPnt.x = eps_Point.x;
					eps_Leg3.Arc.StartPnt.y = eps_Point.y;
					eps_Leg3.Arc.StartPnt.z += num3;
				}
				else if (!_bStart && eps_Leg.Next == null && !IsLegClosed())
				{
					num2 += _dDirection;
					eps_Leg3.Arc.Delta = num2 / eps_Leg3.Arc.Radius;
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg3.Arc.RadiusPnt, eps_Leg3.Arc.StartPnt, ref _dDirection);
					_dDirection = ((eps_Leg3.Arc.Direction != -1) ? (_dDirection + eps_Leg3.Arc.Delta) : (_dDirection - eps_Leg3.Arc.Delta));
					eps_Point = new Eps_Point();
					EPShared.eplGeoE_CalcPolarToXY(eps_Leg3.Arc.RadiusPnt, _dDirection, eps_Leg3.Arc.Radius, ref eps_Point);
					eps_Leg3.Arc.EndPnt.x = eps_Point.x;
					eps_Leg3.Arc.EndPnt.y = eps_Point.y;
					eps_Leg3.Arc.EndPnt.z += num3;
				}
			}
			if (eps_Leg2 == null)
			{
				eps_Leg2 = SingleLeg(eps_Leg3);
			}
			else
			{
				eps_Leg2.Last.Next = SingleLeg(eps_Leg3);
			}
			flag = false;
		}
		return eps_Leg2;
	}
}
