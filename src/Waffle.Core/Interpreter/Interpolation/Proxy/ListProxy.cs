// (c) DeNA Co., Ltd.

using System.Collections.Generic;
using System.Linq;

namespace Waffle.Interpreter;

/// <summary>
/// Wraps a lazy <see cref="IResolvableTo{T}"/> of <see cref="IReadOnlyList{T}"/> and exposes it as an
/// <see cref="IIterationSource{TIterator,TElement}"/> and <see cref="IBlockContent"/> for use in template loops.
/// The underlying list is resolved from the environment at render time.
/// </summary>
/// <typeparam name="T">The element type of the list.</typeparam>
public class ListProxy<T>(IResolvableTo<IReadOnlyList<T>> list) :
    IIterationSource<IResolvableTo<T>, T>, IBlockContent
{
    /// <summary>
    /// Resolves the underlying list from the current environment.
    /// </summary>
    /// <param name="env">The environment dictionary containing resolved values.</param>
    /// <returns>The resolved <see cref="IReadOnlyList{T}"/>.</returns>
    public IReadOnlyList<T> Resolve(Dictionary<int, EnvValue> env)
    {
        return list.Resolve(env);
    }

    /// <summary>
    /// Gets a lazy proxy that resolves to the number of elements in the list.
    /// </summary>
    public IntProxy Count => new(list.To(ls => ls.Count));

    /// <summary>
    /// Returns a lazy proxy that resolves to the element at index <paramref name="i"/>.
    /// </summary>
    /// <param name="i">The zero-based index.</param>
    /// <returns>A lazy <see cref="IResolvableTo{T}"/> for the element at the given index.</returns>
    public IResolvableTo<T> this[int i] => list.To(i, (ls, idx) => ls[idx]);

    /// <summary>
    /// Returns a lazy proxy that resolves to the element at the index provided by <paramref name="i"/>.
    /// </summary>
    /// <param name="i">A lazy proxy resolving to the zero-based index.</param>
    /// <returns>A lazy <see cref="IResolvableTo{T}"/> for the element at the resolved index.</returns>
    public IResolvableTo<T> this[IResolvableTo<int> i] => list.With(i, (ls, k) => ls[k]);


    /// <summary>
    /// Resolves the list and formats it as a newline-joined string.
    /// </summary>
    /// <param name="env">The environment dictionary containing resolved values.</param>
    /// <param name="alignment">The alignment specifier for string formatting.</param>
    /// <param name="format">The format string.</param>
    /// <returns>A <see cref="EvalResult"/> representing the formatted list.</returns>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        return EvalResult.Create(string.Join("\n", Resolve(env)), alignment, format);
    }

    /// <summary>
    /// Returns the enumerable source of items with their indexes.
    /// </summary>
    /// <param name="env">The environment dictionary containing resolved values.</param>
    /// <returns>An enumerable of <c>(Value, Index)</c> tuples for each element in the list.</returns>
    public IEnumerable<(T Value, int Index)> GetSource(Dictionary<int, EnvValue> env)
    {
        return list.Resolve(env).Select((it, i) => (it, i));
    }

    /// <summary>
    /// Creates an iterator proxy for the loop variable identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The environment key that will hold the current element value.</param>
    /// <returns>A lazy <see cref="IResolvableTo{T}"/> that resolves to the current element.</returns>
    public IResolvableTo<T> GetIterator(int id)
    {
        return new IteratorProxy<T>(id);
    }

    /// <summary>
    /// Creates an integer iterator proxy for the loop index identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The environment key that will hold the current loop index.</param>
    /// <returns>An <see cref="IntProxy"/> that resolves to the current zero-based loop index.</returns>
    public IntProxy GetIteratorIndex(int id)
    {
        return new IntProxy(new IntIteratorProxy(id));
    }
}
