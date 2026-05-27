// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;
using System.Linq;
using Waffle.Interpreter;

namespace Waffle;

/// <summary>
/// A list that explicitly specifies the iterator type used in ForEach.
/// </summary>
/// <typeparam name="TIterator">Iterator type for ForEach.</typeparam>
/// <typeparam name="TOriginal">The resolved type obtained by evaluating the ForEach iterator.</typeparam>
public interface IIterationSource<out TIterator, TOriginal> : IResolvableTo<IReadOnlyList<TOriginal>>
    where TIterator : IResolvableTo<TOriginal>
{
    /// <summary>
    /// Number of elements.
    /// </summary>
    IntProxy Count { get; }

    /// <summary>
    /// Direct indexer.
    /// </summary>
    TIterator this[int i] { get; }

    /// <summary>
    /// Lazily-resolved indexer.
    /// </summary>
    TIterator this[IResolvableTo<int> i] { get; }

    /// <summary>
    /// Returns the enumerable source of items with their indexes (zero-based, sequential)
    /// </summary>
    IEnumerable<(TOriginal Value, int Index)> GetSource(Dictionary<int, EnvValue> env);

    /// <summary>
    /// Creates the iterator instance yielded per element during a ForEach loop.
    /// </summary>
    TIterator GetIterator(int id);

    /// <summary>
    /// Creates the index instance yielded per iteration during an indexed ForEach loop.
    /// </summary>
    IntProxy GetIteratorIndex(int id);
}

/// <summary>
/// Provides extension methods for working with iteration sources.
/// </summary>
public static class IterationSourceExtensions
{
    /// <summary>
    /// Converts the sequence to a lazily-resolved list for use in templates.
    /// </summary>
    public static ListLiteralProxy<T> AsProxy<T>(this IEnumerable<T> self)
    {
        return new ListLiteralProxy<T>(self as IReadOnlyList<T> ?? self.ToArray());
    }

    /// <summary>
    /// Wraps the resolvable IEnumerable in a list that supports iterators.
    /// </summary>
    public static ListProxy<T> AsProxy<T>(this IResolvableTo<IEnumerable<T>> self)
    {
        return new ListProxy<T>(self.To(x => x as IReadOnlyList<T> ?? x.ToArray()));
    }

    /// <summary>
    /// Wraps the resolvable IReadOnlyList in a list that supports iterators.
    /// </summary>
    public static ListProxy<T> AsProxy<T>(this IResolvableTo<IReadOnlyList<T>> self)
    {
        return new ListProxy<T>(self);
    }

    /// <summary>
    /// Projects each element of the iteration source into a new form using the specified selector.
    /// </summary>
    public static ListProxy<U> Select<TIterator, TOriginal, U>(
        this IIterationSource<TIterator, TOriginal> self,
        Func<TOriginal, U> selector)
        where TIterator : IResolvableTo<TOriginal>
    {
        return self.To(selector, (ls, s) => ls.Select(s)).AsProxy();
    }

    /// <summary>
    /// Projects each element of the iteration source into a new form, incorporating the element's index.
    /// </summary>
    public static ListProxy<U> Select<TIterator, TOriginal, U>(
        this IIterationSource<TIterator, TOriginal> self,
        Func<TOriginal, int, U> selector)
        where TIterator : IResolvableTo<TOriginal>
    {
        return self.To(selector, (ls, s) => ls.Select(s)).AsProxy();
    }

    /// <summary>
    /// Projects each element of the iteration source into a lazily-resolved form using the specified selector.
    /// </summary>
    public static ListProxy<U> Select<TIterator, TOriginal, U>(
        this IIterationSource<TIterator, TOriginal> self,
        Func<TOriginal, IResolvableTo<U>> selector)
        where TIterator : IResolvableTo<TOriginal>
    {
        return new ExtractListPipe<U>(self.To(selector, (it, s) => it.Select(s).ToArray())).AsProxy();
    }

    /// <summary>
    /// Projects each element of the iteration source into a lazily-resolved form, incorporating the element's index.
    /// </summary>
    public static ListProxy<U> Select<TIterator, TOriginal, U>(
        this IIterationSource<TIterator, TOriginal> self,
        Func<TOriginal, int, IResolvableTo<U>> selector)
        where TIterator : IResolvableTo<TOriginal>
    {
        return new ExtractListPipe<U>(self.To(selector, (it, s) => it.Select(s).ToArray())).AsProxy();
    }

    /// <summary>
    /// Filters the iteration source, preserving the concrete <typeparamref name="TIterator"/> type.
    /// </summary>
    public static IIterationSource<TIterator, TOriginal> Where<TIterator, TOriginal>(
        this IIterationSource<TIterator, TOriginal> self,
        Func<TOriginal, bool> predicate)
        where TIterator : IResolvableTo<TOriginal>, ILazyInitializedBy<TOriginal>, new()
    {
        // NOTE: Where uses a dedicated class to preserve TIterator type information as much as possible.
        return new FilteredIterationSource<TIterator, TOriginal>(self, predicate);
    }

    /// <summary>
    /// Filters the iteration source by element and index, preserving the concrete <typeparamref name="TIterator"/> type.
    /// </summary>
    public static IIterationSource<TIterator, TOriginal> Where<TIterator, TOriginal>(
        this IIterationSource<TIterator, TOriginal> self,
        Func<TOriginal, int, bool> predicate)
        where TIterator : IResolvableTo<TOriginal>, ILazyInitializedBy<TOriginal>, new()
    {
        // NOTE: Where uses a dedicated class to preserve TIterator type information as much as possible.
        return new FilteredIterationSource<TIterator, TOriginal>(self, predicate);
    }

    /// <summary>
    /// Filters the iteration source when <typeparamref name="TOriginal"/> iterator is not a concrete type.
    /// </summary>
    public static ListProxy<TOriginal> Where<TOriginal>(
        this IIterationSource<IResolvableTo<TOriginal>, TOriginal> self,
        Func<TOriginal, bool> predicate)
    {
        // NOTE: This overload is called when TIterator is not a concrete type.
        return self.To(predicate, (it, p) => it.Where(p)).AsProxy();
    }

    /// <summary>
    /// Filters the iteration source by element and index when <typeparamref name="TOriginal"/> iterator is not a concrete type.
    /// </summary>
    public static ListProxy<TOriginal> Where<TOriginal>(
        this IIterationSource<IResolvableTo<TOriginal>, TOriginal> self,
        Func<TOriginal, int, bool> predicate)
    {
        // NOTE: This overload is called when TIterator is not a concrete type.
        return self.To(predicate, (it, p) => it.Where(p)).AsProxy();
    }

    /// <summary>
    /// Sorts the elements of the iteration source in ascending order according to a key,
    /// preserving the concrete <typeparamref name="TIterator"/> type.
    /// </summary>
    public static IIterationSource<TIterator, TOriginal> OrderBy<TIterator, TOriginal, TKey>(
        this IIterationSource<TIterator, TOriginal> self,
        Func<TOriginal, TKey> keySelector)
        where TIterator : IResolvableTo<TOriginal>, ILazyInitializedBy<TOriginal>, new()
    {
        return new SortedIterationSource<TIterator, TOriginal>(self, ls => ls.OrderBy(keySelector).ToArray());
    }

    /// <summary>
    /// Sorts the elements of the iteration source in descending order according to a key,
    /// preserving the concrete <typeparamref name="TIterator"/> type.
    /// </summary>
    public static IIterationSource<TIterator, TOriginal> OrderByDescending<TIterator, TOriginal, TKey>(
        this IIterationSource<TIterator, TOriginal> self,
        Func<TOriginal, TKey> keySelector)
        where TIterator : IResolvableTo<TOriginal>, ILazyInitializedBy<TOriginal>, new()
    {
        return new SortedIterationSource<TIterator, TOriginal>(self, ls => ls.OrderByDescending(keySelector).ToArray());
    }

    /// <summary>
    /// Sorts the elements of the iteration source in ascending order according to a key.
    /// </summary>
    public static ListProxy<TOriginal> OrderBy<TOriginal, TKey>(
        this IIterationSource<IResolvableTo<TOriginal>, TOriginal> self,
        Func<TOriginal, TKey> keySelector)
    {
        return self.To(keySelector, (it, s) => it.OrderBy(s)).AsProxy();
    }

    /// <summary>
    /// Sorts the elements of the iteration source in descending order according to a key.
    /// </summary>
    public static ListProxy<TOriginal> OrderByDescending<TOriginal, TKey>(
        this IIterationSource<IResolvableTo<TOriginal>, TOriginal> self,
        Func<TOriginal, TKey> keySelector)
    {
        return self.To(keySelector, (it, s) => it.OrderByDescending(s)).AsProxy();
    }

    /// <summary>
    /// Bypasses the first <paramref name="count"/> elements and returns the remaining elements,
    /// preserving the concrete <typeparamref name="TIterator"/> type.
    /// </summary>
    public static IIterationSource<TIterator, TOriginal> Skip<TIterator, TOriginal>(
        this IIterationSource<TIterator, TOriginal> self,
        int count)
        where TIterator : IResolvableTo<TOriginal>, ILazyInitializedBy<TOriginal>, new()
    {
        return self.Where((_, i) => i >= count);
    }

    /// <summary>
    /// Bypasses the first <paramref name="count"/> elements and returns the remaining elements as a <see cref="ListProxy{T}"/>.
    /// </summary>
    public static ListProxy<TOriginal> Skip<TOriginal>(
        this IIterationSource<IResolvableTo<TOriginal>, TOriginal> self,
        int count)
    {
        return self.To(count, (ls, c) => ls.Skip(c)).AsProxy();
    }

    /// <summary>
    /// Bypasses elements based on a lazily-resolved count and returns the remaining elements.
    /// </summary>
    public static ListProxy<TOriginal> Skip<TOriginal>(
        this IIterationSource<IResolvableTo<TOriginal>, TOriginal> self,
        IResolvableTo<int> count)
    {
        return self.With(count, (ls, c) => ls.Skip(c)).AsProxy();
    }

    /// <summary>
    /// Returns the first <paramref name="count"/> elements,
    /// preserving the concrete <typeparamref name="TIterator"/> type.
    /// </summary>
    public static IIterationSource<TIterator, TOriginal> Take<TIterator, TOriginal>(
        this IIterationSource<TIterator, TOriginal> self,
        int count)
        where TIterator : IResolvableTo<TOriginal>, ILazyInitializedBy<TOriginal>, new()
    {
        return self.Where((_, i) => i < count);
    }

    /// <summary>
    /// Returns the first <paramref name="count"/> elements as a <see cref="ListProxy{T}"/>.
    /// </summary>
    public static ListProxy<TOriginal> Take<TOriginal>(
        this IIterationSource<IResolvableTo<TOriginal>, TOriginal> self,
        int count)
    {
        return self.To(count, (ls, c) => ls.Take(c)).AsProxy();
    }

    /// <summary>
    /// Returns elements up to a lazily-resolved count as a <see cref="ListProxy{T}"/>.
    /// </summary>
    public static ListProxy<TOriginal> Take<TIterator, TOriginal>(
        this IIterationSource<TIterator, TOriginal> self,
        IResolvableTo<int> count)
        where TIterator : IResolvableTo<TOriginal>
    {
        return self.With(count, (ls, c) => ls.Take(c)).AsProxy();
    }

    /// <summary>
    /// Concatenates the iteration source with another resolvable list of the same element type.
    /// </summary>
    public static ListProxy<TOriginal> Concat<TIterator, TOriginal>(
        this IIterationSource<TIterator, TOriginal> self,
        IResolvableTo<IReadOnlyList<TOriginal>> other)
        where TIterator : IResolvableTo<TOriginal>
    {
        return self.With(other, (s, o) => s.Concat(o)).AsProxy();
    }


    /// <summary>
    /// Creates a new list containing only the elements at the specified indexes.
    /// </summary>
    public static IIterationSource<TIterator, TOriginal> Pick<TIterator, TOriginal>(
        this IIterationSource<TIterator, TOriginal> self,
        params int[] indexes)
        where TIterator : IResolvableTo<TOriginal>, ILazyInitializedBy<TOriginal>, new()
    {
        return self.Where((_, i) => indexes.Contains(i));
    }

    /// <summary>
    /// Creates a new list containing only the elements at the specified indexes.
    /// </summary>
    public static ListProxy<TOriginal> Pick<TOriginal>(
        this IIterationSource<IResolvableTo<TOriginal>, TOriginal> self,
        params int[] indexes)
    {
        return self.To(indexes, (it, idxes) => idxes.Select(i => it[i])).AsProxy();
    }

    /// <summary>
    /// Returns the zero-based index of the first occurrence of <paramref name="value"/> in the iteration source,
    /// or <c>-1</c> if not found.
    /// </summary>
    public static IntProxy IndexOf<TIterator, TOriginal>(
        this IIterationSource<TIterator, TOriginal> self,
        TOriginal value)
        where TIterator : IResolvableTo<TOriginal>
    {
        return new IntProxy(self.To(it => it.IndexOfInternal(value)));
    }

    /// <summary>
    /// Returns the zero-based index of the first occurrence of the lazily-resolved <paramref name="value"/> in the iteration source,
    /// or <c>-1</c> if not found.
    /// </summary>
    public static IntProxy IndexOf<TIterator, TOriginal>(
        this IIterationSource<TIterator, TOriginal> self,
        IResolvableTo<TOriginal> value)
        where TIterator : IResolvableTo<TOriginal>
    {
        return new IntProxy(self.With(value, (it, v) => it.IndexOfInternal(v)));
    }

    /// <summary>
    /// Joins the list elements into a single string using the specified separator.
    /// </summary>
    public static StringProxy Join<TIterator, TOriginal>(
        this IIterationSource<TIterator, TOriginal> self,
        string separator, Func<TOriginal, string>? toString = null)
        where TIterator : IResolvableTo<TOriginal>
    {
        return new StringProxy(
            self.To((separator, toString),
                (ls, c) => string.Join(c.separator, ls.Select(c.toString ?? (it => it?.ToString() ?? "")))));
    }

    /// <summary>
    /// Joins the list elements into a single string using the specified separator,
    /// wrapping the result with <paramref name="prefix"/> and <paramref name="suffix"/>.
    /// When the list is empty, returns an empty string (prefix and suffix are omitted).
    /// </summary>
    public static StringProxy Join<TIterator, TOriginal>(
        this IIterationSource<TIterator, TOriginal> self,
        string separator, string prefix, string suffix,
        Func<TOriginal, string>? toString = null)
        where TIterator : IResolvableTo<TOriginal>
    {
        return new StringProxy(
            self.To((separator, prefix, suffix, toString),
                (ls, c) => ls.Count == 0
                    ? ""
                    : c.prefix
                      + string.Join(c.separator, ls.Select(c.toString ?? (it => it?.ToString() ?? "")))
                      + c.suffix));
    }

    /// <summary>
    /// Returns whether the list is empty.
    /// </summary>
    public static BoolProxy IsEmpty<TIterator, TOriginal>(this IIterationSource<TIterator, TOriginal> self)
        where TIterator : IResolvableTo<TOriginal>
    {
        return self.Count == 0;
    }

    /// <summary>
    /// Returns whether the list contains at least one element.
    /// </summary>
    public static BoolProxy Any<TIterator, TOriginal>(this IIterationSource<TIterator, TOriginal> self)
        where TIterator : IResolvableTo<TOriginal>
    {
        return self.Count > 0;
    }

    /// <summary>
    /// Returns one of two values depending on whether the list is empty.
    /// </summary>
    public static IResolvableTo<T> EmptyOrNot<TIterator, TOriginal, T>(
        this IIterationSource<TIterator, TOriginal> self, T ifEmpty, T ifNotEmpty)
        where TIterator : IResolvableTo<TOriginal>
    {
        return self.To((ifEmpty, ifNotEmpty), (it, c) => it.Count == 0 ? c.ifEmpty : c.ifNotEmpty);
    }

    private static int IndexOfInternal<T>(this IReadOnlyList<T> self, T value)
    {
        for (var i = 0; i < self.Count; i++)
        {
            var elem = self[i];
            if (elem is null && value is null)
            {
                return i;
            }

            if (elem is null || value is null)
            {
                continue;
            }

            if (elem is IEquatable<T> a && value is IEquatable<T> b && a.Equals(b))
            {
                return i;
            }

            if (elem.Equals(value))
            {
                return i;
            }
        }

        return -1;
    }
}
