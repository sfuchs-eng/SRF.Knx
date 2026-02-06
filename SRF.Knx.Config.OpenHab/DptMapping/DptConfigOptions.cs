using System.Text.Json.Serialization;
using SRF.Knx.Config.OpenHab.Generate;
using SRF.Knx.Config.OpenHab.Templating.Items;

namespace SRF.Knx.Config.OpenHab.DptMapping;

public partial class DptMappingLookupItem
{
    public class DptConfigOptions
    {
        public ChannelStereotype Stereotype { get; set; } = ChannelStereotype.Any;
        
        public ChannelType ChannelType { get; set; }

        public string Parameter { get; set; } = "ga";

        public ItemType ItemType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenHabDimension? Dimension { get; set; }
    }
}
