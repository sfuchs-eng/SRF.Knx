using System.Xml.Serialization;

namespace SRF.Knx.Config.ETS5;

[Serializable]
public class GroupRange {
    //<GroupRange Name="Wetter, Umwelt" RangeStart="1" RangeEnd="255" Unfiltered="true">
    [XmlAttribute]
    public string Name { get; set; } = "";

    [XmlAttribute]
    public int RangeStart { get; set; }

    [XmlAttribute]
    public int RangeEnd { get; set; }

    [XmlAttribute]
    public bool Unfiltered { get; set; }

    [XmlElement("GroupRange")]
    public GroupRange[] SubRanges { get; set; } = [];

    /// <summary>
    /// GroupAddresses attached to the range at hand without SubRanges.
    /// </summary>
    [XmlElement("GroupAddress")]
    public EtsGroupAddressConfig[] GroupAddresses { get; set; } = [];

    /// <summary>
    /// All group addresses, incl. those of SubRanges
    /// </summary>
    [XmlIgnore]
    public IEnumerable<EtsGroupAddressConfig> AllGroupAddresses {
        get {
            return GroupAddresses.Union(SubRanges.SelectMany(r => r.AllGroupAddresses));
        }
    }
}
