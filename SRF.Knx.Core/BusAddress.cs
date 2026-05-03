using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace SRF.Knx.Core;

public abstract class BusAddress : IEquatable<BusAddress>, IEqualityComparer<BusAddress>
{
    [JsonIgnore]
    [XmlIgnore]
    abstract public char Separator { get; }

    /// <summary>
    /// The actual address
    /// </summary>
    [XmlIgnore]
    [JsonIgnore]
    public ushort Address { get => _address; set
        {
            _address = value;
            UpdateHashCode();
        }
    }
    protected ushort _address;

    protected int hashCode;

    protected BusAddress()
    {
        UpdateHashCode();
    }

    public BusAddress(ushort address)
    {
        Address = address;
    }

    protected void UpdateHashCode()
    {
        hashCode = _address.GetHashCode();
    }

    protected void SetAddress(string addr)
    {
        this.Address = addr.ToKnxGroupAddress();
    }

    public bool Equals(BusAddress? other)
    {
        if ( other == null )
            return false;
        return _address == other._address;
    }

    public bool Equals(BusAddress? x, BusAddress? y)
    {
        return x != null && y != null && (x._address == y._address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCode([DisallowNull] BusAddress obj)
    {
        return obj.hashCode;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as BusAddress);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return hashCode;
    }
}
