// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.Core.Test;

public class IResolvableToTest
{
    private static readonly Dictionary<int, EnvValue> s_emptyEnv = new();

    [TestCase(0, 0)]
    [TestCase(1, 2)]
    [TestCase(-10, -20)]
    public void To(int source, int expected)
    {
        var actual = new LiteralProxy<int>(source).To(it => it * 2).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(0, 2, 0)]
    [TestCase(1, 2, 2)]
    [TestCase(-10, 2, -20)]
    public void To_Tuple(int source1, int source2, int expected)
    {
        var actual = new LiteralProxy<(int, int)>((source1, source2)).To((it1, it2) => it1 * it2).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(0, 0)]
    [TestCase(1, 2)]
    [TestCase(-10, -20)]
    public void With(int source1, int source2)
    {
        var expected = (source1, source2);
        var actual = new LiteralProxy<int>(source1).With(new LiteralProxy<int>(source2)).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(0, 2, 0)]
    [TestCase(1, 2, 2)]
    [TestCase(-10, -20, 200)]
    public void With_Select(int source1, int source2, int expected)
    {
        var actual = new LiteralProxy<int>(source1)
            .With(new LiteralProxy<int>(source2), (a, b) => a * b)
            .Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(0, 2)]
    [TestCase(1, 10)]
    [TestCase(2, 20)]
    public void Of_Value(int index, int expected)
    {
        var source = new[] { 2, 10, 20 };
        var actual = new LiteralProxy<int>(index).Of(source).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(-10)]
    [TestCase(100)]
    public void Extract(int expected)
    {
        var source = new LiteralProxy<IResolvableTo<int>>(new LiteralProxy<int>(expected));
        var actual = source.Extract().Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }
}
