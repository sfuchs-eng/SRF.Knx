using System.Data;
using Microsoft.Extensions.Logging;
using SRF.Knx.Core.DPT;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Core;

/// <inheritdoc/>
public class UnitSystemsMapper(
    IKnxMasterDataProvider knxMasterDataProvider,
    ILogger<UnitSystemsMapper> logger
) : IUnitSystemsMapper
{
    public DptUnitsNetMapping? GetDpstUnitMapping(DataPointTypeId dpstId, string? queryContextForLogging = null)
    {
        var masterData = knxMasterDataProvider.GetMasterData();
        var dptMetadata = DptMetadata.FromMasterData(dpstId, masterData, logger);
        return GetDptUnitsNetMapping(dptMetadata, queryContextForLogging);
    }

    public DptUnitsNetMapping? GetDptUnitsNetMapping(DptMetadata dptMetadata, string? queryContextForLogging = null)
    {
        if (dptMetadata.Dpst == null)
        {
            logger.LogWarning("No DPST metadata found for DPT {DpstId}. Context: {QueryContext}", dptMetadata.Id, queryContextForLogging);
            return null;
        }

        var dptSubtypeMaster = dptMetadata.Dpst;
        try
        {
            var mapping = DptNamePrefixToUnitMapping.SingleOrDefault(m => System.Text.RegularExpressions.Regex.IsMatch(dptSubtypeMaster.Name, m.DptSubtypeNamePattern));
            if (mapping is null)
                return null;

            // if UnitOverride is set, we can use it directly. If not, we try to get the unit from the KNX master data format.
            if (mapping.UnitNetUnitType is not null && mapping.UnitNetUnit is not null)
            {
                return mapping;
            }

            // if mapping.UnitNetUnitType is null, derive it from the UnitNetDimension type.

            if (mapping.UnitNetUnitType is null)
            {
                var dimensionType = mapping.UnitNetDimension;
                // can we get the type of the unit straight from the Unit property of the dimension type without creating an instance?
                var unitProperty = dimensionType.GetProperty("Unit")
                    ?? throw new InvalidOperationException($"Failed to get Unit property from UnitsNet dimension type {dimensionType} for DPST {dptMetadata.Id} ({dptMetadata.Dpst?.Name}). Context: {queryContextForLogging}");
                var unitType = unitProperty.PropertyType;
                mapping = new DptUnitsNetMapping(mapping.DptSubtypeNamePattern, mapping.UnitNetDimension, unitType: unitType, unit: mapping.UnitNetUnit, knxUnitSymbolOverride: mapping.KnxUnitSymbolOverride, isUnitAware: mapping.IsUnitAware);
            }

            // KNX symbol to be used
            var knxUnitSymbol = mapping.KnxUnitSymbolOverride ?? dptSubtypeMaster.Format?.Elements.OfType<NumericFormat>().FirstOrDefault()?.Unit
                ?? throw new InvalidOperationException($"No KNX unit symbol found for DPST {dptMetadata.Id} ({dptMetadata.Dpst?.Name}). Context: {queryContextForLogging}");

            // if UnitType is set but Unit not, we try to get the unit from the KNX master data format.
            if (mapping.UnitNetUnit is null)
            {
                var unitNetUnit = UnitsNet.UnitParser.Default.Parse(
                    knxUnitSymbol,
                    mapping.UnitNetUnitType ?? throw new InvalidOperationException($"UnitNetUnitType is not set for mapping of DPST {dptMetadata.Id} ({dptMetadata.Dpst?.Name}). Context: {queryContextForLogging}")
                );
                mapping = new DptUnitsNetMapping(mapping.DptSubtypeNamePattern, mapping.UnitNetDimension, unitType: mapping.UnitNetUnitType, unit: unitNetUnit, knxUnitSymbolOverride: mapping.KnxUnitSymbolOverride, isUnitAware: mapping.IsUnitAware);
            }

            return mapping;
        }
        catch /*(Exception ex)*/
        {
            //logger.LogWarning(ex, "Error while trying to map DPT {DptSimpleName} ({DptSimpleId}) to UnitsNet dimension. Context: {QueryContext}", dptMetadata.Dpst?.Name, dptMetadata.Id, queryContextForLogging);
            return null;
        }
    }

    private readonly List<DptUnitsNetMapping> DptNamePrefixToUnitMapping = [
        new("DPT_Value_1_Ucount", typeof(byte), isUnitAware: false), // 5.010
        new("DPT_Scaling", typeof(UnitsNet.Ratio)),
        new("DPT_Angle", typeof(UnitsNet.Angle)),
        new("DPT_Percent.*", typeof(UnitsNet.Ratio)),
        new("DPT_Value_2_Count", typeof(short), isUnitAware: false),
        new("DPT_Value_4_Count", typeof(int), isUnitAware: false), // 13.001
        new("DPT_Coefficient", typeof(UnitsNet.Ratio)), // 9.031
        new("DPT_TimePeriod.*", typeof(UnitsNet.Duration)),
        new("DPT_Length.*", typeof(UnitsNet.Length)),
        new("DPT_UElCurrent.*", typeof(UnitsNet.ElectricCurrent)),
        new("DPT_Brightness", typeof(UnitsNet.Illuminance)),
        new("DPT_Absolute_Colour_Temperature", typeof(UnitsNet.Temperature)),
        new("DPT_DeltaTime.*", typeof(UnitsNet.Duration)),
        new("DPT_Rotation_Angle.*", typeof(UnitsNet.Angle)),
        new("DPT_Value_Angle.*", typeof(UnitsNet.Angle)),
        new("DPT_Value_Temp.*", typeof(UnitsNet.Temperature)),
        new("DPT_Value_Lux", typeof(UnitsNet.Illuminance), unitType: typeof(UnitsNet.Units.IlluminanceUnit), unit: UnitsNet.Units.IlluminanceUnit.Lux),
        new("DPT_Value_Wsp.*", typeof(UnitsNet.Speed)),
        new("DPT_Value_Pres", typeof(UnitsNet.Pressure)),
        new("DPT_Value_Humidity", typeof(UnitsNet.RelativeHumidity)),
        new("DPT_Value_AirQuality", typeof(UnitsNet.VolumeConcentration)),
        new("DPT_Value_AirFlow", typeof(UnitsNet.VolumeFlow), unitType: typeof(UnitsNet.Units.VolumeFlowUnit), unit: UnitsNet.Units.VolumeFlowUnit.CubicMeterPerHour),
        new("DPT_Value_Time.*", typeof(UnitsNet.Duration)),
        new("DPT_Value_Volt", typeof(UnitsNet.ElectricPotential)),
        new("DPT_Value_Curr", typeof(UnitsNet.ElectricCurrent)),
        new("DPT_PowerDensity", typeof(UnitsNet.PowerDensity)),
        new("DPT_Power", typeof(UnitsNet.Power)),
        new("DPT_Value_Volume_Flow", typeof(UnitsNet.VolumeFlow)),
        new("DPT_Rain_Amount", typeof(UnitsNet.Length), knxUnitSymbolOverride: "mm", unitType: typeof(UnitsNet.Units.LengthUnit), unit: UnitsNet.Units.LengthUnit.Millimeter), // KNX "l/m2" is equivalent to "mm" in UnitsNet, so we use the default unit of "mm" for this mapping.
        new("DPT_Value_Absolute_Humidity", typeof(UnitsNet.Density), unitType: typeof(UnitsNet.Units.DensityUnit), unit: UnitsNet.Units.DensityUnit.KilogramPerCubicMeter),
        new("DPT_Concentration.*", typeof(UnitsNet.Density)),
        new("DPT_LongTimePeriod.*", typeof(UnitsNet.Duration)),
        new("DPT_Volume.*", typeof(UnitsNet.Volume)),
        new("DPT_FlowRate.*", typeof(UnitsNet.VolumeFlow)),
        new("DPT_ActiveEnergy.*", typeof(UnitsNet.Energy)),
        new("DPT_ApparentEnergy.*", typeof(UnitsNet.ElectricApparentEnergy)),
        new("DPT_ReactiveEnergy.*", typeof(UnitsNet.ElectricReactiveEnergy)),
        new("DPT_LongDeltaTime.*", typeof(UnitsNet.Duration)),
        new("DPT_DeltaVolumeLiquid.*", typeof(UnitsNet.Volume)),
        new("DPT_Value_Electric_Potential", typeof(UnitsNet.ElectricPotential)),
        new("DPT_Value_Electric_Current", typeof(UnitsNet.ElectricCurrent)),
        new("DPT_Value_Electric_Charge", typeof(UnitsNet.ElectricCharge)),
        new("DPT_Value_Electric_Resistance", typeof(UnitsNet.ElectricResistance)),
        new("DPT_Value_Electric_Conductance", typeof(UnitsNet.ElectricConductance)),
        new("DPT_Value_Electric_Capacitance", typeof(UnitsNet.ElectricCapacitance)),
        new("DPT_Value_Electric_Inductance", typeof(UnitsNet.ElectricInductance)),
        new("DPT_Value_Electric_Power", typeof(UnitsNet.Power)),
        new("DPT_Value_Electric_Energy", typeof(UnitsNet.Energy)),
        new("DPT_Value_Electric_Power_Factor", typeof(UnitsNet.Ratio)),
        new("DPT_Value_Electric_Frequency", typeof(UnitsNet.Frequency)),
        new("DPT_Value_Acceleration", typeof(UnitsNet.Acceleration)),
        new("DPT_Value_Power", typeof(UnitsNet.Power)),
        new("DPT_Value_Power_Factor", typeof(UnitsNet.Ratio)),
        new("DPT_Value_Pressure", typeof(UnitsNet.Pressure)),
        new("DPT_Value_Speed", typeof(UnitsNet.Speed)),
        new("DPT_Value_.*_Temperature", typeof(UnitsNet.Temperature)),
    ];
}

/// <summary>
/// Used by <see cref="UnitSystemsMapper"/> to map KNX DPTs to UnitsNet dimensions and units.
/// Register additions to the mapping in <see cref="UnitSystemsMapper.DptNamePrefixToUnitMapping"/> (interface semantics to be extended for this).
/// </summary>
public class DptUnitsNetMapping(string dptSubtypeNamePattern, Type unitNetDimension, Type? unitType = null, Enum? unit = null, string? knxUnitSymbolOverride = null, bool isUnitAware = true)
{
    /// <summary>
    /// Whether the mapping is unit-aware, i.e. whether it has a corresponding UnitsNet dimension and unit.
    /// </summary>
    public bool IsUnitAware { get; } = isUnitAware;

    /// <summary>
    /// The name of the UnitNet dimension type corresponding to the KNX DPT. This is used for generating code and documentation.
    /// </summary>
    public string DimensionName => UnitNetDimension.Name;

    /// <summary>
    /// The name of the unit corresponding to the KNX DPT. This is used for generating code and documentation.
    /// </summary>
    public string? UnitName => UnitNetDimension.Name;

    /// <summary>
    /// The patttern against which the KNX DPST name (see KNX master data) is matched to determine if this mapping applies to a given DPT.
    /// </summary>
    public string DptSubtypeNamePattern { get; } = dptSubtypeNamePattern;

    /// <summary>
    /// The UnitsNet dimension type corresponding to the KNX DPT.
    /// In the IValue<>, that's the ApplicationType of the DPT, e.g. UnitsNet.Temperature for DPT 9.001 (Temperature), etc.
    /// </summary>
    public Type UnitNetDimension { get; } = unitNetDimension;

    /// <summary>
    /// The UnitsNet unit type corresponding to the KNX DPT, e.g. UnitsNet.Units.TemperatureUnit for DPT 9.001 (Temperature °C), etc.
    /// </summary>
    public Type? UnitNetUnitType { get; } = unitType;

    /// <summary>
    /// The UnitsNet unit corresponding to the KNX DPT, e.g. UnitsNet.Units.TemperatureUnit.DegreeCelsius for DPT 9.001 (Temperature °C), etc.
    /// </summary>
    public Enum? UnitNetUnit { get; } = unit;

    /// <summary>
    /// Set this to override the default KNX DPT unit symbol with a specific unit symbol.
    /// It's used to derive the corresponding UnitsNet unit type for the KNX DPT mapping. If not set, the unit symbol is derived from the KNX master data for the DPST.
    /// </summary>
    public string? KnxUnitSymbolOverride { get; } = knxUnitSymbolOverride;
}
