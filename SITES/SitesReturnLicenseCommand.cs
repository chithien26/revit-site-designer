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
public class SitesReturnLicenseCommand : IExternalCommand
{
	public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
	{
		Result result = Result.Succeeded;
		try
		{
			if (!RevitCommandRuntime.Begin(commandData.Application, GetType().Name))
				return Result.Cancelled;
			if (!EPShared.eplLicM_IsNodeLicense())
			{
				if (EPShared.eplLicM_ReturnBorrowedLicense())
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("LicenseReturnedToNetworkMessage"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
				}
				else
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("SiteworksNetworkLicenseBorrowError"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
				}
			}
			else
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("SiteworksNetworkLicenseError"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
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
