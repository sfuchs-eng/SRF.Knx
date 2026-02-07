using System.Xml;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Test.Core;

[TestFixture]
public class KnxMasterDataLoaderTests
{
    private string _knxMasterFilePath = "";

    [SetUp]
    public void Setup()
    {
        // Use the real knx_master.xml file from SRF.Knx.Config/Resources
        var baseDir = Path.GetDirectoryName(typeof(KnxMasterDataLoaderTests).Assembly.Location) ?? "";
        _knxMasterFilePath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "SRF.Knx.Config", "Resources", "knx_master.xml"));
        
        if (!File.Exists(_knxMasterFilePath))
        {
            Assert.Fail($"knx_master.xml not found at: {_knxMasterFilePath}");
        }
    }

    [Test]
    public void LoadFromFile_ValidFile_ReturnsKnxMasterData()
    {
        // Act
        var result = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MasterData, Is.Not.Null);
        Assert.That(result.MasterData!.Id, Is.Not.Empty);
        Assert.That(result.MasterData.Version, Is.Not.Empty);
        Assert.That(result.MasterData.Signature, Is.Not.Empty);
    }

    [Test]
    public void LoadFromFile_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentFile = "non_existent_file.xml";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => 
            KnxMasterDataLoader.LoadFromFile(nonExistentFile));
    }

    [Test]
    public void LoadFromStream_ValidStream_ReturnsKnxMasterData()
    {
        // Arrange
        using var stream = File.OpenRead(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.LoadFromStream(stream);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MasterData, Is.Not.Null);
        Assert.That(result.MasterData!.DatapointTypes, Is.Not.Null);
    }

    [Test]
    public void LoadFromString_ValidXml_ReturnsKnxMasterData()
    {
        // Arrange
        var xmlContent = File.ReadAllText(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.LoadFromString(xmlContent);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MasterData, Is.Not.Null);
        Assert.That(result.MasterData!.DatapointTypes?.Items, Is.Not.Empty);
    }

    [Test]
    public void LoadFromString_InvalidXml_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidXml = "<invalid>xml</content>";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            KnxMasterDataLoader.LoadFromString(invalidXml));
    }

    [Test]
    public void LoadFromXmlReader_ValidReader_ReturnsKnxMasterData()
    {
        // Arrange
        using var fileStream = File.OpenRead(_knxMasterFilePath);
        using var xmlReader = XmlReader.Create(fileStream);

        // Act
        var result = KnxMasterDataLoader.LoadFromXmlReader(xmlReader);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MasterData, Is.Not.Null);
    }

    [Test]
    public void GetDatapointTypes_ValidMasterData_ReturnsAllDatapointTypes()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetDatapointTypes(masterData);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count, Is.GreaterThan(10), "Expected multiple DPT types");
    }

    [Test]
    public void GetDatapointTypes_NoMasterData_ReturnsEmptyList()
    {
        // Arrange
        var masterData = new KnxMasterData();

        // Act
        var result = KnxMasterDataLoader.GetDatapointTypes(masterData);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetDatapointTypeByNumber_ExistingNumber_ReturnsCorrectType()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetDatapointTypeByNumber(masterData, 1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo("DPT-1"));
        Assert.That(result.Number, Is.EqualTo(1));
        Assert.That(result.SizeInBit, Is.GreaterThan(0));
    }

    [Test]
    public void GetDatapointTypeByNumber_NonExistingNumber_ReturnsNull()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetDatapointTypeByNumber(masterData, 99999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetDatapointTypeById_ExistingId_ReturnsCorrectType()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetDatapointTypeById(masterData, "DPT-9");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Number, Is.EqualTo(9));
        Assert.That(result.Id, Is.EqualTo("DPT-9"));
    }

    [Test]
    public void GetDatapointTypeById_NonExistingId_ReturnsNull()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetDatapointTypeById(masterData, "DPT-99999");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetDatapointSubtypeById_ExistingId_ReturnsCorrectSubtype()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetDatapointSubtypeById(masterData, "DPST-1-1");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo("DPST-1-1"));
        Assert.That(result.Number, Is.EqualTo(1));
        Assert.That(result.Name, Is.Not.Empty);
    }

    [Test]
    public void GetDatapointSubtypeById_NonExistingId_ReturnsNull()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetDatapointSubtypeById(masterData, "DPST-99999-99999");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetDatapointSubtype_ExistingDptAndDpst_ReturnsCorrectSubtype()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetDatapointSubtype(masterData, 1, 2);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo("DPST-1-2"));
        Assert.That(result.Number, Is.EqualTo(2));
    }

    [Test]
    public void GetDatapointSubtype_NonExistingDpt_ReturnsNull()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetDatapointSubtype(masterData, 99999, 1);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetDatapointSubtype_NonExistingDpst_ReturnsNull()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetDatapointSubtype(masterData, 1, 99999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void DatapointSubtype_HasCorrectFormatElements()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act - Test Bit format (DPST-1-1 should have a Bit format)
        var dpst1 = KnxMasterDataLoader.GetDatapointSubtype(masterData, 1, 1);
        
        // Assert
        Assert.That(dpst1, Is.Not.Null);
        Assert.That(dpst1!.Format, Is.Not.Null);
        Assert.That(dpst1.Format!.Elements, Is.Not.Empty);
        
        var bitFormat = dpst1.Format.Elements[0] as BitFormat;
        Assert.That(bitFormat, Is.Not.Null);
        Assert.That(bitFormat!.Cleared, Is.Not.Empty);
        Assert.That(bitFormat.Set, Is.Not.Empty);
    }

    [Test]
    public void DatapointSubtype_FloatFormat_HasCorrectProperties()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act - DPST-9-1 is temperature with float format
        var dpst9 = KnxMasterDataLoader.GetDatapointSubtype(masterData, 9, 1);

        // Assert
        Assert.That(dpst9, Is.Not.Null);
        Assert.That(dpst9!.Format, Is.Not.Null);
        Assert.That(dpst9.Format!.Elements, Is.Not.Empty);
        
        var floatFormat = dpst9.Format.Elements[0] as FloatFormat;
        Assert.That(floatFormat, Is.Not.Null);
        Assert.That(floatFormat!.Width, Is.GreaterThan(0));
        Assert.That(floatFormat.Unit, Is.Not.Empty);
    }

    [Test]
    public void DatapointSubtype_UnsignedIntegerFormat_HasCoefficient()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act - DPST-5-1 is percentage with UnsignedInteger format
        var dpst5 = KnxMasterDataLoader.GetDatapointSubtype(masterData, 5, 1);

        // Assert
        Assert.That(dpst5, Is.Not.Null);
        Assert.That(dpst5!.Format, Is.Not.Null);
        Assert.That(dpst5.Format!.Elements, Is.Not.Empty);
        
        var uintFormat = dpst5.Format.Elements[0] as UnsignedIntegerFormat;
        Assert.That(uintFormat, Is.Not.Null);
        Assert.That(uintFormat!.Width, Is.GreaterThan(0));
    }

    [Test]
    public void DatapointType_HasMultipleSubtypes()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var dpt1 = KnxMasterDataLoader.GetDatapointTypeByNumber(masterData, 1);

        // Assert
        Assert.That(dpt1, Is.Not.Null);
        Assert.That(dpt1!.DatapointSubtypes, Is.Not.Null);
        Assert.That(dpt1.DatapointSubtypes!.DatapointSubtype, Is.Not.Empty);
        Assert.That(dpt1.DatapointSubtypes.DatapointSubtype.Count, Is.GreaterThan(1));
    }

    #region PropertyDataType Tests

    [Test]
    public void LoadFromFile_ValidFile_ContainsPropertyDataTypes()
    {
        // Act
        var result = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Assert
        Assert.That(result.MasterData, Is.Not.Null);
        Assert.That(result.MasterData!.PropertyDataTypes, Is.Not.Null);
        Assert.That(result.MasterData.PropertyDataTypes!.Items, Is.Not.Empty);
    }

    [Test]
    public void GetPropertyDataTypes_ValidMasterData_ReturnsAllPropertyDataTypes()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetPropertyDataTypes(masterData);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count, Is.GreaterThan(20), "Expected multiple PDT types");
    }

    [Test]
    public void GetPropertyDataTypes_NoMasterData_ReturnsEmptyList()
    {
        // Arrange
        var masterData = new KnxMasterData();

        // Act
        var result = KnxMasterDataLoader.GetPropertyDataTypes(masterData);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetPropertyDataTypeByNumber_ExistingNumber_ReturnsCorrectType()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetPropertyDataTypeByNumber(masterData, PropertyDataTypeNumber.PDT_CHAR);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo("PDT-1"));
        Assert.That(result.Number, Is.EqualTo(PropertyDataTypeNumber.PDT_CHAR));
        Assert.That(result.Name, Is.EqualTo("PDT_CHAR"));
        Assert.That(result.Size, Is.EqualTo(1));
    }

    [Test]
    public void GetPropertyDataTypeByNumber_NonExistingNumber_ReturnsNull()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetPropertyDataTypeByNumber(masterData, (PropertyDataTypeNumber)99999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetPropertyDataTypeById_ExistingId_ReturnsCorrectType()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetPropertyDataTypeById(masterData, "PDT-10");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Number, Is.EqualTo(PropertyDataTypeNumber.PDT_FLOAT));
        Assert.That(result.Name, Is.EqualTo("PDT_FLOAT"));
        Assert.That(result.Size, Is.EqualTo(4));
    }

    [Test]
    public void GetPropertyDataTypeById_NonExistingId_ReturnsNull()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetPropertyDataTypeById(masterData, "PDT-99999");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetPropertyDataTypeByName_ExistingName_ReturnsCorrectType()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetPropertyDataTypeByName(masterData, "PDT_UNSIGNED_INT");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo("PDT-4"));
        Assert.That(result.Number, Is.EqualTo(PropertyDataTypeNumber.PDT_UNSIGNED_INT));
        Assert.That(result.Size, Is.EqualTo(2));
    }

    [Test]
    public void GetPropertyDataTypeByName_NonExistingName_ReturnsNull()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var result = KnxMasterDataLoader.GetPropertyDataTypeByName(masterData, "PDT_NON_EXISTENT");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void PropertyDataType_VariableLength_HasNoSize()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act - PDT-16 is PDT_VARIABLE_LENGTH with no Size attribute
        var result = KnxMasterDataLoader.GetPropertyDataTypeByNumber(masterData, (PropertyDataTypeNumber)16);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("PDT_VARIABLE_LENGTH"));
        Assert.That(result.HasSize, Is.False);
        Assert.That(result.SizeSpecified, Is.False);
    }

    [Test]
    public void PropertyDataType_WithReadSize_HasCorrectAttributes()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act - PDT-0 is PDT_CONTROL with both Size and ReadSize
        var result = KnxMasterDataLoader.GetPropertyDataTypeByNumber(masterData, PropertyDataTypeNumber.PDT_CONTROL);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("PDT_CONTROL"));
        Assert.That(result.Size, Is.EqualTo(10));
        Assert.That(result.ReadSize, Is.EqualTo(1));
        Assert.That(result.HasSize, Is.True);
        Assert.That(result.HasReadSize, Is.True);
    }

    [Test]
    public void PropertyDataType_UTF8_HasNoSize()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act - PDT-47 is PDT_UTF-8 with no Size
        var result = KnxMasterDataLoader.GetPropertyDataTypeByNumber(masterData, PropertyDataTypeNumber.PDT_UTF_8);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("PDT_UTF-8"));
        Assert.That(result.HasSize, Is.False);
    }

    [Test]
    public void PropertyDataTypes_AllRequiredFieldsPresent()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var pdts = KnxMasterDataLoader.GetPropertyDataTypes(masterData);

        // Assert
        Assert.That(pdts, Is.Not.Empty);
        
        foreach (var pdt in pdts)
        {
            Assert.That(pdt.Id, Is.Not.Empty, $"PDT {pdt.Number} has empty Id");
            Assert.That(pdt.Name, Is.Not.Empty, $"PDT {pdt.Number} has empty Name");
            Assert.That((int)pdt.Number, Is.GreaterThanOrEqualTo(0), $"PDT {pdt.Id} has invalid Number");
        }
    }

    [Test]
    public void PropertyDataTypes_AllPdtsFromXmlAreEnumeratedWithCorrectIds()
    {
        // Arrange
        var masterData = KnxMasterDataLoader.LoadFromFile(_knxMasterFilePath);

        // Act
        var pdts = KnxMasterDataLoader.GetPropertyDataTypes(masterData);

        // Assert
        Assert.That(pdts, Is.Not.Empty, "No PDTs found in knx_master.xml");

        // Check that each PDT from XML has a valid Number enum value
        foreach (var pdt in pdts)
        {
            // Check that the Number enum value is defined in the enum
            Assert.That(Enum.IsDefined(typeof(PropertyDataTypeNumber), pdt.Number), 
                Is.True, 
                $"PDT '{pdt.Name}' (Id={pdt.Id}) has invalid Number enum value: {pdt.Number}");

            // Check that the numeric value matches the enum value (should always be true now)
            var numericValue = (int)pdt.Number;
            Assert.That(numericValue, Is.GreaterThanOrEqualTo(0), 
                $"PDT '{pdt.Name}' has negative Number value: {numericValue}");
        }

        // Verify we have a reasonable number of PDTs (at least 40, accounting for reserved values)
        Assert.That(pdts.Count, Is.GreaterThanOrEqualTo(40), 
            "Expected at least 40 PDTs in knx_master.xml");
    }

    #endregion
}
