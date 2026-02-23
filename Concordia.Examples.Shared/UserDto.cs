// User management for REST API
using System.Collections.Generic;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The user dto class
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Gets or sets the value of the id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the value of the username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the value of the email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the roles assigned to this user
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
    }
}
