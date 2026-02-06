using System.Text.Json.Serialization;

namespace SRF.Knx.Config.OpenHab.Generate
{

    /// <summary>
    /// To be written all lower case in OpenHAB KNX channel definitions.
    /// All of them have the corresponding "...-control" counterpart wich causes the channel to answer read requests.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ChannelType>))]
    public enum ChannelType
    {
        Default = 0,
        Switch,
        Dimmer,
        Rollershutter,
        Contact,
        Number,
        String,
        DateTime,
        Color,
        NotSupported,
    }
}