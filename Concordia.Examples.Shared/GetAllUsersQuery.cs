// User management for REST API
using System.Collections.Generic;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The get all users query class
    /// </summary>
    /// <seealso cref="IRequest{List{UserDto}}"/>
    public class GetAllUsersQuery : IRequest<List<UserDto>>
    {
    }
}
