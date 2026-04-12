using Synaptrix;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Synaptrix;

/// <summary>
/// Implements the mediator pattern, providing a central point for dispatching requests and publishing notifications.
/// </summary>
public class Mediator : IMediator, ISender
{
    // Static per-type wrapper caches â€“ created once per (TRequest, TResponse) combination.
    private static readonly ConcurrentDictionary<Type, object> _responseWrappers = new();
    private static readonly ConcurrentDictionary<Type, VoidHandlerWrapper> _voidWrappers = new();
    private static readonly ConcurrentDictionary<Type, NotificationWrapper> _notificationWrappers = new();
    private static readonly ConcurrentDictionary<Type, StreamHandlerWrapperBase> _streamWrappers = new();

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="serviceProvider"/> is null.</exception>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Sends a request to a single handler and returns a response.
    /// </summary>
    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        if (!_responseWrappers.TryGetValue(requestType, out var cached))
        {
            cached = Activator.CreateInstance(
                typeof(ResponseHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse)))!;
            _responseWrappers.TryAdd(requestType, cached);
        }
        return ((ResponseHandlerWrapper<TResponse>)cached).Handle(request, _serviceProvider, cancellationToken);
    }

    /// <summary>
    /// Sends a request (no response) to a single handler.
    /// </summary>
    public ValueTask Send(IRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        if (!_voidWrappers.TryGetValue(requestType, out var cached))
        {
            cached = (VoidHandlerWrapper)Activator.CreateInstance(
                typeof(VoidHandlerWrapperImpl<>).MakeGenericType(requestType))!;
            _voidWrappers.TryAdd(requestType, cached);
        }
        return cached.Handle(request, _serviceProvider, cancellationToken);
    }

    /// <summary>
    /// Sends a request to the appropriate handler and returns the response, if any.
    /// </summary>
    public ValueTask<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        if (!_responseWrappers.TryGetValue(requestType, out var cached))
        {
            // Determine TResponse from the request's IRequest<TResponse> interface
            var responseInterface = requestType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

            if (responseInterface != null)
            {
                var responseType = responseInterface.GetGenericArguments()[0];
                // Eagerly check registration so we can emit a specific error message for the object-Send path.
                var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
                if (_serviceProvider.GetService(handlerType) == null)
                    throw new InvalidOperationException($"No handler found for request of type {requestType.Name}");

                cached = Activator.CreateInstance(
                    typeof(ResponseHandlerWrapperImpl<,>).MakeGenericType(requestType, responseType))!;
                _responseWrappers.TryAdd(requestType, cached);
                return ((ObjectResponseHandlerWrapper)cached).HandleObject(request, _serviceProvider, cancellationToken);
            }

            // Void request
            if (!_voidWrappers.TryGetValue(requestType, out var voidCached))
            {
                if (!requestType.GetInterfaces().Any(i => i == typeof(IRequest)))
                    throw new InvalidOperationException($"Request object of type {requestType.Name} does not implement IRequest or IRequest<TResponse>.");
                voidCached = (VoidHandlerWrapper)Activator.CreateInstance(
                    typeof(VoidHandlerWrapperImpl<>).MakeGenericType(requestType))!;
                _voidWrappers.TryAdd(requestType, voidCached);
            }
            return WrapVoidResult(voidCached.Handle((IRequest)request, _serviceProvider, cancellationToken));
        }

        return ((ObjectResponseHandlerWrapper)cached).HandleObject(request, _serviceProvider, cancellationToken);
    }

    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    public ValueTask Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));

        var notificationType = notification.GetType();
        if (!_notificationWrappers.TryGetValue(notificationType, out var cached))
        {
            cached = (NotificationWrapper)Activator.CreateInstance(
                typeof(NotificationWrapperImpl<>).MakeGenericType(notificationType))!;
            _notificationWrappers.TryAdd(notificationType, cached);
        }
        return cached.Publish(notification, _serviceProvider, cancellationToken);
    }

    /// <summary>
    /// Creates an asynchronous stream from a stream request.
    /// </summary>
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        if (!_streamWrappers.TryGetValue(requestType, out var cached))
        {
            cached = (StreamHandlerWrapperBase)Activator.CreateInstance(
                typeof(StreamHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse)))!;
            _streamWrappers.TryAdd(requestType, cached);
        }
        return ((StreamHandlerWrapper<TResponse>)cached).Handle(request, _serviceProvider, cancellationToken);
    }

    private static async ValueTask<object?> WrapVoidResult(ValueTask task)
    {
        await task.ConfigureAwait(false);
        return null;
    }
}

//Wrapper hierarchy for fast dynamic dispatch without reflection after initial cache population

internal abstract class ResponseHandlerWrapper<TResponse> : ObjectResponseHandlerWrapper
{
    public abstract ValueTask<TResponse> Handle(IRequest<TResponse> request, IServiceProvider sp, CancellationToken ct);
    public override async ValueTask<object?> HandleObject(object request, IServiceProvider sp, CancellationToken ct)
        => await Handle((IRequest<TResponse>)request, sp, ct).ConfigureAwait(false);
}

internal abstract class ObjectResponseHandlerWrapper
{
    public abstract ValueTask<object?> HandleObject(object request, IServiceProvider sp, CancellationToken ct);
}

internal sealed class ResponseHandlerWrapperImpl<TRequest, TResponse> : ResponseHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override ValueTask<TResponse> Handle(IRequest<TResponse> request, IServiceProvider sp, CancellationToken ct)
    {
        var typedRequest = (TRequest)request;

        // Resolve pipeline behaviors (typed â€“ no reflection)
        var behaviors = sp.GetServices<IPipelineBehavior<TRequest, TResponse>>();

        // Fast path: no behaviors, no pre/post processors
        if (behaviors is IPipelineBehavior<TRequest, TResponse>[] bArr && bArr.Length == 0)
        {
            var pre = sp.GetServices<IRequestPreProcessor<TRequest>>();
            if (pre is IRequestPreProcessor<TRequest>[] preArr && preArr.Length == 0)
            {
                var post = sp.GetServices<IRequestPostProcessor<TRequest, TResponse>>();
                if (post is IRequestPostProcessor<TRequest, TResponse>[] postArr && postArr.Length == 0)
                {
                    return (sp.GetService<IRequestHandler<TRequest, TResponse>>()
                        ?? throw new InvalidOperationException($"No handler found for request of type {typeof(TRequest).Name}"))
                        .Handle(typedRequest, ct);
                }
            }
        }

        return HandleWithPipeline(typedRequest, behaviors, sp, ct);
    }

    private static async ValueTask<TResponse> HandleWithPipeline(
        TRequest request,
        IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors,
        IServiceProvider sp,
        CancellationToken ct)
    {
        foreach (var pre in sp.GetServices<IRequestPreProcessor<TRequest>>())
            await pre.Process(request, ct).ConfigureAwait(false);

        var handler = sp.GetService<IRequestHandler<TRequest, TResponse>>()
            ?? throw new InvalidOperationException($"No handler found for request of type {typeof(TRequest).Name}");

        ValueTask<TResponse> HandlerDelegate(CancellationToken t) => handler.Handle(request, t);

        RequestHandlerDelegate<TResponse> pipeline = HandlerDelegate;
        var behaviorList = behaviors is IPipelineBehavior<TRequest, TResponse>[] arr
            ? (IReadOnlyList<IPipelineBehavior<TRequest, TResponse>>)arr
            : behaviors.ToList();

        for (int i = behaviorList.Count - 1; i >= 0; i--)
        {
            var b = behaviorList[i];
            var prev = pipeline;
            pipeline = t => b.Handle(request, prev, t);
        }

        var response = await pipeline(ct).ConfigureAwait(false);

        foreach (var post in sp.GetServices<IRequestPostProcessor<TRequest, TResponse>>())
            await post.Process(request, response, ct).ConfigureAwait(false);

        return response;
    }
}

internal abstract class VoidHandlerWrapper
{
    public abstract ValueTask Handle(IRequest request, IServiceProvider sp, CancellationToken ct);
}

internal sealed class VoidHandlerWrapperImpl<TRequest> : VoidHandlerWrapper
    where TRequest : IRequest
{
    public override ValueTask Handle(IRequest request, IServiceProvider sp, CancellationToken ct)
    {
        var typedRequest = (TRequest)request;

        // Fast path: no pre-processors
        var pre = sp.GetServices<IRequestPreProcessor<TRequest>>();
        if (pre is IRequestPreProcessor<TRequest>[] preArr && preArr.Length == 0)
            return (sp.GetService<IRequestHandler<TRequest>>()
                ?? throw new InvalidOperationException($"No handler found for request of type {typeof(TRequest).Name}"))
                .Handle(typedRequest, ct);

        return HandleSlow(typedRequest, pre, sp, ct);
    }

    private static async ValueTask HandleSlow(
        TRequest request,
        IEnumerable<IRequestPreProcessor<TRequest>> preProcessors,
        IServiceProvider sp,
        CancellationToken ct)
    {
        foreach (var pre in preProcessors)
            await pre.Process(request, ct).ConfigureAwait(false);

        var voidHandler = sp.GetService<IRequestHandler<TRequest>>()
            ?? throw new InvalidOperationException($"No handler found for request of type {typeof(TRequest).Name}");
        await voidHandler.Handle(request, ct).ConfigureAwait(false);
    }
}

internal abstract class NotificationWrapper
{
    public abstract ValueTask Publish(INotification notification, IServiceProvider sp, CancellationToken ct);
}

internal sealed class NotificationWrapperImpl<TNotification> : NotificationWrapper
    where TNotification : INotification
{
    public override ValueTask Publish(INotification notification, IServiceProvider sp, CancellationToken ct)
    {
        var publisher = sp.GetRequiredService<INotificationPublisher>();
        var typedNotification = (TNotification)notification;
        var handlers = sp.GetServices<INotificationHandler<TNotification>>();

        // Convert to Func<> without reflection (typed call)
        return publisher.Publish(
            handlers.Select(static h => (Func<INotification, CancellationToken, ValueTask>)
                ((n, t) => ((INotificationHandler<TNotification>)h).Handle((TNotification)n, t))),
            notification,
            ct);
    }
}

// —— Stream wrapper hierarchy —————————————————————————————————————————————————————

internal abstract class StreamHandlerWrapperBase { }

internal abstract class StreamHandlerWrapper<TResponse> : StreamHandlerWrapperBase
{
    public abstract IAsyncEnumerable<TResponse> Handle(IStreamRequest<TResponse> request, IServiceProvider sp, CancellationToken ct);
}

internal sealed class StreamHandlerWrapperImpl<TRequest, TResponse> : StreamHandlerWrapper<TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    public override IAsyncEnumerable<TResponse> Handle(IStreamRequest<TResponse> request, IServiceProvider sp, CancellationToken ct)
    {
        var typedRequest = (TRequest)request;

        var behaviors = sp.GetServices<IStreamPipelineBehavior<TRequest, TResponse>>();

        // Fast path: no behaviors
        if (behaviors is IStreamPipelineBehavior<TRequest, TResponse>[] bArr && bArr.Length == 0)
        {
            return (sp.GetService<IStreamRequestHandler<TRequest, TResponse>>()
                ?? throw new InvalidOperationException($"No stream handler found for request of type {typeof(TRequest).Name}"))
                .Handle(typedRequest, ct);
        }

        return HandleWithPipeline(typedRequest, behaviors, sp, ct);
    }

    private static IAsyncEnumerable<TResponse> HandleWithPipeline(
        TRequest request,
        IEnumerable<IStreamPipelineBehavior<TRequest, TResponse>> behaviors,
        IServiceProvider sp,
        CancellationToken ct)
    {
        var handler = sp.GetService<IStreamRequestHandler<TRequest, TResponse>>()
            ?? throw new InvalidOperationException($"No stream handler found for request of type {typeof(TRequest).Name}");

        StreamHandlerDelegate<TResponse> pipeline = () => handler.Handle(request, ct);
        var behaviorList = behaviors is IStreamPipelineBehavior<TRequest, TResponse>[] arr
            ? (IReadOnlyList<IStreamPipelineBehavior<TRequest, TResponse>>)arr
            : behaviors.ToList();

        for (int i = behaviorList.Count - 1; i >= 0; i--)
        {
            var b = behaviorList[i];
            var prev = pipeline;
            pipeline = () => b.Handle(request, prev, ct);
        }

        return pipeline();
    }
}

