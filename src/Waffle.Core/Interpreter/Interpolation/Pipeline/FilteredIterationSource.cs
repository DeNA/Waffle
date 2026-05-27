// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Waffle.Interpreter;

/// <summary>
/// Represents an iteration source after a Where filter has been applied.
/// </summary>
public class FilteredIterationSource<TIterator, TOriginal> : IIterationSource<TIterator, TOriginal>
    where TIterator : IResolvableTo<TOriginal>, ILazyInitializedBy<TOriginal>, new()
{
    private readonly IIterationSource<TIterator, TOriginal> _source;
    private readonly Func<TOriginal, int, bool> _predicate;

    /// <summary>
    /// Initializes a filtered iteration source with an element predicate.
    /// </summary>
    public FilteredIterationSource(IIterationSource<TIterator, TOriginal> source, Func<TOriginal, bool> predicate)
    {
        _source = source;
        _predicate = (it, _) => predicate(it);
    }

    /// <summary>
    /// Initializes a filtered iteration source with an element-and-index predicate.
    /// </summary>
    public FilteredIterationSource(IIterationSource<TIterator, TOriginal> source, Func<TOriginal, int, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
    }

    /// <inheritdoc/>
    public IntProxy Count => new(_source.To(_predicate, (s, p) => s.Where(p).Count()));

    /// <inheritdoc/>
    public TIterator this[int i]
    {
        get
        {
            var ret = new TIterator();
            ret.Initialize(_source.To((p: _predicate, i), (s, c) => s.Where(c.p).ElementAt(c.i)));
            return ret;
        }
    }

    /// <inheritdoc/>
    public TIterator this[IResolvableTo<int> i]
    {
        get
        {
            var ret = new TIterator();
            ret.Initialize(_source.With(i, _predicate, (s, k, p) => s.Where(p).ElementAt(k)));
            return ret;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<(TOriginal Value, int Index)> GetSource(Dictionary<int, EnvValue> env)
    {
        return _source.GetSource(env).Where(it => _predicate(it.Value, it.Index)).Select((it, i) => (it.Value, i));
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
        return _source.Resolve(env).Where(_predicate).ToArray();
    }
}
