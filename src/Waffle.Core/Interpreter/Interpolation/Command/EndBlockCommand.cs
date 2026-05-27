// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// Command that closes a block (If/Elif/Else or iteration).
/// </summary>
public class EndBlockCommand : ICommandInterpolation
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly EndBlockCommand Instance = new();

    private EndBlockCommand()
    {
    }

    /// <inheritdoc/>
    public bool ShouldRemoveCommandOnlyLine => true;
}
