# Synaptrix: A Lightweight and Powerful .NET Mediator

> **Synaptrix** is the new name for **Concordia**, starting from version **3.0.0**.
> All NuGet packages have been renamed from `Concordia.*` / `Synaptrix.*` (v2.x) to the unified `Synaptrix` family.
> See [Migrating from Concordia](#migrating-from-concordia) for details.

Synaptrix is a .NET library implementing the **Mediator pattern**, designed to be lightweight, performant, and easily integrated with the .NET Dependency Injection system. It leverages **C# Source Generators** for automatic handler registration at compile-time, eliminating the need for runtime reflection and improving application startup performance.

|Project|NuGet Downloads|NuGet Version|
|---|---|---|
| Synaptrix | ![NuGet Downloads](https://img.shields.io/nuget/dt/Synaptrix?cacheSeconds=600) | ![NuGet Version](https://img.shields.io/nuget/v/Synaptrix) |
| Synaptrix.Core | ![NuGet Downloads](https://img.shields.io/nuget/dt/Synaptrix.Core?cacheSeconds=600) | ![NuGet Version](https://img.shields.io/nuget/v/Synaptrix.Core) |
| Synaptrix.Generator | ![NuGet Downloads](https://img.shields.io/nuget/dt/Synaptrix.Generator?cacheSeconds=600) | ![NuGet Version](https://img.shields.io/nuget/v/Synaptrix.Generator) |


## Table of Contents
- [Synaptrix: A Lightweight and Powerful .NET Mediator](#synaptrix-a-lightweight-and-powerful-net-mediator)
  - [Table of Contents](#table-of-contents)
  - [Performance Benchmarks](#performance-benchmarks)
    - [Send Command (fire-and-forget, no pipeline)](#send-command-fire-and-forget-no-pipeline)
    - [Send Query (returns response, no pipeline)](#send-query-returns-response-no-pipeline)
    - [Publish Notification (2 handlers, no pipeline)](#publish-notification-2-handlers-no-pipeline)
  - [Why Synaptrix?](#why-synaptrix)
  - [Key Features](#key-features)
  - [Installation](#installation)
  - [Usage](#usage)
    - [1. Define Requests, Commands, and Notifications](#1-define-requests-commands-and-notifications)
    - [2. Define Handlers, Processors, and Behaviors](#2-define-handlers-processors-and-behaviors)
    - [3. Register Services in `Program.cs`](#3-register-services-in-programcs)
      - [Configure your `.csproj`](#configure-your-csproj)
      - [Register in `Program.cs`](#register-in-programcs)
      - [What the generator produces (for reference)](#what-the-generator-produces-for-reference)
  - [Migrating from Concordia](#migrating-from-concordia)
    - [What changed in v3.0.0](#what-changed-in-v300)
    - [How to migrate](#how-to-migrate)
    - [MediatR Compatibility (`Concordia.MediatR`)](#mediatr-compatibility-concordiamediatr)
  - [Migration Guide from MediatR](#migration-guide-from-mediatr)
    - [1. Update NuGet Packages](#1-update-nuget-packages)
    - [2. Update Namespaces](#2-update-namespaces)
    - [3. Update Service Registration in `Program.cs`](#3-update-service-registration-in-programcs)
  - [Contributing](#contributing)
  - [License](#license)
  - [NuGet Packages](#nuget-packages)
  - [Contact](#contact)
  - [Support](#support)


-----

## Performance Benchmarks

The following benchmarks compare Synaptrix (reflection-based `Mediator`), **SynaptrixGen** (`GeneratedMediator` produced by the Source Generator), [MediatR](https://github.com/jbogard/MediatR) and [Martin](https://github.com/martinothamar/Mediator) — measured with [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) on .NET 10, Intel Core i7-13800H.

> **Smaller is better.** Ratio is relative to MediatR (1.00). `0 B` allocated means no heap allocation on the hot path.

### Send Command (fire-and-forget, no pipeline)

| Method | Mean | Ratio | Allocated | Alloc Ratio |
|---|---:|---:|---:|---:|
| MediatR | 40.5 ns | 1.00 | 128 B | 1.00 |
| Synaptrix | 21.8 ns | 0.54 | 0 B | 0.00 |
| **SynaptrixGen** | **4.4 ns** | **0.11** | **0 B** | **0.00** |
| Martin | 4.4 ns | 0.11 | 0 B | 0.00 |

### Send Query (returns response, no pipeline)

| Method | Mean | Ratio | Allocated | Alloc Ratio |
|---|---:|---:|---:|---:|
| MediatR | 56.0 ns | 1.00 | 240 B | 1.00 |
| Synaptrix | 49.3 ns | 0.88 | 40 B | 0.17 |
| **SynaptrixGen** | **25.8 ns** | **0.46** | **40 B** | **0.17** |
| Martin | 16.4 ns | 0.29 | 40 B | 0.17 |

> The 40 B allocation in queries is inherent to boxing the `ValueTask<TResponse>` result — the mediator dispatch overhead is zero.

### Publish Notification (2 handlers, no pipeline)

| Method | Mean | Ratio | Allocated | Alloc Ratio |
|---|---:|---:|---:|---:|
| MediatR | 67.2 ns | 1.00 | 440 B | 1.00 |
| Synaptrix | 62.6 ns | 0.93 | 224 B | 0.51 |
| **SynaptrixGen** | **0.5 ns** | **0.007** | **0 B** | **0.00** |
| Martin | 6.0 ns | 0.09 | 0 B | 0.00 |

`SynaptrixGen` achieves near-zero cost by generating inline sequential dispatch with an `IsCompletedSuccessfully` fast-path — no DI resolution, no delegate allocations, no virtual dispatch through a publisher interface on the hot path.

-----

## Why Synaptrix?

* **An Open-Source Alternative**: Synaptrix was created as an open-source alternative in response to other popular mediator libraries (like MediatR) transitioning to a paid licensing model. We believe core architectural patterns should remain freely accessible to the developer community.

* **Lightweight and Minimal**: Provides only the essential Mediator pattern functionalities, without unnecessary overhead.

* **Optimized Performance**: Thanks to Source Generators, handler discovery and registration happen entirely at compile-time, ensuring faster application startup and zero runtime reflection. The generator goes one step further: it produces a `GeneratedMediator` class with constructor-injected handlers and direct type-switch dispatch, achieving **sub-nanosecond to ~4 ns per call with zero allocations** on the hot path.

* **Easy DI Integration**: Integrates seamlessly with `Microsoft.Extensions.DependencyInjection`.

* **Same MediatR Interfaces**: Uses interfaces with identical signatures to MediatR, making migration extremely straightforward.

* **CQRS and Pub/Sub Patterns**: Facilitates the implementation of Command Query Responsibility Segregation (CQRS) and Publisher/Subscriber principles, enhancing separation of concerns and code maintainability.

-----

## Key Features

* **Requests with Responses (`IRequest<TResponse>`, `IRequestHandler<TRequest, TResponse>`)**: For operations that return a result.

* **Fire-and-Forget Requests (`IRequest`, `IRequestHandler<TRequest>`)**: For commands that don't return a result.

* **Notifications (`INotification`, `INotificationHandler<TNotification>`)**: For publishing events to zero or more handlers.

* **`IMediator`**: The primary interface for both sending requests and publishing notifications.

* **`ISender`**: A focused interface for sending requests (commands and queries), often preferred when only dispatching is needed, without notification capabilities.

* **Pipeline Behaviors (`IPipelineBehavior<TRequest, TResponse>`)**: Intercept requests before and after their handlers for cross-cutting concerns.

* **Request Pre-Processors (`IRequestPreProcessor<TRequest>`)**: Execute logic before a request handler.

* **Request Post-Processors (`IRequestPostProcessor<TRequest, TResponse>`)**: Execute logic after a request handler and before the response is returned.

* **Streaming Requests (`IStreamRequest<TResponse>`, `IStreamRequestHandler<TRequest, TResponse>`)**: For operations that return an `IAsyncEnumerable<TResponse>`, enabling efficient streaming of multiple results. Dispatch via `ISender.CreateStream<TResponse>()`.

* **Stream Pipeline Behaviors (`IStreamPipelineBehavior<TRequest, TResponse>`)**: Intercept streaming requests before and after their handlers for cross-cutting concerns.

* **Custom Notification Publishers (`INotificationPublisher`)**: Define how notifications are dispatched to multiple handlers (e.g., parallel, sequential).

* **Automatic Handler Registration**: Synaptrix uses **compile-time Source Generation** for handler registration. It requires **Zero Configuration**: just install the package, and handlers are automatically discovered (even in referenced projects). The generator produces two files:
    * `SynaptrixGeneratedHandlersRegistrations.g.cs` — the `AddSynaptrixHandlers()` DI extension method registering all handlers.
    * `SynaptrixGeneratedMediator.g.cs` — the `GeneratedMediator` class: a concrete `IMediator`/`ISender` implementation with constructor-injected handler singletons and branch-free type-switch dispatch, eliminating all DI lookups on the hot path.

* **Configurable Namespace and Method Names**: Control the generated class's namespace and the DI extension method's name via MSBuild properties.

-----

## Installation

Synaptrix is distributed via three NuGet packages:

1.  **`Synaptrix`**: The recommended meta-package — installs both `Synaptrix.Core` and `Synaptrix.Generator` in one step.

2.  **`Synaptrix.Core`**: Contains the interfaces (`IMediator`, `ISender`, `IRequest`, etc.), the `Mediator` implementation, and core DI extension methods.

3.  **`Synaptrix.Generator`**: Contains the C# Source Generator for compile-time handler registration.

**Quick start (recommended):**

```bash
dotnet add package Synaptrix
```

This single command installs everything you need. Alternatively, if you need finer control:

```bash
dotnet add package Synaptrix.Core
dotnet add package Synaptrix.Generator
```

-----

## Usage

### 1. Define Requests, Commands, and Notifications

Your requests, commands, and notifications must implement the `Synaptrix` interfaces.

```csharp
// Request with response
using Synaptrix;

namespace MyProject.Requests
{
    public class GetProductByIdQuery : IRequest<ProductDto>
    {
        public int ProductId { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}

// Fire-and-forget command
using Synaptrix;

namespace MyProject.Commands
{
    public class CreateProductCommand : IRequest
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
}

// Notification
using Synaptrix;

namespace MyProject.Notifications
{
    public class ProductCreatedNotification : INotification
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
}
```

### 2. Define Handlers, Processors, and Behaviors

Your handlers must implement `IRequestHandler` or `INotificationHandler`. Pre-processors implement `IRequestPreProcessor`, post-processors implement `IRequestPostProcessor`, and pipeline behaviors implement `IPipelineBehavior`.

```csharp
// Handler for a request with response
using Synaptrix;
using MyProject.Requests;

namespace MyProject.Handlers
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        public ValueTask<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Handling GetProductByIdQuery for ProductId: {request.ProductId}");
            var product = new ProductDto { Id = request.ProductId, Name = $"Product {request.ProductId}", Price = 10.50m };
            return new ValueTask<ProductDto>(product);
        }
    }
}

// Handler for a fire-and-forget command
using Synaptrix;
using MyProject.Commands;

namespace MyProject.Handlers
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
    {
        public ValueTask Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Creating product: {request.ProductName} with ID: {request.ProductId}");
            return default;
        }
    }
}

// Notification Handler
using Synaptrix;
using MyProject.Notifications;

namespace MyProject.Handlers
{
    public class SendEmailOnProductCreated : INotificationHandler<ProductCreatedNotification>
    {
        public ValueTask Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Sending email for new product: {notification.ProductName} (Id: {notification.ProductId})");
            return default;
        }
    }

    public class LogProductCreation : INotificationHandler<ProductCreatedNotification>
    {
        public ValueTask Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Logging product creation: {notification.ProductName} (Id: {notification.ProductId}) created at {DateTime.Now}");
            return default;
        }
    }
}

// Example Request Pre-Processor
using Synaptrix;
using MyProject.Requests;

namespace MyProject.Processors
{
    public class MyRequestLoggerPreProcessor : IRequestPreProcessor<GetProductByIdQuery>
    {
        public ValueTask Process(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Pre-processing GetProductByIdQuery for ProductId: {request.ProductId}");
            return default;
        }
    }
}

// Example Request Post-Processor
using Synaptrix;
using MyProject.Requests;

namespace MyProject.Processors
{
    public class MyResponseLoggerPostProcessor : IRequestPostProcessor<GetProductByIdQuery, ProductDto>
    {
        public ValueTask Process(GetProductByIdQuery request, ProductDto response, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Post-processing GetProductByIdQuery. Response: {response.Name}");
            return default;
        }
    }
}
```

### 3. Register Services in `Program.cs`

The Source Generator runs at **compile-time** and produces two C# files:

| Generated file | What it contains |
|---|---|
| `SynaptrixGeneratedHandlersRegistrations.g.cs` | An `AddSynaptrixHandlers()` DI extension method that registers all handlers, processors, and behaviors discovered in your project and any referenced assemblies. |
| `SynaptrixGeneratedMediator.g.cs` | A `GeneratedMediator` sealed class — a concrete `IMediator`/`ISender` with constructor-injected handler singletons and a direct `is`-type-switch dispatch, achieving **zero DI lookups and zero allocations** on the hot path. |

The `[assembly: DiscoverSynaptrixHandlers]` attribute that triggers the generator is **injected automatically** by the NuGet package via a `.targets` file — no manual setup required.

#### Configure your `.csproj`

Install the packages. If using the individual packages instead of the meta-package, the generator is consumed as a Roslyn Analyzer:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- Optional: rename the generated DI extension method (default: AddSynaptrixHandlers) -->
    <SynaptrixGeneratedMethodName>AddMyAppHandlers</SynaptrixGeneratedMethodName>
  </PropertyGroup>

  <ItemGroup>
    <!-- Informs that the above property is compiler-visible to the generator -->
    <CompilerVisibleProperty Include="SynaptrixGeneratedMethodName" />
  </ItemGroup>

  <ItemGroup>
    <!-- Option 1: meta-package (recommended) -->
    <PackageReference Include="Synaptrix" Version="3.0.0" />

    <!-- Option 2: individual packages
    <PackageReference Include="Synaptrix.Core" Version="3.0.0" />
    <PackageReference Include="Synaptrix.Generator" Version="3.0.0" PrivateAssets="all" />
    -->
  </ItemGroup>
</Project>
```

#### Register in `Program.cs`

Calling `AddSynaptrixHandlers()` (or your custom name) does everything in one step: it registers all handlers and wires up `GeneratedMediator` as both `IMediator` and `ISender`.

```csharp
using Synaptrix;

var builder = WebApplication.CreateBuilder(args);

// Single call registers all handlers + wires GeneratedMediator as IMediator/ISender.
// Default lifetime is Scoped — correct for ASP.NET Core (per-request) and safe for
// desktop apps with a single root scope (behaves like Singleton there).
builder.Services.AddSynaptrixHandlers();

// If you also need a notification publisher for the reflection-based Mediator (non-generated path):
builder.Services.AddSynaptrixCoreServices();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
```

##### Mediator lifetime

`AddSynaptrixHandlers` accepts an optional `ServiceLifetime` parameter to control the lifetime of `IMediator` / `ISender` / `GeneratedMediator`:

| Lifetime | When to use |
|---|---|
| `Scoped` *(default)* | ASP.NET Core apps (one mediator per HTTP request). Also safe for desktop apps with a single root scope — behaves like Singleton in that context. |
| `Singleton` | Desktop or console apps where no child scopes are created, or when all handler dependencies are themselves Singleton/Transient. |
| `Transient` | Maximum isolation: a fresh mediator instance per resolution. Negligible overhead; avoids any scope-capture concern. |

```csharp
// ASP.NET Core — Scoped is the default, no need to specify it explicitly:
builder.Services.AddSynaptrixHandlers();
builder.Services.AddSynaptrixHandlers(ServiceLifetime.Scoped); // same as above

// Desktop / console — Singleton is efficient when handler deps are not Scoped:
services.AddSynaptrixHandlers(ServiceLifetime.Singleton);

// Maximum safety — a new mediator per dispatch:
builder.Services.AddSynaptrixHandlers(ServiceLifetime.Transient);
```

> **Note for ASP.NET Core Development mode**: with `ValidateScopes = true` (the default), using `Singleton` will throw at startup if any handler depends on a Scoped service (e.g. an EF Core `DbContext`). Use `Scoped` (the default) to avoid this.

#### What the generator produces (for reference)

Look inside **Dependencies → Analyzers → Synaptrix.Generator** in Solution Explorer to inspect the generated files:

`SynaptrixGeneratedHandlersRegistrations.g.cs` — the DI extension method:

```csharp
// Auto-generated — do not edit
public static IServiceCollection AddSynaptrixHandlers(
    this IServiceCollection services,
    ServiceLifetime mediatorLifetime = ServiceLifetime.Scoped)
{
    // Transient registrations for handler types
    services.AddTransient<IRequestHandler<GetProductByIdQuery, ProductDto>, GetProductByIdQueryHandler>();
    services.AddTransient<IRequestHandler<CreateProductCommand>, CreateProductCommandHandler>();
    services.AddTransient<INotificationHandler<ProductCreatedNotification>, SendEmailOnProductCreated>();
    services.AddTransient<INotificationHandler<ProductCreatedNotification>, LogProductCreation>();

    // Wire up GeneratedMediator with the requested lifetime
    switch (mediatorLifetime)
    {
        case ServiceLifetime.Singleton:
            services.AddSingleton<GeneratedMediator>();
            services.AddSingleton<IMediator>(sp => sp.GetRequiredService<GeneratedMediator>());
            services.AddSingleton<ISender>(sp => sp.GetRequiredService<GeneratedMediator>());
            break;
        case ServiceLifetime.Transient:
            services.AddTransient<GeneratedMediator>();
            services.AddTransient<IMediator>(sp => sp.GetRequiredService<GeneratedMediator>());
            services.AddTransient<ISender>(sp => sp.GetRequiredService<GeneratedMediator>());
            break;
        default: // Scoped
            services.AddScoped<GeneratedMediator>();
            services.AddScoped<IMediator>(sp => sp.GetRequiredService<GeneratedMediator>());
            services.AddScoped<ISender>(sp => sp.GetRequiredService<GeneratedMediator>());
            break;
    }
    return services;
}
```

`SynaptrixGeneratedMediator.g.cs` — the zero-overhead mediator:

```csharp
// Auto-generated — do not edit
public sealed partial class GeneratedMediator : IMediator, ISender
{
    private readonly IRequestHandler<GetProductByIdQuery, ProductDto> _handler1;
    private readonly IRequestHandler<CreateProductCommand> _handler2;
    private readonly INotificationHandler<ProductCreatedNotification> _handler3;
    private readonly INotificationHandler<ProductCreatedNotification> _handler4;

    public GeneratedMediator(
        IRequestHandler<GetProductByIdQuery, ProductDto> handler1,
        IRequestHandler<CreateProductCommand> handler2,
        INotificationHandler<ProductCreatedNotification> handler3,
        INotificationHandler<ProductCreatedNotification> handler4)
    { /* assign fields */ }

    // Direct type-switch dispatch — no DI lookup, no reflection, no boxing
    public async ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        if (request is GetProductByIdQuery q1)
            return (TResponse)(object)await _handler1.Handle(q1, ct).ConfigureAwait(false);
        throw new InvalidOperationException(...);
    }

    // Inline sequential publish with IsCompletedSuccessfully fast-path
    public ValueTask Publish(INotification notification, CancellationToken ct = default)
    {
        if (notification is ProductCreatedNotification n)
        {
            var t0 = _handler3.Handle(n, ct);
            if (!t0.IsCompletedSuccessfully) return _FinishPublish_From0(n, t0, ct);
            var t1 = _handler4.Handle(n, ct);
            if (t1.IsCompletedSuccessfully) return default;
            return t1;
        }
        throw new InvalidOperationException(...);
    }
}
```

> **Note**: The example above is simplified for clarity. The real generated code uses fully-qualified type names and handles all edge cases.

-----

## Migrating from Concordia

**Synaptrix v3.0.0** is the direct successor of **Concordia** (v2.x). The library was renamed to better reflect its scope and evolution. The interfaces and APIs remain the same — the only change is the package and namespace naming.

### What changed in v3.0.0

| Before (Concordia v2.x) | After (Synaptrix v3.0.0) |
|---|---|
| `Concordia` namespace | `Synaptrix` namespace |
| `Concordia.Core` package | `Synaptrix.Core` |
| `Concordia.Generator` package | `Synaptrix.Generator` |
| `Concordia.MediatR` package | **Discontinued** — see below |
| — | `Synaptrix` meta-package (new) |

### How to migrate

1. Update your package references to v3.0.0.
2. If you were using the older `using Concordia;` namespace, replace it with `using Synaptrix;`.

### MediatR Compatibility (`Concordia.MediatR`)

The `Concordia.MediatR` compatibility package provided a runtime reflection-based `AddMediator()` extension method for projects migrating from MediatR. **Starting with v3.0.0, this package is no longer maintained or published.**

If you still need the MediatR compatibility layer, install the **last published Concordia-era version**:

```bash
dotnet add package Concordia.MediatR --version 2.4.1
```

This version remains available on NuGet and will continue to work, but it will not receive further updates. We strongly recommend completing your migration to the Source Generator approach, which offers significantly better performance and zero-configuration setup.

-----

## Migration Guide from MediatR

If you are migrating an existing project from MediatR to Concordia, the process is extremely simple thanks to the identical interfaces and patterns.

### 1. Update NuGet Packages

Remove the MediatR package and install the Concordia packages:

```bash
dotnet remove package MediatR
dotnet remove package MediatR.Extensions.Microsoft.DependencyInjection # If present
dotnet add package Concordia.Core --version 1.1.0
dotnet add package Concordia.MediatR --version 1.1.0
```

### 2. Update Namespaces

Change namespaces from `MediatR` to `Concordia` and `Concordia` where necessary.

* **Interfaces**:
    * `MediatR.IRequest<TResponse>` becomes `Concordia.IRequest<TResponse>`
    * `MediatR.IRequest` becomes `Concordia.IRequest`
    * `MediatR.IRequestHandler<TRequest, TResponse>` becomes `Concordia.IRequestHandler<TRequest, TResponse>`
    * `MediatR.IRequestHandler<TRequest>` becomes `Concordia.IRequestHandler<TRequest>`
    * `MediatR.INotification` becomes `Concordia.INotification`
    * `MediatR.INotificationHandler<TNotification>` becomes `Concordia.INotificationHandler<TNotification>`
    * `MediatR.IPipelineBehavior<TRequest, TResponse>` becomes `Concordia.IPipelineBehavior<TRequest, TResponse>`
    * `MediatR.IRequestPreProcessor<TRequest>` becomes `Concordia.IRequestPreProcessor<TRequest>`
    * `MediatR.IRequestPostProcessor<TRequest, TResponse>` becomes `Concordia.IRequestPostProcessor<TRequest, TResponse>`
    * `MediatR.INotificationPublisher` becomes `Concordia.INotificationPublisher`

* **Mediator Implementation**:
    * `MediatR.IMediator` becomes `Concordia.IMediator`
    * `MediatR.ISender` becomes `Concordia.ISender`
    * `MediatR.Mediator` becomes `Concordia.Mediator`

### 3. Update Service Registration in `Program.cs`

Replace the `AddMediatR` extension method with Concordia's Source Generator approach:

**Before (MediatR):**

```csharp
using MediatR;
using MediatR.Extensions.Microsoft.DependencyInjection; // If present
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    // Other MediatR configurations
});
```

**After (Concordia.MediatR):**

```csharp
using Concordia; // For IMediator, ISender
using Concordia.MediatR; // For the AddMediator extension method
using System.Reflection;
using Microsoft.Extensions.DependencyInjection; // For ServiceLifetime

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediator(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    // Configuration options are similar to MediatR, but use the ConcordiaMediatRServiceConfiguration class
    cfg.Lifetime = ServiceLifetime.Scoped; // Example
    // cfg.NotificationPublisherType = typeof(MyCustomNotificationPublisher); // Example
    // cfg.AddOpenBehavior(typeof(MyCustomPipelineBehavior<,>)); // Example
    // cfg.AddRequestPreProcessor<MyCustomPreProcessor>(); // Example
    // cfg.AddRequestPostProcessor<MyCustomPostProcessor>(); // Example
});
```

-----

## Contributing
Synaptrix is an open-source project, and contributions are welcome! If you find a bug, have a feature request, or want to contribute code, please open an issue or pull request on GitHub.
Please ensure your contributions adhere to the project's coding standards and include appropriate tests. For larger changes, consider discussing your ideas in an issue first.

We also have a [Code of Conduct](CODE_OF_CONDUCT.md) that we expect all contributors to adhere to.

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details.


## License
Synaptrix is licensed under the [MIT License](LICENSE).

## NuGet Packages
- [Synaptrix](https://www.nuget.org/packages/Synaptrix) — meta-package (recommended)
- [Synaptrix.Core](https://www.nuget.org/packages/Synaptrix.Core)
- [Synaptrix.Generator](https://www.nuget.org/packages/Synaptrix.Generator)
- [Concordia.MediatR](https://www.nuget.org/packages/Concordia.MediatR) — legacy, frozen at v2.4.1

## Contact
For questions, feedback, or support, please reach out via the project's GitHub repository or contact the maintainers directly.
For more information, visit the [Synaptrix GitHub repository](https://github.com/mrdevrobot/Concordia).

## Support
If you find Synaptrix useful, consider supporting the project by starring it on GitHub or sharing it with your developer community. Your support helps keep the project active and encourages further development.


