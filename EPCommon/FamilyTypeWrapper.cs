using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilyTypeWrapper
{
	private string m_strFamilyTypeName;

	private FamilyType m_FamilyType;

	public string FamilyTypeName
	{
		get
		{
			return m_strFamilyTypeName;
		}
		set
		{
			m_strFamilyTypeName = value;
		}
	}

	public FamilyType FamilyType
	{
		get
		{
			return m_FamilyType;
		}
		set
		{
			m_FamilyType = value;
			m_strFamilyTypeName = m_FamilyType.Name;
		}
	}

	public FamilyTypeWrapper(FamilyType _FamilyType)
	{
		m_FamilyType = _FamilyType;
		m_strFamilyTypeName = m_FamilyType.Name;
	}

	public FamilyTypeWrapper(string _strFamilyTypeName)
	{
		m_strFamilyTypeName = _strFamilyTypeName;
	}
}
