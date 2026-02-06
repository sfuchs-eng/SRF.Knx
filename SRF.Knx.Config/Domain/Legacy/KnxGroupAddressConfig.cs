using System;
using System.Xml.Linq;
using SRF.Knx.Core;

namespace SRF.Knx.Config.Domain.Legacy;

public class KnxGroupAddressConfig
{
    public GroupAddress Address { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string[] Groups { get; set; } = [];
    public bool IsReadable { get; set; } = false;
    public string Icon { get; set; } = string.Empty;
    public bool IsNew { get; set; } = false;
    public bool NoItem { get; set; } = false;
    public string[] AdditionalGA { get; set; } = [];
    public KnxGroupAddressConfig[] AdditionalGAC { get; set; } = [];
    public string MapType { get; set; } = string.Empty;
    public string Mappings { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;

    public KnxGroupAddressConfig(XElement cfg)
    {
        TryParse(cfg);
    }

    public KnxGroupAddressConfig()
    {
    }

    public virtual bool TryParse(XElement x)
    {
        string? tmp = null;

        Address = new GroupAddress(x.Attribute("Address")?.Value ?? throw new Exceptions.KnxGAConfigurationException("missing GAC address attribute"));

        IsNew = x.Attribute("New") != null;

        OptionalPick(x, "Label", (s) => Label = s);

        if (!OptionalPick(x, "Name", (s) => Name = s))
        {
            Console.WriteLine("Name for group address '{0}': {1} missing.", Label, Address.Address);
        }

        IsReadable = false;
        OptionalPick(x, "Readable", (s) => IsReadable = s.Equals("true", StringComparison.InvariantCultureIgnoreCase));

        OptionalPick(x, "Groups", (s) => Groups = s.Trim().Replace(" ", "").Split([',']));

        OptionalPick(x, "Mappings", (s) => Mappings = s);

        tmp = null;
        if (OptionalPick(x, "NoItem", (s) => tmp = s))
        {
            if (string.IsNullOrEmpty(tmp))
                NoItem = true;
            else
            {
                _ = bool.TryParse(tmp, out bool btmp);
                NoItem = btmp;
            }
        }

        if (OptionalPick(x, "AdditionalGA", (s) => tmp = s))
        {
            AdditionalGA = tmp?
                .Split([",", ";"], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray() ?? [];
        }

        OptionalPick(x, "ItemType", (s) => ItemType = s);
        OptionalPick(x, "Icon", (s) => Icon = s);

        if (!OptionalPick(x, "DataType", (s) => DataType = s))
        {
            Console.WriteLine("Undefined data type for group address {0}: {1}",
                                 Address.ToString(), Label);
        }

        OptionalPick(x, "MapType", (s) => MapType = s);
        OptionalPick(x, "Mappings", (s) => Mappings = s);
        OptionalPick(x, "Owner", (s) => Owner = s);

        return true;
    }

    static bool OptionalPick(XElement x, string attrName, Action<string> setter)
    {
        XAttribute? a;
        a = x.Attribute(attrName);
        if (a != null && !string.IsNullOrEmpty(a.Value))
        {
            setter(a.Value);
            return true;
        }
        return false;
    }

    static string EmptyIfNull(string? s)
    {
        return s ?? "";
    }

    public int IndexOfAddGA(string address)
    {
        if (AdditionalGA == null || AdditionalGA.Length < 1)
            return -1;
        for (int i = 0; i < AdditionalGA.Length; i++)
            if ((!string.IsNullOrEmpty(AdditionalGA[i])) && AdditionalGA[i].Equals(address))
                return i;
        return -1;
    }

    public static void PopulateAdditionalGAC(IEnumerable<KnxGroupAddressConfig> allGAC)
    {
        List<KnxGroupAddressConfig> addGAC;
        foreach (KnxGroupAddressConfig gac in allGAC)
        {
            if (gac.AdditionalGA == null || gac.AdditionalGA.Length < 1)
                continue;
            addGAC = new List<KnxGroupAddressConfig>();
            addGAC.AddRange(allGAC.Where(g => gac.AdditionalGA.Contains(g.Address.ToString())));
            // how to bring into same order as the elements in AdditionalGA?
            gac.AdditionalGAC = addGAC.OrderBy(g => gac.IndexOfAddGA(g.Address.ToString())).ToArray();
        }
    }

    public override string ToString()
    {
        return string.Format("{0}, {1}", Address, Label);
    }
}
