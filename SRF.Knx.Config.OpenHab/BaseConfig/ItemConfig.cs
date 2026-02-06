using System.Text.Json.Serialization;
using SRF.Knx.Config.OpenHab.Templating.Items;

namespace SRF.Knx.Config.OpenHab.BaseConfig;

public partial class OHKnxGroupAddress
{
    public class ItemConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public ItemType Type { get; set; } = Templating.Items.ItemType.Undefined;
        public string? Icon { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string[] Groups { get; set; } = [];
    }
}
