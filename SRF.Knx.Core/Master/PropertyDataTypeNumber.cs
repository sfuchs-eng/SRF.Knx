namespace SRF.Knx.Core.Master;

/// <summary>
/// KNX Property Data Type enumeration defining data types for group address value encodings.
/// </summary>
public enum PropertyDataTypeNumber
{
    /// <summary>
    /// Type length: 1 octet read/10 octet write, Format: usage dependent
    /// </summary>
    PDT_CONTROL = 0,

    /// <summary>
    /// Type length: 1 octet, Format: V8 / DPT 6.010
    /// </summary>
    PDT_CHAR = 1,

    /// <summary>
    /// Type length: 1 octet, Format: U8/ DPT 5.010
    /// </summary>
    PDT_UNSIGNED_CHAR = 2,

    /// <summary>
    /// Type length: 2 octets, Format: V16 / DPT 8.001
    /// </summary>
    PDT_INT = 3,

    /// <summary>
    /// Type length: 2 octets, Format: U16 / DPT 7.001
    /// </summary>
    PDT_UNSIGNED_INT = 4,

    /// <summary>
    /// Type length: 2 octets, Format: F16 / DPT 9
    /// </summary>
    PDT_KNX_FLOAT = 5,

    /// <summary>
    /// Type length: 3 octets, Format: r3N5r4N4r1U7 / DPT 11
    /// </summary>
    PDT_DATE = 6,

    /// <summary>
    /// Type length: 3 octets, Format: N3N5r2N6r2N6 / DPT 10
    /// </summary>
    PDT_TIME = 7,

    /// <summary>
    /// Type length: 4 octets, Format: V32 / DPT 13.001
    /// </summary>
    PDT_LONG = 8,

    /// <summary>
    /// Type length: 4 octets, Format: U32 / DPT 12.001
    /// </summary>
    PDT_UNSIGNED_LONG = 9,

    /// <summary>
    /// Type length: 4 octets, Format: F32 / DPT 14
    /// </summary>
    PDT_FLOAT = 10,

    /// <summary>
    /// Type length: 8 octets, Format: F64
    /// </summary>
    PDT_DOUBLE = 11,

    /// <summary>
    /// Type length: 10 octets, Format: A[10]
    /// </summary>
    PDT_CHAR_BLOCK = 12,

    /// <summary>
    /// Type length: 3 octets, Format: U16U8
    /// </summary>
    PDT_POLL_GROUP_SETTINGS = 13,

    /// <summary>
    /// Type length: 5 octets, Format: A[5]
    /// </summary>
    PDT_SHORT_CHAR_BLOCK = 14,

    /// <summary>
    /// Type length: 8 octets, Format: U8[r4U4][r3U5][U3U5][r2U6][r2U6]B16 / DPT 19.001
    /// </summary>
    PDT_DATE_TIME = 15,

    /// <summary>
    /// Type length: variable, Format: undefined
    /// </summary>
    PDT_VARIABLE_LENGTH = 16,

    /// <summary>
    /// Type length: 1 octet, Format: undefined
    /// </summary>
    PDT_GENERIC_01 = 17,

    /// <summary>
    /// Type length: 2 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_02 = 18,

    /// <summary>
    /// Type length: 3 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_03 = 19,

    /// <summary>
    /// Type length: 4 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_04 = 20,

    /// <summary>
    /// Type length: 5 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_05 = 21,

    /// <summary>
    /// Type length: 6 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_06 = 22,

    /// <summary>
    /// Type length: 7 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_07 = 23,

    /// <summary>
    /// Type length: 8 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_08 = 24,

    /// <summary>
    /// Type length: 9 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_09 = 25,

    /// <summary>
    /// Type length: 10 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_10 = 26,

    /// <summary>
    /// Type length: 11 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_11 = 27,

    /// <summary>
    /// Type length: 12 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_12 = 28,

    /// <summary>
    /// Type length: 13 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_13 = 29,

    /// <summary>
    /// Type length: 14 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_14 = 30,

    /// <summary>
    /// Type length: 15 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_15 = 31,

    /// <summary>
    /// Type length: 16 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_16 = 32,

    /// <summary>
    /// Type length: 17 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_17 = 33,

    /// <summary>
    /// Type length: 18 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_18 = 34,

    /// <summary>
    /// Type length: 19 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_19 = 35,

    /// <summary>
    /// Type length: 20 octets, Format: undefined
    /// </summary>
    PDT_GENERIC_20 = 36,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_25 = 37,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_26 = 38,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_27 = 39,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_28 = 40,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_29 = 41,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_2A = 42,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_2B = 43,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_2C = 44,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_2D = 45,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_2E = 46,

    /// <summary>
    /// Type length: variable, Format: DPT 28.001
    /// </summary>
    PDT_UTF8 = 47,

    /// <summary>
    /// Type length: 2 octets, Format: U5U5U6 / DPT 217.001
    /// </summary>
    PDT_VERSION = 48,

    /// <summary>
    /// Type length: 6 octets, Format: U8N8N8N8B8B8 / DPT 219.001
    /// </summary>
    PDT_ALARM_INFO = 49,

    /// <summary>
    /// Type length: 1 bit, Format: B1 / DPT 1
    /// </summary>
    PDT_BINARY_INFORMATION = 50,

    /// <summary>
    /// Type length: 1 octet, Format: B8 / DPT 21
    /// </summary>
    PDT_BITSET8 = 51,

    /// <summary>
    /// Type length: 2 octets, Format: B16 / DPT 22
    /// </summary>
    PDT_BITSET16 = 52,

    /// <summary>
    /// Type length: 1 octet, Format: N8 / DPT 20
    /// </summary>
    PDT_ENUM8 = 53,

    /// <summary>
    /// Type length: 1 octet, Format: U8 / DPT 5.001
    /// </summary>
    PDT_SCALING = 54,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_37 = 55,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_38 = 56,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_39 = 57,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_3A = 58,

    /// <summary>
    /// Reserved
    /// </summary>
    PDT_RESERVED_3B = 59,

    /// <summary>
    /// Type length: variable, Format: undefined
    /// </summary>
    PDT_NE_VL = 60,

    /// <summary>
    /// Type length: undefined but fixed, Format: undefined
    /// </summary>
    PDT_NE_FL = 61,

    /// <summary>
    /// Type length: usage dependent, Format: usage dependent
    /// </summary>
    PDT_FUNCTION = 62,

    /// <summary>
    /// Type length: undefined
    /// </summary>
    PDT_ESCAPE = 63
}
