using System.Text.Json.Serialization;
using System.Xml.Serialization;

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
            KnxDeviceAddress = new KnxDeviceAddress(value);
        }
    }

    [XmlIgnore]
    [JsonIgnore]
    public KnxDeviceAddress KnxDeviceAddress { get; set; } = new("0.0.0");

    [XmlAttribute]
    public BridgeType Type { get; set; } = BridgeType.ROUTER;

    public string? Description { get; set; }
}
