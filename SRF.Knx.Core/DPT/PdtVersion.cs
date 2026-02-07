using System;

namespace SRF.Knx.Core.DPT;

public class PdtVersion
{
    // U5U5U6: 5 bits for major version, 5 bits for minor version, 6 bits for patch version
    public PdtVersion(byte major, byte minor, byte patch)
    {
        if (major > 31) throw new ArgumentOutOfRangeException(nameof(major), "Major version must be between 0 and 31");
        if (minor > 31) throw new ArgumentOutOfRangeException(nameof(minor), "Minor version must be between 0 and 31");
        if (patch > 63) throw new ArgumentOutOfRangeException(nameof(patch), "Patch version must be between 0 and 63");
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    public PdtVersion(byte[] bytes)
    {
        if (bytes.Length != 2) throw new ArgumentException("Version must be exactly 2 bytes long", nameof(bytes));
        var value = BitConverter.ToUInt16(bytes, 0);
        Major = (byte)((value >> 11) & 0x1F); // Extract bits 15-11
        Minor = (byte)((value >> 6) & 0x1F);  // Extract bits 10-6
        Patch = (byte)(value & 0x3F);         // Extract bits 5-0
    }
    
    public byte Major { get; init; }
    public byte Minor { get; init; }
    public byte Patch { get; init; }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Patch}";
    }

    public static implicit operator ushort(PdtVersion version)
    {
        return (ushort)((version.Major << 11) | (version.Minor << 6) | version.Patch);
    }

    public byte[] GetBytes()
    {
        ushort value = this; // Implicit conversion to ushort
        return BitConverter.GetBytes(value);
    }
}
