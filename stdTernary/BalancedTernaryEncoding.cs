using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace stdTernary;

/// <summary>
/// Packed balanced-ternary encoding with word-parallel (SWAR) arithmetic.
///
/// Layout: each trit occupies a 2-bit field holding the digit in 2-bit
/// two's complement:  00 = 0,  01 = +1,  11 = -1   (10 is unused).
///
/// Why this layout (vs. the previous 00=0/01=+1/10=-1):
///  1. default(ulong) == 0 still encodes the value zero, so default structs stay valid.
///  2. Flipping each field's sign bit (XOR with <see cref="Hi"/>) makes the packed
///     word order-isomorphic to the numeric value, so a full 3-way comparison of two
///     numbers is ONE xor + ONE integer compare - the software realization of the
///     "comparison is a single instruction in balanced ternary" property.
///  3. The low bit of every field (<see cref="Lo"/>) is an "occupied" rail and the
///     high bit is the "negative" rail, so the dual-rail (P,N) form used by
///     Frieder &amp; Luk (1973) for word-parallel carry handling falls out with two masks.
///  4. Left/right shifts of the packed word shift in 00 fields, i.e. zero trits,
///     so ternary shifts are single machine shifts.
/// </summary>
internal static class BalancedTernaryEncoding
{
    internal const int MaxTrits = 32;
    private const ulong TritMask = 0b11UL;

    /// <summary>Low ("occupied") bit of every 2-bit trit field.</summary>
    internal const ulong Lo = 0x5555555555555555UL;
    /// <summary>High ("negative"/sign) bit of every 2-bit trit field.</summary>
    internal const ulong Hi = 0xAAAAAAAAAAAAAAAAUL;

    // ---- 6-trit limb tables for fast binary <-> balanced-ternary conversion ----
    // A limb is 6 trits = one balanced base-729 digit in [-364, +364].
    private const int LimbRadix = 729;          // 3^6
    private const int LimbBias = 364;           // (729 - 1) / 2
    private static readonly ushort[] EncTable = BuildEncTable(); // value+364 -> 12-bit pattern
    private static readonly short[] DecTable = BuildDecTable();  // 12-bit pattern -> value

    private static ushort[] BuildEncTable()
    {
        var table = new ushort[LimbRadix];
        for (int v = -LimbBias; v <= LimbBias; v++)
        {
            ulong packed = 0;
            int remaining = v;
            for (int i = 0; i < 6 && remaining != 0; i++)
            {
                int rem = remaining % 3;
                remaining /= 3;
                if (rem > 1) { rem -= 3; remaining += 1; }
                else if (rem < -1) { rem += 3; remaining -= 1; }
                packed |= ((ulong)(rem & 3)) << (i * 2);
            }
            table[v + LimbBias] = (ushort)packed;
        }
        return table;
    }

    private static short[] BuildDecTable()
    {
        var table = new short[4096];
        for (int v = -LimbBias; v <= LimbBias; v++)
            table[EncTable[v + LimbBias]] = (short)v;
        return table;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong FieldMask(int count)
        => count >= MaxTrits ? ulong.MaxValue : (1UL << (count * 2)) - 1UL;

    // ------------------------------------------------------------------
    // Single-trit access (kept for the digit-serial callers: Tryte, FloatT, ...)
    // ------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static sbyte DecodeTrit(ulong packed, int index)
    {
        if ((uint)index >= MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(index));

        // 2-bit two's complement sign extension: 00->0, 01->+1, 11->-1.
        int bits = (int)((packed >> (index * 2)) & TritMask);
        return (sbyte)((bits << 30) >> 30);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong SetTrit(ulong packed, int index, sbyte value)
    {
        if ((uint)index >= MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (value is < -1 or > 1)
            throw new ArgumentOutOfRangeException(nameof(value), "Balanced ternary trits must be -1, 0, or 1.");

        ulong mask = ~(TritMask << (index * 2));
        ulong encoded = (ulong)(value & 3);   // -1 -> 11, 0 -> 00, +1 -> 01
        return (packed & mask) | (encoded << (index * 2));
    }

    internal static void Decode(ulong packed, Span<sbyte> digits, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (digits.Length < count)
            throw new ArgumentException("Destination span too small.", nameof(digits));

        for (int i = 0; i < count; i++)
            digits[i] = DecodeTrit(packed, i);
    }

    internal static ulong Encode(ReadOnlySpan<sbyte> digits, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (digits.Length < count)
            throw new ArgumentException("Source span too small.", nameof(digits));

        ulong packed = 0UL;
        for (int i = 0; i < count; i++)
        {
            sbyte v = digits[i];
            if (v is < -1 or > 1)
                throw new ArgumentOutOfRangeException(nameof(digits), "Balanced ternary trits must be -1, 0, or 1.");
            packed |= ((ulong)(v & 3)) << (i * 2);
        }

        return packed;
    }

    // ------------------------------------------------------------------
    // Dual-rail (Frieder & Luk) helpers.
    // P has a 1 in a field's low-bit position where the trit is +1;
    // N has a 1 where the trit is -1. Both live on the Lo positions.
    // ------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (ulong P, ulong N) Rails(ulong packed)
    {
        ulong n = (packed >> 1) & Lo;               // sign bit set  -> -1
        ulong p = packed & ~(packed >> 1) & Lo;     // occupied, not negative -> +1
        return (p, n);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong PackRails(ulong p, ulong n) => p | n | (n << 1);

    /// <summary>Ternary negation = swap the positive and negative rails. O(1).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Negate(ulong packed)
    {
        var (p, n) = Rails(packed);
        return PackRails(n, p);
    }

    /// <summary>
    /// Word-parallel balanced ternary addition (all trits at once), following the
    /// Frieder &amp; Luk observation that positive and negative carries separate with
    /// Boolean operations. Each round is a tritwise half-add; carries ripple by
    /// whole words, so the loop runs for the length of the longest carry chain
    /// (typically 2-4 rounds, worst case <paramref name="count"/>).
    /// Throws <see cref="OverflowException"/> when a carry leaves the top trit.
    /// </summary>
    internal static ulong AddSwar(ulong left, ulong right, int count)
    {
        var (pa, na) = Rails(left);
        var (pb, nb) = Rails(right);
        return AddRails(pa, na, pb, nb, count);
    }

    internal static ulong SubSwar(ulong left, ulong right, int count)
    {
        var (pa, na) = Rails(left);
        var (pb, nb) = Rails(right);
        return AddRails(pa, na, nb, pb, count);   // negate right = swap its rails
    }

    private static ulong AddRails(ulong pa, ulong na, ulong pb, ulong nb, int count)
    {
        // Carries out of the top word trit (trit 31, low bit 62) are accumulated in
        // a scalar "trit 32". They may transiently appear and later cancel (e.g.
        // "+-" + "+-" = "++"), so overflow is only decided once all carries settle:
        // the result overflows iff the settled trit 32 is nonzero, or - for widths
        // below the word - iff any trit at or above `count` is nonzero.
        const int TopLowBit = (MaxTrits - 1) * 2;   // 62
        int trit32 = 0;

        while (true)
        {
            // Tritwise half adder: s = a + b in [-2, 2]  ->  s = 3*c + r
            //   s = +2: r = -1, c = +1        s = -2: r = +1, c = -1
            //   s = +1: r = +1                s = -1: r = -1
            ulong za = ~(pa | na);
            ulong zb = ~(pb | nb);

            ulong rp = (pa & zb) | (pb & za) | (na & nb);
            ulong rn = (na & zb) | (nb & za) | (pa & pb);
            ulong cp = pa & pb;
            ulong cn = na & nb;

            if ((cp | cn) == 0)
            {
                if (trit32 != 0)
                    throw new OverflowException($"Balanced ternary overflow beyond {count} trits.");

                ulong result = PackRails(rp, rn);
                if (count < MaxTrits && HighestNonZeroTrit(result, MaxTrits) >= count)
                    throw new OverflowException($"Balanced ternary overflow beyond {count} trits.");
                return result;
            }

            trit32 += (int)((cp >> TopLowBit) & 1) - (int)((cn >> TopLowBit) & 1);

            pa = rp; na = rn;
            pb = cp << 2; nb = cn << 2;   // carry moves to the next trit
        }
    }

    /// <summary>Tritwise minimum (ternary AND). O(1).</summary>
    internal static ulong MinSwar(ulong left, ulong right)
    {
        var (pa, na) = Rails(left);
        var (pb, nb) = Rails(right);
        ulong n = na | nb;
        ulong p = pa & pb;
        return PackRails(p, n);
    }

    /// <summary>Tritwise maximum (ternary OR). O(1).</summary>
    internal static ulong MaxSwar(ulong left, ulong right)
    {
        var (pa, na) = Rails(left);
        var (pb, nb) = Rails(right);
        ulong p = pa | pb;
        ulong n = na & nb;
        return PackRails(p, n);
    }

    /// <summary>Tritwise XOR with the library's semantics: 0 if either is 0, -1 if equal, +1 if different. O(1).</summary>
    internal static ulong XorSwar(ulong left, ulong right)
    {
        var (pa, na) = Rails(left);
        var (pb, nb) = Rails(right);
        ulong p = (pa & nb) | (na & pb);
        ulong n = (pa & pb) | (na & nb);
        return PackRails(p, n);
    }

    /// <summary>Number of trit positions in which two packed values differ (for digit-flip accounting).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int DiffTritCount(ulong left, ulong right)
    {
        ulong diff = left ^ right;
        ulong changed = (diff | (diff >> 1)) & Lo;
        return BitOperations.PopCount(changed);
    }

    /// <summary>Number of nonzero trits (ternary "population count").</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int NonZeroTritCount(ulong packed)
        => BitOperations.PopCount(packed & Lo);

    // ------------------------------------------------------------------
    // Whole-value operations
    // ------------------------------------------------------------------

    internal static ulong FromInt64(long value, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count == MaxTrits)
        {
            // Fast path: peel 6-trit limbs (balanced base-729 digits) and stitch
            // pre-encoded 12-bit patterns together. 5 full limbs + one 2-trit top limb.
            ulong packed = 0;
            long remaining = value;
            int shift = 0;
            for (int limb = 0; limb < 5; limb++)
            {
                int rem = (int)(remaining % LimbRadix);
                remaining /= LimbRadix;
                if (rem > LimbBias) { rem -= LimbRadix; remaining += 1; }
                else if (rem < -LimbBias) { rem += LimbRadix; remaining -= 1; }
                packed |= ((ulong)EncTable[rem + LimbBias]) << shift;
                shift += 12;
                if (remaining == 0)
                    return packed;   // higher limbs are all zero
            }

            // Top limb: trits 30 and 31 -> value in [-4, +4].
            if (remaining < -4 || remaining > 4)
                throw new OverflowException($"Value {value} cannot be represented using {count} balanced ternary trits.");
            packed |= ((ulong)(EncTable[(int)remaining + LimbBias] & 0xF)) << 60;
            return packed;
        }

        // Generic path for narrower types (Tryte, CharT, FloatT fields, ...).
        Span<sbyte> digits = stackalloc sbyte[count];
        long rest = value;
        int index = 0;
        while (rest != 0 && index < count)
        {
            int rem = (int)(rest % 3);
            rest /= 3;
            if (rem > 1) { rem -= 3; rest += 1; }
            else if (rem < -1) { rem += 3; rest -= 1; }
            digits[index++] = (sbyte)rem;
        }

        if (rest != 0)
            throw new OverflowException($"Value {value} cannot be represented using {count} balanced ternary trits.");

        return Encode(digits, count);
    }

    internal static long ToInt64(ulong packed, int count)
    {
        if ((uint)count > MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count == MaxTrits)
        {
            long value = DecTable[(packed >> 60) & 0xF];
            for (int limb = 4; limb >= 0; limb--)
                value = value * LimbRadix + DecTable[(packed >> (limb * 12)) & 0xFFF];
            return value;
        }

        long v = 0;
        for (int i = count - 1; i >= 0; i--)
            v = checked(v * 3 + DecodeTrit(packed, i));
        return v;
    }

    /// <summary>
    /// Three-way comparison in O(1). The raw packed bits are not ordered like the
    /// numeric value (-1 is stored as 11, above +1's 01), so XOR each field's sign
    /// bit (the <see cref="Hi"/> mask): that maps -1 -&gt; 01, 0 -&gt; 10, +1 -&gt; 11, i.e.
    /// each field's unsigned value now rises with the trit's value, making the whole
    /// word order-isomorphic to the number. A single unsigned compare of the two
    /// transformed words then decides &lt;, =, &gt; (most-significant trit first).
    /// On this binary host that is two masked XORs plus one integer compare; on
    /// native balanced-ternary hardware the same comparison is one instruction,
    /// which is how <see cref="TernaryFom"/> counts it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Compare(ulong left, ulong right, int count)
    {
        ulong mask = FieldMask(count);
        ulong l = (left & mask) ^ (Hi & mask);
        ulong r = (right & mask) ^ (Hi & mask);
        return l < r ? -1 : (l > r ? 1 : 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsZero(ulong packed, int count)
        => (packed & FieldMask(count)) == 0UL;

    internal static int HighestNonZeroTrit(ulong packed, int count)
    {
        // Every nonzero trit (codes 01 and 11) has its low field bit set.
        ulong occupied = packed & Lo & FieldMask(count);
        if (occupied == 0)
            return -1;
        return (63 - BitOperations.LeadingZeroCount(occupied)) / 2;
    }

    internal static string ToTernaryString(ulong packed, int count)
    {
        Span<char> buffer = stackalloc char[count];
        for (int i = count - 1, j = 0; i >= 0; i--, j++)
        {
            buffer[j] = DecodeTrit(packed, i) switch
            {
                -1 => '-',
                0 => '0',
                1 => '+',
                _ => '?'
            };
        }

        return new string(buffer);
    }
}
