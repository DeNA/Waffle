// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle;

/// <summary>
/// Interface for the execution context of a bakery runner.
/// </summary>
/// <remarks>
/// Extends <see cref="ITemplateInterpreterContext"/> with the concept of named output destinations.
/// </remarks>
public interface IBakeryContext : ITemplateInterpreterContext
{
    /// <summary>
    /// Opens the output destination identified by <paramref name="outputId"/>.
    /// </summary>
    void Open(string outputId);

    /// <summary>
    /// Closes the currently open output destination.
    /// </summary>
    void Close();

    /// <summary>
    /// Discards all generated output.
    /// </summary>
    void Clear();
}
