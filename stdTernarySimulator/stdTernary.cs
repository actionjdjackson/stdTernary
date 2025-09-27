using System;

namespace stdTernary;

public static class MathT
{
    private static readonly FloatT PiValue = FloatT.FromDouble(Math.PI);
    private static readonly FloatT EValue = FloatT.FromDouble(Math.E);

    public static FloatT PI => PiValue;
    public static FloatT E => EValue;

    public static Tryte Abs(Tryte value) => value.CompareTo(Tryte.Zero) < 0 ? value.INVERT() : value;
    public static IntT Abs(IntT value) => value.Sign >= 0 ? value : -value;
    public static FloatT Abs(FloatT value) => value.CompareTo(FloatT.Zero) >= 0 ? value : -value;

    public static IntT Min(IntT a, IntT b) => a.CompareTo(b) <= 0 ? a : b;
    public static IntT Max(IntT a, IntT b) => a.CompareTo(b) >= 0 ? a : b;
    public static IntT Clamp(IntT value, IntT min, IntT max)
    {
        if (min.CompareTo(max) > 0)
            throw new ArgumentException("Clamp minimum must not exceed maximum.");
        if (value.CompareTo(min) < 0)
            return min;
        return value.CompareTo(max) > 0 ? max : value;
    }

    public static FloatT Min(FloatT a, FloatT b) => a.CompareTo(b) <= 0 ? a : b;
    public static FloatT Max(FloatT a, FloatT b) => a.CompareTo(b) >= 0 ? a : b;
    public static FloatT Clamp(FloatT value, FloatT min, FloatT max)
    {
        if (min.CompareTo(max) > 0)
            throw new ArgumentException("Clamp minimum must not exceed maximum.");
        if (value.CompareTo(min) < 0)
            return min;
        return value.CompareTo(max) > 0 ? max : value;
    }

    public static IntT Pow(IntT value, IntT exponent)
    {
        long exp = exponent.ToInt64();
        if (exp < 0)
            throw new ArgumentOutOfRangeException(nameof(exponent), "IntT exponent must be non-negative.");

        IntT result = IntT.One;
        IntT baseValue = value;
        long e = exp;

        while (e > 0)
        {
            if ((e & 1) == 1)
                result *= baseValue;
            if (e > 1)
                baseValue *= baseValue;
            e >>= 1;
        }

        return result;
    }

    public static FloatT Pow(FloatT value, IntT exponent)
    {
        return Pow(value, exponent.ToInt64());
    }

    public static FloatT Pow(FloatT value, FloatT exponent)
    {
        return FloatT.FromDouble(Math.Pow(value.ToDouble(), exponent.ToDouble()));
    }

    public static FloatT Pow(FloatT value, long exponent)
    {
        if (exponent == 0)
            return FloatT.FromDouble(1.0);
        if (exponent < 0)
            return FloatT.FromDouble(Math.Pow(value.ToDouble(), exponent));

        FloatT result = FloatT.FromDouble(1.0);
        FloatT baseValue = value;
        long e = exponent;

        while (e > 0)
        {
            if ((e & 1) == 1)
                result *= baseValue;
            if (e > 1)
                baseValue *= baseValue;
            e >>= 1;
        }

        return result;
    }

    public static FloatT Sqrt(FloatT value)
    {
        double d = value.ToDouble();
        if (d < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Cannot take the square root of a negative FloatT.");
        return FloatT.FromDouble(Math.Sqrt(d));
    }

    public static FloatT Exp(FloatT value) => FloatT.FromDouble(Math.Exp(value.ToDouble()));
    public static FloatT Log(FloatT value) => FloatT.FromDouble(Math.Log(value.ToDouble()));
    public static FloatT Log(FloatT value, FloatT newBase) => FloatT.FromDouble(Math.Log(value.ToDouble(), newBase.ToDouble()));
    public static FloatT Log(FloatT value, double newBase) => FloatT.FromDouble(Math.Log(value.ToDouble(), newBase));
    public static FloatT Log10(FloatT value) => FloatT.FromDouble(Math.Log10(value.ToDouble()));
    public static FloatT Log3(FloatT value) => FloatT.FromDouble(Math.Log(value.ToDouble(), 3.0));

    public static IntT ILogT(FloatT value)
    {
        if (value.CompareTo(FloatT.Zero) <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), "ILogT requires a positive FloatT value.");
        double logBase3 = Math.Log(value.ToDouble(), 3.0);
        return new IntT((long)Math.Floor(logBase3));
    }

    public static FloatT Sin(FloatT value) => FloatT.FromDouble(Math.Sin(value.ToDouble()));
    public static FloatT Cos(FloatT value) => FloatT.FromDouble(Math.Cos(value.ToDouble()));
    public static FloatT Tan(FloatT value) => FloatT.FromDouble(Math.Tan(value.ToDouble()));
    public static FloatT Asin(FloatT value) => FloatT.FromDouble(Math.Asin(value.ToDouble()));
    public static FloatT Acos(FloatT value) => FloatT.FromDouble(Math.Acos(value.ToDouble()));
    public static FloatT Atan(FloatT value) => FloatT.FromDouble(Math.Atan(value.ToDouble()));
    public static FloatT Atan2(FloatT y, FloatT x) => FloatT.FromDouble(Math.Atan2(y.ToDouble(), x.ToDouble()));

    public static FloatT Floor(FloatT value) => FloatT.FromDouble(Math.Floor(value.ToDouble()));
    public static FloatT Ceiling(FloatT value) => FloatT.FromDouble(Math.Ceiling(value.ToDouble()));
    public static FloatT Truncate(FloatT value) => FloatT.FromDouble(Math.Truncate(value.ToDouble()));
    public static FloatT Round(FloatT value) => FloatT.FromDouble(Math.Round(value.ToDouble()));
    public static FloatT Round(FloatT value, int digits, MidpointRounding mode = MidpointRounding.ToEven) => FloatT.FromDouble(Math.Round(value.ToDouble(), digits, mode));

    public static IntT Sign(IntT value) => new IntT(value.Sign);
    public static IntT Sign(FloatT value) => new IntT(value.CompareTo(FloatT.Zero));

    public static FloatT DegToRad(FloatT degrees) => FloatT.FromDouble(degrees.ToDouble() * Math.PI / 180.0);
    public static FloatT RadToDeg(FloatT radians) => FloatT.FromDouble(radians.ToDouble() * 180.0 / Math.PI);
}
