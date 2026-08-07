# Synaptrix.Generator: Compile-Time Handler Registration for Synaptrix

> **Synaptrix** (formerly **Concordia**) — renamed starting from v3.0.0.

**Synaptrix.Generator** is the C# Source Generator component of the Synaptrix library. It automates the registration of your handlers at **compile-time** and generates a concrete `GeneratedMediator` class that dispatches requests and notifications with **zero runtime overhead** — no DI lookups, no reflection, no allocations on the hot path.

# Table of Contents
- [Why Synaptrix?](#why-synaptrix)
- [Key Features](#key-features)
- [GeneratedMediator Fallback for Unregistered Request Shapes](#generatedmediator-fallback-for-unregistered-request-shapes)
- [Performance](#performance)
- [Installation](#installation)
- [Usage](#usage)
- [Contribution](#contribution)
- [License](#license)
- [NuGet Packages](#nuget-packages)
- [Contact](#contact)
- [Support](#support)

## Why Synaptrix?

* **An Open-Source Alternative**: Synaptrix was created as an open-source alternative in response to other popular mediator libraries (like MediatR) transitioning to a paid licensing model. We believe core architectural patterns should remain freely accessible to the developer community.
* **Lightweight and Minimal**: Provides only the essential Mediator pattern functionalities, without unnecessary overhead.
* **Optimized Performance**: The Source Generator goes beyond simple handler registration — it produces a `GeneratedMediator` with constructor-injected handler singletons and direct `is`-type-switch dispatch, achieving **~1–2 ns per call with zero allocations** on the hot path.
* **Easy DI Integration**: Integrates seamlessly with `Microsoft.Extensions.DependencyInjection`.
* **Same MediatR Interfaces**: Uses interfaces with identical signatures to MediatR, making migration extremely straightforward.
* **CQRS and Pub/Sub Patterns**: Facilitates the implementation of Command Query Responsibility Segregation (CQRS) and Publisher/Subscriber principles, enhancing separation of concern and code maintainability.

## Key Features

* **Two generated files**:
  * `SynaptrixGeneratedHandlersRegistrations.g.cs` — an `AddSynaptrixHandlers()` DI extension method that registers all discovered handlers as Singletons and wires up `GeneratedMediator` as `IMediator`/`ISender`.
  * `SynaptrixGeneratedMediator.g.cs` — a sealed `GeneratedMediator` class with constructor-injected handlers, direct type-switch dispatch, and an `IsCompletedSuccessfully` fast-path for notifications.
* **Zero Registration Friction**: The `[assembly: DiscoverSynaptrixHandlers]` attribute is injected **automatically** via a `.targets` file — no manual setup required.
* **Cross-Assembly Discovery**: Handlers defined in referenced assemblies that also use `Synaptrix.Generator` are discovered and included automatically.
* **Configurable Method Name**: Rename the generated extension method via `<SynaptrixGeneratedMethodName>` in your `.csproj`.

## Known Limitation: Asymmetric-Arity Open Generics

An open-generic handler is normally registered with `services.AddTransient(typeof(IFoo<,>), typeof(Impl<,>))`, which binds the two type-parameter lists positionally — this requires the handler class and the interface it implements to have the **same arity** (number of type parameters).

Some handler shapes don't: a single type parameter can fill more than one slot on the interface, e.g.

```csharp
public class FetchTabularCommandHandler<TTabular> : IStreamRequestHandler<FetchTabularCommand<TTabular>, TTabular>
    where TTabular : class, ITabular
{
    // TTabular is used twice on the interface (inside FetchTabularCommand<TTabular> AND as TResponse),
    // but the handler class itself only has one type parameter: arity 1 vs arity 2.
}
```

This is a well-defined mapping conceptually (`T → (FetchTabularCommand<T>, T)`), but .NET's `typeof(...)`-based open-generic registration can't express it — registering it anyway throws at runtime:

```
System.ArgumentException: Arity of open generic service type 'IStreamRequestHandler`2[TRequest,TResponse]'
does not equal arity of open generic implementation type 'FetchTabularCommandHandler`1[TTabular]'.
```

...and since this happens inside `BuildServiceProvider()`, it fails the **entire DI container**, not just this one handler.

**Current behavior**: the generator detects the arity mismatch and **skips** emitting the registration for that handler/interface pair, leaving a `// Skipped: ... (arity mismatch)` comment in the generated source instead of code that would crash at startup. Handlers with this shape are **not auto-registered** — register them yourself (typically per closed `TTabular`, e.g. `services.AddTransient<IStreamRequestHandler<FetchTabularCommand<Product>, Product>, FetchTabularCommandHandler<Product>>()` for each concrete type you use) until a future version adds closed-type scanning for this case. Once registered, dispatching through `IMediator`/`ISender` works normally — see [GeneratedMediator Fallback](#generatedmediator-fallback-for-unregistered-request-shapes) below.

## Known Limitation: Wrapped-Slot Open Generics (matching arity, still unmappable)

A subtler variant of the same problem: the handler's type-parameter *count* matches the interface's arity, but one of the interface's slots isn't a *direct* reference to the handler's own type parameter — it's another constructed generic type wrapping it:

```csharp
public class FindEntitiesCommandHandler<TId, TEntity> : IStreamRequestHandler<FindEntitiesCommand<TId, TEntity>, TEntity>
    where TId : IEquatable<TId>
    where TEntity : class, IEntity<TId, TEntity>
{
    // Both sides have 2 slots (TId, TEntity), so the arity check alone lets this through.
    // But slot 0 is FindEntitiesCommand<TId, TEntity> - not TId itself.
}
```

`services.AddTransient(typeof(IStreamRequestHandler<,>), typeof(FindEntitiesCommandHandler<,>))` binds type-parameter lists **positionally**: resolving `IStreamRequestHandler<FindEntitiesCommand<string, Product>, Product>` makes .NET construct `FindEntitiesCommandHandler<FindEntitiesCommand<string, Product>, Product>` — substituting `TId` with `FindEntitiesCommand<string, Product>`, which fails `TId`'s own `IEquatable<TId>` constraint. Unlike the arity case, this doesn't throw at startup (`BuildServiceProvider()` doesn't inspect constraint satisfiability that deeply) — it just silently fails to resolve the handler the first time something actually dispatches through it, surfacing as `InvalidOperationException: No stream handler registered for ...` (or the request/notification equivalent) at that call site.

**Current behavior**: the generator checks, position-for-position, that each interface slot is *directly* one of the handler's own type parameters (not wrapped, not reordered) before registering. If not, it skips the same way as the arity case, with a `// Skipped: ...'s type parameters don't map directly, position-for-position, onto ...'s slots` comment, and — like the arity case — such handlers need registering per closed type until a future version adds closed-type scanning. Once registered, dispatching through `IMediator`/`ISender` works normally — see the next section.

## GeneratedMediator Fallback for Unregistered Request Shapes

`GeneratedMediator`'s dispatch methods (`Send`, `CreateStream`, `Publish`) are built from a compile-time-known switch over the request/notification types this project's own generator run discovered. Two situations put a request outside that switch even though a handler for it genuinely exists in the DI container:

* A handler shape the generator can't safely auto-register as a fast-path case (the two limitations above) but that you've registered by hand.
* A handler discovered and registered by a **different** referenced assembly's own `Synaptrix.Generator` run — cross-assembly discovery (see Key Features above) composes handler registrations from every discoverable referenced assembly into the same `IServiceCollection`, but each assembly's own `GeneratedMediator` only has switch cases for the handlers *it* saw at its own compile time.

For either case, falling straight to an exception the moment a type isn't recognized would mean whichever assembly's `GeneratedMediator` ends up bound as the app's `IMediator` becomes the sole determinant of what's dispatchable — silently stranding every handler known only to other assemblies in the reference graph.

**Current behavior**: when no switch case matches, `GeneratedMediator` falls back to a lazily-created `Synaptrix.Mediator` (the reflection/DI-based implementation) scoped to the same `IServiceProvider`, instead of throwing immediately. The fast, allocation-free switch still handles everything it knows about; anything else is resolved the same way `Synaptrix.Mediator` would resolve it on its own — succeeding if a handler is registered anywhere in the container, and producing the same "no handler found" exception as `Synaptrix.Mediator` only if it truly isn't.

## Performance

Benchmarks measured with BenchmarkDotNet on .NET 10, Intel Core i7-13800H. Ratio = relative to MediatR (1.00).

### Send Command (fire-and-forget, no pipeline)

| Method | Mean | Ratio | Allocated |
|---|---:|---:|---:|
| MediatR | 50.5 ns | 1.00 | 128 B |
| Synaptrix | 22.2 ns | 0.44 | 0 B |
| **SynaptrixGen** | **1.7 ns** | **0.03** | **0 B** |
| Martin | 5.8 ns | 0.11 | 0 B |

### Publish Notification (2 handlers)

| Method | Mean | Ratio | Allocated |
|---|---:|---:|---:|
| MediatR | 87.2 ns | 1.00 | 440 B |
| Synaptrix | 74.0 ns | 0.85 | 224 B |
| **SynaptrixGen** | **1.6 ns** | **0.02** | **0 B** |
| Martin | 7.9 ns | 0.09 | 0 B |

## Installation

Install the **Synaptrix** meta-package (recommended) or the individual packages:

```bash
dotnet add package Synaptrix
```

Or individually:

```bash
dotnet add package Synaptrix.Core
dotnet add package Synaptrix.Generator
```

## Usage

1. **Define your Handlers, Processors, and Behaviors** (as described in `Synaptrix.Core`'s documentation).

2. **Configure your `.csproj`**: Reference `Synaptrix.Generator` as `PrivateAssets="all"` so it is consumed only as a Roslyn analyzer and not exposed as a transitive dependency. Optionally set a custom method name:

    ```xml
    <Project Sdk="Microsoft.NET.Sdk.Web">
      <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
        <!-- Optional: rename the generated DI extension method -->
        <SynaptrixGeneratedMethodName>AddMyAppHandlers</SynaptrixGeneratedMethodName>
      </PropertyGroup>

      <ItemGroup>
        <CompilerVisibleProperty Include="SynaptrixGeneratedMethodName" />
      </ItemGroup>

      <ItemGroup>
        <PackageReference Include="Synaptrix.Core" Version="3.0.0" />
        <PackageReference Include="Synaptrix.Generator" Version="3.0.0" PrivateAssets="all" />
      </ItemGroup>
    </Project>
    ```

3. **Register services in `Program.cs`**:

    A single call to `AddSynaptrixHandlers()` (or your custom name) registers everything — handler Singletons **and** the `GeneratedMediator` wired as `IMediator`/`ISender`:

    ```csharp
    using Synaptrix;

    var builder = WebApplication.CreateBuilder(args);

    // Registers all handlers + GeneratedMediator as IMediator/ISender in one call.
    builder.Services.AddSynaptrixHandlers();

    builder.Services.AddControllers();
    var app = builder.Build();
    app.MapControllers();
    app.Run();
    ```

4. **Inject `IMediator` or `ISender`** in your controllers / services as usual:

    ```csharp
    public class ProductsController : ControllerBase
    {
        private readonly ISender _sender;
        public ProductsController(ISender sender) => _sender = sender;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
            => Ok(await _sender.Send(new GetProductByIdQuery { ProductId = id }));
    }
    ```

The generator automatically finds your handler implementations and generates two files. Look for them under **Dependencies → Analyzers → Synaptrix.Generator** in Solution Explorer.


## Contribution

Feel free to contribute to the project! Report bugs, suggest new features, or submit pull requests.
Please follow the [Contributing Guidelines](https://github.com/mrdevrobot/Synaptrix/blob/main/CONTRIBUTING.md).

## License

This project is released under the [MIT License](https://opensource.org/licenses/MIT). See the [LICENSE](https://github.com/mrdevrobot/Synaptrix/blob/main/LICENSE) file for more information.

## NuGet Packages
- [Synaptrix](https://www.nuget.org/packages/Synaptrix) — meta-package (recommended)
- [Synaptrix.Core](https://www.nuget.org/packages/Synaptrix.Core)
- [Synaptrix.Generator](https://www.nuget.org/packages/Synaptrix.Generator)

## Contact
For any questions, issues, or feedback, please open an issue on the [GitHub repository](https://github.com/mrdevrobot/Synaptrix/issues).

## Support
If you find this library useful, consider supporting its development: [Buy Me a Coffee](https://www.buymeacoffee.com/lucafabbriu).

