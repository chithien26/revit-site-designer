namespace Autodesk.Revit.DB;

/// <summary>
///    The type of physical quantity to be measured, for example length or force.
/// </summary>
/// <remarks>
///   <para>Some unit types represent fundamental physical quantities to
///    be measured, for example UT_Length, UT_Area, UT_Mass, or
///    UT_Force.</para>
///   <para>Other unit types exist to provide an alternate set of unit
///    settings for a physical quantity.  For example, UT_HVAC_DuctSize is
///    used for measuring sizes of ducts, UT_PipeSize is used for
///    measuring sizes of pipes, and UT_SheetLength is used for measuring
///    distances on a piece of paper.  All of those are lengths and could
///    use UT_Length, but they are typically measured using different
///    units or to a different degree of accuracy compared with typical
///    lengths in a building.  By using different unit types, the Units
///    class can have different default settings for measuring values of
///    the same physical quantity.</para>
/// </remarks>
public enum UnitType
{
	/// <summary>
	///    Undefined unit value
	/// </summary>
	UT_Undefined = -2,
	/// <summary>
	///    A custom unit value
	/// </summary>
	UT_Custom,
	/// <summary>
	///    Length, e.g. ft, in, m, mm
	/// </summary>
	UT_Length,
	/// <summary>
	///    Area, e.g. ftÂ², inÂ², mÂ², mmÂ²
	/// </summary>
	UT_Area,
	/// <summary>
	///    Volume, e.g. ftÂ³, inÂ³, mÂ³, mmÂ³
	/// </summary>
	UT_Volume,
	/// <summary>
	///    Angular measurement, e.g. radians, degrees
	/// </summary>
	UT_Angle,
	/// <summary>
	///    General format unit, appropriate for general counts or percentages
	/// </summary>
	UT_Number,
	/// <summary>
	///    Sheet length
	/// </summary>
	UT_SheetLength,
	/// <summary>
	///    Site angle
	/// </summary>
	UT_SiteAngle,
	/// <summary>
	///    Density (HVAC) e.g.	kg/mÂ³
	/// </summary>
	UT_HVAC_Density,
	/// <summary>
	///    Energy (HVAC) e.g.	(mÂ² Â· kg)/sÂ², J
	/// </summary>
	UT_HVAC_Energy,
	/// <summary>
	///    Friction (HVAC) e.g. kg/(mÂ² Â· sÂ²), Pa/m
	/// </summary>
	UT_HVAC_Friction,
	/// <summary>
	///    Power (HVAC) e.g. (mÂ² Â· kg)/sÂ³, W
	/// </summary>
	UT_HVAC_Power,
	/// <summary>
	///    Power Density (HVAC), e.g. kg/sÂ³, W/mÂ²
	/// </summary>
	UT_HVAC_Power_Density,
	/// <summary>
	///    Pressure (HVAC) e.g. kg/(m Â· sÂ²), Pa
	/// </summary>
	UT_HVAC_Pressure,
	/// <summary>
	///    Temperature (HVAC) e.g. K, C, F
	/// </summary>
	UT_HVAC_Temperature,
	/// <summary>
	///    Velocity (HVAC) e.g. m/s
	/// </summary>
	UT_HVAC_Velocity,
	/// <summary>
	///    Air Flow (HVAC) e.g. mÂ³/s
	/// </summary>
	UT_HVAC_Airflow,
	/// <summary>
	///    Duct Size (HVAC) e.g. mm, in
	/// </summary>
	UT_HVAC_DuctSize,
	/// <summary>
	///    Cross Section (HVAC) e.g. mmÂ², inÂ²
	/// </summary>
	UT_HVAC_CrossSection,
	/// <summary>
	///    Heat Gain (HVAC) e.g. (mÂ² Â· kg)/sÂ³, W
	/// </summary>
	UT_HVAC_HeatGain,
	/// <summary>
	///    Current (Electrical) e.g. A
	/// </summary>
	UT_Electrical_Current,
	/// <summary>
	///    Electrical Potential e.g.	(mÂ² Â· kg) / (sÂ³Â· A), V
	/// </summary>
	UT_Electrical_Potential,
	/// <summary>
	///    Frequency (Electrical) e.g. 1/s, Hz
	/// </summary>
	UT_Electrical_Frequency,
	/// <summary>
	///    Illuminance (Electrical) e.g. (cd Â· sr)/mÂ², lm/mÂ²
	/// </summary>
	UT_Electrical_Illuminance,
	/// <summary>
	///    Luminous Flux (Electrical) e.g. cd Â· sr, lm
	/// </summary>
	UT_Electrical_Luminous_Flux,
	/// <summary>
	///    Power (Electrical) e.g.	(mÂ² Â· kg)/sÂ³, W
	/// </summary>
	UT_Electrical_Power,
	/// <summary>
	///    Roughness factor (HVAC) e,g. ft, in, mm
	/// </summary>
	UT_HVAC_Roughness,
	/// <summary>
	///    Force, e.g. (kg Â· m)/sÂ², N
	/// </summary>
	UT_Force,
	/// <summary>
	///    Force per unit length, e.g. kg/sÂ², N/m
	/// </summary>
	UT_LinearForce,
	/// <summary>
	///    Force per unit area, e.g. kg/(m Â· sÂ²), N/mÂ²
	/// </summary>
	UT_AreaForce,
	/// <summary>
	///    Moment, e.g. (kg Â· mÂ²)/sÂ², N Â· m
	/// </summary>
	UT_Moment,
	/// <summary>
	///    Force scale, e.g. m / N
	/// </summary>
	UT_ForceScale,
	/// <summary>
	///    Linear force scale, e.g. mÂ² / N
	/// </summary>
	UT_LinearForceScale,
	/// <summary>
	///    Area force scale, e.g. mÂ³ / N
	/// </summary>
	UT_AreaForceScale,
	/// <summary>
	///    Moment scale, e.g. 1 / N
	/// </summary>
	UT_MomentScale,
	/// <summary>
	///    Apparent Power (Electrical), e.g. (mÂ² Â· kg)/sÂ³, W
	/// </summary>
	UT_Electrical_Apparent_Power,
	/// <summary>
	///    Power Density (Electrical), e.g. kg/sÂ³, W/mÂ²
	/// </summary>
	UT_Electrical_Power_Density,
	/// <summary>
	///    Density (Piping) e.g. kg/mÂ³
	/// </summary>
	UT_Piping_Density,
	/// <summary>
	///    Flow (Piping), e.g. mÂ³/s
	/// </summary>
	UT_Piping_Flow,
	/// <summary>
	///    Friction (Piping), e.g. kg/(mÂ² Â· sÂ²), Pa/m
	/// </summary>
	UT_Piping_Friction,
	/// <summary>
	///    Pressure (Piping), e.g. kg/(m Â· sÂ²), Pa
	/// </summary>
	UT_Piping_Pressure,
	/// <summary>
	///    Temperature (Piping), e.g. K
	/// </summary>
	UT_Piping_Temperature,
	/// <summary>
	///    Velocity (Piping), e.g. m/s
	/// </summary>
	UT_Piping_Velocity,
	/// <summary>
	///    Dynamic Viscosity (Piping), e.g. kg/(m Â· s), Pa Â· s
	/// </summary>
	UT_Piping_Viscosity,
	/// <summary>
	///    Pipe Size (Piping), e.g.	m
	/// </summary>
	UT_PipeSize,
	/// <summary>
	///    Roughness factor (Piping), e.g. ft, in, mm
	/// </summary>
	UT_Piping_Roughness,
	/// <summary>
	///    Stress, e.g. kg/(m Â· sÂ²), ksi, MPa
	/// </summary>
	UT_Stress,
	/// <summary>
	///    Unit weight, e.g. N/mÂ³
	/// </summary>
	UT_UnitWeight,
	/// <summary>
	///    Thermal expansion, e.g. 1/K
	/// </summary>
	UT_ThermalExpansion,
	/// <summary>
	///    Linear moment, e,g. (N Â· m)/m, lbf / ft
	/// </summary>
	UT_LinearMoment,
	/// <summary>
	///    Linear moment scale, e.g. ft/kip, m/kN
	/// </summary>
	UT_LinearMomentScale,
	/// <summary>
	///    Point Spring Coefficient, e.g. kg/sÂ², N/m
	/// </summary>
	UT_ForcePerLength,
	/// <summary>
	///    Rotational Point Spring Coefficient, e.g. (kg Â· mÂ²)/(sÂ² Â· rad), (N Â· m)/rad
	/// </summary>
	UT_ForceLengthPerAngle,
	/// <summary>
	///    Line Spring Coefficient, e.g. kg/(m Â· sÂ²), (N Â· m)/mÂ²
	/// </summary>
	UT_LinearForcePerLength,
	/// <summary>
	///    Rotational Line Spring Coefficient, e.g. (kg Â· m)/(sÂ² Â· rad), N/rad
	/// </summary>
	UT_LinearForceLengthPerAngle,
	/// <summary>
	///    Area Spring Coefficient, e.g.  kg/(mÂ² Â· sÂ²), N/mÂ³
	/// </summary>
	UT_AreaForcePerLength,
	/// <summary>
	///    Pipe Volume, e.g. gallons, liters
	/// </summary>
	UT_Piping_Volume,
	/// <summary>
	///    Dynamic Viscosity (HVAC), e.g. kg/(m Â· s), Pa Â· s
	/// </summary>
	UT_HVAC_Viscosity,
	/// <summary>
	///    Coefficient of Heat Transfer (U-value) (HVAC), e.g. kg/(sÂ³ Â· K), W/(mÂ² Â· K)
	/// </summary>
	UT_HVAC_CoefficientOfHeatTransfer,
	/// <summary>
	///    Air Flow Density (HVAC), mÂ³/(s Â· mÂ²)
	/// </summary>
	UT_HVAC_Airflow_Density,
	/// <summary>
	///    Slope, rise/run
	/// </summary>
	UT_Slope,
	/// <summary>
	///    Cooling load (HVAC), e.g. (mÂ² Â· kg)/sÂ³, W, kW, Btu/s, Btu/h
	/// </summary>
	UT_HVAC_Cooling_Load,
	/// <summary>
	///    Cooling load per unit area (HVAC), e.g. kg/sÂ³, W/mÂ², W/ftÂ², Btu/(hÂ·ftÂ²)
	/// </summary>
	UT_HVAC_Cooling_Load_Divided_By_Area,
	/// <summary>
	///    Cooling load per unit volume (HVAC), e.g. kg/(sÂ³ Â· m), W/mÂ³, Btu/(hÂ·ftÂ³)
	/// </summary>
	UT_HVAC_Cooling_Load_Divided_By_Volume,
	/// <summary>
	///    Heating load (HVAC), e.g. (mÂ² Â· kg)/sÂ³, W, kW, Btu/s, Btu/h
	/// </summary>
	UT_HVAC_Heating_Load,
	/// <summary>
	///    Heating load per unit area (HVAC), e.g. kg/sÂ³, W/mÂ², W/ftÂ², Btu/(hÂ·ftÂ²)
	/// </summary>
	UT_HVAC_Heating_Load_Divided_By_Area,
	/// <summary>
	///    Heating load per unit volume (HVAC), e.g. kg/(sÂ³ Â· m), W/mÂ³, Btu/(hÂ·ftÂ³)
	/// </summary>
	UT_HVAC_Heating_Load_Divided_By_Volume,
	/// <summary>
	///    Airflow per unit volume (HVAC), e.g. mÂ³/(s Â· mÂ³), CFM/ftÂ³, CFM/CF, L/(sÂ·mÂ³)
	/// </summary>
	UT_HVAC_Airflow_Divided_By_Volume,
	/// <summary>
	///    Airflow per unit cooling load (HVAC), e.g. (m Â· sÂ²)/kg, ftÂ²/ton, SF/ton, mÂ²/kW
	/// </summary>
	UT_HVAC_Airflow_Divided_By_Cooling_Load,
	/// <summary>
	///    Area per unit cooling load (HVAC), e.g.  sÂ³/kg, ftÂ²/ton, mÂ²/kW
	/// </summary>
	UT_HVAC_Area_Divided_By_Cooling_Load,
	/// <summary>
	///    Wire Size (Electrical), e.g.	mm, inch
	/// </summary>
	UT_WireSize,
	/// <summary>
	///    Slope (HVAC)
	/// </summary>
	UT_HVAC_Slope,
	/// <summary>
	///    Slope (Piping)
	/// </summary>
	UT_Piping_Slope,
	/// <summary>
	///    Currency
	/// </summary>
	UT_Currency,
	/// <summary>
	///    Electrical efficacy (lighting), e.g. cdÂ·srÂ·sÂ³/(mÂ²Â·kg), lm/W
	/// </summary>
	UT_Electrical_Efficacy,
	/// <summary>
	///    Wattage (lighting), e.g. (mÂ² Â· kg)/sÂ³, W
	/// </summary>
	UT_Electrical_Wattage,
	/// <summary>
	///    Color temperature (lighting), e.g. K
	/// </summary>
	UT_Color_Temperature,
	/// <summary>
	///    Sheet length in decimal form, decimal inches, mm
	/// </summary>
	UT_DecSheetLength,
	/// <summary>
	///    Luminous Intensity (Lighting), e.g. cd, cd
	/// </summary>
	UT_Electrical_Luminous_Intensity,
	/// <summary>
	///    Luminance (Lighting), cd/mÂ², cd/mÂ²
	/// </summary>
	UT_Electrical_Luminance,
	/// <summary>
	///    Area per unit heating load (HVAC), e.g.  sÂ³/kg, ftÂ²/ton, mÂ²/kW
	/// </summary>
	UT_HVAC_Area_Divided_By_Heating_Load,
	/// <summary>
	///    Heating and coooling factor, percentage
	/// </summary>
	UT_HVAC_Factor,
	/// <summary>
	///    Temperature (electrical), e.g. F, C
	/// </summary>
	UT_Electrical_Temperature,
	/// <summary>
	///    Cable tray size (electrical), e.g. in, mm
	/// </summary>
	UT_Electrical_CableTraySize,
	/// <summary>
	///    Conduit size (electrical), e.g. in, mm
	/// </summary>
	UT_Electrical_ConduitSize,
	/// <summary>
	///    Structural reinforcement volume, e.g. inÂ³, cmÂ³
	/// </summary>
	UT_Reinforcement_Volume,
	/// <summary>
	///    Structural reinforcement length, e.g. mm, in, ft
	/// </summary>
	UT_Reinforcement_Length,
	/// <summary>
	///    Electrical demand factor, percentage
	/// </summary>
	UT_Electrical_Demand_Factor,
	/// <summary>
	///    Duct Insulation Thickness (HVAC), e.g. mm, in
	/// </summary>
	UT_HVAC_DuctInsulationThickness,
	/// <summary>
	///    Duct Lining Thickness (HVAC), e.g. mm, in
	/// </summary>
	UT_HVAC_DuctLiningThickness,
	/// <summary>
	///    Pipe Insulation Thickness (Piping), e.g. mm, in
	/// </summary>
	UT_PipeInsulationThickness,
	/// <summary>
	///    Thermal Resistance (HVAC), R Value, e.g. mÂ²Â·K/W
	/// </summary>
	UT_HVAC_ThermalResistance,
	/// <summary>
	///    Thermal Mass (HVAC), e.g.  J/K, BTU/F
	/// </summary>
	UT_HVAC_ThermalMass,
	/// <summary>
	///    Acceleration, e.g. m/sÂ², km/sÂ², in/sÂ², ft/sÂ², mi/sÂ²
	/// </summary>
	UT_Acceleration,
	/// <summary>
	///    Bar Diameter, e.g. ', LF, ", m, cm, mm
	/// </summary>
	UT_Bar_Diameter,
	/// <summary>
	///    Crack Width, e.g. ', LF, ", m, cm, mm
	/// </summary>
	UT_Crack_Width,
	/// <summary>
	///    Displacement/Deflection, e.g. ', LF, ", m, cm, mm
	/// </summary>
	UT_Displacement_Deflection,
	/// <summary>
	///    Energy, e.g. J, kJ, kgf-m, lb-ft, N-m
	/// </summary>
	UT_Energy,
	/// <summary>
	///    FREQUENCY, Frequency (Structural) e.g. Hz
	/// </summary>
	UT_Structural_Frequency,
	/// <summary>
	///    Mass, e.g.  kg, lb, t
	/// </summary>
	UT_Mass,
	/// <summary>
	///    Mass per Unit Length, e.g. kg/m, lb/ft
	/// </summary>
	UT_Mass_per_Unit_Length,
	/// <summary>
	///    Moment of Inertia, e.g. ft^4, in^4, mm^4, cm^4, m^4
	/// </summary>
	UT_Moment_of_Inertia,
	/// <summary>
	///    Surface Area, e.g. ftÂ²/ft, mÂ²/m
	/// </summary>
	UT_Surface_Area,
	/// <summary>
	///    Period, e.g. ms, s, min, h
	/// </summary>
	UT_Period,
	/// <summary>
	///    Pulsation, e.g. rad/s
	/// </summary>
	UT_Pulsation,
	/// <summary>
	///    Reinforcement Area, e.g. SF, ftÂ², inÂ², mmÂ², cmÂ², mÂ²
	/// </summary>
	UT_Reinforcement_Area,
	/// <summary>
	///    Reinforcement Area per Unit Length, e.g. ftÂ²/ft, inÂ²/ft, mmÂ²/m, cmÂ²/m, mÂ²/m
	/// </summary>
	UT_Reinforcement_Area_per_Unit_Length,
	/// <summary>
	///    Reinforcement Cover, e.g. ', LF, ", m, cm, mm
	/// </summary>
	UT_Reinforcement_Cover,
	/// <summary>
	///    Reinforcement Spacing, e.g. ', LF, ", m, cm, mm
	/// </summary>
	UT_Reinforcement_Spacing,
	/// <summary>
	///    Rotation, e.g. Â°, rad, grad
	/// </summary>
	UT_Rotation,
	/// <summary>
	///    Section Area, e.g.  ftÂ²/ft, inÂ²/ft, mmÂ²/m, cmÂ²/m, mÂ²/m
	/// </summary>
	UT_Section_Area,
	/// <summary>
	///    Section Dimension, e.g.  ', LF, ", m, cm, mm
	/// </summary>
	UT_Section_Dimension,
	/// <summary>
	///    Section Modulus, e.g. ft^3, in^3, mm^3, cm^3, m^3
	/// </summary>
	UT_Section_Modulus,
	/// <summary>
	///    Section Property, e.g.  ', LF, ", m, cm, mm
	/// </summary>
	UT_Section_Property,
	/// <summary>
	///    Section Property, e.g. km/h, m/s, ft/min, ft/s, mph
	/// </summary>
	UT_Structural_Velocity,
	/// <summary>
	///    Warping Constant, e.g. ft^6, in^6, mm^6, cm^6, m^6
	/// </summary>
	UT_Warping_Constant,
	/// <summary>
	///    Weight, e.g. N, daN, kN, MN, kip, kgf, Tf, lb, lbf
	/// </summary>
	UT_Weight,
	/// <summary>
	///    Weight per Unit Length, e.g. N/m, daN/m, kN/m, MN/m, kip/ft, kgf/m, Tf/m, lb/ft, lbf/ft, kip/in
	/// </summary>
	UT_Weight_per_Unit_Length,
	/// <summary>
	///    Thermal Conductivity (HVAC), e.g. W/(mÂ·K)
	/// </summary>
	UT_HVAC_ThermalConductivity,
	/// <summary>
	///    Specific Heat (HVAC), e.g. J/(gÂ·Â°C)
	/// </summary>
	UT_HVAC_SpecificHeat,
	/// <summary>
	///    Specific Heat of Vaporization, e.g. J/g
	/// </summary>
	UT_HVAC_SpecificHeatOfVaporization,
	/// <summary>
	///    Permeability, e.g. ng/(PaÂ·sÂ·mÂ²)
	/// </summary>
	UT_HVAC_Permeability,
	/// <summary>
	///    Electrical Resistivity, e.g.
	/// </summary>
	UT_Electrical_Resistivity,
	/// <summary>
	///    Mass Density, e.g. kg/mÂ³, lb/ftÂ³
	/// </summary>
	UT_MassDensity,
	/// <summary>
	///    Mass Per Unit Area, e.g. kg/mÂ², lb/ftÂ²
	/// </summary>
	UT_MassPerUnitArea,
	/// <summary>
	///    Length unit for pipe dimension, e.g. in, mm
	/// </summary>
	UT_Pipe_Dimension,
	/// <summary>
	///    Mass, e.g.  kg, lb, t
	/// </summary>
	UT_PipeMass,
	/// <summary>
	///    Mass per Unit Length, e.g. kg/m, lb/ft
	/// </summary>
	UT_PipeMassPerUnitLength,
	/// <summary>
	///    Temperature Difference (HVAC) e.g. C, F, K, R
	/// </summary>
	UT_HVAC_TemperatureDifference,
	/// <summary>
	///    Temperature Difference (Piping), e.g. C, F, K, R
	/// </summary>
	UT_Piping_TemperatureDifference,
	/// <summary>
	///    Temperature Difference (Electrical), e.g. C, F, K, R
	/// </summary>
	UT_Electrical_TemperatureDifference,
	/// <summary>
	///    Interval of time e.g. ms, s, min, h
	/// </summary>
	UT_TimeInterval,
	/// <summary>
	///    Distance interval over time e.g.  m/h etc.
	/// </summary>
	UT_Speed
}
