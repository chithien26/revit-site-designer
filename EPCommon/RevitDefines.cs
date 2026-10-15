using System;

namespace EPCommon;

public sealed class RevitDefines
{
	public sealed class Tolerance
	{
		public const double OneTenth = 0.1;

		public const double OneHundredth = 0.01;

		public const double OneThousandth = 0.001;

		public const double TenThousandth = 0.0001;

		public const double OneHundredThousandth = 1E-05;

		public const double OneMillionth = 1E-06;

		public const double OneHundredMillionth = 1E-07;

		public const double OneBillionth = 1E-08;

		public const double OneTrillionth = 1E-10;
	}

	public const int EPC_CLOCKWISE = -1;

	public const int EPC_COUNTERCLOCKWISE = 1;

	public const int EPC_OFFSET_RIGHT = 1;

	public const int EPC_OFFSET_LEFT = 2;

	public const int EPC_ANGLE = 11;

	public const int EPC_ANGLE_RIGHT = 12;

	public const int EPC_ANGLE_LEFT = 13;

	public const int EPC_DEFLECT_RIGHT = 14;

	public const int EPC_DEFLECT_LEFT = 15;

	public const int EPC_TYPE_LEGLIST_NORMAL = 0;

	public const int EPC_TYPE_LEGLIST_SPLINE = 1;

	public const int EPC_TYPE_LEGLIST_FIT = 2;

	public const int EPC_TYPE_LEGLIST_ALL = 3;

	public const int EPC_TYPE_SM_CURVE_BREAKLINES = 4;

	public const double EPC_TOL_POINTS = 1E-05;

	public const double EP_EPC_TOL_POINTS = 1E-09;

	public const double EPC_TOL_LENGTH = 1E-05;

	public const double EPC_TOL_STATION = 1E-05;

	public const double EPC_TOL_ANGLE = 1E-10;

	public const double EPC_TOL_EPOLY = 0.001;

	public const double EPC_TOL_REVIT_DRAW_LINE = 0.0025;

	public const double OFFSET_TOLERANCE = 0.01;

	public const double EPC_METERS_PER_FOOT = 0.3048;

	public const double EPC_FOOT_PER_INCH = 0.0833333;

	public const double EPC_INCHES_PER_FOOT = 12.0;

	public const double EPC_METERS_PER_INCH = 0.0254;

	public const double EPC_FEETS_PER_KILOMETER = 3280.8399;

	public const double EPC_FEETS_PER_MILE = 5280.0;

	public const double EPC_RADIANS_PER_DEGREE = 0.0174532925;

	public const double EPC_HECTARES_PER_SQUARE_FOOT = 9.290304E-06;

	public const double EPC_SQUARE_INCHES_PER_SQUARE_FOOT = 144.0;

	public const double EPC_SQUARE_MILLIMETERS_PER_SQUARE_METER = 1000000.0;

	public const double EPC_SQUARE_CENTIMETERS_PER_SQUARE_METER = 10000.0;

	public const double EPC_SQUARE_FEET_PER_ACRE = 43560.0;

	public const double EPC_SQUARE_METERS_PER_SQUARE_FOOT = 0.09290304;

	public const double EPC_CUBIC_FEET_PER_CUBIC_YARD = 27.0;

	public const double EPC_GALLONS_PER_CUBIC_FOOT = 7.48052;

	public const double EPC_CUBIC_METERS_PER_CUBIC_FOOT = 0.028316846592;

	public const double EPC_LITERS_PER_CUBIC_FOOT = 28.316846592;

	public const double EPC_PI = Math.PI;

	public const double EPC_PI_OVER_2 = Math.PI / 2.0;

	public const double EPC_2PI = Math.PI * 2.0;

	public const double EPC_3PI_OVER_2 = 4.71238898038469;

	public const double EPC_DEGREES_TO_RADIANS = Math.PI / 180.0;

	public const string EPC_FOOT_ABBREVIATION = "'";

	public const string EPC_INCH_ABBREVIATION = "\"";

	public const string EPC_METER_ABBREVIATION = "m";

	public const string EPC_CENTIMETER_ABBREVIATION = "cm";

	public const string EPC_MILLIMETER_ABBREVIATION = "mm";

	public const string EPC_FOOT_LF_ABBREVIATION = "LF";

	public const string EPC_DEGREE_ABBREVIATION = "°";

	public const string EPC_MINUTE_ABBREVIATION = "'";

	public const string EPC_SECOND_ABBREVIATION = "\"";

	public const string EPC_SQUARE_FOOT_ABBREVIATION = "ft²";

	public const string EPC_SQUARE_FOOT_SF_ABBREVIATION = "SF";

	public const string EPC_SQUARE_INCH_ABBREVIATION = "in²";

	public const string EPC_SQUARE_METER_ABBREVIATION = "m²";

	public const string EPC_SQUARE_CENTIMETER_ABBREVIATION = "cm²";

	public const string EPC_SQUARE_MILLIMETER_ABBREVIATION = "mm²";

	public const string EPC_ACRE_ABBREVIATION = "acres";

	public const string EPC_HECTARCE_ABBREVIATION = "hectare";

	public const string EPC_CUBIC_FOOT_ABBREVIATION = "ft³";

	public const string EPC_CUBIC_FOOT_CF_ABBREVIATION = "CF";

	public const string EPC_CUBIC_YARD_ABBREVIATION = "CY";

	public const string EPC_CUBIC_METER_ABBREVIATION = "m³";

	public const string EPC_LITER_ABBREVIATION = "L";

	public const string EPC_US_GALLONS_ABBREVIATION = "gal";

	public const string EPC_RATIO_12 = ":12";

	public const string EPC_RATIO_10 = ":10";

	public const string EPC_12_INCH = "/ 12\"";

	public const string EPC_1_FEET = "/ 1' - 0\"";

	public const string EPC_1000_MM = "/ 1000";

	public const string EPC_MM = "mm";

	public const string EPC_PERCENTAGE_SIGN = "%";

	public const int EPC_DISPLAY_MAX_INCH_DENOMINATOR = 256;

	public const double EPC_DISPLAY_MIN_DECIMAL_FEET = 0.001;

	public const double EPC_DISPLAY_MIN_DECIMAL_INCH = 0.01;

	public const double EPC_DISPLAY_MIN_DECIMAL_METER = 0.0001;

	public const double EPC_DISPLAY_MIN_DECIMAL_CENTIMETER = 0.01;

	public const double EPC_DISPLAY_MIN_DECIMAL_MILLIMETER = 0.1;

	public const double EPC_DISPLAY_MIN_DECIMAL_DEGREE = 0.001;

	public const double EPC_DISPLAY_MIN_DECIMAL_SQUARE_FOOT = 0.01;

	public const double EPC_DISPLAY_MIN_DECIMAL_SQUARE_INCH = 0.01;

	public const double EPC_DISPLAY_MIN_DECIMAL_SQUARE_METER = 0.001;

	public const double EPC_DISPLAY_MIN_DECIMAL_SQUARE_CENTIMETER = 0.01;

	public const double EPC_DISPLAY_MIN_DECIMAL_SQUARE_MILLIMETER = 0.01;

	public const double EPC_DISPLAY_MIN_DECIMAL_ACRE = 0.0001;

	public const double EPC_DISPLAY_MIN_DECIMAL_HECTARE = 0.0001;

	public const double EPC_DISPLAY_MIN_DECIMAL_CUBIC_FOOT = 0.01;

	public const double EPC_DISPLAY_MIN_DECIMAL_CUBIC_YARD = 0.01;

	public const double EPC_DISPLAY_MIN_DECIMAL_CUBIC_METER = 0.001;

	public const double EPC_DISPLAY_MIN_DECIMAL_LITER = 0.1;

	public const double EPC_DISPLAY_MIN_DECIMAL_US_GALLON = 0.1;

	public const double EPC_DISPLAY_MIN_DECIMAL_PERCENTAGE = 0.0001;

	public const int EPC_SUCCESS = 0;

	public const int EPC_ERROR = 1;

	public const int EPC_FAIL = 6;

	public const int EPC_STATION_OUT_OF_RANGE = 506;

	public const int EPC_ILLEGAL_OBJECT_TYPE = 700;

	public const int EPC_LINES_PARALLEL = 1103;

	public const int EPC_LINES_INTERSECT_REVERSE_NONE = 1104;

	public const int EPC_POINTS_DUPLICATE_XY = 1105;

	public const int EPC_LINES_INTERSECT_REVERSE_BOTH = 1106;

	public const int EPC_LINES_INTERSECT_REVERSE_FIRST = 1107;

	public const int EPC_LINES_IDENTICAL_SAME_DIR = 1108;

	public const int EPC_LINES_IDENTICAL_OPPOSITE_DIR = 1109;

	public const int EPC_LINES_NOT_IDENTICAL = 1110;

	public const int EPC_LINES_COLINEAR = 1111;

	public const int EPC_LINES_INTERSECT_REVERSE_SECOND = 1112;

	public const int EPC_POINTS_DUPLICATE_XYZ = 1113;

	public const int EPC_ARC_NOT_DUPLICATE = 1209;

	public const int EPC_ARC_INVALID_CHORD_LENGTH = 1210;

	public const int EPC_ARC_DELTA_EQUALS_PI = 1211;

	public const int EPC_ARC_DELTA_OVER_PI = 1212;

	public const int EPC_INTERSECT_NONE = 1600;

	public const int EPC_INTERSECT_NONE_INSIDE = 1601;

	public const int EPC_INTERSECT_NONE_OUTSIDE = 1602;

	public const int EPC_INTERSECT_NONE_DUPLICATE = 1604;

	public const int EPC_INTERSECT_NONE_CONCENTRIC = 1605;

	public const int EPC_OBJECTS_INTERSECT = 1606;

	public const int EPC_OBJECTS_INTERSECT_TWICE = 1607;

	public const int EPC_OBJECTS_OVERLAP = 1608;

	public const int EPC_OBJECTS_DONT_OVERLAP = 1609;

	public const int EPC_OBJECT1_WITHIN_OBJECT2 = 1610;

	public const int EPC_OBJECT2_WITHIN_OBJECT1 = 1611;

	public const int EPC_OBJECTS_SHARE_ONE_ENDPOINT = 1612;

	public const int EPC_OBJECTS_SHARE_TWO_ENDPOINTS = 1613;

	public const int EPC_OBJECTS_SHARE_STARTPOINT = 1614;

	public const int EPC_OBJECTS_SHARE_ENDPOINT = 1615;

	public const int EPC_OBJECTS_SHARE_START_ENDPOINT = 1616;

	public const int EPC_OBJECTS_SHARE_END_STARTPOINT = 1617;

	public const int EPC_OBJECTS_OVERLAP_ONE_ENDPOINT = 1618;

	public const int EPC_OBJECTS_OVERLAP_TWICE_SHARE_ONE_ENDPOINT = 1619;

	public const int EPC_OBJECTS_OVERLAP_TWICE = 1620;

	public const int EPC_WITHIN_OBJECT = 1700;

	public const int EPC_NOT_WITHIN_OBJECT = 1701;

	public const int EPC_POINT_ON_POLYGON = 1903;

	public const int EPC_INVALID_ANGULAR_UNITS = 2500;

	public const double EPC_THE_BIG_ONE = double.MaxValue;

	public const int EPC_FAHRENHEIT = 0;

	public const int EPC_CELSIUS = 1;

	public const int EPC_KELVIN = 2;

	public const int EPC_INCHES_HG = 0;

	public const int EPC_MM_HG = 1;

	public const int EPC_HPA = 2;

	public const int EPC_MILLIBARS = 3;

	public const int EPC_INCHES = 20;

	public const int EPC_FEET = 21;

	public const int EPC_YARDS = 22;

	public const int EPC_MILES = 23;

	public const int EPC_MILLIMETERS = 24;

	public const int EPC_CENTIMETERS = 25;

	public const int EPC_METERS = 26;

	public const int EPC_KILOMETERS = 27;

	public const int EPC_USSURVEY_FEET = 28;

	public const int EPC_ACRES = 30;

	public const int EPC_HECTARES = 31;

	public const int EPC_SQUARE_FEET = 32;

	public const int EPC_SQUARE_METERS = 33;

	public const int EPC_SQUARE_MILES = 34;

	public const int EPC_SQUARE_KILOMETERS = 35;

	public const int EPC_SQUARE_INCHES = 36;

	public const int EPC_SQUARE_MILLIMETERS = 37;

	public const int EPC_SQUARE_CENTIMETERS = 38;

	public const int EPC_CUBIC_FEET = 40;

	public const int EPC_CUBIC_METERS = 41;

	public const int EPC_ACRE_FEET = 42;

	public const int EPC_CUBIC_YARDS = 43;

	public const int EPC_GALLONS = 44;

	public const int EPC_HECTARE_METERS = 45;

	public const int EPC_LITERS = 46;

	public const int EPC_CUBIC_INCHES = 47;

	public const int EPC_IMP_GALLONS = 48;

	public const int EPC_CUBIC_METERS_PER_SECOND = 70;

	public const int EPC_CUBIC_FEET_PER_SECOND = 71;

	public const int EPC_GALLONS_PER_DAY = 72;

	public const int EPC_IMP_GALLONS_PER_DAY = 73;

	public const int EPC_CUBIC_FEET_PER_DAY = 74;

	public const int EPC_ACRE_FEET_PER_DAY = 75;

	public const int EPC_GALLONS_PER_MINUTE = 76;

	public const int EPC_IMP_GALLONS_PER_MINUTE = 77;

	public const int EPC_LITER_PER_MINUTE = 78;

	public const int EPC_LITER_PER_SECOND = 79;

	public const int EPC_INCHES_PER_HOUR = 80;

	public const int EPC_MILLIMETERS_PER_HOUR = 81;

	public const int EPC_METERS_PER_SECOND = 82;

	public const int EPC_FEET_PER_SECOND = 83;

	public const int EPC_MILES_PER_HOUR = 84;

	public const int EPC_KILOMETERS_PER_HOUR = 85;

	public const int EPC_DECIMAL_DEGREES = 0;

	public const int EPC_DEG_MIN_SEC = 1;

	public const int EPC_GRADS = 2;

	public const int EPC_MILS = 3;

	public const int EPC_RADIANS = 4;

	public const int EPC_ENGLISH = 0;

	public const int EPC_METRIC = 1;

	public const int LEG_LINE_ORDER = 0;

	public const int LEG_ARC_ORDER = 1;

	public const int PROJECT_INNER_SLOPES = 0;

	public const int EXCLUDE_INNER_SLOPES = 1;

	public const int PROJECT_LEFT_SLOPES = 2;

	public const int PROJECT_RIGHT_SLOPES = 3;

	public const int PROJECT_NO_SLOPES = 4;

	public const int PROJECT_NO_CURBS_SLOPES = 5;

	public const int IMPERIAL_NEW_CONSTRUCTION_PHASE = 118390;

	public const int IMPERIAL_EXISTING_PHASE = 32440;

	public const int METRIC_NEW_CONSTRUCTION_PHASE = 86961;

	public const int METRIC_EXISTING_PHASE = 12589;

	public const string EPC_EPDR_REGISTRY_KEY = "Software\\Eagle Point Software\\Eagle Point Data Reduction";

	public static bool GE(double _dX, double _dY)
	{
		return GE(_dX, _dY, 0.0001);
	}

	public static bool GE(double _dX, double _dY, double _dTolerance)
	{
		if (!(_dY < _dX))
		{
			return EQ(_dX, _dY, _dTolerance);
		}
		return true;
	}

	public static bool LE(double _dX, double _dY)
	{
		return LE(_dX, _dY, 0.0001);
	}

	public static bool LE(double _dX, double _dY, double _dTolerance)
	{
		if (!(_dY > _dX))
		{
			return EQ(_dX, _dY, _dTolerance);
		}
		return true;
	}

	public static bool LT(double _dX, double _dY)
	{
		return LT(_dX, _dY, 0.0001);
	}

	public static bool LT(double _dX, double _dY, double _dTolerance)
	{
		return !GE(_dX, _dY, _dTolerance);
	}

	public static bool EQ(double _dX, double _dY)
	{
		return EQ(_dX, _dY, 0.0001);
	}

	public static bool EQ(double _dX, double _dY, double _dTolerance)
	{
		return Math.Abs(_dX - _dY) <= _dTolerance;
	}

	public static bool GT(double _dX, double _dY)
	{
		return GT(_dX, _dY, 0.0001);
	}

	public static bool GT(double _dX, double _dY, double _dTolerance)
	{
		return !LE(_dX, _dY, _dTolerance);
	}
}
