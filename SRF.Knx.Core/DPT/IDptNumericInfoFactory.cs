namespace SRF.Knx.Core.DPT;

public interface IDptNumericInfoFactory
{
    NumericInfo? GetNumericInfo(DptMetadata dptMeta, out bool isNumeric);
}
// PDT Names from KNX Data Types and Data Point Types - ETS6.1.5a2 Master Data:
