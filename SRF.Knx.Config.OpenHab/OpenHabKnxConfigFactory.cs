using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRF.Knx.Config.Domain;
using SRF.Knx.Config.Domain.Legacy;
using SRF.Knx.Config.ETS5;
using SRF.Knx.Config.OpenHab.BaseConfig;
using SRF.Knx.Config.OpenHab.BaseConfig.Modifiers;
using SRF.Knx.Config.OpenHab.DptMapping;
using SRF.Knx.Config.OpenHab.Generate;
using SRF.Knx.Config.OpenHab.Templating;
using Knx.Falcon;
using Knx.Falcon.ApplicationData.DatapointTypes;
using SRF.Knx.Config.OpenHab.UnitSystem;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.DependencyInjection;

namespace SRF.Knx.Config.OpenHab;

/// <summary>
/// Generate OpenHAB KNX configuration files:
/// Generates things and items configuration files for OpenHAB reflecting a KNX/IP bridge and the group addresses reachable through it.
/// </summary>
public class OpenHabKnxConfigFactory : IOpenHabKnxConfigFactory
{
    private readonly KnxConfiguration knxConfig;
    private readonly ILogger<OpenHabKnxConfigFactory> logger;
    private readonly IDomainConfigurationFactory domainConfigurationFactory;
    private readonly ILoggerFactory loggerFactory;
    private readonly IServiceProvider serviceProvider;

    public IConfigGeneratorProvider CfgObjProvider { get; }

    public Encoding OpenHabConfigFilesEncoding { get; set; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly ConfigTemplatesManager itemTemplates;
    private readonly DptMappingLookupItem[] dptMappingLookup;
    private readonly UnitSystemConfig unitSystemConfig;

    public ILabelToNameConverter LabelToNameConverter { get; set; } = new DefaultLabelToNameConverter();

    private readonly DptFactory dptFactory = DptFactory.Default;

    private readonly JsonSerializerOptions jsonSerializerOptions;

    public OpenHabKnxConfigFactory(
        IOptions<KnxConfiguration> knxConfigOptions,
        IDomainConfigurationFactory domainConfigurationFactory,
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider)
    {
        this.knxConfig = knxConfigOptions.Value;
        this.logger = loggerFactory.CreateLogger<OpenHabKnxConfigFactory>();
        this.domainConfigurationFactory = domainConfigurationFactory;
        this.loggerFactory = loggerFactory;
        this.serviceProvider = serviceProvider;
        var ohVersion = knxConfig.OpenHab.OpenHabVersion;
        CfgObjProvider = ohVersion switch
        {
            "3" => new Generate.v3.ConfigGeneratorProviderVersion3(loggerFactory),
            "5" => new Generate.v5.ConfigGeneratorProviderVersion5(loggerFactory),
            _ => throw new NotImplementedException($"OpenHAB version '{ohVersion}' is not supported"),
        };

        var encoderSettings = new TextEncoderSettings();
        encoderSettings.AllowRange(UnicodeRanges.All);
        jsonSerializerOptions = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            AllowOutOfOrderMetadataProperties = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                | System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-encoding
            Encoder = JavaScriptEncoder.Create(encoderSettings),
        };

        dptMappingLookup = LoadMasterData<DptMappingLookupItem[]>(knxConfig.OpenHab.KnxDptMappings);
        unitSystemConfig = LoadMasterData<UnitSystemConfig>(knxConfig.OpenHab.UnitSystemConfig);
        var dimensionParsingFailures = unitSystemConfig.DimensionLookups
            .Where(dlut => dlut.JsonDimensionParsed != null)
            .ToArray();
        if (dimensionParsingFailures.Length > 0)
            logger.LogWarning("Failed to parse dimensions for {count} unit system dimension lookups: {lookups}", dimensionParsingFailures.Length, string.Join(", ", dimensionParsingFailures.Select(dlut => dlut.JsonDimensionParsed)));
        itemTemplates = new ConfigTemplatesManager(
            new FileInfo(Path.Combine(knxConfig.OpenHab.TemplatesFolder, knxConfig.OpenHab.ItemTemplatesFile)),
            loggerFactory.CreateLogger<ConfigTemplatesManager>()
        );
    }

    private KnxOpenHabConfig CreateOHMetaConfiguration(DomainConfiguration domainConfig)
    {
        var fresh = new KnxOpenHabConfig();
        ApplyConfigurationUpdates(IdentifyConfigurationUpdates(domainConfig, fresh), fresh);
        logger.LogDebug("Created entirely new KnxOpenHabConfig and initialized it based on provided domain config.");
        return fresh;
    }

    private KnxOpenHabConfig? _cachedKnxOpenHabConfig = null;

    /// <summary>
    /// Load if possible, otherwise create new KNX OpenHAB configuration file based on provided domain configuration.
    /// </summary>
    public KnxOpenHabConfig GetKnxOpenHabConfig(DomainConfiguration domainConfig)
    {
        if (_cachedKnxOpenHabConfig != null)
            return _cachedKnxOpenHabConfig;

        var cfgFile = knxConfig.OpenHab.BaseConfigFile;
        if (File.Exists(cfgFile))
        {
            // load
            using var fsi = File.OpenRead(cfgFile);
            try
            {
                _cachedKnxOpenHabConfig = System.Text.Json.JsonSerializer.Deserialize<KnxOpenHabConfig>(fsi, KnxConfigFactory.DefaultJsonOptions)
                    ?? throw new InvalidDataException($"The KNX OpenHAB configuration file '{cfgFile}' could not be deserialized.");
                logger.LogTrace("Loaded existing KNX OpenHAB configuration from '{OpenHabKnxMetaConfigFile}'", cfgFile);
                return _cachedKnxOpenHabConfig;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "The KNX OpenHAB configuration file '{OpenHabKnxMetaConfigFile}' could not be deserialized. Creating fresh config.", cfgFile);
            }
        }

        // create fresh and save
        _cachedKnxOpenHabConfig = this.CreateOHMetaConfiguration(domainConfig);
        using var fso = File.OpenWrite(cfgFile);
        System.Text.Json.JsonSerializer.Serialize(fso, _cachedKnxOpenHabConfig, KnxConfigFactory.DefaultJsonOptions);
        fso.Close();
        logger.LogWarning("A new KNX OpenHAB configuration file '{OpenHabKnxMetaConfigFile}' has been created. Please edit it and run the generator again.", cfgFile);

        return _cachedKnxOpenHabConfig;
    }

    public IEnumerable<IOpenHabKnxBaseConfigModifier> IdentifyConfigurationUpdates(DomainConfiguration domainConfig, KnxOpenHabConfig knxOpenHabConfig)
    {
        var allKnxGA = knxOpenHabConfig.Things.SelectMany(t => t.GroupAddresses)
            .ToDictionary(ga => ga.Address.Address, ga => ga);

        // find group addresses not existing any longer
        var deletions = allKnxGA
            .Where(oa => !domainConfig.GroupAddresses.ContainsKey(oa.Key))
            .Select(oa => new DeleteGA(oa.Value));
        foreach (var del in deletions)
            yield return del;
        var extras = GroupAddressConfiguration.FromDomainConfig(domainConfig)
            .ToDictionary(ga => ga.Ets.Address);

        // deal with new and changed ones
        foreach (var dgac in domainConfig.GroupAddresses.Values)
        {
            // Extra config available?
            if (!extras.TryGetValue(dgac.Address, out GroupAddressConfiguration? fullgac) || fullgac == null || fullgac.Extra == null)
            {
                logger.LogWarning("ETS GA {etsGA} '{etsGALabel}' misses an extra config. Please create one.", dgac.Address.AddressAsString, dgac.Label);
                continue;
            }
            var egac = fullgac.Extra;

            // new?
            if (!allKnxGA.TryGetValue(dgac.Address.Address, out OHKnxGroupAddress? ogac) || ogac == null)
            {
                yield return new CreateGA(dgac, egac, this, domainConfig);
                continue;
            }

            // modification? Get new OH from Domain/Extra, compare results.
            if (IsModified(dgac, egac, ogac, domainConfig, out IEnumerable<IOpenHabKnxBaseConfigModifier> modifiers))
                foreach (var mod in modifiers)
                    yield return mod;
        }
    }

    public void ApplyConfigurationUpdates(IEnumerable<IOpenHabKnxBaseConfigModifier> updates, KnxOpenHabConfig knxOpenHabConfig)
    {
        foreach (var modifier in updates)
            modifier.Modify(knxOpenHabConfig);
    }

    public void WriteOHConfigFiles(KnxOpenHabConfig knxOpenHabConfig)
    {
        var baseConfig = knxOpenHabConfig;

        // get config generators
        var bridgeGen = CfgObjProvider.GetBridgeGenerator(
            baseConfig.Bridge,
            baseConfig.Things);

        var targetDirectory = new DirectoryInfo(knxConfig.OpenHab.OHConfigRoot);
        var thingsFile = Path.Combine(targetDirectory.FullName, $"things/{baseConfig.Name}.things");
        var thingsBackupFile = $"{thingsFile}.removed";
        var itemsFile = Path.Combine(targetDirectory.FullName, $"items/{baseConfig.Name}.items");

        // things file
        // OpenHAB v3 would not update the KNX Things. We need to remove the Things file, wait ... (20sec?), then create the new one
        // in order to have KNX Things deleted and recreated.
        // OpenHAB 5?
        if (File.Exists(thingsFile))
        {
            new FileInfo(thingsFile).MoveTo(thingsBackupFile, true);
            logger.LogInformation("Renamed '{thingsFile}'. Waiting {delay} s until OpenHAB removed related things.", thingsFile,
                knxConfig.OpenHab.WaitTimeBeforeWritingThingsFileSec);
            Thread.Sleep(knxConfig.OpenHab.WaitTimeBeforeWritingThingsFileSec * 1000);
        }
        using (var things = new StreamWriter(new FileStream(thingsFile, FileMode.Create), OpenHabConfigFilesEncoding))
        {
            bridgeGen.WriteConfig(things);
            things.Close();
            logger.LogInformation("Generated '{thingsFile}'", thingsFile);
        }

        // items file
        var itemsFactory = new ItemsFactory(bridgeGen, CfgObjProvider);
        using var items = new StreamWriter(itemsFile, new FileStreamOptions() { Mode = FileMode.Create, Access = FileAccess.Write });
        foreach (var item in itemsFactory.Items)
            item.WriteConfig(items);
        items.Close();
        logger.LogInformation("Generated '{itemsFile}'", itemsFile);
    }

    public void SaveBaseConfig(KnxOpenHabConfig openHabConfig)
    {
        using var fs = new FileStream(knxConfig.OpenHab.BaseConfigFile, FileMode.Create);
        JsonSerializer.Serialize<KnxOpenHabConfig>(fs, openHabConfig, KnxConfigFactory.DefaultJsonOptions);
        fs.Close();
        logger.LogInformation("Wrote OpenHAB KNX base configuration to '{metaConfigFile}'", knxConfig.OpenHab.BaseConfigFile);
    }

    public IEnumerable<IOpenHabKnxBaseConfigModifier> OverrideWithLegacy(Domain.DomainConfiguration domainConfig, KnxOpenHabConfig cfg, List<KnxGroupAddressConfig> legacyGAC)
    {
        // Legacy GACs that are not in current config -> log warnings; cannot create them if they're not in the ETS export file.
        var surplusLegacyGAC = legacyGAC
            .Where(lgac => !cfg.Things.SelectMany(t => t.GroupAddresses).Any(ogac => ogac.Address.AddressAsString == lgac.Address.AddressAsString));
        foreach (var lgac in surplusLegacyGAC)
        {
            logger.LogWarning("Legacy GA {legacyGA} '{legacyGALabel}' not in ETS export. Skipping.", lgac.Address.AddressAsString, lgac.Label);
        }
        
        // Add or modify all legacy GACs found in current config
        var allCurrentGacDictionary = cfg.Things.SelectMany(t => t.GroupAddresses)
            .ToDictionary(ga => ga.Address.Address, ga => ga);
        var legacyGACsThatExistInConfig = legacyGAC
            .Select(lgac => new { LegacyGAC = lgac, CurrentGAC = allCurrentGacDictionary.GetValueOrDefault(lgac.Address.Address) })
            .Where(gacPair => gacPair.CurrentGAC != null);
        var stateOwnedIfAnyOf = new string[] { "oh", "bhw17" };
        return legacyGACsThatExistInConfig
            .Select(gac => {
                var ohgac = CreateOpenHabGAC(gac.LegacyGAC.Address, domainConfig);
                // override with legacy values
                ohgac.Address3L = gac.LegacyGAC.Address.AddressAsString;
                ohgac.CreateItem = !gac.LegacyGAC.NoItem;
                ohgac.Name = gac.LegacyGAC.Name;
                ohgac.Label = gac.LegacyGAC.Label;
                ohgac.DPTs = gac.LegacyGAC.DataType;
                ohgac.AdditionalGA = gac.LegacyGAC.AdditionalGA;
                ohgac.EntryStatus = ExtraConfigStatus.Fresh | (gac.LegacyGAC.IsNew ? ExtraConfigStatus.Automatic : ExtraConfigStatus.Manual);
                ohgac.Groups = gac.LegacyGAC.Groups;
                ohgac.Icon = gac.LegacyGAC.Icon;
                ohgac.IsWritable = true;
                ohgac.ItemType = gac.LegacyGAC.ItemType;
                ohgac.Mappings = gac.LegacyGAC.Mappings;
                ohgac.MapType = gac.LegacyGAC.MapType;
                ohgac.Channel.IsStateOwned = stateOwnedIfAnyOf.Any(s => s.Equals(gac.LegacyGAC.Owner)) && ohgac.Channel.IsReadable;

                return new ChangeGA(gac.CurrentGAC!, ohgac);
            }
        );
    }

    private CType LoadMasterData<CType>(string relativeFile)
    {
        string[] candidatePaths = [
            Path.Combine(knxConfig.OpenHab.TemplatesFolder, relativeFile),
            relativeFile,
            Path.Combine(AppContext.BaseDirectory, relativeFile),
            Path.Combine(Environment.CurrentDirectory, relativeFile)
        ];

        var fn = candidatePaths.FirstOrDefault(fp => File.Exists(fp));
        if ( fn == null)
        {
            logger.LogError("Couldn't find required master data file '{filename}' in any of the candidate paths: {paths}", relativeFile, string.Join(", ", candidatePaths));
            throw new FileNotFoundException("Failed to load required master data.", candidatePaths[0]);
        }

        CType? res;
        try
        {
            using var f = new FileStream(fn, FileMode.Open);
            res = JsonSerializer.Deserialize<CType>(f, jsonSerializerOptions);
            f.Close();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while loading master data file '{filename}'", fn);
            throw;
        }
        if (res == null)
        {
            logger.LogError("Failed to deserialize type {typeName} from file '{filename}'", typeof(CType).FullName, fn);
            throw new InvalidDataException();
        }
        else
            logger.LogTrace("Loaded {typeName} from file '{filename}'", typeof(CType).FullName, fn);
        return res;
    }

    private bool IsModified(
        EtsGroupAddressConfig dgacRef,
        GroupAddressExtraConfig egacRef,
        OHKnxGroupAddress ogacCurrent,
        DomainConfiguration domainConfig,
        out IEnumerable<IOpenHabKnxBaseConfigModifier> modifiers)
    {
        var freshMeta = CreateOpenHabGAC(dgacRef.Address, domainConfig);
        if (!freshMeta.Equals(ogacCurrent))
        {
            modifiers = [new ChangeGA(ogacCurrent, freshMeta)];
            return true;
        }

        modifiers = [];
        return false;
    }

    /// <summary>
    /// Creates a new <see cref="OHKnxGroupAddress"/> from <see cref="DomainConfiguration"/>
    /// which reflects the ETS Group Address export combined with <see cref="DomainExtraConfig"/>.
    /// Intended new approach:
    /// - identify/create Thing via GA Name (domain config, not OH specific)
    /// - determine Thing type 
    /// - associate GA to channel parameters
    /// - derive Items for channels
    /// Legacy approach was to generate an OpenHAB Item for each GroupAddress - originates from OpenHAB 2 usage.
    /// </summary>
    public OHKnxGroupAddress CreateOpenHabGAC(KnxGroupAddress groupAddress, DomainConfiguration domainConfig)
    {
        // should this GA belong to a particular Thing? Which? --> go along Domain config.
        var thing = domainConfigurationFactory.AssociateThing(groupAddress, domainConfig, out _, out _);

        // determine item type
        var ets = domainConfig.GroupAddresses[groupAddress.Address];
        if (!domainConfig.Extra.TryGetGAExtraConfig(groupAddress, out GroupAddressExtraConfig? extra) || extra == null)
        {
            logger.LogWarning("Missing extra config for GA {etsGA} '{etsGALabel}' when creating OHKnxGroupAddress", ets.Address.AddressAsString, ets.Label);
        }

        var ohgac = new OHKnxGroupAddress()
        {
            Address = ets.Address,
            DPTs = ets.HasValidDPT ? (ets.DPT.DotFormat ?? "") : "",
            Item = new()
            {
                Label = ets.Label,
            },
        };

        // get infos encoded in ETS GA label
        LabelToNameConverter.GetName(ets, out string etsThing, out string? etsChannel, out string? etsChannelParameter, out string? valueFormat);
        if (!string.IsNullOrWhiteSpace(etsChannel))
            ohgac.Channel.Name = etsChannel;

        // Dpt mapping overrides
        // TODO: make them smarter by looking at entire things. E.g. Dimmer has an on/off 1.001 for a dimmer channel type on a different parameter than a simple light switch without dimmer function.
        var dptMappingCandidates = dptMappingLookup
            .Where(dptm => dptm.DPTs.Select(d => new DPT(d)).Any(d => ohgac.DPT?.Equals(d) ?? false))
            .ToArray();
        if (dptMappingCandidates.Length == 0)
        {
            if (ohgac.DPT == null)
                logger.LogWarning("GA {etsGA} '{etsGALabel}' has no valid DPT assigned in ETS. Cannot apply DPT mapped overrides.", ets.Address.AddressAsString, ets.Label);
            else
                logger.LogDebug("No DPT mapping overrides found for GA {etsGA} '{etsGALabel}' with DPT '{dpt}'", ets.Address.AddressAsString, ets.Label, ohgac.DPTs);
        }
        else
        {
            // if we have a paramerter name from the ETS extra config, try to find a matching channel template in case of multiple matches
            if (dptMappingCandidates.Length > 1 && !string.IsNullOrWhiteSpace(etsChannelParameter))
            {
                var filteredCandidates = dptMappingCandidates
                    .Where(dptm => dptm.Channels.Any(ch => ch.Parameter.Equals(etsChannelParameter, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();
                if (filteredCandidates.Length >= 1)
                {
                    dptMappingCandidates = filteredCandidates;
                }
            }

            if (dptMappingCandidates.Length > 1)
            {
                logger.LogWarning("Multiple ({count}) DPT mapping overrides found for GA {etsGA} '{etsGALabel}' with DPT '{dpt}'. Using first match with channel type '{channelType}'", dptMappingCandidates.Length, ets.Address.AddressAsString, ets.Label, ohgac.DPTs, dptMappingCandidates.FirstOrDefault()?.Channels.FirstOrDefault()?.ChannelType);
            }

            var dptMapping = dptMappingCandidates[0];

            // which channel template to use?
            var channelTemplate = dptMapping.Channels
                .FirstOrDefault(ch =>
                    !string.IsNullOrWhiteSpace(etsChannelParameter)
                    && ch.Parameter.Equals(etsChannelParameter, StringComparison.OrdinalIgnoreCase)
                ) ?? dptMapping.Channels[0];

            // apply overrides
            ohgac.ItemType = channelTemplate.ItemType.ToString();
            ohgac.Name = etsThing ?? etsChannel ?? "";
            if (string.IsNullOrWhiteSpace(ohgac.Name))
                logger.LogError("Failed to derive name for {ga}", ohgac.Address3L);
            ohgac.Channel.Name = etsChannel ?? $"ch{ohgac.Name}";
            ohgac.Channel.Type = channelTemplate.ChannelType;
            ohgac.Channel.Parameter = channelTemplate.Parameter;
            if (channelTemplate.Dimension != null)
                ohgac.Channel.Dimension = channelTemplate.Dimension;

            // fix Stereotype dependent aspects
            switch (channelTemplate.Stereotype)
            {
                case DptMappingLookupItem.ChannelStereotype.Actuator:
                    ohgac.Channel.IsWritable = true;
                    ohgac.Channel.IsReadable = true;
                    ohgac.Channel.IsStateOwned = false;
                    break;
                case DptMappingLookupItem.ChannelStereotype.Sensor:
                    ohgac.Channel.IsWritable = false;
                    ohgac.Channel.IsReadable = true;
                    ohgac.Channel.IsStateOwned = false;
                    break;
                case DptMappingLookupItem.ChannelStereotype.Event:
                    ohgac.Channel.IsWritable = true;
                    ohgac.Channel.IsReadable = false;
                    ohgac.Channel.IsStateOwned = false;
                    break;
                case DptMappingLookupItem.ChannelStereotype.Parameter:
                    ohgac.Channel.IsWritable = true;
                    ohgac.Channel.IsReadable = true;
                    ohgac.Channel.IsStateOwned = true;
                    break;
                case DptMappingLookupItem.ChannelStereotype.Any:
                    ohgac.Channel.IsWritable = true;
                    ohgac.Channel.IsReadable = false;
                    ohgac.Channel.IsStateOwned = false;
                    break;
                default:
                    logger.LogWarning("Unhandled channel stereotype {stereotype} for GA {etsGA} '{etsGALabel}'", channelTemplate.Stereotype, ets.Address.AddressAsString, ets.Label);
                    break;
            }

            // take unit and dimension from DPT if not set via mapping
            if (channelTemplate.Dimension == OpenHabDimension.AccordingDpt && ets.HasValidDPT && !ets.DPT.IsMainOnly)
            {
                // use the Falcon SDK to get dimension from DPT
                var dpt = dptFactory.Get(ets.DPT.Main, ets.DPT.Sub);
                if (dpt != null && dpt is DptSimple simpleDpt)
                {
                    ohgac.Channel.KnxUnit = simpleDpt.NumericInfo.Unit;
                    ohgac.Channel.Dimension = unitSystemConfig.DimensionLookups.FirstOrDefault(dlut => dlut.Units.Any(u => u.Equals(simpleDpt.NumericInfo.Unit)))?.Dimension;
                    if (ohgac.Channel.Dimension == null)
                    {
                        logger.LogWarning("Could not determine dimension for KNX unit '{knxUnit}' of DPT {dpt} of GA {etsGA} '{etsGALabel}'", simpleDpt.NumericInfo.Unit, ets.DPT.EtsFormat, ets.Address.AddressAsString, ets.Label);
                    }
                }
                else
                {
                    logger.LogWarning("Could not determine dimension for DPT {dpt} of GA {etsGA} '{etsGALabel}'", ets.DPT.EtsFormat, ets.Address.AddressAsString, ets.Label);
                }
            }
        }

        // Icon & ItemType set via Template-Matching; other overrides possible (e.g. DataType):
        itemTemplates.SetDefaultConfig(ets, extra, ohgac);

        return ohgac;
    }
    
    /// <summary>
    /// Creates new domain and OpenHAB KNX configurations from legacy group address configuration file.
    /// </summary>
    /// <param name="legacyGroupAddressConfigFile"></param>
    /// <param name="domainConfiguration"></param>
    /// <param name="openHabConfig"></param>
    public void OverrideConfigsFromLegacy(
        string legacyGroupAddressConfigFile,
        out DomainConfiguration domainConfiguration,
        out KnxOpenHabConfig openHabConfig)
    {
        var legacyGAC = new Domain.Legacy.LegacyGroupAddressConfigFactory(
                    loggerFactory.CreateLogger<Domain.Legacy.LegacyGroupAddressConfigFactory>()
                )
                .Load(legacyGroupAddressConfigFile);

        // update domain configuration
        var df = serviceProvider.GetRequiredService<IDomainConfigurationFactory>();
        domainConfiguration = df.Load();
        var dcUpdates = df.UpdateWithLegacyGAC(domainConfiguration, legacyGAC);
        df.ApplyConfigurationUpdates(dcUpdates, domainConfiguration);
        df.Save(domainConfiguration);

        // update OpenHAB configuration
        var kof = this; //serviceProvider.GetRequiredService<IOpenHabKnxConfigFactory>();
        openHabConfig = kof.GetKnxOpenHabConfig(domainConfiguration);
        var updates = kof.IdentifyConfigurationUpdates(domainConfiguration, openHabConfig);
        kof.ApplyConfigurationUpdates(updates, openHabConfig);
        var legacyUpdates = kof.OverrideWithLegacy(domainConfiguration, openHabConfig, legacyGAC);
        kof.ApplyConfigurationUpdates(legacyUpdates, openHabConfig);
        kof.SaveBaseConfig(openHabConfig);
    }
}
