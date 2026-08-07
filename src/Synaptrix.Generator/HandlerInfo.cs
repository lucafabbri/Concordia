using System.Collections.Generic;

namespace Synaptrix.Generator;

/// <summary>
/// The handler info
/// </summary>
/// <param name="ImplementationTypeName">
/// Fully-qualified implementation type name. For open-generic classes this is
/// the unbound form, e.g. <c>global::My.Behavior&lt;,&gt;</c>.
/// </param>
/// <param name="ImplementedInterfaceTypeNames">
/// Fully-qualified Synaptrix interface names the class implements.
/// For open-generic implementations they are emitted in unbound form, e.g.
/// <c>global::Synaptrix.IPipelineBehavior&lt;,&gt;</c>.
/// </param>
/// <param name="IsOpenGeneric">
/// <c>true</c> when the class is itself an open generic type and must be
/// registered with the <c>typeof(MyClass&lt;,&gt;)</c> overload of
/// <c>AddTransient</c>.
/// </param>
/// <param name="SkippedRegistrationNotes">
/// Pre-formatted single-line comments explaining any interfaces this open-generic
/// handler implements that could NOT be registered via the naive
/// <c>typeof(IFoo&lt;,&gt;)</c>/<c>typeof(Impl&lt;,&gt;)</c> mapping - either because the
/// class's own type-parameter count doesn't match the interface's arity, or because
/// (even with matching arity) an interface slot isn't a direct, positional reference to
/// one of the class's own type parameters (e.g. it's wrapped in another constructed
/// generic type, or the positions are swapped). .NET's open-generic DI resolution only
/// supports the direct, position-for-position case; emitting a registration for the
/// other shapes either throws at startup (arity mismatch) or silently fails to resolve
/// at the call site (wrapped/swapped slots) - see SynaptrixGenerator's
/// IsDirectPositionalTypeParameterMapping for the exact check. Empty when there's
/// nothing to skip.
/// </param>
public record HandlerInfo(
    string ImplementationTypeName,
    List<string> ImplementedInterfaceTypeNames,
    bool IsOpenGeneric = false,
    List<string>? SkippedRegistrationNotes = null);