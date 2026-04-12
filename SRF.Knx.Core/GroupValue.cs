namespace SRF.Knx.Core;

/// <summary>
/// Represents the raw value of a KNX group address' object, as a byte array.
/// This is the format used for communication on the KNX bus, and is the format that
/// DPTs convert to and from their typed values via <see cref="PdtEncoder"/>.
/// Use an <see cref="IPdtEncoderFactory"/> to obtain the appropriate encoder for a given PDT.
/// The PDT in turn is determined by the DPT, which can be obtained from an <see cref="IDptFactory"/>
/// using the DPT's main and sub number.
/// </summary>
public class GroupValue
{
    public byte[] Value { get; set; } = [];

    public GroupValue() { }

    public GroupValue(byte[] value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Convert.ToHexString(Value);
    }
}
