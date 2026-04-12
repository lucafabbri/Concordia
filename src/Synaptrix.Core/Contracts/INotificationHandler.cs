namespace Synaptrix;

/// <summary>
/// Handles a notification of type <typeparamref name="TNotification"/>.
/// </summary>
/// <typeparam name="TNotification">The type of the notification to handle.</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Handle(TNotification notification, CancellationToken cancellationToken);
}
