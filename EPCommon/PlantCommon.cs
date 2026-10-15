using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using SITES.Utilities;

namespace EPCommon;

public class PlantCommon
{
	public static bool PickPlantToTag(Document _Document, ElementSet _FamilyList, ref Eps_Point _PickPoint, ref FamilyInstance _FoundFamily)
	{
		if (_FamilyList.Size == 1)
		{
			{
				IEnumerator enumerator = _FamilyList.GetEnumerator();
				try
				{
					if (enumerator.MoveNext())
					{
						Element element = (Element)enumerator.Current;
						_FoundFamily = element as FamilyInstance;
						return true;
					}
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
		}
		ElementSet _Elements = null;
		if (!RevitUtility.GetElementsFromProject(_Document, EPShared.LocalRM.GetString("PickPlantTagMessage"), ref _Elements, _bDisplayError: true))
		{
			return false;
		}
		foreach (Element item in _Elements)
		{
			if (!(item is FamilyInstance) || !(item.Location is LocationPoint locationPoint))
			{
				continue;
			}
			foreach (Element _Family in _FamilyList)
			{
				if (_Family is FamilyInstance { Location: LocationPoint location } familyInstance2 && EPShared.eplGeoE_ArePntsDuplicates2D(new Eps_Point(locationPoint.Point), new Eps_Point(location.Point), 1E-05))
				{
					_PickPoint = new Eps_Point(locationPoint.Point);
					_FoundFamily = familyInstance2;
					return true;
				}
			}
		}
		RevitMessageBox.Show(EPShared.LocalRM.GetString("ObjectPickedPlantMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
		_Document.Regenerate();
		return PickPlantToTag(_Document, _FamilyList, ref _PickPoint, ref _FoundFamily);
	}

	public static void CreateTag(Document _Document, EPTransactionContainer _TransactionContainer, Autodesk.Revit.DB.View _View, ElementSet _FamilyList, double _dTagSpacing, bool _bPlaceGroupTagChecked, bool _bLeaderedTagChecked, bool _bCreateTag)
	{
		if (!_bPlaceGroupTagChecked || _FamilyList.IsEmpty)
		{
			return;
		}
		if (_bCreateTag)
		{
			FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
			FilteredElementIterator elementIterator = filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_PlantingTags).GetElementIterator();
			bool flag = false;
			FamilySymbol familySymbol = null;
			elementIterator.Reset();
			while (elementIterator.MoveNext())
			{
				if (elementIterator.Current is FamilySymbol { Category: not null } familySymbol2 && familySymbol2.Category.Id.Value64() == BuiltInCategory.OST_PlantingTags.GetHashCode())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				using OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Title = EPShared.LocalRM.GetString("NoTag");
				openFileDialog.RestoreDirectory = true;
				openFileDialog.Filter = ApplicationUtility.GetString("FamilyFilePickerFilter");
				Family _Family = null;
				bool flag2 = false;
				DialogResult dialogResult = DialogResult.OK;
				while (dialogResult != DialogResult.Cancel)
				{
					dialogResult = openFileDialog.ShowDialog();
					if (dialogResult == DialogResult.Cancel)
					{
						continue;
					}
					try
					{
						flag2 = RevitUtility.LoadFamily(_Document, openFileDialog.FileName, out _Family, _bUseFailureHandling: true, ref _TransactionContainer);
						if (_Family == null || _Family.Category == null || _Family.Category.Id.Value64() != BuiltInCategory.OST_PlantingTags.GetHashCode())
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("FamilyNotPlantingTagMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
						}
					}
					catch (SEHException ex)
					{
						flag2 = false;
						if (string.Compare(ex.Message, EPShared.LocalRM.GetString("ExternalComponentExceptionMessage"), ignoreCase: true) == 0)
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("LoadingFamilyProblemLoadNewMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						}
						continue;
					}
					break;
				}
				if (!flag2)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("PlantPlaceTagNotPlacedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return;
				}
			}
			_Document.Regenerate();
			FamilyInstance _FoundFamily = null;
			Eps_Point _PickPoint = new Eps_Point();
			if (!PickPlantToTag(_Document, _FamilyList, ref _PickPoint, ref _FoundFamily))
			{
				return;
			}
			_TransactionContainer.BeginTransaction();
			if (_FoundFamily != null)
			{
				_FoundFamily.LookupParameter(RevitParameters.PlantCountPlantFamilyParameter)?.Set(_FamilyList.Size);
				Eps_Point eps_Point = null;
				Eps_Point _NewPnt = null;
				Eps_Point _NewPnt2 = null;
				Eps_Line eps_Line = null;
				Eps_Point _Point = new Eps_Point();
				ElementSet elementSet = new ElementSet();
				ModelCurve modelCurve = null;
				string empty = string.Empty;
				empty = ((!_bLeaderedTagChecked) ? EPShared.LocalRM.GetString("PickTagPointPrompt") : EPShared.LocalRM.GetString("PickLeaderStartPointPrompt"));
				bool flag3 = true;
				while (flag3)
				{
					flag3 = RevitUtility.GetPointFromProject(_Document, empty, ref _Point, _bDisplayError: false);
					if (flag3)
					{
						if (_bLeaderedTagChecked && eps_Point == null)
						{
							eps_Point = _Point;
							empty = EPShared.LocalRM.GetString("PickLeaderElbowPointPrompt");
						}
						else if (_bLeaderedTagChecked && _NewPnt == null)
						{
							_NewPnt = _Point;
							eps_Line = new Eps_Line();
							eps_Line.StartPnt = eps_Point;
							eps_Line.EndPnt = _NewPnt;
							modelCurve = RevitUtility.NewModelCurve(_Document, eps_Line);
							empty = EPShared.LocalRM.GetString("PickTagPointPrompt");
						}
						else if (_NewPnt2 == null)
						{
							_NewPnt2 = _Point;
							flag3 = false;
						}
					}
				}
				if (modelCurve != null)
				{
					_Document.Delete(modelCurve.Id);
				}
				if ((_bLeaderedTagChecked && eps_Point == null) || (!_bLeaderedTagChecked && _NewPnt2 == null))
				{
					eps_Point = _PickPoint;
					_NewPnt = new Eps_Point();
					_NewPnt2 = new Eps_Point();
					EPShared.eplGeoE_CalcPolarToXY(eps_Point, Math.PI * 3.0 / 4.0, _dTagSpacing, ref _NewPnt);
					EPShared.eplGeoE_CalcPolarToXY(_NewPnt, Math.PI, _dTagSpacing, ref _NewPnt2);
				}
				IndependentTag independentTag = null;
				if (_bLeaderedTagChecked)
				{
					if (_NewPnt == null)
					{
						_NewPnt = eps_Point;
						_NewPnt2 = eps_Point;
					}
					else if (_NewPnt2 == null)
					{
						_NewPnt2 = _NewPnt;
					}
					independentTag = IndependentTag.Create(_Document, _View.Id, new Reference(_FoundFamily), addLeader: true, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, _PickPoint.XYZ);
					independentTag.LeaderEndCondition = LeaderEndCondition.Free;
					independentTag.SetLeaderEnd(new Reference(_FoundFamily), eps_Point.XYZ);
					independentTag.SetLeaderElbow(new Reference(_FoundFamily), _NewPnt.XYZ);
					independentTag.TagHeadPosition = _NewPnt2.XYZ;
				}
				else
				{
					independentTag = IndependentTag.Create(_Document, _View.Id, new Reference(_FoundFamily), addLeader: false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, _NewPnt2.XYZ);
				}
			}
			_Document.Create.NewGroup(ElementSetConverter.ToIDList(_FamilyList));
			_TransactionContainer.EndTransaction();
		}
		else
		{
			_Document.Create.NewGroup(ElementSetConverter.ToIDList(_FamilyList));
		}
	}

	public static bool PlaceLegList(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _CurrentLeg)
	{
		if (_PlantPlacementSettings.PlacementType == PlacementType.Single)
		{
			PlaceSingleOnLeg(_Document, _PlantPlacementSettings, _CurrentLeg);
		}
		else if (_PlantPlacementSettings.PlacementType == PlacementType.GroveSquare || _PlantPlacementSettings.PlacementType == PlacementType.GroveTrianglular)
		{
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CurrentLeg.LegStartPnt, _CurrentLeg.LegEndPnt, 1E-05))
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("BoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return false;
			}
			List<Eps_Leg> _IslandList = new List<Eps_Leg>();
			PickIslandLegs(_Document, ref _IslandList);
			PlaceHedgeGrove(_Document, _PlantPlacementSettings, _CurrentLeg, _IslandList);
			if (_PlantPlacementSettings.DeleteExistingChecked)
			{
				_PlantPlacementSettings.TransactionContainer.BeginTransaction();
				for (int i = 0; i < _IslandList.Count; i++)
				{
					if (_IslandList[i].ElementSet != null && _IslandList[i].ElementSet.Size > 0)
					{
						_Document.Delete(ElementSetConverter.ToIDList(_IslandList[i].ElementSet));
					}
				}
				_PlantPlacementSettings.TransactionContainer.EndTransaction();
			}
		}
		else if (_PlantPlacementSettings.PlacementType == PlacementType.HedgeRowCenter || _PlantPlacementSettings.PlacementType == PlacementType.HedgeRowAlternate || _PlantPlacementSettings.PlacementType == PlacementType.HedgeRowSide)
		{
			if (_PlantPlacementSettings.ExistingElementChecked && !RevitUtility.EvaluateLeg(_Document, _PlantPlacementSettings.TransactionContainer, _PlantPlacementSettings.PlantSpacing, ref _CurrentLeg))
			{
				return false;
			}
			_PlantPlacementSettings.IsReverse = _CurrentLeg.IsReverse;
			if (_PlantPlacementSettings.PlacementType == PlacementType.HedgeRowCenter)
			{
				PlaceCenterRowOnLeg(_Document, _PlantPlacementSettings, _CurrentLeg);
			}
			else if (_PlantPlacementSettings.PlacementType == PlacementType.HedgeRowAlternate)
			{
				PlaceAltRowOnLeg(_Document, _PlantPlacementSettings, _CurrentLeg);
			}
			else
			{
				Eps_Leg ppOffsetLegList = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentLeg, _PlantPlacementSettings.SetbackSpacing, 1, ref ppOffsetLegList, TrimClosedObject: true);
				Eps_Leg ppOffsetLegList2 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentLeg, _PlantPlacementSettings.SetbackSpacing, 2, ref ppOffsetLegList2, TrimClosedObject: true);
				_PlantPlacementSettings.TransactionContainer.BeginTransaction();
				ModelCurveArray modelCurveArray = RevitUtility.NewModelCurveArray(_Document, ppOffsetLegList);
				ModelCurveArray modelCurveArray2 = RevitUtility.NewModelCurveArray(_Document, ppOffsetLegList2);
				_PlantPlacementSettings.TransactionContainer.EndTransaction();
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
				_PlantPlacementSettings.TransactionContainer.BeginTransaction();
				_Document.Delete(ElementSetConverter.ToIDList(elementSet));
				_PlantPlacementSettings.TransactionContainer.EndTransaction();
				if (!flag)
				{
					return false;
				}
				_PlantPlacementSettings.UseRightSide = _UseRightSide;
				if (_UseRightSide)
				{
					PlaceCenterRowOnLeg(_Document, _PlantPlacementSettings, ppOffsetLegList, _CurrentLeg.ElementList);
				}
				else
				{
					PlaceCenterRowOnLeg(_Document, _PlantPlacementSettings, ppOffsetLegList2, _CurrentLeg.ElementList);
				}
			}
		}
		if (_PlantPlacementSettings.DeleteExistingChecked && _CurrentLeg.ElementSet != null && _CurrentLeg.ElementSet.Size > 0)
		{
			_PlantPlacementSettings.TransactionContainer.BeginTransaction();
			_Document.Delete(ElementSetConverter.ToIDList(_CurrentLeg.ElementSet));
			_PlantPlacementSettings.TransactionContainer.EndTransaction();
		}
		return true;
	}

	public static void PickIslandLegs(Document _Document, ref List<Eps_Leg> _IslandList)
	{
		ElementSet _Elements = null;
		if (!RevitUtility.GetElementsFromProject(_Document, EPShared.LocalRM.GetString("PickIslandsMessage"), ref _Elements, _bDisplayError: false, PickMethod.PickObjects))
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
				RevitMessageBox.Show(EPShared.LocalRM.GetString("IslandEclipsePickMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				flag = true;
			}
		}
		if (flag2 && (_IslandList == null || _IslandList.Count == 0))
		{
			if (!flag)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("IslandClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
			}
			PickIslandLegs(_Document, ref _IslandList);
		}
	}

	public static void PlaceHedgeGrove(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _BoundaryLeg, List<Eps_Leg> _IslandList)
	{
		PlaceHedgeGrove(_Document, _PlantPlacementSettings, _BoundaryLeg, _IslandList, null);
	}

	public static void PlaceHedgeGrove(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _BoundaryLeg, List<Eps_Leg> _IslandList, List<FamilyInstance> _ExistingPlantList)
	{
		List<Eps_Leg> _IslandList2 = new List<Eps_Leg>();
		for (int i = 0; i < _IslandList.Count; i++)
		{
			_IslandList2.Add(Eps_Leg.CopyLeg(_IslandList[i]));
		}
		OffsetBoundaries(_PlantPlacementSettings.SetbackSpacing, ref _BoundaryLeg, ref _IslandList2);
		if (_BoundaryLeg != null)
		{
			DrawOrderedGrove(_Document, _PlantPlacementSettings, _BoundaryLeg, _IslandList2, _ExistingPlantList);
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

	private static void DrawOrderedGrove(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _BoundaryLeg, List<Eps_Leg> _IslandList, List<FamilyInstance> _ExistingPlantList)
	{
		if (_BoundaryLeg == null)
		{
			return;
		}
		List<Eps_Point> _PointList = new List<Eps_Point>();
		Eps_Point pMinPnt = new Eps_Point();
		Eps_Point pMaxPnt = new Eps_Point();
		RevitGeometry.eplGeoE_FindMinMaxOfLegList(_BoundaryLeg, ref pMinPnt, ref pMaxPnt);
		bool bSpacingType = false;
		if (_PlantPlacementSettings.PlacementType == PlacementType.GroveTrianglular)
		{
			bSpacingType = true;
		}
		RevitGeometry.CreatePointListFromMinMaxOfArea(ref _PointList, pMinPnt, pMaxPnt, _PlantPlacementSettings.RotationAngle, _PlantPlacementSettings.PlantSpacing, bSpacingType);
		RevitGeometry.GetAllPointsInBoundary(ref _PointList, _BoundaryLeg, _IslandList);
		ElementSet elementSet = new ElementSet();
		FamilyInstance familyInstance = null;
		double num = 0.0;
		double num2 = 0.0;
		bool flag = true;
		bool flag2 = true;
		bool flag3 = !_PlantPlacementSettings.DeleteExistingChecked;
		bool flag4 = false;
		bool flag5 = false;
		int num3 = 0;
		Parameter parameter = null;
		Parameter parameter2 = null;
		double num4 = 0.0;
		double num5 = 0.0;
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = _PointList.Count;
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingPlantsMessage");
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		bool bUseFailureHandling = true;
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			flag5 = true;
			_PlantPlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			list = _PlantPlacementSettings.GenericModelIDList;
		}
		for (int i = 0; i < _PointList.Count; i++)
		{
			if (_BoundaryLeg.Element != null && flag3 && !flag4)
			{
				if (flag5)
				{
					flag5 = false;
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
					{
						Guid chainID = Guid.NewGuid();
						list = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _BoundaryLeg.ElementList, string.Empty, chainID);
						if (list.Count > 0)
						{
							for (int j = 0; j < _IslandList.Count; j++)
							{
								RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _IslandList[j].ElementList, list[0].Value, chainID);
							}
						}
					}
					else
					{
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag3 = false;
					}
				}
				if (num3 < list.Count)
				{
					if (string.Compare(list[num3].Key, _BoundaryLeg.Element.UniqueId, ignoreCase: true) == 0)
					{
						flag4 = true;
					}
					else
					{
						for (int k = 0; k < list.Count; k++)
						{
							if (string.Compare(list[k].Key, _BoundaryLeg.Element.UniqueId, ignoreCase: true) == 0)
							{
								num3 = k;
								flag4 = true;
								break;
							}
						}
					}
				}
			}
			Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_PointList[i], _PlantPlacementSettings.SurfaceList);
			if ((surfaceContainingPoint == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlantList != null && _ExistingPlantList.Count > 0)
			{
				familyInstance = _ExistingPlantList[0];
				if (familyInstance.Location is LocationPoint locationPoint && !EPShared.eplGeoE_ArePntsDuplicates3D(_PointList[i], new Eps_Point(locationPoint.Point)))
				{
					XYZ translation = new XYZ(_PointList[i].XYZ.X - locationPoint.Point.X, _PointList[i].XYZ.Y - locationPoint.Point.Y, 0.0);
					ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
				}
				_ExistingPlantList.RemoveAt(0);
			}
			else
			{
				EPTransactionContainer _TransactionContainer = _PlantPlacementSettings.TransactionContainer;
				familyInstance = RevitUtility.NewFamilyInstance(_Document, _PointList[i].XY, _PlantPlacementSettings.Type, surfaceContainingPoint, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
				_PlantPlacementSettings.TransactionContainer = _TransactionContainer;
			}
			if (flag)
			{
				parameter = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
				if (parameter != null)
				{
					num4 = parameter.AsDouble();
					if (!RevitDefines.EQ(num4, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
					{
						num2 = Math.Abs(_PlantPlacementSettings.HeightVariance);
						num = ((!(num2 > 0.0)) ? _PlantPlacementSettings.Height : (_PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num2, num2)));
						parameter.Set(num);
					}
				}
				else
				{
					flag = false;
				}
			}
			if (flag2)
			{
				parameter2 = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
				if (parameter2 != null)
				{
					num5 = parameter2.AsDouble();
					if (!RevitDefines.EQ(num5, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
					{
						num2 = Math.Abs(_PlantPlacementSettings.WidthVariance);
						num = ((!(num2 > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num2, num2)));
						parameter2.Set(num);
					}
				}
				else
				{
					flag2 = false;
				}
			}
			if (_BoundaryLeg.Element != null && flag3 && flag4)
			{
				Parameter parameter3 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
				if (parameter3 != null)
				{
					parameter3.Set(list[num3].Value);
				}
				else
				{
					flag3 = false;
				}
			}
			elementSet.Insert(familyInstance);
			progressBarForm.UpdateProgressBar(1);
			flag4 = false;
		}
		if (_ExistingPlantList != null && _ExistingPlantList.Count > 0)
		{
			for (int l = 0; l < _ExistingPlantList.Count; l++)
			{
				_Document.Delete(_ExistingPlantList[l].Id);
			}
		}
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			_PlantPlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		if (!elementSet.IsEmpty)
		{
			bool bCreateTag = false;
			if (_PlantPlacementSettings.GenericModelIDList == null)
			{
				bCreateTag = true;
			}
			CreateTag(_Document, _PlantPlacementSettings.TransactionContainer, _PlantPlacementSettings.View, elementSet, _PlantPlacementSettings.PlantSpacing, _PlantPlacementSettings.PlaceGroupTagChecked, _PlantPlacementSettings.LeaderedTagChecked, bCreateTag);
		}
	}

	public static void PlaceSingleOnLeg(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _Leg)
	{
		PlaceSingleOnLeg(_Document, _PlantPlacementSettings, _Leg, null);
	}

	public static void PlaceSingleOnLeg(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _Leg, FamilyInstance _ExistingPlant)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		Eps_Leg eps_Leg = null;
		double _dDistance = 0.0;
		if (_Leg == null)
		{
			return;
		}
		ElementSet elementSet = new ElementSet();
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = RevitGeometry.eplGeoE_CalcLegListLength(_Leg);
		bool flag = !_PlantPlacementSettings.DeleteExistingChecked;
		bool flag2 = false;
		bool flag3 = false;
		int num5 = 0;
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(num4 / 2.0);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingPlantsMessage");
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		bool bUseFailureHandling = true;
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			flag3 = true;
			_PlantPlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			list = _PlantPlacementSettings.GenericModelIDList;
		}
		for (eps_Leg = _Leg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDistance);
				num3 += _dDistance;
				if (RevitDefines.GE(num3, num4 / 2.0))
				{
					if (eps_Leg.Element != null && flag && !flag2)
					{
						if (flag3)
						{
							flag3 = false;
							if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
							{
								list = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _Leg.ElementList);
							}
							else
							{
								WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
								warningOptionsForm.ShowDialog();
								flag = false;
							}
						}
						if (num5 < list.Count)
						{
							if (string.Compare(list[num5].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
							{
								flag2 = true;
							}
							else
							{
								for (int i = 0; i < list.Count; i++)
								{
									if (string.Compare(list[i].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										num5 = i;
										flag2 = true;
										break;
									}
								}
							}
						}
					}
					EPShared.eplGeoE_FindStaOffPointAlongLine(eps_Leg.Line, num4 / 2.0, 0.0, ref _StaOffPnt);
					Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlantPlacementSettings.SurfaceList);
					FamilyInstance familyInstance;
					if ((surfaceContainingPoint == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlant != null)
					{
						familyInstance = _ExistingPlant;
						if (familyInstance.Location is LocationPoint locationPoint && !EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint.Point)))
						{
							XYZ translation = new XYZ(_StaOffPnt.XYZ.X - locationPoint.Point.X, _StaOffPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
							ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
						}
					}
					else
					{
						EPTransactionContainer _TransactionContainer = _PlantPlacementSettings.TransactionContainer;
						familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlantPlacementSettings.Type, surfaceContainingPoint, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
						_PlantPlacementSettings.TransactionContainer = _TransactionContainer;
					}
					Parameter parameter = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
					if (parameter != null)
					{
						double dX = parameter.AsDouble();
						if (!RevitDefines.EQ(dX, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
						{
							num2 = Math.Abs(_PlantPlacementSettings.HeightVariance);
							if (num2 > 0.0)
							{
								num = _PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num2, num2);
							}
							else
							{
								num = _PlantPlacementSettings.Height;
							}
						}
					}
					parameter = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
					if (parameter != null)
					{
						double dX2 = parameter.AsDouble();
						if (!RevitDefines.EQ(dX2, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
						{
							num2 = Math.Abs(_PlantPlacementSettings.WidthVariance);
							num = ((!(num2 > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num2, num2)));
							parameter.Set(num);
						}
					}
					if (eps_Leg.Element != null && flag && flag2)
					{
						parameter = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
						if (parameter != null)
						{
							parameter.Set(list[num5].Value);
						}
						else
						{
							flag = false;
						}
					}
					elementSet.Insert(familyInstance);
					break;
				}
			}
			else if (eps_Leg.Arc != null)
			{
				num4 = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg.Arc.Radius, eps_Leg.Arc.Delta);
				EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num4 / 2.0, 0.0, ref _StaOffPnt);
				num3 += _dDistance;
				if (RevitDefines.GE(num3, num4 / 2.0))
				{
					if (eps_Leg.Element != null && flag && !flag2)
					{
						if (flag3)
						{
							flag3 = false;
							if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
							{
								list = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _Leg.ElementList);
							}
							else
							{
								WarningOptionsForm warningOptionsForm2 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
								warningOptionsForm2.ShowDialog();
								flag = false;
							}
						}
						if (num5 < list.Count)
						{
							if (string.Compare(list[num5].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
							{
								flag2 = true;
							}
							else
							{
								for (int j = 0; j < list.Count; j++)
								{
									if (string.Compare(list[j].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										num5 = j;
										flag2 = true;
										break;
									}
								}
							}
						}
					}
					EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num4 / 2.0, 0.0, ref _StaOffPnt);
					Element surfaceContainingPoint2 = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlantPlacementSettings.SurfaceList);
					FamilyInstance familyInstance;
					if ((surfaceContainingPoint2 == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlant != null)
					{
						familyInstance = _ExistingPlant;
						if (familyInstance.Location is LocationPoint locationPoint2 && !EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint2.Point)))
						{
							XYZ translation2 = new XYZ(_StaOffPnt.XYZ.X - locationPoint2.Point.X, _StaOffPnt.XYZ.Y - locationPoint2.Point.Y, 0.0);
							ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation2);
						}
					}
					else
					{
						EPTransactionContainer _TransactionContainer2 = _PlantPlacementSettings.TransactionContainer;
						familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlantPlacementSettings.Type, surfaceContainingPoint2, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer2);
						_PlantPlacementSettings.TransactionContainer = _TransactionContainer2;
					}
					Parameter parameter2 = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
					if (parameter2 != null)
					{
						double dX3 = parameter2.AsDouble();
						if (!RevitDefines.EQ(dX3, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
						{
							num2 = Math.Abs(_PlantPlacementSettings.HeightVariance);
							num = ((!(num2 > 0.0)) ? _PlantPlacementSettings.Height : (_PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num2, num2)));
							parameter2.Set(num);
						}
					}
					parameter2 = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
					if (parameter2 != null)
					{
						double dX4 = parameter2.AsDouble();
						if (!RevitDefines.EQ(dX4, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
						{
							num2 = Math.Abs(_PlantPlacementSettings.WidthVariance);
							num = ((!(num2 > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num2, num2)));
							parameter2.Set(num);
						}
					}
					if (eps_Leg.Element != null && flag && flag2)
					{
						parameter2 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
						if (parameter2 != null)
						{
							parameter2.Set(list[num5].Value);
						}
						else
						{
							flag = false;
						}
					}
					elementSet.Insert(familyInstance);
					break;
				}
			}
			progressBarForm.UpdateProgressBar(Convert.ToInt32(_dDistance));
			flag2 = false;
		}
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			_PlantPlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		if (!elementSet.IsEmpty)
		{
			bool bCreateTag = false;
			if (_PlantPlacementSettings.GenericModelIDList == null)
			{
				bCreateTag = true;
			}
			CreateTag(_Document, _PlantPlacementSettings.TransactionContainer, _PlantPlacementSettings.View, elementSet, _PlantPlacementSettings.PlantSpacing, _PlantPlacementSettings.PlaceGroupTagChecked, _PlantPlacementSettings.LeaderedTagChecked, bCreateTag);
		}
	}

	public static void PlaceCenterRowOnLeg(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _Leg)
	{
		PlaceCenterRowOnLeg(_Document, _PlantPlacementSettings, _Leg, _Leg.ElementList, null);
	}

	public static void PlaceCenterRowOnLeg(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _Leg, List<Element> _OrigLegElementSet)
	{
		PlaceCenterRowOnLeg(_Document, _PlantPlacementSettings, _Leg, _OrigLegElementSet, null);
	}

	public static void PlaceCenterRowOnLeg(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _Leg, List<FamilyInstance> _MovePlantList)
	{
		PlaceCenterRowOnLeg(_Document, _PlantPlacementSettings, _Leg, _Leg.ElementList, _MovePlantList);
	}

	public static void PlaceCenterRowOnLeg(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _Leg, List<Element> _OrigLegElementSet, List<FamilyInstance> _ExistingPlantList)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		Eps_Leg eps_Leg = null;
		double _dDistance = 0.0;
		double _dDirection = 0.0;
		double num = 0.0;
		double num2 = _PlantPlacementSettings.StartingDistance;
		double plantSpacing = _PlantPlacementSettings.PlantSpacing;
		if (_Leg == null)
		{
			return;
		}
		ElementSet elementSet = new ElementSet();
		List<Eps_Point> list = new List<Eps_Point>();
		double num3 = 0.0;
		double num4 = 0.0;
		bool flag = true;
		bool flag2 = true;
		bool flag3 = !_PlantPlacementSettings.DeleteExistingChecked;
		bool flag4 = false;
		bool flag5 = false;
		int num5 = 0;
		Parameter parameter = null;
		Parameter parameter2 = null;
		double num6 = 0.0;
		double num7 = 0.0;
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(RevitGeometry.eplGeoE_CalcLegListLength(_Leg) - num2);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingPlantsMessage");
		List<KeyValuePair<string, string>> list2 = new List<KeyValuePair<string, string>>();
		bool bUseFailureHandling = true;
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			flag5 = true;
			_PlantPlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			list2 = _PlantPlacementSettings.GenericModelIDList;
		}
		for (eps_Leg = _Leg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDistance);
				if (num2 > _dDistance)
				{
					num2 -= _dDistance;
				}
				else
				{
					for (num = num2; num <= _dDistance; num += plantSpacing)
					{
						EPShared.eplGeoE_CalcPolarToXY(eps_Leg.Line.StartPnt, _dDirection, num, ref _StaOffPnt);
						if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
						{
							if (eps_Leg.Element != null && flag3 && !flag4)
							{
								if (flag5)
								{
									flag5 = false;
									if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
									{
										list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _OrigLegElementSet);
									}
									else
									{
										WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
										warningOptionsForm.ShowDialog();
										flag3 = false;
									}
								}
								if (num5 < list2.Count)
								{
									if (string.Compare(list2[num5].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag4 = true;
									}
									else
									{
										for (int i = 0; i < list2.Count; i++)
										{
											if (string.Compare(list2[i].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num5 = i;
												flag4 = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlantPlacementSettings.SurfaceList);
							FamilyInstance familyInstance;
							if ((surfaceContainingPoint == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlantList != null && _ExistingPlantList.Count > 0)
							{
								familyInstance = _ExistingPlantList[0];
								if (familyInstance.Location is LocationPoint locationPoint && !EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint.Point)))
								{
									XYZ translation = new XYZ(_StaOffPnt.XYZ.X - locationPoint.Point.X, _StaOffPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
									ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
								}
								_ExistingPlantList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer = _PlantPlacementSettings.TransactionContainer;
								familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlantPlacementSettings.Type, surfaceContainingPoint, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
								_PlantPlacementSettings.TransactionContainer = _TransactionContainer;
							}
							if (flag)
							{
								parameter = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
								if (parameter != null)
								{
									num6 = parameter.AsDouble();
									if (!RevitDefines.EQ(num6, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
									{
										num4 = Math.Abs(_PlantPlacementSettings.HeightVariance);
										num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Height : (_PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num4, num4)));
										parameter.Set(num3);
									}
								}
								else
								{
									flag = false;
								}
							}
							if (flag2)
							{
								parameter2 = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
								if (parameter2 != null)
								{
									num7 = parameter2.AsDouble();
									if (!RevitDefines.EQ(num7, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
									{
										num4 = Math.Abs(_PlantPlacementSettings.WidthVariance);
										num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num4, num4)));
										parameter2.Set(num3);
									}
								}
								else
								{
									flag2 = false;
								}
							}
							if (eps_Leg.Element != null && flag3 && flag4)
							{
								Parameter parameter3 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter3 != null)
								{
									parameter3.Set(list2[num5].Value);
								}
								else
								{
									flag3 = false;
								}
							}
							elementSet.Insert(familyInstance);
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
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
							if (eps_Leg.Element != null && flag3 && !flag4)
							{
								if (flag5)
								{
									flag5 = false;
									if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
									{
										list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _OrigLegElementSet);
									}
									else
									{
										WarningOptionsForm warningOptionsForm2 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
										warningOptionsForm2.ShowDialog();
										flag3 = false;
									}
								}
								if (num5 < list2.Count)
								{
									if (string.Compare(list2[num5].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag4 = true;
									}
									else
									{
										for (int j = 0; j < list2.Count; j++)
										{
											if (string.Compare(list2[j].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num5 = j;
												flag4 = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint2 = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlantPlacementSettings.SurfaceList);
							FamilyInstance familyInstance;
							if ((surfaceContainingPoint2 == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlantList != null && _ExistingPlantList.Count > 0)
							{
								familyInstance = _ExistingPlantList[0];
								if (familyInstance.Location is LocationPoint locationPoint2 && !EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint2.Point)))
								{
									XYZ translation2 = new XYZ(_StaOffPnt.XYZ.X - locationPoint2.Point.X, _StaOffPnt.XYZ.Y - locationPoint2.Point.Y, 0.0);
									ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation2);
								}
								_ExistingPlantList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer2 = _PlantPlacementSettings.TransactionContainer;
								familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlantPlacementSettings.Type, surfaceContainingPoint2, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer2);
								_PlantPlacementSettings.TransactionContainer = _TransactionContainer2;
							}
							if (flag)
							{
								parameter = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
								if (parameter != null)
								{
									num6 = parameter.AsDouble();
									if (!RevitDefines.EQ(num6, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
									{
										num4 = Math.Abs(_PlantPlacementSettings.HeightVariance);
										num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Height : (_PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num4, num4)));
										parameter.Set(num3);
									}
								}
								else
								{
									flag = false;
								}
							}
							if (flag2)
							{
								parameter2 = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
								if (parameter2 != null)
								{
									num7 = parameter2.AsDouble();
									if (!RevitDefines.EQ(num7, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
									{
										num4 = Math.Abs(_PlantPlacementSettings.WidthVariance);
										num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num4, num4)));
										parameter2.Set(num3);
									}
								}
								else
								{
									flag2 = false;
								}
							}
							if (eps_Leg.Element != null && flag3 && flag4)
							{
								Parameter parameter4 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter4 != null)
								{
									parameter4.Set(list2[num5].Value);
								}
								else
								{
									flag3 = false;
								}
							}
							elementSet.Insert(familyInstance);
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
					}
					num2 = num - _dDistance;
				}
			}
			flag4 = false;
		}
		if (_ExistingPlantList != null && _ExistingPlantList.Count > 0)
		{
			for (int k = 0; k < _ExistingPlantList.Count; k++)
			{
				_Document.Delete(_ExistingPlantList[k].Id);
			}
		}
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			_PlantPlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		if (!elementSet.IsEmpty)
		{
			bool bCreateTag = false;
			if (_PlantPlacementSettings.GenericModelIDList == null)
			{
				bCreateTag = true;
			}
			CreateTag(_Document, _PlantPlacementSettings.TransactionContainer, _PlantPlacementSettings.View, elementSet, _PlantPlacementSettings.PlantSpacing, _PlantPlacementSettings.PlaceGroupTagChecked, _PlantPlacementSettings.LeaderedTagChecked, bCreateTag);
		}
	}

	public static void PlaceAltRowOnLeg(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _Leg)
	{
		PlaceAltRowOnLeg(_Document, _PlantPlacementSettings, _Leg, null);
	}

	public static void PlaceAltRowOnLeg(Document _Document, PlacementSettings _PlantPlacementSettings, Eps_Leg _Leg, List<FamilyInstance> _ExistingPlantList)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		Eps_Leg eps_Leg = null;
		double _dDistance = 0.0;
		double _dTangentDir = 0.0;
		double num = 0.0;
		double num2 = _PlantPlacementSettings.StartingDistance;
		double setbackSpacing = _PlantPlacementSettings.SetbackSpacing;
		double plantSpacing = _PlantPlacementSettings.PlantSpacing;
		bool flag = true;
		if (_Leg == null)
		{
			return;
		}
		ElementSet elementSet = new ElementSet();
		List<Eps_Point> list = new List<Eps_Point>();
		double num3 = 0.0;
		double num4 = 0.0;
		bool flag2 = true;
		bool flag3 = true;
		bool flag4 = !_PlantPlacementSettings.DeleteExistingChecked;
		bool flag5 = false;
		bool flag6 = false;
		int num5 = 0;
		Parameter parameter = null;
		Parameter parameter2 = null;
		double num6 = 0.0;
		double num7 = 0.0;
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(RevitGeometry.eplGeoE_CalcLegListLength(_Leg) - num2);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingPlantsMessage");
		List<KeyValuePair<string, string>> list2 = new List<KeyValuePair<string, string>>();
		bool bUseFailureHandling = true;
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			flag6 = true;
			_PlantPlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			list2 = _PlantPlacementSettings.GenericModelIDList;
		}
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
							if (eps_Leg.Element != null && flag4 && !flag5)
							{
								if (flag6)
								{
									flag6 = false;
									if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
									{
										list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _Leg.ElementList);
									}
									else
									{
										WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
										warningOptionsForm.ShowDialog();
										flag4 = false;
									}
								}
								if (num5 < list2.Count)
								{
									if (string.Compare(list2[num5].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag5 = true;
									}
									else
									{
										for (int i = 0; i < list2.Count; i++)
										{
											if (string.Compare(list2[i].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num5 = i;
												flag5 = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlantPlacementSettings.SurfaceList);
							FamilyInstance familyInstance;
							if ((surfaceContainingPoint == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlantList != null && _ExistingPlantList.Count > 0)
							{
								familyInstance = _ExistingPlantList[0];
								if (familyInstance.Location is LocationPoint locationPoint && !EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint.Point)))
								{
									XYZ translation = new XYZ(_StaOffPnt.XYZ.X - locationPoint.Point.X, _StaOffPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
									ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
								}
								_ExistingPlantList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer = _PlantPlacementSettings.TransactionContainer;
								familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlantPlacementSettings.Type, surfaceContainingPoint, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
								_PlantPlacementSettings.TransactionContainer = _TransactionContainer;
							}
							if (flag2)
							{
								parameter = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
								if (parameter != null)
								{
									num6 = parameter.AsDouble();
									if (!RevitDefines.EQ(num6, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
									{
										num4 = Math.Abs(_PlantPlacementSettings.HeightVariance);
										num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Height : (_PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num4, num4)));
										parameter.Set(num3);
									}
								}
								else
								{
									flag2 = false;
								}
							}
							if (flag3)
							{
								parameter2 = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
								if (parameter2 != null)
								{
									num7 = parameter2.AsDouble();
									if (!RevitDefines.EQ(num7, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
									{
										num4 = Math.Abs(_PlantPlacementSettings.WidthVariance);
										num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num4, num4)));
										parameter2.Set(num3);
									}
								}
								else
								{
									flag3 = false;
								}
							}
							if (eps_Leg.Element != null && flag4 && flag5)
							{
								Parameter parameter3 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter3 != null)
								{
									parameter3.Set(list2[num5].Value);
								}
								else
								{
									flag4 = false;
								}
							}
							elementSet.Insert(familyInstance);
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
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
							if (eps_Leg.Element != null && flag4 && !flag5)
							{
								if (flag6)
								{
									flag6 = false;
									if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
									{
										list2 = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _Leg.ElementList);
									}
									else
									{
										WarningOptionsForm warningOptionsForm2 = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
										warningOptionsForm2.ShowDialog();
										flag4 = false;
									}
								}
								if (num5 < list2.Count)
								{
									if (string.Compare(list2[num5].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
									{
										flag5 = true;
									}
									else
									{
										for (int j = 0; j < list2.Count; j++)
										{
											if (string.Compare(list2[j].Key, eps_Leg.Element.UniqueId, ignoreCase: true) == 0)
											{
												num5 = j;
												flag5 = true;
												break;
											}
										}
									}
								}
							}
							list.Add(_StaOffPnt);
							Element surfaceContainingPoint2 = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlantPlacementSettings.SurfaceList);
							FamilyInstance familyInstance;
							if ((surfaceContainingPoint2 == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlantList != null && _ExistingPlantList.Count > 0)
							{
								familyInstance = _ExistingPlantList[0];
								if (familyInstance.Location is LocationPoint locationPoint2 && !EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint2.Point)))
								{
									XYZ translation2 = new XYZ(_StaOffPnt.XYZ.X - locationPoint2.Point.X, _StaOffPnt.XYZ.Y - locationPoint2.Point.Y, 0.0);
									ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation2);
								}
								_ExistingPlantList.RemoveAt(0);
							}
							else
							{
								EPTransactionContainer _TransactionContainer2 = _PlantPlacementSettings.TransactionContainer;
								familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlantPlacementSettings.Type, surfaceContainingPoint2, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer2);
								_PlantPlacementSettings.TransactionContainer = _TransactionContainer2;
							}
							if (flag2)
							{
								parameter = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
								if (parameter != null)
								{
									num6 = parameter.AsDouble();
									if (!RevitDefines.EQ(num6, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
									{
										num4 = Math.Abs(_PlantPlacementSettings.HeightVariance);
										num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Height : (_PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num4, num4)));
										parameter.Set(num3);
									}
								}
								else
								{
									flag2 = false;
								}
							}
							if (flag3)
							{
								parameter2 = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
								if (parameter2 != null)
								{
									num7 = parameter2.AsDouble();
									if (!RevitDefines.EQ(num7, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
									{
										num4 = Math.Abs(_PlantPlacementSettings.WidthVariance);
										num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num4, num4)));
										parameter2.Set(num3);
									}
								}
								else
								{
									flag3 = false;
								}
							}
							if (eps_Leg.Element != null && flag4 && flag5)
							{
								Parameter parameter4 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
								if (parameter4 != null)
								{
									parameter4.Set(list2[num5].Value);
								}
								else
								{
									flag4 = false;
								}
							}
							elementSet.Insert(familyInstance);
						}
						progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
					}
					num2 = num - _dDistance;
				}
			}
			flag5 = false;
		}
		if (_ExistingPlantList != null && _ExistingPlantList.Count > 0)
		{
			for (int k = 0; k < _ExistingPlantList.Count; k++)
			{
				_Document.Delete(_ExistingPlantList[k].Id);
			}
		}
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			_PlantPlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		if (!elementSet.IsEmpty)
		{
			bool bCreateTag = false;
			if (_PlantPlacementSettings.GenericModelIDList == null)
			{
				bCreateTag = true;
			}
			CreateTag(_Document, _PlantPlacementSettings.TransactionContainer, _PlantPlacementSettings.View, elementSet, _PlantPlacementSettings.PlantSpacing, _PlantPlacementSettings.PlaceGroupTagChecked, _PlantPlacementSettings.LeaderedTagChecked, bCreateTag);
		}
	}

	public static ElementSet PlaceCenterRowOnLine(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Line _Line)
	{
		return PlaceCenterRowOnLine(_Document, _PlantPlacementSettings, _Element, _Line, _bCreateGroupTag: true, null);
	}

	public static ElementSet PlaceCenterRowOnLine(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Line _Line, List<FamilyInstance> _ExistingPlantList)
	{
		return PlaceCenterRowOnLine(_Document, _PlantPlacementSettings, _Element, _Line, _bCreateGroupTag: true, _ExistingPlantList);
	}

	public static ElementSet PlaceCenterRowOnLine(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Line _Line, bool _bCreateGroupTag, List<FamilyInstance> _ExistingPlantList)
	{
		Eps_Point _NewPnt = new Eps_Point();
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		double num = 0.0;
		double plantSpacing = _PlantPlacementSettings.PlantSpacing;
		if (_Line == null)
		{
			return null;
		}
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDirection);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDistance);
		ElementSet elementSet = new ElementSet();
		List<Eps_Point> list = new List<Eps_Point>();
		FamilyInstance familyInstance = null;
		double num2 = 0.0;
		double num3 = 0.0;
		bool flag = true;
		bool flag2 = true;
		bool flag3 = !_PlantPlacementSettings.DeleteExistingChecked;
		bool flag4 = false;
		Parameter parameter = null;
		Parameter parameter2 = null;
		double num4 = 0.0;
		double num5 = 0.0;
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(_dDistance - _PlantPlacementSettings.StartingDistance);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingPlantsMessage");
		KeyValuePair<string, string> keyValuePair = default;
		bool bUseFailureHandling = true;
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			flag4 = true;
			_PlantPlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			keyValuePair = _PlantPlacementSettings.GenericModelIDList[0];
		}
		for (num = _PlantPlacementSettings.StartingDistance; num <= _dDistance; num += plantSpacing)
		{
			EPShared.eplGeoE_CalcPolarToXY(_Line.StartPnt, _dDirection, num, ref _NewPnt);
			if (!RevitUtility.IsPointInPointList(_NewPnt, list))
			{
				if (_Element != null && flag3 && flag4)
				{
					flag4 = false;
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
					{
						keyValuePair = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _Element);
					}
					else
					{
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag3 = false;
					}
				}
				list.Add(_NewPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlantPlacementSettings.SurfaceList);
				if ((surfaceContainingPoint == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlantList != null && _ExistingPlantList.Count > 0)
				{
					familyInstance = _ExistingPlantList[0];
					if (familyInstance.Location is LocationPoint locationPoint && !EPShared.eplGeoE_ArePntsDuplicates3D(_NewPnt, new Eps_Point(locationPoint.Point)))
					{
						XYZ translation = new XYZ(_NewPnt.XYZ.X - locationPoint.Point.X, _NewPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
						ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
					}
					_ExistingPlantList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer = _PlantPlacementSettings.TransactionContainer;
					familyInstance = RevitUtility.NewFamilyInstance(_Document, _NewPnt.XY, _PlantPlacementSettings.Type, surfaceContainingPoint, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
					_PlantPlacementSettings.TransactionContainer = _TransactionContainer;
				}
				if (flag)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
					if (parameter != null)
					{
						num4 = parameter.AsDouble();
						if (!RevitDefines.EQ(num4, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
						{
							num3 = Math.Abs(_PlantPlacementSettings.HeightVariance);
							num2 = ((!(num3 > 0.0)) ? _PlantPlacementSettings.Height : (_PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num3, num3)));
							parameter.Set(num2);
						}
					}
					else
					{
						flag = false;
					}
				}
				if (flag2)
				{
					parameter2 = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
					if (parameter2 != null)
					{
						num5 = parameter2.AsDouble();
						if (!RevitDefines.EQ(num5, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
						{
							num3 = Math.Abs(_PlantPlacementSettings.WidthVariance);
							num2 = ((!(num3 > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num3, num3)));
							parameter2.Set(num2);
						}
					}
					else
					{
						flag2 = false;
					}
				}
				if (_Element != null && flag3 && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter3 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter3 != null)
					{
						parameter3.Set(keyValuePair.Value);
					}
					else
					{
						flag3 = false;
					}
				}
				elementSet.Insert(familyInstance);
			}
			progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
		}
		if (_ExistingPlantList != null && _ExistingPlantList.Count > 0)
		{
			for (int i = 0; i < _ExistingPlantList.Count; i++)
			{
				_Document.Delete(_ExistingPlantList[i].Id);
			}
		}
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			_PlantPlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		if (_bCreateGroupTag && !elementSet.IsEmpty)
		{
			bool bCreateTag = false;
			if (_PlantPlacementSettings.GenericModelIDList == null)
			{
				bCreateTag = true;
			}
			CreateTag(_Document, _PlantPlacementSettings.TransactionContainer, _PlantPlacementSettings.View, elementSet, _PlantPlacementSettings.PlantSpacing, _PlantPlacementSettings.PlaceGroupTagChecked, _PlantPlacementSettings.LeaderedTagChecked, bCreateTag);
		}
		return elementSet;
	}

	public static ElementSet PlaceAltRowOnLine(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Line _Line)
	{
		bool _bOffsetRight = true;
		return PlaceAltRowOnLine(_Document, _PlantPlacementSettings, _Element, _Line, ref _bOffsetRight, _bCreateGroupTag: true, null);
	}

	public static ElementSet PlaceAltRowOnLine(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Line _Line, List<FamilyInstance> _ExistingPlantList)
	{
		bool _bOffsetRight = true;
		return PlaceAltRowOnLine(_Document, _PlantPlacementSettings, _Element, _Line, ref _bOffsetRight, _bCreateGroupTag: true, _ExistingPlantList);
	}

	public static ElementSet PlaceAltRowOnLine(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Line _Line, ref bool _bOffsetRight, bool _bCreateGroupTag, List<FamilyInstance> _ExistingPlantList)
	{
		Eps_Point _NewPnt = new Eps_Point();
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		double num = 0.0;
		double setbackSpacing = _PlantPlacementSettings.SetbackSpacing;
		double plantSpacing = _PlantPlacementSettings.PlantSpacing;
		if (_Line == null)
		{
			return null;
		}
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDirection);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_Line.StartPnt, _Line.EndPnt, ref _dDistance);
		ElementSet elementSet = new ElementSet();
		List<Eps_Point> list = new List<Eps_Point>();
		FamilyInstance familyInstance = null;
		double num2 = 0.0;
		double num3 = 0.0;
		bool flag = true;
		bool flag2 = true;
		bool flag3 = !_PlantPlacementSettings.DeleteExistingChecked;
		bool flag4 = false;
		Parameter parameter = null;
		Parameter parameter2 = null;
		double num4 = 0.0;
		double num5 = 0.0;
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(_dDistance - _PlantPlacementSettings.StartingDistance);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingPlantsMessage");
		KeyValuePair<string, string> keyValuePair = default;
		bool bUseFailureHandling = true;
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			flag4 = true;
			_PlantPlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			keyValuePair = _PlantPlacementSettings.GenericModelIDList[0];
		}
		for (num = _PlantPlacementSettings.StartingDistance; num <= _dDistance; num += plantSpacing / 2.0)
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
				if (_Element != null && flag3 && flag4)
				{
					flag4 = false;
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
					{
						keyValuePair = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _Element);
					}
					else
					{
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag3 = false;
					}
				}
				list.Add(_NewPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_NewPnt, _PlantPlacementSettings.SurfaceList);
				if ((surfaceContainingPoint == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlantList != null && _ExistingPlantList.Count > 0)
				{
					familyInstance = _ExistingPlantList[0];
					if (familyInstance.Location is LocationPoint locationPoint && !EPShared.eplGeoE_ArePntsDuplicates3D(_NewPnt, new Eps_Point(locationPoint.Point)))
					{
						XYZ translation = new XYZ(_NewPnt.XYZ.X - locationPoint.Point.X, _NewPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
						ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
					}
					_ExistingPlantList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer = _PlantPlacementSettings.TransactionContainer;
					familyInstance = RevitUtility.NewFamilyInstance(_Document, _NewPnt.XY, _PlantPlacementSettings.Type, surfaceContainingPoint, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
					_PlantPlacementSettings.TransactionContainer = _TransactionContainer;
				}
				if (flag)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
					if (parameter != null)
					{
						num4 = parameter.AsDouble();
						if (!RevitDefines.EQ(num4, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
						{
							num3 = Math.Abs(_PlantPlacementSettings.HeightVariance);
							num2 = ((!(num3 > 0.0)) ? _PlantPlacementSettings.Height : (_PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num3, num3)));
							parameter.Set(num2);
						}
					}
					else
					{
						flag = false;
					}
				}
				if (flag2)
				{
					parameter2 = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
					if (parameter2 != null)
					{
						num5 = parameter2.AsDouble();
						if (!RevitDefines.EQ(num5, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
						{
							num3 = Math.Abs(_PlantPlacementSettings.WidthVariance);
							num2 = ((!(num3 > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num3, num3)));
							parameter2.Set(num2);
						}
					}
					else
					{
						flag2 = false;
					}
				}
				if (_Element != null && flag3 && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter3 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter3 != null)
					{
						parameter3.Set(keyValuePair.Value);
					}
					else
					{
						flag3 = false;
					}
				}
				elementSet.Insert(familyInstance);
			}
			progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
		}
		if (_ExistingPlantList != null && _ExistingPlantList.Count > 0)
		{
			for (int i = 0; i < _ExistingPlantList.Count; i++)
			{
				_Document.Delete(_ExistingPlantList[i].Id);
			}
		}
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			_PlantPlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		if (_bCreateGroupTag && !elementSet.IsEmpty)
		{
			bool bCreateTag = false;
			if (_PlantPlacementSettings.GenericModelIDList == null)
			{
				bCreateTag = true;
			}
			CreateTag(_Document, _PlantPlacementSettings.TransactionContainer, _PlantPlacementSettings.View, elementSet, _PlantPlacementSettings.PlantSpacing, _PlantPlacementSettings.PlaceGroupTagChecked, _PlantPlacementSettings.LeaderedTagChecked, bCreateTag);
		}
		return elementSet;
	}

	public static void PlaceCenterRowOnArc(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Arc _Arc)
	{
		PlaceCenterRowOnArc(_Document, _PlantPlacementSettings, _Element, _Arc, null);
	}

	public static void PlaceCenterRowOnArc(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Arc _Arc, List<FamilyInstance> _ExistingPlantList)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		double num = 0.0;
		double plantSpacing = _PlantPlacementSettings.PlantSpacing;
		double num2 = 0.0;
		if (_Arc == null)
		{
			return;
		}
		num2 = _Arc.Radius * _Arc.Delta;
		ElementSet elementSet = new ElementSet();
		List<Eps_Point> list = new List<Eps_Point>();
		FamilyInstance familyInstance = null;
		double num3 = 0.0;
		double num4 = 0.0;
		bool flag = true;
		bool flag2 = true;
		bool flag3 = !_PlantPlacementSettings.DeleteExistingChecked;
		bool flag4 = false;
		Parameter parameter = null;
		Parameter parameter2 = null;
		double num5 = 0.0;
		double num6 = 0.0;
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(num2 - _PlantPlacementSettings.StartingDistance);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingPlantsMessage");
		KeyValuePair<string, string> keyValuePair = default;
		bool bUseFailureHandling = true;
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			flag4 = true;
			_PlantPlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			keyValuePair = _PlantPlacementSettings.GenericModelIDList[0];
		}
		for (num = _PlantPlacementSettings.StartingDistance; num <= num2; num += plantSpacing)
		{
			EPShared.eplGeoE_FindStaOffPointAlongArc(_Arc, num, 0.0, ref _StaOffPnt);
			if (!RevitUtility.IsPointInPointList(_StaOffPnt, list))
			{
				if (_Element != null && flag3 && flag4)
				{
					flag4 = false;
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
					{
						keyValuePair = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _Element);
					}
					else
					{
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag3 = false;
					}
				}
				list.Add(_StaOffPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlantPlacementSettings.SurfaceList);
				if ((surfaceContainingPoint == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlantList != null && _ExistingPlantList.Count > 0)
				{
					familyInstance = _ExistingPlantList[0];
					if (familyInstance.Location is LocationPoint locationPoint && !EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint.Point)))
					{
						XYZ translation = new XYZ(_StaOffPnt.XYZ.X - locationPoint.Point.X, _StaOffPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
						ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
					}
					_ExistingPlantList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer = _PlantPlacementSettings.TransactionContainer;
					familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlantPlacementSettings.Type, surfaceContainingPoint, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
					_PlantPlacementSettings.TransactionContainer = _TransactionContainer;
				}
				if (flag)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
					if (parameter != null)
					{
						num5 = parameter.AsDouble();
						if (!RevitDefines.EQ(num5, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
						{
							num4 = Math.Abs(_PlantPlacementSettings.HeightVariance);
							num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Height : (_PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num4, num4)));
							parameter.Set(num3);
						}
					}
					else
					{
						flag = false;
					}
				}
				if (flag2)
				{
					parameter2 = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
					if (parameter2 != null)
					{
						num6 = parameter2.AsDouble();
						if (!RevitDefines.EQ(num6, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
						{
							num4 = Math.Abs(_PlantPlacementSettings.WidthVariance);
							num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num4, num4)));
							parameter2.Set(num3);
						}
					}
					else
					{
						flag2 = false;
					}
				}
				if (_Element != null && flag3 && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter3 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter3 != null)
					{
						parameter3.Set(keyValuePair.Value);
					}
					else
					{
						flag3 = false;
					}
				}
				elementSet.Insert(familyInstance);
			}
			progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
		}
		if (_ExistingPlantList != null && _ExistingPlantList.Count > 0)
		{
			for (int i = 0; i < _ExistingPlantList.Count; i++)
			{
				_Document.Delete(_ExistingPlantList[i].Id);
			}
		}
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			_PlantPlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		if (!elementSet.IsEmpty)
		{
			bool bCreateTag = false;
			if (_PlantPlacementSettings.GenericModelIDList == null)
			{
				bCreateTag = true;
			}
			CreateTag(_Document, _PlantPlacementSettings.TransactionContainer, _PlantPlacementSettings.View, elementSet, _PlantPlacementSettings.PlantSpacing, _PlantPlacementSettings.PlaceGroupTagChecked, _PlantPlacementSettings.LeaderedTagChecked, bCreateTag);
		}
	}

	public static void PlaceAltRowOnArc(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Arc _Arc)
	{
		PlaceAltRowOnArc(_Document, _PlantPlacementSettings, _Element, _Arc, null);
	}

	public static void PlaceAltRowOnArc(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Arc _Arc, List<FamilyInstance> _ExistingPlantList)
	{
		Eps_Point _StaOffPnt = new Eps_Point();
		double num = 0.0;
		double _dTangentDir = 0.0;
		bool flag = true;
		double setbackSpacing = _PlantPlacementSettings.SetbackSpacing;
		double plantSpacing = _PlantPlacementSettings.PlantSpacing;
		double num2 = 0.0;
		if (_Arc == null)
		{
			return;
		}
		num2 = _Arc.Radius * _Arc.Delta;
		ElementSet elementSet = new ElementSet();
		List<Eps_Point> list = new List<Eps_Point>();
		FamilyInstance familyInstance = null;
		double num3 = 0.0;
		double num4 = 0.0;
		bool flag2 = true;
		bool flag3 = true;
		bool flag4 = !_PlantPlacementSettings.DeleteExistingChecked;
		bool flag5 = false;
		Parameter parameter = null;
		Parameter parameter2 = null;
		double num5 = 0.0;
		double num6 = 0.0;
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(num2 - _PlantPlacementSettings.StartingDistance);
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingPlantsMessage");
		KeyValuePair<string, string> keyValuePair = default;
		bool bUseFailureHandling = true;
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			flag5 = true;
			_PlantPlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			keyValuePair = _PlantPlacementSettings.GenericModelIDList[0];
		}
		for (num = _PlantPlacementSettings.StartingDistance; num <= num2; num += plantSpacing / 2.0)
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
				if (_Element != null && flag4 && flag5)
				{
					flag5 = false;
					if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
					{
						keyValuePair = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _Element);
					}
					else
					{
						WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
						warningOptionsForm.ShowDialog();
						flag4 = false;
					}
				}
				list.Add(_StaOffPnt);
				Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(_StaOffPnt, _PlantPlacementSettings.SurfaceList);
				if ((surfaceContainingPoint == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlantList != null && _ExistingPlantList.Count > 0)
				{
					familyInstance = _ExistingPlantList[0];
					if (familyInstance.Location is LocationPoint locationPoint && !EPShared.eplGeoE_ArePntsDuplicates3D(_StaOffPnt, new Eps_Point(locationPoint.Point)))
					{
						XYZ translation = new XYZ(_StaOffPnt.XYZ.X - locationPoint.Point.X, _StaOffPnt.XYZ.Y - locationPoint.Point.Y, 0.0);
						ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
					}
					_ExistingPlantList.RemoveAt(0);
				}
				else
				{
					EPTransactionContainer _TransactionContainer = _PlantPlacementSettings.TransactionContainer;
					familyInstance = RevitUtility.NewFamilyInstance(_Document, _StaOffPnt.XY, _PlantPlacementSettings.Type, surfaceContainingPoint, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
					_PlantPlacementSettings.TransactionContainer = _TransactionContainer;
				}
				if (flag2)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
					if (parameter != null)
					{
						num5 = parameter.AsDouble();
						if (!RevitDefines.EQ(num5, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
						{
							num4 = Math.Abs(_PlantPlacementSettings.HeightVariance);
							num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Height : (_PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num4, num4)));
							parameter.Set(num3);
						}
					}
					else
					{
						flag2 = false;
					}
				}
				if (flag3)
				{
					parameter2 = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
					if (parameter2 != null)
					{
						num6 = parameter2.AsDouble();
						if (!RevitDefines.EQ(num6, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
						{
							num4 = Math.Abs(_PlantPlacementSettings.WidthVariance);
							num3 = ((!(num4 > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num4, num4)));
							parameter2.Set(num3);
						}
					}
					else
					{
						flag3 = false;
					}
				}
				if (_Element != null && flag4 && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
				{
					Parameter parameter3 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
					if (parameter3 != null)
					{
						parameter3.Set(keyValuePair.Value);
					}
					else
					{
						flag4 = false;
					}
				}
				elementSet.Insert(familyInstance);
			}
			progressBarForm.UpdateProgressBar(Convert.ToInt32(plantSpacing));
		}
		if (_ExistingPlantList != null && _ExistingPlantList.Count > 0)
		{
			for (int i = 0; i < _ExistingPlantList.Count; i++)
			{
				_Document.Delete(_ExistingPlantList[i].Id);
			}
		}
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			_PlantPlacementSettings.TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		if (!elementSet.IsEmpty)
		{
			bool bCreateTag = false;
			if (_PlantPlacementSettings.GenericModelIDList == null)
			{
				bCreateTag = true;
			}
			CreateTag(_Document, _PlantPlacementSettings.TransactionContainer, _PlantPlacementSettings.View, elementSet, _PlantPlacementSettings.PlantSpacing, _PlantPlacementSettings.PlaceGroupTagChecked, _PlantPlacementSettings.LeaderedTagChecked, bCreateTag);
		}
	}

	public static FamilyInstance PlaceSingleOnPoint(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Point originPoint)
	{
		return PlaceSingleOnPoint(_Document, _PlantPlacementSettings, _Element, originPoint, _bCreateGroupTag: true, null);
	}

	public static FamilyInstance PlaceSingleOnPoint(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Point originPoint, FamilyInstance _ExistingPlant)
	{
		return PlaceSingleOnPoint(_Document, _PlantPlacementSettings, _Element, originPoint, _bCreateGroupTag: true, _ExistingPlant);
	}

	public static FamilyInstance PlaceSingleOnPoint(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Point originPoint, bool _bCreateTag)
	{
		return PlaceSingleOnPoint(_Document, _PlantPlacementSettings, _Element, originPoint, _bCreateTag, null);
	}

	public static FamilyInstance PlaceSingleOnPoint(Document _Document, PlacementSettings _PlantPlacementSettings, Element _Element, Eps_Point originPoint, bool _bCreateGroupTag, FamilyInstance _ExistingPlant)
	{
		double num = 0.0;
		double num2 = 0.0;
		KeyValuePair<string, string> keyValuePair = default;
		bool bUseFailureHandling = true;
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			_PlantPlacementSettings.TransactionContainer.BeginTransaction();
		}
		else
		{
			bUseFailureHandling = false;
			keyValuePair = _PlantPlacementSettings.GenericModelIDList[0];
		}
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		FamilyInstance familyInstance = null;
		if (!_PlantPlacementSettings.DeleteExistingChecked && _Element != null && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
		{
			if (RevitUtility.IsParameterIDOnFamily(_Document, _PlantPlacementSettings, _PlantPlacementSettings.Type))
			{
				if (_PlantPlacementSettings.GenericModelIDList == null)
				{
					keyValuePair = RevitUtility.CreatePlacementGenericModel(_Document, _PlantPlacementSettings, _Element);
				}
			}
			else
			{
				WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), _Document.PathName, "PlacePlantNoPositioning", EPShared.LocalRM.GetString("PositionPlantsMessage") + EPShared.LocalRM.GetString("_PlacedWontUpdateWithHost_Message") + EPShared.LocalRM.GetString("PlacementIDFamilyParameter") + EPShared.LocalRM.GetString("_ParametersNotPartOfFamilyMessage"), new List<string>(), -1, _bDisplayOKCancel: false);
				warningOptionsForm.ShowDialog();
			}
		}
		Element surfaceContainingPoint = RevitUtility.GetSurfaceContainingPoint(originPoint, _PlantPlacementSettings.SurfaceList);
		if ((surfaceContainingPoint == null || _PlantPlacementSettings.GenericModelIDList != null) && _ExistingPlant != null)
		{
			familyInstance = _ExistingPlant;
			if (familyInstance.Location is LocationPoint locationPoint && !EPShared.eplGeoE_ArePntsDuplicates3D(originPoint, new Eps_Point(locationPoint.Point)))
			{
				XYZ translation = new XYZ(originPoint.XYZ.X - locationPoint.Point.X, originPoint.XYZ.Y - locationPoint.Point.Y, 0.0);
				ElementTransformUtils.MoveElement(_Document, familyInstance.Id, translation);
			}
		}
		else
		{
			EPTransactionContainer _TransactionContainer = _PlantPlacementSettings.TransactionContainer;
			familyInstance = RevitUtility.NewFamilyInstance(_Document, originPoint.XY, _PlantPlacementSettings.Type, surfaceContainingPoint, _PlantPlacementSettings.Level, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
			_PlantPlacementSettings.TransactionContainer = _TransactionContainer;
		}
		Parameter parameter = familyInstance.LookupParameter(RevitParameters.PlantHeightPlantFamilyParameter);
		if (parameter != null)
		{
			double dX = parameter.AsDouble();
			if (!RevitDefines.EQ(dX, _PlantPlacementSettings.Height) || Math.Abs(_PlantPlacementSettings.HeightVariance) > 0.0)
			{
				num = Math.Abs(_PlantPlacementSettings.HeightVariance);
				num2 = ((!(num > 0.0)) ? _PlantPlacementSettings.Height : (_PlantPlacementSettings.Height + RevitGeometry.RandomDouble(0.0 - num, num)));
				parameter.Set(num2);
			}
		}
		parameter = familyInstance.LookupParameter(RevitParameters.PlantWidthPlantFamilyParameter);
		if (parameter != null)
		{
			double dX2 = parameter.AsDouble();
			if (!RevitDefines.EQ(dX2, _PlantPlacementSettings.Width) || Math.Abs(_PlantPlacementSettings.WidthVariance) > 0.0)
			{
				num = Math.Abs(_PlantPlacementSettings.WidthVariance);
				num2 = ((!(num > 0.0)) ? _PlantPlacementSettings.Width : (_PlantPlacementSettings.Width + RevitGeometry.RandomDouble(0.0 - num, num)));
				parameter.Set(num2);
			}
		}
		if (!_PlantPlacementSettings.DeleteExistingChecked && _Element != null && string.Compare(keyValuePair.Key, _Element.UniqueId, ignoreCase: true) == 0)
		{
			familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter)?.Set(keyValuePair.Value);
		}
		if (_PlantPlacementSettings.GenericModelIDList == null)
		{
			_PlantPlacementSettings.TransactionContainer.EndTransaction();
		}
		if (_bCreateGroupTag)
		{
			ElementSet elementSet = new ElementSet();
			elementSet.Insert(familyInstance);
			bool bCreateTag = false;
			if (_PlantPlacementSettings.GenericModelIDList == null)
			{
				bCreateTag = true;
			}
			CreateTag(_Document, _PlantPlacementSettings.TransactionContainer, _PlantPlacementSettings.View, elementSet, _PlantPlacementSettings.PlantSpacing, _PlantPlacementSettings.PlaceGroupTagChecked, _PlantPlacementSettings.LeaderedTagChecked, bCreateTag);
		}
		return familyInstance;
	}
}
