namespace Synaptrix;

/// <summary>
/// Defines a pre-processor for a request of type <typeparamref name="TRequest"/>.
/// Pre-processors are executed before the request is handled by the main handler or pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request to pre-process.</typeparam>
public interface IRequestPreProcessor<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Processes the request before it is handled.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Process(TRequest request, CancellationToken cancellationToken);
}