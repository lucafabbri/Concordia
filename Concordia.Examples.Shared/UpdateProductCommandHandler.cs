// Example Controller for usage (remains unchanged)
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The update product command handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{UpdateProductCommand, ProductDto}"/>
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the updated product dto</returns>
        public Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Updating product {request.ProductId}: name={request.ProductName}, price={request.Price}");
            var updated = new ProductDto { Id = request.ProductId, Name = request.ProductName, Price = request.Price };
            return Task.FromResult(updated);
        }
    }
}
