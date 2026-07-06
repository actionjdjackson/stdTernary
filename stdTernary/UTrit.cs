using System;

namespace stdTernary;

public enum UTritVal : byte
{
    z = 0,
    o = 1,
    t = 2,
}

public struct UTrit : IEquatable<UTrit>, IComparable<UTrit>
{
    private byte _value;

    public UTritVal Value
    {
        readonly get => (UTritVal)_value;
        set => SetValue(value);
    }

    public char GetChar => (char)('0' + _value);

    public UTrit(byte value)
    {
        if (value > 2)
            throw new ArgumentOutOfRangeException(nameof(value), "UTrit values must be 0, 1, or 2.");
        _value = value;
    }

    public UTrit(int value) : this((byte)value) { }

    public UTrit(UTritVal value) : this((byte)value) { }

    public UTrit(Trit value)
    {
        int balanced = value;
        if (balanced < 0)
            throw new OverflowException("Negative Trit values cannot be represented as UTrit.");
        _value = (byte)balanced;
    }

    public UTrit(char value)
    {
        _value = value switch
        {
            '0' => 0,
            '1' => 1,
            '2' => 2,
            _ => throw new ArgumentException("Unbalanced ternary characters must be '0', '1', or '2'.", nameof(value)),
        };
    }

    public void SetValue(UTritVal value) => _value = (byte)value;

    public void SetValue(byte value)
    {
        if (value > 2)
            throw new ArgumentOutOfRangeException(nameof(value), "UTrit values must be 0, 1, or 2.");
        _value = value;
    }

    public void SetValue(int value) => SetValue((byte)value);

    public void SetValue(char value) => _value = new UTrit(value)._value;

    public UTrit NEG() => new UTrit(2 - _value);

    public UTrit SUM(UTrit other) => new UTrit((_value + other._value) % 3);

    public UTrit SUB(UTrit other) => new UTrit((_value - other._value + 3) % 3);

    public UTrit MULT(UTrit other) => new UTrit((_value * other._value) % 3);

    public UTrit DIV(UTrit other)
    {
        if (other._value == 0)
            throw new DivideByZeroException("Attempted to divide by zero UTrit.");

        for (int candidate = 0; candidate <= 2; candidate++)
        {
            if ((other._value * candidate) % 3 == _value)
                return new UTrit(candidate);
        }

        throw new InvalidOperationException("No unbalanced trit quotient exists.");
    }

    public UTrit MIN(UTrit other) => new UTrit(Math.Min(_value, other._value));

    public UTrit AND(UTrit other) => MIN(other);

    public UTrit MAX(UTrit other) => new UTrit(Math.Max(_value, other._value));

    public UTrit OR(UTrit other) => MAX(other);

    public UTrit XOR(UTrit other) => SUM(other);

    public UTrit XNOR(UTrit other) => new UTrit(2 - ((_value + other._value) % 3));

    public UTrit IMP(UTrit other) => NEG().MAX(other);

    public UTrit EQUAL(UTrit other) => _value == other._value ? new UTrit(2) : new UTrit(0);

    public UTrit NOTEQUAL(UTrit other) => _value != other._value ? new UTrit(2) : new UTrit(0);

    public static Trit COMPARET(UTrit a, UTrit b) => Trit.FromComparison(a.CompareTo(b));

    public static Trit COMPARET(UTrit a, Trit b) => Trit.FromComparison(a._value.CompareTo((int)b));

    public static Trit COMPARET(Trit a, UTrit b) => Trit.FromComparison(((int)a).CompareTo(b._value));

    public Trit ToBalancedTrit()
    {
        if (_value == 2)
            throw new OverflowException("UTrit value 2 cannot fit in one balanced Trit.");
        return new Trit(_value);
    }

    public bool Equals(UTrit other) => _value == other._value;

    public override bool Equals(object? obj) => obj is UTrit other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public int CompareTo(UTrit other) => _value.CompareTo(other._value);

    public override string ToString() => GetChar.ToString();

    public static UTrit operator &(UTrit left, UTrit right) => left.AND(right);
    public static UTrit operator |(UTrit left, UTrit right) => left.OR(right);
    public static UTrit operator ^(UTrit left, UTrit right) => left.XOR(right);
    public static UTrit operator ~(UTrit value) => value.NEG();
    public static UTrit operator !(UTrit value) => value.NEG();
    public static UTrit operator *(UTrit left, UTrit right) => left.MULT(right);
    public static UTrit operator +(UTrit left, UTrit right) => left.SUM(right);
    public static UTrit operator -(UTrit left, UTrit right) => left.SUB(right);
    public static UTrit operator /(UTrit left, UTrit right) => left.DIV(right);
    public static bool operator ==(UTrit left, UTrit right) => left.Equals(right);
    public static bool operator !=(UTrit left, UTrit right) => !left.Equals(right);
    public static bool operator >(UTrit left, UTrit right) => left._value > right._value;
    public static bool operator <(UTrit left, UTrit right) => left._value < right._value;
    public static bool operator >=(UTrit left, UTrit right) => left._value >= right._value;
    public static bool operator <=(UTrit left, UTrit right) => left._value <= right._value;
    public static bool operator >(UTrit left, Trit right) => left._value > (int)right;
    public static bool operator <(UTrit left, Trit right) => left._value < (int)right;
    public static bool operator >=(UTrit left, Trit right) => left._value >= (int)right;
    public static bool operator <=(UTrit left, Trit right) => left._value <= (int)right;
    public static bool operator true(UTrit value) => value._value > 0;
    public static bool operator false(UTrit value) => value._value == 0;

    public static explicit operator bool(UTrit value) => value._value > 0;
    public static explicit operator UTrit(bool value) => value ? new UTrit(1) : new UTrit(0);
    public static implicit operator byte(UTrit value) => value._value;
    public static implicit operator UTrit(byte value) => new UTrit(value);
    public static implicit operator int(UTrit value) => value._value;
    public static implicit operator UTrit(int value) => new UTrit(value);
    public static explicit operator Trit(UTrit value) => value.ToBalancedTrit();
    public static explicit operator UTrit(Trit value) => new UTrit(value);
}
