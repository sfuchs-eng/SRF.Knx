using System.Text.Json;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRF.Knx.Config.Domain.ConfigModifiers;
using SRF.Knx.Config.ETS5;
using SRF.Knx.Config.Exceptions;
using SRF.Knx.Core;

namespace SRF.Knx.Config.Domain;

public class DomainConfigurationFactory(
    IOptionsMonitor<KnxConfiguration> knxOptions,
    TimeProvider timeProvider,
    ILogger<DomainConfigurationFactory> logger
) : IDomainConfigurationFactory
{
    public ILabelToNameConverter LabelToNameConverter { get; set; } = new DefaultLabelToNameConverter();
    public IThingNameExtractor ThingNameExtractor { get; set; } = new DefaultThingNameExtractor();

    public JsonSerializerOptions JsonOptionsExtraConfig { get; set; } = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private DateTimeOffset cacheTimeStamp;
    private DomainConfiguration? _cache;
    private DomainConfiguration? Cached {
        get => _cache;
        set
        {
            cacheTimeStamp = timeProvider.GetLocalNow();
            _cache = value;
        }
    }
    private readonly TimeProvider timeProvider = timeProvider;

    /// <summary>
    /// Returns a cached <see cref="DomainConfiguration"/> if possible,
    /// otherwise invokes <see cref="Load"/> and caches the result prior returning it.
    /// </summary>
    public DomainConfiguration Get()
    {
        return IsCacheUpToDate() ? Cached ?? Load() : Load();
    }

    private bool IsCacheUpToDate()
    {
        if ( Cached == null )
            return false;
        string[] relevantFiles = [
            knxOptions.CurrentValue.EtsGAExportFile,
            knxOptions.CurrentValue.KnxDomainConfigFile
        ];
        if (relevantFiles.Any(f => !File.Exists(f)))
        {
            logger.LogWarning("One or more configuration files do not exist, cannot reload config. Keeping cached configuration if it exists.");
            return Cached != null;
        }
        if ( relevantFiles.Any(f => File.GetLastWriteTimeUtc(f) > cacheTimeStamp.UtcDateTime) )
            return false;
        return true;
    }

    public DomainConfiguration Load(bool createFreshExtraConfig = false)
    {
        // import ETS group address file & generate missing / auto extra configs
        try
        {
            if (!File.Exists(knxOptions.CurrentValue.EtsGAExportFile))
            {
                logger.LogError("ETS Group Address export file '{etsGAExportFile}' does not exist. Cannot load domain configuration.", knxOptions.CurrentValue.EtsGAExportFile);
                return new();
            }

            var res = new DomainConfiguration()
            {
                GroupAddresses = LoadGroupAddressConfigurations(),
                Extra = createFreshExtraConfig ? new() : LoadDomainExtraConfig()
            };

            // auto-create / update extra configs
            var modifiers = IdentifyRequiredConfigurationUpdates(res.GroupAddresses, res.Extra);

            // apply modifications to extra config
            ApplyConfigurationUpdates(modifiers, res);

            Cached = res;
            return res;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "KNX domain configuration loading failed. Using blank config.");
            return new();
        }
    }

    protected DomainExtraConfig LoadDomainExtraConfig()
    {
        try
        {
            using var jsonFile = new FileStream(knxOptions.CurrentValue.KnxDomainConfigFile, FileMode.Open, FileAccess.Read);
            var res = JsonSerializer.Deserialize<DomainExtraConfig>(jsonFile);
            if (res == null)
            {
                res = new DomainExtraConfig();
                logger.LogWarning("Domain extra configuration file '{dcFile}' was empty, using blank config.",
                    knxOptions.CurrentValue.KnxDomainConfigFile);
            }
            else
                logger.LogInformation("Loaded domain extra configuration from '{dcFile}'",
                    knxOptions.CurrentValue.KnxDomainConfigFile);
            return res;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Loading domain extra configuration from '{dcFile}' failed. Using blank config.",
                knxOptions.CurrentValue.KnxDomainConfigFile);
            return new DomainExtraConfig();
        }
    }

    /// <summary>
    /// Loads ETS Group Address configurations from ETS group address export XML file.
    /// </summary>
    protected Dictionary<ushort, EtsGroupAddressConfig> LoadGroupAddressConfigurations()
    {
        var xdoc = XDocument.Load(knxOptions.CurrentValue.EtsGAExportFile);
        var gaElems = xdoc.Descendants().Where(e => e.Name.LocalName.Equals("GroupAddress"));
        logger.LogTrace("Parsing {no} Group Address elements from '{EtsGAExportFile}'...", gaElems.Count(), knxOptions.CurrentValue.EtsGAExportFile);
        var ser = new XmlSerializer(typeof(EtsGroupAddressConfig));
        Dictionary<ushort, EtsGroupAddressConfig> gacs = [];
        foreach (var rdr in gaElems.Select(e => e.CreateReader()))
        {
            try
            {
                var gac = ser.Deserialize(rdr) as EtsGroupAddressConfig ?? throw new KnxConfigurationException("Failed to deserialize GroupAddress element.");
                gacs.Add(gac.Address.Address, gac);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to deserialize GroupAddress node: '{nodeName}', does the namespace match?",
                    rdr.Name);
                break;
            }
        }
        return gacs;
    }

    public void ApplyConfigurationUpdates(
        IEnumerable<IDomainConfigModifier> modifiers,
        DomainConfiguration config)
    {
        // modify in memory extra config
        foreach (var modifier in modifiers)
        {
            modifier.Modify(config);
        }
    }

    private List<IDomainConfigModifier> IdentifyRequiredConfigurationUpdates(
        Dictionary<ushort, EtsGroupAddressConfig> groupAddresses,
        DomainExtraConfig extraConfig)
    {
        logger.LogDebug("{methodName} does not do delta upates yet - creating fresh extra configs.", nameof(IdentifyRequiredConfigurationUpdates));
        List<IDomainConfigModifier> modifiers = [.. groupAddresses
            .Select(gac => new {
                gac = gac.Value,
                gaec = CreateNewExtraConfigFromGAC(gac.Value)
                        ?? throw new InvalidOperationException("Failed to create new GAEC from GAC of address " + gac.Key.To3LGroupAddress())
                })
            .Select(ga => 
                new GAECAddOrModify(
                    ga.gac,
                    ga.gaec,
                    ThingNameExtractor
                    ) as IDomainConfigModifier
                )];
        return modifiers;
    }

    public Thing AssociateThing(GroupAddress groupAddress, DomainConfiguration domainConfig, out bool isNewThing, out bool gotNewlyAssociated)
    {
        var gac = domainConfig.GroupAddresses[groupAddress.Address];
        var gaec = domainConfig.Extra.GetGAExtraConfig(groupAddress).SingleOrDefault()
            ?? throw new ArgumentOutOfRangeException($"There are multiple extra configs for group address {groupAddress.AddressAsString}. Reduce to a single one.");
        return AssociateThing(gac, gaec, domainConfig.Extra, out isNewThing, out gotNewlyAssociated);
    }

    public Thing AssociateThing(EtsGroupAddressConfig gac, GroupAddressExtraConfig gaec, DomainExtraConfig extraConfig, out bool isNewThing, out bool gotNewlyAssociated)
    {
        isNewThing = false;
        gotNewlyAssociated = false;

        // is there a thing that contains the GA?
        if (extraConfig.Things.SingleOrDefault(t => t.GroupAddresses.ContainsKey(gac.Address.Address)) is Thing alreadyAssociated)
            return alreadyAssociated;

        gotNewlyAssociated = true;

        // try to find suitable thing
        var thingName = ThingNameExtractor.GetThingName(gac);

        if (extraConfig.Things.SingleOrDefault(t => thingName.Equals(t)) is not Thing thing)
        {
            // Thing doesn't exist, create new
            isNewThing = true;
            thing = new()
            {
                Name = thingName
            };
        }
        thing.GroupAddresses.Add(gac.Address.Address, gaec);
        return thing;
    }

    private GroupAddressExtraConfig CreateNewExtraConfigFromGAC(EtsGroupAddressConfig gac)
    {
        var gaec = new GroupAddressExtraConfig()
        {
            Name = LabelToNameConverter.GetName(gac),
        };
        return gaec;
    }

    public void Save(DomainConfiguration domainConfiguration)
    {
        var extraConfig = domainConfiguration.Extra;
        // modify persisted extra config json file
        try
        {
            using var jsonFile = new FileStream(knxOptions.CurrentValue.KnxDomainConfigFile, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(jsonFile, extraConfig, JsonOptionsExtraConfig);
            logger.LogInformation("Modified domain extra configuration in '{dcFile}'",
                knxOptions.CurrentValue.KnxDomainConfigFile);
            /*
            using var jsonFile = new FileStream(knxOptions.CurrentValue.KnxDomainConfigFile, FileMode.Open, FileAccess.ReadWrite);
            using var jsonDoc = JsonDocument.Parse(jsonFile);
            foreach (var modifier in modifiers)
            {
                modifier.Modify(jsonDoc); ... is not implemented yet...
            }
            */
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Modifying domain extra configuration in '{dcFile}' failed.",
                knxOptions.CurrentValue.KnxDomainConfigFile);
        }
    }

    public void UpdateConfigFiles(IEnumerable<IDomainConfigModifier> domainConfigModifiers, DomainConfiguration? allowOverwriteWith = null)
    {
        if (allowOverwriteWith != null)
        {
            logger.LogWarning("Delta update not implemented yet. Overwriting config files via Save()");
            Save(allowOverwriteWith);
        }
        throw new NotImplementedException("Delta updates not implemented yet and no DomainConfig provided to serialize.");
    }

    /// <summary>
    /// Load / create <see cref="DomainConfiguration"/> regularly but then update with legacy Group Address Configurations.
    /// </summary>
    public List<IDomainConfigModifier> UpdateWithLegacyGAC(DomainConfiguration domainConfig, IEnumerable<Domain.Legacy.KnxGroupAddressConfig> legacyGAC)
    {
        var updates = new List<IDomainConfigModifier>();
        var gacs = GroupAddressConfiguration.FromDomainConfig(domainConfig).ToDictionary(g => g.Ets.Address);

        foreach (var gac in legacyGAC)
        {
            if (gacs.TryGetValue(gac.Address, out var existingGAC))
            {
                var newGaec = CreateNewExtraConfigFromGAC(existingGAC.Ets);
                // create new or modify existing extra config
                updates.Add(new GAECAddOrModify(
                    existingGAC.Ets,
                    newGaec,
                    ThingNameExtractor
                    ));
            }
            else
            {
                // report missing GAC in ETS export
                logger.LogWarning("Legacy GAC contains group address {ga} {name} that is missing in ETS export - skipping.",
                    gac.Address.AddressAsString, gac.Name);
            }
        }
        return updates;
    }
}
