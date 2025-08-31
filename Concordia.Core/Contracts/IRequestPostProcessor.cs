namespace Concordia;

/// <summary>
/// Defines a post-processor for a request of type <typeparamref name="TRequest"/> and its response <typeparamref name="TResponse"/>.
/// Post-processors are executed after the request has been handled by the main handler or pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the processed request.</typeparam>
/// <typeparam name="TResponse">The type of the obtained response.</typeparam>
public interface IRequestPostProcessor<in TRequest, in TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Processes the request and its response after they have been handled.
    /// </summary>
    /// <param name="request">The processed request.</param>
    /// <param name="response">The response obtained from the request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Process(TRequest request, TResponse response, CancellationToken cancellationToken);
}
