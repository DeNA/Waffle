// (c) DeNA Co., Ltd.

namespace Waffle.Bakery.Test;

public class DefaultBakeryTest
{
    private class TemplateAttribute(int value = 0) : Attribute
    {
        public int Value { get; } = value;
    }

    [Template(1)]
    private class TemplateA : ITemplate<DefaultBakeryContext>
    {
        public void Process(DefaultBakeryContext ctx)
        {
            ctx.Open("A");
            ctx.Append("a");
            ctx.Close();
        }
    }

    [Template(2)]
    private class TemplateB : ITemplate<DefaultBakeryContext>
    {
        public void Process(DefaultBakeryContext ctx)
        {
            ctx.Open("B");
            ctx.Append("b");
            ctx.Close();
        }
    }

    [Template(3)]
    private class TemplateC : ITemplate<DefaultBakeryContext>
    {
        public void Process(DefaultBakeryContext ctx)
        {
            ctx.Open("C");
            ctx.Append("c");
            ctx.Close();
        }
    }

    [Test]
    public void Run_NoTemplatesRegistered_ResultIsEmpty()
    {
        var result = new DefaultBakery()
            .Initialize(new DefaultBakeryContext())
            .Run()
            .GetResults();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Register_SingleTemplate_ResolvedResultIsObtained()
    {
        var result = new DefaultBakery()
            .Initialize(new DefaultBakeryContext())
            .Register(new TemplateA())
            .Run()
            .GetResults();
        Assert.That(result, Has.One.Items);
        Assert.That(result["A"], Is.EqualTo("a"));
    }

    [Test]
    public void Register_MultipleTemplates_ResolvedResultIsObtained()
    {
        var result = new DefaultBakery()
            .Initialize(new DefaultBakeryContext())
            .Register(new TemplateB())
            .Register(new TemplateC())
            .Run()
            .GetResults();
        Assert.That(result, Has.Exactly(2).Items);
        Assert.That(result["B"], Is.EqualTo("b"));
        Assert.That(result["C"], Is.EqualTo("c"));
    }

    [Test]
    public void RegisterT_SingleTemplate_ResolvedResultIsObtained()
    {
        var result = new DefaultBakery()
            .Initialize(new DefaultBakeryContext())
            .Register<TemplateA>()
            .Run()
            .GetResults();
        Assert.That(result, Has.One.Items);
        Assert.That(result["A"], Is.EqualTo("a"));
    }

    [Test]
    public void RegisterT_MultipleTemplates_ResolvedResultIsObtained()
    {
        var result = new DefaultBakery()
            .Initialize(new DefaultBakeryContext())
            .Register<TemplateB>()
            .Register<TemplateC>()
            .Run()
            .GetResults();
        Assert.That(result, Has.Exactly(2).Items);
        Assert.That(result["B"], Is.EqualTo("b"));
        Assert.That(result["C"], Is.EqualTo("c"));
    }

    [Test]
    public void RegisterAllByAttribute_NoFilter_ResolvedResultIsObtained()
    {
        var result = new DefaultBakery()
            .Initialize(new DefaultBakeryContext())
            .RegisterAllByAttribute<TemplateAttribute>(typeof(DefaultBakeryTest).Assembly)
            .Run()
            .GetResults();
        Assert.That(result, Has.Exactly(3).Items);
        Assert.That(result["A"], Is.EqualTo("a"));
        Assert.That(result["B"], Is.EqualTo("b"));
        Assert.That(result["C"], Is.EqualTo("c"));
    }

    [Test]
    public void RegisterAllByAttribute_WithFilter_ResolvedResultIsObtained()
    {
        var result = new DefaultBakery()
            .Initialize(new DefaultBakeryContext())
            .RegisterAllByAttribute<TemplateAttribute>(typeof(DefaultBakeryTest).Assembly, attr => attr.Value != 2)
            .Run()
            .GetResults();
        Assert.That(result, Has.Exactly(2).Items);
        Assert.That(result["A"], Is.EqualTo("a"));
        Assert.That(result["C"], Is.EqualTo("c"));
    }
}
