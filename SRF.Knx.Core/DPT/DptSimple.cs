namespace SRF.Knx.Core.DPT;

public abstract class DptSimple : DptBase
{
    public bool IsNumeric { get => NumericInfo != null; }

    public NumericInfo? NumericInfo { get; init; }
}

public class DptSimple<T> : DptSimple, IDptEncoder<T>
{
    public required Func<T, GroupValue> Encoder { get; init; }
    public required Func<GroupValue, T> Decoder { get; init; }

    public override Type ValueType => typeof(T);

    public T Decode(GroupValue groupValue) => Decoder(groupValue);

    public GroupValue Encode(T value) => Encoder(value);

    public override GroupValue ToGroupValue(object value)
    {
        if ( NumericInfo?.Coefficient != 1.0 && value is IConvertible convertibleValue )
        {
            // If a coefficient is defined and the value is numeric, apply the coefficient before encoding
            double doubleValue = convertibleValue.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
            doubleValue /= NumericInfo?.Coefficient ?? 1.0;
            object scaledValue = Convert.ChangeType(doubleValue, typeof(T), System.Globalization.CultureInfo.InvariantCulture) ?? throw new InvalidOperationException($"Failed to convert scaled value of DPT {Id} back to type {typeof(T).Name}");
            return Encode((T)scaledValue);
        }
        else
        {
            return Encode((T)value);
        }
    }

    public override object ToValue(GroupValue groupValue)
    {
        var value = Decode(groupValue);
        if ( NumericInfo?.Coefficient != 1.0 && value is IConvertible convertibleValue )
        {
            // If a coefficient is defined and the value is numeric, apply the coefficient after decoding
            double doubleValue = convertibleValue.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
            doubleValue *= NumericInfo?.Coefficient ?? 1.0;
            var returnType = typeof(T) switch 
            {
                Type t when t == typeof(decimal) => typeof(decimal),
                _ => typeof(double)
            };
            return Convert.ChangeType(doubleValue, returnType, System.Globalization.CultureInfo.InvariantCulture) ?? throw new InvalidOperationException($"Failed to convert scaled value of DPT {Id} to type {returnType.Name}, while group address native is type {typeof(T).Name}");
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
