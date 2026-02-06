using System.Text.RegularExpressions;
using SRF.Knx.Config.OpenHab.Templating.Items;
using SRF.Knx.Config.ETS5;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SRF.Knx.Config.OpenHab.Generate;
using SRF.Knx.Config.Domain;
using SRF.Knx.Config.OpenHab.BaseConfig;

namespace SRF.Knx.Config.OpenHab.Templating;

/// <summary>
/// Derives default settings for OpenHAB entity configurations and modifies <see cref="OHKnxGroupAddress"/> accordingly.
/// Ensure to use <see cref="OpenHabBaseConfigFactory"/> instead of creating <see cref="OHKnxGroupAddress"/> directly.
/// </summary>
internal class ConfigTemplatesManager {
    private readonly ILogger<ConfigTemplatesManager> logger;

    public EtsGroupAddressMatch[] Matches { get; set; }

    public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
    };

    public ConfigTemplatesManager(FileInfo config, ILogger<ConfigTemplatesManager> logger) {
        if (!config.Exists)
            CreateItemTypeMatchDefaultFile(config);
        Matches = JsonSerializer.Deserialize<ConfigTemplates>(File.ReadAllText(config.FullName), JsonOptions)?.ItemTemplates
            ?? throw new InvalidOperationException($"Could not load ItemConfigTemplates from '{config.FullName}'");
        this.logger = logger;
    }

    private void CreateItemTypeMatchDefaultFile(FileInfo config)
    {
        var matches = new ConfigTemplates()
        {
            ItemTemplates = [
                new() {
                    Template = new ItemConfig() { Type = ItemType.Dimmer, Icon = "rollershutter" },
                    Patterns = [@"_Lamellen_.*_(Position|Winkel)$"]
                },
                new() {
                    Template = new ItemConfig() { Type = ItemType.Switch, Icon = "motion", IsWritable = false },
                    Patterns = [@"(PIR_.*_enable)|(_Surveillance_detektion_PIR_)"]
                },
                new() {
                    Template = new ItemConfig() { Type = ItemType.Dimmer, Icon = "slider" },
                    Patterns = [@"Lampe[n]?.*_value$"]
                },
                new() {
                    Template = new ItemConfig() { Type = ItemType.Switch, Icon = "light" },
                    Patterns = [@"Lampe[n]?"],
                    DPTMatches = ["DPST-1-1"]
                },
                new() {
                    Template = new ItemConfig() { Type = ItemType.Switch, Icon = "socket" },
                    Patterns = [@"[Ss]teckdose"],
                    DPTMatches = ["DPST-1-1"]
                },
                new() {
                    Template = new ItemConfig() { Type = ItemType.Number, Icon = "rollershutter" },
                    Patterns = [@"Szene_Storen", @"Szene_Lamellen"],
                    DPTMatches = ["DPST-17-1"]
                },
                new() {
                    Template = new ItemConfig() { Type = ItemType.Number, Icon = "light" },
                    Patterns = [@"Szene_Licht", @"Szene_Lampen"],
                    DPTMatches = ["DPST-17-1"]
                },
                new() {
                    Template = new ItemConfig() { Type = ItemType.Dimmer, Icon = "heating" },
                    Patterns = [@"Heizungsventil"],
                    DPTMatches = ["DPST-5-1"]
                },
                new() {
                    Template = new ItemConfig() { Type = ItemType.Contact, Icon = "window", IsWritable = false },
                    Patterns = [@"Fenster.*geschlossen"],
                    DPTMatches = ["DPST-1-9"]
                },
                new() {
                    Template = new ItemConfig() { Type = ItemType.Contact, Icon = "error" },
                    Patterns = [@"overload"]
                },
                new() {
                    Template = new ItemConfig() { Type = ItemType.Switch, Icon = "led_b", IsReadable = false, IsWritable = false },
                    DPTMatches = ["DPST-1-17"] // trigger
                },
                new() {
                    Template = new ItemConfig() { Type = ItemType.Number, Icon = "temperature", ValueFormat = @"[%.1f C]", IsWritable = false },
                    DPTMatches = ["DPST-9-1"],
                    Patterns = ["[tT]emperature"]
                },
            ]
        };
        using var outstream = new FileStream(config.FullName, FileMode.Create, FileAccess.Write);
        JsonSerializer.Serialize(outstream, matches, JsonOptions);
        outstream.Flush();
        logger.LogInformation("Created default ItemConfigMatch file at '{matchFileName}'", config.FullName);
    }

    private bool IsTemplateMatch(EtsGroupAddressConfig etsGAC, OHKnxGroupAddress gac, EtsGroupAddressMatch template)
    {
        bool mustPatternMatch = (template.Patterns?.Length ?? 0) > 0;
        bool mustDataTypeMatch = (template.DPTMatches?.Length ?? 0) > 0;
        bool patMatch(string p) => Regex.IsMatch(gac.Label ?? string.Empty, p);
        bool dptMatch(string d) => etsGAC.DPTsSpecified && d.Equals(etsGAC.DPT.EtsFormat);
        if ( mustPatternMatch && !mustDataTypeMatch)
        {
            return template.Patterns!.Any(patMatch);
        }
        if ( mustDataTypeMatch && !mustPatternMatch)
        {
            return template.DPTMatches!.Any(dptMatch);
        }
        return template.Patterns!.Any(patMatch) && template.DPTMatches!.Any(dptMatch);
    }

    private EtsGroupAddressMatch? FindMatch(EtsGroupAddressConfig etsGAC, OHKnxGroupAddress gac, GroupAddressExtraConfig? gaec) {
        var matching = Matches
            .Where(m => IsTemplateMatch(etsGAC, gac, m))
            .FirstOrDefault();
        if ( matching == null )
        {
            var dpt = etsGAC.DPTsSpecified ? etsGAC.DPTs : "undefined";
            //logger.LogDebug("Could not determine OpenHAB item template for {gacAddress} '{gaecName}' (DPT: '{dpt}', Label: '{gacLabel}')",
            //    gac.Address, gaec?.Name ?? "undefined", dpt, gac.Label);
            return null;
        }
        return matching;
    }

    public void SetDefaultConfig(EtsGroupAddressConfig etsGAC, GroupAddressExtraConfig? gaec, OHKnxGroupAddress gac) {
        var template = FindMatch(etsGAC, gac, gaec);
        if ( template == null )
        {
            logger.LogDebug("No matching template for GA {etsGA} '{etsGALabel}' with DPT '{dpt}' - applying defaults only.",
                etsGAC.Address.AddressAsString, gac.Label, etsGAC.DPTsSpecified ? etsGAC.DPT.EtsFormat : "undefined");
            ApplyDefaults(etsGAC, gac);
            return;
        }

        ApplyTemplate(etsGAC, gac, template);
    }

    private void ApplyTemplate(EtsGroupAddressConfig etsGAC, OHKnxGroupAddress gac, EtsGroupAddressMatch template)
    {
        var tpl = template.Template;

        gac.Label = tpl.ValueFormatSpecified ? $"{gac.Label} {tpl.ValueFormat}" : gac.Label;
        gac.ItemType = tpl.Type.ToString();
        gac.Icon = tpl.Icon;
        gac.DPTs = tpl.DataType ?? etsGAC.DPT.DotFormat ?? "";
        gac.IsStateOwned = tpl.IsReadable;
        gac.IsWritable = tpl.IsWritable;
    }

    private void ApplyDefaults(EtsGroupAddressConfig etsGAC, OHKnxGroupAddress gac)
    {
        gac.DPTs = etsGAC.HasValidDPT ? etsGAC.DPT.DotFormat : "";
    }
}
