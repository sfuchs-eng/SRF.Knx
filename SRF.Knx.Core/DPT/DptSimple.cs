namespace SRF.Knx.Core.DPT;

public abstract class DptSimple : DptBase
{
    public override bool IsNumeric { get => NumericInfo != null; }

    public override bool IsScaledNumeric { get => NumericInfo?.IsScaled == true; }

    public NumericInfo? NumericInfo { get; init; }
}

public class DptSimple<T> : DptSimple, IDptEncoder<T>
{
    public required Func<T, GroupValue> Encoder { get; init; }
    public required Func<GroupValue, T> Decoder { get; init; }

    public override Type ValueType => typeof(T);

    public override Type ApplicationType => IsScaledNumeric && typeof(T) != typeof(decimal) ? typeof(double) : typeof(T);

    public T Decode(GroupValue groupValue) => Decoder(groupValue);

    public GroupValue Encode(T value) => Encoder(value);

    public override GroupValue ToGroupValue(object value)
    {
        if (IsScaledNumeric)
        {
            if (value is not IConvertible valueConvertible)
            {
                throw new InvalidOperationException($"DPT {Id} is defined as scaled numeric, but the provided value is not IConvertible, which is required to apply the coefficient for scaling. Actual type of the provided value is {value?.GetType().Name ?? "null"}");
            }
            // If a coefficient is defined and the value is numeric, apply the coefficient before encoding
            double doubleValue = valueConvertible.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
            doubleValue /= NumericInfo?.Coefficient ?? 1.0;
            // ensure a round trip safe converstion by appropriate rounding, e.g. fom 0...100% double to byte for DPT 5.001, where the coefficient is 100/255, so we round to the nearest multiple of 100/255 to avoid ending up with a value that is outside of the valid range for the DPT after encoding and decoding again.
            // make it generic though, such that e.g. 0..360°deg mapped to 0..255 for DPT 5.004 with a coefficient of 360/255 also gets rounded to the nearest multiple of 360/255 to ensure a round trip safe conversion.
            // We do this anyhow even though only needed for scaled DPTs with a numeric type that has a smaller range than double.
            // But floating point targets are not common for scaled DPTs, and if they are used, they likely have a coefficient of 1.0, so the rounding will not have any effect in that case, but it will also not cause any harm, so we can just apply this rounding for all scaled numerics for simplicity.
            double step = NumericInfo?.Coefficient ?? 1.0;
            if ( !1.0.Equals(step))
            {
                doubleValue = Math.Round(doubleValue / step) * step + 0.5;
            }
            object scaledValue = TypeConversionUtils.ClampToRange(doubleValue, typeof(T));
            return Encode((T)scaledValue);
        }
        else
        {
            return Encode((T)value); //TODO: DPST-5-4 must be native double instead of byte or int32 - fix it.
        }
    }

    public override object ToValue(GroupValue groupValue)
    {
        var value = Decode(groupValue);
        if (IsScaledNumeric)
        {
            if (value is not IConvertible valueConvertible)
            {
                throw new InvalidOperationException($"DPT {Id} is defined as scaled numeric, but the decoded value is not IConvertible, which is required to apply the coefficient for scaling. Actual type of the decoded value is {value?.GetType().Name ?? "null"}");
            }
            // If a coefficient is defined and the value is numeric, apply the coefficient after decoding
            double doubleValue = valueConvertible.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
            doubleValue *= NumericInfo?.Coefficient ?? 1.0;
            var returnType = typeof(T) switch
            {
                Type t when t == typeof(decimal) => typeof(decimal),
                _ => typeof(double)
            };
            try
            {
                return Convert.ChangeType(doubleValue, returnType, System.Globalization.CultureInfo.InvariantCulture) ?? throw new InvalidOperationException($"Failed to convert scaled value of DPT {Id} to type {returnType.Name}, while group address native is type {typeof(T).Name}");
            }
            catch (Exception)
            {
            }
            return TypeConversionUtils.ClampToRange(doubleValue, returnType);
        }
        return value ?? throw new InvalidOperationException($"got null value for DPT {Id}");
    }

    public DptSimple() : base()
    {
    }

    public DptSimple(DataPointTypeId id, PdtEncoder<T> encoder, NumericInfo? numericInfo = null)
    {
        Id = id;
        Encoder = encoder.Encoder;
        Decoder = encoder.Decoder;
        NumericInfo = numericInfo;
    }
}
