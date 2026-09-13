using SRF.Knx.Core.Master;

namespace SRF.Knx.Core.DPT;

/// <summary>
/// <para>Base class for all DPT types.</para>
/// <para>It provides methods to convert between the raw group value and the typed value,
/// as well as a method to format the value for display purposes.</para>
/// <para>The DPT factory will return an instance of the appropriate DPT or DPST type based on the main and sub number of the DPT/DPST.</para>
/// 
/// </summary>
public abstract class DptBase(DataPointTypeId id, DptMetadata dptMetadata)
{
    public required DataPointTypeId Id { get; init; } = id;

    /// <summary>
    /// KNX master data for this DPT
    /// </summary>
    public required DptMetadata Metadata { get; init; } = dptMetadata;

    public DatapointSubtype? MetadataDpst { get => Metadata.Dpst; }

    public PropertyDataType MetadataPdt { get => Metadata.Pdt; }

    /// <summary>
    /// Converts the given group value (bus native format) to the corresponding typed value for this DPT.
    /// </summary>
    /// <param name="groupValue"></param>
    /// <returns></returns>
    public abstract object ToValue(GroupValue groupValue);

    /// <summary>
    /// Converts the given typed value to the corresponding group value (bus native format) for this DPT.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public abstract GroupValue ToGroupValue(object value);

    /// <summary>
    /// .NET type of the value represented by this DPT in KNX telegrams, e.g. bool for DPT 1.001, byte for DPT 5.001, etc.
    /// It's the type according KNX masterdata for this DPT, and may differ from the <see cref="ApplicationType"/> used in the application.<br/>
    /// It should be the type returned by <see cref="ToValue(GroupValue)"/>.
    /// </summary>
    public abstract Type ValueType { get; }

    /// <summary>
    /// .NET type of the value used in the application for this DPT, e.g. bool for DPT 1.001, double for DPT 5.001 (scaled), etc.<br/>
    /// By default it returns the same type as <see cref="ValueType"/>, but can be overridden in derived classes to provide a different type for application use.<br/>
    /// In particular, DptSimple may return a scaled numeric type (e.g. double) for application use or a different type for unit-aware DPTs (e.g. UnitsNet.Temperature).
    /// </summary>
    public virtual Type ApplicationType => ValueType;

    public virtual bool IsNumeric => false;

    public virtual bool IsScaledNumeric => false;

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
