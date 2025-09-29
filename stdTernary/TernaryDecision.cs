using System;

namespace stdTernary;

public sealed class TernaryDecision
{
    private readonly Trit _result;
    private bool _handled;

    private TernaryDecision(Trit result)
    {
        _result = result;
    }

    public static TernaryDecision From(Trit result) => new(result);

    public static TernaryDecision Compare<T>(T left, T right) where T : IComparable<T>
        => new(Trit.FromComparison(left.CompareTo(right)));

    public TernaryDecision Positive(Action action)
    {
        if (!_handled && _result.Value == TritVal.p)
        {
            action();
            _handled = true;
        }
        return this;
    }

    public TernaryDecision Zero(Action action)
    {
        if (!_handled && _result.Value == TritVal.z)
        {
            action();
            _handled = true;
        }
        return this;
    }

    public TernaryDecision Negative(Action action)
    {
        if (!_handled && _result.Value == TritVal.n)
        {
            action();
            _handled = true;
        }
        return this;
    }

    public TernaryDecision NonPositive(Action action)
    {
        if (!_handled && _result.Value != TritVal.p)
        {
            action();
            _handled = true;
        }
        return this;
    }

    public TernaryDecision NonNegative(Action action)
    {
        if (!_handled && _result.Value != TritVal.n)
        {
            action();
            _handled = true;
        }
        return this;
    }

    public void Else(Action action)
    {
        if (!_handled)
        {
            action();
            _handled = true;
        }
    }

    public T Choose<T>(Func<T> positive, Func<T> zero, Func<T> negative)
    {
        return _result.Value switch
        {
            TritVal.p => positive(),
            TritVal.z => zero(),
            _ => negative(),
        };
    }

    public T Switch<T>(T positive, T zero, T negative)
    {
        return _result.Value switch
        {
            TritVal.p => positive,
            TritVal.z => zero,
            _ => negative,
        };
    }

    public Trit Result => _result;
}
