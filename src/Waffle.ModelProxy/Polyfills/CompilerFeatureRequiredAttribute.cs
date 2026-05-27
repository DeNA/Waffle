// (c) DeNA Co., Ltd.

#pragma warning disable
namespace System.Runtime.CompilerServices;

[AttributeUsage(
    validOn: AttributeTargets.All,
    AllowMultiple = true,
    Inherited = false)]
internal sealed class CompilerFeatureRequiredAttribute(string featureName) : Attribute
{
    public string FeatureName { get; } = featureName;
    public bool IsOptional { get; init; }
    public const string RefStructs = nameof(RefStructs);
    public const string RequiredMembers = nameof(RequiredMembers);
}
