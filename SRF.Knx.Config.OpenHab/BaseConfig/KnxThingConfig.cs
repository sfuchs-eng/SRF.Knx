using System.Text.Json.Serialization;
using System.Xml.Serialization;
using SRF.Knx.Config.OpenHab.Templating;

namespace SRF.Knx.Config.OpenHab.BaseConfig;

[Serializable]
public class KnxThingConfig
{
    /// <summary>
    /// Unique name for programmatical identification
    /// </summary>
    [OHConfigParam("Name")]
    public string Name { get; set; } = (Guid.NewGuid()).ToString();

    [OHConfigParam("Label")]
    [JsonIgnore]
    /// <summary>
    /// Label for GUI display purpose. Instead of using <see cref="Name"/>
    /// </summary>
    public string? Label { get { return _label ?? Name; } set { _label = value; } }
    private string? _label;
    [JsonPropertyName("Label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? JsonLabel { get => _label; set { _label = value; } }

    /// <summary>
    /// Human readable description
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [OHConfigParam("Location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Location { get; set; } = null;

    [OHConfigParam("TypeId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TypeId { get; set; }

    /// <summary>
    /// The full class name that implements the related functionality as digital twin.
    /// Serves custom code, not part of OpenHAB related functionality.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KnxThingClass { get; set; } = null;
    [XmlIgnore]
    [JsonIgnore]
    public bool KnxThingClassSpecified { get => KnxThingClass != null;  set { if (!value) KnxThingClass = null; } }

    public List<OHKnxGroupAddress> GroupAddresses { get; set; } = [];

    [OHConfigParam("address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AlifeMonitorIndividualAddress { get; set; }

    [OHConfigParam("fetch")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ReadDeviceParams { get; set; } = false;

    [OHConfigParam("pingInterval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PingInterval { get; set; } = null;

    [OHConfigParam("readInterval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ActiveReadInterval { get; set; } = 0;
}
