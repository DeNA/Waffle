// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Command for the start of an Else block.
/// </summary>
public class ElseCommand : BeginConditionalBlockCommandBase
{
    /// <inheritdoc/>
    public override bool IsSatisfied(Dictionary<int, EnvValue> env)
    {
        return true;
    }
}
