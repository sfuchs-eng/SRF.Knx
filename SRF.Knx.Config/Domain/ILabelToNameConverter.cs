using System;
using SRF.Knx.Config.ETS5;

namespace SRF.Knx.Config.Domain;

public interface ILabelToNameConverter
{
    /// <summary>
    /// Converts a KNX group address label into thing, channel and parameter names suitable for .NET identifiers.
    /// It follows the principle of "one channel per KNX group address" unless separator tokens indicate differently.
    /// </summary>
    /// <param name="gac">the Group Address' ETS export record</param>
    /// <param name="thing">the thing name. Equals the GA name in case there is no channel or parameter discovered</param>
    /// <param name="channel">optional, OpenHAB channel name. May be linked to an OpenHAB item</param>
    /// <param name="parameter">optional, an OpenHAB channel's parameter the group address shall be mapped to</param>
    /// <param name="valueFormat">optional, a string that the GA label carried in []. Expected is e.g. a value formatting specifier like %.1f and optionally a tailing unit separated by a white space character; e.g. &quot;%.1f m/s&quot;</param>
    void GetName(EtsGroupAddressConfig gac, out string thing, out string? channel, out string? parameter, out string? valueFormat);

    /// <summary>
    /// Converts a KNX group address label into a name suitable for .NET identifiers.
    /// It follows the principle of "one channel per KNX group address" unless separator tokens indicate differently.
    /// </summary>
    /// <returns>The GA entity name where Thing, Channel and Parameter are concatenated</returns>
    string GetName(EtsGroupAddressConfig gac);

    /// <summary>
    /// Extracts the Thing name from the GA label.
    /// </summary>
    string GetThingName(EtsGroupAddressConfig gac);
}
