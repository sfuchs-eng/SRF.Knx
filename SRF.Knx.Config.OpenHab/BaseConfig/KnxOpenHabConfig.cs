using System.Text.Json.Serialization;

namespace SRF.Knx.Config.OpenHab.BaseConfig;

/// <summary>
/// Represents the complete KNX OpenHAB configuration including Bridge, Things, Items and Group Addresses.
/// Root object for generating OpenHAB configuration files.
/// </summary>
public class KnxOpenHabConfig
{
    /// <summary>
    /// The name of this KNX OpenHAB configuration.
    /// This name is used for the generated .things and .items files.
    /// </summary>
    public string Name { get; set; } = "knx";

    public BridgeConfig Bridge { get; set; } = new BridgeConfig();

    public List<KnxThingConfig> Things { get; set; } = [];
}
