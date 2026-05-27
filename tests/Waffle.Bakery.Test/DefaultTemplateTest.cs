// (c) DeNA Co., Ltd.

namespace Waffle.Bakery.Test;

public class DefaultTemplateTest
{
    private class TemplateA : DefaultTemplate
    {
        protected override string OutputId => "Hoge";

        protected override void ProcessImpl(DefaultBakeryContext ctx)
        {
            ctx.Append("a");
            ctx.Append("b");
            ctx.Append("c");
        }
    }


    [Test]
    public void Process_DefaultSetup_OutputIsAsExpected()
    {
        var ctx = new DefaultBakeryContext();
        DefaultTemplate sut = new TemplateA();
        sut.Process(ctx);
        var result = ctx.GetResults();
        Assert.That(result, Has.One.Items);
        Assert.That(result["Hoge"], Is.EqualTo("abc"));
    }

    [Test]
    public void Process_AfterCompletion_ContextIsNotOpen()
    {
        var ctx = new DefaultBakeryContext();
        DefaultTemplate sut = new TemplateA();
        sut.Process(ctx);
        Assert.Throws<InvalidOperationException>(() => ctx.Append("piyo"));
    }
}
