namespace Synaptrix;

/// <summary>
/// Defines a sender interface for dispatching requests to their respective handlers.
/// </summary>
public interface ISender
{
    /// <summary>
    /// Sends a request and returns a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{TResponse}"/> containing the response.</returns>
    ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request without expecting a response.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Send(IRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request and returns a response.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{Object}"/> containing the response, or null for void requests.</returns>
    ValueTask<object?> Send(object request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an asynchronous stream from a stream request.
    /// </summary>
    /// <typeparam name="TResponse">The type of each element in the stream.</typeparam>
    /// <param name="request">The stream request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An <see cref="IAsyncEnumerable{TResponse}"/> representing the stream of responses.</returns>
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default);
}
