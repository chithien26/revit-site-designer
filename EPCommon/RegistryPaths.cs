namespace EPCommon;

public class RegistryPaths
{
	private const string m_strSupportPath = "\\Data\\Support\\";

	private string m_strProgramPath;

	private const string m_strHelpPath = "\\Data\\Help\\";

	private const string m_strLibraryPath = "\\Data\\Library\\";

	private const string m_strSamplePath = "\\Data\\Samples\\";

	public string SupportPath => "\\Data\\Support\\";

	public string ProgramPath
	{
		get
		{
			return m_strProgramPath;
		}
		set
		{
			m_strProgramPath = value;
		}
	}

	public int GetSupportPath(string _strFileName, ref string _strSupportPath)
	{
		int num = 6;
		_strSupportPath = m_strProgramPath + "\\Data\\Support\\";
		if (!_strSupportPath.EndsWith("\\") && !_strSupportPath.EndsWith("/"))
		{
			_strSupportPath += "\\";
		}
		_strSupportPath += _strFileName;
		return 0;
	}

	public int GetProgramPath(string _strFileName, ref string _strProgramPath)
	{
		int num = 6;
		_strProgramPath = m_strProgramPath;
		if (!_strProgramPath.EndsWith("\\") && !_strProgramPath.EndsWith("/"))
		{
			_strProgramPath += "\\";
		}
		_strProgramPath += _strFileName;
		return 0;
	}

	public int GetHelpPath(string _strFileName, ref string _strHelpPath)
	{
		int num = 6;
		_strHelpPath = m_strProgramPath + "\\Data\\Help\\";
		if (!_strHelpPath.EndsWith("\\") && !_strHelpPath.EndsWith("/"))
		{
			_strHelpPath += "\\";
		}
		_strHelpPath += _strFileName;
		return 0;
	}

	public int GetLibraryPath(string _strFileName, ref string _strLibraryPath)
	{
		int num = 6;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("LibraryPath", ref _strLibraryPath) && _strLibraryPath != string.Empty)
		{
			if (!_strLibraryPath.EndsWith("\\") && !_strLibraryPath.EndsWith("/"))
			{
				_strLibraryPath += "\\";
			}
			_strLibraryPath += _strFileName;
			return 0;
		}
		_strLibraryPath = m_strProgramPath + "\\Data\\Library\\";
		if (!_strLibraryPath.EndsWith("\\") && !_strLibraryPath.EndsWith("/"))
		{
			_strLibraryPath += "\\";
		}
		_strLibraryPath += _strFileName;
		return 0;
	}

	public int GetSamplePath(string _strFileName, ref string _strSamplePath)
	{
		int num = 6;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("SamplePath", ref _strSamplePath) && _strSamplePath != string.Empty)
		{
			if (!_strSamplePath.EndsWith("\\") && !_strSamplePath.EndsWith("/"))
			{
				_strSamplePath += "\\";
			}
			_strSamplePath += _strFileName;
			return 0;
		}
		_strSamplePath = m_strProgramPath + "\\Data\\Samples\\";
		if (!_strSamplePath.EndsWith("\\") && !_strSamplePath.EndsWith("/"))
		{
			_strSamplePath += "\\";
		}
		_strSamplePath += _strFileName;
		return 0;
	}
}
