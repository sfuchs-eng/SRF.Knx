namespace SRF.Knx.Core.Master;

/// <summary>
/// This is a dummy which shall be removed.
/// </summary>
[Obsolete("This is a dummy which shall be removed.")]
public record Ref<T>(string RefId)
{
    public T? Value { get; set; }
}