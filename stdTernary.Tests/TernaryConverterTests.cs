using Microsoft.VisualStudio.TestTools.UnitTesting;
using stdTernary;

namespace stdTernary.Tests;

[TestClass]
public class TernaryConverterTests
{
    #region IntT Conversion Tests

    [TestMethod]
    public void IntT_ToInt32_ConvertsCorrectly()
    {
        IntT value = new IntT(12345);
        int result = TernaryConverter.IntTToInt32(value);
        Assert.AreEqual(12345, result);
    }

    [TestMethod]
    public void IntT_ToInt32_ThrowsOnOverflow()
    {
        IntT value = new IntT((long)int.MaxValue + 1);
        Assert.ThrowsException<OverflowException>(() => TernaryConverter.IntTToInt32(value));
    }

    [TestMethod]
    public void IntT_ToUInt32_ConvertsCorrectly()
    {
        IntT value = new IntT(12345);
        uint result = TernaryConverter.IntTToUInt32(value);
        Assert.AreEqual(12345u, result);
    }

    [TestMethod]
    public void IntT_ToUInt32_ThrowsOnNegative()
    {
        IntT value = new IntT(-1);
        Assert.ThrowsException<OverflowException>(() => TernaryConverter.IntTToUInt32(value));
    }

    [TestMethod]
    public void IntT_ToInt16_ConvertsCorrectly()
    {
        IntT value = new IntT(1234);
        short result = TernaryConverter.IntTToInt16(value);
        Assert.AreEqual((short)1234, result);
    }

    [TestMethod]
    public void IntT_ToInt16_ThrowsOnOverflow()
    {
        IntT value = new IntT(40000);
        Assert.ThrowsException<OverflowException>(() => TernaryConverter.IntTToInt16(value));
    }

    [TestMethod]
    public void IntT_ToUInt16_ConvertsCorrectly()
    {
        IntT value = new IntT(1234);
        ushort result = TernaryConverter.IntTToUInt16(value);
        Assert.AreEqual((ushort)1234, result);
    }

    [TestMethod]
    public void IntT_ToUInt8_ConvertsCorrectly()
    {
        IntT value = new IntT(123);
        byte result = TernaryConverter.IntTToUInt8(value);
        Assert.AreEqual((byte)123, result);
    }

    [TestMethod]
    public void IntT_ToUInt8_ThrowsOnOverflow()
    {
        IntT value = new IntT(300);
        Assert.ThrowsException<OverflowException>(() => TernaryConverter.IntTToUInt8(value));
    }

    [TestMethod]
    public void IntT_ToInt8_ConvertsCorrectly()
    {
        IntT value = new IntT(-123);
        sbyte result = TernaryConverter.IntTToInt8(value);
        Assert.AreEqual((sbyte)(-123), result);
    }

    [TestMethod]
    public void IntT_ToDecimal_ConvertsCorrectly()
    {
        IntT value = new IntT(12345);
        decimal result = TernaryConverter.IntTToDecimal(value);
        Assert.AreEqual(12345m, result);
    }

    [TestMethod]
    public void IntT_ToUInt64_ConvertsCorrectly()
    {
        IntT value = new IntT(12345);
        ulong result = TernaryConverter.IntTToUInt64(value);
        Assert.AreEqual(12345ul, result);
    }

    [TestMethod]
    public void IntT_ToUInt64_ThrowsOnNegative()
    {
        IntT value = new IntT(-1);
        Assert.ThrowsException<OverflowException>(() => TernaryConverter.IntTToUInt64(value));
    }

    [TestMethod]
    public void IntT_FromInt32_ConvertsCorrectly()
    {
        IntT result = TernaryConverter.IntTFromInt32(12345);
        Assert.AreEqual(12345L, result.ToInt64());
    }

    [TestMethod]
    public void IntT_FromUInt32_ConvertsCorrectly()
    {
        IntT result = TernaryConverter.IntTFromUInt32(12345u);
        Assert.AreEqual(12345L, result.ToInt64());
    }

    [TestMethod]
    public void IntT_FromInt16_ConvertsCorrectly()
    {
        IntT result = TernaryConverter.IntTFromInt16((short)1234);
        Assert.AreEqual(1234L, result.ToInt64());
    }

    [TestMethod]
    public void IntT_FromUInt16_ConvertsCorrectly()
    {
        IntT result = TernaryConverter.IntTFromUInt16((ushort)1234);
        Assert.AreEqual(1234L, result.ToInt64());
    }

    [TestMethod]
    public void IntT_FromUInt8_ConvertsCorrectly()
    {
        IntT result = TernaryConverter.IntTFromUInt8((byte)123);
        Assert.AreEqual(123L, result.ToInt64());
    }

    [TestMethod]
    public void IntT_FromInt8_ConvertsCorrectly()
    {
        IntT result = TernaryConverter.IntTFromInt8((sbyte)(-123));
        Assert.AreEqual(-123L, result.ToInt64());
    }

    [TestMethod]
    public void IntT_FromDecimal_ConvertsCorrectly()
    {
        IntT result = TernaryConverter.IntTFromDecimal(12345.6m);
        Assert.AreEqual(12345L, result.ToInt64());
    }

    [TestMethod]
    public void IntT_FromUInt64_ConvertsCorrectly()
    {
        IntT result = TernaryConverter.IntTFromUInt64(12345ul);
        Assert.AreEqual(12345L, result.ToInt64());
    }

    [TestMethod]
    public void IntT_FromUInt64_ThrowsOnOverflow()
    {
        Assert.ThrowsException<OverflowException>(() => 
            TernaryConverter.IntTFromUInt64((ulong)long.MaxValue + 1));
    }

    #endregion

    #region FloatT Conversion Tests

    [TestMethod]
    public void FloatT_ToFloat_ConvertsCorrectly()
    {
        FloatT value = FloatT.FromDouble(123.456);
        float result = TernaryConverter.FloatTToFloat(value);
        Assert.AreEqual(123.456f, result, 0.001f);
    }

    [TestMethod]
    public void FloatT_ToDecimal_ConvertsCorrectly()
    {
        FloatT value = FloatT.FromDouble(123.456);
        decimal result = TernaryConverter.FloatTToDecimal(value);
        Assert.AreEqual(123.456m, result, 0.001m);
    }

    [TestMethod]
    public void FloatT_FromFloat_ConvertsCorrectly()
    {
        FloatT result = TernaryConverter.FloatTFromFloat(123.456f);
        double resultDouble = result.ToDouble();
        Assert.AreEqual(123.456, resultDouble, 0.001);
    }

    [TestMethod]
    public void FloatT_FromDecimal_ConvertsCorrectly()
    {
        FloatT result = TernaryConverter.FloatTFromDecimal(123.456m);
        double resultDouble = result.ToDouble();
        Assert.AreEqual(123.456, resultDouble, 0.001);
    }

    #endregion

    #region Tryte Conversion Tests

    [TestMethod]
    public void Tryte_ToUInt8_ConvertsCorrectly()
    {
        Tryte value = new Tryte(123);
        byte result = TernaryConverter.TryteToUInt8(value);
        Assert.AreEqual((byte)123, result);
    }

    [TestMethod]
    public void Tryte_ToUInt8_ThrowsOnNegative()
    {
        Tryte value = new Tryte(-1);
        Assert.ThrowsException<OverflowException>(() => TernaryConverter.TryteToUInt8(value));
    }

    [TestMethod]
    public void Tryte_ToInt8_ConvertsCorrectly()
    {
        Tryte value = new Tryte(-123);
        sbyte result = TernaryConverter.TryteToInt8(value);
        Assert.AreEqual((sbyte)(-123), result);
    }

    [TestMethod]
    public void Tryte_ToUInt16_ConvertsCorrectly()
    {
        Tryte value = new Tryte(123);
        ushort result = TernaryConverter.TryteToUInt16(value);
        Assert.AreEqual((ushort)123, result);
    }

    [TestMethod]
    public void Tryte_ToUInt16_ThrowsOnNegative()
    {
        Tryte value = new Tryte(-1);
        Assert.ThrowsException<OverflowException>(() => TernaryConverter.TryteToUInt16(value));
    }

    [TestMethod]
    public void Tryte_ToUInt32_ConvertsCorrectly()
    {
        Tryte value = new Tryte(123);
        uint result = TernaryConverter.TryteToUInt32(value);
        Assert.AreEqual(123u, result);
    }

    [TestMethod]
    public void Tryte_ToUInt64_ConvertsCorrectly()
    {
        Tryte value = new Tryte(123);
        ulong result = TernaryConverter.TryteToUInt64(value);
        Assert.AreEqual(123ul, result);
    }

    [TestMethod]
    public void Tryte_ToDecimal_ConvertsCorrectly()
    {
        Tryte value = new Tryte(123);
        decimal result = TernaryConverter.TryteToDecimal(value);
        Assert.AreEqual(123m, result);
    }

    [TestMethod]
    public void Tryte_FromUInt8_ConvertsCorrectly()
    {
        Tryte result = TernaryConverter.TryteFromUInt8((byte)123);
        Assert.AreEqual((short)123, result.ShortValue);
    }

    [TestMethod]
    public void Tryte_FromInt8_ConvertsCorrectly()
    {
        Tryte result = TernaryConverter.TryteFromInt8((sbyte)(-123));
        Assert.AreEqual((short)(-123), result.ShortValue);
    }

    [TestMethod]
    public void Tryte_FromUInt16_ConvertsCorrectly()
    {
        Tryte result = TernaryConverter.TryteFromUInt16((ushort)123);
        Assert.AreEqual((short)123, result.ShortValue);
    }

    [TestMethod]
    public void Tryte_FromUInt32_ConvertsCorrectly()
    {
        Tryte result = TernaryConverter.TryteFromUInt32(123u);
        Assert.AreEqual((short)123, result.ShortValue);
    }

    [TestMethod]
    public void Tryte_FromUInt64_ConvertsCorrectly()
    {
        Tryte result = TernaryConverter.TryteFromUInt64(123ul);
        Assert.AreEqual((short)123, result.ShortValue);
    }

    [TestMethod]
    public void Tryte_FromDecimal_ConvertsCorrectly()
    {
        Tryte result = TernaryConverter.TryteFromDecimal(123.6m);
        Assert.AreEqual((short)123, result.ShortValue);
    }

    #endregion

    #region Trit Conversion Tests

    [TestMethod]
    public void Trit_ToBoolean_PositiveIsTrue()
    {
        Trit value = new Trit(1);
        bool result = TernaryConverter.ToBoolean(value);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Trit_ToBoolean_ZeroIsFalse()
    {
        Trit value = new Trit(0);
        bool result = TernaryConverter.ToBoolean(value);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Trit_ToBoolean_NegativeIsFalse()
    {
        Trit value = new Trit(-1);
        bool result = TernaryConverter.ToBoolean(value);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Trit_FromBoolean_TrueIsPositive()
    {
        Trit result = TernaryConverter.FromBoolean(true);
        Assert.AreEqual(1, (int)result);
    }

    [TestMethod]
    public void Trit_FromBoolean_FalseIsNegative()
    {
        Trit result = TernaryConverter.FromBoolean(false);
        Assert.AreEqual(-1, (int)result);
    }

    #endregion

    #region Round-Trip Tests

    [TestMethod]
    public void IntT_RoundTrip_Int32()
    {
        int original = 12345;
        IntT converted = TernaryConverter.IntTFromInt32(original);
        int result = TernaryConverter.IntTToInt32(converted);
        Assert.AreEqual(original, result);
    }

    [TestMethod]
    public void FloatT_RoundTrip_Float()
    {
        float original = 123.456f;
        FloatT converted = TernaryConverter.FloatTFromFloat(original);
        float result = TernaryConverter.FloatTToFloat(converted);
        Assert.AreEqual(original, result, 0.001f);
    }

    [TestMethod]
    public void Tryte_RoundTrip_Byte()
    {
        byte original = 123;
        Tryte converted = TernaryConverter.TryteFromUInt8(original);
        byte result = TernaryConverter.TryteToUInt8(converted);
        Assert.AreEqual(original, result);
    }

    #endregion
}
