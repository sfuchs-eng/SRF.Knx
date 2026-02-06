using System.Text.Json.Serialization;

namespace SRF.Knx.Config.ETS5;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GroupAddressSecurity
{
    Off,
    On,
}
