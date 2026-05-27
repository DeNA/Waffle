// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

// Suppress CS0660/CS0661: the compiler requires IEquatable when overloading == and !=.
// These overloads are not for object equality comparisons, so the warning is intentionally suppressed.
#pragma warning disable CS0660, CS0661

/// <summary>
/// Wrapper class that provides operator overloads for proxies that resolve to a string value.
/// </summary>
public class StringProxy(IResolvableTo<string> value) : IResolvableTo<string>, IBlockContent
{
    private readonly IResolvableTo<string> _value = value;

    /// <inheritdoc/>
    public string Resolve(Dictionary<int, EnvValue> env)
    {
        return _value.Resolve(env);
    }

    /// <inheritdoc/>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        return EvalResult.Create(_value.Resolve(env), alignment, format);
    }

    /// <summary>
    /// Concatenates two <see cref="StringProxy"/> values.
    /// </summary>
    public static StringProxy operator +(StringProxy left, StringProxy right)
    {
        return new StringProxy(left._value.With(right._value, (l, r) => l + r));
    }

    /// <summary>
    /// Concatenates a <see cref="StringProxy"/> value and a <see cref="string"/> value.
    /// </summary>
    public static StringProxy operator +(StringProxy left, string right)
    {
        return new StringProxy(left._value.To(right, (l, r) => l + r));
    }

    /// <summary>
    /// Concatenates a <see cref="string"/> value and a <see cref="StringProxy"/> value.
    /// </summary>
    public static StringProxy operator +(string left, StringProxy right)
    {
        return new StringProxy(right._value.To(left, (r, l) => l + r));
    }

    /// <summary>
    /// Compares two <see cref="StringProxy"/> values for equality.
    /// </summary>
    public static BoolProxy operator ==(StringProxy left, StringProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l == r));
    }

    /// <summary>
    /// Compares two <see cref="StringProxy"/> values for inequality.
    /// </summary>
    public static BoolProxy operator !=(StringProxy left, StringProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l != r));
    }

    /// <summary>
    /// Compares a <see cref="StringProxy"/> value and a <see cref="string"/> value for equality.
    /// </summary>
    public static BoolProxy operator ==(StringProxy left, string right)
    {
        return new BoolProxy(left._value.To(right, (l, r) => l == r));
    }

    /// <summary>
    /// Compares a <see cref="StringProxy"/> value and a <see cref="string"/> value for inequality.
    /// </summary>
    public static BoolProxy operator !=(StringProxy left, string right)
    {
        return new BoolProxy(left._value.To(right, (l, r) => l != r));
    }

    /// <summary>
    /// Compares a <see cref="string"/> value and a <see cref="StringProxy"/> value for equality.
    /// </summary>
    public static BoolProxy operator ==(string left, StringProxy right)
    {
        return new BoolProxy(right._value.To(left, (r, l) => l == r));
    }

    /// <summary>
    /// Compares a <see cref="string"/> value and a <see cref="StringProxy"/> value for inequality.
    /// </summary>
    public static BoolProxy operator !=(string left, StringProxy right)
    {
        return new BoolProxy(right._value.To(left, (r, l) => l != r));
    }
}
