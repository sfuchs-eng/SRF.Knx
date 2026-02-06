using System.Text.Json.Nodes;

namespace SRF.Knx.Config.OpenHab.BaseConfig.Modifiers;

public class DeleteGA(OHKnxGroupAddress toDelete) : IOpenHabKnxBaseConfigModifier
{
    private readonly KnxGroupAddress toDelete = toDelete.Address;

    public void Modify(KnxOpenHabConfig ohMeta)
    {
        throw new NotImplementedException();
    }

    public void Modify(JsonNode ohMeta)
    {
        throw new NotImplementedException();
    }
}
