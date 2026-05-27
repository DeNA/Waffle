// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// Marker interface for commands that have no effect on the formatter output (no-ops).
/// </summary>
public interface INopCommandInterpolation : ICommandInterpolation
{
}
