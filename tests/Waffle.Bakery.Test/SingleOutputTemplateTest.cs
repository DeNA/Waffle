// (c) DeNA Co., Ltd.

namespace Waffle.Bakery.Test;

public class SingleOutputTemplateTest
{
    private class TemplateA : SingleOutputTemplate<DefaultBakeryContext>
    {
        protected override string OutputId => "Hoge";

        protected override void ProcessImpl(DefaultBakeryContext ctx)
        {
            ctx.Append("a");
            ctx.Append("b");
            ctx.Append("c");
        }
    }

    private class TemplateB : SingleOutputTemplate<DefaultBakeryContext>
    {
        protected override string OutputId => "Hoge";

        protected override void OnPreProcess(DefaultBakeryContext ctx)
        {
            ctx.Append("hoge");
        }

        protected override void ProcessImpl(DefaultBakeryContext ctx)
        {
            ctx.Append("a");
            ctx.Append("b");
            ctx.Append("c");
        }

        protected override void OnPostProcess(DefaultBakeryContext ctx)
        {
            ctx.Append("fuga");
        }
    }

    [Test]
    public void Process_DefaultSetup_OutputIsAsExpected()
    {
        var ctx = new DefaultBakeryContext();
        SingleOutputTemplate<DefaultBakeryContext> sut = new TemplateA();
        sut.Process(ctx);
        var result = ctx.GetResults();
        Assert.That(result, Has.One.Items);
        Assert.That(result["Hoge"], Is.EqualTo("abc"));
    }

    [Test]
    public void Process_AfterCompletion_ContextIsNotOpen()
    {
        var ctx = new DefaultBakeryContext();
        SingleOutputTemplate<DefaultBakeryContext> sut = new TemplateA();
        sut.Process(ctx);
        Assert.Throws<InvalidOperationException>(() => ctx.Append("piyo"));
    }

    [Test]
    public void Process_WithPreAndPostProcess_EachHookIsExecutedProperly()
    {
        var ctx = new DefaultBakeryContext();
        SingleOutputTemplate<DefaultBakeryContext> sut = new TemplateB();
        sut.Process(ctx);
        var result = ctx.GetResults();
        Assert.That(result, Has.One.Items);
        Assert.That(result["Hoge"], Is.EqualTo("hogeabcfuga"));
    }

    [Test]
    public void Process_WithPreAndPostProcess_AfterCompletion_ContextIsNotOpen()
    {
        var ctx = new DefaultBakeryContext();
        SingleOutputTemplate<DefaultBakeryContext> sut = new TemplateB();
        sut.Process(ctx);
        Assert.Throws<InvalidOperationException>(() => ctx.Append("piyo"));
    }
}
