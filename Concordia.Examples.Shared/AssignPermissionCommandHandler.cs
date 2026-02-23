// Example command handler for user permission management
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Handles <see cref="AssignPermissionCommand"/> by assigning the specified permission to the user.
    /// </summary>
    /// <seealso cref="IRequestHandler{AssignPermissionCommand}"/>
    public class AssignPermissionCommandHandler : IRequestHandler<AssignPermissionCommand>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(AssignPermissionCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Assigning permission '{request.Permission}' to user '{request.UserId}'");
            return Task.CompletedTask;
        }
    }
}
