using SRF.Knx.Config.ETS5;

namespace SRF.Knx.Config.Domain;

public class GroupAddressConfiguration
{
    public GroupAddressConfiguration(KnxGroupAddress address, DomainConfiguration domainConfig)
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

    public EtsGroupAddressConfig Ets { get; set; } = new();
    public GroupAddressExtraConfig? Extra { get; set; }
}
