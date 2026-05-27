// (c) DeNA Co., Ltd.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Waffle;

/// <summary>
/// Common base implementation for a runner that registers multiple templates and processes them in batch.
/// </summary>
/// <typeparam name="T">The concrete class that inherits from this base.</typeparam>
/// <typeparam name="TContext">The context type that implements <see cref="IBakeryContext"/>.</typeparam>
public abstract class BakeryBase<T, TContext> : IBakery<T, TContext>
    where TContext : IBakeryContext
    where T : BakeryBase<T, TContext>
{
    /// <summary>
    /// Gets the current bakery context.
    /// </summary>
    protected TContext? Context { get; private set; }

    /// <summary>
    /// Gets the registered templates.
    /// </summary>
    protected readonly List<ITemplate<TContext>> Templates = new();

    /// <summary>
    /// Initializes the bakery. Must be called before <see cref="Run" />.
    /// </summary>
    public T Initialize(TContext ctx)
    {
        if (this is not T typed)
        {
            throw new InvalidCastException($"Invalid type argument configuration in {nameof(BakeryBase<T, TContext>)}");
        }

        Context = ctx;

        OnConfigure(Context);

        return typed;
    }

    /// <summary>
    /// Called after initialization to apply context-specific configuration.
    /// </summary>
    protected virtual void OnConfigure(TContext ctx)
    {
    }

    /// <summary>
    /// Registers a template.
    /// </summary>
    public T Register(ITemplate<TContext> template)
    {
        if (this is not T typed)
        {
            throw new InvalidCastException($"Invalid type argument configuration in {nameof(BakeryBase<T, TContext>)}");
        }

        Templates.Add(template);
        return typed;
    }

    /// <summary>
    /// Creates an instance of the specified template type and registers it.
    /// </summary>
    public T Register<TTemplate>(Action<TTemplate>? modifier = null) where TTemplate : class, ITemplate<TContext>, new()
    {
        var template = new TTemplate();
        modifier?.Invoke(template);
        return Register(template);
    }

    /// <summary>
    /// Scans the specified assembly for template classes decorated with <typeparamref name="TAttribute"/> and registers all matching instances.
    /// </summary>
    /// <remarks>Template classes must have a parameterless constructor and implement <see cref="ITemplate{TContext}"/>.</remarks>
    /// <param name="sourceAssembly">The assembly to scan.</param>
    /// <param name="filter">An optional additional filter predicate. Pass <c>null</c> to register all matching types.</param>
    public T RegisterAllByAttribute<TAttribute>(Assembly sourceAssembly, Predicate<TAttribute>? filter = null)
        where TAttribute : Attribute
    {
        if (this is not T typed)
        {
            throw new InvalidCastException($"Invalid type argument configuration in {nameof(BakeryBase<,>)}");
        }

        var invalidCtorTypes = new List<Type>();
        foreach (var t in sourceAssembly.GetTypes()
                     .Where(it => typeof(ITemplate<TContext>).IsAssignableFrom(it) &&
                                  !it.IsAbstract &&
                                  it.GetCustomAttribute<TAttribute>() is { } attr &&
                                  (filter?.Invoke(attr) ?? true)))
        {
            var ctor = t.GetConstructors().FirstOrDefault(it => it.GetParameters().Length == 0);
            if (ctor is null)
            {
                invalidCtorTypes.Add(t);
                continue;
            }

            var template = ctor.Invoke(Array.Empty<object>());
            Register((ITemplate<TContext>)template);
        }

        if (invalidCtorTypes.Count > 0)
        {
            throw new InvalidOperationException(
                $"The following types decorated with {typeof(TAttribute).Name} do not have a parameterless constructor: " +
                string.Join(", ", invalidCtorTypes.Select(t => t.FullName)));
        }

        return typed;
    }

    /// <summary>
    /// Executes all registered templates, generating code in memory.
    /// </summary>
    public T Run()
    {
        if (this is not T typed)
        {
            throw new InvalidCastException($"Invalid type argument configuration in {nameof(BakeryBase<T, TContext>)}");
        }

        if (Context is null)
        {
            throw new InvalidOperationException($"{typeof(T).Name} has not been initialized");
        }

        foreach (var t in Templates)
        {
            t.Process(Context);
        }

        return typed;
    }
}
