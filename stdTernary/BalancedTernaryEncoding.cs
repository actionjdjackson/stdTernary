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

        ulong bits = (packed >> (index * 2)) & TritMask;
        return bits switch
        {
            0b00 => 0,
            0b01 => 1,
            0b10 => -1,
            _ => throw new InvalidOperationException("Invalid 2-bit trit encoding."),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong SetTrit(ulong packed, int index, sbyte value)
    {
        if ((uint)index >= MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (value is < -1 or > 1)
            throw new ArgumentOutOfRangeException(nameof(value), "Balanced ternary trits must be -1, 0, or 1.");

        ulong mask = ~(TritMask << (index * 2));
        ulong encoded = value switch
        {
            -1 => 0b10UL,
            0 => 0b00UL,
            1 => 0b01UL,
            _ => 0UL,
        };

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
        long remaining = Math.Abs(value);
        int index = 0;

        while (remaining != 0 && index < count)
        {
            long remainder = remaining % 3;
            remaining /= 3;

            switch (remainder)
            {
                case 0:
                    digits[index] = 0;
                    break;
                case 1:
                    digits[index] = 1;
                    break;
                case 2:
                    digits[index] = -1;
                    remaining += 1;
                    break;
            }

            index++;
        }

        if (remaining != 0)
            throw new OverflowException($"Value {value} cannot be represented using {count} balanced ternary trits.");

        if (value < 0)
        {
            for (int i = 0; i < count; i++)
            {
                digits[i] = (sbyte)(-digits[i]);
            }
        }

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
