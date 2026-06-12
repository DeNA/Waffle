// (c) DeNA Co., Ltd.

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Waffle.ModelProxy;

/// <summary>
/// Generator that produces proxy wrappers for classes and structs annotated with [ModelProxy].
/// Enables easy proxy member access inside template iteration loops.
/// </summary>
[Generator]
public class ModelProxyGenerator : IIncrementalGenerator
{
    internal const string AttrName = "ModelProxy";
    private const string AttrNamespace = "Waffle.ModelProxy";

    private const AttributeTargets AttrTargets =
        AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface;

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            $"{AttrName}Attribute.g.cs",
            SourceText.From(
                IsgHelper.AttributeDefinitionFile(
                    AttrNamespace, [], AttrTargets, AttrName, "",
                    """
                    Marker attribute that generates a helper class for proxy member access inside For and ForEach loops of template.<br/>
                    The original class instance can be converted via .AsProxy().
                    """),
                Encoding.UTF8)));

        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"{AttrNamespace}.{AttrName}Attribute",
                static (syntax, _) => syntax is TypeDeclarationSyntax,
                static (ctx, token) =>
                {
                    token.ThrowIfCancellationRequested();

                    return ctx.TargetSymbol is INamedTypeSymbol namedTypeSymbol
                        ? new ModelProxyTypeInfoForCache(namedTypeSymbol)
                        : ModelProxyTypeInfoForCache.Default;
                })
            .Where(it => !it.IsDefault)
            .Select((cache, token) =>
            {
                token.ThrowIfCancellationRequested();
                return new ModelProxyTypeInfo(cache);
            })
            .Collect();

        context.RegisterSourceOutput(provider, static (ctx, targets) =>
        {
            if (targets.Length == 0)
            {
                return;
            }

            foreach (var target in targets)
            {
                var fileName = ModelProxyGeneratorHelper.GetOutputFileName(target.OriginalFullName, target.FullNs);
                var rendered = Render(target);
                ctx.AddSource($"{fileName}.g.cs", rendered);
            }
        });
    }

    private static string Render(in ModelProxyTypeInfo info)
    {
        var sb = new StringBuilder();

        sb.AppendLine( /* lang=cs */$$"""
{{IsgHelper.Header()}}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Waffle;
using Waffle.Interpreter;

namespace {{info.Ns}}
{
""");
        for (var i = 0; i < info.ProxyParents.Length; i++)
        {
            var parent = info.ProxyParents[i];
            sb.AppendLine(IsgHelper.InsertIndent($"public sealed partial class {parent}\n{{", i + 1));
        }

        var mainIndentLevel = info.ProxyParents.Length;

        var lazyDecl = ProxyDecl(info.ProxyShortName, info.OriginalFullName, info.Members, 1, false);
        sb.AppendLine(IsgHelper.InsertIndent(lazyDecl, mainIndentLevel));

        if (info.Tuples.Length > 0)
        {
            sb.AppendLine(IsgHelper.InsertIndent( /* lang=cs */$$"""

        // Tuples
""", info.ProxyParents.Length));
        }

        foreach (var tuple in info.Tuples)
        {
            var originalTuple =
                $"({string.Join(", ", tuple.Fields.Select(it => $"{it.OriginalFullTypeName} {it.Name}"))})";
            sb.AppendLine( /* lang=cs */$$"""
{{IsgHelper.InsertIndent(ProxyDecl(tuple.ProxyType, originalTuple, tuple.Fields, 2, true), mainIndentLevel)}}

{{IsgHelper.InsertIndent(ProxyListDecl(tuple.ProxyListType, tuple.ProxyType, originalTuple, 2), mainIndentLevel)}}

""");
        }

        sb.AppendLine(IsgHelper.InsertIndent("    }", mainIndentLevel));

        var lazyListDecl =
            ProxyListDecl(info.ProxyListShortName, info.ProxyFullName, info.OriginalFullName, 1);
        sb.AppendLine(IsgHelper.InsertIndent(lazyListDecl, mainIndentLevel));


        for (var i = 0; i < info.ProxyParents.Length; i++)
        {
            sb.AppendLine(IsgHelper.InsertIndent("    }", info.ProxyParents.Length - i - 1));
        }

        sb.AppendLine( /* lang=cs */$$"""

    public static partial class {{info.ProxyLongName.Replace(".", "_")}}Converter
    {
        /// <summary>
        /// Converts to a proxy wrapper.
        /// </summary>
        public static {{info.ProxyLongName}} AsProxy(this {{info.OriginalFullName}} self) => new(self);

        /// <summary>
        /// Converts to a proxy wrapper.
        /// </summary>
        public static {{info.ProxyLongName}} AsProxy(this IResolvableTo<{{info.OriginalFullName}}> self) => new(self);

        /// <summary>
        /// Converts to a proxy-list wrapper.
        /// </summary>
        public static {{info.ProxyListLongName}} AsProxy(this IReadOnlyList<{{info.OriginalFullName}}> self) => new(self);

        /// <summary>
        /// Converts to a proxy-list wrapper.
        /// </summary>
        public static {{info.ProxyListLongName}} AsProxy(this IEnumerable<{{info.OriginalFullName}}> self) => new(self.ToArray());
    }
}

""");

        return sb.ToString();
    }

    private static string ProxyDecl(string modelShortName, string originalName,
        ImmutableArray<MemberInfo> members, int indentLevel,
        bool shouldCloseLast)
    {
        var sb = new StringBuilder();

        sb.AppendLine( /* lang=cs */$$"""
/// <summary>
/// Proxy wrapper for <see cref="{{originalName}}"/> (generated by ISG via the <see cref="Waffle.ModelProxy.{{AttrName}}Attribute"/> attribute)
/// </summary>
public sealed partial class {{modelShortName}} : IResolvableTo<{{originalName}}>, ILazyInitializedBy<{{originalName}}>
{
    // Backing source
    private IResolvableTo<{{originalName}}> _source;

    // Constructors
    public {{modelShortName}}() { _source = default!; }
    public {{modelShortName}}(IResolvableTo<{{originalName}}> source) { _source = source; }
    public {{modelShortName}}({{originalName}} source) { _source = new LiteralProxy<{{originalName}}>(source); }
    public void Initialize(IResolvableTo<{{originalName}}> source) { _source = source; }

    // IResolvableTo interface
    {{originalName}} IResolvableTo<{{originalName}}>.Resolve(Dictionary<int, Waffle.Interpreter.EnvValue> env) => _source.Resolve(env);

    // Accessor cache for each member
""");
        foreach (var member in members)
        {
            if (member.HasMethodParameters) continue; // parameterized methods have no backing field

            sb.AppendLine( /* lang=cs */$$"""
    private {{member.ProxyTypeName()}}? {{member.PrivateFieldName}};
""");
            if (member.IsNullable)
            {
                sb.AppendLine( /* lang=cs */$$"""
    private BoolProxy? _has{{member.Name}};
""");
            }
        }

        // ToString() is always exposed as StringProxy (shadows object.ToString())
        sb.AppendLine( /* lang=cs */$$"""
    private StringProxy? _toString;
""");

        sb.AppendLine( /* lang=cs */$$"""

    // Accessor for each member
""");
        foreach (var member in members)
        {
            if (member.HasMethodParameters)
            {
                // Parameterized method: no caching; emit IResolvableTo<T> overload + T convenience overload.
                var paramListResolvable = string.Join(", ",
                    member.MethodParameters.Select(p => $"IResolvableTo<{p.FullType}> {p.ParamName}"));
                var paramListDirect =
                    string.Join(", ", member.MethodParameters.Select(p => $"{p.FullType} {p.ParamName}"));
                var convenienceArgs = string.Join(", ",
                    member.MethodParameters.Select(p => $"new LiteralProxy<{p.FullType}>({p.ParamName})"));

                sb.AppendLine( /* lang=cs */$$"""
    public {{member.ProxyTypeName()}} {{member.Name}}({{paramListResolvable}}) => {{member.ParameterizedAccessorBody("_source")}};
    public {{member.ProxyTypeName()}} {{member.Name}}({{paramListDirect}}) => {{member.Name}}({{convenienceArgs}});
""");
                if (member.IsNullable)
                {
                    sb.AppendLine( /* lang=cs */$$"""
    public BoolProxy Has{{member.Name}}({{paramListResolvable}}) => {{member.ParameterizedHasAccessorBody("_source")}};
    public BoolProxy Has{{member.Name}}({{paramListDirect}}) => Has{{member.Name}}({{convenienceArgs}});
""");
                }
            }
            else if (member.IsMethod)
            {
                sb.AppendLine( /* lang=cs */$$"""
    public {{member.ProxyTypeName()}} {{member.Name}}() => {{member.PrivateFieldName}} ??= {{member.AccessorCreator("_source")}};
""");
                if (member.IsNullable)
                {
                    sb.AppendLine( /* lang=cs */$$"""
    public BoolProxy Has{{member.Name}} => _has{{member.Name}} ??= new(_source.To(it => it.{{member.Name}}() is not null));
""");
                }
            }
            else
            {
                sb.AppendLine( /* lang=cs */$$"""
    public {{member.ProxyTypeName()}} {{member.Name}} => {{member.PrivateFieldName}} ??= {{member.AccessorCreator("_source")}};
""");
                if (member.IsNullable)
                {
                    sb.AppendLine( /* lang=cs */$$"""
    public BoolProxy Has{{member.Name}} => _has{{member.Name}} ??= new(_source.To(it => it.{{member.Name}} is not null));
""");
                }
            }
        }

        // ToString() accessor (shadows object.ToString() to return StringProxy for template use)
        sb.AppendLine( /* lang=cs */$$"""
    public new StringProxy ToString() => _toString ??= new(_source.To(it => it.ToString())!);
""");

        if (shouldCloseLast)
        {
            sb.AppendLine( /* lang=cs */$$"""
}

""");
        }

        return IsgHelper.InsertIndent(sb.ToString(), indentLevel);
    }

    private static string ProxyListDecl(string modelListShortName, string modelName,
        string originalName, int indentLevel)
    {
        var sb = new StringBuilder();

        //lang=cs
        sb.AppendLine($$"""
/// <summary>
/// Proxy-list wrapper for <see cref="{{originalName}}"/> (generated by ISG via the <see cref="Waffle.ModelProxy.{{AttrName}}Attribute"/> attribute)
/// </summary>
public sealed partial class {{modelListShortName}} : IIterationSource<{{modelName}}, {{originalName}}>
{
    // Backing source
    private readonly IResolvableTo<IReadOnlyList<{{originalName}}>> _source;

    // Constructors
    public {{modelListShortName}}(IResolvableTo<IReadOnlyList<{{originalName}}>> source) { _source = source; }
    public {{modelListShortName}}(IReadOnlyList<{{originalName}}> source) { _source = new LiteralProxy<IReadOnlyList<{{originalName}}>>(source); }

    // IResolvableToList interface
    public IntProxy Count => new(_source.To(it => it.Count));
    public {{modelName}} this[int i] => new(_source.To(it => it[i]));
    public {{modelName}} this[IResolvableTo<int> i] => new(_source.With(i, (it, i) => it[i]));

    // Interfaces used internally by the template engine
    IEnumerable<({{originalName}} Value, int Index)> IIterationSource<{{modelName}}, {{originalName}}>.GetSource(Dictionary<int, Waffle.Interpreter.EnvValue> env) => _source.Resolve(env).Select((it, i) => (it, i));
    {{modelName}} IIterationSource<{{modelName}}, {{originalName}}>.GetIterator(int id) => new(new IteratorProxy<{{originalName}}>(id));
    IntProxy IIterationSource<{{modelName}}, {{originalName}}>.GetIteratorIndex(int id) => new(new IntIteratorProxy(id));
    IReadOnlyList<{{originalName}}> IResolvableTo<IReadOnlyList<{{originalName}}>>.Resolve(Dictionary<int, Waffle.Interpreter.EnvValue> env) => _source.Resolve(env);
}

""");

        return IsgHelper.InsertIndent(sb.ToString(), indentLevel);
    }
}
