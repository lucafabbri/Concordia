// Example DTO for user permission management
using System.Collections.Generic;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Data transfer object representing a user and their assigned permissions.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Gets or sets the unique user identifier.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of permissions assigned to the user.
        /// </summary>
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
