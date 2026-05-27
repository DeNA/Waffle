// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// Helper available as a ForEach out parameter in indexed loops.
/// </summary>
public class IndexedLoopHelper(IntProxy count, IntProxy i)
{
    /// <summary>
    /// Returns one of two values depending on whether the current element is the first in the loop.
    /// </summary>
    public IResolvableTo<T> FirstOrNot<T>(T first, T notFirst)
    {
        return i.To((notFirst, first), (it, ctx) => it == 0 ? ctx.first : ctx.notFirst);
    }

    /// <summary>
    /// Returns one of two values depending on whether the current element is the last in the loop.
    /// </summary>
    public IResolvableTo<T> LastOrNot<T>(T last, T notLast)
    {
        return i.With(count, (last, notLast), (i2, c, ctx) => i2 == c - 1 ? ctx.last : ctx.notLast);
    }

    /// <summary>
    /// Outputs <c>")"</c> on the last iteration and <c>","</c> on all others.
    /// </summary>
    public IResolvableTo<string> CommaOrLastParen => LastOrNot(")", ",");

    /// <summary>
    /// Outputs <c>")"</c> on the last iteration and <c>", "</c> on all others.
    /// </summary>
    public IResolvableTo<string> CommaSpaceOrLastParen => LastOrNot(")", ", ");

    /// <summary>
    /// Outputs <c>","</c> on every iteration except the last.
    /// </summary>
    public IResolvableTo<string> CommaOrLastEmpty => LastOrNot("", ",");

    /// <summary>
    /// Outputs <c>", "</c> on every iteration except the last.
    /// </summary>
    public IResolvableTo<string> CommaSpaceOrLastEmpty => LastOrNot("", ", ");
}
