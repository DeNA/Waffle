// (c) DeNA Co., Ltd.

using System;

namespace Waffle.Interpreter;

/// <summary>
/// An error that occurred during template parsing or code generation.
/// </summary>
/// <remarks>
/// Not thrown; stored in the context instead. How to handle it is up to the caller.
/// </remarks>
public readonly struct TemplateError
{
    /// <summary>
    /// The message to display to the user.
    /// </summary>
    public readonly string Message;

    /// <summary>
    /// The underlying C# exception, if applicable.
    /// </summary>
    public readonly Exception? InnerException;

    /// <summary>
    /// The source file where the error originated.
    /// </summary>
    public readonly string CallerFilePath;

    /// <summary>
    /// The line number in the source file where the error originated.
    /// </summary>
    public readonly int CallerLineNumber;

    /// <summary>
    /// Initializes an error with an exception and an explicit message.
    /// </summary>
    public TemplateError(Exception innerException, string message, string callerFilePath, int callerLineNumber)
    {
        Message = message;
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
        InnerException = innerException;
    }

    /// <summary>
    /// Initializes an error from an exception.
    /// </summary>
    public TemplateError(Exception innerException, string callerFilePath, int callerLineNumber)
    {
        Message = innerException.Message;
        InnerException = innerException;
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
    }

    /// <summary>
    /// Initializes an error with a message and source location.
    /// </summary>
    public TemplateError(string message, string callerFilePath, int callerLineNumber)
    {
        Message = message;
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
        InnerException = null;
    }
}
