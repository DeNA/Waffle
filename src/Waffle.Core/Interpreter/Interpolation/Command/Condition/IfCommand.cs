// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Command for the start of an If block.
/// </summary>
public class IfCommand(IResolvableTo<bool> cond) : BeginConditionalBlockCommandBase
{
    /// <inheritdoc/>
    public override bool IsSatisfied(Dictionary<int, EnvValue> env)
    {
        return cond.Resolve(env);
    }
}
