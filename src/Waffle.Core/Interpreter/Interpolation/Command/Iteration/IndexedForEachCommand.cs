// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Command that begins a ForEach loop block with an index.
/// </summary>
public class IndexedForEachCommand<T> : IterationCommandBase
{
    private readonly IResolvableTo<IEnumerable<T>> _source;
    private readonly int _indexId;

    /// <summary>
    /// Initializes a command that iterates over a concrete sequence with an index token.
    /// </summary>
    public IndexedForEachCommand(
        int id, int indexId, IEnumerable<T> source, out IResolvableTo<T> it, out IntProxy i) : base(id)
    {
        _source = new LiteralProxy<IEnumerable<T>>(source);
        _indexId = indexId;
        it = new IteratorProxy<T>(id);
        i = new IntProxy(new IntIteratorProxy(indexId));
    }

    /// <summary>
    /// Initializes a command that iterates over a lazily resolved sequence with an index token.
    /// </summary>
    public IndexedForEachCommand(
        int id, int indexId, IResolvableTo<IEnumerable<T>> source, out IResolvableTo<T> it, out IntProxy i) : base(id)
    {
        _source = source;
        _indexId = indexId;
        it = new IteratorProxy<T>(id);
        i = new IntProxy(new IntIteratorProxy(indexId));
    }

    internal override void Iterate(Dictionary<int, EnvValue> env, in TemplateEvaluator.IterationBlockEvaluator state)
    {
        var idx = 0;
        foreach (var item in _source.Resolve(env))
        {
            env[Id] = EnvValue.FromObject(item);
            env[_indexId] = EnvValue.FromInt(idx);
            if (state.Evaluate(env, idx == 0) is FlowControl.Break)
            {
                break;
            }

            idx++;
        }

        env.Remove(Id);
        env.Remove(_indexId);
    }
}

/// <summary>
/// Indexed ForEach command specialized for <see cref="IIterationSource{TIterator,TOriginal}"/>.
/// </summary>
public class IndexedForEachCommand<TProxy, TOriginal> : IterationCommandBase
    where TProxy : IResolvableTo<TOriginal>
{
    private readonly IIterationSource<TProxy, TOriginal> _source;
    private readonly int _indexId;

    /// <summary>
    /// Initializes a command that iterates over a typed iteration source with an index token.
    /// </summary>
    public IndexedForEachCommand(
        int id,
        int indexId,
        IIterationSource<TProxy, TOriginal> source,
        out TProxy it,
        out IntProxy i) : base(id)
    {
        _source = source;
        _indexId = indexId;
        it = source.GetIterator(id);
        i = source.GetIteratorIndex(indexId);
    }

    internal override void Iterate(Dictionary<int, EnvValue> env, in TemplateEvaluator.IterationBlockEvaluator state)
    {
        foreach (var (value, index) in _source.GetSource(env))
        {
            env[Id] = EnvValue.FromObject(value);
            env[_indexId] = EnvValue.FromInt(index);
            if (state.Evaluate(env, index == 0) is FlowControl.Break)
            {
                break;
            }
        }

        env.Remove(Id);
        env.Remove(_indexId);
    }
}
