using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using SITES.Utilities;

namespace EPCommon;

internal class ManageFamily
{
	public static List<FamilyParameterTypeWrapper> SoftTerrainParameterList()
	{
		List<FamilyParameterTypeWrapper> _ReturnParameterList = new List<FamilyParameterTypeWrapper>();
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), RevitParameters.TypeNamePlantFamilyType, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SoftTerrainLeftOffsetWidthParameter"), RevitParameters.SoftTerrainLeftOffsetWidthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SoftTerrainRightOffsetWidthParameter"), RevitParameters.SoftTerrainRightOffsetWidthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SoftTerrainCutSlopeParameter"), RevitParameters.SoftTerrainCutSlopeParameter, ParameterType.Slope));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SoftTerrainFillSlopeParameter"), RevitParameters.SoftTerrainFillSlopeParameter, ParameterType.Slope));
		CommonParamenterList(ref _ReturnParameterList);
		return _ReturnParameterList;
	}

	public static List<FamilyParameterTypeWrapper> StreetParamenterList()
	{
		List<FamilyParameterTypeWrapper> _ReturnParameterList = new List<FamilyParameterTypeWrapper>();
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), RevitParameters.TypeNamePlantFamilyType, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetNumberOfLanesParameter"), RevitParameters.StreetNumberOfLanesParameter, ParameterType.Integer));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetLaneWidthParameter"), RevitParameters.StreetLaneWidthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetPavementDepthParameter"), RevitParameters.StreetPavementDepthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCrossSlopeParameter"), RevitParameters.StreetCrossSlopeParameter, ParameterType.Slope));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCutSlopeParameter"), RevitParameters.StreetCutSlopeParameter, ParameterType.Slope));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetFillSlopeParameter"), RevitParameters.StreetFillSlopeParameter, ParameterType.Slope));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCurbDisplayParameter"), RevitParameters.StreetCurbDisplayParameter, ParameterType.YesNo));
		CommonCurbParametersList(ref _ReturnParameterList);
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetBoulevardWidthParameter"), RevitParameters.StreetBoulevardWidthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetPavementSurfaceParameter"), RevitParameters.StreetPavementSurfaceParameter, ParameterType.Integer));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetBaseSurfaceParameter"), RevitParameters.StreetBaseSurfaceParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetSubbaseSurfaceParameter"), RevitParameters.StreetSubbaseSurfaceParameter, ParameterType.Length));
		CommonParamenterList(ref _ReturnParameterList);
		return _ReturnParameterList;
	}

	public static List<FamilyParameterTypeWrapper> CurbParameterList()
	{
		List<FamilyParameterTypeWrapper> _ReturnParameterList = new List<FamilyParameterTypeWrapper>();
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), RevitParameters.TypeNamePlantFamilyType, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("CurbGutterHeightParameter"), RevitParameters.CurbGutterHeightParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("CurbBaseSurfaceParameter"), RevitParameters.CurbBaseSurfaceParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("CurbSubbaseSurfaceParameter"), RevitParameters.CurbSubbaseSurfaceParameter, ParameterType.Length));
		CommonCurbParametersList(ref _ReturnParameterList);
		CommonParamenterList(ref _ReturnParameterList);
		return _ReturnParameterList;
	}

	private static void CommonCurbParametersList(ref List<FamilyParameterTypeWrapper> _ReturnParameterList)
	{
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCurbHeightParameter"), RevitParameters.StreetCurbHeightParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCurbOffsetParameter"), RevitParameters.StreetCurbOffsetParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCurbWidthParameter"), RevitParameters.StreetCurbWidthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetGutterWidthParameter"), RevitParameters.StreetGutterWidthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetGutterSlopeParameter"), RevitParameters.StreetGutterSlopeParameter, ParameterType.Slope));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCurbMaterialParameter"), RevitParameters.StreetCurbMaterialParameter, ParameterType.Integer));
	}

	public static List<FamilyParameterTypeWrapper> SideWalkParamterList()
	{
		List<FamilyParameterTypeWrapper> _ReturnParameterList = new List<FamilyParameterTypeWrapper>();
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), RevitParameters.TypeNamePlantFamilyType, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SideWalkLocationLineParameter"), RevitParameters.SideWalkLocationLineParameter, ParameterType.Number));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SideWalkWidthParameter"), RevitParameters.SideWalkWidthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SideWalkPavementDepthParameter"), RevitParameters.SideWalkPavementDepthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SideWalkPavementSurfaceParameter"), RevitParameters.SideWalkPavementSurfaceParameter, ParameterType.Integer));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SideWalkBaseSurfaceParameter"), RevitParameters.SideWalkBaseSurfaceParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SideWalkSubbaseSurfaceParameter"), RevitParameters.SideWalkSubbaseSurfaceParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SideWalkCutSlopeParameter"), RevitParameters.SideWalkCutSlopeParameter, ParameterType.Slope));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SideWalkFillSlopeParameter"), RevitParameters.SideWalkFillSlopeParameter, ParameterType.Slope));
		CommonParamenterList(ref _ReturnParameterList);
		return _ReturnParameterList;
	}

	public static List<FamilyParameterTypeWrapper> RetainingWallParameterList()
	{
		List<FamilyParameterTypeWrapper> _ReturnParameterList = new List<FamilyParameterTypeWrapper>();
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), RevitParameters.TypeNamePlantFamilyType, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("RetainingWallWidthParameter"), RevitParameters.RetainingWallWidthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("RetainingWallHeightParameter"), RevitParameters.RetainingWallHeightParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("RetainingWallSlopeProjectionParameter"), RevitParameters.RetainingWallSlopeProjectionParameter, ParameterType.YesNo));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("RetainingWallMaterialParameter"), RevitParameters.RetainingWallMaterialParameter, ParameterType.Integer));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("RetainingWallCutSlopeParameter"), RevitParameters.RetainingWallCutSlopeParameter, ParameterType.Slope));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("RetainingWallFillSlopeParameter"), RevitParameters.RetainingWallFillSlopeParameter, ParameterType.Slope));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("RetainingWallBaseSurfaceParameter"), RevitParameters.RetainingWallBaseSurfaceParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("RetainingWallSubbaseSurfaceParameter"), RevitParameters.RetainingWallSubbaseSurfaceParameter, ParameterType.Length));
		CommonParamenterList(ref _ReturnParameterList);
		return _ReturnParameterList;
	}

	public static List<FamilyParameterTypeWrapper> StepsParameterList()
	{
		List<FamilyParameterTypeWrapper> _ReturnParameterList = new List<FamilyParameterTypeWrapper>();
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), RevitParameters.TypeNamePlantFamilyType, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StepsHeightParameter"), RevitParameters.StepsHeightParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StepsTreadParameter"), RevitParameters.StepsTreadParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StepsWidthParameter"), RevitParameters.StepsWidthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StepsMaterialParameter"), RevitParameters.StepsMaterialParameter, ParameterType.Integer));
		CommonParamenterList(ref _ReturnParameterList);
		return _ReturnParameterList;
	}

	public static List<FamilyParameterTypeWrapper> ParkingLotParameterList()
	{
		List<FamilyParameterTypeWrapper> _ReturnParameterList = new List<FamilyParameterTypeWrapper>();
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), RevitParameters.TypeNamePlantFamilyType, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ParkingLotPavementDepthParameter"), RevitParameters.ParkingLotPavementDepthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ParkingLotParkingPavementSurfaceParameter"), RevitParameters.ParkingLotParkingPavementSurfaceParameter, ParameterType.Integer));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ParkingLotParkingBaseDepthParameter"), RevitParameters.ParkingLotParkingBaseDepthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ParkingLotParkingSubBaseDepthParameter"), RevitParameters.ParkingLotParkingSubBaseDepthParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SiteworksIDFamilyParameter"), RevitParameters.SiteworksIDFamilyParameter, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), RevitParameters.LinearIncrementParameter, ParameterType.Length));
		GenericModelParamenterList(ref _ReturnParameterList);
		return _ReturnParameterList;
	}

	public static void CommonParamenterList(ref List<FamilyParameterTypeWrapper> _ReturnParameterList)
	{
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("SiteworksIDFamilyParameter"), RevitParameters.SiteworksIDFamilyParameter, ParameterType.Text));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), RevitParameters.LinearIncrementParameter, ParameterType.Length));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), RevitParameters.CurveIncrementParameter, ParameterType.Angle));
		GenericModelParamenterList(ref _ReturnParameterList);
	}

	public static void GenericModelParamenterList(ref List<FamilyParameterTypeWrapper> _ReturnParameterList)
	{
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("KeynoteGenericFamilyParameter"), RevitParameters.KeynoteGenericFamilyParameter, ParameterType.Text, BuiltInParameter.KEYNOTE_PARAM));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ModelGenericFamilyParameter"), RevitParameters.ModelGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_MODEL));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ManufacturerGenericFamilyParameter"), RevitParameters.ManufacturerGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_MANUFACTURER));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("TypeCommentsGenericFamilyParameter"), RevitParameters.TypeCommentsGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_TYPE_COMMENTS));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("URLGenericFamilyParameter"), RevitParameters.URLGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_URL));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("DescriptionGenericFamilyParameter"), RevitParameters.DescriptionGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_DESCRIPTION));
		_ReturnParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), RevitParameters.CostGenericFamilyParameter, ParameterType.Currency, BuiltInParameter.DOOR_COST));
	}

	public static bool CreateNewFamilyDocument(Document _ActiveDocument, List<FamilyTypeContainer> _FamilyTypeList, LandcaddID _Type, string _strNewFilename, ref Document _FamilyDocument)
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
		if (!SetupFamilyDocument(_ActiveDocument, _strNewFilename, _Type, ref _bIsActiveDocument, ref _bIsDocumentOpen, ref _CurDocument, ref _TransactionContainer, ref _NewFamilyParameterList))
		{
			return false;
		}
		bool flag = false;
		FamilyType familyType = null;
		string empty = string.Empty;
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.MinValue = 0;
		if (_FamilyTypeList.Count <= 0 || _FamilyTypeList == null)
		{
			progressBarForm.MaxValue = 30;
		}
		else
		{
			progressBarForm.MaxValue = _FamilyTypeList.Count;
		}
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressbarCreatingFamilyMessage");
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		for (int i = 0; i < _FamilyTypeList.Count; i++)
		{
			FamilyType _SelectedType = null;
			if (!(_FamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName != string.Empty))
			{
				continue;
			}
			progressBarForm.Message = string.Concat(str2: (FindTypeNames(_CurDocument.FamilyManager.Types, _FamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName, ref _SelectedType) ? _SelectedType : _CurDocument.FamilyManager.NewType(_FamilyTypeList[i].FamilyTypeWrapper.FamilyTypeName)).Name, str0: EPShared.LocalRM.GetString("CreatingFamilyTypeMessage"), str1: " ");
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
							if (_FamilyTypeList[i].GetDoubleValue(num2, ref _dValue))
							{
								_CurDocument.FamilyManager.Set(familyParameter, _dValue);
							}
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
							catch (ArgumentOutOfRangeException ex)
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
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		_TransactionContainer.EndTransaction();
		_TransactionContainer.Commit();
		_FamilyDocument = _CurDocument;
		if (flag)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("GetNewRevitVersionMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
		return true;
	}

	public static List<FamilyType> RemoveTypesFromDocument(FamilyTypeSet DocFamilyTypeList, List<FamilyTypeContainer> _FamilyTypeList)
	{
		List<FamilyType> list = new List<FamilyType>();
		foreach (FamilyType DocFamilyType in DocFamilyTypeList)
		{
			bool flag = false;
			foreach (FamilyTypeContainer _FamilyType in _FamilyTypeList)
			{
				if (string.Compare(_FamilyType.FamilyTypeWrapper.FamilyTypeName, DocFamilyType.Name, ignoreCase: false) == 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(DocFamilyType);
			}
		}
		return list;
	}

	public static bool FindTypeNames(FamilyTypeSet _CurFamilyTypeList, string _strTypeName, ref FamilyType _SelectedType)
	{
		foreach (FamilyType _CurFamilyType in _CurFamilyTypeList)
		{
			if (string.Compare(_CurFamilyType.Name, _strTypeName, ignoreCase: true) == 0)
			{
				_SelectedType = _CurFamilyType;
				return true;
			}
		}
		return false;
	}

	private static bool SetupFamilyDocument(Document _ActiveDocument, string _strNewFilename, LandcaddID _Type, ref bool _bIsActiveDocument, ref bool _bIsDocumentOpen, ref Document _CurDocument, ref EPTransactionContainer _TransactionContainer, ref List<FamilyParameter> _NewFamilyParameterList)
	{
		_CurDocument = _ActiveDocument;
		if (RevitUtility.CompareFileNames(_ActiveDocument.PathName, _strNewFilename) == 0)
		{
			_bIsActiveDocument = true;
		}
		else if (File.Exists(_strNewFilename))
		{
			_bIsDocumentOpen = ManagePlants.IsDocumentOpen(_ActiveDocument.Application, _strNewFilename, ref _CurDocument);
			if (!_bIsDocumentOpen)
			{
				_CurDocument = _ActiveDocument.Application.OpenDocumentFile(_strNewFilename);
			}
		}
		else
		{
			string empty = string.Empty;
			empty = GetFolderFileName(_Type, empty, out var strFolderName, out var strFileName);
			string _strFullFamilyFilename = string.Empty;
			if (RevitUtility.GetFullFamilyFilenameIfExist(strFolderName, strFileName, ref _strFullFamilyFilename))
			{
				_CurDocument = _ActiveDocument.Application.NewFamilyDocument(_strFullFamilyFilename);
			}
			else
			{
				if (!FindSiteTemplateFileName(_CurDocument, ref _strFullFamilyFilename, _bIssueWarning: true))
				{
					return false;
				}
				_CurDocument = _ActiveDocument.Application.NewFamilyDocument(_strFullFamilyFilename);
			}
		}
		_TransactionContainer = new EPTransactionContainer(_CurDocument);
		List<FamilyParameterTypeWrapper> list = new List<FamilyParameterTypeWrapper>();
		string empty2 = string.Empty;
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
			list = StreetParamenterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("StreetTypeManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.Curb:
			list = CurbParameterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("CurbTypeManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.Sidewalk:
			list = SideWalkParamterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("SidewalkTypeManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.RetainingWall:
			list = RetainingWallParameterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("RetainingWallManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.Steps:
			list = StepsParameterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("StepsManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.ParkingLot:
			list = ParkingLotParameterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("ParkingLotManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.SoftTerrain:
			list = SoftTerrainParameterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("SoftTerrainManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
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

	private static string GetFolderFileName(LandcaddID _Type, string strCurrentFamilyType, out string strFolderName, out string strFileName)
	{
		strFolderName = string.Empty;
		strFileName = string.Empty;
		switch (_Type)
		{
		case LandcaddID.PlantMix:
			strCurrentFamilyType = Convert.ToInt32(LandcaddID.PlantMix).ToString("000");
			strFolderName = string.Empty;
			strFileName = string.Empty;
			break;
		case LandcaddID.PatioWalk:
			strCurrentFamilyType = Convert.ToInt32(LandcaddID.PatioWalk).ToString("000");
			strFolderName = string.Empty;
			strFileName = string.Empty;
			break;
		case LandcaddID.Street:
			strCurrentFamilyType = Convert.ToInt32(LandcaddID.Street).ToString("000");
			strFolderName = string.Empty;
			strFileName = "Sample Street.rft";
			break;
		case LandcaddID.Curb:
			strCurrentFamilyType = Convert.ToInt32(LandcaddID.Curb).ToString("000");
			strFolderName = string.Empty;
			strFileName = "Sample Curb.rft";
			break;
		case LandcaddID.Sidewalk:
			strCurrentFamilyType = Convert.ToInt32(LandcaddID.Sidewalk).ToString("000");
			strFolderName = string.Empty;
			strFileName = "Sample Sidewalk.rft";
			break;
		case LandcaddID.RetainingWall:
			strCurrentFamilyType = Convert.ToInt32(LandcaddID.RetainingWall).ToString("000");
			strFolderName = string.Empty;
			strFileName = "Sample Retaining Wall.rft";
			break;
		case LandcaddID.Steps:
			strCurrentFamilyType = Convert.ToInt32(LandcaddID.Steps).ToString("000");
			strFolderName = string.Empty;
			strFileName = string.Empty;
			break;
		case LandcaddID.ParkingLot:
			strCurrentFamilyType = Convert.ToInt32(LandcaddID.ParkingLot).ToString("000");
			strFolderName = string.Empty;
			strFileName = "Sample Parking Lot.rft";
			break;
		case LandcaddID.SoftTerrain:
			strCurrentFamilyType = Convert.ToInt32(LandcaddID.SoftTerrain).ToString("000");
			strFolderName = string.Empty;
			strFileName = "Sample Soft Terrain.rft";
			break;
		}
		return strCurrentFamilyType;
	}

	private static bool SetupFamilyDocument(Document _ActiveDocument, string _strNewFilename, LandcaddID _Type, Family _CurrFamily, ref bool _bIsActiveDocument, ref bool _bIsDocumentOpen, ref Document _CurDocument, ref EPTransactionContainer _TransactionContainer, ref List<FamilyParameter> _NewFamilyParameterList)
	{
		_CurDocument = _ActiveDocument;
		if (_CurrFamily != null)
		{
			EPTransactionContainer ePTransactionContainer = new EPTransactionContainer(_ActiveDocument);
			ePTransactionContainer.Start(EPShared.LocalRM.GetString("UpdatingFamilyTransaction"));
			ePTransactionContainer.BeginTransaction();
			_ActiveDocument.Delete(_CurrFamily.Id);
			ePTransactionContainer.EndTransaction();
			ePTransactionContainer.Commit();
		}
		if (RevitUtility.CompareFileNames(_ActiveDocument.PathName, _strNewFilename) == 0)
		{
			_bIsActiveDocument = true;
		}
		else if (File.Exists(_strNewFilename))
		{
			_bIsDocumentOpen = ManagePlants.IsDocumentOpen(_ActiveDocument.Application, _strNewFilename, ref _CurDocument);
			if (!_bIsDocumentOpen)
			{
				_CurDocument = _ActiveDocument.Application.OpenDocumentFile(_strNewFilename);
			}
		}
		else
		{
			string empty = string.Empty;
			empty = GetFolderFileName(_Type, empty, out var strFolderName, out var strFileName);
			string _strFullFamilyFilename = string.Empty;
			if (RevitUtility.GetFullFamilyFilenameIfExist(strFolderName, strFileName, ref _strFullFamilyFilename))
			{
				_CurDocument = _ActiveDocument.Application.NewFamilyDocument(_strFullFamilyFilename);
			}
			else
			{
				if (!FindSiteTemplateFileName(_CurDocument, ref _strFullFamilyFilename, _bIssueWarning: true))
				{
					return false;
				}
				_CurDocument = _ActiveDocument.Application.NewFamilyDocument(_strFullFamilyFilename);
			}
		}
		_TransactionContainer = new EPTransactionContainer(_CurDocument);
		List<FamilyParameterTypeWrapper> list = new List<FamilyParameterTypeWrapper>();
		string empty2 = string.Empty;
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
			list = StreetParamenterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("StreetTypeManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.Curb:
			list = CurbParameterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("CurbTypeManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.Sidewalk:
			list = SideWalkParamterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("SidewalkTypeManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.RetainingWall:
			list = RetainingWallParameterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("RetainingWallManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.Steps:
			list = StepsParameterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("StepsManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.ParkingLot:
			list = ParkingLotParameterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("ParkingLotManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
			break;
		case LandcaddID.SoftTerrain:
			list = SoftTerrainParameterList();
			empty2 = EPShared.LocalRM.GetString("Create_") + EPShared.LocalRM.GetString("SoftTerrainManagerString") + EPShared.LocalRM.GetString("_Family_");
			_TransactionContainer.Start(empty2);
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

	public static bool FindSiteTemplateFileName(Document _Document, ref string _strTemplateFileName, bool _bIssueWarning)
	{
		bool flag = false;
		string strSettingsName = "SiteTemplateFileName";
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
									text = ApplicationUtility.GetString("SiteImperialTemplate") + ".rft";
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
									text = ApplicationUtility.GetString("SiteMetricTemplate") + ".rft";
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
								catch (DirectoryNotFoundException)
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
							_strTemplateFileName = text3 + ApplicationUtility.GetString("SiteImperialTemplate");
						}
						else
						{
							_strTemplateFileName = text3 + ApplicationUtility.GetString("SiteMetricTemplate");
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
						if (string.Compare(empty, EPShared.LocalRM.GetString("ContentInstalledName"), ignoreCase: true) != 0)
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
								_strTemplateFileName = text4 + ApplicationUtility.GetString("SiteImperialTemplate");
							}
							else
							{
								_strTemplateFileName = text4 + ApplicationUtility.GetString("SiteMetricTemplate");
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
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoSiteRFTMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
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

	public static List<FamilyParameterWrapper> GetStreetFamilyParameterSetForType(FamilyType _FamilyType, FamilyManager _FamilyManager)
	{
		List<FamilyParameterWrapper> ReturnFamilyParameterSet = new List<FamilyParameterWrapper>();
		_FamilyManager.CurrentType = _FamilyType;
		FamilyParameterWrapper item = ((_FamilyManager.get_Parameter(RevitParameters.StreetNumberOfLanesParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetNumberOfLanesParameter"), RevitParameters.StreetNumberOfLanesParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetNumberOfLanesParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetNumberOfLanesParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetLaneWidthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetLaneWidthParameter"), RevitParameters.StreetLaneWidthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetLaneWidthParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetLaneWidthParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetPavementDepthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetPavementDepthParameter"), RevitParameters.StreetPavementDepthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetPavementDepthParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetPavementDepthParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetCrossSlopeParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCrossSlopeParameter"), RevitParameters.StreetCrossSlopeParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCrossSlopeParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetCrossSlopeParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetCutSlopeParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCutSlopeParameter"), RevitParameters.StreetCutSlopeParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCutSlopeParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetCutSlopeParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetFillSlopeParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetFillSlopeParameter"), RevitParameters.StreetFillSlopeParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetFillSlopeParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetFillSlopeParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetCurbDisplayParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCurbDisplayParameter"), RevitParameters.StreetCurbDisplayParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCurbDisplayParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetCurbDisplayParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		GetCommonCurbParameters(ref ReturnFamilyParameterSet, _FamilyType, _FamilyManager);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetBoulevardWidthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetBoulevardWidthParameter"), RevitParameters.StreetBoulevardWidthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetBoulevardWidthParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetBoulevardWidthParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetPavementSurfaceParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetPavementSurfaceParameter"), RevitParameters.StreetPavementSurfaceParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetPavementSurfaceParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetPavementSurfaceParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetBaseSurfaceParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetBaseSurfaceParameter"), RevitParameters.StreetBaseSurfaceParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetBaseSurfaceParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetBaseSurfaceParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetSubbaseSurfaceParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetSubbaseSurfaceParameter"), RevitParameters.StreetSubbaseSurfaceParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetSubbaseSurfaceParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetSubbaseSurfaceParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), RevitParameters.LinearIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), RevitParameters.CurveIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		GetGenericModelFamilyParameterSetForType(_FamilyType, _FamilyManager, ref ReturnFamilyParameterSet);
		return ReturnFamilyParameterSet;
	}

	public static List<FamilyParameterWrapper> GetCurbFamilyParameterSetForType(FamilyType _FamilyType, FamilyManager _FamilyManager)
	{
		List<FamilyParameterWrapper> ReturnFamilyParameterSet = new List<FamilyParameterWrapper>();
		_FamilyManager.CurrentType = _FamilyType;
		FamilyParameterWrapper item = ((_FamilyManager.get_Parameter(RevitParameters.CurbGutterHeightParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurbGutterHeightParameter"), RevitParameters.CurbGutterHeightParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurbGutterHeightParameter"), _FamilyManager.get_Parameter(RevitParameters.CurbGutterHeightParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CurbBaseSurfaceParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurbBaseSurfaceParameter"), RevitParameters.CurbBaseSurfaceParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurbBaseSurfaceParameter"), _FamilyManager.get_Parameter(RevitParameters.CurbBaseSurfaceParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CurbSubbaseSurfaceParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurbSubbaseSurfaceParameter"), RevitParameters.CurbSubbaseSurfaceParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurbSubbaseSurfaceParameter"), _FamilyManager.get_Parameter(RevitParameters.CurbSubbaseSurfaceParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		GetCommonCurbParameters(ref ReturnFamilyParameterSet, _FamilyType, _FamilyManager);
		item = ((_FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), RevitParameters.LinearIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), RevitParameters.CurveIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		GetGenericModelFamilyParameterSetForType(_FamilyType, _FamilyManager, ref ReturnFamilyParameterSet);
		return ReturnFamilyParameterSet;
	}

	private static void GetCommonCurbParameters(ref List<FamilyParameterWrapper> ReturnFamilyParameterSet, FamilyType _FamilyType, FamilyManager _FamilyManager)
	{
		FamilyParameterWrapper item = ((_FamilyManager.get_Parameter(RevitParameters.StreetCurbHeightParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCurbHeightParameter"), RevitParameters.StreetCurbHeightParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCurbHeightParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetCurbHeightParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetCurbOffsetParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCurbOffsetParameter"), RevitParameters.StreetCurbOffsetParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCurbOffsetParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetCurbOffsetParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetCurbWidthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCurbWidthParameter"), RevitParameters.StreetCurbWidthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetCurbWidthParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetCurbWidthParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StreetGutterWidthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetGutterWidthParameter"), RevitParameters.StreetGutterWidthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StreetGutterWidthParameter"), _FamilyManager.get_Parameter(RevitParameters.StreetGutterWidthParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CurbGutterSlopeParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurbGutterSlopeParameter"), RevitParameters.CurbGutterSlopeParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurbGutterSlopeParameter"), _FamilyManager.get_Parameter(RevitParameters.CurbGutterSlopeParameter), ""));
		ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CurbMaterialParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurbMaterialParameter"), RevitParameters.CurbMaterialParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurbMaterialParameter"), _FamilyManager.get_Parameter(RevitParameters.CurbMaterialParameter), ""));
		ReturnFamilyParameterSet.Add(item);
	}

	public static List<FamilyParameterWrapper> GetSidewalkFamilyParameterSetForType(FamilyType _FamilyType, FamilyManager _FamilyManager)
	{
		List<FamilyParameterWrapper> _ReturnFamilyParameterSet = new List<FamilyParameterWrapper>();
		_FamilyManager.CurrentType = _FamilyType;
		FamilyParameterWrapper item = ((_FamilyManager.get_Parameter(RevitParameters.SideWalkLocationLineParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkLocationLineParameter"), RevitParameters.SideWalkLocationLineParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkLocationLineParameter"), _FamilyManager.get_Parameter(RevitParameters.SideWalkLocationLineParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.SideWalkWidthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkWidthParameter"), RevitParameters.SideWalkWidthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkWidthParameter"), _FamilyManager.get_Parameter(RevitParameters.SideWalkWidthParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.SideWalkPavementDepthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkPavementDepthParameter"), RevitParameters.SideWalkPavementDepthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkPavementDepthParameter"), _FamilyManager.get_Parameter(RevitParameters.SideWalkPavementDepthParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.SideWalkPavementSurfaceParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkPavementSurfaceParameter"), RevitParameters.SideWalkPavementSurfaceParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkPavementSurfaceParameter"), _FamilyManager.get_Parameter(RevitParameters.SideWalkPavementSurfaceParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.SideWalkBaseSurfaceParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkBaseSurfaceParameter"), RevitParameters.SideWalkBaseSurfaceParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkBaseSurfaceParameter"), _FamilyManager.get_Parameter(RevitParameters.SideWalkBaseSurfaceParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.SideWalkSubbaseSurfaceParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkSubbaseSurfaceParameter"), RevitParameters.SideWalkSubbaseSurfaceParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkSubbaseSurfaceParameter"), _FamilyManager.get_Parameter(RevitParameters.SideWalkSubbaseSurfaceParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), RevitParameters.LinearIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), RevitParameters.CurveIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.SideWalkCutSlopeParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkCutSlopeParameter"), RevitParameters.SideWalkCutSlopeParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkCutSlopeParameter"), _FamilyManager.get_Parameter(RevitParameters.SideWalkCutSlopeParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.SideWalkFillSlopeParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkFillSlopeParameter"), RevitParameters.SideWalkFillSlopeParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SideWalkFillSlopeParameter"), _FamilyManager.get_Parameter(RevitParameters.SideWalkFillSlopeParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		GetGenericModelFamilyParameterSetForType(_FamilyType, _FamilyManager, ref _ReturnFamilyParameterSet);
		return _ReturnFamilyParameterSet;
	}

	public static List<FamilyParameterWrapper> GetRetainingWallFamilyParameterSetForType(FamilyType _FamilyType, FamilyManager _FamilyManager)
	{
		List<FamilyParameterWrapper> _ReturnFamilyParameterSet = new List<FamilyParameterWrapper>();
		_FamilyManager.CurrentType = _FamilyType;
		FamilyParameterWrapper item = ((_FamilyManager.get_Parameter(RevitParameters.RetainingWallWidthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallWidthParameter"), RevitParameters.RetainingWallWidthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallWidthParameter"), _FamilyManager.get_Parameter(RevitParameters.RetainingWallWidthParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.RetainingWallHeightParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallHeightParameter"), RevitParameters.RetainingWallHeightParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallHeightParameter"), _FamilyManager.get_Parameter(RevitParameters.RetainingWallHeightParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.RetainingWallSlopeProjectionParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallSlopeProjectionParameter"), RevitParameters.RetainingWallSlopeProjectionParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallSlopeProjectionParameter"), _FamilyManager.get_Parameter(RevitParameters.RetainingWallSlopeProjectionParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.RetainingWallMaterialParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallMaterialParameter"), RevitParameters.RetainingWallMaterialParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallMaterialParameter"), _FamilyManager.get_Parameter(RevitParameters.RetainingWallMaterialParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.RetainingWallCutSlopeParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallCutSlopeParameter"), RevitParameters.RetainingWallCutSlopeParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallCutSlopeParameter"), _FamilyManager.get_Parameter(RevitParameters.RetainingWallCutSlopeParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.RetainingWallFillSlopeParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallFillSlopeParameter"), RevitParameters.RetainingWallFillSlopeParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallFillSlopeParameter"), _FamilyManager.get_Parameter(RevitParameters.RetainingWallFillSlopeParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), RevitParameters.LinearIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), RevitParameters.CurveIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.RetainingWallBaseSurfaceParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallBaseSurfaceParameter"), RevitParameters.RetainingWallBaseSurfaceParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallBaseSurfaceParameter"), _FamilyManager.get_Parameter(RevitParameters.RetainingWallBaseSurfaceParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.RetainingWallSubbaseSurfaceParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallSubbaseSurfaceParameter"), RevitParameters.RetainingWallSubbaseSurfaceParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("RetainingWallSubbaseSurfaceParameter"), _FamilyManager.get_Parameter(RevitParameters.RetainingWallSubbaseSurfaceParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		GetGenericModelFamilyParameterSetForType(_FamilyType, _FamilyManager, ref _ReturnFamilyParameterSet);
		return _ReturnFamilyParameterSet;
	}

	public static List<FamilyParameterWrapper> GetStepsFamilyParameterSetForType(FamilyType _FamilyType, FamilyManager _FamilyManager)
	{
		List<FamilyParameterWrapper> _ReturnFamilyParameterSet = new List<FamilyParameterWrapper>();
		_FamilyManager.CurrentType = _FamilyType;
		FamilyParameterWrapper item = ((_FamilyManager.get_Parameter(RevitParameters.StepsHeightParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StepsHeightParameter"), RevitParameters.StepsHeightParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StepsHeightParameter"), _FamilyManager.get_Parameter(RevitParameters.StepsHeightParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StepsTreadParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StepsTreadParameter"), RevitParameters.StepsTreadParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StepsTreadParameter"), _FamilyManager.get_Parameter(RevitParameters.StepsTreadParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StepsWidthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StepsWidthParameter"), RevitParameters.StepsWidthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StepsWidthParameter"), _FamilyManager.get_Parameter(RevitParameters.StepsWidthParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.StepsMaterialParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("StepsMaterialParameter"), RevitParameters.StepsMaterialParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("StepsMaterialParameter"), _FamilyManager.get_Parameter(RevitParameters.StepsMaterialParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), RevitParameters.LinearIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), RevitParameters.CurveIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		GetGenericModelFamilyParameterSetForType(_FamilyType, _FamilyManager, ref _ReturnFamilyParameterSet);
		return _ReturnFamilyParameterSet;
	}

	public static List<FamilyParameterWrapper> GetParkingLotFamilyParameterSetForType(FamilyType _FamilyType, FamilyManager _FamilyManager)
	{
		List<FamilyParameterWrapper> _ReturnFamilyParameterSet = new List<FamilyParameterWrapper>();
		_FamilyManager.CurrentType = _FamilyType;
		FamilyParameterWrapper item = ((_FamilyManager.get_Parameter(RevitParameters.ParkingLotPavementDepthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("ParkingLotPavementDepthParameter"), RevitParameters.ParkingLotPavementDepthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("ParkingLotPavementDepthParameter"), _FamilyManager.get_Parameter(RevitParameters.ParkingLotPavementDepthParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.ParkingLotParkingPavementSurfaceParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("ParkingLotParkingPavementSurfaceParameter"), RevitParameters.ParkingLotParkingPavementSurfaceParameter, "", StorageType.String) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("ParkingLotParkingPavementSurfaceParameter"), _FamilyManager.get_Parameter(RevitParameters.ParkingLotParkingPavementSurfaceParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.ParkingLotParkingBaseDepthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("ParkingLotParkingBaseDepthParameter"), RevitParameters.ParkingLotParkingBaseDepthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("ParkingLotParkingBaseDepthParameter"), _FamilyManager.get_Parameter(RevitParameters.ParkingLotParkingBaseDepthParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.ParkingLotParkingSubBaseDepthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("ParkingLotParkingSubBaseDepthParameter"), RevitParameters.ParkingLotParkingSubBaseDepthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("ParkingLotParkingSubBaseDepthParameter"), _FamilyManager.get_Parameter(RevitParameters.ParkingLotParkingSubBaseDepthParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), RevitParameters.LinearIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		GetGenericModelFamilyParameterSetForType(_FamilyType, _FamilyManager, ref _ReturnFamilyParameterSet);
		return _ReturnFamilyParameterSet;
	}

	public static List<FamilyParameterWrapper> GetSoftTerrainFamilyParameterSetForType(FamilyType _FamilyType, FamilyManager _FamilyManager)
	{
		List<FamilyParameterWrapper> _ReturnFamilyParameterSet = new List<FamilyParameterWrapper>();
		_FamilyManager.CurrentType = _FamilyType;
		FamilyParameterWrapper item = ((_FamilyManager.get_Parameter(RevitParameters.SoftTerrainLeftOffsetWidthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SoftTerrainLeftOffsetWidthParameter"), RevitParameters.SoftTerrainLeftOffsetWidthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SoftTerrainLeftOffsetWidthParameter"), _FamilyManager.get_Parameter(RevitParameters.SoftTerrainLeftOffsetWidthParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.SoftTerrainRightOffsetWidthParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SoftTerrainRightOffsetWidthParameter"), RevitParameters.SoftTerrainRightOffsetWidthParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SoftTerrainRightOffsetWidthParameter"), _FamilyManager.get_Parameter(RevitParameters.SoftTerrainRightOffsetWidthParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.SoftTerrainCutSlopeParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SoftTerrainCutSlopeParameter"), RevitParameters.SoftTerrainCutSlopeParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SoftTerrainCutSlopeParameter"), _FamilyManager.get_Parameter(RevitParameters.SoftTerrainCutSlopeParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.SoftTerrainFillSlopeParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("SoftTerrainFillSlopeParameter"), RevitParameters.SoftTerrainFillSlopeParameter, "", StorageType.Integer) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("SoftTerrainFillSlopeParameter"), _FamilyManager.get_Parameter(RevitParameters.SoftTerrainFillSlopeParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), RevitParameters.LinearIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.LinearIncrementParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		item = ((_FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter) == null) ? new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), RevitParameters.CurveIncrementParameter, "", StorageType.Double) : new FamilyParameterWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), _FamilyManager.get_Parameter(RevitParameters.CurveIncrementParameter), ""));
		_ReturnFamilyParameterSet.Add(item);
		GetGenericModelFamilyParameterSetForType(_FamilyType, _FamilyManager, ref _ReturnFamilyParameterSet);
		return _ReturnFamilyParameterSet;
	}
}
