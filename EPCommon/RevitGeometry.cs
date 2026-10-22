using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.Revit.DB;

namespace EPCommon;

public class RevitGeometry
{
	private static List<UnitAbbrContainer> m_UnitAbbrContainer = null;

	private static DisplayUnitType m_UnitBaseUnit = DisplayUnitType.DUT_UNDEFINED;

	private static bool FindFeetAndInchSymbol(string _strValue)
	{
		bool result = true;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = true;
		bool flag6 = false;
		for (int i = 0; i < _strValue.Length; i++)
		{
			switch (_strValue[i])
			{
			case '"':
				if (i == 0 || flag4 || !flag5 || (flag6 && !flag3))
				{
					result = false;
					break;
				}
				flag4 = true;
				flag5 = false;
				break;
			case '\'':
				if (i == 0 || flag2 || flag4 || !flag5 || (flag6 && !flag3))
				{
					result = false;
					break;
				}
				flag2 = true;
				flag5 = false;
				if (flag || flag3)
				{
					flag4 = true;
				}
				break;
			default:
				result = false;
				break;
			}
		}
		if (!flag4 && flag6 && !flag3 && !flag5)
		{
			result = false;
		}
		return result;
	}

	public static bool StrE_GetValidSlope(string _strInput, DisplayUnitType _BaseUnit, DisplayUnit _UnitType, ref double _dReturnedDouble)
	{
		return GetCurrentSlopeUnits(_strInput, _BaseUnit, _UnitType, ref _dReturnedDouble);
	}

	public static double GetSlopeAccuracy(DisplayUnitType dut)
	{
		return dut switch
		{
			DisplayUnitType.DUT_RISE_OVER_INCHES => 1.0 / 256.0, 
			DisplayUnitType.DUT_RISE_OVER_FOOT => 0.0003255208333333333, 
			_ => 0.0001, 
		};
	}

	public static double GetDirectShapeVolume(DirectShape ds)
	{
		double num = 0.0;
		GeometryElement geometryElement = ((Element)ds).get_Geometry(new Options());
		foreach (GeometryObject item in geometryElement)
		{
			if (item is Solid)
			{
				num += (item as Solid).Volume;
			}
		}
		return num;
	}

	private static bool CheckForValidSlopeInput(string _strInput, DisplayUnitType _BaseUnit, DisplayUnit _UnitType)
	{
		bool flag = StrE_IsValidArchInput(_strInput);
		switch (_BaseUnit)
		{
		case DisplayUnitType.DUT_PERCENTAGE:
		case DisplayUnitType.DUT_RATIO_10:
		case DisplayUnitType.DUT_RATIO_12:
		case DisplayUnitType.DUT_SLOPE_DEGREES:
			if (flag)
			{
				return false;
			}
			break;
		}
		return true;
	}

	private static bool GetCurrentSlopeUnits(string _strInput, DisplayUnitType _BaseUnit, DisplayUnit _UnitType, ref double _dValue)
	{
		int num = -1;
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (string.IsNullOrEmpty(_strInput))
		{
			return false;
		}
		if (_strInput.IndexOf(":12") > 0)
		{
			num = _strInput.IndexOf(":12");
			empty2 = ":12";
			empty = _strInput.Substring(0, num);
		}
		else if (_strInput.IndexOf(":10") > 0)
		{
			num = _strInput.IndexOf(":10");
			empty2 = ":10";
			empty = _strInput.Substring(0, num);
		}
		else if (_strInput.IndexOf("/ 12\"") > 0)
		{
			num = _strInput.LastIndexOf("/ 12\"");
			empty2 = "/ 12\"";
			empty = _strInput.Substring(0, num);
		}
		else if (_strInput.IndexOf("/ 1' - 0\"") > 0)
		{
			num = _strInput.IndexOf("/ 1' - 0\"");
			empty2 = "/ 1' - 0\"";
			empty = _strInput.Substring(0, num);
		}
		else if (_strInput.IndexOf("/ 1000") > 0)
		{
			num = _strInput.IndexOf("/ 1000");
			empty2 = "/ 1000";
			empty = _strInput.Substring(0, num);
		}
		else if (_strInput.IndexOf("mm") > 0)
		{
			num = _strInput.IndexOf("mm");
			empty2 = "/ 1000";
			empty = _strInput.Substring(0, num);
		}
		else if (_strInput.IndexOf("°") > 0)
		{
			num = _strInput.IndexOf("°");
			empty2 = "°";
			empty = _strInput.Substring(0, num);
		}
		else if (_strInput.IndexOf("%") > 0)
		{
			num = _strInput.IndexOf("%");
			empty2 = "%";
			empty = _strInput.Substring(0, num);
		}
		else if (_strInput.IndexOf("m") > 0)
		{
			num = _strInput.IndexOf("m");
			empty2 = "m";
			empty = _strInput.Substring(0, num);
		}
		else
		{
			empty2 = string.Empty;
			empty = _strInput;
		}
		if (empty2 == string.Empty && empty == string.Empty)
		{
			return false;
		}
		try
		{
			if (string.Compare(empty2, ":12", ignoreCase: true) == 0)
			{
				_dValue = ValueConversion.ToDouble(empty) / 12.0;
			}
			else if (string.Compare(empty2, ":10", ignoreCase: true) == 0)
			{
				_dValue = ValueConversion.ToDouble(empty) / 10.0;
			}
			else if (string.Compare(empty2, "/ 12\"", ignoreCase: true) == 0)
			{
				StrE_GetValidArchNumber(empty, _BaseUnit, _UnitType, ref _dValue);
			}
			else if (string.Compare(empty2, "/ 1' - 0\"", ignoreCase: true) == 0)
			{
				StrE_GetValidArchNumber(empty, _BaseUnit, _UnitType, ref _dValue);
			}
			else if (string.Compare(empty2, "/ 1000", ignoreCase: true) == 0)
			{
				_dValue = ValueConversion.ToDouble(empty) / 304.8;
			}
			else if (string.Compare(empty2, "°", ignoreCase: true) == 0)
			{
				double num2 = 0.0;
				double num3 = 0.0;
				_dValue = DecimalDegreesToRadians(ValueConversion.ToDouble(empty));
				if (RevitDefines.EQ(_dValue, Math.PI / 2.0) || RevitDefines.EQ(_dValue, 4.71238898038469))
				{
					num2 = 0.0;
					num3 = double.MaxValue;
					_dValue = num3 / num2;
				}
				else
				{
					num2 = 1.0;
					num3 = Math.Tan(_dValue);
					if (RevitDefines.EQ(_dValue, 0.0) || RevitDefines.EQ(_dValue, Math.PI))
					{
						_dValue = num2 / num3;
					}
					else
					{
						_dValue = num3 / num2;
					}
				}
			}
			else if (string.Compare(empty2, "%", ignoreCase: true) == 0)
			{
				_dValue = ValueConversion.ToDouble(empty) / 100.0;
			}
			else if (string.Compare(empty2, "m", ignoreCase: true) == 0)
			{
				_dValue = ValueConversion.ToDouble(empty) / 0.3048;
			}
			else if (empty2 == string.Empty)
			{
				if (FindFeetAndInchSymbol(empty) && StrE_GetValidArchNumber(empty, _BaseUnit, _UnitType, ref _dValue))
				{
					return true;
				}
				switch (_BaseUnit)
				{
				case DisplayUnitType.DUT_RATIO_10:
					_dValue = ValueConversion.ToDouble(empty) / 10.0;
					break;
				case DisplayUnitType.DUT_RATIO_12:
					_dValue = ValueConversion.ToDouble(empty) / 12.0;
					break;
				case DisplayUnitType.DUT_RISE_OVER_FOOT:
					StrE_GetValidArchNumber(empty, _BaseUnit, _UnitType, ref _dValue);
					_dValue /= 1.0;
					break;
				case DisplayUnitType.DUT_RISE_OVER_INCHES:
					StrE_GetValidArchNumber(empty, _BaseUnit, _UnitType, ref _dValue);
					break;
				case DisplayUnitType.DUT_RISE_OVER_MMS:
					_dValue = ValueConversion.ToDouble(empty) / 304.8;
					break;
				case DisplayUnitType.DUT_SLOPE_DEGREES:
				{
					double num4 = 0.0;
					double num5 = 0.0;
					_dValue = DecimalDegreesToRadians(ValueConversion.ToDouble(empty));
					if (_dValue >= 0.0 && _dValue <= Math.PI)
					{
						if (RevitDefines.EQ(_dValue, Math.PI / 2.0) || RevitDefines.EQ(_dValue, 4.71238898038469))
						{
							num4 = 0.0;
							num5 = double.MaxValue;
							_dValue = num5 / num4;
							break;
						}
						num4 = 1.0;
						num5 = Math.Tan(_dValue);
						if (RevitDefines.EQ(_dValue, 0.0) || RevitDefines.EQ(_dValue, Math.PI))
						{
							_dValue = num4 / num5;
						}
						else
						{
							_dValue = num5 / num4;
						}
					}
					else
					{
						_dValue = 0.0;
					}
					break;
				}
				case DisplayUnitType.DUT_PERCENTAGE:
					_dValue = ValueConversion.ToDouble(empty) / 100.0;
					break;
				}
			}
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private static string FindSlopeStringToDisplay(double _dConvertedDouble, double _dDisplayPrecision, UnitSymbolType _UnitSymbolType, DecimalSymbol _DecimalSymbolType)
	{
		string empty = string.Empty;
		string text = ValueConversion.ToString(_dDisplayPrecision);
		int num = text.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		if (num >= 0 && num < text.Length - 1)
		{
			int precision = text.Length - num - 1;
			_dConvertedDouble = eplCalE_RoundValue(_dConvertedDouble, precision);
			empty = ValueConversion.Format(_DecimalSymbolType, "{0:f" + precision + "}", _dConvertedDouble);
			return ApplyUnitToSlopeString(empty, _UnitSymbolType);
		}
		int num2 = text.IndexOf('-');
		if (num2 >= 0 && num2 < text.Length - 1)
		{
			bool flag = true;
			string text2 = text.Substring(num2 + 1);
			for (int i = 0; i < text2.Length; i++)
			{
				if (!char.IsDigit(text2[i]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				int precision2 = Convert.ToInt32(text2);
				_dConvertedDouble = eplCalE_RoundValue(_dConvertedDouble, precision2);
				empty = ValueConversion.Format(_DecimalSymbolType, "{0:f" + precision2 + "}", _dConvertedDouble);
				return ApplyUnitToSlopeString(empty, _UnitSymbolType);
			}
		}
		empty = ValueConversion.ToString(_DecimalSymbolType, _dConvertedDouble);
		return ApplyUnitToSlopeString(empty, _UnitSymbolType);
	}

	private static string ApplyUnitToSlopeString(string _strToDisplay, UnitSymbolType _UnitSymbolType)
	{
		string text = _strToDisplay;
		switch (_UnitSymbolType)
		{
		case UnitSymbolType.UST_COLON_10:
			text += " :10";
			break;
		case UnitSymbolType.UST_COLON_12:
			text += ":12";
			break;
		case UnitSymbolType.UST_SLOPE_DEGREE_SYMBOL:
			text += " °";
			break;
		case UnitSymbolType.UST_PERCENT_SIGN:
			text += "%";
			break;
		}
		return text;
	}

	public static bool Units_ConvertStringToDouble(string _strInput, Units _Units, UnitType _UnitType, ref double _dReturnedDouble)
	{
		return Units_ConvertStringToDouble(_strInput, _Units, _UnitType, ref _dReturnedDouble, _bDisplayInvalidInputMessage: true);
	}

	public static bool Units_ConvertStringToDouble(string _strInput, Units _Units, UnitType _UnitType, ref double _dReturnedDouble, bool _bDisplayInvalidInputMessage)
	{
		return Units_ConvertStringToDouble(_strInput, _Units, _UnitType, ref _dReturnedDouble, double.MinValue, _bLowerLimitInclusive: true, double.MaxValue, _bUpperLimitInclusive: true, _bDisplayInvalidInputMessage);
	}

	public static bool Units_ConvertStringToDouble(string _strInput, Units _Units, UnitType _UnitType, ref double _dReturnedDouble, double _dLowerLimit, bool _bLowerLimitInclusive, double _dUpperLimit, bool _bUpperLimitInclusive, bool _bDisplayInvalidInputMessage)
	{
		double value = 0.0;
		if (!LegacyUnits.TryParse(_Units, _UnitType, _strInput, out value))
		{
			return false;
		}
		if (_bLowerLimitInclusive)
		{
			if (value < _dLowerLimit)
			{
				return false;
			}
		}
		else if (value <= _dLowerLimit)
		{
			return false;
		}
		if (_bUpperLimitInclusive)
		{
			if (value > _dUpperLimit)
			{
				return false;
			}
		}
		else if (value >= _dUpperLimit)
		{
			return false;
		}
		_dReturnedDouble = value;
		return true;
	}

	public static void Units_ConvertDoubleToString(double _dConvertedDouble, DisplayUnitType _UnitType, double _dDisplayPrecision, UnitSymbolType _UnitSymbolType, Units _Units, ref string _strToDisplay, bool forEditing = true)
	{
		FormatValueOptions formatValueOptions = new FormatValueOptions();
		formatValueOptions.AppendUnitSymbol = true;
		FormatOptions formatOptions = LegacyUnits.CreateFormatOptions(_UnitType, _UnitSymbolType);
		formatOptions.UseDefault = false;
		formatOptions.SetLegacyDisplayUnits(_UnitType);
		formatOptions.SetLegacyUnitSymbol(_UnitSymbolType);
		formatOptions.Accuracy = _dDisplayPrecision;
		formatOptions.SuppressSpaces = false;
		formatOptions.SuppressTrailingZeros = false;
		formatOptions.UseDigitGrouping = true;
		formatOptions.UsePlusPrefix = false;
		formatValueOptions.SetFormatOptions(formatOptions);
		switch (_UnitType)
		{
		case DisplayUnitType.DUT_METERS:
		case DisplayUnitType.DUT_CENTIMETERS:
		case DisplayUnitType.DUT_MILLIMETERS:
		case DisplayUnitType.DUT_DECIMAL_FEET:
		case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
		case DisplayUnitType.DUT_FRACTIONAL_INCHES:
		case DisplayUnitType.DUT_DECIMAL_INCHES:
		case DisplayUnitType.DUT_METERS_CENTIMETERS:
			_strToDisplay = LegacyUnits.Format(_Units, UnitType.UT_Length, _dConvertedDouble, maxAccuracy: false, forEditing, formatValueOptions);
			break;
		case DisplayUnitType.DUT_ACRES:
		case DisplayUnitType.DUT_HECTARES:
		case DisplayUnitType.DUT_SQUARE_FEET:
		case DisplayUnitType.DUT_SQUARE_METERS:
		case DisplayUnitType.DUT_SQUARE_INCHES:
		case DisplayUnitType.DUT_SQUARE_CENTIMETERS:
		case DisplayUnitType.DUT_SQUARE_MILLIMETERS:
			_strToDisplay = LegacyUnits.Format(_Units, UnitType.UT_Area, _dConvertedDouble, maxAccuracy: false, forEditing, formatValueOptions);
			break;
		case DisplayUnitType.DUT_CUBIC_YARDS:
		case DisplayUnitType.DUT_CUBIC_FEET:
		case DisplayUnitType.DUT_CUBIC_METERS:
		case DisplayUnitType.DUT_LITERS:
		case DisplayUnitType.DUT_GALLONS_US:
			_strToDisplay = LegacyUnits.Format(_Units, UnitType.UT_Volume, _dConvertedDouble, maxAccuracy: false, forEditing, formatValueOptions);
			break;
		case DisplayUnitType.DUT_PERCENTAGE:
		case DisplayUnitType.DUT_RATIO_10:
		case DisplayUnitType.DUT_RATIO_12:
		case DisplayUnitType.DUT_SLOPE_DEGREES:
		case DisplayUnitType.DUT_RISE_OVER_INCHES:
		case DisplayUnitType.DUT_RISE_OVER_FOOT:
		case DisplayUnitType.DUT_RISE_OVER_MMS:
			_strToDisplay = LegacyUnits.Format(_Units, UnitType.UT_Slope, _dConvertedDouble, maxAccuracy: false, forEditing, formatValueOptions);
			break;
		}
	}

	private static string ApplyUnitToString(string _strToDisplay, UnitSymbolType _UnitSymbolType)
	{
		return _strToDisplay;
	}

	private static string FindStringToDisplay(double _dConvertedDouble, double _dDisplayPrecision, UnitSymbolType _UnitSymbolType, DecimalSymbol _DecimalSymbolType)
	{
		string empty = string.Empty;
		string text = ValueConversion.ToString(_dDisplayPrecision);
		int num = text.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		if (num >= 0 && num < text.Length - 1)
		{
			int precision = text.Length - num - 1;
			_dConvertedDouble = eplCalE_RoundValue(_dConvertedDouble, precision);
			empty = ValueConversion.Format(_DecimalSymbolType, "{0:f" + precision + "}", _dConvertedDouble);
			return ApplyUnitToString(empty, _UnitSymbolType);
		}
		int num2 = text.IndexOf('-');
		if (num2 >= 0 && num2 < text.Length - 1)
		{
			bool flag = true;
			string text2 = text.Substring(num2 + 1);
			for (int i = 0; i < text2.Length; i++)
			{
				if (!char.IsDigit(text2[i]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				int precision2 = Convert.ToInt32(text2);
				_dConvertedDouble = eplCalE_RoundValue(_dConvertedDouble, precision2);
				empty = ValueConversion.Format(_DecimalSymbolType, "{0:f" + precision2 + "}", _dConvertedDouble);
				return ApplyUnitToString(empty, _UnitSymbolType);
			}
		}
		empty = ValueConversion.ToString(_DecimalSymbolType, _dConvertedDouble);
		return ApplyUnitToString(empty, _UnitSymbolType);
	}

	private static string FindStringToDisplay(double _dConvertedDouble, double _dDisplayPrecision, DecimalSymbol _DecimalSymbolType)
	{
		string empty = string.Empty;
		string text = ValueConversion.ToString(_dDisplayPrecision);
		int num = text.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		if (num >= 0 && num < text.Length - 1)
		{
			int precision = text.Length - num - 1;
			_dConvertedDouble = eplCalE_RoundValue(_dConvertedDouble, precision);
			return ValueConversion.Format(_DecimalSymbolType, "{0:f" + precision + "}", _dConvertedDouble);
		}
		int num2 = text.IndexOf('-');
		if (num2 >= 0 && num2 < text.Length - 1)
		{
			bool flag = true;
			string text2 = text.Substring(num2 + 1);
			for (int i = 0; i < text2.Length; i++)
			{
				if (!char.IsDigit(text2[i]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				int precision2 = Convert.ToInt32(text2);
				_dConvertedDouble = eplCalE_RoundValue(_dConvertedDouble, precision2);
				return ValueConversion.Format(_DecimalSymbolType, "{0:f" + precision2 + "}", _dConvertedDouble);
			}
		}
		return ValueConversion.ToString(_DecimalSymbolType, _dConvertedDouble);
	}

	private static void eplStrE_FloatToArch(double ArchFloat, ref string OutputStr, bool ShowInch)
	{
		int num = 0;
		int num2 = 0;
		bool flag = true;
		int num3 = 0;
		int num4 = 256;
		double num5 = 0.0;
		double num6 = 0.0;
		string empty = string.Empty;
		string text = string.Empty;
		if (RevitDefines.EQ(ArchFloat, 0.0))
		{
			OutputStr = "0\"";
			return;
		}
		if (ArchFloat < 0.0)
		{
			ArchFloat *= -1.0;
			flag = false;
		}
		if (!ShowInch)
		{
			num6 = ArchFloat / 12.0;
			num = (int)num6;
			num6 = num;
			num5 = ArchFloat - num6 * 12.0;
		}
		else
		{
			num5 = ArchFloat;
		}
		num2 = (int)num5;
		num6 = num;
		num5 -= (double)num2;
		num6 = num5 * 256.0;
		num3 = ((!(num6 - (double)(int)num6 >= 0.5)) ? ((int)num6) : ((int)num6 + 1));
		if (num3 == 256)
		{
			num3 = 0;
			num2++;
		}
		if ((double)num2 == 12.0)
		{
			num2 = 0;
			num++;
		}
		if (num3 > 0)
		{
			while (num3 % 2 == 0 && num4 % 2 == 0)
			{
				num3 = (int)((double)num3 / 2.0);
				num4 = (int)((double)num4 / 2.0);
			}
		}
		if (num > 0 || num2 > 0 || num3 > 0)
		{
			if (!flag)
			{
				text += "-";
			}
			if (num > 0)
			{
				empty = Convert.ToString(num) + "'";
				text += empty;
			}
			if (num > 0)
			{
				text += "-";
				empty = Convert.ToString(num2);
				text += empty;
			}
			else if (num2 > 0)
			{
				empty = Convert.ToString(num2);
				text += empty;
			}
			if (num3 > 0)
			{
				if (num2 > 0 || num > 0)
				{
					text += " ";
				}
				empty = Convert.ToString(num3) + "/" + Convert.ToString(num4);
				text += empty;
			}
		}
		if (text == string.Empty)
		{
			text = "0";
		}
		text += "\"";
		OutputStr = text;
	}

	public static bool StrE_GetValidLinearMixedUnitNumber(string _strInput, DisplayUnitType _BaseUnit, DisplayUnit _UnitType, ref double _dValue)
	{
		string text = _strInput;
		string text2 = string.Empty;
		bool flag = true;
		int num = -1;
		int num2 = -1;
		text = text.Trim();
		text = text.ToLower();
		if (m_UnitAbbrContainer == null)
		{
			m_UnitAbbrContainer = new List<UnitAbbrContainer>();
		}
		if (m_UnitBaseUnit != _BaseUnit)
		{
			m_UnitAbbrContainer.Clear();
			switch (_BaseUnit)
			{
			case DisplayUnitType.DUT_MILLIMETERS:
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("LF", 1.0, 304.8));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("mm", 0.0032808398950131233, 1.0));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("cm", 25.0 / 762.0, 10.0));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("m", 3.280839895013123, 1000.0));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("'", 1.0, 304.8));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("\"", 1.0 / 12.0, 25.4));
				break;
			case DisplayUnitType.DUT_CENTIMETERS:
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("LF", 1.0, 30.48));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("mm", 0.0032808398950131233, 0.1));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("cm", 25.0 / 762.0, 1.0));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("m", 3.280839895013123, 100.0));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("'", 1.0, 30.48));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("\"", 1.0 / 12.0, 2.54));
				break;
			case DisplayUnitType.DUT_METERS:
			case DisplayUnitType.DUT_METERS_CENTIMETERS:
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("LF", 1.0, 0.3048));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("mm", 0.0032808398950131233, 0.001));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("cm", 25.0 / 762.0, 0.01));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("m", 3.280839895013123, 1.0));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("'", 1.0, 0.3048));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("\"", 1.0 / 12.0, 0.0254));
				break;
			case DisplayUnitType.DUT_FRACTIONAL_INCHES:
			case DisplayUnitType.DUT_DECIMAL_INCHES:
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("LF", 1.0 / 12.0, 0.3048));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("mm", 5.0 / 127.0, 0.001));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("cm", 50.0 / 127.0, 0.01));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("m", 39.37007874015748, 1.0));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("'", 1.0 / 12.0, 0.3048));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("\"", 1.0, 0.0254));
				break;
			case DisplayUnitType.DUT_DECIMAL_FEET:
			case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("LF", 1.0, 0.3048));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("mm", 0.0032808398950131233, 0.001));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("cm", 0.0032808398950131233, 0.01));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("m", 3.280839895013123, 1.0));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("'", 1.0, 0.3048));
				m_UnitAbbrContainer.Add(new UnitAbbrContainer("\"", 12.0, 0.0254));
				break;
			}
			m_UnitBaseUnit = _BaseUnit;
		}
		for (num2 = 0; num2 < text.Length && (char.IsDigit(text[num2]) || text[num2] == '.' || text[num2] == '-' || text[num2] == ','); num2++)
		{
			text2 += text[num2];
		}
		if (text2 != string.Empty)
		{
			_dValue = ValueConversion.ToDouble(text2);
		}
		for (num = 0; num < m_UnitAbbrContainer.Count(); num++)
		{
			if ((num2 = text.IndexOf(m_UnitAbbrContainer[num].Abbreviation)) != -1 && num2 + m_UnitAbbrContainer[num].Abbreviation.Length == text.Length)
			{
				string text3 = text.Substring(0, num2);
				if (!(flag = StrE_GetValidRealNumber(text3, ref _dValue, double.MinValue, _bLowerLimitInclusive: true, double.MaxValue, _bUpperLimitInclusive: true)))
				{
					return flag;
				}
				_dValue = ValueConversion.ToDouble(text3);
				if (_UnitType == DisplayUnit.IMPERIAL)
				{
					_dValue *= m_UnitAbbrContainer[num].EnglishFactor;
				}
				else
				{
					_dValue *= m_UnitAbbrContainer[num].MetricFactor;
				}
				return true;
			}
		}
		return StrE_GetValidRealNumber(text, ref _dValue, double.MinValue, _bLowerLimitInclusive: true, double.MaxValue, _bUpperLimitInclusive: true);
	}

	public static bool StrE_GetValidWholeNumber(string _strInput, ref int _nReturnedInt)
	{
		return StrE_GetValidWholeNumber(_strInput, ref _nReturnedInt, int.MinValue, int.MaxValue);
	}

	private static bool StrE_GetValidWholeNumber(string _strInput, ref int _nReturnedInt, int _nLowerLimit, int _nUpperLimit)
	{
		string text = _strInput;
		int num = 0;
		text = text.Trim();
		if (text == string.Empty)
		{
			return false;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (!char.IsDigit(text[i]))
			{
				return false;
			}
		}
		num = Convert.ToInt32(text);
		if (num < _nLowerLimit)
		{
			return false;
		}
		if (num > _nUpperLimit)
		{
			return false;
		}
		_nReturnedInt = num;
		return true;
	}

	public static bool StrE_GetValidRealNumber(string _strInput, ref double _dReturnedDouble)
	{
		return StrE_GetValidRealNumber(_strInput, ref _dReturnedDouble, double.MinValue, _bLowerLimitInclusive: true, double.MaxValue, _bUpperLimitInclusive: true);
	}

	private static bool StrE_GetValidRealNumber(string _strInput, ref double _dReturnedDouble, double _dLowerLimit, bool _bLowerLimitInclusive, double _dUpperLimit, bool _bUpperLimitInclusive)
	{
		string text = _strInput;
		double num = 0.0;
		text = text.Trim();
		if (text == string.Empty)
		{
			return false;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (!char.IsDigit(text[i]) && text[i] != '.' && text[i] != '-' && text[i] != ',')
			{
				return false;
			}
		}
		num = ValueConversion.ToDouble(text);
		if (_bLowerLimitInclusive)
		{
			if (num < _dLowerLimit)
			{
				return false;
			}
		}
		else if (num <= _dLowerLimit)
		{
			return false;
		}
		if (_bUpperLimitInclusive)
		{
			if (num > _dUpperLimit)
			{
				return false;
			}
		}
		else if (num >= _dUpperLimit)
		{
			return false;
		}
		_dReturnedDouble = num;
		return true;
	}

	public static bool StrE_GetValidArchNumber(string _InputString, DisplayUnitType gBaseUnit, DisplayUnit _UnitType, ref double _ReturnedDouble)
	{
		string text = _InputString;
		string text2 = string.Empty;
		string text3 = string.Empty;
		string strValue = string.Empty;
		string strValue2 = string.Empty;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		text = text.Trim();
		if (text == string.Empty)
		{
			return false;
		}
		if (!StrE_IsValidArchInput(text))
		{
			return false;
		}
		for (int i = 0; i < text.Length; i++)
		{
			switch (text[i])
			{
			case ' ':
				text3 = text2;
				text2 = string.Empty;
				break;
			case '"':
				if (num2 > 0.0)
				{
					return false;
				}
				flag3 = true;
				if (flag2)
				{
					strValue2 = text2;
					text2 = string.Empty;
				}
				if (text2 != string.Empty)
				{
					num2 = ValueConversion.ToDouble(text2);
					text2 = string.Empty;
					break;
				}
				if (text3 != string.Empty)
				{
					num3 = ValueConversion.ToDouble(text3);
					text3 = string.Empty;
				}
				num2 = num3;
				if (flag2)
				{
					num4 = ValueConversion.ToDouble(strValue);
					strValue = string.Empty;
					num5 = ValueConversion.ToDouble(strValue2);
					strValue2 = string.Empty;
					num3 = num4 / num5;
					num2 += num3;
				}
				break;
			case '\'':
				flag3 = true;
				if (num > 0.0)
				{
					return false;
				}
				flag4 = true;
				if (flag2)
				{
					strValue2 = text2;
					text2 = string.Empty;
				}
				if (text2 != string.Empty)
				{
					num = ValueConversion.ToDouble(text2);
					text2 = string.Empty;
					break;
				}
				if (text3 != string.Empty)
				{
					num3 = ValueConversion.ToDouble(text3);
					text3 = string.Empty;
				}
				num = num3;
				if (flag2)
				{
					num4 = ValueConversion.ToDouble(strValue);
					strValue = string.Empty;
					num5 = ValueConversion.ToDouble(strValue2);
					strValue2 = string.Empty;
					num3 = num4 / num5;
					num += num3;
				}
				break;
			case '-':
				if (i == 0)
				{
					flag = true;
					break;
				}
				text3 = text2;
				text2 = string.Empty;
				break;
			case ',':
			case '.':
				text2 += text[i];
				break;
			case '/':
				strValue = text2;
				text2 = string.Empty;
				flag2 = true;
				break;
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				text2 += text[i];
				break;
			default:
				return false;
			}
		}
		if (text2 != string.Empty)
		{
			if (flag2)
			{
				strValue2 = text2;
				text2 = string.Empty;
				if (text3 != string.Empty)
				{
					num3 = ValueConversion.ToDouble(text3);
					text3 = string.Empty;
				}
				if (flag3 || gBaseUnit == DisplayUnitType.DUT_FRACTIONAL_INCHES || gBaseUnit == DisplayUnitType.DUT_DECIMAL_INCHES || gBaseUnit == DisplayUnitType.DUT_RISE_OVER_INCHES)
				{
					num2 = num3;
				}
				else
				{
					num = num3;
				}
				num4 = ValueConversion.ToDouble(strValue);
				strValue = string.Empty;
				num5 = ValueConversion.ToDouble(strValue2);
				strValue2 = string.Empty;
				num3 = num4 / num5;
				if (flag3 || gBaseUnit == DisplayUnitType.DUT_FRACTIONAL_INCHES || gBaseUnit == DisplayUnitType.DUT_DECIMAL_INCHES || gBaseUnit == DisplayUnitType.DUT_RISE_OVER_INCHES)
				{
					num2 += num3;
				}
				else
				{
					num += num3;
				}
			}
			else
			{
				if (flag3 || gBaseUnit == DisplayUnitType.DUT_FRACTIONAL_INCHES || gBaseUnit == DisplayUnitType.DUT_DECIMAL_INCHES || gBaseUnit == DisplayUnitType.DUT_RISE_OVER_INCHES)
				{
					num2 = ValueConversion.ToDouble(text2);
				}
				else
				{
					num = ValueConversion.ToDouble(text2);
				}
				text2 = string.Empty;
			}
		}
		if (flag3 || flag4 || gBaseUnit == DisplayUnitType.DUT_FRACTIONAL_INCHES || gBaseUnit == DisplayUnitType.DUT_DECIMAL_INCHES || gBaseUnit == DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES || gBaseUnit == DisplayUnitType.DUT_DECIMAL_FEET || gBaseUnit == DisplayUnitType.DUT_RISE_OVER_INCHES || gBaseUnit == DisplayUnitType.DUT_RISE_OVER_FOOT)
		{
			_ReturnedDouble = num * 12.0 + num2;
			if (_UnitType == DisplayUnit.IMPERIAL)
			{
				_ReturnedDouble *= 1.0 / 12.0;
			}
			else
			{
				_ReturnedDouble *= 0.0254;
			}
		}
		else
		{
			_ReturnedDouble = num;
		}
		if (flag)
		{
			_ReturnedDouble *= -1.0;
		}
		return true;
	}

	private static bool StrE_IsValidArchInput(string _strValue)
	{
		bool result = true;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = true;
		bool flag8 = false;
		for (int i = 0; i < _strValue.Length; i++)
		{
			switch (_strValue[i])
			{
			case ' ':
				if (flag6 || flag || flag2 || flag5 || i == 0)
				{
					result = false;
					break;
				}
				flag = true;
				flag8 = flag7;
				flag7 = false;
				break;
			case '"':
				if (i == 0 || flag6 || !flag7 || (flag8 && !flag5))
				{
					result = false;
					break;
				}
				flag6 = true;
				flag7 = false;
				break;
			case '\'':
				if (i == 0 || flag3 || flag6 || !flag7 || (flag8 && !flag5))
				{
					result = false;
					break;
				}
				flag3 = true;
				flag7 = false;
				if (flag2 || flag5)
				{
					flag6 = true;
				}
				break;
			case '-':
				if (i != 0 && (flag4 || flag6))
				{
					result = false;
				}
				else if (i != 0)
				{
					flag4 = true;
					flag8 = flag7;
					flag7 = false;
				}
				break;
			case ',':
			case '.':
				if (flag2 || flag5 || flag6)
				{
					result = false;
					break;
				}
				flag2 = true;
				flag7 = false;
				break;
			case '/':
				if (flag2 || flag5 || flag6 || i == 0 || !flag7)
				{
					result = false;
					break;
				}
				flag5 = true;
				flag7 = false;
				break;
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				if (flag6)
				{
					result = false;
				}
				flag7 = true;
				break;
			default:
				result = false;
				break;
			}
		}
		if (!flag6 && flag8 && !flag5 && !flag7)
		{
			result = false;
		}
		return result;
	}

	public static int eplStrE_ConvertRadiansToAngleStr(DisplayUnitType _AngularUnits, double _dAngle, double _dRounding, UnitSymbolType _UnitSymbolType, DecimalSymbol _DecimalSymbolType, ref string _strAngleStr)
	{
		int result = 0;
		if (_dRounding > 0.001)
		{
			_dRounding = 0.001;
		}
		int nPrecision = 0;
		string text = ValueConversion.ToString(DecimalSymbol.Dot, _dRounding);
		int num = text.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		if (num >= 0 && num < text.Length - 1)
		{
			nPrecision = text.Length - num - 1;
		}
		else
		{
			int num2 = text.IndexOf('-');
			if (num2 >= 0 && num2 < text.Length - 1)
			{
				bool flag = true;
				string text2 = text.Substring(num2 + 1);
				for (int i = 0; i < text2.Length; i++)
				{
					if (!char.IsDigit(text2[i]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					nPrecision = Convert.ToInt32(text2);
				}
			}
		}
		switch (_AngularUnits)
		{
		case DisplayUnitType.DUT_DEGREES_AND_MINUTES:
			eplStrE_ConvertRadiansToDMSAngleStr(_dAngle, nPrecision, _DecimalSymbolType, ref _strAngleStr);
			break;
		case DisplayUnitType.DUT_DECIMAL_DEGREES:
			eplStrE_ConvertRadiansToDecimalDegreesAngleStr(_dAngle, nPrecision, _UnitSymbolType, _DecimalSymbolType, ref _strAngleStr);
			break;
		default:
			result = 2500;
			break;
		}
		return result;
	}

	public static void RadiansToDMS(double _dRadians, ref int _nSign, ref int _nDegrees, ref int _nMinutes, ref double _dSeconds)
	{
		double num = Math.Abs(_dRadians) / (Math.PI / 180.0);
		_nSign = 1;
		if (_dRadians < 0.0)
		{
			_nSign = -1;
		}
		_nDegrees = (int)num;
		num -= (double)_nDegrees;
		num = Math.Abs(num);
		num *= 60.0;
		_nMinutes = (int)num;
		num -= (double)_nMinutes;
		num *= 60.0;
		_dSeconds = num;
	}

	public static double RadiansToDecimalDegrees(double _dRadians)
	{
		return _dRadians / (Math.PI / 180.0);
	}

	public static double DecimalDegreesToRadians(double _dDecimalDegrees)
	{
		return _dDecimalDegrees * (Math.PI / 180.0);
	}

	public static void DMSToRadians(ref double _dRadians, int _nSign, int _nDegrees, int _nMinutes, double _dSeconds)
	{
		_dRadians = (double)_nSign * (((double)Math.Abs(_nDegrees) + (double)Math.Abs(_nMinutes) / 60.0 + Math.Abs(_dSeconds) / 3600.0) * (Math.PI / 180.0));
	}

	private static void RoundDMSAngle(ref int _nDegrees, ref int _nMinutes, ref double _dSeconds, int _nPrecision)
	{
		_dSeconds = eplCalE_RoundValue(_dSeconds, _nPrecision);
		if (_dSeconds == 60.0)
		{
			_dSeconds = 0.0;
			_nMinutes++;
		}
		if (_nMinutes == 60)
		{
			_nMinutes = 0;
			_nDegrees++;
		}
		if (_nDegrees >= 360)
		{
			_nDegrees -= 360;
		}
	}

	private static void eplStrE_ConvertRadiansToDMSAngleStr(double _dAngle, int _nPrecision, DecimalSymbol _DecimalSymbolType, ref string strAngleString)
	{
		int _nDegrees = 0;
		int _nMinutes = 0;
		double _dSeconds = 0.0;
		strAngleString = string.Empty;
		int _nSign = 1;
		RadiansToDMS(_dAngle, ref _nSign, ref _nDegrees, ref _nMinutes, ref _dSeconds);
		RoundDMSAngle(ref _nDegrees, ref _nMinutes, ref _dSeconds, _nPrecision);
		strAngleString = CreateDMSString(_nDegrees, _nMinutes, _dSeconds, _nPrecision, _DecimalSymbolType);
		if (_nSign != 1)
		{
			strAngleString = "-" + strAngleString;
		}
	}

	private static void eplStrE_ConvertRadiansToDecimalDegreesAngleStr(double _dAngle, int _nPrecision, UnitSymbolType _UnitSymbolType, DecimalSymbol _DecimalSymbolType, ref string strAngleString)
	{
		strAngleString = string.Empty;
		_dAngle = RadiansToDecimalDegrees(_dAngle);
		_dAngle = eplCalE_RoundValue(_dAngle, _nPrecision);
		if (_nPrecision < 0)
		{
			strAngleString = ValueConversion.ToString(_DecimalSymbolType, _dAngle);
		}
		else
		{
			strAngleString = ValueConversion.Format(_DecimalSymbolType, "{0:f" + _nPrecision + "}", _dAngle);
		}
		if (_UnitSymbolType == UnitSymbolType.UST_DEGREE_SYMBOL)
		{
			strAngleString += "°";
		}
	}

	private static string CreateDMSString(int _nDegrees, int _nMinutes, double _dSeconds, int _nPrecision, DecimalSymbol _DecimalSymbolType)
	{
		string empty = string.Empty;
		if (_nPrecision < 0)
		{
			empty = _nDegrees.ToString("00") + "°" + _nMinutes.ToString("00") + "'" + _dSeconds + "\"";
		}
		else
		{
			string text = "";
			for (int i = 0; i < _nPrecision; i++)
			{
				text += "0";
			}
			empty = _nDegrees.ToString("00") + "°" + _nMinutes.ToString("00") + "'" + _dSeconds.ToString("00." + text) + "\"";
		}
		switch (_DecimalSymbolType)
		{
		case DecimalSymbol.Dot:
			empty = empty.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
			break;
		case DecimalSymbol.Comma:
			empty = empty.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ",");
			break;
		}
		return empty;
	}

	public static double eplGeoE_ConvertAngleToRadians(string _strDirection)
	{
		double _dRadians = 0.0;
		_strDirection = _strDirection.Replace(" ", string.Empty);
		if (_strDirection == string.Empty)
		{
			return _dRadians;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string text = "0";
		string text2 = "0";
		string text3 = "0";
		int nSign = 1;
		int _nReturnedInt = 0;
		int _nReturnedInt2 = 0;
		double _dReturnedDouble = 0.0;
		double _dReturnedDouble2 = 0.0;
		int num4 = _strDirection.IndexOf("°");
		int num5 = _strDirection.IndexOf("'");
		int num6 = _strDirection.IndexOf("\"");
		if (_strDirection.IndexOf("-") != -1)
		{
			nSign = -1;
		}
		if (num4 != -1)
		{
			text = _strDirection.Substring(num, num4 - num);
			num2 = num4 + 1;
		}
		else
		{
			num2 = num;
			if (num5 == -1 && num6 == -1 && _strDirection.Length > num)
			{
				text = _strDirection.Substring(num, _strDirection.Length - num);
			}
		}
		if (num5 != -1)
		{
			text2 = _strDirection.Substring(num2, num5 - num2);
			num3 = num5 + 1;
		}
		else
		{
			num3 = num2;
			if (num4 != -1 && num6 == -1 && _strDirection.Length > num2)
			{
				text2 = _strDirection.Substring(num2, _strDirection.Length - num2);
			}
		}
		if (num6 != -1)
		{
			text3 = _strDirection.Substring(num3, num6 - num3);
		}
		else if (num5 != -1 && _strDirection.Length > num3)
		{
			text3 = _strDirection.Substring(num3, _strDirection.Length - num3);
		}
		if (text == string.Empty || text2 == string.Empty || text3 == string.Empty)
		{
			return 0.0;
		}
		if (num5 != -1 || num6 != -1)
		{
			if (!StrE_GetValidWholeNumber(text, ref _nReturnedInt))
			{
				_nReturnedInt = 0;
			}
			if (!StrE_GetValidWholeNumber(text2, ref _nReturnedInt2))
			{
				_nReturnedInt2 = 0;
			}
			if (!StrE_GetValidRealNumber(text3, ref _dReturnedDouble))
			{
				_dReturnedDouble = 0.0;
			}
			DMSToRadians(ref _dRadians, nSign, Math.Abs(_nReturnedInt), Math.Abs(_nReturnedInt2), Math.Abs(_dReturnedDouble));
		}
		else
		{
			if (!StrE_GetValidRealNumber(text, ref _dReturnedDouble2))
			{
				_dReturnedDouble2 = 0.0;
			}
			_dRadians = DecimalDegreesToRadians(_dReturnedDouble2);
		}
		return EPShared.eplGeoE_NormalizeRadians(_dRadians);
	}

	public static void eplGeoE_CalcSweepAngle(int ArcDir, double StartDir, double EndDir, ref double SweepAngle)
	{
		if (ArcDir == -1)
		{
			SweepAngle = EPShared.eplGeoE_NormalizeRadians(StartDir - EndDir);
		}
		else
		{
			SweepAngle = EPShared.eplGeoE_NormalizeRadians(EndDir - StartDir);
		}
	}

	public static int CalcSignedArea(Eps_Point _pPt1, Eps_Point _pPt2, Eps_Point _pPt3)
	{
		double num = 0.0;
		int num2 = 0;
		num = (_pPt3.x - _pPt1.x) * (_pPt2.y - _pPt1.y) - (_pPt2.x - _pPt1.x) * (_pPt3.y - _pPt1.y);
		if (num < 0.0)
		{
			return -1;
		}
		return 1;
	}

	public static double GeoE_CalcSignedArea(Eps_Line pLine, Eps_Point pPoint)
	{
		double num = 0.0;
		return (pPoint.x - pLine.StartPnt.x) * (pLine.EndPnt.y - pLine.StartPnt.y) - (pLine.EndPnt.x - pLine.StartPnt.x) * (pPoint.y - pLine.StartPnt.y);
	}

	public static double ConvertLinearValue(double _dSource, DisplayUnitType _InputLinearDistanceType, DisplayUnitType _OutputLinearDistanceType)
	{
		double num = 1.0;
		switch (_InputLinearDistanceType)
		{
		case DisplayUnitType.DUT_MILLIMETERS:
			switch (_OutputLinearDistanceType)
			{
			case DisplayUnitType.DUT_CENTIMETERS:
				num = 0.1;
				break;
			case DisplayUnitType.DUT_METERS:
			case DisplayUnitType.DUT_METERS_CENTIMETERS:
				num = 0.001;
				break;
			case DisplayUnitType.DUT_FRACTIONAL_INCHES:
			case DisplayUnitType.DUT_DECIMAL_INCHES:
				num = 5.0 / 127.0;
				break;
			case DisplayUnitType.DUT_DECIMAL_FEET:
			case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
				num = 0.0032808398950131233;
				break;
			default:
				num = 1.0;
				break;
			}
			break;
		case DisplayUnitType.DUT_CENTIMETERS:
			switch (_OutputLinearDistanceType)
			{
			case DisplayUnitType.DUT_MILLIMETERS:
				num = 10.0;
				break;
			case DisplayUnitType.DUT_METERS:
			case DisplayUnitType.DUT_METERS_CENTIMETERS:
				num = 0.01;
				break;
			case DisplayUnitType.DUT_FRACTIONAL_INCHES:
			case DisplayUnitType.DUT_DECIMAL_INCHES:
				num = 0.39370078740157477;
				break;
			case DisplayUnitType.DUT_DECIMAL_FEET:
			case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
				num = 25.0 / 762.0;
				break;
			default:
				num = 1.0;
				break;
			}
			break;
		case DisplayUnitType.DUT_METERS:
		case DisplayUnitType.DUT_METERS_CENTIMETERS:
			switch (_OutputLinearDistanceType)
			{
			case DisplayUnitType.DUT_MILLIMETERS:
				num = 1000.0;
				break;
			case DisplayUnitType.DUT_CENTIMETERS:
				num = 100.0;
				break;
			case DisplayUnitType.DUT_FRACTIONAL_INCHES:
			case DisplayUnitType.DUT_DECIMAL_INCHES:
				num = 39.37007874015748;
				break;
			case DisplayUnitType.DUT_DECIMAL_FEET:
			case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
				num = 3.280839895013123;
				break;
			default:
				num = 1.0;
				break;
			}
			break;
		case DisplayUnitType.DUT_FRACTIONAL_INCHES:
		case DisplayUnitType.DUT_DECIMAL_INCHES:
			switch (_OutputLinearDistanceType)
			{
			case DisplayUnitType.DUT_MILLIMETERS:
				num = 25.4;
				break;
			case DisplayUnitType.DUT_CENTIMETERS:
				num = 2.54;
				break;
			case DisplayUnitType.DUT_METERS:
			case DisplayUnitType.DUT_METERS_CENTIMETERS:
				num = 0.0254;
				break;
			case DisplayUnitType.DUT_DECIMAL_FEET:
			case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
				num = 1.0 / 12.0;
				break;
			default:
				num = 1.0;
				break;
			}
			break;
		case DisplayUnitType.DUT_DECIMAL_FEET:
		case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
			switch (_OutputLinearDistanceType)
			{
			case DisplayUnitType.DUT_MILLIMETERS:
				num = 304.8;
				break;
			case DisplayUnitType.DUT_CENTIMETERS:
				num = 30.48;
				break;
			case DisplayUnitType.DUT_METERS:
			case DisplayUnitType.DUT_METERS_CENTIMETERS:
				num = 0.3048;
				break;
			case DisplayUnitType.DUT_FRACTIONAL_INCHES:
			case DisplayUnitType.DUT_DECIMAL_INCHES:
				num = 12.0;
				break;
			default:
				num = 1.0;
				break;
			}
			break;
		}
		return _dSource * num;
	}

	public static double eplCalE_RoundValue(double Value, int Precision)
	{
		double num = 0.0;
		double num2 = 0.0;
		num = Math.Pow(10.0, Precision);
		return eplCalE_floor(Value * num + 0.5, 0.0) / num;
	}

	public static double eplCalE_floor(double Operand, double Tolerance)
	{
		return Math.Floor(Operand + Tolerance);
	}

	public static Eps_Leg GetLegFromElementList(Eps_Leg _CurrentLeg, Element _LocationElement)
	{
		bool flag = true;
		if (_CurrentLeg != null)
		{
			flag = false;
		}
		_CurrentLeg = GetLegFromElementListAtJoin(_CurrentLeg, _LocationElement, 0);
		_CurrentLeg = GetLegFromElementListAtJoin(_CurrentLeg, _LocationElement, 1);
		if (_CurrentLeg != null && flag && _LocationElement.Location != null)
		{
			LocationCurve locationCurve = null;
			if (!(_LocationElement.Location is LocationCurve locationCurve2))
			{
				return null;
			}
			Eps_Point point = new Eps_Point();
			Eps_Point point2 = new Eps_Point();
			if (locationCurve2.Curve is Arc)
			{
				Eps_Arc eps_Arc = new Eps_Arc((Arc)locationCurve2.Curve);
				point = eps_Arc.StartPnt;
				point2 = eps_Arc.EndPnt;
			}
			else if (locationCurve2.Curve is Line)
			{
				Eps_Line eps_Line = new Eps_Line((Line)locationCurve2.Curve);
				point = eps_Line.StartPnt;
				point2 = eps_Line.EndPnt;
			}
			double _dStation = 0.0;
			double _dStation2 = 0.0;
			double _dOffset = 0.0;
			double _dOffset2 = 0.0;
			eplGeoE_CalcStationOffsetAlongLegList(point, _CurrentLeg, ref _dStation, ref _dOffset);
			eplGeoE_CalcStationOffsetAlongLegList(point2, _CurrentLeg, ref _dStation2, ref _dOffset2);
			double num = 0.0;
			num = ((!(_dStation <= _dStation2)) ? _dStation2 : _dStation);
			Eps_Leg eps_Leg = _CurrentLeg.Reverse();
			eplGeoE_CalcStationOffsetAlongLegList(point, eps_Leg, ref _dStation, ref _dOffset);
			eplGeoE_CalcStationOffsetAlongLegList(point2, eps_Leg, ref _dStation2, ref _dOffset2);
			if (_dStation < num || _dStation2 < num)
			{
				_CurrentLeg = eps_Leg;
			}
		}
		return _CurrentLeg;
	}

	public static Eps_Leg GetLegFromSpline(Element _LocationElement, Curve _SplineCurve, bool _bReverseOrder)
	{
		Eps_Leg eps_Leg = null;
		int num = 0;
		IList<XYZ> list = _SplineCurve.Tessellate();
		num = list.Count;
		if (num > 1)
		{
			Eps_Point startPnt = new Eps_Point();
			Eps_Point eps_Point = new Eps_Point();
			Eps_Leg eps_Leg2 = null;
			int num2 = 0;
			if (!_bReverseOrder)
			{
				for (num2 = 0; num2 < num; num2 += 2)
				{
					eps_Point = new Eps_Point(list[num2]);
					if (num2 > 0)
					{
						eps_Leg2 = new Eps_Leg();
						eps_Leg2.Line = new Eps_Line();
						eps_Leg2.Line.StartPnt = startPnt;
						eps_Leg2.Line.EndPnt = eps_Point;
						if (eps_Leg == null)
						{
							eps_Leg = eps_Leg2;
							eps_Leg.Element = _LocationElement;
						}
						else
						{
							eps_Leg.Last.Next = eps_Leg2;
						}
					}
					startPnt = eps_Point;
				}
				if (num2 == num)
				{
					eps_Leg2 = new Eps_Leg();
					eps_Leg2.Line = new Eps_Line();
					eps_Leg2.Line.StartPnt = startPnt;
					eps_Leg2.Line.EndPnt = new Eps_Point(list[num - 1]);
					if (eps_Leg == null)
					{
						eps_Leg = eps_Leg2;
						eps_Leg.Element = _LocationElement;
					}
					else
					{
						eps_Leg.Last.Next = eps_Leg2;
					}
				}
			}
			else
			{
				for (num2 = num - 1; num2 >= 0; num2 -= 2)
				{
					eps_Point = new Eps_Point(list[num2]);
					if (num2 < num - 1)
					{
						eps_Leg2 = new Eps_Leg();
						eps_Leg2.Line = new Eps_Line();
						eps_Leg2.Line.StartPnt = startPnt;
						eps_Leg2.Line.EndPnt = eps_Point;
						if (eps_Leg == null)
						{
							eps_Leg = eps_Leg2;
							eps_Leg.Element = _LocationElement;
						}
						else
						{
							eps_Leg.Last.Next = eps_Leg2;
						}
					}
					startPnt = eps_Point;
				}
				if (num2 == -1)
				{
					eps_Leg2 = new Eps_Leg();
					eps_Leg2.Line = new Eps_Line();
					eps_Leg2.Line.StartPnt = startPnt;
					eps_Leg2.Line.EndPnt = new Eps_Point(list[0]);
					if (eps_Leg == null)
					{
						eps_Leg = eps_Leg2;
						eps_Leg.Element = _LocationElement;
					}
					else
					{
						eps_Leg.Last.Next = eps_Leg2;
					}
				}
			}
		}
		return eps_Leg;
	}

	private static Eps_Leg GetLegFromElementListAtJoin(Eps_Leg _CurrentLeg, Element _LocationElement, int _nIndex)
	{
		ElementArray elementArray = null;
		ElementArrayIterator elementArrayIterator = null;
		Element element = null;
		LocationCurve locationCurve = null;
		Eps_Leg eps_Leg = null;
		Eps_Arc eps_Arc = null;
		Eps_Line eps_Line = null;
		LocationCurve locationCurve2 = null;
		if (_CurrentLeg != null && EPShared.eplGeoE_ArePntsDuplicates2D(_CurrentLeg.LegStartPnt, _CurrentLeg.LegEndPnt))
		{
			return _CurrentLeg;
		}
		if (_LocationElement.Location != null)
		{
			locationCurve2 = _LocationElement.Location as LocationCurve;
			if (locationCurve2 == null)
			{
				return null;
			}
		}
		elementArray = locationCurve2.get_ElementsAtJoin(_nIndex);
		if (elementArray.Size > 0)
		{
			if (_CurrentLeg == null)
			{
				eps_Leg = new Eps_Leg();
				if (locationCurve2.Curve is Arc)
				{
					eps_Leg.Element = _LocationElement;
					eps_Leg.Arc = new Eps_Arc((Arc)locationCurve2.Curve);
				}
				else if (locationCurve2.Curve is Line)
				{
					eps_Leg.Element = _LocationElement;
					eps_Leg.Line = new Eps_Line((Line)locationCurve2.Curve);
				}
				else if (locationCurve2.Curve is NurbSpline || locationCurve2.Curve is HermiteSpline || locationCurve2.Curve is Ellipse)
				{
					eps_Leg = GetLegFromSpline(_LocationElement, locationCurve2.Curve, _bReverseOrder: false);
				}
				_CurrentLeg = eps_Leg;
			}
			elementArrayIterator = elementArray.ForwardIterator();
			elementArrayIterator.Reset();
			while (elementArrayIterator.MoveNext())
			{
				if (!(elementArrayIterator.Current is Element { Location: LocationCurve { Curve: var curve } } element2))
				{
					continue;
				}
				if (curve is Arc)
				{
					eps_Arc = new Eps_Arc((Arc)curve);
					if (_CurrentLeg.IsArcInLeg(eps_Arc))
					{
						continue;
					}
				}
				else if (curve is Line)
				{
					eps_Line = new Eps_Line((Line)curve);
					if (_CurrentLeg.IsLineInLeg(eps_Line))
					{
						continue;
					}
				}
				else if (curve is NurbSpline || curve is HermiteSpline || curve is Ellipse)
				{
					int num = 0;
					IList<XYZ> list = curve.Tessellate();
					num = list.Count;
					if (num > 1 && eplGeoE_IsPointOnLegList2D(new Eps_Point(list[0]), _CurrentLeg) && eplGeoE_IsPointOnLegList2D(new Eps_Point(list[num - 1]), _CurrentLeg))
					{
						continue;
					}
				}
				bool flag = false;
				if (_CurrentLeg.LegStartPnt.IsEqual(new Eps_Point(curve.GetEndPoint(0))))
				{
					eps_Leg = new Eps_Leg();
					if (curve is Arc)
					{
						eps_Leg.Element = element2;
						eps_Leg.Arc = eps_Arc.Reverse();
					}
					else if (curve is Line)
					{
						eps_Leg.Element = element2;
						eps_Leg.Line = eps_Line.Reverse();
					}
					else if (curve is NurbSpline || curve is HermiteSpline || curve is Ellipse)
					{
						eps_Leg = GetLegFromSpline(element2, curve, _bReverseOrder: true);
					}
					eps_Leg.Last.Next = _CurrentLeg;
					_CurrentLeg = eps_Leg;
					flag = true;
				}
				else if (_CurrentLeg.LegStartPnt.IsEqual(new Eps_Point(curve.GetEndPoint(1))))
				{
					eps_Leg = new Eps_Leg();
					if (curve is Arc)
					{
						eps_Leg.Element = element2;
						eps_Leg.Arc = eps_Arc;
					}
					else if (curve is Line)
					{
						eps_Leg.Element = element2;
						eps_Leg.Line = eps_Line;
					}
					else if (curve is NurbSpline || curve is HermiteSpline || curve is Ellipse)
					{
						eps_Leg = GetLegFromSpline(element2, curve, _bReverseOrder: false);
					}
					eps_Leg.Last.Next = _CurrentLeg;
					_CurrentLeg = eps_Leg;
					flag = true;
				}
				else if (_CurrentLeg.LegEndPnt.IsEqual(new Eps_Point(curve.GetEndPoint(0))))
				{
					eps_Leg = new Eps_Leg();
					if (curve is Arc)
					{
						eps_Leg.Element = element2;
						eps_Leg.Arc = eps_Arc;
					}
					else if (curve is Line)
					{
						eps_Leg.Element = element2;
						eps_Leg.Line = eps_Line;
					}
					else if (curve is NurbSpline || curve is HermiteSpline || curve is Ellipse)
					{
						eps_Leg = GetLegFromSpline(element2, curve, _bReverseOrder: false);
					}
					_CurrentLeg.Last.Next = eps_Leg;
					flag = true;
				}
				else if (_CurrentLeg.LegEndPnt.IsEqual(new Eps_Point(curve.GetEndPoint(1))))
				{
					eps_Leg = new Eps_Leg();
					if (curve is Arc)
					{
						eps_Leg.Element = element2;
						eps_Leg.Arc = eps_Arc.Reverse();
					}
					else if (curve is Line)
					{
						eps_Leg.Element = element2;
						eps_Leg.Line = eps_Line.Reverse();
					}
					else if (curve is NurbSpline || curve is HermiteSpline || curve is Ellipse)
					{
						eps_Leg = GetLegFromSpline(element2, curve, _bReverseOrder: true);
					}
					_CurrentLeg.Last.Next = eps_Leg;
					flag = true;
				}
				if (EPShared.eplGeoE_ArePntsDuplicates2D(_CurrentLeg.LegStartPnt, _CurrentLeg.LegEndPnt))
				{
					return _CurrentLeg;
				}
				if (flag)
				{
					_CurrentLeg = GetLegFromElementList(_CurrentLeg, element2);
					break;
				}
			}
		}
		return _CurrentLeg;
	}

	public static List<Eps_Leg> GetStreetNetworkLegs(Document document, Element seed)
	{
		return GetStreetNetworkLegs(document, seed, null);
	}

	public static List<Eps_Leg> GetStreetNetworkLegs(Document document, Element seed, XYZ pickPoint)
	{
		List<Eps_Leg> legs = new List<Eps_Leg>();
		if (document == null || seed == null) return legs;
		List<Element> curves = CollectConnectedHostCurves(document, seed, pickPoint);
		if (curves.Count == 0) return legs;
		List<HostEdge> edges = new List<HostEdge>();
		foreach (Element element in curves)
		{
			if (!TryHostEnds(element, out Eps_Point start, out Eps_Point end)) continue;
			edges.Add(new HostEdge { Element = element, Start = start, End = end, Used = false });
		}
		if (edges.Count == 0) return legs;
		List<HostEdge> cycle = LongestCycle(edges);
		if (cycle.Count >= 3)
		{
			Eps_Leg loop = ChainHostEdges(cycle, true);
			if (loop != null) legs.Add(loop);
			foreach (HostEdge edge in cycle)
				edge.Used = true;
		}
		while (true)
		{
			HostEdge startEdge = edges.FirstOrDefault(e => !e.Used);
			if (startEdge == null) break;
			List<HostEdge> chain = WalkOpenChain(edges, startEdge);
			if (chain.Count == 0)
			{
				startEdge.Used = true;
				continue;
			}
			Eps_Leg branch = ChainHostEdges(chain, false);
			if (branch == null) continue;
			if (eplGeoE_CalcLegListLength(branch) < MinStreetLegFt && !LegContainsElement(branch, seed))
				continue;
			foreach (Eps_Leg existing in legs)
				ExtendLegToMeet(branch, existing);
			legs.Add(branch);
		}
		legs.Sort((a, b) =>
		{
			int closed = (b.IsLegClosed() ? 1 : 0) - (a.IsLegClosed() ? 1 : 0);
			if (closed != 0) return closed;
			return eplGeoE_CalcLegListLength(b).CompareTo(eplGeoE_CalcLegListLength(a));
		});
		return legs;
	}

	private const double HostJoinTolFt = 0.25;
	private const double HostGeoTolFt = 1.25;
	private const double MinStreetLegFt = 2.5;
	private const double JunctionPickRadiusFt = 30.0;
	private const double CollinearDotMin = 0.65;

	private sealed class HostEdge
	{
		public Element Element;
		public Eps_Point Start;
		public Eps_Point End;
		public bool Used;
	}

	private static List<Element> CollectConnectedHostCurves(Document document, Element seed, XYZ pickPoint)
	{
		List<Element> collected = new List<Element>();
		HashSet<long> seen = new HashSet<long>();
		Queue<Element> queue = new Queue<Element>();
		EnqueueHost(seed, seen, queue);
		DrainJoinedHosts(queue, collected, seen);
		List<Element> seedChain = new List<Element>(collected);
		foreach (CurveElement curveElement in AllHostCurves(document))
		{
			if (seen.Contains(curveElement.Id.Value64())) continue;
			if (IsHiddenHost(document, curveElement)) continue;
			if (!ConnectsAsTeeOrCross(curveElement, seedChain)) continue;
			if (pickPoint != null && DistancePointToCurve2D(pickPoint, curveElement) > JunctionPickRadiusFt)
				continue;
			EnqueueHost(curveElement, seen, queue);
		}
		DrainCollinearJoins(queue, collected, seen);
		return collected;
	}

	private static IEnumerable<CurveElement> AllHostCurves(Document document)
	{
		foreach (CurveElement curveElement in new FilteredElementCollector(document).OfClass(typeof(CurveElement)).WhereElementIsNotElementType())
		{
			if (curveElement is not ModelCurve) continue;
			if (curveElement.Location is not LocationCurve location || location.Curve == null || !location.Curve.IsBound)
				continue;
			yield return curveElement;
		}
	}

	private static bool IsHiddenHost(Document document, Element element)
	{
		try
		{
			Autodesk.Revit.DB.View view = document?.ActiveView;
			return view != null && element.IsHidden(view);
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static void EnqueueHost(Element element, HashSet<long> seen, Queue<Element> queue)
	{
		if (element == null || element.Location is not LocationCurve) return;
		if (!seen.Add(element.Id.Value64())) return;
		queue.Enqueue(element);
	}

	private static void DrainJoinedHosts(Queue<Element> queue, List<Element> collected, HashSet<long> seen)
	{
		while (queue.Count > 0)
		{
			Element current = queue.Dequeue();
			collected.Add(current);
			if (current.Location is not LocationCurve location) continue;
			for (int end = 0; end < 2; end++)
			{
				ElementArray joined = location.get_ElementsAtJoin(end);
				if (joined == null) continue;
				foreach (Element next in joined)
					EnqueueHost(next, seen, queue);
			}
		}
	}

	private static void DrainCollinearJoins(Queue<Element> queue, List<Element> collected, HashSet<long> seen)
	{
		while (queue.Count > 0)
		{
			Element current = queue.Dequeue();
			collected.Add(current);
			if (!TryHostEnds(current, out Eps_Point start, out Eps_Point end)) continue;
			if (current.Location is not LocationCurve location) continue;
			for (int endIndex = 0; endIndex < 2; endIndex++)
			{
				ElementArray joined = location.get_ElementsAtJoin(endIndex);
				if (joined == null) continue;
				Eps_Point node = endIndex == 0 ? start : end;
				Eps_Point from = endIndex == 0 ? end : start;
				if (!TryUnitDirection(from, node, out double inX, out double inY)) continue;
				foreach (Element next in joined)
				{
					if (next == null || seen.Contains(next.Id.Value64())) continue;
					if (!TryHostEnds(next, out Eps_Point nextStart, out Eps_Point nextEnd)) continue;
					Eps_Point other = OtherPoint(node, nextStart, nextEnd);
					if (other == null || !TryUnitDirection(node, other, out double outX, out double outY))
						continue;
					if (inX * outX + inY * outY < CollinearDotMin) continue;
					EnqueueHost(next, seen, queue);
				}
			}
		}
	}

	private static Eps_Point OtherPoint(Eps_Point node, Eps_Point start, Eps_Point end)
	{
		if (EPShared.eplGeoE_ArePntsDuplicates2D(node, start, HostGeoTolFt)) return end;
		if (EPShared.eplGeoE_ArePntsDuplicates2D(node, end, HostGeoTolFt)) return start;
		return null;
	}

	private static bool ConnectsAsTeeOrCross(Element candidate, List<Element> hosts)
	{
		if (candidate.Location is not LocationCurve candidateLocation || candidateLocation.Curve == null)
			return false;
		if (!TryHostEnds(candidate, out Eps_Point start, out Eps_Point end)) return false;
		if (EPShared.eplGeoE_ArePntsDuplicates2D(start, end, HostJoinTolFt)) return false;
		double length = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(start, end, ref length);
		if (length < MinStreetLegFt) return false;
		Curve candidateCurve = candidateLocation.Curve;
		foreach (Element existing in hosts)
		{
			if (existing.Location is not LocationCurve location || location.Curve == null) continue;
			if (IsSameHostCurve(candidateCurve, location.Curve)) continue;
			if (CurvesFormTeeOrCross(candidateCurve, location.Curve))
				return true;
		}
		return false;
	}

	private static bool CurvesFormTeeOrCross(Curve a, Curve b)
	{
		Curve flatA = FlattenCurveXY(a);
		Curve flatB = FlattenCurveXY(b);
		if (flatA == null || flatB == null) return false;
		XYZ a0 = flatA.GetEndPoint(0);
		XYZ a1 = flatA.GetEndPoint(1);
		XYZ b0 = flatB.GetEndPoint(0);
		XYZ b1 = flatB.GetEndPoint(1);
		bool aStartOnB = DistancePointToCurve2D(a0, flatB) <= HostGeoTolFt;
		bool aEndOnB = DistancePointToCurve2D(a1, flatB) <= HostGeoTolFt;
		bool bStartOnA = DistancePointToCurve2D(b0, flatA) <= HostGeoTolFt;
		bool bEndOnA = DistancePointToCurve2D(b1, flatA) <= HostGeoTolFt;
		if (aStartOnB && aEndOnB) return false;
		if (bStartOnA && bEndOnA) return false;
		if (aStartOnB != aEndOnB) return true;
		if (bStartOnA != bEndOnA) return true;
		return SegmentsCross2D(a0, a1, b0, b1) || HasInteriorIntersection(a, b);
	}

	private static bool IsSameHostCurve(Curve a, Curve b)
	{
		if (a == null || b == null || !a.IsBound || !b.IsBound) return false;
		return PointNearCurve(new Eps_Point(a.GetEndPoint(0)), b, HostGeoTolFt)
			&& PointNearCurve(new Eps_Point(a.GetEndPoint(1)), b, HostGeoTolFt)
			&& PointNearCurve(new Eps_Point(b.GetEndPoint(0)), a, HostGeoTolFt)
			&& PointNearCurve(new Eps_Point(b.GetEndPoint(1)), a, HostGeoTolFt);
	}

	private static bool HasInteriorIntersection(Curve a, Curve b)
	{
		try
		{
			Curve flatA = FlattenCurveXY(a);
			Curve flatB = FlattenCurveXY(b);
			if (flatA == null || flatB == null) return false;
			IntersectionResultArray results;
			SetComparisonResult result = flatA.Intersect(flatB, out results);
			if (result != SetComparisonResult.Overlap || results == null || results.Size == 0)
				return false;
			foreach (IntersectionResult hit in results)
			{
				XYZ point = hit?.XYZPoint;
				if (point == null) continue;
				bool onAEnd = HorizontalDistance(point, flatA.GetEndPoint(0)) <= HostGeoTolFt
					|| HorizontalDistance(point, flatA.GetEndPoint(1)) <= HostGeoTolFt;
				bool onBEnd = HorizontalDistance(point, flatB.GetEndPoint(0)) <= HostGeoTolFt
					|| HorizontalDistance(point, flatB.GetEndPoint(1)) <= HostGeoTolFt;
				if (!onAEnd || !onBEnd)
					return true;
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	private static Curve FlattenCurveXY(Curve curve)
	{
		if (curve == null || !curve.IsBound) return null;
		XYZ start = curve.GetEndPoint(0);
		XYZ end = curve.GetEndPoint(1);
		XYZ a = new XYZ(start.X, start.Y, 0.0);
		XYZ b = new XYZ(end.X, end.Y, 0.0);
		if (curve is Line)
		{
			if (a.DistanceTo(b) < 1E-09) return null;
			return Line.CreateBound(a, b);
		}
		if (curve is Arc)
		{
			XYZ mid = curve.Evaluate(0.5, true);
			try
			{
				return Arc.Create(a, b, new XYZ(mid.X, mid.Y, 0.0));
			}
			catch (Exception)
			{
				return a.DistanceTo(b) < 1E-09 ? null : Line.CreateBound(a, b);
			}
		}
		return curve;
	}

	private static double HorizontalDistance(XYZ a, XYZ b)
	{
		double dx = a.X - b.X;
		double dy = a.Y - b.Y;
		return Math.Sqrt(dx * dx + dy * dy);
	}

	private static bool LegContainsElement(Eps_Leg leg, Element element)
	{
		if (leg == null || element == null) return false;
		for (Eps_Leg piece = leg; piece != null; piece = piece.Next)
		{
			if (piece.Element != null && piece.Element.Id == element.Id)
				return true;
		}
		return false;
	}

	private static double DistancePointToCurve2D(XYZ point, Element element)
	{
		if (point == null || element?.Location is not LocationCurve location || location.Curve == null)
			return double.MaxValue;
		return DistancePointToCurve2D(point, location.Curve);
	}

	private static double DistancePointToCurve2D(XYZ point, Curve curve)
	{
		Curve flat = FlattenCurveXY(curve);
		if (point == null || flat == null) return double.MaxValue;
		XYZ probe = new XYZ(point.X, point.Y, 0.0);
		IList<XYZ> points = flat is Line
			? new List<XYZ> { flat.GetEndPoint(0), flat.GetEndPoint(1) }
			: flat.Tessellate();
		double best = double.MaxValue;
		for (int i = 1; i < points.Count; i++)
		{
			double distance = DistancePointToSegment2D(probe, points[i - 1], points[i]);
			if (distance < best)
				best = distance;
		}
		return best;
	}

	private static double DistancePointToSegment2D(XYZ point, XYZ start, XYZ end)
	{
		double dx = end.X - start.X;
		double dy = end.Y - start.Y;
		double lengthSq = dx * dx + dy * dy;
		if (lengthSq < 1E-18)
			return HorizontalDistance(point, start);
		double t = ((point.X - start.X) * dx + (point.Y - start.Y) * dy) / lengthSq;
		if (t < 0.0) t = 0.0;
		else if (t > 1.0) t = 1.0;
		return HorizontalDistance(point, new XYZ(start.X + t * dx, start.Y + t * dy, 0.0));
	}

	private static bool SegmentsCross2D(XYZ a0, XYZ a1, XYZ b0, XYZ b1)
	{
		double d1 = Cross2D(b0, b1, a0);
		double d2 = Cross2D(b0, b1, a1);
		double d3 = Cross2D(a0, a1, b0);
		double d4 = Cross2D(a0, a1, b1);
		return ((d1 > 1E-08 && d2 < -1E-08) || (d1 < -1E-08 && d2 > 1E-08))
			&& ((d3 > 1E-08 && d4 < -1E-08) || (d3 < -1E-08 && d4 > 1E-08));
	}

	private static double Cross2D(XYZ origin, XYZ a, XYZ b)
	{
		return (a.X - origin.X) * (b.Y - origin.Y) - (a.Y - origin.Y) * (b.X - origin.X);
	}

	private static bool PointNearCurve(Eps_Point point, Curve curve, double tolerance)
	{
		try
		{
			Curve flat = FlattenCurveXY(curve);
			if (flat == null) return false;
			XYZ probe = new XYZ(point.x, point.y, 0.0);
			IntersectionResult result = flat.Project(probe);
			if (result == null || result.XYZPoint == null) return false;
			return HorizontalDistance(probe, result.XYZPoint) <= tolerance;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static bool TryHostEnds(Element element, out Eps_Point start, out Eps_Point end)
	{
		start = null;
		end = null;
		if (element?.Location is not LocationCurve location || location.Curve == null || !location.Curve.IsBound)
			return false;
		start = new Eps_Point(location.Curve.GetEndPoint(0));
		end = new Eps_Point(location.Curve.GetEndPoint(1));
		return true;
	}

	private static List<HostEdge> LongestCycle(List<HostEdge> edges)
	{
		Dictionary<string, int> degree = new Dictionary<string, int>();
		foreach (HostEdge edge in edges)
		{
			string a = NodeKey(edge.Start);
			string b = NodeKey(edge.End);
			degree[a] = (degree.TryGetValue(a, out int da) ? da : 0) + 1;
			degree[b] = (degree.TryGetValue(b, out int db) ? db : 0) + 1;
		}
		List<HostEdge> best = new List<HostEdge>();
		foreach (HostEdge start in edges)
		{
			List<HostEdge> path = GreedyCycleFrom(edges, start, degree);
			if (path.Count > best.Count) best = path;
		}
		return best;
	}

	private static List<HostEdge> GreedyCycleFrom(List<HostEdge> edges, HostEdge start, Dictionary<string, int> degree)
	{
		List<HostEdge> path = new List<HostEdge> { start };
		HashSet<HostEdge> used = new HashSet<HostEdge> { start };
		Eps_Point origin = start.Start;
		Eps_Point current = start.End;
		for (int step = 0; step < edges.Count; step++)
		{
			HostEdge next = null;
			int nextDeg = -1;
			HostEdge close = null;
			foreach (HostEdge edge in edges)
			{
				if (used.Contains(edge)) continue;
				Eps_Point other = OtherEnd(edge, current);
				if (other == null) continue;
				if (EPShared.eplGeoE_ArePntsDuplicates2D(other, origin, HostGeoTolFt) && path.Count >= 2)
				{
					close = edge;
					continue;
				}
				int deg = NodeDegree(other, degree);
				if (next == null || deg > nextDeg)
				{
					next = edge;
					nextDeg = deg;
				}
			}
			if (close != null && (next == null || nextDeg <= 1 || path.Count >= 3))
			{
				path.Add(close);
				return path;
			}
			if (next == null) break;
			used.Add(next);
			current = OtherEnd(next, current);
			path.Add(next);
		}
		return new List<HostEdge>();
	}

	private static Eps_Point OtherEnd(HostEdge edge, Eps_Point at)
	{
		if (EPShared.eplGeoE_ArePntsDuplicates2D(at, edge.Start, HostGeoTolFt)) return edge.End;
		if (EPShared.eplGeoE_ArePntsDuplicates2D(at, edge.End, HostGeoTolFt)) return edge.Start;
		return null;
	}

	private static string NodeKey(Eps_Point point)
	{
		return Math.Round(point.x / HostJoinTolFt) + "," + Math.Round(point.y / HostJoinTolFt);
	}

	private static int NodeDegree(Eps_Point point, Dictionary<string, int> degree)
	{
		return degree.TryGetValue(NodeKey(point), out int value) ? value : 0;
	}

	private static List<HostEdge> WalkOpenChain(List<HostEdge> edges, HostEdge start)
	{
		List<HostEdge> chain = new List<HostEdge> { start };
		start.Used = true;
		Eps_Point head = start.Start;
		Eps_Point tail = start.End;
		bool grew = true;
		while (grew)
		{
			grew = false;
			HostEdge tailEdge = chain[chain.Count - 1];
			HostEdge next = BestContinuation(edges, tail, OtherEnd(tailEdge, tail), tailEdge);
			if (next != null)
			{
				Eps_Point newTail = OtherEnd(next, tail);
				if (newTail != null)
				{
					chain.Add(next);
					next.Used = true;
					tail = newTail;
					grew = true;
					continue;
				}
			}
			HostEdge headEdge = chain[0];
			HostEdge prev = BestContinuation(edges, head, OtherEnd(headEdge, head), headEdge);
			if (prev != null)
			{
				Eps_Point newHead = OtherEnd(prev, head);
				if (newHead != null)
				{
					chain.Insert(0, prev);
					prev.Used = true;
					head = newHead;
					grew = true;
				}
			}
		}
		return chain;
	}

	private static HostEdge BestContinuation(List<HostEdge> edges, Eps_Point node, Eps_Point from, HostEdge current)
	{
		if (node == null) return null;
		List<HostEdge> candidates = new List<HostEdge>();
		foreach (HostEdge edge in edges)
		{
			if (edge.Used || edge == current) continue;
			if (OtherEnd(edge, node) == null) continue;
			candidates.Add(edge);
		}
		if (candidates.Count == 0) return null;
		if (candidates.Count == 1) return candidates[0];
		if (from == null || !TryUnitDirection(from, node, out double inX, out double inY))
			return candidates[0];
		HostEdge best = null;
		double bestDot = CollinearDotMin;
		foreach (HostEdge edge in candidates)
		{
			Eps_Point other = OtherEnd(edge, node);
			if (other == null || !TryUnitDirection(node, other, out double outX, out double outY))
				continue;
			double dot = inX * outX + inY * outY;
			if (dot >= bestDot)
			{
				bestDot = dot;
				best = edge;
			}
		}
		return best;
	}

	private static bool TryUnitDirection(Eps_Point from, Eps_Point to, out double dx, out double dy)
	{
		dx = to.x - from.x;
		dy = to.y - from.y;
		double length = Math.Sqrt(dx * dx + dy * dy);
		if (length < 1E-09)
		{
			dx = 0.0;
			dy = 0.0;
			return false;
		}
		dx /= length;
		dy /= length;
		return true;
	}

	private static Eps_Leg ChainHostEdges(List<HostEdge> edges, bool close)
	{
		if (edges == null || edges.Count == 0) return null;
		Eps_Leg head = LegFromLocationElement(edges[0].Element);
		if (head == null) return null;
		for (int i = 1; i < edges.Count; i++)
		{
			Eps_Leg piece = LegFromLocationElement(edges[i].Element);
			if (piece == null) continue;
			if (EPShared.eplGeoE_ArePntsDuplicates2D(head.LegEndPnt, piece.LegStartPnt, HostGeoTolFt))
				head.Last.Next = piece;
			else if (EPShared.eplGeoE_ArePntsDuplicates2D(head.LegEndPnt, piece.LegEndPnt, HostGeoTolFt))
				head.Last.Next = piece.Reverse();
			else if (EPShared.eplGeoE_ArePntsDuplicates2D(head.LegStartPnt, piece.LegEndPnt, HostGeoTolFt))
			{
				piece.Last.Next = head;
				head = piece;
			}
			else if (EPShared.eplGeoE_ArePntsDuplicates2D(head.LegStartPnt, piece.LegStartPnt, HostGeoTolFt))
			{
				Eps_Leg reversed = piece.Reverse();
				reversed.Last.Next = head;
				head = reversed;
			}
			else
				head.Last.Next = piece;
		}
		if (close && head != null && !head.IsLegClosed() && EPShared.eplGeoE_ArePntsDuplicates2D(head.LegStartPnt, head.LegEndPnt, HostGeoTolFt * 2.0))
		{
			if (head.Last.Line != null)
				head.Last.Line.EndPnt = new Eps_Point(head.LegStartPnt);
			else if (head.Last.Arc != null)
				head.Last.Arc.EndPnt = new Eps_Point(head.LegStartPnt);
		}
		return head;
	}

	private static Eps_Leg LegFromLocationElement(Element element)
	{
		if (element?.Location is not LocationCurve location || location.Curve == null) return null;
		Eps_Leg leg = new Eps_Leg();
		if (location.Curve is Arc)
		{
			leg.Element = element;
			leg.Arc = new Eps_Arc((Arc)location.Curve);
			return leg;
		}
		if (location.Curve is Line)
		{
			leg.Element = element;
			leg.Line = new Eps_Line((Line)location.Curve);
			return leg;
		}
		if (location.Curve is NurbSpline || location.Curve is HermiteSpline || location.Curve is Ellipse)
			return GetLegFromSpline(element, location.Curve, _bReverseOrder: false);
		return null;
	}

	private static void ExtendLegToMeet(Eps_Leg branch, Eps_Leg loop)
	{
		if (branch == null || loop == null) return;
		TrySnapEndToLeg(branch, loop, true);
		TrySnapEndToLeg(branch, loop, false);
	}

	private static void TrySnapEndToLeg(Eps_Leg branch, Eps_Leg target, bool start)
	{
		Eps_Point end = start ? branch.LegStartPnt : branch.LegEndPnt;
		double station = 0.0;
		double offset = 0.0;
		if (eplGeoE_CalcStationOffsetAlongLegList(end, target, ref station, ref offset) != 1700)
			return;
		if (Math.Abs(offset) > 1.0) return;
		Eps_Point snapped = new Eps_Point();
		if (eplGeoE_FindPntOnLegByStationOffset(target, station, 0.0, ref snapped) != 0)
			return;
		if (start)
		{
			if (branch.Line != null) branch.Line.StartPnt = new Eps_Point(snapped);
			else if (branch.Arc != null) branch.Arc.StartPnt = new Eps_Point(snapped);
		}
		else
		{
			Eps_Leg last = branch.Last;
			if (last.Line != null) last.Line.EndPnt = new Eps_Point(snapped);
			else if (last.Arc != null) last.Arc.EndPnt = new Eps_Point(snapped);
		}
	}

	public static void CreatePointListFromMinMaxOfArea(ref List<Eps_Point> _PointList, Eps_Point _LowerLeftPnt, Eps_Point _UpperRightPnt, double _dBaseLineAngle, double _dSpacing, bool _bSpacingType)
	{
		Eps_Point _MidPoint = new Eps_Point();
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double _dDistance = 0.0;
		int num7 = 0;
		int num8 = 0;
		EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(_LowerLeftPnt, _UpperRightPnt, ref _MidPoint);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_LowerLeftPnt, _UpperRightPnt, ref _dDistance);
		for (num8 = 1; num8 <= 4; num8++)
		{
			switch (num8)
			{
			case 1:
				num4 = _dBaseLineAngle;
				num3 = 1.0;
				break;
			case 2:
				num4 = EPShared.eplGeoE_NormalizeRadians(_dBaseLineAngle + Math.PI);
				num3 = 1.0;
				break;
			case 3:
				num4 = EPShared.eplGeoE_NormalizeRadians(_dBaseLineAngle + Math.PI);
				num3 = -1.0;
				break;
			case 4:
				num4 = _dBaseLineAngle;
				num3 = -1.0;
				break;
			}
			Eps_Point eps_Point = new Eps_Point(_MidPoint);
			Eps_Point _NewPnt = new Eps_Point(_MidPoint);
			num5 = _dDistance;
			num7 = 1;
			while (num5 >= 0.0)
			{
				eps_Point = new Eps_Point(_NewPnt);
				AddBoundedPointToPointArray(ref _PointList, _NewPnt, _LowerLeftPnt, _UpperRightPnt);
				for (num6 = _dDistance; num6 >= 0.0; num6 -= _dSpacing)
				{
					EPShared.eplGeoE_CalcPolarToXY(eps_Point, num4, _dSpacing, ref eps_Point);
					AddBoundedPointToPointArray(ref _PointList, eps_Point, _LowerLeftPnt, _UpperRightPnt);
				}
				if (_bSpacingType)
				{
					num = ((num7 % 2 <= 0) ? (Math.PI * 2.0 / 3.0) : (Math.PI / 3.0));
				}
				else
				{
					num = Math.PI / 2.0;
				}
				num2 = EPShared.eplGeoE_NormalizeRadians(num4 + num * num3);
				EPShared.eplGeoE_CalcPolarToXY(_NewPnt, num2, _dSpacing, ref _NewPnt);
				num5 -= _dSpacing;
				num7++;
			}
		}
	}

	private static void AddBoundedPointToPointArray(ref List<Eps_Point> _PointList, Eps_Point _AddPoint, Eps_Point _LowerLeftPnt, Eps_Point _UpperRightPnt)
	{
		if (_LowerLeftPnt != null && _UpperRightPnt != null && (RevitDefines.LT(_AddPoint.x, _LowerLeftPnt.x) || RevitDefines.LT(_AddPoint.y, _LowerLeftPnt.y) || RevitDefines.GT(_AddPoint.x, _UpperRightPnt.x) || RevitDefines.GT(_AddPoint.y, _UpperRightPnt.y)))
		{
			return;
		}
		if (_PointList == null)
		{
			_PointList = new List<Eps_Point>();
		}
		bool flag = false;
		for (int i = 0; i < _PointList.Count; i++)
		{
			if (EPShared.eplGeoE_ArePntsDuplicates2D(_PointList[i], _AddPoint, 1E-05))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			_PointList.Add(_AddPoint);
		}
	}

	public static void GetAllPointsInBoundary(ref List<Eps_Point> paPntList, Eps_Leg pLeg, List<Eps_Leg> pIslandList)
	{
		if (pLeg == null)
		{
			return;
		}
		Eps_Leg eps_Leg = null;
		List<Eps_Point> list = new List<Eps_Point>();
		int num = 0;
		bool flag = true;
		Eps_Point pMinPnt = new Eps_Point();
		Eps_Point pMaxPnt = new Eps_Point();
		if (paPntList == null || paPntList.Count == 0)
		{
			return;
		}
		eplGeoE_FindMinMaxOfLegList(pLeg, ref pMinPnt, ref pMaxPnt);
		for (num = 0; num < paPntList.Count; num++)
		{
			if (eplGeoE_IsPointInsideLegList(pLeg, paPntList[num], 1E-05, pMinPnt, pMaxPnt) == 6)
			{
				continue;
			}
			flag = true;
			if (pIslandList != null)
			{
				for (int i = 0; i < pIslandList.Count; i++)
				{
					eps_Leg = pIslandList[i];
					if (eplGeoE_IsPointInsideLegList(eps_Leg, paPntList[num]) != 6)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag && (list.Count == 0 || !EPShared.eplGeoE_ArePntsDuplicates2D(paPntList[num], list[list.Count - 1])))
			{
				list.Add(paPntList[num]);
			}
		}
		paPntList = list;
	}

	public static void RemoveAllPointsInBoundary(ref List<Eps_Point> paPntList, Eps_Leg pLeg)
	{
		if (pLeg == null)
		{
			return;
		}
		int num = 0;
		Eps_Point pMinPnt = new Eps_Point();
		Eps_Point pMaxPnt = new Eps_Point();
		if (paPntList == null || paPntList.Count == 0)
		{
			return;
		}
		eplGeoE_FindMinMaxOfLegList(pLeg, ref pMinPnt, ref pMaxPnt);
		for (num = paPntList.Count - 1; num >= 0; num--)
		{
			if (eplGeoE_IsPointInsideLegList(pLeg, paPntList[num], 1E-05, pMinPnt, pMaxPnt) != 6 && paPntList.Count > 0)
			{
				paPntList.RemoveAt(num);
			}
		}
	}

	public static int SolveArcFromTangentLines(Eps_Line _pIn, Eps_Line _pOut, double _dRadius, ref Eps_Arc _pArc)
	{
		return SolveArcFromTangentLines(_pIn, _pOut, _dRadius, ref _pArc, _bVerifyTangentLength: false);
	}

	public static int SolveArcFromTangentLines(Eps_Line _pIn, Eps_Line _pOut, double _dRadius, ref Eps_Arc _pArc, bool _bVerifyTangentLength)
	{
		if (RevitDefines.EQ(_dRadius, 0.0))
		{
			return 6;
		}
		Eps_Arc _ArcStruct = new Eps_Arc();
		Eps_Point eps_Point = null;
		Eps_Point eps_Point2 = null;
		Eps_Point eps_Point3 = null;
		Eps_Point _NewPnt = new Eps_Point();
		double _dDirection = 0.0;
		double _dDirection2 = 0.0;
		double num = 0.0;
		double _dAngle = 0.0;
		eps_Point = _pIn.StartPnt;
		eps_Point3 = _pIn.EndPnt;
		eps_Point2 = _pOut.EndPnt;
		_ArcStruct.Radius = _dRadius;
		if (EPShared.eplGeoE_CalcHorzAngByThreePnts(eps_Point, eps_Point3, eps_Point2, 12, ref _dAngle, 1E-05) == 0)
		{
			if (_dAngle > Math.PI)
			{
				_ArcStruct.Delta = _dAngle - Math.PI;
				_ArcStruct.Direction = -1;
			}
			else
			{
				_ArcStruct.Delta = Math.PI - _dAngle;
				_ArcStruct.Direction = 1;
			}
			num = EPShared.eplGeoE_SolveTanLenByRadiusAndDelta(_dRadius, _ArcStruct.Delta);
			if (_bVerifyTangentLength)
			{
				if (RevitDefines.EQ(num, 0.0))
				{
					return 6;
				}
				double _dDistance = 0.0;
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Point3, eps_Point, ref _dDistance);
				if (RevitDefines.GT(num, _dDistance))
				{
					return 6;
				}
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Point3, eps_Point2, ref _dDistance);
				if (RevitDefines.GT(num, _dDistance))
				{
					return 6;
				}
			}
			EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point3, eps_Point, ref _dDirection);
			EPShared.eplGeoE_CalcPolarToXY(eps_Point3, _dDirection, num, ref _NewPnt);
			_ArcStruct.StartPnt = _NewPnt;
			EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Point3, eps_Point2, ref _dDirection2);
			EPShared.eplGeoE_CalcPolarToXY(eps_Point3, _dDirection2, num, ref _NewPnt);
			_ArcStruct.EndPnt = _NewPnt;
			EPShared.eplGeoE_CalcRadiusPointFromPCAndPIAndPT(_ArcStruct.StartPnt, eps_Point3, _ArcStruct.EndPnt, ref _NewPnt);
			_ArcStruct.RadiusPnt = _NewPnt;
			EPShared.eplGeoE_CalcArcStructFromPCAndPTAndRP(_ArcStruct.StartPnt, _ArcStruct.RadiusPnt, _ArcStruct.EndPnt, _ArcStruct.Direction, ref _ArcStruct);
			_pArc = _ArcStruct;
			return 0;
		}
		return 6;
	}

	public static double RandomDouble(double MinValue, double MaxValue)
	{
		Random random = new Random();
		int num = random.Next();
		double num2 = MinValue;
		double dSecondOperand = MaxValue - MinValue;
		num2 += EPShared.eplCalE_fmod(num, dSecondOperand);
		num = random.Next();
		return num2 + EPShared.eplCalE_fmod(num, 10000.0) * 0.0001;
	}

	public static int OffsetLegList(Eps_Leg pLegListToOffset, double OffsetDist, int OffsetDir, ref Eps_Leg ppOffsetLegList, bool TrimClosedObject)
	{
		int num = 0;
		Eps_Leg eps_Leg = null;
		if (pLegListToOffset == null)
		{
			return 700;
		}
		eps_Leg = Eps_Leg.CopyLeg(pLegListToOffset);
		if (num != 0)
		{
			return num;
		}
		ppOffsetLegList = new Eps_Leg();
		bool flag = EPShared.eplGeoE_ArePntsDuplicates2D(eps_Leg.LegStartPnt, eps_Leg.LegEndPnt);
		num = FindOffsetLegs(eps_Leg, OffsetDist, OffsetDir, ref ppOffsetLegList);
		if (num == 0)
		{
			TrimOffsetLegs(ref ppOffsetLegList, flag);
			TrimAdjacentArcs(ref ppOffsetLegList, flag);
			if (flag)
			{
				CloseLegList(ref ppOffsetLegList);
			}
			if (TrimClosedObject)
			{
				RemoveCrossingLegs(ref ppOffsetLegList);
			}
		}
		return num;
	}

	private static void RemoveCrossingLegs(ref Eps_Leg pOffsetLegList)
	{
		Eps_Point pIntersectionPoint = new Eps_Point();
		Eps_Point[] paIntersections = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		int num = 0;
		if (pOffsetLegList.LegCount < 3)
		{
			return;
		}
		Eps_Leg pLeg = pOffsetLegList;
		Eps_Leg last = pLeg.Last;
		if (IntersectTwoLegs(pOffsetLegList, last, ref pIntersectionPoint) == 0)
		{
			Eps_Leg eps_Leg = null;
			eps_Leg = Eps_Leg.SingleLeg(pOffsetLegList);
			if (eplGeoE_IsPointOnLegList2D(pIntersectionPoint, eps_Leg) && eplGeoE_IsPointOnLegList2D(pIntersectionPoint, last))
			{
				CloseLegList(ref pOffsetLegList);
			}
			eps_Leg = null;
		}
		while (pLeg != null && pLeg.Next != null)
		{
			for (last = pLeg.Next.Next; last != null; last = last.Next)
			{
				num = 0;
				int num2 = eplGeoE_IntersectTwoLegs(pLeg, last, ref paIntersections, ref num, 1E-05);
				if (num2 != 1600 && num > 0)
				{
					if (num == 1)
					{
						pIntersectionPoint = new Eps_Point(paIntersections[0]);
					}
					else if (pLeg.Arc != null)
					{
						GetClosestPoint(pLeg.Arc.StartPnt, paIntersections, ref pIntersectionPoint);
					}
					else if (pLeg.Line != null)
					{
						GetClosestPoint(pLeg.Line.StartPnt, paIntersections, ref pIntersectionPoint);
					}
					else
					{
						num2 = 1600;
					}
					if (num2 != 1600)
					{
						Eps_Point firstCheckPnt = new Eps_Point(pLeg.LegStartPnt);
						Eps_Point firstCheckPnt2 = new Eps_Point(last.LegEndPnt);
						if (!EPShared.eplGeoE_ArePntsDuplicates2D(firstCheckPnt, pIntersectionPoint, 1E-05) && !EPShared.eplGeoE_ArePntsDuplicates2D(firstCheckPnt2, pIntersectionPoint, 1E-05))
						{
							TrimEndOfLeg(ref pLeg, pIntersectionPoint);
							TrimStartOfLeg(ref last, pIntersectionPoint);
							RemoveLegsFromList(pLeg, last);
						}
					}
				}
			}
			pLeg = pLeg.Next;
		}
	}

	private static void RemoveLegsFromList(Eps_Leg pRemoveFromLeg, Eps_Leg pRemoveToLeg)
	{
		Eps_Leg next = pRemoveFromLeg.Next;
		while (next != pRemoveToLeg && next != null)
		{
			Eps_Leg eps_Leg = next;
			next = next.Next;
			eps_Leg = null;
		}
		pRemoveFromLeg.Next = pRemoveToLeg;
	}

	private static void CloseLegList(ref Eps_Leg pLegList)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg pLeg = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Point pIntersectionPoint = new Eps_Point();
		Eps_Point pTrimPoint = new Eps_Point();
		if (pLegList.LegCount < 2 || EPShared.eplGeoE_ArePntsDuplicates2D(pLegList.LegStartPnt, pLegList.LegEndPnt))
		{
			return;
		}
		if (pLegList.LegCount == 2 && pLegList.Arc != null && pLegList.Next.Arc != null)
		{
			CloseLegListWithTwoArcsOnly(pLegList, pLegList.Next);
			return;
		}
		eps_Leg = pLegList;
		while (eps_Leg != null && eps_Leg.Next != null)
		{
			for (eps_Leg2 = eps_Leg.Next.Next; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
			{
				if (IntersectTwoLegs(eps_Leg, eps_Leg2, ref pIntersectionPoint) == 0)
				{
					pLeg = eps_Leg2;
					pTrimPoint = new Eps_Point(pIntersectionPoint);
					Eps_Leg eps_Leg4 = eps_Leg2;
					eps_Leg4.Next = null;
					Eps_Leg eps_Leg5 = eps_Leg;
					eps_Leg5.Next = null;
					if (eplGeoE_IsPointOnLegList2D(pIntersectionPoint, eps_Leg4) && eplGeoE_IsPointOnLegList2D(pIntersectionPoint, eps_Leg5))
					{
						break;
					}
					pLeg = null;
				}
			}
			if (pLeg != null)
			{
				break;
			}
			eps_Leg3 = eps_Leg;
			eps_Leg = eps_Leg.Next;
		}
		if (eps_Leg != null && pLeg != null)
		{
			TrimStartOfLeg(ref eps_Leg, pTrimPoint);
			TrimEndOfLeg(ref pLeg, pTrimPoint);
			if (eps_Leg != pLegList)
			{
				eps_Leg3.Next = null;
				pLegList = eps_Leg;
			}
			pLeg.Next = null;
		}
	}

	private static void CloseLegListWithTwoArcsOnly(Eps_Leg pFirstLeg, Eps_Leg pSecondLeg)
	{
		Eps_Point[] InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		Eps_Point pClosestPoint = new Eps_Point();
		int num = eplGeoE_IntersectTwoDists2D(pFirstLeg.Arc.RadiusPnt, pFirstLeg.Arc.Radius, pSecondLeg.Arc.RadiusPnt, pSecondLeg.Arc.Radius, ref InterPnt, 0.01);
		if (num == 1607)
		{
			GetClosestPoint(pFirstLeg.Arc.StartPnt, InterPnt, ref pClosestPoint);
			TrimStartOfLeg(ref pFirstLeg, pClosestPoint);
			TrimEndOfLeg(ref pSecondLeg, pClosestPoint);
		}
	}

	public static int FindOffsetLegs(Eps_Leg pOrigLegList, double OffsetDist, int OffsetDir, ref Eps_Leg ppOffsetLegList)
	{
		int num = 0;
		Eps_Leg eps_Leg = null;
		for (Eps_Leg eps_Leg2 = pOrigLegList; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Element = eps_Leg2.Element;
			if (eps_Leg2.Arc != null)
			{
				Eps_Arc _OffsetArc = new Eps_Arc();
				num = EPShared.eplGeoE_ConstructOffsetArc(eps_Leg2.Arc, OffsetDist, OffsetDir, ref _OffsetArc);
				eps_Leg.Arc = _OffsetArc;
				eps_Leg.Arc.StartPnt.z = eps_Leg2.Arc.StartPnt.z;
				eps_Leg.Arc.EndPnt.z = eps_Leg2.Arc.EndPnt.z;
				eps_Leg.Arc.RadiusPnt.z = eps_Leg2.Arc.RadiusPnt.z;
			}
			else if (eps_Leg2.Line != null)
			{
				Eps_Line _OffsetLine = new Eps_Line();
				num = EPShared.eplGeoE_ConstructOffsetLine(eps_Leg2.Line, OffsetDist, OffsetDir, ref _OffsetLine);
				eps_Leg.Line = _OffsetLine;
				eps_Leg.Line.StartPnt.z = eps_Leg2.Line.StartPnt.z;
				eps_Leg.Line.EndPnt.z = eps_Leg2.Line.EndPnt.z;
			}
			else
			{
				num = 700;
			}
			if (num == 0)
			{
				eps_Leg.SplineNum = -1;
				eps_Leg.Is3D = eps_Leg2.Is3D;
				eps_Leg.Next = null;
				if (RevitDefines.GT(eplGeoE_CalcLegListLength(eps_Leg), 0.0))
				{
					if (ppOffsetLegList.LegCount == 0)
					{
						ppOffsetLegList = Eps_Leg.SingleLeg(eps_Leg);
					}
					else
					{
						ppOffsetLegList.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
					}
				}
			}
		}
		return num;
	}

	private static void TrimOffsetLegs(ref Eps_Leg pOffsetLegList, bool bClosedLegList)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Leg pLeg = null;
		Eps_Leg eps_Leg3 = null;
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point eps_Point2 = new Eps_Point();
		Eps_Point eps_Point3 = new Eps_Point();
		Eps_Point pIntersectionPoint = new Eps_Point();
		bool flag = false;
		Eps_Point pIntersectionPoint2 = new Eps_Point();
		int num = 0;
		double num2 = 0.0;
		if (pOffsetLegList.LegCount < 2)
		{
			return;
		}
		if (bClosedLegList)
		{
			pLeg = pOffsetLegList.Last;
			if (IntersectTwoLegs(pLeg, pOffsetLegList, ref pIntersectionPoint) != 0)
			{
				if (pLeg.Line != null)
				{
					eps_Point = new Eps_Point(pLeg.Line.StartPnt);
					eps_Point2 = new Eps_Point(pLeg.Line.EndPnt);
				}
				else if (pLeg.Arc != null)
				{
					eps_Point = new Eps_Point(pLeg.Arc.StartPnt);
					eps_Point2 = new Eps_Point(pLeg.Arc.EndPnt);
				}
				else
				{
					num = 1600;
				}
				if (pOffsetLegList.Line != null)
				{
					eps_Point3 = new Eps_Point(pOffsetLegList.Line.StartPnt);
				}
				else if (pOffsetLegList.Arc != null)
				{
					eps_Point3 = new Eps_Point(pOffsetLegList.Arc.StartPnt);
				}
				else
				{
					num = 1600;
				}
				if (num == 0)
				{
					eps_Leg3 = new Eps_Leg();
					eps_Leg3.Arc = new Eps_Arc();
					eps_Leg3.Arc.StartPnt = new Eps_Point(eps_Point2);
					eps_Leg3.Arc.EndPnt = new Eps_Point(eps_Point3);
					eps_Leg3.Arc.Delta = Math.PI;
					Eps_Point _MidPoint = new Eps_Point();
					EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(eps_Point2, eps_Point3, ref _MidPoint);
					eps_Leg3.Arc.RadiusPnt = new Eps_Point(_MidPoint);
					double _dDistance = 0.0;
					EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Point2, eps_Leg3.Arc.RadiusPnt, ref _dDistance);
					eps_Leg3.Arc.Radius = _dDistance;
					num2 = (eps_Point2.x - eps_Point.x) * (eps_Point3.y - eps_Point.y) - (eps_Point3.x - eps_Point.x) * (eps_Point2.y - eps_Point.y);
					if (num2 > 0.0)
					{
						eps_Leg3.Arc.Direction = 1;
					}
					else
					{
						eps_Leg3.Arc.Direction = -1;
					}
					pLeg.Next = eps_Leg3;
					eps_Leg3 = null;
				}
			}
			else
			{
				flag = true;
			}
		}
		eps_Leg = Eps_Leg.CopyLeg(pOffsetLegList);
		eps_Leg2 = Eps_Leg.CopyLeg(pOffsetLegList.Next);
		pOffsetLegList = null;
		while (eps_Leg2 != null)
		{
			if (IntersectTwoLegs(eps_Leg, eps_Leg2, ref pIntersectionPoint2) == 0)
			{
				TrimEndOfLeg(ref eps_Leg, pIntersectionPoint2);
				TrimStartOfLeg(ref eps_Leg2, pIntersectionPoint2);
			}
			else
			{
				if (eps_Leg.Line != null)
				{
					eps_Point = new Eps_Point(eps_Leg.Line.StartPnt);
					eps_Point2 = new Eps_Point(eps_Leg.Line.EndPnt);
				}
				else if (eps_Leg2.Arc != null)
				{
					eps_Point = new Eps_Point(eps_Leg.Arc.StartPnt);
					eps_Point2 = new Eps_Point(eps_Leg.Arc.EndPnt);
				}
				else
				{
					num = 1600;
				}
				if (eps_Leg2.Line != null)
				{
					eps_Point3 = eps_Leg2.Line.StartPnt;
				}
				else if (eps_Leg2.Arc != null)
				{
					eps_Point3 = eps_Leg2.Arc.StartPnt;
				}
				else
				{
					num = 1600;
				}
				if (num == 0)
				{
					eps_Leg3 = new Eps_Leg();
					eps_Leg3.Arc = new Eps_Arc();
					eps_Leg3.Arc.StartPnt = new Eps_Point(eps_Point2);
					eps_Leg3.Arc.EndPnt = new Eps_Point(eps_Point3);
					eps_Leg3.Arc.Delta = Math.PI;
					Eps_Point _MidPoint2 = new Eps_Point();
					EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(eps_Point2, eps_Point3, ref _MidPoint2);
					eps_Leg3.Arc.RadiusPnt = new Eps_Point(_MidPoint2);
					double _dDistance2 = 0.0;
					EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Point2, eps_Leg3.Arc.RadiusPnt, ref _dDistance2);
					eps_Leg3.Arc.Radius = _dDistance2;
					num2 = (eps_Point2.x - eps_Point.x) * (eps_Point3.y - eps_Point.y) - (eps_Point3.x - eps_Point.x) * (eps_Point2.y - eps_Point.y);
					if (num2 > 0.0)
					{
						eps_Leg3.Arc.Direction = 1;
					}
					else
					{
						eps_Leg3.Arc.Direction = -1;
					}
					eps_Leg3.Next = eps_Leg2;
					eps_Leg2 = (eps_Leg.Next = eps_Leg3);
					eps_Leg3 = null;
				}
			}
			if (pOffsetLegList == null)
			{
				pOffsetLegList = Eps_Leg.SingleLeg(eps_Leg);
			}
			else
			{
				pOffsetLegList.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
			}
			eps_Leg = eps_Leg.Next;
			eps_Leg2 = eps_Leg2.Next;
		}
		if (pOffsetLegList == null)
		{
			pOffsetLegList = Eps_Leg.SingleLeg(eps_Leg);
		}
		else
		{
			pOffsetLegList.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
		}
		if (bClosedLegList && flag)
		{
			TrimEndOfLeg(ref pLeg, pIntersectionPoint);
			TrimStartOfLeg(ref pOffsetLegList, pIntersectionPoint);
		}
	}

	private static void TrimAdjacentArcs(ref Eps_Leg pOffsetLegList, bool bClosedLegList)
	{
		int num = 0;
		Eps_Point pClosestPoint = new Eps_Point();
		Eps_Point[] paIntersections = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		int num2 = 0;
		int legCount = pOffsetLegList.LegCount;
		if (legCount < 2 || (legCount == 2 && bClosedLegList))
		{
			return;
		}
		Eps_Leg pLeg = Eps_Leg.CopyLeg(pOffsetLegList);
		Eps_Leg pLeg2 = Eps_Leg.CopyLeg(pOffsetLegList.Next);
		pOffsetLegList = null;
		while (pLeg2 != null)
		{
			if (pLeg.Arc != null && pLeg2.Arc != null)
			{
				num2 = 0;
				num = eplGeoE_IntersectTwoLegs(pLeg, pLeg2, ref paIntersections, ref num2, 0.01);
				if (num != 1600 && num2 > 1)
				{
					GetClosestPointAlongArc(pLeg.Arc, paIntersections, ref pClosestPoint);
					TrimEndOfLeg(ref pLeg, pClosestPoint);
					TrimStartOfLeg(ref pLeg2, pClosestPoint);
				}
			}
			if (pOffsetLegList == null)
			{
				pOffsetLegList = Eps_Leg.SingleLeg(pLeg);
			}
			else
			{
				pOffsetLegList.Last.Next = Eps_Leg.SingleLeg(pLeg);
			}
			pLeg = pLeg.Next;
			pLeg2 = pLeg2.Next;
		}
		if (pOffsetLegList == null)
		{
			pOffsetLegList = Eps_Leg.SingleLeg(pLeg);
		}
		else
		{
			pOffsetLegList.Last.Next = Eps_Leg.SingleLeg(pLeg);
		}
	}

	private static int IntersectTwoLegs(Eps_Leg pCurrLeg, Eps_Leg pNextLeg, ref Eps_Point pIntersectionPoint)
	{
		int result = 1600;
		if (pCurrLeg.Arc != null)
		{
			if (pNextLeg.Arc != null)
			{
				result = IntersectTwoArcs(pCurrLeg.Arc, pNextLeg.Arc, ref pIntersectionPoint);
			}
			else if (pNextLeg.Line != null)
			{
				result = IntersectArcLine(pNextLeg.Line, pCurrLeg.Arc, ref pIntersectionPoint);
			}
		}
		else if (pCurrLeg.Line != null)
		{
			if (pNextLeg.Arc != null)
			{
				result = IntersectLineArc(pCurrLeg.Line, pNextLeg.Arc, ref pIntersectionPoint);
			}
			else if (pNextLeg.Line != null)
			{
				result = IntersectTwoLines(pCurrLeg.Line, pNextLeg.Line, ref pIntersectionPoint);
			}
		}
		return result;
	}

	private static int IntersectTwoLines(Eps_Line pCurrLine, Eps_Line pNextLine, ref Eps_Point pIntersectionPoint)
	{
		double _dDirection = 0.0;
		double _dDirection2 = 0.0;
		double _dDirection3 = 0.0;
		double _dDirection4 = 0.0;
		if (EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(pCurrLine.StartPnt, pCurrLine.EndPnt, ref _dDirection) != 0)
		{
			return 1600;
		}
		if (EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(pNextLine.EndPnt, pNextLine.StartPnt, ref _dDirection2) != 0)
		{
			return 1600;
		}
		int num = EPShared.eplGeoE_IntersectTwoDirs2D(pCurrLine.StartPnt, _dDirection, pNextLine.EndPnt, _dDirection2, ref pIntersectionPoint, 1E-05);
		if (num == 1103)
		{
			if (EPShared.eplGeoE_ArePntsDuplicates2D(pCurrLine.EndPnt, pNextLine.StartPnt, 1E-05))
			{
				pIntersectionPoint = new Eps_Point(pCurrLine.EndPnt);
				return 0;
			}
			if (EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(pCurrLine.EndPnt, pNextLine.StartPnt, ref _dDirection3) != 0)
			{
				return 1600;
			}
			if (EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(pNextLine.EndPnt, pCurrLine.StartPnt, ref _dDirection4) != 0)
			{
				return 1600;
			}
			if (RevitDefines.EQ(_dDirection3, _dDirection) || RevitDefines.EQ(_dDirection3, _dDirection2) || RevitDefines.EQ(_dDirection4, _dDirection) || RevitDefines.EQ(_dDirection4, _dDirection2))
			{
				pIntersectionPoint = new Eps_Point(pCurrLine.EndPnt);
				return 0;
			}
			return 1600;
		}
		return 0;
	}

	private static int IntersectLineArc(Eps_Line pCurrLine, Eps_Arc pNextArc, ref Eps_Point pIntersectionPoint)
	{
		Eps_Point[] InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		int num;
		switch (eplGeoE_IntersectLineArc(pCurrLine, pNextArc, ref InterPnt, 0.01))
		{
		case 1606:
		case 1612:
			pIntersectionPoint = new Eps_Point(InterPnt[0]);
			num = 0;
			break;
		case 1607:
		case 1613:
			GetClosestPoint(pCurrLine.EndPnt, InterPnt, ref pIntersectionPoint);
			num = 0;
			break;
		default:
		{
			num = 1600;
			double _dDirection = 0.0;
			double num2 = 0.0;
			Eps_Point[] array = new Eps_Point[2]
			{
				new Eps_Point(),
				new Eps_Point()
			};
			if (EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(pCurrLine.StartPnt, pCurrLine.EndPnt, ref _dDirection) != 0)
			{
				return 1600;
			}
			num2 = pNextArc.Radius;
			switch (eplGeoE_IntersectDirDist2D(pCurrLine.StartPnt, _dDirection, pNextArc.RadiusPnt, num2, ref InterPnt, 0.01))
			{
			case 1606:
				array[0] = new Eps_Point(InterPnt[0]);
				pIntersectionPoint = new Eps_Point(InterPnt[0]);
				num = eplGeoE_IntersectDirDist2D(pCurrLine.EndPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), pNextArc.RadiusPnt, num2, ref InterPnt, 0.01);
				if (num == 1606)
				{
					array[1] = new Eps_Point(InterPnt[0]);
					GetClosestPoint(pCurrLine.EndPnt, array, ref pIntersectionPoint);
					num = 0;
				}
				break;
			case 1607:
				GetClosestPoint(pCurrLine.EndPnt, InterPnt, ref pIntersectionPoint);
				num = 0;
				break;
			default:
				num = 1600;
				break;
			}
			break;
		}
		}
		return num;
	}

	private static int IntersectArcLine(Eps_Line pNextLine, Eps_Arc pCurrArc, ref Eps_Point pIntersectionPoint)
	{
		Eps_Point[] InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		switch (eplGeoE_IntersectLineArc(pNextLine, pCurrArc, ref InterPnt, 0.01))
		{
		case 1606:
		case 1612:
			pIntersectionPoint = new Eps_Point(InterPnt[0]);
			return 0;
		case 1607:
		case 1613:
			GetClosestPoint(pNextLine.EndPnt, InterPnt, ref pIntersectionPoint);
			return 0;
		default:
		{
			int num = 1600;
			double _dDirection = 0.0;
			double num2 = 0.0;
			Eps_Point[] array = new Eps_Point[2]
			{
				new Eps_Point(),
				new Eps_Point()
			};
			if (EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(pNextLine.EndPnt, pNextLine.StartPnt, ref _dDirection) != 0)
			{
				return 1600;
			}
			num2 = pCurrArc.Radius;
			switch (eplGeoE_IntersectDirDist2D(pNextLine.EndPnt, _dDirection, pCurrArc.RadiusPnt, num2, ref InterPnt, 0.01))
			{
			case 1606:
				array[0] = new Eps_Point(InterPnt[0]);
				pIntersectionPoint = new Eps_Point(InterPnt[0]);
				num = eplGeoE_IntersectDirDist2D(pNextLine.StartPnt, EPShared.eplGeoE_NormalizeRadians(_dDirection + Math.PI), pCurrArc.RadiusPnt, num2, ref InterPnt, 0.01);
				if (num == 1606)
				{
					array[1] = new Eps_Point(InterPnt[0]);
					GetClosestPoint(pCurrArc.EndPnt, array, ref pIntersectionPoint);
					num = 0;
				}
				return 0;
			case 1607:
				GetClosestPoint(pCurrArc.EndPnt, InterPnt, ref pIntersectionPoint);
				return 0;
			default:
				return 1600;
			}
		}
		}
	}

	private static int IntersectTwoArcs(Eps_Arc pCurrArc, Eps_Arc pNextArc, ref Eps_Point pIntersectionPoint)
	{
		Eps_Point[] InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		int result;
		switch (eplGeoE_IntersectTwoDists2D(pCurrArc.RadiusPnt, pCurrArc.Radius, pNextArc.RadiusPnt, pNextArc.Radius, ref InterPnt, 0.01))
		{
		case 1606:
			pIntersectionPoint = new Eps_Point(InterPnt[0]);
			result = 0;
			break;
		case 1607:
			GetClosestPoint(pCurrArc.EndPnt, InterPnt, ref pIntersectionPoint);
			result = 0;
			break;
		case 1604:
			result = 0;
			if (EPShared.eplGeoE_ArePntsDuplicates3D(pCurrArc.EndPnt, pNextArc.StartPnt))
			{
				pIntersectionPoint = new Eps_Point(pNextArc.StartPnt);
			}
			else if (EPShared.eplGeoE_ArePntsDuplicates3D(pCurrArc.StartPnt, pNextArc.EndPnt))
			{
				pIntersectionPoint = new Eps_Point(pNextArc.EndPnt);
			}
			else
			{
				result = 1600;
			}
			break;
		default:
			result = 1600;
			break;
		}
		return result;
	}

	private static void GetClosestPoint(Eps_Point pPoint, Eps_Point[] paPoints, ref Eps_Point pClosestPoint)
	{
		double _dDistance = 0.0;
		double _dDistance2 = 0.0;
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(pPoint, paPoints[0], ref _dDistance);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(pPoint, paPoints[1], ref _dDistance2);
		if (_dDistance <= _dDistance2)
		{
			pClosestPoint = new Eps_Point(paPoints[0]);
		}
		else
		{
			pClosestPoint = new Eps_Point(paPoints[1]);
		}
	}

	public static void TrimStartOfLeg(ref Eps_Leg pLeg, Eps_Point pTrimPoint)
	{
		bool flag = false;
		if (pLeg.Arc != null)
		{
			pTrimPoint.z = pLeg.Arc.StartPnt.z;
			flag = EPShared.eplGeoE_ArePntsDuplicates2D(pLeg.Arc.StartPnt, pTrimPoint);
			pLeg.Arc.StartPnt = new Eps_Point(pTrimPoint);
			if (!flag)
			{
				Eps_Arc _ArcStruct = new Eps_Arc();
				EPShared.eplGeoE_CalcArcStructFromPCAndPTAndRP(pLeg.Arc.StartPnt, pLeg.Arc.RadiusPnt, pLeg.Arc.EndPnt, pLeg.Arc.Direction, ref _ArcStruct);
				pLeg.Arc = _ArcStruct;
			}
		}
		else if (pLeg.Line != null)
		{
			pTrimPoint.z = pLeg.Line.StartPnt.z;
			pLeg.Line.StartPnt = new Eps_Point(pTrimPoint);
		}
	}

	public static void TrimEndOfLeg(ref Eps_Leg pLeg, Eps_Point pTrimPoint)
	{
		bool flag = false;
		if (pLeg.Arc != null)
		{
			pTrimPoint.z = pLeg.Arc.EndPnt.z;
			flag = EPShared.eplGeoE_ArePntsDuplicates2D(pLeg.Arc.EndPnt, pTrimPoint);
			pLeg.Arc.EndPnt = new Eps_Point(pTrimPoint);
			if (!flag)
			{
				Eps_Arc _ArcStruct = new Eps_Arc();
				EPShared.eplGeoE_CalcArcStructFromPCAndPTAndRP(pLeg.Arc.StartPnt, pLeg.Arc.RadiusPnt, pLeg.Arc.EndPnt, pLeg.Arc.Direction, ref _ArcStruct);
				pLeg.Arc = _ArcStruct;
			}
		}
		else if (pLeg.Line != null)
		{
			pTrimPoint.z = pLeg.Line.EndPnt.z;
			pLeg.Line.EndPnt = new Eps_Point(pTrimPoint);
		}
	}

	private static void GetClosestPointAlongArc(Eps_Arc pArc, Eps_Point[] paPoints, ref Eps_Point pClosestPoint)
	{
		double _dStation = 99999.0;
		double _dStation2 = 99999.0;
		double _dOffset = 0.0;
		EPShared.eplGeoE_CalcStationOffsetAlongArc(paPoints[0], pArc, ref _dStation, ref _dOffset);
		EPShared.eplGeoE_CalcStationOffsetAlongArc(paPoints[1], pArc, ref _dStation2, ref _dOffset);
		if (_dStation <= _dStation2)
		{
			pClosestPoint = new Eps_Point(paPoints[0]);
		}
		else
		{
			pClosestPoint = new Eps_Point(paPoints[1]);
		}
	}

	public static void CalcMinMax(Eps_Point pPoint, ref Eps_Point pMinPnt, ref Eps_Point pMaxPnt)
	{
		pMinPnt.x = Math.Min(pMinPnt.x, pPoint.x);
		pMaxPnt.x = Math.Max(pMaxPnt.x, pPoint.x);
		pMinPnt.y = Math.Min(pMinPnt.y, pPoint.y);
		pMaxPnt.y = Math.Max(pMaxPnt.y, pPoint.y);
		pMinPnt.z = Math.Min(pMinPnt.z, pPoint.z);
		pMaxPnt.z = Math.Max(pMaxPnt.z, pPoint.z);
	}

	private static int CheckLinesForCommonEndPnts(Eps_Line LineOne, Eps_Line LineTwo, double PointTolerance)
	{
		int result = 1110;
		bool flag = EPShared.eplGeoE_ArePntsDuplicates2D(LineOne.StartPnt, LineTwo.StartPnt, PointTolerance);
		bool flag2 = EPShared.eplGeoE_ArePntsDuplicates2D(LineOne.EndPnt, LineTwo.EndPnt, PointTolerance);
		bool flag3 = EPShared.eplGeoE_ArePntsDuplicates2D(LineOne.StartPnt, LineTwo.EndPnt, PointTolerance);
		bool flag4 = EPShared.eplGeoE_ArePntsDuplicates2D(LineOne.EndPnt, LineTwo.StartPnt, PointTolerance);
		if (flag && flag2)
		{
			result = 1108;
		}
		else if (flag3 && flag4)
		{
			result = 1109;
		}
		else if (flag)
		{
			result = 1614;
		}
		else if (flag2)
		{
			result = 1615;
		}
		else if (flag3)
		{
			result = 1616;
		}
		else if (flag4)
		{
			result = 1617;
		}
		return result;
	}

	private static int ChkIfArcsOverlapOrWithIn2D(Eps_Arc ArcOne, Eps_Arc ArcTwo, ref Eps_Point[] InterPnt, double PointTolerance)
	{
		int num = 1600;
		InterPnt[0].z = ArcOne.StartPnt.z;
		InterPnt[1].z = ArcOne.StartPnt.z;
		bool flag = ((ArcOne.Direction == ArcTwo.Direction) ? true : false);
		bool flag2 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.StartPnt, ArcTwo.StartPnt, PointTolerance);
		bool flag3 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.EndPnt, ArcTwo.EndPnt, PointTolerance);
		bool flag4 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.StartPnt, ArcTwo.EndPnt, PointTolerance);
		bool flag5 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.EndPnt, ArcTwo.StartPnt, PointTolerance);
		if (flag2 || flag4 || flag5 || flag3)
		{
			num = 1612;
		}
		bool flag6 = EPShared.eplGeoE_IsPointOnArc2D(ArcOne.StartPnt, ArcTwo, PointTolerance);
		bool flag7 = EPShared.eplGeoE_IsPointOnArc2D(ArcOne.EndPnt, ArcTwo, PointTolerance);
		bool flag8 = EPShared.eplGeoE_IsPointOnArc2D(ArcTwo.StartPnt, ArcOne, PointTolerance);
		bool flag9 = EPShared.eplGeoE_IsPointOnArc2D(ArcTwo.EndPnt, ArcOne, PointTolerance);
		if (EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.RadiusPnt, ArcTwo.RadiusPnt, PointTolerance) && Math.Abs(ArcOne.Radius - ArcTwo.Radius) <= 1E-05)
		{
			if (ArcOne.Delta + ArcTwo.Delta > Math.PI * 2.0 && flag6 && flag7 && flag8 && flag9)
			{
				if (ArcOne.Delta > ArcTwo.Delta)
				{
					InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
					InterPnt[1] = new Eps_Point(ArcOne.EndPnt);
				}
				else
				{
					InterPnt[0] = new Eps_Point(ArcTwo.StartPnt);
					InterPnt[1] = new Eps_Point(ArcTwo.EndPnt);
				}
				if (num == 1612)
				{
					return 1619;
				}
				return 1620;
			}
			if (num == 1612)
			{
				if (flag2)
				{
					if (flag7)
					{
						InterPnt[1] = new Eps_Point(ArcOne.EndPnt);
						return 1618;
					}
					if (flag9)
					{
						InterPnt[1] = new Eps_Point(ArcTwo.EndPnt);
						return 1618;
					}
					return 1612;
				}
				if (flag3)
				{
					if (flag6)
					{
						InterPnt[1] = new Eps_Point(ArcOne.StartPnt);
						return 1618;
					}
					if (flag8)
					{
						InterPnt[1] = new Eps_Point(ArcTwo.StartPnt);
						return 1618;
					}
					return 1612;
				}
				if (flag4)
				{
					if (flag7)
					{
						InterPnt[1] = new Eps_Point(ArcOne.EndPnt);
						return 1618;
					}
					if (flag8)
					{
						InterPnt[1] = new Eps_Point(ArcTwo.StartPnt);
						return 1618;
					}
					return 1612;
				}
				if (flag5)
				{
					if (flag6)
					{
						InterPnt[1] = new Eps_Point(ArcOne.StartPnt);
						return 1618;
					}
					if (flag9)
					{
						InterPnt[1] = new Eps_Point(ArcTwo.EndPnt);
						return 1618;
					}
					return 1612;
				}
			}
			if (flag6 && flag7)
			{
				InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
				InterPnt[1] = new Eps_Point(ArcOne.EndPnt);
				return 1610;
			}
			if (flag8 && flag9)
			{
				InterPnt[0] = new Eps_Point(ArcTwo.StartPnt);
				InterPnt[1] = new Eps_Point(ArcTwo.EndPnt);
				return 1611;
			}
			if (flag6 && flag9)
			{
				InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
				InterPnt[1] = new Eps_Point(ArcTwo.EndPnt);
				return 1608;
			}
			if (flag6 && flag8)
			{
				InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
				InterPnt[1] = new Eps_Point(ArcTwo.StartPnt);
				return 1608;
			}
			if (flag7 && flag8)
			{
				InterPnt[0] = new Eps_Point(ArcOne.EndPnt);
				InterPnt[1] = new Eps_Point(ArcTwo.StartPnt);
				return 1608;
			}
			if (flag7 && flag9)
			{
				InterPnt[0] = new Eps_Point(ArcOne.EndPnt);
				InterPnt[1] = new Eps_Point(ArcTwo.EndPnt);
				return 1608;
			}
		}
		return 1609;
	}

	private static bool IsPntInBoundingBoxOfLine(Eps_Point pPoint, Eps_Line pLine, double Tolerance)
	{
		if (RevitDefines.LT(Math.Max(pLine.StartPnt.x, pLine.EndPnt.x), pPoint.x, Tolerance))
		{
			return false;
		}
		if (RevitDefines.GT(Math.Min(pLine.StartPnt.x, pLine.EndPnt.x), pPoint.x, Tolerance))
		{
			return false;
		}
		if (RevitDefines.LT(Math.Max(pLine.StartPnt.y, pLine.EndPnt.y), pPoint.y, Tolerance))
		{
			return false;
		}
		if (RevitDefines.GT(Math.Min(pLine.StartPnt.y, pLine.EndPnt.y), pPoint.y, Tolerance))
		{
			return false;
		}
		return true;
	}

	private static void CalculateInterPnt(Eps_Line pLineOne, Eps_Line pLineTwo, ref Eps_Point[] aInterPnt)
	{
		double num = 0.0;
		Eps_Point _Vector = new Eps_Point();
		Eps_Point _Vector2 = new Eps_Point();
		Eps_Point _Vector3 = new Eps_Point();
		double num2 = 0.0;
		double num3 = 0.0;
		EPShared.eplGeoE_MakeVector(pLineOne.StartPnt, pLineOne.EndPnt, ref _Vector);
		EPShared.eplGeoE_MakeVector(pLineTwo.StartPnt, pLineTwo.EndPnt, ref _Vector2);
		EPShared.eplGeoE_MakeVector(pLineOne.StartPnt, pLineTwo.EndPnt, ref _Vector3);
		num2 = EPShared.eplCalE_CalcComponentKOfCrossProduct(_Vector, _Vector2);
		num3 = EPShared.eplCalE_CalcComponentKOfCrossProduct(_Vector3, _Vector2);
		if (RevitDefines.EQ(num2, 0.0, 1E-10))
		{
			num2 = 1E-10;
		}
		num = Math.Abs(num3 / num2);
		aInterPnt[0].x = pLineOne.StartPnt.x + num * _Vector.x;
		aInterPnt[0].y = pLineOne.StartPnt.y + num * _Vector.y;
		aInterPnt[0].z = 0.0;
	}

	public static double CalculateDirection(double _dHorizontal, double _dVertical)
	{
		return Math.Atan(_dVertical / _dHorizontal);
	}

	public static int eplGeoE_CalcMidPointBetweenTwoPoints3D(Eps_Point _StartPnt, Eps_Point _EndPnt, ref Eps_Point _MidPoint)
	{
		int num = EPShared.eplGeoE_CalcMidPointBetweenTwoPoints(_StartPnt, _EndPnt, ref _MidPoint);
		if (num == 0)
		{
			_MidPoint.z = (_StartPnt.z + _EndPnt.z) / 2.0;
		}
		return num;
	}

	public static int AddVertexToLegList(Eps_Leg _CurrLegList, Eps_Point _AddPoint, ref Eps_Leg _NewLegList)
	{
		double _dStation = 0.0;
		double _dOffset = 0.0;
		double num = 0.0;
		double _dDistance = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Point eps_Point = null;
		int num4 = 506;
		if (eplGeoE_CalcStationOffsetAlongLegList(_AddPoint, _CurrLegList, ref _dStation, ref _dOffset, _bAlwaysSetStationOffset: true) == 1700)
		{
			for (eps_Leg2 = _CurrLegList; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
			{
				double num5 = eplGeoE_CalcLegLength(eps_Leg2);
				if (RevitDefines.LE(num5, 0.0) && num + num5 < _dStation)
				{
					num += num5;
				}
				else if (num4 != 0 && _dStation - (num + num5) <= 1E-05)
				{
					Eps_Point eps_Point2 = null;
					Eps_Point eps_Point3 = null;
					if (eps_Leg2.Line != null)
					{
						eps_Point2 = new Eps_Point(eps_Leg2.Line.StartPnt);
						eps_Point3 = new Eps_Point(eps_Leg2.Line.EndPnt);
						EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Point2, eps_Point3, ref _dDistance);
					}
					else if (eps_Leg2.Arc != null)
					{
						eps_Point2 = new Eps_Point(eps_Leg2.Arc.StartPnt);
						eps_Point3 = new Eps_Point(eps_Leg2.Arc.EndPnt);
						_dDistance = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Leg2.Arc.Radius, eps_Leg2.Arc.Delta);
					}
					if (EPShared.eplGeoE_ArePntsDuplicates2D(_AddPoint, eps_Point2) || EPShared.eplGeoE_ArePntsDuplicates2D(_AddPoint, eps_Point3))
					{
						return 1;
					}
					num2 = eps_Point3.z - eps_Point2.z;
					num3 = num2 / _dDistance;
					num2 = num3 * (_dStation - num);
					eps_Leg = new Eps_Leg();
					eps_Point = new Eps_Point(_AddPoint);
					eps_Point.z = eps_Point2.z + num2;
					eps_Leg.Line = new Eps_Line();
					eps_Leg.Line.StartPnt = new Eps_Point(eps_Point2);
					eps_Leg.Line.EndPnt = new Eps_Point(eps_Point);
					if (_NewLegList == null)
					{
						_NewLegList = Eps_Leg.SingleLeg(eps_Leg);
					}
					else
					{
						_NewLegList.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
					}
					eps_Leg = new Eps_Leg();
					eps_Leg.Line = new Eps_Line();
					eps_Leg.Line.StartPnt = new Eps_Point(eps_Point);
					eps_Leg.Line.EndPnt = new Eps_Point(eps_Point3);
					_NewLegList.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
					num4 = 0;
				}
				else
				{
					num += num5;
					if (_NewLegList == null)
					{
						_NewLegList = Eps_Leg.SingleLeg(eps_Leg2);
					}
					else
					{
						_NewLegList.Last.Next = Eps_Leg.SingleLeg(eps_Leg2);
					}
				}
			}
		}
		else if (RevitDefines.LE(_dStation, 0.0))
		{
			if (_CurrLegList.Line != null)
			{
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_CurrLegList.Line.StartPnt, _CurrLegList.Line.EndPnt, ref _dDistance);
				num2 = _CurrLegList.Line.EndPnt.z - _CurrLegList.Line.StartPnt.z;
			}
			else if (_CurrLegList.Arc != null)
			{
				_dDistance = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_CurrLegList.Arc.Radius, _CurrLegList.Arc.Delta);
				num2 = _CurrLegList.Arc.EndPnt.z - _CurrLegList.Arc.StartPnt.z;
			}
			num3 = num2 / _dDistance;
			num2 = num3 * Math.Abs(_dStation);
			eps_Point = new Eps_Point(_AddPoint);
			eps_Point.z = _CurrLegList.LegStartPnt.z - num2;
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = new Eps_Line();
			eps_Leg.Line.StartPnt = new Eps_Point(eps_Point);
			eps_Leg.Line.EndPnt = new Eps_Point(_CurrLegList.LegStartPnt);
			_NewLegList = Eps_Leg.SingleLeg(eps_Leg);
			_NewLegList.Last.Next = Eps_Leg.CopyLeg(_CurrLegList);
		}
		else
		{
			if (_CurrLegList.Last.Line != null)
			{
				EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_CurrLegList.Last.Line.StartPnt, _CurrLegList.Last.Line.EndPnt, ref _dDistance);
				num2 = _CurrLegList.Last.Line.EndPnt.z - _CurrLegList.Last.Line.StartPnt.z;
			}
			else if (_CurrLegList.Last.Arc != null)
			{
				_dDistance = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_CurrLegList.Last.Arc.Radius, _CurrLegList.Last.Arc.Delta);
				num2 = _CurrLegList.Last.Arc.EndPnt.z - _CurrLegList.Last.Arc.StartPnt.z;
			}
			num3 = num2 / _dDistance;
			num2 = num3 * (_dStation - eplGeoE_CalcLegListLength(_CurrLegList));
			eps_Point = new Eps_Point(_AddPoint);
			eps_Point.z = _CurrLegList.LegEndPnt.z + num2;
			_NewLegList = Eps_Leg.CopyLeg(_CurrLegList);
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = new Eps_Line();
			eps_Leg.Line.StartPnt = new Eps_Point(_CurrLegList.LegEndPnt);
			eps_Leg.Line.EndPnt = new Eps_Point(eps_Point);
			_NewLegList.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
		}
		return num4;
	}

	public static List<OffsetLegWrapper> InsertLegsAtStations(Eps_Leg _OriginalLeg, List<double> _OffsetStations, List<Eps_Leg> _AddLegs, List<double> _CenterIntStations, bool _bInsertFirstLeg)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		double num = 0.0;
		double _dStation = 0.0;
		double _dOffset = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		Eps_Point eps_Point = null;
		bool flag = false;
		bool flag2 = _bInsertFirstLeg;
		int i = 0;
		List<OffsetLegWrapper> list = new List<OffsetLegWrapper>();
		OffsetLegWrapper offsetLegWrapper = null;
		for (eps_Leg = _OriginalLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			num = eplGeoE_CalcLegLength(eps_Leg);
			eps_Leg2 = new Eps_Leg();
			if (eps_Leg.Line != null)
			{
				eps_Leg2.Line = new Eps_Line(eps_Leg.Line);
				for (; i < _OffsetStations.Count && _OffsetStations[i] <= num2 + num; i++)
				{
					if (flag2)
					{
						if (!flag)
						{
							offsetLegWrapper = new OffsetLegWrapper();
							offsetLegWrapper.OriginalLeg = Eps_Leg.CopyLeg(_AddLegs[i]);
							offsetLegWrapper.Station = _CenterIntStations[i];
						}
						if (eps_Leg2 == null)
						{
							eps_Leg2 = new Eps_Leg();
							eps_Leg2.Line = new Eps_Line(eps_Leg.Line);
						}
						if (eplGeoE_CalcStationOffsetAlongLegList(_AddLegs[i].LegEndPnt, Eps_Leg.SingleLeg(eps_Leg), ref _dStation, ref _dOffset) != 1700)
						{
							flag = true;
							eps_Leg2 = null;
							break;
						}
						flag = false;
						eps_Leg2.Line.StartPnt = _AddLegs[i].LegEndPnt;
						eps_Leg2.Line.EndPnt = eps_Leg.Line.EndPnt;
					}
					else
					{
						if (offsetLegWrapper == null)
						{
							offsetLegWrapper = new OffsetLegWrapper();
							offsetLegWrapper.Station = 0.0;
						}
						if (eps_Leg2 != null)
						{
							eps_Leg2.Line.EndPnt = _AddLegs[i].LegStartPnt;
							if (offsetLegWrapper.OriginalLeg == null)
							{
								offsetLegWrapper.OriginalLeg = Eps_Leg.CopyLeg(eps_Leg2);
							}
							else
							{
								offsetLegWrapper.OriginalLeg.Last.Next = Eps_Leg.CopyLeg(eps_Leg2);
							}
						}
						if (offsetLegWrapper.OriginalLeg == null)
						{
							offsetLegWrapper.OriginalLeg = Eps_Leg.CopyLeg(_AddLegs[i]);
						}
						else
						{
							offsetLegWrapper.OriginalLeg.Last.Next = Eps_Leg.CopyLeg(_AddLegs[i]);
						}
						offsetLegWrapper.EndingStation = _CenterIntStations[i];
						list.Add(offsetLegWrapper);
						offsetLegWrapper = null;
					}
					if (eps_Leg2 != null && eps_Leg2.Line.StartPnt.IsEqual(eps_Leg2.Line.EndPnt))
					{
						eps_Leg2 = null;
					}
					flag2 = !flag2;
				}
				if (flag2 || eps_Leg2.Line.StartPnt.IsEqual(eps_Leg2.Line.EndPnt))
				{
					eps_Leg2 = null;
				}
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Leg2.Arc = new Eps_Arc(eps_Leg.Arc);
				num3 = eps_Leg2.Arc.Radius;
				eps_Point = eps_Leg2.Arc.RadiusPnt;
				for (; i < _OffsetStations.Count && _OffsetStations[i] <= num2 + num; i++)
				{
					if (flag2)
					{
						if (!flag)
						{
							offsetLegWrapper = new OffsetLegWrapper();
							offsetLegWrapper.OriginalLeg = Eps_Leg.CopyLeg(_AddLegs[i]);
							offsetLegWrapper.Station = _CenterIntStations[i];
						}
						if (eps_Leg2 == null)
						{
							eps_Leg2 = new Eps_Leg();
							eps_Leg2.Arc = new Eps_Arc(eps_Leg.Arc);
						}
						else
						{
							if (eplGeoE_CalcStationOffsetAlongLegList(_AddLegs[i].LegEndPnt, Eps_Leg.SingleLeg(eps_Leg), ref _dStation, ref _dOffset) != 1700)
							{
								flag = true;
								eps_Leg2 = null;
								break;
							}
							flag = false;
						}
						eps_Leg2.Arc.StartPnt = _AddLegs[i].LegEndPnt;
						eps_Leg2.Arc.EndPnt = eps_Leg.Arc.EndPnt;
						eps_Leg2.Arc.RadiusPnt = eps_Point;
						eps_Leg2.Arc.RadiusPnt.z = (eps_Leg2.Arc.StartPnt.z + eps_Leg2.Arc.EndPnt.z) / 2.0;
						eps_Leg2.Arc.Radius = num3;
						eps_Leg2.Arc.Delta = (num - (_OffsetStations[i] - num2)) / eps_Leg2.Arc.Radius;
					}
					else
					{
						if (offsetLegWrapper == null)
						{
							offsetLegWrapper = new OffsetLegWrapper();
							offsetLegWrapper.Station = 0.0;
						}
						if (eps_Leg2 != null)
						{
							eps_Leg2.Arc.EndPnt = _AddLegs[i].LegStartPnt;
							eps_Leg2.Arc.RadiusPnt.z = (eps_Leg2.Arc.StartPnt.z + eps_Leg2.Arc.EndPnt.z) / 2.0;
							eps_Leg2.Arc.Delta = (_OffsetStations[i] - num2) / eps_Leg2.Arc.Radius;
							if (offsetLegWrapper.OriginalLeg == null)
							{
								offsetLegWrapper.OriginalLeg = Eps_Leg.CopyLeg(eps_Leg2);
							}
							else
							{
								offsetLegWrapper.OriginalLeg.Last.Next = Eps_Leg.CopyLeg(eps_Leg2);
							}
						}
						if (offsetLegWrapper.OriginalLeg == null)
						{
							offsetLegWrapper.OriginalLeg = Eps_Leg.CopyLeg(_AddLegs[i]);
						}
						else
						{
							offsetLegWrapper.OriginalLeg.Last.Next = Eps_Leg.CopyLeg(_AddLegs[i]);
						}
						offsetLegWrapper.EndingStation = _CenterIntStations[i];
						list.Add(offsetLegWrapper);
						offsetLegWrapper = null;
					}
					if (eps_Leg2 != null && eps_Leg2.Arc.StartPnt.IsEqual(eps_Leg2.Arc.EndPnt))
					{
						eps_Leg2 = null;
					}
					flag2 = !flag2;
				}
				if (flag2 || eps_Leg2.Arc.StartPnt.IsEqual(eps_Leg2.Arc.EndPnt))
				{
					eps_Leg2 = null;
				}
			}
			num2 += num;
			if (eps_Leg2 != null)
			{
				if (offsetLegWrapper == null)
				{
					offsetLegWrapper = new OffsetLegWrapper();
					offsetLegWrapper.Station = 0.0;
				}
				if (offsetLegWrapper.OriginalLeg == null)
				{
					offsetLegWrapper.OriginalLeg = Eps_Leg.CopyLeg(eps_Leg2);
				}
				else
				{
					offsetLegWrapper.OriginalLeg.Last.Next = Eps_Leg.CopyLeg(eps_Leg2);
				}
			}
		}
		if (offsetLegWrapper != null)
		{
			offsetLegWrapper.EndingStation = eplGeoE_CalcLegListLength(_OriginalLeg);
			list.Add(offsetLegWrapper);
		}
		return list;
	}

	public static int eplGeoE_FindPntOnLegByStationOffset(Eps_Leg StartOfLegList, double DistAlongObj, double Offset, ref Eps_Point Pnt)
	{
		Eps_Leg Leg = null;
		return eplGeoE_FindPntOnLegByStationOffset(StartOfLegList, DistAlongObj, Offset, ref Pnt, ref Leg);
	}

	public static int eplGeoE_FindPntOnLegByStationOffset(Eps_Leg StartOfLegList, double DistAlongObj, double Offset, ref Eps_Point Pnt, ref Eps_Leg Leg)
	{
		double num = 0.0;
		Eps_Leg eps_Leg = null;
		int result = 506;
		for (eps_Leg = StartOfLegList; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			double num2 = eplGeoE_CalcLegLength(eps_Leg);
			if (RevitDefines.LE(num2, 0.0) && num + num2 < DistAlongObj)
			{
				num += num2;
			}
			else
			{
				if (DistAlongObj - (num + num2) <= 1E-05)
				{
					FindStaOffPointAlongLeg(eps_Leg, DistAlongObj - num, Offset, ref Pnt);
					Leg = eps_Leg;
					result = 0;
					break;
				}
				num += num2;
			}
		}
		return result;
	}

	private static void FindStaOffPointAlongLeg(Eps_Leg Leg, double Station, double Offset, ref Eps_Point StaOffPnt)
	{
		Eps_Point eps_Point = null;
		Eps_Point eps_Point2 = null;
		double _dDistance = 0.0;
		double num = 0.0;
		double num2 = 0.0;
		if (Leg.Line != null)
		{
			EPShared.eplGeoE_FindStaOffPointAlongLine(Leg.Line, Station, Offset, ref StaOffPnt);
			eps_Point = new Eps_Point(Leg.Line.StartPnt);
			eps_Point2 = new Eps_Point(Leg.Line.EndPnt);
			EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Point, eps_Point2, ref _dDistance);
		}
		else if (Leg.Arc != null)
		{
			EPShared.eplGeoE_FindStaOffPointAlongArc(Leg.Arc, Station, Offset, ref StaOffPnt);
			eps_Point = new Eps_Point(Leg.Arc.StartPnt);
			eps_Point2 = new Eps_Point(Leg.Arc.EndPnt);
			_dDistance = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(Leg.Arc.Radius, Leg.Arc.Delta);
		}
		num = eps_Point2.z - eps_Point.z;
		num2 = num / _dDistance;
		num = num2 * Station;
		StaOffPnt.z = eps_Point.z + num;
	}

	public static int eplGeoE_CalcStationOffsetAlongLegList(Eps_Point _Point, Eps_Leg _LegList, ref double _dStation, ref double _dOffset)
	{
		return eplGeoE_CalcStationOffsetAlongLegList(_Point, _LegList, ref _dStation, ref _dOffset, _bAlwaysSetStationOffset: false);
	}

	public static int eplGeoE_CalcStationOffsetAlongLegList(Eps_Point _Point, Eps_Leg _LegList, ref double _dStation, ref double _dOffset, bool _bAlwaysSetStationOffset)
	{
		int num = 0;
		int num2 = 0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double _dStation2 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		Eps_Leg eps_Leg = null;
		num = 1701;
		num3 = double.MaxValue;
		num4 = double.MaxValue;
		num8 = 0.0;
		num7 = 0.0;
		_dStation = 0.0;
		_dOffset = 0.0;
		for (eps_Leg = _LegList; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			num9 = eplGeoE_CalcLegLength(eps_Leg);
			num2 = 1701;
			if (eps_Leg.Line != null)
			{
				num2 = EPShared.eplGeoE_CalcStationOffsetAlongLine(_Point, eps_Leg.Line, ref _dStation2, ref num7, 0.0);
			}
			else
			{
				if (eps_Leg.Arc == null)
				{
					return 700;
				}
				num2 = EPShared.eplGeoE_CalcStationOffsetAlongArc(_Point, eps_Leg.Arc, ref _dStation2, ref num7, 0.0);
			}
			if (num2 == 1700 && Math.Abs(num7) < Math.Abs(num3))
			{
				num = 1700;
				_dStation = _dStation2 + num8;
				_dOffset = num7;
				num3 = num7;
				if (RevitDefines.EQ(num7, 0.0))
				{
					return num;
				}
			}
			else if (num2 == 1701 && Math.Abs(num7) < Math.Abs(num4))
			{
				num5 = _dStation2 + num8;
				num6 = num7;
				num4 = num7;
			}
			num8 += num9;
		}
		if (_bAlwaysSetStationOffset && num == 1701)
		{
			_dStation = num5;
			_dOffset = num4;
		}
		return num;
	}

	private static int eplGeoE_IntersectTwoLegs(Eps_Leg pLegOne, Eps_Leg pLegTwo, ref Eps_Point[] paIntersections, ref int pcNumIntersections, double Tolerance)
	{
		int num = 1600;
		pcNumIntersections = 0;
		if (pLegOne.Line != null)
		{
			if (pLegTwo.Line != null)
			{
				num = eplGeoE_IntersectTwoLines(pLegOne.Line, pLegTwo.Line, ref paIntersections, ref pcNumIntersections, Tolerance);
			}
			else if (pLegTwo.Arc != null)
			{
				num = eplGeoE_IntersectLineArc(pLegOne.Line, pLegTwo.Arc, ref paIntersections, Tolerance);
				switch (num)
				{
				case 1600:
					pcNumIntersections = 0;
					break;
				case 1607:
				case 1613:
					pcNumIntersections = 2;
					break;
				default:
					pcNumIntersections = 1;
					break;
				}
			}
		}
		else if (pLegOne.Arc != null)
		{
			if (pLegTwo.Line != null)
			{
				num = eplGeoE_IntersectLineArc(pLegTwo.Line, pLegOne.Arc, ref paIntersections, Tolerance);
				switch (num)
				{
				case 1600:
					pcNumIntersections = 0;
					break;
				case 1607:
				case 1613:
					pcNumIntersections = 2;
					break;
				default:
					pcNumIntersections = 1;
					break;
				}
			}
			else if (pLegTwo.Arc != null)
			{
				num = eplGeoE_IntersectTwoArcs2D(pLegOne.Arc, pLegTwo.Arc, ref paIntersections, Tolerance);
				switch (num)
				{
				case 1600:
				case 1604:
					pcNumIntersections = 0;
					break;
				case 1607:
				case 1608:
				case 1610:
				case 1611:
				case 1613:
				case 1619:
				case 1620:
					pcNumIntersections = 2;
					break;
				default:
					pcNumIntersections = 1;
					break;
				}
			}
		}
		return num;
	}

	public static int eplGeoE_IntersectTwoLegLists(Eps_Leg pFirstLeg, Eps_Leg pSecondLeg, ref List<Eps_Point> ppPointList, ref int pNumberOfIntersections, double _dTolerance)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		Eps_Point[] paIntersections = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		int pcNumIntersections = 0;
		int num = 0;
		int num2 = 0;
		bool flag = false;
		ppPointList = new List<Eps_Point>();
		pNumberOfIntersections = 0;
		for (eps_Leg = pFirstLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			for (eps_Leg2 = pSecondLeg; eps_Leg2 != null; eps_Leg2 = eps_Leg2.Next)
			{
				paIntersections[0] = new Eps_Point();
				paIntersections[1] = new Eps_Point();
				eplGeoE_IntersectTwoLegs(eps_Leg, eps_Leg2, ref paIntersections, ref pcNumIntersections, _dTolerance);
				for (num = 0; num < pcNumIntersections; num++)
				{
					flag = false;
					for (num2 = 0; num2 < pNumberOfIntersections; num2++)
					{
						if (EPShared.eplGeoE_ArePntsDuplicates3D(ppPointList[num2], paIntersections[num], _dTolerance))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						ppPointList.Add(paIntersections[num]);
						pNumberOfIntersections++;
					}
				}
			}
		}
		return 0;
	}

	public static void eplGeoE_FindMinMaxOfLegList(Eps_Leg pLeg, ref Eps_Point pMinPnt, ref Eps_Point pMaxPnt)
	{
		Eps_Leg eps_Leg = null;
		Eps_Point _MinPnt = new Eps_Point();
		Eps_Point _MaxPnt = new Eps_Point();
		pMinPnt.x = double.MaxValue;
		pMaxPnt.x = double.MinValue;
		pMinPnt.y = double.MaxValue;
		pMaxPnt.y = double.MinValue;
		pMinPnt.z = double.MaxValue;
		pMaxPnt.z = double.MinValue;
		for (eps_Leg = pLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				CalcMinMax(eps_Leg.Line.StartPnt, ref pMinPnt, ref pMaxPnt);
				CalcMinMax(eps_Leg.Line.EndPnt, ref pMinPnt, ref pMaxPnt);
			}
			else if (eps_Leg.Arc != null)
			{
				EPShared.eplGeoE_FindMinMaxOfArc(eps_Leg.Arc, ref _MinPnt, ref _MaxPnt);
				CalcMinMax(_MinPnt, ref pMinPnt, ref pMaxPnt);
				CalcMinMax(_MaxPnt, ref pMinPnt, ref pMaxPnt);
			}
		}
	}

	public static void eplGeoE_FindMinMaxOfLineArcList(Eps_Line[] _LineList, int _nLineCount, Eps_Arc[] _ArcList, int _nArcCount, ref Eps_Point pMinPnt, ref Eps_Point pMaxPnt)
	{
		Eps_Point _MinPnt = new Eps_Point();
		Eps_Point _MaxPnt = new Eps_Point();
		pMinPnt.x = double.MaxValue;
		pMaxPnt.x = double.MinValue;
		pMinPnt.y = double.MaxValue;
		pMaxPnt.y = double.MinValue;
		pMinPnt.z = double.MaxValue;
		pMaxPnt.z = double.MinValue;
		for (int i = 0; i < _nLineCount; i++)
		{
			CalcMinMax(_LineList[i].StartPnt, ref pMinPnt, ref pMaxPnt);
			CalcMinMax(_LineList[i].EndPnt, ref pMinPnt, ref pMaxPnt);
		}
		for (int j = 0; j < _nArcCount; j++)
		{
			EPShared.eplGeoE_FindMinMaxOfArc(_ArcList[j], ref _MinPnt, ref _MaxPnt);
			CalcMinMax(_MinPnt, ref pMinPnt, ref pMaxPnt);
			CalcMinMax(_MaxPnt, ref pMinPnt, ref pMaxPnt);
		}
	}

	private static bool eplGeoE_IsPointOnLegList2D(Eps_Point pPoint, Eps_Leg pLeg)
	{
		Eps_Leg ppCurrLeg = null;
		return eplGeoE_IsPointOnLegList2D(pPoint, pLeg, 1E-05, ref ppCurrLeg);
	}

	private static bool eplGeoE_IsPointOnLegList2D(Eps_Point pPoint, Eps_Leg pLeg, double Tolerance, ref Eps_Leg ppCurrLeg)
	{
		Eps_Leg eps_Leg = null;
		Eps_Leg eps_Leg2 = null;
		bool flag = false;
		Eps_Point pMinPnt = new Eps_Point();
		Eps_Point pMaxPnt = new Eps_Point();
		eplGeoE_FindMinMaxOfLegList(pLeg, ref pMinPnt, ref pMaxPnt);
		if (RevitDefines.LT(pPoint.x, pMinPnt.x, Tolerance) || RevitDefines.GT(pPoint.x, pMaxPnt.x, Tolerance) || RevitDefines.LT(pPoint.y, pMinPnt.y, Tolerance) || RevitDefines.GT(pPoint.y, pMaxPnt.y, Tolerance))
		{
			return false;
		}
		eps_Leg = pLeg;
		while (eps_Leg != null && !flag)
		{
			if (eps_Leg.Arc != null)
			{
				flag = EPShared.eplGeoE_IsPointOnArc2D(pPoint, eps_Leg.Arc, Tolerance);
			}
			else
			{
				if (eps_Leg.Line == null)
				{
					return false;
				}
				flag = EPShared.eplGeoE_IsPointOnLine2D(pPoint, eps_Leg.Line, Tolerance);
			}
			eps_Leg2 = eps_Leg;
			eps_Leg = eps_Leg.Next;
		}
		if (flag && ppCurrLeg != null)
		{
			ppCurrLeg = eps_Leg2;
		}
		return flag;
	}

	public static double eplGeoE_CalcLegListLength(Eps_Leg _LegList)
	{
		double num = 0.0;
		for (Eps_Leg eps_Leg = _LegList; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			num += eplGeoE_CalcLegLength(eps_Leg);
		}
		return num;
	}

	public static double eplGeoE_CalcLegLength(Eps_Leg _Leg)
	{
		double _dDistance = 0.0;
		if (_Leg.Line == null)
		{
			_dDistance = ((_Leg.Arc == null) ? 0.0 : EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(_Leg.Arc.Radius, _Leg.Arc.Delta));
		}
		else
		{
			EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(_Leg.Line.StartPnt, _Leg.Line.EndPnt, ref _dDistance);
		}
		return _dDistance;
	}

	private static int eplGeoE_IntersectTwoDists2D(Eps_Point FirstRadiusPnt, double FirstRadiusLen, Eps_Point SecondRadiusPnt, double SecondRadiusLen, ref Eps_Point[] InterPnt, double Tolerance)
	{
		double _dDirection = 0.0;
		double _dDistance = 0.0;
		double num = Math.Abs(FirstRadiusLen);
		double num2 = Math.Abs(SecondRadiusLen);
		InterPnt[0].z = FirstRadiusPnt.z;
		InterPnt[1].z = FirstRadiusPnt.z;
		if (num < 1E-05 || num2 < 1E-05)
		{
			return 1602;
		}
		double num3 = Math.Abs(num - num2);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(FirstRadiusPnt, SecondRadiusPnt, ref _dDistance);
		if (_dDistance < Tolerance && num3 < 1E-05)
		{
			return 1604;
		}
		if (_dDistance < Tolerance)
		{
			return 1605;
		}
		if (_dDistance + num + Tolerance < num2 || _dDistance + num2 + Tolerance < num)
		{
			return 1601;
		}
		if (num + num2 + 1E-05 < _dDistance)
		{
			return 1602;
		}
		double num4 = (_dDistance * _dDistance + num * num - num2 * num2) / (2.0 * _dDistance * num);
		if (num4 >= 1.0)
		{
			num4 = 0.0;
		}
		else if (num4 <= -1.0)
		{
			num4 = Math.PI;
		}
		else
		{
			num4 = Math.Acos(num4);
			num4 = EPShared.eplGeoE_NormalizeRadians(num4);
		}
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(FirstRadiusPnt, SecondRadiusPnt, ref _dDirection);
		EPShared.eplGeoE_CalcPolarToXY(FirstRadiusPnt, _dDirection + num4, FirstRadiusLen, ref InterPnt[0]);
		if (num4 > 1E-10 && Math.Abs(num4 - Math.PI) > 1E-10)
		{
			EPShared.eplGeoE_CalcPolarToXY(FirstRadiusPnt, _dDirection - num4, FirstRadiusLen, ref InterPnt[1]);
			return 1607;
		}
		return 1606;
	}

	private static int eplGeoE_IntersectDirDist2D(Eps_Point FirstPnt, double DirOfFirstPnt, Eps_Point RadiusPnt, double LenOfRadius, ref Eps_Point[] InterPnt, double Tolerance)
	{
		double _dDistance = 0.0;
		double _dDirection = 0.0;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		Eps_Point eps_Point = new Eps_Point();
		Tolerance = 0.001;
		InterPnt[0].z = FirstPnt.z;
		InterPnt[1].z = FirstPnt.z;
		double num4 = Math.Cos(DirOfFirstPnt);
		double num5 = Math.Sin(DirOfFirstPnt);
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(RadiusPnt, FirstPnt, ref _dDistance);
		if (_dDistance < Tolerance)
		{
			EPShared.eplGeoE_CalcPolarToXY(RadiusPnt, DirOfFirstPnt, LenOfRadius, ref InterPnt[0]);
			EPShared.eplGeoE_CalcPolarToXY(RadiusPnt, DirOfFirstPnt, LenOfRadius, ref InterPnt[1]);
			return 1606;
		}
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(FirstPnt, RadiusPnt, ref _dDirection);
		double num6 = Math.Abs(DirOfFirstPnt - _dDirection);
		if (LenOfRadius - _dDistance > 1E-05)
		{
			num2 = 1;
		}
		if (num2 == 0)
		{
			double value = ((RadiusPnt.y - FirstPnt.y) * Math.Cos(DirOfFirstPnt) - (RadiusPnt.x - FirstPnt.x) * Math.Sin(DirOfFirstPnt)) / LenOfRadius;
			if (Math.Abs(value) - 1.0 > 1E-05)
			{
				return 1600;
			}
		}
		double num7 = Math.Sin(num6);
		double num8 = _dDistance * num7 / LenOfRadius;
		if (num8 < -1.0 || num8 > 1.0)
		{
			if (num8 < -1.0000000001 || num8 > 1.0000000001)
			{
				return 1600;
			}
			num8 = eplCalE_RoundValue(num8, 0);
		}
		double num9 = Math.Asin(num8);
		double d = Math.PI - (num6 + num9);
		double num10 = Math.PI - num6;
		double num11 = Math.PI - num9;
		double d2 = Math.PI - (num6 + num11);
		double value2 = _dDistance * _dDistance + LenOfRadius * LenOfRadius - 2.0 * _dDistance * LenOfRadius * Math.Cos(d);
		value2 = Math.Sqrt(Math.Abs(value2));
		double value3 = _dDistance * _dDistance + LenOfRadius * LenOfRadius - 2.0 * _dDistance * LenOfRadius * Math.Cos(d2);
		value3 = Math.Sqrt(Math.Abs(value3));
		if (num2 == 0)
		{
			num2 = ((Math.Abs(num9 - Math.PI / 2.0) <= 1E-10) ? 1 : 2);
		}
		eps_Point.x = FirstPnt.x + value2 * num4;
		eps_Point.y = FirstPnt.y + value2 * num5;
		double num12 = (eps_Point.x - RadiusPnt.x) * (eps_Point.x - RadiusPnt.x) + (eps_Point.y - RadiusPnt.y) * (eps_Point.y - RadiusPnt.y) - LenOfRadius * LenOfRadius;
		if (num12 <= Tolerance)
		{
			num3 = 1;
			InterPnt[num].x = eps_Point.x;
			InterPnt[num].y = eps_Point.y;
			num = 1;
		}
		if (num2 == 1)
		{
			if (num > 0)
			{
				return 1606;
			}
			return 1600;
		}
		eps_Point.x = FirstPnt.x + value3 * num4;
		eps_Point.y = FirstPnt.y + value3 * num5;
		num12 = (eps_Point.x - RadiusPnt.x) * (eps_Point.x - RadiusPnt.x) + (eps_Point.y - RadiusPnt.y) * (eps_Point.y - RadiusPnt.y) - LenOfRadius * LenOfRadius;
		if (num12 <= Tolerance)
		{
			num3++;
			InterPnt[num].x = eps_Point.x;
			InterPnt[num].y = eps_Point.y;
		}
		return num3 switch
		{
			0 => 1600, 
			1 => 1606, 
			2 => 1607, 
			_ => 1600, 
		};
	}

	private static int eplGeoE_IntersectTwoLines(Eps_Line pLineOne, Eps_Line pLineTwo, ref Eps_Point[] aInterPnt, ref int pNumOfInter, double PointTolerance)
	{
		int result = 1600;
		pNumOfInter = 0;
		if (!EPShared.eplGeoE_DoBoundingBoxesOverlap(pLineOne, pLineTwo, PointTolerance))
		{
			return result;
		}
		double num = EPShared.eplGeoE_CalcPlaneDistBtwnTwoPnts(pLineOne.StartPnt.x, pLineOne.StartPnt.y, pLineOne.EndPnt.x, pLineOne.EndPnt.y);
		double num2 = EPShared.eplGeoE_CalcPlaneDistBtwnTwoPnts(pLineTwo.StartPnt.x, pLineTwo.StartPnt.y, pLineTwo.EndPnt.x, pLineTwo.EndPnt.y);
		if (num <= PointTolerance)
		{
			if (num2 <= PointTolerance)
			{
				result = 1604;
			}
			else if (EPShared.eplGeoE_IsPointOnLine2D(pLineOne.StartPnt, pLineTwo, PointTolerance))
			{
				aInterPnt[0] = new Eps_Point(pLineOne.StartPnt);
				aInterPnt[1] = new Eps_Point(pLineOne.StartPnt);
				result = 1610;
			}
			return result;
		}
		if (num2 <= PointTolerance)
		{
			if (EPShared.eplGeoE_IsPointOnLine2D(pLineTwo.StartPnt, pLineOne, PointTolerance))
			{
				aInterPnt[0] = new Eps_Point(pLineTwo.StartPnt);
				aInterPnt[1] = new Eps_Point(pLineTwo.StartPnt);
				result = 1611;
			}
			return result;
		}
		result = eplGeoE_InterBoundedLineLine(pLineOne, pLineTwo, ref aInterPnt, PointTolerance);
		switch (result)
		{
		case 1111:
			result = eplGeoE_InterColinearBoundedLineLine(pLineOne, pLineTwo, ref aInterPnt, pNumOfInter, PointTolerance);
			break;
		case 1606:
		{
			pNumOfInter = 1;
			int num3 = CheckLinesForCommonEndPnts(pLineOne, pLineTwo, PointTolerance);
			if ((uint)(num3 - 1614) <= 3u)
			{
				result = 1612;
			}
			break;
		}
		}
		return result;
	}

	private static int eplGeoE_IntersectTwoArcs2D(Eps_Arc ArcOne, Eps_Arc ArcTwo, ref Eps_Point[] InterPnt, double PointTolerance)
	{
		Eps_Point[] InterPnt2 = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		int result = 1600;
		int num = 1600;
		Eps_Line[] array = new Eps_Line[2]
		{
			new Eps_Line(),
			new Eps_Line()
		};
		Eps_Point startPnt = array[0].StartPnt;
		double x = (array[0].EndPnt.x = ArcOne.RadiusPnt.x);
		startPnt.x = x;
		Eps_Point startPnt2 = array[0].StartPnt;
		x = (array[0].EndPnt.y = ArcOne.RadiusPnt.y);
		startPnt2.y = x;
		array[0].StartPnt.x -= ArcOne.Radius;
		array[0].EndPnt.x += ArcOne.Radius;
		array[0].StartPnt.y -= ArcOne.Radius;
		array[0].EndPnt.y += ArcOne.Radius;
		Eps_Point startPnt3 = array[1].StartPnt;
		x = (array[1].EndPnt.x = ArcTwo.RadiusPnt.x);
		startPnt3.x = x;
		Eps_Point startPnt4 = array[1].StartPnt;
		x = (array[1].EndPnt.y = ArcTwo.RadiusPnt.y);
		startPnt4.y = x;
		array[1].StartPnt.x -= ArcTwo.Radius;
		array[1].EndPnt.x += ArcTwo.Radius;
		array[1].StartPnt.y -= ArcTwo.Radius;
		array[1].EndPnt.y += ArcTwo.Radius;
		if (!EPShared.eplGeoE_DoBoundingBoxesOverlap(array[0], array[1], PointTolerance))
		{
			return result;
		}
		InterPnt[0].z = ArcOne.StartPnt.z;
		InterPnt[1].z = ArcOne.StartPnt.z;
		num = eplGeoE_FindCommonEndPntsOnArcs2D(ArcOne, ArcTwo, ref InterPnt, PointTolerance);
		switch (num)
		{
		case 1604:
		case 1613:
			return num;
		case 1209:
			num = 1600;
			break;
		}
		int num2 = ChkIfArcsOverlapOrWithIn2D(ArcOne, ArcTwo, ref InterPnt, PointTolerance);
		if (num2 != 1609)
		{
			return num2;
		}
		bool flag = EPShared.eplGeoE_IsPointOnArc2D(ArcOne.StartPnt, ArcTwo, PointTolerance);
		bool flag2 = EPShared.eplGeoE_IsPointOnArc2D(ArcOne.EndPnt, ArcTwo, PointTolerance);
		bool flag3 = EPShared.eplGeoE_IsPointOnArc2D(ArcTwo.StartPnt, ArcOne, PointTolerance);
		bool flag4 = EPShared.eplGeoE_IsPointOnArc2D(ArcTwo.EndPnt, ArcOne, PointTolerance);
		if (num == 1612)
		{
			bool flag5 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.StartPnt, ArcTwo.StartPnt, PointTolerance);
			bool flag6 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.EndPnt, ArcTwo.EndPnt, PointTolerance);
			bool flag7 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.StartPnt, ArcTwo.EndPnt, PointTolerance);
			bool flag8 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.EndPnt, ArcTwo.StartPnt, PointTolerance);
			if (flag5)
			{
				InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
				if (flag2)
				{
					InterPnt[1] = new Eps_Point(ArcOne.EndPnt);
					return 1607;
				}
				if (flag4)
				{
					InterPnt[1] = new Eps_Point(ArcTwo.EndPnt);
					return 1607;
				}
			}
			if (flag6)
			{
				InterPnt[0] = new Eps_Point(ArcOne.EndPnt);
				if (flag)
				{
					InterPnt[1] = new Eps_Point(ArcOne.StartPnt);
					return 1607;
				}
				if (flag3)
				{
					InterPnt[1] = new Eps_Point(ArcTwo.StartPnt);
					return 1607;
				}
			}
			if (flag7)
			{
				InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
				if (flag2)
				{
					InterPnt[1] = new Eps_Point(ArcOne.EndPnt);
					return 1607;
				}
				if (flag3)
				{
					InterPnt[1] = new Eps_Point(ArcTwo.StartPnt);
					return 1607;
				}
			}
			if (flag8)
			{
				InterPnt[0] = new Eps_Point(ArcOne.EndPnt);
				if (flag)
				{
					InterPnt[1] = new Eps_Point(ArcOne.StartPnt);
					return 1607;
				}
				if (flag4)
				{
					InterPnt[1] = new Eps_Point(ArcTwo.EndPnt);
					return 1607;
				}
			}
		}
		if (num != 1612)
		{
			if (flag & flag2)
			{
				InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
				InterPnt[1] = new Eps_Point(ArcOne.EndPnt);
				return 1607;
			}
			if (flag3 & flag4)
			{
				InterPnt[0] = new Eps_Point(ArcTwo.StartPnt);
				InterPnt[1] = new Eps_Point(ArcTwo.EndPnt);
				return 1607;
			}
			if (flag & flag3)
			{
				InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
				InterPnt[1] = new Eps_Point(ArcTwo.StartPnt);
				return 1607;
			}
			if (flag & flag4)
			{
				InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
				InterPnt[1] = new Eps_Point(ArcTwo.EndPnt);
				return 1607;
			}
			if (flag2 & flag3)
			{
				InterPnt[0] = new Eps_Point(ArcOne.EndPnt);
				InterPnt[1] = new Eps_Point(ArcTwo.StartPnt);
				return 1607;
			}
			if (flag2 & flag4)
			{
				InterPnt[0] = new Eps_Point(ArcOne.EndPnt);
				InterPnt[1] = new Eps_Point(ArcTwo.EndPnt);
				return 1607;
			}
			if (flag)
			{
				InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
				num = 1606;
			}
			else if (flag2)
			{
				InterPnt[0] = new Eps_Point(ArcOne.EndPnt);
				num = 1606;
			}
			else if (flag3)
			{
				InterPnt[0] = new Eps_Point(ArcTwo.StartPnt);
				num = 1606;
			}
			else if (flag4)
			{
				InterPnt[0] = new Eps_Point(ArcTwo.EndPnt);
				num = 1606;
			}
		}
		result = num;
		switch (eplGeoE_IntersectTwoDists2D(ArcOne.RadiusPnt, ArcOne.Radius, ArcTwo.RadiusPnt, ArcTwo.Radius, ref InterPnt2, 1E-05))
		{
		case 1606:
			switch (num)
			{
			case 1606:
				return 1606;
			case 1612:
				return 1612;
			case 1600:
				if (EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[0], ArcOne, PointTolerance) && EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[0], ArcTwo, PointTolerance))
				{
					InterPnt[0] = new Eps_Point(InterPnt2[0]);
					return 1606;
				}
				break;
			}
			break;
		case 1607:
			if (num == 1600)
			{
				if (EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[0], ArcOne, PointTolerance) && EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[0], ArcTwo, PointTolerance))
				{
					InterPnt[0] = new Eps_Point(InterPnt2[0]);
					result = ((num != 1612) ? 1606 : 1612);
				}
				if (EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[1], ArcOne, PointTolerance) && EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[1], ArcTwo, PointTolerance))
				{
					if (result != 1606)
					{
						InterPnt[0] = new Eps_Point(InterPnt2[1]);
						result = ((num != 1612) ? 1606 : 1612);
					}
					else
					{
						InterPnt[1] = new Eps_Point(InterPnt2[1]);
						result = 1607;
					}
				}
				return result;
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(InterPnt[0], InterPnt2[0], PointTolerance) && EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[0], ArcOne, PointTolerance) && EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[0], ArcTwo, PointTolerance))
			{
				InterPnt[1] = new Eps_Point(InterPnt2[0]);
				return 1607;
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(InterPnt[0], InterPnt2[1], PointTolerance))
			{
				if (EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[1], ArcOne, PointTolerance) && EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[1], ArcTwo, PointTolerance))
				{
					InterPnt[1] = new Eps_Point(InterPnt2[1]);
					return 1607;
				}
				break;
			}
			return result;
		default:
			result = 1600;
			break;
		}
		return result;
	}

	private static int eplGeoE_InterBoundedLineLine(Eps_Line pLineOne, Eps_Line pLineTwo, ref Eps_Point[] aInterPnt, double PointTolerance)
	{
		int result = 1600;
		double num = (pLineTwo.StartPnt.x - pLineOne.StartPnt.x) * (pLineOne.EndPnt.y - pLineOne.StartPnt.y) - (pLineOne.EndPnt.x - pLineOne.StartPnt.x) * (pLineTwo.StartPnt.y - pLineOne.StartPnt.y);
		double num2 = (pLineTwo.EndPnt.x - pLineOne.StartPnt.x) * (pLineOne.EndPnt.y - pLineOne.StartPnt.y) - (pLineOne.EndPnt.x - pLineOne.StartPnt.x) * (pLineTwo.EndPnt.y - pLineOne.StartPnt.y);
		double num3 = (pLineOne.StartPnt.x - pLineTwo.StartPnt.x) * (pLineTwo.EndPnt.y - pLineTwo.StartPnt.y) - (pLineTwo.EndPnt.x - pLineTwo.StartPnt.x) * (pLineOne.StartPnt.y - pLineTwo.StartPnt.y);
		double num4 = (pLineOne.EndPnt.x - pLineTwo.StartPnt.x) * (pLineTwo.EndPnt.y - pLineTwo.StartPnt.y) - (pLineTwo.EndPnt.x - pLineTwo.StartPnt.x) * (pLineOne.EndPnt.y - pLineTwo.StartPnt.y);
		double num5 = EPShared.eplGeoE_CalcPlaneDistBtwnTwoPnts(pLineOne.StartPnt.x, pLineOne.StartPnt.y, pLineOne.EndPnt.x, pLineOne.EndPnt.y);
		double num6 = EPShared.eplGeoE_CalcPlaneDistBtwnTwoPnts(pLineTwo.StartPnt.x, pLineTwo.StartPnt.y, pLineTwo.EndPnt.x, pLineTwo.EndPnt.y);
		double num7 = Math.Abs(num / num5);
		double num8 = Math.Abs(num2 / num5);
		double num9 = Math.Abs(num3 / num6);
		double num10 = Math.Abs(num4 / num6);
		if (num7 <= PointTolerance)
		{
			if (num8 <= PointTolerance)
			{
				result = 1111;
			}
			else if (IsPntInBoundingBoxOfLine(pLineTwo.StartPnt, pLineOne, PointTolerance))
			{
				aInterPnt[0] = new Eps_Point(pLineTwo.StartPnt);
				result = 1606;
			}
			return result;
		}
		if (num8 <= PointTolerance)
		{
			if (IsPntInBoundingBoxOfLine(pLineTwo.EndPnt, pLineOne, PointTolerance))
			{
				aInterPnt[0] = new Eps_Point(pLineTwo.EndPnt);
				result = 1606;
			}
			return result;
		}
		if (num9 <= PointTolerance)
		{
			if (IsPntInBoundingBoxOfLine(pLineOne.StartPnt, pLineTwo, PointTolerance))
			{
				aInterPnt[0] = new Eps_Point(pLineOne.StartPnt);
				result = 1606;
			}
			return result;
		}
		if (num10 <= PointTolerance)
		{
			if (IsPntInBoundingBoxOfLine(pLineOne.EndPnt, pLineTwo, PointTolerance))
			{
				aInterPnt[0] = new Eps_Point(pLineOne.EndPnt);
				result = 1606;
			}
			return result;
		}
		if (((num < 0.0 && num2 > 0.0) || (num > 0.0 && num2 < 0.0)) && ((num3 < 0.0 && num4 > 0.0) || (num3 > 0.0 && num4 < 0.0)))
		{
			CalculateInterPnt(pLineOne, pLineTwo, ref aInterPnt);
			result = 1606;
		}
		return result;
	}

	private static int eplGeoE_InterColinearBoundedLineLine(Eps_Line pLineOne, Eps_Line pLineTwo, ref Eps_Point[] aInterPnt, int pNumOfInter, double PointTolerance)
	{
		int num = 1600;
		pNumOfInter = 0;
		switch (CheckLinesForCommonEndPnts(pLineOne, pLineTwo, PointTolerance))
		{
		case 1108:
		case 1109:
			return 1604;
		case 1615:
		case 1617:
			aInterPnt[pNumOfInter++] = new Eps_Point(pLineOne.EndPnt);
			num = 1612;
			break;
		case 1614:
		case 1616:
			aInterPnt[pNumOfInter++] = new Eps_Point(pLineOne.StartPnt);
			num = 1612;
			break;
		}
		int num2 = EPShared.eplGeoE_CheckIfLineInsideLine(pLineOne, pLineTwo, 1E-05);
		if (num2 == 1700)
		{
			aInterPnt[0] = new Eps_Point(pLineOne.StartPnt);
			aInterPnt[1] = new Eps_Point(pLineOne.EndPnt);
			pNumOfInter = 2;
			num = 1610;
		}
		else if (EPShared.eplGeoE_CheckIfLineInsideLine(pLineTwo, pLineOne, 1E-05) == 1700)
		{
			aInterPnt[0] = new Eps_Point(pLineTwo.StartPnt);
			aInterPnt[1] = new Eps_Point(pLineTwo.EndPnt);
			pNumOfInter = 2;
			num = 1611;
		}
		if (num == 1600)
		{
			if (EPShared.eplGeoE_IsPointOnLine2D(pLineOne.StartPnt, pLineTwo, PointTolerance))
			{
				aInterPnt[pNumOfInter++] = new Eps_Point(pLineOne.StartPnt);
				if (EPShared.eplGeoE_IsPointOnLine2D(pLineTwo.StartPnt, pLineOne, PointTolerance))
				{
					aInterPnt[pNumOfInter++] = new Eps_Point(pLineTwo.StartPnt);
				}
				else
				{
					aInterPnt[pNumOfInter++] = new Eps_Point(pLineTwo.EndPnt);
				}
				num = 1608;
			}
			else if (EPShared.eplGeoE_IsPointOnLine2D(pLineOne.EndPnt, pLineTwo, PointTolerance))
			{
				aInterPnt[pNumOfInter++] = new Eps_Point(pLineOne.EndPnt);
				if (EPShared.eplGeoE_IsPointOnLine2D(pLineTwo.StartPnt, pLineOne, PointTolerance))
				{
					aInterPnt[pNumOfInter++] = new Eps_Point(pLineTwo.StartPnt);
				}
				else
				{
					aInterPnt[pNumOfInter++] = new Eps_Point(pLineTwo.EndPnt);
				}
				num = 1608;
			}
		}
		return num;
	}

	private static int eplGeoE_FindCommonEndPntsOnArcs2D(Eps_Arc ArcOne, Eps_Arc ArcTwo, ref Eps_Point[] InterPnt, double PointTolerance)
	{
		InterPnt[0].z = ArcOne.StartPnt.z;
		InterPnt[1].z = ArcOne.StartPnt.z;
		bool flag = ((ArcOne.Direction == ArcTwo.Direction) ? true : false);
		bool flag2 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.StartPnt, ArcTwo.StartPnt, PointTolerance);
		bool flag3 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.EndPnt, ArcTwo.EndPnt, PointTolerance);
		bool flag4 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.StartPnt, ArcTwo.EndPnt, PointTolerance);
		bool flag5 = EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.EndPnt, ArcTwo.StartPnt, PointTolerance);
		if (flag2 && flag3)
		{
			if (EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.RadiusPnt, ArcTwo.RadiusPnt, PointTolerance))
			{
				if (flag)
				{
					return 1604;
				}
				InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
				InterPnt[1] = new Eps_Point(ArcOne.EndPnt);
				return 1613;
			}
			InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
			InterPnt[1] = new Eps_Point(ArcOne.EndPnt);
			return 1613;
		}
		if (flag4 && flag5)
		{
			if (EPShared.eplGeoE_ArePntsDuplicates2D(ArcOne.RadiusPnt, ArcTwo.RadiusPnt, PointTolerance))
			{
				if (!flag)
				{
					return 1604;
				}
				InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
				InterPnt[1] = new Eps_Point(ArcOne.EndPnt);
				return 1613;
			}
			InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
			InterPnt[1] = new Eps_Point(ArcOne.EndPnt);
			return 1613;
		}
		if (flag2 || flag4)
		{
			InterPnt[0] = new Eps_Point(ArcOne.StartPnt);
			return 1612;
		}
		if (flag5 || flag3)
		{
			InterPnt[0] = new Eps_Point(ArcOne.EndPnt);
			return 1612;
		}
		return 1209;
	}

	private static int eplGeoE_IntersectLineArc(Eps_Line Line, Eps_Arc Arc, ref Eps_Point[] InterPnt, double PointTolerance)
	{
		Eps_Point[] InterPnt2 = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		double _dDirection = 0.0;
		int result = 1600;
		int num = 1600;
		Eps_Line eps_Line = new Eps_Line();
		Eps_Point startPnt = eps_Line.StartPnt;
		double x = (eps_Line.EndPnt.x = Arc.RadiusPnt.x);
		startPnt.x = x;
		Eps_Point startPnt2 = eps_Line.StartPnt;
		x = (eps_Line.EndPnt.y = Arc.RadiusPnt.y);
		startPnt2.y = x;
		eps_Line.StartPnt.x -= Arc.Radius;
		eps_Line.EndPnt.x += Arc.Radius;
		eps_Line.StartPnt.y -= Arc.Radius;
		eps_Line.EndPnt.y += Arc.Radius;
		if (!EPShared.eplGeoE_DoBoundingBoxesOverlap(Line, eps_Line, PointTolerance))
		{
			return result;
		}
		InterPnt[0].z = Line.StartPnt.z;
		InterPnt[1].z = Line.StartPnt.z;
		bool flag = EPShared.eplGeoE_ArePntsDuplicates2D(Line.StartPnt, Arc.StartPnt, PointTolerance);
		bool flag2 = EPShared.eplGeoE_ArePntsDuplicates2D(Line.EndPnt, Arc.EndPnt, PointTolerance);
		bool flag3 = EPShared.eplGeoE_ArePntsDuplicates2D(Line.StartPnt, Arc.EndPnt, PointTolerance);
		bool flag4 = EPShared.eplGeoE_ArePntsDuplicates2D(Line.EndPnt, Arc.StartPnt, PointTolerance);
		if ((flag && flag2) || (flag3 && flag4))
		{
			InterPnt[0] = new Eps_Point(Line.StartPnt);
			InterPnt[1] = new Eps_Point(Line.EndPnt);
			return 1613;
		}
		bool flag5 = EPShared.eplGeoE_IsPointOnArc2D(Line.StartPnt, Arc, PointTolerance);
		bool flag6 = EPShared.eplGeoE_IsPointOnArc2D(Line.EndPnt, Arc, PointTolerance);
		bool flag7 = EPShared.eplGeoE_IsPointOnLine2D(Arc.StartPnt, Line, PointTolerance);
		bool flag8 = !EPShared.eplGeoE_ArePntsDuplicates2D(Arc.StartPnt, Arc.EndPnt, PointTolerance) && EPShared.eplGeoE_IsPointOnLine2D(Arc.EndPnt, Line, PointTolerance);
		if (flag || flag3)
		{
			InterPnt[0] = new Eps_Point(Line.StartPnt);
			num = 1612;
			if (flag6)
			{
				InterPnt[1] = new Eps_Point(Line.EndPnt);
				return 1607;
			}
			if (flag)
			{
				if (flag8)
				{
					InterPnt[1] = new Eps_Point(Arc.EndPnt);
					return 1607;
				}
			}
			else if (flag7)
			{
				InterPnt[1] = new Eps_Point(Arc.StartPnt);
				return 1607;
			}
		}
		if (num == 1600 && (flag2 || flag4))
		{
			InterPnt[0] = new Eps_Point(Line.EndPnt);
			num = 1612;
			if (flag5)
			{
				InterPnt[1] = new Eps_Point(Line.StartPnt);
				return 1607;
			}
			if (flag2)
			{
				if (flag7)
				{
					InterPnt[1] = new Eps_Point(Arc.StartPnt);
					return 1607;
				}
			}
			else if (flag8)
			{
				InterPnt[1] = new Eps_Point(Arc.EndPnt);
				return 1607;
			}
		}
		if (num == 1600)
		{
			if (flag6)
			{
				InterPnt[0] = new Eps_Point(Line.EndPnt);
				num = 1606;
				if (flag7)
				{
					InterPnt[1] = new Eps_Point(Arc.StartPnt);
					return 1607;
				}
				if (flag8)
				{
					InterPnt[1] = new Eps_Point(Arc.EndPnt);
					return 1607;
				}
			}
			if (flag5)
			{
				if (num == 1606)
				{
					InterPnt[1] = new Eps_Point(Line.StartPnt);
					return 1607;
				}
				InterPnt[0] = new Eps_Point(Line.StartPnt);
				num = 1606;
				if (flag7)
				{
					InterPnt[1] = new Eps_Point(Arc.StartPnt);
					return 1607;
				}
				if (flag8)
				{
					InterPnt[1] = new Eps_Point(Arc.EndPnt);
					return 1607;
				}
			}
		}
		if (num == 1600)
		{
			if (flag7 && flag8)
			{
				InterPnt[0] = new Eps_Point(Arc.StartPnt);
				InterPnt[1] = new Eps_Point(Arc.EndPnt);
				return 1607;
			}
			if (flag7)
			{
				InterPnt[0] = new Eps_Point(Arc.StartPnt);
				num = 1606;
			}
			else if (flag8)
			{
				InterPnt[0] = new Eps_Point(Arc.EndPnt);
				num = 1606;
			}
		}
		result = num;
		EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(Line.StartPnt, Line.EndPnt, ref _dDirection);
		double radius = Arc.Radius;
		switch (eplGeoE_IntersectDirDist2D(Line.StartPnt, _dDirection, Arc.RadiusPnt, radius, ref InterPnt2, 0.01))
		{
		case 1606:
			switch (num)
			{
			case 1606:
				return 1606;
			case 1612:
				return 1612;
			case 1600:
				if (EPShared.eplGeoE_IsPointOnLine2D(InterPnt2[0], Line, PointTolerance) && EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[0], Arc, PointTolerance))
				{
					InterPnt[0] = new Eps_Point(InterPnt2[0]);
					return 1606;
				}
				break;
			}
			break;
		case 1607:
			if (num == 1600)
			{
				if (EPShared.eplGeoE_IsPointOnLine2D(InterPnt2[0], Line, PointTolerance) && EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[0], Arc, PointTolerance))
				{
					InterPnt[0] = new Eps_Point(InterPnt2[0]);
					result = 1606;
				}
				if (EPShared.eplGeoE_IsPointOnLine2D(InterPnt2[1], Line, PointTolerance) && EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[1], Arc, PointTolerance))
				{
					if (result != 1606)
					{
						InterPnt[0] = new Eps_Point(InterPnt2[1]);
						result = ((num != 1612) ? 1606 : 1612);
					}
					else
					{
						InterPnt[1] = new Eps_Point(InterPnt2[1]);
						result = 1607;
					}
				}
				return result;
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(InterPnt[0], InterPnt2[0], PointTolerance) && EPShared.eplGeoE_IsPointOnLine2D(InterPnt2[0], Line, PointTolerance) && EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[0], Arc, PointTolerance))
			{
				InterPnt[1] = new Eps_Point(InterPnt2[0]);
				return 1607;
			}
			if (!EPShared.eplGeoE_ArePntsDuplicates2D(InterPnt[0], InterPnt2[1], PointTolerance))
			{
				if (EPShared.eplGeoE_IsPointOnLine2D(InterPnt2[1], Line, PointTolerance) && EPShared.eplGeoE_IsPointOnArc2D(InterPnt2[1], Arc, PointTolerance))
				{
					InterPnt[1] = new Eps_Point(InterPnt2[1]);
					return 1607;
				}
				break;
			}
			return result;
		}
		return result;
	}

	private static int eplGeoE_IntersectLineCircle(Eps_Line pLine, Eps_Point RadiusPnt, double Radius, ref Eps_Point[] paIntPnts, ref int pNumInts, double Tolerance)
	{
		int num = 1600;
		Eps_Arc eps_Arc = new Eps_Arc();
		Eps_Point _NewPnt = new Eps_Point();
		EPShared.eplGeoE_CalcPolarToXY(RadiusPnt, 0.0, Radius, ref _NewPnt);
		eps_Arc.RadiusPnt = new Eps_Point(RadiusPnt);
		eps_Arc.Radius = Radius;
		eps_Arc.Direction = -1;
		eps_Arc.StartPnt = new Eps_Point(_NewPnt);
		eps_Arc.EndPnt = new Eps_Point(_NewPnt);
		eps_Arc.Delta = Math.PI * 2.0;
		num = eplGeoE_IntersectLineArc(pLine, eps_Arc, ref paIntPnts, Tolerance);
		switch (num)
		{
		case 1606:
		case 1612:
		case 1613:
			pNumInts = 1;
			num = 1606;
			break;
		case 1607:
			pNumInts = 2;
			break;
		}
		return num;
	}

	public static int eplGeoE_IsPointInsideLegList(Eps_Leg pLeg, Eps_Point Point)
	{
		Eps_Point pMinPnt = null;
		Eps_Point pMaxPnt = null;
		return eplGeoE_IsPointInsideLegList(pLeg, Point, 1E-05, pMinPnt, pMaxPnt);
	}

	public static int eplGeoE_IsPointInsideLegList(Eps_Leg pLeg, Eps_Point Point, double Tolerance, Eps_Point pMinPnt, Eps_Point pMaxPnt)
	{
		int num = 0;
		Eps_Leg eps_Leg = null;
		Eps_Point[] InterPnt = new Eps_Point[2]
		{
			new Eps_Point(),
			new Eps_Point()
		};
		Eps_Line eps_Line = new Eps_Line();
		Eps_Point pMinPnt2 = new Eps_Point();
		Eps_Point pMaxPnt2 = new Eps_Point();
		Eps_Point _NewPnt = new Eps_Point();
		double _dDistance = 0.0;
		double _dDistance2 = 0.0;
		int pNumOfInter = 0;
		int num2 = 0;
		List<double> list = new List<double>();
		Eps_Point eps_Point = new Eps_Point();
		Eps_Point eps_Point2 = new Eps_Point();
		double _dDirection = 0.0;
		int num3 = 0;
		int num4 = 0;
		double num5 = 0.0;
		bool flag = false;
		if (pMinPnt == null || pMaxPnt == null)
		{
			eplGeoE_FindMinMaxOfLegList(pLeg, ref pMinPnt2, ref pMaxPnt2);
		}
		else
		{
			pMinPnt2 = new Eps_Point(pMinPnt);
			pMaxPnt2 = new Eps_Point(pMaxPnt);
		}
		if (RevitDefines.LT(Point.x, pMinPnt2.x, Tolerance) || RevitDefines.GT(Point.x, pMaxPnt2.x, Tolerance) || RevitDefines.LT(Point.y, pMinPnt2.y, Tolerance) || RevitDefines.GT(Point.y, pMaxPnt2.y, Tolerance))
		{
			return 6;
		}
		EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(pMinPnt2, pMaxPnt2, ref _dDistance);
		for (eps_Leg = pLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				eps_Point = new Eps_Point(eps_Leg.Line.StartPnt);
				eps_Point2 = new Eps_Point(eps_Leg.Line.EndPnt);
			}
			else if (eps_Leg.Arc != null)
			{
				eps_Point = new Eps_Point(eps_Leg.Arc.StartPnt);
				eps_Point2 = new Eps_Point(eps_Leg.Arc.EndPnt);
			}
			if (RevitDefines.EQ(eps_Point.y, Point.y) || RevitDefines.EQ(eps_Point2.y, Point.y))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			for (eps_Leg = pLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
			{
				if (eps_Leg.Line != null)
				{
					eps_Point = new Eps_Point(eps_Leg.Line.StartPnt);
					eps_Point2 = new Eps_Point(eps_Leg.Line.EndPnt);
				}
				else if (eps_Leg.Arc != null)
				{
					eps_Point = new Eps_Point(eps_Leg.Arc.StartPnt);
					eps_Point2 = new Eps_Point(eps_Leg.Arc.EndPnt);
				}
				if (eps_Leg == pLeg)
				{
					EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(Point, eps_Point, ref _dDirection);
					if (!list.Contains(_dDirection))
					{
						list.Add(_dDirection);
					}
				}
				EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(Point, eps_Point2, ref _dDirection);
				if (!list.Contains(_dDirection))
				{
					list.Add(_dDirection);
				}
			}
			DoubleComparer comparer = new DoubleComparer();
			list.Sort(comparer);
			for (num3 = 0; num3 < list.Count - 1; num3++)
			{
				_dDirection = list[num3 + 1] - list[num3];
				if (num3 == 0 || _dDirection > num5)
				{
					num5 = _dDirection;
					num4 = num3;
				}
			}
			_dDirection = (list[num4 + 1] - list[num4]) / 2.0;
			list.Clear();
		}
		else
		{
			_dDirection = 0.0;
		}
		eps_Line.StartPnt = new Eps_Point(Point);
		EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, _dDirection, _dDistance * 2.0, ref _NewPnt);
		eps_Line.EndPnt = new Eps_Point(_NewPnt);
		for (eps_Leg = pLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				if (EPShared.eplGeoE_IsPointOnLine2D(Point, eps_Leg.Line, Tolerance))
				{
					return 1903;
				}
				switch (eplGeoE_IntersectTwoLines(eps_Line, eps_Leg.Line, ref InterPnt, ref pNumOfInter, 1E-05))
				{
				case 1606:
				case 1612:
					if (!EPShared.eplGeoE_ArePntsDuplicates2D(InterPnt[0], eps_Leg.Line.StartPnt) && !EPShared.eplGeoE_ArePntsDuplicates2D(InterPnt[0], eps_Leg.Line.EndPnt))
					{
						num2++;
					}
					break;
				case 1608:
				case 1610:
				case 1611:
					num2 += 2;
					break;
				}
			}
			else if (eps_Leg.Arc != null)
			{
				if (EPShared.eplGeoE_IsPointOnArc2D(Point, eps_Leg.Arc, Tolerance))
				{
					return 1903;
				}
				switch (eplGeoE_IntersectLineArc(eps_Line, eps_Leg.Arc, ref InterPnt, 1E-05))
				{
				case 1606:
				{
					EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(Point, eps_Leg.Arc.RadiusPnt, ref _dDistance2);
					if (RevitDefines.LT(_dDistance2, eps_Leg.Arc.Radius))
					{
						num2++;
						break;
					}
					Eps_Point[] paIntPnts = new Eps_Point[2]
					{
						new Eps_Point(),
						new Eps_Point()
					};
					int pNumInts = 0;
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, _dDirection, _dDistance2 * 2.0, ref _NewPnt);
					eps_Line.EndPnt = new Eps_Point(_NewPnt);
					if (eplGeoE_IntersectLineCircle(eps_Line, eps_Leg.Arc.RadiusPnt, eps_Leg.Arc.Radius, ref paIntPnts, ref pNumInts, 1E-05) == 1607 && !EPShared.eplGeoE_ArePntsDuplicates2D(InterPnt[0], eps_Leg.Arc.StartPnt) && !EPShared.eplGeoE_ArePntsDuplicates2D(InterPnt[0], eps_Leg.Arc.EndPnt))
					{
						num2++;
					}
					EPShared.eplGeoE_CalcPolarToXY(eps_Line.StartPnt, _dDirection, _dDistance * 2.0, ref _NewPnt);
					eps_Line.EndPnt = new Eps_Point(_NewPnt);
					break;
				}
				case 1607:
					num2 += 2;
					break;
				}
			}
		}
		if (num2 % 2 == 1)
		{
			return 0;
		}
		return 6;
	}

	public static int eplGeoE_CalcLegDirection(Eps_Leg TestLeg, ref int Direction, double Tolerance)
	{
		Eps_Leg pNewLeg = null;
		Eps_Leg eps_Leg = null;
		int num = 0;
		double _dAngle = 0.0;
		double num2 = 0.0;
		Eps_Point[] array = new Eps_Point[3];
		if (!EPShared.eplGeoE_ArePntsDuplicates2D(TestLeg.LegStartPnt, TestLeg.LegEndPnt, Tolerance))
		{
			return 1;
		}
		eplGeoE_ConvLegListIntoLineLegList(TestLeg, ref pNewLeg);
		eps_Leg = pNewLeg;
		num = 0;
		while (eps_Leg.Next != null)
		{
			if (eps_Leg.Line != null)
			{
				array[0] = new Eps_Point(eps_Leg.Line.StartPnt);
				array[1] = new Eps_Point(eps_Leg.Line.EndPnt);
				if (eps_Leg.Next != null)
				{
					array[2] = new Eps_Point(eps_Leg.Next.Line.EndPnt);
					EPShared.eplGeoE_CalcHorzAngByThreePnts(array[0], array[1], array[2], 13, ref _dAngle, 1E-05);
					num2 += _dAngle;
					eps_Leg = eps_Leg.Next;
					num++;
					continue;
				}
				return 1;
			}
			return 1;
		}
		if (pNewLeg.Line != null)
		{
			array[0] = new Eps_Point(pNewLeg.Line.EndPnt);
			num++;
			EPShared.eplGeoE_CalcHorzAngByThreePnts(array[1], array[2], array[0], 13, ref _dAngle, 1E-05);
			num2 += _dAngle;
			double dY = (double)(num - 2) * Math.PI;
			if (RevitDefines.EQ(num2, dY))
			{
				Direction = -1;
			}
			else
			{
				Direction = 1;
			}
			pNewLeg = null;
			return 0;
		}
		return 1;
	}

	private static int eplGeoE_ConvLegListIntoLineLegList(Eps_Leg pLeg, ref Eps_Leg pNewLeg)
	{
		if (pLeg == null)
		{
			return 6;
		}
		Eps_Leg eps_Leg = null;
		for (eps_Leg = pLeg; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				if (pNewLeg == null)
				{
					pNewLeg = Eps_Leg.SingleLeg(eps_Leg);
				}
				else
				{
					pNewLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
				}
			}
			else
			{
				if (eps_Leg.Arc == null)
				{
					return 6;
				}
				eplDsmE_AddArcLinesToLegList(eps_Leg.Arc, ref pNewLeg);
			}
		}
		return 0;
	}

	private static void eplDsmE_AddArcLinesToLegList(Eps_Arc pArc, ref Eps_Leg pNewLeg)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		int num4 = 0;
		Eps_Line eps_Line = new Eps_Line();
		Eps_Leg eps_Leg = null;
		Eps_Point _StaOffPnt = new Eps_Point();
		num2 = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(pArc.Radius, pArc.Delta);
		num4 = (int)(36.0 * pArc.Delta / Math.PI);
		eps_Line.StartPnt = new Eps_Point(pArc.StartPnt);
		eps_Line.EndPnt = new Eps_Point(pArc.EndPnt);
		if (RevitDefines.GT(num2, 0.0) && num4 > 0)
		{
			num = num2 / (double)num4;
			for (int i = 0; i < num4; i++)
			{
				num3 += num;
				EPShared.eplGeoE_FindStaOffPointAlongArc(pArc, num3, 0.0, ref _StaOffPnt);
				eps_Line.EndPnt = _StaOffPnt;
				eps_Leg = new Eps_Leg();
				eps_Leg.Line = new Eps_Line(eps_Line);
				if (pNewLeg == null)
				{
					pNewLeg = Eps_Leg.SingleLeg(eps_Leg);
				}
				else
				{
					pNewLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
				}
				eps_Line.StartPnt = new Eps_Point(eps_Line.EndPnt);
			}
		}
		else
		{
			eps_Leg = new Eps_Leg();
			eps_Leg.Line = new Eps_Line(eps_Line);
			if (pNewLeg == null)
			{
				pNewLeg = Eps_Leg.SingleLeg(eps_Leg);
			}
			else
			{
				pNewLeg.Last.Next = Eps_Leg.SingleLeg(eps_Leg);
			}
		}
	}

	public static int eplGeoE_CalcAreaOfLegList(Eps_Leg BoundaryLegList, ref double LegListArea)
	{
		double LegListPerimeter = 0.0;
		return eplGeoE_CalcAreaAndPerimeterOfLegList(BoundaryLegList, ref LegListArea, ref LegListPerimeter);
	}

	public static int eplGeoE_CalcAreaAndPerimeterOfLegList(Eps_Leg BoundaryLegList, ref double LegListArea, ref double LegListPerimeter)
	{
		LegListArea = 0.0;
		LegListPerimeter = 0.0;
		double _dChordArea = 0.0;
		List<Eps_Point> list = null;
		Eps_Leg eps_Leg = null;
		LegListPerimeter = eplGeoE_CalcLegListLength(BoundaryLegList);
		list = new List<Eps_Point>();
		for (eps_Leg = BoundaryLegList; eps_Leg != null; eps_Leg = eps_Leg.Next)
		{
			if (eps_Leg.Line != null)
			{
				list.Add(eps_Leg.Line.StartPnt);
			}
			else if (eps_Leg.Arc != null)
			{
				list.Add(eps_Leg.Arc.StartPnt);
				EPShared.eplGeoE_CalcArcChordArea(eps_Leg.Arc, ref _dChordArea);
				LegListArea += _dChordArea;
			}
		}
		LegListArea += eplGeoE_CalcPolygonArea(list);
		LegListArea = Math.Abs(LegListArea);
		return 0;
	}

	public static double eplGeoE_CalcPolygonArea(List<Eps_Point> Vertices)
	{
		if (Vertices == null || Vertices.Count == 0)
		{
			return 0.0;
		}
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		List<Eps_Point> list = null;
		int num4 = 0;
		int count = Vertices.Count;
		Eps_Point eps_Point = new Eps_Point(Vertices[0]);
		num2 = eps_Point.x;
		num3 = eps_Point.y;
		for (num4 = 0; num4 < count; num4++)
		{
			eps_Point = new Eps_Point(Vertices[num4]);
			if (eps_Point.x < num2)
			{
				num2 = eps_Point.x;
			}
			if (eps_Point.y < num3)
			{
				num3 = eps_Point.y;
			}
		}
		list = new List<Eps_Point>();
		for (num4 = 0; num4 < count; num4++)
		{
			list.Add(new Eps_Point(Vertices[num4].x - num2, Vertices[num4].y - num3, 0.0));
		}
		Eps_Point eps_Point2 = new Eps_Point(list[0]);
		Eps_Point eps_Point3 = new Eps_Point(list[0]);
		for (num4 = 0; num4 < count; num4++)
		{
			Eps_Point eps_Point4 = new Eps_Point(list[num4]);
			num += eps_Point2.x * eps_Point3.y + eps_Point2.y * eps_Point4.x + eps_Point4.y * eps_Point3.x - eps_Point3.y * eps_Point4.x - eps_Point2.y * eps_Point3.x - eps_Point2.x * eps_Point4.y;
			eps_Point3 = new Eps_Point(eps_Point4);
		}
		return num / 2.0;
	}
}

