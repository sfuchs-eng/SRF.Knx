using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace SRF.Knx.Config.OpenHab.Templating.Items;

/// <summary>
/// Template config for OpenHAB Items.
/// <see cref="ItemConfig"/> reflects the OpenHAB specific properties of an OpenHAB Item template.
/// <see cref="ItemConfig"/> objects are stored as templates in a json file and used to generate group address specific
/// objects of <see cref="OHKnxGroupAddress"/> which are stored in json files too, allowing manual config overrides.
/// The OpenHAB config generators, e.g. <see cref="Generate.v3.Item"/>, consume <see cref="OHKnxGroupAddress"/>.
/// </summary>
[Serializable]
public class ItemConfig {
    [XmlAttribute]
    public ItemType Type { get; set; } = ItemType.Undefined;

    [XmlAttribute]
    public string? ValueFormat { get; set; }
    [XmlIgnore]
    [JsonIgnore]
    public bool ValueFormatSpecified { get => !string.IsNullOrEmpty(ValueFormat);  set { if (!value) ValueFormat = null; } }

    [XmlAttribute]
    public string? Icon { get; set; }
    [XmlIgnore]
    [JsonIgnore]
    public bool IconSpecified { get => !string.IsNullOrEmpty(Icon);  set { if (!value) Icon = null; } }

    /// <summary>
    /// The KNX DPT / DPST to be used in dot-format.
    /// The DPST may differ from ETS5 while the DPT (data packet structure and payload length) must match.
    /// </summary>
    [XmlAttribute]
    public string? DataType { get; set; }
    [XmlIgnore]
    [JsonIgnore]
    public bool DataTypeSpecified { get => !string.IsNullOrEmpty(DataType);  set { if (!value) DataType = null; } }

    /// <summary>
    /// Is it a readonly Group Address? E.g. a sensor value?
    /// </summary>
    [XmlAttribute]
    public bool IsWritable { get; set; } = true;
    [XmlIgnore]
    [JsonIgnore]
    public bool IsWritableSpecified { get => !IsWritable; }

    /// <summary>
    /// Is the KNX bus expected to answer a ReadRequest?
    /// </summary>
    [XmlAttribute]
    public bool IsReadable { get; set; } = true;
    [XmlIgnore]
    [JsonIgnore]
    public bool IsReadableSpecified { get => !IsReadable; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = false });
    }
}
