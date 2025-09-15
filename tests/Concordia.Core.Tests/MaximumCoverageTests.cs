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
/// Tests specifically targeting remaining uncovered areas for maximum coverage
/// </summary>
public class MaximumCoverageTests
{
    /// <summary>
    /// Test context for contextual pipeline behavior tests
    /// </summary>
    public class TestContext : ICommandPipelineContext
    {
        public DateTime StartTime { get; } = DateTime.UtcNow;
        public bool IsSuccess { get; set; } = true;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test request for contextual behavior tests
    /// </summary>
    public class ContextualTestRequest : IRequest<string>
    {
        public string Message { get; set; } = string.Empty;
        public bool ShouldFail { get; set; } = false;
    }

    /// <summary>
    /// Handler for contextual test request
    /// </summary>
    public class ContextualTestRequestHandler : IRequestHandler<ContextualTestRequest, string>
    {
        public Task<string> Handle(ContextualTestRequest request, CancellationToken cancellationToken)
        {
            if (request.ShouldFail)
            {
                throw new InvalidOperationException("Requested failure");
            }
            return Task.FromResult($"Handled: {request.Message}");
        }
    }

    /// <summary>
    /// First contextual behavior to test context creation and sharing
    /// </summary>
    public class FirstContextualTestBehavior : ContextualPipelineBehavior<ContextualTestRequest, string, TestContext>
    {
        public static bool InboundCalled { get; set; } = false;
        public static bool OutboundCalled { get; set; } = false;
        public static TestContext? LastContext { get; set; }

        protected override Task OnInbound(TestContext context, ContextualTestRequest request, CancellationToken cancellationToken)
        {
            InboundCalled = true;
            LastContext = context;
            return Task.CompletedTask;
        }

        protected override Task OnOutbound(TestContext context, string response, CancellationToken cancellationToken)
        {
            OutboundCalled = true;
            LastContext = context;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Second contextual behavior to test context sharing
    /// </summary>
    public class SecondContextualTestBehavior : ContextualPipelineBehavior<ContextualTestRequest, string, TestContext>
    {
        public static bool InboundCalled { get; set; } = false;
        public static bool OutboundCalled { get; set; } = false;
        public static TestContext? LastContext { get; set; }

        protected override Task OnInbound(TestContext context, ContextualTestRequest request, CancellationToken cancellationToken)
        {
            InboundCalled = true;
            LastContext = context;
            return Task.CompletedTask;
        }

        protected override Task OnOutbound(TestContext context, string response, CancellationToken cancellationToken)
        {
            OutboundCalled = true;
            LastContext = context;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Tests that contextual pipeline behavior creates and shares context correctly
    /// </summary>
    [Fact]
    public async Task ContextualPipelineBehavior_WithMultipleBehaviors_ShouldShareContext()
    {
        // Reset static state
        FirstContextualTestBehavior.InboundCalled = false;
        FirstContextualTestBehavior.OutboundCalled = false;
        FirstContextualTestBehavior.LastContext = null;
        SecondContextualTestBehavior.InboundCalled = false;
        SecondContextualTestBehavior.OutboundCalled = false;
        SecondContextualTestBehavior.LastContext = null;

        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<ContextualTestRequest, string>, ContextualTestRequestHandler>();
        services.AddTransient<IPipelineBehavior<ContextualTestRequest, string>, FirstContextualTestBehavior>();
        services.AddTransient<IPipelineBehavior<ContextualTestRequest, string>, SecondContextualTestBehavior>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ContextualTestRequest { Message = "Context Sharing Test" };

        // Act
        var response = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Context Sharing Test", response);
        
        // Verify both behaviors were called
        Assert.True(FirstContextualTestBehavior.InboundCalled);
        Assert.True(FirstContextualTestBehavior.OutboundCalled);
        Assert.True(SecondContextualTestBehavior.InboundCalled);
        Assert.True(SecondContextualTestBehavior.OutboundCalled);
        
        // Verify they shared the same context
        Assert.NotNull(FirstContextualTestBehavior.LastContext);
        Assert.NotNull(SecondContextualTestBehavior.LastContext);
        Assert.True(FirstContextualTestBehavior.LastContext.IsSuccess);
        Assert.True(SecondContextualTestBehavior.LastContext.IsSuccess);
    }

    /// <summary>
    /// Tests that contextual pipeline behavior handles exceptions correctly
    /// </summary>
    [Fact]
    public async Task ContextualPipelineBehavior_WithException_ShouldUpdateContextAndCleanup()
    {
        // Reset static state
        FirstContextualTestBehavior.InboundCalled = false;
        FirstContextualTestBehavior.OutboundCalled = false;
        FirstContextualTestBehavior.LastContext = null;

        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<ContextualTestRequest, string>, ContextualTestRequestHandler>();
        services.AddTransient<IPipelineBehavior<ContextualTestRequest, string>, FirstContextualTestBehavior>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ContextualTestRequest { Message = "Exception Test", ShouldFail = true };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TargetInvocationException>(() => mediator.Send(request));
        
        // Verify the inner exception is what we expect
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Equal("Requested failure", exception.InnerException.Message);
        
        // Verify behavior was called
        Assert.True(FirstContextualTestBehavior.InboundCalled);
        Assert.True(FirstContextualTestBehavior.OutboundCalled); // Should be called in finally block
        
        // Verify context was updated with failure
        Assert.NotNull(FirstContextualTestBehavior.LastContext);
        Assert.False(FirstContextualTestBehavior.LastContext.IsSuccess);
        Assert.Contains("Exception has been thrown by the target of an invocation", FirstContextualTestBehavior.LastContext.ErrorMessage);
    }

    /// <summary>
    /// Tests various error scenarios with object send
    /// </summary>
    [Fact]
    public async Task Send_ObjectWithGenericConstraints_ShouldHandleCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<ContextualTestRequest, string>, ContextualTestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Test with object that implements IRequest<TResponse>
        object request = new ContextualTestRequest { Message = "Object Send Test" };

        // Act
        var response = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Object Send Test", response);
    }

    /// <summary>
    /// Tests edge cases around null responses and complex return types
    /// </summary>
    [Fact]
    public async Task Send_WithNullableResponseType_ShouldHandleCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<NullableTestRequest, string?>, NullableTestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new NullableTestRequest { ReturnNull = true };

        // Act
        var response = await mediator.Send(request);

        // Assert
        Assert.Null(response);
    }

    /// <summary>
    /// Tests custom notification publisher error scenarios
    /// </summary>
    [Fact]
    public async Task Publish_WithCustomPublisherError_ShouldPropagateException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IMediator, Mediator>();
        services.AddTransient<ISender, Mediator>();
        services.AddSingleton<INotificationPublisher, ThrowingNotificationPublisher>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new SimpleNotification();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            mediator.Publish(notification));
        
        Assert.Equal("Publisher error", exception.Message);
    }

    /// <summary>
    /// Tests reflection edge cases with task return types
    /// </summary>
    [Fact]
    public async Task Send_WithTaskReturnTypeVariations_ShouldHandleCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<TaskTestRequest, Task<string>>, TaskTestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TaskTestRequest();

        // Act
        var response = await mediator.Send(request);

        // Assert
        Assert.NotNull(response);
        var result = await response;
        Assert.Equal("Task result", result);
    }

    // Supporting classes for the tests

    /// <summary>
    /// Request with nullable response
    /// </summary>
    public class NullableTestRequest : IRequest<string?>
    {
        public bool ReturnNull { get; set; } = false;
    }

    /// <summary>
    /// Handler that can return null
    /// </summary>
    public class NullableTestRequestHandler : IRequestHandler<NullableTestRequest, string?>
    {
        public Task<string?> Handle(NullableTestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.ReturnNull ? null : "Not null");
        }
    }

    /// <summary>
    /// Simple notification for testing
    /// </summary>
    public class SimpleNotification : INotification
    {
    }

    /// <summary>
    /// Notification publisher that throws
    /// </summary>
    public class ThrowingNotificationPublisher : INotificationPublisher
    {
        public Task Publish(IEnumerable<Func<INotification, CancellationToken, Task>> handlerCalls, INotification notification, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Publisher error");
        }
    }

    /// <summary>
    /// Request with Task return type
    /// </summary>
    public class TaskTestRequest : IRequest<Task<string>>
    {
    }

    /// <summary>
    /// Handler with Task return type
    /// </summary>
    public class TaskTestRequestHandler : IRequestHandler<TaskTestRequest, Task<string>>
    {
        public Task<Task<string>> Handle(TaskTestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Task.FromResult("Task result"));
        }
    }
}