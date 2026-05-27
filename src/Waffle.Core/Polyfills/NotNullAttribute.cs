// (c) DeNA Co., Ltd.

#pragma warning disable
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(
    validOn: AttributeTargets.Field |
             AttributeTargets.Parameter |
             AttributeTargets.Property |
             AttributeTargets.ReturnValue)]
internal sealed class NotNullAttribute : Attribute;
