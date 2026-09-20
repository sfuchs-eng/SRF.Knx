namespace SRF.Knx.Core.DPT;

/// <summary>
/// A DPT (Data Point Type) that is aware of UnitsNet units and can convert between KNX group values and UnitsNet quantities.
/// </summary>
/// <typeparam name="TEncoder">The type used for encoding/decoding the KNX group value.</typeparam>
/// <typeparam name="TApp">The type of the application-level UnitsNet quantity.</typeparam>
/// <typeparam name="TUnit">The UnitsNet Unit enum type of the KNX unit associated with this DPST.</typeparam>
public class DptSimpleQuantity<TEncoder, TApp, TUnit>(DataPointTypeId id, DptMetadata dptMetadata, PdtEncoder<TEncoder> encoder, TUnit knxUnit, NumericInfo? numericInfo = null)
    : DptSimple<TEncoder>(id, dptMetadata, encoder, numericInfo) where TApp : UnitsNet.IQuantity where TUnit : Enum
{
    public override Type ApplicationType => typeof(TApp);

    /// <summary>
    /// The KNX unit associated with this DPT, e.g. UnitsNet.Units.TemperatureUnit.DegreeCelsius for DPT 9.001 (Temperature), etc.
    /// The unit results after applying the coefficient for scaled numerics, e.g. for DPT 5.001 (Percentage), the KNX unit is UnitsNet.Units.RatioUnit.Percent, which is the unit after applying the coefficient of 100/255 to the raw value in the telegram.
    /// </summary>
    /// <value></value>
    public TUnit KnxUnit { get; init; } = knxUnit;

    public bool TryToValue(GroupValue groupValue, out TApp? quantity)
    {
        try
        {
            var value = ToValue(groupValue);
            if (value is TApp typedQuantity)
            {
                quantity = typedQuantity;
                return true;
            }
            else
            {
                quantity = default;
                return false;
            }
        }
        catch
        {
            quantity = default;
            return false;
        }
    }

    public override object ToValue(GroupValue groupValue)
    {
        var value = base.ToValue(groupValue);
        if (value is not IConvertible valueConvertible)
        {
            throw new InvalidOperationException($"DPT {Id} is defined as unit-aware numeric, but the decoded value is not IConvertible, which is required to create a UnitsNet quantity. Actual type of the decoded value is {value?.GetType().Name ?? "null"}");
        }
        double doubleValue = valueConvertible.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
        var quantity = UnitsNet.Quantity.From(doubleValue, KnxUnit);
        return quantity;
    }

    public override GroupValue ToGroupValue(object value)
    {
        if (value is not TApp quantity)
        {
            throw new InvalidOperationException($"DPT {Id} is defined as unit-aware numeric, but the provided value is not of the expected type {typeof(TApp).Name}. Actual type of the provided value is {value?.GetType().Name ?? "null"}");
        }
        return ToGroupValue(quantity);
    }

    public GroupValue ToGroupValue(TApp value)
    {
        var doubleValue = value.As(KnxUnit);
        doubleValue /= NumericInfo?.Coefficient ?? 1.0;

        // if the T target type is not a floating point type, we need to apply round trip safe rounding to ensure that the value can be encoded and decoded without losing precision or going out of range.
        if (typeof(TEncoder) != typeof(double) && typeof(TEncoder) != typeof(decimal) && typeof(TEncoder) != typeof(float))
        {
            doubleValue = Math.Round(doubleValue);
        }

        object scaledValue = TypeConversionUtils.ClampToRange(doubleValue, typeof(TEncoder));
        return Encode((TEncoder)scaledValue);
    }

    public override string Format(GroupValue groupValue, string? language, IFormatProvider? formatProvider, string? format = null)
    {
        if ( !TryToValue(groupValue, out TApp? quantity) || quantity is null)
        {
            return string.Empty;
        }
        return quantity.ToUnit(KnxUnit).ToString(format ?? "S1", formatProvider ?? System.Globalization.CultureInfo.CurrentCulture);
    }
}
