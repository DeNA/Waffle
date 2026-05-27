// (c) DeNA Co., Ltd.

using System.Collections.Generic;

// Suppress CS0660/CS0661: the compiler requires IEquatable when overloading == and !=.
// These overloads are not for object equality comparisons, so the warning is intentionally suppressed.
#pragma warning disable CS0660, CS0661

namespace Waffle.Interpreter;

/// <summary>
/// Wrapper class that provides operator overloads for proxies that resolve to an integer value.
/// </summary>
public class IntProxy(IResolvableTo<int> value) : IResolvableTo<int>, IBlockContent
{
    private readonly IResolvableTo<int> _value = value;

    /// <inheritdoc/>
    public int Resolve(Dictionary<int, EnvValue> env)
    {
        return _value.Resolve(env);
    }

    /// <inheritdoc/>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        return EvalResult.Create(_value.Resolve(env), alignment, format);
    }

    /// <summary>
    /// Adds two <see cref="IntProxy"/> values.
    /// </summary>
    public static IntProxy operator +(IntProxy left, IntProxy right)
    {
        return new IntProxy(left._value.With(right._value, (l, r) => l + r));
    }

    /// <summary>
    /// Subtracts one <see cref="IntProxy"/> value from another.
    /// </summary>
    public static IntProxy operator -(IntProxy left, IntProxy right)
    {
        return new IntProxy(left._value.With(right._value, (l, r) => l - r));
    }

    /// <summary>
    /// Multiplies two <see cref="IntProxy"/> values.
    /// </summary>
    public static IntProxy operator *(IntProxy left, IntProxy right)
    {
        return new IntProxy(left._value.With(right._value, (l, r) => l * r));
    }

    /// <summary>
    /// Divides one <see cref="IntProxy"/> value by another.
    /// </summary>
    public static IntProxy operator /(IntProxy left, IntProxy right)
    {
        return new IntProxy(left._value.With(right._value, (l, r) => l / r));
    }

    /// <summary>
    /// Computes the remainder of one <see cref="IntProxy"/> value divided by another.
    /// </summary>
    public static IntProxy operator %(IntProxy left, IntProxy right)
    {
        return new IntProxy(left._value.With(right._value, (l, r) => l % r));
    }

    /// <summary>
    /// Determines whether one <see cref="IntProxy"/> value is less than another.
    /// </summary>
    public static BoolProxy operator <(IntProxy left, IntProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l < r));
    }

    /// <summary>
    /// Determines whether one <see cref="IntProxy"/> value is greater than another.
    /// </summary>
    public static BoolProxy operator >(IntProxy left, IntProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l > r));
    }

    /// <summary>
    /// Determines whether one <see cref="IntProxy"/> value is less than or equal to another.
    /// </summary>
    public static BoolProxy operator <=(IntProxy left, IntProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l <= r));
    }

    /// <summary>
    /// Determines whether one <see cref="IntProxy"/> value is greater than or equal to another.
    /// </summary>
    public static BoolProxy operator >=(IntProxy left, IntProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l >= r));
    }

    /// <summary>
    /// Compares two <see cref="IntProxy"/> values for equality.
    /// </summary>
    public static BoolProxy operator ==(IntProxy left, IntProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l == r));
    }

    /// <summary>
    /// Compares two <see cref="IntProxy"/> values for inequality.
    /// </summary>
    public static BoolProxy operator !=(IntProxy left, IntProxy right)
    {
        return new BoolProxy(left._value.With(right._value, (l, r) => l != r));
    }

    /// <summary>
    /// Adds an <see cref="int"/> value to an <see cref="IntProxy"/> value.
    /// </summary>
    public static IntProxy operator +(IntProxy left, int right)
    {
        return new IntProxy(left._value.To(right, (l, r) => l + r));
    }

    /// <summary>
    /// Subtracts an <see cref="int"/> value from an <see cref="IntProxy"/> value.
    /// </summary>
    public static IntProxy operator -(IntProxy left, int right)
    {
        return new IntProxy(left._value.To(right, (l, r) => l - r));
    }

    /// <summary>
    /// Multiplies an <see cref="IntProxy"/> value by an <see cref="int"/> value.
    /// </summary>
    public static IntProxy operator *(IntProxy left, int right)
    {
        return new IntProxy(left._value.To(right, (l, r) => l * r));
    }

    /// <summary>
    /// Divides an <see cref="IntProxy"/> value by an <see cref="int"/> value.
    /// </summary>
    public static IntProxy operator /(IntProxy left, int right)
    {
        return new IntProxy(left._value.To(right, (l, r) => l / r));
    }

    /// <summary>
    /// Computes the remainder of an <see cref="IntProxy"/> value divided by an <see cref="int"/> value.
    /// </summary>
    public static IntProxy operator %(IntProxy left, int right)
    {
        return new IntProxy(left._value.To(right, (l, r) => l % r));
    }

    /// <summary>
    /// Determines whether an <see cref="IntProxy"/> value is less than an <see cref="int"/> value.
    /// </summary>
    public static BoolProxy operator <(IntProxy left, int right)
    {
        return new BoolProxy(left._value.To(right, (l, r) => l < r));
    }

    /// <summary>
    /// Determines whether an <see cref="IntProxy"/> value is greater than an <see cref="int"/> value.
    /// </summary>
    public static BoolProxy operator >(IntProxy left, int right)
    {
        return new BoolProxy(left._value.To(right, (l, r) => l > r));
    }

    /// <summary>
    /// Determines whether an <see cref="IntProxy"/> value is less than or equal to an <see cref="int"/> value.
    /// </summary>
    public static BoolProxy operator <=(IntProxy left, int right)
    {
        return new BoolProxy(left._value.To(right, (l, r) => l <= r));
    }

    /// <summary>
    /// Determines whether an <see cref="IntProxy"/> value is greater than or equal to an <see cref="int"/> value.
    /// </summary>
    public static BoolProxy operator >=(IntProxy left, int right)
    {
        return new BoolProxy(left._value.To(right, (l, r) => l >= r));
    }

    /// <summary>
    /// Compares an <see cref="IntProxy"/> value and an <see cref="int"/> value for equality.
    /// </summary>
    public static BoolProxy operator ==(IntProxy left, int right)
    {
        return new BoolProxy(left._value.To(right, (l, r) => l == r));
    }

    /// <summary>
    /// Compares an <see cref="IntProxy"/> value and an <see cref="int"/> value for inequality.
    /// </summary>
    public static BoolProxy operator !=(IntProxy left, int right)
    {
        return new BoolProxy(left._value.To(right, (l, r) => l != r));
    }

    /// <summary>
    /// Adds an <see cref="int"/> value to an <see cref="IntProxy"/> value.
    /// </summary>
    public static IntProxy operator +(int left, IntProxy right)
    {
        return new IntProxy(right._value.To(left, (r, l) => l + r));
    }

    /// <summary>
    /// Subtracts an <see cref="IntProxy"/> value from an <see cref="int"/> value.
    /// </summary>
    public static IntProxy operator -(int left, IntProxy right)
    {
        return new IntProxy(right._value.To(left, (r, l) => l - r));
    }

    /// <summary>
    /// Multiplies an <see cref="int"/> value by an <see cref="IntProxy"/> value.
    /// </summary>
    public static IntProxy operator *(int left, IntProxy right)
    {
        return new IntProxy(right._value.To(left, (r, l) => l * r));
    }

    /// <summary>
    /// Divides an <see cref="int"/> value by an <see cref="IntProxy"/> value.
    /// </summary>
    public static IntProxy operator /(int left, IntProxy right)
    {
        return new IntProxy(right._value.To(left, (r, l) => l / r));
    }

    /// <summary>
    /// Computes the remainder of an <see cref="int"/> value divided by an <see cref="IntProxy"/> value.
    /// </summary>
    public static IntProxy operator %(int left, IntProxy right)
    {
        return new IntProxy(right._value.To(left, (r, l) => l % r));
    }

    /// <summary>
    /// Determines whether an <see cref="int"/> value is less than an <see cref="IntProxy"/> value.
    /// </summary>
    public static BoolProxy operator <(int left, IntProxy right)
    {
        return new BoolProxy(right._value.To(left, (r, l) => l < r));
    }

    /// <summary>
    /// Determines whether an <see cref="int"/> value is greater than an <see cref="IntProxy"/> value.
    /// </summary>
    public static BoolProxy operator >(int left, IntProxy right)
    {
        return new BoolProxy(right._value.To(left, (r, l) => l > r));
    }

    /// <summary>
    /// Determines whether an <see cref="int"/> value is less than or equal to an <see cref="IntProxy"/> value.
    /// </summary>
    public static BoolProxy operator <=(int left, IntProxy right)
    {
        return new BoolProxy(right._value.To(left, (r, l) => l <= r));
    }

    /// <summary>
    /// Determines whether an <see cref="int"/> value is greater than or equal to an <see cref="IntProxy"/> value.
    /// </summary>
    public static BoolProxy operator >=(int left, IntProxy right)
    {
        return new BoolProxy(right._value.To(left, (r, l) => l >= r));
    }

    /// <summary>
    /// Compares an <see cref="int"/> value and an <see cref="IntProxy"/> value for equality.
    /// </summary>
    public static BoolProxy operator ==(int left, IntProxy right)
    {
        return new BoolProxy(right._value.To(left, (r, l) => l == r));
    }

    /// <summary>
    /// Compares an <see cref="int"/> value and an <see cref="IntProxy"/> value for inequality.
    /// </summary>
    public static BoolProxy operator !=(int left, IntProxy right)
    {
        return new BoolProxy(right._value.To(left, (r, l) => l != r));
    }

    /// <summary>
    /// Increments an <see cref="IntProxy"/> value.
    /// </summary>
    public static IntProxy operator ++(IntProxy self)
    {
        return new IntProxy(self._value.To(v => ++v));
    }

    /// <summary>
    /// Decrements an <see cref="IntProxy"/> value.
    /// </summary>
    public static IntProxy operator --(IntProxy self)
    {
        return new IntProxy(self._value.To(v => --v));
    }

    /// <summary>
    /// Returns the unary plus of an <see cref="IntProxy"/> value.
    /// </summary>
    public static IntProxy operator +(IntProxy self)
    {
        return new IntProxy(self._value.To(v => +v));
    }

    /// <summary>
    /// Returns the arithmetic negation of an <see cref="IntProxy"/> value.
    /// </summary>
    public static IntProxy operator -(IntProxy self)
    {
        return new IntProxy(self._value.To(v => -v));
    }

    /// <summary>
    /// Converts the proxy to a lazily resolved string proxy.
    /// </summary>
    public new StringProxy ToString()
    {
        return new StringProxy(_value.To(it => it.ToString()));
    }
}
