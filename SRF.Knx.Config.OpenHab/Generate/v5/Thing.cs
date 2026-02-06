using Microsoft.Extensions.Logging;
using SRF.Knx.Config.OpenHab.BaseConfig;

namespace SRF.Knx.Config.OpenHab.Generate.v5;

public class Thing(
    KnxThingConfig thingConfig,
    IConfigGeneratorProvider configGeneratorProvider,
    ILoggerFactory loggerFactory
        ) : Generate.Base.Thing<Channel>(thingConfig, configGeneratorProvider, loggerFactory)
{
}