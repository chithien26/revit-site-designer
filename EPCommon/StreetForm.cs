using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using SITES.Utilities;

namespace EPCommon;

public class StreetForm : BaseForm
{
	private enum StreetOffsetPoint
	{
		LeftEdge,
		LeftSubgradeEdge,
		CrownEdge,
		SubgradeCrownEdge,
		RightEdge,
		RightSubgradeEdge
	}

	private double m_dSlope;

	private double m_dSubgradeDepth;

	private double m_dWidth;

	private FamilySymbol m_StreetFamilySymbol;

	private CurbForm m_LeftCurbForm;

	private CurbForm m_RightCurbForm;

	public FamilySymbol StreetFamilySymbol => m_StreetFamilySymbol;

	public double PavementWidth
	{
		get => m_dWidth;
		set { if (value > 0.0) m_dWidth = value; }
	}

	public StreetForm(FamilySymbol _FamilySymbol, Eps_Leg _CurrentLeg)
		: base(_CurrentLeg)
	{
		m_LeftCurbForm = null;
		m_RightCurbForm = null;
		m_dSlope = 0.0;
		m_dSubgradeDepth = 0.0;
		m_dWidth = 24.0;
		double num = 12.0;
		m_StreetFamilySymbol = _FamilySymbol;
		if (_FamilySymbol != null)
		{
			Parameter parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
			if (parameter != null)
			{
				num = parameter.AsDouble();
			}
			parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
			if (parameter != null)
			{
				m_dWidth = num * (double)parameter.AsInteger();
			}
			parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
			if (parameter != null)
			{
				m_dSlope = parameter.AsDouble();
			}
			parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetPavementDepthParameter);
			if (parameter != null)
			{
				m_dSubgradeDepth = parameter.AsDouble();
			}
			parameter = _FamilySymbol.LookupParameter(RevitParameters.StreetCurbDisplayParameter);
			if (parameter != null && parameter.AsInteger() != 0)
			{
				m_LeftCurbForm = new CurbForm(_FamilySymbol, _CurrentLeg, _bAttachedToStreet: true, _bIsLeftCurb: true);
				m_RightCurbForm = new CurbForm(_FamilySymbol, _CurrentLeg, _bAttachedToStreet: true, _bIsLeftCurb: false);
			}
		}
	}

	private List<Autodesk.Revit.DB.Form> CreateLoftForm(ref List<Autodesk.Revit.DB.Form> _LeftCurbLoftFormList, ref List<Autodesk.Revit.DB.Form> _RightCurbLoftFormList)
	{
		List<Autodesk.Revit.DB.Form> list = new List<Autodesk.Revit.DB.Form>();
		m_ProgressBarForm = new ProgressBarForm();
		m_ProgressBarForm.ShowForm();
		m_ProgressBarForm.Refresh();
		m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingStreetMassMessage");
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = 0;
		List<ReferenceArrayArray> _LeftCurbArrayArrayList = new List<ReferenceArrayArray>();
		List<ReferenceArrayArray> _RightCurbArrayArrayList = new List<ReferenceArrayArray>();
		List<ReferenceArrayArray> referenceArrayArray = GetReferenceArrayArray(ref _LeftCurbArrayArrayList, ref _RightCurbArrayArrayList);
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = referenceArrayArray.Count + _LeftCurbArrayArrayList.Count + _RightCurbArrayArrayList.Count;
		Autodesk.Revit.DB.Form form = null;
		string _strErrorMessage = string.Empty;
		for (int i = 0; i < referenceArrayArray.Count; i++)
		{
			if (referenceArrayArray[i] != null && referenceArrayArray[i].Size > 0)
			{
				form = NewStreetLoftForm(referenceArrayArray[i], i, ref _strErrorMessage);
				if (form == null)
				{
					m_TransactionContainer.AbortTransaction();
					m_TransactionContainer.BeginTransaction();
					if (m_AdditionalTransactionContainer != null)
					{
						m_AdditionalTransactionContainer.AbortTransaction();
						m_AdditionalTransactionContainer.BeginTransaction();
					}
					m_ProgressBarForm.SetMaxValue();
					Thread.Sleep(300);
					m_ProgressBarForm.Close();
					list.Clear();
					_LeftCurbLoftFormList.Clear();
					_RightCurbLoftFormList.Clear();
					RevitMessageBox.Show(_strErrorMessage, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return list;
				}
				list.Add(form);
			}
			m_ProgressBarForm.UpdateProgressBar(1);
		}
		for (int j = 0; j < _LeftCurbArrayArrayList.Count; j++)
		{
			if (_LeftCurbArrayArrayList[j] != null && _LeftCurbArrayArrayList[j].Size > 0)
			{
				form = NewCurbLoftForm(_LeftCurbArrayArrayList[j], j, _bIsLeftCurb: true, ref _strErrorMessage);
				if (form == null)
				{
					m_TransactionContainer.AbortTransaction();
					m_TransactionContainer.BeginTransaction();
					if (m_AdditionalTransactionContainer != null)
					{
						m_AdditionalTransactionContainer.AbortTransaction();
						m_AdditionalTransactionContainer.BeginTransaction();
					}
					m_ProgressBarForm.SetMaxValue();
					Thread.Sleep(300);
					m_ProgressBarForm.Close();
					list.Clear();
					_LeftCurbLoftFormList.Clear();
					_RightCurbLoftFormList.Clear();
					RevitMessageBox.Show(_strErrorMessage, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return list;
				}
				_LeftCurbLoftFormList.Add(form);
			}
			m_ProgressBarForm.UpdateProgressBar(1);
		}
		for (int k = 0; k < _RightCurbArrayArrayList.Count; k++)
		{
			if (_RightCurbArrayArrayList[k] != null && _RightCurbArrayArrayList[k].Size > 0)
			{
				form = NewCurbLoftForm(_RightCurbArrayArrayList[k], k, _bIsLeftCurb: false, ref _strErrorMessage);
				if (form == null)
				{
					m_TransactionContainer.AbortTransaction();
					m_TransactionContainer.BeginTransaction();
					if (m_AdditionalTransactionContainer != null)
					{
						m_AdditionalTransactionContainer.AbortTransaction();
						m_AdditionalTransactionContainer.BeginTransaction();
					}
					m_ProgressBarForm.SetMaxValue();
					Thread.Sleep(300);
					m_ProgressBarForm.Close();
					list.Clear();
					_LeftCurbLoftFormList.Clear();
					_RightCurbLoftFormList.Clear();
					RevitMessageBox.Show(_strErrorMessage, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return list;
				}
				_RightCurbLoftFormList.Add(form);
			}
			m_ProgressBarForm.UpdateProgressBar(1);
		}
		m_ProgressBarForm.SetMaxValue();
		Thread.Sleep(300);
		m_ProgressBarForm.Close();
		return list;
	}

	private bool TrySweepTypeSection(List<Solid> pavement, List<Solid> leftCurbs, List<Solid> rightCurbs)
	{
		ElementId pavementMaterial = MaterialId(RevitParameters.StreetPavementSurfaceParameter);
		double gutterWidth = TypeDistance(RevitParameters.StreetGutterWidthParameter, 0.0);
		double half = m_dWidth / 2.0 + gutterWidth;
		double depth = Math.Max(m_dSubgradeDepth, 0.04);
		bool hasJunction = m_IntersectionContainers != null && m_IntersectionContainers.Count > 0;
		Eps_Leg path = OverlayPath(!hasJunction);
		if (hasJunction && TrySweepIntersectionSection(path, half, depth, pavementMaterial, pavement, leftCurbs, rightCurbs))
			return true;
		Solid strip = SitePavementOverlay.CreateOffsetStrip(path, half, depth, pavementMaterial);
		if (strip == null) return false;
		pavement.Add(strip);
		TrySweepCurbSections(path, leftCurbs, rightCurbs, half);
		RuntimeDiagnostics.Write("Street continuous strip width=" + (half * 2.0) + " depth=" + depth + " ft; curbs=" + (leftCurbs.Count + rightCurbs.Count) + ".");
		return true;
	}

	private bool TrySweepIntersectionSection(Eps_Leg path, double half, double depth, ElementId pavementMaterial, List<Solid> pavement, List<Solid> leftCurbs, List<Solid> rightCurbs)
	{
		List<double[]> ranges = IntersectionRanges(path);
		if (ranges.Count == 0) return false;
		double length = RevitGeometry.eplGeoE_CalcLegListLength(path);
		List<Eps_Leg> approaches = new List<Eps_Leg>();
		double cursor = 0.0;
		for (int i = 0; i < ranges.Count; i++)
		{
			if (ranges[i][0] - cursor > 1.0)
			{
				Eps_Leg piece = SitePavementOverlay.ExtractBetweenStations(path, cursor, ranges[i][0]);
				if (piece != null) approaches.Add(piece);
			}
			cursor = ranges[i][1];
		}
		if (length - cursor > 1.0)
		{
			Eps_Leg tail = SitePavementOverlay.ExtractBetweenStations(path, cursor, length);
			if (tail != null) approaches.Add(tail);
		}
		foreach (Eps_Leg approach in approaches)
		{
			Solid strip = SitePavementOverlay.CreateOffsetStrip(approach, half, depth, pavementMaterial);
			if (strip != null) pavement.Add(strip);
			TrySweepCurbSections(approach, leftCurbs, rightCurbs, half);
		}
		int pads = 0;
		int returns = 0;
		foreach (IntersectionContainer container in m_IntersectionContainers)
		{
			if (container == null) continue;
			Solid pad = SitePavementOverlay.CreateIntersectionPad(container, depth, pavementMaterial);
			if (pad != null)
			{
				pavement.Add(pad);
				pads++;
			}
			returns += AddCurbReturns(container, leftCurbs, rightCurbs);
		}
		if (pavement.Count == 0) return false;
		RuntimeDiagnostics.Write("Street junction overlay approaches=" + approaches.Count + " pads=" + pads + " curb-returns=" + returns + ".");
		return true;
	}

	private int AddCurbReturns(IntersectionContainer container, List<Solid> leftCurbs, List<Solid> rightCurbs)
	{
		if (m_LeftCurbForm == null && m_RightCurbForm == null) return 0;
		ElementId curbMaterial = MaterialId(RevitParameters.StreetCurbMaterialParameter);
		double curbWidth = TypeDistance(RevitParameters.StreetCurbWidthParameter, 1.0);
		double curbHeight = TypeDistance(RevitParameters.StreetCurbHeightParameter, 0.5);
		int count = 0;
		if (m_LeftCurbForm != null && container.LeftOffsetLeg != null)
		{
			Solid left = SitePavementOverlay.CreateOffsetStrip(container.LeftOffsetLeg, curbWidth * 0.5, curbHeight, curbMaterial);
			if (left != null)
			{
				leftCurbs.Add(left);
				count++;
			}
		}
		if (m_RightCurbForm != null && container.RightOffsetLeg != null)
		{
			Solid right = SitePavementOverlay.CreateOffsetStrip(container.RightOffsetLeg, curbWidth * 0.5, curbHeight, curbMaterial);
			if (right != null)
			{
				rightCurbs.Add(right);
				count++;
			}
		}
		return count;
	}

	private List<double[]> IntersectionRanges(Eps_Leg path)
	{
		List<double[]> ranges = new List<double[]>();
		double length = path == null ? 0.0 : RevitGeometry.eplGeoE_CalcLegListLength(path);
		double sideGap = Math.Max(8.0, m_dWidth * 0.55);
		foreach (IntersectionContainer container in m_IntersectionContainers)
		{
			if (container == null) continue;
			double start = double.MaxValue;
			double end = double.MinValue;
			if (container.Profiles != null)
			{
				foreach (ProfileContainer profile in container.Profiles)
				{
					if (profile == null) continue;
					start = Math.Min(start, profile.Station);
					end = Math.Max(end, profile.Station);
				}
			}
			if (start == double.MaxValue)
			{
				if (container.StartingIntersection) { start = 0.0; end = sideGap; }
				else { start = Math.Max(0.0, length - sideGap); end = length; }
			}
			if (end - start < 2.0)
			{
				double mid = (start + end) * 0.5;
				start = mid - sideGap;
				end = mid + sideGap;
			}
			start = Math.Max(0.0, start - 1.0);
			end = Math.Min(length, end + 1.0);
			if (end > start) ranges.Add(new double[] { start, end });
		}
		ranges.Sort((a, b) => a[0].CompareTo(b[0]));
		List<double[]> merged = new List<double[]>();
		foreach (double[] range in ranges)
		{
			if (merged.Count == 0 || range[0] > merged[merged.Count - 1][1] + 0.5)
				merged.Add(range);
			else
				merged[merged.Count - 1][1] = Math.Max(merged[merged.Count - 1][1], range[1]);
		}
		return merged;
	}

	private void TrySweepCurbSections(Eps_Leg path, List<Solid> leftCurbs, List<Solid> rightCurbs, double pavementHalf)
	{
		if (m_LeftCurbForm == null && m_RightCurbForm == null) return;
		ElementId curbMaterial = MaterialId(RevitParameters.StreetCurbMaterialParameter);
		double curbWidth = TypeDistance(RevitParameters.StreetCurbWidthParameter, 1.0);
		double curbHeight = TypeDistance(RevitParameters.StreetCurbHeightParameter, 0.5);
		double outer = pavementHalf + curbWidth;
		if (m_LeftCurbForm != null)
		{
			Solid left = SitePavementOverlay.CreateOffsetBand(path, pavementHalf, outer, 1, curbHeight, curbMaterial);
			if (left != null) leftCurbs.Add(left);
		}
		if (m_RightCurbForm != null)
		{
			Solid right = SitePavementOverlay.CreateOffsetBand(path, pavementHalf, outer, 2, curbHeight, curbMaterial);
			if (right != null) rightCurbs.Add(right);
		}
	}

	private Eps_Leg OverlayPath()
	{
		return OverlayPath(true);
	}

	private Eps_Leg OverlayPath(bool filletCorners)
	{
		Eps_Leg source = CurrentLeg ?? m_MassLeg;
		if (source == null) return m_MassLeg;
		double distance = 0.0;
		double direction = 0.0;
		Eps_Point insert = m_LegInsertPoint ?? source.LegStartPnt;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(insert, new Eps_Point(0.0, 0.0, 0.0), ref distance);
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(insert, new Eps_Point(0.0, 0.0, 0.0), ref direction);
		Eps_Leg local = source.MoveLeg(direction, distance, 0.0 - insert.z);
		if (!filletCorners) return local;
		double gutter = TypeDistance(RevitParameters.StreetGutterWidthParameter, 0.0);
		double half = m_dWidth / 2.0 + gutter;
		// Tight host arcs (R < half-width) cannot be swept; flatten then re-fillet
		// so the offset strip stays a ring instead of aborting the command.
		if (local != null)
			local = local.FlattenArcsNarrowerThan(half + 2.0) ?? local;
		double fillet = Math.Max(half + 2.0, Math.Max(4.0, m_dWidth * 0.35));
		return SitePavementOverlay.FilletLineCorners(local, fillet) ?? local;
	}

	private double TypeDistance(string parameterName, double fallback)
	{
		Parameter parameter = StreetFamilySymbol?.LookupParameter(parameterName);
		if (parameter != null && parameter.AsDouble() > 0.0) return parameter.AsDouble();
		return fallback;
	}

	private ElementId MaterialId(string parameterName)
	{
		Parameter parameter = StreetFamilySymbol?.LookupParameter(parameterName);
		if (parameter == null) return ElementId.InvalidElementId;
		return RevitIds.Create(parameter.AsInteger());
	}

	private List<Solid> CreateBlends(ref List<Solid> _LeftCurbLoftFormList, ref List<Solid> _RightCurbLoftFormList)
	{
		List<Solid> list = new List<Solid>();
		m_ProgressBarForm = new ProgressBarForm();
		m_ProgressBarForm.ShowForm();
		m_ProgressBarForm.Refresh();
		m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingStreetMassMessage");
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = 0;
		List<List<CurveLoop>> _LeftCurbArrayArrayList = new List<List<CurveLoop>>();
		List<List<CurveLoop>> _RightCurbArrayArrayList = new List<List<CurveLoop>>();
		List<List<CurveLoop>> curveLoopArrayArray = new List<List<CurveLoop>>();
		Solid solid = null;
		string _strErrorMessage = string.Empty;
		bool loftFailed = false;
		curveLoopArrayArray = GetCurveLoopArrayArray(ref _LeftCurbArrayArrayList, ref _RightCurbArrayArrayList);
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = Math.Max(1, curveLoopArrayArray.Count + _LeftCurbArrayArrayList.Count + _RightCurbArrayArrayList.Count);
		for (int i = 0; !loftFailed && i < curveLoopArrayArray.Count; i++)
		{
			if (curveLoopArrayArray[i] != null && curveLoopArrayArray[i].Count > 0)
			{
				solid = NewStreetBlend(curveLoopArrayArray[i], i, ref _strErrorMessage);
				if (solid == null)
				{
					loftFailed = true;
					list.Clear();
					break;
				}
				list.Add(solid);
			}
			m_ProgressBarForm.UpdateProgressBar(1);
		}
		if (!loftFailed)
		{
			for (int j = 0; j < _LeftCurbArrayArrayList.Count; j++)
			{
				if (_LeftCurbArrayArrayList[j] != null && _LeftCurbArrayArrayList[j].Count > 0)
				{
					solid = NewCurbBlend(_LeftCurbArrayArrayList[j], j, _bIsLeftCurb: true, ref _strErrorMessage);
					if (solid == null)
					{
						loftFailed = true;
						list.Clear();
						_LeftCurbLoftFormList.Clear();
						break;
					}
					_LeftCurbLoftFormList.Add(solid);
				}
				m_ProgressBarForm.UpdateProgressBar(1);
			}
		}
		if (!loftFailed)
		{
			for (int k = 0; k < _RightCurbArrayArrayList.Count; k++)
			{
				if (_RightCurbArrayArrayList[k] != null && _RightCurbArrayArrayList[k].Count > 0)
				{
					solid = NewCurbBlend(_RightCurbArrayArrayList[k], k, _bIsLeftCurb: false, ref _strErrorMessage);
					if (solid == null)
					{
						loftFailed = true;
						list.Clear();
						_LeftCurbLoftFormList.Clear();
						_RightCurbLoftFormList.Clear();
						break;
					}
					_RightCurbLoftFormList.Add(solid);
				}
				m_ProgressBarForm.UpdateProgressBar(1);
			}
		}
		if (!loftFailed && list.Count > 0)
		{
			RuntimeDiagnostics.Write("Street 2020 loft created " + list.Count + " pavement solid(s); curbs=" + (_LeftCurbLoftFormList.Count + _RightCurbLoftFormList.Count) + ".");
			m_ProgressBarForm.SetMaxValue();
			Thread.Sleep(150);
			m_ProgressBarForm.Close();
			return list;
		}
		list.Clear();
		_LeftCurbLoftFormList.Clear();
		_RightCurbLoftFormList.Clear();
		if (m_IntersectionContainers != null && m_IntersectionContainers.Count > 0 && TrySweepTypeSection(list, _LeftCurbLoftFormList, _RightCurbLoftFormList))
		{
			m_ProgressBarForm.SetMaxValue();
			Thread.Sleep(150);
			m_ProgressBarForm.Close();
			return list;
		}
		ElementId pavementMaterial = ElementId.InvalidElementId;
		Parameter pavementParameter = StreetFamilySymbol?.LookupParameter(RevitParameters.StreetPavementSurfaceParameter);
		if (pavementParameter != null)
			pavementMaterial = RevitIds.Create(pavementParameter.AsInteger());
		List<Solid> swept = SitePavementOverlay.SweepRibbon(OverlayPath(), m_dWidth, m_dSubgradeDepth, pavementMaterial);
		if (swept != null && swept.Count > 0)
		{
			RuntimeDiagnostics.Write("Street ribbon sweep created " + swept.Count + " solid(s); width=" + m_dWidth + " depth=" + m_dSubgradeDepth + " ft.");
			list.AddRange(swept);
			TrySweepCurbSections(OverlayPath(), _LeftCurbLoftFormList, _RightCurbLoftFormList, m_dWidth / 2.0);
			m_ProgressBarForm.SetMaxValue();
			Thread.Sleep(150);
			m_ProgressBarForm.Close();
			return list;
		}
		if (TrySweepTypeSection(list, _LeftCurbLoftFormList, _RightCurbLoftFormList))
		{
			m_ProgressBarForm.SetMaxValue();
			Thread.Sleep(150);
			m_ProgressBarForm.Close();
			return list;
		}
		if (list.Count == 0 && string.Equals(_strErrorMessage, EPShared.LocalRM.GetString("StreetTemplateNotMassTypeMessage"), StringComparison.OrdinalIgnoreCase))
			RevitMessageBox.Show(_strErrorMessage, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		else if (list.Count == 0)
			RuntimeDiagnostics.Write("Street overlay produced no solid after loft/ribbon/strip fallbacks.");
		m_ProgressBarForm.SetMaxValue();
		Thread.Sleep(150);
		m_ProgressBarForm.Close();
		return list;
	}

	private Solid NewStreetBlend(List<CurveLoop> streetCurveLoopArray, int _nProcessLegSegment, ref string _strErrorMessage)
	{
		Solid result = null;
		try
		{
			SolidOptions solidOptions = null;
			Parameter parameter = StreetFamilySymbol.LookupParameter(RevitParameters.StreetPavementSurfaceParameter);
			if (parameter != null)
			{
				int num = 0;
				num = parameter.AsInteger();
				solidOptions = new SolidOptions(RevitIds.Create(num), ElementId.InvalidElementId);
			}
			result = GeometryCreationUtilities.CreateLoftGeometry(streetCurveLoopArray, solidOptions);
			m_dOffsetAdjust = 0.01;
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("CreateLoftGeometry failed: " + ex.Message);
			if (string.Compare(ex.Message, "The attempted operation is not permitted in this type of family.", ignoreCase: true) == 0)
			{
				_strErrorMessage = EPShared.LocalRM.GetString("StreetTemplateNotMassTypeMessage");
				return null;
			}
			if (RevitDefines.LT(m_dOffsetAdjust, 0.03))
			{
				m_dOffsetAdjust += 0.005;
				List<List<CurveLoop>> _LeftCurbArrayArrayList = new List<List<CurveLoop>>();
				List<List<CurveLoop>> _RightCurbArrayArrayList = new List<List<CurveLoop>>();
				List<List<CurveLoop>> curveLoopArrayArray = GetCurveLoopArrayArray(ref _LeftCurbArrayArrayList, ref _RightCurbArrayArrayList, _nProcessLegSegment);
				if (curveLoopArrayArray.Count > 0)
				{
					result = NewStreetBlend(curveLoopArrayArray[0], _nProcessLegSegment, ref _strErrorMessage);
				}
			}
			else
			{
				_strErrorMessage = EPShared.LocalRM.GetString("UnableToCeateBlendGeometry");
			}
		}
		return result;
	}

	private Autodesk.Revit.DB.Form NewStreetLoftForm(ReferenceArrayArray _StreetReferenceArrayArray, int _nProcessLegSegment, ref string _strErrorMessage)
	{
		Autodesk.Revit.DB.Form result = null;
		try
		{
			result = m_MassDocument.FamilyCreate.NewLoftForm(isSolid: true, _StreetReferenceArrayArray);
			m_dOffsetAdjust = 0.01;
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
		{
			if (string.Compare(ex.Message, "The attempted operation is not permitted in this type of family.", ignoreCase: true) == 0)
			{
				_strErrorMessage = EPShared.LocalRM.GetString("StreetTemplateNotMassTypeMessage");
				return null;
			}
			if (RevitDefines.LT(m_dOffsetAdjust, 0.03))
			{
				m_dOffsetAdjust += 0.005;
				List<ReferenceArrayArray> _LeftCurbArrayArrayList = new List<ReferenceArrayArray>();
				List<ReferenceArrayArray> _RightCurbArrayArrayList = new List<ReferenceArrayArray>();
				List<ReferenceArrayArray> referenceArrayArray = GetReferenceArrayArray(ref _LeftCurbArrayArrayList, ref _RightCurbArrayArrayList, _nProcessLegSegment);
				if (referenceArrayArray.Count > 0)
				{
					result = NewStreetLoftForm(referenceArrayArray[0], _nProcessLegSegment, ref _strErrorMessage);
				}
			}
			else
			{
				_strErrorMessage = EPShared.LocalRM.GetString("UnableToCreateMassMessage");
			}
		}
		return result;
	}

	private Autodesk.Revit.DB.Form NewCurbLoftForm(ReferenceArrayArray _CurbReferenceArrayArray, int _nProcessLegSegment, bool _bIsLeftCurb, ref string _strErrorMessage)
	{
		Autodesk.Revit.DB.Form result = null;
		try
		{
			result = m_AdditionalMassDocument.FamilyCreate.NewLoftForm(isSolid: true, _CurbReferenceArrayArray);
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
				List<ReferenceArrayArray> _LeftCurbArrayArrayList = new List<ReferenceArrayArray>();
				List<ReferenceArrayArray> _RightCurbArrayArrayList = new List<ReferenceArrayArray>();
				GetReferenceArrayArray(ref _LeftCurbArrayArrayList, ref _RightCurbArrayArrayList, _nProcessLegSegment);
				if (_bIsLeftCurb)
				{
					if (_LeftCurbArrayArrayList.Count > 0)
					{
						result = NewStreetLoftForm(_LeftCurbArrayArrayList[0], _nProcessLegSegment, ref _strErrorMessage);
					}
				}
				else if (_RightCurbArrayArrayList.Count > 0)
				{
					result = NewStreetLoftForm(_RightCurbArrayArrayList[0], _nProcessLegSegment, ref _strErrorMessage);
				}
			}
			else
			{
				_strErrorMessage = EPShared.LocalRM.GetString("UnableToCreateMassMessage");
			}
		}
		return result;
	}

	private Solid NewCurbBlend(List<CurveLoop> curbCurveLoopArray, int _nProcessLegSegment, bool _bIsLeftCurb, ref string _strErrorMessage)
	{
		Solid result = null;
		try
		{
			SolidOptions solidOptions = null;
			Parameter parameter = StreetFamilySymbol.LookupParameter(RevitParameters.StreetCurbMaterialParameter);
			if (parameter != null)
			{
				int num = 0;
				num = parameter.AsInteger();
				solidOptions = new SolidOptions(RevitIds.Create(num), ElementId.InvalidElementId);
			}
			result = GeometryCreationUtilities.CreateLoftGeometry(curbCurveLoopArray, solidOptions);
			m_dOffsetAdjust = 0.01;
		}
		catch (Exception ex)
		{
			if (string.Compare(ex.Message, "The attempted operation is not permitted in this type of family.", ignoreCase: true) == 0)
			{
				_strErrorMessage = EPShared.LocalRM.GetString("CurbTemplateNotMassTypeMessage");
				return null;
			}
			if (RevitDefines.LT(m_dOffsetAdjust, 0.03))
			{
				m_dOffsetAdjust += 0.005;
				List<List<CurveLoop>> _LeftCurbArrayArrayList = new List<List<CurveLoop>>();
				List<List<CurveLoop>> _RightCurbArrayArrayList = new List<List<CurveLoop>>();
				GetCurveLoopArrayArray(ref _LeftCurbArrayArrayList, ref _RightCurbArrayArrayList, _nProcessLegSegment);
				if (_bIsLeftCurb)
				{
					if (_LeftCurbArrayArrayList.Count > 0)
					{
						result = NewStreetBlend(_LeftCurbArrayArrayList[0], _nProcessLegSegment, ref _strErrorMessage);
					}
				}
				else if (_RightCurbArrayArrayList.Count > 0)
				{
					result = NewStreetBlend(_RightCurbArrayArrayList[0], _nProcessLegSegment, ref _strErrorMessage);
				}
			}
			else
			{
				_strErrorMessage = EPShared.LocalRM.GetString("UnableToCeateBlendGeometry");
			}
		}
		return result;
	}

	public override string CreateElement(Document doc, ref string _strAdditionalLoftID, string _strSiteComponentName, double _dStartingOffset, double _dStartingElevation, bool _bUseInternalPoint)
	{
		List<Solid> _LeftCurbLoftFormList = new List<Solid>();
		List<Solid> _RightCurbLoftFormList = new List<Solid>();
		List<Solid> list = CreateBlends(ref _LeftCurbLoftFormList, ref _RightCurbLoftFormList);
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
		list2.Clear();
		foreach (Solid item2 in _LeftCurbLoftFormList)
		{
			list2.Add(item2);
		}
		foreach (Solid item3 in _RightCurbLoftFormList)
		{
			list2.Add(item3);
		}
		if (list2.Count != 0)
		{
			DirectShape directShape2 = DirectShape.CreateElement(doc, RevitIds.Create(BuiltInCategory.OST_Roads));
			directShape2.ApplicationId = gUID.ToString();
			directShape2.SetShape(list2);
			_strAdditionalLoftID = directShape2.UniqueId;
			ElementTransformUtils.MoveElement(doc, directShape2.Id, m_LegInsertPoint.XYZ);
			directShape2.Name = _strSiteComponentName + " " + EPShared.LocalRM.GetString("StreetCurbDisplayParameter");
		}
		return uniqueId;
	}

	public List<Autodesk.Revit.DB.Form> CreateLoftFormWithParameters()
	{
		List<Autodesk.Revit.DB.Form> _LeftCurbLoftFormList = new List<Autodesk.Revit.DB.Form>();
		List<Autodesk.Revit.DB.Form> _RightCurbLoftFormList = new List<Autodesk.Revit.DB.Form>();
		List<Autodesk.Revit.DB.Form> list = CreateLoftForm(ref _LeftCurbLoftFormList, ref _RightCurbLoftFormList);
		if (list == null || list.Count == 0)
		{
			return list;
		}
		string _strReturnValue = string.Empty;
		string _strReturnValue2 = string.Empty;
		Category category = null;
		Category category2 = null;
		Category familyCategory = m_MassDocument.OwnerFamily.FamilyCategory;
		if (familyCategory != null && familyCategory.SubCategories != null && familyCategory.SubCategories.Size > 0)
		{
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("StreetMassSubCategoryName", ref _strReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("StreetMassSubCatergoryName", ref _strReturnValue))
			{
				_strReturnValue = EPShared.LocalRM.GetString("SWStreetLineNameString");
				ApplicationUtility.Settings.eplSetE_SetSetting("StreetMassSubCategoryName", _strReturnValue);
			}
			if (familyCategory.SubCategories.Contains(_strReturnValue))
			{
				category = familyCategory.SubCategories.get_Item(_strReturnValue);
			}
		}
		Category category3 = null;
		if (m_AdditionalMassDocument != null)
		{
			category3 = m_AdditionalMassDocument.OwnerFamily.FamilyCategory;
		}
		if (category3 != null && category3.SubCategories != null && category3.SubCategories.Size > 0)
		{
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("CurbMassSubCategoryName", ref _strReturnValue2) && !ApplicationUtility.Settings.eplSetE_GetSetting("CurbMassSubCatergoryName", ref _strReturnValue2))
			{
				_strReturnValue2 = EPShared.LocalRM.GetString("SWCurbLineNameString");
				ApplicationUtility.Settings.eplSetE_SetSetting("CurbMassSubCategoryName", _strReturnValue2);
			}
			if (category3.SubCategories.Contains(_strReturnValue2))
			{
				category2 = category3.SubCategories.get_Item(_strReturnValue2);
			}
		}
		FamilyParameter familyParameter = null;
		if (list != null && list.Count > 0)
		{
			if (category == null)
			{
				category = m_MassDocument.Settings.Categories.NewSubcategory(familyCategory, _strReturnValue);
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (familyParameter == null)
				{
					familyParameter = m_MassDocument.FamilyManager.AddParameter(RevitParameters.StreetPavementSurfaceParameter, BuiltInParameterGroup.PG_MATERIALS, ParameterType.Material, isInstance: true);
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
		FamilyParameter familyParameter2 = null;
		if (_LeftCurbLoftFormList != null && _LeftCurbLoftFormList.Count > 0)
		{
			if (m_AdditionalMassDocument != null && category2 == null)
			{
				category2 = m_AdditionalMassDocument.Settings.Categories.NewSubcategory(familyCategory, _strReturnValue2);
			}
			for (int j = 0; j < _LeftCurbLoftFormList.Count; j++)
			{
				if (familyParameter2 == null)
				{
					familyParameter2 = m_AdditionalMassDocument.FamilyManager.AddParameter(RevitParameters.StreetCurbMaterialParameter, BuiltInParameterGroup.PG_MATERIALS, ParameterType.Material, isInstance: true);
				}
				try
				{
					Parameter parameter2 = ((Element)_LeftCurbLoftFormList[j]).get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
					if (parameter2 != null)
					{
						m_AdditionalMassDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter2, familyParameter2);
					}
					if (category2 != null)
					{
						_LeftCurbLoftFormList[j].Subcategory = category2;
					}
				}
				catch (Autodesk.Revit.Exceptions.InvalidOperationException)
				{
				}
			}
		}
		if (_RightCurbLoftFormList != null && _RightCurbLoftFormList.Count > 0)
		{
			if (m_AdditionalMassDocument != null && category2 == null)
			{
				category2 = m_AdditionalMassDocument.Settings.Categories.NewSubcategory(familyCategory, _strReturnValue2);
			}
			for (int k = 0; k < _RightCurbLoftFormList.Count; k++)
			{
				if (familyParameter2 == null)
				{
					familyParameter2 = m_AdditionalMassDocument.FamilyManager.AddParameter(RevitParameters.StreetCurbMaterialParameter, BuiltInParameterGroup.PG_MATERIALS, ParameterType.Material, isInstance: true);
				}
				try
				{
					Parameter parameter3 = ((Element)_RightCurbLoftFormList[k]).get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
					if (parameter3 != null)
					{
						m_AdditionalMassDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter3, familyParameter2);
					}
					if (category2 != null)
					{
						_RightCurbLoftFormList[k].Subcategory = category2;
					}
				}
				catch (Autodesk.Revit.Exceptions.InvalidOperationException)
				{
				}
			}
		}
		return list;
	}

	private List<ReferenceArrayArray> GetReferenceArrayArray(ref List<ReferenceArrayArray> _LeftCurbArrayArrayList, ref List<ReferenceArrayArray> _RightCurbArrayArrayList)
	{
		return GetReferenceArrayArray(ref _LeftCurbArrayArrayList, ref _RightCurbArrayArrayList, -1);
	}

	private List<ReferenceArrayArray> GetReferenceArrayArray(ref List<ReferenceArrayArray> _LeftCurbArrayArrayList, ref List<ReferenceArrayArray> _RightCurbArrayArrayList, int _nProcessLegSegment)
	{
		List<ReferenceArrayArray> list = new List<ReferenceArrayArray>();
		List<ReferenceArrayArray> list2 = null;
		List<ReferenceArrayArray> list3 = null;
		ReferenceArrayArray referenceArrayArray = null;
		ReferenceArrayArray referenceArrayArray2 = null;
		ReferenceArrayArray referenceArrayArray3 = null;
		ReferenceArray referenceArray = null;
		ReferenceArray _LeftCurbReferenceArray = null;
		ReferenceArray _RightCurbReferenceArray = null;
		Eps_Leg eps_Leg = null;
		double num = 0.0;
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point eps_Point2 = new Eps_Point();
		Eps_Point Pnt = new Eps_Point();
		Eps_Point eps_Point3 = null;
		Eps_Point eps_Point4 = null;
		double num2 = 0.0;
		double dOffsetAdjust = m_dOffsetAdjust;
		double _dDirection = 0.0;
		double num3 = 0.0;
		double _dDirection2 = 0.0;
		double dAdjustDistance = 0.0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		bool flag = false;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		bool flag2 = true;
		bool flag3 = true;
		bool flag4 = true;
		if (m_MassLeg != null)
		{
			if (m_IntersectionContainers != null && m_IntersectionContainers.Count > 0)
			{
				if (m_IntersectionContainers[0].ProfileUsage == ProfileUsage.EntireProfile)
				{
					flag2 = m_IntersectionContainers[0].StartingIntersection;
				}
				else if (m_IntersectionContainers[0].ProfileUsage == ProfileUsage.LeftOnly)
				{
					flag3 = m_IntersectionContainers[0].StartingIntersection;
					if (m_IntersectionContainers.Count > 1 && m_IntersectionContainers[1].ProfileUsage == ProfileUsage.RightOnly)
					{
						flag4 = m_IntersectionContainers[1].StartingIntersection;
					}
				}
				else if (m_IntersectionContainers[0].ProfileUsage == ProfileUsage.RightOnly)
				{
					flag4 = m_IntersectionContainers[0].StartingIntersection;
					if (m_IntersectionContainers.Count > 1 && m_IntersectionContainers[1].ProfileUsage == ProfileUsage.LeftOnly)
					{
						flag3 = m_IntersectionContainers[1].StartingIntersection;
					}
				}
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
					if (num4 == 0 || flag3)
					{
						referenceArrayArray2 = new ReferenceArrayArray();
					}
					if (num4 == 0 || flag4)
					{
						referenceArrayArray3 = new ReferenceArrayArray();
					}
				}
				num = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg);
				if (eps_Leg.Arc != null)
				{
					Arc arc = eps_Leg.Arc.RevitArc(m_MassDocument);
					double _dStation2 = 0.0;
					double _dOffset2 = 0.0;
					int num7 = 0;
					int num8 = 0;
					List<XYZ> list4 = arc.Tessellate().ToList();
					num8 = list4.Count;
					foreach (XYZ item in list4)
					{
						Eps_Point point = new Eps_Point(item);
						if (num7 % 5 != 0 && num7 != 1 && num7 != num8 - 1)
						{
							num7++;
							continue;
						}
						EPShared.eplGeoE_CalcStationOffsetAlongArc(point, eps_Leg.Arc, ref _dStation2, ref _dOffset2);
						if (num4 > 0 && num7 == 0)
						{
							_dStation2 += dOffsetAdjust;
						}
						if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null && num7 == num8 - 1)
						{
							_dStation2 -= dOffsetAdjust;
						}
						if (num7 == num8 - 1 && eps_Leg.Next != null && eps_Leg.Next.Line != null)
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
									referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + _dStation2, _bUseDirection: true, num3, dAdjustDistance, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
								}
								else
								{
									referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + _dStation2, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
								}
							}
							else
							{
								referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + _dStation2, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
							}
						}
						else if (flag)
						{
							referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
							flag = false;
						}
						else
						{
							referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + _dStation2, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
						}
						if (referenceArray != null && referenceArray.Size > 0)
						{
							referenceArrayArray.Append(referenceArray);
							referenceArray = null;
						}
						if (_LeftCurbReferenceArray != null && _LeftCurbReferenceArray.Size > 0)
						{
							referenceArrayArray2.Append(_LeftCurbReferenceArray);
							_LeftCurbReferenceArray = null;
						}
						if (_RightCurbReferenceArray != null && _RightCurbReferenceArray.Size > 0)
						{
							referenceArrayArray3.Append(_RightCurbReferenceArray);
							_RightCurbReferenceArray = null;
						}
						num7++;
					}
					num2 += num;
				}
				else
				{
					if (flag2)
					{
						if (num4 == 0)
						{
							referenceArray = FindReferenceArrayProfileAtStation(num4, num2, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
						}
						else if (flag)
						{
							referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
							flag = false;
						}
						else
						{
							referenceArray = FindReferenceArrayProfileAtStation(num4, num2 + dOffsetAdjust, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
						}
						if (referenceArray != null && referenceArray.Size > 0)
						{
							referenceArrayArray.Append(referenceArray);
							referenceArray = null;
						}
						if (flag3 && _LeftCurbReferenceArray != null && _LeftCurbReferenceArray.Size > 0)
						{
							referenceArrayArray2.Append(_LeftCurbReferenceArray);
							_LeftCurbReferenceArray = null;
						}
						if (flag4 && _RightCurbReferenceArray != null && _RightCurbReferenceArray.Size > 0)
						{
							referenceArrayArray3.Append(_RightCurbReferenceArray);
							_RightCurbReferenceArray = null;
						}
					}
					if (m_IntersectionContainers != null)
					{
						while (num5 < m_IntersectionContainers.Count && num6 < m_IntersectionContainers[num5].Profiles.Count && m_IntersectionContainers[num5].Profiles[num6].Station <= num2 + num)
						{
							if (m_IntersectionContainers[num5].Profiles[num6].Station >= num2)
							{
								if (m_IntersectionContainers[num5].ProfileUsage == ProfileUsage.EntireProfile)
								{
									if (flag2 && m_IntersectionContainers[num5].StartingIntersection && num6 == 0 && m_IntersectionContainers[num5].Profiles[num6].Points.Count == 6)
									{
										eps_Point3 = m_IntersectionContainers[num5].Profiles[num6].Points[0];
										eps_Point4 = m_IntersectionContainers[num5].Profiles[num6].Points[4];
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
										if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point3, m_MassLeg, ref _dStation, ref _dOffset) == 1700 && _dStation > 0.0 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation - dOffsetAdjust, _dOffset, ref Pnt) == 0)
										{
											Pnt.z -= m_dWidth / 2.0 * m_dSlope;
											_LeftCurbReferenceArray = FindCurbReferenceArrayProfileAtStation(_bIsLeftCurb: true, _dStation - dOffsetAdjust, eps_Point3.z - Pnt.z);
											if (_LeftCurbReferenceArray != null && _LeftCurbReferenceArray.Size > 0)
											{
												referenceArrayArray2.Append(_LeftCurbReferenceArray);
												_LeftCurbReferenceArray = null;
											}
											if (referenceArrayArray2 != null && referenceArrayArray2.Size > 0)
											{
												_LeftCurbArrayArrayList.Add(referenceArrayArray2);
												referenceArrayArray2 = new ReferenceArrayArray();
											}
										}
										if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point4, m_MassLeg, ref _dStation, ref _dOffset) == 1700 && _dStation > 0.0 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation - dOffsetAdjust, _dOffset, ref Pnt) == 0)
										{
											Pnt.z -= m_dWidth / 2.0 * m_dSlope;
											_RightCurbReferenceArray = FindCurbReferenceArrayProfileAtStation(_bIsLeftCurb: false, _dStation - dOffsetAdjust, eps_Point4.z - Pnt.z);
											if (_RightCurbReferenceArray != null && _RightCurbReferenceArray.Size > 0)
											{
												referenceArrayArray3.Append(_RightCurbReferenceArray);
												_RightCurbReferenceArray = null;
											}
											if (referenceArrayArray3 != null && referenceArrayArray3.Size > 0)
											{
												_RightCurbArrayArrayList.Add(referenceArrayArray3);
												referenceArrayArray3 = new ReferenceArrayArray();
											}
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
											referenceArrayArray2.Clear();
											list2 = GetCurbReferenceArrayArray(_bIsLeftCurb: true, m_IntersectionContainers[num5].LeftOffsetLeg);
											if (list2 != null && list2.Count > 0)
											{
												_LeftCurbArrayArrayList.AddRange(list2);
											}
											referenceArrayArray3.Clear();
											list3 = GetCurbReferenceArrayArray(_bIsLeftCurb: false, m_IntersectionContainers[num5].RightOffsetLeg);
											if (list3 != null && list3.Count > 0)
											{
												_RightCurbArrayArrayList.AddRange(list3);
											}
										}
										else if (num6 == m_IntersectionContainers[num5].Profiles.Count - 1 && referenceArrayArray != null && referenceArrayArray.Size > 0)
										{
											list.Add(referenceArrayArray);
											referenceArrayArray = new ReferenceArrayArray();
										}
									}
									else if (num6 == 0)
									{
										list2 = GetCurbReferenceArrayArray(_bIsLeftCurb: true, m_IntersectionContainers[num5].LeftOffsetLeg);
										if (list2 != null && list2.Count > 0)
										{
											_LeftCurbArrayArrayList.AddRange(list2);
										}
										list3 = GetCurbReferenceArrayArray(_bIsLeftCurb: false, m_IntersectionContainers[num5].RightOffsetLeg);
										if (list3 != null && list3.Count > 0)
										{
											_RightCurbArrayArrayList.AddRange(list3);
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
										if (m_IntersectionContainers[num5].Profiles[num6].Station + dOffsetAdjust < num2 + num && m_IntersectionContainers[num5].Profiles[num6].Points.Count == 6)
										{
											eps_Point3 = m_IntersectionContainers[num5].Profiles[num6].Points[0];
											eps_Point4 = m_IntersectionContainers[num5].Profiles[num6].Points[4];
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
											if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point3, m_MassLeg, ref _dStation, ref _dOffset) == 1700 && _dStation > 0.0 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation - dOffsetAdjust, _dOffset, ref Pnt) == 0)
											{
												Pnt.z -= m_dWidth / 2.0 * m_dSlope;
												_LeftCurbReferenceArray = FindCurbReferenceArrayProfileAtStation(_bIsLeftCurb: true, _dStation + dOffsetAdjust, eps_Point3.z - Pnt.z);
												if (_LeftCurbReferenceArray != null && _LeftCurbReferenceArray.Size > 0)
												{
													referenceArrayArray2.Append(_LeftCurbReferenceArray);
													_LeftCurbReferenceArray = null;
												}
											}
											if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point4, m_MassLeg, ref _dStation, ref _dOffset) == 1700 && _dStation > 0.0 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation - dOffsetAdjust, _dOffset, ref Pnt) == 0)
											{
												Pnt.z -= m_dWidth / 2.0 * m_dSlope;
												_RightCurbReferenceArray = FindCurbReferenceArrayProfileAtStation(_bIsLeftCurb: false, _dStation + dOffsetAdjust, eps_Point4.z - Pnt.z);
												if (_RightCurbReferenceArray != null && _RightCurbReferenceArray.Size > 0)
												{
													referenceArrayArray3.Append(_RightCurbReferenceArray);
													_RightCurbReferenceArray = null;
												}
											}
										}
									}
								}
								else if (m_IntersectionContainers[num5].ProfileUsage == ProfileUsage.LeftOnly)
								{
									_LeftCurbReferenceArray = FindCurbReferenceArrayProfileAtStation(_bIsLeftCurb: true, m_IntersectionContainers[num5].Profiles[num6].Station);
									if (_LeftCurbReferenceArray != null && _LeftCurbReferenceArray.Size > 0)
									{
										referenceArrayArray2.Append(_LeftCurbReferenceArray);
										_LeftCurbReferenceArray = null;
									}
									if (m_IntersectionContainers[num5].StartingIntersection)
									{
										if (num6 == 0)
										{
											flag3 = false;
											if (referenceArrayArray2 != null && referenceArrayArray2.Size > 0)
											{
												_LeftCurbArrayArrayList.Add(referenceArrayArray2);
												referenceArrayArray2 = new ReferenceArrayArray();
											}
										}
									}
									else if (num6 == m_IntersectionContainers[num5].Profiles.Count - 1)
									{
										flag3 = true;
									}
								}
								else if (m_IntersectionContainers[num5].ProfileUsage == ProfileUsage.RightOnly)
								{
									_RightCurbReferenceArray = FindCurbReferenceArrayProfileAtStation(_bIsLeftCurb: false, m_IntersectionContainers[num5].Profiles[num6].Station);
									if (_RightCurbReferenceArray != null && _RightCurbReferenceArray.Size > 0)
									{
										referenceArrayArray3.Append(_RightCurbReferenceArray);
										_RightCurbReferenceArray = null;
									}
									if (m_IntersectionContainers[num5].StartingIntersection)
									{
										if (num6 == 0)
										{
											flag4 = false;
											if (referenceArrayArray3 != null && referenceArrayArray3.Size > 0)
											{
												_RightCurbArrayArrayList.Add(referenceArrayArray3);
												referenceArrayArray3 = new ReferenceArrayArray();
											}
										}
									}
									else if (num6 == m_IntersectionContainers[num5].Profiles.Count - 1)
									{
										flag4 = true;
									}
								}
							}
							num6++;
							if (num6 >= m_IntersectionContainers[num5].Profiles.Count)
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
									referenceArray = FindReferenceArrayProfileAtStation(num4, num2, _bUseDirection: true, num3, dAdjustDistance, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
								}
								else
								{
									referenceArray = FindReferenceArrayProfileAtStation(num4, num2, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
								}
							}
							else
							{
								referenceArray = FindReferenceArrayProfileAtStation(num4, num2, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
							}
						}
						else
						{
							referenceArray = FindReferenceArrayProfileAtStation(num4, num2, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
						}
						if (referenceArray != null && referenceArray.Size > 0)
						{
							referenceArrayArray.Append(referenceArray);
							referenceArray = null;
						}
						if (flag3 && _LeftCurbReferenceArray != null && _LeftCurbReferenceArray.Size > 0)
						{
							referenceArrayArray2.Append(_LeftCurbReferenceArray);
							_LeftCurbReferenceArray = null;
						}
						if (flag4 && _RightCurbReferenceArray != null && _RightCurbReferenceArray.Size > 0)
						{
							referenceArrayArray3.Append(_RightCurbReferenceArray);
							_RightCurbReferenceArray = null;
						}
					}
				}
				if (flag2 && referenceArrayArray != null && referenceArrayArray.Size > 0)
				{
					list.Add(referenceArrayArray);
					if (flag3 && referenceArrayArray2 != null && referenceArrayArray2.Size > 0)
					{
						_LeftCurbArrayArrayList.Add(referenceArrayArray2);
					}
					if (flag4 && referenceArrayArray3 != null && referenceArrayArray3.Size > 0)
					{
						_RightCurbArrayArrayList.Add(referenceArrayArray3);
					}
				}
				num4++;
			}
		}
		return list;
	}

	private List<List<CurveLoop>> GetCurveLoopArrayArray(ref List<List<CurveLoop>> _LeftCurbArrayArrayList, ref List<List<CurveLoop>> _RightCurbArrayArrayList)
	{
		return GetCurveLoopArrayArray(ref _LeftCurbArrayArrayList, ref _RightCurbArrayArrayList, -1);
	}

	private List<List<CurveLoop>> GetCurveLoopArrayArray(ref List<List<CurveLoop>> _LeftCurbArrayArrayList, ref List<List<CurveLoop>> _RightCurbArrayArrayList, int _nProcessLegSegment)
	{
		List<List<CurveLoop>> list = new List<List<CurveLoop>>();
		List<List<CurveLoop>> list2 = null;
		List<List<CurveLoop>> list3 = null;
		List<CurveLoop> list4 = null;
		List<CurveLoop> list5 = null;
		List<CurveLoop> list6 = null;
		CurveLoop curveLoop = null;
		CurveLoop _LeftCurbCurveLoop = null;
		CurveLoop _RightCurbCurveLoop = null;
		Eps_Leg eps_Leg = null;
		double num = 0.0;
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point eps_Point2 = new Eps_Point();
		Eps_Point Pnt = new Eps_Point();
		Eps_Point eps_Point3 = null;
		Eps_Point eps_Point4 = null;
		double num2 = 0.0;
		double dOffsetAdjust = m_dOffsetAdjust;
		double _dDirection = 0.0;
		double num3 = 0.0;
		double _dDirection2 = 0.0;
		double dAdjustDistance = 0.0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		bool flag = false;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		bool flag2 = true;
		bool flag3 = true;
		bool flag4 = true;
		if (m_MassLeg != null)
		{
			if (m_IntersectionContainers != null && m_IntersectionContainers.Count > 0)
			{
				if (m_IntersectionContainers[0].ProfileUsage == ProfileUsage.EntireProfile)
				{
					flag2 = m_IntersectionContainers[0].StartingIntersection;
				}
				else if (m_IntersectionContainers[0].ProfileUsage == ProfileUsage.LeftOnly)
				{
					flag3 = m_IntersectionContainers[0].StartingIntersection;
					if (m_IntersectionContainers.Count > 1 && m_IntersectionContainers[1].ProfileUsage == ProfileUsage.RightOnly)
					{
						flag4 = m_IntersectionContainers[1].StartingIntersection;
					}
				}
				else if (m_IntersectionContainers[0].ProfileUsage == ProfileUsage.RightOnly)
				{
					flag4 = m_IntersectionContainers[0].StartingIntersection;
					if (m_IntersectionContainers.Count > 1 && m_IntersectionContainers[1].ProfileUsage == ProfileUsage.LeftOnly)
					{
						flag3 = m_IntersectionContainers[1].StartingIntersection;
					}
				}
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
				if (num4 == 0 || num4 == _nProcessLegSegment || flag2)
				{
					list4 = new List<CurveLoop>();
					if (num4 == 0 || num4 == _nProcessLegSegment || flag3)
					{
						list5 = new List<CurveLoop>();
					}
					if (num4 == 0 || num4 == _nProcessLegSegment || flag4)
					{
						list6 = new List<CurveLoop>();
					}
				}
				num = RevitGeometry.eplGeoE_CalcLegLength(eps_Leg);
				if (eps_Leg.Arc != null)
				{
					Arc arc = eps_Leg.Arc.RevitArc(m_MassDocument);
					double _dStation2 = 0.0;
					double _dOffset2 = 0.0;
					int num7 = 0;
					int num8 = 0;
					List<XYZ> list7 = arc.Tessellate().ToList();
					num8 = list7.Count;
					foreach (XYZ item in list7)
					{
						Eps_Point point = new Eps_Point(item);
						if (num7 % 5 != 0 && num7 != 1 && num7 != num8 - 1)
						{
							num7++;
							continue;
						}
						EPShared.eplGeoE_CalcStationOffsetAlongArc(point, eps_Leg.Arc, ref _dStation2, ref _dOffset2);
						if (num4 > 0 && num7 == 0)
						{
							_dStation2 += dOffsetAdjust;
						}
						if (m_MassLeg.IsLegClosed() && eps_Leg.Next == null && num7 == num8 - 1)
						{
							_dStation2 -= dOffsetAdjust;
						}
						if (num7 == num8 - 1 && eps_Leg.Next != null && eps_Leg.Next.Line != null)
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
									curveLoop = FindCurveLoopProfileAtStation(num4, num2 + _dStation2, _bUseDirection: true, num3, dAdjustDistance, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
								}
								else
								{
									curveLoop = FindCurveLoopProfileAtStation(num4, num2 + _dStation2, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
								}
							}
							else
							{
								curveLoop = FindCurveLoopProfileAtStation(num4, num2 + _dStation2, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
							}
						}
						else if (flag)
						{
							curveLoop = FindCurveLoopProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
							flag = false;
						}
						else
						{
							curveLoop = FindCurveLoopProfileAtStation(num4, num2 + _dStation2, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
						}
						if (curveLoop != null)
						{
							list4.Add(curveLoop);
							curveLoop = null;
						}
						if (_LeftCurbCurveLoop != null)
						{
							list5.Add(_LeftCurbCurveLoop);
							_LeftCurbCurveLoop = null;
						}
						if (_RightCurbCurveLoop != null)
						{
							list6.Add(_RightCurbCurveLoop);
							_RightCurbCurveLoop = null;
						}
						num7++;
					}
					num2 += num;
				}
				else
				{
					if (flag2)
					{
						if (num4 == 0)
						{
							curveLoop = FindCurveLoopProfileAtStation(num4, num2, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
						}
						else if (flag)
						{
							curveLoop = FindCurveLoopProfileAtStation(num4, num2 + dOffsetAdjust, _bUseDirection: true, num3, dAdjustDistance, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
							flag = false;
						}
						else
						{
							curveLoop = FindCurveLoopProfileAtStation(num4, num2 + dOffsetAdjust, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
						}
						if (curveLoop != null)
						{
							list4.Add(curveLoop);
							curveLoop = null;
						}
						if (flag3 && _LeftCurbCurveLoop != null)
						{
							list5.Add(_LeftCurbCurveLoop);
							_LeftCurbCurveLoop = null;
						}
						if (flag4 && _RightCurbCurveLoop != null)
						{
							list6.Add(_RightCurbCurveLoop);
							_RightCurbCurveLoop = null;
						}
					}
					if (m_IntersectionContainers != null)
					{
						while (num5 < m_IntersectionContainers.Count && num6 < m_IntersectionContainers[num5].Profiles.Count && m_IntersectionContainers[num5].Profiles[num6].Station <= num2 + num)
						{
							if (m_IntersectionContainers[num5].Profiles[num6].Station >= num2)
							{
								if (m_IntersectionContainers[num5].ProfileUsage == ProfileUsage.EntireProfile)
								{
									if (flag2 && m_IntersectionContainers[num5].StartingIntersection && num6 == 0 && m_IntersectionContainers[num5].Profiles[num6].Points.Count == 6)
									{
										eps_Point3 = m_IntersectionContainers[num5].Profiles[num6].Points[0];
										eps_Point4 = m_IntersectionContainers[num5].Profiles[num6].Points[4];
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
											list4.Add(curveLoop);
											curveLoop = null;
										}
										if (list4 != null && list4.Count > 0)
										{
											list.Add(list4);
											list4 = new List<CurveLoop>();
										}
										if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point3, m_MassLeg, ref _dStation, ref _dOffset) == 1700 && _dStation > 0.0 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation - dOffsetAdjust, _dOffset, ref Pnt) == 0)
										{
											Pnt.z -= m_dWidth / 2.0 * m_dSlope;
											_LeftCurbCurveLoop = FindCurbCurveLoopProfileAtStation(_bIsLeftCurb: true, _dStation - dOffsetAdjust, eps_Point3.z - Pnt.z);
											if (_LeftCurbCurveLoop != null)
											{
												list5.Add(_LeftCurbCurveLoop);
												_LeftCurbCurveLoop = null;
											}
											if (list5 != null)
											{
												_LeftCurbArrayArrayList.Add(list5);
												list5 = new List<CurveLoop>();
											}
										}
										if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point4, m_MassLeg, ref _dStation, ref _dOffset) == 1700 && _dStation > 0.0 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation - dOffsetAdjust, _dOffset, ref Pnt) == 0)
										{
											Pnt.z -= m_dWidth / 2.0 * m_dSlope;
											_RightCurbCurveLoop = FindCurbCurveLoopProfileAtStation(_bIsLeftCurb: false, _dStation - dOffsetAdjust, eps_Point4.z - Pnt.z);
											if (_RightCurbCurveLoop != null)
											{
												list6.Add(_RightCurbCurveLoop);
												_RightCurbCurveLoop = null;
											}
											if (list6 != null)
											{
												_RightCurbArrayArrayList.Add(list6);
												list6 = new List<CurveLoop>();
											}
										}
									}
									curveLoop = ApplyCurveLoopProfile(m_IntersectionContainers[num5].Profiles[num6].Points);
									if (curveLoop != null)
									{
										list4.Add(curveLoop);
										curveLoop = null;
									}
									if (m_IntersectionContainers[num5].StartingIntersection)
									{
										if (num6 == 0)
										{
											flag2 = false;
											list5.Clear();
											list2 = GetCurbCurveLoopArray(_bIsLeftCurb: true, m_IntersectionContainers[num5].LeftOffsetLeg);
											if (list2 != null && list2.Count > 0)
											{
												_LeftCurbArrayArrayList.AddRange(list2);
											}
											list6.Clear();
											list3 = GetCurbCurveLoopArray(_bIsLeftCurb: false, m_IntersectionContainers[num5].RightOffsetLeg);
											if (list3 != null && list3.Count > 0)
											{
												_RightCurbArrayArrayList.AddRange(list3);
											}
										}
										else if (num6 == m_IntersectionContainers[num5].Profiles.Count - 1 && list4 != null && list4.Count > 0)
										{
											list.Add(list4);
											list4 = new List<CurveLoop>();
										}
									}
									else if (num6 == 0)
									{
										list2 = GetCurbCurveLoopArray(_bIsLeftCurb: true, m_IntersectionContainers[num5].LeftOffsetLeg);
										if (list2 != null && list2.Count > 0)
										{
											_LeftCurbArrayArrayList.AddRange(list2);
										}
										list3 = GetCurbCurveLoopArray(_bIsLeftCurb: false, m_IntersectionContainers[num5].RightOffsetLeg);
										if (list3 != null && list3.Count > 0)
										{
											_RightCurbArrayArrayList.AddRange(list3);
										}
									}
									else if (num6 == m_IntersectionContainers[num5].Profiles.Count - 1)
									{
										flag2 = true;
										if (list4 != null && list4.Count > 0)
										{
											list.Add(list4);
											list4 = new List<CurveLoop>();
										}
										if (m_IntersectionContainers[num5].Profiles[num6].Station + dOffsetAdjust < num2 + num && m_IntersectionContainers[num5].Profiles[num6].Points.Count == 6)
										{
											eps_Point3 = m_IntersectionContainers[num5].Profiles[num6].Points[0];
											eps_Point4 = m_IntersectionContainers[num5].Profiles[num6].Points[4];
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
												list4.Add(curveLoop);
												curveLoop = null;
											}
											if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point3, m_MassLeg, ref _dStation, ref _dOffset) == 1700 && _dStation > 0.0 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation - dOffsetAdjust, _dOffset, ref Pnt) == 0)
											{
												Pnt.z -= m_dWidth / 2.0 * m_dSlope;
												_LeftCurbCurveLoop = FindCurbCurveLoopProfileAtStation(_bIsLeftCurb: true, _dStation + dOffsetAdjust, eps_Point3.z - Pnt.z);
												if (_LeftCurbCurveLoop != null)
												{
													list5.Add(_LeftCurbCurveLoop);
													_LeftCurbCurveLoop = null;
												}
											}
											if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point4, m_MassLeg, ref _dStation, ref _dOffset) == 1700 && _dStation > 0.0 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation - dOffsetAdjust, _dOffset, ref Pnt) == 0)
											{
												Pnt.z -= m_dWidth / 2.0 * m_dSlope;
												_RightCurbCurveLoop = FindCurbCurveLoopProfileAtStation(_bIsLeftCurb: false, _dStation + dOffsetAdjust, eps_Point4.z - Pnt.z);
												if (_RightCurbCurveLoop != null)
												{
													list6.Add(_RightCurbCurveLoop);
													_RightCurbCurveLoop = null;
												}
											}
										}
									}
								}
								else if (m_IntersectionContainers[num5].ProfileUsage == ProfileUsage.LeftOnly)
								{
									_LeftCurbCurveLoop = FindCurbCurveLoopProfileAtStation(_bIsLeftCurb: true, m_IntersectionContainers[num5].Profiles[num6].Station);
									if (_LeftCurbCurveLoop != null)
									{
										list5.Add(_LeftCurbCurveLoop);
										_LeftCurbCurveLoop = null;
									}
									if (m_IntersectionContainers[num5].StartingIntersection)
									{
										if (num6 == 0)
										{
											flag3 = false;
											if (list5 != null && list5.Count > 0)
											{
												_LeftCurbArrayArrayList.Add(list5);
												list5 = new List<CurveLoop>();
											}
										}
									}
									else if (num6 == m_IntersectionContainers[num5].Profiles.Count - 1)
									{
										flag3 = true;
									}
								}
								else if (m_IntersectionContainers[num5].ProfileUsage == ProfileUsage.RightOnly)
								{
									_RightCurbCurveLoop = FindCurbCurveLoopProfileAtStation(_bIsLeftCurb: false, m_IntersectionContainers[num5].Profiles[num6].Station);
									if (_RightCurbCurveLoop != null)
									{
										list6.Add(_RightCurbCurveLoop);
										_RightCurbCurveLoop = null;
									}
									if (m_IntersectionContainers[num5].StartingIntersection)
									{
										if (num6 == 0)
										{
											flag4 = false;
											if (list6 != null && list6.Count > 0)
											{
												_RightCurbArrayArrayList.Add(list6);
												list6 = new List<CurveLoop>();
											}
										}
									}
									else if (num6 == m_IntersectionContainers[num5].Profiles.Count - 1)
									{
										flag4 = true;
									}
								}
							}
							num6++;
							if (num6 >= m_IntersectionContainers[num5].Profiles.Count)
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
									curveLoop = FindCurveLoopProfileAtStation(num4, num2, _bUseDirection: true, num3, dAdjustDistance, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
								}
								else
								{
									curveLoop = FindCurveLoopProfileAtStation(num4, num2, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
								}
							}
							else
							{
								curveLoop = FindCurveLoopProfileAtStation(num4, num2, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
							}
						}
						else
						{
							curveLoop = FindCurveLoopProfileAtStation(num4, num2, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
						}
						if (curveLoop != null)
						{
							list4.Add(curveLoop);
							curveLoop = null;
						}
						if (flag3 && _LeftCurbCurveLoop != null)
						{
							list5.Add(_LeftCurbCurveLoop);
							_LeftCurbCurveLoop = null;
						}
						if (flag4 && _RightCurbCurveLoop != null)
						{
							list6.Add(_RightCurbCurveLoop);
							_RightCurbCurveLoop = null;
						}
					}
				}
				if (flag2 && list4 != null && list4.Count > 0)
				{
					list.Add(list4);
					if (flag3 && list5 != null && list5.Count > 0)
					{
						_LeftCurbArrayArrayList.Add(list5);
					}
					if (flag4 && list6 != null && list6.Count > 0)
					{
						_RightCurbArrayArrayList.Add(list6);
					}
				}
				num4++;
			}
		}
		return list;
	}

	private List<ReferenceArrayArray> GetCurbReferenceArrayArray(bool _bIsLeftCurb, Eps_Leg _OffsetLeg)
	{
		List<ReferenceArrayArray> result = new List<ReferenceArrayArray>();
		if (_OffsetLeg != null)
		{
			if (_bIsLeftCurb)
			{
				if (m_LeftCurbForm != null)
				{
					m_LeftCurbForm.MassLeg = _OffsetLeg;
					m_LeftCurbForm.MassDocument = m_AdditionalMassDocument;
					result = m_LeftCurbForm.GetReferenceArrayArray(0.0, 0.0);
				}
			}
			else if (m_RightCurbForm != null)
			{
				m_RightCurbForm.MassLeg = _OffsetLeg;
				m_RightCurbForm.MassDocument = m_AdditionalMassDocument;
				result = m_RightCurbForm.GetReferenceArrayArray(0.0, 0.0);
			}
		}
		return result;
	}

	private List<List<CurveLoop>> GetCurbCurveLoopArray(bool _bIsLeftCurb, Eps_Leg _OffsetLeg)
	{
		List<List<CurveLoop>> result = null;
		if (_OffsetLeg != null)
		{
			if (_bIsLeftCurb)
			{
				if (m_LeftCurbForm != null)
				{
					m_LeftCurbForm.MassLeg = _OffsetLeg;
					m_LeftCurbForm.MassDocument = m_AdditionalMassDocument;
					result = m_LeftCurbForm.GetCurveLoopArray(0.0, 0.0);
				}
			}
			else if (m_RightCurbForm != null)
			{
				m_RightCurbForm.MassLeg = _OffsetLeg;
				m_RightCurbForm.MassDocument = m_AdditionalMassDocument;
				result = m_RightCurbForm.GetCurveLoopArray(0.0, 0.0);
			}
		}
		return result;
	}

	private ReferenceArray FindCurbReferenceArrayProfileAtStation(bool _bIsLeftCurb, double _dStation)
	{
		return FindCurbReferenceArrayProfileAtStation(_bIsLeftCurb, _dStation, 0.0);
	}

	private CurveLoop FindCurbCurveLoopProfileAtStation(bool _bIsLeftCurb, double _dStation)
	{
		return FindCurbCurveLoopProfileAtStation(_bIsLeftCurb, _dStation, 0.0);
	}

	private ReferenceArray FindCurbReferenceArrayProfileAtStation(bool _bIsLeftCurb, double _dStation, double _dAdjustElevation)
	{
		Eps_Leg eps_Leg = null;
		ReferenceArray result = new ReferenceArray();
		double dOffsetAdjust = m_dOffsetAdjust;
		if (_bIsLeftCurb)
		{
			if (m_LeftCurbForm != null)
			{
				eps_Leg = new Eps_Leg();
				RevitGeometry.OffsetLegList(m_MassLeg, (m_dWidth + dOffsetAdjust) / 2.0, 2, ref eps_Leg, TrimClosedObject: true);
				m_LeftCurbForm.MassLeg = eps_Leg;
				m_LeftCurbForm.MassDocument = m_AdditionalMassDocument;
				result = m_LeftCurbForm.FindReferenceArrayProfileAtStation(0, _dStation, 0.0, 0.0 - m_dWidth / 2.0 * m_dSlope + _dAdjustElevation, _bUseDirection: false, 0.0, 1.0);
			}
		}
		else if (m_RightCurbForm != null)
		{
			eps_Leg = new Eps_Leg();
			RevitGeometry.OffsetLegList(m_MassLeg, (m_dWidth + dOffsetAdjust) / 2.0, 1, ref eps_Leg, TrimClosedObject: true);
			m_RightCurbForm.MassLeg = eps_Leg;
			m_RightCurbForm.MassDocument = m_AdditionalMassDocument;
			result = m_RightCurbForm.FindReferenceArrayProfileAtStation(0, _dStation, 0.0, 0.0 - m_dWidth / 2.0 * m_dSlope + _dAdjustElevation, _bUseDirection: false, 0.0, 1.0);
		}
		return result;
	}

	private CurveLoop FindCurbCurveLoopProfileAtStation(bool _bIsLeftCurb, double _dStation, double _dAdjustElevation)
	{
		Eps_Leg eps_Leg = null;
		CurveLoop result = null;
		double dOffsetAdjust = m_dOffsetAdjust;
		if (_bIsLeftCurb)
		{
			if (m_LeftCurbForm != null)
			{
				eps_Leg = new Eps_Leg();
				RevitGeometry.OffsetLegList(m_MassLeg, (m_dWidth + dOffsetAdjust) / 2.0, 2, ref eps_Leg, TrimClosedObject: true);
				m_LeftCurbForm.MassLeg = eps_Leg;
				m_LeftCurbForm.MassDocument = m_AdditionalMassDocument;
				result = m_LeftCurbForm.FindCurveLoopProfileAtStation(0, _dStation, 0.0, 0.0 - m_dWidth / 2.0 * m_dSlope + _dAdjustElevation, _bUseDirection: false, 0.0, 1.0);
			}
		}
		else if (m_RightCurbForm != null)
		{
			eps_Leg = new Eps_Leg();
			RevitGeometry.OffsetLegList(m_MassLeg, (m_dWidth + dOffsetAdjust) / 2.0, 1, ref eps_Leg, TrimClosedObject: true);
			m_RightCurbForm.MassLeg = eps_Leg;
			m_RightCurbForm.MassDocument = m_AdditionalMassDocument;
			result = m_RightCurbForm.FindCurveLoopProfileAtStation(0, _dStation, 0.0, 0.0 - m_dWidth / 2.0 * m_dSlope + _dAdjustElevation, _bUseDirection: false, 0.0, 1.0);
		}
		return result;
	}

	private CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation, ref CurveLoop _LeftCurbCurveLoop, ref CurveLoop _RightCurbCurveLoop)
	{
		return FindCurveLoopProfileAtStation(_nDocIndex, _dStation, _bUseDirection: false, 0.0, 1.0, _bGetCurbReferenceArrayProfile: true, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
	}

	private CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation)
	{
		CurveLoop _LeftCurbCurveLoop = null;
		CurveLoop _RightCurbCurveLoop = null;
		return FindCurveLoopProfileAtStation(_nDocIndex, _dStation, _bUseDirection: false, 0.0, 1.0, _bGetCurbReferenceArrayProfile: false, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
	}

	private CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		CurveLoop _LeftCurbCurveLoop = null;
		CurveLoop _RightCurbCurveLoop = null;
		return FindCurveLoopProfileAtStation(_nDocIndex, _dStation, _bUseDirection, _dDirection, _dAdjustDistance, _bGetCurbReferenceArrayProfile: false, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
	}

	private CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance, ref CurveLoop _LeftCurbCurveLoop, ref CurveLoop _RightCurbCurveLoop)
	{
		return FindCurveLoopProfileAtStation(_nDocIndex, _dStation, _bUseDirection, _dDirection, _dAdjustDistance, _bGetCurbReferenceArrayProfile: true, ref _LeftCurbCurveLoop, ref _RightCurbCurveLoop);
	}

	private CurveLoop FindCurveLoopProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance, bool _bGetCurbReferenceArrayProfile, ref CurveLoop _LeftCurbCurveLoop, ref CurveLoop _RightCurbCurveLoop)
	{
		double dOffsetAdjust = m_dOffsetAdjust;
		CurveLoop curveLoop = new CurveLoop();
		CurveArray curveArray = new CurveArray();
		try
		{
			Eps_Point eps_Point = CalcTemplateOffsets(StreetOffsetPoint.LeftEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point2 = CalcTemplateOffsets(StreetOffsetPoint.RightEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point3 = CalcTemplateOffsets(StreetOffsetPoint.CrownEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point4 = CalcTemplateOffsets(StreetOffsetPoint.SubgradeCrownEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point5 = CalcTemplateOffsets(StreetOffsetPoint.LeftSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point6 = CalcTemplateOffsets(StreetOffsetPoint.RightSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Line line = null;
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point, eps_Point5, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point.XYZ, eps_Point5.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point5, eps_Point4, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point5.XYZ, eps_Point4.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point4, eps_Point6, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point4.XYZ, eps_Point6.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point6, eps_Point2, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point6.XYZ, eps_Point2.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point2, eps_Point3, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point2.XYZ, eps_Point3.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point3, eps_Point, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point3.XYZ, eps_Point.XYZ, bBound: true);
				curveArray.Append(line);
			}
			foreach (Curve item in curveArray)
			{
				curveLoop.Append(item);
			}
			if (_bGetCurbReferenceArrayProfile)
			{
				if (m_LeftCurbForm != null)
				{
					m_LeftCurbForm.MassLeg = m_MassLeg;
					m_LeftCurbForm.MassDocument = m_AdditionalMassDocument;
					_LeftCurbCurveLoop = m_LeftCurbForm.FindCurveLoopProfileAtStation(_nDocIndex, _dStation, (0.0 - (m_dWidth + dOffsetAdjust)) / 2.0, 0.0 - m_dWidth / 2.0 * m_dSlope, _bUseDirection, _dDirection, _dAdjustDistance);
				}
				if (m_RightCurbForm != null)
				{
					m_RightCurbForm.MassLeg = m_MassLeg;
					m_RightCurbForm.MassDocument = m_AdditionalMassDocument;
					_RightCurbCurveLoop = m_RightCurbForm.FindCurveLoopProfileAtStation(_nDocIndex, _dStation, (m_dWidth + dOffsetAdjust) / 2.0, 0.0 - m_dWidth / 2.0 * m_dSlope, _bUseDirection, _dDirection, _dAdjustDistance);
				}
			}
		}
		catch (Exception)
		{
		}
		return curveLoop;
	}

	private ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation, ref ReferenceArray _LeftCurbReferenceArray, ref ReferenceArray _RightCurbReferenceArray)
	{
		return FindReferenceArrayProfileAtStation(_nDocIndex, _dStation, _bUseDirection: false, 0.0, 1.0, _bGetCurbReferenceArrayProfile: true, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
	}

	private ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation)
	{
		ReferenceArray _LeftCurbReferenceArray = null;
		ReferenceArray _RightCurbReferenceArray = null;
		return FindReferenceArrayProfileAtStation(_nDocIndex, _dStation, _bUseDirection: false, 0.0, 1.0, _bGetCurbReferenceArrayProfile: false, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
	}

	private ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		ReferenceArray _LeftCurbReferenceArray = null;
		ReferenceArray _RightCurbReferenceArray = null;
		return FindReferenceArrayProfileAtStation(_nDocIndex, _dStation, _bUseDirection, _dDirection, _dAdjustDistance, _bGetCurbReferenceArrayProfile: false, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
	}

	private ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance, ref ReferenceArray _LeftCurbReferenceArray, ref ReferenceArray _RightCurbReferenceArray)
	{
		return FindReferenceArrayProfileAtStation(_nDocIndex, _dStation, _bUseDirection, _dDirection, _dAdjustDistance, _bGetCurbReferenceArrayProfile: true, ref _LeftCurbReferenceArray, ref _RightCurbReferenceArray);
	}

	private ReferenceArray FindReferenceArrayProfileAtStation(int _nDocIndex, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance, bool _bGetCurbReferenceArrayProfile, ref ReferenceArray _LeftCurbReferenceArray, ref ReferenceArray _RightCurbReferenceArray)
	{
		double dOffsetAdjust = m_dOffsetAdjust;
		ReferenceArray referenceArray = new ReferenceArray();
		CurveArray curveArray = new CurveArray();
		try
		{
			Eps_Point eps_Point = CalcTemplateOffsets(StreetOffsetPoint.LeftEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point2 = CalcTemplateOffsets(StreetOffsetPoint.RightEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point3 = CalcTemplateOffsets(StreetOffsetPoint.CrownEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point4 = CalcTemplateOffsets(StreetOffsetPoint.SubgradeCrownEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point5 = CalcTemplateOffsets(StreetOffsetPoint.LeftSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Eps_Point eps_Point6 = CalcTemplateOffsets(StreetOffsetPoint.RightSubgradeEdge, _dStation, _bUseDirection, _dDirection, _dAdjustDistance);
			Line line = null;
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point, eps_Point5, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point.XYZ, eps_Point5.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point5, eps_Point4, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point5.XYZ, eps_Point4.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point4, eps_Point6, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point4.XYZ, eps_Point6.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point6, eps_Point2, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point6.XYZ, eps_Point2.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point2, eps_Point3, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point2.XYZ, eps_Point3.XYZ, bBound: true);
				curveArray.Append(line);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(eps_Point3, eps_Point, 0.0025))
			{
				line = RevitUtility.CreateLine(eps_Point3.XYZ, eps_Point.XYZ, bBound: true);
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
			if (_bGetCurbReferenceArrayProfile)
			{
				if (m_LeftCurbForm != null)
				{
					m_LeftCurbForm.MassLeg = m_MassLeg;
					m_LeftCurbForm.MassDocument = m_AdditionalMassDocument;
					_LeftCurbReferenceArray = m_LeftCurbForm.FindReferenceArrayProfileAtStation(_nDocIndex, _dStation, (0.0 - (m_dWidth + dOffsetAdjust)) / 2.0, 0.0 - m_dWidth / 2.0 * m_dSlope, _bUseDirection, _dDirection, _dAdjustDistance);
				}
				if (m_RightCurbForm != null)
				{
					m_RightCurbForm.MassLeg = m_MassLeg;
					m_RightCurbForm.MassDocument = m_AdditionalMassDocument;
					_RightCurbReferenceArray = m_RightCurbForm.FindReferenceArrayProfileAtStation(_nDocIndex, _dStation, (m_dWidth + dOffsetAdjust) / 2.0, 0.0 - m_dWidth / 2.0 * m_dSlope, _bUseDirection, _dDirection, _dAdjustDistance);
				}
			}
		}
		catch (Exception)
		{
		}
		return referenceArray;
	}

	private Eps_Point CalcTemplateOffsets(StreetOffsetPoint _StreetOffsetPoint, double _dStation, bool _bUseDirection, double _dDirection, double _dAdjustDistance)
	{
		Eps_Point _NewPnt = new Eps_Point();
		double num = m_dWidth / _dAdjustDistance;
		double num2 = 0.0;
		double num3 = 0.0;
		switch (_StreetOffsetPoint)
		{
		case StreetOffsetPoint.CrownEdge:
			num2 = 0.0;
			num3 = 0.0;
			break;
		case StreetOffsetPoint.SubgradeCrownEdge:
			num2 = 0.0;
			num3 = 0.0 - m_dSubgradeDepth;
			break;
		case StreetOffsetPoint.LeftEdge:
			num2 = (0.0 - num) / 2.0;
			num3 = 0.0 - m_dWidth / 2.0 * m_dSlope;
			break;
		case StreetOffsetPoint.RightEdge:
			num2 = num / 2.0;
			num3 = 0.0 - m_dWidth / 2.0 * m_dSlope;
			break;
		case StreetOffsetPoint.LeftSubgradeEdge:
			num2 = (0.0 - num) / 2.0;
			num3 = 0.0 - (m_dSubgradeDepth + m_dWidth / 2.0 * m_dSlope);
			break;
		case StreetOffsetPoint.RightSubgradeEdge:
			num2 = num / 2.0;
			num3 = 0.0 - (m_dSubgradeDepth + m_dWidth / 2.0 * m_dSlope);
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
