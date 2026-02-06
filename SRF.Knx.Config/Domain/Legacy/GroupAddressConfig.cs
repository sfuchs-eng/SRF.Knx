// // S. Fuchs, http://sfuchs.ch/
//
using System.Xml.Linq;
using System.Text;
using SRF.Knx.Config.Exceptions;
using SRF.Knx.Core;

namespace SRF.Knx.Config.Domain.Legacy;

[Obsolete("Is this the legacy ETS GA export loader? don't use.")]
public class GroupAddressConfig : KnxGroupAddressConfig
{
    //readonly bool GenerateRead = false;

    public int AddressInt;
    public string ItemFile = string.Empty;

    public string EtsDPTs = string.Empty;

    public bool Unfiltered
    {
        get
        {
            if (GA != null)
                return GA.Unfiltered;
            else
                throw new NotImplementedException(
                    string.Format("GroupAddressConfig property Unfiltered is only known if a GroupAddress is associated with the GroupAddressConfig {0}/'{1}': '{2}'",
                                Address.Address, Name, Label));
        }
    }

    EtsGroupAddress? GA;
    public EtsGroupAddress GroupAddress
    {
        get
        {
            return GA ?? new();
        }
        set
        {
            GA = value;
            if (GA == null)
                return;

            if (string.IsNullOrEmpty(Label))
                Label = GA.Name;
            if (string.IsNullOrEmpty(Name))
                Name = SanitizeName(GA.Name);
        }
    }

    public GroupAddressConfig(XElement cfg)
    {
        TryParse(cfg);
    }

    public GroupAddressConfig(EtsGroupAddress ga)
    {
        GroupAddress = ga; // assigns Name & Label

        Address = new GroupAddress(ga.Address);

        try
        {
            ItemType = GuessItemType(Label);
        }
        catch (Exception)
        {
        }

        IsNew = true;
    }

    public bool IsValid
    {
        get
        {
            return Address.Address.Equals(GA?.Address);
        }
    }

    public override bool TryParse(XElement x)
    {
        string? tmp = null;

        Address = new GroupAddress(x.Attribute("Address")?.Value ?? "0/0/0");
        AddressInt = Address.Address;

        IsNew = x.Attribute("New") != null;

        OptionalPick(x, "Label", (s) => Label = s);

        if (OptionalPick(x, "Name", (s) => Name = s))
        {
            // fix names in case they don't adhere to the requirements.
            Name = SanitizeName(Name);
        }
        else if (!string.IsNullOrEmpty(Label))
        {
            Name = SanitizeName(Label);
        }

        if (OptionalPick(x, "Readable", (s) => tmp = s))
            IsReadable = tmp?.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;
        else
            IsReadable = false;

        if (OptionalPick(x, "Groups", (s) => tmp = s))
        {
            Groups = tmp?.Trim().Replace(" ", "").Split(new char[] { ',' }) ?? [];
        }

        //OptionalPick(x, "MapType",  (s) => MapType = s);
        OptionalPick(x, "Mappings", (s) => Mappings = s);

        tmp = null;
        if (OptionalPick(x, "NoItem", (s) => tmp = s))
        {
            if (string.IsNullOrEmpty(tmp))
                NoItem = true;
            else
            {
                bool.TryParse(tmp, out bool btmp);
                NoItem = btmp;
            }
        }

        if (OptionalPick(x, "AdditionalGA", (s) => tmp = s))
        {
            AdditionalGA = [.. tmp?
                .Split (new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()) ?? []];
        }

        ItemType = @"# invalid ";
        if ((!OptionalPick(x, "ItemType", (s) => ItemType = s)) && !string.IsNullOrEmpty(Label))
        {
            try
            {
                ItemType = GuessItemType(Label);
            }
            catch (Exception)
            { }
            //MapType = GuessMapType(ItemType);
        }

        if ((!OptionalPick(x, "Icon", (s) => Icon = s) || string.IsNullOrEmpty(Icon))
            && !string.IsNullOrEmpty(Label))
        {
            Icon = string.Empty;
        }

        OptionalPick(x, "DataType", (s) => DataType = s);
        if (string.IsNullOrEmpty(DataType))
            DataType = string.Empty;

        OptionalPick(x, "ItemFile", (s) => ItemFile = s);
        if (string.IsNullOrEmpty(ItemFile))
            ItemFile = string.Empty;

        OptionalPick(x, "Owner", (s) => Owner = s);
        if (string.IsNullOrEmpty(Owner))
            Owner = string.Empty;
        else
            Owner = Owner.ToLower().Trim();

        OptionalPick(x, "EtsDPTs", (s) => EtsDPTs = s);

        /*
        if ( string.IsNullOrEmpty(MapType) && !(string.IsNullOrEmpty(ItemType) || ItemType.Equals(@"# invalid ")) )
            MapType = GuessMapType(ItemType);*/

        // sanity checks
        if (string.IsNullOrEmpty(ItemType))
        {
            //throw new NoItemTypeException(string.Format("Failed to parse or infer ItemType."), Address, Label);
            Console.Error.WriteLine("Failed to parse or infer ItemType: {0} {1}", Address, Label);
        }
        if (string.IsNullOrEmpty(DataType))
        {
            //throw new NoDataTypeException(string.Format("Failed to parse or infer DataType: {0} {1}", Address, Label));
            Console.Error.WriteLine("Failed to parse or infer DataType: {0} {1}", Address, Label);
        }

        return true;
    }

    /// <summary>
    /// Which (part of the) system owns the item status?
    /// "knx" (bus device)
    /// "oh" (openhab on asgard.fu)
    /// "srv" (BHw17Logic server)
    /// "?" (unknown)
    /// </summary>
    /// <param name="owner">Owner.</param>
    private void GuessOwner(ref string owner)
    {
        if (IsReadable)
        {
            owner = "knx";
            return;
        }

        owner = "?";
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

    public string ChannelType
    {
        get
        {
            // generate OH2 channel types from OH1 item types
            return ItemType.ToLower();
        }
    }

    public string ChannelID
    {
        get
        {
            return "ch" + Name;
        }
    }

    public string ChannelLabel
    {
        get
        {
            var s = Label.Split(new char[] { '[' }).FirstOrDefault();
            if (string.IsNullOrEmpty(s))
                return Label;
            return s.Trim();
        }
    }

    [Obsolete]
    private string GetDefaultDpt(string chanType, string prefix)
    {
        throw new NotImplementedException();
        /*
        var tm = MainClass.GetOH2TypeMap();
        var dptCand = tm.Descendants("DPT")
                .Where(d =>
                {
                    var p = d.Parent.Attribute("Prefix");
                    var t = d.Parent.Parent.Attribute("Type");
                    return t != null && chanType.Equals(t.Value) && p != null && prefix.Equals(p.Value);
                });
        return dptCand.FirstOrDefault()?.Value;
        */
    }

    [Obsolete]
    private bool IsDefaultTypeMapping(string chanType, string prefix, string dpt)
    {
        throw new NotImplementedException();
        /*
        var result = dpt.Equals(GetDefaultDpt(chanType, prefix));

        if (MainClass.DebugTypeMapping && !result)
            Console.Error.WriteLine("IsDefaultTypeMapping({0}, {1}, {2}) = {3} // GetDefaultDpt = {4} ({5})", chanType, prefix, dpt, result, GetDefaultDpt(chanType, prefix), Name);

        return result;
        */
    }


    string SanitizeName(string label)
    {
        StringBuilder l = new StringBuilder(label);

        Dictionary<string, string> replacers = new Dictionary<string, string>() {
            {" - ", "_"},
            {" ", "_"},
            {"ä", "ae"},
            {"ö", "oe"},
            {"ü", "ue"}
        };
        foreach (var r in replacers)
            l = l.Replace(r.Key, r.Value);

        var killS = new string[] { ",", ";", "-", ":", "(", ")", "/", ".", "+" };
        foreach (string s in killS)
            l = l.Replace(s, "");

        while (l.ToString().Contains("  "))
            l.Replace("  ", " ");

        return l.ToString();
    }

    string GuessItemType(string label)
    {
        if (label.Contains("Lamellen") && label.Contains("Position"))
            return "Number";

        if (label.Contains("Lamellen") && label.Contains("Winkel"))
            return "Number";

        if (label.Contains("Lampe") && label.Contains("value"))
            return "Number";

        if (label.Contains("[") && label.Contains("]"))
            return "Number";

        if (label.Contains("Lampe"))
            return "Switch";

        if (label.Contains("teckdose"))
            return "Switch";

        if (label.Contains("Szene"))
            return "Number";

        if (label.Contains("Heizungsventil"))
            return "Dimmer";

        if (label.Contains("detektion"))
            return "Contact";

        if (label.Contains("geschlossen"))
            return "Contact";

        if (label.Contains("overload"))
            return "Contact";

        if (label.Contains("trigger"))
            return "Switch";

        throw new KnxConfigurationException(string.Format("Couldn't figure out the item type for {0}.", Address));
    }

    [Obsolete]
    void GuessDataType(ref string dataType)
    {
        throw new NotImplementedException();
        /*
        // "DataNode" / [] of strings which must be contained in the label
        var typeMapLabel = new StringMatchStore() {
            {"5.001", "Lampe", "value"},
            {"3.007", "Lampe", "rel-dimm"},
            {"5.004", "Lamellen", "Position"},
            {"5.004", "Lamellen", "Winkel"},
            {"1.001", "Rolladen", "trigger"},
            {"1.005", "Stellantrieb", "overload or short"},
            {"17.001", "Szene", "Lamellen"}, // Scene control, activate scene number, add 64 for storing the scene number
            {"17.001", "Szene", "Storen"},
            {"9.001", "emperatur", "C]"},
            {"5.001", "Heizungsventil"},
            {"9.004", "elligkeit", "lux]"},
            {"17.001", "Szene"},
            {"1.001", "Lampe"},
            {"1.001", "teckdose"},
            {"1.001", "etektion"},
            {"1.001", "geschlossen"},
            {"9.005", "eschwindigkeit"},
            {"5.001", "ftungsstufe"}, // Lüftungsstufe
            {"14.027", "voltage"},
            {"14.019", "current"},
        };

        var typeMapItem = new StringMatchStore() {
            {"1.001", "Switch"},
        {"1.005", "Switch"},
            {"16.001", "String"}, // ASCI, use 16.002 for ISO 8895-1
            {"1.005", "Contact"},
            {"5.001", "Dimmer"},
        };

        var x = typeMapLabel.FirstOrDefault(d => d.Value.All (s => Label.Contains (s)));
        var y = typeMapItem.FirstOrDefault(d => (! string.IsNullOrEmpty(ItemType)) && d.Value.All(s => ItemType.Equals(s)));

        if ( !string.IsNullOrEmpty(x.Key) )
            dataType = x.Key;
        else if ( !string.IsNullOrEmpty(y.Key) )
            dataType = y.Key;
            */
    }

    [Obsolete]
    string GuessIcon(string label)
    {
        throw new NotImplementedException();
        /*
        var defaultIcons =  new StringMatchStore() {
            {"rollershutter", "Lamelle"},
            {"temperature", "Raumtemperatur"},
            {"window", "Fenster"},
            {"slider", "value"},
            {"switch", "switch"},
            {"light-on", "Lighton"},
            {"hue", "value_command"},
            {"hue", "value_status"},
            {"hue", "value_feedback"},
            {"sun", "Helligkeit"},
            {"temperature", "temperatur"},
            {"wind", "wind"},
            {"wind", "lueftung"},
            {"switch", "command"},
            {"contact", "feedback"},
            {"light", "Lampe"},
            {"heating", "FBH"},
            {"heating", "Heizungsventil"},
            {"energy", "volage"},
            {"energy", "current"},
        {"siren", "larm"},
        };

        var x = defaultIcons.FirstOrDefault(i =>  i.Value.All(s => Label.ToLower().Contains(s.ToLower())));

        if ( !string.IsNullOrEmpty(x.Key))
            return x.Key;
        return null;
        */
    }

    [Obsolete]
    void GuessItemFile(ref string itemFile)
    {
        throw new NotImplementedException();
        /*
        if ( (!string.IsNullOrEmpty(Icon)) && Icon.Equals("rollershutter") )
        {
            itemFile = "Rolladen";
        }
        else if (
            ((!string.IsNullOrEmpty(Icon)) && Icon.Equals("rollershutter"))
            || ((Groups != null) && (Groups.Contains("gLichtInnen") || Groups.Contains("gLichtAussen")))
        )
        {
            itemFile = "Licht";
        }
        else if ( Groups != null && Groups.Contains("gFBHVentile") )
        {
            itemFile = "Fussbodenheizung";
        }
        else if ( (!string.IsNullOrEmpty(Icon)) && Icon.Equals("window") )
        {
            itemFile = "Fenster";
        }
        else if ( Groups != null && Groups.Contains("gSteckdosen") )
        {
            itemFile = "Steckdosen";
        }
        */
    }

    [Obsolete]
    string GuessMapType(string itemType)
    {
        throw new NotImplementedException();
        /*
        if (string.IsNullOrEmpty(itemType))
            return null;

        if ( (new string[] {"Dimmer", "Rollershutter"}).Any(s => s.Equals(itemType)) )
            return "Slider";

        if ( (new string[] {"Number"}).Any(s => s.Equals(itemType)) )
            return "Setpoint";

        if ( (new string[] {"Switch", "Contact"}).Any(s => s.Equals(itemType)))
            return "Switch";

        return null;
        */
    }
}
