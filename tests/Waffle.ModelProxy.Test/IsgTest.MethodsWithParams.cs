// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.ModelProxy.Test;

public partial class IsgTest
{
    // Model for parameterized method proxy tests
    [ModelProxy]
    public class ParamMethodModel(string label, string? opt = null)
    {
        public string WithPrefix(string prefix) => prefix + label;
        public string Format(string a, string b) => a + label + b;
        public string? TryGet(int id) => id == 0 ? opt : null;
        public bool IsMatch(string pattern) => label.Contains(pattern);
        public string ThreeParams(string a, string b, int c) => a + b + c;
        public string FourParams(string a, string b, int c, bool d) => a + b + c + d;

        // Should NOT be exposed: more than 4 parameters
        public string TooMany(string a, string b, int c, bool d, float e) => a + b + c + d + e;
    }

    // --- 1-parameter method tests ---

    [Test]
    public void AsProxy_ParamMethod_1Param_IResolvable_RendersCorrectly()
    {
        var model = new ParamMethodModel("World");
        IResolvableTo<ParamMethodModel> src = new LiteralProxy<ParamMethodModel>(model);

        var normal = Render($$"""r={{src.To(it => it.WithPrefix("Hello-"))}}""");

        var proxy = model.AsProxy();
        IResolvableTo<string> prefixToken = new LiteralProxy<string>("Hello-");
        var result = Render($$"""r={{proxy.WithPrefix(prefixToken)}}""");

        Assert.That(result, Is.EqualTo(normal));
        Assert.That(result, Is.EqualTo("r=Hello-World"));
    }

    [Test]
    public void AsProxy_ParamMethod_1Param_Direct_RendersCorrectly()
    {
        var model = new ParamMethodModel("World");
        var proxy = model.AsProxy();

        var result = Render($$"""r={{proxy.WithPrefix("Hi-")}}""");

        Assert.That(result, Is.EqualTo("r=Hi-World"));
    }

    // --- 2-parameter method tests ---

    [Test]
    public void AsProxy_ParamMethod_2Params_IResolvable_RendersCorrectly()
    {
        var model = new ParamMethodModel("X");
        IResolvableTo<ParamMethodModel> src = new LiteralProxy<ParamMethodModel>(model);

        var normal = Render($$"""r={{src.To(it => it.Format("[", "]"))}}""");

        var proxy = model.AsProxy();
        IResolvableTo<string> a = new LiteralProxy<string>("[");
        IResolvableTo<string> b = new LiteralProxy<string>("]");
        var result = Render($$"""r={{proxy.Format(a, b)}}""");

        Assert.That(result, Is.EqualTo(normal));
        Assert.That(result, Is.EqualTo("r=[X]"));
    }

    [Test]
    public void AsProxy_ParamMethod_2Params_Direct_RendersCorrectly()
    {
        var model = new ParamMethodModel("X");
        var proxy = model.AsProxy();

        var result = Render($$"""r={{proxy.Format("(", ")")}}""");

        Assert.That(result, Is.EqualTo("r=(X)"));
    }

    // --- 3-parameter method tests ---

    [Test]
    public void AsProxy_ParamMethod_3Params_Direct_RendersCorrectly()
    {
        var model = new ParamMethodModel("");
        var proxy = model.AsProxy();

        var result = Render($$"""r={{proxy.ThreeParams("a", "b", 3)}}""");

        Assert.That(result, Is.EqualTo("r=ab3"));
    }

    // --- 4-parameter method tests ---

    [Test]
    public void AsProxy_ParamMethod_4Params_Direct_RendersCorrectly()
    {
        var model = new ParamMethodModel("");
        var proxy = model.AsProxy();

        var result = Render($$"""r={{proxy.FourParams("a", "b", 3, true)}}""");

        Assert.That(result, Is.EqualTo("r=ab3True"));
    }

    // --- >4 parameters: not exposed ---

    [Test]
    public void AsProxy_ParamMethod_TooManyParams_NotExposedOnProxy()
    {
        var proxy = new ParamMethodModel("").AsProxy();
        var type = proxy.GetType();

        // The proxy should NOT have a TooMany method
        Assert.That(type.GetMethod("TooMany"), Is.Null);
    }

    // --- Nullable return type + HasXxx ---

    [Test]
    public void AsProxy_ParamMethod_NullableReturn_HasMethod_NonNull_IsTrue()
    {
        var model = new ParamMethodModel("X", "found");
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasTryGet(new LiteralProxy<int>(0)))}}yes{{End}}""");

        Assert.That(result, Is.EqualTo("yes"));
    }

    [Test]
    public void AsProxy_ParamMethod_NullableReturn_HasMethod_Direct_NonNull_IsTrue()
    {
        var model = new ParamMethodModel("X", "found");
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasTryGet(0))}}yes{{End}}""");

        Assert.That(result, Is.EqualTo("yes"));
    }

    [Test]
    public void AsProxy_ParamMethod_NullableReturn_HasMethod_Direct_Null_IsFalse()
    {
        var model = new ParamMethodModel("X", null);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasTryGet(0))}}yes{{End}}""");

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void AsProxy_ParamMethod_NullableReturn_Direct_RendersValue()
    {
        var model = new ParamMethodModel("X", "result");
        IResolvableTo<ParamMethodModel> src = new LiteralProxy<ParamMethodModel>(model);

        var normal = Render($$"""v={{src.To(it => it.TryGet(0))}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""v={{proxy.TryGet(0)}}""");

        Assert.That(proxyResult, Is.EqualTo(normal));
    }

    [Test]
    public void AsProxy_ParamMethod_NullableReturn_Direct_RendersEmpty_WhenNull()
    {
        var model = new ParamMethodModel("X", null);
        var proxy = model.AsProxy();

        var result = Render($$"""v={{proxy.TryGet(0)}}""");

        Assert.That(result, Is.EqualTo("v="));
    }

    // --- bool return type with parameter ---

    [Test]
    public void AsProxy_ParamMethod_BoolReturn_Direct_RendersCorrectly()
    {
        var model = new ParamMethodModel("Hello");
        var proxy = model.AsProxy();

        var trueResult = Render($$"""{{If(proxy.IsMatch("Hell"))}}yes{{End}}""");
        var falseResult = Render($$"""{{If(proxy.IsMatch("xyz"))}}yes{{End}}""");

        Assert.That(trueResult, Is.EqualTo("yes"));
        Assert.That(falseResult, Is.EqualTo(""));
    }

    // --- ForEach loop with parameterized method ---

    [Test]
    public void AsProxy_ParamMethod_InForEach_OutputMatches()
    {
        var models = new[]
        {
            new ParamMethodModel("Alice"),
            new ParamMethodModel("Bob"),
            new ParamMethodModel("Charlie"),
        };

        var normal = Render($$"""
            {{ForEach(models, out var m, out var i)}}
            [{{i}}]={{m.To(it => it.WithPrefix(">"))}}
            [{{i}}]={{m.With(i, (it, n) => it.WithPrefix(n.ToString()))}}
            {{End}}
            """);

        var proxyResult = Render($$"""
            {{ForEach(models.AsProxy(), out var pm, out var pi)}}
            [{{pi}}]={{pm.WithPrefix(">")}}
            [{{pi}}]={{pm.WithPrefix(pi.ToString())}}
            {{End}}
            """);

        Assert.That(proxyResult, Is.EqualTo(normal));
    }
}
