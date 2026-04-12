// Example Controller for usage (remains unchanged)
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synaptrix.Examples.Web.Controllers
{
    /// <summary>
    /// The create product command handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{CreateProductCommand}"/>
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public ValueTask Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Creating product: {request.ProductName} with ID: {request.ProductId}");
            return default;
        }
    }
}