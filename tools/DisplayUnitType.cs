namespace Autodesk.Revit.DB;

/// <summary>
///    The units and display format used to format numbers as strings.  Also used for unit conversions.
/// </summary>
/// <remarks>
///   <para>Display units are used for two purposes:</para>
///   <list type="number">
///     <item>Formatting and parsing string representations of values.  See
///    FormatOptions.DisplayUnit and the formatting and parsing utilities
///    in the UnitFormatUtils class.</item>
///     <item>Conversion of values from one unit to another.  See the unit
///    conversion utilities in the UnitUtils class.</item>
///   </list>
///   <para>Most display units represent simple units like meters, square
///    feet, or degrees, and are formatted as decimal numbers.  Some
///    display units represent specialized formatting methods like "feet
///    and fractional inches" or "degrees, minutes and seconds".  Such
///    display units still correspond to an underlying simple unit when
///    used with the conversion utilities.</para>
/// </remarks>
public enum DisplayUnitType
{
	DUT_UNDEFINED = -2,
	DUT_CUSTOM = -1,
	DUT_METERS = 0,
	DUT_CENTIMETERS = 1,
	DUT_MILLIMETERS = 2,
	DUT_DECIMAL_FEET = 3,
	DUT_FEET_FRACTIONAL_INCHES = 4,
	DUT_FRACTIONAL_INCHES = 5,
	DUT_DECIMAL_INCHES = 6,
	DUT_METERS_CENTIMETERS = 9,
	DUT_SQUARE_FEET = 11,
	DUT_SQUARE_INCHES = 20,
	DUT_SQUARE_METERS = 12,
	DUT_SQUARE_CENTIMETERS = 21,
	DUT_SQUARE_MILLIMETERS = 22,
	DUT_ACRES = 7,
	DUT_HECTARES = 8,
	DUT_CUBIC_YARDS = 10,
	DUT_CUBIC_FEET = 13,
	DUT_CUBIC_INCHES = 23,
	DUT_CUBIC_METERS = 14,
	DUT_CUBIC_CENTIMETERS = 24,
	DUT_CUBIC_MILLIMETERS = 25,
	/// <summary>
	///    liter (L)
	/// </summary>
	DUT_LITERS = 26,
	/// <summary>
	///    gallon (U.S.) (gal)
	/// </summary>
	DUT_GALLONS_US = 27,
	DUT_DECIMAL_DEGREES = 15,
	DUT_DEGREES_AND_MINUTES = 16,
	DUT_GENERAL = 17,
	DUT_FIXED = 18,
	DUT_PERCENTAGE = 19,
	/// <summary>
	///    kilogram per cubic meter (kg/mÂ³)
	/// </summary>
	DUT_KILOGRAMS_PER_CUBIC_METER = 28,
	/// <summary>
	///    pound per cubic foot (lb/ftÂ³)
	/// </summary>
	DUT_POUNDS_MASS_PER_CUBIC_FOOT = 29,
	/// <summary>
	///    pound per cubic inch (lb/inÂ³)
	/// </summary>
	DUT_POUNDS_MASS_PER_CUBIC_INCH = 30,
	/// <summary>
	///    British thermal unit[IT] (Btu[IT])
	/// </summary>
	DUT_BRITISH_THERMAL_UNITS = 31,
	/// <summary>
	///    calorie[IT] (cal[IT])
	/// </summary>
	DUT_CALORIES = 32,
	/// <summary>
	///    kilocalorie[IT] (kcal[IT])
	/// </summary>
	DUT_KILOCALORIES = 33,
	/// <summary>
	///    joule (J)
	/// </summary>
	DUT_JOULES = 34,
	/// <summary>
	///    kilowatt hour (kW Â· h)
	/// </summary>
	DUT_KILOWATT_HOURS = 35,
	/// <summary>
	///    therm (EC)
	/// </summary>
	DUT_THERMS = 36,
	/// <summary>
	///    Inches of water per 100 feet
	/// </summary>
	DUT_INCHES_OF_WATER_PER_100FT = 37,
	/// <summary>
	///    pascal per meter (N/m)
	/// </summary>
	DUT_PASCALS_PER_METER = 38,
	/// <summary>
	///    watt (W)
	/// </summary>
	DUT_WATTS = 39,
	/// <summary>
	///    kilowatt (kW)
	/// </summary>
	DUT_KILOWATTS = 40,
	/// <summary>
	///    British thermal unit[IT] per second (Btu[IT]/s)
	/// </summary>
	DUT_BRITISH_THERMAL_UNITS_PER_SECOND = 41,
	/// <summary>
	///    British thermal unit[IT] per hour (Btu[IT]/h)
	/// </summary>
	DUT_BRITISH_THERMAL_UNITS_PER_HOUR = 42,
	/// <summary>
	///    calorie[IT] per second (cal[IT]/s)
	/// </summary>
	DUT_CALORIES_PER_SECOND = 43,
	/// <summary>
	///    kilocalorie[IT] per second (kcal[IT]/s)
	/// </summary>
	DUT_KILOCALORIES_PER_SECOND = 44,
	/// <summary>
	///    watt per square foot (W/ftÂ²)
	/// </summary>
	DUT_WATTS_PER_SQUARE_FOOT = 45,
	/// <summary>
	///    watt per square meter (W/mÂ²)
	/// </summary>
	DUT_WATTS_PER_SQUARE_METER = 46,
	/// <summary>
	///    inch of water (60.8Â°F)
	/// </summary>
	DUT_INCHES_OF_WATER = 47,
	/// <summary>
	///    pascal (Pa)
	/// </summary>
	DUT_PASCALS = 48,
	/// <summary>
	///    kilopascal (kPa)
	/// </summary>
	DUT_KILOPASCALS = 49,
	/// <summary>
	///    megapascal (MPa)
	/// </summary>
	DUT_MEGAPASCALS = 50,
	/// <summary>
	///    pound-force per square inch (psi) (lbf/in2)
	/// </summary>
	DUT_POUNDS_FORCE_PER_SQUARE_INCH = 51,
	/// <summary>
	///    inch of mercury  conventional (inHg)
	/// </summary>
	DUT_INCHES_OF_MERCURY = 52,
	/// <summary>
	///    millimeter of mercury  conventional (mmHg)
	/// </summary>
	DUT_MILLIMETERS_OF_MERCURY = 53,
	/// <summary>
	///    atmosphere  standard (atm)
	/// </summary>
	DUT_ATMOSPHERES = 54,
	/// <summary>
	///    bar (bar)
	/// </summary>
	DUT_BARS = 55,
	/// <summary>
	///    degree Fahrenheit (Â°F)
	/// </summary>
	DUT_FAHRENHEIT = 56,
	/// <summary>
	///    degree Celsius (Â°C)
	/// </summary>
	DUT_CELSIUS = 57,
	/// <summary>
	///    kelvin (K)
	/// </summary>
	DUT_KELVIN = 58,
	/// <summary>
	///    degree Rankine (Â°R)
	/// </summary>
	DUT_RANKINE = 59,
	/// <summary>
	///    foot per minute (ft/min)
	/// </summary>
	DUT_FEET_PER_MINUTE = 60,
	/// <summary>
	///    meter per second (m/s)
	/// </summary>
	DUT_METERS_PER_SECOND = 61,
	/// <summary>
	///    centimeter per minute (cm/min)
	/// </summary>
	DUT_CENTIMETERS_PER_MINUTE = 62,
	/// <summary>
	///    cubic foot per minute (ftÂ³/min)
	/// </summary>
	DUT_CUBIC_FEET_PER_MINUTE = 63,
	/// <summary>
	///    liter per second (L/s)
	/// </summary>
	DUT_LITERS_PER_SECOND = 64,
	/// <summary>
	///    cubic meter per second (mÂ³/s)
	/// </summary>
	DUT_CUBIC_METERS_PER_SECOND = 65,
	/// <summary>
	///    cubic meters per hour (mÂ³/h)
	/// </summary>
	DUT_CUBIC_METERS_PER_HOUR = 66,
	/// <summary>
	///    gallon (U.S.) per minute (gpm) (gal/min)
	/// </summary>
	DUT_GALLONS_US_PER_MINUTE = 67,
	/// <summary>
	///    gallon (U.S.) per hour (gph) (gal/h)
	/// </summary>
	DUT_GALLONS_US_PER_HOUR = 68,
	/// <summary>
	///    ampere (A)
	/// </summary>
	DUT_AMPERES = 69,
	/// <summary>
	///    kiloampere (kA)
	/// </summary>
	DUT_KILOAMPERES = 70,
	/// <summary>
	///    milliampere (mA)
	/// </summary>
	DUT_MILLIAMPERES = 71,
	/// <summary>
	///    volt (V)
	/// </summary>
	DUT_VOLTS = 72,
	/// <summary>
	///    kilovolt (kV)
	/// </summary>
	DUT_KILOVOLTS = 73,
	/// <summary>
	///    millivolt (mV)
	/// </summary>
	DUT_MILLIVOLTS = 74,
	/// <summary>
	///    hertz (Hz)
	/// </summary>
	DUT_HERTZ = 75,
	DUT_CYCLES_PER_SECOND = 76,
	/// <summary>
	///    lux (lx)
	/// </summary>
	DUT_LUX = 77,
	/// <summary>
	///    footcandle
	/// </summary>
	DUT_FOOTCANDLES = 78,
	/// <summary>
	///    footlambert
	/// </summary>
	DUT_FOOTLAMBERTS = 79,
	/// <summary>
	///    candela per square meter (cd/mÂ²)
	/// </summary>
	DUT_CANDELAS_PER_SQUARE_METER = 80,
	/// <summary>
	///    candela (cd)
	/// </summary>
	DUT_CANDELAS = 81,
	/// <summary>
	///    obsolete
	/// </summary>
	DUT_CANDLEPOWER = 82,
	/// <summary>
	///    lumen (lm)
	/// </summary>
	DUT_LUMENS = 83,
	DUT_VOLT_AMPERES = 84,
	DUT_KILOVOLT_AMPERES = 85,
	/// <summary>
	///    horsepower (550 ft Â· lbf/s)
	/// </summary>
	DUT_HORSEPOWER = 86,
	DUT_NEWTONS = 87,
	DUT_DECANEWTONS = 88,
	DUT_KILONEWTONS = 89,
	DUT_MEGANEWTONS = 90,
	DUT_KIPS = 91,
	DUT_KILOGRAMS_FORCE = 92,
	DUT_TONNES_FORCE = 93,
	DUT_POUNDS_FORCE = 94,
	DUT_NEWTONS_PER_METER = 95,
	DUT_DECANEWTONS_PER_METER = 96,
	DUT_KILONEWTONS_PER_METER = 97,
	DUT_MEGANEWTONS_PER_METER = 98,
	DUT_KIPS_PER_FOOT = 99,
	DUT_KILOGRAMS_FORCE_PER_METER = 100,
	DUT_TONNES_FORCE_PER_METER = 101,
	DUT_POUNDS_FORCE_PER_FOOT = 102,
	DUT_NEWTONS_PER_SQUARE_METER = 103,
	DUT_DECANEWTONS_PER_SQUARE_METER = 104,
	DUT_KILONEWTONS_PER_SQUARE_METER = 105,
	DUT_MEGANEWTONS_PER_SQUARE_METER = 106,
	DUT_KIPS_PER_SQUARE_FOOT = 107,
	DUT_KILOGRAMS_FORCE_PER_SQUARE_METER = 108,
	DUT_TONNES_FORCE_PER_SQUARE_METER = 109,
	DUT_POUNDS_FORCE_PER_SQUARE_FOOT = 110,
	DUT_NEWTON_METERS = 111,
	DUT_DECANEWTON_METERS = 112,
	DUT_KILONEWTON_METERS = 113,
	DUT_MEGANEWTON_METERS = 114,
	DUT_KIP_FEET = 115,
	DUT_KILOGRAM_FORCE_METERS = 116,
	DUT_TONNE_FORCE_METERS = 117,
	DUT_POUND_FORCE_FEET = 118,
	DUT_METERS_PER_KILONEWTON = 119,
	DUT_FEET_PER_KIP = 120,
	DUT_SQUARE_METERS_PER_KILONEWTON = 121,
	DUT_SQUARE_FEET_PER_KIP = 122,
	DUT_CUBIC_METERS_PER_KILONEWTON = 123,
	DUT_CUBIC_FEET_PER_KIP = 124,
	DUT_INV_KILONEWTONS = 125,
	DUT_INV_KIPS = 126,
	/// <summary>
	///    foot of water  conventional (ftH2O) per 100 ft
	/// </summary>
	DUT_FEET_OF_WATER_PER_100FT = 127,
	/// <summary>
	///    foot of water  conventional (ftH2O)
	/// </summary>
	DUT_FEET_OF_WATER = 128,
	/// <summary>
	///    pascal second (Pa Â· s)
	/// </summary>
	DUT_PASCAL_SECONDS = 129,
	/// <summary>
	///    pound per foot second (lb/(ft Â· s))
	/// </summary>
	DUT_POUNDS_MASS_PER_FOOT_SECOND = 130,
	/// <summary>
	///    centipoise (cP)
	/// </summary>
	DUT_CENTIPOISES = 131,
	/// <summary>
	///    foot per second (ft/s)
	/// </summary>
	DUT_FEET_PER_SECOND = 132,
	DUT_KIPS_PER_SQUARE_INCH = 133,
	/// <summary>
	///    kilnewtons per cubic meter (kN/mÂ³)
	/// </summary>
	DUT_KILONEWTONS_PER_CUBIC_METER = 134,
	/// <summary>
	///    pound per cubic foot (kip/ftÂ³)
	/// </summary>
	DUT_POUNDS_FORCE_PER_CUBIC_FOOT = 135,
	/// <summary>
	///    pound per cubic foot (kip/inÂ³)
	/// </summary>
	DUT_KIPS_PER_CUBIC_INCH = 136,
	DUT_INV_FAHRENHEIT = 137,
	DUT_INV_CELSIUS = 138,
	DUT_NEWTON_METERS_PER_METER = 139,
	DUT_DECANEWTON_METERS_PER_METER = 140,
	DUT_KILONEWTON_METERS_PER_METER = 141,
	DUT_MEGANEWTON_METERS_PER_METER = 142,
	DUT_KIP_FEET_PER_FOOT = 143,
	DUT_KILOGRAM_FORCE_METERS_PER_METER = 144,
	DUT_TONNE_FORCE_METERS_PER_METER = 145,
	DUT_POUND_FORCE_FEET_PER_FOOT = 146,
	/// <summary>
	///    pound per foot hour (lb/(ft Â· h))
	/// </summary>
	DUT_POUNDS_MASS_PER_FOOT_HOUR = 147,
	DUT_KIPS_PER_INCH = 148,
	/// <summary>
	///    pound per cubic foot (kip/ftÂ³)
	/// </summary>
	DUT_KIPS_PER_CUBIC_FOOT = 149,
	DUT_KIP_FEET_PER_DEGREE = 150,
	DUT_KILONEWTON_METERS_PER_DEGREE = 151,
	DUT_KIP_FEET_PER_DEGREE_PER_FOOT = 152,
	DUT_KILONEWTON_METERS_PER_DEGREE_PER_METER = 153,
	/// <summary>
	///    watt per square meter kelvin (W/(mÂ² Â· K))
	/// </summary>
	DUT_WATTS_PER_SQUARE_METER_KELVIN = 154,
	/// <summary>
	///    British thermal unit[IT] per hour square foot degree Fahrenheit (Btu[IT]/(h Â· ftÂ² Â· Â°F)
	/// </summary>
	DUT_BRITISH_THERMAL_UNITS_PER_HOUR_SQUARE_FOOT_FAHRENHEIT = 155,
	/// <summary>
	///    cubic foot per minute square foot
	/// </summary>
	DUT_CUBIC_FEET_PER_MINUTE_SQUARE_FOOT = 156,
	/// <summary>
	///    liter per second square meter
	/// </summary>
	DUT_LITERS_PER_SECOND_SQUARE_METER = 157,
	DUT_RATIO_10 = 158,
	DUT_RATIO_12 = 159,
	DUT_SLOPE_DEGREES = 160,
	DUT_RISE_OVER_INCHES = 161,
	DUT_RISE_OVER_FOOT = 162,
	DUT_RISE_OVER_MMS = 163,
	/// <summary>
	///    watt per cubic foot (W/mÂ³)
	/// </summary>
	DUT_WATTS_PER_CUBIC_FOOT = 164,
	/// <summary>
	///    watt per cubic meter (W/mÂ³)
	/// </summary>
	DUT_WATTS_PER_CUBIC_METER = 165,
	/// <summary>
	///    British thermal unit[IT] per hour square foot (Btu[IT]/(h Â· ftÂ²)
	/// </summary>
	DUT_BRITISH_THERMAL_UNITS_PER_HOUR_SQUARE_FOOT = 166,
	/// <summary>
	///    British thermal unit[IT] per hour cubic foot (Btu[IT]/(h Â· ftÂ³)
	/// </summary>
	DUT_BRITISH_THERMAL_UNITS_PER_HOUR_CUBIC_FOOT = 167,
	/// <summary>
	///    Ton of refrigeration (12 000 Btu[IT]/h)
	/// </summary>
	DUT_TON_OF_REFRIGERATION = 168,
	/// <summary>
	///    cubic foot per minute cubic foot
	/// </summary>
	DUT_CUBIC_FEET_PER_MINUTE_CUBIC_FOOT = 169,
	/// <summary>
	///    liter per second cubic meter
	/// </summary>
	DUT_LITERS_PER_SECOND_CUBIC_METER = 170,
	/// <summary>
	///    cubic foot per minute ton of refrigeration
	/// </summary>
	DUT_CUBIC_FEET_PER_MINUTE_TON_OF_REFRIGERATION = 171,
	/// <summary>
	///    liter per second kilowatt
	/// </summary>
	DUT_LITERS_PER_SECOND_KILOWATTS = 172,
	/// <summary>
	///    square foot per ton of refrigeration
	/// </summary>
	DUT_SQUARE_FEET_PER_TON_OF_REFRIGERATION = 173,
	/// <summary>
	///    square meter per kilowatt
	/// </summary>
	DUT_SQUARE_METERS_PER_KILOWATTS = 174,
	DUT_CURRENCY = 175,
	DUT_LUMENS_PER_WATT = 176,
	/// <summary>
	///    square foot per thousand British thermal unit[IT] per hour
	/// </summary>
	DUT_SQUARE_FEET_PER_THOUSAND_BRITISH_THERMAL_UNITS_PER_HOUR = 177,
	DUT_KILONEWTONS_PER_SQUARE_CENTIMETER = 178,
	DUT_NEWTONS_PER_SQUARE_MILLIMETER = 179,
	DUT_KILONEWTONS_PER_SQUARE_MILLIMETER = 180,
	DUT_RISE_OVER_120_INCHES = 181,
	DUT_1_RATIO = 182,
	DUT_RISE_OVER_10_FEET = 183,
	DUT_HOUR_SQUARE_FOOT_FAHRENHEIT_PER_BRITISH_THERMAL_UNIT = 184,
	DUT_SQUARE_METER_KELVIN_PER_WATT = 185,
	DUT_BRITISH_THERMAL_UNIT_PER_FAHRENHEIT = 186,
	DUT_JOULES_PER_KELVIN = 187,
	DUT_KILOJOULES_PER_KELVIN = 188,
	/// <summary>
	///    kilograms (kg)
	/// </summary>
	DUT_KILOGRAMS_MASS = 189,
	/// <summary>
	///    tonnes (t)
	/// </summary>
	DUT_TONNES_MASS = 190,
	/// <summary>
	///    pounds (lb)
	/// </summary>
	DUT_POUNDS_MASS = 191,
	/// <summary>
	///    meters per second squared (m/sÂ²)
	/// </summary>
	DUT_METERS_PER_SECOND_SQUARED = 192,
	/// <summary>
	///    kilometers per second squared (km/sÂ²)
	/// </summary>
	DUT_KILOMETERS_PER_SECOND_SQUARED = 193,
	/// <summary>
	///    inches per second squared (in/sÂ²)
	/// </summary>
	DUT_INCHES_PER_SECOND_SQUARED = 194,
	/// <summary>
	///    feet per second squared (ft/sÂ²)
	/// </summary>
	DUT_FEET_PER_SECOND_SQUARED = 195,
	/// <summary>
	///    miles per second squared (mi/sÂ²)
	/// </summary>
	DUT_MILES_PER_SECOND_SQUARED = 196,
	/// <summary>
	///    feet to the fourth power	(ft^4)
	/// </summary>
	DUT_FEET_TO_THE_FOURTH_POWER = 197,
	/// <summary>
	///    inches to the fourth power	(in^4)
	/// </summary>
	DUT_INCHES_TO_THE_FOURTH_POWER = 198,
	/// <summary>
	///    millimeters to the fourth power	(mm^4)
	/// </summary>
	DUT_MILLIMETERS_TO_THE_FOURTH_POWER = 199,
	/// <summary>
	///    centimeters to the fourth power	(cm^4)
	/// </summary>
	DUT_CENTIMETERS_TO_THE_FOURTH_POWER = 200,
	/// <summary>
	///    Meters to the fourth power	(m^4)
	/// </summary>
	DUT_METERS_TO_THE_FOURTH_POWER = 201,
	/// <summary>
	///    feet to the sixth power	(ft^6)
	/// </summary>
	DUT_FEET_TO_THE_SIXTH_POWER = 202,
	/// <summary>
	///    inches to the sixth power	(in^6)
	/// </summary>
	DUT_INCHES_TO_THE_SIXTH_POWER = 203,
	/// <summary>
	///    millimeters to the sixth power	(mm^6)
	/// </summary>
	DUT_MILLIMETERS_TO_THE_SIXTH_POWER = 204,
	/// <summary>
	///    centimeters to the sixth power	(cm^6)
	/// </summary>
	DUT_CENTIMETERS_TO_THE_SIXTH_POWER = 205,
	/// <summary>
	///    meters to the sixth power	(m^6)
	/// </summary>
	DUT_METERS_TO_THE_SIXTH_POWER = 206,
	/// <summary>
	///    square feet per foot	(ftÂ²/ft)
	/// </summary>
	DUT_SQUARE_FEET_PER_FOOT = 207,
	/// <summary>
	///    square inches per foot	(inÂ²/ft)
	/// </summary>
	DUT_SQUARE_INCHES_PER_FOOT = 208,
	/// <summary>
	///    square millimeters per meter	(mmÂ²/m)
	/// </summary>
	DUT_SQUARE_MILLIMETERS_PER_METER = 209,
	/// <summary>
	///    square centimeters per meter	(cmÂ²/m)
	/// </summary>
	DUT_SQUARE_CENTIMETERS_PER_METER = 210,
	/// <summary>
	///    square meters per meter	(mÂ²/m)
	/// </summary>
	DUT_SQUARE_METERS_PER_METER = 211,
	/// <summary>
	///    kilograms per meter (kg/m)
	/// </summary>
	DUT_KILOGRAMS_MASS_PER_METER = 212,
	/// <summary>
	///    pounds per foot (lb/ft)
	/// </summary>
	DUT_POUNDS_MASS_PER_FOOT = 213,
	/// <summary>
	///    radians
	/// </summary>
	DUT_RADIANS = 214,
	/// <summary>
	///    grads
	/// </summary>
	DUT_GRADS = 215,
	/// <summary>
	///    radians per second
	/// </summary>
	DUT_RADIANS_PER_SECOND = 216,
	/// <summary>
	///    millisecond
	/// </summary>
	DUT_MILISECONDS = 217,
	/// <summary>
	///    second
	/// </summary>
	DUT_SECONDS = 218,
	/// <summary>
	///    minutes
	/// </summary>
	DUT_MINUTES = 219,
	/// <summary>
	///    hours
	/// </summary>
	DUT_HOURS = 220,
	/// <summary>
	///    kilometers per hour
	/// </summary>
	DUT_KILOMETERS_PER_HOUR = 221,
	/// <summary>
	///    miles per hour
	/// </summary>
	DUT_MILES_PER_HOUR = 222,
	/// <summary>
	///    kilojoules
	/// </summary>
	DUT_KILOJOULES = 223,
	/// <summary>
	///    kilograms per square meter (kg/mÂ²)
	/// </summary>
	DUT_KILOGRAMS_MASS_PER_SQUARE_METER = 224,
	/// <summary>
	///    pounds per square foot (lb/ftÂ²)
	/// </summary>
	DUT_POUNDS_MASS_PER_SQUARE_FOOT = 225,
	/// <summary>
	///    Watts per meter kelvin (W/(mÂ·K))
	/// </summary>
	DUT_WATTS_PER_METER_KELVIN = 226,
	/// <summary>
	///    Joules per gram Celsius (J/(gÂ·Â°C))
	/// </summary>
	DUT_JOULES_PER_GRAM_CELSIUS = 227,
	/// <summary>
	///    Joules per gram (J/g)
	/// </summary>
	DUT_JOULES_PER_GRAM = 228,
	/// <summary>
	///    Nanograms per pascal second square meter (ng/(PaÂ·sÂ·mÂ²))
	/// </summary>
	DUT_NANOGRAMS_PER_PASCAL_SECOND_SQUARE_METER = 229,
	/// <summary>
	///    Ohm meters
	/// </summary>
	DUT_OHM_METERS = 230,
	/// <summary>
	///    BTU per hour foot Fahrenheit (BTU/(hÂ·ftÂ·Â°F))
	/// </summary>
	DUT_BRITISH_THERMAL_UNITS_PER_HOUR_FOOT_FAHRENHEIT = 231,
	/// <summary>
	///    BTU per pound Fahrenheit (BTU/(lbÂ·Â°F))
	/// </summary>
	DUT_BRITISH_THERMAL_UNITS_PER_POUND_FAHRENHEIT = 232,
	/// <summary>
	///    BTU per pound (BTU/lb)
	/// </summary>
	DUT_BRITISH_THERMAL_UNITS_PER_POUND = 233,
	/// <summary>
	///    Grains per hour square foot inch mercury (gr/(hÂ·ftÂ²Â·inHg))
	/// </summary>
	DUT_GRAINS_PER_HOUR_SQUARE_FOOT_INCH_MERCURY = 234,
	/// <summary>
	///    Per mille or per thousand(â€°)
	/// </summary>
	DUT_PER_MILLE = 235,
	/// <summary>
	///    Decimeters
	/// </summary>
	DUT_DECIMETERS = 236,
	/// <summary>
	///    Joules per kilogram Celsius (J/(kgÂ·Â°C))
	/// </summary>
	DUT_JOULES_PER_KILOGRAM_CELSIUS = 237,
	/// <summary>
	///    Micrometers per meter Celsius (um/(mÂ·Â°C))
	/// </summary>
	DUT_MICROMETERS_PER_METER_CELSIUS = 238,
	/// <summary>
	///    Microinches per inch Fahrenheit (uin/(inÂ·Â°F))
	/// </summary>
	DUT_MICROINCHES_PER_INCH_FAHRENHEIT = 239,
	/// <summary>
	///    US tonnes (T, Tons, ST)
	/// </summary>
	DUT_USTONNES_MASS = 240,
	/// <summary>
	///    US tonnes (Tonsf, STf)
	/// </summary>
	DUT_USTONNES_FORCE = 241,
	/// <summary>
	///    liters per minute (L/min)
	/// </summary>
	DUT_LITERS_PER_MINUTE = 242,
	/// <summary>
	///    degree Fahrenheit difference (delta Â°F)
	/// </summary>
	DUT_FAHRENHEIT_DIFFERENCE = 243,
	/// <summary>
	///    degree Celsius difference (delta Â°C)
	/// </summary>
	DUT_CELSIUS_DIFFERENCE = 244,
	/// <summary>
	///    kelvin difference (delta K)
	/// </summary>
	DUT_KELVIN_DIFFERENCE = 245,
	/// <summary>
	///    degree Rankine difference (delta Â°R)
	/// </summary>
	DUT_RANKINE_DIFFERENCE = 246
}
