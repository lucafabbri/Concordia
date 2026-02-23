// Example of an update command for database management via REST API
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Command to update an existing product in the database.
    /// </summary>
    /// <seealso cref="IRequest"/>
    public class UpdateProductCommand : IRequest
    {
        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the new product name.
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the new product price.
        /// </summary>
        public decimal Price { get; set; }
    }
}
