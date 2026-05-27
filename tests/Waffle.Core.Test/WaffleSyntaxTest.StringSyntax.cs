// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.Core.Test;

public partial class WaffleSyntaxTest
{
    [Test]
    public void RenderCSharp_NoContextNoInterpolation_ProducesSameResultAsRender()
    {
        var actual = RenderCSharp("""
            public enum HogeHoge
            {
                A,
                B
            }
        """);
        var expected = Render("""
            public enum HogeHoge
            {
                A,
                B
            }
        """);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void RenderCSharp_NoContextWithInterpolation_ProducesSameResultAsRender()
    {
        var values = new[] { "A", "B", "C" };

        var actual = RenderCSharp($$"""
            public enum HogeHoge
            {
                {{ForEach(values, out var value1)}}
                {{value1}},
                {{End}}
            }
        """);
        var expected = Render($$"""
            public enum HogeHoge
            {
                {{ForEach(values, out var value2)}}
                {{value2}},
                {{End}}
            }
        """);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void RenderCSharp_WithContextNoInterpolation_ProducesSameResultAsRender()
    {
        var ctx1 = new InstantTemplateContext();
        RenderCSharp(ctx1, """
            public enum HogeHoge
            {
                A,
                B
            }
        """);
        var ctx2 = new InstantTemplateContext();
        Render(ctx2, """
            public enum HogeHoge
            {
                A,
                B
            }
        """);
        Assert.That(ctx1.GetResult(), Is.EqualTo(ctx2.GetResult()));
    }

    [Test]
    public void RenderCSharp_WithContextWithInterpolation_ProducesSameResultAsRender()
    {
        var values = new[] { "A", "B", "C" };

        var ctx1 = new InstantTemplateContext();
        RenderCSharp(ctx1, $$"""
            public enum HogeHoge
            {
                {{ForEach(values, out var value1)}}
                {{value1}},
                {{End}}
            }
        """);
        var ctx2 = new InstantTemplateContext();
        Render(ctx2, $$"""
            public enum HogeHoge
            {
                {{ForEach(values, out var value2)}}
                {{value2}},
                {{End}}
            }
        """);
        Assert.That(ctx1.GetResult(), Is.EqualTo(ctx2.GetResult()));
    }
}
