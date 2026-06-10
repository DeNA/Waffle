// (c) DeNA Co., Ltd.

using Microsoft.CodeAnalysis;

namespace Waffle.Analyzer;

internal static class WaffleDiagnostics
{
    private const string Category = "Waffle";

    // ── Block structure ──────────────────────────────────────────────────────

    /// <summary>
    /// WAF001: A For/ForEach/If block has no matching End.
    /// </summary>
    internal static readonly DiagnosticDescriptor MissingEnd = new(
        id: "WAF001",
        title: "Missing End block",
        messageFormat: "The '{0}' block does not have a matching End",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Every For, Forr, ForEach, ForEachNullable, and If block must be closed with a matching End.");

    /// <summary>
    /// WAF002: End has no matching opening block.
    /// </summary>
    internal static readonly DiagnosticDescriptor UnexpectedEnd = new(
        id: "WAF002",
        title: "Unexpected End",
        messageFormat: "End has no matching For/ForEach/If block",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "End can only appear after an opening For, Forr, ForEach, ForEachNullable, or If block.");

    // ── Variable scope ───────────────────────────────────────────────────────

    /// <summary>
    /// WAF003: A variable introduced by out var in a For/ForEach block is used after its block's End.
    /// </summary>
    internal static readonly DiagnosticDescriptor OutOfScopeVariable = new(
        id: "WAF003",
        title: "Out-of-scope loop variable",
        messageFormat: "Variable '{0}' is accessed outside the scope of its For/ForEach block",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Variables declared with out var in a For or ForEach block cannot be referenced after the matching End.");

    // ── If block structure ───────────────────────────────────────────────────

    /// <summary>
    /// WAF004: Elif or Else appears outside an If...End block.
    /// </summary>
    internal static readonly DiagnosticDescriptor ElifElseOutsideIf = new(
        id: "WAF004",
        title: "Elif/Else outside If block",
        messageFormat: "'{0}' must appear inside an If...End block chain",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Elif and Else can only appear inside an If...End block.");

    /// <summary>
    /// WAF005: More than one Else in the same If block.
    /// </summary>
    internal static readonly DiagnosticDescriptor MultipleElse = new(
        id: "WAF005",
        title: "Multiple Else in If block",
        messageFormat: "An If block can only have one Else",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An If block cannot contain more than one Else clause.");

    /// <summary>
    /// WAF006: Elif appears after Else in the same If block.
    /// </summary>
    internal static readonly DiagnosticDescriptor ElifAfterElse = new(
        id: "WAF006",
        title: "Elif after Else",
        messageFormat: "Elif cannot appear after Else in the same If block",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Elif must appear before Else. Once Else is used, no further Elif is allowed in the same If block.");

    // ── Iteration control ────────────────────────────────────────────────────

    /// <summary>
    /// WAF007: Break or Continue used outside a For/ForEach iteration block.
    /// </summary>
    internal static readonly DiagnosticDescriptor BreakContinueOutsideIteration = new(
        id: "WAF007",
        title: "Break/Continue outside iteration block",
        messageFormat: "'{0}' can only appear inside a For/ForEach block",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Break and Continue can only appear inside a For, Forr, ForEach, or ForEachNullable block.");
}
