using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using SITES.Utilities;

namespace EPCommon;

public class CurbForm : BaseForm
{
	private enum CurbOffsetPoint
	{
		InnerSubgradeEdge,
		GutterEdge,
		FlowlineEdge,
		TopOfCurbEdge,
		BackOfCurbEdge,
		OuterSubgradeEdge
	}

	private bool m_bIsLeftCurb;

	private const double m_bInscribedRadius = 0.0625;

	private double m_dGutterSlope;

	private double m_dSubgradeSlope;

	private double m_dSubgradeDepth;

	private double m_dGutterWidth;

	private double m_dCurbWidth;

	private double m_dCurbOffset;

	private double m_dCurbHeight;

	private FamilySymbol m_CurbFamilySymbol;

	private bool m_bUseInternalPoint = true;

	public FamilySymbol CurbFamilySymbol => m_CurbFamilySymbol;

	public CurbForm(FamilySymbol _FamilySymbol, Eps_Leg _CurrentLeg, bool _bAttachedToStreet, bool _bIsLeftCurb)
		: base(_CurrentLeg)
	{
		m_dSubgradeSlope = 0.0;
		m_dGutterSlope = 0.0;
		m_dSubgradeDepth = 0.0;
		m_dGutterWidth = 2.0;
		m_dCurbWidth = 1.0;
		m_dCurbOffset = 0.2;
		m_dCurbHeight = 0.5;
		m_bIsLeftCurb = _bIsLeftCurb;
		m_CurbFamilySymbol = _FamilySymbol;
		if (_FamilySymbol == null)
		{
			return;
		}
		Parameter parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetGutterWidthParameter);
		if (parameter != null)
		{
			m_dGutterWidth = parameter.AsDouble();
		}
		parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetGutterSlopeParameter);
		if (parameter != null)
		{
			m_dGutterSlope = parameter.AsDouble();
		}
		if (_bAttachedToStreet)
		{
			parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetPavementDepthParameter);
			if (parameter != null)
			{
				m_dSubgradeDepth = parameter.AsDouble();
			}
			parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
			if (parameter != null)
			{
				m_dSubgradeSlope = parameter.AsDouble();
			}
		}
		else
		{
			parameter = _FamilySymbol.LookupParameter(RevitParameters.CurbGutterHeightParameter);
			if (parameter != null)
			{
				m_dSubgradeDepth = parameter.AsDouble();
			}
		}
		parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetCurbHeightParameter);
		if (parameter != null)
		{
			m_dCurbHeight = parameter.AsDouble();
		}
		parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetCurbOffsetParameter);
		if (parameter != null)
		{
			m_dCurbOffset = parameter.AsDouble();
		}
		parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetCurbWidthParameter);
		if (parameter != null)
		{
			m_dCurbWidth = parameter.AsDouble();
		}
	}

	private List<Autodesk.Revit.DB.Form> CreateLoftForm(double _dStartingOffset, double _dStartingElevation)
	{
		List<Autodesk.Revit.DB.Form> list = new List<Autodesk.Revit.DB.Form>();
		m_ProgressBarForm = new ProgressBarForm();
		m_ProgressBarForm.ShowForm();
		m_ProgressBarForm.Refresh();
		m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingCurbMassMessage");
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = 0;
		List<ReferenceArrayArray> referenceArrayArray = GetReferenceArrayArray(_dStartingOffset, _dStartingElevation);
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = referenceArrayArray.Count;
		Autodesk.Revit.DB.Form form = null;
		for (int i = 0; i < referenceArrayArray.Count; i++)
		{
			if (referenceArrayArray[i] != null && referenceArrayArray[i].Size > 0)
			{
				string _strErrorMessage = string.Empty;
				form = NewLoftForm(referenceArrayArray[i], _dStartingOffset, _dStartingElevation, i, ref _strErrorMessage);
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

	private List<Solid> CreateBlends(double _dStartingOffset, double _dStartingElevation)
	{
		List<Solid> list = new List<Solid>();
		m_ProgressBarForm = new ProgressBarForm();
		m_ProgressBarForm.ShowForm();
		m_ProgressBarForm.Refresh();
		m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingCurbMassMessage");
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = 0;
		List<List<CurveLoop>> curveLoopArray = GetCurveLoopArray(_dStartingOffset, _dStartingElevation);
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = curveLoopArray.Count;
		Solid solid = null;
		for (int i = 0; i < curveLoopArray.Count; i++)
		{
			if (curveLoopArray[i] != null && curveLoopArray[i].Count > 0)
			{
				string _strErrorMessage = string.Empty;
				solid = NewBlend(curveLoopArray[i], _dStartingOffset, _dStartingElevation, i, ref _strErrorMessage);
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

	private Autodesk.Revit.DB.Form NewLoftForm(ReferenceArrayArray _CurbReferenceArrayArray, double _dStartingOffset, double _dStartingElevation, int _nProcessLegSegment, ref string _strErrorMessage)
	{
		Autodesk.Revit.DB.Form result = null;
		try
		{
			result = m_MassDocument.FamilyCreate.NewLoftForm(isSolid: true, _CurbReferenceArrayArray);
			m_dOffsetAdjust = 0.01;
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
		{
			if (string.Compare(ex.Message, "The attempted operation is not permitted in this type of family.", ignoreCase: true) == 0)
			{
				_strErrorMessage = EPShared.LocalRM.GetString("CurbTemplateNotMassTypeMessage");
				return null;
			}
			if (RevitDefines.LT(m_dOffsetAdjust, 0.03))
			{
				m_dOffsetAdjust += 0.005;
				List<ReferenceArrayArray> referenceArrayArray = GetReferenceArrayArray(_dStartingOffset, _dStartingElevation, _nProcessLegSegment);
				if (referenceArrayArray.Count > 0)
				{
					result = NewLoftForm(referenceArrayArray[0], _dStartingOffset, _dStartingElevation, _nProcessLegSegment, ref _strErrorMessage);
				}
			}
			else
			{
				_strErrorMessage = EPShared.LocalRM.GetString("UnableToCreateMassMessage");
			}
		}
		return result;
	}

	private Solid NewBlend(List<CurveLoop> _CurbReferenceArrayArray, double _dStartingOffset, double _dStartingElevation, int _nProcessLegSegment, ref string _strErrorMessage)
	{
		Solid result = null;
		try
		{
			SolidOptions solidOptions = null;
			Parameter parameter = CurbFamilySymbol.LookupParameter(RevitParameters.StreetCurbMaterialParameter);
			if (parameter != null)
			{
				int num = 0;
				num = parameter.AsInteger();
				solidOptions = new SolidOptions(RevitIds.Create(num), ElementId.InvalidElementId);
			}
			result = GeometryCreationUtilities.CreateLoftGeometry(_CurbReferenceArrayArray, solidOptions);
			m_dOffsetAdjust = 0.01;
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
		{
			if (string.Compare(ex.Message, "The attempted operation is not permitted in this type of family.", ignoreCase: true) == 0)
			{
				_strErrorMessage = EPShared.LocalRM.GetString("CurbTemplateNotMassTypeMessage");
				return null;
			}
			if (RevitDefines.LT(m_dOffsetAdjust, 0.03))
			{
				m_dOffsetAdjust += 0.005;
				List<List<CurveLoop>> curveLoopArray = GetCurveLoopArray(_dStartingOffset, _dStartingElevation, _nProcessLegSegment);
				if (curveLoopArray.Count > 0)
				{
					result = NewBlend(curveLoopArray[0], _dStartingOffset, _dStartingElevation, _nProcessLegSegment, ref _strErrorMessage);
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
		m_bUseInternalPoint = _bUseInternalPoint;
		List<Solid> list = CreateBlends(_dStartingOffset, _dStartingElevation);
		if (list == null || list.Count == 0)
		{
			return string.Empty;
		}
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

	public List<Autodesk.Revit.DB.Form> CreateLoftFormWithParameters(double _dStartingOffset, double _dStartingElevation, bool _bUseInternalPoint)
	{
		m_bUseInternalPoint = _bUseInternalPoint;
		List<Autodesk.Revit.DB.Form> list = CreateLoftForm(_dStartingOffset, _dStartingElevation);
		string _strReturnValue = string.Empty;
		Category category = null;
		Category familyCategory = m_MassDocument.OwnerFamily.FamilyCategory;
		if (familyCategory != null && familyCategory.SubCategories != null && familyCategory.SubCategories.Size > 0)
		{
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("CurbMassSubCategoryName", ref _strReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("CurbMassSubCatergoryName", ref _strReturnValue))
			{
				_strReturnValue = EPShared.LocalRM.GetString("SWCurbLineNameString");
				ApplicationUtility.Settings.eplSetE_SetSetting("CurbMassSubCategoryName", _strReturnValue);
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
					familyParameter = m_MassDocument.FamilyManager.AddParameter(RevitParameters.StreetCurbMaterialParameter, BuiltInParameterGroup.PG_MATERIALS, ParameterType.Material, isInstance: true);
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

	public List<ReferenceArrayArray> GetReferenceArrayArray(double _dStartingOffset, double _dStartingElevation)
	{
		return GetReferenceArrayArray(_dStartingOffset, _dStartingElevation, -1);
	}

	public List<List<CurveLoop>> GetCurveLoopArray(double _dStartingOffset, double _dStartingElevation)
	{
		return GetCurveLoopArray(_dStartingOffset, _dStartingElevation, -1);
	}

	private List<ReferenceArrayArray> GetReferenceArrayArray(double _dStartingOffset, double _dStartingElevation, int _nProcessLegSegment)
	{
		List<ReferenceArrayArray> list = new List<ReferenceArrayArray>();
		ReferenceArrayArray referenceArrayArray = null;
		ReferenceArray referenceArray = null;
		Eps_Leg eps_Leg = null;
		double num = 0.0;
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point eps_Point2 = new Eps_Point();
		double num2 = 0.0;
		double dOffsetAdjust = m_dOffsetAdjust;
		double _dDirection = 0.0;
		double num3 = 0.0;
		double _dTangentOutDir = 0.0;
		double dAdjustDistance = 0.0;
		bool flag = false;
		double dDirection = 0.0;
		bool flag2 = false;
		double dAdjustDistance2 = 0.0;
		int num4 = 0;
		if (m_MassLeg != null)
		{
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
				referenceArrayArray = new ReferenceArrayArray();
				num = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg);
				if (eps_Leg.Arc != null)
				{
					Arc arc = eps_Leg.Arc.RevitArc(m_MassDocument);
					double _dStation = 0.0;
					double _dOffset = 0.0;
					int num5 = 0;
					int num6 = 0;
					List<XYZ> list2 = arc.Tessellate().ToList();
					num6 = list2.Count;
					foreach (XYZ item in list2)
					{
						Eps_Point point = new Eps_Point(item);
						if (num5 % 5 != 0 && num5 != 1 && num5 != num6 - 1)
						{
							num5++;
							continue;
						}
						EPShared.eplGeoE_CalcStationOffsetAlongArc(point, eps_Leg.Arc, ref _dStation, ref _dOffset);
						if (num4 > 0 && num5 == 0)
						{
							_dStation += dOffsetAdjust;
						}
						if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null && num5 == num6 - 1)
						{
							_dStation -= dOffsetAdjust;
						}
						if (num4 == 0 && num5 == 0)
						{
							if (m_MassLeg.IsLegClosed())
							{
								int num7 = 0;
								EPShared.eplGeoE_CalcTangentInDirOfArc(eps_Leg.Arc, ref _dDirection);
								if (eps_Leg.Last.Line != null)
								{
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Last.Line.StartPnt, eps_Leg.Last.Line.EndPnt, ref _dTangentOutDir);
									num7 = RevitGeometry.CalcSignedArea(eps_Leg.Arc.StartPnt, eps_Leg.Arc.EndPnt, eps_Leg.Last.Line.EndPnt);
								}
								else if (eps_Leg.Last.Arc != null)
								{
									EPShared.eplGeoE_CalcTangentOutDirOfArc(eps_Leg.Last.Arc, ref _dTangentOutDir);
									num7 = RevitGeometry.CalcSignedArea(eps_Leg.Arc.StartPnt, eps_Leg.Arc.EndPnt, eps_Leg.Last.Arc.EndPnt);
								}
								if (!RevitDefines.EQ(_dDirection, _dTangentOutDir))
								{
									flag2 = true;
									double num8 = 0.0;
									num8 = EPShared.eplGeoE_NormalizeRadians(_dTangentOutDir - _dDirection);
									double SweepAngle = 0.0;
									RevitGeometry.eplGeoE_CalcSweepAngle(num7, _dTangentOutDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle);
									dDirection = ((num7 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num7 * (SweepAngle / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num7 * (SweepAngle / 2.0)));
									dAdjustDistance2 = Math.Abs(Math.Cos(num8 / 2.0));
								}
							}
							referenceArray = (((num4 != 0 && eps_Leg.Next != null) || !flag2) ? FindReferenceArrayProfileAtStation(num4, num2 + _dStation, _dStartingElevation) : FindReferenceArrayProfileAtStation(num4, dOffsetAdjust, _bUseDirection: true, dDirection, dAdjustDistance2, _dStartingElevation));
						}
						else if (num5 == num6 - 1)
						{
							if (eps_Leg.Next == null || eps_Leg.Next.Line == null)
							{
								referenceArray = (((num4 != 0 && eps_Leg.Next != null) || !flag2) ? FindReferenceArrayProfileAtStation(num4, num2 + _dStation) : FindReferenceArrayProfileAtStation(num4, num2 + _dStation, _bUseDirection: true, dDirection, dAdjustDistance2, _dStartingElevation));
							}
							else
							{
								int num9 = 0;
								EPShared.eplGeoE_CalcTangentOutDirOfArc(eps_Leg.Arc, ref _dDirection);
								EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dTangentOutDir);
								num9 = RevitGeometry.CalcSignedArea(eps_Leg.Arc.StartPnt, eps_Leg.Arc.EndPnt, eps_Leg.Next.Line.EndPnt);
								if (!RevitDefines.EQ(_dDirection, _dTangentOutDir))
								{
									double num10 = 0.0;
									num10 = EPShared.eplGeoE_NormalizeRadians(_dTangentOutDir - _dDirection);
									double SweepAngle2 = 0.0;
									RevitGeometry.eplGeoE_CalcSweepAngle(num9, _dTangentOutDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle2);
									num3 = ((num9 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle2 / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle2 / 2.0)));
									if (!RevitDefines.EQ(_dDirection, num3))
									{
										flag = true;
										dAdjustDistance = Math.Abs(Math.Cos(num10 / 2.0));
										referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + _dStation, _bUseDirection: true, num3, dAdjustDistance, _dStartingElevation);
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
						}
						else if (flag)
						{
							referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance, _dStartingElevation);
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
						num5++;
					}
					num2 += num;
				}
				else
				{
					if (num4 == 0)
					{
						if (m_MassLeg.IsLegClosed())
						{
							int num11 = 0;
							EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
							if (eps_Leg.Last.Line != null)
							{
								EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Last.Line.StartPnt, eps_Leg.Last.Line.EndPnt, ref _dTangentOutDir);
								num11 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Last.Line.EndPnt);
							}
							else if (eps_Leg.Last.Arc != null)
							{
								EPShared.eplGeoE_CalcTangentOutDirOfArc(eps_Leg.Last.Arc, ref _dTangentOutDir);
								num11 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Last.Arc.EndPnt);
							}
							if (!RevitDefines.EQ(_dDirection, _dTangentOutDir))
							{
								flag2 = true;
								double num12 = 0.0;
								num12 = EPShared.eplGeoE_NormalizeRadians(_dTangentOutDir - _dDirection);
								double SweepAngle3 = 0.0;
								RevitGeometry.eplGeoE_CalcSweepAngle(num11, _dTangentOutDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle3);
								dDirection = ((num11 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num11 * (SweepAngle3 / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num11 * (SweepAngle3 / 2.0)));
								dAdjustDistance2 = Math.Abs(Math.Cos(num12 / 2.0));
							}
						}
						referenceArray = (((num4 != 0 && eps_Leg.Next != null) || !flag2) ? FindReferenceArrayProfileAtStation(num4, num2, _dStartingElevation) : FindReferenceArrayProfileAtStation(num4, dOffsetAdjust, _bUseDirection: true, dDirection, dAdjustDistance2, _dStartingElevation));
					}
					else if (flag)
					{
						referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance, _dStartingElevation);
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
					num2 += num;
					if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null)
					{
						num2 -= dOffsetAdjust;
					}
					if (eps_Leg.Next == null)
					{
						referenceArray = (((num4 != 0 && eps_Leg.Next != null) || !flag2) ? FindReferenceArrayProfileAtStation(num4, num2) : FindReferenceArrayProfileAtStation(num4, num2, _bUseDirection: true, dDirection, dAdjustDistance2, _dStartingElevation));
					}
					else
					{
						int num13 = 0;
						EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
						if (eps_Leg.Next.Line != null)
						{
							EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dTangentOutDir);
							num13 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Line.EndPnt);
						}
						else if (eps_Leg.Next.Arc != null)
						{
							EPShared.eplGeoE_CalcTangentInDirOfArc(eps_Leg.Next.Arc, ref _dTangentOutDir);
							num13 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Arc.EndPnt);
						}
						if (!RevitDefines.EQ(_dDirection, _dTangentOutDir))
						{
							double num14 = 0.0;
							num14 = EPShared.eplGeoE_NormalizeRadians(_dTangentOutDir - _dDirection);
							double SweepAngle4 = 0.0;
							RevitGeometry.eplGeoE_CalcSweepAngle(num13, _dTangentOutDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle4);
							num3 = ((num13 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num13 * (SweepAngle4 / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num13 * (SweepAngle4 / 2.0)));
							if (!RevitDefines.EQ(_dDirection, num3))
							{
								flag = true;
								dAdjustDistance = Math.Abs(Math.Cos(num14 / 2.0));
								referenceArray = FindReferenceArrayProfileAtStation(num4, num2, _bUseDirection: true, num3, dAdjustDistance, _dStartingElevation);
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
					if (referenceArray != null && referenceArray.Size > 0)
					{
						referenceArrayArray.Append(referenceArray);
						referenceArray = null;
					}
				}
				list.Add(referenceArrayArray);
				num4++;
			}
		}
		return list;
	}

	private List<List<CurveLoop>> GetCurveLoopArray(double _dStartingOffset, double _dStartingElevation, int _nProcessLegSegment)
	{
		List<List<CurveLoop>> list = new List<List<CurveLoop>>();
		List<CurveLoop> list2 = null;
		CurveLoop curveLoop = null;
		Eps_Leg eps_Leg = null;
		double num = 0.0;
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point eps_Point2 = new Eps_Point();
		double num2 = 0.0;
		double dOffsetAdjust = m_dOffsetAdjust;
		double _dDirection = 0.0;
		double num3 = 0.0;
		double _dTangentOutDir = 0.0;
		double dAdjustDistance = 0.0;
		bool flag = false;
		double dDirection = 0.0;
		bool flag2 = false;
		double dAdjustDistance2 = 0.0;
		int num4 = 0;
		if (m_MassLeg != null)
		{
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
				list2 = new List<CurveLoop>();
				num = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg);
				if (eps_Leg.Arc != null)
				{
					Arc arc = eps_Leg.Arc.RevitArc(m_MassDocument);
					double _dStation = 0.0;
					double _dOffset = 0.0;
					int num5 = 0;
					int num6 = 0;
					List<XYZ> list3 = arc.Tessellate().ToList();
					num6 = list3.Count;
					foreach (XYZ item in list3)
					{
						Eps_Point point = new Eps_Point(item);
						if (num5 % 5 != 0 && num5 != 1 && num5 != num6 - 1)
						{
							num5++;
							continue;
						}
						EPShared.eplGeoE_CalcStationOffsetAlongArc(point, eps_Leg.Arc, ref _dStation, ref _dOffset);
						if (num4 > 0 && num5 == 0)
						{
							_dStation += dOffsetAdjust;
						}
						if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null && num5 == num6 - 1)
						{
							_dStation -= dOffsetAdjust;
						}
						if (num4 == 0 && num5 == 0)
						{
							if (m_MassLeg.IsLegClosed())
							{
								int num7 = 0;
								EPShared.eplGeoE_CalcTangentInDirOfArc(eps_Leg.Arc, ref _dDirection);
								if (eps_Leg.Last.Line != null)
								{
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Last.Line.StartPnt, eps_Leg.Last.Line.EndPnt, ref _dTangentOutDir);
									num7 = RevitGeometry.CalcSignedArea(eps_Leg.Arc.StartPnt, eps_Leg.Arc.EndPnt, eps_Leg.Last.Line.EndPnt);
								}
								else if (eps_Leg.Last.Arc != null)
								{
									EPShared.eplGeoE_CalcTangentOutDirOfArc(eps_Leg.Last.Arc, ref _dTangentOutDir);
									num7 = RevitGeometry.CalcSignedArea(eps_Leg.Arc.StartPnt, eps_Leg.Arc.EndPnt, eps_Leg.Last.Arc.EndPnt);
								}
								if (!RevitDefines.EQ(_dDirection, _dTangentOutDir))
								{
									flag2 = true;
									double num8 = 0.0;
									num8 = EPShared.eplGeoE_NormalizeRadians(_dTangentOutDir - _dDirection);
									double SweepAngle = 0.0;
									RevitGeometry.eplGeoE_CalcSweepAngle(num7, _dTangentOutDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle);
									dDirection = ((num7 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num7 * (SweepAngle / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num7 * (SweepAngle / 2.0)));
									dAdjustDistance2 = Math.Abs(Math.Cos(num8 / 2.0));
								}
							}
							curveLoop = (((num4 != 0 && eps_Leg.Next != null) || !flag2) ? FindCurveLoopProfileAtStation(num4, num2 + _dStation, _dStartingElevation) : FindCurveLoopProfileAtStation(num4, dOffsetAdjust, _bUseDirection: true, dDirection, dAdjustDistance2, _dStartingElevation));
						}
						else if (num5 == num6 - 1)
						{
							if (eps_Leg.Next == null || eps_Leg.Next.Line == null)
							{
								curveLoop = (((num4 != 0 && eps_Leg.Next != null) || !flag2) ? FindCurveLoopProfileAtStation(num4, num2 + _dStation) : FindCurveLoopProfileAtStation(num4, num2 + _dStation, _bUseDirection: true, dDirection, dAdjustDistance2, _dStartingElevation));
							}
							else
							{
								int num9 = 0;
								EPShared.eplGeoE_CalcTangentOutDirOfArc(eps_Leg.Arc, ref _dDirection);
								EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dTangentOutDir);
								num9 = RevitGeometry.CalcSignedArea(eps_Leg.Arc.StartPnt, eps_Leg.Arc.EndPnt, eps_Leg.Next.Line.EndPnt);
								if (!RevitDefines.EQ(_dDirection, _dTangentOutDir))
								{
									double num10 = 0.0;
									num10 = EPShared.eplGeoE_NormalizeRadians(_dTangentOutDir - _dDirection);
									double SweepAngle2 = 0.0;
									RevitGeometry.eplGeoE_CalcSweepAngle(num9, _dTangentOutDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle2);
									num3 = ((num9 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle2 / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle2 / 2.0)));
									if (!RevitDefines.EQ(_dDirection, num3))
									{
										flag = true;
										dAdjustDistance = Math.Abs(Math.Cos(num10 / 2.0));
										curveLoop = FindCurveLoopProfileAtStation(num4, num2 + _dStation, _bUseDirection: true, num3, dAdjustDistance, _dStartingElevation);
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
						}
						else if (flag)
						{
							curveLoop = FindCurveLoopProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance, _dStartingElevation);
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
						num5++;
					}
					num2 += num;
				}
				else
				{
					if (num4 == 0)
					{
						if (m_MassLeg.IsLegClosed())
						{
							int num11 = 0;
							EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
							if (eps_Leg.Last.Line != null)
							{
								EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Last.Line.StartPnt, eps_Leg.Last.Line.EndPnt, ref _dTangentOutDir);
								num11 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Last.Line.EndPnt);
							}
							else if (eps_Leg.Last.Arc != null)
							{
								EPShared.eplGeoE_CalcTangentOutDirOfArc(eps_Leg.Last.Arc, ref _dTangentOutDir);
								num11 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Last.Arc.EndPnt);
							}
							if (!RevitDefines.EQ(_dDirection, _dTangentOutDir))
							{
								flag2 = true;
								double num12 = 0.0;
								num12 = EPShared.eplGeoE_NormalizeRadians(_dTangentOutDir - _dDirection);
								double SweepAngle3 = 0.0;
								RevitGeometry.eplGeoE_CalcSweepAngle(num11, _dTangentOutDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle3);
								dDirection = ((num11 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num11 * (SweepAngle3 / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num11 * (SweepAngle3 / 2.0)));
								dAdjustDistance2 = Math.Abs(Math.Cos(num12 / 2.0));
							}
						}
						curveLoop = (((num4 != 0 && eps_Leg.Next != null) || !flag2) ? FindCurveLoopProfileAtStation(num4, num2, _dStartingElevation) : FindCurveLoopProfileAtStation(num4, dOffsetAdjust, _bUseDirection: true, dDirection, dAdjustDistance2, _dStartingElevation));
					}
					else if (flag)
					{
						curveLoop = FindCurveLoopProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance, _dStartingElevation);
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
					num2 += num;
					if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null)
					{
						num2 -= dOffsetAdjust;
					}
					if (eps_Leg.Next == null)
					{
						curveLoop = (((num4 != 0 && eps_Leg.Next != null) || !flag2) ? FindCurveLoopProfileAtStation(num4, num2) : FindCurveLoopProfileAtStation(num4, num2, _bUseDirection: true, dDirection, dAdjustDistance2, _dStartingElevation));
					}
					else
					{
						int num13 = 0;
						EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
						if (eps_Leg.Next.Line != null)
						{
							EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dTangentOutDir);
							num13 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Line.EndPnt);
						}
						else if (eps_Leg.Next.Arc != null)
						{
							EPShared.eplGeoE_CalcTangentInDirOfArc(eps_Leg.Next.Arc, ref _dTangentOutDir);
							num13 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Arc.EndPnt);
						}
						if (!RevitDefines.EQ(_dDirection, _dTangentOutDir))
						{
							double num14 = 0.0;
							num14 = EPShared.eplGeoE_NormalizeRadians(_dTangentOutDir - _dDirection);
							double SweepAngle4 = 0.0;
							RevitGeometry.eplGeoE_CalcSweepAngle(num13, _dTangentOutDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle4);
							num3 = ((num13 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num13 * (SweepAngle4 / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num13 * (SweepAngle4 / 2.0)));
							if (!RevitDefines.EQ(_dDirection, num3))
							{
								flag = true;
								dAdjustDistance = Math.Abs(Math.Cos(num14 / 2.0));
								curveLoop = FindCurveLoopProfileAtStation(num4, num2, _bUseDirection: true, num3, dAdjustDistance, _dStartingElevation);
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
					if (curveLoop != null)
					{
						list2.Add(curveLoop);
						curveLoop = null;
					}
				}
				list.Add(list2);
				num4++;
			}
		}
		return list;
	}

	private ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation)
	{
		return FindReferenceArrayProfileAtStation(_nDocIndex, _dStation, 0.0, 0.0, _bUseDirection: false, 0.0, 1.0);
	}

	private CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation)
	{
		return FindCurveLoopProfileAtStation(_nDocIndex, _dStation, 0.0, 0.0, _bUseDirection: false, 0.0, 1.0);
	}

	private ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation, double _dStartElevation)
	{
		return FindReferenceArrayProfileAtStation(_nDocIndex, _dStation, 0.0, _dStartElevation, _bUseDirection: false, 0.0, 1.0);
	}

	private CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation, double _dStartElevation)
	{
		return FindCurveLoopProfileAtStation(_nDocIndex, _dStation, 0.0, _dStartElevation, _bUseDirection: false, 0.0, 1.0);
	}

	private ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance, double _dStartElevation)
	{
		return FindReferenceArrayProfileAtStation(_nDocIndex, _dStation, 0.0, _dStartElevation, _bUseDirection, _dDirection, _dAdjustDistance);
	}

	private CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance, double _dStartElevation)
	{
		return FindCurveLoopProfileAtStation(_nDocIndex, _dStation, 0.0, _dStartElevation, _bUseDirection, _dDirection, _dAdjustDistance);
	}

	public ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation, double _dStartingOffset, double _dStartingElevation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		ReferenceArray referenceArray = new ReferenceArray();
		CurveArray curveArray = new CurveArray();
		try
		{
			Eps_Point eps_Point = CalcTemplateOffsets(CurbOffsetPoint.InnerSubgradeEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point2 = CalcTemplateOffsets(CurbOffsetPoint.OuterSubgradeEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point3 = CalcTemplateOffsets(CurbOffsetPoint.BackOfCurbEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point4 = CalcTemplateOffsets(CurbOffsetPoint.TopOfCurbEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point5 = CalcTemplateOffsets(CurbOffsetPoint.FlowlineEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point6 = CalcTemplateOffsets(CurbOffsetPoint.GutterEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Line line = null;
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point6, eps_Point5, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point6.XYZ, eps_Point5.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point5, eps_Point4, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point5.XYZ, eps_Point4.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point4, eps_Point3, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point4.XYZ, eps_Point3.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point3, eps_Point2, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point3.XYZ, eps_Point2.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point2, eps_Point, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point2.XYZ, eps_Point.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point, eps_Point6, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point.XYZ, eps_Point6.XYZ, bBound: true);
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

	public CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation, double _dStartingOffset, double _dStartingElevation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		CurveLoop curveLoop = new CurveLoop();
		CurveArray curveArray = new CurveArray();
		try
		{
			Eps_Point eps_Point = CalcTemplateOffsets(CurbOffsetPoint.InnerSubgradeEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point2 = CalcTemplateOffsets(CurbOffsetPoint.OuterSubgradeEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point3 = CalcTemplateOffsets(CurbOffsetPoint.BackOfCurbEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point4 = CalcTemplateOffsets(CurbOffsetPoint.TopOfCurbEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point5 = CalcTemplateOffsets(CurbOffsetPoint.FlowlineEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point6 = CalcTemplateOffsets(CurbOffsetPoint.GutterEdge, _dStation, _dStartingOffset, _dStartingElevation, _bUseDirection, _dDirection, _dAdjustDistance);
			Line line = null;
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point6, eps_Point5, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point6.XYZ, eps_Point5.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point5, eps_Point4, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point5.XYZ, eps_Point4.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point4, eps_Point3, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point4.XYZ, eps_Point3.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point3, eps_Point2, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point3.XYZ, eps_Point2.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point2, eps_Point, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point2.XYZ, eps_Point.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point, eps_Point6, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point.XYZ, eps_Point6.XYZ, bBound: true);
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

	private Eps_Point CalcTemplateOffsets(CurbOffsetPoint _CurbOffsetPoint, double _dStation, double _dStartingOffset, double _dStartingElevation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		Eps_Point _NewPnt = new Eps_Point();
		double num = m_dGutterWidth / _dAdjustDistance;
		double num2 = m_dCurbWidth / _dAdjustDistance;
		double num3 = m_dCurbOffset / _dAdjustDistance;
		double num4 = 1.0;
		double num5 = _dStartingOffset / _dAdjustDistance;
		double num6 = _dStartingElevation;
		if (m_bIsLeftCurb)
		{
			num4 *= -1.0;
		}
		switch (_CurbOffsetPoint)
		{
		case CurbOffsetPoint.GutterEdge:
			if (!m_bUseInternalPoint)
			{
				num5 -= num4 * (num + num2);
				num6 -= m_dCurbHeight - num * m_dGutterSlope;
			}
			break;
		case CurbOffsetPoint.FlowlineEdge:
			if (m_bUseInternalPoint)
			{
				num5 += num4 * num;
				num6 -= num * m_dGutterSlope;
			}
			else
			{
				num5 -= num4 * num2;
				num6 -= m_dCurbHeight;
			}
			break;
		case CurbOffsetPoint.TopOfCurbEdge:
			if (m_bUseInternalPoint)
			{
				num5 += num4 * (num + num3);
				num6 += m_dCurbHeight - num * m_dGutterSlope;
			}
			else
			{
				num5 -= num4 * (num2 - num3);
			}
			break;
		case CurbOffsetPoint.BackOfCurbEdge:
			if (m_bUseInternalPoint)
			{
				num5 += num4 * (num + num2);
				num6 += m_dCurbHeight - num * m_dGutterSlope;
			}
			break;
		case CurbOffsetPoint.OuterSubgradeEdge:
			if (m_bUseInternalPoint)
			{
				num5 += num4 * (num + num2);
				num6 -= m_dSubgradeDepth + (num + num2) * m_dSubgradeSlope;
			}
			else
			{
				num6 -= m_dCurbHeight + m_dSubgradeDepth;
			}
			break;
		case CurbOffsetPoint.InnerSubgradeEdge:
			if (m_bUseInternalPoint)
			{
				num6 -= m_dSubgradeDepth;
				break;
			}
			num5 -= num4 * (num + num2);
			num6 -= m_dSubgradeDepth + m_dCurbHeight - (num + num2) * m_dGutterSlope;
			break;
		}
		if (!_bUseDirection)
		{
			RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation, num5, ref _NewPnt);
		}
		else
		{
			Eps_Point Pnt = new Eps_Point();
			RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation, 0.0, ref Pnt);
			if (num5 < 0.0)
			{
				num5 = Math.Abs(num5);
				_dDirection = EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI);
			}
			EPShared.eplGeoE_CalcPolarToXY(Pnt, _dDirection, num5, ref _NewPnt);
		}
		_NewPnt.z += num6;
		return _NewPnt;
	}
}
