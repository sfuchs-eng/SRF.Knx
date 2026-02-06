using System.Text.Json.Nodes;

namespace SRF.Knx.Config.OpenHab.BaseConfig.Modifiers;

public interface IOpenHabKnxBaseConfigModifier
{
    void Modify(KnxOpenHabConfig ohMeta);
    void Modify(JsonNode ohMeta);
}
