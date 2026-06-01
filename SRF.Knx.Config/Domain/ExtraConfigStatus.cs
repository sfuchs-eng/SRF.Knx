using System.Text.Json.Serialization;
using HomeCompanion.Abstractions.Serialization;

namespace SRF.Knx.Config.Domain;

[JsonConverter(typeof(CommaSeparatedFlagsEnumJsonConverter<ExtraConfigStatus>))]
[Flags]
public enum ExtraConfigStatus
{
    /// <summary>
    /// Programmatically added. Ok to change automatically, reflecting changes in the ETS Group Address export.
    /// </summary>
    Automatic = 1 << 0,

    /// <summary>
    /// Manual edits, do not override with automatic changes.
    /// </summary>
    Manual = 1 << 1,

    /// <summary>
    /// There's no such Group Address in the ETS Group Address export
    /// </summary>
    Surplus = 1 << 2,

    /// <summary>
    /// Newly added automatically
    /// </summary>
    Fresh = 1 << 3,

    /// <summary>
    /// Automated changes were applied
    /// </summary>
    Changed = 1 << 4,
}