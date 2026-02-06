using Microsoft.Extensions.Logging;
using SRF.Knx.Config.OpenHab.BaseConfig;

namespace SRF.Knx.Config.OpenHab.Generate.v5;

public class ConfigGeneratorProviderVersion5(ILoggerFactory loggerFactory) : IConfigGeneratorProvider
{
    private readonly ILoggerFactory loggerFactory = loggerFactory;

    public IBridge GetBridgeGenerator(BridgeConfig bridgeConfig, IEnumerable<KnxThingConfig> thingsConfig)
        => new Bridge(
            bridgeConfig,
            thingsConfig,
            this,
            loggerFactory
        );

    public IChannel GetChannelGenerator(OHKnxGroupAddress gaConfig, IEnumerable<OHKnxGroupAddress> allGAConfigs)
        => new Channel(
            gaConfig,
            allGAConfigs,
            loggerFactory
        );

    public IItem GetItemGenerator(IBridge bridge, IThing thing, IChannel channel)
        => new Item(
            bridge as Bridge ?? throw new ArgumentException("require v5 Bridge type", nameof(bridge)),
            thing as Thing ?? throw new ArgumentException("require v5 Thing type", nameof(thing)),
            channel as Channel ?? throw new ArgumentException("require v5 Channel type", nameof(channel)),
            loggerFactory.CreateLogger<Item>()
        );

    public IThing GetThingGenerator(KnxThingConfig thingConfig)
        => new Thing(
            thingConfig,
            this,
            loggerFactory
        );
}
