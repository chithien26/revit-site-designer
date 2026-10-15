using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

// Convert the add-in's persisted legacy settings at the Revit API boundary.
public static class LegacyUnits
{
    private static readonly Dictionary<DisplayUnitType, ForgeTypeId> UnitsMap = new Dictionary<DisplayUnitType, ForgeTypeId>
    {
        {DisplayUnitType.DUT_METERS, UnitTypeId.Meters},
        {DisplayUnitType.DUT_CENTIMETERS, UnitTypeId.Centimeters},
        {DisplayUnitType.DUT_MILLIMETERS, UnitTypeId.Millimeters},
        {DisplayUnitType.DUT_DECIMAL_FEET, UnitTypeId.Feet},
        {DisplayUnitType.DUT_DECIMAL_INCHES, UnitTypeId.Inches},
        {DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES, UnitTypeId.FeetFractionalInches},
        {DisplayUnitType.DUT_FRACTIONAL_INCHES, UnitTypeId.FractionalInches},
        {DisplayUnitType.DUT_METERS_CENTIMETERS, UnitTypeId.MetersCentimeters},
        {DisplayUnitType.DUT_DECIMAL_DEGREES, UnitTypeId.Degrees},
        {DisplayUnitType.DUT_DEGREES_AND_MINUTES, UnitTypeId.DegreesMinutes},
        {DisplayUnitType.DUT_ACRES, UnitTypeId.Acres},
        {DisplayUnitType.DUT_HECTARES, UnitTypeId.Hectares},
        {DisplayUnitType.DUT_SQUARE_FEET, UnitTypeId.SquareFeet},
        {DisplayUnitType.DUT_SQUARE_INCHES, UnitTypeId.SquareInches},
        {DisplayUnitType.DUT_SQUARE_METERS, UnitTypeId.SquareMeters},
        {DisplayUnitType.DUT_SQUARE_CENTIMETERS, UnitTypeId.SquareCentimeters},
        {DisplayUnitType.DUT_SQUARE_MILLIMETERS, UnitTypeId.SquareMillimeters},
        {DisplayUnitType.DUT_CUBIC_FEET, UnitTypeId.CubicFeet},
        {DisplayUnitType.DUT_CUBIC_INCHES, UnitTypeId.CubicInches},
        {DisplayUnitType.DUT_CUBIC_YARDS, UnitTypeId.CubicYards},
        {DisplayUnitType.DUT_CUBIC_METERS, UnitTypeId.CubicMeters},
        {DisplayUnitType.DUT_CUBIC_CENTIMETERS, UnitTypeId.CubicCentimeters},
        {DisplayUnitType.DUT_CUBIC_MILLIMETERS, UnitTypeId.CubicMillimeters},
        {DisplayUnitType.DUT_LITERS, UnitTypeId.Liters},
        {DisplayUnitType.DUT_GALLONS_US, UnitTypeId.UsGallons},
        {DisplayUnitType.DUT_PERCENTAGE, UnitTypeId.Percentage},
        {DisplayUnitType.DUT_RATIO_10, UnitTypeId.RatioTo10},
        {DisplayUnitType.DUT_RATIO_12, UnitTypeId.RatioTo12},
        {DisplayUnitType.DUT_SLOPE_DEGREES, UnitTypeId.SlopeDegrees},
        {DisplayUnitType.DUT_RISE_OVER_FOOT, UnitTypeId.RiseDividedBy1Foot},
        {DisplayUnitType.DUT_RISE_OVER_INCHES, UnitTypeId.RiseDividedBy12Inches},
        {DisplayUnitType.DUT_RISE_OVER_MMS, UnitTypeId.RiseDividedBy1000Millimeters}
    };

    public static ForgeTypeId Spec(UnitType type)
    {
        switch (type)
        {
            case UnitType.UT_Length: return SpecTypeId.Length;
            case UnitType.UT_Area: return SpecTypeId.Area;
            case UnitType.UT_Volume: return SpecTypeId.Volume;
            case UnitType.UT_Angle: return SpecTypeId.Angle;
            case UnitType.UT_Slope: return SpecTypeId.Slope;
            default: throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported legacy specification.");
        }
    }

    public static ForgeTypeId Unit(DisplayUnitType type) => UnitsMap.TryGetValue(type, out var id)
        ? id : throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported legacy display unit.");

    public static FormatOptions GetFormatOptions(this Units units, UnitType type) => units.GetFormatOptions(Spec(type));
    public static void SetFormatOptions(this Units units, UnitType type, FormatOptions options) => units.SetFormatOptions(Spec(type), options);
    public static DisplayUnitType LegacyDisplayUnits(this FormatOptions options)
    {
        foreach (var pair in UnitsMap)
            if (pair.Value == options.GetUnitTypeId()) return pair.Key;
        throw new NotSupportedException("Unsupported display unit: " + options.GetUnitTypeId().TypeId);
    }
    public static void SetLegacyDisplayUnits(this FormatOptions options, DisplayUnitType type) => options.SetUnitTypeId(Unit(type));
    public static UnitSymbolType LegacyUnitSymbol(this FormatOptions options)
    {
        var symbol = options.GetSymbolTypeId();
        if (symbol == SymbolTypeId.Percent) return UnitSymbolType.UST_PERCENT_SIGN;
        if (symbol == SymbolTypeId.Degree) return UnitSymbolType.UST_DEGREE_SYMBOL;
        if (symbol == SymbolTypeId.SlopeDegree) return UnitSymbolType.UST_SLOPE_DEGREE_SYMBOL;
        if (symbol == SymbolTypeId.Colon10) return UnitSymbolType.UST_COLON_10;
        if (symbol == SymbolTypeId.Colon12) return UnitSymbolType.UST_COLON_12;
        return UnitSymbolType.UST_NONE;
    }
    public static void SetLegacyUnitSymbol(this FormatOptions options, UnitSymbolType type)
    {
        ForgeTypeId symbol = new ForgeTypeId();
        if (type == UnitSymbolType.UST_PERCENT_SIGN) symbol = SymbolTypeId.Percent;
        else if (type == UnitSymbolType.UST_DEGREE_SYMBOL) symbol = SymbolTypeId.Degree;
        else if (type == UnitSymbolType.UST_SLOPE_DEGREE_SYMBOL) symbol = SymbolTypeId.SlopeDegree;
        else if (type == UnitSymbolType.UST_COLON_10) symbol = SymbolTypeId.Colon10;
        else if (type == UnitSymbolType.UST_COLON_12) symbol = SymbolTypeId.Colon12;
        if (FormatOptions.GetValidSymbols(options.GetUnitTypeId()).Contains(symbol)) options.SetSymbolTypeId(symbol);
    }
    public static FormatOptions CreateFormatOptions(DisplayUnitType type, UnitSymbolType symbol)
    {
        var options = new FormatOptions(Unit(type));
        options.SetLegacyUnitSymbol(symbol);
        return options;
    }
    public static bool TryParse(Units units, UnitType type, string text, out double value) => UnitFormatUtils.TryParse(units, Spec(type), text, out value);
    public static string Format(Units units, UnitType type, double value, bool maxAccuracy, bool forEditing, FormatValueOptions options) => UnitFormatUtils.Format(units, Spec(type), value, forEditing, options);

    public static ForgeTypeId ParameterSpec(ParameterType type)
    {
        switch (type)
        {
            case ParameterType.Text: return SpecTypeId.String.Text;
            case ParameterType.Integer: return SpecTypeId.Int.Integer;
            case ParameterType.YesNo: return SpecTypeId.Boolean.YesNo;
            case ParameterType.Length: return SpecTypeId.Length;
            case ParameterType.Number: return SpecTypeId.Number;
            case ParameterType.Angle: return SpecTypeId.Angle;
            case ParameterType.Slope: return SpecTypeId.Slope;
            case ParameterType.Material: return SpecTypeId.Reference.Material;
            case ParameterType.Currency: return SpecTypeId.Currency;
            default: throw new NotSupportedException("Unsupported family parameter type: " + type);
        }
    }
    public static ParameterType LegacyParameterType(this Definition definition)
    {
        foreach (ParameterType type in new[] { ParameterType.Text, ParameterType.Integer, ParameterType.YesNo, ParameterType.Length, ParameterType.Number, ParameterType.Angle, ParameterType.Slope, ParameterType.Material, ParameterType.Currency })
            if (definition.GetDataType() == ParameterSpec(type)) return type;
        return ParameterType.Invalid;
    }
    public static ForgeTypeId Group(BuiltInParameterGroup group)
    {
        switch (group)
        {
            case BuiltInParameterGroup.PG_DATA: return GroupTypeId.Data;
            case BuiltInParameterGroup.PG_GEOMETRY: return GroupTypeId.Geometry;
            case BuiltInParameterGroup.PG_IDENTITY_DATA: return GroupTypeId.IdentityData;
            case BuiltInParameterGroup.PG_MATERIALS: return GroupTypeId.Materials;
            case BuiltInParameterGroup.INVALID: return new ForgeTypeId();
            default: throw new NotSupportedException("Unsupported parameter group: " + group);
        }
    }
    public static FamilyParameter AddParameter(this FamilyManager manager, string name, BuiltInParameterGroup group, ParameterType type, bool isInstance) => manager.AddParameter(name, Group(group), ParameterSpec(type), isInstance);
    public static FamilyParameter AddParameter(this FamilyManager manager, ExternalDefinition definition, BuiltInParameterGroup group, bool isInstance) => manager.AddParameter(definition, Group(group), isInstance);
    public static bool ReInsert(this BindingMap map, Definition definition, Binding binding, BuiltInParameterGroup group) => map.ReInsert(definition, binding, Group(group));
    public static bool Insert(this BindingMap map, Definition definition, Binding binding, BuiltInParameterGroup group) => map.Insert(definition, binding, Group(group));
    public static DisplayUnitType LegacyDisplayUnit(this FamilyParameter parameter)
    {
        foreach (var pair in UnitsMap) if (pair.Value == parameter.GetUnitTypeId()) return pair.Key;
        return DisplayUnitType.DUT_UNDEFINED;
    }
    public static FamilyParameter AddParameter(this FamilyManager manager, string name, BuiltInParameterGroup group, Category category, bool isInstance) => manager.AddParameter(name, Group(group), category, isInstance);
    public static ExternalDefinitionCreationOptions DefinitionOptions(string name, ParameterType type) => new ExternalDefinitionCreationOptions(name, type == ParameterType.FamilyType ? Category.GetBuiltInCategoryTypeId(BuiltInCategory.OST_Planting) : ParameterSpec(type));
}
