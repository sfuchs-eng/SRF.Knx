using System;
using System.Text.Json.Serialization;

namespace SRF.Knx.Config.Domain;

public class GroupAddressExtraConfig()
{
    public ExtraConfigStatus EntryStatus { get; set; } = ExtraConfigStatus.Automatic | ExtraConfigStatus.Fresh;

    /// <summary>
    /// The Name is generated from the Label via a <see cref="ILabelToNameConverter"/>
    /// configured with the <see cref="DomainConfigurationFactory"/>.
    /// </summary>
    /// <value></value>
    public string? Name { get; set; }

    /// <summary>
    /// It's allowed to send read requests to the group address, if true. Default is true.
    /// In proper configurations there would also be a bus participant answering to these read requests, otherwise they would just time out.
    /// Typically set to false for write-only group addresses, e.g. for actuators without state feedback, triggers without reset, ...
    /// </summary>
    /// <value></value>
    public bool IsReadable { get; set; } = true;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HomeCompanionGroupAddressConfig? HomeCompanion { get; set; } = new HomeCompanionGroupAddressConfig();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GroupAddressExtraConfig? AutoLatest { get; set; }
}
