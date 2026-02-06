using System.Text;

namespace SRF.Knx.Config.ETS5;

[Obsolete($"Use {nameof(SRF.Knx.Config.Domain.ILabelToNameConverter)} instead.")]
public static class Conversions
{
    public static string ProgNameFromEtsName(EtsGroupAddressConfig etsGA)
    {
        var label = etsGA.Label;
        StringBuilder l = new(label);

        Dictionary<string, string> replacers = new() {
            {" - ", "_"},
            {" ", "_"},
            {"-", "_"},
            {"ä", "ae"},
            {"ö", "oe"},
            {"ü", "ue"}
        };
        foreach (var r in replacers)
            l = l.Replace(r.Key, r.Value);

        var killS = new string[] { ",", ";", "-", ":", "(", ")", "/", ".", "+", "[", "]" };
        foreach (string s in killS)
            l = l.Replace(s, "");

        while (l.ToString().Contains("  "))
            l.Replace("  ", " ");

        return l.ToString();
    }
}