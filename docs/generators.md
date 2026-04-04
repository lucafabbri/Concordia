---
layout: default
title: Source Generators
---

# Source Generators: The Engine of Concordia

Concordia distinguishes itself from many other .NET Mediator implementations by relying heavily on **C# Source Generators**. This technology allows us to inspect your code *during compilation* and generate the necessary wiring code automatically.

## How it Works

1. **Analysis**: The `Concordia.Generator` analyzer runs continuously in the background (within Visual Studio/Rider) or during the build process (`dotnet build`).
2. **Discovery**: It scans your project for classes implementing `IRequestHandler<>`, `INotificationHandler<>`, `IPipelineBehavior<>`, etc.
3. **Synthesis**: It generates **two C# files** compiled directly into your assembly:
   - `ConcordiaGeneratedHandlersRegistrations.g.cs` — the DI extension method.
   - `ConcordiaGeneratedMediator.g.cs` — the `GeneratedMediator` class.

The `[assembly: DiscoverConcordiaHandlers]` attribute that triggers the generator is injected **automatically** by the NuGet package via a `.targets` file — you don't need to add it manually.

## Generated Files

### `ConcordiaGeneratedHandlersRegistrations.g.cs`

Contains the `AddConcordiaHandlers()` extension method (or your custom name). Calling it:
- Registers every discovered handler, processor, and behavior as a **Singleton**.
- Registers `GeneratedMediator` as a Singleton.
- Wires `IMediator` and `ISender` to resolve `GeneratedMediator`.

```csharp
// Auto-generated
public static IServiceCollection AddConcordiaHandlers(this IServiceCollection services)
{
    services.AddSingleton<IRequestHandler<GetFooQuery, FooDto>, GetFooHandler>();
    services.AddSingleton<INotificationHandler<FooCreated>, SendEmailHandler>();
    // ...

    services.AddSingleton<GeneratedMediator>();
    services.AddSingleton<IMediator>(sp => sp.GetRequiredService<GeneratedMediator>());
    services.AddSingleton<ISender>(sp => sp.GetRequiredService<GeneratedMediator>());
    return services;
}
```

### `ConcordiaGeneratedMediator.g.cs`

Contains the `GeneratedMediator` sealed class — a concrete `IMediator`/`ISender` where:

- **Handler singletons are constructor-injected** (no `IServiceProvider` lookup on the hot path).
- **`Send<TResponse>`** uses a direct `is`-type-switch with cast — no reflection, no dictionary lookup, no boxing.
- **`Publish`** uses inline sequential dispatch with an `IsCompletedSuccessfully` fast-path: if all handlers complete synchronously, the method returns `Task.CompletedTask` without any allocations.

```csharp
// Auto-generated (simplified)
public sealed partial class GeneratedMediator : IMediator, ISender
{
    private readonly IRequestHandler<GetFooQuery, FooDto> _handler1;
    private readonly INotificationHandler<FooCreated> _handler2;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        if (request is GetFooQuery q) return (Task<TResponse>)(object)_handler1.Handle(q, ct);
        throw new InvalidOperationException("No handler found for " + request.GetType().Name);
    }

    public Task Publish(INotification notification, CancellationToken ct = default)
    {
        if (notification is FooCreated n)
        {
            var t = _handler2.Handle(n, ct);
            return t.IsCompletedSuccessfully ? Task.CompletedTask : t;
        }
        throw new InvalidOperationException("No handler found for " + notification.GetType().Name);
    }
}
```

## Performance vs. Reflection

Legacy libraries (like older versions of MediatR) typically use `Assembly.GetExecutingAssembly().GetTypes()` at startup to find handlers.

| Metric | Reflection (Legacy) | Source Generators (Concordia) |
| :--- | :--- | :--- |
| **Startup Cost** | O(N) where N is assembly size | **Zero** (O(1)) |
| **Memory Usage** | High (loading all types metadata) | Low (only necessary types) |
| **Safety** | Runtime Errors (Missing Dependencies) | **Compile-Time** Safety |
| **Trimming** | Hard / Requires directives | **Native Support** |

### Hot-Path Benchmark Results

Measured with BenchmarkDotNet on .NET 10, Intel Core i7-13800H. Ratio = relative to MediatR.

**Send Command (fire-and-forget)**

| Method | Mean | Ratio | Allocated |
| :--- | ---: | ---: | ---: |
| MediatR | 50.5 ns | 1.00 | 128 B |
| Concordia (reflection) | 22.2 ns | 0.44 | 0 B |
| **ConcordiaGen** | **1.7 ns** | **0.03** | **0 B** |
| Martin | 5.8 ns | 0.11 | 0 B |

**Publish Notification (2 handlers)**

| Method | Mean | Ratio | Allocated |
| :--- | ---: | ---: | ---: |
| MediatR | 87.2 ns | 1.00 | 440 B |
| Concordia (reflection) | 74.0 ns | 0.85 | 224 B |
| **ConcordiaGen** | **1.6 ns** | **0.02** | **0 B** |
| Martin | 7.9 ns | 0.09 | 0 B |

`ConcordiaGen` achieves near-zero cost because the `IsCompletedSuccessfully` fast-path avoids async state machine allocation and the entire publisher interface indirection.

## Advanced Configuration

### Custom Method Name

By default, the generator creates a method named `AddConcordiaHandlers`. You can change this:

```xml
<PropertyGroup>
    <ConcordiaGeneratedMethodName>AddMyModuleHandlers</ConcordiaGeneratedMethodName>
</PropertyGroup>
<ItemGroup>
    <CompilerVisibleProperty Include="ConcordiaGeneratedMethodName" />
</ItemGroup>
```

### Inspecting Generated Code

To see exactly what Concordia is generating for you:
1. Open **Dependencies** in Solution Explorer.
2. Go to **Analyzers** → **Concordia.Generator**.
3. Expand **Concordia.Generator.ConcordiaGenerator**.
4. Double-click either generated file:
   - `ConcordiaGeneratedHandlersRegistrations.g.cs`
   - `ConcordiaGeneratedMediator.g.cs`

You will see standard, readable C# code that you can inspect, understand, and debug.

## Using `GeneratedMediator` Without the Full DI Setup

If you need the generated mediator outside of a DI container (e.g., in tests or benchmarks), you can instantiate it directly by providing handler instances manually:

```csharp
var mediator = new ConcordiaGenerated.Generated.GeneratedMediator(
    new GetFooQueryHandler(),
    new SendEmailHandler()
);

var result = await mediator.Send(new GetFooQuery { Id = 1 });
```

