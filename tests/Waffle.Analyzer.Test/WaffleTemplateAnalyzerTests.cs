// (c) DeNA Co., Ltd.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Waffle.Analyzer.Test;

[TestFixture]
public class WaffleTemplateAnalyzerTests
{
    private const string Usings = """
        using Waffle;
        using static Waffle.WaffleSyntax;
        using System.Collections.Generic;

        """;

    // ── WAF001: Missing End ──────────────────────────────────────────────────

    [Test]
    public async Task For_WithEnd_NoError()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{For(0, 3, out var i)}}
                    {{i}}
                    {{End}}
                    """);
            }
            """");

        Assert.That(WaffleDiagnostics(diagnostics), Is.Empty);
    }

    [Test]
    public async Task For_WithoutEnd_ReportsWAF001()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{For(0, 3, out var i)}}
                    {{i}}
                    """);
            }
            """");

        var waf001 = diagnostics.Where(d => d.Id == "WAF001").ToArray();
        Assert.That(waf001, Has.Length.EqualTo(1));
        Assert.That(waf001[0].GetMessage(), Does.Contain("For"));
    }

    [Test]
    public async Task ForEach_WithoutEnd_ReportsWAF001()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M()
                {
                    var list = new List<int> { 1, 2, 3 };
                    return Render($$"""
                        {{ForEach(list, out var item)}}
                        {{item}}
                        """);
                }
            }
            """");

        var waf001 = diagnostics.Where(d => d.Id == "WAF001").ToArray();
        Assert.That(waf001, Has.Length.EqualTo(1));
        Assert.That(waf001[0].GetMessage(), Does.Contain("ForEach"));
    }

    [Test]
    public async Task If_WithoutEnd_ReportsWAF001()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{If(true)}}
                    hello
                    """);
            }
            """");

        var waf001 = diagnostics.Where(d => d.Id == "WAF001").ToArray();
        Assert.That(waf001, Has.Length.EqualTo(1));
        Assert.That(waf001[0].GetMessage(), Does.Contain("If"));
    }

    [Test]
    public async Task If_ElifElseEnd_NoError()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M(int x) => Render($$"""
                    {{If(x > 0)}}
                    positive
                    {{Elif(x < 0)}}
                    negative
                    {{Else}}
                    zero
                    {{End}}
                    """);
            }
            """");

        Assert.That(WaffleDiagnostics(diagnostics), Is.Empty);
    }

    [Test]
    public async Task MultipleUnclosedBlocks_ReportsWAF001ForEach()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M()
                {
                    var list = new List<int> { 1 };
                    return Render($$"""
                        {{ForEach(list, out var item)}}
                        {{If(true)}}
                        {{item}}
                        """);
                }
            }
            """");

        Assert.That(diagnostics.Where(d => d.Id == "WAF001"), Has.Exactly(2).Items);
    }

    [Test]
    public async Task NestedFor_WithEnd_NoError()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{For(0, 3, out var i)}}
                    {{For(0, 2, out var j)}}
                    {{i}},{{j}}
                    {{End}}
                    {{End}}
                    """);
            }
            """");

        Assert.That(WaffleDiagnostics(diagnostics), Is.Empty);
    }

    // ── WAF002: Unexpected End ───────────────────────────────────────────────

    [Test]
    public async Task StandaloneEnd_ReportsWAF002()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{End}}
                    """);
            }
            """");

        Assert.That(diagnostics.Where(d => d.Id == "WAF002"), Has.Exactly(1).Items);
    }

    [Test]
    public async Task ExtraEnd_ReportsWAF002()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{For(0, 3, out var i)}}
                    {{i}}
                    {{End}}
                    {{End}}
                    """);
            }
            """");

        Assert.That(diagnostics.Where(d => d.Id == "WAF002"), Has.Exactly(1).Items);
    }

    // ── WAF003: Out-of-scope variable ────────────────────────────────────────

    [Test]
    public async Task For_VariableAfterEnd_ReportsWAF003()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{For(0, 3, out var i)}}
                    {{i}}
                    {{End}}
                    {{i}}
                    """);
            }
            """");

        var waf003 = diagnostics.Where(d => d.Id == "WAF003").ToArray();
        Assert.That(waf003, Has.Length.EqualTo(1));
        Assert.That(waf003[0].GetMessage(), Does.Contain("i"));
    }

    [Test]
    public async Task ForEach_VariableAfterEnd_ReportsWAF003()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M()
                {
                    var list = new List<int> { 1, 2, 3 };
                    return Render($$"""
                        {{ForEach(list, out var item)}}
                        {{item}}
                        {{End}}
                        {{item}}
                        """);
                }
            }
            """");

        var waf003 = diagnostics.Where(d => d.Id == "WAF003").ToArray();
        Assert.That(waf003, Has.Length.EqualTo(1));
        Assert.That(waf003[0].GetMessage(), Does.Contain("item"));
    }

    [Test]
    public async Task OuterVariable_UsedInsideInnerBlock_NoError()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{For(0, 3, out var i)}}
                    {{For(0, 2, out var j)}}
                    {{i}},{{j}}
                    {{End}}
                    {{End}}
                    """);
            }
            """");

        Assert.That(WaffleDiagnostics(diagnostics), Is.Empty);
    }

    [Test]
    public async Task For_RebindVariableInNextBlock_NoError()
    {
        // 'i' is reused via "out i" (without var) in a second For block — should not be WAF003
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{For(0, 3, out var i)}}
                    {{i}}
                    {{End}}
                    {{For(1, 5, out i)}}
                    {{i}}
                    {{End}}
                    {{Forr(4, 0, out i)}}
                    {{i}}
                    {{End}}
                    """);
            }
            """");

        Assert.That(WaffleDiagnostics(diagnostics), Is.Empty);
    }

    [Test]
    public async Task For_RebindVariable_ThenUseAfterEnd_ReportsWAF003()
    {
        // After the second block's End, 'i' is again out of scope
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{For(0, 3, out var i)}}
                    {{i}}
                    {{End}}
                    {{For(1, 5, out i)}}
                    {{i}}
                    {{End}}
                    {{i}}
                    """);
            }
            """");

        var waf003 = diagnostics.Where(d => d.Id == "WAF003").ToArray();
        Assert.That(waf003, Has.Length.EqualTo(1));
        Assert.That(waf003[0].GetMessage(), Does.Contain("i"));
    }

    // ── WAF004: Elif/Else outside If block ───────────────────────────────────

    [Test]
    public async Task Elif_WithNoBlock_ReportsWAF004()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{Elif(true)}}
                    hello
                    """);
            }
            """");

        var waf004 = diagnostics.Where(d => d.Id == "WAF004").ToArray();
        Assert.That(waf004, Has.Length.EqualTo(1));
        Assert.That(waf004[0].GetMessage(), Does.Contain("Elif"));
    }

    [Test]
    public async Task Else_WithNoBlock_ReportsWAF004()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{Else}}
                    hello
                    """);
            }
            """");

        var waf004 = diagnostics.Where(d => d.Id == "WAF004").ToArray();
        Assert.That(waf004, Has.Length.EqualTo(1));
        Assert.That(waf004[0].GetMessage(), Does.Contain("Else"));
    }

    [Test]
    public async Task Elif_InsideForBlock_ReportsWAF004()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{For(0, 3, out var i)}}
                    {{Elif(true)}}
                    {{i}}
                    {{End}}
                    """);
            }
            """");

        Assert.That(diagnostics.Where(d => d.Id == "WAF004"), Has.Exactly(1).Items);
    }

    [Test]
    public async Task Elif_InsideIfBlock_NoError()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M(int x) => Render($$"""
                    {{If(x > 0)}}
                    positive
                    {{Elif(x < 0)}}
                    negative
                    {{End}}
                    """);
            }
            """");

        Assert.That(WaffleDiagnostics(diagnostics), Is.Empty);
    }

    // ── WAF005: Multiple Else in If block ────────────────────────────────────

    [Test]
    public async Task SecondElse_InIfBlock_ReportsWAF005()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{If(true)}}
                    a
                    {{Else}}
                    b
                    {{Else}}
                    c
                    {{End}}
                    """);
            }
            """");

        Assert.That(diagnostics.Where(d => d.Id == "WAF005"), Has.Exactly(1).Items);
    }

    [Test]
    public async Task If_WithSingleElse_NoError()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{If(true)}}
                    yes
                    {{Else}}
                    no
                    {{End}}
                    """);
            }
            """");

        Assert.That(WaffleDiagnostics(diagnostics), Is.Empty);
    }

    // ── WAF006: Elif after Else ──────────────────────────────────────────────

    [Test]
    public async Task ElifAfterElse_ReportsWAF006()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M(int x) => Render($$"""
                    {{If(x > 0)}}
                    positive
                    {{Else}}
                    other
                    {{Elif(x < 0)}}
                    negative
                    {{End}}
                    """);
            }
            """");

        Assert.That(diagnostics.Where(d => d.Id == "WAF006"), Has.Exactly(1).Items);
    }

    [Test]
    public async Task ElifBeforeElse_NoError()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M(int x) => Render($$"""
                    {{If(x > 0)}}
                    positive
                    {{Elif(x < 0)}}
                    negative
                    {{Else}}
                    zero
                    {{End}}
                    """);
            }
            """");

        Assert.That(WaffleDiagnostics(diagnostics), Is.Empty);
    }

    // ── WAF007: Break/Continue outside iteration block ───────────────────────

    [Test]
    public async Task Break_WithNoBlock_ReportsWAF007()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{Break}}
                    """);
            }
            """");

        var waf007 = diagnostics.Where(d => d.Id == "WAF007").ToArray();
        Assert.That(waf007, Has.Length.EqualTo(1));
        Assert.That(waf007[0].GetMessage(), Does.Contain("Break"));
    }

    [Test]
    public async Task Continue_WithNoBlock_ReportsWAF007()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{Continue}}
                    """);
            }
            """");

        var waf007 = diagnostics.Where(d => d.Id == "WAF007").ToArray();
        Assert.That(waf007, Has.Length.EqualTo(1));
        Assert.That(waf007[0].GetMessage(), Does.Contain("Continue"));
    }

    [Test]
    public async Task Break_InsideIfWithoutForEach_ReportsWAF007()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{If(true)}}
                    {{Break}}
                    {{End}}
                    """);
            }
            """");

        Assert.That(diagnostics.Where(d => d.Id == "WAF007"), Has.Exactly(1).Items);
    }

    [Test]
    public async Task Break_InsideIfInsideFor_NoError()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M() => Render($$"""
                    {{For(0, 10, out var i)}}
                    {{If(i == 5)}}
                    {{Break}}
                    {{End}}
                    {{i}}
                    {{End}}
                    """);
            }
            """");

        Assert.That(WaffleDiagnostics(diagnostics), Is.Empty);
    }

    [Test]
    public async Task Continue_InsideForEach_NoError()
    {
        var diagnostics = await GetDiagnosticsAsync(Usings + """"
            class Test
            {
                string M()
                {
                    var list = new List<int> { 1, 2, 3 };
                    return Render($$"""
                        {{ForEach(list, out var item)}}
                        {{If(item == 2)}}
                        {{Continue}}
                        {{End}}
                        {{item}}
                        {{End}}
                        """);
                }
            }
            """");

        Assert.That(WaffleDiagnostics(diagnostics), Is.Empty);
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static IEnumerable<Diagnostic> WaffleDiagnostics(IReadOnlyList<Diagnostic> all) =>
        all.Where(d => d.Id.StartsWith("WAF", StringComparison.Ordinal));

    private static async Task<IReadOnlyList<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var parseOptions = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.Latest);
        var tree = CSharpSyntaxTree.ParseText(source, parseOptions);

        // Explicitly include Waffle.Core to guarantee symbol resolution for Render/For/End etc.
        var waffleCoreRef = MetadataReference.CreateFromFile(typeof(WaffleSyntax).Assembly.Location);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .Append(waffleCoreRef);

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { tree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new WaffleTemplateAnalyzer());
        return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync();
    }
}
