using System.Collections.Generic;

namespace Concordia.Generator;

/// <summary>
/// The handler info
/// </summary>
public record HandlerInfo(string ImplementationTypeName, List<string> ImplementedInterfaceTypeNames);