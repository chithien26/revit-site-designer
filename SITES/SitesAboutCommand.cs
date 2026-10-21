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
public class SitesAboutCommand : IExternalCommand
{
	public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
	{
		Result result = Result.Succeeded;
		try
		{
			if (!RevitCommandRuntime.Begin(commandData.Application, GetType().Name))
				return Result.Cancelled;
			AboutForm aboutForm = new AboutForm();
			aboutForm.ShowDialog();
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
