// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// A single element in the flat token list built during the template parsing phase.
/// Holds either a literal string or an interpolation with its associated directives.
/// It allows mutation during evaluation (whitespace trimming and block pairing).
/// </summary>
internal sealed class Token
{
    public bool IsLiteral;

    // --- Literal fields ---
    public string? LiteralValue;

    /// <summary>
    /// Whether this literal appears at the beginning of a line.
    /// Initialized from <c>isFirstLiteral</c> and may be set to <c>true</c>
    /// during command-only line removal when a leading whitespace-only prefix is trimmed.
    /// </summary>
    public bool IsLineHead;

    // --- Interpolation fields ---
    public object? Interpolation;
    public int Alignment;
    public string? Format;

    /// <summary>
    /// Left-side trim directive decoded from the format string (<c>&lt;</c> or <c>&lt;&lt;</c>).
    /// </summary>
    public TrimMode LeftTrim;

    /// <summary>
    /// Right-side trim directive decoded from the format string (<c>&gt;</c> or <c>&gt;&gt;</c>).
    /// </summary>
    public TrimMode RightTrim;

    /// <summary>
    /// Whether the line containing this command should be removed when it consists only of whitespace.
    /// Mirrors <see cref="ICommandInterpolation.ShouldRemoveCommandOnlyLine"/>.
    /// </summary>
    public bool ShouldRemoveCommandOnlyLine;

    /// <summary>
    /// For block-start/mid-block tokens (If, Elif, Else),
    /// the index of the next sibling or end command in <c>_tokens</c>.
    /// Set during <see cref="TemplateEvaluator.PairBlockIndexes"/>. Default is <c>-1</c>.
    /// </summary>
    public int EndIndex;

    /// <summary>
    /// Whether auto-indentation should be suppressed for this interpolation.
    /// Set to <c>true</c> when the <c>v</c> format specifier is present.
    /// </summary>
    public bool SuppressAutoIndent;

    /// <summary>
    /// Whether the first iteration of this loop command should trim leading whitespace
    /// (including newlines) from the first literal token of the loop body.
    /// Set to <c>true</c> when the <c>&gt;|</c> format specifier is present on a loop command.
    /// </summary>
    public bool FirstIterationRightTrim;

    /// <summary>
    /// Constructor for literal tokens.
    /// </summary>
    private Token(string literalValue, bool isFirstLiteral)
    {
        InitializeAsLiteral(literalValue, isFirstLiteral);
    }

    /// <summary>
    /// Constructor for interpolation tokens.
    /// </summary>
    private Token(object? interpolation, int alignment, string? format,
        bool shouldRemoveCommandOnlyLine, in FormatDirectives directives)
    {
        InitializeAsInterpolation(interpolation, alignment, format, shouldRemoveCommandOnlyLine, directives);
    }

    private void InitializeAsLiteral(string literalValue, bool isFirstLiteral)
    {
        IsLiteral = true;
        LiteralValue = literalValue;
        IsLineHead = isFirstLiteral;
        Interpolation = null;
        Alignment = 0;
        Format = null;
        LeftTrim = TrimMode.None;
        RightTrim = TrimMode.None;
        ShouldRemoveCommandOnlyLine = false;
        EndIndex = -1;
        SuppressAutoIndent = false;
        FirstIterationRightTrim = false;
    }

    private void InitializeAsInterpolation(object? interpolation, int alignment, string? format,
        bool shouldRemoveCommandOnlyLine, in FormatDirectives directives)
    {
        IsLiteral = false;
        LiteralValue = null;
        IsLineHead = false; // not used for interpolation
        Interpolation = interpolation;
        Alignment = alignment;
        Format = format;
        LeftTrim = directives.LeftTrim;
        RightTrim = directives.RightTrim;
        ShouldRemoveCommandOnlyLine = shouldRemoveCommandOnlyLine;
        EndIndex = -1;
        SuppressAutoIndent = directives.SuppressAutoIndent;
        FirstIterationRightTrim = directives.FirstIterationRightTrim;
    }

    internal static class Pool
    {
        [ThreadStatic]
        private static Stack<Token>? s_pool;

        public static Token Rent(string literalValue, bool isFirstLiteral)
        {
            s_pool ??= new Stack<Token>(32);
            if (!s_pool.TryPop(out var instance))
            {
                instance = new Token(literalValue, isFirstLiteral);
                return instance;
            }

            instance.InitializeAsLiteral(literalValue, isFirstLiteral);
            return instance;
        }

        public static Token Rent(object? interpolation, int alignment, string? format,
            bool shouldRemoveCommandOnlyLine, in FormatDirectives directives)
        {
            s_pool ??= new Stack<Token>(32);
            if (!s_pool.TryPop(out var instance))
            {
                instance = new Token(interpolation, alignment, format, shouldRemoveCommandOnlyLine, directives);
                return instance;
            }

            instance.InitializeAsInterpolation(interpolation, alignment, format, shouldRemoveCommandOnlyLine,
                directives);
            return instance;
        }

        public static void Return(Token token)
        {
            s_pool ??= new Stack<Token>();
            s_pool.Push(token);
        }
    }
}
