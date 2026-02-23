// Example Controller for usage (remains unchanged)
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The get all products query handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{GetAllProductsQuery, List{ProductDto}}"/>
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the list of product dtos</returns>
        public Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Product 1", Price = 10.50m },
                new ProductDto { Id = 2, Name = "Product 2", Price = 20.99m },
                new ProductDto { Id = 3, Name = "Product 3", Price = 5.75m },
            };
            return Task.FromResult(products);
        }
    }
}
