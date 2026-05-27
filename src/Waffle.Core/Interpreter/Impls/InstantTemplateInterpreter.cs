// (c) DeNA Co., Ltd.

using System.Runtime.CompilerServices;

namespace Waffle.Interpreter;

/// <summary>
/// <see cref="InterpolatedStringHandlerAttribute"/> implementation for cases where you only need the
/// resulting string directly, without injecting an <see cref="ITemplateInterpreterContext"/> implementation.
/// </summary>
/// <remarks>
/// Internally wraps <see cref="TemplateInterpreter"/> with an <see cref="InstantTemplateContext"/>.
/// </remarks>
[InterpolatedStringHandler]
public readonly struct InstantTemplateInterpreter
{
    private readonly TemplateInterpreter _interpreter;
    private readonly InstantTemplateContext _ctx;

    /// <summary>
    /// Initializes a new interpreter for an interpolated template string.
    /// </summary>
    public InstantTemplateInterpreter(int literalLength, int formattedCount)
    {
        _ctx = new InstantTemplateContext();
        _interpreter = new TemplateInterpreter(literalLength, formattedCount, _ctx);
    }

    ///<inheritdoc cref="TemplateInterpreter.AppendLiteral"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLiteral(string s)
    {
        _interpreter.AppendLiteral(s);
    }

    ///<inheritdoc cref="TemplateInterpreter.AppendFormatted"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted<T>(T x, int alignment = 0, string? format = null)
    {
        _interpreter.AppendFormatted(x, alignment, format);
    }

    /// <summary>
    /// Returns the generated output string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetResult()
    {
        return _ctx.GetResult();
    }
}
