using System;

namespace stdTernary;

public readonly struct UFloatT : IEquatable<UFloatT>, IComparable<UFloatT>
{
    public const int TotalTritCount = 32;
    public const int SignificandTritCount = 26;
    public const int ExponentTritCount = TotalTritCount - SignificandTritCount;

    private const int WorkingTritCount = SignificandTritCount * 2 + 4;
    private static readonly int ExponentBias = ComputeMaxExponent() / 2;
    private static readonly int MaxExponent = ComputeMaxExponent() - ExponentBias;
    private static readonly int MinExponent = -ExponentBias;
    private static readonly ulong SignificandMask = (1UL << (SignificandTritCount * 2)) - 1UL;

    private readonly ulong _packed;

    private UFloatT(ulong packed)
    {
        _packed = packed;
    }

    private UFloatT(int exponent, ulong significand)
    {
        _packed = Compose(exponent, significand);
    }

    public static UFloatT Zero { get; } = new UFloatT(0UL);

    public int Exponent => ExtractExponent(_packed);

    public ulong SignificandPacked => _packed & SignificandMask;

    public bool IsZero => SignificandPacked == 0UL;

    public string SignificandString => UnbalancedTernaryEncoding.ToTernaryString(SignificandPacked, SignificandTritCount);

    public override string ToString() => IsZero ? "0" : $"{SignificandString} x 3^{Exponent}";

    public static implicit operator UFloatT(double value) => FromDouble(value);
    public static implicit operator double(UFloatT value) => value.ToDouble();
    public static explicit operator UFloatT(FloatT value) => FromDouble(value.ToDouble());
    public static explicit operator FloatT(UFloatT value) => FloatT.FromDouble(value.ToDouble());

    public double ToDouble()
    {
        if (IsZero)
            return 0.0;

        double significand = UnbalancedTernaryEncoding.ToUInt64(SignificandPacked, SignificandTritCount);
        return significand * Math.Pow(3.0, Exponent);
    }

    public static UFloatT FromDouble(double value)
    {
        if (value == 0.0)
            return Zero;
        if (!double.IsFinite(value) || value < 0.0)
            throw new ArgumentOutOfRangeException(nameof(value), "UFloatT requires a finite non-negative value.");

        int exponent = 0;
        double magnitude = value;
        double high = Math.Pow(3.0, SignificandTritCount - 1);
        double low = Math.Pow(3.0, SignificandTritCount - 2);

        while (magnitude >= high)
        {
            magnitude /= 3.0;
            exponent++;
        }

        while (magnitude < low)
        {
            magnitude *= 3.0;
            exponent--;
        }

        ulong rounded = (ulong)Math.Round(magnitude, MidpointRounding.ToZero);
        ulong significand = UnbalancedTernaryEncoding.FromUInt64(rounded, SignificandTritCount);
        return new UFloatT(exponent, significand);
    }

    public UIntT ToUIntT()
    {
        if (IsZero)
            return UIntT.Zero;

        UIntT value = new UIntT(UnbalancedTernaryEncoding.ToUInt64(SignificandPacked, SignificandTritCount));
        if (Exponent > 0)
            value <<= Exponent;
        else if (Exponent < 0)
            value >>= -Exponent;
        return value;
    }

    public static UFloatT FromUInt(UIntT value)
    {
        if (value == UIntT.Zero)
            return Zero;

        int msb = UnbalancedTernaryEncoding.HighestNonZeroTrit(value.Packed, UIntT.TritCount);
        int exponent = msb - (SignificandTritCount - 1);
        UIntT mantissa = exponent >= 0 ? (value >> exponent) : (value << -exponent);
        ulong significand = UnbalancedTernaryEncoding.FromUInt64(mantissa.ToUInt64(), SignificandTritCount);
        return new UFloatT(exponent, significand);
    }

    public UFloatT Normalize()
    {
        int exponent = Exponent;
        ulong significand = SignificandPacked;
        Normalize(ref exponent, ref significand);
        return new UFloatT(Compose(exponent, significand));
    }

    internal UFloatT AdjustByTrit(int delta)
    {
        if (delta == 0)
            return this;

        int exponent = Exponent;
        Span<int> digits = stackalloc int[WorkingTritCount];
        LoadDigits(SignificandPacked, digits);
        digits[0] += delta;
        NormalizeDigits(digits);
        EnsureNonNegative(digits);
        return CreateFromDigits(digits, exponent);
    }

    public bool Equals(UFloatT other) => _packed == other._packed;
    public override bool Equals(object? obj) => obj is UFloatT other && Equals(other);
    public override int GetHashCode() => _packed.GetHashCode();

    public int CompareTo(UFloatT other)
    {
        if (IsZero && other.IsZero)
            return 0;
        if (IsZero)
            return -1;
        if (other.IsZero)
            return 1;
        if (Exponent != other.Exponent)
            return Exponent > other.Exponent ? 1 : -1;
        return UnbalancedTernaryEncoding.Compare(SignificandPacked, other.SignificandPacked, SignificandTritCount);
    }

    public int CompareTo(FloatT other) => ToDouble().CompareTo(other.ToDouble());

    public static bool operator ==(UFloatT left, UFloatT right) => left.Equals(right);
    public static bool operator !=(UFloatT left, UFloatT right) => !left.Equals(right);
    public static bool operator <(UFloatT left, UFloatT right) => left.CompareTo(right) < 0;
    public static bool operator >(UFloatT left, UFloatT right) => left.CompareTo(right) > 0;
    public static bool operator <=(UFloatT left, UFloatT right) => left.CompareTo(right) <= 0;
    public static bool operator >=(UFloatT left, UFloatT right) => left.CompareTo(right) >= 0;
    public static bool operator <(UFloatT left, FloatT right) => left.CompareTo(right) < 0;
    public static bool operator >(UFloatT left, FloatT right) => left.CompareTo(right) > 0;
    public static bool operator <=(UFloatT left, FloatT right) => left.CompareTo(right) <= 0;
    public static bool operator >=(UFloatT left, FloatT right) => left.CompareTo(right) >= 0;

    public static UFloatT operator +(UFloatT left, UFloatT right) => Add(left, right);
    public static UFloatT operator -(UFloatT left, UFloatT right) => Subtract(left, right);
    public static UFloatT operator *(UFloatT left, UFloatT right) => Multiply(left, right);
    public static UFloatT operator /(UFloatT left, UFloatT right) => Divide(left, right);

    private static ulong Compose(int exponent, ulong significand)
    {
        Normalize(ref exponent, ref significand);

        ulong packed = significand & SignificandMask;
        ulong exponentPacked = UnbalancedTernaryEncoding.FromUInt64((ulong)(exponent + ExponentBias), ExponentTritCount);
        for (int i = 0; i < ExponentTritCount; i++)
        {
            sbyte digit = UnbalancedTernaryEncoding.DecodeTrit(exponentPacked, i);
            packed = UnbalancedTernaryEncoding.SetTrit(packed, SignificandTritCount + i, digit);
        }

        return packed;
    }

    private static UFloatT Add(UFloatT left, UFloatT right)
    {
        if (left.IsZero)
            return right;
        if (right.IsZero)
            return left;

        int targetExponent = Math.Max(left.Exponent, right.Exponent);
        Span<int> leftDigits = stackalloc int[WorkingTritCount];
        Span<int> rightDigits = stackalloc int[WorkingTritCount];
        LoadDigits(left.SignificandPacked, leftDigits);
        LoadDigits(right.SignificandPacked, rightDigits);
        AlignDigits(leftDigits, targetExponent - left.Exponent);
        AlignDigits(rightDigits, targetExponent - right.Exponent);

        Span<int> result = stackalloc int[WorkingTritCount];
        for (int i = 0; i < WorkingTritCount; i++)
        {
            result[i] = leftDigits[i] + rightDigits[i];
        }

        NormalizeDigits(result);
        return CreateFromDigits(result, targetExponent);
    }

    private static UFloatT Subtract(UFloatT left, UFloatT right)
    {
        if (right.IsZero)
            return left;
        if (left.CompareTo(right) < 0)
            throw new OverflowException("UFloatT subtraction cannot produce a negative value.");

        int targetExponent = Math.Max(left.Exponent, right.Exponent);
        Span<int> leftDigits = stackalloc int[WorkingTritCount];
        Span<int> rightDigits = stackalloc int[WorkingTritCount];
        LoadDigits(left.SignificandPacked, leftDigits);
        LoadDigits(right.SignificandPacked, rightDigits);
        AlignDigits(leftDigits, targetExponent - left.Exponent);
        AlignDigits(rightDigits, targetExponent - right.Exponent);

        Span<int> result = stackalloc int[WorkingTritCount];
        for (int i = 0; i < WorkingTritCount; i++)
        {
            result[i] = leftDigits[i] - rightDigits[i];
        }

        NormalizeDigits(result);
        EnsureNonNegative(result);
        return CreateFromDigits(result, targetExponent);
    }

    private static UFloatT Multiply(UFloatT left, UFloatT right)
    {
        if (left.IsZero || right.IsZero)
            return Zero;

        Span<int> leftDigits = stackalloc int[WorkingTritCount];
        Span<int> rightDigits = stackalloc int[WorkingTritCount];
        LoadDigits(left.SignificandPacked, leftDigits);
        LoadDigits(right.SignificandPacked, rightDigits);

        Span<int> product = stackalloc int[WorkingTritCount];
        for (int i = 0; i < SignificandTritCount; i++)
        {
            for (int j = 0; j < SignificandTritCount; j++)
            {
                product[i + j] += leftDigits[i] * rightDigits[j];
            }
        }

        NormalizeDigits(product);
        return CreateFromDigits(product, checked(left.Exponent + right.Exponent));
    }

    private static UFloatT Divide(UFloatT left, UFloatT right)
    {
        if (right.IsZero)
            throw new DivideByZeroException("Attempted to divide by zero UFloatT.");
        if (left.IsZero)
            return Zero;

        Span<int> numerator = stackalloc int[WorkingTritCount];
        Span<int> denominator = stackalloc int[WorkingTritCount];
        LoadDigits(left.SignificandPacked, numerator);
        LoadDigits(right.SignificandPacked, denominator);
        ShiftLeftDigits(numerator, SignificandTritCount);

        Span<int> quotient = stackalloc int[WorkingTritCount];
        DividePositiveDigits(numerator, denominator, quotient);
        NormalizeDigits(quotient);

        int exponent = left.Exponent - right.Exponent - SignificandTritCount;
        return CreateFromDigits(quotient, exponent);
    }

    private static int ExtractExponent(ulong packed)
    {
        ulong exponentPacked = 0UL;
        for (int i = 0; i < ExponentTritCount; i++)
        {
            sbyte digit = UnbalancedTernaryEncoding.DecodeTrit(packed, SignificandTritCount + i);
            exponentPacked = UnbalancedTernaryEncoding.SetTrit(exponentPacked, i, digit);
        }

        return checked((int)UnbalancedTernaryEncoding.ToUInt64(exponentPacked, ExponentTritCount)) - ExponentBias;
    }

    private static void Normalize(ref int exponent, ref ulong significand)
    {
        if (UnbalancedTernaryEncoding.IsZero(significand, SignificandTritCount))
        {
            exponent = 0;
            significand = 0;
            return;
        }

        int msb = UnbalancedTernaryEncoding.HighestNonZeroTrit(significand, SignificandTritCount);
        int shift = SignificandTritCount - 1 - msb;

        if (shift > 0)
        {
            significand = ShiftLeftSignificand(significand, shift);
            exponent -= shift;
        }
        else if (shift < 0)
        {
            significand = ShiftRightSignificand(significand, -shift);
            exponent += -shift;
        }

        if (exponent > MaxExponent || exponent < MinExponent)
            throw new OverflowException("Exponent out of range for UFloatT.");
    }

    private static ulong ShiftLeftSignificand(ulong packed, int count) => ShiftSignificand(packed, count, true);
    private static ulong ShiftRightSignificand(ulong packed, int count) => ShiftSignificand(packed, count, false);

    private static ulong ShiftSignificand(ulong packed, int count, bool left)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftSignificand(packed, -count, !left);
        if (count >= SignificandTritCount)
            return 0UL;

        Span<sbyte> digits = stackalloc sbyte[SignificandTritCount];
        Span<sbyte> shifted = stackalloc sbyte[SignificandTritCount];
        UnbalancedTernaryEncoding.Decode(packed, digits, SignificandTritCount);
        if (left)
        {
            for (int i = count; i < SignificandTritCount; i++)
                shifted[i] = digits[i - count];
        }
        else
        {
            for (int i = 0; i < SignificandTritCount - count; i++)
                shifted[i] = digits[i + count];
        }
        return UnbalancedTernaryEncoding.Encode(shifted, SignificandTritCount);
    }

    private static void LoadDigits(ulong packed, Span<int> destination)
    {
        destination.Clear();
        Span<sbyte> digits = stackalloc sbyte[SignificandTritCount];
        UnbalancedTernaryEncoding.Decode(packed, digits, SignificandTritCount);
        for (int i = 0; i < SignificandTritCount; i++)
        {
            destination[i] = digits[i];
        }
    }

    private static void AlignDigits(Span<int> digits, int shift)
    {
        if (shift > 0)
            ShiftRightDigits(digits, shift);
        else if (shift < 0)
            ShiftLeftDigits(digits, -shift);
    }

    private static void ShiftLeftDigits(Span<int> digits, int count)
    {
        if (count <= 0)
        {
            if (count < 0)
                ShiftRightDigits(digits, -count);
            return;
        }
        if (count >= digits.Length)
        {
            if (!DigitsAreZero(digits))
                throw new OverflowException("Unbalanced ternary shift left overflow.");
            digits.Clear();
            return;
        }
        for (int i = digits.Length - 1; i >= count; i--)
            digits[i] = digits[i - count];
        for (int i = 0; i < count; i++)
            digits[i] = 0;
    }

    private static void ShiftRightDigits(Span<int> digits, int count)
    {
        if (count <= 0)
        {
            if (count < 0)
                ShiftLeftDigits(digits, -count);
            return;
        }
        if (count >= digits.Length)
        {
            digits.Clear();
            return;
        }
        for (int i = 0; i < digits.Length - count; i++)
            digits[i] = digits[i + count];
        for (int i = digits.Length - count; i < digits.Length; i++)
            digits[i] = 0;
    }

    private static void NormalizeDigits(Span<int> digits)
    {
        int carry = 0;
        for (int i = 0; i < digits.Length; i++)
        {
            int total = digits[i] + carry;
            carry = Math.DivRem(total, 3, out int digit);
            while (digit < 0)
            {
                digit += 3;
                carry--;
            }
            digits[i] = digit;
        }

        if (carry != 0)
            throw new OverflowException("Unbalanced ternary normalization overflowed working precision.");
    }

    private static int HighestNonZero(ReadOnlySpan<int> digits)
    {
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            if (digits[i] != 0)
                return i;
        }
        return -1;
    }

    private static int CompareDigits(ReadOnlySpan<int> left, ReadOnlySpan<int> right)
    {
        int leftMsb = HighestNonZero(left);
        int rightMsb = HighestNonZero(right);
        if (leftMsb != rightMsb)
            return leftMsb > rightMsb ? 1 : -1;
        for (int i = leftMsb; i >= 0; i--)
        {
            if (left[i] != right[i])
                return left[i] > right[i] ? 1 : -1;
        }
        return 0;
    }

    private static void SubtractDigits(Span<int> left, ReadOnlySpan<int> right)
    {
        for (int i = 0; i < left.Length; i++)
            left[i] -= right[i];
        NormalizeDigits(left);
    }

    private static bool DigitsAreZero(ReadOnlySpan<int> digits)
    {
        foreach (int digit in digits)
        {
            if (digit != 0)
                return false;
        }
        return true;
    }

    private static void DividePositiveDigits(Span<int> remainder, ReadOnlySpan<int> divisor, Span<int> quotient)
    {
        Span<int> shiftedDivisor = stackalloc int[WorkingTritCount];
        Span<int> divisorCopy = stackalloc int[WorkingTritCount];
        divisor.CopyTo(divisorCopy);
        NormalizeDigits(remainder);
        NormalizeDigits(divisorCopy);

        while (CompareDigits(remainder, divisorCopy) >= 0)
        {
            int shift = HighestNonZero(remainder) - HighestNonZero(divisorCopy);
            shiftedDivisor.Clear();
            divisorCopy.CopyTo(shiftedDivisor);
            ShiftLeftDigits(shiftedDivisor, shift);

            if (CompareDigits(remainder, shiftedDivisor) < 0)
            {
                shift--;
                if (shift < 0)
                    break;
                shiftedDivisor.Clear();
                divisorCopy.CopyTo(shiftedDivisor);
                ShiftLeftDigits(shiftedDivisor, shift);
            }

            SubtractDigits(remainder, shiftedDivisor);
            quotient[shift] += 1;
            NormalizeDigits(quotient);
        }
    }

    private static void EnsureNonNegative(ReadOnlySpan<int> digits)
    {
        for (int i = 0; i < digits.Length; i++)
        {
            if (digits[i] < 0)
                throw new OverflowException("UFloatT cannot represent negative values.");
        }
    }

    private static UFloatT CreateFromDigits(Span<int> digits, int exponent)
    {
        NormalizeDigits(digits);
        EnsureNonNegative(digits);

        int highest = HighestNonZero(digits);
        if (highest < 0)
            return Zero;

        int desired = SignificandTritCount - 1;
        if (highest > desired)
        {
            int shift = highest - desired;
            ShiftRightDigits(digits, shift);
            exponent = checked(exponent + shift);
        }
        else if (highest < desired)
        {
            int shift = desired - highest;
            ShiftLeftDigits(digits, shift);
            exponent = checked(exponent - shift);
        }

        if (exponent > MaxExponent || exponent < MinExponent)
            throw new OverflowException("Exponent out of range for UFloatT.");

        Span<sbyte> packedDigits = stackalloc sbyte[SignificandTritCount];
        for (int i = 0; i < SignificandTritCount; i++)
            packedDigits[i] = (sbyte)digits[i];

        return new UFloatT(exponent, UnbalancedTernaryEncoding.Encode(packedDigits, SignificandTritCount));
    }

    private static int ComputeMaxExponent()
    {
        int value = 1;
        for (int i = 0; i < ExponentTritCount; i++)
            value = checked(value * 3);
        return value - 1;
    }
}
