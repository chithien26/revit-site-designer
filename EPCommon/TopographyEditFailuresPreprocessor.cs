using Autodesk.Revit.DB;

namespace EPCommon;

public class TopographyEditFailuresPreprocessor : IFailuresPreprocessor
{
	public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
	{
		TerrainFailures.DeleteExpectedWarnings(failuresAccessor);
		return FailureProcessingResult.Continue;
	}
}
