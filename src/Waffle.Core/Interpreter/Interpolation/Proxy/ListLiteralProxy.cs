// (c) DeNA Co., Ltd.

using System.Collections.Generic;
using System.Linq;

namespace Waffle.Interpreter;

/// <summary>
/// Wraps a concrete <see cref="IReadOnlyList{T}"/> value known at template-build time and exposes it as an
/// <see cref="IIterationSource{TIterator,TElement}"/>. This is a specialization of
/// <see cref="LiteralProxy{T}"/> for list values.
/// </summary>
/// <typeparam name="T">The element type of the list.</typeparam>
public class ListLiteralProxy<T>(IReadOnlyList<T> value)
    : LiteralProxy<IReadOnlyList<T>>(value), IIterationSource<IResolvableTo<T>, T>
{
    /// <summary>
    /// Gets a proxy that resolves to the number of elements in the list.
    /// </summary>
    public IntProxy Count => new(new LiteralProxy<int>(Value.Count));

    /// <summary>
    /// Returns a proxy that resolves to the element at index <paramref name="i"/>.
    /// </summary>
    /// <param name="i">The zero-based index.</param>
    /// <returns>A <see cref="IResolvableTo{T}"/> wrapping the element at the given index.</returns>
    public IResolvableTo<T> this[int i] => new LiteralProxy<T>(Value[i]);

    /// <summary>
    /// Returns a lazy proxy that resolves to the element at the index provided by <paramref name="i"/>.
    /// </summary>
    /// <param name="i">A lazy proxy resolving to the zero-based index.</param>
    /// <returns>A lazy <see cref="IResolvableTo{T}"/> for the element at the resolved index.</returns>
    public IResolvableTo<T> this[IResolvableTo<int> i] => i.Of(Value);

    /// <summary>
    /// Returns the enumerable source of items with their indexes.
    /// </summary>
    /// <param name="env">The environment dictionary containing resolved values.</param>
    /// <returns>An enumerable of <c>(Value, Index)</c> tuples for each element in the list.</returns>
    public IEnumerable<(T Value, int Index)> GetSource(Dictionary<int, EnvValue> env)
    {
        return Value.Select((it, i) => (it, i));
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
