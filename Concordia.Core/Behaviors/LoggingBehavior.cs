using Concordia.Contracts;

namespace Concordia.Behaviors;

/// <summary>
/// A simple pipeline behavior for logging requests.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"--- Handling Request: {typeof(TRequest).Name} ---");
        var response = await next(); // Calls the next delegate in the pipeline (or the final handler)
        Console.WriteLine($"--- Handled Request: {typeof(TRequest).Name} - Response Type: {typeof(TResponse).Name} ---");
        return response;
    }
}
