using System.Xml.Serialization;
using SRF.Knx.Config.OpenHab.Templating.Items;

namespace SRF.Knx.Config.OpenHab.Templating;

/// <summary>
/// The root element for the OpenHAB item templates xml file.
/// Holds pattern matches on e.g. GA Config Labels / Names / ... and corresponding OpenHAB related default config settings (e.g. ItemType).
/// </summary>
[Serializable]
public class ConfigTemplates {
    [XmlElement("Match")]
    public EtsGroupAddressMatch[] ItemTemplates { get; set; } = [];
}
