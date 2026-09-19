using SRF.Knx.Core.DPT;

namespace SRF.Knx.Core;

/// <summary>
/// Maps KNX DPST IDs to UnitsNet dimensions and units.
/// Serves the creation of UnitsNet Quantity based DPSTs (DptSimpleUnitsNet) and the mapping of KNX DPSTs to UnitsNet dimensions and units for application use where the information cannot be retrieved from the DPST.
/// </summary>
public interface IUnitSystemsMapper
{
    DptUnitsNetMapping? GetDpstUnitMapping(DataPointTypeId dpstId, string? queryContextForLogging = null);
    DptUnitsNetMapping? GetDptUnitsNetMapping(DptMetadata dptMetadata, string? queryContextForLogging = null);
}
