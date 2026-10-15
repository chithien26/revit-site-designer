using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using SITES.Utilities;

namespace EPCommon;

public class ImportLandXMLClass
{
	private Document m_Document;

	public ImportLandXMLClass(Document _CurDocument)
	{
		m_Document = _CurDocument;
	}

	public void Run()
	{
		ImportLandXMLManager importLandXMLManager = new ImportLandXMLManager(m_Document);
		DialogResult dialogResult = importLandXMLManager.ShowDialog();
		Eps_Point _Point = new Eps_Point();
		while (dialogResult == DialogResult.Retry)
		{
			RevitUtility.GetPointFromProject(m_Document, EPShared.LocalRM.GetString("PickCenterPointPrompt"), ref _Point, _bDisplayError: false);
			importLandXMLManager.ManualCenterBasePoint = _Point;
			dialogResult = importLandXMLManager.ImportSurfaceAlignments();
		}
	}

	public static FamilyInstance GetSiteComponentPlacementFamilyInstance(Document _Document, string _strTopoSurfaceId, bool _bCheckForSurface)
	{
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.LandXMLSurfacePlacement).ToString("000"), ignoreCase: true) == 0 && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
			{
				continue;
			}
			SiteworksIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (SiteworksIDParameter != null)
			{
				if (string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.LandXMLSurfacePlacement).ToString("000"), ignoreCase: true) != 0)
				{
					return null;
				}
				if (!_bCheckForSurface)
				{
					return familyInstance;
				}
				Parameter parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
				if (parameter != null && string.Compare(parameter.AsString(), _strTopoSurfaceId, ignoreCase: true) == 0)
				{
					return familyInstance;
				}
			}
		}
		return null;
	}

	public static bool FindPositioningParameters(FamilyInstance _CurrentInstance, ref double _dDistance, ref double _dHzDirection, ref double _dVzDirection)
	{
		Parameter parameter = null;
		int num = 1;
		parameter = _CurrentInstance.LookupParameter(RevitParameters.PositioningMethodLandXMLSurfacePlacementFamilyInstanceParameter);
		if (parameter != null)
		{
			num = parameter.AsInteger();
		}
		parameter = _CurrentInstance.LookupParameter(RevitParameters.PositionDistanceLandXMLSurfacePlacementFamilyInstanceParameter);
		if (parameter != null)
		{
			_dDistance = parameter.AsDouble();
		}
		parameter = _CurrentInstance.LookupParameter(RevitParameters.PositionHzDirectionLandXMLSurfacePlacementFamilyInstanceParameter);
		if (parameter != null)
		{
			_dHzDirection = parameter.AsDouble();
		}
		parameter = _CurrentInstance.LookupParameter(RevitParameters.PositionVzDirectionLandXMLSurfacePlacementFamilyInstanceParameter);
		if (parameter != null)
		{
			_dVzDirection = parameter.AsDouble();
		}
		if (num == 1)
		{
			return false;
		}
		return true;
	}
}
