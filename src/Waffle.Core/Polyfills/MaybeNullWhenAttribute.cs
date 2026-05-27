// (c) DeNA Co., Ltd.

#pragma warning disable
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(
    validOn: AttributeTargets.Parameter,
    AllowMultiple = true)]
internal sealed class MaybeNullWhenAttribute(bool returnValue) : Attribute
{
    public bool ReturnValue { get; } = returnValue;
}
