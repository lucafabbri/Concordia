using Synaptrix;

namespace Synaptrix;

/// <summary>
/// Implementation of <see cref="INotificationPublisher"/> that publishes
/// notifications to all handlers in parallel using Task.WhenAll.
/// </summary>
public class TaskWhenAllPublisher : INotificationPublisher
{
    /// <summary>
    /// Publishes a notification to a collection of handlers in parallel.
    /// </summary>
    /// <param name="handlerCalls">A collection of functions that, when invoked, will call the Handle method of a notification handler.</param>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous publishing operation.</returns>
    public ValueTask Publish(IEnumerable<Func<INotification, CancellationToken, ValueTask>> handlerCalls, INotification notification, CancellationToken cancellationToken)
    {
        var tasks = handlerCalls.Select(handlerCall => handlerCall(notification, cancellationToken).AsTask());
        return new ValueTask(Task.WhenAll(tasks));
    }
}