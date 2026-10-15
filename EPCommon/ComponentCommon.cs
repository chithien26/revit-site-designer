using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using SITES.Utilities;

namespace EPCommon;

public class ComponentCommon
{
	public static bool PlaceLegList(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _CurrentLeg)
	{
		if (_PlacementSettings.PlacementType == PlacementType.GroveSquare || _PlacementSettings.PlacementType == PlacementType.GroveTrianglular)
		{
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CurrentLeg.LegStartPnt, _CurrentLeg.LegEndPnt, 1E-05))
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("BoundaryClosedChainMessage"), ApplicationUtility.GetString("Error"), MessageBoxIcon.Hand);
				return false;
			}
			List<Eps_Leg> _IslandList = new List<Eps_Leg>();
			PickIslandLegs(_Document, ref _IslandList);
			PlaceHedgeGrove(_Document, _PlacementSettings, _CurrentLeg, _IslandList);
			if (_PlacementSettings.DeleteExistingChecked)
			{
				_PlacementSettings.TransactionContainer.BeginTransaction();
				for (int i = 0; i < _IslandList.Count; i++)
				{
					if (_IslandList[i].ElementSet != null && _IslandList[i].ElementSet.Size > 0)
					{
						_Document.Delete(ElementSetConverter.ToIDList(_IslandList[i].ElementSet));
					}
				}
				_PlacementSettings.TransactionContainer.EndTransaction();
			}
		}
		else
		{
			if (_PlacementSettings.ExistingElementChecked && !RevitUtility.EvaluateLeg(_Document, _PlacementSettings.TransactionContainer, _PlacementSettings.PlantSpacing, ref _CurrentLeg))
			{
				return false;
			}
			_PlacementSettings.IsReverse = _CurrentLeg.IsReverse;
			if (_PlacementSettings.PlacementType == PlacementType.HedgeRowCenter)
			{
				PlaceCenterRowOnLeg(_Document, _PlacementSettings, _CurrentLeg);
			}
			else if (_PlacementSettings.PlacementType == PlacementType.HedgeRowAlternate)
			{
				PlaceAltRowOnLeg(_Document, _PlacementSettings, _CurrentLeg);
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
					PlaceCenterRowOnLeg(_Document, _PlacementSettings, ppOffsetLegList, _CurrentLeg.ElementList);
				}
				else
				{
					PlaceCenterRowOnLeg(_Document, _PlacementSettings, ppOffsetLegList2, _CurrentLeg.ElementList);
				}
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

	public static void PickIslandLegs(Document _Document, ref List<Eps_Leg> _IslandList)
	{
		ElementSet _Elements = null;
		if (!RevitUtility.GetElementsFromProject(_Document, ApplicationUtility.GetString("PickIslandsMessage"), ref _Elements, _bDisplayError: false, PickMethod.PickObjects))
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		foreach (Element item in _Elements)
		{
			if (!(item.Location is LocationCurve locationCurve) || !(locationCurve.Curve != null))
			{
				continue;
			}
			flag2 = true;
			Eps_Leg eps_Leg = null;
			eps_Leg = RevitGeometry.GetLegFromElementList(null, item);
			if (eps_Leg != null)
			{
				if (EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg.LegStartPnt, eps_Leg.LegEndPnt, 1E-05))
				{
					_IslandList.Add(eps_Leg);
				}
				continue;
			}
			Curve curve = locationCurve.Curve;
			if (!curve.IsBound)
			{
				if (curve is Arc)
				{
					Arc arc = (Arc)curve;
					eps_Leg = new Eps_Leg();
					eps_Leg.Arc = new Eps_Arc(arc);
					_IslandList.Add(eps_Leg);
				}
				else
				{
					if (!(curve is Ellipse) && !(curve is NurbSpline) && !(curve is HermiteSpline))
					{
						continue;
					}
					IList<XYZ> list = curve.Tessellate();
					int count = list.Count;
					if (count <= 1)
					{
						continue;
					}
					int num = 0;
					Eps_Point startPnt = new Eps_Point();
					Eps_Point eps_Point = new Eps_Point();
					eps_Leg = null;
					Eps_Leg eps_Leg2 = null;
					for (num = 0; num < count; num += 2)
					{
						eps_Point = new Eps_Point(list[num]);
						if (num > 0)
						{
							eps_Leg2 = new Eps_Leg();
							eps_Leg2.Line = new Eps_Line();
							eps_Leg2.Line.StartPnt = startPnt;
							eps_Leg2.Line.EndPnt = eps_Point;
							if (eps_Leg == null)
							{
								eps_Leg = eps_Leg2;
								eps_Leg.Element = item;
							}
							else
							{
								eps_Leg.Last.Next = eps_Leg2;
								eps_Leg.Last.Element = item;
							}
						}
						startPnt = eps_Point;
					}
					if (num == count)
					{
						eps_Leg2 = new Eps_Leg();
						eps_Leg2.Line = new Eps_Line();
						eps_Leg2.Line.StartPnt = startPnt;
						eps_Leg2.Line.EndPnt = new Eps_Point(list[count - 1]);
						if (eps_Leg == null)
						{
							eps_Leg = eps_Leg2;
							eps_Leg.Element = item;
						}
						else
						{
							eps_Leg.Last.Next = eps_Leg2;
							eps_Leg.Last.Element = item;
						}
					}
					_IslandList.Add(eps_Leg);
				}
			}
			else if (!flag && curve is Ellipse)
			{
				RevitMessageBox.Show(ApplicationUtility.GetString("IslandEclipsePickMessage"), ApplicationUtility.GetString("Error"), MessageBoxIcon.Hand);
				flag = true;
			}
		}
		if (flag2 && (_IslandList == null || _IslandList.Count == 0))
		{
			if (!flag)
			{
				RevitMessageBox.Show(ApplicationUtility.GetString("IslandClosedChainMessage"), ApplicationUtility.GetString("Warning"), MessageBoxIcon.Exclamation);
			}
			PickIslandLegs(_Document, ref _IslandList);
		}
	}

	public static void PlaceHedgeGrove(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _BoundaryLeg, List<Eps_Leg> _IslandList)
	{
		PlaceHedgeGrove(_Document, _PlacementSettings, _BoundaryLeg, _IslandList, null);
	}

	public static void PlaceHedgeGrove(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _BoundaryLeg, List<Eps_Leg> _IslandList, List<FamilyInstance> _ExistingPlantList)
	{
		List<Eps_Leg> _IslandList2 = new List<Eps_Leg>();
		for (int i = 0; i < _IslandList.Count; i++)
		{
			_IslandList2.Add(Eps_Leg.CopyLeg(_IslandList[i]));
		}
		OffsetBoundaries(_PlacementSettings.SetbackSpacing, ref _BoundaryLeg, ref _IslandList2);
		if (_BoundaryLeg != null)
		{
			DrawOrderedGrove(_Document, _PlacementSettings, _BoundaryLeg, _IslandList2, _ExistingPlantList);
		}
	}

	private static void OffsetBoundaries(double dSetbackSpacing, ref Eps_Leg _BoundaryLeg, ref List<Eps_Leg> _IslandList)
	{
		int offsetDir = 1;
		Eps_Leg ppOffsetLegList = new Eps_Leg();
		if (!RevitDefines.GT(dSetbackSpacing, 0.0))
		{
			return;
		}
		int Direction = 0;
		if (_BoundaryLeg != null)
		{
			RevitGeometry.eplGeoE_CalcLegDirection(_BoundaryLeg, ref Direction, 1E-05);
			if (Direction == 1)
			{
				offsetDir = 2;
			}
			RevitGeometry.OffsetLegList(_BoundaryLeg, dSetbackSpacing, offsetDir, ref ppOffsetLegList, TrimClosedObject: true);
			_BoundaryLeg = ppOffsetLegList;
		}
		if (_IslandList != null)
		{
			for (int i = 0; i < _IslandList.Count; i++)
			{
				RevitGeometry.eplGeoE_CalcLegDirection(_IslandList[i], ref Direction, 1E-05);
				RevitGeometry.OffsetLegList(OffsetDir: (Direction == 1) ? 1 : 2, pLegListToOffset: _IslandList[i], OffsetDist: dSetbackSpacing, ppOffsetLegList: ref ppOffsetLegList, TrimClosedObject: true);
				_IslandList[i] = ppOffsetLegList;
			}
		}
	}

	private static void DrawOrderedGrove(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _BoundaryLeg, List<Eps_Leg> _IslandList, List<FamilyInstance> _ExistingComponentList)
	{
		if (_BoundaryLeg == null)
		{
			return;
		}
		List<Eps_Point> _PointList = new List<Eps_Point>();
		Eps_Point pMinPnt = new Eps_Point();
		Eps_Point pMaxPnt = new Eps_Point();
		RevitGeometry.eplGeoE_FindMinMaxOfLegList(_BoundaryLeg, ref pMinPnt, ref pMaxPnt);
		bool flag = !_PlacementSettings.DeleteExistingChecked;
		bool flag2 = false;
		bool flag3 = false;
		int num = 0;
		bool bSpacingType = false;
		if (_PlacementSettings.PlacementType == PlacementType.GroveTrianglular)
		{
			bSpacingType = true;
		}
		RevitGeometry.CreatePointListFromMinMaxOfArea(ref _PointList, pMinPnt, pMaxPnt, _PlacementSettings.RotationAngle, _PlacementSettings.PlantSpacing, bSpacingType);
		RevitGeometry.GetAllPointsInBoundary(ref _PointList, _BoundaryLeg, _IslandList);
		FamilyInstance familyInstance = null;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = _PointList.Count;
		progressBarForm.Message = ApplicationUtility.GetString("ProgressBarPlacingComponentsMessage");
		bool bUseFailureHandling = true;
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		if (_PlacementSettings.GenericModelIDList == null)
		{
			flag3 = true;
			_PlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			list = _PlacementSettings.GenericModelIDList;
		}
		for (int i = 0; i < _PointList.Count; i++)
		{
			if (_BoundaryLeg.Element != null && flag && !flag2)
			{
				if (flag3)
				{
					flag3 = false;
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
					{
						Guid chainID = Guid.NewGuid();
						list = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _BoundaryLeg.ElementList, string.Empty, chainID);
						if (list.Count > 0)
						{
							for (int j = 0; j < _IslandList.Count; j++)
							{
								RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _IslandList[j].ElementList, list[0].Value, chainID);
							}
						}
					}
					else
					{
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(ApplicationUtility.GetString("WarningDialog"), _Document.PathName, "LocateComponentsNoPositioning", ApplicationUtility.GetString("PositionComponetsMessage") + ApplicationUtility.GetString("_PlacedWontUpdateWithHost_Message") + ApplicationUtility.GetString("PlacementIDFamilyParameter") + ApplicationUtility.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag = false;
					}
				}
				if (num < list.Count)
				{
					if (string.Compare(list[num].Key, _BoundaryLeg.Element.UniqueId, ignoreCase: true) == 0)
					{
						flag2 = true;
					}
					else
					{
						for (int k = 0; k < list.Count; k++)
						{
							if (string.Compare(list[k].Key, _BoundaryLeg.Element.UniqueId, ignoreCase: true) == 0)
							{
								num = k;
								flag2 = true;
								break;
							}
						}
					}
				}
			}
			Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_PointList[i], _PlacementSettings.SurfaceList);
			if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingComponentList != null && _ExistingComponentList.Count > 0)
			{
				familyInstance = _ExistingComponentList[0];
				if (familyInstance.Location is LocationPoint locationPoint && !EPShared.eplGeoE_ArePntsDuplicates3D(_PointList[i], new Eps_Point(locationPoint.Point)))
				{
					XYZ translation = new XYZ(_PointList[i].XYZ.X - locationPoint.Point.X, _PointList[i].XYZ.Y - locationPoint.Point.Y, 0.0);
					ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
				}
				_ExistingComponentList.RemoveAt(0);
			}
			else
			{
				EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
				familyInstance = RevitUtility.NewFamilyInstance(_Document, _PointList[i].XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
				_PlacementSettings.TransactionContainer = _TransactionContainer;
			}
			if (_BoundaryLeg.Element != null && flag && flag2)
			{
				Parameter parameter = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
				if (parameter != null)
				{
					parameter.Set(list[num].Value);
				}
				else
				{
					flag = false;
				}
			}
			progressBarForm.UpdateProgressBar(1);
			flag2 = false;
		}
		if (_ExistingComponentList != null && _ExistingComponentList.Count > 0)
		{
			for (int l = 0; l < _ExistingComponentList.Count; l++)
			{
				_Document.Delete(_ExistingComponentList[l].Id);
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

	public static void PlaceCenterRowOnLine(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Line _Line)
	{
		PlaceCenterRowOnLine(_Document, _PlacementSettings, _Element, _Line, null);
	}

	public static void PlaceCenterRowOnLine(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Line _Line, List<FamilyInstance> _ExistingComponentList)
	{
		Eps_Point _NewPnt = new Eps_Point();
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		double num = 0.0;
		double plantSpacing = _PlacementSettings.PlantSpacing;
		if (_Line == null)
		{
			return;
		}
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDirection);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDistance);
		List<Eps_Point> list = new List<Eps_Point>();
		bool flag = !_PlacementSettings.DeleteExistingChecked;
		bool flag2 = false;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(_dDistance - _PlacementSettings.StartingDistance);
		progressBarForm.Message = ApplicationUtility.GetString("ProgressBarPlacingComponentsMessage");
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
		double num2 = 0.0;
		for (num = _PlacementSettings.StartingDistance; num <= _dDistance; num += plantSpacing)
		{
			EPShared.eplGeoE_CalcPolarToXY(_Line.StartPnt, _dDirection, num, ref _NewPnt);
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
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(ApplicationUtility.GetString("WarningDialog"), _Document.PathName, "LocateComponentsNoPositioning", ApplicationUtility.GetString("PositionComponetsMessage") + ApplicationUtility.GetString("_PlacedWontUpdateWithHost_Message") + ApplicationUtility.GetString("PlacementIDFamilyParameter") + ApplicationUtility.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag = false;
					}
				}
				list.Add(_NewPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlacementSettings.SurfaceList);
				FamilyInstance familyInstance;
				if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingComponentList != null && _ExistingComponentList.Count > 0)
				{
					familyInstance = _ExistingComponentList[0];
					if (familyInstance.Location is LocationPoint locationPoint)
					{
						if (!EPShared.eplGeoE_ArePntsDuplicates3D(_NewPnt, new Eps_Point(locationPoint.Point)))
						{
							XYZ translation = new XYZ(_NewPnt.XYZ.X - locationPoint.Point.X, _NewPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
							ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
						}
						if (_PlacementSettings.AlignTo)
						{
							num2 = locationPoint.Rotation;
						}
					}
					_ExistingComponentList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
					familyInstance = RevitUtility.NewFamilyInstance(_Document, _NewPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
					_PlacementSettings.TransactionContainer = _TransactionContainer;
				}
				if (_PlacementSettings.AlignTo && !RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dDirection), EPShared.eplGeoE_NormalizeRadians(num2)))
				{
					XYZ endpoint = new XYZ(_NewPnt.x, _NewPnt.y, _NewPnt.z + 1.0);
					Line axis = Line.CreateBound(_NewPnt.XYZ, endpoint);
					ElementTransformUtils.RotateElement(_Document, familyInstance.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dDirection - num2));
				}
				if (_Element != null && flag && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter != null)
					{
						parameter.Set(keyValuePair.Value);
					}
					else
					{
						flag = false;
					}
				}
			}
			progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
			num2 = 0.0;
		}
		if (_ExistingComponentList != null && _ExistingComponentList.Count > 0)
		{
			for (int i = 0; i < _ExistingComponentList.Count; i++)
			{
				_Document.Delete(_ExistingComponentList[i].Id);
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

	public static void PlaceAltRowOnLine(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Line _Line)
	{
		bool _bOffsetRight = true;
		PlaceAltRowOnLine(_Document, _PlacementSettings, _Element, _Line, ref _bOffsetRight, null);
	}

	public static void PlaceAltRowOnLine(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Line _Line, List<FamilyInstance> _ExistingPlantList)
	{
		bool _bOffsetRight = true;
		PlaceAltRowOnLine(_Document, _PlacementSettings, _Element, _Line, ref _bOffsetRight, _ExistingPlantList);
	}

	public static void PlaceAltRowOnLine(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Line _Line, ref bool _bOffsetRight, List<FamilyInstance> _ExistingComponentList)
	{
		Eps_Point _NewPnt = new Eps_Point();
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		double num = 0.0;
		double setbackSpacing = _PlacementSettings.SetbackSpacing;
		double plantSpacing = _PlacementSettings.PlantSpacing;
		if (_Line == null)
		{
			return;
		}
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDirection);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDistance);
		List<Eps_Point> list = new List<Eps_Point>();
		bool flag = !_PlacementSettings.DeleteExistingChecked;
		bool flag2 = false;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(_dDistance - _PlacementSettings.StartingDistance);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingComponentsMessage");
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
		double num2 = 0.0;
		for (num = _PlacementSettings.StartingDistance; num <= _dDistance; num += plantSpacing / 2.0)
		{
			EPShared.eplGeoE_CalcPolarToXY(_Line.StartPnt, _dDirection, num, ref _NewPnt);
			if (_bOffsetRight)
			{
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection - Math.PI / 2.0), setbackSpacing, ref _NewPnt);
			}
			else
			{
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI / 2.0), setbackSpacing, ref _NewPnt);
			}
			_bOffsetRight = !_bOffsetRight;
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
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateComponentsNoPositioning", EPShared.LocalRM.GetString("PositionComponetsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag = false;
					}
				}
				list.Add(_NewPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlacementSettings.SurfaceList);
				FamilyInstance familyInstance;
				if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingComponentList != null && _ExistingComponentList.Count > 0)
				{
					familyInstance = _ExistingComponentList[0];
					if (familyInstance.Location is LocationPoint locationPoint)
					{
						if (!EPShared.eplGeoE_ArePntsDuplicates3D(_NewPnt, new Eps_Point(locationPoint.Point)))
						{
							XYZ translation = new XYZ(_NewPnt.XYZ.X - locationPoint.Point.X, _NewPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
							ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
						}
						if (_PlacementSettings.AlignTo)
						{
							num2 = locationPoint.Rotation;
						}
					}
					_ExistingComponentList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
					familyInstance = RevitUtility.NewFamilyInstance(_Document, _NewPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
					_PlacementSettings.TransactionContainer = _TransactionContainer;
				}
				if (_PlacementSettings.AlignTo && !RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dDirection), EPShared.eplGeoE_NormalizeRadians(num2)))
				{
					XYZ endpoint = new XYZ(_NewPnt.x, _NewPnt.y, _NewPnt.z + 1.0);
					Line axis = Line.CreateBound(_NewPnt.XYZ, endpoint);
					ElementTransformUtils.RotateElement(_Document, familyInstance.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dDirection - num2));
				}
				if (_Element != null && flag && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter != null)
					{
						parameter.Set(keyValuePair.Value);
					}
					else
					{
						flag = false;
					}
				}
			}
			progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
			num2 = 0.0;
		}
		if (_ExistingComponentList != null && _ExistingComponentList.Count > 0)
		{
			for (int i = 0; i < _ExistingComponentList.Count; i++)
			{
				_Document.Delete(_ExistingComponentList[i].Id);
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

	public static void PlaceCenterRowOnArc(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Arc _Arc, List<FamilyInstance> _ExistingComponentList)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		double num = 0.0;
		double plantSpacing = _PlacementSettings.PlantSpacing;
		double num2 = 0.0;
		if (_Arc == null)
		{
			return;
		}
		num2 = _Arc.Radius * _Arc.Delta;
		List<Eps_Point> list = new List<Eps_Point>();
		bool flag = !_PlacementSettings.DeleteExistingChecked;
		bool flag2 = false;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(num2 - _PlacementSettings.StartingDistance);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingComponentsMessage");
		bool bUseFailureHandling = true;
		KeyValuePair<string, string> keyValuePair = default;
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
		double num3 = 0.0;
		for (num = _PlacementSettings.StartingDistance; num <= num2; num += plantSpacing)
		{
			EPShared.eplGeoE_FindStaOffPointAlongArc(_Arc, num, 0.0, ref _StaOffPnt);
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
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateComponentsNoPositioning", EPShared.LocalRM.GetString("PositionComponetsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag = false;
					}
				}
				list.Add(_StaOffPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList);
				FamilyInstance familyInstance;
				if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingComponentList != null && _ExistingComponentList.Count > 0)
				{
					familyInstance = _ExistingComponentList[0];
					if (familyInstance.Location is LocationPoint locationPoint)
					{
						if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint.Point)))
						{
							XYZ translation = new XYZ(_StaOffPnt.XYZ.X - locationPoint.Point.X, _StaOffPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
							ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
						}
						if (_PlacementSettings.AlignTo)
						{
							num3 = locationPoint.Rotation;
						}
					}
					_ExistingComponentList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
					familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
					_PlacementSettings.TransactionContainer = _TransactionContainer;
				}
				if (_PlacementSettings.AlignTo)
				{
					double _dTangentDir = 0.0;
					EPShared.eplGeoE_CalcTangentDirOfArc(_StaOffPnt, _Arc.RadiusPnt, _Arc.Direction, ref _dTangentDir);
					if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir), EPShared.eplGeoE_NormalizeRadians(num3)))
					{
						XYZ endpoint = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
						Line axis = Line.CreateBound(_StaOffPnt.XYZ, endpoint);
						ElementTransformUtils.RotateElement(_Document, familyInstance.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - num3));
					}
				}
				if (_Element != null && flag && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter != null)
					{
						parameter.Set(keyValuePair.Value);
					}
					else
					{
						flag = false;
					}
				}
			}
			progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
			num3 = 0.0;
		}
		if (_ExistingComponentList != null && _ExistingComponentList.Count > 0)
		{
			for (int i = 0; i < _ExistingComponentList.Count; i++)
			{
				_Document.Delete(_ExistingComponentList[i].Id);
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

	public static void PlaceAltRowOnArc(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Arc _Arc)
	{
		PlaceAltRowOnArc(_Document, _PlacementSettings, _Element, _Arc, null);
	}

	public static void PlaceAltRowOnArc(Document _Document, PlacementSettings _PlacementSettings, Element _Element, Eps_Arc _Arc, List<FamilyInstance> _ExistingComponentList)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		double num = 0.0;
		double _dTangentDir = 0.0;
		bool flag = true;
		double setbackSpacing = _PlacementSettings.SetbackSpacing;
		double plantSpacing = _PlacementSettings.PlantSpacing;
		double num2 = 0.0;
		if (_Arc == null)
		{
			return;
		}
		num2 = _Arc.Radius * _Arc.Delta;
		List<Eps_Point> list = new List<Eps_Point>();
		bool flag2 = !_PlacementSettings.DeleteExistingChecked;
		bool flag3 = false;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(num2 - _PlacementSettings.StartingDistance);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingComponentsMessage");
		KeyValuePair<string, string> keyValuePair = default;
		bool bUseFailureHandling = true;
		if (_PlacementSettings.GenericModelIDList == null)
		{
			flag3 = true;
			_PlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			keyValuePair = _PlacementSettings.GenericModelIDList[0];
		}
		double num3 = 0.0;
		for (num = _PlacementSettings.StartingDistance; num <= num2; num += plantSpacing / 2.0)
		{
			EPShared.eplGeoE_FindStaOffPointAlongArc(_Arc, num, 0.0, ref _StaOffPnt);
			EPShared.eplGeoE_CalcTangentDirOfArc(_StaOffPnt, _Arc.RadiusPnt, _Arc.Direction, ref _dTangentDir);
			if (flag)
			{
				EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - Math.PI / 2.0), setbackSpacing, ref _StaOffPnt);
			}
			else
			{
				EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI / 2.0), setbackSpacing, ref _StaOffPnt);
			}
			flag = !flag;
			if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
			{
				if (_Element != null && flag2 && flag3)
				{
					flag3 = false;
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
					{
						keyValuePair = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Element);
					}
					else
					{
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateComponentsNoPositioning", EPShared.LocalRM.GetString("PositionComponetsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag2 = false;
					}
				}
				list.Add(_StaOffPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList);
				FamilyInstance familyInstance;
				if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingComponentList != null && _ExistingComponentList.Count > 0)
				{
					familyInstance = _ExistingComponentList[0];
					if (familyInstance.Location is LocationPoint locationPoint)
					{
						if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint.Point)))
						{
							XYZ translation = new XYZ(_StaOffPnt.XYZ.X - locationPoint.Point.X, _StaOffPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
							ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
						}
						if (_PlacementSettings.AlignTo)
						{
							num3 = locationPoint.Rotation;
						}
					}
					_ExistingComponentList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
					familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
					_PlacementSettings.TransactionContainer = _TransactionContainer;
				}
				if (_PlacementSettings.AlignTo && !RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir), EPShared.eplGeoE_NormalizeRadians(num3)))
				{
					XYZ endpoint = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
					Line axis = Line.CreateBound(_StaOffPnt.XYZ, endpoint);
					ElementTransformUtils.RotateElement(_Document, familyInstance.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - num3));
				}
				if (_Element != null && flag2 && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter != null)
					{
						parameter.Set(keyValuePair.Value);
					}
					else
					{
						flag2 = false;
					}
				}
			}
			progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
			num3 = 0.0;
		}
		if (_ExistingComponentList != null && _ExistingComponentList.Count > 0)
		{
			for (int i = 0; i < _ExistingComponentList.Count; i++)
			{
				_Document.Delete(_ExistingComponentList[i].Id);
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

	public static void PlaceCenterRowOnLeg(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _Leg, List<Element> _OrigLegElementSet)
	{
		PlaceCenterRowOnLeg(_Document, _PlacementSettings, _Leg, _OrigLegElementSet, null);
	}

	public static void PlaceCenterRowOnLeg(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _Leg, List<FamilyInstance> _MovePlantList)
	{
		PlaceCenterRowOnLeg(_Document, _PlacementSettings, _Leg, _Leg.ElementList, _MovePlantList);
	}

	public static void PlaceCenterRowOnLeg(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _Leg, List<Element> _OrigLegElementSet, List<FamilyInstance> _ExistingComponentList)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		Eps_Leg eps_Leg = null;
		double _dDistance = 0.0;
		double _dTangentDir = 0.0;
		double num = 0.0;
		double num2 = _PlacementSettings.StartingDistance;
		double plantSpacing = _PlacementSettings.PlantSpacing;
		if (_Leg == null)
		{
			return;
		}
		List<Eps_Point> list = new List<Eps_Point>();
		bool flag = !_PlacementSettings.DeleteExistingChecked;
		bool flag2 = false;
		bool flag3 = false;
		int num3 = 0;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(RevitGeometry.eplGeoE_CalcLegListLength(_Leg) - num2);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingComponentsMessage");
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
		double num4 = 0.0;
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
					for (num = num2; num <= _dDistance; num += plantSpacing)
					{
						EPShared.eplGeoE_CalcPolarToXY(eps_Leg.Line.StartPnt, _dTangentDir, num, ref _StaOffPnt);
						if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
						{
							if (eps_Leg.Element != null && flag && !flag2)
							{
								if (flag3)
								{
									flag3 = false;
									if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
									{
										list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _OrigLegElementSet);
									}
									else
									{
										WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateComponentsNoPositioning", EPShared.LocalRM.GetString("PositionComponetsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
										warningOptionsForm.ShowDialog();
										flag = false;
									}
								}
								if (num3 < list2.Count)
								{
									if (string.Compare(list2[num3].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag2 = true;
									}
									else
									{
										for (int i = 0; i < list2.Count; i++)
										{
											if (string.Compare(list2[i].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num3 = i;
												flag2 = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList);
							FamilyInstance familyInstance;
							if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingComponentList != null && _ExistingComponentList.Count > 0)
							{
								familyInstance = _ExistingComponentList[0];
								if (familyInstance.Location is LocationPoint locationPoint)
								{
									if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint.Point)))
									{
										XYZ translation = new XYZ(_StaOffPnt.XYZ.X - locationPoint.Point.X, _StaOffPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
										ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
									}
									if (_PlacementSettings.AlignTo)
									{
										num4 = locationPoint.Rotation;
									}
								}
								_ExistingComponentList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
								familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
								_PlacementSettings.TransactionContainer = _TransactionContainer;
							}
							if (_PlacementSettings.AlignTo && !RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir), EPShared.eplGeoE_NormalizeRadians(num4)))
							{
								XYZ endpoint = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
								Line axis = Line.CreateBound(_StaOffPnt.XYZ, endpoint);
								ElementTransformUtils.RotateElement(_Document, familyInstance.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - num4));
							}
							if (eps_Leg.Element != null && flag && flag2)
							{
								Parameter parameter = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter != null)
								{
									parameter.Set(list2[num3].Value);
								}
								else
								{
									flag = false;
								}
							}
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
						num4 = 0.0;
					}
					num2 = num - _dDistance;
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
					for (num = num2; num <= _dDistance; num += plantSpacing)
					{
						EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num, 0.0, ref _StaOffPnt);
						if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
						{
							if (eps_Leg.Element != null && flag && !flag2)
							{
								if (flag3)
								{
									flag3 = false;
									if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
									{
										list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _OrigLegElementSet);
									}
									else
									{
										WarningOptionsForm warningOptionsForm2 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateComponentsNoPositioning", EPShared.LocalRM.GetString("PositionComponetsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
										warningOptionsForm2.ShowDialog();
										flag = false;
									}
								}
								if (num3 < list2.Count)
								{
									if (string.Compare(list2[num3].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag2 = true;
									}
									else
									{
										for (int j = 0; j < list2.Count; j++)
										{
											if (string.Compare(list2[j].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num3 = j;
												flag2 = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint2 = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList);
							FamilyInstance familyInstance;
							if ((surfaceContainingPoint2 == null || _PlacementSettings.GenericModelIDList != null) && _ExistingComponentList != null && _ExistingComponentList.Count > 0)
							{
								familyInstance = _ExistingComponentList[0];
								if (familyInstance.Location is LocationPoint locationPoint2)
								{
									if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint2.Point)))
									{
										XYZ translation2 = new XYZ(_StaOffPnt.XYZ.X - locationPoint2.Point.X, _StaOffPnt.XYZ.Y - locationPoint2.Point.Y, 0.0);
										ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation2);
									}
									if (_PlacementSettings.AlignTo)
									{
										num4 = locationPoint2.Rotation;
									}
								}
								_ExistingComponentList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer2 = _PlacementSettings.TransactionContainer;
								familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint2, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer2);
								_PlacementSettings.TransactionContainer = _TransactionContainer2;
							}
							if (_PlacementSettings.AlignTo)
							{
								EPShared.eplGeoE_CalcTangentDirOfArc(_StaOffPnt, eps_Leg.Arc.RadiusPnt, eps_Leg.Arc.Direction, ref _dTangentDir);
								if (!RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir), EPShared.eplGeoE_NormalizeRadians(num4)))
								{
									XYZ endpoint2 = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
									Line axis2 = Line.CreateBound(_StaOffPnt.XYZ, endpoint2);
									ElementTransformUtils.RotateElement(_Document, familyInstance.Id, axis2, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - num4));
								}
							}
							if (eps_Leg.Element != null && flag && flag2)
							{
								Parameter parameter2 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter2 != null)
								{
									parameter2.Set(list2[num3].Value);
								}
								else
								{
									flag = false;
								}
							}
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
						num4 = 0.0;
					}
					num2 = num - _dDistance;
				}
			}
			flag2 = false;
		}
		if (_ExistingComponentList != null && _ExistingComponentList.Count > 0)
		{
			for (int k = 0; k < _ExistingComponentList.Count; k++)
			{
				_Document.Delete(_ExistingComponentList[k].Id);
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

	public static void PlaceAltRowOnLeg(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _Leg)
	{
		PlaceAltRowOnLeg(_Document, _PlacementSettings, _Leg, null);
	}

	public static void PlaceAltRowOnLeg(Document _Document, PlacementSettings _PlacementSettings, Eps_Leg _Leg, List<FamilyInstance> _ExistingComponentList)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		Eps_Leg eps_Leg = null;
		double _dDistance = 0.0;
		double _dTangentDir = 0.0;
		double num = 0.0;
		double num2 = _PlacementSettings.StartingDistance;
		double setbackSpacing = _PlacementSettings.SetbackSpacing;
		double plantSpacing = _PlacementSettings.PlantSpacing;
		bool flag = true;
		if (_Leg == null)
		{
			return;
		}
		List<Eps_Point> list = new List<Eps_Point>();
		bool flag2 = !_PlacementSettings.DeleteExistingChecked;
		bool flag3 = false;
		bool flag4 = false;
		int num3 = 0;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(RevitGeometry.eplGeoE_CalcLegListLength(_Leg) - num2);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingComponentsMessage");
		List<KeyValuePair<string, string>> list2 = new List<KeyValuePair<string, string>>();
		bool bUseFailureHandling = true;
		if (_PlacementSettings.GenericModelIDList == null)
		{
			flag4 = true;
			_PlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			list2 = _PlacementSettings.GenericModelIDList;
		}
		double num4 = 0.0;
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
					for (num = num2; num <= _dDistance; num += plantSpacing / 2.0)
					{
						EPShared.eplGeoE_CalcPolarToXY(eps_Leg.Line.StartPnt, _dTangentDir, num, ref _StaOffPnt);
						if (flag)
						{
							EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - Math.PI / 2.0), setbackSpacing, ref _StaOffPnt);
						}
						else
						{
							EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI / 2.0), setbackSpacing, ref _StaOffPnt);
						}
						flag = !flag;
						if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
						{
							if (eps_Leg.Element != null && flag2 && !flag3)
							{
								if (flag4)
								{
									flag4 = false;
									if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
									{
										list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Leg.ElementList);
									}
									else
									{
										WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateComponentsNoPositioning", EPShared.LocalRM.GetString("PositionComponetsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
										warningOptionsForm.ShowDialog();
										flag2 = false;
									}
								}
								if (num3 < list2.Count)
								{
									if (string.Compare(list2[num3].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag3 = true;
									}
									else
									{
										for (int i = 0; i < list2.Count; i++)
										{
											if (string.Compare(list2[i].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num3 = i;
												flag3 = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList);
							FamilyInstance familyInstance;
							if ((surfaceContainingPoint == null || _PlacementSettings.GenericModelIDList != null) && _ExistingComponentList != null && _ExistingComponentList.Count > 0)
							{
								familyInstance = _ExistingComponentList[0];
								if (familyInstance.Location is LocationPoint locationPoint)
								{
									if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint.Point)))
									{
										XYZ translation = new XYZ(_StaOffPnt.XYZ.X - locationPoint.Point.X, _StaOffPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
										ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
									}
									if (_PlacementSettings.AlignTo)
									{
										num4 = locationPoint.Rotation;
									}
								}
								_ExistingComponentList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
								familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
								_PlacementSettings.TransactionContainer = _TransactionContainer;
							}
							if (_PlacementSettings.AlignTo && !RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir), EPShared.eplGeoE_NormalizeRadians(num4)))
							{
								XYZ endpoint = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
								Line axis = Line.CreateBound(_StaOffPnt.XYZ, endpoint);
								ElementTransformUtils.RotateElement(_Document, familyInstance.Id, axis, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - num4));
							}
							if (eps_Leg.Element != null && flag2 && flag3)
							{
								Parameter parameter = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter != null)
								{
									parameter.Set(list2[num3].Value);
								}
								else
								{
									flag2 = false;
								}
							}
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
						num4 = 0.0;
					}
					num2 = num - _dDistance;
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
					for (num = num2; num <= _dDistance; num += plantSpacing / 2.0)
					{
						EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num, 0.0, ref _StaOffPnt);
						EPShared.eplGeoE_CalcTangentDirOfArc(_StaOffPnt, eps_Leg.Arc.RadiusPnt, eps_Leg.Arc.Direction, ref _dTangentDir);
						if (flag)
						{
							EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - Math.PI / 2.0), setbackSpacing, ref _StaOffPnt);
						}
						else
						{
							EPShared.eplGeoE_CalcPolarToXY(_StaOffPnt, EPShared.eplGeoE_NormalizeRadians(_dTangentDir + Math.PI / 2.0), setbackSpacing, ref _StaOffPnt);
						}
						flag = !flag;
						if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
						{
							if (eps_Leg.Element != null && flag2 && !flag3)
							{
								if (flag4)
								{
									flag4 = false;
									if (RevitUtility.IsParameterIDOnFamily(_Document, _PlacementSettings, _PlacementSettings.Type))
									{
										list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlacementSettings, _Leg.ElementList);
									}
									else
									{
										WarningOptionsForm warningOptionsForm2 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "LocateComponentsNoPositioning", EPShared.LocalRM.GetString("PositionComponetsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
										warningOptionsForm2.ShowDialog();
										flag2 = false;
									}
								}
								if (num3 < list2.Count)
								{
									if (string.Compare(list2[num3].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag3 = true;
									}
									else
									{
										for (int j = 0; j < list2.Count; j++)
										{
											if (string.Compare(list2[j].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num3 = j;
												flag3 = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint2 = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlacementSettings.SurfaceList);
							FamilyInstance familyInstance;
							if ((surfaceContainingPoint2 == null || _PlacementSettings.GenericModelIDList != null) && _ExistingComponentList != null && _ExistingComponentList.Count > 0)
							{
								familyInstance = _ExistingComponentList[0];
								if (familyInstance.Location is LocationPoint locationPoint2)
								{
									if (!EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint2.Point)))
									{
										XYZ translation2 = new XYZ(_StaOffPnt.XYZ.X - locationPoint2.Point.X, _StaOffPnt.XYZ.Y - locationPoint2.Point.Y, 0.0);
										ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation2);
									}
									if (_PlacementSettings.AlignTo)
									{
										num4 = locationPoint2.Rotation;
									}
								}
								_ExistingComponentList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer2 = _PlacementSettings.TransactionContainer;
								familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlacementSettings.Type, surfaceContainingPoint2, _PlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer2);
								_PlacementSettings.TransactionContainer = _TransactionContainer2;
							}
							if (_PlacementSettings.AlignTo && !RevitDefines.EQ(EPShared.eplGeoE_NormalizeRadians(_dTangentDir), EPShared.eplGeoE_NormalizeRadians(num4)))
							{
								XYZ endpoint2 = new XYZ(_StaOffPnt.x, _StaOffPnt.y, _StaOffPnt.z + 1.0);
								Line axis2 = Line.CreateBound(_StaOffPnt.XYZ, endpoint2);
								ElementTransformUtils.RotateElement(_Document, familyInstance.Id, axis2, EPShared.eplGeoE_NormalizeRadians(_dTangentDir - num4));
							}
							if (eps_Leg.Element != null && flag2 && flag3)
							{
								Parameter parameter2 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter2 != null)
								{
									parameter2.Set(list2[num3].Value);
								}
								else
								{
									flag2 = false;
								}
							}
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
						num4 = 0.0;
					}
					num2 = num - _dDistance;
				}
			}
			flag3 = false;
		}
		if (_ExistingComponentList != null && _ExistingComponentList.Count > 0)
		{
			for (int k = 0; k < _ExistingComponentList.Count; k++)
			{
				_Document.Delete(_ExistingComponentList[k].Id);
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
