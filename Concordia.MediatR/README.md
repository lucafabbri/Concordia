# Concordia.MediatR: MediatR Compatibility Layer for Concordia

**Concordia.MediatR** provides a compatibility layer for the Concordia library, offering a reflection-based handler registration experience similar to the popular MediatR library. This package is ideal for projects migrating from MediatR or for those who prefer runtime discovery over compile-time code generation.

## Why Concordia?

* **An Open-Source Alternative**: Concordia was created as an open-source alternative in response to other popular mediator libraries (like MediatR) transitioning to a paid licensing model. We believe core architectural patterns should remain freely accessible to the developer community.
* **Lightweight and Minimal**: Provides only the essential Mediator pattern functionalities, without unnecessary overhead.
* **Easy DI Integration**: Integrates seamlessly with `Microsoft.Extensions.DependencyInjection`.
* **Same MediatR Interfaces**: Uses interfaces with identical signatures to MediatR, making migration or parallel adoption extremely straightforward.
* **CQRS and Pub/Sub Patterns**: Facilitates the implementation of Command Query Responsibility Segregation (CQRS) and Publisher/Subscriber principles, enhancing separation of concern and code maintainability.

## Key Features

* **Runtime Reflection-Based Registration**: Scans specified assemblies at application startup to discover and register `IRequestHandler`, `INotificationHandler`, `IPipelineBehavior`, `IRequestPreProcessor`, and `IRequestPostProcessor` implementations.
* **Familiar `AddMediator` Extension**: Provides an `AddMediator` extension method on `IServiceCollection` with a configuration builder, mirroring MediatR's setup.
* **Flexible Configuration**: Offers options to configure service lifetimes, specify custom notification publishers, and explicitly add behaviors or processors.

## Installation

Install **Concordia.Core** and **Concordia.MediatR** in your application project:

```bash
dotnet add package Concordia.Core --version 1.0.0
dotnet add package Concordia.MediatR --version 1.0.0
```

## Usage

1.  **Define your Handlers, Processors, and Behaviors** (as described in `Concordia.Core`'s documentation).

2.  **Register services in `Program.cs`**: Use the `AddMediator` extension method and configure it to scan your assemblies.

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
        // You can add other configurations here, such as:
        // cfg.Lifetime = ServiceLifetime.Scoped;
        // cfg.NotificationPublisherType = typeof(MyCustomNotificationPublisher);
        // cfg.AddOpenBehavior(typeof(MyCustomPipelineBehavior<,>));
        // cfg.AddRequestPreProcessor<MyCustomPreProcessor>();
        // cfg.AddRequestPostProcessor<MyCustomPostProcessor>();
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
}
```
## Contribution

Feel free to contribute to the project! Report bugs, suggest new features, or submit pull requests.

## License

This project is released under the [Insert your license here, e.g., MIT License].

---

## Migration Guide from MediatR

If you are migrating an existing project from MediatR to Concordia, the process is extremely simple thanks to the identical interfaces and patterns.

### 1. Update NuGet Packages

Remove the MediatR package and install the Concordia packages:

```bash
dotnet remove package MediatR
dotnet remove package MediatR.Extensions.Microsoft.DependencyInjection # If present
dotnet add package Concordia.Core --version 1.0.0
dotnet add package Concordia.MediatR --version 1.0.0
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
