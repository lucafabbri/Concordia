using Concordia;

namespace Concordia;

/// <summary>
/// Default implementation of <see cref="INotificationPublisher"/> that publishes
/// notifications to all handlers sequentially, awaiting the completion of each.
/// </summary>
public class ForeachAwaitPublisher : INotificationPublisher
{
    /// <summary>
    /// Publishes a notification to a collection of handlers sequentially.
    /// </summary>
    /// <param name="handlerCalls">A collection of functions that, when invoked, will call the Handle method of a notification handler.</param>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    public async Task Publish(IEnumerable<Func<INotification, CancellationToken, Task>> handlerCalls, INotification notification, CancellationToken cancellationToken)
    {
        foreach (var handlerCall in handlerCalls)
        {
            await handlerCall(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
