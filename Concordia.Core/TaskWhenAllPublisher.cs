using Concordia.Contracts;

namespace Concordia;

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
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    public Task Publish(IEnumerable<Func<INotification, CancellationToken, Task>> handlerCalls, INotification notification, CancellationToken cancellationToken)
    {
        var tasks = handlerCalls.Select(handlerCall => handlerCall(notification, cancellationToken));
        return Task.WhenAll(tasks);
    }
}