using System.Numerics;
using Microsoft.Extensions.Logging.Abstractions;
using SRF.Knx.Core;
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
    /// Test certain DPSTs for correct setting of IScaledNumeric
    /// </summary>
    /// <param name="main">Main number of the DPT to test</param>
    /// <param name="sub">Sub number of the DPT to test</param>
    /// <param name="isScaledNumeric">Expected value for IsScaledNumeric property of the DPT</param>
    [TestCase(5, 1, true)] // DPT 5.001 is a scaled numeric DPT, so IsScaledNumeric should be true
    [TestCase(5, 3, true)] // DPT 5.003 is a scaled numeric DPT, so IsScaledNumeric should be true
    [TestCase(5, 4, false)] // DPT 5.004 is 1:1 mapping 0...255 to 0...255%, coefficient not defined in master data.
    [TestCase(1, 1, false)] // DPT 1.001 is a boolean DPT, which is not a scaled numeric, so IsScaledNumeric should be false
    [TestCase(7, 3, true)] // DPT 7.003 is a scaled numeric DPT, so IsScaledNumeric should be true
    [TestCase(9, 1, false)] // DPT 9.001 is a KNX 2-byte float, which is a non-scaled numeric DPT, so IsScaledNumeric should be false
    public void IsScaledNumeric_CorrectlySet(int main, int sub, bool isScaledNumeric)
    {
        var dpt = _factory.Get(main, sub);
        Assert.That(dpt, Is.Not.Null);
        Assert.That(dpt.Id.Main, Is.EqualTo(main));
        Assert.That(dpt.Id.Sub, Is.EqualTo(sub));
        Assert.That(dpt.IsScaledNumeric, Is.EqualTo(isScaledNumeric), $"IsScaledNumeric property of DPT {dpt.Id} does not match the expected value. Expected: {isScaledNumeric}, Actual: {dpt.IsScaledNumeric}");
    }

    /// <summary>
    /// Test for correct types KNX and Application for scaled and non-scaled DPTs, correct application of coefficients for scaling and error handling for invalid inputs.
    /// </summary>
    /// <param name="main">Main number of the DPT to test</param>
    /// <param name="sub">Sub number of the DPT to test</param>
    /// <param name="coefficient">Coefficient for scaling the DPT value</param>
    /// <param name="appValue">Test value in the application type to encode and decode, which should be within the valid range of the DPT;
    /// attention: choose correct type in TestCase, e.g. 10.0 instead of 10 for double values required for scaled numeric DPTs</param>
    /// <param name="gaValue">Group Address telegram native type value to test encoding and decoding of, which should be within the valid range of the DPT</param>
    /// <param name="eps">Epsilon value for floating point comparisons</param>
    [TestCase(5, 1, 100.0 / 255.0, 50.0, new byte[] { 0x7f }, 100.0 / 255.0 * 0.51)] // DPT 5.001 is a scaled numeric with a coefficient of 100/255
    [TestCase(5, 1, 100.0 / 255.0, 100.0, new byte[] { 0xff }, 100.0/255.0*0.51)] // DPT 5.001 with a double test value
    [TestCase(5, 3, 360.0 / 255.0, 30.0, new byte[] { 21 }, 360.0 / 255.0 * 0.51)] // DPT 5.003 is a scaled numeric with a coefficient of 360/255
    [TestCase(5, 4, 1.0, (byte)128, new byte[] { 0x80 }, 128.0)] // DPT 5.004 is a non-scaled numeric with a coefficient of 1.0 (or no coefficient), so the test value should be encoded and decoded without applying any coefficient
    [TestCase(1, 1, 0.0, true, new byte[] { 1 }, 0.5)] // DPT 1.001 is a boolean, so the test value of true should be encoded to 1 and decoded back to true
    [TestCase(7, 3, 10.0, 200.0, new byte[] { 0x00, 0x14 }, 200.0/10.0*0.51)] // DPT 7.003 is a scaled numeric with a coefficient of 10 encoded in KNX as 16 bit unsigned integer.
    [TestCase(9, 1, 1.0, 23.5f, new byte[] { 0x0c, 0x97 }, 0.001)] // DPT 9.001 is a KNX 2-byte float, which is a non-scaled numeric, so the test value should be encoded and decoded without applying any coefficient
    public void EncodeDecode_ScaledAndNonScaledDpts_CorrectTypesAndValues(int main, int sub, double coefficient, object appValue, byte[] gaValue, double eps)
    {
        var dpt = _factory.Get(main, sub);
        Assert.That(dpt, Is.Not.Null);
        Assert.That(dpt.Id.Main, Is.EqualTo(main));
        Assert.That(dpt.Id.Sub, Is.EqualTo(sub));
        if (dpt.IsScaledNumeric)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(appValue.GetType(), Is.EqualTo(typeof(double)), $"Application value for scaled numeric DPT {dpt.Id} should be of type double to ensure correct application of the coefficient for scaling. Actual type of the application value is {appValue.GetType().Name}. Fix the test setup.");
                Assert.That(dpt, Is.InstanceOf<DptSimple>());
            }

            var dptSimpleT = (DptSimple)dpt;
            Assert.That(dptSimpleT.NumericInfo, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dptSimpleT.NumericInfo!.Coefficient, Is.EqualTo(coefficient).Within(eps), $"Coefficient of the DPT does not match the expected value. Expected: {coefficient}, Actual: {dptSimpleT.NumericInfo.Coefficient}, Epsilon: {eps}");
                Assert.That(dpt.ApplicationType, Is.EqualTo(typeof(double)));
            }

            var coefficientMaster = dpt.IsScaledNumeric && dpt is DptSimple dptSimple ? dptSimple.NumericInfo?.Coefficient ?? 1.0 : 1.0;
            Assert.That(coefficientMaster, Is.EqualTo(coefficient).Within(100.0/(255.0*100)), $"Coefficient for DPT {dpt.Id} does not match the expected value. Expected: {coefficient}, Actual: {coefficientMaster}");
        }
        else
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dpt.ApplicationType, Is.EqualTo(dpt.ValueType), $"For non-scaled DPT {dpt.Id}, the application type should be the same as the value type. Expected: {dpt.ValueType}, Actual: {dpt.ApplicationType}");
                Assert.That(appValue.GetType(), Is.EqualTo(dpt.ApplicationType), $"Application value type does not match the expected application type of the DPT. Expected: {dpt.ApplicationType}, Actual: {appValue.GetType()}. Fix the test setup.");
            }

            Assert.That(dpt.ApplicationType, Is.EqualTo(dpt.ValueType));
        }

        // encoding test
        var groupValue = dpt.ToGroupValue(appValue);
        Assert.That(groupValue, Is.Not.Null);
        Assert.That(groupValue.Value, Is.EqualTo(gaValue), $"Encoded group value bytes do not match the expected bytes. Expected: {BitConverter.ToString(gaValue)}, Actual: {BitConverter.ToString(groupValue.Value)}");

        // decoding test
        var decodedAppValue = dpt.ToValue(groupValue);
        Assert.That(decodedAppValue, Is.Not.Null);
        if (appValue is IConvertible && decodedAppValue is IConvertible)
        {
            double appValueDouble = Convert.ToDouble(appValue, System.Globalization.CultureInfo.InvariantCulture);
            double decodedAppValueDouble = Convert.ToDouble(decodedAppValue, System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(decodedAppValueDouble, Is.EqualTo(appValueDouble).Within(eps), $"Decoded application value does not match the original application value within the expected epsilon. Expected: {appValueDouble}, Actual: {decodedAppValueDouble}, Epsilon: {eps}");
        }
        else
        {
            Assert.That(decodedAppValue, Is.EqualTo(appValue), $"Decoded application value does not match the original application value. Expected: {appValue}, Actual: {decodedAppValue}");
        }
    }
}
