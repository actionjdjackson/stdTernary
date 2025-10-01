using System;

namespace stdTernary;

/// <summary>
/// Provides conversion methods between balanced ternary types and common .NET binary types.
/// </summary>
public static class TernaryConverter
{
    #region IntT Conversions

    /// <summary>
    /// Converts an IntT to a 32-bit signed integer.
    /// </summary>
    public static int IntTToInt32(IntT value)
    {
        long longValue = value.ToInt64();
        if (longValue < int.MinValue || longValue > int.MaxValue)
            throw new OverflowException("IntT value is outside the range of Int32.");
        return (int)longValue;
    }

    /// <summary>
    /// Converts an IntT to a 32-bit unsigned integer.
    /// </summary>
    public static uint IntTToUInt32(IntT value)
    {
        long longValue = value.ToInt64();
        if (longValue < uint.MinValue || longValue > uint.MaxValue)
            throw new OverflowException("IntT value is outside the range of UInt32.");
        return (uint)longValue;
    }

    /// <summary>
    /// Converts an IntT to a 16-bit signed integer.
    /// </summary>
    public static short IntTToInt16(IntT value)
    {
        long longValue = value.ToInt64();
        if (longValue < short.MinValue || longValue > short.MaxValue)
            throw new OverflowException("IntT value is outside the range of Int16.");
        return (short)longValue;
    }

    /// <summary>
    /// Converts an IntT to a 16-bit unsigned integer.
    /// </summary>
    public static ushort IntTToUInt16(IntT value)
    {
        long longValue = value.ToInt64();
        if (longValue < ushort.MinValue || longValue > ushort.MaxValue)
            throw new OverflowException("IntT value is outside the range of UInt16.");
        return (ushort)longValue;
    }

    /// <summary>
    /// Converts an IntT to an 8-bit unsigned integer.
    /// </summary>
    public static byte IntTToUInt8(IntT value)
    {
        long longValue = value.ToInt64();
        if (longValue < byte.MinValue || longValue > byte.MaxValue)
            throw new OverflowException("IntT value is outside the range of Byte.");
        return (byte)longValue;
    }

    /// <summary>
    /// Converts an IntT to an 8-bit signed integer.
    /// </summary>
    public static sbyte IntTToInt8(IntT value)
    {
        long longValue = value.ToInt64();
        if (longValue < sbyte.MinValue || longValue > sbyte.MaxValue)
            throw new OverflowException("IntT value is outside the range of SByte.");
        return (sbyte)longValue;
    }

    /// <summary>
    /// Converts an IntT to a decimal.
    /// </summary>
    public static decimal IntTToDecimal(IntT value)
    {
        return (decimal)value.ToInt64();
    }

    /// <summary>
    /// Converts an IntT to a 64-bit unsigned integer.
    /// </summary>
    public static ulong IntTToUInt64(IntT value)
    {
        long longValue = value.ToInt64();
        if (longValue < 0)
            throw new OverflowException("IntT value is negative and cannot be converted to UInt64.");
        return (ulong)longValue;
    }

    /// <summary>
    /// Creates an IntT from a 32-bit signed integer.
    /// </summary>
    public static IntT IntTFromInt32(int value) => new IntT(value);

    /// <summary>
    /// Creates an IntT from a 32-bit unsigned integer.
    /// </summary>
    public static IntT IntTFromUInt32(uint value) => new IntT(value);

    /// <summary>
    /// Creates an IntT from a 16-bit signed integer.
    /// </summary>
    public static IntT IntTFromInt16(short value) => new IntT(value);

    /// <summary>
    /// Creates an IntT from a 16-bit unsigned integer.
    /// </summary>
    public static IntT IntTFromUInt16(ushort value) => new IntT(value);

    /// <summary>
    /// Creates an IntT from an 8-bit unsigned integer.
    /// </summary>
    public static IntT IntTFromUInt8(byte value) => new IntT(value);

    /// <summary>
    /// Creates an IntT from an 8-bit signed integer.
    /// </summary>
    public static IntT IntTFromInt8(sbyte value) => new IntT(value);

    /// <summary>
    /// Creates an IntT from a decimal.
    /// </summary>
    public static IntT IntTFromDecimal(decimal value)
    {
        if (value < long.MinValue || value > long.MaxValue)
            throw new OverflowException("Decimal value is outside the range of IntT.");
        return new IntT((long)value);
    }

    /// <summary>
    /// Creates an IntT from a 64-bit unsigned integer.
    /// </summary>
    public static IntT IntTFromUInt64(ulong value)
    {
        if (value > long.MaxValue)
            throw new OverflowException("UInt64 value is outside the range of IntT.");
        return new IntT((long)value);
    }

    #endregion

    #region FloatT Conversions

    /// <summary>
    /// Converts a FloatT to a single-precision floating-point number.
    /// </summary>
    public static float FloatTToFloat(FloatT value) => (float)value.ToDouble();

    /// <summary>
    /// Converts a FloatT to a decimal.
    /// </summary>
    public static decimal FloatTToDecimal(FloatT value)
    {
        double doubleValue = value.ToDouble();
        if (doubleValue < (double)decimal.MinValue || doubleValue > (double)decimal.MaxValue)
            throw new OverflowException("FloatT value is outside the range of Decimal.");
        return (decimal)doubleValue;
    }

    /// <summary>
    /// Creates a FloatT from a single-precision floating-point number.
    /// </summary>
    public static FloatT FloatTFromFloat(float value) => FloatT.FromDouble(value);

    /// <summary>
    /// Creates a FloatT from a decimal.
    /// </summary>
    public static FloatT FloatTFromDecimal(decimal value) => FloatT.FromDouble((double)value);

    #endregion

    #region Tryte Conversions

    /// <summary>
    /// Converts a Tryte to an 8-bit unsigned integer.
    /// </summary>
    public static byte TryteToUInt8(Tryte value)
    {
        short shortValue = value.ShortValue;
        if (shortValue < byte.MinValue || shortValue > byte.MaxValue)
            throw new OverflowException("Tryte value is outside the range of Byte.");
        return (byte)shortValue;
    }

    /// <summary>
    /// Converts a Tryte to an 8-bit signed integer.
    /// </summary>
    public static sbyte TryteToInt8(Tryte value)
    {
        short shortValue = value.ShortValue;
        if (shortValue < sbyte.MinValue || shortValue > sbyte.MaxValue)
            throw new OverflowException("Tryte value is outside the range of SByte.");
        return (sbyte)shortValue;
    }

    /// <summary>
    /// Converts a Tryte to a 16-bit unsigned integer.
    /// </summary>
    public static ushort TryteToUInt16(Tryte value)
    {
        short shortValue = value.ShortValue;
        if (shortValue < 0)
            throw new OverflowException("Tryte value is negative and cannot be converted to UInt16.");
        return (ushort)shortValue;
    }

    /// <summary>
    /// Converts a Tryte to a 32-bit unsigned integer.
    /// </summary>
    public static uint TryteToUInt32(Tryte value)
    {
        short shortValue = value.ShortValue;
        if (shortValue < 0)
            throw new OverflowException("Tryte value is negative and cannot be converted to UInt32.");
        return (uint)shortValue;
    }

    /// <summary>
    /// Converts a Tryte to a 64-bit unsigned integer.
    /// </summary>
    public static ulong TryteToUInt64(Tryte value)
    {
        short shortValue = value.ShortValue;
        if (shortValue < 0)
            throw new OverflowException("Tryte value is negative and cannot be converted to UInt64.");
        return (ulong)shortValue;
    }

    /// <summary>
    /// Converts a Tryte to a decimal.
    /// </summary>
    public static decimal TryteToDecimal(Tryte value) => (decimal)value.ShortValue;

    /// <summary>
    /// Creates a Tryte from an 8-bit unsigned integer.
    /// </summary>
    public static Tryte TryteFromUInt8(byte value) => new Tryte((short)value);

    /// <summary>
    /// Creates a Tryte from an 8-bit signed integer.
    /// </summary>
    public static Tryte TryteFromInt8(sbyte value) => new Tryte((short)value);

    /// <summary>
    /// Creates a Tryte from a 16-bit unsigned integer.
    /// </summary>
    public static Tryte TryteFromUInt16(ushort value)
    {
        if (value > Tryte.MaxValue)
            throw new OverflowException("UInt16 value is outside the range of Tryte.");
        return new Tryte((short)value);
    }

    /// <summary>
    /// Creates a Tryte from a 32-bit unsigned integer.
    /// </summary>
    public static Tryte TryteFromUInt32(uint value)
    {
        if (value > Tryte.MaxValue)
            throw new OverflowException("UInt32 value is outside the range of Tryte.");
        return new Tryte((short)value);
    }

    /// <summary>
    /// Creates a Tryte from a 64-bit unsigned integer.
    /// </summary>
    public static Tryte TryteFromUInt64(ulong value)
    {
        if (value > (ulong)Tryte.MaxValue)
            throw new OverflowException("UInt64 value is outside the range of Tryte.");
        return new Tryte((short)value);
    }

    /// <summary>
    /// Creates a Tryte from a decimal.
    /// </summary>
    public static Tryte TryteFromDecimal(decimal value)
    {
        if (value < Tryte.MinValue || value > Tryte.MaxValue)
            throw new OverflowException("Decimal value is outside the range of Tryte.");
        return new Tryte((short)value);
    }

    #endregion

    #region Trit Conversions

    /// <summary>
    /// Converts a Trit to a boolean (true if positive, false otherwise).
    /// </summary>
    public static bool ToBoolean(Trit value) => (bool)value;

    /// <summary>
    /// Creates a Trit from a boolean (true = 1, false = -1).
    /// </summary>
    public static Trit FromBoolean(bool value) => (Trit)value;

    #endregion
}
