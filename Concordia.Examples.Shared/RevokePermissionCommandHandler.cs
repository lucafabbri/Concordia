// Example command handler for user permission management
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Handles <see cref="RevokePermissionCommand"/> by removing the specified permission from the user.
    /// </summary>
    /// <seealso cref="IRequestHandler{RevokePermissionCommand}"/>
    public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Revoking permission '{request.Permission}' from user '{request.UserId}'");
            return Task.CompletedTask;
        }
    }
}
