# Concordia: A Lightweight and Powerful .NET Mediator

Concordia is a .NET library implementing the **Mediator pattern**, designed to be lightweight, performant, and easily integrated with the .NET Dependency Injection system. It leverages **C# Source Generators** for automatic handler registration at compile-time, eliminating the need for runtime reflection and improving application startup performance.

|Project|NuGet Downloads|NuGet Version|
|---|---|---|
| Concordia.Core | ![NuGet Downloads](https://img.shields.io/nuget/dt/Concordia.Core?cacheSeconds=600) | ![NuGet Version](https://img.shields.io/nuget/v/Concordia.Core) |
| Concordia.Generator | ![NuGet Downloads](https://img.shields.io/nuget/dt/Concordia.Generator?cacheSeconds=600) | ![NuGet Version](https://img.shields.io/nuget/v/Concordia.Generator) |
| Concordia.MediatR | ![NuGet Downloads](https://img.shields.io/nuget/dt/Concordia.MediatR?cacheSeconds=600) | ![NuGet Version](https://img.shields.io/nuget/v/Concordia.MediatR) |


## Table of Contents
- [Why Concordia?](#why-concordia)
- [Key Features](#key-features)
- [Installation](#installation)
- [Usage](#usage)
    - [1. Define Requests, Commands, and Notifications](#1-define-requests-commands-and-notifications)
    - [2. Define Handlers, Processors, and Behaviors](#2-define-handlers-processors-and-behaviors)
    - [3. Choose Your Registration Method in `Program.cs`](#3-choose-your-registration-method-in-programcs)
        - [Option A: Using the Source Generator (Recommended)](#option-a-using-the-source-generator-recommended)
        - [Option B: Using the MediatR Compatibility Layer](#option-b-using-the-mediatr-compatibility-layer)
- [Migration Guide from MediatR](#migration-guide-from-mediatr)
- [Contributing](#contributing)
- [License](#license)
- [NuGet Packages](#nuget-packages)
- [Contact](#contact)
- [Support](#support)


-----

## Why Concordia?

* **An Open-Source Alternative**: Concordia was created as an open-source alternative in response to other popular mediator libraries (like MediatR) transitioning to a paid licensing model. We believe core architectural patterns should remain freely accessible to the developer community.

* **Lightweight and Minimal**: Provides only the essential Mediator pattern functionalities, without unnecessary overhead.

* **Optimized Performance**: Thanks to Source Generators, handler discovery and registration happen entirely at compile-time, ensuring faster application startup and zero runtime reflection.

* **Easy DI Integration**: Integrates seamlessly with `Microsoft.Extensions.DependencyInjection`.

* **Same MediatR Interfaces**: Uses interfaces with identical signatures to MediatR, making migration or parallel adoption extremely straightforward.

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

* **Stream Pipeline Behaviors (`IStreamPipelineBehavior<TRequest, TResponse>`)**: (For future streaming request support) Intercept streaming requests.

* **Custom Notification Publishers (`INotificationPublisher`)**: Define how notifications are dispatched to multiple handlers (e.g., parallel, sequential).

* **Automatic Handler Registration**: Concordia offers two approaches for handler registration:

    * **Compile-time (Source Generator)**: The recommended approach for new projects, providing optimal startup performance.

    * **Runtime Reflection**: A compatibility layer for easier migration from existing MediatR setups, now using its own `ConcordiaMediatRServiceConfiguration` class, offering flexible configuration options including service lifetimes, pre/post-processors, and custom notification publishers.

* **Configurable Namespace and Method Names**: Control the generated class's namespace and the DI extension method's name via MSBuild properties (for Source Generator).

-----

## Installation

Concordia is distributed via three NuGet packages, all currently at **version 1.1.0**:

1.  **`Concordia.Core`**: Contains the interfaces (`IMediator`, `ISender`, `IRequest`, etc.), the `Mediator` implementation, and core DI extension methods.

2.  **`Concordia.Generator`**: Contains the C# Source Generator for compile-time handler registration.

3.  **`Concordia.MediatR`**: Provides a compatibility layer with MediatR's `AddMediator` extension method for runtime reflection-based handler registration, now using its own `ConcordiaMediatRServiceConfiguration`.

To get started with Concordia, install the necessary packages in your application project (e.g., an ASP.NET Core project) using the .NET CLI. You will typically choose **either `Concordia.Generator` OR `Concordia.MediatR`** based on your preference for handler registration.

**Option 1: Using the Source Generator (Recommended for New Projects)**

```bash
dotnet add package Concordia.Core --version 1.1.0
dotnet add package Concordia.Generator --version 1.1.0
```

**Option 2: Using the MediatR Compatibility Layer (For Migration or Reflection Preference)**

```bash
dotnet add package Concordia.Core --version 1.1.0
dotnet add package Concordia.MediatR --version 1.1.0
```

Alternatively, you can install them via the NuGet Package Manager in Visual Studio.

-----

## Usage

### 1. Define Requests, Commands, and Notifications

Your requests, commands, and notifications must implement the `Concordia.Contracts` interfaces.

```csharp
// Request with response
using Concordia.Contracts;

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
using Concordia.Contracts;

namespace MyProject.Commands
{
    public class CreateProductCommand : IRequest
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
}

// Notification
using Concordia.Contracts;

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
using Concordia.Contracts;
using MyProject.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace MyProject.Handlers
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        public Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Handling GetProductByIdQuery for ProductId: {request.ProductId}");
            var product = new ProductDto { Id = request.ProductId, Name = $"Product {request.ProductId}", Price = 10.50m };
            return Task.FromResult(product);
        }
    }
}

// Handler for a fire-and-forget command
using Concordia.Contracts;
using MyProject.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace MyProject.Handlers
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
    {
        public Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Creating product: {request.ProductName} with ID: {request.ProductId}");
            return Task.CompletedTask;
        }
    }
}

// Notification Handler
using Concordia.Contracts;
using MyProject.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace MyProject.Handlers
{
    public class SendEmailOnProductCreated : INotificationHandler<ProductCreatedNotification>
    {
        public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Sending email for new product: {notification.ProductName} (Id: {notification.ProductId})");
            return Task.CompletedTask;
        }
    }

    public class LogProductCreation : INotificationHandler<ProductCreatedNotification>
    {
        public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Logging product creation: {notification.ProductName} (Id: {notification.ProductId}) created at {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}

// Example Request Pre-Processor
using Concordia.Contracts;
using MyProject.Requests; // Assuming your requests are here
using System.Threading;
using System.Threading.Tasks;

namespace MyProject.Processors
{
    public class MyRequestLoggerPreProcessor : IRequestPreProcessor<GetProductByIdQuery>
    {
        public Task Process(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Pre-processing GetProductByIdQuery for ProductId: {request.ProductId}");
            return Task.CompletedTask;
        }
    }
}

// Example Request Post-Processor
using Concordia.Contracts;
using MyProject.Requests; // Assuming your requests are here
using System.Threading;
using System.Threading.Tasks;

namespace MyProject.Processors
{
    public class MyResponseLoggerPostProcessor : IRequestPostProcessor<GetProductByIdQuery, ProductDto>
    {
        public Task Process(GetProductByIdQuery request, ProductDto response, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Post-processing GetProductByIdQuery. Response: {response.Name}");
            return Task.CompletedTask;
        }
    }
}

// Example Pipeline Behavior
// using Concordia.Contracts;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;

// namespace MyProject.Behaviors
// {
//     public class TestLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
//         where TRequest : IRequest<TResponse>
//     {
//         private readonly List<string> _logs;
//         public TestLoggingBehavior(List<string> logs) { _logs = logs; }
//         public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//         {
//             _logs.Add($"Before {typeof(TRequest).Name}");
//             var response = await next(cancellationToken);
//             _logs.Add($"After {typeof(TRequest).Name}");
//             return response;
//         }
//     }
// }
```

### 3. Choose Your Registration Method in `Program.cs`

You will use either the **Source Generator method** (recommended for new projects) or the **MediatR Compatibility method** (for easier migration).

#### Option A: Using the Source Generator (Recommended)

This method provides optimal startup performance by registering handlers at compile-time.

##### i. Configure your `.csproj`

Add the `Concordia.Generator` as a `ProjectReference` to your application project's `.csproj` file. Ensure the `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"` attributes are set. You can also customize the generated extension method's name using the `ConcordiaGeneratedMethodName` property.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- Optional: Customize the generated extension method name -->
    <ConcordiaGeneratedMethodName>AddMyConcordiaHandlers</ConcordiaGeneratedMethodName>
    <!-- Optional: Customize the namespace for the generated class (defaults to project's RootNamespace) -->
    <!-- <ConcordiaGeneratedNamespace>MyProject.Generated</ConcordiaGeneratedNamespace> -->
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="PathToYour\Concordia.Core\Concordia.Core.csproj" />
    <ProjectReference Include="PathToYour\Concordia.Generator\Concordia.Generator.csproj"
                      OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  </ItemGroup>

  <!-- Ensure your Request, Handler, Processor, and Behavior files are included in the project -->
  <ItemGroup>
    <Compile Include="Requests\MySimpleQuery.cs" />
    <Compile Include="Handlers\MySimpleQueryHandler.cs" />
    <Compile Include="Processors\MyRequestLoggerPreProcessor.cs" />
    <Compile Include="Processors\MyResponseLoggerPostProcessor.cs" />
    <Compile Include="Behaviors\TestLoggingBehavior.cs" />
    <!-- ... other handlers, processors, behaviors ... -->
  </ItemGroup>
</Project>
```

##### ii. Register services in `Program.cs`

After configuring your `.csproj`, the Source Generator will automatically generate an extension method (e.g., `AddMyConcordiaHandlers`) that registers all your handlers, processors, and behaviors. Call this method in your `Program.cs` after registering Concordia's core services.

```csharp
using Concordia; // Required for IMediator, ISender
using Concordia.DependencyInjection; // For AddConcordiaCoreServices
using Microsoft.AspNetCore.Mvc;
using MyProject.Web; // Example: Namespace where ConcordiaGeneratedRegistrations is generated

var builder = WebApplication.CreateBuilder(args);

// 1. Register Concordia's core services (IMediator, ISender).
// You can use the parameterless method for the default publisher, or:
builder.Services.AddConcordiaCoreServices<Concordia.ForeachAwaitPublisher>(); // Example: Explicitly register the default publisher
// Or, if you have a custom publisher:
// builder.Services.AddConcordiaCoreServices<MyCustomNotificationPublisher>(); // Example: Register your custom publisher

// 2. Register your specific handlers and pipeline behaviors discovered by the generator.
// The method name will depend on your .csproj configuration (e.g., AddMyConcordiaHandlers).
builder.Services.AddMyConcordiaHandlers(); // Example with a custom name

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

// Example Controller for usage (remains unchanged)
namespace Concordia.Examples.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ISender _sender;

        public ProductsController(IMediator mediator, ISender sender)
        {
            _mediator = mediator;
            _sender = sender;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetProductByIdQuery { ProductId = id };
            var product = await _sender.Send(query);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {
            await _sender.Send(command);

            var notification = new ProductCreatedNotification
            {
                ProductId = command.ProductId,
                ProductName = command.ProductName
            };
            await _mediator.Publish(notification);

            return CreatedAtAction(nameof(Get), new { id = command.ProductId }, null);
        }
    }

    // Examples of requests, commands, notifications and handlers for the web project
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class GetProductByIdQuery : IRequest<ProductDto>
    {
        public int ProductId { get; set; }
    }

    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        public Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Handling GetProductByIdQuery for ProductId: {request.ProductId}");
            var product = new ProductDto { Id = request.ProductId, Name = $"Product {request.ProductId}", Price = 10.50m };
            return Task.FromResult(product);
        }
    }

    public class CreateProductCommand : IRequest
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
    {
        public Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Creating product: {request.ProductName} with ID: {request.ProductId}");
            return Task.CompletedTask;
        }
    }

    public class ProductCreatedNotification : INotification
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }

    public class SendEmailOnProductCreated : INotificationHandler<ProductCreatedNotification>
    {
        public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Sending email for new product: {notification.ProductName} (Id: {notification.ProductId})");
            return Task.CompletedTask;
        }
    }

    public class LogProductCreation : INotificationHandler<ProductCreatedNotification>
    {
        public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Logging product creation: {notification.ProductName} (Id: {notification.ProductId}) created at {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}
```

#### Option B: Using the MediatR Compatibility Layer

This method uses runtime reflection to register handlers, offering a familiar setup for those migrating from MediatR.

```csharp
using Concordia; // Required for IMediator, ISender
using Concordia.MediatR; // NEW: Namespace for the AddMediator extension method
using Microsoft.AspNetCore.Mvc;
using System.Reflection; // Required for Assembly.GetExecutingAssembly()
using Microsoft.Extensions.DependencyInjection; // Required for ServiceLifetime

var builder = WebApplication.CreateBuilder(args);

// Register Concordia and all handlers using the reflection-based AddMediator method.
// This will scan the specified assemblies (e.g., the current executing assembly)
// to find and register all handlers and pipeline behaviors.
builder.Services.AddMediator(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Example: Register all services as Scoped
    cfg.Lifetime = ServiceLifetime.Scoped;

    // Example: Register a custom notification publisher
    // cfg.NotificationPublisherType = typeof(MyCustomNotificationPublisher);

    // Example: Explicitly add a pre-processor
    // cfg.AddRequestPreProcessor<MyCustomPreProcessor>();

    // Example: Explicitly add a post-processor
    // cfg.AddRequestPostProcessor<MyCustomPostProcessor>();

    // Example: Explicitly add a stream behavior
    // cfg.AddStreamBehavior<MyCustomStreamBehavior>();
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

// Example Controller for usage (remains unchanged)
namespace Concordia.Examples.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ISender _sender;

        public ProductsController(IMediator mediator, ISender sender)
        {
            _mediator = mediator;
            _sender = sender;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetProductByIdQuery { ProductId = id };
            var product = await _sender.Send(query);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {
            await _sender.Send(command);

            var notification = new ProductCreatedNotification
            {
                ProductId = command.ProductId,
                ProductName = command.ProductName
            };
            await _mediator.Publish(notification);

            return CreatedAtAction(nameof(Get), new { id = command.ProductId }, null);
        }
    }

    // Examples of requests, commands, notifications and handlers for the web project
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class GetProductByIdQuery : IRequest<ProductDto>
    {
        public int ProductId { get; set; }
    }

    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        public Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Handling GetProductByIdQuery for ProductId: {request.ProductId}");
            var product = new ProductDto { Id = request.ProductId, Name = $"Product {request.ProductId}", Price = 10.50m };
            return Task.FromResult(product);
        }
    }

    public class CreateProductCommand : IRequest
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
    {
        public Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Creating product: {request.ProductName} with ID: {request.ProductId}");
            return Task.CompletedTask;
        }
    }

    public class ProductCreatedNotification : INotification
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }

    public class SendEmailOnProductCreated : INotificationHandler<ProductCreatedNotification>
    {
        public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Sending email for new product: {notification.ProductName} (Id: {notification.ProductId})");
            return Task.CompletedTask;
        }
    }

    public class LogProductCreation : INotificationHandler<ProductCreatedNotification>
    {
        public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Logging product creation: {notification.ProductName} (Id: {notification.ProductId}) created at {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}
```

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

Change namespaces from `MediatR` to `Concordia` and `Concordia.Contracts` where necessary.

* **Interfaces**:
    * `MediatR.IRequest<TResponse>` becomes `Concordia.Contracts.IRequest<TResponse>`
    * `MediatR.IRequest` becomes `Concordia.Contracts.IRequest`
    * `MediatR.IRequestHandler<TRequest, TResponse>` becomes `Concordia.Contracts.IRequestHandler<TRequest, TResponse>`
    * `MediatR.IRequestHandler<TRequest>` becomes `Concordia.Contracts.IRequestHandler<TRequest>`
    * `MediatR.INotification` becomes `Concordia.Contracts.INotification`
    * `MediatR.INotificationHandler<TNotification>` becomes `Concordia.Contracts.INotificationHandler<TNotification>`
    * `MediatR.IPipelineBehavior<TRequest, TResponse>` becomes `Concordia.Contracts.IPipelineBehavior<TRequest, TResponse>`
    * `MediatR.IRequestPreProcessor<TRequest>` becomes `Concordia.Contracts.IRequestPreProcessor<TRequest>`
    * `MediatR.IRequestPostProcessor<TRequest, TResponse>` becomes `Concordia.Contracts.IRequestPostProcessor<TRequest, TResponse>`
    * `MediatR.INotificationPublisher` becomes `Concordia.Contracts.INotificationPublisher`

* **Mediator Implementation**:
    * `MediatR.IMediator` becomes `Concordia.IMediator`
    * `MediatR.ISender` becomes `Concordia.ISender`
    * `MediatR.Mediator` becomes `Concordia.Mediator`

### 3. Update Service Registration in `Program.cs` (or `Startup.cs`)

Replace the `AddMediatR` extension method with Concordia's `AddMediator`.

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

### 4. Verify and Test

Rebuild your project and run your tests. Given the interface parity, most of your existing code should function without significant changes.


-----
## Contributing
Concordia is an open-source project, and contributions are welcome! If you find a bug, have a feature request, or want to contribute code, please open an issue or pull request on GitHub.
Please ensure your contributions adhere to the project's coding standards and include appropriate tests. For larger changes, consider discussing your ideas in an issue first.

We also have a [Code of Conduct](CODE_OF_CONDUCT.md) that we expect all contributors to adhere to.

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details.


## License
Concordia is licensed under the [MIT License](LICENSE).

## NuGet Packages
- [Concordia.Core](https://www.nuget.org/packages/Concordia.Core)
- [Concordia.Generator](https://www.nuget.org/packages/Concordia.Generator)
- [Concordia.MediatR](https://www.nuget.org/packages/Concordia.MediatR)

## Contact
For questions, feedback, or support, please reach out via the project's GitHub repository or contact the maintainers directly.
For more information, visit the [Concordia GitHub repository](https://github.com/Concordia).

## Support
If you find Concordia useful, consider supporting the project by starring it on GitHub or sharing it with your developer community. Your support helps keep the project active and encourages further development.  


