// (c) DeNA Co., Ltd.

namespace Waffle.Core.Test;

public partial class WaffleSyntaxTest
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Recipes: Comma-Separated Parameter Lists
    // ─────────────────────────────────────────────────────────────────────────────

    [Test]
    public void Recipes_CommaSeparated_SingleLine_LazyJoin_NonEmpty()
    {
        var args = new[] { "int x", "string name", "bool flag" };

        Assert.That(Render($$"""
            void DoSomething({{args.AsProxy().Join(", ")}})
            """),
            Is.EqualTo("void DoSomething(int x, string name, bool flag)"));
    }

    [Test]
    public void Recipes_CommaSeparated_SingleLine_LazyJoin_Empty()
    {
        var args = Array.Empty<string>();

        Assert.That(Render($$"""
            void DoSomething({{args.AsProxy().Join(", ")}})
            """),
            Is.EqualTo("void DoSomething()"));
    }

    [Test]
    public void Recipes_CommaSeparated_SingleLine_ForEach_NonEmpty()
    {
        var args = new[] { "int x", "string name", "bool flag" };

        Assert.That(Render($$"""
            void DoSomething({{ForEach(args, out var arg, out _, out var h)}}{{arg}}{{h.CommaSpaceOrLastParen}}{{End}}
            """),
            Is.EqualTo("void DoSomething(int x, string name, bool flag)"));
    }

    [Test]
    public void Recipes_CommaSeparated_MultiLine_ClosingParenOnLastLine_NonEmpty()
    {
        var args = new[] { "int x", "string name", "bool flag" };

        Assert.That(Render($$"""
                void DoSomething({{ForEach(args, out var arg, out _, out var h)}}
                    {{arg}}{{h.CommaOrLastEmpty}}{{End}})
                """),
            Is.EqualTo("""
                void DoSomething(
                    int x,
                    string name,
                    bool flag)
                """));
    }

    [Test]
    public void Recipes_CommaSeparated_MultiLine_ClosingParenOnLastLine_Empty()
    {
        var args = Array.Empty<string>();

        Assert.That(Render($$"""
            void DoSomething({{ForEach(args, out var arg, out _, out var h)}}
                {{arg}}{{h.CommaOrLastEmpty}}{{End}})
            """),
            Is.EqualTo("void DoSomething()"));
    }

    [Test]
    public void Recipes_CommaSeparated_MultiLine_ClosingParenOnOwnLine_NonEmpty()
    {
        var args = new[] { "int x", "string name", "bool flag" };

        Assert.That(Render($$"""
                void DoSomething(
                {{ForEach(args, out var arg, out _, out var h)}}
                    {{arg}}{{h.CommaOrLastEmpty}}
                {{End}}
                )
                """),
            Is.EqualTo("""
                void DoSomething(
                    int x,
                    string name,
                    bool flag
                )
                """));
    }

    [Test]
    public void Recipes_CommaSeparated_MultiLine_ClosingParenOnOwnLine_Empty()
    {
        var args = Array.Empty<string>();

        Assert.That(Render($$"""
                void DoSomething(
                {{ForEach(args, out var arg, out _, out var h)}}
                    {{arg}}{{h.CommaOrLastEmpty}}
                {{End}}
                )
                """),
            Is.EqualTo("""
                void DoSomething(
                )
                """));
    }

    [Test]
    public void Recipes_CommaSeparated_MultiLine_WithIfBranch_NonEmpty()
    {
        var args = new[] { "int x", "string name", "bool flag" };

        Assert.That(Render($$"""
                {{If(args.Length > 0)}}
                void DoSomething(
                {{ForEach(args, out var arg, out _, out var h)}}
                    {{arg}}{{h.CommaOrLastEmpty}}
                {{End}}
                )
                {{Else}}
                void DoSomething()
                {{End}}
                """),
            Is.EqualTo("""
                void DoSomething(
                    int x,
                    string name,
                    bool flag
                )

                """));
    }

    [Test]
    public void Recipes_CommaSeparated_MultiLine_WithIfBranch_Empty()
    {
        var args = Array.Empty<string>();

        Assert.That(Render($$"""
                {{If(args.Length > 0)}}
                void DoSomething(
                {{ForEach(args, out var arg, out _, out var h)}}
                    {{arg}}{{h.CommaOrLastEmpty}}
                {{End}}
                )
                {{Else}}
                void DoSomething()
                {{End}}
                """),
            Is.EqualTo("""
                void DoSomething()

                """));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Recipes: Generic Type Parameters
    // ─────────────────────────────────────────────────────────────────────────────

    [Test]
    public void Recipes_GenericTypeParams_LazyJoin_NonEmpty()
    {
        var typeParams = new[] { "T", "U" };

        Assert.That(Render($$"""
            class MyClass{{typeParams.AsProxy().Join(", ", "<", ">")}}
            """),
            Is.EqualTo("class MyClass<T, U>"));
    }

    [Test]
    public void Recipes_GenericTypeParams_LazyJoin_Empty()
    {
        var typeParams = Array.Empty<string>();

        Assert.That(Render($$"""
            class MyClass{{typeParams.AsProxy().Join(", ", "<", ">")}}
            """),
            Is.EqualTo("class MyClass"));
    }

    [Test]
    public void Recipes_GenericTypeParams_ForEach_NonEmpty()
    {
        var typeParams = new[] { "T", "U" };

        Assert.That(Render($$"""
            class MyClass{{If(typeParams.Length > 0)}}<{{ForEach(typeParams, out var t, out _, out var ht)}}{{t}}{{ht.CommaSpaceOrLastEmpty}}{{End}}>{{End}}
            """),
            Is.EqualTo("class MyClass<T, U>"));
    }

    [Test]
    public void Recipes_GenericTypeParams_ForEach_Empty()
    {
        var typeParams = Array.Empty<string>();

        Assert.That(Render($$"""
            class MyClass{{If(typeParams.Length > 0)}}<{{ForEach(typeParams, out var t, out _, out var ht)}}{{t}}{{ht.CommaSpaceOrLastEmpty}}{{End}}>{{End}}
            """),
            Is.EqualTo("class MyClass"));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Recipes: Conditional Wrapping
    // ─────────────────────────────────────────────────────────────────────────────

    [Test]
    public void Recipes_ConditionalWrapping_Enabled()
    {
        var needsNullable = true;

        Assert.That(Render($$"""
                {{If(needsNullable)}}
                #nullable enable
                {{End}}
                public class Foo { }
                {{If(needsNullable)}}
                #nullable restore
                {{End}}
                """),
            Is.EqualTo("""
                #nullable enable
                public class Foo { }
                #nullable restore

                """));
    }

    [Test]
    public void Recipes_ConditionalWrapping_Disabled()
    {
        var needsNullable = false;

        Assert.That(Render($$"""
                {{If(needsNullable)}}
                #nullable enable
                {{End}}
                public class Foo { }
                {{If(needsNullable)}}
                #nullable restore
                {{End}}
                """),
            Is.EqualTo("""
                public class Foo { }

                """));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Recipes: Indented Code Blocks with Auto Indentation
    // ─────────────────────────────────────────────────────────────────────────────

    [Test]
    public void Recipes_AutoIndent_FieldDeclarations()
    {
        var fields = new[] { "int X", "int Y", "int Z" };

        var fieldDeclarations = Render($$"""
            {{ForEach(fields, out var f)}}
            public {{f}} { get; set; }
            {{End}}
            """);

        Assert.That(fieldDeclarations,
            Is.EqualTo("""
                public int X { get; set; }
                public int Y { get; set; }
                public int Z { get; set; }

                """));
    }

    [Test]
    public void Recipes_AutoIndent_NestedStructure()
    {
        var fields = new[] { "int X", "int Y", "int Z" };

        var fieldDeclarations = Render($$"""
            {{ForEach(fields, out var f)}}
            public {{f}} { get; set; }
            {{End}}
            """);

        // Auto-indentation inserts the 4-space indent after every newline in fieldDeclarations,
        // including the trailing newline, which produces a "    \n" line before the closing brace.
        Assert.That(Render($$"""
                public class Vector
                {
                    {{fieldDeclarations}}
                }
                """),
            Is.EqualTo("""
                public class Vector
                {
                    public int X { get; set; }
                    public int Y { get; set; }
                    public int Z { get; set; }
                    
                }
                """));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Recipes: Switch/Enum Generation
    // ─────────────────────────────────────────────────────────────────────────────

    [Test]
    public void Recipes_EnumGeneration()
    {
        var members = new[] { ("None", 0), ("Read", 1), ("Write", 2), ("Execute", 4) };

        Assert.That(Render($$"""
                [Flags]
                public enum Permission
                {
                {{ForEach(members, out var m, out _, out var h)}}
                    {{m.To(x => x.Item1)}} = {{m.To(x => x.Item2)}}{{h.LastOrNot("", ",")}}
                {{End}}
                }
                """),
            Is.EqualTo("""
                [Flags]
                public enum Permission
                {
                    None = 0,
                    Read = 1,
                    Write = 2,
                    Execute = 4
                }
                """));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Recipes: Builder-Style Fluent Chains
    // ─────────────────────────────────────────────────────────────────────────────

    [Test]
    public void Recipes_BuilderStyleChain_NonEmpty()
    {
        var options = new[] { ".UseRouting()", ".UseAuthentication()", ".UseAuthorization()" };

        Assert.That(Render($$"""
                var app = builder.Build(){{ForEach(options, out var opt)}}
                    {{opt}}{{End}};
                """),
            Is.EqualTo("""
                var app = builder.Build()
                    .UseRouting()
                    .UseAuthentication()
                    .UseAuthorization();
                """));
    }

    [Test]
    public void Recipes_BuilderStyleChain_Empty()
    {
        var options = Array.Empty<string>();

        Assert.That(Render($$"""
            var app = builder.Build(){{ForEach(options, out var opt)}}
                {{opt}}{{End}};
            """),
            Is.EqualTo("var app = builder.Build();"));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Recipes: Multi-Line String with Prefix on First Line (>| trim)
    // ─────────────────────────────────────────────────────────────────────────────

    [Test]
    public void Recipes_MultiLineWithPrefix_MultipleConditions()
    {
        var conditions = new[] { "x > 0", "y != null", "z.IsValid" };

        Assert.That(Render($$"""
                if ({{ForEach(conditions, out var c, out _, out var h):>|}}
                    {{c}}{{h.LastOrNot("", " &&")}}
                    {{End:<<}})
                {
                }
                """),
            Is.EqualTo("""
                if (x > 0 &&
                    y != null &&
                    z.IsValid)
                {
                }
                """));
    }

    [Test]
    public void Recipes_MultiLineWithPrefix_SingleCondition()
    {
        var conditions = new[] { "x > 0" };

        Assert.That(Render($$"""
                if ({{ForEach(conditions, out var c, out _, out var h):>|}}
                    {{c}}{{h.LastOrNot("", " &&")}}
                    {{End:<<}})
                {
                }
                """),
            Is.EqualTo("""
                if (x > 0)
                {
                }
                """));
    }

    [Test]
    public void Recipes_MultiLineWithPrefix_EmptyList()
    {
        var conditions = Array.Empty<string>();

        Assert.That(Render($$"""
                if ({{ForEach(conditions, out var c, out _, out var h):>|}}
                    {{c}}{{h.LastOrNot("", " &&")}}
                    {{End:<<}})
                {
                }
                """),
            Is.EqualTo("""
                if ()
                {
                }
                """));
    }

    [Test]
    public void Recipes_MultiLineWithPrefix_Alternative_MultipleConditions()
    {
        var conditions = new[] { "x > 0", "y != null", "z.IsValid" };

        Assert.That(Render($$"""
                if ({{ForEach(conditions, out var c, out _, out var h)}}{{c}}{{h.LastOrNot("", " &&\n    ")}}{{End}})
                {
                }
                """),
            Is.EqualTo("""
                if (x > 0 &&
                    y != null &&
                    z.IsValid)
                {
                }
                """));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Recipes: Using << Trim to Collapse Trailing Whitespace in Loops
    // ─────────────────────────────────────────────────────────────────────────────

    [Test]
    public void Recipes_TrailingTrimCollapse_WithEndTrim()
    {
        var items = new[] { "first", "second", "third" };

        Assert.That(Render($$"""
                items: [{{ForEach(items, out var item, out _, out var h)}}
                    {{item}}{{h.CommaOrLastEmpty}}
                    {{End:<<}}]
                """),
            Is.EqualTo("""
                items: [
                    first,
                    second,
                    third]
                """));
    }

    [Test]
    public void Recipes_TrailingTrimCollapse_EndOnSameLine()
    {
        var items = new[] { "first", "second", "third" };

        Assert.That(Render($$"""
                items: [{{ForEach(items, out var item, out _, out var h)}}
                    {{item}}{{h.CommaOrLastEmpty}}{{End}}]
                """),
            Is.EqualTo("""
                items: [
                    first,
                    second,
                    third]
                """));
    }
}
