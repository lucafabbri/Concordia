// Example Controller for usage (remains unchanged)
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The delete product command class
    /// </summary>
    /// <seealso cref="IRequest"/>
    public class DeleteProductCommand : IRequest
    {
        /// <summary>
        /// Gets or sets the value of the product id
        /// </summary>
        public int ProductId { get; set; }
    }
}
