// Example command for user permission management
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Command to revoke a permission from an existing user.
    /// </summary>
    /// <seealso cref="IRequest"/>
    public class RevokePermissionCommand : IRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the user whose permission will be revoked.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the permission to revoke (e.g. "products:write").
        /// </summary>
        public string Permission { get; set; } = string.Empty;
    }
}
