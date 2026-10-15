using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilySymbolContainer
{
	private FamilySymbol m_FamilySymbol;

	private string m_ServiceLife;

	private string m_MaxHeight;

	private string m_MaxWidth;

	public FamilySymbol FamilySymbol
	{
		get
		{
			return m_FamilySymbol;
		}
		set
		{
			m_FamilySymbol = value;
		}
	}

	public string ServiceLife
	{
		get
		{
			return m_ServiceLife;
		}
		set
		{
			m_ServiceLife = value;
		}
	}

	public string MaxHeight
	{
		get
		{
			return m_MaxHeight;
		}
		set
		{
			m_MaxHeight = value;
		}
	}

	public string MaxWidth
	{
		get
		{
			return m_MaxWidth;
		}
		set
		{
			m_MaxWidth = value;
		}
	}

	public FamilySymbolContainer()
	{
		m_ServiceLife = EPShared.LocalRM.GetString("NotAvailableString");
		m_MaxWidth = EPShared.LocalRM.GetString("NotAvailableString");
		m_MaxWidth = EPShared.LocalRM.GetString("NotAvailableString");
	}
}
