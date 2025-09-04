namespace Concordia;

/// <summary>
/// Defines a mediator for publishing notifications to multiple handlers.
/// </summary>
/// <remarks>The <see cref="IMediator"/> interface extends <see cref="ISender"/> and provides a mechanism for
/// sending notifications to all registered handlers. Implementations of this interface should ensure that notifications
/// are delivered to all appropriate handlers in a reliable manner.</remarks>
public interface IMediator : ISender
{
    /// <summary>
    /// Publishes the notification
    /// </summary>
    /// <param name="notification">The notification</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
}
