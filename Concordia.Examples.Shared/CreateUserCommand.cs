// Example command for user permission management
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Command to create a new user in the system.
    /// </summary>
    /// <seealso cref="IRequest{UserDto}"/>
    public class CreateUserCommand : IRequest<UserDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier for the new user.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name for the new user.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
