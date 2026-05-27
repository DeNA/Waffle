// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// Flow control signal to propagate break/continue of iteration block.
/// </summary>
internal enum FlowControl
{
    Normal,
    Break,
    Continue
}
