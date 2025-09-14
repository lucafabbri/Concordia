using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Concordia;
using Concordia.Behaviors;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace Concordia.Core.Tests;

/// <summary>
/// Additional comprehensive tests to achieve closer to 100% coverage
/// </summary>
public class ComprehensiveCoverageTests
{
    /// <summary>
    /// Test request for coverage tests
    /// </summary>
    public class CoverageTestRequest : IRequest<string>
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test command for coverage tests
    /// </summary>
    public class CoverageTestCommand : IRequest
    {
        public string Command { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test notification for coverage tests
    /// </summary>
    public class CoverageTestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handler for coverage test request
    /// </summary>
    public class CoverageTestRequestHandler : IRequestHandler<CoverageTestRequest, string>
    {
        public Task<string> Handle(CoverageTestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Processed: {request.Message}");
        }
    }

    /// <summary>
    /// Handler for coverage test command
    /// </summary>
    public class CoverageTestCommandHandler : IRequestHandler<CoverageTestCommand>
    {
        public Task Handle(CoverageTestCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Multiple notification handlers to test scenario with multiple handlers
    /// </summary>
    public class CoverageTestNotificationHandler1 : INotificationHandler<CoverageTestNotification>
    {
        public static int CallCount { get; set; } = 0;
        
        public Task Handle(CoverageTestNotification notification, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Second notification handler
    /// </summary>
    public class CoverageTestNotificationHandler2 : INotificationHandler<CoverageTestNotification>
    {
        public static int CallCount { get; set; } = 0;
        
        public Task Handle(CoverageTestNotification notification, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Pre-processor that records execution
    /// </summary>
    public class RecordingPreProcessor : IRequestPreProcessor<CoverageTestRequest>
    {
        public static bool WasCalled { get; set; } = false;
        
        public Task Process(CoverageTestRequest request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Post-processor that records execution
    /// </summary>
    public class RecordingPostProcessor : IRequestPostProcessor<CoverageTestRequest, string>
    {
        public static bool WasCalled { get; set; } = false;
        public static string LastResponse { get; set; } = string.Empty;
        
        public Task Process(CoverageTestRequest request, string response, CancellationToken cancellationToken)
        {
            WasCalled = true;
            LastResponse = response;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Pipeline behavior that records execution order
    /// </summary>
    public class OrderTrackingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public static List<string> ExecutionOrder { get; set; } = new();
        
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ExecutionOrder.Add("Before");
            var response = await next(cancellationToken);
            ExecutionOrder.Add("After");
            return response;
        }
    }

    /// <summary>
    /// Tests that Mediator properly handles the full pipeline with pre-processors, behaviors, handlers, and post-processors
    /// </summary>
    [Fact]
    public async Task Send_WithFullPipeline_ShouldExecuteAllComponentsInCorrectOrder()
    {
        // Reset static state
        RecordingPreProcessor.WasCalled = false;
        RecordingPostProcessor.WasCalled = false;
        RecordingPostProcessor.LastResponse = string.Empty;
        OrderTrackingBehavior<CoverageTestRequest, string>.ExecutionOrder.Clear();
        
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<CoverageTestRequest, string>, CoverageTestRequestHandler>();
        services.AddTransient<IRequestPreProcessor<CoverageTestRequest>, RecordingPreProcessor>();
        services.AddTransient<IRequestPostProcessor<CoverageTestRequest, string>, RecordingPostProcessor>();
        services.AddTransient<IPipelineBehavior<CoverageTestRequest, string>, OrderTrackingBehavior<CoverageTestRequest, string>>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new CoverageTestRequest { Message = "Full Pipeline Test" };

        // Act
        var response = await mediator.Send(request);

        // Assert
        Assert.Equal("Processed: Full Pipeline Test", response);
        Assert.True(RecordingPreProcessor.WasCalled);
        Assert.True(RecordingPostProcessor.WasCalled);
        Assert.Equal("Processed: Full Pipeline Test", RecordingPostProcessor.LastResponse);
        Assert.Equal(new[] { "Before", "After" }, OrderTrackingBehavior<CoverageTestRequest, string>.ExecutionOrder);
    }

    /// <summary>
    /// Tests that Mediator handles commands (IRequest without response) through the full pipeline
    /// </summary>
    [Fact]
    public async Task Send_WithCommandPipeline_ShouldExecuteCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<CoverageTestCommand>, CoverageTestCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new CoverageTestCommand { Command = "Test Command" };

        // Act & Assert - Should complete without throwing
        await mediator.Send(command);
    }

    /// <summary>
    /// Tests Publish with multiple notification handlers
    /// </summary>
    [Fact]
    public async Task Publish_WithMultipleHandlers_ShouldCallAllHandlers()
    {
        // Reset static state
        CoverageTestNotificationHandler1.CallCount = 0;
        CoverageTestNotificationHandler2.CallCount = 0;
        
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<INotificationHandler<CoverageTestNotification>, CoverageTestNotificationHandler1>();
        services.AddTransient<INotificationHandler<CoverageTestNotification>, CoverageTestNotificationHandler2>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new CoverageTestNotification { Message = "Multi Handler Test" };

        // Act
        await mediator.Publish(notification);

        // Assert
        Assert.Equal(1, CoverageTestNotificationHandler1.CallCount);
        Assert.Equal(1, CoverageTestNotificationHandler2.CallCount);
    }

    /// <summary>
    /// Tests that Mediator handles exceptions from reflection-based calls correctly
    /// </summary>
    [Fact]
    public async Task Send_WithBadHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        // Register a type that doesn't implement the expected Handle method properly
        services.AddTransient<IRequestHandler<CoverageTestRequest, string>>(sp => new BadRequestHandler());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new CoverageTestRequest { Message = "Bad Handler Test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request));
    }

    /// <summary>
    /// Tests notification handlers with reflection exceptions
    /// </summary>
    [Fact]
    public async Task Publish_WithBadNotificationHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<INotificationHandler<CoverageTestNotification>>(sp => new BadNotificationHandler());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new CoverageTestNotification { Message = "Bad Handler Test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Publish(notification));
    }

    /// <summary>
    /// Tests pre-processor with reflection exceptions
    /// </summary>
    [Fact]
    public async Task Send_WithBadPreProcessor_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<CoverageTestRequest, string>, CoverageTestRequestHandler>();
        services.AddTransient<IRequestPreProcessor<CoverageTestRequest>>(sp => new BadPreProcessor());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new CoverageTestRequest { Message = "Bad Pre-processor Test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request));
    }

    /// <summary>
    /// Tests post-processor with reflection exceptions
    /// </summary>
    [Fact]
    public async Task Send_WithBadPostProcessor_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<CoverageTestRequest, string>, CoverageTestRequestHandler>();
        services.AddTransient<IRequestPostProcessor<CoverageTestRequest, string>>(sp => new BadPostProcessor());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new CoverageTestRequest { Message = "Bad Post-processor Test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request));
    }

    /// <summary>
    /// Tests pipeline behavior with reflection exceptions
    /// </summary>
    [Fact]
    public async Task Send_WithBadPipelineBehavior_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<CoverageTestRequest, string>, CoverageTestRequestHandler>();
        services.AddTransient<IPipelineBehavior<CoverageTestRequest, string>>(sp => new BadPipelineBehavior());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new CoverageTestRequest { Message = "Bad Behavior Test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request));
    }

    /// <summary>
    /// Bad handler that doesn't implement the expected Handle method
    /// </summary>
    public class BadRequestHandler : IRequestHandler<CoverageTestRequest, string>
    {
        // This will cause reflection to fail finding the Handle method
        public Task<string> BadMethodName(CoverageTestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult("Bad");
        }

        // The interface requires Handle method, but we'll make this private
        Task<string> IRequestHandler<CoverageTestRequest, string>.Handle(CoverageTestRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Bad notification handler
    /// </summary>
    public class BadNotificationHandler : INotificationHandler<CoverageTestNotification>
    {
        Task INotificationHandler<CoverageTestNotification>.Handle(CoverageTestNotification notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Bad pre-processor
    /// </summary>
    public class BadPreProcessor : IRequestPreProcessor<CoverageTestRequest>
    {
        Task IRequestPreProcessor<CoverageTestRequest>.Process(CoverageTestRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Bad post-processor
    /// </summary>
    public class BadPostProcessor : IRequestPostProcessor<CoverageTestRequest, string>
    {
        Task IRequestPostProcessor<CoverageTestRequest, string>.Process(CoverageTestRequest request, string response, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Bad pipeline behavior
    /// </summary>
    public class BadPipelineBehavior : IPipelineBehavior<CoverageTestRequest, string>
    {
        Task<string> IPipelineBehavior<CoverageTestRequest, string>.Handle(CoverageTestRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}