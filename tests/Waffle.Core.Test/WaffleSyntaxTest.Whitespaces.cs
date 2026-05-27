// (c) DeNA Co., Ltd.

using Waffle.ModelProxy;

namespace Waffle.Core.Test;

public partial class WaffleSyntaxTest
{
    [ModelProxy]
    public readonly record struct StructModel(string Name, PropertyModel[] Properties);

    [ModelProxy]
    public readonly record struct PropertyModel(string Type, string Name, string PrivateName);

    private static readonly StructModel s_structModel = new("ReadOnlyIntVector3", new[]
    {
        new PropertyModel("int", "X", "x"),
        new PropertyModel("int", "Y", "y"),
        new PropertyModel("int", "Z", "z"),
    });

    //lang=cs
    private const string ExpectedStruct = """
public readonly partial struct ReadOnlyIntVector3
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;
    public ReadOnlyIntVector3(
        int x,
        int y,
        int z
    ) {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }
}
""";

    [Test]
    public void Render_LineContainingOnlyCommand_IsNotOutput()
    {
        var actual = Render($$"""
            {{Let(out var x, 1)}}
            {{x}}
            """);
        Assert.That(actual, Is.EqualTo("1"));

        actual = Render($$"""
            leading
            {{Let(out x, 1)}}
            {{x}}
            """);
        Assert.That(actual, Is.EqualTo("""
           leading
           1
           """));

        actual = Render($$"""
            {{Let(out _, 1)}}
            """);
        Assert.That(actual, Is.EqualTo(""));

        // The line break after "leading" belongs to the "leading" line, so an empty line remains where the command was
        actual = Render($$"""
            leading
            {{Let(out _, 1)}}
            """);
        Assert.That(actual, Is.EqualTo("""
           leading
           
           """));
    }

    [Test]
    public void Render_LineContainingOnlyCommandAndSpaces_IsNotOutput()
    {
        var actual = Render($$"""
                {{Let(out var x, 1)}}  
            {{x}}
            """);
        Assert.That(actual, Is.EqualTo("1"));
    }

    [Test]
    public void Render_LineContainingOnlyFlowCompletion_IsNotOutput()
    {
        var m = s_structModel.AsProxy();
        var actual = Render($$"""
            public readonly partial struct {{m.Name}}
            {
            {{ForEach(m.Properties, out var p)}}
                public readonly {{p.Type}} {{p.Name}};
            {{End}}
            {{If(m.Properties.Count > 0)}}
                public {{m.Name}}(
            {{ForEach(m.Properties, out p, out _, out var h)}}
                    {{p.Type}} {{p.PrivateName}}{{h.CommaOrLastEmpty}}
            {{End}}
                ) {
            {{ForEach(m.Properties, out p)}}
                    this.{{p.Name}} = {{p.PrivateName}};
            {{End}}
                }
            {{End}}
            }
            """);
        Assert.That(actual, Is.EqualTo(ExpectedStruct));
    }

    [Test]
    public void Render_LineContainingOnlyFlowCompletionAndWhitespace_IsNotOutput()
    {
        var m = s_structModel.AsProxy();
        var actual = Render($$"""
            public readonly partial struct {{m.Name}}
            {
            {{ForEach(m.Properties, out var p)}}
                public readonly {{p.Type}} {{p.Name}};
            {{End}}
            {{If(m.Properties.Count > 0)}}
                public {{m.Name}}(
                {{ForEach(m.Properties, out p, out _, out var h)}}
                    {{p.Type}} {{p.PrivateName}}{{h.CommaOrLastEmpty}}
                {{End}}
                ) {
                {{ForEach(m.Properties, out p)}}
                    this.{{p.Name}} = {{p.PrivateName}};
                {{End}}
                }
            {{End}}
            }
            """);
        Assert.That(actual, Is.EqualTo(ExpectedStruct));
    }

    [Test]
    public void Render_LineContainingOnlyConsecutiveFlowCompletions_IsNotOutput()
    {
        var m = s_structModel.AsProxy();
        var actual = Render($$"""
            public readonly partial struct {{m.Name}}
            {
            {{ForEach(m.Properties, out var p)}}
                public readonly {{p.Type}} {{p.Name}};
            {{End}}{{If(m.Properties.Count > 0)}}
                public {{m.Name}}(
            {{ForEach(m.Properties, out p, out _, out var h)}}
                    {{p.Type}} {{p.PrivateName}}{{h.CommaOrLastEmpty}}
            {{End}}
                ) {
            {{ForEach(m.Properties, out var prop)}}{{Let(out p, prop)}}
                    this.{{p.Name}} = {{p.PrivateName}};
            {{End}}
                }
            {{End}}
            }
            """);
        Assert.That(actual, Is.EqualTo(ExpectedStruct));
    }

    [Test]
    public void Render_LineContainingOnlyConsecutiveFlowCompletionsAndWhitespace_IsNotOutput()
    {
        var m = s_structModel.AsProxy();
        var actual = Render($$"""
            public readonly partial struct {{m.Name}}
            {
            {{ForEach(m.Properties, out var p)}}
                public readonly {{p.Type}} {{p.Name}};
            {{End}}{{If(m.Properties.Count > 0)}}
                public {{m.Name}}(
                {{ForEach(m.Properties, out p, out _, out var h)}}
                    {{p.Type}} {{p.PrivateName}}{{h.CommaOrLastEmpty}}
                {{End}}
                ) {
                {{ForEach(m.Properties, out var prop)}}{{Let(out p, prop)}}
                    this.{{p.Name}} = {{p.PrivateName}};
                {{End}}
                }
            {{End}}
            }
            """);
        Assert.That(actual, Is.EqualTo(ExpectedStruct));
    }

    [Test]
    public void Render_IterationBlockStartLineHasElements_NoPrecedingOrFollowingContent_EndLineIsTrimmed()
    {
        Assert.That(
            Render($$"""
                {{ForEach([1, 2, 3], out var i)}}{{i}}
                {{End}}
            """),
            Is.EqualTo("""
                1
            2
            3

            """));
    }

    [Test]
    public void Render_IterationBlockStartLineHasElements_WithFollowingContent_EndLineIsTrimmed()
    {
        Assert.That(
            Render($$"""
                {{ForEach([1, 2, 3], out var i)}}{{i}}
                {{End}}
            Trailing
            """),
            Is.EqualTo("""
                1
            2
            3
            Trailing
            """));
    }

    [Test]
    public void Render_IterationBlockStartLineHasElements_WithPrecedingContent_EndLineIsTrimmed()
    {
        Assert.That(
            Render($$"""
            Leading
                {{ForEach([1, 2, 3], out var i)}}{{i}}
                {{End}}
            """),
            Is.EqualTo("""
            Leading
                1
            2
            3

            """));
    }

    [Test]
    public void Render_IterationBlockStartLineHasElements_WithBothPrecedingAndFollowingContent_EndLineIsTrimmed()
    {
        Assert.That(
            Render($$"""
            Leading
                {{ForEach([1, 2, 3], out var i)}}{{i}}
                {{End}}
            Trailing
            """),
            Is.EqualTo("""
            Leading
                1
            2
            3
            Trailing
            """));
    }

    [Test]
    public void Render_Format_Lt_TrimsConsecutiveSpacesBeforeOnSameLine()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                2   {{x:<}}   3
                """),
            Is.EqualTo("""
                2x   3
                """));
    }

    [Test]
    public void Render_Format_Lt_DoesNotCrossOtherInterpolations()
    {
        var x = "x";
        var y = "y";
        Assert.That(
            Render($$"""
                2   {{y}}{{x:<}}   3
                """),
            Is.EqualTo("""
                2   yx   3
                """));
    }

    [Test]
    public void Render_Format_Gt_TrimsConsecutiveSpacesAfterOnSameLine()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                2   {{x:>}}   3
                """),
            Is.EqualTo("""
                2   x3
                """));
    }

    [Test]
    public void Render_Format_Gt_DoesNotCrossOtherInterpolations()
    {
        var x = "x";
        var y = "y";
        Assert.That(
            Render($$"""
                2   {{x:>}}{{y}}   3
                """),
            Is.EqualTo("""
                2   xy   3
                """));
    }

    [Test]
    public void Render_Format_LtGt_TrimsConsecutiveSpacesBeforeAndAfterOnSameLine()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                2   {{x:<>}}   3
                """),
            Is.EqualTo("""
                2x3
                """));
    }

    [Test]
    public void Render_Format_LtGt_DoesNotCrossOtherInterpolations()
    {
        var x = "x";
        var y = "y";
        Assert.That(
            Render($$"""
                2   {{y}}{{x:<>}}{{y}}   3
                """),
            Is.EqualTo("""
                2   yxy   3
                """));
    }

    [Test]
    public void Render_Format_GtLt_TrimsConsecutiveSpacesBeforeAndAfterOnSameLine()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                2   {{x:><}}   3
                """),
            Is.EqualTo("""
                2x3
                """));
    }

    [Test]
    public void Render_Format_GtLt_DoesNotCrossOtherInterpolations()
    {
        var x = "x";
        var y = "y";
        Assert.That(
            Render($$"""
                2   {{y}}{{x:><}}{{y}}   3
                """),
            Is.EqualTo("""
                2   yxy   3
                """));
    }

    [Test]
    public void Render_Format_LtLt_TrimsConsecutiveSpacesAndLineBreaksBefore()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                2 
                
                   
                  {{x:<<}}   3
                """),
            Is.EqualTo("""
                2x   3
                """));
    }

    [Test]
    public void Render_Format_LtLt_DoesNotCrossOtherInterpolations()
    {
        var x = "x";
        var y = "y";
        Assert.That(
            Render($$"""
                2 
                
                   
                  {{y}}{{x:<<}}   3
                """),
            Is.EqualTo("""
                2 
                
                   
                  yx   3
                """));
    }

    [Test]
    public void Render_Format_GtGt_TrimsConsecutiveSpacesAndLineBreaksAfter()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                2   {{x:>>}}   
                
                  
                3
                """),
            Is.EqualTo("""
                2   x3
                """));
    }

    [Test]
    public void Render_Format_GtGt_DoesNotCrossOtherInterpolations()
    {
        var x = "x";
        var y = "y";
        Assert.That(
            Render($$"""
                2   {{x:>>}}{{y}}   
                
                  
                3
                """),
            Is.EqualTo("""
                2   xy   
                
                  
                3
                """));
    }

    [Test]
    public void Render_Format_LtLtGtGt_TrimsConsecutiveSpacesAndLineBreaksBeforeAndAfter()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                2 
                
                   
                  {{x:<<>>}}
                
                     3
                """),
            Is.EqualTo("""
                2x3
                """));
    }

    [Test]
    public void Render_Format_LtLtGtGt_DoesNotCrossOtherInterpolations()
    {
        var x = "x";
        var y = "y";
        Assert.That(
            Render($$"""
                2 
                
                   
                  {{y}}{{x:<<>>}}{{y}}
                
                     3
                """),
            Is.EqualTo("""
                2 
                
                   
                  yxy
                
                     3
                """));
    }

    [Test]
    public void Render_Format_GtGtLtLt_TrimsConsecutiveSpacesAndLineBreaksBeforeAndAfter()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                2   
                
                   {{x:>><<}} 
                
                       
                  
                3
                """),
            Is.EqualTo("""
                2x3
                """));
    }

    [Test]
    public void Render_Format_GtGtLtLt_DoesNotCrossOtherInterpolations()
    {
        var x = "x";
        var y = "y";
        Assert.That(
            Render($$"""
                2   
                
                   {{y}}{{x:<<>>}}{{y}} 
                
                       
                  
                3
                """),
            Is.EqualTo($"""
                2   
                
                   yxy 
                
                       
                  
                3
                """));
    }


    [Test]
    public void Render_Format_Lt_AtLineStart_NothingHappens()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                1
                
                {{x:<}}   3
                """),
            Is.EqualTo("""
                1
                
                x   3
                """));
    }

    [Test]
    public void Render_Format_Lt_AtStringStart_NothingHappens()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                {{x:<}}   3

                """),
            Is.EqualTo("""
                x   3
                
                """));
    }

    [Test]
    public void Render_Format_Gt_AtLineEnd_NothingHappens()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                2   {{x:>}}
                
                3
                """),
            Is.EqualTo("""
                2   x
                
                3
                """));
    }

    [Test]
    public void Render_Format_Gt_AtStringEnd_NothingHappens()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                1
                
                2   {{x:>}}
                """),
            Is.EqualTo("""
                1
                
                2   x
                """));
    }

    [Test]
    public void Render_Format_LtLt_AtStringStart_NothingHappens()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                {{x:<<}}   3

                4
                """),
            Is.EqualTo("""
                x   3
                
                4
                """));
    }

    [Test]
    public void Render_Format_GtGt_AtStringEnd_NothingHappens()
    {
        var x = "x";
        Assert.That(
            Render($$"""
                1
                
                2   {{x:>>}}
                """),
            Is.EqualTo("""
                1
                
                2   x
                """));
    }

    [Test]
    public void Render_Format_InsideLoop_NoLineBreaks()
    {
        Assert.That(
            Render($$"""
                {{For(0, 3, out var i)}}
                    {{For(0, 2, out var j)}}
                    {{i * j:<>}}  foo  {{i * j:<>}}  bar  {{i * j:<>}}   
                    {{End}}
                {{End}}
                """),
            Is.EqualTo("""
                0foo0bar0
                0foo0bar0
                0foo0bar0
                1foo1bar1
                0foo0bar0
                2foo2bar2
                
                """));
    }

    [Test]
    public void Render_Format_InsideLoop_WithLineBreaks()
    {
        Assert.That(
            Render($$"""
                {{For(0, 3, out var i)}}
                    {{For(0, 2, out var j)}}
                    {{i * j:<>}}  foo  {{i * j:<>}}  bar  {{i * j:<>>}}   
                    
                     
                    {{End}}
                {{End}}
                """),
            Is.EqualTo("""
                0foo0bar00foo0bar00foo0bar01foo1bar10foo0bar02foo2bar2
                """));
    }

    [Test]
    public void Render_Format_GtGt_InsideLoop_DoesNotTrimContentOutsideLoop()
    {
        Assert.That(
            Render($$"""
                {{For(0, 3, out var i)}}
                    {{For(0, 2, out var j)}}
                    {{i * j:<>}}  foo  {{i * j:<>}}  bar  {{i * j:<>>}}   
                    
                     
                    {{End}}
                {{End}}
                
                 
                """),

            // NOTE: The single line break immediately after End is correctly removed because End performs flow completion
            //       The missing line break inside the loop makes it look as if a line was removed
            Is.EqualTo("""
                0foo0bar00foo0bar00foo0bar01foo1bar10foo0bar02foo2bar2
                 
                """));
    }

    [Test]
    public void Render_FirstIterationLeadingTrim_MultiLineParamsWithPrefix()
    {
        var args = new[] { "int x", "string y", "bool z" };
        var actual = Render($$"""
            if ({{ForEach(args, out var c, out _, out var h):>|}}
                {{c}}{{h.LastOrNot("", " &&")}}
                {{End:<<}})
            {
            }
            """);
        Assert.That(actual, Is.EqualTo("""
            if (int x &&
                string y &&
                bool z)
            {
            }
            """));
    }

    [Test]
    public void Render_FirstIterationLeadingTrim_SingleItem()
    {
        var args = new[] { "int x" };
        var actual = Render($$"""
            if ({{ForEach(args, out var c, out _, out var h):>|}}
                {{c}}{{h.LastOrNot("", " &&")}}
                {{End:<<}})
            {
            }
            """);
        Assert.That(actual, Is.EqualTo("""
            if (int x)
            {
            }
            """));
    }

    [Test]
    public void Render_FirstIterationLeadingTrim_EmptyList()
    {
        var args = Array.Empty<string>();
        var actual = Render($$"""
            if ({{ForEach(args, out var c, out _, out var h):>|}}
                {{c}}{{h.LastOrNot("", " &&")}}
                {{End:<<}})
            {
            }
            """);
        Assert.That(actual, Is.EqualTo("""
            if ()
            {
            }
            """));
    }

    [Test]
    public void Render_FirstIterationLeadingTrim_OnlyTrimsFirstIteration()
    {
        var items = new[] { "A", "B", "C" };
        var actual = Render($$"""
            [{{ForEach(items, out var item, out _):>|}}
             {{item}},{{End:<<}}]
            """);
        // First iteration: "\n " is trimmed → "A," directly after "["
        // Subsequent iterations: "\n " is preserved
        Assert.That(actual, Is.EqualTo("""
            [A,
             B,
             C,]
            """));
    }
}
