# Contributing to Waffle

Thanks for taking the time to contribute! 🧇

If you find a bug or have a feature request, please [open an issue](https://github.com/DeNA/Waffle/issues)
first so it can be discussed before any code is written.

### Bug Reports

If you encounter a bug, please open an issue with:

- A minimal reproducible example
- Expected vs actual behavior
- Your .NET SDK version and runtime environment

### Feature Requests

Waffle's design philosophy is **"flow control complement for interpolated strings"** — it provides the minimal set of
primitives (`For`, `ForEach`, `If`, `Let`, etc.) needed to express loops and conditionals inside C# string literals.
Feature requests are evaluated against the following criteria:

1. **Stays within the "flow control complement" scope** — The proposed feature must be a natural extension of flow
   control or variable binding within interpolated strings. Waffle intentionally does not aim to be a general-purpose
   template engine with rich built-in filters, query operators, or external DSL syntax. However, extension methods on
   `IResolvableTo<T>` or `IIterationSource<TIterator, TOriginal>` that provide sufficiently general value-formatting or
   pipeline operations (e.g., joining, conditional transformation) are considered in scope — these enhance the
   composability of existing primitives without widening the core language surface.

2. **Zero or near-zero cost when unused** — Waffle is designed for performance-critical contexts such as Incremental
   Source Generators. Any new feature must not degrade the performance of existing templates that do not use it.

3. **Expressible as ordinary C#** — Templates are plain C# code. New primitives should compose naturally with the
   language and preserve full IDE support (completion, refactoring, compile-time checking). Features that require
   external parsing, reflection, or runtime code generation are out of scope.

If your request does not meet these criteria, consider whether a user-land extension or a separate library would be a
better fit.

### Pull Requests

Pull requests are also appreciated but you must understand, accept and agree to be bound by the terms and conditions of
the [Contribution License Agreement](https://dena.github.io/cla/).

To send a pull request:

- Fork the repository and create a feature branch from `main`.
- Ensure all existing tests pass: `dotnet test -c Debug`.
- Add tests covering any new behavior.

