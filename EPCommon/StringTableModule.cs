using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace EPCommon;

public class StringTableModule
{
	private Hashtable m_HTStringTable = new Hashtable();

	public StringTableModule(RevitProduct _RevitProduct)
	{
		ReadLanguageFile(_RevitProduct);
	}

	protected void ReadStringEntry(ref StringTableInfo _StringEntry, XmlElementInfo _XMLElement)
	{
		_StringEntry.strID = _XMLElement.GetAttribute("id");
		_StringEntry.strValue = _XMLElement.GetAttribute("value");
	}

	private void ReadLanguageFile(RevitProduct _RevitProduct)
	{
		StringTableInfo _StringEntry = default;
		string _strSupportPath = string.Empty;
		if (_RevitProduct == RevitProduct.LANDCADD)
		{
			ApplicationUtility.RegistryPaths.GetSupportPath("LANDCADD_" + ApplicationUtility.ThreeLetterWindowsLanguageName + ".xml", ref _strSupportPath);
			if (!File.Exists(_strSupportPath))
			{
				ApplicationUtility.RegistryPaths.GetSupportPath("LANDCADD_enu.xml", ref _strSupportPath);
			}
		}
		else
		{
			ApplicationUtility.RegistryPaths.GetSupportPath("SITEDESIGNER_" + ApplicationUtility.ThreeLetterWindowsLanguageName + ".xml", ref _strSupportPath);
			if (!File.Exists(_strSupportPath))
			{
				ApplicationUtility.RegistryPaths.GetSupportPath("SITEDESIGNER_enu.xml", ref _strSupportPath);
			}
		}
		XmlEngine xmlEngine = new XmlEngine();
		xmlEngine.Read(_strSupportPath);
		XmlElementInfo element = xmlEngine.GetElement("StringTable");
		if (element == null)
		{
			return;
		}
		XmlElementInfo xmlElementInfo = null;
		m_HTStringTable.Clear();
		for (int i = 0; i < element.ElementCount; i++)
		{
			xmlElementInfo = element.GetElement(i);
			if (xmlElementInfo == null)
			{
				continue;
			}
			ReadStringEntry(ref _StringEntry, xmlElementInfo);
			if (_StringEntry.strID != null)
			{
				if (m_HTStringTable.ContainsKey(_StringEntry.strID))
				{
					MessageBox.Show("Duplicate String ID: " + _StringEntry.strID + " in file: " + xmlEngine.FileName);
				}
				else if (!_StringEntry.Equals(string.Empty))
				{
					m_HTStringTable.Add(_StringEntry.strID, _StringEntry);
				}
				else
				{
					MessageBox.Show("Empty value: " + _StringEntry.strID + " in file: " + xmlEngine.FileName);
				}
			}
		}
	}

	public string GetString(string _strID)
	{
		string empty = string.Empty;
		return GetString(empty, _strID);
	}

	public string GetString(string _strWindowsLanguageName, string _strID)
	{
		string text = string.Empty;
		if (_strWindowsLanguageName != string.Empty && m_HTStringTable.ContainsKey(_strWindowsLanguageName + "-" + _strID))
		{
			StringTableInfo stringTableInfo = (StringTableInfo)m_HTStringTable[_strWindowsLanguageName + "-" + _strID];
			if (string.Compare(stringTableInfo.strID, _strWindowsLanguageName + "-" + _strID, ignoreCase: true) == 0)
			{
				text = stringTableInfo.strValue;
			}
		}
		if (text == string.Empty && m_HTStringTable.ContainsKey(_strID))
		{
			StringTableInfo stringTableInfo2 = (StringTableInfo)m_HTStringTable[_strID];
			if (string.Compare(stringTableInfo2.strID, _strID, ignoreCase: true) == 0)
			{
				text = stringTableInfo2.strValue;
			}
		}
		if (text == string.Empty)
		{
			if (Debugger.IsAttached)
			{
				Debugger.Break();
			}
			else
			{
				MessageBox.Show("Missing string label:  " + _strID, "XML Error");
			}
		}
		return text;
	}
}
