// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Pipeline that unwraps a nested <see cref="IResolvableTo{T}"/>.
/// </summary>
internal class ExtractPipe<T>(IResolvableTo<IResolvableTo<T>> nested) : IResolvableTo<T>, IBlockContent
{
    public T Resolve(Dictionary<int, EnvValue> env)
    {
        return nested.Resolve(env).Resolve(env);
    }

    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        return EvalResult.Create(Resolve(env), alignment, format);
    }
}
