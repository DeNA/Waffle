# Waffle.Analyzer

A Roslyn analyzer for the [Waffle](https://github.com/DeNA/Waffle) template engine that detects template syntax errors
at compile time.

## Diagnostics

### Block structure

| ID     | Severity | Description                                                                        | Example                                            |
|--------|----------|------------------------------------------------------------------------------------|----------------------------------------------------|
| WAF001 | Error    | A `For`, `Forr`, `ForEach`, `ForEachNullable`, or `If` block has no matching `End` | `{{For(0, 3, out var i)}}` (without `{{End}}` )    |
| WAF002 | Error    | `End` has no matching opening block                                                | `{{End}}` without a preceding `For`/`ForEach`/`If` |

### Variable scope

| ID     | Severity | Description                                                                                         | Example                                |
|--------|----------|-----------------------------------------------------------------------------------------------------|----------------------------------------|
| WAF003 | Error    | A variable declared with `out var` in a `For`/`ForEach` block is referenced after the block's `End` | `{{For(0, 3, out var i)}}{{End}}{{i}}` |

### If block structure

| ID     | Severity | Description                                                                                 | Example                                           |
|--------|----------|---------------------------------------------------------------------------------------------|---------------------------------------------------|
| WAF004 | Error    | `Elif` or `Else` appears outside an `If` block (or inside a `For`/`ForEach` block directly) | `{{Elif(...)}}` (with no enclosing `{{If(...)}}`) |
| WAF005 | Error    | An `If` block contains more than one `Else`                                                 | `{{If(...)}}{{Else}}{{Else}}{{End}}`              |
| WAF006 | Error    | `Elif` appears after `Else` in the same `If` block                                          | `{{If(...)}}{{Else}}{{Elif(...)}}{{End}}`         |

### Iteration control

| ID     | Severity | Description                                                   | Example                                        |
|--------|----------|---------------------------------------------------------------|------------------------------------------------|
| WAF007 | Error    | `Break` or `Continue` is used outside a `For`/`ForEach` block | `{{Break}}` (with no enclosing `{{For(...)}}`) |
