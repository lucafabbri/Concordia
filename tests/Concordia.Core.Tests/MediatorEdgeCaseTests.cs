using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Concordia;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace Concordia.Core.Tests;

/// <summary>
/// Additional edge case tests for Mediator to improve coverage
/// </summary>
public class MediatorEdgeCaseTests
{
    /// <summary>
    /// Test request for edge case tests
    /// </summary>
    public class EdgeCaseRequest : IRequest<string>
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test command for edge case tests
    /// </summary>
    public class EdgeCaseCommand : IRequest
    {
        public string Command { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test notification for edge case tests
    /// </summary>
    public class EdgeCaseNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request handler for edge case tests
    /// </summary>
    public class EdgeCaseRequestHandler : IRequestHandler<EdgeCaseRequest, string>
    {
        public Task<string> Handle(EdgeCaseRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {request.Message}");
        }
    }

    /// <summary>
    /// Command handler for edge case tests
    /// </summary>
    public class EdgeCaseCommandHandler : IRequestHandler<EdgeCaseCommand>
    {
        public Task Handle(EdgeCaseCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Notification handler for edge case tests
    /// </summary>
    public class EdgeCaseNotificationHandler : INotificationHandler<EdgeCaseNotification>
    {
        public Task Handle(EdgeCaseNotification notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Pre-processor that modifies the request
    /// </summary>
    public class EdgeCasePreProcessor : IRequestPreProcessor<EdgeCaseRequest>
    {
        public Task Process(EdgeCaseRequest request, CancellationToken cancellationToken)
        {
            request.Message = $"Pre-processed: {request.Message}";
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Post-processor that verifies the response
    /// </summary>
    public class EdgeCasePostProcessor : IRequestPostProcessor<EdgeCaseRequest, string>
    {
        public Task Process(EdgeCaseRequest request, string response, CancellationToken cancellationToken)
        {
            // Can perform validation or logging here
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Pipeline behavior that throws on specific condition
    /// </summary>
    internal class ConditionalFailureBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is EdgeCaseRequest edgeRequest && edgeRequest.Message == "FAIL")
            {
                throw new InvalidOperationException("Conditional failure triggered");
            }
            return await next(cancellationToken);
        }
    }

    /// <summary>
    /// Invalid handler without proper Handle method (for testing reflection errors)
    /// </summary>
    public class InvalidHandler
    {
        // Missing Handle method
    }

    /// <summary>
    /// Tests that Mediator handles requests with pre and post processors correctly
    /// </summary>
    [Fact]
    public async Task Send_WithPreAndPostProcessors_ShouldExecuteInCorrectOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<EdgeCaseRequest, string>, EdgeCaseRequestHandler>();
        services.AddTransient<IRequestPreProcessor<EdgeCaseRequest>, EdgeCasePreProcessor>();
        services.AddTransient<IRequestPostProcessor<EdgeCaseRequest, string>, EdgeCasePostProcessor>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new EdgeCaseRequest { Message = "Original" };

        // Act
        var response = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Pre-processed: Original", response);
    }

    /// <summary>
    /// Tests that Mediator handles pipeline behaviors correctly
    /// </summary>
    [Fact]
    public async Task Send_WithPipelineBehavior_ShouldExecuteBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<EdgeCaseRequest, string>, EdgeCaseRequestHandler>();
        services.AddTransient<IPipelineBehavior<EdgeCaseRequest, string>, ConditionalFailureBehavior<EdgeCaseRequest, string>>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new EdgeCaseRequest { Message = "Normal" };

        // Act
        var response = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Normal", response);
    }

    /// <summary>
    /// Tests that Mediator pipeline behavior can throw exceptions
    /// </summary>
    [Fact]
    public async Task Send_WithFailingPipelineBehavior_ShouldPropagateException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<EdgeCaseRequest, string>, EdgeCaseRequestHandler>();
        services.AddTransient<IPipelineBehavior<EdgeCaseRequest, string>, ConditionalFailureBehavior<EdgeCaseRequest, string>>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new EdgeCaseRequest { Message = "FAIL" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.Send(request));
        
        Assert.Equal("Conditional failure triggered", exception.Message);
    }

    /// <summary>
    /// Tests that Mediator handles Send for object requests correctly
    /// </summary>
    [Fact]
    public async Task Send_WithObjectRequest_ShouldReturnCorrectResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<EdgeCaseRequest, string>, EdgeCaseRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        object request = new EdgeCaseRequest { Message = "Object Test" };

        // Act
        var response = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Object Test", response);
    }

    /// <summary>
    /// Tests that Mediator handles Send for object commands correctly
    /// </summary>
    [Fact]
    public async Task Send_WithObjectCommand_ShouldExecuteCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<EdgeCaseCommand>, EdgeCaseCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        object command = new EdgeCaseCommand { Command = "Object Command Test" };

        // Act & Assert - Should not throw
        var response = await mediator.Send(command);
        Assert.Null(response); // Commands typically return null
    }

    /// <summary>
    /// Tests that Mediator throws when no handler is found for object request
    /// </summary>
    [Fact]
    public async Task Send_WithObjectRequestNoHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        object request = new EdgeCaseRequest { Message = "No Handler" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.Send(request));
        
        Assert.Contains("No handler found for object request of type", exception.Message);
    }

    /// <summary>
    /// Tests that Mediator throws when INotificationPublisher is not registered
    /// </summary>
    [Fact]
    public async Task Publish_WithoutNotificationPublisher_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IMediator, Mediator>();
        services.AddTransient<ISender, Mediator>();
        // Note: Not registering INotificationPublisher

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new EdgeCaseNotification { Message = "Test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.Publish(notification));
        
        Assert.Contains("No INotificationPublisher is registered", exception.Message);
    }

    /// <summary>
    /// Tests that Mediator handles notifications with no handlers gracefully
    /// </summary>
    [Fact]
    public async Task Publish_WithNoHandlers_ShouldCompleteSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        // Note: Not registering any notification handlers

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new EdgeCaseNotification { Message = "No Handlers" };

        // Act & Assert - Should not throw
        await mediator.Publish(notification);
    }

    /// <summary>
    /// Tests that Mediator handles notifications with multiple handlers
    /// </summary>
    [Fact]
    public async Task Publish_WithMultipleHandlers_ShouldCallAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<INotificationHandler<EdgeCaseNotification>, EdgeCaseNotificationHandler>();
        services.AddTransient<INotificationHandler<EdgeCaseNotification>, SecondEdgeCaseNotificationHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new EdgeCaseNotification { Message = "Multiple Handlers" };

        // Act & Assert - Should not throw
        await mediator.Publish(notification);
    }

    /// <summary>
    /// Tests that Mediator constructor throws ArgumentNullException with null serviceProvider
    /// </summary>
    [Fact]
    public void Mediator_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Mediator(null!));
        Assert.Equal("serviceProvider", exception.ParamName);
    }

    /// <summary>
    /// Tests cancellation token propagation through the pipeline
    /// </summary>
    [Fact]
    public async Task Send_WithCancellationToken_ShouldPropagateCancellation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<EdgeCaseRequest, string>, CancellationAwareHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new EdgeCaseRequest { Message = "Cancellation Test" };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Expect TargetInvocationException wrapping OperationCanceledException
        var exception = await Assert.ThrowsAsync<TargetInvocationException>(() =>
            mediator.Send(request, cts.Token));
        
        // Verify the inner exception is OperationCanceledException
        Assert.IsType<OperationCanceledException>(exception.InnerException);
    }

    /// <summary>
    /// Second notification handler for testing multiple handlers
    /// </summary>
    public class SecondEdgeCaseNotificationHandler : INotificationHandler<EdgeCaseNotification>
    {
        public Task Handle(EdgeCaseNotification notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Handler that respects cancellation tokens
    /// </summary>
    public class CancellationAwareHandler : IRequestHandler<EdgeCaseRequest, string>
    {
        public Task<string> Handle(EdgeCaseRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult($"Handled: {request.Message}");
        }
    }
}