using SRF.Knx.Config.ETS5;
using SRF.Knx.Core;

namespace SRF.Knx.Config.Domain;

/// <summary>
/// The Ets and Extra configuration for a KNX group address. The ETS configuration is derived from the ETS group address export,
/// while the Extra configuration is derived from it by a separate tool and thereafter loaded from a separate file using <see cref="IKnxConfigFactory"/>.
/// </summary>
public class GroupAddressConfiguration
{
    public GroupAddressConfiguration(GroupAddress address, DomainConfiguration domainConfig)
    {
        Ets = domainConfig.GroupAddresses.GetValueOrDefault(address.Address, new());
        Extra = domainConfig.Extra.GetGAExtraConfig(address).FirstOrDefault();
    }

    public GroupAddressConfiguration(EtsGroupAddressConfig etsConfig, GroupAddressExtraConfig? extraConfig)
    {
        Ets = etsConfig;
        Extra = extraConfig;
    }

    public static IEnumerable<GroupAddressConfiguration> FromDomainConfig(DomainConfiguration domainConfig)
    {
        return domainConfig.GroupAddresses.Values
            .Select(gac => new GroupAddressConfiguration(
                gac,
                domainConfig.Extra.GetGAExtraConfig(gac.Address).FirstOrDefault()
            ));
    }

    /// <summary>
    /// Group address configuration from the ETS group address export, containing the numeric address, label, description and DPT information.
    /// </summary>
    public EtsGroupAddressConfig Ets { get; set; } = new();

    /// <summary>
    /// Extra configuration for a KNX group address, containing additional metadata not available in the ETS export.
    /// E.g. the name as an ID for use in generated code properties.
    /// </summary>
    public GroupAddressExtraConfig? Extra { get; set; }
}
