using System.Runtime.InteropServices;
using Autodesk.Revit.DB;

namespace EPCommon;

[StructLayout(LayoutKind.Sequential)]
public class BaseLine
{
	private Eps_Point m_StartPnt;

	private Eps_Point m_EndPnt;

	public Eps_Point StartPnt
	{
		get
		{
			return m_StartPnt;
		}
		set
		{
			m_StartPnt = value;
		}
	}

	public Eps_Point EndPnt
	{
		get
		{
			return m_EndPnt;
		}
		set
		{
			m_EndPnt = value;
		}
	}

	public BaseLine()
	{
		m_StartPnt = new Eps_Point();
		m_EndPnt = new Eps_Point();
	}

	public BaseLine(Line _Line)
	{
		m_StartPnt = new Eps_Point(_Line.GetEndPoint(0));
		m_EndPnt = new Eps_Point(_Line.GetEndPoint(1));
	}

	public BaseLine(BaseLine _Line)
	{
		m_EndPnt = new Eps_Point(_Line.EndPnt);
		m_StartPnt = new Eps_Point(_Line.StartPnt);
	}

	public bool IsEqual(BaseLine _CompareLine)
	{
		bool result = false;
		if ((EPShared.eplGeoE_ArePntsDuplicates2D(m_StartPnt, _CompareLine.StartPnt) && EPShared.eplGeoE_ArePntsDuplicates2D(m_EndPnt, _CompareLine.EndPnt)) || (EPShared.eplGeoE_ArePntsDuplicates2D(m_EndPnt, _CompareLine.StartPnt) && EPShared.eplGeoE_ArePntsDuplicates2D(m_StartPnt, _CompareLine.EndPnt)))
		{
			result = true;
		}
		return result;
	}
}
