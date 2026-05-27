// (c) DeNA Co., Ltd.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Waffle.ModelProxy;

internal static class SymbolExtensions
{
    /// <summary>
    /// Returns whether the specified type is a recognised collection type (array, List&lt;T&gt;,
    /// IReadOnlyList&lt;T&gt;, any type implementing IReadOnlyList&lt;T&gt;, or any type implementing
    /// IEnumerable&lt;T&gt; — including non-generic closed implementations).
    /// </summary>
    /// <param name="symbol">The type to inspect.</param>
    /// <param name="elemType">The element type of the collection.</param>
    internal static bool IsListType(this ITypeSymbol symbol, out ITypeSymbol elemType)
        => IsListType(symbol, out elemType, out _);

    /// <summary>
    /// Returns whether the specified type is a recognised collection type.
    /// </summary>
    /// <param name="symbol">The type to inspect.</param>
    /// <param name="elemType">The element type of the collection.</param>
    /// <param name="needsEnumerableConversion">
    /// <c>true</c> when the type does not directly implement <see cref="System.Collections.Generic.IReadOnlyList{T}"/>
    /// and the generated accessor must call <c>.ToArray()</c> to obtain an <c>IReadOnlyList&lt;T&gt;</c>.
    /// <c>false</c> for arrays, <c>List&lt;T&gt;</c>, <c>IReadOnlyList&lt;T&gt;</c>,
    /// and any type (generic or not) that implements <c>IReadOnlyList&lt;T&gt;</c>.
    /// </param>
    internal static bool IsListType(this ITypeSymbol symbol, out ITypeSymbol elemType,
        out bool needsEnumerableConversion)
    {
        needsEnumerableConversion = false;

        if (symbol is IArrayTypeSymbol arrayType)
        {
            elemType = arrayType.ElementType;
            return true;
        }

        if (symbol is not INamedTypeSymbol namedType)
        {
            elemType = symbol;
            return false;
        }

        // string implements IEnumerable<char> but is a scalar value in template context, not a collection.
        if (namedType.SpecialType == SpecialType.System_String)
        {
            elemType = symbol;
            return false;
        }

        if (namedType.IsGenericType)
        {
            var defName = namedType.OriginalDefinition.Name;

            // Directly-named types whose element type is the first type argument and that are
            // assignable to IReadOnlyList<T> via covariance — no .ToArray() conversion needed.
            if (defName is "List" or "IReadOnlyList")
            {
                elemType = namedType.TypeArguments[0];
                return true;
            }

            // IList<T> does not extend IReadOnlyList<T>, so IResolvableTo<IList<T>> is not
            // covariant to IResolvableTo<IReadOnlyList<T>>. Treat it like any other IEnumerable<T>.
            // IEnumerable<T>, ICollection<T>, and IReadOnlyCollection<T> also need .ToArray().
            if (defName is "IList" or "IEnumerable" or "ICollection" or "IReadOnlyCollection")
            {
                elemType = namedType.TypeArguments[0];
                needsEnumerableConversion = true;
                return true;
            }
        }

        // Any named type (generic or not) that implements IReadOnlyList<T> — covariance handles the cast.
        foreach (var iface in namedType.AllInterfaces)
        {
            if (iface is { IsGenericType: true, OriginalDefinition.Name: "IReadOnlyList" })
            {
                elemType = iface.TypeArguments[0];
                return true;
            }
        }

        // Any named type that implements IEnumerable<T> — needs .ToArray() conversion.
        foreach (var iface in namedType.AllInterfaces)
        {
            if (iface is { IsGenericType: true, OriginalDefinition.Name: "IEnumerable" })
            {
                elemType = iface.TypeArguments[0];
                needsEnumerableConversion = true;
                return true;
            }
        }

        elemType = symbol;
        return false;
    }

    /// <summary>
    /// Returns whether the specified symbol has the specified attribute.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="attrNameWithoutSuffix">Attribute name without the trailing Attribute suffix.</param>
    internal static bool HasAttribute(this ISymbol t, string attrNameWithoutSuffix)
    {
        foreach (var attr in t.GetAttributes())
        {
            var name = attr.AttributeClass!.Name;
            if (name == attrNameWithoutSuffix || name == attrNameWithoutSuffix + "Attribute")
            {
                return true;
            }
        }

        return false;
    }

    private readonly record struct HiddenMethod(string Name, ITypeSymbol[] Args)
    {
        public bool WillConflictWith(IMethodSymbol other)
        {
            if (Name != other.Name)
            {
                return false;
            }

            if (Args.Length != other.Parameters.Length)
            {
                return false;
            }

            if (Args.Length == 0)
            {
                return true;
            }

            for (var i = 0; i < Args.Length; i++)
            {
                if (!SymbolEqualityComparer.Default.Equals(Args[i], other.Parameters[i].Type))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Returns all non-override instance member symbols defined on the specified type.<br/>
    /// Members hidden via new (or by implicit shadowing) are excluded; only the hiding version is kept.
    /// </summary>
    internal static void GetInstanceMembers(
        this ITypeSymbol symbol, List<ISymbol> result)
    {
        var hiddenMethods = new List<HiddenMethod>();
        // Tracks property/field names already contributed by a more-derived type so that
        // shadowed base members (whether explicitly hidden with `new` or implicitly) are skipped.
        var seenPropertyAndFieldNames = new HashSet<string>();

        while (true)
        {
            foreach (var member in symbol.GetMembers())
            {
                if (member.IsStatic || member.IsOverride)
                {
                    continue;
                }

                if (member is IMethodSymbol m)
                {
                    if (hiddenMethods.Any(it => it.WillConflictWith(m)))
                    {
                        continue;
                    }

                    // NOTE: Hiding via new cannot be detected from IMethodSymbol alone, so the SyntaxTree is inspected directly.
                    foreach (var syntaxRef in m.DeclaringSyntaxReferences)
                    {
                        if (syntaxRef.GetSyntax() is not MethodDeclarationSyntax syntax)
                        {
                            continue;
                        }

                        foreach (var modifier in syntax.Modifiers)
                        {
                            if (modifier.Text is "new")
                            {
                                hiddenMethods.Add(
                                    new HiddenMethod(m.Name, m.Parameters.Select(it => it.Type).ToArray()));
                                goto LOOP_END;
                            }
                        }
                    }

                    LOOP_END: ;
                }
                else if (member is IPropertySymbol or IFieldSymbol)
                {
                    // If a more-derived type already contributed a member with this name, skip the
                    // base-class version (handles both explicit `new` and implicit shadowing).
                    if (!seenPropertyAndFieldNames.Add(member.Name))
                    {
                        continue;
                    }
                }

                result.Add(member);
            }

            if (symbol.BaseType is null)
            {
                return;
            }

            symbol = symbol.BaseType;
        }
    }
}
