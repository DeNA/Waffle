// (c) DeNA Co., Ltd.

using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Waffle.Interpreter;

/// <summary>
/// Provides helper methods used by template interpreters.
/// </summary>
public static class TemplateInterpreterHelper
{
    /// <summary>
    /// Formats a value using the same rules as a standard interpolated string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FormatByDefault<T>(T x, int alignment, string? format)
    {
        var isNoFormat = format is null or { Length: 0 };

        string str;
        if (isNoFormat)
        {
            str = x?.ToString() ?? "";
        }
        else if (x is IFormattable formattable)
        {
            str = formattable.ToString(format, CultureInfo.CurrentCulture);
        }
        else
        {
            str = x?.ToString() ?? "";
        }

        if (alignment == 0)
        {
            return str;
        }

        return alignment > 0
            ? str.PadLeft(alignment)
            : str.PadRight(-alignment);
    }
}
