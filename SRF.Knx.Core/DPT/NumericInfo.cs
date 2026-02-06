namespace SRF.Knx.Core.DPT;

public class NumericInfo
{
    public required double MinValue { get; init; }
    public required double MaxValue { get; init; }
    public required Type Type { get; init; }
    public string Unit { get; set; } = string.Empty;
}
