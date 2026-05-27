// (c) DeNA Co., Ltd.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Waffle.ModelProxy;

/// <summary>
/// Member info (fields, properties and methods) parsed from a <see cref="ITypeSymbol"/>.
/// </summary>
internal readonly record struct MemberInfo(
    string Name,
    bool IsList,
    bool ElemHasAttr,
    string ElemFullType,
    string ElemNs,
    string[] ElemParents,
    bool IsNullable,
    SpecialType SpecialType,
    bool IsTupleOrTupleList,
    string ElemProxyTupleName,
    string OriginalFullTypeName,
    bool IsMethod,
    ImmutableArray<(string FullType, string ParamName)> MethodParameters,
    bool ElemIsNullableRef,
    bool ElemNeedsToArray)
{
    /// <summary>True when this is a method with one or more parameters.</summary>
    public bool HasMethodParameters => IsMethod && !MethodParameters.IsEmpty;

    public string PrivateFieldName => Name.ToPrivateFieldName();

    public string ProxyTypeName()
    {
        if (ElemHasAttr)
        {
            if (IsNullable && !IsList)
            {
                // Nullable [ModelProxy] type: expose as IResolvableTo<T?> to preserve nullability.
                // ElemFullType already includes '?' from Roslyn's FullyQualifiedFormat for nullable types,
                // so we must NOT append another '?'.
                return $"IResolvableTo<{ElemFullType}>";
            }

            return ModelProxyGeneratorHelper.ProxyTypeNameForAttrNoted(ElemFullType, IsList, ElemNs, ElemParents);
        }

        if (IsTupleOrTupleList)
        {
            return IsList ? $"{ElemProxyTupleName}List" : ElemProxyTupleName;
        }

        if (IsList)
        {
            // Non-nullable list with nullable reference type elements: use NullableRefListProxy.
            if (ElemIsNullableRef)
            {
                return $"NullableRefListProxy<{ElemFullType}>";
            }

            return $"ListProxy<{ElemFullType}>";
        }

        if (IsNullable)
        {
            // ElemFullType already includes '?' from Roslyn's FullyQualifiedFormat for all nullable types:
            //   - Nullable<T> (e.g., int?, bool?): ElemFullType = "int?", "bool?"
            //   - Nullable reference types (e.g., Model2?): ElemFullType = "global::...Model2?"
            // Exception: for string? we use the keyword form for readability.
            return SpecialType switch
            {
                SpecialType.System_String => "IResolvableTo<string?>",
                _ => $"IResolvableTo<{ElemFullType}>", // ElemFullType already has '?'
            };
        }

        return SpecialType switch
        {
            SpecialType.System_Int32 => "IntProxy",
            SpecialType.System_Boolean => "BoolProxy",
            SpecialType.System_String => "StringProxy",
            _ => $"IResolvableTo<{ElemFullType}>",
        };
    }

    public string AccessorCreator(string sourceName)
    {
        // For methods, invoke with () rather than just accessing a member.
        var memberAccess = IsMethod ? $"it.{Name}()" : $"it.{Name}";

        // Nullable list (the collection itself may be null): null-coalesce to empty to avoid ArgumentNullException.
        if (IsNullable && IsList)
        {
            if (ElemNeedsToArray)
            {
                // IEnumerable<T> variant: use ?.ToArray() for safe null-handling and IReadOnlyList<T> conversion.
                if (ElemIsNullableRef)
                {
                    var coalesced =
                        $"{sourceName}.To(it => {memberAccess}?.ToArray() ?? Array.Empty<{ElemFullType}?>())";
                    return $"new({coalesced})";
                }

                var coalesced2 =
                    $"{sourceName}.To(it => {memberAccess}?.ToArray() ?? Array.Empty<{ElemFullType}>())";
                return $"new({coalesced2})";
            }

            // Cast the nullable collection to IReadOnlyList<T>? first so '??' can coalesce with Array.Empty<T>()
            // regardless of the concrete collection type (e.g., List<T>? vs T[]?).
            if (ElemIsNullableRef)
            {
                // Nullable collection of nullable reference type elements
                var coalesced =
                    $"{sourceName}.To(it => (IReadOnlyList<{ElemFullType}?>?){memberAccess} ?? Array.Empty<{ElemFullType}?>())";
                return $"new({coalesced})";
            }

            var coalesced3 =
                $"{sourceName}.To(it => (IReadOnlyList<{ElemFullType}>?){memberAccess} ?? Array.Empty<{ElemFullType}>())";
            return $"new({coalesced3})";
        }

        // Non-nullable list with nullable reference type elements: cast to IReadOnlyList<T?>.
        if (IsList && ElemIsNullableRef)
        {
            return ElemNeedsToArray
                ? $"new({sourceName}.To(it => {memberAccess}.ToArray()))"
                : $"new({sourceName}.To(it => (IReadOnlyList<{ElemFullType}?>){memberAccess}))";
        }

        // Nullable non-list types: return IResolvableTo<T?> directly, without ! null-forgiving
        if (IsNullable)
        {
            return $"{sourceName}.To(it => {memberAccess})";
        }

        // Non-nullable path: original behaviour
        var defaultCreator = $"{sourceName}.To(it => {memberAccess})!";

        if (ElemHasAttr || IsList || IsTupleOrTupleList ||
            SpecialType is SpecialType.System_Int32 or SpecialType.System_Boolean or SpecialType.System_String)
        {
            if (IsList && ElemNeedsToArray)
            {
                // IEnumerable<T> type: call .ToArray() so the result is IReadOnlyList<T> via covariance.
                return $"new({sourceName}.To(it => {memberAccess}.ToArray())!)";
            }

            return $"new({defaultCreator})";
        }

        return defaultCreator;
    }

    /// <summary>
    /// Returns the full method body expression (after =>) for a parameterized method accessor.
    /// Uses With() to combine source and all parameter resolvables.
    /// </summary>
    public string ParameterizedAccessorBody(string sourceName)
    {
        var paramCount = MethodParameters.Length;
        var lambdaParams = string.Join(", ", Enumerable.Range(1, paramCount).Select(i => $"_p{i}"));
        var callArgs = string.Join(", ", Enumerable.Range(1, paramCount).Select(i => $"_p{i}"));

        string withChain;
        if (paramCount == 1)
        {
            withChain = $"{sourceName}.With({MethodParameters[0].ParamName}, (it, _p1) => it.{Name}(_p1))";
        }
        else
        {
            var withArgs = string.Join(", ", MethodParameters.Select(p => p.ParamName));
            var lambdaSignature = "it, " + lambdaParams;
            withChain = $"{sourceName}.With({withArgs}, ({lambdaSignature}) => it.{Name}({callArgs}))";
        }

        if (IsNullable && IsList)
        {
            // Nullable list return from parameterized method
            if (ElemNeedsToArray)
            {
                if (ElemIsNullableRef)
                {
                    return
                        $"new({withChain}.To(ls => ls?.ToArray() ?? Array.Empty<{ElemFullType}?>()))";
                }

                return $"new({withChain}.To(ls => ls?.ToArray() ?? Array.Empty<{ElemFullType}>()))";
            }

            if (ElemIsNullableRef)
            {
                return
                    $"new({withChain}.To(ls => (IReadOnlyList<{ElemFullType}?>?)ls ?? Array.Empty<{ElemFullType}?>()))";
            }

            return $"new({withChain}.To(ls => (IReadOnlyList<{ElemFullType}>?)ls ?? Array.Empty<{ElemFullType}>()))";
        }

        // Non-nullable list with nullable reference type elements
        if (IsList && ElemIsNullableRef)
        {
            return ElemNeedsToArray
                ? $"new({withChain}.To(ls => ls.ToArray()))"
                : $"new({withChain}.To(ls => (IReadOnlyList<{ElemFullType}?>)ls))";
        }

        if (IsNullable)
        {
            return withChain;
        }

        var nonNullBody = $"{withChain}!";
        if (ElemHasAttr || IsList || IsTupleOrTupleList ||
            SpecialType is SpecialType.System_Int32 or SpecialType.System_Boolean or SpecialType.System_String)
        {
            if (IsList && ElemNeedsToArray)
            {
                return $"new({withChain}.To(ls => ls.ToArray())!)";
            }

            return $"new({nonNullBody})";
        }

        return nonNullBody;
    }

    /// <summary>
    /// Returns the Has-accessor body for a parameterized nullable method.
    /// </summary>
    public string ParameterizedHasAccessorBody(string sourceName)
    {
        var paramCount = MethodParameters.Length;
        var lambdaParams = string.Join(", ", Enumerable.Range(1, paramCount).Select(i => $"_p{i}"));
        var callArgs = string.Join(", ", Enumerable.Range(1, paramCount).Select(i => $"_p{i}"));

        string withChain;
        if (paramCount == 1)
        {
            withChain = $"{sourceName}.With({MethodParameters[0].ParamName}, (it, _p1) => it.{Name}(_p1) is not null)";
        }
        else
        {
            var withArgs = string.Join(", ", MethodParameters.Select(p => p.ParamName));
            var lambdaSignature = "it, " + lambdaParams;
            withChain = $"{sourceName}.With({withArgs}, ({lambdaSignature}) => it.{Name}({callArgs}) is not null)";
        }

        return $"new({withChain})";
    }

    public bool Equals(MemberInfo other)
    {
        return Name == other.Name
               && IsList == other.IsList
               && ElemHasAttr == other.ElemHasAttr
               && ElemFullType == other.ElemFullType
               && ElemNs == other.ElemNs
               && ElemParents.SequenceEqual(other.ElemParents)
               && IsNullable == other.IsNullable
               && SpecialType == other.SpecialType
               && IsTupleOrTupleList == other.IsTupleOrTupleList
               && ElemProxyTupleName == other.ElemProxyTupleName
               && OriginalFullTypeName == other.OriginalFullTypeName
               && IsMethod == other.IsMethod
               && MethodParameters.SequenceEqual(other.MethodParameters)
               && ElemIsNullableRef == other.ElemIsNullableRef
               && ElemNeedsToArray == other.ElemNeedsToArray;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Name.GetHashCode();
            hash = (hash * 397) ^ IsList.GetHashCode();
            hash = (hash * 397) ^ ElemHasAttr.GetHashCode();
            hash = (hash * 397) ^ (ElemFullType != null ? ElemFullType.GetHashCode() : 0);
            hash = (hash * 397) ^ (ElemNs != null ? ElemNs.GetHashCode() : 0);
            if (ElemParents != null)
            {
                foreach (var parent in ElemParents)
                {
                    hash = (hash * 397) ^ (parent.GetHashCode());
                }
            }

            hash = (hash * 397) ^ IsNullable.GetHashCode();
            hash = (hash * 397) ^ SpecialType.GetHashCode();
            hash = (hash * 397) ^ IsTupleOrTupleList.GetHashCode();
            hash = (hash * 397) ^ (ElemProxyTupleName != null ? ElemProxyTupleName.GetHashCode() : 0);
            hash = (hash * 397) ^ (OriginalFullTypeName != null ? OriginalFullTypeName.GetHashCode() : 0);
            hash = (hash * 397) ^ IsMethod.GetHashCode();
            foreach (var p in MethodParameters)
            {
                hash = (hash * 397) ^ p.GetHashCode();
            }

            hash = (hash * 397) ^ ElemIsNullableRef.GetHashCode();
            hash = (hash * 397) ^ ElemNeedsToArray.GetHashCode();
            return hash;
        }
    }
}
