using System.Xml.Serialization;
using SRF.Knx.Config.OpenHab.Templating.Items;

namespace SRF.Knx.Config.OpenHab.Templating;

[Serializable]
public class EtsGroupAddressMatch {
    public EtsGroupAddressMatch()
    {
        Patterns = [];
        DPTMatches = Patterns;
    }

    public ItemConfig Template { get; set; } = new();

    /// <summary>
    /// OR match of pattern list on ETS Label
    /// </summary>
    [XmlElement("Pattern")]
    public string[] Patterns { get; set; }

    /// <summary>
    /// If any DPT / DPST is defined, at least one must match exactly.
    /// Both, one of DPTMatches and one of the patterns must match to have the template applied.
    /// </summary>
    [XmlElement("DPTMatch")]
    public string[] DPTMatches { get; set; }

    public override string ToString()
    {
        return Template.ToString();
    }
}
