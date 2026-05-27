# Waffle.Core

The core engine of the [Waffle](../../README.md) — a pure C# template engine that embeds
flow control directly into interpolated string literals via a customized `InterpolatedStringHandler`.

## Syntax Reference

See [docs/syntax-reference.md](../../docs/syntax-reference.md) for the full syntax reference.

## Architecture

### High-Level Overview

Waffle works by hijacking C#'s interpolated string machinery. When you write:

```csharp
Render(ctx, $"...{For(0, n, out var i)}...{End}...");
```

The compiler expands this into a series of `AppendLiteral` / `AppendFormatted` calls on a
`TemplateInterpreter` struct (marked with `[InterpolatedStringHandler]`). Instead of
immediately producing output, these calls build a flat token list that is later evaluated
as a syntax tree.

### Processing Phases

```
Source code (interpolated string)
        │
        ▼
┌─────────────────────────┐
│  1. PARSING             │  AppendLiteral / AppendFormatted calls
│     TemplateInterpreter │  → builds flat List<Token>
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│  2. BLOCK PAIRING       │  PairBlockIndexes()
│     TemplateEvaluator   │  → links For↔End, If↔Elif↔Else↔End
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│  3. TRIM / WHITESPACE   │  ApplyTrims()
│     TemplateEvaluator   │  → removes command-only lines, applies < > directives
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│  4. EVALUATION          │  EvaluateRange()
│     TemplateEvaluator   │  → walks tokens, executes blocks, resolves values
│                         │  → appends output via ITemplateInterpreterContext.Append()
└─────────────────────────┘
```

### Key Types

| Type                             | Kind         | Role                                                                                                 |
|----------------------------------|--------------|------------------------------------------------------------------------------------------------------|
| `WaffleSyntax`                   | static class | Public API surface — `For`, `ForEach`, `If`, `Elif`, `Else`, `End`, `Render`, etc.                   |
| `TemplateInterpreter`            | struct       | `[InterpolatedStringHandler]` that parses interpolated strings into tokens.                          |
| `InstantTemplateInterpreter`     | struct       | Variant that returns the result string directly (no external context needed).                        |
| `TemplateInterpreterController`  | class        | Mutable state container: token list, block stack, consumption counters.                              |
| `TemplateEvaluator`              | struct       | Pairs blocks, applies trims, and evaluates the token list into output.                               |
| `Token`                          | class        | A single element (literal or interpolation) in the flat token list. Pooled for reuse.                |
| `ITemplateInterpreterContext`    | interface    | Receives output and lifecycle hooks during evaluation.                                               |
| `IBlockContent`                  | interface    | Any interpolated value that resolves to output (tokens, pipelines).                                  |
| `IResolvableTo<T>`               | interface    | A lazily-resolved value — evaluated only at render time using the environment dictionary.            |
| `IIterationSource<TIterator, T>` | interface    | A list with typed iterators for use in `ForEach` loops.                                              |
| `EnvValue`                       | struct       | Tagged union (`int` or `object?`) for the environment dictionary, avoiding boxing for loop counters. |

### Directory Layout

```
Waffle.Core/
├── WaffleSyntax.cs                          # Public static API
├── Interpreter/
│   ├── Interfaces/
│   │   ├── ITemplateInterpreterContext.cs   # Output + lifecycle hooks
│   │   ├── IResolvableTo.cs                 # Lazy resolution abstraction + pipeline extensions
│   │   ├── IIterationSource.cs              # Typed iteration source + LINQ-like extensions
│   │   └── ILazyInitializedBy.cs            # Workaround for abstract static on netstandard2.0
│   ├── Impls/
│   │   ├── TemplateInterpreter.cs           # Parser (InterpolatedStringHandler)
│   │   ├── InstantTemplateInterpreter.cs    # Self-contained variant (no external context)
│   │   ├── InstantTemplateContext.cs        # Simple context for InstantTemplateInterpreter
│   │   ├── TemplateInterpreterController.cs # Mutable parsing state
│   │   ├── TemplateEvaluator.cs             # Block pairing + trim + evaluation
│   │   ├── TemplateInterpreterHelper.cs     # Formatting utilities
│   │   ├── Token.cs                         # Token data + object pool
│   │   ├── FlowControl.cs                   # Break/Continue signal enum
│   │   └── TrimMode.cs                      # Whitespace trim directives
│   ├── Block/
│   │   ├── IBlockContent.cs                 # Evaluable block element interface
│   │   ├── BlockCategory.cs                 # Conditional vs Iteration
│   │   └── EvalResult.cs                    # Stringified evaluation result
│   ├── Interpolation/
│   │   ├── Command/
│   │   │   ├── Iteration/                   # For, ForEach, Break, Continue commands
│   │   │   └── Condition/                   # If, Elif, Else commands
│   │   ├── Pipeline/                        # SelectPipe, CombinePipe, ExtractPipe, etc.
│   │   └── Proxy/                           # IntProxy, StringProxy, BoolProxy, ListProxy, etc.
│   ├── EnvValue.cs                          # Tagged-union for environment values
│   └── TemplateError.cs                     # Error reporting record
├── Helper/                                  # Object pools (List, Stack, StringBuilder, etc.)
└── Polyfills/                               # netstandard2.0 backports (Index, Range, attributes)
```

### Lazy Resolution & Environment

Loop variables in Waffle are not captured by closures. Instead, they are stored in an
**environment dictionary** (`Dictionary<int, EnvValue>`) keyed by a generated integer ID.
At evaluation time, `IResolvableTo<T>.Resolve(env)` reads the current value from this dictionary.

This design enables **composable pipelines** via `.To()`, `.With()`, `.Of()`, `.Extract()`, `.Replace()` and is
supported by **type-safe deferred wrappers** generated by [Waffle.ModelProxy](../Waffle.ModelProxy/README.md).

### Extensibility Points

| Extension Point                               | How to Use                                                                                        |
|-----------------------------------------------|---------------------------------------------------------------------------------------------------|
| `ITemplateInterpreterContext`                 | Implement to control output destination, intercept appends, handle errors, or add pre/post hooks. |
| `IBlockContent`                               | Implement to create custom interpolation objects that evaluate to strings at render time.         |
| `TryHandleUnhandledInterpolation`             | Override in your context to support custom types in interpolation holes.                          |
| `OnPreAppendLiteral` / `OnPreAppendFormatted` | Override to transform content before it is appended (e.g., escaping).                             |

### Threading Model

`WaffleSyntax` uses `[ThreadStatic]` fields (`s_lastUsedId`) for ID generation. The
`TemplateInterpreter` struct is designed for single-threaded use within one template
rendering call. Multiple templates may be rendered concurrently on separate threads,
each with its own `ITemplateInterpreterContext` instance.

### Performance Considerations

- `TemplateInterpreter` is a **struct** to avoid heap allocation on every render call.
- `Token` objects are **pooled** via a thread-static stack to minimize GC pressure.
- `EnvValue` stores `int` values inline (no boxing) for loop counters.
- Collection helpers (`ListPool`, `StackPool`, `StringBuilderPool`, `DictionaryPool`) rent and
  return buffers to avoid repeated allocation.
- The library targets **netstandard2.0** with polyfills, keeping it compatible with
Incremental Source Generators, Unity, and older runtimes while using C# 14 language features
  via the compiler.

## Using Waffle.Core inside a Source Generator project

When you reference Waffle.Core from your own Incremental Source Generator project, consumers of that generator
may fail to build because the Roslyn compilation context does not automatically resolve the transitive
`Waffle.Core.dll` dependency at generator-execution time.

To fix this, add the following to your generator's `.csproj` so that `Waffle.Core.dll` is explicitly declared
as a generator dependency:

```xml
<ItemGroup>
    <PackageReference Include="Waffle.Core" Version="1.x" GeneratePathProperty="true"/>
</ItemGroup>
<PropertyGroup>
    <GetTargetPathDependsOn>$(GetTargetPathDependsOn);GetDependencyTargetPaths</GetTargetPathDependsOn>
</PropertyGroup>
<Target Name="GetDependencyTargetPaths">
    <TargetPathWithTargetPlatformMoniker
        Include="$(PkgWaffle_Core)\lib\netstandard2.0\Waffle.Core.dll"
        IncludeRuntimeDependency="false"/>
</Target>
```

This applies to any project that uses Waffle.Core as a generator-side dependency,
regardless of whether [Waffle.ModelProxy](../Waffle.ModelProxy) is also in use.

## Dependencies

No external dependencies.
