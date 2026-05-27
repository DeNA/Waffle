// (c) DeNA Co., Ltd.

namespace Waffle;

/// <summary>
/// Interface for a runner that registers multiple templates and processes them in batch.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TContext"></typeparam>
public interface IBakery<T, TContext>
    where TContext : IBakeryContext
    where T : IBakery<T, TContext>
{
    /// <summary>
    /// Initializes the bakery with the given context.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    T Initialize(TContext ctx);

    /// <summary>
    /// Registers a template.
    /// </summary>
    T Register(ITemplate<TContext> template);

    /// <summary>
    /// Executes all registered templates.
    /// </summary>
    T Run();
}
