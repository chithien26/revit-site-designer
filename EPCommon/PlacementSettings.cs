using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class PlacementSettings
{
	private LocateType m_LocateType;

	private PlacementType m_PlacementType;

	private bool m_bDeleteExistingChecked;

	private bool m_bExistingElementChecked;

	private bool m_bPlaceGroupTagChecked;

	private bool m_bLeaderedTagChecked;

	private Level m_Level;

	private View m_View;

	private FamilySymbol m_Type;

	private List<TopographySurfaceContainer> m_SurfaceList;

	private double m_dSetbackSpacing;

	private double m_dHeight;

	private double m_dHeightVariance;

	private double m_dWidth;

	private double m_dWidthVariance;

	private double m_dPlantSpacing;

	private double m_dStartingDistance;

	private double m_dRotationAngle;

	private FamilySymbol m_PlacementSymbol;

	private List<KeyValuePair<string, string>> m_GenericModelIDList;

	private bool m_bUseRightSide;

	private bool m_bIsReverse;

	private EPTransactionContainer m_TransactionContainer;

	private bool m_bAlignTo;

	private ParkingPlacementSettings m_ParkingPlacementSettings;

	private string m_strFirstLegSegmentID;

	private string m_strFirstPlacementSegmentID;

	private string m_strLastLegSegmentID;

	public ParkingPlacementSettings Parking
	{
		get
		{
			if (m_ParkingPlacementSettings == null)
			{
				m_ParkingPlacementSettings = new ParkingPlacementSettings();
			}
			return m_ParkingPlacementSettings;
		}
	}

	public LocateType LocateType
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

	public PlacementType PlacementType
	{
		get
		{
			return m_PlacementType;
		}
		set
		{
			m_PlacementType = value;
		}
	}

	public bool DeleteExistingChecked
	{
		get
		{
			return m_bDeleteExistingChecked;
		}
		set
		{
			m_bDeleteExistingChecked = value;
		}
	}

	public bool ExistingElementChecked
	{
		get
		{
			return m_bExistingElementChecked;
		}
		set
		{
			m_bExistingElementChecked = value;
		}
	}

	public bool PlaceGroupTagChecked
	{
		get
		{
			return m_bPlaceGroupTagChecked;
		}
		set
		{
			m_bPlaceGroupTagChecked = value;
		}
	}

	public bool LeaderedTagChecked
	{
		get
		{
			return m_bLeaderedTagChecked;
		}
		set
		{
			m_bLeaderedTagChecked = value;
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

	public View View
	{
		get
		{
			return m_View;
		}
		set
		{
			m_View = value;
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

	public List<TopographySurfaceContainer> SurfaceList
	{
		get
		{
			return m_SurfaceList;
		}
		set
		{
			m_SurfaceList = value;
		}
	}

	public double SetbackSpacing
	{
		get
		{
			return m_dSetbackSpacing;
		}
		set
		{
			m_dSetbackSpacing = value;
		}
	}

	public double Height
	{
		get
		{
			return m_dHeight;
		}
		set
		{
			m_dHeight = value;
		}
	}

	public double HeightVariance
	{
		get
		{
			return m_dHeightVariance;
		}
		set
		{
			m_dHeightVariance = value;
		}
	}

	public double Width
	{
		get
		{
			return m_dWidth;
		}
		set
		{
			m_dWidth = value;
		}
	}

	public double WidthVariance
	{
		get
		{
			return m_dWidthVariance;
		}
		set
		{
			m_dWidthVariance = value;
		}
	}

	public double PlantSpacing
	{
		get
		{
			return m_dPlantSpacing;
		}
		set
		{
			m_dPlantSpacing = value;
		}
	}

	public double RotationAngle
	{
		get
		{
			return m_dRotationAngle;
		}
		set
		{
			m_dRotationAngle = value;
		}
	}

	public double StartingDistance
	{
		get
		{
			return m_dStartingDistance;
		}
		set
		{
			m_dStartingDistance = value;
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

	public List<KeyValuePair<string, string>> GenericModelIDList
	{
		get
		{
			return m_GenericModelIDList;
		}
		set
		{
			m_GenericModelIDList = value;
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

	public bool AlignTo
	{
		get
		{
			return m_bAlignTo;
		}
		set
		{
			m_bAlignTo = value;
		}
	}

	public string FirstLegSegmentID
	{
		get
		{
			return m_strFirstLegSegmentID;
		}
		set
		{
			m_strFirstLegSegmentID = value;
		}
	}

	public string LastLegSegmentID
	{
		get
		{
			return m_strLastLegSegmentID;
		}
		set
		{
			m_strLastLegSegmentID = value;
		}
	}

	public string FirstPlacementSegmentID
	{
		get
		{
			return m_strFirstPlacementSegmentID;
		}
		set
		{
			m_strFirstPlacementSegmentID = value;
		}
	}

	public PlacementSettings()
	{
		m_LocateType = LocateType.Plant;
		m_PlacementType = PlacementType.HedgeRowCenter;
		m_bDeleteExistingChecked = false;
		m_bExistingElementChecked = true;
		m_bPlaceGroupTagChecked = false;
		m_Level = null;
		m_View = null;
		m_Type = null;
		m_SurfaceList = null;
		m_dSetbackSpacing = 0.0;
		m_dHeight = 0.0;
		m_dHeightVariance = 0.0;
		m_dWidth = 0.0;
		m_dWidthVariance = 0.0;
		m_dPlantSpacing = 0.0;
		m_dRotationAngle = 0.0;
		m_dStartingDistance = 0.0;
		m_PlacementSymbol = null;
		m_GenericModelIDList = null;
		m_bUseRightSide = false;
		m_bIsReverse = false;
		m_TransactionContainer = null;
		m_bAlignTo = false;
		m_ParkingPlacementSettings = null;
		m_strFirstLegSegmentID = string.Empty;
		m_strFirstPlacementSegmentID = string.Empty;
	}

	public PlacementSettings(LocateType _LocateType, PlacementType _PlantPlacementType, bool _bDeleteExistingChecked, bool _bExistingElementChecked, bool _bPlaceGroupTagChecked, Level _Level, View _View, FamilySymbol _Type, List<TopographySurfaceContainer> _SurfaceList, double _dSetbackSpacing, double _dHeight, double _dHeightVariance, double _dWidth, double _dWidthVariance, double _dPlantSpacing, double _dRotationAngle, double _dStartingDistance, FamilySymbol _PlacementSymbol, EPTransactionContainer _TransactionContainer)
	{
		m_LocateType = _LocateType;
		m_PlacementType = _PlantPlacementType;
		m_bDeleteExistingChecked = _bDeleteExistingChecked;
		m_bExistingElementChecked = _bExistingElementChecked;
		m_bPlaceGroupTagChecked = _bPlaceGroupTagChecked;
		m_Level = _Level;
		m_View = _View;
		m_Type = _Type;
		m_SurfaceList = _SurfaceList;
		m_dSetbackSpacing = _dSetbackSpacing;
		m_dHeight = _dHeight;
		m_dHeightVariance = _dHeightVariance;
		m_dWidth = _dWidth;
		m_dWidthVariance = _dWidthVariance;
		m_dPlantSpacing = _dPlantSpacing;
		m_dRotationAngle = _dRotationAngle;
		m_dStartingDistance = _dStartingDistance;
		m_PlacementSymbol = _PlacementSymbol;
		m_GenericModelIDList = null;
		m_bUseRightSide = false;
		m_bIsReverse = false;
		m_TransactionContainer = _TransactionContainer;
		m_bAlignTo = false;
		m_ParkingPlacementSettings = null;
		m_strFirstLegSegmentID = string.Empty;
		m_strFirstPlacementSegmentID = string.Empty;
	}
}
