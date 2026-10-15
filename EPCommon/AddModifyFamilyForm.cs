using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using SITES.Sites_Core;
using SITES.Utilities;

namespace EPCommon;

public class AddModifyFamilyForm : EPForm
{
	private enum FormForAddOrModify
	{
		Add,
		Modify
	}

	private Units m_Units;

	private const UnitType m_LengthUnitType = UnitType.UT_Length;

	private const UnitType m_SlopeUnitType = UnitType.UT_Slope;

	private const UnitType m_AngularUnitType = UnitType.UT_Angle;

	private DisplayUnit m_DisplayUnitSystem;

	private FormatOptions m_FormatOptions;

	private FormatOptions m_SlopeFormatOptions;

	private FormatOptions m_AngularOptions;

	private UnitSymbolType m_UnitSymbolType;

	private UnitSymbolType m_SlopeUnitSymbolType;

	private UnitSymbolType m_AngularSymbolType;

	private List<FamilyTypeContainer> m_FamilyTypeContainerList;

	private int m_nSeletedIndex = -1;

	private List<FamilyParameterTypeWrapper> m_ParameterList;

	private QuantumListView m_ParameterListView;

	private LandcaddID m_FamilyType = LandcaddID.Street;

	private List<Material> m_AllMaterialList = new List<Material>();

	private QuantumListView m_GlobalListView;

	private bool m_bPreview = true;

	private PictureBox m_PreviewBox;

	private string m_strLinearIncrement = "5";

	private string m_strCircularIncrement;

	private string m_strFamilyName = string.Empty;

	private List<string> m_SlopeDisplayUnits = new List<string>();

	private List<DisplayUnitType> m_SlopeUnits = new List<DisplayUnitType>();

	private DisplayUnitType m_CurrentSlopeDisplayUnitType;

	private DisplayUnitType m_PreviousSlopeDisplayUnitType;

	private DecimalSymbol m_DecimalSymbolType;

	private double m_dAngularRounding;

	private double m_dCurrentSlopeAccuracy;

	private FormForAddOrModify m_AddOrModify;

	private IContainer components;

	private Button m_ButtonOk;

	private Button m_ButtonCancel;

	private Label m_UnitLabel;

	private TabControl m_StreetTabControl;

	private ComboBox m_SlopeComboBox;

	public AddModifyFamilyForm(List<FamilyParameterTypeWrapper> _ParameterList, ref List<FamilyTypeContainer> _FamilyTypeContainerList, Units _Units, DisplayUnit _UnitType, FormatOptions _FormatOption, UnitSymbolType _UnitSymbolType, FormatOptions _SlopeFormatOption, UnitSymbolType _SlopeUnitSymbolType, FormatOptions _AngleFormatOption, UnitSymbolType _AngleUnitSymbolType, DecimalSymbol _DecimalSymbolType, int _nSelectedIndex, LandcaddID _LandCaddID, string _strFamilyName)
	{
		InitializeComponent();
		m_ParameterList = new List<FamilyParameterTypeWrapper>(_ParameterList);
		int num = m_ParameterList.FindIndex(FindSiteworksIDParameterInList);
		if (num > -1)
		{
			m_ParameterList.RemoveAt(num);
		}
		num = m_ParameterList.FindIndex(FindSideWalkLocationLineParameterInList);
		if (num > -1)
		{
			m_ParameterList.RemoveAt(num);
		}
		m_FamilyTypeContainerList = _FamilyTypeContainerList;
		m_Units = _Units;
		m_DisplayUnitSystem = _UnitType;
		m_FormatOptions = _FormatOption;
		m_UnitSymbolType = _UnitSymbolType;
		m_SlopeFormatOptions = _SlopeFormatOption;
		m_SlopeUnitSymbolType = _SlopeUnitSymbolType;
		m_nSeletedIndex = _nSelectedIndex;
		m_FamilyType = _LandCaddID;
		m_AngularOptions = _AngleFormatOption;
		m_AngularSymbolType = _AngleUnitSymbolType;
		m_DecimalSymbolType = _DecimalSymbolType;
		m_strFamilyName = _strFamilyName;
		try
		{
			m_dAngularRounding = m_AngularOptions.Accuracy;
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_dAngularRounding = 1.0;
		}
		m_strCircularIncrement = ValueConversion.ToString(0.15707963249999998);
		m_ParameterListView.TabIndex = 0;
		m_ButtonOk.TabIndex = 1;
		m_ButtonCancel.TabIndex = 2;
		InitializeData();
		GetDialogPosition();
		if (m_FamilyType != LandcaddID.Street)
		{
			m_ParameterListView.OnListViewItemChanged += ParameterListViewOnListViewItemChanged;
		}
		FormClosing += AddModifyFamilyFormClosing;
		ResizeBegin += FormResizeBegin;
	}

	public AddModifyFamilyForm(List<FamilyParameterTypeWrapper> _ParameterList, ref List<FamilyTypeContainer> _FamilyTypeContainerList, Units _Units, DisplayUnit _UnitType, FormatOptions _FormatOption, UnitSymbolType _UnitSymbolType, FormatOptions _SlopeFormatOption, UnitSymbolType _SlopeUnitSymbolType, FormatOptions _AngleFormatOption, UnitSymbolType _AngleUnitSymbolType, List<Material> _MaterialList, DecimalSymbol _DecimalSymbolType, int _nSelectedIndex, LandcaddID _LandCaddID, string _strFamilyName)
	{
		InitializeComponent();
		m_ParameterList = new List<FamilyParameterTypeWrapper>(_ParameterList);
		int num = m_ParameterList.FindIndex(FindSiteworksIDParameterInList);
		if (num > -1)
		{
			m_ParameterList.RemoveAt(num);
		}
		num = m_ParameterList.FindIndex(FindSideWalkLocationLineParameterInList);
		if (num > -1)
		{
			m_ParameterList.RemoveAt(num);
		}
		m_FamilyTypeContainerList = _FamilyTypeContainerList;
		m_Units = _Units;
		m_DisplayUnitSystem = _UnitType;
		m_FormatOptions = _FormatOption;
		m_UnitSymbolType = _UnitSymbolType;
		m_SlopeFormatOptions = _SlopeFormatOption;
		m_SlopeUnitSymbolType = _SlopeUnitSymbolType;
		m_nSeletedIndex = _nSelectedIndex;
		m_FamilyType = _LandCaddID;
		m_AllMaterialList = _MaterialList;
		m_AngularOptions = _AngleFormatOption;
		m_AngularSymbolType = _AngleUnitSymbolType;
		m_DecimalSymbolType = _DecimalSymbolType;
		m_strFamilyName = _strFamilyName;
		try
		{
			m_dAngularRounding = m_AngularOptions.Accuracy;
		}
		catch (Autodesk.Revit.Exceptions.InvalidOperationException)
		{
			m_dAngularRounding = 1.0;
		}
		m_strCircularIncrement = ValueConversion.ToString(0.15707963249999998);
		InitializeData();
		GetDialogPosition();
		if (m_FamilyType != LandcaddID.Street)
		{
			m_ParameterListView.OnListViewItemChanged += ParameterListViewOnListViewItemChanged;
		}
		FormClosing += AddModifyFamilyFormClosing;
		ResizeBegin += FormResizeBegin;
	}

	private static bool FindSiteworksIDParameterInList(FamilyParameterTypeWrapper _FindParameter)
	{
		if (string.Compare(_FindParameter.ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
		{
			return true;
		}
		return false;
	}

	private static bool FindSideWalkLocationLineParameterInList(FamilyParameterTypeWrapper _FindParameter)
	{
		if (string.Compare(_FindParameter.ParameterName, RevitParameters.SideWalkLocationLineParameter, ignoreCase: true) == 0)
		{
			return true;
		}
		return false;
	}

	private void AddModifyFamilyFormClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private void FormResizeBegin(object sender, EventArgs e)
	{
		if (m_ParameterListView != null)
		{
			m_ParameterListView.SetAnchor();
		}
	}

	private void ParameterListViewOnListViewItemChanged(int _nChangedIndex)
	{
		if (!HandleSlopes(_nChangedIndex))
		{
			switch (m_FamilyType)
			{
			case LandcaddID.PlantMix:
			case LandcaddID.PatioWalk:
				HandlePlantMixPatio(_nChangedIndex);
				break;
			case LandcaddID.Street:
				HandleStreet(_nChangedIndex);
				break;
			case LandcaddID.Curb:
				HandleCurb(_nChangedIndex);
				break;
			case LandcaddID.Sidewalk:
				HandleSideWalk(_nChangedIndex);
				break;
			case LandcaddID.RetainingWall:
				HandleRetainingWall(_nChangedIndex);
				break;
			case LandcaddID.Steps:
				HandleSteps(_nChangedIndex);
				break;
			case LandcaddID.ParkingLot:
				HandleParkingLots(_nChangedIndex);
				break;
			case LandcaddID.SoftTerrain:
				HandleSoftTerrain(_nChangedIndex);
				break;
			}
		}
	}

	private bool HandleSlopes(int _nChangedIndex)
	{
		bool result = false;
		string strA = m_ParameterListView.Items[_nChangedIndex].SubItems[0].Text;
		string text = m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text;
		if (string.Compare(strA, EPShared.LocalRM.GetString("SoftTerrainCutSlopeParameter"), ignoreCase: true) == 0 || string.Compare(strA, EPShared.LocalRM.GetString("SoftTerrainFillSlopeParameter"), ignoreCase: true) == 0 || string.Compare(strA, EPShared.LocalRM.GetString("SideWalkCutSlopeParameter"), ignoreCase: true) == 0 || string.Compare(strA, EPShared.LocalRM.GetString("SideWalkFillSlopeParameter"), ignoreCase: true) == 0 || string.Compare(strA, EPShared.LocalRM.GetString("RetainingWallCutSlopeParameter"), ignoreCase: true) == 0 || string.Compare(strA, EPShared.LocalRM.GetString("RetainingWallFillSlopeParameter"), ignoreCase: true) == 0 || string.Compare(strA, EPShared.LocalRM.GetString("StreetCutSlopeParameter"), ignoreCase: true) == 0 || string.Compare(strA, EPShared.LocalRM.GetString("StreetFillSlopeParameter"), ignoreCase: true) == 0)
		{
			try
			{
				double _dReturnedDouble = 0.0;
				text = text.Trim(default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char));
				string _strToDisplay = text;
				if (RevitGeometry.Units_ConvertStringToDouble(text, m_Units, UnitType.UT_Slope, ref _dReturnedDouble))
				{
					if (_dReturnedDouble < 0.0001)
					{
						RevitGeometry.Units_ConvertDoubleToString(0.0001, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, UnitSymbolType.UST_NONE, m_Units, ref _strToDisplay);
						string strText = EPShared.LocalRM.GetString("InvalidCutFillSlopeValue") + _strToDisplay;
						RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						_dReturnedDouble = 0.0001;
					}
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, UnitSymbolType.UST_NONE, m_Units, ref _strToDisplay);
				}
				text = _strToDisplay;
				text = AppendSlopeSymbol(text);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = text;
			}
			catch (Exception)
			{
				string strText2 = EPShared.LocalRM.GetString("InvalidSlopeValue");
				RevitMessageBox.Show(strText2, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
			}
			result = true;
		}
		else if (string.Compare(strA, EPShared.LocalRM.GetString("StreetGutterSlopeParameter"), ignoreCase: true) == 0 || string.Compare(strA, EPShared.LocalRM.GetString("StreetCrossSlopeParameter"), ignoreCase: true) == 0 || string.Compare(strA, EPShared.LocalRM.GetString("CurbGutterSlopeParameter"), ignoreCase: true) == 0)
		{
			try
			{
				double _dReturnedDouble2 = 0.0;
				text = text.Trim(default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char));
				string _strToDisplay2 = text;
				if (RevitGeometry.Units_ConvertStringToDouble(text, m_Units, UnitType.UT_Slope, ref _dReturnedDouble2))
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble2, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, UnitSymbolType.UST_NONE, m_Units, ref _strToDisplay2);
				}
				text = _strToDisplay2;
				text = AppendSlopeSymbol(text);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = text;
			}
			catch (Exception)
			{
				string strText3 = EPShared.LocalRM.GetString("InvalidSlopeValue");
				RevitMessageBox.Show(strText3, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
			}
			result = true;
		}
		return result;
	}

	private string AppendSlopeSymbol(string _strParameterValue)
	{
		if (m_SlopeUnitSymbolType == UnitSymbolType.UST_NONE)
		{
			return _strParameterValue;
		}
		string text = string.Empty;
		switch (m_CurrentSlopeDisplayUnitType)
		{
		case DisplayUnitType.DUT_RATIO_10:
			text = ":10";
			break;
		case DisplayUnitType.DUT_RATIO_12:
			text = ":12";
			break;
		case DisplayUnitType.DUT_RISE_OVER_FOOT:
			text = "/ 1' - 0\"";
			break;
		case DisplayUnitType.DUT_RISE_OVER_INCHES:
			text = "/ 12\"";
			break;
		case DisplayUnitType.DUT_RISE_OVER_MMS:
			text = "/ 1000";
			break;
		case DisplayUnitType.DUT_SLOPE_DEGREES:
			text = "°";
			break;
		case DisplayUnitType.DUT_PERCENTAGE:
			text = "%";
			break;
		}
		if (_strParameterValue.IndexOf(":12") > 0)
		{
			return _strParameterValue;
		}
		if (_strParameterValue.IndexOf(":10") > 0)
		{
			return _strParameterValue;
		}
		if (_strParameterValue.IndexOf("/ 12\"") > 0)
		{
			return _strParameterValue;
		}
		if (_strParameterValue.IndexOf("/ 1' - 0\"") > 0)
		{
			return _strParameterValue;
		}
		if (_strParameterValue.IndexOf("/ 1000") > 0)
		{
			return _strParameterValue;
		}
		if (_strParameterValue.IndexOf("°") > 0)
		{
			return _strParameterValue;
		}
		if (_strParameterValue.IndexOf("%") > 0)
		{
			return _strParameterValue;
		}
		_strParameterValue += text;
		return _strParameterValue;
	}

	private void HandleStreet(int _nChangedIndex)
	{
		double _dReturnedDouble = 0.0;
		string strB = m_ParameterListView.Items[_nChangedIndex].SubItems[0].Text;
		string _strAngleStr = m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text;
		if (string.Compare(EPShared.LocalRM.GetString("TypeNameGenericModelFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			if (CheckForSpecialCharacter(_strAngleStr))
			{
				string strText = EPShared.LocalRM.GetString("InvalidTypeNameMessage_") + "\n\":{}[]|;<>?`~\n" + EPShared.LocalRM.GetString("_InvalidTypeNameMessage");
				RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
			}
			return;
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetNumberOfLanesParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				int num = Convert.ToInt32(_strAngleStr);
				if (num <= 0)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidNumberOfLanesMessage"), EPShared.LocalRM.GetString("Error"));
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidNumberOfLanesMessage"), EPShared.LocalRM.GetString("Error"));
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetLaneWidthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 1.0)
				{
					string _strToDisplay = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(1.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetLaneWidthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetPavementDepthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay2 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay2);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetPavementDepthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay2, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay2;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetBaseSurfaceParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					string _strToDisplay3 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay3);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetBaseSurfaceParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay3, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetSubbaseSurfaceParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					string _strToDisplay4 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay4);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetSubbaseSurfaceParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay4, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetGutterWidthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetGutterWidthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetCurbOffsetParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					string _strToDisplay5 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay5);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetCurbOffsetParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay5, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay5;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetCurbWidthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay6 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay6);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetCurbWidthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay6, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay6;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetCurbHeightParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay7 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay7);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetCurbHeightParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay7, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay7;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetBoulevardWidthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					string _strToDisplay8 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay8);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetBoulevardWidthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay8, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay8;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetPavementSurfaceParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("StreetCurbMaterialParameter"), strB, ignoreCase: true) == 0)
		{
			return;
		}
		if (string.Compare(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				if (!RevitGeometry.StrE_GetValidRealNumber(_strAngleStr, ref _dReturnedDouble))
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				}
				else if (_dReturnedDouble >= 0.0)
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", _dReturnedDouble);
				}
				else
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", 0.0);
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("LinearIncrementParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay9 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay9);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("LinearIncrementParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay9, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay9;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), strB, ignoreCase: true) != 0)
		{
			return;
		}
		try
		{
			_dReturnedDouble = 0.0174532925;
			_dReturnedDouble = RevitGeometry.eplGeoE_ConvertAngleToRadians(_strAngleStr);
			if (RevitDefines.LT(_dReturnedDouble, 0.0174532925))
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				_dReturnedDouble = 0.15707963249999998;
			}
			RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), _dReturnedDouble, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
		}
		catch (Exception)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
			m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
			m_ParameterListView.SetFocus(_nChangedIndex);
		}
	}

	private void HandleSoftTerrain(int _nChangedIndex)
	{
		double _dReturnedDouble = 0.0;
		string strB = m_ParameterListView.Items[_nChangedIndex].SubItems[0].Text;
		string _strAngleStr = m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text;
		if (string.Compare(EPShared.LocalRM.GetString("TypeNameGenericModelFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			if (CheckForSpecialCharacter(_strAngleStr))
			{
				string strText = EPShared.LocalRM.GetString("InvalidTypeNameMessage_") + "\n\":{}[]|;<>?`~\n" + EPShared.LocalRM.GetString("_InvalidTypeNameMessage");
				RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
			}
			return;
		}
		if (string.Compare(EPShared.LocalRM.GetString("SoftTerrainCutSlopeParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("SoftTerrainFillSlopeParameter"), strB, ignoreCase: true) == 0)
		{
			_strAngleStr = _strAngleStr.Trim(default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char));
			string _strToDisplay = _strAngleStr;
			if (RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Slope, ref _dReturnedDouble))
			{
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref _strToDisplay);
			}
			_strAngleStr = _strToDisplay;
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
			return;
		}
		if (string.Compare(EPShared.LocalRM.GetString("SoftTerrainLeftOffsetWidthParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("SoftTerrainRightOffsetWidthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					string _strToDisplay2 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay2);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidOffsetMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay2;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				if (!RevitGeometry.StrE_GetValidRealNumber(_strAngleStr, ref _dReturnedDouble))
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				}
				else if (_dReturnedDouble >= 0.0)
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", _dReturnedDouble);
				}
				else
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", 0.0);
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("LinearIncrementParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay3 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay3);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("LinearIncrementParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay3, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay3;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), strB, ignoreCase: true) != 0)
		{
			return;
		}
		try
		{
			_dReturnedDouble = 0.0174532925;
			_dReturnedDouble = RevitGeometry.eplGeoE_ConvertAngleToRadians(_strAngleStr);
			if (RevitDefines.LT(_dReturnedDouble, 0.0174532925))
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				_dReturnedDouble = 0.15707963249999998;
			}
			RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), _dReturnedDouble, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
		}
		catch (Exception)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
			m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
			m_ParameterListView.SetFocus(_nChangedIndex);
		}
	}

	private void HandlePlantMixPatio(int _nChangedIndex)
	{
		double _dReturnedDouble = 0.0;
		string strB = m_ParameterListView.Items[_nChangedIndex].SubItems[0].Text;
		string _strToDisplay = m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text;
		if (m_FamilyType == LandcaddID.PlantMix)
		{
			if (string.Compare(EPShared.LocalRM.GetString("TypeNameGenericModelFamilyParameter"), strB, ignoreCase: true) == 0)
			{
				if (CheckForSpecialCharacter(_strToDisplay))
				{
					string strText = EPShared.LocalRM.GetString("InvalidTypeNameMessage_") + "\n\":{}[]|;<>?`~\n" + EPShared.LocalRM.GetString("_InvalidTypeNameMessage");
					RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
			}
			else
			{
				if (string.Compare(EPShared.LocalRM.GetString("CommonNamePlantMixFamilyParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("ScientificNamePlantMixFamilyParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("PlantSizePlantMixFamilyParameter"), strB, ignoreCase: true) == 0)
				{
					return;
				}
				if (string.Compare(EPShared.LocalRM.GetString("SpacingMethodPlantMixFamilyParameter"), strB, ignoreCase: true) == 0)
				{
					if (string.Compare(_strToDisplay, EPShared.LocalRM.GetString("AreaPlantMixFamilyParameter"), ignoreCase: true) == 0)
					{
						m_ParameterListView.Items[_nChangedIndex + 1].SubItems[1].Text = EPShared.LocalRM.GetString("NotAvailableString");
					}
					else if (string.Compare(m_ParameterListView.Items[_nChangedIndex + 1].SubItems[1].Text, EPShared.LocalRM.GetString("NotAvailableString"), ignoreCase: true) == 0 || m_ParameterListView.Items[_nChangedIndex + 1].SubItems[1].Text == string.Empty)
					{
						_strToDisplay = string.Empty;
						RevitGeometry.Units_ConvertDoubleToString(1.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
						m_ParameterListView.Items[_nChangedIndex + 1].SubItems[1].Text = _strToDisplay;
					}
					return;
				}
				if (string.Compare(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), strB, ignoreCase: true) == 0)
				{
					try
					{
						if (!RevitGeometry.StrE_GetValidRealNumber(_strToDisplay, ref _dReturnedDouble))
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
							m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
						}
						else if (_dReturnedDouble >= 0.0)
						{
							m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", _dReturnedDouble);
						}
						else
						{
							m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", 0.0);
						}
						return;
					}
					catch (Exception)
					{
						RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
						m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
						m_ParameterListView.SetFocus(_nChangedIndex);
						return;
					}
				}
				if (string.Compare(EPShared.LocalRM.GetString("PercentagePlantMixFamilyParameter"), strB, ignoreCase: true) == 0)
				{
					try
					{
						if (!RevitGeometry.StrE_GetValidRealNumber(_strToDisplay, ref _dReturnedDouble) || _dReturnedDouble <= 0.0 || _dReturnedDouble > 100.0)
						{
							RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("PercentagePlantMixFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
							m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = "1";
						}
						else
						{
							m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
						}
						return;
					}
					catch (Exception)
					{
						RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage") + EPShared.LocalRM.GetString("PercentagePlantMixFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
						m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
						m_ParameterListView.SetFocus(_nChangedIndex);
						return;
					}
				}
				if (string.Compare(EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter"), strB, ignoreCase: true) == 0)
				{
					try
					{
						_dReturnedDouble = 0.0;
						RevitGeometry.Units_ConvertStringToDouble(_strToDisplay, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
						RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
						m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
					}
					catch (Exception)
					{
						RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
						m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
						m_ParameterListView.SetFocus(_nChangedIndex);
					}
				}
			}
		}
		else
		{
			if (m_FamilyType != LandcaddID.PatioWalk)
			{
				return;
			}
			if (string.Compare(EPShared.LocalRM.GetString("ThicknessPathWalkFamilyParameter"), strB, ignoreCase: true) == 0)
			{
				try
				{
					_dReturnedDouble = 0.0;
					RevitGeometry.Units_ConvertStringToDouble(_strToDisplay, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
					return;
				}
				catch (Exception)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("ThicknessPathWalkFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
					return;
				}
			}
			if (string.Compare(EPShared.LocalRM.GetString("EdgeWidthPathWalkFamilyParameter"), strB, ignoreCase: true) != 0)
			{
				return;
			}
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strToDisplay, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				double num = _dReturnedDouble;
				if (num < 3.0)
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
					return;
				}
				RevitGeometry.Units_ConvertDoubleToString(3.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
				RevitMessageBox.Show(EPShared.LocalRM.GetString("ValidEdgeWidth") + " " + _strToDisplay, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("EdgeWidthPathWalkFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
			}
		}
	}

	private void HandleCurb(int _nChangedIndex)
	{
		double _dReturnedDouble = 0.0;
		string strB = m_ParameterListView.Items[_nChangedIndex].SubItems[0].Text;
		string _strAngleStr = m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text;
		if (string.Compare(EPShared.LocalRM.GetString("CurbTypeManagerString"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("CurbMaterialParameter"), strB, ignoreCase: true) == 0)
		{
			return;
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetCurbHeightParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetCurbHeightParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("CurbGutterHeightParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay2 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay2);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CurbGutterHeightParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay2, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay2;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("CurbBaseSurfaceParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CurbBaseSurfaceParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("CurbSubbaseSurfaceParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CurbSubbaseSurfaceParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetCurbOffsetParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					string _strToDisplay3 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay3);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetCurbOffsetParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay3, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay3;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetCurbWidthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay4 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay4);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetCurbWidthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay4, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay4;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("StreetGutterWidthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					string _strToDisplay5 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay5);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("StreetGutterWidthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay5, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay5;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				if (!RevitGeometry.StrE_GetValidRealNumber(_strAngleStr, ref _dReturnedDouble))
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				}
				else if (_dReturnedDouble >= 0.0)
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", _dReturnedDouble);
				}
				else
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", 0.0);
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("LinearIncrementParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay6 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay6);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("LinearIncrementParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay6, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay6;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), strB, ignoreCase: true) != 0)
		{
			return;
		}
		try
		{
			_dReturnedDouble = 0.0;
			_dReturnedDouble = RevitGeometry.eplGeoE_ConvertAngleToRadians(_strAngleStr);
			if (RevitDefines.LT(_dReturnedDouble, 0.0174532925))
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				_dReturnedDouble = 0.15707963249999998;
			}
			RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), _dReturnedDouble, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
		}
		catch (Exception)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
			m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
			m_ParameterListView.SetFocus(_nChangedIndex);
		}
	}

	private void HandleSideWalk(int _nChangedIndex)
	{
		double _dReturnedDouble = 0.0;
		string strB = m_ParameterListView.Items[_nChangedIndex].SubItems[0].Text;
		string _strAngleStr = m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text;
		if (string.Compare(EPShared.LocalRM.GetString("TypeNameGenericModelFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			if (CheckForSpecialCharacter(_strAngleStr))
			{
				string strText = EPShared.LocalRM.GetString("InvalidTypeNameMessage_") + "\n\":{}[]|;<>?`~\n" + EPShared.LocalRM.GetString("_InvalidTypeNameMessage");
				RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
			}
			return;
		}
		if (string.Compare(EPShared.LocalRM.GetString("SideWalkPavementDepthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("SideWalkPavementDepthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0833333;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("SideWalkBaseSurfaceParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("SideWalkBaseSurfaceParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("SideWalkSubbaseSurfaceParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("SideWalkSubbaseSurfaceParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("SideWalkWidthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("SideWalkWidthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0833333;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				if (!RevitGeometry.StrE_GetValidRealNumber(_strAngleStr, ref _dReturnedDouble))
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				}
				else if (_dReturnedDouble >= 0.0)
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", _dReturnedDouble);
				}
				else
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", 0.0);
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("LinearIncrementParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("LinearIncrementParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), strB, ignoreCase: true) != 0)
		{
			return;
		}
		try
		{
			_dReturnedDouble = 0.0;
			_dReturnedDouble = RevitGeometry.eplGeoE_ConvertAngleToRadians(_strAngleStr);
			if (RevitDefines.LT(_dReturnedDouble, 0.0174532925))
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				_dReturnedDouble = 0.0174532925;
			}
			RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), _dReturnedDouble, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
		}
		catch (Exception)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
			m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
			m_ParameterListView.SetFocus(_nChangedIndex);
		}
	}

	private void HandleRetainingWall(int _nChangedIndex)
	{
		double _dReturnedDouble = 0.0;
		string strB = m_ParameterListView.Items[_nChangedIndex].SubItems[0].Text;
		string _strAngleStr = m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text;
		if (string.Compare(EPShared.LocalRM.GetString("TypeNameGenericModelFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			if (CheckForSpecialCharacter(_strAngleStr))
			{
				string strText = EPShared.LocalRM.GetString("InvalidTypeNameMessage_") + "\n\":{}[]|;<>?`~\n" + EPShared.LocalRM.GetString("_InvalidTypeNameMessage");
				RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
			}
			return;
		}
		if (string.Compare(EPShared.LocalRM.GetString("RetainingWallHeightParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("RetainingWallHeightParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0833333;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("RetainingWallBaseSurfaceParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("RetainingWallBaseSurfaceParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
					_dReturnedDouble = 0.0;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("RetainingWallSubbaseSurfaceParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (_dReturnedDouble < 0.0)
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("RetainingWallSubbaseSurfaceParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
					_dReturnedDouble = 0.0;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("RetainingWallWidthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay2 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay2);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("RetainingWallWidthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay2, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0833333;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				if (!RevitGeometry.StrE_GetValidRealNumber(_strAngleStr, ref _dReturnedDouble))
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				}
				else if (_dReturnedDouble >= 0.0)
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", _dReturnedDouble);
				}
				else
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", 0.0);
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("LinearIncrementParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay3 = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay3);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("LinearIncrementParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay3, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay3;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), strB, ignoreCase: true) != 0)
		{
			return;
		}
		try
		{
			_dReturnedDouble = 0.0;
			_dReturnedDouble = RevitGeometry.eplGeoE_ConvertAngleToRadians(_strAngleStr);
			if (_dReturnedDouble < 0.0174532925)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				_dReturnedDouble = 0.0174532925;
			}
			RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), _dReturnedDouble, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
		}
		catch (Exception)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
			m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
			m_ParameterListView.SetFocus(_nChangedIndex);
		}
	}

	private void HandleSteps(int _nChangedIndex)
	{
		double _dReturnedDouble = 0.0;
		string strB = m_ParameterListView.Items[_nChangedIndex].SubItems[0].Text;
		string _strAngleStr = m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text;
		if (string.Compare(EPShared.LocalRM.GetString("TypeNameGenericModelFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			if (CheckForSpecialCharacter(_strAngleStr))
			{
				string strText = EPShared.LocalRM.GetString("InvalidTypeNameMessage_") + "\n\":{}[]|;<>?`~\n" + EPShared.LocalRM.GetString("_InvalidTypeNameMessage");
				RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
			}
			return;
		}
		if (string.Compare(EPShared.LocalRM.GetString("StepsHeightParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("StepsTreadParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("StepsWidthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				if (!RevitGeometry.StrE_GetValidRealNumber(_strAngleStr, ref _dReturnedDouble))
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				}
				else if (_dReturnedDouble >= 0.0)
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", _dReturnedDouble);
				}
				else
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", 0.0);
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("LinearIncrementParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("LinearIncrementParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), strB, ignoreCase: true) != 0)
		{
			return;
		}
		try
		{
			_dReturnedDouble = 0.0;
			_dReturnedDouble = RevitGeometry.eplGeoE_ConvertAngleToRadians(_strAngleStr);
			if (_dReturnedDouble <= 0.0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				_dReturnedDouble = 0.15707963249999998;
			}
			RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), _dReturnedDouble, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
		}
		catch (Exception)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
			m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
			m_ParameterListView.SetFocus(_nChangedIndex);
		}
	}

	private void HandleParkingLots(int _nChangedIndex)
	{
		double _dReturnedDouble = 0.0;
		string strB = m_ParameterListView.Items[_nChangedIndex].SubItems[0].Text;
		string _strAngleStr = m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text;
		if (string.Compare(EPShared.LocalRM.GetString("TypeNameGenericModelFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			if (CheckForSpecialCharacter(_strAngleStr))
			{
				string strText = EPShared.LocalRM.GetString("InvalidTypeNameMessage_") + "\n\":{}[]|;<>?`~\n" + EPShared.LocalRM.GetString("_InvalidTypeNameMessage");
				RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
			}
			return;
		}
		if (string.Compare(EPShared.LocalRM.GetString("ParkingLotPavementDepthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("ParkingLotPavementDepthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0833333;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("ParkingLotParkingBaseDepthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0))
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("ParkingLotParkingBaseDepthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("ParkingLotParkingSubBaseDepthParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0))
				{
					RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("ParkingLotParkingSubBaseDepthParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strAngleStr, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					_dReturnedDouble = 0.0;
				}
				RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				if (!RevitGeometry.StrE_GetValidRealNumber(_strAngleStr, ref _dReturnedDouble))
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				}
				else if (_dReturnedDouble >= 0.0)
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", _dReturnedDouble);
				}
				else
				{
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = ValueConversion.Format("{0:f2}", 0.0);
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CostGenericFamilyParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("LinearIncrementParameter"), strB, ignoreCase: true) == 0)
		{
			try
			{
				_dReturnedDouble = 0.0;
				RevitGeometry.Units_ConvertStringToDouble(_strAngleStr, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
				if (RevitDefines.LT(_dReturnedDouble, 0.0833333))
				{
					string _strToDisplay = string.Empty;
					RevitGeometry.Units_ConvertDoubleToString(0.0833333, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("LinearIncrementParameter") + ". " + EPShared.LocalRM.GetString("MinimumValueIsMessage_") + _strToDisplay, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strToDisplay;
					m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
					m_ParameterListView.SetFocus(_nChangedIndex);
				}
				else
				{
					RevitGeometry.Units_ConvertDoubleToString(_dReturnedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
					m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
				}
				return;
			}
			catch (Exception)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
				m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
				m_ParameterListView.SetFocus(_nChangedIndex);
				return;
			}
		}
		if (string.Compare(EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), strB, ignoreCase: true) != 0)
		{
			return;
		}
		try
		{
			_dReturnedDouble = 0.0;
			_dReturnedDouble = RevitGeometry.eplGeoE_ConvertAngleToRadians(_strAngleStr);
			if (_dReturnedDouble < 0.0174532925)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputFor_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				_dReturnedDouble = 0.0174532925;
			}
			RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), _dReturnedDouble, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = _strAngleStr;
		}
		catch (Exception)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidInputMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			m_ParameterListView.Items[_nChangedIndex].SubItems[1].Text = string.Empty;
			m_ParameterListView.CurrListItem = m_ParameterListView.Items[_nChangedIndex];
			m_ParameterListView.SetFocus(_nChangedIndex);
		}
	}

	private bool CheckForSpecialCharacter(string _strStringToCheck)
	{
		if (_strStringToCheck.StartsWith(string.Empty))
		{
			return false;
		}
		string text = "\":{}[]|;<>?`~";
		if (_strStringToCheck.IndexOfAny(text.ToCharArray()) < 0)
		{
			return false;
		}
		return true;
	}

	private void InitializeData()
	{
		if (m_FamilyType != LandcaddID.Street)
		{
			SetupDlg(out var _, out var _, out var _);
		}
		m_ButtonOk.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		m_ButtonCancel.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		m_UnitLabel.Text = EPShared.LocalRM.GetString("CurrentSlopeUnitString");
		string selectedItem = string.Empty;
		int selectedIndex = -1;
		switch (m_SlopeFormatOptions.LegacyDisplayUnits())
		{
		case DisplayUnitType.DUT_RATIO_10:
			selectedItem = EPShared.LocalRM.GetString("SlopeUnitRatio10");
			selectedIndex = 0;
			break;
		case DisplayUnitType.DUT_RATIO_12:
			selectedItem = EPShared.LocalRM.GetString("SlopeUnitRatio12");
			selectedIndex = 1;
			break;
		case DisplayUnitType.DUT_RISE_OVER_FOOT:
			selectedItem = EPShared.LocalRM.GetString("SlopeUnitRiseFeet");
			selectedIndex = 2;
			break;
		case DisplayUnitType.DUT_RISE_OVER_INCHES:
			selectedItem = EPShared.LocalRM.GetString("SlopeUnitRiseInches");
			selectedIndex = 3;
			break;
		case DisplayUnitType.DUT_RISE_OVER_MMS:
			selectedItem = EPShared.LocalRM.GetString("SlopeUnitRiseMM");
			selectedIndex = 4;
			break;
		case DisplayUnitType.DUT_SLOPE_DEGREES:
			selectedItem = EPShared.LocalRM.GetString("SlopeUnitDegrees");
			selectedIndex = 5;
			break;
		case DisplayUnitType.DUT_PERCENTAGE:
			selectedItem = EPShared.LocalRM.GetString("SlopeUnitPercentage");
			selectedIndex = 6;
			break;
		}
		m_SlopeUnits.Add(DisplayUnitType.DUT_RATIO_10);
		m_SlopeUnits.Add(DisplayUnitType.DUT_RATIO_12);
		m_SlopeUnits.Add(DisplayUnitType.DUT_RISE_OVER_FOOT);
		m_SlopeUnits.Add(DisplayUnitType.DUT_RISE_OVER_INCHES);
		m_SlopeUnits.Add(DisplayUnitType.DUT_RISE_OVER_MMS);
		m_SlopeUnits.Add(DisplayUnitType.DUT_SLOPE_DEGREES);
		m_SlopeUnits.Add(DisplayUnitType.DUT_PERCENTAGE);
		m_SlopeDisplayUnits.Add(EPShared.LocalRM.GetString("SlopeUnitRatio10"));
		m_SlopeDisplayUnits.Add(EPShared.LocalRM.GetString("SlopeUnitRatio12"));
		m_SlopeDisplayUnits.Add(EPShared.LocalRM.GetString("SlopeUnitRiseFeet"));
		m_SlopeDisplayUnits.Add(EPShared.LocalRM.GetString("SlopeUnitRiseInches"));
		m_SlopeDisplayUnits.Add(EPShared.LocalRM.GetString("SlopeUnitRiseMM"));
		m_SlopeDisplayUnits.Add(EPShared.LocalRM.GetString("SlopeUnitDegrees"));
		m_SlopeDisplayUnits.Add(EPShared.LocalRM.GetString("SlopeUnitPercentage"));
		for (int i = 0; i < m_SlopeDisplayUnits.Count; i++)
		{
			m_SlopeComboBox.Items.Add(m_SlopeDisplayUnits[i]);
		}
		m_CurrentSlopeDisplayUnitType = m_SlopeFormatOptions.LegacyDisplayUnits();
		m_PreviousSlopeDisplayUnitType = m_CurrentSlopeDisplayUnitType;
		m_dCurrentSlopeAccuracy = RevitGeometry.GetSlopeAccuracy(m_CurrentSlopeDisplayUnitType);
		m_SlopeComboBox.SelectedIndex = selectedIndex;
		m_SlopeComboBox.SelectedIndexChanged += OnSlopeUnitsChanged;
		if (m_nSeletedIndex < 0)
		{
			Text = EPShared.LocalRM.GetString("AddDialog");
			FamilyTypeContainer familyTypeContainer = new FamilyTypeContainer();
			switch (m_FamilyType)
			{
			case LandcaddID.Street:
				m_GlobalListView = new QuantumListView();
				SetupGlobalListViewInfoForAdd();
				m_SlopeComboBox.SelectedItem = selectedItem;
				m_StreetTabControl.SelectedIndex = 0;
				m_StreetTabControl.TabIndex = 0;
				m_StreetTabControl.TabPages.Add(EPShared.LocalRM.GetString("StreetInfoTabString"));
				m_StreetTabControl.TabPages.Add(EPShared.LocalRM.GetString("CurbGutterTabString"));
				m_StreetTabControl.TabPages.Add(EPShared.LocalRM.GetString("SlopeProjectionTabString"));
				m_StreetTabControl.TabPages.Add(EPShared.LocalRM.GetString("GeneralParametersTabString"));
				m_AddOrModify = FormForAddOrModify.Add;
				SetupStreetViewInfo(m_StreetTabControl.TabPages[0].Text);
				Text = Text + " " + EPShared.LocalRM.GetString("StreetTypeManagerString") + EPShared.LocalRM.GetString("_TypeString");
				ContextualHelpID = "HID_SDtypeStreetAddInfo";
				break;
			case LandcaddID.Curb:
				Controls.AddRange(new System.Windows.Forms.Control[3] { m_ParameterListView, m_ButtonOk, m_ButtonCancel });
				foreach (Material allMaterial in m_AllMaterialList)
				{
					m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial.Name);
				}
				SetupCurbViewInfo();
				Text = Text + " " + EPShared.LocalRM.GetString("CurbTypeManagerString") + EPShared.LocalRM.GetString("_TypeString");
				ContextualHelpID = "HID_SDtypeCurbAdd";
				break;
			case LandcaddID.Sidewalk:
				Controls.AddRange(new System.Windows.Forms.Control[3] { m_ParameterListView, m_ButtonOk, m_ButtonCancel });
				m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("CenterString"));
				m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("SideString"));
				foreach (Material allMaterial2 in m_AllMaterialList)
				{
					m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial2.Name);
				}
				SetupSideWalkViewInfo();
				Text = Text + " " + EPShared.LocalRM.GetString("SidewalkTypeManagerString") + EPShared.LocalRM.GetString("_TypeString");
				ContextualHelpID = "HID_SDtypeSidewalkAdd";
				break;
			case LandcaddID.RetainingWall:
				Controls.AddRange(new System.Windows.Forms.Control[3] { m_ParameterListView, m_ButtonOk, m_ButtonCancel });
				m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("NoString"));
				m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("YesString"));
				foreach (Material allMaterial3 in m_AllMaterialList)
				{
					m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial3.Name);
				}
				SetupRetainingWallsViewInfo();
				Text = Text + " " + EPShared.LocalRM.GetString("RetainingWallManagerString") + EPShared.LocalRM.GetString("_TypeString");
				ContextualHelpID = "HID_SDtypeRetainingWallAdd";
				break;
			case LandcaddID.Steps:
				Controls.AddRange(new System.Windows.Forms.Control[3] { m_ParameterListView, m_ButtonOk, m_ButtonCancel });
				foreach (Material allMaterial4 in m_AllMaterialList)
				{
					m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial4.Name);
				}
				SetupStepsViewInfo();
				Text = Text + " " + EPShared.LocalRM.GetString("StepsManagerString") + EPShared.LocalRM.GetString("_TypeString");
				break;
			case LandcaddID.ParkingLot:
				Controls.AddRange(new System.Windows.Forms.Control[3] { m_ParameterListView, m_ButtonOk, m_ButtonCancel });
				foreach (Material allMaterial5 in m_AllMaterialList)
				{
					m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial5.Name);
				}
				SetupParkingLotViewInfo();
				Text = Text + " " + EPShared.LocalRM.GetString("ParkingLotManagerString") + EPShared.LocalRM.GetString("_TypeString");
				ContextualHelpID = "HID_SDtypeParkinglotAdd";
				break;
			case LandcaddID.SoftTerrain:
				Controls.AddRange(new System.Windows.Forms.Control[3] { m_ParameterListView, m_ButtonOk, m_ButtonCancel });
				foreach (Material allMaterial6 in m_AllMaterialList)
				{
					m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial6.Name);
				}
				SetupSoftTerrainViewInfo();
				Text = Text + " " + EPShared.LocalRM.GetString("SoftTerrainManagerString") + EPShared.LocalRM.GetString("_TypeString");
				ContextualHelpID = "HID_SDtypeSoftTerrainAdd";
				break;
			}
			return;
		}
		ListViewItem listViewItem = new ListViewItem();
		Text = EPShared.LocalRM.GetString("ModifyDialog");
		switch (m_FamilyType)
		{
		case LandcaddID.PatioWalk:
			Text += " ";
			break;
		case LandcaddID.PlantMix:
			Text += " ";
			break;
		case LandcaddID.Street:
			ContextualHelpID = "HID_SDtypeStreetModifyInfo";
			break;
		case LandcaddID.Curb:
			Text = Text + " " + EPShared.LocalRM.GetString("CurbTypeManagerString") + EPShared.LocalRM.GetString("_TypeString");
			foreach (Material allMaterial7 in m_AllMaterialList)
			{
				m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial7.Name);
			}
			ContextualHelpID = "HID_SDtypeCurbModify";
			break;
		case LandcaddID.Sidewalk:
			m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("CenterString"));
			m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("SideString"));
			Text = Text + " " + EPShared.LocalRM.GetString("SidewalkTypeManagerString") + EPShared.LocalRM.GetString("_TypeString");
			foreach (Material allMaterial8 in m_AllMaterialList)
			{
				m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial8.Name);
			}
			ContextualHelpID = "HID_SDtypeSidewalkModify";
			break;
		case LandcaddID.RetainingWall:
			m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("NoString"));
			m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("YesString"));
			Text = Text + " " + EPShared.LocalRM.GetString("RetainingWallManagerString") + EPShared.LocalRM.GetString("_TypeString");
			foreach (Material allMaterial9 in m_AllMaterialList)
			{
				m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial9.Name);
			}
			ContextualHelpID = "HID_SDtypeRetainingWallModify";
			break;
		case LandcaddID.Steps:
			Text = Text + " " + EPShared.LocalRM.GetString("StepsManagerString") + EPShared.LocalRM.GetString("_TypeString");
			foreach (Material allMaterial10 in m_AllMaterialList)
			{
				m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial10.Name);
			}
			break;
		case LandcaddID.ParkingLot:
			Text = Text + " " + EPShared.LocalRM.GetString("ParkingLotManagerString") + EPShared.LocalRM.GetString("_TypeString");
			foreach (Material allMaterial11 in m_AllMaterialList)
			{
				m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial11.Name);
			}
			ContextualHelpID = "HID_SDtypeParkinglotModify";
			break;
		case LandcaddID.SoftTerrain:
			Text = Text + " " + EPShared.LocalRM.GetString("SoftTerrainManagerString") + EPShared.LocalRM.GetString("_TypeString");
			foreach (Material allMaterial12 in m_AllMaterialList)
			{
				m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial12.Name);
			}
			ContextualHelpID = "HID_SDtypeSoftTerrainModify";
			break;
		}
		int num = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList.FindIndex(CheckTypeNameParameter);
		if (num < 0)
		{
			FamilyParameterWrapper item = new FamilyParameterWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), RevitParameters.TypeNamePlantFamilyType, m_FamilyTypeContainerList[m_nSeletedIndex].FamilyTypeWrapper.FamilyTypeName, StorageType.String);
			m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList.Insert(0, item);
		}
		if (m_FamilyType != LandcaddID.Street)
		{
			for (int j = 0; j < m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList.Count; j++)
			{
				string text = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].DisplayName;
				string text2 = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName;
				string _strToDisplay = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value;
				if (string.Compare(text2, RevitParameters.RegionNameGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(text2, RevitParameters.UniqueIDGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(text2, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (m_FamilyType == LandcaddID.PlantMix)
				{
					if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.PlantSpacingPlantMixFamilyParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble = 1.0;
						if (text2 != string.Empty && _strToDisplay != null)
						{
							dConvertedDouble = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SpacingMethodPlantMixFamilyParameter, ignoreCase: true) == 0)
					{
						_strToDisplay = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value;
						if (_strToDisplay == string.Empty || _strToDisplay == null)
						{
							_strToDisplay = EPShared.LocalRM.GetString("PlantMixSpacingMethodTriangular");
						}
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.PercentagePlantMixFamilyParameter, ignoreCase: true) == 0)
					{
						if (_strToDisplay == null)
						{
							_strToDisplay = "1.0";
						}
						else if (_strToDisplay != null && _strToDisplay == string.Empty)
						{
							_strToDisplay = "1.0";
						}
					}
				}
				else if (m_FamilyType == LandcaddID.PatioWalk)
				{
					if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.ThicknessPathWalkFamilyParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.EdgeWidthPathWalkFamilyParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble2 = 1.0;
						if (text2 != string.Empty && _strToDisplay != null && _strToDisplay != string.Empty)
						{
							dConvertedDouble2 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.EdgePathWalkFamilyParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.MowingStripPathWalkFamilyParameter, ignoreCase: true) == 0)
					{
						_strToDisplay = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value;
						if (_strToDisplay == string.Empty || _strToDisplay == null)
						{
							_strToDisplay = EPShared.LocalRM.GetString("NoString");
						}
						else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value, "0", ignoreCase: true) == 0)
						{
							_strToDisplay = EPShared.LocalRM.GetString("NoString");
						}
						else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value, "1", ignoreCase: true) == 0)
						{
							_strToDisplay = EPShared.LocalRM.GetString("YesString");
						}
					}
				}
				else if (m_FamilyType == LandcaddID.SoftTerrain)
				{
					if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SoftTerrainLeftOffsetWidthParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SoftTerrainRightOffsetWidthParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble3 = 0.0;
						if (text2 != string.Empty && _strToDisplay != null && _strToDisplay != string.Empty)
						{
							dConvertedDouble3 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SoftTerrainCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SoftTerrainFillSlopeParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble4 = 0.0;
						try
						{
							if (!string.IsNullOrEmpty(_strToDisplay))
							{
								dConvertedDouble4 = ValueConversion.ToDouble(_strToDisplay);
							}
						}
						catch (Exception)
						{
							dConvertedDouble4 = 0.0;
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble4, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref _strToDisplay);
					}
				}
				else if (m_FamilyType == LandcaddID.Street)
				{
					if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCurbOffsetParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCurbHeightParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetBoulevardWidthParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble5 = 0.0;
						if (text2 != string.Empty && _strToDisplay != null && _strToDisplay != string.Empty)
						{
							dConvertedDouble5 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble5, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetGutterWidthParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCurbWidthParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble6 = 0.0833333;
						if (text2 != string.Empty && _strToDisplay != null && _strToDisplay != string.Empty)
						{
							dConvertedDouble6 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble6, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCurbDisplayParameter, ignoreCase: true) == 0)
					{
						_strToDisplay = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value;
						if (_strToDisplay == string.Empty || _strToDisplay == null)
						{
							_strToDisplay = EPShared.LocalRM.GetString("NoString");
						}
						else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value, "0", ignoreCase: true) == 0)
						{
							_strToDisplay = EPShared.LocalRM.GetString("NoString");
						}
						else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value, "1", ignoreCase: true) == 0)
						{
							_strToDisplay = EPShared.LocalRM.GetString("YesString");
						}
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCrossSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetGutterSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetFillSlopeParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble7 = 0.0;
						try
						{
							if (!string.IsNullOrEmpty(_strToDisplay))
							{
								dConvertedDouble7 = ValueConversion.ToDouble(_strToDisplay);
							}
						}
						catch (Exception)
						{
							dConvertedDouble7 = 0.0;
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble7, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCurbDisplayParameter, ignoreCase: true) == 0)
					{
						_strToDisplay = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value;
						if (_strToDisplay == string.Empty || _strToDisplay == null)
						{
							_strToDisplay = EPShared.LocalRM.GetString("NoString");
						}
						else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value, "0", ignoreCase: true) == 0)
						{
							_strToDisplay = EPShared.LocalRM.GetString("NoString");
						}
						else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value, "1", ignoreCase: true) == 0)
						{
							_strToDisplay = EPShared.LocalRM.GetString("YesString");
						}
					}
					else if ((string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetPavementSurfaceParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCurbMaterialParameter, ignoreCase: true) == 0) && _strToDisplay != string.Empty)
					{
						string strStringToFind = _strToDisplay;
						IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind));
						List<Material> list = source.ToList();
						_strToDisplay = ((list.Count < 1) ? "Default" : list.ElementAt(0).Name);
					}
				}
				else if (m_FamilyType == LandcaddID.Curb)
				{
					if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCurbHeightParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.CurbGutterHeightParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCurbOffsetParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCurbWidthParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetGutterWidthParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.CurbBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.CurbSubbaseSurfaceParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble8 = 0.0;
						if (text2 != string.Empty && _strToDisplay != null && _strToDisplay != string.Empty)
						{
							dConvertedDouble8 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble8, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetGutterSlopeParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble9 = 0.0;
						try
						{
							if (!string.IsNullOrEmpty(_strToDisplay))
							{
								dConvertedDouble9 = ValueConversion.ToDouble(_strToDisplay);
							}
						}
						catch (Exception)
						{
							dConvertedDouble9 = 0.0;
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble9, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StreetCurbMaterialParameter, ignoreCase: true) == 0 && _strToDisplay != string.Empty)
					{
						string strStringToFind2 = _strToDisplay;
						IEnumerable<Material> source2 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind2));
						List<Material> list2 = source2.ToList();
						_strToDisplay = ((list2.Count < 1) ? "Default" : list2.ElementAt(0).Name);
					}
				}
				else if (m_FamilyType == LandcaddID.Sidewalk)
				{
					if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SideWalkWidthParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble10 = 0.0;
						if (text2 != string.Empty && _strToDisplay != null && _strToDisplay != string.Empty)
						{
							dConvertedDouble10 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble10, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					else
					{
						if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SideWalkLocationLineParameter, ignoreCase: true) == 0)
						{
							continue;
						}
						if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SideWalkPavementDepthParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SideWalkBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SideWalkSubbaseSurfaceParameter, ignoreCase: true) == 0)
						{
							double dConvertedDouble11 = 0.0;
							try
							{
								if (text2 != string.Empty && _strToDisplay != null && _strToDisplay != string.Empty)
								{
									dConvertedDouble11 = ValueConversion.ToDouble(_strToDisplay);
								}
							}
							catch (Exception)
							{
								dConvertedDouble11 = 0.0;
							}
							RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble11, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
						}
						else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SideWalkCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SideWalkFillSlopeParameter, ignoreCase: true) == 0)
						{
							double dConvertedDouble12 = 0.0;
							try
							{
								if (!string.IsNullOrEmpty(_strToDisplay))
								{
									dConvertedDouble12 = ValueConversion.ToDouble(_strToDisplay);
								}
							}
							catch (Exception)
							{
								dConvertedDouble12 = 0.0;
							}
							RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble12, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref _strToDisplay);
						}
						else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SideWalkLocationLineParameter, ignoreCase: true) == 0)
						{
							_strToDisplay = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value;
							if (_strToDisplay == string.Empty || _strToDisplay == null)
							{
								_strToDisplay = EPShared.LocalRM.GetString("CenterString");
							}
							else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value, "0", ignoreCase: true) == 0)
							{
								_strToDisplay = EPShared.LocalRM.GetString("CenterString");
							}
							else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value, "1", ignoreCase: true) == 0)
							{
								_strToDisplay = EPShared.LocalRM.GetString("SideString");
							}
						}
						else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.SideWalkPavementSurfaceParameter, ignoreCase: true) == 0 && _strToDisplay != string.Empty)
						{
							string strStringToFind3 = _strToDisplay;
							IEnumerable<Material> source3 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind3));
							List<Material> list3 = source3.ToList();
							_strToDisplay = ((list3.Count < 1) ? "Default" : list3.ElementAt(0).Name);
						}
					}
				}
				else if (m_FamilyType == LandcaddID.RetainingWall)
				{
					if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.RetainingWallWidthParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.RetainingWallHeightParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.RetainingWallBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.RetainingWallSubbaseSurfaceParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble13 = 0.0;
						if (text2 != string.Empty && _strToDisplay != null && _strToDisplay != string.Empty)
						{
							dConvertedDouble13 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble13, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.RetainingWallCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.RetainingWallFillSlopeParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble14 = 0.0;
						try
						{
							if (!string.IsNullOrEmpty(_strToDisplay))
							{
								dConvertedDouble14 = ValueConversion.ToDouble(_strToDisplay);
							}
						}
						catch (Exception)
						{
							dConvertedDouble14 = 0.0;
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble14, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.RetainingWallSlopeProjectionParameter, ignoreCase: true) == 0)
					{
						_strToDisplay = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value;
						if (_strToDisplay == string.Empty || _strToDisplay == null)
						{
							_strToDisplay = EPShared.LocalRM.GetString("NoString");
						}
						else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value, "0", ignoreCase: true) == 0)
						{
							_strToDisplay = EPShared.LocalRM.GetString("NoString");
						}
						else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].Value, "1", ignoreCase: true) == 0)
						{
							_strToDisplay = EPShared.LocalRM.GetString("YesString");
						}
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.RetainingWallMaterialParameter, ignoreCase: true) == 0 && _strToDisplay != string.Empty)
					{
						string strStringToFind4 = _strToDisplay;
						IEnumerable<Material> source4 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind4));
						List<Material> list4 = source4.ToList();
						_strToDisplay = ((list4.Count < 1) ? "Default" : list4.ElementAt(0).Name);
					}
				}
				else if (m_FamilyType == LandcaddID.Steps)
				{
					if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StepsHeightParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StepsTreadParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StepsWidthParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble15 = 0.0;
						if (text2 != string.Empty && _strToDisplay != null && _strToDisplay != string.Empty)
						{
							dConvertedDouble15 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble15, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.StepsMaterialParameter, ignoreCase: true) == 0 && _strToDisplay != string.Empty)
					{
						string strStringToFind5 = _strToDisplay;
						IEnumerable<Material> source5 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind5));
						List<Material> list5 = source5.ToList();
						_strToDisplay = ((list5.Count < 1) ? "Default" : list5.ElementAt(0).Name);
					}
				}
				else if (m_FamilyType == LandcaddID.ParkingLot)
				{
					if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.ParkingLotPavementDepthParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.ParkingLotParkingBaseDepthParameter, ignoreCase: true) == 0 || string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.ParkingLotParkingSubBaseDepthParameter, ignoreCase: true) == 0)
					{
						double dConvertedDouble16 = 0.0;
						if (text2 != string.Empty && _strToDisplay != null && _strToDisplay != string.Empty)
						{
							dConvertedDouble16 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble16, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[j].ParameterName, RevitParameters.ParkingLotParkingPavementSurfaceParameter, ignoreCase: true) == 0 && _strToDisplay != string.Empty)
					{
						string strStringToFind6 = _strToDisplay;
						IEnumerable<Material> source6 = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind6));
						List<Material> list6 = source6.ToList();
						_strToDisplay = ((list6.Count < 1) ? "Default" : list6.ElementAt(0).Name);
					}
				}
				if (string.Compare(text2, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
				{
					string text3 = text2;
					text2 = RevitParameters.Slope_ + text3;
					text3 = text;
					text = EPShared.LocalRM.GetString("Slope_") + text3;
					try
					{
						double dConvertedDouble17 = 0.0;
						if (_strToDisplay != null && _strToDisplay != string.Empty)
						{
							dConvertedDouble17 = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble17, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = m_strLinearIncrement;
					}
				}
				else if (string.Compare(text2, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
				{
					string text4 = text2;
					text2 = RevitParameters.Slope_ + text4;
					text4 = text;
					text = EPShared.LocalRM.GetString("Slope_") + text4;
					try
					{
						double dAngle = 0.0;
						if (!string.IsNullOrEmpty(_strToDisplay))
						{
							dAngle = ValueConversion.ToDouble(_strToDisplay);
						}
						RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strToDisplay);
					}
					catch (Exception)
					{
						_strToDisplay = m_strCircularIncrement;
					}
				}
				listViewItem = m_ParameterListView.Items.Add(text2, text, string.Empty);
				if (_strToDisplay == null)
				{
					_strToDisplay = string.Empty;
				}
				listViewItem.SubItems.Add(_strToDisplay);
				listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = _strToDisplay;
			}
		}
		else
		{
			m_GlobalListView = new QuantumListView();
			SetupGlobalListViewInfoForModify();
			m_StreetTabControl.TabPages.Add(EPShared.LocalRM.GetString("StreetInfoTabString"));
			m_StreetTabControl.TabPages.Add(EPShared.LocalRM.GetString("CurbGutterTabString"));
			m_StreetTabControl.TabPages.Add(EPShared.LocalRM.GetString("SlopeProjectionTabString"));
			m_StreetTabControl.TabPages.Add(EPShared.LocalRM.GetString("GeneralParametersTabString"));
			Text = Text + " " + EPShared.LocalRM.GetString("StreetTypeManagerString") + EPShared.LocalRM.GetString("_TypeString");
			m_AddOrModify = FormForAddOrModify.Modify;
			SetupStreetViewInfo(m_StreetTabControl.TabPages[0].Text);
		}
	}

	private void OnSlopeUnitsChanged(object sender, EventArgs e)
	{
		m_CurrentSlopeDisplayUnitType = m_SlopeUnits[m_SlopeComboBox.SelectedIndex];
		m_dCurrentSlopeAccuracy = RevitGeometry.GetSlopeAccuracy(m_CurrentSlopeDisplayUnitType);
		FindIndexInListView();
		m_PreviousSlopeDisplayUnitType = m_CurrentSlopeDisplayUnitType;
	}

	private void FindIndexInListView()
	{
		List<string> list = new List<string>();
		switch (m_FamilyType)
		{
		case LandcaddID.Street:
			list.Add(EPShared.LocalRM.GetString("StreetCrossSlopeParameter"));
			list.Add(EPShared.LocalRM.GetString("StreetCutSlopeParameter"));
			list.Add(EPShared.LocalRM.GetString("StreetFillSlopeParameter"));
			list.Add(EPShared.LocalRM.GetString("StreetGutterSlopeParameter"));
			break;
		case LandcaddID.Curb:
			list.Add(EPShared.LocalRM.GetString("StreetGutterSlopeParameter"));
			break;
		case LandcaddID.Sidewalk:
			list.Add(EPShared.LocalRM.GetString("SideWalkCutSlopeParameter"));
			list.Add(EPShared.LocalRM.GetString("SideWalkFillSlopeParameter"));
			break;
		case LandcaddID.RetainingWall:
			list.Add(EPShared.LocalRM.GetString("RetainingWallCutSlopeParameter"));
			list.Add(EPShared.LocalRM.GetString("RetainingWallFillSlopeParameter"));
			break;
		case LandcaddID.SoftTerrain:
			list.Add(EPShared.LocalRM.GetString("SoftTerrainCutSlopeParameter"));
			list.Add(EPShared.LocalRM.GetString("SoftTerrainFillSlopeParameter"));
			break;
		}
		foreach (string item in list)
		{
			for (int i = 0; i < m_ParameterListView.Items.Count; i++)
			{
				string strA = m_ParameterListView.Items[i].SubItems[0].Text;
				string text = m_ParameterListView.Items[i].SubItems[1].Text;
				if (string.Compare(strA, item, ignoreCase: true) == 0)
				{
					HandleSlopes(i);
				}
			}
		}
	}

	private void SetupDlg(out int nListViewColumnWidth, out int nTotalListViewHeight, out int nTotalDlgHeight)
	{
		int num = 21;
		int num2 = 15;
		int num3 = 17;
		int num4 = 23;
		nListViewColumnWidth = 215;
		int count = m_ParameterList.Count;
		int num5 = 10;
		int num6 = 10;
		if (Environment.OSVersion.Version.Major == 5)
		{
			num2 = 15;
			num3 = 17;
		}
		else if (Environment.OSVersion.Version.Major == 6)
		{
			if (Environment.OSVersion.Version.Minor == 0)
			{
				num2 = 18;
				num3 = 24;
			}
			else
			{
				num2 = 21;
				num3 = 24;
			}
		}
		nTotalListViewHeight = num3 + num2 * count;
		nTotalDlgHeight = num3 + nTotalListViewHeight + num4 + num * 3;
		if (m_bPreview)
		{
			Image image = null;
			switch (m_FamilyType)
			{
			case LandcaddID.Street:
				image = Resources.StreetInfo;
				break;
			case LandcaddID.Curb:
				image = Resources.CurbInfo;
				break;
			case LandcaddID.Sidewalk:
				image = Resources.SidewalkInfo;
				break;
			case LandcaddID.RetainingWall:
				image = Resources.RetainingWallInfo;
				break;
			case LandcaddID.Steps:
				image = Resources.StreetInfo;
				break;
			case LandcaddID.ParkingLot:
				image = Resources.ParkingLotInfo;
				break;
			case LandcaddID.SoftTerrain:
				image = Resources.SoftTerrainInfo;
				break;
			}
			m_PreviewBox = new PictureBox();
			m_PreviewBox.Location = new System.Drawing.Point(num5, num6);
			m_PreviewBox.Width = nListViewColumnWidth;
			m_PreviewBox.SizeMode = PictureBoxSizeMode.AutoSize;
			m_PreviewBox.Image = image;
			m_PreviewBox.BorderStyle = BorderStyle.Fixed3D;
			m_PreviewBox.BackColor = System.Drawing.Color.White;
			m_PreviewBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			Controls.AddRange(new System.Windows.Forms.Control[1] { m_PreviewBox });
			if (nTotalListViewHeight < m_PreviewBox.Height)
			{
				nTotalListViewHeight = m_PreviewBox.Height;
			}
			m_ParameterListView = new QuantumListView(nListViewColumnWidth * 2, nTotalListViewHeight);
			m_ParameterListView.Location = new System.Drawing.Point(m_PreviewBox.Width + 2 * num5, num6);
			m_ParameterListView.View = System.Windows.Forms.View.Details;
			m_ParameterListView.Columns.Add(EPShared.LocalRM.GetString("Parameter"));
			m_ParameterListView.Columns.Add(EPShared.LocalRM.GetString("Value"));
			m_ParameterListView.Columns[0].Width = nListViewColumnWidth - 1;
			m_ParameterListView.Columns[1].Width = nListViewColumnWidth - 1;
			m_PreviewBox.Height = m_ParameterListView.Height;
			m_UnitLabel.Location = new System.Drawing.Point(m_PreviewBox.Location.X, num6 + m_ParameterListView.Height + 7);
			m_ButtonOk.Location = new System.Drawing.Point(m_PreviewBox.Width + num5 * 2 + 260, num6 + m_ParameterListView.Height + 7);
			m_ButtonCancel.Location = new System.Drawing.Point(m_PreviewBox.Width + num5 * 2 + 340, num6 + m_ParameterListView.Height + 7);
			m_UnitLabel.Height = m_ButtonOk.Height;
			m_UnitLabel.Width = 150;
			m_SlopeComboBox.Location = new System.Drawing.Point(m_UnitLabel.Location.X + m_UnitLabel.Width + 5, m_UnitLabel.Location.Y);
			m_SlopeComboBox.Height = m_UnitLabel.Height;
			m_SlopeComboBox.Width = m_UnitLabel.Width;
			nTotalDlgHeight = m_ParameterListView.Height + m_ButtonCancel.Height + num * 3;
			Height = nTotalDlgHeight;
			Width = m_PreviewBox.Width + 3 * num5 + nListViewColumnWidth * 2;
			Size = new Size(m_PreviewBox.Width + 3 * num5 + nListViewColumnWidth * 2, nTotalDlgHeight);
			MinimumSize = new Size(Width, Height);
			Refresh();
		}
		else
		{
			m_ParameterListView = new QuantumListView(nListViewColumnWidth * 2, nTotalListViewHeight);
			m_ParameterListView.View = System.Windows.Forms.View.Details;
			m_ParameterListView.Columns.Add(EPShared.LocalRM.GetString("Parameter"));
			m_ParameterListView.Columns.Add(EPShared.LocalRM.GetString("Value"));
			m_ParameterListView.Columns[0].Width = nListViewColumnWidth - 1;
			m_ParameterListView.Columns[1].Width = nListViewColumnWidth - 1;
			m_UnitLabel.Location = new System.Drawing.Point(m_PreviewBox.Location.X, m_ParameterListView.Height + num);
			m_ButtonOk.Location = new System.Drawing.Point(m_ParameterListView.Width - m_ButtonOk.Width - num5 - m_ButtonCancel.Width, m_ParameterListView.Height + num);
			m_ButtonCancel.Location = new System.Drawing.Point(m_ParameterListView.Width - m_ButtonCancel.Width, m_ParameterListView.Height + num);
			m_UnitLabel.Height = m_ButtonOk.Height;
			m_UnitLabel.Width = 150;
			m_SlopeComboBox.Location = new System.Drawing.Point(m_UnitLabel.Location.X + m_UnitLabel.Width + 5, m_UnitLabel.Location.Y);
			m_SlopeComboBox.Height = m_UnitLabel.Height;
			m_SlopeComboBox.Width = m_UnitLabel.Width;
			nTotalDlgHeight = m_ParameterListView.Height + m_ButtonCancel.Height + num * 3;
			Height = nTotalDlgHeight;
			Width = nListViewColumnWidth * 2 + num;
			MinimumSize = new Size(Width, Height);
		}
		Controls.AddRange(new System.Windows.Forms.Control[5] { m_ParameterListView, m_UnitLabel, m_ButtonOk, m_ButtonCancel, m_SlopeComboBox });
	}

	private void SetupStreetDlg(out int nListViewColumnWidth, out int nListViewTotalHeight, out int nTotalDlgHeight)
	{
		int num = 21;
		int num2 = 15;
		int num3 = 17;
		int num4 = 23;
		nListViewColumnWidth = 215;
		int num5 = 10;
		int num6 = 10;
		int num7 = 10;
		if (Environment.OSVersion.Version.Major == 5)
		{
			num2 = 15;
			num3 = 17;
			num5 = 12;
		}
		else if (Environment.OSVersion.Version.Major == 6)
		{
			if (Environment.OSVersion.Version.Minor == 0)
			{
				num2 = 18;
				num3 = 24;
			}
			else
			{
				num2 = 21;
				num3 = 24;
			}
		}
		nListViewTotalHeight = num3 + num2 * num5;
		nTotalDlgHeight = num3 + nListViewTotalHeight + num4 + num * 3;
		if (m_bPreview)
		{
			Image streetInfo = Resources.StreetInfo;
			m_PreviewBox = new PictureBox();
			m_PreviewBox.Location = new System.Drawing.Point(num6, num7);
			m_PreviewBox.Width = nListViewColumnWidth;
			m_PreviewBox.SizeMode = PictureBoxSizeMode.AutoSize;
			m_PreviewBox.Image = streetInfo;
			m_PreviewBox.BorderStyle = BorderStyle.Fixed3D;
			m_PreviewBox.BackColor = System.Drawing.Color.White;
			m_PreviewBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			Controls.AddRange(new System.Windows.Forms.Control[1] { m_PreviewBox });
			m_ParameterListView = new QuantumListView(nListViewColumnWidth * 2, nListViewTotalHeight);
			m_ParameterListView.View = System.Windows.Forms.View.Details;
			m_ParameterListView.Columns.Add(EPShared.LocalRM.GetString("Parameter"));
			m_ParameterListView.Columns.Add(EPShared.LocalRM.GetString("Value"));
			m_ParameterListView.Columns[0].Width = nListViewColumnWidth - 1;
			m_ParameterListView.Columns[1].Width = nListViewColumnWidth - 1;
			m_StreetTabControl.Location = new System.Drawing.Point(m_PreviewBox.Width + 2 * num6, num7);
			m_StreetTabControl.Size = new Size(nListViewColumnWidth * 2 + 30, m_ParameterListView.Height + 40);
			m_PreviewBox.Height = m_StreetTabControl.Height - 2;
			m_UnitLabel.Location = new System.Drawing.Point(m_PreviewBox.Location.X, num7 + m_StreetTabControl.Height + 7);
			m_ButtonOk.Location = new System.Drawing.Point(m_PreviewBox.Width + num6 * 2 + 300, num7 + m_StreetTabControl.Height + 7);
			m_ButtonCancel.Location = new System.Drawing.Point(m_PreviewBox.Width + num6 * 2 + 381, num7 + m_StreetTabControl.Height + 7);
			m_UnitLabel.Height = m_ButtonOk.Height;
			m_UnitLabel.Width = 150;
			m_SlopeComboBox.Location = new System.Drawing.Point(m_UnitLabel.Location.X + m_UnitLabel.Width + 5, m_UnitLabel.Location.Y);
			m_SlopeComboBox.Height = m_UnitLabel.Height;
			m_SlopeComboBox.Width = m_UnitLabel.Width;
			nTotalDlgHeight = m_StreetTabControl.Height + m_ButtonCancel.Height + num * 3;
			Height = nTotalDlgHeight;
			Width = m_PreviewBox.Width + 2 * num6 + nListViewColumnWidth * 2 + 43;
			MinimumSize = new Size(Width, Height);
		}
		else
		{
			m_ParameterListView = new QuantumListView(nListViewColumnWidth * 2, nListViewTotalHeight);
			m_ParameterListView.View = System.Windows.Forms.View.Details;
			m_ParameterListView.Columns.Add(EPShared.LocalRM.GetString("Parameter"));
			m_ParameterListView.Columns.Add(EPShared.LocalRM.GetString("Value"));
			m_ParameterListView.Columns[0].Width = nListViewColumnWidth - 1;
			m_ParameterListView.Columns[1].Width = nListViewColumnWidth - 1;
			m_StreetTabControl.Location = new System.Drawing.Point(3, 3);
			m_StreetTabControl.Size = new Size(nListViewColumnWidth * 2 + 30, m_ParameterListView.Height + 40);
			m_UnitLabel.Location = new System.Drawing.Point(m_StreetTabControl.Location.X, m_StreetTabControl.Height + num);
			m_ButtonOk.Location = new System.Drawing.Point(299, m_StreetTabControl.Height + num);
			m_ButtonCancel.Location = new System.Drawing.Point(380, m_StreetTabControl.Height + num);
			m_UnitLabel.Height = m_ButtonOk.Height;
			m_UnitLabel.Width = 150;
			m_SlopeComboBox.Location = new System.Drawing.Point(m_UnitLabel.Location.X + m_UnitLabel.Width + 5, m_UnitLabel.Location.Y);
			m_SlopeComboBox.Height = m_UnitLabel.Height;
			m_SlopeComboBox.Width = m_UnitLabel.Width;
			nTotalDlgHeight = m_StreetTabControl.Height + m_ButtonCancel.Height + num * 3;
			Height = nTotalDlgHeight;
			Width = nListViewColumnWidth * 2 + 43;
			MinimumSize = new Size(Width, Height);
		}
		Controls.AddRange(new System.Windows.Forms.Control[5] { m_StreetTabControl, m_UnitLabel, m_ButtonOk, m_ButtonCancel, m_SlopeComboBox });
	}

	private void StreetTabControlTabIndexChanged(object sender, EventArgs e)
	{
		SetupStreetViewInfo(m_StreetTabControl.SelectedTab.Text);
		UpdateCurrentTabListView();
		FindIndexInListView();
	}

	private static bool CheckTypeNameParameter(FamilyParameterWrapper _CurrParameter)
	{
		if (string.Compare(_CurrParameter.ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
		{
			return true;
		}
		return false;
	}

	private void SetupGlobalListViewInfoForAdd()
	{
		string empty = string.Empty;
		ListViewItem listViewItem = new ListViewItem();
		for (int i = 0; i < m_ParameterList.Count; i++)
		{
			string text = m_ParameterList[i].ParameterDisplayName;
			string text2 = m_ParameterList[i].ParameterName;
			empty = "";
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
			{
				continue;
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
			{
				empty = EPShared.LocalRM.GetString("NewStreet");
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetNumberOfLanesParameter, ignoreCase: true) == 0)
			{
				empty = 2.ToString();
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetLaneWidthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble = 15.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetPavementDepthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble2 = 0.5;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetBaseSurfaceParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble3 = 2.0 / 3.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetSubbaseSurfaceParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble4 = 1.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble4, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetGutterSlopeParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble5 = 0.03;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble5, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCrossSlopeParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble6 = 0.02;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble6, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetFillSlopeParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble7 = 0.333;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble7, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetPavementSurfaceParameter, ignoreCase: true) == 0)
			{
				empty = "Default";
				string strStringToFind = empty;
				IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strStringToFind, ignoreCase: true) == 0);
				List<Material> list = source.ToList();
				empty = ((list.Count < 1) ? "" : list.ElementAt(0).Name);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbMaterialParameter, ignoreCase: true) == 0)
			{
				empty = "Default";
				string strStringToFind2 = empty;
				IEnumerable<Material> source2 = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strStringToFind2, ignoreCase: true) == 0);
				List<Material> list2 = source2.ToList();
				empty = ((list2.Count < 1) ? "" : list2.ElementAt(0).Name);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbDisplayParameter, ignoreCase: true) == 0)
			{
				empty = "No";
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.AssemblyCodeGenericFamilyParameter, ignoreCase: true) == 0)
			{
				empty = "";
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbHeightParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble8 = 0.5;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble8, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetBoulevardWidthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble9 = 7.5;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble9, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbOffsetParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble10 = 1.0 / 12.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble10, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetGutterWidthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble11 = 2.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble11, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbWidthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble12 = 5.0 / 12.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble12, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble13 = 0.0;
				empty = m_strLinearIncrement;
				if (empty != null && empty != string.Empty)
				{
					dConvertedDouble13 = ValueConversion.ToDouble(empty);
				}
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble13, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				empty = m_strCircularIncrement;
				double dAngle = 0.15707963249999998;
				RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref empty);
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				string text3 = text2;
				text2 = RevitParameters.Slope_ + text3;
				text3 = text;
				text = EPShared.LocalRM.GetString("Slope_") + text3;
			}
			listViewItem = m_GlobalListView.Items.Add(text2, text, string.Empty);
			listViewItem.SubItems.Add(empty);
			listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = empty;
		}
	}

	private void SetupGlobalListViewInfoForModify()
	{
		ListViewItem listViewItem = new ListViewItem();
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		for (int i = 0; i < m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList.Count; i++)
		{
			empty = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[i].DisplayName;
			empty2 = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[i].ParameterName;
			empty3 = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[i].Value;
			if (string.Compare(empty2, RevitParameters.RegionNameGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.UniqueIDGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
			{
				continue;
			}
			if (string.Compare(empty2, RevitParameters.StreetGutterWidthParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetLaneWidthParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetPavementDepthParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetCurbOffsetParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetCurbWidthParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetCurbHeightParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetBoulevardWidthParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetBaseSurfaceParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetSubbaseSurfaceParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble = 0.0;
				if (empty2 != string.Empty && empty3 != null && empty3 != string.Empty)
				{
					dConvertedDouble = ValueConversion.ToDouble(empty3);
				}
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty3);
			}
			else if (string.Compare(empty2, RevitParameters.StreetCrossSlopeParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetGutterSlopeParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetFillSlopeParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble2 = 0.0;
				try
				{
					if (!string.IsNullOrEmpty(empty3))
					{
						dConvertedDouble2 = ValueConversion.ToDouble(empty3);
					}
				}
				catch (Exception)
				{
					dConvertedDouble2 = 0.0;
				}
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref empty3);
			}
			else if (string.Compare(empty2, RevitParameters.StreetPavementSurfaceParameter, ignoreCase: true) == 0 || string.Compare(empty2, RevitParameters.StreetCurbMaterialParameter, ignoreCase: true) == 0)
			{
				if (empty3 != string.Empty)
				{
					string strStringToFind = empty3;
					IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => SelectedMaterial.Id.Value64() == Convert.ToInt64(strStringToFind));
					List<Material> list = source.ToList();
					empty3 = ((list.Count < 1) ? "Default" : list.ElementAt(0).Name);
				}
			}
			else if (string.Compare(empty2, RevitParameters.StreetCurbDisplayParameter, ignoreCase: true) == 0)
			{
				empty3 = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[i].Value;
				if (empty3 == string.Empty || empty3 == null)
				{
					empty3 = EPShared.LocalRM.GetString("NoString");
				}
				else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[i].Value, "0", ignoreCase: true) == 0)
				{
					empty3 = EPShared.LocalRM.GetString("NoString");
				}
				else if (string.Compare(m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[i].Value, "1", ignoreCase: true) == 0)
				{
					empty3 = EPShared.LocalRM.GetString("YesString");
				}
			}
			else if (string.Compare(empty2, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
			{
				string text = empty2;
				empty2 = RevitParameters.Slope_ + text;
				text = empty;
				empty = EPShared.LocalRM.GetString("Slope_") + text;
				try
				{
					double dConvertedDouble3 = 0.0;
					if (empty3 != null && empty3 != string.Empty)
					{
						dConvertedDouble3 = ValueConversion.ToDouble(empty3);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty3);
				}
				catch (Exception)
				{
					empty3 = m_strLinearIncrement;
				}
			}
			else if (string.Compare(empty2, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				string text2 = empty2;
				empty2 = RevitParameters.Slope_ + text2;
				text2 = empty;
				empty = EPShared.LocalRM.GetString("Slope_") + text2;
				try
				{
					double dAngle = 0.0;
					if (!string.IsNullOrEmpty(empty3))
					{
						dAngle = ValueConversion.ToDouble(empty3);
					}
					RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref empty3);
				}
				catch (Exception)
				{
					empty3 = m_strCircularIncrement;
				}
			}
			listViewItem = m_GlobalListView.Items.Add(empty2, empty, string.Empty);
			if (empty3 == null)
			{
				empty3 = string.Empty;
			}
			listViewItem.SubItems.Add(empty3);
			listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = empty3;
		}
	}

	private void SetupStreetViewInfo(string _strCurrentTabText)
	{
		UpdateGlobalListView();
		if (m_StreetTabControl.SelectedTab.Controls.Count > 0)
		{
			m_StreetTabControl.SelectedTab.Controls.RemoveAt(0);
		}
		m_ParameterList.Clear();
		if (string.Compare(_strCurrentTabText, EPShared.LocalRM.GetString("StreetInfoTabString"), ignoreCase: true) == 0)
		{
			switch (m_AddOrModify)
			{
			case FormForAddOrModify.Add:
				ContextualHelpID = "HID_SDtypeStreetAddInfo";
				break;
			case FormForAddOrModify.Modify:
				ContextualHelpID = "HID_SDtypeStreetModifyInfo";
				break;
			}
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), RevitParameters.TypeNamePlantFamilyType, ParameterType.Text));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetNumberOfLanesParameter"), RevitParameters.StreetNumberOfLanesParameter, ParameterType.Integer));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetLaneWidthParameter"), RevitParameters.StreetLaneWidthParameter, ParameterType.Length));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetPavementDepthParameter"), RevitParameters.StreetPavementDepthParameter, ParameterType.Length));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCrossSlopeParameter"), RevitParameters.StreetCrossSlopeParameter, ParameterType.Slope));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetPavementSurfaceParameter"), RevitParameters.StreetPavementSurfaceParameter, ParameterType.Integer));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCurbDisplayParameter"), RevitParameters.StreetCurbDisplayParameter, ParameterType.YesNo));
		}
		else if (string.Compare(_strCurrentTabText, EPShared.LocalRM.GetString("CurbGutterTabString"), ignoreCase: true) == 0)
		{
			switch (m_AddOrModify)
			{
			case FormForAddOrModify.Add:
				ContextualHelpID = "HID_SDtypeStreetAddCurbGutter";
				break;
			case FormForAddOrModify.Modify:
				ContextualHelpID = "HID_SDtypeStreetModifyCurbGutter";
				break;
			}
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetGutterWidthParameter"), RevitParameters.StreetGutterWidthParameter, ParameterType.Length));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetGutterSlopeParameter"), RevitParameters.StreetGutterSlopeParameter, ParameterType.Slope));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCurbOffsetParameter"), RevitParameters.StreetCurbOffsetParameter, ParameterType.Length));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCurbWidthParameter"), RevitParameters.StreetCurbWidthParameter, ParameterType.Length));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCurbHeightParameter"), RevitParameters.StreetCurbHeightParameter, ParameterType.Length));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCurbMaterialParameter"), RevitParameters.StreetCurbMaterialParameter, ParameterType.Integer));
		}
		else if (string.Compare(_strCurrentTabText, EPShared.LocalRM.GetString("SlopeProjectionTabString"), ignoreCase: true) == 0)
		{
			switch (m_AddOrModify)
			{
			case FormForAddOrModify.Add:
				ContextualHelpID = "HID_SDtypeStreetAddSlope";
				break;
			case FormForAddOrModify.Modify:
				ContextualHelpID = "HID_SDtypeStreetModifySlope";
				break;
			}
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetBoulevardWidthParameter"), RevitParameters.StreetBoulevardWidthParameter, ParameterType.Length));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetCutSlopeParameter"), RevitParameters.StreetCutSlopeParameter, ParameterType.Slope));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetFillSlopeParameter"), RevitParameters.StreetFillSlopeParameter, ParameterType.Slope));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("LinearIncrementParameter"), RevitParameters.LinearIncrementParameter, ParameterType.Length));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("CurveIncrementParameter"), RevitParameters.CurveIncrementParameter, ParameterType.Angle));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetBaseSurfaceParameter"), RevitParameters.StreetBaseSurfaceParameter, ParameterType.Length));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("StreetSubbaseSurfaceParameter"), RevitParameters.StreetSubbaseSurfaceParameter, ParameterType.Length));
		}
		else if (string.Compare(_strCurrentTabText, EPShared.LocalRM.GetString("GeneralParametersTabString"), ignoreCase: true) == 0)
		{
			switch (m_AddOrModify)
			{
			case FormForAddOrModify.Add:
				ContextualHelpID = "HID_SDtypeStreetAddGeneralParams";
				break;
			case FormForAddOrModify.Modify:
				ContextualHelpID = "HID_SDtypeStreetModifyGeneralParams";
				break;
			}
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("KeynoteGenericFamilyParameter"), RevitParameters.KeynoteGenericFamilyParameter, ParameterType.Text, BuiltInParameter.KEYNOTE_PARAM));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ModelGenericFamilyParameter"), RevitParameters.ModelGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_MODEL));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("ManufacturerGenericFamilyParameter"), RevitParameters.ManufacturerGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_MANUFACTURER));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("TypeCommentsGenericFamilyParameter"), RevitParameters.TypeCommentsGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_TYPE_COMMENTS));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("URLGenericFamilyParameter"), RevitParameters.URLGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_URL));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("DescriptionGenericFamilyParameter"), RevitParameters.DescriptionGenericFamilyParameter, ParameterType.Text, BuiltInParameter.ALL_MODEL_DESCRIPTION));
			m_ParameterList.Add(new FamilyParameterTypeWrapper(EPShared.LocalRM.GetString("CostGenericFamilyParameter"), RevitParameters.CostGenericFamilyParameter, ParameterType.Currency, BuiltInParameter.DOOR_COST));
		}
		SetupStreetDlg(out var _, out var _, out var _);
		m_ParameterListView.OnListViewItemChanged += ParameterListViewOnListViewItemChanged;
		foreach (Material allMaterial in m_AllMaterialList)
		{
			m_ParameterListView.MaterialCmbBox.Items.Add(allMaterial.Name);
		}
		m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("NoString"));
		m_ParameterListView.CmbBox.Items.Add(EPShared.LocalRM.GetString("YesString"));
		string empty = string.Empty;
		ListViewItem listViewItem = new ListViewItem();
		if (m_nSeletedIndex < 0)
		{
			for (int i = 0; i < m_ParameterList.Count; i++)
			{
				string text = m_ParameterList[i].ParameterDisplayName;
				string text2 = m_ParameterList[i].ParameterName;
				empty = "";
				if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
				{
					empty = EPShared.LocalRM.GetString("NewStreet");
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetNumberOfLanesParameter, ignoreCase: true) == 0)
				{
					empty = 2.ToString();
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetLaneWidthParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble = 15.0;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetPavementDepthParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble2 = 0.5;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetBaseSurfaceParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble3 = 2.0 / 3.0;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetSubbaseSurfaceParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble4 = 1.0;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble4, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetGutterSlopeParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble5 = 0.03;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble5, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCrossSlopeParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble6 = 0.02;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble6, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetFillSlopeParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble7 = 0.333;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble7, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbDisplayParameter, ignoreCase: true) == 0)
				{
					empty = "Yes";
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetPavementSurfaceParameter, ignoreCase: true) == 0)
				{
					empty = "Default";
					string strStringToFind = empty;
					IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strStringToFind, ignoreCase: true) == 0);
					List<Material> list = source.ToList();
					empty = ((list.Count < 1) ? "" : list.ElementAt(0).Name);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbMaterialParameter, ignoreCase: true) == 0)
				{
					empty = "Default";
					string strStringToFind2 = empty;
					IEnumerable<Material> source2 = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strStringToFind2, ignoreCase: true) == 0);
					List<Material> list2 = source2.ToList();
					empty = ((list2.Count < 1) ? "" : list2.ElementAt(0).Name);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.AssemblyCodeGenericFamilyParameter, ignoreCase: true) == 0)
				{
					empty = "";
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbHeightParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble8 = 0.5;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble8, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetBoulevardWidthParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble9 = 7.5;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble9, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetGutterWidthParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble10 = 2.0;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble10, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbWidthParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble11 = 5.0 / 12.0;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble11, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbOffsetParameter, ignoreCase: true) == 0)
				{
					double dConvertedDouble12 = 1.0 / 12.0;
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble12, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
				{
					empty = m_strLinearIncrement;
					try
					{
						double dConvertedDouble13 = 0.0;
						if (empty != null && empty != string.Empty)
						{
							dConvertedDouble13 = ValueConversion.ToDouble(empty);
						}
						RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble13, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref empty);
					}
					catch (Exception)
					{
						empty = m_strLinearIncrement;
					}
				}
				else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
				{
					empty = m_strCircularIncrement;
					try
					{
						double dAngle = 0.15707963249999998;
						RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref empty);
					}
					catch (Exception)
					{
						empty = m_strCircularIncrement;
					}
				}
				if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
				{
					string text3 = text2;
					text2 = RevitParameters.Slope_ + text3;
					text3 = text;
					text = EPShared.LocalRM.GetString("Slope_") + text3;
				}
				listViewItem = m_ParameterListView.Items.Add(text2, text, string.Empty);
				listViewItem.SubItems.Add(empty);
				listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = empty;
			}
		}
		else
		{
			for (int num = 0; num < m_ParameterList.Count; num++)
			{
				string text = m_ParameterList[num].ParameterDisplayName;
				string text2 = m_ParameterList[num].ParameterName;
				empty = "";
				if (string.Compare(m_ParameterList[num].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[num].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
				{
					string text4 = text2;
					text2 = RevitParameters.Slope_ + text4;
					text4 = text;
					text = EPShared.LocalRM.GetString("Slope_") + text4;
				}
				if (string.Compare(m_ParameterList[num].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
				{
					continue;
				}
				string empty2 = string.Empty;
				string empty3 = string.Empty;
				for (int num2 = 0; num2 < m_GlobalListView.Items.Count; num2++)
				{
					empty2 = m_GlobalListView.Items[num2].SubItems[0].Text;
					empty3 = m_GlobalListView.Items[num2].SubItems[1].Text;
					if (string.Compare(empty2, text, ignoreCase: false) == 0)
					{
						empty = empty3;
						break;
					}
				}
				listViewItem = m_ParameterListView.Items.Add(text2, text, string.Empty);
				listViewItem.SubItems.Add(empty);
				listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = empty;
			}
		}
		m_StreetTabControl.SelectedTab.Controls.AddRange(new System.Windows.Forms.Control[1] { m_ParameterListView });
	}

	private void UpdateGlobalListView()
	{
		if (m_ParameterListView == null)
		{
			return;
		}
		string empty = string.Empty;
		string empty2 = string.Empty;
		for (int i = 0; i < m_ParameterListView.Items.Count; i++)
		{
			empty = m_ParameterListView.Items[i].SubItems[0].Text;
			empty2 = m_ParameterListView.Items[i].SubItems[1].Text;
			for (int j = 0; j < m_GlobalListView.Items.Count; j++)
			{
				if (string.Compare(m_GlobalListView.Items[j].SubItems[0].Text, empty, ignoreCase: false) == 0)
				{
					m_GlobalListView.Items[j].SubItems[1].Text = empty2;
					break;
				}
			}
		}
	}

	private void UpdateCurrentTabListView()
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		for (int i = 0; i < m_GlobalListView.Items.Count; i++)
		{
			empty = m_GlobalListView.Items[i].SubItems[0].Text;
			empty2 = m_GlobalListView.Items[i].SubItems[1].Text;
			for (int j = 0; j < m_ParameterListView.Items.Count; j++)
			{
				if (string.Compare(m_ParameterListView.Items[j].SubItems[0].Text, empty, ignoreCase: false) == 0)
				{
					m_ParameterListView.Items[j].SubItems[1].Text = empty2;
					break;
				}
			}
		}
	}

	private void SetupCurbViewInfo()
	{
		ListViewItem listViewItem = new ListViewItem();
		for (int i = 0; i < m_ParameterList.Count; i++)
		{
			string text = m_ParameterList[i].ParameterDisplayName;
			string text2 = m_ParameterList[i].ParameterName;
			string _strAngleStr = "";
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
			{
				continue;
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
			{
				_strAngleStr = EPShared.LocalRM.GetString("NewCurb");
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbHeightParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble = 0.5;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbWidthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble2 = 5.0 / 12.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbOffsetParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble3 = 1.0 / 12.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurbGutterHeightParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble4 = 1.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble4, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurbBaseSurfaceParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble5 = 1.0 / 6.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble5, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurbSubbaseSurfaceParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble6 = 1.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble6, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetGutterWidthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble7 = 2.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble7, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetGutterSlopeParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble8 = 0.03;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble8, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StreetCurbMaterialParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = "Default";
				string strStringToFind = _strAngleStr;
				IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strStringToFind, ignoreCase: true) == 0);
				List<Material> list = source.ToList();
				_strAngleStr = ((list.Count < 1) ? "" : list.ElementAt(0).Name);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strLinearIncrement;
				try
				{
					double dConvertedDouble9 = 0.0;
					if (_strAngleStr != null && _strAngleStr != string.Empty)
					{
						dConvertedDouble9 = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble9, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strLinearIncrement;
				}
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strCircularIncrement;
				try
				{
					double dAngle = 0.0;
					if (!string.IsNullOrEmpty(_strAngleStr))
					{
						dAngle = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strCircularIncrement;
				}
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				string text3 = text2;
				text2 = RevitParameters.Slope_ + text3;
				text3 = text;
				text = EPShared.LocalRM.GetString("Slope_") + text3;
			}
			listViewItem = m_ParameterListView.Items.Add(text2, text, string.Empty);
			listViewItem.SubItems.Add(_strAngleStr);
			listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = _strAngleStr;
		}
	}

	private void SetupSideWalkViewInfo()
	{
		ListViewItem listViewItem = new ListViewItem();
		for (int i = 0; i < m_ParameterList.Count; i++)
		{
			string text = m_ParameterList[i].ParameterDisplayName;
			string text2 = m_ParameterList[i].ParameterName;
			string _strAngleStr = "";
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SideWalkLocationLineParameter, ignoreCase: true) == 0)
			{
				continue;
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
			{
				_strAngleStr = EPShared.LocalRM.GetString("NewSidewalk");
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SideWalkLocationLineParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = EPShared.LocalRM.GetString("CenterString");
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SideWalkWidthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble = 4.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SideWalkPavementDepthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble2 = 0.25;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SideWalkBaseSurfaceParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble3 = 1.0 / 6.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SideWalkSubbaseSurfaceParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble4 = 0.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble4, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SideWalkCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SideWalkFillSlopeParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble5 = 0.01;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble5, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SideWalkPavementSurfaceParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = "Default";
				string strStringToFind = _strAngleStr;
				IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strStringToFind, ignoreCase: true) == 0);
				List<Material> list = source.ToList();
				_strAngleStr = ((list.Count < 1) ? "" : list.ElementAt(0).Name);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strLinearIncrement;
				try
				{
					double dConvertedDouble6 = 0.0;
					if (_strAngleStr != null && _strAngleStr != string.Empty)
					{
						dConvertedDouble6 = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble6, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strLinearIncrement;
				}
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strCircularIncrement;
				try
				{
					double dAngle = 0.0;
					if (!string.IsNullOrEmpty(_strAngleStr))
					{
						dAngle = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strCircularIncrement;
				}
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				string text3 = text2;
				text2 = RevitParameters.Slope_ + text3;
				text3 = text;
				text = EPShared.LocalRM.GetString("Slope_") + text3;
			}
			listViewItem = m_ParameterListView.Items.Add(text2, text, string.Empty);
			listViewItem.SubItems.Add(_strAngleStr);
			listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = _strAngleStr;
		}
	}

	private void SetupRetainingWallsViewInfo()
	{
		ListViewItem listViewItem = new ListViewItem();
		for (int i = 0; i < m_ParameterList.Count; i++)
		{
			string text = m_ParameterList[i].ParameterDisplayName;
			string text2 = m_ParameterList[i].ParameterName;
			string _strAngleStr = "";
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
			{
				continue;
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
			{
				_strAngleStr = EPShared.LocalRM.GetString("NewRetainingWall");
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.RetainingWallWidthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble = 2.0 / 3.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.RetainingWallCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.RetainingWallFillSlopeParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble2 = 0.01;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.RetainingWallHeightParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble3 = 10.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.RetainingWallBaseSurfaceParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble4 = 2.0 / 3.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble4, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.RetainingWallSubbaseSurfaceParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble5 = 1.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble5, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.RetainingWallMaterialParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = "Default";
				string strStringToFind = _strAngleStr;
				IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strStringToFind, ignoreCase: true) == 0);
				List<Material> list = source.ToList();
				_strAngleStr = ((list.Count < 1) ? "" : list.ElementAt(0).Name);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.RetainingWallSlopeProjectionParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = "No";
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strLinearIncrement;
				try
				{
					double dConvertedDouble6 = 0.0;
					if (_strAngleStr != null && _strAngleStr != string.Empty)
					{
						dConvertedDouble6 = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble6, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strLinearIncrement;
				}
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strCircularIncrement;
				try
				{
					double dAngle = 0.0;
					if (!string.IsNullOrEmpty(_strAngleStr))
					{
						dAngle = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strCircularIncrement;
				}
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				string text3 = text2;
				text2 = RevitParameters.Slope_ + text3;
				text3 = text;
				text = EPShared.LocalRM.GetString("Slope_") + text3;
			}
			listViewItem = m_ParameterListView.Items.Add(text2, text, string.Empty);
			listViewItem.SubItems.Add(_strAngleStr);
			listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = _strAngleStr;
		}
	}

	private void SetupStepsViewInfo()
	{
		ListViewItem listViewItem = new ListViewItem();
		for (int i = 0; i < m_ParameterList.Count; i++)
		{
			string text = m_ParameterList[i].ParameterDisplayName;
			string text2 = m_ParameterList[i].ParameterName;
			string _strAngleStr = "";
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
			{
				continue;
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StepsHeightParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StepsTreadParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = "0.66";
				double dConvertedDouble = 0.66;
				if (_strAngleStr != string.Empty)
				{
					dConvertedDouble = ValueConversion.ToDouble(_strAngleStr);
				}
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StepsWidthParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = "4.0";
				double dConvertedDouble2 = 4.0;
				if (_strAngleStr != string.Empty)
				{
					dConvertedDouble2 = ValueConversion.ToDouble(_strAngleStr);
				}
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.StepsMaterialParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = "Default";
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strLinearIncrement;
				try
				{
					double dConvertedDouble3 = 0.0;
					if (_strAngleStr != null && _strAngleStr != string.Empty)
					{
						dConvertedDouble3 = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strLinearIncrement;
				}
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strCircularIncrement;
				try
				{
					double dAngle = 0.0;
					if (!string.IsNullOrEmpty(_strAngleStr))
					{
						dAngle = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strCircularIncrement;
				}
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				string text3 = text2;
				text2 = RevitParameters.Slope_ + text3;
				text3 = text;
				text = EPShared.LocalRM.GetString("Slope_") + text3;
			}
			listViewItem = m_ParameterListView.Items.Add(text2, text, string.Empty);
			listViewItem.SubItems.Add(_strAngleStr);
			listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = _strAngleStr;
		}
	}

	private void SetupParkingLotViewInfo()
	{
		ListViewItem listViewItem = new ListViewItem();
		for (int i = 0; i < m_ParameterList.Count; i++)
		{
			string text = m_ParameterList[i].ParameterDisplayName;
			string text2 = m_ParameterList[i].ParameterName;
			string _strAngleStr = "";
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
			{
				continue;
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
			{
				_strAngleStr = EPShared.LocalRM.GetString("NewParkingLot");
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.ParkingLotPavementDepthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble = 0.5;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.ParkingLotParkingBaseDepthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble2 = 2.0 / 3.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.ParkingLotParkingSubBaseDepthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble3 = 1.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.ParkingLotParkingPavementSurfaceParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = "Default";
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strLinearIncrement;
				try
				{
					double dConvertedDouble4 = 0.0;
					if (_strAngleStr != null && _strAngleStr != string.Empty)
					{
						dConvertedDouble4 = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble4, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strLinearIncrement;
				}
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strCircularIncrement;
				try
				{
					double dAngle = 0.0;
					if (!string.IsNullOrEmpty(_strAngleStr))
					{
						dAngle = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strCircularIncrement;
				}
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				string text3 = text2;
				text2 = RevitParameters.Slope_ + text3;
				text3 = text;
				text = EPShared.LocalRM.GetString("Slope_") + text3;
			}
			listViewItem = m_ParameterListView.Items.Add(text2, text, string.Empty);
			listViewItem.SubItems.Add(_strAngleStr);
			listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = _strAngleStr;
		}
	}

	private void SetupSoftTerrainViewInfo()
	{
		ListViewItem listViewItem = new ListViewItem();
		for (int i = 0; i < m_ParameterList.Count; i++)
		{
			string parameterDisplayName = m_ParameterList[i].ParameterDisplayName;
			string text = m_ParameterList[i].ParameterName;
			string _strAngleStr = "";
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0)
			{
				continue;
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.TypeNamePlantFamilyType, ignoreCase: true) == 0)
			{
				_strAngleStr = EPShared.LocalRM.GetString("NewSoftTerrain");
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SoftTerrainLeftOffsetWidthParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SoftTerrainRightOffsetWidthParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble = 2.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SoftTerrainCutSlopeParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.SoftTerrainFillSlopeParameter, ignoreCase: true) == 0)
			{
				double dConvertedDouble2 = 1.0;
				RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble2, m_CurrentSlopeDisplayUnitType, m_dCurrentSlopeAccuracy, m_SlopeUnitSymbolType, m_Units, ref _strAngleStr);
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strLinearIncrement;
				try
				{
					double dConvertedDouble3 = 0.0;
					if (_strAngleStr != null && _strAngleStr != string.Empty)
					{
						dConvertedDouble3 = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.Units_ConvertDoubleToString(dConvertedDouble3, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strLinearIncrement;
				}
			}
			else if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				_strAngleStr = m_strCircularIncrement;
				try
				{
					double dAngle = 0.0;
					if (!string.IsNullOrEmpty(_strAngleStr))
					{
						dAngle = ValueConversion.ToDouble(_strAngleStr);
					}
					RevitGeometry.eplStrE_ConvertRadiansToAngleStr(m_AngularOptions.LegacyDisplayUnits(), dAngle, m_dAngularRounding, m_AngularSymbolType, m_DecimalSymbolType, ref _strAngleStr);
				}
				catch (Exception)
				{
					_strAngleStr = m_strCircularIncrement;
				}
			}
			if (string.Compare(m_ParameterList[i].ParameterName, RevitParameters.LinearIncrementParameter, ignoreCase: true) == 0 || string.Compare(m_ParameterList[i].ParameterName, RevitParameters.CurveIncrementParameter, ignoreCase: true) == 0)
			{
				string text2 = text;
				text = RevitParameters.Slope_ + text2;
				text2 = parameterDisplayName;
				parameterDisplayName = EPShared.LocalRM.GetString("Slope_") + text2;
			}
			listViewItem = m_ParameterListView.Items.Add(text, text, string.Empty);
			listViewItem.SubItems.Add(_strAngleStr);
			listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text = _strAngleStr;
		}
	}

	private void OnButtonOkClicked(object sender, EventArgs e)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		string strFamilyTypeName = string.Empty;
		string empty = string.Empty;
		string empty2 = string.Empty;
		string text = string.Empty;
		string text2 = string.Empty;
		if (m_FamilyType == LandcaddID.PlantMix)
		{
			for (int i = 0; i < m_ParameterListView.Items.Count; i++)
			{
				empty = m_ParameterListView.Items[i].SubItems[0].Text;
				empty2 = m_ParameterListView.Items[i].SubItems[1].Text;
				if (string.Compare(empty, EPShared.LocalRM.GetString("ScientificNamePlantMixFamilyParameter"), ignoreCase: true) == 0)
				{
					if (!string.IsNullOrEmpty(empty2))
					{
						flag = true;
						text2 = empty2;
						if (flag2)
						{
							break;
						}
					}
				}
				else if (string.Compare(empty, EPShared.LocalRM.GetString("CommonNamePlantMixFamilyParameter"), ignoreCase: true) == 0 && !string.IsNullOrEmpty(empty2))
				{
					flag2 = true;
					text = empty2;
					if (flag)
					{
						break;
					}
				}
			}
			if (!flag && !flag2)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("ValidTypeCommonOrScientificName"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			for (int j = 0; j < m_ParameterListView.Items.Count; j++)
			{
				empty = m_ParameterListView.Items[j].SubItems[0].Text;
				empty2 = m_ParameterListView.Items[j].SubItems[1].Text;
				if (string.Compare(empty, EPShared.LocalRM.GetString("PercentagePlantMixFamilyParameter"), ignoreCase: true) == 0)
				{
					if (empty2 != string.Empty)
					{
						flag3 = true;
					}
				}
				else if (string.Compare(empty, EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter"), ignoreCase: true) == 0 && empty2 != string.Empty)
				{
					flag4 = true;
				}
			}
			if (!flag3 && !flag4)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("PercentagePlantMixFamilyParameter") + " " + EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter") + EPShared.LocalRM.GetString("_And_") + EPShared.LocalRM.GetString("ValidFields"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			if (!flag3)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("PercentagePlantMixFamilyParameter") + EPShared.LocalRM.GetString("ValidFields"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			if (!flag4)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter") + EPShared.LocalRM.GetString("ValidFields"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			if (m_nSeletedIndex < 0)
			{
				FamilyTypeContainer familyTypeContainer = new FamilyTypeContainer();
				familyTypeContainer.FamilyParameterList = new List<FamilyParameterWrapper>();
				string text3 = string.Empty;
				string empty3 = string.Empty;
				string empty4 = string.Empty;
				if ((flag2 || flag) && text3 == string.Empty)
				{
					if (flag2 && flag)
					{
						text3 = text + "/" + text2;
					}
					else if (flag2)
					{
						text3 = text;
					}
					else if (flag)
					{
						text3 = text2;
					}
					if (familyTypeContainer.FamilyTypeWrapper == null)
					{
						familyTypeContainer.FamilyTypeWrapper = new FamilyTypeWrapper(text3);
					}
					else
					{
						familyTypeContainer.FamilyTypeWrapper.FamilyTypeName = text3;
					}
				}
				for (int k = 0; k < m_ParameterListView.Items.Count; k++)
				{
					StorageType storageType = StorageType.String;
					empty4 = m_ParameterListView.Items[k].SubItems[0].Name;
					empty3 = m_ParameterListView.Items[k].SubItems[0].Text;
					text3 = m_ParameterListView.Items[k].SubItems[1].Text;
					if (string.Compare(empty3, EPShared.LocalRM.GetString("PercentagePlantMixFamilyParameter"), ignoreCase: true) == 0 || string.Compare(empty3, EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter"), ignoreCase: true) == 0)
					{
						string text4 = m_ParameterListView.Items[k].SubItems[1].Text;
						if (string.Compare(text4, EPShared.LocalRM.GetString("NotAvailableString"), ignoreCase: true) == 0)
						{
							text4 = "0.0";
						}
						storageType = StorageType.Double;
						if (string.Compare(empty3, EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter"), ignoreCase: true) == 0)
						{
							double _dReturnedDouble = 0.0;
							RevitGeometry.Units_ConvertStringToDouble(text4, m_Units, UnitType.UT_Length, ref _dReturnedDouble);
							text3 = ValueConversion.ToString(_dReturnedDouble);
						}
						else
						{
							text3 = text4;
						}
					}
					else if (string.Compare(empty3, EPShared.LocalRM.GetString("CostGenericFamilyParameter"), ignoreCase: true) == 0)
					{
						storageType = StorageType.Double;
						text3 = m_ParameterListView.Items[k].SubItems[1].Text;
					}
					if (string.Compare(empty3, EPShared.LocalRM.GetString("RegionNameGenericModelFamilyParameter"), ignoreCase: true) == 0 || string.Compare(empty3, EPShared.LocalRM.GetString("UniqueIDGenericModelFamilyParameter"), ignoreCase: true) == 0 || string.Compare(empty3, EPShared.LocalRM.GetString("SiteworksIDFamilyParameter"), ignoreCase: true) == 0 || string.Compare(empty3, EPShared.LocalRM.GetString("AssemblyCodeGenericFamilyParameter"), ignoreCase: true) == 0)
					{
						text3 = string.Empty;
					}
					FamilyParameterWrapper item = new FamilyParameterWrapper(empty3, empty4, text3, storageType);
					familyTypeContainer.FamilyParameterList.Add(item);
				}
				if (!FindParameter(RevitParameters.RegionNameGenericModelFamilyParameter, familyTypeContainer))
				{
					FamilyParameterWrapper item2 = new FamilyParameterWrapper(EPShared.LocalRM.GetString("RegionNameGenericModelFamilyParameter"), RevitParameters.RegionNameGenericModelFamilyParameter, "", StorageType.String);
					familyTypeContainer.FamilyParameterList.Add(item2);
				}
				if (!FindParameter(RevitParameters.UniqueIDGenericModelFamilyParameter, familyTypeContainer))
				{
					FamilyParameterWrapper item3 = new FamilyParameterWrapper(EPShared.LocalRM.GetString("UniqueIDGenericModelFamilyParameter"), RevitParameters.UniqueIDGenericModelFamilyParameter, "", StorageType.String);
					familyTypeContainer.FamilyParameterList.Add(item3);
				}
				if (!FindParameter(RevitParameters.SiteworksIDFamilyParameter, familyTypeContainer))
				{
					FamilyParameterWrapper item4 = new FamilyParameterWrapper(EPShared.LocalRM.GetString("SiteworksIDFamilyParameter"), RevitParameters.SiteworksIDFamilyParameter, "", StorageType.String);
					familyTypeContainer.FamilyParameterList.Add(item4);
				}
				m_FamilyTypeContainerList.Add(familyTypeContainer);
			}
			else
			{
				string text5 = string.Empty;
				if ((flag2 || flag) && text5 == string.Empty)
				{
					if (flag2 && flag)
					{
						text5 = text + "/" + text2;
					}
					else if (flag2)
					{
						text5 = text;
					}
					else if (flag)
					{
						text5 = text2;
					}
					m_FamilyTypeContainerList[m_nSeletedIndex].FamilyTypeWrapper.FamilyTypeName = text5;
				}
				for (int l = 0; l < m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList.Count; l++)
				{
					string displayName = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[l].DisplayName;
					string parameterName = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[l].ParameterName;
					if (string.Compare(parameterName, RevitParameters.RegionNameGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(parameterName, RevitParameters.UniqueIDGenericModelFamilyParameter, ignoreCase: true) == 0 || string.Compare(parameterName, RevitParameters.SiteworksIDFamilyParameter, ignoreCase: true) == 0 || string.Compare(parameterName, RevitParameters.AssemblyCodeGenericFamilyParameter, ignoreCase: true) == 0)
					{
						string empty5 = string.Empty;
						m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[l].Value = empty5;
						continue;
					}
					for (int m = 0; m < m_ParameterListView.Items.Count; m++)
					{
						if (string.Compare(displayName, m_ParameterListView.Items[m].SubItems[0].Text, ignoreCase: true) != 0)
						{
							continue;
						}
						string empty5 = m_ParameterListView.Items[m].SubItems[1].Text;
						if (string.Compare(parameterName, RevitParameters.PlantSpacingPlantMixFamilyParameter, ignoreCase: true) == 0)
						{
							double _dReturnedDouble2 = 0.0;
							if (empty5 != null && empty5 != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), empty5, ignoreCase: true) == 0)
							{
								_dReturnedDouble2 = 0.0;
							}
							else if (empty5 != null && empty5 != string.Empty)
							{
								RevitGeometry.Units_ConvertStringToDouble(empty5, m_Units, UnitType.UT_Length, ref _dReturnedDouble2);
							}
							empty5 = ValueConversion.ToString(_dReturnedDouble2);
						}
						if (empty5 == null)
						{
							empty5 = string.Empty;
						}
						m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[l].Value = empty5;
						break;
					}
				}
			}
		}
		else if (m_FamilyType == LandcaddID.PatioWalk)
		{
			bool flag5 = false;
			if (m_nSeletedIndex < 0)
			{
				flag5 = IsDuplicateEdiging();
			}
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			bool flag9 = false;
			int index = -1;
			for (int n = 0; n < m_ParameterListView.Items.Count; n++)
			{
				empty = m_ParameterListView.Items[n].SubItems[0].Text;
				empty2 = m_ParameterListView.Items[n].SubItems[1].Text;
				if (string.Compare(empty, EPShared.LocalRM.GetString("MaterialPathWalkFamilyParameter"), ignoreCase: true) == 0)
				{
					if (empty2 != string.Empty)
					{
						strFamilyTypeName = empty2;
						flag9 = true;
					}
				}
				else if (string.Compare(empty, EPShared.LocalRM.GetString("EdgePathWalkFamilyParameter"), ignoreCase: true) == 0)
				{
					if (empty2 != string.Empty && string.Compare(empty2, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) == 0)
					{
						flag6 = true;
					}
				}
				else if (string.Compare(empty, EPShared.LocalRM.GetString("MowingStripPathWalkFamilyParameter"), ignoreCase: true) == 0)
				{
					if (empty2 != string.Empty && string.Compare(empty2, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) == 0)
					{
						flag7 = true;
					}
				}
				else
				{
					if (string.Compare(empty, EPShared.LocalRM.GetString("EdgeWidthPathWalkFamilyParameter"), ignoreCase: true) != 0 || !(empty2 != string.Empty))
					{
						continue;
					}
					try
					{
						double _dReturnedDouble3 = 0.0;
						if (RevitGeometry.Units_ConvertStringToDouble(empty2, m_Units, UnitType.UT_Length, ref _dReturnedDouble3) && _dReturnedDouble3 > 0.0)
						{
							empty2 = ValueConversion.ToString(_dReturnedDouble3);
							flag8 = true;
						}
						index = n;
					}
					catch (Exception)
					{
						flag8 = false;
						index = n;
					}
				}
			}
			if (!flag9)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidTypeName"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			if (flag6 && flag7 && !flag8)
			{
				string _strToDisplay = string.Empty;
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedValidEdgeWidth"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				RevitGeometry.Units_ConvertDoubleToString(0.0, m_FormatOptions.LegacyDisplayUnits(), m_FormatOptions.Accuracy, m_UnitSymbolType, m_Units, ref _strToDisplay);
				m_ParameterListView.Items[index].SubItems[1].Text = _strToDisplay;
				return;
			}
			if (!flag6 && !flag7 && flag8)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedEdgeMowingYes"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			if (!flag6 && flag7)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoEdgingYesMowingMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			if (flag6 && !flag7 && flag8)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NeedEdgeMowingYes"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			if (!flag6 && flag7 && flag8)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoEdgingYesMowingMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			if (flag6 && flag5)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("DuplicateEdgeString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			if (m_nSeletedIndex < 0)
			{
				FamilyTypeContainer familyTypeContainer2 = new FamilyTypeContainer();
				familyTypeContainer2.FamilyParameterList = new List<FamilyParameterWrapper>();
				string empty6 = string.Empty;
				string empty7 = string.Empty;
				string empty8 = string.Empty;
				for (int num = 0; num < m_ParameterListView.Items.Count; num++)
				{
					StorageType storageType2 = StorageType.String;
					empty8 = m_ParameterListView.Items[num].SubItems[0].Name;
					empty7 = m_ParameterListView.Items[num].SubItems[0].Text;
					empty6 = m_ParameterListView.Items[num].SubItems[1].Text;
					if (string.Compare(empty7, EPShared.LocalRM.GetString("MaterialPathWalkFamilyParameter"), ignoreCase: true) == 0)
					{
						familyTypeContainer2.FamilyTypeWrapper = new FamilyTypeWrapper(strFamilyTypeName);
					}
					else if (string.Compare(empty7, EPShared.LocalRM.GetString("ThicknessPathWalkFamilyParameter"), ignoreCase: true) == 0 || string.Compare(empty7, EPShared.LocalRM.GetString("EdgeWidthPathWalkFamilyParameter"), ignoreCase: true) == 0)
					{
						storageType2 = StorageType.Double;
						double _dReturnedDouble4 = 0.0;
						if (empty6 != null && empty6 != string.Empty)
						{
							RevitGeometry.Units_ConvertStringToDouble(empty6, m_Units, UnitType.UT_Length, ref _dReturnedDouble4);
						}
						empty6 = ValueConversion.ToString(_dReturnedDouble4);
					}
					else if (string.Compare(empty7, EPShared.LocalRM.GetString("EdgePathWalkFamilyParameter"), ignoreCase: true) == 0 || string.Compare(empty7, EPShared.LocalRM.GetString("MowingStripPathWalkFamilyParameter"), ignoreCase: true) == 0)
					{
						storageType2 = StorageType.Integer;
						if (string.Compare(empty6, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) == 0)
						{
							empty6 = "1";
						}
						else if (string.Compare(empty6, EPShared.LocalRM.GetString("NoString"), ignoreCase: true) == 0)
						{
							empty6 = "0";
						}
					}
					if (string.Compare(empty7, EPShared.LocalRM.GetString("RegionNameGenericModelFamilyParameter"), ignoreCase: true) == 0 || string.Compare(empty7, EPShared.LocalRM.GetString("UniqueIDGenericModelFamilyParameter"), ignoreCase: true) == 0 || string.Compare(empty7, EPShared.LocalRM.GetString("SiteworksIDFamilyParameter"), ignoreCase: true) == 0 || string.Compare(empty7, EPShared.LocalRM.GetString("AssemblyCodeGenericFamilyParameter"), ignoreCase: true) == 0)
					{
						empty6 = string.Empty;
					}
					FamilyParameterWrapper item5 = new FamilyParameterWrapper(empty7, empty8, empty6, storageType2);
					familyTypeContainer2.FamilyParameterList.Add(item5);
				}
				if (!FindParameter(EPShared.LocalRM.GetString("RegionNameGenericModelFamilyParameter"), familyTypeContainer2))
				{
					FamilyParameterWrapper item6 = new FamilyParameterWrapper(EPShared.LocalRM.GetString("RegionNameGenericModelFamilyParameter"), RevitParameters.RegionNameGenericModelFamilyParameter, "", StorageType.String);
					familyTypeContainer2.FamilyParameterList.Add(item6);
				}
				if (!FindParameter(EPShared.LocalRM.GetString("UniqueIDGenericModelFamilyParameter"), familyTypeContainer2))
				{
					FamilyParameterWrapper item7 = new FamilyParameterWrapper(EPShared.LocalRM.GetString("UniqueIDGenericModelFamilyParameter"), RevitParameters.UniqueIDGenericModelFamilyParameter, "", StorageType.String);
					familyTypeContainer2.FamilyParameterList.Add(item7);
				}
				if (!FindParameter(EPShared.LocalRM.GetString("SiteworksIDFamilyParameter"), familyTypeContainer2))
				{
					FamilyParameterWrapper item8 = new FamilyParameterWrapper(EPShared.LocalRM.GetString("SiteworksIDFamilyParameter"), RevitParameters.SiteworksIDFamilyParameter, "", StorageType.String);
					familyTypeContainer2.FamilyParameterList.Add(item8);
				}
				m_FamilyTypeContainerList.Add(familyTypeContainer2);
			}
			else
			{
				for (int num2 = 0; num2 < m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList.Count; num2++)
				{
					string displayName2 = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[num2].DisplayName;
					string parameterName2 = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[num2].ParameterName;
					for (int num3 = 0; num3 < m_ParameterListView.Items.Count; num3++)
					{
						if (string.Compare(displayName2, m_ParameterListView.Items[num3].SubItems[0].Text, ignoreCase: true) != 0)
						{
							continue;
						}
						string text6 = m_ParameterListView.Items[num3].SubItems[1].Text;
						if (string.Compare(parameterName2, RevitParameters.RegionNameGenericModelFamilyParameter, ignoreCase: true) == 0)
						{
							text6 = string.Empty;
							m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[num2].Value = text6;
							continue;
						}
						if (string.Compare(parameterName2, RevitParameters.ThicknessPathWalkFamilyParameter, ignoreCase: true) == 0 || string.Compare(parameterName2, RevitParameters.EdgeWidthPathWalkFamilyParameter, ignoreCase: true) == 0)
						{
							double _dReturnedDouble5 = 1.0;
							if (text6 != null && text6 != string.Empty)
							{
								RevitGeometry.Units_ConvertStringToDouble(text6, m_Units, UnitType.UT_Length, ref _dReturnedDouble5);
							}
							text6 = ValueConversion.ToString(_dReturnedDouble5);
						}
						if (string.Compare(parameterName2, RevitParameters.EdgePathWalkFamilyParameter, ignoreCase: true) == 0 || string.Compare(parameterName2, RevitParameters.MowingStripPathWalkFamilyParameter, ignoreCase: true) == 0)
						{
							if (string.Compare(text6, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) == 0)
							{
								text6 = "1";
							}
							else if (string.Compare(text6, EPShared.LocalRM.GetString("NoString"), ignoreCase: true) == 0)
							{
								text6 = "0";
							}
						}
						if (text6 == null)
						{
							text6 = string.Empty;
						}
						if (string.Compare(parameterName2, RevitParameters.MaterialPathWalkFamilyParameter, ignoreCase: true) == 0 && text6 != string.Empty)
						{
							m_FamilyTypeContainerList[m_nSeletedIndex].FamilyTypeWrapper.FamilyTypeName = text6;
						}
						m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList[num2].Value = text6;
						break;
					}
				}
			}
		}
		else
		{
			QuantumListView quantumListView = null;
			if (m_FamilyType == LandcaddID.Street)
			{
				UpdateGlobalListView();
				quantumListView = m_GlobalListView;
			}
			else
			{
				quantumListView = m_ParameterListView;
			}
			string text7 = string.Empty;
			for (int num4 = 0; num4 < quantumListView.Items.Count; num4++)
			{
				empty = quantumListView.Items[num4].SubItems[0].Text;
				empty2 = quantumListView.Items[num4].SubItems[1].Text;
				if (string.Compare(empty, EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), ignoreCase: true) == 0)
				{
					if (string.IsNullOrEmpty(empty2))
					{
						RevitMessageBox.Show(EPShared.LocalRM.GetString("TypeNameMissing"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
						return;
					}
					text7 = empty2;
					break;
				}
			}
			FamilyTypeContainer familyTypeContainer3 = new FamilyTypeContainer();
			familyTypeContainer3.FamilyParameterList = new List<FamilyParameterWrapper>();
			bool flag10 = true;
			if (IsFamilyAndTypeNameSame(text7))
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("SameTypeNameAsFamilyNameMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			if (m_nSeletedIndex < 0)
			{
				familyTypeContainer3.FamilyParameterList = new List<FamilyParameterWrapper>();
				familyTypeContainer3.FamilyTypeWrapper = null;
			}
			else
			{
				familyTypeContainer3.FamilyTypeWrapper = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyTypeWrapper;
				familyTypeContainer3.FamilyParameterList = m_FamilyTypeContainerList[m_nSeletedIndex].FamilyParameterList;
			}
			if (familyTypeContainer3.FamilyTypeWrapper != null && string.Compare(familyTypeContainer3.FamilyTypeWrapper.FamilyTypeName, text7, ignoreCase: true) == 0)
			{
				flag10 = false;
			}
			if (flag10 && CheckForDuplicateTypeName(text7))
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("DuplicateTypeNameMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return;
			}
			if (familyTypeContainer3.FamilyTypeWrapper == null)
			{
				familyTypeContainer3.FamilyTypeWrapper = new FamilyTypeWrapper(text7);
			}
			else
			{
				familyTypeContainer3.FamilyTypeWrapper.FamilyTypeName = text7;
			}
			string strValue = string.Empty;
			string empty9 = string.Empty;
			string empty10 = string.Empty;
			for (int num5 = 0; num5 < quantumListView.Items.Count; num5++)
			{
				StorageType storageType3 = StorageType.String;
				empty10 = quantumListView.Items[num5].SubItems[0].Name;
				empty9 = quantumListView.Items[num5].SubItems[0].Text;
				strValue = quantumListView.Items[num5].SubItems[1].Text;
				if (string.Compare(empty9, EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), ignoreCase: true) == 0)
				{
					continue;
				}
				if (string.Compare(empty9, EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("LinearIncrementParameter"), ignoreCase: false) == 0)
				{
					empty10 = RevitParameters.LinearIncrementParameter;
					empty9 = EPShared.LocalRM.GetString("LinearIncrementParameter");
				}
				else if (string.Compare(empty9, EPShared.LocalRM.GetString("Slope_") + EPShared.LocalRM.GetString("CurveIncrementParameter"), ignoreCase: false) == 0)
				{
					empty10 = RevitParameters.CurveIncrementParameter;
					empty9 = EPShared.LocalRM.GetString("CurveIncrementParameter");
				}
				if (string.Compare(empty9, EPShared.LocalRM.GetString("LinearIncrementParameter"), ignoreCase: true) == 0)
				{
					storageType3 = StorageType.Double;
					double _dReturnedDouble6 = 0.0;
					if (strValue != null && strValue != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), strValue, ignoreCase: true) == 0)
					{
						_dReturnedDouble6 = 0.0;
					}
					else if (strValue != null && strValue != string.Empty)
					{
						RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Length, ref _dReturnedDouble6);
					}
					strValue = ValueConversion.ToString(_dReturnedDouble6);
				}
				else if (string.Compare(empty9, EPShared.LocalRM.GetString("CurveIncrementParameter"), ignoreCase: true) == 0)
				{
					storageType3 = StorageType.Double;
					double num6 = 0.0;
					if (strValue != null && strValue != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), strValue, ignoreCase: true) == 0)
					{
						num6 = 0.0;
					}
					else if (strValue != null && strValue != string.Empty)
					{
						num6 = RevitGeometry.eplGeoE_ConvertAngleToRadians(strValue);
					}
					strValue = ValueConversion.ToString(num6);
				}
				else if (string.Compare(empty9, EPShared.LocalRM.GetString("CostGenericFamilyParameter"), ignoreCase: true) == 0)
				{
					storageType3 = StorageType.Double;
				}
				else if (string.Compare(empty9, EPShared.LocalRM.GetString("SiteworksIDFamilyParameter"), ignoreCase: true) == 0)
				{
					strValue = m_FamilyType.ToString("000");
					storageType3 = StorageType.String;
				}
				if (m_FamilyType == LandcaddID.Street)
				{
					if (string.Compare(empty9, EPShared.LocalRM.GetString("StreetNumberOfLanesParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Integer;
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("StreetCurbDisplayParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Integer;
						if (string.Compare(strValue, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) == 0)
						{
							strValue = "1";
						}
						else if (string.Compare(strValue, EPShared.LocalRM.GetString("NoString"), ignoreCase: true) == 0)
						{
							strValue = "0";
						}
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("StreetPavementSurfaceParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetCurbMaterialParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Integer;
						IEnumerable<Material> source = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strValue, ignoreCase: false) == 0);
						Dictionary<long, Material> dictionary = source.ToDictionary((Material p) => p.Id.Value64());
						if (dictionary.Count >= 1)
						{
							strValue = dictionary.ElementAt(0).Value.Id.Value64().ToString();
						}
						else
						{
							strValue = "00000";
						}
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("StreetCrossSlopeParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetGutterSlopeParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetCutSlopeParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetFillSlopeParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble7 = 0.0;
						if (strValue != null && strValue != string.Empty && !RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Slope, ref _dReturnedDouble7))
						{
							_dReturnedDouble7 = 0.0;
						}
						strValue = ValueConversion.ToString(_dReturnedDouble7);
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("StreetLaneWidthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetPavementDepthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetGutterWidthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetCurbOffsetParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetCurbWidthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetCurbHeightParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetBoulevardWidthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetBaseSurfaceParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetSubbaseSurfaceParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble8 = 0.0;
						if (strValue != null && strValue != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), strValue, ignoreCase: true) == 0)
						{
							_dReturnedDouble8 = 0.0;
						}
						else if (strValue != null && strValue != string.Empty)
						{
							RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Length, ref _dReturnedDouble8);
						}
						strValue = ValueConversion.ToString(_dReturnedDouble8);
					}
				}
				else if (m_FamilyType == LandcaddID.SoftTerrain)
				{
					if (string.Compare(empty9, EPShared.LocalRM.GetString("SoftTerrainCutSlopeParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("SoftTerrainFillSlopeParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble9 = 0.0;
						if (strValue != null && strValue != string.Empty && !RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Slope, ref _dReturnedDouble9))
						{
							_dReturnedDouble9 = 0.0;
						}
						strValue = ValueConversion.ToString(_dReturnedDouble9);
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("SoftTerrainLeftOffsetWidthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("SoftTerrainRightOffsetWidthParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble10 = 0.0;
						if (strValue != null && strValue != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), strValue, ignoreCase: true) == 0)
						{
							_dReturnedDouble10 = 0.0;
						}
						else if (strValue != null && strValue != string.Empty)
						{
							RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Length, ref _dReturnedDouble10);
						}
						strValue = ValueConversion.ToString(_dReturnedDouble10);
					}
				}
				else if (m_FamilyType == LandcaddID.Curb)
				{
					if (string.Compare(empty9, EPShared.LocalRM.GetString("StreetGutterSlopeParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble11 = 0.0;
						if (strValue != null && strValue != string.Empty && !RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Slope, ref _dReturnedDouble11))
						{
							_dReturnedDouble11 = 0.0;
						}
						strValue = ValueConversion.ToString(_dReturnedDouble11);
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("StreetCurbHeightParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetCurbOffsetParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("CurbGutterHeightParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetCurbWidthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("StreetGutterWidthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("CurbBaseSurfaceParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("CurbSubbaseSurfaceParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble12 = 0.0;
						if (strValue != null && strValue != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), strValue, ignoreCase: true) == 0)
						{
							_dReturnedDouble12 = 0.0;
						}
						else if (strValue != null && strValue != string.Empty)
						{
							RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Length, ref _dReturnedDouble12);
						}
						strValue = ValueConversion.ToString(_dReturnedDouble12);
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("StreetCurbMaterialParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Integer;
						IEnumerable<Material> source2 = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strValue, ignoreCase: false) == 0);
						Dictionary<long, Material> dictionary2 = source2.ToDictionary((Material p) => p.Id.Value64());
						if (dictionary2.Count >= 1)
						{
							strValue = dictionary2.ElementAt(0).Value.Id.Value64().ToString();
						}
						else
						{
							strValue = "00000";
						}
					}
				}
				else if (m_FamilyType == LandcaddID.Sidewalk)
				{
					if (string.Compare(empty9, EPShared.LocalRM.GetString("TypeNamePlantFamilyType"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("SideWalkLocationLineParameter"), ignoreCase: true) == 0)
					{
						continue;
					}
					if (string.Compare(empty9, EPShared.LocalRM.GetString("SideWalkLocationLineParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Integer;
						if (string.Compare(strValue, EPShared.LocalRM.GetString("SideString"), ignoreCase: true) == 0)
						{
							strValue = "1";
						}
						else if (string.Compare(strValue, EPShared.LocalRM.GetString("CenterString"), ignoreCase: true) == 0)
						{
							strValue = "0";
						}
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("SideWalkPavementSurfaceParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Integer;
						IEnumerable<Material> source3 = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strValue, ignoreCase: false) == 0);
						Dictionary<long, Material> dictionary3 = source3.ToDictionary((Material p) => p.Id.Value64());
						if (dictionary3.Count >= 1)
						{
							strValue = dictionary3.ElementAt(0).Value.Id.Value64().ToString();
						}
						else
						{
							strValue = "00000";
						}
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("SideWalkPavementDepthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("SideWalkBaseSurfaceParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("SideWalkSubbaseSurfaceParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble13 = 0.0;
						if (strValue != null && strValue != string.Empty)
						{
							RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Length, ref _dReturnedDouble13);
						}
						strValue = ValueConversion.ToString(_dReturnedDouble13);
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("SideWalkWidthParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble14 = 0.0;
						if (strValue != null && strValue != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), strValue, ignoreCase: true) == 0)
						{
							_dReturnedDouble14 = 0.0;
						}
						else if (strValue != null && strValue != string.Empty)
						{
							RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Length, ref _dReturnedDouble14);
						}
						strValue = ValueConversion.ToString(_dReturnedDouble14);
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("SideWalkCutSlopeParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("SideWalkFillSlopeParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble15 = 0.0;
						if (strValue != null && strValue != string.Empty && !RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Slope, ref _dReturnedDouble15))
						{
							_dReturnedDouble15 = 0.0;
						}
						strValue = ValueConversion.ToString(_dReturnedDouble15);
					}
				}
				else if (m_FamilyType == LandcaddID.RetainingWall)
				{
					if (string.Compare(empty9, EPShared.LocalRM.GetString("RetainingWallSlopeProjectionParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Integer;
						if (string.Compare(strValue, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) == 0)
						{
							strValue = "1";
						}
						else if (string.Compare(strValue, EPShared.LocalRM.GetString("NoString"), ignoreCase: true) == 0)
						{
							strValue = "0";
						}
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("RetainingWallMaterialParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Integer;
						IEnumerable<Material> source4 = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strValue, ignoreCase: false) == 0);
						Dictionary<long, Material> dictionary4 = source4.ToDictionary((Material p) => p.Id.Value64());
						if (dictionary4.Count >= 1)
						{
							strValue = dictionary4.ElementAt(0).Value.Id.Value64().ToString();
						}
						else
						{
							strValue = "00000";
						}
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("RetainingWallCutSlopeParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("RetainingWallFillSlopeParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble16 = 0.0;
						if (strValue != null && strValue != string.Empty && !RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Slope, ref _dReturnedDouble16))
						{
							_dReturnedDouble16 = 0.0;
						}
						strValue = ValueConversion.ToString(_dReturnedDouble16);
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("RetainingWallWidthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("RetainingWallHeightParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("RetainingWallBaseSurfaceParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("RetainingWallSubbaseSurfaceParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble17 = 0.0;
						if (strValue != null && strValue != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), strValue, ignoreCase: true) == 0)
						{
							_dReturnedDouble17 = 0.0;
						}
						else if (strValue != null && strValue != string.Empty)
						{
							RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Length, ref _dReturnedDouble17);
						}
						strValue = ValueConversion.ToString(_dReturnedDouble17);
					}
				}
				else if (m_FamilyType == LandcaddID.ParkingLot)
				{
					if (string.Compare(empty9, EPShared.LocalRM.GetString("ParkingLotParkingPavementSurfaceParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Integer;
						IEnumerable<Material> source5 = m_AllMaterialList.Where((Material SelectedMaterial) => string.Compare(SelectedMaterial.Name, strValue, ignoreCase: false) == 0);
						Dictionary<long, Material> dictionary5 = source5.ToDictionary((Material p) => p.Id.Value64());
						if (dictionary5.Count >= 1)
						{
							strValue = dictionary5.ElementAt(0).Value.Id.Value64().ToString();
						}
						else
						{
							strValue = "00000";
						}
					}
					else if (string.Compare(empty9, EPShared.LocalRM.GetString("ParkingLotPavementDepthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("ParkingLotParkingBaseDepthParameter"), ignoreCase: true) == 0 || string.Compare(empty9, EPShared.LocalRM.GetString("ParkingLotParkingSubBaseDepthParameter"), ignoreCase: true) == 0)
					{
						storageType3 = StorageType.Double;
						double _dReturnedDouble18 = 0.0;
						if (strValue != null && strValue != string.Empty && string.Compare(EPShared.LocalRM.GetString("NotAvailableString"), strValue, ignoreCase: true) == 0)
						{
							_dReturnedDouble18 = 0.0;
						}
						else if (strValue != null && strValue != string.Empty)
						{
							RevitGeometry.Units_ConvertStringToDouble(strValue, m_Units, UnitType.UT_Length, ref _dReturnedDouble18);
						}
						strValue = ValueConversion.ToString(_dReturnedDouble18);
					}
				}
				if (m_nSeletedIndex < 0)
				{
					FamilyParameterWrapper item9 = new FamilyParameterWrapper(empty9, empty10, strValue, storageType3);
					familyTypeContainer3.FamilyParameterList.Add(item9);
					continue;
				}
				for (int num7 = 0; num7 < familyTypeContainer3.FamilyParameterList.Count; num7++)
				{
					if (string.Compare(familyTypeContainer3.FamilyParameterList[num7].ParameterName, empty10, ignoreCase: true) == 0)
					{
						familyTypeContainer3.FamilyParameterList[num7].Value = strValue;
						break;
					}
				}
			}
			if (m_nSeletedIndex < 0)
			{
				if (!FindParameter(EPShared.LocalRM.GetString("SiteworksIDFamilyParameter"), familyTypeContainer3))
				{
					FamilyParameterWrapper item10 = new FamilyParameterWrapper(EPShared.LocalRM.GetString("SiteworksIDFamilyParameter"), RevitParameters.SiteworksIDFamilyParameter, Convert.ToInt32(m_FamilyType).ToString("000"), StorageType.String);
					familyTypeContainer3.FamilyParameterList.Add(item10);
				}
				m_FamilyTypeContainerList.Add(familyTypeContainer3);
			}
		}
		DialogResult = DialogResult.OK;
	}

	private bool IsFamilyAndTypeNameSame(string _strTypeName)
	{
		if (string.Compare(_strTypeName, m_strFamilyName, ignoreCase: false) == 0)
		{
			return true;
		}
		return false;
	}

	private bool CheckForDuplicateTypeName(string _strTypeName)
	{
		int num = 0;
		foreach (FamilyTypeContainer familyTypeContainer in m_FamilyTypeContainerList)
		{
			if (string.Compare(_strTypeName, familyTypeContainer.FamilyTypeWrapper.FamilyTypeName, ignoreCase: true) == 0)
			{
				if (m_nSeletedIndex < 0)
				{
					return true;
				}
				if (num != m_nSeletedIndex)
				{
					return true;
				}
				num++;
			}
			else
			{
				num++;
			}
		}
		return false;
	}

	private bool IsDuplicateEdiging()
	{
		for (int i = 0; i < m_FamilyTypeContainerList.Count; i++)
		{
			int count = m_FamilyTypeContainerList[i].FamilyParameterList.Count;
			for (int j = 0; j < count; j++)
			{
				if (string.Compare(m_FamilyTypeContainerList[i].FamilyParameterList[j].ParameterName, RevitParameters.EdgePathWalkFamilyParameter, ignoreCase: true) == 0 && string.Compare(m_FamilyTypeContainerList[i].FamilyParameterList[j].Value, "1", ignoreCase: true) == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void OnButtonCancelClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
	}

	private bool FindParameter(string _strValue, FamilyTypeContainer _Container)
	{
		string empty = string.Empty;
		for (int i = 0; i < _Container.FamilyParameterList.Count; i++)
		{
			empty = _Container.FamilyParameterList[i].ParameterName;
			if (string.Compare(empty, _strValue, ignoreCase: true) == 0)
			{
				return true;
			}
		}
		return false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.m_ButtonOk = new System.Windows.Forms.Button();
		this.m_ButtonCancel = new System.Windows.Forms.Button();
		this.m_UnitLabel = new System.Windows.Forms.Label();
		this.m_StreetTabControl = new System.Windows.Forms.TabControl();
		this.m_SlopeComboBox = new System.Windows.Forms.ComboBox();
		base.SuspendLayout();
		this.m_ButtonOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_ButtonOk.Location = new System.Drawing.Point(299, 344);
		this.m_ButtonOk.Name = "m_ButtonOk";
		this.m_ButtonOk.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonOk.TabIndex = 0;
		this.m_ButtonOk.Text = "&OK";
		this.m_ButtonOk.UseVisualStyleBackColor = true;
		this.m_ButtonOk.Click += new System.EventHandler(this.OnButtonOkClicked);
		this.m_ButtonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_ButtonCancel.Location = new System.Drawing.Point(380, 344);
		this.m_ButtonCancel.Name = "m_ButtonCancel";
		this.m_ButtonCancel.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonCancel.TabIndex = 1;
		this.m_ButtonCancel.Text = "&Cancel";
		this.m_ButtonCancel.UseVisualStyleBackColor = true;
		this.m_ButtonCancel.Click += new System.EventHandler(this.OnButtonCancelClicked);
		this.m_UnitLabel.Location = new System.Drawing.Point(0, 0);
		this.m_UnitLabel.Name = "m_UnitLabel";
		this.m_UnitLabel.Size = new System.Drawing.Size(100, 23);
		this.m_UnitLabel.TabIndex = 0;
		this.m_SlopeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_SlopeComboBox.FormattingEnabled = true;
		this.m_SlopeComboBox.Location = new System.Drawing.Point(190, 53);
		this.m_SlopeComboBox.Name = "m_PhaseComboBox";
		this.m_SlopeComboBox.Size = new System.Drawing.Size(129, 21);
		this.m_SlopeComboBox.TabIndex = 4;
		this.m_StreetTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_StreetTabControl.Location = new System.Drawing.Point(0, 0);
		this.m_StreetTabControl.Name = "m_StreetTabControl";
		this.m_StreetTabControl.SelectedIndex = 0;
		this.m_StreetTabControl.Size = new System.Drawing.Size(200, 100);
		this.m_StreetTabControl.TabIndex = 0;
		this.m_StreetTabControl.SelectedIndexChanged += new System.EventHandler(this.StreetTabControlTabIndexChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(456, 379);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Name = "AddModifyFamilyForm";
		this.Text = "AddModifyFamily";
		base.ResumeLayout(false);
		base.AcceptButton = this.m_ButtonOk;
		base.CancelButton = this.m_ButtonCancel;
	}
}
