// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Command that begins a ForEach loop block without an index.
/// </summary>
public class ForEachCommand<T> : IterationCommandBase
{
    private readonly IResolvableTo<IEnumerable<T>> _source;

    /// <summary>
    /// Initializes a command that iterates over a concrete sequence.
    /// </summary>
    public ForEachCommand(int id, IEnumerable<T> source, out IResolvableTo<T> it) : base(id)
    {
        _source = new LiteralProxy<IEnumerable<T>>(source);
        it = new IteratorProxy<T>(id);
    }

    /// <summary>
    /// Initializes a command that iterates over a lazily resolved sequence.
    /// </summary>
    public ForEachCommand(int id, IResolvableTo<IEnumerable<T>> source, out IResolvableTo<T> it) : base(id)
    {
        _source = source;
        it = new IteratorProxy<T>(id);
    }

    internal override void Iterate(Dictionary<int, EnvValue> env, in TemplateEvaluator.IterationBlockEvaluator state)
    {
        var isFirst = true;
        foreach (var item in _source.Resolve(env))
        {
            env[Id] = EnvValue.FromObject(item);
            if (state.Evaluate(env, isFirst) is FlowControl.Break)
            {
                break;
            }

            isFirst = false;
        }

        env.Remove(Id);
    }
}

/// <summary>
/// ForEach command specialized for <see cref="IIterationSource{TIterator,TOriginal}"/>.
/// </summary>
public class ForEachCommand<TProxy, TOriginal> : IterationCommandBase
    where TProxy : IResolvableTo<TOriginal>
{
    private readonly IIterationSource<TProxy, TOriginal> _source;

    /// <summary>
    /// Initializes a command that iterates over a typed iteration source.
    /// </summary>
    public ForEachCommand(
        int id, IIterationSource<TProxy, TOriginal> source, out TProxy it) : base(id)
    {
        _source = source;
        it = source.GetIterator(id);
    }

    internal override void Iterate(Dictionary<int, EnvValue> env, in TemplateEvaluator.IterationBlockEvaluator state)
    {
        foreach (var (value, i) in _source.GetSource(env))
        {
            env[Id] = EnvValue.FromObject(value);
            if (state.Evaluate(env, i == 0) is FlowControl.Break)
            {
                break;
            }
        }

        env.Remove(Id);
    }
}
