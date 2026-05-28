// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Tagged-union value type for the template evaluation environment dictionary.
/// Stores either an <see cref="int"/> inline (avoiding heap allocation) or any reference type as <c>object?</c>.
/// </summary>
public readonly struct EnvValue
{
    private readonly byte _tag; // 0 = object (null or ref type), 1 = int
    private readonly int _int;
    private readonly object? _obj;

    private EnvValue(byte tag, int i, object? obj)
    {
        _tag = tag;
        _int = i;
        _obj = obj;
    }

    internal bool IsInt => _tag == 1;

    /// <summary>Creates an <see cref="EnvValue"/> wrapping a reference-typed or boxed value.</summary>
    internal static EnvValue FromObject(object? v) => new(0, 0, v);

    /// <summary>Creates an <see cref="EnvValue"/> wrapping an <see cref="int"/> without boxing.</summary>
    internal static EnvValue FromInt(int v) => new(1, v, null);

    /// <summary>Returns the stored <see cref="int"/> value. Only valid when <see cref="IsInt"/> is <c>true</c>.</summary>
    internal int AsInt() => _int;

    /// <summary>
    /// Returns the value as <c>object?</c>.
    /// If <see cref="IsInt"/> is <c>true</c>, boxes the stored <c>int</c>.
    /// Prefer <see cref="AsInt"/> when the value is known to be <c>int</c>.
    /// </summary>
    internal object? AsObject() => _tag == 1 ? _int : _obj;
}

internal static class EnvLookup
{
    /// <summary>
    /// Retrieves a loop-variable value from the evaluation environment, throwing a user-friendly
    /// exception when the key is absent. Absence indicates the loop variable was referenced after
    /// its corresponding <c>For</c>/<c>ForEach</c> block ended — which the C# compiler accepts but
    /// Waffle cannot satisfy at runtime.
    /// </summary>
    public static EnvValue GetLoopVariable(this Dictionary<int, EnvValue> env, int parentId)
    {
        if (!env.TryGetValue(parentId, out var v))
        {
            throw new InvalidOperationException(
                "Loop variable is accessed outside the scope of its For/ForEach block. " +
                "C# allows referencing a loop variable declared with `out var` after the `End` command, " +
                "but Waffle evaluates the template at runtime and the variable no longer exists at that point. " +
                "Move the reference inside the corresponding For/ForEach ... End block.");
        }
        return v;
    }
}
