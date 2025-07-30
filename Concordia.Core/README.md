# Concordia.Core: The Foundation of Your .NET Mediator

**Concordia.Core** is the foundational package of the Concordia library. It provides the essential interfaces and the core `Mediator` implementation for building robust and maintainable applications using the Mediator pattern.

## Why Concordia?

* **An Open-Source Alternative**: Concordia was created as an open-source alternative in response to other popular mediator libraries (like MediatR) transitioning to a paid licensing model. We believe core architectural patterns should remain freely accessible to the developer community.
* **Lightweight and Minimal**: Provides only the essential Mediator pattern functionalities, without unnecessary overhead.
* **Optimized Performance**: While this core package doesn't include the Source Generator, it's designed to work seamlessly with it for compile-time handler registration, ensuring faster application startup and zero runtime reflection.
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

## Installation

Install **Concordia.Core** in your application project:

```bash
dotnet add package Concordia.Core --version 1.0.0
```

## Usage

After installing, you can define your requests, commands, and notifications by implementing the interfaces from `Concordia.Contracts`.

```csharp
// Request with response
using Concordia.Contracts;

namespace MyProject.Requests
{
    public class GetProductByIdQuery : IRequest<ProductDto>
    {
        public int ProductId { get; set; }
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

You can then register Concordia's core services in your `Program.cs` or `Startup.cs`:

```csharp
using Concordia.DependencyInjection; // For AddConcordiaCoreServices

var builder = WebApplication.CreateBuilder(args);

// Register Concordia's core services (IMediator, ISender).
// This method comes directly from the Concordia.Core library.
builder.Services.AddConcordiaCoreServices();

// Optionally, register a custom notification publisher:
// builder.Services.AddConcordiaCoreServices<MyCustomNotificationPublisher>();
```

For automatic handler discovery and registration, you will typically pair this with `Concordia.Generator` or `Concordia.MediatR`.

## Contribution

Feel free to contribute to the project! Report bugs, suggest new features, or submit pull requests.

## License

This project is released under the [Insert your license here, e.g., MIT License].
