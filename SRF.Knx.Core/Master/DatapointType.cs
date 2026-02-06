using System.Xml.Serialization;

namespace SRF.Knx.Core.Master;

/// <summary>
/// Represents a KNX datapoint type (DPT) with its metadata and subtypes
/// </summary>
public class DatapointType
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = "";

    [XmlAttribute("Number")]
    public int Number { get; set; }

    [XmlAttribute("Name")]
    public string Name { get; set; } = "";

    [XmlAttribute("Text")]
    public string Text { get; set; } = "";

    [XmlAttribute("SizeInBit")]
    public int SizeInBit { get; set; }

    [XmlAttribute("PDT")]
    public string PDT { get; set; } = "";

    [XmlAttribute("Default")]
    public bool Default { get; set; }

    [XmlIgnore]
    public bool DefaultSpecified { get; set; }

    [XmlElement("DatapointSubtypes")]
    public DatapointSubtypes? DatapointSubtypes { get; set; }
}

/// <summary>
/// Collection of datapoint subtypes
/// </summary>
public class DatapointSubtypes
{
    [XmlElement("DatapointSubtype")]
    public List<DatapointSubtype> DatapointSubtype { get; set; } = [];
}

/// <summary>
/// Represents a specific subtype of a datapoint type (DPST)
/// </summary>
public class DatapointSubtype
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = "";

    [XmlAttribute("Number")]
    public int Number { get; set; }

    [XmlAttribute("Name")]
    public string Name { get; set; } = "";

    [XmlAttribute("Text")]
    public string Text { get; set; } = "";

    [XmlAttribute("PDT")]
    public string PDT { get; set; } = "";

    [XmlAttribute("Default")]
    public bool Default { get; set; }

    [XmlIgnore]
    public bool DefaultSpecified { get; set; }

    [XmlElement("Format")]
    public Format? Format { get; set; }
}
