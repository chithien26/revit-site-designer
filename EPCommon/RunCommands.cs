using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Forms;
using Microsoft.Win32;

namespace EPCommon;

public class RunCommands
{
	public delegate int PinnacleCallBack(int _nCommandID);

	public enum ApplicationInstalled
	{
		NONE = -1,
		REVIT_INSTALLED = 1
	}

	private class Externalx86
	{
		[DllImport("PSAPICLIENTx86.DLL", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public static extern IntPtr Pinnacle_RegisterClient([MarshalAs(UnmanagedType.LPWStr)] string _strDisplay, [MarshalAs(UnmanagedType.LPWStr)] string _strClientCode, PinnacleCallBack _ExecutePinnacleCommand);

		[DllImport("PSAPICLIENTx86.DLL", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public static extern int Pinnacle_AddCommand(IntPtr _Handle, [MarshalAs(UnmanagedType.LPWStr)] string _strCmdName, int nCmdID);

		[DllImport("PSAPICLIENTx86.DLL", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public static extern void Pinnacle_UnregisterClient(IntPtr _Handle);
	}

	private class Externalx64
	{
		[DllImport("PSAPICLIENTx64.DLL", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public static extern IntPtr Pinnacle_RegisterClient([MarshalAs(UnmanagedType.LPWStr)] string _strDisplay, [MarshalAs(UnmanagedType.LPWStr)] string _strClientCode, PinnacleCallBack _ExecutePinnacleCommand);

		[DllImport("PSAPICLIENTx64.DLL", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public static extern int Pinnacle_AddCommand(IntPtr _Handle, [MarshalAs(UnmanagedType.LPWStr)] string _strCmdName, int nCmdID);

		[DllImport("PSAPICLIENTx64.DLL", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public static extern void Pinnacle_UnregisterClient(IntPtr _Handle);
	}

	private AutomationElement m_RootElement;

	private Dictionary<int, string> m_CatalogForPinnacle = new Dictionary<int, string>();

	private Dictionary<int, string> m_CatalogForRevit = new Dictionary<int, string>();

	private PinnacleCallBack m_PinnacleExecuteCommandDelegate;

	private ApplicationInstalled m_ApplicationInstalled = ApplicationInstalled.NONE;

	private bool m_bUseTimer;

	private Timer m_WaitTimer = new Timer();

	private int m_nCommandID = -1;

	private IntPtr m_PinnacleClientHandle = IntPtr.Zero;

	private bool m_bPinnacleInstalled;

	private string m_strPreviousPath = string.Empty;

	public void epl_SetRevitVersion(RevitVersion _RevitVersion, bool _bIs64BitInstall)
	{
		if (Process.GetCurrentProcess().MainModule.FileName.IndexOf("Architecture") > 0 || Process.GetCurrentProcess().MainModule.FileName.IndexOf("Structure") > 0 || Process.GetCurrentProcess().MainModule.FileName.IndexOf("MEP") > 0)
		{
			m_ApplicationInstalled = ApplicationInstalled.REVIT_INSTALLED;
		}
		else if (Process.GetCurrentProcess().MainModule.FileName.IndexOf("Revit") > 0)
		{
			m_ApplicationInstalled = ApplicationInstalled.REVIT_INSTALLED;
		}
		m_CatalogForRevit.Clear();
		m_CatalogForPinnacle.Clear();
		GetRootElement();
		ParseCatalogFile(_bIs64BitInstall);
		m_PinnacleExecuteCommandDelegate = InvokePinnacleCmd;
		if (m_bPinnacleInstalled)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			empty = $"Sitesworks for Revit {ApplicationUtility.RevitVersion}";
			empty2 = $"EGPTSWREVIT{ApplicationUtility.RevitVersion}";
			try
			{
				if (!_bIs64BitInstall)
				{
					m_PinnacleClientHandle = Externalx86.Pinnacle_RegisterClient(empty, empty2, m_PinnacleExecuteCommandDelegate);
					foreach (KeyValuePair<int, string> item in m_CatalogForPinnacle)
					{
						Externalx86.Pinnacle_AddCommand(m_PinnacleClientHandle, item.Value, item.Key);
					}
				}
				else
				{
					m_PinnacleClientHandle = Externalx64.Pinnacle_RegisterClient(empty, empty2, m_PinnacleExecuteCommandDelegate);
					foreach (KeyValuePair<int, string> item2 in m_CatalogForPinnacle)
					{
						Externalx64.Pinnacle_AddCommand(m_PinnacleClientHandle, item2.Value, item2.Key);
					}
				}
			}
			catch (Exception)
			{
			}
		}
		m_WaitTimer.Interval = 2000;
		m_WaitTimer.Tick += WaitTimerCallBack;
	}

	private void WaitTimerCallBack(object sender, EventArgs e)
	{
		m_WaitTimer.Stop();
		m_bUseTimer = false;
		InvokePinnacleCmd(m_nCommandID);
		m_nCommandID = -1;
	}

	private void ParseCatalogFile(bool _bIs64BitInstall)
	{
		try
		{
			m_strPreviousPath = Environment.GetEnvironmentVariable("PATH");
			string text = string.Empty;
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Eagle Point Software\\Pinnacle Series\\Pinnacle", writable: false);
			if (registryKey != null && registryKey != null)
			{
				object value = registryKey.GetValue("ProgramPath");
				if (value != null)
				{
					text = value.ToString();
				}
				registryKey.Close();
			}
			if (text == string.Empty)
			{
				registryKey = Registry.LocalMachine.OpenSubKey("Software\\Eagle Point Software\\Pinnacle Series", writable: false);
				if (registryKey != null)
				{
					object value2 = registryKey.GetValue("InstallLocation");
					if (value2 != null)
					{
						text = value2.ToString();
					}
					registryKey.Close();
				}
			}
			if (text != string.Empty)
			{
				if (!text.EndsWith("\\"))
				{
					text += "\\";
				}
				Environment.SetEnvironmentVariable("PATH", m_strPreviousPath + ";" + text);
				IsPinnacleInstalled(_bIs64BitInstall, text);
			}
			string empty = string.Empty;
			string _strSupportPath = string.Empty;
			if (m_ApplicationInstalled == ApplicationInstalled.REVIT_INSTALLED)
			{
				empty = "SiteDesignerForRevit.clg";
				EPShared.eplFilP_GetSupportPath(empty, ref _strSupportPath);
			}
			if (!File.Exists(_strSupportPath))
			{
				return;
			}
			using StreamReader streamReader = new StreamReader(_strSupportPath);
			string text2;
			while ((text2 = streamReader.ReadLine()) != null)
			{
				if (text2.StartsWith("#") || text2 == string.Empty)
				{
					continue;
				}
				string[] array = text2.Split(';');
				if (array.Length > 2)
				{
					m_CatalogForPinnacle.Add(Convert.ToInt16(array[0]), array[1]);
					string text3 = array[2];
					if (text3.IndexOf("FlyoutButtonExecute") > 0)
					{
						text3.Replace("FlyoutButtonExecute", "DropDownButtonBuddy");
					}
					m_CatalogForRevit.Add(Convert.ToInt16(array[0]), text3);
				}
			}
		}
		catch (Exception)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("CatalogFilesMissing"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
		}
	}

	private void IsPinnacleInstalled(bool _bIs64BitInstall, string _strPinnacleLocation)
	{
		string text = _strPinnacleLocation;
		text = (_bIs64BitInstall ? (text + "PSAPICLIENTx64.dll") : (text + "PSAPICLIENTx86.dll"));
		if (!File.Exists(text))
		{
			return;
		}
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
		folderPath += "\\uiautomationcore.dll";
		if (File.Exists(folderPath))
		{
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(folderPath);
			if (versionInfo.FileMajorPart >= 7)
			{
				m_bPinnacleInstalled = true;
			}
		}
	}

	private void GetRootElement()
	{
		m_RootElement = FindElementByProcessId(Process.GetCurrentProcess().Id);
	}

	private int InvokePinnacleCmd(int _nCommandID)
	{
		if (m_CatalogForPinnacle.ContainsKey(_nCommandID))
		{
			InvokeCommand(m_CatalogForRevit[_nCommandID]);
			if (m_bUseTimer)
			{
				m_nCommandID = _nCommandID;
				m_WaitTimer.Start();
			}
		}
		return 0;
	}

	private void InvokeCommand(string _strElementName)
	{
		if (!(_strElementName != string.Empty))
		{
			return;
		}
		AutomationElement automationElement = null;
		string[] array = _strElementName.Split('+');
		int num = 0;
		string empty = string.Empty;
		if (m_RootElement == null)
		{
			return;
		}
		PropertyCondition propertyCondition = null;
		while (num < array.Length)
		{
			empty = array[num];
			propertyCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, empty);
			automationElement = m_RootElement.FindFirst(TreeScope.Descendants, propertyCondition);
			if (automationElement == null)
			{
				if (num > 1)
				{
					m_bUseTimer = true;
				}
				num++;
				continue;
			}
			if (automationElement != null)
			{
				AutomationPattern[] supportedPatterns = automationElement.GetSupportedPatterns();
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				AutomationPattern[] array2 = supportedPatterns;
				foreach (AutomationPattern pattern in array2)
				{
					string text = automationElement.GetCurrentPattern(pattern).ToString();
					if (text.IndexOf("TogglePattern") > 0)
					{
						flag = true;
						continue;
					}
					if (text.IndexOf("InvokePattern") > 0)
					{
						flag2 = true;
						break;
					}
					if (text.IndexOf("SelectionItemPattern") > 0)
					{
						flag3 = true;
						break;
					}
				}
				try
				{
					if (flag2)
					{
						if (automationElement.GetCurrentPattern(InvokePattern.Pattern) is InvokePattern invokePattern)
						{
							invokePattern.Invoke();
						}
					}
					else if (flag)
					{
						if (automationElement.GetCurrentPattern(TogglePattern.Pattern) is TogglePattern togglePattern)
						{
							togglePattern.Toggle();
						}
					}
					else if (flag3 && automationElement.GetCurrentPattern(SelectionItemPattern.Pattern) is SelectionItemPattern selectionItemPattern)
					{
						selectionItemPattern.Select();
						PropertyCondition propertyCondition2 = new PropertyCondition(AutomationElement.AutomationIdProperty, empty);
						PropertyCondition propertyCondition3 = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button);
						AndCondition condition = new AndCondition(propertyCondition2, propertyCondition3);
						AutomationElement automationElement2 = m_RootElement.FindFirst(TreeScope.Subtree, condition);
						if (automationElement2 != null)
						{
							InvokePattern invokePattern2 = automationElement2.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
							invokePattern2.Invoke();
						}
					}
				}
				catch (Exception)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("CommandInvokeFailed"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
				}
			}
			num++;
		}
	}

	private AutomationElement FindElementByProcessId(int pid)
	{
		AutomationElement automationElement = null;
		PropertyCondition[] array = new PropertyCondition[2]
		{
			new PropertyCondition(AutomationElement.ProcessIdProperty, pid),
			new PropertyCondition(AutomationElement.IsKeyboardFocusableProperty, true)
		};
		Condition[] conditions = array;
		AndCondition andCondition = new AndCondition(conditions);
		AutomationElementCollection automationElementCollection = AutomationElement.RootElement.FindAll(TreeScope.Children, array[0]);
		if (automationElementCollection.Count == 0)
		{
			return null;
		}
		if (automationElementCollection.Count == 1)
		{
			return automationElementCollection[0];
		}
		foreach (AutomationElement item in automationElementCollection)
		{
			if (item.Current.Name.Contains("Revit"))
			{
				automationElement = item;
			}
		}
		if (automationElement == null)
		{
			automationElement = automationElementCollection[0];
		}
		return automationElement;
	}

	public void epl_UnloadHelper(bool _bIs64BitInstall)
	{
		if (m_bPinnacleInstalled)
		{
			if (!_bIs64BitInstall)
			{
				Externalx86.Pinnacle_UnregisterClient(m_PinnacleClientHandle);
			}
			else
			{
				Externalx64.Pinnacle_UnregisterClient(m_PinnacleClientHandle);
			}
		}
		EPShared.epl_UnloadHelper(_bIs64BitInstall);
		Environment.SetEnvironmentVariable("PATH", m_strPreviousPath);
	}
}
