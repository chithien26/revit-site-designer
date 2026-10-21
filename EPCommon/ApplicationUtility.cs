using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;

namespace EPCommon;

public class ApplicationUtility
{
	private static RevitLanguageType m_RevitLanguangeType;

	private static string m_strThreeLetterWindowsLanguageName;

	private static string m_strInstallLocation;

	private static string m_strRevitIniFilename;

	private static StringTableModule m_StringTableModule;

	private static Settings m_Settings;

	private static string m_strHelpNamespace;

	private static WindowWrapper m_RevitWindowWrapper;

	private static RegistryPaths m_RegistryPaths;

	private static bool m_bIs64BitInstall = false;

	private static string m_strCurUserLandcaddRegistryKey;

	private static bool m_bIsRevitVersionValid = true;

	private static string m_strRevitVersion;


	public static bool UseMass
	{
		get
		{
			int _nReturnValue = 0;
			if (Settings.eplSetE_GetSetting("UseMass", ref _nReturnValue))
			{
				return _nReturnValue == 1;
			}
			return false;
		}
	}

	public static StringTableModule StringTable => m_StringTableModule;

	public static Settings Settings => m_Settings;

	public static string HelpFilename => m_strHelpNamespace;

	public static WindowWrapper RevitWindowWrapper => m_RevitWindowWrapper;

	public static RegistryPaths RegistryPaths => m_RegistryPaths;

	public static bool Is64BitInstall => m_bIs64BitInstall;

	public static string InstallLocation => m_strInstallLocation;

	public static string RevitIniFilename => m_strRevitIniFilename;

	public static string CurrentUserRegistryKey => m_strCurUserLandcaddRegistryKey;

	public static bool IsRevitVersionValid => m_bIsRevitVersionValid;

	public static string RevitVersion => m_strRevitVersion;

	public static RevitLanguageType RevitLanguageType => m_RevitLanguangeType;

	public static string ThreeLetterWindowsLanguageName => m_strThreeLetterWindowsLanguageName;

	private ApplicationUtility()
	{
	}

	public static bool Initialize(ControlledApplication _Application, string _strCurUserLandcaddRegistryKey, string _strINIFilename, string _strHelpFilename, IntPtr _mainWindowHandle, RevitProduct _RevitProduct)
	{
		return Initialize(_Application.VersionNumber, _Application.VersionName, _Application.Language, _strINIFilename, _strHelpFilename, _mainWindowHandle, _RevitProduct);
	}

	// Command loaders can invoke IExternalCommand without calling ExternalApplication.OnStartup.
	public static bool Initialize(Autodesk.Revit.UI.UIApplication application)
	{
		return Initialize(application.Application.VersionNumber, application.Application.VersionName, application.Application.Language,
			"SitesRevit.ini", "Site Designer for Revit.chm", application.MainWindowHandle, RevitProduct.SITES);
	}

	private static bool Initialize(string version, string versionName, LanguageType language, string _strINIFilename, string _strHelpFilename, IntPtr _mainWindowHandle, RevitProduct _RevitProduct)
	{
		m_strRevitVersion = version;
		m_bIs64BitInstall = Environment.Is64BitProcess;
		m_bIsRevitVersionValid = int.TryParse(m_strRevitVersion, out int year) && year >= 2022 && year <= 2027;
		if (!m_bIsRevitVersionValid) return false;
		RuntimePaths.LoadNativeLibrary();
		m_strCurUserLandcaddRegistryKey = @"Software\SiteDesigner\" + m_strRevitVersion;
		m_RegistryPaths = new RegistryPaths { ProgramPath = RuntimePaths.FindDataRoot(m_strRevitVersion) };
		m_Settings = new Settings(RuntimePaths.SettingsFile(m_strRevitVersion, _strINIFilename, m_RegistryPaths.ProgramPath));
		m_strHelpNamespace = Path.Combine(m_RegistryPaths.ProgramPath, "Data", "Help", _strHelpFilename);
		m_RevitWindowWrapper = new WindowWrapper(_mainWindowHandle);
		m_strInstallLocation = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
		m_strRevitIniFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "Revit", versionName, "Revit.ini");
		SetLanguage(language);
		m_StringTableModule = new StringTableModule(_RevitProduct);
		RuntimeDiagnostics.Write("Initialized Revit " + m_strRevitVersion + "; content=" + m_RegistryPaths.ProgramPath
			+ "; assembly=" + typeof(ApplicationUtility).Assembly.Location + "; build=" + typeof(ApplicationUtility).Module.ModuleVersionId);
		return true;
	}

	private static void SetLanguage(LanguageType language)
	{
		switch (language)
		{
		case LanguageType.Chinese_Simplified:
			m_RevitLanguangeType = RevitLanguageType.ChineseSimplified;
			m_strThreeLetterWindowsLanguageName = "CHS";
			break;
		case LanguageType.Chinese_Traditional:
			m_RevitLanguangeType = RevitLanguageType.ChineseTraditional;
			m_strThreeLetterWindowsLanguageName = "CHT";
			break;
		case LanguageType.Dutch:
			m_RevitLanguangeType = RevitLanguageType.Dutch;
			m_strThreeLetterWindowsLanguageName = "NLD";
			break;
		case LanguageType.English_USA:
			m_RevitLanguangeType = RevitLanguageType.English;
			m_strThreeLetterWindowsLanguageName = "ENU";
			break;
		case LanguageType.French:
			m_RevitLanguangeType = RevitLanguageType.French;
			m_strThreeLetterWindowsLanguageName = "FRA";
			break;
		case LanguageType.German:
			m_RevitLanguangeType = RevitLanguageType.German;
			m_strThreeLetterWindowsLanguageName = "DEU";
			break;
		case LanguageType.Italian:
			m_RevitLanguangeType = RevitLanguageType.Italian;
			m_strThreeLetterWindowsLanguageName = "ITA";
			break;
		case LanguageType.Japanese:
			m_RevitLanguangeType = RevitLanguageType.Japanese;
			m_strThreeLetterWindowsLanguageName = "JPN";
			break;
		case LanguageType.Korean:
			m_RevitLanguangeType = RevitLanguageType.Korean;
			m_strThreeLetterWindowsLanguageName = "KOR";
			break;
		case LanguageType.Russian:
			m_RevitLanguangeType = RevitLanguageType.Russian;
			m_strThreeLetterWindowsLanguageName = "RUS";
			break;
		case LanguageType.Spanish:
			m_RevitLanguangeType = RevitLanguageType.Spanish;
			m_strThreeLetterWindowsLanguageName = "ESP";
			break;
		case LanguageType.Czech:
			m_RevitLanguangeType = RevitLanguageType.Czech;
			m_strThreeLetterWindowsLanguageName = "CSY";
			break;
		case LanguageType.Hungarian:
			m_RevitLanguangeType = RevitLanguageType.Hungarian;
			m_strThreeLetterWindowsLanguageName = "HUN";
			break;
		case LanguageType.Polish:
			m_RevitLanguangeType = RevitLanguageType.Polish;
			m_strThreeLetterWindowsLanguageName = "PLK";
			break;
		case LanguageType.Brazilian_Portuguese:
			m_RevitLanguangeType = RevitLanguageType.Portuguese;
			m_strThreeLetterWindowsLanguageName = "PTG";
			break;
		case LanguageType.Unknown:
		{
			bool flag = false;
			if (m_Settings.eplSetE_GetSetting("LanguageName", ref m_strThreeLetterWindowsLanguageName))
			{
				flag = true;
			}
			else if (m_Settings.eplSetE_GetSetting("LanguageName", ref m_strThreeLetterWindowsLanguageName, m_strRevitIniFilename))
			{
				flag = true;
			}
			if (!flag && !RevitUtility.FindLanguageName(ref m_strThreeLetterWindowsLanguageName) && !string.IsNullOrEmpty(m_strThreeLetterWindowsLanguageName))
			{
				List<string> list = new List<string>();
				list.Add("Czech");
				list.Add("Hungarian");
				list.Add("Polish");
				list.Add("Portuguese");
				int nDefaultOption = 0;
				if (string.Compare(CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName, "CSY", ignoreCase: true) == 0)
				{
					nDefaultOption = 0;
				}
				else if (string.Compare(CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName, "HUN", ignoreCase: true) == 0)
				{
					nDefaultOption = 1;
				}
				else if (string.Compare(CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName, "PLK", ignoreCase: true) == 0)
				{
					nDefaultOption = 2;
				}
				else if (string.Compare(CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName, "PTG", ignoreCase: true) == 0 || string.Compare(CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName, "PTB", ignoreCase: true) == 0)
				{
					nDefaultOption = 3;
				}
				WarningOptionsForm warningOptionsForm = new WarningOptionsForm("Warning", string.Empty, "LanguageName", "Please pick the language for your version of Revit.", list, nDefaultOption, _bDisplayOKCancel: false, _bDisplayDoNotShow: false);
				warningOptionsForm.ShowDialog();
				if (warningOptionsForm.DialogResult == DialogResult.OK)
				{
					if (string.Compare(warningOptionsForm.WarningOption, "Czech", ignoreCase: true) == 0)
					{
						m_strThreeLetterWindowsLanguageName = "CSY";
					}
					else if (string.Compare(warningOptionsForm.WarningOption, "Hungarian", ignoreCase: true) == 0)
					{
						m_strThreeLetterWindowsLanguageName = "HUN";
					}
					else if (string.Compare(warningOptionsForm.WarningOption, "Polish", ignoreCase: true) == 0)
					{
						m_strThreeLetterWindowsLanguageName = "PLK";
					}
					else if (string.Compare(warningOptionsForm.WarningOption, "Portuguese", ignoreCase: true) == 0)
					{
						m_strThreeLetterWindowsLanguageName = "PTG";
					}
				}
				m_Settings.eplSetE_SetSetting("LanguageName", m_strThreeLetterWindowsLanguageName);
			}
			if (string.Compare(m_strThreeLetterWindowsLanguageName, "CSY", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.Czech;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "HUN", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.Hungarian;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "PLK", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.Polish;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "PTG", ignoreCase: true) == 0 || string.Compare(m_strThreeLetterWindowsLanguageName, "PTB", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.Portuguese;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "CHS", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.ChineseSimplified;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "CHT", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.ChineseTraditional;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "NLD", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.Dutch;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "ENU", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.English;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "FRA", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.French;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "DEU", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.German;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "FRA", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.French;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "ITA", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.Italian;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "JPN", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.Japanese;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "KOR", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.Korean;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "RUS", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.Russian;
			}
			else if (string.Compare(m_strThreeLetterWindowsLanguageName, "ESP", ignoreCase: true) == 0)
			{
				m_RevitLanguangeType = RevitLanguageType.Spanish;
			}
			break;
		}
		}
	}

	public static string GetString(string _strValue)
	{
		if (m_StringTableModule != null)
		{
			return m_StringTableModule.GetString(_strValue);
		}
		return string.Empty;
	}

	public static string GetString(bool _bUseWindowsLanguageName, string _strValue)
	{
		if (_bUseWindowsLanguageName)
		{
			return m_StringTableModule.GetString(m_strThreeLetterWindowsLanguageName, _strValue);
		}
		return m_StringTableModule.GetString(_strValue);
	}

	public static void StartWebClient()
	{
		try
		{
			Process.Start(string.Format("ftp://epuserweblinks:3pUser@ftp.eaglepoint.com/Downloads/Siteworks for Revit {0}/SiteworksforRevit{0}.zip", RevitVersion));
		}
		catch (Exception)
		{
			if (!EPShared.eplLicM_IsNodeLicense())
			{
				RevitMessageBox.Show(GetString("ConnectionCouldNotBeMadeContactITAdminMessage"), GetString("Error"), MessageBoxIcon.Hand);
			}
			else
			{
				RevitMessageBox.Show(GetString("ConnectionCouldNotBeMadeMessage"), GetString("Error"), MessageBoxIcon.Hand);
			}
		}
	}

	public static void VerifyProductVersion()
	{
		VerifyProductVersion(_bDailyVersionCheck: true);
	}

	public static void VerifyProductVersion(bool _bDailyVersionCheck)
	{
		try
		{
			ProductUpdate.Check(_bDailyVersionCheck);
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("VerifyProductVersion: " + ex.Message);
		}
	}
}
