using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using SITES.Utilities;

namespace EPCommon;

public class RetainingWallForm : BaseForm
{
	private enum RetainingWallOffsetPoint
	{
		LeftEdge,
		LeftSubgradeEdge,
		RightEdge,
		RightSubgradeEdge
	}

	private double m_dHeight;

	private double m_dWidth;

	private double m_dElevation;

	private FamilySymbol m_RetainingWallFamilySymbol;

	public FamilySymbol RetainingWallFamilySymbol => m_RetainingWallFamilySymbol;

	public RetainingWallForm(FamilySymbol _FamilySymbol, Eps_Leg _CurrentLeg, double _dElevation)
		: base(_CurrentLeg)
	{
		m_dHeight = 0.0;
		m_dWidth = 0.0;
		m_dElevation = _dElevation;
		m_RetainingWallFamilySymbol = _FamilySymbol;
		if (_FamilySymbol != null)
		{
			Parameter parameter = _FamilySymbol.LookupParameter(RevitParameters.RetainingWallWidthParameter);
			if (parameter != null)
			{
				m_dWidth = parameter.AsDouble();
			}
			parameter = _FamilySymbol.LookupParameter(RevitParameters.RetainingWallHeightParameter);
			if (parameter != null)
			{
				m_dHeight = parameter.AsDouble();
			}
		}
	}

	private List<Autodesk.Revit.DB.Form> CreateLoftForm()
	{
		List<Autodesk.Revit.DB.Form> list = new List<Autodesk.Revit.DB.Form>();
		m_ProgressBarForm = new ProgressBarForm();
		m_ProgressBarForm.ShowForm();
		m_ProgressBarForm.Refresh();
		m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingRetainingWallMassMessage");
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

	private List<Solid> CreateBlends(Document doc)
	{
		List<Solid> list = new List<Solid>();
		m_ProgressBarForm = new ProgressBarForm();
		m_ProgressBarForm.ShowForm();
		m_ProgressBarForm.Refresh();
		m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingRetainingWallMassMessage");
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
				solid = NewRetainingWallBlend(doc, curveLoopArrayArray[i], i, ref _strErrorMessage);
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

	private Autodesk.Revit.DB.Form NewLoftForm(ReferenceArrayArray _WallReferenceArrayArray, int _nProcessLegSegment, ref string _strErrorMessage)
	{
		Autodesk.Revit.DB.Form result = null;
		try
		{
			result = m_MassDocument.FamilyCreate.NewLoftForm(isSolid: true, _WallReferenceArrayArray);
			m_dOffsetAdjust = 0.01;
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
		{
			if (string.Compare(ex.Message, "The attempted operation is not permitted in this type of family.", ignoreCase: true) == 0)
			{
				_strErrorMessage = EPShared.LocalRM.GetString("RetainingWallTemplateNotMassTypeMessage");
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

	private Solid NewRetainingWallBlend(Document doc, List<CurveLoop> RetainingWallCurveLoopArray, int _nProcessLegSegment, ref string _strErrorMessage)
	{
		Solid result = null;
		try
		{
			SolidOptions solidOptions = null;
			Parameter parameter = RetainingWallFamilySymbol.LookupParameter(RevitParameters.RetainingWallMaterialParameter);
			if (parameter != null)
			{
				int num = 0;
				num = parameter.AsInteger();
				Material material = doc.GetElement(RevitIds.Create(num)) as Material;
				solidOptions = new SolidOptions(RevitIds.Create(num), ElementId.InvalidElementId);
			}
			result = GeometryCreationUtilities.CreateLoftGeometry(RetainingWallCurveLoopArray, solidOptions);
			m_dOffsetAdjust = 0.01;
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
		{
			if (string.Compare(ex.Message, "The attempted operation is not permitted in this type of family.", ignoreCase: true) == 0)
			{
				_strErrorMessage = EPShared.LocalRM.GetString("RetainingWallTemplateNotMassTypeMessage");
				return null;
			}
			if (RevitDefines.LT(m_dOffsetAdjust, 0.03))
			{
				m_dOffsetAdjust += 0.005;
				List<List<CurveLoop>> curveLoopArrayArray = GetCurveLoopArrayArray(_nProcessLegSegment);
				if (curveLoopArrayArray.Count > 0)
				{
					result = NewRetainingWallBlend(doc, curveLoopArrayArray[0], _nProcessLegSegment, ref _strErrorMessage);
				}
			}
			else
			{
				_strErrorMessage = EPShared.LocalRM.GetString("UnableToCreateMassMessage");
			}
		}
		return result;
	}

	public List<Autodesk.Revit.DB.Form> CreateLoftFormWithParameters()
	{
		List<Autodesk.Revit.DB.Form> list = CreateLoftForm();
		string _strReturnValue = string.Empty;
		Category category = null;
		Category familyCategory = m_MassDocument.OwnerFamily.FamilyCategory;
		if (familyCategory != null && familyCategory.SubCategories != null && familyCategory.SubCategories.Size > 0)
		{
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("RetainingWallMassSubCategoryName", ref _strReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("RetainingWallMassSubCatergoryName", ref _strReturnValue))
			{
				_strReturnValue = EPShared.LocalRM.GetString("SWRetainingWallLineNameString");
				ApplicationUtility.Settings.eplSetE_SetSetting("RetainingWallMassSubCategoryName", _strReturnValue);
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
					familyParameter = m_MassDocument.FamilyManager.AddParameter(RevitParameters.RetainingWallMaterialParameter, BuiltInParameterGroup.PG_MATERIALS, ParameterType.Material, isInstance: true);
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

	public override string CreateElement(Document doc, ref string _strAdditionalLoftID, string _strSiteComponentName, double _dStartingOffset, double _dStartingElevation, bool _bUseInternalPoint)
	{
		List<Solid> list = CreateBlends(doc);
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
		DirectShape directShape = DirectShape.CreateElement(doc, RevitIds.Create(BuiltInCategory.OST_Walls));
		directShape.ApplicationId = gUID.ToString();
		directShape.SetShape(list2);
		string uniqueId = directShape.UniqueId;
		ElementTransformUtils.MoveElement(doc, directShape.Id, m_LegInsertPoint.XYZ);
		directShape.Name = _strSiteComponentName;
		return uniqueId;
	}

	private List<ReferenceArrayArray> GetReferenceArrayArray()
	{
		return GetReferenceArrayArray(-1);
	}

	private List<List<CurveLoop>> GetCurveLoopArrayArray()
	{
		return GetCurveLoopArrayArray(-1);
	}

	private List<ReferenceArrayArray> GetReferenceArrayArray(int _nProcessLegSegment)
	{
		List<ReferenceArrayArray> list = new List<ReferenceArrayArray>();
		ReferenceArrayArray referenceArrayArray = null;
		ReferenceArray referenceArray = null;
		Eps_Leg eps_Leg = null;
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point eps_Point2 = new Eps_Point();
		double num = 0.0;
		double num2 = 0.0;
		double dOffsetAdjust = m_dOffsetAdjust;
		double _dDirection = 0.0;
		double num3 = 0.0;
		double _dTangentInDir = 0.0;
		double dAdjustDistance = 0.0;
		bool flag = false;
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
						if (num5 == num6 - 1 && eps_Leg.Next != null && eps_Leg.Next.Line != null)
						{
							int num7 = 0;
							EPShared.eplGeoE_CalcTangentOutDirOfArc(eps_Leg.Arc, ref _dDirection);
							EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dTangentInDir);
							num7 = RevitGeometry.CalcSignedArea(eps_Leg.Arc.StartPnt, eps_Leg.Arc.EndPnt, eps_Leg.Next.Line.EndPnt);
							if (!RevitDefines.EQ(_dDirection, _dTangentInDir))
							{
								double num8 = 0.0;
								num8 = EPShared.eplGeoE_NormalizeRadians(_dTangentInDir - _dDirection);
								double SweepAngle = 0.0;
								RevitGeometry.eplGeoE_CalcSweepAngle(num7, _dTangentInDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle);
								num3 = ((num7 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num7 * (SweepAngle / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num7 * (SweepAngle / 2.0)));
								if (!RevitDefines.EQ(_dDirection, num3))
								{
									flag = true;
									dAdjustDistance = Math.Abs(Math.Cos(num8 / 2.0));
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
						num5++;
					}
					num2 += num;
				}
				else
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
					num2 += num;
					if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null)
					{
						num2 -= dOffsetAdjust;
					}
					if (eps_Leg.Next != null)
					{
						int num9 = 0;
						EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
						if (eps_Leg.Next.Line != null)
						{
							EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dTangentInDir);
							num9 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Line.EndPnt);
						}
						else if (eps_Leg.Next.Arc != null)
						{
							EPShared.eplGeoE_CalcTangentInDirOfArc(eps_Leg.Next.Arc, ref _dTangentInDir);
							num9 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Arc.EndPnt);
						}
						if (!RevitDefines.EQ(_dDirection, _dTangentInDir))
						{
							double num10 = 0.0;
							num10 = EPShared.eplGeoE_NormalizeRadians(_dTangentInDir - _dDirection);
							double SweepAngle2 = 0.0;
							RevitGeometry.eplGeoE_CalcSweepAngle(num9, _dTangentInDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle2);
							num3 = ((num9 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle2 / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle2 / 2.0)));
							if (!RevitDefines.EQ(_dDirection, num3))
							{
								flag = true;
								dAdjustDistance = Math.Abs(Math.Cos(num10 / 2.0));
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
				list.Add(referenceArrayArray);
				num4++;
			}
		}
		return list;
	}

	private List<List<CurveLoop>> GetCurveLoopArrayArray(int _nProcessLegSegment)
	{
		List<List<CurveLoop>> list = new List<List<CurveLoop>>();
		List<CurveLoop> list2 = null;
		CurveLoop curveLoop = null;
		Eps_Leg eps_Leg = null;
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point eps_Point2 = new Eps_Point();
		double num = 0.0;
		double num2 = 0.0;
		double dOffsetAdjust = m_dOffsetAdjust;
		double _dDirection = 0.0;
		double num3 = 0.0;
		double _dTangentInDir = 0.0;
		double dAdjustDistance = 0.0;
		bool flag = false;
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
						if (num5 == num6 - 1 && eps_Leg.Next != null && eps_Leg.Next.Line != null)
						{
							int num7 = 0;
							EPShared.eplGeoE_CalcTangentOutDirOfArc(eps_Leg.Arc, ref _dDirection);
							EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dTangentInDir);
							num7 = RevitGeometry.CalcSignedArea(eps_Leg.Arc.StartPnt, eps_Leg.Arc.EndPnt, eps_Leg.Next.Line.EndPnt);
							if (!RevitDefines.EQ(_dDirection, _dTangentInDir))
							{
								double num8 = 0.0;
								num8 = EPShared.eplGeoE_NormalizeRadians(_dTangentInDir - _dDirection);
								double SweepAngle = 0.0;
								RevitGeometry.eplGeoE_CalcSweepAngle(num7, _dTangentInDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle);
								num3 = ((num7 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num7 * (SweepAngle / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num7 * (SweepAngle / 2.0)));
								if (!RevitDefines.EQ(_dDirection, num3))
								{
									flag = true;
									dAdjustDistance = Math.Abs(Math.Cos(num8 / 2.0));
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
						num5++;
					}
					num2 += num;
				}
				else
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
					num2 += num;
					if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null)
					{
						num2 -= dOffsetAdjust;
					}
					if (eps_Leg.Next != null)
					{
						int num9 = 0;
						EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
						if (eps_Leg.Next.Line != null)
						{
							EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Next.Line.StartPnt, eps_Leg.Next.Line.EndPnt, ref _dTangentInDir);
							num9 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Line.EndPnt);
						}
						else if (eps_Leg.Next.Arc != null)
						{
							EPShared.eplGeoE_CalcTangentInDirOfArc(eps_Leg.Next.Arc, ref _dTangentInDir);
							num9 = RevitGeometry.CalcSignedArea(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, eps_Leg.Next.Arc.EndPnt);
						}
						if (!RevitDefines.EQ(_dDirection, _dTangentInDir))
						{
							double num10 = 0.0;
							num10 = EPShared.eplGeoE_NormalizeRadians(_dTangentInDir - _dDirection);
							double SweepAngle2 = 0.0;
							RevitGeometry.eplGeoE_CalcSweepAngle(num9, _dTangentInDir, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref SweepAngle2);
							num3 = ((num9 != 1) ? EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle2 / 2.0) + Math.PI) : EPShared.eplGeoE_NormalizeRadians(_dDirection - (double)num9 * (SweepAngle2 / 2.0)));
							if (!RevitDefines.EQ(_dDirection, num3))
							{
								flag = true;
								dAdjustDistance = Math.Abs(Math.Cos(num10 / 2.0));
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
				list.Add(list2);
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

	private ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		ReferenceArray referenceArray = new ReferenceArray();
		CurveArray curveArray = new CurveArray();
		try
		{
			Eps_Point eps_Point = CalcTemplateOffsets(RetainingWallOffsetPoint.LeftEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point2 = CalcTemplateOffsets(RetainingWallOffsetPoint.RightEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point3 = CalcTemplateOffsets(RetainingWallOffsetPoint.LeftSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point4 = CalcTemplateOffsets(RetainingWallOffsetPoint.RightSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
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

	private CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		CurveLoop result = null;
		CurveArray curveArray = new CurveArray();
		List<Curve> list = new List<Curve>();
		try
		{
			Eps_Point eps_Point = CalcTemplateOffsets(RetainingWallOffsetPoint.LeftEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point2 = CalcTemplateOffsets(RetainingWallOffsetPoint.RightEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point3 = CalcTemplateOffsets(RetainingWallOffsetPoint.LeftSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point4 = CalcTemplateOffsets(RetainingWallOffsetPoint.RightSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
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
				list.Add(item);
			}
			result = CurveLoop.Create(list);
		}
		catch (Exception)
		{
		}
		return result;
	}

	private Eps_Point CalcTemplateOffsets(RetainingWallOffsetPoint _RetainingwallOffsetPoint, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		Eps_Point _NewPnt = new Eps_Point();
		double num = m_dWidth / _dAdjustDistance;
		double num2 = 0.0;
		double num3 = m_dElevation;
		switch (_RetainingwallOffsetPoint)
		{
		case RetainingWallOffsetPoint.LeftEdge:
			num2 = (0.0 - num) / 2.0;
			break;
		case RetainingWallOffsetPoint.RightEdge:
			num2 = num / 2.0;
			break;
		case RetainingWallOffsetPoint.LeftSubgradeEdge:
			num2 = (0.0 - num) / 2.0;
			num3 -= m_dHeight;
			break;
		case RetainingWallOffsetPoint.RightSubgradeEdge:
			num2 = num / 2.0;
			num3 -= m_dHeight;
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
