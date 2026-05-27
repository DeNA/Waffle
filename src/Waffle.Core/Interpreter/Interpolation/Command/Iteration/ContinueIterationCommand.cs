// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// Continue command, valid only inside an iteration block.
/// </summary>
public class ContinueIterationCommand : ICommandInterpolation
{
    /// <inheritdoc/>
    public bool ShouldRemoveCommandOnlyLine => true;

    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly ContinueIterationCommand Instance = new();

    private ContinueIterationCommand()
    {
    }
}
