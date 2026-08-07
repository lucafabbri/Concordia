namespace Synaptrix.Examples.MultiAssembly.Catalog;

/// <summary>
/// A stream request handled entirely within this assembly - mirrors the shape that first
/// exposed the need for GeneratedMediator's fallback (a table/list view streaming rows).
/// </summary>
public class ListCatalogItemsQuery : IStreamRequest<string>
{
}

public class ListCatalogItemsQueryHandler : IStreamRequestHandler<ListCatalogItemsQuery, string>
{
    public async IAsyncEnumerable<string> Handle(ListCatalogItemsQuery request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return "widget";
        yield return "gadget";
        await Task.CompletedTask;
    }
}
