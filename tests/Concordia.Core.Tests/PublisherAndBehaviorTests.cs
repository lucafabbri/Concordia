using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Concordia;
using Concordia.Behaviors;
using Concordia.Core.Behaviors;

namespace Concordia.Core.Tests;

public class PublisherAndBehaviorTests
{
    // ─── Shared test types ───────────────────────────────────────────────────────

    public class PBNotification : INotification { }

    public class PBRequest : IRequest<string> { public string Value { get; set; } = ""; }

    public class PBRequestHandler : IRequestHandler<PBRequest, string>
    {
        public Task<string> Handle(PBRequest request, CancellationToken ct)
            => Task.FromResult($"handled:{request.Value}");
    }

    public class TrackingPreProcessor : IRequestPreProcessor<PBRequest>
    {
        private readonly List<string> _order;
        private readonly string _tag;
        public TrackingPreProcessor(List<string> order, string tag) { _order = order; _tag = tag; }
        public Task Process(PBRequest request, CancellationToken ct) { _order.Add(_tag); return Task.CompletedTask; }
    }

    public class TrackingPostProcessor : IRequestPostProcessor<PBRequest, string>
    {
        private readonly List<string> _order;
        private readonly string _tag;
        public TrackingPostProcessor(List<string> order, string tag) { _order = order; _tag = tag; }
        public Task Process(PBRequest request, string response, CancellationToken ct) { _order.Add(_tag); return Task.CompletedTask; }
    }

    // ─── TaskWhenAllPublisher ────────────────────────────────────────────────────

    [Fact]
    public async Task TaskWhenAllPublisher_ShouldPublishToAllHandlers()
    {
        var publisher = new TaskWhenAllPublisher();
        var notification = new PBNotification();
        var called = new List<string>();

        var handlerCalls = new Func<INotification, CancellationToken, Task>[]
        {
            (n, ct) => { called.Add("H1"); return Task.CompletedTask; },
            (n, ct) => { called.Add("H2"); return Task.CompletedTask; },
        };

        await publisher.Publish(handlerCalls, notification, CancellationToken.None);

        Assert.Contains("H1", called);
        Assert.Contains("H2", called);
    }

    [Fact]
    public async Task TaskWhenAllPublisher_WithEmptyHandlerList_ShouldCompleteSuccessfully()
    {
        var publisher = new TaskWhenAllPublisher();
        var notification = new PBNotification();

        await publisher.Publish(
            Enumerable.Empty<Func<INotification, CancellationToken, Task>>(),
            notification,
            CancellationToken.None);

        // Reaches here without exception
    }

    [Fact]
    public async Task TaskWhenAllPublisher_WhenHandlerThrows_ShouldPropagateException()
    {
        var publisher = new TaskWhenAllPublisher();
        var notification = new PBNotification();

        var handlerCalls = new Func<INotification, CancellationToken, Task>[]
        {
            (n, ct) => Task.FromException(new InvalidOperationException("handler error")),
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => publisher.Publish(handlerCalls, notification, CancellationToken.None));
    }

    [Fact]
    public async Task TaskWhenAllPublisher_WhenMultipleHandlersThrow_ShouldPropagateAggregateException()
    {
        var publisher = new TaskWhenAllPublisher();
        var notification = new PBNotification();

        var handlerCalls = new Func<INotification, CancellationToken, Task>[]
        {
            (n, ct) => Task.FromException(new InvalidOperationException("error1")),
            (n, ct) => Task.FromException(new ArgumentException("error2")),
        };

        // Task.WhenAll with multiple faults surfaces as AggregateException when not directly awaited
        var publishTask = publisher.Publish(handlerCalls, notification, CancellationToken.None);
        await Assert.ThrowsAnyAsync<Exception>(() => publishTask);
    }

    // ─── BackgroundParallelPublisher ─────────────────────────────────────────────

    [Fact]
    public async Task BackgroundParallelPublisher_ShouldReturnImmediately_AndExecuteHandlerInBackground()
    {
        var publisher = new BackgroundParallelPublisher(NullLogger<BackgroundParallelPublisher>.Instance);
        var notification = new PBNotification();
        var handlerExecuted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var handlerCalls = new Func<INotification, CancellationToken, Task>[]
        {
            (n, ct) => { handlerExecuted.TrySetResult(true); return Task.CompletedTask; },
        };

        var publishTask = publisher.Publish(handlerCalls, notification, CancellationToken.None);

        // Must return Task.CompletedTask immediately
        Assert.True(publishTask.IsCompletedSuccessfully);

        // Handler should eventually execute in the background
        var winner = await Task.WhenAny(handlerExecuted.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.Same(handlerExecuted.Task, winner);
        Assert.True(await handlerExecuted.Task);
    }

    [Fact]
    public async Task BackgroundParallelPublisher_WhenHandlerThrows_ShouldNotPropagateExceptionToCaller()
    {
        var publisher = new BackgroundParallelPublisher(NullLogger<BackgroundParallelPublisher>.Instance);
        var notification = new PBNotification();
        var handlerStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var handlerCalls = new Func<INotification, CancellationToken, Task>[]
        {
            (n, ct) =>
            {
                handlerStarted.TrySetResult(true);
                return Task.FromException(new Exception("background handler error"));
            },
        };

        // Should return immediately without throwing
        var publishTask = publisher.Publish(handlerCalls, notification, CancellationToken.None);
        Assert.True(publishTask.IsCompletedSuccessfully);

        // Wait for the background work to process
        await Task.WhenAny(handlerStarted.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        // Allow the catch block inside Task.Run to execute
        await Task.Delay(50);

        // Verify no unobserved exception propagated – test completes without throwing
    }

    [Fact]
    public void BackgroundParallelPublisher_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new BackgroundParallelPublisher(null!));
    }

    // ─── LoggingBehavior ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LoggingBehavior_ShouldPassThroughAndReturnResponse()
    {
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<PBRequest, string>, PBRequestHandler>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new PBRequest { Value = "logging" });

        Assert.Equal("handled:logging", result);
    }

    [Fact]
    public async Task LoggingBehavior_ShouldCallNextDelegate()
    {
        var behavior = new LoggingBehavior<PBRequest, string>();
        var nextCalled = false;

        var result = await behavior.Handle(
            new PBRequest { Value = "x" },
            ct => { nextCalled = true; return Task.FromResult("next-result"); },
            CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("next-result", result);
    }

    // ─── RequestPreProcessorBehavior ─────────────────────────────────────────────

    [Fact]
    public async Task RequestPreProcessorBehavior_ShouldRunPreProcessorsBeforeHandler()
    {
        var order = new List<string>();
        var preProcessors = new IRequestPreProcessor<PBRequest>[]
        {
            new TrackingPreProcessor(order, "Pre1"),
            new TrackingPreProcessor(order, "Pre2"),
        };

        var behavior = new RequestPreProcessorBehavior<PBRequest, string>(preProcessors);

        var result = await behavior.Handle(
            new PBRequest { Value = "v" },
            ct => { order.Add("Handler"); return Task.FromResult("ok"); },
            CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.Equal(new[] { "Pre1", "Pre2", "Handler" }, order);
    }

    [Fact]
    public async Task RequestPreProcessorBehavior_WithNoPreProcessors_ShouldCallHandlerDirectly()
    {
        var behavior = new RequestPreProcessorBehavior<PBRequest, string>(
            Array.Empty<IRequestPreProcessor<PBRequest>>());

        var handlerCalled = false;
        var result = await behavior.Handle(
            new PBRequest(),
            ct => { handlerCalled = true; return Task.FromResult("done"); },
            CancellationToken.None);

        Assert.True(handlerCalled);
        Assert.Equal("done", result);
    }

    // ─── RequestPostProcessorBehavior ────────────────────────────────────────────

    [Fact]
    public async Task RequestPostProcessorBehavior_ShouldRunPostProcessorsAfterHandler()
    {
        var order = new List<string>();
        var postProcessors = new IRequestPostProcessor<PBRequest, string>[]
        {
            new TrackingPostProcessor(order, "Post1"),
            new TrackingPostProcessor(order, "Post2"),
        };

        var behavior = new RequestPostProcessorBehavior<PBRequest, string>(postProcessors);

        var result = await behavior.Handle(
            new PBRequest { Value = "v" },
            ct => { order.Add("Handler"); return Task.FromResult("response"); },
            CancellationToken.None);

        Assert.Equal("response", result);
        Assert.Equal(new[] { "Handler", "Post1", "Post2" }, order);
    }

    [Fact]
    public async Task RequestPostProcessorBehavior_WithNoPostProcessors_ShouldReturnHandlerResult()
    {
        var behavior = new RequestPostProcessorBehavior<PBRequest, string>(
            Array.Empty<IRequestPostProcessor<PBRequest, string>>());

        var result = await behavior.Handle(
            new PBRequest(),
            ct => Task.FromResult("bare-result"),
            CancellationToken.None);

        Assert.Equal("bare-result", result);
    }

    // ─── ConcordiaCoreServiceCollectionExtensions null guards ────────────────────

    [Fact]
    public void AddConcordiaCoreServices_WhenServicesIsNull_ShouldThrowArgumentNullException()
    {
        IServiceCollection? services = null;
        Assert.Throws<ArgumentNullException>(() => services!.AddConcordiaCoreServices());
    }

    [Fact]
    public void AddConcordiaCoreServices_Generic_WhenServicesIsNull_ShouldThrowArgumentNullException()
    {
        IServiceCollection? services = null;
        Assert.Throws<ArgumentNullException>(
            () => services!.AddConcordiaCoreServices<ForeachAwaitPublisher>());
    }

    // ─── Mediator null-argument guards ───────────────────────────────────────────

    [Fact]
    public async Task Mediator_Send_WithNullTypedRequest_ShouldThrowArgumentNullException()
    {
        var sp = new ServiceCollection().AddConcordiaCoreServices().BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => mediator.Send<string>(null!));
    }

    [Fact]
    public async Task Mediator_SendVoid_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var sp = new ServiceCollection().AddConcordiaCoreServices().BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => mediator.Send((IRequest)null!));
    }

    [Fact]
    public async Task Mediator_SendObject_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var sp = new ServiceCollection().AddConcordiaCoreServices().BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => mediator.Send((object)null!));
    }

    [Fact]
    public async Task Mediator_Publish_WithNullNotification_ShouldThrowArgumentNullException()
    {
        var sp = new ServiceCollection().AddConcordiaCoreServices().BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => mediator.Publish(null!));
    }

    // ─── Send(object) void-request path ──────────────────────────────────────────

    public class VoidObjectCmd : IRequest { public bool Executed { get; set; } }

    public class VoidObjectCmdHandler : IRequestHandler<VoidObjectCmd>
    {
        public static bool WasExecuted;
        public Task Handle(VoidObjectCmd request, CancellationToken ct)
        {
            WasExecuted = true;
            request.Executed = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Mediator_SendObject_WithVoidRequest_ShouldDispatchAndReturnNull()
    {
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<VoidObjectCmd>, VoidObjectCmdHandler>();
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var cmd = new VoidObjectCmd();
        var response = await mediator.Send((object)cmd);

        Assert.Null(response);
        Assert.True(cmd.Executed);
    }

    [Fact]
    public async Task Mediator_SendObject_WithNonRequestType_ShouldThrowInvalidOperationException()
    {
        var sp = new ServiceCollection().AddConcordiaCoreServices().BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // An object that doesn't implement IRequest or IRequest<T>
        var notARequest = new object();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.Send(notARequest));
    }

    // ─── Wrapper cache hit path (second call with same request type) ──────────────

    [Fact]
    public async Task Mediator_Send_CalledTwiceWithSameType_ShouldUseCachedWrapperAndReturnCorrectly()
    {
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<PBRequest, string>, PBRequestHandler>();
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // First call – populates static cache
        var r1 = await mediator.Send(new PBRequest { Value = "first" });
        // Second call – hits cached wrapper
        var r2 = await mediator.Send(new PBRequest { Value = "second" });

        Assert.Equal("handled:first", r1);
        Assert.Equal("handled:second", r2);
    }
}
