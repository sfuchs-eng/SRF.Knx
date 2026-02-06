using System.Text.Json.Serialization;
using System.Xml.Serialization;
using SRF.Knx.Core;

namespace SRF.Knx.Config.OpenHab.BaseConfig;

[Serializable]
public class BridgeConfig
{
    public string BindingId { get; set; } = "knx";

    public string BindingTypeId { get; set; } = "ip";

    [XmlAttribute]
    public string Name { get; set; } = "bridge1";

    [XmlElement("KnxDeviceAddress")]
    [JsonPropertyName("KnxDeviceAddress")]
    public string KnxDeviceAddressXmlProxy
    {
        get
        {
            return KnxDeviceAddress.ToString();
        }
        set
        {
            KnxDeviceAddress = new IndividualAddress(value);
        }
    }

    [XmlIgnore]
    [JsonIgnore]
    public IndividualAddress KnxDeviceAddress { get; set; } = new("0.0.0");

    [XmlAttribute]
    public BridgeType Type { get; set; } = BridgeType.ROUTER;

    public string? Description { get; set; }
}
