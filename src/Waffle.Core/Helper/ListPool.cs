// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;

namespace Waffle;

/// <summary>
/// Thread-static object pool of <see cref="List{T}"/>
/// </summary>
internal static class ListPool<T>
{
    [ThreadStatic]
    private static Stack<List<T>>? s_pool;

    /// <summary>
    /// Get an instance from pool.
    /// Returned value must be disposed to return the instance to pool.
    /// </summary>
    public static Releaser Get(out List<T> instance, int capacity = 0)
    {
        s_pool ??= new Stack<List<T>>();
        if (s_pool.TryPop(out var pooled))
        {
            if (capacity > pooled.Capacity)
            {
                pooled.Capacity = capacity;
            }

            instance = pooled;
        }
        else
        {
            instance = new List<T>(capacity > 0 ? capacity : 8);
        }

        return new Releaser(instance);
    }

    /// <summary>
    /// Rents a list from the pool without requiring a <c>using</c> scope.
    /// The caller must call <see cref="Return"/> when finished.
    /// </summary>
    public static List<T> Rent(int capacity = 0)
    {
        s_pool ??= new Stack<List<T>>();
        if (s_pool.TryPop(out var pooled))
        {
            if (capacity > pooled.Capacity)
            {
                pooled.Capacity = capacity;
            }

            return pooled;
        }

        return new List<T>(capacity > 0 ? capacity : 8);
    }

    /// <summary>
    /// Returns a list previously obtained via <see cref="Rent"/> back to the pool.
    /// </summary>
    public static void Return(List<T> instance)
    {
        ReleaseInternal(instance);
    }

    private static void ReleaseInternal(List<T> instance)
    {
        s_pool ??= new Stack<List<T>>();
        instance.Clear();
        s_pool.Push(instance);
    }

    public readonly ref struct Releaser(List<T> instance)
    {
        public void Dispose()
        {
            ReleaseInternal(instance);
        }
    }
}
