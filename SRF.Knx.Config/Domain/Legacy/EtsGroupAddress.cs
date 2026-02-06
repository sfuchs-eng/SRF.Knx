using System.Xml.Linq;

namespace SRF.Knx.Config.Domain.Legacy;

public class EtsGroupAddress
{
    public string Name = string.Empty;
    public int AddressInt;

    string address = string.Empty;
    public string Address {
        get {
            return address;
        }
        set {
            address = value;
            AddressInt = ToInt(value);
        }
    }

    public string DPTs { get; protected set; } = string.Empty;

    public bool Unfiltered;

    public EtsGroupAddress ()
    {
    }

    public EtsGroupAddress(XElement x)
    {
        TryParse (x);
    }

    public void TryParse(XElement x)
    {
        XAttribute? a;

        Name = x.Attribute("Name")?.Value ?? string.Empty;
        Address = x.Attribute ("Address")?.Value ?? string.Empty;
        AddressInt = AddressAsInt;
        DPTs = x.Attribute("DPTs")?.Value ?? string.Empty;

        a = x.Attribute("Unfiltered");
        if (a != null)
            Unfiltered = a.Value.Equals ("true", StringComparison.InvariantCultureIgnoreCase);
        else
            Unfiltered = false;
    }

    public string GetDPTsInDotFormat()
    {
        if (string.IsNullOrEmpty(DPTs))
            return string.Empty;
        var dpt = DPTs.Trim().Split(new char[] { ',', ';', ' ' })[0];
        var seg = dpt.Split(new char[] { '-' });
        if ( "DPT".Equals(seg[0]) && seg.Length == 2)
        {
            // "DPT-5" format
            return seg[1] + ".000";
        }
        else if ( "DPST".Equals(seg[0]) && seg.Length == 3 )
        {
            return seg[1] + "." + string.Format("{0:D3}", int.Parse(seg[2]));
        }
        else
        {
            Console.Error.WriteLine("EtsGroupAddress.GetDPTsInDotFormat: unknown DPT format '{0}'.", DPTs);
            return string.Empty;
        }
    }

    public int AddressAsInt {
        get {
            return ToInt(Address);
        }
    }

    public static int ToInt(string groupAddress)
    {
        string[] a = groupAddress.Split (new char[] { '/', '.' });
        return int.Parse (a [0]) * 2048 + int.Parse (a [1]) * 256 + int.Parse (a [2]);
    }
}