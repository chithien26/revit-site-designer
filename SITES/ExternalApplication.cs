using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Windows;
using EPCommon;
using SITES.Sites_Core;
using UIFramework;

namespace SITES;

[Regeneration(RegenerationOption.Manual)]
[Journaling(JournalingMode.NoCommandData)]
public class ExternalApplication : IExternalApplication
{
	private static RunCommands m_RunCommands = null;

	private const string m_strSitesINIFilename = "SitesRevit.ini";

	private const string m_strSitesHelpFilename = "Site Designer for Revit.chm";

	private static string m_strCurUserSitesRegistryKey = $"Software\\Autodesk\\Siteworks for Revit\\Siteworks for Revit {ApplicationUtility.RevitVersion}";

	private DocumentViewContainerList m_DocumentViewContainerList;

	private void DocumentExitHandler(object sender, DocumentSavingEventArgs args)
	{
	}

	private void DocumentExitHandler(object sender, DocumentSavingAsEventArgs args)
	{
		Document document = args.Document;
		if (document == null || document.IsFamilyDocument || RevitUtility.CompareFileNames(args.PathName, document.PathName) == 0)
		{
			return;
		}
		List<KeyValuePair<string, string>> list = ApplicationUtility.Settings.eplSetE_GetSectionValuesAsList(document.PathName);
		if (list == null || list.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<string, string> item in list)
		{
			ApplicationUtility.Settings.eplSetE_SetSettingWithAbbreviation(args.PathName, item.Key, item.Value);
		}
	}

	private void DocumentExitHandler(object sender, DocumentPrintingEventArgs args)
	{
	}

	private void ViewActivatedHandler(object sender, ViewActivatedEventArgs args)
	{
		if (AddLandcaddRibbonIfNotPresent() && !args.Document.IsFamilyDocument)
		{
			int num = m_DocumentViewContainerList.FindIndexDocument(args.Document);
			if (num == -1)
			{
				Document document = args.Document;
				AddDocumentViewContainerToList(args.Document);
				num = m_DocumentViewContainerList.FindIndexDocument(args.Document);
			}
			if (num >= 0 && !m_DocumentViewContainerList.ContainsView(num, args.CurrentActiveView))
			{
				RevitUtility.HideAllSiteComponents(args.Document, args.CurrentActiveView);
				m_DocumentViewContainerList.DocumentViewList[num].ViewList.Add(args.CurrentActiveView);
			}
		}
	}

	private void AddDocumentViewContainerToList(Document _Document)
	{
		if (_Document.IsFamilyDocument || m_DocumentViewContainerList.FindIndexDocument(_Document) != -1)
		{
			return;
		}
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list = filteredElementCollector.OfClass(typeof(View)).ToElements();
		List<View> list2 = new List<View>();
		bool flag = false;
		foreach (View item in list)
		{
			for (int i = 0; i < list2.Count; i++)
			{
				if (string.Compare(list2[i].Name, item.Name, ignoreCase: true) == 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list2.Add(item);
			}
			else
			{
				flag = false;
			}
		}
		m_DocumentViewContainerList.DocumentViewList.Add(new DocumentViewContainer(_Document, list2));
	}

	private void DocumentCreatedHandler(object sender, DocumentCreatedEventArgs args)
	{
		Document document = args.Document;
		if (document != null)
		{
			AddDocumentViewContainerToList(document);
		}
	}

	private void DocumentOpenedHandler(object sender, DocumentOpenedEventArgs args)
	{
		Document document = args.Document;
		if (document != null)
		{
			AddDocumentViewContainerToList(document);
		}
	}

	private void DocumentClosingHandler(object sender, DocumentClosingEventArgs args)
	{
		try
		{
			Document document = args.Document;
			if (document != null && !document.IsFamilyDocument)
			{
				ApplicationUtility.Settings.eplSetE_UpdateIniInformation(document.PathName);
				int num = m_DocumentViewContainerList.FindIndexDocument(document);
				if (num >= 0 && num < m_DocumentViewContainerList.DocumentViewList.Count)
				{
					m_DocumentViewContainerList.DocumentViewList.RemoveAt(num);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private void ApplicationClosingHandler(object sender, ApplicationClosingEventArgs args)
	{
		m_RunCommands.epl_UnloadHelper(ApplicationUtility.Is64BitInstall);
		EPShared.eplLicM_ReturnLicense();
	}

	private void DialogBoxShowingHandler(object sender, DialogBoxShowingEventArgs args)
	{
		Type type = args.GetType();
		if (string.Compare(type.Name, "TaskDialogShowingEventArgs", ignoreCase: true) == 0)
		{
			TaskDialogShowingEventArgs e = (TaskDialogShowingEventArgs)args;
			if (e != null && string.Compare(e.DialogId, "TaskDialog_Missing_Third_Party_Updater", ignoreCase: true) == 0 && e.Message.Contains("LANDCADDApplication : Symbol Placement Updater"))
			{
				e.OverrideResult(1001);
			}
		}
	}

	public Result OnStartup(UIControlledApplication application)
	{
		RuntimeDiagnostics.IsRevitHost = true;
		try
		{
		IntPtr intPtr = default;
		intPtr = application.MainWindowHandle;
		if (!ApplicationUtility.Initialize(application.ControlledApplication, m_strCurUserSitesRegistryKey, "SitesRevit.ini", "Site Designer for Revit.chm", intPtr, RevitProduct.SITES))
		{
			return Result.Failed;
		}
		m_RunCommands = new RunCommands();
		m_DocumentViewContainerList = new DocumentViewContainerList();
		m_RunCommands.epl_SetRevitVersion(RevitVersion.Revit2011, ApplicationUtility.Is64BitInstall);
		application.ControlledApplication.DocumentOpened += DocumentOpenedHandler;
		application.ControlledApplication.DocumentSaving += DocumentExitHandler;
		application.ControlledApplication.DocumentPrinting += DocumentExitHandler;
		application.ControlledApplication.DocumentSavingAs += DocumentExitHandler;
		application.ControlledApplication.DocumentClosing += DocumentClosingHandler;
		application.ControlledApplication.DocumentCreated += DocumentCreatedHandler;
		application.DialogBoxShowing += DialogBoxShowingHandler;
		application.ViewActivated += ViewActivatedHandler;
		application.ApplicationClosing += ApplicationClosingHandler;
		AddRibbonItems(application);
		return Result.Succeeded;
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Report("Site Designer startup", ex);
			return Result.Failed;
		}
	}

	private void SetTabsRight()
	{
		RibbonControl ribbonControl = null;
		try
		{
			ribbonControl = RevitRibbonControl.RibbonControl;
		}
		catch (TypeInitializationException)
		{
			return;
		}
		RibbonTab ribbonTab = null;
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		for (int i = 0; i < ribbonControl.Tabs.Count; i++)
		{
			ribbonTab = ribbonControl.Tabs[i];
			if (string.Compare(ribbonTab.Id, EPShared.LocalRM.GetString("LandcaddSitesRibbonTab"), ignoreCase: true) == 0)
			{
				num = i;
			}
			else if (string.Compare(ribbonTab.Id, EPShared.LocalRM.GetString("LandcaddRibbonTab"), ignoreCase: true) == 0)
			{
				num2 = i;
			}
			else if (string.Compare(ribbonTab.Id, "Add-Ins", ignoreCase: true) == 0)
			{
				num3 = i;
			}
		}
		if (num3 != -1)
		{
			if (num != -1)
			{
				ribbonControl.Tabs.Move(num, num3 + 1);
			}
			if (num2 != -1)
			{
				ribbonControl.Tabs.Move(num, num3 + 2);
			}
		}
		else if (num != -1 && num2 != -1 && num2 > num)
		{
			ribbonControl.Tabs.Move(num, num2);
		}
	}

	private void AddRibbonItems(UIControlledApplication application)
	{
		CreateSitesRibbon(application);
	}

	private void CreateSitesRibbon(UIControlledApplication application)
	{
		CreateSitesRibbon(application, -1);
	}

	private void CreateSitesRibbon(UIControlledApplication application, int _nLandcaddRibbonIndex)
	{
		string location = RuntimePaths.ManagedAssemblyPath;
		application.CreateRibbonTab(EPShared.LocalRM.GetString("LandcaddSitesRibbonTab"));
		Autodesk.Revit.UI.RibbonPanel ribbonPanel = null;
		ribbonPanel = application.CreateRibbonPanel(EPShared.LocalRM.GetString("LandcaddSitesRibbonTab"), EPShared.LocalRM.GetString("ImportExportRibbonPanel"));
		if (ribbonPanel == null)
		{
			return;
		}
		PushButtonData pushButtonData = new PushButtonData(EPShared.LocalRM.GetString("ImportLandXMLRibbonButton"), EPShared.LocalRM.GetString("ImportLandXMLRibbonButton"), location, typeof(ImportLandXMLCommand).FullName);
		pushButtonData.ToolTip = EPShared.LocalRM.GetString("ImportLandXMLRibbonToolTip");
		pushButtonData.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDimportLandXML"));
		pushButtonData.Image = EPCommon.SafeResources.ToImageSource(Resources.ImportLandXML);
		PushButtonData pushButtonData2 = new PushButtonData(EPShared.LocalRM.GetString("ExportLandXMLRibbonButton"), EPShared.LocalRM.GetString("ExportLandXMLRibbonButton"), location, typeof(ExportLandXMLCommand).FullName);
		pushButtonData2.ToolTip = EPShared.LocalRM.GetString("ExportLandXMLRibbonToolTip");
		pushButtonData2.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDexportLandXML"));
		pushButtonData2.Image = EPCommon.SafeResources.ToImageSource(Resources.ExportLandXML);
		ribbonPanel.AddStackedItems(pushButtonData, pushButtonData2);
		ribbonPanel = application.CreateRibbonPanel(EPShared.LocalRM.GetString("LandcaddSitesRibbonTab"), EPShared.LocalRM.GetString("ConvertRibbonPanel"));
		PushButtonData pushButtonData3 = new PushButtonData(EPShared.LocalRM.GetString("SetBaseTopoSurfaceRibbonButton"), TerrainText.ForDisplay(EPShared.LocalRM.GetString("SetBaseTopoSurfaceRibbonButton")), location, typeof(SetBaseToposurfaceCommand).FullName);
		pushButtonData3.ToolTip = TerrainText.ForDisplay(EPShared.LocalRM.GetString("SetBaseTopoSurfaceRibbonToolTip"));
		pushButtonData3.LongDescription = TerrainText.ForDisplay(EPShared.LocalRM.GetString("SetBaseTopoSurfaceRibbonToolTipLongDescription"));
		pushButtonData3.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDsetBaseTopo"));
		pushButtonData3.Image = EPCommon.SafeResources.ToImageSource(Resources.SetBaseToposurface);
		PushButtonData pushButtonData4 = new PushButtonData(EPShared.LocalRM.GetString("ToposurfaceConversionRibbonButton"), TerrainText.ForDisplay(EPShared.LocalRM.GetString("ToposurfaceConversionRibbonButton")), location, typeof(ConvertObjectToToposurfaceCommand).FullName);
		pushButtonData4.ToolTip = TerrainText.ForDisplay(EPShared.LocalRM.GetString("ConvertObjectToToposurfaceRibbonToolTip"));
		pushButtonData4.LongDescription = TerrainText.ForDisplay(EPShared.LocalRM.GetString("ConvertObjectToToposurfaceRibbonToolTipLongDescription"));
		pushButtonData4.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDtopoConversion"));
		pushButtonData4.Image = EPCommon.SafeResources.ToImageSource(Resources.ToposurfaceConversion);
		ribbonPanel.AddStackedItems(pushButtonData3, pushButtonData4);
		ribbonPanel = application.CreateRibbonPanel(EPShared.LocalRM.GetString("LandcaddSitesRibbonTab"), EPShared.LocalRM.GetString("LocateRibbonPanel"));
		if (ribbonPanel == null)
		{
			return;
		}
		PushButtonData pushButtonData5 = new PushButtonData(EPShared.LocalRM.GetString("LocateSoftTerrainRibbonButton"), EPShared.LocalRM.GetString("LocateSoftTerrainRibbonButton"), location, typeof(LocateSoftTerrainCommand).FullName);
		pushButtonData5.ToolTip = EPShared.LocalRM.GetString("LocateSoftTerrainRibbonToolTip");
		pushButtonData5.ToolTipImage = EPCommon.SafeResources.ToImageSource(Resources.tooltip_soft_terrain);
		pushButtonData5.LongDescription = EPShared.LocalRM.GetString("LocateSoftTerrainRibbonToolTipLongDescription");
		pushButtonData5.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDsoftTerrainCreate"));
		pushButtonData5.Image = EPCommon.SafeResources.ToImageSource(Resources.LocateSoftTerrain);
		PushButtonData pushButtonData6 = new PushButtonData(EPShared.LocalRM.GetString("FeatureLineRibbonButton"), EPShared.LocalRM.GetString("FeatureLineRibbonButton"), location, typeof(LocateFeatureLineCommand).FullName);
		pushButtonData6.ToolTip = EPShared.LocalRM.GetString("LocateFeatureLineRibbonToolTip");
		pushButtonData6.ToolTipImage = EPCommon.SafeResources.ToImageSource(Resources.tooltip_feature_line);
		pushButtonData6.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDfeatureLineCreate"));
		pushButtonData6.Image = EPCommon.SafeResources.ToImageSource(Resources.LocateFeatureLine);
		PushButtonData pushButtonData7 = new PushButtonData(EPShared.LocalRM.GetString("PointWipeoutRibbonButton"), EPShared.LocalRM.GetString("PointWipeoutRibbonButton"), location, typeof(LocatePointWipeoutCommand).FullName);
		pushButtonData7.ToolTip = EPShared.LocalRM.GetString("LocatePointWipeoutRibbonToolTip");
		pushButtonData7.ToolTipImage = EPCommon.SafeResources.ToImageSource(Resources.tooltip_point_wipeout);
		pushButtonData7.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDpointWipeoutCreate"));
		pushButtonData7.Image = EPCommon.SafeResources.ToImageSource(Resources.LocatePointWipeout);
		ribbonPanel.AddStackedItems(pushButtonData5, pushButtonData6, pushButtonData7);
		PushButtonData pushButtonData8 = new PushButtonData(EPShared.LocalRM.GetString("LocateStreetRibbonButton"), EPShared.LocalRM.GetString("LocateStreetRibbonButton"), location, typeof(LocateStreetCommand).FullName);
		pushButtonData8.ToolTip = EPShared.LocalRM.GetString("LocateStreetRibbonToolTip");
		pushButtonData8.ToolTipImage = EPCommon.SafeResources.ToImageSource(Resources.tooltip_street);
		pushButtonData8.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDstreetCreate"));
		pushButtonData8.Image = EPCommon.SafeResources.ToImageSource(Resources.LocateStreet);
		PushButtonData pushButtonData9 = new PushButtonData(EPShared.LocalRM.GetString("LocateRetainingWallRibbonButton"), EPShared.LocalRM.GetString("LocateRetainingWallRibbonButton"), location, typeof(LocateRetainingWallCommand).FullName);
		pushButtonData9.ToolTip = EPShared.LocalRM.GetString("LocateRetainingWallRibbonToolTip");
		pushButtonData9.ToolTipImage = EPCommon.SafeResources.ToImageSource(Resources.tooltip_retaining_wall);
		pushButtonData9.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDretainingWallCreate"));
		pushButtonData9.Image = EPCommon.SafeResources.ToImageSource(Resources.LocateRetainingWall);
		ribbonPanel.AddStackedItems(pushButtonData8, pushButtonData9);
		PushButtonData pushButtonData10 = new PushButtonData(EPShared.LocalRM.GetString("LocateSidewalkRibbonButton"), EPShared.LocalRM.GetString("LocateSidewalkRibbonButton"), location, typeof(LocateSideWalkCommand).FullName);
		pushButtonData10.ToolTip = EPShared.LocalRM.GetString("LocateSidewalkRibbonToolTip");
		pushButtonData10.ToolTipImage = EPCommon.SafeResources.ToImageSource(Resources.tooltip_sidewalk);
		pushButtonData10.LongDescription = EPShared.LocalRM.GetString("LocateSidewalkRibbonToolTipLongDescription");
		pushButtonData10.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDsidewalkCreate"));
		pushButtonData10.Image = EPCommon.SafeResources.ToImageSource(Resources.LocateSidewalk);
		PushButtonData pushButtonData11 = new PushButtonData(EPShared.LocalRM.GetString("LocateParkingLotRibbonButton"), EPShared.LocalRM.GetString("LocateParkingLotRibbonButton"), location, typeof(LocateParkingLotCommand).FullName);
		pushButtonData11.ToolTip = EPShared.LocalRM.GetString("LocateParkingLotRibbonToolTip");
		pushButtonData11.ToolTipImage = EPCommon.SafeResources.ToImageSource(Resources.tooltip_parking_lot);
		pushButtonData11.LongDescription = EPShared.LocalRM.GetString("LocateParkingLotRibbonToolTipLongDescription");
		pushButtonData11.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDparkinglotCreate"));
		pushButtonData11.Image = EPCommon.SafeResources.ToImageSource(Resources.LocateParkingLot);
		PushButtonData pushButtonData12 = new PushButtonData(EPShared.LocalRM.GetString("LocateCurbRibbonButton"), EPShared.LocalRM.GetString("LocateCurbRibbonButton"), location, typeof(LocateCurbCommand).FullName);
		pushButtonData12.ToolTip = EPShared.LocalRM.GetString("LocateCurbRibbonToolTip");
		pushButtonData12.ToolTipImage = EPCommon.SafeResources.ToImageSource(Resources.tooltip_curb);
		pushButtonData12.LongDescription = EPShared.LocalRM.GetString("LocateCurbRibbonToolTipLongDescription");
		pushButtonData12.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDcurbCreate"));
		pushButtonData12.Image = EPCommon.SafeResources.ToImageSource(Resources.LocateCurb);
		ribbonPanel.AddStackedItems(pushButtonData10, pushButtonData11, pushButtonData12);
		ribbonPanel = application.CreateRibbonPanel(EPShared.LocalRM.GetString("LandcaddSitesRibbonTab"), EPShared.LocalRM.GetString("ModifyRibbonPanel"));
		if (ribbonPanel == null)
		{
			return;
		}
		PushButtonData pushButtonData13 = new PushButtonData(EPShared.LocalRM.GetString("ModifySoftTerrainRibbonButton"), EPShared.LocalRM.GetString("ModifySoftTerrainRibbonButton"), location, typeof(ModifySoftTerrainCommand).FullName);
		pushButtonData13.ToolTip = EPShared.LocalRM.GetString("ModifySoftTerrainRibbonToolTip");
		pushButtonData13.LongDescription = EPShared.LocalRM.GetString("ModifySoftTerrainRibbonToolTipLongDescription");
		pushButtonData13.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDsoftTerrainModify"));
		pushButtonData13.Image = EPCommon.SafeResources.ToImageSource(Resources.ModifySoftTerrain);
		PushButtonData pushButtonData14 = new PushButtonData(EPShared.LocalRM.GetString("FeatureLineRibbonButton"), EPShared.LocalRM.GetString("FeatureLineRibbonButton"), location, typeof(ModifyFeatureLineCommand).FullName);
		pushButtonData14.ToolTip = EPShared.LocalRM.GetString("ModifyFeatureLineRibbonToolTip");
		pushButtonData14.LongDescription = EPShared.LocalRM.GetString("ModifyFeatureLineRibbonToolTipLongDescription");
		pushButtonData14.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDfeatureLineModify"));
		pushButtonData14.Image = EPCommon.SafeResources.ToImageSource(Resources.ModifyFeatureLine);
		PushButtonData pushButtonData15 = new PushButtonData(EPShared.LocalRM.GetString("PointWipeoutRibbonButton"), EPShared.LocalRM.GetString("PointWipeoutRibbonButton"), location, typeof(ModifyPointWipeoutCommand).FullName);
		pushButtonData15.ToolTip = EPShared.LocalRM.GetString("ModifyPointWipeoutRibbonToolTip");
		pushButtonData15.LongDescription = EPShared.LocalRM.GetString("ModifyPointWipeoutRibbonToolTipLongDescription");
		pushButtonData15.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDpointWipeoutModify"));
		pushButtonData15.Image = EPCommon.SafeResources.ToImageSource(Resources.ModifyPointWipeout);
		ribbonPanel.AddStackedItems(pushButtonData13, pushButtonData14, pushButtonData15);
		PushButtonData pushButtonData16 = new PushButtonData(EPShared.LocalRM.GetString("ModifyStreetRibbonButton"), EPShared.LocalRM.GetString("ModifyStreetRibbonButton"), location, typeof(ModifyStreetCommand).FullName);
		pushButtonData16.ToolTip = EPShared.LocalRM.GetString("ModifyStreetRibbonToolTip");
		pushButtonData16.LongDescription = EPShared.LocalRM.GetString("ModifyStreetRibbonToolTipLongDescription");
		pushButtonData16.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDstreetModify"));
		pushButtonData16.Image = EPCommon.SafeResources.ToImageSource(Resources.ModifyStreet);
		PushButtonData pushButtonData17 = new PushButtonData(EPShared.LocalRM.GetString("ModifyRetainingWallRibbonButton"), EPShared.LocalRM.GetString("ModifyRetainingWallRibbonButton"), location, typeof(ModifyRetainingWallCommand).FullName);
		pushButtonData17.ToolTip = EPShared.LocalRM.GetString("ModifyRetainingWallRibbonToolTip");
		pushButtonData17.LongDescription = EPShared.LocalRM.GetString("ModifyRetainingWallRibbonToolTipLongDescription");
		pushButtonData17.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDretainingWallModify"));
		pushButtonData17.Image = EPCommon.SafeResources.ToImageSource(Resources.ModifyRetainingWall);
		ribbonPanel.AddStackedItems(pushButtonData16, pushButtonData17);
		PushButtonData pushButtonData18 = new PushButtonData(EPShared.LocalRM.GetString("ModifySidewalkRibbonButton"), EPShared.LocalRM.GetString("ModifySidewalkRibbonButton"), location, typeof(ModifySidewalkCommand).FullName);
		pushButtonData18.ToolTip = EPShared.LocalRM.GetString("ModifySidewalkRibbonToolTip");
		pushButtonData18.LongDescription = EPShared.LocalRM.GetString("ModifySidewalkRibbonToolTipLongDescription");
		pushButtonData18.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDsidewalkModify"));
		pushButtonData18.Image = EPCommon.SafeResources.ToImageSource(Resources.ModifySidewalk);
		PushButtonData pushButtonData19 = new PushButtonData(EPShared.LocalRM.GetString("ModifyParkingLotRibbonButton"), EPShared.LocalRM.GetString("ModifyParkingLotRibbonButton"), location, typeof(ModifyParkingLotCommand).FullName);
		pushButtonData19.ToolTip = EPShared.LocalRM.GetString("ModifyParkingLotRibbonToolTip");
		pushButtonData19.LongDescription = EPShared.LocalRM.GetString("ModifyParkingLotRibbonToolTipLongDescription");
		pushButtonData19.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDparkinglotModify"));
		pushButtonData19.Image = EPCommon.SafeResources.ToImageSource(Resources.ModifyParkingLot);
		PushButtonData pushButtonData20 = new PushButtonData(EPShared.LocalRM.GetString("ModifyCurbRibbonButton"), EPShared.LocalRM.GetString("ModifyCurbRibbonButton"), location, typeof(ModifyCurbCommand).FullName);
		pushButtonData20.ToolTip = EPShared.LocalRM.GetString("ModifyCurbRibbonToolTip");
		pushButtonData20.LongDescription = EPShared.LocalRM.GetString("ModifyCurbRibbonToolTipLongDescription");
		pushButtonData20.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDcurbModify"));
		pushButtonData20.Image = EPCommon.SafeResources.ToImageSource(Resources.ModifyCurb);
		ribbonPanel.AddStackedItems(pushButtonData18, pushButtonData19, pushButtonData20);
		ribbonPanel = application.CreateRibbonPanel(EPShared.LocalRM.GetString("LandcaddSitesRibbonTab"), EPShared.LocalRM.GetString("ReportRibbonPanel"));
		if (ribbonPanel == null)
		{
			return;
		}
		PushButtonData pushButtonData21 = new PushButtonData(EPShared.LocalRM.GetString("AnnotateRibbonButton"), EPShared.LocalRM.GetString("AnnotateRibbonButton"), location, typeof(AnnotateCommand).FullName);
		pushButtonData21.ToolTip = EPShared.LocalRM.GetString("AnnotateRibbonToolTip");
		pushButtonData21.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDannotate"));
		pushButtonData21.Image = EPCommon.SafeResources.ToImageSource(Resources.Annotate);
		PushButtonData pushButtonData22 = new PushButtonData(EPShared.LocalRM.GetString("SitesReportRibbonButton"), EPShared.LocalRM.GetString("SitesReportRibbonButton"), location, typeof(SitesReportCommand).FullName);
		pushButtonData22.ToolTip = EPShared.LocalRM.GetString("SitesReportRibbonToolTip");
		pushButtonData22.LongDescription = EPShared.LocalRM.GetString("SitesReportRibbonToolTipLongDescription");
		pushButtonData22.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDreports"));
		pushButtonData22.Image = EPCommon.SafeResources.ToImageSource(Resources.Report);
		ribbonPanel.AddStackedItems(pushButtonData21, pushButtonData22);
		ribbonPanel = application.CreateRibbonPanel(EPShared.LocalRM.GetString("LandcaddSitesRibbonTab"), EPShared.LocalRM.GetString("FamilyManagerRibbonPanel"));
		if (ribbonPanel != null)
		{
			PulldownButtonData pulldownButtonData = new PulldownButtonData(EPShared.LocalRM.GetString("FamilyManagerRibbonPanel"), EPShared.LocalRM.GetString("FamilyManagerRibbonPanel"));
			pulldownButtonData.ToolTip = EPShared.LocalRM.GetString("SiteComponentFamilyManagerToolTip");
			pulldownButtonData.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDtipFamilyManagers"));
			pulldownButtonData.LargeImage = EPCommon.SafeResources.ToImageSource(Resources.StreetFamilyManager);
			PulldownButton pulldownButton = ribbonPanel.AddItem(pulldownButtonData) as PulldownButton;
			PushButtonData pushButtonData23 = new PushButtonData(EPShared.LocalRM.GetString("SoftTerrainFamilyManagerRibbonButton"), EPShared.LocalRM.GetString("SoftTerrainFamilyManagerRibbonButton"), location, typeof(SoftTerrainFamilyManagerCommand).FullName);
			pushButtonData23.ToolTip = EPShared.LocalRM.GetString("SoftTerrainFamilyManagerRibbonToolTip");
			pushButtonData23.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDfamilyMgrSoftTerrain"));
			pushButtonData23.LargeImage = EPCommon.SafeResources.ToImageSource(Resources.SoftTerrainManager);
			pulldownButton.AddPushButton(pushButtonData23);
			PushButtonData pushButtonData24 = new PushButtonData(EPShared.LocalRM.GetString("StreetFamilyManagerRibbonButton"), EPShared.LocalRM.GetString("StreetFamilyManagerRibbonButton"), location, typeof(StreetsFamilyManagerCommand).FullName);
			pushButtonData24.ToolTip = EPShared.LocalRM.GetString("StreetFamilyManagerToolTip");
			pushButtonData24.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDfamilyMgrStreet"));
			pushButtonData24.LargeImage = EPCommon.SafeResources.ToImageSource(Resources.StreetFamilyManager);
			pulldownButton.AddPushButton(pushButtonData24);
			PushButtonData pushButtonData25 = new PushButtonData(EPShared.LocalRM.GetString("RetainingWallFamilyManagerRibbonButton"), EPShared.LocalRM.GetString("RetainingWallFamilyManagerRibbonButton"), location, typeof(RetainingWallFamilyManagerCommand).FullName);
			pushButtonData25.ToolTip = EPShared.LocalRM.GetString("RetainingWallFamilyManagerToolTip");
			pushButtonData25.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDfamilyMgrRetainingWall"));
			pushButtonData25.LargeImage = EPCommon.SafeResources.ToImageSource(Resources.RetainingWallFamilyManager);
			pulldownButton.AddPushButton(pushButtonData25);
			PushButtonData pushButtonData26 = new PushButtonData(EPShared.LocalRM.GetString("SidewalkFamilyManagerRibbonButton"), EPShared.LocalRM.GetString("SidewalkFamilyManagerRibbonButton"), location, typeof(SidewalkFamilyManagerCommand).FullName);
			pushButtonData26.ToolTip = EPShared.LocalRM.GetString("SidewalkFamilyManagerToolTip");
			pushButtonData26.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDfamilyMgrSidewalk"));
			pushButtonData26.LargeImage = EPCommon.SafeResources.ToImageSource(Resources.SidewalkFamilyManager);
			pulldownButton.AddPushButton(pushButtonData26);
			PushButtonData pushButtonData27 = new PushButtonData(EPShared.LocalRM.GetString("ParkingLotFamilyManagerRibbonButton"), EPShared.LocalRM.GetString("ParkingLotFamilyManagerRibbonButton"), location, typeof(ParkingLotFamilyManagerCommand).FullName);
			pushButtonData27.ToolTip = EPShared.LocalRM.GetString("ParkingLotFamilyManagerToolTip");
			pushButtonData27.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDfamilyMgrParkinglot"));
			pushButtonData27.LargeImage = EPCommon.SafeResources.ToImageSource(Resources.ParkingLotFamilyManager);
			pulldownButton.AddPushButton(pushButtonData27);
			PushButtonData pushButtonData28 = new PushButtonData(EPShared.LocalRM.GetString("CurbFamilyManagerRibbonButton"), EPShared.LocalRM.GetString("CurbFamilyManagerRibbonButton"), location, typeof(CurbFamilyManagerCommand).FullName);
			pushButtonData28.ToolTip = EPShared.LocalRM.GetString("CurbFamilyManagerToolTip");
			pushButtonData28.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDfamilyMgrCurb"));
			pushButtonData28.LargeImage = EPCommon.SafeResources.ToImageSource(Resources.CurbFamilyManager);
			pulldownButton.AddPushButton(pushButtonData28);
			ribbonPanel = application.CreateRibbonPanel(EPShared.LocalRM.GetString("LandcaddSitesRibbonTab"), EPShared.LocalRM.GetString("SettingRibbonButton"));
			if (ribbonPanel != null)
			{
				PushButtonData pushButtonData29 = new PushButtonData(EPShared.LocalRM.GetString("LandcaddSitesRibbonTab") + " " + EPShared.LocalRM.GetString("AboutRibbonButton"), EPShared.LocalRM.GetString("AboutRibbonButton"), location, typeof(SitesAboutCommand).FullName);
				pushButtonData29.ToolTip = EPShared.LocalRM.GetString("AboutSiteworksRibbonToolTip");
				pushButtonData29.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDabout"));
				pushButtonData29.Image = EPCommon.SafeResources.ToImageSource(Resources.About);
				PushButtonData pushButtonData30 = new PushButtonData(EPShared.LocalRM.GetString("LandcaddSitesRibbonTab") + " " + EPShared.LocalRM.GetString("SettingRibbonButton"), EPShared.LocalRM.GetString("SettingRibbonButton"), location, typeof(SitesSettingsCommand).FullName);
				pushButtonData30.ToolTip = EPShared.LocalRM.GetString("SettingRibbonToolTip");
				pushButtonData30.SetContextualHelp(new ContextualHelp(ContextualHelpType.ContextId, "HID_SDsettings"));
				pushButtonData30.Image = EPCommon.SafeResources.ToImageSource(Resources.Settings);
				ribbonPanel.AddStackedItems(pushButtonData30, pushButtonData29);
			}
		}
	}

	private bool AddLandcaddRibbonIfNotPresent()
	{
		return true;
	}

	public Result OnShutdown(UIControlledApplication application)
	{
		application.ControlledApplication.DocumentOpened -= DocumentOpenedHandler;
		application.ControlledApplication.DocumentSaving -= DocumentExitHandler;
		application.ControlledApplication.DocumentPrinting -= DocumentExitHandler;
		application.ControlledApplication.DocumentSavingAs -= DocumentExitHandler;
		application.ControlledApplication.DocumentClosing -= DocumentClosingHandler;
		application.ControlledApplication.DocumentCreated -= DocumentCreatedHandler;
		application.DialogBoxShowing -= DialogBoxShowingHandler;
		application.ViewActivated -= ViewActivatedHandler;
		application.ApplicationClosing -= ApplicationClosingHandler;
		return Result.Succeeded;
	}
}
