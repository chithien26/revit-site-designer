namespace Autodesk.Revit.DB;

/// <summary>
///    An enumerated type listing all of the data type interpretation that Autodesk Revit supports.
/// </summary>
public enum ParameterType
{
	/// <summary>
	///    The parameter type is invalid. This value should not be used.
	/// </summary>
	Invalid = 0,
	/// <summary>
	///    The parameter data should be interpreted as a string of text.
	/// </summary>
	Text = 1,
	/// <summary>
	///    The parameter data should be interpreted as a whole number, positive or negative.
	/// </summary>
	Integer = 2,
	/// <summary>
	///    The parameter data should be interpreted as a real number, possibly including decimal points.
	/// </summary>
	Number = 3,
	/// <summary>
	///    The parameter data represents a length.
	/// </summary>
	Length = 4,
	/// <summary>
	///    The parameter data represents an area.
	/// </summary>
	Area = 5,
	/// <summary>
	///    The parameter data represents a volume.
	/// </summary>
	Volume = 6,
	/// <summary>
	///    The parameter data represents an angle.
	/// </summary>
	Angle = 7,
	/// <summary>
	///    A text string that represents a web address.
	/// </summary>
	URL = 8,
	/// <summary>
	///    The value of this property is considered to be a material.
	/// </summary>
	Material = 9,
	/// <summary>
	///    A boolean value that will be represented as Yes or No.
	/// </summary>
	YesNo = 10,
	/// <summary>
	///    The data value will be represented as a force.
	/// </summary>
	Force = 11,
	/// <summary>
	///    The data value will be represented as a linear force.
	/// </summary>
	LinearForce = 12,
	/// <summary>
	///    The data value will be represented as an area force.
	/// </summary>
	AreaForce = 13,
	/// <summary>
	///    The data value will be represented as a moment.
	/// </summary>
	Moment = 14,
	/// <summary>
	///    A parameter value that represents the number of poles, as used in electrical disciplines.
	/// </summary>
	NumberOfPoles = 15,
	/// <summary>
	///    A parameter value that represents the fixture units, as used in piping disciplines.
	/// </summary>
	FixtureUnit = 16,
	/// <summary>
	///    A parameter used to control the type of a family nested within another family.
	/// </summary>
	FamilyType = 17,
	/// <summary>
	///    A parameter value that represents the load classification units, as used in electrical disciplines.
	/// </summary>
	LoadClassification = 18,
	/// <summary>
	///    The value of this parameter is the id of an image.
	/// </summary>
	Image = 19,
	/// <summary>
	///    The value of this parameter will be represented as multiline text.
	/// </summary>
	MultilineText = 20,
	/// <summary>
	///    The data value will be represented as a HVACDensity.
	/// </summary>
	HVACDensity = 107,
	/// <summary>
	///    The data value will be represented as a HVACEnergy.
	/// </summary>
	HVACEnergy = 108,
	/// <summary>
	///    The data value will be represented as a HVACFriction.
	/// </summary>
	HVACFriction = 109,
	/// <summary>
	///    The data value will be represented as a HVACPower.
	/// </summary>
	HVACPower = 110,
	/// <summary>
	///    The data value will be represented as a HVACPowerDensity.
	/// </summary>
	HVACPowerDensity = 111,
	/// <summary>
	///    The data value will be represented as a HVACPressure.
	/// </summary>
	HVACPressure = 112,
	/// <summary>
	///    The data value will be represented as a HVACTemperature.
	/// </summary>
	HVACTemperature = 113,
	/// <summary>
	///    The data value will be represented as a HVACVelocity.
	/// </summary>
	HVACVelocity = 114,
	/// <summary>
	///    The data value will be represented as a HVACAirflow.
	/// </summary>
	HVACAirflow = 115,
	/// <summary>
	///    The data value will be represented as a HVACDuctSize.
	/// </summary>
	HVACDuctSize = 116,
	/// <summary>
	///    The data value will be represented as a HVACCrossSection.
	/// </summary>
	HVACCrossSection = 117,
	/// <summary>
	///    The data value will be represented as a HVACHeatGain.
	/// </summary>
	HVACHeatGain = 118,
	/// <summary>
	///    The data value will be represented as an ElectricalCurrent.
	/// </summary>
	ElectricalCurrent = 119,
	/// <summary>
	///    The data value will be represented as an ElectricalPotential.
	/// </summary>
	ElectricalPotential = 120,
	/// <summary>
	///    The data value will be represented as an ElectricalFrequency.
	/// </summary>
	ElectricalFrequency = 121,
	/// <summary>
	///    The data value will be represented as an ElectricalIlluminance.
	/// </summary>
	ElectricalIlluminance = 122,
	/// <summary>
	///    The data value will be represented as an ElectricalLuminousFlux.
	/// </summary>
	ElectricalLuminousFlux = 123,
	/// <summary>
	///    The data value will be represented as an ElectricalPower.
	/// </summary>
	ElectricalPower = 124,
	/// <summary>
	///    The data value will be represented as a HVACRoughness.
	/// </summary>
	HVACRoughness = 125,
	/// <summary>
	///    The data value will be represented as an ElectricalApparentPower.
	/// </summary>
	ElectricalApparentPower = 134,
	/// <summary>
	///    The data value will be represented as an ElectricalPowerDensity.
	/// </summary>
	ElectricalPowerDensity = 135,
	/// <summary>
	///    The data value will be represented as a PipingDensity.
	/// </summary>
	PipingDensity = 136,
	/// <summary>
	///    The data value will be represented as a PipingFlow.
	/// </summary>
	PipingFlow = 137,
	/// <summary>
	///    The data value will be represented as a PipingFriction.
	/// </summary>
	PipingFriction = 138,
	/// <summary>
	///    The data value will be represented as a PipingPressure.
	/// </summary>
	PipingPressure = 139,
	/// <summary>
	///    The data value will be represented as a PipingTemperature.
	/// </summary>
	PipingTemperature = 140,
	/// <summary>
	///    The data value will be represented as a PipingVelocity.
	/// </summary>
	PipingVelocity = 141,
	/// <summary>
	///    The data value will be represented as a PipingViscosity.
	/// </summary>
	PipingViscosity = 142,
	/// <summary>
	///    The data value will be represented as a PipeSize.
	/// </summary>
	PipeSize = 143,
	/// <summary>
	///    The data value will be represented as a PipingRoughness.
	/// </summary>
	PipingRoughness = 144,
	/// <summary>
	///    The data value will be represented as a Stress.
	/// </summary>
	Stress = 145,
	/// <summary>
	///    The data value will be represented as a UnitWeight.
	/// </summary>
	UnitWeight = 146,
	/// <summary>
	///    The data value will be represented as a ThermalExpansion.
	/// </summary>
	ThermalExpansion = 147,
	/// <summary>
	///    The data value will be represented as a LinearMoment.
	/// </summary>
	LinearMoment = 148,
	/// <summary>
	///    The data value will be represented as a ForcePerLength.
	/// </summary>
	ForcePerLength = 150,
	/// <summary>
	///    The data value will be represented as a ForceLengthPerAngle.
	/// </summary>
	ForceLengthPerAngle = 151,
	/// <summary>
	///    The data value will be represented as a LinearForcePerLength.
	/// </summary>
	LinearForcePerLength = 152,
	/// <summary>
	///    The data value will be represented as a LinearForceLengthPerAngle.
	/// </summary>
	LinearForceLengthPerAngle = 153,
	/// <summary>
	///    The data value will be represented as an AreaForcePerLength.
	/// </summary>
	AreaForcePerLength = 154,
	/// <summary>
	///    The data value will be represented as a PipingVolume.
	/// </summary>
	PipingVolume = 155,
	/// <summary>
	///    The data value will be represented as a HVACViscosity.
	/// </summary>
	HVACViscosity = 156,
	/// <summary>
	///    The data value will be represented as a HVACCoefficientOfHeatTransfer.
	/// </summary>
	HVACCoefficientOfHeatTransfer = 157,
	/// <summary>
	///    The data value will be represented as a HVACAirflowDensity.
	/// </summary>
	HVACAirflowDensity = 158,
	/// <summary>
	///    The data value will be represented as a Slope.
	/// </summary>
	Slope = 159,
	/// <summary>
	///    The data value will be represented as a HVACCoolingLoad.
	/// </summary>
	HVACCoolingLoad = 160,
	/// <summary>
	///    The data value will be represented as a HVACCoolingLoadDividedByArea.
	/// </summary>
	HVACCoolingLoadDividedByArea = 161,
	/// <summary>
	///    The data value will be represented as a HVACCoolingLoadDividedByVolume.
	/// </summary>
	HVACCoolingLoadDividedByVolume = 162,
	/// <summary>
	///    The data value will be represented as a HVACHeatingLoad.
	/// </summary>
	HVACHeatingLoad = 163,
	/// <summary>
	///    The data value will be represented as a HVACHeatingLoadDividedByArea.
	/// </summary>
	HVACHeatingLoadDividedByArea = 164,
	/// <summary>
	///    The data value will be represented as a HVACHeatingLoadDividedByVolume.
	/// </summary>
	HVACHeatingLoadDividedByVolume = 165,
	/// <summary>
	///    The data value will be represented as a HVACAirflowDividedByVolume.
	/// </summary>
	HVACAirflowDividedByVolume = 166,
	/// <summary>
	///    The data value will be represented as a HVACAirflowDividedByCoolingLoad.
	/// </summary>
	HVACAirflowDividedByCoolingLoad = 167,
	/// <summary>
	///    The data value will be represented as a HVACAreaDividedByCoolingLoad.
	/// </summary>
	HVACAreaDividedByCoolingLoad = 168,
	/// <summary>
	///    The data value will be represented as a WireSize.
	/// </summary>
	WireSize = 169,
	/// <summary>
	///    The data value will be represented as a HVACSlope.
	/// </summary>
	HVACSlope = 170,
	/// <summary>
	///    The data value will be represented as a PipingSlope.
	/// </summary>
	PipingSlope = 171,
	/// <summary>
	///    The data value will be represented as a Currency.
	/// </summary>
	Currency = 172,
	/// <summary>
	///    The data value will be represented as an ElectricalEfficacy.
	/// </summary>
	ElectricalEfficacy = 173,
	/// <summary>
	///    The data value will be represented as an ElectricalWattage.
	/// </summary>
	ElectricalWattage = 174,
	/// <summary>
	///    The data value will be represented as a ColorTemperature.
	/// </summary>
	ColorTemperature = 175,
	/// <summary>
	///    The data value will be represented as an ElectricalLuminousIntensity.
	/// </summary>
	ElectricalLuminousIntensity = 177,
	/// <summary>
	///    The data value will be represented as an ElectricalLuminance.
	/// </summary>
	ElectricalLuminance = 178,
	/// <summary>
	///    The data value will be represented as a HVACAreaDividedByHeatingLoad.
	/// </summary>
	HVACAreaDividedByHeatingLoad = 179,
	/// <summary>
	///    The data value will be represented as a HVACFactor.
	/// </summary>
	HVACFactor = 180,
	/// <summary>
	///    The data value will be represented as a ElectricalTemperature.
	/// </summary>
	ElectricalTemperature = 181,
	/// <summary>
	///    The data value will be represented as a ElectricalCableTraySize.
	/// </summary>
	ElectricalCableTraySize = 182,
	/// <summary>
	///    The data value will be represented as a ElectricalConduitSize.
	/// </summary>
	ElectricalConduitSize = 183,
	/// <summary>
	///    The data value will be represented as a ReinforcementVolume.
	/// </summary>
	ReinforcementVolume = 184,
	/// <summary>
	///    The data value will be represented as a ReinforcementLength.
	/// </summary>
	ReinforcementLength = 185,
	/// <summary>
	///    The data value will be represented as a ElectricalDemandFactor.
	/// </summary>
	ElectricalDemandFactor = 186,
	/// <summary>
	///    The data value will be represented as a HVACDuctInsulationThickness.
	/// </summary>
	HVACDuctInsulationThickness = 187,
	/// <summary>
	///    The data value will be represented as a HVACDuctLiningThickness.
	/// </summary>
	HVACDuctLiningThickness = 188,
	/// <summary>
	///    The data value will be represented as a PipeInsulationThickness.
	/// </summary>
	PipeInsulationThickness = 189,
	/// <summary>
	///    The data value will be represented as a HVACThermalResistance.
	/// </summary>
	HVACThermalResistance = 190,
	/// <summary>
	///    The data value will be represented as a HVACThermalMass.
	/// </summary>
	HVACThermalMass = 191,
	/// <summary>
	///    The data value will be represented as an Acceleration.
	/// </summary>
	Acceleration = 192,
	/// <summary>
	///    The data value will be represented as a BarDiameter.
	/// </summary>
	BarDiameter = 193,
	/// <summary>
	///    The data value will be represented as a CrackWidth.
	/// </summary>
	CrackWidth = 194,
	/// <summary>
	///    The data value will be represented as a DisplacementDeflection.
	/// </summary>
	DisplacementDeflection = 195,
	/// <summary>
	///    The data value will be represented as an Energy.
	/// </summary>
	Energy = 196,
	/// <summary>
	///    The data value will be represented as a StructuralFrequency.
	/// </summary>
	StructuralFrequency = 197,
	/// <summary>
	///    The data value will be represented as a Mass.
	/// </summary>
	Mass = 198,
	/// <summary>
	///    The data value will be represented as a MassPerUnitLength.
	/// </summary>
	MassPerUnitLength = 199,
	/// <summary>
	///    The data value will be represented as a MomentOfInertia.
	/// </summary>
	MomentOfInertia = 200,
	/// <summary>
	///    The data value will be represented as a SurfaceArea.
	/// </summary>
	SurfaceArea = 201,
	/// <summary>
	///    The data value will be represented as a Period.
	/// </summary>
	Period = 202,
	/// <summary>
	///    The data value will be represented as a Pulsation.
	/// </summary>
	Pulsation = 203,
	/// <summary>
	///    The data value will be represented as a ReinforcementArea.
	/// </summary>
	ReinforcementArea = 204,
	/// <summary>
	///    The data value will be represented as a ReinforcementAreaPerUnitLength.
	/// </summary>
	ReinforcementAreaPerUnitLength = 205,
	/// <summary>
	///    The data value will be represented as a ReinforcementCover.
	/// </summary>
	ReinforcementCover = 206,
	/// <summary>
	///    The data value will be represented as a ReinforcementSpacing.
	/// </summary>
	ReinforcementSpacing = 207,
	/// <summary>
	///    The data value will be represented as a Rotation.
	/// </summary>
	Rotation = 208,
	/// <summary>
	///    The data value will be represented as a SectionArea.
	/// </summary>
	SectionArea = 209,
	/// <summary>
	///    The data value will be represented as a SectionDimension.
	/// </summary>
	SectionDimension = 210,
	/// <summary>
	///    The data value will be represented as a SectionModulus.
	/// </summary>
	SectionModulus = 211,
	/// <summary>
	///    The data value will be represented as a SectionProperty.
	/// </summary>
	SectionProperty = 212,
	/// <summary>
	///    The data value will be represented as a StructuralVelocity.
	/// </summary>
	StructuralVelocity = 213,
	/// <summary>
	///    The data value will be represented as a WarpingConstant.
	/// </summary>
	WarpingConstant = 214,
	/// <summary>
	///    The data value will be represented as a Weight.
	/// </summary>
	Weight = 215,
	/// <summary>
	///    The data value will be represented as a WeightPerUnitLength.
	/// </summary>
	WeightPerUnitLength = 216,
	/// <summary>
	///    The data value will be represented as a HVACThermalConductivity.
	/// </summary>
	HVACThermalConductivity = 217,
	/// <summary>
	///    The data value will be represented as a HVACSpecificHeat.
	/// </summary>
	HVACSpecificHeat = 218,
	/// <summary>
	///    The data value will be represented as a HVACSpecificHeatOfVaporization.
	/// </summary>
	HVACSpecificHeatOfVaporization = 219,
	/// <summary>
	///    The data value will be represented as a HVACPermeability.
	/// </summary>
	HVACPermeability = 220,
	/// <summary>
	///    The data value will be represented as a ElectricalResistivity.
	/// </summary>
	ElectricalResistivity = 221,
	/// <summary>
	///    The data value will be represented as a MassDensity.
	/// </summary>
	MassDensity = 222,
	/// <summary>
	///    The data value will be represented as a MassPerUnitArea.
	/// </summary>
	MassPerUnitArea = 223,
	/// <summary>
	///    The value of this parameter will be a Pipe Dimension
	/// </summary>
	PipeDimension = 224,
	/// <summary>
	///    The value of this parameter will be the Pipe Mass
	/// </summary>
	PipeMass = 225,
	/// <summary>
	///    The value of this parameter will be the Pipe Mass per Unit Length
	/// </summary>
	PipeMassPerUnitLength = 226,
	/// <summary>
	///    The data value will be represented as a HVACTemperatureDifference.
	/// </summary>
	HVACTemperatureDifference = 227,
	/// <summary>
	///    The data value will be represented as a PipingTemperatureDifference.
	/// </summary>
	PipingTemperatureDifference = 228,
	/// <summary>
	///    The data value will be represented as an ElectricalTemperatureDifference.
	/// </summary>
	ElectricalTemperatureDifference = 229,
	/// <summary>
	///    The data value will be represented as a TimeInterval
	/// </summary>
	TimeInterval = 230,
	/// <summary>
	///    The data value will be represented as a Speed
	/// </summary>
	Speed = 231
}
