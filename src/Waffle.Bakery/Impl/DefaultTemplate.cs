// (c) DeNA Co., Ltd.

namespace Waffle;

/// <summary>
/// Abstract base class for defining code generation templates in the default Bakery pipeline.
/// Subclass this to implement a template that produces a single output using <see cref="DefaultBakeryContext"/>.
/// </summary>
public abstract class DefaultTemplate : SingleOutputTemplate<DefaultBakeryContext>
{
}
