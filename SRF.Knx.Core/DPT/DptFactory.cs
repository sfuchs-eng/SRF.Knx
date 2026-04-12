using Microsoft.Extensions.Logging;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Core.DPT;

public class DptFactory(
    Master.KnxMasterData masterData,
    IPdtEncoderFactory pdtEncoderFactory,
    IDptNumericInfoFactory dptNumericInfoFactory,
    ILogger<DptFactory> logger) : IDptFactory
{
    private readonly Master.KnxMasterData masterData = masterData;
    private readonly IPdtEncoderFactory pdtEncoderFactory = pdtEncoderFactory;
    private readonly IDptNumericInfoFactory numericInfoFactory = dptNumericInfoFactory;
    private readonly ILogger<DptFactory> logger = logger;

    public DptBase Get(int main, int sub)
    {
        return Get(new DataPointTypeId(main, sub));
    }

    public DptBase Get(DataPointTypeId dpstId)
    {
        // Fail if main type only
        //TODO: get a PDT Encoder nevertheless and create a functioning DPT, but log a warning that this is not a recommended way to use DPTs, as the main number alone does not uniquely identify a DPT subtype and may lead to incorrect behavior if the wrong PDT is assumed.
        if (dpstId.IsMainOnly)
        {
            logger.LogWarning("DPT with main number {Main} and no sub number is not supported. Sub number is required to identify a specific DPT subtype.", dpstId.Main);
            throw new ArgumentException($"DPT with main number {dpstId.Main} and no sub number is not supported. Sub number is required to identify a specific DPT subtype.", nameof(dpstId));
        }

        var dptMeta = DptMetadata.FromMasterData(dpstId, masterData);

        // Try to find a DPT creator based on the DPST ID
        if (DptCreatorsById.TryGetValue(dpstId, out var creatorInfo))
        {
            return creatorInfo.Creator(dptMeta, numericInfoFactory);
        }

        // If no creator found by DPST ID, try to find a creator based on the PDT name
        if (DptCreatorsByPdt.TryGetValue(dptMeta.Pdt.Number, out var creatorInfoByPdt))
        {
            return creatorInfoByPdt.Creator(dptMeta, numericInfoFactory);
        }

        // use the PDT encoder factory as fall back if no specific DPT creator is found, but a PDT encoder exists for the PDT specified in master data.
        // This allows for dynamic DPT creation based on available PDT encoders, even if no specific DPT creator is registered for the DPST ID or PDT.
        var pdtEncoder = pdtEncoderFactory.GetPdtEncoder(dptMeta.Pdt);
        var numericInfo = numericInfoFactory.GetNumericInfo(dptMeta, out var isNumeric);
        if (pdtEncoder != null)
        {
            // use the template type of IPdtEncoder<T> to create a DptSimple<T> instance, where T is the type handled by the PDT encoder.
            var dptType = typeof(DptSimple<>).MakeGenericType(pdtEncoder.GetType().GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(PdtEncoder<>))
                .GetGenericArguments()[0]);
            var dpt = (DptBase)(Activator.CreateInstance(dptType, [pdtEncoder, numericInfo!])
                ?? throw new InvalidOperationException($"Failed to create DPT instance of type {dptType} for DPST {dpstId} using PDT encoder for PDT {dptMeta.Pdt.Name}"));
            return dpt;
        }

        logger.LogWarning("No DPT creator found for DPT main {Main} subtype {Sub} with PDT {PdtName}", dpstId.Main, dpstId.Sub, dptMeta.Pdt.Name);
        throw new NotSupportedException($"No DPT creator found for DPT main {dpstId.Main} subtype {dpstId.Sub} with PDT {dptMeta.Pdt.Name}");
    }

    public record DptCreator(
        Func<
            DptMetadata,
            IDptNumericInfoFactory,

            DptBase> Creator
        );

    /// <summary>
    /// Dictionary mapping <see cref="DataPointTypeId"/> to DPT creator functions, creating derivatives of <see cref="DptBase"/>.
    /// First <see cref="DptCreatorsById"/> is searched, if no entry matches, then <see cref="DptCreatorsByPdt"/> is searched using the PDT name from master data.
    /// This allows for flexible and dynamic DPT instantiation based on master data information.
    /// </summary>
    public Dictionary<DPT.DataPointTypeId, DptCreator> DptCreatorsById = new()
    {
        /*
        { new DataPointTypeId("1.001"), new DptCreator((dptm,nif) => new DptSimple<UInt32>()
            {
                Id = dptm.Id,
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToUInt32(groupValue.Value, 0),
                NumericInfo = nif.GetNumericInfo(dptm, out var isNumeric)
            })
        },
        */
    };

    /// <summary>
    /// Dictionary mapping PDT Names to DPT creator functions. This allows for dynamic instantiation of DPTs based on master data information.
    /// The key is typically the PDT name (e.g., "PDT_UNSIGNED_LONG", etc.) and the value is a function that creates an instance of the corresponding DPT class.
    /// </summary>
    public Dictionary<PropertyDataTypeNumber, DptCreator> DptCreatorsByPdt = new()
    {
        /*
        { PropertyDataTypeNumber.PDT_UNSIGNED_LONG, new DptCreator((dptm,nif) => new DptSimple<UInt32>()
            {
                Id = dptm.Id,
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToUInt32(groupValue.Value, 0),
                NumericInfo = nif.GetNumericInfo(dptm, out var isNumeric)
            })
        },
        */
    };
}
