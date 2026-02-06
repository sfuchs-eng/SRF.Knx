using SRF.Knx.Config.OpenHab.BaseConfig;

namespace SRF.Knx.Config.OpenHab.Generate;

public interface IConfigGeneratorProvider
{
    IBridge GetBridgeGenerator(
        BridgeConfig bridgeConfig,
        IEnumerable<KnxThingConfig> thingsConfig);

    IThing GetThingGenerator(KnxThingConfig thingConfig);

    IChannel GetChannelGenerator(OHKnxGroupAddress gaConfig, IEnumerable<OHKnxGroupAddress> allGAConfigs);

    IItem GetItemGenerator(IBridge bridge, IThing thing, IChannel channel);
}
