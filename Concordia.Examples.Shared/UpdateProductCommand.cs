// Example Controller for usage (remains unchanged)
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The update product command class
    /// </summary>
    /// <seealso cref="IRequest{ProductDto}"/>
    public class UpdateProductCommand : IRequest<ProductDto>
    {
        /// <summary>
        /// Gets or sets the value of the product id
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Gets or sets the value of the product name
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        public decimal Price { get; set; }
    }
}
