using SRF.Knx.Core.Master;

namespace SRF.Knx.Core.DPT;

public interface IPdtEncoderFactory
{
    PdtEncoder GetPdtEncoder(PropertyDataType pdt);
}

public class PdtEncoderFactory : IPdtEncoderFactory
{
    public PdtEncoder GetPdtEncoder(PropertyDataType pdt)
    {
        throw new NotImplementedException();
    }

    public Dictionary<PropertyDataTypeNumber, PdtEncoder> PdtEncodersByNumber = new()
    {
        { PropertyDataTypeNumber.PDT_CONTROL, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_CHAR, new PdtEncoder<sbyte>
            {
                Encoder = value => new GroupValue(new[] { (byte)value }),
                Decoder = groupValue => (sbyte)groupValue.Value[0]
            }
        },
        { PropertyDataTypeNumber.PDT_UNSIGNED_CHAR, new PdtEncoder<byte>
            {
                Encoder = value => new GroupValue(new[] { value }),
                Decoder = groupValue => groupValue.Value[0]
            }
        },
        { PropertyDataTypeNumber.PDT_INT, new PdtEncoder<short>
            {
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToInt16(groupValue.Value, 0)
            }
        },
        { PropertyDataTypeNumber.PDT_UNSIGNED_INT, new PdtEncoder<ushort>
            {
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToUInt16(groupValue.Value, 0)
            }
        },
        { PropertyDataTypeNumber.PDT_KNX_FLOAT, new PdtEncoder<float>
            {
                Encoder = value => new GroupValue(BitConverter.GetBytes(value).Take(2).ToArray()),
                Decoder = groupValue => BitConverter.ToSingle(groupValue.Value.Concat(new byte[2]).ToArray(), 0)
            }
        },
        { PropertyDataTypeNumber.PDT_DATE, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_TIME, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_LONG, new PdtEncoder<int>
            {
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToInt32(groupValue.Value, 0)
            }
        },
        { PropertyDataTypeNumber.PDT_UNSIGNED_LONG, new PdtEncoder<uint>
            {
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToUInt32(groupValue.Value, 0)
            }
        },
        { PropertyDataTypeNumber.PDT_FLOAT, new PdtEncoder<float>
            {
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToSingle(groupValue.Value, 0)
            }
        },
        { PropertyDataTypeNumber.PDT_DOUBLE, new PdtEncoder<double>
            {
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToDouble(groupValue.Value, 0)
            }
        },
        { PropertyDataTypeNumber.PDT_CHAR_BLOCK, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_POLL_GROUP_SETTINGS, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_SHORT_CHAR_BLOCK, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_DATE_TIME, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_VARIABLE_LENGTH, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_01, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_02, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_03, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_04, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_05, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_06, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_07, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_08, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_09, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_10, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_11, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_12, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_13, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_14, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_15, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_16, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_17, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_18, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_19, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_20, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_25, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_26, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_27, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_28, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_29, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_2A, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_2B, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_2C, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_2D, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_2E, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_UTF_8, new PdtEncoder<string>
            {
                Encoder = value => new GroupValue(System.Text.Encoding.UTF8.GetBytes(value)),
                Decoder = groupValue => System.Text.Encoding.UTF8.GetString(groupValue.Value)
            }
        },
        { PropertyDataTypeNumber.PDT_VERSION, new PdtEncoder<ushort>
            {
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToUInt16(groupValue.Value, 0)
            }
        },
        { PropertyDataTypeNumber.PDT_ALARM_INFO, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_BINARY_INFORMATION, new PdtEncoder<bool>
            {
                Encoder = value => new GroupValue(new[] { (byte)(value ? 1 : 0) }),
                Decoder = groupValue => groupValue.Value[0] != 0
            }
        },
        { PropertyDataTypeNumber.PDT_BITSET8, new PdtEncoder<byte>
            {
                Encoder = value => new GroupValue(new[] { value }),
                Decoder = groupValue => groupValue.Value[0]
            }
        },
        { PropertyDataTypeNumber.PDT_BITSET16, new PdtEncoder<ushort>
            {
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToUInt16(groupValue.Value, 0)
            }
        },
        { PropertyDataTypeNumber.PDT_ENUM8, new PdtEncoder<byte>
            {
                Encoder = value => new GroupValue(new[] { value }),
                Decoder = groupValue => groupValue.Value[0]
            }
        },
        { PropertyDataTypeNumber.PDT_SCALING, new PdtEncoder<byte>
            {
                Encoder = value => new GroupValue(new[] { value }),
                Decoder = groupValue => groupValue.Value[0]
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_37, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_38, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_39, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_3A, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_3B, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_NE_VL, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_NE_FL, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_FUNCTION, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        },
        { PropertyDataTypeNumber.PDT_ESCAPE, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => groupValue.Value
            }
        }
    };
}
