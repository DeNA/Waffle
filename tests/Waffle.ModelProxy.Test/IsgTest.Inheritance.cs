// (c) DeNA Co., Ltd.

using System.Collections.Generic;
using Waffle.Interpreter;

namespace Waffle.ModelProxy.Test;

public partial class IsgTest
{
    // Models for inheritance tests
    public class InheritanceBaseModel
    {
        public string BaseString { get; }
        public int BaseInt { get; }

        protected InheritanceBaseModel(string baseStr, int baseInt)
        {
            BaseString = baseStr;
            BaseInt = baseInt;
        }
    }

    [ModelProxy]
    public class InheritanceDerivedModel(string baseStr, int baseInt, string derivedStr)
        : InheritanceBaseModel(baseStr, baseInt)
    {
        public string DerivedString { get; } = derivedStr;
    }

    // Models for `new`-hiding tests
    public class InheritanceNewHidingBase
    {
        public string Value { get; } = "base";
        public int Number { get; } = 0;
    }

    [ModelProxy]
    public class InheritanceNewHidingDerived : InheritanceNewHidingBase
    {
        public new string Value { get; } = "derived";
        // Number is inherited as-is from base
    }

    // Models for `override` tests
    public class InheritanceOverrideBase
    {
        public virtual string Value { get; } = "base";
        public int Number { get; } = 0;
    }

    [ModelProxy]
    public class InheritanceOverrideDerived : InheritanceOverrideBase
    {
        public override string Value { get; } = "derived";
    }

    private static string RunInheritanceNormal_BaseMembers(InheritanceDerivedModel[] models)
    {
        return Render($$"""
            {{ForEach(models, out var m, out var i)}}
            [{{i}}] base={{m.To(it => it.BaseString)}}/{{m.To(it => it.BaseInt)}}, derived={{m.To(it => it.DerivedString)}}
            {{End}}
            """);
    }

    private static string RunInheritanceProxy_BaseMembers(InheritanceDerivedModel[] models)
    {
        return Render($$"""
            {{ForEach(models.AsProxy(), out var m, out var i)}}
            [{{i}}] base={{m.BaseString}}/{{m.BaseInt}}, derived={{m.DerivedString}}
            {{End}}
            """);
    }

    [Test]
    public void AsProxy_Inheritance_BaseMembersAccessible_OutputMatches()
    {
        var models = new[]
        {
            new InheritanceDerivedModel("base1", 10, "derived1"),
            new InheritanceDerivedModel("base2", 20, "derived2"),
        };

        Assert.That(RunInheritanceProxy_BaseMembers(models), Is.EqualTo(RunInheritanceNormal_BaseMembers(models)));
    }

    [Test]
    public void AsProxy_Inheritance_DirectScalarAccess_BaseMembersRendered()
    {
        var model = new InheritanceDerivedModel("hello", 42, "world");
        IResolvableTo<InheritanceDerivedModel> src = new LiteralProxy<InheritanceDerivedModel>(model);

        var normalResult =
            Render(
                $$"""base={{src.To(it => it.BaseString)}}/{{src.To(it => it.BaseInt)}}, derived={{src.To(it => it.DerivedString)}}""");

        var proxy = model.AsProxy();
        var proxyResult = Render($$"""base={{proxy.BaseString}}/{{proxy.BaseInt}}, derived={{proxy.DerivedString}}""");

        Assert.That(proxyResult, Is.EqualTo(normalResult));
    }

    [Test]
    public void AsProxy_NewHiding_OnlyDerivedVersionExposed()
    {
        var model = new InheritanceNewHidingDerived();
        var proxy = model.AsProxy();

        // Value should reflect the derived (hiding) property, not the base one.
        var result = Render($$"""value={{proxy.Value}}, number={{proxy.Number}}""");
        Assert.That(result, Is.EqualTo("value=derived, number=0"));
    }

    [Test]
    public void AsProxy_Override_OverridingImplementationCalled()
    {
        var model = new InheritanceOverrideDerived();
        var proxy = model.AsProxy();

        // Value should reflect the overriding property via virtual dispatch.
        var result = Render($$"""value={{proxy.Value}}, number={{proxy.Number}}""");
        Assert.That(result, Is.EqualTo("value=derived, number=0"));
    }
}
