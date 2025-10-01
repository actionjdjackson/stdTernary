using System;
using System.Diagnostics.CodeAnalysis;

namespace stdTernary;

public struct Byte : IEquatable<Byte>, IComparable<Byte>
{
    public const int BitCount = 8;
    private ulong _packed;

    public static Byte Zero => new Byte(0);
    public static Byte One => new Byte(1);
    public static Byte MaxValue => new Byte(255);

    public Byte(int value)
    {
        if (value is < 0 or > 255)
            throw new ArgumentOutOfRangeException(nameof(value), "Byte must be between 0 and 255.");
        _packed = BinaryEncoding.FromInt64(value, BitCount);
    }

    public Byte(Bit[] bits)
    {
        if (bits is null)
            throw new ArgumentNullException(nameof(bits));
        if (bits.Length != BitCount)
            throw new ArgumentException($"Expected {BitCount} bits.", nameof(bits));

        Span<sbyte> digits = stackalloc sbyte[BitCount];
        for (int i = 0; i < BitCount; i++)
        {
            digits[i] = bits[i].Value;
        }

        _packed = BinaryEncoding.Encode(digits, BitCount);
    }

    public Byte(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        if (value.Length != BitCount)
            throw new ArgumentException($"Expected {BitCount} characters.", nameof(value));

        Span<sbyte> digits = stackalloc sbyte[BitCount];
        for (int i = 0; i < BitCount; i++)
        {
            char c = value[i];
            digits[BitCount - 1 - i] = c switch
            {
                '0' => 0,
                '1' => 1,
                _ => throw new ArgumentException("Binary characters must be '0' or '1'.", nameof(value)),
            };
        }

        _packed = BinaryEncoding.Encode(digits, BitCount);
    }

    private Byte(ulong packed, bool skipValidation)
    {
        if (!skipValidation)
        {
            ValidatePacked(packed);
        }
        _packed = packed;
    }

    public string BinaryString => BinaryEncoding.ToBinaryString(_packed, BitCount);

    public int IntValue => (int)BinaryEncoding.ToInt64(_packed, BitCount);

    public Bit this[int index]
    {
        get
        {
            if ((uint)index >= BitCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return new Bit(BinaryEncoding.DecodeBit(_packed, index));
        }
    }

    public override string ToString() => BinaryString;

    public bool Equals(Byte other) => _packed == other._packed;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Byte other && Equals(other);

    public override int GetHashCode() => _packed.GetHashCode();

    public int CompareTo(Byte other) => BinaryEncoding.Compare(_packed, other._packed, BitCount);

    public static bool operator ==(Byte left, Byte right) => left.Equals(right);
    public static bool operator !=(Byte left, Byte right) => !left.Equals(right);
    public static bool operator <(Byte left, Byte right) => left.CompareTo(right) < 0;
    public static bool operator <=(Byte left, Byte right) => left.CompareTo(right) <= 0;
    public static bool operator >(Byte left, Byte right) => left.CompareTo(right) > 0;
    public static bool operator >=(Byte left, Byte right) => left.CompareTo(right) >= 0;

    public static Byte operator +(Byte left, Byte right)
    {
        ulong result = AddPacked(left._packed, right._packed);
        return new Byte(result, skipValidation: true);
    }

    public static Byte operator -(Byte left, Byte right)
    {
        ulong result = SubtractPacked(left._packed, right._packed);
        return new Byte(result, skipValidation: true);
    }

    public static Byte operator *(Byte left, Byte right)
    {
        ulong result = MultiplyPacked(left._packed, right._packed);
        return new Byte(result, skipValidation: true);
    }

    public static Byte operator /(Byte left, Byte right)
    {
        if (BinaryEncoding.IsZero(right._packed, BitCount))
            throw new DivideByZeroException();
        ulong quotient = DivRemPacked(left._packed, right._packed).Quotient;
        return new Byte(quotient, skipValidation: true);
    }

    public static Byte operator %(Byte left, Byte right)
    {
        if (BinaryEncoding.IsZero(right._packed, BitCount))
            throw new DivideByZeroException();
        ulong remainder = DivRemPacked(left._packed, right._packed).Remainder;
        return new Byte(remainder, skipValidation: true);
    }

    public static Byte operator &(Byte left, Byte right)
    {
        ulong result = AndPacked(left._packed, right._packed);
        return new Byte(result, skipValidation: true);
    }

    public static Byte operator |(Byte left, Byte right)
    {
        ulong result = OrPacked(left._packed, right._packed);
        return new Byte(result, skipValidation: true);
    }

    public static Byte operator ^(Byte left, Byte right)
    {
        ulong result = XorPacked(left._packed, right._packed);
        return new Byte(result, skipValidation: true);
    }

    public static Byte operator ~(Byte value)
    {
        ulong result = NotPacked(value._packed);
        return new Byte(result, skipValidation: true);
    }

    public static Byte operator <<(Byte value, int count)
    {
        ulong result = ShiftLeftPacked(value._packed, count);
        return new Byte(result, skipValidation: true);
    }

    public static Byte operator >>(Byte value, int count)
    {
        ulong result = ShiftRightPacked(value._packed, count);
        return new Byte(result, skipValidation: true);
    }

    public static implicit operator Byte(int value) => new Byte(value);
    public static explicit operator int(Byte value) => value.IntValue;

    private static void ValidatePacked(ulong packed)
    {
        if (BinaryEncoding.HighestNonZeroBit(packed, BitCount) >= BitCount)
            throw new ArgumentOutOfRangeException(nameof(packed), "Packed value exceeds byte capacity.");
    }

    private static ulong AddPacked(ulong left, ulong right)
    {
        Span<int> digits = stackalloc int[BitCount + 1];
        for (int i = 0; i < BitCount; i++)
        {
            digits[i] = BinaryEncoding.DecodeBit(left, i) + BinaryEncoding.DecodeBit(right, i);
        }

        return NormalizeAndPack(digits, BitCount);
    }

    private static ulong SubtractPacked(ulong left, ulong right)
    {
        Span<int> digits = stackalloc int[BitCount];
        for (int i = 0; i < BitCount; i++)
        {
            digits[i] = BinaryEncoding.DecodeBit(left, i) - BinaryEncoding.DecodeBit(right, i);
        }

        return NormalizeAndPack(digits, BitCount);
    }

    private static ulong MultiplyPacked(ulong left, ulong right)
    {
        Span<int> digits = stackalloc int[BitCount * 2];
        for (int i = 0; i < BitCount; i++)
        {
            int b = BinaryEncoding.DecodeBit(right, i);
            if (b == 0)
                continue;

            for (int j = 0; j < BitCount; j++)
            {
                int a = BinaryEncoding.DecodeBit(left, j);
                if (a == 0)
                    continue;
                digits[i + j] += a * b;
            }
        }

        return NormalizeAndPack(digits, BitCount);
    }

    private static (ulong Quotient, ulong Remainder) DivRemPacked(ulong dividend, ulong divisor)
    {
        if (BinaryEncoding.Compare(dividend, divisor, BitCount) < 0)
            return (0UL, dividend);

        ulong remainder = dividend;
        ulong quotient = 0UL;
        int highestDividend = BinaryEncoding.HighestNonZeroBit(dividend, BitCount);
        int highestDivisor = BinaryEncoding.HighestNonZeroBit(divisor, BitCount);
        int shift = highestDividend - highestDivisor;

        while (shift >= 0)
        {
            ulong shiftedDivisor = ShiftLeftPacked(divisor, shift);
            if (BinaryEncoding.Compare(remainder, shiftedDivisor, BitCount) >= 0)
            {
                remainder = SubtractPacked(remainder, shiftedDivisor);
                quotient = BinaryEncoding.SetBit(quotient, shift, 1);
            }
            shift--;
        }

        return (quotient, remainder);
    }

    private static ulong ShiftLeftPacked(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftRightPacked(packed, -count);
        if (count >= BitCount)
            return 0UL;

        Span<sbyte> digits = stackalloc sbyte[BitCount];
        BinaryEncoding.Decode(packed, digits, BitCount);
        for (int i = BitCount - 1; i >= count; i--)
        {
            digits[i] = digits[i - count];
        }

        for (int i = 0; i < count; i++)
        {
            digits[i] = 0;
        }

        return BinaryEncoding.Encode(digits, BitCount);
    }

    private static ulong ShiftRightPacked(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftLeftPacked(packed, -count);
        if (count >= BitCount)
            return 0UL;

        Span<sbyte> digits = stackalloc sbyte[BitCount];
        BinaryEncoding.Decode(packed, digits, BitCount);
        for (int i = 0; i < BitCount - count; i++)
        {
            digits[i] = digits[i + count];
        }

        for (int i = BitCount - count; i < BitCount; i++)
        {
            digits[i] = 0;
        }

        return BinaryEncoding.Encode(digits, BitCount);
    }

    private static ulong AndPacked(ulong left, ulong right)
    {
        Span<sbyte> digits = stackalloc sbyte[BitCount];
        for (int i = 0; i < BitCount; i++)
        {
            digits[i] = (sbyte)(BinaryEncoding.DecodeBit(left, i) == 1 && BinaryEncoding.DecodeBit(right, i) == 1 ? 1 : 0);
        }
        return BinaryEncoding.Encode(digits, BitCount);
    }

    private static ulong OrPacked(ulong left, ulong right)
    {
        Span<sbyte> digits = stackalloc sbyte[BitCount];
        for (int i = 0; i < BitCount; i++)
        {
            digits[i] = (sbyte)(BinaryEncoding.DecodeBit(left, i) == 1 || BinaryEncoding.DecodeBit(right, i) == 1 ? 1 : 0);
        }
        return BinaryEncoding.Encode(digits, BitCount);
    }

    private static ulong XorPacked(ulong left, ulong right)
    {
        Span<sbyte> digits = stackalloc sbyte[BitCount];
        for (int i = 0; i < BitCount; i++)
        {
            digits[i] = (sbyte)(BinaryEncoding.DecodeBit(left, i) != BinaryEncoding.DecodeBit(right, i) ? 1 : 0);
        }
        return BinaryEncoding.Encode(digits, BitCount);
    }

    private static ulong NotPacked(ulong value)
    {
        Span<sbyte> digits = stackalloc sbyte[BitCount];
        for (int i = 0; i < BitCount; i++)
        {
            digits[i] = (sbyte)(BinaryEncoding.DecodeBit(value, i) == 1 ? 0 : 1);
        }
        return BinaryEncoding.Encode(digits, BitCount);
    }

    private static ulong NormalizeAndPack(Span<int> digits, int count)
    {
        int carry = 0;
        for (int i = 0; i < count; i++)
        {
            int total = digits[i] + carry;
            int remainder = total % 2;
            if (remainder < 0)
                remainder += 2;
            int adjusted = total - remainder;
            carry = adjusted / 2;
            digits[i] = remainder;
        }

        for (int i = count; i < digits.Length; i++)
        {
            int total = digits[i] + carry;
            int remainder = total % 2;
            if (remainder < 0)
                remainder += 2;
            int adjusted = total - remainder;
            carry = adjusted / 2;
            if (remainder != 0)
                throw new OverflowException("Binary overflow beyond byte capacity.");
        }

        if (carry != 0)
            throw new OverflowException("Binary overflow beyond byte capacity.");

        Span<sbyte> normalized = stackalloc sbyte[count];
        for (int i = 0; i < count; i++)
        {
            normalized[i] = (sbyte)digits[i];
        }

        return BinaryEncoding.Encode(normalized, count);
    }
}
