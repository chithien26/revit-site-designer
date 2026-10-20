using System;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EPCommon;

namespace SITES;

[Transaction(TransactionMode.Manual)]
[Journaling(JournalingMode.NoCommandData)]
public class AnnotateCommand : IExternalCommand
{
	public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
	{
		Result result = Result.Succeeded;
		try
		{
			RevitCommandRuntime.Begin(commandData.Application, GetType().Name);
			{
				ApplicationUtility.VerifyProductVersion();
				Document document = RevitCommandRuntime.RequireDocument(commandData.Application, GetType().Name);
				if (document == null) return Result.Cancelled;
				if (document.ActiveView is ViewSection)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidViewForImportString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				}
				else if (!document.IsFamilyDocument)
				{
					Annotation annotation = new Annotation(document);
					annotation.Run();
				}
				else
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NoProjectMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
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
