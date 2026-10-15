using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilyContainer
{
	private string m_strFamilyName;

	private List<FamilySymbol> m_FamilySymbolList;

	public string FamilyName
	{
		get
		{
			return m_strFamilyName;
		}
		set
		{
			m_strFamilyName = value;
		}
	}

	public List<FamilySymbol> FamilySymbolList
	{
		get
		{
			return m_FamilySymbolList;
		}
		set
		{
			m_FamilySymbolList = value;
		}
	}

	public FamilyContainer(string _strFamilyName)
	{
		m_strFamilyName = _strFamilyName;
		m_FamilySymbolList = new List<FamilySymbol>();
	}
}
