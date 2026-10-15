using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using SITES.Utilities;

namespace EPCommon;

public class TypeManagerForm : EPForm
{
	private Units m_Units;

	private string m_strSourceFile = string.Empty;

	private Document m_CurDocument;

	private List<FamilyParameterTypeWrapper> m_ParameterList;

	private FormatOptions m_FormatOptions;

	private FormatOptions m_SlopeOptions;

	private FormatOptions m_AngularOptions;

	private DisplayUnit m_DisplayUnitSystem;

	private UnitSymbolType m_UnitSymbolType;

	private UnitSymbolType m_SlopeSymbolType;

	private UnitSymbolType m_AngularSymbolType;

	private int m_nMixtListViewSortColumn = -1;

	private EPTransactionContainer m_TransactionContainer;

	private LandcaddID m_CurrentFamilyType = LandcaddID.Street;

	private FamilyBinder m_SelectedFamily;

	private List<Material> m_AllMaterialList;

	private List<Material> m_MaterialList;

	private DecimalSymbol m_DecimalSymbolType;

	private double m_dAngularRounding;

	private DisplayUnitType m_SlopeDisplayUnitType;

	private double m_SlopeAccuracy;

	private IContainer components;

	private GroupBox m_DynGroupBox;

	private ListView m_TypeListView;

	private Button m_DeleteButton;

	private Button m_AddButton;

	private Button m_SearchButton;

	private Button m_ModifyButton;

	private Button m_SaveFamilyButton;

	private Button m_CancelButton;

	private Button m_EqualButton;

	public TypeManagerForm(ref FamilyBinder SelectedFamily, Document _CurDocument, LandcaddID _CurrentFamilyType, Document _ActiveDocument)
	{
		InitializeComponent();
		m_SearchButton.Visible = false;
		m_EqualButton.Visible = false;
		m_CurDocument = _CurDocument;
		m_TransactionContainer = new EPTransactionContainer(m_CurDocument);
		m_FormatOptions = _ActiveDocument.GetUnits().GetFormatOptions(UnitType.UT_Length);
		m_SlopeOptions = _ActiveDocument.GetUnits().GetFormatOptions(UnitType.UT_Slope);
		m_SlopeDisplayUnitType = m_SlopeOptions.LegacyDisplayUnits();
		m_SlopeAccuracy = RevitGeometry.GetSlopeAccuracy(m_SlopeDisplayUnitType);
		m_AngularOptions = _ActiveDocument.GetUnits().GetFormatOptions(UnitType.UT_Angle);
		m_DisplayUnitSystem = _ActiveDocument.DisplayUnitSystem;
		m_Units = _ActiveDocument.GetUnits();
		try
		{
			m_UnitSymbolType = m_FormatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_UnitSymbolType = UnitSymbolType.UST_NONE;
		}
		try
		{
			m_SlopeSymbolType = m_SlopeOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_SlopeSymbolType = UnitSymbolType.UST_NONE;
		}
		try
		{
			m_AngularSymbolType = m_AngularOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_AngularSymbolType = UnitSymbolType.UST_NONE;
		}
		try
		{
			m_dAngularRounding = m_AngularOptions.Accuracy;
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_dAngularRounding = 1.0;
		}
		m_DecimalSymbolType = _ActiveDocument.GetUnits().DecimalSymbol;
		m_SelectedFamily = SelectedFamily;
		m_CurrentFamilyType = _CurrentFamilyType;
		m_MaterialList = new List<Material>();
		m_AllMaterialList = new List<Material>();
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_ActiveDocument);
		IList<Element> list = filteredElementCollector.OfClass(typeof(Material)).OfCategory(BuiltInCategory.OST_Materials).ToElements();
		foreach (Element item2 in list)
		{
			if (item2 != null && item2 is Material item)
			{
				m_AllMaterialList.Add(item);
				m_MaterialList.Add(item);
			}
		}
		SetStringValues();
		SetDefaultValues();
	}

	private void SetStringValues()
	{
		string empty = string.Empty;
		string text = string.Empty;
		switch (m_CurrentFamilyType)
		{
		case LandcaddID.PlantMix:
			m_ParameterList = ManagePlants.PlantMixParamenterList();
			text = EPShared.LocalRM.GetString("");
			break;
		case LandcaddID.PatioWalk:
			m_ParameterList = ManagePlants.PatioWalkParamenterList();
			text = EPShared.LocalRM.GetString("");
			break;
		case LandcaddID.Street:
			m_ParameterList = ManageFamily.StreetParamenterList();
			text = EPShared.LocalRM.GetString("StreetTypeManagerString");
			ContextualHelpID = "HID_SDfamilyModifyStreet";
			break;
		case LandcaddID.Curb:
			m_ParameterList = ManageFamily.CurbParameterList();
			text = EPShared.LocalRM.GetString("CurbTypeManagerString");
			ContextualHelpID = "HID_SDfamilyModifyCurb";
			break;
		case LandcaddID.Sidewalk:
			m_ParameterList = ManageFamily.SideWalkParamterList();
			text = EPShared.LocalRM.GetString("SidewalkTypeManagerString");
			ContextualHelpID = "HID_SDfamilyModifySidewalk";
			break;
		case LandcaddID.RetainingWall:
			m_ParameterList = ManageFamily.RetainingWallParameterList();
			text = EPShared.LocalRM.GetString("RetainingWallManagerString");
			ContextualHelpID = "HID_SDfamilyModifyRetainingWall";
			break;
		case LandcaddID.Steps:
			m_ParameterList = ManageFamily.StepsParameterList();
			text = EPShared.LocalRM.GetString("StepsManagerString");
			break;
		case LandcaddID.ParkingLot:
			m_ParameterList = ManageFamily.ParkingLotParameterList();
			text = EPShared.LocalRM.GetString("ParkingLotManagerString");
			ContextualHelpID = "HID_SDfamilyModifyParkinglot";
			break;
		case LandcaddID.SoftTerrain:
			m_ParameterList = ManageFamily.SoftTerrainParameterList();
			text = EPShared.LocalRM.GetString("SoftTerrainManagerString");
			ContextualHelpID = "HID_SDfamilyModifySoftTerrain";
			break;
		}
		empty = text + " " + EPShared.LocalRM.GetString("FamilyTypeString");
		Text = text + " " + EPShared.LocalRM.GetString("TypeManagerDialog");
		m_SearchButton.Visible = false;
		m_EqualButton.Visible = false;
		m_TypeListView.View = System.Windows.Forms.View.Details;
		for (int i = 0; i < m_ParameterList.Count; i++)
		{
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) != 0 && string.Compare(m_ParameterList[i].ParameterName, RevitParameters.PlacementIDFamilyParameter, ignoreCase: true) != 0 && string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SideWalkLocationLineParameter, ignoreCase: true) != 0)
			{
				if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
				{
					m_TypeListView.Columns.Add(EPShared.LocalRM.GetString("Slope_") + m_ParameterList[i].ParameterDisplayName);
				}
				else
				{
					m_TypeListView.Columns.Add(m_ParameterList[i].ParameterDisplayName);
				}
			}
		}
		string empty2 = string.Empty;
		if (m_SelectedFamily.FamilyTypeList == null)
		{
			m_SelectedFamily.FamilyTypeList = new List<FamilyTypeContainer>();
		}
		for (int j = 0; j < m_SelectedFamily.FamilyTypeList.Count; j++)
		{
			FamilyTypeContainer familyTypeContainer = m_SelectedFamily.FamilyTypeList[j];
			if (familyTypeContainer == null)
			{
				continue;
			}
			ListViewItem listViewItem = new ListViewItem();
			for (int k = 0; k < m_ParameterList.Count; k++)
			{
				if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.PlacementIDFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				switch (m_CurrentFamilyType)
				{
				case LandcaddID.PlantMix:
					empty2 = string.Empty;
					FindParamenterValue(m_CurDocument.FamilyManager, familyTypeContainer.FamilyTypeWrapper.FamilyType, m_ParameterList[k].ParameterName, ref empty2);
					if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.PlantSpacingPlantMixFamilyParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble3 = 1.0;
						if (empty2 != string.Empty)
						{
							dConvertedDouble3 = ValueConversion.ToDouble(empty2);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.PercentagePlantMixFamilyParameter, ignoreCase: true) == 0)
					{
						if (empty2 == null || empty2 == string.Empty)
						{
							empty2 = "1.0";
						}
					}
					else if (empty2 == string.Empty && string.Compare(m_ParameterList[k].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
					{
						empty2 = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					if (k == 0)
					{
						listViewItem = m_TypeListView.Items.Add(familyTypeContainer.FamilyTypeWrapper.FamilyTypeName, empty2, string.Empty);
						listViewItem.Tag = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else
					{
						listViewItem.SubItems.Add(empty2);
					}
					break;
				case LandcaddID.PatioWalk:
					empty2 = string.Empty;
					FindParamenterValue(m_CurDocument.FamilyManager, familyTypeContainer.FamilyTypeWrapper.FamilyType, m_ParameterList[k].ParameterName, ref empty2);
					if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.ThicknessPathWalkFamilyParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.EdgeWidthPathWalkFamilyParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble10 = 0.0;
						if (empty2 != string.Empty)
						{
							dConvertedDouble10 = ValueConversion.ToDouble(empty2);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble10, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.EdgePathWalkFamilyParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.MowingStripPathWalkFamilyParameter, ignoreCase: true) == 0)
					{
						if (string.Compare(empty2, "1", ignoreCase: true) != 0)
						{
							empty2 = ((string.Compare(empty2, "0", ignoreCase: true) != 0) ? EPShared.LocalRM.GetString("NoString") : EPShared.LocalRM.GetString("NoString"));
						}
						else
						{
							empty2 = EPShared.LocalRM.GetString("YesString");
						}
					}
					else if (empty2 == string.Empty && string.Compare(m_ParameterList[k].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
					{
						empty2 = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					if (k == 0)
					{
						listViewItem = m_TypeListView.Items.Add(familyTypeContainer.FamilyTypeWrapper.FamilyTypeName, empty2, string.Empty);
						listViewItem.Tag = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else
					{
						listViewItem.SubItems.Add(empty2);
					}
					break;
				case LandcaddID.SoftTerrain:
					empty2 = string.Empty;
					FindParameterValue(m_ParameterList[k].ParameterName, familyTypeContainer.FamilyParameterList, ref empty2);
					if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SoftTerrainLeftOffsetWidthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SoftTerrainRightOffsetWidthParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble7 = 0.0;
						if (empty2 != string.Empty)
						{
							dConvertedDouble7 = ValueConversion.ToDouble(empty2);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble7, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
					}
					else if (empty2 == string.Empty && string.Compare(m_ParameterList[k].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
					{
						empty2 = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SoftTerrainCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SoftTerrainFillSlopeParameter, ignoreCase: true) == 0)
					{
						if (empty2 != string.Empty)
						{
							RevitGeometry.Units_ConvertDoubleToString(ValueConversion.ToDouble(empty2), m_SlopeDisplayUnitType, m_SlopeAccuracy, m_SlopeSymbolType, m_Units, ref empty2);
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dConvertedDouble8 = 0.0;
							if (empty2 != string.Empty)
							{
								dConvertedDouble8 = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble8, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dAngle6 = 0.0;
							if (!string.IsNullOrEmpty(empty2))
							{
								dAngle6 = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle6, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					if (k == 0)
					{
						listViewItem = m_TypeListView.Items.Add(familyTypeContainer.FamilyTypeWrapper.FamilyTypeName, empty2, string.Empty);
						listViewItem.Tag = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else
					{
						listViewItem.SubItems.Add(empty2);
					}
					break;
				case LandcaddID.Street:
					empty2 = string.Empty;
					FindParameterValue(m_ParameterList[k].ParameterName, familyTypeContainer.FamilyParameterList, ref empty2);
					if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetGutterWidthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetCurbOffsetParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetLaneWidthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetPavementDepthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetCurbWidthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetCurbHeightParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetBoulevardWidthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetSubbaseSurfaceParameter, ignoreCase: true) == 0)
					{
						double num3 = 0.0;
						if (empty2 != string.Empty)
						{
							num3 = ValueConversion.ToDouble(empty2);
							RevitGeometry.Units_ConvertDoubleToString(num3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetCurbDisplayParameter, ignoreCase: true) == 0)
					{
						if (string.Compare(empty2, "1", ignoreCase: true) != 0)
						{
							empty2 = ((string.Compare(empty2, "0", ignoreCase: true) != 0) ? EPShared.LocalRM.GetString("NoString") : EPShared.LocalRM.GetString("NoString"));
						}
						else
						{
							empty2 = EPShared.LocalRM.GetString("YesString");
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetCrossSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetGutterSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetFillSlopeParameter, ignoreCase: true) == 0)
					{
						if (empty2 != string.Empty)
						{
							RevitGeometry.Units_ConvertDoubleToString(ValueConversion.ToDouble(empty2), m_SlopeDisplayUnitType, m_SlopeAccuracy, m_SlopeSymbolType, m_Units, ref empty2);
						}
					}
					else if (empty2 == string.Empty && string.Compare(m_ParameterList[k].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
					{
						empty2 = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetPavementSurfaceParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetCurbMaterialParameter, ignoreCase: true) == 0)
					{
						if (empty2 != string.Empty)
						{
							string strStringToFind3 = empty2;
							IEnumerable<Material> source3 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind3));
							List<Material> list3 = source3.ToList();
							empty2 = ((list3.Count < 1) ? "" : list3.ElementAt(0).Name);
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dConvertedDouble4 = 0.0;
							if (empty2 != string.Empty)
							{
								dConvertedDouble4 = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble4, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dAngle3 = 0.0;
							if (!string.IsNullOrEmpty(empty2))
							{
								dAngle3 = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle3, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					if (k == 0)
					{
						listViewItem = m_TypeListView.Items.Add(familyTypeContainer.FamilyTypeWrapper.FamilyTypeName, empty2, string.Empty);
						listViewItem.Tag = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else
					{
						listViewItem.SubItems.Add(empty2);
					}
					break;
				case LandcaddID.Curb:
					empty2 = string.Empty;
					FindParameterValue(m_ParameterList[k].ParameterName, familyTypeContainer.FamilyParameterList, ref empty2);
					if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetCurbHeightParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetCurbOffsetParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetCurbWidthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.CurbGutterHeightParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetGutterWidthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetPavementDepthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetLaneWidthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.CurbBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.CurbSubbaseSurfaceParameter, ignoreCase: true) == 0)
					{
						double num2 = 0.0;
						if (empty2 != string.Empty)
						{
							num2 = ValueConversion.ToDouble(empty2);
							RevitGeometry.Units_ConvertDoubleToString(num2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetGutterSlopeParameter, ignoreCase: true) == 0)
					{
						if (empty2 != string.Empty)
						{
							RevitGeometry.Units_ConvertDoubleToString(ValueConversion.ToDouble(empty2), m_SlopeDisplayUnitType, m_SlopeAccuracy, m_SlopeSymbolType, m_Units, ref empty2);
						}
					}
					else if (empty2 == string.Empty && string.Compare(m_ParameterList[k].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
					{
						empty2 = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StreetCurbMaterialParameter, ignoreCase: true) == 0)
					{
						if (empty2 != string.Empty)
						{
							string strStringToFind2 = empty2;
							IEnumerable<Material> source2 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind2));
							List<Material> list2 = source2.ToList();
							empty2 = ((list2.Count < 1) ? "" : list2.ElementAt(0).Name);
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dConvertedDouble2 = 0.0;
							if (empty2 != string.Empty)
							{
								dConvertedDouble2 = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dAngle2 = 0.0;
							if (!string.IsNullOrEmpty(empty2))
							{
								dAngle2 = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle2, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					if (k == 0)
					{
						listViewItem = m_TypeListView.Items.Add(familyTypeContainer.FamilyTypeWrapper.FamilyTypeName, empty2, string.Empty);
						listViewItem.Tag = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else
					{
						listViewItem.SubItems.Add(empty2);
					}
					break;
				case LandcaddID.Sidewalk:
					empty2 = string.Empty;
					FindParameterValue(m_ParameterList[k].ParameterName, familyTypeContainer.FamilyParameterList, ref empty2);
					if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SideWalkWidthParameter, ignoreCase: true) == 0)
					{
						double num5 = 0.0;
						if (empty2 != string.Empty)
						{
							num5 = ValueConversion.ToDouble(empty2);
							RevitGeometry.Units_ConvertDoubleToString(num5, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SideWalkPavementDepthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SideWalkBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SideWalkSubbaseSurfaceParameter, ignoreCase: true) == 0)
					{
						double num6 = 0.0;
						if (empty2 != string.Empty)
						{
							num6 = ValueConversion.ToDouble(empty2);
							RevitGeometry.Units_ConvertDoubleToString(num6, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SideWalkCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SideWalkFillSlopeParameter, ignoreCase: true) == 0)
					{
						if (empty2 != string.Empty)
						{
							RevitGeometry.Units_ConvertDoubleToString(ValueConversion.ToDouble(empty2), m_SlopeDisplayUnitType, m_SlopeAccuracy, m_SlopeSymbolType, m_Units, ref empty2);
						}
					}
					else
					{
						if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SideWalkLocationLineParameter, ignoreCase: true) == 0)
						{
							break;
						}
						if (empty2 == string.Empty && string.Compare(m_ParameterList[k].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
						{
							empty2 = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
						}
						else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.SideWalkPavementSurfaceParameter, ignoreCase: true) == 0)
						{
							if (empty2 != string.Empty)
							{
								string strStringToFind5 = empty2;
								IEnumerable<Material> source5 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind5));
								List<Material> list5 = source5.ToList();
								empty2 = ((list5.Count < 1) ? "" : list5.ElementAt(0).Name);
							}
						}
						else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
						{
							try
							{
								double dConvertedDouble6 = 0.0;
								if (empty2 != string.Empty)
								{
									dConvertedDouble6 = ValueConversion.ToDouble(empty2);
								}
								RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble6, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
							}
							catch (Exception)
							{
								empty2 = "";
							}
						}
						else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
						{
							try
							{
								double dAngle5 = 0.0;
								if (!string.IsNullOrEmpty(empty2))
								{
									dAngle5 = ValueConversion.ToDouble(empty2);
								}
								RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle5, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref empty2);
							}
							catch (Exception)
							{
								empty2 = "";
							}
						}
					}
					if (k == 0)
					{
						listViewItem = m_TypeListView.Items.Add(familyTypeContainer.FamilyTypeWrapper.FamilyTypeName, empty2, string.Empty);
						listViewItem.Tag = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else
					{
						listViewItem.SubItems.Add(empty2);
					}
					break;
				case LandcaddID.RetainingWall:
					empty2 = string.Empty;
					FindParameterValue(m_ParameterList[k].ParameterName, familyTypeContainer.FamilyParameterList, ref empty2);
					if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.RetainingWallWidthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.RetainingWallHeightParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.RetainingWallBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.RetainingWallSubbaseSurfaceParameter, ignoreCase: true) == 0)
					{
						double num4 = 0.0;
						if (empty2 != string.Empty)
						{
							num4 = ValueConversion.ToDouble(empty2);
							RevitGeometry.Units_ConvertDoubleToString(num4, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.RetainingWallSlopeProjectionParameter, ignoreCase: true) == 0)
					{
						if (string.Compare(empty2, "1", ignoreCase: true) != 0)
						{
							empty2 = ((string.Compare(empty2, "0", ignoreCase: true) != 0) ? EPShared.LocalRM.GetString("NoString") : EPShared.LocalRM.GetString("NoString"));
						}
						else
						{
							empty2 = EPShared.LocalRM.GetString("YesString");
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.RetainingWallCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.RetainingWallFillSlopeParameter, ignoreCase: true) == 0)
					{
						if (empty2 != string.Empty)
						{
							RevitGeometry.Units_ConvertDoubleToString(ValueConversion.ToDouble(empty2), m_SlopeDisplayUnitType, m_SlopeAccuracy, m_SlopeSymbolType, m_Units, ref empty2);
						}
					}
					else if (empty2 == string.Empty && string.Compare(m_ParameterList[k].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
					{
						empty2 = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.RetainingWallMaterialParameter, ignoreCase: true) == 0)
					{
						if (empty2 != string.Empty)
						{
							string strStringToFind4 = empty2;
							IEnumerable<Material> source4 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind4));
							List<Material> list4 = source4.ToList();
							empty2 = ((list4.Count < 1) ? "" : list4.ElementAt(0).Name);
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dConvertedDouble5 = 0.0;
							if (empty2 != string.Empty)
							{
								dConvertedDouble5 = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble5, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dAngle4 = 0.0;
							if (!string.IsNullOrEmpty(empty2))
							{
								dAngle4 = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle4, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					if (k == 0)
					{
						listViewItem = m_TypeListView.Items.Add(familyTypeContainer.FamilyTypeWrapper.FamilyTypeName, empty2, string.Empty);
						listViewItem.Tag = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else
					{
						listViewItem.SubItems.Add(empty2);
					}
					break;
				case LandcaddID.Steps:
					empty2 = string.Empty;
					FindParameterValue(m_ParameterList[k].ParameterName, familyTypeContainer.FamilyParameterList, ref empty2);
					if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StepsHeightParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StepsTreadParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StepsWidthParameter, ignoreCase: true) == 0)
					{
						double num7 = 0.0;
						if (empty2 != string.Empty)
						{
							num7 = ValueConversion.ToDouble(empty2);
							RevitGeometry.Units_ConvertDoubleToString(num7, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
					}
					else if (empty2 == string.Empty && string.Compare(m_ParameterList[k].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
					{
						empty2 = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.StepsMaterialParameter, ignoreCase: true) == 0)
					{
						if (empty2 != string.Empty)
						{
							string strStringToFind6 = empty2;
							IEnumerable<Material> source6 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind6));
							List<Material> list6 = source6.ToList();
							empty2 = ((list6.Count < 1) ? "" : list6.ElementAt(0).Name);
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dConvertedDouble9 = 0.0;
							if (empty2 != string.Empty)
							{
								dConvertedDouble9 = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble9, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dAngle7 = 0.0;
							if (!string.IsNullOrEmpty(empty2))
							{
								dAngle7 = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle7, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					if (k == 0)
					{
						listViewItem = m_TypeListView.Items.Add(familyTypeContainer.FamilyTypeWrapper.FamilyTypeName, empty2, string.Empty);
						listViewItem.Tag = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else
					{
						listViewItem.SubItems.Add(empty2);
					}
					break;
				case LandcaddID.ParkingLot:
					empty2 = string.Empty;
					FindParameterValue(m_ParameterList[k].ParameterName, familyTypeContainer.FamilyParameterList, ref empty2);
					if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.ParkingLotPavementDepthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.ParkingLotParkingBaseDepthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[k].ParameterName, RevitParameters.ParkingLotParkingSubBaseDepthParameter, ignoreCase: true) == 0)
					{
						double num = 0.0;
						if (empty2 != string.Empty)
						{
							num = ValueConversion.ToDouble(empty2);
							RevitGeometry.Units_ConvertDoubleToString(num, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
					}
					else if (empty2 == string.Empty && string.Compare(m_ParameterList[k].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
					{
						empty2 = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.ParkingLotParkingPavementSurfaceParameter, ignoreCase: true) == 0)
					{
						if (empty2 != string.Empty)
						{
							string strStringToFind = empty2;
							IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind));
							List<Material> list = source.ToList();
							empty2 = ((list.Count < 1) ? "" : list.ElementAt(0).Name);
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dConvertedDouble = 0.0;
							if (empty2 != string.Empty)
							{
								dConvertedDouble = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					else if (string.Compare(m_ParameterList[k].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
					{
						try
						{
							double dAngle = 0.0;
							if (!string.IsNullOrEmpty(empty2))
							{
								dAngle = ValueConversion.ToDouble(empty2);
							}
							RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref empty2);
						}
						catch (Exception)
						{
							empty2 = "";
						}
					}
					if (k == 0)
					{
						listViewItem = m_TypeListView.Items.Add(familyTypeContainer.FamilyTypeWrapper.FamilyTypeName, empty2, string.Empty);
						listViewItem.Tag = familyTypeContainer.FamilyTypeWrapper.FamilyTypeName;
					}
					else
					{
						listViewItem.SubItems.Add(empty2);
					}
					break;
				}
			}
		}
		if (m_TypeListView.Items.Count > 0)
		{
			m_TypeListView.Items[0].Selected = true;
		}
		m_DynGroupBox.Text = empty;
		m_SearchButton.Text = EPShared.LocalRM.GetString("AMP_SearchDialog");
		m_AddButton.Text = EPShared.LocalRM.GetString("AMP_New_NewFamilyDialog");
		m_ModifyButton.Text = EPShared.LocalRM.GetString("AMP_ModifyDialog");
		m_DeleteButton.Text = EPShared.LocalRM.GetString("AMP_DeleteDialog");
		m_EqualButton.Text = EPShared.LocalRM.GetString("AMP_EqualDialog");
		m_SaveFamilyButton.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		m_CancelButton.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
	}

	private void SetDefaultValues()
	{
		string empty = string.Empty;
		GetDialogPosition();
	}

	private void FindParameterValue(string _strParameterName, List<FamilyParameterWrapper> _FamilyParameterList, ref string _strValue)
	{
		_strValue = string.Empty;
		IEnumerable<FamilyParameterWrapper> enumerable = _FamilyParameterList.Where((FamilyParameterWrapper CurrentParameter) => string.Compare(CurrentParameter.ParameterName, _strParameterName, ignoreCase: false) == 0);
		if (enumerable != null)
		{
			List<FamilyParameterWrapper> list = enumerable.ToList();
			if (list.Count > 0)
			{
				_strValue = list[0].Value;
			}
		}
	}

	private bool FindParamenterValue(FamilyManager FamilyManager, FamilyType type, string _strParamter, ref string _strValue)
	{
		bool result = false;
		_strValue = string.Empty;
		FamilyParameter familyParameter = FamilyManager.get_Parameter(_strParamter);
		if (familyParameter != null && type.HasValue(familyParameter))
		{
			switch (familyParameter.StorageType)
			{
			case StorageType.Double:
				if (type.AsDouble(familyParameter) is double)
				{
					result = true;
					_strValue = ValueConversion.ToString(type.AsDouble(familyParameter).Value);
				}
				break;
			case StorageType.Integer:
				if (type.AsInteger(familyParameter) is int)
				{
					result = true;
					_strValue = Convert.ToString(type.AsInteger(familyParameter));
				}
				break;
			case StorageType.String:
				if (type.AsString(familyParameter) != null)
				{
					result = true;
					_strValue = type.AsString(familyParameter);
				}
				break;
			}
		}
		return result;
	}

	private void OnUpdateFamilyButtonClicked(object sender, EventArgs e)
	{
		if (m_SelectedFamily.FamilyTypeList.Count <= 0)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("ValidTypeMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return;
		}
		UpdateFamilyDocument(m_SelectedFamily.FamilyDocument, m_SelectedFamily.FamilyTypeList, m_CurrentFamilyType);
		DialogResult = DialogResult.OK;
	}

	private void CancelButtonClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
	}

	private void OnSearchButtonClicked(object sender, EventArgs e)
	{
	}

	private void CheckForDuplicates(List<FamilyTypeContainer> _PlantListFamilyTypeList, List<string> _TypeNames)
	{
	}

	private void OnTypeManagerFormClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private void SaveSettings()
	{
	}

	private void OnAddButtonClicked(object sender, EventArgs e)
	{
		int nSelectedIndex = -1;
		if (m_SelectedFamily.FamilyTypeList == null)
		{
			m_SelectedFamily.FamilyTypeList = new List<FamilyTypeContainer>();
		}
		List<FamilyTypeContainer> list = new List<FamilyTypeContainer>();
		list = m_SelectedFamily.FamilyTypeList;
		using AddModifyFamilyForm addModifyFamilyForm = new AddModifyFamilyForm(m_ParameterList, ref list, m_Units, m_DisplayUnitSystem, m_FormatOptions, m_UnitSymbolType, m_SlopeOptions, m_SlopeSymbolType, m_AngularOptions, m_AngularSymbolType, m_AllMaterialList, m_DecimalSymbolType, nSelectedIndex, m_CurrentFamilyType, m_SelectedFamily.FamilyName);
		addModifyFamilyForm.ShowDialog();
		if (addModifyFamilyForm.DialogResult == DialogResult.OK)
		{
			m_SelectedFamily.IsModified = true;
			m_SelectedFamily.FamilyTypeList = list;
			UpdateList(m_TypeListView.Items.Count, m_SelectedFamily.FamilyTypeList.Count, _bModify: false);
		}
	}

	private void OnModifyButtonClicked(object sender, EventArgs e)
	{
		if (m_TypeListView.Items.Count > 0)
		{
			Modify();
		}
	}

	private void Modify()
	{
		int num = -1;
		int num2 = -1;
		if (m_TypeListView.SelectedItems.Count >= 1)
		{
			for (num2 = 0; num2 < m_SelectedFamily.FamilyTypeList.Count; num2++)
			{
				if (m_SelectedFamily.FamilyTypeList[num2].Status != FamilyParameterWrapperStatus.Excluded)
				{
					num = m_TypeListView.SelectedItems.IndexOfKey(m_SelectedFamily.FamilyTypeList[num2].FamilyTypeWrapper.FamilyTypeName);
					if (num >= 0)
					{
						break;
					}
				}
			}
			if (m_SelectedFamily.FamilyTypeList == null)
			{
				m_SelectedFamily.FamilyTypeList = new List<FamilyTypeContainer>();
			}
			List<FamilyTypeContainer> list = new List<FamilyTypeContainer>();
			list = m_SelectedFamily.FamilyTypeList;
			using AddModifyFamilyForm addModifyFamilyForm = new AddModifyFamilyForm(m_ParameterList, ref list, m_Units, m_DisplayUnitSystem, m_FormatOptions, m_UnitSymbolType, m_SlopeOptions, m_SlopeSymbolType, m_AngularOptions, m_AngularSymbolType, m_AllMaterialList, m_DecimalSymbolType, num2, m_CurrentFamilyType, m_SelectedFamily.FamilyName);
			addModifyFamilyForm.ShowDialog();
			if (addModifyFamilyForm.DialogResult == DialogResult.OK)
			{
				m_SelectedFamily.FamilyTypeList = list;
				m_SelectedFamily.IsModified = true;
				UpdateList(m_TypeListView.SelectedIndices[num], num2, _bModify: true);
			}
			return;
		}
		string strText = EPShared.LocalRM.GetString("NoTypeSelectedMessage");
		RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
	}

	private void OnDeleteButtonClicked(object sender, EventArgs e)
	{
		int num = -1;
		int num2 = -1;
		if (m_TypeListView.SelectedItems.Count >= 1)
		{
			for (num2 = m_SelectedFamily.FamilyTypeList.Count - 1; num2 >= 0; num2--)
			{
				num = m_TypeListView.SelectedItems.IndexOfKey(m_SelectedFamily.FamilyTypeList[num2].FamilyTypeWrapper.FamilyTypeName);
				if (num >= 0)
				{
					m_SelectedFamily.FamilyTypeList.RemoveAt(num2);
					m_SelectedFamily.IsModified = true;
					m_TypeListView.SelectedItems[num].Remove();
					if (m_TypeListView.SelectedItems.Count == 0)
					{
						break;
					}
				}
			}
		}
		else
		{
			string strText = EPShared.LocalRM.GetString("NoTypeSelectedMessage");
			RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
	}

	private void CleanMix()
	{
	}

	private void UpdateList(int _nSelectedListIndex, int _nFamilyTypeListIndex, bool _bModify)
	{
		if (_bModify)
		{
			m_TypeListView.Items.RemoveAt(_nSelectedListIndex);
		}
		else
		{
			_nFamilyTypeListIndex--;
		}
		List<FamilyParameterTypeWrapper> list = new List<FamilyParameterTypeWrapper>();
		switch (m_CurrentFamilyType)
		{
		case LandcaddID.PlantMix:
		case LandcaddID.PatioWalk:
			list = ManagePlants.PlantMixParamenterList();
			break;
		case LandcaddID.Street:
			list = ManageFamily.StreetParamenterList();
			break;
		case LandcaddID.Curb:
			list = ManageFamily.CurbParameterList();
			break;
		case LandcaddID.Sidewalk:
			list = ManageFamily.SideWalkParamterList();
			break;
		case LandcaddID.RetainingWall:
			list = ManageFamily.RetainingWallParameterList();
			break;
		case LandcaddID.Steps:
			list = ManageFamily.StepsParameterList();
			break;
		case LandcaddID.ParkingLot:
			list = ManageFamily.ParkingLotParameterList();
			break;
		case LandcaddID.SoftTerrain:
			list = ManageFamily.SoftTerrainParameterList();
			break;
		}
		List<FamilyParameterWrapper> list2 = new List<FamilyParameterWrapper>();
		if (!_bModify)
		{
			FamilyParameterWrapper familyParameterWrapper = null;
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = 0; j < m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyParameterList.Count; j++)
				{
					if (string.Compare(list[i].ParameterName, m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyParameterList[j].ParameterName, ignoreCase: true) == 0)
					{
						list2.Add(m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyParameterList[j]);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					if (string.Compare(list[i].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
					{
						familyParameterWrapper = new FamilyParameterWrapper(list[i].ParameterDisplayName, list[i].ParameterName, m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyTypeWrapper.FamilyTypeName, StorageType.String);
						list2.Add(familyParameterWrapper);
						continue;
					}
					if (string.Compare(list[i].ParameterName, RevitParameters.RegionNameGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(list[i].ParameterName, RevitParameters.UniqueIDGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(list[i].ParameterName, RevitParameters.AssemblyCodeGenericFamilyParameter, ignoreCase: true) == 0)
					{
						familyParameterWrapper = new FamilyParameterWrapper(list[i].ParameterDisplayName, list[i].ParameterName, "", StorageType.String);
						list2.Add(familyParameterWrapper);
						continue;
					}
					if (string.Compare(list[i].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
					{
						familyParameterWrapper = new FamilyParameterWrapper(list[i].ParameterDisplayName, list[i].ParameterName, Convert.ToInt32(m_CurrentFamilyType).ToString("000"), StorageType.String);
						list2.Add(familyParameterWrapper);
						continue;
					}
					if (string.Compare(list[i].ParameterName, RevitParameters.PlacementIDFamilyParameter, ignoreCase: true) == 0)
					{
						familyParameterWrapper = new FamilyParameterWrapper(list[i].ParameterDisplayName, list[i].ParameterName, "", StorageType.String);
						list2.Add(familyParameterWrapper);
						continue;
					}
					if (string.Compare(list[i].ParameterName, RevitParameters.SpacingMethodPlantMixFamilyParameter, ignoreCase: true) == 0)
					{
						familyParameterWrapper = new FamilyParameterWrapper(list[i].ParameterDisplayName, list[i].ParameterName, RevitParameters.PlantMixSpacingMethodTriangular, StorageType.String);
						list2.Add(familyParameterWrapper);
						continue;
					}
					if (string.Compare(list[i].ParameterName, RevitParameters.PlantSpacingPlantMixFamilyParameter, ignoreCase: true) == 0 || string.Compare(list[i].ParameterName, RevitParameters.PercentagePlantMixFamilyParameter, ignoreCase: true) == 0)
					{
						familyParameterWrapper = new FamilyParameterWrapper(list[i].ParameterDisplayName, list[i].ParameterName, "0.0", StorageType.Double);
						list2.Add(familyParameterWrapper);
						continue;
					}
				}
				flag = false;
			}
			flag = false;
			string empty = string.Empty;
			StorageType storageType = StorageType.String;
			for (int k = 0; k < list.Count; k++)
			{
				for (int l = 0; l < list2.Count; l++)
				{
					if (string.Compare(list[k].ParameterName, list2[l].ParameterName, ignoreCase: true) == 0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					storageType = StorageType.String;
					empty = "";
					familyParameterWrapper = new FamilyParameterWrapper(list[k].ParameterDisplayName, list[k].ParameterName, empty, storageType);
					list2.Insert(k, familyParameterWrapper);
				}
				flag = false;
			}
		}
		else
		{
			list2 = m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyParameterList;
		}
		string _strToDisplay = string.Empty;
		ListViewItem listViewItem = new ListViewItem();
		listViewItem.Tag = m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyTypeWrapper.FamilyTypeName;
		for (int m = 0; m < list2.Count; m++)
		{
			switch (m_CurrentFamilyType)
			{
			case LandcaddID.PlantMix:
				_strToDisplay = list2[m].Value;
				if (string.Compare(list2[m].ParameterName, RevitParameters.RegionNameGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.UniqueIDGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.AssemblyCodeGenericFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (_strToDisplay == null)
				{
					_strToDisplay = string.Empty;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.PlantSpacingPlantMixFamilyParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble13 = 1.0;
					if (_strToDisplay != string.Empty)
					{
						dConvertedDouble13 = ValueConversion.ToDouble(_strToDisplay);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble13, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.PercentagePlantMixFamilyParameter, ignoreCase: true) == 0)
				{
					double num = 1.0;
					if (_strToDisplay != string.Empty)
					{
						num = ValueConversion.ToDouble(_strToDisplay);
					}
					_strToDisplay = ValueConversion.ToString(num);
				}
				break;
			case LandcaddID.PatioWalk:
				_strToDisplay = list2[m].Value;
				if (string.Compare(list2[m].ParameterName, RevitParameters.RegionNameGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.UniqueIDGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.AssemblyCodeGenericFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.ThicknessPathWalkFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.EdgeWidthPathWalkFamilyParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble3 = 1.0;
					if (_strToDisplay != string.Empty)
					{
						dConvertedDouble3 = ValueConversion.ToDouble(_strToDisplay);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.EdgePathWalkFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.MowingStripPathWalkFamilyParameter, ignoreCase: true) == 0)
				{
					if (string.Compare(_strToDisplay, "1", ignoreCase: true) != 0)
					{
						_strToDisplay = ((string.Compare(_strToDisplay, "0", ignoreCase: true) != 0) ? EPShared.LocalRM.GetString("NoString") : EPShared.LocalRM.GetString("NoString"));
					}
					else
					{
						_strToDisplay = EPShared.LocalRM.GetString("YesString");
					}
				}
				else if (_strToDisplay == string.Empty && string.Compare(list2[m].ParameterName, RevitParameters.MaterialPathWalkFamilyParameter, ignoreCase: true) == 0)
				{
					_strToDisplay = m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyTypeWrapper.FamilyTypeName;
				}
				break;
			case LandcaddID.SoftTerrain:
				_strToDisplay = list2[m].Value;
				if (string.Compare(list2[m].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.PlacementIDFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.SoftTerrainLeftOffsetWidthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.SoftTerrainRightOffsetWidthParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble9 = 0.0;
					if (_strToDisplay != string.Empty)
					{
						dConvertedDouble9 = ValueConversion.ToDouble(_strToDisplay);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble9, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.SoftTerrainCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.SoftTerrainFillSlopeParameter, ignoreCase: true) == 0)
				{
					RevitGeometry.Units_ConvertDoubleToString(ValueConversion.ToDouble(_strToDisplay), m_SlopeDisplayUnitType, m_SlopeAccuracy, m_SlopeSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
				{
					try
					{
						double dConvertedDouble10 = 0.0;
						if (_strToDisplay != string.Empty)
						{
							dConvertedDouble10 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble10, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				else
				{
					if (string.Compare(list2[m].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) != 0)
					{
						break;
					}
					try
					{
						double dAngle4 = 0.0;
						if (!string.IsNullOrEmpty(_strToDisplay))
						{
							dAngle4 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle4, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				break;
			case LandcaddID.Street:
				_strToDisplay = list2[m].Value;
				if (string.Compare(list2[m].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.PlacementIDFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.StreetGutterWidthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetCurbOffsetParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetCurbWidthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetCurbHeightParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetBoulevardWidthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetLaneWidthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetPavementDepthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetSubbaseSurfaceParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble7 = 0.0;
					if (_strToDisplay != string.Empty)
					{
						dConvertedDouble7 = ValueConversion.ToDouble(_strToDisplay);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble7, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.StreetCurbDisplayParameter, ignoreCase: true) == 0)
				{
					if (string.Compare(_strToDisplay, "1", ignoreCase: true) != 0)
					{
						_strToDisplay = ((string.Compare(_strToDisplay, "0", ignoreCase: true) != 0) ? EPShared.LocalRM.GetString("NoString") : EPShared.LocalRM.GetString("NoString"));
					}
					else
					{
						_strToDisplay = EPShared.LocalRM.GetString("YesString");
					}
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.StreetCrossSlopeParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetGutterSlopeParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetFillSlopeParameter, ignoreCase: true) == 0)
				{
					RevitGeometry.Units_ConvertDoubleToString(ValueConversion.ToDouble(_strToDisplay), m_SlopeDisplayUnitType, m_SlopeAccuracy, m_SlopeSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.StreetPavementSurfaceParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetCurbMaterialParameter, ignoreCase: true) == 0)
				{
					if (_strToDisplay != string.Empty)
					{
						string strStringToFind3 = _strToDisplay;
						IEnumerable<Material> source3 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind3));
						List<Material> list5 = source3.ToList();
						_strToDisplay = ((list5.Count < 1) ? "" : list5.ElementAt(0).Name);
					}
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
				{
					try
					{
						double dConvertedDouble8 = 0.0;
						if (_strToDisplay != string.Empty)
						{
							dConvertedDouble8 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble8, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				else
				{
					if (string.Compare(list2[m].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) != 0)
					{
						break;
					}
					try
					{
						double dAngle3 = 0.0;
						if (!string.IsNullOrEmpty(_strToDisplay))
						{
							dAngle3 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle3, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				break;
			case LandcaddID.Curb:
				_strToDisplay = list2[m].Value;
				if (string.Compare(list2[m].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.PlacementIDFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.StreetCurbHeightParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetCurbOffsetParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.CurbGutterHeightParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetCurbWidthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StreetGutterWidthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.CurbBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.CurbSubbaseSurfaceParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble11 = 0.0;
					if (_strToDisplay != string.Empty)
					{
						dConvertedDouble11 = ValueConversion.ToDouble(_strToDisplay);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble11, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.StreetGutterSlopeParameter, ignoreCase: true) == 0)
				{
					RevitGeometry.Units_ConvertDoubleToString(ValueConversion.ToDouble(_strToDisplay), m_SlopeDisplayUnitType, m_SlopeAccuracy, m_SlopeSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.StreetCurbMaterialParameter, ignoreCase: true) == 0)
				{
					if (_strToDisplay != string.Empty)
					{
						string strStringToFind4 = _strToDisplay;
						IEnumerable<Material> source4 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind4));
						List<Material> list6 = source4.ToList();
						_strToDisplay = ((list6.Count < 1) ? "" : list6.ElementAt(0).Name);
					}
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
				{
					try
					{
						double dConvertedDouble12 = 0.0;
						if (_strToDisplay != string.Empty)
						{
							dConvertedDouble12 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble12, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				else
				{
					if (string.Compare(list2[m].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) != 0)
					{
						break;
					}
					try
					{
						double dAngle5 = 0.0;
						if (!string.IsNullOrEmpty(_strToDisplay))
						{
							dAngle5 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle5, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				break;
			case LandcaddID.Sidewalk:
				_strToDisplay = list2[m].Value;
				if (string.Compare(list2[m].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.PlacementIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.SideWalkLocationLineParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.SideWalkWidthParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble4 = 0.0;
					if (_strToDisplay != string.Empty)
					{
						dConvertedDouble4 = ValueConversion.ToDouble(_strToDisplay);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble4, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					break;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.SideWalkPavementDepthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.SideWalkBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.SideWalkSubbaseSurfaceParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble5 = 0.0;
					if (_strToDisplay != string.Empty)
					{
						dConvertedDouble5 = ValueConversion.ToDouble(_strToDisplay);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble5, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					break;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.SideWalkCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.SideWalkFillSlopeParameter, ignoreCase: true) == 0)
				{
					RevitGeometry.Units_ConvertDoubleToString(ValueConversion.ToDouble(_strToDisplay), m_SlopeDisplayUnitType, m_SlopeAccuracy, m_SlopeSymbolType, m_Units, ref _strToDisplay);
					break;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.SideWalkPavementSurfaceParameter, ignoreCase: true) == 0)
				{
					if (_strToDisplay != string.Empty)
					{
						string strStringToFind2 = _strToDisplay;
						IEnumerable<Material> source2 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind2));
						List<Material> list4 = source2.ToList();
						_strToDisplay = ((list4.Count < 1) ? "" : list4.ElementAt(0).Name);
					}
					break;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.SideWalkLocationLineParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
				{
					try
					{
						double dConvertedDouble6 = 0.0;
						if (_strToDisplay != string.Empty)
						{
							dConvertedDouble6 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble6, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				else
				{
					if (string.Compare(list2[m].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) != 0)
					{
						break;
					}
					try
					{
						double dAngle2 = 0.0;
						if (!string.IsNullOrEmpty(_strToDisplay))
						{
							dAngle2 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle2, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				break;
			case LandcaddID.RetainingWall:
				_strToDisplay = list2[m].Value;
				if (string.Compare(list2[m].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.PlacementIDFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.RetainingWallWidthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.RetainingWallHeightParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.RetainingWallBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.RetainingWallSubbaseSurfaceParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble16 = 0.0;
					if (_strToDisplay != string.Empty)
					{
						dConvertedDouble16 = ValueConversion.ToDouble(_strToDisplay);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble16, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.RetainingWallCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.RetainingWallFillSlopeParameter, ignoreCase: true) == 0)
				{
					RevitGeometry.Units_ConvertDoubleToString(ValueConversion.ToDouble(_strToDisplay), m_SlopeDisplayUnitType, m_SlopeAccuracy, m_SlopeSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.RetainingWallMaterialParameter, ignoreCase: true) == 0)
				{
					if (_strToDisplay != string.Empty)
					{
						string strStringToFind6 = _strToDisplay;
						IEnumerable<Material> source6 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind6));
						List<Material> list8 = source6.ToList();
						_strToDisplay = ((list8.Count < 1) ? "" : list8.ElementAt(0).Name);
					}
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.RetainingWallSlopeProjectionParameter, ignoreCase: true) == 0)
				{
					if (string.Compare(_strToDisplay, "1", ignoreCase: true) != 0)
					{
						_strToDisplay = ((string.Compare(_strToDisplay, "0", ignoreCase: true) != 0) ? EPShared.LocalRM.GetString("NoString") : EPShared.LocalRM.GetString("NoString"));
					}
					else
					{
						_strToDisplay = EPShared.LocalRM.GetString("YesString");
					}
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
				{
					try
					{
						double dConvertedDouble17 = 0.0;
						if (_strToDisplay != string.Empty)
						{
							dConvertedDouble17 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble17, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				else
				{
					if (string.Compare(list2[m].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) != 0)
					{
						break;
					}
					try
					{
						double dAngle7 = 0.0;
						if (!string.IsNullOrEmpty(_strToDisplay))
						{
							dAngle7 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle7, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				break;
			case LandcaddID.Steps:
				_strToDisplay = list2[m].Value;
				if (string.Compare(list2[m].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.PlacementIDFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.StepsHeightParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StepsTreadParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.StepsWidthParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble14 = 0.0;
					if (_strToDisplay != string.Empty)
					{
						dConvertedDouble14 = ValueConversion.ToDouble(_strToDisplay);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble14, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.StepsMaterialParameter, ignoreCase: true) == 0)
				{
					if (_strToDisplay != string.Empty)
					{
						string strStringToFind5 = _strToDisplay;
						IEnumerable<Material> source5 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind5));
						List<Material> list7 = source5.ToList();
						_strToDisplay = ((list7.Count < 1) ? "" : list7.ElementAt(0).Name);
					}
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
				{
					try
					{
						double dConvertedDouble15 = 0.0;
						if (_strToDisplay != string.Empty)
						{
							dConvertedDouble15 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble15, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				else
				{
					if (string.Compare(list2[m].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) != 0)
					{
						break;
					}
					try
					{
						double dAngle6 = 0.0;
						if (!string.IsNullOrEmpty(_strToDisplay))
						{
							dAngle6 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle6, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				break;
			case LandcaddID.ParkingLot:
				_strToDisplay = list2[m].Value;
				if (string.Compare(list2[m].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.PlacementIDFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (string.Compare(list2[m].ParameterName, RevitParameters.ParkingLotPavementDepthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.ParkingLotParkingBaseDepthParameter, ignoreCase: true) == 0 || string.Compare(list2[m].ParameterName, RevitParameters.ParkingLotParkingSubBaseDepthParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble = 0.0;
					if (_strToDisplay != string.Empty)
					{
						dConvertedDouble = ValueConversion.ToDouble(_strToDisplay);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.ParkingLotParkingPavementSurfaceParameter, ignoreCase: true) == 0)
				{
					if (_strToDisplay != string.Empty)
					{
						string strStringToFind = _strToDisplay;
						IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind));
						List<Material> list3 = source.ToList();
						_strToDisplay = ((list3.Count < 1) ? "" : list3.ElementAt(0).Name);
					}
				}
				else if (string.Compare(list2[m].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
				{
					try
					{
						double dConvertedDouble2 = 0.0;
						if (_strToDisplay != string.Empty)
						{
							dConvertedDouble2 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				else
				{
					if (string.Compare(list2[m].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) != 0)
					{
						break;
					}
					try
					{
						double dAngle = 0.0;
						if (!string.IsNullOrEmpty(_strToDisplay))
						{
							dAngle = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = "";
					}
				}
				break;
			}
			if (m == 0)
			{
				listViewItem = ((!_bModify) ? m_TypeListView.Items.Add(m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyTypeWrapper.FamilyTypeName, m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyTypeWrapper.FamilyTypeName, string.Empty) : m_TypeListView.Items.Insert(_nSelectedListIndex, m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyTypeWrapper.FamilyTypeName, m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyTypeWrapper.FamilyTypeName, string.Empty));
				listViewItem.Tag = m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyTypeWrapper.FamilyTypeName;
			}
			else
			{
				listViewItem.SubItems.Add(_strToDisplay);
			}
		}
		int num2 = list2.FindIndex(FindTypeName);
		if (num2 >= 0)
		{
			list2.RemoveAt(num2);
		}
		m_SelectedFamily.FamilyTypeList[_nFamilyTypeListIndex].FamilyParameterList = list2;
		if (m_TypeListView.Items.Count > 0)
		{
			if (_bModify)
			{
				m_TypeListView.Items[_nSelectedListIndex].Selected = true;
			}
			else
			{
				m_TypeListView.Items[m_TypeListView.Items.Count - 1].Selected = true;
			}
		}
	}

	private static bool FindTypeName(FamilyParameterWrapper _CurrParameter)
	{
		if (string.Compare(_CurrParameter.ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
		{
			return true;
		}
		return false;
	}

	private void OnEqualButtonClick(object sender, EventArgs e)
	{
	}

	private void RefreshListView()
	{
	}

	private bool CheckPercentage()
	{
		return true;
	}

	private void MixListViewDoubleClicked(object sender, EventArgs e)
	{
		Modify();
	}

	private void OnMixListViewColumnClick(object sender, ColumnClickEventArgs e)
	{
		if (e.Column != m_nMixtListViewSortColumn)
		{
			m_nMixtListViewSortColumn = e.Column;
			m_TypeListView.Sorting = SortOrder.Ascending;
		}
		else if (m_TypeListView.Sorting == SortOrder.Ascending)
		{
			m_TypeListView.Sorting = SortOrder.Descending;
		}
		else
		{
			m_TypeListView.Sorting = SortOrder.Ascending;
		}
		m_TypeListView.Sort();
		m_TypeListView.ListViewItemSorter = new ListViewItemComparer(e.Column, m_TypeListView.Sorting);
	}

	private bool CreateParameterList(Document _ActiveDocument, LandcaddID _Type, ref Document _CurDocument, ref EPTransactionContainer _TransactionContainer, ref List<FamilyParameter> _NewFamilyParameterList)
	{
		_CurDocument = _ActiveDocument;
		_TransactionContainer = new EPTransactionContainer(_CurDocument);
		List<FamilyParameterTypeWrapper> list = new List<FamilyParameterTypeWrapper>();
		string empty = string.Empty;
		switch (_Type)
		{
		case LandcaddID.PlantMix:
			list = ManagePlants.PlantMixParamenterList();
			_TransactionContainer.Start(EPShared.LocalRM.GetString("CreatePlantMixFamilyTransaction"));
			break;
		case LandcaddID.PatioWalk:
			list = ManagePlants.PatioWalkParamenterList();
			_TransactionContainer.Start(EPShared.LocalRM.GetString("CreatePatioWalkFamilyTransaction"));
			break;
		case LandcaddID.Street:
			list = ManageFamily.StreetParamenterList();
			empty = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("StreetTypeManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty);
			break;
		case LandcaddID.Curb:
			list = ManageFamily.CurbParameterList();
			empty = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("CurbTypeManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty);
			break;
		case LandcaddID.Sidewalk:
			list = ManageFamily.SideWalkParamterList();
			empty = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("SidewalkTypeManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty);
			break;
		case LandcaddID.RetainingWall:
			list = ManageFamily.RetainingWallParameterList();
			empty = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("RetainingWallManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty);
			break;
		case LandcaddID.Steps:
			list = ManageFamily.StepsParameterList();
			empty = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("StepsManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty);
			break;
		case LandcaddID.ParkingLot:
			list = ManageFamily.ParkingLotParameterList();
			empty = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("ParkingLotManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty);
			break;
		case LandcaddID.SoftTerrain:
			list = ManageFamily.SoftTerrainParameterList();
			empty = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("SoftTerrainManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty);
			break;
		}
		_TransactionContainer.BeginTransaction();
		BuiltInParameterGroup parameterGroup = BuiltInParameterGroup.INVALID;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = list.Count;
		if (list.Count <= 0)
		{
			progressBarForm.MaxValue = 30;
		}
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressbarSettingParametersMessage");
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		for (int i = 0; i < list.Count; i++)
		{
			if (string.Compare(list[i].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
			{
				continue;
			}
			FamilyParameter familyParameter = null;
			if (list[i].BuiltInParameter != BuiltInParameter.INVALID)
			{
				familyParameter = _CurDocument.FamilyManager.get_Parameter(list[i].BuiltInParameter);
			}
			if (familyParameter == null)
			{
				familyParameter = _CurDocument.FamilyManager.get_Parameter(list[i].ParameterName);
			}
			if (familyParameter != null)
			{
				_NewFamilyParameterList.Add(familyParameter);
			}
			else
			{
				if (list[i].ParameterType == ParameterType.Number || list[i].ParameterType == ParameterType.Integer || list[i].ParameterType == ParameterType.Length)
				{
					parameterGroup = BuiltInParameterGroup.PG_GEOMETRY;
				}
				else if (list[i].ParameterType == ParameterType.Text || list[i].ParameterType == ParameterType.FamilyType || list[i].ParameterType == ParameterType.Currency)
				{
					parameterGroup = BuiltInParameterGroup.PG_IDENTITY_DATA;
				}
				if (string.Compare(list[i].ParameterName, RevitParameters.UniqueIDGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(list[i].ParameterName, RevitParameters.RegionNameGenericModelFamilyParameter, ignoreCase: true) == 0)
				{
					_NewFamilyParameterList.Add(_CurDocument.FamilyManager.AddParameter(list[i].ParameterName, parameterGroup, list[i].ParameterType, isInstance: true));
				}
				else
				{
					_NewFamilyParameterList.Add(_CurDocument.FamilyManager.AddParameter(list[i].ParameterName, parameterGroup, list[i].ParameterType, isInstance: false));
				}
			}
			progressBarForm.UpdateProgressBar(1);
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		return true;
	}

	public bool UpdateFamilyDocument(Document _ActiveDocument, List<FamilyTypeContainer> _FamilyTypeList, LandcaddID _Type)
	{
		double _dValue = 0.0;
		string _strValue = string.Empty;
		int _nValue = 0;
		ElementId _Value = null;
		Document _CurDocument = _ActiveDocument;
		EPTransactionContainer _TransactionContainer = null;
		List<FamilyParameter> _NewFamilyParameterList = new List<FamilyParameter>();
		if (!CreateParameterList(_ActiveDocument, _Type, ref _CurDocument, ref _TransactionContainer, ref _NewFamilyParameterList))
		{
			return false;
		}
		bool flag = false;
		FamilyType familyType = null;
		string empty = string.Empty;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = _FamilyTypeList.Count;
		if (_FamilyTypeList.Count <= 0)
		{
			progressBarForm.MaxValue = 30;
		}
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressbarCreatingFamilyMessage");
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		List<FamilyType> list = ManageFamily.RemoveTypesFromDocument(_CurDocument.FamilyManager.Types, _FamilyTypeList);
		for (int i = 0; i < _FamilyTypeList.Count; i++)
		{
			if (!CheckForSiteworksIDParameter(_FamilyTypeList[i].FamilyParameterList))
			{
				FamilyParameterWrapper item = new FamilyParameterWrapper(EPShared.LocalRM.GetString("SiteworksIDFamilyParameter"), RevitParameters.SiteworksIDFamilyParameter, "", StorageType.String);
				_FamilyTypeList[i].FamilyParameterList.Add(item);
			}
			FamilyType _SelectedType = null;
			if (!(_FamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName != string.Empty))
			{
				continue;
			}
			familyType = (ManageFamily.FindTypeNames(_CurDocument.FamilyManager.Types, _FamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName, ref _SelectedType) ? _SelectedType : _CurDocument.FamilyManager.NewType(_FamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName));
			_CurDocument.FamilyManager.CurrentType = familyType;
			progressBarForm.Message = EPShared.LocalRM.GetString("CreatingFamilyTypeMessage") + " " + familyType.Name;
			progressBarForm.ResetValue();
			progressBarForm.MinValue = 0;
			progressBarForm.MaxValue = _NewFamilyParameterList.Count * 2;
			FamilyParameter familyParameter = null;
			FamilyParameterWrapper familyParameterWrapper = null;
			int num = 0;
			int num2 = 0;
			for (num = 0; num < _NewFamilyParameterList.Count; num++)
			{
				progressBarForm.UpdateProgressBar(1);
				familyParameter = _NewFamilyParameterList[num];
				if (familyParameter == null)
				{
					continue;
				}
				for (num2 = 0; num2 < _FamilyTypeList[i].FamilyParameterList.Count; num2++)
				{
					familyParameterWrapper = _FamilyTypeList[i].FamilyParameterList[num2];
					if (familyParameterWrapper == null || ((familyParameterWrapper.FamilyParameter == null || familyParameterWrapper.FamilyParameter.Id.Value64() == -1 || !(familyParameterWrapper.FamilyParameter.Id == familyParameter.Id)) && string.Compare(familyParameterWrapper.ParameterName, familyParameter.Definition.Name, ignoreCase: true) != 0) || familyParameterWrapper.StorageType != familyParameter.StorageType)
					{
						continue;
					}
					empty = familyParameter.Formula;
					if (empty != null && !(empty == string.Empty))
					{
						continue;
					}
					if (_FamilyTypeList[i].HasValue(num2))
					{
						switch (familyParameterWrapper.StorageType)
						{
						case StorageType.Double:
							if (!_FamilyTypeList[i].GetDoubleValue(num2, ref _dValue))
							{
								break;
							}
							try
							{
								if (familyParameter.LegacyDisplayUnit() == DisplayUnitType.DUT_RATIO_10 || familyParameter.LegacyDisplayUnit() == DisplayUnitType.DUT_RATIO_12 || familyParameter.LegacyDisplayUnit() == DisplayUnitType.DUT_RISE_OVER_INCHES || familyParameter.LegacyDisplayUnit() == DisplayUnitType.DUT_RISE_OVER_FOOT || familyParameter.LegacyDisplayUnit() == DisplayUnitType.DUT_RISE_OVER_MMS || familyParameter.LegacyDisplayUnit() == DisplayUnitType.DUT_SLOPE_DEGREES)
								{
									FormatOptions formatOptions = _CurDocument.GetUnits().GetFormatOptions(UnitType.UT_Slope);
									if (formatOptions.LegacyDisplayUnits() != DisplayUnitType.DUT_PERCENTAGE)
									{
										formatOptions.SetLegacyDisplayUnits(DisplayUnitType.DUT_PERCENTAGE);
									}
								}
							}
							catch (Autodesk.Revit.Exceptions.InvalidOperationException)
							{
							}
							_CurDocument.FamilyManager.Set(familyParameter, _dValue);
							break;
						case StorageType.Integer:
							if (_FamilyTypeList[i].GetIntegerValue(num2, ref _nValue))
							{
								_CurDocument.FamilyManager.Set(familyParameter, _nValue);
							}
							break;
						case StorageType.String:
							if (!_FamilyTypeList[i].GetStringValue(num2, ref _strValue))
							{
								break;
							}
							if (string.Compare(_FamilyTypeList[i].FamilyParameterList[num2].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
							{
								switch (_Type)
								{
								case LandcaddID.PlantMix:
									_strValue = Convert.ToInt32(LandcaddID.PlantMix).ToString("000");
									break;
								case LandcaddID.PatioWalk:
									_strValue = Convert.ToInt32(LandcaddID.PatioWalk).ToString("000");
									break;
								case LandcaddID.Street:
									_strValue = Convert.ToInt32(LandcaddID.Street).ToString("000");
									break;
								case LandcaddID.Curb:
									_strValue = Convert.ToInt32(LandcaddID.Curb).ToString("000");
									break;
								case LandcaddID.Sidewalk:
									_strValue = Convert.ToInt32(LandcaddID.Sidewalk).ToString("000");
									break;
								case LandcaddID.RetainingWall:
									_strValue = Convert.ToInt32(LandcaddID.RetainingWall).ToString("000");
									break;
								case LandcaddID.Steps:
									_strValue = Convert.ToInt32(LandcaddID.Steps).ToString("000");
									break;
								case LandcaddID.ParkingLot:
									_strValue = Convert.ToInt32(LandcaddID.ParkingLot).ToString("000");
									break;
								case LandcaddID.SoftTerrain:
									_strValue = Convert.ToInt32(LandcaddID.SoftTerrain).ToString("000");
									break;
								}
							}
							_CurDocument.FamilyManager.Set(familyParameter, _strValue);
							break;
						case StorageType.ElementId:
							if (!_FamilyTypeList[i].GetElementIdValue(num2, ref _Value))
							{
								break;
							}
							try
							{
								_CurDocument.FamilyManager.Set(familyParameter, _Value);
							}
							catch (System.ArgumentOutOfRangeException ex)
							{
								if (ex.Message.StartsWith(EPShared.LocalRM.GetString("OutOfRangeArgumentMessage")))
								{
									flag = true;
								}
							}
							break;
						}
					}
					else if (string.Compare(_FamilyTypeList[i].FamilyParameterList[num2].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
					{
						switch (_Type)
						{
						case LandcaddID.PlantMix:
							_strValue = Convert.ToInt32(LandcaddID.PlantMix).ToString("000");
							break;
						case LandcaddID.PatioWalk:
							_strValue = Convert.ToInt32(LandcaddID.PatioWalk).ToString("000");
							break;
						case LandcaddID.Street:
							_strValue = Convert.ToInt32(LandcaddID.Street).ToString("000");
							break;
						case LandcaddID.Curb:
							_strValue = Convert.ToInt32(LandcaddID.Curb).ToString("000");
							break;
						case LandcaddID.Sidewalk:
							_strValue = Convert.ToInt32(LandcaddID.Sidewalk).ToString("000");
							break;
						case LandcaddID.RetainingWall:
							_strValue = Convert.ToInt32(LandcaddID.RetainingWall).ToString("000");
							break;
						case LandcaddID.Steps:
							_strValue = Convert.ToInt32(LandcaddID.Steps).ToString("000");
							break;
						case LandcaddID.ParkingLot:
							_strValue = Convert.ToInt32(LandcaddID.ParkingLot).ToString("000");
							break;
						case LandcaddID.SoftTerrain:
							_strValue = Convert.ToInt32(LandcaddID.SoftTerrain).ToString("000");
							break;
						}
						_CurDocument.FamilyManager.Set(familyParameter, _strValue);
					}
					else
					{
						switch (familyParameter.StorageType)
						{
						case StorageType.String:
							_CurDocument.FamilyManager.Set(familyParameter, string.Empty);
							break;
						case StorageType.Double:
							_CurDocument.FamilyManager.Set(familyParameter, 0.0);
							break;
						case StorageType.Integer:
							_CurDocument.FamilyManager.Set(familyParameter, 0);
							break;
						}
					}
					break;
				}
			}
			for (num = 0; num < _NewFamilyParameterList.Count; num++)
			{
				progressBarForm.UpdateProgressBar(1);
				familyParameter = _NewFamilyParameterList[num];
				if (familyParameter == null)
				{
					continue;
				}
				for (num2 = 0; num2 < _FamilyTypeList[i].FamilyParameterList.Count; num2++)
				{
					familyParameterWrapper = _FamilyTypeList[i].FamilyParameterList[num2];
					if (familyParameterWrapper == null || ((familyParameterWrapper.FamilyParameter == null || familyParameterWrapper.FamilyParameter.Id.Value64() == -1 || !(familyParameterWrapper.FamilyParameter.Id == familyParameter.Id)) && string.Compare(familyParameterWrapper.ParameterName, familyParameter.Definition.Name, ignoreCase: true) != 0) || familyParameterWrapper.StorageType != familyParameter.StorageType)
					{
						continue;
					}
					empty = familyParameter.Formula;
					if (empty != null && empty != string.Empty)
					{
						if (_FamilyTypeList[i].HasValue(num2))
						{
							_CurDocument.FamilyManager.SetFormula(familyParameter, empty);
						}
						_FamilyTypeList[i].FamilyParameterList.RemoveAt(num2);
						_NewFamilyParameterList.RemoveAt(num);
						num--;
						break;
					}
				}
			}
		}
		foreach (FamilyType item2 in list)
		{
			_CurDocument.FamilyManager.CurrentType = item2;
			_CurDocument.FamilyManager.DeleteCurrentType();
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		_TransactionContainer.EndTransaction();
		_TransactionContainer.Commit();
		if (flag)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("GetNewRevitVersionMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
		return true;
	}

	private bool CheckForSiteworksIDParameter(List<FamilyParameterWrapper> _ParameterWrapperList)
	{
		foreach (FamilyParameterWrapper _ParameterWrapper in _ParameterWrapperList)
		{
			if (string.Compare(_ParameterWrapper.ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: false) == 0)
			{
				return true;
			}
		}
		return false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.m_DynGroupBox = new System.Windows.Forms.GroupBox();
		this.m_ModifyButton = new System.Windows.Forms.Button();
		this.m_EqualButton = new System.Windows.Forms.Button();
		this.m_DeleteButton = new System.Windows.Forms.Button();
		this.m_AddButton = new System.Windows.Forms.Button();
		this.m_SearchButton = new System.Windows.Forms.Button();
		this.m_TypeListView = new System.Windows.Forms.ListView();
		this.m_SaveFamilyButton = new System.Windows.Forms.Button();
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_DynGroupBox.SuspendLayout();
		base.SuspendLayout();
		this.m_DynGroupBox.Controls.Add(this.m_ModifyButton);
		this.m_DynGroupBox.Controls.Add(this.m_EqualButton);
		this.m_DynGroupBox.Controls.Add(this.m_DeleteButton);
		this.m_DynGroupBox.Controls.Add(this.m_AddButton);
		this.m_DynGroupBox.Controls.Add(this.m_SearchButton);
		this.m_DynGroupBox.Controls.Add(this.m_TypeListView);
		this.m_DynGroupBox.Location = new System.Drawing.Point(12, 12);
		this.m_DynGroupBox.Name = "m_DynGroupBox";
		this.m_DynGroupBox.Size = new System.Drawing.Size(573, 281);
		this.m_DynGroupBox.TabIndex = 0;
		this.m_DynGroupBox.TabStop = false;
		this.m_DynGroupBox.Text = "PlantMix";
		this.m_ModifyButton.Location = new System.Drawing.Point(491, 41);
		this.m_ModifyButton.Name = "m_ModifyButton";
		this.m_ModifyButton.Size = new System.Drawing.Size(75, 23);
		this.m_ModifyButton.TabIndex = 2;
		this.m_ModifyButton.Text = "Modify";
		this.m_ModifyButton.UseVisualStyleBackColor = true;
		this.m_ModifyButton.Click += new System.EventHandler(this.OnModifyButtonClicked);
		this.m_EqualButton.Location = new System.Drawing.Point(491, 100);
		this.m_EqualButton.Name = "m_EqualButton";
		this.m_EqualButton.Size = new System.Drawing.Size(75, 23);
		this.m_EqualButton.TabIndex = 4;
		this.m_EqualButton.Text = "Equal";
		this.m_EqualButton.UseVisualStyleBackColor = true;
		this.m_EqualButton.Click += new System.EventHandler(this.OnEqualButtonClick);
		this.m_DeleteButton.Location = new System.Drawing.Point(492, 70);
		this.m_DeleteButton.Name = "m_DeleteButton";
		this.m_DeleteButton.Size = new System.Drawing.Size(75, 24);
		this.m_DeleteButton.TabIndex = 3;
		this.m_DeleteButton.Text = "Delete";
		this.m_DeleteButton.UseVisualStyleBackColor = true;
		this.m_DeleteButton.Click += new System.EventHandler(this.OnDeleteButtonClicked);
		this.m_AddButton.Location = new System.Drawing.Point(491, 12);
		this.m_AddButton.Name = "m_AddButton";
		this.m_AddButton.Size = new System.Drawing.Size(75, 23);
		this.m_AddButton.TabIndex = 1;
		this.m_AddButton.Text = "Add";
		this.m_AddButton.UseVisualStyleBackColor = true;
		this.m_AddButton.Click += new System.EventHandler(this.OnAddButtonClicked);
		this.m_SearchButton.Location = new System.Drawing.Point(491, 129);
		this.m_SearchButton.Name = "m_SearchButton";
		this.m_SearchButton.Size = new System.Drawing.Size(75, 23);
		this.m_SearchButton.TabIndex = 5;
		this.m_SearchButton.Text = "Search";
		this.m_SearchButton.UseVisualStyleBackColor = true;
		this.m_SearchButton.Click += new System.EventHandler(this.OnSearchButtonClicked);
		this.m_TypeListView.FullRowSelect = true;
		this.m_TypeListView.HideSelection = false;
		this.m_TypeListView.Location = new System.Drawing.Point(4, 14);
		this.m_TypeListView.Name = "m_TypeListView";
		this.m_TypeListView.Size = new System.Drawing.Size(481, 261);
		this.m_TypeListView.TabIndex = 0;
		this.m_TypeListView.UseCompatibleStateImageBehavior = false;
		this.m_TypeListView.DoubleClick += new System.EventHandler(this.MixListViewDoubleClicked);
		this.m_TypeListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.OnMixListViewColumnClick);
		this.m_SaveFamilyButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_SaveFamilyButton.Location = new System.Drawing.Point(429, 299);
		this.m_SaveFamilyButton.Name = "m_SaveFamilyButton";
		this.m_SaveFamilyButton.Size = new System.Drawing.Size(75, 23);
		this.m_SaveFamilyButton.TabIndex = 1;
		this.m_SaveFamilyButton.Text = "OK";
		this.m_SaveFamilyButton.UseVisualStyleBackColor = true;
		this.m_SaveFamilyButton.Click += new System.EventHandler(this.OnUpdateFamilyButtonClicked);
		this.m_CancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_CancelButton.Location = new System.Drawing.Point(510, 299);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 2;
		this.m_CancelButton.Text = "Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.CancelButtonClicked);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(599, 334);
		base.Controls.Add(this.m_CancelButton);
		base.Controls.Add(this.m_SaveFamilyButton);
		base.Controls.Add(this.m_DynGroupBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.MinimumSize = new System.Drawing.Size(598, 359);
		base.Name = "TypeManagerForm";
		this.Text = "TypeManager";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnTypeManagerFormClosing);
		this.m_DynGroupBox.ResumeLayout(false);
		base.ResumeLayout(false);
		base.AcceptButton = this.m_SaveFamilyButton;
		base.CancelButton = this.m_CancelButton;
	}
}
