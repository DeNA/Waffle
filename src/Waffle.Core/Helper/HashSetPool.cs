// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;

namespace Waffle;

/// <summary>
/// Thread-static object pool of <see cref="HashSet{T}"/>
/// </summary>
internal static class HashSetPool<T>
{
    [ThreadStatic]
    private static Stack<HashSet<T>>? s_pool;

    /// <summary>
    /// Get an instance from pool.
    /// Returned value must be disposed to return the instance to pool.
    /// </summary>
    public static Releaser Get(out HashSet<T> instance)
    {
        s_pool ??= new Stack<HashSet<T>>();
        instance = s_pool.TryPop(out var pooled) ? pooled : [];
        return new Releaser(instance);
    }

    private static void ReleaseInternal(HashSet<T> instance)
    {
        s_pool ??= new Stack<HashSet<T>>();
        instance.Clear();
        s_pool.Push(instance);
    }

    public readonly ref struct Releaser(HashSet<T> instance)
    {
        public void Dispose()
        {
            ReleaseInternal(instance);
        }
    }
}
