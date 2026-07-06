using System;
using System.Runtime.CompilerServices;

namespace stdTernary;

internal static class UnbalancedTernaryEncoding
{
    internal const int MaxTrits = 32;
    private const ulong TritMask = 0b11UL;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static sbyte DecodeTrit(ulong packed, int index)
    {
        if ((uint)index >= MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(index));

        ulong bits = (packed >> (index * 2)) & TritMask;
        return bits switch
        {
            <= 2UL => (sbyte)bits,
            _ => throw new InvalidOperationException("Invalid 2-bit unbalanced trit encoding."),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong SetTrit(ulong packed, int index, sbyte value)
    {
        if ((uint)index >= MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (value is < 0 or > 2)
            throw new ArgumentOutOfRangeException(nameof(value), "Unbalanced ternary trits must be 0, 1, or 2.");

        ulong mask = ~(TritMask << (index * 2));
        return (packed & mask) | ((ulong)value << (index * 2));
    }

    internal static void Decode(ulong packed, Span<sbyte> digits, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (digits.Length < count)
            throw new ArgumentException("Destination span too small.", nameof(digits));

        for (int i = 0; i < count; i++)
        {
            digits[i] = DecodeTrit(packed, i);
        }
    }

    internal static ulong Encode(ReadOnlySpan<sbyte> digits, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (digits.Length < count)
            throw new ArgumentException("Source span too small.", nameof(digits));

        ulong packed = 0UL;
        for (int i = 0; i < count; i++)
        {
            packed = SetTrit(packed, i, digits[i]);
        }

        return packed;
    }

    internal static ulong FromUInt64(ulong value, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));

        Span<sbyte> digits = stackalloc sbyte[count];
        ulong remaining = value;
        int index = 0;

        while (remaining != 0 && index < count)
        {
            digits[index++] = (sbyte)(remaining % 3UL);
            remaining /= 3UL;
        }

        if (remaining != 0)
            throw new OverflowException($"Value {value} cannot be represented using {count} unbalanced ternary trits.");

        return Encode(digits, count);
    }

    internal static ulong ToUInt64(ulong packed, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));

        ulong value = 0UL;
        for (int i = count - 1; i >= 0; i--)
        {
            value = checked(value * 3UL + (ulong)DecodeTrit(packed, i));
        }

        return value;
    }

    internal static int Compare(ulong left, ulong right, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));

        for (int i = count - 1; i >= 0; i--)
        {
            sbyte a = DecodeTrit(left, i);
            sbyte b = DecodeTrit(right, i);
            if (a != b)
                return a > b ? 1 : -1;
        }

        return 0;
    }

    internal static bool IsZero(ulong packed, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));

        ulong mask = count >= 32 ? ulong.MaxValue : (1UL << (count * 2)) - 1UL;
        return (packed & mask) == 0UL;
    }

    internal static int HighestNonZeroTrit(ulong packed, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));

        for (int i = count - 1; i >= 0; i--)
        {
            if (DecodeTrit(packed, i) != 0)
                return i;
        }

        return -1;
    }

    internal static string ToTernaryString(ulong packed, int count)
    {
        Span<char> buffer = stackalloc char[count];
        for (int i = count - 1, j = 0; i >= 0; i--, j++)
        {
            buffer[j] = (char)('0' + DecodeTrit(packed, i));
        }

        return new string(buffer);
    }

    internal static ulong ToBalancedPacked(ulong packed, int count)
    {
        long value = checked((long)ToUInt64(packed, count));
        return BalancedTernaryEncoding.FromInt64(value, count);
    }

    internal static ulong FromBalancedPacked(ulong packed, int count)
    {
        long value = BalancedTernaryEncoding.ToInt64(packed, count);
        if (value < 0)
            throw new OverflowException("Negative balanced ternary values cannot be converted to unbalanced ternary.");
        return FromUInt64((ulong)value, count);
    }
}
