// User management for REST API
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The update user permissions command handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{UpdateUserPermissionsCommand, UserDto}"/>
    public class UpdateUserPermissionsCommandHandler : IRequestHandler<UpdateUserPermissionsCommand, UserDto>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the updated user dto</returns>
        public Task<UserDto> Handle(UpdateUserPermissionsCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Updating permissions for user {request.UserId}: roles={string.Join(", ", request.Roles)}");
            var user = new UserDto
            {
                Id = request.UserId,
                Username = $"user{request.UserId}",
                Email = $"user{request.UserId}@example.com",
                Roles = request.Roles
            };
            return Task.FromResult(user);
        }
    }
}
