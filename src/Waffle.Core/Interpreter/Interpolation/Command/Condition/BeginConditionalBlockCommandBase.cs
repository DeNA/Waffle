// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Common base implementation for conditional block start command.
/// </summary>
public abstract class BeginConditionalBlockCommandBase : ICommandInterpolation
{
    /// <inheritdoc/>
    public bool ShouldRemoveCommandOnlyLine => true;

    /// <summary>
    /// Evaluates whether the condition for this block is satisfied.
    /// </summary>
    public abstract bool IsSatisfied(Dictionary<int, EnvValue> env);
}
