using Concordia;
using Concordia.MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;

namespace Concordia.Core.Tests;

/// <summary>
/// Simplified tests for ConcordiaMediatRServiceConfiguration
/// focusing on the publicly accessible functionality.
/// </summary>
public class ConcordiaMediatRServiceConfigurationTests
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

    public class InvalidBehavior
    {
        // Does not implement IPipelineBehavior
    }

    public class InvalidStreamBehavior
    {
        // Does not implement IStreamPipelineBehavior
    }

    public class InvalidPreProcessor
    {
        // Does not implement IRequestPreProcessor
    }

    public class InvalidPostProcessor
    {
        // Does not implement IRequestPostProcessor
    }

    public class InvalidRequestHandler
    {
        // Does not implement IRequestHandler
    }

    public class InvalidNotificationHandler
    {
        // Does not implement INotificationHandler
    }

    #endregion

    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        // Act
        var config = new ConcordiaMediatRServiceConfiguration();

        // Assert
        Assert.Equal(typeof(Mediator), config.MediatorImplementationType);
        Assert.IsType<ForeachAwaitPublisher>(config.NotificationPublisher);
        Assert.Null(config.NotificationPublisherType);
        Assert.Equal(ServiceLifetime.Transient, config.Lifetime);
        Assert.False(config.DisableAssemblyScanning);
        Assert.Empty(config.BehaviorsToRegister);
        Assert.Empty(config.StreamBehaviorsToRegister);
        Assert.Empty(config.RequestPreProcessorsToRegister);
        Assert.Empty(config.RequestPostProcessorsToRegister);
        Assert.Empty(config.RequestHandlersToRegister);
        Assert.Empty(config.NotificationHandlersToRegister);
    }

    #region Assembly Registration Tests

    [Fact]
    public void RegisterServicesFromAssemblyContaining_Generic_ReturnsConfiguration()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act
        var result = config.RegisterServicesFromAssemblyContaining<TestRequestHandler>();

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void RegisterServicesFromAssemblyContaining_Type_ReturnsConfiguration()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act
        var result = config.RegisterServicesFromAssemblyContaining(typeof(TestRequestHandler));

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void RegisterServicesFromAssembly_ReturnsConfiguration()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var result = config.RegisterServicesFromAssembly(assembly);

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void RegisterServicesFromAssemblies_ReturnsConfiguration()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();
        var assembly1 = Assembly.GetExecutingAssembly();
        var assembly2 = typeof(Mediator).Assembly;

        // Act
        var result = config.RegisterServicesFromAssemblies(assembly1, assembly2);

        // Assert
        Assert.Same(config, result);
    }

    #endregion

    #region Manual Registration Tests

    [Fact]
    public void AddBehavior_ServiceAndImplementationType_RegistersBehavior()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();
        var serviceType = typeof(IPipelineBehavior<TestRequest, string>);
        var implementationType = typeof(object); // Dummy type for testing

        // Act
        var result = config.AddBehavior(serviceType, implementationType);

        // Assert
        Assert.Same(config, result);
        Assert.Single(config.BehaviorsToRegister);
        var descriptor = config.BehaviorsToRegister[0];
        Assert.Equal(serviceType, descriptor.ServiceType);
        Assert.Equal(implementationType, descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void AddBehavior_InvalidImplementationType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddBehavior(typeof(InvalidBehavior)));
        
        Assert.Contains("must implement", exception.Message);
        Assert.Contains(typeof(IPipelineBehavior<,>).FullName, exception.Message);
    }

    [Fact]
    public void AddOpenBehavior_NonGenericType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenBehavior(typeof(TestRequestHandler)));
        
        Assert.Contains("must be an open generic type definition", exception.Message);
    }

    [Fact]
    public void AddOpenBehavior_InvalidOpenType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenBehavior(typeof(List<>)));
        
        Assert.Contains("must implement", exception.Message);
        Assert.Contains(typeof(IPipelineBehavior<,>).FullName, exception.Message);
    }

    #endregion

    #region Stream Behavior Registration Tests

    [Fact]
    public void AddStreamBehavior_ServiceAndImplementationType_RegistersStreamBehavior()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();
        var serviceType = typeof(IStreamPipelineBehavior<IRequest<IAsyncEnumerable<string>>, string>);
        var implementationType = typeof(object); // Dummy type for testing

        // Act
        var result = config.AddStreamBehavior(serviceType, implementationType);

        // Assert
        Assert.Same(config, result);
        Assert.Single(config.StreamBehaviorsToRegister);
        var descriptor = config.StreamBehaviorsToRegister[0];
        Assert.Equal(serviceType, descriptor.ServiceType);
        Assert.Equal(implementationType, descriptor.ImplementationType);
    }

    [Fact]
    public void AddStreamBehavior_InvalidImplementationType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddStreamBehavior(typeof(InvalidStreamBehavior)));
        
        Assert.Contains("must implement", exception.Message);
        Assert.Contains(typeof(IStreamPipelineBehavior<,>).FullName, exception.Message);
    }

    [Fact]
    public void AddOpenStreamBehavior_NonGenericType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenStreamBehavior(typeof(TestRequestHandler)));
        
        Assert.Contains("must be an open generic type definition", exception.Message);
    }

    [Fact]
    public void AddOpenStreamBehavior_InvalidOpenType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenStreamBehavior(typeof(List<>)));
        
        Assert.Contains("must implement", exception.Message);
        Assert.Contains(typeof(IStreamPipelineBehavior<,>).FullName, exception.Message);
    }

    #endregion

    #region Pre-Processor Registration Tests

    [Fact]
    public void AddRequestPreProcessor_ServiceAndImplementationType_RegistersPreProcessor()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();
        var serviceType = typeof(IRequestPreProcessor<TestRequest>);
        var implementationType = typeof(object); // Dummy type for testing

        // Act
        var result = config.AddRequestPreProcessor(serviceType, implementationType);

        // Assert
        Assert.Same(config, result);
        Assert.Single(config.RequestPreProcessorsToRegister);
        var descriptor = config.RequestPreProcessorsToRegister[0];
        Assert.Equal(serviceType, descriptor.ServiceType);
        Assert.Equal(implementationType, descriptor.ImplementationType);
    }

    [Fact]
    public void AddRequestPreProcessor_InvalidImplementationType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddRequestPreProcessor(typeof(InvalidPreProcessor)));
        
        Assert.Contains("must implement", exception.Message);
        Assert.Contains(typeof(IRequestPreProcessor<>).FullName, exception.Message);
    }

    [Fact]
    public void AddOpenRequestPreProcessor_NonGenericType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenRequestPreProcessor(typeof(TestRequestHandler)));
        
        Assert.Contains("must be an open generic type definition", exception.Message);
    }

    [Fact]
    public void AddOpenRequestPreProcessor_InvalidOpenType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenRequestPreProcessor(typeof(List<>)));
        
        Assert.Contains("must implement", exception.Message);
        Assert.Contains(typeof(IRequestPreProcessor<>).FullName, exception.Message);
    }

    #endregion

    #region Post-Processor Registration Tests

    [Fact]
    public void AddRequestPostProcessor_ServiceAndImplementationType_RegistersPostProcessor()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();
        var serviceType = typeof(IRequestPostProcessor<TestRequest, string>);
        var implementationType = typeof(object); // Dummy type for testing

        // Act
        var result = config.AddRequestPostProcessor(serviceType, implementationType);

        // Assert
        Assert.Same(config, result);
        Assert.Single(config.RequestPostProcessorsToRegister);
        var descriptor = config.RequestPostProcessorsToRegister[0];
        Assert.Equal(serviceType, descriptor.ServiceType);
        Assert.Equal(implementationType, descriptor.ImplementationType);
    }

    [Fact]
    public void AddRequestPostProcessor_InvalidImplementationType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddRequestPostProcessor(typeof(InvalidPostProcessor)));
        
        Assert.Contains("must implement", exception.Message);
        Assert.Contains(typeof(IRequestPostProcessor<,>).FullName, exception.Message);
    }

    [Fact]
    public void AddOpenRequestPostProcessor_NonGenericType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenRequestPostProcessor(typeof(TestRequestHandler)));
        
        Assert.Contains("must be an open generic type definition", exception.Message);
    }

    [Fact]
    public void AddOpenRequestPostProcessor_InvalidOpenType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenRequestPostProcessor(typeof(List<>)));
        
        Assert.Contains("must implement", exception.Message);
        Assert.Contains(typeof(IRequestPostProcessor<,>).FullName, exception.Message);
    }

    #endregion

    #region Request Handler Registration Tests

    [Fact]
    public void AddRequestHandler_ServiceAndImplementationType_RegistersHandler()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();
        var serviceType = typeof(IRequestHandler<TestRequest, string>);
        var implementationType = typeof(TestRequestHandler);

        // Act
        var result = config.AddRequestHandler(serviceType, implementationType);

        // Assert
        Assert.Same(config, result);
        Assert.Single(config.RequestHandlersToRegister);
        var descriptor = config.RequestHandlersToRegister[0];
        Assert.Equal(serviceType, descriptor.ServiceType);
        Assert.Equal(implementationType, descriptor.ImplementationType);
    }

    [Fact]
    public void AddRequestHandler_InvalidImplementationType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddRequestHandler(typeof(InvalidRequestHandler)));
        
        Assert.Contains("must implement", exception.Message);
        Assert.Contains(typeof(IRequestHandler<,>).FullName, exception.Message);
        Assert.Contains(typeof(IRequestHandler<>).FullName, exception.Message);
    }

    [Fact]
    public void AddOpenRequestHandler_NonGenericType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenRequestHandler(typeof(TestRequestHandler)));
        
        Assert.Contains("must be an open generic type definition", exception.Message);
    }

    [Fact]
    public void AddOpenRequestHandler_InvalidOpenType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenRequestHandler(typeof(List<>)));
        
        Assert.Contains("must implement", exception.Message);
    }

    #endregion

    #region Notification Handler Registration Tests

    [Fact]
    public void AddNotificationHandler_ServiceAndImplementationType_RegistersHandler()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();
        var serviceType = typeof(INotificationHandler<TestNotification>);
        var implementationType = typeof(TestNotificationHandler);

        // Act
        var result = config.AddNotificationHandler(serviceType, implementationType);

        // Assert
        Assert.Same(config, result);
        Assert.Single(config.NotificationHandlersToRegister);
        var descriptor = config.NotificationHandlersToRegister[0];
        Assert.Equal(serviceType, descriptor.ServiceType);
        Assert.Equal(implementationType, descriptor.ImplementationType);
    }

    [Fact]
    public void AddNotificationHandler_InvalidImplementationType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddNotificationHandler(typeof(InvalidNotificationHandler)));
        
        Assert.Contains("must implement", exception.Message);
        Assert.Contains(typeof(INotificationHandler<>).FullName, exception.Message);
    }

    [Fact]
    public void AddOpenNotificationHandler_NonGenericType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenNotificationHandler(typeof(TestNotificationHandler)));
        
        Assert.Contains("must be an open generic type definition", exception.Message);
    }

    [Fact]
    public void AddOpenNotificationHandler_InvalidOpenType_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            config.AddOpenNotificationHandler(typeof(List<>)));
        
        Assert.Contains("must implement", exception.Message);
        Assert.Contains(typeof(INotificationHandler<>).FullName, exception.Message);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void MediatorImplementationType_CanBeSet()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();
        var customType = typeof(Mediator);

        // Act
        config.MediatorImplementationType = customType;

        // Assert
        Assert.Equal(customType, config.MediatorImplementationType);
    }

    [Fact]
    public void NotificationPublisher_CanBeSet()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();
        var customPublisher = new TaskWhenAllPublisher();

        // Act
        config.NotificationPublisher = customPublisher;

        // Assert
        Assert.Same(customPublisher, config.NotificationPublisher);
    }

    [Fact]
    public void NotificationPublisherType_CanBeSet()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();
        var publisherType = typeof(TaskWhenAllPublisher);

        // Act
        config.NotificationPublisherType = publisherType;

        // Assert
        Assert.Equal(publisherType, config.NotificationPublisherType);
    }

    [Fact]
    public void Lifetime_CanBeSet()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act
        config.Lifetime = ServiceLifetime.Singleton;

        // Assert
        Assert.Equal(ServiceLifetime.Singleton, config.Lifetime);
    }

    [Fact]
    public void DisableAssemblyScanning_CanBeSet()
    {
        // Arrange
        var config = new ConcordiaMediatRServiceConfiguration();

        // Act
        config.DisableAssemblyScanning = true;

        // Assert
        Assert.True(config.DisableAssemblyScanning);
    }

    #endregion
}