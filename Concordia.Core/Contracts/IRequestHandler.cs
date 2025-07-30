namespace Concordia.Contracts;

/// <summary>
/// Handles a request of type <typeparamref name="TRequest"/> and returns <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the request to handle.</typeparam>
/// <typeparam name="TResponse">The type of the response to return.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, with the result of the response.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
/// <summary>
/// Handles a request of type <typeparamref name="TRequest"/> without returning a response.
/// </summary>
/// <typeparam name="TRequest">The type of the request to handle.</typeparam>
public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Handle(TRequest request, CancellationToken cancellationToken);
}
