// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Waffle.Interpreter;

namespace Waffle;

/// <summary>
/// Base implementation of <see cref="IBakeryContext"/> backed by in-memory output buffers.
/// </summary>
public abstract class BakeryContextBase : IBakeryContext
{
    /// <summary>
    /// Stores generated output keyed by output ID.
    /// </summary>
    protected readonly Dictionary<string, StringBuilder> Builders = new();

    /// <summary>
    /// Stores template errors keyed by output ID.
    /// </summary>
    protected readonly Dictionary<string, List<TemplateError>> Errors = new();

    /// <summary>
    /// The ID of the output currently being written.
    /// </summary>
    protected string? CurrentOutputId { get; private set; }

    /// <summary>
    /// The <see cref="StringBuilder"/> currently being written to.
    /// </summary>
    protected StringBuilder? CurrentBuilder { get; private set; }

    /// <summary>
    /// Opens the output identified by <paramref name="outputId"/> for writing.
    /// </summary>
    public void Open(string outputId)
    {
        var isNew = !Builders.TryGetValue(outputId, out var builder);

        OnPreOpen(outputId, isNew);

        if (isNew)
        {
            builder = new StringBuilder(512);
            Builders[outputId] = builder;
        }

        CurrentBuilder = builder;
        CurrentOutputId = outputId;

        OnPostOpen(outputId, isNew);
    }

    /// <summary>
    /// Called before an output is opened.
    /// </summary>
    protected virtual void OnPreOpen(string outputId, bool isNew)
    {
    }

    /// <summary>
    /// Called after an output is opened.
    /// </summary>
    protected virtual void OnPostOpen(string outputId, bool isNew)
    {
    }

    /// <summary>
    /// Appends a string to the currently open output.
    /// </summary>
    public void Append(string value)
    {
        ThrowIfNotOpened();
        OnPreAppend(value);
        CurrentBuilder!.Append(value);
        OnPostAppend(value);
    }

    /// <summary>
    /// Called before text is appended to the current output.
    /// </summary>
    protected virtual void OnPreAppend(string value)
    {
    }

    /// <summary>
    /// Called after text is appended to the current output.
    /// </summary>
    protected virtual void OnPostAppend(string value)
    {
    }

    /// <inheritdoc/>
    public void Error(in TemplateError error)
    {
        ThrowIfNotOpened();
        if (CurrentOutputId is null)
        {
            return;
        }

        if (!Errors.TryGetValue(CurrentOutputId, out var errors))
        {
            errors = new List<TemplateError>();
            Errors[CurrentOutputId] = errors;
        }

        errors.Add(error);
    }

    /// <inheritdoc/>
    public void OnHandlerCreated(int literalLength, int formattedCount, TemplateInterpreterController controller)
    {
    }

    /// <summary>
    /// Closes the currently open output.
    /// </summary>
    public void Close()
    {
        var currentOutputId = CurrentOutputId;
        if (currentOutputId is not null)
        {
            OnPreClose(currentOutputId);
        }

        CurrentBuilder = null;
        CurrentOutputId = null;
        if (currentOutputId is not null)
        {
            OnPostClose(currentOutputId);
        }
    }

    /// <summary>
    /// Called before the current output is closed.
    /// </summary>
    protected virtual void OnPreClose(string outputId)
    {
    }

    /// <summary>
    /// Called after the current output is closed.
    /// </summary>
    protected virtual void OnPostClose(string outputId)
    {
    }

    /// <summary>
    /// Clears all generated output.
    /// </summary>
    public void Clear()
    {
        Builders.Clear();
        CurrentBuilder = null;
        CurrentOutputId = null;

        OnCleared();
    }

    /// <summary>
    /// Called before a literal string is appended.
    /// </summary>
    public virtual void OnPreAppendLiteral(ref string willBeAppended, TemplateInterpreterController controller)
    {
    }

    /// <summary>
    /// Called after a literal string has been appended.
    /// </summary>
    public virtual void OnPostAppendLiteral(string appended, TemplateInterpreterController controller)
    {
    }

    /// <summary>
    /// Called before a formatted interpolation value is appended.
    /// </summary>
    public virtual void OnPreAppendFormatted<T>(
        ref T x, ref int alignment, ref string? format, TemplateInterpreterController controller)
    {
    }

    /// <summary>
    /// Attempts to handle an interpolated value not handled by the default AppendFormatted logic.
    /// </summary>
    /// <returns><c>true</c> if the value was handled; <c>false</c> to fall back to the default <c>ToString</c>-based interpolation.</returns>
    /// <remarks>Objects that implement <see cref="IBlockContent"/> are handled automatically; overriding this method is only needed for custom types.</remarks>
    public virtual bool TryHandleUnhandledInterpolation<T>(
        T x, int alignment, string? format, TemplateInterpreterController controller)
    {
        return false;
    }

    /// <summary>
    /// Called after a formatted interpolation value has been appended.
    /// </summary>
    public virtual void OnPostAppendFormatted<T>(T x, TemplateInterpreterController controller)
    {
    }

    /// <inheritdoc/>
    public void OnCompleted(TemplateInterpreterController controller)
    {
    }

    /// <summary>
    /// Called after all outputs are cleared.
    /// </summary>
    protected virtual void OnCleared()
    {
    }

    /// <summary>
    /// Returns the generated output for the specified output ID.
    /// </summary>
    public string GetResult(string outputId)
    {
        return Builders[outputId].ToString();
    }

    /// <summary>
    /// Tries to get the generated output for the specified output ID.
    /// </summary>
    public bool TryGetResult(string outputId, out string content)
    {
        if (Builders.TryGetValue(outputId, out var sb))
        {
            content = sb.ToString();
            return true;
        }

        content = "";
        return false;
    }

    /// <summary>
    /// Returns all generated outputs as a dictionary keyed by output ID.
    /// </summary>
    public Dictionary<string, string> GetResults()
    {
        return Builders.ToDictionary(it => it.Key, it => it.Value.ToString());
    }

    /// <summary>
    /// Returns all errors that occurred during code generation, keyed by output ID.
    /// </summary>
    public Dictionary<string, IReadOnlyList<TemplateError>> GetErrors()
    {
        return Errors.ToDictionary(it => it.Key, it => it.Value as IReadOnlyList<TemplateError>);
    }

    /// <summary>
    /// Throws if no output is currently open.
    /// </summary>
    protected void ThrowIfNotOpened()
    {
        if (CurrentBuilder is null || CurrentOutputId is null)
        {
            throw new InvalidOperationException("No output target has been specified");
        }
    }
}
