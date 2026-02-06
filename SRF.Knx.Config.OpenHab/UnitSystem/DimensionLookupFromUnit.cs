using System;
using System.Text.Json.Serialization;
using SRF.Knx.Config.OpenHab.DptMapping;

namespace SRF.Knx.Config.OpenHab.UnitSystem;

public class DimensionLookupFromUnit
{
    [JsonIgnore]
    public OpenHabDimension? Dimension;

    [JsonPropertyName("Dimension")]
    public string JsonDimension
    {
        get => Dimension?.ToString() ?? string.Empty;
        set
        {
            if (Enum.TryParse<OpenHabDimension>(value, out var dim))
            {
                Dimension = dim;
                JsonDimensionParsed = null;
            }
            else
            {
                Dimension = null;
                JsonDimensionParsed = value;
            }
        }
    }

    /// <summary>
    /// Parsed dimension from JSON property Dimension in case of invalid value.
    /// </summary>
    [JsonIgnore]
    public string? JsonDimensionParsed { get; set; }
    
    public string[] Units { get; set; } = [];
}
