// (c) DeNA Co., Ltd.

using System.Runtime.CompilerServices;

namespace Waffle.Interpreter;

/// <summary>
/// An <see cref="System.Runtime.CompilerServices.InterpolatedStringHandlerAttribute"/> implementation that acts as a template string interpreter.
/// </summary>
/// <remarks>
/// Elements received via <c>AppendLiteral</c> / <c>AppendFormatted</c> are parsed into a syntax tree
/// (e.g. For...End, If...End), which is then evaluated in one pass at the end to produce a single output string.<br/>
/// The logic defined inside <see cref="TemplateInterpreter"/> is primarily the parser; evaluators are defined within each block and block-element type.
/// </remarks>
[InterpolatedStringHandler]
public readonly struct TemplateInterpreter
{
    /// <summary>
    /// Context provided by the caller that defines custom processing rules and receives the final output.
    /// </summary>
    private readonly ITemplateInterpreterContext _ctx;

    /// <summary>
    /// The total length of literal parts in the given interpolated string.
    /// </summary>
    private readonly int _literalLength;

    /// <summary>
    /// The number of interpolation within the given interpolated string.
    /// This counts the number of times <c>AppendFormatted</c> is called, regardless of the type of the interpolated object.
    /// </summary>
    private readonly int _formattedCount;

    /// <summary>
    /// A controller that manages the collection of raw tokens during the template parsing phase,
    /// and evaluates the final syntax tree when all input has been consumed.
    /// </summary>
    private readonly TemplateInterpreterController _controller;

    /// <summary>
    /// Initializes a new template interpreter.
    /// </summary>
    public TemplateInterpreter(int literalLength, int formattedCount, ITemplateInterpreterContext ctx)
    {
        _ctx = ctx;
        _literalLength = literalLength;
        _formattedCount = formattedCount;
        _controller = new TemplateInterpreterController(formattedCount);
        ctx.OnHandlerCreated(literalLength, formattedCount, _controller);
    }

    /// <summary>
    /// Called for non-interpolated (literal) parts of the interpolated string
    /// </summary>
    public void AppendLiteral(string s)
    {
        var len = s.Length;

        // Pre-processing defined in the context
        _ctx.OnPreAppendLiteral(ref s, _controller);

        // Record the (possibly mutated) literal in the flat token list
        AppendLiteralInternal(s);

        // Update the consumed literal length
        _controller.ConsumeLiteral(len);

        // Post-processing defined in the context
        _ctx.OnPostAppendLiteral(s, _controller);

        // Finalize if all input has been consumed
        TerminateIfAllConsumed();
    }

    /// <summary>
    /// Called for interpolated (formatted) parts of the interpolated string
    /// </summary>
    public void AppendFormatted<T>(T x, int alignment = 0, string? format = null)
    {
        // Pre-processing defined in the context
        _ctx.OnPreAppendFormatted(ref x, ref alignment, ref format, _controller);

        // Decode whitespace and indentation options from the format string
        var directives = DecodeAlignmentAndFormat(ref format);

        // Record the interpolation in the flat token list
        RecordInterpolation(x, alignment, format, directives);

        // Update the consumed formatted count
        _controller.ConsumeFormatted(1);

        // Post-processing defined in the context
        _ctx.OnPostAppendFormatted(x, _controller);

        // Finalize if all input has been consumed
        TerminateIfAllConsumed();
    }

    private void AppendLiteralInternal(string s)
    {
        var isFirstLiteral = !_controller.IsConsumedAnyInput;
        _controller.RecordLiteral(s, isFirstLiteral);
    }

    /// <summary>
    /// Decodes trim directives from the format string, removing them from the format so that
    /// the remaining format string can be applied normally.
    /// </summary>
    private static FormatDirectives DecodeAlignmentAndFormat(ref string? format)
    {
        if (format is null)
        {
            return FormatDirectives.None;
        }

        // If no trim directive characters are present, skip processing
        var hasDirective = false;
        foreach (var c in format)
        {
            if (c is '<' or '>' or 'v')
            {
                hasDirective = true;
                break;
            }
        }

        if (!hasDirective)
        {
            return FormatDirectives.None;
        }

        // NOTE: Span<char> requires System.Memory in netstandard2.0 (becomes a Slow Span), so char[] is used instead
        var buf = new char[format.Length];
        var written = 0;
        var leftTrim = TrimMode.None;
        var rightTrim = TrimMode.None;
        var suppressAutoIndent = false;
        var firstIterationRightTrim = false;

        for (var i = 0; i < format.Length; i++)
        {
            var c = format[i];
            var nc = i < format.Length - 1 ? format[i + 1] : '\0';
            switch (c, nc)
            {
                // Remove all consecutive spaces to the left of the interpolation (including line breaks)
                case ('<', '<'):
                    leftTrim = TrimMode.WithLineBreak;
                    i++;
                    break;
                // Remove all consecutive spaces to the right of the interpolation (including line breaks)
                case ('>', '>'):
                    rightTrim = TrimMode.WithLineBreak;
                    i++;
                    break;
                // Remove leading whitespace (including line breaks) from body on first iteration only
                case ('>', '|'):
                    firstIterationRightTrim = true;
                    i++;
                    break;
                // Remove all consecutive spaces to the left of the interpolation (excluding line breaks)
                case ('<', _):
                    leftTrim = TrimMode.NoLineBreak;
                    break;
                // Remove all consecutive spaces to the right of the interpolation (excluding line breaks)
                case ('>', _):
                    rightTrim = TrimMode.NoLineBreak;
                    break;
                // Verbatim mode: disables auto-indentation
                case ('v', _):
                    suppressAutoIndent = true;
                    break;
                default:
                    buf[written++] = c;
                    break;
            }
        }

        // Overwrite with the remaining portion
        format = written == 0 ? null : new string(buf, 0, written);
        return new FormatDirectives(leftTrim, rightTrim, suppressAutoIndent, firstIterationRightTrim);
    }

    /// <summary>
    /// Records an interpolation in the flat token list, updating the block type stack as needed.
    /// </summary>
    private void RecordInterpolation<T>(T x, int alignment, string? format, in FormatDirectives directives)
    {
        switch (x)
        {
            // Conditional if block start command
            case IfCommand:
                _controller.PushBlockType(BlockCategory.Conditional);
                break;

            // Conditional elif block start command (closes the current if/elif block and starts a new one)
            // Conditional else block start command
            case ElifCommand or ElseCommand:
                _controller.TryPopBlockType(out _);
                _controller.PushBlockType(BlockCategory.Conditional);
                break;

            // Iteration block start command
            case IterationCommandBase:
                _controller.PushBlockType(BlockCategory.Iteration);
                break;

            // Block end command
            case EndBlockCommand:
            {
                if (!_controller.TryPopBlockType(out _))
                {
                    _ctx.SyntaxError($"{nameof(EndBlockCommand)} appeared with no block currently open");
                    return;
                }

                break;
            }
        }

        var shouldRemoveCmdLine = x is ICommandInterpolation { ShouldRemoveCommandOnlyLine: true };
        _controller.RecordInterpolation(x, alignment, format, shouldRemoveCmdLine, directives);
    }

    /// <summary>
    /// Performs finalization if the entire interpolated string has been processed.
    /// </summary>
    private void TerminateIfAllConsumed()
    {
        // Do nothing if there is still unconsumed input
        if (!_controller.IsAllConsumed(_literalLength, _formattedCount))
        {
            return;
        }

        // Evaluate all collected tokens, applying trim processing to literal parts as needed, and write the results to the context
        _controller.Evaluate(_ctx);

        _ctx.OnCompleted(_controller);
    }
}
