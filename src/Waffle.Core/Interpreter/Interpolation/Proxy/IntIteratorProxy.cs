// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// A non-generic iterator proxy for integer loop variables.
/// Reads the current loop value directly from <see cref="EnvValue.AsInt()"/> to avoid boxing.
/// Used exclusively by <see cref="ForCommand"/> which stores loop counters as <see cref="EnvValue.FromInt"/>.
/// </summary>
public class IntIteratorProxy(int parentId) : IBlockContent, IResolvableTo<int>
{
    /// <inheritdoc/>
    public int Resolve(Dictionary<int, EnvValue> env)
    {
        return env.GetLoopVariable(parentId).AsInt();
    }

    /// <inheritdoc/>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        return EvalResult.Create(env.GetLoopVariable(parentId).AsInt(), alignment, format);
    }
}
