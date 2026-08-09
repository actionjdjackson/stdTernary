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

    public static IntT operator &(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        return new IntT(BalancedTernaryEncoding.MinSwar(left._packed, right._packed) & WordMask);
    }

    public static IntT operator |(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        return new IntT(BalancedTernaryEncoding.MaxSwar(left._packed, right._packed) & WordMask);
    }

    public static IntT operator ^(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        return new IntT(BalancedTernaryEncoding.XorSwar(left._packed, right._packed) & WordMask);
    }

    public static IntT operator ~(IntT value) => -value;

    /// <summary>Left shift by an immediate, zero fill (SLIZ.T). High trits are dropped (balanced wrap).</summary>
    [Rebel6Intrinsic("SLIZ.T")]
    public static IntT operator <<(IntT value, int count)
    {
        if (TernaryFom.Enabled) TernaryFom.Shifts++;
        return new IntT(ShiftLeftPacked(value._packed, count));
    }

    /// <summary>Right shift by an immediate, zero fill (SRIZ.T).</summary>
    [Rebel6Intrinsic("SRIZ.T")]
    public static IntT operator >>(IntT value, int count)
    {
        if (TernaryFom.Enabled) TernaryFom.Shifts++;
        return new IntT(ShiftRightPacked(value._packed, count));
    }

    /// <summary>
    /// Cyclic rotate by <paramref name="count"/> mod 24 (ROT.T) - no fill; positive
    /// counts rotate toward the most significant trit.
    /// </summary>
    [Rebel6Intrinsic("ROT.T")]
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

    private static ulong ShiftLeftPacked(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftRightPacked(packed, -count);
        if (count >= TritCount)
            return 0UL;

        // Zero trits are encoded as 00, so a machine shift by 2*count IS a
        // truncating ternary shift: dropped high trits fall off, zeros shift in.
        return (packed << (count * 2)) & WordMask;
    }

    private static ulong ShiftRightPacked(ulong packed, int count)
    {
        if (count == 0)
            return packed;
        if (count < 0)
            return ShiftLeftPacked(packed, -count);
        if (count >= TritCount)
            return 0UL;

        return packed >> (count * 2);
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
