// (c) DeNA Co., Ltd.

#pragma warning disable
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(
    validOn: AttributeTargets.Parameter |
             AttributeTargets.Field |
             AttributeTargets.Property)]
internal sealed class StringSyntaxAttribute(string syntax, params object?[] arguments) : Attribute
{
    public StringSyntaxAttribute(string syntax) : this(syntax, [])
    {
    }

    public string Syntax { get; } = syntax;
    public object?[] Arguments { get; } = arguments;

    public const string CSharp = "C#";
}
