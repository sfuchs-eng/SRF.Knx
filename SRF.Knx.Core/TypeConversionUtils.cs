using System.Numerics;

namespace SRF.Knx.Core;

public static class TypeConversionUtils
{
    public static Ttarget ClampToRange<Tin, Ttarget>(Tin value)
        where Tin : INumber<Tin>
        where Ttarget : INumber<Ttarget>, IMinMaxValue<Ttarget>
    {
        // 1. Convert the target's Min/Max boundaries into the input type 'Tin' 
        //    so we can safely compare them without throwing an overflow.
        var targetMinAsIn = Tin.CreateSaturating(Ttarget.MinValue);
        var targetMaxAsIn = Tin.CreateSaturating(Ttarget.MaxValue);

        // 2. Perform the bounds check using the input type
        if (value <= targetMinAsIn)
            return Ttarget.MinValue;

        if (value >= targetMaxAsIn)
            return Ttarget.MaxValue;

        // 3. If it's safely in between, convert it to the target type
        return Ttarget.CreateChecked(value);
    }

    public static object ClampToRange<Tin>(Tin value, Type targetType) 
        where Tin : INumber<Tin>
    {
        if (targetType == typeof(double))
            return ClampToRange<Tin, double>(value);
        if (targetType == typeof(float))
            return ClampToRange<Tin, float>(value);
        if (targetType == typeof(decimal))
            return ClampToRange<Tin, decimal>(value);
        if (targetType == typeof(byte))
            return ClampToRange<Tin, byte>(value);
        if (targetType == typeof(sbyte))
            return ClampToRange<Tin, sbyte>(value);
        if (targetType == typeof(short))
            return ClampToRange<Tin, short>(value);
        if (targetType == typeof(ushort))
            return ClampToRange<Tin, ushort>(value);
        if (targetType == typeof(int))
            return ClampToRange<Tin, int>(value);
        if (targetType == typeof(uint))
            return ClampToRange<Tin, uint>(value);
        if (targetType == typeof(long))
            return ClampToRange<Tin, long>(value);
        if (targetType == typeof(ulong))
            return ClampToRange<Tin, ulong>(value);

        throw new NotSupportedException($"Clamping to the specified target type {targetType.Name} is not supported.");
    }
}
