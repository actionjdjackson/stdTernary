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
        Assert.AreEqual(15L, (x << 1).ToInt64());    // SLIZ.T
        Assert.AreEqual(2L, (x >> 1).ToInt64());     // SRIZ.T: +-- becomes +-, the low trit is dropped
        Assert.AreEqual(IntT.Zero, x << 24);
        Assert.AreEqual(IntT.Zero, x >> 24);

        // A left shift drops trits past position 23 - wrap, not overflow.
        Assert.AreEqual(IntT.Parse("+" + new string('0', 23)), IntT.MaxValue << 23);
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
                var attribute = method.GetCustomAttribute<Rebel6IntrinsicAttribute>();
                if (attribute is not null)
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

        // The REBEL-6 assembler defines exactly 127 instruction patterns.
        Assert.HasCount(127, patterns, "Unexpected pattern count in InstructionSet6.cs.");

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
            "ROT.T", "SLIZ.T", "SRIZ.T", "CLZT.T", "TCNT.T",
            "MUL.T", "DIV.T", "REM.T", "MOD.T", "MAC.T", "MV2.T",
        };

        foreach (string mnemonic in expected)
        {
            Assert.Contains(mnemonic, mnemonics, $"Expected a [Rebel6Intrinsic(\"{mnemonic}\")] annotation.");
        }
    }
}
