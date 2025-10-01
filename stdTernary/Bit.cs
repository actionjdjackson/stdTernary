using System;

namespace stdTernary;

public struct Bit : IEquatable<Bit>, IComparable<Bit>
{
    private sbyte _value;

    public static Bit Zero => new Bit(0);
    public static Bit One => new Bit(1);

    public Bit(int value)
    {
        if (value is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(value), "Bit must be 0 or 1.");
        _value = (sbyte)value;
    }

    public Bit(sbyte value) : this((int)value) { }

    public Bit(char value)
    {
        _value = value switch
        {
            '0' => 0,
            '1' => 1,
            _ => throw new ArgumentException("Bit characters must be '0' or '1'.", nameof(value)),
        };
    }

    public sbyte Value
    {
        readonly get => _value;
        set
        {
            if (value is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Bit must be 0 or 1.");
            _value = value;
        }
    }

    public char AsChar => _value == 1 ? '1' : '0';

    public bool AsBoolean => _value == 1;

    public Bit NOT() => new Bit((sbyte)(1 - _value));

    public Bit AND(Bit other) => _value == 1 && other._value == 1 ? One : Zero;

    public Bit OR(Bit other) => _value == 1 || other._value == 1 ? One : Zero;

    public Bit XOR(Bit other) => _value == other._value ? Zero : One;

    public Bit NAND(Bit other) => AND(other).NOT();

    public Bit NOR(Bit other) => OR(other).NOT();

    public Bit XNOR(Bit other) => XOR(other).NOT();

    public Bit Then(Action action)
    {
        if (_value == 1)
            action();
        return this;
    }

    public Bit Else(Action action)
    {
        if (_value == 0)
            action();
        return this;
    }

    public bool Equals(Bit other) => _value == other._value;

    public override bool Equals(object? obj) => obj is Bit other && Equals(other);

    public override int GetHashCode() => _value;

    public int CompareTo(Bit other) => _value.CompareTo(other._value);

    public override string ToString() => AsChar.ToString();

    public static Bit operator ~(Bit value) => value.NOT();
    public static Bit operator &(Bit left, Bit right) => left.AND(right);
    public static Bit operator |(Bit left, Bit right) => left.OR(right);
    public static Bit operator ^(Bit left, Bit right) => left.XOR(right);

    public static implicit operator Bit(int value) => new Bit(value);
    public static implicit operator Bit(bool value) => new Bit(value ? 1 : 0);
    public static implicit operator bool(Bit value) => value.AsBoolean;
}
