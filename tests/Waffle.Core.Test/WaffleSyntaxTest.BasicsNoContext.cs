// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.Core.Test;

public partial class WaffleSyntaxTest
{
    // Tests for basic syntax (no context)

    [Test]
    public void Render_NoContext_EmptyString()
    {
        Assert.That(
            Render(""),
            Is.EqualTo(""));
    }

    [Test]
    public void Render_NoContext_StringLiteral()
    {
        Assert.That(
            Render("Hello, world!"),
            Is.EqualTo("Hello, world!"));
    }

    [Test]
    public void Render_NoContext_StandardInterpolatedString()
    {
        const string Name = "world";
        Assert.That(
            Render($"Hello, {Name}!"),
            Is.EqualTo($"Hello, {Name}!"));
    }

    [Test]
    public void Render_NoContext_StandardInterpolatedString_Null()
    {
        Assert.That(
            Render($"Hello, {(object?)null}!"),
            Is.EqualTo($"Hello, {null}!"));
    }

    [Test]
    public void Render_NoContext_RawStringLiteral()
    {
        Assert.That(
            Render("""
                Hello, world!
                I am a pen.
                """),
            Is.EqualTo("""
                Hello, world!
                I am a pen.
                """));
    }

    [Test]
    public void Render_NoContext_StandardInterpolatedRawString()
    {
        const string Name = "pen";
        Assert.That(
            Render($"""
                Hello, world!
                I am a {Name}.
                """),
            Is.EqualTo($"""
                Hello, world!
                I am a {Name}.
                """));
    }

    [Test]
    public void Render_NoContext_StandardInterpolatedRawString_Null()
    {
        Assert.That(
            Render($"""
                Hello, world!
                I am {(object?)null}.
                """),
            Is.EqualTo($"""
                Hello, world!
                I am {null}.
                """));
    }

    [Test]
    public void Render_NoContext_ForEach()
    {
        var values = new[] { "A", "B", "C" };
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value)}}
                {{value}},
                {{End}}
                """),
            Is.EqualTo("""
                A,
                B,
                C,
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_Empty()
    {
        var values = new string[0];
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value)}}
                {{value}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_Nested()
    {
        var values1 = new[] { "A", "B" };
        var values2 = new[] { "1", "2" };
        Assert.That(
            Render($$"""
                {{ForEach(values1, out var value1)}}
                {{ForEach(values2, out var value2)}}
                {{value1}}{{value2}},
                {{End}}
                {{End}}
                """),
            Is.EqualTo("""
                A1,
                A2,
                B1,
                B2,
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_IResolvableTo()
    {
        IResolvableTo<IEnumerable<string>> values = new LiteralProxy<string[]>(new[] { "A", "B", "C" });
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value)}}
                {{value}},
                {{End}}
                """),
            Is.EqualTo("""
                A,
                B,
                C,
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_IIterationSource()
    {
        IIterationSource<IResolvableTo<string>, string> values = new[] { "A", "B", "C" }.AsProxy();
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value)}}
                {{value}},
                {{End}}
                """),
            Is.EqualTo("""
                A,
                B,
                C,
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_Indexed()
    {
        var values = new[] { "A", "B", "C" };
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value, out var index)}}
                {{index}}: {{value}},
                {{End}}
                """),
            Is.EqualTo("""
                0: A,
                1: B,
                2: C,
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_Indexed_Empty()
    {
        var values = Array.Empty<string>();
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value, out var index)}}
                {{index}}: {{value}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_Indexed_Nested()
    {
        var values1 = new[] { "A", "B" };
        var values2 = new[] { "1", "2" };
        Assert.That(
            Render($$"""
                {{ForEach(values1, out var value1, out var index1)}}
                {{ForEach(values2, out var value2, out var index2)}}
                {{index1}}{{index2}}: {{value1}}{{value2}},
                {{End}}
                {{End}}
                """),
            Is.EqualTo("""
                00: A1,
                01: A2,
                10: B1,
                11: B2,
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_Indexed_IResolvableTo()
    {
        IResolvableTo<IEnumerable<string>> values = new LiteralProxy<string[]>(new[] { "A", "B", "C" });
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value, out var index)}}
                {{index}}: {{value}},
                {{End}}
                """),
            Is.EqualTo("""
                0: A,
                1: B,
                2: C,
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_Indexed_IIterationSource()
    {
        IIterationSource<IResolvableTo<string>, string> values = new[] { "A", "B", "C" }.AsProxy();
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value, out var index)}}
                {{index}}: {{value}},
                {{End}}
                """),
            Is.EqualTo("""
                0: A,
                1: B,
                2: C,
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_IndexedWithHelper_FirstOrNot()
    {
        var values = new[] { "A", "B", "C" };
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value, out _, out var helper)}}
                {{helper.FirstOrNot("First", "NotFirst")}}: {{value}},
                {{End}}
                """),
            Is.EqualTo("""
                First: A,
                NotFirst: B,
                NotFirst: C,
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_IndexedWithHelper_LastOrNot()
    {
        var values = new[] { "A", "B", "C" };
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value, out _, out var helper)}}
                {{helper.LastOrNot("Last", "NotLast")}}: {{value}},
                {{End}}
                """),
            Is.EqualTo("""
                NotLast: A,
                NotLast: B,
                Last: C,
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_IndexedWithHelper_CommaOrLastEmpty()
    {
        var values = new[] { "A", "B", "C" };
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value, out var index, out var helper)}}
                {{index}}: {{value}}{{helper.CommaOrLastEmpty}}
                {{End}}
                """),
            Is.EqualTo("""
                0: A,
                1: B,
                2: C
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_IndexedWithHelper_CommaOrLastParen()
    {
        var values = new[] { "A", "B", "C" };
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value, out var index, out var helper)}}
                {{index}}: {{value}}{{helper.CommaOrLastParen}}
                {{End}}
                """),
            Is.EqualTo("""
                0: A,
                1: B,
                2: C)
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_IndexedWithHelper_Empty()
    {
        var values = Array.Empty<string>();
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value, out var index, out var helper)}}
                {{index}}: {{value}}{{helper.CommaOrLastEmpty}}
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_IndexedWithHelper_Nested()
    {
        var values1 = new[] { "A", "B" };
        var values2 = new[] { "1", "2" };
        Assert.That(
            Render($$"""
                {{ForEach(values1, out var value1, out var index1, out var helper1)}}
                {{ForEach(values2, out var value2, out var index2, out var helper2)}}
                {{index1}}{{index2}}: {{value1}}{{value2}}{{helper1.CommaOrLastEmpty}}{{helper2.CommaOrLastEmpty}}
                {{End}}
                {{End}}
                """),
            Is.EqualTo("""
                00: A1,,
                01: A2,
                10: B1,
                11: B2
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_IndexedWithHelper_IResolvableTo()
    {
        IResolvableTo<IEnumerable<string>> values = new LiteralProxy<string[]>(new[] { "A", "B", "C" });
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value, out var index, out var helper)}}
                {{index}}: {{value}}{{helper.CommaOrLastEmpty}}
                {{End}}
                """),
            Is.EqualTo("""
                0: A,
                1: B,
                2: C
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_IndexedWithHelper_IIterationSource()
    {
        IIterationSource<IResolvableTo<string>, string> values = new[] { "A", "B", "C" }.AsProxy();
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value, out var index, out var helper)}}
                {{index}}: {{value}}{{helper.CommaOrLastEmpty}}
                {{End}}
                """),
            Is.EqualTo("""
                0: A,
                1: B,
                2: C
                
                """));
    }

    [Test]
    public void Render_NoContext_ForEach_OneLine()
    {
        var values = new[] { "A", "B", "C" };
        Assert.That(
            Render($$"""
                {{ForEach(values, out var value)}}{{value}}{{End}}
                """),
            Is.EqualTo("ABC"));
    }

    [Test]
    public void Render_NoContext_For()
    {
        Assert.That(
            Render($$"""
                {{For(1, 4, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                1,
                2,
                3,
                
                """));
    }

    [Test]
    public void Render_NoContext_For_Empty()
    {
        Assert.That(
            Render($$"""
                {{For(1, 1, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_For_ReversedBounds()
    {
        Assert.That(
            Render($$"""
                {{For(2, 1, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_For_From_IResolvableTo()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{For(from, 4, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                1,
                2,
                3,
                
                """));
    }

    [Test]
    public void Render_NoContext_For_From_IResolvableTo_Empty()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{For(from, 1, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_For_From_IResolvableTo_ReversedBounds()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(2);
        Assert.That(
            Render($$"""
                {{For(from, 1, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_For_To_IResolvableTo()
    {
        IResolvableTo<int> to = new LiteralProxy<int>(4);
        Assert.That(
            Render($$"""
                {{For(1, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                1,
                2,
                3,
                
                """));
    }

    [Test]
    public void Render_NoContext_For_To_IResolvableTo_Empty()
    {
        IResolvableTo<int> to = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{For(1, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_For_To_IResolvableTo_ReversedBounds()
    {
        IResolvableTo<int> to = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{For(2, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_For_FromTo_IResolvableTo()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        IResolvableTo<int> to = new LiteralProxy<int>(4);
        Assert.That(
            Render($$"""
                {{For(from, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                1,
                2,
                3,
                
                """));
    }

    [Test]
    public void Render_NoContext_For_FromTo_IResolvableTo_Empty()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        IResolvableTo<int> to = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{For(from, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_For_FromTo_IResolvableTo_ReversedBounds()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(2);
        IResolvableTo<int> to = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{For(from, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_For_OneLine()
    {
        Assert.That(
            Render($$"""
                {{For(1, 4, out var i)}}{{i}}{{End}}
                """),
            Is.EqualTo("123"));
    }

    [Test]
    public void Render_NoContext_For_WithHelper()
    {
        Assert.That(
            Render($$"""
                {{For(1, 4, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo("1,2,3"));
    }

    [Test]
    public void Render_NoContext_For_WithHelper_Empty()
    {
        Assert.That(
            Render($$"""
                {{For(1, 1, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo(""));
    }

    [Test]
    public void Render_NoContext_For_FromIResolvableTo_WithHelper()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{For(from, 4, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo("1,2,3"));
    }

    [Test]
    public void Render_NoContext_For_FromIResolvableTo_WithHelper_Empty()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{For(from, 1, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo(""));
    }

    [Test]
    public void Render_NoContext_For_ToIResolvableTo_WithHelper()
    {
        IResolvableTo<int> to = new LiteralProxy<int>(4);
        Assert.That(
            Render($$"""
                {{For(1, to, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo("1,2,3"));
    }

    [Test]
    public void Render_NoContext_For_ToIResolvableTo_WithHelper_Empty()
    {
        IResolvableTo<int> to = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{For(1, to, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo(""));
    }

    [Test]
    public void Render_NoContext_For_FromToIResolvableTo_WithHelper()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        IResolvableTo<int> to = new LiteralProxy<int>(4);
        Assert.That(
            Render($$"""
                {{For(from, to, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo("1,2,3"));
    }

    [Test]
    public void Render_NoContext_For_FromToIResolvableTo_WithHelper_Empty()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        IResolvableTo<int> to = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{For(from, to, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo(""));
    }

    [Test]
    public void Render_NoContext_Forr()
    {
        Assert.That(
            Render($$"""
                {{Forr(3, 1, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                3,
                2,
                1,
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_Empty()
    {
        Assert.That(
            Render($$"""
                {{Forr(1, 2, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_ReversedBounds()
    {
        Assert.That(
            Render($$"""
                {{Forr(1, 3, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_From_IResolvableTo()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(3);
        Assert.That(
            Render($$"""
                {{Forr(from, 1, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                3,
                2,
                1,
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_From_IResolvableTo_Empty()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{Forr(from, 2, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_From_IResolvableTo_ReversedBounds()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{Forr(from, 3, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_To_IResolvableTo()
    {
        IResolvableTo<int> to = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{Forr(3, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                3,
                2,
                1,
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_To_IResolvableTo_Empty()
    {
        IResolvableTo<int> to = new LiteralProxy<int>(2);
        Assert.That(
            Render($$"""
                {{Forr(1, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_To_IResolvableTo_ReversedBounds()
    {
        IResolvableTo<int> to = new LiteralProxy<int>(3);
        Assert.That(
            Render($$"""
                {{Forr(1, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_FromTo_IResolvableTo()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(3);
        IResolvableTo<int> to = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{Forr(from, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                3,
                2,
                1,
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_FromTo_IResolvableTo_Empty()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        IResolvableTo<int> to = new LiteralProxy<int>(2);
        Assert.That(
            Render($$"""
                {{Forr(from, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_FromTo_IResolvableTo_ReversedBounds()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        IResolvableTo<int> to = new LiteralProxy<int>(3);
        Assert.That(
            Render($$"""
                {{Forr(from, to, out var i)}}
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                
                """));
    }

    [Test]
    public void Render_NoContext_Forr_OneLine()
    {
        Assert.That(
            Render($$"""
                {{Forr(3, 1, out var i)}}{{i}}{{End}}
                """),
            Is.EqualTo("321"));
    }

    [Test]
    public void Render_NoContext_Forr_WithHelper()
    {
        Assert.That(
            Render($$"""
                {{Forr(3, 1, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo("3,2,1"));
    }

    [Test]
    public void Render_NoContext_Forr_WithHelper_Empty()
    {
        Assert.That(
            Render($$"""
                {{Forr(1, 2, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo(""));
    }

    [Test]
    public void Render_NoContext_Forr_FromIResolvableTo_WithHelper()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(3);
        Assert.That(
            Render($$"""
                {{Forr(from, 1, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo("3,2,1"));
    }

    [Test]
    public void Render_NoContext_Forr_FromIResolvableTo_WithHelper_Empty()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{Forr(from, 2, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo(""));
    }

    [Test]
    public void Render_NoContext_Forr_ToIResolvableTo_WithHelper()
    {
        IResolvableTo<int> to = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{Forr(3, to, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo("3,2,1"));
    }

    [Test]
    public void Render_NoContext_Forr_ToIResolvableTo_WithHelper_Empty()
    {
        IResolvableTo<int> to = new LiteralProxy<int>(2);
        Assert.That(
            Render($$"""
                {{Forr(1, to, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo(""));
    }

    [Test]
    public void Render_NoContext_Forr_FromToIResolvableTo_WithHelper()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(3);
        IResolvableTo<int> to = new LiteralProxy<int>(1);
        Assert.That(
            Render($$"""
                {{Forr(from, to, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo("3,2,1"));
    }

    [Test]
    public void Render_NoContext_Forr_FromToIResolvableTo_WithHelper_Empty()
    {
        IResolvableTo<int> from = new LiteralProxy<int>(1);
        IResolvableTo<int> to = new LiteralProxy<int>(2);
        Assert.That(
            Render($$"""
                {{Forr(from, to, out var i, out var helper)}}
                {{i}}{{helper.CommaOrLastEmpty:>>}}
                {{End}}
                """),
            Is.EqualTo(""));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Render_NoContext_If(bool condition)
    {
        Assert.That(
            Render($$"""
                1,
                {{If(condition)}}
                2,
                {{End}}
                3,
                """),
            Is.EqualTo(condition
                ? """
                    1,
                    2,
                    3,
                    """
                : """
                    1,
                    3,
                    """));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Render_NoContext_If_Else(bool condition)
    {
        Assert.That(
            Render($$"""
                1,
                {{If(condition)}}
                2,
                {{Else}}
                3,
                {{End}}
                4,
                """),
            Is.EqualTo(condition
                ? """
                    1,
                    2,
                    4,
                    """
                : """
                    1,
                    3,
                    4,
                    """));
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Render_NoContext_If_Elif(bool condition1, bool condition2)
    {
        Assert.That(
            Render($$"""
                1,
                {{If(condition1)}}
                2,
                {{Elif(condition2)}}
                3,
                {{End}}
                4,
                """),
            Is.EqualTo(condition1
                ? """
                    1,
                    2,
                    4,
                    """
                : condition2
                    ? """
                        1,
                        3,
                        4,
                        """
                    : """
                        1,
                        4,
                        """));
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Render_NoContext_If_Elif_Else(bool condition1, bool condition2)
    {
        Assert.That(
            Render($$"""
                1,
                {{If(condition1)}}
                2,
                {{Elif(condition2)}}
                3,
                {{Else}}
                4,
                {{End}}
                5,
                """),
            Is.EqualTo(condition1
                ? """
                    1,
                    2,
                    5,
                    """
                : condition2
                    ? """
                        1,
                        3,
                        5,
                        """
                    : """
                        1,
                        4,
                        5,
                        """));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Render_NoContext_If_IResolvableTo(bool condition)
    {
        IResolvableTo<bool> cond = new LiteralProxy<bool>(condition);
        Assert.That(
            Render($$"""
                1,
                {{If(cond)}}
                2,
                {{End}}
                3,
                """),
            Is.EqualTo(condition
                ? """
                    1,
                    2,
                    3,
                    """
                : """
                    1,
                    3,
                    """));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Render_NoContext_If_Else_IResolvableTo(bool condition)
    {
        IResolvableTo<bool> cond = new LiteralProxy<bool>(condition);
        Assert.That(
            Render($$"""
                1,
                {{If(cond)}}
                2,
                {{Else}}
                3,
                {{End}}
                4,
                """),
            Is.EqualTo(condition
                ? """
                    1,
                    2,
                    4,
                    """
                : """
                    1,
                    3,
                    4,
                    """));
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Render_NoContext_If_Elif_IResolvableTo(bool condition1, bool condition2)
    {
        IResolvableTo<bool> cond1 = new LiteralProxy<bool>(condition1);
        IResolvableTo<bool> cond2 = new LiteralProxy<bool>(condition2);
        Assert.That(
            Render($$"""
                1,
                {{If(cond1)}}
                2,
                {{Elif(cond2)}}
                3,
                {{End}}
                4,
                """),
            Is.EqualTo(condition1
                ? """
                    1,
                    2,
                    4,
                    """
                : condition2
                    ? """
                        1,
                        3,
                        4,
                        """
                    : """
                        1,
                        4,
                        """));
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Render_NoContext_If_Elif_Else_IResolvableTo(bool condition1, bool condition2)
    {
        IResolvableTo<bool> cond1 = new LiteralProxy<bool>(condition1);
        IResolvableTo<bool> cond2 = new LiteralProxy<bool>(condition2);
        Assert.That(
            Render($$"""
                1,
                {{If(cond1)}}
                2,
                {{Elif(cond2)}}
                3,
                {{Else}}
                4,
                {{End}}
                5,
                """),
            Is.EqualTo(condition1
                ? """
                    1,
                    2,
                    5,
                    """
                : condition2
                    ? """
                        1,
                        3,
                        5,
                        """
                    : """
                        1,
                        4,
                        5,
                        """));
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Render_NoContext_If_Elif_Else_OneLine(bool condition1, bool condition2)
    {
        IResolvableTo<bool> cond1 = new LiteralProxy<bool>(condition1);
        IResolvableTo<bool> cond2 = new LiteralProxy<bool>(condition2);
        Assert.That(
            Render($$"""
                1{{If(cond1)}}2{{Elif(cond2)}}3{{Else}}4{{End}}5
                """),
            Is.EqualTo("1" + (condition1 ? "2" : condition2 ? "3" : "4") + "5"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Render_NoContext_Cond_IfTrue_IResolvableTo(bool condition)
    {
        IResolvableTo<bool> cond = new LiteralProxy<bool>(condition);
        IResolvableTo<string> ifTrue = new LiteralProxy<string>("2");
        Assert.That(
            Render($$"""
                1{{Cond(cond, ifTrue, "3")}}4
                """),
            Is.EqualTo(condition ? "124" : "134"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Render_NoContext_Cond_IfFalse_IResolvableTo(bool condition)
    {
        IResolvableTo<bool> cond = new LiteralProxy<bool>(condition);
        IResolvableTo<string> ifFalse = new LiteralProxy<string>("3");
        Assert.That(
            Render($$"""
                1{{Cond(cond, "2", ifFalse)}}4
                """),
            Is.EqualTo(condition ? "124" : "134"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Render_NoContext_Cond_IfTrueIfFalse_IResolvableTo(bool condition)
    {
        IResolvableTo<bool> cond = new LiteralProxy<bool>(condition);
        IResolvableTo<string> ifTrue = new LiteralProxy<string>("2");
        IResolvableTo<string> ifFalse = new LiteralProxy<string>("3");
        Assert.That(
            Render($$"""
                1{{Cond(cond, ifTrue, ifFalse)}}4
                """),
            Is.EqualTo(condition ? "124" : "134"));
    }

    [Test]
    public void Render_NoContext_Let()
    {
        Assert.That(
            Render($$"""
                {{For(0, 3, out var i)}}
                {{Let(out var v1, i * 2)}}
                {{Let(i * 3, out var v2)}}
                {{v1}},{{v2}}
                {{End}}
                """),
            Is.EqualTo("""
                0,0
                2,3
                4,6
                
                """));
    }

    [Test]
    public void Render_NoContext_Note()
    {
        _ctx.Clear();

        Assert.That(
            Render($$"""
                {{Note("This is a comment")}}
                {{Note("This is a comment")}}{{For(0, 3, out var i)}}{{Note("This is a comment")}}
                {{Note("This is a comment")}}
                {{Note("This is a comment")}}{{i}}{{Note("This is a comment")}}
                {{Note("This is a comment")}}{{Note("This is a comment")}}
                {{Note("This is a comment")}}{{End}}{{Note("This is a comment")}}
                {{Note("This is a comment")}}
                """),
            Is.EqualTo("""
                0
                1
                2
                
                """));
    }

    [Test]
    public void Render_NoContext_Continue()
    {
        Assert.That(
            Render($$"""
                {{For(0, 10, out var i)}}
                {{If(i % 2 == 1)}}
                {{Continue}}
                {{End}}                
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                0,
                2,
                4,
                6,
                8,
                
                """));
    }

    [Test]
    public void Render_NoContext_Continue_Nested()
    {
        Assert.That(
            Render($$"""
                {{For(0, 3, out var i)}}
                {{If(i == 1)}}
                {{Continue}}
                {{End}}
                {{For(0, 3, out var j)}}
                {{If(j == 2)}}
                {{Continue}}
                {{End}}
                {{i}}{{j}},
                {{End}}
                {{End}}
                """),
            Is.EqualTo("""
                00,
                01,
                20,
                21,
                
                """));
    }

    [Test]
    public void Render_NoContext_Break()
    {
        Assert.That(
            Render($$"""
                {{For(0, 10, out var i)}}
                {{If(i == 5)}}
                {{Break}}
                {{End}}                
                {{i}},
                {{End}}
                """),
            Is.EqualTo("""
                0,
                1,
                2,
                3,
                4,
                
                """));
    }

    [Test]
    public void Render_NoContext_Break_Nested()
    {
        Assert.That(
            Render($$"""
                {{For(0, 4, out var i)}}
                {{For(0, 3, out var j)}}
                {{If(j == 2)}}
                {{Break}}
                {{End}}
                {{i}}{{j}},
                {{End}}
                {{If(i == 2)}}
                {{Break}}
                {{End}}
                {{End}}
                """),
            Is.EqualTo("""
                00,
                01,
                10,
                11,
                20,
                21,
                
                """));
    }

    [Test]
    public void Render_NoContext_FizzBuzz()
    {
        Assert.That(
            Render($$"""
                {{For(1, 16, out var i)}}
                {{If(i.To(i => i % 15 == 0))}}
                FizzBuzz
                {{Elif(i.To(i => i % 3 == 0))}}
                Fizz
                {{Elif(i.To(i => i % 5 == 0))}}
                Buzz
                {{Else}}
                {{i}}
                {{End}}
                {{End}}
                """),
            Is.EqualTo("""
                1
                2
                Fizz
                4
                Buzz
                Fizz
                7
                8
                Fizz
                Buzz
                11
                Fizz
                13
                14
                FizzBuzz
                
                """));
    }

    private class UserDefinedContent : IBlockContent
    {
        public EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format)
        {
            return EvalResult.Create($"hoge(a={alignment}, f={format})");
        }
    }

    [Test]
    public void Render_NoContext_UserDefinedContent()
    {
        Assert.That(Render($"{new UserDefinedContent(),10:fuga}"), Is.EqualTo("hoge(a=10, f=fuga)"));
    }
}
