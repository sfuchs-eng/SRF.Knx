using System.Text.Json.Serialization;

namespace SRF.Knx.Config.OpenHab.BaseConfig;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BridgeType
{
    ROUTER,
    TUNNEL
}
