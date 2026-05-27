// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// Holds the set of format directives decoded from an interpolation's format string.
/// Returned by <see cref="TemplateInterpreter"/>'s format decoder and threaded through
/// to <see cref="Token"/> initialization.
/// </summary>
/// <param name="LeftTrim">Left-side whitespace trim mode (<c>&lt;</c> or <c>&lt;&lt;</c>).</param>
/// <param name="RightTrim">Right-side whitespace trim mode (<c>&gt;</c> or <c>&gt;&gt;</c>).</param>
/// <param name="SuppressAutoIndent">Whether auto-indentation is suppressed (<c>v</c>).</param>
/// <param name="FirstIterationRightTrim">
/// Whether the first iteration of a loop command should trim leading whitespace (including newlines)
/// from the first literal token of the loop body (<c>&gt;|</c>).
/// </param>
internal readonly record struct FormatDirectives(
    TrimMode LeftTrim,
    TrimMode RightTrim,
    bool SuppressAutoIndent,
    bool FirstIterationRightTrim)
{
    public static FormatDirectives None => default;
}
