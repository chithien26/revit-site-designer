using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

namespace EPCommon;

public class GenericModelFamilyInstanceContainer
{
	private ElementId m_ElementId;

	private ElementId m_FamilyInstanceElementId;

	private FamilyInstance m_FamilyInstance;

	public ElementId ID => m_ElementId;

	public ElementId FamilyInstanceID => m_FamilyInstanceElementId;

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

	public GenericModelFamilyInstanceContainer(ElementId _ElementId, FamilyInstance _FamilyInstance)
	{
		m_ElementId = _ElementId;
		m_FamilyInstance = _FamilyInstance;
		if (m_FamilyInstance != null)
		{
			m_FamilyInstanceElementId = m_FamilyInstance.Id;
		}
	}

	public FamilyInstance GetFamilyInstance(Document _CurrentDocument)
	{
		string empty = string.Empty;
		Element element = null;
		FamilyInstance familyInstance = null;
		try
		{
			empty = m_FamilyInstance.UniqueId;
		}
		catch (Exception ex)
		{
			if (ex is AccessViolationException || ex is Autodesk.Revit.Exceptions.InvalidOperationException)
			{
				element = _CurrentDocument.GetElement(m_FamilyInstanceElementId);
				if (!(element is FamilyInstance familyInstance2))
				{
					return null;
				}
				try
				{
					empty = familyInstance2.UniqueId;
				}
				catch (Exception ex2)
				{
					if (ex2 is AccessViolationException || ex2 is Autodesk.Revit.Exceptions.InvalidOperationException)
					{
						return null;
					}
				}
				m_FamilyInstance = familyInstance2;
			}
		}
		return m_FamilyInstance;
	}
}
