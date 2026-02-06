using System;
using System.Text.Json.Serialization;

namespace SRF.Knx.Config.Domain;

public class GroupAddressExtraConfigJsonContainer
{
    [JsonIgnore]
    public Dictionary<ushort, GroupAddressExtraConfig> GroupAddresses { get; set; } = [];

    [JsonPropertyName("GroupAddresses")]
    public Dictionary<string, GroupAddressExtraConfig> GroupAddresses3LIndexed
    {
        get => GroupAddresses.ToDictionary(
            kvp => kvp.Key.To3LGroupAddress(),
            kvp => kvp.Value);
        set => GroupAddresses = value.ToDictionary(
            kvp => kvp.Key.ToKnxGroupAddress(),
            kvp => kvp.Value);
    }
}
