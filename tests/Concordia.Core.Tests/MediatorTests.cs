using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Concordia;
using Concordia.MediatR;
using System.Reflection;

namespace Concordia.Core.Tests;

/// <summary>
/// The mediator tests class
/// </summary>
public class MediatorTests
{
    // --- Mock Requests and Notifications ---

    /// <summary>
    /// The test request class
    /// </summary>
    /// <seealso cref="IRequest{string}"/>
    public class TestRequest : IRequest<string> { /// <summary>
/// Gets or sets the value of the message
/// </summary>
public string Message { get; set; } }
    /// <summary>
    /// The test command class
    /// </summary>
    /// <seealso cref="IRequest"/>
    public class TestCommand : IRequest { /// <summary>
/// Gets or sets the value of the command name
/// </summary>
public string CommandName { get; set; } }
    /// <summary>
    /// The test notification class
    /// </summary>
    /// <seealso cref="INotification"/>
    public class TestNotification : INotification { /// <summary>
/// Gets or sets the value of the event name
/// </summary>
public string EventName { get; set; } }

    // --- Mock Handlers ---

    /// <summary>
    /// The test request handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{TestRequest, string}"/>
    public class TestRequestHandler : IRequestHandler<TestRequest, string>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the string</returns>
        public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {request.Message}");
        }
    }

    /// <summary>
    /// The test command handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{TestCommand}"/>
    public class TestCommandHandler : IRequestHandler<TestCommand>
    {
        /// <summary>
        /// Gets or sets the value of the was handled
        /// </summary>
        public bool WasHandled { get; private set; }
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(TestCommand request, CancellationToken cancellationToken)
        {
            WasHandled = true;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// The test notification handler class
    /// </summary>
    /// <seealso cref="INotificationHandler{TestNotification}"/>
    public class TestNotificationHandler1 : INotificationHandler<TestNotification>
    {
        /// <summary>
        /// Gets or sets the value of the was handled
        /// </summary>
        public bool WasHandled { get; private set; }
        /// <summary>
        /// Handles the notification
        /// </summary>
        /// <param name="notification">The notification</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            WasHandled = true;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// The test notification handler class
    /// </summary>
    /// <seealso cref="INotificationHandler{TestNotification}"/>
    public class TestNotificationHandler2 : INotificationHandler<TestNotification>
    {
        /// <summary>
        /// Gets or sets the value of the was handled
        /// </summary>
        public bool WasHandled { get; private set; }
        /// <summary>
        /// Handles the notification
        /// </summary>
        /// <param name="notification">The notification</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            WasHandled = true;
            return Task.CompletedTask;
        }
    }

    // --- Mock Pipeline Behaviors ---

    /// <summary>
    /// The test logging behavior class
    /// </summary>
    /// <seealso cref="IPipelineBehavior{TRequest, TResponse}"/>
    public class TestLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// The logs
        /// </summary>
        private readonly List<string> _logs;
        /// <summary>
        /// Initializes a new instance of the <see cref="TestLoggingBehavior{TRequest,TResponse}"/> class
        /// </summary>
        /// <param name="logs">The logs</param>
        public TestLoggingBehavior(List<string> logs) // Constructor now resolves List<string> from DI
        {
            _logs = logs;
        }

        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="next">The next</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The response</returns>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logs.Add($"Before {typeof(TRequest).Name}");
            var response = await next(cancellationToken);
            _logs.Add($"After {typeof(TRequest).Name}");
            return response;
        }
    }

    /// <summary>
    /// The test validation behavior class
    /// </summary>
    /// <seealso cref="IPipelineBehavior{TRequest, TResponse}"/>
    public class TestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// The logs
        /// </summary>
        private readonly List<string> _logs;
        /// <summary>
        /// Initializes a new instance of the <see cref="TestValidationBehavior{TRequest,TResponse}"/> class
        /// </summary>
        /// <param name="logs">The logs</param>
        public TestValidationBehavior(List<string> logs) // Constructor now resolves List<string> from DI
        {
            _logs = logs;
        }

        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="next">The next</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The response</returns>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logs.Add($"Validating {typeof(TRequest).Name}");
            var response = await next(cancellationToken);
            _logs.Add($"Validation complete for {typeof(TRequest).Name}");
            return response;
        }
    }

    // --- Mock Request Pre-Processors ---
    /// <summary>
    /// The test pre processor class
    /// </summary>
    /// <seealso cref="IRequestPreProcessor{TRequest}"/>
    public class TestPreProcessor1<TRequest> : IRequestPreProcessor<TRequest>
        where TRequest : IRequest
    {
        /// <summary>
        /// The logs
        /// </summary>
        private readonly List<string> _logs;
        /// <summary>
        /// Initializes a new instance of the <see cref="TestPreProcessor1{TRequest}"/> class
        /// </summary>
        /// <param name="logs">The logs</param>
        public TestPreProcessor1(List<string> logs) { _logs = logs; }

        /// <summary>
        /// Processes the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            _logs.Add($"Pre-Processor 1 for {typeof(TRequest).Name}");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// The test pre processor class
    /// </summary>
    /// <seealso cref="IRequestPreProcessor{TRequest}"/>
    public class TestPreProcessor2<TRequest> : IRequestPreProcessor<TRequest>
        where TRequest : IRequest
    {
        /// <summary>
        /// The logs
        /// </summary>
        private readonly List<string> _logs;
        /// <summary>
        /// Initializes a new instance of the <see cref="TestPreProcessor2{TRequest}"/> class
        /// </summary>
        /// <param name="logs">The logs</param>
        public TestPreProcessor2(List<string> logs) { _logs = logs; }

        /// <summary>
        /// Processes the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            _logs.Add($"Pre-Processor 2 for {typeof(TRequest).Name}");
            return Task.CompletedTask;
        }
    }

    // --- Mock Request Post-Processors ---
    /// <summary>
    /// The test post processor class
    /// </summary>
    /// <seealso cref="IRequestPostProcessor{TRequest, TResponse}"/>
    public class TestPostProcessor1<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// The logs
        /// </summary>
        private readonly List<string> _logs;
        /// <summary>
        /// Initializes a new instance of the <see cref="TestPostProcessor1{TRequest,TResponse}"/> class
        /// </summary>
        /// <param name="logs">The logs</param>
        public TestPostProcessor1(List<string> logs) { _logs = logs; }

        /// <summary>
        /// Processes the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="response">The response</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            _logs.Add($"Post-Processor 1 for {typeof(TRequest).Name} with response {response}");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// The test post processor class
    /// </summary>
    /// <seealso cref="IRequestPostProcessor{TRequest, TResponse}"/>
    public class TestPostProcessor2<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// The logs
        /// </summary>
        private readonly List<string> _logs;
        /// <summary>
        /// Initializes a new instance of the <see cref="TestPostProcessor2{TRequest,TResponse}"/> class
        /// </summary>
        /// <param name="logs">The logs</param>
        public TestPostProcessor2(List<string> logs) { _logs = logs; }

        /// <summary>
        /// Processes the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="response">The response</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            _logs.Add($"Post-Processor 2 for {typeof(TRequest).Name} with response {response}");
            return Task.CompletedTask;
        }
    }

    // --- Mock of a Custom Notification Publisher ---
    /// <summary>
    /// The custom notification publisher class
    /// </summary>
    /// <seealso cref="INotificationPublisher"/>
    public class CustomNotificationPublisher : INotificationPublisher
    {
        /// <summary>
        /// Gets or sets the value of the was called
        /// </summary>
        public bool WasCalled { get; private set; }
        /// <summary>
        /// Gets the value of the handlers called
        /// </summary>
        public List<string> HandlersCalled { get; } = new List<string>();

        /// <summary>
        /// Publishes the handler calls
        /// </summary>
        /// <param name="handlerCalls">The handler calls</param>
        /// <param name="notification">The notification</param>
        /// <param name="cancellationToken">The cancellation token</param>
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

    /// <summary>
    /// Tests that send should invoke correct request handler and return response
    /// </summary>
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

    /// <summary>
    /// Tests that send should execute pipeline behaviors in correct order
    /// </summary>
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


    /// <summary>
    /// Tests that send should throw exception when no handler found
    /// </summary>
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

    /// <summary>
    /// Tests that send command should invoke correct command handler
    /// </summary>
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

    /// <summary>
    /// Tests that send command should throw exception when no handler found
    /// </summary>
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

    /// <summary>
    /// Tests that publish should invoke all registered notification handlers
    /// </summary>
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

    /// <summary>
    /// Tests that publish should not throw exception when no notification handlers found
    /// </summary>
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

    /// <summary>
    /// Tests that send should execute pre processors for request with response
    /// </summary>
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

    /// <summary>
    /// Tests that send should execute pre processors for request without response
    /// </summary>
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

    /// <summary>
    /// Tests that send should execute post processors for request with response
    /// </summary>
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
    /// <summary>
    /// Tests that publish should use custom notification publisher when registered via core services
    /// </summary>
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
    /// <summary>
    /// Tests that publish should use custom notification publisher when registered
    /// </summary>
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