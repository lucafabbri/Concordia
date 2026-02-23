// User management for REST API
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The get user by id query handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{GetUserByIdQuery, UserDto}"/>
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the user dto</returns>
        public Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = new UserDto
            {
                Id = request.UserId,
                Username = $"user{request.UserId}",
                Email = $"user{request.UserId}@example.com",
                Roles = new List<string> { "Viewer" }
            };
            return Task.FromResult(user);
        }
    }
}
