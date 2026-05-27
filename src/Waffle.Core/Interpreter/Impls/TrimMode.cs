// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// Specifies how whitespace trimming is applied adjacent to an interpolation.
/// </summary>
public enum TrimMode
{
    /// <summary>
    /// Do not trim whitespace.
    /// </summary>
    None,

    /// <summary>
    /// Trim whitespace but stop at a newline character.
    /// </summary>
    NoLineBreak,

    /// <summary>
    /// Trim whitespace including newline characters.
    /// </summary>
    WithLineBreak,
}
