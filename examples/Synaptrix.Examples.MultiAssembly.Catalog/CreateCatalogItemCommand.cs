namespace Synaptrix.Examples.MultiAssembly.Catalog;

/// <summary>
/// A command handled entirely within this assembly.
/// </summary>
public class CreateCatalogItemCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
}

public class CreateCatalogItemCommandHandler : IRequestHandler<CreateCatalogItemCommand, int>
{
    public ValueTask<int> Handle(CreateCatalogItemCommand request, CancellationToken cancellationToken)
        => new(42);
}
