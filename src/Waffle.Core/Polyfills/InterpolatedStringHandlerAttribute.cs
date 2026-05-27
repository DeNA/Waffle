// (c) DeNA Co., Ltd.

#pragma warning disable
namespace System.Runtime.CompilerServices;

[AttributeUsage(
    validOn: AttributeTargets.Class | AttributeTargets.Struct,
    Inherited = false)]
internal sealed class InterpolatedStringHandlerAttribute : Attribute;
