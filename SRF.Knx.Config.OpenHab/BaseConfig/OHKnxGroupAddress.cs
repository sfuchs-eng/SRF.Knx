using System.Text.Json.Serialization;
using SRF.Knx.Config.ETS5;
using SRF.Knx.Core;

namespace SRF.Knx.Config.OpenHab.BaseConfig;

/// <summary>
/// OpenHAB configuration for a KNX Group Address
/// Encapsulates all information required to create corresponding channels and linked items
/// TODO: move towards storing Channel &amp; Item configs derived via templating directly,
/// replacing or refining <see cref="OHKnxGroupAddress"/>. Legacy had it all in 1 record, not allowing manual channel/item/ga association fine-tuning.
/// No need to keep legacy compliant. Legacy is addressed by <see cref="Domain.Legacy.KnxGroupAddressConfig"/>.
/// </summary>
[Serializable]
public partial class OHKnxGroupAddress : IEquatable<OHKnxGroupAddress>{
    [JsonIgnore]
    public Core.GroupAddress Address { get; set; } = new(0);

    [JsonPropertyName("Address")]
    public string Address3L { get => Address.Address.To3LGroupAddress(); set => Address = Core.GroupAddress.Parse(value); }

    public Domain.ExtraConfigStatus EntryStatus { get; set; } = Domain.ExtraConfigStatus.Automatic | Domain.ExtraConfigStatus.Fresh;

    public ItemConfig? Item { get; set; }
    public ChannelConfig Channel { get; set; } = new();
    public SiteMapConfig SiteMap { get; set; } = new();

    //============= wrappers for legacy code ============================

    [JsonIgnore]
    public string Name { get => Item?.Name ?? Channel.Name; set { if (Item != null) Item.Name = value; Channel.Name = value; } }

    [JsonIgnore]
    public bool CreateItem { get => Item != null; set => Item ??= value ? new ItemConfig() : null; }

    [JsonIgnore]
    public string? Label { get => Item?.Label; set => (Item ?? throw new IndexOutOfRangeException("got no ItemConfig")).Label = value ?? string.Empty; }

    [JsonIgnore]
    public string ItemType { get => Item?.Type.ToString() ?? string.Empty; set => (Item ?? throw new IndexOutOfRangeException("got no ItemConfig")).Type = Enum.TryParse<Templating.Items.ItemType>(value, out var it) ? it : Templating.Items.ItemType.Undefined; }

    /// <summary>
    /// KNX Data Point Type, but the one used for OpenHAB which might not be equal to the one set in ETS.
    /// </summary> 
    [JsonIgnore]
    public string? DPTs { get => Channel.DPT?.DotFormat; set => Channel.DPT = new(value); }
    [JsonIgnore]
    public DPT? DPT { get => Channel.DPT; set => Channel.DPT = value; }

    [JsonIgnore]
    public string? Icon { get => Item?.Icon; set => (Item ?? throw new IndexOutOfRangeException("got no ItemConfig")).Icon = value; }

    /// <summary>
    /// OpenHAB item groups the GA shall be a member of
    /// </summary>
    [JsonIgnore]
    public string[] Groups { get => Item?.Groups ?? []; set => (Item ?? throw new IndexOutOfRangeException("got no ItemConfig")).Groups = value; }

    /// <summary>
    /// Additional Group Addresses to link to the same item
    /// </summary>
    [JsonIgnore]
    public string[] AdditionalGA { get; set; } = [];
    
    [JsonIgnore]
    public string MapType { get => SiteMap.MapType ?? ""; set => SiteMap.MapType = value; }

    [JsonIgnore]
    public string Mappings { get => SiteMap.Mappings ?? string.Empty; set => SiteMap.Mappings = value; }

    /// <summary>
    /// Is it a readonly Group Address? E.g. a sensor value?
    /// </summary>
    [JsonIgnore]
    public bool IsWritable { get => Channel.IsWritable; set => Channel.IsWritable = value; }

    /// <summary>
    /// Is OpenHAB expected to answer a ReadRequest from the bus?
    /// </summary>
    [JsonIgnore]
    public bool IsStateOwned { get => Channel.IsStateOwned; set => Channel.IsStateOwned = value; }

    public bool Equals(OHKnxGroupAddress? other)
    {
        return Address.Equals(other?.Address)
            && EntryStatus == other?.EntryStatus
            && ((Item == null && other?.Item == null) || (Item != null && other?.Item != null && Item.Name == other.Item.Name && Item.Label == other.Item.Label && Item.Type == other.Item.Type && Item.Icon == other.Item.Icon && Item.Groups.SequenceEqual(other.Item.Groups)))
            && Channel.Name == other?.Channel.Name
            && Channel.Type == other?.Channel.Type
            && Channel.DPTs == other?.Channel.DPTs
            && Channel.IsStateOwned == other?.Channel.IsStateOwned
            && Channel.IsWritable == other?.Channel.IsWritable
            && Channel.StatusFor == other?.Channel.StatusFor
            && SiteMap.MapType == other?.SiteMap.MapType
            && SiteMap.Mappings == other?.SiteMap.Mappings
            && AdditionalGA.SequenceEqual(other?.AdditionalGA ?? []);
    }
}

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Metadata,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(OHKnxGroupAddress))]
internal partial class OHKnxGroupAddressJsonContext : JsonSerializerContext
{
}
