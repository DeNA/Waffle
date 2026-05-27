# Waffle.Bakery

A batch-execution framework for running multiple [Waffle](../../README.md) templates and collecting their outputs.
It handles the orchestration of template registration, execution, and result retrieval so you can focus on writing
templates.

## Quick Start

Add the NuGet packages to your project:

```xml
<ItemGroup>
    <PackageReference Include="Waffle.Core" Version="1.x"/>
    <PackageReference Include="Waffle.Bakery" Version="1.x"/>
</ItemGroup>
```

### Define Templates

Create classes that inherit from `DefaultTemplate`. Each template declares a unique `OutputId` and implements
`ProcessImpl`:

```csharp
using Waffle;
using static Waffle.WaffleSyntax;

public sealed class VectorTemplate : DefaultTemplate
{
    protected override string OutputId => "vectors";
    protected override void ProcessImpl(DefaultBakeryContext ctx)
    {
        const int N = 10;
        Render(ctx, $"""
            {For(2, N + 1, out var i)}
            public readonly record struct Vector{i}(
                {For(0, i, out var k, out var h)}
                float Value{k + 1}{h.LastOrNot(");", ",")}
                {End}
            {End}
            """);
    }
}

public sealed class HelperTemplate : DefaultTemplate
{
    protected override string OutputId => "helpers";
    protected override void ProcessImpl(DefaultBakeryContext ctx)
    {
        const int N = 10;
        Render(ctx, $$"""
            public static class VectorMath
            {
                {{For(2, N + 1, out var i)}}
                public static float Dot(in Vector{{i}} a, in Vector{{i}} b)
                    => {{For(0, i, out var k, out var h):>>}}
                    a.Value{{k + 1}} * b.Value{{k + 1}}{{h.LastOrNot(";\n", " + "):>>}}
                    {{End}}
                {{End}}
            }
            """);
    }
}
```

### Run the Bakery

Create a `DefaultBakery`, register templates, execute them, and retrieve results:

```csharp
using Waffle;

var results = new DefaultBakery()
    .Initialize(new DefaultBakeryContext())
    .Register<VectorTemplate>()
    .Register<HelperTemplate>()
    .Run()
    .GetResults();

// results is a Dictionary<string, string> keyed by OutputId
Console.WriteLine(results["vectors"]);
Console.WriteLine(results["helpers"]);
```

## Architecture

### Core Interfaces

| Interface              | Role                                                                                           |
|------------------------|------------------------------------------------------------------------------------------------|
| `IBakery<T, TContext>` | Orchestrates template registration and batch execution.                                        |
| `IBakeryContext`       | Extends `ITemplateInterpreterContext` with named output destinations (`Open`/`Close`/`Clear`). |
| `ITemplate<TContext>`  | Defines a single template's execution logic.                                                   |

### Class Hierarchy

```
IBakery<T, TContext>
└── BakeryBase<T, TContext>        # Shared registration/run logic
    └── DefaultBakery              # Ready-to-use concrete bakery

IBakeryContext
└── BakeryContextBase              # Output buffering, error collection, lifecycle hooks
    └── DefaultBakeryContext       # No-op concrete context (sufficient for most use cases)

ITemplate<TContext>
└── SingleOutputTemplate<TContext> # Opens/closes a single OutputId automatically
    └── DefaultTemplate            # Shorthand for SingleOutputTemplate<DefaultBakeryContext>
```

### Execution Flow

1. **Initialize** — `bakery.Initialize(ctx)` stores the context and calls `OnConfigure`.
2. **Register** — Templates are added to an internal list.
3. **Run** — Iterates through registered templates and calls `Process` on each.
4. **Collect** — `GetResults()` returns a `Dictionary<string, string>` mapping each `OutputId` to its rendered content.

Within each `SingleOutputTemplate`, `Process` automatically calls `ctx.Open(OutputId)` before your code and
`ctx.Close()` after, so you only need to implement `ProcessImpl`.

## Template Registration

### Manual registration

```csharp
bakery.Register<MyTemplate>();          // instantiates via parameterless constructor
bakery.Register(new MyTemplate());      // pass a pre-built instance
```

### Registration with a modifier

```csharp
bakery.Register<MyTemplate>(t => t.SomeProperty = value);
```

### Attribute-based auto-discovery

Scan an assembly for all template classes marked with a specific attribute:

```csharp
bakery.RegisterAllByAttribute<GenerateAttribute>(typeof(Program).Assembly);
```

Optionally filter by attribute properties:

```csharp
bakery.RegisterAllByAttribute<GenerateAttribute>(
    typeof(Program).Assembly,
    attr => attr.Enabled);
```

## Customization

### Custom Context

Subclass `BakeryContextBase` to add shared state, logging, or transformation hooks:

```csharp
public class MyContext : BakeryContextBase
{
    public string RootNamespace { get; set; } = "MyApp";

    protected override void OnPostOpen(string outputId, bool isNew)
    {
        if (isNew) Console.WriteLine($"Generating: {outputId}");
    }
}
```

Available hooks in `BakeryContextBase`:

| Hook                                             | Timing                                              |
|--------------------------------------------------|-----------------------------------------------------|
| `OnPreOpen` / `OnPostOpen`                       | Before/after an output destination is opened        |
| `OnPreAppend` / `OnPostAppend`                   | Before/after content is written                     |
| `OnPreClose` / `OnPostClose`                     | Before/after an output destination is closed        |
| `OnPreAppendLiteral` / `OnPostAppendLiteral`     | Before/after a literal string segment is appended   |
| `OnPreAppendFormatted` / `OnPostAppendFormatted` | Before/after an interpolated expression is appended |
| `OnCleared`                                      | After all output is discarded                       |

### Custom Bakery

Subclass `BakeryBase<T, TContext>` to add project-specific orchestration:

```csharp
public class MyBakery : BakeryBase<MyBakery, MyContext>
{
    protected override void OnConfigure(MyContext ctx)
    {
        // Called once during Initialize — set up shared resources here
    }
}
```

### Template Lifecycle Hooks

`SingleOutputTemplate<TContext>` provides `OnPreProcess` and `OnPostProcess` hooks:

```csharp
public abstract class MyTemplateBase : SingleOutputTemplate<MyContext>
{
    protected override void OnPreProcess(MyContext ctx)
    {
        // e.g., emit a file header
        Render(ctx, $"// Auto-generated — do not edit\n");
    }
}
```

## Error Handling

If `ProcessImpl` throws an exception, `SingleOutputTemplate` catches it and records a `TemplateError` on the context.
After execution, retrieve errors with:

```csharp
var errors = bakery.GetErrors(); // Dictionary<string, IReadOnlyList<TemplateError>>
foreach (var (outputId, errs) in errors)
{
    foreach (var e in errs)
        Console.Error.WriteLine($"[{outputId}] {e.Message}");
}
```

## Dependencies

- [Waffle.Core](../Waffle.Core)

