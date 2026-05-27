// (c) DeNA Co., Ltd.

using System.Collections.Generic;

// Suppress CS0660/CS0661: the compiler requires IEquatable when overloading == and !=.
// These overloads are not for object equality comparisons, so the warning is intentionally suppressed.
#pragma warning disable CS0660, CS0661

namespace Waffle.Interpreter;

/// <summary>
/// Wrapper class that provides operator overloads for proxies that resolve to a boolean value.
/// </summary>
public class BoolProxy : IResolvableTo<bool>, IBlockContent
{
    private readonly IResolvableTo<bool> _value;

    /// <summary>
    /// Initializes a proxy from a concrete boolean value.
    /// </summary>
    public BoolProxy(bool value)
    {
        _value = new LiteralProxy<bool>(value);
    }

    /// <summary>
    /// Initializes a proxy from a lazily resolved boolean value.
    /// </summary>
    public BoolProxy(IResolvableTo<bool> value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    public bool Resolve(Dictionary<int, EnvValue> env)
    {
        return _value.Resolve(env);
    }

    /// <inheritdoc/>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        return EvalResult.Create(_value.Resolve(env), alignment, format);
    }

    /// <summary>
    /// Negates a <see cref="BoolProxy"/> value.
    /// </summary>
    public static BoolProxy operator !(BoolProxy value)
    {
        return new BoolProxy(value._value.To(it => !it));
    }

    /// <summary>
    /// Applies logical OR to two <see cref="BoolProxy"/> values.
    /// </summary>
    public static BoolProxy operator |(BoolProxy left, BoolProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l | r));
    }

    /// <summary>
    /// Applies logical OR to a <see cref="bool"/> value and a <see cref="BoolProxy"/> value.
    /// </summary>
    public static BoolProxy operator |(bool left, BoolProxy right)
    {
        return new BoolProxy(right._value.To(left, (r, l) => l | r));
    }

    /// <summary>
    /// Applies logical OR to a <see cref="BoolProxy"/> value and a <see cref="bool"/> value.
    /// </summary>
    public static BoolProxy operator |(BoolProxy left, bool right)
    {
        return new BoolProxy(left._value.To(right, (l, r) => l | r));
    }

    /// <summary>
    /// Applies logical AND to two <see cref="BoolProxy"/> values.
    /// </summary>
    public static BoolProxy operator &(BoolProxy left, BoolProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l & r));
    }

    /// <summary>
    /// Applies logical AND to a <see cref="bool"/> value and a <see cref="BoolProxy"/> value.
    /// </summary>
    public static BoolProxy operator &(bool left, BoolProxy right)
    {
        return new BoolProxy(right._value.To(left, (r, l) => l & r));
    }

    /// <summary>
    /// Applies logical AND to a <see cref="BoolProxy"/> value and a <see cref="bool"/> value.
    /// </summary>
    public static BoolProxy operator &(BoolProxy left, bool right)
    {
        return new BoolProxy(left._value.To(right, (l, r) => l & r));
    }

    /// <summary>
    /// Compares two <see cref="BoolProxy"/> values for equality.
    /// </summary>
    public static BoolProxy operator ==(BoolProxy left, BoolProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l == r));
    }

    /// <summary>
    /// Compares two <see cref="BoolProxy"/> values for inequality.
    /// </summary>
    public static BoolProxy operator !=(BoolProxy left, BoolProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l != r));
    }

    /// <summary>
    /// Converts the proxy to a lazily resolved string proxy.
    /// </summary>
    public new StringProxy ToString()
    {
        return new StringProxy(_value.To(it => it.ToString()));
    }
}
