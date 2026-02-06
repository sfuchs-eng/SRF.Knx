using System.Xml.Serialization;

namespace SRF.Knx.Core.Master;

/// <summary>
/// Root element for KNX master data XML structure
/// </summary>
[XmlRoot("KNX", Namespace = "http://knx.org/xml/project/23")]
public class KnxMasterData
{
    [XmlElement("MasterData")]
    public MasterData? MasterData { get; set; }
}

/// <summary>
/// Container for all master data including datapoint types
/// </summary>
public class MasterData
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = "";

    [XmlAttribute("Version")]
    public string Version { get; set; } = "";

    [XmlAttribute("Signature")]
    public string Signature { get; set; } = "";

    [XmlElement("DatapointTypes")]
    public DatapointTypes? DatapointTypes { get; set; }
}

/// <summary>
/// Collection of datapoint types
/// </summary>
public class DatapointTypes
{
    [XmlElement("DatapointType")]
    public List<DatapointType> DatapointType { get; set; } = [];
}
