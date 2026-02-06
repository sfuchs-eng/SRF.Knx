using System;

namespace SRF.Knx.Config.Domain;

public interface IThingNameExtractor
{
    string GetThingName(ETS5.EtsGroupAddressConfig etsGA);
}
