// Example of an update command handler for database management via REST API
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Handles the <see cref="UpdateProductCommand"/> by updating the product record in the database.
    /// </summary>
    /// <seealso cref="IRequestHandler{UpdateProductCommand}"/>
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Updating product ID {request.ProductId}: Name='{request.ProductName}', Price={request.Price}");
            return Task.CompletedTask;
        }
    }
}
