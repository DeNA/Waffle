// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.Core.Test;

public class IntProxyTest
{
    [TestCase(0, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(3, 7)]
    [TestCase(7, 3)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(10, -5)]
    [TestCase(-5, 10)]
    [TestCase(-17, -5)]
    [TestCase(-5, -17)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(int.MaxValue, int.MinValue)]
    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(int.MinValue, int.MinValue)]
    public void Add(int left, int right)
    {
        var l = new IntProxy(new LiteralProxy<int>(left));
        var r = new IntProxy(new LiteralProxy<int>(right));
        var expected = left + right;
        Assert.That((l + r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l + right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left + r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(3, 7)]
    [TestCase(7, 3)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(10, -5)]
    [TestCase(-5, 10)]
    [TestCase(-17, -5)]
    [TestCase(-5, -17)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(int.MaxValue, int.MinValue)]
    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(int.MinValue, int.MinValue)]
    public void Sub(int left, int right)
    {
        var l = new IntProxy(new LiteralProxy<int>(left));
        var r = new IntProxy(new LiteralProxy<int>(right));
        var expected = left - right;
        Assert.That((l - r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l - right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left - r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }


    [TestCase(0, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(3, 7)]
    [TestCase(7, 3)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(10, -5)]
    [TestCase(-5, 10)]
    [TestCase(-17, -5)]
    [TestCase(-5, -17)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(int.MaxValue, int.MinValue)]
    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(int.MinValue, int.MinValue)]
    public void Mul(int left, int right)
    {
        var l = new IntProxy(new LiteralProxy<int>(left));
        var r = new IntProxy(new LiteralProxy<int>(right));
        var expected = left * right;
        Assert.That((l * r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l * right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left * r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }


    [TestCase(0, 1)]
    [TestCase(3, 7)]
    [TestCase(7, 3)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(10, -5)]
    [TestCase(-5, 10)]
    [TestCase(-17, -5)]
    [TestCase(-5, -17)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(int.MaxValue, int.MinValue)]
    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(int.MinValue, int.MinValue)]
    public void Div(int left, int right)
    {
        var l = new IntProxy(new LiteralProxy<int>(left));
        var r = new IntProxy(new LiteralProxy<int>(right));
        var expected = left / right;
        Assert.That((l / r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l / right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left / r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0, 1)]
    [TestCase(3, 7)]
    [TestCase(7, 3)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(10, -5)]
    [TestCase(-5, 10)]
    [TestCase(-17, -5)]
    [TestCase(-5, -17)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(int.MaxValue, int.MinValue)]
    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(int.MinValue, int.MinValue)]
    public void Mod(int left, int right)
    {
        var l = new IntProxy(new LiteralProxy<int>(left));
        var r = new IntProxy(new LiteralProxy<int>(right));
        var expected = left % right;
        Assert.That((l % r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l % right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left % r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(3, 7)]
    [TestCase(7, 3)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(10, -5)]
    [TestCase(-5, 10)]
    [TestCase(-17, -5)]
    [TestCase(-5, -17)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(int.MaxValue, int.MinValue)]
    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(int.MinValue, int.MinValue)]
    public void Lt(int left, int right)
    {
        var l = new IntProxy(new LiteralProxy<int>(left));
        var r = new IntProxy(new LiteralProxy<int>(right));
        var expected = left < right;
        Assert.That((l < r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l < right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left < r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(3, 7)]
    [TestCase(7, 3)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(10, -5)]
    [TestCase(-5, 10)]
    [TestCase(-17, -5)]
    [TestCase(-5, -17)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(int.MaxValue, int.MinValue)]
    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(int.MinValue, int.MinValue)]
    public void Gt(int left, int right)
    {
        var l = new IntProxy(new LiteralProxy<int>(left));
        var r = new IntProxy(new LiteralProxy<int>(right));
        var expected = left > right;
        Assert.That((l > r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l > right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left > r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(3, 7)]
    [TestCase(7, 3)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(10, -5)]
    [TestCase(-5, 10)]
    [TestCase(-17, -5)]
    [TestCase(-5, -17)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(int.MaxValue, int.MinValue)]
    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(int.MinValue, int.MinValue)]
    public void Le(int left, int right)
    {
        var l = new IntProxy(new LiteralProxy<int>(left));
        var r = new IntProxy(new LiteralProxy<int>(right));
        var expected = left <= right;
        Assert.That((l <= r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l <= right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left <= r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(3, 7)]
    [TestCase(7, 3)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(10, -5)]
    [TestCase(-5, 10)]
    [TestCase(-17, -5)]
    [TestCase(-5, -17)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(int.MaxValue, int.MinValue)]
    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(int.MinValue, int.MinValue)]
    public void Ge(int left, int right)
    {
        var l = new IntProxy(new LiteralProxy<int>(left));
        var r = new IntProxy(new LiteralProxy<int>(right));
        var expected = left >= right;
        Assert.That((l >= r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l >= right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left >= r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(3, 7)]
    [TestCase(7, 3)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(10, -5)]
    [TestCase(-5, 10)]
    [TestCase(-17, -5)]
    [TestCase(-5, -17)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(int.MaxValue, int.MinValue)]
    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(int.MinValue, int.MinValue)]
    public void Eq(int left, int right)
    {
        var l = new IntProxy(new LiteralProxy<int>(left));
        var r = new IntProxy(new LiteralProxy<int>(right));
        var expected = left == right;
        Assert.That((l == r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l == right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left == r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(3, 7)]
    [TestCase(7, 3)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(10, -5)]
    [TestCase(-5, 10)]
    [TestCase(-17, -5)]
    [TestCase(-5, -17)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(int.MaxValue, int.MinValue)]
    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(int.MinValue, int.MinValue)]
    public void Ne(int left, int right)
    {
        var l = new IntProxy(new LiteralProxy<int>(left));
        var r = new IntProxy(new LiteralProxy<int>(right));
        var expected = left != right;
        Assert.That((l != r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l != right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left != r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(1)]
    [TestCase(-10)]
    [TestCase(10)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void Inc(int self)
    {
        var v = new IntProxy(new LiteralProxy<int>(self));
        var expected = ++self;
        Assert.That((++v).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(1)]
    [TestCase(-10)]
    [TestCase(10)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void Dec(int self)
    {
        var v = new IntProxy(new LiteralProxy<int>(self));
        var expected = --self;
        Assert.That((--v).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(1)]
    [TestCase(-10)]
    [TestCase(10)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void Pos(int self)
    {
        var v = new IntProxy(new LiteralProxy<int>(self));
        var expected = +self;
        Assert.That((+v).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(1)]
    [TestCase(-10)]
    [TestCase(10)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void Neg(int self)
    {
        var v = new IntProxy(new LiteralProxy<int>(self));
        var expected = -self;
        Assert.That((-v).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(1)]
    [TestCase(-10)]
    [TestCase(10)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void ToString(int self)
    {
        var v = new IntProxy(new LiteralProxy<int>(self));
        var expected = self.ToString();
        Assert.That((v.ToString()).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }
}
