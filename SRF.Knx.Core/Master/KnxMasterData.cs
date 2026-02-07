using System.Xml;
using System.Xml.Schema;
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
    public DatapointTypesDictionary? DatapointTypes { get; set; }

    [XmlElement("PropertyDataTypes")]
    public PropertyDataTypesDictionary? PropertyDataTypes { get; set; }
}

/// <summary>
/// Collection of datapoint types
/// </summary>
public class DatapointTypes
{
    [XmlElement("DatapointType")]
    public List<DatapointType> DatapointType { get; set; } = [];
}

/// <summary>
/// Dictionary wrapper for DatapointTypes that deserializes from XML list into a dictionary keyed by DataPointTypeId.
/// Provides O(1) lookup performance instead of O(n) linear search.
/// </summary>
public class DatapointTypesDictionary : IXmlSerializable
{
    private Dictionary<DPT.DataPointTypeId, DatapointType> _items = new();

    /// <summary>
    /// Gets the dictionary of datapoint types keyed by their DataPointTypeId
    /// </summary>
    public Dictionary<DPT.DataPointTypeId, DatapointType> Items => _items;

    public XmlSchema? GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        var rootAttribute = new XmlRootAttribute("DatapointTypes")
        {
            Namespace = "http://knx.org/xml/project/23"
        };
        var serializer = new XmlSerializer(typeof(DatapointTypes), rootAttribute);
        
        if (reader.IsEmptyElement)
        {
            reader.Read();
            return;
        }

        var datapointTypes = (DatapointTypes?)serializer.Deserialize(reader);
        if (datapointTypes?.DatapointType != null)
        {
            _items = datapointTypes.DatapointType.ToDictionary(dpt => new DPT.DataPointTypeId(dpt.Id));
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        var serializer = new XmlSerializer(typeof(DatapointType));
        foreach (var item in _items.Values)
        {
            serializer.Serialize(writer, item);
        }
    }
}

/// <summary>
/// Dictionary wrapper for PropertyDataTypes that deserializes from XML list into a dictionary keyed by Id.
/// Provides O(1) lookup performance instead of O(n) linear search.
/// </summary>
public class PropertyDataTypesDictionary : IXmlSerializable
{
    private Dictionary<PropertyDataTypeNumber, PropertyDataType> _items = new();

    /// <summary>
    /// Gets the dictionary of property data types keyed by their Id
    /// </summary>
    public Dictionary<PropertyDataTypeNumber, PropertyDataType> Items => _items;
    public Dictionary<string, PropertyDataType> ItemsByStringId { get; private set; } = new();

    public XmlSchema? GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        var rootAttribute = new XmlRootAttribute("PropertyDataTypes")
        {
            Namespace = "http://knx.org/xml/project/23"
        };
        var serializer = new XmlSerializer(typeof(PropertyDataTypes), rootAttribute);
        
        if (reader.IsEmptyElement)
        {
            reader.Read();
            return;
        }

        var propertyDataTypes = (PropertyDataTypes?)serializer.Deserialize(reader);
        if (propertyDataTypes?.PropertyDataType != null)
        {
            _items = propertyDataTypes.PropertyDataType.ToDictionary(pdt => pdt.Number);
            ItemsByStringId = propertyDataTypes.PropertyDataType.ToDictionary(pdt => pdt.Id);
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        var serializer = new XmlSerializer(typeof(PropertyDataType));
        foreach (var item in _items.Values)
        {
            serializer.Serialize(writer, item);
        }
    }
}
