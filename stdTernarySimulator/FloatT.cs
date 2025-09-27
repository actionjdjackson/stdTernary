using System;

namespace stdTernary;

public readonly struct FloatT : IEquatable<FloatT>, IComparable<FloatT>
{
    public const int TotalTritCount = 32;
    public const int SignificandTritCount = 26;
    public const int ExponentTritCount = TotalTritCount - SignificandTritCount;

    private const int WorkingTritCount = SignificandTritCount * 2 + 4;

    private static readonly int MaxExponent = ComputeMaxExponent();
    private static readonly int MinExponent = -MaxExponent;
    private static readonly ulong SignificandMask = (SignificandTritCount >= 32)
        ? ulong.MaxValue
        : (1UL << (SignificandTritCount * 2)) - 1UL;

    private readonly ulong _packed;

    private FloatT(ulong packed)
    {
        _packed = packed;
    }

    private FloatT(int exponent, ulong significand)
    {
        _packed = Compose(exponent, significand);
    }

    public static FloatT Zero { get; } = new FloatT(0UL);

    public int Exponent => ExtractExponent(_packed);

    public ulong SignificandPacked => _packed & SignificandMask;

    public bool IsZero => SignificandPacked == 0UL;

    public string SignificandString => BalancedTernaryEncoding.ToTernaryString(SignificandPacked, SignificandTritCount);

    public override string ToString()
    {
        if (IsZero)
            return "0";

        return $"{SignificandString} × 3^{Exponent}";
    }

    public static implicit operator FloatT(double value) => FromDouble(value);
    public static implicit operator double(FloatT value) => value.ToDouble();

    public double ToDouble()
    {
        if (IsZero)
            return 0.0;

        double significand = BalancedTernaryEncoding.ToInt64(SignificandPacked, SignificandTritCount);
        return significand * Math.Pow(3.0, Exponent);
    }

    public static FloatT FromDouble(double value)
    {
        if (value == 0.0)
            return Zero;

        int exponent = 0;
        double magnitude = Math.Abs(value);
        if (!double.IsFinite(magnitude))
            throw new ArgumentOutOfRangeException(nameof(value), "Cannot convert non-finite value to FloatT.");

        while (magnitude >= Math.Pow(3.0, SignificandTritCount - 1))
        {
            magnitude /= 3.0;
            exponent++;
        }

        while (magnitude < Math.Pow(3.0, SignificandTritCount - 2))
        {
            magnitude *= 3.0;
            exponent--;
        }

        long rounded = (long)Math.Round(magnitude, MidpointRounding.ToZero);
        ulong significand = BalancedTernaryEncoding.FromInt64(Math.Sign(value) < 0 ? -rounded : rounded, SignificandTritCount);

        return new FloatT(exponent, significand);
    }

    public IntT ToIntT()
    {
        if (IsZero)
            return IntT.Zero;

        long significand = BalancedTernaryEncoding.ToInt64(SignificandPacked, SignificandTritCount);
        IntT value = new IntT(significand);

        if (Exponent > 0)
        {
            value = value << Exponent;
        }
        else if (Exponent < 0)
        {
            value = value >> (-Exponent);
        }

        return value;
    }

    public static FloatT FromInt(IntT value)
    {
        if (value == IntT.Zero)
            return Zero;

        ulong packed = value.Packed;
        int msb = BalancedTernaryEncoding.HighestNonZeroTrit(packed, IntT.TritCount);
        if (msb < 0)
            return Zero;

        int exponent = msb - (SignificandTritCount - 1);
        IntT mantissa = exponent >= 0 ? (value >> exponent) : (value << (-exponent));
        ulong significand = BalancedTernaryEncoding.FromInt64(mantissa.ToInt64(), SignificandTritCount);

        return new FloatT(exponent, significand);
    }

    public FloatT Normalize()
    {
        int exponent = Exponent;
        ulong significand = SignificandPacked;
        Normalize(ref exponent, ref significand);
        return new FloatT(Compose(exponent, significand));
    }

    public FloatT Negate()
    {
        if (IsZero)
            return this;

        Span<sbyte> digits = stackalloc sbyte[SignificandTritCount];
        BalancedTernaryEncoding.Decode(SignificandPacked, digits, SignificandTritCount);
        for (int i = 0; i < SignificandTritCount; i++)
        {
            digits[i] = (sbyte)(-digits[i]);
        }

        ulong negSignificand = BalancedTernaryEncoding.Encode(digits, SignificandTritCount);
        return new FloatT(Compose(Exponent, negSignificand));
    }

    public bool Equals(FloatT other) => _packed == other._packed;

    public override bool Equals(object? obj) => obj is FloatT other && Equals(other);

    public override int GetHashCode() => _packed.GetHashCode();

    public int CompareTo(FloatT other)
    {
        if (IsZero && other.IsZero)
            return 0;
        if (IsZero)
            return Math.Sign(-other.Exponent);
        if (other.IsZero)
            return Math.Sign(Exponent);

        if (Exponent != other.Exponent)
            return Exponent > other.Exponent ? 1 : -1;

        return BalancedTernaryEncoding.Compare(SignificandPacked, other.SignificandPacked, SignificandTritCount);
    }

    public static bool operator ==(FloatT left, FloatT right) => left.Equals(right);
    public static bool operator !=(FloatT left, FloatT right) => !left.Equals(right);
    public static bool operator <(FloatT left, FloatT right) => left.CompareTo(right) < 0;
    public static bool operator >(FloatT left, FloatT right) => left.CompareTo(right) > 0;
    public static bool operator <=(FloatT left, FloatT right) => left.CompareTo(right) <= 0;
    public static bool operator >=(FloatT left, FloatT right) => left.CompareTo(right) >= 0;

    public static FloatT operator +(FloatT left, FloatT right) => Add(left, right);
    public static FloatT operator -(FloatT left, FloatT right) => Subtract(left, right);
    public static FloatT operator *(FloatT left, FloatT right) => Multiply(left, right);
    public static FloatT operator /(FloatT left, FloatT right) => Divide(left, right);

    public static FloatT operator -(FloatT value) => value.Negate();

    private static ulong Compose(int exponent, ulong significand)
    {
        Normalize(ref exponent, ref significand);

        ulong packed = significand & SignificandMask;
        ulong exponentPacked = BalancedTernaryEncoding.FromInt64(exponent, ExponentTritCount);

        for (int i = 0; i < ExponentTritCount; i++)
        {
            sbyte digit = BalancedTernaryEncoding.DecodeTrit(exponentPacked, i);
            packed = BalancedTernaryEncoding.SetTrit(packed, SignificandTritCount + i, digit);
        }

        return packed;
    }

    private static FloatT Add(FloatT left, FloatT right)
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

    private static FloatT Subtract(FloatT left, FloatT right)
    {
        if (right.IsZero)
            return left;
        if (left.IsZero)
            return right.Negate();

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
        return CreateFromDigits(result, targetExponent);
    }

    private static FloatT Multiply(FloatT left, FloatT right)
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
        int exponent = checked(left.Exponent + right.Exponent);
        return CreateFromDigits(product, exponent);
    }

    private static FloatT Divide(FloatT left, FloatT right)
    {
        if (right.IsZero)
            throw new DivideByZeroException("Attempted to divide by zero FloatT.");
        if (left.IsZero)
            return Zero;

        int leftSign = SignOfSignificand(left.SignificandPacked);
        int rightSign = SignOfSignificand(right.SignificandPacked);
        int resultSign = leftSign * rightSign;

        Span<int> numerator = stackalloc int[WorkingTritCount];
        Span<int> denominator = stackalloc int[WorkingTritCount];
        LoadDigits(left.SignificandPacked, numerator);
        LoadDigits(right.SignificandPacked, denominator);

        if (leftSign < 0)
            ApplySign(numerator, -1);
        if (rightSign < 0)
            ApplySign(denominator, -1);

        NormalizeDigits(numerator);
        NormalizeDigits(denominator);

        if (DigitsAreZero(denominator))
            throw new DivideByZeroException("Attempted to divide by zero FloatT.");

        ShiftLeftDigits(numerator, SignificandTritCount);

        Span<int> quotient = stackalloc int[WorkingTritCount];
        quotient.Clear();

        DividePositiveDigits(numerator, denominator, quotient);

        if (resultSign < 0)
        {
            ApplySign(quotient, -1);
        }

        NormalizeDigits(quotient);

        int exponent = left.Exponent - right.Exponent - SignificandTritCount;
        return CreateFromDigits(quotient, exponent);
    }

    private static int ExtractExponent(ulong packed)
    {
        long exponent = 0;
        for (int i = SignificandTritCount + ExponentTritCount - 1; i >= SignificandTritCount; i--)
        {
            exponent = exponent * 3 + BalancedTernaryEncoding.DecodeTrit(packed, i);
        }
        return (int)exponent;
    }

    private static void Normalize(ref int exponent, ref ulong significand)
    {
        if (BalancedTernaryEncoding.IsZero(significand, SignificandTritCount))
        {
            exponent = 0;
            significand = 0;
            return;
        }

        int msb = BalancedTernaryEncoding.HighestNonZeroTrit(significand, SignificandTritCount);
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
            throw new OverflowException("Exponent out of range for FloatT.");
    }

    private static ulong ShiftLeftSignificand(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftRightSignificand(packed, -count);
        if (count >= SignificandTritCount)
        {
            if (!BalancedTernaryEncoding.IsZero(packed, SignificandTritCount))
                throw new OverflowException("Significand shift overflow.");
            return 0UL;
        }

        Span<sbyte> digits = stackalloc sbyte[SignificandTritCount];
        BalancedTernaryEncoding.Decode(packed, digits, SignificandTritCount);

        for (int i = SignificandTritCount - count; i < SignificandTritCount; i++)
        {
            if (digits[i] != 0)
                throw new OverflowException("Significand shift overflow.");
        }

        for (int i = SignificandTritCount - 1; i >= count; i--)
        {
            digits[i] = digits[i - count];
        }

        for (int i = 0; i < count; i++)
        {
            digits[i] = 0;
        }

        return BalancedTernaryEncoding.Encode(digits, SignificandTritCount);
    }

    private static ulong ShiftRightSignificand(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftLeftSignificand(packed, -count);
        if (count >= SignificandTritCount)
            return 0UL;

        Span<sbyte> digits = stackalloc sbyte[SignificandTritCount];
        BalancedTernaryEncoding.Decode(packed, digits, SignificandTritCount);

        for (int i = 0; i < SignificandTritCount - count; i++)
        {
            digits[i] = digits[i + count];
        }

        for (int i = SignificandTritCount - count; i < SignificandTritCount; i++)
        {
            digits[i] = 0;
        }

        return BalancedTernaryEncoding.Encode(digits, SignificandTritCount);
    }

    private static void LoadDigits(ulong packed, Span<int> destination)
    {
        destination.Clear();
        Span<sbyte> digits = stackalloc sbyte[SignificandTritCount];
        BalancedTernaryEncoding.Decode(packed, digits, SignificandTritCount);
        for (int i = 0; i < SignificandTritCount; i++)
        {
            destination[i] = digits[i];
        }
    }

    private static void AlignDigits(Span<int> digits, int shift)
    {
        if (shift > 0)
        {
            ShiftRightDigits(digits, shift);
        }
        else if (shift < 0)
        {
            ShiftLeftDigits(digits, -shift);
        }
    }

    private static void ShiftLeftDigits(Span<int> digits, int count)
    {
        if (count <= 0)
        {
            if (count < 0)
                ShiftRightDigits(digits, -count);
            return;
        }

        int length = digits.Length;
        if (count >= length)
        {
            if (!DigitsAreZero(digits))
                throw new OverflowException("Ternary shift left overflow.");
            digits.Clear();
            return;
        }
        for (int i = length - count; i < length; i++)
        {
            if (digits[i] != 0)
                throw new OverflowException("Ternary shift left overflow.");
        }

        for (int i = length - 1; i >= count; i--)
        {
            digits[i] = digits[i - count];
        }

        for (int i = 0; i < count; i++)
        {
            digits[i] = 0;
        }
    }

    private static void ShiftRightDigits(Span<int> digits, int count)
    {
        if (count <= 0)
        {
            if (count < 0)
                ShiftLeftDigits(digits, -count);
            return;
        }

        int length = digits.Length;
        if (count >= length)
        {
            digits.Clear();
            return;
        }

        for (int i = 0; i < length - count; i++)
        {
            digits[i] = digits[i + count];
        }

        for (int i = length - count; i < length; i++)
        {
            digits[i] = 0;
        }
    }

    private static void NormalizeDigits(Span<int> digits)
    {
        int carry = 0;
        for (int i = 0; i < digits.Length; i++)
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

            digits[i] = total;
        }

        if (carry != 0)
            throw new OverflowException("Balanced ternary normalization overflowed working precision.");
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
            int l = left[i];
            int r = right[i];
            if (l != r)
                return l > r ? 1 : -1;
        }

        return 0;
    }

    private static void SubtractDigits(Span<int> left, ReadOnlySpan<int> right)
    {
        for (int i = 0; i < left.Length; i++)
        {
            left[i] -= right[i];
        }
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
            int remainderMsb = HighestNonZero(remainder);
            int divisorMsb = HighestNonZero(divisorCopy);
            int shift = remainderMsb - divisorMsb;

            divisorCopy.CopyTo(shiftedDivisor);
            ShiftLeftDigits(shiftedDivisor, shift);

            if (CompareDigits(remainder, shiftedDivisor) < 0)
            {
                shift--;
                if (shift < 0)
                    break;
                divisorCopy.CopyTo(shiftedDivisor);
                ShiftLeftDigits(shiftedDivisor, shift);
            }

            SubtractDigits(remainder, shiftedDivisor);
            NormalizeDigits(remainder);

            quotient[shift] += 1;
        }

        NormalizeDigits(quotient);
    }

    private static void ApplySign(Span<int> digits, int sign)
    {
        if (sign == 1)
            return;
        if (sign != -1)
            throw new ArgumentException("Sign must be -1 or 1", nameof(sign));

        for (int i = 0; i < digits.Length; i++)
        {
            digits[i] = -digits[i];
        }
    }

    private static int SignOfSignificand(ulong packed)
    {
        int msb = BalancedTernaryEncoding.HighestNonZeroTrit(packed, SignificandTritCount);
        if (msb < 0)
            return 0;
        sbyte trit = BalancedTernaryEncoding.DecodeTrit(packed, msb);
        return trit > 0 ? 1 : -1;
    }

    private static FloatT CreateFromDigits(Span<int> digits, int exponent)
    {
        NormalizeDigits(digits);

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
            throw new OverflowException("Exponent out of range for FloatT.");

        Span<sbyte> packedDigits = stackalloc sbyte[SignificandTritCount];
        for (int i = 0; i < SignificandTritCount; i++)
        {
            packedDigits[i] = (sbyte)digits[i];
        }

        ulong packed = BalancedTernaryEncoding.Encode(packedDigits, SignificandTritCount);
        return new FloatT(exponent, packed);
    }

    private static int ComputeMaxExponent()
    {
        int value = 1;
        for (int i = 0; i < ExponentTritCount; i++)
        {
            value = checked(value * 3);
        }
        return (value - 1) / 2;
    }
}
