using System.Text.Json.Serialization;

namespace SRF.Knx.Config.OpenHab.DptMapping;

public partial class DptMappingLookupItem
{
    /// <summary>
    /// Ad-hoc solution, might need to be moved to another place NS wise.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ChannelStereotype>))]
    public enum ChannelStereotype
    {
        /// <summary>
        /// One or multiple physical actuators commanded by multiple others
        /// </summary>
        Actuator,

        /// <summary>
        /// Write/read n:m relationship, e.g. system state inputs.
        /// </summary>
        Parameter,

        /// <summary>
        /// Occurence of something, e.g. Triggers, means something each time sent even if the same value is sent repeatedly.
        /// </summary>
        Event,

        /// <summary>
        /// Sensor providing measurement values. Normally not writable
        /// unless OpenHAB itself is the owner towards the KNX bus.
        /// </summary>
        Sensor,

        /// <summary>
        /// Match all
        /// </summary>
        Any,
    }
}
