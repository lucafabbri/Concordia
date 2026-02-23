// Example of a delete command for database management via REST API
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Command to delete a product from the database.
    /// </summary>
    /// <seealso cref="IRequest"/>
    public class DeleteProductCommand : IRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the product to delete.
        /// </summary>
        public int ProductId { get; set; }
    }
}
