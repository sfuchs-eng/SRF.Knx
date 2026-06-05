using Microsoft.Extensions.Logging;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Core.DPT;

public class DptNumericInfoFactory(ILogger<DptNumericInfoFactory> logger) : IDptNumericInfoFactory
{
    public NumericInfo? GetNumericInfo(DptMetadata dptMeta, out bool isNumeric)
    {
        if (dptMeta.Dpst is null)
        {
            // Main-only DPT (no sub-type): no format information available.
            isNumeric = false;
            return null;
        }

        if ( dptMeta.Dpst.Format == null ||
             dptMeta.Dpst.Format.Elements == null )
        {
            throw new ArgumentException($"DPST {dptMeta.Dpst.Id} has no format information in master data, which is required to determine numeric information for DPT creation.");
        }

        if (dptMeta.Dpst.Format?.Elements.Count == 0)
        {
            throw new ArgumentException($"DPST {dptMeta.Dpst.Id} has no format elements defined in master data, which is required to determine numeric information for DPT creation.");
        }

        if (dptMeta.Dpst.Format?.Elements.Count > 1)
        {
            //throw new ArgumentException($"DPST {dptMeta.Dpst.Id} has multiple format elements defined in master data, which is currently not supported for determining numeric information for DPT creation.");
            //TODO: how to support complex aggregate datapointtypes?
            isNumeric = false;
            return null;
        }

        var formatElement = dptMeta.Dpst.Format?.Elements.FirstOrDefault()!;
        isNumeric = formatElement is NumericFormat;
        if (formatElement is not NumericFormat numericFormat)
        {
            return null;
        }

        double maxValue = double.MaxValue;
        double minValue = double.MinValue;
        if (formatElement is IntegralNumericFormat integralFormat)
        {
            maxValue = integralFormat.MaxInclusive;
            minValue = integralFormat.MinInclusive;
        }
        else if (formatElement is DecimalNumericFormat decimalFormat)
        {
            if (!string.IsNullOrEmpty(decimalFormat.MaxValue) && !double.TryParse(decimalFormat.MaxValue, out maxValue))
            {
                throw new ArgumentException($"Invalid MaxValue '{decimalFormat.MaxValue}' in format element of DPST {dptMeta.Dpst.Id} in master data. Expected a numeric value.");
            }

            if (!string.IsNullOrEmpty(decimalFormat.MinValue) && !double.TryParse(decimalFormat.MinValue, out minValue))
            {
                throw new ArgumentException($"Invalid MinValue '{decimalFormat.MinValue}' in format element of DPST {dptMeta.Dpst.Id} in master data. Expected a numeric value.");
            }
        }

        var numericInfo = new NumericInfo
        {
            Type = typeof(object), //TODO: determine actual numeric type based on format element (e.g. int, double, etc.)
            Unit = numericFormat.UnitSpecified ? numericFormat.Unit : string.Empty,
            MaxValue = maxValue,
            MinValue = minValue,
            Coefficient = numericFormat.CoefficientSpecified ? numericFormat.Coefficient : (double?)null
        };

        return numericInfo;
    }
    private readonly ILogger<DptNumericInfoFactory> logger = logger;
}
