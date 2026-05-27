// (c) DeNA Co., Ltd.

using System.Collections.Generic;
using Waffle.Interpreter;

namespace Waffle.ModelProxy.Test;

public partial class IsgTest
{
    // Models for basic type tests
    [ModelProxy]
    public class BasicTypeModel
    {
        public string NameProperty { get; }
        public bool IsActiveProperty { get; }
        public string NameField;
        public bool IsActiveField;

        public BasicTypeModel(string name, bool isActive)
        {
            NameProperty = name;
            IsActiveProperty = isActive;
            NameField = name;
            IsActiveField = isActive;
        }
    }

    private static readonly BasicTypeModel[] s_basicTypeModels =
    [
        new("Alice", true),
        new("Bob", false),
        new("Charlie", true),
    ];

    private string RunBasicTypeNormal_Property()
    {
        return Render($$"""
            {{ForEach(s_basicTypeModels, out var m, out var i)}}
            [{{i}}] Name={{m.To(it => it.NameProperty)}}, Active={{m.To(it => it.IsActiveProperty)}}
            {{End}}
            """);
    }

    private string RunBasicTypeProxy_Property()
    {
        var proxy = s_basicTypeModels.AsProxy();
        return Render($$"""
            {{ForEach(proxy, out var m, out var i)}}
            [{{i}}] Name={{m.NameProperty}}, Active={{m.IsActiveProperty}}
            {{End}}
            """);
    }

    private string RunBasicTypeNormal_Field()
    {
        return Render($$"""
            {{ForEach(s_basicTypeModels, out var m, out var i)}}
            [{{i}}] Name={{m.To(it => it.NameField)}}, Active={{m.To(it => it.IsActiveField)}}
            {{End}}
            """);
    }

    private string RunBasicTypeProxy_Field()
    {
        var proxy = s_basicTypeModels.AsProxy();
        return Render($$"""
            {{ForEach(proxy, out var m, out var i)}}
            [{{i}}] Name={{m.NameField}}, Active={{m.IsActiveField}}
            {{End}}
            """);
    }

    [Test]
    public void AsProxy_BasicType_StringAndBoolProperties_OutputMatches()
    {
        var normal = RunBasicTypeNormal_Property();
        var proxy = RunBasicTypeProxy_Property();

        Assert.That(proxy, Is.EqualTo(normal));
    }

    [Test]
    public void AsProxy_BasicType_StringAndBoolFields_OutputMatches()
    {
        var normal = RunBasicTypeNormal_Field();
        var proxy = RunBasicTypeProxy_Field();

        Assert.That(proxy, Is.EqualTo(normal));
    }

    [Test]
    public void AsProxy_BasicType_DirectScalarAccess_OutputMatches()
    {
        var model = new BasicTypeModel("Alice", true);
        IResolvableTo<BasicTypeModel> src = new LiteralProxy<BasicTypeModel>(model);

        var normalResult =
            Render($$"""Name={{src.To(it => it.NameProperty)}}, Active={{src.To(it => it.IsActiveProperty)}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""Name={{proxy.NameProperty}}, Active={{proxy.IsActiveProperty}}""");

        Assert.That(proxyResult, Is.EqualTo(normalResult));
    }
}
