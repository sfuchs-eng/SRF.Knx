using System.Xml.Serialization;

namespace SRF.Knx.Core.Master;

/// <summary>
/// Collection of property data types
/// </summary>
public class PropertyDataTypes
{
    [XmlElement("PropertyDataType")]
    public List<PropertyDataType> PropertyDataType { get; set; } = [];
}

/// <summary>
/// Represents a KNX Property Data Type (PDT) which defines the data type for device properties.
/// Property Data Types are used for interface object properties in KNX devices.
/// </summary>
public class PropertyDataType
{
    /// <summary>
    /// Unique identifier for the property data type (e.g., "PDT-1")
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = "";

    /// <summary>
    /// Numeric identifier for the property data type (e.g., 1 for PDT-1)
    /// </summary>
    [XmlAttribute("Number")]
    public int Number { get; set; }

    /// <summary>
    /// Human-readable name for the property data type (e.g., "PDT_CHAR", "PDT_UNSIGNED_INT")
    /// </summary>
    [XmlAttribute("Name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Size of the property data in bytes.
    /// Optional - not present for variable-length types like PDT_VARIABLE_LENGTH.
    /// </summary>
    [XmlAttribute("Size")]
    public int Size { get; set; }

    /// <summary>
    /// Indicates whether the Size attribute is present in the XML.
    /// Used by XmlSerializer for optional attributes.
    /// </summary>
    [XmlIgnore]
    public bool SizeSpecified { get; set; }

    /// <summary>
    /// Read size in bytes for reading the property.
    /// Optional - only present for certain types like PDT_CONTROL.
    /// </summary>
    [XmlAttribute("ReadSize")]
    public int ReadSize { get; set; }

    /// <summary>
    /// Indicates whether the ReadSize attribute is present in the XML.
    /// Used by XmlSerializer for optional attributes.
    /// </summary>
    [XmlIgnore]
    public bool ReadSizeSpecified { get; set; }

    /// <summary>
    /// Helper property to check if Size is defined
    /// </summary>
    [XmlIgnore]
    public bool HasSize => SizeSpecified;

    /// <summary>
    /// Helper property to check if ReadSize is defined
    /// </summary>
    [XmlIgnore]
    public bool HasReadSize => ReadSizeSpecified;
}
