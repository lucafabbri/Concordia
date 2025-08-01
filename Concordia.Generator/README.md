# Concordia.Generator: Compile-Time Handler Registration for Concordia

**Concordia.Generator** is the C# Source Generator component of the Concordia library. It automates the registration of your request handlers, notification handlers, pre-processors, post-processors, and pipeline behaviors at **compile-time**, eliminating the need for runtime reflection and significantly improving application startup performance.

# Table of Contents
- [Why Concordia?](#why-concordia)
- [Key Features](#key-features)
- [Installation](#installation)
- [Usage](#usage)
- [Contribution](#contribution)
- [License](#license)
- [NuGet Packages](#nuget-packages)
- [Contact](#contact)
- [Support](#support)

## Why Concordia?

* **An Open-Source Alternative**: Concordia was created as an open-source alternative in response to other popular mediator libraries (like MediatR) transitioning to a paid licensing model. We believe core architectural patterns should remain freely accessible to the developer community.
* **Lightweight and Minimal**: Provides only the essential Mediator pattern functionalities, without unnecessary overhead.
* **Optimized Performance**: Thanks to Source Generators, handler discovery and registration happen entirely at compile-time, ensuring faster application startup and zero runtime reflection.
* **Easy DI Integration**: Integrates seamlessly with `Microsoft.Extensions.DependencyInjection`.
* **Same MediatR Interfaces**: Uses interfaces with identical signatures to MediatR, making migration or parallel adoption extremely straightforward.
* **CQRS and Pub/Sub Patterns**: Facilitates the implementation of Command Query Responsibility Segregation (CQRS) and Publisher/Subscriber principles, enhancing separation of concern and code maintainability.

## Key Features

* **Automatic Handler Registration**: Scans your assemblies at compile-time to find and register `IRequestHandler`, `INotificationHandler`, `IPipelineBehavior`, `IRequestPreProcessor`, and `IRequestPostProcessor` implementations.
* **Zero Runtime Reflection**: All service registration code is generated as C# source files, which are then compiled directly into your assembly.
* **Configurable Namespace and Method Names**: Control the generated class's namespace and the DI extension method's name via MSBuild properties in your `.csproj` file.

## Installation

Install **Concordia.Core** and **Concordia.Generator** in your application project:

```bash
dotnet add package Concordia.Core --version 1.0.0
dotnet add package Concordia.Generator --version 1.0.0
```

## Usage

1.  **Define your Handlers, Processors, and Behaviors** (as described in `Concordia.Core`'s documentation).

2.  **Configure your `.csproj`**: Add the `Concordia.Generator` as a `ProjectReference` with specific attributes and optionally configure the generated method name and namespace.

    ```xml
    <Project Sdk="Microsoft.NET.Sdk.Web">
      <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
        <!-- Optional: Customize the generated extension method name -->
        <ConcordiaGeneratedMethodName>AddMyCustomMediatorHandlers</ConcordiaGeneratedMethodName>
        <!-- Optional: Customize the namespace for the generated class (defaults to project's RootNamespace) -->
        <!-- <ConcordiaGeneratedNamespace>MyProject.Generated</ConcordiaGeneratedNamespace> -->
      </PropertyGroup>

      <ItemGroup>
        <ProjectReference Include="PathToYour\Concordia.Core\Concordia.Core.csproj" />
        <ProjectReference Include="PathToYour\Concordia.Generator\Concordia.Generator.csproj"
                          OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
      </ItemGroup>
    </Project>
    ```

3.  **Register services in `Program.cs`**:

    ```csharp
    using Concordia; // Required for IMediator, ISender
    using Concordia.DependencyInjection; // For AddConcordiaCoreServices
    using MyProject.Web; // Example: Namespace where ConcordiaGeneratedRegistrations is generated

    var builder = WebApplication.CreateBuilder(args);

    // 1. Register Concordia's core services.
    builder.Services.AddConcordiaCoreServices();

    // 2. Register your specific handlers and pipeline behaviors discovered by the generator.
    // The method name will depend on your .csproj configuration (e.g., AddMyCustomMediatorHandlers).
    builder.Services.AddMyCustomMediatorHandlers(); // Use the name configured in .csproj

    builder.Services.AddControllers();
    var app = builder.Build();
    app.MapControllers();
    app.Run();
    ```

The Source Generator will automatically find your handler implementations and generate a static class (e.g., `ConcordiaGeneratedRegistrations`) containing an extension method (e.g., `AddMyCustomMediatorHandlers`) that registers all your services with the DI container.


## Contribution

Feel free to contribute to the project! Report bugs, suggest new features, or submit pull requests.
Please follow the [Contributing Guidelines](https://github.com/lucafabbri/Concordia/blob/main/CONTRIBUTING.md).

## License

This project is released under the [MIT License](https://opensource.org/licenses/MIT). See the [LICENSE](https://github.com/lucafabbri/Concordia/blob/main/LICENSE) file for more information.

## NuGet Packages
- [Concordia.Core](https://www.nuget.org/packages/Concordia.Core)   
- [Concordia.Generator](https://www.nuget.org/packages/Concordia.Generator) (for compile-time handler registration)
- [Concordia.MediatR](https://www.nuget.org/packages/Concordia.MediatR) (for MediatR compatibility)

## Contact
For any questions, issues, or feedback, please open an issue on the [GitHub repository](https://github.com/YourUsername/Concordia/issues).

## Support
If you find this library useful, consider supporting its development: [Buy Me a Coffee](https://www.buymeacoffee.com/lucafabbriu).
