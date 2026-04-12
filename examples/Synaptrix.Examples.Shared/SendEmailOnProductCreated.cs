// Example Controller for usage (remains unchanged)
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synaptrix.Examples.Web.Controllers
{
    /// <summary>
    /// The send email on product created class
    /// </summary>
    /// <seealso cref="INotificationHandler{ProductCreatedNotification}"/>
    public class SendEmailOnProductCreated : INotificationHandler<ProductCreatedNotification>
    {
        /// <summary>
        /// Handles the notification
        /// </summary>
        /// <param name="notification">The notification</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public ValueTask Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Sending email for new product: {notification.ProductName} (Id: {notification.ProductId})");
            return default;
        }
    }
}