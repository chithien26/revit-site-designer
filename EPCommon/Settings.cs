using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EPCommon;

public class Settings
{
	private const string m_strIntegerAbbr = "I";

	private const string m_strLongAbbr = "L";

	private const string m_strShortAbbr = "H";

	private const string m_strStringAbbr = "S";

	private const string m_strDoubleAbbr = "D";

	private const string m_strFloatAbbr = "F";

	private const string m_strSettingAppName = "Setting";

	private const string m_strAppDateTime = "DateTime";

	private const int m_nMaxStringBuilderSize = 32767;

	private const int m_nMaxProjectSettingsInIni = 10;

	private string m_strProjectIniFileName;

	private List<ProjectSettingContainer> m_ProjectSettingList;

	public string ProjectIniFileName
	{
		get
		{
			return m_strProjectIniFileName;
		}
		set
		{
			m_strProjectIniFileName = value;
		}
	}

	public Settings(string _strProjectIniFileName)
	{
		m_strProjectIniFileName = _strProjectIniFileName;
	}

	public bool eplSetE_GetSetting(string _strSettingsName, ref double _dReturnValue)
	{
		return eplSetE_GetSetting("Setting", _strSettingsName, ref _dReturnValue);
	}

	public bool eplSetE_GetSetting(string _strSettingsName, ref double _dReturnValue, string _strProjectIniFileName)
	{
		return eplSetE_GetSetting("Setting", _strSettingsName, ref _dReturnValue, _strProjectIniFileName);
	}

	public bool eplSetE_GetSetting(string _strSettingAppName, string _strSettingsName, ref double _dReturnValue)
	{
		string _strReturnValue = string.Empty;
		bool setting = GetSetting(_strSettingAppName, _strSettingsName, ref _strReturnValue, "D", m_strProjectIniFileName);
		if (setting)
		{
			_dReturnValue = ValueConversion.ToDouble(_strReturnValue);
		}
		return setting;
	}

	public bool eplSetE_GetSetting(string _strSettingAppName, string _strSettingsName, ref double _dReturnValue, string _strProjectIniFileName)
	{
		string _strReturnValue = string.Empty;
		bool setting = GetSetting(_strSettingAppName, _strSettingsName, ref _strReturnValue, "D", _strProjectIniFileName);
		if (setting)
		{
			_dReturnValue = ValueConversion.ToDouble(_strReturnValue);
		}
		return setting;
	}

	public bool eplSetE_GetSetting(string _strSettingsName, ref float _fReturnValue)
	{
		return eplSetE_GetSetting("Setting", _strSettingsName, ref _fReturnValue);
	}

	public bool eplSetE_GetSetting(string _strSettingAppName, string _strSettingsName, ref float _fReturnValue)
	{
		string _strReturnValue = string.Empty;
		bool setting = GetSetting(_strSettingAppName, _strSettingsName, ref _strReturnValue, "F", m_strProjectIniFileName);
		if (setting)
		{
			_fReturnValue = Convert.ToSingle(_strReturnValue);
		}
		return setting;
	}

	public bool eplSetE_GetSetting(string _strSettingsName, ref string _strReturnValue)
	{
		return eplSetE_GetSetting("Setting", _strSettingsName, ref _strReturnValue);
	}

	public bool eplSetE_GetSetting(string _strSettingsName, ref string _strReturnValue, string _strProjectIniFileName)
	{
		return eplSetE_GetSetting("Setting", _strSettingsName, ref _strReturnValue, _strProjectIniFileName);
	}

	public bool eplSetE_GetSetting(string _strSettingAppName, string _strSettingsName, ref string _strReturnValue)
	{
		return GetSetting(_strSettingAppName, _strSettingsName, ref _strReturnValue, "S", m_strProjectIniFileName);
	}

	public bool eplSetE_GetSetting(string _strSettingAppName, string _strSettingsName, ref string _strReturnValue, string _strProjectIniFileName)
	{
		return GetSetting(_strSettingAppName, _strSettingsName, ref _strReturnValue, "S", _strProjectIniFileName);
	}

	public bool eplSetE_GetSetting(string _strSettingsName, ref int _nReturnValue)
	{
		return eplSetE_GetSetting("Setting", _strSettingsName, ref _nReturnValue);
	}

	public bool eplSetE_GetSetting(string _strSettingsName, ref int _nReturnValue, string _strProjectIniFileName)
	{
		return eplSetE_GetSetting("Setting", _strSettingsName, ref _nReturnValue, _strProjectIniFileName);
	}

	public bool eplSetE_GetSetting(string _strSettingAppName, string _strSettingsName, ref int _nReturnValue)
	{
		string _strReturnValue = string.Empty;
		bool setting = GetSetting(_strSettingAppName, _strSettingsName, ref _strReturnValue, "I", m_strProjectIniFileName);
		if (setting)
		{
			_nReturnValue = Convert.ToInt32(_strReturnValue);
		}
		return setting;
	}

	public bool eplSetE_GetSetting(string _strSettingAppName, string _strSettingsName, ref int _nReturnValue, string _strProjectIniFileName)
	{
		string _strReturnValue = string.Empty;
		bool setting = GetSetting(_strSettingAppName, _strSettingsName, ref _strReturnValue, "I", _strProjectIniFileName);
		if (setting)
		{
			_nReturnValue = Convert.ToInt32(_strReturnValue);
		}
		return setting;
	}

	public bool eplSetE_GetSetting(string _strSettingsName, ref long _lReturnValue)
	{
		return eplSetE_GetSetting("Setting", _strSettingsName, ref _lReturnValue);
	}

	public bool eplSetE_GetSetting(string _strSettingAppName, string _strSettingsName, ref long _lReturnValue)
	{
		string _strReturnValue = string.Empty;
		bool setting = GetSetting(_strSettingAppName, _strSettingsName, ref _strReturnValue, "L", m_strProjectIniFileName);
		if (!setting) setting = GetSetting(_strSettingAppName, _strSettingsName, ref _strReturnValue, "I", m_strProjectIniFileName);
		if (setting)
		{
			_lReturnValue = Convert.ToInt64(_strReturnValue);
		}
		return setting;
	}

	public bool eplSetE_GetSetting(string _strSettingsName, ref short _sReturnValue)
	{
		return eplSetE_GetSetting("Setting", _strSettingsName, ref _sReturnValue);
	}

	public bool eplSetE_GetSetting(string _strSettingAppName, string _strSettingsName, ref short _sReturnValue)
	{
		string _strReturnValue = string.Empty;
		bool setting = GetSetting(_strSettingAppName, _strSettingsName, ref _strReturnValue, "H", m_strProjectIniFileName);
		if (setting)
		{
			_sReturnValue = Convert.ToInt16(_strReturnValue);
		}
		return setting;
	}

	public bool eplSetE_GetSetting(string _strSettingAppName, string _strSettingsName, ref string _strReturnValue, string _strAbbr, string _strProjectIniFileName)
	{
		return GetSetting(_strSettingAppName, _strSettingsName, ref _strReturnValue, _strAbbr, _strProjectIniFileName);
	}

	private bool GetSetting(string _strSettingAppName, string _strSettingsName, ref string _strReturnValue, string _strAbbr, string _strProjectIniFileName)
	{
		if (_strSettingsName != string.Empty)
		{
			StringBuilder stringBuilder = new StringBuilder(32767);
			string empty = string.Empty;
			string strKeyName = $"{_strSettingsName}" + _strAbbr;
			GetPrivateProfileString(_strSettingAppName, strKeyName, empty, stringBuilder, 32767, _strProjectIniFileName);
			if (stringBuilder != null && stringBuilder.Length > 0)
			{
				_strReturnValue = stringBuilder.ToString();
				return true;
			}
		}
		return false;
	}

	public bool eplSetE_SetSetting(string _strSettingsName, int _nValue)
	{
		return SetSetting("Setting", _strSettingsName, Convert.ToString(_nValue), "I");
	}

	public bool eplSetE_SetSetting(string _strSettingAppName, string _strSettingsName, int _nValue)
	{
		return SetSetting(_strSettingAppName, _strSettingsName, Convert.ToString(_nValue), "I");
	}

	public bool eplSetE_SetSetting(string _strSettingsName, long _lValue)
	{
		return SetSetting("Setting", _strSettingsName, Convert.ToString(_lValue), "L");
	}

	public bool eplSetE_SetSetting(string _strSettingAppName, string _strSettingsName, long _lValue)
	{
		return SetSetting(_strSettingAppName, _strSettingsName, Convert.ToString(_lValue), "L");
	}

	public bool eplSetE_SetSetting(string _strSettingsName, short _sValue)
	{
		return SetSetting("Setting", _strSettingsName, Convert.ToString(_sValue), "H");
	}

	public bool eplSetE_SetSetting(string _strSettingAppName, string _strSettingsName, short _sValue)
	{
		return SetSetting(_strSettingAppName, _strSettingsName, Convert.ToString(_sValue), "H");
	}

	public bool eplSetE_SetSetting(string _strSettingsName, double _dValue)
	{
		return SetSetting("Setting", _strSettingsName, ValueConversion.ToString(_dValue), "D");
	}

	public bool eplSetE_SetSetting(string _strSettingAppName, string _strSettingsName, double _dValue)
	{
		return SetSetting(_strSettingAppName, _strSettingsName, ValueConversion.ToString(_dValue), "D");
	}

	public bool eplSetE_SetSetting(string _strSettingsName, float _fValue)
	{
		return SetSetting("Setting", _strSettingsName, Convert.ToString(_fValue), "F");
	}

	public bool eplSetE_SetSetting(string _strSettingAppName, string _strSettingsName, float _fValue)
	{
		return SetSetting(_strSettingAppName, _strSettingsName, Convert.ToString(_fValue), "F");
	}

	public bool eplSetE_SetSetting(string _strSettingsName, string _strValue)
	{
		return SetSetting("Setting", _strSettingsName, _strValue, "S");
	}

	public bool eplSetE_SetSetting(string _strSettingAppName, string _strSettingsName, string _strValue)
	{
		return SetSetting(_strSettingAppName, _strSettingsName, _strValue, "S");
	}

	private bool SetSetting(string _strSettingAppName, string _strSettingsName, string _strValue, string _strAbbr)
	{
		string strKeyName = $"{_strSettingsName}" + _strAbbr;
		WritePrivateProfileString(_strSettingAppName, strKeyName, _strValue, m_strProjectIniFileName);
		return true;
	}

	public static bool eplSetE_SetSetting(string _strSettingAppName, string _strSettingsName, string _strValue, string _strFileName)
	{
		return WritePrivateProfileString(_strSettingAppName, _strSettingsName, _strValue, _strFileName);
	}

	public bool eplSetE_SetSettingWithAbbreviation(string _strSettingAppName, string _strSettingsName, string _strValue)
	{
		return WritePrivateProfileString(_strSettingAppName, _strSettingsName, _strValue, m_strProjectIniFileName);
	}

	public void eplSetE_UpdateIniInformation(string _strSettingAppName)
	{
		int num = 0;
		string empty = string.Empty;
		DateTime now = DateTime.Now;
		eplSetE_SetSetting(_strSettingAppName, "DateTime", Convert.ToString(now));
		if (m_ProjectSettingList == null)
		{
			m_ProjectSettingList = new List<ProjectSettingContainer>();
			IntPtr intPtr = Marshal.AllocCoTaskMem(32767);
			string[] array;
			try
			{
				int privateProfileSectionNames = GetPrivateProfileSectionNames(intPtr, 32767u, m_strProjectIniFileName);
				array = ConvertNullSeperatedStringToStringArray(intPtr, privateProfileSectionNames);
			}
			finally
			{
				Marshal.FreeCoTaskMem(intPtr);
			}
			for (num = 0; num < array.Length; num++)
			{
				if (string.Compare(array[num], "Setting", ignoreCase: true) != 0)
				{
					string _strReturnValue = string.Empty;
					if (eplSetE_GetSetting(array[num], "DateTime", ref _strReturnValue) && _strReturnValue != string.Empty)
					{
						ProjectSettingContainer item = new ProjectSettingContainer(array[num], Convert.ToDateTime(_strReturnValue));
						m_ProjectSettingList.Add(item);
					}
				}
			}
		}
		else
		{
			bool flag = false;
			for (num = 0; num < m_ProjectSettingList.Count; num++)
			{
				empty = m_ProjectSettingList[num].AppName;
				if (string.Compare(_strSettingAppName, empty, ignoreCase: true) == 0)
				{
					flag = true;
					m_ProjectSettingList[num].CurrentDateTime = now;
					break;
				}
			}
			if (!flag)
			{
				ProjectSettingContainer item = new ProjectSettingContainer(_strSettingAppName, now);
				m_ProjectSettingList.Add(item);
			}
		}
		ProjectSettingComparer comparer = new ProjectSettingComparer(_bOrderByDescending: true);
		m_ProjectSettingList.Sort(comparer);
		for (num = m_ProjectSettingList.Count - 1; num >= 10; num--)
		{
			empty = m_ProjectSettingList[num].AppName;
			WritePrivateProfileString(empty, null, null, m_strProjectIniFileName);
			m_ProjectSettingList.RemoveAt(num);
		}
	}

	private static string[] ConvertNullSeperatedStringToStringArray(IntPtr _IntPtr, int _nValueLength)
	{
		if (_nValueLength == 0)
		{
			return new string[0];
		}
		string text = Marshal.PtrToStringAuto(_IntPtr, _nValueLength - 1);
		return text.Split(default(char));
	}

	public List<KeyValuePair<string, string>> eplSetE_GetSectionValuesAsList(string _strSettingAppName)
	{
		return eplSetE_GetSectionValuesAsList(_strSettingAppName, m_strProjectIniFileName);
	}

	public static List<KeyValuePair<string, string>> eplSetE_GetSectionValuesAsList(string _strSettingAppName, string _strFileName)
	{
		if (_strSettingAppName == null)
		{
			throw new ArgumentNullException("SettingAppName");
		}
		IntPtr intPtr = Marshal.AllocCoTaskMem(32767);
		string[] array;
		try
		{
			int privateProfileSection = GetPrivateProfileSection(_strSettingAppName, intPtr, 32767, _strFileName);
			array = ConvertNullSeperatedStringToStringArray(intPtr, privateProfileSection);
		}
		finally
		{
			Marshal.FreeCoTaskMem(intPtr);
		}
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			int num = array[i].IndexOf('=');
			string key = array[i].Substring(0, num);
			string value = array[i].Substring(num + 1, array[i].Length - num - 1);
			list.Add(new KeyValuePair<string, string>(key, value));
		}
		return list;
	}

	public static bool eplSetE_DeleteSection(string _strSettingAppName, string _strFileName)
	{
		if (_strSettingAppName == null)
		{
			throw new ArgumentNullException("SettingAppName");
		}
		return WritePrivateProfileString(_strSettingAppName, null, null, _strFileName);
	}

	public bool eplSetE_DeleteIntegerSetting(string _strSettingsName)
	{
		string strKeyName = $"{_strSettingsName}" + "I";
		return WritePrivateProfileString("Setting", strKeyName, null, m_strProjectIniFileName);
	}

	public bool eplSetE_DeleteStringSetting(string _strSettingsName)
	{
		string strKeyName = $"{_strSettingsName}" + "S";
		return WritePrivateProfileString("Setting", strKeyName, null, m_strProjectIniFileName);
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	public static extern int GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, uint nSize, string lpFileName);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	private static extern uint GetPrivateProfileString(string _strAppName, string _strKeyName, string _strDefault, StringBuilder ReturnedString, int _nSize, string _strFileName);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	private static extern int GetPrivateProfileInt(string _strAppName, string _strKeyName, int _strDefault, string _strFileName);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	private static extern int GetPrivateProfileSection(string _strAppName, IntPtr lpReturnedString, int _nSize, string _strFileName);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool WritePrivateProfileString(string _strAppName, string _strKeyName, string _strValue, string _strFileName);
}
