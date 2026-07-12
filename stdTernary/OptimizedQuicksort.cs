using System;

namespace stdTernary;

/// <summary>
/// Production-style quicksort built around the balanced-ternary 3-way comparison.
///
/// The partition is a Bentley-McIlroy / Dutch-national-flag 3-way partition driven
/// by ONE spaceship comparison per element: the single Trit result routes each
/// element to the less / equal / greater region. On a binary machine the same
/// routing needs up to two comparison instructions per element; on balanced-ternary
/// hardware it is one. Equal keys collapse into the pivot block, so duplicate-heavy
/// inputs sort in O(n * log(distinct)).
///
/// Optimizations over <see cref="TernaryAlgorithms.TernaryQuicksort{T}(T[])"/>:
/// median-of-three pivot selection, insertion sort below a cutoff, and iteration
/// on the larger partition (recursion only on the smaller) for O(log n) stack depth.
/// </summary>
public static class OptimizedQuicksort
{
    private const int InsertionCutoff = 16;

    // ------------------------------------------------------------------
    // Ternary: IntT keys, instrumented with TernaryFom
    // (comparisons are counted inside IntT.CompareTo itself).
    // ------------------------------------------------------------------

    public static void TernaryQuicksort3Way(IntT[] items)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));
        TernaryQuicksort3Way(items.AsSpan());
    }

    public static void TernaryQuicksort3Way(Span<IntT> span)
    {
        while (span.Length > InsertionCutoff)
        {
            IntT pivot = MedianOfThree(span);

            int low = 0;                 // span[..low]        < pivot
            int mid = 0;                 // span[low..mid]     == pivot
            int high = span.Length - 1;  // span[(high+1)..]   > pivot

            while (mid <= high)
            {
                TritVal c = span[mid].Spaceship(pivot).Value;   // ONE 3-way comparison
                switch (c)
                {
                    case TritVal.n: Swap(span, low++, mid++); break;
                    case TritVal.z: mid++; break;
                    default: Swap(span, mid, high--); break;
                }
            }

            // Recurse into the smaller side, iterate on the larger.
            int leftLen = low;
            int rightLen = span.Length - (high + 1);
            if (leftLen < rightLen)
            {
                TernaryQuicksort3Way(span[..leftLen]);
                span = span[(high + 1)..];
            }
            else
            {
                TernaryQuicksort3Way(span[(high + 1)..]);
                span = span[..leftLen];
            }
        }

        InsertionSort(span);
    }

    /// <summary>Median-of-three pivot VALUE over three random positions (randomized
    /// quicksort: expected O(n log n) comparisons on every input, including sorted
    /// and adversarial patterns). Selected without moving elements; 2-3 ternary comparisons.</summary>
    private static IntT MedianOfThree(Span<IntT> span)
    {
        IntT a = span[NextIndex(span.Length)];
        IntT b = span[NextIndex(span.Length)];
        IntT c = span[NextIndex(span.Length)];
        if (a.Spaceship(b).Value == TritVal.p) (a, b) = (b, a);   // a <= b
        if (b.Spaceship(c).Value == TritVal.p)                    // b > c ?
            b = a.Spaceship(c).Value == TritVal.p ? a : c;        // median of {a, c}
        return b;
    }

    private static ulong _rngState = 0x9E3779B97F4A7C15UL;

    private static int NextIndex(int length)
    {
        ulong s = _rngState;
        s ^= s << 13; s ^= s >> 7; s ^= s << 17;
        _rngState = s;
        return (int)(s % (ulong)length);
    }

    private static void InsertionSort(Span<IntT> span)
    {
        for (int i = 1; i < span.Length; i++)
        {
            IntT key = span[i];
            int j = i - 1;
            while (j >= 0 && span[j].Spaceship(key).Value == TritVal.p)
            {
                TernaryFom.RecordWrite(span[j + 1], span[j]);
                span[j + 1] = span[j];
                j--;
            }
            TernaryFom.RecordWrite(span[j + 1], key);
            span[j + 1] = key;
        }
    }

    private static void Swap(Span<IntT> span, int i, int j)
    {
        if (i == j) return;
        TernaryFom.RecordSwap(span[i], span[j]);
        (span[i], span[j]) = (span[j], span[i]);
    }

    // ------------------------------------------------------------------
    // Binary references on long keys, instrumented with BinaryFom.
    // Same algorithmic structure so instruction-mix relations are meaningful.
    // ------------------------------------------------------------------

    /// <summary>Identical 3-way algorithm; each 3-way decision costs 1-2 binary compares.</summary>
    public static void BinaryQuicksort3Way(Span<long> span)
    {
        while (span.Length > InsertionCutoff)
        {
            long pivot = MedianOfThreeBinary(span);

            int low = 0, mid = 0, high = span.Length - 1;
            while (mid <= high)
            {
                int c = CompareBinary(span[mid], pivot);
                if (c < 0) SwapBinary(span, low++, mid++);
                else if (c == 0) mid++;
                else SwapBinary(span, mid, high--);
            }

            int leftLen = low;
            int rightLen = span.Length - (high + 1);
            if (leftLen < rightLen)
            {
                BinaryQuicksort3Way(span[..leftLen]);
                span = span[(high + 1)..];
            }
            else
            {
                BinaryQuicksort3Way(span[(high + 1)..]);
                span = span[..leftLen];
            }
        }

        InsertionSortBinary(span);
    }

    /// <summary>Classic 2-way Hoare partition (median-of-three, insertion cutoff) as the traditional control.</summary>
    public static void BinaryQuicksort2Way(Span<long> span)
    {
        while (span.Length > InsertionCutoff)
        {
            long pivot = MedianOfThreeBinary(span);
            int i = -1, j = span.Length;

            while (true)
            {
                do { i++; if (BinaryFom.Enabled) BinaryFom.Comparisons++; } while (span[i] < pivot);
                do { j--; if (BinaryFom.Enabled) BinaryFom.Comparisons++; } while (span[j] > pivot);
                if (i >= j) break;
                SwapBinary(span, i, j);
            }

            int leftLen = j + 1;
            if (leftLen < span.Length - leftLen)
            {
                BinaryQuicksort2Way(span[..leftLen]);
                span = span[leftLen..];
            }
            else
            {
                BinaryQuicksort2Way(span[leftLen..]);
                span = span[..leftLen];
            }
        }

        InsertionSortBinary(span);
    }

    /// <summary>
    /// A 3-way decision on binary hardware: first compare a &lt; b, and only when that
    /// fails compare a &gt; b. Counts every comparison instruction actually executed
    /// (1 for the "less" outcome, 2 for "equal"/"greater"), which is exactly what a
    /// binary ISA pays to recover the trit that ternary hardware returns in one shot.
    /// </summary>
    private static int CompareBinary(long a, long b)
    {
        if (BinaryFom.Enabled) BinaryFom.Comparisons++;
        if (a < b) return -1;
        if (BinaryFom.Enabled) BinaryFom.Comparisons++;
        return a > b ? 1 : 0;
    }

    private static long MedianOfThreeBinary(Span<long> span)
    {
        long a = span[NextIndex(span.Length)];
        long b = span[NextIndex(span.Length)];
        long c = span[NextIndex(span.Length)];
        if (CompareBinary(a, b) > 0) (a, b) = (b, a);
        if (CompareBinary(b, c) > 0)
            b = CompareBinary(a, c) > 0 ? a : c;
        return b;
    }

    private static void InsertionSortBinary(Span<long> span)
    {
        for (int i = 1; i < span.Length; i++)
        {
            long key = span[i];
            int j = i - 1;
            while (j >= 0)
            {
                if (BinaryFom.Enabled) BinaryFom.Comparisons++;
                if (span[j] <= key) break;
                if (BinaryFom.Enabled)
                {
                    BinaryFom.BitFlips += System.Numerics.BitOperations.PopCount((ulong)(span[j + 1] ^ span[j]));
                    BinaryFom.BitsWritten += 64;
                }
                span[j + 1] = span[j];
                j--;
            }
            if (BinaryFom.Enabled)
            {
                BinaryFom.BitFlips += System.Numerics.BitOperations.PopCount((ulong)(span[j + 1] ^ key));
                BinaryFom.BitsWritten += 64;
            }
            span[j + 1] = key;
        }
    }

    private static void SwapBinary(Span<long> span, int i, int j)
    {
        if (i == j) return;
        BinaryFom.RecordSwap(span[i], span[j]);
        (span[i], span[j]) = (span[j], span[i]);
    }
}
