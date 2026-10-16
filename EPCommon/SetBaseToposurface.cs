using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using SITES.Utilities;

namespace EPCommon;

public class SetBaseToposurface
{
	private Document m_Document;

	private SetBaseToposurfaceForm m_DisplayForm;

	private ElementSet m_InstanceSet;

	private EPTransactionContainer m_TransactionContainer;

	private FamilySymbol m_PlacementSymbol;

	private List<TopographySurfaceContainer> m_SurfaceList;

	private TopographySurface m_ProposedSurface;

	private TopographySurface m_OriginalGroundSurface;

	private FamilyInstance m_ProposedFamilyInstance;

	private FamilyInstance m_OriginalGroundFamilyInstance;

	private bool m_bPickIndividualSurface;

	private bool m_HideOriginalTopoSurface;

	public Document Document => m_Document;

	public ReadOnlyCollection<TopographySurfaceContainer> SurfaceList => new ReadOnlyCollection<TopographySurfaceContainer>(m_SurfaceList);

	public ElementSet InstanceSet => m_InstanceSet;

	public EPTransactionContainer TransactionContainer => m_TransactionContainer;

	public bool PickIndividualSurface => m_bPickIndividualSurface;

	public SetBaseToposurface(Document _CurDocument)
	{
		m_Document = _CurDocument;
		m_bPickIndividualSurface = false;
		SetDefaults();
	}

	public SetBaseToposurface(Document _CurDocument, bool _bPickIndividualSurface)
	{
		m_Document = _CurDocument;
		m_bPickIndividualSurface = _bPickIndividualSurface;
		SetDefaults();
	}

	private void SetDefaults()
	{
		m_TransactionContainer = new EPTransactionContainer(m_Document);
		m_SurfaceList = new List<TopographySurfaceContainer>();
		m_InstanceSet = new ElementSet();
		m_ProposedSurface = null;
		m_OriginalGroundSurface = null;
		m_PlacementSymbol = null;
		InitializeListData();
	}

	private void InitializeListData()
	{
		PopulateFamilyAndSymbolLists();
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		Parameter parameter = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.TopoSurfacePlacement).ToString("000"), ignoreCase: true) == 0 && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		filteredElementCollector = new FilteredElementCollector(m_Document);
		IList<Element> list2 = filteredElementCollector.OfClass(TerrainApi.ElementType).OfCategory(TerrainApi.Category).ToElements();
		filteredElementCollector = new FilteredElementCollector(m_Document);
		IList<Element> list3 = filteredElementCollector.OfClass(typeof(CeilingAndFloor)).OfCategory(BuiltInCategory.OST_BuildingPad).ToElements();
		List<TopographySurface> list4 = new List<TopographySurface>();
		List<TopographySurfaceContainer> list5 = new List<TopographySurfaceContainer>();
		foreach (Element item in list2)
		{
			TopographySurface topographySurface = TerrainApi.AsSurface(item);
			if (!m_bPickIndividualSurface)
			{
				foreach (Element item2 in list)
				{
					if (!(item2 is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
					{
						continue;
					}
					SiteworksIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
					if (SiteworksIDParameter == null || string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.TopoSurfacePlacement).ToString("000"), ignoreCase: true) != 0)
					{
						continue;
					}
					parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
					if (parameter == null || string.Compare(parameter.AsString(), item.UniqueId, ignoreCase: true) != 0)
					{
						continue;
					}
					parameter = familyInstance.LookupParameter(RevitParameters.IsProposedSurfaceFamilyParameter);
					if (parameter == null)
					{
						continue;
					}
					if (Convert.ToBoolean(parameter.AsInteger()))
					{
						m_ProposedFamilyInstance = familyInstance;
						m_ProposedSurface = topographySurface;
						continue;
					}
					parameter = familyInstance.LookupParameter(RevitParameters.BaseToposurfaceCreatedFamilyParameter);
					if (parameter != null && Convert.ToBoolean(parameter.AsInteger()))
					{
						m_OriginalGroundSurface = topographySurface;
					}
					m_OriginalGroundFamilyInstance = familyInstance;
				}
			}
			list4.Add(topographySurface);
		}
		for (int num = 0; num < list4.Count; num++)
		{
			TopographySurfaceType topographySurfaceType = TopographySurfaceContainer.FindTopographySurfaceType(m_Document.Application, list4[num]);
			switch (topographySurfaceType)
			{
			case TopographySurfaceType.MainSurface:
				m_SurfaceList.Add(new TopographySurfaceContainer(m_Document.Application, list4[num]));
				break;
			case TopographySurfaceType.Subregion:
			case TopographySurfaceType.BuildingPad:
				list5.Add(new TopographySurfaceContainer(m_Document.Application, list4[num], _bFindMinMaxPoints: true, topographySurfaceType));
				break;
			}
		}
		foreach (Element item3 in list3)
		{
			if (item3 is CeilingAndFloor buildingPad)
			{
				list5.Add(new TopographySurfaceContainer(m_Document.Application, buildingPad, _bFindMinMaxPoints: true));
			}
		}
		TopographySurfaceManagement.AddSubRegionBuildingPadsToMainSurfaceList(m_Document, list5, ref m_SurfaceList);
	}

	private void PopulateFamilyAndSymbolLists()
	{
		if (m_bPickIndividualSurface)
		{
			return;
		}
		Parameter CurSiteworksID = null;
		FamilySymbol familySymbol = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Site);
		CurSiteworksID = null;
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (CurSiteworksID = CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(LandcaddID.TopoSurfacePlacement).ToString("000"), ignoreCase: true) == 0);
		List<Element> list = source.ToList();
		bool flag = false;
		familySymbol = null;
		foreach (Element item in list)
		{
			if (!(item is FamilySymbol { Category: not null } familySymbol2))
			{
				continue;
			}
			CurSiteworksID = familySymbol2.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (CurSiteworksID != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32(LandcaddID.TopoSurfacePlacement).ToString("000"), ignoreCase: true) == 0)
			{
				m_PlacementSymbol = familySymbol2;
				flag = true;
				if (m_PlacementSymbol != null && !m_PlacementSymbol.IsActive)
				{
					m_TransactionContainer.Start("ActivateFamilySymbol");
					m_PlacementSymbol.Activate();
					m_TransactionContainer.Commit();
				}
				break;
			}
		}
		string _strFullFamilyFilename = string.Empty;
		if (flag || !RevitUtility.GetFullFamilyFilenameIfExist("Toposurface Placement.rfa", ref _strFullFamilyFilename))
		{
			return;
		}
		Family _Family = null;
		m_TransactionContainer.Start(EPShared.LocalRM.GetString("LoadTopoPlacementFamilyTransaction") + Path.GetFileName(_strFullFamilyFilename));
		RevitUtility.LoadFamily(m_Document, _strFullFamilyFilename, out _Family, _bUseFailureHandling: true, ref m_TransactionContainer);
		m_TransactionContainer.Commit();
		if (_Family == null || _Family.GetFamilySymbolIds() == null)
		{
			return;
		}
		using IEnumerator<ElementId> enumerator2 = _Family.GetFamilySymbolIds().GetEnumerator();
		if (enumerator2.MoveNext())
		{
			ElementId current2 = enumerator2.Current;
			if (m_Document.GetElement(current2) is FamilySymbol placementSymbol)
			{
				m_PlacementSymbol = placementSymbol;
			}
			if (m_PlacementSymbol != null && !m_PlacementSymbol.IsActive)
			{
				m_TransactionContainer.Start("ActivateFamilySymbol");
				m_PlacementSymbol.Activate();
				m_TransactionContainer.Commit();
			}
		}
	}

	public bool Run()
	{
		return DisplayUI();
	}

	private bool DisplayUI()
	{
		bool result = false;
		if (m_SurfaceList.Count == 0)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("ToposurfacNotInProjectMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
			return false;
		}
		if (m_bPickIndividualSurface && m_OriginalGroundFamilyInstance != null)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("BaseToposurfaceAlreadySeteMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
		}
		DialogResult _DialogResult = DialogResult.Retry;
		using (m_DisplayForm = new SetBaseToposurfaceForm(this))
		{
			while (_DialogResult == DialogResult.Retry)
			{
				result = ShowDialog(ref _DialogResult);
			}
			return result;
		}
	}

	private bool PickTopographySurfaces(ref ElementSet _InstanceSet)
	{
		ElementSet _Elements = null;
		if (!m_bPickIndividualSurface)
		{
			if (!RevitUtility.GetElementsFromProject(m_Document, EPShared.LocalRM.GetString("PickToposurfacesPrompt"), ref _Elements, _bDisplayError: true, PickMethod.PickObjects) || _Elements == null || _Elements.Size == 0)
			{
				return false;
			}
		}
		else if (!RevitUtility.GetElementsFromProject(m_Document, EPShared.LocalRM.GetString("PickToposurfacePrompt"), ref _Elements) || _Elements == null || _Elements.Size == 0)
		{
			return false;
		}
		TopographySurface topographySurface = null;
		bool flag = false;
		foreach (Element item in _Elements)
		{
			if (TerrainApi.AsSurface(item) is { Category: not null } topographySurface2 && topographySurface2.Category.Id.Value64() == TerrainApi.Category.GetHashCode())
			{
				for (int i = 0; i < m_SurfaceList.Count; i++)
				{
					if (string.Compare(topographySurface2.UniqueId, m_SurfaceList[i].TopographySurface.UniqueId, ignoreCase: true) == 0)
					{
						flag = true;
						break;
					}
					for (int j = 0; j < m_SurfaceList[i].SubregionBuildingPadList.Count; j++)
					{
						if (string.Compare(topographySurface2.UniqueId, m_SurfaceList[i].SubregionBuildingPadList[j].TopographySurface.UniqueId, ignoreCase: true) == 0)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					_InstanceSet.Insert(item);
				}
				else
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("ToposurfaceAlreadyPickedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				}
			}
			else
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("ObjectSelectedNotToposurfaceMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
		}
		return true;
	}

	private bool ShowDialog(ref DialogResult _DialogResult)
	{
		bool result = false;
		m_DisplayForm.ShowDialog();
		_DialogResult = m_DisplayForm.DialogResult;
		if (_DialogResult == DialogResult.OK)
		{
			if (!m_bPickIndividualSurface)
			{
				CreateBaseToposurface(m_DisplayForm.SelectedSurfaceList);
			}
			else if (m_DisplayForm.SelectedSurfaceList != null && m_DisplayForm.SelectedSurfaceList.Count > 0)
			{
				m_InstanceSet.Clear();
				m_InstanceSet.Insert(m_DisplayForm.SelectedSurfaceList[0].TopographySurface);
			}
			result = true;
		}
		else if (_DialogResult == DialogResult.Cancel)
		{
			result = false;
		}
		else if (_DialogResult == DialogResult.Retry)
		{
			ElementSet _InstanceSet = new ElementSet();
			if (!PickTopographySurfaces(ref _InstanceSet))
			{
				return false;
			}
			m_InstanceSet = _InstanceSet;
			result = true;
			m_DisplayForm.SelectListItems();
		}
		return result;
	}

	public bool CreateBaseToposurface(List<TopographySurfaceContainer> _SurfaceList)
	{
		if (TerrainApi.UsesToposolids && _SurfaceList != null && _SurfaceList.Count > 1)
		{
			RevitMessageBox.Show("Select one Toposolid as the base terrain. Combine or edit its native sketch in Revit before setting the base terrain.", "Site Designer");
			return false;
		}
		if (_SurfaceList != null && _SurfaceList.Count > 0)
		{
			for (int i = 0; i < _SurfaceList.Count; i++)
			{
				if (_SurfaceList[i].BuildingPadPresentInDiffentPhase)
				{
					DialogResult dialogResult = RevitMessageBox.Show(EPShared.LocalRM.GetString("DifferentPhasingForBuildingPadsMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
					if (dialogResult == DialogResult.Yes)
					{
						break;
					}
					return false;
				}
			}
			List<Eps_Point> surfacePoints = TopographySurfaceManagement.GetSurfacePoints(_SurfaceList);
			if (surfacePoints.Count > 0)
			{
				List<TopographySurfaceContainer> list = new List<TopographySurfaceContainer>();
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				for (int j = 0; !TerrainApi.UsesToposolids && j < _SurfaceList.Count; j++)
				{
					if (!flag && _SurfaceList[j].IsTopographySurfaceTypePresent(TopographySurfaceType.Subregion))
					{
						flag = true;
						DialogResult dialogResult2 = RevitMessageBox.Show(EPShared.LocalRM.GetString("AddSubRegionsToProposedSurfaceMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
						if (dialogResult2 == DialogResult.Yes)
						{
							flag3 = true;
						}
					}
					if (!flag2 && _SurfaceList[j].IsTopographySurfaceTypePresent(TopographySurfaceType.BuildingPad))
					{
						flag2 = true;
						DialogResult dialogResult3 = RevitMessageBox.Show(EPShared.LocalRM.GetString("AddBuildingPadsToProposedSurfaceMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
						if (dialogResult3 == DialogResult.Yes)
						{
							flag4 = true;
						}
					}
					for (int k = 0; k < _SurfaceList[j].SubregionBuildingPadList.Count; k++)
					{
						if (_SurfaceList[j].SubregionBuildingPadList[k] != null && ((_SurfaceList[j].SubregionBuildingPadList[k].TopographySurfaceType == TopographySurfaceType.Subregion && flag3) || (_SurfaceList[j].SubregionBuildingPadList[k].TopographySurfaceType == TopographySurfaceType.BuildingPad && flag4)) && (_SurfaceList[j].SubregionBuildingPadList[k].TopographySurface != null || _SurfaceList[j].SubregionBuildingPadList[k].BuildingPad != null))
						{
							list.Add(_SurfaceList[j].SubregionBuildingPadList[k]);
						}
					}
				}
				// Native subdivisions are copied with their host; the legacy detached
				// subregion/array workflow would duplicate them and alter host points.
				if (TerrainApi.UsesToposolids) list.Clear();
				ElementId elementId = null;
				ElementId phaseCreated = null;
				ElementId phaseDemolished = null;
				string strToposurfaceName = string.Empty;
				using (PhasePickerForm phasePickerForm = new PhasePickerForm(m_Document))
				{
					if (phasePickerForm.ShowDialog() != DialogResult.OK)
					{
						return false;
					}
					m_HideOriginalTopoSurface = phasePickerForm.HideOriginalTopoSurface;
					if (phasePickerForm.CreatedPhaseId != -1)
					{
						long createdPhaseId = phasePickerForm.CreatedPhaseId;
						phaseCreated = RevitIds.Create(createdPhaseId);
					}
					else
					{
						phaseCreated = RevitIds.Create(32440);
					}
					strToposurfaceName = phasePickerForm.ToposurfaceName;
				}
				List<Element> list2 = new List<Element>();
				if (m_PlacementSymbol != null)
				{
					list2.Add(m_PlacementSymbol);
				}
				if (m_OriginalGroundSurface != null && (_SurfaceList.Count > 1 || (_SurfaceList.Count == 1 && string.Compare(_SurfaceList[0].TopographySurface.UniqueId, m_OriginalGroundSurface.UniqueId, ignoreCase: true) != 0)))
				{
					list2.Add(m_OriginalGroundSurface);
				}
				if (m_OriginalGroundFamilyInstance != null)
				{
					list2.Add(m_OriginalGroundFamilyInstance);
				}
				if (m_ProposedSurface != null && (_SurfaceList.Count > 1 || (_SurfaceList.Count == 1 && string.Compare(_SurfaceList[0].TopographySurface.UniqueId, m_ProposedSurface.UniqueId, ignoreCase: true) != 0)))
				{
					list2.Add(m_ProposedSurface);
				}
				if (m_ProposedFamilyInstance != null)
				{
					list2.Add(m_ProposedFamilyInstance);
				}
				if (!TopographySurfaceManagement.VerifyUserCanManipulateElements(m_Document, list2))
				{
					return false;
				}
				Parameter parameter = null;
				Parameter parameter2 = null;
				List<XYZ> PointXYZList = new List<XYZ>();
				List<Eps_Point> list3 = new List<Eps_Point>();
				double _dOffset = 0.0;
				double _dStation = 0.0;
				bool flag5 = false;
				bool flag6 = false;
				for (int num = surfacePoints.Count - 1; num >= 0; num--)
				{
					for (int l = 0; l < list.Count; l++)
					{
						if (list[l] == null)
						{
							continue;
						}
						if (list[l].BoundaryLeg != null && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(surfacePoints[num], list[l].BoundaryLeg, ref _dStation, ref _dOffset) == 1700 && Math.Abs(_dOffset) <= 0.001)
						{
							flag6 = true;
							break;
						}
						if (list[l].GetSurfaceContainerContainingPoint(surfacePoints[num]) != null)
						{
							if (list[l].TopographySurfaceType == TopographySurfaceType.Subregion)
							{
								flag5 = true;
							}
							else
							{
								flag6 = true;
							}
							break;
						}
					}
					if (flag5)
					{
						list3.Add(surfacePoints[num]);
						surfacePoints.RemoveAt(num);
					}
					else if (!flag6)
					{
						PointXYZList.Add(surfacePoints[num].XYZ);
					}
					else
					{
						surfacePoints.RemoveAt(num);
					}
					flag6 = false;
					flag5 = false;
				}
				ProgressBarForm progressBarForm = new ProgressBarForm();
				progressBarForm.ShowForm();
				progressBarForm.Refresh();
				progressBarForm.MinValue = 0;
				progressBarForm.MaxValue = Convert.ToInt32(2);
				progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingNewBaseToposurfaceMessage");
				m_TransactionContainer.Start(EPShared.LocalRM.GetString("SetBaseToposurfaceTransaction"));
				progressBarForm.UpdateProgressBar(1);
				m_TransactionContainer.BeginTransaction();
				bool value = false;
				ElementSet elementSet = new ElementSet();
				TopographySurface topographySurface = null;
				long _nReturnValue = -1;
				if (ApplicationUtility.Settings.eplSetE_GetSetting(m_Document.PathName, "ProposedToposurfaceMaterialParameter", ref _nReturnValue))
				{
					elementId = RevitIds.Create(_nReturnValue);
					bool flag7 = false;
					FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
					IList<Element> list4 = filteredElementCollector.OfClass(typeof(Material)).OfCategory(BuiltInCategory.OST_Materials).ToElements();
					foreach (Element item in list4)
					{
						if (item.Id == elementId)
						{
							flag7 = true;
							break;
						}
					}
					if (!flag7)
					{
						_nReturnValue = -1;
					}
				}
				if (_nReturnValue == -1 && _SurfaceList.Count > 0)
				{
					elementId = TerrainApi.GetMaterialId(_SurfaceList[0].TopographySurface);
					_nReturnValue = elementId.Value64();
				}
				if (_SurfaceList.Count > 1)
				{
					value = true;
					TopographySurfaceManagement.RemoveDuplicateXY(ref PointXYZList);
					topographySurface = TerrainApi.Create(m_Document, PointXYZList);
					m_Document.Regenerate();
					elementSet.Insert(topographySurface);
					TerrainApi.SetMaterialId(topographySurface, elementId);
					parameter2 = TerrainApi.NameParameter(((Element)topographySurface));
					if (parameter2 != null && !parameter2.IsReadOnly)
					{
						parameter2.Set(EPShared.LocalRM.GetString("BaseToposurfaceString"));
					}
				}
				else
				{
					topographySurface = _SurfaceList[0].TopographySurface;
				}
				if (m_PlacementSymbol != null)
				{
					FamilyInstance familyInstance = RevitUtility.NewFamilyInstance(m_Document, new XYZ(0.0, 0.0, 0.0), m_PlacementSymbol, topographySurface, m_Document.ActiveView.GenLevel, StructuralType.NonStructural, ref m_TransactionContainer);
					parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(topographySurface.UniqueId);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.IsProposedSurfaceFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(Convert.ToInt32(value: false));
					}
					parameter = familyInstance.LookupParameter(RevitParameters.BaseToposurfaceCreatedFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(Convert.ToInt32(value));
					}
					elementSet.Insert(familyInstance);
				}
				RevitUtility.HideElementsInAllViews(m_Document, elementSet);
				progressBarForm.SetMaxValue();
				Thread.Sleep(300);
				progressBarForm.Close();
				m_TransactionContainer.EndTransaction();
				TopographySurface _ProposedTopographySurface = null;
				FamilyInstance _PlacementFamilyInstance = null;
				TopographySurfaceManagement.CreateProposedSurface(m_Document, m_TransactionContainer, m_PlacementSymbol, surfacePoints, strToposurfaceName, elementId, phaseCreated, phaseDemolished, ref _ProposedTopographySurface, ref _PlacementFamilyInstance, topographySurface);
				Eps_Point eps_Point = null;
				if (list != null && list.Count > 0)
				{
					TopographySurfaceContainer curTopoSurfaceContainer = new TopographySurfaceContainer(m_Document.Application, _ProposedTopographySurface, _bFindMinMaxPoints: true);
					eps_Point = TopographySurfaceManagement.FindToposurfaceAdjustmentPoint(curTopoSurfaceContainer, m_SurfaceList);
					List<ElementId> list5 = new List<ElementId>();
					for (int m = 0; m < list.Count; m++)
					{
						if (list[m].TopographySurface != null)
						{
							list5.Add(list[m].TopographySurface.Id);
						}
						else if (list[m].BuildingPad != null)
						{
							list5.Add(list[m].BuildingPad.Id);
						}
					}
					ElementTransformUtils.MoveElement(m_Document, _ProposedTopographySurface.Id, eps_Point.XY);
					try
					{
						LinearArray.ArrayElementsWithoutAssociation(m_Document, m_Document.ActiveView, list5.ToArray(), 2, eps_Point.XY, ArrayAnchorMember.Second);
					}
					catch (Exception ex)
					{
						if (ex is AccessViolationException || ex is InvalidOperationException)
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("SubregionBuildingPadBoundaryErrorMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
							return false;
						}
					}
					Eps_Point eps_Point2 = null;
					PointXYZList.Clear();
					for (int n = 0; n < list3.Count; n++)
					{
						eps_Point2 = new Eps_Point(list3[n].x + eps_Point.x, list3[n].y + eps_Point.y, list3[n].z);
						PointXYZList.Add(eps_Point2.XYZ);
					}
					for (int num2 = 0; num2 < list.Count; num2++)
					{
						if (list[num2] == null || list[num2].TopographySurfaceType != TopographySurfaceType.Subregion || list[num2].TopographySurface == null || list[num2].BoundaryLeg == null)
						{
							continue;
						}
						for (int num3 = 0; num3 < list[num2].Vertices.Count; num3++)
						{
							if (list[num2].Vertices[num3] != null && (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(list[num2].Vertices[num3], list[num2].BoundaryLeg, ref _dStation, ref _dOffset) != 1700 || Math.Abs(_dOffset) > 1E-05))
							{
								eps_Point2 = new Eps_Point(list[num2].Vertices[num3].x + eps_Point.x, list[num2].Vertices[num3].y + eps_Point.y, list[num2].Vertices[num3].z);
								PointXYZList.Add(eps_Point2.XYZ);
							}
						}
					}
					if (PointXYZList.Count > 0)
					{
						m_TransactionContainer.Commit();
						m_TransactionContainer.StartTopographyEditScope(m_TransactionContainer.GetName(), _ProposedTopographySurface.Id);
						m_TransactionContainer.Start();
						_ProposedTopographySurface.AddPoints(PointXYZList);
						m_TransactionContainer.Commit();
						TopographyEditFailuresPreprocessor failuresPreprocessor = new TopographyEditFailuresPreprocessor();
						m_TransactionContainer.CommitTopographyEditScope(ref failuresPreprocessor);
						m_TransactionContainer.Start();
					}
				}
				if (m_OriginalGroundSurface != null && (_SurfaceList.Count > 1 || (_SurfaceList.Count == 1 && string.Compare(_SurfaceList[0].TopographySurface.UniqueId, m_OriginalGroundSurface.UniqueId, ignoreCase: true) != 0)))
				{
					m_TransactionContainer.BeginTransaction();
					m_Document.Delete(m_OriginalGroundSurface.Id);
					m_TransactionContainer.EndTransaction();
				}
				if (m_OriginalGroundFamilyInstance != null)
				{
					m_TransactionContainer.BeginTransaction();
					m_Document.Delete(m_OriginalGroundFamilyInstance.Id);
					m_TransactionContainer.EndTransaction();
				}
				if (m_ProposedSurface != null && (_SurfaceList.Count > 1 || (_SurfaceList.Count == 1 && string.Compare(_SurfaceList[0].TopographySurface.UniqueId, m_ProposedSurface.UniqueId, ignoreCase: true) != 0)))
				{
					m_TransactionContainer.BeginTransaction();
					m_Document.Delete(m_ProposedSurface.Id);
					m_TransactionContainer.EndTransaction();
				}
				if (m_ProposedFamilyInstance != null)
				{
					m_TransactionContainer.BeginTransaction();
					m_Document.Delete(m_ProposedFamilyInstance.Id);
					m_TransactionContainer.EndTransaction();
				}
				RevitUtility.RemoveAllSiteComponents(m_Document, m_TransactionContainer, _bIssueWarningForRemoval: true);
				if (list != null && list.Count > 0)
				{
					ElementTransformUtils.MoveElement(m_Document, _ProposedTopographySurface.Id, new XYZ(0.0 - eps_Point.x, 0.0 - eps_Point.y, 0.0));
				}
				if (m_HideOriginalTopoSurface)
				{
					ElementSet elementSet2 = new ElementSet();
					elementSet2.Insert(topographySurface);
					RevitUtility.HideElementsInAllViews(m_Document, elementSet2);
				}
				m_TransactionContainer.Commit();
				return true;
			}
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NoPointsFoundMessage") + " " + EPShared.LocalRM.GetString("ToposurfaceNotCreatedMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
			return false;
		}
		RevitMessageBox.Show(EPShared.LocalRM.GetString("ToposurfaceNotCreatedMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
		return false;
	}
}

