using System.Text.Json.Nodes;

namespace SRF.Knx.Config.OpenHab.BaseConfig.Modifiers;

public class ChangeGA(OHKnxGroupAddress current, OHKnxGroupAddress changeTo) : IOpenHabKnxBaseConfigModifier
{
    private readonly OHKnxGroupAddress current = current;
    private readonly OHKnxGroupAddress changeTo = changeTo;

    public void Modify(KnxOpenHabConfig ohMeta)
    {
        // find the current thing the GA is part of
        var thing = ohMeta.Things.Find(t => t.GroupAddresses.Exists(ga => ga.Address.Address == current.Address.Address));
        if (thing == null)
        {
            throw new Exception($"Could not find GA {current.Address.AddressAsString} '{current.Label}' to change it.");
        }

        var item = thing.GroupAddresses.Find(ga => ga.Address.Address == current.Address.Address);

        if ( item?.EntryStatus.HasFlag(Domain.ExtraConfigStatus.Manual) ?? false)
        {
            return;
        }

        if (item != null)
            thing.GroupAddresses.Remove(item);
        thing.GroupAddresses.Add(changeTo);
    }

    public void Modify(JsonNode ohMeta)
    {
        throw new NotImplementedException();
    }
}
