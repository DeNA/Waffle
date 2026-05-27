// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.ModelProxy.Test;

public partial class IsgTest
{
    [ModelProxy]
    public class Model1(IReadOnlyList<Model2> children, (string, Model2, Model2[])[] tuples)
    {
        public readonly IReadOnlyList<Model2> Children = children;
        public readonly (string Hoge, Model2 Fuga, Model2[] Piyo)[] Tuples = tuples;
    }

    [ModelProxy]
    public struct Model2(int value)
    {
        public readonly int Value = value;
    }

    private static readonly Model1 s_model = new(
        [
            new(1),
            new(2),
            new(3),
            new(4),
            new(5),
            new(6),
            new(7),
            new(8),
            new(9)
        ],
        [
            ("aaa", new Model2(100), new Model2[] { new(101), new(102) }),
            ("bbb", new Model2(200), new Model2[] { new(201), new(202) })
        ]
    );

    private string RunNormal()
    {
        return Render($$"""
            {{ForEach(s_model.Children, out var child, out var i, out var h)}}
            Children[{{i}}].Value = {{child.To(it => it.Value)}}{{h.CommaOrLastEmpty}}
            {{End}}
            {{ForEach(s_model.Tuples, out var tuple, out i)}}
            Tuple[{{i}}].Hoge = {{tuple.To(it => it.Hoge)}},
            Tuple[{{i}}].Fuga.Value = {{tuple.To(it => it.Fuga.Value)}},
            {{ForEach(tuple.To(it => it.Piyo), out var v, out var j, out var h2)}}
            Tuple[{{i}}].Piyo[{{j}}].Value = {{v.To(it => it.Value)}}{{h2.CommaOrLastEmpty}}
            {{End}}
            {{End}}
            """);
    }

    private string RunAsProxy()
    {
        var proxy = s_model.AsProxy();
        return Render($$"""
            {{ForEach(proxy.Children, out var child, out var i, out var h)}}
            Children[{{i}}].Value = {{child.Value}}{{h.CommaOrLastEmpty}}
            {{End}}
            {{ForEach(proxy.Tuples, out var tuple, out i)}}
            Tuple[{{i}}].Hoge = {{tuple.Hoge}},
            Tuple[{{i}}].Fuga.Value = {{tuple.Fuga.Value}},
            {{ForEach(tuple.Piyo, out var v, out var j, out var h2)}}
            Tuple[{{i}}].Piyo[{{j}}].Value = {{v.Value}}{{h2.CommaOrLastEmpty}}
            {{End}}
            {{End}}
            """);
    }

    [Test]
    public void AsProxy_BeforeAndAfterModelConversion_OutputMatches()
    {
        var normal = RunNormal();
        var proxy = RunAsProxy();

        Assert.That(proxy, Is.EqualTo(normal));
    }
}
