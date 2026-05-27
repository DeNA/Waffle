// (c) DeNA Co., Ltd.

using System.Collections.Immutable;

namespace Waffle.ModelProxy;

/// <summary>
/// Tuple member information extracted from a target type.
/// </summary>
internal readonly record struct TupleInfo(
    string ProxyType,
    ImmutableArray<MemberInfo> Fields)
{
    public string ProxyListType => ProxyType + "List";

    public bool Equals(TupleInfo other)
    {
        return ProxyType == other.ProxyType && Fields.SequenceEqual(other.Fields);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (ProxyType.GetHashCode() * 397) ^ Fields.GetHashCode();
        }
    }
}
