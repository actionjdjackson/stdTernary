using System;
using System.Text;

namespace stdTernary;

internal static class BinaryEncoding
{
    internal const int MaxBits = 64;
    private static readonly ulong[] PowersOfTwo = CreatePowersOfTwo();

    private static ulong[] CreatePowersOfTwo()
    {
        var cache = new ulong[MaxBits];
        ulong value = 1UL;
        for (int i = 0; i < MaxBits; i++)
        {
            cache[i] = value;
            value += value;
        }

        return cache;
    }

    internal static ulong Pow2(int exponent)
    {
        if ((uint)exponent >= PowersOfTwo.Length)
            throw new ArgumentOutOfRangeException(nameof(exponent));
        return PowersOfTwo[exponent];
    }

    internal static sbyte DecodeBit(ulong packed, int index)
    {
        if ((uint)index >= MaxBits)
            throw new ArgumentOutOfRangeException(nameof(index));

        ulong divisor = Pow2(index);
        ulong quotient = packed / divisor;
        return (sbyte)(quotient % 2UL);
    }

    internal static ulong SetBit(ulong packed, int index, sbyte value)
    {
        if ((uint)index >= MaxBits)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (value is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(value), "Binary digits must be 0 or 1.");

        sbyte existing = DecodeBit(packed, index);
        if (existing == value)
            return packed;

        ulong weight = Pow2(index);
        return value == 1 ? checked(packed + weight) : checked(packed - weight);
    }

    internal static void Decode(ulong packed, Span<sbyte> digits, int count)
    {
        if ((uint)count > MaxBits)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (digits.Length < count)
            throw new ArgumentException("Destination span too small.", nameof(digits));

        for (int i = 0; i < count; i++)
        {
            digits[i] = DecodeBit(packed, i);
        }
    }

    internal static ulong Encode(ReadOnlySpan<sbyte> digits, int count)
    {
        if ((uint)count > MaxBits)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (digits.Length < count)
            throw new ArgumentException("Source span too small.", nameof(digits));

        ulong result = 0UL;
        for (int i = 0; i < count; i++)
        {
            sbyte digit = digits[i];
            if (digit is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(digits), "Binary digits must be 0 or 1.");
            if (digit == 1)
            {
                result = checked(result + Pow2(i));
            }
        }

        return result;
    }

    internal static ulong FromInt64(long value, int count)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Magnitude must be non-negative for binary encoding.");
        if ((uint)count > MaxBits)
            throw new ArgumentOutOfRangeException(nameof(count));

        Span<sbyte> digits = stackalloc sbyte[count];
        ulong remaining = (ulong)value;
        int index = 0;
        while (remaining != 0UL && index < count)
        {
            ulong quotient = remaining / 2UL;
            digits[index] = (sbyte)(remaining - quotient * 2UL);
            remaining = quotient;
            index++;
        }

        if (remaining != 0UL)
            throw new OverflowException($"Value {value} cannot be represented using {count} binary digits.");

        return Encode(digits, count);
    }

    internal static long ToInt64(ulong packed, int count)
    {
        if ((uint)count > MaxBits)
            throw new ArgumentOutOfRangeException(nameof(count));

        long value = 0L;
        for (int i = count - 1; i >= 0; i--)
        {
            value = checked(value * 2L + DecodeBit(packed, i));
        }

        return value;
    }

    internal static int Compare(ulong left, ulong right, int count)
    {
        if ((uint)count > MaxBits)
            throw new ArgumentOutOfRangeException(nameof(count));

        for (int i = count - 1; i >= 0; i--)
        {
            sbyte a = DecodeBit(left, i);
            sbyte b = DecodeBit(right, i);
            if (a != b)
                return a > b ? 1 : -1;
        }

        return 0;
    }

    internal static bool IsZero(ulong packed, int count)
    {
        if ((uint)count > MaxBits)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (packed == 0UL)
            return true;

        int highest = HighestNonZeroBit(packed, count);
        return highest < 0;
    }

    internal static int HighestNonZeroBit(ulong packed, int count)
    {
        if ((uint)count > MaxBits)
            throw new ArgumentOutOfRangeException(nameof(count));

        for (int i = count - 1; i >= 0; i--)
        {
            if (DecodeBit(packed, i) != 0)
                return i;
        }

        return -1;
    }

    internal static string ToBinaryString(ulong packed, int count)
    {
        if ((uint)count > MaxBits)
            throw new ArgumentOutOfRangeException(nameof(count));

        var builder = new StringBuilder(count);
        for (int i = count - 1; i >= 0; i--)
        {
            builder.Append(DecodeBit(packed, i) == 1 ? '1' : '0');
        }

        return builder.ToString();
    }
}
