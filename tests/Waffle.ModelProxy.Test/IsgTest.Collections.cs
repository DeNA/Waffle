// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.ModelProxy.Test;

public partial class IsgTest
{
    // Simple reference type for nullable ref element list tests
    public class SimpleModel(string name)
    {
        public override string ToString() => name;
    }

    // Models for collection type tests
    [ModelProxy]
    public class CollectionModel
    {
        public string[] StringArrayProp { get; }
        public List<int> IntListProp { get; }
        public IReadOnlyList<string> StringROListProp { get; }

        // Nullable element combinations
        public string?[] NullableElemArrayProp { get; }
        public List<string?> NullableElemListProp { get; }

        // Non-string nullable reference type elements (validates ElemFullType normalization)
        public SimpleModel?[] NullableRefElemArrayProp { get; }

        // Nullable collection itself
        public string[]? NullableArrayProp { get; }
        public List<string>? NullableListProp { get; }

        public CollectionModel(
            string[] strings,
            List<int> ints,
            IReadOnlyList<string> roStrings,
            string?[] nullableElems,
            List<string?> nullableElemList,
            SimpleModel?[] nullableRefElems,
            string[]? nullableArray,
            List<string>? nullableList)
        {
            StringArrayProp = strings;
            IntListProp = ints;
            StringROListProp = roStrings;
            NullableElemArrayProp = nullableElems;
            NullableElemListProp = nullableElemList;
            NullableRefElemArrayProp = nullableRefElems;
            NullableArrayProp = nullableArray;
            NullableListProp = nullableList;
        }
    }

    // Helper methods to avoid out-variable scope conflicts
    private string RunCollectionNormal_StringArray(CollectionModel model) =>
        Render($$"""
            {{ForEach(model.StringArrayProp, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string RunCollectionProxy_StringArray(CollectionModel model) =>
        Render($$"""
            {{ForEach(model.AsProxy().StringArrayProp, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string RunCollectionNormal_IntList(CollectionModel model) =>
        Render($$"""
            {{ForEach(model.IntListProp, out var n, out var i)}}
            [{{i}}]={{n}}
            {{End}}
            """);

    private string RunCollectionProxy_IntList(CollectionModel model) =>
        Render($$"""
            {{ForEach(model.AsProxy().IntListProp, out var n, out var i)}}
            [{{i}}]={{n}}
            {{End}}
            """);

    private string RunCollectionNormal_ROList(CollectionModel model) =>
        Render($$"""
            {{ForEach(model.StringROListProp, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string RunCollectionProxy_ROList(CollectionModel model) =>
        Render($$"""
            {{ForEach(model.AsProxy().StringROListProp, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string RunCollectionNormal_NullableElemArray(CollectionModel model) =>
        Render($$"""
            {{ForEach(model.NullableElemArrayProp, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string RunCollectionProxy_NullableElemArray(CollectionModel model) =>
        Render($$"""
            {{ForEach(model.AsProxy().NullableElemArrayProp, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string RunCollectionNormal_NullableElemList(CollectionModel model) =>
        Render($$"""
            {{ForEach(model.NullableElemListProp, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string RunCollectionProxy_NullableElemList(CollectionModel model) =>
        Render($$"""
            {{ForEach(model.AsProxy().NullableElemListProp, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string RunCollectionNormal_NullableArray(CollectionModel model) =>
        Render($$"""
            {{ForEach(model.NullableArrayProp ?? [], out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    private string RunCollectionProxy_NullableArray(CollectionModel model) =>
        // After the Phase 2 fix, null array → 0 iterations (no NRE)
        Render($$"""
            {{ForEach(model.AsProxy().NullableArrayProp, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

    // --- Basic non-nullable collection tests ---

    [Test]
    public void AsProxy_Collection_StringArray_OutputMatches()
    {
        var model = new CollectionModel(
            ["Alice", "Bob", "Charlie"],
            [1, 2, 3],
            ["x", "y"],
            ["a", null, "c"],
            ["p", null, "r"],
            [],
            ["nullable"],
            null);

        Assert.That(RunCollectionProxy_StringArray(model), Is.EqualTo(RunCollectionNormal_StringArray(model)));
    }

    [Test]
    public void AsProxy_Collection_IntList_OutputMatches()
    {
        var model = new CollectionModel(
            ["Alice"],
            [10, 20, 30],
            ["x"],
            [],
            [],
            [],
            null,
            null);

        Assert.That(RunCollectionProxy_IntList(model), Is.EqualTo(RunCollectionNormal_IntList(model)));
    }

    [Test]
    public void AsProxy_Collection_ReadOnlyList_OutputMatches()
    {
        var model = new CollectionModel(
            ["Alice"],
            [1],
            ["foo", "bar", "baz"],
            [],
            [],
            [],
            null,
            null);

        Assert.That(RunCollectionProxy_ROList(model), Is.EqualTo(RunCollectionNormal_ROList(model)));
    }

    // --- Nullable element collection tests ---

    [Test]
    public void AsProxy_Collection_NullableElemArray_AllNonNull_OutputMatches()
    {
        var model = new CollectionModel(
            [],
            [],
            [],
            ["Alice", "Bob"],
            ["x", "y"],
            [],
            null,
            null);

        Assert.That(RunCollectionProxy_NullableElemArray(model),
            Is.EqualTo(RunCollectionNormal_NullableElemArray(model)));
    }

    [Test]
    public void AsProxy_Collection_NullableElemArray_WithNulls_NullElemRendersEmpty()
    {
        var model = new CollectionModel(
            [],
            [],
            [],
            ["Alice", null, "Charlie"],
            [],
            [],
            null,
            null);

        var normalResult = RunCollectionNormal_NullableElemArray(model);
        var proxyResult = RunCollectionProxy_NullableElemArray(model);

        Assert.That(proxyResult, Is.EqualTo(normalResult));
        Assert.That(proxyResult, Does.Contain("[1]="));
    }

    [Test]
    public void AsProxy_Collection_NullableElemList_WithNulls_NullElemRendersEmpty()
    {
        var model = new CollectionModel(
            [],
            [],
            [],
            [],
            ["foo", null, "bar"],
            [],
            null,
            null);

        Assert.That(RunCollectionProxy_NullableElemList(model),
            Is.EqualTo(RunCollectionNormal_NullableElemList(model)));
    }

    // --- Nullable ref type element collection tests (non-string) ---

    [Test]
    public void AsProxy_Collection_NullableRefElemArray_WithNulls_NullElemRendersEmpty()
    {
        var model = new CollectionModel(
            [], [], [], [], [],
            [new SimpleModel("Alpha"), null, new SimpleModel("Gamma")],
            null, null);

        var result = Render($$"""
            {{ForEach(model.AsProxy().NullableRefElemArrayProp, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);

        // null element should render as empty string
        Assert.That(result, Is.EqualTo("[0]=Alpha\n[1]=\n[2]=Gamma\n"));
    }

    // --- HasXxx tests for nullable collection ---

    [Test]
    public void AsProxy_Collection_NullableArray_NonNull_HasPropertyIsTrue()
    {
        var model = new CollectionModel([], [], [], [], [], [], ["item"], null);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableArrayProp)}}has{{End}}""");

        Assert.That(result, Is.EqualTo("has"));
    }

    [Test]
    public void AsProxy_Collection_NullableArray_Null_HasPropertyIsFalse()
    {
        var model = new CollectionModel([], [], [], [], [], [], null, null);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableArrayProp)}}has{{End}}""");

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void AsProxy_Collection_NullableList_NonNull_HasPropertyIsTrue()
    {
        var model = new CollectionModel([], [], [], [], [], [], null, ["item"]);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableListProp)}}has{{End}}""");

        Assert.That(result, Is.EqualTo("has"));
    }

    [Test]
    public void AsProxy_Collection_NullableList_Null_HasPropertyIsFalse()
    {
        var model = new CollectionModel([], [], [], [], [], [], null, null);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableListProp)}}has{{End}}""");

        Assert.That(result, Is.EqualTo(""));
    }

    // --- Nullable collection itself: null should produce 0 iterations, not NRE ---
    // NOTE: These tests document the EXPECTED behavior after Phase 2 fix.
    //       Before the fix, accessing ForEach on a null collection causes NRE.

    [Test]
    public void AsProxy_Collection_NullableArray_Null_ForEachProducesZeroIterations()
    {
        var model = new CollectionModel([], [], [], [], [], [], null, null);

        var normalResult = RunCollectionNormal_NullableArray(model); // uses null-coalescing to []
        var proxyResult = RunCollectionProxy_NullableArray(model); // proxy should handle null gracefully

        Assert.That(proxyResult, Is.EqualTo(normalResult));
        Assert.That(proxyResult, Is.EqualTo(""));
    }

    [Test]
    public void AsProxy_Collection_NullableArray_NonNull_ForEachIteratesNormally()
    {
        var model = new CollectionModel([], [], [], [], [], [], ["Alice", "Bob"], null);

        var normalResult = RunCollectionNormal_NullableArray(model);
        var proxyResult = RunCollectionProxy_NullableArray(model);

        Assert.That(proxyResult, Is.EqualTo(normalResult));
    }
}
