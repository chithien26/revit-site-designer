using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogIcon = Autodesk.Revit.UI.TaskDialogIcon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using SITES.Utilities;

namespace EPCommon;

public class TopographySurfaceManagement
{
	private static double m_dCurbElevation;

	private static void SetTopoDrawOrder(Document _Document, ref int _TopoDrawOrder)
	{
		if (_TopoDrawOrder == -1)
		{
			_TopoDrawOrder = RevitUtility.FindMaxTopoDrawOrderFromSiteComponents(_Document);
		}
		_TopoDrawOrder++;
	}

	public static bool VerifyUserCanManipulateElements(Document _Document, List<Element> _ElementList)
	{
		List<KeyValuePair<string, Element>> list = new List<KeyValuePair<string, Element>>();
		if (_Document.IsWorkshared && _ElementList != null && _ElementList.Count > 0)
		{
			string empty = string.Empty;
			empty = _Document.Application.Username;
			if (empty == null || empty == string.Empty)
			{
				string[] array = WindowsIdentity.GetCurrent().Name.Split('\\');
				if (array.Length > 1)
				{
					empty = array[1];
				}
			}
			Parameter parameter = null;
			string empty2 = string.Empty;
			for (int i = 0; i < _ElementList.Count; i++)
			{
				if (_ElementList[i] == null)
				{
					continue;
				}
				parameter = _ElementList[i].get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
				if (parameter == null)
				{
					continue;
				}
				parameter = _ElementList[i].get_Parameter(BuiltInParameter.EDITED_BY);
				if (parameter == null)
				{
					continue;
				}
				empty2 = parameter.AsString();
				if (string.Compare(empty2, empty, ignoreCase: true) != 0)
				{
					if (empty2 != string.Empty)
					{
						list.Add(new KeyValuePair<string, Element>(empty2, _ElementList[i]));
					}
					else if (!File.Exists(ModelPathUtils.ConvertModelPathToUserVisiblePath(_Document.GetWorksharingCentralModelPath())))
					{
						RevitMessageBox.Show(string.Format(EPShared.LocalRM.GetString("CantObtainPermissionToEditElementMessage"), ModelPathUtils.ConvertModelPathToUserVisiblePath(_Document.GetWorksharingCentralModelPath())), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						return false;
					}
				}
			}
		}
		if (list != null && list.Count > 0)
		{
			list.Sort((KeyValuePair<string, Element> p1, KeyValuePair<string, Element> p2) => p1.Key.CompareTo(p2.Key));
			string text = string.Empty;
			TreeNode treeNode = null;
			TreeNode treeNode2 = null;
			TreeNode treeNode3 = new TreeNode(EPShared.LocalRM.GetString("Error"));
			for (int num = 0; num < list.Count; num++)
			{
				if (treeNode3.Nodes.Count == 0 || !treeNode3.Nodes[treeNode3.Nodes.Count - 1].Text.Contains(list[num].Key))
				{
					treeNode2 = new TreeNode(string.Format(EPShared.LocalRM.GetString("CantEditElementsUntilUserResavesMessage"), list[num].Key));
					treeNode3.Nodes.Add(treeNode2);
				}
				else
				{
					treeNode2 = treeNode3.Nodes[treeNode3.Nodes.Count - 1];
				}
				if (list[num].Value.Category != null)
				{
					text = list[num].Value.Category.Name + ": ";
				}
				text = text + list[num].Value.Name + ": " + EPShared.LocalRM.GetString("IDString") + " " + Convert.ToString(list[num].Value.Id.Value64());
				treeNode = new TreeNode(text);
				treeNode.Tag = list[num].Value;
				treeNode2.Nodes.Add(treeNode);
			}
			ErrorTreeListForm errorTreeListForm = new ErrorTreeListForm(_Document, EPShared.LocalRM.GetString("WorksetPermissionErrorDialog"), treeNode3);
			errorTreeListForm.ShowDialog();
			return false;
		}
		return true;
	}

	public static bool CreateTopographySurfaceUpdate(Document _Document, Eps_Leg _CurrentLeg, SitePlacementSettings _PlacementSettings, double _dArcPointInterval, List<Element> _LineElementList, ref TopographySurfaceContainer _ProposedSurface, ref FamilyInstance _TopoPlacementFamilyInstance, ref int _nTopoDrawOrder)
	{
		return CreateTopographySurfaceUpdate(_Document, _CurrentLeg, _PlacementSettings, _dArcPointInterval, _LineElementList, ref _ProposedSurface, ref _TopoPlacementFamilyInstance, ref _nTopoDrawOrder, null);
	}

	public static bool CreateTopographySurfaceUpdate(Document _Document, Eps_Leg _CurrentLeg, SitePlacementSettings _PlacementSettings, double _dArcPointInterval, List<Element> _LineElementList, ref TopographySurfaceContainer _ProposedSurface, ref FamilyInstance _TopoPlacementFamilyInstance, ref int _nTopoDrawOrder, List<Element> _VerifyElementList)
	{
		int nLineCount = 0;
		int nArcCount = 0;
		Eps_Line[] aLines = null;
		Eps_Arc[] aArcs = null;
		int[] aLegOrder = null;
		int nLineCount2 = 0;
		int nArcCount2 = 0;
		Eps_Line[] aLines2 = null;
		Eps_Arc[] aArcs2 = null;
		int[] aLegOrder2 = null;
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg ppOffsetLegList = null;
		Eps_Leg ppOffsetLegList2 = null;
		List<XYZ> PointXYZList = new List<XYZ>();
		List<XYZ> list = new List<XYZ>();
		List<Eps_Point> _AddPointList = new List<Eps_Point>();
		Parameter parameter = null;
		List<Eps_Point> ppPointList = new List<Eps_Point>();
		int num = 0;
		ProfileContainer profileContainer = null;
		IntersectionContainer intersectionContainer = null;
		FamilyInstanceComponentWrapper familyInstanceComponentWrapper = null;
		List<IntersectionContainer> _IntersectionContainers = new List<IntersectionContainer>();
		List<IntersectionStationContainer> _LeftIntersectionStations = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _RightIntersectionStations = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _CenterIntersectionStations = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> list2 = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> list3 = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> list4 = new List<IntersectionStationContainer>();
		List<OffsetLegWrapper> _OffsetLegWrappers = new List<OffsetLegWrapper>();
		List<Eps_Point> list5 = new List<Eps_Point>();
		List<Eps_Point> list6 = new List<Eps_Point>();
		List<Eps_Point> _CrownPointList = new List<Eps_Point>();
		FamilyInstance familyInstance = null;
		FamilySymbol familySymbol = null;
		double num2 = 0.0;
		double num3 = 0.0;
		Eps_Point eps_Point = null;
		bool flag = _PlacementSettings.IsRelativeElevation;
		double num4 = _PlacementSettings.Elevation;
		double num5 = _PlacementSettings.Grade;
		double num6 = 0.0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		double num7 = 3.0;
		double num8 = 3.0;
		double num9 = 15.0;
		double num10 = 3.0;
		double num11 = 0.0;
		double num12 = 0.0;
		double num13 = 0.0;
		double dAdjustSubgradeElev = 0.0;
		double dExistingAdjustSubgradeElev = 0.0;
		double num14 = 0.0;
		bool bIsCrownPresent = false;
		bool bIsProjectLegToTopo = false;
		bool bUseConstantElevation = !flag;
		double dStreetEdgeAdjustElev = 0.0;
		double num15 = 0.0;
		double num16 = 0.0;
		double dLeftSubgradeAdjustElev = 0.0;
		double dRightSubgradeAdjustElev = 0.0;
		double num17 = num4;
		double num18 = 0.0;
		double num19 = 0.0;
		double num20 = 0.0;
		double num21 = 0.0;
		double num22 = 0.0;
		List<Element> list7 = new List<Element>();
		if (_ProposedSurface.TopographySurface != null)
		{
			list7.Add(_ProposedSurface.TopographySurface);
		}
		if (_PlacementSettings.Type != null)
		{
			list7.Add(_PlacementSettings.Type);
		}
		if (_PlacementSettings.PlacementSymbol != null)
		{
			list7.Add(_PlacementSettings.PlacementSymbol);
		}
		if (_PlacementSettings.IntersectionPlacementSymbol != null)
		{
			list7.Add(_PlacementSettings.IntersectionPlacementSymbol);
		}
		if (_VerifyElementList != null && _VerifyElementList.Count > 0)
		{
			list7.AddRange(_VerifyElementList);
		}
		if (!VerifyUserCanManipulateElements(_Document, list7))
		{
			return false;
		}
		SetTopoDrawOrder(_Document, ref _nTopoDrawOrder);
		_PlacementSettings.TopoDrawOrder = _nTopoDrawOrder;
		int _nReturnValue = 0;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("MassCreationOnlyMode", ref _nReturnValue))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("MassCreationOnlyMode", 0);
		}
		if (_nReturnValue == 0)
		{
			list5 = _ProposedSurface.GetAllVertices();
		}
		List<XYZ> list8 = new List<XYZ>();
		list8.AddRange(_ProposedSurface.TopographySurface.GetBoundaryPoints().ToList());
		Eps_Leg originaltLeg = Eps_Leg.CopyLeg(_CurrentLeg);
		switch (_PlacementSettings.LocateType)
		{
		case LandcaddID.SoftTerrain:
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SoftTerrainCutSlopeParameter);
			if (parameter != null)
			{
				num7 = 1.0 / parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SoftTerrainFillSlopeParameter);
			if (parameter != null)
			{
				num8 = 1.0 / parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SoftTerrainLeftOffsetWidthParameter);
			if (parameter != null)
			{
				num11 = parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SoftTerrainRightOffsetWidthParameter);
			if (parameter != null)
			{
				num12 = parameter.AsDouble();
			}
			break;
		case LandcaddID.Sidewalk:
		{
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SideWalkCutSlopeParameter);
			if (parameter != null)
			{
				num7 = 1.0 / parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SideWalkFillSlopeParameter);
			if (parameter != null)
			{
				num8 = 1.0 / parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SideWalkWidthParameter);
			if (parameter != null)
			{
				double num25 = parameter.AsDouble();
				num11 = num25 / 2.0;
				num12 = num25 / 2.0;
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SideWalkPavementDepthParameter);
			if (parameter != null)
			{
				dAdjustSubgradeElev = 0.0 - parameter.AsDouble();
			}
			List<FamilyInstanceComponentWrapper> list9 = RevitUtility.FindSiteCategoryComponents(_Document, _PlacementSettings.TransactionContainer, Convert.ToInt32(LandcaddID.Sidewalk).ToString("000"));
			for (int i = 0; i < list9.Count; i++)
			{
				if (i == 0)
				{
					ppOffsetLegList = new Eps_Leg();
					RevitGeometry.OffsetLegList(_CurrentLeg, num12, 1, ref ppOffsetLegList, TrimClosedObject: true);
					ppOffsetLegList2 = new Eps_Leg();
					RevitGeometry.OffsetLegList(_CurrentLeg, num11, 2, ref ppOffsetLegList2, TrimClosedObject: true);
				}
				if (list9[i].InstanceLeg == null)
				{
					list9[i].CreateOriginalLeg(_ProposedSurface);
				}
				if (list9[i].InstanceLeg == null)
				{
					continue;
				}
				if (!EPShared.eplGeoE_ArePntsDuplicates2D(list9[i].InstanceLeg.LegStartPnt, _CurrentLeg.LegStartPnt, num11))
				{
					EPShared.eplGeoE_ArePntsDuplicates2D(list9[i].InstanceLeg.LegEndPnt, _CurrentLeg.LegStartPnt, num11);
				}
				if (!EPShared.eplGeoE_ArePntsDuplicates2D(list9[i].InstanceLeg.LegEndPnt, _CurrentLeg.LegEndPnt, num11))
				{
					EPShared.eplGeoE_ArePntsDuplicates2D(list9[i].InstanceLeg.LegStartPnt, _CurrentLeg.LegEndPnt, num11);
				}
				num = 0;
				if (RevitGeometry.eplGeoE_IntersectTwoLegLists(list9[i].InstanceLeg, _CurrentLeg, ref ppPointList, ref num, num11) != 0 || num <= 0)
				{
					continue;
				}
				familyInstance = list9[i].GetFirstExistingFamilyInstance();
				if (familyInstance == null)
				{
					continue;
				}
				familySymbol = RevitUtility.FindFamilyTypeSymbol(_Document, familyInstance, _PlacementSettings.LocateType);
				if (familySymbol == null)
				{
					continue;
				}
				list9[i].ProposedFamilySymbol = familySymbol;
				parameter = familySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
				if (parameter != null)
				{
					num14 = parameter.AsDouble();
				}
				if (num14 > 0.0)
				{
					double _dReturnValue = 0.0;
					double _dReturnValue2 = 0.0;
					bool flag3 = ApplicationUtility.Settings.eplSetE_GetSetting("SideWalkIntersectionDefaultRadius", ref _dReturnValue);
					bool flag4 = ApplicationUtility.Settings.eplSetE_GetSetting("SideWalkIntersectionDefaultTransitioningDistance", ref _dReturnValue2);
					if (!flag3 && !flag4)
					{
						_dReturnValue = 5.0;
					}
					familyInstanceComponentWrapper = list9[i];
					SetupIntersectionContainersForSiteComponent(_Document, _PlacementSettings.TransactionContainer, _ProposedSurface, list9, ref familyInstanceComponentWrapper);
					list9[i] = familyInstanceComponentWrapper;
					if (!FindIntersectionStationAndProfiles(_CurrentLeg, ppOffsetLegList2, ppOffsetLegList, list9[i].InstanceLeg, num14, 0.0, _dReturnValue, _dReturnValue2, _Document, _PlacementSettings.TransactionContainer, _PlacementSettings.LocateType, ref _LeftIntersectionStations, ref _CenterIntersectionStations, ref _RightIntersectionStations))
					{
						_nTopoDrawOrder = -1;
						return false;
					}
				}
				if (_LeftIntersectionStations.Count > 0 && _LeftIntersectionStations.Count == _RightIntersectionStations.Count)
				{
					for (int j = 0; j < _LeftIntersectionStations.Count; j++)
					{
						_LeftIntersectionStations[j].PrimaryPlacementId = familyInstance.UniqueId;
						_RightIntersectionStations[j].PrimaryPlacementId = familyInstance.UniqueId;
					}
				}
				list2.AddRange(_LeftIntersectionStations);
				list4.AddRange(_CenterIntersectionStations);
				list3.AddRange(_RightIntersectionStations);
				IntersectionStationContainerComparer comparer = new IntersectionStationContainerComparer(_bCompareFirstStations: true);
				if (list2 != null && list2.Count > 0)
				{
					list2.Sort(comparer);
				}
				if (list4 != null && list4.Count > 0)
				{
					list4.Sort(comparer);
				}
				if (list3 != null && list3.Count > 0)
				{
					list3.Sort(comparer);
				}
				_LeftIntersectionStations.Clear();
				_CenterIntersectionStations.Clear();
				_RightIntersectionStations.Clear();
			}
			break;
		}
		case LandcaddID.Curb:
			num19 = 0.0;
			num22 = 0.0;
			num18 = 0.0;
			num21 = 0.0;
			num20 = 0.0;
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetGutterWidthParameter);
			if (parameter != null)
			{
				num18 = parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCurbWidthParameter);
			if (parameter != null)
			{
				num19 = parameter.AsDouble();
			}
			if (_PlacementSettings.UseRightSide)
			{
				num12 = num18 + num19;
				num11 = 0.0;
			}
			else
			{
				num11 = num18 + num19;
				num12 = 0.0;
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCurbHeightParameter);
			if (parameter != null)
			{
				num21 = parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.CurbGutterHeightParameter);
			if (parameter != null)
			{
				num22 = parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetGutterSlopeParameter);
			if (parameter != null)
			{
				num20 = parameter.AsDouble();
			}
			if (num12 == 0.0)
			{
				dRightSubgradeAdjustElev = num22 + (num19 + num18) * num20;
				dLeftSubgradeAdjustElev = num22;
			}
			else if (num11 == 0.0)
			{
				dLeftSubgradeAdjustElev = num22 + (num19 + num18) * num20;
				dRightSubgradeAdjustElev = num22;
			}
			flag = true;
			bUseConstantElevation = false;
			num17 = 0.0;
			m_dCurbElevation = num21 - num18 * num20;
			_PlacementSettings.ProjectInnerSlopes = 5;
			break;
		case LandcaddID.RetainingWall:
		{
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.RetainingWallCutSlopeParameter);
			if (parameter != null)
			{
				num7 = 1.0 / parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.RetainingWallFillSlopeParameter);
			if (parameter != null)
			{
				num8 = 1.0 / parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.RetainingWallWidthParameter);
			if (parameter != null)
			{
				double num23 = parameter.AsDouble();
				num11 = num23 / 2.0;
				num12 = num23 / 2.0;
			}
			bool flag2 = false;
			double num24 = 0.0;
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.RetainingWallSlopeProjectionParameter);
			if (parameter != null)
			{
				flag2 = parameter.AsInteger() == 1;
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.RetainingWallHeightParameter);
			if (parameter != null)
			{
				num24 = parameter.AsDouble();
			}
			if (flag2)
			{
				if (_PlacementSettings.UseRightSide)
				{
					num16 = ((!flag) ? num24 : num4);
					_PlacementSettings.ProjectInnerSlopes = 3;
				}
				else
				{
					num15 = ((!flag) ? num24 : num4);
					_PlacementSettings.ProjectInnerSlopes = 2;
				}
			}
			else
			{
				_PlacementSettings.ProjectInnerSlopes = 4;
			}
			dLeftSubgradeAdjustElev = num15;
			dRightSubgradeAdjustElev = num16;
			bIsProjectLegToTopo = !flag;
			if (flag)
			{
				dAdjustSubgradeElev = 0.0 - (num24 - num4);
				num17 = 0.0;
			}
			else
			{
				num17 -= num24;
				num4 = num24;
			}
			break;
		}
		case LandcaddID.Street:
		{
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCutSlopeParameter);
			if (parameter != null)
			{
				num7 = 1.0 / parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetFillSlopeParameter);
			if (parameter != null)
			{
				num8 = 1.0 / parameter.AsDouble();
			}
			double num26 = 0.0;
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetLaneWidthParameter);
			if (parameter != null)
			{
				num26 = parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
			if (parameter != null)
			{
				num11 = num26 * (double)parameter.AsInteger() / 2.0;
				num12 = num26 * (double)parameter.AsInteger() / 2.0;
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
			if (parameter != null)
			{
				num15 = (0.0 - num11) * parameter.AsDouble();
				num16 = (0.0 - num12) * parameter.AsDouble();
				dStreetEdgeAdjustElev = (0.0 - (num11 + num12)) / 2.0 * parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCurbDisplayParameter);
			if (parameter != null && parameter.AsInteger() != 0)
			{
				num18 = 0.0;
				num20 = 0.0;
				num21 = 0.0;
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetGutterWidthParameter);
				if (parameter != null)
				{
					num18 = parameter.AsDouble();
					num11 += num18;
					num12 += num18;
					num3 = num18;
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCurbWidthParameter);
				if (parameter != null)
				{
					num11 += parameter.AsDouble();
					num12 += parameter.AsDouble();
					num3 += parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCurbHeightParameter);
				if (parameter != null)
				{
					num21 = parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetGutterSlopeParameter);
				if (parameter != null)
				{
					num20 = parameter.AsDouble();
				}
				num15 += num21 - num18 * num20;
				num16 += num21 - num18 * num20;
				dLeftSubgradeAdjustElev = num21;
				dRightSubgradeAdjustElev = num21;
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetPavementDepthParameter);
			if (parameter != null)
			{
				dAdjustSubgradeElev = 0.0 - parameter.AsDouble();
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetBoulevardWidthParameter);
			if (parameter != null)
			{
				num13 = parameter.AsDouble();
				num11 += num13;
				num12 += num13;
				num3 += num13;
			}
			bIsCrownPresent = true;
			List<FamilyInstanceComponentWrapper> list10 = RevitUtility.FindSiteCategoryComponents(_Document, _PlacementSettings.TransactionContainer, Convert.ToInt32(LandcaddID.Street).ToString("000"));
			for (int k = 0; k < list10.Count; k++)
			{
				if (k == 0)
				{
					ppOffsetLegList = new Eps_Leg();
					RevitGeometry.OffsetLegList(_CurrentLeg, num12 - num3, 1, ref ppOffsetLegList, TrimClosedObject: true);
					ppOffsetLegList2 = new Eps_Leg();
					RevitGeometry.OffsetLegList(_CurrentLeg, num11 - num3, 2, ref ppOffsetLegList2, TrimClosedObject: true);
				}
				if (list10[k].InstanceLeg == null)
				{
					list10[k].CreateOriginalLeg(_ProposedSurface);
				}
				if (list10[k].InstanceLeg == null)
				{
					continue;
				}
				if (!EPShared.eplGeoE_ArePntsDuplicates2D(list10[k].InstanceLeg.LegStartPnt, _CurrentLeg.LegStartPnt, num11))
				{
					EPShared.eplGeoE_ArePntsDuplicates2D(list10[k].InstanceLeg.LegEndPnt, _CurrentLeg.LegStartPnt, num11);
				}
				if (!EPShared.eplGeoE_ArePntsDuplicates2D(list10[k].InstanceLeg.LegEndPnt, _CurrentLeg.LegEndPnt, num11))
				{
					EPShared.eplGeoE_ArePntsDuplicates2D(list10[k].InstanceLeg.LegStartPnt, _CurrentLeg.LegEndPnt, num11);
				}
				num = 0;
				if (RevitGeometry.eplGeoE_IntersectTwoLegLists(list10[k].InstanceLeg, _CurrentLeg, ref ppPointList, ref num, num11) != 0 || num <= 0)
				{
					continue;
				}
				familyInstance = list10[k].GetFirstExistingFamilyInstance();
				if (familyInstance == null)
				{
					continue;
				}
				familySymbol = RevitUtility.FindFamilyTypeSymbol(_Document, familyInstance, _PlacementSettings.LocateType);
				if (familySymbol == null)
				{
					continue;
				}
				list10[k].ProposedFamilySymbol = familySymbol;
				num26 = 0.0;
				parameter = familySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
				if (parameter != null)
				{
					num26 = parameter.AsDouble();
				}
				parameter = familySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
				if (parameter != null)
				{
					num14 = num26 * (double)parameter.AsInteger();
				}
				parameter = familySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
				if (parameter != null)
				{
					dExistingAdjustSubgradeElev = (0.0 - num14) / 2.0 * parameter.AsDouble();
				}
				if (!(num14 > 0.0))
				{
					continue;
				}
				double _dReturnValue3 = 0.0;
				double _dReturnValue4 = 0.0;
				bool flag5 = ApplicationUtility.Settings.eplSetE_GetSetting("StreetIntersectionDefaultRadius", ref _dReturnValue3);
				bool flag6 = ApplicationUtility.Settings.eplSetE_GetSetting("StreetIntersectionDefaultTransitioningDistance", ref _dReturnValue4);
				if (!flag5 && !flag6)
				{
					_dReturnValue3 = 5.0;
				}
				familyInstanceComponentWrapper = list10[k];
				SetupIntersectionContainersForSiteComponent(_Document, _PlacementSettings.TransactionContainer, _ProposedSurface, list10, ref familyInstanceComponentWrapper);
				list10[k] = familyInstanceComponentWrapper;
				Eps_Leg ppOffsetLegList3 = new Eps_Leg();
				RevitGeometry.OffsetLegList(list10[k].InstanceLeg, num14 / 2.0, 1, ref ppOffsetLegList3, TrimClosedObject: true);
				Eps_Leg ppOffsetLegList4 = new Eps_Leg();
				RevitGeometry.OffsetLegList(list10[k].InstanceLeg, num14 / 2.0, 2, ref ppOffsetLegList4, TrimClosedObject: true);
				if (!FindIntersectionStationAndProfiles(_CurrentLeg, ppOffsetLegList2, ppOffsetLegList, list10[k].InstanceLeg, ppOffsetLegList4, ppOffsetLegList3, num14, dExistingAdjustSubgradeElev, _dReturnValue3, _dReturnValue4, _bFindIntersectionStations: true, _bDisplayPromptWhenNecessary: true, num3, _Document, _PlacementSettings.TransactionContainer, _PlacementSettings.LocateType, ref _LeftIntersectionStations, ref _CenterIntersectionStations, ref _RightIntersectionStations))
				{
					_nTopoDrawOrder = -1;
					return false;
				}
				List<Eps_Point> paPntList = new List<Eps_Point>();
				paPntList.AddRange(list5);
				Eps_Leg ppOffsetLegList5 = new Eps_Leg();
				RevitGeometry.OffsetLegList(list10[k].InstanceLeg, 0.01, 1, ref ppOffsetLegList5, TrimClosedObject: true);
				Eps_Leg ppOffsetLegList6 = new Eps_Leg();
				RevitGeometry.OffsetLegList(list10[k].InstanceLeg, 0.01, 2, ref ppOffsetLegList6, TrimClosedObject: true);
				List<Eps_Leg> list11 = new List<Eps_Leg>();
				list11.Add(ppOffsetLegList5);
				list11.Add(ppOffsetLegList6);
				Eps_Leg eps_Leg3 = FitCorrectLegListsAtGaps(list11);
				if (eps_Leg3 != null && paPntList.Count > 0)
				{
					RevitGeometry.GetAllPointsInBoundary(ref paPntList, eps_Leg3, null);
					list6.AddRange(paPntList);
				}
				int _nIntersectionSWStreetMassLastIndex = 0;
				parameter = familySymbol.LookupParameter(RevitParameters.StreetCurbDisplayParameter);
				string text = string.Empty;
				Parameter parameter2 = familyInstance.LookupParameter(RevitParameters.PrimaryFormIDFamilyParameter);
				if (parameter2 != null)
				{
					DirectShape directShape = RevitUtility.FindDirectShape(_Document, parameter2.AsString(), ref _nIntersectionSWStreetMassLastIndex);
					if (directShape != null)
					{
						text = directShape.Name;
						_Document.Delete(directShape.Id);
					}
					else
					{
						FamilyInstance familyInstance2 = RevitUtility.FindMassFamilyInstance(_Document, parameter2.AsString(), ref _nIntersectionSWStreetMassLastIndex);
						if (familyInstance2 != null && familyInstance2.Symbol != null && familyInstance2.Symbol.Family != null)
						{
							text = familyInstance2.Symbol.Family.Name;
							_Document.Delete(familyInstance2.Symbol.Family.Id);
						}
					}
				}
				Parameter parameter3 = familyInstance.LookupParameter(RevitParameters.AdditionalFormIDFamilyParameter);
				if (parameter3 != null)
				{
					DirectShape directShape2 = RevitUtility.FindDirectShape(_Document, parameter3.AsString());
					if (directShape2 != null)
					{
						if (text == string.Empty)
						{
							text = directShape2.Name;
						}
						_Document.Delete(directShape2.Id);
					}
					else
					{
						FamilyInstance familyInstance3 = RevitUtility.FindMassFamilyInstance(_Document, parameter3.AsString());
						if (familyInstance3 != null && familyInstance3.Symbol != null && familyInstance3.Symbol.Family != null)
						{
							if (text == string.Empty)
							{
								text = familyInstance3.Symbol.Family.Name;
							}
							_Document.Delete(familyInstance3.Symbol.Family.Id);
						}
					}
				}
				if (text == string.Empty)
				{
					_nIntersectionSWStreetMassLastIndex++;
					text = EPShared.LocalRM.GetString("IntersectionSWStreetMass") + " " + Convert.ToString(_nIntersectionSWStreetMassLastIndex);
				}
				IntersectionContainerComparer comparer2 = new IntersectionContainerComparer(_bCompareFirstStations: true);
				if (_LeftIntersectionStations.Count > 0 && _LeftIntersectionStations.Count == _RightIntersectionStations.Count)
				{
					bool flag7 = true;
					for (int l = 0; l < _LeftIntersectionStations.Count; l++)
					{
						_LeftIntersectionStations[l].PrimaryPlacementId = familyInstance.UniqueId;
						_RightIntersectionStations[l].PrimaryPlacementId = familyInstance.UniqueId;
						if (_LeftIntersectionStations[l].IntersectionStations.Count != _RightIntersectionStations[l].IntersectionStations.Count)
						{
							continue;
						}
						for (int m = 0; m < _LeftIntersectionStations[l].IntersectionStations.Count; m++)
						{
							intersectionContainer = new IntersectionContainer();
							intersectionContainer.StartingIntersection = true;
							profileContainer = new ProfileContainer();
							if (_LeftIntersectionStations[l].IntersectionStations[m].PrimaryStation <= _RightIntersectionStations[l].IntersectionStations[m].PrimaryStation)
							{
								flag7 = _LeftIntersectionStations[l].IntersectionStations[m].IsPrimaryLeft;
								eps_Point = GetIntersectionStationPoint(_LeftIntersectionStations, l, m, _bFindEndsOfIntersection: false);
								if ((eps_Point != null && flag7 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, ppOffsetLegList4, ref _dStation, ref _dOffset) == 1700) || (!flag7 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, ppOffsetLegList3, ref _dStation, ref _dOffset) == 1700))
								{
									profileContainer.Station = _dStation;
								}
								else
								{
									profileContainer.Station = _LeftIntersectionStations[l].IntersectionStations[m].PrimaryStation;
								}
							}
							else
							{
								flag7 = _RightIntersectionStations[l].IntersectionStations[m].IsPrimaryLeft;
								eps_Point = GetIntersectionStationPoint(_RightIntersectionStations, l, m, _bFindEndsOfIntersection: false);
								if ((eps_Point != null && flag7 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, ppOffsetLegList4, ref _dStation, ref _dOffset) == 1700) || (!flag7 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, ppOffsetLegList3, ref _dStation, ref _dOffset) == 1700))
								{
									profileContainer.Station = _dStation;
								}
								else
								{
									profileContainer.Station = _RightIntersectionStations[l].IntersectionStations[m].PrimaryStation;
								}
							}
							intersectionContainer.Profiles.Add(profileContainer);
							if (flag7)
							{
								intersectionContainer.ProfileUsage = ProfileUsage.LeftOnly;
							}
							else
							{
								intersectionContainer.ProfileUsage = ProfileUsage.RightOnly;
							}
							_IntersectionContainers.Add(intersectionContainer);
							intersectionContainer = new IntersectionContainer();
							intersectionContainer.StartingIntersection = false;
							profileContainer = new ProfileContainer();
							if (_LeftIntersectionStations[l].IntersectionStations[m].PrimaryStation <= _RightIntersectionStations[l].IntersectionStations[m].PrimaryStation)
							{
								flag7 = _RightIntersectionStations[l].IntersectionStations[m].IsPrimaryLeft;
								eps_Point = GetIntersectionStationPoint(_RightIntersectionStations, l, m, _bFindEndsOfIntersection: false);
								if ((eps_Point != null && flag7 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, ppOffsetLegList4, ref _dStation, ref _dOffset) == 1700) || (!flag7 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, ppOffsetLegList3, ref _dStation, ref _dOffset) == 1700))
								{
									profileContainer.Station = _dStation;
								}
								else
								{
									profileContainer.Station = _RightIntersectionStations[l].IntersectionStations[m].PrimaryStation;
								}
							}
							else
							{
								flag7 = _LeftIntersectionStations[l].IntersectionStations[m].IsPrimaryLeft;
								eps_Point = GetIntersectionStationPoint(_LeftIntersectionStations, l, m, _bFindEndsOfIntersection: false);
								if ((eps_Point != null && flag7 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, ppOffsetLegList4, ref _dStation, ref _dOffset) == 1700) || (!flag7 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, ppOffsetLegList3, ref _dStation, ref _dOffset) == 1700))
								{
									profileContainer.Station = _dStation;
								}
								else
								{
									profileContainer.Station = _LeftIntersectionStations[l].IntersectionStations[m].PrimaryStation;
								}
							}
							intersectionContainer.Profiles.Add(profileContainer);
							if (flag7)
							{
								intersectionContainer.ProfileUsage = ProfileUsage.LeftOnly;
							}
							else
							{
								intersectionContainer.ProfileUsage = ProfileUsage.RightOnly;
							}
							_IntersectionContainers.Add(intersectionContainer);
						}
					}
					if (_IntersectionContainers != null && _IntersectionContainers.Count > 0)
					{
						_IntersectionContainers.Sort(comparer2);
					}
				}
				if (_IntersectionContainers.Count > 0)
				{
					if (list10[k].IntersectionContainerList == null)
					{
						list10[k].IntersectionContainerList = new List<IntersectionContainer>();
					}
					list10[k].IntersectionContainerList.AddRange(_IntersectionContainers);
					_IntersectionContainers.Clear();
				}
				if (list10[k].IntersectionContainerList != null && list10[k].IntersectionContainerList.Count > 0)
				{
					list10[k].IntersectionContainerList.Sort(comparer2);
				}
				StreetForm streetForm = new StreetForm(familySymbol, list10[k].InstanceLeg);
				streetForm.IntersectionContainers = list10[k].IntersectionContainerList;
				string _strAdditionalLoftID = string.Empty;
				string text2 = streetForm.CreateForm(_Document, text, ref _strAdditionalLoftID);
				if (text2 != string.Empty)
				{
					parameter2?.Set(text2);
				}
				if (_strAdditionalLoftID != string.Empty)
				{
					parameter3?.Set(_strAdditionalLoftID);
				}
				list2.AddRange(_LeftIntersectionStations);
				list4.AddRange(_CenterIntersectionStations);
				list3.AddRange(_RightIntersectionStations);
				IntersectionStationContainerComparer comparer3 = new IntersectionStationContainerComparer(_bCompareFirstStations: true);
				if (list2 != null && list2.Count > 0)
				{
					list2.Sort(comparer3);
				}
				if (list4 != null && list4.Count > 0)
				{
					list4.Sort(comparer3);
				}
				if (list3 != null && list3.Count > 0)
				{
					list3.Sort(comparer3);
				}
				_LeftIntersectionStations.Clear();
				_CenterIntersectionStations.Clear();
				_RightIntersectionStations.Clear();
			}
			break;
		}
		case LandcaddID.ParkingLot:
			ppOffsetLegList = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentLeg, 0.02, 1, ref ppOffsetLegList, TrimClosedObject: true);
			ppOffsetLegList2 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentLeg, 0.02, 2, ref ppOffsetLegList2, TrimClosedObject: true);
			_CurrentLeg = ((!(RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2) > RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList))) ? ppOffsetLegList : ppOffsetLegList2);
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.ParkingLotPavementDepthParameter);
			if (parameter != null)
			{
				dLeftSubgradeAdjustElev = parameter.AsDouble();
				dRightSubgradeAdjustElev = parameter.AsDouble();
			}
			num7 = 0.01;
			num8 = 0.01;
			bIsProjectLegToTopo = true;
			_PlacementSettings.ProjectInnerSlopes = 1;
			break;
		}
		if (RevitDefines.EQ(num12 + num11, 0.0))
		{
			eps_Leg = Eps_Leg.CopyLeg(_CurrentLeg);
		}
		else
		{
			ppOffsetLegList = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentLeg, num12, 1, ref ppOffsetLegList, TrimClosedObject: true);
			ppOffsetLegList2 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentLeg, num11, 2, ref ppOffsetLegList2, TrimClosedObject: true);
			eps_Leg = Eps_Leg.CopyLeg(ppOffsetLegList.Reverse());
			eps_Leg2 = new Eps_Leg();
			eps_Leg2.Line = new Eps_Line();
			eps_Leg2.Line.StartPnt = ppOffsetLegList.LegEndPnt;
			eps_Leg2.Line.EndPnt = ppOffsetLegList2.LegEndPnt;
			eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
			eps_Leg.Last.Next = Eps_Leg.CopyLeg(ppOffsetLegList2);
			eps_Leg2 = new Eps_Leg();
			eps_Leg2.Line = new Eps_Line();
			eps_Leg2.Line.StartPnt = ppOffsetLegList2.LegStartPnt;
			eps_Leg2.Line.EndPnt = ppOffsetLegList.LegStartPnt;
			eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
		}
		if (eps_Leg != null && !HostLineFitsTerrain(_Document, ref _ProposedSurface, _CurrentLeg, eps_Leg, ref aLines2, ref nLineCount2, ref aArcs2, ref nArcCount2, ref aLegOrder2))
		{
			_nTopoDrawOrder = -1;
			return false;
		}
		List<double> list12 = new List<double>();
		List<Eps_Point> list13 = new List<Eps_Point>();
		Eps_Point eps_Point2 = null;
		if (list4.Count > 0)
		{
			for (int num27 = 0; num27 < list4.Count; num27++)
			{
				for (int num28 = 0; num28 < list4[num27].IntersectionStations.Count; num28++)
				{
					list12.Add(list4[num27].IntersectionStations[num28].Station);
					eps_Point2 = GetIntersectionStationPoint(list4, num27, num28, _bFindEndsOfIntersection: false);
					if (eps_Point2 != null)
					{
						list13.Add(eps_Point2);
					}
				}
			}
		}
		_CurrentLeg = _CurrentLeg.SplitLegAtStations(list12);
		if (_PlacementSettings.LocateType != LandcaddID.FeatureLine && _PlacementSettings.LocateType != LandcaddID.PointWipeout)
		{
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.LinearIncrementParameter);
			if (parameter != null)
			{
				num9 = parameter.AsDouble();
			}
			if (RevitDefines.LT(num9, 0.0833333))
			{
				num9 = 0.0833333;
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.CurveIncrementParameter);
			if (parameter != null)
			{
				num10 = RevitGeometry.RadiansToDecimalDegrees(EPShared.eplGeoE_NormalizeRadians(parameter.AsDouble()));
			}
			if (RevitDefines.LT(num10, 1.0))
			{
				num10 = 1.0;
			}
			_CurrentLeg.ConvertLegToLineArcArrays(ref aLines, ref nLineCount, ref aArcs, ref nArcCount, ref aLegOrder, _ProposedSurface, flag, bUseConstantElevation, num17, _bExtendLeg: true, list13);
		}
		else if (_PlacementSettings.LocateType == LandcaddID.FeatureLine)
		{
			num9 = _PlacementSettings.LinearPointInterval;
			if (RevitDefines.LT(num9, 0.0833333))
			{
				num9 = 0.0833333;
			}
		}
		if (_PlacementSettings.LocateType == LandcaddID.Curb)
		{
			_CurrentLeg = _CurrentLeg.ApplyElevationsToLeg(_ProposedSurface, flag, bUseConstantElevation, m_dCurbElevation);
		}
		else
		{
			_CurrentLeg = ((!RevitDefines.EQ(num5, 0.0)) ? _CurrentLeg.ApplyElevationsToLeg(num17, num5) : _CurrentLeg.ApplyElevationsToLeg(_ProposedSurface, flag, bUseConstantElevation, num17, list13));
			if (_PlacementSettings.LocateType == LandcaddID.FeatureLine && flag)
			{
				Eps_Point legStartPnt = _CurrentLeg.LegStartPnt;
				Eps_Point legEndPnt = _CurrentLeg.LegEndPnt;
				for (Eps_Leg eps_Leg4 = _CurrentLeg; eps_Leg4 != null; eps_Leg4 = eps_Leg4.Next)
				{
					legEndPnt = eps_Leg4.LegEndPnt;
				}
				num5 = (_PlacementSettings.Grade = (legEndPnt.z - legStartPnt.z) / RevitGeometry.eplGeoE_CalcLegListLength(_CurrentLeg));
			}
		}
		if (TerrainApi.UsesToposolids && !bUseConstantElevation)
		{
			DrapeLegOntoToposolid(_ProposedSurface, _CurrentLeg, flag ? num17 : 0.0);
		}
		if (bUseConstantElevation)
		{
			ApplyConstantElevationToHostGeometry(_CurrentLeg, aLines, nLineCount, aArcs, nArcCount, num17);
		}
		else if (TerrainApi.UsesToposolids && _PlacementSettings.LocateType != LandcaddID.FeatureLine && _PlacementSettings.LocateType != LandcaddID.PointWipeout)
		{
			_CurrentLeg.ConvertLegToLineArcArrays(ref aLines, ref nLineCount, ref aArcs, ref nArcCount, ref aLegOrder, _bExtendLeg: true);
		}
		RuntimeDiagnostics.Write("Host elevation relative=" + flag + " constant=" + bUseConstantElevation + " value=" + num17 + " firstZ=" + (_CurrentLeg?.LegStartPnt != null ? _CurrentLeg.LegStartPnt.z.ToString() : "n/a"));
		FindOffsetLegWrappers(originaltLeg, _CurrentLeg, list2, list4, list3, _PlacementSettings.LocateType, num11, num12, num3, dAdjustSubgradeElev, dStreetEdgeAdjustElev, ref _OffsetLegWrappers, ref _IntersectionContainers);
		if (list2.Count > 0 && list2.Count == list3.Count)
		{
			if (_PlacementSettings.IntersectionStations == null)
			{
				_PlacementSettings.IntersectionStations = new List<IntersectionStationContainer>();
			}
			_PlacementSettings.IntersectionStations.AddRange(list2);
			_PlacementSettings.IntersectionStations.AddRange(list3);
		}
		if (ApplicationUtility.UseMass && _PlacementSettings.LocateType != LandcaddID.SoftTerrain && _PlacementSettings.LocateType != LandcaddID.ParkingLot && _PlacementSettings.LocateType != LandcaddID.FeatureLine && _PlacementSettings.LocateType != LandcaddID.PointWipeout)
		{
			Category category = _Document.Settings.Categories.get_Item(BuiltInCategory.OST_Mass);
			if (category != null && !category.get_Visible(_Document.ActiveView))
			{
				List<string> list14 = new List<string>();
				list14.Add(EPShared.LocalRM.GetString("ShowMassInAllViewsOption"));
				list14.Add(EPShared.LocalRM.GetString("ShowMassInActiveViewOption"));
				WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("InformationString"), _Document.PathName, "MassVisibility", EPShared.LocalRM.GetString("PickViewOptionForMassMessage"), list14, -1, _bDisplayOKCancel: true);
				warningOptionsForm.ShowDialog();
				if (warningOptionsForm.DialogResult == DialogResult.OK && string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("ShowMassInAllViewsOption"), ignoreCase: true) == 0)
				{
					RevitUtility.ShowCategoryInAllViews(_Document, BuiltInCategory.OST_Mass);
				}
				else
				{
					try
					{
						category.set_Visible(_Document.ActiveView, true);
					}
					catch
					{
						ElementId viewTemplateId = _Document.ActiveView.ViewTemplateId;
						if (viewTemplateId != ElementId.InvalidElementId)
						{
							ViewPlan viewPlan = _Document.GetElement(viewTemplateId) as ViewPlan;
							List<ElementId> list15 = ElementTransformUtils.CopyElement(_Document, viewTemplateId, XYZ.Zero).ToList();
							if (list15.Count > 0)
							{
								ViewPlan viewPlan2 = _Document.GetElement(list15[0]) as ViewPlan;
								string text3 = viewPlan.Name + "-Site Designer";
								FilteredElementCollector source = new FilteredElementCollector(_Document).OfClass(typeof(Autodesk.Revit.DB.View));
								int num30 = 0;
								string testViewName = text3;
								while ((from Autodesk.Revit.DB.View f in source
									where f.IsTemplate && f.Name.Equals(testViewName)
									select f).Count() > 0)
								{
									num30++;
									testViewName = text3 + num30;
								}
								if (num30 > 0)
								{
									text3 += num30;
								}
								viewPlan2.Name = text3;
								_Document.ActiveView.ViewTemplateId = viewPlan2.Id;
								category.set_Visible((Autodesk.Revit.DB.View)viewPlan2, true);
							}
						}
					}
				}
			}
		}
		switch (_PlacementSettings.LocateType)
		{
		case LandcaddID.RetainingWall:
		{
			RetainingWallForm retainingWallForm = new RetainingWallForm(_PlacementSettings.Type, _CurrentLeg, num4);
			_PlacementSettings.LoftFormID = retainingWallForm.CreateForm(_Document, _PlacementSettings.Name);
			if (_PlacementSettings.LoftFormID == null || _PlacementSettings.LoftFormID == string.Empty)
			{
				_nTopoDrawOrder = -1;
				return false;
			}
			break;
		}
		case LandcaddID.Sidewalk:
		{
			SidewalkForm sidewalkForm = new SidewalkForm(_PlacementSettings.Type, _CurrentLeg);
			sidewalkForm.IntersectionContainers = _IntersectionContainers;
			_PlacementSettings.LoftFormID = sidewalkForm.CreateForm(_Document, _PlacementSettings.Name);
			if (_PlacementSettings.LoftFormID == null || _PlacementSettings.LoftFormID == string.Empty)
			{
				_nTopoDrawOrder = -1;
				return false;
			}
			break;
		}
		case LandcaddID.Street:
		{
			StreetForm streetForm2 = new StreetForm(_PlacementSettings.Type, _CurrentLeg);
			streetForm2.IntersectionContainers = _IntersectionContainers;
			string _strAdditionalLoftID2 = string.Empty;
			_PlacementSettings.LoftFormID = streetForm2.CreateForm(_Document, _PlacementSettings.Name, ref _strAdditionalLoftID2);
			_PlacementSettings.AdditionalLoftFormID = _strAdditionalLoftID2;
			if (_PlacementSettings.LoftFormID == null || _PlacementSettings.LoftFormID == string.Empty)
			{
				_nTopoDrawOrder = -1;
				return false;
			}
			break;
		}
		case LandcaddID.Curb:
		{
			CurbForm curbForm = new CurbForm(_PlacementSettings.Type, _CurrentLeg, _bAttachedToStreet: false, _PlacementSettings.UseRightSide);
			_PlacementSettings.LoftFormID = curbForm.CreateForm(_Document, _PlacementSettings.Name, _bUseInternalPoint: false, 0.0);
			if (_PlacementSettings.LoftFormID == null || _PlacementSettings.LoftFormID == string.Empty)
			{
				_nTopoDrawOrder = -1;
				return false;
			}
			break;
		}
		}
		if (_nReturnValue == 0)
		{
			ProgressBarForm progressBarForm = new ProgressBarForm();
			progressBarForm.ShowForm();
			progressBarForm.Refresh();
			progressBarForm.MinValue = 0;
			progressBarForm.MaxValue = 5;
			progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarGradingToposurfaceMessage");
			Eps_Leg _CatchLineLeg = null;
			Eps_Leg _FeatureLineLeg = null;
			Eps_Leg _OriginalBoundaryLeg = null;
			bool flag8 = false;
			if (list5.Count == 0)
			{
				Eps_Point pMinPnt = new Eps_Point();
				Eps_Point pMaxPnt = new Eps_Point();
				RevitGeometry.eplGeoE_FindMinMaxOfLineArcList(aLines, nLineCount, aArcs, nArcCount, ref pMinPnt, ref pMaxPnt);
				pMinPnt = new Eps_Point(pMinPnt.x, pMinPnt.y, RevitUtility.GetPlane(_Document.ActiveView.SketchPlane).Origin.Z);
				pMaxPnt = new Eps_Point(pMaxPnt.x, pMaxPnt.y, RevitUtility.GetPlane(_Document.ActiveView.SketchPlane).Origin.Z);
				double num31 = num11;
				if (num31 < num12)
				{
					num31 = num12;
				}
				double num32 = num7;
				if (num32 > num8)
				{
					num32 = num8;
				}
				double num33 = 100.0 / num32;
				double num34 = 0.0;
				for (int num35 = 0; num35 < nLineCount; num35++)
				{
					if (aLines[num35].StartPnt.z > num34)
					{
						num34 = aLines[num35].StartPnt.z;
					}
					if (aLines[num35].EndPnt.z > num34)
					{
						num34 = aLines[num35].EndPnt.z;
					}
				}
				for (int num36 = 0; num36 < nArcCount; num36++)
				{
					if (aArcs[num36].StartPnt.z > num34)
					{
						num34 = aArcs[num36].StartPnt.z;
					}
					if (aArcs[num36].EndPnt.z > num34)
					{
						num34 = aArcs[num36].EndPnt.z;
					}
				}
				double num37 = num34 * num33;
				num2 = 0.0;
				EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(pMinPnt, pMaxPnt, ref num2);
				EPShared.eplGeoE_CalcPolarToXY(pMinPnt, EPShared.eplGeoE_NormalizeRadians(num2 + Math.PI), num37 + num31 + 1.0, ref pMinPnt);
				EPShared.eplGeoE_CalcPolarToXY(pMaxPnt, num2, num37 + num31 + 1.0, ref pMaxPnt);
				list5.Add(pMinPnt);
				list5.Add(new Eps_Point(pMinPnt.x, pMaxPnt.y, RevitUtility.GetPlane(_Document.ActiveView.SketchPlane).Origin.Z));
				list5.Add(pMaxPnt);
				list5.Add(new Eps_Point(pMaxPnt.x, pMinPnt.y, RevitUtility.GetPlane(_Document.ActiveView.SketchPlane).Origin.Z));
			}
			else
			{
				flag8 = true;
			}
			if (_PlacementSettings.LocateType != LandcaddID.FeatureLine && _PlacementSettings.LocateType != LandcaddID.PointWipeout)
			{
				SDTimer sDTimer = new SDTimer("Cal_TIN", _Document.Application);
				sDTimer.Start();
				GetToposurfacePoints(_CurrentLeg, aLines, nLineCount, aArcs, nArcCount, aLegOrder, _OffsetLegWrappers, _Document.PathName, list5, num9, num10, _dArcPointInterval, num11, num12, num13, num7, num8, _PlacementSettings.ProjectInnerSlopes, dAdjustSubgradeElev, bIsCrownPresent, bIsProjectLegToTopo, num15, num16, dLeftSubgradeAdjustElev, dRightSubgradeAdjustElev, ref _AddPointList, ref _FeatureLineLeg, ref _CatchLineLeg, ref _OriginalBoundaryLeg, ref _CrownPointList, progressBarForm);
				sDTimer.Stop();
			}
			else if (_PlacementSettings.LocateType == LandcaddID.FeatureLine)
			{
				progressBarForm.UpdateProgressBar(1);
				num6 = 0.0;
				double num38 = 0.0;
				double num39 = 0.0;
				double num40 = 0.0;
				Eps_Point _StaOffPnt = null;
				for (Eps_Leg eps_Leg5 = _CurrentLeg; eps_Leg5 != null; eps_Leg5 = eps_Leg5.Next)
				{
					num39 = 0.0;
					if (eps_Leg5.Line != null)
					{
						EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Leg5.Line.StartPnt, eps_Leg5.Line.EndPnt, ref num6);
						for (; num39 < num6; num39 += num9)
						{
							EPShared.eplGeoE_FindStaOffPointAlongLine(eps_Leg5.Line, num39, 0.0, ref _StaOffPnt);
							num38 = num5 * (num40 + num39);
							_StaOffPnt.z = _CurrentLeg.LegStartPnt.z + num38;
							_AddPointList.Add(_StaOffPnt);
						}
					}
					else if (eps_Leg5.Arc != null)
					{
						for (num6 = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg5.Arc.Radius, eps_Leg5.Arc.Delta); num39 < num6; num39 += num9)
						{
							EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg5.Arc, num39, 0.0, ref _StaOffPnt);
							num38 = num5 * (num40 + num39);
							_StaOffPnt.z = _CurrentLeg.LegStartPnt.z + num38;
							_AddPointList.Add(_StaOffPnt);
						}
					}
					num40 += num6;
					if (eps_Leg5.Next == null)
					{
						_StaOffPnt = _CurrentLeg.LegEndPnt;
						num38 = num5 * num40;
						_StaOffPnt.z = _CurrentLeg.LegStartPnt.z + num38;
						_AddPointList.Add(_StaOffPnt);
					}
				}
			}
			progressBarForm.SetMaxValue();
			Thread.Sleep(300);
			progressBarForm.Close();
			progressBarForm = new ProgressBarForm();
			progressBarForm.ShowForm();
			progressBarForm.Refresh();
			progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarUpdatingModelMessage");
			progressBarForm.MinValue = 0;
			progressBarForm.MaxValue = 15;
			List<FamilyInstance> siteComponentPlacementFamilyInstances = GetSiteComponentPlacementFamilyInstances(_Document, _ProposedSurface.TopographySurface.UniqueId);
			List<Eps_Point> list16 = new List<Eps_Point>();
			_PlacementSettings.TransactionContainer.BeginTransaction();
			if (flag8)
			{
				if (_PlacementSettings.LocateType == LandcaddID.PointWipeout)
				{
					_CatchLineLeg = Eps_Leg.CopyLeg(_CurrentLeg);
				}
				TopographySurfaceContainer topographySurfaceContainer2 = _ProposedSurface;
				progressBarForm.UpdateProgressBar(3);
				List<Eps_Point> paPntList2 = topographySurfaceContainer2.GetAllVertices();
				RevitGeometry.RemoveAllPointsInBoundary(ref paPntList2, _FeatureLineLeg);
				RevitGeometry.RemoveAllPointsInBoundary(ref paPntList2, _CatchLineLeg);
				RevitGeometry.RemoveAllPointsInBoundary(ref paPntList2, _OriginalBoundaryLeg);
				if (_PlacementSettings.ProjectInnerSlopes == 1 || (TerrainApi.UsesToposolids && _PlacementSettings.ProjectInnerSlopes == 0))
				{
					RevitGeometry.RemoveAllPointsInBoundary(ref paPntList2, _CurrentLeg);
				}
				if (_AddPointList.Count > 0)
				{
					paPntList2.AddRange(_AddPointList);
				}
				if (_CrownPointList != null && _CrownPointList.Count > 0)
				{
					paPntList2.AddRange(_CrownPointList);
				}
				if (list6 != null && list6.Count > 0)
				{
					paPntList2.AddRange(list6);
				}
				progressBarForm.UpdateProgressBar(1);
				IEnumerable<Eps_Point> source2 = paPntList2;
				list16 = source2.Distinct(new PointEqualityComparer()).ToList();
				bool flag9 = false;
				bool flag10 = false;
				for (int num41 = 0; num41 < list16.Count; num41++)
				{
					if (list16[num41] != null)
					{
						for (int num42 = 0; !TerrainApi.UsesToposolids && num42 < topographySurfaceContainer2.SubregionBuildingPadList.Count; num42++)
						{
							if (topographySurfaceContainer2.SubregionBuildingPadList[num42] == null)
							{
								continue;
							}
							if (topographySurfaceContainer2.SubregionBuildingPadList[num42].BoundaryLeg != null && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(list16[num41], topographySurfaceContainer2.SubregionBuildingPadList[num42].BoundaryLeg, ref _dStation, ref _dOffset) == 1700 && Math.Abs(_dOffset) <= 0.001)
							{
								flag10 = true;
								break;
							}
							if (topographySurfaceContainer2.SubregionBuildingPadList[num42].GetSurfaceContainerContainingPoint(list16[num41]) != null)
							{
								if (topographySurfaceContainer2.SubregionBuildingPadList[num42].TopographySurfaceType == TopographySurfaceType.Subregion)
								{
									flag9 = true;
								}
								else
								{
									flag10 = true;
								}
								break;
							}
						}
						if (flag9)
						{
							list.Add(list16[num41].XYZ);
						}
						else if (!flag10)
						{
							PointXYZList.Add(list16[num41].XYZ);
						}
					}
					flag10 = false;
					flag9 = false;
				}
				progressBarForm.UpdateProgressBar(1);
				if (PointXYZList.Count > 0)
				{
					RemoveDuplicateXY(ref PointXYZList);
					RemoveOriginalTopoPoints(list8, ref PointXYZList);
					_PlacementSettings.TransactionContainer.EndTransaction();
					string name = _PlacementSettings.TransactionContainer.GetName();
					_PlacementSettings.TransactionContainer.Commit();
					IList<XYZ> list17 = null;
					list17 = _ProposedSurface.TopographySurface.GetInteriorPoints();
					SDTimer sDTimer2 = new SDTimer("StartTopographyEditScope", _Document.Application);
					sDTimer2.Start();
					_PlacementSettings.TransactionContainer.StartTopographyEditScope(name, _ProposedSurface.TopographySurface.Id);
					sDTimer2.Stop();
					SDTimer sDTimer3 = new SDTimer("Point_Update_Create", _Document.Application);
					sDTimer3.Start();
					try
					{
					SDTimer sDTimer4 = new SDTimer("Traverse_Point", _Document.Application);
					sDTimer4.Start();
					bool toposolidOverlay = TerrainApi.UsesToposolids;
					HashSet<XYZ> hashSet = new HashSet<XYZ>(list17, new XYZEqualityComparer());
					if (!toposolidOverlay)
					{
						for (int num43 = PointXYZList.Count - 1; num43 >= 0; num43--)
						{
							if (hashSet.Contains(PointXYZList[num43]))
							{
								hashSet.Remove(PointXYZList[num43]);
								PointXYZList.RemoveAt(num43);
							}
						}
					}
					sDTimer4.Stop();
					_PlacementSettings.TransactionContainer.Start("Update Topo Points");
					try
					{
						if (toposolidOverlay)
						{
							if (ReplacesInteriorPatch(_PlacementSettings.LocateType))
							{
								ApplyInteriorPointEdits(_ProposedSurface.TopographySurface, PointXYZList, replaceUnmatched: true);
							}
							else
							{
								RuntimeDiagnostics.Write("Skipped Toposolid mesh grading for " + _PlacementSettings.LocateType + "; the 3D road is placed on the existing top face.");
							}
						}
						else if (hashSet.Count <= PointXYZList.Count)
						{
							int num44 = 0;
							foreach (XYZ item in hashSet)
							{
								_ProposedSurface.TopographySurface.MovePoint(item, PointXYZList[num44]);
								num44++;
							}
							List<XYZ> list18 = new List<XYZ>();
							for (; num44 < PointXYZList.Count; num44++)
							{
								list18.Add(PointXYZList[num44]);
							}
							if (list18.Count > 0)
							{
								_ProposedSurface.TopographySurface.AddPoints(list18);
							}
						}
						else
						{
							int num45 = 0;
							int count = PointXYZList.Count;
							List<XYZ> list19 = new List<XYZ>();
							foreach (XYZ item2 in hashSet)
							{
								if (num45 < count)
								{
									_ProposedSurface.TopographySurface.MovePoint(item2, PointXYZList[num45]);
								}
								else
								{
									list19.Add(item2);
								}
								num45++;
							}
							if (list19.Count > 0)
							{
								_ProposedSurface.TopographySurface.DeletePoints(list19);
							}
						}
					}
					catch (Exception)
					{
						TaskDialog.Show("Exception", EPShared.LocalRM.GetString("RevitExceptionError"));
					}
					_PlacementSettings.TransactionContainer.Commit();
					sDTimer3.Stop();
					TopographyEditFailuresPreprocessor failuresPreprocessor = new TopographyEditFailuresPreprocessor();
					SDTimer sDTimer5 = new SDTimer("CommitTopographyEditScope", _Document.Application);
					sDTimer5.Start();
					_PlacementSettings.TransactionContainer.CommitTopographyEditScope(ref failuresPreprocessor);
					sDTimer5.Stop();
					}
					finally
					{
						_PlacementSettings.TransactionContainer.DisposeTopographyEditScope();
					}
					_PlacementSettings.TransactionContainer.Start(name);
					_PlacementSettings.TransactionContainer.BeginTransaction();
					_ProposedSurface.Refresh();
					_Document.Regenerate();
					AddToposolidBreaklines(_ProposedSurface.TopographySurface, _CurrentLeg, _CatchLineLeg, _FeatureLineLeg);
					progressBarForm.UpdateProgressBar(7);
					if (_TopoPlacementFamilyInstance != null)
					{
						parameter = _TopoPlacementFamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
						if (parameter != null && !parameter.IsReadOnly)
						{
							parameter.Set(_ProposedSurface.TopographySurface.UniqueId);
						}
					}
				}
				if (!TerrainApi.UsesToposolids && topographySurfaceContainer2.SubregionBuildingPadList != null && topographySurfaceContainer2.SubregionBuildingPadList.Count > 0)
				{
					List<TopographySurfaceContainer> surfaceList = FindAllMainSurfaces(_Document, _bFindMinMaxPoints: true);
					Eps_Point eps_Point3 = FindToposurfaceAdjustmentPoint(_ProposedSurface, surfaceList);
					ElementTransformUtils.MoveElement(_Document, _ProposedSurface.TopographySurface.Id, eps_Point3.XY);
					List<TopographySurfaceContainer> list20 = new List<TopographySurfaceContainer>();
					List<ElementId> list21 = new List<ElementId>();
					for (int num46 = 0; num46 < topographySurfaceContainer2.SubregionBuildingPadList.Count; num46++)
					{
						if (topographySurfaceContainer2.SubregionBuildingPadList[num46] == null)
						{
							continue;
						}
						if (topographySurfaceContainer2.SubregionBuildingPadList[num46].TopographySurface != null)
						{
							list21.Add(topographySurfaceContainer2.SubregionBuildingPadList[num46].TopographySurface.Id);
							if (topographySurfaceContainer2.SubregionBuildingPadList[num46].TopographySurfaceType == TopographySurfaceType.Subregion)
							{
								list20.Add(topographySurfaceContainer2.SubregionBuildingPadList[num46]);
							}
						}
						else if (topographySurfaceContainer2.SubregionBuildingPadList[num46].BuildingPad != null)
						{
							list21.Add(topographySurfaceContainer2.SubregionBuildingPadList[num46].BuildingPad.Id);
						}
					}
					List<KeyValuePair<FamilyInstance, TopographySurfaceContainer>> updateGenericModelList = FindPlantMixGenericModelList(_Document, list20);
					List<ElementId> newElementIds = null;
					try
					{
						newElementIds = LinearArray.ArrayElementsWithoutAssociation(_Document, _Document.ActiveView, list21.ToArray(), 2, eps_Point3.XY, ArrayAnchorMember.Second).ToList();
					}
					catch (Exception ex2)
					{
						if (ex2 is AccessViolationException || ex2 is System.InvalidOperationException)
						{
							_PlacementSettings.TransactionContainer.EndTransaction();
							progressBarForm.SetMaxValue();
							Thread.Sleep(300);
							progressBarForm.Close();
							RevitMessageBox.Show(EPShared.LocalRM.GetString("SubregionBuildingPadBoundaryErrorMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
							_nTopoDrawOrder = -1;
							return false;
						}
					}
					_Document.Delete(topographySurfaceContainer2.TopographySurface.Id);
					ElementTransformUtils.MoveElement(_Document, _ProposedSurface.TopographySurface.Id, new XYZ(0.0 - eps_Point3.x, 0.0 - eps_Point3.y, 0.0));
					if (list.Count > 0)
					{
						_ProposedSurface.TopographySurface.AddPoints(list);
					}
					List<TopographySurfaceContainer> list22 = null;
					list22 = FindNewTopographySurfaceContainers(_Document, newElementIds);
					UpdatePlantMixGenericModelList(updateGenericModelList, list22);
					if (list22 != null && list22.Count > 0)
					{
						_ProposedSurface.SubregionBuildingPadList.AddRange(list22);
					}
				}
				PointXYZList.Clear();
				progressBarForm.UpdateProgressBar(3);
				progressBarForm.SetMaxValue();
				Thread.Sleep(300);
				progressBarForm.Close();
			}
			else
			{
				IEnumerable<Eps_Point> source3 = _AddPointList;
				list16 = source3.Distinct(new PointEqualityComparer()).ToList();
				for (int num47 = 0; num47 < list16.Count; num47++)
				{
					PointXYZList.Add(_AddPointList[num47].XYZ);
				}
				progressBarForm.UpdateProgressBar(1);
				if (PointXYZList.Count > 0)
				{
					RemoveDuplicateXY(ref PointXYZList);
					_Document.Regenerate();
					progressBarForm.UpdateProgressBar(14);
				}
				progressBarForm.SetMaxValue();
				Thread.Sleep(300);
				progressBarForm.Close();
			}
			for (int num48 = 0; num48 < siteComponentPlacementFamilyInstances.Count; num48++)
			{
				FamilyInstance familyInstance4 = siteComponentPlacementFamilyInstances[num48];
				if (familyInstance4 == null)
				{
					continue;
				}
				LandcaddID landcaddID = LandcaddID.SoftTerrain;
				parameter = familyInstance4.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
				if (parameter != null)
				{
					foreach (int value in Enum.GetValues(typeof(LandcaddID)))
					{
						if (string.Compare(parameter.AsString(), value.ToString("000"), ignoreCase: true) == 0)
						{
							landcaddID = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value);
							break;
						}
					}
				}
				switch (landcaddID)
				{
				case LandcaddID.ParkingLot:
					familyInstance4.LookupParameter(RevitParameters.ParkingLotSurfaceIDFamilyParameter)?.Set(_ProposedSurface.TopographySurface.UniqueId);
					break;
				case LandcaddID.Curb:
					familyInstance4.LookupParameter(RevitParameters.ParentSurfaceIDFamilyParameter)?.Set(_ProposedSurface.TopographySurface.UniqueId);
					break;
				}
			}
			if (_PlacementSettings.LocateType == LandcaddID.Curb)
			{
				_PlacementSettings.ParentTopoSurfaceID = _ProposedSurface.TopographySurface.UniqueId;
			}
			if ((_PlacementSettings.LocateType == LandcaddID.Curb || _PlacementSettings.LocateType == LandcaddID.ParkingLot) && CanMakeClosedPad(_CurrentLeg))
			{
				List<Eps_Point> list23 = new List<Eps_Point>();
				if (_PlacementSettings.LocateType == LandcaddID.Curb)
				{
					if (TerrainApi.UsesToposolids)
					{
						_PlacementSettings.IslandSurfaceID = CreateClosedPavementOverlay(_Document, _PlacementSettings, _CurrentLeg, _ProposedSurface);
					}
					else
					{
						list23 = _CurrentLeg.GetPointsAtIntervals(num9, 1.0);
						TopographySurface topographySurface = CreateAdditionalTopoSurface(_Document, _PlacementSettings, list23, _ProposedSurface);
						if (topographySurface != null)
						{
							_PlacementSettings.IslandSurfaceID = topographySurface.UniqueId;
						}
					}
				}
				else if (_PlacementSettings.LocateType == LandcaddID.ParkingLot)
				{
					ppOffsetLegList = new Eps_Leg();
					RevitGeometry.OffsetLegList(_CurrentLeg, 0.02, 1, ref ppOffsetLegList, TrimClosedObject: true);
					ppOffsetLegList2 = new Eps_Leg();
					RevitGeometry.OffsetLegList(_CurrentLeg, 0.02, 2, ref ppOffsetLegList2, TrimClosedObject: true);
					_CurrentLeg = ((!(RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2) < RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList))) ? ppOffsetLegList : ppOffsetLegList2);
					if (TerrainApi.UsesToposolids)
					{
						_PlacementSettings.ParkingLotSurfaceID = CreateClosedPavementOverlay(_Document, _PlacementSettings, _CurrentLeg, _ProposedSurface);
					}
					else
					{
						list23 = GetParkingLotTopoSurfacePoints(_CurrentLeg, list16, num9, num10, _dArcPointInterval, _Document.PathName, _ProposedSurface);
						TopographySurface topographySurface2 = CreateAdditionalTopoSurface(_Document, _PlacementSettings, list23, _ProposedSurface);
						if (topographySurface2 != null)
						{
							_PlacementSettings.ParkingLotSurfaceID = topographySurface2.UniqueId;
						}
					}
				}
			}
		}
		_PlacementSettings.BeginningElevationList.Clear();
		_PlacementSettings.EndingElevationList.Clear();
		bool flag11 = false;
		bool flag12 = false;
		Eps_Point eps_Point4 = null;
		Eps_Point eps_Point5 = null;
		for (Eps_Leg eps_Leg6 = _CurrentLeg; eps_Leg6 != null; eps_Leg6 = eps_Leg6.Next)
		{
			if (eps_Leg6.Line != null)
			{
				eps_Point4 = eps_Leg6.Line.StartPnt;
				eps_Point5 = eps_Leg6.Line.EndPnt;
			}
			else if (eps_Leg6.Arc != null)
			{
				eps_Point4 = eps_Leg6.Arc.StartPnt;
				eps_Point5 = eps_Leg6.Arc.EndPnt;
			}
			for (int num49 = 0; num49 < list13.Count; num49++)
			{
				if (eps_Point4.IsEqual(list13[num49]))
				{
					flag11 = true;
					if (flag12)
					{
						break;
					}
				}
				else if (eps_Point5.IsEqual(list13[num49]))
				{
					flag12 = true;
					if (flag11)
					{
						break;
					}
				}
			}
			if (eps_Point4 != null && !flag11)
			{
				_PlacementSettings.BeginningElevationList.Add(eps_Point4.z);
			}
			if (eps_Point5 != null && !flag12)
			{
				_PlacementSettings.EndingElevationList.Add(eps_Point5.z);
			}
			flag11 = false;
			flag12 = false;
		}
		RevitUtility.CreatePlacementSiteComponent(_Document, _PlacementSettings, _LineElementList);
		_PlacementSettings.TransactionContainer.EndTransaction();
		return true;
	}

	public static void RemoveOriginalTopoPoints(List<XYZ> removePointList, ref List<XYZ> pointXYZList)
	{
		for (int i = 0; i < pointXYZList.Count - 1; i++)
		{
			Eps_Point eps_Point = new Eps_Point(pointXYZList[i]);
			foreach (XYZ removePoint in removePointList)
			{
				Eps_Point eps_Point2 = new Eps_Point(removePoint);
				if (eps_Point2.XYZ.IsAlmostEqualTo(eps_Point.XYZ))
				{
					pointXYZList.RemoveAt(i);
					i--;
				}
			}
		}
	}

	private static Eps_Point GetIntersectionStationPoint(List<IntersectionStationContainer> _IntersectionStations, int nIntIndex, int nStaIndex, bool _bFindEndsOfIntersection)
	{
		Eps_Line eps_Line = null;
		Eps_Arc eps_Arc = null;
		Eps_Point result = null;
		if (nIntIndex < 0 || nIntIndex >= _IntersectionStations.Count || nStaIndex < 0 || nStaIndex >= _IntersectionStations[nIntIndex].IntersectionStations.Count)
		{
			return result;
		}
		if (_IntersectionStations[nIntIndex].IntersectionStations[nStaIndex].IntersectionLeg != null)
		{
			if (_IntersectionStations[nIntIndex].IntersectionStations[nStaIndex].IntersectionLeg.Line != null)
			{
				eps_Line = _IntersectionStations[nIntIndex].IntersectionStations[nStaIndex].IntersectionLeg.Line;
				result = ((!_IntersectionStations[nIntIndex].IntersectionStations[nStaIndex].StartingIntersection) ? eps_Line.StartPnt : eps_Line.EndPnt);
			}
			else
			{
				eps_Arc = _IntersectionStations[nIntIndex].IntersectionStations[nStaIndex].IntersectionLeg.Arc;
				result = ((!_IntersectionStations[nIntIndex].IntersectionStations[nStaIndex].StartingIntersection) ? eps_Arc.StartPnt : eps_Arc.EndPnt);
			}
		}
		return result;
	}

	private static void FindOffsetLegWrappers(Eps_Leg _OriginaltLeg, Eps_Leg _CurrentLeg, List<IntersectionStationContainer> _LeftIntersectionStations, List<IntersectionStationContainer> _CenterIntersectionStations, List<IntersectionStationContainer> _RightIntersectionStations, LandcaddID _LocateType, double _dLeftOffsetWidth, double _dRightOffsetWidth, double _dAdditionalOffsetWidth, double _dAdjustSubgradeElev, double _dStreetEdgeAdjustElev, ref List<OffsetLegWrapper> _OffsetLegWrappers, ref List<IntersectionContainer> _IntersectionContainers)
	{
		Eps_Leg eps_Leg = null;
		Eps_Arc eps_Arc = null;
		Eps_Line eps_Line = null;
		Eps_Point eps_Point = null;
		Eps_Point Pnt = null;
		IntersectionContainer intersectionContainer = null;
		ProfileContainer profileContainer = null;
		List<Eps_Point> ppPointList = new List<Eps_Point>();
		int pNumberOfIntersections = 0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		double num = 0.0;
		double num2 = 0.0;
		int num3 = 10;
		if (_LeftIntersectionStations != null && _RightIntersectionStations != null && _CenterIntersectionStations != null && _LeftIntersectionStations.Count > 0 && _LeftIntersectionStations.Count == _RightIntersectionStations.Count && _LeftIntersectionStations.Count == _CenterIntersectionStations.Count)
		{
			Eps_Leg ppOffsetLegList = new Eps_Leg();
			RevitGeometry.OffsetLegList(_OriginaltLeg, _dRightOffsetWidth - _dAdditionalOffsetWidth, 1, ref ppOffsetLegList, TrimClosedObject: true);
			Eps_Leg ppOffsetLegList2 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_OriginaltLeg, _dLeftOffsetWidth - _dAdditionalOffsetWidth, 2, ref ppOffsetLegList2, TrimClosedObject: true);
			Eps_Leg ppOffsetLegList3 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentLeg, _dRightOffsetWidth - _dAdditionalOffsetWidth, 1, ref ppOffsetLegList3, TrimClosedObject: true);
			Eps_Leg ppOffsetLegList4 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentLeg, _dLeftOffsetWidth - _dAdditionalOffsetWidth, 2, ref ppOffsetLegList4, TrimClosedObject: true);
			for (int i = 0; i < _CenterIntersectionStations.Count; i++)
			{
				if (_LeftIntersectionStations[i].IntersectionStations.Count != _RightIntersectionStations[i].IntersectionStations.Count)
				{
					continue;
				}
				bool flag = false;
				List<double> list = new List<double>();
				List<double> list2 = new List<double>();
				List<double> list3 = new List<double>();
				List<Eps_Leg> list4 = new List<Eps_Leg>();
				List<Eps_Leg> list5 = new List<Eps_Leg>();
				for (int j = 0; j < _CenterIntersectionStations[i].IntersectionStations.Count; j++)
				{
					intersectionContainer = new IntersectionContainer();
					intersectionContainer.StartingIntersection = _CenterIntersectionStations[i].IntersectionStations[j].StartingIntersection;
					if (j == 0)
					{
						flag = intersectionContainer.StartingIntersection;
					}
					bool flag2 = false;
					double num4 = 0.0;
					if (_CenterIntersectionStations[i].IntersectionStations[j].IntersectionLeg != null)
					{
						if (_CenterIntersectionStations[i].IntersectionStations[j].IntersectionLeg.Line != null)
						{
							eps_Line = _CenterIntersectionStations[i].IntersectionStations[j].IntersectionLeg.Line;
							eps_Point = (intersectionContainer.StartingIntersection ? eps_Line.StartPnt : eps_Line.EndPnt);
							eps_Arc = null;
						}
						else
						{
							eps_Arc = _CenterIntersectionStations[i].IntersectionStations[j].IntersectionLeg.Arc;
							eps_Point = (intersectionContainer.StartingIntersection ? eps_Arc.StartPnt : eps_Arc.EndPnt);
							eps_Line = null;
						}
						Pnt = new Eps_Point();
						if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, _CurrentLeg, ref _dStation, ref _dOffset) == 1700 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_CurrentLeg, _dStation, _dOffset, ref Pnt) == 0)
						{
							flag2 = true;
							num4 = Pnt.z;
							if (eps_Line != null)
							{
								if (!intersectionContainer.StartingIntersection)
								{
									eps_Line.EndPnt.z = num4;
								}
								else
								{
									eps_Line.StartPnt.z = num4;
								}
							}
							if (eps_Arc != null)
							{
								if (!intersectionContainer.StartingIntersection)
								{
									eps_Arc.EndPnt.z = num4;
								}
								else
								{
									eps_Arc.StartPnt.z = num4;
								}
								eps_Arc.RadiusPnt.z = (eps_Arc.StartPnt.z + eps_Arc.EndPnt.z) / 2.0;
							}
						}
						list3.Add(_CenterIntersectionStations[i].IntersectionStations[j].Station);
					}
					if (_LeftIntersectionStations[i].IntersectionStations[j].IntersectionLeg != null)
					{
						if (_LeftIntersectionStations[i].IntersectionStations[j].IntersectionLeg.Line != null)
						{
							eps_Line = _LeftIntersectionStations[i].IntersectionStations[j].IntersectionLeg.Line;
							eps_Point = (intersectionContainer.StartingIntersection ? eps_Line.StartPnt : eps_Line.EndPnt);
							eps_Arc = null;
						}
						else
						{
							eps_Arc = _LeftIntersectionStations[i].IntersectionStations[j].IntersectionLeg.Arc;
							eps_Point = (intersectionContainer.StartingIntersection ? eps_Arc.StartPnt : eps_Arc.EndPnt);
							eps_Line = null;
						}
						if (!flag2 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, ppOffsetLegList4, ref _dStation, ref _dOffset) == 1700)
						{
							Pnt = new Eps_Point();
							if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(ppOffsetLegList4, _dStation, _dOffset, ref Pnt) == 0)
							{
								num4 = Pnt.z;
							}
						}
						if (eps_Line != null)
						{
							if (!intersectionContainer.StartingIntersection)
							{
								eps_Line.EndPnt.z = num4 + _dStreetEdgeAdjustElev;
							}
							else
							{
								eps_Line.StartPnt.z = num4 + _dStreetEdgeAdjustElev;
							}
						}
						if (eps_Arc != null)
						{
							if (!intersectionContainer.StartingIntersection)
							{
								eps_Arc.EndPnt.z = num4 + _dStreetEdgeAdjustElev;
							}
							else
							{
								eps_Arc.StartPnt.z = num4 + _dStreetEdgeAdjustElev;
							}
							eps_Arc.RadiusPnt.z = (eps_Arc.StartPnt.z + eps_Arc.EndPnt.z) / 2.0;
						}
						intersectionContainer.LeftOffsetLeg = _LeftIntersectionStations[i].IntersectionStations[j].IntersectionLeg;
						if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(intersectionContainer.LeftOffsetLeg.LegStartPnt, ppOffsetLegList2, ref _dStation, ref _dOffset, _bAlwaysSetStationOffset: true) == 1700)
						{
							list.Add(_dStation);
						}
						else if (_dStation < 0.0)
						{
							list.Add(0.0);
						}
						else if (_dStation > RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2))
						{
							list.Add(RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2));
						}
						list4.Add(intersectionContainer.LeftOffsetLeg.MoveLeg(0.0, 0.0, 0.0 - _dStreetEdgeAdjustElev));
						num2 = RevitGeometry.eplGeoE_CalcLegLength(_LeftIntersectionStations[i].IntersectionStations[j].IntersectionLeg);
						num = num2 / (double)(num3 - 1);
						for (int k = 0; k < num3; k++)
						{
							_dOffset = 0.0;
							if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_LeftIntersectionStations[i].IntersectionStations[j].IntersectionLeg, num * (double)k, _dOffset, ref Pnt) == 0)
							{
								profileContainer = new ProfileContainer();
								profileContainer.Points.Add(Pnt);
								profileContainer.Points.Add(new Eps_Point(Pnt.x, Pnt.y, Pnt.z + _dAdjustSubgradeElev));
								intersectionContainer.Profiles.Add(profileContainer);
							}
						}
					}
					if (_RightIntersectionStations[i].IntersectionStations[j].IntersectionLeg != null)
					{
						if (_RightIntersectionStations[i].IntersectionStations[j].IntersectionLeg.Line != null)
						{
							eps_Line = _RightIntersectionStations[i].IntersectionStations[j].IntersectionLeg.Line;
							eps_Point = (intersectionContainer.StartingIntersection ? eps_Line.StartPnt : eps_Line.EndPnt);
							eps_Arc = null;
						}
						else
						{
							eps_Arc = _RightIntersectionStations[i].IntersectionStations[j].IntersectionLeg.Arc;
							eps_Point = (intersectionContainer.StartingIntersection ? eps_Arc.StartPnt : eps_Arc.EndPnt);
							eps_Line = null;
						}
						if (!flag2 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, ppOffsetLegList3, ref _dStation, ref _dOffset) == 1700)
						{
							Pnt = new Eps_Point();
							if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(ppOffsetLegList3, _dStation, _dOffset, ref Pnt) == 0)
							{
								num4 = Pnt.z;
							}
						}
						if (eps_Line != null)
						{
							if (!intersectionContainer.StartingIntersection)
							{
								eps_Line.EndPnt.z = num4 + _dStreetEdgeAdjustElev;
							}
							else
							{
								eps_Line.StartPnt.z = num4 + _dStreetEdgeAdjustElev;
							}
						}
						if (eps_Arc != null)
						{
							if (!intersectionContainer.StartingIntersection)
							{
								eps_Arc.EndPnt.z = num4 + _dStreetEdgeAdjustElev;
							}
							else
							{
								eps_Arc.StartPnt.z = num4 + _dStreetEdgeAdjustElev;
							}
							eps_Arc.RadiusPnt.z = (eps_Arc.StartPnt.z + eps_Arc.EndPnt.z) / 2.0;
						}
						intersectionContainer.RightOffsetLeg = _RightIntersectionStations[i].IntersectionStations[j].IntersectionLeg;
						if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(intersectionContainer.RightOffsetLeg.LegStartPnt, ppOffsetLegList, ref _dStation, ref _dOffset, _bAlwaysSetStationOffset: true) == 1700)
						{
							list2.Add(_dStation);
						}
						else if (_dStation < 0.0)
						{
							list2.Add(0.0);
						}
						else if (_dStation > RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList))
						{
							list2.Add(RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList));
						}
						list5.Add(intersectionContainer.RightOffsetLeg.MoveLeg(0.0, 0.0, 0.0 - _dStreetEdgeAdjustElev));
						num2 = RevitGeometry.eplGeoE_CalcLegLength(_RightIntersectionStations[i].IntersectionStations[j].IntersectionLeg);
						num = num2 / (double)(num3 - 1);
						for (int l = 0; l < num3; l++)
						{
							_dOffset = 0.0;
							if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_RightIntersectionStations[i].IntersectionStations[j].IntersectionLeg, num * (double)l, _dOffset, ref Pnt) == 0)
							{
								intersectionContainer.Profiles[l].Points.Add(new Eps_Point(Pnt.x, Pnt.y, Pnt.z + _dAdjustSubgradeElev));
								intersectionContainer.Profiles[l].Points.Add(Pnt);
								Eps_Leg eps_Leg2 = new Eps_Leg();
								eps_Leg2.Line = new Eps_Line();
								eps_Leg2.Line.StartPnt = intersectionContainer.Profiles[l].Points[0];
								eps_Leg2.Line.EndPnt = intersectionContainer.Profiles[l].Points[intersectionContainer.Profiles[l].Points.Count - 1];
								if (RevitGeometry.eplGeoE_IntersectTwoLegLists(_CurrentLeg, eps_Leg2, ref ppPointList, ref pNumberOfIntersections, 1E-05) == 0 && pNumberOfIntersections > 0 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[0], _CurrentLeg, ref _dStation, ref _dOffset) == 1700)
								{
									intersectionContainer.Profiles[l].Station = _dStation;
								}
							}
						}
					}
					if (_LocateType == LandcaddID.Street && _CenterIntersectionStations[i].IntersectionStations[j].IntersectionLeg != null)
					{
						RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_CenterIntersectionStations[i].IntersectionStations[j].IntersectionLeg.LegStartPnt, _CurrentLeg, ref _dStation, ref _dOffset);
						for (int m = 0; m < intersectionContainer.Profiles.Count; m++)
						{
							_dOffset = 0.0;
							if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_CenterIntersectionStations[i].IntersectionStations[j].IntersectionLeg, intersectionContainer.Profiles[m].Station - _dStation, _dOffset, ref Pnt) == 0)
							{
								intersectionContainer.Profiles[m].Points.Insert(2, new Eps_Point(Pnt.x, Pnt.y, Pnt.z + _dAdjustSubgradeElev));
								intersectionContainer.Profiles[m].Points.Add(Pnt);
							}
						}
					}
					_IntersectionContainers.Add(intersectionContainer);
				}
				if (list4.Count > 0)
				{
					_OffsetLegWrappers = RevitGeometry.InsertLegsAtStations(ppOffsetLegList4, list, list4, list3, !flag);
					for (int n = 0; n < _OffsetLegWrappers.Count; n++)
					{
						eps_Leg = new Eps_Leg();
						RevitGeometry.OffsetLegList(_OffsetLegWrappers[n].OriginalLeg, _dAdditionalOffsetWidth, 2, ref eps_Leg, TrimClosedObject: true);
						_OffsetLegWrappers[n].OffsetLeg = eps_Leg;
						_OffsetLegWrappers[n].IsLeft = true;
						_OffsetLegWrappers[n].AdditionalOffsetWidth = _dAdditionalOffsetWidth;
					}
					_OffsetLegWrappers.Reverse();
				}
				if (list5.Count > 0)
				{
					List<OffsetLegWrapper> list6 = RevitGeometry.InsertLegsAtStations(ppOffsetLegList3, list2, list5, list3, !flag);
					for (int num5 = 0; num5 < list6.Count; num5++)
					{
						eps_Leg = new Eps_Leg();
						RevitGeometry.OffsetLegList(list6[num5].OriginalLeg, _dAdditionalOffsetWidth, 1, ref eps_Leg, TrimClosedObject: true);
						list6[num5].OffsetLeg = eps_Leg;
						list6[num5].IsLeft = false;
						list6[num5].AdditionalOffsetWidth = _dAdditionalOffsetWidth;
					}
					_OffsetLegWrappers.AddRange(list6);
				}
			}
		}
		IntersectionContainerComparer comparer = new IntersectionContainerComparer(_bCompareFirstStations: true);
		if (_IntersectionContainers != null && _IntersectionContainers.Count > 0)
		{
			_IntersectionContainers.Sort(comparer);
		}
	}

	private static void GetToposurfacePoints(Eps_Leg _CurrentLeg, Eps_Line[] aLines, int nLineCount, Eps_Arc[] aArcs, int nArcCount, int[] aLegOrder, List<OffsetLegWrapper> _OffsetLegWrappers, string _strPathName, List<Eps_Point> TopoPointList, double dLinearInterval, double dRadialInterval, double _dArcPointInterval, double dLeftOffsetWidth, double dRightOffsetWidth, double dBoulevardWidth, double dCutSlope, double dFillSlope, int _nProjectInnerSlopes, double dAdjustSubgradeElev, bool bIsCrownPresent, bool bIsProjectLegToTopo, double dLeftEdgeAdjustElev, double dRightEdgeAdjustElev, double dLeftSubgradeAdjustElev, double dRightSubgradeAdjustElev, ref List<Eps_Point> _AddPointList, ref Eps_Leg _FeatureLineLeg, ref Eps_Leg _CatchLineLeg, ref Eps_Leg _OriginalBoundaryLeg, ref List<Eps_Point> _CrownPointList, ProgressBarForm _ProgressFrm)
	{
		List<Eps_Leg> list = new List<Eps_Leg>();
		int num = 0;
		Eps_Point[] aPnts = TopoPointList.ToArray();
		int count = TopoPointList.Count;
		_CatchLineLeg = null;
		if (_OffsetLegWrappers == null || _OffsetLegWrappers.Count == 0)
		{
			Eps_Point[] _aCutFillPoints = null;
			int _nNumCutFillPoints = 0;
			Eps_Line[] _aLeftCatchLines = null;
			int[] array = null;
			int _nNumLeftCatchLines = 0;
			Eps_Line[] _aRightCatchLines = null;
			int[] array2 = null;
			int _nNumRightCatchLines = 0;
			Eps_Line[] _aLeftCutFillLines = null;
			int[] array3 = null;
			int _nNumLeftCutFillLines = 0;
			Eps_Line[] _aRightCutFillLines = null;
			int[] array4 = null;
			int _nNumRightCutFillLines = 0;
			EPSharedExternal.CreateCutAndFillFromTopo(_strPathName, aLines, nLineCount, aArcs, nArcCount, aLegOrder, aPnts, count, dLinearInterval, dRadialInterval, _dArcPointInterval, dLeftOffsetWidth, dRightOffsetWidth, dBoulevardWidth, dCutSlope, dFillSlope, _nProjectInnerSlopes, dAdjustSubgradeElev, bIsCrownPresent, bIsProjectLegToTopo, dLeftEdgeAdjustElev, dRightEdgeAdjustElev, dLeftSubgradeAdjustElev, dRightSubgradeAdjustElev, ref _aCutFillPoints, ref _nNumCutFillPoints, ref _aLeftCatchLines, ref _nNumLeftCatchLines, ref _aRightCatchLines, ref _nNumRightCatchLines, ref _aLeftCutFillLines, ref _nNumLeftCutFillLines, ref _aRightCutFillLines, ref _nNumRightCutFillLines);
			_ProgressFrm?.UpdateProgressBar(1);
			if (_aCutFillPoints != null)
			{
				_AddPointList = _aCutFillPoints.ToList();
			}
			array = new int[_nNumLeftCatchLines];
			for (num = 0; num < _nNumLeftCatchLines; num++)
			{
				if (num == 0)
				{
					_AddPointList.Add(_aLeftCatchLines[num].StartPnt);
				}
				_AddPointList.Add(_aLeftCatchLines[num].EndPnt);
				array[num] = 0;
			}
			_ProgressFrm?.UpdateProgressBar(1);
			array2 = new int[_nNumRightCatchLines];
			for (num = 0; num < _nNumRightCatchLines; num++)
			{
				if (num == 0)
				{
					_AddPointList.Add(_aRightCatchLines[num].StartPnt);
				}
				_AddPointList.Add(_aRightCatchLines[num].EndPnt);
				array2[num] = 0;
			}
			_ProgressFrm?.UpdateProgressBar(1);
			array3 = new int[_nNumLeftCutFillLines];
			for (num = 0; num < _nNumLeftCutFillLines; num++)
			{
				if (num == 0)
				{
					_AddPointList.Add(_aLeftCutFillLines[num].StartPnt);
				}
				_AddPointList.Add(_aLeftCutFillLines[num].EndPnt);
				array3[num] = 0;
			}
			_ProgressFrm?.UpdateProgressBar(1);
			array4 = new int[_nNumRightCutFillLines];
			for (num = 0; num < _nNumRightCutFillLines; num++)
			{
				if (num == 0)
				{
					_AddPointList.Add(_aRightCutFillLines[num].StartPnt);
				}
				_AddPointList.Add(_aRightCutFillLines[num].EndPnt);
				array4[num] = 0;
			}
			_ProgressFrm?.UpdateProgressBar(1);
			Eps_Leg leftCatchLinesLeg = Eps_Leg.ConvertLineArcArraysToLeg(_aLeftCatchLines, _nNumLeftCatchLines, new Eps_Arc[0], 0, array);
			Eps_Leg rightCatchLinesLeg = Eps_Leg.ConvertLineArcArraysToLeg(_aRightCatchLines, _nNumRightCatchLines, new Eps_Arc[0], 0, array2);
			Eps_Leg eps_Leg = Eps_Leg.ConvertLineArcArraysToLeg(_aLeftCutFillLines, _nNumLeftCutFillLines, new Eps_Arc[0], 0, array3);
			list.Add(eps_Leg);
			Eps_Leg eps_Leg2 = Eps_Leg.ConvertLineArcArraysToLeg(_aRightCutFillLines, _nNumRightCutFillLines, new Eps_Arc[0], 0, array4);
			list.Add(eps_Leg2);
			_CatchLineLeg = FitCorrectLegListsAtGaps(leftCatchLinesLeg, rightCatchLinesLeg, eps_Leg, eps_Leg2, _CurrentLeg.IsLegClosed());
			if (bIsCrownPresent)
			{
				List<Eps_Point> paPntList = new List<Eps_Point>();
				paPntList.AddRange(_aCutFillPoints);
				Eps_Leg ppOffsetLegList = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentLeg, 0.01, 1, ref ppOffsetLegList, TrimClosedObject: true);
				Eps_Leg ppOffsetLegList2 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentLeg, 0.01, 2, ref ppOffsetLegList2, TrimClosedObject: true);
				List<Eps_Leg> list2 = new List<Eps_Leg>();
				list2.Add(ppOffsetLegList);
				list2.Add(ppOffsetLegList2);
				Eps_Leg eps_Leg3 = FitCorrectLegListsAtGaps(list2);
				if (eps_Leg3 != null && paPntList.Count > 0)
				{
					RevitGeometry.GetAllPointsInBoundary(ref paPntList, eps_Leg3, null);
					_CrownPointList.AddRange(paPntList);
				}
			}
		}
		else
		{
			Eps_Point eps_Point = null;
			Eps_Leg eps_Leg4 = null;
			Eps_Leg eps_Leg5 = null;
			Eps_Leg eps_Leg6 = null;
			Eps_Leg eps_Leg7 = null;
			Eps_Leg eps_Leg8 = null;
			Eps_Leg ppOffsetLegList3 = null;
			Eps_Leg eps_Leg9 = null;
			List<Eps_Leg> list3 = new List<Eps_Leg>();
			List<Eps_Leg> list4 = new List<Eps_Leg>();
			Eps_Line[] array5 = null;
			int[] array6 = null;
			int num2 = 0;
			Eps_Line[] array7 = null;
			int[] array8 = null;
			int num3 = 0;
			double _dStation = 0.0;
			double _dOffset = 0.0;
			Eps_Line[] array9 = null;
			Eps_Arc[] array10 = null;
			int[] array11 = null;
			int num4 = 0;
			int num5 = 0;
			for (int i = 0; i < _OffsetLegWrappers.Count; i++)
			{
				array9 = null;
				array10 = null;
				array11 = null;
				num4 = 0;
				num5 = 0;
				eps_Leg8 = new Eps_Leg();
				if (_OffsetLegWrappers[i].IsLeft)
				{
					eps_Leg6 = _OffsetLegWrappers[i].OffsetLeg.MoveLeg(0.0, 0.0, dLeftEdgeAdjustElev);
					eps_Leg7 = _OffsetLegWrappers[i].OriginalLeg.MoveLeg(0.0, 0.0, dLeftEdgeAdjustElev);
					if (dBoulevardWidth > 0.0)
					{
						ppOffsetLegList3 = new Eps_Leg();
						RevitGeometry.OffsetLegList(eps_Leg7, _OffsetLegWrappers[i].AdditionalOffsetWidth - dBoulevardWidth, 2, ref ppOffsetLegList3, TrimClosedObject: true);
						RevitGeometry.OffsetLegList(ppOffsetLegList3, 0.01, 1, ref eps_Leg8, TrimClosedObject: true);
					}
					else
					{
						RevitGeometry.OffsetLegList(eps_Leg6, 0.01, 1, ref eps_Leg8, TrimClosedObject: true);
					}
					eps_Leg8 = eps_Leg8.MoveLeg(0.0, 0.0, dAdjustSubgradeElev - dLeftSubgradeAdjustElev);
				}
				else
				{
					eps_Leg6 = _OffsetLegWrappers[i].OffsetLeg.MoveLeg(0.0, 0.0, dRightEdgeAdjustElev);
					eps_Leg7 = _OffsetLegWrappers[i].OriginalLeg.MoveLeg(0.0, 0.0, dRightEdgeAdjustElev);
					if (dBoulevardWidth > 0.0)
					{
						ppOffsetLegList3 = new Eps_Leg();
						RevitGeometry.OffsetLegList(eps_Leg7, _OffsetLegWrappers[i].AdditionalOffsetWidth - dBoulevardWidth, 1, ref ppOffsetLegList3, TrimClosedObject: true);
						RevitGeometry.OffsetLegList(ppOffsetLegList3, 0.01, 2, ref eps_Leg8, TrimClosedObject: true);
					}
					else
					{
						RevitGeometry.OffsetLegList(eps_Leg6, 0.01, 2, ref eps_Leg8, TrimClosedObject: true);
					}
					eps_Leg8 = eps_Leg8.MoveLeg(0.0, 0.0, dAdjustSubgradeElev - dRightSubgradeAdjustElev);
				}
				eps_Leg8 = eps_Leg8.ExtendLegList(_bStart: true, -0.005);
				eps_Leg8 = eps_Leg8.ExtendLegList(_bStart: false, -0.005);
				eps_Leg6.ConvertLegToLineArcArrays(ref array9, ref num4, ref array10, ref num5, ref array11, _bExtendLeg: false);
				array5 = null;
				num2 = 0;
				array7 = null;
				num3 = 0;
				EPSharedExternal.CreateCatchLinesAndFeatureLinesFromTopo(_strPathName, array9, num4, array10, num5, array11, aPnts, count, dLinearInterval, dRadialInterval, _dArcPointInterval, dCutSlope, dFillSlope, _OffsetLegWrappers[i].IsLeft, ref array5, ref num2, ref array7, ref num3);
				_ProgressFrm?.UpdateProgressBar(1);
				if (_AddPointList == null)
				{
					_AddPointList = new List<Eps_Point>();
				}
				array6 = new int[num2];
				for (num = 0; num < num2; num++)
				{
					if (num == 0)
					{
						_AddPointList.Add(array5[num].StartPnt);
					}
					_AddPointList.Add(array5[num].EndPnt);
					array6[num] = 0;
				}
				_ProgressFrm?.UpdateProgressBar(1);
				array8 = new int[num3];
				for (num = 0; num < num3; num++)
				{
					if (num == 0)
					{
						_AddPointList.Add(array7[num].StartPnt);
					}
					_AddPointList.Add(array7[num].EndPnt);
					array8[num] = 0;
				}
				_ProgressFrm?.UpdateProgressBar(1);
				Eps_Leg item = Eps_Leg.ConvertLineArcArraysToLeg(array5, num2, new Eps_Arc[0], 0, array6);
				list3.Add(item);
				if (bIsCrownPresent)
				{
					if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_CurrentLeg.LegStartPnt, _CurrentLeg, ref _dStation, ref _dOffset) == 1700 && _dStation >= _OffsetLegWrappers[i].Station && _dStation <= _OffsetLegWrappers[i].EndingStation)
					{
						_AddPointList.Add(_CurrentLeg.LegStartPnt);
						eps_Point = _CurrentLeg.ExtendLegList(_bStart: true, -0.005).LegStartPnt;
						eps_Point.z += dAdjustSubgradeElev;
						_AddPointList.Add(eps_Point);
						_CrownPointList.Add(eps_Point);
					}
					if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_CurrentLeg.LegEndPnt, _CurrentLeg, ref _dStation, ref _dOffset) == 1700 && _dStation >= _OffsetLegWrappers[i].Station && _dStation <= _OffsetLegWrappers[i].EndingStation)
					{
						_AddPointList.Add(_CurrentLeg.LegEndPnt);
						eps_Point = _CurrentLeg.ExtendLegList(_bStart: false, -0.005).LegEndPnt;
						eps_Point.z += dAdjustSubgradeElev;
						_AddPointList.Add(eps_Point);
						_CrownPointList.Add(eps_Point);
					}
				}
				Eps_Leg _CurrentLeg2 = Eps_Leg.ConvertLineArcArraysToLeg(array7, num3, new Eps_Arc[0], 0, array8);
				if (_CurrentLeg2 != null)
				{
					for (eps_Leg4 = _CurrentLeg2; eps_Leg4 != null; eps_Leg4 = eps_Leg4.Next)
					{
						if (eps_Leg4.Line != null || eps_Leg4.Arc != null)
						{
							if (eps_Leg4.LegStartPnt != null)
							{
								_AddPointList.Add(eps_Leg4.LegStartPnt);
								if (eps_Leg8 != null)
								{
									eps_Point = new Eps_Point();
									if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg4.LegStartPnt, eps_Leg8, ref _dStation, ref _dOffset) == 1700 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(eps_Leg8, _dStation, 0.0, ref eps_Point) == 0)
									{
										_AddPointList.Add(eps_Point);
									}
								}
								if (bIsCrownPresent)
								{
									eps_Point = new Eps_Point();
									if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg4.LegStartPnt, _CurrentLeg, ref _dStation, ref _dOffset) == 1700 && _dStation >= _OffsetLegWrappers[i].Station && _dStation <= _OffsetLegWrappers[i].EndingStation && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_CurrentLeg, _dStation, 0.0, ref eps_Point) == 0)
									{
										eps_Point.z += dAdjustSubgradeElev;
										_AddPointList.Add(eps_Point);
										_CrownPointList.Add(eps_Point);
									}
								}
								if (dBoulevardWidth > 0.0 && ppOffsetLegList3 != null)
								{
									eps_Point = new Eps_Point();
									if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg4.LegStartPnt, ppOffsetLegList3, ref _dStation, ref _dOffset) == 1700 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(ppOffsetLegList3, _dStation, 0.0, ref eps_Point) == 0)
									{
										_AddPointList.Add(eps_Point);
									}
								}
							}
							if (eps_Leg4.LegEndPnt != null && eps_Leg4.Next == null)
							{
								_AddPointList.Add(eps_Leg4.LegEndPnt);
								if (eps_Leg8 != null)
								{
									eps_Point = new Eps_Point();
									if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg4.LegEndPnt, eps_Leg8, ref _dStation, ref _dOffset) == 1700 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(eps_Leg8, _dStation, 0.0, ref eps_Point) == 0)
									{
										_AddPointList.Add(eps_Point);
									}
								}
								if (bIsCrownPresent)
								{
									eps_Point = new Eps_Point();
									if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg4.LegEndPnt, _CurrentLeg, ref _dStation, ref _dOffset) == 1700 && _dStation >= _OffsetLegWrappers[i].Station && _dStation <= _OffsetLegWrappers[i].EndingStation && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_CurrentLeg, _dStation, 0.0, ref eps_Point) == 0)
									{
										eps_Point.z += dAdjustSubgradeElev;
										_AddPointList.Add(eps_Point);
										_CrownPointList.Add(eps_Point);
									}
								}
								if (dBoulevardWidth > 0.0 && ppOffsetLegList3 != null)
								{
									eps_Point = new Eps_Point();
									if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg4.LegEndPnt, ppOffsetLegList3, ref _dStation, ref _dOffset) == 1700 && RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(ppOffsetLegList3, _dStation, 0.0, ref eps_Point) == 0)
									{
										_AddPointList.Add(eps_Point);
									}
								}
							}
						}
					}
					if (_OffsetLegWrappers[i].AdditionalOffsetWidth > 0.0)
					{
						eps_Leg5 = new Eps_Leg();
						if (_OffsetLegWrappers[i].IsLeft)
						{
							RevitGeometry.OffsetLegList(_CurrentLeg2, _OffsetLegWrappers[i].AdditionalOffsetWidth, 1, ref eps_Leg5, TrimClosedObject: true);
						}
						else
						{
							RevitGeometry.OffsetLegList(_CurrentLeg2, _OffsetLegWrappers[i].AdditionalOffsetWidth, 2, ref eps_Leg5, TrimClosedObject: true);
						}
						eps_Leg9 = new Eps_Leg();
						eps_Leg9.Line = new Eps_Line();
						eps_Leg9.Line.StartPnt = eps_Leg5.LegStartPnt;
						eps_Leg9.Line.EndPnt = _CurrentLeg2.LegStartPnt;
						Eps_Leg.InsertLeg(ref _CurrentLeg2, eps_Leg9);
						eps_Leg9 = new Eps_Leg();
						eps_Leg9.Line = new Eps_Line();
						eps_Leg9.Line.StartPnt = eps_Leg5.LegEndPnt;
						eps_Leg9.Line.EndPnt = _CurrentLeg2.LegEndPnt;
						Eps_Leg.InsertLeg(ref _CurrentLeg2, eps_Leg9);
					}
					list.Add(_CurrentLeg2);
				}
				if (eps_Leg7 == null)
				{
					continue;
				}
				Eps_Leg _CurrentLeg3 = Eps_Leg.CopyLeg(eps_Leg7);
				if (_OffsetLegWrappers[i].AdditionalOffsetWidth - dBoulevardWidth > 0.0)
				{
					if (dBoulevardWidth > 0.0 && ppOffsetLegList3 != null)
					{
						_CurrentLeg3 = Eps_Leg.CopyLeg(ppOffsetLegList3);
						if (_CurrentLeg3.Arc != null && _OffsetLegWrappers[i].OffsetLeg.Arc == null)
						{
							eps_Leg4 = Eps_Leg.SingleLeg(_CurrentLeg3);
							_AddPointList.AddRange(eps_Leg4.GetPointsAtIntervals(_dArcPointInterval, _dArcPointInterval));
							if (eps_Leg8.Arc != null)
							{
								eps_Leg4 = Eps_Leg.SingleLeg(eps_Leg8);
								_AddPointList.AddRange(eps_Leg4.GetPointsAtIntervals(_dArcPointInterval, _dArcPointInterval));
							}
						}
						if (_CurrentLeg3.Last.Arc != null && _OffsetLegWrappers[i].OffsetLeg.Last.Arc == null)
						{
							_AddPointList.AddRange(_CurrentLeg3.Last.GetPointsAtIntervals(_dArcPointInterval, _dArcPointInterval));
							if (eps_Leg8.Last.Arc != null)
							{
								_AddPointList.AddRange(eps_Leg8.Last.GetPointsAtIntervals(_dArcPointInterval, _dArcPointInterval));
							}
						}
					}
					else
					{
						_CurrentLeg3 = Eps_Leg.CopyLeg(eps_Leg6);
					}
					eps_Leg9 = new Eps_Leg();
					eps_Leg9.Line = new Eps_Line();
					eps_Leg9.Line.StartPnt = eps_Leg7.LegStartPnt;
					eps_Leg9.Line.EndPnt = _CurrentLeg3.LegStartPnt;
					Eps_Leg.InsertLeg(ref _CurrentLeg3, eps_Leg9);
					eps_Leg9 = new Eps_Leg();
					eps_Leg9.Line = new Eps_Line();
					eps_Leg9.Line.StartPnt = eps_Leg7.LegEndPnt;
					eps_Leg9.Line.EndPnt = _CurrentLeg3.LegEndPnt;
					Eps_Leg.InsertLeg(ref _CurrentLeg3, eps_Leg9);
				}
				list4.Add(_CurrentLeg3);
			}
			if (list3.Count > 0)
			{
				_CatchLineLeg = FitCorrectLegListsAtGaps(list3);
			}
			if (list4.Count > 0)
			{
				_OriginalBoundaryLeg = FitCorrectLegListsAtGaps(list4);
			}
		}
		if (list.Count > 0)
		{
			_FeatureLineLeg = FitCorrectLegListsAtGaps(list);
		}
	}

	private static void TrimOrExtendOffsetLegs(bool _IsStart, Eps_Leg _LeftExistingOffsetLeg, Eps_Leg _RightExistingOffsetLeg, double _dExistingOffset, ref Eps_Leg _OffsetLeg)
	{
		double _dOffset = 0.0;
		double _dOffset2 = 0.0;
		double _dStation = 0.0;
		double _dStation2 = 0.0;
		Eps_Leg eps_Leg = null;
		Eps_Point eps_Point = null;
		List<Eps_Point> ppPointList = new List<Eps_Point>();
		int pNumberOfIntersections = 0;
		eps_Point = ((!_IsStart) ? _OffsetLeg.LegEndPnt : _OffsetLeg.LegStartPnt);
		if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, _LeftExistingOffsetLeg, ref _dStation, ref _dOffset) != 1700 || RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, _RightExistingOffsetLeg, ref _dStation2, ref _dOffset2) != 1700)
		{
			return;
		}
		if (_dOffset > 0.0 && _dOffset2 >= 0.0)
		{
			eps_Leg = ((!_IsStart) ? _OffsetLeg.Last : Eps_Leg.SingleLeg(_OffsetLeg));
			if (RevitGeometry.eplGeoE_IntersectTwoLegLists(eps_Leg, _LeftExistingOffsetLeg, ref ppPointList, ref pNumberOfIntersections, 1E-05) == 0 && pNumberOfIntersections > 0)
			{
				if (_IsStart)
				{
					RevitGeometry.TrimStartOfLeg(ref _OffsetLeg, ppPointList[0]);
				}
				else
				{
					RevitGeometry.TrimEndOfLeg(ref _OffsetLeg, ppPointList[pNumberOfIntersections - 1]);
				}
				return;
			}
			while (_dOffset2 > 0.0)
			{
				_OffsetLeg = _OffsetLeg.ExtendLegList(_IsStart, _dExistingOffset);
				eps_Point = ((!_IsStart) ? _OffsetLeg.LegEndPnt : _OffsetLeg.LegStartPnt);
				if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, _RightExistingOffsetLeg, ref _dStation2, ref _dOffset2) != 1700 || !(_dOffset2 < 0.0))
				{
					continue;
				}
				eps_Leg = ((!_IsStart) ? _OffsetLeg.Last : Eps_Leg.SingleLeg(_OffsetLeg));
				if (RevitGeometry.eplGeoE_IntersectTwoLegLists(eps_Leg, _RightExistingOffsetLeg, ref ppPointList, ref pNumberOfIntersections, 1E-05) == 0 && pNumberOfIntersections > 0)
				{
					if (_IsStart)
					{
						RevitGeometry.TrimStartOfLeg(ref _OffsetLeg, ppPointList[0]);
					}
					else
					{
						RevitGeometry.TrimEndOfLeg(ref _OffsetLeg, ppPointList[pNumberOfIntersections - 1]);
					}
				}
			}
		}
		else
		{
			if (!(_dOffset <= 0.0) || !(_dOffset2 < 0.0))
			{
				return;
			}
			eps_Leg = ((!_IsStart) ? _OffsetLeg.Last : Eps_Leg.SingleLeg(_OffsetLeg));
			if (RevitGeometry.eplGeoE_IntersectTwoLegLists(eps_Leg, _RightExistingOffsetLeg, ref ppPointList, ref pNumberOfIntersections, 1E-05) == 0 && pNumberOfIntersections > 0)
			{
				if (_IsStart)
				{
					RevitGeometry.TrimStartOfLeg(ref _OffsetLeg, ppPointList[0]);
				}
				else
				{
					RevitGeometry.TrimEndOfLeg(ref _OffsetLeg, ppPointList[pNumberOfIntersections - 1]);
				}
				return;
			}
			while (_dOffset < 0.0)
			{
				_OffsetLeg = _OffsetLeg.ExtendLegList(_IsStart, _dExistingOffset);
				eps_Point = ((!_IsStart) ? _OffsetLeg.LegEndPnt : _OffsetLeg.LegStartPnt);
				if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, _LeftExistingOffsetLeg, ref _dStation, ref _dOffset) != 1700 || !(_dOffset > 0.0))
				{
					continue;
				}
				eps_Leg = ((!_IsStart) ? _OffsetLeg.Last : Eps_Leg.SingleLeg(_OffsetLeg));
				if (RevitGeometry.eplGeoE_IntersectTwoLegLists(eps_Leg, _LeftExistingOffsetLeg, ref ppPointList, ref pNumberOfIntersections, 1E-05) == 0 && pNumberOfIntersections > 0)
				{
					if (_IsStart)
					{
						RevitGeometry.TrimStartOfLeg(ref _OffsetLeg, ppPointList[0]);
					}
					else
					{
						RevitGeometry.TrimEndOfLeg(ref _OffsetLeg, ppPointList[pNumberOfIntersections - 1]);
					}
				}
			}
		}
	}

	private static bool UpdateRadiusAndTransitionalDistanceForIntersectionStations(List<IntersectionStationContainer> _PrevIntersectionStations, ref List<IntersectionStationContainer> _IntersectionStations)
	{
		double num = double.MaxValue;
		double num2 = 0.0;
		int num3 = -1;
		bool result = false;
		if (_PrevIntersectionStations.Count != _IntersectionStations.Count)
		{
			return false;
		}
		for (int i = 0; i < _PrevIntersectionStations.Count; i++)
		{
			if (_PrevIntersectionStations[i].IntersectionStations.Count != _IntersectionStations[i].IntersectionStations.Count)
			{
				return false;
			}
			for (int j = 0; j < _PrevIntersectionStations[i].IntersectionStations.Count; j++)
			{
				for (int k = 0; k < _IntersectionStations[i].IntersectionStations.Count; k++)
				{
					if (_PrevIntersectionStations[i].IntersectionStations[j].IsPrimaryLeft == _IntersectionStations[i].IntersectionStations[k].IsPrimaryLeft && _PrevIntersectionStations[i].IntersectionStations[j].IsLeft == _IntersectionStations[i].IntersectionStations[k].IsLeft)
					{
						num2 = Math.Abs(_PrevIntersectionStations[i].IntersectionStations[j].Station - _IntersectionStations[i].IntersectionStations[k].Station);
						if (num2 < num)
						{
							num = num2;
							num3 = k;
						}
						num2 = Math.Abs(_PrevIntersectionStations[i].IntersectionStations[j].PrimaryStation - _IntersectionStations[i].IntersectionStations[k].PrimaryStation);
						if (num2 < num)
						{
							num = num2;
							num3 = k;
						}
						if (num2 == 0.0)
						{
							break;
						}
					}
				}
				if (num3 >= 0 && num3 < _IntersectionStations[i].IntersectionStations.Count)
				{
					result = true;
					_IntersectionStations[i].IntersectionStations[num3].Radius = _PrevIntersectionStations[i].IntersectionStations[j].Radius;
					_IntersectionStations[i].IntersectionStations[num3].TransitioningDistance = _PrevIntersectionStations[i].IntersectionStations[j].TransitioningDistance;
				}
				num = double.MaxValue;
				num3 = -1;
			}
		}
		return result;
	}

	private static bool FindIntersectionStationAndProfiles(Eps_Leg _CurrentLeg, Eps_Leg _LeftOffsetLeg, Eps_Leg _RightOffsetLeg, Eps_Leg _ExistingOffsetLeg, double _dExistingOffset, double _dExistingAdjustSubgradeElev, double _dIntersectionRadius, double _dIntersectionTransitioningDistance, Document _Document, EPTransactionContainer _TransactionContainer, LandcaddID _LocateType, ref List<IntersectionStationContainer> _LeftIntersectionStations, ref List<IntersectionStationContainer> _CenterIntersectionStations, ref List<IntersectionStationContainer> _RightIntersectionStations)
	{
		Eps_Leg ppOffsetLegList = new Eps_Leg();
		RevitGeometry.OffsetLegList(_ExistingOffsetLeg, _dExistingOffset / 2.0, 1, ref ppOffsetLegList, TrimClosedObject: true);
		Eps_Leg ppOffsetLegList2 = new Eps_Leg();
		RevitGeometry.OffsetLegList(_ExistingOffsetLeg, _dExistingOffset / 2.0, 2, ref ppOffsetLegList2, TrimClosedObject: true);
		return FindIntersectionStationAndProfiles(_CurrentLeg, _LeftOffsetLeg, _RightOffsetLeg, _ExistingOffsetLeg, ppOffsetLegList2, ppOffsetLegList, _dExistingOffset, _dExistingAdjustSubgradeElev, _dIntersectionRadius, _dIntersectionTransitioningDistance, _bFindIntersectionStations: true, _bDisplayPromptWhenNecessary: true, 0.0, _Document, _TransactionContainer, _LocateType, ref _LeftIntersectionStations, ref _CenterIntersectionStations, ref _RightIntersectionStations);
	}

	private static bool FindIntersectionStationAndProfiles(Eps_Leg _CurrentLeg, Eps_Leg _LeftOffsetLeg, Eps_Leg _RightOffsetLeg, Eps_Leg _ExistingOffsetLeg, Eps_Leg _LeftExistingOffsetLeg, Eps_Leg _RightExistingOffsetLeg, double _dExistingOffset, double _dExistingAdjustSubgradeElev, double _dIntersectionRadius, double _dIntersectionTransitioningDistance, bool _bFindIntersectionStations, bool _bDisplayPromptWhenNecessary, double _dMinimumRadius, Document _Document, EPTransactionContainer _TransactionContainer, LandcaddID _LocateType, ref List<IntersectionStationContainer> _LeftIntersectionStations, ref List<IntersectionStationContainer> _CenterIntersectionStations, ref List<IntersectionStationContainer> _RightIntersectionStations)
	{
		List<Eps_Point> ppPointList = new List<Eps_Point>();
		int pNumberOfIntersections = 0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		bool flag = true;
		double _dOffset2 = 0.0;
		double _dOffset3 = 0.0;
		if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_CurrentLeg.LegStartPnt, _LeftExistingOffsetLeg, ref _dStation, ref _dOffset2) == 1700 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_CurrentLeg.LegStartPnt, _RightExistingOffsetLeg, ref _dStation, ref _dOffset3) == 1700 && RevitDefines.GE(_dOffset2, 0.0) && RevitDefines.LE(_dOffset3, 0.0))
		{
			flag = false;
		}
		if (!flag)
		{
			TrimOrExtendOffsetLegs(_IsStart: true, _LeftExistingOffsetLeg, _RightExistingOffsetLeg, _dExistingOffset, ref _LeftOffsetLeg);
			TrimOrExtendOffsetLegs(_IsStart: true, _LeftExistingOffsetLeg, _RightExistingOffsetLeg, _dExistingOffset, ref _RightOffsetLeg);
		}
		bool flag2 = true;
		if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_CurrentLeg.LegEndPnt, _LeftExistingOffsetLeg, ref _dStation, ref _dOffset2) == 1700 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_CurrentLeg.LegEndPnt, _RightExistingOffsetLeg, ref _dStation, ref _dOffset3) == 1700 && RevitDefines.GE(_dOffset2, 0.0) && RevitDefines.LE(_dOffset3, 0.0))
		{
			flag2 = false;
		}
		if (!flag2)
		{
			TrimOrExtendOffsetLegs(_IsStart: false, _LeftExistingOffsetLeg, _RightExistingOffsetLeg, _dExistingOffset, ref _LeftOffsetLeg);
			TrimOrExtendOffsetLegs(_IsStart: false, _LeftExistingOffsetLeg, _RightExistingOffsetLeg, _dExistingOffset, ref _RightOffsetLeg);
		}
		if (_bFindIntersectionStations)
		{
			List<IntersectionStationContainer> list = new List<IntersectionStationContainer>();
			List<IntersectionStationContainer> list2 = new List<IntersectionStationContainer>();
			list.AddRange(_LeftIntersectionStations);
			list2.AddRange(_RightIntersectionStations);
			_LeftIntersectionStations.Clear();
			_RightIntersectionStations.Clear();
			if (RevitGeometry.eplGeoE_IntersectTwoLegLists(_LeftOffsetLeg, _LeftExistingOffsetLeg, ref ppPointList, ref pNumberOfIntersections, 1E-05) == 0 && pNumberOfIntersections > 0)
			{
				for (int i = 0; i < pNumberOfIntersections; i++)
				{
					RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[i], _LeftOffsetLeg, ref _dStation, ref _dOffset, _bAlwaysSetStationOffset: true);
					int j = 0;
					int k = 0;
					bool flag3 = false;
					if (_LeftIntersectionStations.Count == 0)
					{
						_LeftIntersectionStations.Add(new IntersectionStationContainer(_dStation));
					}
					else
					{
						for (j = 0; j < _LeftIntersectionStations.Count; j++)
						{
							for (k = 0; k < _LeftIntersectionStations[j].IntersectionStations.Count; k++)
							{
								if (_dStation < _LeftIntersectionStations[j].IntersectionStations[k].Station)
								{
									_LeftIntersectionStations[j].IntersectionStations.Insert(k, new IntersectionStation(_dStation));
									flag3 = true;
									break;
								}
							}
							if (flag3)
							{
								break;
							}
							if (k == _LeftIntersectionStations[j].IntersectionStations.Count)
							{
								_LeftIntersectionStations[j].IntersectionStations.Add(new IntersectionStation(_dStation));
								break;
							}
						}
					}
					if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[i], _LeftExistingOffsetLeg, ref _dStation, ref _dOffset) == 1700)
					{
						_LeftIntersectionStations[j].IntersectionStations[k].IsPrimaryLeft = true;
						_LeftIntersectionStations[j].IntersectionStations[k].PrimaryStation = _dStation;
					}
					_LeftIntersectionStations[j].IntersectionStations[k].IsLeft = true;
					_LeftIntersectionStations[j].IntersectionStations[k].Radius = _dIntersectionRadius;
					_LeftIntersectionStations[j].IntersectionStations[k].TransitioningDistance = _dIntersectionTransitioningDistance;
				}
			}
			if (RevitGeometry.eplGeoE_IntersectTwoLegLists(_LeftOffsetLeg, _RightExistingOffsetLeg, ref ppPointList, ref pNumberOfIntersections, 1E-05) == 0 && pNumberOfIntersections > 0)
			{
				for (int l = 0; l < pNumberOfIntersections; l++)
				{
					RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[l], _LeftOffsetLeg, ref _dStation, ref _dOffset);
					int m = 0;
					int n = 0;
					bool flag4 = false;
					if (_LeftIntersectionStations.Count == 0)
					{
						_LeftIntersectionStations.Add(new IntersectionStationContainer(_dStation));
					}
					else
					{
						for (m = 0; m < _LeftIntersectionStations.Count; m++)
						{
							for (n = 0; n < _LeftIntersectionStations[m].IntersectionStations.Count; n++)
							{
								if (_dStation < _LeftIntersectionStations[m].IntersectionStations[n].Station)
								{
									_LeftIntersectionStations[m].IntersectionStations.Insert(n, new IntersectionStation(_dStation));
									flag4 = true;
									break;
								}
							}
							if (flag4)
							{
								break;
							}
							if (n == _LeftIntersectionStations[m].IntersectionStations.Count)
							{
								_LeftIntersectionStations[m].IntersectionStations.Add(new IntersectionStation(_dStation));
								break;
							}
						}
					}
					if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[l], _RightExistingOffsetLeg, ref _dStation, ref _dOffset) == 1700)
					{
						_LeftIntersectionStations[m].IntersectionStations[n].IsPrimaryLeft = false;
						_LeftIntersectionStations[m].IntersectionStations[n].PrimaryStation = _dStation;
					}
					_LeftIntersectionStations[m].IntersectionStations[n].IsLeft = true;
					_LeftIntersectionStations[m].IntersectionStations[n].Radius = _dIntersectionRadius;
					_LeftIntersectionStations[m].IntersectionStations[n].TransitioningDistance = _dIntersectionTransitioningDistance;
				}
			}
			if (RevitGeometry.eplGeoE_IntersectTwoLegLists(_RightOffsetLeg, _LeftExistingOffsetLeg, ref ppPointList, ref pNumberOfIntersections, 1E-05) == 0 && pNumberOfIntersections > 0)
			{
				for (int num = 0; num < pNumberOfIntersections; num++)
				{
					RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[num], _RightOffsetLeg, ref _dStation, ref _dOffset);
					int num2 = 0;
					int num3 = 0;
					bool flag5 = false;
					if (_RightIntersectionStations.Count == 0)
					{
						_RightIntersectionStations.Add(new IntersectionStationContainer(_dStation));
					}
					else
					{
						for (num2 = 0; num2 < _RightIntersectionStations.Count; num2++)
						{
							for (num3 = 0; num3 < _RightIntersectionStations[num2].IntersectionStations.Count; num3++)
							{
								if (_dStation < _RightIntersectionStations[num2].IntersectionStations[num3].Station)
								{
									_RightIntersectionStations[num2].IntersectionStations.Insert(num3, new IntersectionStation(_dStation));
									flag5 = true;
									break;
								}
							}
							if (flag5)
							{
								break;
							}
							if (num3 == _RightIntersectionStations[num2].IntersectionStations.Count)
							{
								_RightIntersectionStations[num2].IntersectionStations.Add(new IntersectionStation(_dStation));
								break;
							}
						}
					}
					if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[num], _LeftExistingOffsetLeg, ref _dStation, ref _dOffset) == 1700)
					{
						_RightIntersectionStations[num2].IntersectionStations[num3].IsPrimaryLeft = true;
						_RightIntersectionStations[num2].IntersectionStations[num3].PrimaryStation = _dStation;
					}
					_RightIntersectionStations[num2].IntersectionStations[num3].IsLeft = false;
					_RightIntersectionStations[num2].IntersectionStations[num3].Radius = _dIntersectionRadius;
					_RightIntersectionStations[num2].IntersectionStations[num3].TransitioningDistance = _dIntersectionTransitioningDistance;
				}
			}
			if (RevitGeometry.eplGeoE_IntersectTwoLegLists(_RightOffsetLeg, _RightExistingOffsetLeg, ref ppPointList, ref pNumberOfIntersections, 1E-05) == 0 && pNumberOfIntersections > 0)
			{
				for (int num4 = 0; num4 < pNumberOfIntersections; num4++)
				{
					RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[num4], _RightOffsetLeg, ref _dStation, ref _dOffset);
					int num5 = 0;
					int num6 = 0;
					bool flag6 = false;
					if (_RightIntersectionStations.Count == 0)
					{
						_RightIntersectionStations.Add(new IntersectionStationContainer(_dStation));
					}
					else
					{
						for (num5 = 0; num5 < _RightIntersectionStations.Count; num5++)
						{
							for (num6 = 0; num6 < _RightIntersectionStations[num5].IntersectionStations.Count; num6++)
							{
								if (_dStation < _RightIntersectionStations[num5].IntersectionStations[num6].Station)
								{
									_RightIntersectionStations[num5].IntersectionStations.Insert(num6, new IntersectionStation(_dStation));
									flag6 = true;
									break;
								}
							}
							if (flag6)
							{
								break;
							}
							if (num6 == _RightIntersectionStations[num5].IntersectionStations.Count)
							{
								_RightIntersectionStations[num5].IntersectionStations.Add(new IntersectionStation(_dStation));
								break;
							}
						}
					}
					if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[num4], _RightExistingOffsetLeg, ref _dStation, ref _dOffset) == 1700)
					{
						_RightIntersectionStations[num5].IntersectionStations[num6].IsPrimaryLeft = false;
						_RightIntersectionStations[num5].IntersectionStations[num6].PrimaryStation = _dStation;
					}
					_RightIntersectionStations[num5].IntersectionStations[num6].IsLeft = false;
					_RightIntersectionStations[num5].IntersectionStations[num6].Radius = _dIntersectionRadius;
					_RightIntersectionStations[num5].IntersectionStations[num6].TransitioningDistance = _dIntersectionTransitioningDistance;
				}
			}
			_bFindIntersectionStations = !UpdateRadiusAndTransitionalDistanceForIntersectionStations(list, ref _LeftIntersectionStations);
			if (!_bFindIntersectionStations)
			{
				_bFindIntersectionStations = !UpdateRadiusAndTransitionalDistanceForIntersectionStations(list2, ref _RightIntersectionStations);
			}
		}
		_CenterIntersectionStations.Clear();
		if (RevitGeometry.eplGeoE_IntersectTwoLegLists(_CurrentLeg, _LeftExistingOffsetLeg, ref ppPointList, ref pNumberOfIntersections, 1E-05) == 0 && pNumberOfIntersections > 0)
		{
			for (int num7 = 0; num7 < pNumberOfIntersections; num7++)
			{
				RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[num7], _CurrentLeg, ref _dStation, ref _dOffset);
				int num8 = 0;
				int num9 = 0;
				bool flag7 = false;
				if (_CenterIntersectionStations.Count == 0)
				{
					_CenterIntersectionStations.Add(new IntersectionStationContainer(_dStation));
				}
				else
				{
					for (num8 = 0; num8 < _CenterIntersectionStations.Count; num8++)
					{
						for (num9 = 0; num9 < _CenterIntersectionStations[num8].IntersectionStations.Count; num9++)
						{
							if (_dStation < _CenterIntersectionStations[num8].IntersectionStations[num9].Station)
							{
								_CenterIntersectionStations[num8].IntersectionStations.Insert(num9, new IntersectionStation(_dStation));
								flag7 = true;
								break;
							}
						}
						if (flag7)
						{
							break;
						}
						if (num9 == _CenterIntersectionStations[num8].IntersectionStations.Count)
						{
							_CenterIntersectionStations[num8].IntersectionStations.Add(new IntersectionStation(_dStation));
							break;
						}
					}
				}
				if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[num7], _LeftExistingOffsetLeg, ref _dStation, ref _dOffset) == 1700)
				{
					_CenterIntersectionStations[num8].IntersectionStations[num9].IsPrimaryLeft = true;
					_CenterIntersectionStations[num8].IntersectionStations[num9].PrimaryStation = _dStation;
				}
			}
		}
		if (RevitGeometry.eplGeoE_IntersectTwoLegLists(_CurrentLeg, _RightExistingOffsetLeg, ref ppPointList, ref pNumberOfIntersections, 1E-05) == 0 && pNumberOfIntersections > 0)
		{
			for (int num10 = 0; num10 < pNumberOfIntersections; num10++)
			{
				RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[num10], _CurrentLeg, ref _dStation, ref _dOffset);
				int num11 = 0;
				int num12 = 0;
				bool flag8 = false;
				if (_CenterIntersectionStations.Count == 0)
				{
					_CenterIntersectionStations.Add(new IntersectionStationContainer(_dStation));
				}
				else
				{
					for (num11 = 0; num11 < _CenterIntersectionStations.Count; num11++)
					{
						for (num12 = 0; num12 < _CenterIntersectionStations[num11].IntersectionStations.Count; num12++)
						{
							if (_dStation < _CenterIntersectionStations[num11].IntersectionStations[num12].Station)
							{
								_CenterIntersectionStations[num11].IntersectionStations.Insert(num12, new IntersectionStation(_dStation));
								flag8 = true;
								break;
							}
						}
						if (flag8)
						{
							break;
						}
						if (num12 == _CenterIntersectionStations[num11].IntersectionStations.Count)
						{
							_CenterIntersectionStations[num11].IntersectionStations.Add(new IntersectionStation(_dStation));
							break;
						}
					}
				}
				if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(ppPointList[num10], _RightExistingOffsetLeg, ref _dStation, ref _dOffset) == 1700)
				{
					_CenterIntersectionStations[num11].IntersectionStations[num12].IsPrimaryLeft = false;
					_CenterIntersectionStations[num11].IntersectionStations[num12].PrimaryStation = _dStation;
				}
			}
		}
		if (_bDisplayPromptWhenNecessary && (_LeftIntersectionStations.Count == 0 || _LeftIntersectionStations.Count != _RightIntersectionStations.Count || _LeftIntersectionStations.Count != _CenterIntersectionStations.Count))
		{
			if (_LocateType == LandcaddID.Street)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("StreetIntersectionNotOnStraightLinesMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
			else
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("SidewalkIntersectionNotOnStraightLinesMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
			return false;
		}
		DialogResult dialogResult = DialogResult.Ignore;
		if (_bFindIntersectionStations && _bDisplayPromptWhenNecessary)
		{
			dialogResult = RevitMessageBox.Show(EPShared.LocalRM.GetString("IntersectionDetectedMessage"), EPShared.LocalRM.GetString("InformationString"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk);
			if (dialogResult == DialogResult.Cancel)
			{
				return false;
			}
		}
		bool bSurpressError = !_bDisplayPromptWhenNecessary;
		bool intersectionProfiles = GetIntersectionProfiles(_LocateType, _LeftExistingOffsetLeg, _RightExistingOffsetLeg, _LeftOffsetLeg, _dExistingAdjustSubgradeElev, flag, bSurpressError, _RightIntersectionStations, ref _LeftIntersectionStations);
		if (intersectionProfiles)
		{
			bSurpressError = true;
		}
		bool intersectionProfiles2 = GetIntersectionProfiles(_LocateType, _LeftExistingOffsetLeg, _RightExistingOffsetLeg, _RightOffsetLeg, _dExistingAdjustSubgradeElev, flag, bSurpressError, _LeftIntersectionStations, ref _RightIntersectionStations);
		if ((dialogResult == DialogResult.Yes || intersectionProfiles || intersectionProfiles2) && _bDisplayPromptWhenNecessary)
		{
			ChangeIntersectionRadiusForm changeIntersectionRadiusForm = new ChangeIntersectionRadiusForm(_Document, _TransactionContainer, _LocateType, _LeftExistingOffsetLeg, _RightExistingOffsetLeg, _LeftOffsetLeg, _RightOffsetLeg, _dExistingAdjustSubgradeElev, flag, _dMinimumRadius);
			changeIntersectionRadiusForm.LeftIntersectionStations = _LeftIntersectionStations;
			changeIntersectionRadiusForm.RightIntersectionStations = _RightIntersectionStations;
			changeIntersectionRadiusForm.Draw();
			DialogResult _DialogResult = DialogResult.Retry;
			while (_DialogResult == DialogResult.Retry)
			{
				changeIntersectionRadiusForm.ShowDialog(ref _DialogResult);
				switch (_DialogResult)
				{
				case DialogResult.Cancel:
					return false;
				case DialogResult.Retry:
				{
					Eps_Point _UsePoint = new Eps_Point();
					if (RevitUtility.PickSide(_Document, _TransactionContainer, 10.0, changeIntersectionRadiusForm.MidPointList, ref _UsePoint))
					{
						int _nStationContainerIndex = -1;
						int _nStationIndex = -1;
						if (FindPointInList(_LeftIntersectionStations, _UsePoint, ref _nStationContainerIndex, ref _nStationIndex))
						{
							changeIntersectionRadiusForm.RightSide = false;
							changeIntersectionRadiusForm.IntersectStationContainerIndex = _nStationContainerIndex;
							changeIntersectionRadiusForm.IntersectStationIndex = _nStationIndex;
							changeIntersectionRadiusForm.SetNewRadius(_LeftIntersectionStations[_nStationContainerIndex].IntersectionStations[_nStationIndex].Radius);
						}
						else if (FindPointInList(_RightIntersectionStations, _UsePoint, ref _nStationContainerIndex, ref _nStationIndex))
						{
							changeIntersectionRadiusForm.RightSide = true;
							changeIntersectionRadiusForm.IntersectStationContainerIndex = _nStationContainerIndex;
							changeIntersectionRadiusForm.IntersectStationIndex = _nStationIndex;
							changeIntersectionRadiusForm.SetNewRadius(_RightIntersectionStations[_nStationContainerIndex].IntersectionStations[_nStationIndex].Radius);
						}
					}
					break;
				}
				case DialogResult.Ignore:
					changeIntersectionRadiusForm.Draw();
					_DialogResult = DialogResult.Retry;
					break;
				}
			}
		}
		else if (dialogResult == DialogResult.No)
		{
			_LeftIntersectionStations.Clear();
			_RightIntersectionStations.Clear();
			_CenterIntersectionStations.Clear();
		}
		if (!GetIntersectionProfiles(_LeftExistingOffsetLeg, _RightExistingOffsetLeg, _CurrentLeg, _dExistingAdjustSubgradeElev, flag, _LeftIntersectionStations, _RightIntersectionStations, ref _CenterIntersectionStations))
		{
			return false;
		}
		return true;
	}

	private static bool FindPointInList(List<IntersectionStationContainer> _IntersectionStations, Eps_Point _Point, ref int _nStationContainerIndex, ref int _nStationIndex)
	{
		int num = 0;
		int num2 = 0;
		foreach (IntersectionStationContainer _IntersectionStation in _IntersectionStations)
		{
			foreach (IntersectionStation intersectionStation in _IntersectionStation.IntersectionStations)
			{
				if (intersectionStation.InvalidIntersectionPnt == null)
				{
					if (intersectionStation.IntersectionLeg != null)
					{
						if (intersectionStation.IntersectionLeg.Arc != null)
						{
							if (EPShared.eplGeoE_IsPointOnArc2D(_Point, intersectionStation.IntersectionLeg.Arc, 0.01))
							{
								_nStationContainerIndex = num;
								_nStationIndex = num2;
								return true;
							}
						}
						else if (intersectionStation.IntersectionLeg.Line != null && EPShared.eplGeoE_IsPointOnLine2D(_Point, intersectionStation.IntersectionLeg.Line, 0.01))
						{
							_nStationContainerIndex = num;
							_nStationIndex = num2;
							return true;
						}
					}
				}
				else if (_Point.IsEqual(intersectionStation.InvalidIntersectionPnt))
				{
					_nStationContainerIndex = num;
					_nStationIndex = num2;
					return true;
				}
				num2++;
			}
			num++;
		}
		return false;
	}

	private static bool GetIntersectionProfiles(Eps_Leg _LeftExistingOffsetLeg, Eps_Leg _RightExistingOffsetLeg, Eps_Leg _CurrentOffsetLeg, double _dExistingAdjustSubgradeElev, bool _bIsFirstIntersectionFourWay, List<IntersectionStationContainer> _LeftIntersectionStations, List<IntersectionStationContainer> _RightIntersectionStations, ref List<IntersectionStationContainer> _IntersectionStations)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Point Pnt = null;
		bool flag = true;
		double _dStation = 0.0;
		double _dStation2 = 0.0;
		double _dOffset = 0.0;
		if (_LeftIntersectionStations.Count != _RightIntersectionStations.Count)
		{
			return false;
		}
		for (int i = 0; i < _IntersectionStations.Count; i++)
		{
			flag = _bIsFirstIntersectionFourWay;
			if (_LeftIntersectionStations[i].IntersectionStations.Count != _RightIntersectionStations[i].IntersectionStations.Count)
			{
				continue;
			}
			for (int j = 0; j < _IntersectionStations[i].IntersectionStations.Count; j++)
			{
				eps_Leg2 = (_IntersectionStations[i].IntersectionStations[j].IsPrimaryLeft ? _LeftExistingOffsetLeg : _RightExistingOffsetLeg);
				if (_LeftIntersectionStations[i].IntersectionStations[j].IntersectionLeg == null || _RightIntersectionStations[i].IntersectionStations[j].IntersectionLeg == null || RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(eps_Leg2, _IntersectionStations[i].IntersectionStations[j].PrimaryStation, 0.0, ref Pnt) != 0)
				{
					continue;
				}
				Pnt.z += _dExistingAdjustSubgradeElev;
				eps_Leg = new Eps_Leg();
				eps_Leg.Line = new Eps_Line();
				if (flag)
				{
					eps_Leg.Line.EndPnt = Pnt;
					RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_LeftIntersectionStations[i].IntersectionStations[j].IntersectionLeg.LegStartPnt, _CurrentOffsetLeg, ref _dStation, ref _dOffset, _bAlwaysSetStationOffset: true);
					RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_RightIntersectionStations[i].IntersectionStations[j].IntersectionLeg.LegStartPnt, _CurrentOffsetLeg, ref _dStation2, ref _dOffset, _bAlwaysSetStationOffset: true);
					if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_CurrentOffsetLeg, (_dStation + _dStation2) / 2.0, 0.0, ref Pnt) == 0)
					{
						eps_Leg.Line.StartPnt = Pnt;
					}
				}
				else
				{
					eps_Leg.Line.StartPnt = Pnt;
					RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_LeftIntersectionStations[i].IntersectionStations[j].IntersectionLeg.LegEndPnt, _CurrentOffsetLeg, ref _dStation, ref _dOffset, _bAlwaysSetStationOffset: true);
					RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_RightIntersectionStations[i].IntersectionStations[j].IntersectionLeg.LegEndPnt, _CurrentOffsetLeg, ref _dStation2, ref _dOffset, _bAlwaysSetStationOffset: true);
					if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_CurrentOffsetLeg, (_dStation + _dStation2) / 2.0, 0.0, ref Pnt) == 0)
					{
						eps_Leg.Line.EndPnt = Pnt;
					}
				}
				_IntersectionStations[i].IntersectionStations[j].IntersectionLeg = eps_Leg;
				_IntersectionStations[i].IntersectionStations[j].StartingIntersection = flag;
				flag = !flag;
			}
		}
		return true;
	}

	public static bool GetIntersectionProfiles(LandcaddID _LocateType, Eps_Leg _LeftExistingOffsetLeg, Eps_Leg _RightExistingOffsetLeg, Eps_Leg _CurrentOffsetLeg, double _dExistingAdjustSubgradeElev, bool _bIsFirstIntersectionFourWay, List<IntersectionStationContainer> _OtherSideExistingStations, ref List<IntersectionStationContainer> _IntersectionStations)
	{
		return GetIntersectionProfiles(_LocateType, _LeftExistingOffsetLeg, _RightExistingOffsetLeg, _CurrentOffsetLeg, _dExistingAdjustSubgradeElev, _bIsFirstIntersectionFourWay, -1, -1, _bSurpressError: false, _OtherSideExistingStations, ref _IntersectionStations);
	}

	public static bool GetIntersectionProfiles(LandcaddID _LocateType, Eps_Leg _LeftExistingOffsetLeg, Eps_Leg _RightExistingOffsetLeg, Eps_Leg _CurrentOffsetLeg, double _dExistingAdjustSubgradeElev, bool _bIsFirstIntersectionFourWay, bool _bSurpressError, List<IntersectionStationContainer> _OtherSideExistingStations, ref List<IntersectionStationContainer> _IntersectionStations)
	{
		return GetIntersectionProfiles(_LocateType, _LeftExistingOffsetLeg, _RightExistingOffsetLeg, _CurrentOffsetLeg, _dExistingAdjustSubgradeElev, _bIsFirstIntersectionFourWay, -1, -1, _bSurpressError, _OtherSideExistingStations, ref _IntersectionStations);
	}

	public static bool GetIntersectionProfiles(LandcaddID _LocateType, Eps_Leg _LeftExistingOffsetLeg, Eps_Leg _RightExistingOffsetLeg, Eps_Leg _CurrentOffsetLeg, double _dExistingAdjustSubgradeElev, bool _bIsFirstIntersectionFourWay, int _nIntersectionContainerIndex, int _nIntersectionStationIndex, List<IntersectionStationContainer> _OtherSideExistingStations, ref List<IntersectionStationContainer> _IntersectionStations)
	{
		return GetIntersectionProfiles(_LocateType, _LeftExistingOffsetLeg, _RightExistingOffsetLeg, _CurrentOffsetLeg, _dExistingAdjustSubgradeElev, _bIsFirstIntersectionFourWay, _nIntersectionContainerIndex, _nIntersectionStationIndex, _bSurpressError: false, _OtherSideExistingStations, ref _IntersectionStations);
	}

	public static bool GetIntersectionProfiles(LandcaddID _LocateType, Eps_Leg _LeftExistingOffsetLeg, Eps_Leg _RightExistingOffsetLeg, Eps_Leg _CurrentOffsetLeg, double _dExistingAdjustSubgradeElev, bool _bIsFirstIntersectionFourWay, int _nIntersectionContainerIndex, int _nIntersectionStationIndex, bool _bSurpressError, List<IntersectionStationContainer> _OtherSideExistingStations, ref List<IntersectionStationContainer> _IntersectionStations)
	{
		Eps_Leg Leg = null;
		Eps_Leg eps_Leg = null;
		Eps_Line eps_Line = null;
		Eps_Line eps_Line2 = null;
		Eps_Point Pnt = null;
		Eps_Point Pnt2 = null;
		Eps_Point Pnt3 = null;
		Eps_Arc eps_Arc = null;
		double _dDistance = 0.0;
		double _dDirection = 0.0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = _bSurpressError;
		if (_IntersectionStations.Count != _OtherSideExistingStations.Count)
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		if (_nIntersectionContainerIndex >= 0 && _nIntersectionContainerIndex < _IntersectionStations.Count)
		{
			num = _nIntersectionContainerIndex;
			num3 = num + 1;
		}
		else
		{
			num = 0;
			num3 = _IntersectionStations.Count;
		}
		for (; num < num3; num++)
		{
			flag = _bIsFirstIntersectionFourWay;
			if (_IntersectionStations[num].IntersectionStations.Count != _OtherSideExistingStations[num].IntersectionStations.Count)
			{
				continue;
			}
			if (_nIntersectionStationIndex >= 0 && _nIntersectionStationIndex < _IntersectionStations[num].IntersectionStations.Count)
			{
				num2 = _nIntersectionStationIndex;
				num4 = num2 + 1;
			}
			else
			{
				num2 = 0;
				num4 = _IntersectionStations[num].IntersectionStations.Count;
			}
			for (; num2 < _IntersectionStations[num].IntersectionStations.Count; num2++)
			{
				eps_Leg = (_IntersectionStations[num].IntersectionStations[num2].IsPrimaryLeft ? _LeftExistingOffsetLeg : _RightExistingOffsetLeg);
				if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(eps_Leg, _IntersectionStations[num].IntersectionStations[num2].PrimaryStation, 0.0, ref Pnt3, ref Leg) == 0)
				{
					if (_IntersectionStations[num].IntersectionStations[num2].Radius > 0.0)
					{
						if (Leg.Line != null)
						{
							eps_Line = new Eps_Line();
							eps_Line.StartPnt = Pnt3;
							if (_IntersectionStations[num].IntersectionStations[num2].PrimaryStation < _OtherSideExistingStations[num].IntersectionStations[num2].PrimaryStation)
							{
								EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(Leg.Line.StartPnt, Pnt3, ref _dDistance);
								if (_dDistance < _IntersectionStations[num].IntersectionStations[num2].Radius)
								{
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(Pnt3, Leg.Line.StartPnt, ref _dDirection);
									Pnt = new Eps_Point();
									EPShared.eplGeoE_CalcPolarToXY(Pnt3, _dDirection, _IntersectionStations[num].IntersectionStations[num2].Radius, ref Pnt);
									if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(Pnt, eps_Leg, ref _dStation, ref _dOffset) != 1700 || !RevitDefines.EQ(_dOffset, 0.0))
									{
										flag2 = true;
									}
									eps_Line.EndPnt = Pnt;
								}
								else
								{
									eps_Line.EndPnt = Leg.Line.StartPnt;
								}
							}
							else
							{
								EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(Leg.Line.EndPnt, Pnt3, ref _dDistance);
								if (_dDistance < _IntersectionStations[num].IntersectionStations[num2].Radius)
								{
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(Pnt3, Leg.Line.EndPnt, ref _dDirection);
									Pnt = new Eps_Point();
									EPShared.eplGeoE_CalcPolarToXY(Pnt3, _dDirection, _IntersectionStations[num].IntersectionStations[num2].Radius, ref Pnt);
									if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(Pnt, eps_Leg, ref _dStation, ref _dOffset) != 1700 || !RevitDefines.EQ(_dOffset, 0.0))
									{
										flag2 = true;
									}
									eps_Line.EndPnt = Pnt;
								}
								else
								{
									eps_Line.EndPnt = Leg.Line.EndPnt;
								}
							}
							Pnt = new Eps_Point();
							if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_CurrentOffsetLeg, _IntersectionStations[num].IntersectionStations[num2].Station, 0.0, ref Pnt, ref Leg) == 0)
							{
								if (Leg.Line != null)
								{
									eps_Line2 = new Eps_Line();
									eps_Line2.EndPnt = Pnt3;
									if (flag)
									{
										EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(Leg.Line.StartPnt, Pnt3, ref _dDistance);
										if (_dDistance < _IntersectionStations[num].IntersectionStations[num2].Radius)
										{
											EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(Pnt3, Leg.Line.StartPnt, ref _dDirection);
											Pnt = new Eps_Point();
											EPShared.eplGeoE_CalcPolarToXY(Pnt3, _dDirection, _IntersectionStations[num].IntersectionStations[num2].Radius, ref Pnt);
											if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(Pnt, _CurrentOffsetLeg, ref _dStation, ref _dOffset) != 1700 || !RevitDefines.EQ(_dOffset, 0.0))
											{
												flag2 = true;
											}
											eps_Line2.StartPnt = Pnt;
										}
										else
										{
											eps_Line2.StartPnt = Leg.Line.StartPnt;
										}
									}
									else
									{
										EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(Leg.Line.EndPnt, Pnt3, ref _dDistance);
										if (_dDistance < _IntersectionStations[num].IntersectionStations[num2].Radius)
										{
											EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(Pnt3, Leg.Line.EndPnt, ref _dDirection);
											Pnt = new Eps_Point();
											EPShared.eplGeoE_CalcPolarToXY(Pnt3, _dDirection, _IntersectionStations[num].IntersectionStations[num2].Radius, ref Pnt);
											if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(Pnt, _CurrentOffsetLeg, ref _dStation, ref _dOffset) != 1700 || !RevitDefines.EQ(_dOffset, 0.0))
											{
												flag2 = true;
											}
											eps_Line2.StartPnt = Pnt;
										}
										else
										{
											eps_Line2.StartPnt = Leg.Line.EndPnt;
										}
									}
									eps_Arc = new Eps_Arc();
									if (RevitGeometry.SolveArcFromTangentLines(eps_Line2, eps_Line, _IntersectionStations[num].IntersectionStations[num2].Radius, ref eps_Arc, _bVerifyTangentLength: false) != 0)
									{
										flag2 = true;
									}
									if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Arc.EndPnt, eps_Leg, ref _dStation, ref _dOffset) != 1700)
									{
										flag2 = true;
									}
									if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(eps_Leg, _dStation, 0.0, ref Pnt2) != 0)
									{
										flag2 = true;
									}
									Pnt2.z += _dExistingAdjustSubgradeElev;
									Leg = new Eps_Leg();
									Leg.Arc = eps_Arc;
									Leg.Arc.EndPnt = Pnt2;
									if (!flag)
									{
										Leg = Leg.Reverse();
									}
									if (flag2)
									{
										if (!flag3)
										{
											if (!flag4)
											{
												RevitMessageBox.Show(EPShared.LocalRM.GetString("RadiusMakesArcLengthMoreThanHostLineMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
											}
											flag3 = true;
										}
										_IntersectionStations[num].IntersectionStations[num2].InvalidIntersectionPnt = Pnt3;
										_IntersectionStations[num].IntersectionStations[num2].IntersectionLeg = null;
									}
									else
									{
										_IntersectionStations[num].IntersectionStations[num2].InvalidIntersectionPnt = null;
										_IntersectionStations[num].IntersectionStations[num2].IntersectionLeg = Leg;
									}
									_IntersectionStations[num].IntersectionStations[num2].StartingIntersection = flag;
									flag = !flag;
								}
								else
								{
									if (!flag3)
									{
										if (!flag4)
										{
											if (_LocateType == LandcaddID.Street)
											{
												RevitMessageBox.Show(EPShared.LocalRM.GetString("StreetIntersectionNotOnStraightLinesMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
											}
											else
											{
												RevitMessageBox.Show(EPShared.LocalRM.GetString("SidewalkIntersectionNotOnStraightLinesMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
											}
										}
										flag3 = true;
									}
									_IntersectionStations[num].IntersectionStations[num2].InvalidIntersectionPnt = Pnt3;
									_IntersectionStations[num].IntersectionStations[num2].IntersectionLeg = null;
								}
							}
						}
						else
						{
							if (!flag3)
							{
								if (!flag4)
								{
									if (_LocateType == LandcaddID.Street)
									{
										RevitMessageBox.Show(EPShared.LocalRM.GetString("StreetIntersectionNotOnStraightLinesMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
									}
									else
									{
										RevitMessageBox.Show(EPShared.LocalRM.GetString("SidewalkIntersectionNotOnStraightLinesMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
									}
								}
								flag3 = true;
							}
							_IntersectionStations[num].IntersectionStations[num2].InvalidIntersectionPnt = Pnt3;
							_IntersectionStations[num].IntersectionStations[num2].IntersectionLeg = null;
						}
					}
					else if (_IntersectionStations[num].IntersectionStations[num2].TransitioningDistance > 0.0)
					{
						if (Leg.Line != null)
						{
							Pnt3.z += _dExistingAdjustSubgradeElev;
							Leg = new Eps_Leg();
							Leg.Line = new Eps_Line();
							if (flag)
							{
								if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_CurrentOffsetLeg, _IntersectionStations[num].IntersectionStations[num2].Station - _IntersectionStations[num].IntersectionStations[num2].TransitioningDistance, 0.0, ref Pnt) != 0)
								{
									flag2 = true;
								}
								Leg.Line.StartPnt = Pnt;
								Leg.Line.EndPnt = Pnt3;
							}
							else
							{
								Leg.Line.StartPnt = Pnt3;
								if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_CurrentOffsetLeg, _IntersectionStations[num].IntersectionStations[num2].Station + _IntersectionStations[num].IntersectionStations[num2].TransitioningDistance, 0.0, ref Pnt) != 0)
								{
									flag2 = true;
								}
								Leg.Line.EndPnt = Pnt;
							}
							if (flag2)
							{
								if (!flag3)
								{
									if (!flag4)
									{
										RevitMessageBox.Show(EPShared.LocalRM.GetString("TransitioningDistanceMakesDistanceMoreThanHostLineMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
									}
									flag3 = true;
								}
								_IntersectionStations[num].IntersectionStations[num2].InvalidIntersectionPnt = Pnt3;
								_IntersectionStations[num].IntersectionStations[num2].IntersectionLeg = null;
							}
							else
							{
								_IntersectionStations[num].IntersectionStations[num2].InvalidIntersectionPnt = null;
								_IntersectionStations[num].IntersectionStations[num2].IntersectionLeg = Leg;
							}
							_IntersectionStations[num].IntersectionStations[num2].StartingIntersection = flag;
							flag = !flag;
						}
						else
						{
							if (!flag3)
							{
								if (!flag4)
								{
									if (_LocateType == LandcaddID.Street)
									{
										RevitMessageBox.Show(EPShared.LocalRM.GetString("StreetIntersectionNotOnStraightLinesMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
									}
									else
									{
										RevitMessageBox.Show(EPShared.LocalRM.GetString("SidewalkIntersectionNotOnStraightLinesMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
									}
								}
								flag3 = true;
							}
							_IntersectionStations[num].IntersectionStations[num2].InvalidIntersectionPnt = Pnt3;
							_IntersectionStations[num].IntersectionStations[num2].IntersectionLeg = null;
						}
					}
					else
					{
						if (!flag3)
						{
							if (!flag4)
							{
								RevitMessageBox.Show(EPShared.LocalRM.GetString("TransitioningDistanceAndRadiusInvalidMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
							}
							flag3 = true;
						}
						_IntersectionStations[num].IntersectionStations[num2].IntersectionLeg = null;
					}
				}
				flag2 = false;
			}
		}
		return flag3;
	}

	private static List<FamilyInstance> GetSiteComponentPlacementFamilyInstances(Document _Document, string _strTopoSurfaceId)
	{
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		List<FamilyInstance> list2 = new List<FamilyInstance>();
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0 && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		Parameter parameter = null;
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
			{
				continue;
			}
			SiteworksIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (SiteworksIDParameter == null || string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) != 0)
			{
				continue;
			}
			LandcaddID landcaddID = LandcaddID.SoftTerrain;
			parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (parameter != null)
			{
				foreach (int value in Enum.GetValues(typeof(LandcaddID)))
				{
					if (string.Compare(parameter.AsString(), value.ToString("000"), ignoreCase: true) == 0)
					{
						landcaddID = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value);
						break;
					}
				}
			}
			switch (landcaddID)
			{
			case LandcaddID.ParkingLot:
				parameter = familyInstance.LookupParameter(RevitParameters.ParkingLotSurfaceIDFamilyParameter);
				if (parameter != null && string.Compare(_strTopoSurfaceId, parameter.AsString(), ignoreCase: false) == 0)
				{
					list2.Add(familyInstance);
				}
				break;
			case LandcaddID.Curb:
				parameter = familyInstance.LookupParameter(RevitParameters.ParentSurfaceIDFamilyParameter);
				if (parameter != null && string.Compare(_strTopoSurfaceId, parameter.AsString(), ignoreCase: false) == 0)
				{
					list2.Add(familyInstance);
				}
				break;
			}
		}
		return list2;
	}

	private static List<Eps_Point> GetParkingLotTopoSurfacePoints(Eps_Leg _CurrentLeg, List<Eps_Point> _TopoPointList, double _dLinearInterval, double _dRadialInterval, double _dArcPointInterval, string _strPathName, TopographySurfaceContainer _ProposedSurface)
	{
		int nLineCount = 0;
		int nArcCount = 0;
		Eps_Line[] aLines = null;
		Eps_Arc[] aArcs = null;
		int[] aLegOrder = null;
		Eps_Point[] _aCutFillPoints = null;
		int _nNumCutFillPoints = 0;
		List<Eps_Point> list = new List<Eps_Point>();
		Eps_Line[] _aLeftCatchLines = null;
		int _nNumLeftCatchLines = 0;
		Eps_Line[] _aRightCatchLines = null;
		int _nNumRightCatchLines = 0;
		Eps_Line[] _aLeftCutFillLines = null;
		int _nNumLeftCutFillLines = 0;
		Eps_Line[] _aRightCutFillLines = null;
		int _nNumRightCutFillLines = 0;
		_CurrentLeg.ConvertLegToLineArcArrays(ref aLines, ref nLineCount, ref aArcs, ref nArcCount, ref aLegOrder, _ProposedSurface, _bUseToposurfaceElevation: false, _bUseConstantElevation: false, 0.0, _bExtendLeg: false);
		Eps_Point[] aPnts = _TopoPointList.ToArray();
		int count = _TopoPointList.Count;
		EPSharedExternal.CreateCutAndFillFromTopo(_strPathName, aLines, nLineCount, aArcs, nArcCount, aLegOrder, aPnts, count, _dLinearInterval, _dRadialInterval, _dArcPointInterval, 0.0, 0.0, 0.0, 0.01, 0.01, 1, 0.0, _bIsCrownPresent: false, _bIsProjectLegToTopo: true, 0.0, 0.0, 0.0, 0.0, ref _aCutFillPoints, ref _nNumCutFillPoints, ref _aLeftCatchLines, ref _nNumLeftCatchLines, ref _aRightCatchLines, ref _nNumRightCatchLines, ref _aLeftCutFillLines, ref _nNumLeftCutFillLines, ref _aRightCutFillLines, ref _nNumRightCutFillLines);
		if (_aCutFillPoints != null)
		{
			list = _aCutFillPoints.ToList();
		}
		for (int i = 0; i < _nNumLeftCatchLines; i++)
		{
			list.Add(_aLeftCatchLines[i].StartPnt);
			list.Add(_aLeftCatchLines[i].EndPnt);
		}
		for (int j = 0; j < _nNumRightCatchLines; j++)
		{
			list.Add(_aRightCatchLines[j].StartPnt);
			list.Add(_aRightCatchLines[j].EndPnt);
		}
		for (int k = 0; k < _nNumLeftCutFillLines; k++)
		{
			list.Add(_aLeftCutFillLines[k].StartPnt);
			list.Add(_aLeftCutFillLines[k].EndPnt);
		}
		for (int l = 0; l < _nNumRightCutFillLines; l++)
		{
			list.Add(_aRightCutFillLines[l].StartPnt);
			list.Add(_aRightCutFillLines[l].EndPnt);
		}
		return list;
	}

	private static bool CanMakeClosedPad(Eps_Leg leg)
	{
		if (leg == null) return false;
		if (leg.IsLegClosed()) return true;
		if (leg.LegStartPnt == null || leg.LegEndPnt == null) return false;
		double gap = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(leg.LegStartPnt, leg.LegEndPnt, ref gap);
		return gap < 1.0;
	}

	private static Eps_Leg CloseNearlyClosed(Eps_Leg boundary)
	{
		if (boundary == null) return null;
		Eps_Leg pad = Eps_Leg.CopyLeg(boundary) ?? boundary;
		if (!pad.IsLegClosed() && pad.LegStartPnt != null && pad.LegEndPnt != null && pad.Last != null)
		{
			double gap = 0.0;
			EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(pad.LegStartPnt, pad.LegEndPnt, ref gap);
			if (gap > 0.0 && gap < 1.0)
			{
				Eps_Leg closer = new Eps_Leg();
				closer.Line = new Eps_Line();
				closer.Line.StartPnt = new Eps_Point(pad.LegEndPnt);
				closer.Line.EndPnt = new Eps_Point(pad.LegStartPnt);
				pad.Last.Next = closer;
			}
		}
		return pad;
	}

	private static string CreateClosedPavementOverlay(Document document, SitePlacementSettings settings, Eps_Leg boundary, TopographySurfaceContainer proposed)
	{
		if (document == null || boundary == null) return string.Empty;
		Eps_Leg padBoundary = CloseNearlyClosed(boundary);
		if (padBoundary == null) return string.Empty;
		if (proposed != null)
			DrapeLegOntoToposolid(proposed, padBoundary, 0.0);
		double depth = 0.5;
		ElementId materialId = ElementId.InvalidElementId;
		if (settings.LocateType == LandcaddID.ParkingLot)
		{
			Parameter depthParameter = settings.Type?.LookupParameter(RevitParameters.ParkingLotPavementDepthParameter);
			if (depthParameter != null && depthParameter.AsDouble() > 1e-6)
				depth = depthParameter.AsDouble();
			Parameter materialParameter = settings.Type?.LookupParameter(RevitParameters.ParkingLotParkingPavementSurfaceParameter);
			if (materialParameter != null)
				materialId = RevitIds.Create(materialParameter.AsInteger());
		}
		else
		{
			depth = 0.15;
			if (proposed?.TopographySurface != null)
				materialId = TerrainApi.GetMaterialId(proposed.TopographySurface);
		}
		string name = settings.Name;
		if (string.IsNullOrEmpty(name))
			name = settings.LocateType == LandcaddID.ParkingLot ? "Parking Lot" : "Island";
		DirectShape pad = SitePavementOverlay.CreateExtrudedPad(document, padBoundary, depth, materialId, name, BuiltInCategory.OST_Roads);
		if (pad == null)
		{
			RuntimeDiagnostics.Write("Failed to create thin pavement overlay for " + settings.LocateType);
			return string.Empty;
		}
		RuntimeDiagnostics.Write("Created thin pavement overlay for " + settings.LocateType + " depth=" + depth + " ft.");
		return pad.UniqueId;
	}

	private static TopographySurface CreateAdditionalTopoSurface(Document _Document, SitePlacementSettings _PlacementSettings, List<Eps_Point> _NewSurfacePoints, TopographySurfaceContainer _ProposedSurface)
	{
		ElementId elementId = null;
		long _nReturnValue = -1;
		Parameter parameter = null;
		if (_PlacementSettings.LocateType == LandcaddID.Curb)
		{
			if (ApplicationUtility.Settings.eplSetE_GetSetting(_Document.PathName, "ParkingLotIslandToposurfaceMaterialParameter", ref _nReturnValue))
			{
				elementId = RevitIds.Create(_nReturnValue);
				bool flag = false;
				FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
				IList<Element> list = filteredElementCollector.OfClass(typeof(Material)).OfCategory(BuiltInCategory.OST_Materials).ToElements();
				foreach (Element item in list)
				{
					if (item.Id == elementId)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					_nReturnValue = -1;
				}
			}
			if (_nReturnValue == -1)
			{
				elementId = TerrainApi.GetMaterialId(_ProposedSurface.TopographySurface);
				_nReturnValue = elementId.Value64();
			}
		}
		else if (_PlacementSettings.LocateType == LandcaddID.ParkingLot)
		{
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.ParkingLotParkingPavementSurfaceParameter);
			if (parameter != null)
			{
				_nReturnValue = parameter.AsInteger();
			}
		}
		return CreateAdditionalTopoSurface(_Document, _PlacementSettings, _NewSurfacePoints, _ProposedSurface, _nReturnValue);
	}

	private static TopographySurface CreateAdditionalTopoSurface(Document _Document, SitePlacementSettings _PlacementSettings, List<Eps_Point> _NewSurfacePoints, TopographySurfaceContainer _ProposedSurface, long _nMaterialId)
	{
		if (TerrainApi.UsesToposolids)
		{
			RuntimeDiagnostics.Write("Skipped extra Toposolid for " + _PlacementSettings.LocateType + "; parking/island use a thin overlay.");
			return null;
		}
		TopographySurface topographySurface = null;
		if (_NewSurfacePoints != null && _NewSurfacePoints.Count > 0)
		{
			List<XYZ> PointXYZList = new List<XYZ>();
			for (int i = 0; i < _NewSurfacePoints.Count; i++)
			{
				PointXYZList.Add(_NewSurfacePoints[i].XYZ);
			}
			if (PointXYZList.Count > 0)
			{
				RemoveDuplicateXY(ref PointXYZList);
				topographySurface = TerrainApi.Create(_Document, PointXYZList, _ProposedSurface?.TopographySurface, copyHostSketch: false);
				TerrainApi.SetMaterialId(topographySurface, RevitIds.Create(_nMaterialId));
				_Document.Regenerate();
			}
			if (topographySurface != null)
			{
				Parameter parameter = null;
				if (_PlacementSettings.LocateType == LandcaddID.Curb)
				{
					int nBeginningIndex = 0;
					string empty = ((!GetIslandFamiliesCount(_Document, _PlacementSettings.Name, ref nBeginningIndex)) ? _PlacementSettings.Name : (_PlacementSettings.Name + "-" + nBeginningIndex));
					TerrainApi.SetUniqueName(topographySurface, empty);
				}
				else if (_PlacementSettings.LocateType == LandcaddID.ParkingLot)
				{
					TerrainApi.SetUniqueName(topographySurface, _PlacementSettings.Name);
				}
				foreach (Phase phase in _Document.Phases)
				{
					if ((_Document.DisplayUnitSystem == DisplayUnit.IMPERIAL && phase.Id.Value64() == 118390) || (_Document.DisplayUnitSystem == DisplayUnit.METRIC && phase.Id.Value64() == 86961) || string.Compare(phase.Name, EPShared.LocalRM.GetString("NewConstructionString"), ignoreCase: true) == 0)
					{
						parameter = ((Element)topographySurface).get_Parameter(BuiltInParameter.PHASE_CREATED);
						if (parameter != null && !parameter.IsReadOnly)
						{
							ElementId id = phase.Id;
							parameter.Set(id);
						}
					}
				}
			}
		}
		return topographySurface;
	}

	private static bool HostLineOnTerrain(TopographySurface surface, List<Eps_Point> points, out int missed, out int counted)
	{
		missed = 0;
		counted = 0;
		if (surface == null || points == null) return false;
		for (int i = 0; i < points.Count; i++)
		{
			if (points[i] == null) continue;
			counted++;
			if (!TerrainApi.ContainsXY(surface, points[i].XYZ)) missed++;
		}
		return counted > 0 && missed == 0;
	}

	private static bool HostLineFitsTerrain(Document document, ref TopographySurfaceContainer proposed, Eps_Leg hostLeg, Eps_Leg corridor, ref Eps_Line[] corridorLines, ref int corridorLineCount, ref Eps_Arc[] corridorArcs, ref int corridorArcCount, ref int[] corridorOrder)
	{
		if (proposed == null || corridor == null)
		{
			return true;
		}
		if (TerrainApi.UsesToposolids)
		{
			Eps_Leg check = hostLeg ?? corridor;
			List<Eps_Point> points = check.GetPointsAtIntervals(1.0, 1.0);
			if (HostLineOnTerrain(proposed.TopographySurface, points, out int missed, out int counted))
			{
				return true;
			}
			ElementId currentId = proposed.TopographySurface?.Id;
			TopographySurface match = null;
			int bestMissed = int.MaxValue;
			foreach (Element element in new FilteredElementCollector(document).OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category))
			{
				if (element == null || (currentId != null && element.Id == currentId)) continue;
				TopographySurface surface = TerrainApi.AsSurface(element);
				if (surface == null || surface.IsSiteSubRegion) continue;
				if (!HostLineOnTerrain(surface, points, out int otherMissed, out int otherCounted)) continue;
				int score = otherMissed;
				if (TerrainApi.IsProposedTerrain(element) || TerrainApi.LooksLikeProposedTerrain(element)) score -= 1;
				if (match == null || score < bestMissed)
				{
					match = surface;
					bestMissed = score;
				}
			}
			if (match != null)
			{
				proposed = new TopographySurfaceContainer(document.Application, match, _bFindMinMaxPoints: true);
				RuntimeDiagnostics.Write("Retargeted site component to the Toposolid that contains the host line.");
				return true;
			}
			RuntimeDiagnostics.Write("Street/host line has " + missed + " of " + counted + " samples outside the Toposolid sketch.");
			RevitMessageBox.Show(EPShared.LocalRM.GetString("LineSegmentsOutsideToposurfaceMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return false;
		}
		List<Eps_Point> pointsAtIntervals = corridor.GetPointsAtIntervals(1.0, 1.0);
		for (int n = 0; n < pointsAtIntervals.Count; n++)
		{
			TopographySurfaceContainer containing = proposed.GetSurfaceContainerContainingPoint(pointsAtIntervals[n]);
			if (containing == null)
			{
				corridor.ConvertLegToLineArcArrays(ref corridorLines, ref corridorLineCount, ref corridorArcs, ref corridorArcCount, ref corridorOrder, _bExtendLeg: false);
				int nLineCount3 = 0;
				int nArcCount3 = 0;
				Eps_Line[] aLines3 = null;
				Eps_Arc[] aArcs3 = null;
				int[] aLegOrder3 = null;
				if (proposed.BoundaryLeg != null)
				{
					proposed.BoundaryLeg.ConvertLegToLineArcArrays(ref aLines3, ref nLineCount3, ref aArcs3, ref nArcCount3, ref aLegOrder3, _bExtendLeg: false);
					if (!EPShared.eplGeoE_IsLegListInsideLegList(corridorLines, corridorLineCount, corridorArcs, corridorArcCount, corridorOrder, aLines3, nLineCount3, aArcs3, nArcCount3, aLegOrder3, 1E-05))
					{
						RevitMessageBox.Show(EPShared.LocalRM.GetString("LineSegmentsOutsideToposurfaceMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						return false;
					}
				}
			}
			else if (containing.TopographySurfaceType == TopographySurfaceType.BuildingPad)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("LineSegmentsInsideBuildingPadMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return false;
			}
		}
		return true;
	}

	private static bool GetIslandFamiliesCount(Document _Document, string _strFindName, ref int nBeginningIndex)
	{
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		List<string> list2 = new List<string>();
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0 && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list3 = filteredElementCollector.OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).ToElements();
		Parameter parameter = null;
		string empty = string.Empty;
		foreach (Element item in list3)
		{
			if (item == null || TerrainApi.AsSurface(item) == null)
			{
				continue;
			}
			empty = item.UniqueId;
			foreach (Element item2 in list)
			{
				if (!(item2 is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
				{
					continue;
				}
				SiteworksIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
				if (SiteworksIDParameter == null || string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) != 0)
				{
					continue;
				}
				parameter = familyInstance.LookupParameter(RevitParameters.IslandSurfaceIdFamilyParameter);
				if (parameter != null && string.Compare(empty, parameter.AsString(), ignoreCase: false) == 0)
				{
					parameter = TerrainApi.NameParameter(item);
					if (parameter != null && parameter.AsString() != null && parameter.AsString() != string.Empty)
					{
						list2.Add(parameter.AsString());
						break;
					}
				}
			}
		}
		nBeginningIndex = 0;
		int num = -1;
		string text = "-";
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		bool flag = true;
		bool result = false;
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			string text2 = list2[num2];
			int num3 = text2.LastIndexOf(text);
			if (num3 >= 0 && num3 < text2.Length)
			{
				empty2 = text2.Substring(num3);
				if (!empty2.StartsWith(text) || empty2.Length <= text.Length)
				{
					continue;
				}
				empty3 = empty2.Substring(text.Length);
				for (int num4 = 0; num4 < empty3.Length; num4++)
				{
					if (!char.IsDigit(empty3[num4]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					result = true;
					num = Convert.ToInt32(empty3);
					if (num > nBeginningIndex)
					{
						nBeginningIndex = num;
					}
				}
				else
				{
					flag = true;
				}
			}
			else if (string.Compare(text2, _strFindName, ignoreCase: true) == 0)
			{
				result = true;
			}
		}
		nBeginningIndex++;
		return result;
	}

	public static bool RebuildProposedTopographySurface(Document _Document, SitePlacementSettings _PlacementSettings, double _dArcPointInterval, Eps_Leg _CurrentLeg, TopographySurfaceContainer _ProposedSurface, List<Element> _ElementList, bool _bDrawForm, List<Eps_Leg> _ParkingLotBreaklineList, List<Element> _ParkingLotBreaklineElementList, List<double> _dParkingLotBreaklineLinearPointIntervals, List<IntersectionStationContainer> _LeftIntersectionStations, List<IntersectionStationContainer> _CenterIntersectionStations, List<IntersectionStationContainer> _RightIntersectionStations, List<IntersectionContainer> _IntersectionContainers, ref List<Eps_Point> _TopoPointList)
	{
		TopographySurfaceContainer _ParkingLotSurface = null;
		List<Eps_Point> _ParkingLotTopoPointList = null;
		List<Eps_Point> _CrownPointList = new List<Eps_Point>();
		List<FamilyInstance> _FamilyInstanceList = null;
		return RebuildProposedTopographySurface(_Document, _PlacementSettings, _dArcPointInterval, _CurrentLeg, _ProposedSurface, _ElementList, _bDrawForm, _ParkingLotBreaklineList, _ParkingLotBreaklineElementList, _dParkingLotBreaklineLinearPointIntervals, _LeftIntersectionStations, _CenterIntersectionStations, _RightIntersectionStations, _IntersectionContainers, ref _TopoPointList, ref _CrownPointList, ref _ParkingLotSurface, ref _ParkingLotTopoPointList, ref _FamilyInstanceList);
	}

	public static bool RebuildProposedTopographySurface(Document _Document, SitePlacementSettings _PlacementSettings, double _dArcPointInterval, Eps_Leg _CurrentLeg, TopographySurfaceContainer _ProposedSurface, List<Element> _ElementList, bool _bDrawForm, List<Eps_Leg> _ParkingLotBreaklineList, List<Element> _ParkingLotBreaklineElementList, List<double> _dParkingLotBreaklineLinearPointIntervals, List<IntersectionStationContainer> _LeftIntersectionStations, List<IntersectionStationContainer> _CenterIntersectionStations, List<IntersectionStationContainer> _RightIntersectionStations, List<IntersectionContainer> _IntersectionContainers, ref List<Eps_Point> _TopoPointList, ref List<Eps_Point> _CrownPointList)
	{
		TopographySurfaceContainer _ParkingLotSurface = null;
		List<Eps_Point> _ParkingLotTopoPointList = null;
		List<FamilyInstance> _FamilyInstanceList = null;
		return RebuildProposedTopographySurface(_Document, _PlacementSettings, _dArcPointInterval, _CurrentLeg, _ProposedSurface, _ElementList, _bDrawForm, _ParkingLotBreaklineList, _ParkingLotBreaklineElementList, _dParkingLotBreaklineLinearPointIntervals, _LeftIntersectionStations, _CenterIntersectionStations, _RightIntersectionStations, _IntersectionContainers, ref _TopoPointList, ref _CrownPointList, ref _ParkingLotSurface, ref _ParkingLotTopoPointList, ref _FamilyInstanceList);
	}

	private static bool hasContainer(List<IntersectionContainer> containers, IntersectionContainer container)
	{
		foreach (IntersectionContainer container2 in containers)
		{
			if (container2.IsEqual(container))
			{
				return true;
			}
		}
		return false;
	}

	public static bool RebuildProposedTopographySurface(Document _Document, SitePlacementSettings _PlacementSettings, double _dArcPointInterval, Eps_Leg _CurrentLeg, TopographySurfaceContainer _ProposedSurface, List<Element> _ElementList, bool _bModifiedSiteComponent, List<Eps_Leg> _ParkingLotBreaklineList, List<Element> _ParkingLotBreaklineElementList, List<double> _dParkingLotBreaklineLinearPointIntervals, List<IntersectionStationContainer> _LeftIntersectionStations, List<IntersectionStationContainer> _CenterIntersectionStations, List<IntersectionStationContainer> _RightIntersectionStations, List<IntersectionContainer> _IntersectionContainers, ref List<Eps_Point> _TopoPointList, ref List<Eps_Point> _CrownPointList, ref TopographySurfaceContainer _ParkingLotSurface, ref List<Eps_Point> _ParkingLotTopoPointList, ref List<FamilyInstance> _FamilyInstanceList)
	{
		List<Eps_Point> _AddPointList = new List<Eps_Point>();
		int nLineCount = 0;
		int nArcCount = 0;
		Eps_Line[] aLines = null;
		Eps_Arc[] aArcs = null;
		int[] aLegOrder = null;
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Leg eps_Leg4 = null;
		List<OffsetLegWrapper> _OffsetLegWrappers = new List<OffsetLegWrapper>();
		List<Eps_Point> list = new List<Eps_Point>();
		Parameter parameter = null;
		int nLineCount2 = 0;
		int nArcCount2 = 0;
		Eps_Line[] aLines2 = null;
		Eps_Arc[] aArcs2 = null;
		int[] aLegOrder2 = null;
		bool bUseToposurfaceElevation = _PlacementSettings.IsRelativeElevation;
		bool bUseConstantElevation = !_PlacementSettings.IsRelativeElevation;
		double num = _PlacementSettings.Elevation;
		double dElevation = 0.0;
		double num2 = 0.0;
		double num3 = 3.0;
		double num4 = 3.0;
		double num5 = 15.0;
		double num6 = 3.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		double num10 = 0.0;
		double dRightSubgradeAdjustElev = 0.0;
		double dAdjustSubgradeElev = 0.0;
		bool bIsCrownPresent = false;
		bool bIsProjectLegToTopo = false;
		double num11 = 0.0;
		double num12 = 0.0;
		double num13 = 0.0;
		double num14 = 0.0;
		double dStreetEdgeAdjustElev = 0.0;
		Eps_Leg originaltLeg = Eps_Leg.CopyLeg(_CurrentLeg);
		if (_PlacementSettings.Type != null)
		{
			switch (_PlacementSettings.LocateType)
			{
			case LandcaddID.SoftTerrain:
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SoftTerrainCutSlopeParameter);
				if (parameter != null)
				{
					num3 = 1.0 / parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SoftTerrainFillSlopeParameter);
				if (parameter != null)
				{
					num4 = 1.0 / parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SoftTerrainLeftOffsetWidthParameter);
				if (parameter != null)
				{
					num7 = parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SoftTerrainRightOffsetWidthParameter);
				if (parameter != null)
				{
					num8 = parameter.AsDouble();
				}
				break;
			case LandcaddID.Sidewalk:
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SideWalkCutSlopeParameter);
				if (parameter != null)
				{
					num3 = 1.0 / parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SideWalkFillSlopeParameter);
				if (parameter != null)
				{
					num4 = 1.0 / parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SideWalkWidthParameter);
				if (parameter != null)
				{
					double num15 = parameter.AsDouble();
					num7 = num15 / 2.0;
					num8 = num15 / 2.0;
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.SideWalkPavementDepthParameter);
				if (parameter != null)
				{
					dAdjustSubgradeElev = 0.0 - parameter.AsDouble();
				}
				break;
			case LandcaddID.Street:
			{
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCutSlopeParameter);
				if (parameter != null)
				{
					num3 = 1.0 / parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetFillSlopeParameter);
				if (parameter != null)
				{
					num4 = 1.0 / parameter.AsDouble();
				}
				double num22 = 0.0;
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetLaneWidthParameter);
				if (parameter != null)
				{
					num22 = parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
				if (parameter != null)
				{
					num7 = num22 * (double)parameter.AsInteger() / 2.0;
					num8 = num22 * (double)parameter.AsInteger() / 2.0;
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
				if (parameter != null)
				{
					num11 = (0.0 - num7) * parameter.AsDouble();
					num12 = (0.0 - num8) * parameter.AsDouble();
					dStreetEdgeAdjustElev = (0.0 - (num7 + num8)) / 2.0 * parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCurbDisplayParameter);
				if (parameter != null && parameter.AsInteger() != 0)
				{
					double num23 = 0.0;
					double num24 = 0.0;
					double num25 = 0.0;
					parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetGutterWidthParameter);
					if (parameter != null)
					{
						num23 = parameter.AsDouble();
						num7 += num23;
						num8 += num23;
						num2 = num23;
					}
					parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCurbWidthParameter);
					if (parameter != null)
					{
						num7 += parameter.AsDouble();
						num8 += parameter.AsDouble();
						num2 += parameter.AsDouble();
					}
					parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCurbHeightParameter);
					if (parameter != null)
					{
						num25 = parameter.AsDouble();
					}
					parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetGutterSlopeParameter);
					if (parameter != null)
					{
						num24 = parameter.AsDouble();
					}
					num11 += num25 - num23 * num24;
					num12 += num25 - num23 * num24;
					num10 = num25;
					dRightSubgradeAdjustElev = num25;
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetPavementDepthParameter);
				if (parameter != null)
				{
					dAdjustSubgradeElev = 0.0 - parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetBoulevardWidthParameter);
				if (parameter != null)
				{
					num9 = parameter.AsDouble();
					num7 += num9;
					num8 += num9;
					num2 += num9;
				}
				bIsCrownPresent = true;
				break;
			}
			case LandcaddID.RetainingWall:
			{
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.RetainingWallCutSlopeParameter);
				if (parameter != null)
				{
					num3 = 1.0 / parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.RetainingWallFillSlopeParameter);
				if (parameter != null)
				{
					num4 = 1.0 / parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.RetainingWallWidthParameter);
				if (parameter != null)
				{
					double num20 = parameter.AsDouble();
					num7 = num20 / 2.0;
					num8 = num20 / 2.0;
				}
				bool flag = false;
				double num21 = 0.0;
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.RetainingWallSlopeProjectionParameter);
				if (parameter != null)
				{
					flag = parameter.AsInteger() == 1;
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.RetainingWallHeightParameter);
				if (parameter != null)
				{
					num21 = parameter.AsDouble();
				}
				if (_PlacementSettings.IsRelativeElevation)
				{
					dAdjustSubgradeElev = 0.0 - (num21 - _PlacementSettings.Elevation);
					bUseToposurfaceElevation = true;
					bUseConstantElevation = false;
					dElevation = _PlacementSettings.Elevation;
				}
				else
				{
					num = _PlacementSettings.Elevation - num21;
					bUseToposurfaceElevation = false;
					bUseConstantElevation = true;
					dElevation = num21;
					num14 = num;
				}
				if (flag)
				{
					if (_PlacementSettings.UseRightSide)
					{
						num12 = ((!_PlacementSettings.IsRelativeElevation) ? num21 : _PlacementSettings.Elevation);
						_PlacementSettings.ProjectInnerSlopes = 3;
					}
					else
					{
						num11 = ((!_PlacementSettings.IsRelativeElevation) ? num21 : _PlacementSettings.Elevation);
						_PlacementSettings.ProjectInnerSlopes = 2;
					}
				}
				else
				{
					_PlacementSettings.ProjectInnerSlopes = 4;
				}
				bIsProjectLegToTopo = !_PlacementSettings.IsRelativeElevation;
				num10 = num11;
				dRightSubgradeAdjustElev = num12;
				break;
			}
			case LandcaddID.Curb:
			{
				double num16 = 0.0;
				num13 = 0.0;
				double num17 = 0.0;
				double num18 = 0.0;
				double num19 = 0.0;
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetGutterWidthParameter);
				if (parameter != null)
				{
					num17 = parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCurbWidthParameter);
				if (parameter != null)
				{
					num16 = parameter.AsDouble();
				}
				if (_PlacementSettings.UseRightSide)
				{
					num8 = num17 + num16;
					num7 = 0.0;
				}
				else
				{
					num7 = num17 + num16;
					num8 = 0.0;
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetCurbHeightParameter);
				if (parameter != null)
				{
					num18 = parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.CurbGutterHeightParameter);
				if (parameter != null)
				{
					num13 = parameter.AsDouble();
				}
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.StreetGutterSlopeParameter);
				if (parameter != null)
				{
					num19 = parameter.AsDouble();
				}
				if (num8 == 0.0)
				{
					dRightSubgradeAdjustElev = num13 + (num16 + num17) * num19;
					num10 = num13;
				}
				else if (num7 == 0.0)
				{
					num10 = num13 + (num16 + num17) * num19;
					dRightSubgradeAdjustElev = num13;
				}
				bUseToposurfaceElevation = true;
				bUseConstantElevation = false;
				m_dCurbElevation = num18 - num17 * num19;
				num = 0.0 - m_dCurbElevation;
				_PlacementSettings.ProjectInnerSlopes = 5;
				break;
			}
			case LandcaddID.ParkingLot:
				eps_Leg3 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentLeg, 0.02, 1, ref eps_Leg3, TrimClosedObject: true);
				eps_Leg4 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentLeg, 0.02, 2, ref eps_Leg4, TrimClosedObject: true);
				_CurrentLeg = ((!(RevitGeometry.eplGeoE_CalcLegListLength(eps_Leg4) > RevitGeometry.eplGeoE_CalcLegListLength(eps_Leg3))) ? eps_Leg3 : eps_Leg4);
				parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.ParkingLotPavementDepthParameter);
				if (parameter != null)
				{
					num10 = parameter.AsDouble();
					dRightSubgradeAdjustElev = parameter.AsDouble();
				}
				num3 = 0.01;
				num4 = 0.01;
				bIsProjectLegToTopo = true;
				_PlacementSettings.ProjectInnerSlopes = 1;
				break;
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.LinearIncrementParameter);
			if (parameter != null)
			{
				num5 = parameter.AsDouble();
			}
			if (RevitDefines.LT(num5, 0.0833333))
			{
				num5 = 0.0833333;
			}
			parameter = _PlacementSettings.Type.LookupParameter(RevitParameters.CurveIncrementParameter);
			if (parameter != null)
			{
				num6 = RevitGeometry.RadiansToDecimalDegrees(EPShared.eplGeoE_NormalizeRadians(parameter.AsDouble()));
			}
			if (RevitDefines.LT(num6, 1.0))
			{
				num6 = 1.0;
			}
		}
		List<double> list2 = new List<double>();
		List<Eps_Point> list3 = new List<Eps_Point>();
		Eps_Point eps_Point = null;
		if (_CenterIntersectionStations != null && _CenterIntersectionStations.Count > 0)
		{
			for (int i = 0; i < _CenterIntersectionStations.Count; i++)
			{
				for (int j = 0; j < _CenterIntersectionStations[i].IntersectionStations.Count; j++)
				{
					list2.Add(_CenterIntersectionStations[i].IntersectionStations[j].Station);
					eps_Point = GetIntersectionStationPoint(_CenterIntersectionStations, i, j, _bFindEndsOfIntersection: false);
					if (eps_Point != null)
					{
						list3.Add(eps_Point);
					}
				}
			}
		}
		_CurrentLeg = _CurrentLeg.SplitLegAtStations(list2);
		if (_PlacementSettings.LocateType != LandcaddID.FeatureLine && _PlacementSettings.LocateType != LandcaddID.PointWipeout)
		{
			_CurrentLeg.ConvertLegToLineArcArrays(ref aLines2, ref nLineCount2, ref aArcs2, ref nArcCount2, ref aLegOrder2, _ProposedSurface, bUseToposurfaceElevation, bUseConstantElevation, num, _bExtendLeg: true, list3);
		}
		else if (_PlacementSettings.LocateType == LandcaddID.FeatureLine)
		{
			num5 = _PlacementSettings.LinearPointInterval;
			if (RevitDefines.LT(num5, 0.0833333))
			{
				num5 = 0.0833333;
			}
		}
		double num26 = 0.0;
		num26 = _PlacementSettings.LocateType == LandcaddID.RetainingWall
			? num14
			: UsesFormPadElevation(_PlacementSettings.LocateType) ? num : 0.0;
		_CurrentLeg = _CurrentLeg.ApplyElevationsToLeg(_ProposedSurface, bUseToposurfaceElevation, bUseConstantElevation, num26, list3);
		if (TerrainApi.UsesToposolids && !bUseConstantElevation)
		{
			DrapeLegOntoToposolid(_ProposedSurface, _CurrentLeg, bUseToposurfaceElevation ? _PlacementSettings.Elevation : 0.0);
		}
		if (bUseConstantElevation)
		{
			ApplyConstantElevationToHostGeometry(_CurrentLeg, aLines2, nLineCount2, aArcs2, nArcCount2, num26);
		}
		else if (TerrainApi.UsesToposolids && _PlacementSettings.LocateType != LandcaddID.FeatureLine && _PlacementSettings.LocateType != LandcaddID.PointWipeout)
		{
			_CurrentLeg.ConvertLegToLineArcArrays(ref aLines2, ref nLineCount2, ref aArcs2, ref nArcCount2, ref aLegOrder2, _bExtendLeg: true);
		}
		List<IntersectionContainer> _IntersectionContainers2 = new List<IntersectionContainer>();
		FindOffsetLegWrappers(originaltLeg, _CurrentLeg, _LeftIntersectionStations, _CenterIntersectionStations, _RightIntersectionStations, _PlacementSettings.LocateType, num7, num8, num2, dAdjustSubgradeElev, dStreetEdgeAdjustElev, ref _OffsetLegWrappers, ref _IntersectionContainers2);
		foreach (IntersectionContainer item in _IntersectionContainers2)
		{
			if (!hasContainer(_IntersectionContainers, item))
			{
				_IntersectionContainers.Add(item);
			}
		}
		IntersectionContainerComparer comparer = new IntersectionContainerComparer(_bCompareFirstStations: true);
		if (_IntersectionContainers != null && _IntersectionContainers.Count > 0)
		{
			_IntersectionContainers.Sort(comparer);
		}
		if (_bModifiedSiteComponent)
		{
			if (RevitDefines.EQ(num8 + num7, 0.0))
			{
				eps_Leg = Eps_Leg.CopyLeg(_CurrentLeg);
			}
			else
			{
				eps_Leg3 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentLeg, num8, 1, ref eps_Leg3, TrimClosedObject: true);
				eps_Leg4 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentLeg, num7, 2, ref eps_Leg4, TrimClosedObject: true);
				eps_Leg = Eps_Leg.CopyLeg(eps_Leg3.Reverse());
				eps_Leg2 = new Eps_Leg();
				eps_Leg2.Line = new Eps_Line();
				eps_Leg2.Line.StartPnt = eps_Leg3.LegEndPnt;
				eps_Leg2.Line.EndPnt = eps_Leg4.LegEndPnt;
				eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
				eps_Leg.Last.Next = Eps_Leg.CopyLeg(eps_Leg4);
				eps_Leg2 = new Eps_Leg();
				eps_Leg2.Line = new Eps_Line();
				eps_Leg2.Line.StartPnt = eps_Leg4.LegStartPnt;
				eps_Leg2.Line.EndPnt = eps_Leg3.LegStartPnt;
				eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
			}
			TopographySurfaceContainer terrain = _ProposedSurface;
			if (eps_Leg != null && !HostLineFitsTerrain(_Document, ref terrain, _CurrentLeg, eps_Leg, ref aLines, ref nLineCount, ref aArcs, ref nArcCount, ref aLegOrder))
			{
				return false;
			}
			if (_LeftIntersectionStations != null && _RightIntersectionStations != null && _LeftIntersectionStations.Count > 0 && _LeftIntersectionStations.Count == _RightIntersectionStations.Count)
			{
				if (_PlacementSettings.IntersectionStations == null)
				{
					_PlacementSettings.IntersectionStations = new List<IntersectionStationContainer>();
				}
				_PlacementSettings.IntersectionStations.AddRange(_LeftIntersectionStations);
				_PlacementSettings.IntersectionStations.AddRange(_RightIntersectionStations);
			}
			switch (_PlacementSettings.LocateType)
			{
			case LandcaddID.Sidewalk:
			{
				SidewalkForm sidewalkForm = new SidewalkForm(_PlacementSettings.Type, _CurrentLeg);
				sidewalkForm.IntersectionContainers = _IntersectionContainers;
				_PlacementSettings.LoftFormID = sidewalkForm.CreateForm(_Document, _PlacementSettings.Name);
				if (_PlacementSettings.LoftFormID == string.Empty)
				{
					return false;
				}
				break;
			}
			case LandcaddID.RetainingWall:
			{
				RetainingWallForm retainingWallForm = new RetainingWallForm(_PlacementSettings.Type, _CurrentLeg, dElevation);
				_PlacementSettings.LoftFormID = retainingWallForm.CreateForm(_Document, _PlacementSettings.Name);
				if (_PlacementSettings.LoftFormID == string.Empty)
				{
					return false;
				}
				break;
			}
			case LandcaddID.Street:
			{
				StreetForm streetForm = new StreetForm(_PlacementSettings.Type, _CurrentLeg);
				streetForm.IntersectionContainers = _IntersectionContainers;
				string _strAdditionalLoftID = string.Empty;
				_PlacementSettings.LoftFormID = streetForm.CreateForm(_Document, _PlacementSettings.Name, ref _strAdditionalLoftID);
				_PlacementSettings.AdditionalLoftFormID = _strAdditionalLoftID;
				if (_PlacementSettings.LoftFormID == string.Empty)
				{
					return false;
				}
				break;
			}
			case LandcaddID.Curb:
			{
				CurbForm curbForm = new CurbForm(_PlacementSettings.Type, _CurrentLeg, _bAttachedToStreet: false, _PlacementSettings.UseRightSide);
				_PlacementSettings.LoftFormID = curbForm.CreateForm(_Document, _PlacementSettings.Name, _bUseInternalPoint: false, 0.0);
				if (_PlacementSettings.LoftFormID == string.Empty)
				{
					return false;
				}
				break;
			}
			}
		}
		int _nReturnValue = 0;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting("MassCreationOnlyMode", ref _nReturnValue))
		{
			ApplicationUtility.Settings.eplSetE_SetSetting("MassCreationOnlyMode", 0);
		}
		if (_nReturnValue == 0)
		{
			ProgressBarForm progressBarForm = null;
			Eps_Leg _CatchLineLeg = null;
			Eps_Leg _FeatureLineLeg = null;
			Eps_Leg _OriginalBoundaryLeg = null;
			bool flag2 = false;
			if (_TopoPointList.Count == 0)
			{
				Eps_Point pMinPnt = new Eps_Point();
				Eps_Point pMaxPnt = new Eps_Point();
				RevitGeometry.eplGeoE_FindMinMaxOfLineArcList(aLines2, nLineCount2, aArcs2, nArcCount2, ref pMinPnt, ref pMaxPnt);
				pMinPnt = new Eps_Point(pMinPnt.x, pMinPnt.y, RevitUtility.GetPlane(_Document.ActiveView.SketchPlane).Origin.Z);
				pMaxPnt = new Eps_Point(pMaxPnt.x, pMaxPnt.y, RevitUtility.GetPlane(_Document.ActiveView.SketchPlane).Origin.Z);
				double num27 = num7;
				if (num27 < num8)
				{
					num27 = num8;
				}
				double num28 = num3;
				if (num28 > num4)
				{
					num28 = num4;
				}
				double num29 = 100.0 / num28;
				double num30 = 0.0;
				for (int l = 0; l < nLineCount2; l++)
				{
					if (aLines2[l].StartPnt.z > num30)
					{
						num30 = aLines2[l].StartPnt.z;
					}
					if (aLines2[l].EndPnt.z > num30)
					{
						num30 = aLines2[l].EndPnt.z;
					}
				}
				for (int m = 0; m < nArcCount2; m++)
				{
					if (aArcs2[m].StartPnt.z > num30)
					{
						num30 = aArcs2[m].StartPnt.z;
					}
					if (aArcs2[m].EndPnt.z > num30)
					{
						num30 = aArcs2[m].EndPnt.z;
					}
				}
				double num31 = num30 * num29;
				double _dDirection = 0.0;
				EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(pMinPnt, pMaxPnt, ref _dDirection);
				EPShared.eplGeoE_CalcPolarToXY(pMinPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), num31 + num27 + 1.0, ref pMinPnt);
				EPShared.eplGeoE_CalcPolarToXY(pMaxPnt, _dDirection, num31 + num27 + 1.0, ref pMaxPnt);
				_TopoPointList.Add(pMinPnt);
				_TopoPointList.Add(new Eps_Point(pMinPnt.x, pMaxPnt.y, RevitUtility.GetPlane(_Document.ActiveView.SketchPlane).Origin.Z));
				_TopoPointList.Add(pMaxPnt);
				_TopoPointList.Add(new Eps_Point(pMaxPnt.x, pMinPnt.y, RevitUtility.GetPlane(_Document.ActiveView.SketchPlane).Origin.Z));
			}
			else
			{
				flag2 = true;
			}
			if (_PlacementSettings.LocateType != LandcaddID.FeatureLine && _PlacementSettings.LocateType != LandcaddID.PointWipeout)
			{
				if (_bModifiedSiteComponent)
				{
					progressBarForm = new ProgressBarForm();
					progressBarForm.ShowForm();
					progressBarForm.Refresh();
					progressBarForm.MinValue = 0;
					progressBarForm.MaxValue = 8;
					progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarProcessingSelectedSiteComponentPointsMessage");
				}
				GetToposurfacePoints(_CurrentLeg, aLines2, nLineCount2, aArcs2, nArcCount2, aLegOrder2, _OffsetLegWrappers, _Document.PathName, _TopoPointList, num5, num6, _dArcPointInterval, num7, num8, num9, num3, num4, _PlacementSettings.ProjectInnerSlopes, dAdjustSubgradeElev, bIsCrownPresent, bIsProjectLegToTopo, num11, num12, num10, dRightSubgradeAdjustElev, ref _AddPointList, ref _FeatureLineLeg, ref _CatchLineLeg, ref _OriginalBoundaryLeg, ref _CrownPointList, progressBarForm);
			}
			else if (_PlacementSettings.LocateType == LandcaddID.FeatureLine)
			{
				double _dDistance = 0.0;
				double num32 = 0.0;
				double num33 = 0.0;
				double num34 = 0.0;
				Eps_Point _StaOffPnt = null;
				for (Eps_Leg eps_Leg5 = _CurrentLeg; eps_Leg5 != null; eps_Leg5 = eps_Leg5.Next)
				{
					num34 = 0.0;
					if (eps_Leg5.Line != null)
					{
						EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Leg5.Line.StartPnt, eps_Leg5.Line.EndPnt, ref _dDistance);
						num32 = eps_Leg5.Line.EndPnt.z - eps_Leg5.Line.StartPnt.z;
						num33 = num32 / _dDistance;
						for (; num34 < _dDistance; num34 += num5)
						{
							EPShared.eplGeoE_FindStaOffPointAlongLine(eps_Leg5.Line, num34, 0.0, ref _StaOffPnt);
							num32 = num33 * num34;
							_StaOffPnt.z = eps_Leg5.Line.StartPnt.z + num32;
							_AddPointList.Add(_StaOffPnt);
						}
					}
					else if (eps_Leg5.Arc != null)
					{
						_dDistance = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg5.Arc.Radius, eps_Leg5.Arc.Delta);
						num32 = eps_Leg5.Arc.EndPnt.z - eps_Leg5.Arc.StartPnt.z;
						num33 = num32 / _dDistance;
						for (; num34 < _dDistance; num34 += num5)
						{
							EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg5.Arc, num34, 0.0, ref _StaOffPnt);
							num32 = num33 * num34;
							_StaOffPnt.z = eps_Leg5.Arc.StartPnt.z + num32;
							_AddPointList.Add(_StaOffPnt);
						}
					}
					if (eps_Leg5.Next == null)
					{
						_AddPointList.Add(_CurrentLeg.LegEndPnt);
					}
				}
			}
			if (flag2)
			{
				if (_PlacementSettings.LocateType == LandcaddID.PointWipeout)
				{
					_CatchLineLeg = Eps_Leg.CopyLeg(_CurrentLeg);
				}
				RevitGeometry.RemoveAllPointsInBoundary(ref _TopoPointList, _FeatureLineLeg);
				RevitGeometry.RemoveAllPointsInBoundary(ref _TopoPointList, _CatchLineLeg);
				RevitGeometry.RemoveAllPointsInBoundary(ref _TopoPointList, _OriginalBoundaryLeg);
				if (_PlacementSettings.ProjectInnerSlopes == 1 || (TerrainApi.UsesToposolids && _PlacementSettings.ProjectInnerSlopes == 0))
				{
					RevitGeometry.RemoveAllPointsInBoundary(ref _TopoPointList, _CurrentLeg);
				}
			}
			progressBarForm?.UpdateProgressBar(1);
			if (_TopoPointList.Count > 0)
			{
				_AddPointList.AddRange(_TopoPointList);
			}
			progressBarForm?.UpdateProgressBar(1);
			if (_bModifiedSiteComponent)
			{
				if ((_PlacementSettings.LocateType == LandcaddID.Curb || _PlacementSettings.LocateType == LandcaddID.ParkingLot) && CanMakeClosedPad(_CurrentLeg))
				{
					List<Eps_Point> list4 = new List<Eps_Point>();
					if (_PlacementSettings.LocateType == LandcaddID.Curb)
					{
						if (TerrainApi.UsesToposolids)
						{
							_PlacementSettings.IslandSurfaceID = CreateClosedPavementOverlay(_Document, _PlacementSettings, _CurrentLeg, _ProposedSurface);
						}
						else
						{
							list4 = _CurrentLeg.GetPointsAtIntervals(num5, 1.0);
							TopographySurface topographySurface = CreateAdditionalTopoSurface(_Document, _PlacementSettings, list4, _ProposedSurface);
							if (topographySurface != null)
							{
								_PlacementSettings.IslandSurfaceID = topographySurface.UniqueId;
							}
						}
					}
					else if (_PlacementSettings.LocateType == LandcaddID.ParkingLot)
					{
						eps_Leg3 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_CurrentLeg, 0.02, 1, ref eps_Leg3, TrimClosedObject: true);
						eps_Leg4 = new Eps_Leg();
						RevitGeometry.OffsetLegList(_CurrentLeg, 0.02, 2, ref eps_Leg4, TrimClosedObject: true);
						_CurrentLeg = ((!(RevitGeometry.eplGeoE_CalcLegListLength(eps_Leg4) < RevitGeometry.eplGeoE_CalcLegListLength(eps_Leg3))) ? eps_Leg3 : eps_Leg4);
						list4 = GetParkingLotTopoSurfacePoints(_CurrentLeg, _TopoPointList, num5, num6, _dArcPointInterval, _Document.PathName, _ProposedSurface);
						if (_ParkingLotBreaklineList != null)
						{
							Eps_Point eps_Point2 = null;
							for (int n = 0; n < _ParkingLotBreaklineList.Count; n++)
							{
								double num35 = RevitGeometry.eplGeoE_CalcLegListLength(_ParkingLotBreaklineList[n]);
								double num36 = _ParkingLotBreaklineList[n].LegEndPnt.z - _ParkingLotBreaklineList[n].LegStartPnt.z;
								double num37 = num36 / num35;
								double num38 = 0.0;
								double num39 = 0.0;
								eps_Point2 = null;
								num35 = 0.0;
								num36 = 0.0;
								double num40 = FindBreaklineIntervalFromList(_ParkingLotBreaklineList[n], _ParkingLotBreaklineElementList, _dParkingLotBreaklineLinearPointIntervals);
								for (Eps_Leg eps_Leg6 = _ParkingLotBreaklineList[n]; eps_Leg6 != null; eps_Leg6 = eps_Leg6.Next)
								{
									num38 = 0.0;
									if (eps_Leg6.Line != null)
									{
										EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Leg6.Line.StartPnt, eps_Leg6.Line.EndPnt, ref num35);
										for (; num38 < num35; num38 += num40)
										{
											EPShared.eplGeoE_FindStaOffPointAlongLine(eps_Leg6.Line, num38, 0.0, ref eps_Point2);
											num36 = num37 * (num39 + num38);
											eps_Point2.z = _ParkingLotBreaklineList[n].LegStartPnt.z + num36;
											list4.Add(eps_Point2);
											eps_Point2 = new Eps_Point(eps_Point2);
											eps_Point2.z -= num10;
											_AddPointList.Add(eps_Point2);
										}
									}
									else if (eps_Leg6.Arc != null)
									{
										for (num35 = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg6.Arc.Radius, eps_Leg6.Arc.Delta); num38 < num35; num38 += num40)
										{
											EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg6.Arc, num38, 0.0, ref eps_Point2);
											num36 = num37 * (num39 + num38);
											eps_Point2.z = _ParkingLotBreaklineList[n].LegStartPnt.z + num36;
											list4.Add(eps_Point2);
											eps_Point2 = new Eps_Point(eps_Point2);
											eps_Point2.z -= num10;
											_AddPointList.Add(eps_Point2);
										}
									}
									if (eps_Leg6.Next == null)
									{
										list4.Add(_ParkingLotBreaklineList[n].LegEndPnt);
										eps_Point2 = new Eps_Point(_ParkingLotBreaklineList[n].LegEndPnt);
										eps_Point2.z -= num10;
										_AddPointList.Add(eps_Point2);
									}
									num39 += num35;
								}
							}
						}
						_ParkingLotTopoPointList = list4;
						if (TerrainApi.UsesToposolids)
						{
							_PlacementSettings.ParkingLotSurfaceID = CreateClosedPavementOverlay(_Document, _PlacementSettings, _CurrentLeg, _ProposedSurface);
						}
						else
						{
							TopographySurface topographySurface2 = CreateAdditionalTopoSurface(_Document, _PlacementSettings, list4, _ProposedSurface);
							if (topographySurface2 != null)
							{
								_PlacementSettings.ParkingLotSurfaceID = topographySurface2.UniqueId;
								_ParkingLotSurface = new TopographySurfaceContainer(_Document.Application, topographySurface2, _bFindMinMaxPoints: true);
							}
						}
					}
				}
				if (_PlacementSettings.LocateType == LandcaddID.Curb)
				{
					_PlacementSettings.ParentTopoSurfaceID = _ProposedSurface.TopographySurface.UniqueId;
				}
				_PlacementSettings.BeginningElevationList.Clear();
				_PlacementSettings.EndingElevationList.Clear();
				bool flag3 = false;
				bool flag4 = false;
				Eps_Point eps_Point3 = null;
				Eps_Point eps_Point4 = null;
				for (Eps_Leg eps_Leg7 = _CurrentLeg; eps_Leg7 != null; eps_Leg7 = eps_Leg7.Next)
				{
					if (eps_Leg7.Line != null)
					{
						eps_Point3 = eps_Leg7.Line.StartPnt;
						eps_Point4 = eps_Leg7.Line.EndPnt;
					}
					else if (eps_Leg7.Arc != null)
					{
						eps_Point3 = eps_Leg7.Arc.StartPnt;
						eps_Point4 = eps_Leg7.Arc.EndPnt;
					}
					for (int num41 = 0; num41 < list3.Count; num41++)
					{
						if (eps_Point3.IsEqual(list3[num41]))
						{
							flag3 = true;
							if (flag4)
							{
								break;
							}
						}
						else if (eps_Point4.IsEqual(list3[num41]))
						{
							flag4 = true;
							if (flag3)
							{
								break;
							}
						}
					}
					if (eps_Point3 != null && !flag3)
					{
						_PlacementSettings.BeginningElevationList.Add(eps_Point3.z);
					}
					if (eps_Point4 != null && !flag4)
					{
						_PlacementSettings.EndingElevationList.Add(eps_Point4.z);
					}
					flag3 = false;
					flag4 = false;
				}
				Guid chainID = Guid.NewGuid();
				_FamilyInstanceList = RevitUtility.CreatePlacementSiteComponent(_Document, _PlacementSettings, _ElementList, chainID);
				_PlacementSettings.BeginningElevationList.Clear();
				_PlacementSettings.EndingElevationList.Clear();
				if (_PlacementSettings.LocateType == LandcaddID.ParkingLot && _FamilyInstanceList != null && _FamilyInstanceList.Count > 0)
				{
					RevitUtility.CreatePlacementSiteComponent(_Document, _PlacementSettings, _ParkingLotBreaklineElementList, _dParkingLotBreaklineLinearPointIntervals, chainID, _bIsParkingLotBreakline: true);
				}
			}
			IEnumerable<Eps_Point> source = _AddPointList;
			_TopoPointList = source.Distinct(new PointEqualityComparer()).ToList();
			if (progressBarForm != null)
			{
				progressBarForm.SetMaxValue();
				Thread.Sleep(300);
				progressBarForm.Close();
			}
		}
		return true;
	}

	public static void FindAllGradeAndElevationPointsInIntersection(Document _Document, TopographySurfaceContainer _ProposedSurface, EPTransactionContainer _TransactionContainer, bool _bFindIntersectionStations, FamilyInstanceComponentWrapper _CurrentFamilyInstanceComponentWrapper, List<FamilyInstanceComponentWrapper> _CommonFamilyInstanceComponentWrappers, ref List<Eps_Point> _GradeAndElevationPnts)
	{
		Eps_Point eps_Point = null;
		List<IntersectionContainer> list = new List<IntersectionContainer>();
		List<IntersectionStationContainer> _LeftIntersectionStations = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _RightIntersectionStations = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _CenterIntersectionStations = new List<IntersectionStationContainer>();
		FamilyInstance familyInstance = null;
		LandcaddID landcaddID = LandcaddID.Sidewalk;
		LandcaddID landcaddID2 = LandcaddID.Sidewalk;
		FamilyInstance familyInstance2 = null;
		FamilySymbol familySymbol = null;
		Parameter parameter = null;
		Eps_Leg ppOffsetLegList = null;
		Eps_Leg ppOffsetLegList2 = null;
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Leg eps_Leg4 = null;
		Eps_Leg eps_Leg5 = null;
		Eps_Leg eps_Leg6 = null;
		Eps_Leg Leg = null;
		List<Eps_Point> ppPointList = new List<Eps_Point>();
		int num = 0;
		int num2 = 0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		double _dReturnValue = 0.0;
		double _dReturnValue2 = 0.0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		double num10 = 0.0;
		List<double> list2 = new List<double>();
		List<double> list3 = new List<double>();
		bool flag = false;
		bool bFindIntersectionStations = true;
		familyInstance2 = _CurrentFamilyInstanceComponentWrapper.GetFirstExistingFamilyInstance();
		if (familyInstance2 != null)
		{
			parameter = familyInstance2.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (parameter != null)
			{
				foreach (int value3 in Enum.GetValues(typeof(LandcaddID)))
				{
					if (string.Compare(parameter.AsString(), value3.ToString("000"), ignoreCase: true) == 0)
					{
						landcaddID = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value3);
						break;
					}
				}
			}
			if (_CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol == null)
			{
				familySymbol = RevitUtility.FindFamilyTypeSymbol(_Document, familyInstance2, landcaddID);
				if (familySymbol != null)
				{
					_CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol = familySymbol;
				}
			}
		}
		if (landcaddID != LandcaddID.Street && landcaddID != LandcaddID.Sidewalk)
		{
			return;
		}
		switch (landcaddID)
		{
		case LandcaddID.Street:
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("StreetIntersectionDefaultRadius", ref _dReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("StreetIntersectionDefaultTransitioningDistance", ref _dReturnValue2))
			{
				_dReturnValue = 5.0;
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
			if (parameter != null)
			{
				num9 = parameter.AsDouble();
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
			if (parameter != null)
			{
				num3 = num9 * (double)parameter.AsInteger();
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
			if (parameter != null)
			{
				num6 = (0.0 - num4) / 2.0 * parameter.AsDouble();
			}
			break;
		case LandcaddID.Sidewalk:
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("SideWalkIntersectionDefaultRadius", ref _dReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("SideWalkIntersectionDefaultTransitioningDistance", ref _dReturnValue2))
			{
				_dReturnValue = 5.0;
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
			if (parameter != null)
			{
				num3 = parameter.AsDouble();
			}
			break;
		}
		if (_CommonFamilyInstanceComponentWrappers == null)
		{
			return;
		}
		for (int i = 0; i < _CommonFamilyInstanceComponentWrappers.Count; i++)
		{
			if (_CurrentFamilyInstanceComponentWrapper.ChainID == _CommonFamilyInstanceComponentWrappers[i].ChainID)
			{
				continue;
			}
			if (i == 0)
			{
				if (_CurrentFamilyInstanceComponentWrapper.InstanceLeg == null)
				{
					_CurrentFamilyInstanceComponentWrapper.CreateProposedLeg(_ProposedSurface);
				}
				ppOffsetLegList = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentFamilyInstanceComponentWrapper.InstanceLeg, num3 / 2.0, 1, ref ppOffsetLegList, TrimClosedObject: true);
				ppOffsetLegList2 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentFamilyInstanceComponentWrapper.InstanceLeg, num3 / 2.0, 2, ref ppOffsetLegList2, TrimClosedObject: true);
			}
			familyInstance = _CommonFamilyInstanceComponentWrappers[i].GetFirstExistingFamilyInstance();
			if (familyInstance == null)
			{
				continue;
			}
			parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (parameter != null)
			{
				foreach (int value4 in Enum.GetValues(typeof(LandcaddID)))
				{
					if (string.Compare(parameter.AsString(), value4.ToString("000"), ignoreCase: true) == 0)
					{
						landcaddID2 = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value4);
						break;
					}
				}
			}
			if (landcaddID != landcaddID2)
			{
				continue;
			}
			if (_CommonFamilyInstanceComponentWrappers[i].InstanceLeg == null)
			{
				_CommonFamilyInstanceComponentWrappers[i].CreateProposedLeg(_ProposedSurface);
			}
			if (_CommonFamilyInstanceComponentWrappers[i].InstanceLeg == null)
			{
				continue;
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegStartPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegStartPnt, num3 / 2.0))
			{
				EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegEndPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegStartPnt, num3 / 2.0);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegEndPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegEndPnt, num3 / 2.0))
			{
				EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegStartPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegEndPnt, num3 / 2.0);
			}
			num = 0;
			if (RevitGeometry.eplGeoE_IntersectTwoLegLists(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg, _CurrentFamilyInstanceComponentWrapper.InstanceLeg, ref ppPointList, ref num, num3 / 2.0) != 0 || num <= 0)
			{
				continue;
			}
			familySymbol = RevitUtility.FindFamilyTypeSymbol(_Document, familyInstance, landcaddID2);
			if (familySymbol == null)
			{
				continue;
			}
			_CommonFamilyInstanceComponentWrappers[i].ProposedFamilySymbol = familySymbol;
			switch (landcaddID2)
			{
			case LandcaddID.Street:
				num9 = 0.0;
				parameter = familySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
				if (parameter != null)
				{
					num9 = parameter.AsDouble();
				}
				parameter = familySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
				if (parameter != null)
				{
					num4 = num9 * (double)parameter.AsInteger();
				}
				parameter = familySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
				if (parameter != null)
				{
					num7 = (0.0 - num4) / 2.0 * parameter.AsDouble();
				}
				break;
			case LandcaddID.Sidewalk:
				parameter = familySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
				if (parameter != null)
				{
					num4 = parameter.AsDouble();
				}
				break;
			}
			if (!(num4 > 0.0))
			{
				continue;
			}
			Eps_Leg ppOffsetLegList3 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg, num4 / 2.0, 1, ref ppOffsetLegList3, TrimClosedObject: true);
			Eps_Leg ppOffsetLegList4 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg, num4 / 2.0, 2, ref ppOffsetLegList4, TrimClosedObject: true);
			for (int num11 = _CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations.Count - 1; num11 >= 0; num11--)
			{
				if (string.Compare(familyInstance.UniqueId, _CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations[num11].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance2.UniqueId, _CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations[num11].SecondaryPlacementId, ignoreCase: true) == 0)
				{
					flag = false;
					bFindIntersectionStations = _bFindIntersectionStations;
					_LeftIntersectionStations.Add(_CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations[num11]);
				}
				else if (string.Compare(familyInstance2.UniqueId, _CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations[num11].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance.UniqueId, _CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations[num11].SecondaryPlacementId, ignoreCase: true) == 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			for (int num12 = _CurrentFamilyInstanceComponentWrapper.RightIntersectionStations.Count - 1; num12 >= 0; num12--)
			{
				if (string.Compare(familyInstance.UniqueId, _CurrentFamilyInstanceComponentWrapper.RightIntersectionStations[num12].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance2.UniqueId, _CurrentFamilyInstanceComponentWrapper.RightIntersectionStations[num12].SecondaryPlacementId, ignoreCase: true) == 0)
				{
					flag = false;
					bFindIntersectionStations = _bFindIntersectionStations;
					_RightIntersectionStations.Add(_CurrentFamilyInstanceComponentWrapper.RightIntersectionStations[num12]);
				}
				else if (string.Compare(familyInstance2.UniqueId, _CurrentFamilyInstanceComponentWrapper.RightIntersectionStations[num12].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance.UniqueId, _CurrentFamilyInstanceComponentWrapper.RightIntersectionStations[num12].SecondaryPlacementId, ignoreCase: true) == 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			eps_Leg = _CommonFamilyInstanceComponentWrappers[i].InstanceLeg;
			eps_Leg4 = ppOffsetLegList4;
			eps_Leg3 = ppOffsetLegList3;
			eps_Leg2 = _CurrentFamilyInstanceComponentWrapper.InstanceLeg;
			eps_Leg6 = ppOffsetLegList2;
			eps_Leg5 = ppOffsetLegList;
			num5 = num4;
			num8 = num7;
			if (!FindIntersectionStationAndProfiles(eps_Leg2, eps_Leg6, eps_Leg5, eps_Leg, eps_Leg4, eps_Leg3, num5, num8, _dReturnValue, _dReturnValue2, bFindIntersectionStations, _bDisplayPromptWhenNecessary: false, 0.0, _Document, _TransactionContainer, landcaddID2, ref _LeftIntersectionStations, ref _CenterIntersectionStations, ref _RightIntersectionStations))
			{
				break;
			}
			bFindIntersectionStations = true;
			IntersectionStationContainerComparer comparer = new IntersectionStationContainerComparer(_bCompareFirstStations: true);
			if (_CenterIntersectionStations != null && _CenterIntersectionStations.Count > 0)
			{
				_CenterIntersectionStations.Sort(comparer);
			}
			for (int j = 0; j < _CenterIntersectionStations.Count; j++)
			{
				for (int k = 0; k < _CenterIntersectionStations[j].IntersectionStations.Count; k++)
				{
					if (_CenterIntersectionStations[j].IntersectionStations[k].StartingIntersection)
					{
						list2.Add(_CenterIntersectionStations[j].IntersectionStations[k].Station);
						if (k == _CenterIntersectionStations[j].IntersectionStations.Count - 1)
						{
							list3.Add(RevitGeometry.eplGeoE_CalcLegListLength(_CurrentFamilyInstanceComponentWrapper.InstanceLeg));
						}
					}
					else
					{
						if (k == 0)
						{
							list2.Add(0.0);
						}
						list3.Add(_CenterIntersectionStations[j].IntersectionStations[k].Station);
					}
				}
			}
			for (int l = 0; l < list2.Count; l++)
			{
				eps_Point = new Eps_Point();
				if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_CurrentFamilyInstanceComponentWrapper.InstanceLeg, list2[l], 0.0, ref eps_Point, ref Leg) == 0)
				{
					eps_Point = new Eps_Point();
					num10 = RevitGeometry.eplGeoE_CalcLegLength(Leg);
					if (Leg.Line != null)
					{
						EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(Leg.Line.StartPnt, Leg.Line.EndPnt, ref eps_Point);
					}
					else if (Leg.Arc != null)
					{
						EPShared.eplGeoE_FindStaOffPointAlongArc(Leg.Arc, num10 / 2.0, 0.0, ref eps_Point);
					}
					for (num2 = 0; num2 < _GradeAndElevationPnts.Count && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Point, _GradeAndElevationPnts[num2]); num2++)
					{
					}
					if (num2 == _GradeAndElevationPnts.Count)
					{
						_GradeAndElevationPnts.Add(eps_Point);
					}
				}
				eps_Point = new Eps_Point();
				if (RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(_CurrentFamilyInstanceComponentWrapper.InstanceLeg, list3[l], 0.0, ref eps_Point, ref Leg) != 0)
				{
					continue;
				}
				eps_Point = new Eps_Point();
				num10 = RevitGeometry.eplGeoE_CalcLegLength(Leg);
				if (Leg.Line != null)
				{
					EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(Leg.Line.StartPnt, Leg.Line.EndPnt, ref eps_Point);
				}
				else if (Leg.Arc != null)
				{
					EPShared.eplGeoE_FindStaOffPointAlongArc(Leg.Arc, num10 / 2.0, 0.0, ref eps_Point);
				}
				for (num2 = 0; num2 < _GradeAndElevationPnts.Count && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Point, _GradeAndElevationPnts[num2]); num2++)
				{
				}
				if (num2 == _GradeAndElevationPnts.Count)
				{
					_GradeAndElevationPnts.Add(eps_Point);
				}
				for (Eps_Leg eps_Leg7 = _CurrentFamilyInstanceComponentWrapper.InstanceLeg; eps_Leg7 != null; eps_Leg7 = eps_Leg7.Next)
				{
					if (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg7.LegStartPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg, ref _dStation, ref _dOffset) == 1700 && _dStation >= list2[l] && _dStation <= list3[l])
					{
						for (num2 = 0; num2 < _GradeAndElevationPnts.Count && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg7.LegStartPnt, _GradeAndElevationPnts[num2]); num2++)
						{
						}
						if (num2 == _GradeAndElevationPnts.Count)
						{
							_GradeAndElevationPnts.Add(eps_Leg7.LegStartPnt);
						}
					}
					if (eps_Leg7.Next == null && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Leg7.LegEndPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg, ref _dStation, ref _dOffset) == 1700 && _dStation >= list2[l] && _dStation <= list3[l])
					{
						for (num2 = 0; num2 < _GradeAndElevationPnts.Count && !EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg7.LegEndPnt, _GradeAndElevationPnts[num2]); num2++)
						{
						}
						if (num2 == _GradeAndElevationPnts.Count)
						{
							_GradeAndElevationPnts.Add(eps_Leg7.LegEndPnt);
						}
					}
				}
			}
			_LeftIntersectionStations.Clear();
			_CenterIntersectionStations.Clear();
			_RightIntersectionStations.Clear();
		}
	}

	public static void RemoveAllIntersectionStationsForCommonSiteComponents(ref List<FamilyInstanceComponentWrapper> _CommonFamilyInstanceComponentWrappers)
	{
		for (int i = 0; i < _CommonFamilyInstanceComponentWrappers.Count; i++)
		{
			_CommonFamilyInstanceComponentWrappers[i].LeftIntersectionStations.Clear();
			_CommonFamilyInstanceComponentWrappers[i].RightIntersectionStations.Clear();
			_CommonFamilyInstanceComponentWrappers[i].CenterIntersectionStations.Clear();
			_CommonFamilyInstanceComponentWrappers[i].IntersectionContainerList.Clear();
		}
	}

	public static void SetupAllIntersectionStationsForSelectedSiteComponent(Document _Document, TopographySurfaceContainer _ProposedSurface, EPTransactionContainer _TransactionContainer, bool _bFindIntersectionStations, ref FamilyInstanceComponentWrapper _CurrentFamilyInstanceComponentWrapper, ref List<FamilyInstanceComponentWrapper> _CommonFamilyInstanceComponentWrappers)
	{
		Eps_Point eps_Point = null;
		ProfileContainer profileContainer = null;
		IntersectionContainer intersectionContainer = null;
		List<IntersectionContainer> list = new List<IntersectionContainer>();
		List<IntersectionStationContainer> _LeftIntersectionStations = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _RightIntersectionStations = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _CenterIntersectionStations = new List<IntersectionStationContainer>();
		LandcaddID landcaddID = LandcaddID.Sidewalk;
		LandcaddID landcaddID2 = LandcaddID.Sidewalk;
		FamilyInstance familyInstance = null;
		FamilyInstance familyInstance2 = null;
		FamilySymbol familySymbol = null;
		Parameter parameter = null;
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Leg eps_Leg4 = null;
		Eps_Leg eps_Leg5 = null;
		Eps_Leg eps_Leg6 = null;
		Eps_Leg eps_Leg7 = null;
		Eps_Leg eps_Leg8 = null;
		List<Eps_Point> ppPointList = new List<Eps_Point>();
		int num = 0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double _dReturnValue = 0.0;
		double _dReturnValue2 = 0.0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		bool flag = false;
		bool flag2 = true;
		bool flag3 = false;
		familyInstance2 = _CurrentFamilyInstanceComponentWrapper.GetFirstExistingFamilyInstance();
		if (familyInstance2 != null)
		{
			parameter = familyInstance2.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (parameter != null)
			{
				foreach (int value3 in Enum.GetValues(typeof(LandcaddID)))
				{
					if (string.Compare(parameter.AsString(), value3.ToString("000"), ignoreCase: true) == 0)
					{
						landcaddID = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value3);
						break;
					}
				}
			}
			if (_CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol == null)
			{
				familySymbol = RevitUtility.FindFamilyTypeSymbol(_Document, familyInstance2, landcaddID);
				if (familySymbol != null)
				{
					_CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol = familySymbol;
				}
			}
		}
		if (landcaddID != LandcaddID.Street && landcaddID != LandcaddID.Sidewalk)
		{
			return;
		}
		switch (landcaddID)
		{
		case LandcaddID.Street:
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("StreetIntersectionDefaultRadius", ref _dReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("StreetIntersectionDefaultTransitioningDistance", ref _dReturnValue2))
			{
				_dReturnValue = 5.0;
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
			if (parameter != null)
			{
				num8 = parameter.AsDouble();
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
			if (parameter != null)
			{
				num2 = num8 * (double)parameter.AsInteger();
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
			if (parameter != null)
			{
				num5 = (0.0 - num2) / 2.0 * parameter.AsDouble();
			}
			break;
		case LandcaddID.Sidewalk:
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("SideWalkIntersectionDefaultRadius", ref _dReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("SideWalkIntersectionDefaultTransitioningDistance", ref _dReturnValue2))
			{
				_dReturnValue = 5.0;
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
			if (parameter != null)
			{
				num2 = parameter.AsDouble();
			}
			break;
		}
		if (_CommonFamilyInstanceComponentWrappers != null)
		{
			if (_CurrentFamilyInstanceComponentWrapper.InstanceLeg == null)
			{
				_CurrentFamilyInstanceComponentWrapper.CreateProposedLeg(_ProposedSurface);
				if (_CurrentFamilyInstanceComponentWrapper.InstanceLeg == null)
				{
					return;
				}
			}
			eps_Leg = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentFamilyInstanceComponentWrapper.InstanceLeg, num2 / 2.0, 1, ref eps_Leg, TrimClosedObject: true);
			eps_Leg2 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentFamilyInstanceComponentWrapper.InstanceLeg, num2 / 2.0, 2, ref eps_Leg2, TrimClosedObject: true);
			for (int i = 0; i < _CommonFamilyInstanceComponentWrappers.Count; i++)
			{
				if (_CurrentFamilyInstanceComponentWrapper.ChainID == _CommonFamilyInstanceComponentWrappers[i].ChainID)
				{
					continue;
				}
				familyInstance = _CommonFamilyInstanceComponentWrappers[i].GetFirstExistingFamilyInstance();
				if (familyInstance == null)
				{
					continue;
				}
				parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
				if (parameter != null)
				{
					foreach (int value4 in Enum.GetValues(typeof(LandcaddID)))
					{
						if (string.Compare(parameter.AsString(), value4.ToString("000"), ignoreCase: true) == 0)
						{
							landcaddID2 = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value4);
							break;
						}
					}
				}
				if (landcaddID != landcaddID2)
				{
					continue;
				}
				if (_CommonFamilyInstanceComponentWrappers[i].InstanceLeg == null)
				{
					_CommonFamilyInstanceComponentWrappers[i].CreateProposedLeg(_ProposedSurface);
					if (_CommonFamilyInstanceComponentWrappers[i].InstanceLeg == null)
					{
						continue;
					}
				}
				if (_CommonFamilyInstanceComponentWrappers[i].InstanceLeg == null)
				{
					continue;
				}
				if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegStartPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegStartPnt, num2 / 2.0))
				{
					EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegEndPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegStartPnt, num2 / 2.0);
				}
				if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegEndPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegEndPnt, num2 / 2.0))
				{
					EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegStartPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegEndPnt, num2 / 2.0);
				}
				num = 0;
				if (RevitGeometry.eplGeoE_IntersectTwoLegLists(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg, _CurrentFamilyInstanceComponentWrapper.InstanceLeg, ref ppPointList, ref num, num2 / 2.0) != 0 || num <= 0)
				{
					continue;
				}
				familySymbol = RevitUtility.FindFamilyTypeSymbol(_Document, familyInstance, landcaddID2);
				if (familySymbol == null)
				{
					continue;
				}
				_CommonFamilyInstanceComponentWrappers[i].ProposedFamilySymbol = familySymbol;
				switch (landcaddID2)
				{
				case LandcaddID.Street:
					num8 = 0.0;
					parameter = familySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
					if (parameter != null)
					{
						num8 = parameter.AsDouble();
					}
					parameter = familySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
					if (parameter != null)
					{
						num3 = num8 * (double)parameter.AsInteger();
					}
					parameter = familySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
					if (parameter != null)
					{
						num6 = (0.0 - num3) / 2.0 * parameter.AsDouble();
					}
					break;
				case LandcaddID.Sidewalk:
					parameter = familySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
					if (parameter != null)
					{
						num3 = parameter.AsDouble();
					}
					break;
				}
				if (!(num3 > 0.0))
				{
					continue;
				}
				Eps_Leg ppOffsetLegList = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg, num3 / 2.0, 1, ref ppOffsetLegList, TrimClosedObject: true);
				Eps_Leg ppOffsetLegList2 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg, num3 / 2.0, 2, ref ppOffsetLegList2, TrimClosedObject: true);
				for (int num9 = _CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations.Count - 1; num9 >= 0; num9--)
				{
					if (string.Compare(familyInstance.UniqueId, _CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations[num9].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance2.UniqueId, _CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations[num9].SecondaryPlacementId, ignoreCase: true) == 0)
					{
						flag = false;
						flag2 = _bFindIntersectionStations;
						_LeftIntersectionStations.Add(_CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations[num9]);
						if (flag2)
						{
							_CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations.RemoveAt(num9);
						}
					}
					else if (string.Compare(familyInstance2.UniqueId, _CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations[num9].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance.UniqueId, _CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations[num9].SecondaryPlacementId, ignoreCase: true) == 0)
					{
						flag = true;
						flag2 = _bFindIntersectionStations;
						_LeftIntersectionStations.Add(_CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations[num9]);
						_CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations.RemoveAt(num9);
					}
				}
				for (int num10 = _CurrentFamilyInstanceComponentWrapper.RightIntersectionStations.Count - 1; num10 >= 0; num10--)
				{
					if (string.Compare(familyInstance.UniqueId, _CurrentFamilyInstanceComponentWrapper.RightIntersectionStations[num10].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance2.UniqueId, _CurrentFamilyInstanceComponentWrapper.RightIntersectionStations[num10].SecondaryPlacementId, ignoreCase: true) == 0)
					{
						flag = false;
						flag2 = _bFindIntersectionStations;
						_RightIntersectionStations.Add(_CurrentFamilyInstanceComponentWrapper.RightIntersectionStations[num10]);
						if (flag2)
						{
							_CurrentFamilyInstanceComponentWrapper.RightIntersectionStations.RemoveAt(num10);
						}
					}
					else if (string.Compare(familyInstance2.UniqueId, _CurrentFamilyInstanceComponentWrapper.RightIntersectionStations[num10].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance.UniqueId, _CurrentFamilyInstanceComponentWrapper.RightIntersectionStations[num10].SecondaryPlacementId, ignoreCase: true) == 0)
					{
						flag = true;
						flag2 = _bFindIntersectionStations;
						_RightIntersectionStations.Add(_CurrentFamilyInstanceComponentWrapper.RightIntersectionStations[num10]);
						_CurrentFamilyInstanceComponentWrapper.RightIntersectionStations.RemoveAt(num10);
					}
				}
				flag3 = false;
				if (landcaddID2 == LandcaddID.Street)
				{
					parameter = familySymbol.LookupParameter(RevitParameters.StreetCurbDisplayParameter);
					if (parameter != null)
					{
						flag3 = Convert.ToBoolean(parameter.AsInteger());
					}
				}
				if (_CurrentFamilyInstanceComponentWrapper.ProposedElementSet == null)
				{
					_CommonFamilyInstanceComponentWrappers[i].LeftIntersectionStations.Clear();
					_CommonFamilyInstanceComponentWrappers[i].CenterIntersectionStations.Clear();
					_CommonFamilyInstanceComponentWrappers[i].RightIntersectionStations.Clear();
					_CommonFamilyInstanceComponentWrappers[i].IntersectionContainerList.Clear();
					if (flag || flag3)
					{
						_CommonFamilyInstanceComponentWrappers[i].RedrawForm = true;
					}
					continue;
				}
				if (flag)
				{
					eps_Leg3 = _CurrentFamilyInstanceComponentWrapper.InstanceLeg;
					eps_Leg6 = eps_Leg2;
					eps_Leg5 = eps_Leg;
					eps_Leg4 = _CommonFamilyInstanceComponentWrappers[i].InstanceLeg;
					eps_Leg8 = ppOffsetLegList2;
					eps_Leg7 = ppOffsetLegList;
					num4 = num2;
					num7 = num5;
				}
				else
				{
					FamilyInstanceComponentWrapper _CurFamilyInstanceComponentWrapper = _CommonFamilyInstanceComponentWrappers[i];
					SetupIntersectionContainersForSiteComponent(_Document, _TransactionContainer, _ProposedSurface, _CurrentFamilyInstanceComponentWrapper.ChainID, _CommonFamilyInstanceComponentWrappers, ref _CurFamilyInstanceComponentWrapper);
					_CommonFamilyInstanceComponentWrappers[i] = _CurFamilyInstanceComponentWrapper;
					eps_Leg3 = _CommonFamilyInstanceComponentWrappers[i].InstanceLeg;
					eps_Leg6 = ppOffsetLegList2;
					eps_Leg5 = ppOffsetLegList;
					eps_Leg4 = _CurrentFamilyInstanceComponentWrapper.InstanceLeg;
					eps_Leg8 = eps_Leg2;
					eps_Leg7 = eps_Leg;
					num4 = num3;
					num7 = num6;
				}
				if (!FindIntersectionStationAndProfiles(eps_Leg4, eps_Leg8, eps_Leg7, eps_Leg3, eps_Leg6, eps_Leg5, num4, num7, _dReturnValue, _dReturnValue2, flag2, _bDisplayPromptWhenNecessary: true, 0.0, _Document, _TransactionContainer, landcaddID2, ref _LeftIntersectionStations, ref _CenterIntersectionStations, ref _RightIntersectionStations))
				{
					return;
				}
				IntersectionStationContainerComparer comparer = new IntersectionStationContainerComparer(_bCompareFirstStations: true);
				if (_LeftIntersectionStations != null && _LeftIntersectionStations.Count > 0)
				{
					_LeftIntersectionStations.Sort(comparer);
				}
				if (_CenterIntersectionStations != null && _CenterIntersectionStations.Count > 0)
				{
					_CenterIntersectionStations.Sort(comparer);
				}
				if (_RightIntersectionStations != null && _RightIntersectionStations.Count > 0)
				{
					_RightIntersectionStations.Sort(comparer);
				}
				if (flag)
				{
					_CommonFamilyInstanceComponentWrappers[i].LeftIntersectionStations.AddRange(_LeftIntersectionStations);
					_CommonFamilyInstanceComponentWrappers[i].CenterIntersectionStations.AddRange(_CenterIntersectionStations);
					_CommonFamilyInstanceComponentWrappers[i].RightIntersectionStations.AddRange(_RightIntersectionStations);
					_CommonFamilyInstanceComponentWrappers[i].RedrawForm = true;
				}
				else
				{
					_CurrentFamilyInstanceComponentWrapper.CenterIntersectionStations.AddRange(_CenterIntersectionStations);
					if (flag2)
					{
						_CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations.AddRange(_LeftIntersectionStations);
						_CurrentFamilyInstanceComponentWrapper.RightIntersectionStations.AddRange(_RightIntersectionStations);
					}
				}
				flag2 = true;
				if (_LeftIntersectionStations.Count > 0 && _LeftIntersectionStations.Count == _RightIntersectionStations.Count)
				{
					bool flag4 = true;
					for (int j = 0; j < _LeftIntersectionStations.Count; j++)
					{
						_LeftIntersectionStations[j].PrimaryPlacementId = familyInstance.UniqueId;
						_RightIntersectionStations[j].PrimaryPlacementId = familyInstance.UniqueId;
						if (_LeftIntersectionStations[j].IntersectionStations.Count != _RightIntersectionStations[j].IntersectionStations.Count)
						{
							continue;
						}
						for (int k = 0; k < _LeftIntersectionStations[j].IntersectionStations.Count; k++)
						{
							intersectionContainer = new IntersectionContainer();
							intersectionContainer.StartingIntersection = true;
							profileContainer = new ProfileContainer();
							if (_LeftIntersectionStations[j].IntersectionStations[k].PrimaryStation <= _RightIntersectionStations[j].IntersectionStations[k].PrimaryStation)
							{
								flag4 = _LeftIntersectionStations[j].IntersectionStations[k].IsPrimaryLeft;
								eps_Point = GetIntersectionStationPoint(_LeftIntersectionStations, j, k, _bFindEndsOfIntersection: false);
								if ((eps_Point != null && flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg6, ref _dStation, ref _dOffset) == 1700) || (!flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg5, ref _dStation, ref _dOffset) == 1700))
								{
									profileContainer.Station = _dStation;
								}
								else
								{
									profileContainer.Station = _LeftIntersectionStations[j].IntersectionStations[k].PrimaryStation;
								}
							}
							else
							{
								flag4 = _RightIntersectionStations[j].IntersectionStations[k].IsPrimaryLeft;
								eps_Point = GetIntersectionStationPoint(_RightIntersectionStations, j, k, _bFindEndsOfIntersection: false);
								if ((eps_Point != null && flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg6, ref _dStation, ref _dOffset) == 1700) || (!flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg5, ref _dStation, ref _dOffset) == 1700))
								{
									profileContainer.Station = _dStation;
								}
								else
								{
									profileContainer.Station = _RightIntersectionStations[j].IntersectionStations[k].PrimaryStation;
								}
							}
							intersectionContainer.Profiles.Add(profileContainer);
							if (flag4)
							{
								intersectionContainer.ProfileUsage = ProfileUsage.LeftOnly;
							}
							else
							{
								intersectionContainer.ProfileUsage = ProfileUsage.RightOnly;
							}
							list.Add(intersectionContainer);
							intersectionContainer = new IntersectionContainer();
							intersectionContainer.StartingIntersection = false;
							profileContainer = new ProfileContainer();
							if (_LeftIntersectionStations[j].IntersectionStations[k].PrimaryStation <= _RightIntersectionStations[j].IntersectionStations[k].PrimaryStation)
							{
								flag4 = _RightIntersectionStations[j].IntersectionStations[k].IsPrimaryLeft;
								eps_Point = GetIntersectionStationPoint(_RightIntersectionStations, j, k, _bFindEndsOfIntersection: false);
								if ((eps_Point != null && flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg6, ref _dStation, ref _dOffset) == 1700) || (!flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg5, ref _dStation, ref _dOffset) == 1700))
								{
									profileContainer.Station = _dStation;
								}
								else
								{
									profileContainer.Station = _RightIntersectionStations[j].IntersectionStations[k].PrimaryStation;
								}
							}
							else
							{
								flag4 = _LeftIntersectionStations[j].IntersectionStations[k].IsPrimaryLeft;
								eps_Point = GetIntersectionStationPoint(_LeftIntersectionStations, j, k, _bFindEndsOfIntersection: false);
								if ((eps_Point != null && flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg6, ref _dStation, ref _dOffset) == 1700) || (!flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg5, ref _dStation, ref _dOffset) == 1700))
								{
									profileContainer.Station = _dStation;
								}
								else
								{
									profileContainer.Station = _LeftIntersectionStations[j].IntersectionStations[k].PrimaryStation;
								}
							}
							intersectionContainer.Profiles.Add(profileContainer);
							if (flag4)
							{
								intersectionContainer.ProfileUsage = ProfileUsage.LeftOnly;
							}
							else
							{
								intersectionContainer.ProfileUsage = ProfileUsage.RightOnly;
							}
							list.Add(intersectionContainer);
						}
					}
					IntersectionContainerComparer comparer2 = new IntersectionContainerComparer(_bCompareFirstStations: true);
					if (list != null && list.Count > 0)
					{
						list.Sort(comparer2);
					}
					if (flag)
					{
						_CurrentFamilyInstanceComponentWrapper.IntersectionContainerList.AddRange(list);
					}
					else
					{
						_CommonFamilyInstanceComponentWrappers[i].IntersectionContainerList.AddRange(list);
						_CommonFamilyInstanceComponentWrappers[i].IntersectionContainerList.Sort(comparer2);
						_CommonFamilyInstanceComponentWrappers[i].RedrawForm = true;
					}
					list.Clear();
				}
				_LeftIntersectionStations.Clear();
				_CenterIntersectionStations.Clear();
				_RightIntersectionStations.Clear();
			}
		}
		if (_CurrentFamilyInstanceComponentWrapper.ProposedElementSet == null)
		{
			_CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations.Clear();
			_CurrentFamilyInstanceComponentWrapper.CenterIntersectionStations.Clear();
			_CurrentFamilyInstanceComponentWrapper.RightIntersectionStations.Clear();
			_CurrentFamilyInstanceComponentWrapper.IntersectionContainerList.Clear();
			_CurrentFamilyInstanceComponentWrapper.ProposedLeg = null;
		}
	}

	public static void SetupAllIntersectionStationsForAllSiteComponents(Document _Document, TopographySurfaceContainer _ProposedSurface, EPTransactionContainer _TransactionContainer, string _IgnoreChainID, ref List<FamilyInstanceComponentWrapper> _CommonFamilyInstanceComponentWrappers)
	{
		List<IntersectionStationContainer> list = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> list2 = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _LeftIntersectionStations = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _RightIntersectionStations = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _CenterIntersectionStations = new List<IntersectionStationContainer>();
		LandcaddID landcaddID = LandcaddID.Sidewalk;
		LandcaddID landcaddID2 = LandcaddID.Sidewalk;
		FamilyInstance familyInstance = null;
		FamilyInstance familyInstance2 = null;
		FamilySymbol familySymbol = null;
		Parameter parameter = null;
		Eps_Leg ppOffsetLegList = null;
		Eps_Leg ppOffsetLegList2 = null;
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Leg eps_Leg4 = null;
		Eps_Leg eps_Leg5 = null;
		Eps_Leg eps_Leg6 = null;
		List<Eps_Point> ppPointList = new List<Eps_Point>();
		int num = 0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double _dReturnValue = 0.0;
		double _dReturnValue2 = 0.0;
		if (_CommonFamilyInstanceComponentWrappers == null)
		{
			return;
		}
		for (int i = 0; i < _CommonFamilyInstanceComponentWrappers.Count; i++)
		{
			if (string.Compare(_CommonFamilyInstanceComponentWrappers[i].ChainID, _IgnoreChainID, ignoreCase: true) == 0)
			{
				continue;
			}
			familyInstance2 = _CommonFamilyInstanceComponentWrappers[i].GetFirstExistingFamilyInstance();
			if (familyInstance2 != null)
			{
				parameter = familyInstance2.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
				if (parameter != null)
				{
					foreach (int value3 in Enum.GetValues(typeof(LandcaddID)))
					{
						if (string.Compare(parameter.AsString(), value3.ToString("000"), ignoreCase: true) == 0)
						{
							landcaddID = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value3);
							break;
						}
					}
				}
				if (landcaddID != LandcaddID.Street && landcaddID != LandcaddID.Sidewalk)
				{
					continue;
				}
				if (_CommonFamilyInstanceComponentWrappers[i].ProposedFamilySymbol == null)
				{
					familySymbol = RevitUtility.FindFamilyTypeSymbol(_Document, familyInstance2, landcaddID);
					if (familySymbol != null)
					{
						_CommonFamilyInstanceComponentWrappers[i].ProposedFamilySymbol = familySymbol;
					}
				}
			}
			switch (landcaddID)
			{
			case LandcaddID.Street:
				if (!ApplicationUtility.Settings.eplSetE_GetSetting("StreetIntersectionDefaultRadius", ref _dReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("StreetIntersectionDefaultTransitioningDistance", ref _dReturnValue2))
				{
					_dReturnValue = 5.0;
				}
				parameter = _CommonFamilyInstanceComponentWrappers[i].ProposedFamilySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
				if (parameter != null)
				{
					num8 = parameter.AsDouble();
				}
				parameter = _CommonFamilyInstanceComponentWrappers[i].ProposedFamilySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
				if (parameter != null)
				{
					num2 = num8 * (double)parameter.AsInteger();
				}
				parameter = _CommonFamilyInstanceComponentWrappers[i].ProposedFamilySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
				if (parameter != null)
				{
					num5 = (0.0 - num3) / 2.0 * parameter.AsDouble();
				}
				break;
			case LandcaddID.Sidewalk:
				if (!ApplicationUtility.Settings.eplSetE_GetSetting("SideWalkIntersectionDefaultRadius", ref _dReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("SideWalkIntersectionDefaultTransitioningDistance", ref _dReturnValue2))
				{
					_dReturnValue = 5.0;
				}
				parameter = _CommonFamilyInstanceComponentWrappers[i].ProposedFamilySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
				if (parameter != null)
				{
					num2 = parameter.AsDouble();
				}
				break;
			}
			if (num2 > 0.0)
			{
				if (_CommonFamilyInstanceComponentWrappers[i].InstanceLeg == null)
				{
					_CommonFamilyInstanceComponentWrappers[i].CreateProposedLeg(_ProposedSurface);
					if (_CommonFamilyInstanceComponentWrappers[i].InstanceLeg == null)
					{
						continue;
					}
				}
				ppOffsetLegList = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg, num2 / 2.0, 1, ref ppOffsetLegList, TrimClosedObject: true);
				ppOffsetLegList2 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg, num2 / 2.0, 2, ref ppOffsetLegList2, TrimClosedObject: true);
			}
			ElementSet familyInstanceElementSet = _CommonFamilyInstanceComponentWrappers[i].GetFamilyInstanceElementSet();
			if (familyInstanceElementSet != null && familyInstanceElementSet.Size > 0)
			{
				List<IntersectionStationContainer> _LeftIntersectionStations2 = null;
				List<IntersectionStationContainer> _RightIntersectionStations2 = null;
				List<FamilyInstance> _SiteFamilyInstanceList = null;
				foreach (Element item in familyInstanceElementSet)
				{
					GetIntersections(_Document, item.UniqueId, _bOnlyFindSecondaryFamilies: true, ref _SiteFamilyInstanceList, ref _LeftIntersectionStations2, ref _RightIntersectionStations2);
					if (_SiteFamilyInstanceList != null && _SiteFamilyInstanceList.Count > 0)
					{
						_CommonFamilyInstanceComponentWrappers[i].AddIntersectionFamilyInstances(_SiteFamilyInstanceList);
						_CommonFamilyInstanceComponentWrappers[i].IntersectionFamilySymbol = _SiteFamilyInstanceList[0].Symbol;
					}
					if (_LeftIntersectionStations2 != null && _LeftIntersectionStations2.Count > 0)
					{
						list.AddRange(_LeftIntersectionStations2);
					}
					if (_RightIntersectionStations2 != null && _RightIntersectionStations2.Count > 0)
					{
						list2.AddRange(_RightIntersectionStations2);
					}
				}
			}
			if (list.Count == 0 || list2.Count == 0)
			{
				continue;
			}
			for (int j = 0; j < _CommonFamilyInstanceComponentWrappers.Count; j++)
			{
				if (i == j || string.Compare(_CommonFamilyInstanceComponentWrappers[j].ChainID, _IgnoreChainID, ignoreCase: true) == 0)
				{
					continue;
				}
				familyInstance = _CommonFamilyInstanceComponentWrappers[j].GetFirstExistingFamilyInstance();
				if (familyInstance == null)
				{
					continue;
				}
				parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
				if (parameter != null)
				{
					foreach (int value4 in Enum.GetValues(typeof(LandcaddID)))
					{
						if (string.Compare(parameter.AsString(), value4.ToString("000"), ignoreCase: true) == 0)
						{
							landcaddID2 = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value4);
							break;
						}
					}
				}
				if (landcaddID != landcaddID2)
				{
					continue;
				}
				if (_CommonFamilyInstanceComponentWrappers[j].InstanceLeg == null)
				{
					_CommonFamilyInstanceComponentWrappers[j].CreateProposedLeg(_ProposedSurface);
					if (_CommonFamilyInstanceComponentWrappers[j].InstanceLeg == null)
					{
						continue;
					}
				}
				if (_CommonFamilyInstanceComponentWrappers[j].InstanceLeg == null)
				{
					continue;
				}
				if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[j].InstanceLeg.LegStartPnt, _CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegStartPnt, num2 / 2.0))
				{
					EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[j].InstanceLeg.LegEndPnt, _CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegStartPnt, num2 / 2.0);
				}
				if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[j].InstanceLeg.LegEndPnt, _CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegEndPnt, num2 / 2.0))
				{
					EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[j].InstanceLeg.LegStartPnt, _CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegEndPnt, num2 / 2.0);
				}
				num = 0;
				if (RevitGeometry.eplGeoE_IntersectTwoLegLists(_CommonFamilyInstanceComponentWrappers[j].InstanceLeg, _CommonFamilyInstanceComponentWrappers[i].InstanceLeg, ref ppPointList, ref num, num2 / 2.0) != 0 || num <= 0)
				{
					continue;
				}
				familySymbol = RevitUtility.FindFamilyTypeSymbol(_Document, familyInstance, landcaddID2);
				if (familySymbol == null)
				{
					continue;
				}
				_CommonFamilyInstanceComponentWrappers[j].ProposedFamilySymbol = familySymbol;
				switch (landcaddID2)
				{
				case LandcaddID.Street:
					num8 = 0.0;
					parameter = familySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
					if (parameter != null)
					{
						num8 = parameter.AsDouble();
					}
					parameter = familySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
					if (parameter != null)
					{
						num3 = num8 * (double)parameter.AsInteger();
					}
					parameter = familySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
					if (parameter != null)
					{
						num6 = (0.0 - num3) / 2.0 * parameter.AsDouble();
					}
					break;
				case LandcaddID.Sidewalk:
					parameter = familySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
					if (parameter != null)
					{
						num3 = parameter.AsDouble();
					}
					break;
				}
				if (!(num3 > 0.0))
				{
					continue;
				}
				Eps_Leg ppOffsetLegList3 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CommonFamilyInstanceComponentWrappers[j].InstanceLeg, num3 / 2.0, 1, ref ppOffsetLegList3, TrimClosedObject: true);
				Eps_Leg ppOffsetLegList4 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CommonFamilyInstanceComponentWrappers[j].InstanceLeg, num3 / 2.0, 2, ref ppOffsetLegList4, TrimClosedObject: true);
				for (int num9 = list.Count - 1; num9 >= 0; num9--)
				{
					if (string.Compare(familyInstance.UniqueId, list[num9].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance2.UniqueId, list[num9].SecondaryPlacementId, ignoreCase: true) == 0)
					{
						_LeftIntersectionStations.Add(list[num9]);
					}
				}
				for (int num10 = list2.Count - 1; num10 >= 0; num10--)
				{
					if (string.Compare(familyInstance.UniqueId, list2[num10].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance2.UniqueId, list2[num10].SecondaryPlacementId, ignoreCase: true) == 0)
					{
						_RightIntersectionStations.Add(list2[num10]);
					}
				}
				if (_LeftIntersectionStations.Count > 0 && _RightIntersectionStations.Count == _LeftIntersectionStations.Count)
				{
					eps_Leg = _CommonFamilyInstanceComponentWrappers[j].InstanceLeg;
					eps_Leg4 = ppOffsetLegList4;
					eps_Leg3 = ppOffsetLegList3;
					eps_Leg2 = _CommonFamilyInstanceComponentWrappers[i].InstanceLeg;
					eps_Leg6 = ppOffsetLegList2;
					eps_Leg5 = ppOffsetLegList;
					num4 = num3;
					num7 = num6;
					if (!FindIntersectionStationAndProfiles(eps_Leg2, eps_Leg6, eps_Leg5, eps_Leg, eps_Leg4, eps_Leg3, num4, num7, _dReturnValue, _dReturnValue2, _bFindIntersectionStations: false, _bDisplayPromptWhenNecessary: false, 0.0, _Document, _TransactionContainer, landcaddID2, ref _LeftIntersectionStations, ref _CenterIntersectionStations, ref _RightIntersectionStations))
					{
						return;
					}
					_CommonFamilyInstanceComponentWrappers[i].CenterIntersectionStations.AddRange(_CenterIntersectionStations);
					_CommonFamilyInstanceComponentWrappers[i].LeftIntersectionStations.AddRange(_LeftIntersectionStations);
					_CommonFamilyInstanceComponentWrappers[i].RightIntersectionStations.AddRange(_RightIntersectionStations);
					_LeftIntersectionStations.Clear();
					_CenterIntersectionStations.Clear();
					_RightIntersectionStations.Clear();
				}
			}
			IntersectionStationContainerComparer comparer = new IntersectionStationContainerComparer(_bCompareFirstStations: true);
			if (_CommonFamilyInstanceComponentWrappers[i].LeftIntersectionStations != null && _CommonFamilyInstanceComponentWrappers[i].LeftIntersectionStations.Count > 0)
			{
				_CommonFamilyInstanceComponentWrappers[i].LeftIntersectionStations.Sort(comparer);
			}
			if (_CommonFamilyInstanceComponentWrappers[i].CenterIntersectionStations != null && _CommonFamilyInstanceComponentWrappers[i].CenterIntersectionStations.Count > 0)
			{
				_CommonFamilyInstanceComponentWrappers[i].CenterIntersectionStations.Sort(comparer);
			}
			if (_CommonFamilyInstanceComponentWrappers[i].RightIntersectionStations != null && _CommonFamilyInstanceComponentWrappers[i].RightIntersectionStations.Count > 0)
			{
				_CommonFamilyInstanceComponentWrappers[i].RightIntersectionStations.Sort(comparer);
			}
			list.Clear();
			list2.Clear();
		}
	}

	public static void SetupIntersectionStationsOnlyForSelectedSiteComponent(Document _Document, TopographySurfaceContainer _ProposedSurface, EPTransactionContainer _TransactionContainer, bool _bFindIntersectionStations, List<FamilyInstanceComponentWrapper> _CommonFamilyInstanceComponentWrappers, ref FamilyInstanceComponentWrapper _CurrentFamilyInstanceComponentWrapper)
	{
		SetupIntersectionStationsOnlyForSelectedSiteComponent(_Document, _ProposedSurface, _TransactionContainer, string.Empty, _CommonFamilyInstanceComponentWrappers, ref _CurrentFamilyInstanceComponentWrapper);
	}

	private static void SetupIntersectionStationsOnlyForSelectedSiteComponent(Document _Document, TopographySurfaceContainer _ProposedSurface, EPTransactionContainer _TransactionContainer, string _IgnoreChainID, List<FamilyInstanceComponentWrapper> _CommonFamilyInstanceComponentWrappers, ref FamilyInstanceComponentWrapper _CurrentFamilyInstanceComponentWrapper)
	{
		Eps_Point eps_Point = null;
		ProfileContainer profileContainer = null;
		IntersectionContainer intersectionContainer = null;
		List<IntersectionContainer> list = new List<IntersectionContainer>();
		List<IntersectionStationContainer> list2 = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> list3 = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _LeftIntersectionStations = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _RightIntersectionStations = new List<IntersectionStationContainer>();
		List<IntersectionStationContainer> _CenterIntersectionStations = new List<IntersectionStationContainer>();
		LandcaddID landcaddID = LandcaddID.Sidewalk;
		LandcaddID landcaddID2 = LandcaddID.Sidewalk;
		FamilyInstance familyInstance = null;
		FamilyInstance familyInstance2 = null;
		FamilySymbol familySymbol = null;
		Parameter parameter = null;
		Eps_Leg ppOffsetLegList = null;
		Eps_Leg ppOffsetLegList2 = null;
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Leg eps_Leg4 = null;
		Eps_Leg eps_Leg5 = null;
		Eps_Leg eps_Leg6 = null;
		List<Eps_Point> ppPointList = new List<Eps_Point>();
		int num = 0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double _dReturnValue = 0.0;
		double _dReturnValue2 = 0.0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		familyInstance2 = _CurrentFamilyInstanceComponentWrapper.GetFirstExistingFamilyInstance();
		if (familyInstance2 != null)
		{
			parameter = familyInstance2.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (parameter != null)
			{
				foreach (int value3 in Enum.GetValues(typeof(LandcaddID)))
				{
					if (string.Compare(parameter.AsString(), value3.ToString("000"), ignoreCase: true) == 0)
					{
						landcaddID = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value3);
						break;
					}
				}
			}
			if (_CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol == null)
			{
				familySymbol = RevitUtility.FindFamilyTypeSymbol(_Document, familyInstance2, landcaddID);
				if (familySymbol != null)
				{
					_CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol = familySymbol;
				}
			}
		}
		if (landcaddID != LandcaddID.Street && landcaddID != LandcaddID.Sidewalk)
		{
			return;
		}
		switch (landcaddID)
		{
		case LandcaddID.Street:
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("StreetIntersectionDefaultRadius", ref _dReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("StreetIntersectionDefaultTransitioningDistance", ref _dReturnValue2))
			{
				_dReturnValue = 5.0;
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
			if (parameter != null)
			{
				num8 = parameter.AsDouble();
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
			if (parameter != null)
			{
				num2 = num8 * (double)parameter.AsInteger();
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
			if (parameter != null)
			{
				num5 = (0.0 - num2) / 2.0 * parameter.AsDouble();
			}
			break;
		case LandcaddID.Sidewalk:
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("SideWalkIntersectionDefaultRadius", ref _dReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting("SideWalkIntersectionDefaultTransitioningDistance", ref _dReturnValue2))
			{
				_dReturnValue = 5.0;
			}
			parameter = _CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
			if (parameter != null)
			{
				num2 = parameter.AsDouble();
			}
			break;
		}
		if (num2 > 0.0)
		{
			if (_CurrentFamilyInstanceComponentWrapper.InstanceLeg == null)
			{
				_CurrentFamilyInstanceComponentWrapper.CreateProposedLeg(_ProposedSurface);
			}
			ppOffsetLegList = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentFamilyInstanceComponentWrapper.InstanceLeg, num2 / 2.0, 1, ref ppOffsetLegList, TrimClosedObject: true);
			ppOffsetLegList2 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CurrentFamilyInstanceComponentWrapper.InstanceLeg, num2 / 2.0, 2, ref ppOffsetLegList2, TrimClosedObject: true);
		}
		ElementSet familyInstanceElementSet = _CurrentFamilyInstanceComponentWrapper.GetFamilyInstanceElementSet();
		if (familyInstanceElementSet != null && familyInstanceElementSet.Size > 0)
		{
			List<IntersectionStationContainer> _LeftIntersectionStations2 = null;
			List<IntersectionStationContainer> _RightIntersectionStations2 = null;
			List<FamilyInstance> _SiteFamilyInstanceList = null;
			foreach (Element item in familyInstanceElementSet)
			{
				GetIntersections(_Document, item.UniqueId, ref _SiteFamilyInstanceList, ref _LeftIntersectionStations2, ref _RightIntersectionStations2);
				if (_SiteFamilyInstanceList != null && _SiteFamilyInstanceList.Count > 0)
				{
					_CurrentFamilyInstanceComponentWrapper.AddIntersectionFamilyInstances(_SiteFamilyInstanceList);
					_CurrentFamilyInstanceComponentWrapper.IntersectionFamilySymbol = _SiteFamilyInstanceList[0].Symbol;
				}
				if (_LeftIntersectionStations2 != null && _LeftIntersectionStations2.Count > 0)
				{
					list2.AddRange(_LeftIntersectionStations2);
				}
				if (_RightIntersectionStations2 != null && _RightIntersectionStations2.Count > 0)
				{
					list3.AddRange(_RightIntersectionStations2);
				}
			}
		}
		if (list2.Count == 0 || list3.Count == 0 || _CommonFamilyInstanceComponentWrappers == null)
		{
			return;
		}
		IntersectionStationContainerComparer comparer = new IntersectionStationContainerComparer(_bCompareFirstStations: true);
		for (int i = 0; i < _CommonFamilyInstanceComponentWrappers.Count; i++)
		{
			if (string.Compare(_CurrentFamilyInstanceComponentWrapper.ChainID, _CommonFamilyInstanceComponentWrappers[i].ChainID, ignoreCase: true) == 0 || string.Compare(_CommonFamilyInstanceComponentWrappers[i].ChainID, _IgnoreChainID, ignoreCase: true) == 0)
			{
				continue;
			}
			familyInstance = _CommonFamilyInstanceComponentWrappers[i].GetFirstExistingFamilyInstance();
			if (familyInstance == null)
			{
				continue;
			}
			parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (parameter != null)
			{
				foreach (int value4 in Enum.GetValues(typeof(LandcaddID)))
				{
					if (string.Compare(parameter.AsString(), value4.ToString("000"), ignoreCase: true) == 0)
					{
						landcaddID2 = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value4);
						break;
					}
				}
			}
			if (landcaddID != landcaddID2)
			{
				continue;
			}
			if (_CommonFamilyInstanceComponentWrappers[i].InstanceLeg == null)
			{
				_CommonFamilyInstanceComponentWrappers[i].CreateProposedLeg(_ProposedSurface);
			}
			if (_CommonFamilyInstanceComponentWrappers[i].InstanceLeg == null)
			{
				continue;
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegStartPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegStartPnt, num2 / 2.0))
			{
				EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegEndPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegStartPnt, num2 / 2.0);
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegEndPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegEndPnt, num2 / 2.0))
			{
				EPShared.eplGeoE_ArePntsDuplicates2D(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg.LegStartPnt, _CurrentFamilyInstanceComponentWrapper.InstanceLeg.LegEndPnt, num2 / 2.0);
			}
			num = 0;
			if (RevitGeometry.eplGeoE_IntersectTwoLegLists(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg, _CurrentFamilyInstanceComponentWrapper.InstanceLeg, ref ppPointList, ref num, num2 / 2.0) != 0 || num <= 0)
			{
				continue;
			}
			familySymbol = RevitUtility.FindFamilyTypeSymbol(_Document, familyInstance, landcaddID2);
			if (familySymbol == null)
			{
				continue;
			}
			_CommonFamilyInstanceComponentWrappers[i].ProposedFamilySymbol = familySymbol;
			switch (landcaddID2)
			{
			case LandcaddID.Street:
				num8 = 0.0;
				parameter = familySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
				if (parameter != null)
				{
					num8 = parameter.AsDouble();
				}
				parameter = familySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
				if (parameter != null)
				{
					num3 = num8 * (double)parameter.AsInteger();
				}
				parameter = familySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
				if (parameter != null)
				{
					num6 = (0.0 - num3) / 2.0 * parameter.AsDouble();
				}
				break;
			case LandcaddID.Sidewalk:
				parameter = familySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
				if (parameter != null)
				{
					num3 = parameter.AsDouble();
				}
				break;
			}
			if (!(num3 > 0.0))
			{
				continue;
			}
			Eps_Leg ppOffsetLegList3 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg, num3 / 2.0, 1, ref ppOffsetLegList3, TrimClosedObject: true);
			Eps_Leg ppOffsetLegList4 = new Eps_Leg();
			RevitGeometry.OffsetLegList(_CommonFamilyInstanceComponentWrappers[i].InstanceLeg, num3 / 2.0, 2, ref ppOffsetLegList4, TrimClosedObject: true);
			for (int num9 = list2.Count - 1; num9 >= 0; num9--)
			{
				if (string.Compare(familyInstance.UniqueId, list2[num9].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance2.UniqueId, list2[num9].SecondaryPlacementId, ignoreCase: true) == 0)
				{
					flag = false;
					flag2 = true;
					_LeftIntersectionStations.Add(list2[num9]);
					list2.RemoveAt(num9);
				}
				else if (string.Compare(familyInstance2.UniqueId, list2[num9].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance.UniqueId, list2[num9].SecondaryPlacementId, ignoreCase: true) == 0)
				{
					flag = true;
					flag2 = true;
					_LeftIntersectionStations.Add(list2[num9]);
					list2.RemoveAt(num9);
				}
			}
			for (int num10 = list3.Count - 1; num10 >= 0; num10--)
			{
				if (string.Compare(familyInstance.UniqueId, list3[num10].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance2.UniqueId, list3[num10].SecondaryPlacementId, ignoreCase: true) == 0)
				{
					flag = false;
					flag2 = true;
					_RightIntersectionStations.Add(list3[num10]);
					list3.RemoveAt(num10);
				}
				else if (string.Compare(familyInstance2.UniqueId, list3[num10].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(familyInstance.UniqueId, list3[num10].SecondaryPlacementId, ignoreCase: true) == 0)
				{
					flag = true;
					flag2 = true;
					_RightIntersectionStations.Add(list3[num10]);
					list3.RemoveAt(num10);
				}
			}
			if (!flag2)
			{
				continue;
			}
			flag3 = false;
			if (landcaddID2 == LandcaddID.Street)
			{
				parameter = familySymbol.LookupParameter(RevitParameters.StreetCurbDisplayParameter);
				if (parameter != null)
				{
					flag3 = Convert.ToBoolean(parameter.AsInteger());
				}
			}
			if (flag)
			{
				eps_Leg = _CurrentFamilyInstanceComponentWrapper.InstanceLeg;
				eps_Leg4 = ppOffsetLegList2;
				eps_Leg3 = ppOffsetLegList;
				eps_Leg2 = _CommonFamilyInstanceComponentWrappers[i].InstanceLeg;
				eps_Leg6 = ppOffsetLegList4;
				eps_Leg5 = ppOffsetLegList3;
				num4 = num2;
				num7 = num5;
			}
			else
			{
				eps_Leg = _CommonFamilyInstanceComponentWrappers[i].InstanceLeg;
				eps_Leg4 = ppOffsetLegList4;
				eps_Leg3 = ppOffsetLegList3;
				eps_Leg2 = _CurrentFamilyInstanceComponentWrapper.InstanceLeg;
				eps_Leg6 = ppOffsetLegList2;
				eps_Leg5 = ppOffsetLegList;
				num4 = num3;
				num7 = num6;
			}
			if (!FindIntersectionStationAndProfiles(eps_Leg2, eps_Leg6, eps_Leg5, eps_Leg, eps_Leg4, eps_Leg3, num4, num7, _dReturnValue, _dReturnValue2, _bFindIntersectionStations: false, _bDisplayPromptWhenNecessary: false, 0.0, _Document, _TransactionContainer, landcaddID2, ref _LeftIntersectionStations, ref _CenterIntersectionStations, ref _RightIntersectionStations))
			{
				return;
			}
			if (_LeftIntersectionStations != null && _LeftIntersectionStations.Count > 0)
			{
				_LeftIntersectionStations.Sort(comparer);
			}
			if (_CenterIntersectionStations != null && _CenterIntersectionStations.Count > 0)
			{
				_CenterIntersectionStations.Sort(comparer);
			}
			if (_RightIntersectionStations != null && _RightIntersectionStations.Count > 0)
			{
				_RightIntersectionStations.Sort(comparer);
			}
			if (!flag)
			{
				_CurrentFamilyInstanceComponentWrapper.CenterIntersectionStations.AddRange(_CenterIntersectionStations);
				_CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations.AddRange(_LeftIntersectionStations);
				_CurrentFamilyInstanceComponentWrapper.RightIntersectionStations.AddRange(_RightIntersectionStations);
			}
			if (flag && _LeftIntersectionStations.Count > 0 && _LeftIntersectionStations.Count == _RightIntersectionStations.Count)
			{
				bool flag4 = true;
				for (int j = 0; j < _LeftIntersectionStations.Count; j++)
				{
					_LeftIntersectionStations[j].PrimaryPlacementId = familyInstance.UniqueId;
					_RightIntersectionStations[j].PrimaryPlacementId = familyInstance.UniqueId;
					if (_LeftIntersectionStations[j].IntersectionStations.Count != _RightIntersectionStations[j].IntersectionStations.Count)
					{
						continue;
					}
					for (int k = 0; k < _LeftIntersectionStations[j].IntersectionStations.Count; k++)
					{
						intersectionContainer = new IntersectionContainer();
						intersectionContainer.StartingIntersection = true;
						profileContainer = new ProfileContainer();
						if (_LeftIntersectionStations[j].IntersectionStations[k].PrimaryStation <= _RightIntersectionStations[j].IntersectionStations[k].PrimaryStation)
						{
							flag4 = _LeftIntersectionStations[j].IntersectionStations[k].IsPrimaryLeft;
							eps_Point = GetIntersectionStationPoint(_LeftIntersectionStations, j, k, _bFindEndsOfIntersection: false);
							if ((eps_Point != null && flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg4, ref _dStation, ref _dOffset) == 1700) || (!flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg3, ref _dStation, ref _dOffset) == 1700))
							{
								profileContainer.Station = _dStation;
							}
							else
							{
								profileContainer.Station = _LeftIntersectionStations[j].IntersectionStations[k].PrimaryStation;
							}
						}
						else
						{
							flag4 = _RightIntersectionStations[j].IntersectionStations[k].IsPrimaryLeft;
							eps_Point = GetIntersectionStationPoint(_RightIntersectionStations, j, k, _bFindEndsOfIntersection: false);
							if ((eps_Point != null && flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg4, ref _dStation, ref _dOffset) == 1700) || (!flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg3, ref _dStation, ref _dOffset) == 1700))
							{
								profileContainer.Station = _dStation;
							}
							else
							{
								profileContainer.Station = _RightIntersectionStations[j].IntersectionStations[k].PrimaryStation;
							}
						}
						intersectionContainer.Profiles.Add(profileContainer);
						if (flag4)
						{
							intersectionContainer.ProfileUsage = ProfileUsage.LeftOnly;
						}
						else
						{
							intersectionContainer.ProfileUsage = ProfileUsage.RightOnly;
						}
						list.Add(intersectionContainer);
						intersectionContainer = new IntersectionContainer();
						intersectionContainer.StartingIntersection = false;
						profileContainer = new ProfileContainer();
						if (_LeftIntersectionStations[j].IntersectionStations[k].PrimaryStation <= _RightIntersectionStations[j].IntersectionStations[k].PrimaryStation)
						{
							flag4 = _RightIntersectionStations[j].IntersectionStations[k].IsPrimaryLeft;
							eps_Point = GetIntersectionStationPoint(_RightIntersectionStations, j, k, _bFindEndsOfIntersection: false);
							if ((eps_Point != null && flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg4, ref _dStation, ref _dOffset) == 1700) || (!flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg3, ref _dStation, ref _dOffset) == 1700))
							{
								profileContainer.Station = _dStation;
							}
							else
							{
								profileContainer.Station = _RightIntersectionStations[j].IntersectionStations[k].PrimaryStation;
							}
						}
						else
						{
							flag4 = _LeftIntersectionStations[j].IntersectionStations[k].IsPrimaryLeft;
							eps_Point = GetIntersectionStationPoint(_LeftIntersectionStations, j, k, _bFindEndsOfIntersection: false);
							if ((eps_Point != null && flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg4, ref _dStation, ref _dOffset) == 1700) || (!flag4 && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(eps_Point, eps_Leg3, ref _dStation, ref _dOffset) == 1700))
							{
								profileContainer.Station = _dStation;
							}
							else
							{
								profileContainer.Station = _LeftIntersectionStations[j].IntersectionStations[k].PrimaryStation;
							}
						}
						intersectionContainer.Profiles.Add(profileContainer);
						if (flag4)
						{
							intersectionContainer.ProfileUsage = ProfileUsage.LeftOnly;
						}
						else
						{
							intersectionContainer.ProfileUsage = ProfileUsage.RightOnly;
						}
						list.Add(intersectionContainer);
					}
				}
				IntersectionContainerComparer comparer2 = new IntersectionContainerComparer(_bCompareFirstStations: true);
				if (list != null && list.Count > 0)
				{
					list.Sort(comparer2);
				}
				_CurrentFamilyInstanceComponentWrapper.IntersectionContainerList.AddRange(list);
				list.Clear();
			}
			flag2 = false;
			_LeftIntersectionStations.Clear();
			_CenterIntersectionStations.Clear();
			_RightIntersectionStations.Clear();
		}
		if (_CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations != null && _CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations.Count > 0)
		{
			_CurrentFamilyInstanceComponentWrapper.LeftIntersectionStations.Sort(comparer);
		}
		if (_CurrentFamilyInstanceComponentWrapper.CenterIntersectionStations != null && _CurrentFamilyInstanceComponentWrapper.CenterIntersectionStations.Count > 0)
		{
			_CurrentFamilyInstanceComponentWrapper.CenterIntersectionStations.Sort(comparer);
		}
		if (_CurrentFamilyInstanceComponentWrapper.RightIntersectionStations != null && _CurrentFamilyInstanceComponentWrapper.RightIntersectionStations.Count > 0)
		{
			_CurrentFamilyInstanceComponentWrapper.RightIntersectionStations.Sort(comparer);
		}
	}

	private static void SetupIntersectionContainersForSiteComponent(Document _Document, EPTransactionContainer _TransactionContainer, TopographySurfaceContainer _ProposedSurface, List<FamilyInstanceComponentWrapper> _CommonFamilyInstanceComponentWrappers, ref FamilyInstanceComponentWrapper _CurFamilyInstanceComponentWrapper)
	{
		SetupIntersectionContainersForSiteComponent(_Document, _TransactionContainer, _ProposedSurface, string.Empty, _CommonFamilyInstanceComponentWrappers, ref _CurFamilyInstanceComponentWrapper);
	}

	private static void SetupIntersectionContainersForSiteComponent(Document _Document, EPTransactionContainer _TransactionContainer, TopographySurfaceContainer _ProposedSurface, string _IgnoreChainID, List<FamilyInstanceComponentWrapper> _CommonFamilyInstanceComponentWrappers, ref FamilyInstanceComponentWrapper _CurFamilyInstanceComponentWrapper)
	{
		List<IntersectionContainer> _IntersectionContainers = new List<IntersectionContainer>();
		List<OffsetLegWrapper> _OffsetLegWrappers = new List<OffsetLegWrapper>();
		List<double> list = new List<double>();
		List<Eps_Point> list2 = new List<Eps_Point>();
		Eps_Point eps_Point = null;
		_CurFamilyInstanceComponentWrapper.IntersectionContainerList = new List<IntersectionContainer>();
		FamilyInstanceComponentWrapper _CurrentFamilyInstanceComponentWrapper = _CurFamilyInstanceComponentWrapper;
		SetupIntersectionStationsOnlyForSelectedSiteComponent(_Document, _ProposedSurface, _TransactionContainer, _IgnoreChainID, _CommonFamilyInstanceComponentWrappers, ref _CurrentFamilyInstanceComponentWrapper);
		_CurFamilyInstanceComponentWrapper = _CurrentFamilyInstanceComponentWrapper;
		Eps_Leg originaltLeg = Eps_Leg.CopyLeg(_CurFamilyInstanceComponentWrapper.InstanceLeg);
		if (_CurFamilyInstanceComponentWrapper.CenterIntersectionStations.Count > 0)
		{
			for (int i = 0; i < _CurFamilyInstanceComponentWrapper.CenterIntersectionStations.Count; i++)
			{
				for (int j = 0; j < _CurFamilyInstanceComponentWrapper.CenterIntersectionStations[i].IntersectionStations.Count; j++)
				{
					list.Add(_CurFamilyInstanceComponentWrapper.CenterIntersectionStations[i].IntersectionStations[j].Station);
					eps_Point = GetIntersectionStationPoint(_CurFamilyInstanceComponentWrapper.CenterIntersectionStations, i, j, _bFindEndsOfIntersection: false);
					if (eps_Point != null)
					{
						list2.Add(eps_Point);
					}
				}
			}
		}
		_CurFamilyInstanceComponentWrapper.InstanceLeg = _CurFamilyInstanceComponentWrapper.InstanceLeg.SplitLegAtStations(list);
		_CurFamilyInstanceComponentWrapper.InstanceLeg = _CurFamilyInstanceComponentWrapper.InstanceLeg.ApplyElevationsToLeg(null, _bUseToposurfaceElevation: false, _bUseConstantElevation: false, 0.0, list2);
		double dAdjustSubgradeElev = 0.0;
		double dStreetEdgeAdjustElev = 0.0;
		double num = 0.0;
		double num2 = 0.0;
		LandcaddID landcaddID = LandcaddID.Sidewalk;
		FamilyInstance firstExistingFamilyInstance = _CurFamilyInstanceComponentWrapper.GetFirstExistingFamilyInstance();
		if (firstExistingFamilyInstance != null)
		{
			Parameter parameter = firstExistingFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (parameter != null)
			{
				foreach (int value in Enum.GetValues(typeof(LandcaddID)))
				{
					if (string.Compare(parameter.AsString(), value.ToString("000"), ignoreCase: true) == 0)
					{
						landcaddID = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value);
						break;
					}
				}
			}
			FamilySymbol familySymbol = RevitUtility.FindFamilyTypeSymbol(_Document, firstExistingFamilyInstance, landcaddID);
			if (familySymbol != null)
			{
				_CurFamilyInstanceComponentWrapper.ProposedFamilySymbol = familySymbol;
				switch (landcaddID)
				{
				case LandcaddID.Street:
					num2 = 0.0;
					parameter = familySymbol.LookupParameter(RevitParameters.StreetLaneWidthParameter);
					if (parameter != null)
					{
						num2 = parameter.AsDouble();
					}
					parameter = familySymbol.LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
					if (parameter != null)
					{
						num = num2 * (double)parameter.AsInteger();
					}
					parameter = familySymbol.LookupParameter(RevitParameters.StreetCrossSlopeParameter);
					if (parameter != null)
					{
						dStreetEdgeAdjustElev = (0.0 - num) / 2.0 * parameter.AsDouble();
					}
					parameter = familySymbol.LookupParameter(RevitParameters.StreetPavementDepthParameter);
					if (parameter != null)
					{
						dAdjustSubgradeElev = 0.0 - parameter.AsDouble();
					}
					break;
				case LandcaddID.Sidewalk:
					parameter = familySymbol.LookupParameter(RevitParameters.SideWalkWidthParameter);
					if (parameter != null)
					{
						num = parameter.AsDouble();
					}
					parameter = familySymbol.LookupParameter(RevitParameters.SideWalkPavementDepthParameter);
					if (parameter != null)
					{
						dAdjustSubgradeElev = 0.0 - parameter.AsDouble();
					}
					break;
				}
			}
		}
		FindOffsetLegWrappers(originaltLeg, _CurFamilyInstanceComponentWrapper.InstanceLeg, _CurFamilyInstanceComponentWrapper.LeftIntersectionStations, _CurFamilyInstanceComponentWrapper.CenterIntersectionStations, _CurFamilyInstanceComponentWrapper.RightIntersectionStations, landcaddID, num / 2.0, num / 2.0, 0.0, dAdjustSubgradeElev, dStreetEdgeAdjustElev, ref _OffsetLegWrappers, ref _IntersectionContainers);
		_OffsetLegWrappers.Clear();
		if (_IntersectionContainers.Count > 0)
		{
			_CurFamilyInstanceComponentWrapper.IntersectionContainerList.AddRange(_IntersectionContainers);
			_IntersectionContainers.Clear();
		}
	}

	public static TopographySurfaceContainer FindParentSurfaceAndRemovePointsForSelectedCurbComponent(Document _Document, TopographySurfaceContainer _ProposedSurface, FamilyInstanceComponentWrapper _CurrentFamilyInstanceComponentWrapper, ref List<Eps_Point> _TopoPointList)
	{
		TopographySurfaceContainer topographySurfaceContainer = null;
		LandcaddID landcaddID = LandcaddID.Curb;
		FamilyInstance familyInstance = null;
		Parameter parameter = null;
		string text = string.Empty;
		string empty = string.Empty;
		familyInstance = _CurrentFamilyInstanceComponentWrapper.GetFirstExistingFamilyInstance();
		if (familyInstance != null)
		{
			parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (parameter != null)
			{
				foreach (int value in Enum.GetValues(typeof(LandcaddID)))
				{
					if (string.Compare(parameter.AsString(), value.ToString("000"), ignoreCase: true) == 0)
					{
						landcaddID = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value);
						break;
					}
				}
			}
			parameter = familyInstance.LookupParameter(RevitParameters.ParentSurfaceIDFamilyParameter);
			if (parameter != null)
			{
				text = parameter.AsString();
			}
		}
		if (landcaddID != LandcaddID.Curb || text == string.Empty || _ProposedSurface == null || _ProposedSurface.TopographySurface == null || string.Compare(_ProposedSurface.TopographySurface.UniqueId, text, ignoreCase: true) == 0)
		{
			return null;
		}
		TopographySurface topographySurface = FindTopoSurfaceInstance(_Document, text);
		if (topographySurface != null)
		{
			topographySurfaceContainer = new TopographySurfaceContainer(_Document.Application, topographySurface, _bFindMinMaxPoints: true);
		}
		if (topographySurfaceContainer != null && familyInstance != null)
		{
			double num = 0.02;
			double num2 = 0.0;
			double num3 = 0.0;
			bool flag = false;
			if (_CurrentFamilyInstanceComponentWrapper.OriginalFamilySymbol != null)
			{
				parameter = _CurrentFamilyInstanceComponentWrapper.OriginalFamilySymbol.LookupParameter(RevitParameters.StreetGutterWidthParameter);
				if (parameter != null)
				{
					num3 = parameter.AsDouble();
				}
				parameter = _CurrentFamilyInstanceComponentWrapper.OriginalFamilySymbol.LookupParameter(RevitParameters.StreetCurbWidthParameter);
				if (parameter != null)
				{
					num2 = parameter.AsDouble();
				}
			}
			if (_CurrentFamilyInstanceComponentWrapper.OriginalLeg == null)
			{
				_CurrentFamilyInstanceComponentWrapper.CreateOriginalLeg(topographySurfaceContainer);
			}
			Eps_Leg eps_Leg = null;
			if (_CurrentFamilyInstanceComponentWrapper.OriginalLeg != null && _CurrentFamilyInstanceComponentWrapper.OriginalLeg.IsLegClosed())
			{
				Eps_Leg ppOffsetLegList = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentFamilyInstanceComponentWrapper.OriginalLeg, num + num2 + num3, 1, ref ppOffsetLegList, TrimClosedObject: true);
				Eps_Leg ppOffsetLegList2 = new Eps_Leg();
				RevitGeometry.OffsetLegList(_CurrentFamilyInstanceComponentWrapper.OriginalLeg, num + num2 + num3, 2, ref ppOffsetLegList2, TrimClosedObject: true);
				eps_Leg = ((!(RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2) < RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList))) ? Eps_Leg.CopyLeg(ppOffsetLegList2) : Eps_Leg.CopyLeg(ppOffsetLegList));
			}
			else
			{
				parameter = familyInstance.LookupParameter(RevitParameters.UseRightSideGenericFamilyParameter);
				if (parameter != null)
				{
					flag = Convert.ToBoolean(parameter.AsInteger());
				}
				Eps_Leg ppOffsetLegList3 = new Eps_Leg();
				Eps_Leg ppOffsetLegList4 = new Eps_Leg();
				if (flag)
				{
					RevitGeometry.OffsetLegList(_CurrentFamilyInstanceComponentWrapper.OriginalLeg, num, 2, ref ppOffsetLegList3, TrimClosedObject: true);
					ppOffsetLegList3 = ppOffsetLegList3.ExtendLegList(_bStart: true, num);
					ppOffsetLegList3 = ppOffsetLegList3.ExtendLegList(_bStart: false, num);
					RevitGeometry.OffsetLegList(ppOffsetLegList3, 2.0 * num + num2 + num3, 1, ref ppOffsetLegList4, TrimClosedObject: true);
				}
				else
				{
					RevitGeometry.OffsetLegList(_CurrentFamilyInstanceComponentWrapper.OriginalLeg, num, 1, ref ppOffsetLegList3, TrimClosedObject: true);
					ppOffsetLegList3 = ppOffsetLegList3.ExtendLegList(_bStart: true, num);
					ppOffsetLegList3 = ppOffsetLegList3.ExtendLegList(_bStart: false, num);
					RevitGeometry.OffsetLegList(ppOffsetLegList3, 2.0 * num + num2 + num3, 2, ref ppOffsetLegList4, TrimClosedObject: true);
				}
				List<Eps_Leg> list = new List<Eps_Leg>();
				list.Add(ppOffsetLegList3);
				list.Add(ppOffsetLegList4);
				eps_Leg = FitCorrectLegListsAtGaps(list);
			}
			_TopoPointList = topographySurfaceContainer.GetAllVertices();
			if (eps_Leg != null && _TopoPointList.Count > 0)
			{
				RevitGeometry.RemoveAllPointsInBoundary(ref _TopoPointList, eps_Leg);
			}
		}
		return topographySurfaceContainer;
	}

	private static double FindBreaklineIntervalFromList(Eps_Leg _ParkingLotBreaklineLeg, List<Element> _ParkingLotBreaklineElementList, List<double> _dParkingLotBreaklineLinearPointIntervals)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		Element element = null;
		int num = 0;
		if (_dParkingLotBreaklineLinearPointIntervals.Count != _ParkingLotBreaklineElementList.Count)
		{
			return 5.0;
		}
		for (eps_Leg2 = _ParkingLotBreaklineLeg; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
		{
			for (num = _ParkingLotBreaklineElementList.Count - 1; num >= 0; num--)
			{
				element = _ParkingLotBreaklineElementList[num];
				if (element != null && element.Location is LocationCurve { Curve: var curve } && curve != null)
				{
					if (curve is Arc && eps_Leg2.Arc != null)
					{
						Arc arc = (Arc)curve;
						Eps_Arc eps_Arc = new Eps_Arc(arc);
						if (eps_Arc.IsEqual(eps_Leg2.Arc))
						{
							if (RevitDefines.LT(_dParkingLotBreaklineLinearPointIntervals[num], 0.0833333))
							{
								_dParkingLotBreaklineLinearPointIntervals[num] = 0.0833333;
							}
							return _dParkingLotBreaklineLinearPointIntervals[num];
						}
					}
					else if (curve is Line && eps_Leg2.Line != null)
					{
						Line line = (Line)curve;
						Eps_Line eps_Line = new Eps_Line(line);
						if (eps_Line.IsEqual(eps_Leg2.Line))
						{
							if (RevitDefines.LT(_dParkingLotBreaklineLinearPointIntervals[num], 0.0833333))
							{
								_dParkingLotBreaklineLinearPointIntervals[num] = 0.0833333;
							}
							return _dParkingLotBreaklineLinearPointIntervals[num];
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
									if (RevitDefines.LT(_dParkingLotBreaklineLinearPointIntervals[num], 0.0833333))
									{
										_dParkingLotBreaklineLinearPointIntervals[num] = 0.0833333;
									}
									return _dParkingLotBreaklineLinearPointIntervals[num];
								}
							}
							else if (eps_Leg3.Arc != null && eps_Leg2.Arc != null && eps_Leg3.Arc.IsEqual(eps_Leg2.Arc))
							{
								if (RevitDefines.LT(_dParkingLotBreaklineLinearPointIntervals[num], 0.0833333))
								{
									_dParkingLotBreaklineLinearPointIntervals[num] = 0.0833333;
								}
								return _dParkingLotBreaklineLinearPointIntervals[num];
							}
						}
					}
				}
			}
		}
		return 5.0;
	}

	private static void CombineLegListsAtGaps(ref Eps_Leg _CombinedLegList, Eps_Leg _AddLegList)
	{
		Eps_Leg eps_Leg = null;
		if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CombinedLegList.LegEndPnt, _AddLegList.LegStartPnt))
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = new Eps_Line();
			eps_Leg.Line.StartPnt = _CombinedLegList.LegEndPnt;
			eps_Leg.Line.EndPnt = _AddLegList.LegStartPnt;
			_CombinedLegList.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
		}
		_CombinedLegList.Last.Next = Eps_Leg.CopyLeg(_AddLegList);
		if (!EPShared.eplGeoE_ArePntsDuplicates2D(_CombinedLegList.LegStartPnt, _AddLegList.LegEndPnt))
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = new Eps_Line();
			eps_Leg.Line.StartPnt = _AddLegList.LegEndPnt;
			eps_Leg.Line.EndPnt = _CombinedLegList.LegStartPnt;
			_CombinedLegList.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
		}
	}

	private static Eps_Leg FitCorrectLegListsAtGaps(Eps_Leg _LeftCatchLinesLeg, Eps_Leg _RightCatchLinesLeg, Eps_Leg _LeftCutFillLinesLeg, Eps_Leg _RightCutFillLinesLeg, bool _bIsOriginalLegClosed)
	{
		Eps_Leg _CombinedLegList = null;
		if (_LeftCatchLinesLeg != null && _LeftCatchLinesLeg.IsLegClosed())
		{
			return _LeftCatchLinesLeg;
		}
		if (_RightCatchLinesLeg != null && _RightCatchLinesLeg.IsLegClosed())
		{
			return _RightCatchLinesLeg;
		}
		bool flag = false;
		if (_LeftCatchLinesLeg != null && _LeftCatchLinesLeg.LegCount > 0 && RevitDefines.GT(RevitGeometry.eplGeoE_CalcLegListLength(_LeftCatchLinesLeg), 0.0))
		{
			_CombinedLegList = Eps_Leg.CopyLeg(_LeftCatchLinesLeg);
		}
		if (_RightCatchLinesLeg != null && _RightCatchLinesLeg.LegCount > 0 && RevitDefines.GT(RevitGeometry.eplGeoE_CalcLegListLength(_RightCatchLinesLeg), 0.0))
		{
			flag = true;
			if (_CombinedLegList != null)
			{
				CombineLegListsAtGaps(ref _CombinedLegList, _RightCatchLinesLeg);
				return _CombinedLegList;
			}
			_CombinedLegList = Eps_Leg.CopyLeg(_RightCatchLinesLeg);
		}
		if ((flag || _CombinedLegList == null) && _LeftCutFillLinesLeg != null && _LeftCutFillLinesLeg.LegCount > 0 && RevitDefines.GT(RevitGeometry.eplGeoE_CalcLegListLength(_LeftCutFillLinesLeg), 0.0))
		{
			if (_CombinedLegList != null)
			{
				CombineLegListsAtGaps(ref _CombinedLegList, _LeftCutFillLinesLeg);
				return _CombinedLegList;
			}
			_CombinedLegList = Eps_Leg.CopyLeg(_LeftCutFillLinesLeg);
		}
		if (_RightCutFillLinesLeg != null && _RightCutFillLinesLeg.LegCount > 0 && RevitDefines.GT(RevitGeometry.eplGeoE_CalcLegListLength(_RightCutFillLinesLeg), 0.0))
		{
			if (_CombinedLegList == null)
			{
				_CombinedLegList = Eps_Leg.CopyLeg(_RightCutFillLinesLeg);
			}
			else
			{
				CombineLegListsAtGaps(ref _CombinedLegList, _RightCutFillLinesLeg);
			}
		}
		if (_bIsOriginalLegClosed && _CombinedLegList != null && !_CombinedLegList.IsLegClosed())
		{
			Eps_Leg eps_Leg = null;
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = new Eps_Line();
			eps_Leg.Line.StartPnt = _CombinedLegList.LegEndPnt;
			eps_Leg.Line.EndPnt = _CombinedLegList.LegStartPnt;
			_CombinedLegList.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
		}
		return _CombinedLegList;
	}

	private static Eps_Leg FitCorrectLegListsAtGaps(List<Eps_Leg> _CatchLinesLegLists)
	{
		Eps_Leg _CombinedLegList = null;
		if (_CatchLinesLegLists == null || _CatchLinesLegLists.Count == 0)
		{
			return null;
		}
		if (_CatchLinesLegLists.Count > 0 && _CatchLinesLegLists[0] != null && _CatchLinesLegLists[0].IsLegClosed())
		{
			return _CatchLinesLegLists[0];
		}
		if (_CatchLinesLegLists.Count > 1 && _CatchLinesLegLists[1] != null && _CatchLinesLegLists[1].IsLegClosed())
		{
			return _CatchLinesLegLists[1];
		}
		for (int i = 0; i < _CatchLinesLegLists.Count; i++)
		{
			if (_CatchLinesLegLists[i] != null && _CatchLinesLegLists[i].LegCount > 0 && RevitDefines.GT(RevitGeometry.eplGeoE_CalcLegListLength(_CatchLinesLegLists[i]), 0.0))
			{
				if (_CombinedLegList == null)
				{
					_CombinedLegList = Eps_Leg.CopyLeg(_CatchLinesLegLists[i]);
				}
				else
				{
					Eps_Leg.CombineLegListsAtShortestGaps(ref _CombinedLegList, _CatchLinesLegLists[i]);
				}
			}
		}
		if (_CombinedLegList != null && !_CombinedLegList.IsLegClosed())
		{
			Eps_Leg eps_Leg = null;
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = new Eps_Line();
			eps_Leg.Line.StartPnt = _CombinedLegList.LegEndPnt;
			eps_Leg.Line.EndPnt = _CombinedLegList.LegStartPnt;
			_CombinedLegList.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
		}
		return _CombinedLegList;
	}

	public static void RefreshSurface(Document _Document, EPTransactionContainer _TransactionContainer, ref TopographySurfaceContainer _NewPointsSurface, ref FamilyInstance _PlacementFamilyInstance)
	{
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarUpdatingModelMessage");
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = 12;
		List<Eps_Point> allVertices = _NewPointsSurface.GetAllVertices();
		List<XYZ> PointXYZList = new List<XYZ>();
		for (int i = 0; i < allVertices.Count; i++)
		{
			PointXYZList.Add(allVertices[i].XYZ);
		}
		progressBarForm.UpdateProgressBar(1);
		if (PointXYZList.Count > 0)
		{
			_TransactionContainer.BeginTransaction();
			Parameter parameter = null;
			RemoveDuplicateXY(ref PointXYZList);
			TopographySurface topographySurface = TerrainApi.Create(_Document, PointXYZList, _NewPointsSurface.TopographySurface);
			_Document.Regenerate();
			progressBarForm.UpdateProgressBar(7);
			TerrainApi.CopyMaterial(_NewPointsSurface.TopographySurface, topographySurface);
			parameter = ((Element)_NewPointsSurface.TopographySurface).get_Parameter(BuiltInParameter.PHASE_CREATED);
			if (parameter != null && parameter.AsElementId() != null && parameter.AsElementId() != ElementId.InvalidElementId)
			{
				Parameter parameter3 = ((Element)topographySurface).get_Parameter(BuiltInParameter.PHASE_CREATED);
				if (parameter3 != null && !parameter3.IsReadOnly)
				{
					ElementId value2 = parameter.AsElementId();
					parameter3.Set(value2);
				}
			}
			parameter = ((Element)_NewPointsSurface.TopographySurface).get_Parameter(BuiltInParameter.PHASE_DEMOLISHED);
			if (parameter != null && parameter.AsElementId() != null && parameter.AsElementId() != ElementId.InvalidElementId)
			{
				Parameter parameter4 = ((Element)topographySurface).get_Parameter(BuiltInParameter.PHASE_DEMOLISHED);
				if (parameter4 != null && !parameter4.IsReadOnly)
				{
					ElementId value3 = parameter.AsElementId();
					parameter4.Set(value3);
				}
			}
			parameter = TerrainApi.NameParameter(((Element)_NewPointsSurface.TopographySurface));
			if (parameter != null && parameter.AsString() != null && parameter.AsString() != string.Empty)
			{
				Parameter parameter5 = TerrainApi.NameParameter(((Element)topographySurface));
				if (parameter5 != null && !parameter5.IsReadOnly)
				{
					parameter5.Set(parameter.AsString());
				}
			}
			_Document.Delete(_NewPointsSurface.TopographySurface.Id);
			progressBarForm.UpdateProgressBar(3);
			_NewPointsSurface = new TopographySurfaceContainer(_Document.Application, topographySurface, _bFindMinMaxPoints: true);
			if (_PlacementFamilyInstance != null)
			{
				parameter = _PlacementFamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(topographySurface.UniqueId);
				}
			}
			_TransactionContainer.EndTransaction();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
	}

	public static void CreateProposedSurface(Document _Document, EPTransactionContainer _TransactionContainer, FamilySymbol _PlacementSymbol, List<Eps_Point> _PointList, string _strToposurfaceName, ElementId _MaterialId, ElementId _PhaseCreated, ElementId _PhaseDemolished)
	{
		TopographySurface _ProposedTopographySurface = null;
		FamilyInstance _PlacementFamilyInstance = null;
		CreateProposedSurface(_Document, _TransactionContainer, _PlacementSymbol, _PointList, _strToposurfaceName, _MaterialId, _PhaseCreated, _PhaseDemolished, ref _ProposedTopographySurface, ref _PlacementFamilyInstance);
	}

	public static void CreateProposedSurface(Document _Document, EPTransactionContainer _TransactionContainer, FamilySymbol _PlacementSymbol, List<Eps_Point> _PointList, ElementId _MaterialId, ref TopographySurface _ProposedTopographySurface, ref FamilyInstance _PlacementFamilyInstance, TopographySurface template = null)
	{
		ElementId phaseCreated = null;
		ElementId phaseDemolished = null;
		CreateProposedSurface(_Document, _TransactionContainer, _PlacementSymbol, _PointList, string.Empty, _MaterialId, phaseCreated, phaseDemolished, ref _ProposedTopographySurface, ref _PlacementFamilyInstance, template);
	}

	public static void CreateProposedSurface(Document _Document, EPTransactionContainer _TransactionContainer, FamilySymbol _PlacementSymbol, List<Eps_Point> _PointList, string _strToposurfaceName, ElementId _MaterialId, ElementId _PhaseCreated, ElementId _PhaseDemolished, ref TopographySurface _ProposedTopographySurface, ref FamilyInstance _PlacementFamilyInstance, TopographySurface template = null)
	{
		Parameter parameter = null;
		Parameter parameter2 = null;
		List<XYZ> PointXYZList = new List<XYZ>();
		for (int i = 0; i < _PointList.Count; i++)
		{
			PointXYZList.Add(_PointList[i].XYZ);
		}
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingNewProposedToposurfaceMessage");
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = Convert.ToInt32(2);
		progressBarForm.UpdateProgressBar(1);
		_TransactionContainer.BeginTransaction();
		RemoveDuplicateXY(ref PointXYZList);
		_ProposedTopographySurface = TerrainApi.Create(_Document, PointXYZList, template);
		_Document.Regenerate();
		if (_MaterialId != null)
		{
			TerrainApi.SetMaterialId(_ProposedTopographySurface, _MaterialId);
		}
		if (_PhaseCreated != null)
		{
			((Element)_ProposedTopographySurface).get_Parameter(BuiltInParameter.PHASE_CREATED)?.Set(_PhaseCreated);
		}
		if (_PhaseDemolished != null)
		{
			((Element)_ProposedTopographySurface).get_Parameter(BuiltInParameter.PHASE_DEMOLISHED)?.Set(_PhaseDemolished);
		}
		parameter2 = TerrainApi.NameParameter(((Element)_ProposedTopographySurface));
		if (parameter2 != null && !parameter2.IsReadOnly)
		{
			TerrainApi.SetUniqueName(_ProposedTopographySurface, _strToposurfaceName != string.Empty ? _strToposurfaceName : EPShared.LocalRM.GetString("ProposedToposurfaceString"));
		}
		ElementSet elementSet = new ElementSet();
		if (_PlacementSymbol != null)
		{
			_PlacementFamilyInstance = RevitUtility.NewFamilyInstance(_Document, new XYZ(0.0, 0.0, 0.0), _PlacementSymbol, _ProposedTopographySurface, _Document.ActiveView.GenLevel, StructuralType.NonStructural, ref _TransactionContainer);
			parameter = _PlacementFamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
			if (parameter != null && !parameter.IsReadOnly)
			{
				parameter.Set(((Element)_ProposedTopographySurface).UniqueId);
			}
			parameter = _PlacementFamilyInstance.LookupParameter(RevitParameters.IsProposedSurfaceFamilyParameter);
			if (parameter != null && !parameter.IsReadOnly)
			{
				parameter.Set(Convert.ToInt32(value: true));
			}
			elementSet.Insert(_PlacementFamilyInstance);
		}
		RevitUtility.HideElementsInAllViews(_Document, elementSet);
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		_TransactionContainer.EndTransaction();
	}

	public static bool RunLocate(Document document, LandcaddID landcaddId)
	{
		if (!TryPrepareProposedTerrain(document, out TopographySurfaceContainer baseContainer, out FamilyInstance basePlacement, out TopographySurfaceContainer proposedContainer, out FamilyInstance proposedPlacement))
		{
			return false;
		}
		new SiteComponentCreation(document, baseContainer, basePlacement, proposedContainer, proposedPlacement, landcaddId).Run();
		return true;
	}

	public static bool TryPrepareProposedTerrain(Document document, out TopographySurfaceContainer baseContainer, out FamilyInstance basePlacement, out TopographySurfaceContainer proposedContainer, out FamilyInstance proposedPlacement)
	{
		baseContainer = null;
		basePlacement = null;
		proposedContainer = null;
		proposedPlacement = null;
		FindAndCreateProposedLandcaddTopographySurface(document, ref baseContainer, ref basePlacement, ref proposedContainer, ref proposedPlacement);
		if (proposedContainer != null && proposedPlacement != null)
		{
			return true;
		}
		SetBaseToposurface setBaseToposurface = new SetBaseToposurface(document);
		if (!setBaseToposurface.Run())
		{
			return false;
		}
		FindAndCreateProposedLandcaddTopographySurface(document, ref baseContainer, ref basePlacement, ref proposedContainer, ref proposedPlacement);
		if (proposedContainer != null && proposedPlacement != null)
		{
			return true;
		}
		if (setBaseToposurface.CreatedProposedSurface != null && setBaseToposurface.CreatedProposedPlacement != null)
		{
			proposedContainer = new TopographySurfaceContainer(document.Application, setBaseToposurface.CreatedProposedSurface, _bFindMinMaxPoints: true);
			proposedPlacement = setBaseToposurface.CreatedProposedPlacement;
			if (setBaseToposurface.CreatedBaseSurface != null)
			{
				baseContainer = new TopographySurfaceContainer(document.Application, setBaseToposurface.CreatedBaseSurface, _bFindMinMaxPoints: true);
			}
			basePlacement = setBaseToposurface.CreatedBasePlacement;
		}
		if (proposedContainer == null || proposedPlacement == null)
		{
			RuntimeDiagnostics.Write("Proposed terrain was not linked after Set Base.");
			return false;
		}
		return true;
	}

	public static void FindAndCreateProposedLandcaddTopographySurface(Document _Document, ref TopographySurfaceContainer _BaseSurfaceContainer, ref FamilyInstance _BasePlacementFamilyInstance, ref TopographySurfaceContainer _ProposedSurfaceContainer, ref FamilyInstance _ProposedPlacementFamilyInstance)
	{
		FindBaseLandcaddTopographySurface(_Document, ref _BaseSurfaceContainer, ref _BasePlacementFamilyInstance, ref _ProposedSurfaceContainer, ref _ProposedPlacementFamilyInstance);
		if (_ProposedSurfaceContainer != null && _ProposedPlacementFamilyInstance != null)
		{
			return;
		}
		if (TryAdoptExistingProposed(_Document, _BaseSurfaceContainer, _BasePlacementFamilyInstance, ref _ProposedSurfaceContainer, ref _ProposedPlacementFamilyInstance))
		{
			return;
		}
		EPTransactionContainer ePTransactionContainer = new EPTransactionContainer(_Document);
		ePTransactionContainer.Start(EPShared.LocalRM.GetString("SurfaceCleanUpTransaction"));
		if (!TerrainApi.UsesToposolids)
		{
			RevitUtility.RemoveAllSiteComponents(_Document, ePTransactionContainer);
		}
		if (_BaseSurfaceContainer != null && _BasePlacementFamilyInstance != null)
		{
			if (_ProposedPlacementFamilyInstance != null)
			{
				ePTransactionContainer.BeginTransaction();
				_Document.Delete(_ProposedPlacementFamilyInstance.Id);
				ePTransactionContainer.EndTransaction();
			}
			ElementId materialId = TerrainApi.GetMaterialId(_BaseSurfaceContainer.TopographySurface);
			TopographySurface _ProposedTopographySurface = null;
			CreateProposedSurface(_Document, ePTransactionContainer, _BasePlacementFamilyInstance.Symbol, _BaseSurfaceContainer.GetAllVertices(), materialId, ref _ProposedTopographySurface, ref _ProposedPlacementFamilyInstance, _BaseSurfaceContainer.TopographySurface);
			_ProposedSurfaceContainer = new TopographySurfaceContainer(_Document.Application, _ProposedTopographySurface, _bFindMinMaxPoints: true);
		}
		else if (_ProposedPlacementFamilyInstance != null || _BasePlacementFamilyInstance != null)
		{
			if (_ProposedPlacementFamilyInstance != null)
			{
				ePTransactionContainer.BeginTransaction();
				_Document.Delete(_ProposedPlacementFamilyInstance.Id);
				ePTransactionContainer.EndTransaction();
			}
			if (_BasePlacementFamilyInstance != null)
			{
				ePTransactionContainer.BeginTransaction();
				_Document.Delete(_BasePlacementFamilyInstance.Id);
				ePTransactionContainer.EndTransaction();
			}
		}
		ePTransactionContainer.Commit();
	}

	private static bool TryAdoptExistingProposed(Document document, TopographySurfaceContainer baseSurface, FamilyInstance basePlacement, ref TopographySurfaceContainer proposedContainer, ref FamilyInstance proposedPlacement)
	{
		TopographySurface found = TerrainApi.FindProposedForBase(document, baseSurface?.TopographySurface);
		if (found == null) return false;
		FamilyInstance placement = proposedPlacement;
		if (placement == null)
		{
			foreach (FamilyInstance instance in new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site).Cast<FamilyInstance>())
			{
				Parameter proposed = instance.LookupParameter(RevitParameters.IsProposedSurfaceFamilyParameter);
				if (proposed != null && proposed.AsInteger() == 1)
				{
					placement = instance;
					break;
				}
			}
		}
		EPTransactionContainer transaction = new EPTransactionContainer(document);
		transaction.Start("Relink proposed terrain");
		transaction.BeginTransaction();
		if (placement == null && basePlacement?.Symbol != null)
		{
			placement = RevitUtility.NewFamilyInstance(document, XYZ.Zero, basePlacement.Symbol, found, document.ActiveView.GenLevel, StructuralType.NonStructural, ref transaction);
			Parameter proposedFlag = placement?.LookupParameter(RevitParameters.IsProposedSurfaceFamilyParameter);
			if (proposedFlag != null && !proposedFlag.IsReadOnly) proposedFlag.Set(1);
		}
		Parameter uniqueId = placement?.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
		if (uniqueId != null && !uniqueId.IsReadOnly) uniqueId.Set(found.UniqueId);
		transaction.EndTransaction();
		transaction.Commit();
		transaction.Dispose();
		if (placement == null) return false;
		proposedContainer = new TopographySurfaceContainer(document.Application, found, _bFindMinMaxPoints: true);
		proposedPlacement = placement;
		RuntimeDiagnostics.Write("Reused existing proposed terrain instead of cloning the ungraded base.");
		return true;
	}

	public static void FindBaseLandcaddTopographySurface(Document _Document, bool _bIsProposedSurface, ref TopographySurfaceContainer _SiteTopographySurfaceContainer, ref FamilyInstance _TopoPlacementFamilyInstance)
	{
		TopographySurfaceContainer _BaseTopographySurfaceContainer = null;
		TopographySurfaceContainer _ProposedTopographySurfaceContainer = null;
		FamilyInstance _BasePlacementFamilyInstance = null;
		FamilyInstance _ProposedPlacementFamilyInstance = null;
		FindBaseLandcaddTopographySurface(_Document, ref _BaseTopographySurfaceContainer, ref _BasePlacementFamilyInstance, ref _ProposedTopographySurfaceContainer, ref _ProposedPlacementFamilyInstance);
		if (_bIsProposedSurface)
		{
			_SiteTopographySurfaceContainer = _ProposedTopographySurfaceContainer;
			_TopoPlacementFamilyInstance = _ProposedPlacementFamilyInstance;
		}
		else
		{
			_SiteTopographySurfaceContainer = _BaseTopographySurfaceContainer;
			_TopoPlacementFamilyInstance = _BasePlacementFamilyInstance;
		}
	}

	public static void FindBaseLandcaddTopographySurface(Document _Document, ref TopographySurfaceContainer _BaseTopographySurfaceContainer, ref FamilyInstance _BasePlacementFamilyInstance, ref TopographySurfaceContainer _ProposedTopographySurfaceContainer, ref FamilyInstance _ProposedPlacementFamilyInstance)
	{
		FindBaseLandcaddTopographySurface(_Document, ref _BaseTopographySurfaceContainer, ref _BasePlacementFamilyInstance, ref _ProposedTopographySurfaceContainer, ref _ProposedPlacementFamilyInstance, null);
	}

	public static void FindBaseLandcaddTopographySurface(Document _Document, ref TopographySurfaceContainer _BaseTopographySurfaceContainer, ref FamilyInstance _BasePlacementFamilyInstance, ref TopographySurfaceContainer _ProposedTopographySurfaceContainer, ref FamilyInstance _ProposedPlacementFamilyInstance, EPTransactionContainer _TransactionContainer)
	{
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		Parameter parameter = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.TopoSurfacePlacement).ToString("000"), ignoreCase: true) == 0 && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list2 = filteredElementCollector.OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).ToElements();
		filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list3 = filteredElementCollector.OfClass(typeof(CeilingAndFloor)).OfCategory(BuiltInCategory.OST_BuildingPad).ToElements();
		List<TopographySurfaceContainer> list4 = new List<TopographySurfaceContainer>();
		Parameter parameter2 = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		ElementSet elementSet = new ElementSet();
		ElementSet elementSet2 = new ElementSet();
		EPTransactionContainer ePTransactionContainer = null;
		if (_TransactionContainer != null)
		{
			ePTransactionContainer = _TransactionContainer;
		}
		else
		{
			ePTransactionContainer = new EPTransactionContainer(_Document);
			ePTransactionContainer.Start(EPShared.LocalRM.GetString("RelinkSurfacesTransaction"));
		}
		foreach (Element item2 in list2)
		{
			if (item2 == null || !(TerrainApi.AsSurface(item2) is {} topographySurface))
			{
				continue;
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				Element element = list[num];
				if (element is FamilyInstance { Category: not null } familyInstance)
				{
					SiteworksIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
					if (SiteworksIDParameter != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.TopoSurfacePlacement).ToString("000"), ignoreCase: true) == 0)
					{
						parameter = familyInstance.LookupParameter(RevitParameters.IsProposedSurfaceFamilyParameter);
						if (parameter != null)
						{
							parameter2 = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
							string storedId = parameter2?.AsString();
							if (TerrainApi.MatchesLinkedId(storedId, item2))
							{
								if (!Convert.ToBoolean(parameter.AsInteger()))
								{
									_BasePlacementFamilyInstance = familyInstance;
									_BaseTopographySurfaceContainer = new TopographySurfaceContainer(_Document.Application, topographySurface, _bFindMinMaxPoints: true);
									flag2 = true;
								}
								else
								{
									_ProposedPlacementFamilyInstance = familyInstance;
									_ProposedTopographySurfaceContainer = new TopographySurfaceContainer(_Document.Application, topographySurface, _bFindMinMaxPoints: true);
									flag3 = true;
								}
								list.RemoveAt(num);
								flag = true;
								break;
							}
							parameter2 = TerrainApi.NameParameter(((Element)topographySurface));
							if (parameter2 != null && parameter2.AsString() != null && parameter2.AsString() != string.Empty)
							{
								if (!flag2 && !Convert.ToBoolean(parameter.AsInteger()) && parameter2.AsString().ToLower().Contains(EPShared.LocalRM.GetString("BaseToposurfaceString").ToLower()))
								{
									if (elementSet.Size == 0)
									{
										_BasePlacementFamilyInstance = familyInstance;
										_BaseTopographySurfaceContainer = new TopographySurfaceContainer(_Document.Application, topographySurface, _bFindMinMaxPoints: true);
									}
									elementSet.Insert(familyInstance);
									flag4 = true;
								}
								else if (!flag3 && Convert.ToBoolean(parameter.AsInteger()) && parameter2.AsString().ToLower().Contains(EPShared.LocalRM.GetString("ProposedToposurfaceString").ToLower()))
								{
									if (elementSet2.Size == 0)
									{
										_ProposedPlacementFamilyInstance = familyInstance;
										_ProposedTopographySurfaceContainer = new TopographySurfaceContainer(_Document.Application, topographySurface, _bFindMinMaxPoints: true);
									}
									elementSet2.Insert(familyInstance);
									flag4 = true;
								}
							}
						}
					}
				}
			}
			if (!flag && !flag4)
			{
				TopographySurfaceType topographySurfaceType = TopographySurfaceContainer.FindTopographySurfaceType(_Document.Application, topographySurface);
				if ((uint)(topographySurfaceType - 1) <= 1u)
				{
					list4.Add(new TopographySurfaceContainer(_Document.Application, topographySurface, _bFindMinMaxPoints: true, topographySurfaceType));
				}
			}
			else
			{
				flag = false;
				flag4 = false;
			}
		}
		foreach (Element item3 in list3)
		{
			if (item3 is CeilingAndFloor buildingPad)
			{
				list4.Add(new TopographySurfaceContainer(_Document.Application, buildingPad, _bFindMinMaxPoints: true));
			}
		}
		if (!flag2 || !flag3)
		{
			if (elementSet.Size > 0)
			{
				if (elementSet.Size == 1)
				{
					if (_BasePlacementFamilyInstance != null && _BaseTopographySurfaceContainer != null)
					{
						parameter2 = _BasePlacementFamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
						if (parameter2 != null && !parameter2.IsReadOnly)
						{
							parameter2.Set(_BaseTopographySurfaceContainer.TopographySurface.UniqueId);
							flag5 = true;
						}
					}
				}
				else
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("BaseSurfaceLinkBrokenMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					SetBaseToposurface setBaseToposurface = new SetBaseToposurface(_Document, _bPickIndividualSurface: true);
					if (setBaseToposurface.Run())
					{
						ElementSet instanceSet = setBaseToposurface.InstanceSet;
						foreach (Element item4 in instanceSet)
						{
							if (!(TerrainApi.AsSurface(item4) is {} topographySurface2))
							{
								continue;
							}
							_BaseTopographySurfaceContainer = new TopographySurfaceContainer(_Document.Application, topographySurface2, _bFindMinMaxPoints: true);
							if (_BasePlacementFamilyInstance != null && _BasePlacementFamilyInstance.Symbol != null)
							{
								_BasePlacementFamilyInstance = RevitUtility.NewFamilyInstance(_Document, new XYZ(0.0, 0.0, 0.0), _BasePlacementFamilyInstance.Symbol, topographySurface2, _Document.ActiveView.GenLevel, StructuralType.NonStructural, ref ePTransactionContainer);
								parameter2 = _BasePlacementFamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
								if (parameter2 != null && !parameter2.IsReadOnly)
								{
									parameter2.Set(topographySurface2.UniqueId);
								}
								parameter2 = _BasePlacementFamilyInstance.LookupParameter(RevitParameters.IsProposedSurfaceFamilyParameter);
								if (parameter2 != null && !parameter2.IsReadOnly)
								{
									parameter2.Set(Convert.ToInt32(value: false));
								}
								if (elementSet.Size > 0)
								{
									ePTransactionContainer.BeginTransaction();
									_Document.Delete(ElementSetConverter.ToIDList(elementSet));
									ePTransactionContainer.EndTransaction();
								}
								flag5 = true;
							}
						}
					}
				}
				elementSet.Clear();
			}
			else if (elementSet2.Size > 0)
			{
				if (elementSet2.Size == 1)
				{
					if (_ProposedPlacementFamilyInstance != null && _ProposedTopographySurfaceContainer != null)
					{
						parameter2 = _ProposedPlacementFamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
						if (parameter2 != null && !parameter2.IsReadOnly)
						{
							parameter2.Set(_ProposedTopographySurfaceContainer.TopographySurface.UniqueId);
							flag6 = true;
						}
					}
				}
				else
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("ProposedSurfaceLinkBrokenMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					SetBaseToposurface setBaseToposurface2 = new SetBaseToposurface(_Document, _bPickIndividualSurface: true);
					if (setBaseToposurface2.Run())
					{
						ElementSet instanceSet2 = setBaseToposurface2.InstanceSet;
						foreach (Element item5 in instanceSet2)
						{
							if (!(TerrainApi.AsSurface(item5) is {} topographySurface3))
							{
								continue;
							}
							_ProposedTopographySurfaceContainer = new TopographySurfaceContainer(_Document.Application, topographySurface3, _bFindMinMaxPoints: true);
							if (_ProposedPlacementFamilyInstance != null && _ProposedPlacementFamilyInstance.Symbol != null)
							{
								_ProposedPlacementFamilyInstance = RevitUtility.NewFamilyInstance(_Document, new XYZ(0.0, 0.0, 0.0), _ProposedPlacementFamilyInstance.Symbol, topographySurface3, _Document.ActiveView.GenLevel, StructuralType.NonStructural, ref ePTransactionContainer);
								parameter2 = _ProposedPlacementFamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
								if (parameter2 != null && !parameter2.IsReadOnly)
								{
									parameter2.Set(topographySurface3.UniqueId);
								}
								parameter2 = _ProposedPlacementFamilyInstance.LookupParameter(RevitParameters.IsProposedSurfaceFamilyParameter);
								if (parameter2 != null && !parameter2.IsReadOnly)
								{
									parameter2.Set(Convert.ToInt32(value: true));
								}
								if (elementSet2.Size > 0)
								{
									ePTransactionContainer.BeginTransaction();
									_Document.Delete(ElementSetConverter.ToIDList(elementSet2));
									ePTransactionContainer.EndTransaction();
								}
								flag6 = true;
							}
						}
					}
				}
				elementSet2.Clear();
			}
		}
		if (_TransactionContainer != null)
		{
			return;
		}
		bool flag7 = false;
		bool _bIssueBuildingPadWarning = true;
		if (_BasePlacementFamilyInstance != null && _BaseTopographySurfaceContainer != null)
		{
			flag7 = AddSubRegionBuildingPadsToMainSurface(_Document, list4, ref _BaseTopographySurfaceContainer, _bAddBuildingPadsIfPresent: false, ref _bIssueBuildingPadWarning);
			if (flag7)
			{
				ePTransactionContainer.SetName(EPShared.LocalRM.GetString("AddBuildingPadsToSurfaceTransaction"));
				List<TopographySurfaceContainer> subregionBuildingPadList = _BaseTopographySurfaceContainer.SubregionBuildingPadList;
				RefreshSurface(_Document, ePTransactionContainer, ref _BaseTopographySurfaceContainer, ref _BasePlacementFamilyInstance);
				for (int num2 = subregionBuildingPadList.Count - 1; num2 >= 0; num2--)
				{
					if (subregionBuildingPadList[num2].TopographySurfaceType == TopographySurfaceType.BuildingPad && subregionBuildingPadList[num2].TopographySurface != null)
					{
						try
						{
							string uniqueId = subregionBuildingPadList[num2].TopographySurface.UniqueId;
							_Document.Delete(subregionBuildingPadList[num2].TopographySurface.Id);
						}
						catch (InvalidObjectException)
						{
						}
					}
				}
			}
		}
		bool flag8 = false;
		if (_ProposedPlacementFamilyInstance != null && _ProposedTopographySurfaceContainer != null)
		{
			flag8 = AddSubRegionBuildingPadsToMainSurface(_Document, list4, ref _ProposedTopographySurfaceContainer, flag7, ref _bIssueBuildingPadWarning);
			if (flag8)
			{
				ePTransactionContainer.SetName(EPShared.LocalRM.GetString("AddBuildingPadsToSurfaceTransaction"));
				ElementSet elementSet3 = new ElementSet();
				List<Element> list5 = new List<Element>();
				FamilySymbol siteComponentPlacementSymbol = GetSiteComponentPlacementSymbol(_Document, ePTransactionContainer);
				FamilySymbol siteComponentSymbol = GetSiteComponentSymbol(_Document, ePTransactionContainer, LandcaddID.SoftTerrain, "Building Pads", "no slope", "Building Pads.rfa");
				for (int num3 = _ProposedTopographySurfaceContainer.SubregionBuildingPadList.Count - 1; num3 >= 0; num3--)
				{
					if (_ProposedTopographySurfaceContainer.SubregionBuildingPadList[num3].TopographySurfaceType == TopographySurfaceType.BuildingPad && _ProposedTopographySurfaceContainer.SubregionBuildingPadList[num3].TopographySurface != null)
					{
						if (siteComponentPlacementSymbol != null)
						{
							ePTransactionContainer.BeginTransaction();
							ModelCurveArray modelCurveArray = RevitUtility.NewModelCurveArray(_Document, _ProposedTopographySurfaceContainer.SubregionBuildingPadList[num3].BuildingPadLeg);
							foreach (ModelCurve item6 in modelCurveArray)
							{
								elementSet3.Insert(item6);
								list5.Add(item6);
							}
							if (elementSet3.Size > 0)
							{
								RevitUtility.HideElementsInAllViews(_Document, elementSet3);
								elementSet3.Clear();
							}
							ePTransactionContainer.EndTransaction();
							int nTopoDrawOrder = GetMaxExistingTopoDrawOrder(_Document) + 1;
							SitePlacementSettings sitePlacementSettings = new SitePlacementSettings(LandcaddID.SoftTerrain, _Document.ActiveView.GenLevel, siteComponentSymbol, siteComponentPlacementSymbol, EPShared.LocalRM.GetString("BuildingPadString"), nTopoDrawOrder, ePTransactionContainer);
							sitePlacementSettings.ProjectInnerSlopes = 1;
							RevitUtility.CreatePlacementSiteComponent(_Document, sitePlacementSettings, list5);
							list5.Clear();
						}
						try
						{
							string uniqueId2 = _ProposedTopographySurfaceContainer.SubregionBuildingPadList[num3].TopographySurface.UniqueId;
							_Document.Delete(_ProposedTopographySurfaceContainer.SubregionBuildingPadList[num3].TopographySurface.Id);
						}
						catch (InvalidObjectException)
						{
						}
					}
				}
				RefreshSurface(_Document, ePTransactionContainer, ref _ProposedTopographySurfaceContainer, ref _ProposedPlacementFamilyInstance);
			}
		}
		if (flag7 || flag8 || flag5 || flag6)
		{
			ePTransactionContainer.Commit();
		}
		else
		{
			ePTransactionContainer.Rollback();
		}
	}

	private static FamilySymbol GetSiteComponentSymbol(Document _Document, EPTransactionContainer _TransactionContainer, LandcaddID _LandcaddID, string _strFamilyName, string _strFamilySymbolName, string _strSampleFileName)
	{
		Parameter CurSiteworksID = null;
		FamilySymbol familySymbol = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (CurSiteworksID = CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(_LandcaddID).ToString("000"), ignoreCase: true) == 0);
		List<Element> list = source.ToList();
		familySymbol = null;
		foreach (Element item in list)
		{
			if (item is FamilySymbol { Category: not null } familySymbol2)
			{
				CurSiteworksID = familySymbol2.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
				if (CurSiteworksID != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(_LandcaddID).ToString("000"), ignoreCase: true) == 0 && string.Compare(familySymbol2.Family.Name, _strFamilyName, ignoreCase: true) == 0 && string.Compare(familySymbol2.Name, _strFamilySymbolName, ignoreCase: true) == 0)
				{
					return familySymbol2;
				}
			}
		}
		string _strFullFamilyFilename = string.Empty;
		if (RevitUtility.GetFullFamilyFilenameIfExist(_strSampleFileName, ref _strFullFamilyFilename))
		{
			Family _Family = null;
			int num = _strFullFamilyFilename.LastIndexOf("\\");
			int num2 = _strFullFamilyFilename.LastIndexOf(".");
			string text = _strFullFamilyFilename.Substring(num + 1, num2 - num - 1);
			_TransactionContainer.BeginTransaction();
			_TransactionContainer.BeginTransaction();
			RevitUtility.LoadFamily(_Document, _strFullFamilyFilename, out _Family, _bUseFailureHandling: true, ref _TransactionContainer);
			_TransactionContainer.EndTransaction();
			if (_Family != null && _Family.GetFamilySymbolIds() != null)
			{
				foreach (ElementId familySymbolId in _Family.GetFamilySymbolIds())
				{
					if (_Document.GetElement(familySymbolId) is FamilySymbol familySymbol3)
					{
						Parameter parameter = null;
						parameter = familySymbol3.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
						if (parameter != null && string.Compare(parameter.AsString(), Convert.ToInt32(_LandcaddID).ToString("000"), ignoreCase: true) == 0 && string.Compare(familySymbol3.Family.Name, _strFamilyName, ignoreCase: true) == 0 && string.Compare(familySymbol3.Name, _strFamilySymbolName, ignoreCase: true) == 0)
						{
							return familySymbol3;
						}
					}
				}
			}
		}
		return null;
	}

	private static FamilySymbol GetSiteComponentPlacementSymbol(Document _Document, EPTransactionContainer _TransactionContainer)
	{
		Parameter CurSiteworksID = null;
		FamilySymbol familySymbol = null;
		FamilySymbol result = null;
		bool flag = true;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Site);
		CurSiteworksID = null;
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (CurSiteworksID = CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0);
		List<Element> list = source.ToList();
		flag = false;
		familySymbol = null;
		foreach (Element item in list)
		{
			if (item is FamilySymbol { Category: not null } familySymbol2)
			{
				CurSiteworksID = familySymbol2.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
				if (CurSiteworksID != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0)
				{
					result = familySymbol2;
					flag = true;
					break;
				}
			}
		}
		string _strFullFamilyFilename = string.Empty;
		Family _Family = null;
		if (!flag && RevitUtility.GetFullFamilyFilenameIfExist("Site Component Placement.rfa", ref _strFullFamilyFilename))
		{
			_TransactionContainer.BeginTransaction();
			RevitUtility.LoadFamily(_Document, _strFullFamilyFilename, out _Family, _bUseFailureHandling: true, ref _TransactionContainer);
			_TransactionContainer.EndTransaction();
			if (_Family != null && _Family.GetFamilySymbolIds() != null)
			{
				using IEnumerator<ElementId> enumerator2 = _Family.GetFamilySymbolIds().GetEnumerator();
				if (enumerator2.MoveNext())
				{
					ElementId current2 = enumerator2.Current;
					if (_Document.GetElement(current2) is FamilySymbol familySymbol3)
					{
						result = familySymbol3;
					}
				}
			}
		}
		return result;
	}

	private static int GetMaxExistingTopoDrawOrder(Document _Document)
	{
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		Parameter LocateTypeParameter = null;
		int num = -1;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0 && (LocateTypeParameter = FindFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter)) != null && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list2 = filteredElementCollector.OfClass(typeof(CurveElement)).OfCategory(BuiltInCategory.OST_Lines).ToElements();
		Parameter parameter = null;
		Parameter parameter2 = null;
		Parameter parameter3 = null;
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
			{
				continue;
			}
			SiteworksIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (SiteworksIDParameter == null)
			{
				continue;
			}
			LocateTypeParameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (LocateTypeParameter == null || string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) != 0)
			{
				continue;
			}
			parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
			if (parameter == null)
			{
				continue;
			}
			parameter2 = familyInstance.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
			if (parameter2 == null || !(parameter2.AsString() != string.Empty))
			{
				continue;
			}
			for (int num2 = 0; num2 < list2.Count; num2++)
			{
				Element element = list2[num2];
				if (element != null && element.Category != null && element.Category.Id.Value64() == BuiltInCategory.OST_Lines.GetHashCode() && string.Compare(parameter.AsString(), element.UniqueId, ignoreCase: true) == 0)
				{
					parameter3 = familyInstance.LookupParameter(RevitParameters.TopoDrawOrderFamilyParameter);
					if (parameter3 != null && num < parameter3.AsInteger())
					{
						num = parameter3.AsInteger();
					}
					list2.RemoveAt(num2);
					break;
				}
			}
			if (list2.Count == 0)
			{
				break;
			}
		}
		return num;
	}

	public static TopographySurface FindTopoSurfaceInstance(Document _Document, string _strFormUniqueId)
	{
		TopographySurface topographySurface = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		List<Element> list = filteredElementCollector.OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).ToList();
		topographySurface = null;
		foreach (Element item in list)
		{
			if (TerrainApi.AsSurface(item) is { Category: not null } topographySurface2 && string.Compare(_strFormUniqueId, topographySurface2.UniqueId, ignoreCase: true) == 0)
			{
				return topographySurface2;
			}
		}
		return null;
	}

	public static bool AddSubRegionBuildingPadsToMainSurface(Document _Document, List<TopographySurfaceContainer> _SubRegionBuildingPadList, ref TopographySurfaceContainer _SurfaceContainer, bool _bAddBuildingPadsIfPresent, ref bool _bIssueBuildingPadWarning)
	{
		List<TopographySurfaceContainer> _SurfaceList = new List<TopographySurfaceContainer>();
		_SurfaceList.Add(_SurfaceContainer);
		return AddSubRegionBuildingPadsToMainSurfaceList(_Document, _SubRegionBuildingPadList, ref _SurfaceList, _bAddBuildingPadsIfPresent, ref _bIssueBuildingPadWarning);
	}

	public static bool AddSubRegionBuildingPadsToMainSurfaceList(Document _Document, List<TopographySurfaceContainer> _SubRegionBuildingPadList, ref List<TopographySurfaceContainer> _SurfaceList)
	{
		bool _bIssueBuildingPadWarning = false;
		return AddSubRegionBuildingPadsToMainSurfaceList(_Document, _SubRegionBuildingPadList, ref _SurfaceList, _bAddBuildingPadsIfPresent: true, ref _bIssueBuildingPadWarning);
	}

	public static bool AddSubRegionBuildingPadsToMainSurfaceList(Document _Document, List<TopographySurfaceContainer> _SubRegionBuildingPadList, ref List<TopographySurfaceContainer> _SurfaceList, bool _bAddBuildingPadsIfPresent, ref bool _bIssueBuildingPadWarning)
	{
		bool result = _bAddBuildingPadsIfPresent;
		if (TerrainApi.UsesToposolids && _SurfaceList != null && _SubRegionBuildingPadList != null)
		{
			foreach (var subdivision in _SubRegionBuildingPadList)
			{
				if (subdivision.TopographySurface == null) continue;
				var hostId = TerrainApi.HostId(subdivision.TopographySurface);
				var host = _SurfaceList.FirstOrDefault(x => x.TopographySurface != null && x.TopographySurface.Id == hostId);
				if (host != null && !host.SubregionBuildingPadList.Contains(subdivision)) host.SubregionBuildingPadList.Add(subdivision);
			}
			// Native subdivisions follow the host shape; no legacy point merge or
			// replacement of the host is needed.
			return false;
		}
		if (_SurfaceList != null && _SurfaceList.Count > 0 && _SubRegionBuildingPadList != null && _SubRegionBuildingPadList.Count > 0)
		{
			List<TopographySurfaceContainer> list = null;
			ElementId elementId = null;
			ElementId elementId2 = null;
			List<TopographySurfaceContainer> list2 = new List<TopographySurfaceContainer>();
			list2.AddRange(_SubRegionBuildingPadList);
			for (int i = 0; i < list2.Count; i++)
			{
				if (list2[i].TopographySurface != null)
				{
					elementId = list2[i].TopographySurface.CreatedPhaseId;
					elementId2 = list2[i].TopographySurface.DemolishedPhaseId;
				}
				else if (list2[i].BuildingPad != null)
				{
					elementId = list2[i].BuildingPad.CreatedPhaseId;
					elementId2 = list2[i].BuildingPad.DemolishedPhaseId;
				}
				for (int j = 0; j < list2[i].Vertices.Count; j++)
				{
					list = GetSurfaceContainerListContainingPoint(list2[i].Vertices[j], _SurfaceList);
					if (list == null || list.Count <= 0)
					{
						continue;
					}
					for (int k = 0; k < list.Count; k++)
					{
						if ((elementId == null || (list[k].TopographySurface != null && list[k].TopographySurface.CreatedPhaseId != null && list[k].TopographySurface.CreatedPhaseId == elementId) || (list[k].BuildingPad != null && list[k].BuildingPad.CreatedPhaseId != null && list[k].BuildingPad.CreatedPhaseId == elementId)) && (elementId2 == null || (list[k].TopographySurface != null && list[k].TopographySurface.DemolishedPhaseId != null && list[k].TopographySurface.DemolishedPhaseId == elementId2) || (list[k].BuildingPad != null && list[k].BuildingPad.DemolishedPhaseId != null && list[k].BuildingPad.DemolishedPhaseId == elementId2)))
						{
							list[k].SubregionBuildingPadList.Add(list2[i]);
						}
						else if (list2[i].TopographySurfaceType == TopographySurfaceType.BuildingPad)
						{
							list[k].BuildingPadPresentInDiffentPhase = true;
							list[k].BuildingPadPresentInDifferentPhaseList.Add(list2[i]);
						}
					}
					list2.RemoveAt(i);
					i--;
					break;
				}
			}
			if (!_bIssueBuildingPadWarning)
			{
				List<TopographySurfaceContainer> list3 = null;
				Eps_Point eps_Point = null;
				bool flag = false;
				for (int l = 0; l < list.Count; l++)
				{
					if (!list[l].BuildingPadPresentInDiffentPhase)
					{
						continue;
					}
					list3 = list[l].BuildingPadPresentInDifferentPhaseList;
					for (int m = 0; m < list3.Count; m++)
					{
						for (int n = 0; n < list3[m].Vertices.Count; n++)
						{
							eps_Point = list3[m].Vertices[n];
							for (int num = list[l].SubregionBuildingPadList.Count - 1; num >= 0; num--)
							{
								if (list[l].SubregionBuildingPadList[num].TopographySurfaceType == TopographySurfaceType.BuildingPad && list[l].SubregionBuildingPadList[num].GetSurfaceContainerContainingPoint(eps_Point) != null)
								{
									list[l].SubregionBuildingPadList.RemoveAt(num);
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
					list3.Clear();
				}
			}
		}
		return result;
	}

	public static TopographySurfaceContainer GetSurfaceContainerContainingPoint(Eps_Point _FindPoint, List<TopographySurfaceContainer> _SurfaceList)
	{
		return GetSurfaceContainerContainingPoint(_FindPoint, _SurfaceList, null, null);
	}

	public static TopographySurfaceContainer GetSurfaceContainerContainingPoint(Eps_Point _FindPoint, List<TopographySurfaceContainer> _SurfaceList, Phase _PhaseCreated)
	{
		return GetSurfaceContainerContainingPoint(_FindPoint, _SurfaceList, _PhaseCreated, null);
	}

	public static TopographySurfaceContainer GetSurfaceContainerContainingPoint(Eps_Point _FindPoint, List<TopographySurfaceContainer> _SurfaceList, Phase _PhaseCreated, Phase _PhaseDemolished)
	{
		TopographySurfaceContainer topographySurfaceContainer = null;
		if (_SurfaceList != null && _SurfaceList.Count > 0)
		{
			for (int i = 0; i < _SurfaceList.Count; i++)
			{
				topographySurfaceContainer = _SurfaceList[i].GetSurfaceContainerContainingPoint(_FindPoint, _PhaseCreated, _PhaseDemolished);
				if (topographySurfaceContainer != null)
				{
					return topographySurfaceContainer;
				}
			}
		}
		return null;
	}

	public static List<Eps_Point> GetSurfacePoints(List<TopographySurfaceContainer> _SurfaceList)
	{
		List<Eps_Point> result = new List<Eps_Point>();
		if (_SurfaceList != null && _SurfaceList.Count > 0)
		{
			List<Eps_Point> list = new List<Eps_Point>();
			for (int i = 0; i < _SurfaceList.Count; i++)
			{
				if (_SurfaceList[i].Vertices == null)
				{
					_SurfaceList[i].FindMinMaxPointsOfTopographySurface();
				}
				if (_SurfaceList[i].Vertices != null)
				{
					list.AddRange(_SurfaceList[i].Vertices);
				}
			}
			IEnumerable<Eps_Point> source = list;
			result = source.Distinct(new PointEqualityComparer()).ToList();
		}
		return result;
	}

	public static List<Eps_Point> GetAllDistinctSurfacePoints(List<TopographySurfaceContainer> _SurfaceList)
	{
		return GetAllDistinctSurfacePoints(_SurfaceList, null);
	}

	public static List<Eps_Point> GetAllDistinctSurfacePoints(List<TopographySurfaceContainer> _SurfaceList, Phase _CurPhase)
	{
		List<Eps_Point> result = new List<Eps_Point>();
		if (_SurfaceList != null && _SurfaceList.Count > 0)
		{
			List<Eps_Point> list = new List<Eps_Point>();
			for (int i = 0; i < _SurfaceList.Count; i++)
			{
				if (_CurPhase == null || (_SurfaceList[i].TopographySurface != null && _SurfaceList[i].TopographySurface.CreatedPhaseId != null && _SurfaceList[i].TopographySurface.CreatedPhaseId == _CurPhase.Id) || (_SurfaceList[i].BuildingPad != null && _SurfaceList[i].BuildingPad.CreatedPhaseId != null && _SurfaceList[i].BuildingPad.CreatedPhaseId == _CurPhase.Id))
				{
					list.AddRange(_SurfaceList[i].GetAllVertices());
				}
			}
			IEnumerable<Eps_Point> source = list;
			result = source.Distinct(new PointEqualityComparer()).ToList();
		}
		return result;
	}

	public static List<TopographySurfaceContainer> GetSurfaceContainerListContainingPoint(Eps_Point _FindPoint, List<TopographySurfaceContainer> _SurfaceList)
	{
		return GetSurfaceContainerListContainingPoint(_FindPoint, _SurfaceList, null, null);
	}

	public static List<TopographySurfaceContainer> GetSurfaceContainerListContainingPoint(Eps_Point _FindPoint, List<TopographySurfaceContainer> _SurfaceList, Phase _PhaseCreated)
	{
		return GetSurfaceContainerListContainingPoint(_FindPoint, _SurfaceList, _PhaseCreated, null);
	}

	public static List<TopographySurfaceContainer> GetSurfaceContainerListContainingPoint(Eps_Point _FindPoint, List<TopographySurfaceContainer> _SurfaceList, Phase _PhaseCreated, Phase _PhaseDemolished)
	{
		TopographySurfaceContainer topographySurfaceContainer = null;
		List<TopographySurfaceContainer> list = null;
		if (_SurfaceList != null && _SurfaceList.Count > 0)
		{
			for (int i = 0; i < _SurfaceList.Count; i++)
			{
				topographySurfaceContainer = _SurfaceList[i].GetSurfaceContainerContainingPoint(_FindPoint, _PhaseCreated, _PhaseDemolished);
				if (topographySurfaceContainer != null)
				{
					if (list == null)
					{
						list = new List<TopographySurfaceContainer>();
					}
					list.Add(topographySurfaceContainer);
				}
			}
		}
		return list;
	}

	public static void GetIntersections(Document _Document, string _strPlacementId, ref List<FamilyInstance> _SiteFamilyInstanceList, ref List<IntersectionStationContainer> _LeftIntersectionStations, ref List<IntersectionStationContainer> _RightIntersectionStations)
	{
		GetIntersections(_Document, _strPlacementId, _bOnlyFindSecondaryFamilies: false, ref _SiteFamilyInstanceList, ref _LeftIntersectionStations, ref _RightIntersectionStations);
	}

	public static void GetIntersections(Document _Document, string _strPlacementId, bool _bOnlyFindSecondaryFamilies, ref List<FamilyInstance> _SiteFamilyInstanceList, ref List<IntersectionStationContainer> _LeftIntersectionStations, ref List<IntersectionStationContainer> _RightIntersectionStations)
	{
		IntersectionStationContainer intersectionStationContainer = null;
		IntersectionStation intersectionStation = null;
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		Parameter PrimaryIdParameter = null;
		Parameter SecondaryIdParameter = null;
		Parameter parameter = null;
		_LeftIntersectionStations = new List<IntersectionStationContainer>();
		_RightIntersectionStations = new List<IntersectionStationContainer>();
		_SiteFamilyInstanceList = new List<FamilyInstance>();
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.IntersectionPlacement).ToString("000"), ignoreCase: true) == 0 && (PrimaryIdParameter = FindFamilyInstance.LookupParameter(RevitParameters.PrimaryPlacementIDParameter)) != null && (SecondaryIdParameter = FindFamilyInstance.LookupParameter(RevitParameters.SecondaryPlacementIDParameter)) != null && ((string.Compare(PrimaryIdParameter.AsString(), _strPlacementId, ignoreCase: true) == 0 && !_bOnlyFindSecondaryFamilies) || string.Compare(SecondaryIdParameter.AsString(), _strPlacementId, ignoreCase: true) == 0));
		list = source.ToList();
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Symbol: not null, Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
			{
				continue;
			}
			SiteworksIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (SiteworksIDParameter == null || string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.IntersectionPlacement).ToString("000"), ignoreCase: true) != 0)
			{
				continue;
			}
			PrimaryIdParameter = familyInstance.LookupParameter(RevitParameters.PrimaryPlacementIDParameter);
			if (PrimaryIdParameter == null || (_bOnlyFindSecondaryFamilies && string.Compare(PrimaryIdParameter.AsString(), _strPlacementId, ignoreCase: true) == 0))
			{
				continue;
			}
			SecondaryIdParameter = familyInstance.LookupParameter(RevitParameters.SecondaryPlacementIDParameter);
			if (SecondaryIdParameter == null)
			{
				continue;
			}
			_SiteFamilyInstanceList.Add(familyInstance);
			intersectionStation = new IntersectionStation();
			parameter = familyInstance.LookupParameter(RevitParameters.IsStartingParameter);
			if (parameter != null)
			{
				intersectionStation.StartingIntersection = Convert.ToBoolean(parameter.AsInteger());
			}
			parameter = familyInstance.LookupParameter(RevitParameters.RadiusParameter);
			if (parameter != null)
			{
				intersectionStation.Radius = parameter.AsDouble();
			}
			parameter = familyInstance.LookupParameter(RevitParameters.SecondaryStationParameter);
			if (parameter != null)
			{
				intersectionStation.Station = parameter.AsDouble();
			}
			parameter = familyInstance.LookupParameter(RevitParameters.PrimaryStationParameter);
			if (parameter != null)
			{
				intersectionStation.PrimaryStation = parameter.AsDouble();
			}
			parameter = familyInstance.LookupParameter(RevitParameters.IsSecondaryLeftParameter);
			if (parameter != null)
			{
				intersectionStation.IsLeft = Convert.ToBoolean(parameter.AsInteger());
			}
			parameter = familyInstance.LookupParameter(RevitParameters.IsPrimaryLeftParameter);
			if (parameter != null)
			{
				intersectionStation.IsPrimaryLeft = Convert.ToBoolean(parameter.AsInteger());
			}
			parameter = familyInstance.LookupParameter(RevitParameters.TransitioningDistanceParameter);
			if (parameter != null)
			{
				intersectionStation.TransitioningDistance = parameter.AsDouble();
			}
			int num = 0;
			if (intersectionStation.IsLeft)
			{
				for (num = 0; num < _LeftIntersectionStations.Count; num++)
				{
					if (string.Compare(PrimaryIdParameter.AsString(), _LeftIntersectionStations[num].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(SecondaryIdParameter.AsString(), _LeftIntersectionStations[num].SecondaryPlacementId, ignoreCase: true) == 0)
					{
						_LeftIntersectionStations[num].IntersectionStations.Add(intersectionStation);
						break;
					}
				}
				if (num == _LeftIntersectionStations.Count)
				{
					intersectionStationContainer = new IntersectionStationContainer();
					intersectionStationContainer.PrimaryPlacementId = PrimaryIdParameter.AsString();
					intersectionStationContainer.SecondaryPlacementId = SecondaryIdParameter.AsString();
					intersectionStationContainer.IntersectionStations.Add(intersectionStation);
					_LeftIntersectionStations.Add(intersectionStationContainer);
				}
				continue;
			}
			for (num = 0; num < _RightIntersectionStations.Count; num++)
			{
				if (string.Compare(PrimaryIdParameter.AsString(), _RightIntersectionStations[num].PrimaryPlacementId, ignoreCase: true) == 0 && string.Compare(SecondaryIdParameter.AsString(), _RightIntersectionStations[num].SecondaryPlacementId, ignoreCase: true) == 0)
				{
					_RightIntersectionStations[num].IntersectionStations.Add(intersectionStation);
					break;
				}
			}
			if (num == _RightIntersectionStations.Count)
			{
				intersectionStationContainer = new IntersectionStationContainer();
				intersectionStationContainer.PrimaryPlacementId = PrimaryIdParameter.AsString();
				intersectionStationContainer.SecondaryPlacementId = SecondaryIdParameter.AsString();
				intersectionStationContainer.IntersectionStations.Add(intersectionStation);
				_RightIntersectionStations.Add(intersectionStationContainer);
			}
		}
		IntersectionStationContainerComparer comparer = new IntersectionStationContainerComparer(_bCompareFirstStations: true);
		if (_LeftIntersectionStations != null && _LeftIntersectionStations.Count > 0)
		{
			_LeftIntersectionStations.Sort(comparer);
		}
		if (_RightIntersectionStations != null && _RightIntersectionStations.Count > 0)
		{
			_RightIntersectionStations.Sort(comparer);
		}
	}

	public static void InvokeUpdaters(string strOldSurfaceId, string strNewSurfaceId, Document _Document)
	{
		string _strProgramPath = string.Empty;
		string empty = string.Empty;
		ApplicationUtility.RegistryPaths.GetProgramPath(string.Empty, ref _strProgramPath);
		if (string.IsNullOrEmpty(_strProgramPath))
		{
			return;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(_strProgramPath);
		FileInfo[] files = directoryInfo.GetFiles("*.dll");
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			fileInfo.Name.Contains("LANDCADDWrapper");
		}
		if (!(empty != string.Empty))
		{
			return;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly[] array2 = assemblies;
		foreach (Assembly assembly in array2)
		{
			if (empty.Contains(assembly.GetName().Name))
			{
				Type type = assembly.GetType("LANDCADD.ExternalApplication");
				MethodInfo method = type.GetMethod("UpdateGenericModelElementList");
				if (method != null)
				{
					method.Invoke(assembly.CreateInstance("LANDCADD.ExternalApplication"), new object[3] { strOldSurfaceId, strNewSurfaceId, _Document });
				}
			}
		}
	}

	private static bool RefreshAllSiteComponents(Document _Document, EPTransactionContainer _TransactionContainer, List<FamilyInstanceComponentWrapper> _AllSiteComponentList, TopographySurfaceContainer _BaseSurface, TopographySurfaceContainer _ProposedSurface, FamilyInstance _TopoPlacementFamilyInstance)
	{
		double _dReturnValue = 0.0;
		bool result = false;
		string empty = string.Empty;
		Parameter parameter = null;
		Parameter parameter2 = null;
		FamilyInstance familyInstance = null;
		ElementSet elementSet = null;
		Eps_Leg eps_Leg = null;
		List<IntersectionStationContainer> list = null;
		List<IntersectionStationContainer> list2 = null;
		List<IntersectionStationContainer> list3 = null;
		List<IntersectionContainer> list4 = null;
		if (_AllSiteComponentList != null)
		{
			if (!ApplicationUtility.Settings.eplSetE_GetSetting("TopographySurfaceArcPointInterval", ref _dReturnValue))
			{
				_dReturnValue = 1.0;
				ApplicationUtility.Settings.eplSetE_SetSetting("TopographySurfaceArcPointInterval", _dReturnValue);
			}
			SitePlacementSettings sitePlacementSettings = new SitePlacementSettings();
			FamilyInstanceComponentWrapperComparer comparer = new FamilyInstanceComponentWrapperComparer();
			if (_AllSiteComponentList.Count > 0)
			{
				_AllSiteComponentList.Sort(comparer);
			}
			List<Eps_Point> list5 = new List<Eps_Point>();
			List<Eps_Point> _CrownPointList = new List<Eps_Point>();
			SetupAllIntersectionStationsForAllSiteComponents(_Document, _ProposedSurface, _TransactionContainer, string.Empty, ref _AllSiteComponentList);
			list5 = _BaseSurface.GetAllVertices();
			for (int i = 0; i < _AllSiteComponentList.Count; i++)
			{
				elementSet = _AllSiteComponentList[i].GetFamilyInstanceElementSet();
				if (elementSet == null || elementSet.Size == 0)
				{
					continue;
				}
				familyInstance = _AllSiteComponentList[i].GetFirstExistingFamilyInstance();
				if (familyInstance != null && familyInstance.Symbol != null)
				{
					sitePlacementSettings.PlacementSymbol = familyInstance.Symbol;
					parameter = familyInstance.LookupParameter(RevitParameters.ProjectInnerSlopesFamilyParameter);
					if (parameter != null)
					{
						sitePlacementSettings.ProjectInnerSlopes = parameter.AsInteger();
					}
					parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
					if (parameter != null)
					{
						sitePlacementSettings.Name = parameter.AsString();
					}
					parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
					if (parameter != null)
					{
						foreach (int value5 in Enum.GetValues(typeof(LandcaddID)))
						{
							if (string.Compare(parameter.AsString(), value5.ToString("000"), ignoreCase: true) == 0)
							{
								sitePlacementSettings.LocateType = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value5);
								break;
							}
						}
					}
					parameter = familyInstance.LookupParameter(RevitParameters.ElevationFamilyParameter);
					if (parameter != null)
					{
						sitePlacementSettings.Elevation = parameter.AsDouble();
					}
					parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentIsRelativeElevationParameter);
					if (parameter != null)
					{
						sitePlacementSettings.IsRelativeElevation = false;
						int num = parameter.AsInteger();
						if (num == 1)
						{
							sitePlacementSettings.IsRelativeElevation = true;
						}
					}
					sitePlacementSettings.Type = RevitUtility.FindFamilyTypeSymbol(_Document, familyInstance, sitePlacementSettings.LocateType);
					parameter = familyInstance.LookupParameter(RevitParameters.UseRightSideGenericFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						if (parameter.AsInteger() == 1)
						{
							sitePlacementSettings.UseRightSide = true;
						}
						else
						{
							sitePlacementSettings.UseRightSide = false;
						}
					}
					parameter = familyInstance.LookupParameter(RevitParameters.LinearPointIntervalParameter);
					if (parameter != null)
					{
						sitePlacementSettings.LinearPointInterval = parameter.AsDouble();
					}
					foreach (Element item in elementSet)
					{
						parameter = item.LookupParameter(RevitParameters.PrimaryFormIDFamilyParameter);
						if (parameter != null)
						{
							sitePlacementSettings.LoftFormID = parameter.AsString();
						}
						parameter = item.LookupParameter(RevitParameters.AdditionalFormIDFamilyParameter);
						if (parameter != null)
						{
							sitePlacementSettings.AdditionalLoftFormID = parameter.AsString();
						}
						parameter = item.LookupParameter(RevitParameters.IslandSurfaceIdFamilyParameter);
						if (parameter != null)
						{
							sitePlacementSettings.IslandSurfaceID = parameter.AsString();
						}
						parameter = item.LookupParameter(RevitParameters.ParkingLotSurfaceIDFamilyParameter);
						if (parameter != null)
						{
							sitePlacementSettings.ParkingLotSurfaceID = parameter.AsString();
						}
						parameter = item.LookupParameter(RevitParameters.ParentSurfaceIDFamilyParameter);
						if (parameter != null)
						{
							sitePlacementSettings.ParentTopoSurfaceID = parameter.AsString();
						}
					}
					parameter = familyInstance.LookupParameter(RevitParameters.TopoDrawOrderFamilyParameter);
					if (parameter != null)
					{
						sitePlacementSettings.TopoDrawOrder = parameter.AsInteger();
					}
				}
				sitePlacementSettings.Level = _Document.ActiveView.GenLevel;
				sitePlacementSettings.TransactionContainer = _TransactionContainer;
				List<Element> elementList = new List<Element>();
				bool bDrawForm = false;
				if (sitePlacementSettings.LocateType == LandcaddID.Curb && sitePlacementSettings.ParentTopoSurfaceID != string.Empty && string.Compare(sitePlacementSettings.ParentTopoSurfaceID, _ProposedSurface.TopographySurface.UniqueId, ignoreCase: true) != 0)
				{
					sitePlacementSettings.LoftFormID = string.Empty;
					sitePlacementSettings.IslandSurfaceID = string.Empty;
					sitePlacementSettings.ParkingLotSurfaceID = string.Empty;
					continue;
				}
				if (_AllSiteComponentList[i].OriginalLeg == null && _AllSiteComponentList[i].ProposedLeg == null)
				{
					_AllSiteComponentList[i].CreateOriginalLeg(_ProposedSurface);
				}
				if (_AllSiteComponentList[i].OriginalLeg != null || _AllSiteComponentList[i].ProposedLeg != null)
				{
					if (_AllSiteComponentList[i].ProposedFamilySymbol == null)
					{
						_AllSiteComponentList[i].ProposedFamilySymbol = sitePlacementSettings.Type;
					}
					if (_AllSiteComponentList[i].ProposedLeg != null)
					{
						eps_Leg = Eps_Leg.CopyLeg(_AllSiteComponentList[i].ProposedLeg);
					}
					else if (_AllSiteComponentList[i].OriginalLeg != null)
					{
						eps_Leg = Eps_Leg.CopyLeg(_AllSiteComponentList[i].OriginalLeg);
					}
				}
				sitePlacementSettings.IntersectionPlacementSymbol = _AllSiteComponentList[i].IntersectionFamilySymbol;
				list = _AllSiteComponentList[i].LeftIntersectionStations;
				list2 = _AllSiteComponentList[i].CenterIntersectionStations;
				list3 = _AllSiteComponentList[i].RightIntersectionStations;
				list4 = _AllSiteComponentList[i].IntersectionContainerList;
				if (eps_Leg != null && eps_Leg.LegCount > 0)
				{
					if (!RebuildProposedTopographySurface(_Document, sitePlacementSettings, _dReturnValue, eps_Leg, _ProposedSurface, elementList, bDrawForm, _AllSiteComponentList[i].ParkingLotBreaklineList, _AllSiteComponentList[i].ParkingLotBreaklineElementList, _AllSiteComponentList[i].ParkingLotBreaklineLinearPointIntervals, list, list2, list3, list4, ref list5, ref _CrownPointList))
					{
						return false;
					}
					eps_Leg = null;
				}
				sitePlacementSettings.LoftFormID = string.Empty;
				sitePlacementSettings.IslandSurfaceID = string.Empty;
				sitePlacementSettings.ParkingLotSurfaceID = string.Empty;
			}
			if (_CrownPointList != null && _CrownPointList.Count > 0)
			{
				list5.AddRange(_CrownPointList);
			}
			ProgressBarForm progressBarForm = new ProgressBarForm();
			progressBarForm.ShowForm();
			progressBarForm.Refresh();
			progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarUpdatingModelMessage");
			progressBarForm.MinValue = 0;
			progressBarForm.MaxValue = 12;
			List<XYZ> PointXYZList = new List<XYZ>();
			for (int j = 0; j < list5.Count; j++)
			{
				PointXYZList.Add(list5[j].XYZ);
			}
			progressBarForm.UpdateProgressBar(1);
			if (PointXYZList.Count > 0)
			{
				RemoveDuplicateXY(ref PointXYZList);
				TopographySurface topographySurface = TerrainApi.Create(_Document, PointXYZList, _ProposedSurface.TopographySurface);
				_Document.Regenerate();
				progressBarForm.UpdateProgressBar(7);
				TerrainApi.CopyMaterial(_ProposedSurface.TopographySurface, topographySurface);
				parameter = ((Element)_ProposedSurface.TopographySurface).get_Parameter(BuiltInParameter.PHASE_CREATED);
				if (parameter != null && parameter.AsElementId() != null && parameter.AsElementId() != ElementId.InvalidElementId)
				{
					parameter2 = ((Element)topographySurface).get_Parameter(BuiltInParameter.PHASE_CREATED);
					if (parameter2 != null && !parameter2.IsReadOnly)
					{
						ElementId value3 = parameter.AsElementId();
						parameter2.Set(value3);
					}
				}
				parameter = ((Element)_ProposedSurface.TopographySurface).get_Parameter(BuiltInParameter.PHASE_DEMOLISHED);
				if (parameter != null && parameter.AsElementId() != null && parameter.AsElementId() != ElementId.InvalidElementId)
				{
					parameter2 = ((Element)topographySurface).get_Parameter(BuiltInParameter.PHASE_DEMOLISHED);
					if (parameter2 != null && !parameter2.IsReadOnly)
					{
						ElementId value4 = parameter.AsElementId();
						parameter2.Set(value4);
					}
				}
				parameter = TerrainApi.NameParameter(((Element)_ProposedSurface.TopographySurface));
				if (parameter != null && parameter.AsString() != null && parameter.AsString() != string.Empty)
				{
					parameter2 = TerrainApi.NameParameter(((Element)topographySurface));
					if (parameter2 != null && !parameter2.IsReadOnly)
					{
						parameter2.Set(parameter.AsString());
					}
				}
				InvokeUpdaters(_ProposedSurface.TopographySurface.UniqueId, topographySurface.UniqueId, _Document);
				_Document.Delete(_ProposedSurface.TopographySurface.Id);
				progressBarForm.UpdateProgressBar(3);
				_ProposedSurface = new TopographySurfaceContainer(_Document.Application, topographySurface, _bFindMinMaxPoints: true);
				if (_TopoPlacementFamilyInstance != null)
				{
					parameter = _TopoPlacementFamilyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(topographySurface.UniqueId);
					}
				}
			}
			progressBarForm.SetMaxValue();
			Thread.Sleep(300);
			progressBarForm.Close();
		}
		return result;
	}

	public static List<TopographySurfaceContainer> FindAllMainSurfaces(Document _Document, bool _bFindMinMaxPoints)
	{
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list = filteredElementCollector.OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).ToElements();
		List<TopographySurface> list2 = new List<TopographySurface>();
		List<TopographySurfaceContainer> list3 = new List<TopographySurfaceContainer>();
		foreach (Element item2 in list)
		{
			TopographySurface item = TerrainApi.AsSurface(item2);
			list2.Add(item);
		}
		for (int i = 0; i < list2.Count; i++)
		{
			if (TopographySurfaceContainer.FindTopographySurfaceType(_Document.Application, list2[i]) == TopographySurfaceType.MainSurface)
			{
				list3.Add(new TopographySurfaceContainer(_Document.Application, list2[i], _bFindMinMaxPoints));
			}
		}
		return list3;
	}

	public static Eps_Point FindToposurfaceAdjustmentPoint(TopographySurfaceContainer _CurTopoSurfaceContainer, List<TopographySurfaceContainer> _SurfaceList)
	{
		double dAdjustmentX = 2.0 * (_CurTopoSurfaceContainer.MaxRightPoint.x + (_CurTopoSurfaceContainer.MaxRightPoint.x - _CurTopoSurfaceContainer.MinLeftPoint.x));
		double dAdjustmentY = 2.0 * (_CurTopoSurfaceContainer.MaxRightPoint.y + (_CurTopoSurfaceContainer.MaxRightPoint.y - _CurTopoSurfaceContainer.MinLeftPoint.y));
		return FindToposurfaceAdjustmentPoint(_CurTopoSurfaceContainer, _SurfaceList, dAdjustmentX, dAdjustmentY);
	}

	public static Eps_Point FindToposurfaceAdjustmentPoint(TopographySurfaceContainer _CurTopoSurfaceContainer, List<TopographySurfaceContainer> _SurfaceList, double _dAdjustmentX, double _dAdjustmentY)
	{
		Eps_Point eps_Point = null;
		bool flag = true;
		int num = 0;
		int num2 = 5;
		int num3 = 1;
		while (flag)
		{
			flag = false;
			for (num = 0; num < _SurfaceList.Count; num++)
			{
				eps_Point = new Eps_Point(_CurTopoSurfaceContainer.MaxRightPoint.x + _dAdjustmentX, _CurTopoSurfaceContainer.MaxRightPoint.y + _dAdjustmentY, 0.0);
				if (_SurfaceList[num].IsPointWithinSurfaceBounds(eps_Point))
				{
					flag = true;
					break;
				}
				eps_Point = new Eps_Point(_CurTopoSurfaceContainer.MaxLeftPoint.x + _dAdjustmentX, _CurTopoSurfaceContainer.MaxLeftPoint.y + _dAdjustmentY, 0.0);
				if (_SurfaceList[num].IsPointWithinSurfaceBounds(eps_Point))
				{
					flag = true;
					break;
				}
				eps_Point = new Eps_Point(_CurTopoSurfaceContainer.MinLeftPoint.x + _dAdjustmentX, _CurTopoSurfaceContainer.MinLeftPoint.y + _dAdjustmentY, 0.0);
				if (_SurfaceList[num].IsPointWithinSurfaceBounds(eps_Point))
				{
					flag = true;
					break;
				}
				eps_Point = new Eps_Point(_CurTopoSurfaceContainer.MinRightPoint.x + _dAdjustmentX, _CurTopoSurfaceContainer.MinRightPoint.y + _dAdjustmentY, 0.0);
				if (_SurfaceList[num].IsPointWithinSurfaceBounds(eps_Point))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			if (num3 == num2)
			{
				flag = false;
			}
			if (_dAdjustmentX > 0.0)
			{
				if (_dAdjustmentY < 0.0)
				{
					_dAdjustmentX *= 2.0;
					_dAdjustmentY *= -2.0;
					num3++;
				}
				else
				{
					_dAdjustmentX = 0.0 - _dAdjustmentX;
				}
			}
			else if (_dAdjustmentY > 0.0)
			{
				_dAdjustmentY = 0.0 - _dAdjustmentY;
			}
			else if (_dAdjustmentX < 0.0)
			{
				_dAdjustmentX = 0.0 - _dAdjustmentX;
			}
		}
		return new Eps_Point(_dAdjustmentX, _dAdjustmentY, 0.0);
	}

	public static List<KeyValuePair<FamilyInstance, TopographySurfaceContainer>> FindPlantMixGenericModelList(Document _Document, List<TopographySurfaceContainer> _SubregionList)
	{
		List<Element> list = new List<Element>();
		FamilyInstance FindFamilyInstance = null;
		Parameter LandcaddIDParameter = null;
		Parameter parameter = null;
		if (_SubregionList.Count == 0)
		{
			return null;
		}
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_GenericModel);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (LandcaddIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter)) != null && (string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.PlantMix).ToString("000"), ignoreCase: true) == 0 || string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.PatioWalk).ToString("000"), ignoreCase: true) == 0) && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		List<KeyValuePair<FamilyInstance, TopographySurfaceContainer>> list2 = new List<KeyValuePair<FamilyInstance, TopographySurfaceContainer>>();
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_GenericModel.GetHashCode())
			{
				continue;
			}
			LandcaddIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter);
			if (LandcaddIDParameter == null || (string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.PlantMix).ToString("000"), ignoreCase: true) != 0 && string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.PatioWalk).ToString("000"), ignoreCase: true) != 0))
			{
				continue;
			}
			parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
			if (parameter == null)
			{
				continue;
			}
			for (int num = _SubregionList.Count - 1; num >= 0; num--)
			{
				if (string.Compare(parameter.AsString(), _SubregionList[num].TopographySurface.UniqueId, ignoreCase: true) == 0)
				{
					list2.Add(new KeyValuePair<FamilyInstance, TopographySurfaceContainer>(familyInstance, _SubregionList[num]));
					_SubregionList.RemoveAt(num);
					break;
				}
			}
			if (_SubregionList.Count == 0)
			{
				break;
			}
		}
		return list2;
	}

	public static List<TopographySurfaceContainer> FindNewTopographySurfaceContainers(Document _Document, List<ElementId> _NewElementIds)
	{
		CeilingAndFloor ceilingAndFloor = null;
		TopographySurface topographySurface = null;
		List<TopographySurfaceContainer> list = new List<TopographySurfaceContainer>();
		Element element = null;
		foreach (ElementId _NewElementId in _NewElementIds)
		{
			element = _Document.GetElement(_NewElementId);
			if (element == null)
			{
				continue;
			}
			if (TerrainApi.AsSurface(element) is {} topographySurface2)
			{
				try
				{
					string uniqueId = topographySurface2.UniqueId;
					list.Add(new TopographySurfaceContainer(_Document.Application, topographySurface2, _bFindMinMaxPoints: true));
				}
				catch (InvalidObjectException)
				{
				}
			}
			else if (element is CeilingAndFloor ceilingAndFloor2)
			{
				try
				{
					string uniqueId2 = ceilingAndFloor2.UniqueId;
					list.Add(new TopographySurfaceContainer(_Document.Application, ceilingAndFloor2, _bFindMinMaxPoints: true));
				}
				catch (InvalidObjectException)
				{
				}
			}
		}
		return list;
	}

	public static void UpdatePlantMixGenericModelList(List<KeyValuePair<FamilyInstance, TopographySurfaceContainer>> _UpdateGenericModelList, List<TopographySurfaceContainer> NewToposurfaceList)
	{
		if (_UpdateGenericModelList == null || _UpdateGenericModelList.Count <= 0)
		{
			return;
		}
		Parameter parameter = null;
		Eps_Point eps_Point = null;
		bool flag = false;
		for (int i = 0; i < _UpdateGenericModelList.Count; i++)
		{
			if (_UpdateGenericModelList[i].Value == null)
			{
				continue;
			}
			for (int j = 0; j < _UpdateGenericModelList[i].Value.Vertices.Count; j++)
			{
				eps_Point = _UpdateGenericModelList[i].Value.Vertices[j];
				for (int num = NewToposurfaceList.Count - 1; num >= 0; num--)
				{
					if (NewToposurfaceList[num].TopographySurface != null && NewToposurfaceList[num].GetSurfaceContainerContainingPoint(eps_Point) != null)
					{
						if (_UpdateGenericModelList[i].Key != null)
						{
							parameter = _UpdateGenericModelList[i].Key.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
							if (parameter != null && !parameter.IsReadOnly)
							{
								parameter.Set(NewToposurfaceList[num].TopographySurface.UniqueId);
							}
						}
						flag = true;
						NewToposurfaceList.RemoveAt(num);
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

	internal static bool ReplacesInteriorPatch(LandcaddID locateType)
	{
		return locateType == LandcaddID.SoftTerrain || locateType == LandcaddID.PointWipeout;
	}

	private static bool UsesFormPadElevation(LandcaddID locateType)
	{
		return locateType == LandcaddID.SoftTerrain || locateType == LandcaddID.FeatureLine || locateType == LandcaddID.Sidewalk;
	}

	private static void ApplyConstantElevationToHostGeometry(Eps_Leg leg, Eps_Line[] lines, int lineCount, Eps_Arc[] arcs, int arcCount, double elevation)
	{
		for (Eps_Leg current = leg; current != null; current = current.Next)
		{
			if (current.Line != null)
			{
				if (current.Line.StartPnt != null) current.Line.StartPnt.z = elevation;
				if (current.Line.EndPnt != null) current.Line.EndPnt.z = elevation;
			}
			if (current.Arc != null)
			{
				if (current.Arc.StartPnt != null) current.Arc.StartPnt.z = elevation;
				if (current.Arc.EndPnt != null) current.Arc.EndPnt.z = elevation;
				if (current.Arc.RadiusPnt != null) current.Arc.RadiusPnt.z = elevation;
			}
		}
		for (int i = 0; i < lineCount && lines != null; i++)
		{
			if (lines[i]?.StartPnt != null) lines[i].StartPnt.z = elevation;
			if (lines[i]?.EndPnt != null) lines[i].EndPnt.z = elevation;
		}
		for (int i = 0; i < arcCount && arcs != null; i++)
		{
			if (arcs[i]?.StartPnt != null) arcs[i].StartPnt.z = elevation;
			if (arcs[i]?.EndPnt != null) arcs[i].EndPnt.z = elevation;
			if (arcs[i]?.RadiusPnt != null) arcs[i].RadiusPnt.z = elevation;
		}
		RuntimeDiagnostics.Write("Forced constant host elevation Z=" + elevation + " ft.");
	}

	private static void DrapeLegOntoToposolid(TopographySurfaceContainer terrain, Eps_Leg leg, double extraZ)
	{
		if (terrain?.TopographySurface == null || leg == null) return;
		int draped = 0;
		for (Eps_Leg current = leg; current != null; current = current.Next)
		{
			if (DrapePointOntoToposolid(terrain.TopographySurface, current.Line?.StartPnt, extraZ)) draped++;
			if (DrapePointOntoToposolid(terrain.TopographySurface, current.Line?.EndPnt, extraZ)) draped++;
			if (DrapePointOntoToposolid(terrain.TopographySurface, current.Arc?.StartPnt, extraZ)) draped++;
			if (DrapePointOntoToposolid(terrain.TopographySurface, current.Arc?.EndPnt, extraZ)) draped++;
		}
		RuntimeDiagnostics.Write("Draped " + draped + " host-line point(s) onto the Toposolid top face.");
	}

	private static bool DrapePointOntoToposolid(TopographySurface surface, Eps_Point point, double extraZ)
	{
		if (point == null) return false;
		if (!TerrainApi.TryGetTopZ(surface, point.XYZ, out double z)) return false;
		point.z = z + extraZ;
		return true;
	}

	internal static void ApplyInteriorPointEdits(TopographySurface surface, IList<XYZ> desired, bool replaceUnmatched)
	{
		if (surface == null || desired == null) return;
		List<XYZ> remaining = (surface.GetInteriorPoints() ?? new List<XYZ>()).ToList();
		List<XYZ> toAdd = new List<XYZ>();
		foreach (XYZ point in desired)
		{
			if (point == null) continue;
			int index = remaining.FindIndex(existing => Math.Abs(existing.X - point.X) < 1E-06 && Math.Abs(existing.Y - point.Y) < 1E-06);
			if (index >= 0)
			{
				XYZ existing = remaining[index];
				remaining.RemoveAt(index);
				if (Math.Abs(existing.Z - point.Z) > 1E-04)
				{
					surface.MovePoint(existing, new XYZ(existing.X, existing.Y, point.Z));
				}
			}
			else
			{
				toAdd.Add(point);
			}
		}
		if (replaceUnmatched && remaining.Count > 0)
		{
			surface.DeletePoints(remaining);
		}
		if (toAdd.Count > 0)
		{
			surface.AddPoints(toAdd);
		}
	}

	internal static void AddToposolidBreaklines(TopographySurface surface, params Eps_Leg[] legs)
	{
		if (!TerrainApi.UsesToposolids || surface == null || legs == null) return;
		List<XYZ[]> creases = new List<XYZ[]>();
		foreach (Eps_Leg leg in legs)
		{
			if (leg == null) continue;
			List<Eps_Point> points = leg.GetPointsAtIntervals(4.0, 4.0);
			for (int i = 1; i < points.Count; i++)
			{
				if (points[i - 1] == null || points[i] == null) continue;
				creases.Add(new XYZ[] { points[i - 1].XYZ, points[i].XYZ });
			}
		}
		if (creases.Count == 0) return;
		try
		{
			ToposolidSurface.RestoreBreaklines(surface, creases, required: false);
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("Street breaklines were skipped: " + ex.Message);
		}
	}

	public static void RemoveDuplicateXY(ref List<XYZ> PointXYZList)
	{
		List<XYZ> list = new List<XYZ>();
		if (TopographySurface.ArePointsDistinct(PointXYZList))
		{
			return;
		}
		for (int i = 0; i < PointXYZList.Count - 1; i++)
		{
			for (int j = i + 1; j < PointXYZList.Count; j++)
			{
				if (Math.Abs(PointXYZList[i].X - PointXYZList[j].X) < 1E-06 && Math.Abs(PointXYZList[i].Y - PointXYZList[j].Y) < 1E-06)
				{
					list.Add(PointXYZList[i]);
					list.Add(PointXYZList[j]);
					if (Math.Abs(PointXYZList[i].Z - PointXYZList[j].Z) < 1E-06)
					{
						PointXYZList.RemoveAt(j);
						j--;
					}
					else
					{
						PointXYZList.RemoveAt(j);
						j--;
					}
				}
			}
		}
	}
}

