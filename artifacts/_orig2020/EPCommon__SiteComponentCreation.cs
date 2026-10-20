// Decompiled with JetBrains decompiler
// Type: EPCommon.SiteComponentCreation
// Assembly: SiteDesigner, Version=20.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 84104107-DD17-4742-AB85-FA77E5583D5F
// Assembly location: C:\Program Files\Autodesk\Revit Site Designer Extension 2020\SiteDesigner.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SITES.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

#nullable disable
namespace EPCommon;

public class SiteComponentCreation
{
  private Document m_Document;
  private LocateSiteComponentForm m_DisplayForm;
  private LocateSiteCurbComponentForm m_CurbDisplayForm;
  private LocateSiteParkingLotComponentForm m_ParkingLotDisplayForm;
  private LocateSiteFeatureLineComponentForm m_FeatureLineDisplayForm;
  private LocateSitePointWipeoutComponentForm m_PointWipeoutDisplayForm;
  private TopographySurfaceContainer m_BaseSurface;
  private TopographySurfaceContainer m_ProposedSurface;
  private List<FamilyContainer> m_FamilyList;
  private EPTransactionContainer m_TransactionContainer;
  private FamilySymbol m_PlacementSymbol;
  private FamilySymbol m_IntersectionPlacementSymbol;
  private FamilyInstance m_BasePlacementFamilyInstance;
  private FamilyInstance m_ProposedPlacementFamilyInstance;
  private LandcaddID m_CurrentLandcaddId = LandcaddID.SoftTerrain;
  private int m_nMaxExistingTopoDrawOrder;
  private bool m_bContinueWithoutLoading;
  private const string m_strNummericSeperator = " - ";

  public SiteComponentCreation(
    Document _CurDocument,
    TopographySurfaceContainer _BaseSurface,
    FamilyInstance _BasePlacementFamilyInstance,
    TopographySurfaceContainer _ProposedSurface,
    FamilyInstance _ProposedPlacementFamilyInstance,
    LandcaddID _CurrentLandcaddID)
  {
    this.m_Document = _CurDocument;
    this.m_CurrentLandcaddId = _CurrentLandcaddID;
    this.m_TransactionContainer = new EPTransactionContainer(this.m_Document);
    this.m_BaseSurface = _BaseSurface;
    this.m_BasePlacementFamilyInstance = _BasePlacementFamilyInstance;
    this.m_ProposedSurface = _ProposedSurface;
    this.m_ProposedPlacementFamilyInstance = _ProposedPlacementFamilyInstance;
    this.m_FamilyList = new List<FamilyContainer>();
    this.m_PlacementSymbol = (FamilySymbol) null;
    this.m_IntersectionPlacementSymbol = (FamilySymbol) null;
    this.m_nMaxExistingTopoDrawOrder = -1;
    this.InitializeListData();
  }

  public void PopulateFamilyAndSymbolLists() => this.PopulateFamilyAndSymbolLists(false);

  private void PopulateFamilyAndSymbolLists(bool _bCheckForFamily)
  {
    this.m_FamilyList.Clear();
    Parameter CurSiteworksID = (Parameter) null;
    FamilySymbol familySymbol1 = (FamilySymbol) null;
    if (this.m_CurrentLandcaddId != LandcaddID.FeatureLine && this.m_CurrentLandcaddId != LandcaddID.PointWipeout)
    {
      FilteredElementCollector source = new FilteredElementCollector(this.m_Document);
      source.OfClass(typeof (FamilySymbol)).OfCategory((BuiltInCategory) -2001260);
      List<Element> list = ((IEnumerable<Element>) source).Where<Element>((Func<Element, bool>) (CurrentElement => (CurSiteworksID = CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32((object) this.m_CurrentLandcaddId).ToString("000"), true) == 0)).ToList<Element>();
      familySymbol1 = (FamilySymbol) null;
      foreach (Element element in list)
      {
        if (element is FamilySymbol familySymbol2 && ((Element) familySymbol2).Category != null)
        {
          CurSiteworksID = ((Element) familySymbol2).LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
          if (CurSiteworksID != null && string.Compare(CurSiteworksID.AsString(), Convert.ToInt32((object) this.m_CurrentLandcaddId).ToString("000"), true) == 0)
          {
            bool flag = false;
            for (int index = 0; index < this.m_FamilyList.Count; ++index)
            {
              if (string.Compare(this.m_FamilyList[index].FamilyName, ((Element) familySymbol2.Family).Name, true) == 0)
              {
                this.m_FamilyList[index].FamilySymbolList.Add(familySymbol2);
                flag = true;
                break;
              }
            }
            if (!flag)
              this.m_FamilyList.Add(new FamilyContainer(((Element) familySymbol2.Family).Name)
              {
                FamilySymbolList = {
                  familySymbol2
                }
              });
          }
        }
      }
    }
    bool flag1 = true;
    FilteredElementCollector source1 = new FilteredElementCollector(this.m_Document);
    source1.OfClass(typeof (FamilySymbol)).OfCategory((BuiltInCategory) -2001260);
    CurSiteworksID = (Parameter) null;
    List<Element> list1 = ((IEnumerable<Element>) source1).Where<Element>((Func<Element, bool>) (CurrentElement =>
    {
      if ((CurSiteworksID = CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) == null)
        return false;
      return string.Compare(CurSiteworksID.AsString(), Convert.ToInt32((object) LandcaddID.SiteComponentPlacement).ToString("000"), true) == 0 || string.Compare(CurSiteworksID.AsString(), Convert.ToInt32((object) LandcaddID.IntersectionPlacement).ToString("000"), true) == 0;
    })).ToList<Element>();
    bool flag2 = false;
    if (this.m_CurrentLandcaddId == LandcaddID.Sidewalk || this.m_CurrentLandcaddId == LandcaddID.Street)
      flag1 = false;
    familySymbol1 = (FamilySymbol) null;
    foreach (Element element in list1)
    {
      if (element is FamilySymbol familySymbol3 && ((Element) familySymbol3).Category != null)
      {
        CurSiteworksID = ((Element) familySymbol3).LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
        if (CurSiteworksID != null)
        {
          if (string.Compare(CurSiteworksID.AsString(), Convert.ToInt32((object) LandcaddID.SiteComponentPlacement).ToString("000"), true) == 0)
          {
            this.m_PlacementSymbol = familySymbol3;
            if (this.m_PlacementSymbol != null && !this.m_PlacementSymbol.IsActive)
            {
              this.m_TransactionContainer.Start("ActivateFamilySymbol");
              this.m_PlacementSymbol.Activate();
              this.m_TransactionContainer.Commit();
            }
            flag2 = true;
            if (flag1)
              break;
          }
          else if (string.Compare(CurSiteworksID.AsString(), Convert.ToInt32((object) LandcaddID.IntersectionPlacement).ToString("000"), true) == 0)
          {
            this.m_IntersectionPlacementSymbol = familySymbol3;
            if (this.m_PlacementSymbol != null && !this.m_PlacementSymbol.IsActive)
            {
              this.m_TransactionContainer.Start("ActivateFamilySymbol");
              this.m_PlacementSymbol.Activate();
              this.m_TransactionContainer.Commit();
            }
            flag1 = true;
            if (flag2)
              break;
          }
        }
      }
    }
    string empty1 = string.Empty;
    Family _Family = (Family) null;
    if (this.m_CurrentLandcaddId != LandcaddID.FeatureLine && this.m_CurrentLandcaddId != LandcaddID.PointWipeout)
    {
      if (_bCheckForFamily && this.m_FamilyList.Count == 0)
      {
        List<string> _WarningOptions = new List<string>();
        _WarningOptions.Add(EPShared.LocalRM.GetString("AllwaysLoadDefaultFamiliesOption"));
        _WarningOptions.Add(EPShared.LocalRM.GetString("ContinueWOLoadFamilyOption"));
        string _strSettingName = "LocateSiteComponent";
        string empty2 = string.Empty;
        string _strSearchFilename = string.Empty;
        string empty3 = string.Empty;
        switch (this.m_CurrentLandcaddId)
        {
          case LandcaddID.SoftTerrain:
            empty2 = EPShared.LocalRM.GetString("SoftTerrainFamilyNotLoadedMessage");
            _strSearchFilename = "Sample Soft Terrain.rfa";
            empty3 = EPShared.LocalRM.GetString("LoadSoftTerrainFamilyTransaction");
            break;
          case LandcaddID.Street:
            empty2 = EPShared.LocalRM.GetString("StreetFamilyNotLoadedMessage");
            _strSearchFilename = "Sample Street.rfa";
            empty3 = EPShared.LocalRM.GetString("LoadStreetFamilyTransaction");
            break;
          case LandcaddID.Curb:
            empty2 = EPShared.LocalRM.GetString("CurbFamilyNotLoadedMessage");
            _strSearchFilename = "Sample Curb.rfa";
            empty3 = EPShared.LocalRM.GetString("LoadCurbFamilyTransaction");
            break;
          case LandcaddID.Sidewalk:
            empty2 = EPShared.LocalRM.GetString("SideWalkFamilyNotLoadedMessage");
            _strSearchFilename = "Sample Sidewalk.rfa";
            empty3 = EPShared.LocalRM.GetString("LoadSidewalkFamilyTransaction");
            break;
          case LandcaddID.RetainingWall:
            empty2 = EPShared.LocalRM.GetString("RetainingWallFamilyNotLoadedMessage");
            _strSearchFilename = "Sample Retaining Wall.rfa";
            empty3 = EPShared.LocalRM.GetString("LoadRetainingWallFamilyTransaction");
            break;
          case LandcaddID.ParkingLot:
            empty2 = EPShared.LocalRM.GetString("ParkingLotFamilyNotLoadedMessage");
            _strSearchFilename = "Sample Parking Lot.rfa";
            empty3 = EPShared.LocalRM.GetString("LoadParkingLotFamilyTransaction");
            break;
        }
        WarningOptionsForm warningOptionsForm = new WarningOptionsForm(EPShared.LocalRM.GetString("WarningDialog"), this.m_Document.PathName, _strSettingName, empty2, _WarningOptions, 0, true, false, 0);
        int num1 = (int) warningOptionsForm.ShowDialog();
        if (warningOptionsForm.DialogResult == DialogResult.OK)
        {
          if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("AllwaysLoadDefaultFamiliesOption"), true) == 0)
          {
            this.m_bContinueWithoutLoading = false;
            if (RevitUtility.GetFullFamilyFilenameIfExist(_strSearchFilename, ref empty1))
            {
              int num2 = empty1.LastIndexOf("\\");
              int num3 = empty1.LastIndexOf(".");
              if (this.CheckForDuplicateSymbols(empty1.Substring(num2 + 1, num3 - num2 - 1)))
              {
                int num4 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("DuplicateFamilySymbolMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
                return;
              }
              this.m_TransactionContainer.Start(empty3 + Path.GetFileName(empty1));
              RevitUtility.LoadFamily(this.m_Document, empty1, out _Family, true, ref this.m_TransactionContainer);
              this.m_TransactionContainer.Commit();
              if (_Family != null && _Family.GetFamilySymbolIds() != null)
              {
                foreach (ElementId familySymbolId in (IEnumerable<ElementId>) _Family.GetFamilySymbolIds())
                {
                  if (this.m_Document.GetElement(familySymbolId) is FamilySymbol element)
                  {
                    Parameter parameter = ((Element) element).LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
                    if (parameter != null && string.Compare(parameter.AsString(), Convert.ToInt32((object) this.m_CurrentLandcaddId).ToString("000"), true) == 0)
                    {
                      bool flag3 = false;
                      for (int index = 0; index < this.m_FamilyList.Count; ++index)
                      {
                        if (string.Compare(this.m_FamilyList[index].FamilyName, ((Element) element.Family).Name, true) == 0)
                        {
                          this.m_FamilyList[index].FamilySymbolList.Add(element);
                          flag3 = true;
                          break;
                        }
                      }
                      if (!flag3)
                        this.m_FamilyList.Add(new FamilyContainer(((Element) element.Family).Name)
                        {
                          FamilySymbolList = {
                            element
                          }
                        });
                    }
                  }
                }
                if (this.m_FamilyList.Count <= 0)
                {
                  int num5 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("NoFamiliesGenericMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
                }
              }
            }
          }
          else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("ContinueWOLoadFamilyOption"), true) == 0)
            this.m_bContinueWithoutLoading = true;
        }
      }
    }
    else
      this.m_bContinueWithoutLoading = true;
    if (!flag2 && RevitUtility.GetFullFamilyFilenameIfExist("Site Component Placement.rfa", ref empty1))
    {
      this.m_TransactionContainer.Start(EPShared.LocalRM.GetString("LocatePlacementFamilyTransaction") + Path.GetFileName(empty1));
      RevitUtility.LoadFamily(this.m_Document, empty1, out _Family, true, ref this.m_TransactionContainer);
      this.m_TransactionContainer.Commit();
      if (_Family != null && _Family.GetFamilySymbolIds() != null)
      {
        using (IEnumerator<ElementId> enumerator = _Family.GetFamilySymbolIds().GetEnumerator())
        {
          if (enumerator.MoveNext())
          {
            if (this.m_Document.GetElement(enumerator.Current) is FamilySymbol element)
              this.m_PlacementSymbol = element;
          }
        }
      }
    }
    if (!flag1 && RevitUtility.GetFullFamilyFilenameIfExist("Intersection Placement.rfa", ref empty1))
    {
      this.m_TransactionContainer.Start(EPShared.LocalRM.GetString("LocatePlacementFamilyTransaction") + Path.GetFileName(empty1));
      RevitUtility.LoadFamily(this.m_Document, empty1, out _Family, true, ref this.m_TransactionContainer);
      this.m_TransactionContainer.Commit();
      if (_Family != null && _Family.GetFamilySymbolIds() != null)
      {
        using (IEnumerator<ElementId> enumerator = _Family.GetFamilySymbolIds().GetEnumerator())
        {
          if (enumerator.MoveNext())
          {
            if (this.m_Document.GetElement(enumerator.Current) is FamilySymbol element)
              this.m_IntersectionPlacementSymbol = element;
          }
        }
      }
    }
    FamilySymbolComparer familySymbolComparer = new FamilySymbolComparer();
    for (int index = 0; index < this.m_FamilyList.Count; ++index)
      this.m_FamilyList[index].FamilySymbolList.Sort((IComparer<FamilySymbol>) familySymbolComparer);
  }

  private void InitializeListData() => this.PopulateFamilyAndSymbolLists(true);

  public void Run() => this.DisplayUI();

  private bool DisplayUI()
  {
    bool flag = false;
    if (!this.m_bContinueWithoutLoading && this.m_FamilyList.Count <= 0)
    {
      string str = EPShared.LocalRM.GetString("NoString") + " ";
      string empty = string.Empty;
      switch (this.m_CurrentLandcaddId)
      {
        case LandcaddID.SoftTerrain:
          empty = EPShared.LocalRM.GetString("ModifySoftTerrainRibbonButton");
          break;
        case LandcaddID.Street:
          empty = EPShared.LocalRM.GetString("StreetFamilyManagerRibbonButton");
          break;
        case LandcaddID.Curb:
          empty = EPShared.LocalRM.GetString("CurbFamilyManagerRibbonButton");
          break;
        case LandcaddID.Sidewalk:
          empty = EPShared.LocalRM.GetString("LocateSidewalkRibbonButton");
          break;
        case LandcaddID.RetainingWall:
          empty = EPShared.LocalRM.GetString("LocateRetainingWallRibbonButton");
          break;
        case LandcaddID.Steps:
          empty = EPShared.LocalRM.GetString("");
          break;
        case LandcaddID.ParkingLot:
          empty = EPShared.LocalRM.GetString("ParkingLotFamilyManagerRibbonButton");
          break;
      }
      int num = (int) RevitMessageBox.Show(str + empty + EPShared.LocalRM.GetString("NoFamiliesSitesMessage") + empty + EPShared.LocalRM.GetString("NoFamiliesGenericCreateMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
    }
    DialogResult _DialogResult = DialogResult.OK;
    if (this.m_CurrentLandcaddId == LandcaddID.Curb)
    {
      using (this.m_CurbDisplayForm = new LocateSiteCurbComponentForm(this))
      {
        while (_DialogResult != DialogResult.Cancel)
        {
          flag = this.ShowDialog(ref _DialogResult);
          if (_DialogResult != DialogResult.Retry)
          {
            if (flag)
            {
              this.m_TransactionContainer.Commit();
              this.m_TransactionContainer.AssimilateTransactionGroup();
              this.m_CurbDisplayForm.ResetSiteComponentName();
            }
            else
            {
              this.m_TransactionContainer.Rollback();
              this.m_TransactionContainer.RollbackTransactionGroup();
            }
          }
        }
      }
    }
    else if (this.m_CurrentLandcaddId == LandcaddID.ParkingLot)
    {
      using (this.m_ParkingLotDisplayForm = new LocateSiteParkingLotComponentForm(this))
      {
        while (_DialogResult != DialogResult.Cancel)
        {
          flag = this.ShowDialog(ref _DialogResult);
          if (_DialogResult != DialogResult.Retry)
          {
            if (flag)
            {
              this.m_TransactionContainer.Commit();
              this.m_TransactionContainer.AssimilateTransactionGroup();
              this.m_ParkingLotDisplayForm.ResetSiteComponentName();
            }
            else
            {
              this.m_TransactionContainer.Rollback();
              this.m_TransactionContainer.RollbackTransactionGroup();
            }
          }
        }
      }
    }
    else if (this.m_CurrentLandcaddId == LandcaddID.FeatureLine)
    {
      using (this.m_FeatureLineDisplayForm = new LocateSiteFeatureLineComponentForm(this))
      {
        while (_DialogResult != DialogResult.Cancel)
        {
          flag = this.ShowDialog(ref _DialogResult);
          if (_DialogResult != DialogResult.Retry)
          {
            if (flag)
            {
              this.m_TransactionContainer.Commit();
              this.m_TransactionContainer.AssimilateTransactionGroup();
            }
            else
            {
              this.m_TransactionContainer.Rollback();
              this.m_TransactionContainer.RollbackTransactionGroup();
            }
          }
        }
      }
    }
    else if (this.m_CurrentLandcaddId == LandcaddID.PointWipeout)
    {
      using (this.m_PointWipeoutDisplayForm = new LocateSitePointWipeoutComponentForm(this))
      {
        while (_DialogResult != DialogResult.Cancel)
        {
          flag = this.ShowDialog(ref _DialogResult);
          if (_DialogResult != DialogResult.Retry)
          {
            if (flag)
            {
              this.m_TransactionContainer.Commit();
              this.m_TransactionContainer.AssimilateTransactionGroup();
            }
            else
            {
              this.m_TransactionContainer.Rollback();
              this.m_TransactionContainer.RollbackTransactionGroup();
            }
          }
        }
      }
    }
    else
    {
      using (this.m_DisplayForm = new LocateSiteComponentForm(this, this.m_CurrentLandcaddId))
      {
        while (_DialogResult != DialogResult.Cancel)
        {
          flag = this.ShowDialog(ref _DialogResult);
          if (_DialogResult != DialogResult.Retry)
          {
            if (flag)
            {
              this.m_TransactionContainer.Commit();
              this.m_TransactionContainer.AssimilateTransactionGroup();
              this.m_DisplayForm.ResetSiteComponentName();
            }
            else
            {
              this.m_TransactionContainer.Rollback();
              this.m_TransactionContainer.RollbackTransactionGroup();
            }
          }
        }
      }
    }
    return flag;
  }

  private bool ShowDialog(ref DialogResult _DialogResult)
  {
    int _nReturnValue = 0;
    if ((this.m_CurrentLandcaddId == LandcaddID.SoftTerrain || this.m_CurrentLandcaddId == LandcaddID.ParkingLot || this.m_CurrentLandcaddId == LandcaddID.FeatureLine || this.m_CurrentLandcaddId == LandcaddID.PointWipeout) && ApplicationUtility.Settings.eplSetE_GetSetting("MassCreationOnlyMode", ref _nReturnValue) && _nReturnValue != 0)
    {
      int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("MassCreationOnlyModeOnMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
      _DialogResult = DialogResult.Cancel;
      return false;
    }
    double _dReturnValue = 0.0;
    ApplicationUtility.Settings.eplSetE_GetSetting("TopographySurfaceArcPointInterval", ref _dReturnValue);
    if (RevitDefines.LE(_dReturnValue, 0.0))
    {
      _dReturnValue = 1.0;
      ApplicationUtility.Settings.eplSetE_SetSetting("TopographySurfaceArcPointInterval", _dReturnValue);
    }
    if (this.m_CurrentLandcaddId == LandcaddID.Curb)
    {
      int num = (int) this.m_CurbDisplayForm.ShowDialog();
      _DialogResult = this.m_CurbDisplayForm.DialogResult;
    }
    else if (this.m_CurrentLandcaddId == LandcaddID.ParkingLot)
    {
      int num = (int) this.m_ParkingLotDisplayForm.ShowDialog();
      _DialogResult = this.m_ParkingLotDisplayForm.DialogResult;
    }
    else if (this.m_CurrentLandcaddId == LandcaddID.FeatureLine)
    {
      int num = (int) this.m_FeatureLineDisplayForm.ShowDialog();
      _DialogResult = this.m_FeatureLineDisplayForm.DialogResult;
    }
    else if (this.m_CurrentLandcaddId == LandcaddID.PointWipeout)
    {
      int num = (int) this.m_PointWipeoutDisplayForm.ShowDialog();
      _DialogResult = this.m_PointWipeoutDisplayForm.DialogResult;
    }
    else
    {
      int num = (int) this.m_DisplayForm.ShowDialog();
      _DialogResult = this.m_DisplayForm.DialogResult;
    }
    if (_DialogResult == DialogResult.Cancel)
      return false;
    if (_DialogResult == DialogResult.Retry)
    {
      Eps_Point _Point = new Eps_Point();
      RevitUtility.GetPointFromProject(this.m_Document, EPShared.LocalRM.GetString("PickPointPrompt"), ref _Point, false);
      double _dElevation = 0.0;
      if (this.m_ProposedSurface.GetSurfaceContainingPoint(_Point, ref _dElevation) != null)
      {
        if (this.m_CurrentLandcaddId == LandcaddID.FeatureLine)
          this.m_FeatureLineDisplayForm.SetFocusedElevation(_dElevation);
        else if (this.m_CurrentLandcaddId != LandcaddID.Curb && this.m_CurrentLandcaddId != LandcaddID.ParkingLot && this.m_CurrentLandcaddId != LandcaddID.PointWipeout)
          this.m_DisplayForm.SetFocusedElevation(_dElevation);
      }
      else
      {
        int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("PointNotInProposedBoundaryMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }
    else if (_DialogResult == DialogResult.OK)
    {
      _nReturnValue = 0;
      if ((!ApplicationUtility.Settings.eplSetE_GetSetting("SiteComponentVerifyPlacementParametersExist", ref _nReturnValue) || _nReturnValue == 0) && this.m_PlacementSymbol != null)
      {
        List<FamilySymbolParameterContainer> FamilySymbolParameterContainerList = new List<FamilySymbolParameterContainer>();
        if (this.m_CurrentLandcaddId == LandcaddID.FeatureLine)
        {
          FamilySymbolParameterContainer parameterContainer = new FamilySymbolParameterContainer(RevitParameters.LinearPointIntervalParameter, (BuiltInParameterGroup) -5000101, (ParameterType) 4, true);
          FamilySymbolParameterContainerList.Add(parameterContainer);
        }
        FamilySymbolParameterContainer parameterContainer1 = new FamilySymbolParameterContainer(RevitParameters.BeginningElevationFamilyParameter, (BuiltInParameterGroup) -5000101, (ParameterType) 4, true);
        FamilySymbolParameterContainerList.Add(parameterContainer1);
        FamilySymbolParameterContainer parameterContainer2 = new FamilySymbolParameterContainer(RevitParameters.EndingElevationFamilyParameter, (BuiltInParameterGroup) -5000101, (ParameterType) 4, true);
        FamilySymbolParameterContainerList.Add(parameterContainer2);
        FamilySymbolParameterContainer parameterContainer3 = new FamilySymbolParameterContainer(RevitParameters.UsePlacementElevationsFamilyParameter, (BuiltInParameterGroup) -1, (ParameterType) 10, true);
        FamilySymbolParameterContainerList.Add(parameterContainer3);
        RevitUtility.AddParametersToFamilySymbol(this.m_Document, this.m_TransactionContainer, FamilySymbolParameterContainerList, ref this.m_PlacementSymbol);
        ApplicationUtility.Settings.eplSetE_SetSetting("SiteComponentVerifyPlacementParametersExist", 1);
      }
      string empty1 = string.Empty;
      switch (this.m_CurrentLandcaddId)
      {
        case LandcaddID.SoftTerrain:
          empty1 = EPShared.LocalRM.GetString("LocateSoftTerrainTransaction");
          break;
        case LandcaddID.Street:
          empty1 = EPShared.LocalRM.GetString("LocateStreetTransaction");
          break;
        case LandcaddID.Curb:
          empty1 = EPShared.LocalRM.GetString("LocateCurbTransaction");
          break;
        case LandcaddID.Sidewalk:
          empty1 = EPShared.LocalRM.GetString("LocateSidewalkTransaction");
          break;
        case LandcaddID.RetainingWall:
          empty1 = EPShared.LocalRM.GetString("LocateRetainingWallTransaction");
          break;
        case LandcaddID.ParkingLot:
          empty1 = EPShared.LocalRM.GetString("LocateParkingLotTransaction");
          break;
        case LandcaddID.FeatureLine:
          empty1 = EPShared.LocalRM.GetString("LocateFeatureLineTransaction");
          break;
        case LandcaddID.PointWipeout:
          empty1 = EPShared.LocalRM.GetString("LocatePointWipeoutTransaction");
          break;
      }
      this.m_TransactionContainer.StartTransactionGroup(empty1);
      this.m_TransactionContainer.Start(empty1);
      this.m_TransactionContainer.FailureHandlingOptions(new List<FailureDefinitionId>()
      {
        BuiltInFailures.InaccurateFailures.InaccurateLine,
        BuiltInFailures.MassFailures.TooManyGeometries
      });
      FamilySymbol _Type = (FamilySymbol) null;
      string empty2 = string.Empty;
      string sitePlacementName;
      if (this.m_CurrentLandcaddId == LandcaddID.Curb)
      {
        _Type = this.m_CurbDisplayForm.Type;
        sitePlacementName = this.m_CurbDisplayForm.SitePlacementName;
      }
      else if (this.m_CurrentLandcaddId == LandcaddID.ParkingLot)
      {
        _Type = this.m_ParkingLotDisplayForm.Type;
        sitePlacementName = this.m_ParkingLotDisplayForm.SitePlacementName;
      }
      else if (this.m_CurrentLandcaddId == LandcaddID.FeatureLine)
        sitePlacementName = EPShared.LocalRM.GetString("FeatureLineString");
      else if (this.m_CurrentLandcaddId == LandcaddID.PointWipeout)
      {
        sitePlacementName = EPShared.LocalRM.GetString("PointWipeoutString");
      }
      else
      {
        _Type = this.m_DisplayForm.Type;
        sitePlacementName = this.m_DisplayForm.SitePlacementName;
      }
      SitePlacementSettings _PlacementSettings = new SitePlacementSettings(this.m_CurrentLandcaddId, this.m_Document.ActiveView.GenLevel, _Type, this.m_PlacementSymbol, sitePlacementName, this.m_nMaxExistingTopoDrawOrder, this.m_TransactionContainer);
      _PlacementSettings.IntersectionPlacementSymbol = this.m_IntersectionPlacementSymbol;
      ElementSet _ElementSet = new ElementSet();
      ElementSet elm_set = new ElementSet();
      List<Element> elementList1 = new List<Element>();
      List<Element> _VerifyElementList = new List<Element>();
      string empty3 = string.Empty;
      double num1 = 0.0;
      double num2 = 0.0;
      double _dHeight = 0.0;
      double num3 = 0.0;
      bool _bSlopeProjection = false;
      switch (this.m_CurrentLandcaddId)
      {
        case LandcaddID.SoftTerrain:
          Parameter parameter1 = ((Element) this.m_DisplayForm.Type).LookupParameter(RevitParameters.SoftTerrainLeftOffsetWidthParameter);
          if (parameter1 != null)
            num1 = parameter1.AsDouble();
          Parameter parameter2 = ((Element) this.m_DisplayForm.Type).LookupParameter(RevitParameters.SoftTerrainRightOffsetWidthParameter);
          if (parameter2 != null)
          {
            num2 = parameter2.AsDouble();
            break;
          }
          break;
        case LandcaddID.Street:
          double num4 = 0.0;
          Parameter parameter3 = ((Element) this.m_DisplayForm.Type).LookupParameter(RevitParameters.StreetLaneWidthParameter);
          if (parameter3 != null)
            num4 = parameter3.AsDouble();
          Parameter parameter4 = ((Element) this.m_DisplayForm.Type).LookupParameter(RevitParameters.StreetNumberOfLanesParameter);
          if (parameter4 != null)
          {
            num1 = num4 * parameter4.AsDouble() / 2.0;
            num2 = num4 * parameter4.AsDouble() / 2.0;
            break;
          }
          break;
        case LandcaddID.Curb:
          double num5 = 0.0;
          double num6 = 0.0;
          Parameter parameter5 = ((Element) this.m_CurbDisplayForm.Type).LookupParameter(RevitParameters.StreetCurbWidthParameter);
          if (parameter5 != null)
            num5 = parameter5.AsDouble();
          Parameter parameter6 = ((Element) this.m_CurbDisplayForm.Type).LookupParameter(RevitParameters.StreetGutterWidthParameter);
          if (parameter6 != null)
            num6 = parameter6.AsDouble();
          num1 = num5;
          num2 = num6;
          _bSlopeProjection = false;
          Parameter parameter7 = ((Element) this.m_CurbDisplayForm.Type).LookupParameter(RevitParameters.LinearIncrementParameter);
          if (parameter7 != null)
            num3 = parameter7.AsDouble();
          if (RevitDefines.LT(num3, 0.0833333))
          {
            num3 = 0.0833333;
            break;
          }
          break;
        case LandcaddID.Sidewalk:
          Parameter parameter8 = ((Element) this.m_DisplayForm.Type).LookupParameter(RevitParameters.SideWalkWidthParameter);
          if (parameter8 != null)
          {
            double num7 = parameter8.AsDouble();
            num1 = num7 / 2.0;
            num2 = num7 / 2.0;
            break;
          }
          break;
        case LandcaddID.RetainingWall:
          Parameter parameter9 = ((Element) this.m_DisplayForm.Type).LookupParameter(RevitParameters.RetainingWallWidthParameter);
          if (parameter9 != null)
          {
            double num8 = parameter9.AsDouble();
            num1 = num8 / 2.0;
            num2 = num8 / 2.0;
          }
          Parameter parameter10 = ((Element) this.m_DisplayForm.Type).LookupParameter(RevitParameters.RetainingWallSlopeProjectionParameter);
          if (parameter10 != null)
            _bSlopeProjection = parameter10.AsInteger() == 1;
          Parameter parameter11 = ((Element) this.m_DisplayForm.Type).LookupParameter(RevitParameters.RetainingWallHeightParameter);
          if (parameter11 != null)
            _dHeight = parameter11.AsDouble();
          Parameter parameter12 = ((Element) this.m_DisplayForm.Type).LookupParameter(RevitParameters.LinearIncrementParameter);
          if (parameter12 != null)
            num3 = parameter12.AsDouble();
          if (RevitDefines.LT(num3, 0.0833333))
          {
            num3 = 0.0833333;
            break;
          }
          break;
      }
      double num9 = num1 + num2;
      if (RevitDefines.EQ(num9, 0.0))
        num9 = 10.0;
      bool flag1 = this.m_CurrentLandcaddId != LandcaddID.Curb ? (this.m_CurrentLandcaddId != LandcaddID.ParkingLot ? (this.m_CurrentLandcaddId != LandcaddID.FeatureLine ? (this.m_CurrentLandcaddId != LandcaddID.PointWipeout ? this.m_DisplayForm.ExistingHostLineChecked : this.m_PointWipeoutDisplayForm.ExistingHostLineChecked) : this.m_FeatureLineDisplayForm.ExistingHostLineChecked) : this.m_ParkingLotDisplayForm.ExistingHostLineChecked) : this.m_CurbDisplayForm.ExistingHostLineChecked;
      foreach (View allView in RevitUtility.FindAllViews(this.m_Document))
        _VerifyElementList.Add((Element) allView);
      Category componentLineSubCategory = RevitUtility.GetSiteComponentLineSubCategory(this.m_Document, this.m_CurrentLandcaddId);
      if (flag1)
        return this.PickObjectAndCreateSiteComponent(_PlacementSettings, componentLineSubCategory, num9, num3, _dReturnValue, _dHeight, _bSlopeProjection);
      string _strPrompt = EPShared.LocalRM.GetString("PickStartPointPrompt");
      List<Element> _LineElementList = new List<Element>();
      List<Element> elementList2 = new List<Element>();
      Eps_Leg epsLeg1 = (Eps_Leg) null;
      Eps_Leg _CompleteLeg1 = (Eps_Leg) null;
      Eps_Point _Point = (Eps_Point) null;
      Eps_Point epsPoint = (Eps_Point) null;
      bool flag2 = true;
      while (flag2)
      {
        flag2 = RevitUtility.GetPointFromProject(this.m_Document, _strPrompt, ref _Point, false);
        if (flag2)
        {
          if (epsPoint != null)
          {
            if (!epsPoint.IsEqual(_Point))
            {
              _CompleteLeg1 = new Eps_Leg();
              Eps_Leg epsLeg2;
              if (epsLeg1 == null)
              {
                _CompleteLeg1.Line = new Eps_Line();
                _CompleteLeg1.Line.StartPnt = epsPoint;
                _CompleteLeg1.Line.EndPnt = _Point;
                epsLeg2 = Eps_Leg.SingleLeg(_CompleteLeg1);
                epsLeg1 = Eps_Leg.SingleLeg(epsLeg2);
              }
              else
              {
                Eps_Line _pOut = new Eps_Line();
                _pOut.StartPnt = epsPoint;
                _pOut.EndPnt = _Point;
                double _dRadius = this.m_CurrentLandcaddId != LandcaddID.Curb ? (this.m_CurrentLandcaddId != LandcaddID.ParkingLot ? (this.m_CurrentLandcaddId != LandcaddID.FeatureLine ? (this.m_CurrentLandcaddId != LandcaddID.PointWipeout ? this.m_DisplayForm.Radius : this.m_PointWipeoutDisplayForm.Radius) : this.m_FeatureLineDisplayForm.Radius) : this.m_ParkingLotDisplayForm.Radius) : this.m_CurbDisplayForm.Radius;
                Eps_Arc _pArc = new Eps_Arc();
                if (RevitGeometry.SolveArcFromTangentLines(epsLeg1.Last.Line, _pOut, _dRadius, ref _pArc, true) == 0)
                {
                  _CompleteLeg1.Line = new Eps_Line();
                  _CompleteLeg1.Line.StartPnt = new Eps_Point(epsLeg1.Last.Line.StartPnt);
                  _CompleteLeg1.Line.EndPnt = new Eps_Point(_pArc.StartPnt);
                  epsLeg2 = Eps_Leg.SingleLeg(_CompleteLeg1);
                  epsLeg2.Last.Next = Eps_Leg.SingleLeg(new Eps_Leg()
                  {
                    Arc = new Eps_Arc(_pArc)
                  });
                  _CompleteLeg1 = new Eps_Leg();
                  _CompleteLeg1.Line = new Eps_Line();
                  _CompleteLeg1.Line.StartPnt = new Eps_Point(_pArc.EndPnt);
                  _CompleteLeg1.Line.EndPnt = _Point;
                  epsLeg2.Last.Next = Eps_Leg.SingleLeg(_CompleteLeg1);
                  Eps_Leg _CompleteLeg2 = Eps_Leg.CopyLeg(epsLeg1);
                  epsLeg1 = (Eps_Leg) null;
                  for (; _CompleteLeg2.Next != null; _CompleteLeg2 = _CompleteLeg2.Next)
                  {
                    if (epsLeg1 == null)
                      epsLeg1 = Eps_Leg.SingleLeg(_CompleteLeg2);
                    else
                      epsLeg1.Last.Next = Eps_Leg.SingleLeg(_CompleteLeg2);
                  }
                  if (epsLeg1 == null)
                    epsLeg1 = Eps_Leg.CopyLeg(epsLeg2);
                  else
                    epsLeg1.Last.Next = Eps_Leg.CopyLeg(epsLeg2);
                }
                else
                {
                  _CompleteLeg1.Line = _pOut;
                  epsLeg2 = Eps_Leg.SingleLeg(_CompleteLeg1);
                  epsLeg1.Last.Next = Eps_Leg.SingleLeg(epsLeg2);
                }
                if (EPShared.eplGeoE_ArePntsDuplicates2D(epsLeg1.LegStartPnt, epsLeg1.LegEndPnt))
                  flag2 = false;
              }
              this.m_TransactionContainer.BeginTransaction();
              if (epsLeg2.LegCount > 1)
              {
                if (_LineElementList.Count > 0)
                {
                  this.m_Document.Delete(_LineElementList[_LineElementList.Count - 1].Id);
                  _LineElementList.RemoveAt(_LineElementList.Count - 1);
                }
                if (elementList2.Count > 0)
                {
                  this.m_Document.Delete(elementList2[elementList2.Count - 1].Id);
                  elementList2.RemoveAt(elementList2.Count - 1);
                }
              }
              foreach (ModelCurve newModelCurve in RevitUtility.NewModelCurveArray(this.m_Document, epsLeg2))
              {
                if (componentLineSubCategory != null)
                  ((CurveElement) newModelCurve).LineStyle = (Element) componentLineSubCategory.GetGraphicsStyle((GraphicsStyleType) 1);
                _LineElementList.Add((Element) newModelCurve);
                _ElementSet.Insert((Element) newModelCurve);
              }
              if (_ElementSet.Size > 0)
              {
                RevitUtility.HideElementsInAllViews(this.m_Document, _ElementSet);
                _ElementSet.Clear();
              }
              foreach (ModelCurve newModelCurve in RevitUtility.NewModelCurveArray(this.m_Document, epsLeg2))
              {
                if (componentLineSubCategory != null)
                  ((CurveElement) newModelCurve).LineStyle = (Element) componentLineSubCategory.GetGraphicsStyle((GraphicsStyleType) 1);
                elementList2.Add((Element) newModelCurve);
              }
              this.m_Document.Regenerate();
              this.m_TransactionContainer.EndTransaction();
              if (this.m_CurrentLandcaddId == LandcaddID.Curb)
              {
                if (this.m_CurbDisplayForm.SegmentChecked)
                  flag2 = false;
              }
              else if (this.m_CurrentLandcaddId == LandcaddID.FeatureLine)
              {
                if (this.m_FeatureLineDisplayForm.SegmentChecked)
                  flag2 = false;
              }
              else if (this.m_CurrentLandcaddId != LandcaddID.ParkingLot && this.m_CurrentLandcaddId != LandcaddID.PointWipeout && this.m_DisplayForm.SegmentChecked)
                flag2 = false;
            }
          }
          else
            _strPrompt = EPShared.LocalRM.GetString("PickEndPointPrompt");
          epsPoint = _Point;
        }
      }
      for (int index = 0; index < elementList2.Count; ++index)
        elm_set.Insert(elementList2[index]);
      bool flag3 = true;
      if (epsLeg1 != null)
      {
        if (epsLeg1.IsLegClosed())
        {
          Eps_Arc _pArc = new Eps_Arc();
          double _dRadius = this.m_CurrentLandcaddId != LandcaddID.Curb ? (this.m_CurrentLandcaddId != LandcaddID.ParkingLot ? (this.m_CurrentLandcaddId != LandcaddID.FeatureLine ? (this.m_CurrentLandcaddId != LandcaddID.PointWipeout ? this.m_DisplayForm.Radius : this.m_PointWipeoutDisplayForm.Radius) : this.m_FeatureLineDisplayForm.Radius) : this.m_ParkingLotDisplayForm.Radius) : this.m_CurbDisplayForm.Radius;
          if (RevitGeometry.SolveArcFromTangentLines(epsLeg1.Last.Line, epsLeg1.Line, _dRadius, ref _pArc, true) == 0)
          {
            _CompleteLeg1.Line = new Eps_Line();
            _CompleteLeg1.Line.StartPnt = new Eps_Point(epsLeg1.Last.Line.StartPnt);
            _CompleteLeg1.Line.EndPnt = new Eps_Point(_pArc.StartPnt);
            Eps_Leg _CompleteLeg3 = Eps_Leg.SingleLeg(_CompleteLeg1);
            _CompleteLeg3.Last.Next = Eps_Leg.SingleLeg(new Eps_Leg()
            {
              Arc = new Eps_Arc(_pArc)
            });
            Eps_Leg _CompleteLeg4 = Eps_Leg.CopyLeg(epsLeg1);
            epsLeg1 = (Eps_Leg) null;
            for (; _CompleteLeg4.Next != null; _CompleteLeg4 = _CompleteLeg4.Next)
            {
              if (epsLeg1 == null)
              {
                Eps_Leg _CompleteLeg5 = new Eps_Leg();
                _CompleteLeg5.Line = new Eps_Line();
                _CompleteLeg5.Line.StartPnt = new Eps_Point(_pArc.EndPnt);
                _CompleteLeg5.Line.EndPnt = new Eps_Point(_CompleteLeg4.Line.EndPnt);
                epsLeg1 = Eps_Leg.SingleLeg(_CompleteLeg5);
              }
              else
                epsLeg1.Last.Next = Eps_Leg.SingleLeg(_CompleteLeg4);
            }
            if (epsLeg1 == null)
              epsLeg1 = Eps_Leg.CopyLeg(_CompleteLeg3);
            else
              epsLeg1.Last.Next = Eps_Leg.CopyLeg(_CompleteLeg3);
            foreach (Element element in _LineElementList)
              this.m_Document.Delete(element.Id);
            _LineElementList.Clear();
            foreach (ModelCurve newModelCurve in RevitUtility.NewModelCurveArray(this.m_Document, epsLeg1, this.m_Document.ActiveView, false))
            {
              if (componentLineSubCategory != null)
                ((CurveElement) newModelCurve).LineStyle = (Element) componentLineSubCategory.GetGraphicsStyle((GraphicsStyleType) 1);
              _LineElementList.Add((Element) newModelCurve);
              _ElementSet.Insert((Element) newModelCurve);
            }
            if (_ElementSet.Size > 0)
            {
              RevitUtility.HideElementsInAllViews(this.m_Document, _ElementSet);
              _ElementSet.Clear();
            }
          }
          if (this.m_CurrentLandcaddId == LandcaddID.SoftTerrain)
            _PlacementSettings.ProjectInnerSlopes = RevitMessageBox.Show(EPShared.LocalRM.GetString("ChainPickedClosedProjectInnerSlopesMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes ? 1 : 0;
        }
        else
        {
          if (this.m_CurrentLandcaddId == LandcaddID.ParkingLot)
          {
            int num10 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("ParkingLotBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
          }
          if (this.m_CurrentLandcaddId == LandcaddID.PointWipeout)
          {
            int num11 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("PointWipeoutBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
          }
        }
        if (this.m_CurrentLandcaddId == LandcaddID.RetainingWall)
        {
          if (!this.m_DisplayForm.RelativeElevationChecked)
          {
            double _dMinElevation = 0.0;
            double _dMaxElevation = 0.0;
            if (epsLeg1.FindMinAndMaxElevationOfLeg(this.m_ProposedSurface, num3, 1.0, ref _dMinElevation, ref _dMaxElevation))
            {
              if (RevitDefines.LT(_dMinElevation, this.m_DisplayForm.Elevation - _dHeight))
              {
                int num12 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("WallCantBePlacedWithDaylightingMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
              }
              if (RevitDefines.GT(_dMaxElevation, this.m_DisplayForm.Elevation))
              {
                int num13 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("WallCantBePlacedWithGreaterElevationMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
              }
            }
          }
          else if (this.m_DisplayForm.RelativeElevationChecked)
          {
            double num14 = 0.0;
            Parameter parameter13 = ((Element) this.m_DisplayForm.Type).LookupParameter(RevitParameters.RetainingWallHeightParameter);
            if (parameter13 != null)
              num14 = parameter13.AsDouble();
            if (this.m_DisplayForm.Elevation - num14 > 0.0)
            {
              int num15 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("WallCantBePlacedWithDaylightingMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
              return false;
            }
          }
          if (_bSlopeProjection)
          {
            bool _UseRightSide = true;
            if (!RevitUtility.PickSideLeg(this.m_Document, epsLeg1, ref _UseRightSide, EPShared.LocalRM.GetString("PickSideForGradeToPlaceMessage")))
              return false;
            _PlacementSettings.UseRightSide = _UseRightSide;
          }
        }
        else if (this.m_CurrentLandcaddId == LandcaddID.Curb)
        {
          bool _UseRightSide = false;
          if (epsLeg1.IsLegClosed())
          {
            Eps_Leg ppOffsetLegList1 = new Eps_Leg();
            RevitGeometry.OffsetLegList(epsLeg1, num1 + num2, 1, ref ppOffsetLegList1, true);
            Eps_Leg ppOffsetLegList2 = new Eps_Leg();
            RevitGeometry.OffsetLegList(epsLeg1, num1 + num2, 2, ref ppOffsetLegList2, true);
            if (RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2) < RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList1))
              epsLeg1 = epsLeg1.Reverse();
          }
          else if (!RevitUtility.PickSideLeg(this.m_Document, epsLeg1, ref _UseRightSide, EPShared.LocalRM.GetString("PickSideForFaceOfCurbMessage")))
            return false;
          _PlacementSettings.UseRightSide = _UseRightSide;
        }
        if (this.m_CurrentLandcaddId == LandcaddID.Curb)
        {
          SetBaseToposurface setBaseToposurface = new SetBaseToposurface(this.Document, true);
          if (!setBaseToposurface.Run())
            return false;
          ElementSet instanceSet = setBaseToposurface.InstanceSet;
          bool flag4 = false;
          bool flag5 = false;
          TopographySurfaceContainer _ProposedSurface = (TopographySurfaceContainer) null;
          FamilyInstance _TopoPlacementFamilyInstance = (FamilyInstance) null;
          foreach (Element element in instanceSet)
          {
            if (element is TopographySurface _TopographySurface)
              _ProposedSurface = new TopographySurfaceContainer(this.m_Document.Application, _TopographySurface, true);
          }
          if (_ProposedSurface == null || string.Compare(((Element) _ProposedSurface.TopographySurface).UniqueId, ((Element) this.m_ProposedSurface.TopographySurface).UniqueId) == 0)
          {
            _ProposedSurface = this.m_ProposedSurface;
            _TopoPlacementFamilyInstance = this.m_ProposedPlacementFamilyInstance;
            flag4 = true;
          }
          else if (this.m_BaseSurface != null && string.Compare(((Element) _ProposedSurface.TopographySurface).UniqueId, ((Element) this.m_BaseSurface.TopographySurface).UniqueId) == 0)
          {
            _ProposedSurface = this.m_BaseSurface;
            _TopoPlacementFamilyInstance = this.m_BasePlacementFamilyInstance;
            flag5 = true;
          }
          _PlacementSettings.IsRelativeElevation = true;
          _PlacementSettings.Elevation = 0.0;
          if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(this.m_Document, epsLeg1, _PlacementSettings, _dReturnValue, _LineElementList, ref _ProposedSurface, ref _TopoPlacementFamilyInstance, ref this.m_nMaxExistingTopoDrawOrder, _VerifyElementList))
            return false;
          if (flag4)
            this.m_ProposedSurface = _ProposedSurface;
          else if (this.m_BaseSurface != null && flag5)
            this.m_BaseSurface = _ProposedSurface;
        }
        else if (this.m_CurrentLandcaddId == LandcaddID.ParkingLot || this.m_CurrentLandcaddId == LandcaddID.PointWipeout)
        {
          if (this.m_CurrentLandcaddId == LandcaddID.PointWipeout && this.m_PointWipeoutDisplayForm.ShowAffectedPointsChecked)
          {
            List<Eps_Point> allVertices = this.m_ProposedSurface.GetAllVertices();
            RevitGeometry.GetAllPointsInBoundary(ref allVertices, epsLeg1, (List<Eps_Leg>) null);
            if (allVertices.Count > 0)
            {
              this.m_TransactionContainer.BeginTransaction();
              for (int index = 0; index < allVertices.Count; ++index)
              {
                Eps_Line _Line = new Eps_Line();
                _Line.StartPnt = new Eps_Point(allVertices[index].x + 5.0, allVertices[index].y, 0.0);
                _Line.EndPnt = new Eps_Point(allVertices[index].x - 5.0, allVertices[index].y, 0.0);
                ModelCurve modelCurve1 = RevitUtility.NewModelCurve(this.m_Document, _Line);
                if (componentLineSubCategory != null)
                  ((CurveElement) modelCurve1).LineStyle = (Element) componentLineSubCategory.GetGraphicsStyle((GraphicsStyleType) 1);
                elm_set.Insert((Element) modelCurve1);
                _Line.StartPnt = new Eps_Point(allVertices[index].x, allVertices[index].y + 5.0, 0.0);
                _Line.EndPnt = new Eps_Point(allVertices[index].x, allVertices[index].y - 5.0, 0.0);
                ModelCurve modelCurve2 = RevitUtility.NewModelCurve(this.m_Document, _Line);
                if (componentLineSubCategory != null)
                  ((CurveElement) modelCurve2).LineStyle = (Element) componentLineSubCategory.GetGraphicsStyle((GraphicsStyleType) 1);
                elm_set.Insert((Element) modelCurve2);
              }
              this.m_Document.Regenerate();
              this.m_TransactionContainer.EndTransaction();
              if (RevitMessageBox.Show(EPShared.LocalRM.GetString("PointsInBoundaryWillBeRemovedMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                return false;
            }
          }
          _PlacementSettings.IsRelativeElevation = true;
          _PlacementSettings.Elevation = 0.0;
          if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(this.m_Document, epsLeg1, _PlacementSettings, _dReturnValue, _LineElementList, ref this.m_ProposedSurface, ref this.m_ProposedPlacementFamilyInstance, ref this.m_nMaxExistingTopoDrawOrder, _VerifyElementList))
            return false;
        }
        else if (this.m_CurrentLandcaddId == LandcaddID.FeatureLine)
        {
          _PlacementSettings.IsRelativeElevation = this.m_FeatureLineDisplayForm.RelativeElevationChecked;
          _PlacementSettings.Elevation = this.m_FeatureLineDisplayForm.Elevation;
          _PlacementSettings.Grade = this.m_FeatureLineDisplayForm.GetGrade(RevitGeometry.eplGeoE_CalcLegListLength(epsLeg1));
          _PlacementSettings.LinearPointInterval = this.m_FeatureLineDisplayForm.LinearInterval;
          if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(this.m_Document, epsLeg1, _PlacementSettings, _dReturnValue, _LineElementList, ref this.m_ProposedSurface, ref this.m_ProposedPlacementFamilyInstance, ref this.m_nMaxExistingTopoDrawOrder, _VerifyElementList))
            return false;
        }
        else
        {
          _PlacementSettings.IsRelativeElevation = this.m_DisplayForm.RelativeElevationChecked;
          _PlacementSettings.Elevation = this.m_DisplayForm.Elevation;
          if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(this.m_Document, epsLeg1, _PlacementSettings, _dReturnValue, _LineElementList, ref this.m_ProposedSurface, ref this.m_ProposedPlacementFamilyInstance, ref this.m_nMaxExistingTopoDrawOrder, _VerifyElementList))
            return false;
        }
      }
      if (elm_set.Size > 0)
      {
        this.m_TransactionContainer.BeginTransaction();
        this.m_Document.Delete((ICollection<ElementId>) ElementSetConverter.ToIDList(elm_set));
        if (this.m_CurrentLandcaddId == LandcaddID.Curb && this.m_CurbDisplayForm != null && this.m_CurbDisplayForm.CreateCopyChecked || this.m_CurrentLandcaddId == LandcaddID.ParkingLot && this.m_ParkingLotDisplayForm != null && this.m_ParkingLotDisplayForm.CreateCopyChecked || this.m_CurrentLandcaddId == LandcaddID.FeatureLine && this.m_FeatureLineDisplayForm != null && this.m_FeatureLineDisplayForm.CreateCopyChecked || this.m_CurrentLandcaddId == LandcaddID.PointWipeout && this.m_PointWipeoutDisplayForm != null && this.m_PointWipeoutDisplayForm.CreateCopyChecked || this.m_DisplayForm != null && this.m_DisplayForm.CreateCopyChecked)
          RevitUtility.NewModelCurveArray(this.m_Document, epsLeg1, this.m_Document.ActiveView, false);
        this.m_TransactionContainer.EndTransaction();
      }
      return flag3;
    }
    return true;
  }

  private bool PickObjectAndCreateSiteComponent(
    SitePlacementSettings _PlacementSettings,
    Category _MassLineSubCategory,
    double _dOffsetWidth,
    double _dLinearInterval,
    double _dArcPointInterval,
    double _dHeight,
    bool _bSlopeProjection)
  {
    ElementSet _ElementSet = new ElementSet();
    ElementSet elm_set1 = new ElementSet();
    List<Element> _LineElementList = new List<Element>();
    List<Element> _VerifyElementList = new List<Element>();
    ElementSet _Elements = (ElementSet) null;
    int _nReturnValue = 0;
    bool flag1 = true;
    if (!RevitUtility.GetElementsFromProject(this.m_Document, EPShared.LocalRM.GetString("PickObjectMessage"), ref _Elements))
      return false;
    foreach (Element _LocationElement in _Elements)
    {
      if (_LocationElement.Location is LocationCurve location)
      {
        Curve curve = location.Curve;
        if (GeometryObject.op_Inequality((GeometryObject) curve, (GeometryObject) null))
        {
          Eps_Leg _EvalLeg = (Eps_Leg) null;
          if (this.m_CurrentLandcaddId == LandcaddID.Curb)
          {
            if (this.m_CurbDisplayForm.ChainChecked)
              _EvalLeg = RevitGeometry.GetLegFromElementList((Eps_Leg) null, _LocationElement);
          }
          else if (this.m_CurrentLandcaddId == LandcaddID.ParkingLot || this.m_CurrentLandcaddId == LandcaddID.PointWipeout)
            _EvalLeg = RevitGeometry.GetLegFromElementList((Eps_Leg) null, _LocationElement);
          else if (this.m_CurrentLandcaddId == LandcaddID.FeatureLine)
          {
            if (this.m_FeatureLineDisplayForm.ChainChecked)
              _EvalLeg = RevitGeometry.GetLegFromElementList((Eps_Leg) null, _LocationElement);
          }
          else if (this.m_DisplayForm.ChainChecked)
            _EvalLeg = RevitGeometry.GetLegFromElementList((Eps_Leg) null, _LocationElement);
          if (_EvalLeg != null)
          {
            if (this.m_CurrentLandcaddId != LandcaddID.ParkingLot && (this.m_CurrentLandcaddId == LandcaddID.FeatureLine && !this.m_FeatureLineDisplayForm.RelativeElevationChecked && !RevitDefines.EQ(this.m_FeatureLineDisplayForm.GetGrade(RevitGeometry.eplGeoE_CalcLegListLength(_EvalLeg)), 0.0) || ApplicationUtility.Settings.eplSetE_GetSetting("SiteComponentPromptForStartingSide", ref _nReturnValue) && _nReturnValue != 0) && !RevitUtility.EvaluateLeg(this.m_Document, this.m_TransactionContainer, 2.0 * _dOffsetWidth, ref _EvalLeg))
              return false;
            if (_EvalLeg.IsLegClosed())
            {
              if (this.m_CurrentLandcaddId == LandcaddID.SoftTerrain)
                _PlacementSettings.ProjectInnerSlopes = RevitMessageBox.Show(EPShared.LocalRM.GetString("ChainPickedClosedProjectInnerSlopesMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes ? 1 : 0;
            }
            else
            {
              if (this.m_CurrentLandcaddId == LandcaddID.ParkingLot)
              {
                int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("ParkingLotBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
              }
              if (this.m_CurrentLandcaddId == LandcaddID.PointWipeout)
              {
                int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("PointWipeoutBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
              }
            }
            elm_set1 = _EvalLeg.ElementSet;
            foreach (ModelCurve newModelCurve in RevitUtility.NewModelCurveArray(this.m_Document, _EvalLeg, this.m_Document.ActiveView, false))
            {
              if (_MassLineSubCategory != null)
                ((CurveElement) newModelCurve).LineStyle = (Element) _MassLineSubCategory.GetGraphicsStyle((GraphicsStyleType) 1);
              _LineElementList.Add((Element) newModelCurve);
              _ElementSet.Insert((Element) newModelCurve);
            }
          }
          else
          {
            switch (curve)
            {
              case Arc _:
                Arc _Arc = (Arc) curve;
                if (!((Curve) _Arc).IsBound)
                {
                  if (this.m_CurrentLandcaddId == LandcaddID.SoftTerrain)
                    _PlacementSettings.ProjectInnerSlopes = RevitMessageBox.Show(EPShared.LocalRM.GetString("ArcPickedClosedProjectInnerSlopesMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes ? 1 : 0;
                }
                else
                {
                  if (this.m_CurrentLandcaddId == LandcaddID.ParkingLot)
                  {
                    int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("ParkingLotBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                  }
                  if (this.m_CurrentLandcaddId == LandcaddID.PointWipeout)
                  {
                    int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("PointWipeoutBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                  }
                }
                Eps_Arc _EvalArc = new Eps_Arc(_Arc);
                double _dLegListLength = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_EvalArc.Radius, _EvalArc.Delta);
                if ((this.m_CurrentLandcaddId == LandcaddID.FeatureLine && !this.m_FeatureLineDisplayForm.RelativeElevationChecked && !RevitDefines.EQ(this.m_FeatureLineDisplayForm.GetGrade(_dLegListLength), 0.0) || ApplicationUtility.Settings.eplSetE_GetSetting("SiteComponentPromptForStartingSide", ref _nReturnValue) && _nReturnValue != 0) && !RevitUtility.EvaluateArc(this.m_Document, this.m_TransactionContainer, 2.0 * _dOffsetWidth, ref _EvalArc))
                  return false;
                elm_set1.Insert(_LocationElement);
                ModelCurve modelCurve1 = RevitUtility.NewModelCurve(this.m_Document, _EvalArc);
                if (_MassLineSubCategory != null)
                  ((CurveElement) modelCurve1).LineStyle = (Element) _MassLineSubCategory.GetGraphicsStyle((GraphicsStyleType) 1);
                _ElementSet.Insert((Element) modelCurve1);
                _LineElementList.Add((Element) modelCurve1);
                _EvalLeg = new Eps_Leg();
                _EvalLeg.Arc = new Eps_Arc(_EvalArc);
                break;
              case Line _:
                if (this.m_CurrentLandcaddId == LandcaddID.ParkingLot)
                {
                  int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("ParkingLotBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                  return false;
                }
                if (this.m_CurrentLandcaddId == LandcaddID.PointWipeout)
                {
                  int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("PointWipeoutBoundaryClosedChainMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                  return false;
                }
                Eps_Line _EvalLine = new Eps_Line((Line) curve);
                double _dDistance = 0.0;
                EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_EvalLine.EndPnt, _EvalLine.StartPnt, ref _dDistance);
                if ((this.m_CurrentLandcaddId == LandcaddID.FeatureLine && !this.m_FeatureLineDisplayForm.RelativeElevationChecked && !RevitDefines.EQ(this.m_FeatureLineDisplayForm.GetGrade(_dDistance), 0.0) || ApplicationUtility.Settings.eplSetE_GetSetting("SiteComponentPromptForStartingSide", ref _nReturnValue) && _nReturnValue != 0) && !RevitUtility.EvaluateLine(this.m_Document, this.m_TransactionContainer, 2.0 * _dOffsetWidth, ref _EvalLine))
                  return false;
                elm_set1.Insert(_LocationElement);
                ModelCurve modelCurve2 = RevitUtility.NewModelCurve(this.m_Document, _EvalLine);
                if (_MassLineSubCategory != null)
                  ((CurveElement) modelCurve2).LineStyle = (Element) _MassLineSubCategory.GetGraphicsStyle((GraphicsStyleType) 1);
                _ElementSet.Insert((Element) modelCurve2);
                _LineElementList.Add((Element) modelCurve2);
                _EvalLeg = new Eps_Leg();
                _EvalLeg.Line = new Eps_Line(_EvalLine);
                break;
              default:
                int num1 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("ObjectSelectedNotSupportedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
                return this.PickObjectAndCreateSiteComponent(_PlacementSettings, _MassLineSubCategory, _dOffsetWidth, _dLinearInterval, _dArcPointInterval, _dHeight, _bSlopeProjection);
            }
          }
          if (_ElementSet.Size > 0)
            RevitUtility.HideElementsInAllViews(this.m_Document, _ElementSet);
          if (this.m_CurrentLandcaddId == LandcaddID.RetainingWall)
          {
            if (!this.m_DisplayForm.RelativeElevationChecked)
            {
              double _dMinElevation = 0.0;
              double _dMaxElevation = 0.0;
              if (_EvalLeg.FindMinAndMaxElevationOfLeg(this.m_ProposedSurface, _dLinearInterval, 1.0, ref _dMinElevation, ref _dMaxElevation))
              {
                if (RevitDefines.LT(_dMinElevation, this.m_DisplayForm.Elevation - _dHeight))
                {
                  int num2 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("WallCantBePlacedWithDaylightingMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                  return false;
                }
                if (RevitDefines.GT(_dMaxElevation, this.m_DisplayForm.Elevation))
                {
                  int num3 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("WallCantBePlacedWithGreaterElevationMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                  return false;
                }
              }
            }
            if (_bSlopeProjection)
            {
              bool _UseRightSide = true;
              if (!RevitUtility.PickSideLeg(this.m_Document, _EvalLeg, ref _UseRightSide, EPShared.LocalRM.GetString("PickSideForGradeToPlaceMessage")))
                return false;
              _PlacementSettings.UseRightSide = _UseRightSide;
            }
            _PlacementSettings.IsRelativeElevation = this.m_DisplayForm.RelativeElevationChecked;
            _PlacementSettings.Elevation = this.m_DisplayForm.Elevation;
          }
          else if (this.m_CurrentLandcaddId == LandcaddID.Curb)
          {
            bool _UseRightSide = false;
            if (_EvalLeg.IsLegClosed())
            {
              Eps_Leg ppOffsetLegList1 = new Eps_Leg();
              RevitGeometry.OffsetLegList(_EvalLeg, _dOffsetWidth, 1, ref ppOffsetLegList1, true);
              Eps_Leg ppOffsetLegList2 = new Eps_Leg();
              RevitGeometry.OffsetLegList(_EvalLeg, _dOffsetWidth, 2, ref ppOffsetLegList2, true);
              if (RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList2) < RevitGeometry.eplGeoE_CalcLegListLength(ppOffsetLegList1))
                _EvalLeg = _EvalLeg.Reverse();
            }
            else if (!RevitUtility.PickSideLeg(this.m_Document, _EvalLeg, ref _UseRightSide, EPShared.LocalRM.GetString("PickSideForFaceOfCurbMessage")))
              return false;
            _PlacementSettings.UseRightSide = _UseRightSide;
          }
          if (this.m_CurrentLandcaddId != LandcaddID.SoftTerrain && this.m_CurrentLandcaddId != LandcaddID.ParkingLot && this.m_CurrentLandcaddId != LandcaddID.FeatureLine && this.m_CurrentLandcaddId != LandcaddID.PointWipeout)
          {
            for (Eps_Leg epsLeg = _EvalLeg; epsLeg != null; epsLeg = epsLeg.Next)
            {
              if (epsLeg.Arc != null && RevitDefines.LT(epsLeg.Arc.Radius, _dOffsetWidth))
              {
                int num4 = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidRadiusString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
                return false;
              }
            }
          }
          if (this.m_CurrentLandcaddId == LandcaddID.Curb)
          {
            SetBaseToposurface setBaseToposurface = new SetBaseToposurface(this.Document, true);
            if (!setBaseToposurface.Run())
              return false;
            ElementSet instanceSet = setBaseToposurface.InstanceSet;
            bool flag2 = false;
            bool flag3 = false;
            TopographySurfaceContainer _ProposedSurface = (TopographySurfaceContainer) null;
            FamilyInstance _TopoPlacementFamilyInstance = (FamilyInstance) null;
            foreach (Element element in instanceSet)
            {
              if (element is TopographySurface _TopographySurface)
                _ProposedSurface = new TopographySurfaceContainer(this.m_Document.Application, _TopographySurface, true);
            }
            if (_ProposedSurface == null || string.Compare(((Element) _ProposedSurface.TopographySurface).UniqueId, ((Element) this.m_ProposedSurface.TopographySurface).UniqueId) == 0)
            {
              _ProposedSurface = this.m_ProposedSurface;
              _TopoPlacementFamilyInstance = this.m_ProposedPlacementFamilyInstance;
              flag2 = true;
            }
            else if (this.m_BaseSurface != null && string.Compare(((Element) _ProposedSurface.TopographySurface).UniqueId, ((Element) this.m_BaseSurface.TopographySurface).UniqueId) == 0)
            {
              _ProposedSurface = this.m_BaseSurface;
              _TopoPlacementFamilyInstance = this.m_BasePlacementFamilyInstance;
              flag3 = true;
            }
            _PlacementSettings.IsRelativeElevation = true;
            _PlacementSettings.Elevation = 0.0;
            if (this.m_CurbDisplayForm.CreateCopyChecked && elm_set1.Size > 0)
            {
              elm_set1.Clear();
            }
            else
            {
              foreach (Element element in elm_set1)
                _VerifyElementList.Add(element);
            }
            if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(this.m_Document, _EvalLeg, _PlacementSettings, _dArcPointInterval, _LineElementList, ref _ProposedSurface, ref _TopoPlacementFamilyInstance, ref this.m_nMaxExistingTopoDrawOrder, _VerifyElementList))
              return false;
            if (flag2)
              this.m_ProposedSurface = _ProposedSurface;
            else if (this.m_BaseSurface != null && flag3)
              this.m_BaseSurface = _ProposedSurface;
            if (elm_set1.Size > 0)
            {
              this.m_TransactionContainer.BeginTransaction();
              this.m_Document.Delete((ICollection<ElementId>) ElementSetConverter.ToIDList(elm_set1));
              this.m_TransactionContainer.EndTransaction();
            }
          }
          else if (this.m_CurrentLandcaddId == LandcaddID.ParkingLot || this.m_CurrentLandcaddId == LandcaddID.PointWipeout)
          {
            if (this.m_CurrentLandcaddId == LandcaddID.PointWipeout && this.m_PointWipeoutDisplayForm.ShowAffectedPointsChecked)
            {
              List<Eps_Point> allVertices = this.m_ProposedSurface.GetAllVertices();
              RevitGeometry.GetAllPointsInBoundary(ref allVertices, _EvalLeg, (List<Eps_Leg>) null);
              if (allVertices.Count > 0)
              {
                ElementSet elm_set2 = new ElementSet();
                this.m_TransactionContainer.BeginTransaction();
                for (int index = 0; index < allVertices.Count; ++index)
                {
                  Eps_Line _Line = new Eps_Line();
                  _Line.StartPnt = new Eps_Point(allVertices[index].x + 5.0, allVertices[index].y, 0.0);
                  _Line.EndPnt = new Eps_Point(allVertices[index].x - 5.0, allVertices[index].y, 0.0);
                  ModelCurve modelCurve3 = RevitUtility.NewModelCurve(this.m_Document, _Line);
                  if (_MassLineSubCategory != null)
                    ((CurveElement) modelCurve3).LineStyle = (Element) _MassLineSubCategory.GetGraphicsStyle((GraphicsStyleType) 1);
                  elm_set2.Insert((Element) modelCurve3);
                  _Line.StartPnt = new Eps_Point(allVertices[index].x, allVertices[index].y + 5.0, 0.0);
                  _Line.EndPnt = new Eps_Point(allVertices[index].x, allVertices[index].y - 5.0, 0.0);
                  ModelCurve modelCurve4 = RevitUtility.NewModelCurve(this.m_Document, _Line);
                  if (_MassLineSubCategory != null)
                    ((CurveElement) modelCurve4).LineStyle = (Element) _MassLineSubCategory.GetGraphicsStyle((GraphicsStyleType) 1);
                  elm_set2.Insert((Element) modelCurve4);
                }
                this.m_Document.Regenerate();
                this.m_TransactionContainer.EndTransaction();
                if (RevitMessageBox.Show(EPShared.LocalRM.GetString("PointsInBoundaryWillBeRemovedMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                  return false;
                if (elm_set2.Size > 0)
                {
                  this.m_TransactionContainer.BeginTransaction();
                  this.m_Document.Delete((ICollection<ElementId>) ElementSetConverter.ToIDList(elm_set2));
                  this.m_TransactionContainer.EndTransaction();
                }
              }
            }
            _PlacementSettings.IsRelativeElevation = true;
            _PlacementSettings.Elevation = 0.0;
            if ((this.m_CurrentLandcaddId == LandcaddID.ParkingLot && this.m_ParkingLotDisplayForm.CreateCopyChecked || this.m_CurrentLandcaddId == LandcaddID.PointWipeout && this.m_PointWipeoutDisplayForm.CreateCopyChecked) && elm_set1.Size > 0)
            {
              elm_set1.Clear();
            }
            else
            {
              foreach (Element element in elm_set1)
                _VerifyElementList.Add(element);
            }
            if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(this.m_Document, _EvalLeg, _PlacementSettings, _dArcPointInterval, _LineElementList, ref this.m_ProposedSurface, ref this.m_ProposedPlacementFamilyInstance, ref this.m_nMaxExistingTopoDrawOrder, _VerifyElementList))
              return false;
            if (elm_set1.Size > 0)
            {
              this.m_TransactionContainer.BeginTransaction();
              this.m_Document.Delete((ICollection<ElementId>) ElementSetConverter.ToIDList(elm_set1));
              this.m_TransactionContainer.EndTransaction();
            }
          }
          else if (this.m_CurrentLandcaddId == LandcaddID.FeatureLine)
          {
            _PlacementSettings.IsRelativeElevation = this.m_FeatureLineDisplayForm.RelativeElevationChecked;
            _PlacementSettings.Elevation = this.m_FeatureLineDisplayForm.Elevation;
            _PlacementSettings.Grade = this.m_FeatureLineDisplayForm.GetGrade(RevitGeometry.eplGeoE_CalcLegListLength(_EvalLeg));
            _PlacementSettings.LinearPointInterval = this.m_FeatureLineDisplayForm.LinearInterval;
            if (this.m_FeatureLineDisplayForm.CreateCopyChecked && elm_set1.Size > 0)
            {
              elm_set1.Clear();
            }
            else
            {
              foreach (Element element in elm_set1)
                _VerifyElementList.Add(element);
            }
            if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(this.m_Document, _EvalLeg, _PlacementSettings, _dArcPointInterval, _LineElementList, ref this.m_ProposedSurface, ref this.m_ProposedPlacementFamilyInstance, ref this.m_nMaxExistingTopoDrawOrder, _VerifyElementList))
              return false;
            if (elm_set1.Size > 0)
            {
              this.m_TransactionContainer.BeginTransaction();
              this.m_Document.Delete((ICollection<ElementId>) ElementSetConverter.ToIDList(elm_set1));
              this.m_TransactionContainer.EndTransaction();
            }
          }
          else
          {
            _PlacementSettings.IsRelativeElevation = this.m_DisplayForm.RelativeElevationChecked;
            _PlacementSettings.Elevation = this.m_DisplayForm.Elevation;
            if (this.m_DisplayForm.CreateCopyChecked && elm_set1.Size > 0)
            {
              elm_set1.Clear();
            }
            else
            {
              foreach (Element element in elm_set1)
                _VerifyElementList.Add(element);
            }
            if (!TopographySurfaceManagement.CreateTopographySurfaceUpdate(this.m_Document, _EvalLeg, _PlacementSettings, _dArcPointInterval, _LineElementList, ref this.m_ProposedSurface, ref this.m_ProposedPlacementFamilyInstance, ref this.m_nMaxExistingTopoDrawOrder, _VerifyElementList))
              return false;
            if (elm_set1.Size > 0)
            {
              this.m_TransactionContainer.BeginTransaction();
              this.m_Document.Delete((ICollection<ElementId>) ElementSetConverter.ToIDList(elm_set1));
              this.m_TransactionContainer.EndTransaction();
            }
          }
        }
        else
          flag1 = false;
      }
      else
        flag1 = false;
      if (!flag1)
      {
        int num = (int) RevitMessageBox.Show(EPShared.LocalRM.GetString("ObjectSelectedNotSupportedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
        return this.PickObjectAndCreateSiteComponent(_PlacementSettings, _MassLineSubCategory, _dOffsetWidth, _dLinearInterval, _dArcPointInterval, _dHeight, _bSlopeProjection);
      }
    }
    return true;
  }

  public bool FindFamilyName(string _strFindName)
  {
    foreach (Element element in ((IEnumerable<Element>) new FilteredElementCollector(this.m_Document).OfClass(typeof (FamilySymbol))).ToList<Element>())
    {
      if (element is FamilySymbol familySymbol && familySymbol.Family != null && ((Element) familySymbol).Category != null && string.Compare(((Element) familySymbol.Family).Name, _strFindName, true) == 0)
        return true;
    }
    return false;
  }

  public bool FindMassingFamilyStartingNumberName(string _strFindName, ref int nBeginningIndex)
  {
    List<Element> list = ((IEnumerable<Element>) new FilteredElementCollector(this.m_Document).OfClass(typeof (FamilySymbol))).ToList<Element>();
    bool startingNumberName = false;
    nBeginningIndex = 0;
    bool flag = true;
    string empty1 = string.Empty;
    string empty2 = string.Empty;
    foreach (Element element in list)
    {
      if (element is FamilySymbol familySymbol && familySymbol.Family != null && ((Element) familySymbol).Category != null && ((Element) familySymbol.Family).Name.ToUpper().StartsWith(_strFindName.ToUpper()))
      {
        int startIndex = ((Element) familySymbol.Family).Name.LastIndexOf(" - ");
        if (startIndex >= 0 && startIndex < ((Element) familySymbol.Family).Name.Length)
        {
          string str1 = ((Element) familySymbol.Family).Name.Substring(startIndex);
          if (str1.StartsWith(" - ") && str1.Length > " - ".Length)
          {
            string str2 = str1.Substring(" - ".Length);
            for (int index = 0; index < str2.Length; ++index)
            {
              if (!char.IsDigit(str2[index]))
              {
                flag = false;
                break;
              }
            }
            if (flag)
            {
              startingNumberName = true;
              int int32 = Convert.ToInt32(str2);
              if (int32 > nBeginningIndex)
                nBeginningIndex = int32;
            }
            else
              flag = true;
          }
        }
        else if (string.Compare(((Element) familySymbol.Family).Name, _strFindName, true) == 0)
          startingNumberName = true;
      }
    }
    foreach (Element element in (IEnumerable<Element>) new FilteredElementCollector(this.m_Document).OfClass(typeof (DirectShape)).ToElements())
    {
      if (element is DirectShape directShape && ((Element) directShape).Category != null && ((Element) directShape).Name.ToUpper().StartsWith(_strFindName.ToUpper()))
      {
        int startIndex = ((Element) directShape).Name.LastIndexOf(" - ");
        if (startIndex >= 0 && startIndex < ((Element) directShape).Name.Length)
        {
          string str3 = ((Element) directShape).Name.Substring(startIndex);
          if (str3.StartsWith(" - ") && str3.Length > " - ".Length)
          {
            string str4 = str3.Substring(" - ".Length);
            for (int index = 0; index < str4.Length; ++index)
            {
              if (!char.IsDigit(str4[index]))
              {
                flag = false;
                break;
              }
            }
            if (flag)
            {
              startingNumberName = true;
              int int32 = Convert.ToInt32(str4);
              if (int32 > nBeginningIndex)
                nBeginningIndex = int32;
            }
            else
              flag = true;
          }
        }
        else if (string.Compare(((Element) directShape).Name, _strFindName, true) == 0)
          startingNumberName = true;
      }
    }
    ++nBeginningIndex;
    return startingNumberName;
  }

  public bool FindPlacementName(string _strFindName)
  {
    FindFamilyInstance = (FamilyInstance) null;
    Parameter SiteworksIDParameter = (Parameter) null;
    Parameter LocateTypeParameter = (Parameter) null;
    FilteredElementCollector source = new FilteredElementCollector(this.m_Document);
    source.OfClass(typeof (FamilyInstance)).OfCategory((BuiltInCategory) -2001260);
    List<Element> list = ((IEnumerable<Element>) source).Where<Element>((Func<Element, bool>) (CurrentElement => CurrentElement is FamilyInstance FindFamilyInstance && (SiteworksIDParameter = ((Element) FindFamilyInstance.Symbol).LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32((object) LandcaddID.SiteComponentPlacement).ToString("000"), true) == 0 && (LocateTypeParameter = ((Element) FindFamilyInstance).LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter)) != null && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null)).ToList<Element>();
    string empty = string.Empty;
    foreach (Element element in list)
    {
      if (element is FamilyInstance familyInstance && ((Element) familyInstance).Category != null && ((Element) familyInstance).Category.Id.IntegerValue == ((BuiltInCategory) -2001260).GetHashCode())
      {
        SiteworksIDParameter = ((Element) familyInstance.Symbol).LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
        if (SiteworksIDParameter != null)
        {
          LocateTypeParameter = ((Element) familyInstance).LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
          if (LocateTypeParameter != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32((object) LandcaddID.SiteComponentPlacement).ToString("000"), true) == 0)
          {
            Parameter parameter = ((Element) familyInstance).LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
            string strA = string.Empty;
            if (parameter != null)
              strA = parameter.AsString();
            if (strA != string.Empty && string.Compare(strA, _strFindName, true) == 0)
              return true;
          }
        }
      }
    }
    return false;
  }

  public bool FindPlacementStartingNumberName(string _strFindName, ref int nBeginningIndex)
  {
    FindFamilyInstance = (FamilyInstance) null;
    Parameter SiteworksIDParameter = (Parameter) null;
    Parameter LocateTypeParameter = (Parameter) null;
    FilteredElementCollector source = new FilteredElementCollector(this.m_Document);
    source.OfClass(typeof (FamilyInstance)).OfCategory((BuiltInCategory) -2001260);
    List<Element> list = ((IEnumerable<Element>) source).Where<Element>((Func<Element, bool>) (CurrentElement => CurrentElement is FamilyInstance FindFamilyInstance && (SiteworksIDParameter = ((Element) FindFamilyInstance.Symbol).LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32((object) LandcaddID.SiteComponentPlacement).ToString("000"), true) == 0 && (LocateTypeParameter = ((Element) FindFamilyInstance).LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter)) != null && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null)).ToList<Element>();
    bool startingNumberName = false;
    nBeginningIndex = 0;
    bool flag = true;
    string empty1 = string.Empty;
    string empty2 = string.Empty;
    string empty3 = string.Empty;
    foreach (Element element in list)
    {
      if (element is FamilyInstance familyInstance && ((Element) familyInstance).Category != null && ((Element) familyInstance).Category.Id.IntegerValue == ((BuiltInCategory) -2001260).GetHashCode())
      {
        SiteworksIDParameter = ((Element) familyInstance.Symbol).LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
        if (SiteworksIDParameter != null)
        {
          LocateTypeParameter = ((Element) familyInstance).LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
          if (LocateTypeParameter != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32((object) LandcaddID.SiteComponentPlacement).ToString("000"), true) == 0)
          {
            Parameter parameter = ((Element) familyInstance).LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
            string strA = string.Empty;
            if (parameter != null)
              strA = parameter.AsString();
            if (strA != string.Empty && strA.ToUpper().StartsWith(_strFindName.ToUpper()))
            {
              int startIndex = strA.LastIndexOf(" - ");
              if (startIndex >= 0 && startIndex < strA.Length)
              {
                string str1 = strA.Substring(startIndex);
                if (str1.StartsWith(" - ") && str1.Length > " - ".Length)
                {
                  string str2 = str1.Substring(" - ".Length);
                  for (int index = 0; index < str2.Length; ++index)
                  {
                    if (!char.IsDigit(str2[index]))
                    {
                      flag = false;
                      break;
                    }
                  }
                  if (flag)
                  {
                    startingNumberName = true;
                    int int32 = Convert.ToInt32(str2);
                    if (int32 > nBeginningIndex)
                      nBeginningIndex = int32;
                  }
                  else
                    flag = true;
                }
              }
              else if (string.Compare(strA, _strFindName, true) == 0)
                startingNumberName = true;
            }
          }
        }
      }
    }
    ++nBeginningIndex;
    return startingNumberName;
  }

  public string GetSitePlacementNameWithoutSeperator(string _strValue)
  {
    int num = _strValue.LastIndexOf(" - ");
    if (num >= 0 && num < _strValue.Length)
    {
      string str = _strValue.Substring(num);
      if (str.StartsWith(" - ") && str.Length > " - ".Length)
      {
        bool flag = true;
        foreach (char c in str.Substring(" - ".Length))
        {
          if (!char.IsDigit(c))
          {
            flag = false;
            break;
          }
        }
        if (flag)
          _strValue = _strValue.Substring(0, num);
      }
    }
    return _strValue;
  }

  private bool CheckForDuplicateSymbols(string _strNewFamilyName)
  {
    FamilySymbol familySymbol1 = (FamilySymbol) null;
    FilteredElementCollector source = new FilteredElementCollector(this.m_Document);
    source.OfClass(typeof (FamilySymbol));
    List<Element> list = ((IEnumerable<Element>) source).ToList<Element>();
    familySymbol1 = (FamilySymbol) null;
    foreach (Element element in list)
    {
      if (element is FamilySymbol familySymbol2 && ((Element) familySymbol2).Category != null && string.Compare(((Element) familySymbol2.Family).Name, _strNewFamilyName, true) == 0)
        return true;
    }
    return false;
  }

  public Document Document => this.m_Document;

  public ReadOnlyCollection<FamilyContainer> FamilyList
  {
    get => new ReadOnlyCollection<FamilyContainer>((IList<FamilyContainer>) this.m_FamilyList);
  }

  public EPTransactionContainer TransactionContainer
  {
    get => this.m_TransactionContainer;
    set => this.m_TransactionContainer = value;
  }

  public string NUMMERIC_SEPERATOR => " - ";
}
