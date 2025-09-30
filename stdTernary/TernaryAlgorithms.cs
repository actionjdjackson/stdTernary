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

    private static void Swap<T>(Span<T> span, int i, int j)
    {
        if (i == j)
            return;

        (span[i], span[j]) = (span[j], span[i]);
    }
}
