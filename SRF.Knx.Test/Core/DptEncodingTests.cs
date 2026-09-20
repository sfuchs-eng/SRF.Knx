using System.Numerics;
using Microsoft.Extensions.Logging.Abstractions;
using SRF.Knx.Core;
using SRF.Knx.Core.DPT;
using SRF.Knx.Core.Master;
using UnitsNet;

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
            new UnitSystemsMapper(provider, NullLogger<UnitSystemsMapper>.Instance),
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
    [TestCase(5, 1, 100.0 / 255.0, 50.0, new byte[] { 0x80 }, 100.0 / 255.0 * 0.51)] // DPT 5.001 is a scaled numeric with a coefficient of 100/255
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
        var normalizedAppValue = NormalizeToApplicationType(dpt, appValue);

        if (dpt.IsScaledNumeric)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(appValue.GetType(), Is.EqualTo(typeof(double)), $"Application value for scaled numeric DPT {dpt.Id} should be of type double to ensure correct application of the coefficient for scaling. Actual type of the application value is {appValue.GetType().Name}. Fix the test setup.");
                Assert.That(dpt, Is.InstanceOf<DptSimple>());
                Assert.That(normalizedAppValue.GetType(), Is.EqualTo(dpt.ApplicationType));
            }

            var dptSimpleT = (DptSimple)dpt;
            Assert.That(dptSimpleT.NumericInfo, Is.Not.Null);
            Assert.That(dptSimpleT.NumericInfo!.Coefficient, Is.EqualTo(coefficient).Within(eps), $"Coefficient of the DPT does not match the expected value. Expected: {coefficient}, Actual: {dptSimpleT.NumericInfo.Coefficient}, Epsilon: {eps}");

            var coefficientMaster = dpt.IsScaledNumeric && dpt is DptSimple dptSimple ? dptSimple.NumericInfo?.Coefficient ?? 1.0 : 1.0;
            Assert.That(coefficientMaster, Is.EqualTo(coefficient).Within(100.0/(255.0*100)), $"Coefficient for DPT {dpt.Id} does not match the expected value. Expected: {coefficient}, Actual: {coefficientMaster}");
        }
        else
        {
            Assert.That(normalizedAppValue.GetType(), Is.EqualTo(dpt.ApplicationType), $"Application value type does not match the expected application type of the DPT. Expected: {dpt.ApplicationType}, Actual: {normalizedAppValue.GetType()}. Fix the test setup.");
            if (!typeof(IQuantity).IsAssignableFrom(dpt.ApplicationType))
                Assert.That(dpt.ApplicationType, Is.EqualTo(dpt.BaseType), $"For non-unit-aware DPT {dpt.Id}, the application type should be the same as the value type. Expected: {dpt.BaseType}, Actual: {dpt.ApplicationType}");
        }

        // encoding test
        var groupValue = dpt.ToGroupValue(normalizedAppValue);
        Assert.That(groupValue, Is.Not.Null);
        Assert.That(groupValue.Value, Is.EqualTo(gaValue), $"Encoded group value bytes do not match the expected bytes. Expected: {BitConverter.ToString(gaValue)}, Actual: {BitConverter.ToString(groupValue.Value)}");

        // decoding test
        var decodedAppValue = dpt.ToValue(groupValue);
        Assert.That(decodedAppValue, Is.Not.Null);

        if (normalizedAppValue is IQuantity expectedQuantity && decodedAppValue is IQuantity decodedQuantity)
        {
            var expectedDecodedFromKnx = dpt.ToValue(new GroupValue(gaValue));
            Assert.That(expectedDecodedFromKnx, Is.InstanceOf<IQuantity>(), $"Expected KNX decoding for DPT {dpt.Id} should produce an IQuantity.");

            var knxUnit = ResolveKnxUnit(dpt);
            var expectedMagnitude = ((IQuantity)expectedDecodedFromKnx).As(knxUnit);
            var decodedMagnitude = decodedQuantity.As(knxUnit);
            Assert.That(decodedMagnitude, Is.EqualTo(expectedMagnitude).Within(eps), $"Decoded quantity value does not match the expected KNX-decoded quantity value within the expected epsilon. Expected: {expectedMagnitude}, Actual: {decodedMagnitude}, Epsilon: {eps}");
            return;
        }

        if (normalizedAppValue is IConvertible && decodedAppValue is IConvertible)
        {
            double appValueDouble = Convert.ToDouble(normalizedAppValue, System.Globalization.CultureInfo.InvariantCulture);
            double decodedAppValueDouble = Convert.ToDouble(decodedAppValue, System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(decodedAppValueDouble, Is.EqualTo(appValueDouble).Within(eps), $"Decoded application value does not match the original application value within the expected epsilon. Expected: {appValueDouble}, Actual: {decodedAppValueDouble}, Epsilon: {eps}");
        }
        else
        {
            Assert.That(decodedAppValue, Is.EqualTo(normalizedAppValue), $"Decoded application value does not match the original application value. Expected: {normalizedAppValue}, Actual: {decodedAppValue}");
        }
    }

    [TestCase(5, 1, (byte)0xff)]
    [TestCase(5, 3, (byte)0xff)]
    [Description("Scaled unit-aware DPTs must apply the KNX coefficient exactly once when decoding and formatting.")]
    public void DecodeAndFormat_ScaledUnitAwareDpts_ApplyCoefficientExactlyOnce(int main, int sub, byte rawValue)
    {
        var dpt = _factory.Get(main, sub);

        Assert.That(typeof(IQuantity).IsAssignableFrom(dpt.ApplicationType),
            $"Test expects a unit-aware DPT, but {dpt.Id} has application type {dpt.ApplicationType.Name}.");
        Assert.That(dpt, Is.InstanceOf<DptSimple>());

        var simple = (DptSimple)dpt;
        var coefficient = simple.NumericInfo?.Coefficient ?? 1.0;
        var eps = Math.Max(0.001, Math.Abs(coefficient) * 0.51);

        var groupValue = new GroupValue([rawValue]);
        var decoded = dpt.ToValue(groupValue);

        Assert.That(decoded, Is.InstanceOf<IQuantity>());
        var quantity = (IQuantity)decoded;
        var knxUnit = ResolveKnxUnit(dpt);

        var expectedMagnitude = rawValue * coefficient;
        var actualMagnitude = quantity.As(knxUnit);
        Assert.That(actualMagnitude, Is.EqualTo(expectedMagnitude).Within(eps),
            $"Decoded magnitude for {dpt.Id} must be raw*coefficient exactly once.");

        var roundTrip = dpt.ToGroupValue(decoded);
        Assert.That(roundTrip.Value, Is.EqualTo(groupValue.Value),
            $"Round-trip encode/decode for {dpt.Id} must preserve raw telegram value.");

        var expectedFormatted = quantity.ToString("S1", System.Globalization.CultureInfo.InvariantCulture);
        var formatted = dpt.Format(groupValue, "en", System.Globalization.CultureInfo.InvariantCulture, null);
        Assert.That(formatted, Is.EqualTo(expectedFormatted),
            $"DPT formatter for {dpt.Id} must reflect the correctly scaled quantity.");
    }

    private static object NormalizeToApplicationType(DptBase dpt, object appValue)
    {
        if (dpt.ApplicationType.IsInstanceOfType(appValue))
            return appValue;

        if (typeof(IQuantity).IsAssignableFrom(dpt.ApplicationType))
        {
            if (appValue is not IConvertible)
                throw new InvalidOperationException($"Test value for DPT {dpt.Id} must be numeric when application type is quantity. Actual type: {appValue.GetType().FullName}");

            var knxUnit = ResolveKnxUnit(dpt);

            var magnitude = Convert.ToDouble(appValue, System.Globalization.CultureInfo.InvariantCulture);
            var quantity = Quantity.From(magnitude, knxUnit);
            if (!dpt.ApplicationType.IsInstanceOfType(quantity))
                throw new InvalidOperationException($"Constructed quantity type {quantity.GetType().FullName} does not match DPT application type {dpt.ApplicationType.FullName} for DPT {dpt.Id}.");

            return quantity;
        }

        if (appValue is IConvertible)
            return Convert.ChangeType(appValue, dpt.ApplicationType, System.Globalization.CultureInfo.InvariantCulture)
                   ?? throw new InvalidOperationException($"Could not convert test value for DPT {dpt.Id} to {dpt.ApplicationType.FullName}");

        return appValue;
    }

    private static Enum ResolveKnxUnit(DptBase dpt)
    {
        var knxUnitProperty = dpt.GetType().GetProperty("KnxUnit");
        if (knxUnitProperty?.GetValue(dpt) is not Enum knxUnit)
            throw new InvalidOperationException($"DPT {dpt.Id} is quantity-based, but KnxUnit could not be resolved from type {dpt.GetType().FullName}.");
        return knxUnit;
    }
}
