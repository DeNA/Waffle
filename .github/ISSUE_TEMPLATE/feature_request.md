---
name: Feature Request
about: Propose a new feature or enhancement for Waffle
labels: enhancement
---

## Summary

<!-- A concise description of the feature you are proposing. -->

## Motivation

<!-- Why is this feature needed? What problem does it solve or what use case does it enable? -->

## Proposed API / Design

<!-- If applicable, sketch what the new API or syntax would look like. For example: -->

```csharp
// Example usage of the proposed feature
```

## Checklist Against Waffle's Design Criteria

Before submitting, please verify that the proposal meets Waffle's evaluation criteria:

- [ ] **Stays within the "flow control complement" scope** — The feature is a natural extension of flow control or
  variable binding within interpolated strings, or an extension method on `IResolvableTo<T>` /
  `IIterationSource<TIterator, TOriginal>` that enhances composability without widening the core language surface.
- [ ] **Zero or near-zero cost when unused** — The feature does not degrade the performance of existing templates that
  do not use it.
- [ ] **Expressible as ordinary C#** — The feature composes naturally with the language and preserves full IDE support
  (completion, refactoring, compile-time checking). It does not require external parsing, reflection, or runtime code
  generation.

## Additional Context

<!-- Any other context, references, or alternatives you have considered. -->
