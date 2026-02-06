using Microsoft.Extensions.Logging;
using SRF.Knx.Config.OpenHab.BaseConfig;

namespace SRF.Knx.Config.OpenHab.Generate.Base;

/// <summary>
/// KNX:IP Bridge in OpenHAB
/// </summary>
public class Bridge<TThing,TChannel> : IConfigGenerator, IBridge
    where TThing : Thing<TChannel>
    where TChannel : Channel
{
    public BridgeConfig Config { get; private set; }

    private readonly IConfigGeneratorProvider configGeneratorProvider;
    private readonly ILogger<Bridge<TThing,TChannel>> logger;
    private string BindingID { get => Config.BindingId; }
    private string BindingTypeID { get => Config.BindingTypeId; }

    public Bridge(
        BridgeConfig bridgeConfig,
        IEnumerable<KnxThingConfig> thingsConfig,
        IConfigGeneratorProvider configGeneratorProvider,
        ILoggerFactory loggerFactory
    )
    {
        Config = bridgeConfig;
        this.configGeneratorProvider = configGeneratorProvider;
        this.logger = loggerFactory.CreateLogger<Bridge<TThing,TChannel>>();

        // create things with their channels
        var thingLogger = loggerFactory.CreateLogger<TThing>();
        foreach (var thing in thingsConfig)
        {
            var nt = configGeneratorProvider.GetThingGenerator(thing) as TThing
                ?? throw new InvalidOperationException($"Generated thing is not of expected type {typeof(TThing).FullName}");
            Things.Add(nt);
        }
    }

    public List<TThing> Things { get; private set; } = [];

    IEnumerable<IThing> IBridge.Things => Things;

    public virtual void WriteConfig(TextWriter to)
    {
        WriteBridgeStart(to);
        foreach ( var t in Things)
            t.WriteConfig(to);
        WriteBridgeEnd(to);
    }

    protected virtual void WriteBridgeStart(TextWriter to) {
        // A bridge is a special thing:
        // Thing <binding_id>:<type_id>:<thing_id> "Label" @ "Location" [ <parameters> ]
        to.WriteLine($"Bridge {BindingID}:{BindingTypeID}:{Config.Name} [");
        to.WriteLine($"    type=\"{Config.Type}\",");
        to.WriteLine($"    localSourceAddr=\"{Config.KnxDeviceAddress}\"");
        to.WriteLine(@" ] {");
    }

    protected virtual void WriteBridgeEnd(TextWriter to) {
        to.WriteLine(@"}");
    }
}
