// (c) DeNA Co., Ltd.

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
