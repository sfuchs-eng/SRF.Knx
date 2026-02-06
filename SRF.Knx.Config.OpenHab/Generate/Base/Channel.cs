using System.Data;
using Microsoft.Extensions.Logging;
using SRF.Knx.Config.Exceptions;
using SRF.Knx.Config.OpenHab.BaseConfig;
using SRF.Knx.Core.DPT;

namespace SRF.Knx.Config.OpenHab.Generate.Base;

/// <summary>
/// KNX Channel within an OpenHAB Thing
///TODO: support channels with multiple addresses (e.g. for status + command)
/// </summary>
public class Channel : IConfigGenerator, IChannel
{
    public ChannelType Type { get; set; }

    public readonly OHKnxGroupAddress Config;

    protected readonly IEnumerable<OHKnxGroupAddress> AllGroupAddrConfigs;
    
    private readonly ILogger<Channel> logger;

    public Channel(OHKnxGroupAddress gaConfig, IEnumerable<OHKnxGroupAddress> allGAConfigs, ILoggerFactory loggerFactory)
    {
        Config = gaConfig;
        this.logger = loggerFactory.CreateLogger<Channel>();
        AllGroupAddrConfigs = allGAConfigs;
        Type = DetermineChannelType();
    }

    protected record ItemChannelMapping(string ItemType, ChannelType ChannelType, string SubChannel, DataPointTypeId DefaultDPT);
    protected static readonly DataPointTypeId InvalidDPT = new();
    protected static readonly string InvalidItemTypeName = "item-to-channel-type-mapping-failure";
    /// <summary>
    /// Maps an <see cref="Templating.Items.ItemType"/> contained in a <see cref="OHKnxGroupAddress"/> to an OpenHAB channel type.
    /// Works as long as the KNX bridge uses channels that match item types.
    /// Item Types as of OpenHAB 3.4, 2022-08-01: https://www.openhab.org/docs/configuration/items.html
    /// </summary>
    protected static readonly Dictionary<string, ItemChannelMapping> ItemToChannelTypeMap = new ItemChannelMapping[]
    {
        new("Color", ChannelType.NotSupported, "", InvalidDPT),
        new("Contact", ChannelType.Contact, "ga", new DataPointTypeId("1.009")),
        new("DateTime", ChannelType.DateTime, "ga", new DataPointTypeId("19.001")),
        new("Dimmer", ChannelType.Dimmer, "position", new DataPointTypeId("5.001")),
        new("Group", ChannelType.NotSupported, "", InvalidDPT),
        new("Image", ChannelType.NotSupported, "", InvalidDPT),
        new("Location", ChannelType.NotSupported, "", InvalidDPT),
        new("Number", ChannelType.Number, "ga", new DataPointTypeId("9.001")),
        new("Player", ChannelType.NotSupported, "", InvalidDPT),
        new("Rollershutter", ChannelType.Rollershutter, "position", new DataPointTypeId("5.001")),
        new("String", ChannelType.String, "ga", new DataPointTypeId("16.001")),
        new("Switch", ChannelType.Switch, "ga", new DataPointTypeId("1.001")),
        //
        new(InvalidItemTypeName, ChannelType.NotSupported, "", InvalidDPT)
    }.ToDictionary(
        r => r.ItemType
    );

    protected virtual ChannelType DetermineChannelType()
    {
        if (Config.Channel.Type != ChannelType.Default && Config.Channel.Type != ChannelType.NotSupported)
            return Config.Channel.Type;
        var itemType = Config.ItemType ?? InvalidItemTypeName;
        var chanType = ItemToChannelTypeMap.GetValueOrDefault(itemType);
        if (chanType == null || chanType.ChannelType == ChannelType.Default || chanType.ChannelType == ChannelType.NotSupported)
            throw new ArgumentOutOfRangeException($"Unable to convert item type '{itemType}' for GA {Config.Address} '{Config.Name}' to an OpenHAB KNX channel type.");
        logger.LogDebug("Determined channel type '{ChannelType}' for GA {GAAddress} '{GAName}' with item type '{ItemType}' the old fashioned way. Consider updating the GA config to specify channel type directly.", chanType.ChannelType, Config.Address, Config.Name, itemType);
        return chanType.ChannelType;
    }

    protected virtual DataPointTypeId DPT
    {
        get
        {
            if (Config.Channel.DPT != null && Config.Channel.DPT.IsValidType)
                return Config.Channel.DPT;
            logger.LogDebug($"Determining default DPT for GA {Config.Address} '{Config.Name}' with item type '{Config.ItemType}' the old fashioned way. Consider updating the GA config to specify DPT directly.");
            return ItemToChannelTypeMap[Config.ItemType ?? InvalidItemTypeName].DefaultDPT;
        }
    }

    public virtual string ChannelID
    {
        get
        {
            return Config.Channel.Name;
        }
    }

    /// <summary>
    /// Address tag for the channel's main GA with DPT if non-standard
    /// </summary>
    protected virtual string MainAddressTag
    {
        get
        {
            var useDPT = Config.Channel.DPT;
            if (!(useDPT?.IsValidType ?? false))
            {
                throw new KnxConfigurationException($"Invalid DPT '{Config.DPTs}' for OpenHAB channel of GA {Config.Address} '{Config.Name}'");
            }
            /*
            var dptTag = useDPT?.Equals(DefaultDPT) ?? false
                ? ""
                : $"{(useDPT ?? throw new NotImplementedException($"Need proper DPT for GA {Config.Address}")).DotFormat}:";
                */
            // always put DPT
            var dptTag = $"{DPT.DotFormat}:";
            var readFlag = Config.IsStateOwned ? "<" : "";
            return $"{dptTag}{readFlag}{Config.Address}";
        }
    }

    protected virtual string ComplementoryAddressTag(OHKnxGroupAddress gac)
    {
        var readFlag = gac.IsStateOwned ? "<" : "";
        return $"{readFlag}{gac.Address}";
    }

    protected virtual string FullAddressesTag
    {
        get
        {
            var addrTags = new string[] {
                MainAddressTag,
            }
            .Union(
                (Config.AdditionalGA ?? [])
                .Select(ga => AllGroupAddrConfigs.FirstOrDefault(gac => gac.Address.Equals(ga)))
                .Where(gac => gac != null)
                .Select(gac => ComplementoryAddressTag(gac!))
                ?? []
            );
            return string.Join("+", addrTags);
        }
    }

    /// <summary>
    /// Reflects a "parameter" of an OpenHAB KNX Channel.
    /// </summary>
    protected virtual string ParameterName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Config.Channel.Parameter))
                return Config.Channel.Parameter!;
            logger.LogDebug($"Determining parameter name for GA {Config.Address} '{Config.Name}' with item type '{Config.ItemType}' the old fashioned way. Consider updating the GA config to specify parameter name directly.");
            var sChan = ItemToChannelTypeMap.GetValueOrDefault(Config.ItemType ?? InvalidItemTypeName);
            if (sChan == null || sChan.ChannelType == ChannelType.NotSupported || sChan.ChannelType == ChannelType.Default)
                throw new ArgumentOutOfRangeException($"Missing sub-channel type mapping for GA {Config.Address} '{Config.Name}'");
            return sChan.SubChannel;
        }
    }

    /// <summary>
    /// Writes an OpenHAB KNX Channel definition as part of / within an OpenHAB KNX Thing (not stand-alone).
    /// </summary>
    public virtual void WriteConfig(TextWriter to)
    {
        var channelType = Type.ToString().ToLower();
        to.WriteLine($"        Type {channelType} : {ChannelID} \"{Config.Label}\" [ {ParameterName}=\"{FullAddressesTag}\" ]");
    }
}
