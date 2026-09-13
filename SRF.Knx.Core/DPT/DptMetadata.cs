using Microsoft.Extensions.Logging;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Core.DPT;

/// <summary>
/// Represents metadata information for a DPT, including its ID, DPST information, and the associated PDT from the KNX masterdata file.
/// This metadata is used for DPT instantiation and encoding/decoding based on master data definitions.
/// </summary>
/// <remarks>
/// When <see cref="DataPointTypeId.IsMainOnly"/> is <see langword="true"/>, <see cref="Dpst"/> will be <see langword="null"/>
/// and the PDT is resolved from the parent <see cref="DatapointType"/> or its first available sub-type.
/// </remarks>
public class DptMetadata
{
    public required DataPointTypeId Id { get; init; }
    public required DatapointType Dpt { get; init; }

    /// <summary>
    /// The specific sub-type from master data. <see langword="null"/> when the DPT was resolved from a main-number-only identifier.
    /// </summary>
    public DatapointSubtype? Dpst { get; init; } = null;

    /// <summary>
    /// Associate the PDT for convenience, as it is needed for encoding/decoding and may be resolved from the parent DPT when only the main number is given.
    /// </summary>
    public required PropertyDataType Pdt { get; init; }

    /// <summary>
    /// Are we carrying full DPST (Datapoint Sub Type) information, or is this a main-number-only DPT with no specific sub-type?
    /// </summary>
    public bool IsDpst => Dpst != null && !Id.IsMainOnly;

    public static DptMetadata FromMasterData(DataPointTypeId id, KnxMasterData masterData, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(masterData.MasterData);

        bool subFound = true;

        // DPST / DPT lookup
        if (!masterData.MasterData.TryGetDataPointSubType(id, out var dpt, out var dpst))
        {
            if (!id.IsMainOnly)
                throw new ArgumentOutOfRangeException(nameof(id), $"DPST {id.ToString()} not found in master data");

            subFound = false;
            if (!masterData.MasterData.TryGetDataPointType(new DataPointTypeId(id.Main), out dpt))
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"DPT {id.ToString()} not found in master data");
            }
            else
            {
                logger?.LogTrace("DPT {DptId} not found in master data, but main DPT {MainDptId} was found. This may be a main-number-only DPT without a specific sub-type.", id, new DataPointTypeId(id.Main));
            }
        }

        // PDT lookup
        if (!masterData.MasterData.TryGetPropertyDataType(id, out var pdt))
        {
            if (!masterData.MasterData.TryGetPropertyDataType(new DataPointTypeId(id.Main), out pdt))
                throw new ArgumentOutOfRangeException(nameof(id), $"PDT for DPT main {id.Main} and sub {id.Sub} not found in master data. Neither DPST nor parent DPT has a valid PDT reference.");
            else
                logger?.LogTrace("DPST {DptId} has no PDT, resolved PDT {PdtId} of parent DPT {ParentDptId}", id, pdt.Id, new DataPointTypeId(id.Main));
        }
        else
        {
            logger?.LogTrace("DPST {DptId} has PDT {PdtId}", id, pdt.Id);
        }

        if (!subFound)
            return new DptMetadata() { Id = id, Dpt = dpt!, Dpst = null, Pdt = pdt };
        else
            return new DptMetadata() { Id = id, Dpt = dpt!, Dpst = dpst, Pdt = pdt };
    }
}
