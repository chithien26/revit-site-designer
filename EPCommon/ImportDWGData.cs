using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Autodesk.Revit.DB;

namespace EPCommon;

public class ImportDWGData : ImportData
{
	private bool m_bImportThisViewOnly;

	private ViewSet m_Views;

	private View m_ImportView;

	private List<string> m_ColorModes;

	private List<ImportColorMode> m_EnumColorModes;

	private ImportColorMode m_ImportColorMode;

	private List<ImportPlacement> m_EnumPlacements;

	private ImportPlacement m_ImportPlacement;

	private List<ImportUnit> m_EnumUnits;

	private ImportUnit m_ImportUnit;

	private double m_dImportCustomScale;

	private List<string> m_Placements;

	private List<string> m_DimensionTypes;

	private List<string> m_Units;

	private List<string> m_VisibleLayersOnlyList;

	private List<bool> m_EnumVisibleLayersOnlyList;

	private bool m_ImportVisibleLayersOnlyList;

	public bool ImportThisViewOnly
	{
		get
		{
			return m_bImportThisViewOnly;
		}
		set
		{
			m_bImportThisViewOnly = value;
		}
	}

	public ViewSet Views
	{
		get
		{
			return m_Views;
		}
		set
		{
			m_Views = value;
		}
	}

	public View ImportView
	{
		get
		{
			return m_ImportView;
		}
		set
		{
			m_ImportView = value;
		}
	}

	public ReadOnlyCollection<string> ColorModes => new ReadOnlyCollection<string>(m_ColorModes);

	public ReadOnlyCollection<ImportColorMode> EnumColorModes => new ReadOnlyCollection<ImportColorMode>(m_EnumColorModes);

	public ImportColorMode ImportColorMode
	{
		get
		{
			return m_ImportColorMode;
		}
		set
		{
			m_ImportColorMode = value;
		}
	}

	public ReadOnlyCollection<ImportPlacement> EnumPlacements => new ReadOnlyCollection<ImportPlacement>(m_EnumPlacements);

	public ImportPlacement ImportPlacement
	{
		get
		{
			return m_ImportPlacement;
		}
		set
		{
			m_ImportPlacement = value;
		}
	}

	public ReadOnlyCollection<ImportUnit> EnumUnits => new ReadOnlyCollection<ImportUnit>(m_EnumUnits);

	public ImportUnit ImportUnit
	{
		get
		{
			return m_ImportUnit;
		}
		set
		{
			m_ImportUnit = value;
		}
	}

	public double ImportCustomScale
	{
		get
		{
			return m_dImportCustomScale;
		}
		set
		{
			m_dImportCustomScale = value;
		}
	}

	public ReadOnlyCollection<string> Placements => new ReadOnlyCollection<string>(m_Placements);

	public ReadOnlyCollection<string> DimensionTypes => new ReadOnlyCollection<string>(m_DimensionTypes);

	public ReadOnlyCollection<string> Units => new ReadOnlyCollection<string>(m_Units);

	public ReadOnlyCollection<string> VisibleLayersOnlyList => new ReadOnlyCollection<string>(m_VisibleLayersOnlyList);

	public ReadOnlyCollection<bool> EnumVisibleLayersOnlyList => new ReadOnlyCollection<bool>(m_EnumVisibleLayersOnlyList);

	public bool ImportVisibleLayersOnlyList
	{
		get
		{
			return m_ImportVisibleLayersOnlyList;
		}
		set
		{
			m_ImportVisibleLayersOnlyList = value;
		}
	}

	public ImportDWGData(Document _CurDocument)
		: base(_CurDocument)
	{
		Initialize();
	}

	public override bool Import()
	{
		bool flag = false;
		DWGImportOptions dWGImportOptions = new DWGImportOptions();
		dWGImportOptions.ColorMode = m_ImportColorMode;
		dWGImportOptions.CustomScale = m_dImportCustomScale;
		dWGImportOptions.OrientToView = false;
		dWGImportOptions.Placement = m_ImportPlacement;
		dWGImportOptions.ThisViewOnly = m_bImportThisViewOnly;
		dWGImportOptions.Unit = m_ImportUnit;
		dWGImportOptions.VisibleLayersOnly = m_ImportVisibleLayersOnlyList;
		Element element = null;
		m_TransactionContainer.Start(EPShared.LocalRM.GetString("ImportDrawingTransaction") + Path.GetFileName(m_strImportFileFullName));
		m_TransactionContainer.BeginTransaction();
		ElementId elementId = null;
		flag = m_ActiveDoc.Import(m_strImportFileFullName, dWGImportOptions, m_ImportView, out elementId);
		element = m_ActiveDoc.GetElement(elementId);
		if (!m_bImportThisViewOnly && flag && element != null && element is ImportInstance importInstance)
		{
			FamilyElementVisibility visibility = importInstance.GetVisibility();
			visibility.IsShownInPlanRCPCut = false;
			visibility.IsShownInTopBottom = false;
			importInstance.SetVisibility(visibility);
			ElementSet elementSet = new ElementSet();
			elementSet.Insert(importInstance);
			m_ImportView.HideElements(ElementSetConverter.ToIDList(elementSet));
		}
		if (m_bAbortTransaction)
		{
			FileInfo fileInfo = new FileInfo(m_strSaveAsFileName);
			m_ActiveDoc.FamilyManager.NewType(fileInfo.Name.TrimEnd(fileInfo.Extension.ToCharArray()));
		}
		m_bAbortTransaction = !flag;
		m_TransactionContainer.EndTransaction();
		m_TransactionContainer.Commit();
		SaveAsOptions saveAsOptions = new SaveAsOptions();
		saveAsOptions.OverwriteExistingFile = true;
		m_ActiveDoc.SaveAs(m_strSaveAsFileName, saveAsOptions);
		return flag;
	}

	private void Initialize()
	{
		m_ColorModes = new List<string>();
		m_EnumColorModes = new List<ImportColorMode>();
		m_ColorModes.Add(EPShared.LocalRM.GetString("BlackAndWhiteColorMode"));
		m_EnumColorModes.Add(ImportColorMode.BlackAndWhite);
		m_ColorModes.Add(EPShared.LocalRM.GetString("PreserveColorsColorMode"));
		m_EnumColorModes.Add(ImportColorMode.Preserved);
		m_ColorModes.Add(EPShared.LocalRM.GetString("InvertColorsColorMode"));
		m_EnumColorModes.Add(ImportColorMode.Inverted);
		m_Placements = new List<string>();
		m_EnumPlacements = new List<ImportPlacement>();
		m_Placements.Add(EPShared.LocalRM.GetString("OriginToOriginPlacement"));
		m_EnumPlacements.Add(ImportPlacement.Origin);
		m_Placements.Add(EPShared.LocalRM.GetString("CenterToCenterPlacement"));
		m_EnumPlacements.Add(ImportPlacement.Centered);
		m_Units = new List<string>();
		m_EnumUnits = new List<ImportUnit>();
		m_Units.Add(EPShared.LocalRM.GetString("AutoDetectUnit"));
		m_EnumUnits.Add(ImportUnit.Default);
		m_Units.Add(ImportUnit.Foot.ToString());
		m_EnumUnits.Add(ImportUnit.Foot);
		m_Units.Add(ImportUnit.Inch.ToString());
		m_EnumUnits.Add(ImportUnit.Inch);
		m_Units.Add(ImportUnit.Meter.ToString());
		m_EnumUnits.Add(ImportUnit.Meter);
		m_Units.Add(ImportUnit.Decimeter.ToString());
		m_EnumUnits.Add(ImportUnit.Decimeter);
		m_Units.Add(ImportUnit.Centimeter.ToString());
		m_EnumUnits.Add(ImportUnit.Centimeter);
		m_Units.Add(ImportUnit.Millimeter.ToString());
		m_EnumUnits.Add(ImportUnit.Millimeter);
		m_Units.Add(EPShared.LocalRM.GetString("CustomUnit"));
		m_EnumUnits.Add(ImportUnit.Default);
		m_VisibleLayersOnlyList = new List<string>();
		m_EnumVisibleLayersOnlyList = new List<bool>();
		m_VisibleLayersOnlyList.Add(EPShared.LocalRM.GetString("AllString"));
		m_EnumVisibleLayersOnlyList.Add(item: false);
		m_VisibleLayersOnlyList.Add(EPShared.LocalRM.GetString("VisibleString"));
		m_EnumVisibleLayersOnlyList.Add(item: true);
		m_DimensionTypes = new List<string>();
		m_DimensionTypes.Add(EPShared.LocalRM.GetString("TwoDDWGSymbolDimensionType"));
		m_DimensionTypes.Add(EPShared.LocalRM.GetString("ThreeDDWGSymbolDimensionType"));
		m_Views = new ViewSet();
		GetViews();
		m_dImportCustomScale = 0.0;
		m_ImportUnit = ImportUnit.Default;
		m_bImportThisViewOnly = false;
		m_ImportView = m_ActiveDoc.ActiveView;
		m_ImportColorMode = ImportColorMode.Inverted;
		m_ImportPlacement = ImportPlacement.Origin;
		m_ImportVisibleLayersOnlyList = false;
	}

	private void GetViews()
	{
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_ActiveDoc);
		IList<Element> list = filteredElementCollector.OfClass(typeof(View)).ToElements();
		foreach (View item in list)
		{
			if (item.ViewType == ViewType.FloorPlan || item.ViewType == ViewType.EngineeringPlan || item.ViewType == ViewType.AreaPlan)
			{
				m_Views.Insert(item);
			}
		}
	}
}
