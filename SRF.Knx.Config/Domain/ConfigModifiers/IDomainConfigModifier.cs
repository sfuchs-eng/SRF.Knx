using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using SRF.Knx.Config.Domain;

namespace SRF.Knx.Config.Domain.ConfigModifiers;

public interface IDomainConfigModifier
{
    void Modify(DomainConfiguration domainConfig);
    void Modify(JsonNode domainConfigRoot);
}
