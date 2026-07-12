using System;
using System.Diagnostics.CodeAnalysis;

namespace stdTernary;

/// <summary>
/// 32-trit balanced ternary integer backed by a single packed <see cref="ulong"/>.
/// All arithmetic is word-parallel (SWAR) over the packed representation - no
/// per-trit decode/normalize/encode loops - following Frieder &amp; Luk's dual-rail
/// scheme for carry separation. Comparison is a single xor+compare thanks to the
/// order-isomorphic encoding.
/// </summary>
public readonly struct IntT : IEquatable<IntT>, IComparable<IntT>
{
    public const int TritCount = 32;
    private static readonly long MaxMagnitudeLong = ComputeMaxMagnitude(TritCount);

    private readonly ulong _packed;

    internal ulong Packed => _packed;

    private IntT(ulong packed)
    {
        _packed = packed;
    }

    public IntT(long value)
    {
        _packed = BalancedTernaryEncoding.FromInt64(value, TritCount);
    }

    public static IntT Zero { get; } = new IntT(0L);
    public static IntT One { get; } = new IntT(1L);
    public static IntT NegativeOne { get; } = new IntT(-1L);
    public static IntT MaxValue { get; } = new IntT(MaxMagnitudeLong);
    public static IntT MinValue { get; } = new IntT(-MaxMagnitudeLong);

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

    /// <summary>Three-way comparison: one xor + one integer compare (one instruction on ternary hardware).</summary>
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

    public static IntT operator -(IntT value)
    {
        if (TernaryFom.Enabled) TernaryFom.Negations++;
        return new IntT(BalancedTernaryEncoding.Negate(value._packed));
    }

    public static IntT operator +(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.Additions++;
        return new IntT(BalancedTernaryEncoding.AddSwar(left._packed, right._packed, TritCount));
    }

    public static IntT operator -(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.Additions++;
        return new IntT(BalancedTernaryEncoding.SubSwar(left._packed, right._packed, TritCount));
    }

    public static IntT operator *(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.Multiplications++;
        return new IntT(MultiplyPacked(left._packed, right._packed));
    }

    public static IntT operator /(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.Divisions++;
        var (quotient, _) = DivRemPacked(left._packed, right._packed);
        return new IntT(quotient);
    }

    public static IntT operator %(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.Divisions++;
        var (_, remainder) = DivRemPacked(left._packed, right._packed);
        return new IntT(remainder);
    }

    public static IntT operator &(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        return new IntT(BalancedTernaryEncoding.MinSwar(left._packed, right._packed));
    }

    public static IntT operator |(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        return new IntT(BalancedTernaryEncoding.MaxSwar(left._packed, right._packed));
    }

    public static IntT operator ^(IntT left, IntT right)
    {
        if (TernaryFom.Enabled) TernaryFom.TritwiseOps++;
        return new IntT(BalancedTernaryEncoding.XorSwar(left._packed, right._packed));
    }

    public static IntT operator ~(IntT value)
    {
        if (TernaryFom.Enabled) TernaryFom.Negations++;
        return new IntT(BalancedTernaryEncoding.Negate(value._packed));
    }

    public static IntT operator <<(IntT value, int count)
    {
        if (TernaryFom.Enabled) TernaryFom.Shifts++;
        return new IntT(ShiftLeftPacked(value._packed, count));
    }

    public static IntT operator >>(IntT value, int count)
    {
        if (TernaryFom.Enabled) TernaryFom.Shifts++;
        return new IntT(ShiftRightPacked(value._packed, count));
    }

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
        return packed << (count * 2);
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

    private static ulong MultiplyPacked(ulong left, ulong right)
    {
        // 32 trits span at most ~50.7 bits, so the product of two in-range values
        // always fits in a checked 64-bit multiply on the binary host. On real
        // ternary hardware this is one MUL instruction; here we take the fastest
        // faithful route (fast limb-table conversions + native multiply) and let
        // FromInt64 raise OverflowException when the product exceeds 32 trits.
        long product = checked(BalancedTernaryEncoding.ToInt64(left, TritCount) *
                               BalancedTernaryEncoding.ToInt64(right, TritCount));
        return BalancedTernaryEncoding.FromInt64(product, TritCount);
    }

    private static (ulong Quotient, ulong Remainder) DivRemPacked(ulong dividend, ulong divisor)
    {
        if (BalancedTernaryEncoding.IsZero(divisor, TritCount))
            throw new DivideByZeroException("Attempted to divide by zero IntT.");

        long a = BalancedTernaryEncoding.ToInt64(dividend, TritCount);
        long b = BalancedTernaryEncoding.ToInt64(divisor, TritCount);
        long q = a / b;   // truncated division; remainder sign follows the dividend,
        long r = a - q * b; // matching the previous shift-and-subtract implementation.

        return (BalancedTernaryEncoding.FromInt64(q, TritCount),
                BalancedTernaryEncoding.FromInt64(r, TritCount));
    }

    private static long ComputeMaxMagnitude(int count)
    {
        long value = 1;
        for (int i = 0; i < count; i++)
        {
            value = checked(value * 3);
        }
        return (value - 1) / 2;
    }
}
