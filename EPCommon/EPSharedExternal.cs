using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EPCommon;

public class EPSharedExternal
{
	private class Externalx86
	{
		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplLM_GetLicense();

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplLM_IsNodeLicense();

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplLM_LicenseManagement();

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplLM_IsBorrowedNetworkLicense();

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplLM_BorrowNetworkLicense();

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplLM_ReturnBorrowedLicense();

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplLM_ReturnLicense();

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplDLLGeoE_ArePntsDuplicates2D(IntPtr _pFirstCheckPnt, IntPtr _pSecondCheckPnt, double _dTolerance);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_CalcHorzAngByThreePnts(IntPtr _pStartPnt, IntPtr _pVertex, IntPtr _pEndPnt, int _nAngleType, ref double _dAngle, double _dPointTol);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_NormalizeRadians(double _dAngle, ref double _dReturnValue);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_CalcHorzDirBtwnTwoPnts(IntPtr _pFirstPnt, IntPtr _pSecondPnt, ref double _dDirection, double _dMinDist);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_CalcHorzDistBtwnTwoPnts(IntPtr _pFirstPnt, IntPtr _pSecondPnt, ref double _dDistance, double _dMinDist);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_CalcPolarToXY(IntPtr _pStartPnt, double _dPolarDirection, double _dPolarDistance, IntPtr _NewPnt);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_FindStaOffPointAlongArc(IntPtr _pArc, double _dStation, double _dOffset, IntPtr _pStaOffPnt);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_FindStaOffPointAlongLine(IntPtr _pLine, double _dStation, double _dOffset, IntPtr _pStaOffPnt);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_CalcTangentDirOfArc(IntPtr _pArcPnt, IntPtr _pRadiusPnt, int _nArcDirection, ref double _dTangentDir);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_CalcStationOffsetAlongLine(IntPtr _pPoint, IntPtr _pLine, ref double _pStation, ref double _pOffset, double _dStaTolerance);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_CalcStationOffsetAlongArc(IntPtr _pPoint, IntPtr _pArc, ref double _pStation, ref double _pOffset, double _dStaTolerance);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_SolveArcLenByRadiusAndDelta(double _dRadius, double _dDeltaAngle, ref double dReturnValue);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_ConstructOffsetArc(IntPtr _pOrigArc, double _dOffsetDist, int _nOffsetDir, IntPtr _OffsetArc);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_ConstructOffsetLine(IntPtr _pOrigLine, double _dOffsetDist, int _nOffsetDir, IntPtr _OffsetLine);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_CalcMidPointBetweenTwoPoints(IntPtr _pFirstPoint, IntPtr _pSecondPoint, IntPtr _pMidPoint);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_IntersectTwoDirs2D(IntPtr _pFirstPnt, double _dFirstDir, IntPtr _pSecondPnt, double _dSecondDir, IntPtr _pInterPnt, double _dTolerance);

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplDLLGeoE_ArePntsDuplicates3D(IntPtr _pFirstCheckPnt, IntPtr _pSecondCheckPnt, double _dTolerance);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_CalcArcStructFromPCAndPTAndRP(IntPtr _pFirstPnt, IntPtr _pRadiusPoint, IntPtr _pPntOfTangency, int _nDirection, IntPtr _pArcStruct);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_FindMinMaxOfArc(IntPtr _pArc, IntPtr _pMinPnt, IntPtr _pMaxPnt);

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplDLLGeoE_IsPointOnArc2D(IntPtr _pPoint, IntPtr pArc, double _dPointTolerance);

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplDLLGeoE_IsPointOnLine2D(IntPtr _pPoint, IntPtr pLine, double _dPointTolerance);

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplDLLGeoE_DoBoundingBoxesOverlap(IntPtr _pLineOne, IntPtr _pLineTwo, double _dPointTolerance);

		[DllImport("SITESWrapperx86.dll")]
		public static extern double eplDLLGeoE_CalcPlaneDistBtwnTwoPnts(double _dFirstPntRightLeft, double _dFirstPntUpDown, double _dSecondPntRightLeft, double _dSecondPntUpDown, ref double dReturnValue);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_MakeVector(IntPtr _pInitialPnt, IntPtr _pFinalPnt, IntPtr _pVector);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_CheckIfLineInsideLine(IntPtr _pLineOne, IntPtr _pLineTwo, double _dPointTolerance);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_SolveDeltaByChordLenAndRadius(double _dChordLength, double _dRadius, ref double _dDeltaAngle);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_SolveTanLenByRadiusAndDelta(double _dRadius, double _dDeltaAngle, ref double dReturnValue);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_CalcRadiusPointFromPCAndPIAndPT(IntPtr _pPntOfCurve, IntPtr _pPntOfInter, IntPtr _pPntOfTangency, IntPtr _pRadiusPoint);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_RotatePoint(IntPtr _pPoint, IntPtr _pReferencePoint, double _dDirection);

		[DllImport("SITESWrapperx86.dll")]
		public static extern int eplDLLGeoE_CalcVertDirBtwnTwoPnts(IntPtr _pFirstPnt, IntPtr _pSecondPnt, ref double _dDirection, double _dMinDist);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_CalcPolarToXYZ(IntPtr pStartPnt, double _dHorzPolarDirection, double _dVertPolarDirection, double _dPolarSlopeDistance, IntPtr _NewPnt);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_CalcXYZToPolar(IntPtr pStartPnt, IntPtr pEndPnt, ref double _dHorzPolarDirection, ref double _dVertPolarDirection, ref double _dPolarSlopeDistance);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_CalcTangentInDirOfArc(IntPtr pArc, ref double _dTangentInDir);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_CalcTangentOutDirOfArc(IntPtr pArc, ref double _dTangentOutDir);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLGeoE_CalcArcChordArea(IntPtr pArc, ref double _dChordArea);

		[DllImport("SITESWrapperx86.dll")]
		public static extern bool eplDLLGeoE_IsLegListInsideLegList(IntPtr _pTestLines, int _nTestLineCount, IntPtr _pTestArcs, int _nTestArcCount, IntPtr _pTestLegOrder, IntPtr _pLines, int _nLineCount, IntPtr _pArcs, int _nArcCount, IntPtr _pLegOrder, double _dTolerance);

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Auto)]
		public static extern void eplDLLFilP_SetRegAppProgramKey(string _strKey);

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetSupportPath(string _strFilename, StringBuilder _SupportPath);

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetProgramPath(string _strFilename, StringBuilder _ProgramPath);

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetHelpPath(string _strFilename, StringBuilder _HelpPath);

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetImagePath(string _strFilename, StringBuilder _ImagePath);

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetSymbolsPath(string _strFilename, StringBuilder _SymbolsPath);

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetTutorialPath(string _strFilename, StringBuilder _TutorialPath);

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetLibraryPath(string _strFilename, StringBuilder _LibraryPath);

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetSamplePath(string _strFilename, StringBuilder _SamplePath);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLCalE_fmod(double _dFirstOperand, double _dSecondOperand, double _dTolerance, ref double _dReturnValue);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLCalE_RoundValue(double _dValue, int _nPrecision, ref double _dRoundedValue);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLLCalE_CalcComponentKOfCrossProduct(IntPtr _pFirstPoint, IntPtr _pSecondPoint, ref double _dReturnValue);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLL_SetRevitVersion(int _nRevitVersion);

		[DllImport("SITESWrapperx86.dll")]
		public static extern void eplDLL_UnloadHelper();

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Unicode)]
		public static extern void epl_CreateCatchLinesAndFeatureLinesFromTopo(string _strFileName, IntPtr _pLines, int _nLineCount, IntPtr _pArcs, int _nArcCount, IntPtr _pLegOrder, IntPtr _pPnts, int _nPointCount, double _dLinearInterval, double _dRadialInterval, double _dArcPointInterval, double _dCutSlope, double _dFillSlope, bool _bProjectLeft, ref IntPtr _aLeftCatchLines, ref int _nNumLeftCatchLines, ref IntPtr _aFeatureLines, ref int _nFeatureLines);

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Unicode)]
		public static extern void epl_CreateCutAndFillFromTopo(string _strFileName, IntPtr _pLines, int _nLineCount, IntPtr _pArcs, int _nArcCount, IntPtr _pLegOrder, IntPtr _pPnts, int _nPointCount, double _dLinearInterval, double _dRadialInterval, double _dArcPointInterval, double _dLeftOffsetWidth, double _dRightOffsetWidth, double _dBoulevardWidth, double _dCutSlope, double _dFillSlope, int _nProjectInnerSlopes, double _dSubgradeAdjustElev, bool _bIsCrownPresent, bool _bIsProjectLegToTopo, double _dLeftEdgeAdjustElev, double _dRightEdgeAdjustElev, double _dLeftSubgradeAdjustElev, double _dRightSubgradeAdjustElev, ref IntPtr _aCutFillPoints, ref int _nNumCutFillPoints, ref IntPtr _aLeftCatchLines, ref int _nNumLeftCatchLines, ref IntPtr _aRightCatchLines, ref int _nNumRightCatchLines, ref IntPtr _aLeftCutFillLines, ref int _nNumLeftCutFillLines, ref IntPtr _aRightCutFillLines, ref int _nNumRightCutFillLines);

		[DllImport("SITESWrapperx86.dll", CharSet = CharSet.Unicode)]
		public static extern void epl_GenerateTrianglesFromTopo(string _strFileName, IntPtr _pPnts, int _nPointCount, ref IntPtr _aFirstTrianglePnts, ref IntPtr _aSecondTrianglePnts, ref IntPtr _aThirdTrianglePnts, ref int _nNumTriangles);
	}

	private class Externalx64
	{
		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplLM_GetLicense();

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplLM_IsNodeLicense();

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplLM_LicenseManagement();

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplLM_IsBorrowedNetworkLicense();

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplLM_BorrowNetworkLicense();

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplLM_ReturnBorrowedLicense();

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplLM_ReturnLicense();

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplDLLGeoE_ArePntsDuplicates2D(IntPtr _pFirstCheckPnt, IntPtr _pSecondCheckPnt, double _dTolerance);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_CalcHorzAngByThreePnts(IntPtr _pStartPnt, IntPtr _pVertex, IntPtr _pEndPnt, int _nAngleType, ref double _dAngle, double _dPointTol);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_NormalizeRadians(double _dAngle, ref double _dReturnValue);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_CalcHorzDirBtwnTwoPnts(IntPtr _pFirstPnt, IntPtr _pSecondPnt, ref double _dDirection, double _dMinDist);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_CalcHorzDistBtwnTwoPnts(IntPtr _pFirstPnt, IntPtr _pSecondPnt, ref double _dDistance, double _dMinDist);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_CalcPolarToXY(IntPtr _pStartPnt, double _dPolarDirection, double _dPolarDistance, IntPtr _NewPnt);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_FindStaOffPointAlongArc(IntPtr _pArc, double _dStation, double _dOffset, IntPtr _pStaOffPnt);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_FindStaOffPointAlongLine(IntPtr _pLine, double _dStation, double _dOffset, IntPtr _pStaOffPnt);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_CalcTangentDirOfArc(IntPtr _pArcPnt, IntPtr _pRadiusPnt, int _nArcDirection, ref double _dTangentDir);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_CalcStationOffsetAlongLine(IntPtr _pPoint, IntPtr _pLine, ref double _pStation, ref double _pOffset, double _dStaTolerance);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_CalcStationOffsetAlongArc(IntPtr _pPoint, IntPtr _pArc, ref double _pStation, ref double _pOffset, double _dStaTolerance);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_SolveArcLenByRadiusAndDelta(double _dRadius, double _dDeltaAngle, ref double dReturnValue);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_ConstructOffsetArc(IntPtr _pOrigArc, double _dOffsetDist, int _nOffsetDir, IntPtr _OffsetArc);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_ConstructOffsetLine(IntPtr _pOrigLine, double _dOffsetDist, int _nOffsetDir, IntPtr _OffsetLine);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_CalcMidPointBetweenTwoPoints(IntPtr _pFirstPoint, IntPtr _pSecondPoint, IntPtr _pMidPoint);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_IntersectTwoDirs2D(IntPtr _pFirstPnt, double _dFirstDir, IntPtr _pSecondPnt, double _dSecondDir, IntPtr _pInterPnt, double _dTolerance);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplDLLGeoE_ArePntsDuplicates3D(IntPtr _pFirstCheckPnt, IntPtr _pSecondCheckPnt, double _dTolerance);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_CalcArcStructFromPCAndPTAndRP(IntPtr _pFirstPnt, IntPtr _pRadiusPoint, IntPtr _pPntOfTangency, int _nDirection, IntPtr _pArcStruct);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_FindMinMaxOfArc(IntPtr _pArc, IntPtr _pMinPnt, IntPtr _pMaxPnt);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplDLLGeoE_IsPointOnArc2D(IntPtr _pPoint, IntPtr pArc, double _dPointTolerance);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplDLLGeoE_IsPointOnLine2D(IntPtr _pPoint, IntPtr pLine, double _dPointTolerance);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplDLLGeoE_DoBoundingBoxesOverlap(IntPtr _pLineOne, IntPtr _pLineTwo, double _dPointTolerance);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern double eplDLLGeoE_CalcPlaneDistBtwnTwoPnts(double _dFirstPntRightLeft, double _dFirstPntUpDown, double _dSecondPntRightLeft, double _dSecondPntUpDown, ref double dReturnValue);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_MakeVector(IntPtr _pInitialPnt, IntPtr _pFinalPnt, IntPtr _pVector);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_CheckIfLineInsideLine(IntPtr _pLineOne, IntPtr _pLineTwo, double _dPointTolerance);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_SolveDeltaByChordLenAndRadius(double _dChordLength, double _dRadius, ref double _dDeltaAngle);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_SolveTanLenByRadiusAndDelta(double _dRadius, double _dDeltaAngle, ref double dReturnValue);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_CalcRadiusPointFromPCAndPIAndPT(IntPtr _pPntOfCurve, IntPtr _pPntOfInter, IntPtr _pPntOfTangency, IntPtr _pRadiusPoint);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_RotatePoint(IntPtr _pPoint, IntPtr _pReferencePoint, double _dDirection);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern int eplDLLGeoE_CalcVertDirBtwnTwoPnts(IntPtr _pFirstPnt, IntPtr _pSecondPnt, ref double _dDirection, double _dMinDist);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_CalcPolarToXYZ(IntPtr pStartPnt, double _dHorzPolarDirection, double _dVertPolarDirection, double _dPolarSlopeDistance, IntPtr _NewPnt);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_CalcXYZToPolar(IntPtr pStartPnt, IntPtr pEndPnt, ref double _dHorzPolarDirection, ref double _dVertPolarDirection, ref double _dPolarSlopeDistance);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_CalcTangentInDirOfArc(IntPtr pArc, ref double _dTangentInDir);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_CalcTangentOutDirOfArc(IntPtr pArc, ref double _dTangentOutDir);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLGeoE_CalcArcChordArea(IntPtr pArc, ref double _dChordArea);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern bool eplDLLGeoE_IsLegListInsideLegList(IntPtr _pTestLines, int _nTestLineCount, IntPtr _pTestArcs, int _nTestArcCount, IntPtr _pTestLegOrder, IntPtr _pLines, int _nLineCount, IntPtr _pArcs, int _nArcCount, IntPtr _pLegOrder, double _dTolerance);

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Auto)]
		public static extern void eplDLLFilP_SetRegAppProgramKey(string _strKey);

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetSupportPath(string _strFilename, StringBuilder _SupportPath);

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetProgramPath(string _strFilename, StringBuilder _ProgramPath);

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetHelpPath(string _strFilename, StringBuilder _HelpPath);

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetImagePath(string _strFilename, StringBuilder _ImagePath);

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetSymbolsPath(string _strFilename, StringBuilder _SymbolsPath);

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetTutorialPath(string _strFilename, StringBuilder _TutorialPath);

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetLibraryPath(string _strFilename, StringBuilder _LibraryPath);

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Auto)]
		public static extern int eplDLLFilP_GetSamplePath(string _strFilename, StringBuilder _SamplePath);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLCalE_fmod(double _dFirstOperand, double _dSecondOperand, double _dTolerance, ref double _dReturnValue);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLCalE_RoundValue(double _dValue, int _nPrecision, ref double _dRoundedValue);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLLCalE_CalcComponentKOfCrossProduct(IntPtr _pFirstPoint, IntPtr _pSecondPoint, ref double _dReturnValue);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLL_SetRevitVersion(int _nRevitVersion);

		[DllImport("SitesDLLWrapper.dll")]
		public static extern void eplDLL_UnloadHelper();

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Unicode)]
		public static extern void epl_CreateCatchLinesAndFeatureLinesFromTopo(string _strFileName, IntPtr _pLines, int _nLineCount, IntPtr _pArcs, int _nArcCount, IntPtr _pLegOrder, IntPtr _pPnts, int _nPointCount, double _dLinearInterval, double _dRadialInterval, double _dArcPointInterval, double _dCutSlope, double _dFillSlope, bool _bProjectLeft, ref IntPtr _aLeftCatchLines, ref int _nNumLeftCatchLines, ref IntPtr _aFeatureLines, ref int _nFeatureLines);

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Unicode)]
		public static extern void epl_CreateCutAndFillFromTopo(string _strFileName, IntPtr _pLines, int _nLineCount, IntPtr _pArcs, int _nArcCount, IntPtr _pLegOrder, IntPtr _pPnts, int _nPointCount, double _dLinearInterval, double _dRadialInterval, double _dArcPointInterval, double _dLeftOffsetWidth, double _dRightOffsetWidth, double _dBoulevardWidth, double _dCutSlope, double _dFillSlope, int _nProjectInnerSlopes, double _dSubgradeAdjustElev, bool _bIsCrownPresent, bool _bIsProjectLegToTopo, double _dLeftEdgeAdjustElev, double _dRightEdgeAdjustElev, double _dLeftSubgradeAdjustElev, double _dRightSubgradeAdjustElev, ref IntPtr _aCutFillPoints, ref int _nNumCutFillPoints, ref IntPtr _aLeftCatchLines, ref int _nNumLeftCatchLines, ref IntPtr _aRightCatchLines, ref int _nNumRightCatchLines, ref IntPtr _aLeftCutFillLines, ref int _nNumLeftCutFillLines, ref IntPtr _aRightCutFillLines, ref int _nNumRightCutFillLines);

		[DllImport("SitesDLLWrapper.dll", CharSet = CharSet.Unicode)]
		public static extern void epl_GenerateTrianglesFromTopo(string _strFileName, IntPtr _pPnts, int _nPointCount, ref IntPtr _aFirstTrianglePnts, ref IntPtr _aSecondTrianglePnts, ref IntPtr _aThirdTrianglePnts, ref int _nNumTriangles);
	}

	public const int m_nMaxStringBuilderSize = 32767;

	public static bool eplLicM_GetLicense()
	{
		if (!ApplicationUtility.Is64BitInstall)
		{
			return Externalx86.eplLM_GetLicense();
		}
		return Externalx64.eplLM_GetLicense();
	}

	public static bool eplLicM_IsNodeLicense()
	{
		if (!ApplicationUtility.Is64BitInstall)
		{
			return Externalx86.eplLM_IsNodeLicense();
		}
		return Externalx64.eplLM_IsNodeLicense();
	}

	public static bool eplLicM_LicenseManagement()
	{
		if (!ApplicationUtility.Is64BitInstall)
		{
			return Externalx86.eplLM_LicenseManagement();
		}
		return Externalx64.eplLM_LicenseManagement();
	}

	public static bool eplLicM_IsBorrowedNetworkLicense()
	{
		if (!ApplicationUtility.Is64BitInstall)
		{
			return Externalx86.eplLM_IsBorrowedNetworkLicense();
		}
		return Externalx64.eplLM_IsBorrowedNetworkLicense();
	}

	public static bool eplLicM_BorrowNetworkLicense()
	{
		if (!ApplicationUtility.Is64BitInstall)
		{
			return Externalx86.eplLM_BorrowNetworkLicense();
		}
		return Externalx64.eplLM_BorrowNetworkLicense();
	}

	public static bool eplLicM_ReturnBorrowedLicense()
	{
		if (!ApplicationUtility.Is64BitInstall)
		{
			return Externalx86.eplLM_ReturnBorrowedLicense();
		}
		return Externalx64.eplLM_ReturnBorrowedLicense();
	}

	public static bool eplLicM_ReturnLicense()
	{
		if (!ApplicationUtility.Is64BitInstall)
		{
			return Externalx86.eplLM_ReturnLicense();
		}
		return Externalx64.eplLM_ReturnLicense();
	}

	public static bool eplGeoE_ArePntsDuplicates2D(Eps_Point _FirstCheckPnt, Eps_Point _SecondCheckPnt)
	{
		return eplGeoE_ArePntsDuplicates2D(_FirstCheckPnt, _SecondCheckPnt, 1E-05);
	}

	public static bool eplGeoE_ArePntsDuplicates2D(Eps_Point _FirstCheckPnt, Eps_Point _SecondCheckPnt, double _dTolerance)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_FirstCheckPnt));
		Marshal.StructureToPtr(_FirstCheckPnt, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_SecondCheckPnt));
		Marshal.StructureToPtr(_SecondCheckPnt, intPtr2, fDeleteOld: true);
		bool flag = false;
		flag = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_ArePntsDuplicates2D(intPtr, intPtr2, _dTolerance) : Externalx86.eplDLLGeoE_ArePntsDuplicates2D(intPtr, intPtr2, _dTolerance));
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return flag;
	}

	public static int eplGeoE_CalcHorzAngByThreePnts(Eps_Point _StartPnt, Eps_Point _Vertex, Eps_Point _EndPnt, int _nAngleType, ref double _dAngle, double _dPointTol)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_StartPnt));
		Marshal.StructureToPtr(_StartPnt, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_Vertex));
		Marshal.StructureToPtr(_Vertex, intPtr2, fDeleteOld: true);
		IntPtr intPtr3 = Marshal.AllocHGlobal(Marshal.SizeOf(_EndPnt));
		Marshal.StructureToPtr(_EndPnt, intPtr3, fDeleteOld: true);
		int num = 0;
		num = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_CalcHorzAngByThreePnts(intPtr, intPtr2, intPtr3, _nAngleType, ref _dAngle, _dPointTol) : Externalx86.eplDLLGeoE_CalcHorzAngByThreePnts(intPtr, intPtr2, intPtr3, _nAngleType, ref _dAngle, _dPointTol));
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		Marshal.FreeHGlobal(intPtr3);
		return num;
	}

	public static double eplGeoE_NormalizeRadians(double _dAngle)
	{
		double _dReturnValue = 0.0;
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_NormalizeRadians(_dAngle, ref _dReturnValue);
		}
		else
		{
			Externalx64.eplDLLGeoE_NormalizeRadians(_dAngle, ref _dReturnValue);
		}
		return _dReturnValue;
	}

	public static int eplGeoE_CalcHorzDirBtwnTwoPnts(Eps_Point _FirstPnt, Eps_Point _SecondPnt, ref double _dDirection)
	{
		return eplGeoE_CalcHorzDirBtwnTwoPnts(_FirstPnt, _SecondPnt, ref _dDirection, 1E-05);
	}

	public static int eplGeoE_CalcHorzDirBtwnTwoPnts(Eps_Point _FirstPnt, Eps_Point _SecondPnt, ref double _dDirection, double _dMinDist)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_FirstPnt));
		Marshal.StructureToPtr(_FirstPnt, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_SecondPnt));
		Marshal.StructureToPtr(_SecondPnt, intPtr2, fDeleteOld: true);
		int num = 0;
		num = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_CalcHorzDirBtwnTwoPnts(intPtr, intPtr2, ref _dDirection, _dMinDist) : Externalx86.eplDLLGeoE_CalcHorzDirBtwnTwoPnts(intPtr, intPtr2, ref _dDirection, _dMinDist));
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return num;
	}

	public static int eplGeoE_CalcHorzDistBtwnTwoPnts(Eps_Point _FirstPnt, Eps_Point _SecondPnt, ref double _dDistance)
	{
		return eplGeoE_CalcHorzDistBtwnTwoPnts(_FirstPnt, _SecondPnt, ref _dDistance, 1E-05);
	}

	public static int eplGeoE_CalcHorzDistBtwnTwoPnts(Eps_Point _FirstPnt, Eps_Point _SecondPnt, ref double _dDistance, double _dMinDist)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_FirstPnt));
		Marshal.StructureToPtr(_FirstPnt, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_SecondPnt));
		Marshal.StructureToPtr(_SecondPnt, intPtr2, fDeleteOld: true);
		int num = 0;
		num = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_CalcHorzDistBtwnTwoPnts(intPtr, intPtr2, ref _dDistance, _dMinDist) : Externalx86.eplDLLGeoE_CalcHorzDistBtwnTwoPnts(intPtr, intPtr2, ref _dDistance, _dMinDist));
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return num;
	}

	public static void eplGeoE_CalcPolarToXY(Eps_Point _StartPnt, double _dPolarDirection, double _dPolarDistance, ref Eps_Point _NewPnt)
	{
		_NewPnt = new Eps_Point();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_StartPnt));
		Marshal.StructureToPtr(_StartPnt, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_NewPnt));
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_CalcPolarToXY(intPtr, _dPolarDirection, _dPolarDistance, intPtr2);
		}
		else
		{
			Externalx64.eplDLLGeoE_CalcPolarToXY(intPtr, _dPolarDirection, _dPolarDistance, intPtr2);
		}
		Marshal.PtrToStructure(intPtr2, _NewPnt);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
	}

	public static void eplGeoE_FindStaOffPointAlongArc(Eps_Arc _Arc, double _dStation, double _dOffset, ref Eps_Point _StaOffPnt)
	{
		_StaOffPnt = new Eps_Point();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((BaseArc)_Arc));
		Marshal.StructureToPtr(_Arc, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_StaOffPnt));
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_FindStaOffPointAlongArc(intPtr, _dStation, _dOffset, intPtr2);
		}
		else
		{
			Externalx64.eplDLLGeoE_FindStaOffPointAlongArc(intPtr, _dStation, _dOffset, intPtr2);
		}
		Marshal.PtrToStructure(intPtr2, _StaOffPnt);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
	}

	public static void eplGeoE_FindStaOffPointAlongLine(Eps_Line _Line, double _dStation, double _dOffset, ref Eps_Point _StaOffPnt)
	{
		_StaOffPnt = new Eps_Point();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((BaseLine)_Line));
		Marshal.StructureToPtr(_Line, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_StaOffPnt));
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_FindStaOffPointAlongLine(intPtr, _dStation, _dOffset, intPtr2);
		}
		else
		{
			Externalx64.eplDLLGeoE_FindStaOffPointAlongLine(intPtr, _dStation, _dOffset, intPtr2);
		}
		Marshal.PtrToStructure(intPtr2, _StaOffPnt);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
	}

	public static void eplGeoE_CalcTangentDirOfArc(Eps_Point _ArcPnt, Eps_Point _RadiusPnt, int _nArcDirection, ref double _dTangentDir)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_ArcPnt));
		Marshal.StructureToPtr(_ArcPnt, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_RadiusPnt));
		Marshal.StructureToPtr(_RadiusPnt, intPtr2, fDeleteOld: true);
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_CalcTangentDirOfArc(intPtr, intPtr2, _nArcDirection, ref _dTangentDir);
		}
		else
		{
			Externalx64.eplDLLGeoE_CalcTangentDirOfArc(intPtr, intPtr2, _nArcDirection, ref _dTangentDir);
		}
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
	}

	public static int eplGeoE_CalcStationOffsetAlongLine(Eps_Point _Point, Eps_Line _Line, ref double _dStation, ref double _dOffset, double _dStaTolerance)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_Point));
		Marshal.StructureToPtr(_Point, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf((BaseLine)_Line));
		Marshal.StructureToPtr(_Line, intPtr2, fDeleteOld: true);
		int num = 0;
		num = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_CalcStationOffsetAlongLine(intPtr, intPtr2, ref _dStation, ref _dOffset, _dStaTolerance) : Externalx86.eplDLLGeoE_CalcStationOffsetAlongLine(intPtr, intPtr2, ref _dStation, ref _dOffset, _dStaTolerance));
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return num;
	}

	public static int eplGeoE_CalcStationOffsetAlongArc(Eps_Point _Point, Eps_Arc _Arc, ref double _dStation, ref double _dOffset)
	{
		return eplGeoE_CalcStationOffsetAlongArc(_Point, _Arc, ref _dStation, ref _dOffset, 1E-05);
	}

	public static int eplGeoE_CalcStationOffsetAlongArc(Eps_Point _Point, Eps_Arc _Arc, ref double _dStation, ref double _dOffset, double _dStaTolerance)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_Point));
		Marshal.StructureToPtr(_Point, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf((BaseArc)_Arc));
		Marshal.StructureToPtr(_Arc, intPtr2, fDeleteOld: true);
		int num = 0;
		num = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_CalcStationOffsetAlongArc(intPtr, intPtr2, ref _dStation, ref _dOffset, _dStaTolerance) : Externalx86.eplDLLGeoE_CalcStationOffsetAlongArc(intPtr, intPtr2, ref _dStation, ref _dOffset, _dStaTolerance));
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return num;
	}

	public static double eplGeoE_SolveArcLenByRadiusAndDelta(double _Radius, double _DeltaAngle)
	{
		double dReturnValue = 0.0;
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_SolveArcLenByRadiusAndDelta(_Radius, _DeltaAngle, ref dReturnValue);
		}
		else
		{
			Externalx64.eplDLLGeoE_SolveArcLenByRadiusAndDelta(_Radius, _DeltaAngle, ref dReturnValue);
		}
		return dReturnValue;
	}

	public static int eplGeoE_ConstructOffsetArc(Eps_Arc _OrigArc, double _dOffsetDist, int _nOffsetDir, ref Eps_Arc _OffsetArc)
	{
		_OffsetArc = new Eps_Arc();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((BaseArc)_OrigArc));
		Marshal.StructureToPtr(_OrigArc, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf((BaseArc)_OffsetArc));
		int num = 0;
		num = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_ConstructOffsetArc(intPtr, _dOffsetDist, _nOffsetDir, intPtr2) : Externalx86.eplDLLGeoE_ConstructOffsetArc(intPtr, _dOffsetDist, _nOffsetDir, intPtr2));
		BaseArc baseArc = new BaseArc();
		Marshal.PtrToStructure(intPtr2, baseArc);
		_OffsetArc = new Eps_Arc(baseArc);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return num;
	}

	public static int eplGeoE_ConstructOffsetLine(Eps_Line _OrigLine, double _dOffsetDist, int _nOffsetDir, ref Eps_Line _OffsetLine)
	{
		_OffsetLine = new Eps_Line();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((BaseLine)_OrigLine));
		Marshal.StructureToPtr(_OrigLine, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf((BaseLine)_OffsetLine));
		int num = 0;
		num = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_ConstructOffsetLine(intPtr, _dOffsetDist, _nOffsetDir, intPtr2) : Externalx86.eplDLLGeoE_ConstructOffsetLine(intPtr, _dOffsetDist, _nOffsetDir, intPtr2));
		BaseLine baseLine = new BaseLine();
		Marshal.PtrToStructure(intPtr2, baseLine);
		_OffsetLine = new Eps_Line(baseLine);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return num;
	}

	public static int eplGeoE_CalcMidPointBetweenTwoPoints(Eps_Point _FirstPoint, Eps_Point _SecondPoint, ref Eps_Point _MidPoint)
	{
		_MidPoint = new Eps_Point();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_FirstPoint));
		Marshal.StructureToPtr(_FirstPoint, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_SecondPoint));
		Marshal.StructureToPtr(_SecondPoint, intPtr2, fDeleteOld: true);
		IntPtr intPtr3 = Marshal.AllocHGlobal(Marshal.SizeOf(_MidPoint));
		int num = 0;
		num = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_CalcMidPointBetweenTwoPoints(intPtr, intPtr2, intPtr3) : Externalx86.eplDLLGeoE_CalcMidPointBetweenTwoPoints(intPtr, intPtr2, intPtr3));
		Marshal.PtrToStructure(intPtr3, _MidPoint);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		Marshal.FreeHGlobal(intPtr3);
		return num;
	}

	public static int eplGeoE_IntersectTwoDirs2D(Eps_Point _FirstPnt, double _dFirstDir, Eps_Point _SecondPnt, double _dSecondDir, ref Eps_Point _InterPnt, double _dTolerance)
	{
		_InterPnt = new Eps_Point();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_FirstPnt));
		Marshal.StructureToPtr(_FirstPnt, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_SecondPnt));
		Marshal.StructureToPtr(_SecondPnt, intPtr2, fDeleteOld: true);
		IntPtr intPtr3 = Marshal.AllocHGlobal(Marshal.SizeOf(_InterPnt));
		int num = 0;
		num = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_IntersectTwoDirs2D(intPtr, _dFirstDir, intPtr2, _dSecondDir, intPtr3, _dTolerance) : Externalx86.eplDLLGeoE_IntersectTwoDirs2D(intPtr, _dFirstDir, intPtr2, _dSecondDir, intPtr3, _dTolerance));
		Marshal.PtrToStructure(intPtr3, _InterPnt);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		Marshal.FreeHGlobal(intPtr3);
		return num;
	}

	public static bool eplGeoE_ArePntsDuplicates3D(Eps_Point _FirstCheckPnt, Eps_Point _SecondCheckPnt)
	{
		return eplGeoE_ArePntsDuplicates3D(_FirstCheckPnt, _SecondCheckPnt, 1E-05);
	}

	public static bool eplGeoE_ArePntsDuplicates3D(Eps_Point _FirstCheckPnt, Eps_Point _SecondCheckPnt, double _dTolerance)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_FirstCheckPnt));
		Marshal.StructureToPtr(_FirstCheckPnt, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_SecondCheckPnt));
		Marshal.StructureToPtr(_SecondCheckPnt, intPtr2, fDeleteOld: true);
		bool flag = false;
		flag = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_ArePntsDuplicates3D(intPtr, intPtr2, _dTolerance) : Externalx86.eplDLLGeoE_ArePntsDuplicates3D(intPtr, intPtr2, _dTolerance));
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return flag;
	}

	public static void eplGeoE_CalcArcStructFromPCAndPTAndRP(Eps_Point _PntOfArc, Eps_Point _RadiusPoint, Eps_Point _PntOfTangency, int _nDirection, ref Eps_Arc _ArcStruct)
	{
		_ArcStruct = new Eps_Arc();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_PntOfArc));
		Marshal.StructureToPtr(_PntOfArc, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_RadiusPoint));
		Marshal.StructureToPtr(_RadiusPoint, intPtr2, fDeleteOld: true);
		IntPtr intPtr3 = Marshal.AllocHGlobal(Marshal.SizeOf(_PntOfTangency));
		Marshal.StructureToPtr(_PntOfTangency, intPtr3, fDeleteOld: true);
		IntPtr intPtr4 = Marshal.AllocHGlobal(Marshal.SizeOf((BaseArc)_ArcStruct));
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_CalcArcStructFromPCAndPTAndRP(intPtr, intPtr2, intPtr3, _nDirection, intPtr4);
		}
		else
		{
			Externalx64.eplDLLGeoE_CalcArcStructFromPCAndPTAndRP(intPtr, intPtr2, intPtr3, _nDirection, intPtr4);
		}
		BaseArc baseArc = new BaseArc();
		Marshal.PtrToStructure(intPtr4, baseArc);
		_ArcStruct = new Eps_Arc(baseArc);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		Marshal.FreeHGlobal(intPtr3);
		Marshal.FreeHGlobal(intPtr4);
	}

	public static void eplGeoE_FindMinMaxOfArc(Eps_Arc _Arc, ref Eps_Point _MinPnt, ref Eps_Point _MaxPnt)
	{
		_MinPnt = new Eps_Point();
		_MaxPnt = new Eps_Point();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((BaseArc)_Arc));
		Marshal.StructureToPtr(_Arc, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_MinPnt));
		IntPtr intPtr3 = Marshal.AllocHGlobal(Marshal.SizeOf(_MaxPnt));
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_FindMinMaxOfArc(intPtr, intPtr2, intPtr3);
		}
		else
		{
			Externalx64.eplDLLGeoE_FindMinMaxOfArc(intPtr, intPtr2, intPtr3);
		}
		Marshal.PtrToStructure(intPtr2, _MinPnt);
		Marshal.PtrToStructure(intPtr3, _MaxPnt);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr3);
		Marshal.FreeHGlobal(intPtr2);
	}

	public static bool eplGeoE_IsPointOnArc2D(Eps_Point _Point, Eps_Arc _Arc, double _dPointTolerance)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_Point));
		Marshal.StructureToPtr(_Point, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf((BaseArc)_Arc));
		Marshal.StructureToPtr(_Arc, intPtr2, fDeleteOld: true);
		bool flag = false;
		flag = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_IsPointOnArc2D(intPtr, intPtr2, _dPointTolerance) : Externalx86.eplDLLGeoE_IsPointOnArc2D(intPtr, intPtr2, _dPointTolerance));
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return flag;
	}

	public static bool eplGeoE_IsPointOnLine2D(Eps_Point _Point, Eps_Line _Line, double _dPointTolerance)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_Point));
		Marshal.StructureToPtr(_Point, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf((BaseLine)_Line));
		Marshal.StructureToPtr(_Line, intPtr2, fDeleteOld: true);
		bool flag = false;
		flag = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_IsPointOnLine2D(intPtr, intPtr2, _dPointTolerance) : Externalx86.eplDLLGeoE_IsPointOnLine2D(intPtr, intPtr2, _dPointTolerance));
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return flag;
	}

	public static bool eplGeoE_DoBoundingBoxesOverlap(Eps_Line _LineOne, Eps_Line _LineTwo, double _dPointTolerance)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((BaseLine)_LineOne));
		Marshal.StructureToPtr(_LineOne, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf((BaseLine)_LineTwo));
		Marshal.StructureToPtr(_LineTwo, intPtr2, fDeleteOld: true);
		bool flag = false;
		flag = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_DoBoundingBoxesOverlap(intPtr, intPtr2, _dPointTolerance) : Externalx86.eplDLLGeoE_DoBoundingBoxesOverlap(intPtr, intPtr2, _dPointTolerance));
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return flag;
	}

	public static double eplGeoE_CalcPlaneDistBtwnTwoPnts(double _dFirstPntRightLeft, double _dFirstPntUpDown, double _dSecondPntRightLeft, double _dSecondPntUpDown)
	{
		double dReturnValue = 0.0;
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_CalcPlaneDistBtwnTwoPnts(_dFirstPntRightLeft, _dFirstPntUpDown, _dSecondPntRightLeft, _dSecondPntUpDown, ref dReturnValue);
		}
		else
		{
			Externalx64.eplDLLGeoE_CalcPlaneDistBtwnTwoPnts(_dFirstPntRightLeft, _dFirstPntUpDown, _dSecondPntRightLeft, _dSecondPntUpDown, ref dReturnValue);
		}
		return dReturnValue;
	}

	public static void eplGeoE_MakeVector(Eps_Point _InitialPnt, Eps_Point _FinalPnt, ref Eps_Point _Vector)
	{
		_Vector = new Eps_Point();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_InitialPnt));
		Marshal.StructureToPtr(_InitialPnt, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_FinalPnt));
		Marshal.StructureToPtr(_FinalPnt, intPtr2, fDeleteOld: true);
		IntPtr intPtr3 = Marshal.AllocHGlobal(Marshal.SizeOf(_Vector));
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_MakeVector(intPtr, intPtr2, intPtr3);
		}
		else
		{
			Externalx64.eplDLLGeoE_MakeVector(intPtr, intPtr2, intPtr3);
		}
		Marshal.PtrToStructure(intPtr3, _Vector);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		Marshal.FreeHGlobal(intPtr3);
	}

	public static int eplGeoE_CheckIfLineInsideLine(Eps_Line _LineOne, Eps_Line _LineTwo, double _dPointTolerance)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((BaseLine)_LineOne));
		Marshal.StructureToPtr(_LineOne, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf((BaseLine)_LineTwo));
		Marshal.StructureToPtr(_LineTwo, intPtr2, fDeleteOld: true);
		int num = 0;
		num = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_CheckIfLineInsideLine(intPtr, intPtr2, _dPointTolerance) : Externalx86.eplDLLGeoE_CheckIfLineInsideLine(intPtr, intPtr2, _dPointTolerance));
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return num;
	}

	public static int eplGeoE_SolveDeltaByChordLenAndRadius(double _dChordLength, double _dRadius, ref double _dDeltaAngle)
	{
		if (!ApplicationUtility.Is64BitInstall)
		{
			return Externalx86.eplDLLGeoE_SolveDeltaByChordLenAndRadius(_dChordLength, _dRadius, ref _dDeltaAngle);
		}
		return Externalx64.eplDLLGeoE_SolveDeltaByChordLenAndRadius(_dChordLength, _dRadius, ref _dDeltaAngle);
	}

	public static double eplGeoE_SolveTanLenByRadiusAndDelta(double _dRadius, double _dDeltaAngle)
	{
		double dReturnValue = 0.0;
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_SolveTanLenByRadiusAndDelta(_dRadius, _dDeltaAngle, ref dReturnValue);
		}
		else
		{
			Externalx64.eplDLLGeoE_SolveTanLenByRadiusAndDelta(_dRadius, _dDeltaAngle, ref dReturnValue);
		}
		return dReturnValue;
	}

	public static void eplGeoE_CalcRadiusPointFromPCAndPIAndPT(Eps_Point _PntOfCurve, Eps_Point _PntOfInter, Eps_Point _PntOfTangency, ref Eps_Point _RadiusPoint)
	{
		_RadiusPoint = new Eps_Point();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_PntOfCurve));
		Marshal.StructureToPtr(_PntOfCurve, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_PntOfInter));
		Marshal.StructureToPtr(_PntOfInter, intPtr2, fDeleteOld: true);
		IntPtr intPtr3 = Marshal.AllocHGlobal(Marshal.SizeOf(_PntOfTangency));
		Marshal.StructureToPtr(_PntOfTangency, intPtr3, fDeleteOld: true);
		IntPtr intPtr4 = Marshal.AllocHGlobal(Marshal.SizeOf(_RadiusPoint));
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_CalcRadiusPointFromPCAndPIAndPT(intPtr, intPtr2, intPtr3, intPtr4);
		}
		else
		{
			Externalx64.eplDLLGeoE_CalcRadiusPointFromPCAndPIAndPT(intPtr, intPtr2, intPtr3, intPtr4);
		}
		Marshal.PtrToStructure(intPtr4, _RadiusPoint);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		Marshal.FreeHGlobal(intPtr3);
		Marshal.FreeHGlobal(intPtr4);
	}

	public static void eplGeoE_RotatePoint(ref Eps_Point _Point, Eps_Point _ReferencePoint, double _dRotation)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_ReferencePoint));
		Marshal.StructureToPtr(_ReferencePoint, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_Point));
		Marshal.StructureToPtr(_Point, intPtr2, fDeleteOld: false);
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_RotatePoint(intPtr2, intPtr, _dRotation);
		}
		else
		{
			Externalx64.eplDLLGeoE_RotatePoint(intPtr2, intPtr, _dRotation);
		}
		Marshal.PtrToStructure(intPtr2, _Point);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
	}

	public static void eplGeoE_CalcPolarToXYZ(Eps_Point _StartPnt, double _dHorzPolarDirection, double _dVertPolarDirection, double _dPolarSlopeDistance, ref Eps_Point _NewPnt)
	{
		_NewPnt = new Eps_Point();
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_StartPnt));
		Marshal.StructureToPtr(_StartPnt, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_NewPnt));
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_CalcPolarToXYZ(intPtr, _dHorzPolarDirection, _dVertPolarDirection, _dPolarSlopeDistance, intPtr2);
		}
		else
		{
			Externalx64.eplDLLGeoE_CalcPolarToXYZ(intPtr, _dHorzPolarDirection, _dVertPolarDirection, _dPolarSlopeDistance, intPtr2);
		}
		Marshal.PtrToStructure(intPtr2, _NewPnt);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
	}

	public static void eplGeoE_CalcXYZToPolar(Eps_Point _StartPnt, Eps_Point _EndPnt, ref double _dHorzPolarDirection, ref double _dVertPolarDirection, ref double _dPolarSlopeDistance)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_StartPnt));
		Marshal.StructureToPtr(_StartPnt, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_EndPnt));
		Marshal.StructureToPtr(_EndPnt, intPtr2, fDeleteOld: true);
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_CalcXYZToPolar(intPtr, intPtr2, ref _dHorzPolarDirection, ref _dVertPolarDirection, ref _dPolarSlopeDistance);
		}
		else
		{
			Externalx64.eplDLLGeoE_CalcXYZToPolar(intPtr, intPtr2, ref _dHorzPolarDirection, ref _dVertPolarDirection, ref _dPolarSlopeDistance);
		}
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
	}

	public static void eplFilP_SetRegAppProgramKey(string _strKey)
	{
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLFilP_SetRegAppProgramKey(_strKey);
		}
		else
		{
			Externalx64.eplDLLFilP_SetRegAppProgramKey(_strKey);
		}
	}

	public static int eplFilP_GetSupportPath(string _strFileName, ref string _strSupportPath)
	{
		StringBuilder stringBuilder = new StringBuilder(32767);
		int num = 0;
		num = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLFilP_GetSupportPath(_strFileName, stringBuilder) : Externalx86.eplDLLFilP_GetSupportPath(_strFileName, stringBuilder));
		_strSupportPath = stringBuilder.ToString();
		return num;
	}

	public static void eplGeoE_CalcTangentInDirOfArc(Eps_Arc _Arc, ref double _dTangentInDir)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((BaseArc)_Arc));
		Marshal.StructureToPtr(_Arc, intPtr, fDeleteOld: true);
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_CalcTangentInDirOfArc(intPtr, ref _dTangentInDir);
		}
		else
		{
			Externalx64.eplDLLGeoE_CalcTangentInDirOfArc(intPtr, ref _dTangentInDir);
		}
		Marshal.FreeHGlobal(intPtr);
	}

	public static void eplGeoE_CalcTangentOutDirOfArc(Eps_Arc _Arc, ref double _dTangentOutDir)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((BaseArc)_Arc));
		Marshal.StructureToPtr(_Arc, intPtr, fDeleteOld: true);
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_CalcTangentOutDirOfArc(intPtr, ref _dTangentOutDir);
		}
		else
		{
			Externalx64.eplDLLGeoE_CalcTangentOutDirOfArc(intPtr, ref _dTangentOutDir);
		}
		Marshal.FreeHGlobal(intPtr);
	}

	public static void eplGeoE_CalcArcChordArea(Eps_Arc _Arc, ref double _dChordArea)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((BaseArc)_Arc));
		Marshal.StructureToPtr(_Arc, intPtr, fDeleteOld: true);
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLGeoE_CalcArcChordArea(intPtr, ref _dChordArea);
		}
		else
		{
			Externalx64.eplDLLGeoE_CalcArcChordArea(intPtr, ref _dChordArea);
		}
		Marshal.FreeHGlobal(intPtr);
	}

	public static bool eplGeoE_IsLegListInsideLegList(Eps_Line[] _aTestLines, int _nTestLineCount, Eps_Arc[] _aTestArcs, int _nTestArcCount, int[] _aTestLegOrder, Eps_Line[] _aLines, int _nLineCount, Eps_Arc[] _aArcs, int _nArcCount, int[] _aLegOrder, double _dTolerance)
	{
		int num = 0;
		long num2 = 0L;
		IntPtr intPtr = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		if (_aTestLines != null)
		{
			num = _nTestLineCount * 2;
			intPtr = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(BaseLine)));
			zero = intPtr;
			for (int i = 0; i < _nTestLineCount; i++)
			{
				Marshal.StructureToPtr((BaseLine)_aTestLines[i], zero, true);
				num2 = zero.ToInt64();
				zero = new IntPtr(num2 + Marshal.SizeOf(typeof(BaseLine)));
			}
		}
		IntPtr intPtr2 = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		if (_aTestArcs != null)
		{
			num = _nTestArcCount * 2;
			intPtr2 = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(BaseArc)));
			zero2 = intPtr2;
			for (int j = 0; j < _nTestArcCount; j++)
			{
				Marshal.StructureToPtr((BaseArc)_aTestArcs[j], zero2, true);
				num2 = zero2.ToInt64();
				zero2 = new IntPtr(num2 + Marshal.SizeOf(typeof(BaseArc)));
			}
		}
		IntPtr intPtr3 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		if (_aTestLegOrder != null)
		{
			num = (_nTestLineCount + _nTestArcCount) * 2;
			intPtr3 = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(int)));
			zero3 = intPtr3;
			for (int k = 0; k < _nTestLineCount + _nTestArcCount; k++)
			{
				Marshal.StructureToPtr(_aTestLegOrder[k], zero3, fDeleteOld: true);
				num2 = zero3.ToInt64();
				zero3 = new IntPtr(num2 + Marshal.SizeOf(typeof(int)));
			}
		}
		IntPtr intPtr4 = IntPtr.Zero;
		IntPtr zero4 = IntPtr.Zero;
		if (_aLines != null)
		{
			num = _nLineCount * 2;
			intPtr4 = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(BaseLine)));
			zero4 = intPtr4;
			for (int l = 0; l < _nLineCount; l++)
			{
				Marshal.StructureToPtr((BaseLine)_aLines[l], zero4, true);
				num2 = zero4.ToInt64();
				zero4 = new IntPtr(num2 + Marshal.SizeOf(typeof(BaseLine)));
			}
		}
		IntPtr intPtr5 = IntPtr.Zero;
		IntPtr zero5 = IntPtr.Zero;
		if (_aArcs != null)
		{
			num = _nArcCount * 2;
			intPtr5 = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(BaseArc)));
			zero5 = intPtr5;
			for (int m = 0; m < _nArcCount; m++)
			{
				Marshal.StructureToPtr((BaseArc)_aArcs[m], zero5, true);
				num2 = zero5.ToInt64();
				zero5 = new IntPtr(num2 + Marshal.SizeOf(typeof(BaseArc)));
			}
		}
		IntPtr intPtr6 = IntPtr.Zero;
		IntPtr zero6 = IntPtr.Zero;
		if (_aLegOrder != null)
		{
			num = (_nLineCount + _nArcCount) * 2;
			intPtr6 = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(int)));
			zero6 = intPtr6;
			for (int n = 0; n < _nLineCount + _nArcCount; n++)
			{
				Marshal.StructureToPtr(_aLegOrder[n], zero6, fDeleteOld: true);
				num2 = zero6.ToInt64();
				zero6 = new IntPtr(num2 + Marshal.SizeOf(typeof(int)));
			}
		}
		bool flag = false;
		flag = (ApplicationUtility.Is64BitInstall ? Externalx64.eplDLLGeoE_IsLegListInsideLegList(intPtr, _nTestLineCount, intPtr2, _nTestArcCount, intPtr3, intPtr4, _nLineCount, intPtr5, _nArcCount, intPtr6, _dTolerance) : Externalx86.eplDLLGeoE_IsLegListInsideLegList(intPtr, _nTestLineCount, intPtr2, _nTestArcCount, intPtr3, intPtr4, _nLineCount, intPtr5, _nArcCount, intPtr6, _dTolerance));
		zero = intPtr;
		for (int num3 = 0; num3 < _nTestLineCount; num3++)
		{
			Marshal.DestroyStructure(zero, typeof(BaseLine));
			num2 = zero.ToInt64();
			zero = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseLine)));
		}
		if (intPtr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr);
		}
		zero2 = intPtr2;
		for (int num4 = 0; num4 < _nTestArcCount; num4++)
		{
			Marshal.DestroyStructure(zero2, typeof(BaseArc));
			num2 = zero2.ToInt64();
			zero2 = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseArc)));
		}
		if (intPtr2 != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr2);
		}
		zero3 = intPtr3;
		for (int num5 = 0; num5 < _nTestLineCount + _nTestArcCount; num5++)
		{
			Marshal.DestroyStructure(zero3, typeof(int));
			num2 = zero3.ToInt64();
			zero3 = (IntPtr)(num2 + Marshal.SizeOf(typeof(int)));
		}
		if (intPtr3 != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr3);
		}
		zero4 = intPtr4;
		for (int num6 = 0; num6 < _nLineCount; num6++)
		{
			Marshal.DestroyStructure(zero4, typeof(BaseLine));
			num2 = zero4.ToInt64();
			zero4 = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseLine)));
		}
		if (intPtr4 != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr4);
		}
		zero5 = intPtr5;
		for (int num7 = 0; num7 < _nArcCount; num7++)
		{
			Marshal.DestroyStructure(zero5, typeof(BaseArc));
			num2 = zero5.ToInt64();
			zero5 = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseArc)));
		}
		if (intPtr5 != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr5);
		}
		zero6 = intPtr6;
		for (int num8 = 0; num8 < _nLineCount + _nArcCount; num8++)
		{
			Marshal.DestroyStructure(zero6, typeof(int));
			num2 = zero6.ToInt64();
			zero6 = (IntPtr)(num2 + Marshal.SizeOf(typeof(int)));
		}
		if (intPtr6 != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr6);
		}
		return flag;
	}

	public static double eplCalE_fmod(double _dFirstOperand, double _dSecondOperand)
	{
		return eplCalE_fmod(_dFirstOperand, _dSecondOperand, 1E-05);
	}

	public static double eplCalE_fmod(double _dFirstOperand, double _dSecondOperand, double _dTolerance)
	{
		double _dReturnValue = 0.0;
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLCalE_fmod(_dFirstOperand, _dSecondOperand, _dTolerance, ref _dReturnValue);
		}
		else
		{
			Externalx64.eplDLLCalE_fmod(_dFirstOperand, _dSecondOperand, _dTolerance, ref _dReturnValue);
		}
		return _dReturnValue;
	}

	public static double eplCalE_RoundValue(double _dValue, int _nPrecision)
	{
		double _dRoundedValue = 0.0;
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLCalE_RoundValue(_dValue, _nPrecision, ref _dRoundedValue);
		}
		else
		{
			Externalx64.eplDLLCalE_RoundValue(_dValue, _nPrecision, ref _dRoundedValue);
		}
		return _dRoundedValue;
	}

	public static double eplCalE_CalcComponentKOfCrossProduct(Eps_Point _FirstPoint, Eps_Point _SecondPoint)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_FirstPoint));
		Marshal.StructureToPtr(_FirstPoint, intPtr, fDeleteOld: true);
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(_SecondPoint));
		Marshal.StructureToPtr(_SecondPoint, intPtr2, fDeleteOld: true);
		double _dReturnValue = 0.0;
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLLCalE_CalcComponentKOfCrossProduct(intPtr, intPtr2, ref _dReturnValue);
		}
		else
		{
			Externalx64.eplDLLCalE_CalcComponentKOfCrossProduct(intPtr, intPtr2, ref _dReturnValue);
		}
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		return _dReturnValue;
	}

	public static void epl_SetRevitVersion(RevitVersion _RevitVersion)
	{
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.eplDLL_SetRevitVersion(_RevitVersion.GetHashCode());
		}
		else
		{
			Externalx64.eplDLL_SetRevitVersion(_RevitVersion.GetHashCode());
		}
	}

	public static void epl_UnloadHelper(bool _bIs64BitInstall)
	{
	}

	public static void CreateCatchLinesAndFeatureLinesFromTopo(string _strFileName, Eps_Line[] _aLines, int _nLineCount, Eps_Arc[] _aArcs, int _nArcCount, int[] _aLegOrder, Eps_Point[] _aPnts, int _nPointCount, double _dLinearInterval, double _dRadialInterval, double _dArcPointInterval, double _dCutSlope, double _dFillSlope, bool _bProjectLeft, ref Eps_Line[] _aCatchLines, ref int _nNumCatchLines, ref Eps_Line[] _aFeatureLines, ref int _nNumFeatureLines)
	{
		int num = 0;
		long num2 = 0L;
		IntPtr intPtr = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		if (_aLines != null)
		{
			num = _nLineCount * 2;
			intPtr = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(BaseLine)));
			zero = intPtr;
			for (int i = 0; i < _nLineCount; i++)
			{
				Marshal.StructureToPtr((BaseLine)_aLines[i], zero, true);
				num2 = zero.ToInt64();
				zero = new IntPtr(num2 + Marshal.SizeOf(typeof(BaseLine)));
			}
		}
		IntPtr intPtr2 = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		if (_aArcs != null)
		{
			num = _nArcCount * 2;
			intPtr2 = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(BaseArc)));
			zero2 = intPtr2;
			for (int j = 0; j < _nArcCount; j++)
			{
				Marshal.StructureToPtr((BaseArc)_aArcs[j], zero2, true);
				num2 = zero2.ToInt64();
				zero2 = new IntPtr(num2 + Marshal.SizeOf(typeof(BaseArc)));
			}
		}
		IntPtr intPtr3 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		if (_aLegOrder != null)
		{
			num = (_nLineCount + _nArcCount) * 2;
			intPtr3 = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(int)));
			zero3 = intPtr3;
			for (int k = 0; k < _nLineCount + _nArcCount; k++)
			{
				Marshal.StructureToPtr(_aLegOrder[k], zero3, fDeleteOld: true);
				num2 = zero3.ToInt64();
				zero3 = new IntPtr(num2 + Marshal.SizeOf(typeof(int)));
			}
		}
		IntPtr intPtr4 = IntPtr.Zero;
		IntPtr zero4 = IntPtr.Zero;
		if (_aPnts != null)
		{
			num = _nPointCount * 2;
			intPtr4 = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(Eps_Point)));
			zero4 = intPtr4;
			for (int l = 0; l < _nPointCount; l++)
			{
				Marshal.StructureToPtr(_aPnts[l], zero4, fDeleteOld: true);
				num2 = zero4.ToInt64();
				zero4 = new IntPtr(num2 + Marshal.SizeOf(typeof(Eps_Point)));
			}
		}
		IntPtr _aLeftCatchLines = IntPtr.Zero;
		IntPtr zero5 = IntPtr.Zero;
		IntPtr _aFeatureLines2 = IntPtr.Zero;
		IntPtr zero6 = IntPtr.Zero;
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.epl_CreateCatchLinesAndFeatureLinesFromTopo(_strFileName, intPtr, _nLineCount, intPtr2, _nArcCount, intPtr3, intPtr4, _nPointCount, _dLinearInterval, _dRadialInterval, _dArcPointInterval, _dCutSlope, _dFillSlope, _bProjectLeft, ref _aLeftCatchLines, ref _nNumCatchLines, ref _aFeatureLines2, ref _nNumFeatureLines);
		}
		else
		{
			Externalx64.epl_CreateCatchLinesAndFeatureLinesFromTopo(_strFileName, intPtr, _nLineCount, intPtr2, _nArcCount, intPtr3, intPtr4, _nPointCount, _dLinearInterval, _dRadialInterval, _dArcPointInterval, _dCutSlope, _dFillSlope, _bProjectLeft, ref _aLeftCatchLines, ref _nNumCatchLines, ref _aFeatureLines2, ref _nNumFeatureLines);
		}
		_aCatchLines = new Eps_Line[_nNumCatchLines];
		zero5 = _aLeftCatchLines;
		for (int m = 0; m < _nNumCatchLines; m++)
		{
			_aCatchLines[m] = new Eps_Line((BaseLine)Marshal.PtrToStructure(zero5, typeof(BaseLine)));
			Marshal.DestroyStructure(zero5, typeof(BaseLine));
			num2 = zero5.ToInt64();
			zero5 = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseLine)));
		}
		if (_aLeftCatchLines != IntPtr.Zero)
		{
			Marshal.DestroyStructure(_aLeftCatchLines, typeof(BaseLine));
		}
		_aFeatureLines = new Eps_Line[_nNumFeatureLines];
		zero6 = _aFeatureLines2;
		for (int n = 0; n < _nNumFeatureLines; n++)
		{
			_aFeatureLines[n] = new Eps_Line((BaseLine)Marshal.PtrToStructure(zero6, typeof(BaseLine)));
			Marshal.DestroyStructure(zero6, typeof(BaseLine));
			num2 = zero6.ToInt64();
			zero6 = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseLine)));
		}
		if (_aFeatureLines2 != IntPtr.Zero)
		{
			Marshal.DestroyStructure(_aFeatureLines2, typeof(BaseLine));
		}
		zero = intPtr;
		for (int num3 = 0; num3 < _nLineCount; num3++)
		{
			Marshal.DestroyStructure(zero, typeof(BaseLine));
			num2 = zero.ToInt64();
			zero = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseLine)));
		}
		if (intPtr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr);
		}
		zero2 = intPtr2;
		for (int num4 = 0; num4 < _nArcCount; num4++)
		{
			Marshal.DestroyStructure(zero2, typeof(BaseArc));
			num2 = zero2.ToInt64();
			zero2 = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseArc)));
		}
		if (intPtr2 != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr2);
		}
		zero3 = intPtr3;
		for (int num5 = 0; num5 < _nLineCount + _nArcCount; num5++)
		{
			Marshal.DestroyStructure(zero3, typeof(int));
			num2 = zero3.ToInt64();
			zero3 = (IntPtr)(num2 + Marshal.SizeOf(typeof(int)));
		}
		if (intPtr3 != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr3);
		}
		zero4 = intPtr4;
		for (int num6 = 0; num6 < _nPointCount; num6++)
		{
			Marshal.DestroyStructure(zero4, typeof(Eps_Point));
			num2 = zero4.ToInt64();
			zero4 = (IntPtr)(num2 + Marshal.SizeOf(typeof(Eps_Point)));
		}
		if (intPtr4 != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr4);
		}
	}

	public static void CreateCutAndFillFromTopo(string _strFileName, Eps_Line[] _aLines, int _nLineCount, Eps_Arc[] _aArcs, int _nArcCount, int[] _aLegOrder, Eps_Point[] _aPnts, int _nPointCount, double _dLinearInterval, double _dRadialInterval, double _dArcPointInterval, double _dLeftOffsetWidth, double _dRightOffsetWidth, double _dBoulevardWidth, double _dCutSlope, double _dFillSlope, int _nProjectInnerSlopes, double _dSubgradeAdjustElev, bool _bIsCrownPresent, bool _bIsProjectLegToTopo, double _dLeftEdgeAdjustElev, double _dRightEdgeAdjustElev, double _dLeftSubgradeAdjustElev, double _dRightSubgradeAdjustElev, ref Eps_Point[] _aCutFillPoints, ref int _nNumCutFillPoints, ref Eps_Line[] _aLeftCatchLines, ref int _nNumLeftCatchLines, ref Eps_Line[] _aRightCatchLines, ref int _nNumRightCatchLines, ref Eps_Line[] _aLeftCutFillLines, ref int _nNumLeftCutFillLines, ref Eps_Line[] _aRightCutFillLines, ref int _nNumRightCutFillLines)
	{
		int num = 0;
		long num2 = 0L;
		IntPtr intPtr = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		if (_aLines != null)
		{
			num = _nLineCount * 2;
			intPtr = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(BaseLine)));
			zero = intPtr;
			for (int i = 0; i < _nLineCount; i++)
			{
				Marshal.StructureToPtr((BaseLine)_aLines[i], zero, true);
				num2 = zero.ToInt64();
				zero = new IntPtr(num2 + Marshal.SizeOf(typeof(BaseLine)));
			}
		}
		IntPtr intPtr2 = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		if (_aArcs != null)
		{
			num = _nArcCount * 2;
			intPtr2 = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(BaseArc)));
			zero2 = intPtr2;
			for (int j = 0; j < _nArcCount; j++)
			{
				Marshal.StructureToPtr((BaseArc)_aArcs[j], zero2, true);
				num2 = zero2.ToInt64();
				zero2 = new IntPtr(num2 + Marshal.SizeOf(typeof(BaseArc)));
			}
		}
		IntPtr intPtr3 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		if (_aLegOrder != null)
		{
			num = (_nLineCount + _nArcCount) * 2;
			intPtr3 = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(int)));
			zero3 = intPtr3;
			for (int k = 0; k < _nLineCount + _nArcCount; k++)
			{
				Marshal.StructureToPtr(_aLegOrder[k], zero3, fDeleteOld: true);
				num2 = zero3.ToInt64();
				zero3 = new IntPtr(num2 + Marshal.SizeOf(typeof(int)));
			}
		}
		IntPtr intPtr4 = IntPtr.Zero;
		IntPtr zero4 = IntPtr.Zero;
		if (_aPnts != null)
		{
			num = _nPointCount * 2;
			intPtr4 = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(Eps_Point)));
			zero4 = intPtr4;
			for (int l = 0; l < _nPointCount; l++)
			{
				Marshal.StructureToPtr(_aPnts[l], zero4, fDeleteOld: true);
				num2 = zero4.ToInt64();
				zero4 = new IntPtr(num2 + Marshal.SizeOf(typeof(Eps_Point)));
			}
		}
		IntPtr _aCutFillPoints2 = IntPtr.Zero;
		IntPtr zero5 = IntPtr.Zero;
		IntPtr _aLeftCatchLines2 = IntPtr.Zero;
		IntPtr zero6 = IntPtr.Zero;
		IntPtr _aRightCatchLines2 = IntPtr.Zero;
		IntPtr zero7 = IntPtr.Zero;
		IntPtr _aLeftCutFillLines2 = IntPtr.Zero;
		IntPtr zero8 = IntPtr.Zero;
		IntPtr _aRightCutFillLines2 = IntPtr.Zero;
		IntPtr zero9 = IntPtr.Zero;
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.epl_CreateCutAndFillFromTopo(_strFileName, intPtr, _nLineCount, intPtr2, _nArcCount, intPtr3, intPtr4, _nPointCount, _dLinearInterval, _dRadialInterval, _dArcPointInterval, _dLeftOffsetWidth, _dRightOffsetWidth, _dBoulevardWidth, _dCutSlope, _dFillSlope, _nProjectInnerSlopes, _dSubgradeAdjustElev, _bIsCrownPresent, _bIsProjectLegToTopo, _dLeftEdgeAdjustElev, _dRightEdgeAdjustElev, _dLeftSubgradeAdjustElev, _dRightSubgradeAdjustElev, ref _aCutFillPoints2, ref _nNumCutFillPoints, ref _aLeftCatchLines2, ref _nNumLeftCatchLines, ref _aRightCatchLines2, ref _nNumRightCatchLines, ref _aLeftCutFillLines2, ref _nNumLeftCutFillLines, ref _aRightCutFillLines2, ref _nNumRightCutFillLines);
		}
		else
		{
			Externalx64.epl_CreateCutAndFillFromTopo(_strFileName, intPtr, _nLineCount, intPtr2, _nArcCount, intPtr3, intPtr4, _nPointCount, _dLinearInterval, _dRadialInterval, _dArcPointInterval, _dLeftOffsetWidth, _dRightOffsetWidth, _dBoulevardWidth, _dCutSlope, _dFillSlope, _nProjectInnerSlopes, _dSubgradeAdjustElev, _bIsCrownPresent, _bIsProjectLegToTopo, _dLeftEdgeAdjustElev, _dRightEdgeAdjustElev, _dLeftSubgradeAdjustElev, _dRightSubgradeAdjustElev, ref _aCutFillPoints2, ref _nNumCutFillPoints, ref _aLeftCatchLines2, ref _nNumLeftCatchLines, ref _aRightCatchLines2, ref _nNumRightCatchLines, ref _aLeftCutFillLines2, ref _nNumLeftCutFillLines, ref _aRightCutFillLines2, ref _nNumRightCutFillLines);
		}
		_aCutFillPoints = new Eps_Point[_nNumCutFillPoints];
		zero5 = _aCutFillPoints2;
		for (int m = 0; m < _nNumCutFillPoints; m++)
		{
			_aCutFillPoints[m] = (Eps_Point)Marshal.PtrToStructure(zero5, typeof(Eps_Point));
			Marshal.DestroyStructure(zero5, typeof(Eps_Point));
			num2 = zero5.ToInt64();
			zero5 = (IntPtr)(num2 + Marshal.SizeOf(typeof(Eps_Point)));
		}
		if (_aCutFillPoints2 != IntPtr.Zero)
		{
			Marshal.DestroyStructure(_aCutFillPoints2, typeof(Eps_Point));
		}
		_aLeftCatchLines = new Eps_Line[_nNumLeftCatchLines];
		zero6 = _aLeftCatchLines2;
		for (int n = 0; n < _nNumLeftCatchLines; n++)
		{
			_aLeftCatchLines[n] = new Eps_Line((BaseLine)Marshal.PtrToStructure(zero6, typeof(BaseLine)));
			Marshal.DestroyStructure(zero6, typeof(BaseLine));
			num2 = zero6.ToInt64();
			zero6 = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseLine)));
		}
		if (_aLeftCatchLines2 != IntPtr.Zero)
		{
			Marshal.DestroyStructure(_aLeftCatchLines2, typeof(BaseLine));
		}
		_aRightCatchLines = new Eps_Line[_nNumRightCatchLines];
		zero7 = _aRightCatchLines2;
		for (int num3 = 0; num3 < _nNumRightCatchLines; num3++)
		{
			_aRightCatchLines[num3] = new Eps_Line((BaseLine)Marshal.PtrToStructure(zero7, typeof(BaseLine)));
			Marshal.DestroyStructure(zero7, typeof(BaseLine));
			num2 = zero7.ToInt64();
			zero7 = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseLine)));
		}
		if (_aRightCatchLines2 != IntPtr.Zero)
		{
			Marshal.DestroyStructure(_aRightCatchLines2, typeof(BaseLine));
		}
		_aLeftCutFillLines = new Eps_Line[_nNumLeftCutFillLines];
		zero8 = _aLeftCutFillLines2;
		for (int num4 = 0; num4 < _nNumLeftCutFillLines; num4++)
		{
			_aLeftCutFillLines[num4] = new Eps_Line((BaseLine)Marshal.PtrToStructure(zero8, typeof(BaseLine)));
			Marshal.DestroyStructure(zero8, typeof(BaseLine));
			num2 = zero8.ToInt64();
			zero8 = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseLine)));
		}
		if (_aLeftCutFillLines2 != IntPtr.Zero)
		{
			Marshal.DestroyStructure(_aLeftCutFillLines2, typeof(BaseLine));
		}
		_aRightCutFillLines = new Eps_Line[_nNumRightCutFillLines];
		zero9 = _aRightCutFillLines2;
		for (int num5 = 0; num5 < _nNumRightCutFillLines; num5++)
		{
			_aRightCutFillLines[num5] = new Eps_Line((BaseLine)Marshal.PtrToStructure(zero9, typeof(BaseLine)));
			Marshal.DestroyStructure(zero9, typeof(BaseLine));
			num2 = zero9.ToInt64();
			zero9 = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseLine)));
		}
		if (_aRightCutFillLines2 != IntPtr.Zero)
		{
			Marshal.DestroyStructure(_aRightCutFillLines2, typeof(BaseLine));
		}
		zero = intPtr;
		for (int num6 = 0; num6 < _nLineCount; num6++)
		{
			Marshal.DestroyStructure(zero, typeof(BaseLine));
			num2 = zero.ToInt64();
			zero = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseLine)));
		}
		if (intPtr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr);
		}
		zero2 = intPtr2;
		for (int num7 = 0; num7 < _nArcCount; num7++)
		{
			Marshal.DestroyStructure(zero2, typeof(BaseArc));
			num2 = zero2.ToInt64();
			zero2 = (IntPtr)(num2 + Marshal.SizeOf(typeof(BaseArc)));
		}
		if (intPtr2 != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr2);
		}
		zero3 = intPtr3;
		for (int num8 = 0; num8 < _nLineCount + _nArcCount; num8++)
		{
			Marshal.DestroyStructure(zero3, typeof(int));
			num2 = zero3.ToInt64();
			zero3 = (IntPtr)(num2 + Marshal.SizeOf(typeof(int)));
		}
		if (intPtr3 != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr3);
		}
		zero4 = intPtr4;
		for (int num9 = 0; num9 < _nPointCount; num9++)
		{
			Marshal.DestroyStructure(zero4, typeof(Eps_Point));
			num2 = zero4.ToInt64();
			zero4 = (IntPtr)(num2 + Marshal.SizeOf(typeof(Eps_Point)));
		}
		if (intPtr4 != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr4);
		}
	}

	public static void GenerateTrianglesFromTopo(string _strFileName, Eps_Point[] _aPnts, int _nPointCount, ref Eps_Point[] _aFirstTrianglePoints, ref Eps_Point[] _aSecondTrianglePoints, ref Eps_Point[] _aThirdTrianglePoints, ref int _nNumTriangles)
	{
		int num = 0;
		long num2 = 0L;
		IntPtr intPtr = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		if (_aPnts != null)
		{
			num = _nPointCount * 2;
			intPtr = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(Eps_Point)));
			zero = intPtr;
			for (int i = 0; i < _nPointCount; i++)
			{
				Marshal.StructureToPtr(_aPnts[i], zero, fDeleteOld: true);
				num2 = zero.ToInt64();
				zero = new IntPtr(num2 + Marshal.SizeOf(typeof(Eps_Point)));
			}
		}
		IntPtr _aFirstTrianglePnts = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		if (_aFirstTrianglePoints != null)
		{
			num = _nNumTriangles * 2;
			_aFirstTrianglePnts = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(Eps_Point)));
			zero2 = _aFirstTrianglePnts;
			for (int j = 0; j < _nNumTriangles; j++)
			{
				Marshal.StructureToPtr(_aFirstTrianglePoints[j], zero2, fDeleteOld: true);
				num2 = zero2.ToInt64();
				zero2 = new IntPtr(num2 + Marshal.SizeOf(typeof(Eps_Point)));
			}
		}
		IntPtr _aSecondTrianglePnts = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		if (_aSecondTrianglePoints != null)
		{
			num = _nNumTriangles * 2;
			_aSecondTrianglePnts = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(Eps_Point)));
			zero3 = _aSecondTrianglePnts;
			for (int k = 0; k < _nNumTriangles; k++)
			{
				Marshal.StructureToPtr(_aSecondTrianglePoints[k], zero3, fDeleteOld: true);
				num2 = zero3.ToInt64();
				zero3 = new IntPtr(num2 + Marshal.SizeOf(typeof(Eps_Point)));
			}
		}
		IntPtr _aThirdTrianglePnts = IntPtr.Zero;
		IntPtr zero4 = IntPtr.Zero;
		if (_aThirdTrianglePoints != null)
		{
			num = _nNumTriangles * 2;
			_aThirdTrianglePnts = Marshal.AllocHGlobal(num * Marshal.SizeOf(typeof(Eps_Point)));
			zero4 = _aThirdTrianglePnts;
			for (int l = 0; l < _nNumTriangles; l++)
			{
				Marshal.StructureToPtr(_aThirdTrianglePoints[l], zero4, fDeleteOld: true);
				num2 = zero4.ToInt64();
				zero4 = new IntPtr(num2 + Marshal.SizeOf(typeof(Eps_Point)));
			}
		}
		if (!ApplicationUtility.Is64BitInstall)
		{
			Externalx86.epl_GenerateTrianglesFromTopo(_strFileName, intPtr, _nPointCount, ref _aFirstTrianglePnts, ref _aSecondTrianglePnts, ref _aThirdTrianglePnts, ref _nNumTriangles);
		}
		else
		{
			Externalx64.epl_GenerateTrianglesFromTopo(_strFileName, intPtr, _nPointCount, ref _aFirstTrianglePnts, ref _aSecondTrianglePnts, ref _aThirdTrianglePnts, ref _nNumTriangles);
		}
		_aFirstTrianglePoints = new Eps_Point[_nNumTriangles];
		zero2 = _aFirstTrianglePnts;
		for (int m = 0; m < _nNumTriangles; m++)
		{
			_aFirstTrianglePoints[m] = (Eps_Point)Marshal.PtrToStructure(zero2, typeof(Eps_Point));
			Marshal.DestroyStructure(zero2, typeof(Eps_Point));
			num2 = zero2.ToInt64();
			zero2 = (IntPtr)(num2 + Marshal.SizeOf(typeof(Eps_Point)));
		}
		if (_aFirstTrianglePnts != IntPtr.Zero)
		{
			Marshal.DestroyStructure(_aFirstTrianglePnts, typeof(Eps_Point));
		}
		_aSecondTrianglePoints = new Eps_Point[_nNumTriangles];
		zero3 = _aSecondTrianglePnts;
		for (int n = 0; n < _nNumTriangles; n++)
		{
			_aSecondTrianglePoints[n] = (Eps_Point)Marshal.PtrToStructure(zero3, typeof(Eps_Point));
			Marshal.DestroyStructure(zero3, typeof(Eps_Point));
			num2 = zero3.ToInt64();
			zero3 = (IntPtr)(num2 + Marshal.SizeOf(typeof(Eps_Point)));
		}
		if (_aSecondTrianglePnts != IntPtr.Zero)
		{
			Marshal.DestroyStructure(_aSecondTrianglePnts, typeof(Eps_Point));
		}
		_aThirdTrianglePoints = new Eps_Point[_nNumTriangles];
		zero4 = _aThirdTrianglePnts;
		for (int num3 = 0; num3 < _nNumTriangles; num3++)
		{
			_aThirdTrianglePoints[num3] = (Eps_Point)Marshal.PtrToStructure(zero4, typeof(Eps_Point));
			Marshal.DestroyStructure(zero4, typeof(Eps_Point));
			num2 = zero4.ToInt64();
			zero4 = (IntPtr)(num2 + Marshal.SizeOf(typeof(Eps_Point)));
		}
		if (_aThirdTrianglePnts != IntPtr.Zero)
		{
			Marshal.DestroyStructure(_aThirdTrianglePnts, typeof(Eps_Point));
		}
		zero = intPtr;
		for (int num4 = 0; num4 < _nPointCount; num4++)
		{
			Marshal.DestroyStructure(zero, typeof(Eps_Point));
			num2 = zero.ToInt64();
			zero = (IntPtr)(num2 + Marshal.SizeOf(typeof(Eps_Point)));
		}
		if (intPtr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}
}
