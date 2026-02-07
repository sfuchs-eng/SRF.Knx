using System.Xml;
using System.Xml.Serialization;

namespace SRF.Knx.Core.Master;

/// <summary>
/// Provides functionality to load and deserialize KNX master data from XML
/// </summary>
public static class KnxMasterDataLoader
{
    private static readonly XmlSerializer Serializer = new(
        typeof(KnxMasterData),
        new XmlRootAttribute("KNX") { Namespace = "" });

    /// <summary>
    /// Loads KNX master data from an XML file
    /// </summary>
    /// <param name="filePath">Path to the knx_master.xml file</param>
    /// <returns>Deserialized KNX master data</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when XML deserialization fails</exception>
    public static KnxMasterData LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"KNX master data file not found: {filePath}");

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return LoadFromStream(fileStream);
    }

    /// <summary>
    /// Loads KNX master data from a stream
    /// </summary>
    /// <param name="stream">Stream containing the XML data</param>
    /// <returns>Deserialized KNX master data</returns>
    /// <exception cref="InvalidOperationException">Thrown when XML deserialization fails</exception>
    public static KnxMasterData LoadFromStream(Stream stream)
    {
        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true
        };
        
        using var xmlReader = XmlReader.Create(stream, settings);
        return LoadFromXmlReader(xmlReader);
    }

    /// <summary>
    /// Loads KNX master data from an XML string
    /// </summary>
    /// <param name="xmlContent">XML content as string</param>
    /// <returns>Deserialized KNX master data</returns>
    /// <exception cref="InvalidOperationException">Thrown when XML deserialization fails</exception>
    public static KnxMasterData LoadFromString(string xmlContent)
    {
        using var stringReader = new StringReader(xmlContent);
        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true
        };
        
        using var xmlReader = XmlReader.Create(stringReader, settings);
        return LoadFromXmlReader(xmlReader);
    }

    /// <summary>
    /// Loads KNX master data from an XML reader
    /// </summary>
    /// <param name="reader">XML reader</param>
    /// <returns>Deserialized KNX master data</returns>
    /// <exception cref="InvalidOperationException">Thrown when XML deserialization fails</exception>
    public static KnxMasterData LoadFromXmlReader(XmlReader reader)
    {
        // Create a namespace-ignoring wrapper
        using var namespaceIgnoringReader = new NamespaceIgnorantXmlReader(reader);
        var result = Serializer.Deserialize(namespaceIgnoringReader) as KnxMasterData;
        if (result == null)
            throw new InvalidOperationException("Failed to deserialize KNX master data");

        return result;
    }
    
    /// <summary>
    /// XmlReader wrapper that ignores XML namespaces during reading
    /// </summary>
    private class NamespaceIgnorantXmlReader : XmlReader
    {
        private readonly XmlReader _innerReader;
        
        public NamespaceIgnorantXmlReader(XmlReader reader)
        {
            _innerReader = reader;
        }
        
        public override bool Read() => _innerReader.Read();
        
        // Override namespace-related properties to return empty strings
        public override string NamespaceURI => string.Empty;
        public override string Prefix => string.Empty;
        
        // Pass through other properties
        public override string LocalName => _innerReader.LocalName;
        public override string Name => _innerReader.LocalName;
        public override XmlNodeType NodeType => _innerReader.NodeType;
        public override string Value => _innerReader.Value;
        public override int Depth => _innerReader.Depth;
        public override string BaseURI => _innerReader.BaseURI;
        public override bool IsEmptyElement => _innerReader.IsEmptyElement;
        public override int AttributeCount => _innerReader.AttributeCount;
        public override bool EOF => _innerReader.EOF;
        public override ReadState ReadState => _innerReader.ReadState;
        public override XmlNameTable NameTable => _innerReader.NameTable;
        
        public override string GetAttribute(string name) => _innerReader.GetAttribute(name) ?? string.Empty;
        public override string? GetAttribute(string name, string? namespaceURI) => _innerReader.GetAttribute(name, namespaceURI);
        public override string GetAttribute(int i) => _innerReader.GetAttribute(i);
        
        public override bool MoveToAttribute(string name) => _innerReader.MoveToAttribute(name);
        public override bool MoveToAttribute(string name, string? ns) => _innerReader.MoveToAttribute(name, ns);
        public override bool MoveToFirstAttribute() => _innerReader.MoveToFirstAttribute();
        public override bool MoveToNextAttribute() => _innerReader.MoveToNextAttribute();
        public override bool MoveToElement() => _innerReader.MoveToElement();
        
        public override string? LookupNamespace(string prefix) => string.Empty;
        public override void ResolveEntity() => _innerReader.ResolveEntity();
        public override bool ReadAttributeValue() => _innerReader.ReadAttributeValue();
    }

    /// <summary>
    /// Gets all datapoint types from the master data
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <returns>List of datapoint types</returns>
    public static List<DatapointType> GetDatapointTypes(KnxMasterData masterData)
    {
        return masterData.MasterData?.DatapointTypes?.Items.Values.ToList() ?? [];
    }

    /// <summary>
    /// Gets a specific datapoint type by its number
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <param name="dptNumber">DPT number (e.g., 1 for DPT-1)</param>
    /// <returns>Datapoint type or null if not found</returns>
    public static DatapointType? GetDatapointTypeByNumber(KnxMasterData masterData, int dptNumber)
    {
        var key = new DPT.DataPointTypeId(dptNumber);
        return masterData.MasterData?.DatapointTypes?.Items.TryGetValue(key, out var dpt) == true ? dpt : null;
    }

    /// <summary>
    /// Gets a specific datapoint type by its ID
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <param name="dptId">DPT ID (e.g., "DPT-1")</param>
    /// <returns>Datapoint type or null if not found</returns>
    public static DatapointType? GetDatapointTypeById(KnxMasterData masterData, string dptId)
    {
        var key = new DPT.DataPointTypeId(dptId);
        return masterData.MasterData?.DatapointTypes?.Items.TryGetValue(key, out var dpt) == true ? dpt : null;
    }

    /// <summary>
    /// Gets a specific datapoint subtype by its ID
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <param name="dpstId">DPST ID (e.g., "DPST-1-1")</param>
    /// <returns>Datapoint subtype or null if not found</returns>
    public static DatapointSubtype? GetDatapointSubtypeById(KnxMasterData masterData, string dpstId)
    {
        return GetDatapointTypes(masterData)
            .SelectMany(dpt => dpt.DatapointSubtypes?.DatapointSubtype ?? [])
            .FirstOrDefault(dpst => dpst.Id == dpstId);
    }

    /// <summary>
    /// Gets a specific datapoint subtype by DPT number and subtype number
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <param name="dptNumber">DPT number (e.g., 1)</param>
    /// <param name="dpstNumber">DPST number (e.g., 1)</param>
    /// <returns>Datapoint subtype or null if not found</returns>
    public static DatapointSubtype? GetDatapointSubtype(KnxMasterData masterData, int dptNumber, int dpstNumber)
    {
        var dpt = GetDatapointTypeByNumber(masterData, dptNumber);
        return dpt?.DatapointSubtypes?.DatapointSubtype.FirstOrDefault(dpst => dpst.Number == dpstNumber);
    }

    /// <summary>
    /// Gets all property data types from the master data
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <returns>List of property data types</returns>
    public static List<PropertyDataType> GetPropertyDataTypes(KnxMasterData masterData)
    {
        return masterData.MasterData?.PropertyDataTypes?.Items.Values.ToList() ?? [];
    }

    /// <summary>
    /// Gets a specific property data type by its number
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <param name="pdtNumber">PDT number (e.g., 1 for PDT-1)</param>
    /// <returns>Property data type or null if not found</returns>
    public static PropertyDataType? GetPropertyDataTypeByNumber(KnxMasterData masterData, PropertyDataTypeNumber pdtNumber)
    {
        return GetPropertyDataTypes(masterData).FirstOrDefault(pdt => pdt.Number == pdtNumber);
    }

    /// <summary>
    /// Gets a specific property data type by its ID
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <param name="pdtId">PDT ID (e.g., "PDT-1")</param>
    /// <returns>Property data type or null if not found</returns>
    public static PropertyDataType? GetPropertyDataTypeById(KnxMasterData masterData, string pdtId)
    {
        return GetPropertyDataTypes(masterData).FirstOrDefault(pdt => pdt.Id == pdtId);
    }

    /// <summary>
    /// Gets a specific property data type by its name
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <param name="pdtName">PDT name (e.g., "PDT_CHAR", "PDT_UNSIGNED_INT")</param>
    /// <returns>Property data type or null if not found</returns>
    public static PropertyDataType? GetPropertyDataTypeByName(KnxMasterData masterData, string pdtName)
    {
        return GetPropertyDataTypes(masterData).FirstOrDefault(pdt => pdt.Name == pdtName);
    }
}
