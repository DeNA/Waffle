// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;

namespace Waffle;

internal static class DictionaryPool<TKey, TValue>
{
    [ThreadStatic]
    private static Stack<Dictionary<TKey, TValue>>? s_pool;

    /// <summary>
    /// Get an instance from pool.
    /// Returned value must be disposed to return the instance to pool.
    /// </summary>
    public static Releaser Get(out Dictionary<TKey, TValue> instance)
    {
        s_pool ??= new Stack<Dictionary<TKey, TValue>>();
        instance = s_pool.TryPop(out var pooled) ? pooled : [];
        return new Releaser(instance);
    }

    private static void ReleaseInternal(Dictionary<TKey, TValue> instance)
    {
        s_pool ??= new Stack<Dictionary<TKey, TValue>>();
        instance.Clear();
        s_pool.Push(instance);
    }

    public readonly ref struct Releaser(Dictionary<TKey, TValue> instance)
    {
        public void Dispose()
        {
            ReleaseInternal(instance);
        }
    }
}
