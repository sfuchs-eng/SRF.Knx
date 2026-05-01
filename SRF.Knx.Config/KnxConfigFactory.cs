using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRF.Knx.Config.Domain;
using SRF.Knx.Config.ETS5;
using SRF.Knx.Core;

namespace SRF.Knx.Config;

/// <summary>
/// Creates, transforms and manages KNX related configuration objects.<br/>
/// - Loads ETS KNX Group Address export files loaded into <see cref="SRF.Knx.Config.ETS5.EtsGroupAddressConfig"/><br/>
/// - Loads existing <see cref="DomainConfiguration"/> or derives new items from the ETS export<br/>
/// The base configuration <see cref="KnxConfiguration"/> is expected to be loaded via <see cref="Microsoft.Extensions.Options"/>.<br/>
/// Get methods use in-memory cached configurations objects once loaded upon the first invocation.<br/>
/// <br/>If only the <see cref="DomainConfiguration"/> is needed, get a singleton directly via dependency injection instead of using the class at hand.
/// </summary>
public class KnxConfigFactory(
    IOptionsSnapshot<KnxConfiguration> options,
    DomainConfiguration domainConfiguration,
    TimeProvider timeProvider,
    ILogger<KnxConfigFactory> logger,
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    ILabelToNameConverter labelToNameConverter
) : IKnxConfigFactory
{
    private readonly IOptionsSnapshot<KnxConfiguration> options = options;
    private readonly DomainConfiguration domainConfiguration = domainConfiguration;
    private readonly TimeProvider timeProvider = timeProvider;
    private KnxConfiguration Config { get => options.Value; }
    private readonly ILogger<KnxConfigFactory> logger = logger;
    private readonly ILoggerFactory loggerFactory = loggerFactory;
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly ILabelToNameConverter labelToNameConverter = labelToNameConverter;

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
        using var fs = new FileStream(options.Value.KnxDomainConfigFile, FileMode.Create);
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
            var address3L = kvp.Key.To3LGroupAddress();
            var gac = kvp.Value;
            var name = config.Extra.TryGetGAExtraConfig(gac.Address, out var extraConfig) && !string.IsNullOrEmpty(extraConfig?.Name)
                ? extraConfig!.Name
                : labelToNameConverter.GetName(gac);
            result[address3L] = new HomeCompanionAutoGenEntry
            {
                PropertyName = name,
                Label = string.IsNullOrWhiteSpace(gac.Label) ? null : gac.Label,
                Description = string.IsNullOrWhiteSpace(gac.Description) ? null : gac.Description,
                Dpt = string.IsNullOrEmpty(gac.DPTs) ? null : gac.DPTs,
            };
        }
        return result;
    }

    /// <inheritdoc/>
    public string GenerateHomeCompanionCode(DomainConfiguration config)
    {
        var entries = GenerateHomeCompanionAutoGen(config);
        return KnxValuesCodeGenerator.Generate(entries);
    }

    public DomainConfiguration CreateDomainConfigFromEtsExport()
    {
        var df = serviceProvider.GetRequiredService<IDomainConfigurationFactory>();
        return df.Load(createFreshExtraConfig: true);
    }
}
