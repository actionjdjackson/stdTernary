using System;

namespace stdTernary;

public enum TritVal : sbyte
{
    n = -1,
    z = 0,
    p = 1,
}

public struct Trit : IEquatable<Trit>, IComparable<Trit>
{
    private sbyte _value;

    public TritVal Value
    {
        readonly get => (TritVal)_value;
        set => SetValue(value);
    }

    public char GetChar => _value switch
    {
        -1 => '-',
        1 => '+',
        _ => '0',
    };

    public Trit(sbyte value)
    {
        if (value is < -1 or > 1)
            throw new ArgumentOutOfRangeException(nameof(value), "Trit values must be -1, 0, or 1.");
        _value = value;
    }

    public Trit(int value) : this((sbyte)value) { }

    public Trit(TritVal value) : this((sbyte)value) { }

    public Trit(char value)
    {
        _value = value switch
        {
            '+' => 1,
            '-' => -1,
            '0' => 0,
            _ => throw new ArgumentException("Trit characters must be '+', '-', or '0'.", nameof(value)),
        };
    }

    public void SetValue(TritVal value) => _value = (sbyte)value;

    public void SetValue(sbyte value)
    {
        if (value is < -1 or > 1)
            throw new ArgumentOutOfRangeException(nameof(value), "Trit values must be -1, 0, or 1.");
        _value = value;
    }

    public void SetValue(int value) => SetValue((sbyte)value);

    public void SetValue(char value)
    {
        _value = value switch
        {
            '+' => 1,
            '-' => -1,
            '0' => 0,
            _ => throw new ArgumentException("Trit characters must be '+', '-', or '0'.", nameof(value)),
        };
    }

    public Trit NEG() => new Trit((sbyte)(-_value));

    public Trit XOR(Trit other)
    {
        if (_value == 0 || other._value == 0)
            return new Trit(0);
        return _value == other._value ? new Trit(-1) : new Trit(1);
    }

    public Trit SUM(Trit other)
    {
        int total = _value + other._value;
        return total switch
        {
            2 => new Trit(-1),
            -2 => new Trit(1),
            _ => new Trit((sbyte)total),
        };
    }

    public Trit CONS(Trit other) => _value == other._value ? new Trit(_value) : new Trit(0);

    public Trit XNOR(Trit other) => new Trit((sbyte)(_value * other._value));

    public Trit MULT(Trit other) => XNOR(other);

    public Trit MIN(Trit other)
    {
        if (_value == -1 || other._value == -1)
            return new Trit(-1);
        if (_value == 0 || other._value == 0)
            return new Trit(0);
        return new Trit(1);
    }

    public Trit AND(Trit other) => MIN(other);

    public Trit MAX(Trit other)
    {
        if (_value == 1 || other._value == 1)
            return new Trit(1);
        if (_value == 0 || other._value == 0)
            return new Trit(0);
        return new Trit(-1);
    }

    public Trit OR(Trit other) => MAX(other);

    public Trit EQUAL(Trit other) => _value == other._value ? new Trit(1) : new Trit(-1);

    public Trit NOTEQUAL(Trit other) => _value != other._value ? new Trit(1) : new Trit(-1);

    public Trit LargerOrEqual(Action action)
    {
        if (_value >= 0)
            action();
        return this;
    }

    public Trit SmallerOrEqual(Action action)
    {
        if (_value <= 0)
            action();
        return this;
    }

    public Trit Larger(Action action)
    {
        if (_value > 0)
            action();
        return this;
    }

    public Trit Smaller(Action action)
    {
        if (_value < 0)
            action();
        return this;
    }

    public Trit Equal(Action action)
    {
        if (_value == 0)
            action();
        return this;
    }

    public Trit Else(Action action)
    {
        action();
        return this;
    }

    public static Trit COMPARET(Trit a, Trit b)
    {
        if (a._value < b._value)
            return new Trit(-1);
        if (a._value > b._value)
            return new Trit(1);
        return new Trit(0);
    }

    public bool Equals(Trit other) => _value == other._value;

    public override bool Equals(object? obj) => obj is Trit other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public int CompareTo(Trit other) => _value.CompareTo(other._value);

    public override string ToString() => GetChar.ToString();

    public static Trit operator &(Trit left, Trit right) => left.AND(right);
    public static Trit operator |(Trit left, Trit right) => left.OR(right);
    public static Trit operator ^(Trit left, Trit right) => left.XOR(right);
    public static Trit operator ~(Trit value) => value.NEG();
    public static Trit operator !(Trit value) => value.NEG();
    public static Trit operator *(Trit left, Trit right) => left.MULT(right);
    public static Trit operator +(Trit left, Trit right) => left.SUM(right);
    public static bool operator ==(Trit left, Trit right) => left.Equals(right);
    public static bool operator !=(Trit left, Trit right) => !left.Equals(right);
    public static bool operator >(Trit left, Trit right) => left._value > right._value;
    public static bool operator <(Trit left, Trit right) => left._value < right._value;
    public static bool operator >=(Trit left, Trit right) => left._value >= right._value;
    public static bool operator <=(Trit left, Trit right) => left._value <= right._value;
    public static bool operator true(Trit value) => value._value > 0;
    public static bool operator false(Trit value) => value._value <= 0;

    public static explicit operator bool(Trit value) => value._value > 0;
    public static explicit operator Trit(bool value) => value ? new Trit(1) : new Trit(-1);
    public static implicit operator sbyte(Trit value) => value._value;
    public static implicit operator Trit(sbyte value) => new Trit(value);
    public static implicit operator int(Trit value) => value._value;
    public static implicit operator Trit(int value) => new Trit(value);
}
