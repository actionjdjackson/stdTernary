namespace stdTernary.Tests;

using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stdTernary;

/// <summary>
/// Tests for the trifloat24 implementation of FloatT, following the test
/// requirements of ternary-workbench/docs/rebel6-trifloat24.md section 6:
/// the underflow identity, canonical form, a differential check against exact
/// BigInteger-rational reference arithmetic (never double), NaN/Inf handling,
/// no signed zero, and saturating conversions.
/// </summary>
[TestClass]
public class Trifloat24Tests
{
    private const long MaxSignificand = 193_710_244L;    // (3^18 - 1) / 2
    private const long MinNormalSignificand = 64_570_082L; // (3^17 + 1) / 2

    // =====================================================================
    // Exact BigInteger-rational reference (spec section 6: "Use rational
    // arithmetic, not double"). Independent of the implementation: it finds
    // the smallest exponent >= E_min whose nearest significand fits 18 trits,
    // rounding the exact rational anew at each candidate - no shared rounding
    // code with the library.
    // =====================================================================

    private static BigInteger Pow3(int exponent) => BigInteger.Pow(3, exponent);

    private static BigInteger DivideToNearest(BigInteger numerator, BigInteger denominator)
    {
        BigInteger quotient = BigInteger.DivRem(numerator, denominator, out BigInteger remainder);
        if (remainder * 2 > denominator)
            quotient += 1; // exact halves stay truncated toward zero, matching the format
        return quotient;
    }

    /// <summary>Nearest significand for candidate exponent e: round(|num| / (den * 3^(e - exp3))).</summary>
    private static BigInteger SignificandAt(BigInteger num, BigInteger den, int exp3, int e)
    {
        int scale = exp3 - e;
        return scale >= 0
            ? DivideToNearest(num * Pow3(scale), den)
            : DivideToNearest(num, den * Pow3(-scale));
    }

    /// <summary>Reference rounding of the exact rational (num / den) * 3^exp3 to canonical trifloat24.</summary>
    private static FloatT RefRound(BigInteger num, BigInteger den, int exp3)
    {
        if (num.IsZero)
            return FloatT.Zero;

        int sign = num.Sign * den.Sign;
        BigInteger n = BigInteger.Abs(num);
        BigInteger d = BigInteger.Abs(den);

        // Start from an estimate, then walk to the smallest exponent >= E_min
        // whose rounded significand fits. Minimality gives canonical form.
        int e = (int)Math.Round(BigInteger.Log(n, 3.0) - BigInteger.Log(d, 3.0)) + exp3 - 18;
        if (e < FloatT.MinExponent)
            e = FloatT.MinExponent;

        BigInteger s = SignificandAt(n, d, exp3, e);
        while (s > MaxSignificand)
        {
            e++;
            s = SignificandAt(n, d, exp3, e);
        }
        while (e > FloatT.MinExponent && SignificandAt(n, d, exp3, e - 1) <= MaxSignificand)
        {
            e--;
            s = SignificandAt(n, d, exp3, e);
        }

        if (s.IsZero)
            return FloatT.Zero;
        if (e > FloatT.MaxExponent)
            return sign > 0 ? FloatT.PositiveInfinity : FloatT.NegativeInfinity;
        return FloatT.FromPacked(FloatT.PackFields(sign < 0 ? -(long)s : (long)s, 0, e));
    }

    private static void AssertCanonical(FloatT value, string context)
    {
        if (value.IsNaN)
        {
            Assert.AreEqual(FloatT.NaN.Packed, value.Packed, $"non-canonical NaN: {context}");
            return;
        }
        if (value.IsInfinity)
        {
            ulong expected = value.Sign > 0 ? FloatT.PositiveInfinity.Packed : FloatT.NegativeInfinity.Packed;
            Assert.AreEqual(expected, value.Packed, $"non-canonical infinity: {context}");
            return;
        }
        if (value.IsZero)
        {
            Assert.AreEqual(0UL, value.Packed, $"non-canonical zero: {context}");
            return;
        }

        long s = value.Significand;
        int e = value.Exponent;
        Assert.IsTrue(Math.Abs(s) <= MaxSignificand, $"significand out of range: {context}");
        Assert.IsTrue(e >= FloatT.MinExponent && e <= FloatT.MaxExponent, $"exponent out of range: {context}");
        if (e > FloatT.MinExponent)
            Assert.IsTrue(Math.Abs(s) >= MinNormalSignificand,
                $"unnormalized significand {s} with exponent {e} above E_min: {context}");
    }

    /// <summary>Random canonical finite value; exponent -121 draws exercise subnormals.</summary>
    private static FloatT RandomFinite(Random random)
    {
        long significand = random.NextInt64(-MaxSignificand, MaxSignificand + 1);
        int exponent = random.Next(FloatT.MinExponent, FloatT.MaxExponent + 1);
        return FloatT.FromPacked(FloatT.PackFields(significand, 0, exponent));
    }

    private static IEnumerable<FloatT> DirectedFixtures()
    {
        yield return FloatT.Zero;
        yield return FloatT.One;
        yield return FloatT.One.Negate();
        yield return FloatT.MaxValue;                 // +max normal
        yield return FloatT.MinValue;                 // -max normal
        yield return FloatT.MinNormal;                // +min normal
        yield return FloatT.MinNormal.Negate();
        yield return FloatT.Epsilon;                  // +min subnormal
        yield return FloatT.Epsilon.Negate();

        // Significands with 0..17 leading zero trits at E_min (subnormal ladder)
        // and at E_max (all normalized on read).
        for (int leadingZeros = 0; leadingZeros <= 17; leadingZeros++)
        {
            long s = (long)(Pow3(17 - leadingZeros) + 1) / 2; // msd exactly at 17 - leadingZeros
            yield return FloatT.FromPacked(FloatT.PackFields(s, 0, FloatT.MinExponent));
            yield return FloatT.FromPacked(FloatT.PackFields(-s, 0, FloatT.MinExponent));
            yield return FloatT.FromPacked(FloatT.PackFields(s, 0, FloatT.MaxExponent));
        }
    }

    // =====================================================================
    // Format and special values
    // =====================================================================

    [TestMethod]
    public void FormatConstantsMatchSpec()
    {
        Assert.AreEqual(24, FloatT.TotalTritCount);
        Assert.AreEqual(18, FloatT.SignificandTritCount);
        Assert.AreEqual(5, FloatT.ExponentTritCount);
        Assert.AreEqual(193_710_244L, FloatT.MaxSignificandMagnitude);
        Assert.AreEqual(121, FloatT.MaxExponent);
        Assert.AreEqual(-121, FloatT.MinExponent);

        Assert.AreEqual(MaxSignificand, FloatT.MaxValue.Significand);
        Assert.AreEqual(121, FloatT.MaxValue.Exponent);      // E_max usable, nothing reserved
        Assert.AreEqual(1L, FloatT.Epsilon.Significand);
        Assert.AreEqual(-121, FloatT.Epsilon.Exponent);      // E_min usable
        Assert.IsTrue(FloatT.Epsilon.IsSubnormal);
        Assert.IsFalse(FloatT.MinNormal.IsSubnormal);
    }

    [TestMethod]
    public void ClassTritDecodesSpecialValues()
    {
        Assert.AreEqual(0, (int)FloatT.Zero.ClassTrit);
        Assert.AreEqual(1, (int)FloatT.PositiveInfinity.ClassTrit);
        Assert.AreEqual(1, (int)FloatT.NegativeInfinity.ClassTrit);
        Assert.AreEqual(-1, (int)FloatT.NaN.ClassTrit);

        Assert.IsTrue(FloatT.PositiveInfinity.IsInfinity);
        Assert.AreEqual(1, FloatT.PositiveInfinity.Sign);
        Assert.AreEqual(-1, FloatT.NegativeInfinity.Sign);
        Assert.IsTrue(FloatT.NaN.IsNaN);
        Assert.IsFalse(FloatT.NaN.IsInfinity);

        // C = + with S = 0 is an invalid encoding and reads as NaN (section 4).
        FloatT invalid = FloatT.FromPacked(FloatT.PackFields(0, 1, 17));
        Assert.IsTrue(invalid.IsNaN);
    }

    // =====================================================================
    // The underflow identity (spec section 6, "the test that fails under FTZ")
    // =====================================================================

    [TestMethod]
    public void UnderflowIdentity_AdjacentNormalsAtEmin()
    {
        // The worked example of section 3.1: two adjacent representable numbers
        // at the bottom of the normal range. Their difference is 1 x 3^E_min -
        // exact, non-zero, and representable only through gradual underflow.
        FloatT x = FloatT.FromPacked(FloatT.PackFields(MinNormalSignificand + 1, 0, FloatT.MinExponent));
        FloatT y = FloatT.FromPacked(FloatT.PackFields(MinNormalSignificand, 0, FloatT.MinExponent));

        FloatT difference = x - y;
        Assert.IsFalse(x == y);
        Assert.IsFalse(difference.IsZero, "flush-to-zero detected: x != y but x - y == 0");
        Assert.IsTrue(difference == FloatT.Epsilon);

        // The guard that motivates the property does not divide by zero.
        FloatT quotient = FloatT.One / difference;
        Assert.IsFalse(quotient.IsNaN);
        Assert.IsFalse(quotient.IsInfinity);
    }

    [TestMethod]
    public void UnderflowIdentity_PropertyTest()
    {
        var random = new Random(6001);
        for (int i = 0; i < 20_000; i++)
        {
            FloatT x = RandomFinite(random);
            FloatT y = i % 4 == 0
                ? FloatT.FromPacked(FloatT.PackFields(
                      Math.Clamp(x.Significand + random.Next(-2, 3), -MaxSignificand, MaxSignificand), 0, x.Exponent))
                : RandomFinite(random);

            FloatT difference = x - y;
            Assert.AreEqual(x == y, difference.IsZero,
                $"underflow identity violated for {x} - {y}");
        }
    }

    [TestMethod]
    public void GradualUnderflowProducesSubnormals()
    {
        // MinNormal / 3 is below the normal range but far above the subnormal
        // floor - flush-to-zero would still represent it, but as a denormal it
        // must keep 17 significant trits, not be renormalized past E_min.
        FloatT third = FloatT.MinNormal / FloatT.FromInt(new IntT(3));
        Assert.IsTrue(third.IsSubnormal);
        Assert.AreEqual(FloatT.MinExponent, third.Exponent);
        Assert.AreEqual((MinNormalSignificand + 1) / 3, third.Significand); // nearest of x.5 - 1/6

        // Epsilon is the floor: half of it underflows to the single zero.
        FloatT half = FloatT.Epsilon / FloatT.FromInt(new IntT(3));
        Assert.IsTrue(half.IsZero);

        // Subnormal arithmetic is exact where the format allows.
        Assert.IsTrue(FloatT.Epsilon + FloatT.Epsilon == FloatT.FromPacked(FloatT.PackFields(2, 0, FloatT.MinExponent)));
    }

    // =====================================================================
    // Canonical form (spec sections 3.4 and 6)
    // =====================================================================

    [TestMethod]
    public void CanonicalFormRoundTrips()
    {
        var random = new Random(6002);
        foreach (FloatT value in DirectedFixtures())
            Assert.AreEqual(value.Packed, FloatT.FromPacked(value.Packed).Packed, "encode-decode-encode");

        for (int i = 0; i < 5_000; i++)
        {
            FloatT value = RandomFinite(random);
            AssertCanonical(value, $"random fixture {value}");
            Assert.AreEqual(value.Packed, FloatT.FromPacked(value.Packed).Packed, "encode-decode-encode");
        }
    }

    [TestMethod]
    public void NonCanonicalInputIsNormalizedOnRead()
    {
        // S = 3, E = 4 and S = 1, E = 5 denote the same value (243) and must
        // compare equal; normalize-on-read maps both to one canonical encoding.
        FloatT a = FloatT.FromPacked(FloatT.PackFields(3, 0, 4));
        FloatT b = FloatT.FromPacked(FloatT.PackFields(1, 0, 5));

        Assert.AreEqual(a.Packed, b.Packed);
        Assert.IsTrue(a == b);
        AssertCanonical(a, "normalized 3 x 3^4");
        Assert.AreEqual(243L, a.ToIntT().ToInt64());

        // A subnormal-looking significand above E_min normalizes.
        FloatT c = FloatT.FromPacked(FloatT.PackFields(27, 0, 0));
        AssertCanonical(c, "normalized 27 x 3^0");
        Assert.AreEqual(27L, c.ToIntT().ToInt64());
    }

    [TestMethod]
    public void EveryOperationEmitsCanonicalForm()
    {
        var random = new Random(6003);
        for (int i = 0; i < 5_000; i++)
        {
            FloatT x = RandomFinite(random);
            FloatT y = RandomFinite(random);

            AssertCanonical(x + y, $"{x} + {y}");
            AssertCanonical(x - y, $"{x} - {y}");
            AssertCanonical(x * y, $"{x} * {y}");
            AssertCanonical(x / y, $"{x} / {y}");
            AssertCanonical(-x, $"-{x}");
        }
    }

    // =====================================================================
    // Differential against the exact rational reference (spec section 6)
    // =====================================================================

    [TestMethod]
    public void ArithmeticMatchesExactRationalReference()
    {
        var random = new Random(6004);
        var fixtures = new List<FloatT>(DirectedFixtures());
        int checks = 0;

        for (int i = 0; i < 12_000; i++)
        {
            FloatT x = i < fixtures.Count * fixtures.Count
                ? fixtures[i / fixtures.Count]
                : RandomFinite(random);
            FloatT y = i < fixtures.Count * fixtures.Count
                ? fixtures[i % fixtures.Count]
                : RandomFinite(random);

            BigInteger sx = x.Significand, sy = y.Significand;
            int ex = x.Exponent, ey = y.Exponent;
            int baseExponent = Math.Min(ex, ey);
            BigInteger ax = sx * Pow3(ex - baseExponent);
            BigInteger ay = sy * Pow3(ey - baseExponent);

            AssertSameValue(RefRound(ax + ay, BigInteger.One, baseExponent), x + y, $"{x} + {y}");
            AssertSameValue(RefRound(ax - ay, BigInteger.One, baseExponent), x - y, $"{x} - {y}");
            AssertSameValue(RefRound(sx * sy, BigInteger.One, ex + ey), x * y, $"{x} * {y}");
            if (!y.IsZero)
                AssertSameValue(RefRound(sx, sy, ex - ey), x / y, $"{x} / {y}");
            checks += 4;
        }

        Assert.IsTrue(checks > 40_000);
    }

    [TestMethod]
    public void FusedMultiplyAddMatchesExactReferenceWithSingleRounding()
    {
        var random = new Random(6005);
        for (int i = 0; i < 4_000; i++)
        {
            FloatT a = RandomFinite(random);
            FloatT b = RandomFinite(random);
            FloatT c = RandomFinite(random);

            int productExponent = a.Exponent + b.Exponent;
            int baseExponent = Math.Min(productExponent, c.Exponent);
            BigInteger exact = (BigInteger)a.Significand * b.Significand * Pow3(productExponent - baseExponent)
                             + (BigInteger)c.Significand * Pow3(c.Exponent - baseExponent);

            AssertSameValue(RefRound(exact, BigInteger.One, baseExponent),
                            FloatT.FusedMultiplyAdd(a, b, c), $"fma({a}, {b}, {c})");
        }

        // FMA specials: Inf * 0 inside the product is NaN even with a finite addend.
        Assert.IsTrue(FloatT.FusedMultiplyAdd(FloatT.PositiveInfinity, FloatT.Zero, FloatT.One).IsNaN);
        Assert.IsTrue(FloatT.FusedMultiplyAdd(FloatT.PositiveInfinity, FloatT.One, FloatT.NegativeInfinity).IsNaN);
        Assert.IsTrue(FloatT.FusedMultiplyAdd(FloatT.One, FloatT.One, FloatT.PositiveInfinity) == FloatT.PositiveInfinity);
    }

    private static void AssertSameValue(FloatT expected, FloatT actual, string context)
    {
        Assert.AreEqual(expected.Packed, actual.Packed,
            $"{context}: expected {expected}, got {actual}");
    }

    // =====================================================================
    // Truncation rounding
    // =====================================================================

    [TestMethod]
    public void TruncationIsRoundToNearest()
    {
        // 19-trit integers must round to the nearest 18-trit significand.
        // (3^24 - 1)/2 is +...+ (24 plus trits): dropping six +1 trits leaves
        // (3^18 - 1)/2 exactly - the truncated tail is below half an ulp.
        FloatT wide = FloatT.FromInt(IntT.MaxValue);
        Assert.AreEqual(MaxSignificand, wide.Significand);
        Assert.AreEqual(6, wide.Exponent);

        // A rational with an exact halfway significand rounds toward zero
        // (documented tie rule; ties are possible only for rationals).
        FloatT half = FloatT.One / FloatT.FromInt(new IntT(2));
        Assert.AreEqual(MaxSignificand, half.Significand); // round(0.5 * 3^18) toward zero
        Assert.AreEqual(-18, half.Exponent);
    }

    // =====================================================================
    // NaN and infinity propagation (spec section 6, special values)
    // =====================================================================

    [TestMethod]
    public void NaNPropagatesThroughAllOperations()
    {
        FloatT[] others = { FloatT.Zero, FloatT.One, FloatT.MaxValue, FloatT.Epsilon, FloatT.PositiveInfinity };

        foreach (FloatT other in others)
        {
            Assert.IsTrue((FloatT.NaN + other).IsNaN);
            Assert.IsTrue((other + FloatT.NaN).IsNaN);
            Assert.IsTrue((FloatT.NaN - other).IsNaN);
            Assert.IsTrue((FloatT.NaN * other).IsNaN);
            Assert.IsTrue((FloatT.NaN / other).IsNaN);
            Assert.IsTrue((other / FloatT.NaN).IsNaN);
            Assert.IsTrue(FloatT.FusedMultiplyAdd(FloatT.NaN, other, other).IsNaN);
        }

        Assert.IsTrue((-FloatT.NaN).IsNaN);
        // NaN operands always produce the canonical quiet NaN.
        Assert.AreEqual(FloatT.NaN.Packed, (FloatT.NaN + FloatT.One).Packed);
    }

    [TestMethod]
    public void InfinityArithmeticFollowsTheUsualRules()
    {
        FloatT inf = FloatT.PositiveInfinity;
        FloatT two = FloatT.FromInt(new IntT(2));

        Assert.IsTrue((inf + two) == inf);
        Assert.IsTrue((inf + inf) == inf);
        Assert.IsTrue((inf - inf).IsNaN);
        Assert.IsTrue((inf * two.Negate()) == FloatT.NegativeInfinity);
        Assert.IsTrue((inf * FloatT.Zero).IsNaN);
        Assert.IsTrue((inf / inf).IsNaN);
        Assert.IsTrue((two / inf).IsZero);
        Assert.IsTrue((two / FloatT.Zero) == inf);
        Assert.IsTrue((two.Negate() / FloatT.Zero) == FloatT.NegativeInfinity);
        Assert.IsTrue((FloatT.Zero / FloatT.Zero).IsNaN);

        // Finite overflow saturates to infinity (documented decision).
        Assert.IsTrue((FloatT.MaxValue * FloatT.MaxValue) == inf);
        Assert.IsTrue((FloatT.MaxValue + FloatT.MaxValue) == inf);
        Assert.IsTrue((FloatT.MinValue * FloatT.MaxValue) == FloatT.NegativeInfinity);
    }

    [TestMethod]
    public void ComparisonIsNaNAware()
    {
        // Relational operators: NaN is unordered (every comparison false, != true).
        Assert.IsFalse(FloatT.NaN == FloatT.NaN);
        Assert.IsTrue(FloatT.NaN != FloatT.NaN);
        Assert.IsFalse(FloatT.NaN < FloatT.One);
        Assert.IsFalse(FloatT.NaN > FloatT.One);
        Assert.IsFalse(FloatT.NaN <= FloatT.NegativeInfinity);
        Assert.IsFalse(FloatT.One >= FloatT.NaN);

        // CompareTo is the total order .NET sorting needs: NaN first.
        Assert.AreEqual(0, FloatT.NaN.CompareTo(FloatT.NaN));
        Assert.IsTrue(FloatT.NaN.CompareTo(FloatT.NegativeInfinity) < 0);
        Assert.IsTrue(FloatT.PositiveInfinity.CompareTo(FloatT.MaxValue) > 0);
        Assert.IsTrue(FloatT.NegativeInfinity.CompareTo(FloatT.MinValue) < 0);

        // Value ordering across normals and subnormals, canonical-form-aware.
        var random = new Random(6006);
        for (int i = 0; i < 5_000; i++)
        {
            FloatT x = RandomFinite(random);
            FloatT y = RandomFinite(random);

            double dx = x.ToDouble(), dy = y.ToDouble();
            if (dx != dy) // double comparison is only used to pick an expectation on clearly distinct values
                Assert.AreEqual(dx < dy, x < y, $"ordering of {x} vs {y}");
        }

        Assert.IsTrue(FloatT.Epsilon > FloatT.Zero);
        Assert.IsTrue(FloatT.Epsilon.Negate() < FloatT.Zero);
        Assert.IsTrue(FloatT.MinNormal > FloatT.Epsilon);
    }

    // =====================================================================
    // No signed zero (spec section 6)
    // =====================================================================

    [TestMethod]
    public void ThereIsExactlyOneZero()
    {
        Assert.AreEqual(0UL, (-FloatT.Zero).Packed);
        Assert.AreEqual(0UL, (FloatT.Zero + FloatT.Zero).Packed);
        Assert.AreEqual(0UL, (FloatT.One - FloatT.One).Packed);
        Assert.AreEqual(0UL, (FloatT.Epsilon - FloatT.Epsilon).Packed);
        Assert.AreEqual(0UL, (FloatT.Zero * FloatT.MinValue).Packed);

        var random = new Random(6007);
        for (int i = 0; i < 2_000; i++)
        {
            FloatT x = RandomFinite(random);
            Assert.AreEqual(0UL, (x - x).Packed, $"x - x for {x}");
        }
    }

    // =====================================================================
    // Saturating conversions
    // =====================================================================

    [TestMethod]
    public void IntConversionsSaturate()
    {
        Assert.AreEqual(IntT.Zero, FloatT.NaN.ToIntT());
        Assert.AreEqual(IntT.MaxValue, FloatT.PositiveInfinity.ToIntT());
        Assert.AreEqual(IntT.MinValue, FloatT.NegativeInfinity.ToIntT());
        Assert.AreEqual(IntT.MaxValue, FloatT.MaxValue.ToIntT());
        Assert.AreEqual(IntT.MinValue, FloatT.MinValue.ToIntT());
        Assert.AreEqual(IntT.Zero, FloatT.Epsilon.ToIntT());

        // FromInt is exact up to 18 trits and correctly rounded beyond:
        // (3^24-1)/2 is 24 plus-trits, and dropping the low six leaves
        // (3^24 - 3^6)/2 - the nearest 18-trit value, still inside the word range.
        Assert.AreEqual(new IntT(141_214_767_876L), FloatT.FromInt(IntT.MaxValue).ToIntT());
        Assert.AreEqual(new IntT(-141_214_767_876L), FloatT.FromInt(IntT.MinValue).ToIntT());

        // One trit higher the magnitude leaves the word range and saturates.
        FloatT beyond = FloatT.FromPacked(FloatT.PackFields(MaxSignificand, 0, 7));
        Assert.AreEqual(IntT.MaxValue, beyond.ToIntT());
        Assert.AreEqual(IntT.MinValue, beyond.Negate().ToIntT());

        var random = new Random(6008);
        for (int i = 0; i < 2_000; i++)
        {
            long value = random.NextInt64(-MaxSignificand, MaxSignificand + 1);
            Assert.AreEqual(value, FloatT.FromInt(new IntT(value)).ToIntT().ToInt64(), $"exact round trip {value}");
        }

        // Fractional trits are dropped: balanced truncation = round to nearest.
        Assert.AreEqual(0L, (FloatT.One / FloatT.FromInt(new IntT(3))).ToIntT().ToInt64());
        Assert.AreEqual(5L, FloatT.FromDouble(4.9).ToIntT().ToInt64());
        Assert.AreEqual(-5L, FloatT.FromDouble(-4.9).ToIntT().ToInt64());
    }

    [TestMethod]
    public void DoubleConversionsHandleSpecialsAndRoundTrip()
    {
        Assert.IsTrue(FloatT.FromDouble(double.NaN).IsNaN);
        Assert.IsTrue(FloatT.FromDouble(double.PositiveInfinity) == FloatT.PositiveInfinity);
        Assert.IsTrue(FloatT.FromDouble(double.NegativeInfinity) == FloatT.NegativeInfinity);
        Assert.AreEqual(0UL, FloatT.FromDouble(0.0).Packed);
        Assert.AreEqual(0UL, FloatT.FromDouble(-0.0).Packed); // no signed zero

        Assert.IsTrue(double.IsNaN(FloatT.NaN.ToDouble()));
        Assert.IsTrue(double.IsPositiveInfinity(FloatT.PositiveInfinity.ToDouble()));

        var random = new Random(6009);
        for (int i = 0; i < 2_000; i++)
        {
            double value = (random.NextDouble() - 0.5) * Math.Pow(10, random.Next(-30, 31));
            if (value == 0.0)
                continue;
            double round = FloatT.FromDouble(value).ToDouble();
            // Worst relative error is half an ulp at the smallest normalized
            // significand: 1 / (2 * (3^17+1)/2) ~ 7.75e-9, plus double rounding.
            Assert.IsTrue(Math.Abs(round - value) <= Math.Abs(value) * 8e-9,
                $"lossy double round trip: {value} -> {round}");
        }
    }
}
