using System;
using System.Numerics;

namespace stdTernary;

/// <summary>
/// trifloat24 - the 24-trit balanced-ternary floating-point format of the REBEL-6
/// ISA (ternary-workbench/docs/rebel6-trifloat24.md), packed into the low 48 bits
/// of a <see cref="ulong"/> (2 bits per trit, same encoding as <see cref="IntT"/>).
///
/// Format (spec section 2): an 18-trit balanced significand S (which carries the
/// sign of the whole number - there is no sign field), a 1-trit class C
/// (0 = finite, + = infinity, - = NaN) and a 5-trit unbiased balanced exponent E.
/// Value of a finite number: v = S x 3^E with E in [-121, +121].
///
/// Semantics implemented per the spec:
///  - Truncation rounding: dropping trailing trits IS round-to-nearest in
///    balanced ternary (spec section 1.1), so every operation computes an exact
///    (integer or rational) result and truncates.
///  - Gradual underflow, never flush-to-zero: the exponent clamps at E_min and
///    the significand keeps leading zeros (spec section 3), preserving
///    x - y == 0  iff  x == y.
///  - Canonical form (spec section 3.4): every produced finite value is
///    normalized (leading significand trit non-zero) unless E == E_min; zero is
///    S = 0, E = 0, C = 0. Non-canonical input is normalized on read
///    (<see cref="FromPacked"/>), the spec's recommended software choice.
///  - No signed zero, no exponent bias, full +/-121 exponent range usable.
///
/// Decisions the spec leaves to the implementation (section 7), as taken here:
///  - NaN is quiet; payloads are not preserved - any NaN operand produces the
///    canonical NaN (S = 0, E = 0, C = -). There is no signaling NaN.
///  - C = + with S = 0 is invalid and reads as NaN.
///  - Division by zero of a non-zero finite value yields infinity with the sign
///    of the dividend; 0/0, Inf/Inf and Inf - Inf yield NaN.
///  - Finite overflow (|result| > MaxValue) yields infinity.
///  - Exact halfway division results (possible because quotients are rationals,
///    e.g. 1/2 - the spec's no-ties argument section 1.1 covers terminating
///    expansions only) round toward zero.
///  - Conversions to integer are saturating: NaN converts to 0, +/-Inf and
///    out-of-range finites to IntT.MaxValue/MinValue; fractional trits are
///    dropped (balanced truncation = round to nearest integer).
/// </summary>
public readonly struct FloatT : IEquatable<FloatT>, IComparable<FloatT>
{
    public const int TotalTritCount = 24;
    public const int SignificandTritCount = 18;
    public const int ExponentTritCount = 5;

    // ---------------------------------------------------------------------
    // Field layout - RATIFIED as exponent-first (spec section 5.1's question
    // is closed: decided by the softfloat benchmark, recorded in
    // ternary-workbench docs/rebel6-trifloat24.md section 5.1). Packed
    // layout, trit offsets LST-first:
    //   trits 0-4 exponent | trit 5 class | trits 6-23 significand
    // This three-constant seam is no longer an open question; it remains
    // only as documentation of the mechanism - every pack/unpack in this
    // file goes through these offsets. The C++ stdTrifloat24.hpp carries
    // the identical ratified layout.
    // ---------------------------------------------------------------------
    private const int ExponentOffset = 0;
    private const int ClassOffset = ExponentOffset + ExponentTritCount; // trit 5
    private const int SignificandOffset = ClassOffset + 1;              // trits 6..23

    /// <summary>(3^18 - 1) / 2: the significand range is +/-193,710,244.</summary>
    public const long MaxSignificandMagnitude = 193_710_244L;

    /// <summary>(3^5 - 1) / 2 = 121. Balanced, unbiased; E_min = -121.</summary>
    public const int MaxExponent = 121;
    public const int MinExponent = -MaxExponent;

    /// <summary>(3^17 + 1) / 2: smallest magnitude with a non-zero leading trit.</summary>
    private const long MinNormalMagnitude = 64_570_082L;

    private const ulong SignificandMask = ((1UL << (SignificandTritCount * 2)) - 1UL) << (SignificandOffset * 2);
    private const ulong ExponentMask = ((1UL << (ExponentTritCount * 2)) - 1UL) << (ExponentOffset * 2);

    private readonly ulong _packed;

    /// <summary>Raw constructor - callers must supply canonical encodings.</summary>
    private FloatT(ulong packed)
    {
        _packed = packed;
    }

    // ---------------------------------------------------------------------
    // Special values and constants
    // ---------------------------------------------------------------------

    public static FloatT Zero { get; } = new FloatT(0UL);
    public static FloatT NaN { get; } = new FloatT(PackFields(0, -1, 0));
    public static FloatT PositiveInfinity { get; } = new FloatT(PackFields(1, 1, 0));
    public static FloatT NegativeInfinity { get; } = new FloatT(PackFields(-1, 1, 0));

    /// <summary>Largest finite value: (3^18-1)/2 x 3^121.</summary>
    public static FloatT MaxValue { get; } = new FloatT(PackFields(MaxSignificandMagnitude, 0, MaxExponent));
    public static FloatT MinValue { get; } = new FloatT(PackFields(-MaxSignificandMagnitude, 0, MaxExponent));

    /// <summary>Smallest positive value, the minimal subnormal: 1 x 3^-121.</summary>
    public static FloatT Epsilon { get; } = new FloatT(PackFields(1, 0, MinExponent));

    /// <summary>Smallest positive normalized value: (3^17+1)/2 x 3^-121.</summary>
    public static FloatT MinNormal { get; } = new FloatT(PackFields(MinNormalMagnitude, 0, MinExponent));

    public static FloatT One { get; } = RoundPack(1, 0);

    // ---------------------------------------------------------------------
    // Field access
    // ---------------------------------------------------------------------

    /// <summary>The full 24-trit encoding (low 48 bits).</summary>
    public ulong Packed => _packed;

    /// <summary>The 18-trit balanced significand S. Carries the sign of the number.</summary>
    public long Significand => BalancedTernaryEncoding.ToInt64(SignificandPacked, SignificandTritCount);

    public ulong SignificandPacked => (_packed & SignificandMask) >> (SignificandOffset * 2);

    public string SignificandString => BalancedTernaryEncoding.ToTernaryString(SignificandPacked, SignificandTritCount);

    /// <summary>The 5-trit balanced exponent E. Unbiased.</summary>
    public int Exponent => (int)BalancedTernaryEncoding.ToInt64((_packed & ExponentMask) >> (ExponentOffset * 2), ExponentTritCount);

    /// <summary>The class trit: 0 = finite, +1 = infinity, -1 = NaN.</summary>
    public sbyte ClassTrit => BalancedTernaryEncoding.DecodeTrit(_packed, ClassOffset);

    public bool IsFinite => ClassTrit == 0;
    public bool IsInfinity => ClassTrit > 0 && !BalancedTernaryEncoding.IsZero(SignificandPacked, SignificandTritCount);
    public bool IsNaN => ClassTrit < 0 || (ClassTrit > 0 && BalancedTernaryEncoding.IsZero(SignificandPacked, SignificandTritCount));
    public bool IsZero => IsFinite && SignificandPacked == 0UL;
    public bool IsSubnormal => IsFinite && !IsZero && Exponent == MinExponent
                               && BalancedTernaryEncoding.HighestNonZeroTrit(SignificandPacked, SignificandTritCount) < SignificandTritCount - 1;

    /// <summary>Sign of the value: the sign of the significand's leading non-zero trit. NaN has sign 0.</summary>
    public int Sign => IsNaN ? 0 : Math.Sign(Significand);

    // ---------------------------------------------------------------------
    // Raw encode / decode
    // ---------------------------------------------------------------------

    /// <summary>
    /// Assembles a raw (not necessarily canonical) trifloat24 encoding from its
    /// three fields. Pair with <see cref="FromPacked"/> to obtain a value.
    /// </summary>
    public static ulong PackFields(long significand, sbyte classTrit, int exponent)
    {
        if (Math.Abs(significand) > MaxSignificandMagnitude)
            throw new ArgumentOutOfRangeException(nameof(significand));
        if (exponent > MaxExponent || exponent < MinExponent)
            throw new ArgumentOutOfRangeException(nameof(exponent));

        ulong packed = BalancedTernaryEncoding.FromInt64(significand, SignificandTritCount) << (SignificandOffset * 2);
        packed |= BalancedTernaryEncoding.FromInt64(exponent, ExponentTritCount) << (ExponentOffset * 2);
        packed = BalancedTernaryEncoding.SetTrit(packed, ClassOffset, classTrit);
        return packed;
    }

    /// <summary>
    /// Reads a raw encoding, normalizing on read (spec section 3.4's recommended
    /// consumer behaviour): a non-canonical finite value is normalized, infinity
    /// is re-canonicalized to S = +/-1, E = 0, and C = + with S = 0 reads as NaN.
    /// </summary>
    public static FloatT FromPacked(ulong raw)
    {
        raw &= (1UL << (TotalTritCount * 2)) - 1UL;
        var candidate = new FloatT(raw);
        sbyte classTrit = candidate.ClassTrit;

        if (classTrit < 0)
            return NaN;
        if (classTrit > 0)
        {
            long s = candidate.Significand;
            if (s == 0)
                return NaN; // invalid encoding (spec section 4 recommendation: reject)
            return s > 0 ? PositiveInfinity : NegativeInfinity;
        }

        return RoundPack(candidate.Significand, candidate.Exponent);
    }

    /// <summary>Returns the canonical form of this value (normalize-on-read of the own encoding).</summary>
    public FloatT Normalize() => FromPacked(_packed);

    // ---------------------------------------------------------------------
    // Conversions
    // ---------------------------------------------------------------------

    public static implicit operator FloatT(double value) => FromDouble(value);
    public static implicit operator double(FloatT value) => value.ToDouble();

    public static FloatT FromDouble(double value)
    {
        if (double.IsNaN(value))
            return NaN;
        if (double.IsPositiveInfinity(value))
            return PositiveInfinity;
        if (double.IsNegativeInfinity(value))
            return NegativeInfinity;
        if (value == 0.0)
            return Zero;

        // Exact binary decomposition: value = mantissa * 2^exp2, then round the
        // exact rational to the nearest trifloat24. No double arithmetic is used.
        long bits = BitConverter.DoubleToInt64Bits(value);
        int biasedExponent = (int)((bits >> 52) & 0x7FF);
        long mantissa = bits & 0xF_FFFF_FFFF_FFFFL;
        int exp2 = biasedExponent == 0 ? -1074 : biasedExponent - 1075;
        if (biasedExponent != 0)
            mantissa |= 1L << 52;
        if (bits < 0)
            mantissa = -mantissa;

        BigInteger numerator = mantissa;
        BigInteger denominator = BigInteger.One;
        if (exp2 >= 0)
            numerator <<= exp2;
        else
            denominator <<= -exp2;

        return RoundPackRational(numerator, denominator, 0);
    }

    /// <summary>Nearest-double approximation of the value (the two formats are incomparable supersets).</summary>
    public double ToDouble()
    {
        if (IsNaN)
            return double.NaN;
        if (IsInfinity)
            return Sign > 0 ? double.PositiveInfinity : double.NegativeInfinity;
        if (IsZero)
            return 0.0;

        long s = Significand;
        int e = Exponent;
        if (e >= 0)
            return (double)(s * BigInteger.Pow(3, e));

        // Scale into an integer wide enough that the BigInteger->double conversion
        // dominates the error (3^121 < 2^192, so 260 binary guard bits suffice).
        const int GuardBits = 260;
        BigInteger q = (new BigInteger(s) << GuardBits) / BigInteger.Pow(3, -e);
        return (double)q * Math.Pow(2.0, -GuardBits);
    }

    /// <summary>Word-to-float conversion (FCVT.T.W): exact for values up to 18 trits, else rounded by truncation.</summary>
    [Rebel6Intrinsic("FCVT.T.W")]
    public static FloatT FromInt(IntT value) => RoundPack(value.ToInt64(), 0);

    /// <summary>
    /// Float-to-word conversion (FCVT.W.T), saturating: fractional trits are
    /// dropped (balanced truncation = round to nearest integer), out-of-range
    /// magnitudes clamp to IntT.MaxValue/MinValue, NaN converts to zero.
    /// </summary>
    [Rebel6Intrinsic("FCVT.W.T")]
    public IntT ToIntT()
    {
        if (IsNaN)
            return IntT.Zero;
        if (IsInfinity)
            return Sign > 0 ? IntT.MaxValue : IntT.MinValue;
        if (IsZero)
            return IntT.Zero;

        Int128 magnitude = Significand;
        int exponent = Exponent;

        while (exponent < 0 && magnitude != 0)
        {
            DropTrit(ref magnitude);
            exponent++;
        }
        while (exponent > 0)
        {
            magnitude *= 3;
            exponent--;
            if (Int128.Abs(magnitude) > IntT.MaxMagnitude)
                return Sign > 0 ? IntT.MaxValue : IntT.MinValue; // saturate early - sign cannot flip
        }

        if (Int128.Abs(magnitude) > IntT.MaxMagnitude)
            return magnitude > 0 ? IntT.MaxValue : IntT.MinValue;
        return new IntT((long)magnitude);
    }

    public override string ToString()
    {
        if (IsNaN)
            return "NaN";
        if (IsInfinity)
            return Sign > 0 ? "+Inf" : "-Inf";
        if (IsZero)
            return "0";

        return $"{SignificandString} × 3^{Exponent}";
    }

    // ---------------------------------------------------------------------
    // Comparison. FCMP.T is NaN-aware: relational operators treat NaN as
    // unordered (IEEE-style: every comparison with NaN is false, != is true),
    // while CompareTo provides the total order .NET sorting requires
    // (NaN first, as with System.Double).
    // ---------------------------------------------------------------------

    public bool Equals(FloatT other) => _packed == other._packed;

    public override bool Equals(object? obj) => obj is FloatT other && Equals(other);

    public override int GetHashCode() => _packed.GetHashCode();

    [Rebel6Intrinsic("FCMP.T")]
    public int CompareTo(FloatT other)
    {
        if (IsNaN)
            return other.IsNaN ? 0 : -1;
        if (other.IsNaN)
            return 1;
        return CompareValues(this, other);
    }

    /// <summary>Three-way compare of two non-NaN values by value (canonical-form-aware).</summary>
    private static int CompareValues(FloatT left, FloatT right)
    {
        int leftSign = left.Sign;
        int rightSign = right.Sign;
        if (leftSign != rightSign)
            return leftSign < rightSign ? -1 : 1;
        if (leftSign == 0)
            return 0;

        if (left.IsInfinity || right.IsInfinity)
        {
            if (left.IsInfinity && right.IsInfinity)
                return 0;
            // Same sign, exactly one infinite.
            return left.IsInfinity ? leftSign : -leftSign;
        }

        // Finite, same non-zero sign. Compare magnitude orders first, then align
        // and compare exactly (18-trit significands 17 trits apart fit a long).
        long ls = left.Significand, rs = right.Significand;
        int leftOrder = MsdIndex(ls) + left.Exponent;
        int rightOrder = MsdIndex(rs) + right.Exponent;
        if (leftOrder != rightOrder)
            return (leftOrder > rightOrder) == (leftSign > 0) ? 1 : -1;

        int shift = left.Exponent - right.Exponent; // |shift| <= 17 when orders match
        long la = ls, ra = rs;
        if (shift >= 0)
            la *= Pow3(shift);
        else
            ra *= Pow3(-shift);
        return la.CompareTo(ra);
    }

    public static bool operator ==(FloatT left, FloatT right)
        => !left.IsNaN && !right.IsNaN && left._packed == right._packed;

    public static bool operator !=(FloatT left, FloatT right) => !(left == right);

    public static bool operator <(FloatT left, FloatT right)
        => !left.IsNaN && !right.IsNaN && CompareValues(left, right) < 0;

    public static bool operator >(FloatT left, FloatT right)
        => !left.IsNaN && !right.IsNaN && CompareValues(left, right) > 0;

    public static bool operator <=(FloatT left, FloatT right)
        => !left.IsNaN && !right.IsNaN && CompareValues(left, right) <= 0;

    public static bool operator >=(FloatT left, FloatT right)
        => !left.IsNaN && !right.IsNaN && CompareValues(left, right) >= 0;

    // ---------------------------------------------------------------------
    // Arithmetic
    // ---------------------------------------------------------------------

    public static FloatT operator +(FloatT left, FloatT right) => Add(left, right);
    public static FloatT operator -(FloatT left, FloatT right) => Subtract(left, right);
    public static FloatT operator *(FloatT left, FloatT right) => Multiply(left, right);
    public static FloatT operator /(FloatT left, FloatT right) => Divide(left, right);
    public static FloatT operator -(FloatT value) => value.Negate();

    /// <summary>
    /// Negation is digit-wise negation of the significand (spec section 2.1) -
    /// exact and total. NaN stays NaN; infinities flip sign.
    /// </summary>
    public FloatT Negate()
    {
        if (IsNaN)
            return NaN;
        if (IsInfinity)
            return Sign > 0 ? NegativeInfinity : PositiveInfinity;
        if (IsZero)
            return Zero; // the single zero (no signed zero)
        return new FloatT(PackFields(-Significand, 0, Exponent));
    }

    [Rebel6Intrinsic("FADD.T")]
    private static FloatT Add(FloatT left, FloatT right)
    {
        if (left.IsNaN || right.IsNaN)
            return NaN;
        if (left.IsInfinity || right.IsInfinity)
        {
            if (!left.IsInfinity)
                return right;
            if (!right.IsInfinity)
                return left;
            return left.Sign == right.Sign ? left : NaN; // Inf - Inf
        }
        if (left.IsZero)
            return right;
        if (right.IsZero)
            return left;

        if (left.Exponent < right.Exponent)
            (left, right) = (right, left);

        int shift = left.Exponent - right.Exponent;
        if (shift >= 20)
        {
            // The smaller operand is below half an ulp of any possible result
            // (|S2| <= (3^18-1)/2 < 3^(shift-1)/2, and normalization after the add
            // can lower the result exponent by at most one), so truncation rounds
            // it away entirely and the larger operand - already canonical - is
            // the exact answer. Ties are impossible: 3^k/2 is not an integer.
            return left;
        }

        // Exact aligned sum: |S1| * 3^19 + |S2| < 2^61, comfortably in range.
        long sum = left.Significand * Pow3(shift) + right.Significand;
        return RoundPack(sum, right.Exponent);
    }

    [Rebel6Intrinsic("FSUB.T")]
    private static FloatT Subtract(FloatT left, FloatT right) => Add(left, right.Negate());

    [Rebel6Intrinsic("FMUL.T")]
    private static FloatT Multiply(FloatT left, FloatT right)
    {
        if (left.IsNaN || right.IsNaN)
            return NaN;
        if (left.IsInfinity || right.IsInfinity)
        {
            if (left.IsZero || right.IsZero)
                return NaN; // Inf * 0
            return left.Sign * right.Sign > 0 ? PositiveInfinity : NegativeInfinity;
        }

        // The exact product spans at most 36 trits; a single truncation rounds it.
        return RoundPack((Int128)left.Significand * right.Significand,
                         left.Exponent + right.Exponent);
    }

    [Rebel6Intrinsic("FDIV.T")]
    private static FloatT Divide(FloatT left, FloatT right)
    {
        if (left.IsNaN || right.IsNaN)
            return NaN;
        if (left.IsInfinity)
        {
            if (right.IsInfinity)
                return NaN; // Inf / Inf
            // Zero is unsigned, so Inf / 0 keeps the dividend's sign.
            int sign = right.IsZero ? left.Sign : left.Sign * right.Sign;
            return sign > 0 ? PositiveInfinity : NegativeInfinity;
        }
        if (right.IsInfinity)
            return Zero;
        if (right.IsZero)
        {
            if (left.IsZero)
                return NaN; // 0 / 0
            return left.Sign > 0 ? PositiveInfinity : NegativeInfinity;
        }
        if (left.IsZero)
            return Zero;

        return RoundPackRational(left.Significand, right.Significand,
                                 left.Exponent - right.Exponent);
    }

    /// <summary>
    /// Fused multiply-add (FMA.T): a * b + c with a single rounding - the exact
    /// product is added at full precision before truncation.
    /// </summary>
    [Rebel6Intrinsic("FMA.T")]
    public static FloatT FusedMultiplyAdd(FloatT a, FloatT b, FloatT c)
    {
        if (a.IsNaN || b.IsNaN || c.IsNaN)
            return NaN;
        if (a.IsInfinity || b.IsInfinity)
        {
            if (a.IsZero || b.IsZero)
                return NaN; // Inf * 0
            int productSign = a.Sign * b.Sign;
            if (c.IsInfinity && c.Sign != productSign)
                return NaN; // Inf - Inf
            return productSign > 0 ? PositiveInfinity : NegativeInfinity;
        }
        if (c.IsInfinity)
            return c;

        Int128 product = (Int128)a.Significand * b.Significand;
        if (product == 0)
            return c;
        int productExponent = a.Exponent + b.Exponent;
        if (c.IsZero)
            return RoundPack(product, productExponent);

        // Single rounding over the exact sum. The 36-trit product and the 18-trit
        // addend can sit up to ~484 exponents apart, so the alignment is done in
        // BigInteger arithmetic; the rational rounder then truncates once.
        int baseExponent = Math.Min(productExponent, c.Exponent);
        BigInteger sum = (BigInteger)product * BigInteger.Pow(3, productExponent - baseExponent)
                       + (BigInteger)c.Significand * BigInteger.Pow(3, c.Exponent - baseExponent);
        return RoundPackRational(sum, BigInteger.One, baseExponent);
    }

    /// <summary>Adds one ulp times <paramref name="delta"/> (used by MathT.TritIncrement/TritDecrement).</summary>
    internal FloatT AdjustByTrit(int delta)
    {
        if (!IsFinite || delta == 0)
            return this;
        if (IsZero)
            return RoundPack(delta, MinExponent);
        return RoundPack((Int128)Significand + delta, Exponent);
    }

    // ---------------------------------------------------------------------
    // Rounding core
    // ---------------------------------------------------------------------

    /// <summary>
    /// Canonical finite value nearest to significand x 3^exponent, following the
    /// format's rounding and underflow rules: balanced truncation of trailing
    /// trits (= round-to-nearest, carry-free per digit), gradual underflow with
    /// the exponent clamped at E_min, and overflow to infinity.
    /// </summary>
    private static FloatT RoundPack(Int128 significand, int exponent)
    {
        if (significand == 0)
            return Zero;

        // Truncate while the significand is too wide or the exponent is below the
        // floor (the latter is the gradual-underflow path - digits are dropped,
        // the value is never flushed).
        while (Int128.Abs(significand) > MaxSignificandMagnitude || exponent < MinExponent)
        {
            DropTrit(ref significand);
            exponent++;
            if (significand == 0)
                return Zero; // underflowed below half the smallest subnormal
        }

        // Canonical form: normalize unless the exponent floor stops us.
        while (exponent > MinExponent && Int128.Abs(significand) < MinNormalMagnitude)
        {
            significand *= 3;
            exponent--;
        }

        if (exponent > MaxExponent)
            return significand > 0 ? PositiveInfinity : NegativeInfinity;

        return new FloatT(PackFields((long)significand, 0, exponent));
    }

    /// <summary>
    /// Canonical finite value nearest to (numerator / denominator) x 3^exponent.
    /// The quotient is carried to at least two guard trits and rounded to the
    /// nearest integer using the exact remainder (halfway cases toward zero);
    /// the guard trits are then dropped by ordinary balanced truncation.
    /// </summary>
    private static FloatT RoundPackRational(BigInteger numerator, BigInteger denominator, int exponent)
    {
        if (numerator.IsZero)
            return Zero;

        int sign = numerator.Sign * denominator.Sign;
        BigInteger n = BigInteger.Abs(numerator);
        BigInteger d = BigInteger.Abs(denominator);

        BigInteger lower = d * GuardThreshold;       // quotient >= 3^19: >= 2 guard trits
        while (n < lower)
        {
            n *= 3;
            exponent--;
        }
        BigInteger upper = lower * 3;
        while (n >= upper)
        {
            d *= 3;
            upper *= 3;
            exponent++;
        }

        BigInteger q = BigInteger.DivRem(n, d, out BigInteger r);
        if (r * 2 > d)
            q += 1; // nearest; exact halves (r*2 == d) stay truncated toward zero

        Int128 rounded = (Int128)q;
        return RoundPack(sign < 0 ? -rounded : rounded, exponent);
    }

    /// <summary>Removes the least significant balanced trit: carry-free round-to-nearest by one digit.</summary>
    private static void DropTrit(ref Int128 value)
    {
        Int128 r = value % 3;
        if (r == 2)
            r = -1;
        else if (r == -2)
            r = 1;
        value = (value - r) / 3;
    }

    /// <summary>Index of the most significant non-zero trit of a non-zero significand (0..17).</summary>
    private static int MsdIndex(long significand)
    {
        long magnitude = Math.Abs(significand);
        int index = 0;
        // The digit at position i is non-zero iff |value| > (3^i - 1) / 2.
        while (index < SignificandTritCount - 1 && magnitude > (Pow3(index + 1) - 1) / 2)
            index++;
        return index;
    }

    private static long Pow3(int exponent)
    {
        long result = 1;
        for (int i = 0; i < exponent; i++)
            result *= 3;
        return result;
    }

    private static readonly BigInteger GuardThreshold = BigInteger.Pow(3, SignificandTritCount + 1);
}
