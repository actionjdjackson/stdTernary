using System;
using System.Diagnostics.CodeAnalysis;

namespace stdTernary;

public readonly struct IntT : IEquatable<IntT>, IComparable<IntT>
{
    public const int TritCount = 32;
    private static readonly long MaxMagnitudeLong = ComputeMaxMagnitude(TritCount);

    private readonly ulong _packed;

    internal ulong Packed => _packed;

    private IntT(ulong packed)
    {
        _packed = packed;
    }

    public IntT(long value)
    {
        _packed = BalancedTernaryEncoding.FromInt64(value, TritCount);
    }

    public static IntT Zero { get; } = new IntT(0L);
    public static IntT One { get; } = new IntT(1L);
    public static IntT NegativeOne { get; } = new IntT(-1L);
    public static IntT MaxValue { get; } = new IntT(MaxMagnitudeLong);
    public static IntT MinValue { get; } = new IntT(-MaxMagnitudeLong);

    public long ToInt64() => BalancedTernaryEncoding.ToInt64(_packed, TritCount);

    public int Sign => Math.Sign(BalancedTernaryEncoding.Compare(_packed, Zero._packed, TritCount));

    public string TernaryString => BalancedTernaryEncoding.ToTernaryString(_packed, TritCount);

    public override string ToString() => $"{TernaryString} (base3) = {ToInt64()}";

    public static IntT Parse(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        value = value.Trim();
        if (value.Length == 0)
            throw new FormatException("Empty ternary string.");
        if (value.Length > TritCount)
            throw new FormatException($"Ternary string longer than {TritCount} trits.");

        Span<sbyte> digits = stackalloc sbyte[TritCount];
        int index = 0;
        for (int i = value.Length - 1; i >= 0; i--)
        {
            digits[index++] = value[i] switch
            {
                '+' => 1,
                '0' => 0,
                '-' => -1,
                _ => throw new FormatException($"Invalid ternary digit '{value[i]}'."),
            };
        }

        return new IntT(BalancedTernaryEncoding.Encode(digits, TritCount));
    }

    public static bool TryParse(string? value, out IntT result)
    {
        try
        {
            result = Parse(value ?? throw new ArgumentNullException(nameof(value)));
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public bool Equals(IntT other) => _packed == other._packed;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is IntT other && Equals(other);

    public override int GetHashCode() => _packed.GetHashCode();

    public int CompareTo(IntT other) => BalancedTernaryEncoding.Compare(_packed, other._packed, TritCount);

    public static bool operator ==(IntT left, IntT right) => left.Equals(right);

    public static bool operator !=(IntT left, IntT right) => !left.Equals(right);

    public static bool operator <(IntT left, IntT right) => left.CompareTo(right) < 0;

    public static bool operator <=(IntT left, IntT right) => left.CompareTo(right) <= 0;

    public static bool operator >(IntT left, IntT right) => left.CompareTo(right) > 0;

    public static bool operator >=(IntT left, IntT right) => left.CompareTo(right) >= 0;

    public static IntT operator +(IntT value) => value;

    public static IntT operator -(IntT value) => new IntT(NegatePacked(value._packed));

    public static IntT operator +(IntT left, IntT right) => new IntT(AddPacked(left._packed, right._packed));

    public static IntT operator -(IntT left, IntT right) => new IntT(SubtractPacked(left._packed, right._packed));

    public static IntT operator *(IntT left, IntT right) => new IntT(MultiplyPacked(left._packed, right._packed));

    public static IntT operator /(IntT left, IntT right)
    {
        var (quotient, _) = DivRemPacked(left._packed, right._packed);
        return new IntT(quotient);
    }

    public static IntT operator %(IntT left, IntT right)
    {
        var (_, remainder) = DivRemPacked(left._packed, right._packed);
        return new IntT(remainder);
    }

    public static IntT operator &(IntT left, IntT right) => new IntT(AndPacked(left._packed, right._packed));

    public static IntT operator |(IntT left, IntT right) => new IntT(OrPacked(left._packed, right._packed));

    public static IntT operator ^(IntT left, IntT right) => new IntT(XorPacked(left._packed, right._packed));

    public static IntT operator ~(IntT value) => new IntT(NegatePacked(value._packed));

    public static IntT operator <<(IntT value, int count) => new IntT(ShiftLeftPacked(value._packed, count));

    public static IntT operator >>(IntT value, int count) => new IntT(ShiftRightPacked(value._packed, count));

    public static implicit operator IntT(long value) => new IntT(value);

    public static explicit operator long(IntT value) => value.ToInt64();

    public IntT Abs() => Sign >= 0 ? this : -this;

    private static ulong AddPacked(ulong left, ulong right)
    {
        Span<int> digits = stackalloc int[TritCount + 1];
        for (int i = 0; i < TritCount; i++)
        {
            digits[i] = BalancedTernaryEncoding.DecodeTrit(left, i) + BalancedTernaryEncoding.DecodeTrit(right, i);
        }

        return NormalizeAndPack(digits);
    }

    private static ulong SubtractPacked(ulong left, ulong right) => AddPacked(left, NegatePacked(right));

    private static ulong MultiplyPacked(ulong left, ulong right)
    {
        Span<int> digits = stackalloc int[TritCount * 2];

        for (int i = 0; i < TritCount; i++)
        {
            int b = BalancedTernaryEncoding.DecodeTrit(right, i);
            if (b == 0)
                continue;

            for (int j = 0; j < TritCount; j++)
            {
                int a = BalancedTernaryEncoding.DecodeTrit(left, j);
                if (a == 0)
                    continue;

                digits[i + j] += a * b;
            }
        }

        return NormalizeAndPack(digits);
    }

    private static ulong ShiftLeftPacked(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftRightPacked(packed, -count);
        if (count >= TritCount)
        {
            if (!BalancedTernaryEncoding.IsZero(packed, TritCount))
                throw new OverflowException("Shift left overflow.");
            return 0UL;
        }

        Span<sbyte> digits = stackalloc sbyte[TritCount];
        BalancedTernaryEncoding.Decode(packed, digits, TritCount);

        for (int i = TritCount - count; i < TritCount; i++)
        {
            if (digits[i] != 0)
                throw new OverflowException("Shift left overflow.");
        }

        for (int i = TritCount - 1; i >= count; i--)
        {
            digits[i] = digits[i - count];
        }

        for (int i = 0; i < count; i++)
        {
            digits[i] = 0;
        }

        return BalancedTernaryEncoding.Encode(digits, TritCount);
    }

    private static ulong ShiftRightPacked(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftLeftPacked(packed, -count);
        if (count >= TritCount)
            return 0UL;

        Span<sbyte> digits = stackalloc sbyte[TritCount];
        BalancedTernaryEncoding.Decode(packed, digits, TritCount);

        for (int i = 0; i < TritCount - count; i++)
        {
            digits[i] = digits[i + count];
        }

        for (int i = TritCount - count; i < TritCount; i++)
        {
            digits[i] = 0;
        }

        return BalancedTernaryEncoding.Encode(digits, TritCount);
    }

    private static ulong AndPacked(ulong left, ulong right)
    {
        Span<sbyte> a = stackalloc sbyte[TritCount];
        Span<sbyte> b = stackalloc sbyte[TritCount];
        BalancedTernaryEncoding.Decode(left, a, TritCount);
        BalancedTernaryEncoding.Decode(right, b, TritCount);

        for (int i = 0; i < TritCount; i++)
        {
            a[i] = (sbyte)Math.Min(a[i], b[i]);
        }

        return BalancedTernaryEncoding.Encode(a, TritCount);
    }

    private static ulong OrPacked(ulong left, ulong right)
    {
        Span<sbyte> a = stackalloc sbyte[TritCount];
        Span<sbyte> b = stackalloc sbyte[TritCount];
        BalancedTernaryEncoding.Decode(left, a, TritCount);
        BalancedTernaryEncoding.Decode(right, b, TritCount);

        for (int i = 0; i < TritCount; i++)
        {
            a[i] = (sbyte)Math.Max(a[i], b[i]);
        }

        return BalancedTernaryEncoding.Encode(a, TritCount);
    }

    private static ulong XorPacked(ulong left, ulong right)
    {
        Span<sbyte> a = stackalloc sbyte[TritCount];
        Span<sbyte> b = stackalloc sbyte[TritCount];
        BalancedTernaryEncoding.Decode(left, a, TritCount);
        BalancedTernaryEncoding.Decode(right, b, TritCount);

        for (int i = 0; i < TritCount; i++)
        {
            if (a[i] == 0 || b[i] == 0)
            {
                a[i] = 0;
            }
            else if (a[i] == b[i])
            {
                a[i] = -1;
            }
            else
            {
                a[i] = 1;
            }
        }

        return BalancedTernaryEncoding.Encode(a, TritCount);
    }

    private static ulong NegatePacked(ulong packed)
    {
        Span<sbyte> digits = stackalloc sbyte[TritCount];
        BalancedTernaryEncoding.Decode(packed, digits, TritCount);
        for (int i = 0; i < TritCount; i++)
        {
            digits[i] = (sbyte)(-digits[i]);
        }
        return BalancedTernaryEncoding.Encode(digits, TritCount);
    }

    private static (ulong Quotient, ulong Remainder) DivRemPacked(ulong dividend, ulong divisor)
    {
        if (BalancedTernaryEncoding.IsZero(divisor, TritCount))
            throw new DivideByZeroException("Attempted to divide by zero IntT.");

        int dividendSign = SignOf(dividend);
        int divisorSign = SignOf(divisor);

        ulong dividendAbs = dividendSign < 0 ? NegatePacked(dividend) : dividend;
        ulong divisorAbs = divisorSign < 0 ? NegatePacked(divisor) : divisor;

        if (BalancedTernaryEncoding.IsZero(dividendAbs, TritCount))
            return (0UL, 0UL);

        if (BalancedTernaryEncoding.Compare(dividendAbs, divisorAbs, TritCount) < 0)
        {
            ulong remain = dividendSign < 0 ? NegatePacked(dividendAbs) : dividendAbs;
            return (0UL, remain);
        }

        ulong quotient = 0UL;
        ulong remainder = dividendAbs;

        while (BalancedTernaryEncoding.Compare(remainder, divisorAbs, TritCount) >= 0)
        {
            int remainderMsb = BalancedTernaryEncoding.HighestNonZeroTrit(remainder, TritCount);
            int divisorMsb = BalancedTernaryEncoding.HighestNonZeroTrit(divisorAbs, TritCount);
            int shift = remainderMsb - divisorMsb;

            ulong shiftedDivisor = ShiftLeftPacked(divisorAbs, shift);

            if (BalancedTernaryEncoding.Compare(shiftedDivisor, remainder, TritCount) > 0)
            {
                shift--;
                if (shift < 0)
                    break;
                shiftedDivisor = ShiftLeftPacked(divisorAbs, shift);
            }

            remainder = SubtractPacked(remainder, shiftedDivisor);
            quotient = AddPacked(quotient, ShiftLeftPacked(One._packed, shift));
        }

        if (dividendSign < 0)
        {
            remainder = NegatePacked(remainder);
        }

        if (dividendSign * divisorSign < 0)
        {
            quotient = NegatePacked(quotient);
        }

        return (quotient, remainder);
    }

    private static ulong NormalizeAndPack(ReadOnlySpan<int> digits)
    {
        Span<sbyte> normalized = stackalloc sbyte[TritCount];
        int carry = 0;
        int limit = Math.Min(TritCount, digits.Length);

        for (int i = 0; i < limit; i++)
        {
            int total = digits[i] + carry;
            carry = 0;
            while (total > 1)
            {
                total -= 3;
                carry++;
            }
            while (total < -1)
            {
                total += 3;
                carry--;
            }
            normalized[i] = (sbyte)total;
        }

        for (int i = limit; i < digits.Length; i++)
        {
            int total = digits[i] + carry;
            carry = 0;
            while (total > 1)
            {
                total -= 3;
                carry++;
            }
            while (total < -1)
            {
                total += 3;
                carry--;
            }
            if (total != 0)
                throw new OverflowException("IntT overflow beyond 32 trits.");
        }

        if (carry != 0)
            throw new OverflowException("IntT overflow beyond 32 trits.");

        return BalancedTernaryEncoding.Encode(normalized, TritCount);
    }

    private static int SignOf(ulong packed)
    {
        int msb = BalancedTernaryEncoding.HighestNonZeroTrit(packed, TritCount);
        if (msb < 0)
            return 0;
        sbyte trit = BalancedTernaryEncoding.DecodeTrit(packed, msb);
        return trit > 0 ? 1 : -1;
    }

    private static long ComputeMaxMagnitude(int count)
    {
        long value = 1;
        for (int i = 0; i < count; i++)
        {
            value = checked(value * 3);
        }
        return (value - 1) / 2;
    }
}
