// (c) DeNA Co., Ltd.

using System.Runtime.CompilerServices;
using System.Threading;

namespace Waffle.Interpreter;

/// <summary>
/// Execution context for <see cref="TemplateInterpreter"/>.
/// </summary>
public interface ITemplateInterpreterContext
{
    /// <summary>
    /// Hook called immediately after the interpolated string handler is created, before any segments are appended.
    /// </summary>
    /// <param name="literalLength">The total length of literal segments in the interpolated string.</param>
    /// <param name="formattedCount">The total number of interpolated segments in the interpolated string.</param>
    /// <param name="controller">The controller that manages the template interpretation process.</param>
    void OnHandlerCreated(int literalLength, int formattedCount, TemplateInterpreterController controller);

    /// <summary>
    /// Appends <paramref name="value"/> to the internal result buffer.
    /// </summary>
    /// <param name="value">The string segment to append.</param>
    void Append(string value);

    /// <summary>
    /// Reports an error that occurred during template resolution.
    /// </summary>
    /// <param name="error">The template error that was encountered.</param>
    void Error(in TemplateError error);

    /// <summary>
    /// Hook called immediately before a literal string segment is appended.
    /// </summary>
    /// <param name="willBeAppended">The literal string about to be appended.</param>
    /// <param name="controller">The controller that manages the template interpretation process.</param>
    void OnPreAppendLiteral(ref string willBeAppended, TemplateInterpreterController controller);

    /// <summary>
    /// Hook called immediately after a literal string segment has been appended.
    /// </summary>
    /// <param name="appended">The literal string that was appended.</param>
    /// <param name="controller">The controller that manages the template interpretation process.</param>
    void OnPostAppendLiteral(string appended, TemplateInterpreterController controller);

    /// <summary>
    /// Hook called immediately before an interpolated value is appended.
    /// Allows modification of the value, alignment, and format.
    /// </summary>
    /// <typeparam name="T">The type of the interpolated value.</typeparam>
    /// <param name="x">The interpolated value, passed by reference.</param>
    /// <param name="alignment">The alignment specifier, passed by reference.</param>
    /// <param name="format">The format string, passed by reference.</param>
    /// <param name="controller">The controller that manages the template interpretation process.</param>
    void OnPreAppendFormatted<T>(ref T x, ref int alignment, ref string? format,
        TemplateInterpreterController controller);

    /// <summary>
    /// Attempts to handle an interpolated value that was not processed by the default formatter.
    /// </summary>
    /// <typeparam name="T">The type of the interpolated value.</typeparam>
    /// <param name="x">The unhandled interpolated value.</param>
    /// <param name="alignment">The alignment specifier.</param>
    /// <param name="format">The format string.</param>
    /// <param name="controller">The controller that manages the template interpretation process.</param>
    /// <returns><c>true</c> if the value was handled.</returns>
    bool TryHandleUnhandledInterpolation<T>(T x, int alignment, string? format,
        TemplateInterpreterController controller);

    /// <summary>
    /// Hook called immediately after an interpolated value has been appended.
    /// </summary>
    /// <typeparam name="T">The type of the interpolated value.</typeparam>
    /// <param name="x">The interpolated value that was appended.</param>
    /// <param name="controller">The controller that manages the template interpretation process.</param>
    void OnPostAppendFormatted<T>(T x, TemplateInterpreterController controller);

    /// <summary>
    /// Hook called immediately after the all evaluation is completed.
    /// </summary>
    /// <param name="controller">The controller that manages the template interpretation process.</param>
    void OnCompleted(TemplateInterpreterController controller);
}

/// <summary>
/// Provides helper methods for reporting template context errors.
/// </summary>
public static class ResolveTemplateContextExtensions
{
    // NOTE: Since multiple templates may be executed in multi threads and results may be aggregated,
    //  we use Interlocked instead of ThreadStatic
    private static int s_lastSyntaxErrorId = -1;

    /// <summary>
    /// Reports a syntax error that occurred during template resolution.
    /// </summary>
    public static void SyntaxError(this ITemplateInterpreterContext self, string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        var id = Interlocked.Increment(ref s_lastSyntaxErrorId);
        self.Error(new TemplateError($"[SyntaxError](id={id}) {message}", callerFilePath, callerLineNumber));
        self.Append($"/* ! Syntax Error (id={id}) ! message={message} */");
    }
}
