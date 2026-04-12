namespace SRF.Knx.Core.DPT;

/// <summary>
/// Converts between a typed value and a raw group value (byte[] <see cref="GroupValue.Value"/>) for a given property data type (PDT).
/// </summary>
public class PdtEncoder
{
    public virtual Type Type { get; set; } = typeof(object);
    public virtual Func<object, GroupValue>? OEncoder { get; set; }
    public virtual Func<GroupValue, object>? ODecoder { get; set; }
}

public class PdtEncoder<T> : PdtEncoder
{
    override public Type Type { get; set; } = typeof(T);
    public required Func<T, GroupValue> Encoder { get; init; }
    public required Func<GroupValue, T> Decoder { get; init; }

    override public Func<object, GroupValue>? OEncoder
        => value => Encoder((T)value);
    override public Func<GroupValue, object>? ODecoder
        => groupValue => Decoder(groupValue) ?? throw new InvalidOperationException($"got null value for PDT {Type.Name}");
}
