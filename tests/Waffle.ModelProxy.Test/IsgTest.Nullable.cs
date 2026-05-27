// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.ModelProxy.Test;

public partial class IsgTest
{
    // Models for nullable member tests
    [ModelProxy]
    public class NullableStringModel(string? s)
    {
        public string? NullableString { get; } = s;
        public string? NullableStringField = s;
    }

    [ModelProxy]
    public class NullableValueTypeModel(int? i, bool? b)
    {
        public int? NullableInt { get; } = i;
        public bool? NullableBool { get; } = b;
    }

    [ModelProxy]
    public class NullableModelRefModel(Model2? m)
    {
        public Model2? NullableModel2Prop { get; } = m;
    }

    // --- HasXxx accessor tests ---

    [Test]
    public void AsProxy_NullableString_NonNull_HasPropertyIsTrue()
    {
        var model = new NullableStringModel("Alice");
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableString)}}has{{End}}""");

        Assert.That(result, Is.EqualTo("has"));
    }

    [Test]
    public void AsProxy_NullableString_Null_HasPropertyIsFalse()
    {
        var model = new NullableStringModel(null);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableString)}}has{{End}}""");

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void AsProxy_NullableStringField_NonNull_HasPropertyIsTrue()
    {
        var model = new NullableStringModel("Bob");
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableStringField)}}has{{End}}""");

        Assert.That(result, Is.EqualTo("has"));
    }

    [Test]
    public void AsProxy_NullableStringField_Null_HasPropertyIsFalse()
    {
        var model = new NullableStringModel(null);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableStringField)}}has{{End}}""");

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void AsProxy_NullableInt_NonNull_HasPropertyIsTrue()
    {
        var model = new NullableValueTypeModel(42, null);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableInt)}}has{{End}}""");

        Assert.That(result, Is.EqualTo("has"));
    }

    [Test]
    public void AsProxy_NullableInt_Null_HasPropertyIsFalse()
    {
        var model = new NullableValueTypeModel(null, null);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableInt)}}has{{End}}""");

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void AsProxy_NullableBool_NonNull_HasPropertyIsTrue()
    {
        var model = new NullableValueTypeModel(null, true);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableBool)}}has{{End}}""");

        Assert.That(result, Is.EqualTo("has"));
    }

    [Test]
    public void AsProxy_NullableBool_Null_HasPropertyIsFalse()
    {
        var model = new NullableValueTypeModel(null, null);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableBool)}}has{{End}}""");

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void AsProxy_NullableModelRef_NonNull_HasPropertyIsTrue()
    {
        var model = new NullableModelRefModel(new Model2(99));
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableModel2Prop)}}has{{End}}""");

        Assert.That(result, Is.EqualTo("has"));
    }

    [Test]
    public void AsProxy_NullableModelRef_Null_HasPropertyIsFalse()
    {
        var model = new NullableModelRefModel(null);
        var proxy = model.AsProxy();

        var result = Render($$"""{{If(proxy.HasNullableModel2Prop)}}has{{End}}""");

        Assert.That(result, Is.EqualTo(""));
    }

    // --- Rendering tests for nullable values ---

    [Test]
    public void AsProxy_NullableString_NonNull_RendersValue()
    {
        var model = new NullableStringModel("Alice");
        IResolvableTo<NullableStringModel> src = new LiteralProxy<NullableStringModel>(model);

        var normalResult = Render($$"""value={{src.To(it => it.NullableString)}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""value={{proxy.NullableString}}""");

        Assert.That(proxyResult, Is.EqualTo(normalResult));
    }

    [Test]
    public void AsProxy_NullableString_Null_RendersEmpty()
    {
        var model = new NullableStringModel(null);
        IResolvableTo<NullableStringModel> src = new LiteralProxy<NullableStringModel>(model);

        var normalResult = Render($$"""value={{src.To(it => it.NullableString)}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""value={{proxy.NullableString}}""");

        Assert.That(proxyResult, Is.EqualTo(normalResult));
        Assert.That(proxyResult, Is.EqualTo("value="));
    }

    [Test]
    public void AsProxy_NullableInt_NonNull_RendersValue()
    {
        var model = new NullableValueTypeModel(42, null);
        IResolvableTo<NullableValueTypeModel> src = new LiteralProxy<NullableValueTypeModel>(model);

        var normalResult = Render($$"""value={{src.To(it => it.NullableInt)}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""value={{proxy.NullableInt}}""");

        Assert.That(proxyResult, Is.EqualTo(normalResult));
    }

    [Test]
    public void AsProxy_NullableInt_Null_RendersEmpty()
    {
        var model = new NullableValueTypeModel(null, null);
        IResolvableTo<NullableValueTypeModel> src = new LiteralProxy<NullableValueTypeModel>(model);

        var normalResult = Render($$"""value={{src.To(it => it.NullableInt)}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""value={{proxy.NullableInt}}""");

        Assert.That(proxyResult, Is.EqualTo(normalResult));
        Assert.That(proxyResult, Is.EqualTo("value="));
    }

    // --- In-loop nullable access ---

    private string RunNullableString_Normal(NullableStringModel[] models)
    {
        return Render($$"""
            {{ForEach(models, out var m, out var i)}}
            [{{i}}]={{m.To(it => it.NullableString)}}
            {{End}}
            """);
    }

    private string RunNullableString_Proxy(NullableStringModel[] models)
    {
        return Render($$"""
            {{ForEach(models.AsProxy(), out var m, out var i)}}
            [{{i}}]={{m.NullableString}}
            {{End}}
            """);
    }

    [Test]
    public void AsProxy_NullableString_InForEach_OutputMatches()
    {
        var models = new[]
            { new NullableStringModel("Alice"), new NullableStringModel(null), new NullableStringModel("Charlie") };

        Assert.That(RunNullableString_Proxy(models), Is.EqualTo(RunNullableString_Normal(models)));
    }
}
