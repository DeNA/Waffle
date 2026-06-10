// (c) DeNA Co., Ltd.

namespace Waffle.Analyzer;

internal enum WaffleCommand
{
    None,

    // Opening block commands — push a frame onto the stack
    For,
    Forr,
    ForEach,
    ForEachNullable,
    If,

    // Block terminator
    End,

    // Mid-If commands — valid only inside an open If frame
    Elif,
    Else,

    // Iteration control — valid only inside a For/ForEach frame
    Break,
    Continue,
}

internal static class WaffleCommandExtensions
{
    extension(WaffleCommand cmd)
    {
        internal bool IsOpeningBlock =>
            cmd is WaffleCommand.For or WaffleCommand.Forr
                or WaffleCommand.ForEach or WaffleCommand.ForEachNullable
                or WaffleCommand.If;

        internal bool IsIterationBlock =>
            cmd is WaffleCommand.For or WaffleCommand.Forr
                or WaffleCommand.ForEach or WaffleCommand.ForEachNullable;
    }
}
