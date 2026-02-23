// User management for REST API
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The delete user command class
    /// </summary>
    /// <seealso cref="IRequest"/>
    public class DeleteUserCommand : IRequest
    {
        /// <summary>
        /// Gets or sets the value of the user id
        /// </summary>
        public int UserId { get; set; }
    }
}
