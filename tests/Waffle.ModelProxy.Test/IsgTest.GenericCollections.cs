// (c) DeNA Co., Ltd.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Waffle.ModelProxy.Test;

public partial class IsgTest
{
    // ---- Custom list that implements IReadOnlyList<T> ----
    public class MyReadOnlyList<T>(IReadOnlyList<T> inner) : IReadOnlyList<T>
    {
        public T this[int index] => inner[index];
        public int Count => inner.Count;
        public IEnumerator<T> GetEnumerator() => inner.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // ---- Custom collection that implements only IEnumerable<T> ----
    public class MyEnumerable<T>(IEnumerable<T> inner) : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator() => inner.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [ModelProxy]
    public class GenericCollectionModel
    {
        // IEnumerable<T> as property type
        public IEnumerable<string> EnumerableStrings { get; }

        // ICollection<T> as property type
        public ICollection<int> CollectionInts { get; }

        // HashSet<T> (implements IEnumerable<T>, not IReadOnlyList<T>)
        public HashSet<string> HashSetStrings { get; }

        // Custom type implementing IReadOnlyList<T> — no .ToArray() needed
        public MyReadOnlyList<string> CustomROList { get; }

        // Custom type implementing only IEnumerable<T> — needs .ToArray()
        public MyEnumerable<string> CustomEnumerable { get; }

        // IReadOnlyCollection<T> (extends IEnumerable<T>, not IReadOnlyList<T>)
        public IReadOnlyCollection<string> ROCollection { get; }

        // Nullable IEnumerable<T>
        public IEnumerable<string>? NullableEnumerable { get; }

        // ObservableCollection<T> (implements IList<T> via Collection<T>, not IReadOnlyList<T>)
        public ObservableCollection<string> ObservableStrings { get; }

        public GenericCollectionModel(
            IEnumerable<string> enumerable,
            ICollection<int> collection,
            HashSet<string> hashSet,
            MyReadOnlyList<string> customROList,
            MyEnumerable<string> customEnumerable,
            IReadOnlyCollection<string> roCollection,
            IEnumerable<string>? nullableEnumerable,
            ObservableCollection<string> observable)
        {
            EnumerableStrings = enumerable;
            CollectionInts = collection;
            HashSetStrings = hashSet;
            CustomROList = customROList;
            CustomEnumerable = customEnumerable;
            ROCollection = roCollection;
            NullableEnumerable = nullableEnumerable;
            ObservableStrings = observable;
        }
    }

    // helpers

    private string Proxy_EnumerableStrings(GenericCollectionModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().EnumerableStrings, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string Direct_Enumerable(IEnumerable<string> src) =>
        Render($$"""
            {{ForEach(src, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string Proxy_CollectionInts(GenericCollectionModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().CollectionInts, out var n, out var i)}}
            [{{i}}]={{n}}
            {{End}}
            """);

    private string Proxy_HashSetStrings(GenericCollectionModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().HashSetStrings, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string Proxy_CustomROList(GenericCollectionModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().CustomROList, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string Proxy_CustomEnumerable(GenericCollectionModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().CustomEnumerable, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string Proxy_ROCollection(GenericCollectionModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().ROCollection, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string Proxy_ObservableStrings(GenericCollectionModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().ObservableStrings, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private GenericCollectionModel MakeGenericModel(
        IEnumerable<string>? enumerable = null,
        ICollection<int>? collection = null,
        HashSet<string>? hashSet = null,
        MyReadOnlyList<string>? customROList = null,
        MyEnumerable<string>? customEnumerable = null,
        IReadOnlyCollection<string>? roCollection = null,
        IEnumerable<string>? nullableEnumerable = null,
        ObservableCollection<string>? observable = null) =>
        new(
            enumerable ?? [],
            collection ?? new List<int>(),
            hashSet ?? [],
            customROList ?? new MyReadOnlyList<string>([]),
            customEnumerable ?? new MyEnumerable<string>([]),
            roCollection ?? [],
            nullableEnumerable,
            observable ?? []);

    // ---- Tests ----

    [Test]
    public void AsProxy_GenericCollection_IEnumerable_IteratesCorrectly()
    {
        var m = MakeGenericModel(enumerable: ["Alice", "Bob", "Charlie"]);
        var expected = Direct_Enumerable(["Alice", "Bob", "Charlie"]);
        Assert.That(Proxy_EnumerableStrings(m), Is.EqualTo(expected));
    }

    [Test]
    public void AsProxy_GenericCollection_ICollection_IteratesCorrectly()
    {
        var m = MakeGenericModel(collection: new List<int> { 10, 20, 30 });
        var result = Proxy_CollectionInts(m);
        Assert.That(result, Is.EqualTo("[0]=10\n[1]=20\n[2]=30\n"));
    }

    [Test]
    public void AsProxy_GenericCollection_HashSet_IteratesAllElements()
    {
        // Use a single-element HashSet to avoid non-deterministic enumeration order.
        var items = new HashSet<string> { "X" };
        var m = MakeGenericModel(hashSet: items);
        Assert.That(Proxy_HashSetStrings(m), Is.EqualTo("[0]=X\n"));
    }

    [Test]
    public void AsProxy_GenericCollection_CustomIReadOnlyList_IteratesCorrectly()
    {
        var m = MakeGenericModel(customROList: new MyReadOnlyList<string>(["foo", "bar"]));
        Assert.That(Proxy_CustomROList(m), Is.EqualTo("[0]=foo\n[1]=bar\n"));
    }

    [Test]
    public void AsProxy_GenericCollection_CustomIEnumerable_IteratesCorrectly()
    {
        var m = MakeGenericModel(customEnumerable: new MyEnumerable<string>(["p", "q"]));
        Assert.That(Proxy_CustomEnumerable(m), Is.EqualTo("[0]=p\n[1]=q\n"));
    }

    [Test]
    public void AsProxy_GenericCollection_IReadOnlyCollection_IteratesCorrectly()
    {
        var m = MakeGenericModel(roCollection: ["a", "b", "c"]);
        Assert.That(Proxy_ROCollection(m), Is.EqualTo("[0]=a\n[1]=b\n[2]=c\n"));
    }

    [Test]
    public void AsProxy_GenericCollection_ObservableCollection_IteratesCorrectly()
    {
        var m = MakeGenericModel(observable: ["one", "two"]);
        Assert.That(Proxy_ObservableStrings(m), Is.EqualTo("[0]=one\n[1]=two\n"));
    }

    [Test]
    public void AsProxy_GenericCollection_NullableIEnumerable_NonNull_IteratesCorrectly()
    {
        var m = MakeGenericModel(nullableEnumerable: ["x", "y"]);
        var result = Render($$"""
            {{ForEach(m.AsProxy().NullableEnumerable, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[0]=x\n[1]=y\n"));
    }

    [Test]
    public void AsProxy_GenericCollection_NullableIEnumerable_Null_ZeroIterations()
    {
        var m = MakeGenericModel(nullableEnumerable: null);
        var result = Render($$"""
            {{ForEach(m.AsProxy().NullableEnumerable, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void AsProxy_GenericCollection_NullableIEnumerable_HasProp_TrueWhenNonNull()
    {
        var m = MakeGenericModel(nullableEnumerable: ["z"]);
        var result = Render($$"""{{If(m.AsProxy().HasNullableEnumerable)}}has{{End}}""");
        Assert.That(result, Is.EqualTo("has"));
    }

    [Test]
    public void AsProxy_GenericCollection_NullableIEnumerable_HasProp_FalseWhenNull()
    {
        var m = MakeGenericModel(nullableEnumerable: null);
        var result = Render($$"""{{If(m.AsProxy().HasNullableEnumerable)}}has{{End}}""");
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void AsProxy_GenericCollection_IEnumerable_Empty_ZeroIterations()
    {
        var m = MakeGenericModel(enumerable: []);
        Assert.That(Proxy_EnumerableStrings(m), Is.EqualTo(""));
    }
}
