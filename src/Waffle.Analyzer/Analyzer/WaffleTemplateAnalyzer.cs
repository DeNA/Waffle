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
        var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

        if (!IsWaffleRenderMethod(symbol))
        {
            return;
        }

        // The interpolated string argument may appear at different positions depending on the overload.
        var interpolatedString = invocation.ArgumentList.Arguments
            .Select(a => a.Expression)
            .OfType<InterpolatedStringExpressionSyntax>()
            .FirstOrDefault();
        if (interpolatedString == null)
        {
            return;
        }

        new TemplateBlockWalker(context.SemanticModel)
            .Analyze(interpolatedString, context.ReportDiagnostic);
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
