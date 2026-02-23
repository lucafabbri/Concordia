// User management for REST API
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The create user command handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{CreateUserCommand, UserDto}"/>
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private static int _nextId = 100;

        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the created user dto</returns>
        public Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new UserDto
            {
                Id = System.Threading.Interlocked.Increment(ref _nextId),
                Username = request.Username,
                Email = request.Email,
                Roles = request.Roles
            };
            return Task.FromResult(user);
        }
    }
}
