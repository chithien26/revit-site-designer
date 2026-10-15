using System.Collections.Generic;

namespace EPCommon;

public class LandXMLAlignment
{
	private string m_strName = string.Empty;

	private string m_strLength = string.Empty;

	private string m_strStartStation = string.Empty;

	private Eps_Point m_StartPoint = new Eps_Point();

	private string m_strAreaUnit = string.Empty;

	private string m_strLinearUnit = string.Empty;

	private string m_strVolumeUnit = string.Empty;

	private string m_strAngularUnit = string.Empty;

	private List<Eps_Line> m_Lines = new List<Eps_Line>();

	private List<Eps_Arc> m_Curves = new List<Eps_Arc>();

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

	public string Length
	{
		get
		{
			return m_strLength;
		}
		set
		{
			m_strLength = value;
		}
	}

	public string StartStation
	{
		get
		{
			return m_strStartStation;
		}
		set
		{
			m_strStartStation = value;
		}
	}

	public Eps_Point StartPoint
	{
		get
		{
			return m_StartPoint;
		}
		set
		{
			m_StartPoint = value;
		}
	}

	public List<Eps_Line> Lines
	{
		get
		{
			return m_Lines;
		}
		set
		{
			m_Lines = value;
		}
	}

	public List<Eps_Arc> Curves
	{
		get
		{
			return m_Curves;
		}
		set
		{
			m_Curves = value;
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

	public string AngularUnit
	{
		get
		{
			return m_strAngularUnit;
		}
		set
		{
			m_strAngularUnit = value;
		}
	}

	public static LandXMLAlignment CopyAlignment(LandXMLAlignment _CurrentAlignment)
	{
		LandXMLAlignment landXMLAlignment = new LandXMLAlignment();
		landXMLAlignment.AreaUnit = _CurrentAlignment.AreaUnit;
		landXMLAlignment.Curves = new List<Eps_Arc>(_CurrentAlignment.Curves);
		landXMLAlignment.Length = _CurrentAlignment.Length;
		landXMLAlignment.LinearUnit = _CurrentAlignment.LinearUnit;
		landXMLAlignment.Lines = new List<Eps_Line>(_CurrentAlignment.Lines);
		landXMLAlignment.Name = _CurrentAlignment.Name;
		landXMLAlignment.StartPoint = new Eps_Point(_CurrentAlignment.StartPoint);
		landXMLAlignment.StartStation = _CurrentAlignment.StartStation;
		landXMLAlignment.VolumeUnit = _CurrentAlignment.VolumeUnit;
		return landXMLAlignment;
	}
}
