using SRF.Knx.Core.Master;

namespace SRF.Knx.Core.DPT;

public class DptMetadata(
    DataPointTypeId Id,
    DatapointType Dpt,
    DatapointSubtype Dpst,
    PropertyDataType Pdt
)
{
    public DataPointTypeId Id { get; } = Id;
    public DatapointType Dpt { get; } = Dpt;
    public DatapointSubtype Dpst { get; } = Dpst;
    public PropertyDataType Pdt { get; } = Pdt;

    public static DptMetadata FromMasterData(DataPointTypeId id, KnxMasterData masterData)
    {
        var dpt = masterData.MasterData?.DatapointTypes?.Items.Values.FirstOrDefault(dt => dt.Number == id.Main)
            ?? throw new ArgumentOutOfRangeException(nameof(id), $"DPT with main number {id.Main} not found in master data");
        DatapointSubtype dpst = dpt?.DatapointSubtypes?.DatapointSubtype.FirstOrDefault(ds => ds.Number == id.Sub)
            ?? throw new ArgumentOutOfRangeException(nameof(id), $"DPT with main number {id.Main} and sub number {id.Sub} not found in master data");
        var pdt = masterData.MasterData?.PropertyDataTypes?.ItemsByStringId.TryGetValue(dpst?.PDT ?? "", out var pdtValue) == true ? pdtValue
            : throw new ArgumentOutOfRangeException(nameof(id), $"PDT with ID {dpst?.PDT} not found in master data for DPT main {id.Main} subtype {id.Sub}");

        if ( dpst is null )
        {
            throw new ArgumentException($"Invalid DPT metadata for DPT main {id.Main} subtype {id.Sub}. DPST or PDT information is missing in master data.");
        }

        //TODO: resolve reference types in format definitions, which currently only contain the RefId but not a direct reference to the target format element. This requires building a lookup dictionary for format elements by their Id for each format definition and resolving the references after deserialization of the master data. Currently, the RefTypeFormat elements will only contain the RefId without a reference to the target format element, which needs to be resolved manually by the user of the DptMetadata.
        return new DptMetadata(id, dpt, dpst, pdt);
    }
}
