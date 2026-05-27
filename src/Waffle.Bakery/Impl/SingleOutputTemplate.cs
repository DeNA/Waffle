// (c) DeNA Co., Ltd.

using System;
using Waffle.Interpreter;

namespace Waffle;

/// <summary>
/// Base implementation of <see cref="ITemplate{TContext}"/> for templates that produce a single output.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public abstract class SingleOutputTemplate<TContext> : ITemplate<TContext> where TContext : IBakeryContext
{
    /// <summary>
    /// The unique identifier for the output produced by this template.
    /// </summary>
    protected abstract string OutputId { get; }

    /// <inheritdoc/>
    public void Process(TContext ctx)
    {
        try
        {
            ctx.Open(OutputId);
            OnPreProcess(ctx);
            ProcessImpl(ctx);
            OnPostProcess(ctx);
        }
        catch (Exception e)
        {
            ctx.Error(new TemplateError(e, e.Message, "", 0));
        }
        finally
        {
            ctx.Close();
        }
    }

    /// <summary>
    /// Called immediately before <see cref="ProcessImpl"/>.
    /// </summary>
    protected virtual void OnPreProcess(TContext ctx)
    {
    }

    /// <summary>
    /// Performs the actual code generation. The output destination is already opened; call <c>ctx.Append</c> directly.
    /// </summary>
    protected abstract void ProcessImpl(TContext ctx);

    /// <summary>
    /// Called immediately after <see cref="ProcessImpl"/>.
    /// </summary>
    protected virtual void OnPostProcess(TContext ctx)
    {
    }
}
