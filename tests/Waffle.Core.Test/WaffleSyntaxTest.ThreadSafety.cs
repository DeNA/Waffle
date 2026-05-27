// (c) DeNA Co., Ltd.

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Waffle.Interpreter;

namespace Waffle.Core.Test;

public partial class WaffleSyntaxTest
{
    // Tests for thread safety
    // Each test uses a Barrier to release all threads simultaneously, maximizing race conditions for verification.

    private const int ConcurrentCount = 10;

    [Test]
    public async Task Render_Multithreaded_ForEach_AllThreadsProduceSameResult()
    {
        var items = new[] { "alpha", "beta", "gamma" };
        var expected = Render($$"""
            {{ForEach(items, out var e0)}}
            - {{e0}}
            {{End}}
            """);

        var barrier = new Barrier(ConcurrentCount);
        var tasks = Enumerable.Range(0, ConcurrentCount).Select(_ => Task.Run(() =>
        {
            barrier.SignalAndWait();
            return Render($$"""
                {{ForEach(items, out var item)}}
                - {{item}}
                {{End}}
                """);
        })).ToArray();

        var results = await Task.WhenAll(tasks);
        Assert.That(results, Is.All.EqualTo(expected));
    }

    [Test]
    public async Task Render_Multithreaded_NestedForEach_AllThreadsProduceSameResult()
    {
        var outer = new[] { "A", "B" };
        var inner = new[] { "1", "2", "3" };
        var expected = Render($$"""
            {{ForEach(outer, out var eo)}}
            {{ForEach(inner, out var ei)}}
            {{eo}}-{{ei}}
            {{End}}
            {{End}}
            """);

        var barrier = new Barrier(ConcurrentCount);
        var tasks = Enumerable.Range(0, ConcurrentCount).Select(_ => Task.Run(() =>
        {
            barrier.SignalAndWait();
            return Render($$"""
                {{ForEach(outer, out var o)}}
                {{ForEach(inner, out var ii)}}
                {{o}}-{{ii}}
                {{End}}
                {{End}}
                """);
        })).ToArray();

        var results = await Task.WhenAll(tasks);
        Assert.That(results, Is.All.EqualTo(expected));
    }

    [Test]
    public async Task Render_Multithreaded_For_AllThreadsProduceSameResult()
    {
        var expected = Render($$"""
            {{For(0, 5, out var e1)}}
            {{e1}}
            {{End}}
            """);

        var barrier = new Barrier(ConcurrentCount);
        var tasks = Enumerable.Range(0, ConcurrentCount).Select(_ => Task.Run(() =>
        {
            barrier.SignalAndWait();
            return Render($$"""
                {{For(0, 5, out var i)}}
                {{i}}
                {{End}}
                """);
        })).ToArray();

        var results = await Task.WhenAll(tasks);
        Assert.That(results, Is.All.EqualTo(expected));
    }

    [Test]
    public async Task Render_Multithreaded_If_Elif_Else_AllThreadsProduceSameResult()
    {
        var expected = Render($$"""
            {{If(false)}}
            yes
            {{Elif(true)}}
            no
            {{Else}}
            neither
            {{End}}
            """);

        var barrier = new Barrier(ConcurrentCount);
        var tasks = Enumerable.Range(0, ConcurrentCount).Select(_ => Task.Run(() =>
        {
            barrier.SignalAndWait();
            return Render($$"""
                {{If(false)}}
                yes
                {{Elif(true)}}
                no
                {{Else}}
                neither
                {{End}}
                """);
        })).ToArray();

        var results = await Task.WhenAll(tasks);
        Assert.That(results, Is.All.EqualTo(expected));
    }

    [Test]
    public async Task Render_Multithreaded_ForEach_ModelProxy_AllThreadsProduceSameResult()
    {
        var expected = Render($$"""
            {{ForEach(s_structModel.AsProxy().Properties, out var ep)}}
            {{ep.Type}} {{ep.Name}};
            {{End}}
            """);

        var barrier = new Barrier(ConcurrentCount);
        var tasks = Enumerable.Range(0, ConcurrentCount).Select(_ => Task.Run(() =>
        {
            var m = s_structModel.AsProxy();
            barrier.SignalAndWait();
            return Render($$"""
                {{ForEach(m.Properties, out var prop)}}
                {{prop.Type}} {{prop.Name}};
                {{End}}
                """);
        })).ToArray();

        var results = await Task.WhenAll(tasks);
        Assert.That(results, Is.All.EqualTo(expected));
    }

    [Test]
    public async Task SyntaxError_Multithreaded_ErrorIdIsUnique()
    {
        var capturedMessages = new ConcurrentBag<string>();

        var barrier = new Barrier(ConcurrentCount);
        var tasks = Enumerable.Range(0, ConcurrentCount).Select(_ => Task.Run(() =>
        {
            var ctx = new SyntaxErrorCapturingContext(capturedMessages);
            barrier.SignalAndWait();
            ctx.SyntaxError("concurrent error test");
        })).ToArray();

        await Task.WhenAll(tasks);

        var ids = capturedMessages
            .Select(msg =>
            {
                var match = ErrorIdRegex().Match(msg);
                return match.Success ? (int?)int.Parse(match.Groups[1].Value) : null;
            })
            .ToList();

        Assert.That(ids, Has.Count.EqualTo(ConcurrentCount));
        Assert.That(ids, Has.None.Null);
        Assert.That(ids, Is.Unique);
    }

    private sealed class SyntaxErrorCapturingContext(ConcurrentBag<string> bag) : ITemplateInterpreterContext
    {
        public void Append(string value) { }

        public void Error(in TemplateError error)
        {
            bag.Add(error.Message);
        }

        public void OnHandlerCreated(int literalLength, int formattedCount, TemplateInterpreterController controller)
        {
        }

        public void OnPreAppendLiteral(ref string willBeAppended, TemplateInterpreterController controller) { }
        public void OnPostAppendLiteral(string appended, TemplateInterpreterController controller) { }

        public void OnPreAppendFormatted<T>(
            ref T x, ref int alignment, ref string? format, TemplateInterpreterController controller)
        {
        }

        public bool TryHandleUnhandledInterpolation<T>(
            T x, int alignment, string? format, TemplateInterpreterController controller) => false;

        public void OnPostAppendFormatted<T>(T x, TemplateInterpreterController controller) { }

        public void OnCompleted(TemplateInterpreterController controller)
        {
        }
    }

    [GeneratedRegex(@"\(id=(\d+)\)")]
    private static partial Regex ErrorIdRegex();
}
