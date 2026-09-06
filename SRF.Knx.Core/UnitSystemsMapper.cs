using Microsoft.Extensions.Logging;
using SRF.Knx.Core.DPT;

namespace SRF.Knx.Core;

/// <inheritdoc/>
public class UnitSystemsMapper(
    ILogger<UnitSystemsMapper> logger
) : IUnitSystemsMapper
{
    public DptUnitsNetMapping? GetDptUnitMapping(DptSimple dptSimple)
    {
        //var dptMaster = dptSimple.Metadata.Dpt ?? throw new InvalidOperationException($"DPT {dptSimple.Id} has no DPT metadata, cannot map to UnitsNet dimension.");
        var dptSubtypeMaster = dptSimple.Metadata.Dpst ?? throw new InvalidOperationException($"DPT {dptSimple.Id} has no DPST metadata, cannot map to UnitsNet dimension.");

        try
        {
            var mapping = DptNamePrefixToUnitMapping.SingleOrDefault(m => System.Text.RegularExpressions.Regex.IsMatch(dptSubtypeMaster.Name, m.DptSubtypeNamePattern));
            if (mapping != null)
                return mapping;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error while trying to map DPT {DptSimpleName} ({DptSimpleId}) to UnitsNet dimension.", dptSimple.Metadata.Dpst?.Name, dptSimple.Id);
            return null;
        }

        logger.LogWarning("No mapping found for DPT {DptSimpleName} ({DptSimpleId}) to UnitsNet dimension.", dptSimple.Metadata.Dpst?.Name, dptSimple.Id);
        return null;
    }

    private readonly List<DptUnitsNetMapping> DptNamePrefixToUnitMapping = [
        new("DPT_Scaling", typeof(UnitsNet.Ratio)),
        new("DPT_Angle", typeof(UnitsNet.Angle)),
        new("DPT_Percent.*", typeof(UnitsNet.Ratio)),
        new("DPT_TimePeriod.*", typeof(UnitsNet.Duration)),
        new("DPT_Length.*", typeof(UnitsNet.Length)),
        new("DPT_UElCurrent.*", typeof(UnitsNet.ElectricCurrent)),
        new("DPT_Brightness", typeof(UnitsNet.Illuminance)),
        new("DPT_Absolute_Colour_Temperature", typeof(UnitsNet.Temperature)),
        new("DPT_DeltaTime.*", typeof(UnitsNet.Duration)),
        new("DPT_Rotation_Angle", typeof(UnitsNet.Angle)),
        new("DPT_Value_Temp.*", typeof(UnitsNet.Temperature)),
        new("DPT_Value_Lux", typeof(UnitsNet.Illuminance)),
        new("DPT_Value_Wsp.*", typeof(UnitsNet.Speed)),
        new("DPT_Value_Pres", typeof(UnitsNet.Pressure)),
        new("DPT_Value_Humidity", typeof(UnitsNet.RelativeHumidity)),
        new("DPT_Value_AirQuality", typeof(UnitsNet.VolumeConcentration)),
        new("DPT_Value_AirFlow", typeof(UnitsNet.VolumeFlow), unitOverride: UnitsNet.Units.VolumeFlowUnit.CubicMeterPerHour),
        new("DPT_Value_Time.*", typeof(UnitsNet.Duration)),
        new("DPT_Value_Volt", typeof(UnitsNet.ElectricPotential)),
        new("DPT_Value_Curr", typeof(UnitsNet.ElectricCurrent)),
        new("DPT_PowerDensity", typeof(UnitsNet.PowerDensity)),
        //new("DPT_KelvinPerPercent", typeof(UnitsNet.)),
        new("DPT_Power", typeof(UnitsNet.Power)),
        new("DPT_Value_Volume_Flow", typeof(UnitsNet.VolumeFlow)),
        new("DPT_Rain_Amount", typeof(UnitsNet.Length), knxUnitSymbolOverride: "mm", unitOverride: UnitsNet.Units.LengthUnit.Millimeter), // KNX "l/m2" is equivalent to "mm" in UnitsNet, so we use the default unit of "mm" for this mapping.
        new("DPT_Value_Absolute_Humidity", typeof(UnitsNet.Density), unitOverride: UnitsNet.Units.DensityUnit.KilogramPerCubicMeter),
        new("DPT_Concentration.*", typeof(UnitsNet.Density)),
        new("DPT_LongTimePeriod.*", typeof(UnitsNet.Duration)),
        new("DPT_Volume.*", typeof(UnitsNet.Volume)),
        new("DPT_FlowRate.*", typeof(UnitsNet.VolumeFlow)),
        new("DPT_ActiveEnergy.*", typeof(UnitsNet.Energy)),
        new("DPT_ApparentEnergy.*", typeof(UnitsNet.ElectricApparentEnergy)),
        new("DPT_ReactiveEnergy.*", typeof(UnitsNet.ElectricReactiveEnergy)),
        new("DPT_LongDeltaTime.*", typeof(UnitsNet.Duration)),
        new("DPT_DeltaVolumeLiquid.*", typeof(UnitsNet.Volume)),
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
public class DptUnitsNetMapping(string dptSubtypeNamePattern, Type unitNetDimension, string? knxUnitSymbolOverride = null, Enum? unitOverride = null)
{
    /// <summary>
    /// The name of the UnitNet dimension type corresponding to the KNX DPT. This is used for generating code and documentation.
    /// </summary>
    public string DimensionName => UnitNetDimension.Name;

    /// <summary>
    /// The name of the unit corresponding to the KNX DPT. This is used for generating code and documentation.
    /// </summary>
    public string? UnitName => KnxUnitSymbolOverride ?? UnitNetDimension.Name;

    /// <summary>
    /// The patttern against which the KNX DPST name (see KNX master data) is matched to determine if this mapping applies to a given DPT.
    /// </summary>
    public string DptSubtypeNamePattern { get; } = dptSubtypeNamePattern;

    /// <summary>
    /// The UnitsNet dimension type corresponding to the KNX DPT.
    /// </summary>
    public Type UnitNetDimension { get; } = unitNetDimension;

    /// <summary>
    /// Set this to override the default KNX DPT unit symbol with a specific unit symbol.
    /// </summary>
    public string? KnxUnitSymbolOverride { get; } = knxUnitSymbolOverride;

    /// <summary>
    /// Set this to override the default UnitsNet unit with a specific unit matching the KNX native unit.
    /// </summary>
    public Enum? UnitOverride { get; } = unitOverride;
}
