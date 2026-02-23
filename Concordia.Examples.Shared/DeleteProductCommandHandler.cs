// Example of a delete command handler for database management via REST API
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Handles the <see cref="DeleteProductCommand"/> by removing the product record from the database.
    /// </summary>
    /// <seealso cref="IRequestHandler{DeleteProductCommand}"/>
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Deleting product ID {request.ProductId}");
            return Task.CompletedTask;
        }
    }
}
