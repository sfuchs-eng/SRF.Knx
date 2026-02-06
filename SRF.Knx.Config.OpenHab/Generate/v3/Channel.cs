using System.Data;
using Microsoft.Extensions.Logging;
using SRF.Knx.Config.ETS5;
using SRF.Knx.Config.OpenHab.BaseConfig;
using SRF.Knx.Config.OpenHab.Templating;

namespace SRF.Knx.Config.OpenHab.Generate.v3;

/// <summary>
/// Uses a GA <--> Channel 1:1 scheme where a Channel has exactly 1 parameter (sub-channel) only.
/// OpenHAB in general: Thing --1:n--> Channel --1:n--> Sub-Channels(as parameters to Channel) --1:n--> Group Addresses
/// OpenHAB KNX: ignoring Thing and using sub-channels/parameters on Channels instead.
/// </summary>
public class Channel(
    OHKnxGroupAddress gaConfig,
    IEnumerable<OHKnxGroupAddress> allGAConfigs,
    ILoggerFactory loggerFactory
        ) : Generate.Base.Channel(gaConfig, allGAConfigs, loggerFactory)
{
}