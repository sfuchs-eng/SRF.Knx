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

    protected virtual string GetChannelConfig(string? unitToken = null)
    {
        unitToken = !string.IsNullOrEmpty(unitToken) ? unitToken : string.Empty;
        return $"{{ channel=\"{BindingId}:device:{BridgeName}:{Thing.Config.Name}:{Channel.ChannelID}\"{unitToken} }}";
    }

    /// <summary>
    /// For which Dimenions OpenHAB's unit system shall NOT be used.
    /// </summary>
    /// <remarks>
    /// The "AccordingDpt" dimension is supposed to be resolved at this point, but in case it isn't, we should not apply unit system transformations.
    /// The "Dimensionless" dimension is explicitly unitless and should not be transformed either.
    /// Other dimensions that might be unitless or have non-standard units could be added here as needed.
    /// </remarks>
    readonly HashSet<DptMapping.OpenHabDimension> unitSystemExemptDimensions = [
        DptMapping.OpenHabDimension.Dimensionless,
        DptMapping.OpenHabDimension.AccordingDpt
    ];

    /// <summary>
    /// Translate from KNX units to OpenHAB units for Number items.
    /// Some units do not match 1:1. Put those into this dictionary.
    /// If a unit is not in the dictionary, it is assumed that it can be used as-is.
    /// </summary>
    readonly Dictionary<string, string> unitSystemTransformUnit = new()
    {
        { "Lux", "lx" },
        { "Percentage", "%" },
    };

    bool ProduceUnitSystemTokens(out string itemTypeSuffix, out string itemChannelLinkUnitTag)
    {
        itemTypeSuffix = string.Empty;
        itemChannelLinkUnitTag = string.Empty;

        if (!"Number".Equals(Config.ItemType, StringComparison.OrdinalIgnoreCase))
            return false;

        var dimension = Channel.Config.Channel.Dimension;
        if (dimension == null || unitSystemExemptDimensions.Contains(dimension.Value))
            return false;

        itemTypeSuffix = $":{dimension.Value}";

        var knxUnit = Channel.Config.Channel.KnxUnit;
        if ( string.IsNullOrEmpty(knxUnit))
            return false;

        var openHabUnit = unitSystemTransformUnit.TryGetValue(knxUnit, out var transformedUnit) ? transformedUnit : knxUnit;
        itemChannelLinkUnitTag = $", unit=\"{openHabUnit}\"";

        return true;
    }

    public virtual void WriteConfig(TextWriter to)
    {
        var itemType = Config.ItemType ?? throw new KnxConfigurationException($"{Config.Address}: OpenHAB item type needs to be defined.");
        var dimension = Channel.Config.Channel.Dimension;
        var exemptDimensions = new HashSet<DptMapping.OpenHabDimension> { DptMapping.OpenHabDimension.Dimensionless, DptMapping.OpenHabDimension.AccordingDpt };
        var useUnitSystem = ProduceUnitSystemTokens(out var itemTypeSuffix, out var itemChannelLinkUnitTag);
        // Append Dimension for numeric types?
        if (useUnitSystem)
        {
            itemType += itemTypeSuffix;
        }
        var itemName = Config.Name;
        var label = Config.Label;
        var iconTag = !string.IsNullOrEmpty(Config.Icon)
            ? $" <{Config.Icon}>"
            : "";
        var groupTag = HasContent(Config.Groups)
            ? $" ({string.Join(", ", Config.Groups)})"
            : "";
        to.WriteLine($"{itemType} {itemName} \"{label}\"{iconTag} {groupTag} {GetChannelConfig(useUnitSystem ? itemChannelLinkUnitTag : null)}");
    }
}
