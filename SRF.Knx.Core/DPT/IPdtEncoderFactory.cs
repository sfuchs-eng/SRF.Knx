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
        if (PdtEncodersByNumber.TryGetValue(pdt.Number, out var encoder))
        {
            return encoder;
        }

        throw new NotSupportedException($"No encoder found for PDT number {pdt.Number}");
    }

    private static readonly Func<string, int, GroupValue> FixedLengthAsciiStringEncoder = (value, length) =>
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(value);
        if (bytes.Length > length)
            bytes = bytes.Take(length).ToArray(); // Truncate to specified length if necessary
        else if (bytes.Length < length)
            bytes = bytes.Concat(new byte[length - bytes.Length]).ToArray(); // Pad with zeros if less than specified length
        return new GroupValue(bytes); // Assuming a fixed length for the char block
    };

    private static readonly Func<GroupValue, string> FixedLengthAsciiStringDecoder = groupValue =>
        System.Text.Encoding.ASCII.GetString(groupValue.Value).TrimEnd('\0'); // Remove padding zeros

    private static readonly Func<byte[], int, GroupValue> FixedLengthByteArrayEncoder = (value, length) =>
    {
        if (value.Length > length)
            value = value.Take(length).ToArray(); // Truncate to specified length if necessary
        else if (value.Length < length)
            value = value.Concat(new byte[length - value.Length]).ToArray(); // Pad with zeros if less than specified length
        return new GroupValue(value); // Assuming a fixed length for the char block
    };

    private static readonly Func<GroupValue, byte[]> FixedLengthByteArrayDecoder = groupValue =>
        groupValue.Value; // Assuming the value is already the correct length, or will be handled by the caller

    public Dictionary<PropertyDataTypeNumber, PdtEncoder> PdtEncodersByNumber { get; init; } = new()
    {
        { PropertyDataTypeNumber.PDT_CONTROL, new PdtEncoder<byte[]>
            {
                Encoder = value => new GroupValue(value),
                Decoder = groupValue => [ groupValue.Value.First() ]
            }
        },
        { PropertyDataTypeNumber.PDT_CHAR, new PdtEncoder<sbyte>
            {
                Encoder = value => new GroupValue([(byte)value]),
                Decoder = groupValue => (sbyte)groupValue.Value[0]
            }
        },
        { PropertyDataTypeNumber.PDT_UNSIGNED_CHAR, new PdtEncoder<byte>
            {
                Encoder = value => new GroupValue([value]),
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
                //TODO: this is not a standard float encoding, but the KNX specific 2 byte float encoding. We need to implement the conversion to and from this format.
                //The KNX 2 byte float format is a custom format that uses 1 bit for the sign, 4 bits for the exponent, and 11 bits for the mantissa. The value is calculated as: (-1)^sign * (1 + mantissa/2048) * 2^(exponent-15)
                Encoder = (value) => {
                    // Convert the float to the KNX 2 byte float format
                    uint intValue = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
                    uint sign = (intValue >> 31) & 0x1;
                    uint exponent = ((intValue >> 23) & 0xFF) - 127 + 15; // Adjust exponent from IEEE 754 to KNX
                    uint mantissa = (intValue & 0x7FFFFF) >> 11; // Get the top 11 bits of the mantissa

                    if (exponent < 0)
                    {
                        // Underflow, return zero
                        return new GroupValue(new byte[2]);
                    }
                    else if (exponent > 0xF)
                    {
                        // Overflow, return max value
                        exponent = 0xF;
                        mantissa = 0x7FF; // Max mantissa
                    }

                    ushort knxFloat = (ushort)((sign << 15) | (exponent << 11) | mantissa);
                    return new GroupValue(BitConverter.GetBytes(knxFloat));
                },
                Decoder = groupValue => {
                     // Convert the KNX 2 byte float format back to a standard float
                     uint knxFloat = BitConverter.ToUInt16(groupValue.Value, 0);
                     uint sign = (knxFloat >> 15) & 0x1;
                     uint exponent = ((knxFloat >> 11) & 0xF) - 15 + 127; // Adjust exponent from KNX to IEEE 754
                     uint mantissa = (knxFloat & 0x7FF) << 11; // Shift mantissa back to its position

                     uint intValue = (sign << 31) | (exponent << 23) | mantissa;
                     return BitConverter.ToSingle(BitConverter.GetBytes(intValue), 0);
                }
            }
        },
        { PropertyDataTypeNumber.PDT_DATE, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_DATE encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_DATE decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_TIME, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_TIME encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_TIME decoding not implemented yet")
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
        { PropertyDataTypeNumber.PDT_CHAR_BLOCK, new PdtEncoder<string>
            {
                Encoder = value => FixedLengthAsciiStringEncoder(value, 10),
                Decoder = groupValue => FixedLengthAsciiStringDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_POLL_GROUP_SETTINGS, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_POLL_GROUP_SETTINGS encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_POLL_GROUP_SETTINGS decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_SHORT_CHAR_BLOCK, new PdtEncoder<string>
            {
                Encoder = value => FixedLengthAsciiStringEncoder(value, 5),
                Decoder = groupValue => FixedLengthAsciiStringDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_DATE_TIME, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_DATE_TIME encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_DATE_TIME decoding not implemented yet")
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
                Encoder = value => FixedLengthByteArrayEncoder(value, 1),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_02, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 2),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_03, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 3),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_04, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 4),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_05, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 5),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_06, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 6),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_07, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 7),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_08, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 8),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_09, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 9),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_10, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 10),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_11, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 11),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_12, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 12),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_13, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 13),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_14, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 14),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_15, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 15),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_16, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 16),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_17, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 17),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_18, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 18),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_19, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 19),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_GENERIC_20, new PdtEncoder<byte[]>
            {
                Encoder = value => FixedLengthByteArrayEncoder(value, 20),
                Decoder = groupValue => FixedLengthByteArrayDecoder(groupValue)
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_25, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_25 encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_25 decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_26, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_26 encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_26 decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_27, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_27 encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_27 decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_28, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_28 encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_28 decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_29, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_29 encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_29 decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_2A, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_2A encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_2A decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_2B, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_2B encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_2B decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_2C, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_2C encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_2C decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_2D, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_2D encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_2D decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_2E, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_2E encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_2E decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_UTF_8, new PdtEncoder<string>
            {
                Encoder = value => new GroupValue(System.Text.Encoding.UTF8.GetBytes(value)),
                Decoder = groupValue => System.Text.Encoding.UTF8.GetString(groupValue.Value)
            }
        },
        { PropertyDataTypeNumber.PDT_VERSION, new PdtEncoder<PdtVersion>
            {
                Encoder = value => new GroupValue(value.GetBytes()),
                Decoder = groupValue => new PdtVersion(groupValue.Value)
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
                Encoder = value => new GroupValue([(byte)(value ? 1 : 0)]),
                Decoder = groupValue => (groupValue.Value[0] & 1) != 0
            }
        },
        { PropertyDataTypeNumber.PDT_BITSET8, new PdtEncoder<byte>
            {
                Encoder = value => new GroupValue([value]),
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
                Encoder = value => new GroupValue([value]),
                Decoder = groupValue => groupValue.Value[0]
            }
        },
        { PropertyDataTypeNumber.PDT_SCALING, new PdtEncoder<byte>
            {
                Encoder = value => new GroupValue([value]),
                Decoder = groupValue => groupValue.Value[0]
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_37, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_37 encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_37 decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_38, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_38 encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_38 decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_39, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_39 encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_39 decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_3A, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_3A encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_3A decoding not implemented yet")
            }
        },
        { PropertyDataTypeNumber.PDT_RESERVED_3B, new PdtEncoder<byte[]>
            {
                Encoder = value => throw new NotImplementedException("PDT_RESERVED_3B encoding not implemented yet"),
                Decoder = groupValue => throw new NotImplementedException("PDT_RESERVED_3B decoding not implemented yet")
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
