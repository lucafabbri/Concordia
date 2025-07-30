# Concordia: A Lightweight and Powerful .NET Mediator

Concordia is a .NET library implementing the **Mediator pattern**, designed to be lightweight, performant, and easily integrated with the .NET Dependency Injection system. It leverages **C# Source Generators** for automatic handler registration at compile-time, eliminating the need for runtime reflection and improving application startup performance.

## Why Concordia?

* **An Open-Source Alternative**: Concordia was created as an open-source alternative in response to other popular mediator libraries (like MediatR) transitioning to a paid licensing model. We believe core architectural patterns should remain freely accessible to the developer community.

* **Lightweight and Minimal**: Provides only the essential Mediator pattern functionalities, without unnecessary overhead.

* **Optimized Performance**: Thanks to Source Generators, handler discovery and registration happen entirely at compile-time, ensuring faster application startup and zero runtime reflection.

* **Easy DI Integration**: Integrates seamlessly with `Microsoft.Extensions.DependencyInjection`.

* **Same MediatR Interfaces**: Uses interfaces with identical signatures to MediatR, making migration or parallel adoption extremely straightforward.

* **CQRS and Pub/Sub Patterns**: Facilitates the implementation of Command Query Responsibility Segregation (CQRS) and Publisher/Subscriber principles, enhancing separation of concerns and code maintainability.

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

## Installation

Concordia is distributed via three NuGet packages, all currently at **version 1.0.0**:

1.  **`Concordia.Core`**: Contains the interfaces (`IMediator`, `ISender`, `IRequest`, etc.), the `Mediator` implementation, and core DI extension methods.

2.  **`Concordia.Generator`**: Contains the C# Source Generator for compile-time handler registration.

3.  **`Concordia.MediatR`**: Provides a compatibility layer with MediatR's `AddMediator` extension method for runtime reflection-based handler registration, now using its own `ConcordiaMediatRServiceConfiguration`.

To get started with Concordia, install the necessary packages in your application project (e.g., an ASP.NET Core project) using the .NET CLI. You will typically choose **either `Concordia.Generator` OR `Concordia.MediatR`** based on your preference for handler registration.

**Option 1: Using the Source Generator (Recommended for New Projects)**

```bash
dotnet add package Concordia.Core --version 1.0.0
dotnet add package Concordia.Generator --version 1.0.0
```

**Option 2: Using the MediatR Compatibility Layer (For Migration or Reflection Preference)**

```bash
dotnet add package Concordia.Core --version 1.0.0
dotnet add package Concordia.MediatR --version 1.0.0
```

Alternatively, you can install them via the NuGet Package Manager in Visual Studio.

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

// Example Pipeline Behavior (already in previous examples)
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
//             var response = await next();
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

```csharp
using Concordia; // Required for IMediator, ISender
using Concordia.DependencyInjection; // For AddConcordiaCoreServices
using Microsoft.AspNetCore.Mvc;
using Concordia.Examples.Web; // Example: Namespace where ConcordiaGeneratedRegistrations is generated

var builder = WebApplication.CreateBuilder(args);

// 1. Register Concordia's core services (IMediator, ISender).
// Puoi usare il metodo senza parametri per il publisher di default, oppure:
builder.Services.AddConcordiaCoreServices<Concordia.ForeachAwaitPublisher>(); // Esempio: Registra il publisher di default esplicitamente
// Oppure, se hai un publisher personalizzato:
// builder.Services.AddConcordiaCoreServices<MyCustomNotificationPublisher>(); // Esempio: Registra il tuo publisher personalizzato

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

    // Esempi di richieste, comandi, notifiche e handler per il progetto web
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
    
    // Esempio: Registrare tutti i servizi come Scoped
    cfg.Lifetime = ServiceLifetime.Scoped;

    // Esempio: Registrare un publisher di notifiche personalizzato
    // cfg.NotificationPublisherType = typeof(MyCustomNotificationPublisher);

    // Esempio: Aggiungere un pre-processore in modo esplicito
    // cfg.AddRequestPreProcessor<MyCustomPreProcessor>();

    // Esempio: Aggiungere un post-processore in modo esplicito
    // cfg.AddRequestPostProcessor<MyCustomPostProcessor>();

    // Esempio: Aggiungere un comportamento di stream in modo esplicito
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

    // Esempi di richieste, comandi, notifiche e handler per il progetto web
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
