using Autodesk.Revit.DB;

namespace EPCommon;

public class TopographyEditFailuresPreprocessor : IFailuresPreprocessor
{
	public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
	{
		return FailureProcessingResult.Continue;
	}
}
