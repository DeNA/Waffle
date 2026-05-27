// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// A no-op command that does nothing.
/// </summary>
public class NopCommand : INopCommandInterpolation
{
    /// <inheritdoc/>
    public bool ShouldRemoveCommandOnlyLine => true;

    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly NopCommand Instance = new();

    private NopCommand()
    {
    }
}
