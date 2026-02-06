using System;

namespace SRF.Knx.Core;

/// <summary>
/// Device address in the KNX network
/// </summary>
public class IndividualAddress : BusAddress
{
    public override char Separator { get { return '.'; } }

    public IndividualAddress() : base()
    {
    }

    public IndividualAddress(string addr) : base()
    {
        SetAddress(addr);
    }

    public IndividualAddress(ushort address) : base(address)
    {}
    public override string ToString()
    {
        return _address.To3LIndividualAddress(Separator);
    }
}
