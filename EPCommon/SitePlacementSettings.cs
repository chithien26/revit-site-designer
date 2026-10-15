using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class SitePlacementSettings
{
	private LandcaddID m_LocateType;

	private Level m_Level;

	private FamilySymbol m_Type;

	private double m_dElevation;

	private double m_dGrade;

	private List<double> m_BeginningElevationList;

	private List<double> m_EndingElevationList;

	private double m_dLinearPointInterval;

	private FamilySymbol m_PlacementSymbol;

	private bool m_bUseRightSide;

	private bool m_bIsReverse;

	private bool m_bIsRelativeElevation;

	private string m_strName;

	private int m_nTopoDrawOrder;

	private string m_strLoftFormID;

	private string m_strAdditionalLoftFormID;

	private string m_strIslandSurfaceId;

	private string m_strParkingLotSurfaceID;

	private string m_strParentTopoSurfaceID;

	private FamilySymbol m_IntersectionPlacementSymbol;

	private List<IntersectionStationContainer> m_IntersectionStations;

	private int m_nProjectInnerSlopes;

	private EPTransactionContainer m_TransactionContainer;

	public LandcaddID LocateType
	{
		get
		{
			return m_LocateType;
		}
		set
		{
			m_LocateType = value;
		}
	}

	public Level Level
	{
		get
		{
			return m_Level;
		}
		set
		{
			m_Level = value;
		}
	}

	public FamilySymbol Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public double Elevation
	{
		get
		{
			return m_dElevation;
		}
		set
		{
			m_dElevation = value;
		}
	}

	public double Grade
	{
		get
		{
			return m_dGrade;
		}
		set
		{
			m_dGrade = value;
		}
	}

	public List<double> BeginningElevationList
	{
		get
		{
			return m_BeginningElevationList;
		}
		set
		{
			m_BeginningElevationList = value;
		}
	}

	public List<double> EndingElevationList
	{
		get
		{
			return m_EndingElevationList;
		}
		set
		{
			m_EndingElevationList = value;
		}
	}

	public double LinearPointInterval
	{
		get
		{
			return m_dLinearPointInterval;
		}
		set
		{
			m_dLinearPointInterval = value;
		}
	}

	public bool IsRelativeElevation
	{
		get
		{
			return m_bIsRelativeElevation;
		}
		set
		{
			m_bIsRelativeElevation = value;
		}
	}

	public FamilySymbol PlacementSymbol
	{
		get
		{
			return m_PlacementSymbol;
		}
		set
		{
			m_PlacementSymbol = value;
		}
	}

	public FamilySymbol IntersectionPlacementSymbol
	{
		get
		{
			return m_IntersectionPlacementSymbol;
		}
		set
		{
			m_IntersectionPlacementSymbol = value;
		}
	}

	public bool UseRightSide
	{
		get
		{
			return m_bUseRightSide;
		}
		set
		{
			m_bUseRightSide = value;
		}
	}

	public bool IsReverse
	{
		get
		{
			return m_bIsReverse;
		}
		set
		{
			m_bIsReverse = value;
		}
	}

	public string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			m_strName = value;
		}
	}

	public EPTransactionContainer TransactionContainer
	{
		get
		{
			return m_TransactionContainer;
		}
		set
		{
			m_TransactionContainer = value;
		}
	}

	public int ProjectInnerSlopes
	{
		get
		{
			return m_nProjectInnerSlopes;
		}
		set
		{
			m_nProjectInnerSlopes = value;
		}
	}

	public int TopoDrawOrder
	{
		get
		{
			return m_nTopoDrawOrder;
		}
		set
		{
			m_nTopoDrawOrder = value;
		}
	}

	public string LoftFormID
	{
		get
		{
			return m_strLoftFormID;
		}
		set
		{
			m_strLoftFormID = value;
		}
	}

	public string AdditionalLoftFormID
	{
		get
		{
			return m_strAdditionalLoftFormID;
		}
		set
		{
			m_strAdditionalLoftFormID = value;
		}
	}

	public string IslandSurfaceID
	{
		get
		{
			return m_strIslandSurfaceId;
		}
		set
		{
			m_strIslandSurfaceId = value;
		}
	}

	public string ParkingLotSurfaceID
	{
		get
		{
			return m_strParkingLotSurfaceID;
		}
		set
		{
			m_strParkingLotSurfaceID = value;
		}
	}

	public string ParentTopoSurfaceID
	{
		get
		{
			return m_strParentTopoSurfaceID;
		}
		set
		{
			m_strParentTopoSurfaceID = value;
		}
	}

	public List<IntersectionStationContainer> IntersectionStations
	{
		get
		{
			return m_IntersectionStations;
		}
		set
		{
			m_IntersectionStations = value;
		}
	}

	public SitePlacementSettings()
	{
		m_LocateType = LandcaddID.Street;
		m_Level = null;
		m_Type = null;
		m_dElevation = 0.0;
		m_dGrade = 0.0;
		m_BeginningElevationList = new List<double>();
		m_EndingElevationList = new List<double>();
		m_dLinearPointInterval = 0.0;
		m_PlacementSymbol = null;
		m_bUseRightSide = false;
		m_bIsReverse = false;
		m_bIsRelativeElevation = false;
		m_nProjectInnerSlopes = 0;
		m_TransactionContainer = null;
		m_nTopoDrawOrder = -1;
		m_strLoftFormID = string.Empty;
		AdditionalLoftFormID = string.Empty;
		m_strIslandSurfaceId = string.Empty;
		m_strParkingLotSurfaceID = string.Empty;
		m_strParentTopoSurfaceID = string.Empty;
		m_IntersectionPlacementSymbol = null;
		m_IntersectionStations = null;
	}

	public SitePlacementSettings(LandcaddID _LocateType, Level _Level, FamilySymbol _Type, FamilySymbol _PlacementSymbol, string _strName, int _nTopoDrawOrder, EPTransactionContainer _TransactionContainer)
	{
		m_LocateType = _LocateType;
		m_Level = _Level;
		m_Type = _Type;
		m_dElevation = 0.0;
		m_dGrade = 0.0;
		m_BeginningElevationList = new List<double>();
		m_EndingElevationList = new List<double>();
		m_dLinearPointInterval = 0.0;
		m_PlacementSymbol = _PlacementSymbol;
		m_bUseRightSide = false;
		m_bIsReverse = false;
		m_bIsRelativeElevation = false;
		m_strName = _strName;
		m_nTopoDrawOrder = _nTopoDrawOrder;
		m_TransactionContainer = _TransactionContainer;
		m_strLoftFormID = string.Empty;
		AdditionalLoftFormID = string.Empty;
		m_strIslandSurfaceId = string.Empty;
		m_strParkingLotSurfaceID = string.Empty;
		m_strParentTopoSurfaceID = string.Empty;
		m_IntersectionPlacementSymbol = null;
		m_IntersectionStations = null;
		if (m_Type != null && !m_Type.IsActive)
		{
			m_Type.Activate();
		}
		if (m_PlacementSymbol != null && !m_PlacementSymbol.IsActive)
		{
			m_PlacementSymbol.Activate();
		}
	}
}
