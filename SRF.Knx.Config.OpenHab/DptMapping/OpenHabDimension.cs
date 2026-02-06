using System.Text.Json.Serialization;

namespace SRF.Knx.Config.OpenHab.DptMapping;

/// <summary>
/// Taken 08.11.2025 from https://www.openhab.org/docs/concepts/units-of-measurement.html
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<OpenHabDimension>))]
public enum OpenHabDimension
{
	/// Requires Dimension that depends on the specific DPT
	AccordingDpt,

	Acceleration, // Meter per square second (m/s²)	Meter per square second (m/s²)
	AmountOfSubstance, //	Mole (mol)	Mole (mol)
	Angle, // Degree (°)	Degree (°)
	Area, // Square Meter (m²)	Square foot (ft²)
	ArealDensity, //	Dobson unit (DU)	Dobson unit (DU)
	CatalyticActivity, //	Katal (kat)	Katal (kat)
	DataAmount, //	Byte (B)	Byte (B)
	DataTransferRate, //    Megabit per second (Mbit/s)	Megabit per second (Mbit/s)
	Density, // Kilogram per cubic meter (kg/m³)	Kilogram per cubic meter (kg/m³)
	Dimensionless, // Abstract unit one (one)	Abstract unit one (one)
	ElectricCapacitance, // Farad (F)	Farad (F)
	ElectricCharge, // Coulomb (C)	Coulomb (C)
	ElectricConductance, // Siemens (S)	Siemens (S)
	ElectricConductivity, // Siemens per meter (S/m)	Siemens per meter (S/m)
	ElectricCurrent, // Ampere (A)	Ampere (A)
	ElectricInductance, // Henry (H)	Henry (H)
	ElectricPotential, // Volt (V)	Volt (V)
	ElectricResistance, // Ohm (Ω)	Ohm (Ω)
	EmissionIntensity, // Gram per kilowatt hour (g/kWh)	Gram per kilowatt hour (g/kWh)
	Energy, // Kilowatt hours (kWh)	Kilowatt hours (kWh)
	ActiveEnergy, // supported? acc. KNX binding yes, but not acc. linked page.
	Force, // Newton (N)	Newton (N)
	Frequency, // Hertz (Hz)	Hertz (Hz)
	Illuminance, // Lux (lx)	Lux (lx)
	Intensity, // Irradiance (W/m²)	Irradiance (W/m²)
	Length, // Meter (m)	Inch (in)
	LuminousFlux, // Lumen (lm)	Lumen (lm)
	LuminousIntensity, // Candela (cd)	Candela (cd)
	MagneticFlux, // Weber (Wb)	Weber (Wb)
	MagneticFluxDensity, // Tesla (T)	Tesla (T)
	Mass, // Kilogram (kg)	Pound (lb)
	Power, // Watt (W)	Watt (W)
	Pressure, // Hectopascal (hPa)	Inch of mercury (inHg)
	RadiantExposure, // Joule per square meter (J/m²)	Joule per square meter (J/m²)
	RadiationAbsorbedDose, // Gray (Gy)	Gray (Gy)
	RadiationEffectiveDose, // Sievert (Sv)	Sievert (Sv)
	Radioactivity, // Becquerel (Bq)	Becquerel (Bq)
	SolidAngle, // Steradian (sr)	Steradian (sr)
	Speed, // Kilometers per hour (km/h)	Miles per hour (mph)
	Temperature, // Celsius (°C)	Fahrenheit (°F)
	Time, // Seconds (s)	Seconds (s)
	Volume, // Cubic Meter (m³)	US Gallon (gal)
	VolumetricFlowRate, // Liter per minute (l/min)	US Gallon per minute (gal/min)
}
