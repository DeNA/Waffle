// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Pipeline that combines two objects into a tuple.
/// </summary>
public class CombinePipe<T, U>(IResolvableTo<T> prev1, IResolvableTo<U> prev2) :
    IResolvableTo<(T, U)>, IBlockContent
{
    /// <inheritdoc/>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        return EvalResult.Create(Resolve(env), alignment, format);
    }

    /// <inheritdoc/>
    public (T, U) Resolve(Dictionary<int, EnvValue> env)
    {
        return (prev1.Resolve(env), prev2.Resolve(env));
    }
}
