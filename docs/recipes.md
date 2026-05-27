# Recipes

Practical patterns for common code-generation tasks with Waffle.
These examples assume `using static Waffle.WaffleSyntax;`.

---

## Comma-Separated Parameter Lists

One of the most frequent patterns in code generation: rendering a method signature with
comma-separated arguments that collapses to empty parentheses when the list is empty.

### Single-line parameters

The simplest approach uses `.Lazy().Join()`:

```csharp
var args = new[] { "int x", "string name", "bool flag" };

Console.WriteLine(Render($$"""
void DoSomething({{args.Lazy().Join(", ")}})
"""));
```

```text
void DoSomething(int x, string name, bool flag)
```

When `args` is empty: `void DoSomething()`

Alternatively, using inline `ForEach` with `IndexedLoopHelper`:

```csharp
Console.WriteLine(Render($$"""
void DoSomething({{ForEach(args, out var arg, out var i, out var h)}}{{arg}}{{h.CommaSpaceOrLastParen}}{{End}}
"""));
```

Same output. 
The `ForEach` approach is verbose and harder to read, but both versions are shown intentionally because it forms the foundation of the patterns explained in the following sections.

**How it works:** The opening `(` is a literal before `ForEach`. With zero iterations,
nothing is produced between `(` and `)`. With N iterations, each element and its
trailing separator are rendered inline.

### Multi-line parameters (closing paren on last line)

For longer parameter lists where each argument goes on its own line with a closing `)` at
the end of the last argument:

```csharp
var args = new[] { "int x", "string name", "bool flag" };

Console.WriteLine(Render($$"""
void DoSomething({{ForEach(args, out var arg, out var i, out var h)}}
    {{arg}}{{h.CommaOrLastEmpty}}{{End}})
"""));
```

```text
void DoSomething(
    int x,
    string name,
    bool flag)
```

When `args` is empty: `void DoSomething()`

**How it works:**

1. `ForEach` is on the same line as `(`, so the line is **not** command-only — the newline
   after `ForEach` becomes the literal `"\n    "` at the start of each iteration body.
2. `End` is on the same line as `)`, so the line is also not command-only.
3. The loop body is: `"\n    "` + arg + commaOrEmpty. There is no trailing literal in the
   body (`End` immediately follows `CommaOrLastEmpty`).
4. When N=0, no iterations run, and the output is just `(` + `)`.
5. When N>0, each iteration contributes `\n    argX,` (or `\n    argN` for the last), then `)` follows.

### Multi-line parameters (closing paren on its own line)

If you prefer the closing `)` on a separate line:

```csharp
var args = new[] { "int x", "string name", "bool flag" };

Console.WriteLine(Render($$"""
void DoSomething(
{{ForEach(args, out var arg, out var i, out var h)}}
    {{arg}}{{h.CommaOrLastEmpty}}
{{End}}
)
"""));
```

```text
void DoSomething(
    int x,
    string name,
    bool flag
)
```

Here `ForEach` and `End` are on their own command-only lines (automatically removed),
giving a cleaner template layout. The trade-off is that when `args` is empty, the newline
after `(` remains in the output:

```text
void DoSomething(
)
```

If you want true `()` for empty lists, wrap with an `If` branch:

```csharp
Console.WriteLine(Render($$"""
{{If(args.Length > 0)}}
void DoSomething(
{{ForEach(args, out var arg, out var i, out var h)}}
    {{arg}}{{h.CommaOrLastEmpty}}
{{End}}
)
{{Else}}
void DoSomething()
{{End}}
"""));
```


```text
void DoSomething(
    int x,
    string name,
    bool flag
)

```

```text
void DoSomething()

```

### Generic type parameters (empty = omit entirely)

Use `Join(separator, prefix, suffix)` overload to render bracket-delimited lists that should disappear
entirely when empty:

```csharp
var typeParams = new[] { "T", "U" };

Console.WriteLine(Render($$"""
class MyClass{{typeParams.Lazy().Join(", ", "<", ">")}}
"""));
```

```text
class MyClass<T, U>
```

When `typeParams` is empty: `class MyClass`

**How it works:** `Join(separator, prefix, suffix)` returns an empty string when the list
is empty, so nothing is emitted between `MyClass` and the newline. When non-empty it wraps
the joined elements with the given prefix and suffix.

Alternatively, using the inline `ForEach` + `If` pattern:

```csharp
Console.WriteLine(Render($$"""
class MyClass{{If(typeParams.Length > 0)}}<{{ForEach(typeParams, out var t, out var i, out var h)}}{{t}}{{h.CommaSpaceOrLastEmpty}}{{End}}>{{End}}
"""));
```

---

## Conditional Wrapping

Wrap output in a surrounding structure only when a condition is met:

```csharp
var needsNullable = true;

Console.WriteLine(Render($$"""
{{If(needsNullable)}}
#nullable enable
{{End}}
public class Foo { }
{{If(needsNullable)}}
#nullable restore
{{End}}
"""));
```

```text
#nullable enable
public class Foo { }
#nullable restore

```

When `needsNullable` is false:

```text
public class Foo { }

```

---

## Indented Code Blocks with Auto Indentation

When a multi-line value is interpolated at an indented position, Waffle automatically
indents subsequent lines to match. Use this to compose nested structures:

```csharp
var fields = new[] { "int X", "int Y", "int Z" };

var fieldDeclarations = Render($$"""
{{ForEach(fields, out var f)}}
public {{f}} { get; set; }
{{End}}
""");

Console.WriteLine(Render($$"""
public class Vector
{
    {{fieldDeclarations}}
}
"""));
```

```text
public class Vector
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    
}
```

The `fieldDeclarations` string contains newlines, and auto indentation adds the 4 leading
spaces to lines 2+ automatically.

Notice that the final output has an extra indented blank line next to the last property.
This is because the `fieldDeclarations`ends with a newline and the last empty line also be indented.
To avoid this, please trim the trailing newline from `fieldDeclarations` before interpolation.

---

## Switch/Enum Generation

Generate switch arms or enum members from a list:

```csharp
var members = new[] { ("None", 0), ("Read", 1), ("Write", 2), ("Execute", 4) };

Console.WriteLine(Render($$"""
[Flags]
public enum Permission
{
{{ForEach(members, out var m, out var i, out var h)}}
    {{m.To(x => x.Item1)}} = {{m.To(x => x.Item2)}}{{h.LastOrNot("", ",")}}
{{End}}
}
"""));
```

```text
[Flags]
public enum Permission
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4
}
```

---

## Builder-Style Fluent Chains

Generate method chains where each call is on its own line:

```csharp
var options = new[] { ".UseRouting()", ".UseAuthentication()", ".UseAuthorization()" };

Console.WriteLine(Render($$"""
var app = builder.Build(){{ForEach(options, out var opt)}}
    {{opt}}{{End}};
"""));
```

```text
var app = builder.Build()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization();
```

When `options` is empty:

```text
var app = builder.Build();
```

**How it works:** Same pattern as multi-line parameters — `ForEach` is on a non-command-only
line, so the newline after it becomes part of the loop body's leading literal.

---

## Multi-Line String with Prefix on First Line

A frequent pattern in code generation: the first item must appear on the same line as a
preceding delimiter (e.g., `if (`), while subsequent items each get their own indented line.
Use the `>|` format specifier on the loop command:

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

Single condition: `if (x > 0)` — the `>|` trim leaves nothing before the single item, and
`<<` on `End` removes the trailing whitespace so `)` follows directly.

Empty list: `if ()`.

**How it works:**

1. The loop body starts with `"\n    "` (the newline + indent written after `ForEach`).
2. `>|` trims that leading `"\n    "` only on the **first** iteration, so the first item
   runs directly after `(`.
3. From the second iteration onward the `"\n    "` is kept, so each item gets its own line.
4. `<<` on `End` removes the `"\n    "` that would otherwise precede it, so `)` attaches
   directly to the last item.

Alternatively, without `>|`, you can embed the separator and newline explicitly in the
format string — but the template is harder to read because the line structure is invisible:

```csharp
Console.WriteLine(Render($$"""
if ({{ForEach(conditions, out var c, out var i, out var h)}}{{c}}{{h.LastOrNot("", " &&\n    ")}}{{End}})
{
}
"""));
```

Same output, but the newline and indent are hidden inside the string literal.

---

## Using `<<` Trim to Collapse Trailing Whitespace in Loops

The `<<` format specifier on `End` removes the preceding literal (including newlines).
This is useful when you want the text **after** `End` to appear on the same line as the
last iteration's content, but each iteration still needs a newline between them:

```csharp
var items = new[] { "first", "second", "third" };

Console.WriteLine(Render($$"""
items: [{{ForEach(items, out var item, out var i, out var h)}}
    {{item}}{{h.CommaOrLastEmpty}}
    {{End:<<}}]
"""));
```

```text
items: [
    first,
    second,
    third]
```

Compare with another approach (putting `End` on the same line as the content):

```csharp
Console.WriteLine(Render($$"""
items: [{{ForEach(items, out var item, out var i, out var h)}}
    {{item}}{{h.CommaOrLastEmpty}}{{End}}]
"""));
```

```text
items: [
    first,
    second,
    third]
```

Both produce the same result.

---

## Key Principle: Inline Placement Controls Line Behavior

The underlying principle behind many of these patterns:

- **Command on its own line** → the entire line (including surrounding whitespace) is removed
  from output (command-only line trimming).
- **Command on a line with other content** → the line is preserved. Any newlines adjacent to
  the command become part of the iteration body or conditional block.

By deliberately placing `ForEach` on the same line as a preceding delimiter (`(`, `[`, `<`)
and `End` on the same line as a following delimiter (`)`, `]`, `>`), you create a pattern
where:

1. Zero iterations naturally collapse (delimiter literals sit next to each other).
2. N iterations produce content between the delimiters with proper line breaks.

This is the foundational technique for most delimiter-wrapping patterns in Waffle templates.
