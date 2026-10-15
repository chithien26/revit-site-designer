using System;
using System.Collections.Generic;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using SITES.Utilities;

namespace EPCommon;

public class ParkingLotCommon
{
	private static FamilyInstance DrawStall(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Point _InsertPoint, FamilySymbol _StallSymbol, double _dDirection, double _dStallLength, double _dStallWidth, double _dStallAngle, bool _bHideLeftStripe, bool _bHideRightStripe, ref List<FamilyInstance> _ExistingParkingStallList)
	{
		return DrawStall(_Document, _PlacementSettings, _Element, _InsertPoint, _StallSymbol, _dDirection, _dStallLength, _dStallWidth, _dStallAngle, _bHideLeftStripe, _bHideRightStripe, _bIsHCapStall: false, _bRightAisle: false, ref _ExistingParkingStallList);
	}

	private static FamilyInstance DrawStall(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Point _InsertPoint, FamilySymbol _StallSymbol, double _dDirection, double _dStallLength, double _dStallWidth, double _dStallAngle, bool _bHideLeftStripe, bool _bHideRightStripe, bool _bIsHCapStall, bool _bRightAisle, ref List<FamilyInstance> _ExistingParkingStallList)
	{
		if (_StallSymbol != null)
		{
			FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
			double num = 0.0;
			FamilyInstance familyInstance = null;
			Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_InsertPoint, _PlacementSettings.SurfaceList);
			if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingParkingStallList != null && _ExistingParkingStallList.Count > 0)
			{
				familyInstance = _ExistingParkingStallList[0];
				if (familyInstance.Location is LocationPoint locationPoint)
				{
					if (!EPShared.eplGeoE_ArePntsDuplicates3D(_InsertPoint, new Eps_Point(locationPoint.Point)))
					{
						XYZ translation = new XYZ(_InsertPoint.XYZ.X - locationPoint.Point.X, _InsertPoint.XYZ.Y - locationPoint.Point.Y, 0.0);
						ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
					}
					num = locationPoint.Rotation;
				}
				_ExistingParkingStallList.RemoveAt(0);
			}
			else
			{
				bool bUseFailureHandling = true;
				if (_PlacementSettings.TransactionContainer == null)
				{
					bUseFailureHandling = false;
				}
				EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
				familyInstance = RevitUtility.NewFamilyInstance(_Document, _InsertPoint.XY, _StallSymbol, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
				_PlacementSettings.TransactionContainer = _TransactionContainer;
			}
			if (_dStallLength > 0.0)
			{
				Parameter parameter = familyInstance.LookupParameter(RevitParameters.ParkingLotLength);
				if (parameter != null)
				{
					double dX = parameter.AsDouble();
					if (!RevitDefines.EQ(dX, _dStallLength))
					{
						parameter.Set(_dStallLength);
					}
				}
			}
			if (_dStallWidth > 0.0)
			{
				Parameter parameter = familyInstance.LookupParameter(RevitParameters.ParkingLotWidth);
				if (parameter != null)
				{
					double dX2 = parameter.AsDouble();
					if (!RevitDefines.EQ(dX2, _dStallWidth))
					{
						parameter.Set(_dStallWidth);
					}
				}
			}
			if (_dStallAngle >= 0.0)
			{
				familyInstance.LookupParameter(RevitParameters.ParkingLotAngle)?.Set(EPShared.eplGeoE_NormalizeRadians(_dStallAngle));
			}
			if (_bIsHCapStall)
			{
				familyInstance.LookupParameter(RevitParameters.ParkingLotRightAisle)?.Set(Convert.ToInt32(_bRightAisle));
			}
			familyInstance.LookupParameter(RevitParameters.ParkingLotLeftStripeVisible)?.Set(Convert.ToInt32(_bHideLeftStripe));
			familyInstance.LookupParameter(RevitParameters.ParkingLotRightStripeVisible)?.Set(Convert.ToInt32(_bHideRightStripe));
			if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dDirection), EPShared.eplGeoE_NormalizeRadians(num)))
			{
				XYZ endpoint = new XYZ(_InsertPoint.x, _InsertPoint.y, _InsertPoint.z + 1.0);
				Line axis = Line.CreateBound(_InsertPoint.XYZ, endpoint);
				ElementTransformUtils.RotateElement(_Document, familyInstance.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dDirection - num));
			}
			if (_Element != null && _PlacementSettings.GenericModelIDList != null)
			{
				int num2 = -1;
				for (int i = 0; i < _PlacementSettings.GenericModelIDList.Count; i++)
				{
					if (string.Compare(_PlacementSettings.GenericModelIDList[i].Key, _Element.UniqueId, ignoreCase: true) == 0)
					{
						num2 = i;
						break;
					}
				}
				if (num2 >= 0 && num2 < _PlacementSettings.GenericModelIDList.Count)
				{
					familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter)?.Set(_PlacementSettings.GenericModelIDList[num2].Value);
				}
			}
			if (!_bHideRightStripe)
			{
				return familyInstance;
			}
		}
		return null;
	}

	private static void DrawBumper(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Point _InsertPoint, double _dParkingWidth, double _dDirection, ref List<FamilyInstance> _ExistingBumperList)
	{
		if (_PlacementSettings.Parking.BumperSymbol == null)
		{
			return;
		}
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		double num = 0.0;
		FamilyInstance familyInstance = null;
		Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_InsertPoint, _PlacementSettings.SurfaceList);
		if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingBumperList != null && _ExistingBumperList.Count > 0)
		{
			familyInstance = _ExistingBumperList[0];
			if (familyInstance.Location is LocationPoint locationPoint)
			{
				if (!EPShared.eplGeoE_ArePntsDuplicates3D(_InsertPoint, new Eps_Point(locationPoint.Point)))
				{
					XYZ translation = new XYZ(_InsertPoint.XYZ.X - locationPoint.Point.X, _InsertPoint.XYZ.Y - locationPoint.Point.Y, 0.0);
					ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
				}
				num = locationPoint.Rotation;
			}
			_ExistingBumperList.RemoveAt(0);
		}
		else
		{
			bool bUseFailureHandling = true;
			if (_PlacementSettings.TransactionContainer == null)
			{
				bUseFailureHandling = false;
			}
			EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
			familyInstance = RevitUtility.NewFamilyInstance(_Document, _InsertPoint.XY, _PlacementSettings.Parking.BumperSymbol, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
			_PlacementSettings.TransactionContainer = _TransactionContainer;
		}
		if (_dParkingWidth - 2.0 * _PlacementSettings.Parking.BumperOffset > 0.0)
		{
			Parameter parameter = familyInstance.LookupParameter(RevitParameters.ParkingLotLength);
			if (parameter != null)
			{
				double dX = parameter.AsDouble();
				if (!RevitDefines.EQ(dX, _dParkingWidth - 2.0 * _PlacementSettings.Parking.BumperOffset))
				{
					parameter.Set(_dParkingWidth - 2.0 * _PlacementSettings.Parking.BumperOffset);
				}
			}
		}
		if (_PlacementSettings.Parking.BumperWidth > 0.0)
		{
			Parameter parameter2 = familyInstance.LookupParameter(RevitParameters.ParkingLotWidth);
			if (parameter2 != null)
			{
				double dX2 = parameter2.AsDouble();
				if (!RevitDefines.EQ(dX2, _PlacementSettings.Parking.BumperWidth))
				{
					parameter2.Set(_PlacementSettings.Parking.BumperWidth);
				}
			}
		}
		if (_PlacementSettings.Parking.BumperHeight > 0.0)
		{
			Parameter parameter3 = familyInstance.LookupParameter(RevitParameters.ParkingLotHeight);
			if (parameter3 != null)
			{
				double dX3 = parameter3.AsDouble();
				if (!RevitDefines.EQ(dX3, _PlacementSettings.Parking.BumperHeight))
				{
					parameter3.Set(_PlacementSettings.Parking.BumperHeight);
				}
			}
		}
		if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dDirection), EPShared.eplGeoE_NormalizeRadians(num)))
		{
			XYZ endpoint = new XYZ(_InsertPoint.x, _InsertPoint.y, _InsertPoint.z + 1.0);
			Line axis = Line.CreateBound(_InsertPoint.XYZ, endpoint);
			ElementTransformUtils.RotateElement(_Document, familyInstance.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dDirection - num));
		}
		if (_Element == null || _PlacementSettings.GenericModelIDList == null)
		{
			return;
		}
		int num2 = -1;
		for (int i = 0; i < _PlacementSettings.GenericModelIDList.Count; i++)
		{
			if (string.Compare(_PlacementSettings.GenericModelIDList[i].Key, _Element.UniqueId, ignoreCase: true) == 0)
			{
				num2 = i;
				break;
			}
		}
		if (num2 >= 0 && num2 < _PlacementSettings.GenericModelIDList.Count)
		{
			familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter)?.Set(_PlacementSettings.GenericModelIDList[num2].Value);
		}
	}

	private static void DrawIsland(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Point _InsertPoint, double _dIslandWidth, double _dIslandAngle, double _dStallLength, double _dDirection, ref List<FamilyInstance> _ExistingIslandList)
	{
		DrawIsland(_Document, _PlacementSettings, _Element, _InsertPoint, 0.0, _dIslandWidth, _dIslandAngle, _dStallLength, _dDirection, ref _ExistingIslandList);
	}

	private static void DrawIsland(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Point _InsertPoint, double _dIslandLength, double _dIslandWidth, double _dIslandAngle, double _dStallLength, double _dDirection, ref List<FamilyInstance> _ExistingIslandList)
	{
		if (_PlacementSettings.Parking.IslandSymbol == null)
		{
			return;
		}
		double num = 0.0;
		FamilyInstance familyInstance = null;
		Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_InsertPoint, _PlacementSettings.SurfaceList);
		if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingIslandList != null && _ExistingIslandList.Count > 0)
		{
			familyInstance = _ExistingIslandList[0];
			if (familyInstance.Location is LocationPoint locationPoint)
			{
				if (!EPShared.eplGeoE_ArePntsDuplicates3D(_InsertPoint, new Eps_Point(locationPoint.Point)))
				{
					XYZ translation = new XYZ(_InsertPoint.XYZ.X - locationPoint.Point.X, _InsertPoint.XYZ.Y - locationPoint.Point.Y, 0.0);
					ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
				}
				num = locationPoint.Rotation;
			}
			_ExistingIslandList.RemoveAt(0);
		}
		else
		{
			bool bUseFailureHandling = true;
			if (_PlacementSettings.TransactionContainer == null)
			{
				bUseFailureHandling = false;
			}
			EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
			familyInstance = RevitUtility.NewFamilyInstance(_Document, _InsertPoint.XY, _PlacementSettings.Parking.IslandSymbol, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
			_PlacementSettings.TransactionContainer = _TransactionContainer;
		}
		if (_dIslandLength > 0.0)
		{
			familyInstance.LookupParameter(RevitParameters.ParkingLotLength)?.Set(_dIslandLength);
		}
		if (_dIslandWidth > 0.0)
		{
			familyInstance.LookupParameter(RevitParameters.ParkingLotWidth)?.Set(_dIslandWidth);
		}
		if (_PlacementSettings.Parking.IslandHeight > 0.0)
		{
			familyInstance.LookupParameter(RevitParameters.ParkingLotHeight)?.Set(_PlacementSettings.Parking.IslandHeight);
		}
		if (_dIslandAngle >= 0.0)
		{
			familyInstance.LookupParameter(RevitParameters.ParkingLotAngle)?.Set(_dIslandAngle);
		}
		if (_dStallLength > 0.0)
		{
			familyInstance.LookupParameter(RevitParameters.ParkingLotStallLength)?.Set(_dStallLength);
		}
		if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dDirection), EPShared.eplGeoE_NormalizeRadians(num)))
		{
			XYZ endpoint = new XYZ(_InsertPoint.x, _InsertPoint.y, _InsertPoint.z + 1.0);
			Line axis = Line.CreateBound(_InsertPoint.XYZ, endpoint);
			ElementTransformUtils.RotateElement(_Document, familyInstance.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dDirection - num));
		}
		if (_Element == null || _PlacementSettings.GenericModelIDList == null)
		{
			return;
		}
		int num2 = -1;
		for (int i = 0; i < _PlacementSettings.GenericModelIDList.Count; i++)
		{
			if (string.Compare(_PlacementSettings.GenericModelIDList[i].Key, _Element.UniqueId, ignoreCase: true) == 0)
			{
				num2 = i;
				break;
			}
		}
		if (num2 >= 0 && num2 < _PlacementSettings.GenericModelIDList.Count)
		{
			familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter)?.Set(_PlacementSettings.GenericModelIDList[num2].Value);
		}
	}

	private static void GetIslandParameters(Document _Document, ref PlacementSettings _PlacementSettings)
	{
		if (_PlacementSettings.Parking.IslandSymbol == null || !(_PlacementSettings.Parking.IslandWidth > 0.0))
		{
			return;
		}
		Eps_Point eps_Point = new Eps_Point();
		bool bUseFailureHandling = true;
		if (_PlacementSettings.TransactionContainer == null)
		{
			bUseFailureHandling = false;
		}
		EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
		FamilyInstance familyInstance = RevitUtility.NewFamilyInstance(_Document, eps_Point.XY, _PlacementSettings.Parking.IslandSymbol, null, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
		_PlacementSettings.TransactionContainer = _TransactionContainer;
		if (familyInstance != null)
		{
			Parameter parameter = familyInstance.LookupParameter(RevitParameters.ParkingLotCenterIsland);
			if (parameter != null)
			{
				_PlacementSettings.Parking.CenterIsland = Convert.ToBoolean(parameter.AsInteger());
			}
			parameter = familyInstance.LookupParameter(RevitParameters.ParkingLotHasSides);
			if (parameter != null)
			{
				_PlacementSettings.Parking.SidesOnIsland = Convert.ToBoolean(parameter.AsInteger());
			}
			if (_PlacementSettings.TransactionContainer != null)
			{
				_PlacementSettings.TransactionContainer.BeginTransaction();
			}
			_Document.Delete(familyInstance.Id);
			if (_PlacementSettings.TransactionContainer != null)
			{
				_PlacementSettings.TransactionContainer.EndTransaction();
			}
		}
	}

	private static void GetAisleWidthAdjustment(Document _Document, ref PlacementSettings _PlacementSettings)
	{
		if (!_PlacementSettings.Parking.GetAisleWidthAdjustment && _PlacementSettings.Parking.ADAStallCount > 0 && _PlacementSettings.Parking.ADAStallSymbol != null)
		{
			Eps_Point eps_Point = new Eps_Point();
			bool bUseFailureHandling = true;
			if (_PlacementSettings.TransactionContainer == null)
			{
				bUseFailureHandling = false;
			}
			EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
			FamilyInstance familyInstance = RevitUtility.NewFamilyInstance(_Document, eps_Point.XY, _PlacementSettings.Parking.ADAStallSymbol, null, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
			_PlacementSettings.TransactionContainer = _TransactionContainer;
			if (familyInstance != null)
			{
				Parameter parameter = familyInstance.LookupParameter(RevitParameters.ParkingLotAisleWidth);
				if (parameter != null)
				{
					_PlacementSettings.Parking.AdjustAisleWidth = parameter.AsDouble();
				}
				if (_PlacementSettings.TransactionContainer != null)
				{
					_PlacementSettings.TransactionContainer.BeginTransaction();
				}
				_Document.Delete(familyInstance.Id);
				if (_PlacementSettings.TransactionContainer != null)
				{
					_PlacementSettings.TransactionContainer.EndTransaction();
				}
			}
		}
		_PlacementSettings.Parking.GetAisleWidthAdjustment = true;
	}

	private static int CalculateNumberOfStalls(ref PlacementSettings _PlacementSettings, Eps_Line _CurLine, bool _bDoubleRowStallAdjustment)
	{
		int result = 0;
		int num = 0;
		int num2 = 0;
		double _dDistance = 0.0;
		double num3 = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_CurLine.StartPnt, _CurLine.EndPnt, ref _dDistance);
		_PlacementSettings.Parking.CurrentADAStallCount = 0;
		_PlacementSettings.Parking.CurrentStallCount = 0;
		num3 = ((!_PlacementSettings.Parking.ADAStallSharedAccessibleAisle) ? _PlacementSettings.Parking.ADAStallAdjustWidth : ((_PlacementSettings.Parking.ADAStallAdjustWidth + _PlacementSettings.Parking.ADAStallWidth) / 2.0));
		if (_PlacementSettings.Parking.ADAStallCount >= 1)
		{
			num = (int)(_dDistance / num3);
			if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
			{
				num2 = Math.DivRem(num, 2, out result);
				if (result > 0 && _dDistance < _PlacementSettings.Parking.ADAStallWidth * (double)num2 + _PlacementSettings.Parking.ADAStallAdjustWidth * (double)(num2 + 1))
				{
					num--;
				}
			}
			if (_bDoubleRowStallAdjustment)
			{
				if (2 * num < _PlacementSettings.Parking.ADAStallCount)
				{
					_PlacementSettings.Parking.CurrentADAStallCount = num;
				}
				else if (_PlacementSettings.Parking.ADAStallCount > 0)
				{
					_PlacementSettings.Parking.CurrentADAStallCount = Math.DivRem(_PlacementSettings.Parking.ADAStallCount, 2, out result);
					if (result > 0)
					{
						_PlacementSettings.Parking.CurrentADAStallCount += result;
					}
				}
			}
			else if (num < _PlacementSettings.Parking.ADAStallCount)
			{
				_PlacementSettings.Parking.CurrentADAStallCount = num;
			}
			else
			{
				_PlacementSettings.Parking.CurrentADAStallCount = _PlacementSettings.Parking.ADAStallCount;
			}
			if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
			{
				num2 = Math.DivRem(_PlacementSettings.Parking.CurrentADAStallCount, 2, out result);
				_dDistance = ((result <= 0) ? (_dDistance - (double)_PlacementSettings.Parking.CurrentADAStallCount * num3) : (_dDistance - (_PlacementSettings.Parking.ADAStallWidth * (double)num2 + _PlacementSettings.Parking.ADAStallAdjustWidth * (double)(num2 + 1))));
			}
			else
			{
				_dDistance -= (double)_PlacementSettings.Parking.CurrentADAStallCount * num3;
			}
		}
		if (RevitDefines.GT(_dDistance, 0.0))
		{
			num = (int)(_dDistance / _PlacementSettings.Parking.StallAdjustWidth);
			if (num > 0)
			{
				_PlacementSettings.Parking.CurrentStallCount = num;
			}
		}
		if (!_PlacementSettings.Parking.OptimizeParking)
		{
			if (_PlacementSettings.Parking.CurrentADAStallCount > _PlacementSettings.Parking.ADAStallCount)
			{
				_PlacementSettings.Parking.CurrentADAStallCount = _PlacementSettings.Parking.ADAStallCount;
			}
			if (_PlacementSettings.Parking.CurrentStallCount > _PlacementSettings.Parking.StallCount)
			{
				_PlacementSettings.Parking.CurrentStallCount = _PlacementSettings.Parking.StallCount;
			}
			if (_PlacementSettings.Parking.CurrentStallCount > 0 && _bDoubleRowStallAdjustment)
			{
				_PlacementSettings.Parking.CurrentStallCount = Math.DivRem(_PlacementSettings.Parking.CurrentStallCount, 2, out result);
				if (result > 0)
				{
					_PlacementSettings.Parking.CurrentStallCount += result;
				}
			}
		}
		return _PlacementSettings.Parking.CurrentStallCount + _PlacementSettings.Parking.CurrentADAStallCount;
	}

	private static void CalculateStallLength(ref PlacementSettings _PlacementSettings, double _dParkingAngle)
	{
		double num = 0.0;
		num = Math.PI - _dParkingAngle - Math.PI / 2.0;
		if (RevitDefines.GT(_dParkingAngle, Math.PI / 2.0))
		{
			num = 0.0 - num;
		}
		if (RevitDefines.EQ(num, Math.PI / 2.0))
		{
			_PlacementSettings.Parking.StripeLength = _PlacementSettings.Parking.StallLength;
		}
		else if ((_PlacementSettings.Parking.CurrentADAStallCount > 0 && RevitDefines.GE(_PlacementSettings.Parking.ADAStallWidth, _PlacementSettings.Width) && _PlacementSettings.Parking.CurrentStallCount > 0) || _PlacementSettings.Parking.CurrentStallCount <= 0)
		{
			_PlacementSettings.Parking.StripeLength = (_PlacementSettings.Parking.ADAStallAdjustWidthNoAisleWidth - _PlacementSettings.Parking.ADAStallWidth) / Math.Abs(Math.Cos(num)) + _PlacementSettings.Parking.StallLength;
		}
		else
		{
			_PlacementSettings.Parking.StripeLength = (_PlacementSettings.Parking.StallAdjustWidth - _PlacementSettings.Width) / Math.Abs(Math.Cos(num)) + _PlacementSettings.Parking.StallLength;
		}
	}

	private static void CalculateStallParameters(Document _Document, ref PlacementSettings _PlacementSettings, double _dParkingAngle)
	{
		double num = 0.0;
		num = Math.PI - _dParkingAngle - Math.PI / 2.0;
		if (RevitDefines.GT(_dParkingAngle, Math.PI / 2.0))
		{
			num = 0.0 - num;
		}
		GetAisleWidthAdjustment(_Document, ref _PlacementSettings);
		_PlacementSettings.Parking.ADAStallAdjustWidthNoAisleWidth = _PlacementSettings.Parking.ADAStallWidth / Math.Cos(num);
		_PlacementSettings.Parking.ADAStallAisleAdjustWidth = _PlacementSettings.Parking.AdjustAisleWidth / Math.Cos(num);
		_PlacementSettings.Parking.ADAStallAdjustWidth = (_PlacementSettings.Parking.ADAStallWidth + _PlacementSettings.Parking.AdjustAisleWidth) / Math.Cos(num);
		_PlacementSettings.Parking.StallAdjustWidth = _PlacementSettings.Width / Math.Cos(num);
	}

	private static int CalculateNumberOfRows(PlacementSettings _PlacementSettings, Eps_Line _CurLine, ref double AdjRowHeight)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		AdjRowHeight = 0.0;
		if (RevitDefines.EQ(_PlacementSettings.RotationAngle, Math.PI) || RevitDefines.EQ(_PlacementSettings.RotationAngle, 0.0))
		{
			return 0;
		}
		if (_PlacementSettings.Parking.AreaRowSpacingOptions == 0)
		{
			AdjRowHeight = _PlacementSettings.Parking.AreaDistanceBetweenCenterlines;
		}
		else
		{
			num3 = _PlacementSettings.Parking.StripeLength;
			num2 = _PlacementSettings.Parking.AreaAisleWidth;
			if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_PlacementSettings.RotationAngle), Math.PI / 2.0) && !RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_PlacementSettings.RotationAngle), 4.71238898038469))
			{
				num = Math.PI - _PlacementSettings.RotationAngle - Math.PI / 2.0;
				if (RevitDefines.GT(_PlacementSettings.RotationAngle, Math.PI / 2.0))
				{
					num = 0.0 - num;
				}
				num3 = _PlacementSettings.Parking.StripeLength * Math.Cos(num);
			}
			AdjRowHeight = 2.0 * num3 + num2;
		}
		return _PlacementSettings.Parking.AreaRowCount;
	}

	private static bool IsStallWithinStallLength(Eps_Leg _NextLeg, Eps_Point _StallPoint, double _dParkingLength)
	{
		double _dStation = 0.0;
		double _dOffset = 0.0;
		bool result = false;
		if (_NextLeg != null)
		{
			if (_NextLeg.Line != null && EPShared.eplGeoE_CalcStationOffsetAlongLine(_StallPoint, _NextLeg.Line, ref _dStation, ref _dOffset, 0.0) == 1700)
			{
				if (RevitDefines.LT(Math.Abs(_dOffset), _dParkingLength))
				{
					result = true;
				}
			}
			else if (_NextLeg.Arc != null && EPShared.eplGeoE_CalcStationOffsetAlongArc(_StallPoint, _NextLeg.Arc, ref _dStation, ref _dOffset, 0.0) == 1700 && RevitDefines.LT(Math.Abs(_dOffset), _dParkingLength))
			{
				result = true;
			}
		}
		return result;
	}

	private static void CreateSingleStalls(Document _Document, ref PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, Element _Element, Eps_Line _CurLine, double SideAngle, bool _bDoubleRowStallAdjustment, ref Eps_Point _LastCurbPoint, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList, bool _bDrawFirstDoubleStripe)
	{
		Eps_Leg _RemainingLeg = new Eps_Leg();
		CreateSingleStalls(_Document, ref _PlacementSettings, _ProgressFrm, _Element, _CurLine, SideAngle, _bDoubleRowStallAdjustment, null, ref _RemainingLeg, ref _LastCurbPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, ref _bDrawFirstDoubleStripe);
	}

	private static void CreateSingleStalls(Document _Document, ref PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, Element _Element, Eps_Line _CurLine, double SideAngle, bool _bDoubleRowStallAdjustment, Eps_Leg _NextLeg, ref Eps_Leg RemainingLeg, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList, ref bool _bDrawFirstDoubleStripe)
	{
		Eps_Point _LastCurbPoint = new Eps_Point();
		CreateSingleStalls(_Document, ref _PlacementSettings, _ProgressFrm, _Element, _CurLine, SideAngle, _bDoubleRowStallAdjustment, _NextLeg, ref RemainingLeg, ref _LastCurbPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, ref _bDrawFirstDoubleStripe);
	}

	private static void CreateSingleStalls(Document _Document, ref PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, Element _Element, Eps_Line _CurLine, double SideAngle, bool _bDoubleRowStallAdjustment, Eps_Leg _NextLeg, ref Eps_Leg _RemainingLeg, ref Eps_Point _LastCurbPoint, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList, ref bool _bDrawFirstDoubleStripe)
	{
		Eps_Line eps_Line = new Eps_Line();
		Eps_Point _NewPnt = new Eps_Point();
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point _NewPnt2 = new Eps_Point();
		Eps_Point _NewPnt3 = new Eps_Point();
		double num = 0.0;
		double _dDirection = 0.0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		int num2 = 0;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		FamilyInstance familyInstance = null;
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(_CurLine.StartPnt, _CurLine.EndPnt, ref _dDirection);
		CalculateStallParameters(_Document, ref _PlacementSettings, _PlacementSettings.RotationAngle);
		if (CalculateNumberOfStalls(ref _PlacementSettings, _CurLine, _bDoubleRowStallAdjustment) < 1)
		{
			return;
		}
		_ProgressFrm.ResetValue();
		_ProgressFrm.MinValue = 0;
		if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
		{
			_ProgressFrm.MaxValue = Convert.ToInt32(_PlacementSettings.Parking.ADAStallAdjustWidth * (double)(_PlacementSettings.Parking.CurrentADAStallCount / 2) + _PlacementSettings.Parking.ADAStallWidth * (double)(_PlacementSettings.Parking.CurrentADAStallCount / 2) + _PlacementSettings.Parking.StallAdjustWidth * (double)_PlacementSettings.Parking.CurrentStallCount);
		}
		else
		{
			_ProgressFrm.MaxValue = Convert.ToInt32(_PlacementSettings.Parking.ADAStallAdjustWidth * (double)_PlacementSettings.Parking.CurrentADAStallCount + _PlacementSettings.Parking.StallAdjustWidth * (double)_PlacementSettings.Parking.CurrentStallCount);
		}
		Thread.Sleep(300);
		CalculateStallLength(ref _PlacementSettings, _PlacementSettings.RotationAngle);
		eps_Point = _CurLine.StartPnt;
		EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection + SideAngle), _PlacementSettings.Parking.StripeLength, ref _NewPnt);
		if (_RemainingLeg != null)
		{
			if (_RemainingLeg.Line != null && EPShared.eplGeoE_CalcStationOffsetAlongLine(_NewPnt, _RemainingLeg.Line, ref _dStation, ref _dOffset, 0.0) == 1700)
			{
				EPShared.eplGeoE_FindStaOffPointAlongLine(_RemainingLeg.Line, _dStation, 0.0, ref _NewPnt);
				_RemainingLeg.Line.StartPnt = _NewPnt;
			}
			else if (_RemainingLeg.Arc != null && EPShared.eplGeoE_CalcStationOffsetAlongArc(_NewPnt, _RemainingLeg.Arc, ref _dStation, ref _dOffset, 0.0) == 1700)
			{
				EPShared.eplGeoE_FindStaOffPointAlongArc(_RemainingLeg.Arc, _dStation, 0.0, ref _NewPnt);
				_RemainingLeg.Arc.StartPnt = _NewPnt;
				double _dDistance = 0.0;
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_RemainingLeg.Arc.StartPnt, _RemainingLeg.Arc.EndPnt, ref _dDistance);
				double _dDeltaAngle = 0.0;
				EPShared.eplGeoE_SolveDeltaByChordLenAndRadius(_dDistance, _RemainingLeg.Arc.Radius, ref _dDeltaAngle);
				_RemainingLeg.Arc.Delta = _dDeltaAngle;
			}
		}
		if (RevitDefines.LT(SideAngle, Math.PI) && RevitDefines.GE(SideAngle, 0.0))
		{
			num = EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI);
			flag = true;
		}
		else
		{
			num = _dDirection;
			flag = false;
		}
		bool flag4 = true;
		double num3 = _PlacementSettings.Parking.ADAStallAdjustWidth;
		double aDAStallWidth = _PlacementSettings.Parking.ADAStallWidth;
		FamilySymbol stallSymbol = _PlacementSettings.Parking.ADAStallSymbol;
		for (num2 = 0; num2 < _PlacementSettings.Parking.CurrentADAStallCount; num2++)
		{
			EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection), num3, ref _NewPnt2);
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(_dDirection + SideAngle), _PlacementSettings.Parking.StripeLength, ref _NewPnt3);
			if (!IsStallWithinStallLength(_NextLeg, _NewPnt3, _PlacementSettings.Parking.StallLength))
			{
				if (RevitDefines.LT(SideAngle, Math.PI) && RevitDefines.GE(SideAngle, 0.0))
				{
					if (RevitDefines.LT(_PlacementSettings.RotationAngle, Math.PI / 2.0) && RevitDefines.GE(_PlacementSettings.RotationAngle, 0.0))
					{
						if (flag4 && !flag && _PlacementSettings.Parking.ADAStallAisleAdjustWidth > 0.0)
						{
							EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection), _PlacementSettings.Parking.ADAStallAisleAdjustWidth, ref _NewPnt);
							eps_Line.StartPnt = _NewPnt;
						}
						else
						{
							eps_Line.StartPnt = eps_Point;
						}
						EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth, ref _NewPnt);
						eps_Line.EndPnt = _NewPnt;
					}
					else
					{
						if (flag4 && flag && _PlacementSettings.Parking.ADAStallAisleAdjustWidth > 0.0)
						{
							EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(Math.PI + _dDirection), _PlacementSettings.Parking.ADAStallAisleAdjustWidth, ref _NewPnt);
							eps_Line.EndPnt = _NewPnt;
						}
						else
						{
							eps_Line.EndPnt = _NewPnt2;
						}
						EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth, ref _NewPnt);
						eps_Line.StartPnt = _NewPnt;
					}
				}
				else if (RevitDefines.LT(_PlacementSettings.RotationAngle, Math.PI / 2.0) && RevitDefines.GE(_PlacementSettings.RotationAngle, 0.0))
				{
					if (flag4 && !flag && _PlacementSettings.Parking.ADAStallAisleAdjustWidth > 0.0)
					{
						EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(Math.PI + _dDirection), _PlacementSettings.Parking.ADAStallAisleAdjustWidth, ref _NewPnt);
						eps_Line.EndPnt = _NewPnt;
					}
					else
					{
						eps_Line.EndPnt = _NewPnt2;
					}
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth, ref _NewPnt);
					eps_Line.StartPnt = _NewPnt;
				}
				else
				{
					if (flag4 && flag && _PlacementSettings.Parking.ADAStallAisleAdjustWidth > 0.0)
					{
						EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(Math.PI + _dDirection), _PlacementSettings.Parking.ADAStallAisleAdjustWidth, ref _NewPnt);
						eps_Line.StartPnt = _NewPnt;
					}
					else
					{
						eps_Line.StartPnt = eps_Point;
					}
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth, ref _NewPnt);
					eps_Line.EndPnt = _NewPnt;
				}
				if (_PlacementSettings.Parking.PlaceBumper)
				{
					if (RevitDefines.LT(SideAngle, Math.PI) && RevitDefines.GE(SideAngle, 0.0))
					{
						EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
						EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth / 2.0, ref _NewPnt);
						DrawBumper(_Document, _PlacementSettings, _Element, _NewPnt, aDAStallWidth, Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
					}
					else
					{
						EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI * 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
						EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth / 2.0, ref _NewPnt);
						DrawBumper(_Document, _PlacementSettings, _Element, _NewPnt, aDAStallWidth, 4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
					}
				}
				if (flag)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection), num3, ref _NewPnt);
					flag2 = false;
					flag3 = true;
					if (!_PlacementSettings.Parking.SidesOnIsland && num2 == 0 && _bDrawFirstDoubleStripe)
					{
						flag2 = true;
					}
					else if (_PlacementSettings.Parking.SidesOnIsland && num2 == _PlacementSettings.Parking.CurrentADAStallCount - 1)
					{
						flag3 = false;
					}
				}
				else
				{
					_NewPnt = eps_Point;
					flag2 = true;
					flag3 = false;
					if (!_PlacementSettings.Parking.SidesOnIsland && num2 == 0 && _bDrawFirstDoubleStripe)
					{
						flag3 = true;
					}
					else if (_PlacementSettings.Parking.SidesOnIsland && num2 == _PlacementSettings.Parking.CurrentADAStallCount - 1)
					{
						flag2 = false;
					}
				}
				familyInstance = ((!flag4) ? DrawStall(_Document, _PlacementSettings, _Element, _NewPnt, stallSymbol, num, _PlacementSettings.Parking.StallLength, aDAStallWidth, _PlacementSettings.RotationAngle, flag3, flag2, flag4, !flag, ref _ExistingParkingStallList) : DrawStall(_Document, _PlacementSettings, _Element, _NewPnt, stallSymbol, num, _PlacementSettings.Parking.StallLength, aDAStallWidth, _PlacementSettings.RotationAngle, flag3, flag2, flag4, !flag, ref _ExistingADAParkingStallList));
				eps_Point = _NewPnt2;
				_ProgressFrm.UpdateProgressBar(Convert.ToInt32(num3));
				if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
				{
					flag4 = !flag4;
					if (flag4)
					{
						num3 = _PlacementSettings.Parking.ADAStallAdjustWidth;
						stallSymbol = _PlacementSettings.Parking.ADAStallSymbol;
					}
					else
					{
						num3 = _PlacementSettings.Parking.ADAStallWidth;
						stallSymbol = _PlacementSettings.Type;
					}
				}
				continue;
			}
			if (!_PlacementSettings.Parking.SidesOnIsland && familyInstance != null)
			{
				familyInstance.LookupParameter(RevitParameters.RightVisibleParameter)?.Set(Convert.ToInt32(value: true));
				familyInstance = null;
			}
			_PlacementSettings.Parking.CurrentADAStallCount = num2;
			break;
		}
		for (num2 = 0; num2 < _PlacementSettings.Parking.CurrentStallCount; num2++)
		{
			EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection), _PlacementSettings.Parking.StallAdjustWidth, ref _NewPnt2);
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(_dDirection + SideAngle), _PlacementSettings.Parking.StripeLength, ref _NewPnt3);
			if (!IsStallWithinStallLength(_NextLeg, _NewPnt3, _PlacementSettings.Parking.StallLength))
			{
				if (RevitDefines.LT(SideAngle, Math.PI) && RevitDefines.GE(SideAngle, 0.0))
				{
					if (RevitDefines.LT(_PlacementSettings.RotationAngle, Math.PI / 2.0) && RevitDefines.GE(_PlacementSettings.RotationAngle, 0.0))
					{
						eps_Line.StartPnt = eps_Point;
						EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width, ref _NewPnt);
						eps_Line.EndPnt = _NewPnt;
					}
					else
					{
						eps_Line.EndPnt = _NewPnt2;
						EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width, ref _NewPnt);
						eps_Line.StartPnt = _NewPnt;
					}
				}
				else if (RevitDefines.LT(_PlacementSettings.RotationAngle, Math.PI / 2.0) && RevitDefines.GE(_PlacementSettings.RotationAngle, 0.0))
				{
					eps_Line.EndPnt = _NewPnt2;
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width, ref _NewPnt);
					eps_Line.StartPnt = _NewPnt;
				}
				else
				{
					eps_Line.StartPnt = eps_Point;
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width, ref _NewPnt);
					eps_Line.EndPnt = _NewPnt;
				}
				if (_PlacementSettings.Parking.PlaceBumper)
				{
					if (RevitDefines.LT(SideAngle, Math.PI) && RevitDefines.GE(SideAngle, 0.0))
					{
						EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
						EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width / 2.0, ref _NewPnt);
						DrawBumper(_Document, _PlacementSettings, _Element, _NewPnt, _PlacementSettings.Width, Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
					}
					else
					{
						EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI * 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
						EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width / 2.0, ref _NewPnt);
						DrawBumper(_Document, _PlacementSettings, _Element, _NewPnt, _PlacementSettings.Width, 4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
					}
				}
				if (flag)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection), _PlacementSettings.Parking.StallAdjustWidth, ref _NewPnt);
					flag2 = false;
					flag3 = true;
					if (!_PlacementSettings.Parking.SidesOnIsland && num2 == 0 && _PlacementSettings.Parking.CurrentADAStallCount == 0 && _bDrawFirstDoubleStripe)
					{
						flag2 = true;
					}
					else if (_PlacementSettings.Parking.SidesOnIsland && num2 == _PlacementSettings.Parking.CurrentStallCount - 1)
					{
						flag3 = false;
					}
				}
				else
				{
					_NewPnt = eps_Point;
					flag2 = true;
					flag3 = false;
					if (!_PlacementSettings.Parking.SidesOnIsland && num2 == 0 && _PlacementSettings.Parking.CurrentADAStallCount == 0 && _bDrawFirstDoubleStripe)
					{
						flag3 = true;
					}
					else if (_PlacementSettings.Parking.SidesOnIsland && num2 == _PlacementSettings.Parking.CurrentStallCount - 1)
					{
						flag2 = false;
					}
				}
				familyInstance = DrawStall(_Document, _PlacementSettings, _Element, _NewPnt, _PlacementSettings.Type, num, _PlacementSettings.Parking.StallLength, _PlacementSettings.Width, _PlacementSettings.RotationAngle, flag3, flag2, ref _ExistingParkingStallList);
				eps_Point = _NewPnt2;
				_ProgressFrm.UpdateProgressBar(Convert.ToInt32(_PlacementSettings.Parking.StallAdjustWidth));
				continue;
			}
			if (!_PlacementSettings.Parking.SidesOnIsland && familyInstance != null)
			{
				familyInstance.LookupParameter(RevitParameters.RightVisibleParameter)?.Set(Convert.ToInt32(value: true));
				familyInstance = null;
			}
			_PlacementSettings.Parking.CurrentStallCount = num2;
			break;
		}
		if (!_PlacementSettings.Parking.OptimizeParking)
		{
			_PlacementSettings.Parking.StallCount -= _PlacementSettings.Parking.CurrentStallCount;
		}
		_PlacementSettings.Parking.ADAStallCount -= _PlacementSettings.Parking.CurrentADAStallCount;
		if (_NextLeg != null)
		{
			if (_NextLeg.Line != null && EPShared.eplGeoE_CalcStationOffsetAlongLine(_NewPnt3, _NextLeg.Line, ref _dStation, ref _dOffset, 0.0) == 1700)
			{
				EPShared.eplGeoE_FindStaOffPointAlongLine(_NextLeg.Line, _dStation, 0.0, ref _NewPnt);
				_NextLeg.Line.StartPnt = _NewPnt;
			}
			else if (_NextLeg.Arc != null && EPShared.eplGeoE_CalcStationOffsetAlongArc(_NewPnt3, _NextLeg.Arc, ref _dStation, ref _dOffset, 0.0) == 1700)
			{
				EPShared.eplGeoE_FindStaOffPointAlongArc(_NextLeg.Arc, _dStation, 0.0, ref _NewPnt);
				_NextLeg.Arc.StartPnt = _NewPnt;
				double _dDistance2 = 0.0;
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NextLeg.Arc.StartPnt, _NextLeg.Arc.EndPnt, ref _dDistance2);
				double _dDeltaAngle2 = 0.0;
				EPShared.eplGeoE_SolveDeltaByChordLenAndRadius(_dDistance2, _NextLeg.Arc.Radius, ref _dDeltaAngle2);
				_NextLeg.Arc.Delta = _dDeltaAngle2;
			}
		}
		_LastCurbPoint = eps_Point;
		_bDrawFirstDoubleStripe = true;
	}

	private static void CreateDoubleStalls(Document _Document, ref PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, Element _Element, Eps_Line _CurLine, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList, ref List<FamilyInstance> _ExistingIslandList)
	{
		Eps_Line eps_Line = new Eps_Line();
		Eps_Point _NewPnt = new Eps_Point();
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		int num = 0;
		bool flag = false;
		bool flag2 = true;
		if (_CurLine == null)
		{
			return;
		}
		bool flag3 = true;
		double num2 = _PlacementSettings.Parking.ADAStallAdjustWidth;
		double aDAStallWidth = _PlacementSettings.Parking.ADAStallWidth;
		double aDAStallAdjustWidthNoAisleWidth = _PlacementSettings.Parking.ADAStallAdjustWidthNoAisleWidth;
		FamilySymbol stallSymbol = _PlacementSettings.Parking.ADAStallSymbol;
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(_CurLine.StartPnt, _CurLine.EndPnt, ref _dDirection);
		Eps_Point _NewPnt2 = _CurLine.StartPnt;
		if (_PlacementSettings.Parking.IslandSymbol != null && _PlacementSettings.Parking.IslandWidth > 0.0 && !_PlacementSettings.Parking.CenterIsland)
		{
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, _dDirection, _PlacementSettings.Parking.IslandWidth, ref _NewPnt2);
			DrawIsland(_Document, _PlacementSettings, _Element, _CurLine.StartPnt, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, _dDirection, ref _ExistingIslandList);
			DrawIsland(_Document, _PlacementSettings, _Element, _NewPnt2, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref _ExistingIslandList);
		}
		for (num = 0; num < _PlacementSettings.Parking.CurrentADAStallCount; num++)
		{
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(_dDirection), aDAStallAdjustWidthNoAisleWidth * 0.5, ref _NewPnt);
			eps_Line.StartPnt = _NewPnt;
			EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth * 0.5, ref _NewPnt);
			eps_Line.StartPnt = _NewPnt;
			EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth, ref _NewPnt);
			eps_Line.EndPnt = _NewPnt;
			if (_PlacementSettings.Parking.OptimizeParking)
			{
				if (_PlacementSettings.Parking.IslandWidth > 0.0)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI / 2.0), _PlacementSettings.Parking.IslandWidth / 2.0, ref _NewPnt);
					eps_Line.StartPnt = _NewPnt;
				}
				EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth / 2.0, ref _NewPnt);
				DrawBumper(_Document, _PlacementSettings, _Element, _NewPnt, aDAStallWidth, Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
				if (_PlacementSettings.Parking.IslandWidth > 0.0)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection - Math.PI / 2.0), _PlacementSettings.Parking.IslandWidth / 2.0, ref _NewPnt);
					eps_Line.EndPnt = _NewPnt;
				}
				EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI * 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth / 2.0, ref _NewPnt);
				DrawBumper(_Document, _PlacementSettings, _Element, _NewPnt, aDAStallWidth, 4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
			}
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(_dDirection), num2, ref _NewPnt);
			flag = false;
			flag2 = true;
			if (!_PlacementSettings.Parking.SidesOnIsland && num == 0)
			{
				flag = true;
			}
			else if (_PlacementSettings.Parking.SidesOnIsland && num == _PlacementSettings.Parking.CurrentADAStallCount - 1)
			{
				flag2 = false;
			}
			if (_PlacementSettings.Parking.IslandWidth > 0.0)
			{
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI / 2.0), _PlacementSettings.Parking.IslandWidth / 2.0, ref _NewPnt);
			}
			if (flag3)
			{
				DrawStall(_Document, _PlacementSettings, _Element, _NewPnt, stallSymbol, _dDirection + Math.PI, _PlacementSettings.Parking.StallLength, aDAStallWidth, _PlacementSettings.RotationAngle, flag2, flag, flag3, _bRightAisle: false, ref _ExistingADAParkingStallList);
			}
			else
			{
				DrawStall(_Document, _PlacementSettings, _Element, _NewPnt, stallSymbol, _dDirection + Math.PI, _PlacementSettings.Parking.StallLength, aDAStallWidth, _PlacementSettings.RotationAngle, flag2, flag, flag3, _bRightAisle: false, ref _ExistingParkingStallList);
			}
			_NewPnt = _NewPnt2;
			flag = true;
			flag2 = false;
			if (!_PlacementSettings.Parking.SidesOnIsland && num == 0)
			{
				flag2 = true;
			}
			else if (_PlacementSettings.Parking.SidesOnIsland && num == _PlacementSettings.Parking.CurrentADAStallCount - 1)
			{
				flag = false;
			}
			if (_PlacementSettings.Parking.IslandWidth > 0.0)
			{
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection - Math.PI / 2.0), _PlacementSettings.Parking.IslandWidth / 2.0, ref _NewPnt);
			}
			if (flag3)
			{
				DrawStall(_Document, _PlacementSettings, _Element, _NewPnt, stallSymbol, _dDirection, _PlacementSettings.Parking.StallLength, aDAStallWidth, _PlacementSettings.RotationAngle, flag2, flag, flag3, _bRightAisle: true, ref _ExistingADAParkingStallList);
			}
			else
			{
				DrawStall(_Document, _PlacementSettings, _Element, _NewPnt, stallSymbol, _dDirection, _PlacementSettings.Parking.StallLength, aDAStallWidth, _PlacementSettings.RotationAngle, flag2, flag, flag3, _bRightAisle: true, ref _ExistingParkingStallList);
			}
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(_dDirection), num2, ref _NewPnt2);
			_ProgressFrm.UpdateProgressBar(Convert.ToInt32(num2));
			if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
			{
				flag3 = !flag3;
				if (flag3)
				{
					num2 = _PlacementSettings.Parking.ADAStallAdjustWidth;
					stallSymbol = _PlacementSettings.Parking.ADAStallSymbol;
				}
				else
				{
					num2 = _PlacementSettings.Parking.ADAStallWidth;
					stallSymbol = _PlacementSettings.Type;
				}
			}
		}
		for (num = 0; num < _PlacementSettings.Parking.CurrentStallCount; num++)
		{
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(_dDirection), _PlacementSettings.Parking.StallAdjustWidth * 0.5, ref _NewPnt);
			eps_Line.StartPnt = _NewPnt;
			EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width * 0.5, ref _NewPnt);
			eps_Line.StartPnt = _NewPnt;
			EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width, ref _NewPnt);
			eps_Line.EndPnt = _NewPnt;
			if (_PlacementSettings.Parking.OptimizeParking)
			{
				if (_PlacementSettings.Parking.IslandWidth > 0.0)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI / 2.0), _PlacementSettings.Parking.IslandWidth / 2.0, ref _NewPnt);
					eps_Line.StartPnt = _NewPnt;
				}
				EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width / 2.0, ref _NewPnt);
				DrawBumper(_Document, _PlacementSettings, _Element, _NewPnt, _PlacementSettings.Width, Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
				if (_PlacementSettings.Parking.IslandWidth > 0.0)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection - Math.PI / 2.0), _PlacementSettings.Parking.IslandWidth / 2.0, ref _NewPnt);
					eps_Line.EndPnt = _NewPnt;
				}
				EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI * 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width / 2.0, ref _NewPnt);
				DrawBumper(_Document, _PlacementSettings, _Element, _NewPnt, _PlacementSettings.Width, 4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
			}
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(_dDirection), _PlacementSettings.Parking.StallAdjustWidth, ref _NewPnt);
			flag = false;
			flag2 = true;
			if (!_PlacementSettings.Parking.SidesOnIsland && num == 0 && _PlacementSettings.Parking.CurrentADAStallCount == 0)
			{
				flag = true;
			}
			else if (_PlacementSettings.Parking.SidesOnIsland && num == _PlacementSettings.Parking.CurrentStallCount - 1)
			{
				flag2 = false;
			}
			if (_PlacementSettings.Parking.IslandWidth > 0.0)
			{
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI / 2.0), _PlacementSettings.Parking.IslandWidth / 2.0, ref _NewPnt);
			}
			DrawStall(_Document, _PlacementSettings, _Element, _NewPnt, _PlacementSettings.Type, _dDirection + Math.PI, _PlacementSettings.Parking.StallLength, _PlacementSettings.Width, _PlacementSettings.RotationAngle, flag2, flag, ref _ExistingParkingStallList);
			_NewPnt = _NewPnt2;
			flag = true;
			flag2 = false;
			if (!_PlacementSettings.Parking.SidesOnIsland && num == 0 && _PlacementSettings.Parking.CurrentADAStallCount == 0)
			{
				flag2 = true;
			}
			else if (_PlacementSettings.Parking.SidesOnIsland && num == _PlacementSettings.Parking.CurrentStallCount - 1)
			{
				flag = false;
			}
			if (_PlacementSettings.Parking.IslandWidth > 0.0)
			{
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection - Math.PI / 2.0), _PlacementSettings.Parking.IslandWidth / 2.0, ref _NewPnt);
			}
			DrawStall(_Document, _PlacementSettings, _Element, _NewPnt, _PlacementSettings.Type, _dDirection, _PlacementSettings.Parking.StallLength, _PlacementSettings.Width, _PlacementSettings.RotationAngle, flag2, flag, ref _ExistingParkingStallList);
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(_dDirection), _PlacementSettings.Parking.StallAdjustWidth, ref _NewPnt2);
			_ProgressFrm.UpdateProgressBar(Convert.ToInt32(_PlacementSettings.Parking.StallAdjustWidth));
		}
		if (!_PlacementSettings.Parking.OptimizeParking)
		{
			_PlacementSettings.Parking.StallCount -= 2 * _PlacementSettings.Parking.CurrentStallCount;
		}
		_PlacementSettings.Parking.ADAStallCount -= 2 * _PlacementSettings.Parking.CurrentADAStallCount;
		if (_PlacementSettings.Parking.IslandSymbol != null && _PlacementSettings.Parking.IslandWidth > 0.0)
		{
			if (_PlacementSettings.Parking.CenterIsland)
			{
				EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(_CurLine.StartPnt, _NewPnt2, ref _NewPnt);
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_CurLine.StartPnt, _NewPnt2, ref _dDistance);
				DrawIsland(_Document, _PlacementSettings, _Element, _NewPnt, _dDistance, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, _dDirection, ref _ExistingIslandList);
			}
			else
			{
				DrawIsland(_Document, _PlacementSettings, _Element, _NewPnt2, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, _dDirection, ref _ExistingIslandList);
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(_dDirection), _PlacementSettings.Parking.IslandWidth, ref _NewPnt);
				DrawIsland(_Document, _PlacementSettings, _Element, _NewPnt, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), ref _ExistingIslandList);
			}
		}
	}

	private static void CreateDoubleStalls(Document _Document, ref PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, Element _Element, Eps_Line _CurLine, Eps_Point _LastCurbPoint, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList, bool _bDrawFirstDoubleStripe)
	{
		Eps_Leg _RemainingLeg = new Eps_Leg();
		CreateDoubleStalls(_Document, ref _PlacementSettings, _ProgressFrm, _Element, _CurLine, null, ref _RemainingLeg, ref _LastCurbPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, ref _bDrawFirstDoubleStripe);
	}

	private static void CreateDoubleStalls(Document _Document, ref PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, Element _Element, Eps_Line _CurLine, Eps_Leg _NextLeg, ref Eps_Leg _RemainingLeg, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList, ref bool _bDrawFirstDoubleStripe)
	{
		Eps_Point _LastCurbPoint = new Eps_Point();
		CreateDoubleStalls(_Document, ref _PlacementSettings, _ProgressFrm, _Element, _CurLine, _NextLeg, ref _RemainingLeg, ref _LastCurbPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, ref _bDrawFirstDoubleStripe);
	}

	private static void CreateDoubleStalls(Document _Document, ref PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, Element _Element, Eps_Line _CurLine, Eps_Leg _NextLeg, ref Eps_Leg _RemainingLeg, ref Eps_Point _LastCurbPoint, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList, ref bool _bDrawFirstDoubleStripe)
	{
		Eps_Line eps_Line = new Eps_Line();
		Eps_Point _StaOffPnt = new Eps_Point();
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point _NewPnt = new Eps_Point();
		Eps_Point _NewPnt2 = new Eps_Point();
		Eps_Point _NewPnt3 = new Eps_Point();
		Eps_Point _NewPnt4 = new Eps_Point();
		Eps_Point _NewPnt5 = new Eps_Point();
		double _dDirection = 0.0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		int num = 0;
		bool flag = false;
		bool flag2 = true;
		FamilyInstance familyInstance = null;
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(_CurLine.StartPnt, _CurLine.EndPnt, ref _dDirection);
		CalculateStallParameters(_Document, ref _PlacementSettings, _PlacementSettings.RotationAngle);
		if (CalculateNumberOfStalls(ref _PlacementSettings, _CurLine, _bDoubleRowStallAdjustment: true) < 1)
		{
			return;
		}
		_ProgressFrm.ResetValue();
		_ProgressFrm.MinValue = 0;
		if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
		{
			_ProgressFrm.MaxValue = Convert.ToInt32(_PlacementSettings.Parking.ADAStallAdjustWidth * (double)(_PlacementSettings.Parking.CurrentADAStallCount / 4) + _PlacementSettings.Parking.ADAStallWidth * (double)(_PlacementSettings.Parking.CurrentADAStallCount / 4) + _PlacementSettings.Parking.StallAdjustWidth * (double)(_PlacementSettings.Parking.CurrentStallCount / 2));
		}
		else
		{
			_ProgressFrm.MaxValue = Convert.ToInt32(_PlacementSettings.Parking.ADAStallAdjustWidth * (double)(_PlacementSettings.Parking.CurrentADAStallCount / 2) + _PlacementSettings.Parking.StallAdjustWidth * (double)(_PlacementSettings.Parking.CurrentStallCount / 2));
		}
		Thread.Sleep(300);
		CalculateStallLength(ref _PlacementSettings, _PlacementSettings.RotationAngle);
		eps_Point = _CurLine.StartPnt;
		EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(Math.PI - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.StripeLength, ref _NewPnt4);
		EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(Math.PI * 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.StripeLength, ref _NewPnt5);
		if (_RemainingLeg != null)
		{
			if (_RemainingLeg.Line != null && EPShared.eplGeoE_CalcStationOffsetAlongLine(_NewPnt4, _RemainingLeg.Line, ref _dStation, ref _dOffset, 0.0) == 1700)
			{
				EPShared.eplGeoE_FindStaOffPointAlongLine(_RemainingLeg.Line, _dStation, 0.0, ref _StaOffPnt);
				_RemainingLeg.Line.StartPnt = _StaOffPnt;
			}
			else if (_RemainingLeg.Arc != null && EPShared.eplGeoE_CalcStationOffsetAlongArc(_NewPnt4, _RemainingLeg.Arc, ref _dStation, ref _dOffset, 0.0) == 1700)
			{
				EPShared.eplGeoE_FindStaOffPointAlongArc(_RemainingLeg.Arc, _dStation, 0.0, ref _StaOffPnt);
				_RemainingLeg.Arc.StartPnt = _StaOffPnt;
				double _dDistance = 0.0;
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_RemainingLeg.Arc.StartPnt, _RemainingLeg.Arc.EndPnt, ref _dDistance);
				double _dDeltaAngle = 0.0;
				EPShared.eplGeoE_SolveDeltaByChordLenAndRadius(_dDistance, _RemainingLeg.Arc.Radius, ref _dDeltaAngle);
				_RemainingLeg.Arc.Delta = _dDeltaAngle;
			}
			else if (_RemainingLeg.Line != null && EPShared.eplGeoE_CalcStationOffsetAlongLine(_NewPnt5, _RemainingLeg.Line, ref _dStation, ref _dOffset, 0.0) == 1700)
			{
				EPShared.eplGeoE_FindStaOffPointAlongLine(_RemainingLeg.Line, _dStation, 0.0, ref _StaOffPnt);
				_RemainingLeg.Line.StartPnt = _StaOffPnt;
			}
			else if (_RemainingLeg.Arc != null && EPShared.eplGeoE_CalcStationOffsetAlongArc(_NewPnt5, _RemainingLeg.Arc, ref _dStation, ref _dOffset, 0.0) == 1700)
			{
				EPShared.eplGeoE_FindStaOffPointAlongArc(_RemainingLeg.Arc, _dStation, 0.0, ref _StaOffPnt);
				_RemainingLeg.Arc.StartPnt = _StaOffPnt;
				double _dDistance2 = 0.0;
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_RemainingLeg.Arc.StartPnt, _RemainingLeg.Arc.EndPnt, ref _dDistance2);
				double _dDeltaAngle2 = 0.0;
				EPShared.eplGeoE_SolveDeltaByChordLenAndRadius(_dDistance2, _RemainingLeg.Arc.Radius, ref _dDeltaAngle2);
				_RemainingLeg.Arc.Delta = _dDeltaAngle2;
			}
		}
		bool flag3 = true;
		double num2 = _PlacementSettings.Parking.ADAStallAdjustWidth;
		double aDAStallWidth = _PlacementSettings.Parking.ADAStallWidth;
		double aDAStallAdjustWidthNoAisleWidth = _PlacementSettings.Parking.ADAStallAdjustWidthNoAisleWidth;
		FamilySymbol stallSymbol = _PlacementSettings.Parking.ADAStallSymbol;
		for (num = 0; num < _PlacementSettings.Parking.CurrentADAStallCount; num++)
		{
			EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection), aDAStallAdjustWidthNoAisleWidth * 0.5, ref _StaOffPnt);
			eps_Line.StartPnt = _StaOffPnt;
			EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth * 0.5, ref _StaOffPnt);
			eps_Line.StartPnt = _StaOffPnt;
			EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth, ref _StaOffPnt);
			eps_Line.EndPnt = _StaOffPnt;
			EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection), num2, ref _NewPnt);
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.StripeLength, ref _NewPnt2);
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI * 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.StripeLength, ref _NewPnt3);
			if (!IsStallWithinStallLength(_NextLeg, _NewPnt2, _PlacementSettings.Parking.StallLength) && !IsStallWithinStallLength(_NextLeg, _NewPnt3, _PlacementSettings.Parking.StallLength))
			{
				if (_PlacementSettings.Parking.PlaceBumper)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _StaOffPnt);
					EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth / 2.0, ref _StaOffPnt);
					DrawBumper(_Document, _PlacementSettings, _Element, _StaOffPnt, aDAStallWidth, Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI * 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _StaOffPnt);
					EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), aDAStallWidth / 2.0, ref _StaOffPnt);
					DrawBumper(_Document, _PlacementSettings, _Element, _StaOffPnt, aDAStallWidth, 4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
				}
				EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection), num2, ref _StaOffPnt);
				flag = false;
				flag2 = true;
				if (!_PlacementSettings.Parking.SidesOnIsland && num == 0 && _bDrawFirstDoubleStripe)
				{
					flag = true;
				}
				else if (_PlacementSettings.Parking.SidesOnIsland && num == _PlacementSettings.Parking.CurrentADAStallCount - 1)
				{
					flag2 = false;
				}
				if (flag3)
				{
					DrawStall(_Document, _PlacementSettings, _Element, _StaOffPnt, stallSymbol, _dDirection + Math.PI, _PlacementSettings.Parking.StallLength, aDAStallWidth, _PlacementSettings.RotationAngle, flag2, flag, flag3, _bRightAisle: false, ref _ExistingADAParkingStallList);
				}
				else
				{
					DrawStall(_Document, _PlacementSettings, _Element, _StaOffPnt, stallSymbol, _dDirection + Math.PI, _PlacementSettings.Parking.StallLength, aDAStallWidth, _PlacementSettings.RotationAngle, flag2, flag, flag3, _bRightAisle: false, ref _ExistingParkingStallList);
				}
				_StaOffPnt = eps_Point;
				flag = true;
				flag2 = false;
				if (!_PlacementSettings.Parking.SidesOnIsland && num == 0 && _bDrawFirstDoubleStripe)
				{
					flag2 = true;
				}
				else if (_PlacementSettings.Parking.SidesOnIsland && num == _PlacementSettings.Parking.CurrentADAStallCount - 1)
				{
					flag = false;
				}
				familyInstance = ((!flag3) ? DrawStall(_Document, _PlacementSettings, _Element, _StaOffPnt, stallSymbol, _dDirection, _PlacementSettings.Parking.StallLength, aDAStallWidth, _PlacementSettings.RotationAngle, flag2, flag, flag3, _bRightAisle: true, ref _ExistingParkingStallList) : DrawStall(_Document, _PlacementSettings, _Element, _StaOffPnt, stallSymbol, _dDirection, _PlacementSettings.Parking.StallLength, aDAStallWidth, _PlacementSettings.RotationAngle, flag2, flag, flag3, _bRightAisle: true, ref _ExistingADAParkingStallList));
				eps_Point = _NewPnt;
				_ProgressFrm.UpdateProgressBar(Convert.ToInt32(num2));
				if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
				{
					flag3 = !flag3;
					if (flag3)
					{
						num2 = _PlacementSettings.Parking.ADAStallAdjustWidth;
						stallSymbol = _PlacementSettings.Parking.ADAStallSymbol;
					}
					else
					{
						num2 = _PlacementSettings.Parking.ADAStallWidth;
						stallSymbol = _PlacementSettings.Type;
					}
				}
				continue;
			}
			if (!_PlacementSettings.Parking.SidesOnIsland && familyInstance != null)
			{
				familyInstance.LookupParameter(RevitParameters.RightVisibleParameter)?.Set(Convert.ToInt32(value: true));
				familyInstance = null;
			}
			_PlacementSettings.Parking.CurrentADAStallCount = num;
			break;
		}
		for (num = 0; num < _PlacementSettings.Parking.CurrentStallCount; num++)
		{
			EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection), _PlacementSettings.Parking.StallAdjustWidth * 0.5, ref _StaOffPnt);
			eps_Line.StartPnt = _StaOffPnt;
			EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width * 0.5, ref _StaOffPnt);
			eps_Line.StartPnt = _StaOffPnt;
			EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width, ref _StaOffPnt);
			eps_Line.EndPnt = _StaOffPnt;
			EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection), _PlacementSettings.Parking.StallAdjustWidth, ref _NewPnt);
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.StripeLength, ref _NewPnt2);
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI * 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.StripeLength, ref _NewPnt3);
			if (!IsStallWithinStallLength(_NextLeg, _NewPnt2, _PlacementSettings.Parking.StallLength) && !IsStallWithinStallLength(_NextLeg, _NewPnt3, _PlacementSettings.Parking.StallLength))
			{
				if (_PlacementSettings.Parking.PlaceBumper)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _StaOffPnt);
					EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width / 2.0, ref _StaOffPnt);
					DrawBumper(_Document, _PlacementSettings, _Element, _StaOffPnt, _PlacementSettings.Width, Math.PI / 2.0 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI * 2.0 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _StaOffPnt);
					EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, EPShared.eplGeoE_NormalizeRadians(4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection)), _PlacementSettings.Width / 2.0, ref _StaOffPnt);
					DrawBumper(_Document, _PlacementSettings, _Element, _StaOffPnt, _PlacementSettings.Width, 4.71238898038469 - (_PlacementSettings.RotationAngle - _dDirection), ref _ExistingBumperList);
				}
				EPShared.eplGeoE_CalcPolarToXY(eps_Point, EPShared.eplGeoE_NormalizeRadians(_dDirection), _PlacementSettings.Parking.StallAdjustWidth, ref _StaOffPnt);
				flag = false;
				flag2 = true;
				if (!_PlacementSettings.Parking.SidesOnIsland && num == 0 && _PlacementSettings.Parking.CurrentADAStallCount == 0 && _bDrawFirstDoubleStripe)
				{
					flag = true;
				}
				else if (_PlacementSettings.Parking.SidesOnIsland && num == _PlacementSettings.Parking.CurrentStallCount - 1)
				{
					flag2 = false;
				}
				DrawStall(_Document, _PlacementSettings, _Element, _StaOffPnt, _PlacementSettings.Type, _dDirection + Math.PI, _PlacementSettings.Parking.StallLength, _PlacementSettings.Width, _PlacementSettings.RotationAngle, flag2, flag, ref _ExistingParkingStallList);
				_StaOffPnt = eps_Point;
				flag = true;
				flag2 = false;
				if (!_PlacementSettings.Parking.SidesOnIsland && num == 0 && _PlacementSettings.Parking.CurrentADAStallCount == 0 && _bDrawFirstDoubleStripe)
				{
					flag2 = true;
				}
				else if (_PlacementSettings.Parking.SidesOnIsland && num == _PlacementSettings.Parking.CurrentStallCount - 1)
				{
					flag = false;
				}
				DrawStall(_Document, _PlacementSettings, _Element, _StaOffPnt, _PlacementSettings.Type, _dDirection, _PlacementSettings.Parking.StallLength, _PlacementSettings.Width, _PlacementSettings.RotationAngle, flag2, flag, ref _ExistingParkingStallList);
				flag = false;
				flag2 = true;
				eps_Point = _NewPnt;
				_ProgressFrm.UpdateProgressBar(Convert.ToInt32(_PlacementSettings.Parking.StallAdjustWidth));
				continue;
			}
			if (!_PlacementSettings.Parking.SidesOnIsland && familyInstance != null)
			{
				familyInstance.LookupParameter(RevitParameters.RightVisibleParameter)?.Set(Convert.ToInt32(value: true));
				familyInstance = null;
			}
			_PlacementSettings.Parking.CurrentStallCount = num;
			break;
		}
		if (!_PlacementSettings.Parking.OptimizeParking)
		{
			_PlacementSettings.Parking.StallCount -= 2 * _PlacementSettings.Parking.CurrentStallCount;
		}
		_PlacementSettings.Parking.ADAStallCount -= 2 * _PlacementSettings.Parking.CurrentADAStallCount;
		if (_NextLeg != null)
		{
			if (_NextLeg.Line != null)
			{
				if (EPShared.eplGeoE_CalcStationOffsetAlongLine(_NewPnt2, _NextLeg.Line, ref _dStation, ref _dOffset, 0.0) == 1700)
				{
					EPShared.eplGeoE_FindStaOffPointAlongLine(_NextLeg.Line, _dStation, 0.0, ref _StaOffPnt);
					_NextLeg.Line.StartPnt = _StaOffPnt;
				}
				else if (EPShared.eplGeoE_CalcStationOffsetAlongLine(_NewPnt3, _NextLeg.Line, ref _dStation, ref _dOffset, 0.0) == 1700)
				{
					EPShared.eplGeoE_FindStaOffPointAlongLine(_NextLeg.Line, _dStation, 0.0, ref _StaOffPnt);
					_NextLeg.Line.StartPnt = _StaOffPnt;
				}
			}
			else if (_NextLeg.Arc != null)
			{
				if (EPShared.eplGeoE_CalcStationOffsetAlongArc(_NewPnt2, _NextLeg.Arc, ref _dStation, ref _dOffset, 0.0) == 1700)
				{
					EPShared.eplGeoE_FindStaOffPointAlongArc(_NextLeg.Arc, _dStation, 0.0, ref _StaOffPnt);
					_NextLeg.Arc.StartPnt = _StaOffPnt;
					double _dDistance3 = 0.0;
					EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NextLeg.Arc.StartPnt, _NextLeg.Arc.EndPnt, ref _dDistance3);
					double _dDeltaAngle3 = 0.0;
					EPShared.eplGeoE_SolveDeltaByChordLenAndRadius(_dDistance3, _NextLeg.Arc.Radius, ref _dDeltaAngle3);
					_NextLeg.Arc.Delta = _dDeltaAngle3;
				}
				else if (EPShared.eplGeoE_CalcStationOffsetAlongArc(_NewPnt3, _NextLeg.Arc, ref _dStation, ref _dOffset, 0.0) == 1700)
				{
					EPShared.eplGeoE_FindStaOffPointAlongArc(_NextLeg.Arc, _dStation, 0.0, ref _StaOffPnt);
					_NextLeg.Arc.StartPnt = _StaOffPnt;
					double _dDistance4 = 0.0;
					EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NextLeg.Arc.StartPnt, _NextLeg.Arc.EndPnt, ref _dDistance4);
					double _dDeltaAngle4 = 0.0;
					EPShared.eplGeoE_SolveDeltaByChordLenAndRadius(_dDistance4, _NextLeg.Arc.Radius, ref _dDeltaAngle4);
					_NextLeg.Arc.Delta = _dDeltaAngle4;
				}
			}
		}
		_LastCurbPoint = eps_Point;
		_bDrawFirstDoubleStripe = true;
	}

	private static void DrawCurvedParking(Document _Document, ref PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, Element _Element, Eps_Arc _CurrentArc, bool _bIsInside, bool _bDoubleRowStallAdjustment, Eps_Leg _NextLeg, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList)
	{
		int num = 0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double _dDirection = 0.0;
		Eps_Line eps_Line = new Eps_Line();
		Eps_Line eps_Line2 = new Eps_Line();
		Eps_Line eps_Line3 = new Eps_Line();
		Eps_Point _NewPnt = new Eps_Point();
		Eps_Point _NewPnt2 = new Eps_Point();
		Eps_Point insertPoint = new Eps_Point();
		Eps_Point _NewPnt3 = new Eps_Point();
		int num7 = 0;
		int num8 = 0;
		int result = 0;
		double num9 = 0.0;
		double num10 = 0.0;
		double _dDirection2 = 0.0;
		double num11 = 0.0;
		bool flag = false;
		double num12 = 0.0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		double num13 = 0.0;
		bool flag2 = true;
		Eps_Arc eps_Arc = new Eps_Arc(_CurrentArc);
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Arc.RadiusPnt, eps_Arc.StartPnt, ref _dDirection);
		if (_bIsInside)
		{
			num10 = EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI);
			num9 = ((eps_Arc.Direction == 1) ? (Math.PI / 2.0) : 4.71238898038469);
			num3 = (eps_Arc.Radius - _PlacementSettings.Parking.StallLength) * 2.0 * Math.PI * (eps_Arc.Delta / (Math.PI * 2.0));
			num12 = _PlacementSettings.Parking.StallLength;
			if (eps_Arc.Direction == -1)
			{
				flag = true;
			}
			else if (eps_Arc.Direction == 1)
			{
				flag = false;
			}
		}
		else
		{
			num10 = _dDirection;
			num9 = ((eps_Arc.Direction == -1) ? (Math.PI / 2.0) : 4.71238898038469);
			num3 = eps_Arc.Radius * 2.0 * Math.PI * (eps_Arc.Delta / (Math.PI * 2.0));
			if (eps_Arc.Direction == -1)
			{
				flag = false;
			}
			else if (eps_Arc.Direction == 1)
			{
				flag = true;
			}
		}
		CalculateStallParameters(_Document, ref _PlacementSettings, Math.PI / 2.0);
		num5 = (Math.PI / 2.0 - Math.Acos(_PlacementSettings.Parking.ADAStallAdjustWidth / 2.0 / (eps_Arc.Radius - num12))) * 2.0;
		num4 = (Math.PI / 2.0 - Math.Acos(_PlacementSettings.Parking.ADAStallWidth / 2.0 / (eps_Arc.Radius - num12))) * 2.0;
		num6 = (Math.PI / 2.0 - Math.Acos(_PlacementSettings.Width / 2.0 / (eps_Arc.Radius - num12))) * 2.0;
		num2 = eps_Arc.Delta;
		_PlacementSettings.Parking.CurrentStallCount = 0;
		_PlacementSettings.Parking.CurrentADAStallCount = 0;
		num13 = ((!_PlacementSettings.Parking.ADAStallSharedAccessibleAisle) ? num4 : ((num4 + num5) / 2.0));
		if (_PlacementSettings.Parking.ADAStallCount >= 1)
		{
			num = (int)(num2 / num13);
			if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
			{
				num8 = Math.DivRem(num, 2, out result);
				if (result > 0 && num2 < num4 * (double)num8 + num5 * (double)(num8 + 1))
				{
					num--;
				}
			}
			if (_bDoubleRowStallAdjustment)
			{
				if (2 * num < _PlacementSettings.Parking.ADAStallCount)
				{
					_PlacementSettings.Parking.CurrentADAStallCount = num;
				}
				else if (_PlacementSettings.Parking.ADAStallCount > 0)
				{
					_PlacementSettings.Parking.CurrentADAStallCount = _PlacementSettings.Parking.ADAStallCount / 2;
				}
			}
			else if (num < _PlacementSettings.Parking.ADAStallCount)
			{
				_PlacementSettings.Parking.CurrentADAStallCount = num;
			}
			else
			{
				_PlacementSettings.Parking.CurrentADAStallCount = _PlacementSettings.Parking.ADAStallCount;
			}
			if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
			{
				num8 = Math.DivRem(_PlacementSettings.Parking.CurrentADAStallCount, 2, out result);
				num2 = ((result <= 0) ? (num2 - num13 * (double)_PlacementSettings.Parking.CurrentADAStallCount) : (num2 - (num4 * (double)num8 + num5 * (double)(num8 + 1))));
			}
			else
			{
				num2 -= num13 * (double)_PlacementSettings.Parking.CurrentADAStallCount;
			}
		}
		if (RevitDefines.GT(num2, 0.0))
		{
			num = (int)(num2 / num6);
			if (num > 0)
			{
				_PlacementSettings.Parking.CurrentStallCount = num;
			}
		}
		if (!_PlacementSettings.Parking.OptimizeParking)
		{
			if (_PlacementSettings.Parking.CurrentADAStallCount > _PlacementSettings.Parking.ADAStallCount)
			{
				_PlacementSettings.Parking.CurrentADAStallCount = _PlacementSettings.Parking.ADAStallCount;
			}
			if (_PlacementSettings.Parking.CurrentStallCount > _PlacementSettings.Parking.StallCount)
			{
				_PlacementSettings.Parking.CurrentStallCount = _PlacementSettings.Parking.StallCount;
			}
			if (_PlacementSettings.Parking.CurrentStallCount > 0 && _bDoubleRowStallAdjustment)
			{
				_PlacementSettings.Parking.CurrentStallCount /= 2;
			}
		}
		if (_PlacementSettings.Parking.CurrentStallCount + _PlacementSettings.Parking.CurrentADAStallCount <= 0)
		{
			return;
		}
		_ProgressFrm.ResetValue();
		_ProgressFrm.MinValue = 0;
		_ProgressFrm.MaxValue = Convert.ToInt32(_PlacementSettings.Parking.ADAStallAdjustWidth * (double)_PlacementSettings.Parking.CurrentADAStallCount + _PlacementSettings.Parking.StallAdjustWidth * (double)_PlacementSettings.Parking.CurrentStallCount);
		Thread.Sleep(300);
		CalculateStallLength(ref _PlacementSettings, Math.PI / 2.0);
		if (eps_Arc.Direction != 1)
		{
			num4 *= -1.0;
			num6 *= -1.0;
		}
		bool flag3 = true;
		double num14 = _PlacementSettings.Parking.ADAStallAdjustWidth;
		double aDAStallWidth = _PlacementSettings.Parking.ADAStallWidth;
		double num15 = num5;
		double num16 = _dDirection;
		double num17 = num10;
		FamilySymbol stallSymbol = _PlacementSettings.Parking.ADAStallSymbol;
		for (num7 = 0; num7 < _PlacementSettings.Parking.CurrentADAStallCount; num7++)
		{
			num11 = EPShared.eplGeoE_NormalizeRadians(num16 + num15 * 0.5);
			EPShared.eplGeoE_CalcPolarToXY(eps_Arc.RadiusPnt, num11, eps_Arc.Radius, ref _NewPnt2);
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(num11 - Math.PI / 2.0), num14 / 2.0, ref _NewPnt);
			if (flag)
			{
				eps_Line.StartPnt = _NewPnt;
			}
			else
			{
				eps_Line.EndPnt = _NewPnt;
			}
			if (!_bIsInside)
			{
				insertPoint = _NewPnt;
				if (flag3)
				{
					if (flag)
					{
						EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num11 - Math.PI / 2.0), _PlacementSettings.Parking.AdjustAisleWidth / 2.0, ref _NewPnt);
					}
					else
					{
						EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num11 + Math.PI / 2.0), _PlacementSettings.Parking.AdjustAisleWidth / 2.0, ref _NewPnt);
					}
				}
			}
			else if (flag3)
			{
				if (flag)
				{
					EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num11 + Math.PI / 2.0), _PlacementSettings.Parking.AdjustAisleWidth / 2.0, ref _NewPnt);
				}
				else
				{
					EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num11 - Math.PI / 2.0), _PlacementSettings.Parking.AdjustAisleWidth / 2.0, ref _NewPnt);
				}
			}
			if (flag)
			{
				eps_Line2.StartPnt = _NewPnt;
			}
			else
			{
				eps_Line2.EndPnt = _NewPnt;
			}
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, EPShared.eplGeoE_NormalizeRadians(num11 + Math.PI / 2.0), num14 / 2.0, ref _NewPnt);
			if (flag)
			{
				eps_Line.EndPnt = _NewPnt;
			}
			else
			{
				eps_Line.StartPnt = _NewPnt;
			}
			if (_bIsInside)
			{
				insertPoint = _NewPnt;
				if (flag3)
				{
					if (flag)
					{
						EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num11 + Math.PI / 2.0), _PlacementSettings.Parking.AdjustAisleWidth / 2.0, ref _NewPnt);
					}
					else
					{
						EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num11 - Math.PI / 2.0), _PlacementSettings.Parking.AdjustAisleWidth / 2.0, ref _NewPnt);
					}
				}
			}
			else if (flag3)
			{
				if (flag)
				{
					EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num11 - Math.PI / 2.0), _PlacementSettings.Parking.AdjustAisleWidth / 2.0, ref _NewPnt);
				}
				else
				{
					EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num11 + Math.PI / 2.0), _PlacementSettings.Parking.AdjustAisleWidth / 2.0, ref _NewPnt);
				}
			}
			if (flag)
			{
				eps_Line2.EndPnt = _NewPnt;
			}
			else
			{
				eps_Line2.StartPnt = _NewPnt;
			}
			if (_bIsInside)
			{
				num11 -= Math.PI / 2.0;
				EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(num17 + num15 * 0.5), _PlacementSettings.Parking.StripeLength, ref _NewPnt3);
			}
			else
			{
				num11 += Math.PI / 2.0;
				EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(num17 + num15 * 0.5), _PlacementSettings.Parking.StripeLength, ref _NewPnt3);
			}
			flag2 = true;
			if (_NextLeg != null)
			{
				if (_NextLeg.Line != null && EPShared.eplGeoE_CalcStationOffsetAlongLine(_NewPnt3, _NextLeg.Line, ref _dStation, ref _dOffset, 0.0) == 1700)
				{
					if (RevitDefines.LT(_dOffset, _PlacementSettings.Parking.StripeLength))
					{
						flag2 = false;
					}
				}
				else if (_NextLeg.Arc != null && EPShared.eplGeoE_CalcStationOffsetAlongArc(_NewPnt3, _NextLeg.Arc, ref _dStation, ref _dOffset, 0.0) == 1700 && RevitDefines.LT(_dOffset, _PlacementSettings.Parking.StripeLength))
				{
					flag2 = false;
				}
			}
			if (flag2)
			{
				if (flag3)
				{
					DrawStall(_Document, _PlacementSettings, _Element, insertPoint, stallSymbol, num11, _PlacementSettings.Parking.StallLength, aDAStallWidth, Math.PI / 2.0, _bHideLeftStripe: true, _bHideRightStripe: true, flag3, flag, ref _ExistingADAParkingStallList);
				}
				else
				{
					DrawStall(_Document, _PlacementSettings, _Element, insertPoint, stallSymbol, num11, _PlacementSettings.Parking.StallLength, aDAStallWidth, Math.PI / 2.0, _bHideLeftStripe: true, _bHideRightStripe: true, flag3, flag, ref _ExistingParkingStallList);
				}
				if (_PlacementSettings.Parking.PlaceBumper)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Line2.StartPnt, EPShared.eplGeoE_NormalizeRadians(num17), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
					EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num17 - num9), _PlacementSettings.Parking.BumperOffset, ref _NewPnt);
					eps_Line3.StartPnt = _NewPnt;
					EPShared.eplGeoE_CalcPolarToXY(eps_Line2.EndPnt, EPShared.eplGeoE_NormalizeRadians(num17 + num15), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
					EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num17 + num9 + num15), _PlacementSettings.Parking.BumperOffset, ref _NewPnt);
					eps_Line3.EndPnt = _NewPnt;
					EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(eps_Line3.StartPnt, eps_Line3.EndPnt, ref _NewPnt);
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Line3.StartPnt, eps_Line3.EndPnt, ref _dDirection2);
					DrawBumper(_Document, _PlacementSettings, _Element, _NewPnt, aDAStallWidth, _dDirection2, ref _ExistingBumperList);
				}
			}
			_ProgressFrm.UpdateProgressBar(Convert.ToInt32(num14));
			num16 += num15;
			num17 += num15;
			if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
			{
				flag3 = !flag3;
				if (flag3)
				{
					num14 = _PlacementSettings.Parking.ADAStallAdjustWidth;
					stallSymbol = _PlacementSettings.Parking.ADAStallSymbol;
					num15 = num5;
				}
				else
				{
					num14 = _PlacementSettings.Parking.ADAStallWidth;
					stallSymbol = _PlacementSettings.Type;
					num15 = num4;
				}
			}
		}
		for (num7 = 0; num7 < _PlacementSettings.Parking.CurrentStallCount; num7++)
		{
			num11 = EPShared.eplGeoE_NormalizeRadians(num16 + num6 * 0.5);
			EPShared.eplGeoE_CalcPolarToXY(eps_Arc.RadiusPnt, num11, eps_Arc.Radius, ref _NewPnt2);
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, num11 - Math.PI / 2.0, _PlacementSettings.Width / 2.0, ref _NewPnt);
			if (!_bIsInside)
			{
				insertPoint = _NewPnt;
			}
			if (flag)
			{
				eps_Line2.StartPnt = _NewPnt;
			}
			else
			{
				eps_Line2.EndPnt = _NewPnt;
			}
			EPShared.eplGeoE_CalcPolarToXY(_NewPnt2, num11 + Math.PI / 2.0, _PlacementSettings.Width / 2.0, ref _NewPnt);
			if (_bIsInside)
			{
				insertPoint = _NewPnt;
			}
			if (flag)
			{
				eps_Line2.EndPnt = _NewPnt;
			}
			else
			{
				eps_Line2.StartPnt = _NewPnt;
			}
			if (_bIsInside)
			{
				num11 -= Math.PI / 2.0;
				EPShared.eplGeoE_CalcPolarToXY(eps_Line2.StartPnt, EPShared.eplGeoE_NormalizeRadians(num17 + num6 * 0.5), _PlacementSettings.Parking.StripeLength, ref _NewPnt3);
			}
			else
			{
				num11 += Math.PI / 2.0;
				EPShared.eplGeoE_CalcPolarToXY(eps_Line2.EndPnt, EPShared.eplGeoE_NormalizeRadians(num17 + num6 * 0.5), _PlacementSettings.Parking.StripeLength, ref _NewPnt3);
			}
			flag2 = true;
			if (_NextLeg != null)
			{
				if (_NextLeg.Line != null && EPShared.eplGeoE_CalcStationOffsetAlongLine(_NewPnt3, _NextLeg.Line, ref _dStation, ref _dOffset, 0.0) == 1700)
				{
					if (RevitDefines.LT(_dOffset, _PlacementSettings.Parking.StripeLength))
					{
						flag2 = false;
					}
				}
				else if (_NextLeg.Arc != null && EPShared.eplGeoE_CalcStationOffsetAlongArc(_NewPnt3, _NextLeg.Arc, ref _dStation, ref _dOffset, 0.0) == 1700 && RevitDefines.LT(_dOffset, _PlacementSettings.Parking.StripeLength))
				{
					flag2 = false;
				}
			}
			if (flag2)
			{
				DrawStall(_Document, _PlacementSettings, _Element, insertPoint, _PlacementSettings.Type, num11, _PlacementSettings.Parking.StallLength, _PlacementSettings.Width, Math.PI / 2.0, _bHideLeftStripe: true, _bHideRightStripe: true, ref _ExistingParkingStallList);
				if (_PlacementSettings.Parking.PlaceBumper)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Line2.StartPnt, EPShared.eplGeoE_NormalizeRadians(num17), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
					EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num17 - num9), _PlacementSettings.Parking.BumperOffset, ref _NewPnt);
					eps_Line3.StartPnt = _NewPnt;
					EPShared.eplGeoE_CalcPolarToXY(eps_Line2.EndPnt, EPShared.eplGeoE_NormalizeRadians(num17 + num6), _PlacementSettings.Parking.BumperSetback + _PlacementSettings.Parking.BumperWidth / 2.0, ref _NewPnt);
					EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(num17 + num9 + num6), _PlacementSettings.Parking.BumperOffset, ref _NewPnt);
					eps_Line3.EndPnt = _NewPnt;
					EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(eps_Line3.StartPnt, eps_Line3.EndPnt, ref _NewPnt);
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Line3.StartPnt, eps_Line3.EndPnt, ref _dDirection2);
					DrawBumper(_Document, _PlacementSettings, _Element, _NewPnt, _PlacementSettings.Width, _dDirection2, ref _ExistingBumperList);
				}
			}
			_ProgressFrm.UpdateProgressBar(Convert.ToInt32(_PlacementSettings.Parking.StallAdjustWidth));
			num16 += num6;
			num17 += num6;
		}
		if (!_PlacementSettings.Parking.OptimizeParking)
		{
			_PlacementSettings.Parking.StallCount -= _PlacementSettings.Parking.CurrentStallCount;
		}
		_PlacementSettings.Parking.ADAStallCount -= _PlacementSettings.Parking.CurrentADAStallCount;
		if (_NextLeg != null)
		{
			if (_NextLeg.Line != null && EPShared.eplGeoE_CalcStationOffsetAlongLine(_NewPnt3, _NextLeg.Line, ref _dStation, ref _dOffset, 0.0) == 1700)
			{
				EPShared.eplGeoE_FindStaOffPointAlongLine(_NextLeg.Line, _dStation, 0.0, ref _NewPnt);
				_NextLeg.Line.StartPnt = _NewPnt;
			}
			else if (_NextLeg.Arc != null && EPShared.eplGeoE_CalcStationOffsetAlongArc(_NewPnt3, _NextLeg.Arc, ref _dStation, ref _dOffset, 0.0) == 1700)
			{
				EPShared.eplGeoE_FindStaOffPointAlongArc(_NextLeg.Arc, _dStation, 0.0, ref _NewPnt);
				_NextLeg.Arc.StartPnt = _NewPnt;
				double _dDistance = 0.0;
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_NextLeg.Arc.StartPnt, _NextLeg.Arc.EndPnt, ref _dDistance);
				double _dDeltaAngle = 0.0;
				EPShared.eplGeoE_SolveDeltaByChordLenAndRadius(_dDistance, _NextLeg.Arc.Radius, ref _dDeltaAngle);
				_NextLeg.Arc.Delta = _dDeltaAngle;
			}
		}
	}

	private static void DrawAreaParking(Document _Document, ref PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, Element _Element, Eps_Line _CurLine, Eps_Point _SidePoint, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList, ref List<FamilyInstance> _ExistingIslandList)
	{
		Eps_Point _NewPnt = new Eps_Point();
		double _dDirection = 0.0;
		double AdjRowHeight = 0.0;
		CalculateStallParameters(_Document, ref _PlacementSettings, _PlacementSettings.RotationAngle);
		if (CalculateNumberOfStalls(ref _PlacementSettings, _CurLine, _bDoubleRowStallAdjustment: true) < 1)
		{
			return;
		}
		_ProgressFrm.ResetValue();
		_ProgressFrm.MinValue = 0;
		if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
		{
			_ProgressFrm.MaxValue = Convert.ToInt32(_PlacementSettings.Parking.ADAStallAdjustWidth * (double)(_PlacementSettings.Parking.CurrentADAStallCount / 2) + _PlacementSettings.Parking.ADAStallWidth * (double)(_PlacementSettings.Parking.CurrentADAStallCount / 2) + _PlacementSettings.Parking.StallAdjustWidth * (double)_PlacementSettings.Parking.CurrentStallCount);
		}
		else
		{
			_ProgressFrm.MaxValue = Convert.ToInt32(_PlacementSettings.Parking.ADAStallAdjustWidth * (double)_PlacementSettings.Parking.CurrentADAStallCount + _PlacementSettings.Parking.StallAdjustWidth * (double)_PlacementSettings.Parking.CurrentStallCount);
		}
		Thread.Sleep(300);
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(_CurLine.StartPnt, _SidePoint, ref _dDirection);
		CalculateStallLength(ref _PlacementSettings, _PlacementSettings.RotationAngle);
		if (CalculateNumberOfRows(_PlacementSettings, _CurLine, ref AdjRowHeight) < 1)
		{
			return;
		}
		for (int i = 0; i < _PlacementSettings.Parking.AreaRowCount; i++)
		{
			CreateDoubleStalls(_Document, ref _PlacementSettings, _ProgressFrm, _Element, _CurLine, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, ref _ExistingIslandList);
			EPShared.eplGeoE_CalcPolarToXY(_CurLine.StartPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection), AdjRowHeight, ref _NewPnt);
			_CurLine.StartPnt = _NewPnt;
			EPShared.eplGeoE_CalcPolarToXY(_CurLine.EndPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection), AdjRowHeight, ref _NewPnt);
			_CurLine.EndPnt = _NewPnt;
			if (!_PlacementSettings.Parking.AreaBiDirectionalAisles)
			{
				_PlacementSettings.RotationAngle = EPShared.eplGeoE_NormalizeRadians(Math.PI - _PlacementSettings.RotationAngle);
			}
			if (CalculateNumberOfStalls(ref _PlacementSettings, _CurLine, _bDoubleRowStallAdjustment: true) < 1)
			{
				break;
			}
			_ProgressFrm.ResetValue();
			_ProgressFrm.MinValue = 0;
			if (_PlacementSettings.Parking.ADAStallSharedAccessibleAisle)
			{
				_ProgressFrm.MaxValue = Convert.ToInt32(_PlacementSettings.Parking.ADAStallAdjustWidth * (double)(_PlacementSettings.Parking.CurrentADAStallCount / 2) + _PlacementSettings.Parking.ADAStallWidth * (double)(_PlacementSettings.Parking.CurrentADAStallCount / 2) + _PlacementSettings.Parking.StallAdjustWidth * (double)_PlacementSettings.Parking.CurrentStallCount);
			}
			else
			{
				_ProgressFrm.MaxValue = Convert.ToInt32(_PlacementSettings.Parking.ADAStallAdjustWidth * (double)_PlacementSettings.Parking.CurrentADAStallCount + _PlacementSettings.Parking.StallAdjustWidth * (double)_PlacementSettings.Parking.CurrentStallCount);
			}
			Thread.Sleep(300);
		}
	}

	public static bool PlaceLegList(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _CurrentLeg)
	{
		Eps_Leg _FirstLeg = null;
		Eps_Leg _SecondLeg = null;
		bool _UseRightSide = true;
		bool _bPickFirstLeg = true;
		bool _bPickMiddleSegment = false;
		if (!_PlacementSettings.ExistingElementChecked)
		{
			_SecondLeg = _CurrentLeg;
			_bPickFirstLeg = false;
		}
		else if (!RevitUtility.BreakApartLeg(_Document, _PlacementSettings.TransactionContainer, _PlacementSettings.Width / 2.0, _CurrentLeg, _PlacementSettings.Parking.PromptForDirection, ref _FirstLeg, ref _SecondLeg, ref _bPickFirstLeg, ref _bPickMiddleSegment))
		{
			return false;
		}
		_PlacementSettings.Parking.MiddleStallPlacement = _bPickMiddleSegment;
		if (_PlacementSettings.PlacementType == PlacementType.ParkingSingleRow)
		{
			Eps_Leg ppOffsetLegList = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentLeg, _PlacementSettings.Parking.StallLength, 1, ref ppOffsetLegList, TrimClosedObject: true);
			Eps_Leg ppOffsetLegList2 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentLeg, _PlacementSettings.Parking.StallLength, 2, ref ppOffsetLegList2, TrimClosedObject: true);
			_PlacementSettings.TransactionContainer.BeginTransaction();
			ModelCurveArray modelCurveArray = RevitUtility.NewModelCurveArray(_Document, ppOffsetLegList);
			ModelCurveArray modelCurveArray2 = RevitUtility.NewModelCurveArray(_Document, ppOffsetLegList2);
			_PlacementSettings.TransactionContainer.EndTransaction();
			_Document.Regenerate();
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
		}
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		int _nProgressBarCount = 1;
		List<FamilyInstance> _ExistingParkingStallList = null;
		List<FamilyInstance> _ExistingADAParkingStallList = null;
		List<FamilyInstance> _ExistingBumperList = null;
		if (_PlacementSettings.TransactionContainer != null)
		{
			_PlacementSettings.TransactionContainer.BeginTransaction();
		}
		if (_CurrentLeg.Element != null)
		{
			_PlacementSettings.FirstLegSegmentID = _CurrentLeg.Element.UniqueId;
		}
		if (_CurrentLeg.Last.Element != null)
		{
			_PlacementSettings.LastLegSegmentID = _CurrentLeg.Last.Element.UniqueId;
		}
		_PlacementSettings.IsReverse = _bPickFirstLeg;
		_PlacementSettings.UseRightSide = _UseRightSide;
		if (_bPickFirstLeg)
		{
			if (_FirstLeg != null)
			{
				PlaceStallsOnLeg(_Document, _PlacementSettings, progressBarForm, _FirstLeg, ref _nProgressBarCount, !_UseRightSide, ref _SecondLeg, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
			}
			if (_SecondLeg != null)
			{
				PlaceStallsOnLeg(_Document, _PlacementSettings, progressBarForm, _SecondLeg, ref _nProgressBarCount, _UseRightSide, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
			}
		}
		else
		{
			if (_SecondLeg != null)
			{
				PlaceStallsOnLeg(_Document, _PlacementSettings, progressBarForm, _SecondLeg, ref _nProgressBarCount, _UseRightSide, ref _FirstLeg, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
			}
			if (_FirstLeg != null)
			{
				PlaceStallsOnLeg(_Document, _PlacementSettings, progressBarForm, _FirstLeg, ref _nProgressBarCount, !_UseRightSide, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
			}
		}
		if (_ExistingParkingStallList != null && _ExistingParkingStallList.Count > 0)
		{
			for (int i = 0; i < _ExistingParkingStallList.Count; i++)
			{
				_Document.Delete(_ExistingParkingStallList[i].Id);
			}
		}
		if (_ExistingADAParkingStallList != null && _ExistingADAParkingStallList.Count > 0)
		{
			for (int j = 0; j < _ExistingADAParkingStallList.Count; j++)
			{
				_Document.Delete(_ExistingADAParkingStallList[j].Id);
			}
		}
		if (_ExistingBumperList != null && _ExistingBumperList.Count > 0)
		{
			for (int k = 0; k < _ExistingBumperList.Count; k++)
			{
				_Document.Delete(_ExistingBumperList[k].Id);
			}
		}
		if (_PlacementSettings.TransactionContainer != null)
		{
			_PlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm?.Close();
		if (_PlacementSettings.DeleteExistingChecked && _CurrentLeg.ElementSet != null && _CurrentLeg.ElementSet.Size > 0)
		{
			_PlacementSettings.TransactionContainer.BeginTransaction();
			_Document.Delete(ElementSetConverter.ToIDList(_CurrentLeg.ElementSet));
			_PlacementSettings.TransactionContainer.EndTransaction();
		}
		return true;
	}

	public static bool PlaceLegList(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _CurrentLeg, Eps_Leg FirstLeg, Eps_Leg SecondLeg, bool _bUseFirstLeg, List<FamilyInstance> _ExistingParkingStallList, List<FamilyInstance> _ExistingADAParkingStallList, List<FamilyInstance> _ExistingBumperList)
	{
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		int _nProgressBarCount = 1;
		if (_PlacementSettings.TransactionContainer != null)
		{
			_PlacementSettings.TransactionContainer.BeginTransaction();
		}
		if (_bUseFirstLeg)
		{
			if (FirstLeg != null)
			{
				PlaceStallsOnLeg(_Document, _PlacementSettings, progressBarForm, FirstLeg, ref _nProgressBarCount, !_PlacementSettings.UseRightSide, ref SecondLeg, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
			}
			if (SecondLeg != null)
			{
				PlaceStallsOnLeg(_Document, _PlacementSettings, progressBarForm, SecondLeg, ref _nProgressBarCount, _PlacementSettings.UseRightSide, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
			}
		}
		else
		{
			if (SecondLeg != null)
			{
				PlaceStallsOnLeg(_Document, _PlacementSettings, progressBarForm, SecondLeg, ref _nProgressBarCount, _PlacementSettings.UseRightSide, ref FirstLeg, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
			}
			if (FirstLeg != null)
			{
				PlaceStallsOnLeg(_Document, _PlacementSettings, progressBarForm, FirstLeg, ref _nProgressBarCount, !_PlacementSettings.UseRightSide, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
			}
		}
		if (_ExistingParkingStallList != null && _ExistingParkingStallList.Count > 0)
		{
			for (int i = 0; i < _ExistingParkingStallList.Count; i++)
			{
				_Document.Delete(_ExistingParkingStallList[i].Id);
			}
		}
		if (_ExistingADAParkingStallList != null && _ExistingADAParkingStallList.Count > 0)
		{
			for (int j = 0; j < _ExistingADAParkingStallList.Count; j++)
			{
				_Document.Delete(_ExistingADAParkingStallList[j].Id);
			}
		}
		if (_ExistingBumperList != null && _ExistingBumperList.Count > 0)
		{
			for (int k = 0; k < _ExistingBumperList.Count; k++)
			{
				_Document.Delete(_ExistingBumperList[k].Id);
			}
		}
		if (_PlacementSettings.TransactionContainer != null)
		{
			_PlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm?.Close();
		return true;
	}

	private static void PlaceStallsOnLeg(Document _Document, PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, Eps_Leg _Leg, ref int _nProgressBarCount, bool _bUseRightLeg, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList)
	{
		Eps_Leg _RemainingLeg = new Eps_Leg();
		PlaceStallsOnLeg(_Document, _PlacementSettings, _ProgressFrm, _bFirstCall: false, _Leg, ref _nProgressBarCount, _bUseRightLeg, ref _RemainingLeg, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, !_PlacementSettings.Parking.MiddleStallPlacement);
	}

	private static void PlaceStallsOnLeg(Document _Document, PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, Eps_Leg _Leg, ref int _nProgressBarCount, bool _bUseRightLeg, ref Eps_Leg _RemainingLeg, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList)
	{
		PlaceStallsOnLeg(_Document, _PlacementSettings, _ProgressFrm, _bFirstCall: true, _Leg, ref _nProgressBarCount, _bUseRightLeg, ref _RemainingLeg, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, _bDrawFirstDoubleStripe: true);
	}

	private static void PlaceStallsOnLeg(Document _Document, PlacementSettings _PlacementSettings, ProgressBarForm _ProgressFrm, bool _bFirstCall, Eps_Leg _Leg, ref int _nProgressBarCount, bool _bUseRightLeg, ref Eps_Leg _RemainingLeg, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList, bool _bDrawFirstDoubleStripe)
	{
		Eps_Leg eps_Leg = null;
		Eps_Line _OffsetLine = new Eps_Line();
		Eps_Line _OffsetLine2 = new Eps_Line();
		Eps_Arc _OffsetArc = new Eps_Arc();
		Eps_Arc _OffsetArc2 = new Eps_Arc();
		Eps_Point _MidPoint = new Eps_Point();
		Eps_Line eps_Line = null;
		Eps_Arc eps_Arc = null;
		Eps_Point _StaOffPnt = new Eps_Point();
		if (_Leg == null)
		{
			return;
		}
		if (_bFirstCall && _Leg.ElementList != null && _Leg.ElementList.Count > 0 && !_PlacementSettings.DeleteExistingChecked && _PlacementSettings.GenericModelIDList == null)
		{
			if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.ADAStallSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.BumperSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.IslandSymbol))
			{
				if (_Leg.ElementList != null && _Leg.ElementList.Count > 0)
				{
					List<Element> elementList = _Leg.ElementList;
					if (_RemainingLeg != null && _RemainingLeg.ElementList != null && _RemainingLeg.ElementList.Count > 0)
					{
						int i = 0;
						if (_PlacementSettings.Parking.MiddleStallPlacement)
						{
							i = 1;
						}
						for (; i < _RemainingLeg.ElementList.Count; i++)
						{
							elementList.Add(_RemainingLeg.ElementList[i]);
						}
					}
					_PlacementSettings.GenericModelIDList = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, elementList);
				}
			}
			else
			{
				WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "ParkingLotNoPositioning", EPShared.LocalRM.GetString("PositionParkingLotsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
				warningOptionsForm.ShowDialog();
			}
		}
		double _dDistance = 0.0;
		double _dDirection = 0.0;
		double num = 0.0;
		if (_bFirstCall)
		{
			num = _PlacementSettings.StartingDistance;
		}
		else
		{
			_bDrawFirstDoubleStripe = true;
		}
		for (eps_Leg = _Leg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDistance);
				if (RevitDefines.GT(num, _dDistance))
				{
					num -= _dDistance;
				}
				else
				{
					eps_Line = new Eps_Line(eps_Leg.Line);
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
					EPShared.eplGeoE_CalcPolarToXY(eps_Leg.Line.StartPnt, _dDirection, num, ref _StaOffPnt);
					eps_Line.StartPnt = _StaOffPnt;
					if (_PlacementSettings.PlacementType == PlacementType.ParkingSingleRow)
					{
						if (_bUseRightLeg)
						{
							EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlacementSettings.Parking.StallLength, 1, ref _OffsetLine);
						}
						else
						{
							EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlacementSettings.Parking.StallLength, 2, ref _OffsetLine);
						}
						EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(_OffsetLine.StartPnt, _OffsetLine.EndPnt, ref _MidPoint);
						double _dAngle = 0.0;
						EPShared.eplGeoE_CalcHorzAngByThreePnts(eps_Line.StartPnt, eps_Line.EndPnt, _MidPoint, 12, ref _dAngle, 1E-05);
						_dAngle = ((!RevitDefines.GT(_dAngle, Math.PI)) ? _PlacementSettings.RotationAngle : EPShared.eplGeoE_NormalizeRadians(_PlacementSettings.RotationAngle * -1.0));
						if (RevitDefines.GT(_PlacementSettings.Parking.IslandWidth, 0.0))
						{
							if (_bUseRightLeg)
							{
								EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlacementSettings.Parking.IslandWidth, 1, ref _OffsetLine2);
							}
							else
							{
								EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlacementSettings.Parking.IslandWidth, 2, ref _OffsetLine2);
							}
						}
						else
						{
							_OffsetLine2 = eps_Line;
						}
						_ProgressFrm.Message = EPShared.LocalRM.GetString("PlacingStallMessage") + _nProgressBarCount;
						CreateSingleStalls(_Document, ref _PlacementSettings, _ProgressFrm, eps_Leg.Element, _OffsetLine2, _dAngle, _bDoubleRowStallAdjustment: false, eps_Leg.Next, ref _RemainingLeg, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, ref _bDrawFirstDoubleStripe);
					}
					else if (_PlacementSettings.PlacementType == PlacementType.ParkingDoubleRow)
					{
						if (_PlacementSettings.Parking.PlaceIsland && RevitDefines.GT(_PlacementSettings.Parking.IslandWidth, 0.0))
						{
							Eps_Line _OffsetLine3 = new Eps_Line();
							EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlacementSettings.Parking.IslandWidth / 2.0, 1, ref _OffsetLine3);
							Eps_Line _OffsetLine4 = new Eps_Line();
							EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlacementSettings.Parking.IslandWidth / 2.0, 2, ref _OffsetLine4);
							_ProgressFrm.Message = EPShared.LocalRM.GetString("PlacingStallMessage") + _nProgressBarCount;
							bool _bDrawFirstDoubleStripe2 = _bDrawFirstDoubleStripe;
							CreateSingleStalls(_Document, ref _PlacementSettings, _ProgressFrm, eps_Leg.Element, _OffsetLine3, EPShared.eplGeoE_NormalizeRadians(_PlacementSettings.RotationAngle * -1.0), _bDoubleRowStallAdjustment: true, eps_Leg.Next, ref _RemainingLeg, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, ref _bDrawFirstDoubleStripe2);
							CreateSingleStalls(_Document, ref _PlacementSettings, _ProgressFrm, eps_Leg.Element, _OffsetLine4, _PlacementSettings.RotationAngle, _bDoubleRowStallAdjustment: false, eps_Leg.Next, ref _RemainingLeg, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, ref _bDrawFirstDoubleStripe);
						}
						else
						{
							_ProgressFrm.Message = EPShared.LocalRM.GetString("PlacingStallMessage") + _nProgressBarCount;
							CreateDoubleStalls(_Document, ref _PlacementSettings, _ProgressFrm, eps_Leg.Element, eps_Line, eps_Leg.Next, ref _RemainingLeg, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, ref _bDrawFirstDoubleStripe);
						}
					}
					num = 0.0;
				}
			}
			else if (eps_Leg.Arc != null)
			{
				_dDistance = eps_Leg.Arc.Radius * eps_Leg.Arc.Delta;
				if (RevitDefines.GT(num, _dDistance))
				{
					num -= _dDistance;
				}
				else
				{
					eps_Arc = new Eps_Arc(eps_Leg.Arc);
					EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num, 0.0, ref _StaOffPnt);
					eps_Arc.Delta = (_dDistance - num) / eps_Leg.Arc.Radius;
					eps_Arc.StartPnt = _StaOffPnt;
					if (_PlacementSettings.PlacementType == PlacementType.ParkingSingleRow)
					{
						if (_bUseRightLeg)
						{
							EPShared.eplGeoE_ConstructOffsetArc(eps_Leg.Arc, _PlacementSettings.Parking.StallLength, 1, ref _OffsetArc2);
						}
						else
						{
							EPShared.eplGeoE_ConstructOffsetArc(eps_Leg.Arc, _PlacementSettings.Parking.StallLength, 2, ref _OffsetArc2);
						}
						bool bIsInside = false;
						if (EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_OffsetArc2.Radius, _OffsetArc2.Delta) < EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg.Arc.Radius, eps_Leg.Arc.Delta))
						{
							bIsInside = true;
						}
						if (RevitDefines.GT(_PlacementSettings.Parking.IslandWidth, 0.0))
						{
							if (_bUseRightLeg)
							{
								EPShared.eplGeoE_ConstructOffsetArc(eps_Leg.Arc, _PlacementSettings.Parking.IslandWidth, 1, ref _OffsetArc);
							}
							else
							{
								EPShared.eplGeoE_ConstructOffsetArc(eps_Leg.Arc, _PlacementSettings.Parking.IslandWidth, 2, ref _OffsetArc);
							}
						}
						else
						{
							_OffsetArc = eps_Leg.Arc;
						}
						_ProgressFrm.Message = EPShared.LocalRM.GetString("PlacingStallMessage") + _nProgressBarCount;
						DrawCurvedParking(_Document, ref _PlacementSettings, _ProgressFrm, eps_Leg.Element, _OffsetArc, bIsInside, _bDoubleRowStallAdjustment: false, eps_Leg.Next, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
					}
					else if (_PlacementSettings.PlacementType == PlacementType.ParkingDoubleRow)
					{
						if (_PlacementSettings.Parking.PlaceIsland && RevitDefines.GT(_PlacementSettings.Parking.IslandWidth, 0.0))
						{
							Eps_Arc _OffsetArc3 = new Eps_Arc();
							EPShared.eplGeoE_ConstructOffsetArc(eps_Leg.Arc, _PlacementSettings.Parking.IslandWidth / 2.0, 1, ref _OffsetArc3);
							Eps_Arc _OffsetArc4 = new Eps_Arc();
							EPShared.eplGeoE_ConstructOffsetArc(eps_Leg.Arc, _PlacementSettings.Parking.IslandWidth / 2.0, 2, ref _OffsetArc4);
							_ProgressFrm.Message = EPShared.LocalRM.GetString("PlacingStallMessage") + _nProgressBarCount;
							DrawCurvedParking(_Document, ref _PlacementSettings, _ProgressFrm, eps_Leg.Element, _OffsetArc3, _bIsInside: true, _bDoubleRowStallAdjustment: true, eps_Leg.Next, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
							DrawCurvedParking(_Document, ref _PlacementSettings, _ProgressFrm, eps_Leg.Element, _OffsetArc4, _bIsInside: false, _bDoubleRowStallAdjustment: false, eps_Leg.Next, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
						}
						else
						{
							_ProgressFrm.Message = EPShared.LocalRM.GetString("PlacingStallMessage") + _nProgressBarCount;
							DrawCurvedParking(_Document, ref _PlacementSettings, _ProgressFrm, eps_Leg.Element, eps_Leg.Arc, _bIsInside: true, _bDoubleRowStallAdjustment: true, eps_Leg.Next, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
							DrawCurvedParking(_Document, ref _PlacementSettings, _ProgressFrm, eps_Leg.Element, eps_Leg.Arc, _bIsInside: false, _bDoubleRowStallAdjustment: false, eps_Leg.Next, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
						}
					}
				}
				num = 0.0;
			}
			_nProgressBarCount++;
		}
	}

	public static bool PlaceStallsOnArc(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Arc _ParkingArc, Eps_Arc _FirstArc, Eps_Arc _SecondArc, bool _bUseFirstArc)
	{
		List<FamilyInstance> _ExistingParkingStallList = null;
		List<FamilyInstance> _ExistingADAParkingStallList = null;
		List<FamilyInstance> _ExistingBumperList = null;
		return PlaceStallsOnArc(_Document, _PlacementSettings, _Element, _ParkingArc, _FirstArc, _SecondArc, _bUseFirstArc, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
	}

	public static bool PlaceStallsOnArc(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Arc _ParkingArc, Eps_Arc _FirstArc, Eps_Arc _SecondArc, bool _bUseFirstArc, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		Eps_Arc eps_Arc = null;
		double num = 0.0;
		double startingDistance = _PlacementSettings.StartingDistance;
		if (_bUseFirstArc)
		{
			num = _FirstArc.Radius * _FirstArc.Delta;
			eps_Arc = new Eps_Arc(_FirstArc);
		}
		else
		{
			num = _SecondArc.Radius * _SecondArc.Delta;
			eps_Arc = new Eps_Arc(_SecondArc);
		}
		EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Arc, startingDistance, 0.0, ref _StaOffPnt);
		eps_Arc.Delta = (num - startingDistance) / eps_Arc.Radius;
		eps_Arc.StartPnt = _StaOffPnt;
		if (_bUseFirstArc)
		{
			_FirstArc = new Eps_Arc(eps_Arc);
		}
		else
		{
			_SecondArc = new Eps_Arc(eps_Arc);
		}
		if (_PlacementSettings.PlacementType == PlacementType.ParkingSingleRow)
		{
			Eps_Arc _OffsetArc = new Eps_Arc();
			EPShared.eplGeoE_ConstructOffsetArc(_ParkingArc, _PlacementSettings.Parking.StallLength, 1, ref _OffsetArc);
			Eps_Arc _OffsetArc2 = new Eps_Arc();
			EPShared.eplGeoE_ConstructOffsetArc(_ParkingArc, _PlacementSettings.Parking.StallLength, 2, ref _OffsetArc2);
			if (_PlacementSettings.TransactionContainer != null)
			{
				ElementSet elementSet = new ElementSet();
				if (_OffsetArc.Radius > 0.0)
				{
					_PlacementSettings.TransactionContainer.BeginTransaction();
					ModelCurve item = RevitUtility.NewModelCurve(_Document, _OffsetArc);
					_PlacementSettings.TransactionContainer.EndTransaction();
					elementSet.Insert(item);
				}
				if (_OffsetArc2.Radius > 0.0)
				{
					_PlacementSettings.TransactionContainer.BeginTransaction();
					ModelCurve item2 = RevitUtility.NewModelCurve(_Document, _OffsetArc2);
					_PlacementSettings.TransactionContainer.EndTransaction();
					elementSet.Insert(item2);
				}
				bool _UseRightSide = true;
				bool flag = RevitUtility.PickSideArc(_Document, _ParkingArc, ref _UseRightSide);
				_PlacementSettings.TransactionContainer.BeginTransaction();
				_Document.Delete(ElementSetConverter.ToIDList(elementSet));
				_PlacementSettings.TransactionContainer.EndTransaction();
				if (!flag)
				{
					return false;
				}
				_PlacementSettings.UseRightSide = _UseRightSide;
			}
			bool bIsInside = false;
			if (_PlacementSettings.UseRightSide)
			{
				if (EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_OffsetArc.Radius, _OffsetArc.Delta) < EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_ParkingArc.Radius, _ParkingArc.Delta))
				{
					bIsInside = true;
				}
			}
			else if (EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_OffsetArc2.Radius, _OffsetArc2.Delta) < EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_ParkingArc.Radius, _ParkingArc.Delta))
			{
				bIsInside = true;
			}
			if (RevitDefines.GT(_PlacementSettings.Parking.IslandWidth, 0.0))
			{
				if (_PlacementSettings.UseRightSide)
				{
					if (_FirstArc != null)
					{
						EPShared.eplGeoE_ConstructOffsetArc(_FirstArc, _PlacementSettings.Parking.IslandWidth, 2, ref _FirstArc);
					}
					if (_SecondArc != null)
					{
						EPShared.eplGeoE_ConstructOffsetArc(_SecondArc, _PlacementSettings.Parking.IslandWidth, 1, ref _SecondArc);
					}
				}
				else
				{
					if (_FirstArc != null)
					{
						EPShared.eplGeoE_ConstructOffsetArc(_FirstArc, _PlacementSettings.Parking.IslandWidth, 1, ref _FirstArc);
					}
					if (_SecondArc != null)
					{
						EPShared.eplGeoE_ConstructOffsetArc(_SecondArc, _PlacementSettings.Parking.IslandWidth, 2, ref _SecondArc);
					}
				}
			}
			if (!_PlacementSettings.DeleteExistingChecked && _PlacementSettings.GenericModelIDList == null && _Element != null)
			{
				if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.ADAStallSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.BumperSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.IslandSymbol))
				{
					_PlacementSettings.GenericModelIDList = new List<KeyValuePair<string, string>>();
					_PlacementSettings.GenericModelIDList.Add(RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Element));
				}
				else
				{
					WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "ParkingLotNoPositioning", EPShared.LocalRM.GetString("PositionParkingLotsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
					warningOptionsForm.ShowDialog();
				}
			}
			if (_PlacementSettings.TransactionContainer != null)
			{
				_PlacementSettings.TransactionContainer.BeginTransaction();
			}
			ProgressBarForm progressBarForm = new ProgressBarForm();
			progressBarForm.ShowForm();
			progressBarForm.Refresh();
			progressBarForm.Message = EPShared.LocalRM.GetString("StallOnArcMessage");
			if (_bUseFirstArc)
			{
				if (_FirstArc != null)
				{
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm, _Element, _FirstArc, bIsInside, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
				}
				if (_SecondArc != null)
				{
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm, _Element, _SecondArc, bIsInside, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
				}
			}
			else
			{
				if (_SecondArc != null)
				{
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm, _Element, _SecondArc, bIsInside, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
				}
				if (_FirstArc != null)
				{
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm, _Element, _FirstArc, bIsInside, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
				}
			}
			progressBarForm.Close();
			if (_ExistingParkingStallList != null && _ExistingParkingStallList.Count > 0)
			{
				for (int i = 0; i < _ExistingParkingStallList.Count; i++)
				{
					_Document.Delete(_ExistingParkingStallList[i].Id);
				}
			}
			if (_ExistingADAParkingStallList != null && _ExistingADAParkingStallList.Count > 0)
			{
				for (int j = 0; j < _ExistingADAParkingStallList.Count; j++)
				{
					_Document.Delete(_ExistingADAParkingStallList[j].Id);
				}
			}
			if (_ExistingBumperList != null && _ExistingBumperList.Count > 0)
			{
				for (int k = 0; k < _ExistingBumperList.Count; k++)
				{
					_Document.Delete(_ExistingBumperList[k].Id);
				}
			}
			if (_PlacementSettings.TransactionContainer != null)
			{
				_PlacementSettings.TransactionContainer.EndTransaction();
			}
		}
		else if (_PlacementSettings.PlacementType == PlacementType.ParkingDoubleRow)
		{
			if (!_PlacementSettings.DeleteExistingChecked && _PlacementSettings.GenericModelIDList == null && _Element != null)
			{
				if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.ADAStallSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.BumperSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.IslandSymbol))
				{
					_PlacementSettings.GenericModelIDList = new List<KeyValuePair<string, string>>();
					_PlacementSettings.GenericModelIDList.Add(RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Element));
				}
				else
				{
					WarningOptionsForm warningOptionsForm2 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "ParkingLotNoPositioning", EPShared.LocalRM.GetString("PositionParkingLotsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
					warningOptionsForm2.ShowDialog();
				}
			}
			if (_PlacementSettings.TransactionContainer != null)
			{
				_PlacementSettings.TransactionContainer.BeginTransaction();
			}
			ProgressBarForm progressBarForm2 = new ProgressBarForm();
			progressBarForm2.ShowForm();
			progressBarForm2.Refresh();
			progressBarForm2.Message = EPShared.LocalRM.GetString("StallOnArcMessage");
			if (_PlacementSettings.Parking.PlaceIsland && RevitDefines.GT(_PlacementSettings.Parking.IslandWidth, 0.0))
			{
				if (_bUseFirstArc)
				{
					if (_FirstArc != null)
					{
						Eps_Arc _OffsetArc3 = new Eps_Arc();
						EPShared.eplGeoE_ConstructOffsetArc(_FirstArc, _PlacementSettings.Parking.IslandWidth / 2.0, 1, ref _OffsetArc3);
						Eps_Arc _OffsetArc4 = new Eps_Arc();
						EPShared.eplGeoE_ConstructOffsetArc(_FirstArc, _PlacementSettings.Parking.IslandWidth / 2.0, 2, ref _OffsetArc4);
						DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _OffsetArc3, _bIsInside: false, _bDoubleRowStallAdjustment: true, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
						DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _OffsetArc4, _bIsInside: true, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
					}
					if (_SecondArc != null)
					{
						Eps_Arc _OffsetArc5 = new Eps_Arc();
						EPShared.eplGeoE_ConstructOffsetArc(_SecondArc, _PlacementSettings.Parking.IslandWidth / 2.0, 1, ref _OffsetArc5);
						Eps_Arc _OffsetArc6 = new Eps_Arc();
						EPShared.eplGeoE_ConstructOffsetArc(_SecondArc, _PlacementSettings.Parking.IslandWidth / 2.0, 2, ref _OffsetArc6);
						DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _OffsetArc5, _bIsInside: true, _bDoubleRowStallAdjustment: true, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
						DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _OffsetArc6, _bIsInside: false, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
					}
				}
				else
				{
					if (_SecondArc != null)
					{
						Eps_Arc _OffsetArc7 = new Eps_Arc();
						EPShared.eplGeoE_ConstructOffsetArc(_SecondArc, _PlacementSettings.Parking.IslandWidth / 2.0, 1, ref _OffsetArc7);
						Eps_Arc _OffsetArc8 = new Eps_Arc();
						EPShared.eplGeoE_ConstructOffsetArc(_SecondArc, _PlacementSettings.Parking.IslandWidth / 2.0, 2, ref _OffsetArc8);
						DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _OffsetArc7, _bIsInside: true, _bDoubleRowStallAdjustment: true, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
						DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _OffsetArc8, _bIsInside: false, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
					}
					if (_FirstArc != null)
					{
						Eps_Arc _OffsetArc9 = new Eps_Arc();
						EPShared.eplGeoE_ConstructOffsetArc(_FirstArc, _PlacementSettings.Parking.IslandWidth / 2.0, 1, ref _OffsetArc9);
						Eps_Arc _OffsetArc10 = new Eps_Arc();
						EPShared.eplGeoE_ConstructOffsetArc(_FirstArc, _PlacementSettings.Parking.IslandWidth / 2.0, 2, ref _OffsetArc10);
						DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _OffsetArc9, _bIsInside: false, _bDoubleRowStallAdjustment: true, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
						DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _OffsetArc10, _bIsInside: true, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
					}
				}
			}
			else if (_bUseFirstArc)
			{
				if (_FirstArc != null)
				{
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _FirstArc, _bIsInside: true, _bDoubleRowStallAdjustment: true, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _FirstArc, _bIsInside: false, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
				}
				if (_SecondArc != null)
				{
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _SecondArc, _bIsInside: true, _bDoubleRowStallAdjustment: true, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _SecondArc, _bIsInside: false, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
				}
			}
			else
			{
				if (_SecondArc != null)
				{
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _SecondArc, _bIsInside: true, _bDoubleRowStallAdjustment: true, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _SecondArc, _bIsInside: false, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
				}
				if (_FirstArc != null)
				{
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _FirstArc, _bIsInside: true, _bDoubleRowStallAdjustment: true, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
					DrawCurvedParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _FirstArc, _bIsInside: false, _bDoubleRowStallAdjustment: false, null, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList);
				}
			}
			progressBarForm2.Close();
			if (_ExistingParkingStallList != null && _ExistingParkingStallList.Count > 0)
			{
				for (int l = 0; l < _ExistingParkingStallList.Count; l++)
				{
					_Document.Delete(_ExistingParkingStallList[l].Id);
				}
			}
			if (_ExistingADAParkingStallList != null && _ExistingADAParkingStallList.Count > 0)
			{
				for (int m = 0; m < _ExistingADAParkingStallList.Count; m++)
				{
					_Document.Delete(_ExistingADAParkingStallList[m].Id);
				}
			}
			if (_ExistingBumperList != null && _ExistingBumperList.Count > 0)
			{
				for (int n = 0; n < _ExistingBumperList.Count; n++)
				{
					_Document.Delete(_ExistingBumperList[n].Id);
				}
			}
			if (_PlacementSettings.TransactionContainer != null)
			{
				_PlacementSettings.TransactionContainer.EndTransaction();
			}
		}
		return true;
	}

	public static bool PlaceStallsOnLine(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Line _ParkingLotLine, Eps_Line _FirstLine, Eps_Line _SecondLine, bool _bUseFirstLine)
	{
		List<FamilyInstance> _ExistingParkingStallList = null;
		List<FamilyInstance> _ExistingADAParkingStallList = null;
		List<FamilyInstance> _ExistingBumperList = null;
		List<FamilyInstance> _ExistingIslandList = null;
		return PlaceStallsOnLine(_Document, _PlacementSettings, _Element, _ParkingLotLine, _FirstLine, _SecondLine, _bUseFirstLine, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, ref _ExistingIslandList);
	}

	public static bool PlaceStallsOnLine(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Line _ParkingLotLine, Eps_Line _FirstLine, Eps_Line _SecondLine, bool _bUseFirstLine, ref List<FamilyInstance> _ExistingParkingStallList, ref List<FamilyInstance> _ExistingADAParkingStallList, ref List<FamilyInstance> _ExistingBumperList, ref List<FamilyInstance> _ExistingIslandList)
	{
		double _dDirection = 0.0;
		Eps_Point _LastCurbPoint = new Eps_Point();
		Eps_Point _LastCurbPoint2 = new Eps_Point();
		Eps_Point _NewPnt = new Eps_Point();
		Eps_Line eps_Line = new Eps_Line();
		Eps_Line eps_Line2 = null;
		double startingDistance = _PlacementSettings.StartingDistance;
		eps_Line2 = ((!_bUseFirstLine) ? new Eps_Line(_SecondLine) : new Eps_Line(_FirstLine));
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Line2.StartPnt, eps_Line2.EndPnt, ref _dDirection);
		EPShared.eplGeoE_CalcPolarToXY(eps_Line2.StartPnt, _dDirection, startingDistance, ref _NewPnt);
		eps_Line2.StartPnt = _NewPnt;
		if (_bUseFirstLine)
		{
			_FirstLine = new Eps_Line(eps_Line2);
		}
		else
		{
			_SecondLine = new Eps_Line(eps_Line2);
		}
		GetIslandParameters(_Document, ref _PlacementSettings);
		if (_PlacementSettings.PlacementType == PlacementType.ParkingSingleRow || _PlacementSettings.PlacementType == PlacementType.ParkingArea)
		{
			Eps_Line _OffsetLine = new Eps_Line();
			EPShared.eplGeoE_ConstructOffsetLine(_ParkingLotLine, _PlacementSettings.Parking.StallLength, 1, ref _OffsetLine);
			Eps_Line _OffsetLine2 = new Eps_Line();
			EPShared.eplGeoE_ConstructOffsetLine(_ParkingLotLine, _PlacementSettings.Parking.StallLength, 2, ref _OffsetLine2);
			if (_PlacementSettings.TransactionContainer != null)
			{
				ElementSet elementSet = new ElementSet();
				_PlacementSettings.TransactionContainer.BeginTransaction();
				ModelCurve item = RevitUtility.NewModelCurve(_Document, _OffsetLine);
				ModelCurve item2 = RevitUtility.NewModelCurve(_Document, _OffsetLine2);
				_PlacementSettings.TransactionContainer.EndTransaction();
				_Document.Regenerate();
				elementSet.Insert(item);
				elementSet.Insert(item2);
				bool _UseRightSide = true;
				bool flag = RevitUtility.PickSideLine(_Document, _ParkingLotLine, ref _UseRightSide);
				_PlacementSettings.TransactionContainer.BeginTransaction();
				_Document.Delete(ElementSetConverter.ToIDList(elementSet));
				_PlacementSettings.TransactionContainer.EndTransaction();
				if (!flag)
				{
					return false;
				}
				_PlacementSettings.UseRightSide = _UseRightSide;
			}
			Eps_Point _MidPoint = new Eps_Point();
			if (_PlacementSettings.PlacementType == PlacementType.ParkingSingleRow)
			{
				if (_PlacementSettings.UseRightSide)
				{
					EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(_OffsetLine.StartPnt, _OffsetLine.EndPnt, ref _MidPoint);
				}
				else
				{
					EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(_OffsetLine2.StartPnt, _OffsetLine2.EndPnt, ref _MidPoint);
				}
				double _dAngle = 0.0;
				EPShared.eplGeoE_CalcHorzAngByThreePnts(_ParkingLotLine.StartPnt, _ParkingLotLine.EndPnt, _MidPoint, 12, ref _dAngle, 1E-05);
				_dAngle = ((!RevitDefines.GT(_dAngle, Math.PI)) ? _PlacementSettings.RotationAngle : EPShared.eplGeoE_NormalizeRadians(_PlacementSettings.RotationAngle * -1.0));
				if (RevitDefines.GT(_PlacementSettings.Parking.IslandWidth, 0.0))
				{
					if (_PlacementSettings.UseRightSide)
					{
						if (_FirstLine != null)
						{
							EPShared.eplGeoE_ConstructOffsetLine(_FirstLine, _PlacementSettings.Parking.IslandWidth, 2, ref _FirstLine);
						}
						if (_SecondLine != null)
						{
							EPShared.eplGeoE_ConstructOffsetLine(_SecondLine, _PlacementSettings.Parking.IslandWidth, 1, ref _SecondLine);
						}
					}
					else
					{
						if (_FirstLine != null)
						{
							EPShared.eplGeoE_ConstructOffsetLine(_FirstLine, _PlacementSettings.Parking.IslandWidth, 1, ref _FirstLine);
						}
						if (_SecondLine != null)
						{
							EPShared.eplGeoE_ConstructOffsetLine(_SecondLine, _PlacementSettings.Parking.IslandWidth, 2, ref _SecondLine);
						}
					}
				}
				if (!_PlacementSettings.DeleteExistingChecked && _PlacementSettings.GenericModelIDList == null && _Element != null)
				{
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.ADAStallSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.BumperSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.IslandSymbol))
					{
						_PlacementSettings.GenericModelIDList = new List<KeyValuePair<string, string>>();
						_PlacementSettings.GenericModelIDList.Add(RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Element));
					}
					else
					{
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "ParkingLotNoPositioning", EPShared.LocalRM.GetString("PositionParkingLotsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
					}
				}
				if (_PlacementSettings.TransactionContainer != null)
				{
					_PlacementSettings.TransactionContainer.BeginTransaction();
				}
				ProgressBarForm progressBarForm = new ProgressBarForm();
				progressBarForm.ShowForm();
				progressBarForm.Refresh();
				progressBarForm.Message = EPShared.LocalRM.GetString("StallOnLineMessage");
				bool bDrawFirstDoubleStripe = true;
				if (_bUseFirstLine)
				{
					if (_FirstLine != null)
					{
						CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm, _Element, _FirstLine, EPShared.eplGeoE_NormalizeRadians(_dAngle + Math.PI), _bDoubleRowStallAdjustment: false, ref _LastCurbPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe);
						eps_Line.StartPnt = _LastCurbPoint;
						if (_SecondLine == null)
						{
							eps_Line.EndPnt = _FirstLine.StartPnt;
						}
					}
					if (_SecondLine != null)
					{
						if (RevitDefines.EQ(startingDistance, 0.0))
						{
							bDrawFirstDoubleStripe = false;
						}
						CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm, _Element, _SecondLine, _dAngle, _bDoubleRowStallAdjustment: false, ref _LastCurbPoint2, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe);
						eps_Line.EndPnt = _LastCurbPoint2;
						if (_FirstLine == null)
						{
							eps_Line.StartPnt = _SecondLine.StartPnt;
						}
					}
				}
				else
				{
					if (_SecondLine != null)
					{
						CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm, _Element, _SecondLine, _dAngle, _bDoubleRowStallAdjustment: false, ref _LastCurbPoint2, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe);
						eps_Line.EndPnt = _LastCurbPoint2;
						if (_FirstLine == null)
						{
							eps_Line.StartPnt = _SecondLine.StartPnt;
						}
					}
					if (_FirstLine != null)
					{
						if (RevitDefines.EQ(startingDistance, 0.0))
						{
							bDrawFirstDoubleStripe = false;
						}
						CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm, _Element, _FirstLine, EPShared.eplGeoE_NormalizeRadians(_dAngle + Math.PI), _bDoubleRowStallAdjustment: false, ref _LastCurbPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe);
						eps_Line.StartPnt = _LastCurbPoint;
						if (_SecondLine == null)
						{
							eps_Line.EndPnt = _FirstLine.StartPnt;
						}
					}
				}
				if (_PlacementSettings.Parking.IslandSymbol != null && _PlacementSettings.Parking.IslandWidth > 0.0)
				{
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Line.StartPnt, eps_Line.EndPnt, ref _dDirection);
					if (!_PlacementSettings.Parking.CenterIsland)
					{
						EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI + _dDirection), _PlacementSettings.Parking.IslandWidth, ref _LastCurbPoint);
						DrawIsland(_Document, _PlacementSettings, _Element, _LastCurbPoint, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, _dDirection, ref _ExistingIslandList);
						DrawIsland(_Document, _PlacementSettings, _Element, eps_Line.EndPnt, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, _dDirection, ref _ExistingIslandList);
					}
					else
					{
						double _dDistance = 0.0;
						EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(eps_Line.StartPnt, eps_Line.EndPnt, ref _NewPnt);
						EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Line.StartPnt, eps_Line.EndPnt, ref _dDistance);
						if (RevitDefines.GT(_dAngle, Math.PI))
						{
							EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI / 2.0), _PlacementSettings.Parking.IslandWidth, ref _NewPnt);
						}
						else
						{
							EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection - Math.PI / 2.0), _PlacementSettings.Parking.IslandWidth, ref _NewPnt);
						}
						DrawIsland(_Document, _PlacementSettings, _Element, _NewPnt, _dDistance, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, _dDirection, ref _ExistingIslandList);
					}
				}
				progressBarForm.Close();
				if (_ExistingParkingStallList != null && _ExistingParkingStallList.Count > 0)
				{
					for (int i = 0; i < _ExistingParkingStallList.Count; i++)
					{
						_Document.Delete(_ExistingParkingStallList[i].Id);
					}
				}
				if (_ExistingADAParkingStallList != null && _ExistingADAParkingStallList.Count > 0)
				{
					for (int j = 0; j < _ExistingADAParkingStallList.Count; j++)
					{
						_Document.Delete(_ExistingADAParkingStallList[j].Id);
					}
				}
				if (_ExistingBumperList != null && _ExistingBumperList.Count > 0)
				{
					for (int k = 0; k < _ExistingBumperList.Count; k++)
					{
						_Document.Delete(_ExistingBumperList[k].Id);
					}
				}
				if (_ExistingIslandList != null && _ExistingIslandList.Count > 0)
				{
					for (int l = 0; l < _ExistingIslandList.Count; l++)
					{
						_Document.Delete(_ExistingIslandList[l].Id);
					}
				}
				if (_PlacementSettings.TransactionContainer != null)
				{
					_PlacementSettings.TransactionContainer.EndTransaction();
				}
			}
			else
			{
				_MidPoint = ((!_PlacementSettings.UseRightSide) ? _OffsetLine2.StartPnt : _OffsetLine.StartPnt);
				if (!_PlacementSettings.DeleteExistingChecked && _PlacementSettings.GenericModelIDList == null && _Element != null)
				{
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.ADAStallSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.BumperSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.IslandSymbol))
					{
						_PlacementSettings.GenericModelIDList = new List<KeyValuePair<string, string>>();
						_PlacementSettings.GenericModelIDList.Add(RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Element));
					}
					else
					{
						WarningOptionsForm warningOptionsForm2 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "ParkingLotNoPositioning", EPShared.LocalRM.GetString("PositionParkingLotsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm2.ShowDialog();
					}
				}
				if (_PlacementSettings.TransactionContainer != null)
				{
					_PlacementSettings.TransactionContainer.BeginTransaction();
				}
				ProgressBarForm progressBarForm2 = new ProgressBarForm();
				progressBarForm2.ShowForm();
				progressBarForm2.Refresh();
				progressBarForm2.Message = EPShared.LocalRM.GetString("StallOnLineMessage");
				DrawAreaParking(_Document, ref _PlacementSettings, progressBarForm2, _Element, _ParkingLotLine, _MidPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, ref _ExistingIslandList);
				progressBarForm2.Close();
				if (_ExistingParkingStallList != null && _ExistingParkingStallList.Count > 0)
				{
					for (int m = 0; m < _ExistingParkingStallList.Count; m++)
					{
						_Document.Delete(_ExistingParkingStallList[m].Id);
					}
				}
				if (_ExistingADAParkingStallList != null && _ExistingADAParkingStallList.Count > 0)
				{
					for (int n = 0; n < _ExistingADAParkingStallList.Count; n++)
					{
						_Document.Delete(_ExistingADAParkingStallList[n].Id);
					}
				}
				if (_ExistingBumperList != null && _ExistingBumperList.Count > 0)
				{
					for (int num = 0; num < _ExistingBumperList.Count; num++)
					{
						_Document.Delete(_ExistingBumperList[num].Id);
					}
				}
				if (_ExistingIslandList != null && _ExistingIslandList.Count > 0)
				{
					for (int num2 = 0; num2 < _ExistingIslandList.Count; num2++)
					{
						_Document.Delete(_ExistingIslandList[num2].Id);
					}
				}
				if (_PlacementSettings.TransactionContainer != null)
				{
					_PlacementSettings.TransactionContainer.EndTransaction();
				}
			}
		}
		else if (_PlacementSettings.PlacementType == PlacementType.ParkingDoubleRow)
		{
			if (!_PlacementSettings.DeleteExistingChecked && _PlacementSettings.GenericModelIDList == null && _Element != null)
			{
				if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.ADAStallSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.BumperSymbol) && RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Parking.IslandSymbol))
				{
					_PlacementSettings.GenericModelIDList = new List<KeyValuePair<string, string>>();
					_PlacementSettings.GenericModelIDList.Add(RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Element));
				}
				else
				{
					WarningOptionsForm warningOptionsForm3 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "ParkingLotNoPositioning", EPShared.LocalRM.GetString("PositionParkingLotsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
					warningOptionsForm3.ShowDialog();
				}
			}
			if (_PlacementSettings.TransactionContainer != null)
			{
				_PlacementSettings.TransactionContainer.BeginTransaction();
			}
			ProgressBarForm progressBarForm3 = new ProgressBarForm();
			progressBarForm3.ShowForm();
			progressBarForm3.Refresh();
			progressBarForm3.Message = EPShared.LocalRM.GetString("StallOnLineMessage");
			bool bDrawFirstDoubleStripe2 = true;
			if (_PlacementSettings.Parking.PlaceIsland && RevitDefines.GT(_PlacementSettings.Parking.IslandWidth, 0.0))
			{
				if (_bUseFirstLine)
				{
					if (_FirstLine != null)
					{
						Eps_Line _OffsetLine3 = new Eps_Line();
						EPShared.eplGeoE_ConstructOffsetLine(_FirstLine, _PlacementSettings.Parking.IslandWidth / 2.0, 1, ref _OffsetLine3);
						Eps_Line _OffsetLine4 = new Eps_Line();
						EPShared.eplGeoE_ConstructOffsetLine(_FirstLine, _PlacementSettings.Parking.IslandWidth / 2.0, 2, ref _OffsetLine4);
						CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _OffsetLine3, EPShared.eplGeoE_NormalizeRadians(_PlacementSettings.RotationAngle * -1.0), _bDoubleRowStallAdjustment: true, ref _NewPnt, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
						if (_SecondLine == null)
						{
							CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _OffsetLine4, _PlacementSettings.RotationAngle, _bDoubleRowStallAdjustment: false, ref _LastCurbPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
							eps_Line.EndPnt = _OffsetLine4.StartPnt;
						}
						else
						{
							CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _OffsetLine4, _PlacementSettings.RotationAngle, _bDoubleRowStallAdjustment: true, ref _LastCurbPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
						}
						eps_Line.StartPnt = _LastCurbPoint;
					}
					if (_SecondLine != null)
					{
						Eps_Line _OffsetLine5 = new Eps_Line();
						EPShared.eplGeoE_ConstructOffsetLine(_SecondLine, _PlacementSettings.Parking.IslandWidth / 2.0, 1, ref _OffsetLine5);
						Eps_Line _OffsetLine6 = new Eps_Line();
						EPShared.eplGeoE_ConstructOffsetLine(_SecondLine, _PlacementSettings.Parking.IslandWidth / 2.0, 2, ref _OffsetLine6);
						bDrawFirstDoubleStripe2 = false;
						CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _OffsetLine5, EPShared.eplGeoE_NormalizeRadians(_PlacementSettings.RotationAngle * -1.0), _bDoubleRowStallAdjustment: true, ref _LastCurbPoint2, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
						bDrawFirstDoubleStripe2 = false;
						CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _OffsetLine6, _PlacementSettings.RotationAngle, _bDoubleRowStallAdjustment: false, ref _NewPnt, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
						if (_FirstLine == null)
						{
							eps_Line.StartPnt = _OffsetLine5.StartPnt;
						}
						eps_Line.EndPnt = _LastCurbPoint2;
					}
				}
				else
				{
					if (_SecondLine != null)
					{
						Eps_Line _OffsetLine7 = new Eps_Line();
						EPShared.eplGeoE_ConstructOffsetLine(_SecondLine, _PlacementSettings.Parking.IslandWidth / 2.0, 1, ref _OffsetLine7);
						Eps_Line _OffsetLine8 = new Eps_Line();
						EPShared.eplGeoE_ConstructOffsetLine(_SecondLine, _PlacementSettings.Parking.IslandWidth / 2.0, 2, ref _OffsetLine8);
						CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _OffsetLine7, EPShared.eplGeoE_NormalizeRadians(_PlacementSettings.RotationAngle * -1.0), _bDoubleRowStallAdjustment: true, ref _LastCurbPoint2, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
						if (_FirstLine == null)
						{
							CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _OffsetLine8, _PlacementSettings.RotationAngle, _bDoubleRowStallAdjustment: false, ref _NewPnt, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
							eps_Line.StartPnt = _OffsetLine7.StartPnt;
						}
						else
						{
							CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _OffsetLine8, _PlacementSettings.RotationAngle, _bDoubleRowStallAdjustment: true, ref _NewPnt, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
						}
						eps_Line.EndPnt = _LastCurbPoint2;
					}
					if (_FirstLine != null)
					{
						Eps_Line _OffsetLine9 = new Eps_Line();
						EPShared.eplGeoE_ConstructOffsetLine(_FirstLine, _PlacementSettings.Parking.IslandWidth / 2.0, 1, ref _OffsetLine9);
						Eps_Line _OffsetLine10 = new Eps_Line();
						EPShared.eplGeoE_ConstructOffsetLine(_FirstLine, _PlacementSettings.Parking.IslandWidth / 2.0, 2, ref _OffsetLine10);
						bDrawFirstDoubleStripe2 = false;
						CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _OffsetLine9, EPShared.eplGeoE_NormalizeRadians(_PlacementSettings.RotationAngle * -1.0), _bDoubleRowStallAdjustment: true, ref _NewPnt, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
						bDrawFirstDoubleStripe2 = false;
						CreateSingleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _OffsetLine10, _PlacementSettings.RotationAngle, _bDoubleRowStallAdjustment: false, ref _LastCurbPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
						if (_SecondLine == null)
						{
							eps_Line.EndPnt = _OffsetLine10.StartPnt;
						}
						eps_Line.StartPnt = _LastCurbPoint;
					}
				}
			}
			else if (_bUseFirstLine)
			{
				if (_FirstLine != null)
				{
					CreateDoubleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _FirstLine, _LastCurbPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
					eps_Line.StartPnt = _LastCurbPoint;
					if (_SecondLine == null)
					{
						eps_Line.EndPnt = _FirstLine.StartPnt;
					}
				}
				if (_SecondLine != null)
				{
					bDrawFirstDoubleStripe2 = false;
					CreateDoubleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _SecondLine, _LastCurbPoint2, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
					eps_Line.EndPnt = _LastCurbPoint2;
					if (_FirstLine == null)
					{
						eps_Line.StartPnt = _SecondLine.StartPnt;
					}
				}
			}
			else
			{
				if (_SecondLine != null)
				{
					CreateDoubleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _SecondLine, _LastCurbPoint2, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
					eps_Line.EndPnt = _LastCurbPoint2;
					if (_FirstLine == null)
					{
						eps_Line.StartPnt = _SecondLine.StartPnt;
					}
				}
				if (_FirstLine != null)
				{
					bDrawFirstDoubleStripe2 = false;
					CreateDoubleStalls(_Document, ref _PlacementSettings, progressBarForm3, _Element, _FirstLine, _LastCurbPoint, ref _ExistingParkingStallList, ref _ExistingADAParkingStallList, ref _ExistingBumperList, bDrawFirstDoubleStripe2);
					eps_Line.StartPnt = _LastCurbPoint;
					if (_SecondLine == null)
					{
						eps_Line.EndPnt = _FirstLine.StartPnt;
					}
				}
			}
			if (_PlacementSettings.Parking.IslandSymbol != null && _PlacementSettings.Parking.IslandWidth > 0.0)
			{
				EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Line.StartPnt, eps_Line.EndPnt, ref _dDirection);
				if (!_PlacementSettings.Parking.CenterIsland)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI + _dDirection), _PlacementSettings.Parking.IslandWidth, ref _LastCurbPoint);
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.EndPnt, EPShared.eplGeoE_NormalizeRadians(Math.PI + _dDirection), _PlacementSettings.Parking.IslandWidth, ref _LastCurbPoint2);
					DrawIsland(_Document, _PlacementSettings, _Element, _LastCurbPoint, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, _dDirection, ref _ExistingIslandList);
					DrawIsland(_Document, _PlacementSettings, _Element, eps_Line.StartPnt, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, EPShared.eplGeoE_NormalizeRadians(Math.PI + _dDirection), ref _ExistingIslandList);
					DrawIsland(_Document, _PlacementSettings, _Element, _LastCurbPoint2, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, EPShared.eplGeoE_NormalizeRadians(Math.PI + _dDirection), ref _ExistingIslandList);
					DrawIsland(_Document, _PlacementSettings, _Element, eps_Line.EndPnt, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, _dDirection, ref _ExistingIslandList);
				}
				else
				{
					double _dDistance2 = 0.0;
					EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(eps_Line.StartPnt, eps_Line.EndPnt, ref _NewPnt);
					EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Line.StartPnt, eps_Line.EndPnt, ref _dDistance2);
					EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI / 2.0), _PlacementSettings.Parking.IslandWidth / 2.0, ref _NewPnt);
					DrawIsland(_Document, _PlacementSettings, _Element, _NewPnt, _dDistance2, _PlacementSettings.Parking.IslandWidth, _PlacementSettings.RotationAngle, _PlacementSettings.Parking.StripeLength, _dDirection, ref _ExistingIslandList);
				}
			}
			progressBarForm3.Close();
			if (_ExistingParkingStallList != null && _ExistingParkingStallList.Count > 0)
			{
				for (int num3 = 0; num3 < _ExistingParkingStallList.Count; num3++)
				{
					_Document.Delete(_ExistingParkingStallList[num3].Id);
				}
			}
			if (_ExistingADAParkingStallList != null && _ExistingADAParkingStallList.Count > 0)
			{
				for (int num4 = 0; num4 < _ExistingADAParkingStallList.Count; num4++)
				{
					_Document.Delete(_ExistingADAParkingStallList[num4].Id);
				}
			}
			if (_ExistingBumperList != null && _ExistingBumperList.Count > 0)
			{
				for (int num5 = 0; num5 < _ExistingBumperList.Count; num5++)
				{
					_Document.Delete(_ExistingBumperList[num5].Id);
				}
			}
			if (_ExistingIslandList != null && _ExistingIslandList.Count > 0)
			{
				for (int num6 = 0; num6 < _ExistingIslandList.Count; num6++)
				{
					_Document.Delete(_ExistingIslandList[num6].Id);
				}
			}
			if (_PlacementSettings.TransactionContainer != null)
			{
				_PlacementSettings.TransactionContainer.EndTransaction();
			}
		}
		return true;
	}
}
