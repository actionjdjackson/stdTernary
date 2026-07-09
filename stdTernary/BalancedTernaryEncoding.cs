using System;
using System.Runtime.CompilerServices;

namespace stdTernary;

internal static class BalancedTernaryEncoding
{
    internal const int MaxTrits = 32;
    private const ulong TritMask = 0b11UL;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static sbyte DecodeTrit(ulong packed, int index)
    {
        if ((uint)index >= MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(index));

        // Layout: 00->0, 01->+1, 10->-1. Decode branchlessly with
        // trit = bits - 3*(bits>>1)  (0->0, 1->1, 2->-1). This avoids the per-trit
        // value-translating switch that made balanced decode ~1.6x slower than
        // unbalanced (whose digit == stored value, so decode is a bare cast).
        ulong bits = (packed >> (index * 2)) & TritMask;
        return (sbyte)((long)bits - 3L * (long)(bits >> 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong SetTrit(ulong packed, int index, sbyte value)
    {
        if ((uint)index >= MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (value is < -1 or > 1)
            throw new ArgumentOutOfRangeException(nameof(value), "Balanced ternary trits must be -1, 0, or 1.");

        ulong mask = ~(TritMask << (index * 2));
        // Inverse of DecodeTrit: {-1,0,+1} -> {10,00,01}. Branchless:
        // code = value + (3 & (value>>31))  maps {-1,0,1} -> {2,0,1}.
        int v = value;
        ulong encoded = (ulong)(v + (3 & (v >> 31)));

        return (packed & mask) | (encoded << (index * 2));
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

    internal static ulong FromInt64(long value, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));

        Span<sbyte> digits = stackalloc sbyte[count];
        // Sign-agnostic single pass: convert the signed value directly to balanced
        // trits. C#'s % keeps the dividend's sign, so the remainder lands in [-2, 2];
        // fold 2 -> -1 (carry +1) and -2 -> +1 (borrow -1). This avoids Math.Abs and,
        // crucially, the separate full-width negation loop the old code ran for every
        // negative input (~half of them), which was the residual construction cost.
        long remaining = value;
        int index = 0;

        while (remaining != 0 && index < count)
        {
            int rem = (int)(remaining % 3);
            remaining /= 3;
            if (rem > 1) { rem -= 3; remaining += 1; }
            else if (rem < -1) { rem += 3; remaining -= 1; }
            digits[index++] = (sbyte)rem;
        }

        if (remaining != 0)
            throw new OverflowException($"Value {value} cannot be represented using {count} balanced ternary trits.");

        return Encode(digits, count);
    }

    internal static long ToInt64(ulong packed, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));

        long value = 0;
        for (int i = count - 1; i >= 0; i--)
        {
            value = checked(value * 3 + DecodeTrit(packed, i));
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

        ulong mask = (count >= 32) ? ulong.MaxValue : (1UL << (count * 2)) - 1UL;
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
            buffer[j] = DecodeTrit(packed, i) switch
            {
                -1 => '-',
                0 => '0',
                1 => '+',
                _ => '?'
            };
        }

        return new string(buffer);
    }
}
