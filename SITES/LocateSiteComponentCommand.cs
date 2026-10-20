using System;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EPCommon;

namespace SITES;

internal static class LocateSiteComponentCommand
{
	public static Result Execute(ExternalCommandData commandData, ref string message, LandcaddID landcaddId)
	{
		Result result = Result.Succeeded;
		try
		{
			RevitCommandRuntime.Begin(commandData.Application, "Locate" + landcaddId + "Command");
			ApplicationUtility.VerifyProductVersion();
			Document document = RevitCommandRuntime.RequireDocument(commandData.Application, "Locate" + landcaddId + "Command");
			if (document == null) return Result.Cancelled;
			if (document.ActiveView is ViewSection)
			{
				RevitMessageBox.Show(ApplicationUtility.GetString("InvalidViewForImportString"), ApplicationUtility.GetString("Error"), MessageBoxIcon.Hand);
			}
			else if (!document.IsFamilyDocument)
			{
				if (!TopographySurfaceManagement.RunLocate(document, landcaddId))
				{
					return Result.Cancelled;
				}
			}
			else
			{
				RevitMessageBox.Show(ApplicationUtility.GetString("NoProjectMessage"), ApplicationUtility.GetString("Error"), MessageBoxIcon.Hand);
			}
		}
		catch (Autodesk.Revit.Exceptions.OperationCanceledException)
		{
			return Result.Cancelled;
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Report("Locate" + landcaddId + "Command", ex);
			message = ex.Message;
			result = Result.Failed;
		}
		return result;
	}
}
