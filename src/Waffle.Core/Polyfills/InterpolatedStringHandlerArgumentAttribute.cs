// (c) DeNA Co., Ltd.

#pragma warning disable
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class InterpolatedStringHandlerArgumentAttribute(params string[] arguments) : Attribute
{
    public string[] Arguments { get; } = arguments;

    public InterpolatedStringHandlerArgumentAttribute(string argument) : this([argument])
    {
    }
}
