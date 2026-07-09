using System;
using System.Diagnostics.CodeAnalysis;

namespace stdTernary;

public struct UTryte : IEquatable<UTryte>, IComparable<UTryte>
{
    private static byte _tritCount = 6;
    private ulong _packed;

    public static byte N_TRITS_PER_TRYTE
    {
        get => _tritCount;
        set
        {
            if (value is < 2 or > UnbalancedTernaryEncoding.MaxTrits)
                throw new ArgumentOutOfRangeException(nameof(value), "UTryte must contain between 2 and 32 trits.");
            _tritCount = value;
        }
    }

    public static ulong MaxValue => Pow3(_tritCount) - 1UL;
    public static ulong MinValue => 0UL;

    public static UTryte Zero => new UTryte(0UL);
    public static UTryte One => new UTryte(1UL);
    public static UTryte Two => new UTryte(2UL);

    public ulong PackedTrits => _packed;

    public UTrit[] Value
    {
        readonly get
        {
            int count = _tritCount;
            var trits = new UTrit[count];
            Span<sbyte> digits = stackalloc sbyte[count];
            UnbalancedTernaryEncoding.Decode(_packed, digits, count);
            for (int i = 0; i < count; i++)
            {
                trits[i] = new UTrit((byte)digits[i]);
            }
            return trits;
        }
        set => SetValue(value);
    }

    public string TryteString => UnbalancedTernaryEncoding.ToTernaryString(_packed, _tritCount);

    public ulong ULongValue
    {
        readonly get => UnbalancedTernaryEncoding.ToUInt64(_packed, _tritCount);
        set => SetValue(value);
    }

    public UTrit this[int index]
    {
        get
        {
            if ((uint)index >= _tritCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return new UTrit((byte)UnbalancedTernaryEncoding.DecodeTrit(_packed, index));
        }
    }

    public UTryte(UTrit[] value)
    {
        _packed = 0;
        SetValue(value);
    }

    public UTryte(string value)
    {
        _packed = 0;
        SetValue(value);
    }

    public UTryte(ulong value)
    {
        if (value > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value must fit in {N_TRITS_PER_TRYTE} unbalanced trits.");
        _packed = UnbalancedTernaryEncoding.FromUInt64(value, _tritCount);
    }

    public UTryte(int value) : this(CheckedUnsigned(value)) { }

    public UTryte(Tryte value) : this(CheckedUnsigned(value.ShortValue)) { }

    public UTryte(uint packedTrits) : this((ulong)packedTrits, true) { }

    public UTryte(ulong packedTrits, bool packed)
    {
        if (!packed)
        {
            if (packedTrits > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(packedTrits));
            _packed = UnbalancedTernaryEncoding.FromUInt64(packedTrits, _tritCount);
            return;
        }

        ValidatePacked(packedTrits, _tritCount);
        _packed = packedTrits;
    }

    public void SetValue(UTrit[] value)
    {
        if (value.Length != _tritCount)
            throw new ArgumentException($"Expected {N_TRITS_PER_TRYTE} trits.", nameof(value));

        Span<sbyte> digits = stackalloc sbyte[_tritCount];
        for (int i = 0; i < _tritCount; i++)
        {
            digits[i] = (sbyte)(byte)value[i];
        }

        _packed = UnbalancedTernaryEncoding.Encode(digits, _tritCount);
    }

    public void SetValue(string value)
    {
        if (value.Length != _tritCount)
            throw new ArgumentException($"Expected {N_TRITS_PER_TRYTE} characters.", nameof(value));

        Span<sbyte> digits = stackalloc sbyte[_tritCount];
        for (int i = 0; i < _tritCount; i++)
        {
            char c = value[i];
            digits[_tritCount - 1 - i] = c switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                _ => throw new ArgumentException("Unbalanced ternary characters must be '0', '1', or '2'.", nameof(value)),
            };
        }

        _packed = UnbalancedTernaryEncoding.Encode(digits, _tritCount);
    }

    public void SetValue(ulong value)
    {
        if (value > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value must fit in {N_TRITS_PER_TRYTE} unbalanced trits.");
        _packed = UnbalancedTernaryEncoding.FromUInt64(value, _tritCount);
    }

    public void SetTrit(int index, UTrit trit)
    {
        if ((uint)index >= _tritCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        _packed = UnbalancedTernaryEncoding.SetTrit(_packed, index, (sbyte)(byte)trit);
    }

    public Tryte ToTryte()
    {
        ulong value = ULongValue;
        if (value > (ulong)Tryte.MaxValue)
            throw new OverflowException("UTryte value cannot fit in the current balanced Tryte width.");
        return new Tryte((int)value);
    }

    public UTryte INVERT() => new UTryte(ComplementPacked(_packed), true);
    public void InvertSelf() => _packed = ComplementPacked(_packed);
    public UTryte SHIFTLEFT(int trits) => new UTryte(ShiftLeftPacked(_packed, trits), true);
    public UTryte SHIFTRIGHT(int trits) => new UTryte(ShiftRightPacked(_packed, trits), true);
    public UTryte ADD(UTryte other) => new UTryte(AddPacked(_packed, other._packed), true);
    public UTryte SUB(UTryte other) => new UTryte(SubtractPacked(_packed, other._packed), true);
    public UTryte MULT(UTryte other) => new UTryte(MultiplyPacked(_packed, other._packed), true);
    public UTryte DIV(UTryte divisor) => DIVREM(divisor).Quotient;
    public UTryte MOD(UTryte divisor) => DIVREM(divisor).Remainder;
    public (UTryte Quotient, UTryte Remainder) DIVREM(UTryte divisor)
    {
        var (quotient, remainder) = DivRemPacked(_packed, divisor._packed);
        return (new UTryte(quotient, true), new UTryte(remainder, true));
    }
    public UTryte AND(UTryte other) => new UTryte(TritwisePacked(_packed, other._packed, Math.Min), true);
    public UTryte OR(UTryte other) => new UTryte(TritwisePacked(_packed, other._packed, Math.Max), true);
    public UTryte XOR(UTryte other) => new UTryte(XorPacked(_packed, other._packed), true);
    public UTryte IMP(UTryte other) => INVERT().OR(other);

    public static Trit COMPARET(UTryte a, UTryte b) => Trit.FromComparison(a.CompareTo(b));

    public bool Equals(UTryte other) => _packed == other._packed;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is UTryte other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_packed, _tritCount);
    public int CompareTo(UTryte other) => UnbalancedTernaryEncoding.Compare(_packed, other._packed, _tritCount);
    public override string ToString() => $"{TryteString} = {ULongValue}";

    public static UTryte operator &(UTryte left, UTryte right) => left.AND(right);
    public static UTryte operator |(UTryte left, UTryte right) => left.OR(right);
    public static UTryte operator ^(UTryte left, UTryte right) => left.XOR(right);
    public static UTryte operator ~(UTryte value) => value.INVERT();
    public static bool operator ==(UTryte left, UTryte right) => left.Equals(right);
    public static bool operator !=(UTryte left, UTryte right) => !left.Equals(right);
    public static bool operator >(UTryte left, UTryte right) => left.CompareTo(right) > 0;
    public static bool operator <(UTryte left, UTryte right) => left.CompareTo(right) < 0;
    public static bool operator >=(UTryte left, UTryte right) => left.CompareTo(right) >= 0;
    public static bool operator <=(UTryte left, UTryte right) => left.CompareTo(right) <= 0;
    public static UTryte operator +(UTryte left, UTryte right) => left.ADD(right);
    public static UTryte operator -(UTryte left, UTryte right) => left.SUB(right);
    public static UTryte operator *(UTryte left, UTryte right) => left.MULT(right);
    public static UTryte operator /(UTryte left, UTryte right) => left.DIV(right);
    public static UTryte operator %(UTryte left, UTryte right) => left.MOD(right);
    public static UTryte operator <<(UTryte value, int trits) => value.SHIFTLEFT(trits);
    public static UTryte operator >>(UTryte value, int trits) => value.SHIFTRIGHT(trits);
    public static UTryte operator ++(UTryte value) => value + One;
    public static UTryte operator --(UTryte value) => value - One;
    public static implicit operator ulong(UTryte value) => value.ULongValue;
    public static implicit operator UTryte(ulong value) => new UTryte(value);
    public static explicit operator string(UTryte value) => value.TryteString;
    public static explicit operator UTryte(string value) => new UTryte(value);
    public static explicit operator Tryte(UTryte value) => value.ToTryte();
    public static explicit operator UTryte(Tryte value) => new UTryte(value);

    private static ulong AddPacked(ulong left, ulong right)
    {
        int count = _tritCount;
        Span<int> digits = stackalloc int[count + 1];
        for (int i = 0; i < count; i++)
        {
            digits[i] = UnbalancedTernaryEncoding.DecodeTrit(left, i) + UnbalancedTernaryEncoding.DecodeTrit(right, i);
        }

        return NormalizeAndPack(digits, count);
    }

    private static ulong SubtractPacked(ulong left, ulong right)
    {
        if (UnbalancedTernaryEncoding.Compare(left, right, _tritCount) < 0)
            throw new OverflowException("UTryte subtraction cannot produce a negative value.");

        Span<int> digits = stackalloc int[_tritCount];
        int borrow = 0;
        for (int i = 0; i < _tritCount; i++)
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

        return NormalizeAndPack(digits, _tritCount);
    }

    private static ulong MultiplyPacked(ulong left, ulong right)
    {
        int count = _tritCount;
        Span<int> digits = stackalloc int[count * 2];

        for (int i = 0; i < count; i++)
        {
            int b = UnbalancedTernaryEncoding.DecodeTrit(right, i);
            for (int j = 0; j < count; j++)
            {
                digits[i + j] += UnbalancedTernaryEncoding.DecodeTrit(left, j) * b;
            }
        }

        return NormalizeAndPack(digits, count);
    }

    private static (ulong Quotient, ulong Remainder) DivRemPacked(ulong dividend, ulong divisor)
    {
        if (UnbalancedTernaryEncoding.IsZero(divisor, _tritCount))
            throw new DivideByZeroException("Attempted to divide by zero UTryte.");

        ulong quotient = 0UL;
        ulong remainder = dividend;
        ulong unit = UnbalancedTernaryEncoding.FromUInt64(1, _tritCount);

        while (UnbalancedTernaryEncoding.Compare(remainder, divisor, _tritCount) >= 0)
        {
            int shift = UnbalancedTernaryEncoding.HighestNonZeroTrit(remainder, _tritCount) - UnbalancedTernaryEncoding.HighestNonZeroTrit(divisor, _tritCount);
            ulong shiftedDivisor = ShiftLeftPacked(divisor, shift);
            if (UnbalancedTernaryEncoding.Compare(shiftedDivisor, remainder, _tritCount) > 0)
            {
                shift--;
                shiftedDivisor = ShiftLeftPacked(divisor, shift);
            }

            remainder = SubtractPacked(remainder, shiftedDivisor);
            quotient = AddPacked(quotient, ShiftLeftPacked(unit, shift));
        }

        return (quotient, remainder);
    }

    private static ulong ShiftLeftPacked(ulong packed, int count) => ShiftPacked(packed, count, true);
    private static ulong ShiftRightPacked(ulong packed, int count) => ShiftPacked(packed, count, false);

    private static ulong ShiftPacked(ulong packed, int count, bool left)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftPacked(packed, -count, !left);
        if (count >= _tritCount)
            return 0UL;

        Span<sbyte> digits = stackalloc sbyte[_tritCount];
        UnbalancedTernaryEncoding.Decode(packed, digits, _tritCount);
        Span<sbyte> shifted = stackalloc sbyte[_tritCount];

        if (left)
        {
            // Truncating shift: trits carried past the top position are dropped, like a
            // hardware left shift (no overflow exception).
            for (int i = count; i < _tritCount; i++)
            {
                shifted[i] = digits[i - count];
            }
        }
        else
        {
            for (int i = 0; i < _tritCount - count; i++)
            {
                shifted[i] = digits[i + count];
            }
        }

        return UnbalancedTernaryEncoding.Encode(shifted, _tritCount);
    }

    private static ulong NormalizeAndPack(ReadOnlySpan<int> digits, int tritCount) => UIntT.NormalizeAndPack(digits, tritCount);

    private static ulong TritwisePacked(ulong left, ulong right, Func<int, int, int> op)
    {
        Span<sbyte> digits = stackalloc sbyte[_tritCount];
        for (int i = 0; i < _tritCount; i++)
        {
            digits[i] = (sbyte)op(UnbalancedTernaryEncoding.DecodeTrit(left, i), UnbalancedTernaryEncoding.DecodeTrit(right, i));
        }
        return UnbalancedTernaryEncoding.Encode(digits, _tritCount);
    }

    private static ulong XorPacked(ulong left, ulong right)
    {
        Span<sbyte> digits = stackalloc sbyte[_tritCount];
        for (int i = 0; i < _tritCount; i++)
        {
            digits[i] = (sbyte)((UnbalancedTernaryEncoding.DecodeTrit(left, i) + UnbalancedTernaryEncoding.DecodeTrit(right, i)) % 3);
        }
        return UnbalancedTernaryEncoding.Encode(digits, _tritCount);
    }

    private static ulong ComplementPacked(ulong packed)
    {
        Span<sbyte> digits = stackalloc sbyte[_tritCount];
        UnbalancedTernaryEncoding.Decode(packed, digits, _tritCount);
        for (int i = 0; i < _tritCount; i++)
        {
            digits[i] = (sbyte)(2 - digits[i]);
        }
        return UnbalancedTernaryEncoding.Encode(digits, _tritCount);
    }

    private static ulong Pow3(int exponent)
    {
        ulong value = 1UL;
        for (int i = 0; i < exponent; i++)
        {
            value *= 3UL;
        }
        return value;
    }

    private static ulong CheckedUnsigned(int value)
    {
        if (value < 0)
            throw new OverflowException("Negative values cannot be represented as UTryte.");
        return (ulong)value;
    }

    private static void ValidatePacked(ulong packed, int count)
    {
        ulong mask = count >= 32 ? ulong.MaxValue : (1UL << (count * 2)) - 1UL;
        if ((packed & ~mask) != 0)
            throw new ArgumentOutOfRangeException(nameof(packed), "Packed value contains bits outside configured trit width.");
        for (int i = 0; i < count; i++)
        {
            _ = UnbalancedTernaryEncoding.DecodeTrit(packed, i);
        }
    }
}
