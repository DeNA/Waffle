// (c) DeNA Co., Ltd.

namespace Waffle.Core.Test;

public partial class WaffleSyntaxTest
{
    // Tests for the auto-indent feature
    [Test]
    public void AutoIndent_MultilineString_IndentIsApplied()
    {
        var x = "1\n2\n3";
        var actual = Render($$"""
            leading
                {{x}}trailing1
            trailing2
            """);
        Assert.That(actual, Is.EqualTo("""
            leading
                1
                2
                3trailing1
            trailing2
            """));
    }

    [Test]
    public void AutoIndent_MultilineStringWithTrailingNewline_IndentIsAlsoAppliedToTrailingLine()
    {
        var x = "1\n2\n3\n";
        var actual = Render($$"""
            leading
                {{x}}trailing1
            trailing2
            """);
        Assert.That(actual, Is.EqualTo("""
            leading
                1
                2
                3
                trailing1
            trailing2
            """));
    }

    [Test]
    public void AutoIndent_LeftTrimWithLineBreak_ProcessedAfterTrimAndNoIndentApplied()
    {
        var x = "1\n2\n3\n";
        var actual = Render($$"""
            leading
                {{x:<<}}trailing1
            trailing2
            """);
        Assert.That(actual, Is.EqualTo("""
            leading1
            2
            3
            trailing1
            trailing2
            """));
    }

    [Test]
    public void AutoIndent_VerbatimFormat_IndentIsNotApplied()
    {
        var x = "1\n2\n3\n";
        var actual = Render($$"""
            leading
                {{x:v}}trailing1
            trailing2
            """);
        Assert.That(actual, Is.EqualTo("""
            leading
                1
            2
            3
            trailing1
            trailing2
            """));
    }

    [Test]
    public void AutoIndent_LeftTrimCombinedWithVerbatim_ContentInterpolatedWithoutIndentAfterTrim()
    {
        var x = "1\n2\n3\n";
        var actual = Render($$"""
            leading
                {{x:<v}}trailing1
            trailing2
            """);
        Assert.That(actual, Is.EqualTo("""
            leading
            1
            2
            3
            trailing1
            trailing2
            """));
    }

    [Test]
    public void AutoIndent_LeftTrimWithLineBreakCombinedWithR_ContentInterpolatedWithoutIndentAfterTrim()
    {
        var x = "1\n2\n3\n";
        var actual = Render($$"""
            leading
                {{x:<<r}}trailing1
            trailing2
            """);
        Assert.That(actual, Is.EqualTo("""
            leading1
            2
            3
            trailing1
            trailing2
            """));
    }

    [Test]
    public void AutoIndent_InterpolationNotAtLineStart_IndentIsNotApplied()
    {
        var x = "1\n2\n3\n";
        var actual = Render($$"""
            leading ABC {{x}}trailing1
            trailing2
            """);
        Assert.That(actual, Is.EqualTo("""
            leading ABC 1
            2
            3
            trailing1
            trailing2
            """));
    }

    [Test]
    public void AutoIndent_SingleLineString_NoChange()
    {
        var x = "singleline";
        var actual = Render($$"""
            leading
                {{x}}trailing
            """);
        Assert.That(actual, Is.EqualTo("""
            leading
                singlelinetrailing
            """));
    }

    [Test]
    public void AutoIndent_ZeroIndentInterpolationAtLineStart_NoExtraWhitespace()
    {
        var x = "1\n2\n3\n";
        // No whitespace before {{x}}
        var actual = Render($$"""
            leading
            {{x}}trailing1
            trailing2
            """);
        // indent is "" so no change to the value
        Assert.That(actual, Is.EqualTo("""
            leading
            1
            2
            3
            trailing1
            trailing2
            """));
    }

    [Test]
    public void AutoIndent_InsideForEachLoop_NoTrailingNewline_IndentIsAppliedToEachIteration()
    {
        var items = new[] { "a\nb", "c\nd" };
        var actual = Render($$"""
            {{ForEach(items, out var item)}}
                {{item}}
            {{End}}
            """);
        Assert.That(actual, Is.EqualTo("""
                a
                b
                c
                d
            
            """));
    }

    [Test]
    public void AutoIndent_InsideForEachLoop_WithTrailingNewline_IndentIsAppliedToEachIteration()
    {
        var items = new[] { "a\nb\n", "c\nd\n" };
        var actual = Render($$"""
            {{ForEach(items, out var item)}}
                {{item}}
            {{End}}
            """);
        Assert.That(actual, Is.EqualTo("""
                a
                b
                
                c
                d
                
            
            """));
    }

    [Test]
    public void AutoIndent_NoTrailingNewline_FollowingContentContinuesDirectly()
    {
        var x = "1\n2\n3";
        var actual = Render($$"""
            leading
                {{x}}trailing
            """);
        Assert.That(actual, Is.EqualTo("""
            leading
                1
                2
                3trailing
            """));
    }

    [Test]
    public void AutoIndent_VerbatimFormat_CanBeCombinedWithStandardFormat()
    {
        // 'v' is stripped and the remaining format string (D5) is applied normally
        var x = 1234;
        var actual = Render($$"""
            leading
                {{x:vD5}}trailing
            """);
        Assert.That(actual, Is.EqualTo($"leading\n    {x:D5}trailing"));
    }

    [Test]
    public void AutoIndent_PrecedingTokenIsInterpolation_AutoIndentNotApplied()
    {
        var prefix = "  ";
        var x = "1\n2\n3\n";
        // prefix is an interpolation before x on the same line, so GetAutoIndent should return null
        var actual = Render($$"""
            leading
            {{prefix}}{{x}}trailing
            """);
        // No auto-indent because immediately preceding token is an interpolation, not a literal
        Assert.That(actual, Is.EqualTo("leading\n  1\n2\n3\ntrailing"));
    }
}
