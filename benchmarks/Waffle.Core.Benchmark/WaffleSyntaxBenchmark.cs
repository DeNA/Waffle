// (c) DeNA Co., Ltd.

using System.Text;
using BenchmarkDotNet.Attributes;
using Mono.TextTemplating;
using static Waffle.WaffleSyntax;

namespace Waffle.Core.Benchmark;

[MemoryDiagnoser(false)]
public class WaffleSyntaxBenchmark
{
    private const int N = 43; // max value that Scriban does not fail for loop limit
    private const string T4InputFilename = "T4.tt";
    private const string T4OutputFilename = "T4.cs";

    private const string T4InputContent = $$"""
        <#@ template language="C#" linePragmas="false" #>
        <#
            const int N = 43;
            for (var i = 2; i <= N; i++)
            {
        #>
        public readonly record struct Vector<#= i #>(
        <#
                for (var k = 0; k < i; k++)
                {
        #>
            float Value<#= k+1 #><#if(k==i-1){#>);<#}else{#>,<#}#>

        <#
                }
            }
        #>

        <#
            for (var i = 1; i < N; i++)
            {
                if (i % 15 == 0)
                {
        #>
        FizzBuzz
        <#
                }
                else if (i % 3 == 0)
                {
        #>
        Fizz
        <#
                }
                else if (i % 5 == 0)
                {
        #>
        Buzz
        <#
                }
                else
                {
        #>
        <#= i #>
        <#      }
            }
        #>
        """;

    private const string ScribanTemplate = """
        {{~ for i in 2..n ~}}
        public readonly record struct Vector{{ i }}(
            {{~ for k in 1..i ~}}
            float Value{{ k }}{{ if k == i }});{{ else }},{{ end }}
            {{~ end ~}}
        {{~ end ~}}
        
        {{~ for i in 1..(n-1) ~}}
            {{~ if i % 15 == 0 ~}}
        FizzBuzz
            {{~ else if i % 3 == 0 ~}}
        Fizz
            {{~ else if i % 5 == 0 ~}}
        Buzz
            {{~ else ~}}
        {{ i }}
            {{~ end ~}}
        {{~ end ~}}
        """;

    [Benchmark(Description = "T4")]
    public string WithT4()
    {
        //cf. https://www.nuget.org/packages/Mono.TextTemplating
        var generator = new TemplateGenerator();
        var parsed = generator.ParseTemplate(T4InputFilename, T4InputContent);
        var settings = TemplatingEngine.GetSettings(generator, parsed);
        settings.CompilerOptions = "-nullable:enable";
        var task = generator.ProcessTemplateAsync(
            parsed, T4InputFilename, T4InputContent, T4OutputFilename, settings
        );
        task.Wait();

        return task.Result.content;
    }

    [Benchmark(Description = "T4(preprocessed)")]
    public string WithT4Preprocessed()
    {
        var template = new T4Preprocessed();
        return template.TransformText();
    }

    [Benchmark(Description = "Scriban")]
    public string WithScriban()
    {
        return Scriban.Template.Parse(ScribanTemplate).Render(new { N });
    }

    [Benchmark(Description = "Waffle")]
    public string WithWaffle()
    {
        //lang=cs
        return Render($"""
            {Note("Vector")}
            {For(2, N + 1, out var i)}
            public readonly record struct Vector{i}(
                {For(0, i, out var k)}
                float Value{k + 1}{(k == i - 1).To(b => b ? ");" : ",")}
                {End}
            {End}

            {Note("FizzBuzz")}
            {For(1, N, out i)}
                {If(i % 15 == 0)}
            FizzBuzz
                {Elif(i % 3 == 0)}
            Fizz
                {Elif(i % 5 == 0)}
            Buzz
                {Else}
            {i}
                {End}
            {End}
            """);
    }

    [Benchmark(Description = "StringBuilder", Baseline = true)]
    public string WithStringBuilder()
    {
        using var _ = StringBuilderPool.Get(out var sb);
        for (var i = 2; i <= N; i++)
        {
            //lang=cs
            sb.Append($"""
                public readonly record struct Vector{i}(

                """);
            for (var k = 1; k <= i; k++)
            {
                //lang=cs
                sb.Append($"""
                    float Value{k}{(k == i ? ");" : ",")}

                """);
            }
        }

        sb.Append('\n');

        for (var i = 1; i < N; i++)
        {
            sb.Append(i % 15 == 0 ? "FizzBuzz" :
                i % 3 == 0 ? "Fizz" :
                i % 5 == 0 ? "Buzz" :
                i.ToString());
            sb.Append('\n');
        }

        return sb.ToString();
    }
}
