// (c) DeNA Co., Ltd.

#pragma warning disable
namespace System;

using Runtime.CompilerServices;

internal readonly struct Range(Index start, Index end)
{
    public Index Start { get; } = start;
    public Index End { get; } = end;

    public static Range StartAt(Index start) => new(start, Index.End);
    public static Range EndAt(Index end) => new(Index.Start, end);
    public static Range All => new(Index.Start, Index.End);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int Offset, int Length) GetOffsetAndLength(int length)
    {
        int start = Start.IsFromEnd ? length - Start.Value : Start.Value;
        int end = End.IsFromEnd ? length - End.Value : End.Value;
        if ((uint)end > (uint)length || (uint)start > (uint)end)
            throw new ArgumentOutOfRangeException(nameof(length));
        return (start, end - start);
    }

    public override string ToString() => $"{Start}..{End}";
}
