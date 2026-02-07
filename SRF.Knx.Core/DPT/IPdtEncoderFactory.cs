using SRF.Knx.Core.Master;

namespace SRF.Knx.Core.DPT;

public interface IPdtEncoderFactory
{
    PdtEncoder GetPdtEncoder(PropertyDataType pdt);
}
