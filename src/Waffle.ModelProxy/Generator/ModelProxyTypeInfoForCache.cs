// (c) DeNA Co., Ltd.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Waffle.ModelProxy;

internal readonly record struct ModelProxyTypeInfoForCache(
    string OriginalShortName,
    string OriginalFullName,
    string FullNameSpace,
    ImmutableArray<MemberInfo> Members,
    ImmutableArray<TupleInfo> Tuples)
{
    public bool Equals(ModelProxyTypeInfoForCache other)
    {
        return OriginalShortName == other.OriginalShortName &&
               OriginalFullName == other.OriginalFullName &&
               FullNameSpace == other.FullNameSpace &&
               Members.SequenceEqual(other.Members) &&
               Tuples.SequenceEqual(other.Tuples);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = OriginalShortName.GetHashCode();
            hashCode = (hashCode * 397) ^ OriginalFullName.GetHashCode();
            hashCode = (hashCode * 397) ^ FullNameSpace.GetHashCode();
            hashCode = (hashCode * 397) ^ Members.GetHashCode();
            hashCode = (hashCode * 397) ^ Tuples.GetHashCode();
            return hashCode;
        }
    }

    public static readonly ModelProxyTypeInfoForCache Default =
        new("", "", "", ImmutableArray<MemberInfo>.Empty, ImmutableArray<TupleInfo>.Empty);

    public bool IsDefault => this.Equals(Default);

    public ModelProxyTypeInfoForCache(ITypeSymbol typeSymbol) : this(
        typeSymbol.Name,
        typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
        typeSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
        ParseMembers(typeSymbol, out var tuples),
        tuples)
    {
    }

    private static ImmutableArray<MemberInfo> ParseMembers(
        ITypeSymbol symbol, out ImmutableArray<TupleInfo> tuples)
    {
        var builder = ImmutableArray.CreateBuilder<MemberInfo>();
        var tuplesBuilder = ImmutableArray.CreateBuilder<TupleInfo>();
        var members = new List<ISymbol>();
        symbol.GetInstanceMembers(members);

        foreach (var m in members)
        {
            if (m is IPropertySymbol
                    {
                        DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                    }
                    or IFieldSymbol
                    {
                        DeclaredAccessibility: Accessibility.Public or Accessibility.Internal,
                    } && !m.Name.Contains('<'))
            {
                var mi = ParseMember(m, tuplesBuilder);
                if (!string.IsNullOrEmpty(mi.Name))
                {
                    builder.Add(mi);
                }
            }
            else if (m is IMethodSymbol
                     {
                         DeclaredAccessibility: Accessibility.Public or Accessibility.Internal,
                         MethodKind: MethodKind.Ordinary,
                         ReturnsVoid: false,
                         TypeParameters.Length: 0,
                     } method
                     && method.ContainingType.SpecialType is not SpecialType.System_Object
                     && method.Parameters.Length <= 4
                     && method.Parameters.All(p => p.RefKind == RefKind.None && !p.IsParams)
                     // Parameterless ToString is always emitted explicitly; skip here to avoid duplicate.
                     && !(method.Name == "ToString" && method.Parameters.Length == 0))
            {
                var mi = ParseMember(m, tuplesBuilder);
                if (!string.IsNullOrEmpty(mi.Name))
                {
                    builder.Add(mi);
                }
            }
        }

        tuples = tuplesBuilder.ToImmutable();
        return builder.ToImmutable();
    }

    private static MemberInfo ParseMember(ISymbol symbol, ImmutableArray<TupleInfo>.Builder tuplesToGenerateWrapper)
    {
        var type = symbol switch
        {
            IFieldSymbol fSymbol => fSymbol.Type,
            IPropertySymbol pSymbol => pSymbol.Type,
            IMethodSymbol mSymbol => mSymbol.ReturnType,
            _ => null
        };

        if (type is null)
        {
            return default;
        }

        var isMethod = symbol is IMethodSymbol;
        var isNullable = type.NullableAnnotation is NullableAnnotation.Annotated;
        var specialType = type.SpecialType;
        var name = symbol.Name;

        var methodParameters = isMethod && symbol is IMethodSymbol mSym
            ? ImmutableArray.CreateRange(mSym.Parameters.Select(p =>
                (p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), p.Name)))
            : ImmutableArray<(string, string)>.Empty;

        ParseTuples(symbol, tuplesToGenerateWrapper, out var isList, out var elemIsTuple, out _);
        // The last wrapped type added to the list corresponds to this
        var wrappedTupleType = elemIsTuple ? tuplesToGenerateWrapper[^1].ProxyType : "";

        // Tuple type
        if (!isList && elemIsTuple)
        {
            var fullTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return new MemberInfo(
                name, false, false, fullTypeName, "", [], isNullable, specialType, true, wrappedTupleType,
                fullTypeName, isMethod, methodParameters, false, false);
        }

        // List
        if (type.IsListType(out var elemType, out var needsToArray))
        {
            var fullTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            // List of tuples
            if (elemIsTuple)
            {
                var elemFullTypeNameTuple = elemType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return new MemberInfo(
                    name, true, false, elemFullTypeNameTuple, "", [], isNullable, specialType, true, wrappedTupleType,
                    fullTypeName, isMethod, methodParameters, false, needsToArray);
            }

            // Detect nullable reference type elements (e.g. string?[], List<string?>)
            var elemIsNullableRef = !elemType.IsValueType
                                    && elemType.NullableAnnotation is NullableAnnotation.Annotated;
            // Strip '?' from element type when nullable: downstream code appends '?' explicitly (e.g. IReadOnlyList<T?>).
            var elemFullTypeName = elemIsNullableRef
                ? elemType.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
                    .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                : elemType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var elemNs = elemType.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var elemParents = elemFullTypeName
                .RemovePrefix(elemNs).RemoveSuffix(elemType.Name)
                .Split('.').Where(it => !string.IsNullOrEmpty(it)).ToArray();
            var elemHasAttr = elemType.HasAttribute(ModelProxyGenerator.AttrName);
            return new MemberInfo(
                name, true, elemHasAttr, elemFullTypeName, elemNs, elemParents, isNullable, specialType, false, "",
                fullTypeName, isMethod, methodParameters, elemIsNullableRef, needsToArray);
        }

        // Anonymous type
        if (type is not INamedTypeSymbol namedType)
        {
            var fullTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var elemNs = type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var elemParents = fullTypeName
                .RemovePrefix(elemNs).RemoveSuffix(elemType.Name)
                .Split('.').Where(it => !string.IsNullOrEmpty(it)).ToArray();
            return new MemberInfo(
                name, false, false, fullTypeName, elemNs, elemParents, isNullable, specialType, false, "",
                fullTypeName, isMethod, methodParameters, false, false);
        }

        // Other
        {
            var fullTypeName = namedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var elemNs = namedType.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var elemParents = fullTypeName
                .RemovePrefix(elemNs).RemoveSuffix(elemType.Name)
                .Split('.').Where(it => !string.IsNullOrEmpty(it)).ToArray();
            var hasAttr = namedType.HasAttribute(ModelProxyGenerator.AttrName);
            return new MemberInfo(
                name, false, hasAttr, fullTypeName, elemNs, elemParents, isNullable, specialType, false, "",
                fullTypeName, isMethod, methodParameters, false, false);
        }
    }

    private static void ParseTuples(
        ISymbol memberSymbol, ImmutableArray<TupleInfo>.Builder result, out bool isList, out bool elemIsTuple,
        out bool listNeedsToArray)
    {
        var type = memberSymbol switch
        {
            IFieldSymbol fSymbol => fSymbol.Type,
            IPropertySymbol pSymbol => pSymbol.Type,
            IMethodSymbol mSymbol => mSymbol.ReturnType,
            _ => null
        };

        if (type is null)
        {
            isList = false;
            elemIsTuple = false;
            listNeedsToArray = false;
            return;
        }

        if (type.IsTupleType && type is INamedTypeSymbol namedType)
        {
            // The type itself is a tuple (not a list of tuples), so no list conversion is involved.
            Register(namedType, result);
            isList = false;
            elemIsTuple = true;
            listNeedsToArray = false;
            return;
        }

        isList = type.IsListType(out var elemType, out var needsToArray);
        listNeedsToArray = needsToArray;

        if (elemType.IsTupleType && elemType is INamedTypeSymbol namedElemType)
        {
            elemIsTuple = true;
            Register(namedElemType, result);
            return;
        }

        elemIsTuple = false;
        return;


        static void Register(INamedTypeSymbol namedType, ImmutableArray<TupleInfo>.Builder tuplesToCreate)
        {
            var fieldsBuilder = ImmutableArray.CreateBuilder<MemberInfo>();

            foreach (var tupleField in namedType.TupleElements)
            {
                // Pass out _ for listNeedsToArray; fieldNeedsToArray captures whether THIS field's own list
                // requires .ToArray() conversion — independent of any outer collection's conversion needs.
                ParseTuples(tupleField, tuplesToCreate, out var isList, out var elemIsTuple,
                    out var fieldNeedsToArray);
                if (elemIsTuple)
                {
                    var fullTypeName = tupleField.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    fieldsBuilder.Add(new MemberInfo(
                        tupleField.Name,
                        isList,
                        false,
                        fullTypeName,
                        "",
                        [],
                        false,
                        SpecialType.None,
                        true,
                        tuplesToCreate[^1].ProxyType,
                        fullTypeName,
                        false,
                        ImmutableArray<(string, string)>.Empty,
                        false,
                        fieldNeedsToArray));
                }
                else
                {
                    fieldsBuilder.Add(ParseMember(tupleField, null!));
                }
            }

            tuplesToCreate.Add(new TupleInfo($"TupleProxy{tuplesToCreate.Count}", fieldsBuilder.ToImmutable()));
        }
    }
}
