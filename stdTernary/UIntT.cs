using System;
using System.Diagnostics.CodeAnalysis;

namespace stdTernary;

public readonly struct UIntT : IEquatable<UIntT>, IComparable<UIntT>
{
    public const int TritCount = 32;
    private static readonly ulong MaxUnsigned = ComputeMaxUnsigned(TritCount);

    private readonly ulong _packed;

    internal ulong Packed => _packed;

    private UIntT(ulong packed, bool _)
    {
        _packed = packed;
    }

    public UIntT(ulong value)
    {
        if (value > MaxUnsigned)
            throw new OverflowException($"Value {value} cannot be represented using {TritCount} unbalanced trits.");
        _packed = UnbalancedTernaryEncoding.FromUInt64(value, TritCount);
    }

    public UIntT(IntT value)
    {
        long signed = value.ToInt64();
        if (signed < 0)
            throw new OverflowException("Negative IntT values cannot be represented as UIntT.");
        _packed = UnbalancedTernaryEncoding.FromUInt64((ulong)signed, TritCount);
    }

    public static UIntT Zero { get; } = new UIntT(0UL);
    public static UIntT One { get; } = new UIntT(1UL);
    public static UIntT Two { get; } = new UIntT(2UL);
    public static UIntT MaxValue { get; } = new UIntT(MaxUnsigned);

    public ulong ToUInt64() => UnbalancedTernaryEncoding.ToUInt64(_packed, TritCount);

    public int Sign => UnbalancedTernaryEncoding.IsZero(_packed, TritCount) ? 0 : 1;

    public string TernaryString => UnbalancedTernaryEncoding.ToTernaryString(_packed, TritCount);

    public override string ToString() => $"{TernaryString} (base3) = {ToUInt64()}";

    public static UIntT Parse(string value)
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
                '0' => 0,
                '1' => 1,
                '2' => 2,
                _ => throw new FormatException($"Invalid unbalanced ternary digit '{value[i]}'."),
            };
        }

        return new UIntT(UnbalancedTernaryEncoding.Encode(digits, TritCount), true);
    }

    public static bool TryParse(string? value, out UIntT result)
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

    public IntT ToIntT() => new IntT(checked((long)ToUInt64()));

    public bool Equals(UIntT other) => _packed == other._packed;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is UIntT other && Equals(other);

    public override int GetHashCode() => _packed.GetHashCode();

    public int CompareTo(UIntT other) => UnbalancedTernaryEncoding.Compare(_packed, other._packed, TritCount);

    public int CompareTo(IntT other)
    {
        long otherValue = other.ToInt64();
        if (otherValue < 0)
            return 1;
        return ToUInt64().CompareTo((ulong)otherValue);
    }

    public static bool operator ==(UIntT left, UIntT right) => left.Equals(right);
    public static bool operator !=(UIntT left, UIntT right) => !left.Equals(right);
    public static bool operator <(UIntT left, UIntT right) => left.CompareTo(right) < 0;
    public static bool operator <=(UIntT left, UIntT right) => left.CompareTo(right) <= 0;
    public static bool operator >(UIntT left, UIntT right) => left.CompareTo(right) > 0;
    public static bool operator >=(UIntT left, UIntT right) => left.CompareTo(right) >= 0;
    public static bool operator <(UIntT left, IntT right) => left.CompareTo(right) < 0;
    public static bool operator <=(UIntT left, IntT right) => left.CompareTo(right) <= 0;
    public static bool operator >(UIntT left, IntT right) => left.CompareTo(right) > 0;
    public static bool operator >=(UIntT left, IntT right) => left.CompareTo(right) >= 0;

    public UIntT ADD(UIntT other) => this + other;
    public UIntT SUB(UIntT other) => this - other;
    public UIntT MULT(UIntT other) => this * other;
    public UIntT DIV(UIntT divisor) => this / divisor;
    public UIntT MOD(UIntT divisor) => this % divisor;
    public UIntT AND(UIntT other) => this & other;
    public UIntT OR(UIntT other) => this | other;
    public UIntT XOR(UIntT other) => this ^ other;
    public UIntT INVERT() => ~this;
    public UIntT SHIFTLEFT(int trits) => this << trits;
    public UIntT SHIFTRIGHT(int trits) => this >> trits;

    public static UIntT operator +(UIntT value) => value;
    public static UIntT operator +(UIntT left, UIntT right) => new UIntT(AddPacked(left._packed, right._packed), true);
    public static UIntT operator -(UIntT left, UIntT right) => new UIntT(SubtractPacked(left._packed, right._packed), true);
    public static UIntT operator *(UIntT left, UIntT right) => new UIntT(MultiplyPacked(left._packed, right._packed), true);
    public static UIntT operator /(UIntT left, UIntT right)
    {
        var (quotient, _) = DivRemPacked(left._packed, right._packed);
        return new UIntT(quotient, true);
    }
    public static UIntT operator %(UIntT left, UIntT right)
    {
        var (_, remainder) = DivRemPacked(left._packed, right._packed);
        return new UIntT(remainder, true);
    }

    public static UIntT operator &(UIntT left, UIntT right) => new UIntT(TritwisePacked(left._packed, right._packed, Math.Min), true);
    public static UIntT operator |(UIntT left, UIntT right) => new UIntT(TritwisePacked(left._packed, right._packed, Math.Max), true);
    public static UIntT operator ^(UIntT left, UIntT right) => new UIntT(XorPacked(left._packed, right._packed), true);
    public static UIntT operator ~(UIntT value) => new UIntT(ComplementPacked(value._packed), true);
    public static UIntT operator <<(UIntT value, int count) => new UIntT(ShiftLeftPacked(value._packed, count), true);
    public static UIntT operator >>(UIntT value, int count) => new UIntT(ShiftRightPacked(value._packed, count), true);

    public static implicit operator UIntT(ulong value) => new UIntT(value);
    public static explicit operator ulong(UIntT value) => value.ToUInt64();
    public static explicit operator UIntT(IntT value) => new UIntT(value);
    public static explicit operator IntT(UIntT value) => value.ToIntT();

    internal static ulong AddPacked(ulong left, ulong right)
    {
        Span<int> digits = stackalloc int[TritCount + 1];
        for (int i = 0; i < TritCount; i++)
        {
            digits[i] = UnbalancedTernaryEncoding.DecodeTrit(left, i) + UnbalancedTernaryEncoding.DecodeTrit(right, i);
        }

        return NormalizeAndPack(digits, TritCount);
    }

    internal static ulong SubtractPacked(ulong left, ulong right)
    {
        if (UnbalancedTernaryEncoding.Compare(left, right, TritCount) < 0)
            throw new OverflowException("UIntT subtraction cannot produce a negative value.");

        Span<int> digits = stackalloc int[TritCount];
        int borrow = 0;
        for (int i = 0; i < TritCount; i++)
        {
            int total = UnbalancedTernaryEncoding.DecodeTrit(left, i) - UnbalancedTernaryEncoding.DecodeTrit(right, i) - borrow;
            if (total < 0)
            {
                total += 3;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }
            digits[i] = total;
        }

        if (borrow != 0)
            throw new OverflowException("UIntT subtraction cannot produce a negative value.");

        return NormalizeAndPack(digits, TritCount);
    }

    internal static ulong MultiplyPacked(ulong left, ulong right)
    {
        Span<int> digits = stackalloc int[TritCount * 2];

        for (int i = 0; i < TritCount; i++)
        {
            int b = UnbalancedTernaryEncoding.DecodeTrit(right, i);
            if (b == 0)
                continue;

            for (int j = 0; j < TritCount; j++)
            {
                int a = UnbalancedTernaryEncoding.DecodeTrit(left, j);
                if (a != 0)
                    digits[i + j] += a * b;
            }
        }

        return NormalizeAndPack(digits, TritCount);
    }

    internal static (ulong Quotient, ulong Remainder) DivRemPacked(ulong dividend, ulong divisor)
    {
        if (UnbalancedTernaryEncoding.IsZero(divisor, TritCount))
            throw new DivideByZeroException("Attempted to divide by zero UIntT.");

        if (UnbalancedTernaryEncoding.Compare(dividend, divisor, TritCount) < 0)
            return (0UL, dividend);

        ulong quotient = 0UL;
        ulong remainder = dividend;

        while (UnbalancedTernaryEncoding.Compare(remainder, divisor, TritCount) >= 0)
        {
            int remainderMsb = UnbalancedTernaryEncoding.HighestNonZeroTrit(remainder, TritCount);
            int divisorMsb = UnbalancedTernaryEncoding.HighestNonZeroTrit(divisor, TritCount);
            int shift = remainderMsb - divisorMsb;

            ulong shiftedDivisor = ShiftLeftPacked(divisor, shift);
            if (UnbalancedTernaryEncoding.Compare(shiftedDivisor, remainder, TritCount) > 0)
            {
                shift--;
                if (shift < 0)
                    break;
                shiftedDivisor = ShiftLeftPacked(divisor, shift);
            }

            remainder = SubtractPacked(remainder, shiftedDivisor);
            quotient = AddPacked(quotient, ShiftLeftPacked(One._packed, shift));
        }

        return (quotient, remainder);
    }

    internal static ulong ShiftLeftPacked(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftRightPacked(packed, -count);
        if (count >= TritCount)
        {
            if (!UnbalancedTernaryEncoding.IsZero(packed, TritCount))
                throw new OverflowException("UIntT shift left overflow.");
            return 0UL;
        }

        Span<sbyte> digits = stackalloc sbyte[TritCount];
        UnbalancedTernaryEncoding.Decode(packed, digits, TritCount);

        for (int i = TritCount - count; i < TritCount; i++)
        {
            if (digits[i] != 0)
                throw new OverflowException("UIntT shift left overflow.");
        }

        for (int i = TritCount - 1; i >= count; i--)
        {
            digits[i] = digits[i - count];
        }

        for (int i = 0; i < count; i++)
        {
            digits[i] = 0;
        }

        return UnbalancedTernaryEncoding.Encode(digits, TritCount);
    }

    internal static ulong ShiftRightPacked(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftLeftPacked(packed, -count);
        if (count >= TritCount)
            return 0UL;

        Span<sbyte> digits = stackalloc sbyte[TritCount];
        UnbalancedTernaryEncoding.Decode(packed, digits, TritCount);

        for (int i = 0; i < TritCount - count; i++)
        {
            digits[i] = digits[i + count];
        }

        for (int i = TritCount - count; i < TritCount; i++)
        {
            digits[i] = 0;
        }

        return UnbalancedTernaryEncoding.Encode(digits, TritCount);
    }

    internal static ulong NormalizeAndPack(ReadOnlySpan<int> digits, int tritCount)
    {
        Span<sbyte> normalized = stackalloc sbyte[tritCount];
        int carry = 0;
        int limit = Math.Min(tritCount, digits.Length);

        for (int i = 0; i < limit; i++)
        {
            int total = digits[i] + carry;
            carry = Math.DivRem(total, 3, out int digit);
            while (digit < 0)
            {
                digit += 3;
                carry--;
            }
            normalized[i] = (sbyte)digit;
        }

        for (int i = limit; i < digits.Length; i++)
        {
            int total = digits[i] + carry;
            carry = Math.DivRem(total, 3, out int digit);
            while (digit < 0)
            {
                digit += 3;
                carry--;
            }
            if (digit != 0)
                throw new OverflowException("UIntT overflow beyond 32 trits.");
        }

        if (carry != 0)
            throw new OverflowException("UIntT overflow beyond 32 trits.");

        return UnbalancedTernaryEncoding.Encode(normalized, tritCount);
    }

    private static ulong TritwisePacked(ulong left, ulong right, Func<int, int, int> op)
    {
        Span<sbyte> digits = stackalloc sbyte[TritCount];
        for (int i = 0; i < TritCount; i++)
        {
            digits[i] = (sbyte)op(UnbalancedTernaryEncoding.DecodeTrit(left, i), UnbalancedTernaryEncoding.DecodeTrit(right, i));
        }

        return UnbalancedTernaryEncoding.Encode(digits, TritCount);
    }

    private static ulong XorPacked(ulong left, ulong right)
    {
        Span<sbyte> digits = stackalloc sbyte[TritCount];
        for (int i = 0; i < TritCount; i++)
        {
            digits[i] = (sbyte)((UnbalancedTernaryEncoding.DecodeTrit(left, i) + UnbalancedTernaryEncoding.DecodeTrit(right, i)) % 3);
        }

        return UnbalancedTernaryEncoding.Encode(digits, TritCount);
    }

    private static ulong ComplementPacked(ulong packed)
    {
        Span<sbyte> digits = stackalloc sbyte[TritCount];
        UnbalancedTernaryEncoding.Decode(packed, digits, TritCount);
        for (int i = 0; i < TritCount; i++)
        {
            digits[i] = (sbyte)(2 - digits[i]);
        }

        return UnbalancedTernaryEncoding.Encode(digits, TritCount);
    }

    private static ulong ComputeMaxUnsigned(int count)
    {
        ulong value = 1UL;
        for (int i = 0; i < count; i++)
        {
            value = checked(value * 3UL);
        }
        return value - 1UL;
    }
}
