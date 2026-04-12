// Example Controller for usage (remains unchanged)
namespace Synaptrix.Examples.Web.Controllers
{
    /// <summary>
    /// The product created notification class
    /// </summary>
    /// <seealso cref="INotification"/>
    public class ProductCreatedNotification : INotification
    {
        /// <summary>
        /// Gets or sets the value of the product id
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Gets or sets the value of the product name
        /// </summary>
        public string ProductName { get; set; }
    }
}