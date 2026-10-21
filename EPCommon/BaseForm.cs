using System.Collections.Generic;
using System.IO;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using SITES.Utilities;

namespace EPCommon;

public class BaseForm
{
	protected EPTransactionContainer m_TransactionContainer;

	protected EPTransactionContainer m_AdditionalTransactionContainer;

	private Eps_Leg m_CurrentLeg;

	protected Eps_Point m_LegInsertPoint;

	protected Eps_Leg m_MassLeg;

	protected Document m_MassDocument;

	protected Document m_AdditionalMassDocument;

	protected ProgressBarForm m_ProgressBarForm;

	protected double m_dOffsetAdjust = 0.01;

	protected List<IntersectionContainer> m_IntersectionContainers;

	public Eps_Leg CurrentLeg
	{
		get
		{
			return m_CurrentLeg;
		}
		set
		{
			m_CurrentLeg = value;
		}
	}

	public Eps_Leg MassLeg
	{
		get
		{
			return m_MassLeg;
		}
		set
		{
			m_MassLeg = value;
		}
	}

	public Document MassDocument
	{
		get
		{
			return m_MassDocument;
		}
		set
		{
			m_MassDocument = value;
		}
	}

	public List<IntersectionContainer> IntersectionContainers
	{
		get
		{
			return m_IntersectionContainers;
		}
		set
		{
			m_IntersectionContainers = value;
		}
	}

	public BaseForm(Eps_Leg _CurrentLeg)
	{
		m_CurrentLeg = _CurrentLeg;
		m_TransactionContainer = null;
		m_AdditionalTransactionContainer = null;
		m_MassDocument = null;
		m_AdditionalMassDocument = null;
		m_MassLeg = null;
		m_IntersectionContainers = null;
	}

	public string CreateForm(Document _ProjectDocument, string _strSiteComponentName)
	{
		string _strAdditionalLoftID = string.Empty;
		return CreateForm(_ProjectDocument, _strSiteComponentName, _bUseInternalPoint: true, 0.0, ref _strAdditionalLoftID);
	}

	public string CreateForm(Document _ProjectDocument, string _strSiteComponentName, ref string _strAdditionalLoftID)
	{
		return CreateForm(_ProjectDocument, _strSiteComponentName, _bUseInternalPoint: true, 0.0, ref _strAdditionalLoftID);
	}

	public string CreateForm(Document _ProjectDocument, string _strSiteComponentName, bool _bUseInternalPoint, double _dElevation)
	{
		string _strAdditionalLoftID = string.Empty;
		return CreateForm(_ProjectDocument, _strSiteComponentName, _bUseInternalPoint, _dElevation, ref _strAdditionalLoftID);
	}

	public string CreateForm(Document _ProjectDocument, string _strSiteComponentName, bool _bUseInternalPoint, double _dElevation, ref string _strAdditionalLoftID)
	{
		if (!ApplicationUtility.UseMass || (this is StreetForm && TerrainApi.UsesToposolids))
		{
			return CreateElement(_ProjectDocument, _strSiteComponentName, _bUseInternalPoint, _dElevation, ref _strAdditionalLoftID);
		}
		LandcaddID curID = LandcaddID.Steps;
		if (this is SidewalkForm)
		{
			curID = LandcaddID.Sidewalk;
		}
		else if (this is RetainingWallForm)
		{
			curID = LandcaddID.RetainingWall;
		}
		else if (this is CurbForm)
		{
			curID = LandcaddID.Curb;
		}
		else if (this is StreetForm)
		{
			curID = LandcaddID.Street;
		}
		string _strTemplateFileName = string.Empty;
		string _strAdditionalTemplateFileName = string.Empty;
		string empty = string.Empty;
		_strAdditionalLoftID = string.Empty;
		if (!RevitUtility.FindMassTemplateFileName(_ProjectDocument, curID, ref _strTemplateFileName, ref _strAdditionalTemplateFileName))
		{
			return null;
		}
		if (m_CurrentLeg == null)
		{
			return null;
		}
		string _strSupportPath = Path.GetTempPath();
		if (_strSupportPath != string.Empty)
		{
			_strSupportPath += "\\Autodesk\\Site Designer for Revit\\";
			if (!Directory.Exists(_strSupportPath))
			{
				Directory.CreateDirectory(_strSupportPath);
			}
		}
		else
		{
			ApplicationUtility.RegistryPaths.GetSupportPath(string.Empty, ref _strSupportPath);
		}
		double _dDistance = 0.0;
		double _dDirection = 0.0;
		Eps_Point eps_Point = new Eps_Point(m_CurrentLeg.LegStartPnt);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Point, new Eps_Point(0.0, 0.0, 0.0), ref _dDistance);
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point, new Eps_Point(0.0, 0.0, 0.0), ref _dDirection);
		m_MassLeg = m_CurrentLeg.MoveLeg(_dDirection, _dDistance, 0.0 - eps_Point.z);
		if (m_IntersectionContainers != null && m_IntersectionContainers.Count > 0)
		{
			Eps_Point _NewPnt = new Eps_Point();
			for (int i = 0; i < m_IntersectionContainers.Count; i++)
			{
				for (int j = 0; j < m_IntersectionContainers[i].Profiles.Count; j++)
				{
					for (int k = 0; k < m_IntersectionContainers[i].Profiles[j].Points.Count; k++)
					{
						EPShared.eplGeoE_CalcPolarToXY(m_IntersectionContainers[i].Profiles[j].Points[k], EPShared.eplGeoE_NormalizeRadians(_dDirection), _dDistance, ref _NewPnt);
						_NewPnt.z -= eps_Point.z;
						m_IntersectionContainers[i].Profiles[j].Points[k] = new Eps_Point(_NewPnt);
					}
				}
				if (m_IntersectionContainers[i].LeftOffsetLeg != null)
				{
					m_IntersectionContainers[i].LeftOffsetLeg = m_IntersectionContainers[i].LeftOffsetLeg.MoveLeg(_dDirection, _dDistance, 0.0 - eps_Point.z);
				}
				if (m_IntersectionContainers[i].RightOffsetLeg != null)
				{
					m_IntersectionContainers[i].RightOffsetLeg = m_IntersectionContainers[i].RightOffsetLeg.MoveLeg(_dDirection, _dDistance, 0.0 - eps_Point.z);
				}
			}
		}
		m_MassLeg = m_MassLeg.SplitArcsInLeg();
		m_MassDocument = _ProjectDocument.Application.NewFamilyDocument(_strTemplateFileName);
		m_TransactionContainer = new EPTransactionContainer(m_MassDocument);
		m_TransactionContainer.Start(EPShared.LocalRM.GetString("CreateMassFormTransaction") + " " + _strSiteComponentName);
		List<FailureDefinitionId> list = new List<FailureDefinitionId>();
		list.Add(BuiltInFailures.InaccurateFailures.InaccurateLine);
		list.Add(BuiltInFailures.InaccurateFailures.InaccurateMassingSketchLine);
		list.Add(BuiltInFailures.OverlapFailures.CurvesOverlap);
		list.Add(BuiltInFailures.JoinElementsFailures.CannotJoinElements);
		list.Add(BuiltInFailures.MassFailures.CannotMergeGeometry);
		m_TransactionContainer.FailureHandlingOptions(list);
		m_TransactionContainer.BeginTransaction();
		m_MassDocument.FamilyManager.NewType(_strSiteComponentName);
		List<Form> list2 = null;
		if (this is SidewalkForm)
		{
			SidewalkForm sidewalkForm = (SidewalkForm)this;
			list2 = sidewalkForm.CreateLoftFormWithParameters();
		}
		else if (this is StreetForm)
		{
			StreetForm streetForm = (StreetForm)this;
			if (streetForm.StreetFamilySymbol != null)
			{
				Parameter parameter = streetForm.StreetFamilySymbol.LookupParameter(RevitParameters.StreetCurbDisplayParameter);
				if (parameter != null && parameter.AsInteger() != 0)
				{
					m_AdditionalMassDocument = _ProjectDocument.Application.NewFamilyDocument(_strAdditionalTemplateFileName);
					m_AdditionalTransactionContainer = new EPTransactionContainer(m_AdditionalMassDocument);
					m_AdditionalTransactionContainer.Start(EPShared.LocalRM.GetString("CreateMassFormTransaction") + " " + _strSiteComponentName + " " + EPShared.LocalRM.GetString("StreetCurbDisplayParameter"));
					m_AdditionalTransactionContainer.FailureHandlingOptions(list);
					m_AdditionalTransactionContainer.BeginTransaction();
					m_AdditionalMassDocument.FamilyManager.NewType(_strSiteComponentName + " " + EPShared.LocalRM.GetString("StreetCurbDisplayParameter"));
				}
			}
			list2 = streetForm.CreateLoftFormWithParameters();
		}
		else if (this is RetainingWallForm)
		{
			RetainingWallForm retainingWallForm = (RetainingWallForm)this;
			list2 = retainingWallForm.CreateLoftFormWithParameters();
		}
		else if (this is CurbForm)
		{
			CurbForm curbForm = (CurbForm)this;
			list2 = curbForm.CreateLoftFormWithParameters(0.0, _dElevation, _bUseInternalPoint);
		}
		m_TransactionContainer.EndTransaction();
		if (m_AdditionalTransactionContainer != null)
		{
			m_AdditionalTransactionContainer.EndTransaction();
		}
		if (list2 == null || list2.Count == 0)
		{
			m_TransactionContainer.Rollback();
			if (m_AdditionalTransactionContainer != null)
			{
				m_AdditionalTransactionContainer.Rollback();
			}
			return string.Empty;
		}
		m_TransactionContainer.Commit();
		if (m_AdditionalTransactionContainer != null)
		{
			m_AdditionalTransactionContainer.Commit();
		}
		m_ProgressBarForm = new ProgressBarForm();
		m_ProgressBarForm.ShowForm();
		m_ProgressBarForm.Refresh();
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = 10;
		if (this is SidewalkForm)
		{
			m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingSidewalkMassMessage");
		}
		else if (this is RetainingWallForm)
		{
			m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingRetainingWallMassMessage");
		}
		else if (this is StreetForm)
		{
			m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingStreetMassMessage");
		}
		else if (this is CurbForm)
		{
			m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingCurbMassMessage");
		}
		SaveAsOptions saveAsOptions = new SaveAsOptions();
		saveAsOptions.OverwriteExistingFile = true;
		m_MassDocument.SaveAs(Path.Combine(_strSupportPath, _strSiteComponentName + ".rfa"), saveAsOptions);
		m_MassDocument.Close(saveModified: false);
		if (m_AdditionalMassDocument != null)
		{
			SaveAsOptions saveAsOptions2 = new SaveAsOptions();
			saveAsOptions2.OverwriteExistingFile = true;
			m_AdditionalMassDocument.SaveAs(Path.Combine(_strSupportPath, _strSiteComponentName + " " + EPShared.LocalRM.GetString("StreetCurbDisplayParameter") + ".rfa"), saveAsOptions2);
			m_AdditionalMassDocument.Close(saveModified: false);
		}
		m_ProgressBarForm.UpdateProgressBar(1);
		FamilySymbol _FamilySymbol = null;
		FamilySymbol _FamilySymbol2 = null;
		RevitUtility.LoadFamilySymbol(_ProjectDocument, Path.Combine(_strSupportPath, _strSiteComponentName + ".rfa"), _strSiteComponentName, out _FamilySymbol, _bUseFailureHandling: true, ref m_TransactionContainer);
		if (m_AdditionalMassDocument != null)
		{
			RevitUtility.LoadFamilySymbol(_ProjectDocument, Path.Combine(_strSupportPath, _strSiteComponentName + " " + EPShared.LocalRM.GetString("StreetCurbDisplayParameter") + ".rfa"), _strSiteComponentName + " " + EPShared.LocalRM.GetString("StreetCurbDisplayParameter"), out _FamilySymbol2, _bUseFailureHandling: true, ref m_TransactionContainer);
		}
		m_ProgressBarForm.UpdateProgressBar(6);
		FamilyInstance familyInstance = RevitUtility.NewFamilyInstance(_ProjectDocument, eps_Point.XYZ, _FamilySymbol, null, _ProjectDocument.ActiveView.GenLevel, StructuralType.NonStructural, ref m_TransactionContainer);
		empty = familyInstance.UniqueId;
		FamilyInstance familyInstance2 = null;
		if (_FamilySymbol2 != null)
		{
			familyInstance2 = RevitUtility.NewFamilyInstance(_ProjectDocument, eps_Point.XYZ, _FamilySymbol2, null, _ProjectDocument.ActiveView.GenLevel, StructuralType.NonStructural, ref m_TransactionContainer);
			_strAdditionalLoftID = familyInstance2.UniqueId;
		}
		m_ProgressBarForm.UpdateProgressBar(1);
		if (this is SidewalkForm)
		{
			Parameter parameter2 = familyInstance.LookupParameter(RevitParameters.SideWalkPavementSurfaceParameter);
			if (parameter2 != null)
			{
				SidewalkForm sidewalkForm2 = (SidewalkForm)this;
				Parameter parameter3 = sidewalkForm2.SidewalkFamilySymbol.LookupParameter(RevitParameters.SideWalkPavementSurfaceParameter);
				if (parameter3 != null)
				{
					int num = 0;
					num = parameter3.AsInteger();
					parameter2.Set(RevitIds.Create(num));
				}
			}
		}
		else if (this is RetainingWallForm)
		{
			Parameter parameter4 = familyInstance.LookupParameter(RevitParameters.RetainingWallMaterialParameter);
			if (parameter4 != null)
			{
				RetainingWallForm retainingWallForm2 = (RetainingWallForm)this;
				Parameter parameter5 = retainingWallForm2.RetainingWallFamilySymbol.LookupParameter(RevitParameters.RetainingWallMaterialParameter);
				if (parameter5 != null)
				{
					int num2 = 0;
					num2 = parameter5.AsInteger();
					parameter4.Set(RevitIds.Create(num2));
				}
			}
		}
		else if (this is StreetForm)
		{
			Parameter parameter6 = familyInstance.LookupParameter(RevitParameters.StreetPavementSurfaceParameter);
			if (parameter6 != null)
			{
				StreetForm streetForm2 = (StreetForm)this;
				Parameter parameter7 = streetForm2.StreetFamilySymbol.LookupParameter(RevitParameters.StreetPavementSurfaceParameter);
				if (parameter7 != null)
				{
					int num3 = 0;
					num3 = parameter7.AsInteger();
					parameter6.Set(RevitIds.Create(num3));
				}
			}
			if (familyInstance2 != null)
			{
				parameter6 = familyInstance2.LookupParameter(RevitParameters.StreetCurbMaterialParameter);
				if (parameter6 != null)
				{
					StreetForm streetForm3 = (StreetForm)this;
					Parameter parameter8 = streetForm3.StreetFamilySymbol.LookupParameter(RevitParameters.StreetCurbMaterialParameter);
					if (parameter8 != null)
					{
						int num4 = 0;
						num4 = parameter8.AsInteger();
						parameter6.Set(RevitIds.Create(num4));
					}
				}
			}
		}
		else if (this is CurbForm)
		{
			Parameter parameter9 = familyInstance.LookupParameter(RevitParameters.StreetCurbMaterialParameter);
			if (parameter9 != null)
			{
				CurbForm curbForm2 = (CurbForm)this;
				Parameter parameter10 = curbForm2.CurbFamilySymbol.LookupParameter(RevitParameters.StreetCurbMaterialParameter);
				if (parameter10 != null)
				{
					int num5 = 0;
					num5 = parameter10.AsInteger();
					parameter9.Set(RevitIds.Create(num5));
				}
			}
		}
		m_ProgressBarForm.UpdateProgressBar(1);
		File.Delete(Path.Combine(_strSupportPath, _strSiteComponentName + ".rfa"));
		if (m_AdditionalMassDocument != null)
		{
			File.Delete(Path.Combine(_strSupportPath, _strSiteComponentName + " " + EPShared.LocalRM.GetString("StreetCurbDisplayParameter") + ".rfa"));
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(_strSupportPath);
		if (Directory.Exists(_strSupportPath))
		{
			FileInfo[] files = directoryInfo.GetFiles("*.rfa");
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				fileInfo.Delete();
			}
		}
		m_ProgressBarForm.SetMaxValue();
		Thread.Sleep(300);
		m_ProgressBarForm.Close();
		return empty;
	}

	public string CreateElement(Document _ProjectDocument, string _strSiteComponentName, bool _bUseInternalPoint, double _dElevation, ref string _strAdditionalLoftID)
	{
		string empty = string.Empty;
		_strAdditionalLoftID = string.Empty;
		if (m_CurrentLeg == null)
		{
			return string.Empty;
		}
		double _dDistance = 0.0;
		double _dDirection = 0.0;
		m_LegInsertPoint = new Eps_Point(m_CurrentLeg.LegStartPnt);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(m_LegInsertPoint, new Eps_Point(0.0, 0.0, 0.0), ref _dDistance);
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(m_LegInsertPoint, new Eps_Point(0.0, 0.0, 0.0), ref _dDirection);
		m_MassLeg = m_CurrentLeg.MoveLeg(_dDirection, _dDistance, 0.0 - m_LegInsertPoint.z);
		if (m_IntersectionContainers != null && m_IntersectionContainers.Count > 0)
		{
			Eps_Point _NewPnt = new Eps_Point();
			for (int i = 0; i < m_IntersectionContainers.Count; i++)
			{
				for (int j = 0; j < m_IntersectionContainers[i].Profiles.Count; j++)
				{
					for (int k = 0; k < m_IntersectionContainers[i].Profiles[j].Points.Count; k++)
					{
						EPShared.eplGeoE_CalcPolarToXY(m_IntersectionContainers[i].Profiles[j].Points[k], EPShared.eplGeoE_NormalizeRadians(_dDirection), _dDistance, ref _NewPnt);
						_NewPnt.z -= m_LegInsertPoint.z;
						m_IntersectionContainers[i].Profiles[j].Points[k] = new Eps_Point(_NewPnt);
					}
				}
				if (m_IntersectionContainers[i].LeftOffsetLeg != null)
				{
					m_IntersectionContainers[i].LeftOffsetLeg = m_IntersectionContainers[i].LeftOffsetLeg.MoveLeg(_dDirection, _dDistance, 0.0 - m_LegInsertPoint.z);
				}
				if (m_IntersectionContainers[i].RightOffsetLeg != null)
				{
					m_IntersectionContainers[i].RightOffsetLeg = m_IntersectionContainers[i].RightOffsetLeg.MoveLeg(_dDirection, _dDistance, 0.0 - m_LegInsertPoint.z);
				}
			}
		}
		m_MassLeg = m_MassLeg.SplitArcsInLeg();
		empty = CreateElement(_ProjectDocument, ref _strAdditionalLoftID, _strSiteComponentName, 0.0, _dElevation, _bUseInternalPoint);
		if (empty == null || empty == string.Empty)
		{
			return empty;
		}
		m_ProgressBarForm = new ProgressBarForm();
		m_ProgressBarForm.ShowForm();
		m_ProgressBarForm.Refresh();
		m_ProgressBarForm.MinValue = 0;
		m_ProgressBarForm.MaxValue = 10;
		if (this is SidewalkForm)
		{
			m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingSidewalkMassMessage");
		}
		else if (this is RetainingWallForm)
		{
			m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingRetainingWallMassMessage");
		}
		else if (this is StreetForm)
		{
			m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingStreetMassMessage");
		}
		else if (this is CurbForm)
		{
			m_ProgressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarPlacingCurbMassMessage");
		}
		m_ProgressBarForm.SetMaxValue();
		Thread.Sleep(300);
		m_ProgressBarForm.Close();
		return empty;
	}

	public virtual string CreateElement(Document doc, ref string _strAdditionalLoftID, string _strSiteComponentName, double _dStartingOffset, double _dStartingElevation, bool _bUseInternalPoint)
	{
		return null;
	}

	protected ReferenceArray ApplyReferenceArrayProfile(List<Eps_Point> _PointList)
	{
		ReferenceArray referenceArray = new ReferenceArray();
		CurveArray curveArray = new CurveArray();
		Line line = null;
		int num = 0;
		for (int i = 0; i < _PointList.Count; i++)
		{
			num = ((i != _PointList.Count - 1) ? (i + 1) : 0);
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(_PointList[i], _PointList[num], 0.0025))
			{
				line = RevitUtility.CreateLine(_PointList[i].XYZ, _PointList[num].XYZ, bBound: true);
				curveArray.Append(line);
			}
		}
		CurveLoop curveLoop = new CurveLoop();
		foreach (Curve item in curveArray)
		{
			curveLoop.Append(item);
		}
		Plane plane = curveLoop.GetPlane();
		SketchPlane sketchPlane = SketchPlane.Create(m_MassDocument, plane);
		ModelCurve modelCurve = null;
		foreach (Curve item2 in curveArray)
		{
			modelCurve = m_MassDocument.FamilyCreate.NewModelCurve(item2, sketchPlane);
			referenceArray.Append(modelCurve.GeometryCurve.Reference);
		}
		return referenceArray;
	}

	protected CurveLoop ApplyCurveLoopProfile(List<Eps_Point> _PointList)
	{
		CurveArray curveArray = new CurveArray();
		Line line = null;
		int num = 0;
		for (int i = 0; i < _PointList.Count; i++)
		{
			num = ((i != _PointList.Count - 1) ? (i + 1) : 0);
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(_PointList[i], _PointList[num], 0.0025))
			{
				line = RevitUtility.CreateLine(_PointList[i].XYZ, _PointList[num].XYZ, bBound: true);
				curveArray.Append(line);
			}
		}
		CurveLoop curveLoop = new CurveLoop();
		foreach (Curve item in curveArray)
		{
			curveLoop.Append(item);
		}
		return curveLoop;
	}

	protected ReferenceArray ApplyReferenceArrayProfileAdjustingStation(double _dStation, List<Eps_Point> _PointList)
	{
		ReferenceArray referenceArray = new ReferenceArray();
		CurveArray curveArray = new CurveArray();
		Line line = null;
		double num = 0.0;
		double _dStation2 = 0.0;
		double _dOffset = 0.0;
		Eps_Point eps_Point = new Eps_Point();
		List<Eps_Point> list = new List<Eps_Point>();
		list.AddRange(_PointList);
		for (int i = 0; i < list.Count; i++)
		{
			num = list[i].z;
			RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(list[i], m_MassLeg, ref _dStation2, ref _dOffset);
			eps_Point = new Eps_Point();
			RevitGeometry.eplGeoE_FindPntOnLegByStationOffset(m_MassLeg, _dStation2 + _dStation, _dOffset, ref eps_Point);
			list[i] = eps_Point;
			list[i].z = num;
		}
		int num2 = 0;
		for (int j = 0; j < list.Count; j++)
		{
			num2 = ((j != _PointList.Count - 1) ? (j + 1) : 0);
			if (!EPShared.eplGeoE_ArePntsDuplicates3D(list[j], list[num2], 0.0025))
			{
				line = RevitUtility.CreateLine(list[j].XYZ, list[num2].XYZ, bBound: true);
				curveArray.Append(line);
			}
		}
		CurveLoop curveLoop = new CurveLoop();
		foreach (Curve item in curveArray)
		{
			curveLoop.Append(item);
		}
		Plane plane = curveLoop.GetPlane();
		SketchPlane sketchPlane = SketchPlane.Create(m_MassDocument, plane);
		ModelCurve modelCurve = null;
		foreach (Curve item2 in curveArray)
		{
			modelCurve = m_MassDocument.FamilyCreate.NewModelCurve(item2, sketchPlane);
			referenceArray.Append(modelCurve.GeometryCurve.Reference);
		}
		return referenceArray;
	}
}
