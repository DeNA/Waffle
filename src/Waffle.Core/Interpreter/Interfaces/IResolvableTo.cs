// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;
using Waffle.Interpreter;

namespace Waffle;

/// <summary>
/// An object that is lazily-resolved to type <typeparamref name="T" /> when a template is evaluated.
/// </summary>
public interface IResolvableTo<out T>
{
    /// <summary>
    /// Resolves the value using the current template environment.
    /// </summary>
    T Resolve(Dictionary<int, EnvValue> env);
}

/// <summary>
/// Provides extension methods for composing lazily resolved values.
/// </summary>
public static class ResolvableToExtensions
{
    extension<T>(IResolvableTo<T> self)
    {
        /// <summary>
        /// Projects the resolved value to another object (e.g. member access, method call).
        /// </summary>
        public IResolvableTo<U> To<U>(Func<T, U> selector)
        {
            return new SelectPipe<T, U>(self, selector);
        }

        /// <summary>
        /// Projects the resolved value to another object (e.g. member access, method call).
        /// </summary>
        public IResolvableTo<U> To<TCtx, U>(TCtx ctx, Func<T, TCtx, U> selector)
        {
            return new SelectPipe<T, TCtx, U>(self, ctx, selector);
        }

        /// <summary>
        /// Combines this instance with another resolvable to form a tuple.
        /// </summary>
        public IResolvableTo<(T, U)> With<U>(IResolvableTo<U> another)
        {
            return new CombinePipe<T, U>(self, another);
        }

        /// <summary>
        /// Projects this resolved value together with another resolvable value using a selector.
        /// </summary>
        public IResolvableTo<V> With<U, V>(IResolvableTo<U> another, Func<T, U, V> selector)
        {
            return self.With(another).To(selector);
        }

        /// <summary>
        /// Projects this resolved value together with another resolvable value using a selector.
        /// </summary>
        public IResolvableTo<V> With<U, TCtx, V>(IResolvableTo<U> another, TCtx ctx, Func<T, U, TCtx, V> selector)
        {
            return self.With(another).To(ctx, selector);
        }

        /// <summary>
        /// Projects this resolved value together with two other resolvable values using a selector.
        /// </summary>
        public IResolvableTo<W> With<U1, U2, W>(IResolvableTo<U1> a1, IResolvableTo<U2> a2, Func<T, U1, U2, W> selector)
        {
            return self.With(a1).With(a2, (t, p2) => selector(t.Item1, t.Item2, p2));
        }

        /// <summary>
        /// Projects this resolved value together with three other resolvable values using a selector.
        /// </summary>
        public IResolvableTo<X> With<U1, U2, U3, X>(IResolvableTo<U1> a1, IResolvableTo<U2> a2, IResolvableTo<U3> a3,
            Func<T, U1, U2, U3, X> selector)
        {
            // Chain single-arg With to avoid overload ambiguity; t = ((T, U1), U2)
            return self.With(a1).With(a2).With(a3, (t, p3) => selector(t.Item1.Item1, t.Item1.Item2, t.Item2, p3));
        }

        /// <summary>
        /// Projects this resolved value together with four other resolvable values using a selector.
        /// </summary>
        public IResolvableTo<Y> With<U1, U2, U3, U4, Y>(IResolvableTo<U1> a1, IResolvableTo<U2> a2,
            IResolvableTo<U3> a3, IResolvableTo<U4> a4, Func<T, U1, U2, U3, U4, Y> selector)
        {
            // Chain single-arg With to avoid overload ambiguity; t = (((T, U1), U2), U3)
            return self.With(a1).With(a2).With(a3).With(a4,
                (t, p4) => selector(t.Item1.Item1.Item1, t.Item1.Item1.Item2, t.Item1.Item2, t.Item2, p4));
        }
    }

    extension<T, U>(IResolvableTo<(T, U)> self)
    {
        /// <summary>
        /// Projects the resolved tuple to another object.
        /// </summary>
        public IResolvableTo<V> To<V>(Func<T, U, V> selector)
        {
            return new SelectPipe<(T, U), Func<T, U, V>, V>(self, selector, (pair, s) => s(pair.Item1, pair.Item2));
        }

        /// <summary>
        /// Projects the resolved tuple to another object.
        /// </summary>
        public IResolvableTo<V> To<TCtx, V>(TCtx ctx, Func<T, U, TCtx, V> selector)
        {
            return new SelectPipe<(T, U), (TCtx Ctx, Func<T, U, TCtx, V> Selector), V>(self, (ctx, selector),
                (pair, c) => c.Selector(pair.Item1, pair.Item2, c.Ctx));
        }
    }

    /// <summary>
    /// Uses the resolved integer as an index to retrieve an element from the specified list.
    /// </summary>
    public static IResolvableTo<T> Of<T>(this IResolvableTo<int> self, IReadOnlyList<T> list)
    {
        return self.To(list, (i, ls) => ls[i]);
    }

    /// <summary>
    /// Flattens a nested IResolvableTo, unwrapping one level of indirection.
    /// </summary>
    public static IResolvableTo<T> Extract<T>(this IResolvableTo<IResolvableTo<T>> self)
    {
        return new ExtractPipe<T>(self);
    }

    /// <summary>
    /// Returns a new resolvable string with all occurrences of <paramref name="oldValue"/> replaced by <paramref name="newValue"/>.
    /// </summary>
    public static IResolvableTo<string> Replace(this IResolvableTo<string> self, string oldValue, string newValue)
    {
        return self.To((oldValue, newValue), (it, ctx) => it.Replace(ctx.oldValue, ctx.newValue));
    }
}
