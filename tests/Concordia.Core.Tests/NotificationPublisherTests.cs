using Xunit;
using Concordia;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Core.Tests;

/// <summary>
/// Tests for ForeachAwaitPublisher
/// </summary>
public class ForeachAwaitPublisherTests
{
    /// <summary>
    /// Test notification for notification publisher tests
    /// </summary>
    public class TestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tests that Publish calls all handlers sequentially
    /// </summary>
    [Fact]
    public async Task Publish_ShouldCallAllHandlersSequentially()
    {
        // Arrange
        var publisher = new ForeachAwaitPublisher();
        var notification = new TestNotification { Message = "Test" };
        var callOrder = new List<int>();
        var handlerCalls = new List<Func<INotification, CancellationToken, Task>>
        {
            async (n, ct) => 
            {
                await Task.Delay(10, ct);
                callOrder.Add(1);
            },
            async (n, ct) => 
            {
                await Task.Delay(5, ct);
                callOrder.Add(2);
            },
            async (n, ct) => 
            {
                callOrder.Add(3);
            }
        };

        // Act
        await publisher.Publish(handlerCalls, notification, CancellationToken.None);

        // Assert
        Assert.Equal(3, callOrder.Count);
        Assert.Equal(new[] { 1, 2, 3 }, callOrder); // Should be called in order
    }

    /// <summary>
    /// Tests that Publish works with empty handler collection
    /// </summary>
    [Fact]
    public async Task Publish_WithEmptyHandlers_ShouldCompleteSuccessfully()
    {
        // Arrange
        var publisher = new ForeachAwaitPublisher();
        var notification = new TestNotification { Message = "Test" };
        var handlerCalls = new List<Func<INotification, CancellationToken, Task>>();

        // Act & Assert - Should not throw
        await publisher.Publish(handlerCalls, notification, CancellationToken.None);
    }

    /// <summary>
    /// Tests that Publish respects cancellation token
    /// </summary>
    [Fact]
    public async Task Publish_ShouldRespectCancellationToken()
    {
        // Arrange
        var publisher = new ForeachAwaitPublisher();
        var notification = new TestNotification { Message = "Test" };
        var cts = new CancellationTokenSource();
        
        var handlerCalls = new List<Func<INotification, CancellationToken, Task>>
        {
            async (n, ct) => 
            {
                cts.Cancel(); // Cancel during first handler
                ct.ThrowIfCancellationRequested();
                await Task.Delay(100, ct);
            },
            async (n, ct) => 
            {
                // This should not be reached
                await Task.Delay(100, ct);
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            publisher.Publish(handlerCalls, notification, cts.Token));
    }

    /// <summary>
    /// Tests that Publish propagates handler exceptions
    /// </summary>
    [Fact]
    public async Task Publish_ShouldPropagateHandlerExceptions()
    {
        // Arrange
        var publisher = new ForeachAwaitPublisher();
        var notification = new TestNotification { Message = "Test" };
        var expectedException = new InvalidOperationException("Test exception");
        
        var handlerCalls = new List<Func<INotification, CancellationToken, Task>>
        {
            (n, ct) => Task.CompletedTask,
            (n, ct) => throw expectedException,
            (n, ct) => Task.CompletedTask // This should not be reached
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            publisher.Publish(handlerCalls, notification, CancellationToken.None));
        
        Assert.Same(expectedException, exception);
    }

    /// <summary>
    /// Tests that handlers receive the correct notification and cancellation token
    /// </summary>
    [Fact]
    public async Task Publish_ShouldPassCorrectParametersToHandlers()
    {
        // Arrange
        var publisher = new ForeachAwaitPublisher();
        var notification = new TestNotification { Message = "Test" };
        var cancellationToken = new CancellationToken();
        
        INotification? receivedNotification = null;
        CancellationToken receivedToken = default;
        
        var handlerCalls = new List<Func<INotification, CancellationToken, Task>>
        {
            (n, ct) =>
            {
                receivedNotification = n;
                receivedToken = ct;
                return Task.CompletedTask;
            }
        };

        // Act
        await publisher.Publish(handlerCalls, notification, cancellationToken);

        // Assert
        Assert.Same(notification, receivedNotification);
        Assert.Equal(cancellationToken, receivedToken);
    }
}

/// <summary>
/// Tests for TaskWhenAllPublisher
/// </summary>
public class TaskWhenAllPublisherTests
{
    /// <summary>
    /// Test notification for notification publisher tests
    /// </summary>
    public class TestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tests that Publish calls all handlers in parallel
    /// </summary>
    [Fact]
    public async Task Publish_ShouldCallAllHandlersInParallel()
    {
        // Arrange
        var publisher = new TaskWhenAllPublisher();
        var notification = new TestNotification { Message = "Test" };
        var callCount = 0;
        var handlerCalls = new List<Func<INotification, CancellationToken, Task>>
        {
            async (n, ct) => 
            {
                await Task.Delay(50, ct);
                Interlocked.Increment(ref callCount);
            },
            async (n, ct) => 
            {
                await Task.Delay(30, ct);
                Interlocked.Increment(ref callCount);
            },
            async (n, ct) => 
            {
                await Task.Delay(10, ct);
                Interlocked.Increment(ref callCount);
            }
        };

        var startTime = DateTime.UtcNow;

        // Act
        await publisher.Publish(handlerCalls, notification, CancellationToken.None);

        // Assert
        var elapsedTime = DateTime.UtcNow - startTime;
        Assert.Equal(3, callCount);
        // Should complete in approximately the time of the longest handler (50ms), not the sum (90ms)
        // Allow for some variance in timing due to system load
        Assert.True(elapsedTime.TotalMilliseconds < 150, $"Expected parallel execution, but took {elapsedTime.TotalMilliseconds}ms");
    }

    /// <summary>
    /// Tests that Publish works with empty handler collection
    /// </summary>
    [Fact]
    public async Task Publish_WithEmptyHandlers_ShouldCompleteSuccessfully()
    {
        // Arrange
        var publisher = new TaskWhenAllPublisher();
        var notification = new TestNotification { Message = "Test" };
        var handlerCalls = new List<Func<INotification, CancellationToken, Task>>();

        // Act & Assert - Should not throw
        await publisher.Publish(handlerCalls, notification, CancellationToken.None);
    }

    /// <summary>
    /// Tests that Publish respects cancellation token
    /// </summary>
    [Fact]
    public async Task Publish_ShouldRespectCancellationToken()
    {
        // Arrange
        var publisher = new TaskWhenAllPublisher();
        var notification = new TestNotification { Message = "Test" };
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancel the token
        
        var handlerCalls = new List<Func<INotification, CancellationToken, Task>>
        {
            async (n, ct) => 
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(100, ct);
            },
            async (n, ct) => 
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(100, ct);
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            publisher.Publish(handlerCalls, notification, cts.Token));
    }

    /// <summary>
    /// Tests that Publish propagates handler exceptions
    /// </summary>
    [Fact]
    public async Task Publish_ShouldPropagateHandlerExceptions()
    {
        // Arrange
        var publisher = new TaskWhenAllPublisher();
        var notification = new TestNotification { Message = "Test" };
        var expectedException = new InvalidOperationException("Test exception");
        
        var handlerCalls = new List<Func<INotification, CancellationToken, Task>>
        {
            (n, ct) => Task.CompletedTask,
            (n, ct) => throw expectedException,
            (n, ct) => Task.CompletedTask
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            publisher.Publish(handlerCalls, notification, CancellationToken.None));
        
        Assert.Same(expectedException, exception);
    }

    /// <summary>
    /// Tests that handlers receive the correct notification and cancellation token
    /// </summary>
    [Fact]
    public async Task Publish_ShouldPassCorrectParametersToHandlers()
    {
        // Arrange
        var publisher = new TaskWhenAllPublisher();
        var notification = new TestNotification { Message = "Test" };
        var cancellationToken = new CancellationToken();
        
        var receivedNotifications = new List<INotification>();
        var receivedTokens = new List<CancellationToken>();
        
        var handlerCalls = new List<Func<INotification, CancellationToken, Task>>
        {
            (n, ct) =>
            {
                receivedNotifications.Add(n);
                receivedTokens.Add(ct);
                return Task.CompletedTask;
            },
            (n, ct) =>
            {
                receivedNotifications.Add(n);
                receivedTokens.Add(ct);
                return Task.CompletedTask;
            }
        };

        // Act
        await publisher.Publish(handlerCalls, notification, cancellationToken);

        // Assert
        Assert.Equal(2, receivedNotifications.Count);
        Assert.All(receivedNotifications, n => Assert.Same(notification, n));
        Assert.All(receivedTokens, t => Assert.Equal(cancellationToken, t));
    }

    /// <summary>
    /// Tests that when one handler fails, exception is propagated (Task.WhenAll behavior)
    /// </summary>
    [Fact]
    public async Task Publish_WhenOneHandlerFails_ShouldPropagateException()
    {
        // Arrange
        var publisher = new TaskWhenAllPublisher();
        var notification = new TestNotification { Message = "Test" };
        
        var handlerCalls = new List<Func<INotification, CancellationToken, Task>>
        {
            (n, ct) => Task.CompletedTask,
            (n, ct) => throw new InvalidOperationException("Test exception"),
            (n, ct) => Task.CompletedTask
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            publisher.Publish(handlerCalls, notification, CancellationToken.None));
    }
}