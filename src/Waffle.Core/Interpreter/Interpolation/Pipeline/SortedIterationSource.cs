// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Waffle.Interpreter;

/// <summary>
/// Represents an iteration source after an OrderBy or OrderByDescending sort has been applied,
/// preserving the concrete <typeparamref name="TIterator"/> type.
/// </summary>
public class SortedIterationSource<TIterator, TOriginal> : IIterationSource<TIterator, TOriginal>
    where TIterator : IResolvableTo<TOriginal>, ILazyInitializedBy<TOriginal>, new()
{
    private readonly IIterationSource<TIterator, TOriginal> _source;
    private readonly Func<IReadOnlyList<TOriginal>, IReadOnlyList<TOriginal>> _sort;

    /// <summary>
    /// Initializes a sorted iteration source with the given sort function.
    /// </summary>
    public SortedIterationSource(
        IIterationSource<TIterator, TOriginal> source,
        Func<IReadOnlyList<TOriginal>, IReadOnlyList<TOriginal>> sort)
    {
        _source = source;
        _sort = sort;
    }

    /// <inheritdoc/>
    public IntProxy Count => _source.Count;

    /// <inheritdoc/>
    public TIterator this[int i]
    {
        get
        {
            var ret = new TIterator();
            ret.Initialize(_source.To((f: _sort, i), (it, p) => p.f(it)[p.i]));
            return ret;
        }
    }

    /// <inheritdoc/>
    public TIterator this[IResolvableTo<int> i]
    {
        get
        {
            var ret = new TIterator();
            ret.Initialize(_source.With(i, _sort, (it, k, f) => f(it)[k]));
            return ret;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<(TOriginal Value, int Index)> GetSource(Dictionary<int, EnvValue> env)
    {
        return _sort(_source.Resolve(env)).Select((v, i) => (v, i));
    }

    /// <inheritdoc/>
    public TIterator GetIterator(int id)
    {
        var ret = new TIterator();
        ret.Initialize(new IteratorProxy<TOriginal>(id));
        return ret;
    }

    /// <inheritdoc/>
    public IntProxy GetIteratorIndex(int id)
    {
        return new IntProxy(new IntIteratorProxy(id));
    }

    /// <inheritdoc/>
    public IReadOnlyList<TOriginal> Resolve(Dictionary<int, EnvValue> env)
    {
        return _sort(_source.Resolve(env));
    }
}
