// Decompiled with JetBrains decompiler
// Type: EPCommon.SetBaseToposurface
// Assembly: SiteDesigner, Version=20.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 84104107-DD17-4742-AB85-FA77E5583D5F
// Assembly location: C:\Program Files\Autodesk\Revit Site Designer Extension 2020\SiteDesigner.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using SITES.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

#nullable disable
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

  public SetBaseToposurface(Document _CurDocument)
  {
    this.m_Document = _CurDocument;
    this.m_bPickIndividualSurface = false;
    this.SetDefaults();
  }

  public SetBaseToposurface(Document _CurDocument, bool _bPickIndividualSurface)
  {
    this.m_Document = _CurDocument;
    this.m_bPickIndividualSurface = _bPickIndividualSurface;
    this.SetDefaults();
  }

  private void SetDefaults()
  {
    this.m_TransactionContainer = new EPTransactionContainer(this.m_Document);
    this.m_SurfaceList = new List<TopographySurfaceContainer>();
    this.m_InstanceSet = new ElementSet();
    this.m_ProposedSurface = (TopographySurface) null;
    this.m_OriginalGroundSurface = (TopographySurface) null;
    this.m_PlacementSymbol = (FamilySymbol) null;
    this.InitializeListData();
  }

  private void InitializeListData()
  {
    this.PopulateFamilyAndSymbolLists();
    FindFamilyInstance = (FamilyInstance) null;
    Parameter SiteworksIDParameter = (Parameter) null;
    FilteredElementCollector source = new FilteredElementCollector(this.m_Document);
    source.OfClass(typeof (FamilyInstance)).OfCategory((BuiltInCategory) -2001260);
    List<Element> list = ((IEnumerable<Element>) source).Where<Element>((Func<Element, bool>) (CurrentElement => CurrentElement is FamilyInstance FindFamilyInstance && (SiteworksIDParameter = ((Element) FindFamilyInstance.Symbol).LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32((object) LandcaddID.TopoSurfacePlacement).ToString("000"), true) == 0 && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null)).ToList<Element>();
    IList<Element> elements1 = new FilteredElementCollector(this.m_Document).OfClass(typeof (TopographySurface)).OfCategory((BuiltInCategory) -2001340).ToElements();
    IList<Element> elements2 = new FilteredElementCollector(this.m_Document).OfClass(typeof (CeilingAndFloor)).OfCategory((BuiltInCategory) -2001263).ToElements();
    List<TopographySurface> topographySurfaceList = new List<TopographySurface>();
    List<TopographySurfaceContainer> _SubRegionBuildingPadList = new List<TopographySurfaceContainer>();
    foreach (Element element1 in (IEnumerable<Element>) elements1)
    {
      TopographySurface topographySurface = element1 as TopographySurface;
      if (!this.m_bPickIndividualSurface)
      {
        foreach (Element element2 in list)
        {
          if (element2 is FamilyInstance familyInstance && ((Element) familyInstance).Category != null && ((Element) familyInstance).Category.Id.IntegerValue == ((BuiltInCategory) -2001260).GetHashCode())
          {
            SiteworksIDParameter = ((Element) familyInstance.Symbol).LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
            if (SiteworksIDParameter != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32((object) LandcaddID.TopoSurfacePlacement).ToString("000"), true) == 0)
            {
              Parameter parameter1 = ((Element) familyInstance).LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
              if (parameter1 != null && string.Compare(parameter1.AsString(), element1.UniqueId, true) == 0)
              {
                Parameter parameter2 = ((Element) familyInstance).LookupParameter(RevitParameters.IsProposedSurfaceFamilyParameter);
                if (parameter2 != null)
                {
                  if (Convert.ToBoolean(parameter2.AsInteger()))
                  {
                    this.m_ProposedFamilyInstance = familyInstance;
                    this.m_ProposedSurface = topographySurface;
                  }
                  else
                  {
                    Parameter parameter3 = ((Element) familyInstance).LookupParameter(RevitParameters.BaseToposurfaceCreatedFamilyParameter);
                    if (parameter3 != null && Convert.ToBoolean(parameter3.AsInteger()))
                      this.m_OriginalGroundSurface = topographySurface;
                    this.m_OriginalGroundFamilyInstance = familyInstance;
                  }
                }
              }
            }
          }
        }
      }
      topographySurfaceList.Add(topographySurface);
    }
    for (int index = 0; index < topographySurfaceList.Count; ++index)
    {
      TopographySurfaceType topographySurfaceType = TopographySurfaceContainer.FindTopographySurfaceType(this.m_Document.Application, topographySurfaceList[index]);
      switch (topographySurfaceType)
      {
        case TopographySurfaceType.MainSurface:
          this.m_SurfaceList.Add(new TopographySurfaceContainer(this.m_Document.Application, topographySurfaceList[index]));
          break;
        case TopographySurfaceType.Subregion:
        case TopographySurfaceType.BuildingPad:
          _SubRegionBuildingPadList.Add(new TopographySurfaceContainer(this.m_Document.Application, topographySurfaceList[index], true, topographySurfaceType));
          break;
      }
    }
    foreach (Element element in (IEnumerable<Element>) elements2)
    {
      if (element is CeilingAndFloor _BuildingPad)
        _SubRegionBuildingPadList.Add(new TopographySurfaceContainer(this.m_Document.Application, _BuildingPad, true));
    }
    TopographySurfaceManagement.AddSubRegionBuildingPadsToMainSurfaceList(this.m_Document, _SubRegionBuildingPadList, ref this.m_SurfaceList);
  }

  private void PopulateFamilyAndSymbolLists()
  {
    if (this.m_bPickIndividualSurface)
      return;
    Parameter CurSiteworksID = (Parameter) null;
    FamilySymbol familySymbol1 = (FamilySymbol) null;
    FilteredElementCollector source = new FilteredElementCollector(this.m_Document);
    source.OfClass(typeof (FamilySymbol)).OfCategory((BuiltInCategory) -2001260);
    CurSiteworksID = (Parameter) null;
    List<Element> list = ((IEnumerable<Element>) source).Where<Element>((Func<Element, bool>) (CurrentElement => (CurSiteworksID = CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32((object) LandcaddID.TopoSurfacePlacement).ToString("000"), true) == 0)).ToList<Element>();
    bool flag = false;
    familySymbol1 = (FamilySymbol) null;
    foreach (Element element in list)
    {
      if (element is FamilySymbol familySymbol2 && ((Element) familySymbol2).Category != null)
      {
        CurSiteworksID = ((Element) familySymbol2).LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
        if (CurSiteworksID != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32((object) LandcaddID.TopoSurfacePlacement).ToString("000"), true) == 0)
        {
          this.m_PlacementSymbol = familySymbol2;
          flag = true;
          if (this.m_PlacementSymbol != null)
          {
            if (!this.m_PlacementSymbol.IsActive)
            {
              this.m_TransactionContainer.Start("ActivateFamilySymbol");
              this.m_PlacementSymbol.Activate();
              this.m_TransactionContainer.Commit();
              break;
            }
            break;
          }
          break;
        }
      }
    }
    string empty = string.Empty;
    if (flag || !RevitUtility.GetFullFamilyFilenameIfExist("Toposurface Placement.rfa", ref empty))
      return;
    Family _Family = (Family) null;
    this.m_TransactionContainer.Start(EPShared.LocalRM.GetString("LoadTopoPlacementFamilyTransaction") + Path.GetFileName(empty));
    RevitUtility.LoadFamily(this.m_Document, empty, out _Family, true, ref this.m_TransactionContainer);
    this.m_TransactionContainer.Commit();
    if (_Family == null || _Family.GetFamilySymbolIds() == null)
      return;
    using (IEnumerator<ElementId> enumerator = _Family.GetFamilySymbolIds().GetEnumerator())
    {
      if (!enumerator.MoveNext())
        return;
      if (this.m_Document.GetElement(enumerator.Current) is FamilySymbol element)
        this.m_PlacementSymbol = element;
      if (this.m_PlacementSymbol == null || this.m_PlacementSymbol.IsActive)
        return;
      this.m_TransactionContainer.Start("ActivateFamilySymbol");
      this.m_PlacementSymbol.Activate();
      this.m_TransactionContainer.Commit();
    }
  }

  public bool Run() => this.DisplayUI();

  private bool DisplayUI()
  {
    bool flag = false;
    if (this.m_SurfaceList.Count == 0)
    {
      int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("ToposurfacNotInProjectMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
      return false;
    }
    if (this.m_bPickIndividualSurface && this.m_OriginalGroundFamilyInstance != null)
    {
      int num1 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("BaseToposurfaceAlreadySeteMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
    }
    DialogResult _DialogResult = DialogResult.Retry;
    using (this.m_DisplayForm = new SetBaseToposurfaceForm(this))
    {
      while (_DialogResult == DialogResult.Retry)
        flag = this.ShowDialog(ref _DialogResult);
    }
    return flag;
  }

  private bool PickTopographySurfaces(ref ElementSet _InstanceSet)
  {
    ElementSet _Elements = (ElementSet) null;
    if (!this.m_bPickIndividualSurface)
    {
      if (!RevitUtility.GetElementsFromProject(this.m_Document, EPShared.LocalRM.GetString("PickToposurfacesPrompt"), ref _Elements, true, PickMethod.PickObjects) || _Elements == null || _Elements.Size == 0)
        return false;
    }
    else if (!RevitUtility.GetElementsFromProject(this.m_Document, EPShared.LocalRM.GetString("PickToposurfacePrompt"), ref _Elements) || _Elements == null || _Elements.Size == 0)
      return false;
    bool flag = false;
    foreach (Element element in _Elements)
    {
      if (element is TopographySurface topographySurface && ((Element) topographySurface).Category != null && ((Element) topographySurface).Category.Id.IntegerValue == ((BuiltInCategory) -2001340).GetHashCode())
      {
        for (int index1 = 0; index1 < this.m_SurfaceList.Count; ++index1)
        {
          if (string.Compare(((Element) topographySurface).UniqueId, ((Element) this.m_SurfaceList[index1].TopographySurface).UniqueId, true) == 0)
          {
            flag = true;
            break;
          }
          for (int index2 = 0; index2 < this.m_SurfaceList[index1].SubregionBuildingPadList.Count; ++index2)
          {
            if (string.Compare(((Element) topographySurface).UniqueId, ((Element) this.m_SurfaceList[index1].SubregionBuildingPadList[index2].TopographySurface).UniqueId, true) == 0)
            {
              flag = true;
              break;
            }
          }
        }
        if (flag)
        {
          _InstanceSet.Insert(element);
        }
        else
        {
          int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("ToposurfaceAlreadyPickedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
        }
      }
      else
      {
        int num1 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("ObjectSelectedNotToposurfaceMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
      }
    }
    return true;
  }

  private bool ShowDialog(ref DialogResult _DialogResult)
  {
    bool flag = false;
    int num = (int) this.m_DisplayForm.ShowDialog();
    _DialogResult = this.m_DisplayForm.DialogResult;
    if (_DialogResult == DialogResult.OK)
    {
      if (!this.m_bPickIndividualSurface)
        this.CreateBaseToposurface(this.m_DisplayForm.SelectedSurfaceList);
      else if (this.m_DisplayForm.SelectedSurfaceList != null && this.m_DisplayForm.SelectedSurfaceList.Count > 0)
      {
        this.m_InstanceSet.Clear();
        this.m_InstanceSet.Insert((Element) this.m_DisplayForm.SelectedSurfaceList[0].TopographySurface);
      }
      flag = true;
    }
    else if (_DialogResult == DialogResult.Cancel)
      flag = false;
    else if (_DialogResult == DialogResult.Retry)
    {
      ElementSet _InstanceSet = new ElementSet();
      if (!this.PickTopographySurfaces(ref _InstanceSet))
        return false;
      this.m_InstanceSet = _InstanceSet;
      flag = true;
      this.m_DisplayForm.SelectListItems();
    }
    return flag;
  }

  public bool CreateBaseToposurface(List<TopographySurfaceContainer> _SurfaceList)
  {
    if (_SurfaceList != null && _SurfaceList.Count > 0)
    {
      for (int index = 0; index < _SurfaceList.Count; ++index)
      {
        if (_SurfaceList[index].BuildingPadPresentInDiffentPhase)
        {
          if (RevitMessageBox.Show(EPShared.LocalRM.GetString("DifferentPhasingForBuildingPadsMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
            return false;
          break;
        }
      }
      List<Eps_Point> surfacePoints = TopographySurfaceManagement.GetSurfacePoints(_SurfaceList);
      if (surfacePoints.Count > 0)
      {
        List<TopographySurfaceContainer> surfaceContainerList = new List<TopographySurfaceContainer>();
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = false;
        for (int index1 = 0; index1 < _SurfaceList.Count; ++index1)
        {
          if (!flag1 && _SurfaceList[index1].IsTopographySurfaceTypePresent(TopographySurfaceType.Subregion))
          {
            flag1 = true;
            if (RevitMessageBox.Show(EPShared.LocalRM.GetString("AddSubRegionsToProposedSurfaceMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
              flag3 = true;
          }
          if (!flag2 && _SurfaceList[index1].IsTopographySurfaceTypePresent(TopographySurfaceType.BuildingPad))
          {
            flag2 = true;
            if (RevitMessageBox.Show(EPShared.LocalRM.GetString("AddBuildingPadsToProposedSurfaceMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
              flag4 = true;
          }
          for (int index2 = 0; index2 < _SurfaceList[index1].SubregionBuildingPadList.Count; ++index2)
          {
            if (_SurfaceList[index1].SubregionBuildingPadList[index2] != null && (_SurfaceList[index1].SubregionBuildingPadList[index2].TopographySurfaceType == TopographySurfaceType.Subregion && flag3 || _SurfaceList[index1].SubregionBuildingPadList[index2].TopographySurfaceType == TopographySurfaceType.BuildingPad && flag4) && (_SurfaceList[index1].SubregionBuildingPadList[index2].TopographySurface != null || _SurfaceList[index1].SubregionBuildingPadList[index2].BuildingPad != null))
              surfaceContainerList.Add(_SurfaceList[index1].SubregionBuildingPadList[index2]);
          }
        }
        ElementId _MaterialId = (ElementId) null;
        ElementId _PhaseCreated = (ElementId) null;
        ElementId _PhaseDemolished = (ElementId) null;
        string _strToposurfaceName = string.Empty;
        using (PhasePickerForm phasePickerForm = new PhasePickerForm(this.m_Document))
        {
          if (phasePickerForm.ShowDialog() != DialogResult.OK)
            return false;
          this.m_HideOriginalTopoSurface = phasePickerForm.HideOriginalTopoSurface;
          _PhaseCreated = phasePickerForm.CreatedPhaseId == -1 ? new ElementId(32440) : new ElementId(phasePickerForm.CreatedPhaseId);
          _strToposurfaceName = phasePickerForm.ToposurfaceName;
        }
        List<Element> _ElementList = new List<Element>();
        if (this.m_PlacementSymbol != null)
          _ElementList.Add((Element) this.m_PlacementSymbol);
        if (this.m_OriginalGroundSurface != null && (_SurfaceList.Count > 1 || _SurfaceList.Count == 1 && string.Compare(((Element) _SurfaceList[0].TopographySurface).UniqueId, ((Element) this.m_OriginalGroundSurface).UniqueId, true) != 0))
          _ElementList.Add((Element) this.m_OriginalGroundSurface);
        if (this.m_OriginalGroundFamilyInstance != null)
          _ElementList.Add((Element) this.m_OriginalGroundFamilyInstance);
        if (this.m_ProposedSurface != null && (_SurfaceList.Count > 1 || _SurfaceList.Count == 1 && string.Compare(((Element) _SurfaceList[0].TopographySurface).UniqueId, ((Element) this.m_ProposedSurface).UniqueId, true) != 0))
          _ElementList.Add((Element) this.m_ProposedSurface);
        if (this.m_ProposedFamilyInstance != null)
          _ElementList.Add((Element) this.m_ProposedFamilyInstance);
        if (!TopographySurfaceManagement.VerifyUserCanManipulateElements(this.m_Document, _ElementList))
          return false;
        List<XYZ> PointXYZList = new List<XYZ>();
        List<Eps_Point> epsPointList = new List<Eps_Point>();
        double _dOffset = 0.0;
        double _dStation = 0.0;
        bool flag5 = false;
        bool flag6 = false;
        for (int index3 = surfacePoints.Count - 1; index3 >= 0; --index3)
        {
          for (int index4 = 0; index4 < surfaceContainerList.Count; ++index4)
          {
            if (surfaceContainerList[index4] != null)
            {
              if (surfaceContainerList[index4].BoundaryLeg != null && RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(surfacePoints[index3], surfaceContainerList[index4].BoundaryLeg, ref _dStation, ref _dOffset) == 1700 && Math.Abs(_dOffset) <= 0.001)
              {
                flag6 = true;
                break;
              }
              if (surfaceContainerList[index4].GetSurfaceContainerContainingPoint(surfacePoints[index3]) != null)
              {
                if (surfaceContainerList[index4].TopographySurfaceType == TopographySurfaceType.Subregion)
                {
                  flag5 = true;
                  break;
                }
                flag6 = true;
                break;
              }
            }
          }
          if (flag5)
          {
            epsPointList.Add(surfacePoints[index3]);
            surfacePoints.RemoveAt(index3);
          }
          else if (!flag6)
            PointXYZList.Add(surfacePoints[index3].XYZ);
          else
            surfacePoints.RemoveAt(index3);
          flag6 = false;
          flag5 = false;
        }
        ProgressBarForm progressBarForm = new ProgressBarForm();
        progressBarForm.ShowForm();
        progressBarForm.Refresh();
        progressBarForm.MinValue = 0;
        progressBarForm.MaxValue = Convert.ToInt32(2);
        progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarCreatingNewBaseToposurfaceMessage");
        this.m_TransactionContainer.Start(EPShared.LocalRM.GetString("SetBaseToposurfaceTransaction"));
        progressBarForm.UpdateProgressBar(1);
        this.m_TransactionContainer.BeginTransaction();
        bool flag7 = false;
        ElementSet _ElementSet1 = new ElementSet();
        int _nReturnValue = -1;
        if (ApplicationUtility.Settings.eplSetE_GetSetting(this.m_Document.PathName, "ProposedToposurfaceMaterialParameter", ref _nReturnValue))
        {
          _MaterialId = new ElementId(_nReturnValue);
          bool flag8 = false;
          foreach (Element element in (IEnumerable<Element>) new FilteredElementCollector(this.m_Document).OfClass(typeof (Material)).OfCategory((BuiltInCategory) -2000700).ToElements())
          {
            if (ElementId.op_Equality(element.Id, _MaterialId))
            {
              flag8 = true;
              break;
            }
          }
          if (!flag8)
            _nReturnValue = -1;
        }
        if (_nReturnValue == -1 && _SurfaceList.Count > 0)
        {
          Parameter parameter = ((Element) _SurfaceList[0].TopographySurface)[(BuiltInParameter) -1002107];
          if (parameter != null)
          {
            _MaterialId = parameter.AsElementId();
            _nReturnValue = _MaterialId.IntegerValue;
          }
        }
        TopographySurface topographySurface;
        if (_SurfaceList.Count > 1)
        {
          flag7 = true;
          TopographySurfaceManagement.RemoveDuplicateXY(ref PointXYZList);
          topographySurface = TopographySurface.Create(this.m_Document, (IList<XYZ>) PointXYZList);
          this.m_Document.Regenerate();
          _ElementSet1.Insert((Element) topographySurface);
          Parameter parameter1 = ((Element) topographySurface)[(BuiltInParameter) -1002107];
          if (parameter1 != null && _nReturnValue != -1)
            parameter1.Set(_MaterialId);
          Parameter parameter2 = ((Element) topographySurface)[(BuiltInParameter) -1006900];
          if (parameter2 != null && !((APIObject) parameter2).IsReadOnly)
            parameter2.Set(EPShared.LocalRM.GetString("BaseToposurfaceString"));
        }
        else
          topographySurface = _SurfaceList[0].TopographySurface;
        if (this.m_PlacementSymbol != null)
        {
          FamilyInstance familyInstance = RevitUtility.NewFamilyInstance(this.m_Document, new XYZ(0.0, 0.0, 0.0), this.m_PlacementSymbol, (Element) topographySurface, this.m_Document.ActiveView.GenLevel, (StructuralType) 0, ref this.m_TransactionContainer);
          Parameter parameter3 = ((Element) familyInstance).LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
          if (parameter3 != null && !((APIObject) parameter3).IsReadOnly)
            parameter3.Set(((Element) topographySurface).UniqueId);
          Parameter parameter4 = ((Element) familyInstance).LookupParameter(RevitParameters.IsProposedSurfaceFamilyParameter);
          if (parameter4 != null && !((APIObject) parameter4).IsReadOnly)
            parameter4.Set(Convert.ToInt32(false));
          Parameter parameter5 = ((Element) familyInstance).LookupParameter(RevitParameters.BaseToposurfaceCreatedFamilyParameter);
          if (parameter5 != null && !((APIObject) parameter5).IsReadOnly)
            parameter5.Set(Convert.ToInt32(flag7));
          _ElementSet1.Insert((Element) familyInstance);
        }
        RevitUtility.HideElementsInAllViews(this.m_Document, _ElementSet1);
        progressBarForm.SetMaxValue();
        Thread.Sleep(300);
        progressBarForm.Close();
        this.m_TransactionContainer.EndTransaction();
        TopographySurface _ProposedTopographySurface = (TopographySurface) null;
        FamilyInstance _PlacementFamilyInstance = (FamilyInstance) null;
        TopographySurfaceManagement.CreateProposedSurface(this.m_Document, this.m_TransactionContainer, this.m_PlacementSymbol, surfacePoints, _strToposurfaceName, _MaterialId, _PhaseCreated, _PhaseDemolished, ref _ProposedTopographySurface, ref _PlacementFamilyInstance);
        Eps_Point epsPoint1 = (Eps_Point) null;
        if (surfaceContainerList != null && surfaceContainerList.Count > 0)
        {
          epsPoint1 = TopographySurfaceManagement.FindToposurfaceAdjustmentPoint(new TopographySurfaceContainer(this.m_Document.Application, _ProposedTopographySurface, true), this.m_SurfaceList);
          List<ElementId> elementIdList = new List<ElementId>();
          for (int index = 0; index < surfaceContainerList.Count; ++index)
          {
            if (surfaceContainerList[index].TopographySurface != null)
              elementIdList.Add(((Element) surfaceContainerList[index].TopographySurface).Id);
            else if (surfaceContainerList[index].BuildingPad != null)
              elementIdList.Add(((Element) surfaceContainerList[index].BuildingPad).Id);
          }
          ElementTransformUtils.MoveElement(this.m_Document, ((Element) _ProposedTopographySurface).Id, epsPoint1.XY);
          try
          {
            LinearArray.ArrayElementsWithoutAssociation(this.m_Document, this.m_Document.ActiveView, (ICollection<ElementId>) elementIdList.ToArray(), 2, epsPoint1.XY, (ArrayAnchorMember) 0);
          }
          catch (Exception ex)
          {
            switch (ex)
            {
              case AccessViolationException _:
              case InvalidOperationException _:
                int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("SubregionBuildingPadBoundaryErrorMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
                return false;
            }
          }
          PointXYZList.Clear();
          for (int index = 0; index < epsPointList.Count; ++index)
          {
            Eps_Point epsPoint2 = new Eps_Point(epsPointList[index].x + epsPoint1.x, epsPointList[index].y + epsPoint1.y, epsPointList[index].z);
            PointXYZList.Add(epsPoint2.XYZ);
          }
          for (int index5 = 0; index5 < surfaceContainerList.Count; ++index5)
          {
            if (surfaceContainerList[index5] != null && surfaceContainerList[index5].TopographySurfaceType == TopographySurfaceType.Subregion && surfaceContainerList[index5].TopographySurface != null && surfaceContainerList[index5].BoundaryLeg != null)
            {
              for (int index6 = 0; index6 < surfaceContainerList[index5].Vertices.Count; ++index6)
              {
                if (surfaceContainerList[index5].Vertices[index6] != null && (RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(surfaceContainerList[index5].Vertices[index6], surfaceContainerList[index5].BoundaryLeg, ref _dStation, ref _dOffset) != 1700 || Math.Abs(_dOffset) > 1E-05))
                {
                  Eps_Point epsPoint3 = new Eps_Point(surfaceContainerList[index5].Vertices[index6].x + epsPoint1.x, surfaceContainerList[index5].Vertices[index6].y + epsPoint1.y, surfaceContainerList[index5].Vertices[index6].z);
                  PointXYZList.Add(epsPoint3.XYZ);
                }
              }
            }
          }
          if (PointXYZList.Count > 0)
          {
            this.m_TransactionContainer.Commit();
            this.m_TransactionContainer.StartTopographyEditScope(this.m_TransactionContainer.GetName(), ((Element) _ProposedTopographySurface).Id);
            this.m_TransactionContainer.Start();
            _ProposedTopographySurface.AddPoints((IList<XYZ>) PointXYZList);
            this.m_TransactionContainer.Commit();
            TopographyEditFailuresPreprocessor failuresPreprocessor = new TopographyEditFailuresPreprocessor();
            this.m_TransactionContainer.CommitTopographyEditScope(ref failuresPreprocessor);
            this.m_TransactionContainer.Start();
          }
        }
        if (this.m_OriginalGroundSurface != null && (_SurfaceList.Count > 1 || _SurfaceList.Count == 1 && string.Compare(((Element) _SurfaceList[0].TopographySurface).UniqueId, ((Element) this.m_OriginalGroundSurface).UniqueId, true) != 0))
        {
          this.m_TransactionContainer.BeginTransaction();
          this.m_Document.Delete(((Element) this.m_OriginalGroundSurface).Id);
          this.m_TransactionContainer.EndTransaction();
        }
        if (this.m_OriginalGroundFamilyInstance != null)
        {
          this.m_TransactionContainer.BeginTransaction();
          this.m_Document.Delete(((Element) this.m_OriginalGroundFamilyInstance).Id);
          this.m_TransactionContainer.EndTransaction();
        }
        if (this.m_ProposedSurface != null && (_SurfaceList.Count > 1 || _SurfaceList.Count == 1 && string.Compare(((Element) _SurfaceList[0].TopographySurface).UniqueId, ((Element) this.m_ProposedSurface).UniqueId, true) != 0))
        {
          this.m_TransactionContainer.BeginTransaction();
          this.m_Document.Delete(((Element) this.m_ProposedSurface).Id);
          this.m_TransactionContainer.EndTransaction();
        }
        if (this.m_ProposedFamilyInstance != null)
        {
          this.m_TransactionContainer.BeginTransaction();
          this.m_Document.Delete(((Element) this.m_ProposedFamilyInstance).Id);
          this.m_TransactionContainer.EndTransaction();
        }
        RevitUtility.RemoveAllSiteComponents(this.m_Document, this.m_TransactionContainer, true);
        if (surfaceContainerList != null && surfaceContainerList.Count > 0)
          ElementTransformUtils.MoveElement(this.m_Document, ((Element) _ProposedTopographySurface).Id, new XYZ(-epsPoint1.x, -epsPoint1.y, 0.0));
        if (this.m_HideOriginalTopoSurface)
        {
          ElementSet _ElementSet2 = new ElementSet();
          _ElementSet2.Insert((Element) topographySurface);
          RevitUtility.HideElementsInAllViews(this.m_Document, _ElementSet2);
        }
        this.m_TransactionContainer.Commit();
        return true;
      }
      int num1 = (int) RevitMessageBox.Show($"{EPShared.LocalRM.GetString("NoPointsFoundMessage")} {EPShared.LocalRM.GetString("ToposurfaceNotCreatedMessage")}", EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
      return false;
    }
    int num2 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("ToposurfaceNotCreatedMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
    return false;
  }

  public Document Document => this.m_Document;

  public ReadOnlyCollection<TopographySurfaceContainer> SurfaceList
  {
    get
    {
      return new ReadOnlyCollection<TopographySurfaceContainer>((IList<TopographySurfaceContainer>) this.m_SurfaceList);
    }
  }

  public ElementSet InstanceSet => this.m_InstanceSet;

  public EPTransactionContainer TransactionContainer => this.m_TransactionContainer;

  public bool PickIndividualSurface => this.m_bPickIndividualSurface;
}
