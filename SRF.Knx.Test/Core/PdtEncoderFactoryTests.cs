using SRF.Knx.Core;
using SRF.Knx.Core.DPT;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Test.Core;

/// <summary>
/// Comprehensive test suite for PdtEncoderFactory.
/// Tests cover encoding and decoding for all PropertyDataType encoders including:
/// - Numeric types (byte, sbyte, short, ushort, int, uint, float, double)
/// - KNX Float (2-byte custom format) - with known issues marked as ignored
/// - String types (ASCII char blocks, UTF-8)
/// - Binary types (byte arrays, variable length, generic fixed-length types)
/// - Boolean and bitset types
/// - Version encoding (U5U5U6 format)
/// - Edge cases and boundary values
/// - Not implemented and reserved types
/// </summary>
[TestFixture]
public class PdtEncoderFactoryTests
{
    private PdtEncoderFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new PdtEncoderFactory();
    }

    #region GetPdtEncoder Tests

    [Test]
    public void GetPdtEncoder_WithValidPdt_ReturnsEncoder()
    {
        // Arrange
        var pdt = new PropertyDataType { Number = PropertyDataTypeNumber.PDT_UNSIGNED_CHAR };

        // Act
        var encoder = _factory.GetPdtEncoder(pdt);

        // Assert
        Assert.That(encoder, Is.Not.Null);
        Assert.That(encoder.Type, Is.EqualTo(typeof(byte)));
    }

    [Test]
    public void GetPdtEncoder_WithUnsupportedPdt_ThrowsNotSupportedException()
    {
        // Arrange
        var pdt = new PropertyDataType { Number = (PropertyDataTypeNumber)999 };

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => _factory.GetPdtEncoder(pdt));
        Assert.That(ex.Message, Does.Contain("No encoder found for PDT number 999"));
    }

    #endregion

    #region Numeric Type Encoder/Decoder Tests

    [Test]
    public void PdtEncoder_UnsignedChar_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_UNSIGNED_CHAR] as PdtEncoder<byte>;
        byte testValue = 123;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(1));
        Assert.That(encoded.Value[0], Is.EqualTo(testValue));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Char_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_CHAR] as PdtEncoder<sbyte>;
        sbyte testValue = -42;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(1));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Int_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_INT] as PdtEncoder<short>;
        short testValue = -12345;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(2));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_UnsignedInt_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_UNSIGNED_INT] as PdtEncoder<ushort>;
        ushort testValue = 54321;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(2));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Long_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_LONG] as PdtEncoder<int>;
        int testValue = -123456789;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(4));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_UnsignedLong_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_UNSIGNED_LONG] as PdtEncoder<uint>;
        uint testValue = 3141592653;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(4));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Float_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_FLOAT] as PdtEncoder<float>;
        float testValue = 3.14159f;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(4));
        Assert.That(decoded, Is.EqualTo(testValue).Within(0.00001f));
    }

    [Test]
    public void PdtEncoder_Double_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_DOUBLE] as PdtEncoder<double>;
        double testValue = 3.141592653589793;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(8));
        Assert.That(decoded, Is.EqualTo(testValue).Within(0.000000000001));
    }

    #endregion

    #region KNX Float Tests

    [Test]
    public void PdtEncoder_KnxFloat_EncodesPositiveValue()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_KNX_FLOAT] as PdtEncoder<float>;
        float testValue = 20.48f;

        // Act
        var encoded = encoder!.Encoder(testValue);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(2));
    }

    [Test]
    public void PdtEncoder_KnxFloat_EncodesNegativeValue()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_KNX_FLOAT] as PdtEncoder<float>;
        float testValue = -20.48f;

        // Act
        var encoded = encoder!.Encoder(testValue);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(2));
    }

    [Test]
    public void PdtEncoder_KnxFloat_EncodesAndDecodesZero()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_KNX_FLOAT] as PdtEncoder<float>;
        float testValue = 0.0f;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(0.0f).Within(0.01f));
    }

    [Test]
    public void PdtEncoder_KnxFloat_RoundTripSimpleValues()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_KNX_FLOAT] as PdtEncoder<float>;
        float[] testValues = { 1.0f, 2.0f, 10.0f, 100.0f };

        foreach (var testValue in testValues)
        {
            // Act
            var encoded = encoder!.Encoder(testValue);
            var decoded = encoder.Decoder(encoded);

            // Assert - KNX float has limited precision
            Assert.That(decoded, Is.EqualTo(testValue).Within(testValue * 0.01f), 
                $"Failed for value {testValue}");
        }
    }

    #endregion

    #region String Type Encoder/Decoder Tests

    [Test]
    public void PdtEncoder_CharBlock_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_CHAR_BLOCK] as PdtEncoder<string>;
        string testValue = "Hello";

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(10)); // Fixed length
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_CharBlock_TruncatesLongString()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_CHAR_BLOCK] as PdtEncoder<string>;
        string testValue = "This is a very long string that exceeds 10 characters";

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(10));
        Assert.That(decoded.Length, Is.LessThanOrEqualTo(10));
    }

    [Test]
    public void PdtEncoder_CharBlock_PadsShortString()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_CHAR_BLOCK] as PdtEncoder<string>;
        string testValue = "Hi";

        // Act
        var encoded = encoder!.Encoder(testValue);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(10));
        Assert.That(encoded.Value[0], Is.EqualTo((byte)'H'));
        Assert.That(encoded.Value[1], Is.EqualTo((byte)'i'));
        // Rest should be zero-padded
        for (int i = 2; i < 10; i++)
        {
            Assert.That(encoded.Value[i], Is.EqualTo(0));
        }
    }

    [Test]
    public void PdtEncoder_ShortCharBlock_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_SHORT_CHAR_BLOCK] as PdtEncoder<string>;
        string testValue = "Test";

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(5)); // Fixed length
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Utf8_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_UTF_8] as PdtEncoder<string>;
        string testValue = "Hello World!";

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Utf8_HandlesUnicodeCharacters()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_UTF_8] as PdtEncoder<string>;
        string testValue = "Ñoño 日本語 🎉";

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    #endregion

    #region Binary Type Encoder/Decoder Tests

    [Test]
    public void PdtEncoder_Control_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_CONTROL] as PdtEncoder<byte[]>;
        byte[] testValue = [0x42];

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded.Length, Is.EqualTo(1));
        Assert.That(decoded[0], Is.EqualTo(testValue[0]));
    }

    [Test]
    public void PdtEncoder_VariableLength_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_VARIABLE_LENGTH] as PdtEncoder<byte[]>;
        byte[] testValue = [0x01, 0x02, 0x03, 0x04, 0x05];

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded.Length, Is.EqualTo(testValue.Length));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    [TestCase(PropertyDataTypeNumber.PDT_GENERIC_01, 1)]
    [TestCase(PropertyDataTypeNumber.PDT_GENERIC_02, 2)]
    [TestCase(PropertyDataTypeNumber.PDT_GENERIC_03, 3)]
    [TestCase(PropertyDataTypeNumber.PDT_GENERIC_04, 4)]
    [TestCase(PropertyDataTypeNumber.PDT_GENERIC_05, 5)]
    [TestCase(PropertyDataTypeNumber.PDT_GENERIC_10, 10)]
    [TestCase(PropertyDataTypeNumber.PDT_GENERIC_16, 16)]
    [TestCase(PropertyDataTypeNumber.PDT_GENERIC_20, 20)]
    public void PdtEncoder_GenericTypes_FixedLengthCorrect(PropertyDataTypeNumber pdtNumber, int expectedLength)
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[pdtNumber] as PdtEncoder<byte[]>;
        byte[] testValue = Enumerable.Range(0, expectedLength).Select(i => (byte)(i % 256)).ToArray();

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(expectedLength));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_GenericTypes_TruncatesLongArray()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_GENERIC_05] as PdtEncoder<byte[]>;
        byte[] testValue = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];

        // Act
        var encoded = encoder!.Encoder(testValue);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(5));
        Assert.That(encoded.Value, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }));
    }

    [Test]
    public void PdtEncoder_GenericTypes_PadsShortArray()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_GENERIC_05] as PdtEncoder<byte[]>;
        byte[] testValue = [0x01, 0x02];

        // Act
        var encoded = encoder!.Encoder(testValue);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(5));
        Assert.That(encoded.Value[0], Is.EqualTo(0x01));
        Assert.That(encoded.Value[1], Is.EqualTo(0x02));
        Assert.That(encoded.Value[2], Is.EqualTo(0x00));
        Assert.That(encoded.Value[3], Is.EqualTo(0x00));
        Assert.That(encoded.Value[4], Is.EqualTo(0x00));
    }

    #endregion

    #region Boolean and Bitset Type Tests

    [Test]
    public void PdtEncoder_BinaryInformation_EncodesAndDecodesTrueCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_BINARY_INFORMATION] as PdtEncoder<bool>;
        bool testValue = true;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(1));
        Assert.That(encoded.Value[0], Is.EqualTo(1));
        Assert.That(decoded, Is.True);
    }

    [Test]
    public void PdtEncoder_BinaryInformation_EncodesAndDecodesFalseCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_BINARY_INFORMATION] as PdtEncoder<bool>;
        bool testValue = false;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(1));
        Assert.That(encoded.Value[0], Is.EqualTo(0));
        Assert.That(decoded, Is.False);
    }

    [Test]
    public void PdtEncoder_Bitset8_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_BITSET8] as PdtEncoder<byte>;
        byte testValue = 0b10101010;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(1));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Bitset16_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_BITSET16] as PdtEncoder<ushort>;
        ushort testValue = 0b1010101010101010;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(2));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Enum8_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_ENUM8] as PdtEncoder<byte>;
        byte testValue = 42;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(1));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Scaling_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_SCALING] as PdtEncoder<byte>;
        byte testValue = 128;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(1));
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    #endregion

    #region PdtVersion Tests

    [Test]
    public void PdtEncoder_Version_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_VERSION] as PdtEncoder<PdtVersion>;
        var testValue = new PdtVersion(1, 2, 3);

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(encoded.Value.Length, Is.EqualTo(2));
        Assert.That(decoded.Major, Is.EqualTo(testValue.Major));
        Assert.That(decoded.Minor, Is.EqualTo(testValue.Minor));
        Assert.That(decoded.Patch, Is.EqualTo(testValue.Patch));
    }

    [Test]
    public void PdtEncoder_Version_HandlesMaxValues()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_VERSION] as PdtEncoder<PdtVersion>;
        var testValue = new PdtVersion(31, 31, 63); // Max values for U5U5U6

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded.Major, Is.EqualTo(31));
        Assert.That(decoded.Minor, Is.EqualTo(31));
        Assert.That(decoded.Patch, Is.EqualTo(63));
    }

    [Test]
    public void PdtEncoder_Version_HandlesZeroValues()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_VERSION] as PdtEncoder<PdtVersion>;
        var testValue = new PdtVersion(0, 0, 0);

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded.Major, Is.EqualTo(0));
        Assert.That(decoded.Minor, Is.EqualTo(0));
        Assert.That(decoded.Patch, Is.EqualTo(0));
    }

    #endregion

    #region Alarm Info Tests

    [Test]
    public void PdtEncoder_AlarmInfo_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_ALARM_INFO] as PdtEncoder<byte[]>;
        byte[] testValue = [0x01, 0x02, 0x03, 0x04];

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    #endregion

    #region Not Implemented Tests

    [Test]
    [TestCase(PropertyDataTypeNumber.PDT_DATE)]
    [TestCase(PropertyDataTypeNumber.PDT_TIME)]
    [TestCase(PropertyDataTypeNumber.PDT_DATE_TIME)]
    [TestCase(PropertyDataTypeNumber.PDT_POLL_GROUP_SETTINGS)]
    public void PdtEncoder_NotImplementedTypes_ThrowsNotImplementedException(PropertyDataTypeNumber pdtNumber)
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[pdtNumber] as PdtEncoder<byte[]>;
        byte[] testValue = [0x01];

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => encoder!.Encoder(testValue));
    }

    [Test]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_25)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_26)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_27)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_28)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_29)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_2A)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_2B)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_2C)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_2D)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_2E)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_37)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_38)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_39)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_3A)]
    [TestCase(PropertyDataTypeNumber.PDT_RESERVED_3B)]
    public void PdtEncoder_ReservedTypes_ThrowsNotImplementedException(PropertyDataTypeNumber pdtNumber)
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[pdtNumber] as PdtEncoder<byte[]>;
        byte[] testValue = [0x01];

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => encoder!.Encoder(testValue));
    }

    #endregion

    #region Special Types Tests

    [Test]
    public void PdtEncoder_NeVl_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_NE_VL] as PdtEncoder<byte[]>;
        byte[] testValue = [0x01, 0x02, 0x03];

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_NeFl_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_NE_FL] as PdtEncoder<byte[]>;
        byte[] testValue = [0x01, 0x02, 0x03, 0x04];

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Function_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_FUNCTION] as PdtEncoder<byte[]>;
        byte[] testValue = [0xAA, 0xBB];

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Escape_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_ESCAPE] as PdtEncoder<byte[]>;
        byte[] testValue = [0xFF, 0x00];

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    #endregion

    #region Dictionary Completeness Tests

    [Test]
    public void PdtEncodersByNumber_ContainsAllExpectedEncoders()
    {
        // Arrange
        var expectedCount = Enum.GetValues<PropertyDataTypeNumber>().Length;

        // Act
        var actualCount = _factory.PdtEncodersByNumber.Count;

        // Assert
        Assert.That(actualCount, Is.EqualTo(expectedCount),
            "The factory should have an encoder for every PropertyDataTypeNumber");
    }

    [Test]
    public void PdtEncodersByNumber_AllEncodersHaveValidTypes()
    {
        // Act & Assert
        foreach (var kvp in _factory.PdtEncodersByNumber)
        {
            Assert.That(kvp.Value, Is.Not.Null, $"Encoder for {kvp.Key} should not be null");
            Assert.That(kvp.Value.Type, Is.Not.Null, $"Type for {kvp.Key} encoder should not be null");
            Assert.That(kvp.Value.OEncoder, Is.Not.Null, $"OEncoder for {kvp.Key} should not be null");
            Assert.That(kvp.Value.ODecoder, Is.Not.Null, $"ODecoder for {kvp.Key} should not be null");
        }
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Test]
    public void PdtEncoder_UnsignedChar_HandlesMinValue()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_UNSIGNED_CHAR] as PdtEncoder<byte>;
        byte testValue = byte.MinValue;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_UnsignedChar_HandlesMaxValue()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_UNSIGNED_CHAR] as PdtEncoder<byte>;
        byte testValue = byte.MaxValue;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Char_HandlesMinValue()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_CHAR] as PdtEncoder<sbyte>;
        sbyte testValue = sbyte.MinValue;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Char_HandlesMaxValue()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_CHAR] as PdtEncoder<sbyte>;
        sbyte testValue = sbyte.MaxValue;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Int_HandlesMinValue()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_INT] as PdtEncoder<short>;
        short testValue = short.MinValue;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_Int_HandlesMaxValue()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_INT] as PdtEncoder<short>;
        short testValue = short.MaxValue;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(testValue));
    }

    [Test]
    public void PdtEncoder_EmptyString_HandlesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_UTF_8] as PdtEncoder<string>;
        string testValue = string.Empty;

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded, Is.EqualTo(string.Empty));
    }

    [Test]
    public void PdtEncoder_EmptyByteArray_HandlesCorrectly()
    {
        // Arrange
        var encoder = _factory.PdtEncodersByNumber[PropertyDataTypeNumber.PDT_VARIABLE_LENGTH] as PdtEncoder<byte[]>;
        byte[] testValue = [];

        // Act
        var encoded = encoder!.Encoder(testValue);
        var decoded = encoder.Decoder(encoded);

        // Assert
        Assert.That(decoded.Length, Is.EqualTo(0));
    }

    #endregion
}
