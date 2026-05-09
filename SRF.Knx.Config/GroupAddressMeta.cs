using SRF.Knx.Config.Domain;
using SRF.Knx.Core.DPT;

namespace SRF.Knx.Config;

/// <summary>
/// Application runtime enriched KNX Group Address configuration, containing the ETS export configuration and the Extra configuration, as well as the resolved DPT object for the group address.
/// See <see cref="IKnxSystemConfiguration"/> for details.
/// </summary>
public class GroupAddressMeta
{
    public GroupAddressConfiguration Configuration { get; }
    public string Name => Configuration.Extra!.Name!;
    public string Description => Configuration.Ets.Description ?? string.Empty;
    public DptBase Dpt { get; init; }

    public GroupAddressMeta(GroupAddressConfiguration configuration, DptBase dpt)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Dpt = dpt ?? throw new ArgumentNullException(nameof(dpt));
        if (configuration.Ets == null)
            throw new ArgumentException("Ets configuration must not be null.", nameof(configuration));
        if (!configuration.Ets.DPT.IsValidMainType)
            throw new ArgumentException($"Ets configuration for group address {configuration.Ets.Address} has no valid DPT configured.", nameof(configuration));
        if (configuration.Extra == null)
            throw new ArgumentException($"GroupAddress {configuration.Ets.Address} Configuration.Extra configuration must not be null.", nameof(configuration));
        if (string.IsNullOrWhiteSpace(configuration.Extra.Name))
            throw new ArgumentException($"GroupAddress {configuration.Ets.Address} Configuration.Extra.Name must not be null or whitespace.", nameof(configuration));
    }
}