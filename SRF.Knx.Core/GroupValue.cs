using System;

namespace SRF.Knx.Core;

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
