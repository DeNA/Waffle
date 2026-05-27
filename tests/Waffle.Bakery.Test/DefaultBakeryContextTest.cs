// (c) DeNA Co., Ltd.

namespace Waffle.Bakery.Test;

public class DefaultBakeryContextTest
{
    [Test]
    public void Append_SingleOutputIdAppendedOnce_ResultIsAsExpected()
    {
        var ctx = new DefaultBakeryContext();
        ctx.Open("hoge");
        ctx.Append("fuga");
        ctx.Close();

        var result = ctx.GetResults();
        Assert.That(result, Has.One.Items);
        Assert.That(result["hoge"], Is.EqualTo("fuga"));
    }

    [Test]
    public void Append_SingleOutputIdAppendedMultipleTimes_ContentIsAppendedInOrder()
    {
        var ctx = new DefaultBakeryContext();
        ctx.Open("hoge");
        ctx.Append("fuga");
        ctx.Append("piyo");
        ctx.Append("nyan");
        ctx.Close();

        var result = ctx.GetResults();
        Assert.That(result, Has.One.Items);
        Assert.That(result["hoge"], Is.EqualTo("fugapiyonyan"));
    }

    [Test]
    public void Append_MultipleOutputIdsEachOpenedOnce_ContentIsAddedToCorrectId()
    {
        var ctx = new DefaultBakeryContext();
        ctx.Open("1");
        ctx.Append("fuga");
        ctx.Close();
        ctx.Open("2");
        ctx.Append("piyo");
        ctx.Close();
        ctx.Open("3");
        ctx.Append("nyan");
        ctx.Close();

        var result = ctx.GetResults();
        Assert.That(result, Has.Exactly(3).Items);
        Assert.That(result["1"], Is.EqualTo("fuga"));
        Assert.That(result["2"], Is.EqualTo("piyo"));
        Assert.That(result["3"], Is.EqualTo("nyan"));
    }

    [Test]
    public void Append_MultipleOutputIdsEachOpenedMultipleTimes_ContentIsAddedToCorrectId()
    {
        var ctx = new DefaultBakeryContext();
        ctx.Open("1");
        ctx.Append("fuga");
        ctx.Close();
        ctx.Open("2");
        ctx.Append("piyo");
        ctx.Close();
        ctx.Open("3");
        ctx.Append("nyan");
        ctx.Close();
        ctx.Open("1");
        ctx.Append("hoge");
        ctx.Close();
        ctx.Open("2");
        ctx.Append("hoge");
        ctx.Close();
        ctx.Open("3");
        ctx.Append("hoge");
        ctx.Close();

        var result = ctx.GetResults();
        Assert.That(result, Has.Exactly(3).Items);
        Assert.That(result["1"], Is.EqualTo("fugahoge"));
        Assert.That(result["2"], Is.EqualTo("piyohoge"));
        Assert.That(result["3"], Is.EqualTo("nyanhoge"));
    }

    [Test]
    public void Clear_HasContent_ResultBecomesEmpty()
    {
        var ctx = new DefaultBakeryContext();
        ctx.Open("1");
        ctx.Append("fuga");
        ctx.Close();
        ctx.Open("2");
        ctx.Append("piyo");
        ctx.Close();
        ctx.Open("3");
        ctx.Append("nyan");
        ctx.Close();
        ctx.Open("1");
        ctx.Append("hoge");
        ctx.Close();
        ctx.Open("2");
        ctx.Append("hoge");
        ctx.Close();
        ctx.Open("3");
        ctx.Append("hoge");
        ctx.Close();
        ctx.Clear();

        var result = ctx.GetResults();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Clear_WhileOpen_ResultBecomesEmpty()
    {
        var ctx = new DefaultBakeryContext();
        ctx.Open("1");
        ctx.Append("fuga");
        ctx.Clear();
        var result = ctx.GetResults();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Clear_WhileOpen_ContextBecomesNotOpen()
    {
        var ctx = new DefaultBakeryContext();
        ctx.Open("1");
        ctx.Append("fuga");
        ctx.Clear();
        Assert.Throws<InvalidOperationException>(() =>
        {
            ctx.Append("piyo");
        });
    }

    [Test]
    public void Clear_AlreadyEmpty_NothingHappens()
    {
        var ctx = new DefaultBakeryContext();
        ctx.Clear();
        var result = ctx.GetResults();
        Assert.That(result, Is.Empty);
    }
}
