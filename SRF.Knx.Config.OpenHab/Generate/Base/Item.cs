using Microsoft.Extensions.Logging;
using SRF.Knx.Config.Exceptions;
using SRF.Knx.Config.OpenHab.BaseConfig;

namespace SRF.Knx.Config.OpenHab.Generate.Base;

public class Item<TBridge, TThing,TChannel>(TBridge bridge, TThing thing, TChannel channel, ILogger<Item<TBridge, TThing,TChannel>> logger) : IConfigGenerator, IItem
    where TBridge : Bridge<TThing,TChannel>
    where TThing : Thing<TChannel>
    where TChannel : Channel
{
    private string BindingId { get => bridge.Config.BindingId; }
    private string BridgeName { get => bridge.Config.Name; }
    private readonly TBridge bridge = bridge;
    private readonly ILogger<Item<TBridge,TThing,TChannel>> logger = logger;

    public TThing Thing { get; private init; } = thing;

    public TChannel Channel { get; private init; } = channel;

    public OHKnxGroupAddress Config { get; private init; } = channel.Config;

    public static bool HasContent(string?[]? configElement)
    {
        return configElement != null && configElement.Any(c => !string.IsNullOrEmpty(c));
    }

    protected virtual string GetChannelConfig() {
        return $"{{ channel=\"{BindingId}:device:{BridgeName}:{Thing.Config.Name}:{Channel.ChannelID}\" }}";
    }

    public virtual void WriteConfig(TextWriter to)
    {
        var itemType = Config.ItemType ?? throw new KnxConfigurationException($"{Config.Address}: OpenHAB item type needs to be defined.");
        var itemName = Config.Name;
        var label = Config.Label;
        var iconTag = !string.IsNullOrEmpty(Config.Icon)
            ? $" <{Config.Icon}>"
            : "";
        var groupTag = HasContent(Config.Groups)
            ? $" ({string.Join(", ", Config.Groups)})"
            : "";
        to.WriteLine($"{itemType} {itemName} \"{label}\"{iconTag} {groupTag} {GetChannelConfig()}");
    }
}
