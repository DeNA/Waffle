// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;
using System.Text;

namespace Waffle;

/// <summary>
/// Thread-static object pool of <see cref="StringBuilder"/>
/// </summary>
internal static class StringBuilderPool
{
    [ThreadStatic]
    private static Stack<StringBuilder>? s_pool;

    /// <summary>
    /// Get an instance from pool.
    /// Returned value must be disposed to return the instance to pool.
    /// </summary>
    public static Releaser Get(out StringBuilder instance)
    {
        s_pool ??= new Stack<StringBuilder>();
        instance = s_pool.TryPop(out var pooled) ? pooled : new StringBuilder(512);
        return new Releaser(instance);
    }

    private static void ReleaseInternal(StringBuilder instance)
    {
        s_pool ??= new Stack<StringBuilder>();
        instance.Clear();
        s_pool.Push(instance);
    }

    public readonly struct Releaser(StringBuilder instance) : IDisposable
    {
        public void Dispose()
        {
            ReleaseInternal(instance);
        }
    }
}
