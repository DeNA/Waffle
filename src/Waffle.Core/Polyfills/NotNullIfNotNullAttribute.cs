// (c) DeNA Co., Ltd.

#pragma warning disable
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(
    validOn: AttributeTargets.Parameter |
             AttributeTargets.Property |
             AttributeTargets.ReturnValue,
    AllowMultiple = true)]
internal sealed class NotNullIfNotNullAttribute(string parameterName) : Attribute
{
    public string ParameterName { get; } = parameterName;
}
