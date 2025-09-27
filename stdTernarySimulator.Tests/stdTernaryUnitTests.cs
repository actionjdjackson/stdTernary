namespace stdTernary.Tests;

using System;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stdTernary;

    [TestClass]
    public class UnitTestTrits
    {
        [TestMethod]
        public void TestTritComparisons()
        {
            Trit a = 1;
            Trit b = 1;
            Assert.IsTrue(a == b, "Expected a = 1 and b = 1 to be equal");
            a = -1;
            Assert.IsFalse(a == b, "Expected a = -1 and b = 1 to not be equal");
            Assert.IsTrue(a != b, "Expected a = -1 and b = 1 to not be equal");
            b = -1;
            Assert.IsTrue(a == b, "Expected a = -1 and b = -1 to be equal");
            a = 0;
            b = 0;
            Assert.IsTrue(a == b, "Expected a = 0 and b = 0 to be equal");
            a = 1;
            b = -1;
            Assert.IsTrue(a > b, "Expected 1 > -1, but returned false");
            Assert.IsTrue(b < a, "Expected -1 < 1, but returned false");
            var result = Trit.COMPARET(a, b).Value;
            Assert.IsTrue(result == TritVal.p, "Expected 1 <=> -1 to be p, but returned {0} <=> {1} = {2}", a, b, result);
            result = Trit.COMPARET(b, a).Value;
            Assert.IsTrue(result == TritVal.n, "Expected -1 <=> 1 to be n, but returned {0} <=> {1} = {2}", a, b, result);
            a = 0;
            b = 0;
            result = Trit.COMPARET(a, b).Value;
            Assert.IsTrue(result == TritVal.z, "Expected 0 <=> 0 to be z, but returned {0} <=> {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestTritAND()
        {
            Trit a = 1;
            Trit b = 1;
            Assert.IsTrue((a & b).Value == TritVal.p, "Expected 1 & 1 to be 1");
            a = 0;
            Assert.IsTrue((a & b).Value == TritVal.z, "Expected 0 & 1 to be 0");
            a = -1;
            Assert.IsTrue((a & b).Value == TritVal.n, "Expected -1 & 1 to be -1");
        }

        [TestMethod]
        public void TestTritOR()
        {
            Trit a = 1;
            Trit b = 0;
            Assert.IsTrue((a | b).Value == TritVal.p, "Expected 1 | 0 to be 1");
            a = -1;
            Assert.IsTrue((a | b).Value == TritVal.z, "Expected -1 | 0 to be 0");
            b = -1;
            Assert.IsTrue((a | b).Value == TritVal.n, "Expected -1 | -1 to be -1");
        }

        [TestMethod]
        public void TestTritXOR()
        {
            Trit a = 1;
            Trit b = -1;
            Assert.IsTrue((a ^ b).Value == TritVal.p, "Expected 1 ^ -1 to be 1");
            b = 1;
            Assert.IsTrue((a ^ b).Value == TritVal.n, "Expected 1 ^ 1 to be -1");
            b = 0;
            Assert.IsTrue((a ^ b).Value == TritVal.z, "Expected 1 ^ 0 to be 0");
            a = -1;
            b = -1;
            Assert.IsTrue((a ^ b).Value == TritVal.n, "Expected -1 ^ -1 to br -1");
        }

        [TestMethod]
        public void TestXNOR()
        {
            Trit a = 1;
            Trit b = 0;
            var result = a * b;
            Assert.IsTrue(result.Value == TritVal.z, "Expected a = 1 * b = 0 to be 0 but returned {0}", result);
            b = -1;
            result = a * b;
            Assert.IsTrue(result.Value == TritVal.n, "Expected a = 1 * b = -1 to be -1 but returned {0}", result);
            a = -1;
            result = a * b;
            Assert.IsTrue(result.Value == TritVal.p, "Expected a = -1 * b = -1 to be 1 but returned {0}", result);
        }

        [TestMethod]
        public void TestNegation()
        {
            Trit a = 1;
            var result = ~a;
            Assert.IsTrue(result.Value == TritVal.n, "Expected ~a = 1 to be -1, but returned {0}", result);
            a = -1;
            result = ~a;
            Assert.IsTrue(result.Value == TritVal.p, "Expected ~a = -1 to be 1, but returned {0}", result);
        }
    }

    [TestClass]
    public class UnitTestTrytes
    {
        [TestMethod]
        public void TestMinMax()
        {
            Tryte a = Tryte.MaxValue;
            Tryte b = Tryte.MinValue;
            var result = a + b;
            Assert.IsTrue(result == 0, "Expected Max + Min values to equal zero, but was {0} + {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestTryteEquality()
        {
            Tryte a = 145;
            Tryte b = -145;
            var result = a != b;
            Assert.IsTrue(result, "Expected 145 and -145 to be not equal, but was {0}", result);
            b = 145;
            result = a == b;
            Assert.IsTrue(result, "Expected 145 to equal 145, but was {0}", result);
        }

        [TestMethod]
        public void TestTryteComparisons()
        {
            Tryte a = 125;
            Tryte b = 98;
            var result = a > b;
            Assert.IsTrue(result, "Expected 125 > 98 to be true, but returned {0}", result);
            result = b < a;
            Assert.IsTrue(result, "Expected 98 < 125 to be true, but returned {0}", result);
            a = 98;
            result = a <= b && b >= a;
            Assert.IsTrue(result, "Expected 98 <= 98 and 98 >= 98, but returned {0}", result);
            a = 148;
            var result2 = Tryte.COMPARET(a, b).Value;

            Tryte.COMPARET(a, b)
                            .Larger(() => Assert.IsTrue(true))
                            .Equal(() => Assert.IsTrue(false))
                            .Smaller(() => Assert.IsTrue(false));

            Assert.IsTrue(result2 == TritVal.p, "Expected 148 <=> 98 to be p, but returned {0} <=> {1} = {2}", a, b, result2);
            result2 = Tryte.COMPARET(b, a).Value;

            Tryte.COMPARET(b, a)
                            .Larger(() => Assert.IsTrue(false))
                            .Equal(() => Assert.IsTrue(false))
                            .Smaller(() => Assert.IsTrue(true));

            Assert.IsTrue(result2 == TritVal.n, "Expected 98 <=> 148 to be n, but returned {0} <=> {1} = {2}", a, b, result2);
            a = 12;
            b = 12;
            result2 = Tryte.COMPARET(a, b).Value;
            Assert.IsTrue(result2 == TritVal.z, "Expected 0 <=> 0 to be z, but returned {0} <=> {1} = {2}", a, b, result2);

            Tryte.COMPARET(a, b)
                            .Larger(() => Assert.IsTrue(false))
                            .Equal(() => Assert.IsTrue(true))
                            .Smaller(() => Assert.IsTrue(false));

            Tryte.COMPARET(a, b)
                            .LargerOrEqual(() => Assert.IsTrue(true))
                            .Smaller(() => Assert.IsTrue(false));
            a = 15;
            Tryte.COMPARET(a, b)
                            .LargerOrEqual(() => Assert.IsTrue(true))
                            .Smaller(() => Assert.IsTrue(false));

        }

        [TestMethod]
        public void TestTryteTritShift()
        {
            Tryte a = (Tryte)"+-+0-+";
            var result = a << 2;
            Assert.IsTrue(result == (Tryte)"+0-+00", "Expected trit shift left by 2, but it wasn't");
            result = a >> 4;
            Assert.IsTrue(result == (Tryte)"0000+-", "Expected trit shift right by 4, but it wasn't");
            result = a << 6;
            Assert.IsTrue(result == (Tryte)"000000", "Expected a full wipe (trit shift left by 6), but it wasn't");
        }

        [TestMethod]
        public void TestTryteAddition()
        {
            Tryte a = 127;
            Tryte b = 85;
            var result = a + b;
            Assert.IsTrue(result == 212, "Expected a = 127 + b = 85 to be 212, but returned {0} + {1} = {2}", a, b, result);
            a = -45;
            b = -12;
            result = a + b;
            Assert.IsTrue(result == -57, "Expected a = -45 + b = -12 to be -57, but returned {0} + {1} = {2}", a, b, result);
            a = -45;
            b = 12;
            result = a + b;
            Assert.IsTrue(result == -33, "Expected a = -45 + b = 12 to be -33, but returned {0} + {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestTryteSubtraction()
        {
            Tryte a = 145;
            Tryte b = 45;
            var result = a - b;
            Assert.IsTrue(result == 100, "Expected a = 145 - b = 45 to be 100, but returned {0}", result);
            b = -20;
            a = -30;
            result = a - b;
            Assert.IsTrue(result == -10, "Expected a = -20 - b = -30 to be -10, but returned {0}", result);
            a = 120;
            b = -20;
            result = a - b;
            Assert.IsTrue(result == 140, "Expected 120 - -20 to be 140, but returned {0} - {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestTryteMultiplication()
        {
            Tryte a = 3;
            Tryte b = 5;
            var result = a * b;
            Assert.IsTrue(result == 15, "Expected a = 3 * b = 5 to be 15, but returned {0} * {1} = {2}", a, b, result);
            a = -12;
            b = 4;
            result = a * b;
            Assert.IsTrue(result == -48, "Expected a = -12 * b = 4 to be -48, but returned {0} * {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestTryteDivision()
        {
            Tryte a = 12;
            Tryte b = 3;
            var result = a / b;
            Assert.IsTrue(result == 4, "Expected a = 12 / b = 3 to be 4, but returned {0} / {1} = {2}", a, b, result);
            a = 36;
            b = 5;
            result = a / b;
            Assert.IsTrue(result == 7, "Expected a = 36 / b = 5 to be 7, but returned {0} / {1} = {2}", a, b, result);
            a = -42;
            b = 2;
            result = a / b;
            Assert.IsTrue(result == -21, "Expected a = -42 / b = 2 to be -21, but returned {0} / {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestTryteModulus()
        {
            Tryte a = 15;
            Tryte b = 7;
            var result = a % b;
            Assert.IsTrue(result == 1, "Expected a = 15 % b = 7 to be 1, but returned {0} % {1} = {2}", a, b, result);
            a = -67;
            b = 20;
            result = a % b;
            Assert.IsTrue(result == 13, "Expected a = -67 % b = 20 to be 13, but returned {0} % {1} = {2}", a, b, result);
            a = 45;
            b = -20;
            result = a % b;
            Assert.IsTrue(result == -15, "Expected a = 45 % b = -20 to be -15, but returned {0} % {1} = {2}", a, b, result);
            a = -23;
            b = -3;
            result = a % b;
            Assert.IsTrue(result == -2, "Expected a = -23 % b = -3 to be -2, but returned {0} % {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestTryteAND()
        {
            Tryte a = (Tryte)"+-0-+0";
            Tryte b = (Tryte)"--+0-0";
            var result = a & b;
            var stringResult = (string)result;
            Assert.IsTrue(stringResult == "--0--0", "Expected +-0-+0 & --+0-0 to be --0--0, but returned {0} & {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestTryteOR()
        {
            Tryte a = (Tryte)"+-0-+0";
            Tryte b = (Tryte)"--+0-0";
            var result = a | b;
            var stringResult = (string)result;
            Assert.IsTrue(stringResult == "+-+0+0", "Expected +-0-+0 | --+0-0 to be +-+0+0, but returned {0} | {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestTryteXOR()
        {
            Tryte a = (Tryte)"+-0-+0";
            Tryte b = (Tryte)"--+0-0";
            var result = a ^ b;
            var stringResult = (string)result;
            Assert.IsTrue(stringResult == "+-00+0", "Expected +-0-+0 ^ --+0-0 to be +-00+0, but returned {0} ^ {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestTryteNegation()
        {
            Tryte a = 128;
            var result = ~a;
            Assert.IsTrue(result == -128, "Expected ~a = 128 to be -128, but returned ~{0} = {1}", a, result);
            a = -75;
            result = ~a;
            Assert.IsTrue(result == 75, "Expected ~a = -75 to be 75 but returned ~{0} = {1}", a, result);
        }
    }

    [TestClass]
    public class UnitTestIntT
    {
        [TestMethod]
        public void TestMinMax()
        {
            IntT a = IntT.MaxValue;
            IntT b = IntT.MinValue;
            var result = a + b;
            Assert.IsTrue(result == 0, "Expected Max + Min values to equal zero, but returned {0} + {1} = {2}", a, b, result);
            Assert.ThrowsException<OverflowException>(() => result = a + 1);
            Assert.ThrowsException<OverflowException>(() => result = b - 1);
        }

        [TestMethod]
        public void TestIntTComparisons()
        {
            IntT a = 12512344;
            IntT b = 982343;
            var result = a > b;
            Assert.IsTrue(result, "Expected 12512344 > 982343 to be true, but returned {0}", result);
            result = b < a;
            Assert.IsTrue(result, "Expected 982343 < 12512344 to be true, but returned {0}", result);
            a = 982343;
            result = a <= b && b >= a;
            Assert.IsTrue(result, "Expected 982343 <= 982343 and 982343 >= 982343 to be true, but returned {0}", result);
            a = 1480234000;
            var result2 = IntT.COMPARET(a, b).Value;

            IntT.COMPARET(a, b)
                            .Larger(() => Assert.IsTrue(true))
                            .Equal(() => Assert.IsTrue(false))
                            .Smaller(() => Assert.IsTrue(false));

            IntT.COMPARET(a, b)
                            .LargerOrEqual(() => Assert.IsTrue(true))
                            .Smaller(() => Assert.IsTrue(false));

            IntT.COMPARET(a, b)
                            .Smaller(() => Assert.IsTrue(false))
                            .Else(() => Assert.IsTrue(true));
            
            Assert.IsTrue(result2 == TritVal.p, "Expected 148 <=> 98 to be p, but returned {0} <=> {1} = {2}", a, b, result2);
            result2 = IntT.COMPARET(b, a).Value;
            Assert.IsTrue(result2 == TritVal.n, "Expected 98 <=> 148 to be n, but returned {0} <=> {1} = {2}", a, b, result2);
            a = 1200033;
            b = 1200033;

            IntT.COMPARET(a, b)
                            .Larger(() => Assert.IsTrue(false))
                            .Equal(() => Assert.IsTrue(true))
                            .Smaller(() => Assert.IsTrue(false));

            IntT.COMPARET(a, b)
                            .SmallerOrEqual(() => Assert.IsTrue(true))
                            .Larger(() => Assert.IsTrue(false));

            result2 = IntT.COMPARET(a, b).Value;
            Assert.IsTrue(result2 == TritVal.z, "Expected 0 <=> 0 to be z, but returned {0} <=> {1} = {2}", a, b, result2);

        }

        [TestMethod]
        public void TestTritShift()
        {
            IntT a = (IntT)"000+-+0-++00-+0++-+00-+0";
            var result = a << 2;
            Assert.IsTrue((string)result == "0+-+0-++00-+0++-+00-+000", "Expected shift left of 2 trits, but it didn't work");
            result = a << 5;
            Assert.IsTrue((string)result == "+0-++00-+0++-+00-+000000", "Expected shift left of 5 trits, but it didn't work");
            result = a >> 5;
            Assert.IsTrue((string)result == "00000000+-+0-++00-+0++-+", "Expected shift right of 5 trits, but it didn't work");
        }

        [TestMethod]
        public void TestInversion()
        {
            IntT a = 147;
            var result = ~a;
            Assert.IsTrue(result == -147, "Expected inversion of 147 to be -147, but returned {0} => {1}", a, result);
        }

        [TestMethod]
        public void TestIntTAddition()
        {
            IntT a = 1450;
            IntT b = 1200;
            var result = a + b;
            Assert.IsTrue(result == 2650, "Expected a = 1450 + b = 1200 to be 2650, but returned {0} + {1} = {2}", a, b, result);
            a = -50;
            b = 1450;
            result = a + b;
            Assert.IsTrue(result == 1400, "Expected a = -50 + b = 1450 to be 1400, but returned {0} + {1} = {2}", a, b, result);
            a = 34000;
            b = -4000;
            result = a + b;
            Assert.IsTrue(result == 30000, "Expected a = 34000 + b = -4000 to be 30000, but returned {0} + {1} = {2}", a, b, result);
            a = -12345;
            b = -54321;
            result = a + b;
            Assert.IsTrue(result == -66666, "Expected a = -12345 + b = -54321 to be -66666, but returned {0} + {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestIntTSubtraction()
        {
            IntT a = 1345;
            IntT b = 765;
            var result = a - b;
            Assert.IsTrue(result == 580, "Expected a = 1345 - b = 765 to be 580, but returned {0} - {1} = {2}", a, b, result);
            b = -450;
            result = a - b;
            Assert.IsTrue(result == 1795, "Expected a = 1345 - b = -450 to be 1795, but returned {0} - {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestIntTMultiplication()
        {
            IntT a = 120;
            IntT b = 5;
            var result = a * b;
            Assert.IsTrue(result == 600, "Expected a = 120 * b = 5 to be 600, but returned {0} * {1} = {2}", a, b, result);
            b = 6;
            result = a * b;
            Assert.IsTrue(result == 720, "Expected a = 120 * b = 6 to be 720, but returned {0} * {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestIntTDivision()
        {
            IntT a = 145;
            IntT b = 5;
            var result = a / b;
            Assert.IsTrue(result == 29, "Expected a = 145 / b = 5 to be 29, but returned {0} / {1} = {2}", a, b, result);
            a = 147;
            result = a / b;
            Assert.IsTrue(result == 29, "Expected a = 147 / b = 5 to be 29, but returned {0} / {1} = {2}", a, b, result);
            b = 0;
            Assert.ThrowsException<DivideByZeroException>(() => a / b, "Expected a dividebyzero exception, but it wasn't thrown.");
            b = -2;
            result = a / b;
            Assert.IsTrue(result == -73, "Expected a = 147 / b = -2 to be -73, but returned {0} / {1} = {2}", a, b, result);
            b = 158;
            result = a / b;
            Assert.IsTrue(result == 0, "Expected 147 / 158 to be 0,  but returned {0} / {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestIntTModulus()
        {
            IntT a = 270;
            IntT b = 28;
            var result = a % b;
            Assert.IsTrue(result == 18, "Expected a = 270 % b = 28 to be 18, but returned {0} % {1} = {2}", a, b, result);
            a = -459;
            b = 28;
            result = a % b;
            Assert.IsTrue(result == 17, "Expected a = -459 % b = 28 to be 17, but returned {0} % {1} = {2}", a, b, result);
            a = 7;
            b = -5;
            result = a % b;
            Assert.IsTrue(result == -3, "Expected a = 7 % b = -5 to be -3, but returned {0} % {1} = {2}", a, b, result);
            a = -23;
            b = -4;
            result = a % b;
            Assert.IsTrue(result == -3, "Expected a = -23 % b = -4 to be -3, but returned {0} % {1} = {2}", a, b, result);
            a = 24;
            b = 6;
            result = a % b;
            Assert.IsTrue(result == 0, "Expected a = 24 % b = 6 to be 0, but returned {0} % {1} = {2}", a, b, result);
            a = 3;
            b = 5;
            result = a % b;
            Assert.IsTrue(result == 3, "Expected 3 % 5 = 3, but returned {0} % {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestIntTPower()
        {
            IntT a = 5;
            IntT b = 10;
            var result = MathT.Pow(a, b);
            Assert.IsTrue(result == 9765625, "Expected 5 to the power of 10 to be 9765625, but returned {0} ^ {1} = {2}", a, b, result);
            a = 3;
            b = 23;
            result = MathT.Pow(a, b);
            Assert.IsTrue(result == 94143178827, "Expected 3 to the power of 23 to be 94143178827, but returned {0} ^ {1} = {2}", a, b, result);
        }
    }

    [TestClass]
    public class UnitTestCharT
    {
        [TestMethod]
        public void TestCharT()
        {
            CharT a = 'a';
            CharT b = 'b';
            var result = b - a;
            Assert.IsTrue(result == 1, "Expected b - a to be 1, but it wasn't");
            Assert.IsTrue(a.CharValue == 'a' && b.CharValue == 'b', "Expected a to be a and b to be b, but it wasn't");
        }
    }

    [TestClass]
    public class UnitTestConversions
    {
        [TestMethod]
        public void TestConversions()
        {
            IntT a = 25;
            FloatT b = 100.0;
            Tryte c = 12;
            var result = (FloatT)a;
            Assert.IsTrue(result == 25.0, "Expected FloatT conversion from IntT to be 25.0");
            var result2 = (IntT)b;
            Assert.IsTrue(result2 == 100, "Expected IntT conversion from FloatT to be 100");
            var result3 = (IntT)c;
            Assert.IsTrue(result3 == 12, "Expected IntT conversion from Tryte to be 12");
            
        }
    }

    [TestClass]
    public class UnitTestFloatT
    {
        [TestMethod]
        public void TestMaxMin()
        {
            FloatT a = FloatT.MaxValue;
            Assert.ThrowsException<ArgumentException>(() => a += FloatT.Epsilon, "Expected Argument Exception for too big number for FloatT but one wasn't thrown");
            a = FloatT.MinValue;
            Assert.ThrowsException<ArgumentException>(() => a -= FloatT.Epsilon, "Expected Argument Exception for too big number for FloatT but one wasn't thrown");
        }

        [TestMethod]
        public void TestFloatTComparisons()
        {
            FloatT a = 1.75;
            FloatT b = 3.8;
            FloatT c = 3.8;
            var result = a < b;
            Assert.IsTrue(result, "Expected 1.75 < 3.8 to be true, but returned {0} < {1} = {2}", a, b, result);
            result = b > a;
            Assert.IsTrue(result, "Expected 3.8 > 1.75 to be true, but returned {0} > {1} = {2}", b, a, result);
            result = b == c && b <= c && c >= b;
            Assert.IsTrue(result, "Expected 3.8 == 3.8 to be true as well as <= and >=, but returned {0} == {1} = {2}", b, c, result);
            result = b != a;
            Assert.IsTrue(result, "Expected 3.8 != 1.75 to be true, but returned {0} == {1} = {2}", b, c, result);
            var result2 = FloatT.COMPARET(a, b).Value;
            
            FloatT.COMPARET(a, b)
                            .Larger(() => Assert.IsTrue(false))
                            .Equal(() => Assert.IsTrue(false))
                            .Smaller(() => Assert.IsTrue(true));

            FloatT.COMPARET(a, b)
                            .Larger(() => Assert.IsTrue(false))
                            .SmallerOrEqual(() => Assert.IsTrue(true));

            Assert.IsTrue(result2 == TritVal.n, "Expected 1.75 <=> 3.8 to be n, but returned COMPARET({0}, {1}).Value = {2}", a, b, result2);
            result2 = FloatT.COMPARET(b, a).Value;

            FloatT.COMPARET(b, a)
                            .Larger(() => Assert.IsTrue(true))
                            .Equal(() => Assert.IsTrue(false))
                            .Smaller(() => Assert.IsTrue(false));

            Assert.IsTrue(result2 == TritVal.p, "Expected 3.8 <=> 1.75 to be p, but returned COMPARET({0}, {1}).Value = {2}", a, b, result2);
            result2 = FloatT.COMPARET(b, c).Value;

            FloatT.COMPARET(b, c)
                            .Larger(() => Assert.IsTrue(false))
                            .Equal(() => Assert.IsTrue(true))
                            .Smaller(() => Assert.IsTrue(false));
            
            FloatT.COMPARET(b, c)
                            .LargerOrEqual(() => Assert.IsTrue(true))
                            .Equal(() => Assert.IsTrue(true))
                            .Smaller(() => Assert.IsTrue(false));

            FloatT.COMPARET(b, c)
                            .Larger(() => Assert.IsTrue(false))
                            .Else(() => Assert.IsTrue(true));

            Assert.IsTrue(result2 == TritVal.z, "Expected 3.8 <=> 3.8 to be z, but returned COMPARET({0}, {1}).Value = {2}", b, c, result2);
        }

        [TestMethod]
        public void TestFloatTAddition()
        {
            FloatT a = 3.687;
            FloatT b = 1.313;
            var result = a + b;
            Assert.IsTrue(result == 5.0, "Expected 3.687 + 1.313 to be 5.0, but returned {0} + {1} = {2}", a, b, result);
            a = 3.687123;
            b = 1.312877;
            result = a + b;
            Assert.IsTrue(result == 5.0, "Expected 3.687123 + 1.312877 to be 5.0, but returned {0} + {1} = {2}", a, b, result);
            a = 5.6873593E10;
            b = 2.3492837E10;
            result = a + b;
            var shouldEqual = 8.036643E10;
            Assert.IsTrue(result == shouldEqual, "{0} + {1} = {2}", a, b, result);
            a = 1.40684745E-10;
            b = 8.62048494E-10;
            result = a + b;
            shouldEqual = 1.002733E-9;
            Assert.IsTrue(ApproxEqual(shouldEqual, result), "Expected 1.40684745E-10 + 8.62048494E-10 to be 1.00273324E-9 (rounded to nearest level of precision digit),\n but returned {0} + {1} = {2}", a, b, result);
            a = 125;
            b = 1123;
            result = a + b;
            shouldEqual = 1248;
            Assert.IsTrue(result == shouldEqual, "Expected 125 + 1123 to equal 1248 but returned {0} + {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestFloatTSubtraction()
        {
            FloatT a = 5.0;
            FloatT b = 1.313;
            var result = a - b;
            Assert.IsTrue(result == 3.687, "Expected 5.0 - 1.313 to be 3.687 but returned {0} - {1} = {2}", a, b, result);
            b = 1.312877;
            result = a - b;
            Assert.IsTrue(result == 3.687123, "Expected 5.0 - 1.312877 to be 3.687123 but returned {0} - {1} = {2}", a, b, result);
            a = 1.00273323E-9;
            b = 1.40684745E-10;
            result = a - b;
            var shouldEqual = 8.620485E-10;
            Assert.IsTrue(ApproxEqual(shouldEqual, result), "Expected 1.00273324E-9 - 1.40684745E-10 to be close to 8.62048485E-10, but returned \n {0} - {1} = {2} \n with margin of {3}", a, b, result, MarginOfPrecision(shouldEqual));
            a = 1.00273323;
            b = 1.40684745;
            result = a - b;
            shouldEqual = -0.4041142;
            Assert.IsTrue(result == shouldEqual, "Expected 1.00273324 - 1.40684745 to be -0.40411422, but returned \n {0} - {1} = {2}", a, b, result);
            a = 3.4562312E10;
            b = 2.5432985E10;
            result = a - b;
            shouldEqual = 9.129327E9;
            Assert.IsTrue(result == shouldEqual, "Expected 3.4562312E10 - 2.5432985E10 to be to 9.129327E9, but returned \n {0} - {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestFloatTMultiplication()
        {
            FloatT a = 2.5;
            FloatT b = 2.0;
            var result = a * b;
            var shouldEqual = 5.0;
            Assert.IsTrue(result == shouldEqual, "Expected 2.5 * 2.0 to be 5.0, but returned {0} * {1} = {2}", a, b, result);
            a = 3.14159265359;
            b = 2.71828182846;
            result = a * b;
            shouldEqual = 8.539734;
            Assert.IsTrue(ApproxEqual(shouldEqual, result), "Expected pi * e to be 8.539734, but returned {0} * {1} = {2}", a, b, result);
            a = 12345.6789;
            b = 98765.4321;
            result = a * b;
            shouldEqual = 1219326311.13;
            Assert.IsTrue(ApproxEqual(shouldEqual, result), "Expected 12345.6789 * 98765.4321 to be close to 1219326311.13, but returned {0} * {1} = {2}", a, b, result);
            a = 1.44873647493E-20;
            b = -5.38371718394E-50;
            result = a * b;
            shouldEqual = -7.79958745508E-70;
            Assert.IsTrue(ApproxEqual(shouldEqual, result), "Two very small floats should equal -7.799587E-250, but returned {0} * {1} = {2}", a, b, result);
            a = 1.38563483734E20;
            b = 3.48272382434E20;
            result = a * b;
            shouldEqual = 4.82578345984E40;
            Assert.IsTrue(ApproxEqual(shouldEqual, result), "Two very large floats should equal 4.82579345984E40, but returned {0} * {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestFloatTDivision()
        {
            FloatT a = 10.0;
            FloatT b = 2.0;
            var result = a / b;
            var shouldEqual = 5.0;
            Assert.IsTrue(result == shouldEqual, "Expected 10.0 / 5.0 = 2.0, but returned {0} / {1} = {2}", a, b, result);
            a = 10.0;
            b = 0.126;
            result = a / b;
            shouldEqual = 79.365079;
            Assert.IsTrue(ApproxEqual(shouldEqual, result), "Expected 10 / 0.126 to be about 79.365079, but returned {0} / {1} = {2}", a, b, result);
            a = 127.458;
            b = 14.234;
            result = a / b;
            shouldEqual = 8.95447;
            Assert.IsTrue(ApproxEqual(shouldEqual, result), "Expected 127.458 / 14.234 to be about 8.95447, but returned {0} / {1} = {2}", a, b, result);
            a = 1000;
            b = 5;
            result = a / b;
            shouldEqual = 200;
            Assert.IsTrue(result == shouldEqual, "Expected 1000 / 5 = 200, but returned {0} / {1} = {2}", a, b, result);
            a = MathT.PI;
            b = 2.0;
            result = a / b;
            shouldEqual = 1.57079633679;
            Assert.IsTrue(ApproxEqual(shouldEqual, result), "Expected PI / 2 to be about 1.570796, but returned {0} / {1} = {2}", a, b, result);
            a = 5;
            b = 7;
            result = a / b;
            shouldEqual = 0.71428;
            Assert.IsTrue(ApproxEqual(shouldEqual, result), "Expected 5 / 7 to be about 0.71428, but returned {0} / {1} = {2}", a, b, result);
        }

        [TestMethod]
        public void TestFloatTModulus()
        {
            FloatT a = 10.0;
            FloatT b = 0.126;
            var result = a % b;
            var shouldEqual = 0.046;
            Assert.IsTrue(result == shouldEqual, "Expected 10.0 % 0.126 to be 0.046, but returned {0} % {1} = {2}", a, b, result);
            a = 127.458;
            b = 14.234;
            result = a % b;
            shouldEqual = 13.586;
            Assert.IsTrue(result == shouldEqual, "Expected 127.458 % 0.126 = 14.234, but returned {0} % {1} = {2}", a, b, result);
            a = 1000;
            b = 5;
            result = a % b;
            shouldEqual = 0;
            Assert.IsTrue(result == shouldEqual, "Expected 1000 % 5 = 0, but returned {0} % {1} = {2}", a, b, result);
            a = MathT.PI;
            b = MathT.E;
            result = a % b;
            shouldEqual = 0.4233108251308;
            Assert.IsTrue(ApproxEqual(shouldEqual, result), "Expected PI % E = 0.4233108, but returned {0} % {1} = {2}", a, b, result);
            a = 3;
            b = 5;
            result = a % b;
            shouldEqual = 3;
            Assert.IsTrue(result == shouldEqual, "Expected 3 % 5 = 3, but returned {0} % {1} = {2}", a, b, result);
        }

        public bool ApproxEqual(FloatT expected, FloatT actual)
        {
            return actual <= (expected.DoubleValue + MarginOfPrecision(expected)) && actual >= (expected.DoubleValue - MarginOfPrecision(expected));
        }

        public double MarginOfPrecision(FloatT expectedValue)
        {
            var doubleString = expectedValue.DoubleValue.ToString("E");
            if (doubleString.Contains('E'))
            {
                var sigsplit = doubleString.Split('E');
                var exp = int.Parse(sigsplit[1]);
                double margin;
                if (exp < 0)
                {
                    margin = Math.Pow(10, exp - expectedValue.ExpectedNDigitsOfPrecision() + 3);
                }
                else if (exp > 0)
                {
                    margin = Math.Pow(10, exp + expectedValue.ExpectedNDigitsOfPrecision() - 3);
                }
                else
                {
                    margin = Math.Pow(10, -expectedValue.ExpectedNDigitsOfPrecision() + 3);
                }
                
                return margin;
            }
            else
            {
                return Math.Pow(10, -expectedValue.ExpectedNDigitsOfPrecision());
            }
        }
    }
