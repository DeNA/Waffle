// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Base class for proxies that hold a concrete value known at template-build time.
/// Implements <see cref="IResolvableTo{T}"/> and <see cref="IBlockContent"/>; the
/// <see cref="Resolve"/> method simply returns the stored value unchanged.
/// </summary>
/// <typeparam name="T">The type of the concrete value held by this proxy.</typeparam>
public class LiteralProxy<T>(T value) : IResolvableTo<T>, IBlockContent
{
    /// <summary>
    /// Gets the literal value stored by this proxy.
    /// </summary>
    protected readonly T Value = value;

    /// <summary>
    /// Returns the concrete value stored in this proxy, ignoring the environment.
    /// </summary>
    /// <param name="env">The environment dictionary (unused for literal proxies).</param>
    /// <returns>The stored value.</returns>
    public T Resolve(Dictionary<int, EnvValue> env)
    {
        return Value;
    }

    /// <summary>
    /// Resolves the stored value and formats it as a string.
    /// </summary>
    /// <param name="env">The environment dictionary (unused for literal proxies).</param>
    /// <param name="alignment">The alignment specifier for string formatting.</param>
    /// <param name="format">The format string.</param>
    /// <returns>A <see cref="EvalResult"/> representing the formatted value.</returns>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        return EvalResult.Create(Value, alignment, format);
    }
}
