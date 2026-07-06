using System;

namespace stdTernary;

public static class TernaryAlgorithms
{
    public static void TernaryQuicksort<T>(T[] items) where T : IComparable<T>
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));
        if (items.Length < 2)
            return;

        TernaryQuicksort(items.AsSpan());
    }

    public static void BinaryQuicksort<T>(T[] items) where T : IComparable<T>
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));
        if (items.Length < 2)
            return;

        BinaryQuicksort(items.AsSpan());
    }

    public static void TernaryQuicksort<T>(Span<T> span) where T : IComparable<T>
    {
        if (span.Length < 2)
            return;

        var pivot = span[span.Length / 2];
        int low = 0;
        int mid = 0;
        int high = span.Length - 1;

        while (mid <= high)
        {
            TritVal comparison = span[mid].Spaceship(pivot).Value;
            switch (comparison)
            {
                case TritVal.n:
                    Swap(span, low++, mid++);
                    break;
                case TritVal.z:
                    mid++;
                    break;
                case TritVal.p:
                    Swap(span, mid, high--);
                    break;
            }
        }

        TernaryQuicksort(span[..low]);
        TernaryQuicksort(span[(high + 1)..]);
    }

    public static void BinaryQuicksort<T>(Span<T> span) where T : IComparable<T>
    {
        if (span.Length < 2)
            return;

        int left = 0;
        int right = span.Length - 1;
        T pivot = span[span.Length / 2];

        while (left <= right)
        {
            while (left <= right && span[left].CompareTo(pivot) < 0)
                left++;

            while (left <= right && span[right].CompareTo(pivot) > 0)
                right--;

            if (left <= right)
            {
                Swap(span, left, right);
                left++;
                right--;
            }
        }

        if (right > 0)
            BinaryQuicksort(span[..(right + 1)]);

        if (left < span.Length)
            BinaryQuicksort(span[left..]);
    }

    private const double DefaultEquilibriumTolerance = 1e-9;

    /// <summary>
    /// Classifies the net direction of a reversible biochemical reaction using balanced ternary logic.
    /// Negative means the reaction is currently biased in the reverse direction, zero means it is close
    /// enough to equilibrium to be treated as stable, and positive means it is biased forward.
    /// </summary>
    /// <remarks>
    /// This is an example of the kind of ternary mathematics a biochemical simulator could use.
    /// Instead of immediately calculating a continuous reaction rate for every molecule, the simulator
    /// can first classify each reaction into one of three meaningful physical states:
    /// reverse, equilibrium, or forward.
    /// </remarks>
    public static Trit ClassifyReactionDirection(
        FloatT forwardPressure,
        FloatT reversePressure,
        FloatT equilibriumTolerance)
    {
        FloatT netPressure = forwardPressure - reversePressure;
        Trit pressureDecision = MathT.Abs(netPressure).Spaceship(equilibriumTolerance);

        return pressureDecision.Value switch
        {
            TritVal.p => netPressure.Spaceship(FloatT.Zero),
            _ => new Trit(TritVal.z)
        };
    }

    public static Trit ClassifyReactionDirection(
        FloatT forwardPressure,
        FloatT reversePressure)
        => ClassifyReactionDirection(
            forwardPressure,
            reversePressure,
            FloatT.FromDouble(DefaultEquilibriumTolerance));

    /// <summary>
    /// Chooses the next coarse biochemical event from three competing pressures:
    /// degradation/reverse pressure, no-change/equilibrium pressure, and synthesis/forward pressure.
    /// </summary>
    /// <remarks>
    /// A larger simulator could use this as a coarse ternary event selector before running a more expensive
    /// stochastic or spatial simulation step. For example, a protein population could be classified as
    /// tending toward degradation, remaining stable, or tending toward synthesis.
    /// </remarks>
    public static Trit ChooseTernaryBiochemicalEvent(
        FloatT negativePressure,
        FloatT zeroPressure,
        FloatT positivePressure)
    {
        Trit negativeVsZero = negativePressure.Spaceship(zeroPressure);
        Trit negativeVsPositive = negativePressure.Spaceship(positivePressure);
        Trit positiveVsNegative = positivePressure.Spaceship(negativePressure);
        Trit positiveVsZero = positivePressure.Spaceship(zeroPressure);

        if (negativeVsZero.Value == TritVal.p && negativeVsPositive.Value == TritVal.p)
            return new Trit(TritVal.n);

        if (positiveVsNegative.Value == TritVal.p && positiveVsZero.Value == TritVal.p)
            return new Trit(TritVal.p);

        return new Trit(TritVal.z);
    }

    private static void Swap<T>(Span<T> span, int i, int j)
    {
        if (i == j)
            return;

        (span[i], span[j]) = (span[j], span[i]);
    }
}
