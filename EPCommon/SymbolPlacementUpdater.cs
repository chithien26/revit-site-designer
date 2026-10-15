using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.Exceptions;
using SITES.Utilities;

namespace EPCommon;

public class SymbolPlacementUpdater : IUpdater
{
	private static AddInId m_ApplicationId = new AddInId(new Guid("da9d271e-f7a0-4291-8837-02947862c85e"));

	private static UpdaterId m_UpdaterId = new UpdaterId(m_ApplicationId, new Guid("df40ed3d-8df8-44e8-a41f-24dfec3317ee"));

	private List<GenericModelFamilyInstanceWrapper> m_GenericModelElementList;

	private int m_nDocumentsOpenCount;

	public SymbolPlacementUpdater()
	{
		m_GenericModelElementList = new List<GenericModelFamilyInstanceWrapper>();
	}

	public void Execute(UpdaterData _Data)
	{
		Document document = _Data.GetDocument();
		Parameter parameter = null;
		Parameter parameter2 = null;
		Parameter parameter3 = null;
		FilteredElementCollector filteredElementCollector = null;
		List<Element> list = null;
		List<KeyValuePair<string, FamilyInstance>> _GenericModelElementList = null;
		List<LocateType> _FamilyQueryLocateTyoeList = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		foreach (ElementId addedElementId in _Data.GetAddedElementIds())
		{
			Element element = document.GetElement(addedElementId);
			if (!(element is FamilyInstance { Category: not null } familyInstance) || (familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Planting.GetHashCode() && familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Parking.GetHashCode() && familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode() && familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Entourage.GetHashCode() && familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Furniture.GetHashCode() && familyInstance.Category.Id.Value64() != BuiltInCategory.OST_ElectricalEquipment.GetHashCode() && familyInstance.Category.Id.Value64() != BuiltInCategory.OST_GenericModel.GetHashCode() && familyInstance.Category.Id.Value64() != BuiltInCategory.OST_LightingFixtures.GetHashCode() && familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Columns.GetHashCode()))
			{
				continue;
			}
			parameter = familyInstance.Symbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter);
			if (parameter != null && string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.FamilyInstancePlacement).ToString("000"), ignoreCase: true) == 0)
			{
				parameter2 = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
				if (parameter2 == null)
				{
					continue;
				}
				if (list == null)
				{
					list = new List<Element>();
					filteredElementCollector = new FilteredElementCollector(document);
					IList<Element> list2 = filteredElementCollector.OfClass(typeof(CurveElement)).OfCategory(BuiltInCategory.OST_Lines).ToElements();
					foreach (Element item in list2)
					{
						if (item == null || item.Category == null || item.Category.Id.Value64() != BuiltInCategory.OST_Lines.GetHashCode())
						{
							continue;
						}
						for (int i = 0; i < m_GenericModelElementList.Count; i++)
						{
							if (m_GenericModelElementList[i].IndexOfFamilyInstanceUniqueID(document, item.UniqueId) >= 0)
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							list.Add(item);
						}
						else
						{
							flag2 = false;
						}
					}
				}
				for (int j = 0; j < list.Count; j++)
				{
					if (string.Compare(parameter2.AsString(), list[j].UniqueId, ignoreCase: true) != 0)
					{
						continue;
					}
					parameter3 = familyInstance.LookupParameter(RevitParameters.BoundaryIslandIDGenericFamilyParameter);
					if (parameter3 == null || parameter3.AsString() == string.Empty)
					{
						parameter3 = familyInstance.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
						if (parameter3 != null && parameter3.AsString() != string.Empty)
						{
							for (int k = 0; k < m_GenericModelElementList.Count; k++)
							{
								if (string.Compare(parameter3.AsString(), m_GenericModelElementList[k].ChainID, ignoreCase: true) == 0)
								{
									flag3 = true;
									m_GenericModelElementList[k].FamilyContainerList.Add(new GenericModelFamilyInstanceContainer(list[j].Id, familyInstance));
									break;
								}
							}
							if (!flag3)
							{
								List<GenericModelFamilyInstanceContainer> list3 = new List<GenericModelFamilyInstanceContainer>();
								list3.Add(new GenericModelFamilyInstanceContainer(list[j].Id, familyInstance));
								m_GenericModelElementList.Add(new GenericModelFamilyInstanceWrapper(parameter3.AsString(), list3));
							}
							else
							{
								flag3 = false;
							}
						}
					}
					else
					{
						for (int l = 0; l < m_GenericModelElementList.Count; l++)
						{
							if (m_GenericModelElementList[l].IndexOfFamilyInstanceUniqueID(document, parameter3.AsString()) >= 0)
							{
								if (m_GenericModelElementList[l].IslandFamilyInstanceList == null)
								{
									m_GenericModelElementList[l].IslandFamilyInstanceList = new List<GenericModelFamilyInstanceContainer>();
								}
								m_GenericModelElementList[l].IslandFamilyInstanceList.Add(new GenericModelFamilyInstanceContainer(list[j].Id, familyInstance));
								break;
							}
						}
					}
					flag = true;
					break;
				}
			}
			else
			{
				if (familyInstance.Category.Id.Value64() == BuiltInCategory.OST_GenericModel.GetHashCode() && parameter == null)
				{
					continue;
				}
				parameter3 = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
				if (parameter3 != null && !AddFamilyAssignmentToList(document, parameter3.AsString(), familyInstance))
				{
					if (_GenericModelElementList == null)
					{
						_GenericModelElementList = new List<KeyValuePair<string, FamilyInstance>>();
					}
					_GenericModelElementList.Add(new KeyValuePair<string, FamilyInstance>(parameter3.AsString(), familyInstance));
					flag = true;
				}
			}
		}
		if (flag)
		{
			if (_GenericModelElementList != null)
			{
				for (int m = 0; m < _GenericModelElementList.Count; m++)
				{
					AddFamilyAssignmentToList(document, _GenericModelElementList[m].Key, _GenericModelElementList[m].Value);
				}
			}
			return;
		}
		bool flag4 = false;
		List<GenericModelFamilyInstanceWrapper> list4 = null;
		List<FamilyInstance> list5 = null;
		int num = -1;
		List<ElementId> _DeletedElementsList = new List<ElementId>();
		foreach (ElementId deletedElementId in _Data.GetDeletedElementIds())
		{
			_DeletedElementsList.Add(deletedElementId);
		}
		ElementId elementId = null;
		for (int n = 0; n < _DeletedElementsList.Count; n++)
		{
			elementId = _DeletedElementsList[n];
			for (int num2 = 0; num2 < m_GenericModelElementList.Count; num2++)
			{
				num = m_GenericModelElementList[num2].IndexOfContainerID(document, elementId);
				if (num >= 0)
				{
					bool flag5 = false;
					FamilyInstance familyInstance2 = m_GenericModelElementList[num2].FamilyContainerList[num].GetFamilyInstance(document);
					if (familyInstance2 != null)
					{
						parameter3 = familyInstance2.LookupParameter(RevitParameters.PlacementTypeGenericFamilyParameter);
						if (parameter3 != null)
						{
							foreach (int value in Enum.GetValues(typeof(PlacementType)))
							{
								if (parameter3.AsInteger() == value)
								{
									if ((PlacementType)Enum.ToObject(typeof(PlacementType), value) == PlacementType.GroveSquare || (PlacementType)Enum.ToObject(typeof(PlacementType), value) == PlacementType.GroveTrianglular)
									{
										flag5 = true;
									}
									break;
								}
							}
						}
						_GenericModelElementList = ((!flag5) ? m_GenericModelElementList[num2].GetFamilyAssignmentInstanceList(document, familyInstance2) : m_GenericModelElementList[num2].GetFamilyAssignmentInstanceList(document));
					}
					if (_GenericModelElementList != null)
					{
						int num4 = -1;
						for (int num5 = _GenericModelElementList.Count - 1; num5 >= 0; num5--)
						{
							num4 = m_GenericModelElementList[num2].IndexOfFamilyInstanceUniqueID(document, _GenericModelElementList[num5].Key);
							if (num4 >= 0 && (flag5 || num4 == num))
							{
								if (list5 == null)
								{
									list5 = new List<FamilyInstance>();
								}
								FamilyInstance familyInstance3 = m_GenericModelElementList[num2].FamilyContainerList[num4].GetFamilyInstance(document);
								if (familyInstance3 != null && !list5.Contains(familyInstance3))
								{
									list5.Add(familyInstance3);
								}
								int num6 = _DeletedElementsList.IndexOf(_GenericModelElementList[num5].Value.Id);
								if (num6 >= 0)
								{
									_DeletedElementsList.RemoveAt(num6);
									if (num6 < n)
									{
										n--;
									}
								}
								document.Delete(_GenericModelElementList[num5].Value.Id);
								_GenericModelElementList.RemoveAt(num5);
							}
						}
					}
					DeleteGenericModelsFromList(document, list5, num2);
					flag4 = true;
					break;
				}
				num = -1;
				if (m_GenericModelElementList[num2].IslandFamilyInstanceList != null && m_GenericModelElementList[num2].IslandFamilyInstanceList.Count > 0)
				{
					num = m_GenericModelElementList[num2].IndexOfIslandContainerID(document, elementId);
					if (num >= 0)
					{
						FamilyInstance familyInstance4 = m_GenericModelElementList[num2].IslandFamilyInstanceList[num].GetFamilyInstance(document);
						if (familyInstance4 != null)
						{
							parameter3 = familyInstance4.LookupParameter(RevitParameters.BoundaryIslandIDGenericFamilyParameter);
							if (parameter3 != null)
							{
								string strB = parameter3.AsString();
								for (int num7 = m_GenericModelElementList[num2].IslandFamilyInstanceList.Count - 1; num7 >= 0; num7--)
								{
									FamilyInstance familyInstance5 = m_GenericModelElementList[num2].IslandFamilyInstanceList[num7].GetFamilyInstance(document);
									if (familyInstance5 != null)
									{
										parameter3 = familyInstance5.LookupParameter(RevitParameters.BoundaryIslandIDGenericFamilyParameter);
										if (parameter3 != null && string.Compare(parameter3.AsString(), strB, ignoreCase: true) == 0)
										{
											document.Delete(familyInstance5.Id);
										}
									}
								}
							}
						}
						if (list4 == null)
						{
							list4 = new List<GenericModelFamilyInstanceWrapper>();
						}
						bool flag6 = false;
						for (int num8 = 0; num8 < list4.Count; num8++)
						{
							if (list4[num8].FamilyContainerList == null || list4[num8].FamilyContainerList.Count == 0)
							{
								continue;
							}
							for (int num9 = 0; num9 < list4[num8].FamilyContainerList.Count; num9++)
							{
								if (m_GenericModelElementList[num2].IndexOfContainerID(document, list4[num8].FamilyContainerList[num9].ID) >= 0)
								{
									flag6 = true;
									break;
								}
							}
							if (flag6)
							{
								break;
							}
						}
						if (!flag6)
						{
							list4.Add(m_GenericModelElementList[num2]);
						}
						flag4 = true;
						break;
					}
				}
				if (num >= 0)
				{
					continue;
				}
				num = m_GenericModelElementList[num2].IndexOfFamilyAssignmentInstanceID(document, elementId);
				if (num < 0)
				{
					continue;
				}
				FamilyInstance firstExistingFamilyInstance = m_GenericModelElementList[num2].GetFirstExistingFamilyInstance(document);
				if (firstExistingFamilyInstance == null)
				{
					break;
				}
				int _nFamilyDeletedCount = -1;
				bool familyAssignmentDeletedList = m_GenericModelElementList[num2].GetFamilyAssignmentDeletedList(document, ref _DeletedElementsList, ref _nFamilyDeletedCount);
				if (_nFamilyDeletedCount > 0)
				{
					if (n >= _nFamilyDeletedCount)
					{
						n -= _nFamilyDeletedCount;
					}
					if (familyAssignmentDeletedList)
					{
						DeleteGenericModelFromList(document, firstExistingFamilyInstance, num2);
						flag4 = true;
					}
				}
				break;
			}
		}
		List<string> _GenericModelAlreadyModified = new List<string>();
		if (flag4)
		{
			if (list4 == null)
			{
				return;
			}
			for (int num10 = 0; num10 < list4.Count; num10++)
			{
				if (list4[num10].FamilyContainerList != null && list4[num10].FamilyContainerList.Count > 0)
				{
					Element firstExistingElement = list4[num10].GetFirstExistingElement(document);
					if (firstExistingElement != null && firstExistingElement.Location is LocationCurve)
					{
						ModifyPlacement(document, firstExistingElement, list4[num10].GetFirstExistingFamilyInstance(document), list4[num10], ref _GenericModelElementList, ref _FamilyQueryLocateTyoeList, ref _GenericModelAlreadyModified);
					}
				}
			}
			return;
		}
		int num11 = -1;
		foreach (ElementId modifiedElementId in _Data.GetModifiedElementIds())
		{
			Element element2 = document.GetElement(modifiedElementId);
			LocationCurve locationCurve2 = element2.Location as LocationCurve;
			Curve curve = null;
			if (locationCurve2 != null)
			{
				curve = locationCurve2.Curve;
			}
			if (!(curve != null))
			{
				continue;
			}
			for (int num12 = 0; num12 < m_GenericModelElementList.Count; num12++)
			{
				num = m_GenericModelElementList[num12].IndexOfContainerID(document, modifiedElementId);
				if (num >= 0)
				{
					ModifyPlacement(document, m_GenericModelElementList[num12].GetFirstExistingElement(document, m_GenericModelElementList[num12].FamilyContainerList[num].ID), m_GenericModelElementList[num12].GetFirstExistingFamilyInstance(document, m_GenericModelElementList[num12].FamilyContainerList[num].ID), m_GenericModelElementList[num12], ref _GenericModelElementList, ref _FamilyQueryLocateTyoeList, ref _GenericModelAlreadyModified);
					continue;
				}
				num11 = m_GenericModelElementList[num12].IndexOfIslandContainerID(document, modifiedElementId);
				if (num11 >= 0)
				{
					ModifyPlacement(document, m_GenericModelElementList[num12].GetFirstExistingElement(document), m_GenericModelElementList[num12].GetFirstExistingFamilyInstance(document), m_GenericModelElementList[num12], ref _GenericModelElementList, ref _FamilyQueryLocateTyoeList, ref _GenericModelAlreadyModified);
				}
			}
		}
	}

	private void ModifyPlacement(Document _CurrentDocument, Element _CurrentElement, FamilyInstance _CurFamilyInstance, GenericModelFamilyInstanceWrapper _CurGenericModelPlantContainer, ref List<KeyValuePair<string, FamilyInstance>> _GenericModelElementList, ref List<LocateType> _FamilyQueryLocateTyoeList, ref List<string> _GenericModelAlreadyModified)
	{
		if (_CurrentElement == null || _CurFamilyInstance == null || _CurGenericModelPlantContainer.FamilyContainerList == null || _CurGenericModelPlantContainer.FamilyContainerList.Count == 0)
		{
			return;
		}
		for (int i = 0; i < _CurGenericModelPlantContainer.FamilyContainerList.Count; i++)
		{
			FamilyInstance familyInstance = _CurGenericModelPlantContainer.FamilyContainerList[i].GetFamilyInstance(_CurrentDocument);
			if (familyInstance != null && _GenericModelAlreadyModified.Contains(familyInstance.UniqueId))
			{
				return;
			}
		}
		FamilySymbol _FamilySymbol = null;
		Parameter parameter = null;
		LocateType locateType = LocateType.Plant;
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
		if (parameter != null)
		{
			foreach (int value in Enum.GetValues(typeof(LocateType)))
			{
				if (parameter.AsInteger() == value)
				{
					locateType = (LocateType)Enum.ToObject(typeof(LocateType), value);
					break;
				}
			}
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.SymbolIDGenericFamilyParameter);
		if (parameter != null)
		{
			GetFamilySymbolList(_CurrentDocument, locateType, parameter.AsString(), ref _FamilySymbol);
		}
		if (_FamilyQueryLocateTyoeList == null)
		{
			_FamilyQueryLocateTyoeList = new List<LocateType>();
		}
		if (_GenericModelElementList == null || _GenericModelElementList.Count == 0 || !_FamilyQueryLocateTyoeList.Contains(locateType))
		{
			GetFamilyInstanceList(_CurrentDocument, locateType, ref _GenericModelElementList);
			if (!_FamilyQueryLocateTyoeList.Contains(locateType))
			{
				_FamilyQueryLocateTyoeList.Add(locateType);
			}
		}
		if (_FamilySymbol == null)
		{
			return;
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.IsSingleSegmentGenericFamilyParameter);
		if (parameter == null)
		{
			return;
		}
		bool flag = Convert.ToBoolean(parameter.AsInteger());
		Eps_Leg _CurrentLeg = null;
		PlacementSettings _PlacementSettings = new PlacementSettings();
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.HeightGenericFamilyParameter);
		if (parameter != null)
		{
			if (RevitDefines.GE(parameter.AsDouble(), 1E-05))
			{
				_PlacementSettings.Height = parameter.AsDouble();
			}
			else
			{
				_PlacementSettings.Height = 1.0;
			}
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.HeightVarianceGenericFamilyParameter);
		if (parameter != null)
		{
			_PlacementSettings.HeightVariance = parameter.AsDouble();
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.WidthGenericFamilyParameter);
		if (parameter != null)
		{
			if (RevitDefines.GE(parameter.AsDouble(), 1E-05))
			{
				_PlacementSettings.Width = parameter.AsDouble();
			}
			else
			{
				_PlacementSettings.Width = 1.0;
			}
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.WidthVarianceGenericFamilyParameter);
		if (parameter != null)
		{
			_PlacementSettings.WidthVariance = parameter.AsDouble();
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.SpacingGenericFamilyParameter);
		if (parameter != null)
		{
			_PlacementSettings.PlantSpacing = parameter.AsDouble();
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.SetbackGenericFamilyParameter);
		if (parameter != null)
		{
			_PlacementSettings.SetbackSpacing = parameter.AsDouble();
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.GroveRotationGenericFamilyParameter);
		if (parameter != null)
		{
			_PlacementSettings.RotationAngle = parameter.AsDouble();
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.PlacementTypeGenericFamilyParameter);
		if (parameter != null)
		{
			foreach (int value2 in Enum.GetValues(typeof(PlacementType)))
			{
				if (parameter.AsInteger() == value2)
				{
					_PlacementSettings.PlacementType = (PlacementType)Enum.ToObject(typeof(PlacementType), value2);
					break;
				}
			}
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.UseRightSideGenericFamilyParameter);
		if (parameter != null)
		{
			_PlacementSettings.UseRightSide = Convert.ToBoolean(parameter.AsInteger());
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.AlignToGenericFamilyParameter);
		if (parameter != null)
		{
			_PlacementSettings.AlignTo = Convert.ToBoolean(parameter.AsInteger());
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.StartingDistanceGenericFamilyParameter);
		if (parameter != null)
		{
			_PlacementSettings.StartingDistance = parameter.AsDouble();
		}
		_PlacementSettings.Type = _FamilySymbol;
		if (_CurFamilyInstance.LevelId != null)
		{
			_PlacementSettings.Level = (Level)_CurrentDocument.GetElement(_CurFamilyInstance.LevelId);
		}
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.ToposurfaceIDFamilyParameter);
		if (parameter != null)
		{
			string text = parameter.AsString();
			if (!string.IsNullOrEmpty(text))
			{
				TopographySurface topographySurface = TopographySurfaceManagement.FindTopoSurfaceInstance(_CurrentDocument, text);
				TopographySurfaceContainer item = new TopographySurfaceContainer(_CurrentDocument.Application, topographySurface);
				if (_PlacementSettings.SurfaceList == null)
				{
					_PlacementSettings.SurfaceList = new List<TopographySurfaceContainer>();
				}
				_PlacementSettings.SurfaceList.Add(item);
			}
		}
		if (locateType == LocateType.ParkingLot)
		{
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.ADAStallWidthGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.ADAStallWidth = parameter.AsDouble();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.ADAStallCountGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.ADAStallCount = parameter.AsInteger();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.BumperHeightGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.BumperHeight = parameter.AsDouble();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.BumperWidthGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.BumperWidth = parameter.AsDouble();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.BumperOffsetGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.BumperOffset = parameter.AsDouble();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.BumperSetbackGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.BumperSetback = parameter.AsDouble();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.IslandHeightGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.IslandHeight = parameter.AsDouble();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.IslandWidthGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.IslandWidth = parameter.AsDouble();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.StallLengthGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.StallLength = parameter.AsDouble();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.AreaDistanceBetweenCenterlinesGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.AreaDistanceBetweenCenterlines = parameter.AsDouble();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.AreaAisleWidthGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.AreaAisleWidth = parameter.AsDouble();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.PlaceIslandGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.PlaceIsland = Convert.ToBoolean(parameter.AsInteger());
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.PlaceBumperGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.PlaceBumper = Convert.ToBoolean(parameter.AsInteger());
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.OptimizeParkingGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.OptimizeParking = Convert.ToBoolean(parameter.AsInteger());
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.AreaBidirectionalAislesGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.AreaBiDirectionalAisles = Convert.ToBoolean(parameter.AsInteger());
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.StallCountGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.StallCount = parameter.AsInteger();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.AreaRowCountGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.AreaRowCount = parameter.AsInteger();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.AreaRowSpacingOptionsGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.AreaRowSpacingOptions = parameter.AsInteger();
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.ADAStallSymbolIDGenericFamilyParameter);
			if (parameter != null)
			{
				FamilySymbol _FamilySymbol2 = null;
				GetFamilySymbolList(_CurrentDocument, locateType, parameter.AsString(), ref _FamilySymbol2);
				if (_FamilySymbol2 != null)
				{
					_PlacementSettings.Parking.ADAStallSymbol = _FamilySymbol2;
				}
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.IslandSymbolIDGenericFamilyParameter);
			if (parameter != null)
			{
				FamilySymbol _FamilySymbol3 = null;
				GetFamilySymbolList(_CurrentDocument, locateType, parameter.AsString(), ref _FamilySymbol3);
				if (_FamilySymbol3 != null)
				{
					_PlacementSettings.Parking.IslandSymbol = _FamilySymbol3;
				}
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.BumperSymbolIDGenericFamilyParameter);
			if (parameter != null)
			{
				FamilySymbol _FamilySymbol4 = null;
				GetFamilySymbolList(_CurrentDocument, locateType, parameter.AsString(), ref _FamilySymbol4);
				if (_FamilySymbol4 != null)
				{
					_PlacementSettings.Parking.BumperSymbol = _FamilySymbol4;
				}
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.MiddleStallPlacementGenericFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.MiddleStallPlacement = Convert.ToBoolean(parameter.AsInteger());
			}
			parameter = _CurFamilyInstance.LookupParameter(RevitParameters.ADAStallSharedAccessibleAisleGenericModelFamilyParameter);
			if (parameter != null)
			{
				_PlacementSettings.Parking.ADAStallSharedAccessibleAisle = Convert.ToBoolean(parameter.AsInteger());
			}
		}
		if (flag)
		{
			_CurrentLeg = RevitGeometry.GetLegFromElementList(null, _CurrentElement);
		}
		List<FamilyInstance> _FamilyInstanceList = new List<FamilyInstance>();
		List<FamilyInstance> _ParkingADAStallInstanceList = new List<FamilyInstance>();
		List<FamilyInstance> _ParkingBumperInstanceList = new List<FamilyInstance>();
		List<FamilyInstance> _ParkingIslandInstanceList = new List<FamilyInstance>();
		if (_CurrentLeg != null)
		{
			switch (locateType)
			{
			case LocateType.Plant:
				UpdateFamilyListsAndSettings(_CurrentDocument, locateType, _CurGenericModelPlantContainer.FamilyContainerList, ref _CurrentLeg, ref _GenericModelElementList, ref _GenericModelAlreadyModified, ref _PlacementSettings, ref _FamilyInstanceList);
				PlacePlantsOnLeg(_CurrentDocument, _CurrentLeg, _FamilyInstanceList, _CurGenericModelPlantContainer.IslandFamilyInstanceList, _PlacementSettings);
				break;
			case LocateType.Component:
				UpdateFamilyListsAndSettings(_CurrentDocument, locateType, _CurGenericModelPlantContainer.FamilyContainerList, ref _CurrentLeg, ref _GenericModelElementList, ref _GenericModelAlreadyModified, ref _PlacementSettings, ref _FamilyInstanceList);
				PlaceComponentsOnLeg(_CurrentDocument, _CurrentLeg, _FamilyInstanceList, _CurGenericModelPlantContainer.IslandFamilyInstanceList, _PlacementSettings);
				break;
			case LocateType.Fence:
				UpdateFamilyListsAndSettings(_CurrentDocument, locateType, _CurGenericModelPlantContainer.FamilyContainerList, ref _CurrentLeg, ref _GenericModelElementList, ref _GenericModelAlreadyModified, ref _PlacementSettings, ref _FamilyInstanceList);
				PlaceFenceOnLeg(_CurrentDocument, _CurrentLeg, _FamilyInstanceList, _PlacementSettings);
				break;
			case LocateType.ParkingLot:
				UpdateFamilyListsAndSettings(_CurrentDocument, locateType, _CurGenericModelPlantContainer.FamilyContainerList, ref _CurrentLeg, ref _GenericModelElementList, ref _GenericModelAlreadyModified, ref _PlacementSettings, ref _FamilyInstanceList, ref _ParkingADAStallInstanceList, ref _ParkingBumperInstanceList, ref _ParkingIslandInstanceList);
				PlaceParkingStallsOnLeg(_CurrentDocument, _CurrentLeg, _FamilyInstanceList, _ParkingADAStallInstanceList, _ParkingBumperInstanceList, _PlacementSettings);
				break;
			}
			return;
		}
		for (int num3 = _GenericModelElementList.Count - 1; num3 >= 0; num3--)
		{
			if (string.Compare(_GenericModelElementList[num3].Key, _CurFamilyInstance.UniqueId, ignoreCase: true) == 0)
			{
				if (locateType == LocateType.ParkingLot && _GenericModelElementList[num3].Value.Symbol != null)
				{
					parameter = _GenericModelElementList[num3].Value.Symbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter);
					if (parameter != null)
					{
						if (string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.ParkingStall).ToString("000"), ignoreCase: true) == 0)
						{
							_FamilyInstanceList.Add(_GenericModelElementList[num3].Value);
						}
						else if (string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.ADAParkingStall).ToString("000"), ignoreCase: true) == 0)
						{
							_ParkingADAStallInstanceList.Add(_GenericModelElementList[num3].Value);
						}
						else if (string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.ParkingBumper).ToString("000"), ignoreCase: true) == 0)
						{
							_ParkingBumperInstanceList.Add(_GenericModelElementList[num3].Value);
						}
						else if (string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.ParkingIsland).ToString("000"), ignoreCase: true) == 0)
						{
							_ParkingIslandInstanceList.Add(_GenericModelElementList[num3].Value);
						}
					}
				}
				else
				{
					_FamilyInstanceList.Add(_GenericModelElementList[num3].Value);
				}
				_GenericModelElementList.RemoveAt(num3);
			}
		}
		_GenericModelAlreadyModified.Add(_CurFamilyInstance.UniqueId);
		if (_PlacementSettings.GenericModelIDList == null)
		{
			_PlacementSettings.GenericModelIDList = new List<KeyValuePair<string, string>>();
		}
		_PlacementSettings.GenericModelIDList.Add(new KeyValuePair<string, string>(_CurrentElement.UniqueId, _CurFamilyInstance.UniqueId));
		parameter = _CurFamilyInstance.LookupParameter(RevitParameters.IsReverseGenericFamilyParameter);
		if (parameter != null)
		{
			_PlacementSettings.IsReverse = Convert.ToBoolean(parameter.AsInteger());
		}
		LocationCurve locationCurve = _CurrentElement.Location as LocationCurve;
		Curve curve = locationCurve.Curve;
		if (curve is Arc)
		{
			Arc arc = (Arc)curve;
			switch (locateType)
			{
			case LocateType.Plant:
				PlacePlantsOnArc(_CurrentDocument, _CurrentElement, arc, _FamilyInstanceList, _CurGenericModelPlantContainer.IslandFamilyInstanceList, _PlacementSettings);
				break;
			case LocateType.Component:
				PlaceComponentsOnArc(_CurrentDocument, _CurrentElement, arc, _FamilyInstanceList, _CurGenericModelPlantContainer.IslandFamilyInstanceList, _PlacementSettings);
				break;
			case LocateType.Fence:
				PlaceFenceOnArc(_CurrentDocument, _CurrentElement, arc, _FamilyInstanceList, _PlacementSettings);
				break;
			case LocateType.ParkingLot:
				PlaceParkingStallsOnArc(_CurrentDocument, _CurrentElement, arc, _FamilyInstanceList, _ParkingADAStallInstanceList, _ParkingBumperInstanceList, _PlacementSettings);
				break;
			}
		}
		else if (curve is Line)
		{
			Line line = (Line)curve;
			switch (locateType)
			{
			case LocateType.Plant:
				PlacePlantsOnLine(_CurrentDocument, _CurrentElement, line, _FamilyInstanceList, _CurGenericModelPlantContainer.IslandFamilyInstanceList, _PlacementSettings);
				break;
			case LocateType.Component:
				PlaceComponentsOnLine(_CurrentDocument, _CurrentElement, line, _FamilyInstanceList, _CurGenericModelPlantContainer.IslandFamilyInstanceList, _PlacementSettings);
				break;
			case LocateType.Fence:
				PlaceFenceOnLine(_CurrentDocument, _CurrentElement, line, _FamilyInstanceList, _PlacementSettings);
				break;
			case LocateType.ParkingLot:
				PlaceParkingStallsOnLine(_CurrentDocument, _CurrentElement, line, _FamilyInstanceList, _ParkingADAStallInstanceList, _ParkingBumperInstanceList, _ParkingIslandInstanceList, _PlacementSettings);
				break;
			}
		}
		else if (curve is Ellipse || curve is NurbSpline || curve is HermiteSpline)
		{
			switch (locateType)
			{
			case LocateType.Plant:
				PlacePlantsOnSpline(_CurrentDocument, curve, _CurrentElement, _FamilyInstanceList, _CurGenericModelPlantContainer.FamilyContainerList, _CurGenericModelPlantContainer.IslandFamilyInstanceList, _PlacementSettings, ref _GenericModelElementList, ref _GenericModelAlreadyModified);
				break;
			case LocateType.Component:
				PlaceComponentsOnSpline(_CurrentDocument, curve, _CurrentElement, _FamilyInstanceList, _CurGenericModelPlantContainer.FamilyContainerList, _CurGenericModelPlantContainer.IslandFamilyInstanceList, _PlacementSettings, ref _GenericModelElementList, ref _GenericModelAlreadyModified);
				break;
			case LocateType.Fence:
				PlaceFenceOnSpline(_CurrentDocument, curve, _CurrentElement, _FamilyInstanceList, _CurGenericModelPlantContainer.FamilyContainerList, _PlacementSettings, ref _GenericModelElementList, ref _GenericModelAlreadyModified);
				break;
			}
		}
	}

	private void UpdateFamilyListsAndSettings(Document _CurrentDocument, LocateType _LocateType, List<GenericModelFamilyInstanceContainer> PlantContainerList, ref Eps_Leg _CurrentLeg, ref List<KeyValuePair<string, FamilyInstance>> _GenericModelElementList, ref List<string> _GenericModelAlreadyModified, ref PlacementSettings _PlacementSettings, ref List<FamilyInstance> _FamilyInstanceList)
	{
		List<FamilyInstance> _ParkingADAStallInstanceList = null;
		List<FamilyInstance> _ParkingBumperInstanceList = null;
		List<FamilyInstance> _ParkingIslandInstanceList = null;
		UpdateFamilyListsAndSettings(_CurrentDocument, _LocateType, PlantContainerList, ref _CurrentLeg, ref _GenericModelElementList, ref _GenericModelAlreadyModified, ref _PlacementSettings, ref _FamilyInstanceList, ref _ParkingADAStallInstanceList, ref _ParkingBumperInstanceList, ref _ParkingIslandInstanceList);
	}

	private void UpdateFamilyListsAndSettings(Document _CurrentDocument, LocateType _LocateType, List<GenericModelFamilyInstanceContainer> _ContainerList, ref Eps_Leg _CurrentLeg, ref List<KeyValuePair<string, FamilyInstance>> _GenericModelElementList, ref List<string> _GenericModelAlreadyModified, ref PlacementSettings _PlacementSettings, ref List<FamilyInstance> _FamilyInstanceList, ref List<FamilyInstance> _ParkingADAStallInstanceList, ref List<FamilyInstance> _ParkingBumperInstanceList, ref List<FamilyInstance> _ParkingIslandInstanceList)
	{
		FamilyInstance familyInstance = null;
		Parameter parameter = null;
		for (int i = 0; i < _ContainerList.Count; i++)
		{
			for (int num = _GenericModelElementList.Count - 1; num >= 0; num--)
			{
				familyInstance = _ContainerList[i].GetFamilyInstance(_CurrentDocument);
				if (familyInstance != null && string.Compare(_GenericModelElementList[num].Key, familyInstance.UniqueId, ignoreCase: true) == 0)
				{
					if (_LocateType == LocateType.ParkingLot && _GenericModelElementList[num].Value.Symbol != null)
					{
						parameter = _GenericModelElementList[num].Value.Symbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter);
						if (parameter != null)
						{
							if (string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.ParkingStall).ToString("000"), ignoreCase: true) == 0)
							{
								_FamilyInstanceList.Add(_GenericModelElementList[num].Value);
							}
							else if (string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.ADAParkingStall).ToString("000"), ignoreCase: true) == 0)
							{
								_ParkingADAStallInstanceList.Add(_GenericModelElementList[num].Value);
							}
							else if (string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.ParkingBumper).ToString("000"), ignoreCase: true) == 0)
							{
								_ParkingBumperInstanceList.Add(_GenericModelElementList[num].Value);
							}
							else if (string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.ParkingIsland).ToString("000"), ignoreCase: true) == 0)
							{
								_ParkingIslandInstanceList.Add(_GenericModelElementList[num].Value);
							}
						}
					}
					else
					{
						_FamilyInstanceList.Add(_GenericModelElementList[num].Value);
					}
					_GenericModelElementList.RemoveAt(num);
				}
			}
		}
		List<KeyValuePair<string, string>> list = null;
		Parameter parameter2 = null;
		parameter = null;
		int num2 = 0;
		int num3 = -1;
		int num4 = -1;
		string firstPlacementSegmentID = string.Empty;
		Eps_Leg eps_Leg = null;
		for (eps_Leg = _CurrentLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Element != null)
			{
				for (int j = 0; j < _ContainerList.Count; j++)
				{
					familyInstance = _ContainerList[j].GetFamilyInstance(_CurrentDocument);
					if (familyInstance == null)
					{
						continue;
					}
					parameter2 = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
					if (parameter2 == null || string.Compare(eps_Leg.Element.UniqueId, parameter2.AsString(), ignoreCase: true) != 0)
					{
						continue;
					}
					parameter = familyInstance.LookupParameter(RevitParameters.IsFirstPlacementSegmentGenericFamilyParameter);
					if (parameter != null && Convert.ToBoolean(parameter.AsInteger()))
					{
						firstPlacementSegmentID = parameter2.AsString();
						Parameter parameter3 = familyInstance.LookupParameter(RevitParameters.IsReverseGenericFamilyParameter);
						if (parameter3 != null)
						{
							_PlacementSettings.IsReverse = Convert.ToBoolean(parameter3.AsInteger());
						}
					}
					if (num3 == -1 || num4 == -1)
					{
						parameter = familyInstance.LookupParameter(RevitParameters.IsFirstLegSegmentGenericFamilyParameter);
						if (parameter != null && Convert.ToBoolean(parameter.AsInteger()))
						{
							num3 = num2;
						}
						parameter = familyInstance.LookupParameter(RevitParameters.IsLastLegSegmentGenericFamilyParameter);
						if (parameter != null && Convert.ToBoolean(parameter.AsInteger()))
						{
							num4 = num2;
						}
					}
					if (list == null)
					{
						list = new List<KeyValuePair<string, string>>();
					}
					list.Add(new KeyValuePair<string, string>(parameter2.AsString(), familyInstance.UniqueId));
					_GenericModelAlreadyModified.Add(familyInstance.UniqueId);
					break;
				}
				num2++;
			}
		}
		if (num4 <= 0 && num3 == num2 - 1)
		{
			_CurrentLeg = _CurrentLeg.Reverse();
		}
		_PlacementSettings.GenericModelIDList = list;
		_PlacementSettings.FirstPlacementSegmentID = firstPlacementSegmentID;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_CurrentDocument);
		List<Element> list2 = filteredElementCollector.OfClass(typeof(Group)).OfCategory(BuiltInCategory.OST_IOSModelGroups).ToList();
		List<Group> list3 = new List<Group>();
		Group obj = null;
		familyInstance = null;
		bool flag = false;
		foreach (Element item in list2)
		{
			if (!(item is Group { Category: not null } obj2))
			{
				continue;
			}
			IList<ElementId> memberIds = obj2.GetMemberIds();
			foreach (ElementId item2 in memberIds)
			{
				Element element = _CurrentDocument.GetElement(item2);
				if (!(element is FamilyInstance familyInstance2))
				{
					continue;
				}
				for (int k = 0; k < _FamilyInstanceList.Count; k++)
				{
					if (string.Compare(familyInstance2.UniqueId, _FamilyInstanceList[k].UniqueId) == 0)
					{
						if (!flag)
						{
							flag = true;
							list3.Add(obj2);
						}
						_FamilyInstanceList.RemoveAt(k);
						_PlacementSettings.PlaceGroupTagChecked = true;
						break;
					}
				}
			}
			flag = false;
		}
		for (int l = 0; l < list3.Count; l++)
		{
			List<ElementId> list4 = list3[l].UngroupMembers().ToList();
			if (list4 != null && list4.Count > 0)
			{
				_CurrentDocument.Delete(list4);
			}
		}
	}

	private void PlacePlantsOnLine(Document _CurrentDocument, Element _CurrentElement, Line _Line, List<FamilyInstance> _PlantFamilyInstanceList, List<GenericModelFamilyInstanceContainer> _IslandFamilyInstanceList, PlacementSettings _PlantPlacementSettings)
	{
		if (_PlantPlacementSettings.PlacementType == PlacementType.Single)
		{
			Eps_Point _MidPoint = new Eps_Point();
			EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(new Eps_Point(_Line.GetEndPoint(0)), new Eps_Point(_Line.GetEndPoint(1)), ref _MidPoint);
			PlantCommon.PlaceSingleOnPoint(_CurrentDocument, _PlantPlacementSettings, _CurrentElement, _MidPoint, _PlantFamilyInstanceList[0]);
		}
		else
		{
			if (_PlantPlacementSettings.PlacementType != PlacementType.HedgeRowCenter && _PlantPlacementSettings.PlacementType != PlacementType.HedgeRowAlternate && _PlantPlacementSettings.PlacementType != PlacementType.HedgeRowSide)
			{
				return;
			}
			Eps_Line eps_Line = new Eps_Line();
			eps_Line.StartPnt = new Eps_Point(_Line.GetEndPoint(0));
			eps_Line.EndPnt = new Eps_Point(_Line.GetEndPoint(1));
			if (_PlantPlacementSettings.IsReverse)
			{
				eps_Line = eps_Line.Reverse();
			}
			if (_PlantPlacementSettings.PlacementType == PlacementType.HedgeRowCenter)
			{
				PlantCommon.PlaceCenterRowOnLine(_CurrentDocument, _PlantPlacementSettings, _CurrentElement, eps_Line, _PlantFamilyInstanceList);
				return;
			}
			if (_PlantPlacementSettings.PlacementType == PlacementType.HedgeRowAlternate)
			{
				PlantCommon.PlaceAltRowOnLine(_CurrentDocument, _PlantPlacementSettings, _CurrentElement, eps_Line, _PlantFamilyInstanceList);
				return;
			}
			Eps_Line _OffsetLine = new Eps_Line();
			if (_PlantPlacementSettings.UseRightSide)
			{
				EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlantPlacementSettings.SetbackSpacing, 1, ref _OffsetLine);
			}
			else
			{
				EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlantPlacementSettings.SetbackSpacing, 2, ref _OffsetLine);
			}
			PlantCommon.PlaceCenterRowOnLine(_CurrentDocument, _PlantPlacementSettings, _CurrentElement, _OffsetLine, _PlantFamilyInstanceList);
		}
	}

	private void PlacePlantsOnArc(Document _CurrentDocument, Element _CurrentElement, Arc _Arc, List<FamilyInstance> _PlantFamilyInstanceList, List<GenericModelFamilyInstanceContainer> _IslandFamilyInstanceList, PlacementSettings _PlantPlacementSettings)
	{
		if (_PlantPlacementSettings.PlacementType == PlacementType.Single)
		{
			Eps_Point _StaOffPnt = new Eps_Point();
			if (!_Arc.IsBound)
			{
				_StaOffPnt = new Eps_Point(_Arc.Center);
			}
			else
			{
				EPShared.eplGeoE_FindStaOffPointAlongArc(new Eps_Arc(_Arc), _Arc.Length / 2.0, 0.0, ref _StaOffPnt);
			}
			PlantCommon.PlaceSingleOnPoint(_CurrentDocument, _PlantPlacementSettings, _CurrentElement, _StaOffPnt, _PlantFamilyInstanceList[0]);
		}
		else if (_PlantPlacementSettings.PlacementType == PlacementType.GroveSquare || _PlantPlacementSettings.PlacementType == PlacementType.GroveTrianglular)
		{
			if (!_Arc.IsBound)
			{
				Eps_Leg eps_Leg = new Eps_Leg();
				eps_Leg.Arc = new Eps_Arc(_Arc);
				List<Eps_Leg> _IslandList = new List<Eps_Leg>();
				GetIslandList(_CurrentDocument, _IslandFamilyInstanceList, ref _IslandList);
				PlantCommon.PlaceHedgeGrove(_CurrentDocument, _PlantPlacementSettings, eps_Leg, _IslandList, _PlantFamilyInstanceList);
			}
		}
		else
		{
			if (_PlantPlacementSettings.PlacementType != PlacementType.HedgeRowCenter && _PlantPlacementSettings.PlacementType != PlacementType.HedgeRowAlternate && _PlantPlacementSettings.PlacementType != PlacementType.HedgeRowSide)
			{
				return;
			}
			Eps_Arc eps_Arc = new Eps_Arc(_Arc);
			if (_PlantPlacementSettings.IsReverse)
			{
				eps_Arc = eps_Arc.Reverse();
			}
			if (_PlantPlacementSettings.PlacementType == PlacementType.HedgeRowCenter)
			{
				PlantCommon.PlaceCenterRowOnArc(_CurrentDocument, _PlantPlacementSettings, _CurrentElement, eps_Arc, _PlantFamilyInstanceList);
				return;
			}
			if (_PlantPlacementSettings.PlacementType == PlacementType.HedgeRowAlternate)
			{
				PlantCommon.PlaceAltRowOnArc(_CurrentDocument, _PlantPlacementSettings, _CurrentElement, eps_Arc, _PlantFamilyInstanceList);
				return;
			}
			Eps_Arc _OffsetArc = new Eps_Arc();
			if (_PlantPlacementSettings.UseRightSide)
			{
				EPShared.eplGeoE_ConstructOffsetArc(eps_Arc, _PlantPlacementSettings.SetbackSpacing, 1, ref _OffsetArc);
			}
			else
			{
				EPShared.eplGeoE_ConstructOffsetArc(eps_Arc, _PlantPlacementSettings.SetbackSpacing, 2, ref _OffsetArc);
			}
			PlantCommon.PlaceCenterRowOnArc(_CurrentDocument, _PlantPlacementSettings, _CurrentElement, _OffsetArc, _PlantFamilyInstanceList);
		}
	}

	private void PlacePlantsOnLeg(Document _CurrentDocument, Eps_Leg _CurrentLeg, List<FamilyInstance> _PlantFamilyInstanceList, List<GenericModelFamilyInstanceContainer> _IslandFamilyInstanceList, PlacementSettings _PlantPlacementSettings)
	{
		if (_PlantPlacementSettings.PlacementType == PlacementType.Single)
		{
			PlantCommon.PlaceSingleOnLeg(_CurrentDocument, _PlantPlacementSettings, _CurrentLeg, _PlantFamilyInstanceList[0]);
		}
		else if (_PlantPlacementSettings.PlacementType == PlacementType.GroveSquare || _PlantPlacementSettings.PlacementType == PlacementType.GroveTrianglular)
		{
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_CurrentLeg.LegStartPnt, _CurrentLeg.LegEndPnt, 1E-05))
			{
				List<Eps_Leg> _IslandList = new List<Eps_Leg>();
				GetIslandList(_CurrentDocument, _IslandFamilyInstanceList, ref _IslandList);
				PlantCommon.PlaceHedgeGrove(_CurrentDocument, _PlantPlacementSettings, _CurrentLeg, _IslandList, _PlantFamilyInstanceList);
			}
		}
		else
		{
			if (_PlantPlacementSettings.PlacementType != PlacementType.HedgeRowCenter && _PlantPlacementSettings.PlacementType != PlacementType.HedgeRowAlternate && _PlantPlacementSettings.PlacementType != PlacementType.HedgeRowSide)
			{
				return;
			}
			if (_PlantPlacementSettings.GenericModelIDList != null && string.Compare(_CurrentLeg.Element.UniqueId, _PlantPlacementSettings.GenericModelIDList[0].Key, ignoreCase: true) != 0)
			{
				_PlantPlacementSettings.GenericModelIDList.Reverse();
			}
			if (_PlantPlacementSettings.PlacementType == PlacementType.HedgeRowCenter)
			{
				PlantCommon.PlaceCenterRowOnLeg(_CurrentDocument, _PlantPlacementSettings, _CurrentLeg, _PlantFamilyInstanceList);
				return;
			}
			if (_PlantPlacementSettings.PlacementType == PlacementType.HedgeRowAlternate)
			{
				PlantCommon.PlaceAltRowOnLeg(_CurrentDocument, _PlantPlacementSettings, _CurrentLeg, _PlantFamilyInstanceList);
				return;
			}
			Eps_Leg ppOffsetLegList = new Eps_Leg();
			if (_PlantPlacementSettings.UseRightSide)
			{
				RevitGeometry.OffsetLegList(_CurrentLeg, _PlantPlacementSettings.SetbackSpacing, 1, ref ppOffsetLegList, TrimClosedObject: true);
			}
			else
			{
				RevitGeometry.OffsetLegList(_CurrentLeg, _PlantPlacementSettings.SetbackSpacing, 2, ref ppOffsetLegList, TrimClosedObject: true);
			}
			PlantCommon.PlaceCenterRowOnLeg(_CurrentDocument, _PlantPlacementSettings, ppOffsetLegList, _CurrentLeg.ElementList, _PlantFamilyInstanceList);
		}
	}

	private void PlacePlantsOnSpline(Document _CurrentDocument, Curve _Curve, Element _CurrentElement, List<FamilyInstance> _FamilyInstanceList, List<GenericModelFamilyInstanceContainer> _FamilyContainerList, List<GenericModelFamilyInstanceContainer> _IslandFamilyInstanceList, PlacementSettings _PlacementSettings, ref List<KeyValuePair<string, FamilyInstance>> _GenericModelElementList, ref List<string> _GenericModelAlreadyModified)
	{
		if (_PlacementSettings.PlacementType == PlacementType.Single && _Curve is Ellipse && !_Curve.IsBound)
		{
			Ellipse ellipse = (Ellipse)_Curve;
			PlantCommon.PlaceSingleOnPoint(_CurrentDocument, _PlacementSettings, _CurrentElement, new Eps_Point(ellipse.Center), _FamilyInstanceList[0]);
			return;
		}
		IList<XYZ> list = _Curve.Tessellate();
		int count = list.Count;
		Eps_Leg _CurrentLeg = null;
		if (count > 1)
		{
			int num = 0;
			Eps_Point startPnt = new Eps_Point();
			Eps_Point eps_Point = new Eps_Point();
			Eps_Leg eps_Leg = null;
			for (num = 0; num < count; num += 2)
			{
				eps_Point = new Eps_Point(list[num]);
				if (num > 0)
				{
					eps_Leg = new Eps_Leg();
					eps_Leg.Line = new Eps_Line();
					eps_Leg.Line.StartPnt = startPnt;
					eps_Leg.Line.EndPnt = eps_Point;
					if (_CurrentLeg == null)
					{
						_CurrentLeg = eps_Leg;
						_CurrentLeg.Element = _CurrentElement;
					}
					else
					{
						_CurrentLeg.Last.Next = eps_Leg;
						_CurrentLeg.Last.Element = _CurrentElement;
					}
				}
				startPnt = eps_Point;
			}
			if (num == count)
			{
				eps_Leg = new Eps_Leg();
				eps_Leg.Line = new Eps_Line();
				eps_Leg.Line.StartPnt = startPnt;
				eps_Leg.Line.EndPnt = new Eps_Point(list[count - 1]);
				if (_CurrentLeg == null)
				{
					_CurrentLeg = eps_Leg;
					_CurrentLeg.Element = _CurrentElement;
				}
				else
				{
					_CurrentLeg.Last.Next = eps_Leg;
					_CurrentLeg.Last.Element = _CurrentElement;
				}
			}
		}
		if (_CurrentLeg != null)
		{
			UpdateFamilyListsAndSettings(_CurrentDocument, LocateType.Plant, _FamilyContainerList, ref _CurrentLeg, ref _GenericModelElementList, ref _GenericModelAlreadyModified, ref _PlacementSettings, ref _FamilyInstanceList);
			PlacePlantsOnLeg(_CurrentDocument, _CurrentLeg, _FamilyInstanceList, _IslandFamilyInstanceList, _PlacementSettings);
		}
	}

	private void PlaceComponentsOnLine(Document _CurrentDocument, Element _CurrentElement, Line _Line, List<FamilyInstance> _FamilyInstanceList, List<GenericModelFamilyInstanceContainer> _IslandFamilyInstanceList, PlacementSettings _PlacementSettings)
	{
		if (_PlacementSettings.PlacementType != PlacementType.HedgeRowCenter && _PlacementSettings.PlacementType != PlacementType.HedgeRowAlternate && _PlacementSettings.PlacementType != PlacementType.HedgeRowSide)
		{
			return;
		}
		Eps_Line eps_Line = new Eps_Line();
		eps_Line.StartPnt = new Eps_Point(_Line.GetEndPoint(0));
		eps_Line.EndPnt = new Eps_Point(_Line.GetEndPoint(1));
		if (_PlacementSettings.IsReverse)
		{
			eps_Line = eps_Line.Reverse();
		}
		if (_PlacementSettings.PlacementType == PlacementType.HedgeRowCenter)
		{
			ComponentCommon.PlaceCenterRowOnLine(_CurrentDocument, _PlacementSettings, _CurrentElement, eps_Line, _FamilyInstanceList);
			return;
		}
		if (_PlacementSettings.PlacementType == PlacementType.HedgeRowAlternate)
		{
			ComponentCommon.PlaceAltRowOnLine(_CurrentDocument, _PlacementSettings, _CurrentElement, eps_Line, _FamilyInstanceList);
			return;
		}
		Eps_Line _OffsetLine = new Eps_Line();
		if (_PlacementSettings.UseRightSide)
		{
			EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlacementSettings.SetbackSpacing, 1, ref _OffsetLine);
		}
		else
		{
			EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlacementSettings.SetbackSpacing, 2, ref _OffsetLine);
		}
		ComponentCommon.PlaceCenterRowOnLine(_CurrentDocument, _PlacementSettings, _CurrentElement, _OffsetLine, _FamilyInstanceList);
	}

	private void PlaceComponentsOnArc(Document _CurrentDocument, Element _CurrentElement, Arc _Arc, List<FamilyInstance> _FamilyInstanceList, List<GenericModelFamilyInstanceContainer> _IslandFamilyInstanceList, PlacementSettings _PlacementSettings)
	{
		if (_PlacementSettings.PlacementType == PlacementType.GroveSquare || _PlacementSettings.PlacementType == PlacementType.GroveTrianglular)
		{
			if (!_Arc.IsBound)
			{
				Eps_Leg eps_Leg = new Eps_Leg();
				eps_Leg.Arc = new Eps_Arc(_Arc);
				List<Eps_Leg> _IslandList = new List<Eps_Leg>();
				GetIslandList(_CurrentDocument, _IslandFamilyInstanceList, ref _IslandList);
				ComponentCommon.PlaceHedgeGrove(_CurrentDocument, _PlacementSettings, eps_Leg, _IslandList, _FamilyInstanceList);
			}
		}
		else
		{
			if (_PlacementSettings.PlacementType != PlacementType.HedgeRowCenter && _PlacementSettings.PlacementType != PlacementType.HedgeRowAlternate && _PlacementSettings.PlacementType != PlacementType.HedgeRowSide)
			{
				return;
			}
			Eps_Arc eps_Arc = new Eps_Arc(_Arc);
			if (_PlacementSettings.IsReverse)
			{
				eps_Arc = eps_Arc.Reverse();
			}
			if (_PlacementSettings.PlacementType == PlacementType.HedgeRowCenter)
			{
				ComponentCommon.PlaceCenterRowOnArc(_CurrentDocument, _PlacementSettings, _CurrentElement, eps_Arc, _FamilyInstanceList);
				return;
			}
			if (_PlacementSettings.PlacementType == PlacementType.HedgeRowAlternate)
			{
				ComponentCommon.PlaceAltRowOnArc(_CurrentDocument, _PlacementSettings, _CurrentElement, eps_Arc, _FamilyInstanceList);
				return;
			}
			Eps_Arc _OffsetArc = new Eps_Arc();
			if (_PlacementSettings.UseRightSide)
			{
				EPShared.eplGeoE_ConstructOffsetArc(eps_Arc, _PlacementSettings.SetbackSpacing, 1, ref _OffsetArc);
			}
			else
			{
				EPShared.eplGeoE_ConstructOffsetArc(eps_Arc, _PlacementSettings.SetbackSpacing, 2, ref _OffsetArc);
			}
			ComponentCommon.PlaceCenterRowOnArc(_CurrentDocument, _PlacementSettings, _CurrentElement, _OffsetArc, _FamilyInstanceList);
		}
	}

	private void PlaceComponentsOnLeg(Document _CurrentDocument, Eps_Leg _CurrentLeg, List<FamilyInstance> _FamilyInstanceList, List<GenericModelFamilyInstanceContainer> _IslandFamilyInstanceList, PlacementSettings _PlacementSettings)
	{
		if (_PlacementSettings.PlacementType == PlacementType.GroveSquare || _PlacementSettings.PlacementType == PlacementType.GroveTrianglular)
		{
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_CurrentLeg.LegStartPnt, _CurrentLeg.LegEndPnt, 1E-05))
			{
				List<Eps_Leg> _IslandList = new List<Eps_Leg>();
				GetIslandList(_CurrentDocument, _IslandFamilyInstanceList, ref _IslandList);
				ComponentCommon.PlaceHedgeGrove(_CurrentDocument, _PlacementSettings, _CurrentLeg, _IslandList, _FamilyInstanceList);
			}
		}
		else
		{
			if (_PlacementSettings.PlacementType != PlacementType.HedgeRowCenter && _PlacementSettings.PlacementType != PlacementType.HedgeRowAlternate && _PlacementSettings.PlacementType != PlacementType.HedgeRowSide)
			{
				return;
			}
			if (_PlacementSettings.GenericModelIDList != null && string.Compare(_CurrentLeg.Element.UniqueId, _PlacementSettings.GenericModelIDList[0].Key, ignoreCase: true) != 0)
			{
				_PlacementSettings.GenericModelIDList.Reverse();
			}
			if (_PlacementSettings.PlacementType == PlacementType.HedgeRowCenter)
			{
				ComponentCommon.PlaceCenterRowOnLeg(_CurrentDocument, _PlacementSettings, _CurrentLeg, _FamilyInstanceList);
				return;
			}
			if (_PlacementSettings.PlacementType == PlacementType.HedgeRowAlternate)
			{
				ComponentCommon.PlaceAltRowOnLeg(_CurrentDocument, _PlacementSettings, _CurrentLeg, _FamilyInstanceList);
				return;
			}
			Eps_Leg ppOffsetLegList = new Eps_Leg();
			if (_PlacementSettings.UseRightSide)
			{
				RevitGeometry.OffsetLegList(_CurrentLeg, _PlacementSettings.SetbackSpacing, 1, ref ppOffsetLegList, TrimClosedObject: true);
			}
			else
			{
				RevitGeometry.OffsetLegList(_CurrentLeg, _PlacementSettings.SetbackSpacing, 2, ref ppOffsetLegList, TrimClosedObject: true);
			}
			ComponentCommon.PlaceCenterRowOnLeg(_CurrentDocument, _PlacementSettings, ppOffsetLegList, _CurrentLeg.ElementList, _FamilyInstanceList);
		}
	}

	private void PlaceComponentsOnSpline(Document _CurrentDocument, Curve _Curve, Element _CurrentElement, List<FamilyInstance> _FamilyInstanceList, List<GenericModelFamilyInstanceContainer> _FamilyContainerList, List<GenericModelFamilyInstanceContainer> _IslandFamilyInstanceList, PlacementSettings _PlacementSettings, ref List<KeyValuePair<string, FamilyInstance>> _GenericModelElementList, ref List<string> _GenericModelAlreadyModified)
	{
		IList<XYZ> list = _Curve.Tessellate();
		int count = list.Count;
		Eps_Leg _CurrentLeg = null;
		if (count > 1)
		{
			int num = 0;
			Eps_Point startPnt = new Eps_Point();
			Eps_Point eps_Point = new Eps_Point();
			Eps_Leg eps_Leg = null;
			for (num = 0; num < count; num += 2)
			{
				eps_Point = new Eps_Point(list[num]);
				if (num > 0)
				{
					eps_Leg = new Eps_Leg();
					eps_Leg.Line = new Eps_Line();
					eps_Leg.Line.StartPnt = startPnt;
					eps_Leg.Line.EndPnt = eps_Point;
					if (_CurrentLeg == null)
					{
						_CurrentLeg = eps_Leg;
						_CurrentLeg.Element = _CurrentElement;
					}
					else
					{
						_CurrentLeg.Last.Next = eps_Leg;
						_CurrentLeg.Last.Element = _CurrentElement;
					}
				}
				startPnt = eps_Point;
			}
			if (num == count)
			{
				eps_Leg = new Eps_Leg();
				eps_Leg.Line = new Eps_Line();
				eps_Leg.Line.StartPnt = startPnt;
				eps_Leg.Line.EndPnt = new Eps_Point(list[count - 1]);
				if (_CurrentLeg == null)
				{
					_CurrentLeg = eps_Leg;
					_CurrentLeg.Element = _CurrentElement;
				}
				else
				{
					_CurrentLeg.Last.Next = eps_Leg;
					_CurrentLeg.Last.Element = _CurrentElement;
				}
			}
		}
		if (_CurrentLeg != null)
		{
			UpdateFamilyListsAndSettings(_CurrentDocument, LocateType.Component, _FamilyContainerList, ref _CurrentLeg, ref _GenericModelElementList, ref _GenericModelAlreadyModified, ref _PlacementSettings, ref _FamilyInstanceList);
			PlaceComponentsOnLeg(_CurrentDocument, _CurrentLeg, _FamilyInstanceList, _IslandFamilyInstanceList, _PlacementSettings);
		}
	}

	private void PlaceFenceOnLine(Document _CurrentDocument, Element _CurrentElement, Line _Line, List<FamilyInstance> _FamilyInstanceList, PlacementSettings _PlacementSettings)
	{
		Eps_Line eps_Line = new Eps_Line();
		eps_Line.StartPnt = new Eps_Point(_Line.GetEndPoint(0));
		eps_Line.EndPnt = new Eps_Point(_Line.GetEndPoint(1));
		if (_PlacementSettings.IsReverse)
		{
			eps_Line = eps_Line.Reverse();
		}
		if (RevitDefines.EQ(_PlacementSettings.SetbackSpacing, 0.0))
		{
			FenceCommon.PlaceCenterRowOnLine(_CurrentDocument, _PlacementSettings, _CurrentElement, eps_Line, _FamilyInstanceList);
			return;
		}
		Eps_Line _OffsetLine = new Eps_Line();
		if (_PlacementSettings.UseRightSide)
		{
			EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlacementSettings.SetbackSpacing, 1, ref _OffsetLine);
		}
		else
		{
			EPShared.eplGeoE_ConstructOffsetLine(eps_Line, _PlacementSettings.SetbackSpacing, 2, ref _OffsetLine);
		}
		FenceCommon.PlaceCenterRowOnLine(_CurrentDocument, _PlacementSettings, _CurrentElement, _OffsetLine, _FamilyInstanceList);
	}

	private void PlaceFenceOnArc(Document _CurrentDocument, Element _CurrentElement, Arc _Arc, List<FamilyInstance> _FamilyInstanceList, PlacementSettings _PlacementSettings)
	{
		Eps_Arc eps_Arc = new Eps_Arc(_Arc);
		if (_PlacementSettings.IsReverse)
		{
			eps_Arc = eps_Arc.Reverse();
		}
		if (RevitDefines.EQ(_PlacementSettings.SetbackSpacing, 0.0))
		{
			FenceCommon.PlaceCenterRowOnArc(_CurrentDocument, _PlacementSettings, _CurrentElement, eps_Arc, _FamilyInstanceList);
			return;
		}
		Eps_Arc _OffsetArc = new Eps_Arc();
		if (_PlacementSettings.UseRightSide)
		{
			EPShared.eplGeoE_ConstructOffsetArc(eps_Arc, _PlacementSettings.SetbackSpacing, 1, ref _OffsetArc);
		}
		else
		{
			EPShared.eplGeoE_ConstructOffsetArc(eps_Arc, _PlacementSettings.SetbackSpacing, 2, ref _OffsetArc);
		}
		FenceCommon.PlaceCenterRowOnArc(_CurrentDocument, _PlacementSettings, _CurrentElement, _OffsetArc, _FamilyInstanceList);
	}

	private void PlaceFenceOnLeg(Document _CurrentDocument, Eps_Leg _CurrentLeg, List<FamilyInstance> _FamilyInstanceList, PlacementSettings _PlacementSettings)
	{
		if (_PlacementSettings.GenericModelIDList != null && string.Compare(_CurrentLeg.Element.UniqueId, _PlacementSettings.GenericModelIDList[0].Key, ignoreCase: true) != 0)
		{
			_PlacementSettings.GenericModelIDList.Reverse();
		}
		if (RevitDefines.EQ(_PlacementSettings.SetbackSpacing, 0.0))
		{
			FenceCommon.PlaceCenterRowOnLeg(_CurrentDocument, _PlacementSettings, _CurrentLeg, _FamilyInstanceList);
			return;
		}
		Eps_Leg ppOffsetLegList = new Eps_Leg();
		if (_PlacementSettings.UseRightSide)
		{
			RevitGeometry.OffsetLegList(_CurrentLeg, _PlacementSettings.SetbackSpacing, 1, ref ppOffsetLegList, TrimClosedObject: true);
		}
		else
		{
			RevitGeometry.OffsetLegList(_CurrentLeg, _PlacementSettings.SetbackSpacing, 2, ref ppOffsetLegList, TrimClosedObject: true);
		}
		FenceCommon.PlaceCenterRowOnLeg(_CurrentDocument, _PlacementSettings, ppOffsetLegList, _CurrentLeg.ElementList, _FamilyInstanceList);
	}

	private void PlaceFenceOnSpline(Document _CurrentDocument, Curve _Curve, Element _CurrentElement, List<FamilyInstance> _FamilyInstanceList, List<GenericModelFamilyInstanceContainer> _FamilyContainerList, PlacementSettings _PlacementSettings, ref List<KeyValuePair<string, FamilyInstance>> _GenericModelElementList, ref List<string> _GenericModelAlreadyModified)
	{
		IList<XYZ> list = _Curve.Tessellate();
		int count = list.Count;
		Eps_Leg _CurrentLeg = null;
		if (count > 1)
		{
			int num = 0;
			Eps_Point startPnt = new Eps_Point();
			Eps_Point eps_Point = new Eps_Point();
			Eps_Leg eps_Leg = null;
			for (num = 0; num < count; num += 2)
			{
				eps_Point = new Eps_Point(list[num]);
				if (num > 0)
				{
					eps_Leg = new Eps_Leg();
					eps_Leg.Line = new Eps_Line();
					eps_Leg.Line.StartPnt = startPnt;
					eps_Leg.Line.EndPnt = eps_Point;
					if (_CurrentLeg == null)
					{
						_CurrentLeg = eps_Leg;
						_CurrentLeg.Element = _CurrentElement;
					}
					else
					{
						_CurrentLeg.Last.Next = eps_Leg;
						_CurrentLeg.Last.Element = _CurrentElement;
					}
				}
				startPnt = eps_Point;
			}
			if (num == count)
			{
				eps_Leg = new Eps_Leg();
				eps_Leg.Line = new Eps_Line();
				eps_Leg.Line.StartPnt = startPnt;
				eps_Leg.Line.EndPnt = new Eps_Point(list[count - 1]);
				if (_CurrentLeg == null)
				{
					_CurrentLeg = eps_Leg;
					_CurrentLeg.Element = _CurrentElement;
				}
				else
				{
					_CurrentLeg.Last.Next = eps_Leg;
					_CurrentLeg.Last.Element = _CurrentElement;
				}
			}
		}
		if (_CurrentLeg != null)
		{
			UpdateFamilyListsAndSettings(_CurrentDocument, LocateType.Fence, _FamilyContainerList, ref _CurrentLeg, ref _GenericModelElementList, ref _GenericModelAlreadyModified, ref _PlacementSettings, ref _FamilyInstanceList);
			PlaceFenceOnLeg(_CurrentDocument, _CurrentLeg, _FamilyInstanceList, _PlacementSettings);
		}
	}

	private void PlaceParkingStallsOnLine(Document _CurrentDocument, Element _CurrentElement, Line _Line, List<FamilyInstance> _FamilyInstanceList, List<FamilyInstance> _ParkingADAStallInstanceList, List<FamilyInstance> _ParkingBumperInstanceList, List<FamilyInstance> _ParkingIslandInstanceList, PlacementSettings _PlacementSettings)
	{
		Eps_Line eps_Line = new Eps_Line();
		eps_Line.StartPnt = new Eps_Point(_Line.GetEndPoint(0));
		eps_Line.EndPnt = new Eps_Point(_Line.GetEndPoint(1));
		Eps_Line eps_Line2 = null;
		Eps_Line eps_Line3 = null;
		if (_PlacementSettings.PlacementType == PlacementType.ParkingArea)
		{
			if (_PlacementSettings.IsReverse)
			{
				eps_Line = eps_Line.Reverse();
			}
			eps_Line3 = eps_Line;
			eps_Line2 = eps_Line;
		}
		else if (!_PlacementSettings.Parking.MiddleStallPlacement)
		{
			if (!_PlacementSettings.IsReverse)
			{
				eps_Line3 = eps_Line;
			}
			else
			{
				eps_Line2 = eps_Line.Reverse();
			}
		}
		else
		{
			Eps_Point _MidPoint = new Eps_Point();
			EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(eps_Line.StartPnt, eps_Line.EndPnt, ref _MidPoint);
			Eps_Line eps_Line4 = new Eps_Line();
			eps_Line4.StartPnt = eps_Line.StartPnt;
			eps_Line4.EndPnt = _MidPoint;
			if (eps_Line2 == null)
			{
				eps_Line2 = eps_Line4;
			}
			eps_Line4 = new Eps_Line();
			eps_Line4.StartPnt = _MidPoint;
			eps_Line4.EndPnt = eps_Line.EndPnt;
			if (eps_Line3 == null)
			{
				eps_Line3 = eps_Line4;
			}
			if (eps_Line2 != null)
			{
				eps_Line2 = eps_Line2.Reverse();
			}
		}
		ParkingLotCommon.PlaceStallsOnLine(_CurrentDocument, _PlacementSettings, _CurrentElement, eps_Line, eps_Line2, eps_Line3, _PlacementSettings.IsReverse, ref _FamilyInstanceList, ref _ParkingADAStallInstanceList, ref _ParkingBumperInstanceList, ref _ParkingIslandInstanceList);
	}

	private void PlaceParkingStallsOnArc(Document _CurrentDocument, Element _CurrentElement, Arc _Arc, List<FamilyInstance> _FamilyInstanceList, List<FamilyInstance> _ParkingADAStallInstanceList, List<FamilyInstance> _ParkingBumperInstanceList, PlacementSettings _PlacementSettings)
	{
		Eps_Arc eps_Arc = new Eps_Arc(_Arc);
		Eps_Arc eps_Arc2 = null;
		Eps_Arc eps_Arc3 = null;
		if (_PlacementSettings.PlacementType == PlacementType.ParkingArea)
		{
			if (_PlacementSettings.IsReverse)
			{
				eps_Arc = eps_Arc.Reverse();
			}
		}
		else if (!_PlacementSettings.Parking.MiddleStallPlacement)
		{
			if (!_PlacementSettings.IsReverse)
			{
				eps_Arc3 = eps_Arc;
			}
			else
			{
				eps_Arc2 = eps_Arc.Reverse();
			}
		}
		else
		{
			Eps_Point _StaOffPnt = new Eps_Point();
			double num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Arc.Radius, eps_Arc.Delta);
			EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Arc, num / 2.0, 0.0, ref _StaOffPnt);
			Eps_Arc eps_Arc4 = new Eps_Arc(eps_Arc);
			eps_Arc4.Delta = eps_Arc.Delta / 2.0;
			eps_Arc4.StartPnt = eps_Arc.StartPnt;
			eps_Arc4.EndPnt = _StaOffPnt;
			if (eps_Arc2 == null)
			{
				eps_Arc2 = eps_Arc4;
			}
			eps_Arc4 = new Eps_Arc(eps_Arc);
			eps_Arc4.Delta = eps_Arc.Delta / 2.0;
			eps_Arc4.StartPnt = _StaOffPnt;
			eps_Arc4.EndPnt = eps_Arc.EndPnt;
			if (eps_Arc3 == null)
			{
				eps_Arc3 = eps_Arc4;
			}
			if (eps_Arc2 != null)
			{
				eps_Arc2 = eps_Arc2.Reverse();
			}
		}
		ParkingLotCommon.PlaceStallsOnArc(_CurrentDocument, _PlacementSettings, _CurrentElement, eps_Arc, eps_Arc2, eps_Arc3, _PlacementSettings.IsReverse, ref _FamilyInstanceList, ref _ParkingADAStallInstanceList, ref _ParkingBumperInstanceList);
	}

	private void PlaceParkingStallsOnLeg(Document _CurrentDocument, Eps_Leg _CurrentLeg, List<FamilyInstance> _FamilyInstanceList, List<FamilyInstance> _ParkingADAStallInstanceList, List<FamilyInstance> _ParkingBumperInstanceList, PlacementSettings _PlacementSettings)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Point _StaOffPnt = new Eps_Point();
		Eps_Leg eps_Leg3 = null;
		Eps_Leg eps_Leg4 = null;
		bool flag = false;
		double num = 0.0;
		for (eps_Leg4 = _CurrentLeg; eps_Leg4 != null; eps_Leg4 = eps_Leg4.Next)
		{
			if (flag)
			{
				if (eps_Leg2 == null)
				{
					eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg4);
				}
				else
				{
					eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
				}
			}
			else
			{
				if (string.Compare(_PlacementSettings.FirstPlacementSegmentID, eps_Leg4.Element.UniqueId, ignoreCase: true) == 0)
				{
					flag = true;
				}
				if (eps_Leg4.Line != null)
				{
					if (flag && _PlacementSettings.Parking.MiddleStallPlacement)
					{
						EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(eps_Leg4.Line.StartPnt, eps_Leg4.Line.EndPnt, ref _StaOffPnt);
						eps_Leg3 = new Eps_Leg();
						eps_Leg3.Line = new Eps_Line();
						eps_Leg3.Line.StartPnt = eps_Leg4.Line.StartPnt;
						eps_Leg3.Line.EndPnt = _StaOffPnt;
						eps_Leg3.Element = eps_Leg4.Element;
						if (eps_Leg == null)
						{
							eps_Leg = Eps_Leg.SingleLeg(eps_Leg3);
						}
						else
						{
							eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg3);
						}
						eps_Leg3 = new Eps_Leg();
						eps_Leg3.Line = new Eps_Line();
						eps_Leg3.Line.StartPnt = _StaOffPnt;
						eps_Leg3.Line.EndPnt = eps_Leg4.Line.EndPnt;
						eps_Leg3.Element = eps_Leg4.Element;
						if (eps_Leg2 == null)
						{
							eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg3);
						}
						else
						{
							eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg3);
						}
					}
					else if (flag && !_PlacementSettings.IsReverse)
					{
						if (eps_Leg2 == null)
						{
							eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg4);
						}
						else
						{
							eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
						}
					}
					else if (eps_Leg == null)
					{
						eps_Leg = Eps_Leg.SingleLeg(eps_Leg4);
					}
					else
					{
						eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
					}
				}
				else if (eps_Leg4.Arc != null)
				{
					if (flag && _PlacementSettings.Parking.MiddleStallPlacement)
					{
						num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg4.Arc.Radius, eps_Leg4.Arc.Delta);
						EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg4.Arc, num / 2.0, 0.0, ref _StaOffPnt);
						eps_Leg3 = new Eps_Leg();
						eps_Leg3.Arc = new Eps_Arc(eps_Leg4.Arc);
						eps_Leg3.Arc.Delta = eps_Leg4.Arc.Delta / 2.0;
						eps_Leg3.Arc.StartPnt = eps_Leg4.Arc.StartPnt;
						eps_Leg3.Arc.EndPnt = _StaOffPnt;
						eps_Leg3.Element = eps_Leg4.Element;
						if (eps_Leg == null)
						{
							eps_Leg = Eps_Leg.SingleLeg(eps_Leg3);
						}
						else
						{
							eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg3);
						}
						eps_Leg3 = new Eps_Leg();
						eps_Leg3.Arc = new Eps_Arc(eps_Leg4.Arc);
						eps_Leg3.Arc.Delta = eps_Leg4.Arc.Delta / 2.0;
						eps_Leg3.Arc.StartPnt = _StaOffPnt;
						eps_Leg3.Arc.EndPnt = eps_Leg4.Arc.EndPnt;
						eps_Leg3.Element = eps_Leg4.Element;
						if (eps_Leg2 == null)
						{
							eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg3);
						}
						else
						{
							eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg3);
						}
					}
					else if (flag && !_PlacementSettings.IsReverse)
					{
						if (eps_Leg2 == null)
						{
							eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg4);
						}
						else
						{
							eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
						}
					}
					else if (eps_Leg == null)
					{
						eps_Leg = Eps_Leg.SingleLeg(eps_Leg4);
					}
					else
					{
						eps_Leg.Last.Next = Eps_Leg.SingleLeg(eps_Leg4);
					}
				}
			}
		}
		if (eps_Leg != null)
		{
			eps_Leg = eps_Leg.Reverse();
		}
		ParkingLotCommon.PlaceLegList(_CurrentDocument, _PlacementSettings, _CurrentLeg, eps_Leg, eps_Leg2, _PlacementSettings.IsReverse, _FamilyInstanceList, _ParkingADAStallInstanceList, _ParkingBumperInstanceList);
	}

	private void GetIslandList(Document _CurrentDocument, List<GenericModelFamilyInstanceContainer> _IslandGenericModelList, ref List<Eps_Leg> _IslandList)
	{
		if (_IslandGenericModelList == null)
		{
			return;
		}
		List<ElementId> list = null;
		for (int i = 0; i < _IslandGenericModelList.Count; i++)
		{
			if (list == null)
			{
				list = new List<ElementId>();
			}
			if (!list.Contains(_IslandGenericModelList[i].ID))
			{
				list.Add(_IslandGenericModelList[i].ID);
			}
		}
		if (list == null)
		{
			return;
		}
		Eps_Leg eps_Leg = null;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Element element = _CurrentDocument.GetElement(list[num]);
			if (element == null)
			{
				list.RemoveAt(num);
			}
			else if (!(element.Location is LocationCurve locationCurve))
			{
				list.RemoveAt(num);
			}
			else
			{
				eps_Leg = RevitGeometry.GetLegFromElementList(null, element);
				if (eps_Leg != null)
				{
					if (EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg.LegStartPnt, eps_Leg.LegEndPnt, 1E-05))
					{
						_IslandList.Add(eps_Leg);
					}
					for (int j = 0; j < eps_Leg.ElementList.Count; j++)
					{
						list.Remove(eps_Leg.ElementList[j].Id);
						if (j > 0)
						{
							num--;
						}
					}
				}
				else
				{
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
						else if (curve is Ellipse || curve is NurbSpline || curve is HermiteSpline)
						{
							IList<XYZ> list2 = curve.Tessellate();
							int count = list2.Count;
							if (count > 1)
							{
								int num2 = 0;
								Eps_Point startPnt = new Eps_Point();
								Eps_Point eps_Point = new Eps_Point();
								eps_Leg = null;
								Eps_Leg eps_Leg2 = null;
								for (num2 = 0; num2 < count; num2 += 2)
								{
									eps_Point = new Eps_Point(list2[num2]);
									if (num2 > 0)
									{
										eps_Leg2 = new Eps_Leg();
										eps_Leg2.Line = new Eps_Line();
										eps_Leg2.Line.StartPnt = startPnt;
										eps_Leg2.Line.EndPnt = eps_Point;
										if (eps_Leg == null)
										{
											eps_Leg = eps_Leg2;
											eps_Leg.Element = element;
										}
										else
										{
											eps_Leg.Last.Next = eps_Leg2;
										}
									}
									startPnt = eps_Point;
								}
								if (num2 == count)
								{
									eps_Leg2 = new Eps_Leg();
									eps_Leg2.Line = new Eps_Line();
									eps_Leg2.Line.StartPnt = startPnt;
									eps_Leg2.Line.EndPnt = new Eps_Point(list2[count - 1]);
									if (eps_Leg == null)
									{
										eps_Leg = eps_Leg2;
										eps_Leg.Element = element;
									}
									else
									{
										eps_Leg.Last.Next = eps_Leg2;
									}
								}
								_IslandList.Add(eps_Leg);
							}
						}
					}
					list.RemoveAt(num);
				}
			}
		}
	}

	private bool IsCategoryCorrect(FamilySymbol _FamilySymbol, LocateType _LocateType)
	{
		if (_FamilySymbol != null && _FamilySymbol.Category != null)
		{
			switch (_LocateType)
			{
			case LocateType.Plant:
				if (_FamilySymbol.Category.Id.Value64() == BuiltInCategory.OST_Planting.GetHashCode())
				{
					return true;
				}
				break;
			case LocateType.Component:
				if (_FamilySymbol.Category.Id.Value64() == BuiltInCategory.OST_Site.GetHashCode() || _FamilySymbol.Category.Id.Value64() == BuiltInCategory.OST_Entourage.GetHashCode() || _FamilySymbol.Category.Id.Value64() == BuiltInCategory.OST_Furniture.GetHashCode() || _FamilySymbol.Category.Id.Value64() == BuiltInCategory.OST_ElectricalEquipment.GetHashCode() || _FamilySymbol.Category.Id.Value64() == BuiltInCategory.OST_GenericModel.GetHashCode() || _FamilySymbol.Category.Id.Value64() == BuiltInCategory.OST_LightingFixtures.GetHashCode() || _FamilySymbol.Category.Id.Value64() == BuiltInCategory.OST_Columns.GetHashCode())
				{
					return true;
				}
				break;
			case LocateType.Fence:
				if (_FamilySymbol.Category.Id.Value64() == BuiltInCategory.OST_GenericModel.GetHashCode())
				{
					Parameter parameter = _FamilySymbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter);
					if (parameter != null && string.Compare(parameter.AsString(), Convert.ToInt32(LandcaddID.Fence).ToString("000"), ignoreCase: true) == 0)
					{
						return true;
					}
				}
				break;
			case LocateType.ParkingLot:
				if (_FamilySymbol.Category.Id.Value64() == BuiltInCategory.OST_Parking.GetHashCode())
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	private void GetFamilyInstanceList(Document _CurrentDocument, LocateType _LocateType, ref List<KeyValuePair<string, FamilyInstance>> _FamilyInstanceList)
	{
		if (_FamilyInstanceList == null)
		{
			_FamilyInstanceList = new List<KeyValuePair<string, FamilyInstance>>();
		}
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_CurrentDocument);
		switch (_LocateType)
		{
		case LocateType.Plant:
			filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Planting);
			break;
		case LocateType.Component:
		{
			FilteredElementCollector filteredElementCollector2 = new FilteredElementCollector(_CurrentDocument);
			FilteredElementCollector filteredElementCollector3 = new FilteredElementCollector(_CurrentDocument);
			FilteredElementCollector filteredElementCollector4 = new FilteredElementCollector(_CurrentDocument);
			FilteredElementCollector filteredElementCollector5 = new FilteredElementCollector(_CurrentDocument);
			FilteredElementCollector filteredElementCollector6 = new FilteredElementCollector(_CurrentDocument);
			FilteredElementCollector filteredElementCollector7 = new FilteredElementCollector(_CurrentDocument);
			filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site).UnionWith(filteredElementCollector2.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Entourage).UnionWith(filteredElementCollector3.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Furniture).UnionWith(filteredElementCollector4.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_ElectricalEquipment).UnionWith(filteredElementCollector5.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_GenericModel).UnionWith(filteredElementCollector6.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_LightingFixtures).UnionWith(filteredElementCollector7.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Columns)))))));
			break;
		}
		case LocateType.Fence:
			filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_GenericModel);
			break;
		case LocateType.ParkingLot:
			filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Parking);
			break;
		}
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => CurrentElement.LookupParameter(RevitParameters.PlacementIDFamilyParameter) != null);
		IList<Element> list = source.ToList();
		bool flag = false;
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Symbol: not null } familyInstance) || familyInstance.Symbol.Category == null || !IsCategoryCorrect(familyInstance.Symbol, _LocateType))
			{
				continue;
			}
			Parameter parameter = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
			if (parameter == null || !(parameter.AsString() != string.Empty))
			{
				continue;
			}
			for (int num = 0; num < _FamilyInstanceList.Count; num++)
			{
				if (string.Compare(_FamilyInstanceList[num].Value.UniqueId, familyInstance.UniqueId, ignoreCase: true) == 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_FamilyInstanceList.Add(new KeyValuePair<string, FamilyInstance>(parameter.AsString(), familyInstance));
			}
			else
			{
				flag = false;
			}
		}
	}

	private void GetFamilySymbolList(Document _CurrentDocument, LocateType _LocateType, string _strSymbolID, ref FamilySymbol _FamilySymbol)
	{
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_CurrentDocument);
		List<Element> list = new List<Element>();
		switch (_LocateType)
		{
		case LocateType.Plant:
		{
			filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Planting);
			IEnumerable<Element> source4 = filteredElementCollector.Where((Element CurrentElement) => CurrentElement.UniqueId == _strSymbolID);
			list = source4.ToList();
			break;
		}
		case LocateType.Component:
		{
			filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Site);
			IEnumerable<Element> source3 = filteredElementCollector.Where((Element CurrentElement) => CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter) == null && CurrentElement.UniqueId == _strSymbolID);
			list = source3.ToList();
			FilteredElementCollector filteredElementCollector2 = new FilteredElementCollector(_CurrentDocument);
			FilteredElementCollector filteredElementCollector3 = new FilteredElementCollector(_CurrentDocument);
			FilteredElementCollector filteredElementCollector4 = new FilteredElementCollector(_CurrentDocument);
			FilteredElementCollector filteredElementCollector5 = new FilteredElementCollector(_CurrentDocument);
			FilteredElementCollector filteredElementCollector6 = new FilteredElementCollector(_CurrentDocument);
			FilteredElementCollector filteredElementCollector7 = new FilteredElementCollector(_CurrentDocument);
			filteredElementCollector2.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Entourage).UnionWith(filteredElementCollector3.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Furniture).UnionWith(filteredElementCollector4.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_ElectricalEquipment).UnionWith(filteredElementCollector6.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_LightingFixtures).UnionWith(filteredElementCollector7.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Columns)))));
			source3 = filteredElementCollector.Where((Element CurrentElement) => CurrentElement.UniqueId == _strSymbolID);
			List<Element> collection = source3.ToList();
			list.AddRange(collection);
			filteredElementCollector5.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel);
			source3 = filteredElementCollector5.Where((Element CurrentElement) => CurrentElement.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter) != null && CurrentElement.UniqueId == _strSymbolID);
			collection = source3.ToList();
			list.AddRange(collection);
			break;
		}
		case LocateType.Fence:
		{
			filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel);
			Parameter CurLanddCadID = null;
			IEnumerable<Element> source2 = filteredElementCollector.Where((Element CurrentElement) => CurrentElement.UniqueId == _strSymbolID && (CurLanddCadID = CurrentElement.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter)) != null && string.Compare(CurLanddCadID.AsString(), Convert.ToInt32(LandcaddID.Fence).ToString("000"), ignoreCase: true) == 0);
			list = source2.ToList();
			break;
		}
		case LocateType.ParkingLot:
		{
			filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Parking);
			IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => CurrentElement.UniqueId == _strSymbolID);
			list = source.ToList();
			break;
		}
		}
		foreach (Element item in list)
		{
			if (item is FamilySymbol { Category: not null } familySymbol && IsCategoryCorrect(familySymbol, _LocateType) && string.Compare(familySymbol.UniqueId, _strSymbolID, ignoreCase: true) == 0)
			{
				_FamilySymbol = familySymbol;
				break;
			}
		}
	}

	public void UpdateGenericModelElementList(string _strCurrentToposurfaceID, string _strNewToposurfaceID, Document _CurrentDocument)
	{
		FamilyInstance familyInstance = null;
		Parameter parameter = null;
		List<KeyValuePair<string, FamilyInstance>> _GenericModelElementList = null;
		List<LocateType> _FamilyQueryLocateTyoeList = null;
		List<string> _GenericModelAlreadyModified = new List<string>();
		foreach (GenericModelFamilyInstanceWrapper genericModelElement in m_GenericModelElementList)
		{
			foreach (GenericModelFamilyInstanceContainer familyContainer in genericModelElement.FamilyContainerList)
			{
				familyInstance = familyContainer.GetFamilyInstance(_CurrentDocument);
				if (familyInstance != null)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.ToposurfaceIDFamilyParameter);
					if (parameter != null && string.Compare(parameter.AsString(), _strCurrentToposurfaceID, ignoreCase: false) == 0)
					{
						parameter.Set(_strNewToposurfaceID);
						ModifyPlacement(_CurrentDocument, genericModelElement.GetFirstExistingElement(_CurrentDocument, familyInstance.Id), familyInstance, genericModelElement, ref _GenericModelElementList, ref _FamilyQueryLocateTyoeList, ref _GenericModelAlreadyModified);
					}
				}
			}
		}
	}

	public void AddElements(Document _CurrentDocument)
	{
		m_nDocumentsOpenCount++;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_CurrentDocument);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_GenericModel);
		Element element = filteredElementCollector.FirstElement();
		if (element == null)
		{
			return;
		}
		Parameter LandcaddIDParameter = null;
		FamilyInstance FindFamilyInstance = null;
		List<Element> list = null;
		try
		{
			IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (LandcaddIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter)) != null && string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.FamilyInstancePlacement).ToString("000"), ignoreCase: true) == 0 && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
			list = source.ToList();
		}
		catch (InternalException)
		{
			return;
		}
		catch (Autodesk.Revit.Exceptions.ApplicationException)
		{
			return;
		}
		List<KeyValuePair<string, GenericModelFamilyInstanceContainer>> list2 = new List<KeyValuePair<string, GenericModelFamilyInstanceContainer>>();
		LandcaddIDParameter = null;
		Parameter parameter = null;
		Parameter parameter2 = null;
		bool flag = false;
		bool flag2 = false;
		filteredElementCollector = new FilteredElementCollector(_CurrentDocument);
		IList<Element> list3 = filteredElementCollector.OfClass(typeof(CurveElement)).OfCategory(BuiltInCategory.OST_Lines).ToElements();
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_GenericModel.GetHashCode())
			{
				continue;
			}
			LandcaddIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.LandcaddIDGenericModelFamilyParameter);
			if (LandcaddIDParameter == null || string.Compare(LandcaddIDParameter.AsString(), Convert.ToInt32(LandcaddID.FamilyInstancePlacement).ToString("000"), ignoreCase: true) != 0)
			{
				continue;
			}
			parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
			if (parameter == null)
			{
				continue;
			}
			parameter2 = familyInstance.LookupParameter(RevitParameters.BoundaryIslandIDGenericFamilyParameter);
			if (parameter2 == null || parameter2.AsString() == string.Empty)
			{
				flag2 = false;
				parameter2 = familyInstance.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
			}
			else
			{
				flag2 = true;
			}
			if (parameter2 == null || !(parameter2.AsString() != string.Empty))
			{
				continue;
			}
			for (int num = 0; num < list3.Count; num++)
			{
				Element element2 = list3[num];
				if (element2 == null || element2.Category == null || element2.Category.Id.Value64() != BuiltInCategory.OST_Lines.GetHashCode() || string.Compare(parameter.AsString(), element2.UniqueId, ignoreCase: true) != 0)
				{
					continue;
				}
				if (!flag2)
				{
					for (int num2 = 0; num2 < m_GenericModelElementList.Count; num2++)
					{
						if (string.Compare(parameter2.AsString(), m_GenericModelElementList[num2].ChainID, ignoreCase: true) == 0)
						{
							flag = true;
							m_GenericModelElementList[num2].FamilyContainerList.Add(new GenericModelFamilyInstanceContainer(element2.Id, familyInstance));
							break;
						}
					}
					if (!flag)
					{
						List<GenericModelFamilyInstanceContainer> list4 = new List<GenericModelFamilyInstanceContainer>();
						list4.Add(new GenericModelFamilyInstanceContainer(element2.Id, familyInstance));
						m_GenericModelElementList.Add(new GenericModelFamilyInstanceWrapper(parameter2.AsString(), list4));
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					list2.Add(new KeyValuePair<string, GenericModelFamilyInstanceContainer>(parameter2.AsString(), new GenericModelFamilyInstanceContainer(element2.Id, familyInstance)));
				}
				break;
			}
		}
		for (int num3 = 0; num3 < m_GenericModelElementList.Count; num3++)
		{
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				if (m_GenericModelElementList[num3].IndexOfFamilyInstanceUniqueID(_CurrentDocument, list2[num4].Key) >= 0)
				{
					if (m_GenericModelElementList[num3].IslandFamilyInstanceList == null)
					{
						m_GenericModelElementList[num3].IslandFamilyInstanceList = new List<GenericModelFamilyInstanceContainer>();
					}
					m_GenericModelElementList[num3].IslandFamilyInstanceList.Add(list2[num4].Value);
				}
			}
		}
		List<KeyValuePair<string, FamilyInstance>> list5 = new List<KeyValuePair<string, FamilyInstance>>();
		filteredElementCollector = new FilteredElementCollector(_CurrentDocument);
		FilteredElementCollector filteredElementCollector2 = new FilteredElementCollector(_CurrentDocument);
		FilteredElementCollector filteredElementCollector3 = new FilteredElementCollector(_CurrentDocument);
		FilteredElementCollector filteredElementCollector4 = new FilteredElementCollector(_CurrentDocument);
		FilteredElementCollector filteredElementCollector5 = new FilteredElementCollector(_CurrentDocument);
		FilteredElementCollector filteredElementCollector6 = new FilteredElementCollector(_CurrentDocument);
		FilteredElementCollector filteredElementCollector7 = new FilteredElementCollector(_CurrentDocument);
		FilteredElementCollector filteredElementCollector8 = new FilteredElementCollector(_CurrentDocument);
		FilteredElementCollector filteredElementCollector9 = new FilteredElementCollector(_CurrentDocument);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Planting).UnionWith(filteredElementCollector7.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Parking).UnionWith(filteredElementCollector6.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site).UnionWith(filteredElementCollector2.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Entourage).UnionWith(filteredElementCollector3.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Furniture).UnionWith(filteredElementCollector4.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_ElectricalEquipment).UnionWith(filteredElementCollector5.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_GenericModel).UnionWith(filteredElementCollector8.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_LightingFixtures).UnionWith(filteredElementCollector9.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Columns)))))))));
		IEnumerable<Element> source2 = filteredElementCollector.Where((Element CurrentElement) => CurrentElement.LookupParameter(RevitParameters.PlacementIDFamilyParameter) != null);
		IList<Element> list6 = source2.ToList();
		foreach (Element item2 in list6)
		{
			if (item2 is FamilyInstance { Symbol: not null } familyInstance2 && familyInstance2.Symbol.Category != null)
			{
				parameter2 = familyInstance2.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
				if (parameter2 != null && parameter2.AsString() != string.Empty)
				{
					list5.Add(new KeyValuePair<string, FamilyInstance>(parameter2.AsString(), familyInstance2));
				}
			}
		}
		for (int num5 = 0; num5 < m_GenericModelElementList.Count; num5++)
		{
			for (int num6 = 0; num6 < list5.Count; num6++)
			{
				if (m_GenericModelElementList[num5].IndexOfFamilyInstanceUniqueID(_CurrentDocument, list5[num6].Key) >= 0)
				{
					if (m_GenericModelElementList[num5].FamilyAssignmentList == null)
					{
						m_GenericModelElementList[num5].FamilyAssignmentList = new List<FamilyInstanceAssignmentContainer>();
					}
					m_GenericModelElementList[num5].FamilyAssignmentList.Add(new FamilyInstanceAssignmentContainer(list5[num6].Value.Id, list5[num6].Value));
				}
			}
		}
	}

	public void RemoveElements(Document _CurrentDocument)
	{
		m_nDocumentsOpenCount--;
		bool flag = false;
		for (int num = m_GenericModelElementList.Count - 1; num >= 0; num--)
		{
			flag = false;
			if (m_GenericModelElementList[num].FamilyContainerList != null)
			{
				for (int num2 = m_GenericModelElementList[num].FamilyContainerList.Count - 1; num2 >= 0; num2--)
				{
					FamilyInstance familyInstance = m_GenericModelElementList[num].FamilyContainerList[num2].GetFamilyInstance(_CurrentDocument);
					if (familyInstance != null && RevitUtility.CompareFileNames(familyInstance.Document.PathName, _CurrentDocument.PathName) == 0 && RevitUtility.CompareFileNames(familyInstance.Document.Title, _CurrentDocument.Title) == 0)
					{
						m_GenericModelElementList[num].FamilyContainerList.RemoveAt(num2);
						if (m_GenericModelElementList[num].FamilyContainerList.Count == 0)
						{
							m_GenericModelElementList.RemoveAt(num);
							flag = true;
							break;
						}
					}
				}
			}
			if (!flag && m_GenericModelElementList.Count > 0 && m_GenericModelElementList[num] != null)
			{
				if (m_GenericModelElementList[num].IslandFamilyInstanceList != null)
				{
					for (int num3 = m_GenericModelElementList[num].IslandFamilyInstanceList.Count - 1; num3 >= 0; num3--)
					{
						FamilyInstance familyInstance2 = m_GenericModelElementList[num].IslandFamilyInstanceList[num3].GetFamilyInstance(_CurrentDocument);
						if (familyInstance2 != null && RevitUtility.CompareFileNames(familyInstance2.Document.PathName, _CurrentDocument.PathName) == 0 && RevitUtility.CompareFileNames(familyInstance2.Document.Title, _CurrentDocument.Title) == 0)
						{
							m_GenericModelElementList[num].IslandFamilyInstanceList.RemoveAt(num3);
						}
					}
				}
				if (m_GenericModelElementList[num].FamilyAssignmentList != null)
				{
					for (int num4 = m_GenericModelElementList[num].FamilyAssignmentList.Count - 1; num4 >= 0; num4--)
					{
						FamilyInstance familyInstance3 = m_GenericModelElementList[num].FamilyAssignmentList[num4].GetFamilyInstance(_CurrentDocument);
						if (familyInstance3 != null && RevitUtility.CompareFileNames(familyInstance3.Document.PathName, _CurrentDocument.PathName) == 0 && RevitUtility.CompareFileNames(familyInstance3.Document.Title, _CurrentDocument.Title) == 0)
						{
							m_GenericModelElementList[num].FamilyAssignmentList.RemoveAt(num4);
						}
					}
				}
			}
		}
	}

	private bool AddFamilyAssignmentToList(Document _CurrentDocument, string _strPlacementID, FamilyInstance _FamilyInstance)
	{
		bool flag = false;
		bool result = false;
		int num = -1;
		for (int i = 0; i < m_GenericModelElementList.Count; i++)
		{
			num = m_GenericModelElementList[i].IndexOfFamilyInstanceUniqueID(_CurrentDocument, _strPlacementID);
			if (num < 0)
			{
				continue;
			}
			if (m_GenericModelElementList[i].FamilyAssignmentList == null)
			{
				m_GenericModelElementList[i].FamilyAssignmentList = new List<FamilyInstanceAssignmentContainer>();
			}
			if (m_GenericModelElementList[i].IndexOfFamilyAssignmentInstanceID(_CurrentDocument, _FamilyInstance.Id) < 0)
			{
				for (int j = 0; j < m_GenericModelElementList[i].FamilyAssignmentList.Count; j++)
				{
					if (_FamilyInstance.Id.Value64() == m_GenericModelElementList[i].FamilyAssignmentList[j].ID.Value64())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					m_GenericModelElementList[i].FamilyAssignmentList.Add(new FamilyInstanceAssignmentContainer(_FamilyInstance.Id, _FamilyInstance));
				}
			}
			result = true;
			break;
		}
		return result;
	}

	private void DeleteGenericModelFromList(Document _CurrentDocument, FamilyInstance _DeleteFamilyInstance, int _nListIndex)
	{
		List<FamilyInstance> list = new List<FamilyInstance>();
		list.Add(_DeleteFamilyInstance);
		DeleteGenericModelsFromList(_CurrentDocument, list, _nListIndex);
	}

	private void DeleteGenericModelsFromList(Document _CurrentDocument, List<FamilyInstance> _DeleteFamilyInstanceList, int _nListIndex)
	{
		if (_DeleteFamilyInstanceList == null)
		{
			return;
		}
		for (int i = 0; i < _DeleteFamilyInstanceList.Count; i++)
		{
			if (_DeleteFamilyInstanceList[i] != null)
			{
				Parameter parameter = _DeleteFamilyInstanceList[i].LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
				if (parameter != null)
				{
					string strB = parameter.AsString();
					if (m_GenericModelElementList[_nListIndex].FamilyContainerList != null && m_GenericModelElementList[_nListIndex].FamilyContainerList.Count > 0)
					{
						for (int num = m_GenericModelElementList[_nListIndex].FamilyContainerList.Count - 1; num >= 0; num--)
						{
							FamilyInstance familyInstance = m_GenericModelElementList[_nListIndex].FamilyContainerList[num].GetFamilyInstance(_CurrentDocument);
							if (familyInstance != null && !_DeleteFamilyInstanceList.Contains(familyInstance))
							{
								parameter = familyInstance.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
								if (parameter != null && string.Compare(parameter.AsString(), strB, ignoreCase: true) == 0)
								{
									_CurrentDocument.Delete(familyInstance.Id);
								}
							}
						}
					}
					if (m_GenericModelElementList[_nListIndex].IslandFamilyInstanceList != null && m_GenericModelElementList[_nListIndex].IslandFamilyInstanceList.Count > 0)
					{
						for (int num2 = m_GenericModelElementList[_nListIndex].IslandFamilyInstanceList.Count - 1; num2 >= 0; num2--)
						{
							FamilyInstance familyInstance2 = m_GenericModelElementList[_nListIndex].IslandFamilyInstanceList[num2].GetFamilyInstance(_CurrentDocument);
							if (familyInstance2 != null && !_DeleteFamilyInstanceList.Contains(familyInstance2))
							{
								parameter = familyInstance2.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
								if (parameter != null && string.Compare(parameter.AsString(), strB, ignoreCase: true) == 0)
								{
									_CurrentDocument.Delete(familyInstance2.Id);
								}
							}
						}
					}
				}
			}
			_CurrentDocument.Delete(_DeleteFamilyInstanceList[i].Id);
		}
	}

	public string GetAdditionalInformation()
	{
		return "This will handle Symbol Placement Generic Models.";
	}

	public ChangePriority GetChangePriority()
	{
		return ChangePriority.DetailComponents;
	}

	public UpdaterId GetUpdaterId()
	{
		return m_UpdaterId;
	}

	public string GetUpdaterName()
	{
		return "Symbol Placement Updater";
	}

	public int DocumentsOpenCount()
	{
		return m_nDocumentsOpenCount;
	}
}
