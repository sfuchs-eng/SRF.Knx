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
}

/// <summary>
/// Unsigned integer format element
/// </summary>
public class UnsignedIntegerFormat : FormatElement
{
    [XmlAttribute("Width")]
    public int Width { get; set; }

    [XmlAttribute("Unit")]
    public string Unit { get; set; } = "";

    [XmlAttribute("Coefficient")]
    public double Coefficient { get; set; }

    [XmlIgnore]
    public bool CoefficientSpecified { get; set; }

    [XmlAttribute("MinInclusive")]
    public string MinInclusive { get; set; } = "";

    [XmlAttribute("MaxInclusive")]
    public string MaxInclusive { get; set; } = "";
}

/// <summary>
/// Signed integer format element
/// </summary>
public class SignedIntegerFormat : FormatElement
{
    [XmlAttribute("Width")]
    public int Width { get; set; }

    [XmlAttribute("Unit")]
    public string Unit { get; set; } = "";

    [XmlAttribute("Coefficient")]
    public double Coefficient { get; set; }

    [XmlIgnore]
    public bool CoefficientSpecified { get; set; }

    [XmlAttribute("MinInclusive")]
    public string MinInclusive { get; set; } = "";

    [XmlAttribute("MaxInclusive")]
    public string MaxInclusive { get; set; } = "";
}

/// <summary>
/// Float format element
/// </summary>
public class FloatFormat : FormatElement
{
    [XmlAttribute("Width")]
    public int Width { get; set; }

    [XmlAttribute("Unit")]
    public string Unit { get; set; } = "";

    [XmlAttribute("MinValue")]
    public string MinValue { get; set; } = "";

    [XmlAttribute("MaxValue")]
    public string MaxValue { get; set; } = "";
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
    [XmlAttribute("RefId")]
    public string RefId { get; set; } = "";
}
