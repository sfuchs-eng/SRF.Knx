using SRF.Knx.Core.Master;

namespace SRF.Knx.Core.DPT;

public interface IPdtEncoderFactory
{
    PdtEncoder GetPdtEncoder(PropertyDataType pdt);
}

public class PdtEncoderFactory : IPdtEncoderFactory
{
    public PdtEncoder GetPdtEncoder(PropertyDataType pdt)
    {
        throw new NotImplementedException();
    }

    public Dictionary<PropertyDataTypeNumber, PdtEncoder> PdtEncodersByNumber = new()
    {
        { PropertyDataTypeNumber.PDT_UNSIGNED_LONG, new PdtEncoder<uint>
            {
                Encoder = value => new GroupValue(BitConverter.GetBytes(value)),
                Decoder = groupValue => BitConverter.ToUInt32(groupValue.Value, 0)
            }
        }
    };
}
