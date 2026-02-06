
using SRF.Knx.Config.ETS5;

namespace SRF.Knx.Config.Domain;

public class DefaultThingNameExtractor : DefaultLabelToNameConverter, IThingNameExtractor
{
    public string[] TokenSeparators { get; set; } = [" ", "_", " ", ","];

    /// <summary>
    /// Compare names after removing those tokens, ingoring caps --> match = same thing.
    /// </summary>
    public List<string> ChannelDifferentiators { get; set; } = [
        "status",
        "position",
        "winkel",
        "angle",
        "enable",
        "enabled",
        "disable",
        "disabled",
        "masked",
        "mask",
        "toggledimm",
        "toggle",
        "auto",
        "automatic",
        "automatik",
        "manuell",
        "manual",
        "force",
        "forced",
        "operation",
        "dimmer",
        "onoff",
        "switch",
        "reldimm",
        "value",
        "rel",
        "dimm",
        "command",
        "cmd",
        "feedback",
        "matchA",
        "matchB",
        "matchC",
        "matchD",
        "matchE",
        "impuls",
        "impulse",
        "kurz",
        "lang",
        "keepon",
        "minimum",
        "timed",
        "detection",
        "acknowledge",
        "funktion",
        "function",
        "1",
        "1a",
        "1b",
        "2",
        "3",
        "zu",
        "auf",
        "open",
        "close",
        "control",
        "ctrl",
        "reference",
        "soll",
        "target",
        "oben",
        "mitte",
        "unten",
        "NC",
        "NO",
        "inv",
        "silent",
        "mode",
        "indication",
        "aktiviert",
        "active",
        "reset",
        "volume",
        "rgbw",
        "rgb",
        "hsb",
        "deg",
        "azimuth",
        "elevation",
        "gesammt",
        "low",
        "local",
        "threshold"
    ];

    public new string GetThingName(EtsGroupAddressConfig etsGA)
    {
        string[] separators = [base.SeparatorChannelParameter, base.SeparatorThingChannel];
        if (separators.Any(s => etsGA.Label.Contains(s)))
        {
            // use full parsing if separators are present
            base.GetName(etsGA, out var thing, out var channel, out var parameter, out _);
            return thing;
        }
        else
        {
            // no separators present, just tokenize and remove channel differentiators to get the thing name
            var toks = base.GetName(etsGA)
                .Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return string.Join("_", toks.Where(t => !ChannelDifferentiators.Any(c => c.Equals(t, StringComparison.InvariantCultureIgnoreCase))));
        }
    }
}
