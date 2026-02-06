using Microsoft.Extensions.Logging;
using SRF.Knx.Config.OpenHab.BaseConfig;

namespace SRF.Knx.Config.OpenHab.Generate.Base;

public abstract class Thing<TChannel> : IConfigGenerator, IThing where TChannel : Channel
{
    private readonly ILogger<Thing<TChannel>> logger;
    private readonly IConfigGeneratorProvider configGeneratorProvider;

    public Thing(KnxThingConfig thingConfig, IConfigGeneratorProvider configGeneratorProvider, ILoggerFactory loggerFactory)
    {
        Config = thingConfig;
        this.configGeneratorProvider = configGeneratorProvider;
        this.logger = loggerFactory.CreateLogger<Thing<TChannel>>();
        foreach (var ga in thingConfig.GroupAddresses)
        {
            var chan = configGeneratorProvider.GetChannelGenerator(ga, thingConfig.GroupAddresses) as TChannel
                ?? throw new InvalidOperationException($"Generated channel is not of expected type {typeof(TChannel).FullName}");
            Channels.Add(chan);
        }
    }

    public KnxThingConfig Config { get; private set; }

    public List<Channel> Channels { get; private set; } = [];
    IEnumerable<IChannel> IThing.Channels => Channels;

    public virtual void WriteConfig(TextWriter to)
    {
        WriteThingStart(to);
        foreach ( var c in Channels )
            c.WriteConfig(to);
        WriteThingEnd(to);
    }

    protected virtual void WriteThingStart(TextWriter to) {
        // Thing <binding_id>:<type_id>:<thing_id> "Label" @ "Location" [ <parameters> ]
        // as Thing is stated within Binding block, the binding_id is not repeated.
        var locUndef = "undefined";
        var labelUndef = "Label missing";
        var location = Config.Location ?? locUndef;
        var locationTag = Config.Location == null || location.Equals(locUndef, StringComparison.OrdinalIgnoreCase) ? "" : $" @ \"{location}\"";
        to.WriteLine($"    Thing device {Config.Name} \"{Config.Label ?? labelUndef}\"{locationTag} [ ]");
        to.WriteLine(@"    {");
    }

    protected virtual void WriteThingEnd(TextWriter to) {
        to.WriteLine(@"    }");
    }
}
