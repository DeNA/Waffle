// (c) DeNA Co., Ltd.

#pragma warning disable
namespace Polyfills;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

static partial class Polyfill
{
    extension<T>(Stack<T> target)
    {
        public bool TryPeek([MaybeNullWhen(false)] out T result)
        {
            if (target.Count > 0)
            {
                result = target.Peek();
                return true;
            }

            result = default;
            return false;
        }

        public bool TryPop([MaybeNullWhen(false)] out T result)
        {
            if (target.Count > 0)
            {
                result = target.Pop();
                return true;
            }

            result = default;
            return false;
        }
    }
}
