namespace EPCommon;

public class CgPoint
{
	private int m_nName = -1;

	private string m_strCode = string.Empty;

	private string m_strDesc = string.Empty;

	private string m_strNameUnit = string.Empty;

	private Eps_Point m_CgPnt = new Eps_Point();

	public int Name
	{
		get
		{
			return m_nName;
		}
		set
		{
			m_nName = value;
		}
	}

	public string Code
	{
		get
		{
			return m_strCode;
		}
		set
		{
			m_strCode = value;
		}
	}

	public string Desc
	{
		get
		{
			return m_strDesc;
		}
		set
		{
			m_strDesc = value;
		}
	}

	public string NameUnit
	{
		get
		{
			return m_strNameUnit;
		}
		set
		{
			m_strNameUnit = value;
		}
	}

	public Eps_Point CgPnt
	{
		get
		{
			return m_CgPnt;
		}
		set
		{
			m_CgPnt = value;
		}
	}

	public CgPoint(int _nName, string _strCode, string _strDesc, string _strNameUnit, Eps_Point _CgPnt)
	{
		m_nName = _nName;
		m_strCode = _strCode;
		m_strDesc = _strDesc;
		m_strNameUnit = _strNameUnit;
		m_CgPnt = _CgPnt;
	}
}
