using System.Text.Json.Nodes;
using SRF.Knx.Core;

namespace SRF.Knx.Config.OpenHab.BaseConfig.Modifiers;

public class DeleteGA(OHKnxGroupAddress toDelete) : IOpenHabKnxBaseConfigModifier
{
    private readonly Core.GroupAddress toDelete = toDelete.Address;

    public void Modify(KnxOpenHabConfig ohMeta)
    {
        throw new NotImplementedException();
    }

    public void Modify(JsonNode ohMeta)
    {
        throw new NotImplementedException();
    }
}
