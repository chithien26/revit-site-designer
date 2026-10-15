using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using SITES.Utilities;

namespace EPCommon;

public class FamilyInstanceComponentWrapper
{
	private string m_strChainID;

	private List<FamilyInstanceComponentContainer> m_FamilyContainerList;

	private ElementSet m_ProposedElementSet;

	private List<Element> m_ParkingLotBreaklineElementList;

	private List<FamilyInstance> m_ParkingLotBreaklineFamilyInstanceList;

	private Eps_Leg m_ProposedLeg;

	private Eps_Leg m_OriginalLeg;

	private int m_nDrawOrder;

	private FamilySymbol m_ProposedFamilySymbol;

	private FamilySymbol m_IntersectionFamilySymbol;

	private FamilySymbol m_OriginalFamilySymbol;

	private List<FamilyInstance> m_IntersectionFamilyInstanceList;

	private TopographySurfaceContainer m_ParkingLotTopographySurface;

	private List<Eps_Leg> m_ParkingLotBreaklineList;

	private FamilyInstance m_PrimarySiteComponentMass;

	private FamilyInstance m_AdditionalSiteComponentMass;

	private DirectShape m_PrimarySiteComponentDirectShape;

	private DirectShape m_AdditionalSiteComponentDirectShape;

	private bool m_bFormDepthApplied;

	private bool m_bRedrawForm;

	private Category m_MassLineSubCategory;

	private LandcaddID m_CurrentLandcaddId;

	private List<double> m_ParkingLotBreaklineLinearPointIntervals;

	private List<IntersectionStationContainer> m_LeftIntersectionStations;

	private List<IntersectionStationContainer> m_RightIntersectionStations;

	private List<IntersectionStationContainer> m_CenterIntersectionStations;

	private List<IntersectionContainer> m_IntersectionContainerList;

	public string ChainID => m_strChainID;

	public List<FamilyInstanceComponentContainer> FamilyContainerList => m_FamilyContainerList;

	public Eps_Leg ProposedLeg
	{
		get
		{
			return m_ProposedLeg;
		}
		set
		{
			m_ProposedLeg = value;
		}
	}

	public Eps_Leg InstanceLeg
	{
		get
		{
			if (m_ProposedLeg != null)
			{
				return m_ProposedLeg;
			}
			if (m_OriginalLeg != null)
			{
				return m_OriginalLeg;
			}
			return null;
		}
		set
		{
			m_ProposedLeg = value;
		}
	}

	public ElementSet ProposedElementSet
	{
		get
		{
			return m_ProposedElementSet;
		}
		set
		{
			m_ProposedElementSet = value;
		}
	}

	public Eps_Leg OriginalLeg => m_OriginalLeg;

	public int DrawOrder
	{
		get
		{
			return m_nDrawOrder;
		}
		set
		{
			m_nDrawOrder = value;
		}
	}

	public FamilySymbol ProposedFamilySymbol
	{
		get
		{
			return m_ProposedFamilySymbol;
		}
		set
		{
			m_ProposedFamilySymbol = value;
			if (m_OriginalFamilySymbol == null)
			{
				m_OriginalFamilySymbol = value;
			}
			ApplyFormDepthToLegs();
		}
	}

	public FamilySymbol OriginalFamilySymbol => m_OriginalFamilySymbol;

	public FamilySymbol IntersectionFamilySymbol
	{
		get
		{
			return m_IntersectionFamilySymbol;
		}
		set
		{
			m_IntersectionFamilySymbol = value;
		}
	}

	public List<FamilyInstance> IntersectionFamilyInstanceList
	{
		get
		{
			return m_IntersectionFamilyInstanceList;
		}
		set
		{
			m_IntersectionFamilyInstanceList = value;
		}
	}

	public TopographySurfaceContainer ParkingLotTopographySurface
	{
		get
		{
			return m_ParkingLotTopographySurface;
		}
		set
		{
			m_ParkingLotTopographySurface = value;
		}
	}

	public List<Eps_Leg> ParkingLotBreaklineList
	{
		get
		{
			return m_ParkingLotBreaklineList;
		}
		set
		{
			m_ParkingLotBreaklineList = value;
		}
	}

	public List<Element> ParkingLotBreaklineElementList
	{
		get
		{
			return m_ParkingLotBreaklineElementList;
		}
		set
		{
			m_ParkingLotBreaklineElementList = value;
		}
	}

	public ElementSet ParkingLotBreaklineElementSet
	{
		get
		{
			ElementSet elementSet = new ElementSet();
			for (int num = m_ParkingLotBreaklineElementList.Count - 1; num >= 0; num--)
			{
				elementSet.Insert(m_ParkingLotBreaklineElementList[num]);
			}
			return elementSet;
		}
	}

	public List<FamilyInstance> ParkingLotBreaklineFamilyInstanceList
	{
		get
		{
			return m_ParkingLotBreaklineFamilyInstanceList;
		}
		set
		{
			m_ParkingLotBreaklineFamilyInstanceList = value;
		}
	}

	public FamilyInstance PrimarySiteComponentMass
	{
		get
		{
			return m_PrimarySiteComponentMass;
		}
		set
		{
			m_PrimarySiteComponentMass = value;
		}
	}

	public FamilyInstance AdditionalSiteComponentMass
	{
		get
		{
			return m_AdditionalSiteComponentMass;
		}
		set
		{
			m_AdditionalSiteComponentMass = value;
		}
	}

	public DirectShape PrimarySiteComponentDirectShape
	{
		get
		{
			return m_PrimarySiteComponentDirectShape;
		}
		set
		{
			m_PrimarySiteComponentDirectShape = value;
		}
	}

	public DirectShape AdditionalSiteComponentDirectShape
	{
		get
		{
			return m_AdditionalSiteComponentDirectShape;
		}
		set
		{
			m_AdditionalSiteComponentDirectShape = value;
		}
	}

	public LandcaddID LandcaddId => m_CurrentLandcaddId;

	public List<double> ParkingLotBreaklineLinearPointIntervals
	{
		get
		{
			return m_ParkingLotBreaklineLinearPointIntervals;
		}
		set
		{
			m_ParkingLotBreaklineLinearPointIntervals = value;
		}
	}

	public List<IntersectionStationContainer> LeftIntersectionStations
	{
		get
		{
			return m_LeftIntersectionStations;
		}
		set
		{
			m_LeftIntersectionStations = value;
		}
	}

	public List<IntersectionStationContainer> RightIntersectionStations
	{
		get
		{
			return m_RightIntersectionStations;
		}
		set
		{
			m_RightIntersectionStations = value;
		}
	}

	public List<IntersectionStationContainer> CenterIntersectionStations
	{
		get
		{
			return m_CenterIntersectionStations;
		}
		set
		{
			m_CenterIntersectionStations = value;
		}
	}

	public List<IntersectionContainer> IntersectionContainerList
	{
		get
		{
			return m_IntersectionContainerList;
		}
		set
		{
			m_IntersectionContainerList = value;
		}
	}

	public bool RedrawForm
	{
		get
		{
			return m_bRedrawForm;
		}
		set
		{
			m_bRedrawForm = value;
		}
	}

	public FamilyInstanceComponentWrapper(string _strChainID, List<FamilyInstanceComponentContainer> _FamilyContainerList)
	{
		m_strChainID = _strChainID;
		m_FamilyContainerList = _FamilyContainerList;
		m_ProposedElementSet = null;
		m_ProposedLeg = null;
		m_OriginalLeg = null;
		m_nDrawOrder = 0;
		m_ProposedFamilySymbol = null;
		m_OriginalFamilySymbol = null;
		m_IntersectionFamilySymbol = null;
		m_IntersectionFamilyInstanceList = new List<FamilyInstance>();
		m_ParkingLotTopographySurface = null;
		m_ParkingLotBreaklineList = new List<Eps_Leg>();
		m_ParkingLotBreaklineElementList = new List<Element>();
		m_ParkingLotBreaklineFamilyInstanceList = new List<FamilyInstance>();
		m_PrimarySiteComponentMass = null;
		m_AdditionalSiteComponentMass = null;
		m_PrimarySiteComponentDirectShape = null;
		m_AdditionalSiteComponentDirectShape = null;
		m_bFormDepthApplied = false;
		m_bRedrawForm = false;
		m_MassLineSubCategory = null;
		m_ParkingLotBreaklineLinearPointIntervals = new List<double>();
		m_LeftIntersectionStations = new List<IntersectionStationContainer>();
		m_RightIntersectionStations = new List<IntersectionStationContainer>();
		m_CenterIntersectionStations = new List<IntersectionStationContainer>();
		m_IntersectionContainerList = new List<IntersectionContainer>();
		SetCurrentLandcaddId();
	}

	public FamilyInstanceComponentWrapper(string _strChainID, List<FamilyInstanceComponentContainer> _FamilyContainerList, int _nDrawOrder)
	{
		m_strChainID = _strChainID;
		m_FamilyContainerList = _FamilyContainerList;
		m_ProposedElementSet = null;
		m_ProposedLeg = null;
		m_OriginalLeg = null;
		m_nDrawOrder = _nDrawOrder;
		m_ProposedFamilySymbol = null;
		m_OriginalFamilySymbol = null;
		m_IntersectionFamilySymbol = null;
		m_IntersectionFamilyInstanceList = new List<FamilyInstance>();
		m_ParkingLotTopographySurface = null;
		m_ParkingLotBreaklineList = new List<Eps_Leg>();
		m_ParkingLotBreaklineElementList = new List<Element>();
		m_ParkingLotBreaklineFamilyInstanceList = new List<FamilyInstance>();
		m_PrimarySiteComponentMass = null;
		m_AdditionalSiteComponentMass = null;
		m_PrimarySiteComponentDirectShape = null;
		m_AdditionalSiteComponentDirectShape = null;
		m_bFormDepthApplied = false;
		m_bRedrawForm = false;
		m_MassLineSubCategory = null;
		m_ParkingLotBreaklineLinearPointIntervals = new List<double>();
		m_LeftIntersectionStations = new List<IntersectionStationContainer>();
		m_RightIntersectionStations = new List<IntersectionStationContainer>();
		m_CenterIntersectionStations = new List<IntersectionStationContainer>();
		m_IntersectionContainerList = new List<IntersectionContainer>();
		SetCurrentLandcaddId();
	}

	public void CreateOriginalLeg(TopographySurfaceContainer _Surface)
	{
		List<TopographySurfaceContainer> list = new List<TopographySurfaceContainer>();
		list.Add(_Surface);
		CreateOriginalOrProposedLeg(list, null, _bIsOriginalLeg: true);
	}

	public void CreateOriginalLeg(List<TopographySurfaceContainer> _SurfaceList)
	{
		CreateOriginalOrProposedLeg(_SurfaceList, null, _bIsOriginalLeg: true);
	}

	public void CreateOriginalLeg(List<TopographySurfaceContainer> _SurfaceList, Phase _CurPhase)
	{
		CreateOriginalOrProposedLeg(_SurfaceList, _CurPhase, _bIsOriginalLeg: true);
	}

	public void CreateOriginalOrProposedLeg(List<TopographySurfaceContainer> _SurfaceList, Phase _CurPhase, bool _bIsOriginalLeg)
	{
		List<Eps_Line> list = new List<Eps_Line>();
		List<Eps_Arc> list2 = new List<Eps_Arc>();
		Eps_Leg _CurrentLeg = null;
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Element element = null;
		FamilyInstance familyInstance = null;
		Parameter parameter = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		bool flag4 = false;
		bool flag5 = false;
		double num = 0.0;
		double num2 = 0.0;
		for (int i = 0; i < m_FamilyContainerList.Count; i++)
		{
			element = m_FamilyContainerList[i].Element;
			if (element != null)
			{
				if (element.Location is LocationCurve { Curve: var curve })
				{
					if (curve != null)
					{
						if (!flag2 && m_FamilyContainerList[i].FamilyInstance != null)
						{
							parameter = m_FamilyContainerList[i].FamilyInstance.LookupParameter(RevitParameters.IsFirstPlacementSegmentGenericFamilyParameter);
							if (parameter != null && parameter.AsInteger() == 1)
							{
								parameter = m_FamilyContainerList[i].FamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
								if (parameter != null && string.Compare(parameter.AsString(), element.UniqueId, ignoreCase: true) == 0)
								{
									flag = true;
									flag2 = true;
								}
							}
						}
						if (flag3)
						{
							flag4 = false;
							flag5 = false;
							familyInstance = m_FamilyContainerList[i].FamilyInstance;
							if (familyInstance != null)
							{
								parameter = familyInstance.LookupParameter(RevitParameters.UsePlacementElevationsFamilyParameter);
								if (parameter != null && parameter.AsInteger() == 1)
								{
									parameter = familyInstance.LookupParameter(RevitParameters.BeginningElevationFamilyParameter);
									if (parameter != null)
									{
										num = parameter.AsDouble();
										flag4 = true;
									}
									parameter = familyInstance.LookupParameter(RevitParameters.EndingElevationFamilyParameter);
									if (parameter != null)
									{
										num2 = parameter.AsDouble();
										flag5 = true;
									}
								}
							}
							if (!flag4 || !flag5)
							{
								flag3 = false;
							}
						}
						if (curve is Arc)
						{
							Arc arc = (Arc)curve;
							Eps_Arc eps_Arc = new Eps_Arc(arc);
							if (familyInstance != null)
							{
								eps_Arc.StartPnt.z = num;
								eps_Arc.EndPnt.z = num2;
								eps_Arc.RadiusPnt.z = (eps_Arc.StartPnt.z + eps_Arc.EndPnt.z) / 2.0;
							}
							if (flag)
							{
								Eps_Leg.InsertArcLegSegment(ref _CurrentLeg, eps_Arc);
							}
							else
							{
								list2.Add(eps_Arc);
							}
						}
						else if (curve is Line)
						{
							Line line = (Line)curve;
							Eps_Line eps_Line = new Eps_Line(line);
							if (familyInstance != null)
							{
								eps_Line.StartPnt.z = num;
								eps_Line.EndPnt.z = num2;
							}
							if (flag)
							{
								Eps_Leg.InsertLineLegSegment(ref _CurrentLeg, eps_Line);
							}
							else
							{
								list.Add(eps_Line);
							}
						}
						else if (curve is Ellipse || curve is NurbSpline || curve is HermiteSpline)
						{
							eps_Leg = RevitGeometry.GetLegFromSpline(element, curve, _bReverseOrder: false);
							double _dDistance = 0.0;
							double num3 = 0.0;
							double num4 = 0.0;
							double num5 = 0.0;
							if (familyInstance != null)
							{
								num4 = num2 - num;
								_dDistance = RevitGeometry.eplGeoE_CalcLegListLength(eps_Leg);
								num5 = num4 / _dDistance;
								num3 = num;
							}
							for (eps_Leg2 = eps_Leg; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
							{
								if (eps_Leg2.Line != null)
								{
									Eps_Line eps_Line2 = new Eps_Line(eps_Leg2.Line);
									if (familyInstance != null)
									{
										EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Line2.StartPnt, eps_Line2.EndPnt, ref _dDistance);
										num4 = num5 * _dDistance;
										eps_Line2.StartPnt.z = num3;
										num3 += num4;
										eps_Line2.EndPnt.z = num3;
									}
									if (flag)
									{
										Eps_Leg.InsertLineLegSegment(ref _CurrentLeg, eps_Line2);
									}
									else
									{
										list.Add(new Eps_Line(eps_Line2));
									}
								}
								else if (eps_Leg2.Arc != null)
								{
									Eps_Arc eps_Arc2 = new Eps_Arc(eps_Leg2.Arc);
									if (familyInstance != null)
									{
										_dDistance = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Arc2.Radius, eps_Arc2.Delta);
										num4 = num5 * _dDistance;
										eps_Arc2.StartPnt.z = num3;
										num3 += num4;
										eps_Arc2.EndPnt.z = num3;
										eps_Arc2.RadiusPnt.z = (eps_Arc2.StartPnt.z + eps_Arc2.EndPnt.z) / 2.0;
									}
									if (flag)
									{
										Eps_Leg.InsertArcLegSegment(ref _CurrentLeg, eps_Arc2);
									}
									else
									{
										list2.Add(new Eps_Arc(eps_Arc2));
									}
								}
							}
						}
						else
						{
							flag3 = false;
						}
						flag = false;
					}
					else
					{
						flag3 = false;
					}
				}
				else
				{
					flag3 = false;
				}
			}
			else
			{
				flag3 = false;
			}
		}
		int num6 = -1;
		int num7 = -1;
		double num8 = double.MaxValue;
		double num9 = double.MaxValue;
		bool flag6 = false;
		while (list.Count > 0 || list2.Count > 0)
		{
			for (int num10 = list.Count - 1; num10 >= 0; num10--)
			{
				Eps_Line currentLine = list[num10];
				if (Eps_Leg.InsertLineLegSegment(ref _CurrentLeg, currentLine, flag2))
				{
					list.RemoveAt(num10);
					flag6 = true;
				}
			}
			for (int num11 = list2.Count - 1; num11 >= 0; num11--)
			{
				Eps_Arc currentArc = list2[num11];
				if (Eps_Leg.InsertArcLegSegment(ref _CurrentLeg, currentArc, flag2))
				{
					list2.RemoveAt(num11);
					flag6 = true;
				}
			}
			if (!flag6)
			{
				for (int num12 = list.Count - 1; num12 >= 0; num12--)
				{
					eps_Leg = new Eps_Leg();
					eps_Leg.Line = list[num12];
					num8 = Eps_Leg.FindMinimumDistanceBetweenLegs(_CurrentLeg, eps_Leg);
					if (num8 < num9)
					{
						num9 = num8;
						num6 = num12;
					}
				}
				for (int num13 = list2.Count - 1; num13 >= 0; num13--)
				{
					eps_Leg = new Eps_Leg();
					eps_Leg.Arc = list2[num13];
					num8 = Eps_Leg.FindMinimumDistanceBetweenLegs(_CurrentLeg, eps_Leg);
					if (num8 < num9)
					{
						num9 = num8;
						num7 = num13;
						num6 = -1;
					}
				}
				if (num6 >= 0)
				{
					eps_Leg = new Eps_Leg();
					eps_Leg.Line = list[num6];
					Eps_Leg.CombineLegListsAtShortestGaps(ref _CurrentLeg, eps_Leg);
					list.RemoveAt(num6);
					num6 = -1;
				}
				else if (num7 >= 0)
				{
					eps_Leg = new Eps_Leg();
					eps_Leg.Arc = list2[num7];
					Eps_Leg.CombineLegListsAtShortestGaps(ref _CurrentLeg, eps_Leg);
					list2.RemoveAt(num7);
					num7 = -1;
				}
				num8 = double.MaxValue;
				num9 = double.MaxValue;
			}
			else
			{
				flag6 = false;
			}
		}
		if (_CurrentLeg == null)
		{
			return;
		}
		m_bFormDepthApplied = flag3;
		if (_bIsOriginalLeg)
		{
			if (!flag3)
			{
				m_OriginalLeg = _CurrentLeg.TopographySurfaceLeg(_SurfaceList, _CurPhase);
			}
			else
			{
				m_OriginalLeg = Eps_Leg.CopyLeg(_CurrentLeg);
			}
		}
		else if (!flag3)
		{
			m_ProposedLeg = _CurrentLeg.TopographySurfaceLeg(_SurfaceList, _CurPhase);
		}
		else
		{
			m_ProposedLeg = Eps_Leg.CopyLeg(_CurrentLeg);
		}
	}

	public void CreateProposedLeg(TopographySurfaceContainer _Surface)
	{
		List<TopographySurfaceContainer> list = new List<TopographySurfaceContainer>();
		list.Add(_Surface);
		CreateOriginalOrProposedLeg(list, null, _bIsOriginalLeg: false);
	}

	public void CreateProposedLeg(List<TopographySurfaceContainer> _SurfaceList)
	{
		CreateOriginalOrProposedLeg(_SurfaceList, null, _bIsOriginalLeg: false);
	}

	public void CreateProposedLeg(List<TopographySurfaceContainer> _SurfaceList, Phase _CurPhase)
	{
		CreateOriginalOrProposedLeg(_SurfaceList, _CurPhase, _bIsOriginalLeg: false);
	}

	private void SetCurrentLandcaddId()
	{
		Parameter parameter = null;
		FamilyInstance firstExistingFamilyInstance = GetFirstExistingFamilyInstance();
		if (firstExistingFamilyInstance == null)
		{
			return;
		}
		m_CurrentLandcaddId = LandcaddID.SoftTerrain;
		parameter = firstExistingFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
		if (parameter == null)
		{
			return;
		}
		foreach (int value in Enum.GetValues(typeof(LandcaddID)))
		{
			if (string.Compare(parameter.AsString(), value.ToString("000"), ignoreCase: true) == 0)
			{
				m_CurrentLandcaddId = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value);
				break;
			}
		}
	}

	private void ApplyFormDepthToLegs()
	{
		Parameter parameter = null;
		if (m_bFormDepthApplied || m_ProposedFamilySymbol == null || (m_ProposedLeg == null && m_OriginalLeg == null))
		{
			return;
		}
		double num = 0.0;
		parameter = null;
		switch (m_CurrentLandcaddId)
		{
		case LandcaddID.Sidewalk:
			parameter = m_ProposedFamilySymbol.LookupParameter(RevitParameters.SideWalkPavementDepthParameter);
			if (parameter != null)
			{
				num = parameter.AsDouble();
			}
			break;
		case LandcaddID.RetainingWall:
			parameter = m_ProposedFamilySymbol.LookupParameter(RevitParameters.RetainingWallHeightParameter);
			if (parameter != null)
			{
				num = parameter.AsDouble();
			}
			break;
		case LandcaddID.Street:
			parameter = m_ProposedFamilySymbol.LookupParameter(RevitParameters.StreetPavementDepthParameter);
			if (parameter != null)
			{
				num = parameter.AsDouble();
			}
			break;
		case LandcaddID.Curb:
			parameter = m_ProposedFamilySymbol.LookupParameter(RevitParameters.CurbGutterHeightParameter);
			if (parameter != null)
			{
				num = parameter.AsDouble();
			}
			break;
		}
		if (!RevitDefines.EQ(num, 0.0))
		{
			if (m_OriginalLeg != null)
			{
				m_OriginalLeg = m_OriginalLeg.MoveLeg(0.0, 0.0, num);
			}
			if (m_ProposedLeg != null)
			{
				m_ProposedLeg = m_ProposedLeg.MoveLeg(0.0, 0.0, num);
			}
		}
		m_bFormDepthApplied = true;
	}

	public int IndexOfFamilyInstanceUniqueID(string _strUniqueID)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			FamilyInstance familyInstance = null;
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				familyInstance = m_FamilyContainerList[i].FamilyInstance;
				if (familyInstance != null && string.Compare(familyInstance.UniqueId, _strUniqueID, ignoreCase: true) == 0)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public int IndexOfOriginalElementID(ElementId _ContainerID)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				if (m_FamilyContainerList[i].Element.Id == _ContainerID)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public int IndexOfProposedElementID(ElementId _ElementId)
	{
		int num = 0;
		if (m_ProposedElementSet != null)
		{
			foreach (Element item in m_ProposedElementSet)
			{
				if (item.Id == _ElementId)
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	public int IndexOfFamilyInstanceID(ElementId _ElementId)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				if (m_FamilyContainerList[i].FamilyInstance.Id == _ElementId)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public FamilyInstance FindFamilyInstance(ElementId _FindElementId)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				if (m_FamilyContainerList[i].Element.Id == _FindElementId)
				{
					return m_FamilyContainerList[i].FamilyInstance;
				}
			}
		}
		return null;
	}

	public Element GetFirstExistingElement()
	{
		ElementId findElementId = RevitIds.Create(-1);
		return GetFirstExistingElement(findElementId);
	}

	public Element GetFirstExistingElement(ElementId _FindElementId)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				if (_FindElementId.Value64() == -1 || _FindElementId == m_FamilyContainerList[i].Element.Id)
				{
					return m_FamilyContainerList[i].Element;
				}
			}
		}
		return null;
	}

	public FamilyInstance GetFirstExistingFamilyInstance()
	{
		ElementId findElementId = RevitIds.Create(-1);
		return GetFirstExistingFamilyInstance(findElementId);
	}

	public FamilyInstance GetFirstExistingFamilyInstance(ElementId _FindElementId)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				if (_FindElementId.Value64() == -1 || _FindElementId == m_FamilyContainerList[i].Element.Id)
				{
					return m_FamilyContainerList[i].FamilyInstance;
				}
			}
		}
		return null;
	}

	public Element FindElement(ElementId _FindElementId)
	{
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				if (m_FamilyContainerList[i].Element.Id == _FindElementId)
				{
					return m_FamilyContainerList[i].Element;
				}
			}
		}
		return null;
	}

	public ElementSet GetOriginalElementSet()
	{
		ElementSet elementSet = new ElementSet();
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				elementSet.Insert(m_FamilyContainerList[i].Element);
			}
		}
		return elementSet;
	}

	public ElementSet GetFamilyInstanceElementSet()
	{
		ElementSet elementSet = new ElementSet();
		if (m_FamilyContainerList != null && m_FamilyContainerList.Count > 0)
		{
			for (int i = 0; i < m_FamilyContainerList.Count; i++)
			{
				elementSet.Insert(m_FamilyContainerList[i].FamilyInstance);
			}
		}
		return elementSet;
	}

	public void CreateNewProposedElementSet(Document _Document)
	{
		if (m_MassLineSubCategory == null)
		{
			m_MassLineSubCategory = RevitUtility.GetSiteComponentLineSubCategory(_Document, m_CurrentLandcaddId);
		}
		if (m_ProposedElementSet == null)
		{
			m_ProposedElementSet = new ElementSet();
		}
		else
		{
			m_ProposedElementSet.Clear();
		}
		SketchPlane sketchPlane = null;
		Element firstExistingElement = GetFirstExistingElement();
		if (firstExistingElement != null && firstExistingElement is CurveElement curveElement)
		{
			sketchPlane = curveElement.SketchPlane;
		}
		ModelCurveArray modelCurveArray = null;
		modelCurveArray = ((sketchPlane == null) ? RevitUtility.NewModelCurveArray(_Document, m_ProposedLeg) : RevitUtility.NewModelCurveArray(_Document, m_ProposedLeg, sketchPlane, _bUse2011TempLines: true));
		foreach (ModelCurve item in modelCurveArray)
		{
			if (m_MassLineSubCategory != null)
			{
				item.LineStyle = m_MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
			}
			m_ProposedElementSet.Insert(item);
		}
	}

	public void CreateNewProposedElementSet(Document _Document, EPTransactionContainer _TransactionContainer)
	{
		if (m_MassLineSubCategory == null)
		{
			m_MassLineSubCategory = RevitUtility.GetSiteComponentLineSubCategory(_Document, m_CurrentLandcaddId);
		}
		if (m_ProposedElementSet == null)
		{
			m_ProposedElementSet = new ElementSet();
		}
		else
		{
			m_ProposedElementSet.Clear();
		}
		SketchPlane sketchPlane = null;
		Element firstExistingElement = GetFirstExistingElement();
		if (firstExistingElement != null && firstExistingElement is CurveElement curveElement)
		{
			sketchPlane = curveElement.SketchPlane;
		}
		_TransactionContainer.BeginTransaction();
		ModelCurveArray modelCurveArray = null;
		modelCurveArray = ((sketchPlane == null) ? RevitUtility.NewModelCurveArray(_Document, m_ProposedLeg) : RevitUtility.NewModelCurveArray(_Document, m_ProposedLeg, sketchPlane, _bUse2011TempLines: true));
		_TransactionContainer.EndTransaction();
		foreach (ModelCurve item in modelCurveArray)
		{
			if (m_MassLineSubCategory != null)
			{
				item.LineStyle = m_MassLineSubCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
			}
			m_ProposedElementSet.Insert(item);
		}
	}

	public void CreateParkingLotBreaklineLegs(TopographySurfaceContainer _Surface)
	{
		List<TopographySurfaceContainer> list = new List<TopographySurfaceContainer>();
		list.Add(_Surface);
		CreateParkingLotBreaklineLegs(list, null);
	}

	public void CreateParkingLotBreaklineLegs(List<TopographySurfaceContainer> _SurfaceList)
	{
		CreateParkingLotBreaklineLegs(_SurfaceList, null);
	}

	public void CreateParkingLotBreaklineLegs(List<TopographySurfaceContainer> _SurfaceList, Phase _CurPhase)
	{
		List<Eps_Line> list = new List<Eps_Line>();
		List<Eps_Arc> list2 = new List<Eps_Arc>();
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Element element = null;
		Parameter parameter = null;
		m_ParkingLotBreaklineLinearPointIntervals.Clear();
		bool flag = false;
		if (m_ParkingLotBreaklineFamilyInstanceList.Count == m_ParkingLotBreaklineElementList.Count)
		{
			flag = true;
		}
		for (int i = 0; i < m_ParkingLotBreaklineElementList.Count; i++)
		{
			element = m_ParkingLotBreaklineElementList[i];
			if (element == null || !(element.Location is LocationCurve { Curve: var curve }) || !(curve != null))
			{
				continue;
			}
			if (flag && m_ParkingLotBreaklineFamilyInstanceList[i] != null)
			{
				parameter = m_ParkingLotBreaklineFamilyInstanceList[i].LookupParameter(RevitParameters.LinearPointIntervalParameter);
				if (parameter != null)
				{
					m_ParkingLotBreaklineLinearPointIntervals.Add(parameter.AsDouble());
				}
			}
			if (curve is Arc)
			{
				Arc arc = (Arc)curve;
				Eps_Arc item = new Eps_Arc(arc);
				list2.Add(item);
			}
			else if (curve is Line)
			{
				Line line = (Line)curve;
				Eps_Line item2 = new Eps_Line(line);
				list.Add(item2);
			}
			else
			{
				if (!(curve is Ellipse) && !(curve is NurbSpline) && !(curve is HermiteSpline))
				{
					continue;
				}
				eps_Leg2 = RevitGeometry.GetLegFromSpline(element, curve, _bReverseOrder: false);
				for (eps_Leg3 = eps_Leg2; eps_Leg3 != null; eps_Leg3 = eps_Leg3.Next)
				{
					if (eps_Leg3.Line != null)
					{
						list.Add(new Eps_Line(eps_Leg3.Line));
					}
					else if (eps_Leg3.Arc != null)
					{
						list2.Add(new Eps_Arc(eps_Leg3.Arc));
					}
				}
			}
		}
		bool flag2 = false;
		while (list.Count > 0 || list2.Count > 0)
		{
			for (int j = 0; j < m_ParkingLotBreaklineList.Count; j++)
			{
				eps_Leg = m_ParkingLotBreaklineList[j];
				for (int num = list.Count - 1; num >= 0; num--)
				{
					Eps_Line currentLine = list[num];
					if (Eps_Leg.InsertLineLegSegment(ref eps_Leg, currentLine))
					{
						m_ParkingLotBreaklineList[j] = eps_Leg;
						flag2 = true;
						list.RemoveAt(num);
					}
				}
				for (int num2 = list2.Count - 1; num2 >= 0; num2--)
				{
					Eps_Arc currentArc = list2[num2];
					if (Eps_Leg.InsertArcLegSegment(ref eps_Leg, currentArc))
					{
						m_ParkingLotBreaklineList[j] = eps_Leg;
						flag2 = true;
						list2.RemoveAt(num2);
					}
				}
			}
			if (!flag2)
			{
				if (list.Count > 0)
				{
					eps_Leg = new Eps_Leg();
					eps_Leg.Line = list[0];
					m_ParkingLotBreaklineList.Add(eps_Leg);
					list.RemoveAt(0);
				}
				else if (list2.Count > 0)
				{
					eps_Leg = new Eps_Leg();
					eps_Leg.Arc = list2[0];
					m_ParkingLotBreaklineList.Add(eps_Leg);
					list2.RemoveAt(0);
				}
			}
			else
			{
				flag2 = false;
			}
		}
		for (int k = 0; k < m_ParkingLotBreaklineList.Count; k++)
		{
			m_ParkingLotBreaklineList[k] = m_ParkingLotBreaklineList[k].TopographySurfaceLeg(_SurfaceList, _CurPhase);
		}
	}

	public int FindBreaklineIndexFromElementId(Element CurElement)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		int num = 0;
		if (CurElement != null && CurElement.Location is LocationCurve { Curve: var curve } && curve != null)
		{
			for (num = 0; num < m_ParkingLotBreaklineList.Count; num++)
			{
				Eps_Leg eps_Leg4 = m_ParkingLotBreaklineList[num];
				for (eps_Leg2 = eps_Leg4; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
				{
					if (curve is Arc && eps_Leg2.Arc != null)
					{
						Arc arc = (Arc)curve;
						Eps_Arc eps_Arc = new Eps_Arc(arc);
						if (eps_Arc.IsEqual(eps_Leg2.Arc))
						{
							return num;
						}
					}
					else if (curve is Line && eps_Leg2.Line != null)
					{
						Line line = (Line)curve;
						Eps_Line eps_Line = new Eps_Line(line);
						if (eps_Line.IsEqual(eps_Leg2.Line))
						{
							return num;
						}
					}
					else if (curve is Ellipse || curve is NurbSpline || curve is HermiteSpline)
					{
						eps_Leg = RevitGeometry.GetLegFromSpline(CurElement, curve, _bReverseOrder: false);
						for (eps_Leg3 = eps_Leg; eps_Leg3 != null; eps_Leg3 = eps_Leg3.Next)
						{
							if (eps_Leg3.Line != null && eps_Leg2.Line != null)
							{
								if (eps_Leg3.Line.IsEqual(eps_Leg2.Line))
								{
									return num;
								}
							}
							else if (eps_Leg3.Arc != null && eps_Leg2.Arc != null && eps_Leg3.Arc.IsEqual(eps_Leg2.Arc))
							{
								return num;
							}
						}
					}
				}
			}
		}
		return -1;
	}

	public ElementSet RemoveBreaklineAndElementsFromList(int _nRemoveBreaklineIndex)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Element element = null;
		ElementSet elementSet = new ElementSet();
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		if (m_ParkingLotBreaklineFamilyInstanceList.Count == m_ParkingLotBreaklineElementList.Count)
		{
			flag2 = true;
		}
		bool flag3 = false;
		if (m_ParkingLotBreaklineLinearPointIntervals.Count == m_ParkingLotBreaklineElementList.Count)
		{
			flag3 = true;
		}
		if (_nRemoveBreaklineIndex >= 0 && _nRemoveBreaklineIndex < m_ParkingLotBreaklineList.Count)
		{
			Eps_Leg eps_Leg4 = m_ParkingLotBreaklineList[_nRemoveBreaklineIndex];
			for (eps_Leg2 = eps_Leg4; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
			{
				for (num = m_ParkingLotBreaklineElementList.Count - 1; num >= 0; num--)
				{
					element = m_ParkingLotBreaklineElementList[num];
					if (element != null && element.Location is LocationCurve { Curve: var curve } && curve != null)
					{
						if (curve is Arc && eps_Leg2.Arc != null)
						{
							Arc arc = (Arc)curve;
							Eps_Arc eps_Arc = new Eps_Arc(arc);
							if (eps_Arc.IsEqual(eps_Leg2.Arc))
							{
								elementSet.Insert(m_ParkingLotBreaklineElementList[num]);
								m_ParkingLotBreaklineElementList.RemoveAt(num);
								if (flag2)
								{
									elementSet.Insert(m_ParkingLotBreaklineFamilyInstanceList[num]);
									m_ParkingLotBreaklineFamilyInstanceList.RemoveAt(num);
								}
								if (flag3)
								{
									m_ParkingLotBreaklineLinearPointIntervals.RemoveAt(num);
								}
								break;
							}
						}
						else if (curve is Line && eps_Leg2.Line != null)
						{
							Line line = (Line)curve;
							Eps_Line eps_Line = new Eps_Line(line);
							if (eps_Line.IsEqual(eps_Leg2.Line))
							{
								elementSet.Insert(m_ParkingLotBreaklineElementList[num]);
								m_ParkingLotBreaklineElementList.RemoveAt(num);
								if (flag2)
								{
									elementSet.Insert(m_ParkingLotBreaklineFamilyInstanceList[num]);
									m_ParkingLotBreaklineFamilyInstanceList.RemoveAt(num);
								}
								if (flag3)
								{
									m_ParkingLotBreaklineLinearPointIntervals.RemoveAt(num);
								}
								break;
							}
						}
						else if (curve is Ellipse || curve is NurbSpline || curve is HermiteSpline)
						{
							eps_Leg = RevitGeometry.GetLegFromSpline(element, curve, _bReverseOrder: false);
							for (eps_Leg3 = eps_Leg; eps_Leg3 != null; eps_Leg3 = eps_Leg3.Next)
							{
								if (eps_Leg3.Line != null && eps_Leg2.Line != null)
								{
									if (eps_Leg3.Line.IsEqual(eps_Leg2.Line))
									{
										elementSet.Insert(m_ParkingLotBreaklineElementList[num]);
										m_ParkingLotBreaklineElementList.RemoveAt(num);
										if (flag2)
										{
											elementSet.Insert(m_ParkingLotBreaklineFamilyInstanceList[num]);
											m_ParkingLotBreaklineFamilyInstanceList.RemoveAt(num);
										}
										if (flag3)
										{
											m_ParkingLotBreaklineLinearPointIntervals.RemoveAt(num);
										}
										flag = true;
										break;
									}
								}
								else if (eps_Leg3.Arc != null && eps_Leg2.Arc != null && eps_Leg3.Arc.IsEqual(eps_Leg2.Arc))
								{
									elementSet.Insert(m_ParkingLotBreaklineElementList[num]);
									m_ParkingLotBreaklineElementList.RemoveAt(num);
									if (flag2)
									{
										elementSet.Insert(m_ParkingLotBreaklineFamilyInstanceList[num]);
										m_ParkingLotBreaklineFamilyInstanceList.RemoveAt(num);
									}
									if (flag3)
									{
										m_ParkingLotBreaklineLinearPointIntervals.RemoveAt(num);
									}
									flag = true;
									break;
								}
							}
							if (flag)
							{
								flag = false;
								break;
							}
						}
					}
				}
			}
			m_ParkingLotBreaklineList.RemoveAt(_nRemoveBreaklineIndex);
		}
		return elementSet;
	}

	public FamilyInstance FindIntersectionFamilyInstance(ElementId id)
	{
		foreach (FamilyInstance intersectionFamilyInstance in IntersectionFamilyInstanceList)
		{
			if (intersectionFamilyInstance.Id == id)
			{
				return intersectionFamilyInstance;
			}
		}
		return null;
	}

	public void AddIntersectionFamilyInstances(List<FamilyInstance> familyInstances)
	{
		foreach (FamilyInstance familyInstance in familyInstances)
		{
			if (FindIntersectionFamilyInstance(familyInstance.Id) == null)
			{
				IntersectionFamilyInstanceList.Add(familyInstance);
			}
		}
	}
}
