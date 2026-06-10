// (c) DeNA Co., Ltd.

namespace Waffle.Core.Test;

public partial class WaffleSyntaxTest
{
    // Loop variables declared with `out var` inside For/ForEach are still in scope
    // for the C# compiler after `End`, but Waffle removes them from the runtime
    // environment when the block ends. Accessing them afterwards must throw a
    // user-friendly InvalidOperationException rather than a raw KeyNotFoundException.

#pragma warning disable WAF003
    [Test]
    public void For_LoopVariable_UsedAfterEnd_ThrowsFriendlyError()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = Render($$"""
                {{For(0, 3, out var i)}}
                {{i}}
                {{End}}
                {{i}}
                """);
        });

        Assert.That(ex!.Message, Does.Contain("For/ForEach"));
        Assert.That(ex.Message, Does.Contain("End"));
    }

    [Test]
    public void ForEach_LoopVariable_UsedAfterEnd_ThrowsFriendlyError()
    {
        var items = new[] { "a", "b", "c" };

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = Render($$"""
                {{ForEach(items, out var x)}}
                {{x}}
                {{End}}
                {{x}}
                """);
        });

        Assert.That(ex!.Message, Does.Contain("For/ForEach"));
        Assert.That(ex.Message, Does.Contain("End"));
    }

    [Test]
    public void ForEachNullable_LoopVariable_UsedAfterEnd_ThrowsFriendlyError()
    {
        IEnumerable<string?> items = ["a", null, "c"];

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = Render($$"""
                {{ForEachNullable(items, out var x)}}
                {{x}}
                {{End}}
                {{x}}
                """);
        });

        Assert.That(ex!.Message, Does.Contain("For/ForEach"));
        Assert.That(ex.Message, Does.Contain("End"));
    }
#pragma warning restore WAF003
}
