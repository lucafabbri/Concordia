// Example of a list query handler for database management via REST API
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Handles <see cref="ListProductsQuery"/> by returning all products from the database.
    /// </summary>
    /// <seealso cref="IRequestHandler{ListProductsQuery, List{ProductDto}}"/>
    public class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, List<ProductDto>>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the list of products</returns>
        public Task<List<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Listing all products");
            var products = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Product 1", Price = 10.50m },
                new ProductDto { Id = 2, Name = "Product 2", Price = 25.00m },
                new ProductDto { Id = 3, Name = "Product 3", Price = 5.99m },
            };
            return Task.FromResult(products);
        }
    }
}
