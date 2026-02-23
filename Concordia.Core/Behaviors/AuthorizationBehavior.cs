using Microsoft.Extensions.DependencyInjection;

namespace Concordia.Behaviors;

/// <summary>
/// A pipeline behavior that enforces permission checks for requests that implement
/// <see cref="IRequirePermission"/>. When the request declares a required permission,
/// this behavior resolves the registered <see cref="ICurrentUserService"/> and verifies
/// that the current user holds that permission before forwarding the request to the handler.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <remarks>
/// Register this behavior as an open-generic pipeline behavior in the DI container:
/// <code>
/// services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(AuthorizationBehavior&lt;,&gt;));
/// </code>
/// You must also register an implementation of <see cref="ICurrentUserService"/>.
/// </remarks>
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService? _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationBehavior{TRequest,TResponse}"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve <see cref="ICurrentUserService"/>.</param>
    public AuthorizationBehavior(IServiceProvider serviceProvider)
    {
        _currentUserService = serviceProvider.GetService<ICurrentUserService>();
    }

    /// <summary>
    /// Checks whether the current user has the required permission (if declared by the request),
    /// then forwards the call to the next step in the pipeline.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The response produced by the handler.</returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when <see cref="ICurrentUserService"/> is not registered or the current user
    /// is not authenticated or lacks the required permission.
    /// </exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IRequirePermission permissionRequest)
        {
            if (_currentUserService == null)
            {
                throw new UnauthorizedAccessException(
                    $"Request '{typeof(TRequest).Name}' requires permission '{permissionRequest.RequiredPermission}', " +
                    "but no ICurrentUserService is registered.");
            }

            if (string.IsNullOrEmpty(_currentUserService.UserId))
            {
                throw new UnauthorizedAccessException(
                    $"Request '{typeof(TRequest).Name}' requires authentication. No user is currently signed in.");
            }

            if (!_currentUserService.HasPermission(permissionRequest.RequiredPermission))
            {
                throw new UnauthorizedAccessException(
                    $"User '{_currentUserService.UserId}' does not have the required permission " +
                    $"'{permissionRequest.RequiredPermission}' to execute '{typeof(TRequest).Name}'.");
            }
        }

        return await next(cancellationToken);
    }
}
