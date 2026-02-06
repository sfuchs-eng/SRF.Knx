using System.Text.Json.Nodes;
using SRF.Knx.Config.ETS5;

namespace SRF.Knx.Config.Domain.ConfigModifiers;

public class GAECAddOrModify(EtsGroupAddressConfig gac, GroupAddressExtraConfig newGaec, IThingNameExtractor thingNameExtractor) : GAECModifierBase
{
    private readonly IThingNameExtractor thingNameExtractor = thingNameExtractor;

    public EtsGroupAddressConfig GAC { get; } = gac;

    public GroupAddressExtraConfig NewGAEC { get; } = newGaec;
    
    public ushort GroupAddressU { get => GAC.Address.Address; }

    public override void Modify(DomainConfiguration domainConfig)
    {
        ushort addr = GroupAddressU;
        var domainExtraConfig = domainConfig.Extra;

        // 1. if GA is presently in a thing already, keep it there, set target thing to that one
        // 2. else
            // 1. determine target thing name
            // 2. ensure target thing is set: if target thing doesn't exist, create it
        // 4. if GA is in target thing, modify it, otherwise add it

        // GA already part of a Thing? If so, we need to update that Thing's GA list
        var targetThing = domainExtraConfig.Things.FirstOrDefault(t => t.GroupAddresses.ContainsKey(addr));
        if ( targetThing == null )
        {
            // determine target thing name if not given
            var targetThingName = thingNameExtractor.GetThingName(GAC);

            // does thing already exist?
            targetThing = domainExtraConfig.Things.FirstOrDefault(t => t.Name.Equals(targetThingName, StringComparison.InvariantCulture));
            if (targetThing == null)
            {
                // no, create it
                targetThing = new Thing() { Name = targetThingName };
                domainExtraConfig.Things.Add(targetThing);
            }
        }

        // here the GA goes into
        var targetDix = targetThing.GroupAddresses;

        if (!targetDix.TryGetValue(addr, out var existingNode))
        {
            // new entry
            targetDix[addr] = NewGAEC;
            NewGAEC.EntryStatus |= ExtraConfigStatus.Fresh | ExtraConfigStatus.Automatic;
            return;
        }

        // existing entry: replace or hook to AutoLatest?

        // manual status prevents modification?
        if (existingNode.EntryStatus.HasFlag(ExtraConfigStatus.Manual))
        {
            // Do not override manually created entries, but keep AutoLatest up to date
            existingNode.AutoLatest = NewGAEC;
            NewGAEC.EntryStatus |= ExtraConfigStatus.Fresh | ExtraConfigStatus.Automatic;
            return;
        }

        // not set to manual, we can modify
        NewGAEC.AutoLatest = null;
        NewGAEC.EntryStatus ^= ExtraConfigStatus.Fresh;
        targetDix[addr] = NewGAEC;
    }

    public override void Modify(JsonNode domainConfigRoot)
    {
        throw new NotImplementedException();
    }
}
