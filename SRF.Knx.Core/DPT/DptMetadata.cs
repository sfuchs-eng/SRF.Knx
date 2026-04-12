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
    public required PropertyDataType Pdt { get; init; }

    public static DptMetadata FromMasterData(DataPointTypeId id, KnxMasterData masterData)
    {
        var dpt = masterData.MasterData?.DatapointTypes?.Items.Values.FirstOrDefault(dt => dt.Number == id.Main)
            ?? throw new ArgumentOutOfRangeException(nameof(id), $"DPT with main number {id.Main} not found in master data");

        // When only the main number is given, resolve the PDT from the parent DPT or its first sub-type.
        if (id.IsMainOnly)
        {
            var pdtIdMainOnly = !string.IsNullOrEmpty(dpt.PDT)
                ? dpt.PDT
                : dpt.DatapointSubtypes?.DatapointSubtype.FirstOrDefault(ds => !string.IsNullOrEmpty(ds.PDT))?.PDT
                  ?? throw new ArgumentOutOfRangeException(nameof(id), $"No PDT could be resolved for DPT main number {id.Main} in master data");
            var pdtMainOnly = masterData.MasterData?.PropertyDataTypes?.ItemsByStringId.TryGetValue(pdtIdMainOnly, out var pdtMainOnlyValue) == true ? pdtMainOnlyValue
                : throw new ArgumentOutOfRangeException(nameof(id), $"PDT with ID {pdtIdMainOnly} not found in master data for DPT main {id.Main}");
            return new DptMetadata() { Id = id, Dpt = dpt, Dpst = null, Pdt = pdtMainOnly };
        }

        DatapointSubtype dpst = dpt?.DatapointSubtypes?.DatapointSubtype.FirstOrDefault(ds => ds.Number == id.Sub)
            ?? throw new ArgumentOutOfRangeException(nameof(id), $"DPT with main number {id.Main} and sub number {id.Sub} not found in master data");
        // DPST may not carry its own PDT attribute; fall back to the parent DPT's PDT in that case.
        var pdtId = !string.IsNullOrEmpty(dpst?.PDT) ? dpst.PDT : dpt?.PDT ?? "";
        var pdt = masterData.MasterData?.PropertyDataTypes?.ItemsByStringId.TryGetValue(pdtId, out var pdtValue) == true ? pdtValue
            : throw new ArgumentOutOfRangeException(nameof(id), $"PDT with ID {pdtId} not found in master data for DPT main {id.Main} subtype {id.Sub}");

        return new DptMetadata() { Id = id, Dpt = dpt!, Dpst = dpst, Pdt = pdt };
    }
}
