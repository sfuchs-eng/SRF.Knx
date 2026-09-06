namespace SRF.Knx.Core.DPT;

/// <summary>
/// Base class for all DPT types.
/// It provides methods to convert between the raw group value and the typed value,
/// as well as a method to format the value for display purposes.
/// 
/// The DPT factory will return an instance of the appropriate DPT type based on the main and sub number of the DPT.
/// </summary>
public abstract class DptBase
{
    public required DataPointTypeId Id { get; init; }

    /// <summary>
    /// KNX master data for this DPT
    /// </summary>
    public required DptMetadata Metadata { get; init; }

    public abstract object ToValue(GroupValue groupValue);

    public abstract GroupValue ToGroupValue(object value);

    /// <summary>
    /// .NET type of the value represented by this DPT in KNX telegrams, e.g. bool for DPT 1.001, byte for DPT 5.001, etc.
    /// </summary>
    public abstract Type ValueType { get; }

    /// <summary>
    /// .NET type of the value used in the application for this DPT, e.g. bool for DPT 1.001, double for DPT 5.001 (scaled), etc.
    /// </summary>
    public virtual Type ApplicationType => ValueType;

    public virtual bool IsNumeric => false;

    public virtual bool IsScaledNumeric => false;

    public DptBase(DataPointTypeId id, DptMetadata dptMetadata)
    {
        Id = id;
        Metadata = dptMetadata;
    }

    public virtual string Format(
        GroupValue groupValue,
        string? language,
        IFormatProvider? formatProvider
    )
    {
        language ??= System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        formatProvider ??= System.Globalization.CultureInfo.CurrentCulture;
        var value = ToValue(groupValue);

        if (value is null)
            return string.Empty;

        string formatted = value switch
        {
            DateTime dt => dt.ToString(formatProvider),
            DateTimeOffset dto => dto.ToString(formatProvider),
            DateOnly dateOnly => dateOnly.ToString(null, formatProvider),
            TimeOnly timeOnly => timeOnly.ToString(null, formatProvider),
            TimeSpan span => span.ToString(),
            byte[] bytes => Convert.ToHexString(bytes),
            IFormattable formattable => formattable.ToString(null, formatProvider),
            _ => Convert.ToString(value, formatProvider) ?? string.Empty,
        };

        if (this is DptSimple { NumericInfo.Unit.Length: > 0 } simple && IsNumericValue(value))
            return string.Concat(formatted, " ", simple.NumericInfo!.Unit);

        return formatted;
    }

    private static bool IsNumericValue(object value)
    {
        return value is byte or sbyte
            or short or ushort
            or int or uint
            or long or ulong
            or float or double
            or decimal;
    }
}
