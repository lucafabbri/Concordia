// Example Controller for usage (remains unchanged)
namespace Synaptrix.Examples.Web.Controllers
{
    /// <summary>
    /// The create product command class
    /// </summary>
    /// <seealso cref="IRequest"/>
    public class CreateProductCommand : IRequest
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