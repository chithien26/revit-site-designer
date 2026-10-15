using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SITES.Sites_Core;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
public class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("SITES.Sites_Core.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	public static Bitmap About
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "About", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap Annotate
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "Annotate", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap AssignPatioWalk
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "AssignPatioWalk", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap AssignPlantMix
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "AssignPlantMix", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap BorrowLicense
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "BorrowLicense", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ConvertPlants
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ConvertPlants", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap CurbFamilyManager
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "CurbFamilyManager", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap CurbInfo
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "CurbInfo", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ExportLandXML
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ExportLandXML", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap GrowthSimulation
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "GrowthSimulation", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ImportLandXML
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ImportLandXML", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Icon lc4revit
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "lc4revit", resourceCulture);
			return (Icon)obj;
		}
	}

	public static Bitmap LocateComponents
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "LocateComponents", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap LocateCurb
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "LocateCurb", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap LocateFeatureLine
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "LocateFeatureLine", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap LocateFence
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "LocateFence", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap LocateParkingLot
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "LocateParkingLot", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap LocatePlants
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "LocatePlants", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap LocatePointWipeout
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "LocatePointWipeout", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap LocateRetainingWall
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "LocateRetainingWall", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap LocateSidewalk
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "LocateSidewalk", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap LocateSoftTerrain
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "LocateSoftTerrain", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap LocateStreet
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "LocateStreet", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ManageLicense
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ManageLicense", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ModifyCurb
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ModifyCurb", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ModifyFeatureLine
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ModifyFeatureLine", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ModifyParkingLot
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ModifyParkingLot", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ModifyPlants
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ModifyPlants", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ModifyPointWipeout
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ModifyPointWipeout", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ModifyRetainingWall
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ModifyRetainingWall", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ModifySidewalk
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ModifySidewalk", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ModifySoftTerrain
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ModifySoftTerrain", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ModifyStreet
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ModifyStreet", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ParkingLot
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ParkingLot", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ParkingLotFamilyManager
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ParkingLotFamilyManager", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ParkingLotInfo
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ParkingLotInfo", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap PatioWalkManager
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "PatioWalkManager", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap PatioWalkReport
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "PatioWalkReport", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap PickElevation
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "PickElevation", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap PlantBooklet
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "PlantBooklet", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap PlantFamilyManager
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "PlantFamilyManager", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap PlantMixManager
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "PlantMixManager", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap PlantMixReport
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "PlantMixReport", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap Report
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "Report", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap RetainingWallFamilyManager
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "RetainingWallFamilyManager", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap RetainingWallInfo
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "RetainingWallInfo", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ReturnLicense
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ReturnLicense", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap SetBaseToposurface
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "SetBaseToposurface", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap Settings
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "Settings", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap SidewalkFamilyManager
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "SidewalkFamilyManager", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap SidewalkInfo
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "SidewalkInfo", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap SitesReport
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "SitesReport", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Icon Siteworks
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "Siteworks", resourceCulture);
			return (Icon)obj;
		}
	}

	public static Bitmap SoftTerrainInfo
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "SoftTerrainInfo", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap SoftTerrainManager
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "SoftTerrainManager", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap StreetFamilyManager
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "StreetFamilyManager", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap StreetInfo
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "StreetInfo", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static string String1 => ResourceManager.GetString("String1", resourceCulture);

	public static Bitmap tooltip_curb
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "tooltip_curb", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap tooltip_feature_line
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "tooltip_feature_line", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap tooltip_parking_lot
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "tooltip_parking_lot", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap tooltip_point_wipeout
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "tooltip_point_wipeout", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap tooltip_retaining_wall
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "tooltip_retaining_wall", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap tooltip_sidewalk
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "tooltip_sidewalk", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap tooltip_soft_terrain
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "tooltip_soft_terrain", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap tooltip_street
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "tooltip_street", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ToposurfaceConversion
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "ToposurfaceConversion", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap UserLog
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "UserLog", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap UserLog1
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "UserLog1", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
