using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Synaptrix;
using System.Runtime.CompilerServices;

namespace Synaptrix.Core.Tests;

public class CreateStreamTests
{
    // --- Mock Stream Requests ---

    public class GetNumbersRequest : IStreamRequest<int>
    {
        public int Count { get; set; }
    }

    public class GetNamesRequest : IStreamRequest<string>
    {
        public string Prefix { get; set; } = string.Empty;
    }

    // --- Mock Stream Handlers ---

    public class GetNumbersHandler : IStreamRequestHandler<GetNumbersRequest, int>
    {
        public async IAsyncEnumerable<int> Handle(GetNumbersRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 1; i <= request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return i;
                await Task.Yield();
            }
        }
    }

    public class GetNamesHandler : IStreamRequestHandler<GetNamesRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(GetNamesRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 0; i < 3; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return $"{request.Prefix}_{i}";
                await Task.Yield();
            }
        }
    }

    public class ThrowingStreamHandler : IStreamRequestHandler<GetNumbersRequest, int>
    {
        public async IAsyncEnumerable<int> Handle(GetNumbersRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return 1;
            await Task.Yield();
            throw new InvalidOperationException("Stream handler error");
        }
    }

    // --- Mock Stream Pipeline Behaviors ---

    public class TestStreamLoggingBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
        where TRequest : IStreamRequest<TResponse>
    {
        private readonly List<string> _logs;

        public TestStreamLoggingBehavior(List<string> logs)
        {
            _logs = logs;
        }

        public async IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _logs.Add($"Before stream {typeof(TRequest).Name}");
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                _logs.Add($"Yielding item from {typeof(TRequest).Name}");
                yield return item;
            }
            _logs.Add($"After stream {typeof(TRequest).Name}");
        }
    }

    public class TestStreamValidationBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
        where TRequest : IStreamRequest<TResponse>
    {
        private readonly List<string> _logs;

        public TestStreamValidationBehavior(List<string> logs)
        {
            _logs = logs;
        }

        public async IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _logs.Add($"Validating stream {typeof(TRequest).Name}");
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return item;
            }
            _logs.Add($"Validation complete for stream {typeof(TRequest).Name}");
        }
    }

    public class FilterEvenNumbersBehavior : IStreamPipelineBehavior<GetNumbersRequest, int>
    {
        public async IAsyncEnumerable<int> Handle(GetNumbersRequest request, StreamHandlerDelegate<int> next, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                if (item % 2 == 0)
                    yield return item;
            }
        }
    }

    // --- Tests ---

    [Fact]
    public async Task CreateStream_ShouldReturnStreamFromHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        services.AddTransient<IStreamRequestHandler<GetNumbersRequest, int>, GetNumbersHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new GetNumbersRequest { Count = 5 };

        // Act
        var results = new List<int>();
        await foreach (var item in mediator.CreateStream<int>(request))
        {
            results.Add(item);
        }

        // Assert
        Assert.Equal(5, results.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, results);
    }

    [Fact]
    public async Task CreateStream_ShouldReturnCorrectTypedStream()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        services.AddTransient<IStreamRequestHandler<GetNamesRequest, string>, GetNamesHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new GetNamesRequest { Prefix = "Item" };

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.CreateStream<string>(request))
        {
            results.Add(item);
        }

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("Item_0", results[0]);
        Assert.Equal("Item_1", results[1]);
        Assert.Equal("Item_2", results[2]);
    }

    [Fact]
    public async Task CreateStream_ShouldReturnEmptyStream_WhenHandlerYieldsNothing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        services.AddTransient<IStreamRequestHandler<GetNumbersRequest, int>, GetNumbersHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new GetNumbersRequest { Count = 0 };

        // Act
        var results = new List<int>();
        await foreach (var item in mediator.CreateStream<int>(request))
        {
            results.Add(item);
        }

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void CreateStream_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mediator.CreateStream<int>(null!));
    }

    [Fact]
    public void CreateStream_ShouldThrowInvalidOperationException_WhenNoHandlerRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new GetNumbersRequest { Count = 3 };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => mediator.CreateStream<int>(request));
        Assert.Contains("No stream handler found for request of type GetNumbersRequest", exception.Message);
    }

    [Fact]
    public async Task CreateStream_ShouldPropagateCancellation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        services.AddTransient<IStreamRequestHandler<GetNumbersRequest, int>, GetNumbersHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new GetNumbersRequest { Count = 100 };
        var cts = new CancellationTokenSource();

        // Act
        var results = new List<int>();
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in mediator.CreateStream<int>(request, cts.Token))
            {
                results.Add(item);
                if (results.Count == 3)
                    cts.Cancel();
            }
        });

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(new[] { 1, 2, 3 }, results);
    }

    [Fact]
    public async Task CreateStream_ShouldPropagateHandlerException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        services.AddTransient<IStreamRequestHandler<GetNumbersRequest, int>, ThrowingStreamHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new GetNumbersRequest { Count = 5 };

        // Act & Assert
        var results = new List<int>();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var item in mediator.CreateStream<int>(request))
            {
                results.Add(item);
            }
        });

        Assert.Equal("Stream handler error", exception.Message);
        Assert.Single(results);
        Assert.Equal(1, results[0]);
    }

    // --- Pipeline Behavior Tests ---

    [Fact]
    public async Task CreateStream_ShouldExecuteStreamPipelineBehaviors()
    {
        // Arrange
        var logs = new List<string>();
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        services.AddSingleton(logs);
        services.AddTransient<IStreamRequestHandler<GetNumbersRequest, int>, GetNumbersHandler>();
        services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(TestStreamLoggingBehavior<,>));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new GetNumbersRequest { Count = 3 };

        // Act
        var results = new List<int>();
        await foreach (var item in mediator.CreateStream<int>(request))
        {
            results.Add(item);
        }

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, results);
        Assert.Equal(5, logs.Count);
        Assert.Equal("Before stream GetNumbersRequest", logs[0]);
        Assert.Equal("Yielding item from GetNumbersRequest", logs[1]);
        Assert.Equal("Yielding item from GetNumbersRequest", logs[2]);
        Assert.Equal("Yielding item from GetNumbersRequest", logs[3]);
        Assert.Equal("After stream GetNumbersRequest", logs[4]);
    }

    [Fact]
    public async Task CreateStream_ShouldExecuteMultiplePipelineBehaviors_InCorrectOrder()
    {
        // Arrange
        var logs = new List<string>();
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        services.AddSingleton(logs);
        services.AddTransient<IStreamRequestHandler<GetNumbersRequest, int>, GetNumbersHandler>();
        services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(TestStreamLoggingBehavior<,>));
        services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(TestStreamValidationBehavior<,>));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new GetNumbersRequest { Count = 2 };

        // Act
        var results = new List<int>();
        await foreach (var item in mediator.CreateStream<int>(request))
        {
            results.Add(item);
        }

        // Assert
        Assert.Equal(new[] { 1, 2 }, results);
        // Pipeline wrapping: Logging wraps Validation wraps Handler
        // Execution: Logging.Before -> Validation.Validating -> yield items -> Validation.Complete -> Logging.After
        Assert.Equal("Before stream GetNumbersRequest", logs[0]);
        Assert.Equal("Validating stream GetNumbersRequest", logs[1]);
        Assert.Contains("Yielding item from GetNumbersRequest", logs);
        Assert.Equal("Validation complete for stream GetNumbersRequest", logs[^2]);
        Assert.Equal("After stream GetNumbersRequest", logs[^1]);
    }

    [Fact]
    public async Task CreateStream_ShouldApplyFilteringBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        services.AddTransient<IStreamRequestHandler<GetNumbersRequest, int>, GetNumbersHandler>();
        services.AddTransient<IStreamPipelineBehavior<GetNumbersRequest, int>, FilterEvenNumbersBehavior>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new GetNumbersRequest { Count = 6 };

        // Act
        var results = new List<int>();
        await foreach (var item in mediator.CreateStream<int>(request))
        {
            results.Add(item);
        }

        // Assert
        Assert.Equal(new[] { 2, 4, 6 }, results);
    }

    [Fact]
    public async Task CreateStream_ShouldWorkWithoutPipelineBehaviors_FastPath()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        services.AddTransient<IStreamRequestHandler<GetNumbersRequest, int>, GetNumbersHandler>();
        // No pipeline behaviors registered — exercises the fast path

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new GetNumbersRequest { Count = 4 };

        // Act
        var results = new List<int>();
        await foreach (var item in mediator.CreateStream<int>(request))
        {
            results.Add(item);
        }

        // Assert
        Assert.Equal(new[] { 1, 2, 3, 4 }, results);
    }

    [Fact]
    public async Task CreateStream_ShouldResolveDifferentHandlers_ForDifferentRequestTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        services.AddTransient<IStreamRequestHandler<GetNumbersRequest, int>, GetNumbersHandler>();
        services.AddTransient<IStreamRequestHandler<GetNamesRequest, string>, GetNamesHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        var numberResults = new List<int>();
        await foreach (var item in mediator.CreateStream<int>(new GetNumbersRequest { Count = 2 }))
        {
            numberResults.Add(item);
        }

        var nameResults = new List<string>();
        await foreach (var item in mediator.CreateStream<string>(new GetNamesRequest { Prefix = "Test" }))
        {
            nameResults.Add(item);
        }

        // Assert
        Assert.Equal(new[] { 1, 2 }, numberResults);
        Assert.Equal(new[] { "Test_0", "Test_1", "Test_2" }, nameResults);
    }

    [Fact]
    public async Task CreateStream_ShouldCacheWrapperAcrossMultipleCalls()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSynaptrixCoreServices();
        services.AddTransient<IStreamRequestHandler<GetNumbersRequest, int>, GetNumbersHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act — call CreateStream multiple times for the same request type
        var results1 = new List<int>();
        await foreach (var item in mediator.CreateStream<int>(new GetNumbersRequest { Count = 2 }))
        {
            results1.Add(item);
        }

        var results2 = new List<int>();
        await foreach (var item in mediator.CreateStream<int>(new GetNumbersRequest { Count = 3 }))
        {
            results2.Add(item);
        }

        // Assert
        Assert.Equal(new[] { 1, 2 }, results1);
        Assert.Equal(new[] { 1, 2, 3 }, results2);
    }
}
