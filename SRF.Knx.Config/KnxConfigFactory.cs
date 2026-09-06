using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRF.Knx.Config.Domain;
using SRF.Knx.Config.ETS5;
using SRF.Knx.Core;
using SRF.Knx.Core.DPT;
using UnitsNet;

namespace SRF.Knx.Config;

/// <summary>
/// Creates, transforms and manages KNX related configuration objects.<br/>
/// - Loads ETS KNX Group Address export files loaded into <see cref="SRF.Knx.Config.ETS5.EtsGroupAddressConfig"/><br/>
/// - Loads existing <see cref="DomainConfiguration"/> or derives new items from the ETS export<br/>
/// <see cref="KnxSystemConfigOptions"/> is expected to be loaded via <see cref="Microsoft.Extensions.Options"/>.<br/>
/// Get methods use in-memory cached configurations objects once loaded upon the first invocation.<br/>
/// <br/>If only the <see cref="DomainConfiguration"/> is needed, get a singleton directly via dependency injection instead of using the class at hand.
/// </summary>
public class KnxConfigFactory(
    IOptionsMonitor<KnxSystemConfigOptions> options,
    DomainConfiguration domainConfiguration,
    TimeProvider timeProvider,
    ILogger<KnxConfigFactory> logger,
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    ILabelToNameConverter labelToNameConverter,
    IDptFactory dptFactory,
    IKnxMasterDataProvider knxMasterDataProvider
) : IKnxConfigFactory
{
    private readonly IOptionsMonitor<KnxSystemConfigOptions> options = options;
    private readonly DomainConfiguration domainConfiguration = domainConfiguration;
    private readonly TimeProvider timeProvider = timeProvider;
    private KnxSystemConfigOptions Config { get => options.CurrentValue; }
    private readonly ILogger<KnxConfigFactory> logger = logger;
    private readonly ILoggerFactory loggerFactory = loggerFactory;
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly ILabelToNameConverter labelToNameConverter = labelToNameConverter;
    private readonly IDptFactory dptFactory = dptFactory;
    private readonly IKnxMasterDataProvider knxMasterDataProvider = knxMasterDataProvider;

    /// <summary>
    /// Consider injecting <see cref="DomainConfiguration"/> directly.
    /// </summary>
    public DomainConfiguration GetDomainConfig() => domainConfiguration;

    /// <summary>
    /// Consider getting it from <see cref="DomainConfiguration"/> directly.
    /// </summary>
    public Dictionary<ushort, EtsGroupAddressConfig> GetEtsGroupAddressConfigs() => domainConfiguration.GroupAddresses;

    private static JsonSerializerOptions? _cachedDefaultJsonOptions;
    public static JsonSerializerOptions DefaultJsonOptions
    {
        get
        {
            if (_cachedDefaultJsonOptions == null)
            {
                _cachedDefaultJsonOptions = new()
                {
                    WriteIndented = true,
                    AllowTrailingCommas = true,
                    AllowOutOfOrderMetadataProperties = true,
                };
                _cachedDefaultJsonOptions.Converters.Add(new JsonStringEnumConverter());
            }
            return _cachedDefaultJsonOptions;
        }
        set => _cachedDefaultJsonOptions = value;
    }

    public void SaveDomainConfig(DomainConfiguration domainConfig)
    {
        using var fs = new FileStream(options.CurrentValue.KnxDomainConfigFile, FileMode.Create);
        JsonSerializer.Serialize<Domain.DomainExtraConfig>(fs, domainConfig.Extra, DefaultJsonOptions);
        fs.Close();
    }

    /// <summary>
    /// Builds the <c>HomeCompanionKnxAutoGen.json</c> mapping from the loaded <see cref="DomainConfiguration"/>.
    /// For each KNX group address the property name is taken from <see cref="GroupAddressExtraConfig.Name"/>
    /// when available, with a fallback to the <see cref="ILabelToNameConverter"/>.
    /// </summary>
    public Dictionary<string, HomeCompanionAutoGenEntry> GenerateHomeCompanionAutoGen(DomainConfiguration config)
    {
        var result = new Dictionary<string, HomeCompanionAutoGenEntry>();
        foreach (var kvp in config.GroupAddresses)
        {
            var extra = config.Extra.TryGetGAExtraConfig(kvp.Value.Address, out var extraConfig) ? extraConfig : null;
            var address3L = kvp.Key.To3LGroupAddress();
            var gac = kvp.Value;
            var name = extra != null && !string.IsNullOrEmpty(extra.Name)
                ? extra.Name
                : labelToNameConverter.GetName(gac);
            var comms = KnxObjectBusCommunication.Write | KnxObjectBusCommunication.Transmit | KnxObjectBusCommunication.Update;
            if (extra?.HomeCompanion?.AnswerReadRequests ?? false)
                comms |= KnxObjectBusCommunication.Read;
            if (extra?.HomeCompanion?.InitializeFromKnxBus ?? false)
                comms |= KnxObjectBusCommunication.Initialize;

            var hacge = new HomeCompanionAutoGenEntry
            {
                PropertyName = name,
                Label = string.IsNullOrWhiteSpace(gac.Label) ? null : gac.Label,
                Description = string.IsNullOrWhiteSpace(gac.Description) ? null : gac.Description,
                Dpt = string.IsNullOrEmpty(gac.DPTs) ? null : gac.DPTs,
                Communication = comms,
                WantsOpenHabInitialization = extra?.HomeCompanion?.InitializeFromOpenHab ?? false,
            };
            result[address3L] = hacge;

            // does it need to be unit aware? Consult KNX DPT master data for the DPT and check if it has a unit. If so, add the unit to the description.
            if (gac.DPT is not null)
            {
                var dpt = dptFactory.Get(gac.DPT);
                if (dpt is DptSimple dptSimple && dptSimple.NumericInfo?.Unit is not null)
                {
                    var unit = dptSimple.NumericInfo.Unit;
                    var unitMapping = serviceProvider.GetRequiredService<IUnitSystemsMapper>().GetDptUnitMapping(dptSimple);
                    hacge.Dimension = unitMapping?.DimensionName ?? unit?.ToString();
                    hacge.Unit = unitMapping?.UnitName;
                }
            }
        }
        return result;
    }

    /// <inheritdoc/>
    public string GenerateHomeCompanionCode(DomainConfiguration config, Action<Dictionary<string, HomeCompanionAutoGenEntry>>? postProcessEntries = null)
    {
        var entries = GenerateHomeCompanionAutoGen(config);
        postProcessEntries?.Invoke(entries);
        return KnxValuesCodeGenerator.Generate(entries, dptFactory, loggerFactory);
    }

    public DomainConfiguration CreateDomainConfigFromEtsExport()
    {
        var df = serviceProvider.GetRequiredService<IDomainConfigurationFactory>();
        return df.Load(createFreshExtraConfig: true);
    }
}
