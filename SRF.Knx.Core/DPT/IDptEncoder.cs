namespace SRF.Knx.Core.DPT;

internal interface IDptEncoder<T>
{
    GroupValue Encode(T value);
    T Decode(GroupValue groupValue);
}