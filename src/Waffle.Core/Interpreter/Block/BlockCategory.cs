// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// Classifies the type of open block for dispatch purposes.
/// </summary>
public enum BlockCategory
{
    /// <summary>
    /// Represents a conditional block such as <c>If</c>, <c>Elif</c>, or <c>Else</c>.
    /// </summary>
    Conditional,

    /// <summary>
    /// Represents an iteration block such as <c>For</c> or <c>ForEach</c>.
    /// </summary>
    Iteration
}
