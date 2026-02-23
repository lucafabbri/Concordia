// Example query for user permission management
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Query to retrieve a user and their permissions by user identifier.
    /// </summary>
    /// <seealso cref="IRequest{UserDto}"/>
    public class GetUserByIdQuery : IRequest<UserDto>
    {
        /// <summary>
        /// Gets or sets the user identifier to look up.
        /// </summary>
        public string UserId { get; set; } = string.Empty;
    }
}
