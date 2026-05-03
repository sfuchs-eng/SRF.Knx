using System.Buffers.Binary;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Core.DPT;

/// <summary>
/// Provides <see cref="PdtEncoder"/> instances for given property data types (PDTs).
/// It uses a dictionary to map PDT numbers to their corresponding encoder methods.
/// Encoder methods are implemented for common PDTs, and can be extended to support additional PDTs as needed
/// by registration in <see cref="PdtEncodersByNumber"/> or by replacing existing entries.
/// </summary>
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
                Encoder = value => { var b = new byte[2]; BinaryPrimitives.WriteInt16BigEndian(b, value); return new GroupValue(b); },
                Decoder = groupValue => BinaryPrimitives.ReadInt16BigEndian(groupValue.Value)
            }
        },
        { PropertyDataTypeNumber.PDT_UNSIGNED_INT, new PdtEncoder<ushort>
            {
                Encoder = value => { var b = new byte[2]; BinaryPrimitives.WriteUInt16BigEndian(b, value); return new GroupValue(b); },
                Decoder = groupValue => BinaryPrimitives.ReadUInt16BigEndian(groupValue.Value)
            }
        },
        { PropertyDataTypeNumber.PDT_KNX_FLOAT, new PdtEncoder<float>
            {
                // KNX DPT 9 2-byte float format, big-endian on the wire: SEEEEMMMMMMMMMMM
                // Bit 15: Sign (S)
                // Bits 14-11: Exponent (E, 4 bits, 0-15)
                // Bits 10-0: Two's-complement mantissa bits (mBits, 11 bits)
                //   mBits encodes a signed value: M = S==1 ? mBits - 2048 : mBits
                // Value = M * 0.01 * 2^E
                Encoder = (value) => {
                    if (value == 0.0f)
                        return new GroupValue(new byte[2]);

                    // True representable range: M in [-2048, 2047], E in [0, 15]
                    if (value > 670760.96f)
                        throw new ArgumentOutOfRangeException(nameof(value), "Value exceeds maximum representable value for KNX 2-byte float");
                    if (value < -671088.64f)
                        throw new ArgumentOutOfRangeException(nameof(value), "Value is below minimum representable value for KNX 2-byte float");

                    // Scale signed value into mantissa range [-2048, 2047]
                    int exponent = 0;
                    float scaledValue = value / 0.01f;

                    while ((scaledValue > 2047f || scaledValue < -2048f) && exponent < 15)
                    {
                        scaledValue /= 2.0f;
                        exponent++;
                    }

                    int m = Math.Clamp((int)Math.Round(scaledValue), -2048, 2047);
                    int s = m < 0 ? 1 : 0;
                    int mBits = m < 0 ? m + 2048 : m;

                    ushort knxFloat = (ushort)(s << 15 | exponent << 11 | mBits);
                    // Emit big-endian (KNX wire order)
                    return new GroupValue([(byte)(knxFloat >> 8), (byte)(knxFloat & 0xFF)]);
                },
                Decoder = groupValue => {
                    // Read big-endian
                    ushort knxFloat = (ushort)((groupValue.Value[0] << 8) | groupValue.Value[1]);

                    if (knxFloat == 0)
                        return 0.0f;

                    int s = (knxFloat >> 15) & 0x01;
                    int exponent = (knxFloat >> 11) & 0x0F;
                    int mBits = knxFloat & 0x07FF;
                    // Two's-complement: when S=1 the 11-bit field represents a negative number
                    int m = s == 1 ? mBits - 2048 : mBits;

                    return m * 0.01f * (float)Math.Pow(2, exponent);
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
                Encoder = value => { var b = new byte[4]; BinaryPrimitives.WriteInt32BigEndian(b, value); return new GroupValue(b); },
                Decoder = groupValue => BinaryPrimitives.ReadInt32BigEndian(groupValue.Value)
            }
        },
        { PropertyDataTypeNumber.PDT_UNSIGNED_LONG, new PdtEncoder<uint>
            {
                Encoder = value => { var b = new byte[4]; BinaryPrimitives.WriteUInt32BigEndian(b, value); return new GroupValue(b); },
                Decoder = groupValue => BinaryPrimitives.ReadUInt32BigEndian(groupValue.Value)
            }
        },
        { PropertyDataTypeNumber.PDT_FLOAT, new PdtEncoder<float>
            {
                Encoder = value => { var b = new byte[4]; BinaryPrimitives.WriteSingleBigEndian(b, value); return new GroupValue(b); },
                Decoder = groupValue => BinaryPrimitives.ReadSingleBigEndian(groupValue.Value)
            }
        },
        { PropertyDataTypeNumber.PDT_DOUBLE, new PdtEncoder<double>
            {
                Encoder = value => { var b = new byte[8]; BinaryPrimitives.WriteDoubleBigEndian(b, value); return new GroupValue(b); },
                Decoder = groupValue => BinaryPrimitives.ReadDoubleBigEndian(groupValue.Value)
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
        { PropertyDataTypeNumber.PDT_GENERIC_01, new PdtEncoder<byte>
            {
                Encoder = value => new GroupValue([value]),
                Decoder = groupValue => groupValue.Value[0]
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
                Encoder = value => { var b = new byte[2]; BinaryPrimitives.WriteUInt16BigEndian(b, value); return new GroupValue(b); },
                Decoder = groupValue => BinaryPrimitives.ReadUInt16BigEndian(groupValue.Value)
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
