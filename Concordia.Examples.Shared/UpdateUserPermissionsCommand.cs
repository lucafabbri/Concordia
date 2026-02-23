// User management for REST API
using System.Collections.Generic;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The update user permissions command class
    /// </summary>
    /// <seealso cref="IRequest{UserDto}"/>
    public class UpdateUserPermissionsCommand : IRequest<UserDto>
    {
        /// <summary>
        /// Gets or sets the value of the user id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Gets or sets the roles assigned to this user
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
    }
}
