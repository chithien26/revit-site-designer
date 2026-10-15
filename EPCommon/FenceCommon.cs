using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using SITES.Utilities;

namespace EPCommon;

public class FenceCommon
{
	public static bool PlaceLegList(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _CurrentLeg)
	{
		if (_PlacementSettings.ExistingElementChecked && !RevitUtility.EvaluateLeg(_Document, _PlacementSettings.TransactionContainer, _PlacementSettings.Width, ref _CurrentLeg))
		{
			return false;
		}
		_PlacementSettings.IsReverse = _CurrentLeg.IsReverse;
		if (_PlacementSettings.SetbackSpacing == 0.0)
		{
			PlaceCenterRowOnLeg(_Document, _PlacementSettings, _CurrentLeg);
		}
		else
		{
			Eps_Leg ppOffsetLegList = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentLeg, _PlacementSettings.SetbackSpacing, 1, ref ppOffsetLegList, TrimClosedObject: true);
			Eps_Leg ppOffsetLegList2 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentLeg, _PlacementSettings.SetbackSpacing, 2, ref ppOffsetLegList2, TrimClosedObject: true);
			_PlacementSettings.TransactionContainer.BeginTransaction();
			ModelCurveArray modelCurveArray = RevitUtility.NewModelCurveArray(_Document, ppOffsetLegList);
			ModelCurveArray modelCurveArray2 = RevitUtility.NewModelCurveArray(_Document, ppOffsetLegList2);
			_PlacementSettings.TransactionContainer.EndTransaction();
			_Document.Regenerate();
			bool _UseRightSide = true;
			bool flag = RevitUtility.PickSideLeg(_Document, _CurrentLeg, ref _UseRightSide);
			ElementSet elementSet = new ElementSet();
			foreach (ModelCurve item3 in modelCurveArray)
			{
				elementSet.Insert(item3);
			}
			foreach (ModelCurve item4 in modelCurveArray2)
			{
				elementSet.Insert(item4);
			}
			_PlacementSettings.TransactionContainer.BeginTransaction();
			_Document.Delete(ElementSetConverter.ToIDList(elementSet));
			_PlacementSettings.TransactionContainer.EndTransaction();
			if (!flag)
			{
				return false;
			}
			_PlacementSettings.UseRightSide = _UseRightSide;
			if (_UseRightSide)
			{
				PlaceCenterRowOnLeg(_Document, _PlacementSettings, ppOffsetLegList);
			}
			else
			{
				PlaceCenterRowOnLeg(_Document, _PlacementSettings, ppOffsetLegList2);
			}
		}
		if (_PlacementSettings.DeleteExistingChecked && _CurrentLeg.ElementSet != null && _CurrentLeg.ElementSet.Size > 0)
		{
			_PlacementSettings.TransactionContainer.BeginTransaction();
			_Document.Delete(ElementSetConverter.ToIDList(_CurrentLeg.ElementSet));
			_PlacementSettings.TransactionContainer.EndTransaction();
		}
		return true;
	}

	public static void PlaceCenterRowOnLine(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Line _Line)
	{
		PlaceCenterRowOnLine(_Document, _PlacementSettings, _Element, _Line, null);
	}

	public static void PlaceCenterRowOnLine(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Line _Line, List<FamilyInstance> _ExistingFenceList)
	{
		Eps_Point _NewPnt = new Eps_Point();
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point _NewPnt2 = new Eps_Point();
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		double num = 0.0;
		double width = _PlacementSettings.Width;
		double _dElevation = 0.0;
		double _dDistance2 = 0.0;
		if (_Line == null)
		{
			return;
		}
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDirection);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDistance);
		List<Eps_Point> list = new List<Eps_Point>();
		FamilyInstance familyInstance = null;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		bool flag = !_PlacementSettings.DeleteExistingChecked;
		bool flag2 = false;
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		KeyValuePair<string, string> keyValuePair = default;
		bool bUseFailureHandling = true;
		if (_PlacementSettings.GenericModelIDList == null)
		{
			flag2 = true;
			_PlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			keyValuePair = _PlacementSettings.GenericModelIDList[0];
		}
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(_dDistance - _PlacementSettings.StartingDistance);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressPlacingFenceMessage");
		eps_Point = _Line.StartPnt;
		for (num = _PlacementSettings.StartingDistance; num <= _dDistance; num += width)
		{
			if (num > _dDistance - width)
			{
				continue;
			}
			EPShared.eplGeoE_CalcPolarToXY(_Line.StartPnt, _dDirection, num + width / 2.0, ref _NewPnt);
			if (!RevitUtility.IsPointInPointList(_NewPnt, list))
			{
				if (_Element != null && flag && flag2)
				{
					flag2 = false;
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
					{
						keyValuePair = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Element);
					}
					else
					{
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateFenceNoPositioning", EPShared.LocalRM.GetString("PositionFenceMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag = false;
					}
				}
				list.Add(_NewPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlacementSettings.SurfaceList, ref _dElevation);
				_NewPnt.z = _dElevation;
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, _dDirection, width / 2.0, ref eps_Point);
				RevitUtility.GetSurfaceContainingPoint(eps_Point, _PlacementSettings.SurfaceList, ref _dElevation);
				eps_Point.z = _dElevation;
				EPShared.eplGeoE_CalcPolarToXY(eps_Point, _dDirection + Math.PI, width, ref _NewPnt2);
				RevitUtility.GetSurfaceContainingPoint(_NewPnt2, _PlacementSettings.SurfaceList, ref _dElevation);
				_NewPnt2.z = _dElevation;
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NewPnt2, eps_Point, ref _dDistance2);
				double dVertical = eps_Point.z - _NewPnt2.z;
				double num5 = RevitGeometry.CalculateDirection(_dDistance2, dVertical);
				if (num5 <= -Math.PI / 2.0 && num5 >= Math.PI / 2.0)
				{
					_PlacementSettings.TransactionContainer.EndTransaction();
					RevitMessageBox.Show(EPShared.LocalRM.GetString("FenceError"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return;
				}
				FamilyInstance familyInstance2;
				if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingFenceList != null && _ExistingFenceList.Count > 0)
				{
					familyInstance2 = _ExistingFenceList[0];
					if (familyInstance2.Location is LocationPoint locationPoint)
					{
						if (!EPShared.eplGeoE_ArePntsDuplicates3D(_NewPnt, new Eps_Point(locationPoint.Point)))
						{
							XYZ translation = new XYZ(_NewPnt.XYZ.X - locationPoint.Point.X, _NewPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
							ElementTransformUtils.MoveElement(_Document, familyInstance2.Id, translation);
						}
						num4 = locationPoint.Rotation;
					}
					_ExistingFenceList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
					familyInstance2 = RevitUtility.NewFamilyInstance(_Document, _NewPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
					_PlacementSettings.TransactionContainer = _TransactionContainer;
				}
				familyInstance = familyInstance2;
				if (Math.Abs(_PlacementSettings.Height) > 0.0)
				{
					Parameter parameter = familyInstance2.LookupParameter(RevitParameters.FenceFamilyHeightParameter);
					if (parameter != null)
					{
						num2 = parameter.AsDouble();
						if (!RevitDefines.EQ(num2, _PlacementSettings.Height))
						{
							parameter.Set(_PlacementSettings.Height);
						}
					}
				}
				if (Math.Abs(_PlacementSettings.Width) > 0.0)
				{
					Parameter parameter2 = familyInstance2.LookupParameter(RevitParameters.FenceFamilySpanParameter);
					if (parameter2 != null)
					{
						num3 = parameter2.AsDouble();
						if (!RevitDefines.EQ(num3, _PlacementSettings.Width))
						{
							parameter2.Set(_PlacementSettings.Width);
						}
					}
				}
				XYZ endpoint = new XYZ(_NewPnt.x, _NewPnt.y, _NewPnt.z + 1.0);
				Line axis = Line.CreateBound(_NewPnt.XYZ, endpoint);
				Parameter parameter3 = familyInstance2.LookupParameter(RevitParameters.FenceRightVisiability);
				Parameter parameter4 = familyInstance2.LookupParameter(RevitParameters.FenceLeftVisiability);
				bool value = false;
				bool value2 = false;
				if (num5 < 0.0)
				{
					if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), EPShared.eplGeoE_NormalizeRadians(num4)))
					{
						ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI - num4));
					}
					value = true;
				}
				else
				{
					if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dDirection), EPShared.eplGeoE_NormalizeRadians(num4)))
					{
						ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dDirection - num4));
					}
					value2 = true;
				}
				if (parameter3 != null && parameter4 != null)
				{
					parameter3.Set(Convert.ToInt32(value));
					parameter4.Set(Convert.ToInt32(value2));
				}
				num5 = Math.Abs(num5);
				familyInstance2.LookupParameter(RevitParameters.FenceFamilyAngleParameter)?.Set(num5);
				if (_Element != null && flag && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter5 = familyInstance2.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter5 != null)
					{
						parameter5.Set(keyValuePair.Value);
					}
					else
					{
						flag = false;
					}
				}
			}
			else
			{
				familyInstance = null;
			}
			progressBarForm.UpdateProgressBar(Convert.ToInt32(width));
			num4 = 0.0;
		}
		double num6 = 0.0;
		num6 = _dDistance - (num - width);
		if (RevitDefines.GT(num6, 0.001))
		{
			EPShared.eplGeoE_CalcPolarToXY(eps_Point, _dDirection, num6 / 2.0, ref _NewPnt);
			if (!RevitUtility.IsPointInPointList(_NewPnt, list))
			{
				if (_Element != null && flag && flag2)
				{
					flag2 = false;
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
					{
						keyValuePair = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Element);
					}
					else
					{
						WarningOptionsForm warningOptionsForm2 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateFenceNoPositioning", EPShared.LocalRM.GetString("PositionFenceMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm2.ShowDialog();
						flag = false;
					}
				}
				list.Add(_NewPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlacementSettings.SurfaceList, ref _dElevation);
				_NewPnt.z = _dElevation;
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, _dDirection, (num - _dDistance) / 2.0, ref eps_Point);
				RevitUtility.GetSurfaceContainingPoint(eps_Point, _PlacementSettings.SurfaceList, ref _dElevation);
				eps_Point.z = _dElevation;
				EPShared.eplGeoE_CalcPolarToXY(eps_Point, _dDirection + Math.PI, width, ref _NewPnt2);
				RevitUtility.GetSurfaceContainingPoint(_NewPnt2, _PlacementSettings.SurfaceList, ref _dElevation);
				_NewPnt2.z = _dElevation;
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NewPnt2, eps_Point, ref _dDistance2);
				double dVertical2 = eps_Point.z - _NewPnt2.z;
				double num7 = RevitGeometry.CalculateDirection(_dDistance2, dVertical2);
				if (num7 <= -Math.PI / 2.0 && num7 >= Math.PI / 2.0)
				{
					_PlacementSettings.TransactionContainer.EndTransaction();
					RevitMessageBox.Show(EPShared.LocalRM.GetString("FenceError"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return;
				}
				FamilyInstance familyInstance2;
				if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingFenceList != null && _ExistingFenceList.Count > 0)
				{
					familyInstance2 = _ExistingFenceList[0];
					if (familyInstance2.Location is LocationPoint locationPoint2)
					{
						if (!EPShared.eplGeoE_ArePntsDuplicates3D(_NewPnt, new Eps_Point(locationPoint2.Point)))
						{
							XYZ translation2 = new XYZ(_NewPnt.XYZ.X - locationPoint2.Point.X, _NewPnt.XYZ.Y - locationPoint2.Point.Y, 0.0);
							ElementTransformUtils.MoveElement(_Document, familyInstance2.Id, translation2);
						}
						num4 = locationPoint2.Rotation;
					}
					_ExistingFenceList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer2 = _PlacementSettings.TransactionContainer;
					familyInstance2 = RevitUtility.NewFamilyInstance(_Document, _NewPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer2);
					_PlacementSettings.TransactionContainer = _TransactionContainer2;
				}
				if (Math.Abs(_PlacementSettings.Height) > 0.0)
				{
					Parameter parameter6 = familyInstance2.LookupParameter(RevitParameters.FenceFamilyHeightParameter);
					if (parameter6 != null)
					{
						num2 = parameter6.AsDouble();
						if (!RevitDefines.EQ(num2, _PlacementSettings.Height))
						{
							parameter6.Set(_PlacementSettings.Height);
						}
					}
				}
				if (Math.Abs(_PlacementSettings.Width) > 0.0)
				{
					Parameter parameter7 = familyInstance2.LookupParameter(RevitParameters.FenceFamilySpanParameter);
					if (parameter7 != null)
					{
						num3 = parameter7.AsDouble();
						if (!RevitDefines.EQ(num3, num6))
						{
							parameter7.Set(num6);
						}
					}
				}
				XYZ endpoint2 = new XYZ(_NewPnt.x, _NewPnt.y, _NewPnt.z + 1.0);
				Line axis2 = Line.CreateBound(_NewPnt.XYZ, endpoint2);
				Parameter parameter8 = familyInstance2.LookupParameter(RevitParameters.FenceRightVisiability);
				Parameter parameter9 = familyInstance2.LookupParameter(RevitParameters.FenceLeftVisiability);
				bool value3 = true;
				bool value4 = true;
				if (num7 < 0.0)
				{
					if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), EPShared.eplGeoE_NormalizeRadians(num4)))
					{
						ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis2, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI - num4));
					}
				}
				else if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dDirection), EPShared.eplGeoE_NormalizeRadians(num4)))
				{
					ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis2, EPShared.eplGeoE_NormalizeRadians(_dDirection - num4));
				}
				if (parameter8 != null && parameter9 != null)
				{
					parameter8.Set(Convert.ToInt32(value3));
					parameter9.Set(Convert.ToInt32(value4));
				}
				num7 = Math.Abs(num7);
				familyInstance2.LookupParameter(RevitParameters.FenceFamilyAngleParameter)?.Set(num7);
				if (_Element != null && flag && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter10 = familyInstance2.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter10 != null)
					{
						parameter10.Set(keyValuePair.Value);
					}
					else
					{
						flag = false;
					}
				}
			}
		}
		else if (familyInstance != null)
		{
			Parameter parameter11 = familyInstance.LookupParameter(RevitParameters.FenceRightVisiability);
			Parameter parameter12 = familyInstance.LookupParameter(RevitParameters.FenceLeftVisiability);
			if (parameter11 != null && parameter12 != null)
			{
				parameter11.Set(Convert.ToInt32(value: true));
				parameter12.Set(Convert.ToInt32(value: true));
			}
		}
		if (_ExistingFenceList != null && _ExistingFenceList.Count > 0)
		{
			for (int i = 0; i < _ExistingFenceList.Count; i++)
			{
				_Document.Delete(_ExistingFenceList[i].Id);
			}
		}
		if (_PlacementSettings.GenericModelIDList == null)
		{
			_PlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
	}

	public static void PlaceCenterRowOnArc(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Arc _Arc)
	{
		PlaceCenterRowOnArc(_Document, _PlacementSettings, _Element, _Arc, null);
	}

	public static void PlaceCenterRowOnArc(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Arc _Arc, List<FamilyInstance> _ExistingFenceList)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point _NewPnt = new Eps_Point();
		double num = 0.0;
		double width = _PlacementSettings.Width;
		double num2 = 0.0;
		double _dElevation = 0.0;
		if (_Arc == null)
		{
			return;
		}
		num2 = _Arc.Radius * _Arc.Delta;
		List<Eps_Point> list = new List<Eps_Point>();
		FamilyInstance familyInstance = null;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		bool flag = !_PlacementSettings.DeleteExistingChecked;
		bool flag2 = false;
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		KeyValuePair<string, string> keyValuePair = default;
		bool bUseFailureHandling = true;
		if (_PlacementSettings.GenericModelIDList == null)
		{
			flag2 = true;
			_PlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			keyValuePair = _PlacementSettings.GenericModelIDList[0];
		}
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(num2 - _PlacementSettings.StartingDistance);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressPlacingFenceMessage");
		eps_Point = _Arc.StartPnt;
		for (num = _PlacementSettings.StartingDistance; num <= num2; num += width)
		{
			if (num > num2 - width)
			{
				continue;
			}
			EPShared.eplGeoE_FindStaOffPointAlongArc(_Arc, num + width / 2.0, 0.0, ref _StaOffPnt);
			if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
			{
				if (_Element != null && flag && flag2)
				{
					flag2 = false;
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
					{
						keyValuePair = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Element);
					}
					else
					{
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateFenceNoPositioning", EPShared.LocalRM.GetString("PositionFenceMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag = false;
					}
				}
				list.Add(_StaOffPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList, ref _dElevation);
				_StaOffPnt.z = _dElevation;
				double _dTangentDir = 0.0;
				double _dDistance = 0.0;
				EPShared.eplGeoE_CalcTangentDirOfArc(_StaOffPnt, _Arc.RadiusPnt, _Arc.Direction, ref _dTangentDir);
				EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, _dTangentDir, width / 2.0, ref eps_Point);
				RevitUtility.GetSurfaceContainingPoint(eps_Point, _PlacementSettings.SurfaceList, ref _dElevation);
				eps_Point.z = _dElevation;
				EPShared.eplGeoE_CalcPolarToXY(eps_Point, _dTangentDir + Math.PI, width, ref _NewPnt);
				RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlacementSettings.SurfaceList, ref _dElevation);
				_NewPnt.z = _dElevation;
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NewPnt, eps_Point, ref _dDistance);
				double dVertical = eps_Point.z - _NewPnt.z;
				double num6 = RevitGeometry.CalculateDirection(_dDistance, dVertical);
				if (num6 <= -Math.PI / 2.0 && num6 >= Math.PI / 2.0)
				{
					_PlacementSettings.TransactionContainer.EndTransaction();
					RevitMessageBox.Show(EPShared.LocalRM.GetString("FenceError"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return;
				}
				FamilyInstance familyInstance2;
				if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingFenceList != null && _ExistingFenceList.Count > 0)
				{
					familyInstance2 = _ExistingFenceList[0];
					if (familyInstance2.Location is LocationPoint locationPoint)
					{
						if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint.Point)))
						{
							XYZ translation = new XYZ(_StaOffPnt.XYZ.X - locationPoint.Point.X, _StaOffPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
							ElementTransformUtils.MoveElement(_Document, familyInstance2.Id, translation);
						}
						num5 = locationPoint.Rotation;
					}
					_ExistingFenceList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
					familyInstance2 = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
					_PlacementSettings.TransactionContainer = _TransactionContainer;
				}
				familyInstance = familyInstance2;
				if (Math.Abs(_PlacementSettings.Height) > 0.0)
				{
					Parameter parameter = familyInstance2.LookupParameter(RevitParameters.FenceFamilyHeightParameter);
					if (parameter != null)
					{
						num3 = parameter.AsDouble();
						if (!RevitDefines.EQ(num3, _PlacementSettings.Height))
						{
							parameter.Set(_PlacementSettings.Height);
						}
					}
				}
				if (Math.Abs(_PlacementSettings.Width) > 0.0)
				{
					Parameter parameter2 = familyInstance2.LookupParameter(RevitParameters.FenceFamilySpanParameter);
					if (parameter2 != null)
					{
						num4 = parameter2.AsDouble();
						if (!RevitDefines.EQ(num4, _PlacementSettings.Width))
						{
							parameter2.Set(_PlacementSettings.Width);
						}
					}
				}
				XYZ endpoint = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
				Line axis = Line.CreateBound(_StaOffPnt.XYZ, endpoint);
				Parameter parameter3 = familyInstance2.LookupParameter(RevitParameters.FenceRightVisiability);
				Parameter parameter4 = familyInstance2.LookupParameter(RevitParameters.FenceLeftVisiability);
				bool value = false;
				bool value2 = false;
				if (num6 < 0.0)
				{
					if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI), EPShared.eplGeoE_NormalizeRadians(num5)))
					{
						ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI - num5));
					}
					value = true;
				}
				else
				{
					if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir), EPShared.eplGeoE_NormalizeRadians(num5)))
					{
						ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - num5));
					}
					value2 = true;
				}
				if (parameter3 != null && parameter4 != null)
				{
					parameter3.Set(Convert.ToInt32(value));
					parameter4.Set(Convert.ToInt32(value2));
				}
				num6 = Math.Abs(num6);
				familyInstance2.LookupParameter(RevitParameters.FenceFamilyAngleParameter)?.Set(num6);
				if (_Element != null && flag && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter5 = familyInstance2.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter5 != null)
					{
						parameter5.Set(keyValuePair.Value);
					}
					else
					{
						flag = false;
					}
				}
			}
			else
			{
				familyInstance = null;
			}
			progressBarForm.UpdateProgressBar(Convert.ToInt32(width));
			num5 = 0.0;
		}
		double num7 = 0.0;
		num7 = num2 - (num - width);
		if (RevitDefines.GT(num7, 0.001))
		{
			EPShared.eplGeoE_FindStaOffPointAlongArc(_Arc, num2 - num7 / 2.0, 0.0, ref _StaOffPnt);
			if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
			{
				if (_Element != null && flag && flag2)
				{
					flag2 = false;
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
					{
						keyValuePair = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Element);
					}
					else
					{
						WarningOptionsForm warningOptionsForm2 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateFenceNoPositioning", EPShared.LocalRM.GetString("PositionFenceMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm2.ShowDialog();
						flag = false;
					}
				}
				list.Add(_StaOffPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList, ref _dElevation);
				_StaOffPnt.z = _dElevation;
				double _dTangentDir2 = 0.0;
				double _dDistance2 = 0.0;
				EPShared.eplGeoE_CalcTangentDirOfArc(_StaOffPnt, _Arc.RadiusPnt, _Arc.Direction, ref _dTangentDir2);
				EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, _dTangentDir2, num7 / 2.0, ref eps_Point);
				RevitUtility.GetSurfaceContainingPoint(eps_Point, _PlacementSettings.SurfaceList, ref _dElevation);
				eps_Point.z = _dElevation;
				EPShared.eplGeoE_CalcPolarToXY(eps_Point, _dTangentDir2 + Math.PI, width, ref _NewPnt);
				RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlacementSettings.SurfaceList, ref _dElevation);
				_NewPnt.z = _dElevation;
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NewPnt, eps_Point, ref _dDistance2);
				double dVertical2 = eps_Point.z - _NewPnt.z;
				double num8 = RevitGeometry.CalculateDirection(_dDistance2, dVertical2);
				if (num8 <= -Math.PI / 2.0 && num8 >= Math.PI / 2.0)
				{
					_PlacementSettings.TransactionContainer.EndTransaction();
					RevitMessageBox.Show(EPShared.LocalRM.GetString("FenceError"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return;
				}
				FamilyInstance familyInstance2;
				if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingFenceList != null && _ExistingFenceList.Count > 0)
				{
					familyInstance2 = _ExistingFenceList[0];
					if (familyInstance2.Location is LocationPoint locationPoint2)
					{
						if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint2.Point)))
						{
							XYZ translation2 = new XYZ(_StaOffPnt.XYZ.X - locationPoint2.Point.X, _StaOffPnt.XYZ.Y - locationPoint2.Point.Y, 0.0);
							ElementTransformUtils.MoveElement(_Document, familyInstance2.Id, translation2);
						}
						num5 = locationPoint2.Rotation;
					}
					_ExistingFenceList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer2 = _PlacementSettings.TransactionContainer;
					familyInstance2 = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer2);
					_PlacementSettings.TransactionContainer = _TransactionContainer2;
				}
				if (Math.Abs(_PlacementSettings.Height) > 0.0)
				{
					Parameter parameter6 = familyInstance2.LookupParameter(RevitParameters.FenceFamilyHeightParameter);
					if (parameter6 != null)
					{
						num3 = parameter6.AsDouble();
						if (!RevitDefines.EQ(num3, _PlacementSettings.Height))
						{
							parameter6.Set(_PlacementSettings.Height);
						}
					}
				}
				if (Math.Abs(_PlacementSettings.Width) > 0.0)
				{
					Parameter parameter7 = familyInstance2.LookupParameter(RevitParameters.FenceFamilySpanParameter);
					if (parameter7 != null)
					{
						num4 = parameter7.AsDouble();
						if (!RevitDefines.EQ(num4, num7))
						{
							parameter7.Set(num7);
						}
					}
				}
				XYZ endpoint2 = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
				Line axis2 = Line.CreateBound(_StaOffPnt.XYZ, endpoint2);
				Parameter parameter8 = familyInstance2.LookupParameter(RevitParameters.FenceRightVisiability);
				Parameter parameter9 = familyInstance2.LookupParameter(RevitParameters.FenceLeftVisiability);
				bool value3 = true;
				bool value4 = true;
				if (num8 < 0.0)
				{
					if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir2 + Math.PI), EPShared.eplGeoE_NormalizeRadians(num5)))
					{
						ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis2, EPShared.eplGeoE_NormalizeRadians(_dTangentDir2 + Math.PI - num5));
					}
				}
				else if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir2), EPShared.eplGeoE_NormalizeRadians(num5)))
				{
					ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis2, EPShared.eplGeoE_NormalizeRadians(_dTangentDir2 - num5));
				}
				if (parameter8 != null && parameter9 != null)
				{
					parameter8.Set(Convert.ToInt32(value3));
					parameter9.Set(Convert.ToInt32(value4));
				}
				num8 = Math.Abs(num8);
				familyInstance2.LookupParameter(RevitParameters.FenceFamilyAngleParameter)?.Set(num8);
				if (_Element != null && flag && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter10 = familyInstance2.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter10 != null)
					{
						parameter10.Set(keyValuePair.Value);
					}
					else
					{
						flag = false;
					}
				}
			}
		}
		else if (familyInstance != null)
		{
			Parameter parameter11 = familyInstance.LookupParameter(RevitParameters.FenceRightVisiability);
			Parameter parameter12 = familyInstance.LookupParameter(RevitParameters.FenceLeftVisiability);
			if (parameter11 != null && parameter12 != null)
			{
				parameter11.Set(Convert.ToInt32(value: true));
				parameter12.Set(Convert.ToInt32(value: true));
			}
		}
		if (_ExistingFenceList != null && _ExistingFenceList.Count > 0)
		{
			for (int i = 0; i < _ExistingFenceList.Count; i++)
			{
				_Document.Delete(_ExistingFenceList[i].Id);
			}
		}
		if (_PlacementSettings.GenericModelIDList == null)
		{
			_PlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
	}

	public static void PlaceCenterRowOnLeg(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _Leg)
	{
		PlaceCenterRowOnLeg(_Document, _PlacementSettings, _Leg, _Leg.ElementList, null);
	}

	public static void PlaceCenterRowOnLeg(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _Leg, List<FamilyInstance> _MovePlantList)
	{
		PlaceCenterRowOnLeg(_Document, _PlacementSettings, _Leg, _Leg.ElementList, _MovePlantList);
	}

	public static void PlaceCenterRowOnLeg(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _Leg, List<Element> _OrigLegElementSet, List<FamilyInstance> _ExistingFenceList)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point _NewPnt = new Eps_Point();
		Eps_Leg eps_Leg = null;
		double _dDistance = 0.0;
		double _dTangentDir = 0.0;
		double num = 0.0;
		double num2 = _PlacementSettings.StartingDistance;
		double width = _PlacementSettings.Width;
		double _dElevation = 0.0;
		double _dDistance2 = 0.0;
		if (_Leg == null)
		{
			return;
		}
		List<Eps_Point> list = new List<Eps_Point>();
		FamilyInstance familyInstance = null;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		bool flag = false;
		bool flag2 = !_PlacementSettings.DeleteExistingChecked;
		bool flag3 = false;
		int num6 = 0;
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		List<KeyValuePair<string, string>> list2 = new List<KeyValuePair<string, string>>();
		bool bUseFailureHandling = true;
		if (_PlacementSettings.GenericModelIDList == null)
		{
			flag3 = true;
			_PlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			list2 = _PlacementSettings.GenericModelIDList;
		}
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(RevitGeometry.eplGeoE_CalcLegListLength(_Leg) - num2);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressPlacingFenceMessage");
		eps_Point = _Leg.LegStartPnt;
		for (eps_Leg = _Leg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dTangentDir);
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDistance);
				if (num2 > _dDistance)
				{
					num2 -= _dDistance;
				}
				else
				{
					for (num = num2; num <= _dDistance; num += width)
					{
						if (num > _dDistance - width)
						{
							continue;
						}
						EPShared.eplGeoE_CalcPolarToXY(eps_Leg.Line.StartPnt, _dTangentDir, num + width / 2.0, ref _StaOffPnt);
						if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
						{
							if (eps_Leg.Element != null && flag2 && flag3)
							{
								flag3 = false;
								if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
								{
									list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Leg.ElementList);
								}
								else
								{
									WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateFenceNoPositioning", EPShared.LocalRM.GetString("PositionFenceMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
									warningOptionsForm.ShowDialog();
									flag2 = false;
								}
								if (num6 < list2.Count)
								{
									if (string.Compare(list2[num6].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag = true;
									}
									else
									{
										for (int i = 0; i < list2.Count; i++)
										{
											if (string.Compare(list2[i].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num6 = i;
												flag = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList, ref _dElevation);
							_StaOffPnt.z = _dElevation;
							EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, _dTangentDir, width / 2.0, ref eps_Point);
							RevitUtility.GetSurfaceContainingPoint(eps_Point, _PlacementSettings.SurfaceList, ref _dElevation);
							eps_Point.z = _dElevation;
							EPShared.eplGeoE_CalcPolarToXY(eps_Point, _dTangentDir + Math.PI, width, ref _NewPnt);
							RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlacementSettings.SurfaceList, ref _dElevation);
							_NewPnt.z = _dElevation;
							EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NewPnt, eps_Point, ref _dDistance2);
							double dVertical = eps_Point.z - _NewPnt.z;
							double num7 = RevitGeometry.CalculateDirection(_dDistance2, dVertical);
							if (num7 <= -Math.PI / 2.0 && num7 >= Math.PI / 2.0)
							{
								_PlacementSettings.TransactionContainer.EndTransaction();
								RevitMessageBox.Show(EPShared.LocalRM.GetString("FenceError"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
								return;
							}
							FamilyInstance familyInstance2;
							if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingFenceList != null && _ExistingFenceList.Count > 0)
							{
								familyInstance2 = _ExistingFenceList[0];
								if (familyInstance2.Location is LocationPoint locationPoint)
								{
									if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint.Point)))
									{
										XYZ translation = new XYZ(_StaOffPnt.XYZ.X - locationPoint.Point.X, _StaOffPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
										ElementTransformUtils.MoveElement(_Document, familyInstance2.Id, translation);
									}
									num5 = locationPoint.Rotation;
								}
								_ExistingFenceList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
								familyInstance2 = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
								_PlacementSettings.TransactionContainer = _TransactionContainer;
							}
							familyInstance = familyInstance2;
							if (Math.Abs(_PlacementSettings.Height) > 0.0)
							{
								Parameter parameter = familyInstance2.LookupParameter(RevitParameters.FenceFamilyHeightParameter);
								if (parameter != null)
								{
									num3 = parameter.AsDouble();
									if (!RevitDefines.EQ(num3, _PlacementSettings.Height))
									{
										parameter.Set(_PlacementSettings.Height);
									}
								}
							}
							if (Math.Abs(_PlacementSettings.Width) > 0.0)
							{
								Parameter parameter2 = familyInstance2.LookupParameter(RevitParameters.FenceFamilySpanParameter);
								if (parameter2 != null)
								{
									num4 = parameter2.AsDouble();
									if (!RevitDefines.EQ(num4, _PlacementSettings.Width))
									{
										parameter2.Set(_PlacementSettings.Width);
									}
								}
							}
							XYZ endpoint = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
							Line axis = Line.CreateBound(_StaOffPnt.XYZ, endpoint);
							Parameter parameter3 = familyInstance2.LookupParameter(RevitParameters.FenceRightVisiability);
							Parameter parameter4 = familyInstance2.LookupParameter(RevitParameters.FenceLeftVisiability);
							bool value = false;
							bool value2 = false;
							if (num7 < 0.0)
							{
								if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI), EPShared.eplGeoE_NormalizeRadians(num5)))
								{
									ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI - num5));
								}
								value = true;
							}
							else
							{
								if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir), EPShared.eplGeoE_NormalizeRadians(num5)))
								{
									ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI - num5));
								}
								value2 = true;
							}
							if (parameter3 != null && parameter4 != null)
							{
								parameter3.Set(Convert.ToInt32(value));
								parameter4.Set(Convert.ToInt32(value2));
							}
							num7 = Math.Abs(num7);
							familyInstance2.LookupParameter(RevitParameters.FenceFamilyAngleParameter)?.Set(num7);
							if (eps_Leg.Element != null && flag2 && flag)
							{
								Parameter parameter5 = familyInstance2.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter5 != null)
								{
									parameter5.Set(list2[num6].Value);
								}
								else
								{
									flag2 = false;
								}
							}
						}
						else
						{
							familyInstance = null;
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(_PlacementSettings.Width));
						num5 = 0.0;
					}
					num2 = _dDistance - (num - width);
					if (RevitDefines.GT(num2, 0.001))
					{
						EPShared.eplGeoE_CalcPolarToXY(eps_Point, _dTangentDir, num2 / 2.0, ref _StaOffPnt);
						if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
						{
							if (eps_Leg.Element != null && flag2 && flag3)
							{
								flag3 = false;
								if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
								{
									list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Leg.ElementList);
								}
								else
								{
									WarningOptionsForm warningOptionsForm2 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateFenceNoPositioning", EPShared.LocalRM.GetString("PositionFenceMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
									warningOptionsForm2.ShowDialog();
									flag2 = false;
								}
								if (num6 < list2.Count)
								{
									if (string.Compare(list2[num6].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag = true;
									}
									else
									{
										for (int j = 0; j < list2.Count; j++)
										{
											if (string.Compare(list2[j].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num6 = j;
												flag = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList, ref _dElevation);
							_StaOffPnt.z = _dElevation;
							EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, _dTangentDir, num2 / 2.0, ref eps_Point);
							RevitUtility.GetSurfaceContainingPoint(eps_Point, _PlacementSettings.SurfaceList, ref _dElevation);
							eps_Point.z = _dElevation;
							EPShared.eplGeoE_CalcPolarToXY(eps_Point, _dTangentDir + Math.PI, width, ref _NewPnt);
							RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlacementSettings.SurfaceList, ref _dElevation);
							_NewPnt.z = _dElevation;
							EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NewPnt, eps_Point, ref _dDistance2);
							double dVertical2 = eps_Point.z - _NewPnt.z;
							double num8 = RevitGeometry.CalculateDirection(_dDistance2, dVertical2);
							if (num8 <= -Math.PI / 2.0 && num8 >= Math.PI / 2.0)
							{
								_PlacementSettings.TransactionContainer.EndTransaction();
								RevitMessageBox.Show(EPShared.LocalRM.GetString("FenceError"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
								return;
							}
							FamilyInstance familyInstance2;
							if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingFenceList != null && _ExistingFenceList.Count > 0)
							{
								familyInstance2 = _ExistingFenceList[0];
								if (familyInstance2.Location is LocationPoint locationPoint2)
								{
									if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint2.Point)))
									{
										XYZ translation2 = new XYZ(_StaOffPnt.XYZ.X - locationPoint2.Point.X, _StaOffPnt.XYZ.Y - locationPoint2.Point.Y, 0.0);
										ElementTransformUtils.MoveElement(_Document, familyInstance2.Id, translation2);
									}
									num5 = locationPoint2.Rotation;
								}
								_ExistingFenceList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer2 = _PlacementSettings.TransactionContainer;
								familyInstance2 = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer2);
								_PlacementSettings.TransactionContainer = _TransactionContainer2;
							}
							if (Math.Abs(_PlacementSettings.Height) > 0.0)
							{
								Parameter parameter6 = familyInstance2.LookupParameter(RevitParameters.FenceFamilyHeightParameter);
								if (parameter6 != null)
								{
									num3 = parameter6.AsDouble();
									if (!RevitDefines.EQ(num3, _PlacementSettings.Height))
									{
										parameter6.Set(_PlacementSettings.Height);
									}
								}
							}
							if (Math.Abs(_PlacementSettings.Width) > 0.0)
							{
								Parameter parameter7 = familyInstance2.LookupParameter(RevitParameters.FenceFamilySpanParameter);
								if (parameter7 != null)
								{
									num4 = parameter7.AsDouble();
									if (!RevitDefines.EQ(num4, num2))
									{
										parameter7.Set(num2);
									}
								}
							}
							XYZ endpoint2 = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
							Line axis2 = Line.CreateBound(_StaOffPnt.XYZ, endpoint2);
							Parameter parameter8 = familyInstance2.LookupParameter(RevitParameters.FenceRightVisiability);
							Parameter parameter9 = familyInstance2.LookupParameter(RevitParameters.FenceLeftVisiability);
							bool value3 = true;
							bool value4 = true;
							if (num8 < 0.0)
							{
								if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI), EPShared.eplGeoE_NormalizeRadians(num5)))
								{
									ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis2, EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI - num5));
								}
							}
							else if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir), EPShared.eplGeoE_NormalizeRadians(num5)))
							{
								ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis2, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - num5));
							}
							if (parameter8 != null && parameter9 != null)
							{
								parameter8.Set(Convert.ToInt32(value3));
								parameter9.Set(Convert.ToInt32(value4));
							}
							num8 = Math.Abs(num8);
							familyInstance2.LookupParameter(RevitParameters.FenceFamilyAngleParameter)?.Set(num8);
							if (eps_Leg.Element != null && flag2 && flag)
							{
								Parameter parameter10 = familyInstance2.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter10 != null)
								{
									parameter10.Set(list2[num6].Value);
								}
								else
								{
									flag2 = false;
								}
							}
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(_PlacementSettings.Width));
						num5 = 0.0;
					}
					else
					{
						if (familyInstance != null)
						{
							Parameter parameter11 = familyInstance.LookupParameter(RevitParameters.FenceRightVisiability);
							Parameter parameter12 = familyInstance.LookupParameter(RevitParameters.FenceLeftVisiability);
							if (parameter11 != null && parameter12 != null)
							{
								parameter11.Set(Convert.ToInt32(value: true));
								parameter12.Set(Convert.ToInt32(value: true));
							}
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(_PlacementSettings.Width));
						num5 = 0.0;
					}
					num2 = 0.0;
				}
			}
			else if (eps_Leg.Arc != null)
			{
				_dDistance = eps_Leg.Arc.Radius * eps_Leg.Arc.Delta;
				if (num2 > _dDistance)
				{
					num2 -= _dDistance;
				}
				else
				{
					for (num = num2; num <= _dDistance; num += width)
					{
						if (num > _dDistance - width)
						{
							continue;
						}
						EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num + width / 2.0, 0.0, ref _StaOffPnt);
						if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
						{
							if (eps_Leg.Element != null && flag2 && flag3)
							{
								flag3 = false;
								if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
								{
									list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Leg.ElementList);
								}
								else
								{
									WarningOptionsForm warningOptionsForm3 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateFenceNoPositioning", EPShared.LocalRM.GetString("PositionFenceMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
									warningOptionsForm3.ShowDialog();
									flag2 = false;
								}
								if (num6 < list2.Count)
								{
									if (string.Compare(list2[num6].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag = true;
									}
									else
									{
										for (int k = 0; k < list2.Count; k++)
										{
											if (string.Compare(list2[k].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num6 = k;
												flag = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList, ref _dElevation);
							_StaOffPnt.z = _dElevation;
							EPShared.eplGeoE_CalcTangentDirOfArc(_StaOffPnt, eps_Leg.Arc.RadiusPnt, eps_Leg.Arc.Direction, ref _dTangentDir);
							EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, _dTangentDir, width / 2.0, ref eps_Point);
							RevitUtility.GetSurfaceContainingPoint(eps_Point, _PlacementSettings.SurfaceList, ref _dElevation);
							eps_Point.z = _dElevation;
							EPShared.eplGeoE_CalcPolarToXY(eps_Point, _dTangentDir + Math.PI, width, ref _NewPnt);
							RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlacementSettings.SurfaceList, ref _dElevation);
							_NewPnt.z = _dElevation;
							EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NewPnt, eps_Point, ref _dDistance2);
							double dVertical3 = eps_Point.z - _NewPnt.z;
							double num9 = RevitGeometry.CalculateDirection(_dDistance2, dVertical3);
							if (num9 <= -Math.PI / 2.0 && num9 >= Math.PI / 2.0)
							{
								_PlacementSettings.TransactionContainer.EndTransaction();
								RevitMessageBox.Show(EPShared.LocalRM.GetString("FenceError"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
								return;
							}
							FamilyInstance familyInstance2;
							if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingFenceList != null && _ExistingFenceList.Count > 0)
							{
								familyInstance2 = _ExistingFenceList[0];
								if (familyInstance2.Location is LocationPoint locationPoint3)
								{
									if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint3.Point)))
									{
										XYZ translation3 = new XYZ(_StaOffPnt.XYZ.X - locationPoint3.Point.X, _StaOffPnt.XYZ.Y - locationPoint3.Point.Y, 0.0);
										ElementTransformUtils.MoveElement(_Document, familyInstance2.Id, translation3);
									}
									num5 = locationPoint3.Rotation;
								}
								_ExistingFenceList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer3 = _PlacementSettings.TransactionContainer;
								familyInstance2 = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer3);
								_PlacementSettings.TransactionContainer = _TransactionContainer3;
							}
							familyInstance = familyInstance2;
							if (Math.Abs(_PlacementSettings.Height) > 0.0)
							{
								Parameter parameter13 = familyInstance2.LookupParameter(RevitParameters.FenceFamilyHeightParameter);
								if (parameter13 != null)
								{
									num3 = parameter13.AsDouble();
									if (!RevitDefines.EQ(num3, _PlacementSettings.Height))
									{
										parameter13.Set(_PlacementSettings.Height);
									}
								}
							}
							if (Math.Abs(_PlacementSettings.Width) > 0.0)
							{
								Parameter parameter14 = familyInstance2.LookupParameter(RevitParameters.FenceFamilySpanParameter);
								if (parameter14 != null)
								{
									num4 = parameter14.AsDouble();
									if (!RevitDefines.EQ(num4, _PlacementSettings.Width))
									{
										parameter14.Set(_PlacementSettings.Width);
									}
								}
							}
							XYZ endpoint3 = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
							Line axis3 = Line.CreateBound(_StaOffPnt.XYZ, endpoint3);
							Parameter parameter15 = familyInstance2.LookupParameter(RevitParameters.FenceRightVisiability);
							Parameter parameter16 = familyInstance2.LookupParameter(RevitParameters.FenceLeftVisiability);
							bool value5 = false;
							bool value6 = false;
							if (num9 < 0.0)
							{
								if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI), EPShared.eplGeoE_NormalizeRadians(num5)))
								{
									ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis3, EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI - num5));
								}
								value5 = true;
							}
							else
							{
								if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir), EPShared.eplGeoE_NormalizeRadians(num5)))
								{
									ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis3, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - num5));
								}
								value6 = true;
							}
							if (parameter15 != null && parameter16 != null)
							{
								parameter15.Set(Convert.ToInt32(value5));
								parameter16.Set(Convert.ToInt32(value6));
							}
							num9 = Math.Abs(num9);
							familyInstance2.LookupParameter(RevitParameters.FenceFamilyAngleParameter)?.Set(num9);
							if (eps_Leg.Element != null && flag2 && flag)
							{
								Parameter parameter17 = familyInstance2.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter17 != null)
								{
									parameter17.Set(list2[num6].Value);
								}
								else
								{
									flag2 = false;
								}
							}
						}
						else
						{
							familyInstance = null;
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(_PlacementSettings.Width));
						num5 = 0.0;
					}
					num2 = _dDistance - (num - width);
					if (RevitDefines.GT(num2, 0.001))
					{
						EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, _dDistance - num2 / 2.0, 0.0, ref _StaOffPnt);
						if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
						{
							if (eps_Leg.Element != null && flag2 && flag3)
							{
								flag3 = false;
								if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
								{
									list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Leg.ElementList);
								}
								else
								{
									WarningOptionsForm warningOptionsForm4 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateFenceNoPositioning", EPShared.LocalRM.GetString("PositionFenceMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
									warningOptionsForm4.ShowDialog();
									flag2 = false;
								}
								if (num6 < list2.Count)
								{
									if (string.Compare(list2[num6].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag = true;
									}
									else
									{
										for (int l = 0; l < list2.Count; l++)
										{
											if (string.Compare(list2[l].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num6 = l;
												flag = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList, ref _dElevation);
							_StaOffPnt.z = _dElevation;
							EPShared.eplGeoE_CalcTangentDirOfArc(_StaOffPnt, eps_Leg.Arc.RadiusPnt, eps_Leg.Arc.Direction, ref _dTangentDir);
							EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, _dTangentDir, num2 / 2.0, ref eps_Point);
							RevitUtility.GetSurfaceContainingPoint(eps_Point, _PlacementSettings.SurfaceList, ref _dElevation);
							eps_Point.z = _dElevation;
							EPShared.eplGeoE_CalcPolarToXY(eps_Point, _dTangentDir + Math.PI, width, ref _NewPnt);
							RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlacementSettings.SurfaceList, ref _dElevation);
							_NewPnt.z = _dElevation;
							EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NewPnt, eps_Point, ref _dDistance2);
							double dVertical4 = eps_Point.z - _NewPnt.z;
							double num10 = RevitGeometry.CalculateDirection(_dDistance2, dVertical4);
							if (num10 <= -Math.PI / 2.0 && num10 >= Math.PI / 2.0)
							{
								_PlacementSettings.TransactionContainer.EndTransaction();
								RevitMessageBox.Show(EPShared.LocalRM.GetString("FenceError"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
								return;
							}
							FamilyInstance familyInstance2;
							if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingFenceList != null && _ExistingFenceList.Count > 0)
							{
								familyInstance2 = _ExistingFenceList[0];
								if (familyInstance2.Location is LocationPoint locationPoint4)
								{
									if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint4.Point)))
									{
										XYZ translation4 = new XYZ(_StaOffPnt.XYZ.X - locationPoint4.Point.X, _StaOffPnt.XYZ.Y - locationPoint4.Point.Y, 0.0);
										ElementTransformUtils.MoveElement(_Document, familyInstance2.Id, translation4);
									}
									num5 = locationPoint4.Rotation;
								}
								_ExistingFenceList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer4 = _PlacementSettings.TransactionContainer;
								familyInstance2 = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer4);
								_PlacementSettings.TransactionContainer = _TransactionContainer4;
							}
							if (Math.Abs(_PlacementSettings.Height) > 0.0)
							{
								Parameter parameter18 = familyInstance2.LookupParameter(RevitParameters.FenceFamilyHeightParameter);
								if (parameter18 != null)
								{
									num3 = parameter18.AsDouble();
									if (!RevitDefines.EQ(num3, _PlacementSettings.Height))
									{
										parameter18.Set(_PlacementSettings.Height);
									}
								}
							}
							if (Math.Abs(_PlacementSettings.Width) > 0.0)
							{
								Parameter parameter19 = familyInstance2.LookupParameter(RevitParameters.FenceFamilySpanParameter);
								if (parameter19 != null)
								{
									num4 = parameter19.AsDouble();
									if (!RevitDefines.EQ(num4, num2))
									{
										parameter19.Set(num2);
									}
								}
							}
							XYZ endpoint4 = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
							Line axis4 = Line.CreateBound(_StaOffPnt.XYZ, endpoint4);
							Parameter parameter20 = familyInstance2.LookupParameter(RevitParameters.FenceRightVisiability);
							Parameter parameter21 = familyInstance2.LookupParameter(RevitParameters.FenceLeftVisiability);
							bool value7 = true;
							bool value8 = true;
							if (num10 < 0.0)
							{
								if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI), EPShared.eplGeoE_NormalizeRadians(num5)))
								{
									ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis4, EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI - num5));
								}
							}
							else if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir), EPShared.eplGeoE_NormalizeRadians(num5)))
							{
								ElementTransformUtils.RotateElement(_Document, familyInstance2.Id, axis4, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - num5));
							}
							if (parameter20 != null && parameter21 != null)
							{
								parameter20.Set(Convert.ToInt32(value7));
								parameter21.Set(Convert.ToInt32(value8));
							}
							num10 = Math.Abs(num10);
							familyInstance2.LookupParameter(RevitParameters.FenceFamilyAngleParameter)?.Set(num10);
							if (eps_Leg.Element != null && flag2 && flag)
							{
								Parameter parameter22 = familyInstance2.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter22 != null)
								{
									parameter22.Set(list2[num6].Value);
								}
								else
								{
									flag2 = false;
								}
							}
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(_PlacementSettings.Width));
						num5 = 0.0;
					}
					else
					{
						if (familyInstance != null)
						{
							Parameter parameter23 = familyInstance.LookupParameter(RevitParameters.FenceRightVisiability);
							Parameter parameter24 = familyInstance.LookupParameter(RevitParameters.FenceLeftVisiability);
							if (parameter23 != null && parameter24 != null)
							{
								parameter23.Set(Convert.ToInt32(value: true));
								parameter24.Set(Convert.ToInt32(value: true));
							}
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(_PlacementSettings.Width));
						num5 = 0.0;
					}
					num2 = 0.0;
				}
			}
			flag = false;
		}
		if (_ExistingFenceList != null && _ExistingFenceList.Count > 0)
		{
			for (int m = 0; m < _ExistingFenceList.Count; m++)
			{
				_Document.Delete(_ExistingFenceList[m].Id);
			}
		}
		if (_PlacementSettings.GenericModelIDList == null)
		{
			_PlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
	}
}
