// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;

namespace Waffle;

/// <summary>
/// Thread-static object pool of <see cref="Stack{T}"/>
/// </summary>
internal static class StackPool<T>
{
    [ThreadStatic]
    private static Stack<Stack<T>>? s_pool;

    /// <summary>
    /// Get an instance from pool.
    /// Returned value must be disposed to return the instance to pool.
    /// </summary>
    public static Releaser Get(out Stack<T> instance)
    {
        s_pool ??= new Stack<Stack<T>>();
        instance = s_pool.TryPop(out var pooled) ? pooled : new Stack<T>();
        return new Releaser(instance);
    }

    /// <summary>
    /// Rents a stack from the pool without requiring a <c>using</c> scope.
    /// The caller must call <see cref="Return"/> when finished.
    /// </summary>
    public static Stack<T> Rent()
    {
        s_pool ??= new Stack<Stack<T>>();
        return s_pool.TryPop(out var pooled) ? pooled : new Stack<T>();
    }

    /// <summary>
    /// Returns a stack previously obtained via <see cref="Rent"/> back to the pool.
    /// </summary>
    public static void Return(Stack<T> instance)
    {
        ReleaseInternal(instance);
    }

    private static void ReleaseInternal(Stack<T> instance)
    {
        s_pool ??= new Stack<Stack<T>>();
        instance.Clear();
        s_pool.Push(instance);
    }

    public readonly ref struct Releaser(Stack<T> instance)
    {
        public void Dispose()
        {
            ReleaseInternal(instance);
        }
    }
}
