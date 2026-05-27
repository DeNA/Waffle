// (c) DeNA Co., Ltd.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Waffle.Interpreter;

/// <summary>
/// Evaluates the flat token list produced by <see cref="TemplateInterpreterController"/>
/// and appends the generated output to an <see cref="ITemplateInterpreterContext"/>.
/// </summary>
internal readonly struct TemplateEvaluator
{
    private readonly List<Token> _tokens;
    private readonly TemplateInterpreterController _controller;

    internal TemplateEvaluator(List<Token> tokens, TemplateInterpreterController controller)
    {
        _tokens = tokens;
        _controller = controller;
    }

    /// <summary>
    /// Pairs block indexes, applies trims, then evaluates the token list.
    /// </summary>
    internal void Evaluate(ITemplateInterpreterContext ctx)
    {
        // Pair block start/end indexes (needed for evaluation to skip/iterate over blocks)
        PairBlockIndexes();

        ApplyTrims();

        // Evaluate tokens as a syntax tree
        using var _ = DictionaryPool<int, EnvValue>.Get(out var env);
        var flow = EvaluateRange(0, _tokens.Count, env, ctx);

        if (flow == FlowControl.Break)
        {
            ctx.SyntaxError("Unexpected 'break' outside of iteration block");
        }
        else if (flow == FlowControl.Continue)
        {
            ctx.SyntaxError("Unexpected 'continue' outside of iteration block");
        }
    }

    /// <summary>
    /// Applies command-only line removal and trim directives in two passes.
    /// Pass 1 handles structural command-only line removal (operating on original literal values).
    /// Pass 2 applies left/right trim directives, respecting pass 1's results.
    /// </summary>
    private void ApplyTrims()
    {
        using var _ = HashSetPool<int>.Get(out var commandOnlyLineRemoved);
        // Pass 1: Command-only line removal (operates on unmodified literal values)
        ApplyCommandOnlyLineRemovals(commandOnlyLineRemoved);

        // Pass 2: Trim directives (left-trim, right-trim)
        ApplyTrimDirectives(commandOnlyLineRemoved);
    }

    /// <summary>
    /// Pass 1: Applies command-only line removal to all literals in a single forward scan — O(n).
    /// Returns a set of literal token indexes that had command-only line removal applied.
    /// <para>
    /// For each group of consecutive non-literal tokens (between two literals), the scan checks
    /// whether all tokens have <see cref="Token.ShouldRemoveCommandOnlyLine"/> = true.
    /// Each line is evaluated independently — an <see cref="EndBlockCommand"/> on a command-only
    /// line is always removed regardless of whether its paired start was on a command-only line.
    /// </para>
    /// </summary>
    private void ApplyCommandOnlyLineRemovals(HashSet<int> removedTokenIndexes)
    {
        var prevLiteralIdx = -1;
        var currentGroupStart = -1;
        var shouldRemoveCurrentLineIfCommandOnly = true;

        var count = _tokens.Count;
        for (var i = 0; i <= count; i++)
        {
            var isEnd = i == count;

            if (!isEnd && !_tokens[i].IsLiteral)
            {
                if (currentGroupStart < 0)
                {
                    currentGroupStart = i;
                }

                if (!_tokens[i].ShouldRemoveCommandOnlyLine)
                {
                    shouldRemoveCurrentLineIfCommandOnly = false;
                }

                continue;
            }

            // Literal or end-of-tokens: finalize the preceding command group (if any).
            if (currentGroupStart >= 0)
            {
                if (shouldRemoveCurrentLineIfCommandOnly)
                {
                    if (!isEnd)
                    {
                        // Try to remove the whitespace-only line surrounding this command group.
                        if (HasSpaceOnlyLinePrefix(_tokens[i].LiteralValue!))
                        {
                            if (prevLiteralIdx < 0)
                            {
                                // No preceding literal: trim just the leading whitespace line of this literal.
                                RemoveSpaceOnlyLinePrefixAndLineBreak(i);
                                removedTokenIndexes.Add(i);
                            }
                            else
                            {
                                var prev = _tokens[prevLiteralIdx];
                                var prevValue = prev.LiteralValue!;
                                var prevIsLineHead = prev.IsLineHead;
                                if (HasSpaceOnlyLineSuffix(prevValue) ||
                                    (prevIsLineHead && IsSpaceOnlyOneLine(prevValue)))
                                {
                                    prev.LiteralValue = RemoveSpaceOnlyLineSuffix(prevValue, prevIsLineHead);
                                    RemoveSpaceOnlyLinePrefixAndLineBreak(i);
                                    removedTokenIndexes.Add(i);
                                }
                            }
                        }
                    }
                    else if (prevLiteralIdx >= 0)
                    {
                        // Trailing command group with no following literal: trim only the preceding literal's suffix.
                        var prev = _tokens[prevLiteralIdx];
                        var prevValue = prev.LiteralValue!;
                        var prevIsLineHead = prev.IsLineHead;
                        if (HasSpaceOnlyLineSuffix(prevValue) ||
                            (prevIsLineHead && IsSpaceOnlyOneLine(prevValue)))
                        {
                            prev.LiteralValue = RemoveSpaceOnlyLineSuffix(prevValue, prevIsLineHead);
                        }
                    }
                }

                currentGroupStart = -1;
                shouldRemoveCurrentLineIfCommandOnly = true;
            }

            if (!isEnd)
            {
                prevLiteralIdx = i;
            }
        }
    }

    /// <summary>
    /// Pass 2: Applies left-trim and right-trim directives to literals.
    /// </summary>
    /// <param name="commandOnlyLineRemoved">
    /// Set of literal token indexes where command-only line removal was applied in pass 1.
    /// Used to gate right-trim: if removal was applied and right-trim is <see cref="TrimMode.NoLineBreak"/>,
    /// the right-trim is skipped.
    /// </param>
    private void ApplyTrimDirectives(HashSet<int> commandOnlyLineRemoved)
    {
        for (var i = 0; i < _tokens.Count; i++)
        {
            var token = _tokens[i];

            if (!token.IsLiteral)
            {
                // Apply left trim: remove trailing whitespace from the immediately preceding literal
                if (token.LeftTrim is TrimMode.None || !_tokens[i - 1].IsLiteral)
                {
                    continue;
                }

                var prev = _tokens[i - 1];
                prev.LiteralValue = ApplyTrailingWhiteSpaceTrim(
                    prev.LiteralValue!, token.LeftTrim == TrimMode.WithLineBreak);
            }
            else
            {
                // For each literal, apply right-trim from the immediately preceding interpolation
                if (i <= 0 || _tokens[i - 1].IsLiteral)
                {
                    continue;
                }

                var preceding = _tokens[i - 1];
                var wasRemoved = commandOnlyLineRemoved.Contains(i);

                // Apply right trim: remove leading whitespace from this literal
                if (preceding.RightTrim != TrimMode.None &&
                    (!wasRemoved || preceding.RightTrim == TrimMode.WithLineBreak))
                {
                    _tokens[i].LiteralValue = ApplyLeadingWhiteSpaceTrim(
                        token.LiteralValue!, preceding.RightTrim == TrimMode.WithLineBreak);
                }
            }
        }
    }

    /// <summary>
    /// Removes all trailing whitespace from a literal value.
    /// </summary>
    private static string ApplyTrailingWhiteSpaceTrim(string value, bool includeLineBreak)
    {
        for (var i = value.Length - 1; i >= 0; i--)
        {
            var c = value[i];
            if (!includeLineBreak && c == '\n' || !char.IsWhiteSpace(c))
            {
                return value[..(i + 1)];
            }
        }

        return "";
    }

    /// <summary>
    /// Removes leading whitespace from a literal value.
    /// </summary>
    private static string ApplyLeadingWhiteSpaceTrim(string value, bool includeLineBreaks)
    {
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (!includeLineBreaks && c == '\n' || !char.IsWhiteSpace(c))
            {
                return value[i..];
            }
        }

        return "";
    }

    /// <summary>
    /// Whether the value starts with a whitespace-only line followed by a newline.
    /// </summary>
    private static bool HasSpaceOnlyLinePrefix(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '\n')
            {
                return true;
            }

            if (!char.IsWhiteSpace(c))
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Whether the value ends with a newline followed by a whitespace-only line.
    /// </summary>
    private static bool HasSpaceOnlyLineSuffix(string value)
    {
        if (value.Length == 0)
        {
            return false;
        }

        for (var i = value.Length - 1; i >= 0; i--)
        {
            var c = value[i];
            if (c == '\n')
            {
                return true;
            }

            if (!char.IsWhiteSpace(c))
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Whether the value contains only whitespace characters and no newlines.
    /// </summary>
    private static bool IsSpaceOnlyOneLine(string value)
    {
        if (value.Length == 0)
        {
            return true;
        }

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '\n')
            {
                return false;
            }

            if (!char.IsWhiteSpace(c))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Removes the leading whitespace-only line prefix (up to and including the first newline)
    /// and sets <see cref="Token.IsLineHead"/> to <c>true</c> on the token at <paramref name="tokenIdx"/>.
    /// </summary>
    private void RemoveSpaceOnlyLinePrefixAndLineBreak(int tokenIdx)
    {
        var token = _tokens[tokenIdx];
        var value = token.LiteralValue!;

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '\n')
            {
                value = value[(i + 1)..];
                break;
            }

            if (!char.IsWhiteSpace(c))
            {
                break;
            }
        }

        // The leading line up to the newline has been removed, so this becomes a line-head literal.
        token.LiteralValue = value;
        token.IsLineHead = true;
        _tokens[tokenIdx] = token;
    }

    /// <summary>
    /// Removes the trailing whitespace-only line suffix (from the last newline onward).
    /// If the literal is a line-head and consists only of whitespace, the entire value is cleared.
    /// </summary>
    private static string RemoveSpaceOnlyLineSuffix(string value, bool isLineHead)
    {
        if (isLineHead && IsSpaceOnlyOneLine(value))
        {
            return "";
        }

        for (var i = value.Length - 1; i >= 0; i--)
        {
            var c = value[i];
            if (c == '\n')
            {
                return value[..(i + 1)];
            }

            if (!char.IsWhiteSpace(c))
            {
                return value;
            }
        }

        return value;
    }

    /// <summary>
    /// Fills in <see cref="Token.EndIndex"/> for all block-start and mid-block commands
    /// by pairing them with their corresponding end/sibling commands.
    /// </summary>
    internal void PairBlockIndexes()
    {
        using var _ = StackPool<int>.Get(out var stack);
        for (var i = 0; i < _tokens.Count; i++)
        {
            var token = _tokens[i];
            if (token.IsLiteral)
            {
                continue;
            }

            switch (token.Interpolation)
            {
                case IfCommand:
                case IterationCommandBase:
                    stack.Push(i);
                    break;

                case ElifCommand:
                case ElseCommand:
                {
                    if (stack.TryPop(out var prevIdx))
                    {
                        _tokens[prevIdx].EndIndex = i;
                    }

                    stack.Push(i);
                    break;
                }

                case EndBlockCommand:
                {
                    if (stack.TryPop(out var prevIdx))
                    {
                        _tokens[prevIdx].EndIndex = i;
                    }

                    break;
                }
            }
        }
    }

    /// <summary>
    /// Evaluates tokens in the range [<paramref name="start"/>, <paramref name="end"/>),
    /// appending output directly to <paramref name="ctx"/>.
    /// </summary>
    private FlowControl EvaluateRange(
        int start, int end,
        Dictionary<int, EnvValue> env,
        ITemplateInterpreterContext ctx,
        bool trimLeadingOnFirstLiteral = false)
    {
        for (var i = start; i < end; i++)
        {
            var token = _tokens[i];

            if (token.IsLiteral)
            {
                var value = token.LiteralValue!;
                if (trimLeadingOnFirstLiteral)
                {
                    trimLeadingOnFirstLiteral = false;
                    value = ApplyLeadingWhiteSpaceTrim(value, includeLineBreaks: true);
                }

                if (value.Length > 0)
                {
                    ctx.Append(value);
                }

                continue;
            }

            switch (token.Interpolation)
            {
                case IfCommand:
                {
                    var (nextIdx, flow) = EvaluateConditionalChain(i, env, ctx);
                    if (flow != FlowControl.Normal)
                    {
                        return flow;
                    }

                    i = nextIdx - 1; // -1 because loop increments
                    break;
                }

                // optimization: specialize non-generic iteration
                case ForCommand c:
                {
                    var bodyStart = i + 1;
                    var bodyEnd = token.EndIndex;
                    EvaluateForIteration(c, bodyStart, bodyEnd, env, ctx,
                        token.FirstIterationRightTrim);
                    i = token.EndIndex;
                    break;
                }

                case IterationCommandBase iterCommand:
                {
                    var bodyStart = i + 1;
                    var bodyEnd = token.EndIndex;
                    iterCommand.Iterate(env,
                        new IterationBlockEvaluator(this, bodyStart, bodyEnd, ctx, token.FirstIterationRightTrim));
                    i = token.EndIndex; // skip past EndIteration; loop increments past it
                    break;
                }

                case BreakIterationCommand:
                    return FlowControl.Break;

                case ContinueIterationCommand:
                    return FlowControl.Continue;

                case INopCommandInterpolation:
                    break;

                case IBlockContent content:
                {
                    var evaluated = content.Evaluate(env, token.Alignment, token.Format).Value;
                    if (!token.SuppressAutoIndent && evaluated.Length > 0 && evaluated.IndexOf('\n') >= 0)
                    {
                        var indent = GetAutoIndent(i);
                        if (indent?.Length > 0)
                        {
                            evaluated = ApplyAutoIndent(evaluated, indent);
                        }
                    }

                    if (evaluated.Length > 0)
                    {
                        ctx.Append(evaluated);
                    }

                    break;
                }

                default:
                {
                    if (ctx.TryHandleUnhandledInterpolation(
                            token.Interpolation, token.Alignment, token.Format, _controller))
                    {
                        break;
                    }

                    var formatted = TemplateInterpreterHelper.FormatByDefault(
                        token.Interpolation, token.Alignment, token.Format);

                    if (!token.SuppressAutoIndent && formatted.Length > 0 && formatted.IndexOf('\n') >= 0)
                    {
                        var indent = GetAutoIndent(i);
                        if (indent?.Length > 0)
                        {
                            formatted = ApplyAutoIndent(formatted, indent);
                        }
                    }

                    if (formatted.Length > 0)
                    {
                        ctx.Append(formatted);
                    }

                    break;
                }
            }
        }

        return FlowControl.Normal;
    }

    /// <summary>
    /// Evaluates an If/Elif/Else conditional chain starting at <paramref name="ifIdx"/>.
    /// Returns the index to resume from (past EndIf) and the flow control signal.
    /// </summary>
    private (int nextIdx, FlowControl flow) EvaluateConditionalChain(
        int ifIdx,
        Dictionary<int, EnvValue> env,
        ITemplateInterpreterContext ctx)
    {
        var idx = ifIdx;
        while (true)
        {
            var token = _tokens[idx];

            if (token.Interpolation is EndBlockCommand)
            {
                // No branch matched
                return (idx + 1, FlowControl.Normal);
            }

            if (token.Interpolation is BeginConditionalBlockCommandBase cond && cond.IsSatisfied(env))
            {
                // Evaluate the matched branch body
                var flow = EvaluateRange(idx + 1, token.EndIndex, env, ctx);

                // Skip to EndBlock by following the EndIndex chain
                var endIdx = token.EndIndex;
                while (_tokens[endIdx].Interpolation is not EndBlockCommand)
                {
                    endIdx = _tokens[endIdx].EndIndex;
                }

                return (endIdx + 1, flow);
            }

            // Condition not satisfied; advance to next branch
            idx = token.EndIndex;
        }
    }

    /// <summary>
    /// Inlined ForCommand evaluation to avoid virtual dispatch.
    /// </summary>
    private void EvaluateForIteration(
        ForCommand forCmd,
        int bodyStart,
        int bodyEnd,
        Dictionary<int, EnvValue> env,
        ITemplateInterpreterContext ctx,
        bool firstIterationLeadingTrim = false)
    {
        var from = forCmd.FromInclusive.Resolve(env);
        var toExclusive = forCmd.ToExclusive.Resolve(env);

        if (toExclusive > from)
        {
            if (!forCmd.Reverse)
            {
                for (var x = from; x < toExclusive; x++)
                {
                    env[forCmd.Id] = EnvValue.FromInt(x);
                    var trim = firstIterationLeadingTrim && x == from;
                    if (EvaluateRange(bodyStart, bodyEnd, env, ctx, trim) == FlowControl.Break)
                    {
                        env.Remove(forCmd.Id);
                        return;
                    }
                }
            }
            else
            {
                for (var x = toExclusive - 1; x >= from; x--)
                {
                    env[forCmd.Id] = EnvValue.FromInt(x);
                    var trim = firstIterationLeadingTrim && x == toExclusive - 1;
                    if (EvaluateRange(bodyStart, bodyEnd, env, ctx, trim) == FlowControl.Break)
                    {
                        env.Remove(forCmd.Id);
                        return;
                    }
                }
            }
        }

        env.Remove(forCmd.Id);
    }

    /// <summary>
    /// Returns the auto-indent string for the interpolation at <paramref name="tokenIdx"/>,
    /// or <c>null</c> if auto-indent does not apply.
    /// Auto-indent applies when the immediately preceding token is a literal whose suffix
    /// after the last newline (or the entire value when <see cref="Token.IsLineHead"/> is true)
    /// consists solely of whitespace characters.
    /// </summary>
    private string? GetAutoIndent(int tokenIdx)
    {
        if (tokenIdx <= 0)
        {
            return null;
        }

        var prev = _tokens[tokenIdx - 1];
        if (!prev.IsLiteral)
        {
            return null;
        }

        var value = prev.LiteralValue ?? "";

        // Case: literal is at line head and contains only spaces (no newline)
        if (prev.IsLineHead && IsSpaceOnlyOneLine(value))
        {
            return value;
        }

        // Find the suffix after the last \n; if it is all-whitespace, use it as the indent
        for (var i = value.Length - 1; i >= 0; i--)
        {
            var c = value[i];
            if (c == '\n')
            {
                var suffix = value[(i + 1)..];
                return IsSpaceOnlyOneLine(suffix) ? suffix : null;
            }

            if (!char.IsWhiteSpace(c))
            {
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Applies auto-indentation to <paramref name="value"/> by inserting <paramref name="indent"/>
    /// after every newline character.
    /// The trailing newline at the very end of the string also gets the indent inserted after it.
    /// </summary>
    private static string ApplyAutoIndent(string value, string indent)
    {
        var sb = new StringBuilder(value.Length + indent.Length * 4);
        foreach (var c in value)
        {
            sb.Append(c);
            if (c == '\n')
            {
                sb.Append(indent);
            }
        }

        return sb.ToString();
    }


    /// <summary>
    /// Captures the context for evaluating a block body.
    /// This is passed to <b>generic</b> iteration commands to avoid virtual dispatching and closure allocations.
    /// </summary>
    internal readonly ref struct IterationBlockEvaluator
    {
        private readonly TemplateEvaluator _evaluator;
        private readonly int _bodyStart;
        private readonly int _bodyEnd;
        private readonly ITemplateInterpreterContext _ctx;
        private readonly bool _trimFirstLiteral;

        internal IterationBlockEvaluator(
            in TemplateEvaluator evaluator, int bodyStart, int bodyEnd, ITemplateInterpreterContext ctx,
            bool trimFirstLiteral = false)
        {
            _evaluator = evaluator;
            _bodyStart = bodyStart;
            _bodyEnd = bodyEnd;
            _ctx = ctx;
            _trimFirstLiteral = trimFirstLiteral;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal FlowControl Evaluate(Dictionary<int, EnvValue> env, bool isFirstIteration)
        {
            var trimLeading = _trimFirstLiteral && isFirstIteration;
            return _evaluator.EvaluateRange(_bodyStart, _bodyEnd, env, _ctx, trimLeading);
        }
    }
}
