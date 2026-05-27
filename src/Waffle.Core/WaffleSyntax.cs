// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Waffle.Interpreter;

namespace Waffle;

/// <summary>
/// A collection of template commands. Intended to be used with a <c>using static</c> directive.
/// </summary>
public static class WaffleSyntax
{
    [ThreadStatic]
    private static int s_lastUsedId;

    /// <summary>
    /// Resets the internal ID generator.
    /// Intended for cases such as long-running processes, where IDs should be reset periodically
    /// to reduce the risk of overflowing.<br/>
    /// Notice that the internal ID is thread-static so it must be reset per thread.<br/>
    /// <b>Warning: The interpreter's behavior is not guaranteed if this method is called during template evaluation.</b>
    /// </summary>
    public static void ResetInternalIdGenerator()
    {
        s_lastUsedId = 0;
    }

    /// <summary>
    /// Iterates over each element of <paramref name="source"/>, assigns it to <paramref name="it"/>,
    /// and repeatedly outputs the block up to the matching <see cref="End"/>.
    /// </summary>
    /// <remarks>
    /// Note: accessing <paramref name="it"/> outside its scope results in a runtime error, not a compile-time error.
    /// </remarks>
    public static ForEachCommand<T> ForEach<T>(IEnumerable<T>? source, out IResolvableTo<T> it)
    {
        return new ForEachCommand<T>(++s_lastUsedId, source ?? Array.Empty<T>(), out it);
    }

    /// <inheritdoc cref="ForEach{T}(IEnumerable{T},out IResolvableTo{T})"/>
    public static ForEachCommand<T> ForEach<T>(IResolvableTo<IEnumerable<T>> source, out IResolvableTo<T> it)
    {
        return new ForEachCommand<T>(++s_lastUsedId, source, out it);
    }

    /// <inheritdoc cref="ForEach{T}(IEnumerable{T},out IResolvableTo{T})"/>
    public static ForEachCommand<TModel, TOriginal> ForEach<TModel, TOriginal>(
        IIterationSource<TModel, TOriginal> source, [NotNull] out TModel? it)
        where TModel : IResolvableTo<TOriginal>
    {
        return new ForEachCommand<TModel, TOriginal>(++s_lastUsedId, source, out it);
    }

    /// <summary>
    /// Iterates over each element of a nullable-element collection, assigning each element
    /// (which may be <see langword="null"/>) to <paramref name="it"/>.
    /// Use this overload instead of <see cref="ForEach{T}(IEnumerable{T},out IResolvableTo{T})"/>
    /// when the collection has nullable reference type elements.
    /// </summary>
    /// <remarks>
    /// Note: accessing <paramref name="it"/> outside its scope results in a runtime error, not a compile-time error.
    /// </remarks>
    public static NullableRefForEachCommand<T> ForEachNullable<T>(
        IEnumerable<T?>? source, out IResolvableTo<T?> it)
        where T : class
    {
        return new NullableRefForEachCommand<T>(++s_lastUsedId, source, out it);
    }

    /// <inheritdoc cref="ForEachNullable{T}(IEnumerable{T},out IResolvableTo{T})"/>
    public static NullableRefForEachCommand<T> ForEachNullable<T>(
        IResolvableTo<IEnumerable<T?>> source, out IResolvableTo<T?> it)
        where T : class
    {
        return new NullableRefForEachCommand<T>(++s_lastUsedId, source, out it);
    }


    /// <summary>
    /// Iterates over each element of <paramref name="source"/>, assigns it to <paramref name="it"/> and its
    /// index to <paramref name="i"/>, and repeatedly outputs the block up to the matching <see cref="End"/>.
    /// </summary>
    /// <remarks>
    /// Note: accessing <paramref name="it"/> or <paramref name="i"/> outside their scope results in a runtime error, not a compile-time error.
    /// </remarks>
    public static IndexedForEachCommand<T> ForEach<T>(
        IEnumerable<T>? source, out IResolvableTo<T> it, out IntProxy i)
    {
        var valueId = ++s_lastUsedId;
        var indexId = ++s_lastUsedId;
        return new IndexedForEachCommand<T>(valueId, indexId, source ?? [], out it, out i);
    }

    /// <inheritdoc cref="ForEach{T}(IEnumerable{T},out IResolvableTo{T}, out IntProxy)"/>
    public static IndexedForEachCommand<T> ForEach<T>(
        IResolvableTo<IEnumerable<T>> source, out IResolvableTo<T> it, out IntProxy i)
    {
        var valueId = ++s_lastUsedId;
        var indexId = ++s_lastUsedId;
        return new IndexedForEachCommand<T>(valueId, indexId, source, out it, out i);
    }

    /// <inheritdoc cref="ForEach{T}(IEnumerable{T},out IResolvableTo{T}, out IntProxy)"/>
    public static IndexedForEachCommand<TModel, TOriginal> ForEach<TModel, TOriginal>(
        IIterationSource<TModel, TOriginal> source, [NotNull] out TModel? it, out IntProxy i)
        where TModel : IResolvableTo<TOriginal>
    {
        var valueId = ++s_lastUsedId;
        var indexId = ++s_lastUsedId;
        return new IndexedForEachCommand<TModel, TOriginal>(
            valueId, indexId, source, out it, out i);
    }

    /// <inheritdoc cref="ForEachNullable{T}(IEnumerable{T},out IResolvableTo{T},out IntProxy)"/>
    public static IndexedNullableRefForEachCommand<T> ForEachNullable<T>(
        IEnumerable<T?>? source, out IResolvableTo<T?> it, out IntProxy i)
        where T : class
    {
        var valueId = ++s_lastUsedId;
        var indexId = ++s_lastUsedId;
        return new IndexedNullableRefForEachCommand<T>(valueId, indexId, source, out it, out i);
    }

    /// <inheritdoc cref="ForEachNullable{T}(IEnumerable{T},out IResolvableTo{T},out IntProxy)"/>
    public static IndexedNullableRefForEachCommand<T> ForEachNullable<T>(
        IResolvableTo<IEnumerable<T?>> source, out IResolvableTo<T?> it, out IntProxy i)
        where T : class
    {
        var valueId = ++s_lastUsedId;
        var indexId = ++s_lastUsedId;
        return new IndexedNullableRefForEachCommand<T>(valueId, indexId, source, out it, out i);
    }

    /// <inheritdoc cref="ForEachNullable{T}(IEnumerable{T},out IResolvableTo{T},out IntProxy,out IndexedLoopHelper)"/>
    public static IndexedNullableRefForEachCommand<T> ForEachNullable<T>(
        IEnumerable<T?>? source, out IResolvableTo<T?> it, out IntProxy i, out IndexedLoopHelper helper)
        where T : class
    {
        var valueId = ++s_lastUsedId;
        var indexId = ++s_lastUsedId;
        var command = new IndexedNullableRefForEachCommand<T>(valueId, indexId, source, out it, out i);
        helper = new IndexedLoopHelper(new IntProxy(new LiteralProxy<int>(source?.Count() ?? 0)), i);
        return command;
    }

    /// <inheritdoc cref="ForEachNullable{T}(IEnumerable{T},out IResolvableTo{T},out IntProxy,out IndexedLoopHelper)"/>
    public static IndexedNullableRefForEachCommand<T> ForEachNullable<T>(
        IResolvableTo<IEnumerable<T?>> source, out IResolvableTo<T?> it, out IntProxy i, out IndexedLoopHelper helper)
        where T : class
    {
        var valueId = ++s_lastUsedId;
        var indexId = ++s_lastUsedId;
        var command = new IndexedNullableRefForEachCommand<T>(valueId, indexId, source, out it, out i);
        helper = new IndexedLoopHelper(new IntProxy(source.To(it => it.Count())), i);
        return command;
    }

    /// <summary>
    /// Iterates over each element of <paramref name="source"/>, assigns it to <paramref name="it"/> and its
    /// index to <paramref name="i"/>, and repeatedly outputs the block up to the matching <see cref="End"/>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="it"></param>
    /// <param name="i"></param>
    /// <param name="helper">A helper for easily branching on whether the current index is the first or last element.</param>
    /// <remarks>
    /// Note: accessing <paramref name="it"/>, <paramref name="i"/>, or <paramref name="helper"/> outside their scope results in a runtime error, not a compile-time error.
    /// </remarks>
    public static IndexedForEachCommand<T> ForEach<T>(
        IEnumerable<T>? source, out IResolvableTo<T> it, out IntProxy i, out IndexedLoopHelper helper)
    {
        var valueId = ++s_lastUsedId;
        var indexId = ++s_lastUsedId;
        var command = new IndexedForEachCommand<T>(valueId, indexId, source ?? Array.Empty<T>(), out it, out i);
        helper = new IndexedLoopHelper(new IntProxy(new LiteralProxy<int>(source?.Count() ?? 0)), i);
        return command;
    }

    /// <inheritdoc cref="ForEach{T}(IEnumerable{T},out IResolvableTo{T}, out IntProxy, out IndexedLoopHelper)"/>
    public static IndexedForEachCommand<T> ForEach<T>(
        IResolvableTo<IEnumerable<T>> source, out IResolvableTo<T> it, out IntProxy i, out IndexedLoopHelper helper)
    {
        var command = ForEach(source, out it, out i);
        helper = new IndexedLoopHelper(new IntProxy(source.To(it => it.Count())), i);
        return command;
    }

    /// <inheritdoc cref="ForEach{T}(IEnumerable{T},out IResolvableTo{T}, out IntProxy, out IndexedLoopHelper)"/>
    public static IndexedForEachCommand<TModel, TOriginal> ForEach<TModel, TOriginal>(
        IIterationSource<TModel, TOriginal> source, [NotNull] out TModel? it, out IntProxy i,
        out IndexedLoopHelper helper)
        where TModel : IResolvableTo<TOriginal>
    {
        var command = ForEach(source, out it, out i);
        helper = new IndexedLoopHelper(source.Count, i);
        return command;
    }

    /// <inheritdoc cref="For(int,int,out IntProxy)"/>
    public static ForCommand For(
        IResolvableTo<int> fromInclusive, IResolvableTo<int> toExclusive, out IntProxy i)
    {
        return new ForCommand(++s_lastUsedId, fromInclusive, toExclusive, false, out i);
    }

    /// <inheritdoc cref="For(int,int,out IntProxy)"/>
    public static ForCommand For(
        int fromInclusive, IResolvableTo<int> toExclusive, out IntProxy i)
    {
        return For(new LiteralProxy<int>(fromInclusive), toExclusive, out i);
    }

    /// <inheritdoc cref="For(int,int,out IntProxy)"/>
    public static ForCommand For(
        IResolvableTo<int> fromInclusive, int toExclusive, out IntProxy i)
    {
        return For(fromInclusive, new LiteralProxy<int>(toExclusive), out i);
    }

    /// <summary>
    /// Iterates ascending integers from <paramref name="fromInclusive"/> (inclusive) to
    /// <paramref name="toExclusive"/> (exclusive), assigning each to <paramref name="i"/>,
    /// and outputs the block up to the matching <see cref="End"/>.
    /// </summary>
    /// <remarks>Equivalent to <c>for(var i=fromInclusive; i&lt;toExclusive; i++)</c></remarks>
    public static ForCommand For(int fromInclusive, int toExclusive, out IntProxy i)
    {
        return For(new LiteralProxy<int>(fromInclusive), new LiteralProxy<int>(toExclusive), out i);
    }

    /// <inheritdoc cref="For(int,int,out IntProxy,out IndexedLoopHelper)"/>
    public static ForCommand For(
        IResolvableTo<int> fromInclusive, IResolvableTo<int> toExclusive, out IntProxy i, out IndexedLoopHelper helper)
    {
        var command = new ForCommand(++s_lastUsedId, fromInclusive, toExclusive, false, out i);
        helper = new IndexedLoopHelper(
            new IntProxy(toExclusive.With(fromInclusive).To(p => p.Item1 - p.Item2)),
            i - new IntProxy(fromInclusive));
        return command;
    }

    /// <inheritdoc cref="For(int,int,out IntProxy,out IndexedLoopHelper)"/>
    public static ForCommand For(
        int fromInclusive, IResolvableTo<int> toExclusive, out IntProxy i, out IndexedLoopHelper helper)
    {
        var command = new ForCommand(
            ++s_lastUsedId, new LiteralProxy<int>(fromInclusive), toExclusive, false, out i);
        helper = new IndexedLoopHelper(
            new IntProxy(toExclusive.To(fromInclusive, (t, f) => t - f)),
            i - fromInclusive);
        return command;
    }

    /// <inheritdoc cref="For(int,int,out IntProxy,out IndexedLoopHelper)"/>
    public static ForCommand For(
        IResolvableTo<int> fromInclusive, int toExclusive, out IntProxy i, out IndexedLoopHelper helper)
    {
        var command = new ForCommand(
            ++s_lastUsedId, fromInclusive, new LiteralProxy<int>(toExclusive), false, out i);
        helper = new IndexedLoopHelper(
            new IntProxy(fromInclusive.To(toExclusive, (f, t) => t - f)),
            i - new IntProxy(fromInclusive));
        return command;
    }

    /// <summary>
    /// Iterates ascending integers from <paramref name="fromInclusive"/> (inclusive) to
    /// <paramref name="toExclusive"/> (exclusive), assigning each to <paramref name="i"/>,
    /// and outputs the block up to the matching <see cref="End"/>.
    /// </summary>
    /// <param name="fromInclusive"></param>
    /// <param name="toExclusive"></param>
    /// <param name="i"></param>
    /// <param name="helper">A helper for easily branching on whether the current index is the first or last element.</param>
    /// <remarks>Equivalent to <c>for(var i=fromInclusive; i&lt;toExclusive; i++)</c></remarks>
    public static ForCommand For(int fromInclusive, int toExclusive, out IntProxy i, out IndexedLoopHelper helper)
    {
        var command = new ForCommand(
            ++s_lastUsedId, new LiteralProxy<int>(fromInclusive), new LiteralProxy<int>(toExclusive), false,
            out i);
        helper = new IndexedLoopHelper(
            new IntProxy(new LiteralProxy<int>(toExclusive - fromInclusive)),
            i - fromInclusive);
        return command;
    }


    /// <inheritdoc cref="Forr(int,int,out IntProxy)"/>
    public static ForCommand Forr(
        IResolvableTo<int> fromInclusive, IResolvableTo<int> toInclusive, out IntProxy i)
    {
        return new ForCommand(++s_lastUsedId, toInclusive, fromInclusive.To(i => i + 1), true, out i);
    }

    /// <inheritdoc cref="Forr(int,int,out IntProxy)"/>
    public static ForCommand Forr(
        int fromInclusive, IResolvableTo<int> toInclusive, out IntProxy i)
    {
        return Forr(new LiteralProxy<int>(fromInclusive), toInclusive, out i);
    }

    /// <inheritdoc cref="Forr(int,int,out IntProxy)"/>
    public static ForCommand Forr(
        IResolvableTo<int> fromInclusive, int toInclusive, out IntProxy i)
    {
        return Forr(fromInclusive, new LiteralProxy<int>(toInclusive), out i);
    }

    /// <summary>
    /// Iterates descending integers from <paramref name="fromInclusive"/> down to <paramref name="toInclusive"/>
    /// (both inclusive), assigning each to <paramref name="i"/>, and outputs the block up to the matching <see cref="End"/>.
    /// </summary>
    /// <remarks>Equivalent to <c>for(var i=fromInclusive; i>=toInclusive; i--)</c></remarks>
    public static ForCommand Forr(
        int fromInclusive, int toInclusive, out IntProxy i)
    {
        return Forr(new LiteralProxy<int>(fromInclusive), new LiteralProxy<int>(toInclusive), out i);
    }

    /// <inheritdoc cref="Forr(int,int,out IntProxy,out IndexedLoopHelper)"/>
    public static ForCommand Forr(
        IResolvableTo<int> fromInclusive, IResolvableTo<int> toInclusive, out IntProxy i, out IndexedLoopHelper helper)
    {
        var command = new ForCommand(++s_lastUsedId, toInclusive, fromInclusive.To(f => f + 1), true, out i);
        helper = new IndexedLoopHelper(
            new IntProxy(fromInclusive.With(toInclusive).To(p => p.Item1 - p.Item2 + 1)),
            new IntProxy(fromInclusive) - i);
        return command;
    }

    /// <inheritdoc cref="Forr(int,int,out IntProxy,out IndexedLoopHelper)"/>
    public static ForCommand Forr(
        int fromInclusive, IResolvableTo<int> toInclusive, out IntProxy i, out IndexedLoopHelper helper)
    {
        var command = new ForCommand(
            ++s_lastUsedId, toInclusive, new LiteralProxy<int>(fromInclusive + 1), true, out i);
        helper = new IndexedLoopHelper(
            new IntProxy(toInclusive.To(fromInclusive, (t, f) => f - t + 1)),
            fromInclusive - i);
        return command;
    }

    /// <inheritdoc cref="Forr(int,int,out IntProxy,out IndexedLoopHelper)"/>
    public static ForCommand Forr(
        IResolvableTo<int> fromInclusive, int toInclusive, out IntProxy i, out IndexedLoopHelper helper)
    {
        var command = new ForCommand(
            ++s_lastUsedId, new LiteralProxy<int>(toInclusive), fromInclusive.To(f => f + 1), true, out i);
        helper = new IndexedLoopHelper(
            new IntProxy(fromInclusive.To(toInclusive, (f, t) => f - t + 1)),
            new IntProxy(fromInclusive) - i);
        return command;
    }

    /// <summary>
    /// Iterates descending integers from <paramref name="fromInclusive"/> down to <paramref name="toInclusive"/>
    /// (both inclusive), assigning each to <paramref name="i"/>, and outputs the block up to the matching <see cref="End"/>.
    /// </summary>
    /// <param name="fromInclusive"></param>
    /// <param name="toInclusive"></param>
    /// <param name="i"></param>
    /// <param name="helper">A helper for easily branching on whether the current index is the first or last element.</param>
    /// <remarks>Equivalent to <c>for(var i=fromInclusive; i>=toInclusive; i--)</c></remarks>
    public static ForCommand Forr(
        int fromInclusive, int toInclusive, out IntProxy i, out IndexedLoopHelper helper)
    {
        var command = new ForCommand(
            ++s_lastUsedId, new LiteralProxy<int>(toInclusive), new LiteralProxy<int>(fromInclusive + 1), true,
            out i);
        helper = new IndexedLoopHelper(
            new IntProxy(new LiteralProxy<int>(fromInclusive - toInclusive + 1)),
            fromInclusive - i);
        return command;
    }

    /// <inheritdoc cref="If(bool)"/>
    public static IfCommand If(IResolvableTo<bool> condition)
    {
        return new IfCommand(condition);
    }

    /// <summary>
    /// Outputs the block up to the matching <see cref="End"/>, <see cref="Elif(bool)"/>, or <see cref="Else"/>
    /// if <paramref name="condition"/> is <see langword="true"/>.
    /// </summary>
    public static IfCommand If(bool condition)
    {
        return If(new LiteralProxy<bool>(condition));
    }

    /// <inheritdoc cref="Elif(bool)"/>
    public static ElifCommand Elif(IResolvableTo<bool> condition)
    {
        return new ElifCommand(condition);
    }

    /// <summary>
    /// Outputs the block up to the matching <see cref="End"/>, <see cref="Elif(bool)"/>, or <see cref="Else"/>
    /// if no preceding <see cref="If(bool)"/> or <see cref="Elif(bool)"/> condition was satisfied
    /// and <paramref name="condition"/> is <see langword="true"/>.
    /// </summary>
    public static ElifCommand Elif(bool condition)
    {
        return Elif(new LiteralProxy<bool>(condition));
    }

    /// <summary>
    /// Outputs the block up to the matching <see cref="End"/> when no preceding
    /// <see cref="If(bool)"/> or <see cref="Elif(bool)"/> condition was satisfied.
    /// </summary>
    public static readonly ElseCommand Else = new();

    /// <summary>
    /// Outputs <paramref name="ifTrue"/> or <paramref name="ifFalse"/> depending on whether
    /// <paramref name="subject"/> satisfies <paramref name="predicate"/>.
    /// </summary>
    public static IResolvableTo<U> Cond<T, U>(
        IResolvableTo<T> subject, Predicate<T> predicate, U ifTrue, U ifFalse)
    {
        return new SelectPipe<T, (Predicate<T> Predicate, U IfTrue, U IfFalse), U>(
            subject, (predicate, ifTrue, ifFalse),
            (it, c) => c.Predicate(it) ? c.IfTrue : c.IfFalse);
    }

    /// <summary>
    /// Outputs <paramref name="ifTrue"/> or <paramref name="ifFalse"/> depending on whether
    /// <paramref name="condition"/> is satisfied.
    /// </summary>
    public static IResolvableTo<U> Cond<U>(
        IResolvableTo<bool> condition, IResolvableTo<U> ifTrue, IResolvableTo<U> ifFalse)
    {
        return condition.To((ifTrue, ifFalse), (b, c) => b ? c.ifTrue : c.ifFalse).Extract();
    }

    /// <summary>
    /// Outputs <paramref name="ifTrue"/> or <paramref name="ifFalse"/> depending on whether
    /// <paramref name="condition"/> is satisfied.
    /// </summary>
    public static IResolvableTo<U> Cond<U>(IResolvableTo<bool> condition, U ifTrue, IResolvableTo<U> ifFalse)
    {
        return condition.To((ifTrue, ifFalse), (b, c) => b ? new LiteralProxy<U>(c.ifTrue) : c.ifFalse).Extract();
    }

    /// <summary>
    /// Outputs <paramref name="ifTrue"/> or <paramref name="ifFalse"/> depending on whether
    /// <paramref name="condition"/> is satisfied.
    /// </summary>
    public static IResolvableTo<U> Cond<U>(IResolvableTo<bool> condition, IResolvableTo<U> ifTrue, U ifFalse)
    {
        return condition.To((ifTrue, ifFalse), (b, c) => b ? c.ifTrue : new LiteralProxy<U>(c.ifFalse)).Extract();
    }

    /// <summary>
    /// Assigns <paramref name="value"/> to <paramref name="varName"/>.
    /// </summary>
    /// <remarks>
    /// Note: accessing <paramref name="varName"/> outside its scope results in a runtime error, not a compile-time error.
    /// </remarks>
    public static NopCommand Let<T>([NotNullIfNotNull(nameof(value))] out T? varName, T? value)
    {
        varName = value;
        return NopCommand.Instance;
    }

    /// <summary>
    /// Assigns <paramref name="value"/> to <paramref name="varName"/>.
    /// </summary>
    /// <remarks>
    /// Note: accessing <paramref name="varName"/> outside its scope results in a runtime error, not a compile-time error.
    /// </remarks>
    public static NopCommand Let<T>(T? value, [NotNullIfNotNull(nameof(value))] out T? varName)
    {
        varName = value;
        return NopCommand.Instance;
    }

    /// <summary>
    /// Put anything you want, and it will be ignored. Useful for adding comments.
    /// </summary>
    public static NopCommand Note<T>(T? _)
    {
        return NopCommand.Instance;
    }

    /// <summary>
    /// In an iteration block, skips the rest of the current iteration and advances to the next element.
    /// </summary>
    public static readonly ContinueIterationCommand Continue = ContinueIterationCommand.Instance;

    /// <summary>
    /// In an iteration block, terminates the iteration.
    /// </summary>
    public static readonly BreakIterationCommand Break = BreakIterationCommand.Instance;

    /// <summary>
    /// Marks the end of a block.
    /// </summary>
    public static readonly EndBlockCommand End = EndBlockCommand.Instance;

    /// <summary>
    /// Renders a template and stores the result in <paramref name="ctx"/>.<br/>
    /// Pass an interpolated string literal ($"...") directly to <paramref name="template"/>.<br/>
    /// The result is stored in <paramref name="ctx"/>.
    /// </summary>
    public static void Render<T>(
        T ctx,
        [InterpolatedStringHandlerArgument("ctx")]
        in TemplateInterpreter template) where T : ITemplateInterpreterContext
    {
        // Called when an interpolated string is passed.
        // The compiler expands the call and parses/evaluates the interpolated string during that expansion.
    }

    /// <summary>
    /// Renders a template string and stores the result in <paramref name="ctx"/>.
    /// </summary>
    public static void Render<T>(
        T ctx,
        string? template) where T : ITemplateInterpreterContext
    {
        // Called when a plain string (non-interpolated) is passed.
        ctx.Append(template ?? "");
    }

    /// <summary>
    /// Renders a template and returns the result immediately.<br/>
    /// Pass an interpolated string literal ($"...") directly to <paramref name="template"/>.
    /// </summary>
    public static string Render(in InstantTemplateInterpreter template)
    {
        // Called when an interpolated string is passed.
        // The compiler expands the call and parses/evaluates the interpolated string during that expansion.
        return template.GetResult();
    }

    /// <summary>
    /// Renders a template string and returns it immediately.
    /// </summary>
    public static string Render(string? template)
    {
        // Called when a plain string (non-interpolated) is passed.
        // Defined to match the overload signature.
        return template ?? "";
    }

    private const string CSharpSyntaxName = "C#";

    /// <summary>
    /// Renders a template and stores the result in <paramref name="ctx"/>.<br/>
    /// Pass an interpolated string literal ($"...") directly to <paramref name="template"/>.<br/>
    /// The result is stored in <paramref name="ctx"/>.
    /// </summary>
    /// <remarks>
    /// Identical to <see cref="Render{T}(T, in TemplateInterpreter)"/> in behavior, but the argument is
    /// highlighted as C# in IDEs that support <c>[StringSyntax]</c>.
    /// Equivalent to adding <c>//lang=cs</c> before a <c>Render</c> call.
    /// </remarks>
    public static void RenderCSharp<T>(
        T ctx,
        [InterpolatedStringHandlerArgument("ctx")] [StringSyntax(CSharpSyntaxName)]
        in TemplateInterpreter template) where T : ITemplateInterpreterContext
    {
    }

    /// <summary>
    /// Renders a template string and stores the result in <paramref name="ctx"/>.
    /// </summary>
    /// <remarks>
    /// Identical to <see cref="Render{T}(T, string)"/> in behavior, but the argument is
    /// highlighted as C# in IDEs that support <c>[StringSyntax]</c>.
    /// Equivalent to adding <c>//lang=cs</c> before a <c>Render</c> call.
    /// </remarks>
    public static void RenderCSharp<T>(
        T ctx,
        [StringSyntax(CSharpSyntaxName)] string? template) where T : ITemplateInterpreterContext
    {
        ctx.Append(template ?? "");
    }

    /// <summary>
    /// Renders a template and returns the result immediately.<br/>
    /// Pass an interpolated string literal ($"...") directly to <paramref name="template"/>.
    /// </summary>
    /// <remarks>
    /// Identical to <see cref="Render(in InstantTemplateInterpreter)"/> in behavior, but the argument is
    /// highlighted as C# in IDEs that support <c>[StringSyntax]</c>.
    /// Equivalent to adding <c>//lang=cs</c> before a <c>Render</c> call.
    /// </remarks>
    public static string RenderCSharp([StringSyntax(CSharpSyntaxName)] in InstantTemplateInterpreter template)
    {
        return template.GetResult();
    }

    /// <summary>
    /// Renders a template string and returns it immediately.
    /// </summary>
    /// <remarks>
    /// Identical to <see cref="Render(string)"/> in behavior, but the argument is
    /// highlighted as C# in IDEs that support <c>[StringSyntax]</c>.
    /// Equivalent to adding <c>//lang=cs</c> before a <c>Render</c> call.
    /// </remarks>
    public static string RenderCSharp([StringSyntax(CSharpSyntaxName)] string? template)
    {
        return template ?? "";
    }
}
