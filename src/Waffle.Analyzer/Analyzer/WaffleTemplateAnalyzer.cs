// (c) DeNA Co., Ltd.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Waffle.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WaffleTemplateAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            WaffleDiagnostics.MissingEnd,
            WaffleDiagnostics.UnexpectedEnd,
            WaffleDiagnostics.OutOfScopeVariable,
            WaffleDiagnostics.ElifElseOutsideIf,
            WaffleDiagnostics.MultipleElse,
            WaffleDiagnostics.ElifAfterElse,
            WaffleDiagnostics.BreakContinueOutsideIteration);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Cheap syntax-only filters first; GetSymbolInfo is expensive and runs for every
        // invocation in the compilation, so bail out before touching the semantic model.
        if (!HasRenderMethodName(invocation.Expression))
        {
            return;
        }

        // The interpolated string argument may appear at different positions depending on the overload.
        InterpolatedStringExpressionSyntax? interpolatedString = null;
        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (argument.Expression is InterpolatedStringExpressionSyntax interpolated)
            {
                interpolatedString = interpolated;
                break;
            }
        }

        if (interpolatedString == null)
        {
            return;
        }

        var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (!IsWaffleRenderMethod(symbol))
        {
            return;
        }

        new TemplateBlockWalker(context.SemanticModel)
            .Analyze(interpolatedString, context.ReportDiagnostic);
    }

    /// <summary>
    /// Syntax-only pre-filter: whether the invoked expression's simple name is
    /// <c>Render</c> or <c>RenderCSharp</c>. The authoritative check is done later via the symbol.
    /// </summary>
    private static bool HasRenderMethodName(ExpressionSyntax expression)
    {
        // SimpleNameSyntax covers both Render(...) and explicit Render<TContext>(...) calls.
        var name = expression switch
        {
            SimpleNameSyntax simple => simple.Identifier.ValueText,
            MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
            _ => null,
        };

        return name is "Render" or "RenderCSharp";
    }

    private static bool IsWaffleRenderMethod(IMethodSymbol? method)
    {
        return method is { Name: "Render" or "RenderCSharp" }
               && IsWaffleSyntaxType(method.ContainingType);
    }

    internal static bool IsWaffleSyntaxType(INamedTypeSymbol? type)
    {
        return type is
        {
            Name: "WaffleSyntax", ContainingNamespace: { Name: "Waffle", ContainingNamespace.IsGlobalNamespace: true }
        };
    }
}
