using SRF.Knx.Config.ETS5;

namespace SRF.Knx.Config.Domain;

/// <summary>
/// Reflects the KNX related configuration of a domain of 16-bit group addresses
/// and IoT nodes, typically equivalent to an ETS project.
/// The property <see cref="GroupAddresses"/> contains the ETS exported group address configurations,
/// while <see cref="Extra"/> contains additional configuration loaded from a separate domain extra config file
/// as specified in <see cref="KnxConfiguration.KnxDomainConfigFile"/>.
/// </summary>
public class DomainConfiguration
{
    /// <summary>
    /// ETS exported Group Address configurations by their 16-bit address.
    /// Always loaded from ETS export file.
    /// </summary>
    public Dictionary<ushort, EtsGroupAddressConfig> GroupAddresses { get; init; } = [];

    /// <summary>
    /// Root node of the domain extra config file.
    /// </summary>
    public DomainExtraConfig Extra { get; set; } = new();
}
