using System.Runtime.InteropServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace EPCommon;

[StructLayout(LayoutKind.Sequential)]
public class Eps_Line : BaseLine
{
	private bool m_bIsReverse;

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

	public Eps_Line()
	{
	}

	public Eps_Line(Line _Line)
		: base(_Line)
	{
	}

	public Eps_Line(Eps_Line _Line)
		: base(_Line)
	{
	}

	public Eps_Line(BaseLine _Line)
		: base(_Line)
	{
	}

	public Eps_Line Reverse()
	{
		Eps_Line eps_Line = new Eps_Line();
		eps_Line.StartPnt = EndPnt;
		eps_Line.EndPnt = StartPnt;
		eps_Line.IsReverse = !m_bIsReverse;
		return eps_Line;
	}

	public Line RevitLine(UIDocument _CurDocument)
	{
		return RevitLine(_CurDocument.Document);
	}

	public Line RevitLine(Document _CurDocument)
	{
		return RevitUtility.CreateLine(StartPnt.XYZ, EndPnt.XYZ, bBound: true);
	}
}
