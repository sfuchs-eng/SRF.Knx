using System.Xml;
using System.Xml.Serialization;

namespace SRF.Knx.Core.Master;

/// <summary>
/// Provides functionality to load and deserialize KNX master data from XML
/// </summary>
public static class KnxMasterDataLoader
{
    private static readonly XmlSerializer Serializer = new(typeof(KnxMasterData));

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
        var result = Serializer.Deserialize(stream) as KnxMasterData;
        if (result == null)
            throw new InvalidOperationException("Failed to deserialize KNX master data");

        return result;
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
        var result = Serializer.Deserialize(stringReader) as KnxMasterData;
        if (result == null)
            throw new InvalidOperationException("Failed to deserialize KNX master data");

        return result;
    }

    /// <summary>
    /// Loads KNX master data from an XML reader
    /// </summary>
    /// <param name="reader">XML reader</param>
    /// <returns>Deserialized KNX master data</returns>
    /// <exception cref="InvalidOperationException">Thrown when XML deserialization fails</exception>
    public static KnxMasterData LoadFromXmlReader(XmlReader reader)
    {
        var result = Serializer.Deserialize(reader) as KnxMasterData;
        if (result == null)
            throw new InvalidOperationException("Failed to deserialize KNX master data");

        return result;
    }

    /// <summary>
    /// Gets all datapoint types from the master data
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <returns>List of datapoint types</returns>
    public static List<DatapointType> GetDatapointTypes(KnxMasterData masterData)
    {
        return masterData.MasterData?.DatapointTypes?.DatapointType ?? [];
    }

    /// <summary>
    /// Gets a specific datapoint type by its number
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <param name="dptNumber">DPT number (e.g., 1 for DPT-1)</param>
    /// <returns>Datapoint type or null if not found</returns>
    public static DatapointType? GetDatapointTypeByNumber(KnxMasterData masterData, int dptNumber)
    {
        return GetDatapointTypes(masterData).FirstOrDefault(dpt => dpt.Number == dptNumber);
    }

    /// <summary>
    /// Gets a specific datapoint type by its ID
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <param name="dptId">DPT ID (e.g., "DPT-1")</param>
    /// <returns>Datapoint type or null if not found</returns>
    public static DatapointType? GetDatapointTypeById(KnxMasterData masterData, string dptId)
    {
        return GetDatapointTypes(masterData).FirstOrDefault(dpt => dpt.Id == dptId);
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
        return masterData.MasterData?.PropertyDataTypes?.PropertyDataType ?? [];
    }

    /// <summary>
    /// Gets a specific property data type by its number
    /// </summary>
    /// <param name="masterData">KNX master data</param>
    /// <param name="pdtNumber">PDT number (e.g., 1 for PDT-1)</param>
    /// <returns>Property data type or null if not found</returns>
    public static PropertyDataType? GetPropertyDataTypeByNumber(KnxMasterData masterData, int pdtNumber)
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
