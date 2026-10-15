using System;
using System.Globalization;
using Autodesk.Revit.DB;

namespace EPCommon;

public class ValueConversion
{
	public static double ToDouble(string _strValue) => Parse(_strValue);

	public static double Parse(string _strValue)
	{
		double num = 0.0;
		if (_strValue.IndexOf(".") >= 0)
		{
			NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
			numberFormatInfo.NumberDecimalSeparator = ".";
			return double.Parse(_strValue, numberFormatInfo);
		}
		if (_strValue.IndexOf(",") >= 0)
		{
			NumberFormatInfo numberFormatInfo2 = new NumberFormatInfo();
			numberFormatInfo2.NumberDecimalSeparator = ",";
			return double.Parse(_strValue, numberFormatInfo2);
		}
		return double.Parse(_strValue);
	}

	public static string ToString(double _dValue)
	{
		return Convert.ToString(_dValue);
	}

	public static string ToString(DecimalSymbol _DecimalSymbolType, double _dValue)
	{
		string text = Convert.ToString(_dValue);
		switch (_DecimalSymbolType)
		{
		case DecimalSymbol.Dot:
			text = text.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
			break;
		case DecimalSymbol.Comma:
			text = text.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ",");
			break;
		}
		return text;
	}

	public static string Format(string _strFormat, params object[] _Arguments)
	{
		return string.Format(_strFormat, _Arguments);
	}

	public static string Format(DecimalSymbol _DecimalSymbolType, string _strFormat, params object[] _Arguments)
	{
		string text = string.Format(_strFormat, _Arguments);
		switch (_DecimalSymbolType)
		{
		case DecimalSymbol.Dot:
			text = text.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
			break;
		case DecimalSymbol.Comma:
			text = text.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ",");
			break;
		}
		return text;
	}
}
