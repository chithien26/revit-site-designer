using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilyRegionContainer
{
	private string m_strFamilyName;

	private List<FamilySymbol> m_FamilySymbolList;

	private List<RegionContainer> m_RegionElementList;

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

	public List<RegionContainer> RegionElementList
	{
		get
		{
			return m_RegionElementList;
		}
		set
		{
			m_RegionElementList = value;
		}
	}

	public FamilyRegionContainer(string _strFamilyName, List<FamilySymbol> _FamilySymbolList)
	{
		m_strFamilyName = _strFamilyName;
		m_FamilySymbolList = _FamilySymbolList;
		m_RegionElementList = new List<RegionContainer>();
	}
}
