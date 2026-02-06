using System.Text.Json.Serialization;
using System.Xml.Serialization;
using SRF.Knx.Core;
using SRF.Knx.Core.DPT;

namespace SRF.Knx.Config.ETS5;

/// <summary>
/// GroupAddress as exported by ETS 5 into the Group Address export xml file.
/// </summary>
[Serializable]
[XmlRoot(ElementName = "GroupAddress", Namespace = "http://knx.org/xml/ga-export/01")]
public class EtsGroupAddressConfig
{
    // <GroupAddress Name="Helligkeit Deckenspots im Gang (O6.43, O6.47, E5.22, E5.23)" Address="0/1/1" Unfiltered="true" DPTs="DPST-5-1" />
    [XmlAttribute("Name")]
    [JsonPropertyName("Label")]
    public string Label { get; set; } = "";
    [XmlIgnore]
    [JsonIgnore]
    public bool NameSpecified { get => !string.IsNullOrEmpty(Label); set { if (!value) Label = ""; } }

    [XmlAttribute("Address")]
    [JsonPropertyName("Address")]
    public string AddressProxyString
    {
        get
        {
            return Address.ToString();
        }
        set
        {
            Address = new GroupAddress(value);
        }
    }   
    [XmlIgnore]
    [JsonIgnore]
    public bool AddressSpecified { get => Address.Address == 0; set { } }
    [XmlIgnore]
    [JsonIgnore]
    public GroupAddress Address { get; set; } = new();

    [XmlAttribute]
    public bool Unfiltered { get; set; }

    /// <summary>
    /// Data point type as string in ETS notation (DPT-1, DPST-1-1)
    /// </summary>
    [XmlAttribute]
    public string DPTs
    {
        get
        {
            return DPT.EtsFormat;
        }
        set
        {
            DPT = new DataPointTypeId(value);
        }
    }
    [XmlIgnore]
    [JsonIgnore]
    public bool DPTsSpecified { get => _dpt != null && _dpt.IsValidType; }

    private DataPointTypeId? _dpt;
    [XmlIgnore]
    [JsonIgnore]
    public DataPointTypeId DPT
    {
        get
        {
            _dpt ??= DataPointTypeId.CreateInvalid();
            return _dpt; // ?? throw new Exceptions.KnxGAConfigurationException($"DPT of ETS GA {Address} is not configured.");
        }
        set
        {
            _dpt = value;
        }
    }
    [XmlIgnore]
    [JsonIgnore]
    public bool HasValidDPT { get => _dpt != null && _dpt.IsValidType; }

    [XmlAttribute]
    public string? Description { get; set; }
    [XmlIgnore]
    [JsonIgnore]
    public bool DescriptionSpecified { get => !string.IsNullOrEmpty(Description); set { if (!value) Description = null; } }

    [XmlAttribute]
    public GroupAddressSecurity Security { get; set; } = GroupAddressSecurity.Off;
    [XmlIgnore]
    [JsonIgnore]
    public bool SecuritySpecified { get => Security != GroupAddressSecurity.Off; set { if (!value) Security = GroupAddressSecurity.Off; } }

    public override string ToString()
    {
        return $"[{Address}, '{Label}', {DPTs}]";
    }
}
