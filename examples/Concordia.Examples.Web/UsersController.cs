using Microsoft.AspNetCore.Mvc;

// REST API controller demonstrating user permission management via Concordia mediator
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// REST API controller for user permission management.
    /// Demonstrates how to use the Concordia mediator pattern to manage users and their permissions.
    /// </summary>
    /// <seealso cref="ControllerBase"/>
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// The sender
        /// </summary>
        private readonly ISender _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="sender">The sender</param>
        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Returns the user with the specified identifier along with their permissions.
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <returns>A task containing the action result with the user and permissions</returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(string userId)
        {
            var user = await _sender.Send(new GetUserByIdQuery { UserId = userId });
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="command">The create user command</param>
        /// <returns>A task containing the action result with the created user</returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var user = await _sender.Send(command);
            return CreatedAtAction(nameof(Get), new { userId = user.UserId }, user);
        }

        /// <summary>
        /// Assigns a permission to the specified user.
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="command">The assign permission command</param>
        /// <returns>A task containing the action result</returns>
        [HttpPost("{userId}/permissions")]
        public async Task<IActionResult> AssignPermission(string userId, [FromBody] AssignPermissionCommand command)
        {
            command.UserId = userId;
            await _sender.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Revokes a permission from the specified user.
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="command">The revoke permission command</param>
        /// <returns>A task containing the action result</returns>
        [HttpDelete("{userId}/permissions")]
        public async Task<IActionResult> RevokePermission(string userId, [FromBody] RevokePermissionCommand command)
        {
            command.UserId = userId;
            await _sender.Send(command);
            return NoContent();
        }
    }
}
