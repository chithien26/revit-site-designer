using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using SITES.Utilities;

namespace EPCommon;

public class RevitUtility
{
	public const int PROGRESSBAR_DELAY = 300;

	public static XYZ LastSelectionPoint { get; private set; }

	public static string GetFormattedFileName(string _strFileName)
	{
		return GetFormattedFileName(_strFileName, _bCreateDirectory: false);
	}

	public static string GetFormattedFileName(string _strFileName, bool _bCreateDirectory)
	{
		string result = string.Empty;
		if (_strFileName != string.Empty)
		{
			_strFileName = _strFileName.Replace('/', '\\');
			FileInfo fileInfo = new FileInfo(_strFileName);
			string text = string.Empty;
			string name = fileInfo.Name;
			if (_strFileName.IndexOf('\\') >= 0)
			{
				text = fileInfo.DirectoryName;
				if (!text.EndsWith("\\"))
				{
					text += "\\";
				}
			}
			if (_bCreateDirectory && !Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			result = text + name;
		}
		return result;
	}

	public static int CompareFileNames(string _strFirstFileName, string _strSecondFileName)
	{
		return string.Compare(GetFormattedFileName(_strFirstFileName), GetFormattedFileName(_strSecondFileName), ignoreCase: true);
	}

	public static string GetFormattedDirectoryName(string _strDirectoryName)
	{
		return GetFormattedDirectoryName(_strDirectoryName, _bCreateDirectory: false);
	}

	public static string GetFormattedDirectoryName(string _strDirectoryName, bool _bCreateDirectory)
	{
		string result = string.Empty;
		if (_strDirectoryName != string.Empty)
		{
			_strDirectoryName = _strDirectoryName.Replace('/', '\\');
			DirectoryInfo directoryInfo = new DirectoryInfo(_strDirectoryName);
			string text = directoryInfo.Name;
			if (!text.EndsWith("\\"))
			{
				text += "\\";
			}
			if (_bCreateDirectory && !Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			result = text;
		}
		return result;
	}

	public static int CompareDirectoryNames(string _strFirstDirectoryName, string _strSecondDirectoryName)
	{
		return string.Compare(GetFormattedDirectoryName(_strFirstDirectoryName), GetFormattedDirectoryName(_strSecondDirectoryName), ignoreCase: true);
	}

	public static Element GetSurfaceContainingPoint(Eps_Point _FindPoint, List<TopographySurfaceContainer> _SurfaceList)
	{
		double _dElevation = 0.0;
		return GetSurfaceContainingPoint(_FindPoint, _SurfaceList, ref _dElevation);
	}

	public static bool GetFullFamilyFilenameIfExist(string _strSearchFilename, ref string _strFullFamilyFilename)
	{
		return GetFullFamilyFilenameIfExist(string.Empty, _strSearchFilename, ref _strFullFamilyFilename);
	}

	public static bool GetFullFamilyFilenameIfExist(string _strLibrarySubFolder, string _strSearchFilename, ref string _strFullFamilyFilename)
	{
		if (_strLibrarySubFolder != string.Empty && !_strLibrarySubFolder.EndsWith("\\"))
		{
			_strLibrarySubFolder += "\\";
		}
		string empty = string.Empty;
		empty = "2011\\";
		ApplicationUtility.RegistryPaths.GetLibraryPath(empty + _strLibrarySubFolder + _strSearchFilename, ref _strFullFamilyFilename);
		if (File.Exists(_strFullFamilyFilename))
		{
			return true;
		}
		ApplicationUtility.RegistryPaths.GetSamplePath(empty + _strSearchFilename, ref _strFullFamilyFilename);
		if (File.Exists(_strFullFamilyFilename))
		{
			return true;
		}
		ApplicationUtility.RegistryPaths.GetLibraryPath(_strLibrarySubFolder + _strSearchFilename, ref _strFullFamilyFilename);
		if (File.Exists(_strFullFamilyFilename))
		{
			return true;
		}
		ApplicationUtility.RegistryPaths.GetSamplePath(_strSearchFilename, ref _strFullFamilyFilename);
		if (File.Exists(_strFullFamilyFilename))
		{
			return true;
		}
		return false;
	}

	public static Element GetSurfaceContainingPoint(Eps_Point _FindPoint, List<TopographySurfaceContainer> _SurfaceList, ref double _dElevation)
	{
		return GetSurfaceContainingPoint(_FindPoint, _SurfaceList, null, ref _dElevation);
	}

	public static Element GetSurfaceContainingPoint(Eps_Point _FindPoint, List<TopographySurfaceContainer> _SurfaceList, Phase _CurPhase, ref double _dElevation)
	{
		_dElevation = 0.0;
		Element element = null;
		if (_SurfaceList != null && _SurfaceList.Count > 0)
		{
			for (int i = 0; i < _SurfaceList.Count; i++)
			{
				element = _SurfaceList[i].GetSurfaceContainingPoint(_FindPoint, _CurPhase, ref _dElevation);
				if (element != null)
				{
					return element;
				}
			}
		}
		return null;
	}

	public static bool GetElementsFromProject(Document _Document, string _strPrompt, ref ElementSet _Elements)
	{
		return GetElementsFromProject(_Document, _strPrompt, ref _Elements, _bDisplayError: true, PickMethod.PickObject);
	}

	public static bool GetElementsFromProject(Document _Document, string _strPrompt, ref ElementSet _Elements, bool _bDisplayError)
	{
		return GetElementsFromProject(_Document, _strPrompt, ref _Elements, _bDisplayError, PickMethod.PickObject);
	}

	public static bool GetElementsFromProject(Document _Document, string _strPrompt, ref ElementSet _Elements, bool _bDisplayError, PickMethod _PickMethod)
	{
		UIDocument uIDocument = new UIDocument(_Document);
		switch (_PickMethod)
		{
		case PickMethod.PickObject:
		{
			Reference reference = null;
			try
			{
				reference = uIDocument.Selection.PickObject(ObjectType.Element, _strPrompt);
			}
			catch (Autodesk.Revit.Exceptions.OperationCanceledException)
			{
				if (_bDisplayError)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NoObject"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
				}
				return false;
			}
			if (reference != null && _Document.GetElement(reference) != null)
			{
				LastSelectionPoint = reference.GlobalPoint;
				_Elements = new ElementSet();
				_Elements.Insert(_Document.GetElement(reference));
				break;
			}
			if (_bDisplayError)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoObject"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
			return false;
		}
		case PickMethod.PickObjects:
		{
			Reference reference2 = null;
			try
			{
				bool flag = true;
				while (flag)
				{
					reference2 = uIDocument.Selection.PickObject(ObjectType.Element, _strPrompt);
					if (reference2 != null && _Document.GetElement(reference2) != null)
					{
						if (_Elements == null)
						{
							_Elements = new ElementSet();
						}
						_Elements.Insert(_Document.GetElement(reference2));
					}
					else
					{
						flag = false;
					}
				}
			}
			catch (Autodesk.Revit.Exceptions.OperationCanceledException)
			{
				if (_Elements == null || _Elements.Size == 0)
				{
					if (_bDisplayError)
					{
						RevitMessageBox.Show(EPShared.LocalRM.GetString("NoObject"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					}
					return false;
				}
			}
			if (_Elements == null || _Elements.Size == 0)
			{
				if (_bDisplayError)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NoObject"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				}
				return false;
			}
			break;
		}
		case PickMethod.WindowSelect:
		{
			IList<Element> list = null;
			try
			{
				list = uIDocument.Selection.PickElementsByRectangle(_strPrompt);
			}
			catch (Autodesk.Revit.Exceptions.OperationCanceledException)
			{
				if (_bDisplayError)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NoObject"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				}
				return false;
			}
			if (list != null && list.Count > 0)
			{
				_Elements = new ElementSet();
				for (int i = 0; i < list.Count; i++)
				{
					_Elements.Insert(list[i]);
				}
				break;
			}
			if (_bDisplayError)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoObject"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
			return false;
		}
		}
		uIDocument.Selection.Dispose();
		return true;
	}

	public static bool GetPointFromProject(Document _Document, string _strPrompt, ref Eps_Point _Point, bool _bDisplayError)
	{
		try
		{
			UIDocument uIDocument = new UIDocument(_Document);
			_Point = new Eps_Point(uIDocument.Selection.PickPoint(_strPrompt));
		}
		catch (Autodesk.Revit.Exceptions.OperationCanceledException)
		{
			if (_bDisplayError)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoPointPickedMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
			return false;
		}
		catch (AccessViolationException)
		{
			RevitMessageBox.Show("Install Update 1 for Revit 2011.", EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
		return true;
	}

	public static bool PickSideLeg(Document _Document, Eps_Leg _CurrentLeg, ref bool _UseRightSide)
	{
		return PickSideLeg(_Document, _CurrentLeg, ref _UseRightSide, EPShared.LocalRM.GetString("PickOffsetSideMessage"));
	}

	public static bool PickSideLeg(Document _Document, Eps_Leg _CurrentLeg, ref bool _UseRightSide, string _strPrompt)
	{
		Eps_Point _Point = new Eps_Point();
		if (!GetPointFromProject(_Document, _strPrompt, ref _Point, _bDisplayError: true))
		{
			return false;
		}
		double _dStation = 0.0;
		double _dOffset = 0.0;
		RevitGeometry.eplGeoE_CalcStationOffsetAlongLegList(_Point, _CurrentLeg, ref _dStation, ref _dOffset, _bAlwaysSetStationOffset: true);
		if (RevitDefines.GT(_dOffset, 0.0))
		{
			_UseRightSide = true;
		}
		else
		{
			_UseRightSide = false;
		}
		return true;
	}

	public static bool PickSideLine(Document _Document, Eps_Line _CurrentLine, ref bool _UseRightSide)
	{
		return PickSideLine(_Document, _CurrentLine, ref _UseRightSide, EPShared.LocalRM.GetString("PickOffsetSideMessage"));
	}

	public static bool PickSideLine(Document _Document, Eps_Line _CurrentLine, ref bool _UseRightSide, string _strPrompt)
	{
		Eps_Point _Point = new Eps_Point();
		if (!GetPointFromProject(_Document, _strPrompt, ref _Point, _bDisplayError: true))
		{
			return false;
		}
		double _dStation = 0.0;
		double _dOffset = 0.0;
		EPShared.eplGeoE_CalcStationOffsetAlongLine(_Point, _CurrentLine, ref _dStation, ref _dOffset, 0.0);
		if (RevitDefines.GT(_dOffset, 0.0))
		{
			_UseRightSide = true;
		}
		else
		{
			_UseRightSide = false;
		}
		return true;
	}

	public static bool PickSideArc(Document _Document, Eps_Arc _CurrentArc, ref bool _UseRightSide)
	{
		return PickSideArc(_Document, _CurrentArc, ref _UseRightSide, EPShared.LocalRM.GetString("PickOffsetSideMessage"));
	}

	public static bool PickSideArc(Document _Document, Eps_Arc _CurrentArc, ref bool _UseRightSide, string _strPrompt)
	{
		Eps_Point _Point = new Eps_Point();
		if (!GetPointFromProject(_Document, _strPrompt, ref _Point, _bDisplayError: true))
		{
			return false;
		}
		double _dStation = 0.0;
		double _dOffset = 0.0;
		EPShared.eplGeoE_CalcStationOffsetAlongArc(_Point, _CurrentArc, ref _dStation, ref _dOffset, 0.0);
		if (RevitDefines.GT(_dOffset, 0.0))
		{
			_UseRightSide = true;
		}
		else
		{
			_UseRightSide = false;
		}
		return true;
	}

	public static bool BreakApartLeg(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, Eps_Leg _EvalLeg, ref Eps_Leg _FirstLeg, ref Eps_Leg _SecondLeg, ref bool _bPickFirstLeg, ref bool _bPickMiddleSegment, ref Eps_Point _PickedPoint, string _strPrompt)
	{
		bool result = BreakApartLeg(_Document, _TransactionContainer, _dCircleRadius, _EvalLeg, _bAllPromptForDirection: false, _bOnlyPromptMiddle: true, ref _FirstLeg, ref _SecondLeg, ref _bPickFirstLeg, ref _bPickMiddleSegment, _bDisplayError: false, _strPrompt, null);
		if (_FirstLeg != null)
		{
			_PickedPoint = _FirstLeg.LegStartPnt;
			_FirstLeg = _FirstLeg.Reverse();
		}
		else if (_SecondLeg != null)
		{
			_PickedPoint = _SecondLeg.LegStartPnt;
		}
		return result;
	}

	public static bool BreakApartLeg(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, Eps_Leg _EvalLeg, ref Eps_Leg _FirstLeg, ref Eps_Leg _SecondLeg, ref bool _bPickFirstLeg, ref bool _bPickMiddleSegment, ref Eps_Point _PickedPoint, string _strPrompt, List<Eps_Point> _ExcludePoints)
	{
		bool result = BreakApartLeg(_Document, _TransactionContainer, _dCircleRadius, _EvalLeg, _bAllPromptForDirection: false, _bOnlyPromptMiddle: true, ref _FirstLeg, ref _SecondLeg, ref _bPickFirstLeg, ref _bPickMiddleSegment, _bDisplayError: false, _strPrompt, _ExcludePoints);
		if (_FirstLeg != null)
		{
			_PickedPoint = _FirstLeg.LegStartPnt;
			_FirstLeg = _FirstLeg.Reverse();
		}
		else if (_SecondLeg != null)
		{
			_PickedPoint = _SecondLeg.LegStartPnt;
		}
		return result;
	}

	public static bool BreakApartLeg(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, Eps_Leg _EvalLeg, bool _bPromptForDirection, ref Eps_Leg _FirstLeg, ref Eps_Leg _SecondLeg, ref bool _bPickFirstLeg, ref bool _bPickMiddleSegment)
	{
		return BreakApartLeg(_Document, _TransactionContainer, _dCircleRadius, _EvalLeg, _bPromptForDirection, _bOnlyPromptMiddle: false, ref _FirstLeg, ref _SecondLeg, ref _bPickFirstLeg, ref _bPickMiddleSegment, _bDisplayError: true, EPShared.LocalRM.GetString("PickStartingSideMessage"), null);
	}

	public static bool BreakApartLeg(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, Eps_Leg _EvalLeg, bool _bAllPromptForDirection, bool _bOnlyPromptMiddle, ref Eps_Leg _FirstLeg, ref Eps_Leg _SecondLeg, ref bool _bPickFirstLeg, ref bool _bPickMiddleSegment, bool _bDisplayError, string _strPrompt)
	{
		return BreakApartLeg(_Document, _TransactionContainer, _dCircleRadius, _EvalLeg, _bAllPromptForDirection, _bOnlyPromptMiddle, ref _FirstLeg, ref _SecondLeg, ref _bPickFirstLeg, ref _bPickMiddleSegment, _bDisplayError, _strPrompt, null);
	}

	public static bool BreakApartLeg(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, Eps_Leg _EvalLeg, bool _bAllPromptForDirection, bool _bOnlyPromptMiddle, ref Eps_Leg _FirstLeg, ref Eps_Leg _SecondLeg, ref bool _bPickFirstLeg, ref bool _bPickMiddleSegment, bool _bDisplayError, string _strPrompt, List<Eps_Point> _ExcludePoints)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Point _StaOffPnt = new Eps_Point();
		bool result = false;
		bool flag = false;
		double num = 0.0;
		List<Eps_Point> list = new List<Eps_Point>();
		list.Add(_EvalLeg.LegStartPnt);
		for (eps_Leg = _EvalLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _StaOffPnt);
				list.Add(_StaOffPnt);
				list.Add(eps_Leg.Line.EndPnt);
			}
			else if (eps_Leg.Arc != null)
			{
				num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg.Arc.Radius, eps_Leg.Arc.Delta);
				EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num / 2.0, 0.0, ref _StaOffPnt);
				list.Add(_StaOffPnt);
				list.Add(eps_Leg.Arc.EndPnt);
			}
		}
		double _dDistance = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_EvalLeg.LegStartPnt, _EvalLeg.LegEndPnt, ref _dDistance);
		if (!RevitDefines.EQ(_dDistance, 0.0) && _dCircleRadius > _dDistance / 10.0)
		{
			_dCircleRadius = _dDistance / 10.0;
		}
		if (_ExcludePoints != null && _ExcludePoints.Count > 0 && list.Count > 0)
		{
			List<Eps_Point> list2 = new List<Eps_Point>();
			list2.AddRange(_ExcludePoints);
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				for (int num3 = list2.Count - 1; num3 >= 0; num3--)
				{
					if (EPShared.eplGeoE_ArePntsDuplicates2D(list[num2], list2[num3]))
					{
						list.RemoveAt(num2);
						list2.RemoveAt(num3);
						break;
					}
				}
			}
		}
		Eps_Point _UsePoint = new Eps_Point();
		if (PickSide(_Document, _TransactionContainer, _dCircleRadius, list, ref _UsePoint, null, _bDisplayError, _strPrompt))
		{
			_bPickMiddleSegment = false;
			result = true;
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_EvalLeg.LegStartPnt, _UsePoint))
			{
				_SecondLeg = _EvalLeg;
				_FirstLeg = null;
				_bPickFirstLeg = false;
				return true;
			}
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_EvalLeg.LegEndPnt, _UsePoint))
			{
				_SecondLeg = null;
				_FirstLeg = _EvalLeg.Reverse();
				_bPickFirstLeg = true;
				return true;
			}
			for (eps_Leg = _EvalLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
			{
				if (flag)
				{
					if (_SecondLeg == null)
					{
						_SecondLeg = Eps_Leg.SingleLeg(eps_Leg);
					}
					else
					{
						_SecondLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
					}
				}
				else if (eps_Leg.Line != null)
				{
					RevitGeometry.eplGeoE_CalcMidPointBetweenTwoPoints3D(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _StaOffPnt);
					if (EPShared.eplGeoE_ArePntsDuplicates2D(_StaOffPnt, _UsePoint))
					{
						_bPickMiddleSegment = true;
						flag = true;
						eps_Leg2 = new Eps_Leg();
						eps_Leg2.Line = new Eps_Line();
						eps_Leg2.Line.StartPnt = eps_Leg.Line.StartPnt;
						eps_Leg2.Line.EndPnt = _StaOffPnt;
						eps_Leg2.Element = eps_Leg.Element;
						if (_FirstLeg == null)
						{
							_FirstLeg = Eps_Leg.SingleLeg(eps_Leg2);
						}
						else
						{
							_FirstLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
						}
						eps_Leg2 = new Eps_Leg();
						eps_Leg2.Line = new Eps_Line();
						eps_Leg2.Line.StartPnt = _StaOffPnt;
						eps_Leg2.Line.EndPnt = eps_Leg.Line.EndPnt;
						eps_Leg2.Element = eps_Leg.Element;
						if (_SecondLeg == null)
						{
							_SecondLeg = Eps_Leg.SingleLeg(eps_Leg2);
						}
						else
						{
							_SecondLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
						}
					}
					else
					{
						if (_FirstLeg == null)
						{
							_FirstLeg = Eps_Leg.SingleLeg(eps_Leg);
						}
						else
						{
							_FirstLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
						}
						if (EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg.Line.EndPnt, _UsePoint))
						{
							flag = true;
						}
					}
				}
				else if (eps_Leg.Arc != null)
				{
					num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg.Arc.Radius, eps_Leg.Arc.Delta);
					EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num / 2.0, 0.0, ref _StaOffPnt);
					_StaOffPnt.z = eps_Leg.Arc.RadiusPnt.z;
					if (EPShared.eplGeoE_ArePntsDuplicates2D(_StaOffPnt, _UsePoint))
					{
						_bPickMiddleSegment = true;
						flag = true;
						eps_Leg2 = new Eps_Leg();
						eps_Leg2.Arc = new Eps_Arc(eps_Leg.Arc);
						eps_Leg2.Arc.Delta = eps_Leg.Arc.Delta / 2.0;
						eps_Leg2.Arc.StartPnt = eps_Leg.Arc.StartPnt;
						eps_Leg2.Arc.EndPnt = _StaOffPnt;
						eps_Leg2.Element = eps_Leg.Element;
						if (_FirstLeg == null)
						{
							_FirstLeg = Eps_Leg.SingleLeg(eps_Leg2);
						}
						else
						{
							_FirstLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
						}
						eps_Leg2 = new Eps_Leg();
						eps_Leg2.Arc = new Eps_Arc(eps_Leg.Arc);
						eps_Leg2.Arc.Delta = eps_Leg.Arc.Delta / 2.0;
						eps_Leg2.Arc.StartPnt = _StaOffPnt;
						eps_Leg2.Arc.EndPnt = eps_Leg.Arc.EndPnt;
						eps_Leg2.Element = eps_Leg.Element;
						if (_SecondLeg == null)
						{
							_SecondLeg = Eps_Leg.SingleLeg(eps_Leg2);
						}
						else
						{
							_SecondLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
						}
					}
					else
					{
						if (_FirstLeg == null)
						{
							_FirstLeg = Eps_Leg.SingleLeg(eps_Leg);
						}
						else
						{
							_FirstLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
						}
						if (EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg.Arc.EndPnt, _UsePoint))
						{
							flag = true;
						}
					}
				}
			}
			if (_FirstLeg != null)
			{
				_FirstLeg = _FirstLeg.Reverse();
				if ((_bAllPromptForDirection || (_bOnlyPromptMiddle && _bPickMiddleSegment)) && _SecondLeg != null)
				{
					Eps_Point _FindPoint = new Eps_Point();
					Eps_Point _FindPoint2 = new Eps_Point();
					double _dDirection = 0.0;
					double _dDirection2 = 0.0;
					if (FindDistPointAndDirectionAlongLeg(_FirstLeg, _dCircleRadius, ref _FindPoint, ref _dDirection) && FindDistPointAndDirectionAlongLeg(_SecondLeg, _dCircleRadius, ref _FindPoint2, ref _dDirection2))
					{
						return PickDirection(_Document, _TransactionContainer, _dCircleRadius * 2.0, _FindPoint, _dDirection, _FindPoint2, _dDirection2, ref _bPickFirstLeg);
					}
				}
			}
		}
		return result;
	}

	public static bool RemoveLegFromLegList(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, Eps_Leg _EvalLeg, ref Eps_Leg _ReturnLeg, bool _bDisplayError, string _strPrompt)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Point _StaOffPnt = new Eps_Point();
		bool result = false;
		bool flag = false;
		bool flag2 = false;
		double num = 0.0;
		List<Eps_Point> list = new List<Eps_Point>();
		if (_EvalLeg.Line != null)
		{
			list.Add(_EvalLeg.Line.StartPnt);
		}
		for (eps_Leg = _EvalLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				if (eps_Leg.Next == null || eps_Leg.Next.Line != null)
				{
					list.Add(eps_Leg.Line.EndPnt);
				}
			}
			else if (eps_Leg.Arc != null)
			{
				num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg.Arc.Radius, eps_Leg.Arc.Delta);
				EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num / 2.0, 0.0, ref _StaOffPnt);
				list.Add(_StaOffPnt);
			}
		}
		double _dDistance = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_EvalLeg.LegStartPnt, _EvalLeg.LegEndPnt, ref _dDistance);
		if (!RevitDefines.EQ(_dDistance, 0.0) && _dCircleRadius > _dDistance / 10.0)
		{
			_dCircleRadius = _dDistance / 10.0;
		}
		Eps_Point _UsePoint = new Eps_Point();
		if (PickSide(_Document, _TransactionContainer, _dCircleRadius, list, ref _UsePoint, null, _bDisplayError, _strPrompt))
		{
			result = true;
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_EvalLeg.LegStartPnt, _UsePoint))
			{
				if (_EvalLeg.Next != null)
				{
					if (!_EvalLeg.IsLegClosed())
					{
						_ReturnLeg = Eps_Leg.CopyLeg(_EvalLeg.Next);
					}
					else
					{
						eps_Leg = _EvalLeg.Next;
						while (eps_Leg.Next != null)
						{
							if (_ReturnLeg == null)
							{
								_ReturnLeg = Eps_Leg.SingleLeg(eps_Leg);
							}
							else
							{
								_ReturnLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
							}
							eps_Leg = eps_Leg.Next;
						}
						if (eps_Leg != null)
						{
							eps_Leg2 = new Eps_Leg();
							if (eps_Leg.Line != null)
							{
								eps_Leg2.Line = new Eps_Line();
								eps_Leg2.Line.StartPnt = eps_Leg.Line.StartPnt;
								eps_Leg2.Line.EndPnt = _ReturnLeg.LegStartPnt;
							}
							else if (eps_Leg.Arc != null)
							{
								eps_Leg2.Arc = new Eps_Arc(eps_Leg.Arc);
								eps_Leg2.Arc.EndPnt = _ReturnLeg.LegStartPnt;
							}
							if (_ReturnLeg == null)
							{
								_ReturnLeg = Eps_Leg.SingleLeg(eps_Leg2);
							}
							else
							{
								_ReturnLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
							}
						}
					}
				}
				return true;
			}
			for (eps_Leg = _EvalLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
			{
				if (flag2)
				{
					flag2 = false;
					continue;
				}
				if (!flag)
				{
					if (eps_Leg.Line != null)
					{
						if (EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg.Line.EndPnt, _UsePoint))
						{
							flag = true;
							if (eps_Leg.Next != null && eps_Leg.Next.Line != null)
							{
								flag2 = true;
								eps_Leg2 = new Eps_Leg();
								eps_Leg2.Line = new Eps_Line();
								eps_Leg2.Line.StartPnt = eps_Leg.Line.StartPnt;
								eps_Leg2.Line.EndPnt = eps_Leg.Next.Line.EndPnt;
								if (_ReturnLeg == null)
								{
									_ReturnLeg = Eps_Leg.SingleLeg(eps_Leg2);
								}
								else
								{
									_ReturnLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
								}
							}
							continue;
						}
					}
					else if (eps_Leg.Arc != null)
					{
						num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg.Arc.Radius, eps_Leg.Arc.Delta);
						EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, num / 2.0, 0.0, ref _StaOffPnt);
						_StaOffPnt.z = eps_Leg.Arc.RadiusPnt.z;
						if (EPShared.eplGeoE_ArePntsDuplicates2D(_StaOffPnt, _UsePoint))
						{
							flag = true;
							eps_Leg2 = new Eps_Leg();
							eps_Leg2.Line = new Eps_Line();
							eps_Leg2.Line.StartPnt = eps_Leg.Arc.StartPnt;
							eps_Leg2.Line.EndPnt = eps_Leg.Arc.EndPnt;
							if (_ReturnLeg == null)
							{
								_ReturnLeg = Eps_Leg.SingleLeg(eps_Leg2);
							}
							else
							{
								_ReturnLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
							}
							continue;
						}
					}
				}
				if (_ReturnLeg == null)
				{
					_ReturnLeg = Eps_Leg.SingleLeg(eps_Leg);
				}
				else
				{
					_ReturnLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
				}
			}
		}
		return result;
	}

	private static bool FindDistPointAndDirectionAlongLeg(Eps_Leg _EvalLeg, double _dDistance, ref Eps_Point _FindPoint, ref double _dDirection)
	{
		_dDirection = 0.0;
		Eps_Leg eps_Leg = null;
		double _dDistance2 = 0.0;
		for (eps_Leg = _EvalLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDistance2);
				if (_dDistance < _dDistance2)
				{
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg.Line.StartPnt, eps_Leg.Line.EndPnt, ref _dDirection);
					EPShared.eplGeoE_CalcPolarToXY(eps_Leg.Line.StartPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection), _dDistance, ref _FindPoint);
					return true;
				}
				_dDistance -= _dDistance2;
			}
			else if (eps_Leg.Arc != null)
			{
				_dDistance2 = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg.Arc.Radius, eps_Leg.Arc.Delta);
				if (_dDistance < _dDistance2)
				{
					EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg.Arc, _dDistance, 0.0, ref _FindPoint);
					EPShared.eplGeoE_CalcTangentDirOfArc(_FindPoint, eps_Leg.Arc.RadiusPnt, eps_Leg.Arc.Direction, ref _dDirection);
					return true;
				}
				_dDistance -= _dDistance2;
			}
		}
		return false;
	}

	public static bool EvaluateLeg(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, ref Eps_Leg _EvalLeg)
	{
		bool result = false;
		double _dDistance = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_EvalLeg.LegStartPnt, _EvalLeg.LegEndPnt, ref _dDistance);
		if (RevitDefines.EQ(_dDistance, 0.0) && RevitDefines.GT(RevitGeometry.eplGeoE_CalcLegListLength(_EvalLeg), 0.0))
		{
			return true;
		}
		if (RevitDefines.GT(_dDistance / 10.0, 0.0) && _dCircleRadius > _dDistance / 10.0)
		{
			_dCircleRadius = _dDistance / 10.0;
		}
		if (RevitDefines.EQ(_dCircleRadius, 0.0))
		{
			_dCircleRadius = 1.0;
		}
		List<Eps_Point> list = new List<Eps_Point>();
		list.Add(_EvalLeg.LegStartPnt);
		list.Add(_EvalLeg.LegEndPnt);
		Eps_Point _UsePoint = new Eps_Point();
		if (PickSide(_Document, _TransactionContainer, _dCircleRadius, list, ref _UsePoint))
		{
			result = true;
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_EvalLeg.LegEndPnt, _UsePoint))
			{
				_EvalLeg = _EvalLeg.Reverse();
			}
		}
		return result;
	}

	public static bool BreakApartLine(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, Eps_Line _EvalLine, bool _bPromptForDirection, ref Eps_Line _FirstLine, ref Eps_Line _SecondLine, ref bool _bPickFirstLine)
	{
		Eps_Line eps_Line = null;
		Eps_Point _MidPoint = new Eps_Point();
		bool result = false;
		List<Eps_Point> list = new List<Eps_Point>();
		list.Add(_EvalLine.StartPnt);
		EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(_EvalLine.StartPnt, _EvalLine.EndPnt, ref _MidPoint);
		list.Add(_MidPoint);
		list.Add(_EvalLine.EndPnt);
		double _dDistance = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_EvalLine.StartPnt, _EvalLine.EndPnt, ref _dDistance);
		if (_dCircleRadius > _dDistance / 10.0)
		{
			_dCircleRadius = _dDistance / 10.0;
		}
		Eps_Point _UsePoint = new Eps_Point();
		if (PickSide(_Document, _TransactionContainer, _dCircleRadius, list, ref _UsePoint))
		{
			result = true;
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_EvalLine.StartPnt, _UsePoint))
			{
				_SecondLine = _EvalLine;
				_FirstLine = null;
				_bPickFirstLine = false;
				return true;
			}
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_EvalLine.EndPnt, _UsePoint))
			{
				_SecondLine = null;
				_FirstLine = _EvalLine.Reverse();
				_bPickFirstLine = true;
				return true;
			}
			EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(_EvalLine.StartPnt, _EvalLine.EndPnt, ref _MidPoint);
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_MidPoint, _UsePoint))
			{
				eps_Line = new Eps_Line();
				eps_Line.StartPnt = _EvalLine.StartPnt;
				eps_Line.EndPnt = _MidPoint;
				if (_FirstLine == null)
				{
					_FirstLine = eps_Line;
				}
				eps_Line = new Eps_Line();
				eps_Line.StartPnt = _MidPoint;
				eps_Line.EndPnt = _EvalLine.EndPnt;
				if (_SecondLine == null)
				{
					_SecondLine = eps_Line;
				}
			}
			if (_FirstLine != null)
			{
				_FirstLine = _FirstLine.Reverse();
				if (_bPromptForDirection && _SecondLine != null)
				{
					Eps_Point _FindPoint = new Eps_Point();
					Eps_Point _FindPoint2 = new Eps_Point();
					double _dDirection = 0.0;
					double _dDirection2 = 0.0;
					if (FindDistPointAndDirectionAlongLine(_FirstLine, _dCircleRadius, ref _FindPoint, ref _dDirection) && FindDistPointAndDirectionAlongLine(_SecondLine, _dCircleRadius, ref _FindPoint2, ref _dDirection2))
					{
						return PickDirection(_Document, _TransactionContainer, _dCircleRadius * 2.0, _FindPoint, _dDirection, _FindPoint2, _dDirection2, ref _bPickFirstLine);
					}
				}
			}
		}
		return result;
	}

	private static bool FindDistPointAndDirectionAlongLine(Eps_Line _EvalLine, double _dDistance, ref Eps_Point _FindPoint, ref double _dDirection)
	{
		_dDirection = 0.0;
		double _dDistance2 = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_EvalLine.StartPnt, _EvalLine.EndPnt, ref _dDistance2);
		if (_dDistance < _dDistance2)
		{
			EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(_EvalLine.StartPnt, _EvalLine.EndPnt, ref _dDirection);
			EPShared.eplGeoE_CalcPolarToXY(_EvalLine.StartPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection), _dDistance, ref _FindPoint);
			return true;
		}
		return false;
	}

	public static bool EvaluateLine(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, ref Eps_Line _EvalLine)
	{
		bool result = false;
		double _dDistance = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_EvalLine.StartPnt, _EvalLine.EndPnt, ref _dDistance);
		if (_dCircleRadius > _dDistance / 10.0)
		{
			_dCircleRadius = _dDistance / 10.0;
		}
		List<Eps_Point> list = new List<Eps_Point>();
		list.Add(_EvalLine.StartPnt);
		list.Add(_EvalLine.EndPnt);
		Eps_Point _UsePoint = new Eps_Point();
		if (PickSide(_Document, _TransactionContainer, _dCircleRadius, list, ref _UsePoint))
		{
			result = true;
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_EvalLine.EndPnt, _UsePoint))
			{
				_EvalLine = _EvalLine.Reverse();
			}
		}
		return result;
	}

	public static bool BreakApartArc(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, Eps_Arc _EvalArc, bool _bPromptForDirection, ref Eps_Arc _FirstArc, ref Eps_Arc _SecondArc, ref bool _bPickFirstArc)
	{
		Eps_Arc eps_Arc = null;
		Eps_Point _StaOffPnt = new Eps_Point();
		double num = 0.0;
		bool result = false;
		List<Eps_Point> list = new List<Eps_Point>();
		list.Add(_EvalArc.StartPnt);
		num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_EvalArc.Radius, _EvalArc.Delta);
		EPShared.eplGeoE_FindStaOffPointAlongArc(_EvalArc, num / 2.0, 0.0, ref _StaOffPnt);
		list.Add(_StaOffPnt);
		list.Add(_EvalArc.EndPnt);
		double _dDistance = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_EvalArc.StartPnt, _EvalArc.EndPnt, ref _dDistance);
		if (_dCircleRadius > _dDistance / 10.0)
		{
			_dCircleRadius = _dDistance / 10.0;
		}
		Eps_Point _UsePoint = new Eps_Point();
		if (PickSide(_Document, _TransactionContainer, _dCircleRadius, list, ref _UsePoint))
		{
			result = true;
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_EvalArc.StartPnt, _UsePoint))
			{
				_SecondArc = _EvalArc;
				_FirstArc = null;
				_bPickFirstArc = false;
				return true;
			}
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_EvalArc.EndPnt, _UsePoint))
			{
				_SecondArc = null;
				_FirstArc = _EvalArc.Reverse();
				_bPickFirstArc = true;
				return true;
			}
			num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_EvalArc.Radius, _EvalArc.Delta);
			EPShared.eplGeoE_FindStaOffPointAlongArc(_EvalArc, num / 2.0, 0.0, ref _StaOffPnt);
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_StaOffPnt, _UsePoint))
			{
				eps_Arc = new Eps_Arc(_EvalArc);
				eps_Arc.Delta = _EvalArc.Delta / 2.0;
				eps_Arc.StartPnt = _EvalArc.StartPnt;
				eps_Arc.EndPnt = _StaOffPnt;
				if (_FirstArc == null)
				{
					_FirstArc = eps_Arc;
				}
				eps_Arc = new Eps_Arc(_EvalArc);
				eps_Arc.Delta = _EvalArc.Delta / 2.0;
				eps_Arc.StartPnt = _StaOffPnt;
				eps_Arc.EndPnt = _EvalArc.EndPnt;
				if (_SecondArc == null)
				{
					_SecondArc = eps_Arc;
				}
			}
			if (_FirstArc != null)
			{
				_FirstArc = _FirstArc.Reverse();
				if (_bPromptForDirection && _SecondArc != null)
				{
					Eps_Point _FindPoint = new Eps_Point();
					Eps_Point _FindPoint2 = new Eps_Point();
					double _dDirection = 0.0;
					double _dDirection2 = 0.0;
					if (FindDistPointAndDirectionAlongArc(_FirstArc, _dCircleRadius, ref _FindPoint, ref _dDirection) && FindDistPointAndDirectionAlongArc(_SecondArc, _dCircleRadius, ref _FindPoint2, ref _dDirection2))
					{
						return PickDirection(_Document, _TransactionContainer, _dCircleRadius * 2.0, _FindPoint, _dDirection, _FindPoint2, _dDirection2, ref _bPickFirstArc);
					}
				}
			}
		}
		return result;
	}

	private static bool FindDistPointAndDirectionAlongArc(Eps_Arc _EvalArc, double _dDistance, ref Eps_Point _FindPoint, ref double _dDirection)
	{
		_dDirection = 0.0;
		double num = 0.0;
		num = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_EvalArc.Radius, _EvalArc.Delta);
		if (_dDistance < num)
		{
			EPShared.eplGeoE_FindStaOffPointAlongArc(_EvalArc, _dDistance, 0.0, ref _FindPoint);
			EPShared.eplGeoE_CalcTangentDirOfArc(_FindPoint, _EvalArc.RadiusPnt, _EvalArc.Direction, ref _dDirection);
			return true;
		}
		return false;
	}

	public static bool EvaluateArc(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, ref Eps_Arc _EvalArc)
	{
		bool result = false;
		double _dDistance = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_EvalArc.StartPnt, _EvalArc.EndPnt, ref _dDistance);
		if (_dCircleRadius > _dDistance / 10.0)
		{
			_dCircleRadius = _dDistance / 10.0;
		}
		List<Eps_Point> list = new List<Eps_Point>();
		list.Add(_EvalArc.StartPnt);
		list.Add(_EvalArc.EndPnt);
		Eps_Point _UsePoint = new Eps_Point();
		if (PickSide(_Document, _TransactionContainer, _dCircleRadius, list, ref _UsePoint))
		{
			result = true;
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_EvalArc.EndPnt, _UsePoint))
			{
				_EvalArc = _EvalArc.Reverse();
			}
		}
		return result;
	}

	public static bool PickSide(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, List<Eps_Point> _PointList, ref Eps_Point _UsePoint)
	{
		return PickSide(_Document, _TransactionContainer, _dCircleRadius, _PointList, ref _UsePoint, null, _bDisplayError: true, EPShared.LocalRM.GetString("PickStartingSideMessage"));
	}

	public static bool PickSide(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, List<Eps_Point> _PointList, ref Eps_Point _UsePoint, ElementSet _DeleteElementSet, bool _bDisplayError)
	{
		return PickSide(_Document, _TransactionContainer, _dCircleRadius, _PointList, ref _UsePoint, null, _bDisplayError: true, EPShared.LocalRM.GetString("PickStartingSideMessage"));
	}

	public static bool PickSide(Document _Document, EPTransactionContainer _TransactionContainer, double _dCircleRadius, List<Eps_Point> _PointList, ref Eps_Point _UsePoint, ElementSet _DeleteElementSet, bool _bDisplayError, string _strPrompt)
	{
		List<Eps_Arc> list = new List<Eps_Arc>();
		Eps_Arc eps_Arc = null;
		ModelCurve modelCurve = null;
		if (_DeleteElementSet == null)
		{
			_DeleteElementSet = new ElementSet();
			for (int i = 0; i < _PointList.Count; i++)
			{
				eps_Arc = new Eps_Arc();
				eps_Arc.Radius = _dCircleRadius;
				eps_Arc.EndPnt = _PointList[i];
				eps_Arc.RadiusPnt = _PointList[i];
				eps_Arc.StartPnt = _PointList[i];
				list.Add(eps_Arc);
				_TransactionContainer.BeginTransaction();
				modelCurve = NewModelCurve(_Document, eps_Arc);
				_DeleteElementSet.Insert(modelCurve);
				_TransactionContainer.EndTransaction();
			}
			_Document.Regenerate();
		}
		Eps_Point _Point = new Eps_Point();
		if (!GetPointFromProject(_Document, _strPrompt, ref _Point, _bDisplayError))
		{
			_TransactionContainer.BeginTransaction();
			_Document.Delete(ElementSetConverter.ToIDList(_DeleteElementSet));
			_TransactionContainer.EndTransaction();
			_Document.Regenerate();
			return false;
		}
		bool flag = false;
		if (_PointList.Count == list.Count)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (EPShared.eplGeoE_IsPointOnArc2D(_Point, list[j], 0.01))
				{
					flag = true;
					_UsePoint = _PointList[j];
					break;
				}
			}
		}
		if (!flag)
		{
			double dY = double.MaxValue;
			double _dDistance = 0.0;
			for (int k = 0; k < _PointList.Count; k++)
			{
				if (_PointList[k].IsEqual(_Point))
				{
					_UsePoint = _Point;
					break;
				}
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_PointList[k], _Point, ref _dDistance);
				if (RevitDefines.LT(_dDistance, dY))
				{
					dY = _dDistance;
					_UsePoint = _PointList[k];
				}
			}
		}
		_TransactionContainer.BeginTransaction();
		_Document.Delete(ElementSetConverter.ToIDList(_DeleteElementSet));
		_TransactionContainer.EndTransaction();
		_Document.Regenerate();
		return true;
	}

	private static bool PickDirection(Document _Document, EPTransactionContainer _TransactionContainer, double _dLineLength, Eps_Point _FirstPoint, double _dFirstDirection, Eps_Point _SecondPoint, double _dSecondDirection, ref bool _UseFirstPoint)
	{
		return PickDirection(_Document, _TransactionContainer, _dLineLength, _FirstPoint, _dFirstDirection, _SecondPoint, _dSecondDirection, ref _UseFirstPoint, null);
	}

	private static bool PickDirection(Document _Document, EPTransactionContainer _TransactionContainer, double _dLineLength, Eps_Point _FirstPoint, double _dFirstDirection, Eps_Point _SecondPoint, double _dSecondDirection, ref bool _UseFirstPoint, ElementSet _DeleteElementSet)
	{
		Eps_Point _NewPnt = new Eps_Point();
		Eps_Line eps_Line = new Eps_Line();
		Eps_Line eps_Line2 = new Eps_Line();
		Eps_Line eps_Line3 = new Eps_Line();
		Eps_Line eps_Line4 = new Eps_Line();
		Eps_Line eps_Line5 = new Eps_Line();
		Eps_Line eps_Line6 = new Eps_Line();
		if (_DeleteElementSet == null)
		{
			_DeleteElementSet = new ElementSet();
			_TransactionContainer.BeginTransaction();
			EPShared.eplGeoE_CalcPolarToXY(_FirstPoint, EPShared.eplGeoE_NormalizeRadians(_dFirstDirection + Math.PI / 2.0), _dLineLength / 2.0, ref _NewPnt);
			eps_Line.StartPnt = _NewPnt;
			EPShared.eplGeoE_CalcPolarToXY(_FirstPoint, EPShared.eplGeoE_NormalizeRadians(_dFirstDirection - Math.PI / 2.0), _dLineLength / 2.0, ref _NewPnt);
			eps_Line.EndPnt = _NewPnt;
			ModelCurve item = NewModelCurve(_Document, eps_Line);
			_DeleteElementSet.Insert(item);
			EPShared.eplGeoE_CalcPolarToXY(_FirstPoint, EPShared.eplGeoE_NormalizeRadians(_dFirstDirection), _dLineLength, ref _NewPnt);
			eps_Line2.StartPnt = _NewPnt;
			eps_Line2.EndPnt = eps_Line.StartPnt;
			item = NewModelCurve(_Document, eps_Line2);
			_DeleteElementSet.Insert(item);
			eps_Line3.StartPnt = _NewPnt;
			eps_Line3.EndPnt = eps_Line.EndPnt;
			item = NewModelCurve(_Document, eps_Line3);
			_DeleteElementSet.Insert(item);
			EPShared.eplGeoE_CalcPolarToXY(_SecondPoint, EPShared.eplGeoE_NormalizeRadians(_dSecondDirection + Math.PI / 2.0), _dLineLength / 2.0, ref _NewPnt);
			eps_Line4.StartPnt = _NewPnt;
			EPShared.eplGeoE_CalcPolarToXY(_SecondPoint, EPShared.eplGeoE_NormalizeRadians(_dSecondDirection - Math.PI / 2.0), _dLineLength / 2.0, ref _NewPnt);
			eps_Line4.EndPnt = _NewPnt;
			item = NewModelCurve(_Document, eps_Line4);
			_DeleteElementSet.Insert(item);
			EPShared.eplGeoE_CalcPolarToXY(_SecondPoint, EPShared.eplGeoE_NormalizeRadians(_dSecondDirection), _dLineLength, ref _NewPnt);
			eps_Line5.StartPnt = _NewPnt;
			eps_Line5.EndPnt = eps_Line4.StartPnt;
			item = NewModelCurve(_Document, eps_Line5);
			_DeleteElementSet.Insert(item);
			eps_Line6.StartPnt = _NewPnt;
			eps_Line6.EndPnt = eps_Line4.EndPnt;
			item = NewModelCurve(_Document, eps_Line6);
			_DeleteElementSet.Insert(item);
			_TransactionContainer.EndTransaction();
			_Document.Regenerate();
		}
		Eps_Point _Point = new Eps_Point();
		if (!GetPointFromProject(_Document, EPShared.LocalRM.GetString("PickStartingDirectionMessage"), ref _Point, _bDisplayError: true))
		{
			_TransactionContainer.BeginTransaction();
			_Document.Delete(ElementSetConverter.ToIDList(_DeleteElementSet));
			_TransactionContainer.EndTransaction();
			return false;
		}
		double _dDistance = 0.0;
		double _dDistance2 = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_FirstPoint, _Point, ref _dDistance);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_SecondPoint, _Point, ref _dDistance2);
		if (RevitDefines.LE(_dDistance, _dDistance2))
		{
			_UseFirstPoint = true;
		}
		else
		{
			_UseFirstPoint = false;
		}
		_TransactionContainer.BeginTransaction();
		_Document.Delete(ElementSetConverter.ToIDList(_DeleteElementSet));
		_TransactionContainer.EndTransaction();
		return true;
	}

	public static bool IsPointInPointList(Eps_Point _FindPoint, List<Eps_Point> _PointList)
	{
		for (int i = 0; i < _PointList.Count; i++)
		{
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_FindPoint, _PointList[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsArcInArcList(Eps_Arc _FindArc, List<Eps_Arc> _ArcList)
	{
		for (int i = 0; i < _ArcList.Count; i++)
		{
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_FindArc.RadiusPnt, _ArcList[i].RadiusPnt))
			{
				return true;
			}
		}
		return false;
	}

	public static KeyValuePair<string, string> CreatePlacementGenericModel(Document _Document, PlacementSettings _PlacementSettings, Element _Element)
	{
		Guid chainID = Guid.NewGuid();
		List<Element> list = new List<Element>();
		list.Add(_Element);
		List<KeyValuePair<string, string>> list2 = CreatePlacementGenericModel(_Document, _PlacementSettings, list, string.Empty, chainID);
		if (list2 != null && list2.Count > 0)
		{
			return list2[0];
		}
		return new KeyValuePair<string, string>(string.Empty, string.Empty);
	}

	public static List<KeyValuePair<string, string>> CreatePlacementGenericModel(Document _Document, PlacementSettings _PlacementSettings, List<Element> _CurveElementList)
	{
		Guid chainID = Guid.NewGuid();
		return CreatePlacementGenericModel(_Document, _PlacementSettings, _CurveElementList, string.Empty, chainID);
	}

	public static List<KeyValuePair<string, string>> CreatePlacementGenericModel(Document _Document, PlacementSettings _PlacementSettings, List<Element> _CurveElementList, string _strBoundaryID, Guid _ChainID)
	{
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		_PlacementSettings.TransactionContainer.BeginTransaction();
		if (_CurveElementList != null && _PlacementSettings.PlacementSymbol != null && _PlacementSettings.ExistingElementChecked && !_PlacementSettings.DeleteExistingChecked)
		{
			if (_PlacementSettings.SurfaceList != null && _PlacementSettings.SurfaceList.Count > 0)
			{
				FamilySymbolParameterContainer item = new FamilySymbolParameterContainer(RevitParameters.ToposurfaceIDFamilyParameter, BuiltInParameterGroup.PG_DATA, ParameterType.Text, _bIsInstance: true);
				List<FamilySymbolParameterContainer> list2 = new List<FamilySymbolParameterContainer>();
				FamilySymbol _FamilySymbol = _PlacementSettings.PlacementSymbol;
				list2.Add(item);
				AddParametersToFamilySymbol(_Document, _PlacementSettings.TransactionContainer, list2, ref _FamilySymbol);
				_PlacementSettings.PlacementSymbol = _FamilySymbol;
			}
			FamilyInstance familyInstance = null;
			int num = 0;
			bool flag = false;
			foreach (Element _CurveElement in _CurveElementList)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (string.Compare(list[i].Key, _CurveElement.UniqueId, ignoreCase: true) == 0)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					flag = false;
					continue;
				}
				EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
				familyInstance = NewFamilyInstance(_Document, new XYZ(0.0, 0.0, 0.0), _PlacementSettings.PlacementSymbol, _CurveElement, _PlacementSettings.Level, StructuralType.NonStructural, ref _TransactionContainer);
				_PlacementSettings.TransactionContainer = _TransactionContainer;
				if (familyInstance == null)
				{
					continue;
				}
				ElementSet elementSet = new ElementSet();
				elementSet.Insert(familyInstance);
				_Document.ActiveView.HideElements(ElementSetConverter.ToIDList(elementSet));
				Parameter parameter = familyInstance.LookupParameter(RevitParameters.HeightGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.Height);
				}
				parameter = familyInstance.LookupParameter(RevitParameters.HeightVarianceGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.HeightVariance);
				}
				parameter = familyInstance.LookupParameter(RevitParameters.WidthGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.Width);
				}
				parameter = familyInstance.LookupParameter(RevitParameters.WidthVarianceGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.WidthVariance);
				}
				parameter = familyInstance.LookupParameter(RevitParameters.SpacingGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.PlantSpacing);
				}
				parameter = familyInstance.LookupParameter(RevitParameters.SetbackGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.SetbackSpacing);
				}
				parameter = familyInstance.LookupParameter(RevitParameters.GroveRotationGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.RotationAngle);
				}
				parameter = familyInstance.LookupParameter(RevitParameters.IsSingleSegmentGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					if (_CurveElementList.Count > 1)
					{
						parameter.Set(Convert.ToInt32(value: true));
					}
					else
					{
						parameter.Set(Convert.ToInt32(value: false));
					}
				}
				parameter = familyInstance.LookupParameter(RevitParameters.IsFirstPlacementSegmentGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					if (num == 0)
					{
						parameter.Set(Convert.ToInt32(value: true));
						if (_PlacementSettings.FirstLegSegmentID == string.Empty)
						{
							_PlacementSettings.FirstLegSegmentID = _CurveElement.UniqueId;
						}
					}
					else
					{
						if (num == _CurveElementList.Count() - 1 && _PlacementSettings.LastLegSegmentID == string.Empty)
						{
							_PlacementSettings.LastLegSegmentID = _CurveElement.UniqueId;
						}
						parameter.Set(Convert.ToInt32(value: false));
					}
				}
				num++;
				parameter = familyInstance.LookupParameter(RevitParameters.IsFirstLegSegmentGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					if (string.Compare(_CurveElement.UniqueId, _PlacementSettings.FirstLegSegmentID, ignoreCase: true) == 0)
					{
						parameter.Set(Convert.ToInt32(value: true));
					}
					else
					{
						parameter.Set(Convert.ToInt32(value: false));
					}
				}
				parameter = familyInstance.LookupParameter(RevitParameters.IsLastLegSegmentGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					if (string.Compare(_CurveElement.UniqueId, _PlacementSettings.LastLegSegmentID, ignoreCase: true) == 0)
					{
						parameter.Set(Convert.ToInt32(value: true));
					}
					else
					{
						parameter.Set(Convert.ToInt32(value: false));
					}
				}
				parameter = familyInstance.LookupParameter(RevitParameters.IsReverseGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(Convert.ToInt32(_PlacementSettings.IsReverse));
				}
				parameter = familyInstance.LookupParameter(RevitParameters.PlacementTypeGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.PlacementType.GetHashCode());
				}
				parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_CurveElement.UniqueId);
				}
				if (_PlacementSettings.Type != null)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.SymbolIDGenericFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Type.UniqueId);
					}
				}
				parameter = familyInstance.LookupParameter(RevitParameters.UseRightSideGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(Convert.ToInt32(_PlacementSettings.UseRightSide));
				}
				if (_strBoundaryID != string.Empty)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.BoundaryIslandIDGenericFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_strBoundaryID);
					}
				}
				if (_ChainID != Guid.Empty)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_ChainID.ToString());
					}
				}
				parameter = familyInstance.LookupParameter(RevitParameters.AlignToGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(Convert.ToInt32(_PlacementSettings.AlignTo));
				}
				parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.LocateType.GetHashCode());
				}
				parameter = familyInstance.LookupParameter(RevitParameters.StartingDistanceGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.StartingDistance);
				}
				if (_PlacementSettings.SurfaceList != null && _PlacementSettings.SurfaceList.Count > 0)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.ToposurfaceIDFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.SurfaceList[0].TopographySurface.UniqueId);
					}
				}
				if (_PlacementSettings.Parking != null)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.ADAStallWidthGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.ADAStallWidth);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.ADAStallCountGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.ADAStallCount);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.BumperHeightGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.BumperHeight);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.BumperWidthGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.BumperWidth);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.BumperOffsetGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.BumperOffset);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.BumperSetbackGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.BumperSetback);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.IslandHeightGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.IslandHeight);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.IslandWidthGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.IslandWidth);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.StallLengthGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.StallLength);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.AreaDistanceBetweenCenterlinesGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.AreaDistanceBetweenCenterlines);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.AreaAisleWidthGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.AreaAisleWidth);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.PlaceIslandGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(Convert.ToInt32(_PlacementSettings.Parking.PlaceIsland));
					}
					parameter = familyInstance.LookupParameter(RevitParameters.PlaceBumperGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(Convert.ToInt32(_PlacementSettings.Parking.PlaceBumper));
					}
					parameter = familyInstance.LookupParameter(RevitParameters.OptimizeParkingGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(Convert.ToInt32(_PlacementSettings.Parking.OptimizeParking));
					}
					parameter = familyInstance.LookupParameter(RevitParameters.AreaBidirectionalAislesGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(Convert.ToInt32(_PlacementSettings.Parking.AreaBiDirectionalAisles));
					}
					parameter = familyInstance.LookupParameter(RevitParameters.StallCountGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.StallCount);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.AreaRowCountGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.AreaRowCount);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.AreaRowSpacingOptionsGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Parking.AreaRowSpacingOptions);
					}
					if (_PlacementSettings.Parking.ADAStallSymbol != null)
					{
						parameter = familyInstance.LookupParameter(RevitParameters.ADAStallSymbolIDGenericFamilyParameter);
						if (parameter != null && !parameter.IsReadOnly)
						{
							parameter.Set(_PlacementSettings.Parking.ADAStallSymbol.UniqueId);
						}
					}
					if (_PlacementSettings.Parking.IslandSymbol != null)
					{
						parameter = familyInstance.LookupParameter(RevitParameters.IslandSymbolIDGenericFamilyParameter);
						if (parameter != null && !parameter.IsReadOnly)
						{
							parameter.Set(_PlacementSettings.Parking.IslandSymbol.UniqueId);
						}
					}
					if (_PlacementSettings.Parking.BumperSymbol != null)
					{
						parameter = familyInstance.LookupParameter(RevitParameters.BumperSymbolIDGenericFamilyParameter);
						if (parameter != null && !parameter.IsReadOnly)
						{
							parameter.Set(_PlacementSettings.Parking.BumperSymbol.UniqueId);
						}
					}
					parameter = familyInstance.LookupParameter(RevitParameters.MiddleStallPlacementGenericFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(Convert.ToInt32(_PlacementSettings.Parking.MiddleStallPlacement));
					}
					parameter = familyInstance.LookupParameter(RevitParameters.ADAStallSharedAccessibleAisleGenericModelFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(Convert.ToInt32(_PlacementSettings.Parking.ADAStallSharedAccessibleAisle));
					}
				}
				list.Add(new KeyValuePair<string, string>(_CurveElement.UniqueId, familyInstance.UniqueId));
			}
		}
		_PlacementSettings.TransactionContainer.EndTransaction();
		return list;
	}

	public static FamilyInstance CreatePlacementSiteComponent(Document _Document, SitePlacementSettings _PlacementSettings, Element _Element)
	{
		Guid chainID = Guid.NewGuid();
		List<Element> list = new List<Element>();
		list.Add(_Element);
		List<FamilyInstance> list2 = CreatePlacementSiteComponent(_Document, _PlacementSettings, list, chainID, _bIsParkingLotBreakline: false);
		if (list2 != null && list2.Count > 0)
		{
			return list2[0];
		}
		return null;
	}

	public static List<FamilyInstance> CreatePlacementSiteComponent(Document _Document, SitePlacementSettings _PlacementSettings, List<Element> _CurveElementList)
	{
		Guid chainID = Guid.NewGuid();
		return CreatePlacementSiteComponent(_Document, _PlacementSettings, _CurveElementList, chainID, _bIsParkingLotBreakline: false);
	}

	public static List<FamilyInstance> CreatePlacementSiteComponent(Document _Document, SitePlacementSettings _PlacementSettings, List<Element> _CurveElementList, Guid _ChainID)
	{
		List<double> list = new List<double>();
		list.Add(_PlacementSettings.LinearPointInterval);
		return CreatePlacementSiteComponent(_Document, _PlacementSettings, _CurveElementList, list, _ChainID, _bIsParkingLotBreakline: false);
	}

	public static List<FamilyInstance> CreatePlacementSiteComponent(Document _Document, SitePlacementSettings _PlacementSettings, List<Element> _CurveElementList, Guid _ChainID, bool _bIsParkingLotBreakline)
	{
		List<double> list = new List<double>();
		list.Add(_PlacementSettings.LinearPointInterval);
		return CreatePlacementSiteComponent(_Document, _PlacementSettings, _CurveElementList, list, _ChainID, _bIsParkingLotBreakline);
	}

	public static List<FamilyInstance> CreatePlacementSiteComponent(Document _Document, SitePlacementSettings _PlacementSettings, List<Element> _CurveElementList, List<double> _LinearPointIntervals, Guid _ChainID, bool _bIsParkingLotBreakline)
	{
		List<FamilyInstance> list = new List<FamilyInstance>();
		EPTransactionContainer ePTransactionContainer = null;
		FamilyInstance familyInstance = null;
		Parameter parameter = null;
		_PlacementSettings.TransactionContainer.BeginTransaction();
		ElementSet elementSet = new ElementSet();
		if (_CurveElementList != null && _PlacementSettings.PlacementSymbol != null)
		{
			int num = 0;
			foreach (Element _CurveElement in _CurveElementList)
			{
				ePTransactionContainer = _PlacementSettings.TransactionContainer;
				familyInstance = NewFamilyInstance(_Document, new XYZ(0.0, 0.0, 0.0), _PlacementSettings.PlacementSymbol, _CurveElement, _PlacementSettings.Level, StructuralType.NonStructural, ref ePTransactionContainer);
				_PlacementSettings.TransactionContainer = ePTransactionContainer;
				elementSet.Insert(familyInstance);
				parameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.LocateType.GetHashCode().ToString("000"));
				}
				parameter = familyInstance.LookupParameter(RevitParameters.IsSingleSegmentGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					if (_CurveElementList.Count > 1)
					{
						parameter.Set(Convert.ToInt32(value: true));
					}
					else
					{
						parameter.Set(Convert.ToInt32(value: false));
					}
				}
				parameter = familyInstance.LookupParameter(RevitParameters.IsFirstPlacementSegmentGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					if (num == 0)
					{
						parameter.Set(Convert.ToInt32(value: true));
					}
					else
					{
						parameter.Set(Convert.ToInt32(value: false));
					}
				}
				parameter = familyInstance.LookupParameter(RevitParameters.IsReverseGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(Convert.ToInt32(_PlacementSettings.IsReverse));
				}
				parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_CurveElement.UniqueId);
				}
				if (_PlacementSettings.Type != null)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.SymbolIDGenericFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Type.UniqueId);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.FamilySymbolNameFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Type.Name);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.FamilyNameFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.Type.Family.Name);
					}
				}
				if (_PlacementSettings.LoftFormID != string.Empty)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.PrimaryFormIDFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.LoftFormID);
					}
				}
				if (_PlacementSettings.AdditionalLoftFormID != string.Empty)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.AdditionalFormIDFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.AdditionalLoftFormID);
					}
				}
				if (_PlacementSettings.IslandSurfaceID != string.Empty)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.IslandSurfaceIdFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.IslandSurfaceID);
					}
				}
				if (_PlacementSettings.ParkingLotSurfaceID != string.Empty)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.ParkingLotSurfaceIDFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.ParkingLotSurfaceID);
					}
				}
				if (_PlacementSettings.ParentTopoSurfaceID != string.Empty)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.ParentSurfaceIDFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.ParentTopoSurfaceID);
					}
				}
				parameter = familyInstance.LookupParameter(RevitParameters.UseRightSideGenericFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(Convert.ToInt32(_PlacementSettings.UseRightSide));
				}
				if (_ChainID != Guid.Empty)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_ChainID.ToString());
					}
				}
				parameter = familyInstance.LookupParameter(RevitParameters.ProjectInnerSlopesFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.ProjectInnerSlopes);
				}
				parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentNameFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.Name);
				}
				parameter = familyInstance.LookupParameter(RevitParameters.TopoDrawOrderFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.TopoDrawOrder);
				}
				parameter = familyInstance.LookupParameter(RevitParameters.ElevationFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(_PlacementSettings.Elevation);
				}
				if (num < _PlacementSettings.BeginningElevationList.Count)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.BeginningElevationFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.BeginningElevationList[num]);
					}
				}
				if (num < _PlacementSettings.EndingElevationList.Count)
				{
					parameter = familyInstance.LookupParameter(RevitParameters.EndingElevationFamilyParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.EndingElevationList[num]);
					}
				}
				parameter = familyInstance.LookupParameter(RevitParameters.UsePlacementElevationsFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					if (num < _PlacementSettings.BeginningElevationList.Count && num < _PlacementSettings.EndingElevationList.Count)
					{
						parameter.Set(Convert.ToInt32(value: true));
					}
					else
					{
						parameter.Set(Convert.ToInt32(value: false));
					}
				}
				parameter = familyInstance.LookupParameter(RevitParameters.SiteComponentIsRelativeElevationParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(Convert.ToInt32(_PlacementSettings.IsRelativeElevation));
				}
				parameter = familyInstance.LookupParameter(RevitParameters.IsBreaklineFamilyParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(Convert.ToInt32(_bIsParkingLotBreakline));
				}
				parameter = familyInstance.LookupParameter(RevitParameters.LinearPointIntervalParameter);
				if (parameter != null && !parameter.IsReadOnly)
				{
					if (num < _LinearPointIntervals.Count)
					{
						parameter.Set(_LinearPointIntervals[num]);
					}
					else
					{
						parameter.Set(_LinearPointIntervals[0]);
					}
				}
				list.Add(familyInstance);
				num++;
			}
		}
		if (_PlacementSettings.IntersectionStations != null && _PlacementSettings.IntersectionStations.Count > 0 && _PlacementSettings.IntersectionPlacementSymbol != null)
		{
			for (int i = 0; i < _PlacementSettings.IntersectionStations.Count; i++)
			{
				for (int j = 0; j < _PlacementSettings.IntersectionStations[i].IntersectionStations.Count; j++)
				{
					ePTransactionContainer = _PlacementSettings.TransactionContainer;
					familyInstance = NewFamilyInstance(_Document, new XYZ(0.0, 0.0, 0.0), _PlacementSettings.IntersectionPlacementSymbol, null, _PlacementSettings.Level, StructuralType.NonStructural, ref ePTransactionContainer);
					_PlacementSettings.TransactionContainer = ePTransactionContainer;
					elementSet.Insert(familyInstance);
					parameter = familyInstance.LookupParameter(RevitParameters.PrimaryPlacementIDParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.IntersectionStations[i].PrimaryPlacementId);
					}
					if (list.Count > 0)
					{
						parameter = familyInstance.LookupParameter(RevitParameters.SecondaryPlacementIDParameter);
						if (parameter != null && !parameter.IsReadOnly)
						{
							parameter.Set(list[0].UniqueId);
						}
					}
					parameter = familyInstance.LookupParameter(RevitParameters.IsStartingParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(Convert.ToInt32(_PlacementSettings.IntersectionStations[i].IntersectionStations[j].StartingIntersection));
					}
					parameter = familyInstance.LookupParameter(RevitParameters.RadiusParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.IntersectionStations[i].IntersectionStations[j].Radius);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.SecondaryStationParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.IntersectionStations[i].IntersectionStations[j].Station);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.PrimaryStationParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.IntersectionStations[i].IntersectionStations[j].PrimaryStation);
					}
					parameter = familyInstance.LookupParameter(RevitParameters.IsSecondaryLeftParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(Convert.ToInt32(_PlacementSettings.IntersectionStations[i].IntersectionStations[j].IsLeft));
					}
					parameter = familyInstance.LookupParameter(RevitParameters.IsPrimaryLeftParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(Convert.ToInt32(_PlacementSettings.IntersectionStations[i].IntersectionStations[j].IsPrimaryLeft));
					}
					parameter = familyInstance.LookupParameter(RevitParameters.TransitioningDistanceParameter);
					if (parameter != null && !parameter.IsReadOnly)
					{
						parameter.Set(_PlacementSettings.IntersectionStations[i].IntersectionStations[j].TransitioningDistance);
					}
				}
			}
		}
		if (elementSet.Size > 0)
		{
			HideElementsInAllViews(_Document, elementSet);
		}
		_PlacementSettings.TransactionContainer.EndTransaction();
		return list;
	}

	public static List<FamilyInstanceComponentWrapper> FindAllSiteComponents(Document _Document)
	{
		EPTransactionContainer transactionContainer = new EPTransactionContainer(_Document);
		int nMaxExistingTopoDrawOrder = -1;
		return FindSiteCategoryComponents(_Document, transactionContainer, string.Empty, ref nMaxExistingTopoDrawOrder);
	}

	public static List<FamilyInstanceComponentWrapper> FindAllSiteComponents(Document _Document, EPTransactionContainer _TransactionContainer)
	{
		int nMaxExistingTopoDrawOrder = -1;
		return FindSiteCategoryComponents(_Document, _TransactionContainer, string.Empty, ref nMaxExistingTopoDrawOrder);
	}

	public static List<FamilyInstanceComponentWrapper> FindAllSiteComponents(Document _Document, EPTransactionContainer _TransactionContainer, ref int _nMaxExistingTopoDrawOrder)
	{
		return FindSiteCategoryComponents(_Document, _TransactionContainer, string.Empty, ref _nMaxExistingTopoDrawOrder);
	}

	public static List<FamilyInstanceComponentWrapper> FindSiteCategoryComponents(Document _Document, EPTransactionContainer _TransactionContainer, string _strLandcaddID)
	{
		int nMaxExistingTopoDrawOrder = -1;
		return FindSiteCategoryComponents(_Document, _TransactionContainer, _strLandcaddID, ref nMaxExistingTopoDrawOrder);
	}

	private static List<FamilyInstanceComponentWrapper> FindSiteCategoryComponents(Document _Document, EPTransactionContainer _TransactionContainer, string _strLandcaddID, ref int nMaxExistingTopoDrawOrder)
	{
		List<FamilyInstanceComponentWrapper> list = new List<FamilyInstanceComponentWrapper>();
		List<Element> list2 = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		Parameter LocateTypeParameter = null;
		List<KeyValuePair<string, Category>> MassLineSubCategoryList = null;
		bool _bDoesMassLineSubCategoryExist = false;
		nMaxExistingTopoDrawOrder = -1;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0 && (LocateTypeParameter = FindFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter)) != null && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list2 = source.ToList();
		if (list2.Count == 0)
		{
			return list;
		}
		filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.WhereElementIsViewIndependent();
		filteredElementCollector.OfClass(typeof(CurveElement)).OfCategory(BuiltInCategory.OST_Lines);
		bool flag = _TransactionContainer.Start(EPShared.LocalRM.GetString("CreateLineSubCategoriesTransaction"));
		if (_strLandcaddID == string.Empty)
		{
			MassLineSubCategoryList = GetAllMassLineSubCategories(_Document, ref _bDoesMassLineSubCategoryExist);
		}
		else
		{
			LandcaddID curLandcaddId = LandcaddID.SoftTerrain;
			foreach (int value in Enum.GetValues(typeof(LandcaddID)))
			{
				if (string.Compare(_strLandcaddID, value.ToString("000"), ignoreCase: true) == 0)
				{
					curLandcaddId = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value);
					break;
				}
			}
			Category siteComponentLineSubCategory = GetSiteComponentLineSubCategory(_Document, curLandcaddId, ref _bDoesMassLineSubCategoryExist);
			if (!_bDoesMassLineSubCategoryExist)
			{
				MassLineSubCategoryList = GetAllMassLineSubCategories(_Document, ref _bDoesMassLineSubCategoryExist);
			}
			else if (siteComponentLineSubCategory != null)
			{
				MassLineSubCategoryList = new List<KeyValuePair<string, Category>>();
				MassLineSubCategoryList.Add(new KeyValuePair<string, Category>(_strLandcaddID, siteComponentLineSubCategory));
			}
		}
		List<Element> list3 = null;
		if (_bDoesMassLineSubCategoryExist && MassLineSubCategoryList != null && MassLineSubCategoryList.Count > 0)
		{
			for (int nIndex = 0; nIndex < MassLineSubCategoryList.Count; nIndex++)
			{
				if (MassLineSubCategoryList[nIndex].Value != null)
				{
					Parameter LineStyleParameter = null;
					CurveElement FindCurveElement = null;
					IEnumerable<Element> source2 = filteredElementCollector.Where((Element CurrentElement) => (FindCurveElement = CurrentElement as CurveElement) != null && (LineStyleParameter = ((Element)FindCurveElement).get_Parameter(BuiltInParameter.BUILDING_CURVE_GSTYLE)) != null && LineStyleParameter.AsElementId() == MassLineSubCategoryList[nIndex].Value.GetGraphicsStyle(GraphicsStyleType.Projection).Id);
					if (nIndex == 0)
					{
						list3 = source2.ToList();
					}
					else
					{
						list3.AddRange(source2.ToList());
					}
				}
			}
		}
		else
		{
			list3 = filteredElementCollector.ToList();
		}
		FamilyInstanceComponentContainer familyInstanceComponentContainer = null;
		Parameter parameter = null;
		Parameter parameter2 = null;
		Parameter parameter3 = null;
		bool flag2 = false;
		ModelCurve modelCurve = null;
		foreach (Element item in list2)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
			{
				continue;
			}
			SiteworksIDParameter = familyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (SiteworksIDParameter == null)
			{
				continue;
			}
			LocateTypeParameter = familyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (LocateTypeParameter == null || string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) != 0)
			{
				continue;
			}
			parameter = familyInstance.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter);
			if (parameter == null)
			{
				continue;
			}
			parameter2 = familyInstance.LookupParameter(RevitParameters.ChainIDGenericFamilyParameter);
			if (parameter2 == null || !(parameter2.AsString() != string.Empty))
			{
				continue;
			}
			for (int num = 0; num < list3.Count; num++)
			{
				Element element = list3[num];
				if (element == null || element.Category == null || element.Category.Id.Value64() != BuiltInCategory.OST_Lines.GetHashCode() || string.Compare(parameter.AsString(), element.UniqueId, ignoreCase: true) != 0)
				{
					continue;
				}
				if (MassLineSubCategoryList != null && !_bDoesMassLineSubCategoryExist)
				{
					for (int num2 = 0; num2 < MassLineSubCategoryList.Count; num2++)
					{
						if (string.Compare(MassLineSubCategoryList[num2].Key, LocateTypeParameter.AsString(), ignoreCase: true) == 0)
						{
							if (MassLineSubCategoryList[num2].Value != null && element is ModelCurve modelCurve2)
							{
								modelCurve2.LineStyle = MassLineSubCategoryList[num2].Value.GetGraphicsStyle(GraphicsStyleType.Projection);
							}
							break;
						}
					}
				}
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					if (string.Compare(parameter2.AsString(), list[num3].ChainID, ignoreCase: true) == 0)
					{
						flag2 = true;
						if (_strLandcaddID == string.Empty || string.Compare(LocateTypeParameter.AsString(), _strLandcaddID, ignoreCase: true) == 0)
						{
							familyInstanceComponentContainer = new FamilyInstanceComponentContainer(element, familyInstance);
							list[num3].FamilyContainerList.Add(familyInstanceComponentContainer);
						}
						break;
					}
				}
				if (!flag2)
				{
					int num4 = 0;
					parameter3 = familyInstance.LookupParameter(RevitParameters.TopoDrawOrderFamilyParameter);
					if (parameter3 != null)
					{
						num4 = parameter3.AsInteger();
						if (nMaxExistingTopoDrawOrder < num4)
						{
							nMaxExistingTopoDrawOrder = parameter3.AsInteger();
						}
					}
					if (_strLandcaddID == string.Empty || string.Compare(LocateTypeParameter.AsString(), _strLandcaddID, ignoreCase: true) == 0)
					{
						List<FamilyInstanceComponentContainer> list4 = new List<FamilyInstanceComponentContainer>();
						familyInstanceComponentContainer = new FamilyInstanceComponentContainer(element, familyInstance);
						list4.Add(familyInstanceComponentContainer);
						list.Add(new FamilyInstanceComponentWrapper(parameter2.AsString(), list4, num4));
					}
				}
				else
				{
					flag2 = false;
				}
				list3.RemoveAt(num);
				break;
			}
			if (list3.Count == 0)
			{
				break;
			}
		}
		if (flag)
		{
			if (!_bDoesMassLineSubCategoryExist)
			{
				_TransactionContainer.Commit();
			}
			else
			{
				_TransactionContainer.Rollback();
			}
		}
		return list;
	}

	public static int FindMaxTopoDrawOrderFromSiteComponents(Document _Document)
	{
		List<Element> list = null;
		FamilyInstance FindFamilyInstance = null;
		Parameter SiteworksIDParameter = null;
		Parameter LocateTypeParameter = null;
		Parameter parameter = null;
		int num = -1;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (FindFamilyInstance = CurrentElement as FamilyInstance) != null && (SiteworksIDParameter = FindFamilyInstance.Symbol.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(SiteworksIDParameter.AsString(), Convert.ToInt32(LandcaddID.SiteComponentPlacement).ToString("000"), ignoreCase: true) == 0 && (LocateTypeParameter = FindFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter)) != null && CurrentElement.LookupParameter(RevitParameters.UniqueIDGenericModelFamilyParameter) != null);
		list = source.ToList();
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance) || familyInstance.Category.Id.Value64() != BuiltInCategory.OST_Site.GetHashCode())
			{
				continue;
			}
			int num2 = 0;
			parameter = familyInstance.LookupParameter(RevitParameters.TopoDrawOrderFamilyParameter);
			if (parameter != null)
			{
				num2 = parameter.AsInteger();
				if (num < num2)
				{
					num = parameter.AsInteger();
				}
			}
		}
		return num;
	}

	public static bool RemoveAllSiteComponents(Document _Document, EPTransactionContainer _TransactionContainer)
	{
		return RemoveAllSiteComponents(_Document, _TransactionContainer, _bIssueWarningForRemoval: false);
	}

	public static bool RemoveAllSiteComponents(Document _Document, EPTransactionContainer _TransactionContainer, bool _bIssueWarningForRemoval)
	{
		bool result = false;
		List<FamilyInstanceComponentWrapper> list = FindAllSiteComponents(_Document, _TransactionContainer);
		ElementSet elementSet = new ElementSet();
		ElementSet elementSet2 = null;
		if (list.Count > 0)
		{
			if (_bIssueWarningForRemoval && RevitMessageBox.Show(EPShared.LocalRM.GetString("SiteComponentsFoundInProjectMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
			{
				return false;
			}
			for (int i = 0; i < list.Count; i++)
			{
				elementSet2 = list[i].GetFamilyInstanceElementSet();
				foreach (Element item3 in elementSet2)
				{
					elementSet.Insert(item3);
				}
				elementSet2 = list[i].GetOriginalElementSet();
				foreach (Element item4 in elementSet2)
				{
					elementSet.Insert(item4);
				}
			}
		}
		if (elementSet.Size > 0)
		{
			result = true;
			_TransactionContainer.BeginTransaction();
			_Document.Delete(ElementSetConverter.ToIDList(elementSet));
			_TransactionContainer.EndTransaction();
		}
		return result;
	}

	public static void AddParameterToFamilySymbol(Document _Document, EPTransactionContainer _TransactionContainer, FamilySymbolParameterContainer FamilySymbolParameterContainer, ref FamilySymbol _FamilySymbol)
	{
		List<FamilySymbolParameterContainer> list = new List<FamilySymbolParameterContainer>();
		list.Add(FamilySymbolParameterContainer);
		AddParametersToFamilySymbol(_Document, _TransactionContainer, list, ref _FamilySymbol);
	}

	public static void AddParametersToFamilySymbol(Document _Document, EPTransactionContainer _TransactionContainer, List<FamilySymbolParameterContainer> FamilySymbolParameterContainerList, ref FamilySymbol _FamilySymbol)
	{
		bool flag = _TransactionContainer.HasStarted();
		string name = _TransactionContainer.GetName();
		if (flag)
		{
			_TransactionContainer.Rollback();
		}
		if (_FamilySymbol != null && _FamilySymbol.Family != null)
		{
			Document document = _Document.EditFamily(_FamilySymbol.Family);
			if (document != null)
			{
				EPTransactionContainer ePTransactionContainer = new EPTransactionContainer(document);
				ePTransactionContainer.Start(EPShared.LocalRM.GetString("AddParametersToFamilyTransaction"));
				ePTransactionContainer.BeginTransaction();
				bool flag2 = false;
				for (int i = 0; i < FamilySymbolParameterContainerList.Count; i++)
				{
					if (FamilySymbolParameterContainerList[i].FamilyDefinition != null)
					{
						if (document.FamilyManager.get_Parameter((Definition)FamilySymbolParameterContainerList[i].FamilyDefinition) == null)
						{
							FamilyParameter familyParameter = document.FamilyManager.AddParameter(FamilySymbolParameterContainerList[i].FamilyDefinition, FamilySymbolParameterContainerList[i].BuiltInParameterGroup, FamilySymbolParameterContainerList[i].IsInstance);
							flag2 = true;
						}
					}
					else if (FamilySymbolParameterContainerList[i].FamilyCategory != null)
					{
						if (FamilySymbolParameterContainerList[i].ParameterName != string.Empty && document.FamilyManager.get_Parameter(FamilySymbolParameterContainerList[i].ParameterName) == null)
						{
							FamilyParameter familyParameter2 = document.FamilyManager.AddParameter(FamilySymbolParameterContainerList[i].ParameterName, FamilySymbolParameterContainerList[i].BuiltInParameterGroup, FamilySymbolParameterContainerList[i].FamilyCategory, FamilySymbolParameterContainerList[i].IsInstance);
							flag2 = true;
						}
					}
					else if (FamilySymbolParameterContainerList[i].ParameterName != string.Empty && document.FamilyManager.get_Parameter(FamilySymbolParameterContainerList[i].ParameterName) == null)
					{
						FamilyParameter familyParameter3 = document.FamilyManager.AddParameter(FamilySymbolParameterContainerList[i].ParameterName, FamilySymbolParameterContainerList[i].BuiltInParameterGroup, FamilySymbolParameterContainerList[i].ParameterType, FamilySymbolParameterContainerList[i].IsInstance);
						flag2 = true;
					}
				}
				ePTransactionContainer.EndTransaction();
				if (flag2)
				{
					ePTransactionContainer.Commit();
					string empty = string.Empty;
					string name2 = _FamilySymbol.Name;
					bool flag3 = false;
					if (File.Exists(document.PathName))
					{
						empty = document.PathName;
						flag3 = true;
					}
					else
					{
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
						empty = Path.Combine(_strSupportPath, document.Title);
					}
					if (empty != string.Empty)
					{
						SaveAsOptions saveAsOptions = new SaveAsOptions();
						saveAsOptions.OverwriteExistingFile = true;
						document.SaveAs(empty, saveAsOptions);
						document.Close(saveModified: false);
						bool flag4 = _TransactionContainer.Start(EPShared.LocalRM.GetString("ReloadFamilyTransaction") + Path.GetFileName(empty));
						FamilyLoadOptions familyLoadOptions = new FamilyLoadOptions(_bOverwriteParameterValues: true);
						LoadFamilySymbol(_Document, empty, name2, familyLoadOptions, out _FamilySymbol, _bUseFailureHandling: true, ref _TransactionContainer);
						if (flag4)
						{
							_TransactionContainer.Commit();
						}
						if (!flag3)
						{
							File.Delete(empty);
						}
					}
				}
				else
				{
					ePTransactionContainer.Rollback();
					document.Close(saveModified: false);
				}
			}
		}
		if (flag)
		{
			_TransactionContainer.Start(name);
		}
	}

	public static void HideAllSiteComponents(Document _Document, Autodesk.Revit.DB.View _View)
	{
		ChangeVisibilityOfAllSiteComponents(_Document, _View, _bShow: false);
	}

	public static void ShowAllSiteComponents(Document _Document, Autodesk.Revit.DB.View _View)
	{
		ChangeVisibilityOfAllSiteComponents(_Document, _View, _bShow: true);
	}

	private static void ChangeVisibilityOfAllSiteComponents(Document _Document, Autodesk.Revit.DB.View _View, bool _bShow)
	{
		if (_View.GenLevel == null && !(_View is View3D) && !(_View is ViewSection))
		{
			return;
		}
		List<FamilyInstanceComponentWrapper> list = FindAllSiteComponents(_Document);
		ElementSet elementSet = new ElementSet();
		ElementSet elementSet2 = null;
		for (int i = 0; i < list.Count; i++)
		{
			if (!_bShow)
			{
				elementSet2 = list[i].GetFamilyInstanceElementSet();
				foreach (Element item in elementSet2)
				{
					if (item.IsHidden(_View) == _bShow)
					{
						elementSet.Insert(item);
					}
				}
			}
			elementSet2 = list[i].GetOriginalElementSet();
			foreach (Element item2 in elementSet2)
			{
				if (item2.IsHidden(_View) == _bShow)
				{
					elementSet.Insert(item2);
				}
			}
		}
		if (elementSet.Size > 0)
		{
			EPTransactionContainer ePTransactionContainer = null;
			ePTransactionContainer = new EPTransactionContainer(_Document);
			ePTransactionContainer.Start("Hide Element(s) In " + _View.Name);
			ePTransactionContainer.BeginTransaction();
			if (_bShow)
			{
				_View.UnhideElements(ElementSetConverter.ToIDList(elementSet));
			}
			else
			{
				_View.HideElements(ElementSetConverter.ToIDList(elementSet));
			}
			ePTransactionContainer.EndTransaction();
			ePTransactionContainer.Commit();
		}
	}

	public static void AnnotateGradeAndElevationOnLegList(Document _Document, EPTransactionContainer _TransactionContainer, FamilyInstanceComponentWrapper _CurrentFamilyInstanceComponentWrapper, bool _bUseOriginalLeg, ref ElementSet _ElevationText, ref ElementSet _GradeText)
	{
		AnnotateGradeAndElevationOnLegList(_Document, _TransactionContainer, _CurrentFamilyInstanceComponentWrapper, _bAnnotateElevations: true, _bAnnotateGrades: true, _bUseOriginalLeg, null, ref _ElevationText, ref _GradeText, _bCheckProjectSharedCoordinatesSetting: false);
	}

	public static void AnnotateGradeAndElevationOnLegList(Document _Document, EPTransactionContainer _TransactionContainer, FamilyInstanceComponentWrapper _CurrentFamilyInstanceComponentWrapper, bool _bAnnotateElevations, bool _bAnnotateGrades, bool _bUseOriginalLeg, TextNoteType _TextNoteType, ref ElementSet _ElevationText, ref ElementSet _GradeText)
	{
		AnnotateGradeAndElevationOnLegList(_Document, _TransactionContainer, _CurrentFamilyInstanceComponentWrapper, _bAnnotateElevations: true, _bAnnotateGrades: true, _bUseOriginalLeg, _TextNoteType, ref _ElevationText, ref _GradeText, _bCheckProjectSharedCoordinatesSetting: false);
	}

	public static void AnnotateGradeAndElevationOnLegList(Document _Document, EPTransactionContainer _TransactionContainer, FamilyInstanceComponentWrapper _CurrentFamilyInstanceComponentWrapper, bool _bAnnotateElevations, bool _bAnnotateGrades, bool _bUseOriginalLeg, TextNoteType _TextNoteType, ref ElementSet _ElevationText, ref ElementSet _GradeText, bool _bCheckProjectSharedCoordinatesSetting)
	{
		double width = 0.125;
		Eps_Leg eps_Leg = null;
		eps_Leg = ((!_bUseOriginalLeg) ? _CurrentFamilyInstanceComponentWrapper.ProposedLeg : _CurrentFamilyInstanceComponentWrapper.OriginalLeg);
		if (_GradeText == null)
		{
			_GradeText = new ElementSet();
		}
		if (_ElevationText == null)
		{
			_ElevationText = new ElementSet();
		}
		int _nReturnValue = 0;
		bool flag = false;
		if ((_bCheckProjectSharedCoordinatesSetting && ApplicationUtility.Settings.eplSetE_GetSetting(_Document.PathName, "AnnotateUsingSharedCoordinates", ref _nReturnValue)) || ApplicationUtility.Settings.eplSetE_GetSetting(_Document.PathName, "UseSharedElevation", ref _nReturnValue))
		{
			flag = Convert.ToBoolean(_nReturnValue);
		}
		double num = 0.0;
		if (flag)
		{
			ProjectLocation activeProjectLocation = _Document.ActiveProjectLocation;
			XYZ point = new XYZ();
			ProjectPosition projectPosition = activeProjectLocation.GetProjectPosition(point);
			num = projectPosition.Elevation;
		}
		XYZ xYZ = new XYZ(1.0, 0.0, 0.0);
		XYZ xYZ2 = null;
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		string _strToDisplay = string.Empty;
		TextNote textNote = null;
		List<TextNote> list = new List<TextNote>();
		List<TextNote> list2 = new List<TextNote>();
		Eps_Leg eps_Leg2 = null;
		Eps_Point _StaOffPnt = new Eps_Point();
		FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
		UnitSymbolType unitSymbolType;
		try
		{
			unitSymbolType = formatOptions.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			unitSymbolType = UnitSymbolType.UST_NONE;
		}
		FormatOptions formatOptions2 = _Document.GetUnits().GetFormatOptions(UnitType.UT_Slope);
		UnitSymbolType unitSymbolType2;
		try
		{
			unitSymbolType2 = formatOptions2.LegacyUnitSymbol();
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			unitSymbolType2 = UnitSymbolType.UST_NONE;
		}
		DecimalSymbol decimalSymbol = _Document.GetUnits().DecimalSymbol;
		ElementSet familyInstanceElementSet = _CurrentFamilyInstanceComponentWrapper.GetFamilyInstanceElementSet();
		if (familyInstanceElementSet == null || familyInstanceElementSet.Size == 0 || eps_Leg == null)
		{
			return;
		}
		Parameter parameter = null;
		FamilyInstance firstExistingFamilyInstance = _CurrentFamilyInstanceComponentWrapper.GetFirstExistingFamilyInstance();
		if (firstExistingFamilyInstance != null)
		{
			LandcaddID locateType = LandcaddID.SoftTerrain;
			parameter = firstExistingFamilyInstance.LookupParameter(RevitParameters.LocateTypeGenericFamilyParameter);
			if (parameter != null)
			{
				foreach (int value4 in Enum.GetValues(typeof(LandcaddID)))
				{
					if (string.Compare(parameter.AsString(), value4.ToString("000"), ignoreCase: true) == 0)
					{
						locateType = (LandcaddID)Enum.ToObject(typeof(LandcaddID), value4);
						break;
					}
				}
			}
			if (_CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol == null)
			{
				_CurrentFamilyInstanceComponentWrapper.ProposedFamilySymbol = FindFamilyTypeSymbol(_Document, firstExistingFamilyInstance, locateType);
			}
		}
		List<bool> list3 = new List<bool>();
		List<bool> list4 = new List<bool>();
		bool flag2 = false;
		List<XYZ> list5 = new List<XYZ>();
		List<XYZ> list6 = new List<XYZ>();
		XYZ xYZ3 = null;
		ElementId defaultElementTypeId = _Document.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
		TextNoteOptions textNoteOptions = new TextNoteOptions();
		textNoteOptions.HorizontalAlignment = HorizontalTextAlignment.Center;
		textNoteOptions.TypeId = defaultElementTypeId;
		for (eps_Leg2 = eps_Leg; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
		{
			if (eps_Leg2.Line != null)
			{
				if (_bAnnotateElevations)
				{
					RevitGeometry.Units_ConvertDoubleToString(eps_Leg2.Line.StartPnt.z + num, formatOptions.LegacyDisplayUnits(), formatOptions.Accuracy, unitSymbolType, _Document.GetUnits(), ref _strToDisplay, forEditing: false);
					textNote = TextNote.Create(_Document, _Document.ActiveView.Id, eps_Leg2.Line.StartPnt.XY, width, _strToDisplay, textNoteOptions);
					list.Add(textNote);
					list3.Add(item: true);
					list5.Add(eps_Leg2.Line.StartPnt.XY);
				}
				else
				{
					list3.Add(item: false);
					list5.Add(null);
				}
				if (_bAnnotateGrades)
				{
					EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Leg2.Line.StartPnt, eps_Leg2.Line.EndPnt, ref _dDistance);
					num2 = eps_Leg2.Line.EndPnt.z - eps_Leg2.Line.StartPnt.z;
					num3 = num2 / _dDistance;
					RevitGeometry.Units_ConvertDoubleToString(num3, formatOptions2.LegacyDisplayUnits(), formatOptions2.Accuracy, unitSymbolType2, _Document.GetUnits(), ref _strToDisplay, forEditing: false);
					EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(eps_Leg2.Line.StartPnt, eps_Leg2.Line.EndPnt, ref _StaOffPnt);
					xYZ2 = new XYZ(eps_Leg2.Line.StartPnt.x - eps_Leg2.Line.EndPnt.x, eps_Leg2.Line.StartPnt.y - eps_Leg2.Line.EndPnt.y, GetPlane(_Document.ActiveView.SketchPlane).Origin.Z);
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg2.Line.StartPnt, eps_Leg2.Line.EndPnt, ref _dDirection);
					_strToDisplay = ((!(_dDirection > Math.PI / 2.0) || !(_dDirection <= 4.71238898038469)) ? (_strToDisplay + " >") : ("< " + _strToDisplay));
					textNote = TextNote.Create(_Document, _Document.ActiveView.Id, _StaOffPnt.XY, width, _strToDisplay, textNoteOptions);
					list2.Add(textNote);
					list4.Add(item: true);
					list6.Add(_StaOffPnt.XY);
				}
				else
				{
					list4.Add(item: false);
					list6.Add(null);
				}
				if (eps_Leg2.Next == null)
				{
					if (_bAnnotateElevations)
					{
						RevitGeometry.Units_ConvertDoubleToString(eps_Leg2.Line.EndPnt.z + num, formatOptions.LegacyDisplayUnits(), formatOptions.Accuracy, unitSymbolType, _Document.GetUnits(), ref _strToDisplay, forEditing: false);
						textNote = TextNote.Create(_Document, _Document.ActiveView.Id, eps_Leg2.Line.EndPnt.XY, width, _strToDisplay, textNoteOptions);
						list.Add(textNote);
						flag2 = true;
						xYZ3 = eps_Leg2.Line.EndPnt.XY;
					}
					else
					{
						flag2 = false;
						xYZ3 = null;
					}
				}
			}
			else if (eps_Leg2.Arc != null)
			{
				if (_bAnnotateElevations)
				{
					RevitGeometry.Units_ConvertDoubleToString(eps_Leg2.Arc.StartPnt.z + num, formatOptions.LegacyDisplayUnits(), formatOptions.Accuracy, unitSymbolType, _Document.GetUnits(), ref _strToDisplay, forEditing: false);
					textNote = TextNote.Create(_Document, _Document.ActiveView.Id, eps_Leg2.Arc.StartPnt.XY, width, _strToDisplay, textNoteOptions);
					list.Add(textNote);
					list3.Add(item: true);
					list5.Add(eps_Leg2.Arc.StartPnt.XY);
				}
				else
				{
					list3.Add(item: false);
					list5.Add(null);
				}
				if (_bAnnotateGrades)
				{
					_dDistance = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg2.Arc.Radius, eps_Leg2.Arc.Delta);
					num2 = eps_Leg2.Arc.StartPnt.z - eps_Leg2.Arc.EndPnt.z;
					num3 = num2 / _dDistance;
					RevitGeometry.Units_ConvertDoubleToString(num3, formatOptions2.LegacyDisplayUnits(), formatOptions2.Accuracy, unitSymbolType2, _Document.GetUnits(), ref _strToDisplay, forEditing: false);
					EPShared.eplGeoE_FindStaOffPointAlongArc(eps_Leg2.Arc, _dDistance / 2.0, 0.0, ref _StaOffPnt);
					xYZ2 = new XYZ(eps_Leg2.Arc.StartPnt.x - eps_Leg2.Arc.EndPnt.x, eps_Leg2.Arc.StartPnt.y - eps_Leg2.Arc.EndPnt.y, GetPlane(_Document.ActiveView.SketchPlane).Origin.Z);
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Leg2.Arc.StartPnt, eps_Leg2.Arc.EndPnt, ref _dDirection);
					_strToDisplay = ((!(_dDirection > Math.PI / 2.0) || !(_dDirection <= 4.71238898038469)) ? (_strToDisplay + " >") : ("< " + _strToDisplay));
					textNote = TextNote.Create(_Document, _Document.ActiveView.Id, _StaOffPnt.XY, width, _strToDisplay, textNoteOptions);
					list2.Add(textNote);
					list4.Add(item: true);
					list6.Add(_StaOffPnt.XY);
				}
				else
				{
					list4.Add(item: false);
					list6.Add(null);
				}
				if (eps_Leg2.Next == null)
				{
					if (_bAnnotateElevations)
					{
						RevitGeometry.Units_ConvertDoubleToString(eps_Leg2.Arc.EndPnt.z + num, formatOptions.LegacyDisplayUnits(), formatOptions.Accuracy, unitSymbolType, _Document.GetUnits(), ref _strToDisplay, forEditing: false);
						textNote = TextNote.Create(_Document, _Document.ActiveView.Id, eps_Leg2.Arc.EndPnt.XY, width, _strToDisplay, textNoteOptions);
						list.Add(textNote);
						flag2 = true;
						xYZ3 = eps_Leg2.Arc.EndPnt.XY;
					}
					else
					{
						flag2 = false;
						xYZ3 = null;
					}
				}
			}
		}
		foreach (Element item in familyInstanceElementSet)
		{
			parameter = item.LookupParameter(RevitParameters.SiteComponentHasElevationAnnotationParameter);
			if (parameter != null)
			{
				int value2 = (_bAnnotateElevations ? 1 : 0);
				parameter.Set(value2);
			}
			parameter = item.LookupParameter(RevitParameters.SiteComponentHasGradeAnnotationParameter);
			if (parameter != null)
			{
				int value3 = (_bAnnotateGrades ? 1 : 0);
				parameter.Set(value3);
			}
		}
		if (list.Count > 0)
		{
			_TransactionContainer.BeginTransaction();
			foreach (TextNote item2 in list)
			{
				_ElevationText.Insert(item2);
			}
			_TransactionContainer.EndTransaction();
		}
		if (list2.Count > 0)
		{
			_TransactionContainer.BeginTransaction();
			foreach (TextNote item3 in list2)
			{
				_GradeText.Insert(item3);
			}
			_TransactionContainer.EndTransaction();
		}
		int num4 = 0;
		Eps_Point eps_Point = null;
		foreach (Element item4 in _ElevationText)
		{
			if (!(item4 is TextNote textNote2))
			{
				continue;
			}
			if (_TextNoteType != null)
			{
				textNote2.TextNoteType = _TextNoteType;
			}
			eps_Point = new Eps_Point(textNote2.Coord);
			num4 = 0;
			foreach (Element item5 in familyInstanceElementSet)
			{
				if (list3.Count > 0 && list5.Count > 0 && num4 < list3.Count && num4 < list5.Count && list3[num4])
				{
					Eps_Point eps_Point2 = new Eps_Point(list5[num4]);
					if (eps_Point2.IsEqual(eps_Point))
					{
						item5.LookupParameter(RevitParameters.SiteComponentAnnotationBeginElevationIDParameter)?.Set(item4.UniqueId);
					}
				}
				if (!eps_Leg.IsLegClosed() && num4 == familyInstanceElementSet.Size - 1 && flag2 && xYZ3 != null)
				{
					Eps_Point eps_Point3 = new Eps_Point(xYZ3);
					if (eps_Point3.IsEqual(eps_Point))
					{
						item5.LookupParameter(RevitParameters.SiteComponentAnnotationEndElevationIDParameter)?.Set(item4.UniqueId);
					}
				}
				num4++;
			}
		}
		foreach (Element item6 in _GradeText)
		{
			if (!(item6 is TextNote textNote3))
			{
				continue;
			}
			if (_TextNoteType != null)
			{
				textNote3.TextNoteType = _TextNoteType;
			}
			eps_Point = new Eps_Point(textNote3.Coord);
			num4 = 0;
			foreach (Element item7 in familyInstanceElementSet)
			{
				if (list4.Count > 0 && list6.Count > 0 && num4 < list4.Count && num4 < list6.Count && list4[num4])
				{
					Eps_Point eps_Point4 = new Eps_Point(list6[num4]);
					if (eps_Point4.IsEqual(eps_Point))
					{
						item7.LookupParameter(RevitParameters.SiteComponentAnnotationGradeIDParameter)?.Set(item6.UniqueId);
					}
				}
				num4++;
			}
		}
	}

	public static FamilySymbol FindFamilyTypeSymbol(Document _Document, FamilyInstance _PlacementInstance, LandcaddID _LocateType)
	{
		bool _bFamilySymbolPresent = false;
		return FindFamilyTypeSymbol(_Document, _PlacementInstance, _LocateType, ref _bFamilySymbolPresent);
	}

	public static FamilySymbol FindFamilyTypeSymbol(Document _Document, FamilyInstance _PlacementInstance, LandcaddID _LocateType, ref bool _bFamilySymbolPresent)
	{
		_bFamilySymbolPresent = false;
		string strA = string.Empty;
		string strA2 = string.Empty;
		string strA3 = string.Empty;
		Parameter parameter = _PlacementInstance.LookupParameter(RevitParameters.SymbolIDGenericFamilyParameter);
		if (parameter != null)
		{
			strA = parameter.AsString();
		}
		Parameter parameter2 = _PlacementInstance.LookupParameter(RevitParameters.FamilySymbolNameFamilyParameter);
		if (parameter2 != null)
		{
			strA2 = parameter2.AsString();
		}
		parameter2 = _PlacementInstance.LookupParameter(RevitParameters.FamilyNameFamilyParameter);
		if (parameter2 != null)
		{
			strA3 = parameter2.AsString();
		}
		Parameter CurLanddCadID = null;
		FamilySymbol familySymbol = null;
		FamilySymbol familySymbol2 = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (CurLanddCadID = CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(CurLanddCadID.AsString(), Convert.ToInt32(_LocateType).ToString("000"), ignoreCase: true) == 0);
		List<Element> list = source.ToList();
		familySymbol = null;
		foreach (Element item in list)
		{
			if (!(item is FamilySymbol { Category: not null, Family: not null } familySymbol3))
			{
				continue;
			}
			CurLanddCadID = familySymbol3.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (CurLanddCadID != null && string.Compare(CurLanddCadID.AsString(), Convert.ToInt32(_LocateType).ToString("000"), ignoreCase: true) == 0)
			{
				_bFamilySymbolPresent = true;
				if (string.Compare(strA, familySymbol3.UniqueId, ignoreCase: true) == 0)
				{
					return familySymbol3;
				}
				if (string.Compare(strA2, familySymbol3.Name, ignoreCase: true) == 0 && string.Compare(strA3, familySymbol3.Family.Name, ignoreCase: true) == 0)
				{
					familySymbol2 = familySymbol3;
				}
			}
		}
		if (familySymbol2 != null)
		{
			parameter.Set(familySymbol2.UniqueId);
		}
		return familySymbol2;
	}

	public static bool FindMassTemplateFileName(Document _Document, LandcaddID _CurID, ref string _strTemplateFileName, ref string _strAdditionalTemplateFileName)
	{
		return FindMassTemplateFileName(_Document, _CurID, ref _strTemplateFileName, ref _strAdditionalTemplateFileName, _bIssueWarning: true);
	}

	public static bool FindMassTemplateFileName(Document _Document, LandcaddID _CurID, ref string _strTemplateFileName, bool _bIssueWarning)
	{
		string _strAdditionalTemplateFileName = string.Empty;
		return FindMassTemplateFileName(_Document, _CurID, ref _strTemplateFileName, ref _strAdditionalTemplateFileName, _bIssueWarning);
	}

	public static bool FindMassTemplateFileName(Document _Document, LandcaddID _CurID, ref string _strTemplateFileName, ref string _strAdditionalTemplateFileName, bool _bIssueWarning)
	{
		bool flag = false;
		string text = string.Empty;
		string strSettingsName = string.Empty;
		switch (_CurID)
		{
		case LandcaddID.Street:
			text = "StreetMassTemplateFileName";
			strSettingsName = "CurbMassTemplateFileName";
			break;
		case LandcaddID.Sidewalk:
			text = "SidewalkMassTemplateFileName";
			break;
		case LandcaddID.RetainingWall:
			text = "RetainingWallMassTemplateFileName";
			break;
		case LandcaddID.Curb:
			text = "CurbMassTemplateFileName";
			break;
		case LandcaddID.Pipe:
			text = "PipeMassTemplateFileName";
			strSettingsName = "StructureMassTemplateFileName";
			break;
		case LandcaddID.Structure:
			text = "StructureMassTemplateFileName";
			break;
		}
		if (!ApplicationUtility.Settings.eplSetE_GetSetting(text, ref _strTemplateFileName) || !File.Exists(_strTemplateFileName))
		{
			string empty = string.Empty;
			empty = "MassTemplateFileName";
			if (text == string.Empty)
			{
				text = empty;
			}
			if (!ApplicationUtility.Settings.eplSetE_GetSetting(empty, ref _strTemplateFileName) || !File.Exists(_strTemplateFileName))
			{
				if (_Document != null)
				{
					string text2 = string.Empty;
					string searchPattern = string.Empty;
					string familyTemplatePath = _Document.Application.FamilyTemplatePath;
					if (!string.IsNullOrEmpty(familyTemplatePath))
					{
						for (int i = 0; i < 2; i++)
						{
							DirectoryInfo directoryInfo = new DirectoryInfo(familyTemplatePath);
							DirectoryInfo directoryInfo2 = null;
							if (directoryInfo.Parent != null)
							{
								directoryInfo2 = directoryInfo.Parent.Parent;
							}
							if (directoryInfo2 != null)
							{
								if (_Document.DisplayUnitSystem == DisplayUnit.IMPERIAL)
								{
									if (text2 == string.Empty)
									{
										text2 = ApplicationUtility.GetString("MassImperialTemplate") + ".rft";
									}
									switch (i)
									{
									case 0:
										searchPattern = ApplicationUtility.GetString("ENU-ImperialTemplateFolderName");
										break;
									case 1:
										searchPattern = "Imperial";
										break;
									}
								}
								else if (_Document.DisplayUnitSystem == DisplayUnit.METRIC)
								{
									if (text2 == string.Empty)
									{
										text2 = ApplicationUtility.GetString("MassMetricTemplate") + ".rft";
									}
									switch (i)
									{
									case 0:
										searchPattern = ApplicationUtility.GetString("ENU-MetricTemplateFolderName");
										break;
									case 1:
										searchPattern = "Metric";
										break;
									}
								}
								if (directoryInfo2.Exists)
								{
									try
									{
										DirectoryInfo[] directories = directoryInfo2.GetDirectories(searchPattern, SearchOption.AllDirectories);
										DirectoryInfo[] array = directories;
										foreach (DirectoryInfo directoryInfo3 in array)
										{
											FileInfo[] files = directoryInfo3.GetFiles(text2, SearchOption.AllDirectories);
											FileInfo[] array2 = files;
											foreach (FileInfo fileInfo in array2)
											{
												if (fileInfo.Exists)
												{
													_strTemplateFileName = fileInfo.FullName;
													flag = true;
													break;
												}
											}
											if (flag)
											{
												break;
											}
										}
									}
									catch (UnauthorizedAccessException)
									{
										continue;
									}
									catch (System.IO.DirectoryNotFoundException)
									{
										continue;
									}
								}
							}
							if (flag)
							{
								break;
							}
						}
					}
				}
				if (!flag)
				{
					string empty2 = string.Empty;
					string empty3 = string.Empty;
					int num = -1;
					if (!File.Exists(ApplicationUtility.RevitIniFilename))
					{
						if (_bIssueWarning)
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("RevitINIFileMissingMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
						}
						return false;
					}
					Stream stream = new FileStream(ApplicationUtility.RevitIniFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					StreamReader streamReader = new StreamReader(stream, Encoding.ASCII);
					while ((empty2 = streamReader.ReadLine()) != null)
					{
						if (empty2.Contains(ApplicationUtility.GetString("FamilyTemplatePath")))
						{
							string text3 = string.Empty;
							num = empty2.IndexOf('=');
							if (num >= 0 && num < empty2.Length - 1)
							{
								text3 = empty2.Substring(num + 1);
								if (!text3.EndsWith("\\"))
								{
									text3 += "\\";
								}
								text3 = text3 + ApplicationUtility.GetString("ConceptualMassFolder") + "\\";
							}
							if (empty2.Contains(ApplicationUtility.GetString("ENU-ImperialTemplateFolderName")))
							{
								_strTemplateFileName = text3 + ApplicationUtility.GetString("MassImperialTemplate");
							}
							else
							{
								_strTemplateFileName = text3 + ApplicationUtility.GetString("MassMetricTemplate");
							}
							_strTemplateFileName += ".rft";
							if (File.Exists(_strTemplateFileName))
							{
								flag = true;
								break;
							}
							_strTemplateFileName = string.Empty;
						}
						else
						{
							if (string.Compare(empty2, ApplicationUtility.GetString("ContentInstalledName"), ignoreCase: true) != 0)
							{
								continue;
							}
							while ((empty3 = streamReader.ReadLine()) != null)
							{
								if (!empty3.Contains(ApplicationUtility.GetString("ImperialTemplateCabName")) && !empty3.Contains(ApplicationUtility.GetString("MetricTemplateCabName")))
								{
									continue;
								}
								string text4 = string.Empty;
								num = empty3.IndexOf('=');
								if (num >= 0 && num < empty3.Length - 1)
								{
									text4 = empty3.Substring(num + 1);
									if (!text4.EndsWith("\\"))
									{
										text4 += "\\";
									}
									text4 = text4 + ApplicationUtility.GetString("ConceptualMassFolder") + "\\";
								}
								if (empty3.Contains(ApplicationUtility.GetString("ENU-ImperialTemplateFolderName")))
								{
									_strTemplateFileName = text4 + ApplicationUtility.GetString("MassImperialTemplate");
								}
								else
								{
									_strTemplateFileName = text4 + ApplicationUtility.GetString("MassMetricTemplate");
								}
								_strTemplateFileName += ".rft";
								if (File.Exists(_strTemplateFileName))
								{
									flag = true;
									break;
								}
								_strTemplateFileName = string.Empty;
							}
							break;
						}
					}
					streamReader.Close();
				}
				if (!flag)
				{
					if (!_bIssueWarning)
					{
						return false;
					}
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NoMassRFTMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
					using OpenFileDialog openFileDialog = new OpenFileDialog();
					openFileDialog.Title = EPShared.LocalRM.GetString("TemplateNotLoadedErrorMessage");
					openFileDialog.RestoreDirectory = true;
					openFileDialog.Filter = ApplicationUtility.GetString("TemplateFilePickerFilter");
					DialogResult dialogResult = openFileDialog.ShowDialog();
					if (dialogResult == DialogResult.Cancel)
					{
						return false;
					}
					_strTemplateFileName = openFileDialog.FileName;
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (_CurID == LandcaddID.Street || _CurID == LandcaddID.Pipe)
		{
			if (!ApplicationUtility.Settings.eplSetE_GetSetting(strSettingsName, ref _strAdditionalTemplateFileName) || !File.Exists(_strAdditionalTemplateFileName))
			{
				_strAdditionalTemplateFileName = _strTemplateFileName;
			}
			ApplicationUtility.Settings.eplSetE_SetSetting(strSettingsName, _strAdditionalTemplateFileName);
		}
		ApplicationUtility.Settings.eplSetE_SetSetting(text, _strTemplateFileName);
		return flag;
	}

	public static bool FindLanguageName(ref string _strLanguageName)
	{
		bool result = false;
		string strSettingsName = "LanguageName";
		if (!ApplicationUtility.Settings.eplSetE_GetSetting(strSettingsName, ref _strLanguageName))
		{
			string empty = string.Empty;
			if (!File.Exists(ApplicationUtility.RevitIniFilename))
			{
				if (ApplicationUtility.StringTable != null)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("RevitINIFileMissingMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
				}
				return false;
			}
			Stream stream = new FileStream(ApplicationUtility.RevitIniFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			StreamReader streamReader = new StreamReader(stream, Encoding.ASCII);
			while ((empty = streamReader.ReadLine()) != null)
			{
				if (string.Compare(empty, "[Language]", ignoreCase: true) == 0)
				{
					string text = streamReader.ReadLine();
					int num = text.IndexOf('=');
					if (num >= 0 && num < text.Length - 1)
					{
						_strLanguageName = text.Substring(num + 1);
						result = true;
					}
					break;
				}
			}
			streamReader.Close();
			ApplicationUtility.Settings.eplSetE_SetSetting(strSettingsName, _strLanguageName);
		}
		else
		{
			result = true;
		}
		return result;
	}

	public static ModelCurve NewModelCurve(Document _Document, Eps_Arc _Arc)
	{
		return NewModelCurve(_Document, _Arc, _Document.ActiveView.SketchPlane);
	}

	public static ModelCurve NewModelCurve(Document _Document, Eps_Arc _Arc, Autodesk.Revit.DB.View _CurView)
	{
		return NewModelCurve(_Document, _Arc, _CurView.SketchPlane);
	}

	public static ModelCurve NewModelCurve(Document _Document, Eps_Arc _Arc, SketchPlane _SketchPlane)
	{
		Eps_Arc eps_Arc = new Eps_Arc(_Arc);
		eps_Arc.RadiusPnt.z = GetPlane(_SketchPlane).Origin.Z;
		eps_Arc.EndPnt.z = GetPlane(_SketchPlane).Origin.Z;
		eps_Arc.StartPnt.z = GetPlane(_SketchPlane).Origin.Z;
		Arc geometryCurve = eps_Arc.RevitArc(_Document);
		try
		{
			return _Document.Create.NewModelCurve(geometryCurve, _SketchPlane);
		}
		catch (Autodesk.Revit.Exceptions.ArgumentException)
		{
			Plane plane = Plane.CreateByNormalAndOrigin(new XYZ(0.0, 0.0, 1.0), GetPlane(_SketchPlane).Origin);
			SketchPlane sketchPlane = SketchPlane.Create(_Document, plane);
			return _Document.Create.NewModelCurve(geometryCurve, sketchPlane);
		}
	}

	public static ModelCurve NewModelCurve(Document _Document, Eps_Line _Line)
	{
		return NewModelCurve(_Document, _Line, _Document.ActiveView.SketchPlane);
	}

	public static ModelCurve NewModelCurve(Document _Document, Eps_Line _Line, Autodesk.Revit.DB.View _CurView)
	{
		return NewModelCurve(_Document, _Line, _CurView.SketchPlane);
	}

	public static ModelCurve NewModelCurve(Document _Document, Eps_Line _Line, SketchPlane _SketchPlane)
	{
		Eps_Line eps_Line = new Eps_Line(_Line);
		eps_Line.EndPnt.z = GetPlane(_SketchPlane).Origin.Z;
		eps_Line.StartPnt.z = GetPlane(_SketchPlane).Origin.Z;
		if (!EPShared.eplGeoE_ArePntsDuplicates2D(eps_Line.StartPnt, eps_Line.EndPnt, 0.0025))
		{
			Line geometryCurve = eps_Line.RevitLine(_Document);
			try
			{
				return _Document.Create.NewModelCurve(geometryCurve, _SketchPlane);
			}
			catch (Autodesk.Revit.Exceptions.ArgumentException)
			{
				Plane plane = Plane.CreateByNormalAndOrigin(new XYZ(0.0, 0.0, 1.0), GetPlane(_SketchPlane).Origin);
				SketchPlane sketchPlane = SketchPlane.Create(_Document, plane);
				return _Document.Create.NewModelCurve(geometryCurve, sketchPlane);
			}
		}
		return null;
	}

	public static ModelCurveArray NewModelCurveArray(Document _Document, Eps_Leg _Leg)
	{
		return NewModelCurveArray(_Document, _Leg, _Document.ActiveView.SketchPlane, _bUse2011TempLines: true);
	}

	public static ModelCurveArray NewModelCurveArray(Document _Document, Eps_Leg _Leg, Autodesk.Revit.DB.View _CurView)
	{
		return NewModelCurveArray(_Document, _Leg, _CurView.SketchPlane, _bUse2011TempLines: true);
	}

	public static ModelCurveArray NewModelCurveArray(Document _Document, Eps_Leg _Leg, Autodesk.Revit.DB.View _CurView, bool _bUse2011TempLines)
	{
		return NewModelCurveArray(_Document, _Leg, _CurView.SketchPlane, _bUse2011TempLines);
	}

	public static Plane GetPlane(SketchPlane p)
	{
		return p.GetPlane();
	}

	public static Line CreateLine(XYZ pt1, XYZ pt2, bool bBound)
	{
		if (bBound)
		{
			return Line.CreateBound(pt1, pt2);
		}
		return Line.CreateUnbound(pt1, pt2);
	}

	public static ModelCurveArray NewModelCurveArray(Document _Document, Eps_Leg _Leg, SketchPlane _SketchPlane, bool _bUse2011TempLines)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		for (eps_Leg = _Leg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			eps_Leg3 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line(eps_Leg.Line);
				eps_Leg3.Line.EndPnt.z = GetPlane(_SketchPlane).Origin.Z;
				eps_Leg3.Line.StartPnt.z = GetPlane(_SketchPlane).Origin.Z;
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				eps_Leg3.Arc.RadiusPnt.z = GetPlane(_SketchPlane).Origin.Z;
				eps_Leg3.Arc.EndPnt.z = GetPlane(_SketchPlane).Origin.Z;
				eps_Leg3.Arc.StartPnt.z = GetPlane(_SketchPlane).Origin.Z;
			}
			if (eps_Leg3.Line != null || eps_Leg3.Arc != null)
			{
				if (eps_Leg2 == null)
				{
					eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg3);
				}
				else
				{
					eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg3);
				}
			}
		}
		CurveArray curveArray = eps_Leg2.CurveArray(_Document);
		ModelCurveArray result = null;
		if (curveArray != null && curveArray.Size > 0)
		{
			try
			{
				result = CreateModelCurveArray(_Document, curveArray, _SketchPlane, _bUse2011TempLines);
			}
			catch (Autodesk.Revit.Exceptions.ArgumentException)
			{
				Plane plane = Plane.CreateByNormalAndOrigin(new XYZ(0.0, 0.0, 1.0), GetPlane(_SketchPlane).Origin);
				SketchPlane sketchPlane = SketchPlane.Create(_Document, plane);
				result = CreateModelCurveArray(_Document, curveArray, sketchPlane, _bUse2011TempLines);
			}
		}
		return result;
	}

	private static ModelCurveArray CreateModelCurveArray(Document _Document, CurveArray _CurveArray, SketchPlane _SketchPlane, bool _bUse2011TempLines)
	{
		ModelCurveArray modelCurveArray = null;
		if (_bUse2011TempLines)
		{
			modelCurveArray = new ModelCurveArray();
			for (int i = 0; i < _CurveArray.Size; i++)
			{
				modelCurveArray.Append(_Document.Create.NewModelCurve(_CurveArray.get_Item(i), _SketchPlane));
			}
		}
		else
		{
			modelCurveArray = _Document.Create.NewModelCurveArray(_CurveArray, _SketchPlane);
		}
		return modelCurveArray;
	}

	public static DetailCurveArray NewDetailCurveArray(Document _Document, Eps_Leg _Leg)
	{
		return NewDetailCurveArray(_Document, _Leg, _Document.ActiveView);
	}

	public static DetailCurveArray NewDetailCurveArray(Document _Document, Eps_Leg _Leg, Autodesk.Revit.DB.View _CurView)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg eps_Leg3 = null;
		for (eps_Leg = _Leg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			eps_Leg3 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg3.Line = new Eps_Line(eps_Leg.Line);
				eps_Leg3.Line.EndPnt.z = GetPlane(_CurView.SketchPlane).Origin.Z;
				eps_Leg3.Line.StartPnt.z = GetPlane(_CurView.SketchPlane).Origin.Z;
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg3.Arc = new Eps_Arc(eps_Leg.Arc);
				eps_Leg3.Arc.RadiusPnt.z = GetPlane(_CurView.SketchPlane).Origin.Z;
				eps_Leg3.Arc.EndPnt.z = GetPlane(_CurView.SketchPlane).Origin.Z;
				eps_Leg3.Arc.StartPnt.z = GetPlane(_CurView.SketchPlane).Origin.Z;
			}
			if (eps_Leg2 == null)
			{
				eps_Leg2 = Eps_Leg.SingleLeg(eps_Leg3);
			}
			else
			{
				eps_Leg2.Last.Next = Eps_Leg.SingleLeg(eps_Leg3);
			}
		}
		CurveArray curveArray = eps_Leg2.CurveArray(_Document);
		DetailCurveArray result = null;
		if (curveArray != null && curveArray.Size > 0)
		{
			result = _Document.Create.NewDetailCurveArray(_CurView, curveArray);
		}
		return result;
	}

	public static void HideElementsInAllViews(Document _Document, ElementSet _ElementSet)
	{
		if (_ElementSet == null || _ElementSet.Size == 0)
		{
			return;
		}
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list = filteredElementCollector.OfClass(typeof(Autodesk.Revit.DB.View)).ToElements();
		List<Autodesk.Revit.DB.View> list2 = new List<Autodesk.Revit.DB.View>();
		foreach (Autodesk.Revit.DB.View item in list)
		{
			if (!list2.Contains(item))
			{
				if (item.GenLevel != null || item is ViewSection)
				{
					item.HideElements(ElementSetConverter.ToIDList(_ElementSet));
					list2.Add(item);
				}
				else if (item is View3D && (item.CropBox != null || item.Origin != null || item.SketchPlane != null))
				{
					item.HideElements(ElementSetConverter.ToIDList(_ElementSet));
					list2.Add(item);
				}
			}
		}
	}

	public static void ShowElementsInAllViews(Document _Document, ElementSet _ElementSet)
	{
		if (_ElementSet == null || _ElementSet.Size == 0)
		{
			return;
		}
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list = filteredElementCollector.OfClass(typeof(Autodesk.Revit.DB.View)).ToElements();
		List<Autodesk.Revit.DB.View> list2 = new List<Autodesk.Revit.DB.View>();
		foreach (Autodesk.Revit.DB.View item in list)
		{
			if (!list2.Contains(item))
			{
				if (item.GenLevel != null || item is ViewSection)
				{
					item.UnhideElements(ElementSetConverter.ToIDList(_ElementSet));
					list2.Add(item);
				}
				else if (item is View3D && (item.CropBox != null || item.Origin != null || item.SketchPlane != null))
				{
					item.UnhideElements(ElementSetConverter.ToIDList(_ElementSet));
					list2.Add(item);
				}
			}
		}
	}

	public static List<Autodesk.Revit.DB.View> FindAllViews(Document _Document)
	{
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list = filteredElementCollector.OfClass(typeof(Autodesk.Revit.DB.View)).ToElements();
		List<Autodesk.Revit.DB.View> list2 = new List<Autodesk.Revit.DB.View>();
		foreach (Autodesk.Revit.DB.View item in list)
		{
			if (!list2.Contains(item))
			{
				if (item.GenLevel != null || item is ViewSection)
				{
					list2.Add(item);
				}
				else if (item is View3D && (item.CropBox != null || item.Origin != null || item.SketchPlane != null))
				{
					list2.Add(item);
				}
			}
		}
		return list2;
	}

	public static List<KeyValuePair<Autodesk.Revit.DB.View, string>> FindShowByForAllViews(Document _Document)
	{
		Parameter parameter = null;
		string value = string.Empty;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list = filteredElementCollector.OfClass(typeof(Autodesk.Revit.DB.View)).ToElements();
		List<KeyValuePair<Autodesk.Revit.DB.View, string>> list2 = new List<KeyValuePair<Autodesk.Revit.DB.View, string>>();
		List<Autodesk.Revit.DB.View> list3 = new List<Autodesk.Revit.DB.View>();
		foreach (Autodesk.Revit.DB.View item in list)
		{
			if (list3.Contains(item))
			{
				continue;
			}
			if (item.GenLevel != null || item is ViewSection)
			{
				if (_Document.IsWorkshared)
				{
					parameter = ((Element)item).get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
					if (parameter != null)
					{
						parameter = ((Element)item).get_Parameter(BuiltInParameter.EDITED_BY);
						if (parameter != null)
						{
							value = parameter.AsString();
						}
					}
				}
				list3.Add(item);
				list2.Add(new KeyValuePair<Autodesk.Revit.DB.View, string>(item, value));
				value = string.Empty;
			}
			else
			{
				if (!(item is View3D) || (item.CropBox == null && item.Origin == null && item.SketchPlane == null))
				{
					continue;
				}
				if (_Document.IsWorkshared)
				{
					parameter = ((Element)item).get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
					if (parameter != null)
					{
						parameter = ((Element)item).get_Parameter(BuiltInParameter.EDITED_BY);
						if (parameter != null)
						{
							value = parameter.AsString();
						}
					}
				}
				list3.Add(item);
				list2.Add(new KeyValuePair<Autodesk.Revit.DB.View, string>(item, value));
				value = string.Empty;
			}
		}
		return list2;
	}

	public static bool CreateTrue3DViewIfNotPresent(Document _Document, ref View3D _ReturnView3D)
	{
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list = filteredElementCollector.OfClass(typeof(View3D)).ToElements();
		bool flag = false;
		foreach (View3D item in list)
		{
			if (item.CropBox != null || item.Origin != null || item.SketchPlane != null)
			{
				_ReturnView3D = item;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			ViewFamilyType viewFamilyType = (from ViewFamilyType v in new FilteredElementCollector(_Document).OfClass(typeof(ViewFamilyType))
				where v.ViewFamily == ViewFamily.ThreeDimensional
				select v).First();
			_ReturnView3D = View3D.CreateIsometric(_Document, viewFamilyType.Id);
			_ReturnView3D.Name = EPShared.LocalRM.GetString("ThreeDString");
		}
		return flag;
	}

	public static void HideCategoryInAllViews(Document _Document, BuiltInCategory _BuiltInCategory)
	{
		ChangeVisibilityOfCategoryInAllViews(_Document, _BuiltInCategory, _bShow: false);
	}

	public static void ShowCategoryInAllViews(Document _Document, BuiltInCategory _BuiltInCategory)
	{
		ChangeVisibilityOfCategoryInAllViews(_Document, _BuiltInCategory, _bShow: true);
	}

	private static void ChangeVisibilityOfCategoryInAllViews(Document _Document, BuiltInCategory _BuiltInCategory, bool _bShow)
	{
		if (_BuiltInCategory == BuiltInCategory.INVALID)
		{
			return;
		}
		Category category = _Document.Settings.Categories.get_Item(_BuiltInCategory);
		if (category == null)
		{
			return;
		}
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list = filteredElementCollector.OfClass(typeof(Autodesk.Revit.DB.View)).ToElements();
		List<Autodesk.Revit.DB.View> list2 = new List<Autodesk.Revit.DB.View>();
		foreach (Autodesk.Revit.DB.View item in list)
		{
			if (list2.Contains(item))
			{
				continue;
			}
			if (item.GenLevel != null || item is ViewSection)
			{
				try
				{
					category.set_Visible(item, _bShow);
				}
				catch
				{
					if (_Document.GetElement(_Document.ActiveView.ViewTemplateId) is ViewPlan view2)
					{
						category.set_Visible((Autodesk.Revit.DB.View)view2, true);
					}
				}
				list2.Add(item);
			}
			else
			{
				if (!(item is View3D) || (item.CropBox == null && item.Origin == null && item.SketchPlane == null))
				{
					continue;
				}
				try
				{
					category.set_Visible(item, _bShow);
				}
				catch
				{
					if (_Document.GetElement(_Document.ActiveView.ViewTemplateId) is ViewPlan view3)
					{
						category.set_Visible((Autodesk.Revit.DB.View)view3, true);
					}
				}
				list2.Add(item);
			}
		}
	}

	public static List<Autodesk.Revit.DB.View> FindAllViewsWithElementsVisible(Document _Document, ElementSet _ElementSet)
	{
		if (_ElementSet == null || _ElementSet.Size == 0)
		{
			return null;
		}
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		IList<Element> list = filteredElementCollector.OfClass(typeof(Autodesk.Revit.DB.View)).ToElements();
		List<Autodesk.Revit.DB.View> list2 = new List<Autodesk.Revit.DB.View>();
		foreach (Element item in _ElementSet)
		{
			foreach (Autodesk.Revit.DB.View item2 in list)
			{
				if (!list2.Contains(item2) && !item.IsHidden(item2))
				{
					if (item2.GenLevel != null || item2 is ViewSection)
					{
						list2.Add(item2);
					}
					else if (item2 is View3D && (item2.CropBox != null || item2.Origin != null || item2.SketchPlane != null))
					{
						list2.Add(item2);
					}
				}
			}
		}
		return list2;
	}

	public static List<KeyValuePair<string, Category>> GetAllMassLineSubCategories(Document _Document, ref bool _bDoesAllMassLineSubCategoriesExist)
	{
		return GetAllMassLineSubCategories(_Document, ref _bDoesAllMassLineSubCategoriesExist, string.Empty);
	}

	public static List<KeyValuePair<string, Category>> GetAllMassLineSubCategories(Document _Document, ref bool _bDoesAllMassLineSubCategoriesExist, string _strExcludeCategory)
	{
		_bDoesAllMassLineSubCategoriesExist = true;
		bool _bDoesMassLineSubCategoryExist = false;
		Category category = null;
		List<KeyValuePair<string, Category>> list = new List<KeyValuePair<string, Category>>();
		if (string.Compare(_strExcludeCategory, Convert.ToInt32(LandcaddID.SoftTerrain).ToString("000"), ignoreCase: true) != 0)
		{
			category = GetSiteComponentLineSubCategory(_Document, LandcaddID.SoftTerrain, ref _bDoesMassLineSubCategoryExist);
			if (!_bDoesMassLineSubCategoryExist)
			{
				_bDoesAllMassLineSubCategoriesExist = false;
			}
			if (category == null)
			{
				return null;
			}
			list.Add(new KeyValuePair<string, Category>(Convert.ToInt32(LandcaddID.SoftTerrain).ToString("000"), category));
		}
		if (string.Compare(_strExcludeCategory, Convert.ToInt32(LandcaddID.Sidewalk).ToString("000"), ignoreCase: true) != 0)
		{
			category = GetSiteComponentLineSubCategory(_Document, LandcaddID.Sidewalk, ref _bDoesMassLineSubCategoryExist);
			if (!_bDoesMassLineSubCategoryExist)
			{
				_bDoesAllMassLineSubCategoriesExist = false;
			}
			if (category == null)
			{
				return null;
			}
			list.Add(new KeyValuePair<string, Category>(Convert.ToInt32(LandcaddID.Sidewalk).ToString("000"), category));
		}
		if (string.Compare(_strExcludeCategory, Convert.ToInt32(LandcaddID.RetainingWall).ToString("000"), ignoreCase: true) != 0)
		{
			category = GetSiteComponentLineSubCategory(_Document, LandcaddID.RetainingWall, ref _bDoesMassLineSubCategoryExist);
			if (!_bDoesMassLineSubCategoryExist)
			{
				_bDoesAllMassLineSubCategoriesExist = false;
			}
			if (category == null)
			{
				return null;
			}
			list.Add(new KeyValuePair<string, Category>(Convert.ToInt32(LandcaddID.RetainingWall).ToString("000"), category));
		}
		if (string.Compare(_strExcludeCategory, Convert.ToInt32(LandcaddID.Street).ToString("000"), ignoreCase: true) != 0)
		{
			category = GetSiteComponentLineSubCategory(_Document, LandcaddID.Street, ref _bDoesMassLineSubCategoryExist);
			if (!_bDoesMassLineSubCategoryExist)
			{
				_bDoesAllMassLineSubCategoriesExist = false;
			}
			if (category == null)
			{
				return null;
			}
			list.Add(new KeyValuePair<string, Category>(Convert.ToInt32(LandcaddID.Street).ToString("000"), category));
		}
		if (string.Compare(_strExcludeCategory, Convert.ToInt32(LandcaddID.Curb).ToString("000"), ignoreCase: true) != 0)
		{
			category = GetSiteComponentLineSubCategory(_Document, LandcaddID.Curb, ref _bDoesMassLineSubCategoryExist);
			if (!_bDoesMassLineSubCategoryExist)
			{
				_bDoesAllMassLineSubCategoriesExist = false;
			}
			if (category == null)
			{
				return null;
			}
			list.Add(new KeyValuePair<string, Category>(Convert.ToInt32(LandcaddID.Curb).ToString("000"), category));
		}
		if (string.Compare(_strExcludeCategory, Convert.ToInt32(LandcaddID.ParkingLot).ToString("000"), ignoreCase: true) != 0)
		{
			category = GetSiteComponentLineSubCategory(_Document, LandcaddID.ParkingLot, ref _bDoesMassLineSubCategoryExist);
			if (!_bDoesMassLineSubCategoryExist)
			{
				_bDoesAllMassLineSubCategoriesExist = false;
			}
			if (category == null)
			{
				return null;
			}
			list.Add(new KeyValuePair<string, Category>(Convert.ToInt32(LandcaddID.ParkingLot).ToString("000"), category));
		}
		if (string.Compare(_strExcludeCategory, Convert.ToInt32(LandcaddID.FeatureLine).ToString("000"), ignoreCase: true) != 0)
		{
			category = GetSiteComponentLineSubCategory(_Document, LandcaddID.FeatureLine, ref _bDoesMassLineSubCategoryExist);
			if (!_bDoesMassLineSubCategoryExist)
			{
				_bDoesAllMassLineSubCategoriesExist = false;
			}
			if (category == null)
			{
				return null;
			}
			list.Add(new KeyValuePair<string, Category>(Convert.ToInt32(LandcaddID.FeatureLine).ToString("000"), category));
		}
		if (string.Compare(_strExcludeCategory, Convert.ToInt32(LandcaddID.PointWipeout).ToString("000"), ignoreCase: true) != 0)
		{
			category = GetSiteComponentLineSubCategory(_Document, LandcaddID.PointWipeout, ref _bDoesMassLineSubCategoryExist);
			if (!_bDoesMassLineSubCategoryExist)
			{
				_bDoesAllMassLineSubCategoriesExist = false;
			}
			if (category == null)
			{
				return null;
			}
			list.Add(new KeyValuePair<string, Category>(Convert.ToInt32(LandcaddID.PointWipeout).ToString("000"), category));
		}
		return list;
	}

	public static Category GetSiteComponentLineSubCategory(Document _Document, LandcaddID _CurLandcaddId)
	{
		bool _bDoesMassLineSubCategoryExist = false;
		return GetSiteComponentLineSubCategory(_Document, _CurLandcaddId, ref _bDoesMassLineSubCategoryExist);
	}

	public static Category GetSiteComponentLineSubCategory(Document _Document, LandcaddID _CurLandcaddId, ref bool _bDoesMassLineSubCategoryExist)
	{
		Category category = null;
		string text = string.Empty;
		string _strReturnValue = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		switch (_CurLandcaddId)
		{
		case LandcaddID.SoftTerrain:
			text = "SoftTerrainLineSubCategoryName";
			_strReturnValue = EPShared.LocalRM.GetString("SWSoftTerrainLineNameString");
			text2 = "SoftTerrainLineSubCategoryColor";
			text3 = "SoftTerrainLineSubCategoryLineWeight";
			break;
		case LandcaddID.Sidewalk:
			text = "SidewalkLineSubCategoryName";
			_strReturnValue = EPShared.LocalRM.GetString("SWSidewalkLineNameString");
			text2 = "SidewalkLineSubCategoryColor";
			text3 = "SidewalkLineSubCategoryLineWeight";
			break;
		case LandcaddID.RetainingWall:
			text = "RetainingWallLineSubCategoryName";
			_strReturnValue = EPShared.LocalRM.GetString("SWRetainingWallLineNameString");
			text2 = "RetainingWallLineSubCategoryColor";
			text3 = "RetainingWallLineSubCategoryLineWeight";
			break;
		case LandcaddID.Street:
			text = "StreetLineSubCategoryName";
			_strReturnValue = EPShared.LocalRM.GetString("SWStreetLineNameString");
			text2 = "StreetLineSubCategoryColor";
			text3 = "StreetLineSubCategoryLineWeight";
			break;
		case LandcaddID.Curb:
			text = "CurbLineSubCategoryName";
			_strReturnValue = EPShared.LocalRM.GetString("SWCurbLineNameString");
			text2 = "CurbLineSubCategoryColor";
			text3 = "CurbLineSubCategoryLineWeight";
			break;
		case LandcaddID.ParkingLot:
			text = "ParkingLotLineSubCategoryName";
			_strReturnValue = EPShared.LocalRM.GetString("SWParkingLotLineNameString");
			text2 = "ParkingLotLineSubCategoryColor";
			text3 = "ParkingLotLineSubCategoryLineWeight";
			break;
		case LandcaddID.FeatureLine:
			text = "FeatureLineLineSubCategoryName";
			_strReturnValue = EPShared.LocalRM.GetString("SWFeatureLineLineNameString");
			text2 = "FeatureLineLineSubCategoryColor";
			text3 = "FeatureLineLineSubCategoryLineWeight";
			break;
		case LandcaddID.PointWipeout:
			text = "PointWipeoutLineSubCategoryName";
			_strReturnValue = EPShared.LocalRM.GetString("SWPointWipeoutLineNameString");
			text2 = "PointWipeoutLineSubCategoryColor";
			text3 = "PointWipeoutLineSubCategoryLineWeight";
			break;
		case LandcaddID.IntersectionPlacement:
			text = "IntersectionLineSubCategoryName";
			_strReturnValue = EPShared.LocalRM.GetString("SWIntersectionLineNameString");
			text2 = "IntersectionLineSubCategoryColor";
			text3 = "IntersectionLineSubCategoryLineWeight";
			break;
		}
		Category category2 = _Document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
		if (category2 != null && category2.SubCategories != null && category2.SubCategories.Size > 0)
		{
			if (!ApplicationUtility.Settings.eplSetE_GetSetting(text, ref _strReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting(text.Replace("Category", "Catergory"), ref _strReturnValue))
			{
				ApplicationUtility.Settings.eplSetE_SetSetting(text, _strReturnValue);
			}
			if (category2.SubCategories.Contains(_strReturnValue))
			{
				category = category2.SubCategories.get_Item(_strReturnValue);
			}
		}
		string _strColorValue = string.Empty;
		if (category == null)
		{
			_strColorValue = EPShared.LocalRM.GetString("RGBString") + " " + EPShared.LocalRM.GetString("DefaultLineColor");
			_bDoesMassLineSubCategoryExist = false;
			category = _Document.Settings.Categories.NewSubcategory(category2, _strReturnValue);
			if (!ApplicationUtility.Settings.eplSetE_GetSetting(text2, ref _strColorValue) && !ApplicationUtility.Settings.eplSetE_GetSetting(text2.Replace("Category", "Catergory"), ref _strColorValue))
			{
				ApplicationUtility.Settings.eplSetE_SetSetting(text2, _strColorValue);
			}
			Autodesk.Revit.DB.Color _FindColor = null;
			if (FindColorFromString(_strColorValue, ref _FindColor) && _FindColor != null)
			{
				category.LineColor = _FindColor;
			}
			int _nReturnValue = 1;
			if (!ApplicationUtility.Settings.eplSetE_GetSetting(text3, ref _nReturnValue) && !ApplicationUtility.Settings.eplSetE_GetSetting(text3.Replace("Category", "Catergory"), ref _nReturnValue))
			{
				ApplicationUtility.Settings.eplSetE_SetSetting(text3, _nReturnValue);
			}
			category.SetLineWeight(_nReturnValue, GraphicsStyleType.Projection);
		}
		else
		{
			if (category.Name != string.Empty)
			{
				ApplicationUtility.Settings.eplSetE_SetSetting(text, category.Name);
			}
			FindStringFromColor(category.LineColor, ref _strColorValue);
			if (_strColorValue != string.Empty)
			{
				ApplicationUtility.Settings.eplSetE_SetSetting(text2, _strColorValue);
			}
			int? lineWeight = category.GetLineWeight(GraphicsStyleType.Projection);
			if (lineWeight.HasValue)
			{
				ApplicationUtility.Settings.eplSetE_SetSetting(text3, lineWeight.Value);
			}
			_bDoesMassLineSubCategoryExist = true;
		}
		return category;
	}

	public static bool FindColorFromString(string _strColorValue, ref Autodesk.Revit.DB.Color _FindColor)
	{
		_FindColor = null;
		System.Drawing.Color _FindColor2 = default;
		bool flag = FindColorFromString(_strColorValue, ref _FindColor2);
		if (flag)
		{
			_FindColor = new Autodesk.Revit.DB.Color(_FindColor2.R, _FindColor2.G, _FindColor2.B);
		}
		return flag;
	}

	public static bool FindColorFromString(string _strColorValue, ref System.Drawing.Color _FindColor)
	{
		bool flag = false;
		_FindColor = default;
		if (!_strColorValue.StartsWith(EPShared.LocalRM.GetString("RGBString")))
		{
			_FindColor = System.Drawing.Color.FromName(_strColorValue);
			flag = true;
		}
		if (!flag && _strColorValue != string.Empty)
		{
			int num = _strColorValue.IndexOf(' ');
			if (num >= 0 && num < _strColorValue.Length)
			{
				string[] array = _strColorValue.Substring(num + 1).Split('-');
				if (array.Length == 3)
				{
					_FindColor = System.Drawing.Color.FromArgb(Convert.ToInt32(array[0]), Convert.ToInt32(array[1]), Convert.ToInt32(array[2]));
					flag = true;
				}
			}
		}
		return flag;
	}

	public static bool FindStringFromColor(Autodesk.Revit.DB.Color _Color, ref string _strColorValue)
	{
		System.Drawing.Color color = System.Drawing.Color.FromArgb(_Color.Red, _Color.Green, _Color.Blue);
		return FindStringFromColor(color, ref _strColorValue);
	}

	public static bool FindStringFromColor(System.Drawing.Color _Color, ref string _strColorValue)
	{
		bool flag = false;
		string text = _strColorValue;
		if (_Color.IsNamedColor)
		{
			_strColorValue = _Color.Name;
			return true;
		}
		_strColorValue = EPShared.LocalRM.GetString("RGBString") + " " + Convert.ToString(_Color.R) + "-" + Convert.ToString(_Color.G) + "-" + Convert.ToString(_Color.B);
		return true;
	}

	public static FamilyInstance NewFamilyInstance(Document _CurDocument, XYZ _Location, FamilySymbol _FamilySymbol, Element _Host, Level _Level, StructuralType _StructuralType, ref EPTransactionContainer _TransactionContainer)
	{
		return NewFamilyInstance(_CurDocument, _Location, _FamilySymbol, _Host, _Level, _StructuralType, _bUseFailureHandling: true, ref _TransactionContainer);
	}

	public static FamilyInstance NewFamilyInstance(Document _CurDocument, XYZ _Location, FamilySymbol _FamilySymbol, Element _Host, Level _Level, StructuralType _StructuralType, bool _bUseFailureHandling, ref EPTransactionContainer _TransactionContainer)
	{
		if (_bUseFailureHandling)
		{
			_TransactionContainer.FailureHandlingOptions(BuiltInFailures.OverlapFailures.DuplicateInstances);
		}
		if (!_FamilySymbol.IsActive)
		{
			_FamilySymbol.Activate();
		}
		try
		{
			return _CurDocument.Create.NewFamilyInstance(_Location, _FamilySymbol, _Host, _Level, _StructuralType);
		}
		catch (System.Exception ex)
		{
			if (_Host == null || !TerrainApi.UsesToposolids)
			{
				throw;
			}
			RuntimeDiagnostics.Write("Terrain host placement failed, placing on level: " + ex.Message);
			return _CurDocument.Create.NewFamilyInstance(_Location, _FamilySymbol, _Level, _StructuralType);
		}
	}

	public static bool LoadFamilySymbol(Document _CurDocument, string _strFamilyFilename, string _strSymbolName, IFamilyLoadOptions _FamilyLoadOptions, out FamilySymbol _FamilySymbol, bool _bUseFailureHandling, ref EPTransactionContainer _TransactionContainer)
	{
		if (_bUseFailureHandling)
		{
			_TransactionContainer.FailureHandlingOptions(BuiltInFailures.OverlapFailures.DuplicateInstances);
		}
		return _CurDocument.LoadFamilySymbol(_strFamilyFilename, _strSymbolName, _FamilyLoadOptions, out _FamilySymbol);
	}

	public static bool LoadFamilySymbol(Document _CurDocument, string _strFamilyFilename, string _strSymbolName, bool _bUseFailureHandling, ref EPTransactionContainer _TransactionContainer)
	{
		if (_bUseFailureHandling)
		{
			_TransactionContainer.FailureHandlingOptions(BuiltInFailures.OverlapFailures.DuplicateInstances);
		}
		return _CurDocument.LoadFamilySymbol(_strFamilyFilename, _strSymbolName);
	}

	public static bool LoadFamilySymbol(Document _CurDocument, string _strFamilyFilename, string _strSymbolName, out FamilySymbol _FamilySymbol, bool _bUseFailureHandling, ref EPTransactionContainer _TransactionContainer)
	{
		if (_bUseFailureHandling)
		{
			_TransactionContainer.FailureHandlingOptions(BuiltInFailures.OverlapFailures.DuplicateInstances);
		}
		bool result = _CurDocument.LoadFamilySymbol(_strFamilyFilename, _strSymbolName, out _FamilySymbol);
		if (_FamilySymbol != null && !_FamilySymbol.IsActive)
		{
			_FamilySymbol.Activate();
		}
		return result;
	}

	public static bool LoadFamily(Document _CurDocument, string _strFilename, out Family _Family, bool _bUseFailureHandling, ref EPTransactionContainer _TransactionContainer)
	{
		if (_bUseFailureHandling)
		{
			_TransactionContainer.FailureHandlingOptions(BuiltInFailures.OverlapFailures.DuplicateInstances);
		}
		return _CurDocument.LoadFamily(_strFilename, out _Family);
	}

	public static bool LoadFamily(Document _CurDocument, string _strFilename, bool _bUseFailureHandling, ref EPTransactionContainer _TransactionContainer)
	{
		if (_bUseFailureHandling)
		{
			_TransactionContainer.FailureHandlingOptions(BuiltInFailures.OverlapFailures.DuplicateInstances);
		}
		return _CurDocument.LoadFamily(_strFilename);
	}

	public static Family LoadFamily(Document _CurDocument, Document _SymbolDocument, bool _bUseFailureHandling, ref EPTransactionContainer _TransactionContainer)
	{
		if (_bUseFailureHandling)
		{
			_TransactionContainer.FailureHandlingOptions(BuiltInFailures.OverlapFailures.DuplicateInstances);
		}
		return _CurDocument.LoadFamily(_SymbolDocument);
	}

	public static Eps_Point GetSurveyPoint(Document _Document)
	{
		BasePoint basePoint = null;
		BasePoint basePoint2 = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		filteredElementCollector.OfClass(typeof(BasePoint)).OfCategory(BuiltInCategory.OST_SharedBasePoint);
		List<Element> list = filteredElementCollector.ToList();
		foreach (Element item in list)
		{
			if (item is BasePoint { Category: not null, IsShared: not false } basePoint3)
			{
				basePoint = basePoint3;
				break;
			}
		}
		Eps_Point eps_Point = new Eps_Point();
		if (basePoint != null)
		{
			Parameter parameter = ((Element)basePoint).get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM);
			if (parameter != null)
			{
				eps_Point.y = parameter.AsDouble();
			}
			parameter = ((Element)basePoint).get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM);
			if (parameter != null)
			{
				eps_Point.x = parameter.AsDouble();
			}
			parameter = ((Element)basePoint).get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
			if (parameter != null)
			{
				eps_Point.z = parameter.AsDouble();
			}
		}
		return eps_Point;
	}

	public static FamilyInstance FindMassFamilyInstance(Document _Document, string _strFormUniqueId)
	{
		int _nIntersectionSWStreetMassLastIndex = 0;
		return FindMassFamilyInstance(_Document, _strFormUniqueId, ref _nIntersectionSWStreetMassLastIndex);
	}

	public static FamilyInstance FindMassFamilyInstance(Document _Document, string _strFormUniqueId, ref int _nIntersectionSWStreetMassLastIndex)
	{
		FamilyInstance familyInstance = null;
		_nIntersectionSWStreetMassLastIndex = 0;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		List<Element> list = filteredElementCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Mass).ToList();
		string text = EPShared.LocalRM.GetString("IntersectionSWStreetMass");
		string empty = string.Empty;
		string text2 = string.Empty;
		familyInstance = null;
		foreach (Element item in list)
		{
			if (!(item is FamilyInstance { Category: not null } familyInstance2))
			{
				continue;
			}
			if (string.Compare(_strFormUniqueId, familyInstance2.UniqueId, ignoreCase: true) == 0)
			{
				return familyInstance2;
			}
			if (!familyInstance2.Symbol.Family.Name.StartsWith(text))
			{
				continue;
			}
			empty = familyInstance2.Symbol.Family.Name.Substring(text.Length + 1);
			for (int i = 0; i < empty.Length && char.IsDigit(empty[i]); i++)
			{
				text2 += empty[i];
			}
			if (text2 != string.Empty)
			{
				int num = Convert.ToInt32(text2);
				if (num > _nIntersectionSWStreetMassLastIndex)
				{
					_nIntersectionSWStreetMassLastIndex = num;
				}
			}
			text2 = string.Empty;
		}
		return null;
	}

	public static DirectShape FindDirectShape(Document _Document, string _strFormUniqueId)
	{
		int _nIntersectionSWStreetMassLastIndex = 0;
		return FindDirectShape(_Document, _strFormUniqueId, ref _nIntersectionSWStreetMassLastIndex);
	}

	public static DirectShape FindDirectShape(Document _Document, string _strFormUniqueId, ref int _nIntersectionSWStreetMassLastIndex)
	{
		DirectShape directShape = null;
		_nIntersectionSWStreetMassLastIndex = 0;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(_Document);
		List<Element> list = filteredElementCollector.OfClass(typeof(DirectShape)).ToList();
		string text = EPShared.LocalRM.GetString("IntersectionSWStreetMass");
		string empty = string.Empty;
		string text2 = string.Empty;
		directShape = null;
		foreach (Element item in list)
		{
			if (!(item is DirectShape { Category: not null } directShape2))
			{
				continue;
			}
			if (string.Compare(_strFormUniqueId, directShape2.UniqueId, ignoreCase: true) == 0)
			{
				return directShape2;
			}
			if (!directShape2.Name.StartsWith(text))
			{
				continue;
			}
			empty = directShape2.Name.Substring(text.Length + 1);
			for (int i = 0; i < empty.Length && char.IsDigit(empty[i]); i++)
			{
				text2 += empty[i];
			}
			if (text2 != string.Empty)
			{
				int num = Convert.ToInt32(text2);
				if (num > _nIntersectionSWStreetMassLastIndex)
				{
					_nIntersectionSWStreetMassLastIndex = num;
				}
			}
			text2 = string.Empty;
		}
		return null;
	}

	public static bool IsParameterIDOnFamily(Document _Document, PlacementSettings _PlacementSettings, FamilySymbol _FamilySymbol)
	{
		if (_FamilySymbol == null)
		{
			return true;
		}
		Eps_Point eps_Point = new Eps_Point();
		bool result = false;
		bool bUseFailureHandling = true;
		if (_PlacementSettings.TransactionContainer == null)
		{
			bUseFailureHandling = false;
		}
		EPTransactionContainer _TransactionContainer = _PlacementSettings.TransactionContainer;
		FamilyInstance familyInstance = NewFamilyInstance(_Document, eps_Point.XY, _FamilySymbol, null, _Document.ActiveView.GenLevel, StructuralType.NonStructural, bUseFailureHandling, ref _TransactionContainer);
		_PlacementSettings.TransactionContainer = _TransactionContainer;
		if (familyInstance != null)
		{
			Parameter parameter = familyInstance.LookupParameter(RevitParameters.PlacementIDFamilyParameter);
			if (parameter != null)
			{
				result = true;
			}
		}
		_PlacementSettings.TransactionContainer.BeginTransaction();
		_Document.Delete(familyInstance.Id);
		_PlacementSettings.TransactionContainer.EndTransaction();
		return result;
	}

	public static void ShowHelpInRevit(string contextualID)
	{
		ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.ContextId, contextualID);
		contextualHelp.Launch();
	}
}
