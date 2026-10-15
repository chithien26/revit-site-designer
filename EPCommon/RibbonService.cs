using System;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using UIFramework;

namespace EPCommon;

public static class RibbonService
{
	private static readonly RibbonControl m_RibbonControl;

	static RibbonService()
	{
		try
		{
			m_RibbonControl = RevitRibbonControl.RibbonControl;
		}
		catch (TypeInitializationException)
		{
			m_RibbonControl = null;
		}
	}

	public static RibbonTab CreateTab(string _strID, string _strTitle, bool _bIsVisible, int _nLandcaddRibbonIndex)
	{
		if (m_RibbonControl == null)
		{
			return null;
		}
		RibbonTab ribbonTab = new RibbonTab
		{
			Id = _strID,
			Title = _strTitle,
			IsVisible = _bIsVisible
		};
		if (_nLandcaddRibbonIndex == -1)
		{
			int num = 0;
			RibbonTab ribbonTab2 = null;
			for (num = 0; num < m_RibbonControl.Tabs.Count; num++)
			{
				ribbonTab2 = m_RibbonControl.Tabs[num];
				if (string.Compare(ribbonTab2.Id, EPShared.LocalRM.GetString("AddInsRibbonTabIDValue"), ignoreCase: true) == 0)
				{
					_nLandcaddRibbonIndex = num + 1;
					break;
				}
			}
		}
		if (_nLandcaddRibbonIndex >= 0)
		{
			m_RibbonControl.Tabs.Insert(_nLandcaddRibbonIndex, ribbonTab);
		}
		return ribbonTab;
	}

	public static Autodesk.Revit.UI.RibbonPanel CreatePanel(RibbonTab _RibbonTab, string _strPanelTitle)
	{
		try
		{
			if (_RibbonTab == null)
			{
				throw new ArgumentNullException("ribbontab", "RibbonTab cannot be null");
			}
			if (string.IsNullOrEmpty(_strPanelTitle))
			{
				throw new ArgumentException("Panel title cannot be empty");
			}
			RibbonPanelSource source = new RibbonPanelSource();
			Autodesk.Windows.RibbonPanel ribbonPanel = new Autodesk.Windows.RibbonPanel
			{
				Source = source
			};
			Autodesk.Revit.UI.RibbonPanel ribbonPanel2 = UIPanel(ribbonPanel, _RibbonTab.Title);
			if (ribbonPanel2 != null)
			{
				ribbonPanel.Source.Name = _strPanelTitle;
				ribbonPanel.Source.Title = _strPanelTitle;
				ribbonPanel.Source.Id = "CustomCtrl_" + _strPanelTitle;
				_RibbonTab.Panels.Add(ribbonPanel);
			}
			return ribbonPanel2;
		}
		catch (Exception innerException)
		{
			throw new Exception("Error creating panel :", innerException);
		}
	}

	private static Autodesk.Revit.UI.RibbonPanel UIPanel(Autodesk.Windows.RibbonPanel _Panel, string _strParentID)
	{
		try
		{
			Autodesk.Revit.UI.RibbonPanel result = null;
			Type typeFromHandle = typeof(Autodesk.Revit.UI.RibbonPanel);
			ConstructorInfo[] constructors = typeFromHandle.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
			if (constructors != null && constructors.Length != 0)
			{
				ConstructorInfo constructorInfo = constructors[0];
				if (constructorInfo != null)
				{
					if (constructorInfo.GetParameters().Length == 2)
					{
						result = constructorInfo.Invoke(new object[2] { _Panel, _strParentID }) as Autodesk.Revit.UI.RibbonPanel;
					}
					else if (constructorInfo.GetParameters().Length == 1)
					{
						result = constructorInfo.Invoke(new object[1] { _Panel }) as Autodesk.Revit.UI.RibbonPanel;
					}
				}
			}
			else
			{
				ConstructorInfo constructor = typeFromHandle.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[1] { typeof(Autodesk.Windows.RibbonPanel) }, null);
				if (constructor != null)
				{
					result = constructor.Invoke(new object[1] { _Panel }) as Autodesk.Revit.UI.RibbonPanel;
				}
			}
			return result;
		}
		catch (Exception innerException)
		{
			throw new Exception("Error creating Revit ribbon panel :", innerException);
		}
	}

	public static List<KeyValuePair<string, bool>> GetRibbonVisibiltyStates()
	{
		List<KeyValuePair<string, bool>> list = new List<KeyValuePair<string, bool>>();
		for (int i = 0; i < m_RibbonControl.Tabs.Count; i++)
		{
			list.Add(new KeyValuePair<string, bool>(m_RibbonControl.Tabs[i].Id, m_RibbonControl.Tabs[i].IsVisible));
		}
		return list;
	}

	public static void SetRibbonVisibiltyStates(List<KeyValuePair<string, bool>> _RibbonList)
	{
		if (_RibbonList == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < m_RibbonControl.Tabs.Count; i++)
		{
			for (num = 0; num < _RibbonList.Count; num++)
			{
				if (string.Compare(m_RibbonControl.Tabs[i].Id, _RibbonList[num].Key, ignoreCase: true) == 0)
				{
					m_RibbonControl.Tabs[i].IsVisible = _RibbonList[num].Value;
					_RibbonList.RemoveAt(num);
					break;
				}
			}
		}
	}
}
