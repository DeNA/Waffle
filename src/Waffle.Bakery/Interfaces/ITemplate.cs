// (c) DeNA Co., Ltd.

using Waffle.Interpreter;

namespace Waffle;

/// <summary>
/// Interface that defines a template and its execution logic.
/// </summary>
public interface ITemplate<in TContext> where TContext : ITemplateInterpreterContext
{
    /// <summary>
    /// Processes the template and generates code output according to the template's definition.
    /// </summary>
    /// <param name="ctx">The generation context into which output is written.</param>
    void Process(TContext ctx);
}
