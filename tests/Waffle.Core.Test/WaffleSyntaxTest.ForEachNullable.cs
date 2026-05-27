// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.Core.Test;

public partial class WaffleSyntaxTest
{
    // Tests for ForEachNullable (nullable reference type elements)

    // --- Non-indexed overloads ---

    [Test]
    public void ForEachNullable_AllNonNull_RendersAllElements()
    {
        var result = Render($$"""
            {{ForEachNullable(["Alice", "Bob", "Charlie"], out var s)}}
            {{s}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("Alice\nBob\nCharlie\n"));
    }

    [Test]
    public void ForEachNullable_WithNullElement_NullRendersAsEmpty()
    {
        var result = Render($$"""
            {{ForEachNullable(["Alice", null, "Charlie"], out var s)}}
            [{{s}}]
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[Alice]\n[]\n[Charlie]\n"));
    }

    [Test]
    public void ForEachNullable_AllNull_RendersAllEmpty()
    {
        var result = Render($$"""
            {{ForEachNullable(new string?[] { null, null }, out var s)}}
            [{{s}}]
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[]\n[]\n"));
    }

    [Test]
    public void ForEachNullable_EmptyCollection_RendersNothing()
    {
        var result = Render($$"""
            {{ForEachNullable(new string?[] { }, out var s)}}
            {{s}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void ForEachNullable_NullCollection_RendersNothing()
    {
        string?[]? nullArr = null;
        var result = Render($$"""
            {{ForEachNullable(nullArr, out var s)}}
            {{s}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void ForEachNullable_ResolvableSource_RendersElements()
    {
        var source = new LiteralProxy<IEnumerable<string?>>(["X", null, "Z"]);
        var result = Render($$"""
            {{ForEachNullable(source, out var s)}}
            [{{s}}]
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[X]\n[]\n[Z]\n"));
    }

    // --- Indexed overloads ---

    [Test]
    public void ForEachNullable_Indexed_NonNull_RendersIndexAndValue()
    {
        var result = Render($$"""
            {{ForEachNullable(["Alice", "Bob"], out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[0]=Alice\n[1]=Bob\n"));
    }

    [Test]
    public void ForEachNullable_Indexed_NullElement_IndexIncrements()
    {
        var result = Render($$"""
            {{ForEachNullable(["Alice", null, "Charlie"], out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[0]=Alice\n[1]=\n[2]=Charlie\n"));
    }

    [Test]
    public void ForEachNullable_Indexed_ResolvableSource_RendersIndexAndValue()
    {
        var source = new LiteralProxy<IEnumerable<string?>>(["X", null]);
        var result = Render($$"""
            {{ForEachNullable(source, out var s, out var i)}}
            [{{i}}]={{s}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[0]=X\n[1]=\n"));
    }

    // --- Indexed with IndexedLoopHelper overloads ---

    [Test]
    public void ForEachNullable_Helper_IsFirst_WorksCorrectly()
    {
        var result = Render($$"""
            {{ForEachNullable(["Alice", null, "Charlie"], out var s, out var i, out var h)}}
            {{h.FirstOrNot("FIRST:", "")}}[{{i}}]={{s}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("FIRST:[0]=Alice\n[1]=\n[2]=Charlie\n"));
    }

    [Test]
    public void ForEachNullable_Helper_IsLast_WorksCorrectly()
    {
        var result = Render($$"""
            {{ForEachNullable(["Alice", null, "Charlie"], out var s, out var i, out var h)}}
            [{{i}}]={{s}}{{h.LastOrNot(" LAST", "")}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[0]=Alice\n[1]=\n[2]=Charlie LAST\n"));
    }

    [Test]
    public void ForEachNullable_Helper_ResolvableSource_IsLastWorksCorrectly()
    {
        var source = new LiteralProxy<IEnumerable<string?>>(["A", null]);
        var result = Render($$"""
            {{ForEachNullable(source, out var s, out var i, out var h)}}
            [{{i}}]={{s}}{{h.LastOrNot("*", "")}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[0]=A\n[1]=*\n"));
    }

    // --- IteratorProxy throws for reference-type null elements when Resolve() is called ---

    [Test]
    public void ForEach_NonNullableString_NullElement_ThrowsInvalidCastOnResolve()
    {
        // ForEach with IEnumerable<string> (non-nullable T), encountering null at runtime.
        // {{s}} alone calls Evaluate() which renders null as empty — no throw.
        // Piping s through .To() calls Resolve(), which DOES throw for reference-type null.
        var dangerousSource = new[] { "ok", null }.Cast<string>();
        Assert.Throws<InvalidCastException>(() =>
        {
            _ = Render($$"""
                {{ForEach(dangerousSource, out var s)}}
                {{s.To(it => it.ToUpper())}}
                {{End}}
                """);
        });
    }
}
