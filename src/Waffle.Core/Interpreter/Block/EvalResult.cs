// (c) DeNA Co., Ltd.

using System.Runtime.CompilerServices;

namespace Waffle.Interpreter;

/// <summary>
/// Intermediate result produced while stringifying and concatenating each element after the
/// template has been parsed into a syntax tree. Each <see cref="IBlockContent"/> is converted
/// to this on its evaluation.
/// </summary>
public readonly struct EvalResult
{
    /// <summary>
    /// The string value to append to the context.
    /// </summary>
    public readonly string Value;

    private EvalResult(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates an evaluation result from an already formatted string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvalResult Create(string value)
    {
        return new EvalResult(value);
    }

    /// <summary>
    /// Creates an evaluation result by formatting the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvalResult Create<T>(T value, int alignment, string? format)
    {
        return new EvalResult(TemplateInterpreterHelper.FormatByDefault(value, alignment, format));
    }
}
