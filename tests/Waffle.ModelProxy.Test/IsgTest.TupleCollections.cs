// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.ModelProxy.Test;

/// <summary>
/// Tests that verify ElemNeedsToArray is correctly propagated through tuple-member paths in the generator:
/// - tuple fields typed as IEnumerable&lt;T&gt; must emit .ToArray()
/// - IEnumerable&lt;(tuple)&gt; properties must emit .ToArray() on the outer collection
/// - List&lt;(tuple with IEnumerable field)&gt;: outer needs no ToArray, inner field does
/// - Existing T[]-based tuple properties continue to work (regression guard)
/// </summary>
public partial class IsgTest
{
    // ---- Models ----

    /// <summary>Tuple field is IEnumerable&lt;Model2&gt; (needs ToArray), inside a T[] list of tuples.</summary>
    [ModelProxy]
    public class TupleWithEnumerableFieldModel(
        (string Label, IEnumerable<Model2> Items)[] tuples)
    {
        public readonly (string Label, IEnumerable<Model2> Items)[] Tuples = tuples;
    }

    /// <summary>Property is IEnumerable&lt;(string, int)&gt; — outer collection needs ToArray.</summary>
    [ModelProxy]
    public class EnumerableTupleListModel(IEnumerable<(string Name, int Score)> items)
    {
        public readonly IEnumerable<(string Name, int Score)> Items = items;
    }

    /// <summary>
    /// Outer collection is List&lt;T&gt; (no ToArray needed), but the tuple field
    /// <c>Tags</c> is IEnumerable&lt;string&gt; (needs ToArray).
    /// </summary>
    [ModelProxy]
    public class ListOfTuplesWithEnumerableFieldModel(
        List<(string Name, IEnumerable<string> Tags)> entries)
    {
        public readonly List<(string Name, IEnumerable<string> Tags)> Entries = entries;
    }

    // ---- Helpers ----

    private string Proxy_TupleWithEnumerableField(TupleWithEnumerableFieldModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().Tuples, out var t, out var i)}}
            {{i}}:{{t.Label}}
            {{ForEach(t.Items, out var item, out var j)}}
            {{i}}-{{j}}:{{item.Value}}
            {{End}}
            {{End}}
            """);

    private string Normal_TupleWithEnumerableField(TupleWithEnumerableFieldModel m) =>
        Render($$"""
            {{ForEach(m.Tuples, out var t, out var i)}}
            {{i}}:{{t.To(it => it.Label)}}
            {{ForEach(t.To(it => it.Items), out var item, out var j)}}
            {{i}}-{{j}}:{{item.To(it => it.Value)}}
            {{End}}
            {{End}}
            """);

    private string Proxy_EnumerableTupleList(EnumerableTupleListModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().Items, out var t, out var i)}}
            {{i}}:{{t.Name}},{{t.Score}}
            {{End}}
            """);

    private string Normal_EnumerableTupleList(EnumerableTupleListModel m) =>
        Render($$"""
            {{ForEach(m.Items, out var t, out var i)}}
            {{i}}:{{t.To(it => it.Name)}},{{t.To(it => it.Score)}}
            {{End}}
            """);

    private string Proxy_ListOfTuplesWithEnumerableField(ListOfTuplesWithEnumerableFieldModel m) =>
        Render($$"""
            {{ForEach(m.AsProxy().Entries, out var e, out var i)}}
            {{i}}:{{e.Name}}[{{ForEach(e.Tags, out var tag, out var j)}}{{If(j > 0)}},{{End}}{{tag}}{{End}}]
            {{End}}
            """);

    private string Normal_ListOfTuplesWithEnumerableField(ListOfTuplesWithEnumerableFieldModel m) =>
        Render($$"""
            {{ForEach(m.Entries, out var e, out var i)}}
            {{i}}:{{e.To(it => it.Name)}}[{{ForEach(e.To(it => it.Tags), out var tag, out var j)}}{{If(j > 0)}},{{End}}{{tag}}{{End}}]
            {{End}}
            """);

    // ---- Tests ----

    /// <summary>
    /// Tuple field typed as IEnumerable&lt;T&gt; inside a T[] list of tuples.
    /// The generator must emit .ToArray() for the Items field accessor.
    /// </summary>
    [Test]
    public void AsProxy_TupleField_IEnumerable_IteratesCorrectly()
    {
        var m = new TupleWithEnumerableFieldModel([
            ("alpha", [new Model2(1), new Model2(2)]),
            ("beta", [new Model2(3)])
        ]);

        Assert.That(Proxy_TupleWithEnumerableField(m), Is.EqualTo(Normal_TupleWithEnumerableField(m)));
    }

    [Test]
    public void AsProxy_TupleField_IEnumerable_Empty_ZeroInnerIterations()
    {
        var m = new TupleWithEnumerableFieldModel([
            ("empty", [])
        ]);

        var result = Proxy_TupleWithEnumerableField(m);
        Assert.That(result, Is.EqualTo("0:empty\n"));
    }

    /// <summary>
    /// Property typed as IEnumerable&lt;(string, int)&gt;: the outer list itself needs .ToArray().
    /// </summary>
    [Test]
    public void AsProxy_IEnumerable_OfTuples_IteratesCorrectly()
    {
        IEnumerable<(string, int)> src = [("Alice", 90), ("Bob", 80)];
        var m = new EnumerableTupleListModel(src);

        Assert.That(Proxy_EnumerableTupleList(m), Is.EqualTo(Normal_EnumerableTupleList(m)));
    }

    [Test]
    public void AsProxy_IEnumerable_OfTuples_Empty_ZeroIterations()
    {
        var m = new EnumerableTupleListModel([]);
        Assert.That(Proxy_EnumerableTupleList(m), Is.EqualTo(""));
    }

    /// <summary>
    /// Outer collection is List&lt;T&gt; (no ToArray for the list itself), but the tuple field
    /// Tags is IEnumerable&lt;string&gt; (needs ToArray). This specifically tests the bug where
    /// fieldNeedsToArray was incorrectly derived from the outer list's flag rather than the
    /// field's own collection type.
    /// </summary>
    [Test]
    public void AsProxy_ListOfTuples_WithEnumerableField_IteratesCorrectly()
    {
        var m = new ListOfTuplesWithEnumerableFieldModel([
            ("Alice", ["c#", "waffle"]),
            ("Bob", ["go"])
        ]);

        Assert.That(
            Proxy_ListOfTuplesWithEnumerableField(m),
            Is.EqualTo(Normal_ListOfTuplesWithEnumerableField(m)));
    }

    [Test]
    public void AsProxy_ListOfTuples_WithEnumerableField_EmptyTags_Renders()
    {
        var m = new ListOfTuplesWithEnumerableFieldModel([
            ("Charlie", [])
        ]);

        var result = Proxy_ListOfTuplesWithEnumerableField(m);
        Assert.That(result, Is.EqualTo("0:Charlie[]\n"));
    }
}
