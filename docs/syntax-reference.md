# Syntax Reference

Waffle provides template commands as methods and properties on the static class `WaffleSyntax`.
Use them with `using static Waffle.WaffleSyntax;`.

## Summary

| Syntax                                                                                | Description                                      |
|---------------------------------------------------------------------------------------|--------------------------------------------------|
| [`For(start, end, out var i)`](#for--end)                                             | Ascending integer loop                           |
| [`For(start, end, out var i, out var h)`](#for-with-indexedloophelper)                | Ascending integer loop with iteration helper     |
| [`Forr(start, end, out var i)`](#forr--end)                                           | Descending integer loop                          |
| [`Forr(start, end, out var i, out var h)`](#forr-with-indexedloophelper)              | Descending integer loop with iteration helper    |
| [`ForEach(source, out var e)`](#foreach--end)                                         | Collection loop                                  |
| [`ForEach(source, out var e, out var i)`](#foreach-with-index)                        | Collection loop with index                       |
| [`ForEach(source, out var e, out var i, out var h)`](#foreach-with-indexedloophelper) | Collection loop with index and iteration helper  |
| [`If` / `Elif` / `Else`](#if--elif--else--end)                                        | Conditional branching                            |
| [`Cond(condition, ifTrue, ifFalse)`](#cond)                                           | Inline conditional expression                    |
| [`Break` / `Continue`](#break--continue)                                              | Loop control                                     |
| [`Let(out var x, expr)`](#let)                                                        | Variable binding                                 |
| [`Note("...")`](#note)                                                                | Comment (no output)                              |
| [`Render(...)` / `RenderCSharp(...)`](#render)                                        | Template rendering                               |
| [Command-only line trimming](#command-only-line-trimming)                             | Automatic removal of control-only lines          |
| [Whitespace control](#whitespace-control)                                             | Trimming whitespace around interpolations        |
| [Auto indentation](#auto-indentation)                                                 | Automatic indent insertion for multi-line values |
| [Lazy-evaluated objects](#lazy-evaluated-objects)                                     | `.To()`, `.Of()`, operators on loop variables    |

---

## For – End

Repeats the block for ascending integers in the given range.
The second argument is **exclusive** (like C#'s `for (var i = start; i < end; i++)`).

```csharp
Console.WriteLine(Render($$"""
{{For(0, 3, out var i)}}
Line {{i}}
{{End}}
"""));
```

```text
Line 0
Line 1
Line 2

```

The bounds may also be `IResolvableTo<int>` (i.e. lazily-resolved values from an outer loop).

### For with IndexedLoopHelper

An additional `out IndexedLoopHelper` parameter provides convenient first/last detection:

```csharp
Console.WriteLine(Render($$"""
({{For(0, 3, out var i, out var h)}}{{i}}{{h.LastOrNot(")", ",")}}{{End}}
"""));
```

```text
(0,1,2)
```

See [IndexedLoopHelper](#indexedloophelper) for the full API.

---

## Forr – End

Repeats the block for descending integers in the given range.
Both arguments are **inclusive** (like C#'s `for (var i = start; i >= end; i--)`).

```csharp
Console.WriteLine(Render($$"""
{{Forr(3, 0, out var i)}}
Line {{i}}
{{End}}
"""));
```

```text
Line 3
Line 2
Line 1
Line 0

```

### Forr with IndexedLoopHelper

```csharp
Console.WriteLine(Render($$"""
({{Forr(3, 1, out var i, out var h)}}{{i}}{{h.LastOrNot(")", ",")}}{{End}}
"""));
```

```text
(3,2,1)
```

---

## ForEach – End

Repeats the block for each element of a collection.
Each element is exposed as an `IResolvableTo<T>` via the `out` parameter.

```csharp
Console.WriteLine(Render($$"""
{{ForEach(new[] { "Alice", "Bob", "Charlie" }, out var name)}}
Hello, {{name}}!
{{End}}
"""));
```

```text
Hello, Alice!
Hello, Bob!
Hello, Charlie!

```

The source can be:

- `IEnumerable<T>` (evaluated eagerly)
- `IResolvableTo<IEnumerable<T>>` (lazily-resolved, for nested loops)
- `IIterationSource<TModel, T>` (typed iteration source from ModelProxy)

### ForEach with index

Add an `out IntProxy i` parameter to get the zero-based index:

```csharp
Console.WriteLine(Render($$"""
{{ForEach(new[] { "a", "b", "c" }, out var elem, out var i)}}
[{{i}}] {{elem}}
{{End}}
"""));
```

```text
[0] a
[1] b
[2] c

```

### ForEach with IndexedLoopHelper

Add an `out IndexedLoopHelper h` parameter for first/last detection:

```csharp
Console.WriteLine(Render($$"""
({{ForEach(new[] { "x", "y", "z" }, out var elem, out var i, out var h)}}{{elem}}{{h.LastOrNot(")", ", ")}}{{End}}
"""));
```

```text
(x, y, z)
```

---

## IndexedLoopHelper

`IndexedLoopHelper` is available as an `out` parameter on `For`, `Forr`, and `ForEach`.
It provides methods for branching on whether the current iteration is the first or last:

| Method / Property               | Description                                                  |
|---------------------------------|--------------------------------------------------------------|
| `h.FirstOrNot(first, notFirst)` | Returns `first` on the first iteration, `notFirst` otherwise |
| `h.LastOrNot(last, notLast)`    | Returns `last` on the last iteration, `notLast` otherwise    |
| `h.CommaOrLastParen`            | `","` or `")"` on last                                       |
| `h.CommaSpaceOrLastParen`       | `", "` or `")"` on last                                      |
| `h.CommaOrLastEmpty`            | `","` or `""` on last                                        |
| `h.CommaSpaceOrLastEmpty`       | `", "` or `""` on last                                       |

A common use case is generating comma-separated lists:

```csharp
Console.WriteLine(Render($$"""
void Foo({{For(0, 3, out var i, out var h)}}int arg{{i}}{{h.CommaSpaceOrLastParen}}{{End}}
"""));
```

```text
void Foo(int arg0, int arg1, int arg2)
```

See [Recipes](recipes.md) for more advanced patterns including multi-line parameter lists
that collapse to `()` when empty.

---

## If – Elif – Else – End

Conditionally outputs a block. Supports chained `Elif` and an optional `Else`.

```csharp
Console.WriteLine(Render($$"""
{{For(1, 16, out var i)}}
    {{If(i % 15 == 0)}}
FizzBuzz
    {{Elif(i % 3 == 0)}}
Fizz
    {{Elif(i % 5 == 0)}}
Buzz
    {{Else}}
{{i}}
    {{End}}
{{End}}
"""));
```

```text
1
2
Fizz
4
Buzz
Fizz
7
8
Fizz
Buzz
11
Fizz
13
14
FizzBuzz

```

Conditions can be:

- `bool` — evaluated immediately
- `IResolvableTo<bool>` / `BoolProxy` — lazily resolved (e.g., comparisons on loop variables)

---

## Cond

An inline conditional expression (like the ternary operator). Returns an `IResolvableTo<T>`.

```csharp
Console.WriteLine(Render($$"""
{{For(0, 4, out var i)}}
{{Cond(i % 2 == 0, "even", "odd")}}
{{End}}
"""));
```

```text
even
odd
even
odd

```

Overloads:

```csharp
Cond(IResolvableTo<T> subject, Predicate<T> predicate, U ifTrue, U ifFalse)
Cond(IResolvableTo<bool> condition, U ifTrue, U ifFalse)
Cond(IResolvableTo<bool> condition, IResolvableTo<U> ifTrue, IResolvableTo<U> ifFalse)
```

---

## Break / Continue

Usable only inside iteration blocks (`For`, `Forr`, `ForEach`).

- **`Break`** — terminates the innermost loop entirely.
- **`Continue`** — skips the rest of the current iteration and advances to the next element.

```csharp
Console.WriteLine(Render($$"""
{{For(0, 5, out var i)}}
    {{If(i == 3)}}{{Break}}{{End}}
{{i}}
{{End}}
"""));
```

```text
0
1
2

```

```csharp
Console.WriteLine(Render($$"""
{{For(0, 5, out var i)}}
    {{If(i == 2)}}{{Continue}}{{End}}
{{i}}
{{End}}
"""));
```

```text
0
1
3
4

```

---

## Let

Binds a value to a new variable. The arguments can be provided in either order:

```csharp
Let(out var x, expr)   // out-first
Let(expr, out var x)   // value-first (equivalent)
```

This is useful for creating derived values from loop variables:

```csharp
Console.WriteLine(Render($$"""
{{For(0, 3, out var x)}}
    {{Let(out var y, x + 10)}}
x={{x}}, y={{y}}
{{End}}
"""));
```

```text
x=0, y=10
x=1, y=11
x=2, y=12

```

`Let` itself produces no output.

---

## Note

Takes any value and produces no output. Useful for inline comments:

```csharp
Console.WriteLine(Render($$"""
{{Note("Generate vector types")}}
{{For(2, 4, out var i)}}
Vector{{i}}
{{End}}
"""));
```

```text
Vector2
Vector3

```

---

## Render

`Render` evaluates a Waffle template. There are two usage patterns:

### Standalone (returns a string)

When you just need the rendered string directly:

```csharp
string result = Render($"Hello, {name}!");
string result = Render($$"""
    {{For(0, 3, out var i)}}
    Line {{i}}
    {{End}}
    """);
```

### With context (appends to an `ITemplateInterpreterContext`)

When used within the Bakery framework or a custom context, pass the context as the first argument:

```csharp
// Inside a template's ProcessImpl:
Render(ctx, $$"""
    {{For(0, 3, out var i)}}
    Line {{i}}
    {{End}}
    """);
```

The context receives the output via its `Append` method and can intercept the rendering process
through lifecycle hooks.

### RenderCSharp

`RenderCSharp` is functionally identical to `Render`, but annotates the template string with
`[StringSyntax("C#")]` so that IDEs provide C# syntax highlighting inside the template literal.

```csharp
RenderCSharp(ctx, $$"""
    {{For(0, 3, out var i)}}
    public int Field{{i}} { get; set; }
    {{End}}
    """);
```

The same effect may be achieved by annotating the containing method with a comment like `// lang=cs` for IDEs that
support it (e.g., JetBrains Rider):

```csharp
// lang=cs
Render(ctx, $$"""
    {{For(0, 3, out var i)}}
    public int Field{{i}} { get; set; }
    {{End}}
    """);
```

Note: When using `// lang=proto` or other language specifiers with languages other than C#,
IDEs that support them will apply syntax highlighting.
However, since the template contains C# interpolation holes that don't match the specified
language syntax, some IDEs may flag these as errors, resulting in red squiggles or error messages
on the interpolated string.

---

## Command-Only Line Trimming

Lines that contain **only** command interpolations and whitespace are automatically removed
from the output. This lets you indent commands for readability without affecting the result.

```csharp
Console.WriteLine(Render($$"""
{{For(0, 3, out var x)}}
    {{Let(out var y, x + 10)}}
    {{If(x == 2)}}{{Break}}{{End}}
x+y={{x + y}}
{{End}}
"""));
```

```text
x+y=10
x+y=12

```

In this example, the leading whitespace on the `Let` and `If` lines is completely removed.

**Note:** When multiple command interpolations appear on the same line, the line is only removed
if all interpolations are adjacent (no content including whitespace between them) and there is no other content on the
line.

**Independent evaluation:** Each line is evaluated independently. An `End` on a command-only
line is always removed, regardless of whether its paired block-start was on a command-only line.

This means placing a block-start command on a line with content (making it non-command-only)
does not prevent the `End` line from being removed:

```csharp
Console.WriteLine(Render($$"""
    {{For(0, 3, out var x)}}{{x}}
    {{End}}
"""));
```

```text
    1
2
3

```

Here the `For` line has non-command content `{{x}}`, so its whitespace is preserved — the
`"    "` before `For` is output once. But the `End` line is independently command-only, so
it **is** removed. Each iteration produces `"{x}\n"` (the `\n` from the trimmed literal
before `End`), resulting in the values without leading indentation on lines 2+.

> **Tip:** By placing loop commands on non-command-only lines (alongside delimiters like `(`
> or `)`), you can create patterns where zero iterations naturally collapse to empty output.
> See [Recipes](recipes.md) for practical examples such as comma-separated parameter lists.

---

## Whitespace Control

Format specifiers on interpolation holes control whitespace trimming around them:

| Specifier | Effect                                                                                                                |
|-----------|-----------------------------------------------------------------------------------------------------------------------|
| `<`       | Removes preceding whitespace on the same line (spaces/tabs only)                                                      |
| `>`       | Removes following whitespace on the same line (spaces/tabs only)                                                      |
| `<<`      | Removes preceding whitespace including newlines                                                                       |
| `>>`      | Removes following whitespace including newlines                                                                       |
| `>\|`     | For iteration commands only. Removes leading whitespace (including newlines) of its block on **first iteration only** |

These can be combined: `<>`, `<<>>`, `<>>`, etc., except for `>|` and `>` / `>>` pairs.
Order does not matter.
They can coexist with standard format specifiers (e.g., `{v:D5<>}`).

Trimming never crosses another interpolation boundary — it only affects
the adjacent literal string tokens.

### `<` — Remove preceding whitespace on the same line

```csharp
var label = "total";
Console.WriteLine(Render($$"""
    {{label:<}} = 42;
"""));
```

```text
total = 42;
```

### `>` — Remove following whitespace on the same line

```csharp
var tag = "b";
Console.WriteLine(Render($$"""<{{tag:>}}   >bold</{{tag}}>"""));
```

```text
<b>bold</b>
```

### `<>` — Remove whitespace on both sides (same line)

```csharp
var op = "+";
Console.WriteLine(Render($$"""
a    {{op:<>}}    b;
"""));
```

```text
a+b;
```

### `<<` — Remove preceding whitespace including newlines

```csharp
var value = "42";
Console.WriteLine(Render($$"""
result =

    {{value:<<}};
"""));
```

```text
result =42;
```

### `>>` — Remove following whitespace including newlines

```csharp
var value = "42";
Console.WriteLine(Render($$"""
result = {{value:>>}}

    ;
"""));
```

```text
result = 42;
```

Useful for joining loop iterations without line breaks:

```csharp
Console.WriteLine(Render($$"""
{{ForEach(new[] { "Alice", "Bob", "Charlie" }, out var name)}}
{{name:>>}}
{{End}}
"""));
```

```text
AliceBobCharlie
```

### `<<>>` — Remove whitespace including newlines on both sides

```csharp
var separator = "---";
Console.WriteLine(Render($$"""
header

    {{separator:<<>>}}

trailer
"""));
```

```text
header---trailer
```

### `>|` — Trim leading whitespace of loop block on first iteration only

When applied to a loop command (`ForEach`, `For`, `Forr`), `>|` removes the leading
whitespace (including any preceding newline) from the very first literal token of the
loop body, but **only on the first iteration**. All subsequent iterations are unaffected.

This is the key tool for writing templates where the first item should appear on the same
line as a preceding delimiter while subsequent items each start on their own indented line:

```csharp
var conditions = new[] { "x > 0", "y != null", "z.IsValid" };

Console.WriteLine(Render($$"""
if ({{ForEach(conditions, out var c, out _, out var h):>|}}
    {{c}}{{h.LastOrNot("", " &&")}}
    {{End:<<}})
{
}
"""));
```

```text
if (x > 0 &&
    y != null &&
    z.IsValid)
{
}
```

When the list has a single element the output is `if (x > 0)`.
When the list is empty the output is `if ()`.

`>|` is only meaningful on iteration commands; on non-loop interpolations it has no effect.

See [Recipes — Multi-Line String with Prefix on First Line](recipes.md#multi-line-string-with-prefix-on-first-line)
for a practical walkthrough.

---

## Auto Indentation

When a multi-line string is interpolated at a position where only whitespace precedes it
on the same line, that whitespace is automatically prepended to lines 2+ of the value.

```csharp
var inner = "line1\nline2\nline3";

Console.WriteLine(Render($$"""
class Foo
{
    {{inner}}
}
"""));
```

```text
class Foo
{
    line1
    line2
    line3
}
```

### Suppressing auto indentation with `v`

Use the `v` (verbatim) format specifier to disable auto indentation:

```csharp
var inner = "line1\nline2\nline3";

Console.WriteLine(Render($$"""
class Foo
{
    {{inner:v}}
}
"""));
```

```text
class Foo
{
    line1
line2
line3
}
```

`v` can be combined with other format specifiers (e.g., `{value:vD5}`).

When `<` or `<<` is combined with `v`, the leading whitespace is removed first, which
effectively means auto indentation would not apply anyway. `<` and `<v` are equivalent.

---

## Lazy-Evaluated Objects

Loop variables produced by `For`, `Forr`, and `ForEach` are **lazily-resolved** objects
(`IResolvableTo<T>`). Their actual values are only determined at evaluation time.
This means you cannot directly access members on them:

```csharp
// ❌ ERROR: elem is IResolvableTo<string>, not string
Console.WriteLine(Render($$"""
{{ForEach(new[] { "hello", "hi" }, out var elem)}}
{{elem}} has length {{elem.Length}}
{{End}}
"""));
```

### `.To()` — Project to a derived value

Use `.To()` to specify how to resolve to a derived value:

```csharp
Console.WriteLine(Render($$"""
{{ForEach(new[] { "hello", "hi" }, out var elem)}}
{{elem}} has length {{elem.To(e => e.Length)}}
{{End}}
"""));
```

```text
hello has length 5
hi has length 2

```

### `.Of()` — Index into a list

For indexing into a list using a lazily-resolved integer:

```csharp
var names = new[] { "Alice", "Bob", "Charlie" };

Console.WriteLine(Render($$"""
{{For(0, 3, out var i)}}
[{{i}}] {{i.Of(names)}}
{{End}}
"""));
```

```text
[0] Alice
[1] Bob
[2] Charlie

```

### `.With()` — Combine multiple resolvables

Combine two resolvable values and project them together:

```csharp
{{a.With(b, (x, y) => x + y)}}
```

### `.Extract()` — Flatten nested resolvables

Unwrap one level of `IResolvableTo<IResolvableTo<T>>` into `IResolvableTo<T>`.

### `.Replace()` — String replacement

For `IResolvableTo<string>`, replace all occurrences of a substring:

```csharp
{{name.Replace("_", "-")}}
```

### Operator overloads on `IntProxy`

Integer loop variables (`IntProxy`) support arithmetic and comparison operators directly:

| Operators                        | Result Type |
|----------------------------------|-------------|
| `+`, `-`, `*`, `/`, `%`          | `IntProxy`  |
| `<`, `>`, `<=`, `>=`, `==`, `!=` | `BoolProxy` |
| `++`, `--`, unary `+`, unary `-` | `IntProxy`  |

Both `IntProxy ○ IntProxy` and `IntProxy ○ int` / `int ○ IntProxy` are supported.

```csharp
Console.WriteLine(Render($$"""
{{For(0, 3, out var i)}}
    {{Let(out var doubled, i * 2)}}
i={{i}}, doubled={{doubled}}
{{End}}
"""));
```

```text
i=0, doubled=0
i=1, doubled=2
i=2, doubled=4

```

### Operator overloads on `BoolProxy`

| Operators             | Result Type |
|-----------------------|-------------|
| `!`                   | `BoolProxy` |
| `\|`, `&`, `==`, `!=` | `BoolProxy` |

### Operator overloads on `StringProxy`

| Operators  | Result Type   |
|------------|---------------|
| `+`        | `StringProxy` |
| `==`, `!=` | `BoolProxy`   |

---

## IIterationSource Extensions

When using `IIterationSource<TIterator, T>` (e.g., from Waffle.ModelProxy's `.AsProxy()`),
LINQ-like operations are available:

| Method                             | Description                                                       |
|------------------------------------|-------------------------------------------------------------------|
| `.Select(x => ...)`                | Project each element                                              |
| `.Where(x => ...)`                 | Filter elements                                                   |
| `.OrderBy(x => ...)`               | Sort elements in ascending order                                  |
| `.OrderByDescending(x => ...)`     | Sort elements in descending order                                 |
| `.Skip(n)`                         | Skip first N elements                                             |
| `.Take(n)`                         | Take first N elements                                             |
| `.Concat(other)`                   | Concatenate with another list                                     |
| `.Pick(0, 2, 4)`                   | Select elements at specific indexes                               |
| `.IndexOf(value)`                  | Find the index of a value                                         |
| `.Join(separator)`                 | Join elements into a string                                       |
| `.Join(separator, prefix, suffix)` | Join and wrap with prefix/suffix; empty string when list is empty |
| `.Count`                           | Number of elements (`IntProxy`)                                   |
| `.IsEmpty()`                       | Whether the list is empty (`BoolProxy`)                           |
| `.Any()`                           | Whether the list has elements (`BoolProxy`)                       |
| `.EmptyOrNot(ifEmpty, ifNot)`      | Branch on emptiness                                               |

You can also call `.AsProxy()` on any `IEnumerable<T>` or `IResolvableTo<IEnumerable<T>>` to create an
`IIterationSource` for use with `ForEach`.

