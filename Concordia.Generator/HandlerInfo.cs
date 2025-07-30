using System.Collections.Generic;

namespace Concordia.Generator;

public record HandlerInfo(string ImplementationTypeName, List<string> ImplementedInterfaceTypeNames);