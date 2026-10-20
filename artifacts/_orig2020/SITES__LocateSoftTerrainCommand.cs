// Decompiled with JetBrains decompiler
// Type: SITES.LocateSoftTerrainCommand
// Assembly: SiteDesigner, Version=20.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 84104107-DD17-4742-AB85-FA77E5583D5F
// Assembly location: C:\Program Files\Autodesk\Revit Site Designer Extension 2020\SiteDesigner.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EPCommon;
using System;
using System.Windows.Forms;

#nullable disable
namespace SITES;

[Transaction]
[Regeneration]
[Journaling]
public class LocateSoftTerrainCommand : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    if (!ApplicationUtility.IsRevitVersionValid)
      return (Result) -1;
    Result result = (Result) 0;
    try
    {
      if (EPShared.eplLicM_GetLicense())
      {
        ApplicationUtility.VerifyProductVersion();
        Document document = commandData.Application.ActiveUIDocument.Document;
        if (document.ActiveView is ViewSection)
        {
          int num1 = (int) RevitMessageBox.Show(ApplicationUtility.GetString("InvalidViewForImportString"), ApplicationUtility.GetString("Error"), MessageBoxIcon.Hand);
        }
        else if (!document.IsFamilyDocument)
        {
          TopographySurfaceContainer _BaseSurfaceContainer = (TopographySurfaceContainer) null;
          FamilyInstance _BasePlacementFamilyInstance = (FamilyInstance) null;
          TopographySurfaceContainer _ProposedSurface = (TopographySurfaceContainer) null;
          FamilyInstance _ProposedPlacementFamilyInstance = (FamilyInstance) null;
          TopographySurfaceManagement.FindAndCreateProposedLandcaddTopographySurface(document, ref _BaseSurfaceContainer, ref _BasePlacementFamilyInstance, ref _ProposedSurface, ref _ProposedPlacementFamilyInstance);
          if (_ProposedSurface != null && _ProposedPlacementFamilyInstance != null)
            new SiteComponentCreation(document, _BaseSurfaceContainer, _BasePlacementFamilyInstance, _ProposedSurface, _ProposedPlacementFamilyInstance, LandcaddID.SoftTerrain).Run();
          else if (new SetBaseToposurface(document).Run())
          {
            TopographySurfaceManagement.FindBaseLandcaddTopographySurface(document, true, ref _ProposedSurface, ref _ProposedPlacementFamilyInstance);
            if (_ProposedSurface != null)
            {
              if (_ProposedPlacementFamilyInstance != null)
                new SiteComponentCreation(document, _BaseSurfaceContainer, _BasePlacementFamilyInstance, _ProposedSurface, _ProposedPlacementFamilyInstance, LandcaddID.SoftTerrain).Run();
            }
          }
        }
        else
        {
          int num2 = (int) RevitMessageBox.Show(ApplicationUtility.GetString("NoProjectMessage"), ApplicationUtility.GetString("Error"), MessageBoxIcon.Hand);
        }
      }
    }
    catch (Exception ex)
    {
      int num = (int) RevitMessageBox.Show(ex.Message, ApplicationUtility.GetString("Error"), MessageBoxIcon.Hand);
    }
    return result;
  }
}
