using Microsoft.Extensions.Logging;

namespace Concordia;

/// <summary>
/// Implementation of <see cref="INotificationPublisher"/> that publishes
/// notifications to all handlers in parallel on a background thread (fire-and-forget).
/// This publisher returns control to the caller immediately.
/// Exceptions occurring during handling are logged.
/// </summary>
public class BackgroundParallelPublisher : INotificationPublisher
{
    private readonly ILogger<BackgroundParallelPublisher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundParallelPublisher"/> class.
    /// </summary>
    /// <param name="logger">The logger to record background exceptions.</param>
    public BackgroundParallelPublisher(ILogger<BackgroundParallelPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Publishes a notification to a collection of handlers in parallel on a background thread.
    /// This method returns Task.CompletedTask immediately, queueing the work.
    /// </summary>
    /// <param name="handlerCalls">A collection of functions that, when invoked, will call the Handle method of a notification handler.</param>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the *scheduling* of the operation (Task.CompletedTask).</returns>
    public Task Publish(IEnumerable<Func<INotification, CancellationToken, Task>> handlerCalls, INotification notification, CancellationToken cancellationToken)
    {
        // Do not await this task.
        // This queues the work to the ThreadPool and returns control immediately.
        _ = Task.Run(async () =>
        {
            try
            {
                // Use Task.WhenAll for parallel execution in the background
                var tasks = handlerCalls.Select(handlerCall => handlerCall(notification, cancellationToken));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the aggregate exception from Task.WhenAll or any single handler failure
                _logger.LogError(ex, "Error occurred while publishing notification {NotificationType} in the background.", notification.GetType().Name);
            }
        }, cancellationToken); // Pass CToken to Task.Run

        // Return immediately
        return Task.CompletedTask;
    }
}