namespace stdTernary.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stdTernary;

[TestClass]
public class Rebel6IntTTests
{
    private const long Modulus = 282_429_536_481L;          // 3^24
    private const long Max = 141_214_768_240L;              // (3^24 - 1) / 2

    private static long WrapExpected(Int128 value)
    {
        long r = (long)(value % Modulus);
        if (r > Max)
            r -= Modulus;
        else if (r < -Max)
            r += Modulus;
        return r;
    }

    [TestMethod]
    public void RangeConstantsMatchSpec()
    {
        Assert.AreEqual(Max, IntT.MaxValue.ToInt64());
        Assert.AreEqual(-Max, IntT.MinValue.ToInt64());
        Assert.AreEqual(new string('+', 24), IntT.MaxValue.TernaryString);
        Assert.AreEqual(new string('-', 24), IntT.MinValue.TernaryString);
    }

    [TestMethod]
    public void AdditionWrapsBalancedModulo3Pow24()
    {
        // MAX + 1 == MIN, MIN - 1 == MAX: the balanced transposition of two's-complement wrap.
        Assert.AreEqual(IntT.MinValue, IntT.MaxValue + IntT.One);
        Assert.AreEqual(IntT.MaxValue, IntT.MinValue - IntT.One);
        Assert.AreEqual(IntT.MinValue + new IntT(41), IntT.MaxValue + new IntT(42));

        // Wrap is congruence mod 3^24.
        var random = new Random(2461);
        for (int i = 0; i < 200; i++)
        {
            long a = random.NextInt64(-Max, Max + 1);
            long b = random.NextInt64(-Max, Max + 1);
            Assert.AreEqual(WrapExpected((Int128)a + b), (new IntT(a) + new IntT(b)).ToInt64());
            Assert.AreEqual(WrapExpected((Int128)a - b), (new IntT(a) - new IntT(b)).ToInt64());
        }
    }

    [TestMethod]
    public void MultiplicationWrapsBalancedModulo3Pow24()
    {
        Assert.AreEqual(WrapExpected((Int128)Max * Max), (IntT.MaxValue * IntT.MaxValue).ToInt64());

        var random = new Random(2462);
        for (int i = 0; i < 200; i++)
        {
            long a = random.NextInt64(-Max, Max + 1);
            long b = random.NextInt64(-Max, Max + 1);
            Assert.AreEqual(WrapExpected((Int128)a * b), (new IntT(a) * new IntT(b)).ToInt64());
        }
    }

    [TestMethod]
    public void NegationIsTotal()
    {
        // No INT_MIN asymmetry: -MIN == MAX and -MAX == MIN.
        Assert.AreEqual(IntT.MaxValue, -IntT.MinValue);
        Assert.AreEqual(IntT.MinValue, -IntT.MaxValue);
        Assert.AreEqual(IntT.Zero, -IntT.Zero);
    }

    [TestMethod]
    public void DivisionByZeroYieldsZeroQuotientAndDividendRemainder()
    {
        IntT x = new IntT(123_456_789L);

        var (q, r) = IntT.DivRem(x, IntT.Zero);
        Assert.AreEqual(IntT.Zero, q);
        Assert.AreEqual(x, r);

        Assert.AreEqual(IntT.Zero, x / IntT.Zero);
        Assert.AreEqual(x, x % IntT.Zero);
        Assert.AreEqual(x, IntT.Mod(x, IntT.Zero));

        // The identity dividend == q * divisor + r holds with q = 0.
        Assert.AreEqual(x, (x / IntT.Zero) * IntT.Zero + (x % IntT.Zero));
    }

    [TestMethod]
    public void MinDividedByNegativeOneIsMax()
    {
        // Division has no overflow case in a symmetric range.
        Assert.AreEqual(IntT.MaxValue, IntT.MinValue / IntT.NegativeOne);
    }

    [TestMethod]
    public void RemSignFollowsDividendAndModSignFollowsDivisor()
    {
        // All four sign combinations of 7 and 3.
        var cases = new (long A, long B, long Rem, long Mod)[]
        {
            ( 7,  3,  1,  1),
            (-7,  3, -1,  2),
            ( 7, -3,  1, -2),
            (-7, -3, -1, -1),
        };

        foreach (var (a, b, rem, mod) in cases)
        {
            IntT dividend = new IntT(a);
            IntT divisor = new IntT(b);

            Assert.AreEqual(rem, (dividend % divisor).ToInt64(), $"REM {a} % {b}");
            Assert.AreEqual(mod, IntT.Mod(dividend, divisor).ToInt64(), $"MOD {a} mod {b}");

            // REM pairs the truncating DIV: a == q * b + r.
            var (q, r) = IntT.DivRem(dividend, divisor);
            Assert.AreEqual(a, q.ToInt64() * b + r.ToInt64(), $"DIVREM identity for {a}, {b}");
        }
    }

    [TestMethod]
    public void MinMaxAreDualUnderNegation()
    {
        var random = new Random(2463);
        for (int i = 0; i < 200; i++)
        {
            IntT a = new IntT(random.NextInt64(-Max, Max + 1));
            IntT b = new IntT(random.NextInt64(-Max, Max + 1));

            Assert.AreEqual(IntT.Min(a, b), -IntT.Max(-a, -b));
            Assert.AreEqual(IntT.Max(a, b), -IntT.Min(-a, -b));
            Assert.AreEqual(Math.Min(a.ToInt64(), b.ToInt64()), IntT.Min(a, b).ToInt64());
            Assert.AreEqual(Math.Max(a.ToInt64(), b.ToInt64()), IntT.Max(a, b).ToInt64());
        }
    }

    [TestMethod]
    public void MajvIsThePerTritMedianAndMinvItsNegation()
    {
        var random = new Random(2464);
        for (int i = 0; i < 100; i++)
        {
            IntT a = new IntT(random.NextInt64(-Max, Max + 1));
            IntT b = new IntT(random.NextInt64(-Max, Max + 1));
            IntT c = new IntT(random.NextInt64(-Max, Max + 1));

            IntT majority = IntT.MAJV(a, b, c);
            IntT minority = IntT.MINV(a, b, c);

            string sa = a.TernaryString, sb = b.TernaryString, sc = c.TernaryString;
            string sMaj = majority.TernaryString;
            for (int t = 0; t < IntT.TritCount; t++)
            {
                Trit median = Trit.Median(new Trit(sa[t]), new Trit(sb[t]), new Trit(sc[t]));
                Assert.AreEqual(median.GetChar, sMaj[t], $"trit {t} of MAJV({a}, {b}, {c})");
            }

            Assert.AreEqual(-majority, minority);
        }

        // MAJV of a value with MAX and MIN is the identity (median clamps to the value).
        IntT v = new IntT(-987_654_321L);
        Assert.AreEqual(v, IntT.MAJV(v, IntT.MaxValue, IntT.MinValue));
    }

    [TestMethod]
    public void RotateHasFullPeriod24()
    {
        IntT value = IntT.Parse("+-0++--0+0-+00++---0+0-+");

        Assert.AreEqual(value, value.Rotate(24));
        Assert.AreEqual(value, value.Rotate(0));
        Assert.AreEqual(value, value.Rotate(-24));
        Assert.AreEqual(value, value.Rotate(7).Rotate(17));
        Assert.AreEqual(value.Rotate(5), value.Rotate(5 - 24));

        // Positive counts rotate toward the most significant trit (cyclic, no fill).
        IntT one = IntT.One;
        Assert.AreEqual(3L, one.Rotate(1).ToInt64());
        Assert.AreEqual(IntT.Parse("+" + new string('0', 23)), one.Rotate(-1));
    }

    [TestMethod]
    public void ShiftsAreZeroFillAndTruncating()
    {
        IntT x = new IntT(5);                    // +--
        Assert.AreEqual(15L, (x << 1).ToInt64());    // SHIZ.T, amount +1
        Assert.AreEqual(2L, (x >> 1).ToInt64());     // SHIZ.T, amount -1: +-- becomes +-, the low trit is dropped
        Assert.AreEqual(IntT.Zero, x << 24);
        Assert.AreEqual(IntT.Zero, x >> 24);

        // A left shift drops trits past position 23 - wrap, not overflow.
        Assert.AreEqual(IntT.Parse("+" + new string('0', 23)), IntT.MaxValue << 23);
    }

    [TestMethod]
    public void SignedShiftDirectionFollowsTheAmountSign()
    {
        // SH<f>.T/SHI<f>.T: k > 0 shifts toward the MST (the old left shift),
        // k < 0 toward the LST (the old right shift), k = 0 is the identity.
        Trit zero = new Trit(0);
        var random = new Random(2465);
        for (int i = 0; i < 50; i++)
        {
            IntT v = new IntT(random.NextInt64(-Max, Max + 1));
            Assert.AreEqual(v, v.Shift(0, zero));
            for (int k = 1; k < IntT.TritCount; k++)
            {
                Assert.AreEqual(v << k, v.Shift(k, zero), $"Shift(+{k}) vs << {k}");
                Assert.AreEqual(v >> k, v.Shift(-k, zero), $"Shift(-{k}) vs >> {k}");
            }
        }
    }

    [TestMethod]
    public void SignedShiftFillLandsInTheVacatedTrits()
    {
        IntT v = IntT.Parse("+-0++--0+0-+00++---0+0-+");
        string s = v.TernaryString;   // MST first

        // Toward the MST: the top k trits drop, the fill enters at the LST end.
        Assert.AreEqual(s.Substring(2) + "++", v.Shift(2, new Trit(1)).TernaryString);
        Assert.AreEqual(s.Substring(3) + "---", v.Shift(3, new Trit(-1)).TernaryString);
        Assert.AreEqual(s.Substring(5) + "00000", v.Shift(5, new Trit(0)).TernaryString);

        // Toward the LST: the low k trits drop, the fill enters at the MST end.
        Assert.AreEqual("++" + s.Substring(0, 22), v.Shift(-2, new Trit(1)).TernaryString);
        Assert.AreEqual("---" + s.Substring(0, 21), v.Shift(-3, new Trit(-1)).TernaryString);
        Assert.AreEqual("00000" + s.Substring(0, 19), v.Shift(-5, new Trit(0)).TernaryString);
    }

    [TestMethod]
    public void SignedShiftSaturatesToAllFillAt24()
    {
        IntT v = new IntT(123_456_789L);
        foreach (int k in new[] { 24, -24, 25, -25, 40, -40 })
        {
            Assert.AreEqual(IntT.MinValue, v.Shift(k, new Trit(-1)), $"all-N at k={k}");
            Assert.AreEqual(IntT.Zero, v.Shift(k, new Trit(0)), $"all-Z at k={k}");
            Assert.AreEqual(IntT.MaxValue, v.Shift(k, new Trit(1)), $"all-P at k={k}");
        }
    }

    [TestMethod]
    public void ShiftFillWrappersMatchTheCore()
    {
        var random = new Random(2466);
        for (int i = 0; i < 50; i++)
        {
            IntT v = new IntT(random.NextInt64(-Max, Max + 1));
            int k = random.Next(-30, 31);
            Assert.AreEqual(v.Shift(k, new Trit(-1)), v.ShiftN(k));
            Assert.AreEqual(v.Shift(k, new Trit(0)), v.ShiftZ(k));
            Assert.AreEqual(v.Shift(k, new Trit(1)), v.ShiftP(k));
        }

        // A zero-fill signed shift by 3^k is exact multiplication/division by 3^k.
        IntT x = new IntT(42);
        Assert.AreEqual(42L * 27, x.ShiftZ(3).ToInt64());
        Assert.AreEqual(new IntT(42L * 27), new IntT(42L * 27).ShiftZ(-3).ShiftZ(3));
    }

    [TestMethod]
    public void TryteSignedShiftMatchesWordSemanticsAtWidth6()
    {
        Tryte t = new Tryte("+-0+-0");

        // Direction from the sign; zero fill matches the legacy wrappers.
        Assert.AreEqual((string)t.SHIFTLEFT(2), (string)t.Shift(2, new Trit(0)));
        Assert.AreEqual((string)t.SHIFTRIGHT(2), (string)t.Shift(-2, new Trit(0)));
        Assert.AreEqual((string)t, (string)t.Shift(0, new Trit(1)));

        // Fill lands in the vacated trits at both ends.
        Assert.AreEqual("-0+-0+", (string)t.Shift(1, new Trit(1)));
        Assert.AreEqual("-+-0+-", (string)t.Shift(-1, new Trit(-1)));

        // |k| >= 6 saturates to all-fill.
        Assert.AreEqual("------", (string)t.Shift(6, new Trit(-1)));
        Assert.AreEqual("++++++", (string)t.Shift(-6, new Trit(1)));
        Assert.AreEqual("000000", (string)t.Shift(40, new Trit(0)));
        Assert.AreEqual("++++++", (string)t.Shift(-9, new Trit(1)));
    }

    [TestMethod]
    public void CountLeadingZeroTritsMatchesSpec()
    {
        Assert.AreEqual(24, IntT.Zero.CountLeadingZeroTrits());
        Assert.AreEqual(23, IntT.One.CountLeadingZeroTrits());
        Assert.AreEqual(23, IntT.NegativeOne.CountLeadingZeroTrits());
        Assert.AreEqual(0, IntT.MaxValue.CountLeadingZeroTrits());
        Assert.AreEqual(22, new IntT(4).CountLeadingZeroTrits());   // ++
    }

    [TestMethod]
    public void CountNonZeroTritsMatchesSpec()
    {
        Assert.AreEqual(0, IntT.Zero.CountNonZeroTrits());
        Assert.AreEqual(1, IntT.One.CountNonZeroTrits());
        Assert.AreEqual(24, IntT.MaxValue.CountNonZeroTrits());
        Assert.AreEqual(24, IntT.MinValue.CountNonZeroTrits());
        Assert.AreEqual(2, IntT.Parse("+0000000-").CountNonZeroTrits());
    }

    [TestMethod]
    public void MacFusesMultiplyAndAccumulateWithWrap()
    {
        Assert.AreEqual(new IntT(7L * 6 + 5), IntT.Mac(new IntT(7), new IntT(6), new IntT(5)));

        // Wraps like MUL.T followed by ADD.T.
        Assert.AreEqual(IntT.MaxValue * IntT.MaxValue + IntT.One,
                        IntT.Mac(IntT.MaxValue, IntT.MaxValue, IntT.One));
    }

    [TestMethod]
    public void Mv2ReadsBothSourcesBeforeWriting()
    {
        IntT a = new IntT(111);
        IntT b = new IntT(-222);

        (a, b) = IntT.Mv2(b, a);   // SWAP.T pseudo: both reads happen before either write
        Assert.AreEqual(-222L, a.ToInt64());
        Assert.AreEqual(111L, b.ToInt64());

        IntT.Swap(ref a, ref b);
        Assert.AreEqual(111L, a.ToInt64());
        Assert.AreEqual(-222L, b.ToInt64());
    }

    [TestMethod]
    public void ParseAndToStringRoundTrip()
    {
        IntT value = IntT.Parse("+-0+-0+-0");
        Assert.AreEqual(value, IntT.Parse(value.TernaryString.TrimStart('0')));
        Assert.AreEqual(4542L, value.ToInt64());
        Assert.AreEqual(new IntT(4542), value);
    }

    [TestMethod]
    public void CompareToIsAThreeWayResult()
    {
        Assert.IsLessThan(0, IntT.MinValue.CompareTo(IntT.MaxValue));
        Assert.IsGreaterThan(0, IntT.MaxValue.CompareTo(IntT.MinValue));
        Assert.AreEqual(0, new IntT(42).CompareTo(new IntT(42)));
        Assert.IsTrue(new IntT(-1) < IntT.Zero);
    }
}

[TestClass]
public class Rebel6MulhTests
{
    private const long Modulus = 282_429_536_481L;          // 3^24
    private const long Max = 141_214_768_240L;              // (3^24 - 1) / 2

    /// <summary>MULH.T:MUL.T must compose to the exact 48-trit product.</summary>
    private static void AssertComposition(long a, long b)
    {
        IntT x = new IntT(a), y = new IntT(b);
        long low = (x * y).ToInt64();          // MUL.T: balanced wrap mod 3^24
        long high = IntT.Mulh(x, y).ToInt64(); // MULH.T: (exact - low) / 3^24

        // Low word is the balanced residue, so |low| <= Max always.
        Assert.IsLessThanOrEqualTo(Max, Math.Abs(low), $"low out of balanced range for {a} * {b}");

        // BigInteger reference, independent of the library's Int128 arithmetic.
        System.Numerics.BigInteger exact = (System.Numerics.BigInteger)a * b;
        System.Numerics.BigInteger recomposed =
            (System.Numerics.BigInteger)high * Modulus + low;
        Assert.AreEqual(exact, recomposed, $"MULH:MUL composition broken for {a} * {b}");

        // Cross-check with Int128 as well.
        Assert.AreEqual((Int128)a * b, (Int128)high * Modulus + low);
    }

    [TestMethod]
    public void MulhComposesTheExactProductOnBoundaries()
    {
        long[] edges = { 0, 1, -1, 2, -2, Max, -Max, Max - 1, -(Max - 1), 3, 27, -27 };
        foreach (long a in edges)
            foreach (long b in edges)
                AssertComposition(a, b);
    }

    [TestMethod]
    public void MulhComposesTheExactProductOnRandomOperands()
    {
        var random = new Random(2467);
        for (int i = 0; i < 500; i++)
        {
            long a = random.NextInt64(-Max, Max + 1);
            long b = random.NextInt64(-Max, Max + 1);
            AssertComposition(a, b);
        }
    }

    [TestMethod]
    public void MulhOfSmallOperandsIsZeroAndOfMaxIsLarge()
    {
        // A product inside the single-word range has high word 0.
        Assert.AreEqual(IntT.Zero, IntT.Mulh(new IntT(1000), new IntT(1000)));
        Assert.AreEqual(IntT.Zero, IntT.Mulh(IntT.MaxValue, IntT.One));
        // Max * Max: high = (Max^2 - wrap(Max^2)) / 3^24.
        Int128 exact = (Int128)Max * Max;
        long low = (IntT.MaxValue * IntT.MaxValue).ToInt64();
        Assert.AreEqual((long)((exact - low) / Modulus), IntT.Mulh(IntT.MaxValue, IntT.MaxValue).ToInt64());
    }
}

[TestClass]
public class Rebel6PackedTests
{
    private const long Max = 141_214_768_240L;

    private static int ReferenceLaneSum(IntT a, IntT b)
    {
        // Scalar reference: decode both words to trits via the string form.
        string sa = a.TernaryString, sb = b.TernaryString;
        int sum = 0;
        for (int i = 0; i < IntT.TritCount; i++)
        {
            int ta = sa[i] == '+' ? 1 : sa[i] == '-' ? -1 : 0;
            int tb = sb[i] == '+' ? 1 : sb[i] == '-' ? -1 : 0;
            sum += ta * tb;
        }
        return sum;
    }

    private static IntT RandomWord(Random random) => new IntT(random.NextInt64(-Max, Max + 1));

    [TestMethod]
    public void TmacMatchesTheScalarReferenceLoop()
    {
        var random = new Random(2468);
        for (int i = 0; i < 300; i++)
        {
            IntT a = RandomWord(random), b = RandomWord(random), acc = RandomWord(random);
            long expected = (acc + new IntT(ReferenceLaneSum(a, b))).ToInt64();
            Assert.AreEqual(expected, IntT.Tmac(a, b, acc).ToInt64());
        }

        // All-ones lanes: TMAC of MAX (24 '+' trits) with itself sums to +24.
        Assert.AreEqual(24L, IntT.Tmac(IntT.MaxValue, IntT.MaxValue, IntT.Zero).ToInt64());
        Assert.AreEqual(-24L, IntT.Tmac(IntT.MaxValue, IntT.MinValue, IntT.Zero).ToInt64());

        // The accumulate wraps balanced mod 3^24 like ADD.T.
        Assert.AreEqual(IntT.MinValue + new IntT(23),
                        IntT.Tmac(IntT.MaxValue, IntT.MaxValue, IntT.MaxValue));

        // TDOT.T pseudo == TMAC.T with a zero accumulator.
        IntT x = RandomWord(random), y = RandomWord(random);
        Assert.AreEqual(IntT.Tmac(x, y, IntT.Zero), IntT.Tdot(x, y));
        Assert.AreEqual(ReferenceLaneSum(x, y), (int)IntT.Tdot(x, y).ToInt64());
    }

    [TestMethod]
    public void TsumIsTheHorizontalTritSum()
    {
        Assert.AreEqual(0, IntT.Zero.TritSum());
        Assert.AreEqual(24, IntT.MaxValue.TritSum());
        Assert.AreEqual(-24, IntT.MinValue.TritSum());
        Assert.AreEqual(1, IntT.One.TritSum());
        Assert.AreEqual(0, IntT.Parse("+-").TritSum());

        var random = new Random(2469);
        for (int i = 0; i < 300; i++)
        {
            IntT v = RandomWord(random);
            int expected = v.TernaryString.Sum(c => c == '+' ? 1 : c == '-' ? -1 : 0);
            Assert.AreEqual(expected, v.TritSum());
            // TSUM(a) == TDOT(a, all-ones word).
            Assert.AreEqual(expected, (int)IntT.Tdot(v, IntT.MaxValue).ToInt64());
        }
    }

    [TestMethod]
    public void QntTernarizesAgainstTheThreshold()
    {
        IntT t = new IntT(100);
        Assert.AreEqual(IntT.One, IntT.Qnt(new IntT(101), t));
        Assert.AreEqual(IntT.Zero, IntT.Qnt(new IntT(100), t));
        Assert.AreEqual(IntT.Zero, IntT.Qnt(IntT.Zero, t));
        Assert.AreEqual(IntT.Zero, IntT.Qnt(new IntT(-100), t));
        Assert.AreEqual(IntT.NegativeOne, IntT.Qnt(new IntT(-101), t));

        // Zero threshold: QNT.T is the sign function.
        var random = new Random(2470);
        for (int i = 0; i < 200; i++)
        {
            IntT v = RandomWord(random);
            Assert.AreEqual(v.Sign, (int)IntT.Qnt(v, IntT.Zero).ToInt64());
        }

        // Randomized threshold semantics.
        for (int i = 0; i < 200; i++)
        {
            long x = random.NextInt64(-Max, Max + 1);
            long th = random.NextInt64(0, Max + 1);
            long expected = x > th ? 1 : x < -th ? -1 : 0;
            Assert.AreEqual(expected, IntT.Qnt(new IntT(x), new IntT(th)).ToInt64(), $"QNT({x}, {th})");
        }
    }

    [TestMethod]
    public void HmaxPicksTheLargestTryteLane()
    {
        var random = new Random(2471);
        for (int i = 0; i < 300; i++)
        {
            // Compose a word from four known 6-trit lane values in +/-364.
            long[] lanes = new long[4];
            IntT word = IntT.Zero;
            for (int lane = 0; lane < 4; lane++)
            {
                lanes[lane] = random.NextInt64(-364, 365);
                word += new IntT(lanes[lane]) << (lane * 6);
            }
            Assert.AreEqual(lanes.Max(), word.Hmax().ToInt64(),
                $"HMAX of lanes [{string.Join(", ", lanes)}]");
        }

        Assert.AreEqual(0L, IntT.Zero.Hmax().ToInt64());
        Assert.AreEqual(364L, IntT.MaxValue.Hmax().ToInt64());
        Assert.AreEqual(-364L, IntT.MinValue.Hmax().ToInt64());
    }

    [TestMethod]
    public void SltIsABinaryPredicateNotAThreeWayCompare()
    {
        var random = new Random(2472);
        for (int i = 0; i < 200; i++)
        {
            IntT a = RandomWord(random), b = RandomWord(random);
            long expected = a.CompareTo(b) < 0 ? 1 : 0;
            Assert.AreEqual(expected, IntT.Slt(a, b).ToInt64());
        }
        Assert.AreEqual(IntT.One, IntT.Slt(IntT.MinValue, IntT.MaxValue));
        Assert.AreEqual(IntT.Zero, IntT.Slt(IntT.MaxValue, IntT.MinValue));  // CMP.T would say +1... SLT says 0
        Assert.AreEqual(IntT.Zero, IntT.Slt(new IntT(42), new IntT(42)));
    }
}

[TestClass]
public class Rebel6ZtlTests
{
    private static readonly sbyte[] TritValues = { -1, 0, 1 };

    /// <summary>A 24-lane input word whose lane i carries inputs[i % inputs.Length].</summary>
    private static IntT LanePattern(params sbyte[] inputs)
    {
        IntT word = IntT.Zero;
        for (int i = IntT.TritCount - 1; i >= 0; i--)
            word = (word << 1) + new IntT(inputs[i % inputs.Length]);
        return word;
    }

    private static sbyte LaneAt(IntT word, int index)
    {
        char c = word.TernaryString[IntT.TritCount - 1 - index];
        return (sbyte)(c == '+' ? 1 : c == '-' ? -1 : 0);
    }

    [TestMethod]
    public void TlutiCoversAll27UnaryGatesExhaustively()
    {
        // The full 1-input gate space: 3^3 = 27 truth tables x 3 inputs each.
        IntT input = LanePattern(-1, 0, 1);   // every lane triple cycles -,0,+
        foreach (sbyte fN in TritValues)
        foreach (sbyte fZ in TritValues)
        foreach (sbyte fP in TritValues)
        {
            IntT table = IntT.UnaryGateTable(fN, fZ, fP);
            IntT result = IntT.Tluti(input, table);
            for (int i = 0; i < IntT.TritCount; i++)
            {
                sbyte a = LaneAt(input, i);
                sbyte expected = a < 0 ? fN : a == 0 ? fZ : fP;
                Assert.AreEqual(expected, LaneAt(result, i), $"table ({fN},{fZ},{fP}), input {a}");
            }
        }
    }

    [TestMethod]
    public void TlutCoversAll19683BinaryGatesExhaustively()
    {
        // Lanes 0..8 enumerate all 9 input pairs; the full 2-input gate space
        // is every 9-trit table: 3^9 = 19,683 gates x 9 input pairs each.
        sbyte[] aLanes = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
        sbyte[] bLanes = { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
        IntT a = LanePattern(aLanes);
        IntT b = LanePattern(bLanes);

        var entries = new sbyte[9];
        for (int gate = 0; gate < 19683; gate++)
        {
            int g = gate;
            for (int k = 0; k < 9; k++)
            {
                entries[k] = (sbyte)(g % 3 - 1);
                g /= 3;
            }
            IntT table = IntT.BinaryGateTable(entries);
            IntT result = IntT.Tlut(a, b, table);
            for (int i = 0; i < 9; i++)
            {
                int index = (aLanes[i] * 3 + bLanes[i]) + 4;
                Assert.AreEqual(entries[index], LaneAt(result, i),
                    $"gate {gate}, pair ({aLanes[i]},{bLanes[i]})");
            }
        }
    }

    [TestMethod]
    public void CanonicalUnaryGatesMatchTheirNormativeTables()
    {
        IntT input = LanePattern(-1, 0, 1);
        for (int i = 0; i < IntT.TritCount; i++)
        {
            int x = LaneAt(input, i);
            Assert.AreEqual(x < 0 ? 1 : -1, (int)LaneAt(input.Nti(), i), "NTI");        // (+, -, -)
            Assert.AreEqual(x > 0 ? -1 : 1, (int)LaneAt(input.Pti(), i), "PTI");        // (+, +, -)
            Assert.AreEqual(Math.Abs(x), (int)LaneAt(input.Mti(), i), "MTI");           // (+, 0, +)
            Assert.AreEqual(x == 1 ? -1 : x + 1, (int)LaneAt(input.Cyu(), i), "CYU");   // - -> 0 -> + -> -
            Assert.AreEqual(x == -1 ? 1 : x - 1, (int)LaneAt(input.Cyd(), i), "CYD");   // - -> + -> 0 -> -
        }

        // Cycle laws: CYU then CYD is the identity; CYU^3 == identity.
        IntT v = new IntT(-987_654_321L);
        Assert.AreEqual(v, v.Cyu().Cyd());
        Assert.AreEqual(v, v.Cyu().Cyu().Cyu());
        // MTI == STI of NTI's complement structure aside: |x| per trit is idempotent.
        Assert.AreEqual(v.Mti(), v.Mti().Mti());
    }

    [TestMethod]
    public void CanonicalBinaryGatesMatchTheirNormativeTables()
    {
        sbyte[] aLanes = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
        sbyte[] bLanes = { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
        IntT a = LanePattern(aLanes);
        IntT b = LanePattern(bLanes);

        IntT kimp = IntT.Kimp(a, b);
        IntT cmpt = IntT.Cmpt(a, b);
        IntT cons = IntT.Cons(a, b);

        for (int i = 0; i < IntT.TritCount; i++)
        {
            int x = LaneAt(a, i), y = LaneAt(b, i);
            Assert.AreEqual(Math.Max(-x, y), (int)LaneAt(kimp, i), $"KIMP({x},{y})");
            Assert.AreEqual(Math.Sign(x - y), (int)LaneAt(cmpt, i), $"CMPT({x},{y})");
            Assert.AreEqual(x == y ? x : 0, (int)LaneAt(cons, i), $"CONS({x},{y})");
        }
    }

    [TestMethod]
    public void TlutReproducesTheBaseTritwiseOps()
    {
        // AND.T, OR.T and XOR.T are single TLUT.T tables - build them and
        // check against the operators on random words.
        IntT andTable = IntT.BinaryGateTable(-1, -1, -1, -1, 0, 0, -1, 0, 1);  // min
        IntT orTable = IntT.BinaryGateTable(-1, 0, 1, 0, 0, 1, 1, 1, 1);       // max
        IntT xorTable = IntT.BinaryGateTable(-1, 0, 1, 0, 0, 0, 1, 0, -1);     // -(a*b)

        var random = new Random(2473);
        for (int i = 0; i < 200; i++)
        {
            IntT x = new IntT(random.NextInt64(-141_214_768_240L, 141_214_768_241L));
            IntT y = new IntT(random.NextInt64(-141_214_768_240L, 141_214_768_241L));
            Assert.AreEqual(x & y, IntT.Tlut(x, y, andTable));
            Assert.AreEqual(x | y, IntT.Tlut(x, y, orTable));
            Assert.AreEqual(x ^ y, IntT.Tlut(x, y, xorTable));
        }
    }

    [TestMethod]
    public void TlutIgnoresTheTableWordsUpperTrits()
    {
        IntT a = LanePattern(-1, 0, 1);
        IntT b = LanePattern(1, -1, 0);
        IntT table = IntT.BinaryGateTable(0, -1, -1, 1, 0, -1, 1, 1, 0);
        // Junk in trits 9..23 of the table word must not change the result.
        IntT junk = new IntT(123_456_789L) << 9;
        Assert.AreEqual(IntT.Tlut(a, b, table), IntT.Tlut(a, b, table + junk));
        // Same for TLUTI above trit 2.
        IntT utable = IntT.UnaryGateTable(1, -1, 0);
        IntT ujunk = new IntT(-42) << 3;
        Assert.AreEqual(IntT.Tluti(a, utable), IntT.Tluti(a, utable + ujunk));
    }

    [TestMethod]
    public void TryteTlutMatchesWordSemanticsPerLane()
    {
        Tryte a = new Tryte("+-0+-0");
        Tryte b = new Tryte("++--00");
        IntT cmptTable = IntT.BinaryGateTable(0, -1, -1, 1, 0, -1, 1, 1, 0);   // sign(a - b)

        // Per lane (MST first): (+,+) (-,+) (0,-) (+,-) (-,0) (0,0) -> 0 - + + - 0
        Assert.AreEqual("0-++-0", Tryte.TLUT(a, b, cmptTable).TryteString);

        IntT ntiTable = IntT.UnaryGateTable(1, -1, -1);
        Assert.AreEqual("-+--+-", Tryte.TLUTI(a, ntiTable).TryteString);
    }
}

[TestClass]
public class TritMedianTests
{
    [TestMethod]
    public void MedianIsExhaustivelyCorrect()
    {
        for (sbyte a = -1; a <= 1; a++)
        {
            for (sbyte b = -1; b <= 1; b++)
            {
                for (sbyte c = -1; c <= 1; c++)
                {
                    var sorted = new[] { a, b, c }.OrderBy(v => v).ToArray();
                    Trit median = Trit.Median(new Trit(a), new Trit(b), new Trit(c));
                    Assert.AreEqual(sorted[1], (sbyte)median, $"median({a},{b},{c})");
                    Assert.AreEqual((sbyte)(-sorted[1]), (sbyte)Trit.MINV(new Trit(a), new Trit(b), new Trit(c)));
                }
            }
        }
    }

    [TestMethod]
    public void TryteMajvAndMinvActPerTrit()
    {
        Tryte a = new Tryte("+-0+-0");
        Tryte b = new Tryte("++--00");
        Tryte c = new Tryte("+0-0+-");

        Assert.AreEqual("+0-000", (string)Tryte.MAJV(a, b, c));
        Assert.AreEqual("-0+000", (string)Tryte.MINV(a, b, c));
    }
}

[TestClass]
public class Rebel6IntrinsicMapTests
{
    private const string InstructionSetRelativePath =
        "ternary-workbench/TernaryWorkbench/src/TernaryWorkbench.RebelAssembler/Assembly/InstructionSet6.cs";

    /// <summary>
    /// Locates the REBEL-6 assembler's pattern table by probing the
    /// TERNARY_WORKBENCH_ROOT environment variable, then walking up from the test
    /// directory looking for a sibling ternary-workbench checkout.
    /// </summary>
    private static string? FindInstructionSetFile()
    {
        string? envRoot = Environment.GetEnvironmentVariable("TERNARY_WORKBENCH_ROOT");
        if (!string.IsNullOrEmpty(envRoot))
        {
            string candidate = Path.Combine(envRoot, "TernaryWorkbench", "src",
                "TernaryWorkbench.RebelAssembler", "Assembly", "InstructionSet6.cs");
            if (File.Exists(candidate))
                return candidate;
        }

        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            string candidate = Path.Combine(dir.FullName, InstructionSetRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private static IEnumerable<(string Mnemonic, string Method)> CollectAnnotatedMnemonics()
    {
        const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        foreach (Type type in typeof(IntT).Assembly.GetTypes())
        {
            foreach (MethodInfo method in type.GetMethods(All))
            {
                // AllowMultiple: one method may cover several encodings of the
                // same semantics (e.g. Rotate -> ROT.T and ROTR.T).
                foreach (var attribute in method.GetCustomAttributes<Rebel6IntrinsicAttribute>())
                    yield return (attribute.Mnemonic, $"{type.Name}.{method.Name}");
            }
        }
    }

    [TestMethod]
    public void EveryIntrinsicMnemonicExistsInTheAssemblerPatternTable()
    {
        string? path = FindInstructionSetFile();
        if (path is null)
        {
            Assert.Inconclusive(
                "InstructionSet6.cs not found - checkout ternary-workbench next to stdTernary " +
                "or set TERNARY_WORKBENCH_ROOT to run the intrinsic-map consistency check.");
            return;
        }

        string source = File.ReadAllText(path);
        var patterns = Regex.Matches(source, "new InstructionPattern\\(\"([A-Z0-9.]+)\"")
                            .Select(m => m.Groups[1].Value)
                            .ToList();

        // The REBEL-6 assembler defines exactly 122 architectural instruction
        // patterns (127 before the signed-shift redesign collapsed the 12
        // SL*/SR*/SLI*/SRI* forms into SHN/SHZ/SHP.T + SHIN/SHIZ/SHIP.T and
        // added ROTR.T in the freed R-ALU func slot).
        Assert.HasCount(122, patterns, "Unexpected pattern count in InstructionSet6.cs.");

        var table = patterns.ToHashSet(StringComparer.Ordinal);
        var annotated = CollectAnnotatedMnemonics().ToList();
        Assert.IsNotEmpty(annotated, "No [Rebel6Intrinsic] annotations found in stdTernary.");

        var missing = annotated.Where(entry => !table.Contains(entry.Mnemonic)).ToList();
        Assert.IsEmpty(missing,
            "Mnemonics not present in the assembler pattern table: " +
            string.Join(", ", missing.Select(entry => $"{entry.Mnemonic} ({entry.Method})")));
    }

    [TestMethod]
    public void CoreWordOperationsAreAnnotated()
    {
        var mnemonics = CollectAnnotatedMnemonics().Select(entry => entry.Mnemonic).ToHashSet(StringComparer.Ordinal);
        string[] expected =
        {
            "ADD.T", "SUB.T", "STI.T", "CMP.T", "MIN.T", "MAX.T", "MAJV.T", "MINV.T",
            "ROT.T", "ROTR.T", "SHN.T", "SHZ.T", "SHP.T", "SHIN.T", "SHIZ.T", "SHIP.T",
            "CLZT.T", "TCNT.T",
            "MUL.T", "MULH.T", "DIV.T", "REM.T", "MOD.T", "MAC.T", "MV2.T",
            "AND.T", "ANDI.T", "OR.T", "ORI.T", "XOR.T", "XORI.T", "SLT.T", "SLTI.T",
            "TMAC.T", "TDOT.T", "TSUM.T", "QNT.T", "HMAX.T",
            "TLUT.T", "TLUTI.T", "NTI.T", "PTI.T", "MTI.T", "CYU.T", "CYD.T",
        };

        foreach (string mnemonic in expected)
        {
            Assert.Contains(mnemonic, mnemonics, $"Expected a [Rebel6Intrinsic(\"{mnemonic}\")] annotation.");
        }
    }
}
