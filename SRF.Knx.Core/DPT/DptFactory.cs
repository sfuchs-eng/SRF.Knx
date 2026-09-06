using Microsoft.Extensions.Logging;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Core.DPT;

/// <summary>
/// Factory class for creating DPT (Data Point Type) instances.
/// The factory uses master data to determine the appropriate DPT type to instantiate based on the provided DataPointTypeId (main and sub type).
/// The factory also uses the PDT encoder factory to get the appropriate PDT encoder for the DPT type, and the DPT numeric info factory to get the numeric information for the DPT type.
/// Two dictionaries, <see cref="DptCreatorsById"/> and <see cref="DptCreatorsByPdt"/>, are used to override the default DPT instantiation logic based on DPST ID or PDT name, allowing for flexible and dynamic DPT creation.
/// If no specific creator is found, the factory falls back to using the PDT encoder factory to create a generic DPT instance based on the available PDT encoder.
/// </summary>
/// <typeparam name="DptFactory"></typeparam>
public class DptFactory(
    IKnxMasterDataProvider masterDataProvider,
    IPdtEncoderFactory pdtEncoderFactory,
    IDptNumericInfoFactory dptNumericInfoFactory,
    ILogger<DptFactory> logger) : IDptFactory
{
    private readonly Master.KnxMasterData masterData = masterDataProvider.GetMasterData();
    private readonly IPdtEncoderFactory pdtEncoderFactory = pdtEncoderFactory;
    private readonly IDptNumericInfoFactory numericInfoFactory = dptNumericInfoFactory;
    private readonly ILogger<DptFactory> logger = logger;

    public DptBase Get(int main, int sub)
    {
        return Get(new DataPointTypeId(main, sub));
    }

    public DptBase Get(string dptId)
    {
        if ( string.IsNullOrWhiteSpace(dptId) )
            throw new ArgumentException("DPT ID cannot be null or whitespace.", nameof(dptId));

        if ( !DataPointTypeId.TryParse(dptId, out var dpstId) )
            throw new ArgumentException($"Invalid DPT ID format: {dptId}. Expected format is 'main.sub' (e.g., '1.001').", nameof(dptId));

        return Get(dpstId);
    }

    public virtual DptBase Get(DataPointTypeId dpstId)
    {
        /* not relevant
        if (dpstId.IsMainOnly)
        {
            logger.LogDebug("DPT {Main} requested with main number only (no sub-type). The main type PDT will be used for instantiation. This may lead to incorrect behavior if the assumed PDT does not match the intended sub-type.", dpstId.Main);
        }*/

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
            // PdtEncoder<T> is a class (not an interface), so walk the base type chain to find the generic PdtEncoder<T>.
            var encoderType = pdtEncoder.GetType();
            var genericPdtEncoderType = encoderType;
            while (genericPdtEncoderType != null && !(genericPdtEncoderType.IsGenericType && genericPdtEncoderType.GetGenericTypeDefinition() == typeof(PdtEncoder<>)))
                genericPdtEncoderType = genericPdtEncoderType.BaseType;
            var dptType = typeof(DptSimple<>).MakeGenericType(
                (genericPdtEncoderType ?? throw new InvalidOperationException($"PDT encoder type {encoderType} does not derive from PdtEncoder<T> for DPST {dpstId}"))
                .GetGenericArguments()[0]);
            var dpt = (DptBase)(Activator.CreateInstance(dptType, [dpstId, dptMeta, pdtEncoder, numericInfo!])
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
    /// This allows for flexible and dynamic DPT instantiation, overriding master data information.
    /// </summary>
    public Dictionary<DPT.DataPointTypeId, DptCreator> DptCreatorsById = new()
    {
        /* Example
        { new DataPointTypeId("1.001"), new DptCreator((dptm,nif) => new DptSimple<UInt32>()
            {
                Id = dptm.Id,
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToUInt32(groupValue.Value, 0),
                NumericInfo = nif.GetNumericInfo(dptm, out var isNumeric),
                Metadata = dptm
            })
        },
        */
    };

    /// <summary>
    /// Dictionary mapping PDT Names to DPT creator functions. This allows for dynamic instantiation of DPTs.
    /// The key is typically the PDT name (e.g., "PDT_UNSIGNED_LONG", etc.) and the value is a function that creates an instance of the corresponding DPT class.
    /// Allows to override the master data based DPT instantiation with custom DPT creators.
    /// </summary>
    public Dictionary<PropertyDataTypeNumber, DptCreator> DptCreatorsByPdt = new()
    {
        /* Example:
        { PropertyDataTypeNumber.PDT_UNSIGNED_LONG, new DptCreator((dptm,nif) => new DptSimple<UInt32>()
            {
                Id = dptm.Id,
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToUInt32(groupValue.Value, 0),
                NumericInfo = nif.GetNumericInfo(dptm, out var isNumeric),
                Metadata = dptm
            })
        },
        */
    };
}
