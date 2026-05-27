// (c) DeNA Co., Ltd.

namespace Waffle.ModelProxy;

/// <summary>
/// Static helper methods used by the <c>ModelProxy</c> source generator.
/// </summary>
internal static class ModelProxyGeneratorHelper
{
    /// <summary>
    /// Computes the output file name for the generated proxy class derived from the given fully qualified type name.
    /// </summary>
    /// <param name="originalFullTypeName">The fully qualified name of the original type.</param>
    /// <param name="ns">The namespace of the original type.</param>
    /// <returns>The file name (without extension) to use for the generated source file.</returns>
    internal static string GetOutputFileName(string originalFullTypeName, string ns)
    {
        var lastPeriod = originalFullTypeName.LastIndexOf('.');
        var nsEnd = originalFullTypeName.LastIndexOf(ns, StringComparison.Ordinal) + ns.Length;

        if (lastPeriod < 0 || nsEnd < 0)
        {
            return originalFullTypeName;
        }

        return originalFullTypeName[(nsEnd + 1)..] + "Proxy";
    }

    /// <summary>
    /// Computes the proxy type name as it should appear in <c>[ModelProxy]</c> attribute references.
    /// </summary>
    /// <param name="original">The fully qualified name of the original type.</param>
    /// <param name="isList">
    /// <see langword="true"/> to compute the name for the proxy-list wrapper;
    /// <see langword="false"/> for the plain proxy wrapper.
    /// </param>
    /// <param name="ns">The namespace of the original type.</param>
    /// <param name="parents">The containing type names (parent chain) of the original type, ordered outermost first.</param>
    /// <returns>The type name string suitable for use in attribute references.</returns>
    internal static string ProxyTypeNameForAttrNoted(string original, bool isList, string ns, string[] parents)
    {
        var lastPeriod = original.LastIndexOf('.');
        var nsEnd = original.LastIndexOf(ns, StringComparison.Ordinal) + ns.Length;
        var parentsText = parents.Length > 0
            ? string.Join(".", parents.Select(it => $"{it}Proxy")) + "."
            : "";

        if (lastPeriod < 0 || nsEnd < 0)
        {
            return isList
                ? $"{parentsText}{original}ProxyList"
                : $"{parentsText}{original}Proxy";
        }
        else
        {
            return isList
                ? $"{original[..(nsEnd + 1)]}{parentsText}{original[(lastPeriod + 1)..]}ProxyList"
                : $"{original[..(nsEnd + 1)]}{parentsText}{original[(lastPeriod + 1)..]}Proxy";
        }
    }
}
