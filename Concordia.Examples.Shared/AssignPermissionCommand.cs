// Example command for user permission management
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Command to assign a permission to an existing user.
    /// </summary>
    /// <seealso cref="IRequest"/>
    public class AssignPermissionCommand : IRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the user receiving the permission.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the permission to assign (e.g. "products:write").
        /// </summary>
        public string Permission { get; set; } = string.Empty;
    }
}
