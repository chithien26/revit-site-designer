// Decompiled with JetBrains decompiler
// Type: SITES.SoftTerrainFamilyManagerCommand
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
public class SoftTerrainFamilyManagerCommand : IExternalCommand
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
        int num = (int) new FamilyManagerForm(commandData.Application.ActiveUIDocument.Document, LandcaddID.SoftTerrain).ShowDialog();
      }
    }
    catch (Exception ex)
    {
      int num = (int) RevitMessageBox.Show(ex.Message, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
    }
    return result;
  }
}
