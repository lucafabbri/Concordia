namespace Concordia;

/// <summary>
/// Provides information about the current user executing a request.
/// Implement and register this interface to integrate with your authentication system.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the identifier of the current user.
    /// Returns <see langword="null"/> if no user is authenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Determines whether the current user has the specified permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <returns><see langword="true"/> if the current user has the permission; otherwise, <see langword="false"/>.</returns>
    bool HasPermission(string permission);
}
