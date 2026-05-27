// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// Break command, valid only inside an iteration block.
/// </summary>
public class BreakIterationCommand : ICommandInterpolation
{
    /// <inheritdoc/>
    public bool ShouldRemoveCommandOnlyLine => true;

    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly BreakIterationCommand Instance = new();

    private BreakIterationCommand()
    {
    }
}
