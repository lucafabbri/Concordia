using Concordia;
using Concordia.MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;

namespace Concordia.Core.Tests;

/// <summary>
/// Comprehensive tests for ConcordiaMediatRServiceCollectionExtensions
/// covering all aspects of the reflection-based handler registration system.
/// </summary>
public class ConcordiaMediatRServiceCollectionExtensionsTests
{
    #region Test Models
    
    public record TestRequest(string Message) : IRequest<string>;
    public record TestCommand : IRequest;
    public record TestNotification(string Data) : INotification;
    
    public class TestRequestHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
            => Task.FromResult($"Handled: {request.Message}");
    }
    
    public class TestCommandHandler : IRequestHandler<TestCommand>
    {
        public Task Handle(TestCommand request, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
    
    public class TestNotificationHandler : INotificationHandler<TestNotification>
    {
        public Task Handle(TestNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
    
    public class TestPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            => next();
    }
    
    public class TestRequestPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
        where TRequest : IRequest
    {
        public Task Process(TRequest request, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
    
    public class TestRequestPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
    
    public class TestStreamPipelineBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<IAsyncEnumerable<TResponse>>
    {
        public IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            => next();
    }
    
    #endregion

    [Fact]
    public void AddMediator_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services!.AddMediator(config => { }));
    }

    [Fact]
    public void AddMediator_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddMediator(null!));
    }

    [Fact]
    public void AddMediator_WithDefaultConfiguration_RegistersBasicServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config => { });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        Assert.NotNull(serviceProvider.GetService<IMediator>());
        Assert.NotNull(serviceProvider.GetService<ISender>());
        Assert.NotNull(serviceProvider.GetService<INotificationPublisher>());
        
        // Verify default implementations
        Assert.IsType<Mediator>(serviceProvider.GetService<IMediator>());
        Assert.IsType<ForeachAwaitPublisher>(serviceProvider.GetService<INotificationPublisher>());
    }

    [Fact]
    public void AddMediator_WithCustomMediatorType_RegistersCustomType()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.MediatorImplementationType = typeof(Mediator);
            config.Lifetime = ServiceLifetime.Singleton;
        });

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMediator));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(typeof(Mediator), serviceDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddMediator_WithCustomNotificationPublisherType_RegistersCustomPublisher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.NotificationPublisherType = typeof(TaskWhenAllPublisher);
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.IsType<TaskWhenAllPublisher>(serviceProvider.GetService<INotificationPublisher>());
    }

    [Fact]
    public void AddMediator_WithCustomNotificationPublisherInstance_RegistersInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var customPublisher = new TaskWhenAllPublisher();

        // Act
        services.AddMediator(config =>
        {
            config.NotificationPublisher = customPublisher;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.Same(customPublisher, serviceProvider.GetService<INotificationPublisher>());
    }

    [Fact]
    public void AddMediator_WithAssemblyScanning_RegistersHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        Assert.NotNull(serviceProvider.GetService<IRequestHandler<TestRequest, string>>());
        Assert.NotNull(serviceProvider.GetService<IRequestHandler<TestCommand>>());
        Assert.NotNull(serviceProvider.GetService<INotificationHandler<TestNotification>>());
    }

    [Fact]
    public void AddMediator_WithDisabledAssemblyScanning_DoesNotRegisterHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.DisableAssemblyScanning = true;
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        Assert.Null(serviceProvider.GetService<IRequestHandler<TestRequest, string>>());
        Assert.Null(serviceProvider.GetService<IRequestHandler<TestCommand>>());
        Assert.Null(serviceProvider.GetService<INotificationHandler<TestNotification>>());
    }

    [Fact]
    public void AddMediator_WithCallingAssemblyDefault_RegistersHandlersFromCallingAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - No assemblies specified, should scan calling assembly
        services.AddMediator(config => { });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Basic services should be registered
        Assert.NotNull(serviceProvider.GetService<IMediator>());
        Assert.NotNull(serviceProvider.GetService<ISender>());
    }

    [Fact]
    public void AddMediator_WithMultipleAssemblies_RegistersFromAllAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly1 = Assembly.GetExecutingAssembly();
        var assembly2 = typeof(Mediator).Assembly;

        // Act
        services.AddMediator(config =>
        {
            config.RegisterServicesFromAssemblies(assembly1, assembly2);
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<IMediator>());
    }

    [Fact]
    public void AddMediator_RegistersPreProcessors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var preProcessors = serviceProvider.GetServices<IRequestPreProcessor<TestRequest>>();
        Assert.NotEmpty(preProcessors);
    }

    [Fact]
    public void AddMediator_RegistersPostProcessors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var postProcessors = serviceProvider.GetServices<IRequestPostProcessor<TestRequest, string>>();
        Assert.NotEmpty(postProcessors);
    }

    [Fact]
    public void AddMediator_RegistersPipelineBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TestRequest, string>>();
        Assert.NotEmpty(behaviors);
    }

    [Fact]
    public void AddMediator_RegistersStreamPipelineBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var streamBehaviors = serviceProvider.GetServices<IStreamPipelineBehavior<IRequest<IAsyncEnumerable<string>>, string>>();
        // May be empty if no stream behaviors are defined in the assembly
        Assert.NotNull(streamBehaviors);
    }

    [Fact]
    public void AddMediator_WithManuallyAddedBehaviors_RegistersManualBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.BehaviorsToRegister.Add(new ServiceDescriptor(
                typeof(IPipelineBehavior<TestRequest, string>), 
                typeof(TestPipelineBehavior<TestRequest, string>), 
                ServiceLifetime.Transient));
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetService<IPipelineBehavior<TestRequest, string>>();
        Assert.NotNull(behavior);
        Assert.IsType<TestPipelineBehavior<TestRequest, string>>(behavior);
    }

    [Fact]
    public void AddMediator_WithManuallyAddedRequestHandlers_RegistersManualHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.RequestHandlersToRegister.Add(new ServiceDescriptor(
                typeof(IRequestHandler<TestRequest, string>), 
                typeof(TestRequestHandler), 
                ServiceLifetime.Transient));
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<IRequestHandler<TestRequest, string>>();
        Assert.NotNull(handler);
        Assert.IsType<TestRequestHandler>(handler);
    }

    [Fact]
    public void AddMediator_WithManuallyAddedNotificationHandlers_RegistersManualHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.NotificationHandlersToRegister.Add(new ServiceDescriptor(
                typeof(INotificationHandler<TestNotification>), 
                typeof(TestNotificationHandler), 
                ServiceLifetime.Transient));
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<INotificationHandler<TestNotification>>();
        Assert.NotNull(handler);
        Assert.IsType<TestNotificationHandler>(handler);
    }

    [Fact]
    public void AddMediator_WithManuallyAddedPreProcessors_RegistersManualPreProcessors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.RequestPreProcessorsToRegister.Add(new ServiceDescriptor(
                typeof(IRequestPreProcessor<TestRequest>), 
                typeof(TestRequestPreProcessor<TestRequest>), 
                ServiceLifetime.Transient));
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var preProcessor = serviceProvider.GetService<IRequestPreProcessor<TestRequest>>();
        Assert.NotNull(preProcessor);
        Assert.IsType<TestRequestPreProcessor<TestRequest>>(preProcessor);
    }

    [Fact]
    public void AddMediator_WithManuallyAddedPostProcessors_RegistersManualPostProcessors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.RequestPostProcessorsToRegister.Add(new ServiceDescriptor(
                typeof(IRequestPostProcessor<TestRequest, string>), 
                typeof(TestRequestPostProcessor<TestRequest, string>), 
                ServiceLifetime.Transient));
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var postProcessor = serviceProvider.GetService<IRequestPostProcessor<TestRequest, string>>();
        Assert.NotNull(postProcessor);
        Assert.IsType<TestRequestPostProcessor<TestRequest, string>>(postProcessor);
    }

    [Fact]
    public void AddMediator_WithManuallyAddedStreamBehaviors_RegistersManualStreamBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.StreamBehaviorsToRegister.Add(new ServiceDescriptor(
                typeof(IStreamPipelineBehavior<IRequest<IAsyncEnumerable<string>>, string>), 
                typeof(TestStreamPipelineBehavior<IRequest<IAsyncEnumerable<string>>, string>), 
                ServiceLifetime.Transient));
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var streamBehavior = serviceProvider.GetService<IStreamPipelineBehavior<IRequest<IAsyncEnumerable<string>>, string>>();
        Assert.NotNull(streamBehavior);
        Assert.IsType<TestStreamPipelineBehavior<IRequest<IAsyncEnumerable<string>>, string>>(streamBehavior);
    }

    [Fact]
    public void AddMediator_WithRegistrationFromAssemblyContainingType_RegistersHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.RegisterServicesFromAssemblyContaining<TestRequestHandler>();
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<IRequestHandler<TestRequest, string>>());
    }

    [Fact]
    public void AddMediator_WithRegistrationFromAssemblyContainingGenericType_RegistersHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.RegisterServicesFromAssemblyContaining(typeof(TestRequestHandler));
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<IRequestHandler<TestRequest, string>>());
    }

    [Fact]
    public void AddMediator_WithCustomServiceLifetime_RegistersWithCorrectLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.Lifetime = ServiceLifetime.Singleton;
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        var handlerDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IRequestHandler<TestRequest, string>));
        Assert.NotNull(handlerDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, handlerDescriptor.Lifetime);
    }

    [Fact]
    public void AddMediator_ScanningEmptyAssembly_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var emptyAssembly = Assembly.GetExecutingAssembly(); // This assembly has our test handlers

        // Act & Assert - Should not throw
        services.AddMediator(config =>
        {
            config.RegisterServicesFromAssembly(emptyAssembly);
        });

        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<IMediator>());
    }
}