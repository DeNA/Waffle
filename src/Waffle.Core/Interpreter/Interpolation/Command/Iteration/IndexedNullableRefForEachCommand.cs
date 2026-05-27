// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Command that begins an indexed ForEach loop block over a nullable-element collection.
/// Each element resolves to <typeparamref name="T"/>?, allowing null elements to be iterated safely.
/// </summary>
public class IndexedNullableRefForEachCommand<T> : IterationCommandBase
    where T : class
{
    private readonly IResolvableTo<IEnumerable<T?>> _source;
    private readonly int _indexId;

    /// <summary>
    /// Initializes a command that iterates over a concrete nullable-reference sequence with an index token.
    /// </summary>
    public IndexedNullableRefForEachCommand(
        int id, int indexId, IEnumerable<T?>? source, out IResolvableTo<T?> it, out IntProxy i) : base(id)
    {
        _source = new LiteralProxy<IEnumerable<T?>>(source ?? []);
        _indexId = indexId;
        it = new NullableRefIteratorProxy<T>(id);
        i = new IntProxy(new IntIteratorProxy(indexId));
    }

    /// <summary>
    /// Initializes a command that iterates over a lazily resolved nullable-reference sequence with an index token.
    /// </summary>
    public IndexedNullableRefForEachCommand(
        int id, int indexId, IResolvableTo<IEnumerable<T?>> source, out IResolvableTo<T?> it, out IntProxy i) : base(id)
    {
        _source = source;
        _indexId = indexId;
        it = new NullableRefIteratorProxy<T>(id);
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
