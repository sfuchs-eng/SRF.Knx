using System.Text.Json.Serialization;
using SRF.Knx.Config.OpenHab.DptMapping;
using SRF.Knx.Config.OpenHab.Generate;
using SRF.Knx.Core.DPT;

namespace SRF.Knx.Config.OpenHab.BaseConfig;

public partial class OHKnxGroupAddress
{
    public class ChannelConfig
    {
        public string Name { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Parameter { get; set; }

        public ChannelType Type { get; set; } = ChannelType.Default;
        
        /// <summary>
        /// KNX Data Point Type, but the one used for OpenHAB which might not be equal to the one set in ETS.
        /// </summary> 
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DPTs { get => DPT?.DotFormat; set => DPT = new(value); }

        [JsonIgnore]
        public DataPointTypeId? DPT { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsStateOwned { get; set; } = false;

        public bool IsWritable { get; set; } = true;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsReadable { get; set; } = false;

        /// <summary>
        /// Parameter name this Group Address shall serve as status for.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? StatusFor { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenHabDimension? Dimension { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? KnxUnit { get; set; } 
    }
}
