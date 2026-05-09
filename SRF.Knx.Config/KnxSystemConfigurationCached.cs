using SRF.Knx.Config.Domain;
using SRF.Knx.Core;
using SRF.Knx.Core.DPT;

namespace SRF.Knx.Config;

/// <summary>
/// Default implementation of <see cref="IKnxSystemConfiguration"/> that caches all group address metadata in memory for efficient lookup by group address or name.
/// The cache is initialized in the constructor and is immutable afterwards.
/// </summary>
public class KnxSystemConfigurationCached : IKnxSystemConfiguration
{
    private readonly Dictionary<GroupAddress, GroupAddressMeta> GroupAddressesByAddress;
    private readonly Dictionary<string, GroupAddressMeta> GroupAddressesByName;

    public KnxSystemConfigurationCached(IEnumerable<GroupAddressConfiguration> groupAddresses, IDptFactory dptFactory)
    {
        ArgumentNullException.ThrowIfNull(groupAddresses, nameof(groupAddresses));
        ArgumentNullException.ThrowIfNull(dptFactory, nameof(dptFactory));

        GroupAddressesByAddress = new Dictionary<GroupAddress, GroupAddressMeta>();
        GroupAddressesByName = new Dictionary<string, GroupAddressMeta>();

        foreach (var gac in groupAddresses)
        {
            var meta = new GroupAddressMeta(gac, dptFactory.Get(gac.Ets.DPT));
            GroupAddressesByAddress[gac.Ets.Address] = meta;
            GroupAddressesByName[meta.Name] = meta;
        }
    }

    public void ClearCache()
    {
        throw new NotImplementedException();
    }

    public DptBase GetDpt(GroupAddress groupAddress)
    {
        throw new NotImplementedException();
    }

    public GroupAddressMeta GetGroupAddressMeta(GroupAddress groupAddress)
    {
        ArgumentNullException.ThrowIfNull(groupAddress, nameof(groupAddress));
        return GroupAddressesByAddress.TryGetValue(groupAddress, out var meta) ? meta : throw new KeyNotFoundException($"Group address {groupAddress} not found.");
    }

    public GroupAddressMeta GetGroupAddressMeta(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        return GroupAddressesByName.TryGetValue(name, out var meta) ? meta : throw new KeyNotFoundException($"Group address with name '{name}' not found.");
    }

    public GroupAddressMeta? GetGroupAddressMetaOrNull(GroupAddress groupAddress)
    {
        ArgumentNullException.ThrowIfNull(groupAddress, nameof(groupAddress));
        return GroupAddressesByAddress.TryGetValue(groupAddress, out var meta) ? meta : null;
    }

    public GroupAddressMeta? GetGroupAddressMetaOrNull(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        return GroupAddressesByName.TryGetValue(name, out var meta) ? meta : null;
    }
}
