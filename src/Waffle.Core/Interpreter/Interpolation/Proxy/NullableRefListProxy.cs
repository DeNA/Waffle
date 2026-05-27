// (c) DeNA Co., Ltd.

using System.Collections.Generic;
using System.Linq;

namespace Waffle.Interpreter;

/// <summary>
/// Wraps a lazy <see cref="IResolvableTo{T}"/> of <see cref="IReadOnlyList{T}"/> and exposes it as an
/// <see cref="IIterationSource{TIterator,TElement}"/> for use in template loops over nullable-element collections.
/// </summary>
/// <typeparam name="T">The non-nullable element type; the actual elements are <typeparamref name="T"/>? and may be null.</typeparam>
public class NullableRefListProxy<T>(IResolvableTo<IReadOnlyList<T?>> list) :
    IIterationSource<IResolvableTo<T?>, T?>, IBlockContent
    where T : class
{
    /// <inheritdoc/>
    public IReadOnlyList<T?> Resolve(Dictionary<int, EnvValue> env)
    {
        return list.Resolve(env);
    }

    /// <summary>Gets a lazy proxy that resolves to the number of elements in the list.</summary>
    public IntProxy Count => new(list.To(ls => ls.Count));

    /// <summary>Returns a lazy proxy that resolves to the element at index <paramref name="i"/>.</summary>
    public IResolvableTo<T?> this[int i] => list.To(i, (ls, idx) => ls[idx]);

    /// <summary>Returns a lazy proxy that resolves to the element at the index provided by <paramref name="i"/>.</summary>
    public IResolvableTo<T?> this[IResolvableTo<int> i] => list.With(i, (ls, k) => ls[k]);

    /// <inheritdoc/>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        // null elements render as empty string (same as string.Join behaviour for null entries)
        return EvalResult.Create(string.Join("\n", Resolve(env)), alignment, format);
    }

    /// <inheritdoc/>
    public IEnumerable<(T? Value, int Index)> GetSource(Dictionary<int, EnvValue> env)
    {
        return list.Resolve(env).Select((it, i) => (it, i));
    }

    /// <summary>Creates a <see cref="NullableRefIteratorProxy{T}"/> for the loop variable identified by <paramref name="id"/>.</summary>
    public IResolvableTo<T?> GetIterator(int id)
    {
        return new NullableRefIteratorProxy<T>(id);
    }

    /// <inheritdoc/>
    public IntProxy GetIteratorIndex(int id)
    {
        return new IntProxy(new IntIteratorProxy(id));
    }
}
