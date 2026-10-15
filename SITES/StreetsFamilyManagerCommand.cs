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
public class StreetsFamilyManagerCommand : IExternalCommand
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
				FamilyManagerForm familyManagerForm = new FamilyManagerForm(document, LandcaddID.Street);
				familyManagerForm.ShowDialog();
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
