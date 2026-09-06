using SRF.Knx.Core.DPT;

namespace SRF.Knx.Core;

/// <summary>
/// Maps KNX DPTs to UnitsNet dimensions and units.
/// </summary>
public interface IUnitSystemsMapper
{
    DptUnitsNetMapping? GetDptUnitMapping(DptSimple dptSimple);
}
