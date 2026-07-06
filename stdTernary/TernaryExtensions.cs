using System;

namespace stdTernary;

public static class TernaryExtensions
{
    public static Trit Spaceship<T>(this T left, T right) where T : IComparable<T>
        => Trit.FromComparison(left.CompareTo(right));

    public static Trit Spaceship(this UIntT left, IntT right)
        => Trit.FromComparison(left.CompareTo(right));

    public static Trit Spaceship(this IntT left, UIntT right)
        => Trit.FromComparison(-right.CompareTo(left));

    public static Trit Spaceship(this UTryte left, Tryte right)
    {
        short rightValue = right.ShortValue;
        if (rightValue < 0)
            return new Trit(TritVal.p);
        return Trit.FromComparison(left.ULongValue.CompareTo((ulong)rightValue));
    }

    public static Trit Spaceship(this Tryte left, UTryte right)
    {
        short leftValue = left.ShortValue;
        if (leftValue < 0)
            return new Trit(TritVal.n);
        return Trit.FromComparison(((ulong)leftValue).CompareTo(right.ULongValue));
    }

    public static Trit Spaceship(this UFloatT left, FloatT right)
        => Trit.FromComparison(left.CompareTo(right));

    public static Trit Spaceship(this FloatT left, UFloatT right)
        => Trit.FromComparison(-right.CompareTo(left));

    public static Trit Spaceship(this UTrit left, Trit right)
        => UTrit.COMPARET(left, right);

    public static Trit Spaceship(this Trit left, UTrit right)
        => UTrit.COMPARET(left, right);

    /// <summary>
    /// Performs a ternary range comparison.
    /// Returns negative if the value is below the minimum,
    /// positive if the value is above the maximum,
    /// and zero if the value is within the inclusive range.
    /// </summary>
    public static Trit Spaceship(this FloatT value, FloatT minimum, FloatT maximum)
    {
        if (minimum.CompareTo(maximum) > 0)
            throw new ArgumentOutOfRangeException(nameof(minimum), "Minimum cannot exceed maximum.");

        Trit belowMinimum = value.Spaceship(minimum);
        if (belowMinimum.Value == TritVal.n)
            return new Trit(TritVal.n);

        Trit aboveMaximum = value.Spaceship(maximum);
        if (aboveMaximum.Value == TritVal.p)
            return new Trit(TritVal.p);

        return new Trit(TritVal.z);
    }

    public static TernaryDecision Ternary<T>(this T left, T right) where T : IComparable<T>
        => TernaryDecision.Compare(left, right);
}
