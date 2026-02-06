using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SRF.Knx.Config.ETS5;

namespace SRF.Knx.Config.Domain;

public partial class DefaultLabelToNameConverter : ILabelToNameConverter
{
    public string SeparatorThingChannel { get; set; } = "+";
    public string SeparatorChannelParameter { get; set; } = "#";

    [GeneratedRegex(@"\\u[0-9A-Fa-f]{4}")]
    private static partial Regex EscapedUnicodeCharacter();

    // catch a tailing [%.1f m/s] or similar and store it in a string variable
    [GeneratedRegex(@"\[(.*)\][^\[\]]+$")]
    private static partial Regex TailingValueFormatInBrackets();

    /// <summary>
    /// Converts a KNX group address label into thing, channel and parameter names suitable for .NET identifiers.
    /// It follows the principle of "one channel per KNX group address" unless separator tokens indicate differently.
    /// </summary>
    /// <param name="label">KNX Group Adress label acc. ETS configuration</param>
    /// <param name="thing"></param>
    /// <param name="channel"></param>
    /// <param name="parameter"></param>
    public void GetName(EtsGroupAddressConfig gac, out string thing, out string? channel, out string? parameter, out string? valueFormat)
    {
        var label = gac.Label;
        thing = string.Empty;
        channel = null;
        parameter = null;

        var noUnicode = EscapedUnicodeCharacter().Replace(label, string.Empty);

        // capture optional tailing [%.1f m/s] or similar into valueFormat
        if (TailingValueFormatInBrackets().IsMatch(noUnicode))
        {
            var match = TailingValueFormatInBrackets().Match(noUnicode);
            valueFormat = match.Groups[1].Value;
            noUnicode = noUnicode[..match.Index].Trim();
        }
        else
        {
            valueFormat = null;
        }

        var l = noUnicode.Trim();

        // if there's another [, cut it off
        if (label.Contains('['))
        {
            var start = label.IndexOf('[');
            var length = label.Length - start;
            l = l.Remove(start, length);
        }

        l = l.Trim();

        Dictionary<string, string> regexReplacers = new() {
            {@"\s+-\s+", "_"},
            {@"\s+", "_"},
            {@"-+", "_"},
            {@"ä", "ae"},
            {@"ö", "oe"},
            {@"ü", "ue"},
            {@"Ä", "Ae"},
            {@"Ö", "Oe"},
            {@"Ü", "Ue"},
            {@"ß", "ss"},
            {@"[^a-zA-Z0-9_]", ""},
            {@"_+", "_"},
            {@"^_+", ""},
            {@"_+$", ""},
            {@"^([0-9])", "N$1"},
            //{@"\+", "_"}, // keep + for channel separation
            //{@"#", "_"}, // keep # for parameter separation
            {@"[,;\:\(\)\/\.\[\]]", ""} // remove special chars
        };
        foreach (var r in regexReplacers)
            l = Regex.Replace(l.ToString(), r.Key, r.Value);

        var hasThingSeparator = l.Contains(SeparatorThingChannel);
        var hasChannelSeparator = l.Contains(SeparatorChannelParameter);
        var hasNoSeparator = !hasThingSeparator && !hasChannelSeparator;
        var hasBothSeparators = hasThingSeparator && hasChannelSeparator;

        if (hasNoSeparator)
        {
            thing = l;
            channel = null;
            parameter = null;
        }
        else if (hasBothSeparators)
        {
            var firstSplit = l.Split(SeparatorThingChannel, 2);
            thing = firstSplit[0];
            var secondPart = firstSplit[1];
            var secondSplit = secondPart.Split(SeparatorChannelParameter, 2);
            channel = secondSplit[0];
            parameter = secondSplit[1];
        }
        else if (hasChannelSeparator) // extract channel, parameter
        {
            var parts = l.Split(SeparatorChannelParameter, 2);
            thing = parts[0];
            channel = null;
            parameter = parts[1];
        }
        else
        {
            // hasThingSeparator
            var parts = l.Split(SeparatorThingChannel, 2);
            thing = parts[0];
            channel = parts[1];
            parameter = null;
        }

        // camelcase all parts
        thing = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(thing).Replace("_", string.Empty);
        channel = channel != null ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(channel).Replace("_", string.Empty) : null;
        parameter = parameter != null ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parameter).Replace("_", string.Empty) : null;
    }

    public string GetName(EtsGroupAddressConfig gac)
    {
        GetName(gac, out var thing, out var channel, out var parameter, out _);

        return string.Join("_", new[] { thing, channel, parameter }.Where(s => !string.IsNullOrEmpty(s)));
    }

    public string GetThingName(EtsGroupAddressConfig gac)
    {
        GetName(gac, out var thing, out _, out _, out _);
        return thing;
    }
}
