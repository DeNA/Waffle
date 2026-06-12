// (c) DeNA Co., Ltd.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Waffle.Analyzer;

internal sealed class TemplateBlockWalker
{
    private readonly SemanticModel _semanticModel;
    private readonly Stack<BlockFrame> _blockStack = new();
    private readonly HashSet<ILocalSymbol> _closedVariables = new(SymbolEqualityComparer.Default);

    internal TemplateBlockWalker(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    internal void Analyze(InterpolatedStringExpressionSyntax interpolated, Action<Diagnostic> report)
    {
        foreach (var content in interpolated.Contents)
        {
            if (content is not InterpolationSyntax interpolation)
            {
                continue;
            }

            var expr = interpolation.Expression;
            var command = ClassifySymbol(_semanticModel.GetSymbolInfo(expr).Symbol);

            if (command.IsOpeningBlock)
            {
                var ownedVars = ExtractOutVarSymbols(expr);
                // A variable reused via "out i" (without var) may still be in _closedVariables
                // from a previous block. Remove it so references inside this block are not
                // falsely flagged as WAF003.
                foreach (var v in ownedVars)
                {
                    _closedVariables.Remove(v);
                }

                _blockStack.Push(new BlockFrame(interpolation, command, ownedVars));
            }
            else if (command is WaffleCommand.End)
            {
                if (_blockStack.Count > 0)
                {
                    var frame = _blockStack.Pop();
                    foreach (var v in frame.OwnedVariables)
                    {
                        _closedVariables.Add(v);
                    }
                }
                else
                {
                    // WAF002: End with no matching open block
                    report(Diagnostic.Create(
                        WaffleDiagnostics.UnexpectedEnd,
                        interpolation.GetLocation()));
                }
            }
            else if (command is WaffleCommand.Elif or WaffleCommand.Else)
            {
                if (_blockStack.Count == 0 || _blockStack.Peek().Command != WaffleCommand.If)
                {
                    // WAF004: Elif/Else outside an If block
                    report(Diagnostic.Create(
                        WaffleDiagnostics.ElifElseOutsideIf,
                        interpolation.GetLocation(),
                        command.ToString()));
                }
                else if (command is WaffleCommand.Else)
                {
                    var topFrame = _blockStack.Peek();
                    if (topFrame.HasElse)
                    {
                        // WAF005: second Else in the same If block
                        report(Diagnostic.Create(
                            WaffleDiagnostics.MultipleElse,
                            interpolation.GetLocation()));
                    }
                    else
                    {
                        topFrame.HasElse = true;
                    }
                }
                else // Elif
                {
                    if (_blockStack.Peek().HasElse)
                    {
                        // WAF006: Elif after Else
                        report(Diagnostic.Create(
                            WaffleDiagnostics.ElifAfterElse,
                            interpolation.GetLocation()));
                    }
                }
            }
            else if (command is WaffleCommand.Break or WaffleCommand.Continue)
            {
                if (!HasIterationFrame())
                {
                    // WAF007: Break/Continue outside any For/ForEach block
                    report(Diagnostic.Create(
                        WaffleDiagnostics.BreakContinueOutsideIteration,
                        interpolation.GetLocation(),
                        command.ToString()));
                }
            }
            else // WaffleCommand.None — non-block commands or user expression
            {
                // A non-block command (e.g. Let) may rebind a closed variable via "out x".
                // Remove those from _closedVariables before checking, so that both the "out x"
                // position itself and subsequent references within this interpolation are not
                // falsely flagged.
                RemoveReboundOutVarsFromClosedSet(expr);
                CheckForOutOfScopeVariables(expr, report);
            }
        }

        // Any unclosed block at the end of the template → WAF001
        foreach (var frame in _blockStack)
        {
            report(Diagnostic.Create(
                WaffleDiagnostics.MissingEnd,
                frame.OpeningNode.GetLocation(),
                frame.Command.ToString()));
        }
    }

    private bool HasIterationFrame()
    {
        foreach (var frame in _blockStack)
        {
            if (frame.Command.IsIterationBlock)
            {
                return true;
            }
        }

        return false;
    }

    private void CheckForOutOfScopeVariables(ExpressionSyntax expr, Action<Diagnostic> report)
    {
        // No variable has gone out of scope yet (true until the first End of a block that owns
        // out-vars), so the per-identifier semantic model queries below can never report anything.
        if (_closedVariables.Count == 0)
        {
            return;
        }

        foreach (var id in expr.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            // Skip identifiers in "out x" position — they are being written to, not read.
            if (id.Parent is ArgumentSyntax { RefOrOutKeyword: { RawKind: (int)SyntaxKind.OutKeyword } })
            {
                continue;
            }

            if (_semanticModel.GetSymbolInfo(id).Symbol is ILocalSymbol local
                && _closedVariables.Contains(local))
            {
                report(Diagnostic.Create(
                    WaffleDiagnostics.OutOfScopeVariable,
                    id.GetLocation(),
                    id.Identifier.Text));
            }
        }
    }

    private void RemoveReboundOutVarsFromClosedSet(ExpressionSyntax expr)
    {
        if (expr is not InvocationExpressionSyntax invocation) return;

        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (!argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword))
            {
                continue;
            }

            // out var x — new declaration, not a rebind of a closed variable
            if (argument.Expression is DeclarationExpressionSyntax)
            {
                continue;
            }

            // out x — rebind; remove from closed set so subsequent references are not flagged
            if (_semanticModel.GetSymbolInfo(argument.Expression).Symbol is ILocalSymbol local)
            {
                _closedVariables.Remove(local);
            }
        }
    }

    private ImmutableArray<ILocalSymbol> ExtractOutVarSymbols(ExpressionSyntax expr)
    {
        if (expr is not InvocationExpressionSyntax invocation)
            return ImmutableArray<ILocalSymbol>.Empty;

        var builder = ImmutableArray.CreateBuilder<ILocalSymbol>();

        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (!argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword))
            {
                continue;
            }

            if (argument.Expression is DeclarationExpressionSyntax decl
                && decl.Designation is SingleVariableDesignationSyntax designation
                && _semanticModel.GetDeclaredSymbol(designation) is ILocalSymbol declared)
            {
                // out var i — new declaration
                builder.Add(declared);
            }
            else if (_semanticModel.GetSymbolInfo(argument.Expression).Symbol is ILocalSymbol reused)
            {
                // out i — rebinding an existing variable to a new block
                builder.Add(reused);
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>Resolves a symbol to its Waffle command classification in a single pass.</summary>
    private static WaffleCommand ClassifySymbol(ISymbol? symbol) => symbol switch
    {
        IMethodSymbol m when WaffleTemplateAnalyzer.IsWaffleSyntaxType(m.ContainingType) => m.Name switch
        {
            "For" => WaffleCommand.For,
            "Forr" => WaffleCommand.Forr,
            "ForEach" => WaffleCommand.ForEach,
            "ForEachNullable" => WaffleCommand.ForEachNullable,
            "If" => WaffleCommand.If,
            "Elif" => WaffleCommand.Elif,
            _ => WaffleCommand.None,
        },
        IFieldSymbol f when WaffleTemplateAnalyzer.IsWaffleSyntaxType(f.ContainingType) => f.Name switch
        {
            "End" => WaffleCommand.End,
            "Else" => WaffleCommand.Else,
            "Break" => WaffleCommand.Break,
            "Continue" => WaffleCommand.Continue,
            _ => WaffleCommand.None,
        },
        _ => WaffleCommand.None,
    };

    private sealed class BlockFrame
    {
        internal BlockFrame(InterpolationSyntax openingNode, WaffleCommand command,
            ImmutableArray<ILocalSymbol> ownedVariables)
        {
            OpeningNode = openingNode;
            Command = command;
            OwnedVariables = ownedVariables;
        }

        internal InterpolationSyntax OpeningNode { get; }
        internal WaffleCommand Command { get; }
        internal ImmutableArray<ILocalSymbol> OwnedVariables { get; }

        /// <summary>True once Else has been seen in this If block.</summary>
        internal bool HasElse { get; set; }
    }
}
