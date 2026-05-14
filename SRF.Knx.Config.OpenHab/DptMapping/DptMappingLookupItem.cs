namespace SRF.Knx.Config.OpenHab.DptMapping;

/// <summary>
/// Normally <see cref="DptMappingLookupItem"/>s are loaded from a JSON file configured in <see cref="KnxConfiguration.OpenHabOptions.KnxDptMappings"/>.
/// </summary>
public partial class DptMappingLookupItem
{
    public string? Comment { get; set; }

    public string[] DPTs { get; set; } = [];

    /// <summary>
    /// If set, this DPT shall override the DPT set in ETS to derive the OpenHAB channel DPT in <see cref="BaseConfig.OHKnxGroupAddress.ChannelConfig.DPTs"/>
    /// </summary>
    /// <value></value>
    public string? TreatAsDpt { get; set; }

    public DptConfigOptions[] Channels { get; set; } = [];
}
