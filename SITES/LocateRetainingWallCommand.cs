using System;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EPCommon;

namespace SITES;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
[Journaling(JournalingMode.NoCommandData)]
public class LocateRetainingWallCommand : IExternalCommand
{
	public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
	{
		Result result = Result.Succeeded;
		try
		{
			RevitCommandRuntime.Begin(commandData.Application, GetType().Name);
			if (!EPShared.eplLicM_GetLicense())
			{
				throw new System.InvalidOperationException("The original Site Designer license check did not grant a license.");
			}
			{
				ApplicationUtility.VerifyProductVersion();
				Document document = RevitCommandRuntime.RequireDocument(commandData.Application, GetType().Name);
				if (document == null) return Result.Cancelled;
				if (document.ActiveView is ViewSection)
				{
					RevitMessageBox.Show(ApplicationUtility.GetString("InvalidViewForImportString"), ApplicationUtility.GetString("Error"), MessageBoxIcon.Hand);
				}
				else if (!document.IsFamilyDocument)
				{
					TopographySurfaceContainer _BaseSurfaceContainer = null;
					FamilyInstance _BasePlacementFamilyInstance = null;
					TopographySurfaceContainer _ProposedSurfaceContainer = null;
					FamilyInstance _ProposedPlacementFamilyInstance = null;
					TopographySurfaceManagement.FindAndCreateProposedLandcaddTopographySurface(document, ref _BaseSurfaceContainer, ref _BasePlacementFamilyInstance, ref _ProposedSurfaceContainer, ref _ProposedPlacementFamilyInstance);
					if (_ProposedSurfaceContainer != null && _ProposedPlacementFamilyInstance != null)
					{
						SiteComponentCreation siteComponentCreation = new SiteComponentCreation(document, _BaseSurfaceContainer, _BasePlacementFamilyInstance, _ProposedSurfaceContainer, _ProposedPlacementFamilyInstance, LandcaddID.RetainingWall);
						siteComponentCreation.Run();
					}
					else
					{
						SetBaseToposurface setBaseToposurface = new SetBaseToposurface(document);
						if (setBaseToposurface.Run())
						{
							TopographySurfaceManagement.FindBaseLandcaddTopographySurface(document, _bIsProposedSurface: true, ref _ProposedSurfaceContainer, ref _ProposedPlacementFamilyInstance);
							if (_ProposedSurfaceContainer != null && _ProposedPlacementFamilyInstance != null)
							{
								SiteComponentCreation siteComponentCreation2 = new SiteComponentCreation(document, _BaseSurfaceContainer, _BasePlacementFamilyInstance, _ProposedSurfaceContainer, _ProposedPlacementFamilyInstance, LandcaddID.RetainingWall);
								siteComponentCreation2.Run();
							}
						}
					}
				}
				else
				{
					RevitMessageBox.Show(ApplicationUtility.GetString("NoProjectMessage"), ApplicationUtility.GetString("Error"), MessageBoxIcon.Hand);
				}
			}
		}
		catch (Autodesk.Revit.Exceptions.OperationCanceledException)
		{
			return Result.Cancelled;
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Report(GetType().Name, ex);
			message = ex.Message;
			result = Result.Failed;
		}
		return result;
	}
}
