using System;
using System.Diagnostics.CodeAnalysis;

namespace stdTernary;

/// <summary>
/// 24-trit balanced ternary integer - the canonical REBEL-6 machine word and the
/// standard integer type of this library - backed by a single packed
/// <see cref="ulong"/> (low 48 bits, 2 bits per trit).
///
/// All arithmetic is word-parallel (SWAR) over the packed representation - no
/// per-trit decode/normalize/encode loops - following Frieder &amp; Luk's dual-rail
/// scheme for carry separation. Comparison is a single xor+compare thanks to the
/// order-isomorphic encoding.
///
/// IntT follows the REBEL-6 Arithmetic Semantics: add, subtract and multiply wrap
/// balanced modulo 3^24 (range +/-(3^24-1)/2 = +/-141,214,768,240), negation is
/// total, and division by zero does not trap (quotient 0, remainder = dividend).
/// Methods that map 1:1 onto REBEL-6 instructions carry
/// <see cref="Rebel6IntrinsicAttribute"/>.
/// </summary>
public readonly struct IntT : IEquatable<IntT>, IComparable<IntT>
{
    public const int TritCount = 24;

    /// <summary>3^24 - the wrap modulus of the REBEL-6 ternary word.</summary>
    public const long Modulus = 282_429_536_481L;

    /// <summary>(3^24 - 1) / 2 = 141,214,768,240.</summary>
    public const long MaxMagnitude = (Modulus - 1) / 2;

    /// <summary>Mask covering the 24 two-bit trit fields (low 48 bits).</summary>
    private const ulong WordMask = (1UL << (TritCount * 2)) - 1UL;

    private readonly ulong _packed;

    internal ulong Packed => _packed;

    private IntT(ulong packed)
    {
        _packed = packed;
    }

    /// <summary>
    /// Constructs the word congruent to <paramref name="value"/> modulo 3^24
    /// (balanced wrap - register-write semantics, never throws).
    /// </summary>
    public IntT(long value)
    {
        _packed = BalancedTernaryEncoding.FromInt64(Wrap(value), TritCount);
    }

    public static IntT Zero { get; } = new IntT(0L);
    public static IntT One { get; } = new IntT(1L);
    public static IntT NegativeOne { get; } = new IntT(-1L);
    public static IntT MaxValue { get; } = new IntT(MaxMagnitude);
    public static IntT MinValue { get; } = new IntT(-MaxMagnitude);

    public long ToInt64() => BalancedTernaryEncoding.ToInt64(_packed, TritCount);

    public int Sign => BalancedTernaryEncoding.Compare(_packed, 0UL, TritCount);

    public string TernaryString => BalancedTernaryEncoding.ToTernaryString(_packed, TritCount);

    public override string ToString() => $"{TernaryString} (base3) = {ToInt64()}";

    public static IntT Parse(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        value = value.Trim();
        if (value.Length == 0)
            throw new FormatException("Empty ternary string.");
        if (value.Length > TritCount)
            throw new FormatException($"Ternary string longer than {TritCount} trits.");

        Span<sbyte> digits = stackalloc sbyte[TritCount];
        int index = 0;
        for (int i = value.Length - 1; i >= 0; i--)
        {
            digits[index++] = value[i] switch
            {
                '+' => 1,
                '0' => 0,
                '-' => -1,
                _ => throw new FormatException($"Invalid ternary digit '{value[i]}'."),
            };
        }

        return new IntT(BalancedTernaryEncoding.Encode(digits, TritCount));
    }

    public static bool TryParse(string? value, out IntT result)
    {
        try
        {
            result = Parse(value ?? throw new ArgumentNullException(nameof(value)));
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public bool Equals(IntT other) => _packed == other._packed;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is IntT other && Equals(other);

    public override int GetHashCode() => _packed.GetHashCode();

    /// <summary>Three-way comparison - one CMP.T on REBEL-6 (result trit -/0/+).</summary>
    [Rebel6Intrinsic("CMP.T")]
    public int CompareTo(IntT other)
    {
        if (TernaryFom.Enabled) TernaryFom.Comparisons++;
        return BalancedTernaryEncoding.Compare(_packed, other._packed, TritCount);
    }

    public static bool operator ==(IntT left, IntT right) => left.Equals(right);

    public static bool operator !=(IntT left, IntT right) => !left.Equals(right);

    public static bool operator <(IntT left, IntT right) => left.CompareTo(right) < 0;

    public static bool operator <=(IntT left, IntT right) => left.CompareTo(right) <= 0;

    public static bool operator >(IntT left, IntT right) => left.CompareTo(right) > 0;

    public static bool operator >=(IntT left, IntT right) => left.CompareTo(right) >= 0;

    public static IntT operator +(IntT value) => value;

    /// <summary>Negation (STI.T). Total: the balanced range is symmetric, -MinValue == MaxValue.</summary>
    [Rebel6Intrinsic("STI.T")]
    public static IntT operator -(IntT value)
    {
        if (TernaryFom.Enabled) TernaryFom.Negations++;
        return new IntT(BalancedTernaryEncoding.Negate(value._packed) & WordMask);
    }

    [Rebel6Intrinsic("ADD.T")]
    public static IntT operator +(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.Additions++;
        // Full-width SWAR add, then truncate to 24 trits: dropping balanced ternary
        // digits at and above trit 24 IS reduction balanced mod 3^24, so the mask is
        // the wrap. Two 24-trit operands can never overflow the 32-trit adder.
        return new IntT(BalancedTernaryEncoding.AddSwar(left._packed, right._packed, BalancedTernaryEncoding.MaxTrits) & WordMask);
    }

    [Rebel6Intrinsic("SUB.T")]
    public static IntT operator -(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.Additions++;
        return new IntT(BalancedTernaryEncoding.SubSwar(left._packed, right._packed, BalancedTernaryEncoding.MaxTrits) & WordMask);
    }

    [Rebel6Intrinsic("MUL.T")]
    public static IntT operator *(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.Multiplications++;
        // The exact product spans up to 48 trits (~77 bits), so it is formed in an
        // Int128 and wrapped balanced mod 3^24 - the MUL.T low word.
        Int128 product = (Int128)left.ToInt64() * right.ToInt64();
        return new IntT(Wrap(product));
    }

    /// <summary>Truncating division (DIV.T). Division by zero yields quotient 0 - no trap.</summary>
    [Rebel6Intrinsic("DIV.T")]
    public static IntT operator /(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.Divisions++;
        return DivRem(left, right).Quotient;
    }

    /// <summary>Remainder pairing DIV.T (REM.T): sign follows the dividend. x % 0 == x - no trap.</summary>
    [Rebel6Intrinsic("REM.T")]
    public static IntT operator %(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.Divisions++;
        return DivRem(left, right).Remainder;
    }

    /// <summary>
    /// Truncating quotient and remainder in one pass. REBEL-6 division has no
    /// overflow case (MinValue / -1 == MaxValue) and no trap: x / 0 == (q: 0, r: x),
    /// which preserves the identity dividend == q * divisor + r.
    /// </summary>
    public static (IntT Quotient, IntT Remainder) DivRem(IntT dividend, IntT divisor)
    {
        long a = dividend.ToInt64();
        long b = divisor.ToInt64();
        if (b == 0)
            return (Zero, dividend);

        long q = a / b;       // truncated toward zero; remainder sign follows the dividend
        long r = a - q * b;
        return (new IntT(q), new IntT(r));
    }

    /// <summary>Floored modulus (MOD.T): sign follows the divisor. x mod 0 == x - no trap.</summary>
    [Rebel6Intrinsic("MOD.T")]
    public static IntT Mod(IntT dividend, IntT divisor)
    {
        if (TernaryFom.Enabled) TernaryFom.Divisions++;
        long a = dividend.ToInt64();
        long b = divisor.ToInt64();
        if (b == 0)
            return dividend;

        long r = a % b;                       // truncating remainder, sign of dividend
        if (r != 0 && (r < 0) != (b < 0))
            r += b;                           // floor correction: sign of divisor
        return new IntT(r);
    }

    /// <summary>
    /// High word of the exact 48-trit product (MULH.T) under the balanced split:
    /// low = balanced wrap mod 3^24 of the exact product (MUL.T), high =
    /// (exact - low) / 3^24. The pair MULH.T:MUL.T composes the exact product
    /// of two 24-trit values as a 48-trit result. No unsigned variant exists -
    /// balanced ternary has no unsigned reading.
    /// </summary>
    [Rebel6Intrinsic("MULH.T")]
    public static IntT Mulh(IntT a, IntT b)
    {
        if (TernaryFom.Enabled) TernaryFom.Multiplications++;
        Int128 exact = (Int128)a.ToInt64() * b.ToInt64();
        long low = Wrap(exact);
        return new IntT((long)((exact - low) / Modulus));
    }

    /// <summary>Fused multiply-accumulate (MAC.T): a * b + c, balanced wrap.</summary>
    [Rebel6Intrinsic("MAC.T")]
    public static IntT Mac(IntT a, IntT b, IntT c)
    {
        if (TernaryFom.Enabled) { TernaryFom.Multiplications++; TernaryFom.Additions++; }
        Int128 result = (Int128)a.ToInt64() * b.ToInt64() + c.ToInt64();
        return new IntT(Wrap(result));
    }

    /// <summary>Wordwise minimum (MIN.T) - arithmetic select of the smaller whole word.</summary>
    [Rebel6Intrinsic("MIN.T")]
    public static IntT Min(IntT a, IntT b) => a.CompareTo(b) <= 0 ? a : b;

    /// <summary>Wordwise maximum (MAX.T) - arithmetic select of the larger whole word.</summary>
    [Rebel6Intrinsic("MAX.T")]
    public static IntT Max(IntT a, IntT b) => a.CompareTo(b) >= 0 ? a : b;

    /// <summary>
    /// Set-if-less-than (SLT.T; the immediate form is SLTI.T): +1 when
    /// a &lt; b, else 0 - a binary predicate, distinct from the three-way
    /// CMP.T which also reports the greater-than case as -1/+1.
    /// </summary>
    [Rebel6Intrinsic("SLT.T")]
    [Rebel6Intrinsic("SLTI.T")]
    public static IntT Slt(IntT a, IntT b)
    {
        if (TernaryFom.Enabled) TernaryFom.Comparisons++;
        return BalancedTernaryEncoding.Compare(a._packed, b._packed, TritCount) < 0 ? One : Zero;
    }

    /// <summary>Per-trit 3-input majority (MAJV.T) = the median of the three trits at each position.</summary>
    [Rebel6Intrinsic("MAJV.T")]
    public static IntT MAJV(IntT a, IntT b, IntT c)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        return new IntT(BalancedTernaryEncoding.MajoritySwar(a._packed, b._packed, c._packed));
    }

    /// <summary>Per-trit 3-input minority (MINV.T) = the negated median at each position.</summary>
    [Rebel6Intrinsic("MINV.T")]
    public static IntT MINV(IntT a, IntT b, IntT c)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        return new IntT(BalancedTernaryEncoding.Negate(BalancedTernaryEncoding.MajoritySwar(a._packed, b._packed, c._packed)) & WordMask);
    }

    /// <summary>
    /// Dual move (MV2.T): rd1 = rs1, rd2 = rs2 with both sources read before either
    /// destination is written - which is what makes `(a, b) = IntT.Mv2(b, a)` a swap.
    /// </summary>
    [Rebel6Intrinsic("MV2.T")]
    public static (IntT Rd1, IntT Rd2) Mv2(IntT rs1, IntT rs2) => (rs1, rs2);

    /// <summary>Exchange two words - the REBEL-6 SWAP.T pseudo-instruction over MV2.T.</summary>
    public static void Swap(ref IntT a, ref IntT b) => (a, b) = Mv2(b, a);

    // ------------------------------------------------------------------
    // P - packed ternary SIMD (reductions and quantization; the tritwise
    // base ops already are the 24-lane arithmetic)
    // ------------------------------------------------------------------

    /// <summary>
    /// Packed ternary multiply-accumulate (TMAC.T): acc + sum over the 24 trit
    /// lanes of a[i] * b[i]. The lane-product sum lies in +/-24; the accumulate
    /// wraps balanced mod 3^24 like ADD.T.
    /// </summary>
    [Rebel6Intrinsic("TMAC.T")]
    public static IntT Tmac(IntT a, IntT b, IntT acc)
    {
        if (TernaryFom.Enabled) { TernaryFom.Multiplications++; TernaryFom.Additions++; }
        int sum = 0;
        for (int i = 0; i < TritCount; i++)
            sum += BalancedTernaryEncoding.DecodeTrit(a._packed, i) * BalancedTernaryEncoding.DecodeTrit(b._packed, i);
        return acc + new IntT(sum);
    }

    /// <summary>Packed ternary dot product - the TDOT.T pseudo-instruction (TMAC.T with a zero accumulator).</summary>
    [Rebel6Intrinsic("TDOT.T")]
    public static IntT Tdot(IntT a, IntT b) => Tmac(a, b, Zero);

    /// <summary>Horizontal trit sum (TSUM.T): sum of the 24 trit lanes, in +/-24.</summary>
    [Rebel6Intrinsic("TSUM.T")]
    public int TritSum()
    {
        int sum = 0;
        for (int i = 0; i < TritCount; i++)
            sum += BalancedTernaryEncoding.DecodeTrit(_packed, i);
        return sum;
    }

    /// <summary>
    /// Ternarize against a threshold (QNT.T): +1 if value &gt; threshold, -1 if
    /// value &lt; -threshold, else 0. The threshold is expected to be &gt;= 0 (the
    /// BitNet absmean threshold); the conditions are evaluated in this order,
    /// exactly as the spec states them.
    /// </summary>
    [Rebel6Intrinsic("QNT.T")]
    public static IntT Qnt(IntT value, IntT threshold)
    {
        if (TernaryFom.Enabled) TernaryFom.Comparisons++;
        if (value.CompareTo(threshold) > 0)
            return One;
        if (value.CompareTo(-threshold) < 0)
            return NegativeOne;
        return Zero;
    }

    /// <summary>
    /// Horizontal tryte-lane maximum (HMAX.T): the largest of the four 6-trit
    /// tryte lanes of this word, each lane read as a balanced value (+/-364).
    /// </summary>
    [Rebel6Intrinsic("HMAX.T")]
    public IntT Hmax()
    {
        // The architectural tryte lane is 6 trits, independent of the library's
        // configurable Tryte width.
        const int LaneTrits = 6;
        if (TernaryFom.Enabled) TernaryFom.Comparisons++;
        long best = long.MinValue;
        for (int lane = 0; lane < TritCount / LaneTrits; lane++)
        {
            long v = BalancedTernaryEncoding.ToInt64(_packed >> (lane * LaneTrits * 2), LaneTrits);
            if (v > best)
                best = v;
        }
        return new IntT(best);
    }

    // ------------------------------------------------------------------
    // Ztl - ternary logic (programmable tritwise gates)
    // ------------------------------------------------------------------

    /// <summary>
    /// Programmable 2-input tritwise gate (TLUT.T): per lane,
    /// result[i] = table[a[i], b[i]], where the 9-trit truth table lives in the
    /// low trits of <paramref name="table"/> - the entry for the input pair
    /// (ta, tb) at trit position (ta * 3 + tb) + 4 (positions 0..8, LST-first).
    /// The table word's upper 15 trits are ignored (reserved).
    /// </summary>
    [Rebel6Intrinsic("TLUT.T")]
    public static IntT Tlut(IntT a, IntT b, IntT table)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        ulong result = 0UL;
        for (int i = 0; i < TritCount; i++)
        {
            int ta = BalancedTernaryEncoding.DecodeTrit(a._packed, i);
            int tb = BalancedTernaryEncoding.DecodeTrit(b._packed, i);
            sbyte r = BalancedTernaryEncoding.DecodeTrit(table._packed, (ta * 3 + tb) + 4);
            result = BalancedTernaryEncoding.SetTrit(result, i, r);
        }
        return new IntT(result);
    }

    /// <summary>
    /// Programmable 1-input tritwise gate (TLUTI.T): per lane,
    /// result[i] = table[a[i]], where the 3-trit truth table lives in the low
    /// trits of <paramref name="table"/> - the entry for input trit ta at trit
    /// position ta + 1. In the instruction the table is a 12-trit immediate
    /// whose trits 3..11 must be zero; here the upper trits are ignored.
    /// </summary>
    [Rebel6Intrinsic("TLUTI.T")]
    public static IntT Tluti(IntT a, IntT table)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        ulong result = 0UL;
        for (int i = 0; i < TritCount; i++)
        {
            int ta = BalancedTernaryEncoding.DecodeTrit(a._packed, i);
            sbyte r = BalancedTernaryEncoding.DecodeTrit(table._packed, ta + 1);
            result = BalancedTernaryEncoding.SetTrit(result, i, r);
        }
        return new IntT(result);
    }

    /// <summary>Builds a TLUTI.T table word from (f(-), f(0), f(+)).</summary>
    public static IntT UnaryGateTable(sbyte fNeg, sbyte fZero, sbyte fPos)
    {
        ulong packed = BalancedTernaryEncoding.SetTrit(0UL, 0, fNeg);
        packed = BalancedTernaryEncoding.SetTrit(packed, 1, fZero);
        packed = BalancedTernaryEncoding.SetTrit(packed, 2, fPos);
        return new IntT(packed);
    }

    /// <summary>
    /// Builds a TLUT.T table word from the nine entries in canonical pair order
    /// (-,-) (-,0) (-,+) (0,-) (0,0) (0,+) (+,-) (+,0) (+,+).
    /// </summary>
    public static IntT BinaryGateTable(params sbyte[] entries)
    {
        if (entries is not { Length: 9 })
            throw new ArgumentException("A 2-input truth table has exactly 9 entries.", nameof(entries));
        ulong packed = 0UL;
        for (int i = 0; i < 9; i++)
            packed = BalancedTernaryEncoding.SetTrit(packed, i, entries[i]);
        return new IntT(packed);
    }

    // Canonical gate tables - normative in rebel6-extensions.md (Ztl). The
    // unary gates are full pseudo-instructions in the assembler; the binary
    // gates are canonical table constants written as LI.T + TLUT.T idioms
    // (no single mnemonic exists in the pattern table yet), so their wrappers
    // below carry no [Rebel6Intrinsic] annotation.
    private static readonly IntT NtiTable = UnaryGateTable(+1, -1, -1);
    private static readonly IntT PtiTable = UnaryGateTable(+1, +1, -1);
    private static readonly IntT MtiTable = UnaryGateTable(+1, 0, +1);
    private static readonly IntT CyuTable = UnaryGateTable(0, +1, -1);
    private static readonly IntT CydTable = UnaryGateTable(+1, -1, 0);
    private static readonly IntT KimpTable = BinaryGateTable(+1, +1, +1, 0, 0, +1, -1, 0, +1);
    private static readonly IntT CmptTable = BinaryGateTable(0, -1, -1, +1, 0, -1, +1, +1, 0);
    private static readonly IntT ConsTable = BinaryGateTable(-1, 0, 0, 0, 0, 0, 0, 0, +1);

    /// <summary>Negative ternary inverter (NTI.T): per trit (+, -, -) - 0 reads as high.</summary>
    [Rebel6Intrinsic("NTI.T")]
    public IntT Nti() => Tluti(this, NtiTable);

    /// <summary>Positive ternary inverter (PTI.T): per trit (+, +, -) - 0 reads as low.</summary>
    [Rebel6Intrinsic("PTI.T")]
    public IntT Pti() => Tluti(this, PtiTable);

    /// <summary>Magnitude (MTI.T): |x| per trit - (+, 0, +).</summary>
    [Rebel6Intrinsic("MTI.T")]
    public IntT Mti() => Tluti(this, MtiTable);

    /// <summary>Cycle up (CYU.T): - -&gt; 0 -&gt; + -&gt; - per trit.</summary>
    [Rebel6Intrinsic("CYU.T")]
    public IntT Cyu() => Tluti(this, CyuTable);

    /// <summary>Cycle down (CYD.T): - -&gt; + -&gt; 0 -&gt; - per trit.</summary>
    [Rebel6Intrinsic("CYD.T")]
    public IntT Cyd() => Tluti(this, CydTable);

    /// <summary>Kleene implication per trit: a -&gt; b = max(-a, b). Canonical TLUT.T table (LI.T + TLUT.T idiom).</summary>
    public static IntT Kimp(IntT a, IntT b) => Tlut(a, b, KimpTable);

    /// <summary>Tritwise compare: sign(a - b) per lane. Canonical TLUT.T table (LI.T + TLUT.T idiom).</summary>
    public static IntT Cmpt(IntT a, IntT b) => Tlut(a, b, CmptTable);

    /// <summary>Consensus per trit: a when a == b, else 0. Canonical TLUT.T table (LI.T + TLUT.T idiom).</summary>
    public static IntT Cons(IntT a, IntT b) => Tlut(a, b, ConsTable);

    /// <summary>Tritwise minimum (AND.T; the immediate form is ANDI.T) - 24-lane Kleene AND.</summary>
    [Rebel6Intrinsic("AND.T")]
    [Rebel6Intrinsic("ANDI.T")]
    public static IntT operator &(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        return new IntT(BalancedTernaryEncoding.MinSwar(left._packed, right._packed) & WordMask);
    }

    /// <summary>Tritwise maximum (OR.T; the immediate form is ORI.T) - 24-lane Kleene OR.</summary>
    [Rebel6Intrinsic("OR.T")]
    [Rebel6Intrinsic("ORI.T")]
    public static IntT operator |(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        return new IntT(BalancedTernaryEncoding.MaxSwar(left._packed, right._packed) & WordMask);
    }

    /// <summary>Tritwise negated product (XOR.T; the immediate form is XORI.T): -(a*b) per lane.</summary>
    [Rebel6Intrinsic("XOR.T")]
    [Rebel6Intrinsic("XORI.T")]
    public static IntT operator ^(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        return new IntT(BalancedTernaryEncoding.XorSwar(left._packed, right._packed) & WordMask);
    }

    public static IntT operator ~(IntT value) => -value;

    /// <summary>
    /// Signed shift - the REBEL-6 SH&lt;f&gt;.T / SHI&lt;f&gt;.T family, where the
    /// balanced amount carries the direction in its own sign: a positive
    /// <paramref name="amount"/> shifts toward the most significant trit
    /// (the old left shift), a negative one toward the least significant trit,
    /// and 0 is the identity. Vacated trits take <paramref name="fill"/>
    /// (N/Z/P = -/0/+); |amount| &gt;= 24 yields an all-fill word.
    /// </summary>
    public IntT Shift(int amount, Trit fill)
    {
        if (TernaryFom.Enabled) TernaryFom.Shifts++;
        return new IntT(ShiftPacked(_packed, amount, (sbyte)fill.Value));
    }

    /// <summary>Signed shift, negative fill (SHN.T; the immediate form is SHIN.T).</summary>
    [Rebel6Intrinsic("SHN.T")]
    [Rebel6Intrinsic("SHIN.T")]
    public IntT ShiftN(int amount) => Shift(amount, new Trit(-1));

    /// <summary>Signed shift, zero fill (SHZ.T; the immediate form is SHIZ.T).</summary>
    [Rebel6Intrinsic("SHZ.T")]
    [Rebel6Intrinsic("SHIZ.T")]
    public IntT ShiftZ(int amount) => Shift(amount, new Trit(0));

    /// <summary>Signed shift, positive fill (SHP.T; the immediate form is SHIP.T).</summary>
    [Rebel6Intrinsic("SHP.T")]
    [Rebel6Intrinsic("SHIP.T")]
    public IntT ShiftP(int amount) => Shift(amount, new Trit(1));

    /// <summary>
    /// Left shift, zero fill - compatibility wrapper for
    /// <c>Shift(+count, 0)</c>, i.e. SHIZ.T with a positive amount.
    /// High trits are dropped (balanced wrap).
    /// </summary>
    [Rebel6Intrinsic("SHIZ.T")]
    public static IntT operator <<(IntT value, int count)
    {
        if (TernaryFom.Enabled) TernaryFom.Shifts++;
        return new IntT(ShiftPacked(value._packed, count, 0));
    }

    /// <summary>
    /// Right shift, zero fill - compatibility wrapper for
    /// <c>Shift(-count, 0)</c>, i.e. SHIZ.T with a negated amount.
    /// </summary>
    [Rebel6Intrinsic("SHIZ.T")]
    public static IntT operator >>(IntT value, int count)
    {
        if (TernaryFom.Enabled) TernaryFom.Shifts++;
        // Unchecked negation: -int.MinValue wraps, but any |amount| >= 24
        // saturates to all-fill (zero here) in either direction anyway.
        return new IntT(ShiftPacked(value._packed, unchecked(-count), 0));
    }

    /// <summary>
    /// Cyclic rotate by <paramref name="count"/> mod 24 (ROT.T for an
    /// immediate amount, ROTR.T for a register amount) - no fill; positive
    /// counts rotate toward the most significant trit, negative counts the
    /// other way (the amount is signed, like the shifts).
    /// </summary>
    [Rebel6Intrinsic("ROT.T")]
    [Rebel6Intrinsic("ROTR.T")]
    public IntT Rotate(int count)
    {
        if (TernaryFom.Enabled) TernaryFom.Shifts++;
        int k = ((count % TritCount) + TritCount) % TritCount;
        if (k == 0)
            return this;
        return new IntT(((_packed << (k * 2)) | (_packed >> ((TritCount - k) * 2))) & WordMask);
    }

    /// <summary>Count of leading zero trits (CLZT.T): 0..24, with CLZT(0) == 24.</summary>
    [Rebel6Intrinsic("CLZT.T")]
    public int CountLeadingZeroTrits()
    {
        int msb = BalancedTernaryEncoding.HighestNonZeroTrit(_packed, TritCount);
        return TritCount - 1 - msb;   // msb == -1 (all zero) gives 24
    }

    /// <summary>Count of non-zero trits (TCNT.T): 0..24.</summary>
    [Rebel6Intrinsic("TCNT.T")]
    public int CountNonZeroTrits() => BalancedTernaryEncoding.NonZeroTritCount(_packed & WordMask);

    public static implicit operator IntT(long value) => new IntT(value);

    public static explicit operator long(IntT value) => value.ToInt64();

    public IntT Abs() => Sign >= 0 ? this : -this;

    // ------------------------------------------------------------------
    // Packed-word implementations
    // ------------------------------------------------------------------

    /// <summary>
    /// Signed shift over the packed word: amount &gt; 0 moves toward the MST,
    /// amount &lt; 0 toward the LST, vacated fields take the fill trit.
    /// </summary>
    private static ulong ShiftPacked(ulong packed, int amount, sbyte fill)
    {
        if (amount == 0)
            return packed;

        // Replicate the fill trit's 2-bit encoding across every field:
        // 0 -> 00..., +1 -> 01... (Lo), -1 -> 11... (Lo * 3).
        ulong fillWord = (BalancedTernaryEncoding.Lo * (ulong)(fill & 3)) & WordMask;

        if (amount >= TritCount || amount <= -TritCount)
            return fillWord;

        if (amount > 0)
        {
            // Trit fields are 2 bits and zero trits are encoded as 00, so a
            // machine shift by 2*amount IS a truncating ternary shift; the
            // vacated low fields then take the (field-aligned) fill pattern.
            int bits = amount * 2;
            return ((packed << bits) & WordMask) | (fillWord >> (TritCount * 2 - bits));
        }

        int rbits = -amount * 2;
        return (packed >> rbits) | ((fillWord << (TritCount * 2 - rbits)) & WordMask);
    }

    /// <summary>The unique value in +/-(3^24-1)/2 congruent to <paramref name="value"/> mod 3^24.</summary>
    private static long Wrap(long value)
    {
        long r = value % Modulus;
        if (r > MaxMagnitude)
            r -= Modulus;
        else if (r < -MaxMagnitude)
            r += Modulus;
        return r;
    }

    private static long Wrap(Int128 value)
    {
        long r = (long)(value % Modulus);
        if (r > MaxMagnitude)
            r -= Modulus;
        else if (r < -MaxMagnitude)
            r += Modulus;
        return r;
    }
}
