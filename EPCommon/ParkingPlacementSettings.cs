using Autodesk.Revit.DB;

namespace EPCommon;

public class ParkingPlacementSettings
{
	private FamilySymbol m_ADAStallSymbol;

	private FamilySymbol m_IslandSymbol;

	private FamilySymbol m_BumperSymbol;

	private double m_dStallAdjustWidth;

	private double m_dADAStallWidth;

	private double m_dADAStallAdjustWidth;

	private double m_dBumperHeight;

	private double m_dBumperWidth;

	private double m_dBumperOffset;

	private double m_dBumperSetback;

	private double m_dIslandHeight;

	private double m_dIslandWidth;

	private double m_dStallLength;

	private double m_dStripeLength;

	private double m_dAdjustAisleWidth;

	private double m_dADAStallAisleAdjustWidth;

	private double m_dADAStallAdjustWidthNoAisleWidth;

	private double m_dAreaDistanceBetweenCenterlines;

	private double m_dAreaAisleWidth;

	private bool m_bPlaceIsland;

	private bool m_bPlaceBumper;

	private bool m_bSidesOnIsland;

	private bool m_bCenterIsland;

	private bool m_bOptimizeParking;

	private bool m_bAreaBiDirectionalAisles;

	private bool m_bGetAisleWidthAdjustment;

	private bool m_bPromptForDirection;

	private bool m_bMiddleStallPlacement;

	private bool m_bADAStallSharedAccessibleAisle;

	private int m_nCurrentStallCount;

	private int m_nStallCount;

	private int m_nCurrentADAStallCount;

	private int m_nADAStallCount;

	private int m_nAreaRowCount;

	private int m_nAreaRowSpacingOptions;

	public FamilySymbol ADAStallSymbol
	{
		get
		{
			return m_ADAStallSymbol;
		}
		set
		{
			m_ADAStallSymbol = value;
		}
	}

	public FamilySymbol IslandSymbol
	{
		get
		{
			return m_IslandSymbol;
		}
		set
		{
			m_IslandSymbol = value;
		}
	}

	public FamilySymbol BumperSymbol
	{
		get
		{
			return m_BumperSymbol;
		}
		set
		{
			m_BumperSymbol = value;
		}
	}

	public double ADAStallWidth
	{
		get
		{
			return m_dADAStallWidth;
		}
		set
		{
			m_dADAStallWidth = value;
		}
	}

	public double BumperHeight
	{
		get
		{
			return m_dBumperHeight;
		}
		set
		{
			m_dBumperHeight = value;
		}
	}

	public double BumperWidth
	{
		get
		{
			return m_dBumperWidth;
		}
		set
		{
			m_dBumperWidth = value;
		}
	}

	public double BumperOffset
	{
		get
		{
			return m_dBumperOffset;
		}
		set
		{
			m_dBumperOffset = value;
		}
	}

	public double BumperSetback
	{
		get
		{
			return m_dBumperSetback;
		}
		set
		{
			m_dBumperSetback = value;
		}
	}

	public double IslandHeight
	{
		get
		{
			return m_dIslandHeight;
		}
		set
		{
			m_dIslandHeight = value;
		}
	}

	public double IslandWidth
	{
		get
		{
			return m_dIslandWidth;
		}
		set
		{
			m_dIslandWidth = value;
		}
	}

	public double StallLength
	{
		get
		{
			return m_dStallLength;
		}
		set
		{
			m_dStallLength = value;
		}
	}

	public double AreaDistanceBetweenCenterlines
	{
		get
		{
			return m_dAreaDistanceBetweenCenterlines;
		}
		set
		{
			m_dAreaDistanceBetweenCenterlines = value;
		}
	}

	public double AreaAisleWidth
	{
		get
		{
			return m_dAreaAisleWidth;
		}
		set
		{
			m_dAreaAisleWidth = value;
		}
	}

	public bool PlaceIsland
	{
		get
		{
			return m_bPlaceIsland;
		}
		set
		{
			m_bPlaceIsland = value;
		}
	}

	public bool PlaceBumper
	{
		get
		{
			return m_bPlaceBumper;
		}
		set
		{
			m_bPlaceBumper = value;
		}
	}

	public bool OptimizeParking
	{
		get
		{
			return m_bOptimizeParking;
		}
		set
		{
			m_bOptimizeParking = value;
		}
	}

	public bool AreaBiDirectionalAisles
	{
		get
		{
			return m_bAreaBiDirectionalAisles;
		}
		set
		{
			m_bAreaBiDirectionalAisles = value;
		}
	}

	public bool PromptForDirection
	{
		get
		{
			return m_bPromptForDirection;
		}
		set
		{
			m_bPromptForDirection = value;
		}
	}

	public bool ADAStallSharedAccessibleAisle
	{
		get
		{
			return m_bADAStallSharedAccessibleAisle;
		}
		set
		{
			m_bADAStallSharedAccessibleAisle = value;
		}
	}

	public int StallCount
	{
		get
		{
			return m_nStallCount;
		}
		set
		{
			if (value >= 0)
			{
				m_nStallCount = value;
			}
			else
			{
				m_nStallCount = 0;
			}
		}
	}

	public int ADAStallCount
	{
		get
		{
			return m_nADAStallCount;
		}
		set
		{
			if (value >= 0)
			{
				m_nADAStallCount = value;
			}
			else
			{
				m_nADAStallCount = 0;
			}
		}
	}

	public int AreaRowCount
	{
		get
		{
			return m_nAreaRowCount;
		}
		set
		{
			m_nAreaRowCount = value;
		}
	}

	public int AreaRowSpacingOptions
	{
		get
		{
			return m_nAreaRowSpacingOptions;
		}
		set
		{
			m_nAreaRowSpacingOptions = value;
		}
	}

	public bool MiddleStallPlacement
	{
		get
		{
			return m_bMiddleStallPlacement;
		}
		set
		{
			m_bMiddleStallPlacement = value;
		}
	}

	public bool SidesOnIsland
	{
		get
		{
			return m_bSidesOnIsland;
		}
		set
		{
			m_bSidesOnIsland = value;
		}
	}

	public bool CenterIsland
	{
		get
		{
			return m_bCenterIsland;
		}
		set
		{
			m_bCenterIsland = value;
		}
	}

	public bool GetAisleWidthAdjustment
	{
		get
		{
			return m_bGetAisleWidthAdjustment;
		}
		set
		{
			m_bGetAisleWidthAdjustment = value;
		}
	}

	public double StallAdjustWidth
	{
		get
		{
			return m_dStallAdjustWidth;
		}
		set
		{
			m_dStallAdjustWidth = value;
		}
	}

	public double ADAStallAdjustWidth
	{
		get
		{
			return m_dADAStallAdjustWidth;
		}
		set
		{
			m_dADAStallAdjustWidth = value;
		}
	}

	public double StripeLength
	{
		get
		{
			return m_dStripeLength;
		}
		set
		{
			m_dStripeLength = value;
		}
	}

	public int CurrentStallCount
	{
		get
		{
			return m_nCurrentStallCount;
		}
		set
		{
			m_nCurrentStallCount = value;
		}
	}

	public int CurrentADAStallCount
	{
		get
		{
			return m_nCurrentADAStallCount;
		}
		set
		{
			m_nCurrentADAStallCount = value;
		}
	}

	public double AdjustAisleWidth
	{
		get
		{
			return m_dAdjustAisleWidth;
		}
		set
		{
			m_dAdjustAisleWidth = value;
		}
	}

	public double ADAStallAisleAdjustWidth
	{
		get
		{
			return m_dADAStallAisleAdjustWidth;
		}
		set
		{
			m_dADAStallAisleAdjustWidth = value;
		}
	}

	public double ADAStallAdjustWidthNoAisleWidth
	{
		get
		{
			return m_dADAStallAdjustWidthNoAisleWidth;
		}
		set
		{
			m_dADAStallAdjustWidthNoAisleWidth = value;
		}
	}

	public ParkingPlacementSettings()
	{
		m_ADAStallSymbol = null;
		m_IslandSymbol = null;
		m_BumperSymbol = null;
		m_dADAStallWidth = 0.0;
		m_dBumperHeight = 0.0;
		m_dBumperWidth = 0.0;
		m_dBumperOffset = 0.0;
		m_dBumperSetback = 0.0;
		m_dIslandHeight = 0.0;
		m_dIslandWidth = 0.0;
		m_dStallLength = 0.0;
		m_dAreaDistanceBetweenCenterlines = 0.0;
		m_dAreaAisleWidth = 0.0;
		m_bPlaceIsland = false;
		m_bPlaceBumper = false;
		m_bOptimizeParking = false;
		m_bAreaBiDirectionalAisles = false;
		m_bPromptForDirection = false;
		m_bADAStallSharedAccessibleAisle = false;
		m_nStallCount = 0;
		m_nADAStallCount = 0;
		m_nAreaRowCount = 0;
		m_nAreaRowSpacingOptions = 0;
		m_bMiddleStallPlacement = false;
		m_bSidesOnIsland = false;
		m_bCenterIsland = false;
		m_bGetAisleWidthAdjustment = false;
		m_dStripeLength = 0.0;
		m_dADAStallAdjustWidth = 0.0;
		m_dADAStallAisleAdjustWidth = 0.0;
		m_dStallAdjustWidth = 0.0;
		m_nCurrentStallCount = 0;
		m_nCurrentADAStallCount = 0;
		m_dAdjustAisleWidth = 0.0;
		m_dADAStallAdjustWidthNoAisleWidth = 0.0;
	}
}
