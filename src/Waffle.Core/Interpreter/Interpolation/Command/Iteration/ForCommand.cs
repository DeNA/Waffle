// (c) DeNA Co., Ltd.

using System.Collections.Generic;

namespace Waffle.Interpreter;

/// <summary>
/// Command that begins a For loop block.
/// </summary>
public class ForCommand : IterationCommandBase
{
    private readonly IResolvableTo<int> _fromInclusive;
    private readonly IResolvableTo<int> _toExclusive;
    private readonly bool _reverse;

    internal IResolvableTo<int> FromInclusive => _fromInclusive;
    internal IResolvableTo<int> ToExclusive => _toExclusive;
    internal bool Reverse => _reverse;

    /// <summary>
    /// Initializes a command that iterates over an integer range.
    /// </summary>
    public ForCommand(
        int id,
        IResolvableTo<int> fromInclusive,
        IResolvableTo<int> toExclusive,
        bool reverse,
        out IntProxy i) : base(id)
    {
        _fromInclusive = fromInclusive;
        _toExclusive = toExclusive;
        _reverse = reverse;
        i = new IntProxy(new IntIteratorProxy(Id));
    }

    internal override void Iterate(Dictionary<int, EnvValue> env, in TemplateEvaluator.IterationBlockEvaluator state)
    {
        // NOTE: This method is never called because TemplateEvaluator.EvaluateRange specializes it
        throw new System.NotImplementedException(
            $"The {nameof(Iterate)} method of {nameof(ForCommand)} is not implemented because" +
            $" {nameof(TemplateEvaluator)} specializes it. If this exception is thrown, it means that" +
            $" the specialization logic in {nameof(TemplateEvaluator)} is not working as intended.");
    }
}
