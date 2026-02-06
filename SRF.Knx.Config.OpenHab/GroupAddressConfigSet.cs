using SRF.Knx.Config.Domain;
using SRF.Knx.Config.OpenHab.BaseConfig;

namespace SRF.Knx.Config.OpenHab;

public class GroupAddressConfigSet
{
    public required ETS5.EtsGroupAddressConfig EtsGroupAddress { get; init; }
    public required Domain.GroupAddressExtraConfig ExtraConfig { get; init; }
    public required OHKnxGroupAddress OpenHab { get; init; }

    public static GroupAddressConfigSet Get(
        KnxGroupAddress groupAddress,
        DomainConfiguration domainConfig,
        OHKnxGroupAddress ohGAC,
        Func<IEnumerable<GroupAddressExtraConfig>, GroupAddressExtraConfig>? extraConfigResolver = null
        )
    {
        var egac = domainConfig.Extra.GetGAExtraConfig(groupAddress).ToArray();
        extraConfigResolver ??= (extras) => extras.First();
        if (egac.Length < 1)
            throw new ArgumentOutOfRangeException(nameof(groupAddress), $"There's no Group Address {groupAddress} in the Extra Domain Config");

        return new()
        {
            EtsGroupAddress = domainConfig.GroupAddresses[groupAddress.Address],
            ExtraConfig = extraConfigResolver(egac),
            OpenHab = ohGAC
        };
    }
}
