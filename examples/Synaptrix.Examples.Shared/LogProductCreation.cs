// Example Controller for usage (remains unchanged)
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synaptrix.Examples.Web.Controllers
{
    /// <summary>
    /// The log product creation class
    /// </summary>
    /// <seealso cref="INotificationHandler{ProductCreatedNotification}"/>
    public class LogProductCreation : INotificationHandler<ProductCreatedNotification>
    {
        /// <summary>
        /// Handles the notification
        /// </summary>
        /// <param name="notification">The notification</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public ValueTask Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Logging product creation: {notification.ProductName} (Id: {notification.ProductId}) created at {DateTime.Now}");
            return default;
        }
    }
}