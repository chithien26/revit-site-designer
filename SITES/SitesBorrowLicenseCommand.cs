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
public class SitesBorrowLicenseCommand : IExternalCommand
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
				if (EPShared.eplLicM_BorrowNetworkLicense())
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("LicenseBorrowedFromNetworkMessage"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
				}
				else if (EPShared.eplLicM_IsBorrowedNetworkLicense())
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("LicenseBorrowedFromNetworkMessageError"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
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
