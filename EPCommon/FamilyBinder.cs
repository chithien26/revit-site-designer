using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilyBinder
{
	private string m_strFamilyName;

	private string m_strFamilyPath;

	private List<FamilyTypeContainer> m_FamilySymbolList;

	private bool m_bModified;

	private Document m_FamilyDocument;

	private Family m_Family;

	private bool m_bLoaded;

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

	public string FamilyPath
	{
		get
		{
			return m_strFamilyPath;
		}
		set
		{
			m_strFamilyPath = value;
		}
	}

	public List<FamilyTypeContainer> FamilyTypeList
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

	public bool IsModified
	{
		get
		{
			return m_bModified;
		}
		set
		{
			m_bModified = value;
		}
	}

	public Document FamilyDocument
	{
		get
		{
			return m_FamilyDocument;
		}
		set
		{
			m_FamilyDocument = value;
		}
	}

	public Family BinderFamily
	{
		get
		{
			return m_Family;
		}
		set
		{
			m_Family = value;
		}
	}

	public bool Loaded
	{
		get
		{
			return m_bLoaded;
		}
		set
		{
			m_bLoaded = value;
		}
	}

	public FamilyBinder()
	{
		m_strFamilyName = string.Empty;
		m_strFamilyPath = string.Empty;
		m_FamilySymbolList = new List<FamilyTypeContainer>();
		m_bModified = false;
	}

	public FamilyBinder(string _strFamilyName)
	{
		m_strFamilyName = _strFamilyName;
		m_strFamilyPath = string.Empty;
		m_FamilySymbolList = new List<FamilyTypeContainer>();
		m_bModified = false;
		m_FamilyDocument = null;
		m_Family = null;
	}

	public FamilyBinder(string _strFamilyName, string _strFamilyPath)
	{
		m_strFamilyName = _strFamilyName;
		m_strFamilyPath = _strFamilyPath;
		m_FamilySymbolList = new List<FamilyTypeContainer>();
		m_bModified = false;
		m_FamilyDocument = null;
		m_Family = null;
	}

	public FamilyBinder(string _strFamilyName, string _strFamilyPath, Document _FamilyDocument)
	{
		m_strFamilyName = _strFamilyName;
		m_strFamilyPath = _strFamilyPath;
		m_FamilySymbolList = new List<FamilyTypeContainer>();
		m_bModified = false;
		m_FamilyDocument = _FamilyDocument;
		m_Family = null;
	}

	public FamilyBinder(string _strFamilyName, string _strFamilyPath, Document _FamilyDocument, Family _Family)
	{
		m_strFamilyName = _strFamilyName;
		m_strFamilyPath = _strFamilyPath;
		m_FamilySymbolList = new List<FamilyTypeContainer>();
		m_bModified = false;
		m_FamilyDocument = _FamilyDocument;
		m_Family = _Family;
	}
}
