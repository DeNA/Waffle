// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.Core.Test;

public class IIterationSourceTest
{
    private static readonly Dictionary<int, EnvValue> s_emptyEnv = new();

    [Test]
    public void Count()
    {
        var source = new[] { 10, 0, -5 };
        var actual = source.AsProxy().Count.Resolve(s_emptyEnv);
        var expected = source.Length;
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(0, 10)]
    [TestCase(1, 0)]
    [TestCase(2, -5)]
    public void Indexer_Value(int index, int expected)
    {
        var source = new[] { 10, 0, -5 }.AsProxy();
        var actual = source[index].Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(0, 10)]
    [TestCase(1, 0)]
    [TestCase(2, -5)]
    public void Indexer_IResolvable(int index, int expected)
    {
        var source = new[] { 10, 0, -5 }.AsProxy();
        var actual = source[new LiteralProxy<int>(index)].Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void AsProxy_IEnumerable()
    {
        IEnumerable<int> source = [1, 2, 3];
        var actual = source.AsProxy().Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(source));
    }

    [Test]
    public void AsProxy_IResolvableToIEnumerable()
    {
        var expected = new[] { 1, 2, 3 };
        var source = new LiteralProxy<IEnumerable<int>>(expected);
        var actual = source.AsProxy().Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void AsProxy_IResolvableToIReadOnlyList()
    {
        var expected = new[] { 1, 2, 3 };
        var source = new LiteralProxy<IReadOnlyList<int>>(expected.ToArray());
        var actual = source.AsProxy().Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void AsProxy_IResolvableToList()
    {
        var expected = new[] { 1, 2, 3 };
        var source = new LiteralProxy<List<int>>(expected.ToList());
        var actual = source.AsProxy().Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void AsProxy_IResolvableToArray()
    {
        var expected = new[] { 1, 2, 3 };
        var source = new LiteralProxy<int[]>(expected);
        var actual = source.AsProxy().Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Select_ValueToValue()
    {
        var source = new[] { 1, 2, 3 }.AsProxy();
        var expected = new[] { 2, 4, 6 };
        var actual = source.Select(it => it * 2).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Select_ValueToValueIndexed()
    {
        var source = new[] { 1, 2, 3 }.AsProxy();
        var expected = new[] { 0, 2, 6 };
        var actual = source.Select((it, i) => it * i).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Select_ValueToIResolvable()
    {
        var source = new[] { 1, 2, 3 }.AsProxy();
        var expected = new[] { 2, 4, 6 };
        var actual = source.Select<IResolvableTo<int>, int, int>(it => new LiteralProxy<int>(it * 2))
            .Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Select_ValueToIResolvableIndexed()
    {
        var source = new[] { 1, 2, 3 }.AsProxy();
        var expected = new[] { 0, 2, 6 };
        var actual = source.Select<IResolvableTo<int>, int, int>((it, i) => new LiteralProxy<int>(it * i))
            .Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Where()
    {
        var source = new[] { 1, 2, 3 }.AsProxy();
        var expected = new[] { 1, 3 };
        var actual = source.Where(it => (it & 1) == 1).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Skip_Value()
    {
        var source = new[] { 1, 2, 3, 4, 5 }.AsProxy();
        var expected = new[] { 3, 4, 5 };
        var actual = source.Skip(2).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Skip_IResolvable()
    {
        var source = new[] { 1, 2, 3, 4, 5 }.AsProxy();
        var expected = new[] { 3, 4, 5 };
        var actual = source.Skip(new LiteralProxy<int>(2)).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Take_Value()
    {
        var source = new[] { 1, 2, 3, 4, 5 }.AsProxy();
        var expected = new[] { 1, 2 };
        var actual = source.Take(2).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Take_IResolvable()
    {
        var source = new[] { 1, 2, 3, 4, 5 }.AsProxy();
        var expected = new[] { 1, 2 };
        var actual = source.Take(new LiteralProxy<int>(2)).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void IndexOf_Value()
    {
        var source = new[] { 1, 2, 3, 4, 5 }.AsProxy();
        var expected = 3;
        var actual = source.IndexOf(4).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void IndexOf_IResolvable()
    {
        var source = new[] { 1, 2, 3, 4, 5 }.AsProxy();
        var expected = 3;
        var actual = source.IndexOf(new LiteralProxy<int>(4)).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Join()
    {
        var source = new[] { 1, 2, 3, 4, 5 }.AsProxy();
        var expected = "[1]-[2]-[3]-[4]-[5]";
        var actual = source.Join("-", it => $"[{it}]").Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Join_PrefixSuffix_NonEmpty()
    {
        var source = new[] { "T", "U", "V" }.AsProxy();
        var actual = source.Join(", ", "<", ">").Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo("<T, U, V>"));
    }

    [Test]
    public void Join_PrefixSuffix_Empty_ReturnsEmptyString()
    {
        var source = Array.Empty<string>().AsProxy();
        var actual = source.Join(", ", "<", ">").Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(""));
    }

    [Test]
    public void Join_PrefixSuffix_SingleItem()
    {
        var source = new[] { "T" }.AsProxy();
        var actual = source.Join(", ", "<", ">").Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo("<T>"));
    }

    [Test]
    public void Join_PrefixSuffix_WithTransform()
    {
        var source = new[] { 1, 2, 3 }.AsProxy();
        var actual = source.Join(", ", "(", ")", it => $"int arg{it}").Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo("(int arg1, int arg2, int arg3)"));
    }

    [Test]
    public void OrderBy_ByValue()
    {
        var source = new[] { 3, 1, 4, 1, 5, 9, 2, 6 }.AsProxy();
        var expected = new[] { 1, 1, 2, 3, 4, 5, 6, 9 };
        var actual = source.OrderBy(it => it).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void OrderBy_ByKeySelector()
    {
        var source = new[] { "banana", "fig", "apple", "kiwi" }.AsProxy();
        var expected = new[] { "fig", "kiwi", "apple", "banana" };
        var actual = source.OrderBy(it => it.Length).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void OrderBy_AlreadySorted_ReturnsSameOrder()
    {
        var source = new[] { 1, 2, 3, 4, 5 }.AsProxy();
        var expected = new[] { 1, 2, 3, 4, 5 };
        var actual = source.OrderBy(it => it).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void OrderBy_SingleElement()
    {
        var source = new[] { 42 }.AsProxy();
        var expected = new[] { 42 };
        var actual = source.OrderBy(it => it).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void OrderBy_Empty()
    {
        var source = Array.Empty<int>().AsProxy();
        var actual = source.OrderBy(it => it).Resolve(s_emptyEnv);
        Assert.That(actual, Is.Empty);
    }

    [Test]
    public void OrderByDescending_ByValue()
    {
        var source = new[] { 3, 1, 4, 1, 5, 9, 2, 6 }.AsProxy();
        var expected = new[] { 9, 6, 5, 4, 3, 2, 1, 1 };
        var actual = source.OrderByDescending(it => it).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void OrderByDescending_ByKeySelector()
    {
        var source = new[] { "banana", "fig", "apple", "kiwi" }.AsProxy();
        var expected = new[] { "banana", "apple", "kiwi", "fig" };
        var actual = source.OrderByDescending(it => it.Length).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void OrderByDescending_AlreadySorted_ReversesOrder()
    {
        var source = new[] { 1, 2, 3, 4, 5 }.AsProxy();
        var expected = new[] { 5, 4, 3, 2, 1 };
        var actual = source.OrderByDescending(it => it).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void OrderByDescending_SingleElement()
    {
        var source = new[] { 42 }.AsProxy();
        var expected = new[] { 42 };
        var actual = source.OrderByDescending(it => it).Resolve(s_emptyEnv);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void OrderByDescending_Empty()
    {
        var source = Array.Empty<int>().AsProxy();
        var actual = source.OrderByDescending(it => it).Resolve(s_emptyEnv);
        Assert.That(actual, Is.Empty);
    }
}
