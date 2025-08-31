namespace Concordia;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken t = default);

/// <summary>
/// Defines a behavior in the request processing pipeline.
/// Behaviors can execute logic before and after the request handler.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request and invokes the next step in the pipeline.
    /// </summary>
    /// <param name="request">The current request.</param>
    /// <param name="next">The delegate to invoke the next behavior or the final handler.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response of the request.</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
