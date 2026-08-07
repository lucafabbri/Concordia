using Microsoft.Extensions.DependencyInjection;
using Synaptrix.Examples.MultiAssembly.Catalog;
using Synaptrix.Examples.MultiAssembly.Shipping;

namespace Synaptrix.Examples.MultiAssembly.Host;

/// <summary>
/// End-to-end regression coverage for cross-assembly handler composition - the scenario the
/// generator-source unit tests in Synaptrix.Generator.Tests can't exercise, because those run
/// the generator against one fake in-memory compilation rather than real, separately-compiled
/// assemblies. Catalog and Shipping are two class libraries with their own local handlers and
/// their own Synaptrix.Generator run each; Catalog references Shipping, and Host below
/// references only Catalog. Only Catalog's own generated registration method is called below;
/// its generator-emitted chain discovers Shipping through that reference (both assemblies get
/// the auto-discovery marker attribute by default) and registers it too, which rebinds
/// IMediator/ISender to whichever of the two registers last. That assembly's own
/// GeneratedMediator has switch cases only for handlers it saw at its own compile time - the
/// other assembly's request types must reach their handlers through GeneratedMediator's
/// DI-based fallback. These tests send one request type from each assembly, so regardless of
/// which one ends up bound to IMediator, at least one of them is only reachable if the fallback
/// still works.
/// </summary>
public class CrossAssemblyDispatchTests
{
    private static IServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        Synaptrix.Examples.MultiAssembly.Catalog.Generated.SynaptrixGeneratedRegistrations.AddSynaptrixHandlers(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Command_HandledInShippingAssembly_DispatchesThroughIMediator()
    {
        var sp = BuildProvider();
        var mediator = sp.GetRequiredService<Synaptrix.IMediator>();

        var result = await mediator.Send(new CreateShipmentCommand { OrderId = 7 });

        Assert.Equal("SHIP-7", result);
    }

    [Fact]
    public async Task Command_HandledInCatalogAssembly_DispatchesThroughIMediator()
    {
        var sp = BuildProvider();
        var mediator = sp.GetRequiredService<Synaptrix.IMediator>();

        var result = await mediator.Send(new CreateCatalogItemCommand { Name = "Widget" });

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task StreamRequest_HandledInCatalogAssembly_DispatchesThroughIMediator()
    {
        var sp = BuildProvider();
        var mediator = sp.GetRequiredService<Synaptrix.IMediator>();

        var items = new List<string>();
        await foreach (var item in mediator.CreateStream(new ListCatalogItemsQuery()))
            items.Add(item);

        Assert.Equal(new[] { "widget", "gadget" }, items);
    }
}
