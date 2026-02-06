using System.Text.Json.Nodes;

namespace SRF.Knx.Config.Domain.ConfigModifiers;

public abstract class GAECModifierBase : IDomainConfigModifier
{
    public abstract void Modify(DomainConfiguration domainConfig);
    public abstract void Modify(JsonNode doaminConfigRoot);
}
