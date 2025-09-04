using Concordia;

namespace Concordia.Behaviors;

/// <summary>
/// A simple pipeline behavior for logging requests.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="next">The next</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The response</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"--- Handling Request: {typeof(TRequest).Name} ---");
        var response = await next(cancellationToken); // Calls the next delegate in the pipeline (or the final handler)
        Console.WriteLine($"--- Handled Request: {typeof(TRequest).Name} - Response Type: {typeof(TResponse).Name} ---");
        return response;
    }
}
