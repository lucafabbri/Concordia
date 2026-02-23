// User management for REST API
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The get all users query handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{GetAllUsersQuery, List{UserDto}}"/>
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the list of user dtos</returns>
        public Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "admin", Email = "admin@example.com", Roles = new List<string> { "Admin" } },
                new UserDto { Id = 2, Username = "editor", Email = "editor@example.com", Roles = new List<string> { "Editor" } },
                new UserDto { Id = 3, Username = "viewer", Email = "viewer@example.com", Roles = new List<string> { "Viewer" } },
            };
            return Task.FromResult(users);
        }
    }
}
