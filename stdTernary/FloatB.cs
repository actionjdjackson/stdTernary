using System;
using System.Diagnostics.CodeAnalysis;

namespace stdTernary;

public readonly struct FloatB : IEquatable<FloatB>, IComparable<FloatB>
{
    public const int SignificandBitCount = 32;

    private readonly IntB _mantissa;
    private readonly int _exponent;

    private FloatB(IntB mantissa, int exponent, bool alreadyNormalized)
    {
        _mantissa = mantissa;
        _exponent = exponent;
    }

    public FloatB(IntB mantissa, int exponent)
    {
        if (mantissa.Sign == 0)
        {
            _mantissa = IntB.Zero;
            _exponent = 0;
            return;
        }

        Normalize(ref mantissa, ref exponent);
        _mantissa = mantissa;
        _exponent = exponent;
    }

    public FloatB(long value) : this(new IntB(value), 0) { }

    public static FloatB Zero { get; } = new FloatB(IntB.Zero, 0, alreadyNormalized: true);

    public IntB Mantissa => _mantissa;
    public int Exponent => _exponent;
    public bool IsZero => _mantissa.Sign == 0;

    public double ToDouble()
    {
        if (IsZero)
            return 0.0;
        double mantissaValue = _mantissa.ToInt64();
        double scale = Pow2Double(_exponent);
        return mantissaValue * scale;
    }

    public static FloatB FromDouble(double value)
    {
        if (value == 0.0)
            return Zero;
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Cannot convert non-finite value to FloatB.");

        double magnitude = Math.Abs(value);
        int exponent = 0;

        while (magnitude >= 1.0)
        {
            magnitude *= 0.5;
            exponent++;
        }

        while (magnitude < 0.5)
        {
            magnitude *= 2.0;
            exponent--;
        }

        double scaled = magnitude * Pow2Double(SignificandBitCount);
        long rounded = (long)Math.Round(scaled, MidpointRounding.AwayFromZero);
        if (rounded == 0)
            return Zero;

        IntB mantissa = new IntB(rounded);
        if (value < 0)
            mantissa = -mantissa;

        exponent -= SignificandBitCount;
        return new FloatB(mantissa, exponent);
    }

    public IntB ToIntB()
    {
        if (IsZero)
            return IntB.Zero;

        IntB mantissa = _mantissa;
        if (_exponent > 0)
        {
            return mantissa << _exponent;
        }
        else if (_exponent < 0)
        {
            return mantissa >> (-_exponent);
        }

        return mantissa;
    }

    public static FloatB FromInt(IntB value) => new FloatB(value, 0);

    public bool Equals(FloatB other) => _mantissa.Equals(other._mantissa) && _exponent == other._exponent;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is FloatB other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_mantissa, _exponent);

    public int CompareTo(FloatB other)
    {
        if (IsZero && other.IsZero)
            return 0;
        if (IsZero)
            return other._mantissa.Sign > 0 ? -1 : 1;
        if (other.IsZero)
            return _mantissa.Sign > 0 ? 1 : -1;

        if (_mantissa.Sign != other._mantissa.Sign)
            return _mantissa.Sign < other._mantissa.Sign ? -1 : 1;

        int targetExponent = Math.Min(_exponent, other._exponent);
        IntB leftAligned = ScaleMantissa(_mantissa, _exponent - targetExponent);
        IntB rightAligned = ScaleMantissa(other._mantissa, other._exponent - targetExponent);
        int comparison = leftAligned.CompareTo(rightAligned);
        return _mantissa.Sign > 0 ? comparison : -comparison;
    }

    public override string ToString()
    {
        if (IsZero)
            return "0";
        return $"{_mantissa} × 2^{_exponent}";
    }

    public static FloatB operator +(FloatB left, FloatB right) => Add(left, right);
    public static FloatB operator -(FloatB left, FloatB right) => Subtract(left, right);
    public static FloatB operator *(FloatB left, FloatB right) => Multiply(left, right);
    public static FloatB operator /(FloatB left, FloatB right) => Divide(left, right);
    public static FloatB operator -(FloatB value) => new FloatB(-value._mantissa, value._exponent, alreadyNormalized: true);

    public static bool operator ==(FloatB left, FloatB right) => left.Equals(right);
    public static bool operator !=(FloatB left, FloatB right) => !left.Equals(right);
    public static bool operator <(FloatB left, FloatB right) => left.CompareTo(right) < 0;
    public static bool operator >(FloatB left, FloatB right) => left.CompareTo(right) > 0;
    public static bool operator <=(FloatB left, FloatB right) => left.CompareTo(right) <= 0;
    public static bool operator >=(FloatB left, FloatB right) => left.CompareTo(right) >= 0;

    public static implicit operator FloatB(double value) => FromDouble(value);
    public static implicit operator double(FloatB value) => value.ToDouble();

    private static FloatB Add(FloatB left, FloatB right)
    {
        if (left.IsZero)
            return right;
        if (right.IsZero)
            return left;

        int targetExponent = Math.Min(left._exponent, right._exponent);
        IntB leftMantissa = ScaleMantissa(left._mantissa, left._exponent - targetExponent);
        IntB rightMantissa = ScaleMantissa(right._mantissa, right._exponent - targetExponent);
        IntB sum = leftMantissa + rightMantissa;
        if (sum.Sign == 0)
            return Zero;
        return new FloatB(sum, targetExponent);
    }

    private static FloatB Subtract(FloatB left, FloatB right) => Add(left, new FloatB(-right._mantissa, right._exponent, alreadyNormalized: true));

    private static FloatB Multiply(FloatB left, FloatB right)
    {
        if (left.IsZero || right.IsZero)
            return Zero;

        IntB product = left._mantissa * right._mantissa;
        int exponent = left._exponent + right._exponent;
        return new FloatB(product, exponent);
    }

    private static FloatB Divide(FloatB left, FloatB right)
    {
        if (right.IsZero)
            throw new DivideByZeroException();
        if (left.IsZero)
            return Zero;

        IntB numerator = left._mantissa;
        IntB denominator = right._mantissa;
        int exponent = left._exponent - right._exponent;

        IntB quotient = numerator / denominator;
        IntB remainder = numerator % denominator;

        int shift = 0;
        while (remainder.Sign != 0 && shift < SignificandBitCount)
        {
            numerator = numerator << 1;
            exponent--;
            quotient = numerator / denominator;
            remainder = numerator % denominator;
            shift++;
        }

        if (remainder.Sign != 0)
            throw new InvalidOperationException("FloatB division cannot be represented exactly without native fractional support.");

        return new FloatB(quotient, exponent);
    }

    private static void Normalize(ref IntB mantissa, ref int exponent)
    {
        IntB absMantissa = mantissa.Abs();
        while (absMantissa.Sign != 0 && BinaryEncoding.DecodeBit(absMantissa.Magnitude, 0) == 0)
        {
            mantissa = mantissa >> 1;
            exponent++;
            absMantissa = mantissa.Abs();
        }
    }

    private static IntB ScaleMantissa(IntB mantissa, int shift)
    {
        if (mantissa.Sign == 0 || shift == 0)
            return mantissa;
        if (shift > 0)
            return mantissa << shift;
        return mantissa >> (-shift);
    }

    private static double Pow2Double(int exponent)
    {
        if (exponent == 0)
            return 1.0;

        if (exponent > 0)
        {
            double value = 1.0;
            for (int i = 0; i < exponent; i++)
                value += value;
            return value;
        }
        else
        {
            double value = 1.0;
            for (int i = 0; i < -exponent; i++)
                value *= 0.5;
            return value;
        }
    }
}
