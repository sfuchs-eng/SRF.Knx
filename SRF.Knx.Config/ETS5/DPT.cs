using System.Xml.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Schema;
using System.Xml;

namespace SRF.Knx.Config.ETS5;

/// <summary>
/// KNX Data Point Type
///TODO: support "vendor specific" DPTs, like e.g. 232.60000 (MDT HSB instead of RGBW 232.600) or 251.60600 (single frame instead of separate with 251.600) needed for OpenHAB
/// </summary>
public class DPT : IEquatable<DPT>, IEqualityComparer<DPT>, IXmlSerializable
{
    /// <summary>
    /// Main Type
    /// </summary>
    public int Main { get; set; }

    /// <summary>
    /// Sub-Type
    /// </summary>
    public int Sub { get; set; }

    public bool IsMainOnly => Sub == 0;

    public DPT(string? dpts)
    {
        InitFromString(dpts);
    }

    public DPT()
    {
    }

    public bool IsValidMainType { get => Main != 0; }

    public bool IsValidType { get => IsValidMainType && Sub != 0; }

    private void InitFromString(string? etsDpt)
    {
        if (string.IsNullOrEmpty(etsDpt))
            return;
        if (TryParse(etsDpt, out int maj, out int min))
        {
            Main = maj;
            Sub = min;
        }
        else
            throw new ArgumentException($"Failed to parse '{etsDpt}' into {this.GetType().FullName}");
    }

    public static bool TryParse(string dpt, out int major, out int minor)
    {
        major = 0;
        minor = 0;
        var dotFormat = dpt.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (dotFormat.Length == 2)
        {
            // "1.001" type of DPT formulation
            return int.TryParse(dotFormat[0], out major) && int.TryParse(dotFormat[1], out minor);
        }

        var etsFormat = dpt.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if ("DPT".Equals(etsFormat[0]))
        {
            // "DPT-1" type for DPT formulation
            return int.TryParse(etsFormat[1], out major);
        }
        if ("DPST".Equals(etsFormat[0]))
        {
            // "DPST-1-1" type of DPT formulation
            return int.TryParse(etsFormat[1], out major) && int.TryParse(etsFormat[2], out minor);
        }

        if (int.TryParse(dpt, out major))
        {
            // "1" type of DPT formulation, main only
            minor = 0;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Equals each <see cref="Main"/> &amp; <see cref="Sub"/> unless one of the two is <see cref="IsMainOnly"/>, then only Main are equaled.
    /// E.g. DPT 1 equals DPT 1.001, but DPT 1.001 does not equal DPT 1.002
    /// </summary>
    public bool Equals(DPT? other)
    {
        return (Main == other?.Main && Sub == other?.Sub) || ((IsMainOnly || (other?.IsMainOnly ?? false)) && Main == other?.Main);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not DPT)
            return false;
        return Equals(obj as DPT);
    }

    public string DotFormat { get
        {
            var sub = Sub < 999 ? Sub.ToString("D3") : Sub.ToString();
            return $"{Main}.{sub}";
        }
    }

    public string EtsFormat
    {
        get
        {
            if (Sub > 0)
                return $"DPST-{Main}-{Sub}";
            else
                return $"DPT-{Main}";
        }
    }

    public override string ToString()
    {
        return DotFormat;
    }

    public bool Equals(DPT? x, DPT? y)
    {
        return x != null && y != null && x.Equals(y);
    }

    public int GetHashCode([DisallowNull] DPT obj)
    {
        HashCode hc = new HashCode();
        hc.Add(Main);
        hc.Add(Sub);
        return hc.ToHashCode();
    }

    public override int GetHashCode()
    {
        return GetHashCode(this);
    }

    public XmlSchema? GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        switch (reader.NodeType)
        {
            case XmlNodeType.Element:
            case XmlNodeType.Attribute:
                InitFromString(reader.ReadContentAsString());
                return;
        }
        throw new ArgumentException($"{this.GetType()}.ReadXml not on a suitable node to parsean ETS DPT value from.");
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("DPT", EtsFormat);
    }

    public static DPT CreateInvalid()
    {
        return new();
    }
}
