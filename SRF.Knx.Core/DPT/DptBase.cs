using System;

namespace SRF.Knx.Core.DPT;

public abstract class DptBase
{
    public required DataPointTypeId Id { get; init; }

    public abstract object ToValue(GroupValue groupValue);

    public abstract GroupValue ToGroupValue(object value);

    public virtual string Format(
        GroupValue groupValue,
        string? language,
        IFormatProvider? formatProvider
    )
    {
        language ??= System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        formatProvider ??= System.Globalization.CultureInfo.CurrentCulture;
        var value = ToValue(groupValue);
        //TODO: use the knx master data to format the value according to the language and format provider,
        //including enum values, date and time formats, units, etc.
        return Convert.ToString(value, formatProvider) ?? string.Empty;
    }
}
