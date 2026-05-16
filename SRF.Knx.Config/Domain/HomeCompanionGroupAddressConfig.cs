namespace SRF.Knx.Config.Domain;

/// <summary>
/// HomeCompanion-specific additional configuration for KNX group addresses.
/// Kept as member of <see cref="GroupAddressExtraConfig"/> for now.
/// </summary>
/// <remarks>
/// Used for code generation of HomeCompanion KNX bus/group address mapped <c>IValue</c> entities, e.g. to determine whether read requests should be answered for a specific group address.
/// </remarks>
public class HomeCompanionGroupAddressConfig
{
    public bool AnswerReadRequests { get; set; } = false;

    public bool InitializeFromOpenHab { get; set; } = true;

    public bool InitializeFromKnxBus { get; set; } = false;
}
