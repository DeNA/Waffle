# Waffle.ModelProxy

Waffle.ModelProxy is a C# Incremental Source Generator that eliminates the boilerplate of `.To(it => it.Member)`
when accessing model properties inside Waffle's loop variables.

See [README.md](../../README.md) for a general overview of Waffle.

---

## The Problem It Solves

In a plain Waffle template, accessing a member of a loop variable requires a `.To()` lambda at every use:

```csharp
// Without ModelProxy — verbose
Render($$"""
    {{ForEach(properties, out var p, out _, out var h)}}
        public readonly {{p.To(it => it.Type)}} {{p.To(it => it.Name)}};
    {{End}}
    """);
```

With ModelProxy, the generator produces a proxy wrapper that exposes each member directly as a Waffle proxy,
so the template reads like ordinary C# property access:

```csharp
// With ModelProxy — clean
Render($$"""
    {{ForEach(properties.AsProxy(), out var p, out _, out var h)}}
        public readonly {{p.Type}} {{p.Name}};
    {{End}}
    """);
```

---

## Setup

Waffle.ModelProxy is consumed as a source generator, not a regular library reference.
The `.csproj` entry differs from the usual `PackageReference`:

```xml

<ItemGroup>
    <PackageReference Include="Waffle.Core" Version="1.x"/>
    <PackageReference Include="Waffle.ModelProxy" Version="1.x">
        <ReferenceOutputAssembly>false</ReferenceOutputAssembly>
        <OutputItemType>Analyzer</OutputItemType>
    </PackageReference>
</ItemGroup>
```

### Referencing from another Source Generator project

If your project is itself an Incremental Source Generator and references Waffle.Core,
additional setup is required to make `Waffle.Core.dll` visible to the Roslyn compilation context.
See [Waffle.Core — Using Waffle.Core inside a Source Generator project](../Waffle.Core/README.md#using-wafflecore-inside-a-source-generator-project)
for details.

---

## Basic Usage

### 1. Annotate your model types with `[ModelProxy]`

Apply `[ModelProxy]` to any `class`, `struct`, or `interface` whose instances will be used as loop elements in
`ForEach` or `ForEachNullable`.

```csharp
[ModelProxy]
public readonly record struct StructModel(string Name, PropertyModel[] Properties);

[ModelProxy]
public readonly record struct PropertyModel(string Type, string Name, string PrivateName);
```

The generator produces an in-memory `{TypeName}Proxy` wrapper and a `{TypeName}ProxyList` wrapper for each annotated
type.

### 2. Convert a model instance to its proxy with `.AsProxy()`

`.AsProxy()` extension methods are generated for:

| Input type         | Resulting proxy type |
|--------------------|----------------------|
| `T`                | `TProxy`             |
| `IResolvableTo<T>` | `TProxy`             |
| `IReadOnlyList<T>` | `TProxyList`         |
| `IEnumerable<T>`   | `TProxyList`         |

```csharp
var model = new StructModel("ReadOnlyIntVector3", new[]
{
    new PropertyModel("int", "X", "x"),
    new PropertyModel("int", "Y", "y"),
    new PropertyModel("int", "Z", "z"),
}).AsProxy();
```

### 3. Access proxy members in templates

Proxy properties and fields are accessible with the same `.MemberName` syntax as ordinary C# objects.

```csharp
Console.WriteLine(Render($$"""
    public readonly partial struct {{model.Name}}
    {
    {{ForEach(model.Properties, out var p)}}
        public readonly {{p.Type}} {{p.Name}};
    {{End}}
    {{If(model.Properties.Count > 0)}}
        public {{model.Name}}(
        {{ForEach(model.Properties, out p, out _, out var h)}}
            {{p.Type}} {{p.PrivateName}}{{h.CommaOrLastEmpty}}
        {{End}}
        ) {
        {{ForEach(model.Properties, out p)}}
            this.{{p.Name}} = {{p.PrivateName}};
        {{End}}
        }
    {{End}}
    }

    """));
```

Output:

```text
public readonly partial struct ReadOnlyIntVector3
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;

    public ReadOnlyIntVector3(
        int x,
        int y,
        int z
    ) {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }
}
```

---

## What the Generator Produces

### Member types and their proxy representations

The generator inspects **public and internal** instance members and maps each type to the corresponding Waffle proxy:

| Original member type                                                                     | Proxy accessor type             |
|------------------------------------------------------------------------------------------|---------------------------------|
| `string`                                                                                 | `StringProxy`                   |
| `int`                                                                                    | `IntProxy`                      |
| `bool`                                                                                   | `BoolProxy`                     |
| Any other type `T`                                                                       | `IResolvableTo<T>`              |
| `T[]` / `List<T>` / `IReadOnlyList<T>` / `IList<T>` / any `IReadOnlyList<T>` implementor | `ListProxy<T>`                  |
| Any `IEnumerable<T>` implementor (except for `string` )                                  | `ListProxy<T>`                  |
| List with nullable ref elements (e.g. `string?[]`)                                       | `NullableRefListProxy<T>`       |
| `[ModelProxy]`-annotated type `T`                                                        | `TProxy`                        |
| List of `[ModelProxy]`-annotated type `T`                                                | `TProxyList`                    |
| C# value tuple (e.g. `(string A, T B)`)                                                  | Auto-generated `TupleProxy`     |
| List of value tuples                                                                     | Auto-generated `TupleProxyList` |

See [Limitations](#limitations) for unsupported member types.

### Nullable members and `Has{Name}` accessors

For any member declared as a nullable type (`T?`, `string?`, `int?`, etc.), the generator additionally emits a
`Has{Name}` accessor of type `BoolProxy` that evaluates to `true` when the value is non-null:

```csharp
[ModelProxy]
public class Config(string? description)
{
    public string? Description { get; } = description;
}

// Usage in template:
var c = new Config("optional text").AsProxy();

Render($$"""
    {{If(c.HasDescription)}}
    Description: {{c.Description}}
    {{End}}
    """);
```

For a nullable collection (`T[]?`, `List<T>?`), iterating over the proxy property with `ForEach` when the collection
is `null` produces **zero iterations** without throwing a `NullReferenceException`.

### Methods

Parameterless public/internal instance methods with a non-`void` return type are exposed with the same call syntax:

```csharp
[ModelProxy]
public class Item(string label, int score)
{
    public string GetLabel() => label;
    public int GetScore() => score;
    public bool IsHighScore() => score >= 90;
}

var item = new Item("Waffle", 95).AsProxy();

Render($$"""label={{item.GetLabel()}}, high={{If(item.IsHighScore())}}yes{{End}}""");
```

The return type follows the same proxy-mapping table above. Result values are **cached per proxy instance**
so each parameterless method is invoked at most once during rendering.

### Methods with parameters (up to 4)

Methods with up to 4 parameters are exposed as proxy methods that accept either the raw value type or `IResolvableTo<T>`
for each parameter:

```csharp
[ModelProxy]
public class Formatter(string label)
{
    public string WithPrefix(string prefix) => prefix + label;
    public bool IsMatch(string pattern) => label.Contains(pattern);
}

var f = new Formatter("World").AsProxy();

Render($$"""
    {{f.WithPrefix("Hello-" /* passes string directly */)}}
    {{If(f.IsMatch("orl"))}}match{{End}}
    """);
```

When a loop variable or another proxy instance is the argument, you can pass it as an `IResolvableTo<T>`:

```csharp
Render($$"""
    {{ForEach(items.AsProxy(), out var item)}}
    {{item.WithPrefix(item.AnyStringMember /* passes IResolvableTo<string> */)}}
    {{End}}
    """);
```

For a nullable-returning parameterized method `T? Foo(...)`, a corresponding `HasFoo(...)` accessor is also generated:

```csharp
{{If(proxy.HasGetSomething(0))}}found{{End}}
```

### Inheritance

The proxy for a derived type **includes all inherited members** from its base classes (up to but not including
`object`).
You do not need to annotate the base class separately.

```csharp
public class Base { public string BaseValue { get; } = "base"; }

[ModelProxy]
public class Derived(string own) : Base
{
    public string OwnValue { get; } = own;
}

var d = new Derived("own").AsProxy();
// Both d.BaseValue and d.OwnValue are available.
```

When a derived type overrides a base member, the member appears once in the proxy; virtual dispatch ensures the
overriding implementation is called. If the derived type instead hides a base member with `new`, only the derived (
hiding) version is exposed.

### Nested types

When a `[ModelProxy]` type is nested inside another type, the generated proxy mirrors the nesting.
For `Outer.Inner`, the proxies are `OuterProxy.InnerProxy` and `OuterProxy.InnerProxyList`.

### `ToString()`

Every proxy exposes a `ToString()` method that returns a `StringProxy` wrapping the original object's `ToString()`
result. This shadows `object.ToString()` and makes `ToString()` available directly in template interpolation.

---

## Limitations

The following are **not** supported by the generator:

| Scenario                                                              | Reason / Workaround                                                                                                                                                                                                                                                                                                               |
|-----------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Methods with more than 4 parameters                                   | Not generated. Use `.To()` / `.With()` in the template directly.                                                                                                                                                                                                                                                                  |
| Methods with `ref`, `out`, or `params` parameters                     | Not generated.                                                                                                                                                                                                                                                                                                                    |
| Generic methods                                                       | Not generated.                                                                                                                                                                                                                                                                                                                    |
| `void` methods                                                        | Not applicable (no value to embed in a template).                                                                                                                                                                                                                                                                                 |
| `protected` / `private` members                                       | Not exposed (only `public` and `internal` members are processed).                                                                                                                                                                                                                                                                 |
| Static members                                                        | Not exposed.                                                                                                                                                                                                                                                                                                                      |
| Indexers                                                              | Not exposed.                                                                                                                                                                                                                                                                                                                      |
| `[ModelProxy]` on a base class used as a proxy for a derived instance | The proxy is generated for exactly the annotated type. A `BaseProxy` cannot accept a `Derived` instance via `.AsProxy()`.                                                                                                                                                                                                         |
| Thread safety                                                         | A single proxy instance must not be accessed from multiple threads simultaneously — the instance-level lazy-init caches (`??=`) are not thread-safe. However, parallel rendering is fully supported: create a separate proxy instance per thread (or per render call) and each thread can render independently without any issue. |
