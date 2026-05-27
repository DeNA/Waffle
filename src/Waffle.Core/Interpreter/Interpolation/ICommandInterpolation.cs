// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// Marker interface for interpolations that act as template engine commands (such as <c>For</c>, <c>If</c>,
/// and <c>End</c>) rather than producing output content.
/// </summary>
public interface ICommandInterpolation
{
    /// <summary>
    /// Gets a value indicating whether the line containing this interpolation should be suppressed
    /// when it would consist solely of whitespace after the command is removed.
    /// </summary>
    bool ShouldRemoveCommandOnlyLine { get; }
}
