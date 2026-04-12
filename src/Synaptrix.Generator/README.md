# Synaptrix.Generator: Compile-Time Handler Registration for Synaptrix

> **Synaptrix** (formerly **Concordia**) — renamed starting from v3.0.0.

**Synaptrix.Generator** is the C# Source Generator component of the Synaptrix library. It automates the registration of your handlers at **compile-time** and generates a concrete `GeneratedMediator` class that dispatches requests and notifications with **zero runtime overhead** — no DI lookups, no reflection, no allocations on the hot path.

# Table of Contents
- [Why Synaptrix?](#why-synaptrix)
- [Key Features](#key-features)
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

