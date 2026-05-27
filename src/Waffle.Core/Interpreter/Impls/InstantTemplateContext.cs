// (c) DeNA Co., Ltd.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Waffle.Interpreter;

/// <summary>
/// A simple <see cref="ITemplateInterpreterContext"/> implementation that accumulates generated output
/// into a <see cref="System.Text.StringBuilder"/> and throws <see cref="InvalidOperationException"/>
/// on template errors. Use this when you only need the final result string without a custom context.
/// </summary>
/// <remarks>
/// This context cannot be reused after the template interpreter signals completion,
/// as it releases the internal <see cref="StringBuilder"/> back to the pool at that point.
/// </remarks>
internal sealed class InstantTemplateContext : ITemplateInterpreterContext
{
    private StringBuilder? _builder;
    private StringBuilderPool.Releaser? _releaser;
    private string? _result;

    /// <inheritdoc cref="ITemplateInterpreterContext.OnHandlerCreated"/>
    /// <remarks>This implementation initializes a <see cref="StringBuilder"/> from the pool to accumulate the output.</remarks>
    public void OnHandlerCreated(int literalLength, int formattedCount, TemplateInterpreterController controller)
    {
        _releaser = StringBuilderPool.Get(out _builder);
    }

    /// <summary>
    /// Returns the latest generated output string accumulated so far.
    /// </summary>
    /// <returns>The full generated output.</returns>
    public string GetResult()
    {
        return _result ?? "";
    }

    /// <inheritdoc cref="ITemplateInterpreterContext.Append"/>
    public void Append(string value)
    {
        _builder?.Append(value);
    }

    /// <inheritdoc cref="ITemplateInterpreterContext.Error"/>
    public void Error(in TemplateError error)
    {
        throw new InvalidOperationException(error.Message, error.InnerException);
    }

    /// <inheritdoc cref="ITemplateInterpreterContext.OnPreAppendLiteral"/>
    /// <remarks>This implementation does nothing.</remarks>
    public void OnPreAppendLiteral(ref string willBeAppended, TemplateInterpreterController controller)
    {
    }

    /// <inheritdoc cref="ITemplateInterpreterContext.OnPostAppendLiteral"/>
    /// <remarks>This implementation does nothing.</remarks>
    public void OnPostAppendLiteral(string appended, TemplateInterpreterController controller)
    {
    }

    /// <inheritdoc cref="ITemplateInterpreterContext.OnPreAppendFormatted"/>
    /// <remarks>This implementation does nothing.</remarks>
    public void OnPreAppendFormatted<T>(
        ref T x, ref int alignment, ref string? format, TemplateInterpreterController controller)
    {
    }

    /// <inheritdoc cref="ITemplateInterpreterContext.TryHandleUnhandledInterpolation"/>
    /// <remarks>This implementation does nothing and always returns <c>false</c>.</remarks>
    public bool TryHandleUnhandledInterpolation<T>(
        T x, int alignment, string? format, TemplateInterpreterController controller)
    {
        return false;
    }

    /// <inheritdoc cref="ITemplateInterpreterContext.OnPostAppendFormatted"/>
    /// <remarks>This implementation does nothing.</remarks>
    public void OnPostAppendFormatted<T>(T x, TemplateInterpreterController controller)
    {
    }

    /// <inheritdoc cref="ITemplateInterpreterContext.OnPostAppendFormatted"/>
    /// <remarks>
    /// This implementation finalizes the result string and releases the <see cref="StringBuilder"/> back to the pool.
    /// </remarks>
    public void OnCompleted(TemplateInterpreterController controller)
    {
        _result = _builder?.ToString();
        _releaser?.Dispose();
        _builder = null;
        _releaser = null;
    }
}
