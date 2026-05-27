// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.Core.Test;

public class ListProxyTest
{
    private static readonly Dictionary<int, EnvValue> s_emptyEnv = new();

    [Test]
    public void Count()
    {
        var source = new[] { 10, 0, -5 };
        var actual = new ListProxy<int>(new LiteralProxy<IReadOnlyList<int>>(source)).Count
            .Resolve(s_emptyEnv);
        var expected = source.Length;
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(0, 10)]
    [TestCase(1, 0)]
    [TestCase(2, -5)]
    public void Indexer_Value(int index, int expected)
    {
        var source = new[] { 10, 0, -5 };
        var actual = new ListProxy<int>(new LiteralProxy<IReadOnlyList<int>>(source))[index]
            .Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(0, 10)]
    [TestCase(1, 0)]
    [TestCase(2, -5)]
    public void Indexer_IResolvable(int index, int expected)
    {
        var source = new[] { 10, 0, -5 };
        var actual = new ListProxy<int>(new LiteralProxy<IReadOnlyList<int>>(source))[new LiteralProxy<int>(index)]
            .Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }
}
