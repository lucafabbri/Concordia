# Concordia.Core: The Foundation of Your .NET Mediator

**Concordia.Core** is the foundational package of the Concordia library. It provides the essential interfaces and the core `Mediator` implementation for building robust and maintainable applications using the Mediator pattern. By decoupling components and promoting a clean, command-based architecture, Concordia helps you write scalable and testable code.

# Table of Contents
- [Why Concordia?](#why-concordia)
- [Key Features](#key-features)
- [Installation](#installation)
- [Usage](#usage)
- [Migration Guide from MediatR](#migration-guide-from-mediatr)
- [Contributing](#contributing)
- [License](#license)
- [NuGet Packages](#nuget-packages)
- [Contact](#contact)
- [Support](#support)

## Why Concordia?

* **An Open-Source Alternative**: Concordia was created as an open-source alternative in response to other popular mediator libraries (like MediatR) transitioning to a paid licensing model. We believe core architectural patterns should remain freely accessible to the developer community, fostering innovation and collaboration without imposing financial barriers. Concordia is our commitment to this principle.

* **Lightweight and Minimal**: In a world of increasingly complex frameworks, Concordia provides only the essential Mediator pattern functionalities without unnecessary overhead. This focused approach means a smaller library, a gentler learning curve, and less cognitive load for developers. You get exactly what you need to implement CQRS and the Mediator pattern effectively.

* **Optimized Performance**: While this core package is designed for maximum compatibility, it shines when paired with `Concordia.Generator`. This source generator performs compile-time handler registration, completely eliminating the need for runtime reflection. This results in significantly faster application startup times and reduced memory consumption, which is critical for high-performance services and serverless environments.

* **Easy DI Integration**: Built with modern .NET applications in mind, Concordia integrates seamlessly with `Microsoft.Extensions.DependencyInjection`. Registration is straightforward and familiar, allowing you to get up and running in minutes without complex configuration.

* **Same MediatR Interfaces**: To ensure a smooth transition for developers, Concordia uses interfaces with identical signatures to MediatR. This design choice makes migration from MediatR incredibly simple and allows teams to adopt Concordia incrementally or even use both libraries in parallel during a transition period, minimizing disruption.

* **CQRS and Pub/Sub Patterns**: The library is a natural fit for implementing Command Query Responsibility Segregation (CQRS) and Publisher/Subscriber patterns. `IRequest`/`IRequestHandler` map directly to the Command/Query aspect of CQRS, while `INotification`/`INotificationHandler` provide a powerful and simple mechanism for implementing the event-based Pub/Sub pattern, enhancing separation of concerns and code maintainability.

## Key Features

* **Requests with Responses (`IRequest<TResponse>`, `IRequestHandler<TRequest, TResponse>`)**: Ideal for operations that must return a result, such as fetching data from a database (Queries) or executing a command that returns the state of a newly created entity.

* **Fire-and-Forget Requests (`IRequest`, `IRequestHandler<TRequest>`)**: Perfect for commands that do not need to return a value, such as enqueuing a background job, deleting a record, or updating a record where the client doesn't need immediate feedback.

* **Notifications (`INotification`, `INotificationHandler<TNotification>`)**: A powerful tool for publishing events to zero or more handlers. This enables a decoupled architecture where multiple parts of an application can react to a single event (e.g., `UserCreatedNotification`) without being directly coupled to the originator of the event.

* **`IMediator`**: The primary interface for application logic. It unifies the sending of requests and the publishing of notifications into a single, cohesive API, serving as the central point of interaction for your application's components.

* **`ISender`**: A focused interface for sending requests (commands and queries). This is often preferred in components that only need to dispatch operations, as it adheres to the Interface Segregation Principle by not exposing the notification publishing capabilities.

* **Pipeline Behaviors (`IPipelineBehavior<TRequest, TResponse>`)**: A cornerstone of extensible architectures, pipeline behaviors allow you to intercept requests and wrap additional logic around their handlers. This is the perfect place to implement cross-cutting concerns like logging, validation, caching, and transactional behavior in a clean and reusable way.

* **Request Pre-Processors (`IRequestPreProcessor<TRequest>`)**: Provides a hook to execute logic immediately *before* a request handler is invoked. Unlike pipeline behaviors, they don't wrap the handler but are executed as a distinct preliminary step. `Concordia.Core` includes the `RequestPreProcessorBehavior` to automatically discover and run all registered pre-processors.

* **Request Post-Processors (`IRequestPostProcessor<TRequest, TResponse>`)**: Allows you to execute logic *after* a request handler has completed but *before* the response is returned to the caller. This is useful for tasks like logging the outcome of an operation or auditing results. `Concordia.Core` includes the `RequestPostProcessorBehavior` to automatically execute all registered post-processors.

* **Stream Pipeline Behaviors (`IStreamPipelineBehavior<TRequest, TResponse>`)**: Designed for future support of streaming requests, allowing you to intercept and manage data streams in a similar fashion to standard pipeline behaviors.

* **Custom Notification Publishers (`INotificationPublisher`)**: Gives you full control over how notifications are dispatched to their handlers. `Concordia.Core` provides two powerful built-in strategies to cover the most common scenarios:
  * **`ForeachAwaitPublisher` (Default)**: Publishes notifications to all handlers **sequentially**, awaiting the completion of each one before proceeding to the next. This strategy is essential when the order of execution is important, for example, when one handler must complete its transaction before another begins.
  * **`TaskWhenAllPublisher`**: Publishes notifications to all handlers **in parallel** using `Task.WhenAll`. This strategy can significantly improve performance when the handlers are independent and can run concurrently, such as sending a welcome email, updating a read model, and pushing a real-time notification to a client.

## Installation

Install **Concordia.Core** in your application project. It's always a good practice to check for the latest stable version on NuGet.

```bash
dotnet add package Concordia.Core --version 1.0.0
```

## Usage

After installing, you can define your requests, commands, and notifications by implementing the interfaces from `Concordia`.

```csharp
// Request with response
using Concordia;

namespace MyProject.Requests
{
    // Represents a query to fetch a product
    public class GetProductByIdQuery : IRequest<ProductDto>
    {
        public int ProductId { get; set; }
    }
}

// Fire-and-forget command
using Concordia;

namespace MyProject.Commands
{
    // Represents a command to create a product
    public class CreateProductCommand : IRequest
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
}

// Notification
using Concordia;

namespace MyProject.Notifications
{
    // Represents an event that is published after a product is created
    public class ProductCreatedNotification : INotification
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
}
```

You can then register Concordia's core services in your `Program.cs` or `Startup.cs`:

```csharp
using Concordia; // Namespace for the built-in publishers
using Concordia.DependencyInjection; // For AddConcordiaCoreServices

var builder = WebApplication.CreateBuilder(args);

// Register Concordia's core services (IMediator, ISender).
// This method comes directly from the Concordia.Core library.
// By default, it uses the ForeachAwaitPublisher for sequential notification handling.
builder.Services.AddConcordiaCoreServices();

// Optionally, you can specify a different notification publishing strategy.
// For example, to run all notification handlers in parallel for better performance:
// builder.Services.AddConcordiaCoreServices<TaskWhenAllPublisher>();
```

For automatic handler discovery and registration, you will typically pair this with `Concordia.Generator` or `Concordia.MediatR`.

## Contribution

Feel free to contribute to the project! We welcome bug reports, feature suggestions, and pull requests. Your involvement helps make Concordia better for everyone.
Please follow the [Contributing Guidelines](https://github.com/lucafabbri/Concordia/blob/main/CONTRIBUTING.md).

## License

This project is released under the permissive [MIT License](https://opensource.org/licenses/MIT). See the [LICENSE](https://github.com/lucafabbri/Concordia/blob/main/LICENSE) file for more information.

## NuGet Packages
- [Concordia.Core](https://www.nuget.org/packages/Concordia.Core)
- [Concordia.Generator](https://www.nuget.org/packages/Concordia.Generator) (for compile-time handler registration)
- [Concordia.MediatR](https://www.nuget.org/packages/Concordia.MediatR) (for MediatR compatibility)

## Contact
For any questions, issues, or feedback, please open an issue on the [GitHub repository](https://github.com/lucafabbri/Concordia/issues).

## Support
If you find this library useful, consider supporting its development. Your support helps maintain the project and fund future enhancements. [Buy Me a Coffee](https://www.buymeacoffee.com/lucafabbriu).
