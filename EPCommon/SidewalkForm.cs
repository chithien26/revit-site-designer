using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using SITES.Utilities;

namespace EPCommon;

public class SidewalkForm : BaseForm
{
	private enum SidewalkOffsetPoint
	{
		LeftEdge,
		LeftSubgradeEdge,
		RightEdge,
		RightSubgradeEdge
	}

	private double m_dSubgradeDepth;

	private double m_dWidth;

	private FamilySymbol m_SidewalkFamilySymbol;

	public FamilySymbol SidewalkFamilySymbol => m_SidewalkFamilySymbol;

	public SidewalkForm(FamilySymbol _FamilySymbol, Eps_Leg _CurrentLeg)
		: base(_CurrentLeg)
	{
		m_dSubgradeDepth = 0.0;
		m_dWidth = 0.0;
		m_SidewalkFamilySymbol = _FamilySymbol;
		if (_FamilySymbol != null)
		{
			Parameter parameter = _FamilySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
			if (parameter != null)
			{
				m_dWidth = parameter.AsDouble();
			}
			parameter = _FamilySymbol.LookupParameter(RevitParameters.SideWalkPavementDepthParameter);
			if (parameter != null)
			{
				m_dSubgradeDepth = parameter.AsDouble();
			}
		}
	}

	private List<Autodesk.Revit.DB.Form> CreateLoftForm()
	{
		List<Autodesk.Revit.DB.Form> list = new List<Autodesk.Revit.DB.Form>();
		m_ProgressBarForm = new ProgressBarForm();
		m_ProgressBarForm.ShowForm();
		m_ProgressBarForm.Refresh();
		m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingSidewalkMassMessage");
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = 0;
		List<ReferenceArrayArray> referenceArrayArray = GetReferenceArrayArray();
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = referenceArrayArray.Count;
		Autodesk.Revit.DB.Form form = null;
		for (int i = 0; i < referenceArrayArray.Count; i++)
		{
			if (referenceArrayArray[i] != null && referenceArrayArray[i].Size > 0)
			{
				string _strErrorMessage = string.Empty;
				form = NewLoftForm(referenceArrayArray[i], i, ref _strErrorMessage);
				if (form == null)
				{
					m_TransactionContainer.AbortTransaction();
					m_TransactionContainer.BeginTransaction();
					m_ProgressBarForm.SetMaxValue();
					Thread.Sleep(300);
					m_ProgressBarForm.Close();
					list.Clear();
					RevitMessageBox.Show(_strErrorMessage, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return list;
				}
				list.Add(form);
			}
			m_ProgressBarForm.UpdateProgressBar(1);
		}
		m_ProgressBarForm.SetMaxValue();
		Thread.Sleep(300);
		m_ProgressBarForm.Close();
		return list;
	}

	private List<Solid> CreateBlends()
	{
		List<Solid> list = new List<Solid>();
		m_ProgressBarForm = new ProgressBarForm();
		m_ProgressBarForm.ShowForm();
		m_ProgressBarForm.Refresh();
		m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingSidewalkMassMessage");
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = 0;
		List<List<CurveLoop>> curveLoopArrayArray = GetCurveLoopArrayArray();
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = curveLoopArrayArray.Count;
		Solid solid = null;
		for (int i = 0; i < curveLoopArrayArray.Count; i++)
		{
			if (curveLoopArrayArray[i] != null && curveLoopArrayArray[i].Count > 0)
			{
				string _strErrorMessage = string.Empty;
				solid = NewBlend(curveLoopArrayArray[i], i, ref _strErrorMessage);
				if (solid == null)
				{
					m_ProgressBarForm.SetMaxValue();
					Thread.Sleep(300);
					m_ProgressBarForm.Close();
					list.Clear();
					RevitMessageBox.Show(_strErrorMessage, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return list;
				}
				list.Add(solid);
			}
			m_ProgressBarForm.UpdateProgressBar(1);
		}
		m_ProgressBarForm.SetMaxValue();
		Thread.Sleep(300);
		m_ProgressBarForm.Close();
		return list;
	}

	private Autodesk.Revit.DB.Form NewLoftForm(ReferenceArrayArray _SidewalkReferenceArrayArray, int _nProcessLegSegment, ref string _strErrorMessage)
	{
		Autodesk.Revit.DB.Form result = null;
		try
		{
			result = m_MassDocument.FamilyCreate.NewLoftForm(isSolid: true, _SidewalkReferenceArrayArray);
			m_dOffsetAdjust = 0.01;
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
		{
			if (string.Compare(ex.Message, "The attempted operation is not permitted in this type of family.", ignoreCase: true) == 0)
			{
				_strErrorMessage = EPShared.LocalRM.GetString("SidewalkTemplateNotMassTypeMessage");
				return null;
			}
			if (RevitDefines.LT(m_dOffsetAdjust, 0.03))
			{
				m_dOffsetAdjust += 0.005;
				List<ReferenceArrayArray> referenceArrayArray = GetReferenceArrayArray(_nProcessLegSegment);
				if (referenceArrayArray.Count > 0)
				{
					result = NewLoftForm(referenceArrayArray[0], _nProcessLegSegment, ref _strErrorMessage);
				}
			}
			else
			{
				_strErrorMessage = EPShared.LocalRM.GetString("UnableToCreateMassMessage");
			}
		}
		return result;
	}

	private Solid NewBlend(List<CurveLoop> _SidewalkCurveLoopArray, int _nProcessLegSegment, ref string _strErrorMessage)
	{
		Solid result = null;
		try
		{
			SolidOptions solidOptions = null;
			Parameter parameter = SidewalkFamilySymbol.LookupParameter(RevitParameters.SideWalkPavementSurfaceParameter);
			if (parameter != null)
			{
				int num = 0;
				num = parameter.AsInteger();
				solidOptions = new SolidOptions(RevitIds.Create(num), ElementId.InvalidElementId);
			}
			result = GeometryCreationUtilities.CreateLoftGeometry(_SidewalkCurveLoopArray, solidOptions);
			m_dOffsetAdjust = 0.01;
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
		{
			if (string.Compare(ex.Message, "The attempted operation is not permitted in this type of family.", ignoreCase: true) == 0)
			{
				_strErrorMessage = EPShared.LocalRM.GetString("SidewalkTemplateNotMassTypeMessage");
				return null;
			}
			if (RevitDefines.LT(m_dOffsetAdjust, 0.03))
			{
				m_dOffsetAdjust += 0.005;
				List<List<CurveLoop>> curveLoopArrayArray = GetCurveLoopArrayArray(_nProcessLegSegment);
				if (curveLoopArrayArray.Count > 0)
				{
					result = NewBlend(curveLoopArrayArray[0], _nProcessLegSegment, ref _strErrorMessage);
				}
			}
			else
			{
				_strErrorMessage = EPShared.LocalRM.GetString("UnableToCreateBlendGeometry");
			}
		}
		return result;
	}

	public override string CreateElement(Document doc, ref string _strAdditionalLoftID, string _strSiteComponentName, double _dStartingOffset, double _dStartingElevation, bool _bUseInternalPoint)
	{
		List<Solid> list = CreateBlends();
		List<GeometryObject> list2 = new List<GeometryObject>();
		foreach (Solid item in list)
		{
			list2.Add(item);
		}
		if (list2.Count == 0)
		{
			return string.Empty;
		}
		Guid gUID = doc.Application.ActiveAddInId.GetGUID();
		DirectShape directShape = DirectShape.CreateElement(doc, RevitIds.Create(BuiltInCategory.OST_Roads));
		directShape.ApplicationId = gUID.ToString();
		directShape.SetShape(list2);
		string uniqueId = directShape.UniqueId;
		ElementTransformUtils.MoveElement(doc, directShape.Id, m_LegInsertPoint.XYZ);
		directShape.Name = _strSiteComponentName;
		return uniqueId;
	}

	public List<Autodesk.Revit.DB.Form> CreateLoftFormWithParameters()
	{
		List<Autodesk.Revit.DB.Form> list = CreateLoftForm();
		string _strReturnValue = string.Empty;
		Category category = null;
		Category familyCategory = m_MassDocument.OwnerFamily.FamilyCategory;
		if (familyCategory != null && familyCategory.SubCategories != null && familyCategory.SubCategories.Size > 0)
		{
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("SidewalkMassSubCategoryName", ref _strReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("SidewalkMassSubCatergoryName", ref _strReturnValue))
			{
				_strReturnValue = EPShared.LocalRM.GetString("SWSidewalkLineNameString");
				ApplicationUtility.Settings.eplSetE_SetSetting("SidewalkMassSubCategoryName", _strReturnValue);
			}
			if (familyCategory.SubCategories.Contains(_strReturnValue))
			{
				category = familyCategory.SubCategories.get_Item(_strReturnValue);
			}
		}
		if (list != null && list.Count > 0)
		{
			if (category == null)
			{
				category = m_MassDocument.Settings.Categories.NewSubcategory(familyCategory, _strReturnValue);
			}
			FamilyParameter familyParameter = null;
			for (int i = 0; i < list.Count; i++)
			{
				if (i == 0)
				{
					familyParameter = m_MassDocument.FamilyManager.AddParameter(RevitParameters.SideWalkPavementSurfaceParameter, BuiltInParameterGroup.PG_MATERIALS, ParameterType.Material, isInstance: true);
				}
				try
				{
					Parameter parameter = ((Element)list[i]).get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
					if (parameter != null)
					{
						m_MassDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter, familyParameter);
					}
					if (category != null)
					{
						list[i].Subcategory = category;
					}
				}
				catch (Autodesk.Revit.Exceptions.InvalidOperationException)
				{
				}
			}
		}
		return list;
	}

	private List<ReferenceArrayArray> GetReferenceArrayArray()
	{
		return GetReferenceArrayArray(-1);
	}

	private List<List<CurveLoop>> GetCurveLoopArrayArray()
	{
		return GetCurveLoopArrayArray(-1);
	}

	private List<List<CurveLoop>> GetCurveLoopArrayArray(int _nProcessLegSegment)
	{
		List<List<CurveLoop>> list = new List<List<CurveLoop>>();
		List<CurveLoop> list2 = null;
		CurveLoop curveLoop = null;
		Eps_Leg eps_Leg = null;
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point eps_Point2 = new Eps_Point();
		Eps_Point eps_Point3 = null;
		Eps_Point eps_Point4 = null;
		double num = 0.0;
		double num2 = 0.0;
		double dOffsetAdjust = m_dOffsetAdjust;
		double _dDirection = 0.0;
		double num3 = 0.0;
		double _dDirection2 = 0.0;
		double dAdjustDistance = 0.0;
		bool flag = false;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		bool flag2 = true;
		if (m_MassLeg != null)
		{
			if (m_IntersectionContainers != null && m_IntersectionContainers.Count > 0)
			{
				flag2 = m_IntersectionContainers[0].StartingIntersection;
			}
			for (eps_Leg = m_MassLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
			{
				if (_nProcessLegSegment > -1)
				{
					if (num4 > _nProcessLegSegment)
					{
						break;
					}
					if (num4 < _nProcessLegSegment)
					{
						num4++;
						continue;
					}
				}
				if (num4 == 0 || flag2)
				{
					list2 = new List<CurveLoop>();
				}
				num = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg);
				if (eps_Leg.Arc != null)
				{
					Arc arc = eps_Leg.Arc.RevitArc(m_MassDocument);
					double _dStation = 0.0;
					double _dOffset = 0.0;
					double _dStation2 = 0.0;
					int num7 = 0;
					int num8 = 0;
					List<XYZ> list3 = arc.Tessellate().ToList();
					num8 = list3.Count;
					for (int i = 0; i < num8; i++)
					{
						Eps_Point point = new Eps_Point(list3[i]);
						if (i % 5 != 0 && i != 1 && i != num8 - 1)
						{
							continue;
						}
						EPShared.eplGeoE_CalcStationOffsetAlongArc(point, eps_Leg.Arc, ref _dStation, ref _dOffset);
						if (flag2)
						{
							if (num4 > 0 && i == 0)
							{
								_dStation += dOffsetAdjust;
							}
							if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null && i == num8 - 1)
							{
								_dStation -= dOffsetAdjust;
							}
							if (i == num8 - 1 && eps_Leg.Next != null && eps_Leg.Next.Line != null)
							{
								int num9 = 0;
								EPShared.eplGeoE_CalcTangentOutDirOfArc(eps_Leg.Arc, ref _dDirection);
								EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dDirection2);
								num9 = RevitGeometry.CalcSignedArea(eps_Leg.Arc.StartPnt, eps_Leg.Arc.EndPnt, eps_Leg.Next.Line.EndPnt);
								if (!RevitDefines.EQ(_dDirection, _dDirection2))
								{
									double num10 = 0.0;
									num10 = EPShared.eplGeoE_NormalizeRadians(_dDirection2 - _dDirection);
									double SweepAngle = 0.0;
									RevitGeometry.eplGeoE_CalcSweepAngle(num9, _dDirection2, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle);
									num3 = ((num9 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle / 2.0)));
									if (!RevitDefines.EQ(_dDirection, num3))
									{
										flag = true;
										dAdjustDistance = Math.Abs(Math.Cos(num10 / 2.0));
										curveLoop = FindCurveLoopProfileAtStation(num4, num2 + _dStation, _bUseDirection: true, num3, dAdjustDistance);
									}
									else
									{
										curveLoop = FindCurveLoopProfileAtStation(num4, num2 + _dStation);
									}
								}
								else
								{
									curveLoop = FindCurveLoopProfileAtStation(num4, num2 + _dStation);
								}
							}
							else if (flag)
							{
								curveLoop = FindCurveLoopProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance);
								flag = false;
							}
							else
							{
								curveLoop = FindCurveLoopProfileAtStation(num4, num2 + _dStation);
							}
							if (curveLoop != null)
							{
								list2.Add(curveLoop);
								curveLoop = null;
							}
						}
						num7 = i switch
						{
							0 => 1, 
							1 => 5, 
							_ => num7 + 5, 
						};
						if (num7 > num8 - 1)
						{
							num7 = num8 - 1;
						}
						Eps_Point point2 = new Eps_Point(list3[num7]);
						EPShared.eplGeoE_CalcStationOffsetAlongArc(point2, eps_Leg.Arc, ref _dStation2, ref _dOffset);
					}
					num2 += num;
				}
				else
				{
					if (flag2)
					{
						if (num4 == 0)
						{
							curveLoop = FindCurveLoopProfileAtStation(num4, num2);
						}
						else if (flag)
						{
							curveLoop = FindCurveLoopProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance);
							flag = false;
						}
						else
						{
							curveLoop = FindCurveLoopProfileAtStation(num4, num2 + dOffsetAdjust);
						}
						if (curveLoop != null)
						{
							list2.Add(curveLoop);
							curveLoop = null;
						}
					}
					if (m_IntersectionContainers != null)
					{
						while (num5 < m_IntersectionContainers.Count && num6 < m_IntersectionContainers[num5].Profiles.Count && m_IntersectionContainers[num5].Profiles[num6].Station <= num2 + num)
						{
							if (m_IntersectionContainers[num5].Profiles[num6].Station >= num2)
							{
								if (flag2 && m_IntersectionContainers[num5].StartingIntersection && num6 == 0 && m_IntersectionContainers[num5].Profiles[num6].Points.Count == 4)
								{
									eps_Point3 = m_IntersectionContainers[num5].Profiles[num6].Points[0];
									eps_Point4 = m_IntersectionContainers[num5].Profiles[num6].Points[m_IntersectionContainers[num5].Profiles[num6].Points.Count - 1];
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
									_dDirection = EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI / 2.0);
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point4, eps_Point3, ref _dDirection2);
									if (!RevitDefines.EQ(_dDirection, _dDirection2))
									{
										double num11 = 0.0;
										num11 = EPShared.eplGeoE_NormalizeRadians(_dDirection2 - _dDirection);
										dAdjustDistance = Math.Abs(Math.Cos(num11));
										curveLoop = FindCurveLoopProfileAtStation(num4, m_IntersectionContainers[num5].Profiles[num6].Station - dOffsetAdjust, _bUseDirection: true, _dDirection2, dAdjustDistance);
									}
									else
									{
										curveLoop = FindCurveLoopProfileAtStation(num4, m_IntersectionContainers[num5].Profiles[num6].Station - dOffsetAdjust);
									}
									if (curveLoop != null)
									{
										list2.Add(curveLoop);
										curveLoop = null;
									}
									if (list2 != null)
									{
										list.Add(list2);
										list2 = new List<CurveLoop>();
									}
								}
								curveLoop = ApplyCurveLoopProfile(m_IntersectionContainers[num5].Profiles[num6].Points);
								if (curveLoop != null)
								{
									list2.Add(curveLoop);
									curveLoop = null;
								}
								if (m_IntersectionContainers[num5].StartingIntersection)
								{
									if (num6 == 0)
									{
										flag2 = false;
									}
									else if (num6 == m_IntersectionContainers[num5].Profiles.Count - 1 && list2 != null)
									{
										list.Add(list2);
										list2 = new List<CurveLoop>();
									}
								}
								else if (num6 == m_IntersectionContainers[num5].Profiles.Count - 1)
								{
									flag2 = true;
									if (list2 != null)
									{
										list.Add(list2);
										list2 = new List<CurveLoop>();
									}
									if (m_IntersectionContainers[num5].Profiles[num6].Station + dOffsetAdjust < num2 + num && m_IntersectionContainers[num5].Profiles[num6].Points.Count == 4)
									{
										eps_Point3 = m_IntersectionContainers[num5].Profiles[num6].Points[0];
										eps_Point4 = m_IntersectionContainers[num5].Profiles[num6].Points[m_IntersectionContainers[num5].Profiles[num6].Points.Count - 1];
										EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.EndPnt, eps_Leg.Line.StartPnt, ref _dDirection);
										_dDirection = EPShared.eplGeoE_NormalizeRadians(_dDirection - Math.PI / 2.0);
										EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point4, eps_Point3, ref _dDirection2);
										if (!RevitDefines.EQ(_dDirection, _dDirection2))
										{
											double num12 = 0.0;
											num12 = EPShared.eplGeoE_NormalizeRadians(_dDirection2 - _dDirection);
											dAdjustDistance = Math.Abs(Math.Cos(num12));
											curveLoop = FindCurveLoopProfileAtStation(num4, m_IntersectionContainers[num5].Profiles[num6].Station + dOffsetAdjust, _bUseDirection: true, _dDirection2, dAdjustDistance);
										}
										else
										{
											curveLoop = FindCurveLoopProfileAtStation(num4, m_IntersectionContainers[num5].Profiles[num6].Station + dOffsetAdjust);
										}
										if (curveLoop != null)
										{
											list2.Add(curveLoop);
											curveLoop = null;
										}
									}
								}
							}
							num6++;
							if (num6 == m_IntersectionContainers[num5].Profiles.Count)
							{
								num6 = 0;
								num5++;
							}
							flag = false;
						}
					}
					num2 += num;
					if (flag2)
					{
						if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null)
						{
							num2 -= dOffsetAdjust;
						}
						if (eps_Leg.Next != null)
						{
							int num13 = 0;
							EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
							if (eps_Leg.Next.Line != null)
							{
								EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dDirection2);
								num13 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Line.EndPnt);
							}
							else if (eps_Leg.Next.Arc != null)
							{
								EPShared.eplGeoE_CalcTangentInDirOfArc(eps_Leg.Next.Arc, ref _dDirection2);
								num13 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Arc.EndPnt);
							}
							if (!RevitDefines.EQ(_dDirection, _dDirection2))
							{
								double num14 = 0.0;
								num14 = EPShared.eplGeoE_NormalizeRadians(_dDirection2 - _dDirection);
								double SweepAngle2 = 0.0;
								RevitGeometry.eplGeoE_CalcSweepAngle(num13, _dDirection2, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle2);
								num3 = ((num13 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num13 * (SweepAngle2 / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num13 * (SweepAngle2 / 2.0)));
								if (!RevitDefines.EQ(_dDirection, num3))
								{
									flag = true;
									dAdjustDistance = Math.Abs(Math.Cos(num14 / 2.0));
									curveLoop = FindCurveLoopProfileAtStation(num4, num2, _bUseDirection: true, num3, dAdjustDistance);
								}
								else
								{
									curveLoop = FindCurveLoopProfileAtStation(num4, num2);
								}
							}
							else
							{
								curveLoop = FindCurveLoopProfileAtStation(num4, num2);
							}
						}
						else
						{
							curveLoop = FindCurveLoopProfileAtStation(num4, num2);
						}
						if (curveLoop != null)
						{
							list2.Add(curveLoop);
							curveLoop = null;
						}
					}
				}
				if (flag2 && list2 != null)
				{
					list.Add(list2);
				}
				num4++;
			}
		}
		return list;
	}

	private List<ReferenceArrayArray> GetReferenceArrayArray(int _nProcessLegSegment)
	{
		List<ReferenceArrayArray> list = new List<ReferenceArrayArray>();
		ReferenceArrayArray referenceArrayArray = null;
		ReferenceArray referenceArray = null;
		Eps_Leg eps_Leg = null;
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point eps_Point2 = new Eps_Point();
		Eps_Point eps_Point3 = null;
		Eps_Point eps_Point4 = null;
		double num = 0.0;
		double num2 = 0.0;
		double dOffsetAdjust = m_dOffsetAdjust;
		double _dDirection = 0.0;
		double num3 = 0.0;
		double _dDirection2 = 0.0;
		double dAdjustDistance = 0.0;
		bool flag = false;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		bool flag2 = true;
		if (m_MassLeg != null)
		{
			if (m_IntersectionContainers != null && m_IntersectionContainers.Count > 0)
			{
				flag2 = m_IntersectionContainers[0].StartingIntersection;
			}
			for (eps_Leg = m_MassLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
			{
				if (_nProcessLegSegment > -1)
				{
					if (num4 > _nProcessLegSegment)
					{
						break;
					}
					if (num4 < _nProcessLegSegment)
					{
						num4++;
						continue;
					}
				}
				if (num4 == 0 || flag2)
				{
					referenceArrayArray = new ReferenceArrayArray();
				}
				num = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg);
				if (eps_Leg.Arc != null)
				{
					Arc arc = eps_Leg.Arc.RevitArc(m_MassDocument);
					double _dStation = 0.0;
					double _dOffset = 0.0;
					double _dStation2 = 0.0;
					int num7 = 0;
					int num8 = 0;
					List<XYZ> list2 = arc.Tessellate().ToList();
					num8 = list2.Count;
					for (int i = 0; i < num8; i++)
					{
						Eps_Point point = new Eps_Point(list2[i]);
						if (i % 5 != 0 && i != 1 && i != num8 - 1)
						{
							continue;
						}
						EPShared.eplGeoE_CalcStationOffsetAlongArc(point, eps_Leg.Arc, ref _dStation, ref _dOffset);
						if (flag2)
						{
							if (num4 > 0 && i == 0)
							{
								_dStation += dOffsetAdjust;
							}
							if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null && i == num8 - 1)
							{
								_dStation -= dOffsetAdjust;
							}
							if (i == num8 - 1 && eps_Leg.Next != null && eps_Leg.Next.Line != null)
							{
								int num9 = 0;
								EPShared.eplGeoE_CalcTangentOutDirOfArc(eps_Leg.Arc, ref _dDirection);
								EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dDirection2);
								num9 = RevitGeometry.CalcSignedArea(eps_Leg.Arc.StartPnt, eps_Leg.Arc.EndPnt, eps_Leg.Next.Line.EndPnt);
								if (!RevitDefines.EQ(_dDirection, _dDirection2))
								{
									double num10 = 0.0;
									num10 = EPShared.eplGeoE_NormalizeRadians(_dDirection2 - _dDirection);
									double SweepAngle = 0.0;
									RevitGeometry.eplGeoE_CalcSweepAngle(num9, _dDirection2, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle);
									num3 = ((num9 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle / 2.0)));
									if (!RevitDefines.EQ(_dDirection, num3))
									{
										flag = true;
										dAdjustDistance = Math.Abs(Math.Cos(num10 / 2.0));
										referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + _dStation, _bUseDirection: true, num3, dAdjustDistance);
									}
									else
									{
										referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + _dStation);
									}
								}
								else
								{
									referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + _dStation);
								}
							}
							else if (flag)
							{
								referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance);
								flag = false;
							}
							else
							{
								referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + _dStation);
							}
							if (referenceArray != null && referenceArray.Size > 0)
							{
								referenceArrayArray.Append(referenceArray);
								referenceArray = null;
							}
						}
						num7 = i switch
						{
							0 => 1, 
							1 => 5, 
							_ => num7 + 5, 
						};
						if (num7 > num8 - 1)
						{
							num7 = num8 - 1;
						}
						Eps_Point point2 = new Eps_Point(list2[num7]);
						EPShared.eplGeoE_CalcStationOffsetAlongArc(point2, eps_Leg.Arc, ref _dStation2, ref _dOffset);
					}
					num2 += num;
				}
				else
				{
					if (flag2)
					{
						if (num4 == 0)
						{
							referenceArray = FindReferenceArrayProfileAtStation(num4, num2);
						}
						else if (flag)
						{
							referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance);
							flag = false;
						}
						else
						{
							referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + dOffsetAdjust);
						}
						if (referenceArray != null && referenceArray.Size > 0)
						{
							referenceArrayArray.Append(referenceArray);
							referenceArray = null;
						}
					}
					if (m_IntersectionContainers != null)
					{
						while (num5 < m_IntersectionContainers.Count && num6 < m_IntersectionContainers[num5].Profiles.Count && m_IntersectionContainers[num5].Profiles[num6].Station <= num2 + num)
						{
							if (m_IntersectionContainers[num5].Profiles[num6].Station >= num2)
							{
								if (flag2 && m_IntersectionContainers[num5].StartingIntersection && num6 == 0 && m_IntersectionContainers[num5].Profiles[num6].Points.Count == 4)
								{
									eps_Point3 = m_IntersectionContainers[num5].Profiles[num6].Points[0];
									eps_Point4 = m_IntersectionContainers[num5].Profiles[num6].Points[m_IntersectionContainers[num5].Profiles[num6].Points.Count - 1];
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
									_dDirection = EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI / 2.0);
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point4, eps_Point3, ref _dDirection2);
									if (!RevitDefines.EQ(_dDirection, _dDirection2))
									{
										double num11 = 0.0;
										num11 = EPShared.eplGeoE_NormalizeRadians(_dDirection2 - _dDirection);
										dAdjustDistance = Math.Abs(Math.Cos(num11));
										referenceArray = FindReferenceArrayProfileAtStation(num4, m_IntersectionContainers[num5].Profiles[num6].Station - dOffsetAdjust, _bUseDirection: true, _dDirection2, dAdjustDistance);
									}
									else
									{
										referenceArray = FindReferenceArrayProfileAtStation(num4, m_IntersectionContainers[num5].Profiles[num6].Station - dOffsetAdjust);
									}
									if (referenceArray != null && referenceArray.Size > 0)
									{
										referenceArrayArray.Append(referenceArray);
										referenceArray = null;
									}
									if (referenceArrayArray != null && referenceArrayArray.Size > 0)
									{
										list.Add(referenceArrayArray);
										referenceArrayArray = new ReferenceArrayArray();
									}
								}
								referenceArray = ApplyReferenceArrayProfile(m_IntersectionContainers[num5].Profiles[num6].Points);
								if (referenceArray != null && referenceArray.Size > 0)
								{
									referenceArrayArray.Append(referenceArray);
									referenceArray = null;
								}
								if (m_IntersectionContainers[num5].StartingIntersection)
								{
									if (num6 == 0)
									{
										flag2 = false;
									}
									else if (num6 == m_IntersectionContainers[num5].Profiles.Count - 1 && referenceArrayArray != null && referenceArrayArray.Size > 0)
									{
										list.Add(referenceArrayArray);
										referenceArrayArray = new ReferenceArrayArray();
									}
								}
								else if (num6 == m_IntersectionContainers[num5].Profiles.Count - 1)
								{
									flag2 = true;
									if (referenceArrayArray != null && referenceArrayArray.Size > 0)
									{
										list.Add(referenceArrayArray);
										referenceArrayArray = new ReferenceArrayArray();
									}
									if (m_IntersectionContainers[num5].Profiles[num6].Station + dOffsetAdjust < num2 + num && m_IntersectionContainers[num5].Profiles[num6].Points.Count == 4)
									{
										eps_Point3 = m_IntersectionContainers[num5].Profiles[num6].Points[0];
										eps_Point4 = m_IntersectionContainers[num5].Profiles[num6].Points[m_IntersectionContainers[num5].Profiles[num6].Points.Count - 1];
										EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.EndPnt, eps_Leg.Line.StartPnt, ref _dDirection);
										_dDirection = EPShared.eplGeoE_NormalizeRadians(_dDirection - Math.PI / 2.0);
										EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point4, eps_Point3, ref _dDirection2);
										if (!RevitDefines.EQ(_dDirection, _dDirection2))
										{
											double num12 = 0.0;
											num12 = EPShared.eplGeoE_NormalizeRadians(_dDirection2 - _dDirection);
											dAdjustDistance = Math.Abs(Math.Cos(num12));
											referenceArray = FindReferenceArrayProfileAtStation(num4, m_IntersectionContainers[num5].Profiles[num6].Station + dOffsetAdjust, _bUseDirection: true, _dDirection2, dAdjustDistance);
										}
										else
										{
											referenceArray = FindReferenceArrayProfileAtStation(num4, m_IntersectionContainers[num5].Profiles[num6].Station + dOffsetAdjust);
										}
										if (referenceArray != null && referenceArray.Size > 0)
										{
											referenceArrayArray.Append(referenceArray);
											referenceArray = null;
										}
									}
								}
							}
							num6++;
							if (num6 == m_IntersectionContainers[num5].Profiles.Count)
							{
								num6 = 0;
								num5++;
							}
							flag = false;
						}
					}
					num2 += num;
					if (flag2)
					{
						if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null)
						{
							num2 -= dOffsetAdjust;
						}
						if (eps_Leg.Next != null)
						{
							int num13 = 0;
							EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
							if (eps_Leg.Next.Line != null)
							{
								EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dDirection2);
								num13 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Line.EndPnt);
							}
							else if (eps_Leg.Next.Arc != null)
							{
								EPShared.eplGeoE_CalcTangentInDirOfArc(eps_Leg.Next.Arc, ref _dDirection2);
								num13 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Arc.EndPnt);
							}
							if (!RevitDefines.EQ(_dDirection, _dDirection2))
							{
								double num14 = 0.0;
								num14 = EPShared.eplGeoE_NormalizeRadians(_dDirection2 - _dDirection);
								double SweepAngle2 = 0.0;
								RevitGeometry.eplGeoE_CalcSweepAngle(num13, _dDirection2, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle2);
								num3 = ((num13 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num13 * (SweepAngle2 / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num13 * (SweepAngle2 / 2.0)));
								if (!RevitDefines.EQ(_dDirection, num3))
								{
									flag = true;
									dAdjustDistance = Math.Abs(Math.Cos(num14 / 2.0));
									referenceArray = FindReferenceArrayProfileAtStation(num4, num2, _bUseDirection: true, num3, dAdjustDistance);
								}
								else
								{
									referenceArray = FindReferenceArrayProfileAtStation(num4, num2);
								}
							}
							else
							{
								referenceArray = FindReferenceArrayProfileAtStation(num4, num2);
							}
						}
						else
						{
							referenceArray = FindReferenceArrayProfileAtStation(num4, num2);
						}
						if (referenceArray != null && referenceArray.Size > 0)
						{
							referenceArrayArray.Append(referenceArray);
							referenceArray = null;
						}
					}
				}
				if (flag2 && referenceArrayArray != null && referenceArrayArray.Size > 0)
				{
					list.Add(referenceArrayArray);
				}
				num4++;
			}
		}
		return list;
	}

	private ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation)
	{
		return FindReferenceArrayProfileAtStation(_nDocIndex, _dStation, _bUseDirection: false, 0.0, 1.0);
	}

	private CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation)
	{
		return FindCurveLoopProfileAtStation(_nDocIndex, _dStation, _bUseDirection: false, 0.0, 1.0);
	}

	private CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		CurveLoop curveLoop = new CurveLoop();
		CurveArray curveArray = new CurveArray();
		try
		{
			Eps_Point eps_Point = CalcTemplateOffsets(SidewalkOffsetPoint.LeftEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point2 = CalcTemplateOffsets(SidewalkOffsetPoint.RightEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point3 = CalcTemplateOffsets(SidewalkOffsetPoint.LeftSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point4 = CalcTemplateOffsets(SidewalkOffsetPoint.RightSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Line line = null;
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point, eps_Point3, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point.XYZ, eps_Point3.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point3, eps_Point4, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point3.XYZ, eps_Point4.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point4, eps_Point2, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point4.XYZ, eps_Point2.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point2, eps_Point, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point2.XYZ, eps_Point.XYZ, bBound: true);
				curveArray.Append(line);
			}
			foreach (Curve item in curveArray)
			{
				curveLoop.Append(item);
			}
		}
		catch (Exception)
		{
		}
		return curveLoop;
	}

	private ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		ReferenceArray referenceArray = new ReferenceArray();
		CurveArray curveArray = new CurveArray();
		try
		{
			Eps_Point eps_Point = CalcTemplateOffsets(SidewalkOffsetPoint.LeftEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point2 = CalcTemplateOffsets(SidewalkOffsetPoint.RightEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point3 = CalcTemplateOffsets(SidewalkOffsetPoint.LeftSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point4 = CalcTemplateOffsets(SidewalkOffsetPoint.RightSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Line line = null;
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point, eps_Point3, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point.XYZ, eps_Point3.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point3, eps_Point4, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point3.XYZ, eps_Point4.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point4, eps_Point2, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point4.XYZ, eps_Point2.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point2, eps_Point, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point2.XYZ, eps_Point.XYZ, bBound: true);
				curveArray.Append(line);
			}
			CurveLoop curveLoop = new CurveLoop();
			foreach (Curve item in curveArray)
			{
				curveLoop.Append(item);
			}
			Plane plane = curveLoop.GetPlane();
			SketchPlane sketchPlane = SketchPlane.Create(m_MassDocument, plane);
			ModelCurve modelCurve = null;
			foreach (Curve item2 in curveArray)
			{
				modelCurve = m_MassDocument.FamilyCreate.NewModelCurve(item2, sketchPlane);
				referenceArray.Append(modelCurve.GeometryCurve.Reference);
			}
		}
		catch (Exception)
		{
		}
		return referenceArray;
	}

	private Eps_Point CalcTemplateOffsets(SidewalkOffsetPoint _SidewalkOffsetPoint, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		Eps_Point _NewPnt = new Eps_Point();
		double num = m_dWidth / _dAdjustDistance;
		double num2 = 0.0;
		double num3 = 0.0;
		switch (_SidewalkOffsetPoint)
		{
		case SidewalkOffsetPoint.LeftEdge:
			num2 = (0.0 - num) / 2.0;
			break;
		case SidewalkOffsetPoint.RightEdge:
			num2 = num / 2.0;
			break;
		case SidewalkOffsetPoint.LeftSubgradeEdge:
			num2 = (0.0 - num) / 2.0;
			num3 = 0.0 - m_dSubgradeDepth;
			break;
		case SidewalkOffsetPoint.RightSubgradeEdge:
			num2 = num / 2.0;
			num3 = 0.0 - m_dSubgradeDepth;
			break;
		}
		if (!_bUseDirection)
		{
			RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation, num2, ref _NewPnt);
		}
		else
		{
			Eps_Point Pnt = new Eps_Point();
			RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation, 0.0, ref Pnt);
			if (num2 < 0.0)
			{
				num2 = Math.Abs(num2);
				_dDirection = EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI);
			}
			EPShared.eplGeoE_CalcPolarToXY(Pnt, _dDirection, num2, ref _NewPnt);
		}
		_NewPnt.z += num3;
		return _NewPnt;
	}
}
