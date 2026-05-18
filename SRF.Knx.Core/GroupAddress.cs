using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace SRF.Knx.Core;

/// <summary>
/// Group Address (GA) in the KNX network
/// Represents only the address itself, without value or configuration metadata
/// </summary>
public class GroupAddress : BusAddress, IEquatable<GroupAddress>, IEquatable<string>
{
    [XmlIgnore]
    [JsonIgnore]
    public override char Separator { get { return '/'; } }

	[XmlAttribute("Address")]
    public string AddressAsString {
        get => this.ToString();
        set => base.SetAddress(value);
    }

    public GroupAddress() : base()
    {
    }

    public GroupAddress(string addr) : base()
    {
        SetAddress(addr);
    }

    public GroupAddress(ushort groupAddress) : base(groupAddress)
    {
    }

    public static GroupAddress Parse(string ga)
    {
        return new GroupAddress(ga);
    }

    public bool Equals(GroupAddress? other)
    {   
        return Equals(other as BusAddress);
    }

    public bool Equals(GroupAddress? x, GroupAddress? y)
    {
        return x != null && y != null && x.Equals(y);
    }

    public int GetHashCode([DisallowNull] GroupAddress obj)
    {
        return obj.hashCode;
    }

    public bool Equals(string? other)
    {
        return other != null && other.Equals(Address);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as GroupAddress);
    }

    public override int GetHashCode()
    {
        return GetHashCode(this);
    }

    public override string ToString()
    {
        return _address.To3LGroupAddress(Separator);
    }
}
