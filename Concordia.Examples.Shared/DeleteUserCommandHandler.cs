// User management for REST API
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The delete user command handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{DeleteUserCommand}"/>
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Deleting user with ID: {request.UserId}");
            return Task.CompletedTask;
        }
    }
}
