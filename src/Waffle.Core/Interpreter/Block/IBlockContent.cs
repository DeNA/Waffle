// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Interface for objects treated as elements within a template block.
/// </summary>
/// <remarks>
/// Implementations carry meaning after the block has been parsed. Whether an instance was
/// supplied as an interpolation object in the template is irrelevant.
/// </remarks>
public interface IBlockContent
{
    /// <summary>
    /// Resolves all content into a single concatenated string.
    /// </summary>
    /// <remarks>
    /// This method may be called multiple times by iteration blocks, so implementations
    /// must not mutate internal state.
    /// </remarks>
    EvalResult Evaluate(Dictionary<int, EnvValue> env, int alignment, string? format);
}
