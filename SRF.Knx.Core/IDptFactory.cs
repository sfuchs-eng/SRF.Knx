using SRF.Knx.Core.DPT;

namespace SRF.Knx.Core;

/// <summary>
/// Provides the DPT object for a given DPT main and sub number.
/// The DPT factory uses the master data to determine the appropriate DPT type and its properties
/// and uses the PDT encoder factory to get the appropriate PDT encoder for the DPT type.
/// The DPT factory also uses the DPT numeric info factory to get the numeric information for
/// the DPT type, such as the range, resolution, unit, etc., which can be used by the DPT types to validate and format the values.
/// </summary>
public interface IDptFactory
{
    DptBase Get(int main, int sub);
}
