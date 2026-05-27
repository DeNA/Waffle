// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Pipeline that maps one object to another using a selector function.
/// </summary>
public class SelectPipe<T, U>(IResolvableTo<T> prev, Func<T, U> selector) : IResolvableTo<U>, IBlockContent
{
    /// <inheritdoc/>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        return EvalResult.Create(selector(prev.Resolve(env)), alignment, format);
    }

    /// <inheritdoc/>
    public U Resolve(Dictionary<int, EnvValue> env)
    {
        return selector(prev.Resolve(env));
    }
}

/// <summary>
/// Pipeline that maps one object to another using a selector function and an additional context value.
/// </summary>
public class SelectPipe<T, TContext, U>(IResolvableTo<T> prev, TContext ctx, Func<T, TContext, U> selector) :
    IResolvableTo<U>, IBlockContent
{
    /// <inheritdoc/>
    public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
    {
        return EvalResult.Create(selector(prev.Resolve(env), ctx), alignment, format);
    }

    /// <inheritdoc/>
    public U Resolve(Dictionary<int, EnvValue> env)
    {
        return selector(prev.Resolve(env), ctx);
    }
}
