using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.Exceptions;
using SITES.Utilities;

namespace EPCommon;

public class ManagePlants
{
	public static PlantField GetPlantFieldFromParamenter(string _strValue)
	{
		PlantField result = PlantField.None;
		if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantScientificNamePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantScientificName;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantCommonNamePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantCommonName;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantCodeNamePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantCodeName;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsAdmirationPointPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsAdmirationPoint;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("TemperatureRangePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.TemperatureRange;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("ConditionsSoilConditionPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.ConditionsSoilCondition;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("ConditionsExposurePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.ConditionsExposure;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("ConditionsMiscellaneousPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.ConditionsMiscellaneous;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("ConditionsTolerancesPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.ConditionsTolerances;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantTypePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantType;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("GrowthGrowthRatePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.GrowthGrowthRate;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("GrowthServiceLifePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.GrowthServiceLife;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantHeightPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantHeight;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantWidthPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantWidth;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("GrowthPatternPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.GrowthPattern;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("GrowthFormPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.GrowthForm;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("MaintenanceWaterRequirementsPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.MaintenanceWaterRequirements;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("MaintenanceTransplantPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.MaintenanceTransplant;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("MaintenanceDetrimentsPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.MaintenanceDetriments;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("MaintenancePlantingTimePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.MaintenancePlantingTime;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("MaintenanceTransplantingTimePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.MaintenanceTransplantingTime;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("MaintenanceFertilizingTimePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.MaintenanceFertilizingTime;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("MaintenancePruningTimePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.MaintenancePruningTime;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsFoliageSizePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsFoliageSize;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsFoliageSurfacePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsFoliageSurface;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsFoliageColorPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsFoliageColor;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsFallColorPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsFallColor;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsCrownDensityPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsCrownDensity;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsBarkSurfacePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsBarkSurface;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsBarkColorPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsBarkColor;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsFruitColorPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsFruitColor;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsBloomEffectPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsBloomEffect;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsFragrancePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsFragrance;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsBloomPeriodPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsBloomPeriod;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CharacteristicsFruitPeriodPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.CharacteristicsFruitPeriod;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantUsesPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantUses;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantClimateAndZonePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantClimateAndZone;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantingSizePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantingSize;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantSpacingPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantSpacing;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantCommentsPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantComments;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantingSymbolPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantingSymbol;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlantCountPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantCount;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("GenericModelSymbolPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.GenericModelSymbol;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("PlacementIDFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.PlantPlacementID;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("HeightPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.Height;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("KeynotePlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.Keynote;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("ModelPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.Model;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("ManufacturerPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.Manufacturer;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("TypeCommentsPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.TypeComments;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("URLPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.URL;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("DescriptionPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.Description;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("CostPlantFamilyParameter"), ignoreCase: true) == 0)
		{
			result = PlantField.Cost;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), ignoreCase: true) == 0)
		{
			result = PlantField.TypeName;
		}
		return result;
	}

	public static PlantField GetPlantField(string _strPlantFieldName)
	{
		foreach (int value in Enum.GetValues(typeof(PlantField)))
		{
			if (string.Compare(_strPlantFieldName, Enum.GetName(typeof(PlantField), value), ignoreCase: true) == 0)
			{
				return (PlantField)Enum.ToObject(typeof(PlantField), value);
			}
		}
		return PlantField.None;
	}

	public static string GetPlantParamenterName(PlantField _PlantField)
	{
		string result = string.Empty;
		switch (_PlantField)
		{
		case PlantField.CharacteristicsAdmirationPoint:
			result = EPShared.LocalRM.GetString("CharacteristicsAdmirationPointPlantFamilyParameter");
			break;
		case PlantField.PlantUses:
			result = EPShared.LocalRM.GetString("PlantUsesPlantFamilyParameter");
			break;
		case PlantField.CharacteristicsBarkColor:
			result = EPShared.LocalRM.GetString("CharacteristicsBarkColorPlantFamilyParameter");
			break;
		case PlantField.CharacteristicsBarkSurface:
			result = EPShared.LocalRM.GetString("CharacteristicsBarkSurfacePlantFamilyParameter");
			break;
		case PlantField.CharacteristicsBloomEffect:
			result = EPShared.LocalRM.GetString("CharacteristicsBloomEffectPlantFamilyParameter");
			break;
		case PlantField.CharacteristicsBloomPeriod:
			result = EPShared.LocalRM.GetString("CharacteristicsBloomPeriodPlantFamilyParameter");
			break;
		case PlantField.PlantCodeName:
			result = EPShared.LocalRM.GetString("PlantCodeNamePlantFamilyParameter");
			break;
		case PlantField.PlantComments:
			result = EPShared.LocalRM.GetString("PlantCommentsPlantFamilyParameter");
			break;
		case PlantField.PlantCommonName:
			result = EPShared.LocalRM.GetString("PlantCommonNamePlantFamilyParameter");
			break;
		case PlantField.CharacteristicsCrownDensity:
			result = EPShared.LocalRM.GetString("CharacteristicsCrownDensityPlantFamilyParameter");
			break;
		case PlantField.MaintenanceDetriments:
			result = EPShared.LocalRM.GetString("MaintenanceDetrimentsPlantFamilyParameter");
			break;
		case PlantField.ConditionsExposure:
			result = EPShared.LocalRM.GetString("ConditionsExposurePlantFamilyParameter");
			break;
		case PlantField.CharacteristicsFallColor:
			result = EPShared.LocalRM.GetString("CharacteristicsFallColorPlantFamilyParameter");
			break;
		case PlantField.MaintenanceFertilizingTime:
			result = EPShared.LocalRM.GetString("MaintenanceFertilizingTimePlantFamilyParameter");
			break;
		case PlantField.CharacteristicsFoliageColor:
			result = EPShared.LocalRM.GetString("CharacteristicsFoliageColorPlantFamilyParameter");
			break;
		case PlantField.CharacteristicsFoliageSize:
			result = EPShared.LocalRM.GetString("CharacteristicsFoliageSizePlantFamilyParameter");
			break;
		case PlantField.CharacteristicsFoliageSurface:
			result = EPShared.LocalRM.GetString("CharacteristicsFoliageSurfacePlantFamilyParameter");
			break;
		case PlantField.CharacteristicsFragrance:
			result = EPShared.LocalRM.GetString("CharacteristicsFragrancePlantFamilyParameter");
			break;
		case PlantField.CharacteristicsFruitColor:
			result = EPShared.LocalRM.GetString("CharacteristicsFruitColorPlantFamilyParameter");
			break;
		case PlantField.CharacteristicsFruitPeriod:
			result = EPShared.LocalRM.GetString("CharacteristicsFruitPeriodPlantFamilyParameter");
			break;
		case PlantField.PlantCount:
			result = EPShared.LocalRM.GetString("PlantCountPlantFamilyParameter");
			break;
		case PlantField.GrowthForm:
			result = EPShared.LocalRM.GetString("GrowthFormPlantFamilyParameter");
			break;
		case PlantField.GrowthPattern:
			result = EPShared.LocalRM.GetString("GrowthPatternPlantFamilyParameter");
			break;
		case PlantField.GrowthGrowthRate:
			result = EPShared.LocalRM.GetString("GrowthGrowthRatePlantFamilyParameter");
			break;
		case PlantField.PlantHeight:
			result = EPShared.LocalRM.GetString("PlantHeightPlantFamilyParameter");
			break;
		case PlantField.ConditionsMiscellaneous:
			result = EPShared.LocalRM.GetString("ConditionsMiscellaneousPlantFamilyParameter");
			break;
		case PlantField.PlantingSymbol:
			result = EPShared.LocalRM.GetString("PlantingSymbolPlantFamilyParameter");
			break;
		case PlantField.PlantingSize:
			result = EPShared.LocalRM.GetString("PlantingSizePlantFamilyParameter");
			break;
		case PlantField.MaintenancePlantingTime:
			result = EPShared.LocalRM.GetString("MaintenancePlantingTimePlantFamilyParameter");
			break;
		case PlantField.PlantType:
			result = EPShared.LocalRM.GetString("PlantTypePlantFamilyParameter");
			break;
		case PlantField.MaintenancePruningTime:
			result = EPShared.LocalRM.GetString("MaintenancePruningTimePlantFamilyParameter");
			break;
		case PlantField.PlantScientificName:
			result = EPShared.LocalRM.GetString("PlantScientificNamePlantFamilyParameter");
			break;
		case PlantField.GrowthServiceLife:
			result = EPShared.LocalRM.GetString("GrowthServiceLifePlantFamilyParameter");
			break;
		case PlantField.ConditionsSoilCondition:
			result = EPShared.LocalRM.GetString("ConditionsSoilConditionPlantFamilyParameter");
			break;
		case PlantField.PlantSpacing:
			result = EPShared.LocalRM.GetString("PlantSpacingPlantFamilyParameter");
			break;
		case PlantField.ConditionsTolerances:
			result = EPShared.LocalRM.GetString("ConditionsTolerancesPlantFamilyParameter");
			break;
		case PlantField.MaintenanceTransplant:
			result = EPShared.LocalRM.GetString("MaintenanceTransplantPlantFamilyParameter");
			break;
		case PlantField.MaintenanceTransplantingTime:
			result = EPShared.LocalRM.GetString("MaintenanceTransplantingTimePlantFamilyParameter");
			break;
		case PlantField.PlantClimateAndZone:
			result = EPShared.LocalRM.GetString("PlantClimateAndZonePlantFamilyParameter");
			break;
		case PlantField.TemperatureRange:
			result = EPShared.LocalRM.GetString("TemperatureRangePlantFamilyParameter");
			break;
		case PlantField.MaintenanceWaterRequirements:
			result = EPShared.LocalRM.GetString("MaintenanceWaterRequirementsPlantFamilyParameter");
			break;
		case PlantField.PlantWidth:
			result = EPShared.LocalRM.GetString("PlantWidthPlantFamilyParameter");
			break;
		case PlantField.GenericModelSymbol:
			result = EPShared.LocalRM.GetString("GenericModelSymbolPlantFamilyParameter");
			break;
		case PlantField.PlantPlacementID:
			result = EPShared.LocalRM.GetString("PlacementIDFamilyParameter");
			break;
		case PlantField.Height:
			result = EPShared.LocalRM.GetString("HeightPlantFamilyParameter");
			break;
		case PlantField.Keynote:
			result = EPShared.LocalRM.GetString("KeynotePlantFamilyParameter");
			break;
		case PlantField.Model:
			result = EPShared.LocalRM.GetString("ModelPlantFamilyParameter");
			break;
		case PlantField.Manufacturer:
			result = EPShared.LocalRM.GetString("ManufacturerPlantFamilyParameter");
			break;
		case PlantField.TypeComments:
			result = EPShared.LocalRM.GetString("TypeCommentsPlantFamilyParameter");
			break;
		case PlantField.URL:
			result = EPShared.LocalRM.GetString("URLPlantFamilyParameter");
			break;
		case PlantField.Description:
			result = EPShared.LocalRM.GetString("DescriptionPlantFamilyParameter");
			break;
		case PlantField.Cost:
			result = EPShared.LocalRM.GetString("CostPlantFamilyParameter");
			break;
		case PlantField.TypeName:
			result = EPShared.LocalRM.GetString("TypeNamePlantFamilyType");
			break;
		}
		return result;
	}

	public static List<string> PlantAllParamenterList()
	{
		List<string> list = PlantSharedParamenterList();
		List<string> list2 = PlantingCommonParamenterList();
		for (int i = 0; i < list2.Count; i++)
		{
			list.Add(list2[i]);
		}
		return list;
	}

	private static List<string> PlantingCommonParamenterList()
	{
		List<BuiltInParameter> _BuiltInParameterList = new List<BuiltInParameter>();
		return PlantingCommonParamenterList(ref _BuiltInParameterList);
	}

	private static List<string> PlantingCommonParamenterList(ref List<BuiltInParameter> _BuiltInParameterList)
	{
		List<string> list = new List<string>();
		list.Add(EPShared.LocalRM.GetString("HeightPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.RENDER_PLANT_HEIGHT);
		list.Add(EPShared.LocalRM.GetString("KeynotePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.KEYNOTE_PARAM);
		list.Add(EPShared.LocalRM.GetString("ModelPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.ALL_MODEL_MODEL);
		list.Add(EPShared.LocalRM.GetString("ManufacturerPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.ALL_MODEL_MANUFACTURER);
		list.Add(EPShared.LocalRM.GetString("TypeCommentsPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
		list.Add(EPShared.LocalRM.GetString("URLPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.ALL_MODEL_URL);
		list.Add(EPShared.LocalRM.GetString("DescriptionPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.ALL_MODEL_DESCRIPTION);
		list.Add(EPShared.LocalRM.GetString("CostPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.DOOR_COST);
		list.Add(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		return list;
	}

	public static void GenericModelParamenterList(ref List<FamilyParameterTypeWrapper> _ReturnParameterList)
	{
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("KeynoteGenericFamilyParameter"), RevitParameters.KeynoteGenericFamilyParameter, ParameterType.Text, BuiltInParameter.KEYNOTE_PARAM));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ModelGenericFamilyParameter"), RevitParameters.ModelGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_MODEL));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ManufacturerGenericFamilyParameter"), RevitParameters.ManufacturerGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_MANUFACTURER));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("TypeCommentsGenericFamilyParameter"), RevitParameters.TypeCommentsGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_TYPE_COMMENTS));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("URLGenericFamilyParameter"), RevitParameters.URLGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_URL));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("DescriptionGenericFamilyParameter"), RevitParameters.DescriptionGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_DESCRIPTION));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), RevitParameters.CostGenericFamilyParameter, ParameterType.Text, BuiltInParameter.DOOR_COST));
	}

	private static void GenericModelUndefinedParamenterList(ref List<FamilyParameterTypeWrapper> _ReturnParameterList)
	{
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("RegionNameGenericModelFamilyParameter"), RevitParameters.RegionNameGenericModelFamilyParameter, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("UniqueIDGenericModelFamilyParameter"), RevitParameters.UniqueIDGenericModelFamilyParameter, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("LandcaddIDGenericModelFamilyParameter"), RevitParameters.LandcaddIDGenericModelFamilyParameter, ParameterType.Text));
	}

	public static List<FamilyParameterTypeWrapper> PlantMixParamenterList()
	{
		List<FamilyParameterTypeWrapper> _ReturnParameterList = new List<FamilyParameterTypeWrapper>();
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ScientificNamePlantMixFamilyParameter"), RevitParameters.ScientificNamePlantMixFamilyParameter, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("CommonNamePlantMixFamilyParameter"), RevitParameters.CommonNamePlantMixFamilyParameter, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("CodeNamePlantMixFamilyParameter"), RevitParameters.CodeNamePlantMixFamilyParameter, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("CodeNamePlantMixFamilyParameter"), RevitParameters.CodeNamePlantMixFamilyParameter, ParameterType.Text));
		GenericModelUndefinedParamenterList(ref _ReturnParameterList);
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("PlantSizePlantMixFamilyParameter"), RevitParameters.PlantSizePlantMixFamilyParameter, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SpacingMethodPlantMixFamilyParameter"), RevitParameters.SpacingMethodPlantMixFamilyParameter, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter"), RevitParameters.PlantSpacingPlantMixFamilyParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("PercentagePlantMixFamilyParameter"), RevitParameters.PercentagePlantMixFamilyParameter, ParameterType.Number));
		GenericModelParamenterList(ref _ReturnParameterList);
		return _ReturnParameterList;
	}

	public static List<FamilyParameterTypeWrapper> PatioWalkParamenterList()
	{
		List<FamilyParameterTypeWrapper> _ReturnParameterList = new List<FamilyParameterTypeWrapper>();
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("MaterialPathWalkFamilyParameter"), RevitParameters.MaterialPathWalkFamilyParameter, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("MaterialPathWalkFamilyParameter"), RevitParameters.MaterialPathWalkFamilyParameter, ParameterType.Text));
		GenericModelUndefinedParamenterList(ref _ReturnParameterList);
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ThicknessPathWalkFamilyParameter"), RevitParameters.ThicknessPathWalkFamilyParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("EdgePathWalkFamilyParameter"), RevitParameters.EdgePathWalkFamilyParameter, ParameterType.YesNo));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("MowingStripPathWalkFamilyParameter"), RevitParameters.MowingStripPathWalkFamilyParameter, ParameterType.YesNo));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("EdgeWidthPathWalkFamilyParameter"), RevitParameters.EdgeWidthPathWalkFamilyParameter, ParameterType.Length));
		GenericModelParamenterList(ref _ReturnParameterList);
		return _ReturnParameterList;
	}

	public static List<string> PlantSharedParamenterList()
	{
		List<BuiltInParameter> _BuiltInParameterList = new List<BuiltInParameter>();
		return PlantSharedParamenterList(ref _BuiltInParameterList);
	}

	public static List<string> PlantSharedParamenterList(ref List<BuiltInParameter> _BuiltInParameterList)
	{
		List<string> list = new List<string>();
		list.Add(EPShared.LocalRM.GetString("PlantScientificNamePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantCommonNamePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantCodeNamePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsAdmirationPointPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("TemperatureRangePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("ConditionsSoilConditionPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("ConditionsExposurePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("ConditionsMiscellaneousPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("ConditionsTolerancesPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantTypePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("GrowthGrowthRatePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("GrowthServiceLifePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantHeightPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantWidthPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("GrowthPatternPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("GrowthFormPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("MaintenanceWaterRequirementsPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("MaintenanceTransplantPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("MaintenanceDetrimentsPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("MaintenancePlantingTimePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("MaintenanceTransplantingTimePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("MaintenanceFertilizingTimePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("MaintenancePruningTimePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsFoliageSizePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsFoliageSurfacePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsFoliageColorPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsFallColorPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsCrownDensityPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsBarkSurfacePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsBarkColorPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsFruitColorPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsBloomEffectPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsFragrancePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsBloomPeriodPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("CharacteristicsFruitPeriodPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantUsesPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantClimateAndZonePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantingSizePlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantSpacingPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantCommentsPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantCountPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlantingSymbolPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("GenericModelSymbolPlantFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		list.Add(EPShared.LocalRM.GetString("PlacementIDFamilyParameter"));
		_BuiltInParameterList.Add(BuiltInParameter.INVALID);
		return list;
	}

	public static PlantFieldsComparisionType FindPlantComparisonType(string _strValue)
	{
		PlantFieldsComparisionType result = PlantFieldsComparisionType.Equal;
		if (string.Compare(_strValue, EPShared.LocalRM.GetString("LessThanComparator"), ignoreCase: true) == 0)
		{
			result = PlantFieldsComparisionType.LessThan;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("LessThanOrEqualToComparator"), ignoreCase: true) == 0)
		{
			result = PlantFieldsComparisionType.LessThanOrEqualTo;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("GreaterThanComparator"), ignoreCase: true) == 0)
		{
			result = PlantFieldsComparisionType.GreaterThan;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("GreaterThanOrEqualToComparator"), ignoreCase: true) == 0)
		{
			result = PlantFieldsComparisionType.GreaterThanOrEqualTo;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("NotEqualComparator"), ignoreCase: true) == 0)
		{
			result = PlantFieldsComparisionType.NotEqual;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("StartsWithComparator"), ignoreCase: true) == 0)
		{
			result = PlantFieldsComparisionType.StartsWith;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("EndsWithComparator"), ignoreCase: true) == 0)
		{
			result = PlantFieldsComparisionType.EndsWith;
		}
		else if (string.Compare(_strValue, EPShared.LocalRM.GetString("ContainsComparator"), ignoreCase: true) == 0)
		{
			result = PlantFieldsComparisionType.Contains;
		}
		return result;
	}

	public static List<string> ComparisonList()
	{
		List<string> list = new List<string>();
		list.Add(EPShared.LocalRM.GetString("ContainsComparator"));
		list.Add(EPShared.LocalRM.GetString("EqualComparator"));
		list.Add(EPShared.LocalRM.GetString("LessThanComparator"));
		list.Add(EPShared.LocalRM.GetString("LessThanOrEqualToComparator"));
		list.Add(EPShared.LocalRM.GetString("GreaterThanComparator"));
		list.Add(EPShared.LocalRM.GetString("GreaterThanOrEqualToComparator"));
		list.Add(EPShared.LocalRM.GetString("NotEqualComparator"));
		list.Add(EPShared.LocalRM.GetString("StartsWithComparator"));
		list.Add(EPShared.LocalRM.GetString("EndsWithComparator"));
		return list;
	}

	private static void ReturnFieldValue(string _strCurPlantField, List<KeyValuePair<string, SearchPlantParameter>> _SearchParameterResults, ref List<SearchPlantParameter> _ReturnComparisionList)
	{
		for (int i = 0; i < _SearchParameterResults.Count; i++)
		{
			KeyValuePair<string, SearchPlantParameter> keyValuePair = _SearchParameterResults[i];
			if (string.Compare(keyValuePair.Key, _strCurPlantField, ignoreCase: true) == 0)
			{
				_ReturnComparisionList.Add(keyValuePair.Value);
			}
		}
	}

	private static bool SearchFamilyList(List<KeyValuePair<string, SearchPlantParameter>> _SearchParameterResults, ref List<FamilyTypeContainer> _SearchFamilyTypeList)
	{
		PlantField plantField = PlantField.None;
		bool flag = false;
		double num = 0.0;
		double _dValue = 0.0;
		int num2 = 0;
		int _nValue = 0;
		string empty = string.Empty;
		string _strValue = string.Empty;
		int num3 = 0;
		List<FamilyTypeContainer> list = null;
		if (_SearchFamilyTypeList.Count == 0)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NoPlantsFoundMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return false;
		}
		list = new List<FamilyTypeContainer>();
		foreach (FamilyTypeContainer _SearchFamilyType in _SearchFamilyTypeList)
		{
			FamilyTypeContainer familyTypeContainer = new FamilyTypeContainer(_SearchFamilyType.FamilyTypeWrapper, _SearchFamilyType.FamilyParameterList);
			if (_SearchFamilyType.Status == FamilyParameterWrapperStatus.Selected)
			{
				familyTypeContainer.Status = FamilyParameterWrapperStatus.Selected;
			}
			list.Add(familyTypeContainer);
		}
		num3 = list.Count;
		List<SearchPlantParameter> _ReturnComparisionList = new List<SearchPlantParameter>();
		FamilyTypeContainer familyTypeContainer2 = null;
		for (int i = 0; i < list.Count; i++)
		{
			familyTypeContainer2 = list[i];
			if (familyTypeContainer2.Status != FamilyParameterWrapperStatus.Included)
			{
				continue;
			}
			FamilyParameterWrapper familyParameterWrapper = null;
			for (int j = 0; j < familyTypeContainer2.FamilyParameterList.Count; j++)
			{
				familyParameterWrapper = familyTypeContainer2.FamilyParameterList[j];
				if (familyParameterWrapper == null || GetPlantFieldFromParamenter(familyParameterWrapper.ParameterName) == PlantField.None)
				{
					continue;
				}
				_ReturnComparisionList.Clear();
				ReturnFieldValue(familyParameterWrapper.ParameterName, _SearchParameterResults, ref _ReturnComparisionList);
				flag = true;
				if (!familyTypeContainer2.HasValue(j))
				{
					if (_ReturnComparisionList.Count > 0)
					{
						flag = false;
					}
				}
				else
				{
					switch (familyParameterWrapper.StorageType)
					{
					case StorageType.Double:
						if (!familyTypeContainer2.GetDoubleValue(j, ref _dValue))
						{
							break;
						}
						foreach (SearchPlantParameter item in _ReturnComparisionList)
						{
							if (!(item.Parameter != string.Empty))
							{
								continue;
							}
							num = ValueConversion.ToDouble(item.Parameter);
							switch (item.ComparisionType)
							{
							case PlantFieldsComparisionType.Equal:
								if (_dValue != num)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.NotEqual:
								if (_dValue == num)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.LessThan:
								if (_dValue >= num)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.LessThanOrEqualTo:
								if (_dValue > num)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.GreaterThan:
								if (_dValue <= num)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.GreaterThanOrEqualTo:
								if (_dValue < num)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.StartsWith:
								if (!Convert.ToString(_dValue).StartsWith(item.Parameter, StringComparison.CurrentCultureIgnoreCase))
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.EndsWith:
								if (!Convert.ToString(_dValue).EndsWith(item.Parameter, StringComparison.CurrentCultureIgnoreCase))
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.Contains:
								if (!Convert.ToString(_dValue).Contains(item.Parameter))
								{
									flag = false;
								}
								break;
							}
							if (!flag)
							{
								break;
							}
						}
						break;
					case StorageType.Integer:
						if (!familyTypeContainer2.GetIntegerValue(j, ref _nValue))
						{
							break;
						}
						foreach (SearchPlantParameter item2 in _ReturnComparisionList)
						{
							if (!(item2.Parameter != string.Empty))
							{
								continue;
							}
							num2 = Convert.ToInt32(item2.Parameter);
							switch (item2.ComparisionType)
							{
							case PlantFieldsComparisionType.Equal:
								if (_nValue != num2)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.NotEqual:
								if (_nValue == num2)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.LessThan:
								if (_nValue >= num2)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.LessThanOrEqualTo:
								if (_nValue > num2)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.GreaterThan:
								if (_nValue <= num2)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.GreaterThanOrEqualTo:
								if (_nValue < num2)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.StartsWith:
								if (!Convert.ToString(_nValue).StartsWith(item2.Parameter, StringComparison.CurrentCultureIgnoreCase))
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.EndsWith:
								if (!Convert.ToString(_nValue).EndsWith(item2.Parameter, StringComparison.CurrentCultureIgnoreCase))
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.Contains:
								if (!Convert.ToString(_nValue).Contains(item2.Parameter))
								{
									flag = false;
								}
								break;
							}
							if (!flag)
							{
								break;
							}
						}
						break;
					case StorageType.String:
						if (!familyTypeContainer2.GetStringValue(j, ref _strValue))
						{
							break;
						}
						foreach (SearchPlantParameter item3 in _ReturnComparisionList)
						{
							if (!(item3.Parameter != string.Empty))
							{
								continue;
							}
							empty = item3.Parameter;
							switch (item3.ComparisionType)
							{
							case PlantFieldsComparisionType.Equal:
								if (string.Compare(empty, _strValue, ignoreCase: true) != 0)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.NotEqual:
								if (string.Compare(empty, _strValue, ignoreCase: true) == 0)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.LessThan:
								if (string.Compare(empty, _strValue, ignoreCase: true) != 1)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.LessThanOrEqualTo:
								if (string.Compare(empty, _strValue, ignoreCase: true) == -1)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.GreaterThan:
								if (string.Compare(empty, _strValue, ignoreCase: true) != -1)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.GreaterThanOrEqualTo:
								if (string.Compare(empty, _strValue, ignoreCase: true) == 1)
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.StartsWith:
								if (!_strValue.StartsWith(empty, StringComparison.CurrentCultureIgnoreCase))
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.EndsWith:
								if (!_strValue.EndsWith(empty, StringComparison.CurrentCultureIgnoreCase))
								{
									flag = false;
								}
								break;
							case PlantFieldsComparisionType.Contains:
								if (!_strValue.ToLower().Contains(empty.ToLower()))
								{
									flag = false;
								}
								break;
							}
							if (!flag)
							{
								break;
							}
						}
						break;
					}
				}
				if (!flag)
				{
					if (num3 > 1)
					{
						familyTypeContainer2.Status = FamilyParameterWrapperStatus.Excluded;
						num3--;
						break;
					}
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NoPlantsMatchingCriteriaMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return false;
				}
			}
		}
		_SearchFamilyTypeList.Clear();
		foreach (FamilyTypeContainer item4 in list)
		{
			_SearchFamilyTypeList.Add(item4);
		}
		return true;
	}

	public static bool FindTemplateFileName(Document _Document, ref string _strTemplateFileName, bool _bIssueWarning)
	{
		bool flag = false;
		string empty = string.Empty;
		if (!ApplicationUtility.Settings.eplSetE_GetSetting(empty, ref _strTemplateFileName) || !File.Exists(_strTemplateFileName))
		{
			if (_Document != null)
			{
				string text = string.Empty;
				string text2 = string.Empty;
				IDictionary<string, string> libraryPaths = _Document.Application.GetLibraryPaths();
				if (libraryPaths != null && libraryPaths.Count > 0)
				{
					foreach (string value in libraryPaths.Values)
					{
						DirectoryInfo directoryInfo = new DirectoryInfo(value);
						DirectoryInfo directoryInfo2 = null;
						if (directoryInfo.Parent != null)
						{
							directoryInfo2 = directoryInfo.Parent.Parent;
						}
						if (directoryInfo2 != null)
						{
							if (_Document.DisplayUnitSystem == DisplayUnit.IMPERIAL)
							{
								if (text == string.Empty)
								{
									text = ApplicationUtility.GetString("PlantingImperialTemplate") + ".rft";
								}
								if (text2 == string.Empty)
								{
									text2 = ApplicationUtility.GetString("ENU-ImperialTemplateFolderName");
								}
							}
							else if (_Document.DisplayUnitSystem == DisplayUnit.METRIC)
							{
								if (text == string.Empty)
								{
									text = ApplicationUtility.GetString("PlantingMetricTemplate") + ".rft";
								}
								if (text2 == string.Empty)
								{
									text2 = ApplicationUtility.GetString("ENU-MetricTemplateFolderName");
								}
							}
							if (directoryInfo2.Exists)
							{
								try
								{
									DirectoryInfo[] directories = directoryInfo2.GetDirectories(text2, SearchOption.AllDirectories);
									DirectoryInfo[] array = directories;
									foreach (DirectoryInfo directoryInfo3 in array)
									{
										FileInfo[] files = directoryInfo3.GetFiles(text, SearchOption.AllDirectories);
										FileInfo[] array2 = files;
										foreach (FileInfo fileInfo in array2)
										{
											if (fileInfo.Exists)
											{
												_strTemplateFileName = fileInfo.FullName;
												flag = true;
												break;
											}
										}
										if (flag)
										{
											break;
										}
									}
								}
								catch (UnauthorizedAccessException)
								{
									continue;
								}
								catch (System.IO.DirectoryNotFoundException)
								{
									continue;
								}
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
			}
			if (!flag)
			{
				string empty2 = string.Empty;
				string empty3 = string.Empty;
				int num = -1;
				if (!File.Exists(ApplicationUtility.RevitIniFilename))
				{
					if (_bIssueWarning)
					{
						RevitMessageBox.Show(EPShared.LocalRM.GetString("RevitINIFileMissingMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
					}
					return false;
				}
				Stream stream = new FileStream(ApplicationUtility.RevitIniFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				StreamReader streamReader = new StreamReader(stream, Encoding.ASCII);
				while ((empty2 = streamReader.ReadLine()) != null)
				{
					if (empty2.Contains(ApplicationUtility.GetString("FamilyTemplatePath")))
					{
						string text3 = string.Empty;
						num = empty2.IndexOf('=');
						if (num >= 0 && num < empty2.Length - 1)
						{
							text3 = empty2.Substring(num + 1);
							if (!text3.EndsWith("\\"))
							{
								text3 += "\\";
							}
						}
						if (empty2.Contains(ApplicationUtility.GetString("ENU-ImperialTemplateFolderName")))
						{
							_strTemplateFileName = text3 + ApplicationUtility.GetString("PlantingImperialTemplate");
						}
						else
						{
							_strTemplateFileName = text3 + ApplicationUtility.GetString("PlantingMetricTemplate");
						}
						_strTemplateFileName += ".rft";
						if (File.Exists(_strTemplateFileName))
						{
							flag = true;
							break;
						}
						_strTemplateFileName = string.Empty;
					}
					else
					{
						if (string.Compare(empty2, ApplicationUtility.GetString("ContentInstalledName"), ignoreCase: true) != 0)
						{
							continue;
						}
						while ((empty3 = streamReader.ReadLine()) != null)
						{
							if (!empty3.Contains(ApplicationUtility.GetString("ImperialTemplateCabName")) && !empty3.Contains(ApplicationUtility.GetString("MetricTemplateCabName")))
							{
								continue;
							}
							string text4 = string.Empty;
							num = empty3.IndexOf('=');
							if (num >= 0 && num < empty3.Length - 1)
							{
								text4 = empty3.Substring(num + 1);
								if (!text4.EndsWith("\\"))
								{
									text4 += "\\";
								}
							}
							if (empty3.Contains(ApplicationUtility.GetString("ENU-ImperialTemplateFolderName")))
							{
								_strTemplateFileName = text4 + ApplicationUtility.GetString("PlantingImperialTemplate");
							}
							else
							{
								_strTemplateFileName = text4 + ApplicationUtility.GetString("PlantingMetricTemplate");
							}
							_strTemplateFileName += ".rft";
							if (File.Exists(_strTemplateFileName))
							{
								flag = true;
								break;
							}
							_strTemplateFileName = string.Empty;
						}
						break;
					}
				}
				streamReader.Close();
			}
			if (!flag)
			{
				if (!_bIssueWarning)
				{
					return false;
				}
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoPlantingRFTMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
				using OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Title = EPShared.LocalRM.GetString("TemplateNotLoadedErrorMessage");
				openFileDialog.RestoreDirectory = true;
				openFileDialog.Filter = ApplicationUtility.GetString("TemplateFilePickerFilter");
				DialogResult dialogResult = openFileDialog.ShowDialog();
				if (dialogResult == DialogResult.Cancel)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("TemplateNotPickedFamilyNotCreatedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return false;
				}
				_strTemplateFileName = openFileDialog.FileName;
				flag = true;
			}
			ApplicationUtility.Settings.eplSetE_SetSetting(empty, _strTemplateFileName);
		}
		else
		{
			flag = true;
		}
		return flag;
	}

	public static bool FindMixTemplateFileName(Document _Document, ref string _strTemplateFileName, bool _bIssueWarning)
	{
		bool flag = false;
		string strSettingsName = "GenericTemplateFileName";
		if (!ApplicationUtility.Settings.eplSetE_GetSetting(strSettingsName, ref _strTemplateFileName) || !File.Exists(_strTemplateFileName))
		{
			if (_Document != null)
			{
				string text = string.Empty;
				string text2 = string.Empty;
				IDictionary<string, string> libraryPaths = _Document.Application.GetLibraryPaths();
				if (libraryPaths != null && libraryPaths.Count > 0)
				{
					foreach (string value in libraryPaths.Values)
					{
						DirectoryInfo directoryInfo = new DirectoryInfo(value);
						DirectoryInfo directoryInfo2 = null;
						if (directoryInfo.Parent != null)
						{
							directoryInfo2 = directoryInfo.Parent.Parent;
						}
						if (directoryInfo2 != null)
						{
							if (_Document.DisplayUnitSystem == DisplayUnit.IMPERIAL)
							{
								if (text == string.Empty)
								{
									text = ApplicationUtility.GetString("GenericImperialTemplate") + ".rft";
								}
								if (text2 == string.Empty)
								{
									text2 = ApplicationUtility.GetString("ENU-ImperialTemplateFolderName");
								}
							}
							else if (_Document.DisplayUnitSystem == DisplayUnit.METRIC)
							{
								if (text == string.Empty)
								{
									text = ApplicationUtility.GetString("GenericMetricTemplate") + ".rft";
								}
								if (text2 == string.Empty)
								{
									text2 = ApplicationUtility.GetString("ENU-MetricTemplateFolderName");
								}
							}
							if (directoryInfo2.Exists)
							{
								try
								{
									DirectoryInfo[] directories = directoryInfo2.GetDirectories(text2, SearchOption.AllDirectories);
									DirectoryInfo[] array = directories;
									foreach (DirectoryInfo directoryInfo3 in array)
									{
										FileInfo[] files = directoryInfo3.GetFiles(text, SearchOption.AllDirectories);
										FileInfo[] array2 = files;
										foreach (FileInfo fileInfo in array2)
										{
											if (fileInfo.Exists)
											{
												_strTemplateFileName = fileInfo.FullName;
												flag = true;
												break;
											}
										}
										if (flag)
										{
											break;
										}
									}
								}
								catch (UnauthorizedAccessException)
								{
									continue;
								}
								catch (System.IO.DirectoryNotFoundException)
								{
									continue;
								}
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
			}
			if (!flag)
			{
				string empty = string.Empty;
				string empty2 = string.Empty;
				int num = -1;
				if (!File.Exists(ApplicationUtility.RevitIniFilename))
				{
					if (_bIssueWarning)
					{
						RevitMessageBox.Show(EPShared.LocalRM.GetString("RevitINIFileMissingMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
					}
					return false;
				}
				Stream stream = new FileStream(ApplicationUtility.RevitIniFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				StreamReader streamReader = new StreamReader(stream, Encoding.ASCII);
				while ((empty = streamReader.ReadLine()) != null)
				{
					if (empty.Contains(ApplicationUtility.GetString("FamilyTemplatePath")))
					{
						string text3 = string.Empty;
						num = empty.IndexOf('=');
						if (num >= 0 && num < empty.Length - 1)
						{
							text3 = empty.Substring(num + 1);
							if (!text3.EndsWith("\\"))
							{
								text3 += "\\";
							}
						}
						if (empty.Contains(ApplicationUtility.GetString("ENU-ImperialTemplateFolderName")))
						{
							_strTemplateFileName = text3 + ApplicationUtility.GetString("GenericImperialTemplate");
						}
						else
						{
							_strTemplateFileName = text3 + ApplicationUtility.GetString("GenericMetricTemplate");
						}
						_strTemplateFileName += ".rft";
						if (File.Exists(_strTemplateFileName))
						{
							flag = true;
							break;
						}
						_strTemplateFileName = string.Empty;
					}
					else
					{
						if (string.Compare(empty, ApplicationUtility.GetString("ContentInstalledName"), ignoreCase: true) != 0)
						{
							continue;
						}
						while ((empty2 = streamReader.ReadLine()) != null)
						{
							if (!empty2.Contains(ApplicationUtility.GetString("ImperialTemplateCabName")) && !empty2.Contains(ApplicationUtility.GetString("MetricTemplateCabName")))
							{
								continue;
							}
							string text4 = string.Empty;
							num = empty2.IndexOf('=');
							if (num >= 0 && num < empty2.Length - 1)
							{
								text4 = empty2.Substring(num + 1);
								if (!text4.EndsWith("\\"))
								{
									text4 += "\\";
								}
							}
							if (empty2.Contains(ApplicationUtility.GetString("ENU-ImperialTemplateFolderName")))
							{
								_strTemplateFileName = text4 + ApplicationUtility.GetString("PlantingImperialTemplate");
							}
							else
							{
								_strTemplateFileName = text4 + ApplicationUtility.GetString("PlantingMetricTemplate");
							}
							_strTemplateFileName += ".rft";
							if (File.Exists(_strTemplateFileName))
							{
								flag = true;
								break;
							}
							_strTemplateFileName = string.Empty;
						}
						break;
					}
				}
				streamReader.Close();
			}
			if (!flag)
			{
				if (!_bIssueWarning)
				{
					return false;
				}
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoGenericModelRFTMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
				using OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Title = EPShared.LocalRM.GetString("TemplateNotLoadedErrorMessage");
				openFileDialog.RestoreDirectory = true;
				openFileDialog.Filter = ApplicationUtility.GetString("TemplateFilePickerFilter");
				DialogResult dialogResult = openFileDialog.ShowDialog();
				if (dialogResult == DialogResult.Cancel)
				{
					return false;
				}
				_strTemplateFileName = openFileDialog.FileName;
				flag = true;
			}
			ApplicationUtility.Settings.eplSetE_SetSetting(strSettingsName, _strTemplateFileName);
		}
		else
		{
			flag = true;
		}
		return flag;
	}

	public static bool IsTypeNameDuplicate(List<string> _CurStringList, string _strQueryTypeName)
	{
		foreach (string _CurString in _CurStringList)
		{
			if (string.Compare(_CurString, _strQueryTypeName, ignoreCase: true) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public static string GetUniqueTypeName(List<string> _CurStringList, string _strQueryTypeName)
	{
		foreach (string _CurString in _CurStringList)
		{
			if (string.Compare(_CurString, _strQueryTypeName, ignoreCase: true) != 0)
			{
				continue;
			}
			string text = string.Empty;
			string text2 = string.Empty;
			int num = _strQueryTypeName.LastIndexOf(" ");
			if (num > 0 && num < _strQueryTypeName.Length - 1)
			{
				text = _strQueryTypeName.Substring(0, num);
				int num2 = num + 1;
				while (num < _strQueryTypeName.Length)
				{
					if (char.IsDigit(_strQueryTypeName[num2]))
					{
						text2 += _strQueryTypeName[num2];
						num2++;
						continue;
					}
					text2 = string.Empty;
					break;
				}
			}
			if (text2 == string.Empty)
			{
				text = _strQueryTypeName;
				text2 = "1";
			}
			else
			{
				text2 = Convert.ToString(Convert.ToInt32(text2) + 1);
			}
			return GetUniqueTypeName(_CurStringList, text + " " + text2);
		}
		return _strQueryTypeName;
	}

	private static bool IsTypeNameDuplicate(FamilyTypeSet _CurFamilyTypeSet, string _strQueryTypeName)
	{
		foreach (FamilyType item in _CurFamilyTypeSet)
		{
			if (string.Compare(item.Name, _strQueryTypeName, ignoreCase: true) == 0)
			{
				return true;
			}
		}
		return false;
	}

	private static void DeleteDuplicateTypeName(FamilyManager _CurFamilyManager, string _strQueryTypeName)
	{
		foreach (FamilyType type in _CurFamilyManager.Types)
		{
			if (string.Compare(type.Name, _strQueryTypeName, ignoreCase: true) == 0)
			{
				_CurFamilyManager.CurrentType = type;
				_CurFamilyManager.DeleteCurrentType();
				break;
			}
		}
	}

	private static string GetUniqueTypeName(FamilyTypeSet _CurFamilyTypeSet, string _strQueryTypeName)
	{
		foreach (FamilyType item in _CurFamilyTypeSet)
		{
			if (string.Compare(item.Name, _strQueryTypeName, ignoreCase: true) != 0)
			{
				continue;
			}
			string text = string.Empty;
			string text2 = string.Empty;
			int num = _strQueryTypeName.LastIndexOf(" ");
			if (num > 0 && num < _strQueryTypeName.Length - 1)
			{
				text = _strQueryTypeName.Substring(0, num);
				int num2 = num + 1;
				while (num < _strQueryTypeName.Length)
				{
					if (char.IsDigit(_strQueryTypeName[num2]))
					{
						text2 += _strQueryTypeName[num2];
						num2++;
						continue;
					}
					text2 = string.Empty;
					break;
				}
			}
			if (text2 == string.Empty)
			{
				text = _strQueryTypeName;
				text2 = "1";
			}
			else
			{
				text2 = Convert.ToString(Convert.ToInt32(text2) + 1);
			}
			return GetUniqueTypeName(_CurFamilyTypeSet, text + " " + text2);
		}
		return _strQueryTypeName;
	}

	public static bool CreateNewMixFamilyDocument(Document _ActiveDocument, List<FamilyTypeContainer> _PlantListFamilyTypeList, DialogType _Type, string _strNewFilename)
	{
		double _dValue = 0.0;
		string _strValue = string.Empty;
		int _nValue = 0;
		ElementId _Value = null;
		bool _bIsActiveDocument = false;
		bool _bIsDocumentOpen = false;
		Document _CurDocument = _ActiveDocument;
		EPTransactionContainer _TransactionContainer = null;
		List<FamilyParameter> _NewFamilyParameterList = new List<FamilyParameter>();
		if (!SetupMixFamilyDocument(_ActiveDocument, _strNewFilename, _Type, ref _bIsActiveDocument, ref _bIsDocumentOpen, ref _CurDocument, ref _TransactionContainer, ref _NewFamilyParameterList))
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		FamilyType familyType = null;
		string empty = string.Empty;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = _PlantListFamilyTypeList.Count;
		if (_PlantListFamilyTypeList.Count <= 0)
		{
			progressBarForm.MaxValue = 30;
		}
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressbarCreatingFamilyMessage");
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		for (int i = 0; i < _PlantListFamilyTypeList.Count; i++)
		{
			if (!(_PlantListFamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName != string.Empty))
			{
				continue;
			}
			if (flag2)
			{
				familyType = _CurDocument.FamilyManager.NewType(GetUniqueTypeName(_CurDocument.FamilyManager.Types, _PlantListFamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName));
				progressBarForm.Message = EPShared.LocalRM.GetString("CreatingFamilyTypeMessage") + " " + familyType.Name;
			}
			else if (IsTypeNameDuplicate(_CurDocument.FamilyManager.Types, _PlantListFamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName))
			{
				if (flag3)
				{
					continue;
				}
				flag3 = true;
				DialogResult dialogResult = RevitMessageBox.Show(EPShared.LocalRM.GetString("DuplicateTypeAppendMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				if (dialogResult == DialogResult.Yes)
				{
					flag2 = true;
					familyType = _CurDocument.FamilyManager.NewType(GetUniqueTypeName(_CurDocument.FamilyManager.Types, _PlantListFamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName));
					progressBarForm.Message = EPShared.LocalRM.GetString("CreatingFamilyTypeMessage") + " " + familyType.Name;
				}
			}
			else
			{
				familyType = _CurDocument.FamilyManager.NewType(_PlantListFamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName);
				progressBarForm.Message = EPShared.LocalRM.GetString("CreatingFamilyTypeMessage") + " " + familyType.Name;
			}
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
				for (num2 = 0; num2 < _PlantListFamilyTypeList[i].FamilyParameterList.Count; num2++)
				{
					familyParameterWrapper = _PlantListFamilyTypeList[i].FamilyParameterList[num2];
					if (familyParameterWrapper == null || ((familyParameterWrapper.FamilyParameter == null || familyParameterWrapper.FamilyParameter.Id.Value64() == -1 || !(familyParameterWrapper.FamilyParameter.Id == familyParameter.Id)) && string.Compare(familyParameterWrapper.ParameterName, familyParameter.Definition.Name, ignoreCase: true) != 0) || familyParameterWrapper.StorageType != familyParameter.StorageType)
					{
						continue;
					}
					empty = familyParameter.Formula;
					if (empty != null && !(empty == string.Empty))
					{
						continue;
					}
					if (_PlantListFamilyTypeList[i].HasValue(num2))
					{
						switch (familyParameterWrapper.StorageType)
						{
						case StorageType.Double:
							if (_PlantListFamilyTypeList[i].GetDoubleValue(num2, ref _dValue))
							{
								_CurDocument.FamilyManager.Set(familyParameter, _dValue);
							}
							break;
						case StorageType.Integer:
							if (_PlantListFamilyTypeList[i].GetIntegerValue(num2, ref _nValue))
							{
								_CurDocument.FamilyManager.Set(familyParameter, _nValue);
							}
							break;
						case StorageType.String:
							if (_PlantListFamilyTypeList[i].GetStringValue(num2, ref _strValue))
							{
								if (string.Compare(_PlantListFamilyTypeList[i].FamilyParameterList[num2].ParameterName, RevitParameters.LandcaddIDGenericModelFamilyParameter, ignoreCase: true) == 0)
								{
									_strValue = ((_Type != DialogType.PlantMix) ? Convert.ToInt32(LandcaddID.PatioWalk).ToString("000") : Convert.ToInt32(LandcaddID.PlantMix).ToString("000"));
								}
								_CurDocument.FamilyManager.Set(familyParameter, _strValue);
							}
							break;
						case StorageType.ElementId:
							if (!_PlantListFamilyTypeList[i].GetElementIdValue(num2, ref _Value))
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
						break;
					}
					switch (familyParameter.StorageType)
					{
					case StorageType.Double:
						_CurDocument.FamilyManager.Set(familyParameter, 0.0);
						break;
					case StorageType.Integer:
						_CurDocument.FamilyManager.Set(familyParameter, 0);
						break;
					case StorageType.String:
					{
						string value = string.Empty;
						if (string.Compare(_PlantListFamilyTypeList[i].FamilyParameterList[num2].ParameterName, RevitParameters.LandcaddIDGenericModelFamilyParameter, ignoreCase: true) == 0)
						{
							value = ((_Type != DialogType.PlantMix) ? Convert.ToInt32(LandcaddID.PatioWalk).ToString("000") : Convert.ToInt32(LandcaddID.PlantMix).ToString("000"));
						}
						_CurDocument.FamilyManager.Set(familyParameter, value);
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
				for (num2 = 0; num2 < _PlantListFamilyTypeList[i].FamilyParameterList.Count; num2++)
				{
					familyParameterWrapper = _PlantListFamilyTypeList[i].FamilyParameterList[num2];
					if (familyParameterWrapper == null || ((familyParameterWrapper.FamilyParameter == null || familyParameterWrapper.FamilyParameter.Id.Value64() == -1 || !(familyParameterWrapper.FamilyParameter.Id == familyParameter.Id)) && string.Compare(familyParameterWrapper.ParameterName, familyParameter.Definition.Name, ignoreCase: true) != 0) || familyParameterWrapper.StorageType != familyParameter.StorageType)
					{
						continue;
					}
					empty = familyParameter.Formula;
					if (empty != null && empty != string.Empty)
					{
						if (_PlantListFamilyTypeList[i].HasValue(num2))
						{
							_CurDocument.FamilyManager.SetFormula(familyParameter, empty);
						}
						_PlantListFamilyTypeList[i].FamilyParameterList.RemoveAt(num2);
						_NewFamilyParameterList.RemoveAt(num);
						num--;
						break;
					}
				}
			}
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		_TransactionContainer.EndTransaction();
		_TransactionContainer.Commit();
		if (!_bIsActiveDocument)
		{
			SaveAsOptions saveAsOptions = new SaveAsOptions();
			saveAsOptions.OverwriteExistingFile = true;
			_CurDocument.SaveAs(_strNewFilename, saveAsOptions);
			if (!_bIsDocumentOpen)
			{
				_CurDocument.Close(saveModified: false);
			}
		}
		if (flag)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("GetNewRevitVersionMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
		return true;
	}

	public static bool CreateNewFamilyDocument(Document _ActiveDocument, List<FamilyTypeContainer> _PlantListFamilyTypeList, string _strSymbolFilename, string _strSymbolName, string _strNewFilename)
	{
		double _dValue = 0.0;
		string _strValue = string.Empty;
		int _nValue = 0;
		ElementId value = null;
		bool _bIsActiveDocument = false;
		bool _bIsDocumentOpen = false;
		bool _bCopied = false;
		Document _CurDocument = _ActiveDocument;
		EPTransactionContainer _TransactionContainer = null;
		List<FamilyParameter> _NewFamilyParameterList = new List<FamilyParameter>();
		FamilySymbol _PlantingFamilySymbol = null;
		if (!SetupFamilyDocument(_ActiveDocument, _strSymbolFilename, _strSymbolName, _strNewFilename, ref _bIsActiveDocument, ref _bIsDocumentOpen, ref _CurDocument, ref _TransactionContainer, ref _NewFamilyParameterList, ref _PlantingFamilySymbol, ref _bCopied))
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		FamilyType familyType = null;
		string empty = string.Empty;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = _PlantListFamilyTypeList.Count;
		if (_PlantListFamilyTypeList.Count <= 0)
		{
			progressBarForm.MaxValue = 30;
		}
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressbarCreatingFamilyMessage");
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		string text = EPShared.LocalRM.GetString("CreatingFamilyTypeMessage");
		string strText = EPShared.LocalRM.GetString("DuplicateTypeAppendMessage");
		string strCaption = EPShared.LocalRM.GetString("Warning");
		string strB = EPShared.LocalRM.GetString("PlantHeightPlantFamilyParameter");
		string strB2 = EPShared.LocalRM.GetString("PlantWidthPlantFamilyParameter");
		string strB3 = EPShared.LocalRM.GetString("PlantingSymbolPlantFamilyParameter");
		string strB4 = EPShared.LocalRM.GetString("GenericModelSymbolPlantFamilyParameter");
		if (_bCopied)
		{
			familyType = _CurDocument.FamilyManager.NewType(GetUniqueTypeName(_CurDocument.FamilyManager.Types, "AddedOnlyToDeleteAllTypes"));
			foreach (FamilyType type in _CurDocument.FamilyManager.Types)
			{
				if (string.Compare(type.Name, "AddedOnlyToDeleteAllTypes", ignoreCase: true) != 0)
				{
					_CurDocument.FamilyManager.CurrentType = type;
					_CurDocument.FamilyManager.DeleteCurrentType();
				}
			}
		}
		for (int i = 0; i < _PlantListFamilyTypeList.Count; i++)
		{
			if (!(_PlantListFamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName != string.Empty))
			{
				continue;
			}
			if (flag2)
			{
				familyType = _CurDocument.FamilyManager.NewType(GetUniqueTypeName(_CurDocument.FamilyManager.Types, _PlantListFamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName));
				progressBarForm.Message = text + " " + familyType.Name;
			}
			else if (IsTypeNameDuplicate(_CurDocument.FamilyManager.Types, _PlantListFamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName))
			{
				if (flag3)
				{
					continue;
				}
				flag3 = true;
				DialogResult dialogResult = RevitMessageBox.Show(strText, strCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				if (dialogResult == DialogResult.Yes)
				{
					flag2 = true;
					familyType = _CurDocument.FamilyManager.NewType(GetUniqueTypeName(_CurDocument.FamilyManager.Types, _PlantListFamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName));
					progressBarForm.Message = text + " " + familyType.Name;
				}
			}
			else
			{
				familyType = _CurDocument.FamilyManager.NewType(_PlantListFamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName);
				progressBarForm.Message = text + " " + familyType.Name;
			}
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
				for (num2 = 0; num2 < _PlantListFamilyTypeList[i].FamilyParameterList.Count; num2++)
				{
					familyParameterWrapper = _PlantListFamilyTypeList[i].FamilyParameterList[num2];
					if (familyParameterWrapper == null || ((familyParameterWrapper.FamilyParameter == null || familyParameterWrapper.FamilyParameter.Id.Value64() == -1 || !(familyParameterWrapper.FamilyParameter.Id == familyParameter.Id)) && string.Compare(familyParameterWrapper.ParameterName, familyParameter.Definition.Name, ignoreCase: true) != 0) || familyParameterWrapper.StorageType != familyParameter.StorageType)
					{
						continue;
					}
					if ((string.Compare(familyParameterWrapper.ParameterName, strB3, ignoreCase: true) == 0 || string.Compare(familyParameterWrapper.ParameterName, strB4, ignoreCase: true) == 0) && _PlantingFamilySymbol != null && _PlantingFamilySymbol.Id.Value64() > 0)
					{
						familyParameterWrapper.ElementIdValue = _PlantingFamilySymbol.Id;
					}
					empty = familyParameter.Formula;
					if (empty != null && !(empty == string.Empty))
					{
						continue;
					}
					if (!_PlantListFamilyTypeList[i].HasValue(num2))
					{
						break;
					}
					switch (familyParameterWrapper.StorageType)
					{
					case StorageType.Double:
						if (_PlantListFamilyTypeList[i].GetDoubleValue(num2, ref _dValue))
						{
							if (RevitDefines.LE(_dValue, 0.0) && (string.Compare(familyParameterWrapper.ParameterName, strB, ignoreCase: true) == 0 || string.Compare(familyParameterWrapper.ParameterName, strB2, ignoreCase: true) == 0))
							{
								_CurDocument.FamilyManager.Set(familyParameter, 1.0);
							}
							else
							{
								_CurDocument.FamilyManager.Set(familyParameter, _dValue);
							}
						}
						break;
					case StorageType.Integer:
						if (_PlantListFamilyTypeList[i].GetIntegerValue(num2, ref _nValue))
						{
							_CurDocument.FamilyManager.Set(familyParameter, _nValue);
						}
						break;
					case StorageType.String:
						if (_PlantListFamilyTypeList[i].GetStringValue(num2, ref _strValue))
						{
							_CurDocument.FamilyManager.Set(familyParameter, _strValue);
						}
						break;
					case StorageType.ElementId:
						if (string.Compare(familyParameterWrapper.ParameterName, strB3, ignoreCase: true) == 0)
						{
							break;
						}
						try
						{
							_CurDocument.FamilyManager.Set(familyParameter, value);
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
				for (num2 = 0; num2 < _PlantListFamilyTypeList[i].FamilyParameterList.Count; num2++)
				{
					familyParameterWrapper = _PlantListFamilyTypeList[i].FamilyParameterList[num2];
					if (familyParameterWrapper == null || ((familyParameterWrapper.FamilyParameter == null || familyParameterWrapper.FamilyParameter.Id.Value64() == -1 || !(familyParameterWrapper.FamilyParameter.Id == familyParameter.Id)) && string.Compare(familyParameterWrapper.ParameterName, familyParameter.Definition.Name, ignoreCase: true) != 0) || familyParameterWrapper.StorageType != familyParameter.StorageType)
					{
						continue;
					}
					empty = familyParameter.Formula;
					if (empty != null && empty != string.Empty)
					{
						if (_PlantListFamilyTypeList[i].HasValue(num2))
						{
							_CurDocument.FamilyManager.SetFormula(familyParameter, empty);
						}
						_NewFamilyParameterList.RemoveAt(num);
						num--;
						break;
					}
				}
			}
		}
		if (_bCopied)
		{
			foreach (FamilyType type2 in _CurDocument.FamilyManager.Types)
			{
				if (string.Compare(type2.Name, "AddedOnlyToDeleteAllTypes", ignoreCase: true) == 0)
				{
					_CurDocument.FamilyManager.CurrentType = type2;
					_CurDocument.FamilyManager.DeleteCurrentType();
				}
			}
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		_TransactionContainer.EndTransaction();
		_TransactionContainer.Commit();
		if (!_bIsActiveDocument)
		{
			SaveAsOptions saveAsOptions = new SaveAsOptions();
			saveAsOptions.OverwriteExistingFile = true;
			_CurDocument.SaveAs(_strNewFilename, saveAsOptions);
			if (!_bIsDocumentOpen)
			{
				_CurDocument.Close(saveModified: false);
			}
		}
		if (flag)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("GetNewRevitVersionMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
		return true;
	}

	private static bool SetupMixFamilyDocument(Document _ActiveDocument, string _strNewFilename, DialogType _Type, ref bool _bIsActiveDocument, ref bool _bIsDocumentOpen, ref Document _CurDocument, ref EPTransactionContainer _TransactionContainer, ref List<FamilyParameter> _NewFamilyParameterList)
	{
		_CurDocument = _ActiveDocument;
		if (RevitUtility.CompareFileNames(_ActiveDocument.PathName, _strNewFilename) == 0)
		{
			_bIsActiveDocument = true;
		}
		else if (File.Exists(_strNewFilename))
		{
			_bIsDocumentOpen = IsDocumentOpen(_ActiveDocument.Application, _strNewFilename, ref _CurDocument);
			if (!_bIsDocumentOpen)
			{
				_CurDocument = _ActiveDocument.Application.OpenDocumentFile(_strNewFilename);
			}
		}
		else
		{
			string _strTemplateFileName = string.Empty;
			if (!FindMixTemplateFileName(_CurDocument, ref _strTemplateFileName, _bIssueWarning: true))
			{
				return false;
			}
			_CurDocument = _ActiveDocument.Application.NewFamilyDocument(_strTemplateFileName);
		}
		_TransactionContainer = new EPTransactionContainer(_CurDocument);
		List<FamilyParameterTypeWrapper> list = new List<FamilyParameterTypeWrapper>();
		if (_Type == DialogType.PlantMix)
		{
			list = PlantMixParamenterList();
			_TransactionContainer.Start(EPShared.LocalRM.GetString("CreatePlantMixFamilyTransaction"));
		}
		else
		{
			list = PatioWalkParamenterList();
			_TransactionContainer.Start(EPShared.LocalRM.GetString("CreatePatioWalkFamilyTransaction"));
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
		string uniqueIDGenericModelFamilyParameter = RevitParameters.UniqueIDGenericModelFamilyParameter;
		string regionNameGenericModelFamilyParameter = RevitParameters.RegionNameGenericModelFamilyParameter;
		for (int i = 0; i < list.Count; i++)
		{
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
				if (string.Compare(list[i].ParameterName, uniqueIDGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(list[i].ParameterName, regionNameGenericModelFamilyParameter, ignoreCase: true) == 0)
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

	private static bool SetupFamilyDocument(Document _ActiveDocument, string _strSymbolFilename, string _strSymbolName, string _strNewFilename, ref bool _bIsActiveDocument, ref bool _bIsDocumentOpen, ref Document _CurDocument, ref EPTransactionContainer _TransactionContainer, ref List<FamilyParameter> _NewFamilyParameterList, ref FamilySymbol _PlantingFamilySymbol, ref bool _bCopied)
	{
		_CurDocument = _ActiveDocument;
		bool flag = false;
		if (string.Compare(_strSymbolName, EPShared.LocalRM.GetString("AllSymbolsString"), ignoreCase: true) == 0 || string.Compare(_strSymbolName, EPShared.LocalRM.GetString("AllGeometryStringQuotes"), ignoreCase: true) == 0)
		{
			flag = true;
		}
		if (RevitUtility.CompareFileNames(_ActiveDocument.PathName, _strNewFilename) == 0)
		{
			_bIsActiveDocument = true;
		}
		else
		{
			_bCopied = false;
			if (!File.Exists(_strNewFilename))
			{
				File.Copy(_strSymbolFilename, _strNewFilename);
				_bCopied = true;
			}
			if (File.Exists(_strNewFilename))
			{
				_bIsDocumentOpen = IsDocumentOpen(_ActiveDocument.Application, _strNewFilename, ref _CurDocument);
				if (!_bIsDocumentOpen)
				{
					_CurDocument = _ActiveDocument.Application.OpenDocumentFile(_strNewFilename);
				}
			}
			else
			{
				string _strTemplateFileName = string.Empty;
				if (!FindTemplateFileName(_CurDocument, ref _strTemplateFileName, _bIssueWarning: true))
				{
					return false;
				}
				_CurDocument = _ActiveDocument.Application.NewFamilyDocument(_strTemplateFileName);
			}
		}
		_TransactionContainer = new EPTransactionContainer(_CurDocument);
		_TransactionContainer.Start(EPShared.LocalRM.GetString("CreatePlantFamilyTransaction"));
		_TransactionContainer.BeginTransaction();
		string strPlantingSymbolParameter = EPShared.LocalRM.GetString("PlantingSymbolPlantFamilyParameter");
		if (!_bCopied)
		{
			bool flag2 = false;
			if (RevitUtility.CompareFileNames(_ActiveDocument.PathName, _strSymbolFilename) != 0)
			{
				if (RevitUtility.CompareFileNames(_CurDocument.PathName, _strSymbolFilename) == 0)
				{
					Document CurDocument = null;
					IsDocumentOpen(_ActiveDocument.Application, _strSymbolFilename, ref CurDocument);
					if (CurDocument != null)
					{
						if (flag)
						{
							RevitUtility.LoadFamily(_CurDocument, CurDocument, _bUseFailureHandling: true, ref _TransactionContainer);
						}
						else
						{
							RevitUtility.LoadFamilySymbol(_CurDocument, CurDocument.PathName, _strSymbolName, _bUseFailureHandling: true, ref _TransactionContainer);
						}
					}
					else if (flag)
					{
						flag2 = RevitUtility.LoadFamily(_CurDocument, _strSymbolFilename, _bUseFailureHandling: true, ref _TransactionContainer);
					}
					else
					{
						flag2 = RevitUtility.LoadFamilySymbol(_CurDocument, _strSymbolFilename, _strSymbolName, _bUseFailureHandling: true, ref _TransactionContainer);
					}
				}
				else if (flag)
				{
					flag2 = RevitUtility.LoadFamily(_CurDocument, _strSymbolFilename, _bUseFailureHandling: true, ref _TransactionContainer);
				}
				else
				{
					flag2 = RevitUtility.LoadFamilySymbol(_CurDocument, _strSymbolFilename, _strSymbolName, _bUseFailureHandling: true, ref _TransactionContainer);
				}
			}
		}
		FileInfo fileInfo = null;
		if (!string.IsNullOrEmpty(_ActiveDocument.Application.SharedParametersFilename))
		{
			fileInfo = new FileInfo(_ActiveDocument.Application.SharedParametersFilename);
		}
		List<ExternalDefinitionContainer> list = null;
		int num = 0;
		if (fileInfo != null && fileInfo.Exists)
		{
			DefinitionFile definitionFile = _ActiveDocument.Application.OpenSharedParameterFile();
			DefinitionGroups groups = definitionFile.Groups;
			DefinitionGroup definitionGroup = groups.get_Item(EPShared.LocalRM.GetString("PlantingDefinitionGroup"));
			if (definitionGroup == null)
			{
				return false;
			}
			List<Definition> definitions = new List<Definition>();
			list = PopulateExternalDefinitionList(definitions, GetParameterList("PlantFields", _bPopulateDefaultParameters: false));
			num = list.Count;
		}
		ParameterType parameterType = ParameterType.Text;
		BuiltInParameterGroup builtInParameterGroup = BuiltInParameterGroup.INVALID;
		bool flag3 = false;
		List<BuiltInParameter> _BuiltInParameterList = new List<BuiltInParameter>();
		List<string> list2 = PlantingCommonParamenterList(ref _BuiltInParameterList);
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = list2.Count + num;
		if (list2.Count + num <= 0)
		{
			progressBarForm.MaxValue = 100;
		}
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressbarSettingParametersMessage");
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		string text = EPShared.LocalRM.GetString("PlantHeightPlantFamilyParameter");
		string text2 = EPShared.LocalRM.GetString("PlantWidthPlantFamilyParameter");
		string strB = EPShared.LocalRM.GetString("PlantCountPlantFamilyParameter");
		string strB2 = EPShared.LocalRM.GetString("PlantScientificNamePlantFamilyParameter");
		string strB3 = EPShared.LocalRM.GetString("PlantTypePlantFamilyParameter");
		string strB4 = EPShared.LocalRM.GetString("PlantCommonNamePlantFamilyParameter");
		string strB5 = EPShared.LocalRM.GetString("PlantCodeNamePlantFamilyParameter");
		string strGenericPlantingSymbolParameter = EPShared.LocalRM.GetString("GenericModelSymbolPlantFamilyParameter");
		string strB6 = EPShared.LocalRM.GetString("PlacementIDFamilyParameter");
		string strB7 = EPShared.LocalRM.GetString("CharacteristicsFoliageSizePlantFamilyParameter");
		string strB8 = EPShared.LocalRM.GetString("PlantSpacingPlantFamilyParameter");
		string name = EPShared.LocalRM.GetString("LandcaddIDGenericModelFamilyParameter");
		string name2 = EPShared.LocalRM.GetString("LabelFamilyInstanceParameter");
		for (int i = 0; i < list2.Count; i++)
		{
			FamilyParameter familyParameter = null;
			if (_BuiltInParameterList[i] != BuiltInParameter.INVALID)
			{
				familyParameter = _CurDocument.FamilyManager.get_Parameter(_BuiltInParameterList[i]);
			}
			if (familyParameter == null)
			{
				familyParameter = _CurDocument.FamilyManager.get_Parameter(list2[i]);
			}
			if (familyParameter != null)
			{
				_NewFamilyParameterList.Add(familyParameter);
			}
			else
			{
				try
				{
					parameterType = ((string.Compare(list2[i], text, ignoreCase: true) != 0) ? ParameterType.Text : ParameterType.Length);
					builtInParameterGroup = BuiltInParameterGroup.PG_IDENTITY_DATA;
					flag3 = false;
					_NewFamilyParameterList.Add(_CurDocument.FamilyManager.AddParameter(list2[i], builtInParameterGroup, parameterType, flag3));
				}
				catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
				{
					if (string.Compare(ex.Message, EPShared.LocalRM.GetString("FailedCreatingSharedParameterMessage"), ignoreCase: true) == 0)
					{
						continue;
					}
				}
			}
			progressBarForm.UpdateProgressBar(1);
		}
		FamilyParameter familyParameter2 = null;
		FamilyParameter familyParameter3 = null;
		bool flag4 = false;
		bool flag5 = false;
		for (int j = 0; j < num; j++)
		{
			parameterType = ParameterType.Text;
			if (string.Compare(list[j].Name, text2, ignoreCase: true) == 0 || string.Compare(list[j].Name, text, ignoreCase: true) == 0)
			{
				parameterType = ParameterType.Length;
				builtInParameterGroup = BuiltInParameterGroup.PG_GEOMETRY;
				flag3 = true;
			}
			else if (string.Compare(list[j].Name, strB, ignoreCase: true) == 0)
			{
				parameterType = ParameterType.Integer;
				builtInParameterGroup = BuiltInParameterGroup.PG_IDENTITY_DATA;
				flag3 = true;
			}
			else if (string.Compare(list[j].Name, strB2, ignoreCase: true) == 0 || string.Compare(list[j].Name, strB3, ignoreCase: true) == 0 || string.Compare(list[j].Name, strB4, ignoreCase: true) == 0 || string.Compare(list[j].Name, strB5, ignoreCase: true) == 0)
			{
				builtInParameterGroup = BuiltInParameterGroup.PG_IDENTITY_DATA;
				flag3 = false;
			}
			else if (string.Compare(list[j].Name, strB7, ignoreCase: true) == 0 || string.Compare(list[j].Name, strB8, ignoreCase: true) == 0)
			{
				parameterType = ParameterType.Length;
				builtInParameterGroup = BuiltInParameterGroup.INVALID;
				flag3 = false;
			}
			else if (string.Compare(list[j].Name, strB6, ignoreCase: true) == 0)
			{
				builtInParameterGroup = BuiltInParameterGroup.INVALID;
				flag3 = true;
			}
			else
			{
				if (string.Compare(list[j].Name, strPlantingSymbolParameter, ignoreCase: true) == 0)
				{
					flag4 = true;
				}
				else if (string.Compare(list[j].Name, strGenericPlantingSymbolParameter, ignoreCase: true) == 0)
				{
					flag5 = true;
				}
				builtInParameterGroup = BuiltInParameterGroup.INVALID;
				flag3 = false;
			}
			FamilyParameter familyParameter4 = _CurDocument.FamilyManager.get_Parameter(list[j].Name);
			if (familyParameter4 != null)
			{
				_NewFamilyParameterList.Add(familyParameter4);
			}
			else
			{
				try
				{
					if (list[j].ExternalDefinition != null)
					{
						familyParameter4 = _CurDocument.FamilyManager.AddParameter(list[j].ExternalDefinition, builtInParameterGroup, flag3);
						_NewFamilyParameterList.Add(familyParameter4);
					}
					else
					{
						familyParameter4 = _CurDocument.FamilyManager.AddParameter(list[j].Name, builtInParameterGroup, parameterType, flag3);
						_NewFamilyParameterList.Add(familyParameter4);
					}
				}
				catch (Autodesk.Revit.Exceptions.InvalidOperationException ex2)
				{
					if (string.Compare(ex2.Message, "Shared parameter creation failed.", ignoreCase: true) == 0)
					{
						if (string.Compare(list[j].Name, strPlantingSymbolParameter, ignoreCase: true) == 0)
						{
							flag4 = false;
						}
						else if (string.Compare(list[j].Name, strGenericPlantingSymbolParameter, ignoreCase: true) == 0)
						{
							flag5 = false;
						}
						continue;
					}
				}
			}
			if (string.Compare(list[j].Name, text, ignoreCase: true) == 0)
			{
				familyParameter2 = familyParameter4;
			}
			else if (string.Compare(list[j].Name, text2, ignoreCase: true) == 0)
			{
				familyParameter3 = familyParameter4;
			}
			progressBarForm.UpdateProgressBar(1);
		}
		FamilyParameterComparer comparer = new FamilyParameterComparer();
		_NewFamilyParameterList.Sort(comparer);
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_CurDocument);
		IList<Element> list3 = filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Planting).ToElements();
		bool flag6 = false;
		FamilyInstance familyInstance = null;
		foreach (Element item in list3)
		{
			if (!(item is FamilyInstance { Symbol: not null } familyInstance2) || familyInstance2.Symbol.Category == null || familyInstance2.Symbol.Category.Id.Value64() != BuiltInCategory.OST_Planting.GetHashCode())
			{
				continue;
			}
			familyInstance = familyInstance2;
			if (!flag && string.Compare(_strSymbolName, familyInstance2.Symbol.Name, ignoreCase: true) != 0)
			{
				continue;
			}
			flag6 = true;
			if (familyParameter2 != null)
			{
				Parameter parameter = familyInstance2.LookupParameter(text);
				if (parameter != null)
				{
					_CurDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter, familyParameter2);
				}
			}
			if (familyParameter3 != null)
			{
				Parameter parameter2 = familyInstance2.LookupParameter(text2);
				if (parameter2 != null)
				{
					_CurDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter2, familyParameter3);
				}
			}
			break;
		}
		if (familyInstance == null)
		{
			filteredElementCollector = new FilteredElementCollector(_CurDocument);
			list3 = filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
			bool flag7 = false;
			Parameter parameter3 = null;
			foreach (Element item2 in list3)
			{
				if (!(item2 is FamilyInstance { Symbol: not null } familyInstance3) || familyInstance3.Symbol.Category == null || familyInstance3.Symbol.Category.Id.Value64() != BuiltInCategory.OST_GenericModel.GetHashCode())
				{
					continue;
				}
				parameter3 = familyInstance3.Symbol.LookupParameter(name);
				if (parameter3 == null)
				{
					continue;
				}
				if (string.Compare(parameter3.AsString(), Convert.ToInt32(LandcaddID.Planting).ToString("000"), ignoreCase: true) == 0)
				{
					flag7 = true;
				}
				familyInstance = familyInstance3;
				if (!flag && string.Compare(_strSymbolName, familyInstance3.Symbol.Name, ignoreCase: true) != 0)
				{
					continue;
				}
				flag6 = true;
				if (familyParameter2 != null)
				{
					Parameter parameter4 = familyInstance3.LookupParameter(text);
					if (parameter4 != null)
					{
						_CurDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter4, familyParameter2);
					}
				}
				if (familyParameter3 != null)
				{
					Parameter parameter5 = familyInstance3.LookupParameter(text2);
					if (parameter5 != null)
					{
						_CurDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter5, familyParameter3);
					}
				}
				break;
			}
			if (familyInstance != null && !flag7)
			{
				familyInstance = null;
			}
		}
		filteredElementCollector = new FilteredElementCollector(_CurDocument);
		list3 = filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Planting).ToElements();
		foreach (Element item3 in list3)
		{
			if (!(item3 is FamilySymbol { Category: not null } familySymbol) || familySymbol.Category.Id.Value64() != BuiltInCategory.OST_Planting.GetHashCode())
			{
				continue;
			}
			if (flag || string.Compare(_strSymbolName, familySymbol.Name, ignoreCase: true) == 0)
			{
				if (_PlantingFamilySymbol == null)
				{
					_PlantingFamilySymbol = familySymbol;
				}
				if (familyParameter2 != null)
				{
					Parameter parameter6 = familySymbol.LookupParameter(text);
					if (parameter6 != null)
					{
						_CurDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter6, familyParameter2);
					}
				}
				if (familyParameter3 != null)
				{
					Parameter parameter7 = familySymbol.LookupParameter(text2);
					if (parameter7 != null)
					{
						_CurDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter7, familyParameter3);
					}
				}
			}
			else if (!flag4)
			{
				_CurDocument.Delete(familySymbol.Id);
			}
		}
		if (_PlantingFamilySymbol == null)
		{
			filteredElementCollector = new FilteredElementCollector(_CurDocument);
			list3 = filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
			Parameter parameter8 = null;
			foreach (Element item4 in list3)
			{
				if (!(item4 is FamilySymbol { Category: not null } familySymbol2) || familySymbol2.Category.Id.Value64() != BuiltInCategory.OST_GenericModel.GetHashCode())
				{
					continue;
				}
				parameter8 = familySymbol2.LookupParameter(name);
				if (parameter8 == null || string.Compare(parameter8.AsString(), Convert.ToInt32(LandcaddID.Planting).ToString("000"), ignoreCase: true) != 0)
				{
					continue;
				}
				if (flag || string.Compare(_strSymbolName, familySymbol2.Name, ignoreCase: true) == 0)
				{
					if (_PlantingFamilySymbol == null)
					{
						_PlantingFamilySymbol = familySymbol2;
					}
					if (familyParameter2 != null)
					{
						Parameter parameter9 = familySymbol2.LookupParameter(text);
						if (parameter9 != null)
						{
							_CurDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter9, familyParameter2);
						}
					}
					if (familyParameter3 != null)
					{
						Parameter parameter10 = familySymbol2.LookupParameter(text2);
						if (parameter10 != null)
						{
							_CurDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter10, familyParameter3);
						}
					}
				}
				else if (!flag5)
				{
					_CurDocument.Delete(familySymbol2.Id);
				}
			}
		}
		if (_PlantingFamilySymbol != null)
		{
			if (familyInstance == null)
			{
				if (!_PlantingFamilySymbol.IsActive)
				{
					_PlantingFamilySymbol.Activate();
				}
				XYZ location = new XYZ(0.0, 0.0, 0.0);
				familyInstance = _CurDocument.FamilyCreate.NewFamilyInstance(location, _PlantingFamilySymbol, StructuralType.NonStructural);
			}
			else
			{
				familyInstance.Symbol = _PlantingFamilySymbol;
			}
			Parameter parameter11 = familyInstance.LookupParameter(name2);
			if (parameter11 != null)
			{
				Element element = null;
				if (flag4)
				{
					filteredElementCollector = new FilteredElementCollector(_CurDocument);
					IEnumerable<Element> source = from CurrentElement in filteredElementCollector.WhereElementIsNotElementType()
						where CurrentElement.Name == strPlantingSymbolParameter
						select CurrentElement;
					list3 = source.ToList();
					foreach (Element item5 in list3)
					{
						element = item5;
						if (element == null || string.Compare(element.Name, strPlantingSymbolParameter, ignoreCase: true) != 0)
						{
							continue;
						}
						ElementId id = element.Id;
						try
						{
							if (parameter11.Set(id))
							{
								break;
							}
						}
						catch (SEHException)
						{
						}
					}
				}
				else if (flag5)
				{
					filteredElementCollector = new FilteredElementCollector(_CurDocument);
					IEnumerable<Element> source2 = from CurrentElement in filteredElementCollector.WhereElementIsNotElementType()
						where CurrentElement.Name == strGenericPlantingSymbolParameter
						select CurrentElement;
					list3 = source2.ToList();
					foreach (Element item6 in list3)
					{
						element = item6;
						if (element == null || string.Compare(element.Name, strGenericPlantingSymbolParameter, ignoreCase: true) != 0)
						{
							continue;
						}
						ElementId id = element.Id;
						try
						{
							if (parameter11.Set(id))
							{
								break;
							}
						}
						catch (SEHException)
						{
						}
					}
				}
			}
			if (!flag6)
			{
				if (familyParameter2 != null)
				{
					Parameter parameter12 = familyInstance.LookupParameter(text);
					if (parameter12 != null && _CurDocument.FamilyManager.CanElementParameterBeAssociated(parameter12))
					{
						_CurDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter12, familyParameter2);
					}
				}
				if (familyParameter3 != null)
				{
					Parameter parameter13 = familyInstance.LookupParameter(text2);
					if (parameter13 != null && _CurDocument.FamilyManager.CanElementParameterBeAssociated(parameter13))
					{
						_CurDocument.FamilyManager.AssociateElementParameterToFamilyParameter(parameter13, familyParameter3);
					}
				}
			}
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		return true;
	}

	public static string GetParameterInformation(Element _Element, Document _CurDocument)
	{
		string text = "";
		foreach (Parameter parameter in _Element.Parameters)
		{
			text += parameter.Definition.Name;
			switch (parameter.StorageType)
			{
			case StorageType.Double:
				text = text + " (Double):\t" + parameter.AsValueString();
				break;
			case StorageType.ElementId:
			{
				text += " (ElementId):\t";
				ElementId elementId = parameter.AsElementId();
				text = ((elementId.Value64() < 0) ? (text + elementId.Value64()) : (text + _CurDocument.GetElement(elementId).Name));
				break;
			}
			case StorageType.Integer:
				text += " (Integer):\t";
				if (ParameterType.YesNo != parameter.Definition.LegacyParameterType())
				{
					text += parameter.AsInteger();
				}
				else
				{
					text = ((parameter.AsInteger() != 0) ? (text + "True") : (text + "False"));
				}
				break;
			case StorageType.String:
				text = text + " (String):\t" + parameter.AsString();
				break;
			default:
				text += " :\tUnexposed parameter.";
				break;
			}
			text += "\n";
		}
		return text;
	}

	public static List<ExternalDefinitionContainer> PopulateExternalDefinitionList(List<Definition> _Definitions)
	{
		return PopulateExternalDefinitionList(_Definitions, new List<ParameterColumn>());
	}

	public static List<ExternalDefinitionContainer> PopulateExternalDefinitionList(List<Definition> _Definitions, List<ParameterColumn> _SettingParameterList)
	{
		bool flag = false;
		int num = -1;
		string strA = EPShared.LocalRM.GetString("PlantCountPlantFamilyParameter");
		string strA2 = EPShared.LocalRM.GetString("PlantingSymbolPlantFamilyParameter");
		string strA3 = EPShared.LocalRM.GetString("GenericModelSymbolPlantFamilyParameter");
		string strA4 = EPShared.LocalRM.GetString("PlacementIDFamilyParameter");
		List<ExternalDefinitionContainer> list = new List<ExternalDefinitionContainer>();
		string empty = string.Empty;
		foreach (string item in PlantSharedParamenterList())
		{
			if (string.Compare(strA, item, ignoreCase: true) == 0 || string.Compare(strA2, item, ignoreCase: true) == 0 || string.Compare(strA3, item, ignoreCase: true) == 0)
			{
				flag = true;
			}
			else if (string.Compare(strA4, item, ignoreCase: true) == 0)
			{
				flag = true;
			}
			else
			{
				flag = false;
				for (num = 0; num < _SettingParameterList.Count; num++)
				{
					if (string.Compare(GetPlantParamenterName(_SettingParameterList[num].PlantField), item, ignoreCase: true) == 0)
					{
						flag = true;
						break;
					}
				}
			}
			if (_SettingParameterList.Count != 0 && !flag)
			{
				continue;
			}
			ExternalDefinition externalDefinition = null;
			foreach (Definition _Definition in _Definitions)
			{
				if (_Definition.Name == item)
				{
					externalDefinition = _Definition as ExternalDefinition;
					break;
				}
			}
			if (externalDefinition != null)
			{
				list.Add(new ExternalDefinitionContainer(externalDefinition));
			}
			else if (string.Compare(strA2, item, ignoreCase: true) != 0 && string.Compare(strA3, item, ignoreCase: true) != 0)
			{
				list.Add(new ExternalDefinitionContainer(item));
			}
			else if (string.Compare(strA4, item, ignoreCase: true) != 0)
			{
				list.Add(new ExternalDefinitionContainer(item));
			}
		}
		return list;
	}

	public static bool IsDocumentOpen(Autodesk.Revit.ApplicationServices.Application _CurApplication, string _strFilename, ref Document CurDocument)
	{
		bool result = false;
		foreach (Document document in _CurApplication.Documents)
		{
			if (RevitUtility.CompareFileNames(document.PathName, _strFilename) == 0)
			{
				CurDocument = document;
				result = true;
				break;
			}
		}
		return result;
	}

	public static List<FamilyParameterWrapper> GetFamilyParameterSetForType(FamilyType _FamilyType, FamilyManager _FamilyManager, List<ExternalDefinitionContainer> _ExternalDefinitionList)
	{
		List<FamilyParameterWrapper> list = new List<FamilyParameterWrapper>();
		_FamilyManager.CurrentType = _FamilyType;
		List<string> list2 = PlantingCommonParamenterList();
		for (int i = 0; i < list2.Count; i++)
		{
			FamilyParameterWrapper item = new FamilyParameterWrapper(list2[i], _FamilyManager.get_Parameter(list2[i]), list2[i]);
			list.Add(item);
		}
		string strB = EPShared.LocalRM.GetString("PlantingSymbolPlantFamilyParameter");
		string strB2 = EPShared.LocalRM.GetString("GenericModelSymbolPlantFamilyParameter");
		FamilyParameter familyParameter = null;
		for (int j = 0; j < _ExternalDefinitionList.Count; j++)
		{
			if (_ExternalDefinitionList[j].ExternalDefinition != null)
			{
				familyParameter = _FamilyManager.get_Parameter((Definition)_ExternalDefinitionList[j].ExternalDefinition);
			}
			if (familyParameter == null)
			{
				familyParameter = _FamilyManager.get_Parameter(_ExternalDefinitionList[j].Name);
			}
			FamilyParameterWrapper familyParameterWrapper = new FamilyParameterWrapper(_ExternalDefinitionList[j].Name, familyParameter, _ExternalDefinitionList[j].Name);
			if (string.Compare(_ExternalDefinitionList[j].Name, strB, ignoreCase: true) == 0 || string.Compare(_ExternalDefinitionList[j].Name, strB2, ignoreCase: true) == 0)
			{
				familyParameterWrapper.StorageType = StorageType.ElementId;
			}
			list.Add(familyParameterWrapper);
		}
		FamilyParameterWrapperComparer comparer = new FamilyParameterWrapperComparer();
		list.Sort(comparer);
		return list;
	}

	public static List<ParameterColumn> GetParameterList(string _strSettingName)
	{
		return GetParameterList(_strSettingName, _bPopulateDefaultParameters: true);
	}

	public static List<ParameterColumn> GetParameterList(string _strSettingName, bool _bPopulateDefaultParameters)
	{
		int _nReturnValue = 0;
		List<ParameterColumn> list = new List<ParameterColumn>();
		string text = EPShared.LocalRM.GetString("ColumnOrderString");
		foreach (int value in Enum.GetValues(typeof(PlantField)))
		{
			if ((PlantField)Enum.ToObject(typeof(PlantField), value) != PlantField.None && ApplicationUtility.Settings.eplSetE_GetSetting(_strSettingName + Enum.GetName(typeof(PlantField), value) + text, ref _nReturnValue) && _nReturnValue > 0)
			{
				list.Add(new ParameterColumn((PlantField)Enum.ToObject(typeof(PlantField), value), _nReturnValue));
			}
		}
		if (list.Count > 0)
		{
			ParameterColumnComparer comparer = new ParameterColumnComparer();
			list.Sort(comparer);
		}
		else if (_bPopulateDefaultParameters)
		{
			if (string.Compare(_strSettingName, "PlantFields", ignoreCase: true) == 0)
			{
				List<string> list2 = PlantSharedParamenterList();
				for (int i = 0; i < list2.Count; i++)
				{
					list.Add(new ParameterColumn(GetPlantFieldFromParamenter(list2[i]), i + 1));
				}
			}
			else
			{
				list.Add(new ParameterColumn(PlantField.PlantCommonName, 1));
				list.Add(new ParameterColumn(PlantField.PlantScientificName, 2));
				list.Add(new ParameterColumn(PlantField.PlantType, 3));
				list.Add(new ParameterColumn(PlantField.PlantHeight, 4));
				list.Add(new ParameterColumn(PlantField.PlantWidth, 5));
			}
		}
		return list;
	}

	public static List<FamilyParameterWrapper> GetPlantMixFamilyParameterSetForType(FamilyType _FamilyType, FamilyManager _FamilyManager)
	{
		List<FamilyParameterWrapper> _ReturnFamilyParameterSet = new List<FamilyParameterWrapper>();
		_FamilyManager.CurrentType = _FamilyType;
		FamilyParameterWrapper item = ((_FamilyManager.get_Parameter(RevitParameters.TypeNamePlantFamilyType) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), RevitParameters.TypeNamePlantFamilyType, _FamilyType.Name, StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), _FamilyManager.get_Parameter(RevitParameters.TypeNamePlantFamilyType), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CommonNamePlantMixFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CommonNamePlantMixFamilyParameter"), RevitParameters.CommonNamePlantMixFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CommonNamePlantMixFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.CommonNamePlantMixFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.ScientificNamePlantMixFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("ScientificNamePlantMixFamilyParameter"), RevitParameters.ScientificNamePlantMixFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("ScientificNamePlantMixFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.ScientificNamePlantMixFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.RegionNameGenericModelFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("RegionNameGenericModelFamilyParameter"), RevitParameters.RegionNameGenericModelFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("RegionNameGenericModelFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.RegionNameGenericModelFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.UniqueIDGenericModelFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("UniqueIDGenericModelFamilyParameter"), RevitParameters.UniqueIDGenericModelFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("UniqueIDGenericModelFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.UniqueIDGenericModelFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.LandcaddIDGenericModelFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("LandcaddIDGenericModelFamilyParameter"), RevitParameters.LandcaddIDGenericModelFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("LandcaddIDGenericModelFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.LandcaddIDGenericModelFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CodeNamePlantMixFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CodeNamePlantMixFamilyParameter"), RevitParameters.CodeNamePlantMixFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CodeNamePlantMixFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.CodeNamePlantMixFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.PlantSizePlantMixFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("PlantSizePlantMixFamilyParameter"), RevitParameters.PlantSizePlantMixFamilyParameter, "1", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("PlantSizePlantMixFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.PlantSizePlantMixFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.SpacingMethodPlantMixFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SpacingMethodPlantMixFamilyParameter"), RevitParameters.SpacingMethodPlantMixFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SpacingMethodPlantMixFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.SpacingMethodPlantMixFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.PlantSpacingPlantMixFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter"), RevitParameters.PlantSpacingPlantMixFamilyParameter, "1.0", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.PlantSpacingPlantMixFamilyParameter), EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter")));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.PercentagePlantMixFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("PercentagePlantMixFamilyParameter"), RevitParameters.PercentagePlantMixFamilyParameter, "1.0", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("PercentagePlantMixFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.PercentagePlantMixFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		GetGenericModelFamilyParameterSetForType(_FamilyType, _FamilyManager, ref _ReturnFamilyParameterSet);
		return _ReturnFamilyParameterSet;
	}

	public static List<FamilyParameterWrapper> GetPatioWalkFamilyParameterSetForType(FamilyType _FamilyType, FamilyManager _FamilyManager)
	{
		List<FamilyParameterWrapper> _ReturnFamilyParameterSet = new List<FamilyParameterWrapper>();
		_FamilyManager.CurrentType = _FamilyType;
		FamilyParameterWrapper item = ((_FamilyManager.get_Parameter(RevitParameters.TypeNamePlantFamilyType) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), RevitParameters.TypeNamePlantFamilyType, _FamilyType.Name, StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), _FamilyManager.get_Parameter(RevitParameters.TypeNamePlantFamilyType), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.RegionNameGenericModelFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("RegionNameGenericModelFamilyParameter"), RevitParameters.RegionNameGenericModelFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("RegionNameGenericModelFamilyParameter"), _FamilyManager.get_Parameter(EPShared.LocalRM.GetString("RegionNameGenericModelFamilyParameter")), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.UniqueIDGenericModelFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("UniqueIDGenericModelFamilyParameter"), RevitParameters.UniqueIDGenericModelFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("UniqueIDGenericModelFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.UniqueIDGenericModelFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.LandcaddIDGenericModelFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("LandcaddIDGenericModelFamilyParameter"), RevitParameters.LandcaddIDGenericModelFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("LandcaddIDGenericModelFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.LandcaddIDGenericModelFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.MaterialPathWalkFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("MaterialPathWalkFamilyParameter"), RevitParameters.MaterialPathWalkFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("MaterialPathWalkFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.MaterialPathWalkFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.ThicknessPathWalkFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("ThicknessPathWalkFamilyParameter"), RevitParameters.ThicknessPathWalkFamilyParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("ThicknessPathWalkFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.ThicknessPathWalkFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.EdgePathWalkFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("EdgePathWalkFamilyParameter"), RevitParameters.EdgePathWalkFamilyParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("EdgePathWalkFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.EdgePathWalkFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.MowingStripPathWalkFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("MowingStripPathWalkFamilyParameter"), RevitParameters.MowingStripPathWalkFamilyParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("MowingStripPathWalkFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.MowingStripPathWalkFamilyParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.EdgeWidthPathWalkFamilyParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("EdgeWidthPathWalkFamilyParameter"), RevitParameters.EdgeWidthPathWalkFamilyParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("EdgeWidthPathWalkFamilyParameter"), _FamilyManager.get_Parameter(RevitParameters.EdgeWidthPathWalkFamilyParameter), EPShared.LocalRM.GetString("EdgeWidthPathWalkFamilyParameter")));
		_ReturnFamilyParameterSet.Add(item);
		GetGenericModelFamilyParameterSetForType(_FamilyType, _FamilyManager, ref _ReturnFamilyParameterSet);
		return _ReturnFamilyParameterSet;
	}

	public static void GetGenericModelFamilyParameterSetForType(FamilyType _FamilyType, FamilyManager _FamilyManager, ref List<FamilyParameterWrapper> _ReturnFamilyParameterSet)
	{
		_FamilyManager.CurrentType = _FamilyType;
		FamilyParameterWrapper familyParameterWrapper = null;
		FamilyParameter familyParameter = null;
		familyParameter = _FamilyManager.get_Parameter(BuiltInParameter.KEYNOTE_PARAM);
		if (familyParameter == null)
		{
			familyParameter = _FamilyManager.get_Parameter(RevitParameters.KeynoteGenericFamilyParameter);
		}
		familyParameterWrapper = ((familyParameter == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("KeynoteGenericFamilyParameter"), RevitParameters.KeynoteGenericFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("KeynoteGenericFamilyParameter"), familyParameter, ""));
		_ReturnFamilyParameterSet.Add(familyParameterWrapper);
		familyParameter = _FamilyManager.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL);
		if (familyParameter == null)
		{
			familyParameter = _FamilyManager.get_Parameter(RevitParameters.ModelGenericFamilyParameter);
		}
		familyParameterWrapper = ((familyParameter == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("ModelGenericFamilyParameter"), RevitParameters.ModelGenericFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("ModelGenericFamilyParameter"), familyParameter, ""));
		_ReturnFamilyParameterSet.Add(familyParameterWrapper);
		familyParameter = _FamilyManager.get_Parameter(BuiltInParameter.ALL_MODEL_MANUFACTURER);
		if (familyParameter == null)
		{
			familyParameter = _FamilyManager.get_Parameter(RevitParameters.ManufacturerGenericFamilyParameter);
		}
		familyParameterWrapper = ((familyParameter == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("ManufacturerGenericFamilyParameter"), RevitParameters.ManufacturerGenericFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("ManufacturerGenericFamilyParameter"), familyParameter, ""));
		_ReturnFamilyParameterSet.Add(familyParameterWrapper);
		familyParameter = _FamilyManager.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
		if (familyParameter == null)
		{
			familyParameter = _FamilyManager.get_Parameter(RevitParameters.TypeCommentsGenericFamilyParameter);
		}
		familyParameterWrapper = ((familyParameter == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("TypeCommentsGenericFamilyParameter"), RevitParameters.TypeCommentsGenericFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("TypeCommentsGenericFamilyParameter"), familyParameter, ""));
		_ReturnFamilyParameterSet.Add(familyParameterWrapper);
		familyParameter = _FamilyManager.get_Parameter(BuiltInParameter.ALL_MODEL_URL);
		if (familyParameter == null)
		{
			familyParameter = _FamilyManager.get_Parameter(RevitParameters.URLGenericFamilyParameter);
		}
		familyParameterWrapper = ((familyParameter == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("URLGenericFamilyParameter"), RevitParameters.URLGenericFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("URLGenericFamilyParameter"), familyParameter, ""));
		_ReturnFamilyParameterSet.Add(familyParameterWrapper);
		familyParameter = _FamilyManager.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION);
		if (familyParameter == null)
		{
			familyParameter = _FamilyManager.get_Parameter(RevitParameters.DescriptionGenericFamilyParameter);
		}
		familyParameterWrapper = ((familyParameter == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("DescriptionGenericFamilyParameter"), RevitParameters.DescriptionGenericFamilyParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("DescriptionGenericFamilyParameter"), familyParameter, ""));
		_ReturnFamilyParameterSet.Add(familyParameterWrapper);
		familyParameter = _FamilyManager.get_Parameter(BuiltInParameter.DOOR_COST);
		if (familyParameter == null)
		{
			familyParameter = _FamilyManager.get_Parameter(RevitParameters.CostGenericFamilyParameter);
		}
		familyParameterWrapper = ((familyParameter == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), RevitParameters.CostGenericFamilyParameter, "1.0", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), familyParameter, ""));
		_ReturnFamilyParameterSet.Add(familyParameterWrapper);
	}

	public static void CreateSharedParameters(Document _Document, ref Definitions Definitions, List<string> ParameterList, EPTransactionContainer _Transaction)
	{
		if (Definitions == null)
		{
			return;
		}
		Definition definition = null;
		ExternalDefinition externalDefinition = null;
		ExternalDefinition externalDefinition2 = null;
		TypeBinding typeBinding = null;
		BindingMap bindingMap = null;
		ParameterType parameterType = ParameterType.Text;
		EPTransactionContainer ePTransactionContainer = null;
		string strB = EPShared.LocalRM.GetString("PlantHeightPlantFamilyParameter");
		string strB2 = EPShared.LocalRM.GetString("PlantWidthPlantFamilyParameter");
		string strB3 = EPShared.LocalRM.GetString("PlantCountPlantFamilyParameter");
		string strB4 = EPShared.LocalRM.GetString("PlantingSymbolPlantFamilyParameter");
		string strB5 = EPShared.LocalRM.GetString("GenericModelSymbolPlantFamilyParameter");
		string strB6 = EPShared.LocalRM.GetString("CharacteristicsFoliageSizePlantFamilyParameter");
		string strB7 = EPShared.LocalRM.GetString("PlantSpacingPlantFamilyParameter");
		for (int i = 0; i < ParameterList.Count; i++)
		{
			ExternalDefinitionCreationOptions option = LegacyUnits.DefinitionOptions(type: (string.Compare(ParameterList[i], strB2, ignoreCase: true) != 0 && string.Compare(ParameterList[i], strB, ignoreCase: true) != 0 && string.Compare(ParameterList[i], strB6, ignoreCase: true) != 0 && string.Compare(ParameterList[i], strB7, ignoreCase: true) != 0) ? ((string.Compare(ParameterList[i], strB3, ignoreCase: true) == 0) ? ParameterType.Integer : ((string.Compare(ParameterList[i], strB4, ignoreCase: true) != 0 && string.Compare(ParameterList[i], strB5, ignoreCase: true) != 0) ? ParameterType.Text : ParameterType.FamilyType)) : ParameterType.Length, name: ParameterList[i]);
			definition = Definitions.Create(option);
			if (string.Compare(ParameterList[i], strB4, ignoreCase: true) == 0 || string.Compare(ParameterList[i], strB5, ignoreCase: true) == 0)
			{
				CategorySet categorySet = _Document.Application.Create.NewCategorySet();
				BuiltInCategory categoryId = BuiltInCategory.OST_Planting;
				if (string.Compare(ParameterList[i], strB4, ignoreCase: true) == 0)
				{
					externalDefinition = (ExternalDefinition)definition;
				}
				else if (string.Compare(ParameterList[i], strB5, ignoreCase: true) == 0)
				{
					externalDefinition2 = (ExternalDefinition)definition;
					categoryId = BuiltInCategory.OST_GenericModel;
				}
				if (ePTransactionContainer == null && _Transaction == null)
				{
					ePTransactionContainer = new EPTransactionContainer(_Document);
					ePTransactionContainer.Start(EPShared.LocalRM.GetString("CreateSharedParametersTransaction"));
				}
				Category item = _Document.Settings.Categories.get_Item(categoryId);
				categorySet.Insert(item);
				typeBinding = _Document.Application.Create.NewTypeBinding(categorySet);
				bindingMap = _Document.ParameterBindings;
				bindingMap.Insert(definition, typeBinding);
			}
		}
		if (_Transaction == null && ePTransactionContainer != null && ePTransactionContainer.HasStarted())
		{
			ePTransactionContainer.Commit();
		}
		if (externalDefinition == null && externalDefinition2 == null)
		{
			return;
		}
		FileStream stream = new FileStream(_Document.Application.SharedParametersFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		StreamReader streamReader = new StreamReader(stream, Encoding.ASCII);
		List<string> list = null;
		int num = -1;
		string text = Enum.GetName(typeof(ParameterType), ParameterType.FamilyType.GetHashCode()).ToUpper();
		string empty = string.Empty;
		string empty2 = string.Empty;
		while ((empty = streamReader.ReadLine()) != null)
		{
			if ((externalDefinition != null && empty.IndexOf(externalDefinition.GUID.ToString()) >= 0) || (externalDefinition2 != null && empty.IndexOf(externalDefinition2.GUID.ToString()) >= 0))
			{
				num = empty.IndexOf(text);
				if (num >= 0)
				{
					empty2 = empty.Substring(num + text.Length);
					empty = empty.Substring(0, num + text.Length);
					num = empty2.IndexOf("-1");
					if (num >= 0)
					{
						empty += empty2.Substring(0, num);
						empty = ((empty.IndexOf(externalDefinition.GUID.ToString()) < 0) ? (empty + Convert.ToString(BuiltInCategory.OST_GenericModel.GetHashCode())) : (empty + Convert.ToString(BuiltInCategory.OST_Planting.GetHashCode())));
						empty += empty2.Substring(num + 2);
					}
					else
					{
						empty += empty2;
					}
				}
			}
			if (list == null)
			{
				list = new List<string>();
			}
			list.Add(empty);
		}
		streamReader.Close();
		if (list != null)
		{
			FileStream stream2 = new FileStream(_Document.Application.SharedParametersFilename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			StreamWriter streamWriter = new StreamWriter(stream2, Encoding.ASCII);
			for (int j = 0; j < list.Count; j++)
			{
				streamWriter.WriteLine(list[j]);
			}
			streamWriter.Close();
		}
	}

	public static List<Definition> GetAllCurPlantingSharedParameters(Autodesk.Revit.ApplicationServices.Application _Application)
	{
		List<Definition> result = null;
		FileInfo fileInfo = null;
		if (!string.IsNullOrEmpty(_Application.SharedParametersFilename))
		{
			fileInfo = new FileInfo(_Application.SharedParametersFilename);
		}
		if (fileInfo != null && fileInfo.Exists)
		{
			try
			{
				DefinitionFile definitionFile = _Application.OpenSharedParameterFile();
				result = GetPlantingSharedParameterList(definitionFile);
			}
			catch (Autodesk.Revit.Exceptions.InvalidOperationException)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("TheFile_Message") + _Application.SharedParametersFilename + EPShared.LocalRM.GetString("_CouldNotBeReadMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return null;
			}
		}
		return result;
	}

	private static List<Definition> GetPlantingSharedParameterList(DefinitionFile _DefinitionFile)
	{
		List<string> list = PlantSharedParamenterList();
		List<Definition> list2 = new List<Definition>();
		foreach (DefinitionGroup group in _DefinitionFile.Groups)
		{
			if (group.Definitions == null)
			{
				continue;
			}
			foreach (string item2 in list)
			{
				if (group.Definitions.get_Item(item2) is ExternalDefinition item)
				{
					try
					{
						list2.Add(item);
					}
					catch (Exception)
					{
					}
				}
			}
		}
		return list2;
	}

	private static List<string> FindSharedParametersNotInList(DefinitionFile _DefinitionFile, List<string> _ParameterList)
	{
		List<string> list = null;
		List<string> list2 = null;
		foreach (DefinitionGroup group in _DefinitionFile.Groups)
		{
			if (group.Definitions == null)
			{
				continue;
			}
			List<string> _ParametersInList = new List<string>();
			List<string> list3 = FindSharedParametersNotInList(group.Definitions, _ParameterList, ref _ParametersInList);
			if (list2 == null)
			{
				list2 = _ParametersInList;
			}
			else
			{
				for (int i = 0; i < _ParametersInList.Count; i++)
				{
					if (!list2.Contains(_ParametersInList[i]))
					{
						list2.Add(_ParametersInList[i]);
					}
				}
			}
			if (list == null)
			{
				list = list3;
				continue;
			}
			for (int j = 0; j < list3.Count; j++)
			{
				if (!list2.Contains(list3[j]))
				{
					if (!list.Contains(list3[j]))
					{
						list.Add(list3[j]);
					}
				}
				else if (list.Contains(list3[j]))
				{
					list.Remove(list3[j]);
				}
			}
		}
		return list;
	}

	public static List<string> FindSharedParametersNotInList(Definitions Definitions, List<string> ParameterList)
	{
		List<string> _ParametersInList = new List<string>();
		return FindSharedParametersNotInList(Definitions, ParameterList, ref _ParametersInList);
	}

	private static List<string> FindSharedParametersNotInList(Definitions _Definitions, List<string> _ParameterList, ref List<string> _ParametersInList)
	{
		List<string> list = null;
		if (_ParameterList != null)
		{
			for (int i = 0; i < _ParameterList.Count; i++)
			{
				if (_Definitions == null || _Definitions.get_Item(_ParameterList[i]) == null)
				{
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(_ParameterList[i]);
				}
				else
				{
					_ParametersInList.Add(_ParameterList[i]);
				}
			}
		}
		return list;
	}

	public static List<string> FilterAllLandcaddPlantingSharedParameters(List<string> _FindParameterList)
	{
		List<string> list = null;
		List<string> list2 = PlantSharedParamenterList();
		for (int i = 0; i < _FindParameterList.Count; i++)
		{
			if (list2.Contains(_FindParameterList[i]))
			{
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(_FindParameterList[i]);
			}
		}
		return list;
	}

	public static void VerifySharedParameters(Document _Document, EPTransactionContainer _Transaction)
	{
		FileInfo fileInfo = null;
		string _strSupportPath = string.Empty;
		string text = string.Empty;
		if (!string.IsNullOrEmpty(_Document.Application.SharedParametersFilename))
		{
			text = _Document.Application.SharedParametersFilename;
		}
		fileInfo = new FileInfo(text);
		if (fileInfo != null && !fileInfo.Exists && ApplicationUtility.RegistryPaths.GetSupportPath("LC Shared Parameters.txt", ref _strSupportPath) == 0)
		{
			File.Copy(_strSupportPath, text);
		}
		fileInfo = new FileInfo(text);
		if (fileInfo != null && !fileInfo.Exists)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("TheFile_Message") + _Document.Application.SharedParametersFilename + EPShared.LocalRM.GetString("_CouldNotBeReadMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return;
		}
		List<string> _FindParameterList = new List<string>();
		VerifySharedParametersAreMissing(_Document.Application, ref _FindParameterList);
		if (_FindParameterList == null || _FindParameterList.Count <= 0)
		{
			return;
		}
		List<string> list = new List<string>();
		list.Add(EPShared.LocalRM.GetString("YesString"));
		list.Add(EPShared.LocalRM.GetString("NoString"));
		WarningOptionsForm warningOptionsForm = new WarningOptionsForm("MissingSharedParameters", EPShared.LocalRM.GetString("MissingSharedParametersMessage"), list);
		warningOptionsForm.ShowDialog();
		if (warningOptionsForm.DialogResult != DialogResult.OK || string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) != 0)
		{
			return;
		}
		DefinitionFile definitionFile = _Document.Application.OpenSharedParameterFile();
		if (definitionFile != null)
		{
			DefinitionGroup definitionGroup = definitionFile.Groups.get_Item(EPShared.LocalRM.GetString("PlantingDefinitionGroup"));
			if (definitionGroup == null)
			{
				definitionGroup = definitionFile.Groups.Create(EPShared.LocalRM.GetString("PlantingDefinitionGroup"));
			}
			Definitions Definitions = definitionGroup.Definitions;
			_Transaction.Start("GetFamilyInformationTransaction");
			CreateSharedParameters(_Document, ref Definitions, _FindParameterList, _Transaction);
			_Transaction.Rollback();
		}
	}

	public static void VerifySharedParametersAreMissing(Autodesk.Revit.ApplicationServices.Application _Application, ref List<string> _FindParameterList)
	{
		_FindParameterList = PlantSharedParamenterList();
		if (_FindParameterList == null || _FindParameterList.Count <= 0)
		{
			return;
		}
		FileInfo fileInfo = null;
		if (!string.IsNullOrEmpty(_Application.SharedParametersFilename))
		{
			fileInfo = new FileInfo(_Application.SharedParametersFilename);
		}
		if (fileInfo == null || !fileInfo.Exists)
		{
			return;
		}
		FileStream stream = new FileStream(_Application.SharedParametersFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		StreamReader streamReader = new StreamReader(stream, Encoding.ASCII);
		string empty = string.Empty;
		while ((empty = streamReader.ReadLine()) != null)
		{
			for (int i = 0; i < _FindParameterList.Count; i++)
			{
				if (empty.Contains("\t" + _FindParameterList[i] + "\t"))
				{
					_FindParameterList.RemoveAt(i);
					break;
				}
			}
		}
		streamReader.Close();
	}
}
