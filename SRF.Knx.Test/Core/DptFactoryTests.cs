using Microsoft.Extensions.Logging.Abstractions;
using SRF.Knx.Core;
using SRF.Knx.Core.DPT;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Test.Core;

/// <summary>
/// Tests for <see cref="DptFactory"/> using the real knx_master.xml file.
/// Covers the three-tier DPT lookup strategy (by DPST ID, by PDT, and dynamic fallback),
/// error handling for invalid inputs, and metadata correctness.
/// </summary>
[TestFixture]
public class DptFactoryTests
{
    private DptFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        if (!KnxMasterDataUtils.TryGetKnxMasterData(out var filename, out var masterData, out var provider))
        {
            Assert.Fail("knx_master.xml file not found. Ensure that the file exists at the expected path relative to the test assembly: " + filename);
        }

        _factory = new DptFactory(
            provider,
            new PdtEncoderFactory(),
            new DptNumericInfoFactory(NullLogger<DptNumericInfoFactory>.Instance),
            NullLogger<DptFactory>.Instance
        );
    }

    [Test]
    public void Get_ByMainAndSub_ReturnsDptBase()
    {
        var dpt = _factory.Get(1, 1);

        Assert.That(dpt, Is.Not.Null);
        Assert.That(dpt.Id.Main, Is.EqualTo(1));
        Assert.That(dpt.Id.Sub, Is.EqualTo(1));
    }

    [Test]
    public void Get_ByDataPointTypeId_ReturnsDptBase()
    {
        var id = new DataPointTypeId(1, 1);

        var dpt = _factory.Get(id);

        Assert.That(dpt, Is.Not.Null);
        Assert.That(dpt.Id.Main, Is.EqualTo(1));
        Assert.That(dpt.Id.Sub, Is.EqualTo(1));
    }

    [Test]
    public void Get_MainOnlyId_ReturnsDptUsingMainTypePdt()
    {
        var mainOnlyId = new DataPointTypeId(1, 0);

        var dpt = _factory.Get(mainOnlyId);

        Assert.That(dpt, Is.Not.Null);
        Assert.That(dpt.Id.Main, Is.EqualTo(1));
    }

    [Test]
    public void Get_NonExistentDptMain_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _factory.Get(9999, 1));
    }

    [Test]
    public void Get_NonExistentDptSub_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _factory.Get(1, 9999));
    }

    [Test]
    [Description("DPT 5.001 is a percentage (0-100%) with a numeric range, so NumericInfo should be populated.")]
    public void Get_NumericDpt_HasNumericInfo()
    {
        // DPT 5.001 = DPT_Value_1_Ucount, percentage 0-100%
        var dpt = _factory.Get(5, 1);

        Assert.That(dpt, Is.InstanceOf<DptSimple>());
        var simple = (DptSimple)dpt;
        Assert.That(simple.NumericInfo, Is.Not.Null);
        Assert.That(simple.IsNumeric, Is.True);
    }

    [Test]
    [Description("DPT 1.001 is a 1-bit switch (Bit format), which is not a numeric range, so NumericInfo should be null.")]
    public void Get_BitDpt_HasNullNumericInfo()
    {
        // DPT 1.001 = DPT_Switch, single bit
        var dpt = _factory.Get(1, 1);

        Assert.That(dpt, Is.InstanceOf<DptSimple>());
        var simple = (DptSimple)dpt;
        Assert.That(simple.NumericInfo, Is.Null);
        Assert.That(simple.IsNumeric, Is.False);
    }

    [TestCase(1,  1,   Description = "DPST-1-1   switch")]
    [TestCase(1,  3,   Description = "DPST-1-3   enable")]
    [TestCase(1,  5,   Description = "DPST-1-5   alarm")]
    [TestCase(1,  6,   Description = "DPST-1-6   binary value (with low priority)")]
    [TestCase(1,  9,   Description = "DPST-1-9   open/close")]
    [TestCase(1,  10,  Description = "DPST-1-10  start/stop")]
    [TestCase(1,  11,  Description = "DPST-1-11  state")]
    [TestCase(1,  15,  Description = "DPST-1-15  reset")]
    [TestCase(1,  16,  Description = "DPST-1-16  ack")]
    [TestCase(1,  17,  Description = "DPST-1-17  trigger")]
    [TestCase(1,  19,  Description = "DPST-1-19  window/door")]
    [TestCase(3,  7,   Description = "DPST-3-7   dimming (controlled)")]
    [TestCase(5,  1,   Description = "DPST-5-1   percentage (0..100%)")]
    [TestCase(5,  3,   Description = "DPST-5-3   angle (degrees)")]
    [TestCase(5,  10,  Description = "DPST-5-10  value 1 byte unsigned (generic)")]
    [TestCase(9,  1,   Description = "DPST-9-1   temperature (°C)")]
    [TestCase(9,  4,   Description = "DPST-9-4   illuminance (lux)")]
    [TestCase(9,  5,   Description = "DPST-9-5   wind speed (m/s)")]
    [TestCase(14, 19,  Description = "DPST-14-19 electric current (A)")]
    [TestCase(14, 27,  Description = "DPST-14-27 electric potential (V)")]
    [TestCase(16, 1,   Description = "DPST-16-1  ASCII string")]
    [TestCase(17, 1,   Description = "DPST-17-1  scene number")]
    [TestCase(21, 1,   Description = "DPST-21-1  general status")]
    [TestCase(21, 601, Description = "DPST-21-601 1-10V dimmer status")]
    public void Get_WellKnownDpts_ReturnCorrectId(int main, int sub)
    {
        var dpt = _factory.Get(main, sub);

        Assert.That(dpt, Is.Not.Null);
        Assert.That(dpt.Id.Main, Is.EqualTo(main));
        Assert.That(dpt.Id.Sub, Is.EqualTo(sub));
    }

    [Test]
    [Description("DPST-17-1 (scene number) must decode to scalar byte, not byte[].")]
    public void Get_Dpst17_1_DecodesToByte()
    {
        var dpt = _factory.Get(17, 1);

        var decoded = dpt.ToValue(new GroupValue([0x3F]));

        Assert.That(decoded, Is.TypeOf<byte>());
        Assert.That(decoded, Is.EqualTo((byte)0x3F));
    }
}
