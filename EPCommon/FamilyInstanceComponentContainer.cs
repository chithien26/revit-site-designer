using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilyInstanceComponentContainer
{
	private Element m_Element;

	private FamilyInstance m_FamilyInstance;

	public Element Element => m_Element;

	public FamilyInstance FamilyInstance
	{
		get
		{
			return m_FamilyInstance;
		}
		set
		{
			m_FamilyInstance = value;
		}
	}

	public FamilyInstanceComponentContainer(Element _Element, FamilyInstance _FamilyInstance)
	{
		m_Element = _Element;
		m_FamilyInstance = _FamilyInstance;
	}
}
