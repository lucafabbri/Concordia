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
public record HandlerInfo(
    string ImplementationTypeName,
    List<string> ImplementedInterfaceTypeNames,
    bool IsOpenGeneric = false);