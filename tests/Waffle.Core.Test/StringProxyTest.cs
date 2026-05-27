// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.Core.Test;

public class StringProxyTest
{
    [TestCase("", "")]
    [TestCase("", "abc")]
    [TestCase("abc", "")]
    [TestCase("hello", " world")]
    [TestCase("foo", "bar")]
    [TestCase("foo", "foo")]
    public void Add(string left, string right)
    {
        var l = new StringProxy(new LiteralProxy<string>(left));
        var r = new StringProxy(new LiteralProxy<string>(right));
        var expected = left + right;
        Assert.That((l + r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l + right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left + r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase("", "")]
    [TestCase("", "abc")]
    [TestCase("abc", "")]
    [TestCase("hello", "hello")]
    [TestCase("foo", "bar")]
    public void Eq(string left, string right)
    {
        var l = new StringProxy(new LiteralProxy<string>(left));
        var r = new StringProxy(new LiteralProxy<string>(right));
        var expected = left == right;
        Assert.That((l == r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l == right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left == r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase("", "")]
    [TestCase("", "abc")]
    [TestCase("abc", "")]
    [TestCase("hello", "hello")]
    [TestCase("foo", "bar")]
    public void Ne(string left, string right)
    {
        var l = new StringProxy(new LiteralProxy<string>(left));
        var r = new StringProxy(new LiteralProxy<string>(right));
        var expected = left != right;
        Assert.That((l != r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l != right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left != r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }
}
