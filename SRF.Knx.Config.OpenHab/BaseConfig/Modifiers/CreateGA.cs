using System.Text.Json.Nodes;
using SRF.Knx.Config.Domain;
using SRF.Knx.Config.ETS5;

namespace SRF.Knx.Config.OpenHab.BaseConfig.Modifiers;

public class CreateGA(EtsGroupAddressConfig gac,
                      GroupAddressExtraConfig egac,
                      IOpenHabKnxConfigFactory configFactory,
                      DomainConfiguration domainConfig) : IOpenHabKnxBaseConfigModifier
{
    private readonly EtsGroupAddressConfig gac = gac;
    private readonly GroupAddressExtraConfig egac = egac;
    private readonly IOpenHabKnxConfigFactory baseConfigFactory = configFactory;
    private readonly DomainConfiguration domainConfig = domainConfig;

    public void Modify(KnxOpenHabConfig ohMeta)
    {
        var newbie = baseConfigFactory.CreateOpenHabGAC(gac.Address, domainConfig);

        // 1. to which thing should it belong to?
        // 2. does the target thing exist? If not, create the target thing.
        // 3. does the GA already exist? If so, remove it first.
        // 4. add the GA to the target thing ohThing.

        // determine target thing
        var domainThing = domainConfig.Extra.Things.Find(t => t.GroupAddresses.Any(ga => ga.Key == gac.Address.Address))
            ?? throw new Exception($"Could not find domain thing for GA {gac.Address.AddressAsString} '{gac.Label}'. There must be a misconfiguration.");
        var thingName = domainThing.Name;
        var ohThing = ohMeta.Things.Find(t => t.Name == thingName);
        if (ohThing == null)
        {
            ohThing = new KnxThingConfig()
            {
                Name = thingName,
            };
            ohMeta.Things.Add(ohThing);
        }
        
        // 2. is the GA already part of another thing?
        var existingThing = ohMeta.Things.Find(t => t.GroupAddresses.Exists(ga => ga.Address.Address == newbie.Address.Address));
        if (existingThing != null)
        {
            var item = existingThing.GroupAddresses.Find(ga => ga.Address.Address == newbie.Address.Address);
            if (item != null)
                existingThing.GroupAddresses.Remove(item);
        }

        // 3. add the GA to the target thing
        ohThing.GroupAddresses.Add(newbie);
    }

    public void Modify(JsonNode ohMeta)
    {
        throw new NotImplementedException();
    }
}
