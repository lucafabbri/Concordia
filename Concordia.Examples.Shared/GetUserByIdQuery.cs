// User management for REST API
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The get user by id query class
    /// </summary>
    /// <seealso cref="IRequest{UserDto}"/>
    public class GetUserByIdQuery : IRequest<UserDto>
    {
        /// <summary>
        /// Gets or sets the value of the user id
        /// </summary>
        public int UserId { get; set; }
    }
}
