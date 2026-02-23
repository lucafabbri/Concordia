using Microsoft.AspNetCore.Mvc;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The users controller class
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
        /// Initializes a new instance of the <see cref="UsersController"/> class
        /// </summary>
        /// <param name="sender">The sender</param>
        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>A task containing the action result</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllUsersQuery();
            var users = await _sender.Send(query);
            return Ok(users);
        }

        /// <summary>
        /// Gets the user by id
        /// </summary>
        /// <param name="id">The user id</param>
        /// <returns>A task containing the action result</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetUserByIdQuery { UserId = id };
            var user = await _sender.Send(query);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="command">The create user command</param>
        /// <returns>A task containing the action result</returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var user = await _sender.Send(command);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        /// <summary>
        /// Updates the permissions for a user
        /// </summary>
        /// <param name="id">The user id</param>
        /// <param name="command">The update permissions command</param>
        /// <returns>A task containing the action result</returns>
        [HttpPut("{id}/permissions")]
        public async Task<IActionResult> UpdatePermissions(int id, [FromBody] UpdateUserPermissionsCommand command)
        {
            command.UserId = id;
            var user = await _sender.Send(command);
            return Ok(user);
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="id">The user id</param>
        /// <returns>A task containing the action result</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var command = new DeleteUserCommand { UserId = id };
            await _sender.Send(command);
            return NoContent();
        }
    }
}
