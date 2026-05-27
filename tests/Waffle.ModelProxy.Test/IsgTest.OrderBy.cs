// (c) DeNA Co., Ltd.

namespace Waffle.ModelProxy.Test;

public partial class IsgTest
{
    [Test]
    public void OrderBy_TIterator_SortsAscendingAndPreservesProxy()
    {
        var model = new Model1([new Model2(3), new Model2(1), new Model2(4)], []);
        var proxy = model.AsProxy();
        var result = Render($$"""
            {{ForEach(proxy.Children.OrderBy(m => m.Value), out var child, out var i)}}
            [{{i}}]:{{child.Value}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[0]:1\n[1]:3\n[2]:4\n"));
    }

    [Test]
    public void OrderByDescending_TIterator_SortsDescendingAndPreservesProxy()
    {
        var model = new Model1([new Model2(3), new Model2(1), new Model2(4)], []);
        var proxy = model.AsProxy();
        var result = Render($$"""
            {{ForEach(proxy.Children.OrderByDescending(m => m.Value), out var child, out var i)}}
            [{{i}}]:{{child.Value}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[0]:4\n[1]:3\n[2]:1\n"));
    }

    [Test]
    public void OrderBy_TIterator_EmptySource_ProducesNoOutput()
    {
        var model = new Model1([], []);
        var proxy = model.AsProxy();
        var result = Render($$"""
            {{ForEach(proxy.Children.OrderBy(m => m.Value), out var child)}}
            {{child.Value}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void OrderByDescending_TIterator_EmptySource_ProducesNoOutput()
    {
        var model = new Model1([], []);
        var proxy = model.AsProxy();
        var result = Render($$"""
            {{ForEach(proxy.Children.OrderByDescending(m => m.Value), out var child)}}
            {{child.Value}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void OrderBy_TIterator_SingleElement_ProducesSingleOutput()
    {
        var model = new Model1([new Model2(7)], []);
        var proxy = model.AsProxy();
        var result = Render($$"""
            {{ForEach(proxy.Children.OrderBy(m => m.Value), out var child)}}
            {{child.Value}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("7\n"));
    }

    [Test]
    public void OrderByDescending_TIterator_SingleElement_ProducesSingleOutput()
    {
        var model = new Model1([new Model2(7)], []);
        var proxy = model.AsProxy();
        var result = Render($$"""
            {{ForEach(proxy.Children.OrderByDescending(m => m.Value), out var child)}}
            {{child.Value}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("7\n"));
    }

    [Test]
    public void OrderBy_TIterator_ByCustomKey_SortsCorrectly()
    {
        // Sort by negative value → ascending by negative = descending by actual value
        var model = new Model1([new Model2(1), new Model2(2), new Model2(3)], []);
        var proxy = model.AsProxy();
        var result = Render($$"""
            {{ForEach(proxy.Children.OrderBy(m => -m.Value), out var child, out var i)}}
            [{{i}}]:{{child.Value}}
            {{End}}
            """);
        Assert.That(result, Is.EqualTo("[0]:3\n[1]:2\n[2]:1\n"));
    }
}
