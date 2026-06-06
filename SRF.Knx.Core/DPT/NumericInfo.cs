using System.Text.Json.Serialization;

namespace SRF.Knx.Core.DPT;

public class NumericInfo
{
    public required double MinValue { get; init; }
    public required double MaxValue { get; init; }

    /// <summary>
    /// Originates from the PDT Encoder's preferred CLS type,
    /// <see cref="IPdtEncoderFactory"/>, respectively <see cref="PdtEncoder{T}"/> and <see cref="PdtEncoderFactory"/>.
    /// </summary>
    public required Type Type { get; init; }

    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Bus raw value * coefficient = physical value for display and application logic.
    /// </summary>
    /// <value></value>
    public double? Coefficient { get; set; }

    [JsonIgnore]
    public bool IsScaled => Coefficient != null && !(1.0).Equals(Coefficient.Value) && !0.0.Equals(Coefficient.Value);
}
