using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class LandXMLSurface
{
	private string m_strSurfaceName = string.Empty;

	private string m_strSurfaceDesc = string.Empty;

	private string m_str2DSurfaceArea = string.Empty;

	private string m_str3DSurfaceArea = string.Empty;

	private string m_strElevationMax = string.Empty;

	private string m_strElevationMin = string.Empty;

	private string m_strAreaUnit = string.Empty;

	private string m_strLinearUnit = string.Empty;

	private string m_strVolumeUnit = string.Empty;

	private string m_strSurfacePhase = string.Empty;

	private string m_strXMLSurfaceName = string.Empty;

	private string m_strOverwriteSurfaceID = string.Empty;

	private List<Eps_Point> m_SurfacePoints;

	private int m_nSurfacePointCount;

	private Phase m_Phase;

	public string SurfaceName
	{
		get
		{
			return m_strSurfaceName;
		}
		set
		{
			m_strSurfaceName = value;
		}
	}

	public List<Eps_Point> SurfacePoints
	{
		get
		{
			return m_SurfacePoints;
		}
		set
		{
			m_SurfacePoints = value;
		}
	}

	public string SurfaceDescription
	{
		get
		{
			return m_strSurfaceDesc;
		}
		set
		{
			m_strSurfaceDesc = value;
		}
	}

	public string SurfaceArea2D
	{
		get
		{
			return m_str2DSurfaceArea;
		}
		set
		{
			m_str2DSurfaceArea = value;
		}
	}

	public string SurfaceArea3D
	{
		get
		{
			return m_str3DSurfaceArea;
		}
		set
		{
			m_str3DSurfaceArea = value;
		}
	}

	public string MaxElevation
	{
		get
		{
			return m_strElevationMax;
		}
		set
		{
			m_strElevationMax = value;
		}
	}

	public string MinElevation
	{
		get
		{
			return m_strElevationMin;
		}
		set
		{
			m_strElevationMin = value;
		}
	}

	public string AreaUnit
	{
		get
		{
			return m_strAreaUnit;
		}
		set
		{
			m_strAreaUnit = value;
		}
	}

	public string LinearUnit
	{
		get
		{
			return m_strLinearUnit;
		}
		set
		{
			m_strLinearUnit = value;
		}
	}

	public string VolumeUnit
	{
		get
		{
			return m_strVolumeUnit;
		}
		set
		{
			m_strVolumeUnit = value;
		}
	}

	public string SurfacePhase
	{
		get
		{
			return m_strSurfacePhase;
		}
		set
		{
			m_strSurfacePhase = value;
		}
	}

	public string OverwriteSurfaceID
	{
		get
		{
			return m_strOverwriteSurfaceID;
		}
		set
		{
			m_strOverwriteSurfaceID = value;
		}
	}

	public int SurfacePointCount
	{
		get
		{
			return m_nSurfacePointCount;
		}
		set
		{
			m_nSurfacePointCount = value;
		}
	}

	public string XMLSurfaceName
	{
		get
		{
			return m_strXMLSurfaceName;
		}
		set
		{
			m_strXMLSurfaceName = value;
		}
	}

	public Phase Phase
	{
		get
		{
			return m_Phase;
		}
		set
		{
			m_Phase = value;
		}
	}

	public LandXMLSurface()
	{
		m_SurfacePoints = null;
	}
}
