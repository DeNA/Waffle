// (c) DeNA Co., Ltd.

#pragma warning disable
namespace Polyfills;

using System.Collections.Generic;

static partial class Polyfill
{
    public static void EnsureCapacity<T>(this List<T> target, int capacity)
    {
        if (capacity < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(capacity));
        }

        if (target.Capacity < capacity)
        {
            target.Capacity = capacity;
        }
    }
}
