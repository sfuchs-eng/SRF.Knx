using Microsoft.Extensions.Logging;

namespace SRF.Knx.Config.OpenHab.Generate.v5;

public class Item(
    Bridge bridge,
    Thing thing,
    Channel channel,
    ILogger<Item> logger
            ) : Generate.Base.Item<Bridge,Thing,Channel>(bridge, thing, channel, logger)
{
}