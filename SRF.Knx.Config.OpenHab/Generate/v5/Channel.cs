using Microsoft.Extensions.Logging;
using SRF.Knx.Config.OpenHab.BaseConfig;

namespace SRF.Knx.Config.OpenHab.Generate.v5;

public class Channel(
    OHKnxGroupAddress gaConfig,
    IEnumerable<OHKnxGroupAddress> allGAConfigs,
    ILoggerFactory loggerFactory
        ) : Generate.Base.Channel(gaConfig, allGAConfigs, loggerFactory)
{
    /// <summary>
    /// Writes an OpenHAB v5 KNX Channel definition with Dimension configuration support.
    /// Includes the dimension parameter if available from the channel configuration.
    /// </summary>
    public override void WriteConfig(TextWriter to)
    {
        var channelType = Type.ToString().ToLower();
        var dimensionParam = Config.Channel.Dimension != null
            ? $", dimension=\"{Config.Channel.Dimension}\""
            : "";
        to.WriteLine($"        Type {channelType} : {ChannelID} \"{Config.Label}\" [ {ParameterName}=\"{FullAddressesTag}\"{dimensionParam} ]");
    }
}