using Microsoft.Extensions.Logging;
using SRF.Knx.Config.OpenHab.BaseConfig;

namespace SRF.Knx.Config.OpenHab.Generate.v5;

public class Bridge(
    BridgeConfig bridgeConfig,
    IEnumerable<KnxThingConfig> thingsConfig,
    IConfigGeneratorProvider configGeneratorProvider,
    ILoggerFactory loggerFactory
        ) : Generate.Base.Bridge<Thing, Channel>(bridgeConfig, thingsConfig, configGeneratorProvider, loggerFactory)
{
}
