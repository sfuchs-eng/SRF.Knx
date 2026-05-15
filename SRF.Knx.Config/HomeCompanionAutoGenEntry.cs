using System.Text.Json.Serialization;

namespace SRF.Knx.Config;

/// <summary>
/// A single entry in <c>HomeCompanionKnxAutoGen.json</c>, mapping a KNX group address to its
/// generated C# property name and KNX DPT.
/// </summary>
public class HomeCompanionAutoGenEntry
{
    /// <summary>The C# property name to emit on <c>KnxValues</c>.</summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable label for the property's XML doc summary, if set.
    /// Typically derived from the ETS export XML label for the group address.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Human-readable description for the property's XML doc remarks, if set.
    /// Typically derived from the ETS export XML description for the group address.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// KNX Data Point Type string (e.g. <c>"DPT-9"</c>, <c>"DPST-9-1"</c>),
    /// or <see langword="null"/> if unset.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Dpt { get; set; }

    /// <summary>
    /// Optional OpenHAB item name for initial state retrieval during value initialization, if set.
    /// </summary>
    public string? OpenHabItemName { get; set; }
}
