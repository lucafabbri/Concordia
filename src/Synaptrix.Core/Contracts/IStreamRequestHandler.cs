namespace Synaptrix;

/// <summary>
/// Handles a streaming request of type <typeparamref name="TRequest"/>
/// and returns an <see cref="IAsyncEnumerable{TResponse}"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the stream request.</typeparam>
/// <typeparam name="TResponse">The type of each element in the stream.</typeparam>
public interface IStreamRequestHandler<in TRequest, out TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Handles the stream request and returns an asynchronous enumerable of responses.
    /// </summary>
    /// <param name="request">The stream request to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="IAsyncEnumerable{TResponse}"/> representing the stream of responses.</returns>
    IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
