namespace Synaptrix;

/// <summary>
/// The stream handler delegate.
/// </summary>
public delegate IAsyncEnumerable<TResponse> StreamHandlerDelegate<TResponse>();

/// <summary>
/// Defines a behavior in the processing pipeline of a stream request.
/// </summary>
/// <typeparam name="TRequest">The type of the stream request.</typeparam>
/// <typeparam name="TResponse">The type of the stream elements.</typeparam>
public interface IStreamPipelineBehavior<in TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Handles the stream request and invokes the next step in the pipeline.
    /// </summary>
    /// <param name="request">The current stream request.</param>
    /// <param name="next">The delegate to invoke the next behavior or the final handler.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="IAsyncEnumerable{TResponse}"/> representing the stream of responses.</returns>
    IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
