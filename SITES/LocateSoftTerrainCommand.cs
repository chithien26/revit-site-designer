using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EPCommon;

namespace SITES;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
[Journaling(JournalingMode.NoCommandData)]
public class LocateSoftTerrainCommand : IExternalCommand
{
	public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
	{
		return LocateSiteComponentCommand.Execute(commandData, ref message, LandcaddID.SoftTerrain);
	}
}
