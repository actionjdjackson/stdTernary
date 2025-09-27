using System;
using System.Diagnostics.CodeAnalysis;

namespace stdTernary;

public struct Tryte : IEquatable<Tryte>, IComparable<Tryte>
{
    private static byte _tritCount = 6;
    private ulong _packed;

    public static byte N_TRITS_PER_TRYTE
    {
        get => _tritCount;
        set
        {
            if (value is < 2 or > BalancedTernaryEncoding.MaxTrits)
                throw new ArgumentOutOfRangeException(nameof(value), "Tryte must contain between 2 and 32 trits.");
            _tritCount = value;
        }
    }

    public static short MaxValue => (short)((Pow3(_tritCount) - 1) / 2);
    public static short MinValue => (short)(-MaxValue);

    public static Tryte Zero => new Tryte(0);
    public static Tryte One => new Tryte(1);
    public static Tryte NegativeOne => new Tryte(-1);

    public ulong PackedTrits => _packed;

    public Trit[] Value
    {
        readonly get
        {
            int count = _tritCount;
            var trits = new Trit[count];
            Span<sbyte> digits = stackalloc sbyte[count];
            BalancedTernaryEncoding.Decode(_packed, digits, count);
            for (int i = 0; i < count; i++)
            {
                trits[i] = new Trit(digits[i]);
            }
            return trits;
        }
        set => SetValue(value);
    }

    public string TryteString => ConvertToStringRepresentation();

    public short ShortValue
    {
        readonly get => (short)BalancedTernaryEncoding.ToInt64(_packed, _tritCount);
        set => SetValue(value);
    }

    public Trit this[int index]
    {
        get
        {
            if ((uint)index >= _tritCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return new Trit(BalancedTernaryEncoding.DecodeTrit(_packed, index));
        }
    }

    public Tryte(Trit[] value)
    {
        _packed = 0;
        SetValue(value);
    }

    public Tryte(string value)
    {
        _packed = 0;
        SetValue(value);
    }

    public Tryte(short value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value must fit in {N_TRITS_PER_TRYTE} balanced trits.");
        _packed = BalancedTernaryEncoding.FromInt64(value, _tritCount);
    }

    public Tryte(int value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value must fit in {N_TRITS_PER_TRYTE} balanced trits.");
        _packed = BalancedTernaryEncoding.FromInt64(value, _tritCount);
    }

    public Tryte(uint packedTrits) : this((ulong)packedTrits) { }

    public Tryte(ulong packedTrits)
    {
        ValidatePacked(packedTrits, _tritCount);
        _packed = packedTrits;
    }

    public void SetValue(Trit[] value)
    {
        if (value.Length != _tritCount)
            throw new ArgumentException($"Expected {N_TRITS_PER_TRYTE} trits.", nameof(value));

        Span<sbyte> digits = stackalloc sbyte[_tritCount];
        for (int i = 0; i < _tritCount; i++)
        {
            digits[i] = (sbyte)value[i].Value;
        }

        _packed = BalancedTernaryEncoding.Encode(digits, _tritCount);
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
                '+' => 1,
                '-' => -1,
                '0' => 0,
                _ => throw new ArgumentException("Ternary characters must be '+', '-', or '0'.", nameof(value)),
            };
        }

        _packed = BalancedTernaryEncoding.Encode(digits, _tritCount);
    }

    public void SetValue(short value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value must fit in {N_TRITS_PER_TRYTE} balanced trits.");
        _packed = BalancedTernaryEncoding.FromInt64(value, _tritCount);
    }

    public void SetValue(int value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value must fit in {N_TRITS_PER_TRYTE} balanced trits.");
        _packed = BalancedTernaryEncoding.FromInt64(value, _tritCount);
    }

    public void SetValue(ulong packedTrits)
    {
        ValidatePacked(packedTrits, _tritCount);
        _packed = packedTrits;
    }

    public void SetTrit(int index, Trit trit)
    {
        if ((uint)index >= _tritCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        _packed = BalancedTernaryEncoding.SetTrit(_packed, index, (sbyte)trit.Value);
    }

    public string ConvertToStringRepresentation()
    {
        return BalancedTernaryEncoding.ToTernaryString(_packed, _tritCount);
    }

    public Tryte INVERT() => new Tryte(NegatePacked(_packed));

    public void InvertSelf() => _packed = NegatePacked(_packed);

    public Tryte SHIFTLEFT(int trits) => new Tryte(ShiftLeftPacked(_packed, trits));

    public Tryte SHIFTRIGHT(int trits) => new Tryte(ShiftRightPacked(_packed, trits));

    public Tryte ADD(Tryte other) => new Tryte(AddPacked(_packed, other._packed));

    public Tryte SUB(Tryte other) => new Tryte(SubtractPacked(_packed, other._packed));

    public Tryte MULT(Tryte other) => new Tryte(MultiplyPacked(_packed, other._packed));

    public Tryte DIV(Tryte divisor) => DIVREM(divisor).Quotient;

    public Tryte MOD(Tryte divisor) => DIVREM(divisor).Remainder;

    public (Tryte Quotient, Tryte Remainder) DIVREM(Tryte divisor)
    {
        var (quotient, remainder) = DivRemPacked(_packed, divisor._packed);
        return (new Tryte(quotient), new Tryte(remainder));
    }

    public Tryte AND(Tryte other) => new Tryte(AndPacked(_packed, other._packed));

    public Tryte OR(Tryte other) => new Tryte(OrPacked(_packed, other._packed));

    public Tryte XOR(Tryte other) => new Tryte(XorPacked(_packed, other._packed));

    public static Trit COMPARET(Tryte a, Tryte b)
    {
        int comparison = BalancedTernaryEncoding.Compare(a._packed, b._packed, _tritCount);
        return comparison switch
        {
            < 0 => new Trit(-1),
            > 0 => new Trit(1),
            _ => new Trit(0),
        };
    }

    public bool Equals(Tryte other) => _packed == other._packed;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Tryte other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_packed, _tritCount);

    public int CompareTo(Tryte other) => BalancedTernaryEncoding.Compare(_packed, other._packed, _tritCount);

    public override string ToString() => $"{TryteString} = {ShortValue}";

    public static Tryte operator &(Tryte left, Tryte right) => left.AND(right);
    public static Tryte operator |(Tryte left, Tryte right) => left.OR(right);
    public static Tryte operator ^(Tryte left, Tryte right) => left.XOR(right);
    public static Tryte operator ~(Tryte value) => value.INVERT();
    public static bool operator ==(Tryte left, Tryte right) => left.Equals(right);
    public static bool operator !=(Tryte left, Tryte right) => !left.Equals(right);
    public static bool operator >(Tryte left, Tryte right) => left.CompareTo(right) > 0;
    public static bool operator <(Tryte left, Tryte right) => left.CompareTo(right) < 0;
    public static bool operator >=(Tryte left, Tryte right) => left.CompareTo(right) >= 0;
    public static bool operator <=(Tryte left, Tryte right) => left.CompareTo(right) <= 0;
    public static bool operator >(Tryte left, int right) => left.ShortValue > right;
    public static bool operator <(Tryte left, int right) => left.ShortValue < right;
    public static bool operator >=(Tryte left, int right) => left.ShortValue >= right;
    public static bool operator <=(Tryte left, int right) => left.ShortValue <= right;
    public static Tryte operator +(Tryte left, Tryte right) => left.ADD(right);
    public static Tryte operator -(Tryte left, Tryte right) => left.SUB(right);
    public static Tryte operator *(Tryte left, Tryte right) => left.MULT(right);
    public static Tryte operator /(Tryte left, Tryte right) => left.DIV(right);
    public static Tryte operator %(Tryte left, Tryte right) => left.MOD(right);
    public static Tryte operator <<(Tryte value, int trits) => value.SHIFTLEFT(trits);
    public static Tryte operator >>(Tryte value, int trits) => value.SHIFTRIGHT(trits);
    public static Tryte operator ++(Tryte value) => value + One;
    public static Tryte operator --(Tryte value) => value - One;
    public static Tryte operator +(Tryte value) => MathT.Abs(value);
    public static Tryte operator -(Tryte value) => value.INVERT();
    public static implicit operator short(Tryte value) => value.ShortValue;
    public static implicit operator Tryte(short value) => new Tryte(value);
    public static implicit operator int(Tryte value) => value.ShortValue;
    public static implicit operator Tryte(int value) => new Tryte(value);
    public static explicit operator string(Tryte value) => value.TryteString;
    public static explicit operator Tryte(string value) => new Tryte(value);
    public static implicit operator double(Tryte value) => value.ShortValue;

    private static ulong AddPacked(ulong left, ulong right)
    {
        int count = _tritCount;
        Span<int> digits = stackalloc int[count + 1];
        for (int i = 0; i < count; i++)
        {
            digits[i] = BalancedTernaryEncoding.DecodeTrit(left, i) + BalancedTernaryEncoding.DecodeTrit(right, i);
        }
        return NormalizeAndPack(digits, count);
    }

    private static ulong SubtractPacked(ulong left, ulong right) => AddPacked(left, NegatePacked(right));

    private static ulong MultiplyPacked(ulong left, ulong right)
    {
        int count = _tritCount;
        Span<int> digits = stackalloc int[count * 2];

        for (int i = 0; i < count; i++)
        {
            int b = BalancedTernaryEncoding.DecodeTrit(right, i);
            if (b == 0)
                continue;

            for (int j = 0; j < count; j++)
            {
                int a = BalancedTernaryEncoding.DecodeTrit(left, j);
                if (a == 0)
                    continue;

                digits[i + j] += a * b;
            }
        }

        return NormalizeAndPack(digits, count);
    }

    private static (ulong Quotient, ulong Remainder) DivRemPacked(ulong dividend, ulong divisor)
    {
        if (BalancedTernaryEncoding.IsZero(divisor, _tritCount))
            throw new DivideByZeroException("Attempted to divide by zero Tryte.");

        int dividendSign = SignOf(dividend);
        int divisorSign = SignOf(divisor);

        ulong dividendAbs = dividendSign < 0 ? NegatePacked(dividend) : dividend;
        ulong divisorAbs = divisorSign < 0 ? NegatePacked(divisor) : divisor;

        if (BalancedTernaryEncoding.IsZero(dividendAbs, _tritCount))
            return (0UL, 0UL);

        if (BalancedTernaryEncoding.Compare(dividendAbs, divisorAbs, _tritCount) < 0)
        {
            ulong remainderSmall = dividendSign < 0 ? NegatePacked(dividendAbs) : dividendAbs;
            return (0UL, remainderSmall);
        }

        ulong quotient = 0UL;
        ulong remainder = dividendAbs;
        ulong unit = BalancedTernaryEncoding.FromInt64(1, _tritCount);

        while (BalancedTernaryEncoding.Compare(remainder, divisorAbs, _tritCount) >= 0)
        {
            int remainderMsb = BalancedTernaryEncoding.HighestNonZeroTrit(remainder, _tritCount);
            int divisorMsb = BalancedTernaryEncoding.HighestNonZeroTrit(divisorAbs, _tritCount);
            int shift = remainderMsb - divisorMsb;

            ulong shiftedDivisor = ShiftLeftPacked(divisorAbs, shift);
            if (BalancedTernaryEncoding.Compare(shiftedDivisor, remainder, _tritCount) > 0)
            {
                shift--;
                if (shift < 0)
                    break;
                shiftedDivisor = ShiftLeftPacked(divisorAbs, shift);
            }

            remainder = SubtractPacked(remainder, shiftedDivisor);
            quotient = AddPacked(quotient, ShiftLeftPacked(unit, shift));
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

    private static ulong AndPacked(ulong left, ulong right)
    {
        int count = _tritCount;
        Span<sbyte> a = stackalloc sbyte[count];
        Span<sbyte> b = stackalloc sbyte[count];
        BalancedTernaryEncoding.Decode(left, a, count);
        BalancedTernaryEncoding.Decode(right, b, count);

        for (int i = 0; i < count; i++)
        {
            a[i] = (sbyte)Math.Min(a[i], b[i]);
        }

        return BalancedTernaryEncoding.Encode(a, count);
    }

    private static ulong OrPacked(ulong left, ulong right)
    {
        int count = _tritCount;
        Span<sbyte> a = stackalloc sbyte[count];
        Span<sbyte> b = stackalloc sbyte[count];
        BalancedTernaryEncoding.Decode(left, a, count);
        BalancedTernaryEncoding.Decode(right, b, count);

        for (int i = 0; i < count; i++)
        {
            a[i] = (sbyte)Math.Max(a[i], b[i]);
        }

        return BalancedTernaryEncoding.Encode(a, count);
    }

    private static ulong XorPacked(ulong left, ulong right)
    {
        int count = _tritCount;
        Span<sbyte> a = stackalloc sbyte[count];
        Span<sbyte> b = stackalloc sbyte[count];
        BalancedTernaryEncoding.Decode(left, a, count);
        BalancedTernaryEncoding.Decode(right, b, count);

        for (int i = 0; i < count; i++)
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

        return BalancedTernaryEncoding.Encode(a, count);
    }

    private static ulong NegatePacked(ulong packed)
    {
        int count = _tritCount;
        Span<sbyte> digits = stackalloc sbyte[count];
        BalancedTernaryEncoding.Decode(packed, digits, count);
        for (int i = 0; i < count; i++)
        {
            digits[i] = (sbyte)(-digits[i]);
        }
        return BalancedTernaryEncoding.Encode(digits, count);
    }

    private static ulong ShiftLeftPacked(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftRightPacked(packed, -count);

        int length = _tritCount;
        if (count >= length)
        {
            if (!BalancedTernaryEncoding.IsZero(packed, length))
                throw new OverflowException("Tryte shift left overflow.");
            return 0UL;
        }

        Span<sbyte> digits = stackalloc sbyte[length];
        BalancedTernaryEncoding.Decode(packed, digits, length);

        for (int i = length - count; i < length; i++)
        {
            if (digits[i] != 0)
                throw new OverflowException("Tryte shift left overflow.");
        }

        for (int i = length - 1; i >= count; i--)
        {
            digits[i] = digits[i - count];
        }

        for (int i = 0; i < count; i++)
        {
            digits[i] = 0;
        }

        return BalancedTernaryEncoding.Encode(digits, length);
    }

    private static ulong ShiftRightPacked(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftLeftPacked(packed, -count);

        int length = _tritCount;
        if (count >= length)
            return 0UL;

        Span<sbyte> digits = stackalloc sbyte[length];
        BalancedTernaryEncoding.Decode(packed, digits, length);

        for (int i = 0; i < length - count; i++)
        {
            digits[i] = digits[i + count];
        }

        for (int i = length - count; i < length; i++)
        {
            digits[i] = 0;
        }

        return BalancedTernaryEncoding.Encode(digits, length);
    }

    private static ulong NormalizeAndPack(ReadOnlySpan<int> digits, int tritCount)
    {
        Span<sbyte> normalized = stackalloc sbyte[tritCount];
        int carry = 0;
        int limit = Math.Min(tritCount, digits.Length);

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
                throw new OverflowException("Tryte overflow beyond allocated trits.");
        }

        if (carry != 0)
            throw new OverflowException("Tryte overflow beyond allocated trits.");

        return BalancedTernaryEncoding.Encode(normalized, tritCount);
    }

    private static int SignOf(ulong packed)
    {
        int msb = BalancedTernaryEncoding.HighestNonZeroTrit(packed, _tritCount);
        if (msb < 0)
            return 0;
        sbyte digit = BalancedTernaryEncoding.DecodeTrit(packed, msb);
        return digit > 0 ? 1 : -1;
    }

    private static long Pow3(int exponent)
    {
        long value = 1;
        for (int i = 0; i < exponent; i++)
        {
            value *= 3;
        }
        return value;
    }

    private static void ValidatePacked(ulong packed, int count)
    {
        ulong mask = count >= 32 ? ulong.MaxValue : (1UL << (count * 2)) - 1UL;
        if ((packed & ~mask) != 0)
            throw new ArgumentOutOfRangeException(nameof(packed), "Packed value contains bits outside configured trit width.");
    }
}
