// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.ModelProxy.Test;

public partial class IsgTest
{
    // ---- Non-generic closed implementations ----

    /// <summary>
    /// Non-generic class that implements IReadOnlyList&lt;string&gt; with fixed element type.
    /// The generator must detect the IReadOnlyList&lt;T&gt; interface via AllInterfaces
    /// even though the class itself is not generic.
    /// </summary>
    public sealed class ClosedStringROList(IReadOnlyList<string> inner) : IReadOnlyList<string>
    {
        public string this[int index] => inner[index];
        public int Count => inner.Count;
        public IEnumerator<string> GetEnumerator() => inner.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Non-generic class that implements IEnumerable&lt;int&gt; with fixed element type.
    /// The generator must detect IEnumerable&lt;T&gt; via AllInterfaces and emit .ToArray().
    /// </summary>
    public sealed class ClosedIntEnumerable(IEnumerable<int> inner) : IEnumerable<int>
    {
        public IEnumerator<int> GetEnumerator() => inner.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [ModelProxy]
    public class NonGenericCollectionModel(
        ClosedStringROList closedRoList,
        ClosedIntEnumerable closedEnumerable,
        IList<string> iListStrings,
        IList<string>? nullableIListStrings)
    {
        /// Non-generic IReadOnlyList<string> implementation — no ToArray conversion needed.
        public ClosedStringROList ClosedROList { get; } = closedRoList;

        /// Non-generic IEnumerable<int> implementation — requires ToArray conversion.
        public ClosedIntEnumerable ClosedEnumerable { get; } = closedEnumerable;

        /// IList<T> — does not extend IReadOnlyList<T>, so requires ToArray conversion.
        public IList<string> IListStrings { get; } = iListStrings;

        /// Nullable IList<T> — same conversion requirement, also gets a HasXxx accessor.
        public IList<string>? NullableIListStrings { get; } = nullableIListStrings;
    }

    // ---- Helpers ----

    private string Proxy_ClosedROList(NonGenericCollectionModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().ClosedROList, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string Proxy_ClosedEnumerable(NonGenericCollectionModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().ClosedEnumerable, out var n, out var i)}}
            [{{i}}]={{n}}
            {{End}}
            """);

    private string Proxy_IListStrings(NonGenericCollectionModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().IListStrings, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string Proxy_NullableIListStrings(NonGenericCollectionModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().NullableIListStrings, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private NonGenericCollectionModel MakeNonGenericModel(
        ClosedStringROList? closedROList = null,
        ClosedIntEnumerable? closedEnumerable = null,
        IList<string>? iListStrings = null,
        IList<string>? nullableIListStrings = null) =>
        new(
            closedROList ?? new ClosedStringROList([]),
            closedEnumerable ?? new ClosedIntEnumerable([]),
            iListStrings ?? [],
            nullableIListStrings);

    // ---- Tests: non-generic IReadOnlyList<T> implementor ----

    [Test]
    public void AsProxy_NonGenericClosedROList_IteratesCorrectly()
    {
        var m = MakeNonGenericModel(closedROList: new ClosedStringROList(["foo", "bar", "baz"]));
        Assert.That(Proxy_ClosedROList(m), Is.EqualTo("[0]=foo\n[1]=bar\n[2]=baz\n"));
    }

    [Test]
    public void AsProxy_NonGenericClosedROList_Empty_ZeroIterations()
    {
        var m = MakeNonGenericModel(closedROList: new ClosedStringROList([]));
        Assert.That(Proxy_ClosedROList(m), Is.EqualTo(""));
    }

    // ---- Tests: non-generic IEnumerable<T> implementor ----

    [Test]
    public void AsProxy_NonGenericClosedEnumerable_IteratesCorrectly()
    {
        var m = MakeNonGenericModel(closedEnumerable: new ClosedIntEnumerable([10, 20, 30]));
        Assert.That(Proxy_ClosedEnumerable(m), Is.EqualTo("[0]=10\n[1]=20\n[2]=30\n"));
    }

    [Test]
    public void AsProxy_NonGenericClosedEnumerable_Empty_ZeroIterations()
    {
        var m = MakeNonGenericModel(closedEnumerable: new ClosedIntEnumerable([]));
        Assert.That(Proxy_ClosedEnumerable(m), Is.EqualTo(""));
    }

    // ---- Tests: IList<T> property ----

    [Test]
    public void AsProxy_IList_IteratesCorrectly()
    {
        var m = MakeNonGenericModel(iListStrings: ["alpha", "beta", "gamma"]);
        Assert.That(Proxy_IListStrings(m), Is.EqualTo("[0]=alpha\n[1]=beta\n[2]=gamma\n"));
    }

    [Test]
    public void AsProxy_IList_Empty_ZeroIterations()
    {
        var m = MakeNonGenericModel(iListStrings: []);
        Assert.That(Proxy_IListStrings(m), Is.EqualTo(""));
    }

    // ---- Tests: nullable IList<T> property ----

    [Test]
    public void AsProxy_NullableIList_NonNull_IteratesCorrectly()
    {
        var m = MakeNonGenericModel(nullableIListStrings: ["x", "y"]);
        Assert.That(Proxy_NullableIListStrings(m), Is.EqualTo("[0]=x\n[1]=y\n"));
    }

    [Test]
    public void AsProxy_NullableIList_Null_ZeroIterations()
    {
        var m = MakeNonGenericModel(nullableIListStrings: null);
        Assert.That(Proxy_NullableIListStrings(m), Is.EqualTo(""));
    }

    [Test]
    public void AsProxy_NullableIList_HasProp_TrueWhenNonNull()
    {
        var m = MakeNonGenericModel(nullableIListStrings: ["z"]);
        var result = Render($$"""{{If(m.AsProxy().HasNullableIListStrings)}}has{{End}}""");
        Assert.That(result, Is.EqualTo("has"));
    }

    [Test]
    public void AsProxy_NullableIList_HasProp_FalseWhenNull()
    {
        var m = MakeNonGenericModel(nullableIListStrings: null);
        var result = Render($$"""{{If(m.AsProxy().HasNullableIListStrings)}}has{{End}}""");
        Assert.That(result, Is.EqualTo(""));
    }
}
