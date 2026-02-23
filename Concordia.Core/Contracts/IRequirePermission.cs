namespace Concordia;

/// <summary>
/// Marker interface for requests that require specific user permissions to be executed.
/// Implement this interface on a request to declare the permission(s) needed.
/// When used with <see cref="Behaviors.AuthorizationBehavior{TRequest,TResponse}"/>,
/// the pipeline will verify that the current user holds the required permission
/// before the request handler is invoked.
/// </summary>
public interface IRequirePermission
{
    /// <summary>
    /// Gets the permission required to execute this request.
    /// </summary>
    string RequiredPermission { get; }
}
