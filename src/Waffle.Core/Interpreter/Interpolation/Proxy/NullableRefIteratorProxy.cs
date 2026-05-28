// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Waffle.Interpreter;

/// <summary>
/// Iterator proxy for ForEach loops over collections whose elements may be <see langword="null"/>.
/// Implements <see cref="IResolvableTo{T}"/> so that null elements are represented as <c>null</c>
/// rather than requiring a null-forgiving cast.
/// </summary>
/// <typeparam name="T">The non-nullable element type; the resolved type is <typeparamref name="T"/>?.</typeparam>
public class NullableRefIteratorProxy<T>(int parentId) : IBlockContent, IResolvableTo<T?>
    where T : class
{
    /// <inheritdoc/>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        var it = env.GetLoopVariable(parentId).AsObject();

        // null element: render as empty string.
        if (it is null)
        {
            return EvalResult.Create("", alignment, format);
        }

        if (it is not T casted)
        {
            throw new InvalidCastException($"Expected type {typeof(T)}, but got {it.GetType()}");
        }

        if (casted is string str)
        {
            return EvalResult.Create(str);
        }

        return EvalResult.Create(casted, alignment, format);
    }

    /// <inheritdoc/>
    public T? Resolve(Dictionary<int, EnvValue> env)
    {
        var it = env.GetLoopVariable(parentId).AsObject();
        return (T?)it;
    }
}
