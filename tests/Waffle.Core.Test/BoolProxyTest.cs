// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle.Core.Test;

public class BoolProxyTest
{
    [TestCase(true)]
    [TestCase(false)]
    public void Not(bool self)
    {
        var v = new BoolProxy(self);
        var expected = !self;
        Assert.That((!v).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Or(bool left, bool right)
    {
        var l = new BoolProxy(left);
        var r = new BoolProxy(right);
        var expected = left | right;
        Assert.That((l | r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l | right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left | r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void And(bool left, bool right)
    {
        var l = new BoolProxy(left);
        var r = new BoolProxy(right);
        var expected = left & right;
        Assert.That((l & r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((l & right).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
        Assert.That((left & r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Eq(bool left, bool right)
    {
        var l = new BoolProxy(left);
        var r = new BoolProxy(right);
        var expected = left == right;
        Assert.That((l == r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Ne(bool left, bool right)
    {
        var l = new BoolProxy(left);
        var r = new BoolProxy(right);
        var expected = left != right;
        Assert.That((l != r).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ToString(bool self)
    {
        var v = new BoolProxy(self);
        var expected = self.ToString();
        Assert.That((v.ToString()).Resolve(new Dictionary<int, EnvValue>()), Is.EqualTo(expected));
    }
}
