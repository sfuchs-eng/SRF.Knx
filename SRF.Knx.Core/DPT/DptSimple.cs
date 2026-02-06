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

    public T Decode(GroupValue groupValue) => Decoder(groupValue);

    public GroupValue Encode(T value) => Encoder(value);

    public override GroupValue ToGroupValue(object value) => Encode((T)value);

    public override object ToValue(GroupValue groupValue)
        => Decode(groupValue)
        ?? throw new InvalidOperationException($"got null value for DPT {Id}");
}
