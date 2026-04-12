using System.Xml.Serialization;

namespace SRF.Knx.Core.Master;

/// <summary>
/// Container for format specifications of a datapoint subtype
/// Holds the structure and encoding information for data values
/// </summary>
public class Format
{
    /// <summary>
    /// Collection of all format elements (Bit, UnsignedInteger, SignedInteger, Float, String, Enumeration, Reserved, RefType)
    /// </summary>
    [XmlElement("Bit", typeof(BitFormat))]
    [XmlElement("UnsignedInteger", typeof(UnsignedIntegerFormat))]
    [XmlElement("SignedInteger", typeof(SignedIntegerFormat))]
    [XmlElement("Float", typeof(FloatFormat))]
    [XmlElement("String", typeof(StringFormat))]
    [XmlElement("Enumeration", typeof(EnumerationFormat))]
    [XmlElement("Reserved", typeof(ReservedFormat))]
    [XmlElement("RefType", typeof(RefTypeFormat))]
    public List<FormatElement> Elements { get; set; } = [];
}

/// <summary>
/// Base class for all format elements
/// </summary>
public abstract class FormatElement
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = "";

    [XmlAttribute("Name")]
    public string Name { get; set; } = "";

    [XmlIgnore]
    public virtual Type? PreferredCSharpType { get; set; }
}

/// <summary>
/// Bit field format element
/// </summary>
public class BitFormat : FormatElement
{
    [XmlAttribute("Cleared")]
    public string Cleared { get; set; } = "";

    [XmlAttribute("Set")]
    public string Set { get; set; } = "";

    public override Type PreferredCSharpType => typeof(bool);
}


/// <summary>
/// Base class for numeric format elements (UnsignedInteger, SignedInteger, Float, ...)
/// </summary>
public class NumericFormat : FormatElement
{
    [XmlAttribute("Width")]
    public int Width { get; set; }

    [XmlAttribute("Coefficient")]
    public double Coefficient { get; set; } = 1;

    [XmlIgnore]
    public bool CoefficientSpecified { get; set; }
    [XmlAttribute("Unit")]
    public string Unit { get; set; } = "";
    [XmlIgnore]
    public bool UnitSpecified { get; set; }
}

public class IntegralNumericFormat : NumericFormat
{
    [XmlAttribute("MinInclusive")]
    public long MinInclusive { get; set; } = 0;

    [XmlAttribute("MaxInclusive")]
    public long MaxInclusive { get; set; } = 0;
}

public class DecimalNumericFormat : NumericFormat
{
    [XmlAttribute("MinValue")]
    public string MinValue { get; set; } = "";

    [XmlAttribute("MaxValue")]
    public string MaxValue { get; set; } = "";

    public override Type? PreferredCSharpType => typeof(decimal);
}

/// <summary>
/// Unsigned integer format element
/// </summary>
public class UnsignedIntegerFormat : IntegralNumericFormat
{
    public override Type? PreferredCSharpType => Width switch
    {
        <= 8 => typeof(byte),
        <= 16 => typeof(ushort),
        <= 32 => typeof(uint),
        <= 64 => typeof(ulong),
        _ => null
    };
}

/// <summary>
/// Signed integer format element
/// </summary>
public class SignedIntegerFormat : IntegralNumericFormat
{
    public override Type? PreferredCSharpType => Width switch
    {
        <= 8 => typeof(sbyte),
        <= 16 => typeof(short),
        <= 32 => typeof(int),
        <= 64 => typeof(long),
        _ => null
    };
}

/// <summary>
/// Float format element
/// </summary>
public class FloatFormat : DecimalNumericFormat
{
    public override Type? PreferredCSharpType => typeof(float);
}

/// <summary>
/// String format element
/// </summary>
public class StringFormat : FormatElement
{
    [XmlAttribute("Width")]
    public int Width { get; set; }

    [XmlAttribute("Encoding")]
    public string Encoding { get; set; } = "";

    public override Type? PreferredCSharpType => typeof(string);
}

/// <summary>
/// Enumeration format element
/// </summary>
public class EnumerationFormat : FormatElement
{
    [XmlAttribute("Width")]
    public int Width { get; set; }

    [XmlElement("EnumValue")]
    public List<EnumValue> EnumValues { get; set; } = [];

    public override Type? PreferredCSharpType => typeof(int);
}

/// <summary>
/// Enumeration value definition
/// </summary>
public class EnumValue
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = "";

    [XmlAttribute("Value")]
    public int Value { get; set; }

    [XmlAttribute("Text")]
    public string Text { get; set; } = "";
}

/// <summary>
/// Reserved bits format element (padding)
/// </summary>
public class ReservedFormat : FormatElement
{
    [XmlAttribute("Width")]
    public int Width { get; set; }
}

/// <summary>
/// Reference to another format type element
/// </summary>
public class RefTypeFormat : FormatElement
{
    /// <summary>
    /// Matches the Id of another format element in the same format definition, which is referenced by this element
    /// </summary>
    [XmlAttribute("RefId")]
    public string RefId { get; set; } = "";
}
