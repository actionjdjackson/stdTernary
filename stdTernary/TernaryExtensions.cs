using System;

namespace stdTernary;

public static class TernaryExtensions
{
    public static Trit Spaceship<T>(this T left, T right) where T : IComparable<T>
        => Trit.FromComparison(left.CompareTo(right));

    public static TernaryDecision Ternary<T>(this T left, T right) where T : IComparable<T>
        => TernaryDecision.Compare(left, right);
}
