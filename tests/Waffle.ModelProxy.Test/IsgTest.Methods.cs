// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.ModelProxy.Test;

public partial class IsgTest
{
    // Model for method proxy tests
    [ModelProxy]
    public class MethodModel(string label, int score, bool enabled, string? optionalLabel = null)
    {
        public string GetLabel() => label;
        public int GetScore() => score;
        public bool IsEnabled() => enabled;
        public string? GetOptionalLabel() => optionalLabel;

        // Methods with parameters: now exposed via parameterized proxy accessors (Phase 4)
        public string GetLabelWithPrefix(string prefix) => prefix + label;
    }

    // --- Basic method proxy tests ---

    [Test]
    public void AsProxy_Method_String_RendersCorrectly()
    {
        var model = new MethodModel("Hello", 42, true);
        IResolvableTo<MethodModel> src = new LiteralProxy<MethodModel>(model);

        var normalResult = Render($$"""label={{src.To(it => it.GetLabel())}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""label={{proxy.GetLabel()}}""");

        Assert.That(proxyResult, Is.EqualTo(normalResult));
        Assert.That(proxyResult, Is.EqualTo("label=Hello"));
    }

    [Test]
    public void AsProxy_Method_Int_RendersCorrectly()
    {
        var model = new MethodModel("Test", 99, false);
        IResolvableTo<MethodModel> src = new LiteralProxy<MethodModel>(model);

        var normalResult = Render($$"""score={{src.To(it => it.GetScore())}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""score={{proxy.GetScore()}}""");

        Assert.That(proxyResult, Is.EqualTo(normalResult));
        Assert.That(proxyResult, Is.EqualTo("score=99"));
    }

    [Test]
    public void AsProxy_Method_Bool_RendersCorrectly()
    {
        var model = new MethodModel("X", 0, true);
        IResolvableTo<MethodModel> src = new LiteralProxy<MethodModel>(model);

        var normalResult = Render($$"""{{If(src.To(it => it.IsEnabled()))}}yes{{End}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""{{If(proxy.IsEnabled())}}yes{{End}}""");

        Assert.That(proxyResult, Is.EqualTo(normalResult));
        Assert.That(proxyResult, Is.EqualTo("yes"));
    }

    // --- Nullable return type method tests ---

    [Test]
    public void AsProxy_Method_NullableReturn_NonNull_HasMethodIsTrue()
    {
        var model = new MethodModel("X", 0, false, "available");
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasGetOptionalLabel)}}has{{End}}""");

        Assert.That(result, Is.EqualTo("has"));
    }

    [Test]
    public void AsProxy_Method_NullableReturn_Null_HasMethodIsFalse()
    {
        var model = new MethodModel("X", 0, false, null);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasGetOptionalLabel)}}has{{End}}""");

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void AsProxy_Method_NullableReturn_NonNull_RendersValue()
    {
        var model = new MethodModel("X", 0, false, "optional");
        IResolvableTo<MethodModel> src = new LiteralProxy<MethodModel>(model);

        var normalResult = Render($$"""v={{src.To(it => it.GetOptionalLabel())}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""v={{proxy.GetOptionalLabel()}}""");

        Assert.That(proxyResult, Is.EqualTo(normalResult));
    }

    [Test]
    public void AsProxy_Method_NullableReturn_Null_RendersEmpty()
    {
        var model = new MethodModel("X", 0, false, null);
        IResolvableTo<MethodModel> src = new LiteralProxy<MethodModel>(model);

        var normalResult = Render($$"""v={{src.To(it => it.GetOptionalLabel())}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""v={{proxy.GetOptionalLabel()}}""");

        Assert.That(proxyResult, Is.EqualTo(normalResult));
        Assert.That(proxyResult, Is.EqualTo("v="));
    }

    // --- In-loop method access ---

    private static string RunMethodNormal(MethodModel[] models)
    {
        return Render($$"""
            {{ForEach(models, out var m, out var i)}}
            [{{i}}]={{m.To(it => it.GetLabel())}}({{m.To(it => it.GetScore())}})
            {{End}}
            """);
    }

    private static string RunMethodProxy(MethodModel[] models)
    {
        return Render($$"""
            {{ForEach(models.AsProxy(), out var m, out var i)}}
            [{{i}}]={{m.GetLabel()}}({{m.GetScore()}})
            {{End}}
            """);
    }

    [Test]
    public void AsProxy_Method_InForEach_OutputMatches()
    {
        var models = new[]
        {
            new MethodModel("Alice", 10, true),
            new MethodModel("Bob", 20, false),
            new MethodModel("Charlie", 30, true),
        };

        Assert.That(RunMethodProxy(models), Is.EqualTo(RunMethodNormal(models)));
    }

    // --- ToString() proxy ---

    [Test]
    public void AsProxy_ToString_RendersModelToString()
    {
        var model = new MethodModel("Hello", 42, true);
        IResolvableTo<MethodModel> src = new LiteralProxy<MethodModel>(model);

        var normal = Render($$"""r={{src.To(it => it.ToString())}}""");

        var proxy = model.AsProxy();
        var result = Render($$"""r={{proxy.ToString()}}""");

        Assert.That(result, Is.EqualTo(normal));
    }

    [Test]
    public void AsProxy_ToString_WithOverride_RendersOverriddenValue()
    {
        // MethodModel does not override ToString(), so object.ToString() is used
        var model = new MethodModel("Test", 0, false);
        var proxy = model.AsProxy();

        // proxy.ToString() must return StringProxy, not the proxy's own object.ToString()
        Assert.That(proxy.ToString(), Is.InstanceOf<StringProxy>());
    }
}
