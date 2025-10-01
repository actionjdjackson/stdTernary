using System;
using System.Diagnostics.CodeAnalysis;

namespace stdTernary;

public readonly struct IntB : IEquatable<IntB>, IComparable<IntB>
{
    public const int BitCount = 64;
    private static readonly long MaxMagnitudeLong = long.MaxValue;

    private readonly sbyte _sign;
    private readonly ulong _magnitude;

    internal ulong Magnitude => _magnitude;

    private IntB(sbyte sign, ulong magnitude)
    {
        if (BinaryEncoding.IsZero(magnitude, BitCount))
        {
            _sign = 0;
            _magnitude = 0UL;
        }
        else
        {
            _sign = sign;
            _magnitude = magnitude;
        }
    }

    public IntB(long value)
    {
        if (value == 0)
        {
            _sign = 0;
            _magnitude = 0UL;
            return;
        }

        long magnitude = Math.Abs(value);
        _magnitude = BinaryEncoding.FromInt64(magnitude, BitCount);
        _sign = value > 0 ? (sbyte)1 : (sbyte)(-1);
    }

    public static IntB Zero { get; } = new IntB(0);
    public static IntB One { get; } = new IntB(1);
    public static IntB NegativeOne { get; } = new IntB(-1);
    public static IntB MaxValue { get; } = new IntB(MaxMagnitudeLong);
    public static IntB MinValue { get; } = new IntB(-MaxMagnitudeLong);

    public int Sign => _sign;

    public long ToInt64()
    {
        //long magnitude = BinaryEncoding.ToInt64(_magnitude, BitCount);
        int highest = BinaryEncoding.HighestNonZeroBit(_magnitude, BitCount);
        if (highest < 0) return 0; // all zero
        int activeCount = highest + 1;

        long magnitude = BinaryEncoding.ToInt64(_magnitude, activeCount);
        return _sign switch
        {
            0 => 0L,
            1 => magnitude,
            -1 => -magnitude,
            _ => 0L,
        };
    }

    public string BinaryString
    {
        get
        {
            if (_sign < 0)
                return "-" + BinaryEncoding.ToBinaryString(_magnitude, BitCount);
            if (_sign == 0)
                return new string('0', BitCount);
            return BinaryEncoding.ToBinaryString(_magnitude, BitCount);
        }
    }

    public override string ToString()
    {
        return $"{BinaryString} (base2) = {ToInt64()}";
    }

    public static IntB Parse(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        value = value.Trim();
        if (value.Length == 0)
            throw new FormatException("Empty binary string.");

        sbyte sign = 1;
        int index = 0;
        if (value[0] == '-')
        {
            sign = -1;
            index = 1;
        }
        else if (value[0] == '+')
        {
            sign = 1;
            index = 1;
        }

        int digitCount = value.Length - index;
        if (digitCount == 0)
            throw new FormatException("Missing digits in binary string.");
        if (digitCount > BitCount)
            throw new FormatException($"Binary string longer than {BitCount} bits.");

        Span<sbyte> digits = stackalloc sbyte[BitCount];
        int dest = 0;
        for (int i = value.Length - 1; i >= index; i--)
        {
            char c = value[i];
            digits[dest++] = c switch
            {
                '0' => 0,
                '1' => 1,
                _ => throw new FormatException($"Invalid binary digit '{c}'."),
            };
        }

        ulong packed = BinaryEncoding.Encode(digits, BitCount);
        return new IntB(sign, packed);
    }

    public static bool TryParse(string? value, out IntB result)
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

    public bool Equals(IntB other) => _sign == other._sign && _magnitude == other._magnitude;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is IntB other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_sign, _magnitude);

    public int CompareTo(IntB other)
    {
        if (_sign != other._sign)
            return _sign < other._sign ? -1 : 1;

        if (_sign == 0)
            return 0;

        int comparison = BinaryEncoding.Compare(_magnitude, other._magnitude, BitCount);
        return _sign > 0 ? comparison : -comparison;
    }

    public static bool operator ==(IntB left, IntB right) => left.Equals(right);
    public static bool operator !=(IntB left, IntB right) => !left.Equals(right);
    public static bool operator <(IntB left, IntB right) => left.CompareTo(right) < 0;
    public static bool operator <=(IntB left, IntB right) => left.CompareTo(right) <= 0;
    public static bool operator >(IntB left, IntB right) => left.CompareTo(right) > 0;
    public static bool operator >=(IntB left, IntB right) => left.CompareTo(right) >= 0;

    public static IntB operator +(IntB value) => value;

    public static IntB operator -(IntB value)
    {
        if (value._sign == 0)
            return value;
        return new IntB((sbyte)(-value._sign), value._magnitude);
    }

    public static IntB operator +(IntB left, IntB right)
    {
        if (left._sign == 0)
            return right;
        if (right._sign == 0)
            return left;

        if (left._sign == right._sign)
        {
            ulong sum = AddPacked(left._magnitude, right._magnitude);
            return new IntB(left._sign, sum);
        }

        int comparison = BinaryEncoding.Compare(left._magnitude, right._magnitude, BitCount);
        if (comparison == 0)
            return Zero;

        if (comparison > 0)
        {
            ulong diff = SubtractPacked(left._magnitude, right._magnitude);
            return new IntB(left._sign, diff);
        }
        else
        {
            ulong diff = SubtractPacked(right._magnitude, left._magnitude);
            return new IntB(right._sign, diff);
        }
    }

    public static IntB operator -(IntB left, IntB right) => left + (-right);

    public static IntB operator *(IntB left, IntB right)
    {
        if (left._sign == 0 || right._sign == 0)
            return Zero;

        ulong product = MultiplyPacked(left._magnitude, right._magnitude);
        sbyte sign = (sbyte)(left._sign == right._sign ? 1 : -1);
        return new IntB(sign, product);
    }

    public static IntB operator /(IntB left, IntB right)
    {
        var (quotient, _) = DivRem(left, right);
        return quotient;
    }

    public static IntB operator %(IntB left, IntB right)
    {
        var (_, remainder) = DivRem(left, right);
        return remainder;
    }

    public static IntB operator <<(IntB value, int count)
    {
        if (count == 0)
            return value;
        if (count < 0)
            return value >> (-count);

        ulong shifted = ShiftLeftPacked(value._magnitude, count);
        return new IntB(value._sign, shifted);
    }

    public static IntB operator >>(IntB value, int count)
    {
        if (count == 0)
            return value;
        if (count < 0)
            return value << (-count);

        ulong shifted = ShiftRightPacked(value._magnitude, count);
        return new IntB(value._sign, shifted);
    }

    public static IntB operator &(IntB left, IntB right)
    {
        ulong a = RequireNonNegativeMagnitude(left, "bitwise AND");
        ulong b = RequireNonNegativeMagnitude(right, "bitwise AND");
        ulong result = AndPacked(a, b);
        return new IntB(1, result);
    }

    public static IntB operator |(IntB left, IntB right)
    {
        ulong a = RequireNonNegativeMagnitude(left, "bitwise OR");
        ulong b = RequireNonNegativeMagnitude(right, "bitwise OR");
        ulong result = OrPacked(a, b);
        return new IntB(1, result);
    }

    public static IntB operator ^(IntB left, IntB right)
    {
        ulong a = RequireNonNegativeMagnitude(left, "bitwise XOR");
        ulong b = RequireNonNegativeMagnitude(right, "bitwise XOR");
        ulong result = XorPacked(a, b);
        return new IntB(1, result);
    }

    public static IntB operator ~(IntB value)
    {
        ulong magnitude = RequireNonNegativeMagnitude(value, "bitwise NOT");
        ulong flipped = NotPacked(magnitude);
        return new IntB(1, flipped);
    }

    public static implicit operator IntB(long value) => new IntB(value);
    public static explicit operator long(IntB value) => value.ToInt64();

    public IntB Abs() => _sign >= 0 ? this : -this;

    private static ulong RequireNonNegativeMagnitude(IntB value, string operation)
    {
        if (value._sign < 0)
            throw new InvalidOperationException($"{operation} is only defined for non-negative IntB values in this binary demonstration.");
        return value._magnitude;
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

    private static (IntB Quotient, IntB Remainder) DivRem(IntB left, IntB right)
    {
        if (right._sign == 0)
            throw new DivideByZeroException();
        if (left._sign == 0)
            return (Zero, Zero);

        var (quotientMag, remainderMag) = DivRemPacked(left._magnitude, right._magnitude);
        sbyte quotientSign = (sbyte)(left._sign == right._sign ? 1 : -1);
        sbyte remainderSign = left._sign;
        return (new IntB(quotientSign, quotientMag), new IntB(remainderSign, remainderMag));
    }

    private static (ulong Quotient, ulong Remainder) DivRemPacked(ulong dividend, ulong divisor)
    {
        if (BinaryEncoding.IsZero(divisor, BitCount))
            throw new DivideByZeroException();
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
        {
            if (!BinaryEncoding.IsZero(packed, BitCount))
                throw new OverflowException("Shift left overflow.");
            return 0UL;
        }

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

        // Ignore any overflow in digits beyond count
        // (we don't check them, just drop whatever ends up there).
        // Ignore any final carry as well.

        Span<sbyte> normalized = stackalloc sbyte[count];
        for (int i = 0; i < count; i++)
        {
            normalized[i] = (sbyte)digits[i];
        }

        return BinaryEncoding.Encode(normalized, count);
    }

}
