// (c) DeNA Co., Ltd.

namespace Waffle.Interpreter;

/// <summary>
/// Interface for objects that can be initialized with an <see cref="IResolvableTo{T}"/> value.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Because abstract static interface members are not available on netstandard2.0,
/// this interface works around the limitation by being combined with a <c>new()</c> constraint
/// to enforce constructor-like initialization.
/// </remarks>
public interface ILazyInitializedBy<in T>
{
    /// <summary>
    /// Initializes the instance with a lazily resolved source.
    /// </summary>
    void Initialize(IResolvableTo<T> source);
}
