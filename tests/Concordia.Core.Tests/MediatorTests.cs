using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Concordia.Contracts;
using Concordia.MediatR;
using System.Reflection;

namespace Concordia.Core.Tests;

public class MediatorTests
{
    // --- Mock Requests and Notifications ---

    public class TestRequest : IRequest<string> { public string Message { get; set; } }
    public class TestCommand : IRequest { public string CommandName { get; set; } }
    public class TestNotification : INotification { public string EventName { get; set; } }

    // --- Mock Handlers ---

    public class TestRequestHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {request.Message}");
        }
    }

    public class TestCommandHandler : IRequestHandler<TestCommand>
    {
        public bool WasHandled { get; private set; }
        public Task Handle(TestCommand request, CancellationToken cancellationToken)
        {
            WasHandled = true;
            return Task.CompletedTask;
        }
    }

    public class TestNotificationHandler1 : INotificationHandler<TestNotification>
    {
        public bool WasHandled { get; private set; }
        public Task Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            WasHandled = true;
            return Task.CompletedTask;
        }
    }

    public class TestNotificationHandler2 : INotificationHandler<TestNotification>
    {
        public bool WasHandled { get; private set; }
        public Task Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            WasHandled = true;
            return Task.CompletedTask;
        }
    }

    // --- Mock Pipeline Behaviors ---

    public class TestLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly List<string> _logs;
        public TestLoggingBehavior(List<string> logs) // Constructor now resolves List<string> from DI
        {
            _logs = logs;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logs.Add($"Before {typeof(TRequest).Name}");
            var response = await next();
            _logs.Add($"After {typeof(TRequest).Name}");
            return response;
        }
    }

    public class TestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly List<string> _logs;
        public TestValidationBehavior(List<string> logs) // Constructor now resolves List<string> from DI
        {
            _logs = logs;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logs.Add($"Validating {typeof(TRequest).Name}");
            var response = await next();
            _logs.Add($"Validation complete for {typeof(TRequest).Name}");
            return response;
        }
    }

    // --- Mock Request Pre-Processors ---
    public class TestPreProcessor1<TRequest> : IRequestPreProcessor<TRequest>
        where TRequest : IRequest
    {
        private readonly List<string> _logs;
        public TestPreProcessor1(List<string> logs) { _logs = logs; }

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            _logs.Add($"Pre-Processor 1 for {typeof(TRequest).Name}");
            return Task.CompletedTask;
        }
    }

    public class TestPreProcessor2<TRequest> : IRequestPreProcessor<TRequest>
        where TRequest : IRequest
    {
        private readonly List<string> _logs;
        public TestPreProcessor2(List<string> logs) { _logs = logs; }

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            _logs.Add($"Pre-Processor 2 for {typeof(TRequest).Name}");
            return Task.CompletedTask;
        }
    }

    // --- Mock Request Post-Processors ---
    public class TestPostProcessor1<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly List<string> _logs;
        public TestPostProcessor1(List<string> logs) { _logs = logs; }

        public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            _logs.Add($"Post-Processor 1 for {typeof(TRequest).Name} with response {response}");
            return Task.CompletedTask;
        }
    }

    public class TestPostProcessor2<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly List<string> _logs;
        public TestPostProcessor2(List<string> logs) { _logs = logs; }

        public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            _logs.Add($"Post-Processor 2 for {typeof(TRequest).Name} with response {response}");
            return Task.CompletedTask;
        }
    }

    // --- Mock of a Custom Notification Publisher ---
    public class CustomNotificationPublisher : INotificationPublisher
    {
        public bool WasCalled { get; private set; }
        public List<string> HandlersCalled { get; } = new List<string>();

        public async Task Publish(IEnumerable<Func<INotification, CancellationToken, Task>> handlerCalls, INotification notification, CancellationToken cancellationToken)
        {
            WasCalled = true;
            foreach (var handlerCall in handlerCalls)
            {
                await handlerCall(notification, cancellationToken);
                HandlersCalled.Add("Handler invoked by CustomPublisher");
            }
        }
    }


    // --- Tests for Send Method (with response) ---

    [Fact]
    public async Task Send_ShouldInvokeCorrectRequestHandler_AndReturnResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices(); // Registers IMediator and ISender
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "Hello World" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Hello World", result);
    }

    [Fact]
    public async Task Send_ShouldExecutePipelineBehaviors_InCorrectOrder()
    {
        // Arrange
        var logs = new List<string>();
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();

        services.AddSingleton(logs); // Makes the logs list available for DI
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>(); // Register the handler for TestRequest

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TestLoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TestValidationBehavior<,>));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "Hello Pipeline" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Hello Pipeline", result);
        // The pipeline execution order is determined by how delegates are wrapped.
        // The for loop in Mediator.Send goes from behaviors.Count - 1 down to 0.
        // So, the last registered behavior wraps the 'next' delegate first.
        // Registration order: Logging (first), Validation (second)
        // Wrapping order: Validation (wraps handler), Logging (wraps Validation)
        // Execution order: Logging -> Validation -> Handler
        Assert.Equal(4, logs.Count);
        Assert.Equal("Before TestRequest", logs[0]);
        Assert.Equal("Validating TestRequest", logs[1]);
        Assert.Equal("Validation complete for TestRequest", logs[2]);
        Assert.Equal("After TestRequest", logs[3]);
    }


    [Fact]
    public async Task Send_ShouldThrowException_WhenNoHandlerFound()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "No Handler" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request));
        Assert.Contains("No handler found for request of type TestRequest", exception.Message);
    }

    // --- Tests for Send Method (without response) ---

    [Fact]
    public async Task SendCommand_ShouldInvokeCorrectCommandHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        // Register TestCommandHandler as Singleton to be able to check its state
        services.AddSingleton<IRequestHandler<TestCommand>, TestCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new TestCommand { CommandName = "ProcessData" };

        // Act
        await mediator.Send(command);

        // Assert
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TestCommand>>() as TestCommandHandler;
        Assert.True(handler.WasHandled);
    }

    [Fact]
    public async Task SendCommand_ShouldThrowException_WhenNoHandlerFound()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new TestCommand { CommandName = "No Handler Command" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(command));
        Assert.Contains("No handler found for request of type TestCommand", exception.Message);
    }

    // --- Tests for Publish Method ---

    [Fact]
    public async Task Publish_ShouldInvokeAllRegisteredNotificationHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddSingleton<INotificationHandler<TestNotification>, TestNotificationHandler1>();
        services.AddSingleton<INotificationHandler<TestNotification>, TestNotificationHandler2>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new TestNotification { EventName = "ItemCreated" };

        // Act
        await mediator.Publish(notification);

        // Assert
        var handlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>().ToList();
        var handler1 = handlers.OfType<TestNotificationHandler1>().FirstOrDefault();
        var handler2 = handlers.OfType<TestNotificationHandler2>().FirstOrDefault();

        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        Assert.True(handler1.WasHandled);
        Assert.True(handler2.WasHandled);
    }

    [Fact]
    public async Task Publish_ShouldNotThrowException_WhenNoNotificationHandlersFound()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new TestNotification { EventName = "NoHandlerEvent" };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => mediator.Publish(notification));
        Assert.Null(exception);
    }

    // --- NEW TESTS: Pre-Processors ---

    [Fact]
    public async Task Send_ShouldExecutePreProcessors_ForRequestWithResponse()
    {
        // Arrange
        var logs = new List<string>();
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddSingleton(logs); // Makes the logs list available for DI
        services.AddTransient(typeof(IRequestHandler<TestRequest, string>), typeof(TestRequestHandler));
        // Register open generic pre-processor types with their open generic implementations
        services.AddTransient(typeof(IRequestPreProcessor<>), typeof(TestPreProcessor1<>));
        services.AddTransient(typeof(IRequestPreProcessor<>), typeof(TestPreProcessor2<>));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "Test Pre-Processors" };

        // Act
        await mediator.Send(request);

        // Assert
        Assert.Equal(2, logs.Count);
        Assert.Equal("Pre-Processor 1 for TestRequest", logs[0]);
        Assert.Equal("Pre-Processor 2 for TestRequest", logs[1]);
    }

    [Fact]
    public async Task Send_ShouldExecutePreProcessors_ForRequestWithoutResponse()
    {
        // Arrange
        var logs = new List<string>();
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddSingleton(logs);
        services.AddTransient(typeof(IRequestHandler<TestCommand>), typeof(TestCommandHandler));
        // Register open generic pre-processor types with their open generic implementations
        services.AddTransient(typeof(IRequestPreProcessor<>), typeof(TestPreProcessor1<>));
        services.AddTransient(typeof(IRequestPreProcessor<>), typeof(TestPreProcessor2<>));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new TestCommand { CommandName = "Test Pre-Processors Command" };

        // Act
        await mediator.Send(command);

        // Assert
        Assert.Equal(2, logs.Count);
        Assert.Equal("Pre-Processor 1 for TestCommand", logs[0]);
        Assert.Equal("Pre-Processor 2 for TestCommand", logs[1]);
    }

    // --- NEW TESTS: Post-Processors ---

    [Fact]
    public async Task Send_ShouldExecutePostProcessors_ForRequestWithResponse()
    {
        // Arrange
        var logs = new List<string>();
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddSingleton(logs); // Makes the logs list available for DI
        services.AddTransient(typeof(IRequestHandler<TestRequest, string>), typeof(TestRequestHandler));
        // Register open generic post-processor types with their open generic implementations
        services.AddTransient(typeof(IRequestPostProcessor<,>), typeof(TestPostProcessor1<,>));
        services.AddTransient(typeof(IRequestPostProcessor<,>), typeof(TestPostProcessor2<,>));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "Test Post-Processors" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Test Post-Processors", result);
        Assert.Equal(2, logs.Count);
        // Post-processors execution order is the registration order
        Assert.Equal("Post-Processor 1 for TestRequest with response Handled: Test Post-Processors", logs[0]);
        Assert.Equal("Post-Processor 2 for TestRequest with response Handled: Test Post-Processors", logs[1]);
    }

    // --- NEW TEST: Custom Notification Publisher via AddConcordiaCoreServices<TNotificationPublisher> ---
    [Fact]
    public async Task Publish_ShouldUseCustomNotificationPublisher_WhenRegisteredViaCoreServices()
    {
        // Arrange
        var services = new ServiceCollection();
        // Use the new overload to register the custom publisher type
        services.AddConcordiaCoreServices<CustomNotificationPublisher>();
        services.AddSingleton<INotificationHandler<TestNotification>, TestNotificationHandler1>();
        services.AddSingleton<INotificationHandler<TestNotification>, TestNotificationHandler2>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new TestNotification { EventName = "Custom Publisher Event via Core Services" };

        // Get the registered custom publisher instance to check its state
        var customPublisher = serviceProvider.GetRequiredService<INotificationPublisher>() as CustomNotificationPublisher;
        Assert.NotNull(customPublisher); // Ensure it's our custom publisher

        // Act
        await mediator.Publish(notification);

        // Assert
        Assert.True(customPublisher.WasCalled);
        Assert.Equal(2, customPublisher.HandlersCalled.Count);
        Assert.True(customPublisher.HandlersCalled.All(s => s == "Handler invoked by CustomPublisher"));

        // Also verify that the handlers were actually called
        var handlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>().ToList();
        var handler1 = handlers.OfType<TestNotificationHandler1>().FirstOrDefault();
        var handler2 = handlers.OfType<TestNotificationHandler2>().FirstOrDefault();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        Assert.True(handler1.WasHandled);
        Assert.True(handler2.WasHandled);
    }


    // --- ORIGINAL TEST: Custom Notification Publisher via AddMediator (needs further debugging) ---
    [Fact]
    public async Task Publish_ShouldUseCustomNotificationPublisher_WhenRegistered()
    {
        // Arrange
        var customPublisher = new CustomNotificationPublisher();
        var services = new ServiceCollection();

        // Register the logs list, as behaviors (if discovered by scanning) might need it.
        // This ensures all dependencies are met when AddMediator scans the assembly.
        var logs = new List<string>();
        services.AddSingleton(logs);

        // Use AddMediator to register the custom publisher and handlers
        services.AddMediator(cfg =>
        {
            cfg.NotificationPublisher = customPublisher; // Pass the custom publisher instance
                                                         // CRITICAL: Disable assembly scanning for this specific test
                                                         // This prevents AddMediator from trying to auto-register behaviors that require 'logs'
                                                         // which would cause an an ArgumentException due to open generic type instantiation issues.
            cfg.DisableAssemblyScanning = true;

            // Explicitly register notification handlers needed for this test using the new dedicated method.
            cfg.AddNotificationHandler<TestNotificationHandler1>(ServiceLifetime.Singleton);
            cfg.AddNotificationHandler<TestNotificationHandler2>(ServiceLifetime.Singleton);
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new TestNotification { EventName = "Custom Publisher Event" };

        // Act
        await mediator.Publish(notification);

        // Assert
        Assert.True(customPublisher.WasCalled);
        Assert.Equal(2, customPublisher.HandlersCalled.Count);
        Assert.True(customPublisher.HandlersCalled.All(s => s == "Handler invoked by CustomPublisher"));

        // Also verify that the handlers were actually called
        var handlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>().ToList();
        var handler1 = handlers.OfType<TestNotificationHandler1>().FirstOrDefault();
        var handler2 = handlers.OfType<TestNotificationHandler2>().FirstOrDefault();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        Assert.True(handler1.WasHandled);
        Assert.True(handler2.WasHandled);
    }
}