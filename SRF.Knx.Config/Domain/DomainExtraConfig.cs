using SRF.Knx.Core;

namespace SRF.Knx.Config.Domain;

/// <summary>
/// Additional configuration for KNX domain elements, e.g. group addresses and their mapping to .NET CLR types, loaded from
/// an extra configuration file specified in <see cref="KnxConfiguration.KnxDomainConfigFile"/>.
/// </summary>
public class DomainExtraConfig
{
    public List<Thing> Things { get; set; } = [];

    public IEnumerable<GroupAddressExtraConfig> GetGAExtraConfig(GroupAddress groupAddress)
    {
        return [
            .. Things.SelectMany(t => t.GroupAddresses.Where(a => a.Key == groupAddress.Address).Select(a => a.Value))
        ];
    }

    public bool TryGetGAExtraConfig(GroupAddress groupAddress, out GroupAddressExtraConfig? extraConfig)
    {
        extraConfig = GetGAExtraConfig(groupAddress).FirstOrDefault();
        return extraConfig != null;
    }

    public IEnumerable<GroupAddressExtraConfig> GetAllExtraConfigs()
    {
        return [
            .. Things.SelectMany(t => t.GroupAddresses.Values)
        ];
    }
}
