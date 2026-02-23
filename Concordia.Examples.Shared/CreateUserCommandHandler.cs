// Example command handler for user permission management
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Handles <see cref="CreateUserCommand"/> by creating the user record.
    /// </summary>
    /// <seealso cref="IRequestHandler{CreateUserCommand, UserDto}"/>
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the created user</returns>
        public Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Creating user: Id='{request.UserId}', Name='{request.Name}'");
            var user = new UserDto
            {
                UserId = request.UserId,
                Name = request.Name,
                Permissions = new List<string>()
            };
            return Task.FromResult(user);
        }
    }
}
