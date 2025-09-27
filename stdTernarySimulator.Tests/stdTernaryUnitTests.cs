namespace stdTernary.Tests;

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stdTernary;

[TestClass]
public class TritTests
{
    [TestMethod]
    public void NegationFlipsSign()
    {
        Trit positive = new Trit(1);
        Trit negative = positive.NEG();
        Assert.AreEqual(TritVal.n, negative.Value);
        Assert.AreEqual(TritVal.p, negative.NEG().Value);
    }

    [TestMethod]
    public void XorMatchesBalancedTruthTable()
    {
        Trit a = new Trit(1);
        Trit b = new Trit(-1);
        Assert.AreEqual(TritVal.p, (a ^ b).Value);

        a = new Trit(1);
        b = new Trit(1);
        Assert.AreEqual(TritVal.n, (a ^ b).Value);

        a = new Trit(1);
        b = new Trit(0);
        Assert.AreEqual(TritVal.z, (a ^ b).Value);
    }
}

[TestClass]
public class TryteTests
{
    [TestMethod]
    public void AdditionProducesExpectedValue()
    {
        Tryte a = new Tryte(82);
        Tryte b = new Tryte(-41);
        Tryte sum = a + b;
        Assert.AreEqual(41, sum.ShortValue);
    }

    [TestMethod]
    public void MultiplicationUsesBalancedDigits()
    {
        Tryte a = new Tryte(12);
        Tryte b = new Tryte(-7);
        Tryte product = a * b;
        Assert.AreEqual(-84, product.ShortValue);
    }

    [TestMethod]
    public void DivisionAndModMatchBalancedArithmetic()
    {
        Tryte dividend = new Tryte(140);
        Tryte divisor = new Tryte(9);
        var (quotient, remainder) = dividend.DIVREM(divisor);

        Assert.AreEqual(140 / 9, quotient.ShortValue);
        Assert.AreEqual(140 % 9, remainder.ShortValue);
    }

    [TestMethod]
    public void BitwiseOperatorsActPerTrit()
    {
        Tryte a = new Tryte("+-0+-0");
        Tryte b = new Tryte("++0-+-");

        Tryte and = a & b;
        Tryte or = a | b;
        Tryte xor = a ^ b;

        Assert.AreEqual("+-0---", (string)and);
        Assert.AreEqual("++0++0", (string)or);
        Assert.AreEqual("-+0++0", (string)xor);
    }
}

[TestClass]
public class IntTTests
{
    [TestMethod]
    public void AdditionProducesExpectedBalancedResults()
    {
        IntT a = new IntT(245);
        IntT b = new IntT(-123);
        IntT c = a + b;
        Assert.AreEqual(122L, c.ToInt64(), "245 + (-123) should equal 122 in balanced ternary.");

        a = new IntT(-150);
        b = new IntT(-150);
        c = a + b;
        Assert.AreEqual(-300L, c.ToInt64(), "-150 + -150 should equal -300.");
    }

    [TestMethod]
    public void SubtractionHandlesBorrowAcrossTrits()
    {
        IntT a = new IntT(512);
        IntT b = new IntT(123);
        IntT c = a - b;
        Assert.AreEqual(389L, c.ToInt64());

        a = new IntT(-42);
        b = new IntT(58);
        c = a - b;
        Assert.AreEqual(-100L, c.ToInt64());
    }

    [TestMethod]
    public void MultiplicationUsesBalancedTernaryCarry()
    {
        IntT a = new IntT(81);
        IntT b = new IntT(-45);
        IntT c = a * b;
        Assert.AreEqual(-3645L, c.ToInt64());

        a = new IntT(3_456);
        b = new IntT(789);
        c = a * b;
        Assert.AreEqual(3_456L * 789L, c.ToInt64());
    }

    [TestMethod]
    public void DivisionAndModulusMatchIntegerArithmetic()
    {
        IntT a = new IntT(5_432);
        IntT b = new IntT(123);

        IntT quotient = a / b;
        IntT remainder = a % b;

        Assert.AreEqual(5_432 / 123, quotient.ToInt64());
        Assert.AreEqual(5_432 % 123, remainder.ToInt64());

        a = new IntT(-9999);
        b = new IntT(321);
        quotient = a / b;
        remainder = a % b;

        Assert.AreEqual(-9999 / 321, quotient.ToInt64());
        Assert.AreEqual(-9999 % 321, remainder.ToInt64());
    }

    [TestMethod]
    public void ShiftOperatorsScaleByPowersOfThree()
    {
        IntT value = new IntT(10);
        IntT left = value << 2;  // multiply by 3^2 = 9
        Assert.AreEqual(10L * 9L, left.ToInt64());

        IntT right = left >> 2;
        Assert.AreEqual(value.ToInt64(), right.ToInt64());

        IntT truncated = new IntT(5) >> 3;
        Assert.AreEqual(0L, truncated.ToInt64());
    }

    [TestMethod]
    public void BitwiseOperationsFollowBalancedRules()
    {
        IntT a = IntT.Parse("++0-0+");
        IntT b = IntT.Parse("+-0+-+");

        IntT and = a & b;
        IntT or = a | b;
        IntT xor = a ^ b;

        Assert.IsTrue(and.TernaryString.EndsWith("+-0--+"));
        Assert.IsTrue(or.TernaryString.EndsWith("++0+0+"));
        Assert.IsTrue(xor.TernaryString.EndsWith("-+0+0-"));
    }

    [TestMethod]
    public void ComparisonsUseTernaryOrdering()
    {
        IntT a = new IntT(5000);
        IntT b = new IntT(-2000);
        IntT c = new IntT(5000);

        Assert.IsTrue(a > b);
        Assert.IsTrue(b < a);
        Assert.IsTrue(a >= c);
        Assert.IsTrue(a == c);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void ParsingRoundTripsBalancedStrings()
    {
        string ternary = "+-0+-0+-0+--00++--";
        IntT parsed = IntT.Parse(ternary);
        Assert.IsTrue(parsed.TernaryString.EndsWith(ternary));

        long backToLong = parsed.ToInt64();
        IntT reconstructed = new IntT(backToLong);
        Assert.AreEqual(parsed, reconstructed);
    }
}

[TestClass]
public class FloatTTests
{
    [TestMethod]
    public void AdditionOnIntegerValuesMatchesIntT()
    {
        FloatT a = FloatT.FromInt(new IntT(243));
        FloatT b = FloatT.FromInt(new IntT(-162));

        FloatT sum = a + b;

        Assert.AreEqual(81L, sum.ToIntT().ToInt64());
    }

    [TestMethod]
    public void MultiplicationUsesBalancedTernaryDigits()
    {
        FloatT a = FloatT.FromInt(new IntT(27));
        FloatT b = FloatT.FromInt(new IntT(-12));

        FloatT product = a * b;

        Assert.AreEqual(-324L, product.ToIntT().ToInt64());
    }

    [TestMethod]
    public void DivisionProducesNormalizedResult()
    {
        FloatT numerator = FloatT.FromInt(new IntT(729));
        FloatT denominator = FloatT.FromInt(new IntT(27));

        FloatT quotient = numerator / denominator;

        Assert.AreEqual(27L, quotient.ToIntT().ToInt64());
    }

    [TestMethod]
    public void DivisionHandlesFractionalOutputs()
    {
        FloatT one = FloatT.FromInt(new IntT(1));
        FloatT three = FloatT.FromInt(new IntT(3));

        FloatT fraction = one / three;

        double approx = fraction.ToDouble();
        Assert.IsTrue(Math.Abs(approx - (1.0 / 3.0)) < 1e-6);
    }
}
