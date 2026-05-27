// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Command that begins a ForEach loop block over a nullable-element collection.
/// Each element resolves to <typeparamref name="T"/>?, allowing null elements to be iterated safely.
/// </summary>
public class NullableRefForEachCommand<T> : IterationCommandBase
    where T : class
{
    private readonly IResolvableTo<IEnumerable<T?>> _source;

    /// <summary>
    /// Initializes a command that iterates over a concrete nullable-reference sequence.
    /// </summary>
    public NullableRefForEachCommand(int id, IEnumerable<T?>? source, out IResolvableTo<T?> it) : base(id)
    {
        _source = new LiteralProxy<IEnumerable<T?>>(source ?? []);
        it = new NullableRefIteratorProxy<T>(id);
    }

    /// <summary>
    /// Initializes a command that iterates over a lazily resolved nullable-reference sequence.
    /// </summary>
    public NullableRefForEachCommand(int id, IResolvableTo<IEnumerable<T?>> source, out IResolvableTo<T?> it) : base(id)
    {
        _source = source;
        it = new NullableRefIteratorProxy<T>(id);
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
