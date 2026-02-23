// Example Controller for usage (remains unchanged)
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The delete product command handler class
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
            Console.WriteLine($"Deleting product with ID: {request.ProductId}");
            return Task.CompletedTask;
        }
    }
}
