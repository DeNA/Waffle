// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;
using Waffle.Interpreter;

namespace Waffle;

/// <summary>
/// Default implementation of <see cref="IBakery{T,TContext}"/>.
/// </summary>
public class DefaultBakery : BakeryBase<DefaultBakery, DefaultBakeryContext>
{
    /// <summary>
    /// Returns all generated outputs.
    /// </summary>
    public Dictionary<string, string> GetResults()
    {
        if (Context is null)
        {
            throw new InvalidOperationException($"{nameof(DefaultBakery)} has not been initialized");
        }

        return Context.GetResults();
    }

    /// <summary>
    /// Returns all errors that occurred during code generation.
    /// </summary>
    public Dictionary<string, IReadOnlyList<TemplateError>> GetErrors()
    {
        if (Context is null)
        {
            throw new InvalidOperationException($"{nameof(DefaultBakery)} has not been initialized");
        }

        return Context.GetErrors();
    }
}
