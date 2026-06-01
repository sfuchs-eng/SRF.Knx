using System.Text.Json.Serialization;
using HomeCompanion.Abstractions.Serialization;

namespace SRF.Knx.Config;

[JsonConverter(typeof(CommaSeparatedFlagsEnumJsonConverter<KnxObjectBusCommunication>))]
[Flags]
public enum KnxObjectBusCommunication
{
    /// <summary>
    /// Allows other devices on the bus to request the current value of this object via a GroupValue_Read telegram. The device will respond with a GroupValue_Response.
    /// </summary>
    Read = 1,

    /// <summary>
    /// Allows the object’s value to be changed by other devices via a GroupValue_Write telegram. This is typical for actuators (e.g., a relay receiving an "ON" command).
    /// </summary>
    Write = 2,

    /// <summary>
    /// Allows the device to spontaneously send its value to the bus (e.g., a sensor sending a temperature update or a switch sending a toggle command).
    /// </summary>
    Transmit = 4,

    /// <summary>
    /// If set, the object will update its internal value when it sees a GroupValue_Response on the bus for its group address, even if it didn't request the data.
    /// </summary>
    Update = 8,

    /// <summary>
    /// (Less common) Forces the device to send a Read Request upon power-up to synchronize its state with the rest of the installation.
    /// </summary>
    Initialize = 16,

    /// <summary>
    /// KNX: The "Master Switch." If this is not set, the object cannot communicate with the bus at all. It must be active for any other flags to function.
    /// Implemented differently: In our implementation, we assume that if any communication flags are set, the object is intended to communicate with the bus accordingly, so we treat the presence of any flags as implicitly including "Master Switch" functionality. If no flags are set, the object is considered non-communicative.
    /// </summary>
    Communication = Read | Write | Transmit | Update | Initialize,
}