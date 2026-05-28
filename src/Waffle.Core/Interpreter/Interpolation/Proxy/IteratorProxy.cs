// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Waffle.Interpreter;

/// <summary>
/// Iterator object used in For and ForEach blocks.
/// </summary>
public class IteratorProxy<T>(int parentId) : IBlockContent, IResolvableTo<T>
{
    /// <summary>
    /// Corresponding parent block's ID.
    /// </summary>
    private readonly int _parentId = parentId;

    /// <inheritdoc/>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        // The actual iterator value is stored under the corresponding block's ID in the environment.
        var it = env.GetLoopVariable(_parentId).AsObject();

        // null is a valid value for reference types and Nullable<T> value types; render as empty string.
        if (it is null)
        {
            if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) is null)
            {
                throw new InvalidCastException($"Expected non-null value type {typeof(T)}, but got null");
            }

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
    public T Resolve(Dictionary<int, EnvValue> env)
    {
        var it = env.GetLoopVariable(_parentId).AsObject();

        if (it is null)
        {
            if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) is null)
            {
                // Non-nullable value type: null is never valid.
                throw new InvalidCastException($"Expected non-null value type {typeof(T)}, but got null");
            }

            if (!typeof(T).IsValueType)
            {
                // Reference type: null means the source collection contains null elements.
                // Use ForEachNullable<T> to iterate over nullable-element collections.
                throw new InvalidCastException(
                    $"Null element encountered for reference type {typeof(T).Name}. " +
                    $"Use {nameof(WaffleSyntax.ForEachNullable)} to iterate over nullable-element collections.");
            }

            // Nullable<T> value type: default(Nullable<T>) == null, which is a valid value.
            return default!;
        }

        return (T)it;
    }
}
