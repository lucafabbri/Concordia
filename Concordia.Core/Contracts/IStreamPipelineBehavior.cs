namespace Concordia;

// Note: The IRequestStream interface is not yet defined in Concordia,
// but this interface is included for compatibility with MediatR configuration.
// If you intend to implement streaming, you will need to define IRequestStream<TResponse>.
// For now, we use IRequest<IAsyncEnumerable<TResponse>> as a constraint for compatibility.

public delegate IAsyncEnumerable<TResponse> StreamHandlerDelegate<TResponse>();

/// <summary>
/// Defines a behavior in the processing pipeline of a stream request.
/// </summary>
/// <typeparam name="TRequest">The type of the stream request.</typeparam>
/// <typeparam name="TResponse">The type of the stream elements.</typeparam>
public interface IStreamPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<IAsyncEnumerable<TResponse>>
{
    /// <summary>
    /// Handles the stream request and invokes the next step in the pipeline.
    /// </summary>
    /// <param name="request">The current stream request.</param>
    /// <param name="next">The delegate to invoke the next behavior or the final handler.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An IAsyncEnumerable representing the stream of responses.</returns>
    IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
