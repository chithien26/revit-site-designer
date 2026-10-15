using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace EPCommon;

internal class TopographyEditFailuresPreprocessorVerbose : IFailuresPreprocessor
{
	public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
	{
		try
		{
			TaskDialog.Show("Preprocess failures", "Hello");
			IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
			int count = failureMessages.Count;
			TaskDialog.Show("Preprocess failures", "Found " + count + " failure messages.");
			if (count < 5)
			{
				foreach (FailureMessageAccessor item in failureMessages)
				{
					TaskDialog.Show("Failure!", item.GetDescriptionText());
				}
			}
			else
			{
				TaskDialog.Show("Failure 1 of " + count, failureMessages.First().GetDescriptionText());
			}
			TaskDialog.Show("Preprocess failures", "Goodbye");
			return FailureProcessingResult.Continue;
		}
		catch (Exception ex)
		{
			TaskDialog.Show("Exception", ex.ToString());
			return FailureProcessingResult.ProceedWithRollBack;
		}
	}
}
