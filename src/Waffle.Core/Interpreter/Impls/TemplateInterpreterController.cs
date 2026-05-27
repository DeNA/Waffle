// (c) DeNA Co., Ltd.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Waffle.Interpreter;

/// <summary>
/// Accessor and operation interface for the mutable internal state of <see cref="TemplateInterpreter" />.
/// </summary>
public sealed class TemplateInterpreterController(int formattedCount)
{
    /// <summary>
    /// Flat list of all tokens collected during <c>AppendLiteral</c> / <c>AppendFormatted</c> calls.
    /// Rented from <see cref="ListPool{T}"/> and returned after evaluation completes.
    /// </summary>
    private readonly List<Token> _tokens = ListPool<Token>.Rent(formattedCount * 2 + 1);

    /// <summary>
    /// Tracks the category of each currently open block during the template parsing phase.
    /// Used only for <see cref="EndBlockCommand"/> dispatch and unclosed-block validation.
    /// Rented from <see cref="StackPool{T}"/> and returned after evaluation completes.
    /// </summary>
    private readonly Stack<BlockCategory> _blockTypeStack = StackPool<BlockCategory>.Rent();

    /// <summary>
    /// Total character count of literals that have been processed by <c>AppendLiteral</c>.
    /// Counted as the length passed to the handler, not the length actually appended to the output.
    /// </summary>
    public int ConsumedLiteralLength { get; private set; }

    /// <summary>
    /// Number of interpolations that have been processed by <c>AppendFormatted</c>.
    /// </summary>
    public int ConsumedFormattedCount { get; private set; }

    /// <summary>
    /// Whether at least one input (literal or interpolation) has been processed.
    /// </summary>
    public bool IsConsumedAnyInput => ConsumedLiteralLength > 0 || ConsumedFormattedCount > 0;

    /// <summary>
    /// Advances the <c>AppendLiteral</c> progress by the specified length.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ConsumeLiteral(int length)
    {
        ConsumedLiteralLength += length;
    }

    /// <summary>
    /// Advances the <c>AppendFormatted</c> progress by the specified count.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ConsumeFormatted(int count)
    {
        ConsumedFormattedCount += count;
    }

    /// <summary>
    /// Records a literal string collected during <c>AppendLiteral</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RecordLiteral(string value, bool isFirstLiteral)
    {
        _tokens.Add(Token.Pool.Rent(value, isFirstLiteral));
    }

    /// <summary>
    /// Records an interpolation collected during <c>AppendFormatted</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void RecordInterpolation(object? interpolation, int alignment, string? format,
        bool shouldRemoveCommandOnlyLine, in FormatDirectives directives)
    {
        _tokens.Add(Token.Pool.Rent(interpolation, alignment, format, shouldRemoveCommandOnlyLine, directives));
    }

    /// <summary>
    /// Pushes a block category onto the nesting stack when a block-start command is encountered.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushBlockType(BlockCategory category)
    {
        _blockTypeStack.Push(category);
    }

    /// <summary>
    /// Pops the top block category from the nesting stack.
    /// Returns <c>false</c> if the stack is empty.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPopBlockType(out BlockCategory category)
    {
        return _blockTypeStack.TryPop(out category);
    }

    /// <summary>
    /// Returns the top block category without removing it.
    /// Returns <c>false</c> if the stack is empty.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeekBlockType(out BlockCategory category)
    {
        return _blockTypeStack.TryPeek(out category);
    }

    /// <summary>
    /// Returns whether the entire interpolated string has been processed by <c>AppendLiteral</c>/<c>AppendFormatted</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAllConsumed(int literalLength, int formattedCount)
    {
        return ConsumedFormattedCount >= formattedCount && ConsumedLiteralLength >= literalLength;
    }

    /// <summary>
    /// Evaluates the collected tokens as a syntax tree, appending output to the context.
    /// Returns the pooled token list and block-type stack to their respective pools after evaluation.
    /// </summary>
    internal void Evaluate(ITemplateInterpreterContext ctx)
    {
        // Validate that there are no unclosed blocks
        if (_blockTypeStack.Count > 0)
        {
            ctx.SyntaxError($"There are unclosed blocks (depth: {_blockTypeStack.Count})");
            return;
        }

        try
        {
            new TemplateEvaluator(_tokens, this).Evaluate(ctx);
        }
        finally
        {
            // Return pooled token and collections now that evaluation is complete.
            // No code path accesses _tokens or _blockTypeStack after this point.
            foreach (var token in _tokens)
            {
                Token.Pool.Return(token);
            }

            ListPool<Token>.Return(_tokens);
            StackPool<BlockCategory>.Return(_blockTypeStack);
        }
    }
}
