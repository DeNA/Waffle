// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Base implementation shared by all iteration block start commands.
/// </summary>
public abstract class IterationCommandBase(int id) : ICommandInterpolation
{
    /// <summary>
    /// Iteration block id
    /// </summary>
    public int Id => id;

    /// <inheritdoc/>
    public bool ShouldRemoveCommandOnlyLine => true;

    internal abstract void Iterate(Dictionary<int, EnvValue> env, in TemplateEvaluator.IterationBlockEvaluator state);
}
