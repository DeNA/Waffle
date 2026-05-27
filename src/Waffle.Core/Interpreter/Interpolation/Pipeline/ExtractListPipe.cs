// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Pipeline that unwraps a nested <see cref="IResolvableTo{T}"/> containing a list.
/// </summary>
internal class ExtractListPipe<T>(IResolvableTo<IReadOnlyList<IResolvableTo<T>>> source)
    : IResolvableTo<IReadOnlyList<T>>, IBlockContent
{
    public IReadOnlyList<T> Resolve(Dictionary<int, EnvValue> env)
    {
        var ls = source.Resolve(env);
        var ret = new T[ls.Count];
        for (var i = 0; i < ls.Count; i++)
        {
            ret[i] = ls[i].Resolve(env);
        }

        return ret;
    }

    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        return EvalResult.Create(string.Join("\n", Resolve(env)), alignment, format);
    }
}
