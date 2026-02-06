using SRF.Knx.Config.OpenHab.Templating;

namespace SRF.Knx.Config.OpenHab.Generate;

public class ItemsFactory(IBridge bridge, IConfigGeneratorProvider cfgObjProvider)
{
    private readonly IBridge bridge = bridge;

    public IEnumerable<IItem> Items
    {
        get
        {
            // also indep GAs got associated to a Thing being part of the bridge
            // --> iterate through all Things in the Bridge
            foreach ( var thing in bridge.Things )
                foreach ( var channel in thing.Channels )
                    yield return CfgObjProvider?.GetItemGenerator(bridge, thing, channel) ?? throw new NotImplementedException();
        }
    }

    public IConfigGeneratorProvider CfgObjProvider { get; } = cfgObjProvider;
}
