using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Concordia;
using System;

namespace Concordia.Core.Tests;

/// <summary>
/// Tests for ConcordiaCoreServiceCollectionExtensions
/// </summary>
public class ConcordiaCoreServiceCollectionExtensionsTests
{
    /// <summary>
    /// Tests that AddConcordiaCoreServices registers all required services
    /// </summary>
    [Fact]
    public void AddConcordiaCoreServices_ShouldRegisterAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddConcordiaCoreServices();

        // Assert
        Assert.Same(services, result); // Should return the same instance for fluent interface

        var serviceProvider = services.BuildServiceProvider();
        
        // Verify IMediator is registered
        var mediator = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);
        Assert.IsType<Mediator>(mediator);
        
        // Verify ISender is registered
        var sender = serviceProvider.GetService<ISender>();
        Assert.NotNull(sender);
        Assert.IsType<Mediator>(sender);
        
        // Verify INotificationPublisher is registered with default implementation
        var notificationPublisher = serviceProvider.GetService<INotificationPublisher>();
        Assert.NotNull(notificationPublisher);
        Assert.IsType<ForeachAwaitPublisher>(notificationPublisher);
    }

    /// <summary>
    /// Tests that AddConcordiaCoreServices throws ArgumentNullException when services is null
    /// </summary>
    [Fact]
    public void AddConcordiaCoreServices_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddConcordiaCoreServices());
        Assert.Equal("services", exception.ParamName);
    }

    /// <summary>
    /// Tests that AddConcordiaCoreServices with custom publisher registers all required services
    /// </summary>
    [Fact]
    public void AddConcordiaCoreServices_WithCustomPublisher_ShouldRegisterAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddConcordiaCoreServices<TaskWhenAllPublisher>();

        // Assert
        Assert.Same(services, result); // Should return the same instance for fluent interface

        var serviceProvider = services.BuildServiceProvider();
        
        // Verify IMediator is registered
        var mediator = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);
        Assert.IsType<Mediator>(mediator);
        
        // Verify ISender is registered
        var sender = serviceProvider.GetService<ISender>();
        Assert.NotNull(sender);
        Assert.IsType<Mediator>(sender);
        
        // Verify INotificationPublisher is registered with custom implementation
        var notificationPublisher = serviceProvider.GetService<INotificationPublisher>();
        Assert.NotNull(notificationPublisher);
        Assert.IsType<TaskWhenAllPublisher>(notificationPublisher);
    }

    /// <summary>
    /// Tests that AddConcordiaCoreServices with custom publisher throws ArgumentNullException when services is null
    /// </summary>
    [Fact]
    public void AddConcordiaCoreServices_WithCustomPublisher_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddConcordiaCoreServices<TaskWhenAllPublisher>());
        Assert.Equal("services", exception.ParamName);
    }

    /// <summary>
    /// Tests that multiple IMediator instances are created (transient registration)
    /// </summary>
    [Fact]
    public void AddConcordiaCoreServices_ShouldRegisterMediatorAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var mediator1 = serviceProvider.GetService<IMediator>();
        var mediator2 = serviceProvider.GetService<IMediator>();

        // Assert
        Assert.NotNull(mediator1);
        Assert.NotNull(mediator2);
        Assert.NotSame(mediator1, mediator2); // Should be different instances (transient)
    }

    /// <summary>
    /// Tests that the same INotificationPublisher instance is used (singleton registration)
    /// </summary>
    [Fact]
    public void AddConcordiaCoreServices_ShouldRegisterNotificationPublisherAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var publisher1 = serviceProvider.GetService<INotificationPublisher>();
        var publisher2 = serviceProvider.GetService<INotificationPublisher>();

        // Assert
        Assert.NotNull(publisher1);
        Assert.NotNull(publisher2);
        Assert.Same(publisher1, publisher2); // Should be the same instance (singleton)
    }

    /// <summary>
    /// Custom implementation of INotificationPublisher for testing
    /// </summary>
    public class CustomNotificationPublisher : INotificationPublisher
    {
        public Task Publish(IEnumerable<Func<INotification, CancellationToken, Task>> handlerCalls, INotification notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Tests that custom notification publisher types work correctly
    /// </summary>
    [Fact]
    public void AddConcordiaCoreServices_WithCustomNotificationPublisher_ShouldRegisterCorrectType()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddConcordiaCoreServices<CustomNotificationPublisher>();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var notificationPublisher = serviceProvider.GetService<INotificationPublisher>();
        Assert.NotNull(notificationPublisher);
        Assert.IsType<CustomNotificationPublisher>(notificationPublisher);
    }
}