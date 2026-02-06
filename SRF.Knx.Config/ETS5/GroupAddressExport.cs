using System.Xml.Serialization;

namespace SRF.Knx.Config.ETS5;

[XmlRoot("GroupAddress-Export", Namespace = "http://knx.org/xml/ga-export/01")]
public class GroupAddressExport {
    [XmlElement("GroupRange")]
    public GroupRange[] Ranges { get; set; } = [];

    private static void Nullfix(GroupRange range)
    {
        range.GroupAddresses ??= [];
        range.SubRanges ??= [];
        foreach ( var r in range.SubRanges )
            Nullfix(r);
    }

    /// <summary>
    /// <see cref="System.Xml.Serializer"/> leaves null behind even for non-nullables instead of their default property initialization.
    /// </summary>
    public void FixNonNullablesAfterDeserialization()
    {
        foreach ( var r in Ranges)
            Nullfix(r);
    }

    [XmlIgnore]
    public IEnumerable<EtsGroupAddressConfig> AllGroupAddresses {
        get {
            return Ranges.SelectMany(r => r.AllGroupAddresses);
        }
    }
}
