using Microsoft.Extensions.Logging.Abstractions;
using SRF.Knx.Core.DPT;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Test.Core;

/// <summary>
/// Tests for encoding and decoding of DPT values.
/// Including data type conversions, scaling with coefficients, and error handling for invalid inputs.
/// </summary>
[TestFixture]
public class DptEncodingTests
{
    private DptFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        var baseDir = Path.GetDirectoryName(typeof(DptEncodingTests).Assembly.Location) ?? "";
        var knxMasterFilePath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "SRF.Knx.Config", "Resources", "knx_master.xml"));

        if (!File.Exists(knxMasterFilePath))
            Assert.Fail($"knx_master.xml not found at: {knxMasterFilePath}");

        var masterData = KnxMasterDataLoader.LoadFromFile(knxMasterFilePath);
        var provider = new KnxMasterDataProviderStub(masterData);

        _factory = new DptFactory(
            provider,
            new PdtEncoderFactory(),
            new DptNumericInfoFactory(NullLogger<DptNumericInfoFactory>.Instance),
            NullLogger<DptFactory>.Instance
        );
    }

    private class KnxMasterDataProviderStub : KnxMasterDataProvider
    {
        private readonly KnxMasterData _masterData;

        public KnxMasterDataProviderStub(KnxMasterData masterData)
        {
            _masterData = masterData;
        }

        public override KnxMasterData GetMasterData()
        {
            return _masterData;
        }
    }

    /// <summary>
    /// Test for correct types KNX and Application for scaled and non-scaled DPTs, correct application of coefficients for scaling and error handling for invalid inputs.
    /// </summary>
    /// <param name="main">Main number of the DPT to test</param>
    /// <param name="sub">Sub number of the DPT to test</param>
    /// <param name="coefficient">Coefficient for scaling the DPT value</param>
    /// <param name="testValue">Group Address telegram native type value to test encoding and decoding of, which should be within the valid range of the DPT</param>
    /// <param name="eps">Epsilon value for floating point comparisons</param>
    [TestCase(5, 1, 100.0 / 255.0, (byte)128, 0.001)] // DPT 5.001 is a scaled numeric with a coefficient of 100/255
    [TestCase(5, 1, 100.0 / 255.0, (byte)255, 0.001)] // DPT 5.001 with a double test value
    [TestCase(1, 1, 1.0, true, 0.001)] // DPT 1.001 is a boolean, so the test value of true should be encoded to 1 and decoded back to true
    [TestCase(9, 1, 1.0, 23.5f, 0.001)] // DPT 9.001 is a KNX 2-byte float, which is a non-scaled numeric, so the test value should be encoded and decoded without applying any coefficient
    public void EncodeDecode_ScaledAndNonScaledDpts_CorrectTypesAndValues(int main, int sub, double coefficient, object testValue, double eps)
    {
        var dpt = _factory.Get(main, sub);
        Assert.That(dpt, Is.Not.Null);
        Assert.That(dpt.Id.Main, Is.EqualTo(main));
        Assert.That(dpt.Id.Sub, Is.EqualTo(sub));
        Assert.That(dpt.IsScaledNumeric, Is.EqualTo(coefficient != 1.0));
        if (dpt.IsScaledNumeric)
        {
            Assert.That(dpt, Is.InstanceOf<DptSimple>());
            var dptSimpleT = (DptSimple)dpt;
            Assert.That(dptSimpleT.NumericInfo, Is.Not.Null);
            Assert.That(dptSimpleT.NumericInfo!.Coefficient, Is.EqualTo(coefficient).Within(eps));
            Assert.That(dpt.ApplicationType, Is.EqualTo(typeof(double)));
        }
        else
        {
            Assert.That(dpt.ApplicationType, Is.EqualTo(dpt.ValueType));
        }
        var coefficientMaster = dpt.IsScaledNumeric && dpt is DptSimple dptSimple ? dptSimple.NumericInfo?.Coefficient ?? 1.0 : 1.0;
        Assert.That(coefficientMaster, Is.EqualTo(coefficient).Within(eps), $"Coefficient for DPT {dpt.Id} does not match the expected value. Expected: {coefficient}, Actual: {coefficientMaster}");
        if (dpt.IsScaledNumeric)
        {
            // determine the expected application value and check whether both types are IConvertible to be able to apply the coefficient for scaling in the test
            if (testValue is IConvertible testValueConvertible1)
            {
                double doubleValue = testValueConvertible1.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
                doubleValue /= coefficient;
                var expectedEncodedValue = Convert.ChangeType(doubleValue, dpt.ValueType, System.Globalization.CultureInfo.InvariantCulture);
                var encodedValue = dpt.ToGroupValue(testValue!).Value;
                Assert.That(encodedValue, Is.EqualTo(expectedEncodedValue).Within(eps), $"Encoded value does not match the expected value for DPT {dpt.Id}. Expected: {expectedEncodedValue}, Actual: {encodedValue}");
            }
            else
            {
                Assert.Fail($"Test value for scaled DPT {dpt.Id} must be IConvertible to apply the coefficient for scaling in the test. Actual type of the provided test value is {testValue?.GetType().Name ?? "null"}");
            }
        }
        var groupValue = dpt.ToGroupValue(testValue!);
        var decodedValue = dpt.ToValue(groupValue);
        if (decodedValue is IConvertible decodedValueConvertible && testValue is IConvertible testValueConvertible2)
        {
            double decodedDouble = decodedValueConvertible.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
            double testDouble = testValueConvertible2.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(decodedDouble, Is.EqualTo(testDouble).Within(eps), $"Decoded value does not match the original test value for DPT {dpt.Id}. Expected: {testValue}, Actual: {decodedValue}");
        }
        else
        {
            Assert.That(decodedValue, Is.EqualTo(testValue).Within(eps), $"Decoded value does not match the original test value for DPT {dpt.Id}. Expected: {testValue}, Actual: {decodedValue}");
        }
    }
}
